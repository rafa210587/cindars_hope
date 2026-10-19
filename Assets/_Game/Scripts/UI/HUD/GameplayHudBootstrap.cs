using UnityEngine;

namespace CindarsHope.UI.HUD
{
    public static class GameplayHudBootstrap
    {
        private const string HudGameObjectName = "GameplayHudCanvas";

        public static void Install(Transform owner)
        {
            if (GameplayHudCanvasController.Instance != null) return;

            var go = new GameObject(HudGameObjectName);
            go.transform.SetParent(owner);
            if (owner == null) Object.DontDestroyOnLoad(go);

            var controller = go.AddComponent<GameplayHudCanvasController>();
            var binder = go.AddComponent<GameplayHudRuntimeBinder>();
            var feedbackService = go.AddComponent<GameplayFeedbackService>();
            var visibilityController = go.AddComponent<HudVisibilityController>();

            var viewModel = new GameplayHudViewModel();
            controller.Initialize(viewModel, binder, feedbackService, visibilityController);

            // fable_62: servico de hints de onboarding (mesmo GameObject DontDestroyOnLoad do HUD;
            // sem GameObject.Find). Consome eventos existentes e publica no canal de toast WI-23.
            go.AddComponent<CindarsHope.UI.Onboarding.OnboardingHintService>();

            // HUD de texto VISIVEL (relogio/dia/fase + vitais + ouro + prompt). Canvas proprio
            // (sortingOrder alto) montado em codigo; fecha o gap "headless" do canvas controller.
            // GameObject dedicado p/ nao colidir com as Views do controller no mesmo transform.
            var overlayGo = new GameObject("GameplayHudTextOverlay");
            Object.DontDestroyOnLoad(overlayGo);
            var overlay = overlayGo.AddComponent<GameplayHudTextOverlay>();
            overlay.Build();

            Debug.Log("[GameplayHudBootstrap] GameplayHudCanvas created via composition root Install.");
        }
    }
}
