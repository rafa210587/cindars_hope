using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Farm.Mining;

namespace CindarsHope.Tests.EditMode.Farm
{
    [TestFixture]
    public class RockMiningServiceTests
    {
        private Dictionary<string, RockDefinition> _defs;
        private RockMiningService _service;

        [SetUp]
        public void Setup()
        {
            _defs = new Dictionary<string, RockDefinition>
            {
                ["rock_small"] = new RockDefinition { RockId = "rock_small", Kind = RockKind.SmallRock, MaxHits = 2, RefreshAfterDays = 3, CanRefresh = true },
                ["rock_quarry"] = new RockDefinition { RockId = "rock_quarry", Kind = RockKind.QuarryFuture, MaxHits = 0 }
            };
            _service = new RockMiningService(_defs);
        }

        private RockInstanceState FreshRock() => new RockInstanceState
        {
            RockInstanceId = "rock_01",
            RockId = "rock_small",
            RemainingHits = 2,
            IsDepleted = false
        };

        [Test]
        public void Mine_Hit_ReducesHits()
        {
            var rock = FreshRock();
            var result = _service.Mine(rock, "Basic", 1);
            Assert.IsTrue(result.Success);
            Assert.IsFalse(result.RockDepleted);
            Assert.AreEqual(1, rock.RemainingHits);
        }

        [Test]
        public void Mine_FinalHit_Depletes()
        {
            var rock = FreshRock();
            rock.RemainingHits = 1;
            var result = _service.Mine(rock, "Basic", 5);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.RockDepleted);
            Assert.IsTrue(rock.IsDepleted);
        }

        [Test]
        public void Mine_AlreadyDepleted_Fails()
        {
            var rock = FreshRock();
            rock.IsDepleted = true;
            var result = _service.Mine(rock, "Basic", 1);
            Assert.IsFalse(result.Success);
            Assert.AreEqual("AlreadyDepleted", result.FailureReason);
        }

        [Test]
        public void Mine_QuarryReserved_Blocked()
        {
            var rock = new RockInstanceState { RockInstanceId = "q_01", RockId = "rock_quarry", RemainingHits = 99 };
            var result = _service.Mine(rock, "Basic", 1);
            Assert.IsFalse(result.Success);
            Assert.AreEqual("QuarryReservedFuture", result.FailureReason);
        }

        [Test]
        public void Refresh_RockRefreshes_AfterEligibleDay()
        {
            var rock = FreshRock();
            rock.RemainingHits = 1;
            _service.Mine(rock, "Basic", 3);
            Assert.IsTrue(rock.IsDepleted);
            _service.RefreshIfEligible(rock, 6);
            Assert.IsFalse(rock.IsDepleted);
            Assert.AreEqual(2, rock.RemainingHits);
        }

        [Test]
        public void Refresh_NotRefreshes_BeforeEligibleDay()
        {
            var rock = FreshRock();
            rock.RemainingHits = 1;
            _service.Mine(rock, "Basic", 3);
            _service.RefreshIfEligible(rock, 4);
            Assert.IsTrue(rock.IsDepleted);
        }
    }
}
