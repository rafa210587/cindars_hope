using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using UnityEngine;

namespace CindarsHope.World
{
    /// <summary>
    /// Veia de minerio bloqueada na base da montanha. Ao interagir antes do gate de progressao,
    /// publica PlayerActionFeedbackEvent com mensagem de recusa. Sem entrega de item.
    /// Gate futuro: integrar via spec de progressao/evolucao (aqui apenas stub de flag).
    /// Criado em 2026-06-26 (spec_farm_scene_relayout_v4).
    /// </summary>
    public class LockedOreNodeInteractable : MonoBehaviour, IInteractable
    {
        private const string RecusaMessage = "Minerio bloqueado — requer progressao";

        [SerializeField] private string _nodeId = "ore_node_locked";

        // Gate de progressao — false = bloqueado (padrao). Integrar via spec futura.
        [SerializeField] private bool _unlocked = false;

        public string InteractionPrompt => _unlocked ? "Minerar minerio" : "Minerio bloqueado";

        public bool CanInteract(GameObject interactor)
        {
            // Sempre pode interagir (para mostrar o feedback de recusa).
            return true;
        }

        public void Interact(GameObject interactor)
        {
            if (_unlocked)
            {
                // TODO (spec futura): entregar item de minerio ao jogador.
                Debug.Log($"[LockedOreNode] {_nodeId} desbloqueado — coleta nao implementada ainda.");
                return;
            }

            // Recusa com feedback ao jogador.
            GameEventBus.Publish(new PlayerActionFeedbackEvent(RecusaMessage));
        }

        /// <summary>
        /// API de editor/gerador para definir o ID do no.
        /// </summary>
        public void EditorSetNodeId(string nodeId)
        {
            _nodeId = nodeId;
        }
    }
}
