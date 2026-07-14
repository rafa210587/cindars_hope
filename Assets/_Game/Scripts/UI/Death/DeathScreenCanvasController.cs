using System;
using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Core.Time;
using CindarsHope.Inventory;
using CindarsHope.Player.Death;
using CindarsHope.UI.Modal;
using CindarsHope.UI.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace CindarsHope.UI.Death
{
    /// <summary>
    /// Canvas-based death screen "Voce Morreu" (replaces the IMGUI DeathScreenController).
    ///
    /// Responsibilities (SURFACE ONLY — no death/penalty/corpse rule lives here):
    /// - subscribes to the triggers (PlayerDiedEvent / CavePlayerDefeatedEvent);
    /// - publishes the contract (DeathScreenOpenedEvent / DeathScreenClosedEvent);
    /// - reads the corpse snapshot from CorpseRecoveryManager (authoritative source);
    /// - reads the Lagrima da Deusa count from InventoryManager at show time;
    /// - PAUSES the game (Time.timeScale=0 + ModalManager.Death) while open; Esc does NOT close.
    ///
    /// Death no longer auto-respawns (DeathSystemBootstrap only creates the corpse). The player
    /// chooses on this screen:
    /// - "Usar Lagrima da Deusa (reviver aqui)" — enabled only if count &gt; 0: consumes 1, heals
    ///   to full HP, dismisses, unpauses. Player is alive IN PLACE.
    /// - "Respawnar na Fonte da Anya" — always enabled: starts the (possibly cross-scene) respawn
    ///   via AnyaFountainRespawnFlow, dismisses, unpauses.
    /// The dismiss path is observable via <see cref="DeathScreenDismissed"/> for EditMode contract
    /// tests. Item-id + tear count are exposed as testable static helpers.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DeathScreenCanvasController : MonoBehaviour
    {
        public static DeathScreenCanvasController Instance { get; private set; }

        /// <summary>Stable id of the revive consumable (catalog: CanonicalItemCatalog AddSpecials).</summary>
        public const string GoddessTearItemId = "item_goddess_tear";

        // Cached cross-event state (subscription order is not guaranteed).
        private int _lastResolvedCaveLevel = -1;
        private int _lastXpLost;

        private bool _isShowing;
        private IDisposable _pauseToken;
        private DeathScreenViewModel _viewModel;
        private readonly UiFocusController _focus = new();

        // Canvas refs (built lazily, programmatic — WI-23 fallback, no prefab/scene).
        private Canvas _canvas;
        private GameObject _root;
        private Text _titleText;
        private Text _bodyText;
        private Button _reviveButton;
        private Text _reviveLabel;
        private Button _respawnButton;

        /// <summary>
        /// Raised when the screen is dismissed by the player. The MonoBehaviour itself
        /// publishes DeathScreenClosedEvent; a test can subscribe to verify the action fires
        /// the dismiss path exactly once without re-triggering respawn.
        /// </summary>
        public event Action DeathScreenDismissed;

        public static void Install(Transform owner)
        {
            if (Instance != null) return;

            var go = new GameObject("DeathScreenCanvas");
            go.transform.SetParent(owner);
            if (owner == null) DontDestroyOnLoad(go);
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

            // Resolve a contagem de Lagrima da Deusa AGORA (do inventario autoritativo) e injeta na
            // projection, para o botao de revive saber se habilita e mostrar "(N)".
            _viewModel = viewModel.WithGoddessTearCount(ResolveGoddessTearCount());
            _isShowing = true;

            PauseGame();
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
            UnpauseGame();

            // Reset cached penalty data for the next death.
            _lastResolvedCaveLevel = -1;
            _lastXpLost = 0;

            DeathScreenDismissed?.Invoke();
            GameEventBus.Publish(new DeathScreenClosedEvent());
        }

        // ---- Pause (Time.timeScale + ModalManager) ----

        private void PauseGame()
        {
            _pauseToken ??= GameTimeScaleCoordinator.AcquirePause();
        }

        private void UnpauseGame()
        {
            _pauseToken?.Dispose();
            _pauseToken = null;
        }

        // ---- Inventory: Lagrima da Deusa ----

        private static InventoryManager ResolveInventoryManager()
        {
            var bootstrap = GameBootstrap.Instance;
            return bootstrap != null ? bootstrap.InventoryManager : null;
        }

        private static int ResolveGoddessTearCount()
        {
            var inventory = ResolveInventoryManager();
            return inventory != null ? inventory.GetAmount(GoddessTearItemId) : 0;
        }

        /// <summary>
        /// Acao do botao "Usar Lagrima da Deusa": consome 1, cura HP cheio e fecha a tela. So roda
        /// se ainda houver pelo menos 1 (guard contra clique tardio). Reviver no LUGAR (sem mover).
        /// </summary>
        private void OnReviveWithTear()
        {
            if (!_isShowing) return;

            var inventory = ResolveInventoryManager();
            if (inventory == null || !inventory.HasItem(GoddessTearItemId, 1))
            {
                Debug.LogWarning("[DeathScreen] Revive recusado: sem Lagrima da Deusa no inventario.");
                return;
            }

            if (!inventory.RemoveItem(GoddessTearItemId, 1))
            {
                Debug.LogWarning("[DeathScreen] Revive recusado: RemoveItem falhou para Lagrima da Deusa.");
                return;
            }

            var bootstrap = GameBootstrap.Instance;
            var playerManager = bootstrap != null ? bootstrap.PlayerManager : null;
            if (playerManager != null)
            {
                // SetHP(MaxHP) re-arma o PlayerDeathController (HP volta > 0).
                playerManager.SetHP(playerManager.MaxHP);
            }

            Debug.Log("[DeathScreen] Revive com Lagrima da Deusa: -1 item, HP cheio, jogador vivo no lugar.");
            Dismiss();
        }

        /// <summary>
        /// Acao do botao "Respawnar na Fonte da Anya": inicia o fluxo (possivelmente cross-cena) e
        /// fecha a tela. Anti-softlock: sempre disponivel.
        /// </summary>
        private void OnRespawnAtFountain()
        {
            if (!_isShowing) return;

            // Fecha/despausa ANTES de carregar a cena (a transicao destroi a cena atual). O fluxo
            // completa o teleporte/restauracao quando a cena da Fonte termina de carregar.
            Dismiss();

            var flow = AnyaFountainRespawnFlow.Instance;
            if (flow != null)
            {
                flow.Respawn();
            }
            else
            {
                // Fallback extremo: o flow ainda nao nasceu. Revive no lugar para nao travar.
                var bootstrap = GameBootstrap.Instance;
                var playerManager = bootstrap != null ? bootstrap.PlayerManager : null;
                if (playerManager != null)
                {
                    playerManager.SetHP(playerManager.MaxHP);
                }
                Debug.LogWarning("[DeathScreen] AnyaFountainRespawnFlow ausente; revivendo no lugar (anti-softlock).");
            }
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
            // Focus order: somente as acoes HABILITADAS sao selecionaveis. O revive so entra na
            // ordem se houver Lagrima da Deusa; a Fonte esta sempre presente (anti-softlock).
            var ids = new List<string>(2);
            if (_viewModel != null && _viewModel.CanReviveWithTear)
            {
                ids.Add(DeathScreenViewModel.ReviveActionId);
            }
            ids.Add(DeathScreenViewModel.RespawnActionId);

            _focus.SetElements(ids);
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
                ActivateFocusedAction();
            }
            // Esc intentionally does NOT close: death requires an explicit choice.
        }

        private void ActivateFocusedAction()
        {
            if (_focus.IsFocused(DeathScreenViewModel.ReviveActionId))
            {
                OnReviveWithTear();
            }
            else
            {
                OnRespawnAtFountain();
            }
        }

        // ---- Canvas construction (programmatic — WI-23 fallback pattern) ----

        private void BuildOrRefreshCanvas()
        {
            if (_root == null)
            {
                BuildCanvas();
            }

            if (_titleText != null)
            {
                _titleText.text = DeathScreenViewModel.DeathTitle;
            }

            if (_bodyText != null)
            {
                _bodyText.text = ComposeBodyText(_viewModel);
            }

            // O canvas e construido uma vez e reusado: refaz o estado do botao de revive a cada show
            // (a contagem de Lagrima da Deusa muda entre mortes).
            RefreshReviveButton();

            if (_root != null)
            {
                _root.SetActive(true);
            }
        }

        private void RefreshReviveButton()
        {
            if (_viewModel == null) return;

            bool canRevive = _viewModel.CanReviveWithTear;

            if (_reviveLabel != null)
            {
                _reviveLabel.text = _viewModel.ReviveActionLabel;
                _reviveLabel.color = canRevive ? Color.white : new Color(0.6f, 0.6f, 0.6f, 1f);
            }

            if (_reviveButton != null)
            {
                _reviveButton.interactable = canRevive;
                var image = _reviveButton.GetComponent<Image>();
                if (image != null)
                {
                    image.color = canRevive
                        ? new Color(0.2f, 0.3f, 0.2f, 1f)
                        : new Color(0.15f, 0.15f, 0.15f, 1f);
                }
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

            // Button.onClick exige EventSystem; o jogo usa IMGUI em varias telas entao pode estar ausente.
            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                var esGo = new GameObject("EventSystem");
                DontDestroyOnLoad(esGo);
                esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Debug.Log("[DeathScreen] EventSystem criado (ausente na cena).");
            }

            var panel = CreateChild(_root.transform, "Panel");
            var panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.85f);
            StretchFull(panel.GetComponent<RectTransform>());

            // Titulo "Voce Morreu" em destaque no topo.
            var titleGo = CreateChild(panel.transform, "Title");
            _titleText = titleGo.AddComponent<Text>();
            _titleText.alignment = TextAnchor.MiddleCenter;
            _titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _titleText.fontSize = 48;
            _titleText.fontStyle = FontStyle.Bold;
            _titleText.color = new Color(0.85f, 0.2f, 0.2f, 1f);
            _titleText.text = DeathScreenViewModel.DeathTitle;
            var titleRect = titleGo.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.74f);
            titleRect.anchorMax = new Vector2(0.9f, 0.92f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            var bodyGo = CreateChild(panel.transform, "Body");
            _bodyText = bodyGo.AddComponent<Text>();
            _bodyText.alignment = TextAnchor.MiddleCenter;
            _bodyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _bodyText.color = Color.white;
            var bodyRect = bodyGo.GetComponent<RectTransform>();
            bodyRect.anchorMin = new Vector2(0.1f, 0.40f);
            bodyRect.anchorMax = new Vector2(0.9f, 0.72f);
            bodyRect.offsetMin = Vector2.zero;
            bodyRect.offsetMax = Vector2.zero;

            // Botao 1: Usar Lagrima da Deusa (reviver aqui). Interatividade/label sao ajustados por
            // RefreshReviveButton a cada show (dependem da contagem).
            _reviveButton = CreateButton(panel.transform, "ReviveButton",
                new Vector2(0.25f, 0.22f), new Vector2(0.75f, 0.32f),
                "Usar Lagrima da Deusa (reviver aqui)", true, OnReviveWithTear, out _reviveLabel);

            // Botao 2: Respawnar na Fonte da Anya (sempre habilitado).
            _respawnButton = CreateButton(panel.transform, "RespawnButton",
                new Vector2(0.25f, 0.08f), new Vector2(0.75f, 0.18f),
                "Respawnar na Fonte da Anya", true, OnRespawnAtFountain, out _);
        }

        private Button CreateButton(
            Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            string label, bool interactable, Action onClick, out Text labelText)
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
            labelText = textGo.AddComponent<Text>();
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.color = interactable ? Color.white : new Color(0.6f, 0.6f, 0.6f, 1f);
            labelText.text = label;
            StretchFull(textGo.GetComponent<RectTransform>());

            return button;
        }

        private void RefreshFocusVisuals()
        {
            if (_respawnButton != null)
            {
                bool respawnFocused = _focus.IsFocused(DeathScreenViewModel.RespawnActionId);
                ApplyFocusColor(_respawnButton, respawnFocused, enabled: true);
            }

            if (_reviveButton != null)
            {
                bool reviveFocused = _focus.IsFocused(DeathScreenViewModel.ReviveActionId);
                bool reviveEnabled = _viewModel != null && _viewModel.CanReviveWithTear;
                ApplyFocusColor(_reviveButton, reviveFocused && reviveEnabled, reviveEnabled);
            }
        }

        private static void ApplyFocusColor(Button button, bool focused, bool enabled)
        {
            var image = button.GetComponent<Image>();
            if (image == null) return;

            if (!enabled)
            {
                image.color = new Color(0.15f, 0.15f, 0.15f, 1f);
                return;
            }

            image.color = focused
                ? new Color(0.35f, 0.35f, 0.5f, 1f)
                : new Color(0.2f, 0.2f, 0.3f, 1f);
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
