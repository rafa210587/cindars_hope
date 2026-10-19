using System.Collections;
using CindarsHope.Foundation;
using CindarsHope.Skills.Runtime;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.PlayMode.Composition
{
    public sealed class MaterialEyeHarvestPlayModeTests
    {
        [UnityTest]
        public IEnumerator RegisteredStateAndPolicy_ConsumeWinningAttemptWithCommittedHarvest()
        {
            var state = new CraftingPassiveRngState("playmode-save");
            ICommonHarvestItemPolicy policy = new ExplicitCommonHarvestItemPolicy(
                new[] { "item_crop_wheat" });
            DomainManagerRegistry.Register(state);
            DomainManagerRegistry.Register(policy);

            try
            {
                var consumer = new MaterialEyeHarvestConsumer(
                    DomainManagerRegistry.Get<ICommonHarvestItemPolicy>());
                Assert.That(consumer.TryPrepare(
                    DomainManagerRegistry.Get<CraftingPassiveRngState>(),
                    1f,
                    "plot-01",
                    new[] { "item_crop_wheat" },
                    out var prepared), Is.True);

                Assert.That(consumer.Commit(state, prepared, true, false), Is.True);
                Assert.That(state.GetNextAttempt(MaterialEyeHarvestConsumer.OperationKind, "plot-01"), Is.EqualTo(1));
                Assert.That(consumer.Commit(state, prepared, true, true), Is.False,
                    "The same prepared winner cannot be committed twice.");
            }
            finally
            {
                DomainManagerRegistry.Unregister(state);
                DomainManagerRegistry.Unregister(policy);
            }

            yield return null;
        }
    }
}
