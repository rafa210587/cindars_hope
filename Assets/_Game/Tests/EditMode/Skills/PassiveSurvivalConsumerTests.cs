using CindarsHope.Combat.StatusEffect;
using CindarsHope.Core;
using CindarsHope.Foundation;
using CindarsHope.Player;
using CindarsHope.Player.Movement;
using CindarsHope.Save;
using CindarsHope.Save.Providers;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Skills
{
    public sealed class PassiveSurvivalConsumerTests
    {
        [TearDown]
        public void TearDown()
        {
            PassiveSurvivalModifierProvider.Reset();
            ResistanceProvider.Source = null;
            DodgeCostModifierProvider.TrainingReductionSource = null;
            PlayerVitalsApplier.CraftTimeReductionSource = null;
            RepairEfficiencyProvider.EffectiveRepairAmountSource = null;
            GameEventBus.Clear<PlayerResistancesChangedEvent>();
        }

        [Test]
        public void QuickChannel_ScalesOnlyBaseManaRegeneration()
        {
            PassiveSurvivalModifierProvider.ManaBaseRegenBonusFractionSource = () => .24f;
            Assert.That(PassiveSurvivalModifierProvider.ResolveManaBaseRegen(2f),
                Is.EqualTo(.48f).Within(.0001f));

            float unrelatedExternalBonus = 3f;
            Assert.That(2f + unrelatedExternalBonus +
                PassiveSurvivalModifierProvider.ResolveManaBaseRegen(2f),
                Is.EqualTo(5.48f).Within(.0001f));
        }

        [Test]
        public void RationsAndContext_ComposeMultiplicativelyWithFinalFloor()
        {
            Assert.That(HungerManager.ResolveFinalDrainMultiplier(.70f, .50f),
                Is.EqualTo(.35f).Within(.0001f));
            Assert.That(HungerManager.ResolveFinalDrainMultiplier(.10f, .50f),
                Is.EqualTo(.25f).Within(.0001f));
        }

        [Test]
        public void PlayerSection_RoundTripPreservesFractionalHungerDrain()
        {
            var sourceHost = new UnityEngine.GameObject("hunger-save-source");
            var targetHost = new UnityEngine.GameObject("hunger-save-target");
            sourceHost.SetActive(false);
            targetHost.SetActive(false);
            try
            {
                var sourcePlayer = sourceHost.AddComponent<PlayerManager>();
                sourcePlayer.RestoreState(100, 87, 12);
                var sourceHunger = sourceHost.AddComponent<HungerManager>();
                sourceHunger.RestoreFromSaveData(73, 100, .625f);
                var source = new PlayerSectionProvider(
                    sourcePlayer, sourceHunger, null, () => UnityEngine.Vector2.zero);

                var captured = source.Capture(null) as PlayerSaveData;

                Assert.That(captured, Is.Not.Null);
                Assert.That(captured.HungerFractionalDrainAccumulator,
                    Is.EqualTo(.625f).Within(.0001f));

                var targetHunger = targetHost.AddComponent<HungerManager>();
                var target = new PlayerSectionProvider(
                    null, targetHunger, null, () => UnityEngine.Vector2.zero);
                target.Restore(captured);

                Assert.That(targetHunger.CurrentHunger, Is.EqualTo(73));
                Assert.That(targetHunger.MaxHunger, Is.EqualTo(100));
                Assert.That(targetHunger.FractionalDrainAccumulator,
                    Is.EqualTo(.625f).Within(.0001f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(sourceHost);
                UnityEngine.Object.DestroyImmediate(targetHost);
            }
        }

        [Test]
        public void LegacyPlayerSection_MissingFractionDefaultsToZero()
        {
            var legacy = UnityEngine.JsonUtility.FromJson<PlayerSaveData>(
                "{\"CurrentHunger\":55,\"MaxHunger\":100}");

            Assert.That(legacy.HungerFractionalDrainAccumulator, Is.Zero);
            Assert.That(HungerManager.NormalizeFractionalDrainAccumulator(
                legacy.HungerFractionalDrainAccumulator), Is.Zero);
        }

        [TestCase(float.NaN, 0f)]
        [TestCase(float.PositiveInfinity, 0f)]
        [TestCase(-.2f, 0f)]
        [TestCase(1.75f, .75f)]
        public void FractionalAccumulator_InvalidOrWholePartIsNormalized(float input, float expected)
        {
            Assert.That(HungerManager.NormalizeFractionalDrainAccumulator(input),
                Is.EqualTo(expected).Within(.0001f));
        }

        [Test]
        public void Recovery_AppliesAfterResistanceOnlyToPoisonBurnAndSlow()
        {
            PassiveSurvivalModifierProvider.StatusRecoveryReductionSource = () => .30f;
            Assert.That(PassiveSurvivalModifierProvider.ResolveStatusDuration(
                8f, PlayerStatusReceiver.RecoveryFamilyFor(StatusEffectType.Poison)),
                Is.EqualTo(5.6f).Within(.0001f));
            Assert.That(PassiveSurvivalModifierProvider.ResolveStatusDuration(
                8f, PlayerStatusReceiver.RecoveryFamilyFor(StatusEffectType.Burn)),
                Is.EqualTo(5.6f).Within(.0001f));
            Assert.That(PassiveSurvivalModifierProvider.ResolveStatusDuration(
                8f, PlayerStatusReceiver.RecoveryFamilyFor(StatusEffectType.Slow)),
                Is.EqualTo(5.6f).Within(.0001f));
            Assert.That(PassiveSurvivalModifierProvider.ResolveStatusDuration(
                8f, PlayerStatusReceiver.RecoveryFamilyFor(StatusEffectType.Bleed)),
                Is.EqualTo(8f));
        }

        [Test]
        public void StatusDuration_ClampsOnlyAfterResistanceAndRecovery()
        {
            Assert.That(DerivedFollowupFormulas.ApplyStatusDurationModifiers(
                    50f, 10, .30f, 1f, 30f),
                Is.EqualTo(28f).Within(.0001f));
            Assert.That(DerivedFollowupFormulas.ApplyStatusDurationModifiers(
                    100f, 0, 0f, 1f, 30f),
                Is.EqualTo(30f).Within(.0001f));
        }

        [Test]
        public void ResistanceSnapshot_ExposesValuesAndEffectiveDurationReduction()
        {
            var stats = new DerivedStatsCalculator.DerivedStats
            {
                ToxicResistance = 5,
                ColdResistance = 10,
                HeatResistance = 30
            };
            var snapshot = PlayerVitalsApplier.CreateResistanceSnapshot(stats);
            PlayerResistancesChangedEvent received = default;
            GameEventBus.Subscribe<PlayerResistancesChangedEvent>(evt => received = evt);

            GameEventBus.Publish(snapshot);

            Assert.That(received.ToxicResistance, Is.EqualTo(5));
            Assert.That(received.ColdResistance, Is.EqualTo(10));
            Assert.That(received.HeatResistance, Is.EqualTo(30));
            Assert.That(received.ToxicDurationReduction, Is.EqualTo(.10f).Within(.0001f));
            Assert.That(received.ColdDurationReduction, Is.EqualTo(.20f).Within(.0001f));
            Assert.That(received.HeatDurationReduction, Is.EqualTo(.50f).Within(.0001f));
        }

        [Test]
        public void VitalsTeardown_ClearsOwnedSourcesButPreservesNewerOwner()
        {
            var host = new UnityEngine.GameObject("vitals-provider-owner");
            host.SetActive(false);
            try
            {
                var applier = host.AddComponent<PlayerVitalsApplier>();
                var flags = System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic;
                typeof(PlayerVitalsApplier).GetMethod("EnsureOwnedProviderDelegates", flags)
                    ?.Invoke(applier, null);

                var mana = (System.Func<float>)typeof(PlayerVitalsApplier)
                    .GetField("_ownedManaRegenSource", flags)?.GetValue(applier);
                var status = (System.Func<float>)typeof(PlayerVitalsApplier)
                    .GetField("_ownedStatusRecoverySource", flags)?.GetValue(applier);
                var terrain = (System.Func<float>)typeof(PlayerVitalsApplier)
                    .GetField("_ownedTerrainRecoverySource", flags)?.GetValue(applier);
                var resistance = (System.Func<DamageType, int>)typeof(PlayerVitalsApplier)
                    .GetField("_ownedResistanceSource", flags)?.GetValue(applier);
                var dodge = (System.Func<float>)typeof(PlayerVitalsApplier)
                    .GetField("_ownedDodgeTrainingSource", flags)?.GetValue(applier);
                var craft = (System.Func<float>)typeof(PlayerVitalsApplier)
                    .GetField("_ownedCraftTimeSource", flags)?.GetValue(applier);
                var repair = (System.Func<int, int>)typeof(PlayerVitalsApplier)
                    .GetField("_ownedRepairEfficiencySource", flags)?.GetValue(applier);

                Assert.That(mana, Is.Not.Null);
                Assert.That(status, Is.Not.Null);
                Assert.That(terrain, Is.Not.Null);
                Assert.That(resistance, Is.Not.Null);
                Assert.That(dodge, Is.Not.Null);
                Assert.That(craft, Is.Not.Null);
                Assert.That(repair, Is.Not.Null);

                PassiveSurvivalModifierProvider.ManaBaseRegenBonusFractionSource = mana;
                PassiveSurvivalModifierProvider.StatusRecoveryReductionSource = status;
                PassiveSurvivalModifierProvider.TerrainPenaltyRecoverySource = terrain;
                ResistanceProvider.Source = resistance;
                DodgeCostModifierProvider.TrainingReductionSource = dodge;
                PlayerVitalsApplier.CraftTimeReductionSource = craft;
                RepairEfficiencyProvider.EffectiveRepairAmountSource = repair;

                System.Func<DamageType, int> newerResistance = _ => 42;
                ResistanceProvider.Source = newerResistance;
                // Exercise the teardown contract directly; EditMode does not guarantee the full
                // MonoBehaviour destruction lifecycle for a component created on an inactive host.
                typeof(PlayerVitalsApplier).GetMethod("OnDestroy", flags)?.Invoke(applier, null);

                Assert.That(PassiveSurvivalModifierProvider.ManaBaseRegenBonusFractionSource, Is.Null);
                Assert.That(PassiveSurvivalModifierProvider.StatusRecoveryReductionSource, Is.Null);
                Assert.That(PassiveSurvivalModifierProvider.TerrainPenaltyRecoverySource, Is.Null);
                Assert.That(PlayerVitalsApplier.CraftTimeReductionSource, Is.Null);
                Assert.That(RepairEfficiencyProvider.EffectiveRepairAmountSource, Is.Null);
                Assert.That(DodgeCostModifierProvider.TrainingReductionSource, Is.Null);
                Assert.That(ResistanceProvider.Source, Is.SameAs(newerResistance));
            }
            finally
            {
                if (host != null)
                    UnityEngine.Object.DestroyImmediate(host);
            }
        }

        [Test]
        public void SafeStep_RecoversOnlyTerrainPenaltyAndPreservesStatusFactor()
        {
            PassiveSurvivalModifierProvider.TerrainPenaltyRecoverySource = () => .35f;
            float terrain = PassiveSurvivalModifierProvider.ResolveTerrainFactor(.45f);
            Assert.That(terrain, Is.EqualTo(.6425f).Within(.0001f));
            Assert.That(PassiveSurvivalModifierProvider.ResolveTerrainFactor(1f), Is.EqualTo(1f));

            var composer = new PlayerSpeedComposer();
            composer.SetFactor(SpeedFactorKind.Status, .60f);
            composer.SetFactor(SpeedFactorKind.Terrain, terrain);
            Assert.That(composer.GetFactor(SpeedFactorKind.Status), Is.EqualTo(.60f));
            Assert.That(composer.GetFactor(SpeedFactorKind.Terrain), Is.EqualTo(.6425f).Within(.0001f));
            Assert.That(composer.Value, Is.EqualTo(.3855f).Within(.0001f));
            composer.ClearFactor(SpeedFactorKind.Terrain);
            Assert.That(composer.Value, Is.EqualTo(.60f));
        }
    }
}
