using System;
using System.Collections.Generic;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Equipment;
using CindarsHope.Farm;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using CindarsHope.Player;
using CindarsHope.Skills.Runtime;
using CindarsHope.Tools;
using UnityEngine;

namespace CindarsHope.Skills.Runtime.Effects
{
    public readonly struct IrrigationLineCandidate
    {
        public string Id { get; }
        public float X { get; }
        public float Y { get; }
        public bool IsDry { get; }

        public IrrigationLineCandidate(string id, float x, float y, bool isDry)
        {
            Id = id ?? string.Empty;
            X = x;
            Y = y;
            IsDry = isDry;
        }
    }

    public static class IrrigationLineRules
    {
        public static int ResolveLength(int rank) => rank <= 1 ? 3 : rank == 2 ? 4 : 5;

        public static string SelectCharge(ISkillItemInventory inventory, string preferredItemId)
        {
            if (inventory == null) return string.Empty;
            if (IsIrrigatorCharge(preferredItemId) && inventory.GetAmount(preferredItemId) > 0)
                return preferredItemId;

            foreach (var itemId in LivingForgeOutputVariantCatalog.EnumerateItemFamily(
                         CraftingSkillItemIds.IrrigatorCharge))
            {
                if (inventory.GetAmount(itemId) > 0)
                    return itemId;
            }
            return string.Empty;
        }

        public static bool IsIrrigatorCharge(string itemId)
        {
            if (string.Equals(itemId, CraftingSkillItemIds.IrrigatorCharge, StringComparison.Ordinal))
                return true;
            return LivingForgeOutputVariantCatalog.TryDescribe(itemId, out var variant)
                   && string.Equals(variant.BaseItemId, CraftingSkillItemIds.IrrigatorCharge,
                       StringComparison.Ordinal);
        }

        public static List<string> SelectDry(
            IEnumerable<IrrigationLineCandidate> candidates,
            float originX,
            float originY,
            float directionX,
            float directionY,
            int rank)
        {
            var result = new List<(string id, float forward)>();
            if (candidates == null) return new List<string>();
            var magnitude = Math.Sqrt(directionX * directionX + directionY * directionY);
            if (magnitude < .001d) { directionX = 1f; directionY = 0f; magnitude = 1d; }
            directionX /= (float)magnitude;
            directionY /= (float)magnitude;
            int length = ResolveLength(rank);

            foreach (var candidate in candidates)
            {
                if (!candidate.IsDry || string.IsNullOrWhiteSpace(candidate.Id)) continue;
                float dx = candidate.X - originX;
                float dy = candidate.Y - originY;
                float forward = dx * directionX + dy * directionY;
                float lateral = Math.Abs(dx * -directionY + dy * directionX);
                if (forward >= .25f && forward <= length + .5f && lateral <= .5f)
                    result.Add((candidate.Id, forward));
            }

            result.Sort((a, b) =>
            {
                int distance = a.forward.CompareTo(b.forward);
                return distance != 0 ? distance : string.CompareOrdinal(a.id, b.id);
            });
            if (result.Count > length) result.RemoveRange(length, result.Count - length);
            return result.ConvertAll(entry => entry.id);
        }
    }

    public sealed class IrrigationLineSkillEffectExecutor : ISkillEffectExecutor, IPreparableSkillEffectExecutor
    {
        public const string EffectIdValue = "farm.crop.water_skill";
        private const int StaminaCost = 18;
        private readonly SkillActionSO _defaultAction;
        private readonly EquipmentManager _equipment;

        public IrrigationLineSkillEffectExecutor(SkillActionSO defaultAction = null, EquipmentManager equipment = null)
        {
            _defaultAction = defaultAction;
            _equipment = equipment;
        }

        public string EffectId => EffectIdValue;
        public SkillEffectCategory Category => SkillEffectCategory.Farm;
        public SkillEffectTargetType TargetType => SkillEffectTargetType.None;

        public SkillEffectResult Validate(SkillEffectContext context)
        {
            if (context?.Caster == null || !context.Caster.activeInHierarchy)
                return SkillEffectResult.Failed("NoCaster", "Jogador não encontrado.");
            var equipment = _equipment != null ? _equipment : EquipmentManager.Instance;
            if (equipment == null || !equipment.HasTool(ToolType.WateringCan))
                return SkillEffectResult.Failed("WateringCanRequired", "Equipe um regador.");
            if (ResolveInventory() is not InventoryManager inventory ||
                string.IsNullOrEmpty(IrrigationLineRules.SelectCharge(
                    new SkillItemInventoryAdapter(inventory), context.VariantId)))
                return SkillEffectResult.Failed("MissingIrrigatorCharge", "Sem carga de irrigador.");
            var stamina = GameBootstrap.Instance?.StaminaManager as StaminaManager;
            if (stamina == null || stamina.CurrentStamina < StaminaCost)
                return SkillEffectResult.Failed("InsufficientStamina", "Stamina insuficiente (18).");
            return SelectPlots(context).Count > 0
                ? SkillEffectResult.Succeeded(string.Empty)
                : SkillEffectResult.Failed("NoDrySoil", "Nenhum solo seco na linha.");
        }

        public SkillEffectResult Execute(SkillEffectContext context)
        {
            var readiness = Validate(context);
            if (!readiness.Success) return readiness;
            var plots = SelectPlots(context);
            var inventory = (InventoryManager)ResolveInventory();
            var stamina = GameBootstrap.Instance.StaminaManager as StaminaManager;
            var inventoryBeforeCommit = inventory.CaptureSaveData();
            var chargeId = IrrigationLineRules.SelectCharge(
                new SkillItemInventoryAdapter(inventory), context.VariantId);
            if (!stamina.TrySpendStamina(StaminaCost))
                return SkillEffectResult.Failed("InsufficientStamina", "Stamina insuficiente (18).");

            var commit = new SkillItemTransaction(new SkillItemInventoryAdapter(inventory))
                .TryCommit(chargeId);
            if (!commit.Success)
            {
                stamina.AddStamina(StaminaCost);
                return SkillEffectResult.Failed("MissingIrrigatorCharge", "A carga não estava disponível no commit.");
            }

            int watered = 0;
            foreach (var plot in plots)
                if (plot != null && plot.TryWaterViaSkill()) watered++;
            if (watered == 0)
            {
                stamina.AddStamina(StaminaCost);
                inventory.RestoreFromSaveData(inventoryBeforeCommit);
                return SkillEffectResult.Failed("NoDrySoil", "Nenhum solo seco permaneceu válido.");
            }

            return SkillEffectResult.Succeeded($"Irrigador regou {watered} solo(s).",
                costSpent: true, cooldownStarted: true, cooldownSeconds: 10f);
        }

        private static IInventoryRuntime ResolveInventory() =>
            DomainManagerRegistry.Get<IInventoryRuntime>();

        private static List<FarmPlot> SelectPlots(SkillEffectContext context)
        {
            var selected = new List<FarmPlot>();
            var registry = FarmPlotRegistry.Active;
            if (registry?.Plots == null) return selected;
            var controller = context.Caster.GetComponentInChildren<PlayerController>()
                ?? context.Caster.GetComponent<PlayerController>();
            var direction = controller != null ? controller.LastFacingDirection : Vector2.right;
            var candidates = new List<IrrigationLineCandidate>();
            var byId = new Dictionary<string, FarmPlot>(StringComparer.Ordinal);
            foreach (var plot in registry.Plots)
            {
                if (plot == null) continue;
                candidates.Add(new IrrigationLineCandidate(plot.PlotId,
                    plot.transform.position.x, plot.transform.position.y, plot.CanBeWatered));
                byId[plot.PlotId] = plot;
            }

            foreach (var id in IrrigationLineRules.SelectDry(candidates,
                context.Caster.transform.position.x, context.Caster.transform.position.y,
                direction.x, direction.y, context.Rank))
                if (byId.TryGetValue(id, out var plot)) selected.Add(plot);
            return selected;
        }
    }
}
