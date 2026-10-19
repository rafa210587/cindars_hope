using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.City.Services;
using CindarsHope.City.Validation;
using CindarsHope.City.Layout;

namespace CindarsHope.Tests.EditMode.City
{
    [TestFixture]
    public class CityServiceAvailabilityTests
    {
        private CityServiceAvailabilityResolver _resolver;
        private CityServiceValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _resolver = new CityServiceAvailabilityResolver();
            _validator = new CityServiceValidator();
        }

        private CityServiceDefinition ShopService(string id = "shop_renko_general") => new CityServiceDefinition
        {
            ServiceId = id,
            ServiceType = CityServiceType.ShopGeneral,
            DisplayName = "Renko's Shop",
            ProviderNpcIds = new List<string> { "npc_renko" },
            ProviderBuildingId = "building_general_shop",
            ShopInventoryId = "inventory_renko",
            PriceChannel = "SellPoint"
        };

        private ServiceAccessContext DefaultCtx() => new ServiceAccessContext
        {
            CurrentHour = 10, PlayerReputation = 50, FarmLevel = 1, CaveProgress = 0,
            IsNpcAvailable = true, PlayerQuestFlags = new List<string>(), PlayerStoryFlags = new List<string>()
        };

        [Test]
        public void Service_Available_HappyPath()
        {
            var result = _resolver.Resolve(ShopService(), DefaultCtx());
            Assert.IsTrue(result.Available);
            Assert.AreEqual("npc_renko", result.ProviderNpcId);
        }

        [Test]
        public void Service_NpcUnavailable_NotAvailable()
        {
            var ctx = DefaultCtx();
            ctx.IsNpcAvailable = false;
            var result = _resolver.Resolve(ShopService(), ctx);
            Assert.IsFalse(result.Available);
        }

        [Test]
        public void Service_OpenHoursClosed_NotAvailable()
        {
            var service = ShopService();
            service.RequiredOpenHoursRule = new OpenHoursRule { OpenHour = 9, CloseHour = 17, ClosedMessage = "Closed now" };
            var ctx = DefaultCtx();
            ctx.CurrentHour = 20;
            var result = _resolver.Resolve(service, ctx);
            Assert.IsFalse(result.Available);
            Assert.AreEqual("Closed now", result.UnavailableReason);
        }

        [Test]
        public void Service_NightShop_RequiresNyx()
        {
            var service = ShopService("shop_yael_night");
            service.IsNightShop = true;
            var ctx = DefaultCtx();
            ctx.IsNight = false;
            ctx.ActiveLunarPhase = "Normal";
            var result = _resolver.Resolve(service, ctx);
            Assert.IsFalse(result.Available);

            ctx.ActiveLunarPhase = "Nyx";
            result = _resolver.Resolve(service, ctx);
            Assert.IsTrue(result.Available);
        }

        [Test]
        public void Service_NightShop_AvailableAtNight()
        {
            var service = ShopService("shop_yael_night");
            service.IsNightShop = true;
            var ctx = DefaultCtx();
            ctx.IsNight = true;
            var result = _resolver.Resolve(service, ctx);
            Assert.IsTrue(result.Available);
        }

        [Test]
        public void Service_AnyaTemple_AlwaysBlocked()
        {
            var service = ShopService("anya_temple_service");
            var result = _resolver.Resolve(service, DefaultCtx());
            Assert.IsFalse(result.Available);
            Assert.IsTrue(result.UnavailableReason.Contains("Anya"));
        }

        [Test]
        public void Service_ReputationGate_Insufficient()
        {
            var service = ShopService();
            service.RequiredReputation = 80;
            var ctx = DefaultCtx();
            ctx.PlayerReputation = 50;
            var result = _resolver.Resolve(service, ctx);
            Assert.IsFalse(result.Available);
        }

        [Test]
        public void Service_QuestFlag_Missing()
        {
            var service = ShopService();
            service.RequiredQuestFlag = "quest_brumdar_forge_unlocked";
            var result = _resolver.Resolve(service, DefaultCtx());
            Assert.IsFalse(result.Available);
            Assert.IsTrue(result.RequiredFlagsMissing.Contains("quest_brumdar_forge_unlocked"));
        }

        [Test]
        public void Service_FarmLevelGate()
        {
            var service = ShopService();
            service.RequiredFarmLevel = 3;
            var ctx = DefaultCtx();
            ctx.FarmLevel = 1;
            var result = _resolver.Resolve(service, ctx);
            Assert.IsFalse(result.Available);
        }

        [Test]
        public void Validator_ShopWithoutInventory_Warning()
        {
            var service = ShopService();
            service.ShopInventoryId = null;
            var issues = _validator.Validate(service);
            Assert.IsTrue(issues.Exists(i => i.Code == "SHOP_NO_INVENTORY"));
        }

        [Test]
        public void Validator_LicenseWithoutGrantFlag_IsBlocker()
        {
            var license = new LicenseDefinition { LicenseId = "license_carpentry", IssuerServiceId = "service_townhall" };
            var issues = _validator.ValidateLicense(license);
            Assert.IsTrue(issues.Exists(i => i.Code == "LICENSE_NO_GRANT_FLAG" && i.IsBlocker));
        }

        [Test]
        public void Validator_ValidLicense_NoBlockers()
        {
            var license = new LicenseDefinition
            {
                LicenseId = "license_carpentry",
                IssuerServiceId = "service_townhall",
                GrantedFlag = "flag_carpentry_license",
                GrantedPermission = "build_workshop"
            };
            var issues = _validator.ValidateLicense(license);
            Assert.IsFalse(issues.Exists(i => i.IsBlocker));
        }

        [Test]
        public void Contract_WithObjectiveAdapter_IsValid()
        {
            var contract = new ContractDefinition
            {
                ContractId = "contract_zrix_cave_map",
                ProviderServiceId = "service_roads_guild",
                ObjectiveDefinitionId = "obj_explore_floor_5",
                RewardTableId = "reward_map_contract"
            };
            Assert.IsTrue(contract.HasObjectiveAdapter());
            Assert.IsTrue(contract.HasRewardTable());
        }

        [Test]
        public void Contract_WithoutObjectiveAdapter_NotValid()
        {
            var contract = new ContractDefinition { ContractId = "contract_broken" };
            Assert.IsFalse(contract.HasObjectiveAdapter());
        }
    }
}
