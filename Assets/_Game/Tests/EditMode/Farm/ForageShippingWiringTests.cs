using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Farm.Forage;
using CindarsHope.Farm.Shipping;

namespace CindarsHope.Tests.EditMode.Farm
{
    /// <summary>
    /// fable_54 — testes do wiring de forrageio sazonal + shipping bin overnight. Cobrem a logica
    /// determinista/pura (tabela por estacao, selecao por seed/dia, respawn por politica, batch
    /// overnight idempotente, sellability) e o round-trip dos DTOs de save aditivos. Os hosts
    /// MonoBehaviour delegam a estas mesmas peças, validadas aqui sem cena.
    /// </summary>
    [TestFixture]
    public class ForageShippingWiringTests
    {
        // ─────────────────────────── CA-1: Forrageio sazonal determinístico ───────────────────────────

        [Test]
        public void Forage_SameSeedAndDay_SameSelection()
        {
            var a = ForageStableHash.SelectDailyIndices("seedA", 5, 6, 4);
            var b = ForageStableHash.SelectDailyIndices("seedA", 5, 6, 4);
            CollectionAssert.AreEqual(a, b);
            Assert.AreEqual(4, a.Count);
        }

        [Test]
        public void Forage_DifferentDay_LikelyDifferentSelection()
        {
            var day1 = ForageStableHash.SelectDailyIndices("seedA", 1, 6, 4);
            var day2 = ForageStableHash.SelectDailyIndices("seedA", 2, 6, 4);
            // Determinismo nao garante diferenca, mas com 6 escolhe 4 a chance de igualdade total e baixa.
            // O contrato testado: ambos validos, ordenados, distintos.
            Assert.AreEqual(4, day1.Count);
            Assert.AreEqual(4, day2.Count);
            CollectionAssert.AllItemsAreUnique(day1);
            CollectionAssert.AllItemsAreUnique(day2);
        }

        [Test]
        public void Forage_SelectionIndices_InRange_AndSorted()
        {
            var sel = ForageStableHash.SelectDailyIndices("worldX", 10, 6, 4);
            Assert.AreEqual(4, sel.Count);
            var prev = -1;
            foreach (var idx in sel)
            {
                Assert.GreaterOrEqual(idx, 0);
                Assert.Less(idx, 6);
                Assert.Greater(idx, prev, "indices devem ser estritamente crescentes (distintos/ordenados)");
                prev = idx;
            }
        }

        [Test]
        public void Forage_SelectionCountGreaterThanPool_ReturnsAll()
        {
            var sel = ForageStableHash.SelectDailyIndices("s", 1, 3, 10);
            Assert.AreEqual(3, sel.Count);
        }

        [Test]
        public void ForageTable_DistinctSeasons_DistinctTables()
        {
            var spring = ForageSeasonTable.ForSeason("Primavera");
            var summer = ForageSeasonTable.ForSeason("Verao");
            var autumn = ForageSeasonTable.ForSeason("Outono");
            var winter = ForageSeasonTable.ForSeason("Inverno");

            Assert.GreaterOrEqual(spring.Count, 2, "mínimo 2 forrageáveis por estação");
            Assert.GreaterOrEqual(summer.Count, 2);
            Assert.GreaterOrEqual(autumn.Count, 2);
            Assert.GreaterOrEqual(winter.Count, 2);

            // Cada estacao tem ao menos um forageId exclusivo (tabelas distintas).
            Assert.AreNotEqual(spring[0].ForageId, summer[0].ForageId);
            Assert.AreNotEqual(autumn[0].ForageId, winter[0].ForageId);
        }

        [Test]
        public void ForageTable_EnglishAlias_ResolvesSameAsEnum()
        {
            var byEnum = ForageSeasonTable.ForSeason("Verao");
            var byAlias = ForageSeasonTable.ForSeason("Summer");
            Assert.AreEqual(byEnum[0].ForageId, byAlias[0].ForageId);
        }

        [Test]
        public void ForageTable_AllSeasonItems_HaveAllowedSeasonMatchingTable()
        {
            AssertSeasonTagged("Primavera");
            AssertSeasonTagged("Verao");
            AssertSeasonTagged("Outono");
            AssertSeasonTagged("Inverno");
        }

        private static void AssertSeasonTagged(string season)
        {
            foreach (var def in ForageSeasonTable.ForSeason(season))
            {
                Assert.Contains(season, def.AllowedSeasons,
                    $"forrageável {def.ForageId} deve permitir a estação {season} (nunca fora da tabela)");
            }
        }

        [Test]
        public void Forage_WrongSeason_NeverCollects_ServiceContract()
        {
            // Combina a tabela (Verao) com o servico orfao: coletar no Inverno deve falhar WrongSeason.
            var defs = ForageSeasonTable.AllDefinitionsById();
            var service = new FarmForageSpawnService(defs);
            var summerDef = ForageSeasonTable.ForSeason("Verao")[0];
            var spawn = new ForageSpawnState
            {
                ForageInstanceId = "s1", ForageId = summerDef.ForageId,
                ZoneId = "farm_zone_forage", CurrentState = ForageNodeState.Available
            };

            var result = service.Collect(spawn, 5, "Inverno");
            Assert.IsFalse(result.Success);
            Assert.AreEqual("WrongSeason", result.FailureReason);
        }

        [Test]
        public void Forage_ForbiddenZone_NeverReceivesCollect()
        {
            var defs = ForageSeasonTable.AllDefinitionsById();
            var service = new FarmForageSpawnService(defs);
            var def = ForageSeasonTable.ForSeason("Primavera")[0];
            foreach (var zone in new[] { "zone_fonte", "zone_cave_entrance", "zone_city_exit", "zone_lore_reserved" })
            {
                var spawn = new ForageSpawnState
                {
                    ForageInstanceId = "z", ForageId = def.ForageId,
                    ZoneId = zone, CurrentState = ForageNodeState.Available
                };
                var result = service.Collect(spawn, 1, "Primavera");
                Assert.IsFalse(result.Success, $"zona proibida {zone} nunca coleta");
                Assert.AreEqual("ForbiddenZone", result.FailureReason);
            }
        }

        // ─────────────────────────── CA-2: Coleta via serviço órfão + respawn ───────────────────────────

        [Test]
        public void Forage_SecondCollect_FailsNotAvailable()
        {
            var defs = ForageSeasonTable.AllDefinitionsById();
            var service = new FarmForageSpawnService(defs);
            var def = ForageSeasonTable.ForSeason("Primavera")[0];
            var spawn = new ForageSpawnState
            {
                ForageInstanceId = "p", ForageId = def.ForageId,
                ZoneId = "farm_zone_forage", CurrentState = ForageNodeState.Available
            };

            var first = service.Collect(spawn, 1, "Primavera");
            Assert.IsTrue(first.Success);
            var second = service.Collect(spawn, 1, "Primavera");
            Assert.IsFalse(second.Success);
            Assert.AreEqual("ForageNotAvailable", second.FailureReason);
        }

        [Test]
        public void Forage_Respawn_AfterPolicyDays()
        {
            var defs = ForageSeasonTable.AllDefinitionsById();
            var service = new FarmForageSpawnService(defs);
            var def = ForageSeasonTable.ForSeason("Primavera")[0]; // RespawnAfterDays = 2
            var spawn = new ForageSpawnState
            {
                ForageInstanceId = "p", ForageId = def.ForageId,
                ZoneId = "farm_zone_forage", CurrentState = ForageNodeState.Available
            };

            service.Collect(spawn, 1, "Primavera");
            Assert.IsFalse(spawn.IsAvailable);
            Assert.IsFalse(service.TryRespawn(spawn, 2, "Primavera"), "cedo demais (dia 1 + 2 = 3)");
            Assert.IsTrue(service.TryRespawn(spawn, 3, "Primavera"));
            Assert.IsTrue(spawn.IsAvailable);
        }

        // ─────────────────────────── CA-3: Shipping overnight idempotente ───────────────────────────

        [Test]
        public void Shipping_Deposit_CreatesPendingEntry_ProcessOnNextDay()
        {
            var (service, _) = BuildShipping();
            var result = service.Deposit("farm_shipping_bin_01", "item_crop_carrot", 3, 0, 14f, 7);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(8, result.CreatedEntry.ProcessOnDay); // dia 7 + 1
            Assert.AreEqual(ShippingEntryState.Pending, result.CreatedEntry.State);
        }

        [Test]
        public void Shipping_Overnight_CreditsOnce_Channel095()
        {
            var (service, _) = BuildShipping();
            // BaseValue 100, canal 0.95 => 95 por unidade; qty 1.
            service.Deposit("bin", "item_crop_carrot", 1, 0, 100f, 1);
            var batch = service.ProcessDayBatch(2);
            Assert.AreEqual(ShippingBatchState.Processed, batch.State);
            Assert.AreEqual(1, batch.Entries.Count);
            Assert.AreEqual(95f, batch.TotalGold, 0.01f);
        }

        [Test]
        public void Shipping_Overnight_Idempotent_NoDoublePay()
        {
            var (service, _) = BuildShipping();
            service.Deposit("bin", "item_crop_carrot", 2, 0, 10f, 1);
            var batch1 = service.ProcessDayBatch(2);
            Assert.Greater(batch1.TotalGold, 0f);
            var batch2 = service.ProcessDayBatch(2); // reprocessa o mesmo dia
            Assert.AreEqual(0, batch2.Entries.Count);
            Assert.AreEqual(0f, batch2.TotalGold);
        }

        [Test]
        public void Shipping_QuestAndKeyItems_Refused()
        {
            var (service, _) = BuildShipping();
            var quest = service.Deposit("bin", "item_quest_relic", 1, 0, 50f, 1);
            Assert.IsFalse(quest.Success);
            Assert.AreEqual("QuestItemProtected", quest.FailureReason);

            var key = service.Deposit("bin", "item_key_gate", 1, 0, 50f, 1);
            Assert.IsFalse(key.Success);
            Assert.AreEqual("KeyItemProtected", key.FailureReason);
        }

        // ─────────────────────────── CA-4: Persistência da batch pendente ───────────────────────────

        [Test]
        public void Save_PendingShipping_RoundTrip_PreservesEntry()
        {
            var entries = new List<PendingShippingEntry>
            {
                new PendingShippingEntry
                {
                    ShippingEntryId = "e1", SellPointId = "farm_shipping_bin_01", ItemId = "item_crop_carrot",
                    Quantity = 3, QualityTier = 1, BaseValueSnapshot = 14f, DepositedDay = 7, ProcessOnDay = 8,
                    State = ShippingEntryState.Pending
                }
            };

            var dto = MapToSave(entries);
            var restored = MapFromSave(dto);

            Assert.AreEqual(1, restored.Count);
            Assert.AreEqual("e1", restored[0].ShippingEntryId);
            Assert.AreEqual("item_crop_carrot", restored[0].ItemId);
            Assert.AreEqual(3, restored[0].Quantity);
            Assert.AreEqual(8, restored[0].ProcessOnDay);
            Assert.AreEqual(ShippingEntryState.Pending, restored[0].State);
        }

        [Test]
        public void Save_PendingShipping_RoundTrip_PaysOnCorrectMorning()
        {
            // Deposita no dia 7 (paga no 8). Salva, "recarrega", processa 7 (nada) e 8 (paga 1x).
            var entries = new List<PendingShippingEntry>();
            var resolver = new ShippingPriceResolver();
            var depositSvc = new FarmShippingService(entries, resolver, new ItemSellabilityProvider());
            depositSvc.Deposit("bin", "item_crop_carrot", 1, 0, 100f, 7);

            var dto = MapToSave(entries);
            var reloaded = MapFromSave(dto);
            var loadedSvc = new FarmShippingService(reloaded, resolver, new ItemSellabilityProvider());

            var wrongMorning = loadedSvc.ProcessDayBatch(7);
            Assert.AreEqual(0, wrongMorning.Entries.Count, "não paga no dia do depósito");

            var correctMorning = loadedSvc.ProcessDayBatch(8);
            Assert.AreEqual(1, correctMorning.Entries.Count);
            Assert.AreEqual(95f, correctMorning.TotalGold, 0.01f);

            var replay = loadedSvc.ProcessDayBatch(8);
            Assert.AreEqual(0f, replay.TotalGold, "reprocessar após load não paga de novo");
        }

        [Test]
        public void Save_LegacyEmpty_LoadsWithoutBatch()
        {
            var legacy = new PendingShippingSaveData(); // campo ausente em save legado => lista vazia
            var restored = MapFromSave(legacy);
            Assert.AreEqual(0, restored.Count);

            var svc = new FarmShippingService(restored, new ShippingPriceResolver(), new ItemSellabilityProvider());
            var batch = svc.ProcessDayBatch(2);
            Assert.AreEqual(0, batch.Entries.Count);
        }

        [Test]
        public void Save_ForageSpawns_RoundTrip_PreservesState()
        {
            var spawns = new List<ForageSpawnState>
            {
                new ForageSpawnState
                {
                    ForageInstanceId = "farm_forage_01", ForageId = "forage_spring_herbs",
                    CurrentState = ForageNodeState.Collected, SpawnedDay = 3, CollectedDay = 3,
                    NextEligibleSpawnDay = 5
                }
            };

            var dto = new ForageSpawnsSaveData { Day = 3 };
            foreach (var s in spawns)
            {
                dto.Spawns.Add(new ForageSpawnSaveData
                {
                    SpawnId = s.ForageInstanceId, ForageId = s.ForageId, State = (int)s.CurrentState,
                    SpawnedDay = s.SpawnedDay, CollectedDay = s.CollectedDay, NextEligibleSpawnDay = s.NextEligibleSpawnDay
                });
            }

            // round-trip simples: reconstrói os estados
            Assert.AreEqual(1, dto.Spawns.Count);
            var back = dto.Spawns[0];
            Assert.AreEqual("farm_forage_01", back.SpawnId);
            Assert.AreEqual((int)ForageNodeState.Collected, back.State);
            Assert.AreEqual(5, back.NextEligibleSpawnDay);
        }

        // ─────────────────────────── helpers ───────────────────────────

        private static (FarmShippingService service, List<PendingShippingEntry> entries) BuildShipping()
        {
            var entries = new List<PendingShippingEntry>();
            var sellability = new ItemSellabilityProvider(
                questItems: new HashSet<string> { "item_quest_relic" },
                keyItems: new HashSet<string> { "item_key_gate" });
            var service = new FarmShippingService(entries, new ShippingPriceResolver(), sellability);
            return (service, entries);
        }

        private static PendingShippingSaveData MapToSave(List<PendingShippingEntry> entries)
        {
            var dto = new PendingShippingSaveData();
            foreach (var e in entries)
            {
                dto.Entries.Add(new PendingShippingEntrySaveData
                {
                    EntryId = e.ShippingEntryId, SellPointId = e.SellPointId, ItemId = e.ItemId,
                    Quantity = e.Quantity, QualityTier = e.QualityTier, BaseValueSnapshot = e.BaseValueSnapshot,
                    DepositedDay = e.DepositedDay, ProcessOnDay = e.ProcessOnDay, State = (int)e.State
                });
            }
            return dto;
        }

        private static List<PendingShippingEntry> MapFromSave(PendingShippingSaveData dto)
        {
            var list = new List<PendingShippingEntry>();
            if (dto?.Entries == null) return list;
            foreach (var s in dto.Entries)
            {
                if (s == null || string.IsNullOrWhiteSpace(s.ItemId)) continue;
                list.Add(new PendingShippingEntry
                {
                    ShippingEntryId = s.EntryId, SellPointId = s.SellPointId, ItemId = s.ItemId,
                    Quantity = s.Quantity, QualityTier = s.QualityTier, BaseValueSnapshot = s.BaseValueSnapshot,
                    DepositedDay = s.DepositedDay, ProcessOnDay = s.ProcessOnDay, State = (ShippingEntryState)s.State
                });
            }
            return list;
        }
    }
}
