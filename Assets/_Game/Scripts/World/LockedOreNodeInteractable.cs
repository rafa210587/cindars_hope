using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.World
{
    /// <summary>
    /// Veia de minerio bloqueada na base da montanha. Ao interagir antes do gate de progressao,
    /// publica PlayerActionFeedbackEvent com mensagem de recusa. Uma vez desbloqueada (_unlocked
    /// == true, hoje setavel so via editor/inspector — gate real de progressao e spec futura),
    /// entrega item_material_copper_ore real via InventoryManager.AddItem e depleta em sucesso
    /// (single-shot: uma interacao = uma entrega). Se o inventario estiver cheio, nao depleta.
    /// Gate de desbloqueio (_unlocked): integrar via spec de progressao/evolucao futura.
    /// Criado em 2026-06-26 (spec_farm_scene_relayout_v4). Entrega real: spec_codex_04.
    /// </summary>
    public class LockedOreNodeInteractable : MonoBehaviour, IInteractable
    {
        private const string RecusaBloqueadoMessage = "Minerio bloqueado — requer progressao";
        private const string RecusaInventarioCheioMessage = "Inventario cheio";
        private const string SucessoMessage = "Minerio coletado!";
        private const string OreItemId = "item_material_copper_ore";
        private const int OreYieldAmount = 1;

        [SerializeField] private string _nodeId = "ore_node_locked";

        // Gate de progressao — false = bloqueado (padrao). Integrar via spec futura.
        [SerializeField] private bool _unlocked = false;

        [SerializeField] private InventoryManager _inventoryManager;

        // Depletion local simples (single-shot). Nao reusa FarmResourceInteractable/refresh runtime:
        // aquele padrao e para nos de refresh ciclico (farm_node_tree/farm_node_rock) com re-spawn por dia;
        // este no e um desbloqueio unico sem ciclo de refresh, entao um campo local e o minimo necessario.
        private bool _depleted;

        public string InteractionPrompt
        {
            get
            {
                if (_depleted)
                {
                    return "Veia esgotada";
                }

                return _unlocked ? "Minerar minerio" : "Minerio bloqueado";
            }
        }

        public bool CanInteract(GameObject interactor)
        {
            // Sempre pode interagir (para mostrar o feedback de recusa mesmo bloqueado/esgotado).
            return true;
        }

        public void Interact(GameObject interactor)
        {
            if (!_unlocked)
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent(RecusaBloqueadoMessage));
                return;
            }

            if (_depleted)
            {
                return;
            }

            if (_inventoryManager == null)
            {
                Debug.LogWarning($"[LockedOreNode] {_nodeId} sem InventoryManager wired — entrega abortada.", this);
                return;
            }

            var added = _inventoryManager.AddItem(OreItemId, OreYieldAmount);
            if (!added)
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent(RecusaInventarioCheioMessage));
                Debug.LogWarning($"[LockedOreNode] {_nodeId}: AddItem retornou false para {OreYieldAmount}x {OreItemId}. No permanece disponivel.", this);
                return;
            }

            _depleted = true;
            GameEventBus.Publish(new PlayerActionFeedbackEvent(SucessoMessage));
            Debug.Log($"[LockedOreNode] {_nodeId}: adicionado {OreYieldAmount}x {OreItemId} ao inventario. No depletado.", this);
        }

        /// <summary>
        /// API de editor/gerador para definir o ID do no.
        /// </summary>
        public void EditorSetNodeId(string nodeId)
        {
            _nodeId = nodeId;
        }

        /// <summary>
        /// API de wiring para o gerador de cena ligar o InventoryManager sem FindObjectOfType,
        /// analoga a TreeNode.RebindInventoryManager.
        /// </summary>
        public void RebindInventoryManager(InventoryManager inventoryManager)
        {
            if (inventoryManager == null)
            {
                Debug.LogWarning($"[LockedOreNode] {_nodeId} recebeu InventoryManager nulo para rebind.", this);
                return;
            }

            _inventoryManager = inventoryManager;
        }

        /// <summary>
        /// Decisao pura de entrega, extraida para ser testavel em EditMode sem MonoBehaviour/GameEventBus.
        /// Dado o estado do no e o resultado (simulado) de AddItem, retorna o proximo estado.
        /// </summary>
        public static LockedOreDeliveryResult ResolveDelivery(bool unlocked, bool depleted, bool addItemSucceeded)
        {
            if (!unlocked)
            {
                return LockedOreDeliveryResult.Blocked;
            }

            if (depleted)
            {
                return LockedOreDeliveryResult.AlreadyDepleted;
            }

            return addItemSucceeded ? LockedOreDeliveryResult.Delivered : LockedOreDeliveryResult.InventoryFull;
        }
    }

    /// <summary>
    /// Resultado possivel da decisao de entrega de LockedOreNodeInteractable, para testes EditMode
    /// e clareza de leitura no chamador.
    /// </summary>
    public enum LockedOreDeliveryResult
    {
        Blocked,
        AlreadyDepleted,
        Delivered,
        InventoryFull
    }
}
