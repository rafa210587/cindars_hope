using System.Collections.Generic;
using CindarsHope.Foundation;
using CindarsHope.Quests.Flags;
using CindarsHope.Quests.Rewards;
using CindarsHope.Quests.Save;

namespace CindarsHope.Quests.Runtime
{
    /// <summary>
    /// fable_34/fable_51 — registers dynamic (board / cave-contract / secret) quest instances into
    /// the EXISTING QuestRegistry (no parallel registry/flow), maps template id → objective type,
    /// and rebuilds an instance's registry entry from its persisted QuestStateRecord after load.
    /// Also keeps the QuestInstance metadata so QuestService.AcceptQuest can hydrate the richer
    /// dynamic fields (source, template, target, scaled rewards) onto the QuestStateRecord.
    ///
    /// Extracted from QuestService to isolate the dynamic-instance registration concern from
    /// accept/progress/turn-in lifecycle orchestration. Pure C# — no Unity refs.
    /// </summary>
    public sealed class QuestDynamicInstanceRegistrar
    {
        private readonly QuestRegistry _registry;
        private readonly QuestFlagService _flagService;
        private readonly Dictionary<string, QuestInstance> _dynamicInstances = new Dictionary<string, QuestInstance>();

        public QuestDynamicInstanceRegistrar(QuestRegistry registry, QuestFlagService flagService)
        {
            _registry = registry;
            _flagService = flagService;
        }

        public bool TryGetInstance(string questId, out QuestInstance instance) =>
            _dynamicInstances.TryGetValue(questId, out instance);

        public void EnsureRewardFlagsRegistered(IEnumerable<QuestRewardDefinition> rewards)
        {
            if (rewards == null || _flagService == null) return;
            foreach (var reward in rewards)
            {
                if (reward == null || reward.RewardType != QuestRewardType.QuestFlagGrant) continue;
                _flagService.EnsureRewardFlagRegistered(reward.GrantedFlagId ?? reward.TargetId);
            }
        }

        /// <summary>
        /// fable_34 (CA-1/CA-2) — registers a dynamic board/procedural instance into the EXISTING
        /// registry/flow (no second registry). Builds the objective from the template kind and a
        /// scaled Gold reward; XP is applied on turn-in via the progression hook. Returns the
        /// concrete quest id, or null if the instance is invalid.
        /// </summary>
        public string Register(QuestInstance instance)
        {
            if (instance == null || string.IsNullOrEmpty(instance.QuestId) || string.IsNullOrEmpty(instance.TargetId))
                return null;

            _dynamicInstances[instance.QuestId] = instance;
            EnsureRewardFlagsRegistered(instance.AdditionalRewards);

            var objectiveType = ObjectiveTypeForSource(instance);
            var objective = new QuestObjective
            {
                ObjectiveId = $"obj_{instance.QuestId}",
                ObjectiveType = objectiveType,
                TargetId = instance.TargetId,
                RequiredAmount = instance.Quantity < 1 ? 1 : instance.Quantity
            };

            var definition = new QuestDefinition
            {
                QuestId = instance.QuestId,
                Category = SourceToCategory(instance.Source),
                DisplayName = instance.QuestId,
                Description = instance.QuestTemplateId,
                Trackable = true
            };

            var rewards = new List<QuestRewardDefinition>();
            if (instance.RewardGold > 0)
            {
                rewards.Add(new QuestRewardDefinition
                {
                    RewardId = $"reward_{instance.QuestId}_gold",
                    RewardType = QuestRewardType.Gold,
                    Quantity = instance.RewardGold,
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByRewardId
                });
            }

            // fable_51 — additive non-gold rewards (item / flag) flow through the SAME applicator;
            // no parallel reward system. Used by cave contracts (map segment, boss essence, title, charm).
            if (instance.AdditionalRewards != null)
            {
                foreach (var extra in instance.AdditionalRewards)
                {
                    if (extra != null && !string.IsNullOrEmpty(extra.RewardId)) rewards.Add(extra);
                }
            }

            _registry.Register(definition, new List<QuestObjective> { objective }, rewards, instance.QuestTemplateId);
            return instance.QuestId;
        }

        /// <summary>
        /// fable_34 — rebuild a dynamic instance's registry entry from its persisted record so it
        /// rejoins the live flow after load (objective + scaled gold reward reconstructed).
        /// </summary>
        public void ReRegisterFromRecord(QuestStateRecord record)
        {
            var instance = new QuestInstance
            {
                QuestId = record.QuestId,
                QuestTemplateId = record.TemplateId,
                Source = (QuestSource)record.Source,
                TargetId = record.InstanceTargetId,
                Quantity = record.InstanceQuantity,
                QuestLevel = record.QuestLevel,
                RewardGold = record.InstanceRewardGold,
                RewardXp = record.InstanceRewardXp,
                GeneratedForDay = record.GeneratedForDay,
                AdditionalRewards = QuestDynamicInstancePersistence.RestoreRewards(record)
            };
            Register(instance);
        }

        // fable_34/fable_51 — template id → objective type. The instance carries the template id;
        // map by id prefix. Board: bd_cull/bd_gather/bd_delivery. Cave contracts (fable_51):
        // cc_depth_* reach a depth; cc_boss_rematch defeats a boss band; cc_no_hit_floor has no
        // count-based objective (completion is driven by NoHitFloorTracker via CompleteCaveContract).
        private static QuestObjectiveType ObjectiveTypeForSource(QuestInstance instance)
        {
            var template = instance.QuestTemplateId ?? string.Empty;
            if (template.StartsWith("bd_cull")) return QuestObjectiveType.DefeatEnemy;
            if (template.StartsWith("bd_delivery")) return QuestObjectiveType.DeliverItem;
            if (template.StartsWith("cc_depth")) return QuestObjectiveType.ReachCaveDepth;
            if (template.StartsWith("cc_boss_rematch")) return QuestObjectiveType.DefeatEnemy;
            if (template.StartsWith("cc_no_hit")) return QuestObjectiveType.CompleteCaveRun;
            return QuestObjectiveType.CollectItem; // bd_gather and any other gather-like template
        }

        private static QuestCategory SourceToCategory(QuestSource source)
        {
            switch (source)
            {
                case QuestSource.Board: return QuestCategory.FarmOrder;
                case QuestSource.CaveContract: return QuestCategory.CaveContract;
                case QuestSource.CaveSecret: return QuestCategory.Hidden;
                case QuestSource.Main: return QuestCategory.Main;
                case QuestSource.Npc:
                case QuestSource.Mural:
                default: return QuestCategory.Side;
            }
        }
    }
}
