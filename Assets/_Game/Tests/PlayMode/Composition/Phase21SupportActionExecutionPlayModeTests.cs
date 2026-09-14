using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Craft;
using CindarsHope.Cave;
using CindarsHope.Cave.Runtime;
using CindarsHope.Combat;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.Interaction;
using CindarsHope.Player;
using CindarsHope.Player.Progression;
using CindarsHope.SceneManagement;
using CindarsHope.Skills;
using CindarsHope.Skills.Runtime;
using CindarsHope.Skills.Runtime.Effects;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.PlayMode.Composition
{
    public sealed class Phase21SupportActionExecutionPlayModeTests
    {
        private const string ActionId = "skill_crafting_marca_eficiencia";
        private const string NodeId = "crafting.marca_eficiencia";
        private readonly List<SkillActionResolvedEvent> _outcomes = new List<SkillActionResolvedEvent>();
        private SkillTreeManager _skills;
        private PlayerProgressionManager _progression;
        private StaminaManager _stamina;
        private PlayerController _player;
        private SkillTreeSaveData _originalSkills;
        private PlayerProgressionSaveData _originalProgression;
        private Vector3 _originalPosition;
        private int _originalStamina;
        private int _originalMaxStamina;
        private float _fractionalCreditBefore;
        private int _staminaAtCommit = -1;
        private SurvivalSkillSaveData _originalSurvival;
        private bool _farmCachePreexisting;
        private bool _caveCachePreexisting;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _farmCachePreexisting = FarmSceneRuntimeStateCache.HasCachedState;
            _caveCachePreexisting = CaveRunStateCache.Cached != null;
            if (GameBootstrap.Instance == null)
                LogAssert.Expect(LogType.Error, new Regex("^\\[DeathSystemBootstrap\\] GameBootstrap/PlayerManager/CorpseRecoveryManager nao ficaram prontos"));
            yield return SceneManager.LoadSceneAsync("FarmScene", LoadSceneMode.Single);
            yield return null;

            _skills = SkillTreeManager.Instance;
            _progression = PlayerProgressionManager.Instance;
            _stamina = GameBootstrap.Instance?.StaminaManager as StaminaManager;
            _player = PlayerController.ActiveInstance;
            Assert.That(_skills, Is.Not.Null);
            Assert.That(_progression, Is.Not.Null);
            Assert.That(_stamina, Is.Not.Null);
            Assert.That(_player, Is.Not.Null);
            Assert.That(ActiveSkillExecutionController.Instance, Is.Not.Null);
            Assert.That(EfficiencyMarkRuntimeCoordinator.Instance, Is.Not.Null);

            _originalSkills = _skills.CaptureSaveData();
            _originalProgression = _progression.CaptureSaveData();
            _originalPosition = _player.transform.position;
            _originalStamina = _stamina.CurrentStamina;
            _originalMaxStamina = _stamina.MaxStamina;
            _fractionalCreditBefore = EfficiencyMarkRuntimeCoordinator.Instance.FractionalCredit;
            _originalSurvival = SurvivalSkillRuntimeCoordinator.Instance?.State?.CaptureSaveData();

            var stage = _progression.CaptureSaveData();
            stage.TotalXp = ProgressionCurve.TotalXpForLevel(Phase21BuildManifest.Level50);
            stage.UnspentSkillPoints = PlayerProgressionRules.CalculateTotalSkillPointsAtLevel(Phase21BuildManifest.Level50);
            _progression.RestoreFromSaveData(stage);
            _skills.RestoreFromSaveData(new SkillTreeSaveData(), Phase21BuildManifest.Level50);
            Assert.That(PurchaseWithPrerequisites(NodeId, Phase21BuildManifest.Level50,
                new HashSet<string>(), out var reason), Is.True, reason);
            Assert.That(_skills.GetRank(NodeId), Is.EqualTo(1));
            Assert.That(_skills.TryAssignActiveSlot(0, ActionId), Is.True);
            Assert.That(_skills.State.GetActiveSlotSkillActionId(0), Is.EqualTo(ActionId));
            GameEventBus.Subscribe<SkillActionResolvedEvent>(OnResolved);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            GameEventBus.Unsubscribe<SkillActionResolvedEvent>(OnResolved);
            EfficiencyMarkRuntimeCoordinator.Instance?.ClearActiveMark();
            if (SceneManager.GetActiveScene().name == "CaveScene")
            {
                yield return SceneManager.LoadSceneAsync("FarmScene", LoadSceneMode.Single);
                yield return null;
            }
            var currentPlayer = PlayerController.ActiveInstance;
            if (currentPlayer != null)
            {
                currentPlayer.transform.position = _originalPosition;
                Physics2D.SyncTransforms();
            }
            if (_originalSurvival != null)
                SurvivalSkillRuntimeCoordinator.Instance?.State?.RestoreFromSaveData(
                    _originalSurvival, _originalSurvival.ActiveRunId,
                    _originalSurvival.ActiveCaveLevel);
            _progression?.RestoreFromSaveData(_originalProgression);
            _skills?.RestoreFromSaveData(_originalSkills, _originalProgression?.Level ?? 1);
            _stamina?.Initialize(_originalMaxStamina, _originalStamina);
            if (!_farmCachePreexisting && FarmSceneRuntimeStateCache.HasCachedState)
                FarmSceneRuntimeStateCache.Clear();
            if (!_caveCachePreexisting && CaveRunStateCache.Cached != null)
                CaveRunStateCache.Set(null);
            _outcomes.Clear();
            yield return null;
        }

        [UnityTest]
        public IEnumerator EfficiencyMark_RefusesWithoutStation_ThenCommitsAtFarmStation()
        {
            var controller = ActiveSkillExecutionController.Instance;
            var coordinator = EfficiencyMarkRuntimeCoordinator.Instance;
            var interaction = _player.GetComponentInChildren<InteractionSystem>();
            Assert.That(interaction, Is.Not.Null);
            var station = SceneManager.GetActiveScene().GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<CraftingPoint>(true))
                .FirstOrDefault(point => point.isActiveAndEnabled &&
                    !string.IsNullOrWhiteSpace(point.StationInstanceId) &&
                    point.GetComponent<Collider2D>()?.isActiveAndEnabled == true);
            Assert.That(station, Is.Not.Null, "FarmScene precisa de uma estação materializada.");
            var stationCollider = station.GetComponent<Collider2D>();

            float deadline = Time.time + 31f;
            while (controller.GetSlotCooldownRemaining(0) > 0f && Time.time < deadline)
                yield return null;
            Assert.That(controller.GetSlotCooldownRemaining(0), Is.Zero.Within(.01f));
            coordinator.ClearActiveMark();
            _stamina.FullRecover();

            // The same real slot must refuse when no CraftingPoint is in interaction range.
            _player.transform.position = station.transform.position + new Vector3(1000f, 1000f, 0f);
            Physics2D.SyncTransforms();
            Assert.That(interaction.GetCurrentInteractable(), Is.Not.TypeOf<CraftingPoint>());
            _outcomes.Clear();
            controller.TryUseSlot(0);
            Assert.That(_outcomes.Count, Is.EqualTo(1));
            Assert.That(_outcomes[0].ActionId, Is.EqualTo(ActionId));
            Assert.That(_outcomes[0].Outcome, Is.EqualTo(SkillActionResolvedEvent.Refused));
            Assert.That(_outcomes[0].FailureKey, Is.EqualTo("TargetIsNotCraftingStation"));
            Assert.That(coordinator.IsActive, Is.False);
            Assert.That(_stamina.CurrentStamina, Is.EqualTo(_stamina.MaxStamina));

            _player.transform.position = stationCollider.bounds.center;
            Physics2D.SyncTransforms();
            interaction.RegisterCandidate(stationCollider);
            Assert.That(interaction.GetCurrentInteractable(), Is.SameAs(station));
            Assert.That(WorkStaminaCostModifierProvider.PreviewCost(
                WorkStaminaChannel.Crafting, 8, station.transform.position.x,
                station.transform.position.y, station.StationInstanceId), Is.EqualTo(8));

            _outcomes.Clear();
            _staminaAtCommit = -1;
            controller.TryUseSlot(0);
            float commitDeadline = Time.time + 2f;
            while (_outcomes.Count == 0 && Time.time < commitDeadline)
                yield return null;
            Assert.That(_outcomes.Count, Is.EqualTo(1));
            Assert.That(_outcomes[0].ActionId, Is.EqualTo(ActionId));
            Assert.That(_outcomes[0].Rank, Is.EqualTo(1));
            Assert.That(_outcomes[0].Outcome, Is.EqualTo(SkillActionResolvedEvent.Committed),
                _outcomes[0].FailureKey);
            Assert.That(_staminaAtCommit, Is.EqualTo(_stamina.MaxStamina - 6));
            Assert.That(coordinator.IsActive, Is.True);
            Assert.That(coordinator.StationInstanceId, Is.EqualTo(station.StationInstanceId));
            int discountedCost = WorkStaminaCostModifierProvider.PreviewCost(
                WorkStaminaChannel.Crafting, 8, station.transform.position.x,
                station.transform.position.y, station.StationInstanceId);
            Assert.That(discountedCost, Is.InRange(6, 7),
                "Marca R1 deve reduzir o custo mesmo com crédito fracionário prévio.");
            Assert.That(WorkStaminaCostModifierProvider.PreviewCost(
                WorkStaminaChannel.Crafting, 8, station.transform.position.x + 100f,
                station.transform.position.y, string.Empty), Is.EqualTo(8));
        }

        [UnityTest]
        public IEnumerator EfficiencyMark_RankThreeRealSlot_DiscountsSixStaminaTransactionsAtFarmStation()
        {
            Assert.That(_skills.TryRankUpNode(NodeId, out var secondRankReason), Is.True,
                secondRankReason);
            Assert.That(_skills.TryRankUpNode(NodeId, out var thirdRankReason), Is.True,
                thirdRankReason);
            Assert.That(_skills.GetRank(NodeId), Is.EqualTo(3));
            var purchasedBuild = _skills.CaptureSaveData();
            _skills.RestoreFromSaveData(purchasedBuild, Phase21BuildManifest.Level50);
            Assert.That(_skills.GetRank(NodeId), Is.EqualTo(3),
                "O round-trip do DTO de árvore deve preservar a Marca R3 comprada.");
            Assert.That(_skills.State.GetActiveSlotSkillActionId(0), Is.EqualTo(ActionId));

            var controller = ActiveSkillExecutionController.Instance;
            var coordinator = EfficiencyMarkRuntimeCoordinator.Instance;
            var interaction = _player.GetComponentInChildren<InteractionSystem>();
            Assert.That(interaction, Is.Not.Null);
            var station = SceneManager.GetActiveScene().GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<CraftingPoint>(true))
                .FirstOrDefault(point => point.isActiveAndEnabled &&
                    !string.IsNullOrWhiteSpace(point.StationInstanceId) &&
                    point.GetComponent<Collider2D>()?.isActiveAndEnabled == true);
            Assert.That(station, Is.Not.Null, "FarmScene precisa de uma estação materializada.");

            float cooldownDeadline = Time.time + 31f;
            while (controller.GetSlotCooldownRemaining(0) > 0f && Time.time < cooldownDeadline)
                yield return null;
            Assert.That(controller.GetSlotCooldownRemaining(0), Is.Zero.Within(.01f));
            coordinator.ClearActiveMark();
            _stamina.FullRecover();
            _player.transform.position = station.GetComponent<Collider2D>().bounds.center;
            Physics2D.SyncTransforms();
            interaction.RegisterCandidate(station.GetComponent<Collider2D>());
            Assert.That(interaction.GetCurrentInteractable(), Is.SameAs(station));

            _outcomes.Clear();
            _staminaAtCommit = -1;
            controller.TryUseSlot(0);
            float commitDeadline = Time.time + 2f;
            while (_outcomes.Count == 0 && Time.time < commitDeadline)
                yield return null;
            Assert.That(_outcomes, Has.Count.EqualTo(1));
            Assert.That(_outcomes[0].Outcome, Is.EqualTo(SkillActionResolvedEvent.Committed),
                _outcomes[0].FailureKey);
            Assert.That(_outcomes[0].Rank, Is.EqualTo(3));
            Assert.That(_staminaAtCommit, Is.EqualTo(_stamina.MaxStamina - 6));
            Assert.That(coordinator.IsActive, Is.True);
            Assert.That(coordinator.RemainingSeconds, Is.GreaterThan(18f));

            // The slot cast above activates the real provider. Each row below performs a real
            // StaminaManager spend before advancing its fractional credit; this does not claim
            // to be six completed farm/crafting jobs or an income sample.
            int cumulativeSavings = 0;
            var savingsAfter = new int[6];
            const int baseCost = 8;
            for (int action = 0; action < savingsAfter.Length; action++)
            {
                int charged = WorkStaminaCostModifierProvider.PreviewCost(
                    WorkStaminaChannel.Crafting, baseCost, station.transform.position.x,
                    station.transform.position.y, station.StationInstanceId);
                Assert.That(charged, Is.InRange(5, 6));
                Assert.That(_stamina.TrySpendStamina(charged), Is.True);
                WorkStaminaCostModifierProvider.CommitSpend(
                    WorkStaminaChannel.Crafting, baseCost, charged,
                    station.transform.position.x, station.transform.position.y,
                    station.StationInstanceId);
                cumulativeSavings += baseCost - charged;
                savingsAfter[action] = cumulativeSavings;
            }

            Assert.That(savingsAfter[0], Is.InRange(2, 3));
            Assert.That(savingsAfter[1], Is.InRange(4, 5));
            Assert.That(savingsAfter[3], Is.InRange(9, 10));
            Assert.That(savingsAfter[5], Is.InRange(14, 15));
            Assert.That(savingsAfter[1] - 6, Is.LessThan(0),
                "O custo da Marca ainda não se paga após duas ações base 8.");
            Assert.That(savingsAfter[3] - 6, Is.GreaterThan(0),
                "O custo da Marca se paga após quatro ações base 8.");
            Assert.That(_stamina.CurrentStamina,
                Is.EqualTo(_stamina.MaxStamina - 6 - 6 * baseCost + cumulativeSavings));

            // Ten base-8 spends at R3 earn an integer amount of fractional savings.
            // Complete the cycle so this persistent coordinator retains its prior credit.
            _stamina.FullRecover();
            for (int action = 0; action < 4; action++)
            {
                int charged = WorkStaminaCostModifierProvider.PreviewCost(
                    WorkStaminaChannel.Crafting, baseCost, station.transform.position.x,
                    station.transform.position.y, station.StationInstanceId);
                Assert.That(_stamina.TrySpendStamina(charged), Is.True);
                WorkStaminaCostModifierProvider.CommitSpend(
                    WorkStaminaChannel.Crafting, baseCost, charged,
                    station.transform.position.x, station.transform.position.y,
                    station.StationInstanceId);
            }
            Assert.That(coordinator.FractionalCredit,
                Is.EqualTo(_fractionalCreditBefore).Within(.001f),
                "O teste nao deve alterar o credito fracionario persistente da run.");
        }

        [UnityTest]
        public IEnumerator RetreatSignal_ControlledAggroOnNaturalCaveEnemy_RealSlotAppliesDirectionalModifier()
        {
            const string retreatNodeId = "survival.sinal_retirada";
            const string retreatActionId = "skill_survival_sinal_retirada";
            Assert.That(_farmCachePreexisting || _caveCachePreexisting, Is.False,
                "Diagnóstico de Cave exige caches vazios para preservar estado preexistente.");

            var stage = _progression.CaptureSaveData();
            stage.TotalXp = ProgressionCurve.TotalXpForLevel(Phase21BuildManifest.Level20);
            stage.UnspentSkillPoints = PlayerProgressionRules.CalculateTotalSkillPointsAtLevel(
                Phase21BuildManifest.Level20);
            _progression.RestoreFromSaveData(stage);
            _skills.RestoreFromSaveData(new SkillTreeSaveData(), Phase21BuildManifest.Level20);
            Assert.That(PurchaseWithPrerequisites(retreatNodeId, Phase21BuildManifest.Level20,
                new HashSet<string>(), out var purchaseReason), Is.True, purchaseReason);
            Assert.That(_skills.GetRank(retreatNodeId), Is.EqualTo(1));
            Assert.That(_skills.TryAssignActiveSlot(0, retreatActionId), Is.True);

            var entrance = SceneManager.GetActiveScene().GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<CaveEntranceInteractable>(true))
                .FirstOrDefault(item => item.isActiveAndEnabled);
            Assert.That(entrance, Is.Not.Null, "FarmScene precisa da entrada real da Cave.");
            entrance.Interact(_player.gameObject);
            for (int frame = 0; frame < 360 && SceneManager.GetActiveScene().name != "CaveScene";
                frame++)
                yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("CaveScene"));

            CaveLevelRuntimeController level = null;
            for (int frame = 0; frame < 360; frame++)
            {
                level = SceneManager.GetActiveScene().GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<CaveLevelRuntimeController>(true))
                    .FirstOrDefault();
                if (level?.Materializer?.LastEnemySpawnPlan?.Entries?.Count > 0) break;
                yield return null;
            }
            Assert.That(level?.Materializer?.LastEnemySpawnPlan?.Entries,
                Is.Not.Null.And.Not.Empty, "Cave não materializou inimigos naturais.");
            var naturalIds = new HashSet<string>(
                level.Materializer.LastEnemySpawnPlan.Entries.Select(entry => entry.EnemyInstanceId));
            var threat = EnemyHealth.ActiveInstances.FirstOrDefault(enemy =>
                enemy != null && !enemy.IsDead && enemy.gameObject.scene.isLoaded &&
                naturalIds.Contains(enemy.EnemyInstanceId));
            Assert.That(threat, Is.Not.Null, "Nenhum EnemyHealth natural foi materializado.");
            var player = PlayerController.ActiveInstance;
            var runtime = SurvivalSkillRuntimeCoordinator.Instance;
            var controller = ActiveSkillExecutionController.Instance;
            Assert.That(player, Is.Not.Null);
            Assert.That(runtime, Is.Not.Null);
            Assert.That(controller, Is.Not.Null);
            Assert.That(_skills.State.GetActiveSlotSkillActionId(0), Is.EqualTo(retreatActionId));

            // Controlled aggro uses the ID of a live Cave spawn. This proves the slot/effect
            // pipeline, but is not a naturally triggered encounter or AC04 long-run sample.
            GameEventBus.Publish(new EnemyAggroStartedEvent(threat.EnemyInstanceId, string.Empty));
            Assert.That(runtime.HasEncounterThreat, Is.True);
            _stamina.FullRecover();
            int staminaBefore = _stamina.CurrentStamina;
            var resolved = new List<SkillActionResolvedEvent>();
            System.Action<SkillActionResolvedEvent> onResolved = evt =>
            {
                if (evt.ActionId == retreatActionId) resolved.Add(evt);
            };
            GameEventBus.Subscribe(onResolved);
            try
            {
                controller.TryUseSlot(0);
                float deadline = Time.time + 2f;
                while (resolved.Count == 0 && Time.time < deadline)
                    yield return null;
                Assert.That(resolved, Has.Count.EqualTo(1));
                Assert.That(resolved[0].Outcome, Is.EqualTo(SkillActionResolvedEvent.Committed),
                    resolved[0].FailureKey);
                Assert.That(resolved[0].Rank, Is.EqualTo(1));
                Assert.That(_stamina.CurrentStamina, Is.EqualTo(staminaBefore - 15));
                Assert.That(runtime.IsRetreatActive, Is.True);

                var primary = EnemyHealth.ActiveInstances.Where(enemy =>
                        enemy != null && !enemy.IsDead &&
                        runtime.State.ActiveEnemyInstanceIds.Contains(enemy.EnemyInstanceId))
                    .OrderBy(enemy => ((Vector2)enemy.transform.position -
                        (Vector2)player.transform.position).sqrMagnitude)
                    .ThenBy(enemy => enemy.EnemyInstanceId)
                    .First();
                var away = ((Vector2)player.transform.position -
                    (Vector2)primary.transform.position).normalized;
                Assert.That(away.sqrMagnitude, Is.GreaterThan(.5f));
                var retreat = DirectionalMobilityModifierProvider.Resolve(
                    MobilityActionKind.SprintTick, away.x, away.y);
                var advance = DirectionalMobilityModifierProvider.Resolve(
                    MobilityActionKind.SprintTick, -away.x, -away.y);
                Assert.That(retreat.CostMultiplier, Is.EqualTo(.70f).Within(.001f));
                Assert.That(retreat.SpeedMultiplier, Is.EqualTo(1.12f).Within(.001f));
                Assert.That(advance.CostMultiplier, Is.EqualTo(1f).Within(.001f));
                Assert.That(advance.SpeedMultiplier, Is.EqualTo(1f).Within(.001f));
            }
            finally
            {
                GameEventBus.Unsubscribe(onResolved);
                runtime.ClearRetreat();
            }
        }

        private void OnResolved(SkillActionResolvedEvent outcome)
        {
            if (outcome.ActionId != ActionId) return;
            _outcomes.Add(outcome);
            if (outcome.Outcome == SkillActionResolvedEvent.Committed)
                _staminaAtCommit = _stamina.CurrentStamina;
        }

        private bool PurchaseWithPrerequisites(string nodeId, int level,
            HashSet<string> visiting, out string reason)
        {
            reason = string.Empty;
            if (_skills.GetRank(nodeId) > 0) return true;
            if (!_skills.NodeIndex.TryGetValue(nodeId, out var node))
            { reason = "nó ausente"; return false; }
            if (!visiting.Add(nodeId))
            { reason = "ciclo de pré-requisitos"; return false; }
            try
            {
                foreach (var prerequisite in node.PrerequisiteNodeIds ?? new List<string>())
                    if (!PurchaseWithPrerequisites(prerequisite, level, visiting, out reason)) return false;
                for (int attempt = 0; attempt < _skills.NodeIndex.Count * SkillTierRules.AbsoluteRankCap; attempt++)
                {
                    if (_skills.TryPurchaseNode(nodeId, level, null, out var feedback)) return true;
                    bool progressed = false;
                    foreach (var filler in _skills.NodeIndex.Values
                        .Where(n => n.TreeId == node.TreeId && n.Tier <= node.Tier &&
                            !n.IsCapstone && _skills.GetRank(n.SkillNodeId) == 0 &&
                            n.SkillNodeId != nodeId && !visiting.Contains(n.SkillNodeId))
                        .OrderBy(n => n.Tier).ThenBy(n => n.SkillNodeId))
                    {
                        if (!_skills.TryPurchaseNode(filler.SkillNodeId, level, null, out _)) continue;
                        progressed = true;
                        break;
                    }
                    if (progressed) continue;
                    foreach (var filler in _skills.NodeIndex.Values
                        .Where(n => n.TreeId == node.TreeId && !n.IsCapstone &&
                            _skills.GetRank(n.SkillNodeId) > 0 && n.SkillNodeId != nodeId)
                        .OrderBy(n => n.Tier).ThenBy(n => n.SkillNodeId))
                    {
                        if (!_skills.TryRankUpNode(filler.SkillNodeId, out _)) continue;
                        progressed = true;
                        break;
                    }
                    if (!progressed) { reason = feedback; return false; }
                }
                reason = "limite de tentativas";
                return false;
            }
            finally { visiting.Remove(nodeId); }
        }
    }
}
