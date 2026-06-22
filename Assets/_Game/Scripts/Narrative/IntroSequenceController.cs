using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.UI.Modal;
using UnityEngine;

namespace CindarsHope.Narrative
{
    /// <summary>
    /// fable_63 — adapter MonoBehaviour FINO da sequencia de abertura (IMGUI, padrao do
    /// QuestLogPanelController). A logica vive em <see cref="IntroSequenceModel"/> (pura/testavel).
    ///
    /// Disparo: NewGameStartedEvent (F56 — New Game pela tela de titulo) OU boot de save novo
    /// (GameLoadedEvent com WasSuccessful=false e nenhuma quest carregada — fallback sem E56).
    /// Exibida exatamente 1x por save (flag IntroSeen na familia de hints persistida).
    ///
    /// Bloqueia o gameplay enquanto ativa (PushModal(ModalType.Dialogue) — mesmo bloqueio das
    /// telas de dialogo). Avanco por E/Enter; Esc pula tudo. Ao terminar/skipar, libera o modal e
    /// sela IntroSeen. A F56 continua dona do reset; aqui so ESCUTAMOS o fluxo.
    /// </summary>
    public sealed class IntroSequenceController : MonoBehaviour
    {
        public static IntroSequenceController Instance { get; private set; }

        private IntroSequenceModel _model;
        private bool _isOpen;
        private bool _modalPushed;

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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (Instance != null) return;
            var go = new GameObject("IntroSequenceController");
            DontDestroyOnLoad(go);
            go.AddComponent<IntroSequenceController>();
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<NewGameStartedEvent>(OnNewGameStarted);
            GameEventBus.Subscribe<GameLoadedEvent>(OnGameLoaded);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<NewGameStartedEvent>(OnNewGameStarted);
            GameEventBus.Unsubscribe<GameLoadedEvent>(OnGameLoaded);
            ReleaseModal();
        }

        // ─── Disparo ────────────────────────────────────────────────────────────────

        private void OnNewGameStarted(NewGameStartedEvent evt)
        {
            TryStartIntro();
        }

        // Fallback sem E56: um boot que NAO carregou save (save novo) tambem dispara a intro,
        // desde que a flag IntroSeen ainda nao esteja selada para este save.
        private void OnGameLoaded(GameLoadedEvent evt)
        {
            if (evt.WasSuccessful) return; // save carregado com sucesso => nao e jogo novo
            TryStartIntro();
        }

        /// <summary>
        /// Inicia a intro se ainda nao foi vista neste save. Idempotente: se a flag IntroSeen ja
        /// estiver selada (reload, save legado em progresso), nao reexibe.
        /// </summary>
        public void TryStartIntro()
        {
            if (_isOpen) return;

            var store = NarrativeRuntimeBootstrap.FlagStore;
            // Se o store ainda nao inicializou, seguimos: nao podemos checar IntroSeen, mas a intro
            // so seria perdida se o store falhasse permanentemente. Em pratica o store inicializa no
            // mesmo frame do boot. Se ja inicializado e a flag esta setada, abortamos.
            if (store != null && store.IsSet(NarrativeIds.FlagIntroSeen)) return;

            _model = new IntroSequenceModel();
            _isOpen = true;

            var mm = GameBootstrap.Instance?.ModalManager;
            if (mm != null)
            {
                _modalPushed = mm.PushModal(ModalType.Dialogue);
            }
        }

        private void Finish()
        {
            // Sela IntroSeen (1x/save) na familia de flags persistida.
            NarrativeRuntimeBootstrap.FlagStore?.Set(NarrativeIds.FlagIntroSeen);
            ReleaseModal();
            _isOpen = false;
            _model = null;
        }

        private void ReleaseModal()
        {
            if (!_modalPushed) return;
            GameBootstrap.Instance?.ModalManager?.TryPopModal(ModalType.Dialogue, out _);
            _modalPushed = false;
        }

        // ─── Input / render ───────────────────────────────────────────────────────────

        private void Update()
        {
            if (!_isOpen || _model == null) return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _model.Skip();
            }
            else if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                _model.Advance();
            }

            if (_model.Finished)
            {
                Finish();
            }
        }

        private void OnGUI()
        {
            if (!_isOpen || _model == null) return;
            CindarsHope.UI.MenuGuiStyle.Apply();
            var screen = _model.Current;
            if (screen == null) return;

            var rect = new Rect(Screen.width / 2f - 300, Screen.height / 2f - 120, 600, 240);
            GUI.Box(rect, "Cindar's Hope");

            GUI.Label(new Rect(rect.x + 16, rect.y + 34, rect.width - 32, 130), screen.Text);

            string nav = _model.IsOnLastScreen ? "[E/Enter] Comecar    [Esc] Pular" : "[E/Enter] Continuar    [Esc] Pular";
            GUI.Label(new Rect(rect.x + 16, rect.y + rect.height - 56, rect.width - 32, 20),
                $"Tela {_model.CurrentIndex + 1}/{_model.ScreenCount}");
            GUI.Label(new Rect(rect.x + 16, rect.y + rect.height - 34, rect.width - 32, 20), nav);
        }

        /// <summary>Exposto para testes/diagnostico (evita Find).</summary>
        public bool IsOpen => _isOpen;
    }
}
