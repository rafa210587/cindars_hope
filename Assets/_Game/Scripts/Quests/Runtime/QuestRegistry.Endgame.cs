using System.Collections.Generic;
using CindarsHope.Quests;
using CindarsHope.Quests.Rewards;

namespace CindarsHope.Quests.Runtime
{
    /// <summary>
    /// fable_43 — main-quest ENDGAME (Act 5) registration: the canonical finale
    /// <c>mq_act4_05_final_choice</c> (catalog id preserved), chained by PrerequisiteQuestId from
    /// <c>mq_act4_04_broken_remembrance</c> (F36). It REUSES the same QuestRegistry pattern as Acts
    /// 1-4 (QuestDefinition + QuestObjective + reward flags) and the same supported runtime objective
    /// handlers (ReachCaveDepth / TalkToNpc). No parallel main-quest flow is created (quest_rules
    /// Rule 1/9): the finale simply extends the existing Main source chain.
    ///
    /// The Hope-fragment integration and the +1 final-act skill point are applied by
    /// MainProgressionQuestBridge on QuestCompletedEvent (the Act 5 entry in Finales,
    /// IntegratesFragment = true) — NOT as generic rewards here. The irreversible Protect/Seal/Use
    /// decision is then executed at the Fonte by FinalChoiceRuntimeAdapter, after Hope is integrated
    /// and level-101 access is unlocked (fonte_rules Rule 7).
    ///
    /// Anti-softlock (CA-2): Category.Main => CanExpire() is false; the quest never expires, and the
    /// objectives (reach the 101 gate depth + return to Corvus) are re-entrable after a defeat.
    /// </summary>
    public partial class QuestRegistry
    {
        // The 101-gate depth marker. The deepest real procedural content is 90 (CreateCaveBossAssets
        // gates), but level 100 is the canonical access threshold to the special level-101 CaveScene
        // (decision v2 4.5-B; EMENDA 2026-06-12-D §3). ReachCaveDepth 100 marks the player has cleared
        // to the gate; the runtime gate (EndgameGate.CanEnterLevel101) governs actual 101 entry.
        private const int Level101GateDepth = 100;

        private void RegisterEndgameAct5()
        {
            // mq_act4_05_final_choice — "A Ultima Pergunta da Fonte". ACT 5 FINALE.
            // Prereq: mq_act4_04_broken_remembrance (Hope prepared/hinted in F36). Objectives: reach the
            // level-100 gate, then return to Corvus to face the Fonte's final question. Hope integration
            // + skill point: MainProgressionQuestBridge. The choice itself: Fonte (FinalChoiceRuntimeAdapter).
            var objectives = new List<QuestObjective>
            {
                ReachDepth("obj_mq4_05_reach_gate_100", Level101GateDepth),
                TalkObj("obj_mq4_05_talk_corvus", QuestMainActsIds.CorvusId)
            };

            var rewards = new List<QuestRewardDefinition>
            {
                new QuestRewardDefinition
                {
                    RewardId = "reward_mq_act5_gold",
                    RewardType = QuestRewardType.Gold,
                    Quantity = 700,
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByRewardId
                },
                new QuestRewardDefinition
                {
                    RewardId = "reward_mq_act5_done_flag",
                    RewardType = QuestRewardType.QuestFlagGrant,
                    GrantedFlagId = QuestMainActsIds.FlagAct5Done,
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByFlagId
                },
                new QuestRewardDefinition
                {
                    RewardId = "reward_mq_act5_complete_flag",
                    RewardType = QuestRewardType.QuestFlagGrant,
                    GrantedFlagId = QuestMainActsIds.FlagAct5Complete,
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByFlagId
                }
            };

            Register(
                MainQuest(QuestMainActsIds.Act4Quest05FinalChoice, "A Ultima Pergunta da Fonte",
                    "A Esperanca, enfim, pode ser integrada. Mas integra-la abre a ultima porta: o nivel 101, onde quatro guardioes esperam e a Fonte fara a pergunta que so voce pode responder. Alcance o portao mais fundo e volte a Corvus: o que vier depois sera para sempre. Proteger, Selar ou Usar.",
                    QuestMainActsIds.Act4Quest04BrokenRemembrance),
                objectives,
                rewards,
                QuestMainActsIds.CorvusId);
        }
    }
}
