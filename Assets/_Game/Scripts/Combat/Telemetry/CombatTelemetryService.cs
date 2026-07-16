using System;
using CindarsHope.Combat.Bestiary;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;
using CindarsHope.Foundation;

namespace CindarsHope.Combat.Telemetry
{
    /// <summary>
    /// fable_59 â€” serviÃ§o de telemetria de combate. Host bootstrap, OFF por default, ligado por um
    /// TOGGLE DEBUG estÃ¡tico (<see cref="DebugEnabled"/>, seguindo o estilo de toggle do projeto).
    /// Coleta 100% PASSIVA via GameEventBus â€” nÃ£o toca NENHUM sistema de combate. Toda a matemÃ¡tica
    /// Ã© delegada a <see cref="CombatTelemetrySession"/> (pura/testÃ¡vel). Flush por nÃ­vel em
    /// CaveLevelEnteredEvent (fecha o nÃ­vel anterior) e CaveExitedEvent; grava JSON local fora do save.
    ///
    /// PadrÃ£o de bootstrap idÃªntico ao CombatStateTracker (fable_69): instÃ¢ncia Ãºnica auto-registrada
    /// via RuntimeInitializeOnLoadMethod, sem global search. Assinaturas sÃ³ existem quando o toggle
    /// estÃ¡ ON (CA-1): desligar remove TODAS as assinaturas e descarta o estado.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CombatTelemetryService : MonoBehaviour
    {
        private static CombatTelemetryService _activeInstance;

        // OFF por default (CA-1). Toggle debug estÃ¡tico: nada coleta atÃ© alguÃ©m ligar.
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
                // Desligar descarta a sessÃ£o em andamento (telemetria Ã© descartÃ¡vel).
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
            GameEventBus.Subscribe<PlayerNormalBlockEvent>(OnBlock);
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
            GameEventBus.Unsubscribe<PlayerNormalBlockEvent>(OnBlock);
            GameEventBus.Unsubscribe<PlayerPerfectBlockEvent>(OnPerfectBlock);
            GameEventBus.Unsubscribe<EnemyPostureBrokenEvent>(OnPostureBroken);
            GameEventBus.Unsubscribe<PlayerChargedAttackEvent>(OnChargedAttack);
            GameEventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);

            _subscribed = false;
            Debug.Log("[CombatTelemetryService] Telemetria de combate DESLIGADA (assinaturas removidas).");
        }

        // â”€â”€ Handlers (delegam para a sessÃ£o pura) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        private void OnCaveLevelEntered(CaveLevelEnteredEvent evt)
        {
            // Fecha o nÃ­vel anterior (se havia um trecho aberto com dados) e abre o novo.
            FlushIfMeaningful();
            var band = ResolveBandForLevel(evt.CaveLevel);
            _session = new CombatTelemetrySession(_classLookup);
            _session.BeginLevel(evt.CaveLevel, band, evt.CaveRunSeed, Now);
        }

        private void OnCaveExited(CaveExitedEvent evt)
        {
            // Sair da caverna fecha o nÃ­vel corrente.
            FlushIfMeaningful();
            _session = new CombatTelemetrySession(_classLookup);
        }

        private void OnEnemyDamaged(EnemyDamagedEvent evt) =>
            _session?.RecordEnemyDamaged(evt.EnemyId, evt.DamageAmount, evt.DamageType, Now);

        private void OnDamageApplied(DamageAppliedEvent evt)
        {
            // DamageAppliedEvent traz o DamageResult completo; usamos para iniciar o cronÃ´metro de TTK
            // quando o alvo Ã© um inimigo (TargetId preenchido) sem dupla contagem de dano dado: o
            // somatÃ³rio de dano dado jÃ¡ vem de EnemyDamagedEvent. Aqui sÃ³ registramos o primeiro dano
            // por TargetId, caso EnemyDamagedEvent nÃ£o seja publicado para aquele golpe.
            var result = evt.DamageResult;
            if (result == null || string.IsNullOrEmpty(result.TargetId))
            {
                return;
            }

            // Marca primeiro-dano sem somar dano (evita dupla contagem); o somatÃ³rio fica em EnemyDamaged.
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

        private void OnBlock(PlayerNormalBlockEvent evt) =>
            _session?.RecordBlock(Now);

        private void OnPerfectBlock(PlayerPerfectBlockEvent evt) =>
            _session?.RecordPerfectBlock(Now);

        private void OnPostureBroken(EnemyPostureBrokenEvent evt) =>
            _session?.RecordPostureBreak(Now);

        private void OnChargedAttack(PlayerChargedAttackEvent evt) =>
            _session?.RecordChargedAttack(Now);

        private void OnPlayerDied(PlayerDiedEvent evt) =>
            _session?.RecordDeath(Now);

        // â”€â”€ Flush â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        private void FlushIfMeaningful()
        {
            if (_session == null)
            {
                return;
            }

            // SÃ³ grava se houve alguma atividade de combate no trecho (evita lixo vazio por transiÃ§Ã£o).
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
        /// Gaps Â§53 nominais â€” mÃ©tricas que a direction pede mas que NÃƒO tÃªm evento disponÃ­vel hoje
        /// (esta spec nÃ£o altera eventos de gameplay). Documentadas no relatÃ³rio para follow-up.
        /// </summary>
        private static string[] BuildNominalGaps()
        {
            return new[]
            {
                "no block-held-time event: time spent blocking unavailable",
                "no event for dash distance / average movement speed (COMBAT_CORE Â§53)",
                "no event for consumables used in combat (COMBAT_CORE Â§53)",
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

        /// <summary>Garante a instÃ¢ncia na cena (instalada pelo composition root, sem global search recorrente).</summary>
        public static class Bootstrap
        {
            public static void Install(Transform owner)
            {
                if (_activeInstance != null)
                {
                    return;
                }

                var go = new GameObject("CombatTelemetryService");
                go.transform.SetParent(owner);
                if (owner == null) UnityEngine.Object.DontDestroyOnLoad(go);
                go.AddComponent<CombatTelemetryService>();
                Debug.Log("[CombatTelemetryService.Bootstrap] Servico instanciado via bootstrap (OFF por default, fable_59).");
            }
        }
    }
}
