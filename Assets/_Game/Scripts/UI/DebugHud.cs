using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Inventory;
using CindarsHope.Interaction;
using CindarsHope.Player;
using CindarsHope.Core.Time;
using CindarsHope.Save;
using UnityEngine;

namespace CindarsHope.UI
{
    [DisallowMultipleComponent]
    public class DebugHud : MonoBehaviour
    {
        private static DebugHud _instance;

        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private HungerManager _hungerManager;
        [SerializeField] private InteractionSystem _interactionSystem;
        [SerializeField] private TimeManager _timeManager;
        [SerializeField] private SaveManager _saveManager;

        private bool _hasInteractionCandidate;
        private string _currentInteractionPrompt = string.Empty;
        private string _lastEconomyTransaction = "nenhuma transacao";
        private bool _isPrimaryInstance;

        public static DebugHud Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                _isPrimaryInstance = false;
                enabled = false;
                gameObject.SetActive(false);
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _isPrimaryInstance = true;
            DontDestroyOnLoad(gameObject);
            Debug.Log("DebugHud: initialized as primary instance.");
        }

        private void OnEnable()
        {
            if (!_isPrimaryInstance || _instance != this)
            {
                return;
            }

            GameEventBus.Subscribe<InteractionPromptChangedEvent>(OnInteractionPromptChanged);
            GameEventBus.Subscribe<EconomyTransactionCompletedEvent>(OnEconomyTransactionCompleted);
        }

        private void OnDisable()
        {
            if (!_isPrimaryInstance || _instance != this)
            {
                return;
            }

            GameEventBus.Unsubscribe<InteractionPromptChangedEvent>(OnInteractionPromptChanged);
            GameEventBus.Unsubscribe<EconomyTransactionCompletedEvent>(OnEconomyTransactionCompleted);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                _isPrimaryInstance = false;
            }
        }

        private void OnGUI()
        {
            if (!_isPrimaryInstance || _instance != this)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(12f, 12f, 360f, Screen.height - 24f), GUI.skin.box);
            GUILayout.Label("Cindar's Hope - Debug HUD");
            DrawPlayerState();
            DrawHungerState();
            DrawWorldState();
            DrawInteractionState();
            DrawEconomyState();
            DrawInventory();
            DrawCommands();
            GUILayout.EndArea();
        }

        private void DrawWorldState()
        {
            if (_timeManager == null)
            {
                GUILayout.Label("Day: not assigned");
                return;
            }

            GUILayout.Label($"Day: {_timeManager.CurrentDay}");
        }

        private void DrawInteractionState()
        {
            if (_hasInteractionCandidate)
            {
                GUILayout.Label($"Interacao: {_currentInteractionPrompt}");
                return;
            }

            if (_interactionSystem != null && _interactionSystem.HasCandidate)
            {
                GUILayout.Label($"Interacao: {_interactionSystem.CurrentPrompt}");
                return;
            }

            GUILayout.Label("Interacao: nenhum alvo");
        }

        private void DrawEconomyState()
        {
            GUILayout.Label($"Economia: {_lastEconomyTransaction}");
        }

        private void DrawPlayerState()
        {
            if (_playerManager == null)
            {
                GUILayout.Label("PlayerManager: not assigned");
                return;
            }

            GUILayout.Label($"Gold: {_playerManager.CurrentGold}");
            GUILayout.Label($"HP: {_playerManager.CurrentHP}/{_playerManager.MaxHP}");
        }

        private void DrawHungerState()
        {
            if (_hungerManager == null)
            {
                GUILayout.Label("HungerManager: not assigned");
                return;
            }

            GUILayout.Label($"Hunger: {_hungerManager.CurrentHunger}/{_hungerManager.MaxHunger}");
        }

        private void DrawInventory()
        {
            GUILayout.Space(8f);
            GUILayout.Label("Inventory:");

            if (_inventoryManager == null)
            {
                GUILayout.Label("- not assigned");
                return;
            }

            if (_inventoryManager.Items.Count == 0)
            {
                GUILayout.Label("- empty");
                return;
            }

            foreach (var item in _inventoryManager.Items)
            {
                GUILayout.Label($"- {item.Key}: {item.Value}");
            }
        }

        private void DrawCommands()
        {
            GUILayout.Space(8f);
            GUILayout.Label("Commands:");
            GUILayout.Label("WASD/arrows: mover");
            GUILayout.Label("E: interagir");
            GUILayout.Label("Tab: avancar dia");
            GUILayout.Label("H: consumir comida");
            GUILayout.Label("Loja: E compra trigo x3 por 5g");
            GUILayout.Label("Pesca: E no lago com cana");
            GUILayout.Label("Arvore: E para cortar");
            GUILayout.Label("F5: salvar");
            GUILayout.Label("F9: carregar");

            if (_saveManager != null)
            {
                GUILayout.Label($"Save: {_saveManager.SaveFilePath}");
            }
        }

        private void OnInteractionPromptChanged(InteractionPromptChangedEvent evt)
        {
            _hasInteractionCandidate = evt.HasCandidate;
            _currentInteractionPrompt = evt.Prompt ?? string.Empty;
        }

        private void OnEconomyTransactionCompleted(EconomyTransactionCompletedEvent evt)
        {
            _lastEconomyTransaction = evt.Message;
        }

        public void RebindRuntimeReferences(
            PlayerManager playerManager,
            InventoryManager inventoryManager,
            HungerManager hungerManager,
            InteractionSystem interactionSystem,
            TimeManager timeManager,
            SaveManager saveManager)
        {
            if (playerManager != null)
            {
                _playerManager = playerManager;
            }

            if (inventoryManager != null)
            {
                _inventoryManager = inventoryManager;
            }

            if (hungerManager != null)
            {
                _hungerManager = hungerManager;
            }

            if (interactionSystem != null)
            {
                _interactionSystem = interactionSystem;
            }

            if (timeManager != null)
            {
                _timeManager = timeManager;
            }

            if (saveManager != null)
            {
                _saveManager = saveManager;
            }

            Debug.Log("DebugHud: runtime references rebound.");
        }

        public static void RebindExisting(
            PlayerManager playerManager,
            InventoryManager inventoryManager,
            HungerManager hungerManager,
            InteractionSystem interactionSystem,
            TimeManager timeManager,
            SaveManager saveManager)
        {
            if (_instance != null)
            {
                _instance.RebindRuntimeReferences(playerManager, inventoryManager, hungerManager, interactionSystem, timeManager, saveManager);
            }
        }
    }
}
