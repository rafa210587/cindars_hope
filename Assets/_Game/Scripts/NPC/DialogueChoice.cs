using System;
using UnityEngine;

namespace CindarsHope.NPC
{
    public enum DialogueActionType
    {
        None,
        OpenShop,
        OfferQuest,   // ActionPayload = questId
        CloseDialogue
    }

    [Serializable]
    public class DialogueChoice
    {
        public string Label;
        public string NextNodeId;
        public DialogueActionType ActionType = DialogueActionType.None;
        public string ActionPayload;

        // Override opcional de feicao: ao selecionar esta resposta, o NPC reage com ExpressionOverride.
        public bool HasExpressionOverride;
        public NpcExpression ExpressionOverride = NpcExpression.Neutral;
    }
}
