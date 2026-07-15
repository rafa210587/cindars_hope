using System.Collections.Generic;
// arch: quebra do par mutuo NPC|UI (2026-07-15) — movido de CindarsHope.UI.Dialogue para
// CindarsHope.NPC (não CindarsHope.Dialogue: colocá-lo em Dialogue criaria um novo par mútuo
// Dialogue|NPC, já que ele depende de NpcShopChoiceDefinition). É um conversor puro
// (NpcShopChoiceDefinition -> DialogueChoice), sem dependência de engine/UI; único consumidor é NPC
// (NpcController/NpcShopController); nenhuma mudança de comportamento. Alias necessário: este
// namespace já declara CindarsHope.NPC.DialogueChoice (nó de árvore de diálogo) — sem o alias, o
// nome simples "DialogueChoice" resolveria para esse tipo, não para o POCO de UI.
using UiDialogueChoice = CindarsHope.Dialogue.DialogueChoice;

namespace CindarsHope.NPC
{
    public static class NpcShopChoiceUiAdapter
    {
        public static List<UiDialogueChoice> ToUiChoices(IReadOnlyList<NpcShopChoiceDefinition> definitions)
        {
            var choices = new List<UiDialogueChoice>(definitions.Count);
            for (var i = 0; i < definitions.Count; i++)
            {
                choices.Add(new UiDialogueChoice(definitions[i].Label, definitions[i].ChoiceId));
            }

            return choices;
        }
    }
}
