using CindarsHope.Cave.Data;
using CindarsHope.Cave.Ecosystem;
using CindarsHope.Cave.Runtime;
using CindarsHope.Combat.StatusEffect;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.DebugTools;
using CindarsHope.Enemy;
using CindarsHope.Player.Progression;
using UnityEngine;
using CindarsHope.Foundation;

namespace CindarsHope.Combat
{
    [DisallowMultipleComponent]
    public class EnemyHealth : MonoBehaviour
    {
        // spec_enemy_attack_kits_v1 (AllyHeal/AllyBuff): registro estatico de instancias ativas,
        // mesmo padrao de CraftingRuntime.ActiveInstances (FIX-001) â€” evita FindObjectsOfType.
        // Populado via OnEnable/OnDisable. EnemyActionRunner le esta lista para enumerar aliados
        // vivos candidatos a cura/buff (posicao + HP), sem scene search.
        public static readonly System.Collections.Generic.List<EnemyHealth> ActiveInstances = new System.Collections.Generic.List<EnemyHealth>();

        [SerializeField] private EnemyDataSO _enemyData;

        private int _currentHp;
        // Quando > 0, sobrescreve _enemyData.maxHp (HP escalado via ConfigureWithScaling).
        // Sentinel 0 = usar _enemyData.maxHp (caminho de Configure legado sem scaling).
        private int _scaledMaxHp;
        private bool _hpRestoredFromSnapshot;
        private CindarsHope.Combat.StatusEffect.StatusEffectManager _statusEffects = new CindarsHope.Combat.StatusEffect.StatusEffectManager();

        // fable_06: contexto de loot determinÃ­stico (ADR-0005). Setado pelo materializer da caverna
        // APÃ“S Configure. Vazio => caminho legado (dropItemId fixo) no EnemyDropSpawner.
        private string _enemyInstanceId = string.Empty;
        private string _caveRunSeed = string.Empty;

        // fable_06: perfil de vulnerabilidade (matriz Element/Material/Status). Null => neutro.
        private EnemyVulnerabilityProfileSO _vulnerabilityProfile;

        // fable_78 (SLICE 4): estado runtime "Ferido" do conflito inter-monstro. Timestamp atÃ© quando o
        // alvo apanhou de um rival; enquanto ativo a defesa efetiva Ã© reduzida (gancho tÃ¡tico). Estado
        // transitÃ³rio/aditivo (mesmo idioma do _stunUntil do EnemyBrain e da janela de vulnerabilidade);
        // nÃ£o persiste no save (comportamento por visita, fora do LayoutHash).
        private float _woundedUntil;
        private float _woundedDefenseMultiplier = 1f;
        // fable_78 (SLICE 4): setado por TakeDamageFromEnemy logo antes de aplicar o golpe; consumido por
        // Die() para escolher a rota de corpo reduzido (EnemyKilledByEnemyEvent) em vez da rota de loot
        // do jogador (EnemyKilledEvent). Null = kill normal (pelo jogador) â€” comportamento inalterado.
        private string _pendingEnemyKillerInstanceId;
        private float _pendingKillLootMultiplier = 1f;
        private int _pendingKillCaveLevel;

        // spec_enemy_attack_kits_v1 (Rise-once, primitiva P2): DamageType do ultimo golpe recebido
        // (setado em TakeDamage antes de checar morte) + flag idempotente de consumo. Estado runtime
        // transitorio (mesmo idioma do _woundedUntil acima) â€” nao persiste no save.
        private DamageType _lastDamageType = DamageType.Physical;
        private bool _riseOnceConsumed;
        private bool _isCollapsedPendingRise;

        public int CurrentHp => _currentHp;
        /// <summary>HP mÃ¡ximo desta instÃ¢ncia. Usa valor escalado quando disponÃ­vel (ConfigureWithScaling),
        /// senÃ£o retorna o valor do asset (Configure legado).</summary>
        public int MaxHp => _scaledMaxHp > 0 ? _scaledMaxHp : (_enemyData != null ? _enemyData.maxHp : 0);
        public string EnemyId => _enemyData != null ? _enemyData.enemyId : string.Empty;
        // fable_78: id de instÃ¢ncia estÃ¡vel (setado por ConfigureLootContext). Usado como killer/victim
        // id nos eventos de conflito inter-monstro. Vazio fora de uma run de caverna.
        public string EnemyInstanceId => _enemyInstanceId;
        // fable_78 (SLICE 4): true enquanto o alvo estÃ¡ "Ferido" (apanhou de um rival recentemente).
        public bool IsWounded => Time.time < _woundedUntil;
        public string DisplayName => _enemyData != null && !string.IsNullOrWhiteSpace(_enemyData.DisplayName) ? _enemyData.DisplayName : name;
        public CindarsHope.Combat.StatusEffect.StatusEffectManager StatusEffects => _statusEffects;
        // SPEC 14A-FIX10: expose IsDead so EnemyBrain/external controllers can check death state.
        // Previously a duplicate CindarsHope.Enemy.EnemyHealth in /Enemy/ provided this; removed
        // in FIX10 because it was a parallel/legacy class that never got Configure'd at runtime.
        public bool IsDead => _enemyData != null && _currentHp <= 0;

        public void Configure(EnemyDataSO enemyData)
        {
            _scaledMaxHp = 0; // reset sentinel â€” MaxHp volta a usar asset value
            _enemyData = enemyData;
            if (_enemyData != null)
            {
                _currentHp = _enemyData.maxHp;
                CombatLog.Log($"CombatLog: Enemy configured. {BuildEnemyLogPrefix()}, HP={_currentHp}/{MaxHp}, Level={_enemyData.enemyLevel}, Difficulty={_enemyData.baseDifficulty}.", this);
            }

            if (GetComponent<CindarsHope.Combat.StatusEffect.EnemyStatusRuntimeTicker>() == null)
                gameObject.AddComponent<CindarsHope.Combat.StatusEffect.EnemyStatusRuntimeTicker>();
        }

        /// <summary>
        /// Configura HP com scaling por nÃ­vel de caverna e multiplicador global de balance.
        /// Aplica CaveBandScaling.ScaleHp para crescimento intra-banda (+12%/nÃ­vel),
        /// depois aplica hpBaseMultiplier (do CaveEcosystemBalanceSO) para corrigir desproporÃ§Ã£o
        /// vs. dano do player. Chame este overload em vez de Configure(enemyData) em spawners
        /// de caverna que conhecem o nÃ­vel real. caveLevel=0 ou hpBaseMultiplier=1 degenera ao
        /// comportamento original.
        /// </summary>
        public void ConfigureWithScaling(EnemyDataSO enemyData, int caveLevel, float hpBaseMultiplier = 1f)
        {
            _enemyData = enemyData;
            if (_enemyData != null)
            {
                var bandMinLevel = CaveBandScaling.BandMinLevel(CaveBandScaling.BandForLevel(caveLevel > 0 ? caveLevel : _enemyData.enemyLevel));
                var baseHp = _enemyData.maxHp;
                var scaledHp = caveLevel > 0
                    ? CaveBandScaling.ScaleHp(baseHp, caveLevel, bandMinLevel)
                    : baseHp;
                var multipliedHp = Mathf.Max(1, Mathf.RoundToInt(scaledHp * Mathf.Max(1f, hpBaseMultiplier)));
                _currentHp = multipliedHp;
                // Override MaxHp for this instance via a private backing field to reflect scaled value.
                _scaledMaxHp = multipliedHp;
                CombatLog.Log($"CombatLog: Enemy configured (scaled). {BuildEnemyLogPrefix()}, BaseHP={baseHp}, ScaledHP={scaledHp}, FinalHP={_currentHp}/{multipliedHp}, CaveLevel={caveLevel}, HpMult={hpBaseMultiplier:F2}, BandMin={bandMinLevel}.", this);
            }

            if (GetComponent<CindarsHope.Combat.StatusEffect.EnemyStatusRuntimeTicker>() == null)
                gameObject.AddComponent<CindarsHope.Combat.StatusEffect.EnemyStatusRuntimeTicker>();
        }

        // fable_06: liga o contexto de loot estÃ¡vel (instance id + run seed da caverna). Aditivo;
        // chamado pelo CaveRuntimeMaterializer apÃ³s Configure. Sem isto, o drop usa o caminho legado.
        public void ConfigureLootContext(string enemyInstanceId, string caveRunSeed)
        {
            _enemyInstanceId = enemyInstanceId ?? string.Empty;
            _caveRunSeed = caveRunSeed ?? string.Empty;
        }

        // fable_06: liga a matriz de vulnerabilidade (Element/Material/Status). Aditivo; null = neutro.
        public void ConfigureVulnerabilityMatrix(EnemyVulnerabilityProfileSO profile)
        {
            _vulnerabilityProfile = profile;
        }

        // spec_enemy_attack_kits_v1 (Rise-once, primitiva P2): parametros lidos do EnemyActionSO
        // marcado RiseOnceEnabled no actionset ativo. Chamado por EnemyBrain apos resolver o
        // ActionSet (InitActionSet); null/disabled = comportamento de morte 100% inalterado.
        private bool _riseOnceEnabled;
        private float _riseOnceHpPercent;
        private string[] _riseOnceBlockedByDamageTypes = System.Array.Empty<string>();
        private float _riseOnceCollapseSeconds;

        public void ConfigureRiseOnce(bool enabled, float hpPercent, string[] blockedByDamageTypes, float collapseSeconds)
        {
            _riseOnceEnabled = enabled;
            _riseOnceHpPercent = hpPercent;
            _riseOnceBlockedByDamageTypes = blockedByDamageTypes ?? System.Array.Empty<string>();
            _riseOnceCollapseSeconds = collapseSeconds;
        }

        // spec_enemy_attack_kits_v1 (AllyHeal/AllyBuff): mantem ActiveInstances sem scene search.
        private void OnEnable()
        {
            if (!ActiveInstances.Contains(this))
                ActiveInstances.Add(this);
        }

        private void OnDisable()
        {
            ActiveInstances.Remove(this);
        }

        // F13: restaura HP salvo do snapshot da run (chamado APÃ“S Configure, antes do Start).
        // O guard impede o Start de resetar o valor restaurado para o mÃ¡ximo.
        public void RestoreHp(int savedHp)
        {
            if (_enemyData == null)
            {
                return;
            }

            _currentHp = Mathf.Clamp(savedHp, 0, MaxHp);
            _hpRestoredFromSnapshot = true;
        }

        private void Start()
        {
            if (_enemyData == null)
            {
                Debug.LogWarning($"EnemyHealth on '{name}' has no EnemyDataSO assigned.", this);
                return;
            }

            if (_hpRestoredFromSnapshot)
            {
                return;
            }

            // Preserva HP escalado se ConfigureWithScaling jÃ¡ setou _currentHp corretamente.
            if (_scaledMaxHp <= 0)
            {
                _currentHp = _enemyData.maxHp;
            }
            CombatLog.Log($"CombatLog: Enemy spawned. {BuildEnemyLogPrefix()}, HP={_currentHp}/{MaxHp}, Level={_enemyData.enemyLevel}, Difficulty={_enemyData.baseDifficulty}.", this);

            if (GetComponent<CindarsHope.Combat.StatusEffect.EnemyStatusRuntimeTicker>() == null)
                gameObject.AddComponent<CindarsHope.Combat.StatusEffect.EnemyStatusRuntimeTicker>();
        }

        public void ApplyStatusEffect(CindarsHope.Combat.StatusEffect.StatusEffectSO statusEffect)
        {
            if (statusEffect != null)
            {
                _statusEffects.ApplyStatusEffect(statusEffect);
                CombatLog.Log($"CombatLog: Applied status effect '{statusEffect.DisplayName}' to {DisplayName}.", this);
            }
        }

        public void TakeDamage(int amount)
        {
            var request = new DamageRequest(EnemyId, amount);
            request.SourcePosition = transform.position;
            request.KnockbackForce = 0f;
            TakeDamage(request);
        }

        // fable_78 (SLICE 4): caminho de dano com ORIGEM-INIMIGO (conflito inter-monstro, seÃ§Ã£o 14.6).
        // - aplica InterMonsterDamageMultiplier (default 0.10) ao dano base â€” dano monstroâ†”jogador NÃƒO
        //   passa por aqui, entÃ£o permanece inalterado;
        // - aplica o status leve "Ferido" ao alvo (defesa reduzida por uma janela curta);
        // - se for kill, marca a morte como "by enemy" â†’ corpo dropa lootÃ—InterMonsterKillLootMultiplier
        //   e publica EnemyKilledByEnemyEvent (NUNCA EnemyKilledEvent â†’ sem XP/quest/bestiÃ¡rio ao jogador).
        // killerInstanceId identifica o atacante para o evento; balance carrega todos os multiplicadores.
        public void TakeDamageFromEnemy(int rawDamage, DamageType damageType, string killerInstanceId, int caveLevel, CaveEcosystemBalanceSO balance)
        {
            if (_enemyData == null || balance == null || _currentHp <= 0 || rawDamage <= 0)
            {
                return;
            }

            // Aplica o status "Ferido" ANTES de calcular o dano: a defesa reduzida jÃ¡ vale para este golpe
            // e para os prÃ³ximos da janela, dando vantagem real a quem intervÃ©m no conflito.
            ApplyWounded(balance.WoundedDefenseMultiplier, balance.WoundedDurationSeconds);

            int scaledDamage = InterMonsterCombatMath.ScaleInterMonsterDamage(rawDamage, balance.InterMonsterDamageMultiplier);
            if (scaledDamage <= 0)
            {
                return;
            }

            var request = new DamageRequest(EnemyId, scaledDamage, damageType, killerInstanceId ?? "enemy")
            {
                SourcePosition = transform.position,
                KnockbackForce = 0f,
                CanTriggerVulnerability = false
            };

            // Roteia pelo caminho de dano padrÃ£o (defesa "Ferido" + mitigaÃ§Ã£o) marcando o killer-inimigo,
            // para que Die() escolha a rota de corpo reduzido em vez da rota normal de loot do jogador.
            _pendingEnemyKillerInstanceId = killerInstanceId ?? string.Empty;
            _pendingKillLootMultiplier = balance.InterMonsterKillLootMultiplier;
            _pendingKillCaveLevel = caveLevel;
            TakeDamage(request);
            _pendingEnemyKillerInstanceId = null;
        }

        // fable_78 (SLICE 4): aplica/renova o status leve "Ferido" (defesa reduzida por uma janela curta).
        // Reusa a janela transitÃ³ria runtime (mesmo idioma de _stunUntil/janela de vulnerabilidade) em vez
        // de um StatusEffectSO porque o asset/database de "Ferido" Ã© DEFERRED_UNITY (slice 6) â€” sem criar
        // sistema paralelo; a semÃ¢ntica de defesa reduzida vive no DamageCalculator existente.
        public void ApplyWounded(float defenseMultiplier, float durationSeconds)
        {
            if (durationSeconds <= 0f)
            {
                return;
            }

            _woundedDefenseMultiplier = Mathf.Clamp(defenseMultiplier, 0f, 1f);
            _woundedUntil = Mathf.Max(_woundedUntil, Time.time + durationSeconds);
        }

        public void TakeDamage(DamageRequest request)
        {
            if (_enemyData == null)
            {
                return;
            }

            if (request.Amount <= 0)
            {
                return;
            }

            if (_currentHp <= 0)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(request.TargetId))
            {
                request.TargetId = EnemyId;
            }

            var vulnerabilityState = GetComponent<EnemyVulnerabilityState>();
            float vulnerabilityMultiplier = vulnerabilityState != null && vulnerabilityState.IsVulnerable
                ? vulnerabilityState.Multiplier
                : 1f;

            // fable_06: multiplicador de elemento/material da famÃ­lia (matriz do perfil do inimigo).
            // 1.0 quando nÃ£o hÃ¡ perfil ou tags casadas (neutro). Ordem aplicada no DamageCalculator:
            // resistance â†’ vulnerability window â†’ element/material â†’ status.
            float elementMaterialMultiplier = VulnerabilityMatcher.GetDamageMultiplier(
                _vulnerabilityProfile, request.DamageType, request.WeaponMaterialTags);

            // fable_78 (SLICE 4): alvo "Ferido" tem defesa reduzida (gancho tÃ¡tico do conflito). Aplica-se
            // a TODO dano recebido enquanto a janela do status estÃ¡ ativa, inclusive do jogador que intervÃ©m.
            int effectiveDefense = IsWounded
                ? InterMonsterCombatMath.ApplyWoundedDefense(_enemyData.defense, _woundedDefenseMultiplier)
                : _enemyData.defense;

            var damageResult = DamageCalculator.Calculate(
                request, effectiveDefense, null, vulnerabilityMultiplier, 1f, elementMaterialMultiplier);
            if (damageResult.FinalDamage <= 0)
            {
                return;
            }

            var hpBefore = _currentHp;
            _currentHp -= damageResult.FinalDamage;
            _currentHp = Mathf.Max(0, _currentHp);
            // spec_enemy_attack_kits_v1: registra o DamageType deste golpe ANTES de checar morte â€”
            // Die() consulta este valor para decidir se o Rise-once e bloqueado (ex.: fire/radiant).
            _lastDamageType = damageResult.DamageType;
            CombatLog.Log($"CombatLog: Hit enemy. {BuildEnemyLogPrefix()}, Damage={damageResult.FinalDamage}, HP={hpBefore}->{_currentHp}/{MaxHp}.", this);

            GameEventBus.Publish(new DamageAppliedEvent(damageResult, transform.position));
            // SPEC 14A-FIX10: explicit show-at-target so popup lands above this enemy's collider
            // top, not at the OverlapPoint guess (which is unreliable for fast-moving enemies).
            FloatingDamageNumberDisplayer.ShowAtTarget(gameObject, damageResult.FinalDamage, damageResult.DamageType, damageResult.WasImmune, false);

            var hitFlash = GetComponentInChildren<HitFlashController>();
            if (hitFlash != null)
            {
                hitFlash.Flash();
            }

            if (request.KnockbackForce > 0f)
            {
                var knockback = GetComponent<KnockbackController>();
                if (knockback != null)
                {
                    Vector3 currentPosition = transform.position;
                    Vector3 direction = (currentPosition - request.SourcePosition).normalized;
                    float finalForce = request.KnockbackForce * _enemyData.receivedKnockbackMultiplier;
                    knockback.ApplyKnockback((Vector2)direction, finalForce);
                    CombatLog.Log($"CombatLog: Knockback enemy. {BuildEnemyLogPrefix()}, Force={finalForce}, HP={_currentHp}/{MaxHp}.", this);
                }
            }

            if (_currentHp <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            // spec_enemy_attack_kits_v1 (Rise-once, primitiva P2): intercepta a morte ANTES de
            // publicar qualquer evento. So se aplica ao caminho de kill NORMAL (pelo player) â€”
            // kill inter-monstro (_pendingEnemyKillerInstanceId setado) sempre segue o fluxo padrao,
            // preservando o loot reduzido do ecossistema (fable_78) intacto.
            if (string.IsNullOrEmpty(_pendingEnemyKillerInstanceId)
                && CindarsHope.Enemy.EnemyActionExecution.ShouldRiseOnce(_riseOnceEnabled, _riseOnceConsumed, _lastDamageType.ToString(), _riseOnceBlockedByDamageTypes))
            {
                _riseOnceConsumed = true;
                _isCollapsedPendingRise = true;
                CombatLog.Log($"CombatLog: EnemyRiseOnceCollapse. {BuildEnemyLogPrefix()}, CollapseSeconds={_riseOnceCollapseSeconds:F2}, LastDamageType={_lastDamageType}.", this);
                Invoke(nameof(ResolveRise), Mathf.Max(0f, _riseOnceCollapseSeconds));
                return;
            }

            var xpReward = PlayerProgressionRules.CalculateEnemyXpReward(
                _enemyData.enemyLevel,
                _enemyData.baseDifficulty,
                _enemyData.xpRewardOverride);

            CombatLog.Log($"CombatLog: Enemy defeated. {BuildEnemyLogPrefix()}, HP=0/{MaxHp}, Drop={_enemyData.dropItemId} x{_enemyData.dropAmount}, XP={xpReward}.", this);

            var bossReporter = GetComponent<CaveBossDeathReporter>();
            if (bossReporter != null)
            {
                bossReporter.ReportDefeatedFromOwner(transform.position);
            }

            // fable_06: seed de loot estÃ¡vel por run/instÃ¢ncia (ADR-0005). Vazio quando nÃ£o hÃ¡
            // contexto de caverna (ex.: inimigo de smoke test fora de run) => loot resolver usa o
            // seed 0 mas o spawner cai no caminho legado se nÃ£o houver lootTableId.
            int lootSeed = CindarsHope.Loot.EnemyLootResolver.BuildLootSeed(_caveRunSeed, _enemyInstanceId);
            bool isMinibossOrBoss = _enemyData.IsMiniBoss || _enemyData.IsBoss;

            // fable_78 (SLICE 4): kill monstro-vs-monstro NÃƒO dispara a rota normal de loot do jogador
            // (sem XP/quest/bestiÃ¡rio ao player). Em vez disso publica EnemyKilledByEnemyEvent com o
            // payload do corpo REDUZIDO (Ã— InterMonsterKillLootMultiplier), que o EnemyDropSpawner
            // existente concede como Ãºnico caminho de drop. Estado de morte persiste via F13 (HP=0).
            if (!string.IsNullOrEmpty(_pendingEnemyKillerInstanceId))
            {
                int reducedDrop = InterMonsterCombatMath.ScaleReducedLoot(_enemyData.dropAmount, _pendingKillLootMultiplier);

                GameEventBus.Publish(new EnemyKilledByEnemyEvent(
                    _enemyInstanceId,
                    _pendingEnemyKillerInstanceId,
                    _pendingKillCaveLevel,
                    _enemyData.enemyId,
                    _enemyData.dropItemId,
                    reducedDrop,
                    _enemyData.lootTableId,
                    lootSeed,
                    _enemyData.IsElite,
                    isMinibossOrBoss,
                    _pendingKillLootMultiplier));

                CombatLog.Log($"CombatLog: EnemyKilledByEnemy. Victim={_enemyInstanceId}, Killer={_pendingEnemyKillerInstanceId}, ReducedDrop={_enemyData.dropItemId} x{reducedDrop}.", this);

                gameObject.SetActive(false);
                return;
            }

            GameEventBus.Publish(new EnemyKilledEvent(
                _enemyData.enemyId,
                _enemyData.dropItemId,
                _enemyData.dropAmount,
                transform.position,
                xpReward,
                _enemyInstanceId,
                _enemyData.lootTableId,
                lootSeed,
                _enemyData.IsElite,
                isMinibossOrBoss));

            gameObject.SetActive(false);
        }

        // spec_enemy_attack_kits_v1 (Rise-once): chamado via Invoke() apos o colapso. Reergue com o
        // % HP configurado; se o GameObject foi desativado/destruido nesse meio tempo (ex.: cena
        // trocou), Invoke nao dispara em objeto destruido â€” no-op seguro.
        private void ResolveRise()
        {
            if (!_isCollapsedPendingRise) return;
            _isCollapsedPendingRise = false;

            _currentHp = CindarsHope.Enemy.EnemyActionExecution.ResolveRiseHp(MaxHp, _riseOnceHpPercent);
            CombatLog.Log($"CombatLog: EnemyRiseOnceResolved. {BuildEnemyLogPrefix()}, HP={_currentHp}/{MaxHp}.", this);
        }

        private string BuildEnemyLogPrefix()
        {
            var bossReporter = GetComponent<CaveBossDeathReporter>();
            if (bossReporter == null)
            {
                return $"Name={DisplayName}, EnemyId={EnemyId}, IsBoss=false";
            }

            return $"Name={DisplayName}, EnemyId={EnemyId}, IsBoss=true, BossGateId={bossReporter.BossGateId}, CaveLevel={bossReporter.CaveLevel}, CheckpointUnlock={bossReporter.CheckpointUnlockedOnDefeat}";
        }
    }
}
