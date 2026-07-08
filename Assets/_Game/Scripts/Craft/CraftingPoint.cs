using CindarsHope.Core;
using CindarsHope.Craft.Data;
using CindarsHope.Craft.Events;
using CindarsHope.Interaction;
using CindarsHope.UI.Crafting;
using UnityEngine;

namespace CindarsHope.Craft
{
    /// <summary>
    /// Estação de craft física e interagível (forja, alambique, tear, fogão, bancada). Apertar E abre o
    /// craft filtrado pelo WorkshopType. Funciona de DOIS modos: se os refs de runtime+modal forem plugados
    /// no momento da geração (cenas que os criam localmente, ex.: fazenda), abre direto; senão (ex.: cidade,
    /// onde o modal é resolvido em runtime) publica <see cref="OpenCraftingStationRequestedEvent"/> e o
    /// CraftingModal assina. Componente ÚNICO de estação — sem paralelo.
    /// </summary>
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
            return interactor != null && !string.IsNullOrWhiteSpace(_stationInstanceId);
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
            {
                Debug.LogWarning($"Crafting station '{name}' is not configured.", this);
                return;
            }

            // Modo direto (refs plugados no gerador) ou modo desacoplado (evento; o modal da cena assina).
            if (_craftingRuntime != null && _craftingModal != null)
            {
                _craftingModal.Open(_craftingRuntime.GetOrCreateStation(_stationInstanceId, _stationType));
            }
            else
            {
                GameEventBus.Publish(new OpenCraftingStationRequestedEvent(_stationInstanceId, _stationType));
            }
        }

        public void Configure(string stationInstanceId, WorkshopType stationType, CraftingRuntime craftingRuntime, CraftingModal craftingModal)
        {
            _stationInstanceId = stationInstanceId;
            _stationType = stationType;
            _craftingRuntime = craftingRuntime;
            _craftingModal = craftingModal;
        }

        /// <summary>Configuração desacoplada (sem refs): a estação publica o evento e o CraftingModal da cena abre.</summary>
        public void Configure(string stationInstanceId, WorkshopType stationType)
        {
            _stationInstanceId = stationInstanceId;
            _stationType = stationType;
            _craftingRuntime = null;
            _craftingModal = null;
        }

        public void RebindCraftingManager(CraftingManager craftingManager)
        {
            // Compatibility hook for the existing scene installer; workstation flow uses CraftingRuntime.
        }
    }
}
