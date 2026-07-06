using System.Collections;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Gameplay.Narrative;
using CindarsHope.Quests.Runtime;
using UnityEngine;

namespace CindarsHope.Narrative
{
    /// <summary>
    /// fable_63 — bootstrap unico do conteudo narrativo de abertura. DontDestroyOnLoad singleton,
    /// padrao identico a QuestRuntimeBootstrap (RuntimeInitializeOnLoadMethod).
    ///
    /// Responsabilidades:
    /// - expor o <see cref="NarrativeFlagStore"/> sobre a familia de flags ja persistida do
    ///   QuestStateSection (GlobalKnownHints) para a intro e a carta consumirem (sem Find);
    /// - criar o <see cref="MainQuestHookService"/> e assina-lo ao DayStartedEvent (oferta
    ///   idempotente da mq_act1_00 no 1o dia);
    /// - default seguro de save legado: se o save ja tem quest ativa/progresso ao iniciar, a intro
    ///   e tratada como vista (a flag IntroSeen e selada) — nao interromper um save em progresso.
    ///
    /// Sem GameObject.Find em runtime (so o singleton + acesso ao QuestService estatico). Toda
    /// comunicacao de gameplay via GameEventBus.
    /// </summary>
    public sealed class NarrativeRuntimeBootstrap : MonoBehaviour
    {
        private const int MaxBindAttempts = 180;

        private static NarrativeRuntimeBootstrap _instance;

        /// <summary>Flag store narrativa (intro/carta/hook). Null ate o QuestService estar pronto.</summary>
        public static NarrativeFlagStore FlagStore { get; private set; }

        /// <summary>Hook da quest-ponte (exposto para diagnostico/testes; evita Find).</summary>
        public static MainQuestHookService MainQuestHook { get; private set; }

        private bool _dayStartedWired;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (_instance != null) return;
            var go = new GameObject("NarrativeRuntimeBootstrap");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<NarrativeRuntimeBootstrap>();
        }

        private void OnEnable()
        {
            StartCoroutine(InitializeWhenReady());
        }

        private IEnumerator InitializeWhenReady()
        {
            if (FlagStore != null)
            {
                yield break;
            }

            for (var attempt = 0; attempt < MaxBindAttempts; attempt++)
            {
                var questService = QuestRuntimeBootstrap.QuestService;
                if (questService == null)
                {
                    yield return null;
                    continue;
                }

                Initialize(questService);
                yield break;
            }

            Debug.LogWarning("[NarrativeRuntimeBootstrap] QuestService nao encontrado apos max tentativas. " +
                "Intro/quest-ponte ficarao inativos nesta sessao. " +
                "Cena: runtime | componente: NarrativeRuntimeBootstrap | dependencia: QuestRuntimeBootstrap.QuestService.");
        }

        private void Initialize(QuestService questService)
        {
            var section = questService.GetSaveSection();
            FlagStore = new NarrativeFlagStore(section.GlobalKnownHints);

            // Default seguro p/ save legado em progresso: se ja existe estado de quest carregado
            // (save no meio), a intro e tratada como vista — nao reabrir a tela de abertura.
            if (!FlagStore.IsSet(NarrativeIds.FlagIntroSeen) && section.QuestStates != null && section.QuestStates.Count > 0)
            {
                FlagStore.Set(NarrativeIds.FlagIntroSeen);
            }

            MainQuestHook = new MainQuestHookService(FlagStore, questService.AcceptQuest);

            GameEventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
            GameEventBus.Subscribe<DayStartedEvent>(OnDayStarted);
            _dayStartedWired = true;

            Debug.Log("[NarrativeRuntimeBootstrap] Narrativa inicializada. FlagStore WIRED; hook da mq_act1_00 assinado ao DayStarted.");
        }

        private void OnDayStarted(DayStartedEvent evt)
        {
            if (MainQuestHook == null) return;
            MainQuestHook.OnDayStarted();
        }

        private void OnDisable()
        {
            if (_dayStartedWired)
            {
                GameEventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
                _dayStartedWired = false;
            }
        }
    }
}
