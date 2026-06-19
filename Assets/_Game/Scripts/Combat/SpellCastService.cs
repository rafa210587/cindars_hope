using System.Collections.Generic;
using CindarsHope.Combat.Magic;
using CindarsHope.Combat.Weapon;
using CindarsHope.Core;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Equipment;
using CindarsHope.Inventory.Data;
using CindarsHope.Player;
using UnityEngine;

namespace CindarsHope.Combat
{
    public class SpellCastService
    {
        // F02: provider de stats derivados (setado pelo PlayerAttackController; null-safe).
        public PlayerCombatStatsProvider StatsProvider { get; set; }

        // fable_07: estado de conhecimento arcano (null-safe). Setado pelo wiring quando disponível.
        // O fluxo de cast por ITEM EQUIPADO NÃO depende disto (preservado 100% — ver CanCast).
        public CindarsHope.Magic.PlayerSpellbook Spellbook { get; set; }

        // fable_08: alvos de SelfRestore (null-safe; setados pelo PlayerAttackController).
        public PlayerManager PlayerManager { get; set; }
        public StaminaManager StaminaManager { get; set; }

        /// <summary>
        /// fable_08: fornecedor de posições de inimigos para auto-target (EMENDA 6.6-A) e shapes de
        /// área. Recebe (centro, raio) e devolve as posições candidatas. Setado pelo
        /// PlayerAttackController usando Physics2D.OverlapCircleAll — NUNCA FindObjectsByType.
        /// Em testes/headless fica null e o auto-target cai para tiro à frente.
        /// </summary>
        public System.Func<Vector2, float, IReadOnlyList<Vector2>> EnemyPositionQuery { get; set; }

        private readonly ManaManager _manaManager;
        private readonly EquipmentManager _equipmentManager;
        private readonly EquippedItemResolver _itemResolver;
        private readonly float _knockbackForce;
        private readonly StatusEffectDatabaseSO _statusEffectDatabase;

        public SpellCastService(
            ManaManager manaManager,
            EquipmentManager equipmentManager,
            EquippedItemResolver itemResolver,
            float knockbackForce,
            StatusEffectDatabaseSO statusEffectDatabase = null)
        {
            _manaManager = manaManager;
            _equipmentManager = equipmentManager;
            _itemResolver = itemResolver;
            _knockbackForce = knockbackForce;
            _statusEffectDatabase = statusEffectDatabase;
        }

        /// <summary>
        /// fable_07 — castabilidade por ESTADO DE CONHECIMENTO de uma spellId:
        /// conhecida permanentemente (spellbook) OU concedida por item equipado.
        /// Não checa mana/cooldown (isso é do TryCast). Quando não há spellbook, devolve true
        /// (compat: sem estado de conhecimento, a única gate é o item equipado, como hoje).
        /// </summary>
        public bool CanCast(string spellId)
        {
            if (string.IsNullOrEmpty(spellId))
            {
                return false;
            }

            // Sem spellbook wired: comportamento legado (a posse do item equipado é a única condição).
            if (Spellbook == null)
            {
                return true;
            }

            return Spellbook.CanCast(spellId);
        }

        /// <summary>
        /// fable_08 — descritor de um cast já validado (cooldown/mana/conhecimento). Carrega tudo o
        /// que o <see cref="ResolveCast"/> precisa, permitindo que o SpellCastRoutine resolva depois
        /// da janela de cast time (ou que o TryCast resolva imediatamente quando CastTimeSeconds==0).
        /// </summary>
        public sealed class SpellCastPlan
        {
            public EquipmentSlot Slot;
            public SpellDataSO Spell;
            public Vector2 Direction;
            public Vector2 SpawnPosition;
            public int FinalDamage;
            public int ManaSpent;
            public CindarsHope.Combat.StatusEffect.StatusEffectSO StatusEffect;
        }

        public AttackResult TryCast(
            EquipmentSlot slot,
            ItemDataSO itemData,
            float lastAttackTime,
            Vector2 direction,
            Vector2 spawnPosition)
        {
            var begin = TryBeginCast(slot, itemData, lastAttackTime, direction, spawnPosition, out var plan);
            if (!begin.Success || plan == null)
            {
                return begin;
            }

            // fable_08: caminho legado (cast instantâneo). Spells com CastTimeSeconds>0 são resolvidas
            // pelo SpellCastRoutine, que chama ResolveCast após a janela (ou RefundCast se cancelar).
            ResolveCast(plan);
            return AttackResult.CreateSuccess();
        }

        /// <summary>
        /// fable_08 — valida e RESERVA o cast (cooldown, conhecimento, mana gasto). Não executa a
        /// magia ainda. Em sucesso devolve <paramref name="plan"/> != null para resolver/refundar.
        /// </summary>
        public AttackResult TryBeginCast(
            EquipmentSlot slot,
            ItemDataSO itemData,
            float lastAttackTime,
            Vector2 direction,
            Vector2 spawnPosition,
            out SpellCastPlan plan)
        {
            plan = null;

            var spellData = _itemResolver?.ResolveEquippedSpell(itemData);
            if (spellData == null)
            {
                Debug.LogError($"CombatLog: PlayerAttackBlocked. Reason=SpellNotResolved, ItemId={itemData.Id}, SpellId='{itemData.SpellId}'");
                return AttackResult.CreateError("SpellNotResolved");
            }

            // fable_07: a magia do item equipado fica disponível como fonte EquippedItem ENQUANTO
            // equipada, sem adicionar a knownSpellIds (CA-3: cast por item equipado inalterado).
            Spellbook?.SetEquipmentGrantedSpell(spellData.Id);

            float cooldown = Mathf.Max(0.1f, spellData.CooldownSeconds);
            if (!CooldownHelper.IsCooldownExpired(lastAttackTime, cooldown))
            {
                Debug.Log($"CombatLog: PlayerAttackBlocked. Reason=Cooldown, Slot={slot}, RemainingSeconds={CooldownHelper.GetRemainingCooldown(lastAttackTime, cooldown):F2}");
                return AttackResult.CreateError("Cooldown");
            }

            if (_manaManager != null && !_manaManager.TrySpendMana(spellData.ManaCost))
            {
                Debug.Log($"CombatLog: PlayerAttackBlocked. Reason=InsufficientMana, Slot={slot}, ManaCost={spellData.ManaCost}");
                return AttackResult.CreateError("InsufficientMana");
            }

            // Null prefab is allowed: ProjectileSpawnService falls back to RuntimeProjectileFactory
            // (procedural magic bolt tinted by damage type) so spells work before art is authored.
            CindarsHope.Combat.StatusEffect.StatusEffectSO statusEffect = ResolveStatusEffect(spellData);

            // F02: dano final via stats derivados (projéteis disparam como golpe leve).
            var finalDamage = StatsProvider != null
                ? StatsProvider.FinalDamage(spellData.BaseDamage, AttackWeight.Light, false, out _)
                : spellData.BaseDamage;

            plan = new SpellCastPlan
            {
                Slot = slot,
                Spell = spellData,
                Direction = direction,
                SpawnPosition = spawnPosition,
                FinalDamage = finalDamage,
                ManaSpent = _manaManager != null ? spellData.ManaCost : 0,
                StatusEffect = statusEffect
            };

            return AttackResult.CreateSuccess();
        }

        /// <summary>fable_08 — executa a magia conforme o shape. Chamado imediatamente (cast 0s) ou pelo routine.</summary>
        public AttackResult ResolveCast(SpellCastPlan plan)
        {
            if (plan == null || plan.Spell == null)
            {
                return AttackResult.CreateError("NullCastPlan");
            }

            var spell = plan.Spell;
            AttackResult result;
            switch (spell.Shape)
            {
                case SpellShape.Cone:
                    result = ExecuteCone(plan);
                    break;
                case SpellShape.Nova:
                    result = ExecuteNova(plan);
                    break;
                case SpellShape.SelfRestore:
                    result = ExecuteSelfRestore(plan);
                    break;
                case SpellShape.Barrier:
                    result = ExecuteBarrier(plan);
                    break;
                case SpellShape.Bolt:
                default:
                    result = ExecuteBolt(plan);
                    break;
            }

            if (result.Success)
            {
                if (_equipmentManager != null)
                    _equipmentManager.RegisterEquipmentUsage();
                GameEventBus.Publish(new SpellCastSucceededEvent(spell.Id));
                Debug.Log($"CombatLog: SpellResolved. Slot={plan.Slot}, Spell={spell.Id}, Shape={spell.Shape}, Damage={plan.FinalDamage}, ManaCost={plan.ManaSpent}");
            }

            return result;
        }

        /// <summary>fable_08 — reembolsa a mana reservada quando o cast é interrompido por dano.</summary>
        public void RefundCast(SpellCastPlan plan)
        {
            if (plan == null)
            {
                return;
            }

            if (_manaManager != null && plan.ManaSpent > 0)
            {
                _manaManager.RestoreMana(plan.ManaSpent);
            }

            Debug.Log($"CombatLog: SpellCastInterrupted. Spell={plan.Spell?.Id}, RefundedMana={plan.ManaSpent}");
            GameEventBus.Publish(new CindarsHope.Core.Events.SpellCastInterruptedEvent(plan.Spell?.Id, plan.ManaSpent));
        }

        // ----------------------------------------------------------------- shape executors

        private AttackResult ExecuteBolt(SpellCastPlan plan)
        {
            var spell = plan.Spell;
            Vector2 direction = plan.Direction;

            // EMENDA 6.6-A: Projétil Arcano (e qualquer Bolt com AutoTarget) mira no inimigo elegível.
            if (spell.AutoTarget && EnemyPositionQuery != null)
            {
                var candidates = EnemyPositionQuery(plan.SpawnPosition, spell.Range);
                direction = SpellTargeting.ResolveAimDirection(plan.SpawnPosition, plan.Direction, candidates, spell.Range);
            }

            return SpawnBoltProjectile(plan, direction);
        }

        private AttackResult SpawnBoltProjectile(SpellCastPlan plan, Vector2 direction)
        {
            var spell = plan.Spell;
            var spawnRequest = new ProjectileSpawnRequest(
                spell.ProjectilePrefab,
                plan.SpawnPosition,
                direction,
                spell.ProjectileSpeed,
                spell.Range,
                plan.FinalDamage,
                spell.DamageType,
                _knockbackForce,
                spawnOffset: 0.5f,
                statusEffect: plan.StatusEffect,
                statusApplyChance: spell.StatusApplyChance
            );
            spawnRequest.VisualStyle = ProjectileVisualStyle.MagicBolt;

            var spawnResult = ProjectileSpawnService.SpawnProjectile(spawnRequest);
            if (!spawnResult.Success)
            {
                Debug.LogError($"CombatLog: PlayerAttackBlocked. Reason=ProjectileSpawnFailed, ErrorCode={spawnResult.ErrorCode}");
                return AttackResult.CreateError("ProjectileSpawnFailed", spawnResult.ErrorCode);
            }

            Debug.Log($"CombatLog: SpellFired. Slot={plan.Slot}, Spell={spell.Id}, Range={spell.Range}, Speed={spell.ProjectileSpeed}, ManaCost={plan.ManaSpent}");
            return AttackResult.CreateSuccess();
        }

        // Cone = leque de N projéteis dentro do arco; reusa o spawn de projétil (regra de não-duplicação).
        private AttackResult ExecuteCone(SpellCastPlan plan)
        {
            var spell = plan.Spell;
            int count = Mathf.Max(1, spell.ConeProjectileCount);
            float spreadDeg = Mathf.Clamp(spell.ConeHalfAngleDegrees, 0f, 170f) * 2f;
            Vector2 baseDir = plan.Direction.sqrMagnitude > 0.0001f ? plan.Direction.normalized : Vector2.right;
            float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;

            int spawned = 0;
            for (int i = 0; i < count; i++)
            {
                // distribui simetricamente de -half a +half (1 projétil = reto à frente).
                float t = count == 1 ? 0.5f : (float)i / (count - 1);
                float angle = baseAngle - spreadDeg * 0.5f + spreadDeg * t;
                float rad = angle * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                var r = SpawnBoltProjectile(plan, dir);
                if (r.Success) spawned++;
            }

            if (spawned == 0)
            {
                return AttackResult.CreateError("ConeSpawnFailed");
            }

            Debug.Log($"CombatLog: SpellConeFired. Spell={spell.Id}, Projectiles={spawned}/{count}, SpreadDeg={spreadDeg:F1}");
            return AttackResult.CreateSuccess();
        }

        // Nova = OverlapCircleAll radial: dano + status em todos os inimigos no raio.
        private AttackResult ExecuteNova(SpellCastPlan plan)
        {
            var spell = plan.Spell;
            float radius = Mathf.Max(0.1f, spell.NovaRadius);
            var hits = Physics2D.OverlapCircleAll(plan.SpawnPosition, radius);
            int affected = 0;
            var seen = new HashSet<EnemyHealth>();

            foreach (var col in hits)
            {
                if (col == null) continue;
                var enemy = col.GetComponentInParent<EnemyHealth>() ?? col.GetComponent<EnemyHealth>();
                if (enemy == null || enemy.IsDead || !seen.Add(enemy))
                {
                    continue;
                }

                var damageRequest = new DamageRequest(enemy.EnemyId, plan.FinalDamage)
                {
                    DamageType = spell.DamageType,
                    SourcePosition = plan.SpawnPosition,
                    KnockbackForce = _knockbackForce
                };
                enemy.TakeDamage(damageRequest);

                if (plan.StatusEffect != null && spell.StatusApplyChance > 0f &&
                    (spell.StatusApplyChance >= 1f || Random.value <= spell.StatusApplyChance))
                {
                    enemy.ApplyStatusEffect(plan.StatusEffect);
                }

                affected++;
            }

            Debug.Log($"CombatLog: SpellNovaResolved. Spell={spell.Id}, Radius={radius:F2}, Affected={affected}");
            return AttackResult.CreateSuccess();
        }

        // SelfRestore = restaura recursos do próprio caster (sem alvo). Sempre "sucesso" (gastou mana).
        private AttackResult ExecuteSelfRestore(SpellCastPlan plan)
        {
            var spell = plan.Spell;
            int hp = 0, st = 0, mp = 0;

            if (spell.RestoreHp > 0 && PlayerManager != null)
            {
                PlayerManager.RestoreHP(spell.RestoreHp);
                hp = spell.RestoreHp;
            }

            if (spell.RestoreStamina > 0 && StaminaManager != null)
            {
                StaminaManager.AddStamina(spell.RestoreStamina);
                st = spell.RestoreStamina;
            }

            if (spell.RestoreMana > 0 && _manaManager != null)
            {
                _manaManager.RestoreMana(spell.RestoreMana);
                mp = spell.RestoreMana;
            }

            Debug.Log($"CombatLog: SpellSelfRestore. Spell={spell.Id}, HP=+{hp}, Stamina=+{st}, Mana=+{mp}");
            return AttackResult.CreateSuccess();
        }

        // Barrier = levanta PlayerBarrierState (absorve antes do PlayerDamageReceiver — F03).
        private AttackResult ExecuteBarrier(SpellCastPlan plan)
        {
            var spell = plan.Spell;
            PlayerBarrierState.Cast(spell.BarrierAbsorb, spell.BarrierSeconds, Time.time, spell.Id);
            Debug.Log($"CombatLog: SpellBarrierCast. Spell={spell.Id}, Absorb={spell.BarrierAbsorb}, Seconds={spell.BarrierSeconds:F2}");
            return AttackResult.CreateSuccess();
        }

        private CindarsHope.Combat.StatusEffect.StatusEffectSO ResolveStatusEffect(SpellDataSO spellData)
        {
            if (string.IsNullOrEmpty(spellData.StatusEffectId))
            {
                return null;
            }

            if (_statusEffectDatabase != null && _statusEffectDatabase.TryGetById(spellData.StatusEffectId, out var dbEffect))
            {
                return dbEffect;
            }

            return Resources.Load<CindarsHope.Combat.StatusEffect.StatusEffectSO>(spellData.StatusEffectId);
        }
    }
}
