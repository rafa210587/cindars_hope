using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Skills.Runtime.Effects;
using CindarsHope.UI.HUD;
using UnityEngine;
using UnityEngine.UI;

namespace CindarsHope.UI.HUD.Views
{
    [DisallowMultipleComponent]
    public sealed class ActiveSkillSlotsHudView : MonoBehaviour
    {
        // fable_71: wiring opcional do canvas (4 slots = teclas 1-4). Headless-safe: se os arrays
        // não estiverem ligados no Editor, a view só roda o guard (comportamento antigo preservado).
        [Header("fable_71 — slots ativos (4 = teclas 1-4)")]
        [SerializeField] private Button[] _slotButtons;        // clique -> TryUseSlot(i)
        [SerializeField] private Image[] _cooldownFills;       // radial fill (fillAmount 0..1)
        [SerializeField] private GameObject[] _blockedOverlays; // overlay quando inutilizável

        private GameplayHudViewModel _viewModel;

        public void Initialize(GameplayHudViewModel viewModel) => _viewModel = viewModel;

        private void OnEnable()
        {
            GameEventBus.Subscribe<HudVisibilityChangedEvent>(OnVisibilityChanged);
            WireButtons();
        }

        private void OnDisable() => GameEventBus.Unsubscribe<HudVisibilityChangedEvent>(OnVisibilityChanged);

        private void WireButtons()
        {
            if (_slotButtons == null) return;
            for (int i = 0; i < _slotButtons.Length; i++)
            {
                if (_slotButtons[i] == null) continue;
                int slot = i; // captura por valor
                _slotButtons[i].onClick.RemoveAllListeners();
                // Clique e teclas 1-4 convergem no mesmo ponto de uso (sem duplicar lógica).
                _slotButtons[i].onClick.AddListener(() => ActiveSkillExecutionController.Instance?.TryUseSlot(slot));
            }
        }

        private void Update()
        {
            if (_viewModel == null) return;
            Refresh();
        }

        private void Refresh()
        {
            var errors = FinalHudGuardValidator.Validate(_viewModel);
            if (errors.Count > 0)
                Debug.LogWarning($"[ActiveSkillSlotsHudView] Guard violation: {string.Join(", ", errors)}");

            var slots = _viewModel.ActiveSkillSlots;
            if (slots == null) return;
            var controller = ActiveSkillExecutionController.Instance;

            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                int idx = slot.SlotIndex;

                // Cooldown radial fill (remaining/total). Fonte única = controller.
                if (_cooldownFills != null && idx >= 0 && idx < _cooldownFills.Length && _cooldownFills[idx] != null)
                {
                    float total = controller != null ? controller.GetSlotCooldownTotal(idx) : 0f;
                    float remaining = controller != null ? controller.GetSlotCooldownRemaining(idx) : 0f;
                    _cooldownFills[idx].fillAmount = total > 0f ? Mathf.Clamp01(remaining / total) : 0f;
                }

                // Overlay de bloqueio (vazio/cooldown).
                if (_blockedOverlays != null && idx >= 0 && idx < _blockedOverlays.Length && _blockedOverlays[idx] != null)
                    _blockedOverlays[idx].SetActive(!slot.IsUsableInContext);

                // Botão interagível só quando usável.
                if (_slotButtons != null && idx >= 0 && idx < _slotButtons.Length && _slotButtons[idx] != null)
                    _slotButtons[idx].interactable = slot.IsUsableInContext;
            }
        }

        private void OnVisibilityChanged(HudVisibilityChangedEvent evt) => gameObject.SetActive(evt.IsVisible);
    }
}
