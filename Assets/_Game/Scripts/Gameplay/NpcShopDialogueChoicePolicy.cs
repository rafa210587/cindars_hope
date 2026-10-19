using System.Collections.Generic;

namespace CindarsHope.NPC
{
    public readonly struct NpcShopChoiceDefinition
    {
        public readonly string Label;
        public readonly string ChoiceId;

        public NpcShopChoiceDefinition(string label, string choiceId)
        {
            Label = label;
            ChoiceId = choiceId;
        }
    }

    public readonly struct NpcShopServiceChoiceDefinition
    {
        public readonly string Label;
        public readonly string ServiceId;

        public NpcShopServiceChoiceDefinition(string label, string serviceId)
        {
            Label = label;
            ServiceId = serviceId;
        }
    }

    /// <summary>Owns stable shop-choice ordering and identifiers without depending on Unity UI.</summary>
    public static class NpcShopDialogueChoicePolicy
    {
        public const string DebugExpressionChoiceId = "dbg_open";
        public const string NpcServiceChoicePrefix = "svc:";

        public static IReadOnlyList<NpcShopChoiceDefinition> BuildRootChoices(
            bool showTempering,
            string cityServiceLabel,
            IReadOnlyList<NpcShopServiceChoiceDefinition> npcServices,
            bool showDebug)
        {
            var choices = new List<NpcShopChoiceDefinition>
            {
                new NpcShopChoiceDefinition("Conversar", "talk"),
                new NpcShopChoiceDefinition("Comprar", "buy"),
                new NpcShopChoiceDefinition("Vender", "sell"),
                new NpcShopChoiceDefinition("Dar presente", "gift")
            };

            if (showTempering)
                choices.Add(new NpcShopChoiceDefinition("Temperar", "temper"));
            if (!string.IsNullOrEmpty(cityServiceLabel))
                choices.Add(new NpcShopChoiceDefinition(cityServiceLabel, "service"));
            AddNpcServices(choices, npcServices);
            if (showDebug)
                choices.Add(new NpcShopChoiceDefinition("[Debug] expressao", DebugExpressionChoiceId));
            choices.Add(new NpcShopChoiceDefinition("Adeus", "exit"));
            return choices;
        }

        public static IReadOnlyList<NpcShopChoiceDefinition> BuildThalindraChoices(
            ThalindraQuestDialogueDecision questDecision,
            IReadOnlyList<NpcShopServiceChoiceDefinition> npcServices,
            bool showDebug)
        {
            var choices = new List<NpcShopChoiceDefinition>();
            if (questDecision.ShowChoice)
                choices.Add(new NpcShopChoiceDefinition(questDecision.ChoiceLabel, "quest"));
            choices.Add(new NpcShopChoiceDefinition("Comprar", "buy"));
            choices.Add(new NpcShopChoiceDefinition("Vender", "sell"));
            AddNpcServices(choices, npcServices);
            if (showDebug)
                choices.Add(new NpcShopChoiceDefinition("[Debug] expressao", DebugExpressionChoiceId));
            choices.Add(new NpcShopChoiceDefinition("Adeus", "exit"));
            return choices;
        }

        private static void AddNpcServices(
            List<NpcShopChoiceDefinition> choices,
            IReadOnlyList<NpcShopServiceChoiceDefinition> npcServices)
        {
            if (npcServices == null) return;
            for (var i = 0; i < npcServices.Count; i++)
            {
                var service = npcServices[i];
                if (string.IsNullOrEmpty(service.ServiceId)) continue;
                choices.Add(new NpcShopChoiceDefinition(
                    service.Label,
                    NpcServiceChoicePrefix + service.ServiceId));
            }
        }
    }
}
