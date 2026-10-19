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

        // fable_28 — optional conditional line pool (season/weather/friendship/festival/flag).
        // Additive: when empty the node behaves exactly as before (Text / RandomLinePool). When
        // populated, DialogueLineSelector picks one deterministically per day; the existing text
        // remains the guaranteed fallback so this never produces an empty line.
        public List<ConditionalDialogueLine> ConditionalLines = new();

        // Override opcional de feicao do retrato: quando HasExpressionOverride, ao exibir este no
        // o NPC mostra ExpressionOverride (sobrepondo a afinidade) ate o fim da interacao.
        public bool HasExpressionOverride;
        public NpcExpression ExpressionOverride = NpcExpression.Neutral;
    }
}
