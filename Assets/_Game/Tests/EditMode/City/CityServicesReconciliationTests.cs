using NUnit.Framework;
using CindarsHope.City.Services;
using CindarsHope.NPC.Schedule;
using CindarsHope.Quests.Flags;
using CindarsHope.Farm.Shipping;

namespace CindarsHope.Tests.EditMode.City
{
    /// <summary>
    /// fable_19 — testes determinísticos da reconciliação de schedule + serviços/licenças vivos:
    /// crosswalk de período sem perda semântica (CA-1), posse/efeito das 2 licenças, recusa de venda
    /// urbana sem licença e +5% no shipping com contrato (CA-2), e idempotência da posse (CA-3).
    /// Pure C# (sem cena). O estado estático de CityServiceAccess é isolado por TearDown.
    /// </summary>
    [TestFixture]
    public class CityServicesReconciliationTests
    {
        [TearDown]
        public void TearDown()
        {
            CityServiceAccess.ResetForTests();
        }

        // ─── CA-1: crosswalk City/Schedule (period) → NPC/Schedule (block) sem perda ──────────────

        [Test]
        public void Crosswalk_FromHour_MatchesObsoleteCityPeriodWindows()
        {
            // Paridade 1:1 com as janelas canônicas (city_rules.md Rule 6).
            Assert.AreEqual(NpcSchedulePeriod.SleepLateNight, NpcSchedulePeriodHelper.FromHour(3));
            Assert.AreEqual(NpcSchedulePeriod.Morning, NpcSchedulePeriodHelper.FromHour(7));
            Assert.AreEqual(NpcSchedulePeriod.WorkStart, NpcSchedulePeriodHelper.FromHour(10));
            Assert.AreEqual(NpcSchedulePeriod.Midday, NpcSchedulePeriodHelper.FromHour(13));
            Assert.AreEqual(NpcSchedulePeriod.WorkAfternoon, NpcSchedulePeriodHelper.FromHour(15));
            Assert.AreEqual(NpcSchedulePeriod.Evening, NpcSchedulePeriodHelper.FromHour(19));
            Assert.AreEqual(NpcSchedulePeriod.Night, NpcSchedulePeriodHelper.FromHour(22));
        }

        [Test]
        public void Crosswalk_EveryPeriod_MapsToACoarseBlock_NoOrphan()
        {
            foreach (NpcSchedulePeriod period in System.Enum.GetValues(typeof(NpcSchedulePeriod)))
            {
                var block = NpcSchedulePeriodHelper.ToTimeBlock(period);
                // Nenhum período colapsa em Default (perda semântica): todos têm bloco nomeado.
                Assert.AreNotEqual(NpcTimeBlock.Default, block,
                    $"Período {period} não mapeou para um bloco nomeado (perda semântica).");
            }
        }

        [Test]
        public void Crosswalk_HomePeriods_ResolveToNightBlock_BedToHomeAnchor()
        {
            // "BedDefinition → âncora home" absorvido: madrugada/noite tardia => bloco Night (home).
            Assert.AreEqual(NpcTimeBlock.Night, NpcSchedulePeriodHelper.BlockFromHour(2));
            Assert.IsTrue(NpcSchedulePeriodHelper.IsHomePeriod(NpcSchedulePeriod.SleepLateNight));
            Assert.IsTrue(NpcSchedulePeriodHelper.IsHomePeriod(NpcSchedulePeriod.Night));
            Assert.IsFalse(NpcSchedulePeriodHelper.IsHomePeriod(NpcSchedulePeriod.WorkStart));
        }

        [Test]
        public void Crosswalk_WorkPeriods_MapToWorkBlocks()
        {
            Assert.AreEqual(NpcTimeBlock.Morning, NpcSchedulePeriodHelper.ToTimeBlock(NpcSchedulePeriod.Morning));
            Assert.AreEqual(NpcTimeBlock.Morning, NpcSchedulePeriodHelper.ToTimeBlock(NpcSchedulePeriod.WorkStart));
            Assert.AreEqual(NpcTimeBlock.Midday, NpcSchedulePeriodHelper.ToTimeBlock(NpcSchedulePeriod.Midday));
            Assert.AreEqual(NpcTimeBlock.Midday, NpcSchedulePeriodHelper.ToTimeBlock(NpcSchedulePeriod.WorkAfternoon));
            Assert.AreEqual(NpcTimeBlock.Evening, NpcSchedulePeriodHelper.ToTimeBlock(NpcSchedulePeriod.Evening));
        }

        // ─── Catálogo: identidade Tovin/Mara e mapeamento de serviço ──────────────────────────────

        [Test]
        public void Catalog_TovinSellsMarketStall_MaraIssuesFarmRegistry()
        {
            Assert.IsTrue(CityServiceCatalog.IsServiceProvider("npc_tovin"));
            Assert.IsTrue(CityServiceCatalog.IsServiceProvider("npc_mara"));
            Assert.IsFalse(CityServiceCatalog.IsServiceProvider("npc_renko"));

            Assert.AreEqual(CityServiceFlags.LicenseMarketStallId, CityServiceCatalog.ServiceIdFor("npc_tovin"));
            Assert.AreEqual(CityServiceFlags.ContractFarmRegistryId, CityServiceCatalog.ServiceIdFor("npc_mara"));
        }

        [Test]
        public void Catalog_CostsAndFlags_MatchCityRules()
        {
            Assert.AreEqual(200, CityServiceCatalog.CostFor(CityServiceFlags.LicenseMarketStallId));
            Assert.AreEqual(100, CityServiceCatalog.CostFor(CityServiceFlags.ContractFarmRegistryId));
            Assert.AreEqual(CityServiceFlags.LicenseMarketStallFlag,
                CityServiceCatalog.PossessionFlagFor(CityServiceFlags.LicenseMarketStallId));
            Assert.AreEqual(CityServiceFlags.ContractFarmRegistryFlag,
                CityServiceCatalog.PossessionFlagFor(CityServiceFlags.ContractFarmRegistryId));
        }

        // ─── CA-2: gate de venda urbana sem licença / com licença ─────────────────────────────────

        [Test]
        public void UrbanSell_WithoutLicense_Refused()
        {
            // Fail-closed: sem resolver ligado, posse = false.
            Assert.IsFalse(CityServiceAccess.UrbanSellAllowed());
        }

        [Test]
        public void UrbanSell_WithLicense_Allowed()
        {
            var owned = new System.Collections.Generic.HashSet<string> { CityServiceFlags.LicenseMarketStallFlag };
            CityServiceAccess.OwnsServiceQuery = flag => owned.Contains(flag);

            Assert.IsTrue(CityServiceAccess.UrbanSellAllowed());
        }

        // ─── CA-2: efeito do contrato no shipping (+5%) ────────────────────────────────────────────

        [Test]
        public void FarmShipping_WithoutContract_PriceUnchanged()
        {
            Assert.AreEqual(100f, CityServiceAccess.ApplyFarmRegistryContract(100f), 0.001f);
        }

        [Test]
        public void FarmShipping_WithContract_PlusFivePercent()
        {
            var owned = new System.Collections.Generic.HashSet<string> { CityServiceFlags.ContractFarmRegistryFlag };
            CityServiceAccess.OwnsServiceQuery = flag => owned.Contains(flag);

            Assert.AreEqual(105f, CityServiceAccess.ApplyFarmRegistryContract(100f), 0.001f);
        }

        [Test]
        public void FarmShipping_ContractDoesNotAffectUrbanLicense_AndViceVersa()
        {
            // Posse de contrato não habilita venda urbana; licença não dá bônus de shipping.
            var owned = new System.Collections.Generic.HashSet<string> { CityServiceFlags.ContractFarmRegistryFlag };
            CityServiceAccess.OwnsServiceQuery = flag => owned.Contains(flag);

            Assert.IsFalse(CityServiceAccess.UrbanSellAllowed());
            Assert.AreEqual(105f, CityServiceAccess.ApplyFarmRegistryContract(100f), 0.001f);

            owned.Clear();
            owned.Add(CityServiceFlags.LicenseMarketStallFlag);
            Assert.IsTrue(CityServiceAccess.UrbanSellAllowed());
            Assert.AreEqual(100f, CityServiceAccess.ApplyFarmRegistryContract(100f), 0.001f);
        }

        [Test]
        public void ShippingPriceResolver_AppliesContractAtSinglePoint()
        {
            var owned = new System.Collections.Generic.HashSet<string> { CityServiceFlags.ContractFarmRegistryFlag };
            CityServiceAccess.OwnsServiceQuery = flag => owned.Contains(flag);

            var resolver = new ShippingPriceResolver();
            var input = new ShippingPriceInput { ItemId = "item_crop", BaseValue = 100f, ChannelMultiplier = 1f };
            var withContract = resolver.Resolve(input);

            owned.Clear();
            var withoutContract = resolver.Resolve(input);

            // O contrato deixa o preço ~5% maior no ponto único do resolver.
            Assert.Greater(withContract, withoutContract);
            Assert.AreEqual(withoutContract * 1.05f, withContract, 0.01f);
        }

        // ─── CA-3: compra idempotente + persistência por flag (resolver puro) ──────────────────────

        [Test]
        public void Purchase_InsufficientGold_DoesNothing()
        {
            var resolver = new CityServicePurchaseResolver();
            bool spent = false;
            string granted = null;

            var result = resolver.Resolve(
                CityServiceFlags.LicenseMarketStallId,
                alreadyOwns: false,
                currentGold: 50, // < 200
                spendGold: c => { spent = true; return true; },
                grantFlag: f => granted = f);

            Assert.AreEqual(CityServicePurchaseOutcome.InsufficientGold, result.Outcome);
            Assert.IsFalse(spent, "Não deve debitar ouro sem saldo.");
            Assert.IsNull(granted, "Não deve conceder a flag sem saldo.");
        }

        [Test]
        public void Purchase_EnoughGold_GrantsFlag_AndSpends()
        {
            var resolver = new CityServicePurchaseResolver();
            int spentAmount = 0;
            string granted = null;

            var result = resolver.Resolve(
                CityServiceFlags.LicenseMarketStallId,
                alreadyOwns: false,
                currentGold: 300,
                spendGold: c => { spentAmount = c; return true; },
                grantFlag: f => granted = f);

            Assert.AreEqual(CityServicePurchaseOutcome.Purchased, result.Outcome);
            Assert.AreEqual(200, spentAmount);
            Assert.AreEqual(200, result.GoldSpent);
            Assert.AreEqual(CityServiceFlags.LicenseMarketStallFlag, granted);
        }

        [Test]
        public void Purchase_AlreadyOwned_IsIdempotent_NoSpend()
        {
            var resolver = new CityServicePurchaseResolver();
            bool spent = false;

            var result = resolver.Resolve(
                CityServiceFlags.ContractFarmRegistryId,
                alreadyOwns: true,
                currentGold: 1000,
                spendGold: c => { spent = true; return true; },
                grantFlag: f => { });

            Assert.AreEqual(CityServicePurchaseOutcome.AlreadyOwned, result.Outcome);
            Assert.AreEqual(0, result.GoldSpent);
            Assert.IsFalse(spent, "Compra repetida não deve debitar ouro (idempotente).");
        }

        [Test]
        public void Purchase_UnknownService_ReportsUnknown()
        {
            var resolver = new CityServicePurchaseResolver();
            var result = resolver.Resolve("service_does_not_exist", false, 1000, c => true, f => { });
            Assert.AreEqual(CityServicePurchaseOutcome.UnknownService, result.Outcome);
        }

        [Test]
        public void Purchase_SpendRejected_LeavesStateUnchanged()
        {
            var resolver = new CityServicePurchaseResolver();
            string granted = null;
            var result = resolver.Resolve(
                CityServiceFlags.LicenseMarketStallId,
                alreadyOwns: false,
                currentGold: 1000,
                spendGold: c => false, // débito recusado pelo provedor
                grantFlag: f => granted = f);

            Assert.AreEqual(CityServicePurchaseOutcome.Failed, result.Outcome);
            Assert.IsNull(granted, "Flag não pode ser concedida se o débito falhou.");
        }

        // ─── CA-3: posse por flag persiste via QuestFlagService (integração) ──────────────────────

        [Test]
        public void Possession_PersistsViaQuestFlagService_RoundTripStyle()
        {
            // Simula o runtime bridge: flags registradas + setter autorizado.
            var registry = new QuestFlagRegistry();
            CityServiceFlags.RegisterFlags(registry);
            var flagService = new QuestFlagService(registry);

            CityServiceAccess.OwnsServiceQuery = flag => flagService.IsSet(flag);
            CityServiceAccess.GrantPossessionAction = flag => flagService.GrantFlag(flag, CityServiceFlags.CityServiceSetter);
            CityServiceAccess.CurrentGoldFunc = () => 1000;
            CityServiceAccess.SpendGoldFunc = c => true;

            Assert.IsFalse(CityServiceAccess.UrbanSellAllowed());

            var first = CityServiceAccess.TryPurchase(CityServiceFlags.LicenseMarketStallId);
            Assert.AreEqual(CityServicePurchaseOutcome.Purchased, first.Outcome);
            Assert.IsTrue(CityServiceAccess.UrbanSellAllowed());

            // "Round-trip": o estado de posse persistido na flag service sobrevive; recriar a query
            // a partir da MESMA flag service (como faria o load) mantém a posse.
            CityServiceAccess.OwnsServiceQuery = flag => flagService.IsSet(flag);
            Assert.IsTrue(CityServiceAccess.UrbanSellAllowed());

            // Segunda compra é idempotente (já possui).
            var second = CityServiceAccess.TryPurchase(CityServiceFlags.LicenseMarketStallId);
            Assert.AreEqual(CityServicePurchaseOutcome.AlreadyOwned, second.Outcome);
        }

        [Test]
        public void Possession_UnauthorizedSetter_CannotGrantFlag()
        {
            var registry = new QuestFlagRegistry();
            CityServiceFlags.RegisterFlags(registry);
            var flagService = new QuestFlagService(registry);

            // Setter não autorizado não consegue setar a flag de civic service (AllowedSetters).
            var result = flagService.GrantFlag(CityServiceFlags.LicenseMarketStallFlag, "some_other_system");
            Assert.IsFalse(result.Success);
            Assert.IsFalse(flagService.IsSet(CityServiceFlags.LicenseMarketStallFlag));
        }
    }
}
