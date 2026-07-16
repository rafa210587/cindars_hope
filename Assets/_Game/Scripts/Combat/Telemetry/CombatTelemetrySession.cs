using System;
using System.Collections.Generic;
using CindarsHope.Foundation;

namespace CindarsHope.Combat.Telemetry
{
    /// <summary>
    /// fable_59 â€” estado AGREGADO de telemetria de combate, PURO (sem Unity), por nÃ­vel da caverna.
    /// Toda a matemÃ¡tica (TTK, % de dano recebido, somatÃ³rios, contadores) vive aqui para ser testÃ¡vel
    /// em EditMode com eventos sintÃ©ticos. O <see cref="CombatTelemetryService"/> sÃ³ repassa os dados
    /// dos eventos para os mÃ©todos Record* desta classe e injeta o relÃ³gio (timestamps em segundos),
    /// exatamente como CombatWindowRules evita depender de Time.time em teste.
    ///
    /// TTK por enemyId: o cronÃ´metro abre no PRIMEIRO dano sofrido pela criatura
    /// (<see cref="RecordEnemyDamaged"/>) e fecha na morte (<see cref="RecordEnemyKilled"/>):
    /// TTK = tMorte - tPrimeiroDano. Mortes sem dano registrado sÃ£o DESCARTADAS (gap logado),
    /// nunca contam TTK=0.
    /// </summary>
    public sealed class CombatTelemetrySession
    {
        private readonly ICreatureClassLookup _classLookup;

        // enemyId â†’ instante (s) do primeiro dano sofrido (cronÃ´metro aberto). Reaberto a cada kill.
        private readonly Dictionary<string, float> _firstDamageTimeByEnemy = new Dictionary<string, float>();
        private readonly Dictionary<string, KillAggregate> _killsByEnemy = new Dictionary<string, KillAggregate>();
        private readonly Dictionary<string, DamageSourceAggregate> _damageBySource = new Dictionary<string, DamageSourceAggregate>();
        private readonly Dictionary<string, DamageTypeAggregate> _damageDealtByType = new Dictionary<string, DamageTypeAggregate>();

        private float _startTimeSeconds;
        private float _lastTimeSeconds;
        private bool _hasStart;

        // Ãºltimo HP mÃ¡ximo conhecido (para % de dano recebido); 0 = desconhecido â†’ % nÃ£o computÃ¡vel.
        private int _lastKnownMaxHp;

        private int _damageDealtTotal;
        private int _damageTakenTotal;
        private double _damageTakenPctSum; // soma dos % por hit, para a mÃ©dia do trecho
        private int _damageTakenHitCount;

        private int _staminaSpent;
        private int _mpSpent;
        private int _dodges;
        private int _blocks;
        private int _perfectBlocks;
        private int _postureBreaks;
        private int _chargedAttacks;
        private int _deaths;

        // Ãºltimo valor lido (para derivar deltas negativos = gasto) â€” sentinela -1 = nÃ£o lido ainda.
        private int _lastStamina = -1;
        private int _lastMana = -1;

        public int CaveLevel { get; private set; } = -1;
        public int Band { get; private set; }
        public string RunId { get; private set; } = string.Empty;

        public CombatTelemetrySession(ICreatureClassLookup classLookup)
        {
            _classLookup = classLookup;
        }

        /// <summary>Marca o inÃ­cio do trecho (entrada no nÃ­vel). Limpa deltas de stamina/mana.</summary>
        public void BeginLevel(int caveLevel, int band, string runId, float nowSeconds)
        {
            CaveLevel = caveLevel;
            Band = band;
            RunId = runId ?? string.Empty;
            _startTimeSeconds = nowSeconds;
            _lastTimeSeconds = nowSeconds;
            _hasStart = true;
            // baseline de stamina/mana sÃ³ conta a partir do prÃ³ximo evento (evita delta gigante inicial).
            _lastStamina = -1;
            _lastMana = -1;
        }

        public float DurationSeconds => _hasStart ? Math.Max(0f, _lastTimeSeconds - _startTimeSeconds) : 0f;
        public int DamageDealtTotal => _damageDealtTotal;
        public int DamageTakenTotal => _damageTakenTotal;
        public int StaminaSpent => _staminaSpent;
        public int MpSpent => _mpSpent;
        public int Dodges => _dodges;
        public int Blocks => _blocks;
        public int PerfectBlocks => _perfectBlocks;
        public int PostureBreaks => _postureBreaks;
        public int ChargedAttacks => _chargedAttacks;
        public int Deaths => _deaths;
        public int KillEntryCount => _killsByEnemy.Count;

        /// <summary>MÃ©dia do % de HP perdido por hit no trecho (0 se nenhum hit com MaxHP conhecido).</summary>
        public float DamageTakenPctAvg =>
            _damageTakenHitCount > 0 ? (float)(_damageTakenPctSum / _damageTakenHitCount) : 0f;

        private void Touch(float nowSeconds)
        {
            if (!_hasStart)
            {
                _startTimeSeconds = nowSeconds;
                _hasStart = true;
            }

            if (nowSeconds > _lastTimeSeconds)
            {
                _lastTimeSeconds = nowSeconds;
            }
        }

        // â”€â”€ Coletores (chamados pelo service a partir dos eventos) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        /// <summary>Primeiro dano sofrido por uma criatura abre/atualiza o cronÃ´metro de TTK.</summary>
        public void RecordEnemyDamaged(string enemyId, int amount, string damageType, float nowSeconds)
        {
            Touch(nowSeconds);
            if (string.IsNullOrEmpty(enemyId))
            {
                return;
            }

            if (!_firstDamageTimeByEnemy.ContainsKey(enemyId))
            {
                _firstDamageTimeByEnemy[enemyId] = nowSeconds;
            }

            // EnemyDamagedEvent Ã© dano DADO ao inimigo â†’ conta como dano dado e por tipo.
            if (amount > 0)
            {
                _damageDealtTotal += amount;
                var type = string.IsNullOrEmpty(damageType) ? "unknown" : damageType;
                if (!_damageDealtByType.TryGetValue(type, out var agg))
                {
                    agg = new DamageTypeAggregate();
                    _damageDealtByType[type] = agg;
                }

                agg.DamageTotal += amount;
                agg.HitCount += 1;
            }
        }

        /// <summary>
        /// Dano DADO genÃ©rico (DamageAppliedEvent) â€” soma ao total apenas se NÃƒO houver enemyId jÃ¡
        /// contabilizado por RecordEnemyDamaged. Como DamageAppliedEvent e EnemyDamagedEvent podem
        /// representar o mesmo golpe, esta entrada Ã© usada sÃ³ para o total quando o chamador indicar
        /// que Ã© uma fonte distinta. Mantida separada para evitar dupla contagem por padrÃ£o.
        /// </summary>
        public void RecordDamageDealtRaw(int finalDamage, string damageType, float nowSeconds)
        {
            Touch(nowSeconds);
            if (finalDamage <= 0)
            {
                return;
            }

            _damageDealtTotal += finalDamage;
            var type = string.IsNullOrEmpty(damageType) ? "unknown" : damageType;
            if (!_damageDealtByType.TryGetValue(type, out var agg))
            {
                agg = new DamageTypeAggregate();
                _damageDealtByType[type] = agg;
            }

            agg.DamageTotal += finalDamage;
            agg.HitCount += 1;
        }

        /// <summary>Morte da criatura fecha o cronÃ´metro: TTK = morte - primeiro dano (se houver).</summary>
        public void RecordEnemyKilled(string enemyId, float nowSeconds)
        {
            Touch(nowSeconds);
            if (string.IsNullOrEmpty(enemyId))
            {
                return;
            }

            if (!_killsByEnemy.TryGetValue(enemyId, out var agg))
            {
                agg = new KillAggregate { EnemyId = enemyId };
                _killsByEnemy[enemyId] = agg;
            }

            if (_firstDamageTimeByEnemy.TryGetValue(enemyId, out var firstDamage))
            {
                var ttk = Math.Max(0f, nowSeconds - firstDamage);
                agg.AddTtk(ttk);
                _firstDamageTimeByEnemy.Remove(enemyId); // prÃ³xima instÃ¢ncia reabre o cronÃ´metro
            }
            else
            {
                agg.KillsWithoutDamage += 1; // gap: morte sem dano registrado, descartada do TTK
            }
        }

        /// <summary>Dano recebido pelo player: total, por fonte e % do HP mÃ¡x no momento do hit.</summary>
        public void RecordPlayerDamaged(int amount, string sourceId, float nowSeconds)
        {
            Touch(nowSeconds);
            if (amount <= 0)
            {
                return;
            }

            _damageTakenTotal += amount;

            float pct = 0f;
            bool pctKnown = _lastKnownMaxHp > 0;
            if (pctKnown)
            {
                pct = (float)amount / _lastKnownMaxHp * 100f;
                _damageTakenPctSum += pct;
                _damageTakenHitCount += 1;
            }

            var src = string.IsNullOrEmpty(sourceId) ? "unknown" : sourceId;
            if (!_damageBySource.TryGetValue(src, out var agg))
            {
                agg = new DamageSourceAggregate { SourceId = src };
                _damageBySource[src] = agg;
            }

            agg.DamageTotal += amount;
            agg.HitCount += 1;
            if (pctKnown)
            {
                agg.PctSum += pct;
                agg.PctHitCount += 1;
            }
        }

        /// <summary>Atualiza o HP mÃ¡x conhecido (de HPChangedEvent) para o cÃ¡lculo de % de dano.</summary>
        public void RecordMaxHp(int maxHp)
        {
            if (maxHp > 0)
            {
                _lastKnownMaxHp = maxHp;
            }
        }

        /// <summary>StaminaChangedEvent: delta negativo (current &lt; anterior) = stamina gasta.</summary>
        public void RecordStamina(int currentStamina, float nowSeconds)
        {
            Touch(nowSeconds);
            if (_lastStamina >= 0 && currentStamina < _lastStamina)
            {
                _staminaSpent += _lastStamina - currentStamina;
            }

            _lastStamina = currentStamina;
        }

        /// <summary>ManaChangedEvent: delta negativo = MP gasto.</summary>
        public void RecordMana(int currentMana, float nowSeconds)
        {
            Touch(nowSeconds);
            if (_lastMana >= 0 && currentMana < _lastMana)
            {
                _mpSpent += _lastMana - currentMana;
            }

            _lastMana = currentMana;
        }

        public void RecordDodge(float nowSeconds) { Touch(nowSeconds); _dodges += 1; }
        public void RecordBlock(float nowSeconds) { Touch(nowSeconds); _blocks += 1; }
        public void RecordPerfectBlock(float nowSeconds) { Touch(nowSeconds); _perfectBlocks += 1; }
        public void RecordPostureBreak(float nowSeconds) { Touch(nowSeconds); _postureBreaks += 1; }
        public void RecordChargedAttack(float nowSeconds) { Touch(nowSeconds); _chargedAttacks += 1; }
        public void RecordDeath(float nowSeconds) { Touch(nowSeconds); _deaths += 1; }

        // â”€â”€ Consultas para teste/relatÃ³rio â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        /// <summary>TTK mÃ©dio (s) de um enemyId; 0 se nÃ£o hÃ¡ kills com dano registrado.</summary>
        public float GetTtkAvg(string enemyId) =>
            _killsByEnemy.TryGetValue(enemyId, out var agg) ? agg.TtkAvg : 0f;

        public float GetTtkMin(string enemyId) =>
            _killsByEnemy.TryGetValue(enemyId, out var agg) ? agg.TtkMin : 0f;

        public float GetTtkMax(string enemyId) =>
            _killsByEnemy.TryGetValue(enemyId, out var agg) ? agg.TtkMax : 0f;

        public int GetTtkSampleCount(string enemyId) =>
            _killsByEnemy.TryGetValue(enemyId, out var agg) ? agg.TtkSampleCount : 0;

        /// <summary>ConstrÃ³i o DTO serializÃ¡vel com todas as avaliaÃ§Ãµes Â§6-Â§7 preenchidas.</summary>
        public CombatTelemetryReport BuildReport(string generatedAtUtc, IList<string> extraGaps)
        {
            var report = new CombatTelemetryReport
            {
                ShapeVersion = CombatTelemetryReport.CurrentShapeVersion,
                RunId = RunId,
                CaveLevel = CaveLevel,
                Band = Band,
                GeneratedAtUtc = generatedAtUtc ?? string.Empty,
                DurationSeconds = DurationSeconds,
                DamageDealtTotal = _damageDealtTotal,
                DamageTakenTotal = _damageTakenTotal,
                DamageTakenPctAvg = DamageTakenPctAvg,
                StaminaSpent = _staminaSpent,
                MpSpent = _mpSpent,
                Dodges = _dodges,
                Blocks = _blocks,
                PerfectBlocks = _perfectBlocks,
                PostureBreaks = _postureBreaks,
                ChargedAttacks = _chargedAttacks,
                Deaths = _deaths,
            };

            foreach (var pair in _killsByEnemy)
            {
                var agg = pair.Value;
                var creatureClass = _classLookup != null
                    ? _classLookup.GetClass(agg.EnemyId)
                    : TelemetryCreatureClass.Unknown;

                var evaluation = TelemetryTargetEvaluator.EvaluateTtk(creatureClass, agg.TtkAvg, agg.TtkSampleCount);

                report.Kills.Add(new CombatTelemetryKillEntry
                {
                    EnemyId = agg.EnemyId,
                    CreatureClass = TelemetryTargetEvaluator.ClassToToken(creatureClass),
                    KillCount = agg.TtkSampleCount + agg.KillsWithoutDamage,
                    TtkAvgSeconds = agg.TtkAvg,
                    TtkMinSeconds = agg.TtkMin,
                    TtkMaxSeconds = agg.TtkMax,
                    TtkEvaluation = TelemetryTargetEvaluator.ToToken(evaluation),
                });

                if (agg.KillsWithoutDamage > 0)
                {
                    report.Gaps.Add($"enemy {agg.EnemyId}: {agg.KillsWithoutDamage} kill(s) without recorded first-damage (TTK discarded)");
                }
            }

            foreach (var pair in _damageBySource)
            {
                var agg = pair.Value;
                var creatureClass = _classLookup != null
                    ? _classLookup.GetClass(agg.SourceId)
                    : TelemetryCreatureClass.Unknown;
                var pctAvg = agg.PctHitCount > 0 ? (float)(agg.PctSum / agg.PctHitCount) : 0f;
                var evaluation = TelemetryTargetEvaluator.EvaluateDamageTakenPct(creatureClass, pctAvg, agg.PctHitCount);

                report.DamageTakenBySource.Add(new CombatTelemetryDamageBySourceEntry
                {
                    SourceId = agg.SourceId,
                    CreatureClass = TelemetryTargetEvaluator.ClassToToken(creatureClass),
                    HitCount = agg.HitCount,
                    DamageTotal = agg.DamageTotal,
                    DamagePctAvg = pctAvg,
                    DamagePctEvaluation = TelemetryTargetEvaluator.ToToken(evaluation),
                });
            }

            foreach (var pair in _damageDealtByType)
            {
                report.DamageDealtByType.Add(new CombatTelemetryDamageByTypeEntry
                {
                    DamageType = pair.Key,
                    DamageTotal = pair.Value.DamageTotal,
                    HitCount = pair.Value.HitCount,
                });
            }

            if (_damageTakenHitCount == 0 && _damageTakenTotal > 0)
            {
                report.Gaps.Add("damage taken recorded but MaxHP unknown at hit time â†’ % targets not evaluated");
            }

            if (extraGaps != null)
            {
                foreach (var gap in extraGaps)
                {
                    if (!string.IsNullOrEmpty(gap))
                    {
                        report.Gaps.Add(gap);
                    }
                }
            }

            return report;
        }

        private sealed class KillAggregate
        {
            public string EnemyId;
            public int TtkSampleCount;
            public int KillsWithoutDamage;
            private float _ttkSum;
            public float TtkMin { get; private set; } = float.MaxValue;
            public float TtkMax { get; private set; }

            public float TtkAvg => TtkSampleCount > 0 ? _ttkSum / TtkSampleCount : 0f;

            public void AddTtk(float ttk)
            {
                _ttkSum += ttk;
                TtkSampleCount += 1;
                if (ttk < TtkMin) TtkMin = ttk;
                if (ttk > TtkMax) TtkMax = ttk;
            }
        }

        private sealed class DamageSourceAggregate
        {
            public string SourceId;
            public int HitCount;
            public int DamageTotal;
            public double PctSum;
            public int PctHitCount;
        }

        private sealed class DamageTypeAggregate
        {
            public int DamageTotal;
            public int HitCount;
        }
    }
}
