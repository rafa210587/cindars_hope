using System;
using CindarsHope.Combat.Bestiary;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Combat.Telemetry
{
    /// <summary>
    /// fable_59 — serviço de telemetria de combate. Host bootstrap, OFF por default, ligado por um
    /// TOGGLE DEBUG estático (<see cref="DebugEnabled"/>, seguindo o estilo de toggle do projeto).
    /// Coleta 100% PASSIVA via GameEventBus — não toca NENHUM sistema de combate. Toda a matemática
    /// é delegada a <see cref="CombatTelemetrySession"/> (pura/testável). Flush por nível em
    /// CaveLevelEnteredEvent (fecha o nível anterior) e CaveExitedEvent; grava JSON local fora do save.
    ///
    /// Padrão de bootstrap idêntico ao CombatStateTracker (fable_69): instância única auto-registrada
    /// via RuntimeInitializeOnLoadMethod, sem global search. Assinaturas só existem quando o toggle
    /// está ON (CA-1): desligar remove TODAS as assinaturas e descarta o estado.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CombatTelemetryService : MonoBehaviour
    {
        private static CombatTelemetryService _activeInstance;

        // OFF por default (CA-1). Toggle debug estático: nada coleta até alguém ligar.
        private static bool _debugEnabled;

        public static CombatTelemetryService ActiveInstance => _activeInstance;

        /// <summary>Estado do toggle debug. OFF por default; ligar inicia coleta, desligar para e descarta.</summary>
        public static bool DebugEnabled
        {
            get => _debugEnabled;
            set
            {
                if (_debugEnabled == value)
                {
                    return;
                }

                _debugEnabled = value;
                if (_activeInstance != null)
                {
                    _activeInstance.ApplyToggle();
                }
            }
        }

        private ICreatureClassLookup _classLookup;
        private CombatTelemetrySession _session;
        private bool _subscribed;
        private string _telemetryDirectory;

        // Estado para a linha do DebugHud.
        public bool IsCollecting => _debugEnabled && _subscribed;
        public int CurrentLevel => _session?.CaveLevel ?? -1;
        public int CurrentKillEntries => _session?.KillEntryCount ?? 0;
        public string LastReportPath { get; private set; } = string.Empty;

        private void Awake()
        {
            if (_activeInstance != null && _activeInstance != this)
            {
                Destroy(this);
                return;
            }

            _activeInstance = this;
            _classLookup = new BestiaryTelemetryClassLookup();
            _telemetryDirectory = CombatTelemetryWriter.DefaultDirectory();
        }

        private void OnEnable()
        {
            if (_activeInstance != this)
            {
                return;
            }

            ApplyToggle();
        }

        private void OnDisable()
        {
            if (_activeInstance != this)
            {
                return;
            }

            Unsubscribe();
        }

        private void OnDestroy()
        {
            if (_activeInstance == this)
            {
                Unsubscribe();
                _activeInstance = null;
            }
        }

        /// <summary>Liga/desliga as assinaturas conforme o toggle (CA-1: OFF = zero assinaturas).</summary>
        private void ApplyToggle()
        {
            if (_debugEnabled)
            {
                Subscribe();
            }
            else
            {
                // Desligar descarta a sessão em andamento (telemetria é descartável).
                Unsubscribe();
                _session = null;
            }
        }

        private float Now => Time.time;

        private void Subscribe()
        {
            if (_subscribed)
            {
                return;
            }

            if (_session == null)
            {
                _session = new CombatTelemetrySession(_classLookup);
            }

            GameEventBus.Subscribe<CaveLevelEnteredEvent>(OnCaveLevelEntered);
            GameEventBus.Subscribe<CaveExitedEvent>(OnCaveExited);
            GameEventBus.Subscribe<EnemyDamagedEvent>(OnEnemyDamaged);
            GameEventBus.Subscribe<DamageAppliedEvent>(OnDamageApplied);
            GameEventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
            GameEventBus.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            GameEventBus.Subscribe<HPChangedEvent>(OnHpChanged);
            GameEventBus.Subscribe<StaminaChangedEvent>(OnStaminaChanged);
            GameEventBus.Subscribe<ManaChangedEvent>(OnManaChanged);
            GameEventBus.Subscribe<PlayerDodgeStartedEvent>(OnDodge);
            GameEventBus.Subscribe<PlayerPerfectBlockEvent>(OnPerfectBlock);
            GameEventBus.Subscribe<EnemyPostureBrokenEvent>(OnPostureBroken);
            GameEventBus.Subscribe<PlayerChargedAttackEvent>(OnChargedAttack);
            GameEventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);

            _subscribed = true;
            Debug.Log("[CombatTelemetryService] Telemetria de combate LIGADA (coleta passiva via eventos).");
        }

        private void Unsubscribe()
        {
            if (!_subscribed)
            {
                return;
            }

            GameEventBus.Unsubscribe<CaveLevelEnteredEvent>(OnCaveLevelEntered);
            GameEventBus.Unsubscribe<CaveExitedEvent>(OnCaveExited);
            GameEventBus.Unsubscribe<EnemyDamagedEvent>(OnEnemyDamaged);
            GameEventBus.Unsubscribe<DamageAppliedEvent>(OnDamageApplied);
            GameEventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
            GameEventBus.Unsubscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            GameEventBus.Unsubscribe<HPChangedEvent>(OnHpChanged);
            GameEventBus.Unsubscribe<StaminaChangedEvent>(OnStaminaChanged);
            GameEventBus.Unsubscribe<ManaChangedEvent>(OnManaChanged);
            GameEventBus.Unsubscribe<PlayerDodgeStartedEvent>(OnDodge);
            GameEventBus.Unsubscribe<PlayerPerfectBlockEvent>(OnPerfectBlock);
            GameEventBus.Unsubscribe<EnemyPostureBrokenEvent>(OnPostureBroken);
            GameEventBus.Unsubscribe<PlayerChargedAttackEvent>(OnChargedAttack);
            GameEventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);

            _subscribed = false;
            Debug.Log("[CombatTelemetryService] Telemetria de combate DESLIGADA (assinaturas removidas).");
        }

        // ── Handlers (delegam para a sessão pura) ────────────────────────────────────────────────

        private void OnCaveLevelEntered(CaveLevelEnteredEvent evt)
        {
            // Fecha o nível anterior (se havia um trecho aberto com dados) e abre o novo.
            FlushIfMeaningful();
            var band = ResolveBandForLevel(evt.CaveLevel);
            _session = new CombatTelemetrySession(_classLookup);
            _session.BeginLevel(evt.CaveLevel, band, evt.CaveRunSeed, Now);
        }

        private void OnCaveExited(CaveExitedEvent evt)
        {
            // Sair da caverna fecha o nível corrente.
            FlushIfMeaningful();
            _session = new CombatTelemetrySession(_classLookup);
        }

        private void OnEnemyDamaged(EnemyDamagedEvent evt) =>
            _session?.RecordEnemyDamaged(evt.EnemyId, evt.DamageAmount, evt.DamageType, Now);

        private void OnDamageApplied(DamageAppliedEvent evt)
        {
            // DamageAppliedEvent traz o DamageResult completo; usamos para iniciar o cronômetro de TTK
            // quando o alvo é um inimigo (TargetId preenchido) sem dupla contagem de dano dado: o
            // somatório de dano dado já vem de EnemyDamagedEvent. Aqui só registramos o primeiro dano
            // por TargetId, caso EnemyDamagedEvent não seja publicado para aquele golpe.
            var result = evt.DamageResult;
            if (result == null || string.IsNullOrEmpty(result.TargetId))
            {
                return;
            }

            // Marca primeiro-dano sem somar dano (evita dupla contagem); o somatório fica em EnemyDamaged.
            _session?.RecordEnemyDamaged(result.TargetId, 0, result.DamageType.ToString(), Now);
        }

        private void OnEnemyKilled(EnemyKilledEvent evt) =>
            _session?.RecordEnemyKilled(evt.EnemyId, Now);

        private void OnPlayerDamaged(PlayerDamagedEvent evt) =>
            _session?.RecordPlayerDamaged(evt.DamageAmount, evt.SourceId, Now);

        private void OnHpChanged(HPChangedEvent evt) =>
            _session?.RecordMaxHp(evt.MaxHP);

        private void OnStaminaChanged(StaminaChangedEvent evt) =>
            _session?.RecordStamina(evt.CurrentStamina, Now);

        private void OnManaChanged(ManaChangedEvent evt) =>
            _session?.RecordMana(evt.CurrentMana, Now);

        private void OnDodge(PlayerDodgeStartedEvent evt) =>
            _session?.RecordDodge(Now);

        private void OnPerfectBlock(PlayerPerfectBlockEvent evt) =>
            _session?.RecordPerfectBlock(Now);

        private void OnPostureBroken(EnemyPostureBrokenEvent evt) =>
            _session?.RecordPostureBreak(Now);

        private void OnChargedAttack(PlayerChargedAttackEvent evt) =>
            _session?.RecordChargedAttack(Now);

        private void OnPlayerDied(PlayerDiedEvent evt) =>
            _session?.RecordDeath(Now);

        // ── Flush ────────────────────────────────────────────────────────────────────────────────

        private void FlushIfMeaningful()
        {
            if (_session == null)
            {
                return;
            }

            // Só grava se houve alguma atividade de combate no trecho (evita lixo vazio por transição).
            var hasActivity = _session.KillEntryCount > 0
                              || _session.DamageDealtTotal > 0
                              || _session.DamageTakenTotal > 0;
            if (!hasActivity)
            {
                return;
            }

            var report = _session.BuildReport(DateTime.UtcNow.ToString("o"), BuildNominalGaps());
            LastReportPath = CombatTelemetryWriter.Write(report, _telemetryDirectory);
        }

        /// <summary>
        /// Gaps §53 nominais — métricas que a direction pede mas que NÃO têm evento disponível hoje
        /// (esta spec não altera eventos de gameplay). Documentadas no relatório para follow-up.
        /// </summary>
        private static string[] BuildNominalGaps()
        {
            return new[]
            {
                "no DamageBlockedEvent in project: regular (non-perfect) block count unavailable",
                "no block-held-time event: time spent blocking unavailable",
                "no event for dash distance / average movement speed (COMBAT_CORE §53)",
                "no event for consumables used in combat (COMBAT_CORE §53)",
            };
        }

        private static int ResolveBandForLevel(int caveLevel)
        {
            foreach (var band in CanonicalBestiaryCatalog.Bands)
            {
                if (caveLevel >= band.MinLevel && caveLevel <= band.MaxLevel)
                {
                    return band.Band;
                }
            }

            return 0;
        }

        /// <summary>Garante a instância na cena (bootstrap do projeto, sem global search recorrente).</summary>
        public static class Bootstrap
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
            private static void EnsureInstance()
            {
                if (_activeInstance != null)
                {
                    return;
                }

                var go = new GameObject("CombatTelemetryService");
                UnityEngine.Object.DontDestroyOnLoad(go);
                go.AddComponent<CombatTelemetryService>();
                Debug.Log("[CombatTelemetryService.Bootstrap] Servico instanciado via bootstrap (OFF por default, fable_59).");
            }
        }
    }
}
