using CindarsHope.Interaction;
using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.Farm.Integration
{
    // TODO_INTEGRATION_NOT_FINAL (RE-REGISTRADO fable_66, owner: kit inicial canônico + tool/tier flow):
    // adapter simplificado de slice gerado por CreateMvpFarmScene para validação jogável. Substituir
    // pelo fluxo final (TreeNode/FarmResourceNodeService com ferramenta/tier) SÓ depois que o kit inicial
    // existir E a FarmScene for regenerada no Unity Editor sem este adapter. Mantido honestamente como
    // débito ABERTO — ver docs/validation/fable_66_spec_code_debt_cleanup_slice_mode_execution_report.md.
    [DisallowMultipleComponent]
    public sealed class FarmResourceInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private FarmResourceInteractableType _resourceType = FarmResourceInteractableType.Unknown;
        [SerializeField] private FarmResourceReward _reward = new FarmResourceReward();
        [SerializeField] private FarmResourceVisualController _visualController;
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private string _interactionPrompt = "Interagir";

        private FarmResourceVisualState _state = FarmResourceVisualState.Available;

        public string InteractionPrompt => _interactionPrompt;

        public bool CanInteract(GameObject interactor)
        {
            return _state == FarmResourceVisualState.Available;
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
                return;

            if (_inventoryManager == null || string.IsNullOrEmpty(_reward.ItemId))
            {
                // TODO_INTEGRATION_NOT_FINAL — feedback-only fallback: inventory not wired or item id empty
                Debug.LogWarning($"[FarmResource] {_resourceType}: cannot collect — inventory not wired or reward item id empty. Resource stays Available.", this);
                return;
            }

            var amount = _reward.ClampedAmount;
            var added = _inventoryManager.AddItem(_reward.ItemId, amount);
            if (added)
            {
                _state = FarmResourceVisualState.Depleted;
                _visualController?.Apply(_state);
                RegisterForRefresh();
                Debug.Log($"[FarmResource] {_resourceType}: added {amount}x {_reward.ItemId} to inventory.", this);
            }
            else
            {
                // TODO_INTEGRATION_NOT_FINAL — AddItem returned false (inventory full or item not found); resource stays Available
                Debug.LogWarning($"[FarmResource] {_resourceType}: AddItem returned false for {amount}x {_reward.ItemId}. Resource stays Available.", this);
            }
        }

        public void ResetResource()
        {
            _state = FarmResourceVisualState.Available;
            _visualController?.Apply(_state);
        }

        // F15: registra o nó depletado no host de refresh (FarmResourceRefreshProcessor WAVE 05).
        // Seed determinístico pela posição (estável por cena; sem GUID/timestamp).
        private void RegisterForRefresh()
        {
            var runtime = Farm.Resources.FarmResourceRefreshRuntime.Instance;
            if (runtime == null)
            {
                return;
            }

            var nodeId = Farm.Resources.FarmResourceRefreshRuntime.ResolveNodeId(_resourceType);
            if (string.IsNullOrEmpty(nodeId))
            {
                return;
            }

            var tileX = Mathf.RoundToInt(transform.position.x);
            var tileY = Mathf.RoundToInt(transform.position.y);
            var weatherService = World.Weather.WorldWeatherService.Instance;
            var currentDay = weatherService != null ? weatherService.CurrentDay : 1;

            var state = new Farm.Resources.ResourceNodeInstanceState
            {
                NodeInstanceId = $"{nodeId}_{tileX}_{tileY}",
                NodeId = nodeId,
                TileX = tileX,
                TileY = tileY,
                CurrentState = Farm.Resources.ResourceNodeCurrentState.Harvested,
                LastHarvestedDay = currentDay,
                NextEligibleRefreshDay = -1,
                RandomSeed = tileX * 73856093 ^ tileY * 19349663
            };

            // FixedDays: o processor exige NextEligibleRefreshDay definido pelo chamador.
            if (nodeId == "farm_node_tree" || nodeId == "farm_node_rock")
            {
                state.NextEligibleRefreshDay = currentDay + 3;
            }

            runtime.RegisterDepletedNode(state, ResetResource);
        }

        private void Start()
        {
            _visualController?.Apply(_state);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_reward != null && _reward.Amount < 1)
                Debug.LogWarning($"[FarmResource] {gameObject.name}: reward amount is {_reward.Amount} — must be >= 1. ClampedAmount will be used at runtime.", this);
        }
#endif
    }
}
