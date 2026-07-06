using System;
using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Audio
{
    /// <summary>
    /// fable_58 — ÚNICO call site de SFX (CA-2). Assina os eventos audíveis v1 do
    /// GameEventBus (tabela <see cref="SfxEventMap"/>), aplica cooldown anti-spam por
    /// categoria e chama <see cref="AudioManager.PlaySfx"/>. Nenhum sistema de gameplay
    /// chama PlaySfx — o som SEMPRE nasce de um evento aqui.
    ///
    /// EMENDA V3 — também DERIVA o MusicState a partir de eventos já publicados:
    /// - EnemySpawnedEvent / EnemySeenEvent → inimigo engajado (entra em Combate);
    /// - EnemyKilledEvent → inimigo desengajado (sai de Combate se zerar);
    /// - CaveBossDefeatedEvent → boss inativo (auditoria Fase 0: NÃO há evento de
    ///   "boss começou"; boss-on fica disponível na API do resolver para quando um
    ///   sinal existir — ver report). Festival idem (sem evento no projeto v1).
    /// Quando o estado dominante muda, publica <see cref="MusicStateChangedEvent"/>
    /// (único evento próprio, autorizado pela EMENDA V3) e pede o crossfade ao
    /// AudioManager. Não há segundo caminho de troca de faixa.
    ///
    /// Subscribe/Unsubscribe simétrico (event_rules): assina em OnEnable, cancela em
    /// OnDisable. Instalado pelo GameRuntimeCompositionRoot no Start() (era self-bootstrap
    /// via RuntimeInitializeOnLoadMethod).
    /// Nunca lança (GameEventBus.Publish isola exceções de listeners; PlaySfx é
    /// fallback-silencioso).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SfxEventBridge : MonoBehaviour
    {
        private static SfxEventBridge _instance;
        public static SfxEventBridge Instance => _instance;

        private readonly SfxCooldownGate _cooldownGate = new SfxCooldownGate();
        private readonly MusicStateResolver _musicResolver = new MusicStateResolver();
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

        private MusicState _lastPublishedMusicState = MusicState.Calmo;
        private string _lastSceneName;

        public static void Install(Transform owner)
        {
            if (_instance != null)
            {
                return;
            }

            var go = new GameObject("SfxEventBridge");
            go.transform.SetParent(owner);
            DontDestroyOnLoad(go);
            go.AddComponent<SfxEventBridge>();
            Debug.Log("[Music] SfxEventBridge instanciado (driver de musica por cena/combate).");
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        private void OnEnable()
        {
            SubscribeAll();
            // Música por CENA: a cena de gameplay (Farm/Town/Cave) define o ambiente-base;
            // combate/boss/festival sobrepõem. Cobrimos carga inicial, carga aditiva e troca
            // de cena ativa — e o Update() garante o playback assim que o AudioManager existir.
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            DetectInitialSceneAmbient();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            UnsubscribeAll();
        }

        private void Update()
        {
            // POLL da cena ativa: as transicoes usam EditorSceneManager.LoadSceneInPlayMode, que
            // nem sempre dispara sceneLoaded/activeSceneChanged pros nossos handlers. GetActiveScene
            // SEMPRE reflete a cena atual, entao detectamos a troca aqui (a prova de bala) e
            // reaplicamos o ambiente. Depois sincronizamos a faixa ao estado resolvido toda frame
            // (musica comeca quando o AudioManager fica pronto e acompanha cena/combate). Barato.
            var activeName = SceneManager.GetActiveScene().name;
            if (activeName != _lastSceneName)
            {
                _lastSceneName = activeName;
                TryApplySceneAmbient(activeName);
            }

            PushMusicStateIfChanged();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TryApplySceneAmbient(scene.name);
        }

        private void OnActiveSceneChanged(Scene previous, Scene next)
        {
            TryApplySceneAmbient(next.name);
        }

        /// <summary>Varre as cenas carregadas e aplica o ambiente da primeira de gameplay encontrada.</summary>
        private void DetectInitialSceneAmbient()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                TryApplySceneAmbient(SceneManager.GetSceneAt(i).name);
            }
        }

        /// <summary>Se o nome for de cena de gameplay, define o ambiente; senão NÃO mexe (mantém o atual).</summary>
        private void TryApplySceneAmbient(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                return;
            }

            MusicState ambient;
            if (sceneName.Contains("Farm")) ambient = MusicState.Fazenda;
            else if (sceneName.Contains("Town")) ambient = MusicState.Cidade;
            else if (sceneName.Contains("Cave")) ambient = MusicState.Caverna;
            else return; // cena não-gameplay (bootstrap/persistente): mantém o ambiente atual

            Debug.Log($"[Music] Cena ativa='{sceneName}' -> ambiente={ambient}.");
            _musicResolver.SetSceneAmbient(ambient);
            PushMusicStateIfChanged();
        }

        private void OnDestroy()
        {
            UnsubscribeAll();
            if (_instance == this)
            {
                _instance = null;
            }
        }

        // --------------------------------------------------------------- subscriptions

        private void SubscribeAll()
        {
            if (_subscriptions.Count > 0)
            {
                return;
            }

            // SFX por categoria (eventos audíveis v1). Cada categoria gateada por cooldown.
            Add(GameEventBus.Subscribe<PlayerChargedAttackEvent>(_ => PlayMapped<PlayerChargedAttackEvent>()));
            Add(GameEventBus.Subscribe<EnemyPostureBrokenEvent>(_ => PlayMapped<EnemyPostureBrokenEvent>()));
            Add(GameEventBus.Subscribe<PlayerPerfectBlockEvent>(_ => PlayMapped<PlayerPerfectBlockEvent>()));
            Add(GameEventBus.Subscribe<StatusEffectAppliedEvent>(_ => PlayMapped<StatusEffectAppliedEvent>()));
            Add(GameEventBus.Subscribe<DamageAppliedEvent>(_ => PlayMapped<DamageAppliedEvent>()));
            Add(GameEventBus.Subscribe<PlayerDamagedEvent>(_ => PlayMapped<PlayerDamagedEvent>()));
            Add(GameEventBus.Subscribe<NotificationToastRequestedEvent>(_ => PlayMapped<NotificationToastRequestedEvent>()));
            Add(GameEventBus.Subscribe<PlayerActionFeedbackEvent>(_ => PlayMapped<PlayerActionFeedbackEvent>()));
            Add(GameEventBus.Subscribe<ItemPickedUpEvent>(_ => PlayMapped<ItemPickedUpEvent>()));
            Add(GameEventBus.Subscribe<ItemCraftedEvent>(_ => PlayMapped<ItemCraftedEvent>()));
            Add(GameEventBus.Subscribe<CropHarvestedEvent>(_ => PlayMapped<CropHarvestedEvent>()));
            Add(GameEventBus.Subscribe<FishCaughtEvent>(_ => PlayMapped<FishCaughtEvent>()));
            Add(GameEventBus.Subscribe<PlayerLevelChangedEvent>(_ => PlayMapped<PlayerLevelChangedEvent>()));
            Add(GameEventBus.Subscribe<DayStartedEvent>(_ => PlayMapped<DayStartedEvent>()));

            // EnemyKilledEvent: SFX de kill + desengajamento de combate (MusicState).
            Add(GameEventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled));
            // GameSavedEvent: SFX de save.
            Add(GameEventBus.Subscribe<GameSavedEvent>(_ => PlayMapped<GameSavedEvent>()));

            // EMENDA V3 — derivação de MusicState (combate):
            Add(GameEventBus.Subscribe<EnemySpawnedEvent>(_ => OnEnemyEngaged()));
            Add(GameEventBus.Subscribe<EnemySeenEvent>(_ => OnEnemyEngaged()));
            // Boss: único sinal existente é "boss derrotado" (auditoria Fase 0).
            Add(GameEventBus.Subscribe<CaveBossDefeatedEvent>(_ => SetBossActive(false)));
            // Transição de cena: os inimigos da cena atual vão embora SEM disparar kill/disengage,
            // então o combate ficaria "preso" e a música não voltaria ao ambiente da próxima cena.
            // Zera combate/boss ao SAIR (antes da próxima cena carregar; os inimigos dela re-engajam no Start dela).
            Add(GameEventBus.Subscribe<SceneTransitionStartedEvent>(_ => OnSceneTransitionStarted()));
        }

        private void Add(IDisposable subscription)
        {
            if (subscription != null)
            {
                _subscriptions.Add(subscription);
            }
        }

        private void UnsubscribeAll()
        {
            for (int i = 0; i < _subscriptions.Count; i++)
            {
                _subscriptions[i]?.Dispose();
            }

            _subscriptions.Clear();
        }

        /// <summary>Número de assinaturas ativas (diagnóstico/teste de vazamento).</summary>
        public int SubscriptionCount => _subscriptions.Count;

        // --------------------------------------------------------------- SFX dispatch

        private void PlayMapped<TEvent>()
        {
            var category = SfxEventMap.CategoryFor<TEvent>();
            Trigger(category);
        }

        private void OnEnemyKilled(EnemyKilledEvent evt)
        {
            Trigger(SfxEventMap.CategoryFor<EnemyKilledEvent>());
            OnEnemyDisengaged();
        }

        /// <summary>
        /// Gateia por cooldown e dispara o SFX. Exposto internamente para teste do
        /// caminho de rajada/anti-spam. Usa Time.unscaledTime quando há AudioManager
        /// (Play Mode); em EditMode use <see cref="TryTrigger"/>.
        /// </summary>
        private void Trigger(SfxCategory category)
        {
            float now = Time.unscaledTime;
            if (!_cooldownGate.TryConsume(category, now))
            {
                return;
            }

            AudioManager.Instance?.PlaySfx(category);
        }

        // --------------------------------------------------------------- MusicState

        private void OnSceneTransitionStarted()
        {
            // Limpa o estado de combate ao trocar de cena (inimigos antigos somem sem evento de
            // kill). Sem isto, sair da caverna em combate deixaria a música presa em Combate e o
            // ambiente da fazenda/cidade nunca tocaria.
            _musicResolver.SetEngagedEnemies(0);
            _musicResolver.SetBossActive(false);
            PushMusicStateIfChanged();
        }

        private void OnEnemyEngaged()
        {
            _musicResolver.EnemyEngaged();
            PushMusicStateIfChanged();
        }

        private void OnEnemyDisengaged()
        {
            _musicResolver.EnemyDisengaged();
            PushMusicStateIfChanged();
        }

        private void SetBossActive(bool active)
        {
            _musicResolver.SetBossActive(active);
            PushMusicStateIfChanged();
        }

        private void PushMusicStateIfChanged()
        {
            var manager = AudioManager.Instance;
            if (manager == null)
            {
                // AudioManager ainda não existe (ordem de bootstrap): NÃO avança o tracker,
                // para que o Update() tente de novo na próxima frame (a música começa quando
                // o manager ficar pronto). Sem isto, a faixa nunca tocaria fora de combate.
                return;
            }

            var next = _musicResolver.CurrentState;
            if (next == _lastPublishedMusicState)
            {
                return;
            }

            var previous = _lastPublishedMusicState;
            _lastPublishedMusicState = next;

            // Único evento próprio (EMENDA V3). Nenhum gameplay assina; o crossfade
            // nasce SOMENTE desta transição.
            Debug.Log($"[Music] PlayMusic({next}) (antes={previous}, cena={_lastSceneName}).");
            GameEventBus.Publish(new MusicStateChangedEvent(previous, next));
            manager.PlayMusic(next);
        }

        /// <summary>Acesso somente leitura ao resolver (diagnóstico/teste).</summary>
        public MusicState CurrentMusicState => _musicResolver.CurrentState;
    }
}
