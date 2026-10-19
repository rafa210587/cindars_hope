using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using UnityEngine;

namespace CindarsHope.Quests.Runtime
{
    /// <summary>
    /// fable_34 — town hall mural: READ-ONLY announcements (QuestSource.Mural).
    ///
    /// The mural is NOT the notice board — it has no accept/turn-in. It only surfaces
    /// announcements (upcoming festival via F37, main-quest milestones). This spec wires the
    /// channel; F37 populates the festival announcements.
    ///
    /// Interacting publishes a player feedback message; it never publishes
    /// QuestGiverInteractedEvent (no quest accept path). No GameObject.Find/FindObjectOfType.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MuralInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _interactionPrompt = "Ler o mural da prefeitura";
        [SerializeField] private string _defaultAnnouncement = "Mural da prefeitura: nenhum anuncio novo.";

        public string InteractionPrompt => _interactionPrompt;

        public bool CanInteract(GameObject interactor) => true;

        public void Interact(GameObject interactor)
        {
            // Read-only: surface the current announcement only. No quest is accepted here.
            GameEventBus.Publish(new PlayerActionFeedbackEvent(_defaultAnnouncement));
        }
    }
}
