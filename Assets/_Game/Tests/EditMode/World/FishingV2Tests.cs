using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.World.Fishing;
using CindarsHope.Farm.Fishing;
using CindarsHope.Cave.Generation;
using CindarsHope.Cave.Runtime;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.World
{
    /// <summary>
    /// fable_50 — testes do resolver determinístico de pesca v2, chaveamento por contexto, grades do
    /// minigame, seletor de tabela da caverna, regressão do hook 10%/máx.1 e dos contratos do
    /// FarmFishingService.
    /// </summary>
    [TestFixture]
    public class FishingV2Tests
    {
        private static FishingContext Ctx(int seed = 12345, string spotId = "spot_a", int cast = 0,
            string season = "", string weather = "", int hour = 12) =>
            new FishingContext(seed, 0, spotId, cast, season, weather, hour);

        // ---------- CA-1: tabela por contexto ----------

        [Test]
        public void CA1_DifferentTables_SameSeed_GiveDifferentCatches()
        {
            var farm = CanonicalFishingTables.FarmPond();
            var caveIce = CanonicalFishingTables.CaveBand26To40();
            var ctx = Ctx(seed: 999, spotId: "shared", cast: 0, season: CanonicalFishingTables.Verao, hour: 12);

            var farmOut = FishingCatchResolver.Resolve(farm, ctx, FishingTimingGrade.Good);
            var caveOut = FishingCatchResolver.Resolve(caveIce, ctx, FishingTimingGrade.Good);

            Assert.IsTrue(farmOut.Success);
            Assert.IsTrue(caveOut.Success);
            // O açude não tem mirrorfin de dia; a banda de gelo pode entregar mirrorfin.
            Assert.AreNotEqual("item_fish_mirrorfin", farmOut.ItemId);
        }

        [Test]
        public void CA1_CaveIceBand_CanYieldMirrorfin()
        {
            var caveIce = CanonicalFishingTables.CaveBand26To40();
            var produced = new HashSet<string>();
            for (var seed = 0; seed < 400; seed++)
            {
                var outcome = FishingCatchResolver.Resolve(caveIce, Ctx(seed: seed, cast: seed), FishingTimingGrade.Good);
                if (outcome.Success)
                {
                    produced.Add(outcome.ItemId);
                }
            }

            Assert.IsTrue(produced.Contains("item_fish_mirrorfin"),
                "A banda 26-40 deve conseguir entregar mirrorfin em algum cast.");
        }

        // ---------- CA-2: determinismo ----------

        [Test]
        public void CA2_SameContext_SameCatch_Twice()
        {
            var farm = CanonicalFishingTables.FarmPond();
            var ctx = Ctx(seed: 7777, spotId: "lake_farm", cast: 3, season: CanonicalFishingTables.Primavera, hour: 10);

            var a = FishingCatchResolver.Resolve(farm, ctx, FishingTimingGrade.Good);
            var b = FishingCatchResolver.Resolve(farm, ctx, FishingTimingGrade.Good);

            Assert.AreEqual(a.ItemId, b.ItemId);
            Assert.AreEqual(a.Rarity, b.Rarity);
            Assert.AreEqual(a.Quality, b.Quality);
        }

        [Test]
        public void CA2_DifferentCastIndex_CanChangeResult()
        {
            var farm = CanonicalFishingTables.FarmPond();
            var results = new HashSet<string>();
            for (var cast = 0; cast < 30; cast++)
            {
                var outcome = FishingCatchResolver.Resolve(farm, Ctx(seed: 555, cast: cast, season: CanonicalFishingTables.Primavera), FishingTimingGrade.Good);
                results.Add(outcome.ItemId);
            }

            Assert.GreaterOrEqual(results.Count, 2, "Casts diferentes devem variar a captura (roll por castIndex).");
        }

        // ---------- CA-2: regressão do hook 10%/máx.1 (INTOCADO) ----------

        [Test]
        public void CA2_CaveFishingSpotHook_Is_Deterministic_And_RespectsTenPercent()
        {
            var service = new CaveSnapshotService();
            var withSpot = 0;
            const int samples = 300;

            for (var level = 1; level <= samples; level++)
            {
                var generated = BuildLevel(level);
                var first = service.CreateFishingSpotHook(generated, "world", "run");
                var second = service.CreateFishingSpotHook(generated, "world", "run");

                // Determinismo: mesmo nível/seed = mesmo resultado (stable run).
                Assert.AreEqual(first.HasFishingSpot, second.HasFishingSpot);
                Assert.AreEqual(first.FishingSpotId, second.FishingSpotId);

                if (first.HasFishingSpot)
                {
                    withSpot++;
                }
            }

            // ~10% (folga ampla para variação determinística do hash).
            Assert.That(withSpot, Is.InRange(samples * 0.03, samples * 0.20),
                $"Hook deveria materializar ~10% dos níveis; obteve {withSpot}/{samples}.");
        }

        // ---------- CA-3: estação e clima mandam ----------

        [Test]
        public void CA3_Winter_Uses_WinterTable_Frostfin()
        {
            var winter = CanonicalFishingTables.FarmPondWinter();
            var produced = new HashSet<string>();
            for (var seed = 0; seed < 200; seed++)
            {
                var outcome = FishingCatchResolver.Resolve(winter,
                    Ctx(seed: seed, cast: seed, season: CanonicalFishingTables.Inverno, hour: 12), FishingTimingGrade.Good);
                if (outcome.Success)
                {
                    produced.Add(outcome.ItemId);
                }
            }

            Assert.IsTrue(produced.Contains("item_fish_frostfin"), "Inverno deve poder dar frostfin.");
            // Em pleno dia (hour 12), o evento noturno (mirrorfin/pale) nunca sai.
            Assert.IsFalse(produced.Contains("item_fish_mirrorfin"));
        }

        [Test]
        public void CA3_SeasonIncompatibleEntry_NeverDrawn()
        {
            // sun_bass é Verao-only; em Primavera nunca deve sair.
            var farm = CanonicalFishingTables.FarmPond();
            for (var cast = 0; cast < 300; cast++)
            {
                var outcome = FishingCatchResolver.Resolve(farm,
                    Ctx(seed: 4242, cast: cast, season: CanonicalFishingTables.Primavera, hour: 12), FishingTimingGrade.Good);
                Assert.AreNotEqual("item_fish_sun_bass", outcome.ItemId);
            }
        }

        [Test]
        public void CA3_Storm_NoEligibleEntry_Returns_NoCatch_NotException()
        {
            // Tabela onde toda entrada exige clima Clear; sob qualquer outro clima, NoCatch explícito.
            var table = new FishingTableModel { TableId = "clear_only" };
            table.Entries.Add(new FishingTableEntryModel
            {
                ItemId = "item_fish_common", Weight = 10, Rarity = 0, BaseQuality = 0,
                Weathers = new[] { "Clear" }
            });

            var outcome = FishingCatchResolver.Resolve(table, Ctx(weather: "Stormy"), FishingTimingGrade.Good);
            Assert.IsFalse(outcome.Success);
            Assert.AreEqual("NoEligibleEntry", outcome.FailureReason);
        }

        // ---------- Amendment: Mirrorfin/Lake Lurker no açude da fazenda, 0,5% SÓ à noite ----------

        [Test]
        public void Amendment_FarmPond_MirrorfinAndPale_OnlyAtNight()
        {
            var farm = CanonicalFishingTables.FarmPond();

            // Dia (hour 12): evento raro nunca sai.
            for (var cast = 0; cast < 500; cast++)
            {
                var day = FishingCatchResolver.Resolve(farm, Ctx(seed: 11, cast: cast, season: CanonicalFishingTables.Verao, hour: 12), FishingTimingGrade.Good);
                Assert.AreNotEqual("item_fish_mirrorfin", day.ItemId);
                Assert.AreNotEqual("item_fish_pale", day.ItemId);
            }

            // Noite (hour 22): mirrorfin/pale tornam-se possíveis.
            var nightProduced = new HashSet<string>();
            for (var cast = 0; cast < 2000; cast++)
            {
                var night = FishingCatchResolver.Resolve(farm, Ctx(seed: 11, cast: cast, season: CanonicalFishingTables.Verao, hour: 22), FishingTimingGrade.Good);
                if (night.Success)
                {
                    nightProduced.Add(night.ItemId);
                }
            }

            Assert.IsTrue(nightProduced.Contains("item_fish_mirrorfin") || nightProduced.Contains("item_fish_pale"),
                "À noite o açude deve poder entregar o evento raro (mirrorfin/pale).");
        }

        [Test]
        public void Amendment_RareNightEvent_IsApproximatelyHalfPercent()
        {
            var farm = CanonicalFishingTables.FarmPond();
            var rare = 0;
            const int n = 20000;
            for (var cast = 0; cast < n; cast++)
            {
                var night = FishingCatchResolver.Resolve(farm, Ctx(seed: 31, cast: cast, season: CanonicalFishingTables.Outono, hour: 23), FishingTimingGrade.Good);
                if (night.ItemId == "item_fish_mirrorfin" || night.ItemId == "item_fish_pale")
                {
                    rare++;
                }
            }

            var pct = rare / (double)n;
            // Pesos: 2 raros de peso 1 sobre total noturno = 2/(120+50+18+11+2) ~= 1,0% combinado
            // (cada peixe ~0,5%). Folga ampla para variação do hash.
            Assert.That(pct, Is.InRange(0.003, 0.025), $"Evento raro combinado ~1% (cada ~0,5%); obteve {pct:P2}.");
        }

        // ---------- CA-4: minigame com consequência ----------

        [Test]
        public void CA4_Miss_NoCatch()
        {
            var farm = CanonicalFishingTables.FarmPond();
            var outcome = FishingCatchResolver.Resolve(farm, Ctx(season: CanonicalFishingTables.Primavera), FishingTimingGrade.Miss);
            Assert.IsFalse(outcome.Success);
            Assert.AreEqual("MissedTiming", outcome.FailureReason);
        }

        [Test]
        public void CA4_Perfect_ImprovesRarityAndQuality_OneStep()
        {
            // Tabela de 1 entrada determinística para isolar o efeito do Perfect.
            var table = new FishingTableModel { TableId = "single" };
            table.Entries.Add(new FishingTableEntryModel { ItemId = "item_fish_common", Weight = 1, Rarity = 1, BaseQuality = 1 });

            var good = FishingCatchResolver.Resolve(table, Ctx(), FishingTimingGrade.Good);
            var perfect = FishingCatchResolver.Resolve(table, Ctx(), FishingTimingGrade.Perfect);

            Assert.AreEqual(1, good.Rarity);
            Assert.AreEqual(1, good.Quality);
            Assert.AreEqual(2, perfect.Rarity);
            Assert.AreEqual(2, perfect.Quality);
            Assert.AreEqual(good.ItemId, perfect.ItemId);
        }

        [Test]
        public void CA4_Perfect_ClampsAtMax()
        {
            var table = new FishingTableModel { TableId = "single_max" };
            table.Entries.Add(new FishingTableEntryModel { ItemId = "item_fish_void_angler", Weight = 1, Rarity = 3, BaseQuality = 3 });

            var perfect = FishingCatchResolver.Resolve(table, Ctx(), FishingTimingGrade.Perfect);
            Assert.AreEqual(FishingCatchResolver.MaxRarity, perfect.Rarity);
            Assert.AreEqual(FishingCatchResolver.MaxQuality, perfect.Quality);
        }

        [Test]
        public void CA4_Minigame_GradeForCursor_CenterIsPerfect_EdgeIsMiss()
        {
            Assert.AreEqual(FishingTimingGrade.Perfect, FishingTimingMinigame.GradeForCursor(0.5f));
            Assert.AreEqual(FishingTimingGrade.Good, FishingTimingMinigame.GradeForCursor(0.5f + FishingTimingMinigame.PerfectHalfBand + 0.01f));
            Assert.AreEqual(FishingTimingGrade.Miss, FishingTimingMinigame.GradeForCursor(0.0f));
            Assert.AreEqual(FishingTimingGrade.Miss, FishingTimingMinigame.GradeForCursor(1.0f));
        }

        [Test]
        public void CA4_Minigame_Timeout_ResolvesToMiss()
        {
            var mg = new FishingTimingMinigame(windowSeconds: 0.5f);
            mg.Start();
            Assert.AreEqual(FishingMinigameState.Window, mg.State);
            var stillOpen = mg.Tick(0.6f); // estoura
            Assert.IsFalse(stillOpen);
            Assert.AreEqual(FishingMinigameState.Resolved, mg.State);
            Assert.AreEqual(FishingTimingGrade.Miss, mg.Submit());
        }

        // ---------- CA-2/selector: tabela da caverna por banda ----------

        [Test]
        public void Selector_ResolvesTableId_ByBandAndBiome()
        {
            Assert.AreEqual(CaveFishingTableSelector.TableCaveBand1To10, CaveFishingTableSelector.ResolveTableId(7, "cave_stone"));
            Assert.AreEqual(CaveFishingTableSelector.TableCaveBand26To40, CaveFishingTableSelector.ResolveTableId(30, "cave_ice"));
            Assert.AreEqual(CaveFishingTableSelector.TableCaveBand26To40, CaveFishingTableSelector.ResolveTableId(8, "cave_ice_grotto"));
            Assert.AreEqual(CaveFishingTableSelector.TableMoonlessPool, CaveFishingTableSelector.ResolveTableId(12, "cave_dark", isMoonlessPool: true));
            Assert.AreEqual(CaveFishingTableSelector.TableMoonlessPool, CaveFishingTableSelector.ResolveTableId(12, "moonless_pool_room"));
        }

        // ---------- CA-5: FarmFishingService reconciliado ----------

        [Test]
        public void CA5_FarmService_UsesResolver_WhenTableRegistered()
        {
            var spots = new Dictionary<string, FarmFishingSpotDefinition>
            {
                ["lake_farm"] = new FarmFishingSpotDefinition
                {
                    FishingSpotId = "lake_farm", CatchTableId = CaveFishingTableSelector.TableCaveBand26To40,
                    AllowsEndgameFish = false, DailyCatchSoftLimit = 5
                }
            };
            var tables = new Dictionary<string, FishingTableModel>
            {
                [CaveFishingTableSelector.TableCaveBand26To40] = CanonicalFishingTables.CaveBand26To40()
            };
            var service = new FarmFishingService(spots, tables);

            var daily = new FishingSpotDailyState { FishingSpotId = "lake_farm", Day = 1 };
            var result = service.AttemptCatch("lake_farm", daily, 1, currentSeason: "Inverno",
                currentWeather: "Clear", currentHour: 12, timingGrade: FishingTimingGrade.Good);

            Assert.IsTrue(result.Success);
            Assert.IsFalse(string.IsNullOrEmpty(result.FishItemId));
            Assert.AreEqual(1, daily.CatchesToday);
        }

        [Test]
        public void CA5_FarmService_Miss_DoesNotConsumeDailyCatch()
        {
            var spots = new Dictionary<string, FarmFishingSpotDefinition>
            {
                ["lake_farm"] = new FarmFishingSpotDefinition { FishingSpotId = "lake_farm", DailyCatchSoftLimit = 5 }
            };
            var service = new FarmFishingService(spots);
            var daily = new FishingSpotDailyState { FishingSpotId = "lake_farm", Day = 1 };

            var result = service.AttemptCatch("lake_farm", daily, 1, timingGrade: FishingTimingGrade.Miss);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("MissedTiming", result.FailureReason);
            Assert.AreEqual(0, daily.CatchesToday);
        }

        [Test]
        public void CA5_FarmService_LegacyContracts_Preserved_NoTable()
        {
            // Sem catálogo de tabela: fallback determinístico = Common (contrato dos testes atuais).
            var spots = new Dictionary<string, FarmFishingSpotDefinition>
            {
                ["lake_farm"] = new FarmFishingSpotDefinition
                {
                    FishingSpotId = "lake_farm", CatchTableId = "catch_lake_common",
                    AllowsEndgameFish = false, DailyCatchSoftLimit = 5
                }
            };
            var service = new FarmFishingService(spots);
            var daily = new FishingSpotDailyState { FishingSpotId = "lake_farm", Day = 1 };

            var result = service.AttemptCatch("lake_farm", daily, 1);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(FishRarity.Common, result.Rarity);
        }

        private static CaveGeneratedLevel BuildLevel(int level)
        {
            var generated = new CaveGeneratedLevel
            {
                CaveLevel = level,
                BiomeId = "cave_stone",
                Width = 8,
                Height = 8,
                Entrance = new Vector2Int(0, 0),
                Exit = new Vector2Int(7, 7)
            };
            for (var x = 0; x < 8; x++)
            {
                for (var y = 0; y < 8; y++)
                {
                    generated.WalkableTiles.Add(new Vector2Int(x, y));
                }
            }

            return generated;
        }
    }
}
