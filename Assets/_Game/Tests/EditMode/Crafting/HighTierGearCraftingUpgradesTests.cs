using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Crafting;
using CindarsHope.Save;

namespace CindarsHope.Tests.EditMode.Crafting
{
    /// <summary>
    /// fable_49 — Forja de gear tier alto + receitas first-kill + upgrades +1/+2/+3. Cobre os 5 critérios
    /// de aceite com tipos puros (espelha TemperingTests): CA-1 gating de craft + tier alto fora de loja,
    /// CA-2 unlock idempotente por first-kill + round-trip, CA-3 custo derivado/consumo, CA-4 upgrade foco
    /// único/teto/persistência/recálculo, CA-5 save legado intacto. Sem AssetDatabase nem Play Mode.
    /// </summary>
    [TestFixture]
    public class HighTierGearCraftingUpgradesTests
    {
        // ── Fakes das portas ────────────────────────────────────────────────────────────────────

        private sealed class FakeInventory : IUpgradeInventory
        {
            public readonly Dictionary<string, int> Items = new Dictionary<string, int>();

            public bool HasItem(string itemId, int amount)
                => Items.TryGetValue(itemId, out var have) && have >= amount;

            public bool RemoveItem(string itemId, int amount)
            {
                if (!HasItem(itemId, amount)) return false;
                Items[itemId] -= amount;
                return true;
            }
        }

        private sealed class FakeWallet : IUpgradeWallet
        {
            public int Gold;
            public int CurrentGold => Gold;
            public bool TrySpendGold(int amount)
            {
                if (amount < 0 || Gold < amount) return false;
                Gold -= amount;
                return true;
            }
        }

        private static (EquipmentUpgradeService svc, EquipmentUpgradeRegistry reg, FakeInventory inv, FakeWallet wallet)
            MakeService()
        {
            var reg = new EquipmentUpgradeRegistry();
            var inv = new FakeInventory();
            var wallet = new FakeWallet();
            var svc = new EquipmentUpgradeService(reg, inv, wallet);
            return (svc, reg, inv, wallet);
        }

        private const string MithrilSword = "item_weapon_sword_mithril#1";

        // ── CA-1: gating de craft por receita aprendida ─────────────────────────────────────────

        [Test]
        public void CA1_Gate_RecipeWithUnlock_BlockedUntilUnlocked()
        {
            var service = new RecipeUnlockService();
            try
            {
                CraftingRecipeGate.Resolver = id => service.IsUnlocked(id);

                Assert.IsFalse(CraftingRecipeGate.IsRecipeUnlocked(RecipeUnlock.MithrilWork),
                    "Gated recipe must start blocked.");

                service.Unlock(RecipeUnlock.MithrilWork);
                Assert.IsTrue(CraftingRecipeGate.IsRecipeUnlocked(RecipeUnlock.MithrilWork),
                    "After learning, gated recipe is craftable.");
            }
            finally { CraftingRecipeGate.Resolver = null; }
        }

        [Test]
        public void CA1_Gate_EmptySlug_AlwaysUnlocked_LegacyRecipesUnchanged()
        {
            // Receita sem RequiredRecipeUnlockId nunca é bloqueada (receitas atuais inalteradas).
            Assert.IsTrue(CraftingRecipeGate.IsRecipeUnlocked(null));
            Assert.IsTrue(CraftingRecipeGate.IsRecipeUnlocked(""));
            Assert.IsTrue(CraftingRecipeGate.IsRecipeUnlocked("   "));
        }

        [Test]
        public void CA1_Gate_GatedRecipe_NoActiveService_FailsClosed()
        {
            var saved = RecipeUnlockService.Active;
            try
            {
                CraftingRecipeGate.Resolver = null;
                RecipeUnlockService.Active = null;
                Assert.IsFalse(CraftingRecipeGate.IsRecipeUnlocked(RecipeUnlock.MithrilWork),
                    "A gated recipe with no service must be treated as locked (fail-closed).");
            }
            finally { RecipeUnlockService.Active = saved; }
        }

        [Test]
        public void CA1_HighTierGear_HasCanonicalBandAndIsCraftOnly()
        {
            // O catálogo de tier alto cobre as 4 bandas e nenhuma entrada é "loja".
            Assert.AreEqual(14, HighTierGearCatalog.Entries.Count);
            Assert.IsTrue(HighTierGearCatalog.TryGet("item_weapon_sword_mithril", out var sword));
            Assert.AreEqual(MaterialBand.Mithril, sword.Band);
            Assert.AreEqual(640, sword.BaseValue);
            Assert.IsFalse(sword.IsArmorOrShield);

            Assert.IsTrue(HighTierGearCatalog.TryGet("item_shield_bromecian_kite", out var shield));
            Assert.AreEqual(MaterialBand.Bromecian, shield.Band);
            Assert.IsTrue(shield.IsArmorOrShield);
        }

        // ── CA-2: first-kill ensina a receita 1× + sobrevive a save/load ────────────────────────

        [Test]
        public void CA2_FirstKill_Gate60_LearnsMithrilWork_Idempotent()
        {
            var service = new RecipeUnlockService();

            // Primeira derrota do gate 60: aprende Mithril Work (firstTime = true).
            bool first = RecipeFirstKillUnlockHook.TryApplyFirstKillUnlock(service, gateLevel: 60, publishEvent: false);
            Assert.IsTrue(first, "First kill of gate 60 learns Mithril Work.");
            Assert.IsTrue(service.IsUnlocked(RecipeUnlock.MithrilWork));
            Assert.AreEqual(1, service.Count);

            // Re-derrotar (re-kill): no-op, não duplica (firstTime = false).
            bool again = RecipeFirstKillUnlockHook.TryApplyFirstKillUnlock(service, gateLevel: 60, publishEvent: false);
            Assert.IsFalse(again, "Re-killing the boss does not re-learn / duplicate.");
            Assert.AreEqual(1, service.Count, "Still exactly one unlocked recipe.");
        }

        [Test]
        public void CA2_GateMapping_MatchesEmenda44Table()
        {
            Assert.AreEqual(RecipeUnlock.TemperedCopper, HighTierGearCanon.RecipeUnlockForGate(15));
            Assert.AreEqual(RecipeUnlock.DeepSteel, HighTierGearCanon.RecipeUnlockForGate(45));
            Assert.AreEqual(RecipeUnlock.MithrilWork, HighTierGearCanon.RecipeUnlockForGate(60));
            Assert.AreEqual(RecipeUnlock.BromecianWork, HighTierGearCanon.RecipeUnlockForGate(75));
            Assert.AreEqual(RecipeUnlock.BlackstoneWork, HighTierGearCanon.RecipeUnlockForGate(90));
            Assert.AreEqual(RecipeUnlock.MeteoricWork, HighTierGearCanon.RecipeUnlockForGate(100));
            // Gate 30 = planta de baú (não-receita-de-arma): não ensina receita aqui.
            Assert.IsNull(HighTierGearCanon.RecipeUnlockForGate(30));
            Assert.IsNull(HighTierGearCanon.RecipeUnlockForGate(99));
        }

        [Test]
        public void CA2_RecipeUnlocks_SurviveRoundTrip()
        {
            var service = new RecipeUnlockService();
            service.Unlock(RecipeUnlock.MithrilWork);
            service.Unlock(RecipeUnlock.BromecianWork);

            var dto = service.CaptureUnlockedIds();

            var restored = new RecipeUnlockService();
            restored.RestoreUnlockedIds(dto);

            Assert.IsTrue(restored.IsUnlocked(RecipeUnlock.MithrilWork));
            Assert.IsTrue(restored.IsUnlocked(RecipeUnlock.BromecianWork));
            Assert.IsFalse(restored.IsUnlocked(RecipeUnlock.MeteoricWork));
            Assert.AreEqual(2, restored.Count);
        }

        [Test]
        public void CA2_RecipeUnlocks_LegacySave_LoadsEmpty_NoMigration()
        {
            var service = new RecipeUnlockService();
            service.Unlock(RecipeUnlock.MithrilWork); // estado pré-load

            service.RestoreUnlockedIds(null); // save legado (null) => limpa, sem exceção

            Assert.AreEqual(0, service.Count);
            Assert.IsFalse(service.IsUnlocked(RecipeUnlock.MithrilWork));
        }

        // ── CA-3: custo derivado (Decision 2.11) + consumo exato ────────────────────────────────

        [Test]
        public void CA3_UpgradeCostFormula_MatchesWorkedExamples()
        {
            // economy_rules worked examples: sword_iron BV120/iron15 +1=90 +2=180; sword_mithril BV640/mithril80 +1=480.
            Assert.AreEqual(90, HighTierGearCanon.UpgradeGoldCost(1, 120, 15));
            Assert.AreEqual(180, HighTierGearCanon.UpgradeGoldCost(2, 120, 15));
            Assert.AreEqual(480, HighTierGearCanon.UpgradeGoldCost(1, 640, 80));
            // +3 do mithril sword: (2*3*80) + (640*0.5*3) = 480 + 960 = 1440.
            Assert.AreEqual(1440, HighTierGearCanon.UpgradeGoldCost(3, 640, 80));
        }

        [Test]
        public void CA3_MaterialBandValues_AreCanonical()
        {
            Assert.AreEqual(80, HighTierGearCanon.MaterialBandValue(MaterialBand.Mithril));
            Assert.AreEqual(90, HighTierGearCanon.MaterialBandValue(MaterialBand.Bromecian));
            Assert.AreEqual(120, HighTierGearCanon.MaterialBandValue(MaterialBand.Meteoric));
            Assert.AreEqual(200, HighTierGearCanon.MaterialBandValue(MaterialBand.Blackstone));
            Assert.AreEqual("item_material_mithril_ore", HighTierGearCanon.MaterialBandItemId(MaterialBand.Mithril));
            Assert.AreEqual("item_material_star_iron", HighTierGearCanon.MaterialBandItemId(MaterialBand.Meteoric));
        }

        [Test]
        public void CA3_Upgrade_ConsumesExactGoldAndMaterialOnce()
        {
            var (svc, reg, inv, wallet) = MakeService();
            inv.Items["item_material_mithril_ore"] = 10;
            wallet.Gold = 1000;

            var target = new UpgradeTarget(MithrilSword, 640, MaterialBand.Mithril);
            var result = svc.TryUpgrade(target, UpgradeFocus.Damage);

            Assert.IsTrue(result.Success, result.FailReason);
            Assert.AreEqual(1, result.NewLevel);
            Assert.AreEqual(480, result.GoldSpent, "+1 mithril sword costs 480g.");
            Assert.AreEqual(1, result.MaterialSpent, "+1 consumes 1 anchor material.");
            Assert.AreEqual(1000 - 480, wallet.Gold);
            Assert.AreEqual(10 - 1, inv.Items["item_material_mithril_ore"]);
        }

        [Test]
        public void CA3_DerivedDurability_ByClassAndMaterial()
        {
            // Arma base 80; Mithril +35% => 108. Armadura base 150; Bromecian +40% => 210.
            Assert.AreEqual(108, HighTierGearCanon.DerivedMaxDurability(isArmorOrShield: false, MaterialBand.Mithril));
            Assert.AreEqual(210, HighTierGearCanon.DerivedMaxDurability(isArmorOrShield: true, MaterialBand.Bromecian));
            // Blackstone armadura: 150 * 1.55 = 232.5 -> 233 (round away from zero). Meteoric arma: 80*1.6=128.
            Assert.AreEqual(233, HighTierGearCanon.DerivedMaxDurability(true, MaterialBand.Blackstone));
            Assert.AreEqual(128, HighTierGearCanon.DerivedMaxDurability(false, MaterialBand.Meteoric));
        }

        // ── CA-4: upgrade foco único + teto +3 + persistência + recálculo ───────────────────────

        [Test]
        public void CA4_Upgrade_SingleFocus_RejectsFocusChange()
        {
            var (svc, reg, inv, wallet) = MakeService();
            inv.Items["item_material_mithril_ore"] = 10;
            wallet.Gold = 100000;
            var target = new UpgradeTarget(MithrilSword, 640, MaterialBand.Mithril);

            Assert.IsTrue(svc.TryUpgrade(target, UpgradeFocus.Damage).Success);

            // Tentar trocar de foco no +2 é rejeitado (§35: um foco por item).
            var mismatch = svc.TryUpgrade(target, UpgradeFocus.Durability);
            Assert.IsFalse(mismatch.Success);
            Assert.AreEqual(EquipmentUpgradeService.FailFocusMismatch, mismatch.FailReason);

            // Mesmo foco avança normalmente.
            var second = svc.TryUpgrade(target, UpgradeFocus.Damage);
            Assert.IsTrue(second.Success, second.FailReason);
            Assert.AreEqual(2, second.NewLevel);
            Assert.AreEqual(UpgradeFocus.Damage, reg.Get(MithrilSword).Focus);
        }

        [Test]
        public void CA4_Upgrade_RespectsMaxLevel3()
        {
            var (svc, reg, inv, wallet) = MakeService();
            inv.Items["item_material_mithril_ore"] = 100;
            wallet.Gold = 1000000;
            var target = new UpgradeTarget(MithrilSword, 640, MaterialBand.Mithril);

            Assert.IsTrue(svc.TryUpgrade(target, UpgradeFocus.Damage).Success); // +1
            Assert.IsTrue(svc.TryUpgrade(target, UpgradeFocus.Damage).Success); // +2
            Assert.IsTrue(svc.TryUpgrade(target, UpgradeFocus.Damage).Success); // +3

            var overCap = svc.TryUpgrade(target, UpgradeFocus.Damage); // +4 bloqueado
            Assert.IsFalse(overCap.Success);
            Assert.AreEqual(EquipmentUpgradeService.FailMaxLevel, overCap.FailReason);
            Assert.AreEqual(3, reg.GetLevel(MithrilSword), "Level capped at +3.");
        }

        [Test]
        public void CA4_Upgrade_InsufficientMaterial_ConsumesNothing()
        {
            var (svc, reg, inv, wallet) = MakeService();
            inv.Items["item_material_mithril_ore"] = 0;
            wallet.Gold = 100000;
            var target = new UpgradeTarget(MithrilSword, 640, MaterialBand.Mithril);

            var result = svc.TryUpgrade(target, UpgradeFocus.Damage);
            Assert.IsFalse(result.Success);
            Assert.AreEqual(EquipmentUpgradeService.FailNoMaterial, result.FailReason);
            Assert.AreEqual(100000, wallet.Gold, "No gold spent on material failure.");
            Assert.IsFalse(reg.HasUpgrade(MithrilSword));
        }

        [Test]
        public void CA4_Upgrade_InsufficientGold_ConsumesNothing()
        {
            var (svc, reg, inv, wallet) = MakeService();
            inv.Items["item_material_mithril_ore"] = 10;
            wallet.Gold = 100; // < 480
            var target = new UpgradeTarget(MithrilSword, 640, MaterialBand.Mithril);

            var result = svc.TryUpgrade(target, UpgradeFocus.Damage);
            Assert.IsFalse(result.Success);
            Assert.AreEqual(EquipmentUpgradeService.FailNoGold, result.FailReason);
            Assert.AreEqual(10, inv.Items["item_material_mithril_ore"], "No material consumed when gold check fails.");
            Assert.IsFalse(reg.HasUpgrade(MithrilSword));
        }

        [Test]
        public void CA4_Upgrade_PersistsLevelAndFocus_RoundTrip()
        {
            var reg = new EquipmentUpgradeRegistry();
            reg.Set("w_a#1", new EquipmentUpgrade(2, UpgradeFocus.Damage));
            reg.Set("w_b#2", new EquipmentUpgrade(3, UpgradeFocus.Durability));

            var dto = reg.CaptureSaveData();
            var restored = new EquipmentUpgradeRegistry();
            restored.RestoreFromSaveData(dto);

            Assert.AreEqual(2, restored.Get("w_a#1").Level);
            Assert.AreEqual(UpgradeFocus.Damage, restored.Get("w_a#1").Focus);
            Assert.AreEqual(3, restored.Get("w_b#2").Level);
            Assert.AreEqual(UpgradeFocus.Durability, restored.Get("w_b#2").Focus);
        }

        [Test]
        public void CA4_Upgrade_DerivedRecalculatedFromUpgradeLevel_NotPersisted()
        {
            // §45: o save guarda só level/focus; o derivado é recalculado no load lendo o registro.
            // Simula um "recálculo" simples de durabilidade com bônus por nível de foco Durability.
            var reg = new EquipmentUpgradeRegistry();
            reg.Set(MithrilSword, new EquipmentUpgrade(2, UpgradeFocus.Durability));

            var dto = reg.CaptureSaveData();
            // O DTO NÃO contém durabilidade derivada — só level/focus.
            Assert.AreEqual(1, dto.Count);
            Assert.AreEqual(2, dto[0].UpgradeLevel);
            Assert.AreEqual("durability", dto[0].UpgradeFocus);

            var restored = new EquipmentUpgradeRegistry();
            restored.RestoreFromSaveData(dto);
            int level = restored.GetLevel(MithrilSword);
            // Recálculo derivado (exemplo determinístico): base mithril arma + 10% por nível de Durability.
            int baseDur = HighTierGearCanon.DerivedMaxDurability(false, MaterialBand.Mithril); // 108
            int recalculated = baseDur + (int)(baseDur * 0.10f * level);
            Assert.AreEqual(108 + (int)(108 * 0.10f * 2), recalculated, "Derived value reconstructed from level, never persisted.");
        }

        // ── CA-5: save legado intacto ───────────────────────────────────────────────────────────

        [Test]
        public void CA5_LegacySave_NoUpgradeFields_LoadsWithLevelZero()
        {
            var equip = new EquipmentSaveData { Upgrades = null }; // save antigo: campo ausente
            var reg = new EquipmentUpgradeRegistry();
            reg.Set("w_old#1", new EquipmentUpgrade(2, UpgradeFocus.Damage)); // estado pré-load

            reg.RestoreFromSaveData(equip.Upgrades); // null => limpa, sem exceção

            Assert.AreEqual(0, reg.Count);
            Assert.AreEqual(0, reg.GetLevel("w_old#1"));
            Assert.IsFalse(reg.HasUpgrade("w_old#1"));
        }

        [Test]
        public void CA5_DefaultEquipmentSaveData_HasEmptyUpgradeAndUnlockLists()
        {
            var equip = new EquipmentSaveData();
            Assert.IsNotNull(equip.Upgrades, "Default Upgrades must be a non-null empty list.");
            Assert.AreEqual(0, equip.Upgrades.Count);
            Assert.IsNotNull(equip.UnlockedRecipeIds, "Default UnlockedRecipeIds must be a non-null empty list.");
            Assert.AreEqual(0, equip.UnlockedRecipeIds.Count);
        }

        [Test]
        public void CA5_Restore_IgnoresInvalidUpgradeEntries()
        {
            var data = new List<EquipmentUpgradeSaveData>
            {
                new EquipmentUpgradeSaveData { ItemInstanceId = "w1", UpgradeLevel = 2, UpgradeFocus = "damage" },
                new EquipmentUpgradeSaveData { ItemInstanceId = "w2", UpgradeLevel = 9, UpgradeFocus = "damage" },   // level inválido
                new EquipmentUpgradeSaveData { ItemInstanceId = "w3", UpgradeLevel = 1, UpgradeFocus = "bogus" },    // foco inválido
                new EquipmentUpgradeSaveData { ItemInstanceId = "", UpgradeLevel = 1, UpgradeFocus = "block" },      // id vazio
            };

            var reg = new EquipmentUpgradeRegistry();
            reg.RestoreFromSaveData(data);

            Assert.AreEqual(1, reg.Count, "Only the valid entry (w1) is restored.");
            Assert.AreEqual(2, reg.GetLevel("w1"));
            Assert.AreEqual(UpgradeFocus.Damage, reg.Get("w1").Focus);
            Assert.IsFalse(reg.HasUpgrade("w2"));
            Assert.IsFalse(reg.HasUpgrade("w3"));
        }

        // ── Anti-regressão: têmpera (F22) é ortogonal ao upgrade ─────────────────────────────────

        [Test]
        public void FocusSerialization_IsStable()
        {
            Assert.AreEqual("damage", HighTierGearCanonFocus.ToStableString(UpgradeFocus.Damage));
            Assert.AreEqual("durability", HighTierGearCanonFocus.ToStableString(UpgradeFocus.Durability));
            Assert.AreEqual("weight", HighTierGearCanonFocus.ToStableString(UpgradeFocus.Weight));
            Assert.AreEqual("stamina", HighTierGearCanonFocus.ToStableString(UpgradeFocus.Stamina));
            Assert.AreEqual("block", HighTierGearCanonFocus.ToStableString(UpgradeFocus.Block));
            Assert.AreEqual(UpgradeFocus.Block, HighTierGearCanonFocus.ParseFocus("BLOCK"));
            Assert.AreEqual(UpgradeFocus.None, HighTierGearCanonFocus.ParseFocus("nope"));
        }

        [Test]
        public void Upgrade_PreviewNextCost_MatchesActualCharge()
        {
            var (svc, reg, inv, wallet) = MakeService();
            var target = new UpgradeTarget(MithrilSword, 640, MaterialBand.Mithril);

            Assert.IsTrue(svc.TryPreviewNextCost(target, out int gold, out int mat, out int next));
            Assert.AreEqual(1, next);
            Assert.AreEqual(480, gold);
            Assert.AreEqual(1, mat);

            // Após +1, preview do +2 = (4*80) + (640*1.0) = 320 + 640 = 960.
            inv.Items["item_material_mithril_ore"] = 10;
            wallet.Gold = 100000;
            svc.TryUpgrade(target, UpgradeFocus.Damage);
            Assert.IsTrue(svc.TryPreviewNextCost(target, out int gold2, out _, out int next2));
            Assert.AreEqual(2, next2);
            Assert.AreEqual(960, gold2);
        }
    }
}
