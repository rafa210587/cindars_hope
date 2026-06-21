using System;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Player.Death;
using CindarsHope.UI.Modal;
using CindarsHope.UI.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CindarsHope.UI.Death
{
    /// <summary>
    /// Canvas-based death screen (replaces the IMGUI DeathScreenController).
    ///
    /// Responsibilities (SURFACE ONLY — no death/penalty/corpse rule lives here):
    /// - subscribes to the same triggers (PlayerDiedEvent / CavePlayerDefeatedEvent);
    /// - publishes the same contract (DeathScreenOpenedEvent / DeathScreenClosedEvent);
    /// - reads the corpse snapshot from CorpseRecoveryManager (authoritative source);
    /// - caches cave level + XP loss from CavePlayerDeathResolvedEvent / XpResetToLevelStartEvent;
    /// - shows a keyboard-navigable canvas; Esc does NOT close (death requires a choice).
    ///
    /// Respawn already happens at death time inside DeathSystemBootstrap. The "Renascer na
    /// Fonte" action therefore only DISMISSES the screen — it never starts a second respawn
    /// path. The dismiss path is abstracted via <see cref="DeathScreenDismissed"/> so the
    /// contract can be EditMode-tested without a live scene.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DeathScreenCanvasController : MonoBehaviour
    {
        public static DeathScreenCanvasController Instance { get; private set; }

        // Cached cross-event state (subscription order is not guaranteed).
        private int _lastResolvedCaveLevel = -1;
        private int _lastXpLost;

        private bool _isShowing;
        private DeathScreenViewModel _viewModel;
        private readonly UiFocusController _focus = new();

        // Canvas refs (built lazily, programmatic — WI-23 fallback, no prefab/scene).
        private Canvas _canvas;
        private GameObject _root;
        private Text _bodyText;
        private Button _respawnButton;
        private Button _futureButton;

        /// <summary>
        /// Raised when the screen is dismissed by the player. The MonoBehaviour itself
        /// publishes DeathScreenClosedEvent; a test can subscribe to verify the action fires
        /// the dismiss path exactly once without re-triggering respawn.
        /// </summary>
        public event Action DeathScreenDismissed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (Instance != null) return;

            var go = new GameObject("DeathScreenCanvas");
            DontDestroyOnLoad(go);
            go.AddComponent<DeathScreenCanvasController>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
            GameEventBus.Subscribe<CavePlayerDefeatedEvent>(OnCavePlayerDefeated);
            GameEventBus.Subscribe<CavePlayerDeathResolvedEvent>(OnCaveDeathResolved);
            GameEventBus.Subscribe<XpResetToLevelStartEvent>(OnXpReset);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
            GameEventBus.Unsubscribe<CavePlayerDefeatedEvent>(OnCavePlayerDefeated);
            GameEventBus.Unsubscribe<CavePlayerDeathResolvedEvent>(OnCaveDeathResolved);
            GameEventBus.Unsubscribe<XpResetToLevelStartEvent>(OnXpReset);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        // ---- Cross-event caching (no UI side effect) ----

        private void OnCaveDeathResolved(CavePlayerDeathResolvedEvent evt)
        {
            _lastResolvedCaveLevel = evt.CaveLevel;
        }

        private void OnXpReset(XpResetToLevelStartEvent evt)
        {
            _lastXpLost = evt.XpLost;
        }

        // ---- Triggers (preserved) ----

        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            if (_isShowing) return;

            if (IsCaveScene(evt.SceneName))
            {
                ShowCave(ResolveCaveLevel());
                return;
            }

            Show(DeathScreenViewModel.ForOverworldDeath(evt.SceneName));
        }

        private void OnCavePlayerDefeated(CavePlayerDefeatedEvent evt)
        {
            if (_isShowing) return;
            ShowCave(evt.CaveLevel > 0 ? evt.CaveLevel : ResolveCaveLevel());
        }

        private void ShowCave(int caveLevel)
        {
            var corpse = BuildCorpseSnapshot();
            Show(DeathScreenViewModel.ForCaveDeath(caveLevel, corpse, _lastXpLost));
        }

        private int ResolveCaveLevel()
        {
            return _lastResolvedCaveLevel > 0 ? _lastResolvedCaveLevel : 0;
        }

        private static bool IsCaveScene(string sceneName)
        {
            return !string.IsNullOrEmpty(sceneName) && sceneName.Contains("Cave");
        }

        /// <summary>
        /// Reads the authoritative corpse snapshot from CorpseRecoveryManager. Returns null
        /// when there is no active corpse (overworld death, or nothing was lost).
        /// </summary>
        private DeathScreenViewModel.CorpseSnapshot? BuildCorpseSnapshot()
        {
            var manager = ResolveCorpseRecoveryManager();
            if (manager == null || !manager.HasActiveCorpse)
            {
                return null;
            }

            var corpse = manager.ActiveCorpse;
            int itemCount = corpse.GetTotalRecoverableItems();
            int gold = corpse.GetTotalRecoverableGold();
            return new DeathScreenViewModel.CorpseSnapshot(itemCount, gold, corpse.CaveLevel);
        }

        private static CorpseRecoveryManager ResolveCorpseRecoveryManager()
        {
            var bootstrap = GameBootstrap.Instance;
            return bootstrap != null ? bootstrap.CorpseRecoveryManager : null;
        }

        // ---- Show / dismiss (contract preserved) ----

        private void Show(DeathScreenViewModel viewModel)
        {
            if (_isShowing) return;

            _viewModel = viewModel;
            _isShowing = true;

            PushModal();
            BuildOrRefreshCanvas();
            SetupFocus();

            GameEventBus.Publish(new DeathScreenOpenedEvent());
        }

        private void Dismiss()
        {
            if (!_isShowing) return;

            _isShowing = false;
            HideCanvas();
            PopModal();

            // Reset cached penalty data for the next death.
            _lastResolvedCaveLevel = -1;
            _lastXpLost = 0;

            DeathScreenDismissed?.Invoke();
            GameEventBus.Publish(new DeathScreenClosedEvent());
        }

        private void PushModal()
        {
            var modalManager = GameBootstrap.Instance != null ? GameBootstrap.Instance.ModalManager : null;
            modalManager?.PushModal(ModalType.Death);
        }

        private void PopModal()
        {
            var modalManager = GameBootstrap.Instance != null ? GameBootstrap.Instance.ModalManager : null;
            modalManager?.TryPopIfCurrent(ModalType.Death);
        }

        // ---- Keyboard navigation ----

        private void SetupFocus()
        {
            // Only the respawn action is selectable; the future slot is disabled.
            _focus.SetElements(new[] { DeathScreenViewModel.RespawnActionId });
            RefreshFocusVisuals();
        }

        private void Update()
        {
            if (!_isShowing) return;

            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                _focus.Apply(UiFocusController.FocusInput.Next);
                RefreshFocusVisuals();
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                _focus.Apply(UiFocusController.FocusInput.Previous);
                RefreshFocusVisuals();
            }
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                Dismiss();
            }
            // Esc intentionally does NOT close: death requires an explicit choice.
        }

        // ---- Canvas construction (programmatic — WI-23 fallback pattern) ----

        private void BuildOrRefreshCanvas()
        {
            if (_root == null)
            {
                BuildCanvas();
            }

            if (_bodyText != null)
            {
                _bodyText.text = ComposeBodyText(_viewModel);
            }

            if (_root != null)
            {
                _root.SetActive(true);
            }
        }

        private void HideCanvas()
        {
            if (_root != null)
            {
                _root.SetActive(false);
            }
        }

        private void BuildCanvas()
        {
            _root = new GameObject("DeathScreenRoot");
            _root.transform.SetParent(transform, false);

            _canvas = _root.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 900;
            _root.AddComponent<CanvasScaler>();
            _root.AddComponent<GraphicRaycaster>();

            var panel = CreateChild(_root.transform, "Panel");
            var panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.85f);
            StretchFull(panel.GetComponent<RectTransform>());

            var bodyGo = CreateChild(panel.transform, "Body");
            _bodyText = bodyGo.AddComponent<Text>();
            _bodyText.alignment = TextAnchor.MiddleCenter;
            _bodyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _bodyText.color = Color.white;
            var bodyRect = bodyGo.GetComponent<RectTransform>();
            bodyRect.anchorMin = new Vector2(0.1f, 0.35f);
            bodyRect.anchorMax = new Vector2(0.9f, 0.95f);
            bodyRect.offsetMin = Vector2.zero;
            bodyRect.offsetMax = Vector2.zero;

            _respawnButton = CreateButton(panel.transform, "RespawnButton",
                new Vector2(0.3f, 0.18f), new Vector2(0.7f, 0.26f),
                "Renascer na Fonte de Anya", true, Dismiss);

            _futureButton = CreateButton(panel.transform, "FutureButton",
                new Vector2(0.3f, 0.06f), new Vector2(0.7f, 0.14f),
                "Recuperar no local (em breve)", false, null);
        }

        private Button CreateButton(
            Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            string label, bool interactable, Action onClick)
        {
            var go = CreateChild(parent, name);
            var image = go.AddComponent<Image>();
            image.color = interactable
                ? new Color(0.2f, 0.2f, 0.3f, 1f)
                : new Color(0.15f, 0.15f, 0.15f, 1f);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var button = go.AddComponent<Button>();
            button.interactable = interactable;
            if (onClick != null)
            {
                button.onClick.AddListener(() => onClick());
            }

            var textGo = CreateChild(go.transform, "Label");
            var text = textGo.AddComponent<Text>();
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.color = interactable ? Color.white : new Color(0.6f, 0.6f, 0.6f, 1f);
            text.text = label;
            StretchFull(textGo.GetComponent<RectTransform>());

            return button;
        }

        private void RefreshFocusVisuals()
        {
            if (_respawnButton == null) return;

            bool respawnFocused = _focus.IsFocused(DeathScreenViewModel.RespawnActionId);
            var image = _respawnButton.GetComponent<Image>();
            if (image != null)
            {
                image.color = respawnFocused
                    ? new Color(0.35f, 0.35f, 0.5f, 1f)
                    : new Color(0.2f, 0.2f, 0.3f, 1f);
            }
        }

        private static GameObject CreateChild(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        /// <summary>
        /// Pure text composition from the projection. Static so it is EditMode-testable.
        /// </summary>
        public static string ComposeBodyText(DeathScreenViewModel vm)
        {
            if (vm == null) return string.Empty;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine(vm.Headline);
            sb.AppendLine();
            sb.AppendLine($"Local: {vm.LocationLabel}");

            if (vm.HasCorpse)
            {
                sb.AppendLine();
                sb.AppendLine($"Corpo: {vm.CorpseItemCount} itens + {vm.CorpseGold} ouro deixados");
                if (vm.CorpseCaveLevel > 0)
                {
                    sb.AppendLine($"Nivel do corpo: {vm.CorpseCaveLevel}");
                }
                sb.AppendLine(vm.RecoveryInstruction);
            }
            else
            {
                sb.AppendLine();
                sb.AppendLine(vm.RecoveryInstruction);
            }

            if (vm.HasXpLoss)
            {
                sb.AppendLine();
                sb.AppendLine($"XP perdido: {vm.XpLost}");
            }

            return sb.ToString();
        }
    }
}
