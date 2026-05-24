using System;
using UnityEngine;

namespace CindarsHope.NPC
{
    public enum DialogueActionType
    {
        None,
        OpenShop,
        CloseDialogue
    }

    [Serializable]
    public class DialogueChoice
    {
        public string Label;
        public string NextNodeId;
        public DialogueActionType ActionType = DialogueActionType.None;
        public string ActionPayload;
    }
}
