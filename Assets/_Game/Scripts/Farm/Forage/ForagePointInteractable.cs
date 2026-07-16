using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.Farm.Forage
{
    /// <summary>
    /// fable_54 — ponto de forrageio FISICO da cena (substitui o smoke ForageResource_01 de reward
    /// fixo; os dois NAO coexistem). Registra-se em FarmForageRuntimeService (refs serializadas pelo
    /// gerador; SEM GameObject.Find). Em Interact: delega a coleta ao servico (idempotencia/estacao/
    /// zona proibida sao do servico orfao) e, em sucesso, adiciona o item resolvido ao inventario.
    ///
    /// Comunicacao de gameplay via GameEventBus (feedback). InventoryManager e ref de wiring do
    /// gerador (excecao de bootstrap permitida), nunca FindObjectOfType de gameplay.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ForagePointInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _spawnId = "farm_forage_01";
        [SerializeField] private string _zoneId = "farm_zone_forage";
        [SerializeField] private string _interactionPrompt = "Coletar forrageio";
        [SerializeField] private Farm.Integration.FarmResourceVisualController _visualController;

        private bool _registered;

        public string InteractionPrompt => _interactionPrompt;

        public bool CanInteract(GameObject interactor)
        {
            var service = FarmForageRuntimeService.Instance;
            if (service == null) return false;
            return service.TryGetSpawnItem(_spawnId, out _, out _);
        }

        public void Interact(GameObject interactor)
        {
            var service = FarmForageRuntimeService.Instance;
            if (service == null)
            {
                Debug.LogWarning($"[ForagePoint] {_spawnId}: serviço de forrageio ausente.", this);
                return;
            }

            var result = service.Collect(_spawnId);
            if (!result.Success)
            {
                // Feedback discreto (forrageio indisponivel/estacao/zona). Sem remover nada.
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Nada para coletar aqui.", 1.5f));
                return;
            }

            var added = false;
            var inventory = ResolveInventory();
            if (inventory != null && !string.IsNullOrEmpty(result.ItemId))
            {
                added = inventory.AddItem(result.ItemId, Mathf.Max(1, result.Quantity));
            }

            if (added)
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent($"Coletado: {result.ItemId}", 2f));
            }
            else
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Inventário cheio.", 2f));
            }
        }

        private InventoryManager ResolveInventory()
        {
            // Ref de bootstrap (wiring), nao busca de gameplay.
            var bootstrap = Core.Bootstrap.GameBootstrap.Instance;
            // arch: quebra do par mutuo Core|Inventory (2026-07-15) — cast local para o tipo concreto
            // (GameBootstrap.InventoryManager agora retorna a porta IInventoryRuntime).
            return bootstrap != null ? bootstrap.InventoryManager as InventoryManager : null;
        }

        private void Start()
        {
            Register();
        }

        private void Register()
        {
            if (_registered) return;
            var service = FarmForageRuntimeService.Instance;
            if (service == null) return;

            var tileX = Mathf.RoundToInt(transform.position.x);
            var tileY = Mathf.RoundToInt(transform.position.y);
            service.RegisterScenePoint(_spawnId, _zoneId, tileX, tileY, ShowAvailable, ShowDepleted);
            _registered = true;
        }

        private void ShowAvailable()
        {
            _visualController?.Apply(Farm.Integration.FarmResourceVisualState.Available);
        }

        private void ShowDepleted()
        {
            _visualController?.Apply(Farm.Integration.FarmResourceVisualState.Depleted);
        }
    }
}
