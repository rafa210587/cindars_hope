using CindarsHope.Craft.Data;
using CindarsHope.Interaction;
using CindarsHope.UI.Crafting;
using UnityEngine;

namespace CindarsHope.Craft
{
    [DisallowMultipleComponent]
    public sealed class CraftingPoint : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _stationInstanceId;
        [SerializeField] private WorkshopType _stationType;
        [SerializeField] private CraftingRuntime _craftingRuntime;
        [SerializeField] private CraftingModal _craftingModal;

        public string StationInstanceId => _stationInstanceId;
        public WorkshopType StationType => _stationType;
        public string InteractionPrompt => $"Craftar - {_stationType}";

        public bool CanInteract(GameObject interactor)
        {
            return _craftingRuntime != null && _craftingModal != null && !string.IsNullOrWhiteSpace(_stationInstanceId);
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
            {
                Debug.LogWarning($"Crafting station '{name}' is not configured.", this);
                return;
            }

            var station = _craftingRuntime.GetOrCreateStation(_stationInstanceId, _stationType);
            _craftingModal.Open(station);
        }

        public void Configure(string stationInstanceId, WorkshopType stationType, CraftingRuntime craftingRuntime, CraftingModal craftingModal)
        {
            _stationInstanceId = stationInstanceId;
            _stationType = stationType;
            _craftingRuntime = craftingRuntime;
            _craftingModal = craftingModal;
        }

        public void RebindCraftingManager(CraftingManager craftingManager)
        {
            // Compatibility hook for the existing scene installer; workstation flow uses CraftingRuntime.
        }
    }
}
