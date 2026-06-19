using CindarsHope.Combat.Weapon;
using CindarsHope.Core.Data;
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

        public AttackResult TryCast(
            EquipmentSlot slot,
            ItemDataSO itemData,
            float lastAttackTime,
            Vector2 direction,
            Vector2 spawnPosition)
        {
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
            CindarsHope.Combat.StatusEffect.StatusEffectSO statusEffect = null;
            if (!string.IsNullOrEmpty(spellData.StatusEffectId))
            {
                if (_statusEffectDatabase != null && _statusEffectDatabase.TryGetById(spellData.StatusEffectId, out var dbEffect))
                    statusEffect = dbEffect;
                else
                    statusEffect = Resources.Load<CindarsHope.Combat.StatusEffect.StatusEffectSO>(spellData.StatusEffectId);
            }

            // F02: dano final via stats derivados (projéteis disparam como golpe leve).
            var finalDamage = StatsProvider != null
                ? StatsProvider.FinalDamage(spellData.BaseDamage, AttackWeight.Light, false, out _)
                : spellData.BaseDamage;

            var spawnRequest = new ProjectileSpawnRequest(
                spellData.ProjectilePrefab,
                spawnPosition,
                direction,
                spellData.ProjectileSpeed,
                spellData.Range,
                finalDamage,
                spellData.DamageType,
                _knockbackForce,
                spawnOffset: 0.5f,
                statusEffect: statusEffect,
                statusApplyChance: spellData.StatusApplyChance
            );
            spawnRequest.VisualStyle = ProjectileVisualStyle.MagicBolt;

            var spawnResult = ProjectileSpawnService.SpawnProjectile(spawnRequest);
            if (!spawnResult.Success)
            {
                Debug.LogError($"CombatLog: PlayerAttackBlocked. Reason=ProjectileSpawnFailed, ErrorCode={spawnResult.ErrorCode}");
                return AttackResult.CreateError("ProjectileSpawnFailed", spawnResult.ErrorCode);
            }

            if (_equipmentManager != null)
                _equipmentManager.RegisterEquipmentUsage();

            Debug.Log($"CombatLog: SpellFired. Slot={slot}, Spell={spellData.Id}, Range={spellData.Range}, Speed={spellData.ProjectileSpeed}, ManaCost={spellData.ManaCost}");
            return AttackResult.CreateSuccess();
        }
    }
}
