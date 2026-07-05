using CindarsHope.Core.Events;

namespace CindarsHope.NPC
{
    public readonly struct ThalindraQuestDialogueDecision
    {
        public readonly string ChoiceLabel;
        public readonly QuestGiverInteractionMode InteractionMode;

        public bool ShowChoice => !string.IsNullOrEmpty(ChoiceLabel);

        public ThalindraQuestDialogueDecision(string choiceLabel, QuestGiverInteractionMode interactionMode)
        {
            ChoiceLabel = choiceLabel;
            InteractionMode = interactionMode;
        }
    }

    /// <summary>Keeps the visible quest option and the emitted interaction mode consistent.</summary>
    public static class ThalindraQuestDialoguePolicy
    {
        public static ThalindraQuestDialogueDecision Resolve(bool hasQuestState, bool canTurnIn)
        {
            if (canTurnIn)
                return new ThalindraQuestDialogueDecision("Entregar suprimentos", QuestGiverInteractionMode.TurnIn);
            if (!hasQuestState)
                return new ThalindraQuestDialogueDecision("! Qual é a tarefa?", QuestGiverInteractionMode.Offer);
            return new ThalindraQuestDialogueDecision(null, QuestGiverInteractionMode.NoQuest);
        }
    }
}
