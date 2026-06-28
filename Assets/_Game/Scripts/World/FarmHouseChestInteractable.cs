using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using UnityEngine;

namespace CindarsHope.World
{
    /// <summary>
    /// Bau de armazenamento da casa da fazenda. Placeholder stub: ao interagir exibe mensagem
    /// de que o sistema de armazenamento sera implementado na spec de inventario da fazenda.
    /// Criado em 2026-06-26 (spec_farm_scene_relayout_v4).
    /// </summary>
    public class FarmHouseChestInteractable : MonoBehaviour, IInteractable
    {
        public string InteractionPrompt => "Abrir bau";

        public bool CanInteract(GameObject interactor)
        {
            return true;
        }

        public void Interact(GameObject interactor)
        {
            // Stub: feedback ao jogador enquanto o sistema de storage nao esta implementado.
            GameEventBus.Publish(new PlayerActionFeedbackEvent("Bau de armazenamento — em desenvolvimento"));
            Debug.Log("[FarmHouseChest] Interacao com o bau da casa. Sistema de armazenamento a ser implementado em spec futura.");
        }
    }
}
