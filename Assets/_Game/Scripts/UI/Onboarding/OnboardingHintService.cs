using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.UI.HUD;
using UnityEngine;

namespace CindarsHope.UI.Onboarding
{
    /// <summary>
    /// fable_62 — servico de hints de onboarding de primeira ocorrencia. Adapter FINO: a decisao de
    /// primeira-ocorrencia vive em <see cref="OnboardingHintTracker"/> (puro, testavel); aqui apenas
    /// assinamos eventos EXISTENTES do GameEventBus, traduzimos o gatilho em um hintId e publicamos
    /// no canal de toast WI-23 existente (PlayerActionFeedbackEvent). NUNCA bloqueia gameplay, NUNCA
    /// abre modal, NUNCA usa GameObject.Find. Wired via GameplayHudBootstrap (mesmo GameObject
    /// DontDestroyOnLoad do HUD). Unsubscribe pareado em OnDisable.
    ///
    /// Gatilhos (todos eventos ja existentes):
    ///   hint_move        — Awake/OnEnable do servico (1o frame de gameplay pos-bootstrap do HUD);
    ///   hint_interact    — 1o InteractionPromptChangedEvent com HasCandidate;
    ///   hint_attack      — 1o EquipmentSlotChangedEvent de arma (RightHand/LeftHand) com item;
    ///   hint_cave_danger — 1o CaveLevelEnteredEvent (prioridade Important — ver nota CA-2);
    ///   hint_status      — 1o StatusAppliedEvent no player (TargetId == "player").
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class OnboardingHintService : MonoBehaviour
    {
        // Id do alvo de status do player (StatusEffectManager.PlayerTargetId — auditado Fase 0).
        private const string PlayerStatusTargetId = "player";

        private readonly OnboardingHintTracker _tracker = new OnboardingHintTracker();

        /// <summary>
        /// Instancia ativa (padrao F07 PlayerSpellbook.Instance / F13/F17): o provider de save
        /// resolve o tracker por aqui no momento de capture/restore, sem acoplar SaveManager ao
        /// servico nem usar GameObject.Find.
        /// </summary>
        public static OnboardingHintService Instance { get; private set; }

        /// <summary>Tracker em memoria (fonte de verdade dos hints vistos). Usado pelo provider.</summary>
        public OnboardingHintTracker Tracker => _tracker;

        private void OnEnable()
        {
            Instance = this;
            GameEventBus.Subscribe<InteractionPromptChangedEvent>(OnInteractionPrompt);
            GameEventBus.Subscribe<EquipmentSlotChangedEvent>(OnEquipmentSlotChanged);
            GameEventBus.Subscribe<CaveLevelEnteredEvent>(OnCaveLevelEntered);
            GameEventBus.Subscribe<StatusAppliedEvent>(OnStatusApplied);

            // hint_move: gameplay HUD ja esta vivo quando este componente habilita (pos-bootstrap,
            // AfterSceneLoad). Dispara no primeiro frame de gameplay, 1x/save.
            TryShowHint(OnboardingHintCatalog.HintMove);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<InteractionPromptChangedEvent>(OnInteractionPrompt);
            GameEventBus.Unsubscribe<EquipmentSlotChangedEvent>(OnEquipmentSlotChanged);
            GameEventBus.Unsubscribe<CaveLevelEnteredEvent>(OnCaveLevelEntered);
            GameEventBus.Unsubscribe<StatusAppliedEvent>(OnStatusApplied);
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Ponto unico de exibicao: checa primeira-ocorrencia no tracker e, se for a 1a vez,
        /// publica o toast no canal existente. Idempotente. Retorna true se publicou.
        /// </summary>
        public bool TryShowHint(string hintId)
        {
            if (!OnboardingHintCatalog.TryGet(hintId, out var entry))
            {
                return false;
            }

            if (!_tracker.TryMarkSeen(hintId))
            {
                return false; // ja visto neste save — nunca re-dispara.
            }

            PublishToast(entry);
            return true;
        }

        private static void PublishToast(OnboardingHintCatalog.HintEntry entry)
        {
            // Canal unico de toast WI-23 (GameplayFeedbackService consome PlayerActionFeedbackEvent).
            // NOTA CA-2: o canal Normal nao carrega prioridade; a marca Important do catalogo e
            // honrada por duracao estendida do aviso da caverna. Furar a fila (Important real)
            // exigiria editar GameplayFeedbackService (arquivo proibido por esta spec) — registrado
            // como debito no execution report.
            GameEventBus.Publish(new PlayerActionFeedbackEvent(entry.Text, entry.DurationSeconds));
        }

        private void OnInteractionPrompt(InteractionPromptChangedEvent evt)
        {
            if (evt.HasCandidate)
            {
                TryShowHint(OnboardingHintCatalog.HintInteract);
            }
        }

        private void OnEquipmentSlotChanged(EquipmentSlotChangedEvent evt)
        {
            if (evt == null || string.IsNullOrEmpty(evt.ItemInstanceId))
            {
                return; // desequipar nao conta como "1a arma equipada".
            }

            if (evt.Slot == CindarsHope.Foundation.EquipmentSlot.RightHand ||
                evt.Slot == CindarsHope.Foundation.EquipmentSlot.LeftHand)
            {
                TryShowHint(OnboardingHintCatalog.HintAttack);
            }
        }

        private void OnCaveLevelEntered(CaveLevelEnteredEvent evt)
        {
            TryShowHint(OnboardingHintCatalog.HintCaveDanger);
        }

        private void OnStatusApplied(StatusAppliedEvent evt)
        {
            if (evt != null && evt.TargetId == PlayerStatusTargetId)
            {
                TryShowHint(OnboardingHintCatalog.HintStatus);
            }
        }
    }
}
