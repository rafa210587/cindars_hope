namespace CindarsHope.NPC
{
    /// <summary>
    /// fable_26 — feicao do retrato do NPC derivada da opiniao (-100..+100). Cinco faixas
    /// simetricas: odio / desdem / neutro / contente / amor.
    /// </summary>
    public enum NpcExpression
    {
        Hatred,
        Disdain,
        Neutral,
        Happiness,
        Love
    }

    /// <summary>
    /// fable_26 — resolvedor PURO (sem UnityEngine) da feicao a partir da opiniao, e rotulos PT-BR.
    /// Usado por NpcInteractionPortraitHud. Limiares simetricos sobre o range -100..+100.
    /// </summary>
    public static class NpcExpressionResolver
    {
        // Limiares (inclusivos no lado mais hostil) — bandas: <=-60 odio, <=-20 desdem,
        // (-20,20) neutro, [20,60) contente, >=60 amor.
        public const int HatredAtOrBelow = -60;
        public const int DisdainAtOrBelow = -20;
        public const int HappinessAtOrAbove = 20;
        public const int LoveAtOrAbove = 60;

        public static NpcExpression Resolve(int opinion)
        {
            if (opinion <= HatredAtOrBelow) return NpcExpression.Hatred;
            if (opinion <= DisdainAtOrBelow) return NpcExpression.Disdain;
            if (opinion >= LoveAtOrAbove) return NpcExpression.Love;
            if (opinion >= HappinessAtOrAbove) return NpcExpression.Happiness;
            return NpcExpression.Neutral;
        }

        public static string LabelPtBr(NpcExpression expression)
        {
            switch (expression)
            {
                case NpcExpression.Hatred: return "Ódio";
                case NpcExpression.Disdain: return "Desdém";
                case NpcExpression.Happiness: return "Contente";
                case NpcExpression.Love: return "Amor";
                default: return "Neutro";
            }
        }
    }
}
