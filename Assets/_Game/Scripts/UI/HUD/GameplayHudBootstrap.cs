using UnityEngine;

namespace CindarsHope.UI.HUD
{
    public static class GameplayHudBootstrap
    {
        private const string HudGameObjectName = "GameplayHudCanvas";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (GameplayHudCanvasController.Instance != null) return;

            var go = new GameObject(HudGameObjectName);
            Object.DontDestroyOnLoad(go);

            var controller = go.AddComponent<GameplayHudCanvasController>();
            var binder = go.AddComponent<GameplayHudRuntimeBinder>();
            var feedbackService = go.AddComponent<GameplayFeedbackService>();
            var visibilityController = go.AddComponent<HudVisibilityController>();

            var viewModel = new GameplayHudViewModel();
            controller.Initialize(viewModel, binder, feedbackService, visibilityController);

            // fable_62: servico de hints de onboarding (mesmo GameObject DontDestroyOnLoad do HUD;
            // sem GameObject.Find). Consome eventos existentes e publica no canal de toast WI-23.
            go.AddComponent<CindarsHope.UI.Onboarding.OnboardingHintService>();

            Debug.Log("[GameplayHudBootstrap] GameplayHudCanvas created via RuntimeInitializeOnLoadMethod.");
        }
    }
}
