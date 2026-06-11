using CindarsHope.UI.HUD.Views;
using UnityEngine;

namespace CindarsHope.UI.HUD
{
    [DisallowMultipleComponent]
    public sealed class GameplayHudCanvasController : MonoBehaviour
    {
        public static GameplayHudCanvasController Instance { get; private set; }

        private GameplayHudViewModel _viewModel;
        private GameplayHudRuntimeBinder _binder;
        private GameplayFeedbackService _feedbackService;
        private HudVisibilityController _visibilityController;

        private StatusBarsHudView _statusBars;
        private QuestTrackerHudView _questTracker;
        private ActiveSkillSlotsHudView _skillSlots;
        private InteractionPromptHudView _interactionPrompt;
        private FeedbackToastHudView _feedbackToast;
        private ModalBlockerHudView _modalBlocker;

        public GameplayHudViewModel ViewModel => _viewModel;
        public bool IsInitialized { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Initialize(
            GameplayHudViewModel viewModel,
            GameplayHudRuntimeBinder binder,
            GameplayFeedbackService feedbackService,
            HudVisibilityController visibilityController)
        {
            _viewModel = viewModel;
            _binder = binder;
            _feedbackService = feedbackService;
            _visibilityController = visibilityController;

            binder.Initialize(viewModel);
            AttachViews();

            IsInitialized = true;
            Debug.Log("[GameplayHudCanvasController] Initialized. Canvas HUD ready (headless — Unity Editor wiring pending).");
        }

        private void AttachViews()
        {
            _statusBars = gameObject.AddComponent<StatusBarsHudView>();
            _statusBars.Initialize(_viewModel);

            _questTracker = gameObject.AddComponent<QuestTrackerHudView>();
            _questTracker.Initialize(_viewModel);

            _skillSlots = gameObject.AddComponent<ActiveSkillSlotsHudView>();
            _skillSlots.Initialize(_viewModel);

            _interactionPrompt = gameObject.AddComponent<InteractionPromptHudView>();
            _interactionPrompt.Initialize(_viewModel);

            _feedbackToast = gameObject.AddComponent<FeedbackToastHudView>();
            _feedbackToast.Initialize(_feedbackService);

            _modalBlocker = gameObject.AddComponent<ModalBlockerHudView>();
        }
    }
}
