using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CindarsHope.Cave;
using CindarsHope.Cave.Ecosystem;
using CindarsHope.Cave.Generation;
using CindarsHope.Cave.Runtime;
using CindarsHope.Combat;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Enemy;
using CindarsHope.Equipment;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Player.Progression;
using CindarsHope.Player;
using CindarsHope.Player.InputRouting;
using CindarsHope.Quests.Runtime;
using CindarsHope.Save;
using CindarsHope.SceneManagement;
using CindarsHope.Skills;
using CindarsHope.Skills.Runtime.Effects;
using CindarsHope.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.PlayMode.Composition
{
    /// <summary>AC01 purchase fixture. It uses only production purchase and quest award paths.</summary>
    public sealed class Phase21BuildPurchasePlayModeTests
    {
        private const float TtkIsolationRadius = 10f;
        private static readonly string[] TtkSeedCandidates =
        {
            "phase21_seed_l20", "phase21_seed_l20_open_01", "phase21_seed_l20_open_02",
            "phase21_seed_l20_open_03", "phase21_seed_l20_open_04", "phase21_seed_l20_open_05",
            "phase21_seed_l20_open_06", "phase21_seed_l20_open_07",
            "phase21_seed_l20_open_10"
        };

        private sealed class TtkControlledKeys : IPlayerKeyReader
        {
            public Vector2 Movement;
            public bool RightHeld => _rightHeld;
            private bool _rightHeld;
            private bool _rightDown;
            private bool _rightUp;

            public void SetRightHeld(bool held)
            {
                _rightDown = held && !_rightHeld;
                _rightUp = !held && _rightHeld;
                _rightHeld = held;
            }

            public bool GetKey(KeyCode key)
            {
                switch (key)
                {
                    case KeyCode.A: return Movement.x < -0.25f;
                    case KeyCode.D: return Movement.x > 0.25f;
                    case KeyCode.S: return Movement.y < -0.25f;
                    case KeyCode.W: return Movement.y > 0.25f;
                    case KeyCode.E: return _rightHeld;
                    default: return false;
                }
            }

            public bool GetKeyDown(KeyCode key) => key == KeyCode.E && _rightDown;
            public bool GetKeyUp(KeyCode key) => key == KeyCode.E && _rightUp;
        }

        [Serializable]
        private sealed class TtkDamageRow
        {
            public float secondsFromFirstHit;
            public string sourceKind;
            public string actionToken;
            public int damage;
            public int rawDamage;
            public int hpAfter;
        }

        [Serializable]
        private sealed class TtkWeaponAttemptRow
        {
            public float secondsFromStart;
            public Vector2 playerPosition;
            public Vector2 targetPosition;
            public Vector2 facing;
            public Vector2 moveInput;
            public float targetDistance;
            public float targetColliderGap;
            public Vector2 queryCenter;
            public float queryRadius;
            public int queryCandidates;
            public bool targetInQuery;
            public int enemyLayerCandidates;
            public bool targetInEnemyLayerQuery;
            public int targetRootColliderLayer;
            public bool worldSolidBetweenPlayerAndTarget;
            public float firstWorldSolidHitDistance;
            public string firstBlockingColliderName;
            public int firstBlockingColliderLayer;
            public bool wallTileBetweenPlayerAndTarget;
            public bool movementLocked;
            public bool castActive;
        }

        [Serializable]
        private sealed class TtkIncomingDamageRow
        {
            public float secondsFromStart;
            public string sourceId;
            public int damage;
        }

        [Serializable]
        private sealed class TtkSelectionRejection
        {
            public string seed;
            public string enemyInstanceId;
            public string reason;
            public float nearestOtherEnemyDistance = -1f;
        }

        [Serializable]
        private sealed class TtkObservation
        {
            public string scenarioId = "phase21_l20_melee_whirl_common_r1_ttk";
            public string seed = "phase21_seed_l20";
            public string actionId = "skill_melee_whirl_cut";
            public string equipmentItemId = "item_weapon_sword_iron";
            public string weaponActionId;
            public string enemyId = "enemy_spore_imp";
            public string enemyInstanceId;
            public string creatureClass = "Common";
            public string classSource = "EnemyDataSO+SpawnPlan";
            public string eliteAffix = "None";
            public string movementMode = "scripted_keys_through_controller_update_fixedupdate";
            public string commandPolicy = "skill_at_0_and_each_2.5s;_E_press_release_after_0.85s_when_no_active_cast_and_each_0.65s";
            public string outcome = "NOT RUN";
            public string reason;
            public Vector2Int selectedSpawnGrid;
            public Vector2 selectedAttackStart;
            public List<TtkSelectionRejection> selectionRejections = new List<TtkSelectionRejection>();
            public int actionRank = 1;
            public int hpInitial;
            public int hpMax;
            public int hpFinal;
            public int playerHpInitial;
            public int playerHpFinal;
            public int staminaInitial;
            public int staminaFinal;
            public int manaInitial;
            public int manaFinal;
            public int skillCommitted;
            public int skillRefused;
            public int weaponCommitted;
            public int skillHits;
            public int weaponHits;
            public int externalHits;
            public int skillDamage;
            public int weaponDamage;
            public int externalDamage;
            public int incomingDamage;
            public int otherEnemyIncomingDamage;
            public float nearestOtherEnemyDistanceAtStart = -1f;
            public int otherEnemiesWithinEightUnitsAtStart;
            public int whiffs;
            public float ttkSeconds = -1f;
            public float elapsedSeconds;
            public List<TtkDamageRow> damageRows = new List<TtkDamageRow>();
            public List<TtkWeaponAttemptRow> weaponAttempts = new List<TtkWeaponAttemptRow>();
            public List<TtkIncomingDamageRow> incomingDamageRows = new List<TtkIncomingDamageRow>();
        }
        [Serializable]
        private sealed class SkillHitObservation
        {
            public string scenarioId = "phase21_l20_melee_whirl_common";
            public string seed = "phase21_seed_l20";
            public string actionId = "skill_melee_whirl_cut";
            public string enemyId = "enemy_spore_imp";
            public string enemyInstanceId;
            public string creatureClass = "Common";
            public string eliteAffix = "None";
            public string outcome;
            public string ttkOutcome = "NOT RUN";
            public int hpBefore;
            public int hpAfter;
            public int skillHitCount;
            public int skillDamage;
            public int externalDamageCount;
            public int committedCount;
            public string actionToken;
            public float distanceBeforeCast;
            public float distanceAfterCast;
            public int physicsOverlapsBeforeCast;
            public int targetCollidersBeforeCast;
        }

        private SkillTreeManager _skills;
        private PlayerProgressionManager _progression;
        private SkillTreeSaveData _originalSkills;
        private PlayerProgressionSaveData _originalProgression;
        private QuestStateSectionSaveData _originalQuests;
        private InventoryManager _inventory;
        private EquipmentManager _equipment;
        private ItemDatabaseSO _items;
        private InventorySaveData _originalInventory;
        private EquipmentSaveData _originalEquipment;
        private PlayerManager _playerManager;
        private StaminaManager _staminaManager;
        private ManaManager _manaManager;
        private int _originalPlayerHp;
        private int _originalPlayerMaxHp;
        private int _originalPlayerGold;
        private int _originalStamina;
        private int _originalStaminaMax;
        private ManaManagerSaveData _originalMana;
        private bool _farmCachePreexisting;
        private bool _caveCachePreexisting;
        private bool _vitalsCaptured;
        private float _originalTimeScale;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _vitalsCaptured = false;
            _originalTimeScale = Time.timeScale;
            _farmCachePreexisting = FarmSceneRuntimeStateCache.HasCachedState;
            _caveCachePreexisting = CaveRunStateCache.Cached != null;
            // The empty Test Runner scene can outlive DeathSystemBootstrap's 120-frame
            // bind window. Load the real composition scene synchronously before yielding.
            SceneManager.LoadScene("FarmScene", LoadSceneMode.Single);
            yield return null;
            _skills = SkillTreeManager.Instance;
            _progression = PlayerProgressionManager.Instance;
            Assert.That(_skills, Is.Not.Null);
            Assert.That(_progression, Is.Not.Null);
            for (int frame = 0; frame < 180 && QuestRuntimeBootstrap.QuestService == null; frame++)
                yield return null;
            Assert.That(QuestRuntimeBootstrap.QuestService, Is.Not.Null,
                "FarmScene não materializou QuestService nem após o timeout de bootstrap.");
            _originalSkills = _skills.CaptureSaveData();
            _originalProgression = _progression.CaptureSaveData();
            _originalQuests = QuestRuntimeBootstrap.CaptureSaveData();
            Assert.That(_originalQuests, Is.Not.Null);
            var bootstrap = GameBootstrap.Instance;
            _inventory = bootstrap?.InventoryManager as InventoryManager;
            _equipment = EquipmentManager.Instance;
            _items = bootstrap?.ItemDatabase as ItemDatabaseSO;
            Assert.That(_inventory, Is.Not.Null, "FarmScene não expôs inventário runtime.");
            Assert.That(_equipment, Is.Not.Null, "FarmScene não expôs equipamento runtime.");
            Assert.That(_items, Is.Not.Null, "FarmScene não expôs ItemDatabaseSO.");
            _originalInventory = _inventory.CaptureSaveData();
            _originalEquipment = _equipment.CaptureSaveData();
            _playerManager = bootstrap.PlayerManager as PlayerManager;
            _staminaManager = bootstrap.StaminaManager as StaminaManager;
            _manaManager = bootstrap.ManaManager as ManaManager;
            Assert.That(_playerManager, Is.Not.Null);
            Assert.That(_staminaManager, Is.Not.Null);
            Assert.That(_manaManager, Is.Not.Null);
            _originalPlayerHp = _playerManager.CurrentHP;
            _originalPlayerMaxHp = _playerManager.MaxHP;
            _originalPlayerGold = _playerManager.CurrentGold;
            _originalStamina = _staminaManager.CurrentStamina;
            _originalStaminaMax = _staminaManager.MaxStamina;
            _originalMana = _manaManager.CaptureSaveData();
            Assert.That(_originalMana, Is.Not.Null);
            _vitalsCaptured = true;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_originalProgression != null) _progression?.RestoreFromSaveData(_originalProgression);
            if (_originalSkills != null) _skills?.RestoreFromSaveData(_originalSkills, _originalProgression?.Level ?? 1);
            if (_originalQuests != null) QuestRuntimeBootstrap.RestoreFromSaveData(_originalQuests);
            if (_originalInventory != null) _inventory?.RestoreFromSaveData(_originalInventory);
            if (_originalEquipment != null) _equipment?.RestoreFromSaveData(_originalEquipment);
            if (_vitalsCaptured)
            {
                _playerManager?.RestoreState(_originalPlayerMaxHp, _originalPlayerHp, _originalPlayerGold);
                _staminaManager?.Initialize(_originalStaminaMax, _originalStamina);
                _manaManager?.RestoreFromSaveData(_originalMana);
            }
            Time.timeScale = _originalTimeScale;
            if (!_farmCachePreexisting && FarmSceneRuntimeStateCache.HasCachedState)
                FarmSceneRuntimeStateCache.Clear();
            if (!_caveCachePreexisting && CaveRunStateCache.Cached != null)
                CaveRunStateCache.Set(null);
            yield return null;
        }

        [UnityTest]
        public IEnumerator AC01_StagedPurchases_UseCanonicalBudgetAndPersistState()
        {
            int l20Rows = 0, capstoneRows = 0, equippedRows = 0, supportRows = 0;
            var unavailable = new List<string>();
            var coverageRows = new List<string>();

            foreach (var action in Phase21BuildManifest.Level20Actions)
            foreach (var rank in Phase21BuildManifest.Ranks)
            {
                ResetStage(Phase21BuildManifest.Level20);
                string label = $"{action.Id}/L20/R{rank}";
                if (!TryPurchaseAction(action.ActionId, Phase21BuildManifest.Level20, rank, out var reason))
                { unavailable.Add($"{label}: {reason}"); continue; }
                l20Rows++;
                if (TryEquipFixture(action.EquipmentItemId, out var equipReason)) equippedRows++;
                else unavailable.Add($"{label}: equipamento: {equipReason}");
                Assert.That(_skills.State.GetActiveSlotSkillActionId(0), Is.EqualTo(action.ActionId), label);
                RoundTrip(label, Phase21BuildManifest.Level20, action.ActionId, FindActionNode(action.ActionId), rank);
                coverageRows.Add(FormatRow(label, Phase21BuildManifest.Level20,
                    action.ActionId, null, null));
                yield return null;
            }

            foreach (var build in Phase21BuildManifest.Builds.Where(b => b.OffensiveActionId != null))
            {
                ResetStage(Phase21BuildManifest.Level50);
                string label = $"{build.BuildId}/L50/capstoneR1";
                if (!TryAwardActs(out var actReason)) { unavailable.Add($"{label}: {actReason}"); continue; }
                if (!TryPurchaseBuild(build, build.OffensiveActionId, Phase21BuildManifest.Level50, Phase21BuildManifest.Rank1, out var reason))
                { unavailable.Add($"{label}: {reason}"); continue; }
                capstoneRows++;
                if (TryEquipFixture(build.Level50Equipment.ItemId, out var equipReason)) equippedRows++;
                else unavailable.Add($"{label}: equipamento: {equipReason}");
                var actionNodeId = FindActionNode(build.OffensiveActionId);
                RoundTrip(label, Phase21BuildManifest.Level50, build.OffensiveActionId, actionNodeId, _skills.GetRank(actionNodeId));
                Assert.That(_skills.GetRank(build.CapstoneNodeId), Is.EqualTo(Phase21BuildManifest.Rank1), label + " capstone após load");
                Assert.That(_skills.GetChosenVariant(build.CapstoneNodeId), Is.EqualTo(build.VariantId), label + " variante após load");
                coverageRows.Add(FormatRow(label, Phase21BuildManifest.Level50,
                    build.OffensiveActionId, build.CapstoneNodeId, build.VariantId));
                yield return null;
            }

            foreach (var build in Phase21BuildManifest.Builds.Where(b => b.OffensiveActionId != null))
            {
                ResetStage(Phase21BuildManifest.Level54);
                string label = $"{build.BuildId}/L54/capstoneR3";
                if (!TryAwardActs(out var actReason)) { unavailable.Add($"{label}: {actReason}"); continue; }
                if (!TryPurchaseBuild(build, build.OffensiveActionId, Phase21BuildManifest.Level54, Phase21BuildManifest.Rank3, out var reason))
                { unavailable.Add($"{label}: {reason}"); continue; }
                capstoneRows++;
                if (TryEquipFixture(build.Level50Equipment.ItemId, out var equipReason)) equippedRows++;
                else unavailable.Add($"{label}: equipamento: {equipReason}");
                Assert.That(_skills.GetRank(build.CapstoneNodeId), Is.EqualTo(Phase21BuildManifest.Rank3), label);
                var actionNodeId = FindActionNode(build.OffensiveActionId);
                RoundTrip(label, Phase21BuildManifest.Level54, build.OffensiveActionId, actionNodeId, _skills.GetRank(actionNodeId));
                Assert.That(_skills.GetRank(build.CapstoneNodeId), Is.EqualTo(Phase21BuildManifest.Rank3), label + " capstone após load");
                Assert.That(_skills.GetChosenVariant(build.CapstoneNodeId), Is.EqualTo(build.VariantId), label + " variante após load");
                coverageRows.Add(FormatRow(label, Phase21BuildManifest.Level54,
                    build.OffensiveActionId, build.CapstoneNodeId, build.VariantId));
                yield return null;
            }

            foreach (var support in Phase21BuildManifest.Builds.Where(b => b.OffensiveActionId == null))
            foreach (var level in new[] { Phase21BuildManifest.Level50, Phase21BuildManifest.Level54 })
            {
                ResetStage(level);
                int rank = level == Phase21BuildManifest.Level50 ? Phase21BuildManifest.Rank1 : Phase21BuildManifest.Rank3;
                string label = $"{support.BuildId}/L{level}/capstoneR{rank}";
                string blockedNode = FindNotYetExecutablePrerequisite(support.CapstoneNodeId);
                if (blockedNode != null)
                { unavailable.Add($"{label}: NOT RUN — pré-requisito {blockedNode} NotYetExecutable"); continue; }
                string actionId = SupportActionId(support.CapstoneNodeId);
                if (actionId == null)
                { unavailable.Add($"{label}: NOT RUN — ação de suporte sem mapeamento canônico"); continue; }
                if (!TryAwardActs(out var actReason))
                { unavailable.Add($"{label}: NOT RUN — {actReason}"); continue; }
                if (!TryPurchaseBuild(support, actionId, level, rank, out var reason))
                { unavailable.Add($"{label}: NOT RUN — {reason}"); continue; }
                capstoneRows++;
                supportRows++;
                string actionNodeId = FindActionNode(actionId);
                RoundTrip(label, level, actionId, actionNodeId, _skills.GetRank(actionNodeId));
                Assert.That(_skills.GetRank(support.CapstoneNodeId), Is.EqualTo(rank), label + " capstone após load");
                Assert.That(_skills.GetChosenVariant(support.CapstoneNodeId), Is.EqualTo(support.VariantId), label + " variante após load");
                coverageRows.Add(FormatRow(label, level, actionId, support.CapstoneNodeId, support.VariantId,
                    equipmentRequired: false));
                yield return null;
            }

            // Tiro Carregado is intentionally purchase-only: its authored DamagePerRank is zero.
            Debug.Log($"Phase21 AC01 purchase coverage: L20={l20Rows}/8, capstones={capstoneRows}/18, equipped={equippedRows}/22, support={supportRows}/4, unavailable={unavailable.Count}. Charged Shot rank effect not validated.\n{string.Join("\n", coverageRows)}\n{string.Join("\n", unavailable)}");
            Assert.That(l20Rows, Is.EqualTo(8), "Every L20 pre-capstone action/rank row must be legal.");
            Assert.That(supportRows, Is.EqualTo(4), "Both support capstones must materialize at L50 R1 and L54 R3.");
            Assert.That(capstoneRows, Is.EqualTo(18), "All nine archetypes must materialize at L50 R1 and L54 R3.");
            Assert.That(equippedRows, Is.EqualTo(22), "Every combat purchase row needs a real equipped item.");
            Assert.That(unavailable.Count, Is.EqualTo(0), string.Join("\n", unavailable));
            yield return null;
        }

        [UnityTest]
        public IEnumerator AC21_L20WhirlCut_HitsNaturalCommonInstanceThroughRealSkillPipeline()
        {
            Assert.That(_farmCachePreexisting || _caveCachePreexisting, Is.False,
                "Cave probe requer caches de cena vazios para preservar estado pré-existente.");
            ResetStage(Phase21BuildManifest.Level20);
            Assert.That(TryPurchaseAction("skill_melee_whirl_cut", Phase21BuildManifest.Level20,
                Phase21BuildManifest.Rank1, out var purchaseReason), Is.True, purchaseReason);
            Assert.That(TryEquipFixture("item_weapon_sword_iron", out var equipReason), Is.True, equipReason);

            var entrance = FindInActiveScene<CaveEntranceInteractable>();
            var player = PlayerController.ActiveInstance;
            Assert.That(entrance, Is.Not.Null);
            Assert.That(player, Is.Not.Null);
            entrance.Interact(player.gameObject);
            for (int frame = 0; frame < 360 && SceneManager.GetActiveScene().name != "CaveScene"; frame++)
                yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("CaveScene"));

            var levelController = FindInActiveScene<CaveLevelRuntimeController>();
            var run = CaveRunManager.Instance;
            Assert.That(levelController, Is.Not.Null);
            Assert.That(run, Is.Not.Null);
            run.RestoreRuntimeState(new CaveRuntimeState
            {
                HasActiveRun = true,
                CurrentCaveLevel = Phase21BuildManifest.Level20,
                DeepestLayerReached = Phase21BuildManifest.Level20,
                CaveWorldSeed = "world_phase21_probe",
                CaveRunSeed = "phase21_seed_l20",
                CaveRunId = "probe_phase21_l20_melee_whirl_common"
            });
            levelController.GenerateCurrentLevel();
            for (int frame = 0; frame < 360; frame++)
            {
                var plan = levelController.Materializer?.LastEnemySpawnPlan;
                if (plan?.Entries != null && plan.Entries.Count > 0 &&
                    plan.CaveLevel == Phase21BuildManifest.Level20 &&
                    plan.CaveRunSeed == "phase21_seed_l20") break;
                yield return null;
            }

            var entries = levelController.Materializer?.LastEnemySpawnPlan?.Entries;
            Assert.That(entries, Is.Not.Null.And.Not.Empty, "Cave não materializou o spawn natural.");
            var entry = entries.FirstOrDefault(candidate =>
                candidate.EnemyId == "enemy_spore_imp" &&
                candidate.EliteAffix == CindarsHope.Enemy.EliteAffix.None);
            Assert.That(entry, Is.Not.Null, "Seed L20 não gerou Spore Imp sem afixo.");
            var target = EnemyHealth.ActiveInstances.FirstOrDefault(health =>
                health != null && health.EnemyInstanceId == entry.EnemyInstanceId &&
                health.EnemyId == entry.EnemyId && health.gameObject.scene.isLoaded && !health.IsDead);
            Assert.That(target, Is.Not.Null, "Spore Imp Common não está ativo pelo EnemyInstanceId.");
            var combatRegistry = Resources.Load<CombatRuntimeDatabasesRegistrySO>("CombatRuntimeDatabasesRegistry");
            Assert.That(combatRegistry?.EnemyDatabase, Is.Not.Null);
            Assert.That(combatRegistry.EnemyDatabase.TryGetById(entry.EnemyId, out var enemyData), Is.True);
            Assert.That(enemyData, Is.Not.Null);
            Assert.That(enemyData.IsBoss || enemyData.IsMiniBoss || enemyData.IsElite ||
                        enemyData.PrimaryRole == EnemyRole.Boss ||
                        enemyData.PrimaryRole == EnemyRole.MiniBoss ||
                        enemyData.PrimaryRole == EnemyRole.Elite, Is.False,
                "Asset do alvo deixou de ser Common.");
            var brain = target.GetComponent<EnemyBrain>();
            Assert.That(brain, Is.Not.Null);
            Assert.That(brain.HasEliteClassification, Is.False,
                "Instância sem afixo deixou de ser Common no runtime.");

            player = PlayerController.ActiveInstance;
            var skillController = ActiveSkillExecutionController.Instance;
            Assert.That(player, Is.Not.Null);
            Assert.That(skillController, Is.Not.Null);
            Assert.That(_skills.State.GetActiveSlotSkillActionId(0), Is.EqualTo("skill_melee_whirl_cut"));
            var body = player.GetComponent<Rigidbody2D>();
            Assert.That(body, Is.Not.Null);
            yield return new WaitForFixedUpdate();
            var strikeAnchor = (Vector2)target.transform.position + Vector2.left * 0.6f;
            player.transform.position = strikeAnchor;
            body.position = strikeAnchor;
            Physics2D.SyncTransforms();

            var stamina = GameBootstrap.Instance?.StaminaManager as CindarsHope.Player.StaminaManager;
            Assert.That(stamina, Is.Not.Null);
            stamina.AddStamina(stamina.MaxStamina);
            var observation = new SkillHitObservation
            {
                enemyInstanceId = entry.EnemyInstanceId,
                hpBefore = target.CurrentHp,
                distanceBeforeCast = Vector2.Distance(body.position, target.transform.position),
                physicsOverlapsBeforeCast = Physics2D.OverlapCircleAll(body.position, 1.7f).Length,
                targetCollidersBeforeCast = target.GetComponentsInChildren<Collider2D>().Length
            };
            Assert.That(observation.hpBefore, Is.GreaterThan(0));

            Action<DamageAppliedEvent> onDamage = evt =>
            {
                var damage = evt?.DamageResult;
                if (damage == null || damage.TargetInstanceId != observation.enemyInstanceId) return;
                if (!string.IsNullOrEmpty(damage.ActionToken) &&
                    damage.ActionToken.StartsWith(observation.actionId + ":", StringComparison.Ordinal) &&
                    damage.SourceKind == CindarsHope.Foundation.DamageSourceKind.PlayerMelee)
                {
                    observation.skillHitCount++;
                    observation.skillDamage += damage.FinalDamage;
                    observation.actionToken = damage.ActionToken;
                }
                else observation.externalDamageCount++;
            };
            Action<SkillActionResolvedEvent> onResolved = evt =>
            {
                if (evt.ActionId == observation.actionId &&
                    evt.Outcome == SkillActionResolvedEvent.Committed) observation.committedCount++;
            };
            GameEventBus.Subscribe(onDamage);
            GameEventBus.Subscribe(onResolved);
            try
            {
                skillController.TryUseSlot(0);
                yield return new WaitForSecondsRealtime(0.7f);
            }
            finally
            {
                GameEventBus.Unsubscribe(onDamage);
                GameEventBus.Unsubscribe(onResolved);
            }

            observation.hpAfter = target != null ? target.CurrentHp : 0;
            observation.distanceAfterCast = target != null
                ? Vector2.Distance(body.position, target.transform.position) : -1f;
            observation.outcome = observation.committedCount == 1 &&
                observation.skillHitCount > 0 && observation.skillDamage > 0 &&
                observation.externalDamageCount == 0 &&
                observation.hpBefore - observation.hpAfter == observation.skillDamage
                    ? "SKILL_HIT_OBSERVED" : "NOT RUN";
            WriteSkillHitObservation(observation);
            Assert.That(observation.committedCount, Is.EqualTo(1), "Corte Giratório não foi committed.");
            Assert.That(observation.skillHitCount, Is.GreaterThan(0), "Nenhum DamageAppliedEvent da skill atingiu a instância Common.");
            Assert.That(observation.skillDamage, Is.GreaterThan(0));
            Assert.That(observation.outcome, Is.EqualTo("SKILL_HIT_OBSERVED"),
                "HP ou dano externo tornaram a amostra de hit ambígua.");
            yield return SceneManager.LoadSceneAsync("FarmScene", LoadSceneMode.Single);
            yield return null;
        }

        [UnityTest]
        [Ignore("Exploratory AC02: conservative selector rejected natural Common targets in nine fixed L20 seeds (r11); reactivate after encounter policy review.")]
        public IEnumerator AC21_L20WhirlCut_R1_MeasuresNaturalCommonKillThroughSkillAndEquippedSword()
        {
            Assert.That(_farmCachePreexisting || _caveCachePreexisting, Is.False,
                "Cave probe requer caches de cena vazios para preservar estado pré-existente.");
            ResetStage(Phase21BuildManifest.Level20);
            Assert.That(TryPurchaseAction("skill_melee_whirl_cut", Phase21BuildManifest.Level20,
                Phase21BuildManifest.Rank1, out var purchaseReason), Is.True, purchaseReason);
            Assert.That(TryEquipFixture("item_weapon_sword_iron", out var equipReason), Is.True, equipReason);
            Assert.That(_skills.State.GetActiveSlotSkillActionId(0), Is.EqualTo("skill_melee_whirl_cut"));

            var player = PlayerController.ActiveInstance;
            var entrance = FindInActiveScene<CaveEntranceInteractable>();
            Assert.That(player, Is.Not.Null);
            Assert.That(entrance, Is.Not.Null);
            entrance.Interact(player.gameObject);
            for (int frame = 0; frame < 360 && SceneManager.GetActiveScene().name != "CaveScene"; frame++)
                yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("CaveScene"));

            var levelController = FindInActiveScene<CaveLevelRuntimeController>();
            var run = CaveRunManager.Instance;
            Assert.That(levelController, Is.Not.Null);
            Assert.That(run, Is.Not.Null);
            var observation = new TtkObservation { seed = string.Empty };
            var combatRegistry = Resources.Load<CombatRuntimeDatabasesRegistrySO>("CombatRuntimeDatabasesRegistry");
            Assert.That(combatRegistry?.EnemyDatabase, Is.Not.Null);
            Assert.That(combatRegistry.EnemyDatabase.TryGetById(observation.enemyId, out var enemyData), Is.True);
            Assert.That(enemyData, Is.Not.Null);
            Assert.That(enemyData.IsBoss || enemyData.IsMiniBoss || enemyData.IsElite ||
                        enemyData.PrimaryRole == EnemyRole.Boss ||
                        enemyData.PrimaryRole == EnemyRole.MiniBoss ||
                        enemyData.PrimaryRole == EnemyRole.Elite, Is.False,
                "Asset do alvo divergiu da classe Common.");
            player = PlayerController.ActiveInstance;
            Assert.That(player, Is.Not.Null);
            var playerCollider = player.GetComponent<Collider2D>();
            Assert.That(playerCollider, Is.Not.Null);
            CaveEnemySpawnPlanEntry entry = null;
            EnemyHealth target = null;
            Vector2 startPosition = default;
            foreach (var seed in TtkSeedCandidates)
            {
                // A different seed is a different run. RestoreRuntimeState starts with no
                // visited snapshots; never re-roll a visited level of the same run.
                run.RestoreRuntimeState(new CaveRuntimeState
                {
                    HasActiveRun = true,
                    CurrentCaveLevel = Phase21BuildManifest.Level20,
                    DeepestLayerReached = Phase21BuildManifest.Level20,
                    CaveWorldSeed = "world_phase21_probe",
                    CaveRunSeed = seed,
                    CaveRunId = "probe_phase21_l20_melee_whirl_common_ttk_" + seed
                });
                levelController.GenerateCurrentLevel();
                yield return null; // let previous run's Destroy calls finish before physics queries
                Physics2D.SyncTransforms();
                var plan = levelController.Materializer?.LastEnemySpawnPlan;
                var elements = levelController.Materializer?.LastEnvironmentElements;
                var level = levelController.CurrentGeneratedLevel;
                if (plan?.Entries == null || plan.CaveLevel != Phase21BuildManifest.Level20 ||
                    plan.CaveRunSeed != seed || level == null || elements == null)
                {
                    observation.selectionRejections.Add(new TtkSelectionRejection
                    { seed = seed, reason = "spawn plan, environment or generated level missing/mismatched" });
                    continue;
                }
                foreach (var candidate in plan.Entries.Where(item =>
                    item.EnemyId == observation.enemyId && item.EliteAffix == CindarsHope.Enemy.EliteAffix.None))
                {
                    var candidateTarget = EnemyHealth.ActiveInstances.FirstOrDefault(health =>
                        health != null && health.EnemyInstanceId == candidate.EnemyInstanceId &&
                        health.EnemyId == candidate.EnemyId && health.gameObject.scene == player.gameObject.scene &&
                        !health.IsDead);
                    if (candidateTarget == null)
                    {
                        observation.selectionRejections.Add(new TtkSelectionRejection
                        { seed = seed, enemyInstanceId = candidate.EnemyInstanceId, reason = "natural instance absent" });
                        continue;
                    }
                    if (candidateTarget.GetComponent<EnemyBrain>()?.HasEliteClassification != false)
                    {
                        observation.selectionRejections.Add(new TtkSelectionRejection
                        { seed = seed, enemyInstanceId = candidate.EnemyInstanceId, reason = "runtime elite classification" });
                        continue;
                    }
                    if (!TryFindClearTtkStart(level, elements, candidate, candidateTarget,
                        playerCollider, out var candidateStart, out var rejection,
                        out var nearestOtherEnemyDistance))
                    {
                        observation.selectionRejections.Add(new TtkSelectionRejection
                        {
                            seed = seed,
                            enemyInstanceId = candidate.EnemyInstanceId,
                            reason = rejection,
                            nearestOtherEnemyDistance = nearestOtherEnemyDistance
                        });
                        continue;
                    }
                    entry = candidate;
                    target = candidateTarget;
                    startPosition = candidateStart;
                    observation.seed = seed;
                    observation.enemyInstanceId = candidate.EnemyInstanceId;
                    observation.selectedSpawnGrid = candidate.GridPosition;
                    observation.selectedAttackStart = startPosition;
                    break;
                }
                if (entry != null) break;
                if (!plan.Entries.Any(item => item.EnemyId == observation.enemyId &&
                    item.EliteAffix == CindarsHope.Enemy.EliteAffix.None))
                    observation.selectionRejections.Add(new TtkSelectionRejection
                    { seed = seed, reason = "no natural Spore Imp entry with EliteAffix.None" });
            }
            if (entry == null)
            {
                observation.reason = "No reachable natural Common Spore Imp in fixed seed candidates";
                WriteTtkObservation(observation);
                Assert.Fail(observation.reason);
            }
            Assert.That(target.GetComponent<EnemyBrain>()?.HasEliteClassification, Is.False);

            player = PlayerController.ActiveInstance;
            var body = player != null ? player.GetComponent<Rigidbody2D>() : null;
            var attack = player != null ? player.GetComponent<PlayerAttackController>() : null;
            var skill = ActiveSkillExecutionController.Instance;
            var stamina = GameBootstrap.Instance?.StaminaManager as CindarsHope.Player.StaminaManager;
            var mana = GameBootstrap.Instance?.ManaManager as CindarsHope.Player.ManaManager;
            var playerManager = GameBootstrap.Instance?.PlayerManager as CindarsHope.Player.PlayerManager;
            Assert.That(body, Is.Not.Null);
            Assert.That(attack, Is.Not.Null);
            Assert.That(skill, Is.Not.Null);
            Assert.That(stamina, Is.Not.Null);
            Assert.That(mana, Is.Not.Null);
            Assert.That(playerManager, Is.Not.Null);
            var equippedInstanceId = EquipmentManager.Instance?.GetEquippedItem(EquipmentSlot.RightHand);
            Assert.That(ItemInstanceIdUtility.GetItemId(equippedInstanceId),
                Is.EqualTo(observation.equipmentItemId), "A espada comprada não continuou equipada na Cave.");
            Assert.That(_items.TryGetById(observation.equipmentItemId, out var equippedItemData), Is.True);
            observation.weaponActionId = equippedItemData.WeaponId;
            Assert.That(observation.weaponActionId, Is.Not.Null.And.Not.Empty);
            Assert.That(combatRegistry.WeaponDatabase, Is.Not.Null);
            Assert.That(combatRegistry.WeaponDatabase.TryGetById(observation.weaponActionId,
                out var equippedWeapon), Is.True);
            Assert.That(equippedWeapon, Is.Not.Null);
            Assert.That(equippedWeapon.Range, Is.GreaterThan(0f));
            yield return new WaitForFixedUpdate();
            body.position = startPosition;
            player.transform.position = startPosition;
            Physics2D.SyncTransforms();
            var otherEnemyDistances = EnemyHealth.ActiveInstances
                .Where(health => health != null && health != target && !health.IsDead &&
                    health.gameObject.scene == target.gameObject.scene)
                .Select(health => Vector2.Distance(startPosition, health.transform.position))
                .ToArray();
            observation.nearestOtherEnemyDistanceAtStart = otherEnemyDistances.Length > 0
                ? otherEnemyDistances.Min() : -1f;
            observation.otherEnemiesWithinEightUnitsAtStart =
                otherEnemyDistances.Count(distance => distance <= 8f);
            stamina.AddStamina(stamina.MaxStamina);
            observation.hpInitial = target.CurrentHp;
            observation.hpMax = target.MaxHp;
            observation.playerHpInitial = playerManager.CurrentHP;
            observation.staminaInitial = stamina.CurrentStamina;
            observation.manaInitial = mana.CurrentMana;
            if (observation.hpInitial <= 0 || observation.hpInitial != observation.hpMax)
            {
                observation.reason = "Target HP was not full before the first action";
                WriteTtkObservation(observation);
                Assert.Fail(observation.reason);
            }

            var keys = new TtkControlledKeys();
            var previousMovementReader = player.KeyReader;
            var previousAttackReader = attack.KeyReader;
            player.KeyReader = keys;
            attack.KeyReader = keys;

            float startedAt = Time.time, startedRealtime = Time.realtimeSinceStartup;
            float firstPlayerHitAt = -1f, killedAt = -1f;
            var hitTokens = new HashSet<string>();
            int lastObservedTargetHp = observation.hpInitial;
            Action<DamageAppliedEvent> onDamage = evt =>
            {
                var damage = evt?.DamageResult;
                if (damage == null || damage.TargetInstanceId != observation.enemyInstanceId) return;
                var token = damage.ActionToken ?? string.Empty;
                bool skillHit = token.StartsWith(observation.actionId + ":", StringComparison.Ordinal);
                bool playerMelee = damage.SourceKind == DamageSourceKind.PlayerMelee;
                bool weaponHit = token.StartsWith("melee:", StringComparison.Ordinal) && playerMelee;
                int currentHp = target != null ? target.CurrentHp : 0;
                int effectiveDamage = Mathf.Max(0, lastObservedTargetHp - currentHp);
                lastObservedTargetHp = currentHp;
                if (playerMelee && firstPlayerHitAt < 0f) firstPlayerHitAt = Time.time;
                if (!string.IsNullOrEmpty(token) && playerMelee) hitTokens.Add(token);
                if (skillHit && playerMelee) { observation.skillHits++; observation.skillDamage += effectiveDamage; }
                else if (weaponHit) { observation.weaponHits++; observation.weaponDamage += effectiveDamage; }
                else { observation.externalHits++; observation.externalDamage += effectiveDamage; }
                observation.damageRows.Add(new TtkDamageRow
                {
                    secondsFromFirstHit = Time.time,
                    sourceKind = damage.SourceKind.ToString(),
                    actionToken = token,
                    damage = effectiveDamage,
                    rawDamage = damage.FinalDamage,
                    hpAfter = currentHp
                });
            };
            Action<EnemyKilledEvent> onKilled = evt =>
            {
                if (evt.EnemyInstanceId == observation.enemyInstanceId) killedAt = Time.time;
            };
            Action<SkillActionResolvedEvent> onSkill = evt =>
            {
                if (evt.ActionId != observation.actionId) return;
                if (evt.Outcome == SkillActionResolvedEvent.Committed) observation.skillCommitted++;
                else if (evt.Outcome == SkillActionResolvedEvent.Refused) observation.skillRefused++;
            };
            Action<PlayerOffensiveActionCommittedEvent> onWeapon = evt =>
            {
                if (evt.SourceKind == "Melee" && evt.ActionId == observation.weaponActionId)
                    observation.weaponCommitted++;
            };
            Action<PlayerDamagedEvent> onPlayerDamaged = evt =>
            {
                if (evt == null || evt.DamageAmount <= 0) return;
                observation.incomingDamage += evt.DamageAmount;
                if (evt.SourceId != observation.enemyId)
                    observation.otherEnemyIncomingDamage += evt.DamageAmount;
                observation.incomingDamageRows.Add(new TtkIncomingDamageRow
                {
                    secondsFromStart = Time.time - startedAt,
                    sourceId = evt.SourceId,
                    damage = evt.DamageAmount
                });
            };
            GameEventBus.Subscribe(onDamage);
            GameEventBus.Subscribe(onKilled);
            GameEventBus.Subscribe(onSkill);
            GameEventBus.Subscribe(onWeapon);
            GameEventBus.Subscribe(onPlayerDamaged);
            float nextSkillAt = Time.time, nextWeaponAt = Time.time + 0.85f;
            try
            {
                while (Time.realtimeSinceStartup - startedRealtime < 35f && killedAt < 0f)
                {
                    if (SceneManager.GetActiveScene().name != "CaveScene" || target == null ||
                        !target.gameObject.scene.isLoaded || player == null || playerManager.CurrentHP <= 0)
                    {
                        observation.outcome = "INVALID";
                        observation.reason = playerManager.CurrentHP <= 0
                            ? "Player defeated before matching kill"
                            : "Scene, player or target lost before matching kill";
                        break;
                    }

                    // The next Update reads these keys; the controller's FixedUpdate applies
                    // its normal physics, movement locks and facing rules.
                    var targetPosition = (Vector2)target.transform.position;
                    var delta = targetPosition - body.position;
                    keys.Movement = delta.magnitude > 0.85f ? delta.normalized : Vector2.zero;
                    if (Time.time >= nextSkillAt)
                    {
                        skill.TryUseSlot(0);
                        nextSkillAt = Time.time + 2.5f;
                    }
                    if (keys.RightHeld)
                    {
                        var facing = player.LastFacingDirection;
                        var queryCenter = (Vector2)player.transform.position + facing * 0.5f;
                        var candidates = Physics2D.OverlapCircleAll(queryCenter, equippedWeapon.Range);
                        var enemyLayerCandidates = Physics2D.OverlapCircleAll(queryCenter,
                            equippedWeapon.Range, LayerMask.GetMask("Enemy"));
                        var candidateEnemies = candidates
                            .Select(collider => collider != null
                                ? collider.GetComponentInParent<EnemyHealth>() : null)
                            .Where(health => health != null && !health.IsDead)
                            .Distinct()
                            .ToArray();
                        var targetCollider = target.GetComponent<Collider2D>();
                        var towardTarget = ((Vector2)target.transform.position - (Vector2)player.transform.position);
                        var solidHit = Physics2D.Raycast(player.transform.position,
                            towardTarget.normalized, towardTarget.magnitude,
                            LayerMask.GetMask("WorldSolid"));
                        var physicalHits = Physics2D.RaycastAll(player.transform.position,
                            towardTarget.normalized, towardTarget.magnitude);
                        var anyPhysicalHit = physicalHits
                            .FirstOrDefault(hit => hit.collider != null && !hit.collider.isTrigger &&
                                hit.collider.gameObject != player.gameObject &&
                                hit.collider.GetComponentInParent<EnemyHealth>() != target);
                        observation.weaponAttempts.Add(new TtkWeaponAttemptRow
                        {
                            secondsFromStart = Time.time - startedAt,
                            playerPosition = player.transform.position,
                            targetPosition = target.transform.position,
                            facing = facing,
                            moveInput = player.MoveInput,
                            targetDistance = Vector2.Distance(player.transform.position, target.transform.position),
                            targetColliderGap = targetCollider != null
                                ? Vector2.Distance(queryCenter, targetCollider.ClosestPoint(queryCenter)) : -1f,
                            queryCenter = queryCenter,
                            queryRadius = equippedWeapon.Range,
                            queryCandidates = candidateEnemies.Length,
                            targetInQuery = candidateEnemies.Contains(target),
                            enemyLayerCandidates = enemyLayerCandidates.Length,
                            targetInEnemyLayerQuery = enemyLayerCandidates.Any(collider =>
                                collider != null && collider.GetComponentInParent<EnemyHealth>() == target),
                            targetRootColliderLayer = targetCollider != null ? targetCollider.gameObject.layer : -1,
                            worldSolidBetweenPlayerAndTarget = solidHit.collider != null,
                            firstWorldSolidHitDistance = solidHit.collider != null ? solidHit.distance : -1f,
                            firstBlockingColliderName = anyPhysicalHit.collider != null
                                ? anyPhysicalHit.collider.name : string.Empty,
                            firstBlockingColliderLayer = anyPhysicalHit.collider != null
                                ? anyPhysicalHit.collider.gameObject.layer : -1,
                            wallTileBetweenPlayerAndTarget = physicalHits.Any(hit =>
                                hit.collider != null && hit.collider.name.StartsWith("WallTile_", StringComparison.Ordinal)),
                            movementLocked = player.MovementLocked,
                            castActive = skill.HasActiveCast
                        });
                        // The real attack occurs next Update, on E up, after the charge tracker.
                        keys.SetRightHeld(false);
                        nextWeaponAt = Time.time + 0.65f;
                    }
                    else if (Time.time >= nextWeaponAt && !skill.HasActiveCast)
                        keys.SetRightHeld(true);
                    else
                        keys.SetRightHeld(false);
                    // Death can pause timeScale; yield a frame so the recorder can observe
                    // player HP=0 and exit instead of waiting forever for FixedUpdate.
                    yield return null;
                }
                // Deliver any pending E-up through the real Update before restoring the
                // ordinary reader, so the charge tracker never keeps a held key.
                if (keys.RightHeld)
                {
                    keys.SetRightHeld(false);
                    yield return null;
                }
            }
            finally
            {
                keys.Movement = Vector2.zero;
                keys.SetRightHeld(false);
                if (player != null) player.KeyReader = previousMovementReader;
                if (attack != null) attack.KeyReader = previousAttackReader;
                GameEventBus.Unsubscribe(onDamage);
                GameEventBus.Unsubscribe(onKilled);
                GameEventBus.Unsubscribe(onSkill);
                GameEventBus.Unsubscribe(onWeapon);
                GameEventBus.Unsubscribe(onPlayerDamaged);
            }

            observation.elapsedSeconds = Time.time - startedAt;
            observation.hpFinal = target != null ? target.CurrentHp : 0;
            observation.playerHpFinal = playerManager.CurrentHP;
            observation.staminaFinal = stamina.CurrentStamina;
            observation.manaFinal = mana.CurrentMana;
            observation.whiffs = Mathf.Max(0,
                observation.skillCommitted + observation.weaponCommitted - hitTokens.Count);
            foreach (var row in observation.damageRows)
                row.secondsFromFirstHit = firstPlayerHitAt >= 0f ? row.secondsFromFirstHit - firstPlayerHitAt : -1f;
            if (killedAt >= firstPlayerHitAt && firstPlayerHitAt >= 0f &&
                observation.externalHits == 0 && observation.incomingDamage == 0 &&
                observation.skillHits > 0 &&
                observation.weaponHits > 0 && hitTokens.Count >= 2 &&
                observation.hpInitial - observation.hpFinal ==
                observation.skillDamage + observation.weaponDamage)
            {
                observation.ttkSeconds = killedAt - firstPlayerHitAt;
                observation.outcome = "MEASURED_DIAGNOSTIC";
                observation.reason = "Single scripted-chase kill; insufficient sample for AC02 median";
            }
            else if (observation.outcome != "INVALID")
            {
                observation.outcome = observation.externalHits > 0 ||
                    observation.incomingDamage > 0 || killedAt >= 0f ? "INVALID" : "NOT RUN";
                observation.reason = observation.externalHits > 0
                    ? "External damage confounded target HP"
                    : observation.incomingDamage > 0
                        ? "Player damage has no source instance ID; isolated target cannot be proven"
                    : killedAt >= 0f ? "Kill lacked both action sources or HP reconciliation"
                    : "Timeout before matching kill";
            }
            WriteTtkObservation(observation);
            Assert.That(observation.outcome, Is.EqualTo("MEASURED_DIAGNOSTIC"), observation.reason);
            yield return SceneManager.LoadSceneAsync("FarmScene", LoadSceneMode.Single);
            yield return null;
        }

        private static bool TryFindClearTtkStart(CaveGeneratedLevel level,
            IReadOnlyList<SerializedEnvironmentElement> elements, CaveEnemySpawnPlanEntry entry,
            EnemyHealth target, Collider2D playerCollider, out Vector2 start, out string reason,
            out float nearestOtherEnemyDistance)
        {
            start = default;
            reason = string.Empty;
            nearestOtherEnemyDistance = -1f;
            var targetCollider = target.GetComponent<Collider2D>();
            if (targetCollider == null || !targetCollider.enabled || playerCollider == null ||
                !playerCollider.enabled || level.WalkableTiles == null)
            { reason = "target/player collider or walkable map absent"; return false; }

            // The old seed puts this spawn directly inside cave_elem_20_58_41_DecorBlocking.
            // Exclude adjacent blocking decor as well because a one-cell collider reaches
            // the player footprint and the sword query near the nominal spawn cell.
            var nearDecor = elements.FirstOrDefault(element => element != null &&
                element.Kind == (int)CaveEnvironmentElementKind.DecorBlocking &&
                Mathf.Abs(element.GridX - entry.GridPosition.x) <= 1 &&
                Mathf.Abs(element.GridY - entry.GridPosition.y) <= 1);
            if (nearDecor != null)
            { reason = "blocking decor near spawn: " + nearDecor.ElementId; return false; }

            var targetBounds = targetCollider.bounds;
            if (Physics2D.OverlapBoxAll(targetBounds.center, targetBounds.size * 0.9f, 0f)
                .Any(collider => IsTtkObstacle(collider, playerCollider, target)))
            { reason = "target overlaps live blocking collider"; return false; }

            var targetPosition = (Vector2)target.transform.position;
            var footprint = playerCollider.bounds.size;
            var otherEnemies = EnemyHealth.ActiveInstances
                .Where(health => health != null && health != target && !health.IsDead &&
                    health.gameObject.scene == target.gameObject.scene)
                .ToArray();
            var directions = new[] { Vector2.left, Vector2.right, Vector2.down, Vector2.up };
            foreach (var direction in directions)
            {
                var candidate = targetPosition + direction * 1.1f;
                var cell = new Vector2Int(Mathf.RoundToInt(candidate.x + level.Width * 0.5f),
                    Mathf.RoundToInt(candidate.y + level.Height * 0.5f));
                if (!level.WalkableTiles.Contains(cell)) continue;
                if (elements.Any(element => element != null &&
                    element.Kind == (int)CaveEnvironmentElementKind.DecorBlocking &&
                    element.GridX == cell.x && element.GridY == cell.y)) continue;
                if (Physics2D.OverlapBoxAll(candidate, footprint * 0.9f, 0f)
                    .Any(collider => collider != null &&
                        (collider.GetComponentInParent<EnemyHealth>() == target ||
                        IsTtkObstacle(collider, playerCollider, target)))) continue;
                var line = Physics2D.LinecastAll(candidate, targetPosition);
                if (line.Any(hit => IsTtkObstacle(hit.collider, playerCollider, target))) continue;
                var nearest = otherEnemies.Length == 0 ? float.PositiveInfinity :
                    otherEnemies.Min(health => Vector2.Distance(candidate, health.transform.position));
                if (nearest <= TtkIsolationRadius)
                {
                    if (nearestOtherEnemyDistance < 0f || nearest < nearestOtherEnemyDistance)
                        nearestOtherEnemyDistance = nearest;
                    continue;
                }
                start = candidate;
                nearestOtherEnemyDistance = otherEnemies.Length == 0 ? -1f : nearest;
                return true;
            }

            reason = nearestOtherEnemyDistance >= 0f
                ? $"other enemy within {TtkIsolationRadius:R} units of clear attack start " +
                  $"(nearest {nearestOtherEnemyDistance:R})"
                : "no walkable attack start with clear live collider footprint and line";
            return false;
        }

        private static bool IsTtkObstacle(Collider2D collider, Collider2D playerCollider,
            EnemyHealth target)
        {
            return collider != null && collider.enabled && !collider.isTrigger &&
                collider != playerCollider && collider.GetComponentInParent<EnemyHealth>() != target &&
                collider.GetComponentInParent<PlayerController>() == null;
        }

        private string FormatRow(string label, int level, string actionId,
            string capstoneNodeId, string variantId, bool equipmentRequired = true)
        {
            var actionNodeId = FindActionNode(actionId);
            var treeId = _skills.NodeIndex[actionNodeId].TreeId;
            int basePoints = PlayerProgressionRules.CalculateTotalSkillPointsAtLevel(level);
            int actBonus = level >= Phase21BuildManifest.Level50 ? 2 : 0;
            int capstoneRank = string.IsNullOrEmpty(capstoneNodeId) ? 0 : _skills.GetRank(capstoneNodeId);
            return $"{label}: base={basePoints}, actBonus={actBonus}, spentTree={_skills.GetPointsSpentInTree(treeId)}, " +
                $"unspent={_progression.UnspentSkillPoints}, action={actionId}/R{_skills.GetRank(actionNodeId)}, " +
                $"capstone={capstoneNodeId ?? "none"}/R{capstoneRank}, variant={variantId ?? "none"}, " +
                $"gearInstance={_equipment.GetEquippedItem(EquipmentSlot.RightHand)}, gearRequired={equipmentRequired}, " +
                $"slot0={_skills.State.GetActiveSlotSkillActionId(0)}";
        }

        private string FindNotYetExecutablePrerequisite(string rootNodeId)
        {
            var pending = new Stack<string>();
            var seen = new HashSet<string>();
            pending.Push(rootNodeId);
            while (pending.Count > 0)
            {
                var id = pending.Pop();
                if (!seen.Add(id) || !_skills.NodeIndex.TryGetValue(id, out var node)) continue;
                if (node.NotYetExecutable) return id;
                foreach (var prerequisite in node.PrerequisiteNodeIds ?? new List<string>())
                    pending.Push(prerequisite);
            }
            return null;
        }

        private void ResetStage(int level)
        {
            _inventory.RestoreFromSaveData(_originalInventory);
            _equipment.RestoreFromSaveData(_originalEquipment);
            var state = _progression.CaptureSaveData();
            state.TotalXp = ProgressionCurve.TotalXpForLevel(level);
            state.UnspentSkillPoints = PlayerProgressionRules.CalculateTotalSkillPointsAtLevel(level);
            _progression.RestoreFromSaveData(state);
            _skills.RestoreFromSaveData(new SkillTreeSaveData(), level);
            QuestRuntimeBootstrap.RestoreFromSaveData(new QuestStateSectionSaveData());
            Assert.That(_progression.Level, Is.EqualTo(level));
        }

        private bool TryEquipFixture(string itemId, out string reason)
        {
            reason = string.Empty;
            if (string.IsNullOrEmpty(itemId) || !_items.TryGetById(itemId, out var item) ||
                !EquipmentSlotRules.IsCompatibleWithEquipmentSlot(item, EquipmentSlot.RightHand))
            { reason = "item não existe ou não é compatível com RightHand"; return false; }

            var before = new HashSet<string>(_inventory.Slots
                .Where(slot => slot != null && slot.ItemId == itemId && !slot.IsEmpty)
                .Select(slot => slot.EffectiveItemInstanceId));
            if (!_inventory.TryAddItem(itemId, 1).Success)
            { reason = "inventário recusou adicionar item da fixture"; return false; }

            for (int index = 0; index < _inventory.Slots.Count; index++)
            {
                if (!_inventory.TryGetSlot(index, out var slot) || slot.IsEmpty ||
                    slot.ItemId != itemId || before.Contains(slot.EffectiveItemInstanceId)) continue;
                var previous = _equipment.GetEquippedItem(EquipmentSlot.RightHand);
                if (!string.IsNullOrEmpty(previous))
                    _inventory.ClearEquippedBinding(EquipmentSlot.RightHand, previous);
                _equipment.EquipItem(EquipmentSlot.RightHand, slot.EffectiveItemInstanceId);
                if (_equipment.GetEquippedItem(EquipmentSlot.RightHand) != slot.EffectiveItemInstanceId ||
                    !_inventory.MarkSlotEquipped(index, EquipmentSlot.RightHand))
                { reason = "equipamento ou binding de inventário recusado"; return false; }
                if (!_inventory.TryGetSlot(index, out var equippedSlot) || !equippedSlot.IsEquipped)
                { reason = "slot de inventário não persistiu como equipado"; return false; }
                return true;
            }

            reason = "instância nova do item não foi encontrada no inventário";
            return false;
        }

        private bool TryAwardActs(out string reason)
        {
            reason = string.Empty;
            var service = QuestRuntimeBootstrap.QuestService;
            if (service == null) { reason = "QuestService não está disponível"; return false; }
            // Controlled stage setup: award the two canonical finale IDs through the same
            // idempotent service used by the bridge, without mutating Fonte/scene state.
            var act1 = QuestMainActsIds.Finales[0].ActIdForSkillPoint;
            var act2 = QuestMainActsIds.Finales[1].ActIdForSkillPoint;
            if (!service.TryAwardActSkillPoint(act1) || !service.TryAwardActSkillPoint(act2))
            { reason = "recompensa canônica dos Atos 1/2 indisponível"; return false; }
            var questState = QuestRuntimeBootstrap.CaptureSaveData();
            if (questState == null ||
                !questState.RewardedMainActIds.Contains(act1) ||
                !questState.RewardedMainActIds.Contains(act2))
            { reason = "recompensas dos Atos 1/2 não persistiram"; return false; }
            int expected = PlayerProgressionRules.CalculateTotalSkillPointsAtLevel(_progression.Level) + 2;
            if (_progression.UnspentSkillPoints != expected)
            { reason = $"orçamento após Atos incorreto: {_progression.UnspentSkillPoints} != {expected}"; return false; }
            QuestRuntimeBootstrap.RestoreFromSaveData(questState);
            if (service.TryAwardActSkillPoint(act1) || service.TryAwardActSkillPoint(act2) ||
                _progression.UnspentSkillPoints != expected)
            { reason = "recompensa duplicou após restaurar quest save"; return false; }
            return true;
        }

        private bool TryPurchaseAction(string actionId, int level, int rank, out string reason)
        {
            reason = string.Empty;
            var nodeId = FindActionNode(actionId);
            if (nodeId == null) { reason = "ação sem nó autorado"; return false; }
            if (!PurchaseWithPrerequisites(nodeId, level, null, new HashSet<string>(), out reason)) return false;
            if (rank >= Phase21BuildManifest.Rank3 &&
                !FillTreeToTierTwo(_skills.NodeIndex[nodeId].TreeId, nodeId, level, out reason)) return false;
            if (!RankTo(nodeId, rank, out reason)) return false;
            if (!_skills.TryAssignActiveSlot(0, actionId))
            { reason = "ação comprada não pôde ocupar slot ativo"; return false; }
            return true;
        }

        private bool FillTreeToTierTwo(string treeId, string protectedNodeId, int level, out string reason)
        {
            reason = string.Empty;
            while (_skills.GetPointsSpentInTree(treeId) < SkillTierRules.TierPointThresholds[2])
            {
                bool spent = false;
                foreach (var filler in _skills.NodeIndex.Values
                    .Where(n => n.TreeId == treeId && n.Tier == 1 && !n.IsCapstone &&
                        n.SkillNodeId != protectedNodeId && _skills.GetRank(n.SkillNodeId) == 0)
                    .OrderBy(n => n.SkillNodeId))
                {
                    if (!_skills.TryPurchaseNode(filler.SkillNodeId, level, null, out _)) continue;
                    spent = true;
                    break;
                }
                if (!spent)
                {
                    foreach (var filler in _skills.NodeIndex.Values
                        .Where(n => n.TreeId == treeId && n.Tier == 1 && !n.IsCapstone &&
                            n.SkillNodeId != protectedNodeId && _skills.GetRank(n.SkillNodeId) > 0)
                        .OrderBy(n => n.SkillNodeId))
                    {
                        if (!_skills.TryRankUpNode(filler.SkillNodeId, out _)) continue;
                        spent = true;
                        break;
                    }
                }
                if (!spent)
                { reason = "não há gasto legal suficiente para abrir rank 3"; return false; }
            }
            return true;
        }

        private bool TryPurchaseBuild(Phase21BuildDefinition build, string actionId, int level, int capstoneRank, out string reason)
        {
            reason = string.Empty;
            if (!TryPurchaseAction(actionId, level, Phase21BuildManifest.Rank1, out reason)) return false;
            if (!PurchaseWithPrerequisites(build.CapstoneNodeId, level, build.VariantId, new HashSet<string>(), out reason)) return false;
            if (!RankTo(build.CapstoneNodeId, capstoneRank, out reason)) return false;
            if (!_skills.TryAssignActiveSlot(0, actionId)) { reason = "slot ativo recusado"; return false; }
            Assert.That(_skills.GetChosenVariant(build.CapstoneNodeId), Is.EqualTo(build.VariantId));
            return true;
        }

        private static string SupportActionId(string capstoneNodeId)
        {
            switch (capstoneNodeId)
            {
                case "survival_capstone_caveborn": return "skill_survival_sinal_retirada";
                case "crafting_capstone_master_artisan": return "skill_crafting_marca_eficiencia";
                default: return null;
            }
        }

        private bool PurchaseWithPrerequisites(string nodeId, int level, string variant, HashSet<string> visiting, out string reason)
        {
            reason = string.Empty;
            if (_skills.GetRank(nodeId) > 0) return true;
            if (!_skills.NodeIndex.TryGetValue(nodeId, out var node)) { reason = "nó ausente"; return false; }
            if (!visiting.Add(nodeId)) { reason = "ciclo de pré-requisitos"; return false; }
            try
            {
                foreach (var prerequisite in node.PrerequisiteNodeIds ?? new List<string>())
                    if (!PurchaseWithPrerequisites(prerequisite, level, variant, visiting, out reason)) return false;
                for (int attempt = 0; attempt < _skills.NodeIndex.Count * SkillTierRules.AbsoluteRankCap; attempt++)
                {
                    if (_skills.TryPurchaseNode(nodeId, level, node.CapstoneVariants?.Count > 0 ? variant : null, out var feedback)) return true;
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
                reason = "limite de tentativas"; return false;
            }
            finally { visiting.Remove(nodeId); }
        }

        private bool RankTo(string nodeId, int rank, out string reason)
        {
            reason = string.Empty;
            for (int i = 1; i < rank; i++) if (!_skills.TryRankUpNode(nodeId, out reason)) return false;
            return true;
        }
        private void RoundTrip(string label, int level, string actionId, string nodeId, int rank)
        {
            var snapshot = _skills.CaptureSaveData();
            _skills.RestoreFromSaveData(snapshot, level);
            Assert.That(_skills.GetRank(nodeId), Is.EqualTo(rank), label + " após load");
            Assert.That(_skills.State.GetActiveSlotSkillActionId(0), Is.EqualTo(actionId), label + " slot após load");
        }
        private string FindActionNode(string actionId) => _skills.NodeIndex.Values.FirstOrDefault(n => n.UnlockedSkillActionId == actionId)?.SkillNodeId;

        private static T FindInActiveScene<T>() where T : Component
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                var component = root.GetComponentInChildren<T>(true);
                if (component != null) return component;
            }
            return null;
        }

        private static void WriteTtkObservation(TtkObservation observation)
        {
            var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "phase21_combat_probe");
            Directory.CreateDirectory(outputDir);
            var stem = "phase21_common_whirl_r1_ttk_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
            var jsonPath = Path.Combine(outputDir, stem + ".json");
            var csvPath = Path.Combine(outputDir, stem + ".csv");
            File.WriteAllText(jsonPath, JsonUtility.ToJson(observation, true));
            using (var writer = new StreamWriter(csvPath))
            {
                writer.WriteLine("recordType,scenarioId,seed,actionId,actionRank,equipmentItemId,weaponActionId,enemyId,enemyInstanceId,creatureClass,classSource,eliteAffix,movementMode,commandPolicy,outcome,reason,hpInitial,hpMax,hpFinal,playerHpInitial,playerHpFinal,staminaInitial,staminaFinal,manaInitial,manaFinal,skillCommitted,skillRefused,weaponCommitted,skillHits,weaponHits,externalHits,skillDamage,weaponDamage,externalDamage,whiffs,ttkSeconds,elapsedSeconds,hitSeconds,sourceKind,actionToken,hitDamage,rawHitDamage,hitHpAfter");
                writer.WriteLine(string.Join(",", "SUMMARY", TtkCsv(observation.scenarioId),
                    TtkCsv(observation.seed), TtkCsv(observation.actionId), observation.actionRank,
                    TtkCsv(observation.equipmentItemId), TtkCsv(observation.weaponActionId),
                    TtkCsv(observation.enemyId),
                    TtkCsv(observation.enemyInstanceId), TtkCsv(observation.creatureClass),
                    TtkCsv(observation.classSource), TtkCsv(observation.eliteAffix),
                    TtkCsv(observation.movementMode), TtkCsv(observation.commandPolicy),
                    TtkCsv(observation.outcome),
                    TtkCsv(observation.reason), observation.hpInitial, observation.hpMax,
                    observation.hpFinal, observation.playerHpInitial, observation.playerHpFinal,
                    observation.staminaInitial, observation.staminaFinal, observation.manaInitial,
                    observation.manaFinal, observation.skillCommitted, observation.skillRefused,
                    observation.weaponCommitted, observation.skillHits, observation.weaponHits,
                    observation.externalHits, observation.skillDamage, observation.weaponDamage,
                    observation.externalDamage, observation.whiffs,
                    observation.ttkSeconds.ToString("R", System.Globalization.CultureInfo.InvariantCulture),
                    observation.elapsedSeconds.ToString("R", System.Globalization.CultureInfo.InvariantCulture),
                    "", "", "", "", "", ""));
                foreach (var row in observation.damageRows)
                    writer.WriteLine(string.Join(",", "DAMAGE", TtkCsv(observation.scenarioId),
                        TtkCsv(observation.seed), TtkCsv(observation.actionId), observation.actionRank,
                        TtkCsv(observation.equipmentItemId), TtkCsv(observation.weaponActionId),
                        TtkCsv(observation.enemyId),
                        TtkCsv(observation.enemyInstanceId), TtkCsv(observation.creatureClass),
                        TtkCsv(observation.classSource), TtkCsv(observation.eliteAffix),
                        TtkCsv(observation.movementMode), TtkCsv(observation.commandPolicy),
                        TtkCsv(observation.outcome),
                        TtkCsv(observation.reason), "", "", "", "", "", "", "", "", "",
                        "", "", "", "", "", "", "", "", "", "", "", "",
                        row.secondsFromFirstHit.ToString("R", System.Globalization.CultureInfo.InvariantCulture),
                        TtkCsv(row.sourceKind), TtkCsv(row.actionToken), row.damage,
                        row.rawDamage, row.hpAfter));
            }
            Debug.Log($"Phase21 TTK diagnostic JSON: {jsonPath}; CSV: {csvPath}; outcome: {observation.outcome}");
        }

        private static string TtkCsv(string value)
        {
            var safe = value ?? string.Empty;
            return "\"" + safe.Replace("\"", "\"\"") + "\"";
        }

        private static void WriteSkillHitObservation(SkillHitObservation observation)
        {
            var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "phase21_combat_probe");
            Directory.CreateDirectory(outputDir);
            var stem = "phase21_common_whirl_hit_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
            File.WriteAllText(Path.Combine(outputDir, stem + ".json"),
                JsonUtility.ToJson(observation, true));
            File.WriteAllText(Path.Combine(outputDir, stem + ".csv"),
                "scenarioId,seed,actionId,enemyId,enemyInstanceId,creatureClass,eliteAffix,outcome,ttkOutcome,hpBefore,hpAfter,skillHitCount,skillDamage,externalDamageCount,committedCount,actionToken,distanceBeforeCast,distanceAfterCast,physicsOverlapsBeforeCast,targetCollidersBeforeCast\n" +
                string.Join(",", observation.scenarioId, observation.seed, observation.actionId,
                    observation.enemyId, observation.enemyInstanceId, observation.creatureClass,
                    observation.eliteAffix, observation.outcome, observation.ttkOutcome,
                    observation.hpBefore, observation.hpAfter, observation.skillHitCount,
                    observation.skillDamage, observation.externalDamageCount,
                    observation.committedCount, observation.actionToken,
                    observation.distanceBeforeCast, observation.distanceAfterCast,
                    observation.physicsOverlapsBeforeCast, observation.targetCollidersBeforeCast) + "\n");
        }
    }
}
