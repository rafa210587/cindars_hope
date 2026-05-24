using System;
using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.NPC
{
    [Serializable]
    public class DialogueNode
    {
        public string NodeId;
        public string Text;
        public List<DialogueChoice> Choices = new();
        public List<string> RandomLinePool = new();
    }
}
