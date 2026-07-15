using System;
using System.Collections.Generic;
using CindarsHope.Foundation;
// arch: quebra do par mutuo NPC|UI (2026-07-15) — alias explicito: este arquivo vive em
// CindarsHope.NPC, que já declara o tipo CindarsHope.NPC.DialogueChoice (nó de diálogo de árvore).
// Sem o alias, o nome simples "DialogueChoice" resolveria para o tipo do mesmo namespace (regra do
// C#: tipo do namespace corrente tem precedência sobre "using"), não para o POCO de UI abaixo.
using UiDialogueChoice = CindarsHope.Dialogue.DialogueChoice;

namespace CindarsHope.NPC
{
    /// <summary>
    /// Porta local do módulo NPC (arch: corte do par mútuo NPC|UI, 2026-07-15 — precedente
    /// Craft/ICraftingStationModal, World/ICorpseRecoveryPresenter) para o modal de diálogo
    /// consumido por NpcController/NpcManager/NpcShopController, sem depender do tipo concreto
    /// CindarsHope.UI.Dialogue.DialogueModal. Cobre só os membros realmente chamados por esses
    /// consumidores.
    /// </summary>
    public interface INpcDialoguePresenter
    {
        event Action OnClose;
        event Action<UiDialogueChoice> OnChoiceSelected;

        void Initialize(IModalRuntime modalManager);
        void Show(string dialogueText);
        void ShowWithChoices(string dialogueText, List<UiDialogueChoice> choices);
        void Hide();
    }
}
