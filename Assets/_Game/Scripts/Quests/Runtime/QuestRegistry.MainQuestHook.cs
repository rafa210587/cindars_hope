using System.Collections.Generic;
using CindarsHope.Narrative;
using CindarsHope.Quests.Rewards;

namespace CindarsHope.Quests.Runtime
{
    /// <summary>
    /// fable_63 — registro da quest-ponte mq_act1_00 ("Cartas de Cindar's Hope").
    ///
    /// Entra no MESMO QuestRegistry/QuestService (sem segundo fluxo). Objetivo unico TalkToNpc
    /// Corvus; recompensa = flag de conclusao (sem ouro/XP significativo — e quest-ponte).
    /// Categoria Main (source Main, anti-softlock: Main nunca expira).
    ///
    /// Ponte para a cadeia E40: injeta mq_act1_00 como PrerequisiteQuestId da mq_act1_01, de modo
    /// que a oferta do Corvus (cadeia E40) assuma do ponto certo. Diff aditivo e idempotente.
    /// </summary>
    public partial class QuestRegistry
    {
        private void RegisterMainQuestHook()
        {
            var objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    ObjectiveId = NarrativeIds.HookTalkObjectiveId,
                    ObjectiveType = QuestObjectiveType.TalkToNpc,
                    TargetId = NarrativeIds.CorvusNpcId,
                    RequiredAmount = 1
                }
            };

            var definition = new QuestDefinition
            {
                QuestId = NarrativeIds.MainQuestHookId,
                Category = QuestCategory.Main,
                IsMainProgression = true,
                // PLACEHOLDER_LORE — texto provisorio (refinamento Nymirianos/Cindar pendente).
                DisplayName = "Cartas de Cindar's Hope",
                Description = "PLACEHOLDER_LORE: Uma carta na sua cama fala de Corvus, no templo da cidade. " +
                              "Procure-o: parece que ele esperava a sua chegada.",
                Trackable = true
            };

            var rewards = new List<QuestRewardDefinition>
            {
                new QuestRewardDefinition
                {
                    RewardId = "reward_mq_act1_00_flag",
                    RewardType = QuestRewardType.QuestFlagGrant,
                    GrantedFlagId = NarrativeIds.FlagHookComplete,
                    IdempotencyPolicy = RewardIdempotencyPolicy.TrackByFlagId
                }
            };

            Register(definition, objectives, rewards, NarrativeIds.CorvusNpcId);

            // Ponte E40: mq_act1_00 vira prerequisite da mq_act1_01 (se a cadeia ja existe).
            // Aditivo e idempotente (nao duplica se ja presente). Se a cadeia ainda nao existir,
            // o consumo fica documentado para a F10.
            if (_quests.TryGetValue(QuestMainAct1Ids.Quest01FonteAdormecida, out var act1Quest) && act1Quest != null)
            {
                if (act1Quest.PrerequisiteQuestIds == null)
                {
                    act1Quest.PrerequisiteQuestIds = new List<string>();
                }
                if (!act1Quest.PrerequisiteQuestIds.Contains(NarrativeIds.MainQuestHookId))
                {
                    act1Quest.PrerequisiteQuestIds.Add(NarrativeIds.MainQuestHookId);
                }
            }
        }
    }
}
