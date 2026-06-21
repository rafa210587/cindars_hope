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
    /// OnDisable. Self-bootstrap via RuntimeInitializeOnLoadMethod (idiom do projeto).
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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null)
            {
                return;
            }

            var go = new GameObject("SfxEventBridge");
            DontDestroyOnLoad(go);
            go.AddComponent<SfxEventBridge>();
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
            // Música por CENA: a cena ativa define o ambiente-base (Fazenda/Cidade/Caverna);
            // combate/boss/festival sobrepõem. Atualiza ao trocar de cena.
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            ApplySceneAmbient(SceneManager.GetActiveScene().name);
        }

        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            UnsubscribeAll();
        }

        private void OnActiveSceneChanged(Scene previous, Scene next)
        {
            ApplySceneAmbient(next.name);
        }

        /// <summary>Mapeia o nome da cena para o ambiente musical e empurra a troca se mudou.</summary>
        private void ApplySceneAmbient(string sceneName)
        {
            var ambient = MusicState.Calmo;
            if (!string.IsNullOrEmpty(sceneName))
            {
                if (sceneName.Contains("Farm")) ambient = MusicState.Fazenda;
                else if (sceneName.Contains("Town")) ambient = MusicState.Cidade;
                else if (sceneName.Contains("Cave")) ambient = MusicState.Caverna;
            }

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
            var next = _musicResolver.CurrentState;
            if (next == _lastPublishedMusicState)
            {
                return;
            }

            var previous = _lastPublishedMusicState;
            _lastPublishedMusicState = next;

            // Único evento próprio (EMENDA V3). Nenhum gameplay assina; o crossfade
            // nasce SOMENTE desta transição.
            GameEventBus.Publish(new MusicStateChangedEvent(previous, next));
            AudioManager.Instance?.PlayMusic(next);
        }

        /// <summary>Acesso somente leitura ao resolver (diagnóstico/teste).</summary>
        public MusicState CurrentMusicState => _musicResolver.CurrentState;
    }
}
