using CindarsHope.NPC;

namespace CindarsHope.Core.Events
{
    /// <summary>
    /// Publicado quando um no ou uma escolha (resposta) de dialogo forca uma feicao especifica do
    /// retrato do NPC, sobrepondo a feicao derivada da afinidade. Consumido por
    /// NpcInteractionPortraitHud; o override vale ate o fim da interacao (NpcInteractionEndedEvent)
    /// ou ate outro override. Imutavel; tipos simples + enum + id estavel.
    /// </summary>
    public readonly struct NpcExpressionOverrideEvent
    {
        public readonly string NpcId;
        public readonly NpcExpression Expression;

        public NpcExpressionOverrideEvent(string npcId, NpcExpression expression)
        {
            NpcId = npcId;
            Expression = expression;
        }
    }
}
