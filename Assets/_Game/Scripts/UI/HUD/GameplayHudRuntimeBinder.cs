using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Skills;
using CindarsHope.Skills.Runtime.Effects;
using UnityEngine;

namespace CindarsHope.UI.HUD
{
    [DisallowMultipleComponent]
    public sealed class GameplayHudRuntimeBinder : MonoBehaviour
    {
        private GameplayHudViewModel _viewModel;

        public GameplayHudViewModel ViewModel => _viewModel;

        public void Initialize(GameplayHudViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<HPChangedEvent>(OnHpChanged);
            GameEventBus.Subscribe<StaminaChangedEvent>(OnStaminaChanged);
            GameEventBus.Subscribe<ManaChangedEvent>(OnManaChanged);
            GameEventBus.Subscribe<HungerChangedEvent>(OnHungerChanged);
            GameEventBus.Subscribe<InteractionPromptChangedEvent>(OnInteractionPromptChanged);
            GameEventBus.Subscribe<QuestAcceptedEvent>(OnQuestAccepted);
            GameEventBus.Subscribe<QuestObjectiveProgressedEvent>(OnQuestObjectiveProgressed);
            GameEventBus.Subscribe<QuestReadyToCompleteEvent>(OnQuestReady);
            GameEventBus.Subscribe<QuestCompletedEvent>(OnQuestCompleted);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<HPChangedEvent>(OnHpChanged);
            GameEventBus.Unsubscribe<StaminaChangedEvent>(OnStaminaChanged);
            GameEventBus.Unsubscribe<ManaChangedEvent>(OnManaChanged);
            GameEventBus.Unsubscribe<HungerChangedEvent>(OnHungerChanged);
            GameEventBus.Unsubscribe<InteractionPromptChangedEvent>(OnInteractionPromptChanged);
            GameEventBus.Unsubscribe<QuestAcceptedEvent>(OnQuestAccepted);
            GameEventBus.Unsubscribe<QuestObjectiveProgressedEvent>(OnQuestObjectiveProgressed);
            GameEventBus.Unsubscribe<QuestReadyToCompleteEvent>(OnQuestReady);
            GameEventBus.Unsubscribe<QuestCompletedEvent>(OnQuestCompleted);
        }

        private void Update()
        {
            if (_viewModel == null) return;
            RefreshActiveSkillSlots();
        }

        private void RefreshActiveSkillSlots()
        {
            var skillTreeManager = GameBootstrap.Instance?.SkillTreeManager;
            if (skillTreeManager == null) return;
            var state = skillTreeManager.State;
            // fable_71: cooldown vem do controller (fonte única); a HUD não recalcula.
            var controller = ActiveSkillExecutionController.Instance;
            _viewModel.ActiveSkillSlots.Clear();
            for (int i = 0; i < 4; i++) // slots 0-3 = teclas 1-4
            {
                var skillId = state.GetActiveSlotSkillActionId(i);
                bool equipped = !string.IsNullOrEmpty(skillId);
                float cooldownRemaining = controller != null ? controller.GetSlotCooldownRemaining(i) : 0f;

                var vm = new ActiveSkillSlotViewModel
                {
                    SlotIndex = i,
                    SkillId = skillId ?? string.Empty,
                    IconId = skillId ?? string.Empty
                };
                ActiveSkillSlotProjection.Project(equipped, cooldownRemaining, vm);
                _viewModel.ActiveSkillSlots.Add(vm);
            }
        }

        private void OnHpChanged(HPChangedEvent evt)
        {
            if (_viewModel == null) return;
            _viewModel.Hp = evt.CurrentHP;
            _viewModel.MaxHp = evt.MaxHP;
        }

        private void OnStaminaChanged(StaminaChangedEvent evt)
        {
            if (_viewModel == null) return;
            _viewModel.Stamina = evt.CurrentStamina;
            _viewModel.MaxStamina = evt.MaxStamina;
        }

        private void OnManaChanged(ManaChangedEvent evt)
        {
            if (_viewModel == null) return;
            _viewModel.Mp = evt.CurrentMana;
            _viewModel.MaxMp = evt.MaxMana;
            _viewModel.ShowMp = evt.MaxMana > 0;
        }

        private void OnHungerChanged(HungerChangedEvent evt)
        {
            if (_viewModel == null) return;
            _viewModel.HungerCompact = evt.MaxValue > 0 ? (float)evt.CurrentValue / evt.MaxValue : 1f;
        }

        private void OnInteractionPromptChanged(InteractionPromptChangedEvent evt)
        {
            if (_viewModel == null) return;
            _viewModel.ContextPrompt = new ContextPromptProjection
            {
                IsVisible = evt.HasCandidate,
                DescriptionKey = evt.Prompt ?? string.Empty,
                ActionKey = "E"
            };
        }

        private void OnQuestAccepted(QuestAcceptedEvent evt)
        {
            if (_viewModel == null) return;
            _viewModel.QuestPrompt = $"Quest ativa: {evt.QuestId}";
        }

        private void OnQuestObjectiveProgressed(QuestObjectiveProgressedEvent evt)
        {
            if (_viewModel == null) return;
            _viewModel.QuestPrompt = $"{evt.QuestId}: {evt.CurrentProgress}/{evt.RequiredProgress}";
        }

        private void OnQuestReady(QuestReadyToCompleteEvent evt)
        {
            if (_viewModel == null) return;
            _viewModel.QuestPrompt = $"Entregar: {evt.QuestId}";
        }

        private void OnQuestCompleted(QuestCompletedEvent evt)
        {
            if (_viewModel == null) return;
            _viewModel.QuestPrompt = string.Empty;
        }
    }
}
