using NUnit.Framework;
using CindarsHope.City.Services;
using CindarsHope.Editor.SceneCreation;

namespace CindarsHope.Tests.EditMode.City
{
    [TestFixture]
    public class TownShopDoorHoursPolicyTests
    {
        [Test]
        public void AllClassifiedTownShops_HaveOneCanonicalProviderMapping()
        {
            Assert.IsTrue(TownShopDoorHoursWiring.Validate(out var reason), reason);
        }

        [TestCase("npc_mella", 9)]
        [TestCase("npc_mella", 21)]
        [TestCase("npc_renko", 18)]
        public void DayShop_IsOpen_WhenItsCanonicalScheduleIsAvailable(string providerNpcId, int hour)
        {
            Assert.IsTrue(TownShopDoorHoursPolicy.TownShopDoorHours.IsOpen(providerNpcId, hour, out var reason));
            Assert.IsNull(reason);
        }

        [TestCase("npc_mella", 8)]
        [TestCase("npc_mella", 22)]
        [TestCase("npc_renko", 3)]
        public void DayShop_IsClosed_OutsideItsCanonicalSchedule(string providerNpcId, int hour)
        {
            Assert.IsFalse(TownShopDoorHoursPolicy.TownShopDoorHours.IsOpen(providerNpcId, hour, out var reason));
            Assert.AreEqual("Loja fechada. Horário: 09:00–22:00.", reason);
        }

        [TestCase("npc_yael", 20)]
        [TestCase("npc_yael", 1)]
        [TestCase("npc_maelor", 21)]
        [TestCase("npc_maelor", 0)]
        public void NightProviders_AreOpen_OnlyInTheirExistingNightWindow(string providerNpcId, int hour)
        {
            Assert.IsTrue(TownShopDoorHoursPolicy.TownShopDoorHours.IsOpen(providerNpcId, hour, out var reason));
            Assert.IsNull(reason);
        }

        [TestCase("npc_yael", 14)]
        [TestCase("npc_maelor", 8)]
        public void NightProviders_AreClosed_DuringDay(string providerNpcId, int hour)
        {
            Assert.IsFalse(TownShopDoorHoursPolicy.TownShopDoorHours.IsOpen(providerNpcId, hour, out var reason));
            Assert.AreEqual("Loja fechada. Horário: 20:00–02:00.", reason);
        }
    }
}
