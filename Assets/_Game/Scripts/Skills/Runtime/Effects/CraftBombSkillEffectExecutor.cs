using System;
using CindarsHope.Combat.Weapon;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using CindarsHope.Player;
using CindarsHope.Skills.Runtime;
using UnityEngine;

namespace CindarsHope.Skills.Runtime.Effects
{
    public static class CraftBombRules
    {
        public const float ExplosionRadius = 1.8f;
        public const int MaximumTargets = 4;
        public static int ResolveDamage(int rank) => rank <= 1 ? 18 : rank == 2 ? 21 : 24;

        public static bool TryResolveDamageType(string itemId, out DamageType damageType)
        {
            var baseItemId = itemId;
            if (LivingForgeOutputVariantCatalog.TryDescribe(itemId, out var variant))
                baseItemId = variant.BaseItemId;
            return CraftingSkillItemIds.TryResolveBombDamageType(baseItemId, out damageType);
        }

        public static string SelectBomb(ISkillItemInventory inventory, string preferredItemId)
        {
            if (inventory == null) return string.Empty;
            if (TryResolveDamageType(preferredItemId, out _) &&
                inventory.GetAmount(preferredItemId) > 0) return preferredItemId;
            foreach (var baseId in CraftingSkillItemIds.BombIds)
            {
                foreach (var id in LivingForgeOutputVariantCatalog.EnumerateItemFamily(baseId))
                    if (inventory.GetAmount(id) > 0) return id;
            }
            return string.Empty;
        }
    }

    public sealed class CraftBombSkillEffectExecutor : ISkillEffectExecutor, IPreparableSkillEffectExecutor
    {
        public const string EffectIdValue = "crafting.bomba_improvisada";
        private readonly SkillActionSO _defaultAction;

        public CraftBombSkillEffectExecutor(SkillActionSO defaultAction = null) => _defaultAction = defaultAction;

        public string EffectId => EffectIdValue;
        public SkillEffectCategory Category => SkillEffectCategory.Combat;
        public SkillEffectTargetType TargetType => SkillEffectTargetType.None;

        public SkillEffectResult Validate(SkillEffectContext context)
        {
            if (context?.Caster == null || !context.Caster.activeInHierarchy)
                return SkillEffectResult.Failed("NoCaster", "Jogador não encontrado.");
            var inventory = DomainManagerRegistry.Get<IInventoryRuntime>() as InventoryManager;
            if (inventory == null)
                return SkillEffectResult.Failed("MissingInventory", "Inventário indisponível.");
            var selected = CraftBombRules.SelectBomb(new SkillItemInventoryAdapter(inventory), context.VariantId);
            return string.IsNullOrEmpty(selected)
                ? SkillEffectResult.Failed("MissingBomb", "Nenhuma bomba disponível.")
                : SkillEffectResult.Succeeded(string.Empty);
        }

        public SkillEffectResult Execute(SkillEffectContext context)
        {
            var readiness = Validate(context);
            if (!readiness.Success) return readiness;
            var inventory = DomainManagerRegistry.Get<IInventoryRuntime>() as InventoryManager;
            var adapter = new SkillItemInventoryAdapter(inventory);
            var inventoryBeforeCommit = inventory.CaptureSaveData();
            string bombId = CraftBombRules.SelectBomb(adapter, context.VariantId);
            if (!CraftBombRules.TryResolveDamageType(bombId, out var damageType))
                return SkillEffectResult.Failed("InvalidBomb", "Tipo de bomba inválido.");
            var commit = new SkillItemTransaction(adapter).TryCommit(bombId);
            if (!commit.Success)
                return SkillEffectResult.Failed("MissingBomb", "A bomba não estava disponível no commit.");

            var action = context.ActionData != null ? context.ActionData : _defaultAction;
            float range = action != null ? Math.Max(1f, action.ResolveRank(context.Rank).Range) : 5f;
            float speed = action != null ? Math.Max(1f, action.ProjectileSpeed) : 8f;
            var controller = context.Caster.GetComponentInChildren<PlayerController>()
                ?? context.Caster.GetComponent<PlayerController>();
            var direction = controller != null ? controller.LastFacingDirection : Vector2.right;
            if (direction.sqrMagnitude < .001f) direction = Vector2.right;
            Vector2 origin = context.Caster.transform.position;
            if (!CraftBombProjectile.TryLaunch(origin, direction.normalized, range, speed,
                CraftBombRules.ResolveDamage(context.Rank), damageType, context.SkillActionId))
            {
                inventory.RestoreFromSaveData(inventoryBeforeCommit);
                return SkillEffectResult.Failed("BombSpawnFailed", "Não foi possível lançar a bomba.");
            }

            return SkillEffectResult.Succeeded("Bomba lançada.", costSpent: true,
                cooldownStarted: true, cooldownSeconds: 6f);
        }
    }
}
