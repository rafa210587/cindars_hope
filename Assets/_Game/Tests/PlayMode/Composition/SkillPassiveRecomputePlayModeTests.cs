using System.Collections;
using System.Linq;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.Player;
using CindarsHope.Player.Movement;
using CindarsHope.Player.Progression;
using CindarsHope.Skills;
using CindarsHope.Skills.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.PlayMode.Composition
{
    public sealed class SkillPassiveRecomputePlayModeTests
    {
        private SkillTreeManager _skills;
        private PlayerProgressionManager _progression;
        private SkillTreeSaveData _originalSkills;
        private PlayerProgressionSaveData _originalProgression;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            if (GameBootstrap.Instance == null || SkillTreeManager.Instance == null || PlayerController.ActiveInstance == null)
            {
                SceneManager.LoadScene("FarmScene", LoadSceneMode.Single);
                yield return null;
            }

            // The PlayMode runner can preserve a DontDestroyOnLoad composition root between
            // fixture cases; Start() is then not replayed after FarmScene reload. Exercise the
            // same idempotent installer used by the root so this fixture remains order-independent.
            if (PlayerVitalsApplier.Instance == null && GameBootstrap.Instance != null)
            {
                PlayerVitalsApplierBootstrap.Install(GameBootstrap.Instance.transform);
                yield return null;
            }

            _skills = SkillTreeManager.Instance;
            _progression = PlayerProgressionManager.Instance;
            Assert.That(_skills, Is.Not.Null);
            Assert.That(_progression, Is.Not.Null);
            _originalSkills = _skills.CaptureSaveData();
            _originalProgression = _progression.CaptureSaveData();

            var points = _progression.CaptureSaveData();
            points.UnspentSkillPoints = 20;
            _progression.RestoreFromSaveData(points);
            _skills.RestoreFromSaveData(new SkillTreeSaveData(), 100);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            _progression?.RestoreFromSaveData(_originalProgression);
            _skills?.RestoreFromSaveData(_originalSkills, 100);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PurchaseRankRespec_ReplacesAggregatesWithoutDuplication()
        {
            int recomputes = 0;
            System.Action<SkillDerivedStatsChangedEvent> handler = _ => recomputes++;
            GameEventBus.Subscribe(handler);
            try
            {
                Assert.That(_skills.TryPurchaseNode("magic_mana_well", 100), Is.True);
                Assert.That(Total(SkillModifierType.MaxManaFlat, "magic_mana_well"), Is.EqualTo(10f));
                Assert.That(_skills.GetAllActivePassiveModifiers().Count(modifier =>
                    modifier.SourceNodeId == "magic_mana_well"), Is.EqualTo(1));

                Assert.That(_skills.TryRankUpNode("magic_mana_well", out _), Is.True);
                Assert.That(Total(SkillModifierType.MaxManaFlat, "magic_mana_well"), Is.EqualTo(20f));
                Assert.That(_skills.GetAllActivePassiveModifiers().Count(modifier =>
                    modifier.SourceNodeId == "magic_mana_well"), Is.EqualTo(1));

                int gold = 0;
                Assert.That(_skills.TryRespec(ref gold, 100), Is.True);
                Assert.That(Total(SkillModifierType.MaxManaFlat, "magic_mana_well"), Is.Zero);
                Assert.That(recomputes, Is.EqualTo(3));
            }
            finally
            {
                GameEventBus.Unsubscribe(handler);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator RepeatedLoad_RecomputesProviderFromCurrentRankWithoutAccumulating()
        {
            Assert.That(PlayerVitalsApplier.Instance, Is.Not.Null,
                "The gameplay composition must install the passive consumer.");
            var save = new SkillTreeSaveData();
            save.NodeRanks.Add(new SkillNodeRankEntry("magic_quick_channel", 3)
            {
                SpentPoints = 3
            });

            _skills.RestoreFromSaveData(save, 100);
            float first = PassiveSurvivalModifierProvider.ResolveManaBaseRegen(2f);
            int firstCount = _skills.GetAllActivePassiveModifiers().Count(modifier =>
                modifier.SourceNodeId == "magic_quick_channel" &&
                modifier.ModifierType == SkillModifierType.ManaRegenBasePercent);

            _skills.RestoreFromSaveData(save, 100);
            float second = PassiveSurvivalModifierProvider.ResolveManaBaseRegen(2f);
            int secondCount = _skills.GetAllActivePassiveModifiers().Count(modifier =>
                modifier.SourceNodeId == "magic_quick_channel" &&
                modifier.ModifierType == SkillModifierType.ManaRegenBasePercent);

            Assert.That(first, Is.EqualTo(.48f).Within(.0001f));
            Assert.That(second, Is.EqualTo(first).Within(.0001f));
            Assert.That(firstCount, Is.EqualTo(1));
            Assert.That(secondCount, Is.EqualTo(1));
            yield return null;
        }

        [UnityTest]
        public IEnumerator TerrainKitingAndStatus_FactorsCoexistAndClearIndependently()
        {
            var composer = new PlayerSpeedComposer();
            composer.SetFactor(SpeedFactorKind.Status, .60f);
            composer.SetFactor(SpeedFactorKind.Terrain, .70f);
            composer.SetFactor(SpeedFactorKind.Kiting, 1.08f);

            Assert.That(composer.GetFactor(SpeedFactorKind.Status), Is.EqualTo(.60f));
            Assert.That(composer.GetFactor(SpeedFactorKind.Terrain), Is.EqualTo(.70f));
            Assert.That(composer.GetFactor(SpeedFactorKind.Kiting), Is.EqualTo(1.08f));
            Assert.That(composer.Value, Is.EqualTo(.4536f).Within(.0001f));

            composer.ClearFactor(SpeedFactorKind.Terrain);
            Assert.That(composer.GetFactor(SpeedFactorKind.Status), Is.EqualTo(.60f));
            Assert.That(composer.GetFactor(SpeedFactorKind.Kiting), Is.EqualTo(1.08f));
            Assert.That(composer.Value, Is.EqualTo(.648f).Within(.0001f));
            yield return null;
        }

        [UnityTest]
        public IEnumerator KitingWindow_ExpiresAndMeleeCommitCancelsDeterministically()
        {
            var player = PlayerController.ActiveInstance;
            Assert.That(player, Is.Not.Null, "Farm composition must expose the active player.");

            float kitingBonus = .08f;
            var host = new GameObject("Test_RangedKitingRuntime");
            var runtime = host.AddComponent<RangedKitingRuntime>();
            var threat = new RangedKitingRuntime.ThreatSnapshot("enemy-test", player.transform.position + Vector3.right);
            runtime.Configure(
                player,
                () => kitingBonus,
                _ => threat,
                id => id == threat.EnemyInstanceId ? threat : (RangedKitingRuntime.ThreatSnapshot?)null);

            try
            {
                GameEventBus.Publish(new PlayerBowShootEvent(Vector2.right, .2f));
                Assert.That(runtime.IsActive, Is.True);

                runtime.Tick(Time.time + RangedKitingRuntime.WindowSeconds + .01f);
                Assert.That(runtime.IsActive, Is.False, "the fixed two-second window must expire on an injected tick");
                Assert.That(player.SpeedComposer.GetFactor(SpeedFactorKind.Kiting), Is.EqualTo(1f));

                GameEventBus.Publish(new PlayerBowShootEvent(Vector2.right, .2f));
                Assert.That(runtime.IsActive, Is.True);
                GameEventBus.Publish(new PlayerOffensiveActionCommittedEvent("attack-test", "Melee"));
                Assert.That(runtime.IsActive, Is.False);

                GameEventBus.Publish(new PlayerBowShootEvent(Vector2.right, .2f));
                Assert.That(runtime.IsActive, Is.True);
                kitingBonus = 0f;
                GameEventBus.Publish(new SkillDerivedStatsChangedEvent());
                Assert.That(runtime.IsActive, Is.False);
            }
            finally
            {
                Object.Destroy(host);
            }

            yield return null;
        }

        private float Total(SkillModifierType type, string sourceNodeId)
            => _skills.GetAllActivePassiveModifiers()
                .Where(modifier => modifier.ModifierType == type &&
                    modifier.SourceNodeId == sourceNodeId)
                .Sum(modifier => modifier.Value);
    }
}
