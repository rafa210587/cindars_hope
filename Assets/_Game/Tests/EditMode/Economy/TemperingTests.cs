using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using CindarsHope.Combat;
using CindarsHope.Economy;
using CindarsHope.Foundation;
using CindarsHope.Save;

namespace CindarsHope.Tests.EditMode.Economy
{
    /// <summary>
    /// fable_22 — Têmpera de Essência. Cobre os 5 critérios de aceite com tipos puros:
    /// CA-1 tag no matching F06, CA-2 invariante 1-elemento + devolução, CA-3 gate, CA-4 persistência
    /// aditiva (round-trip + load legado), CA-5 óleo sobrepõe têmpera. Sem AssetDatabase nem Play Mode.
    /// </summary>
    [TestFixture]
    public class TemperingTests
    {
        // ── Fakes das portas do serviço ─────────────────────────────────────────────────────────

        private sealed class FakeInventory : ITemperingInventory
        {
            public readonly Dictionary<string, int> Items = new Dictionary<string, int>();

            public bool HasItem(string itemId, int amount)
            {
                return Items.TryGetValue(itemId, out var have) && have >= amount;
            }

            public bool RemoveItem(string itemId, int amount)
            {
                if (!HasItem(itemId, amount)) return false;
                Items[itemId] -= amount;
                return true;
            }

            public bool AddItem(string itemId, int amount)
            {
                Items.TryGetValue(itemId, out var have);
                Items[itemId] = have + amount;
                return true;
            }
        }

        private sealed class FakeWallet : ITemperingWallet
        {
            public int Gold;
            public int CurrentGold => Gold;
            public bool TrySpendGold(int amount)
            {
                if (amount <= 0 || Gold < amount) return false;
                Gold -= amount;
                return true;
            }
        }

        private sealed class FakeClassifier : ITemperingItemClassifier
        {
            public bool WeaponResult = true;
            public bool ToolResult = false;
            public bool IsWeaponInstance(string id) => WeaponResult;
            public bool IsToolInstance(string id) => ToolResult;
        }

        private static (TemperingService svc, WeaponInfusionRegistry reg, FakeInventory inv, FakeWallet wallet, FakeClassifier cls)
            MakeService()
        {
            var reg = new WeaponInfusionRegistry();
            var inv = new FakeInventory();
            var wallet = new FakeWallet();
            var cls = new FakeClassifier();
            var svc = new TemperingService(reg, inv, wallet, cls);
            return (svc, reg, inv, wallet, cls);
        }

        private const string Weapon = "weapon_sword_iron#1";

        // ── CA-1: tag de infusão entra no matching canônico F06 ─────────────────────────────────

        private static EnemyVulnerabilityProfileSO MakeProfile(MaterialMultiplier[] materials)
        {
            var p = ScriptableObject.CreateInstance<EnemyVulnerabilityProfileSO>();
            p.VulnerabilityProfileId = "vuln_temper_test";
            p.ElementMultipliers = new ElementMultiplier[0];
            p.MaterialMultipliers = materials ?? new MaterialMultiplier[0];
            p.StatusVulnerabilities = new StatusVulnerability[0];
            return p;
        }

        [Test]
        public void CA1_FireEdgeTag_BonusOnlyAgainstDeclaredVulnerability()
        {
            // Arma temperada fire => tag de gume "FireEdge".
            Assert.AreEqual("FireEdge", TemperingCanon.EdgeTag(TemperingElement.Fire));

            var vulnerable = MakeProfile(new[] { new MaterialMultiplier { MaterialTag = "FireEdge", Multiplier = 1.5f } });
            var neutral = MakeProfile(new[] { new MaterialMultiplier { MaterialTag = "silver", Multiplier = 1.5f } });

            var infusedTags = new[] { "FireEdge" };

            // Contra inimigo com vulnerabilidade declarada: bônus 1.5x via matching F06.
            Assert.AreEqual(1.5f,
                VulnerabilityMatcher.GetDamageMultiplier(vulnerable, DamageType.Physical, infusedTags), 0.0001f,
                "Tempered weapon bonus must apply against a declared FireEdge vulnerability.");

            // Contra inimigo NÃO vulnerável: idêntico ao sem-têmpera (1.0).
            Assert.AreEqual(1f,
                VulnerabilityMatcher.GetDamageMultiplier(neutral, DamageType.Physical, infusedTags), 0.0001f,
                "Against a non-vulnerable enemy the bonus must be neutral (regra-mãe do adapter).");

            Object.DestroyImmediate(vulnerable);
            Object.DestroyImmediate(neutral);
        }

        [Test]
        public void CA1_RegistryEdgeTag_FeedsMatchingForEquippedInstance()
        {
            var (svc, reg, inv, wallet, _) = MakeService();
            inv.Items["item_essence_fire"] = 3;
            wallet.Gold = 1000;

            var result = svc.ApplyTempering(Weapon, TemperingElement.Fire, 1, gateOpen: true);
            Assert.IsTrue(result.Success, result.FailReason);

            // O ponto de combate lê a tag de gume daqui; deve ser exatamente FireEdge.
            Assert.AreEqual("FireEdge", reg.GetEdgeTag(Weapon));

            var vulnerable = MakeProfile(new[] { new MaterialMultiplier { MaterialTag = "FireEdge", Multiplier = 1.5f } });
            float mult = VulnerabilityMatcher.GetDamageMultiplier(vulnerable, DamageType.Physical, new[] { reg.GetEdgeTag(Weapon) });
            Assert.AreEqual(1.5f, mult, 0.0001f);
            Object.DestroyImmediate(vulnerable);
        }

        // ── CA-2: invariante 1 elemento + devolução de metade ───────────────────────────────────

        [Test]
        public void CA2_ApplyTempering_T1_ConsumesEssencesAndGold()
        {
            var (svc, reg, inv, wallet, _) = MakeService();
            inv.Items["item_essence_fire"] = 3;
            wallet.Gold = 200;

            var result = svc.ApplyTempering(Weapon, TemperingElement.Fire, 1, gateOpen: true);

            Assert.IsTrue(result.Success, result.FailReason);
            Assert.AreEqual(0, inv.Items["item_essence_fire"], "T1 consumes 3 fire essences.");
            Assert.AreEqual(50, wallet.Gold, "T1 costs 150g.");
            Assert.AreEqual(TemperingElement.Fire, reg.Get(Weapon).Element);
            Assert.AreEqual(1, reg.Get(Weapon).Tier);
        }

        [Test]
        public void CA2_T2_RequiresSixEssencesPlusVoidCatalystAnd600Gold()
        {
            var (svc, reg, inv, wallet, _) = MakeService();
            inv.Items["item_essence_fire"] = 6;
            inv.Items["item_essence_void"] = 1;
            wallet.Gold = 600;

            var result = svc.ApplyTempering(Weapon, TemperingElement.Fire, 2, gateOpen: true);

            Assert.IsTrue(result.Success, result.FailReason);
            Assert.AreEqual(0, inv.Items["item_essence_fire"], "T2 consumes 6 element essences.");
            Assert.AreEqual(0, inv.Items["item_essence_void"], "T2 consumes 1 void catalyst.");
            Assert.AreEqual(0, wallet.Gold, "T2 costs 600g.");
            Assert.AreEqual(2, reg.Get(Weapon).Tier);
        }

        [Test]
        public void CA2_ReTemper_ReplacesElement_AndRefundsHalfEssences_NeverGold()
        {
            var (svc, reg, inv, wallet, _) = MakeService();
            // Primeira têmpera fire T1 (gasta 3 fire).
            inv.Items["item_essence_fire"] = 3;
            inv.Items["item_essence_ice"] = 3;
            wallet.Gold = 300;
            Assert.IsTrue(svc.ApplyTempering(Weapon, TemperingElement.Fire, 1, true).Success);
            Assert.AreEqual(150, wallet.Gold);

            // Re-têmpera para ice T1: gasta 3 ice + 150g, devolve metade das 3 fire = 1 (truncado).
            var result = svc.ApplyTempering(Weapon, TemperingElement.Ice, 1, true);

            Assert.IsTrue(result.Success, result.FailReason);
            Assert.AreEqual(TemperingElement.Ice, reg.Get(Weapon).Element, "Element must be replaced (1 element per weapon).");
            Assert.AreEqual(0, inv.Items["item_essence_ice"], "New ice essences consumed.");
            Assert.AreEqual(1, inv.Items["item_essence_fire"], "Half of the 3 old fire essences refunded (3/2 = 1).");
            Assert.AreEqual(TemperingElement.Fire, result.RefundedElement);
            Assert.AreEqual(1, result.RefundedEssences);
            Assert.AreEqual(0, wallet.Gold, "Gold is never refunded; second temper still costs 150g.");
        }

        [Test]
        public void CA2_NeverTwoInfusionsOnSameWeapon()
        {
            var (svc, reg, inv, wallet, _) = MakeService();
            inv.Items["item_essence_fire"] = 3;
            inv.Items["item_essence_lightning"] = 3;
            wallet.Gold = 1000;

            svc.ApplyTempering(Weapon, TemperingElement.Fire, 1, true);
            svc.ApplyTempering(Weapon, TemperingElement.Lightning, 1, true);

            // Apenas uma infusão (a última); nunca soma de duas.
            Assert.AreEqual(TemperingElement.Lightning, reg.Get(Weapon).Element);
            Assert.AreEqual("ShockEdge", reg.GetEdgeTag(Weapon));
            Assert.AreEqual(1, reg.Count, "Exactly one infusion entry for the instance.");
        }

        [Test]
        public void CA2_InsufficientResources_DoesNotConsumeAnything()
        {
            var (svc, reg, inv, wallet, _) = MakeService();
            inv.Items["item_essence_fire"] = 2; // falta 1
            wallet.Gold = 150;

            var result = svc.ApplyTempering(Weapon, TemperingElement.Fire, 1, true);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("NO_ESSENCE", result.FailReason);
            Assert.AreEqual(2, inv.Items["item_essence_fire"], "No essence consumed on failure.");
            Assert.AreEqual(150, wallet.Gold, "No gold spent on failure.");
            Assert.IsFalse(reg.HasInfusion(Weapon));
        }

        [Test]
        public void CA2_InsufficientGold_DoesNotConsumeEssences()
        {
            var (svc, reg, inv, wallet, _) = MakeService();
            inv.Items["item_essence_fire"] = 3;
            wallet.Gold = 100; // falta 50

            var result = svc.ApplyTempering(Weapon, TemperingElement.Fire, 1, true);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("NO_GOLD", result.FailReason);
            Assert.AreEqual(3, inv.Items["item_essence_fire"], "No essence consumed when gold check fails first.");
        }

        // ── CA-3: gate narrativo ────────────────────────────────────────────────────────────────

        [Test]
        public void CA3_Gate_BlocksBeforeBrumdarChainAndAct1()
        {
            Assert.IsFalse(TemperingService.IsGateOpen(brumdarChainDone: false, act1Reached: true));
            Assert.IsFalse(TemperingService.IsGateOpen(brumdarChainDone: true, act1Reached: false));
            Assert.IsFalse(TemperingService.IsGateOpen(false, false));
            Assert.IsTrue(TemperingService.IsGateOpen(true, true));
        }

        [Test]
        public void CA3_ApplyTempering_FailsWhenGateClosed()
        {
            var (svc, reg, inv, wallet, _) = MakeService();
            inv.Items["item_essence_fire"] = 3;
            wallet.Gold = 1000;

            var result = svc.ApplyTempering(Weapon, TemperingElement.Fire, 1, gateOpen: false);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("GATE_CLOSED", result.FailReason);
            Assert.AreEqual(3, inv.Items["item_essence_fire"], "Closed gate consumes nothing.");
            Assert.IsFalse(reg.HasInfusion(Weapon));
        }

        [Test]
        public void CA3_ForgeAccess_FailClosedWithoutResolver()
        {
            var savedResolver = TemperingForgeAccess.GateResolver;
            var savedUi = TemperingForgeAccess.OpenForgeUi;
            try
            {
                TemperingForgeAccess.GateResolver = null;
                Assert.IsFalse(TemperingForgeAccess.IsGateOpen(), "No resolver => fail-closed.");

                TemperingForgeAccess.GateResolver = () => true;
                Assert.IsTrue(TemperingForgeAccess.IsGateOpen());

                TemperingForgeAccess.GateResolver = () => false;
                Assert.IsFalse(TemperingForgeAccess.IsGateOpen());
            }
            finally
            {
                TemperingForgeAccess.GateResolver = savedResolver;
                TemperingForgeAccess.OpenForgeUi = savedUi;
            }
        }

        // ── Ferramentas: só T1 ──────────────────────────────────────────────────────────────────

        [Test]
        public void Tool_AcceptsT1_RejectsT2()
        {
            var (svc, reg, inv, wallet, cls) = MakeService();
            cls.WeaponResult = false;
            cls.ToolResult = true;
            inv.Items["item_essence_fire"] = 6;
            inv.Items["item_essence_void"] = 1;
            wallet.Gold = 1000;

            var t2 = svc.ApplyTempering("item_tool_axe_basic#1", TemperingElement.Fire, 2, true);
            Assert.IsFalse(t2.Success);
            Assert.AreEqual("TOOL_T1_ONLY", t2.FailReason);

            var t1 = svc.ApplyTempering("item_tool_axe_basic#1", TemperingElement.Fire, 1, true);
            Assert.IsTrue(t1.Success, t1.FailReason);
            Assert.AreEqual(1, reg.Get("item_tool_axe_basic#1").Tier);
        }

        [Test]
        public void NonTemperable_Rejected()
        {
            var (svc, _, inv, wallet, cls) = MakeService();
            cls.WeaponResult = false;
            cls.ToolResult = false;
            inv.Items["item_essence_fire"] = 3;
            wallet.Gold = 1000;

            var result = svc.ApplyTempering("item_material_stone#1", TemperingElement.Fire, 1, true);
            Assert.IsFalse(result.Success);
            Assert.AreEqual("NOT_TEMPERABLE", result.FailReason);
        }

        // ── CA-4: persistência aditiva (round-trip + load legado) ────────────────────────────────

        [Test]
        public void CA4_Infusion_SurvivesRoundTrip()
        {
            var reg = new WeaponInfusionRegistry();
            reg.Set("weapon_a#1", new WeaponInfusion(TemperingElement.Fire, 1));
            reg.Set("weapon_b#2", new WeaponInfusion(TemperingElement.Void, 2));

            var dto = reg.CaptureSaveData();

            var restored = new WeaponInfusionRegistry();
            restored.RestoreFromSaveData(dto);

            Assert.AreEqual(TemperingElement.Fire, restored.Get("weapon_a#1").Element);
            Assert.AreEqual(1, restored.Get("weapon_a#1").Tier);
            Assert.AreEqual(TemperingElement.Void, restored.Get("weapon_b#2").Element);
            Assert.AreEqual(2, restored.Get("weapon_b#2").Tier);
            Assert.AreEqual("FireEdge", restored.GetEdgeTag("weapon_a#1"));
            Assert.AreEqual("VoidEdge", restored.GetEdgeTag("weapon_b#2"));
        }

        [Test]
        public void CA4_LegacySave_LoadsWithoutInfusion_NoMigration()
        {
            // Save antigo: campo Infusions ausente/null.
            var equip = new EquipmentSaveData { Infusions = null };
            var reg = new WeaponInfusionRegistry();
            reg.Set("weapon_old#1", new WeaponInfusion(TemperingElement.Ice, 1)); // estado pré-load

            reg.RestoreFromSaveData(equip.Infusions); // null => limpa, sem exceção

            Assert.AreEqual(0, reg.Count, "Legacy save (null infusions) loads with zero infusions.");
            Assert.IsFalse(reg.HasInfusion("weapon_old#1"));
            Assert.IsNull(reg.GetEdgeTag("weapon_old#1"));
        }

        [Test]
        public void CA4_DefaultEquipmentSaveData_HasEmptyInfusions()
        {
            var equip = new EquipmentSaveData();
            Assert.IsNotNull(equip.Infusions, "Default Infusions must be a non-null empty list.");
            Assert.AreEqual(0, equip.Infusions.Count);
        }

        [Test]
        public void CA4_RestoreIgnoresInvalidEntries()
        {
            var data = new List<WeaponInfusionSaveData>
            {
                new WeaponInfusionSaveData { ItemInstanceId = "w1", InfusionElement = "fire", InfusionTier = 1 },
                new WeaponInfusionSaveData { ItemInstanceId = "w2", InfusionElement = "bogus", InfusionTier = 1 },
                new WeaponInfusionSaveData { ItemInstanceId = "w3", InfusionElement = "ice", InfusionTier = 9 },
                new WeaponInfusionSaveData { ItemInstanceId = "", InfusionElement = "void", InfusionTier = 2 },
            };

            var reg = new WeaponInfusionRegistry();
            reg.RestoreFromSaveData(data);

            Assert.AreEqual(1, reg.Count, "Only the valid entry (w1) is restored.");
            Assert.AreEqual(TemperingElement.Fire, reg.Get("w1").Element);
            Assert.IsFalse(reg.HasInfusion("w2"));
            Assert.IsFalse(reg.HasInfusion("w3"));
        }

        // ── CA-5: óleo sobrepõe têmpera temporariamente ─────────────────────────────────────────

        [Test]
        public void CA5_Oil_SuppressesTemper_ThenTemperReturns()
        {
            // Regra pura: óleo (coating) ativo SUPRIME a tag da têmpera; expirado, volta a têmpera.
            Assert.AreEqual("FrostOil",
                TemperingCanon.ResolveActiveEdgeTag(infusionTag: "FireEdge", coatingTag: "FrostOil"),
                "Active oil coating suppresses the tempering tag.");

            Assert.AreEqual("FireEdge",
                TemperingCanon.ResolveActiveEdgeTag(infusionTag: "FireEdge", coatingTag: null),
                "Expired oil (no coating) => tempering tag is active again.");

            Assert.IsNull(TemperingCanon.ResolveActiveEdgeTag(null, null),
                "No tempering and no oil => no edge tag.");
        }

        [Test]
        public void CA5_Registry_AppliesActiveCoatingProvider()
        {
            var reg = new WeaponInfusionRegistry();
            reg.Set("weapon_x#1", new WeaponInfusion(TemperingElement.Fire, 1));

            // Sem óleo: vale a têmpera.
            Assert.AreEqual("FireEdge", reg.GetEdgeTag("weapon_x#1"));

            // Óleo ativo na instância: suprime a têmpera pela duração.
            reg.ActiveCoatingTagProvider = id => id == "weapon_x#1" ? "FrostOil" : null;
            Assert.AreEqual("FrostOil", reg.GetEdgeTag("weapon_x#1"));

            // Óleo expira (provider volta a null): têmpera de novo.
            reg.ActiveCoatingTagProvider = id => null;
            Assert.AreEqual("FireEdge", reg.GetEdgeTag("weapon_x#1"));
        }

        // ── Anti-regressão: arma sem têmpera permanece neutra ───────────────────────────────────

        [Test]
        public void NoTemper_NoEdgeTag_AndMatchingUnaffected()
        {
            var reg = new WeaponInfusionRegistry();
            Assert.IsNull(reg.GetEdgeTag("weapon_clean#1"));
            Assert.IsFalse(reg.HasInfusion("weapon_clean#1"));

            // Sem tag de têmpera, matching contra perfil neutro = 1.0 (comportamento prévio).
            var profile = MakeProfile(new[] { new MaterialMultiplier { MaterialTag = "FireEdge", Multiplier = 1.5f } });
            Assert.AreEqual(1f,
                VulnerabilityMatcher.GetDamageMultiplier(profile, DamageType.Physical, null), 0.0001f);
            Object.DestroyImmediate(profile);
        }

        // ── Mapeamento canônico completo ────────────────────────────────────────────────────────

        [Test]
        public void Canon_ElementMappings_AreStable()
        {
            Assert.AreEqual("FireEdge", TemperingCanon.EdgeTag(TemperingElement.Fire));
            Assert.AreEqual("FrostEdge", TemperingCanon.EdgeTag(TemperingElement.Ice));
            Assert.AreEqual("PoisonEdge", TemperingCanon.EdgeTag(TemperingElement.Toxic));
            Assert.AreEqual("ShockEdge", TemperingCanon.EdgeTag(TemperingElement.Lightning));
            Assert.AreEqual("ArcaneEdge", TemperingCanon.EdgeTag(TemperingElement.Arcane));
            Assert.AreEqual("VoidEdge", TemperingCanon.EdgeTag(TemperingElement.Void));

            Assert.AreEqual("item_essence_fire", TemperingCanon.EssenceItemId(TemperingElement.Fire));
            Assert.AreEqual("item_essence_void", TemperingCanon.EssenceItemId(TemperingElement.Void));

            Assert.AreEqual(0.10f, TemperingCanon.StatusChanceForTier(1), 0.0001f);
            Assert.AreEqual(0.20f, TemperingCanon.StatusChanceForTier(2), 0.0001f);

            Assert.AreEqual(TemperingElement.Lightning, TemperingCanon.ParseElement("LIGHTNING"));
            Assert.AreEqual(TemperingElement.None, TemperingCanon.ParseElement("nope"));
        }

        [Test]
        public void VoidT2_ConsumesSevenVoidEssences_WhenElementIsVoid()
        {
            // Caso de borda: têmpera VOID T2 = 6 (elemento) + 1 (catalisador) do MESMO item void.
            var (svc, reg, inv, wallet, _) = MakeService();
            inv.Items["item_essence_void"] = 7;
            wallet.Gold = 600;

            var result = svc.ApplyTempering(Weapon, TemperingElement.Void, 2, true);

            Assert.IsTrue(result.Success, result.FailReason);
            Assert.AreEqual(0, inv.Items["item_essence_void"], "Void T2 consumes 6+1 = 7 void essences.");
            Assert.AreEqual(TemperingElement.Void, reg.Get(Weapon).Element);
        }
    }
}
