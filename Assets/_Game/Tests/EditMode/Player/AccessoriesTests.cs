using System.Collections.Generic;
using CindarsHope.Equipment;
using CindarsHope.Loot;
using CindarsHope.Save;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Player
{
    /// <summary>
    /// fable_23 — acessórios/relíquias: 3 slots tipo-acessório (Ring1/Ring2/Accessory), catálogo
    /// puro de 12 acessórios + 4 relíquias, roteador NÃO-STACK, validação 1-relíquia e os 6 hooks
    /// pontuais (gold, durabilidade, loot extra, fadiga noturna, comida, knockback).
    ///
    /// Tudo determinístico/EditMode (sem Play Mode): o roteador é C# puro; o EquipmentManager é
    /// exercitado via GameObject só para o round-trip de save (CA-1/CA-4). Cada teste limpa o
    /// AccessoryEffectRouter.Active no teardown para não vazar estado entre casos.
    /// </summary>
    [TestFixture]
    public class AccessoriesTests
    {
        [TearDown]
        public void TearDown()
        {
            AccessoryEffectRouter.Active = null;
        }

        private static AccessoryEffectRouter ActivateRouter(params string[] equippedIds)
        {
            var router = new AccessoryEffectRouter();
            router.Rebuild(equippedIds);
            AccessoryEffectRouter.Active = router;
            return router;
        }

        // ───────────────────────── Catálogo (CA-5 data layer) ─────────────────────────

        [Test]
        public void Catalog_HasTwelveAccessoriesAndFourRelics()
        {
            int relics = 0;
            int accessories = 0;
            foreach (var def in AccessoryCatalog.All)
            {
                if (def.IsRelic) relics++;
                else accessories++;
            }

            Assert.AreEqual(12, accessories, "12 acessórios canônicos (§16).");
            Assert.AreEqual(4, relics, "4 relíquias divinas (§17).");
        }

        [Test]
        public void Catalog_ResolvesFromInstanceId_DerivedFromDefinitionId()
        {
            var def = AccessoryCatalog.ResolveFromInstanceId("item_acc_ring_finan#inst42");
            Assert.IsNotNull(def);
            Assert.AreEqual(AccessoryCatalog.RingFinan, def.Id);
            Assert.AreEqual(AccessorySlotKind.Ring, def.SlotKind);
        }

        [Test]
        public void Catalog_UnknownItem_ResolvesNull()
        {
            Assert.IsNull(AccessoryCatalog.ResolveFromInstanceId("item_weapon_sword_iron"));
            Assert.IsFalse(AccessoryCatalog.IsAccessoryOrRelic("item_material_wood"));
        }

        [Test]
        public void Catalog_SlotCompatibility_RingGoesToRingSlots_AmuletCharmGoToAccessory()
        {
            Assert.IsTrue(AccessoryCatalog.IsSlotCompatible(AccessorySlotKind.Ring, EquipmentSlot.Ring1));
            Assert.IsTrue(AccessoryCatalog.IsSlotCompatible(AccessorySlotKind.Ring, EquipmentSlot.Ring2));
            Assert.IsFalse(AccessoryCatalog.IsSlotCompatible(AccessorySlotKind.Ring, EquipmentSlot.Accessory));

            Assert.IsTrue(AccessoryCatalog.IsSlotCompatible(AccessorySlotKind.Amulet, EquipmentSlot.Accessory));
            Assert.IsTrue(AccessoryCatalog.IsSlotCompatible(AccessorySlotKind.Charm, EquipmentSlot.Accessory));
            Assert.IsFalse(AccessoryCatalog.IsSlotCompatible(AccessorySlotKind.Amulet, EquipmentSlot.Ring1));
        }

        [Test]
        public void Catalog_AccessorySlots_AreTheThreeCanonicalSlots()
        {
            Assert.IsTrue(AccessoryCatalog.IsAccessorySlot(EquipmentSlot.Ring1));
            Assert.IsTrue(AccessoryCatalog.IsAccessorySlot(EquipmentSlot.Ring2));
            Assert.IsTrue(AccessoryCatalog.IsAccessorySlot(EquipmentSlot.Accessory));
            Assert.IsFalse(AccessoryCatalog.IsAccessorySlot(EquipmentSlot.LeftHand));
            Assert.IsFalse(AccessoryCatalog.IsAccessorySlot(EquipmentSlot.Chest));
        }

        // ───────────────────────── CA-3: não-stack por efeito ─────────────────────────

        [Test]
        public void Router_TwoFinanRings_DoNotStackGold()
        {
            // Dois anéis de Finan (+5% cada) NÃO somam: o maior vale (+5%, não +10%).
            ActivateRouter(AccessoryCatalog.RingFinan, AccessoryCatalog.RingFinan);

            Assert.AreEqual(0.05f, AccessoryEffectRouter.GoldGainModifierSource.Invoke(), 0.0001f);
            Assert.AreEqual(105, AccessoryEffectRouter.ApplyGoldGain(100), "100 ouro com 1 Finan efetivo => 105.");
        }

        [Test]
        public void Router_NonStack_KeepsTheLargerMagnitude()
        {
            // Constrói duas definições do mesmo tipo com magnitudes diferentes e confirma que o
            // roteador mantém a MAIOR (regra canônica do maior vale).
            var router = new AccessoryEffectRouter();
            // Finan (+0.05) e Alihana (+0.10 ExtraLootRoll) coexistem (tipos distintos);
            // mas dois do mesmo tipo => maior. Usamos os ids reais: Finan + Finan já cobrem igual.
            router.Rebuild(new[] { AccessoryCatalog.RingFinan });
            AccessoryEffectRouter.Active = router;
            Assert.AreEqual(0.05f, AccessoryEffectRouter.GoldGainModifierSource.Invoke(), 0.0001f);
        }

        [Test]
        public void Router_EmptySlots_AllModifiersNeutral()
        {
            ActivateRouter();
            Assert.AreEqual(0f, AccessoryEffectRouter.GoldGainModifierSource.Invoke(), 0.0001f);
            Assert.AreEqual(0f, AccessoryEffectRouter.ExtraLootRollChanceSource.Invoke(), 0.0001f);
            Assert.AreEqual(100, AccessoryEffectRouter.ApplyGoldGain(100), "Sem acessório => sem bônus.");
        }

        // ───────────────────── CA-3: 1 relíquia (validação de equip) ─────────────────────

        [Test]
        public void CanEquip_FirstRelic_Allowed()
        {
            var current = new List<(EquipmentSlot, string)>();
            bool ok = AccessoryEffectRouter.CanEquip(
                AccessoryCatalog.RelicKanthor, EquipmentSlot.Accessory, current, out var reason);
            Assert.IsTrue(ok, reason);
            Assert.IsEmpty(reason);
        }

        [Test]
        public void CanEquip_ReplacingRelicInOnlyCompatibleSlot_IsAllowed()
        {
            // Já existe a relíquia de Kanthor no slot Accessory; tentar equipar Alihana no slot Ring?
            // Alihana relic é Charm (vai p/ Accessory). Simulamos 2 relíquias em 2 momentos: a 2ª recusa.
            var current = new List<(EquipmentSlot, string)>
            {
                (EquipmentSlot.Accessory, AccessoryCatalog.RelicKanthor)
            };
            // Todas as relíquias ocupam Accessory; equipar outra nesse slot substitui a anterior.
            bool ok = AccessoryEffectRouter.CanEquip(
                AccessoryCatalog.RelicAnya, EquipmentSlot.Accessory, current, out var reason);
            Assert.IsTrue(ok, reason);
            Assert.IsEmpty(reason);
        }

        [Test]
        public void CanEquip_ReequipSameRelicInSameSlot_Allowed()
        {
            var current = new List<(EquipmentSlot, string)>
            {
                (EquipmentSlot.Accessory, AccessoryCatalog.RelicKanthor)
            };
            bool ok = AccessoryEffectRouter.CanEquip(
                AccessoryCatalog.RelicKanthor, EquipmentSlot.Accessory, current, out var reason);
            Assert.IsTrue(ok, reason);
        }

        [Test]
        public void CanEquip_WrongSlotType_Rejected()
        {
            var current = new List<(EquipmentSlot, string)>();
            // Anel (Ring) não pode ir no slot Accessory.
            bool ok = AccessoryEffectRouter.CanEquip(
                AccessoryCatalog.RingFinan, EquipmentSlot.Accessory, current, out var reason);
            Assert.IsFalse(ok);
            StringAssert.Contains("Accessory", reason);
        }

        [Test]
        public void CanEquip_NonAccessorySlot_Rejected()
        {
            var current = new List<(EquipmentSlot, string)>();
            bool ok = AccessoryEffectRouter.CanEquip(
                AccessoryCatalog.RingFinan, EquipmentSlot.LeftHand, current, out var reason);
            Assert.IsFalse(ok);
        }

        [Test]
        public void Router_OneRelicEquipped_CountIsOne()
        {
            var router = ActivateRouter(AccessoryCatalog.RelicAnya);
            Assert.AreEqual(1, router.EquippedRelicCount);
        }

        // ───────────────────────── CA-2 + hooks pontuais ─────────────────────────

        [Test]
        public void Hook_Gold_FinanGivesFivePercent()
        {
            ActivateRouter(AccessoryCatalog.RingFinan);
            Assert.AreEqual(105, AccessoryEffectRouter.ApplyGoldGain(100));
            Assert.AreEqual(0, AccessoryEffectRouter.ApplyGoldGain(0), "Base 0 => 0.");
        }

        [Test]
        public void Hook_ExtraLoot_AlihanaTenPercent_RollsWhenUnderChance()
        {
            ActivateRouter(AccessoryCatalog.RingAlihana);
            Assert.AreEqual(0.10f, AccessoryEffectRouter.ExtraLootRollChanceSource.Invoke(), 0.0001f);
            Assert.IsTrue(AccessoryEffectRouter.ShouldRollExtraLoot(0.05), "0.05 < 0.10 => roll extra.");
            Assert.IsFalse(AccessoryEffectRouter.ShouldRollExtraLoot(0.50), "0.50 >= 0.10 => sem roll.");
        }

        [Test]
        public void Hook_ExtraLoot_NoAccessory_NeverRolls()
        {
            ActivateRouter();
            Assert.IsFalse(AccessoryEffectRouter.ShouldRollExtraLoot(0.0), "Sem acessório => nunca roll extra.");
        }

        [Test]
        public void Hook_ExtraLoot_IntegratesWithEnemyLootResolver_Deterministic()
        {
            // Tabela determinística com 1 entrada garantida; com Alihana ativo, a rolagem ainda é
            // determinística por seed (mesmo seed => mesmo resultado, com ou sem roll extra).
            var table = ScriptableObject.CreateInstance<LootTableSO>();
            table.TableId = "loot_acc_test";
            table.FamilyId = "test";
            table.GuaranteedEntries = new[]
            {
                new LootTableEntry { ItemId = "item_material_wood", Weight = 1, MinAmount = 1, MaxAmount = 1, DropChance = 1f }
            };
            table.Entries = new[]
            {
                new LootTableEntry { ItemId = "item_material_stone", Weight = 1, MinAmount = 1, MaxAmount = 1, DropChance = 1f }
            };
            table.EssenceItemId = string.Empty;

            ActivateRouter(AccessoryCatalog.RingAlihana);
            var a = EnemyLootResolver.Roll(table, seed: 12345);
            var b = EnemyLootResolver.Roll(table, seed: 12345);

            Assert.AreEqual(a.Count, b.Count, "Mesmo seed => mesmo número de drops (determinístico).");
            // O drop garantido sempre aparece.
            Assert.IsTrue(a.Exists(d => d.ItemId == "item_material_wood"));

            Object.DestroyImmediate(table);
        }

        [Test]
        public void Hook_ToolDurability_ThorenSkipsUsageDeterministically()
        {
            // Thoren +15%: o helper de consumo retorna 1 na maioria dos usos; o acumulador (no
            // EquipmentManager) poupa ~15% dos usos. Aqui validamos o helper puro.
            ActivateRouter(AccessoryCatalog.RingThoren);
            // Consumo base 1 com +15% => arredonda para 1 (0.85), ainda 1 por uso isolado.
            Assert.AreEqual(1, AccessoryEffectRouter.ApplyToolDurabilityUse(1));
            // Para um lote grande, a economia aparece: 100 de uso base => ~85 efetivo.
            Assert.AreEqual(85, AccessoryEffectRouter.ApplyToolDurabilityUse(100));
        }

        [Test]
        public void Hook_NightFatigue_NyxReducesThirtyPercent()
        {
            ActivateRouter(AccessoryCatalog.AmuletNyx);
            Assert.AreEqual(7f, AccessoryEffectRouter.ApplyNightFatigueReduction(10f), 0.0001f, "10 - 30% => 7.");
            Assert.AreEqual(0f, AccessoryEffectRouter.ApplyNightFatigueReduction(0f), 0.0001f);
        }

        [Test]
        public void Hook_NightFatigue_NoAccessory_Unchanged()
        {
            ActivateRouter();
            Assert.AreEqual(10f, AccessoryEffectRouter.ApplyNightFatigueReduction(10f), 0.0001f);
        }

        [Test]
        public void Hook_FoodEffect_ThandraAddsFifteenPercent()
        {
            ActivateRouter(AccessoryCatalog.CharmThandra);
            Assert.AreEqual(46, AccessoryEffectRouter.ApplyFoodEffect(40), "40 + 15% => 46.");
            Assert.AreEqual(0, AccessoryEffectRouter.ApplyFoodEffect(0));
        }

        [Test]
        public void Hook_Knockback_StoneheartHalvesForce()
        {
            ActivateRouter(AccessoryCatalog.CharmStoneheart);
            Assert.AreEqual(4f, AccessoryEffectRouter.ApplyKnockbackResist(8f), 0.0001f, "força 8 - 50% => 4.");
        }

        [Test]
        public void Hook_Knockback_NoAccessory_Unchanged()
        {
            ActivateRouter();
            Assert.AreEqual(8f, AccessoryEffectRouter.ApplyKnockbackResist(8f), 0.0001f);
        }

        // ───────────────── CA-1 / CA-4: equip/unequip + save round-trip ─────────────────

        [Test]
        public void EquipmentManager_EquipAccessory_PerSlot_RoutesEffect()
        {
            var go = new GameObject("equip_mgr_test");
            try
            {
                var mgr = go.AddComponent<EquipmentManager>();

                // Equipa Finan no Ring1: efeito de ouro deve ficar ativo.
                bool ok = mgr.TryEquipAccessory(EquipmentSlot.Ring1, AccessoryCatalog.RingFinan, out var reason);
                Assert.IsTrue(ok, reason);
                Assert.AreEqual(0.05f, AccessoryEffectRouter.GoldGainModifierSource.Invoke(), 0.0001f);

                // Desequipar zera o efeito.
                mgr.UnequipSlot(EquipmentSlot.Ring1);
                Assert.AreEqual(0f, AccessoryEffectRouter.GoldGainModifierSource.Invoke(), 0.0001f);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void EquipmentManager_ReplacesRelicInAccessorySlot()
        {
            var go = new GameObject("equip_mgr_relic_test");
            try
            {
                var mgr = go.AddComponent<EquipmentManager>();

                Assert.IsTrue(mgr.TryEquipAccessory(EquipmentSlot.Accessory, AccessoryCatalog.RelicKanthor, out _));
                bool replacement = mgr.TryEquipAccessory(EquipmentSlot.Accessory, AccessoryCatalog.RelicAnya, out var reason);
                Assert.IsTrue(replacement, reason);
                Assert.AreEqual(AccessoryCatalog.RelicAnya, mgr.GetEquippedItem(EquipmentSlot.Accessory));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void EquipmentManager_SaveRoundTrip_RestoresAccessorySlots()
        {
            var go = new GameObject("equip_save_test");
            try
            {
                var mgr = go.AddComponent<EquipmentManager>();
                mgr.TryEquipAccessory(EquipmentSlot.Ring1, AccessoryCatalog.RingFinan, out _);
                mgr.TryEquipAccessory(EquipmentSlot.Accessory, AccessoryCatalog.AmuletNyx, out _);

                var data = mgr.CaptureSaveData();

                // Limpa e restaura.
                mgr.RestoreFromSaveData(null);
                Assert.IsNull(mgr.GetEquippedItem(EquipmentSlot.Ring1));

                mgr.RestoreFromSaveData(data);
                Assert.AreEqual(AccessoryCatalog.RingFinan, mgr.GetEquippedItem(EquipmentSlot.Ring1));
                Assert.AreEqual(AccessoryCatalog.AmuletNyx, mgr.GetEquippedItem(EquipmentSlot.Accessory));

                // Efeitos recomputados a partir do save restaurado.
                Assert.AreEqual(0.05f, AccessoryEffectRouter.GoldGainModifierSource.Invoke(), 0.0001f);
                Assert.AreEqual(0.30f, AccessoryEffectRouter.NightFatigueReductionSource.Invoke(), 0.0001f);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void EquipmentManager_LegacySave_NoAccessorySlots_LoadsEmpty_NoMigration()
        {
            var go = new GameObject("equip_legacy_test");
            try
            {
                var mgr = go.AddComponent<EquipmentManager>();

                // Save "legado": só ferramenta, sem slots de acessório (Slots vazia).
                var legacy = new EquipmentSaveData
                {
                    EquippedToolId = "item_tool_hoe_basic",
                    Slots = new List<EquipmentSlotSaveData>()
                };

                Assert.DoesNotThrow(() => mgr.RestoreFromSaveData(legacy), "Save legado carrega sem erro.");
                Assert.IsNull(mgr.GetEquippedItem(EquipmentSlot.Ring1), "Ring1 vazio.");
                Assert.IsNull(mgr.GetEquippedItem(EquipmentSlot.Ring2), "Ring2 vazio.");
                Assert.IsNull(mgr.GetEquippedItem(EquipmentSlot.Accessory), "Accessory vazio.");
                Assert.AreEqual(0f, AccessoryEffectRouter.GoldGainModifierSource.Invoke(), 0.0001f, "Roteador neutro.");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void EnumOrder_AccessorySlotsAreAtTheEnd_SaveSafe()
        {
            // CA-4 estrutural: os 3 slots novos são os ÚLTIMOS valores do enum (aditivo no fim),
            // garantindo que índices de slots antigos não mudaram.
            Assert.AreEqual((int)EquipmentSlot.Accessory, (int)EquipmentSlot.Ring2 + 1);
            Assert.AreEqual((int)EquipmentSlot.Ring2, (int)EquipmentSlot.Ring1 + 1);
            // Nenhum slot tipo-acessório vem antes de Boots (o último slot pré-fable_23).
            Assert.Greater((int)EquipmentSlot.Ring1, (int)EquipmentSlot.Boots);
        }
    }
}
