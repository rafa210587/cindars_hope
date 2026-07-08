using System.Collections.Generic;

namespace CindarsHope.NPC
{
    /// <summary>
    /// Owns debug-expression choice identifiers, labels and parsing shared by regular NPCs and shop NPCs.
    /// </summary>
    public static class NpcDebugExpressionChoicePolicy
    {
        public const string OpenChoiceId = "dbg_open";
        public const string BackChoiceId = "dbg_back";

        private const string ExpressionChoicePrefix = "dbg:";

        public static IReadOnlyList<NpcShopChoiceDefinition> BuildExpressionChoices()
        {
            return new List<NpcShopChoiceDefinition>
            {
                new NpcShopChoiceDefinition("Neutro", ExpressionChoicePrefix + nameof(NpcExpression.Neutral)),
                new NpcShopChoiceDefinition("Felicidade", ExpressionChoicePrefix + nameof(NpcExpression.Happiness)),
                new NpcShopChoiceDefinition("Amor", ExpressionChoicePrefix + nameof(NpcExpression.Love)),
                new NpcShopChoiceDefinition("Desdem", ExpressionChoicePrefix + nameof(NpcExpression.Disdain)),
                new NpcShopChoiceDefinition("Odio", ExpressionChoicePrefix + nameof(NpcExpression.Hatred)),
                new NpcShopChoiceDefinition("Voltar", BackChoiceId),
            };
        }

        public static bool TryParseExpressionChoice(string choiceId, out NpcExpression expression)
        {
            expression = default;
            if (string.IsNullOrEmpty(choiceId) || !choiceId.StartsWith(ExpressionChoicePrefix, System.StringComparison.Ordinal))
            {
                return false;
            }

            var name = choiceId.Substring(ExpressionChoicePrefix.Length);
            return System.Enum.TryParse(name, out expression);
        }
    }
}
