using System.Collections.Generic;
using CindarsHope.NPC;

namespace CindarsHope.UI.Dialogue
{
    public static class NpcShopChoiceUiAdapter
    {
        public static List<DialogueChoice> ToUiChoices(IReadOnlyList<NpcShopChoiceDefinition> definitions)
        {
            var choices = new List<DialogueChoice>(definitions.Count);
            for (var i = 0; i < definitions.Count; i++)
            {
                choices.Add(new DialogueChoice(definitions[i].Label, definitions[i].ChoiceId));
            }

            return choices;
        }
    }
}
