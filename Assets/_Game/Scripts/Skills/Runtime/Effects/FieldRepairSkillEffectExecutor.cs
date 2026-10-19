using System.Collections.Generic;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Equipment;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.Skills.Runtime.Effects
{
    public sealed class FieldRepairSkillEffectExecutor : ISkillEffectExecutor,
        IPreparableSkillEffectExecutor, ISkillCastLifecycleExecutor, ISkillCastTimingResolver
    {
        private readonly string _effectId;
        private readonly string _displayName;
        private readonly SkillActionSO _defaultAction;
        private readonly EquipmentManager _equipmentManager;

        public string EffectId => _effectId;
        public SkillEffectCategory Category => SkillEffectCategory.Utility;
        public SkillEffectTargetType TargetType => SkillEffectTargetType.None;

        public FieldRepairSkillEffectExecutor(string effectId, string displayName,
            SkillActionSO defaultAction, EquipmentManager equipmentManager)
        {
            _effectId = effectId;
            _displayName = displayName;
            _defaultAction = defaultAction;
            _equipmentManager = equipmentManager;
        }

        public SkillEffectResult Validate(SkillEffectContext context)
        {
            var action = context?.ActionData ?? _defaultAction;
            var transaction = CreateTransaction();
            if (action == null || transaction == null)
                return Failed("RuntimeUnavailable", "Sistemas de reparo indisponíveis.");
            if (IsQuickRepair && CombatStateProvider.IsInCombat?.Invoke() == true)
                return Failed("InCombat", "Reparo Rápido exige estar fora de combate.");
            var result = transaction.Validate(KitId, RepairFraction(action, context.Rank),
                action.ResolveSecondaryMagnitude(context.Rank));
            return result.Success ? SkillEffectResult.Succeeded("Pronto.") : ToFailure(result);
        }

        public SkillEffectResult Execute(SkillEffectContext context)
        {
            var validation = Validate(context);
            if (!validation.Success) return validation;
            var action = context.ActionData ?? _defaultAction;
            var result = CreateTransaction().TryCommit(KitId, RepairFraction(action, context.Rank),
                action.ResolveSecondaryMagnitude(context.Rank));
            return result.Success
                ? SkillEffectResult.Succeeded($"{_displayName}: +{result.Restored} durabilidade.", true, true)
                : ToFailure(result);
        }

        public float ResolveWindupSeconds(SkillEffectContext context, float authoredWindupSeconds)
        {
            if (!IsQuickRepair) return Mathf.Max(0f, authoredWindupSeconds);
            var action = context?.ActionData ?? _defaultAction;
            return action != null ? action.ResolveSecondaryDuration(context?.Rank ?? 1) : authoredWindupSeconds;
        }

        public void OnCastStarted(SkillEffectContext context) { }

        public SkillEffectResult TickBeforeCommit(SkillEffectContext context, float deltaSeconds)
        {
            if (IsQuickRepair && CombatStateProvider.IsInCombat?.Invoke() == true)
                return Failed("InCombat", "Combate interrompeu o Reparo Rápido.");
            return SkillEffectResult.Succeeded("Pronto.");
        }

        public void OnCastCancelled(SkillEffectContext context) { }

        private bool IsQuickRepair => _effectId == SkillActionEffectCatalog.CraftingQuickRepairEffectId;
        private string KitId => IsQuickRepair ? CraftingRepairItemIds.StandardKit : CraftingRepairItemIds.BasicKit;
        private static float RepairFraction(SkillActionSO action, int rank) =>
            action.ResolveEffectMagnitude(rank); // zero means fill exactly to the authored cap.

        private CraftingRepairTransaction CreateTransaction()
        {
            var inventory = GameBootstrap.Instance?.InventoryManager as InventoryManager;
            if (inventory == null || _equipmentManager == null) return null;
            return new CraftingRepairTransaction(new SkillItemInventoryAdapter(inventory),
                new EquipmentAdapter(_equipmentManager));
        }

        private static SkillEffectResult ToFailure(FieldRepairResult result) =>
            Failed(result.Failure.ToString(), result.Failure == FieldRepairFailure.MissingKit
                ? "Kit de reparo necessário indisponível."
                : "Nenhum equipamento elegível pode ser reparado.");

        private static SkillEffectResult Failed(string reason, string message) =>
            SkillEffectResult.Failed(reason, message);

        private sealed class EquipmentAdapter : IFieldRepairEquipment
        {
            private readonly EquipmentManager _manager;
            public EquipmentAdapter(EquipmentManager manager) { _manager = manager; }

            public IReadOnlyList<FieldRepairTarget> GetEquippedTargets()
            {
                var source = _manager.GetAllEquippedItems();
                var result = new List<FieldRepairTarget>(source.Count);
                for (int i = 0; i < source.Count; i++)
                {
                    var item = source[i];
                    result.Add(new FieldRepairTarget(item.ItemInstanceId,
                        Mathf.RoundToInt(item.DurabilityCurrent), Mathf.RoundToInt(item.DurabilityMax),
                        item.SlotIndex, item.IsBroken, AccessoryCatalog.IsRelic(item.ItemInstanceId)));
                }
                return result;
            }

            public bool TryApplyExactRepair(FieldRepairTarget expected, int amount, int capDurability, out int restored)
                => _manager.TryRepairItemExact(expected.ItemInstanceId, expected.CurrentDurability,
                    expected.MaxDurability, amount, capDurability, out restored);
        }
    }
}
