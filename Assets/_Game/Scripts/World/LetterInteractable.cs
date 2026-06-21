using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using CindarsHope.Narrative;
using UnityEngine;

namespace CindarsHope.World
{
    /// <summary>
    /// fable_63 — carta na cama da fazenda (criada via CreateMvpFarmScene, nunca YAML manual).
    ///
    /// Interagir mostra o texto da carta (PLACEHOLDER_LORE, "Procure Corvus na cidade") via o
    /// HUD feedback do GameEventBus (mesmo canal do BedInteractable). Estado lido persistido como
    /// flag simples (letter_read) na familia de flags do QuestStateSection — sem secao nova. Apos
    /// a 1a leitura a carta fica marcada como lida (prompt muda; nao some, para nao depender de
    /// edicao de cena). IInteractable padrao; sem GameObject.Find.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LetterInteractable : MonoBehaviour, IInteractable
    {
        // PLACEHOLDER_LORE — texto provisorio (refinamento Nymirianos/Cindar pendente).
        private const string LetterText =
            "PLACEHOLDER_LORE: \"Bem-vindo a Cindar's Hope. Se le isto, a terra agora e sua. " +
            "Procure Corvus, no templo da cidade — ele guarda o que eu nao pude contar.\"";

        public string InteractionPrompt =>
            (NarrativeRuntimeBootstrap.FlagStore?.IsSet(NarrativeIds.FlagLetterRead) ?? false)
                ? "Reler a carta"
                : "Ler a carta";

        public bool CanInteract(GameObject interactor) => isActiveAndEnabled;

        public void Interact(GameObject interactor)
        {
            GameEventBus.Publish(new PlayerActionFeedbackEvent(LetterText));

            // Marca a carta como lida (idempotente) na familia de flags persistida.
            NarrativeRuntimeBootstrap.FlagStore?.Set(NarrativeIds.FlagLetterRead);
        }
    }
}
