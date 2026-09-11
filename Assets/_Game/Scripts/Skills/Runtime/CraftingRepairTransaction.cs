using System;
using System.Collections.Generic;

namespace CindarsHope.Skills.Runtime
{
    public static class CraftingRepairItemIds
    {
        public const string BasicKit = "item_consumable_repair_kit_basic";
        public const string StandardKit = "item_consumable_repair_kit_standard";
    }

    public readonly struct FieldRepairTarget
    {
        public string ItemInstanceId { get; }
        public int CurrentDurability { get; }
        public int MaxDurability { get; }
        public int SlotIndex { get; }
        public bool IsBroken { get; }
        public bool IsArtifact { get; }

        public FieldRepairTarget(string itemInstanceId, int currentDurability, int maxDurability,
            int slotIndex, bool isBroken, bool isArtifact)
        {
            ItemInstanceId = itemInstanceId ?? string.Empty;
            CurrentDurability = currentDurability;
            MaxDurability = maxDurability;
            SlotIndex = slotIndex;
            IsBroken = isBroken;
            IsArtifact = isArtifact;
        }
    }

    public interface IFieldRepairEquipment
    {
        IReadOnlyList<FieldRepairTarget> GetEquippedTargets();
        bool TryApplyExactRepair(FieldRepairTarget expected, int amount, int capDurability, out int restored);
    }

    public enum FieldRepairFailure
    {
        None = 0,
        InvalidRequest,
        NoRepairableEquipment,
        BrokenEquipment,
        ArtifactEquipment,
        AtRepairCap,
        MissingKit,
        InventoryCommitFailed,
        EquipmentChanged,
        RollbackFailed
    }

    public readonly struct FieldRepairResult
    {
        public bool Success { get; }
        public FieldRepairFailure Failure { get; }
        public string ItemInstanceId { get; }
        public int Restored { get; }

        public FieldRepairResult(bool success, FieldRepairFailure failure, string itemInstanceId, int restored)
        {
            Success = success;
            Failure = failure;
            ItemInstanceId = itemInstanceId ?? string.Empty;
            Restored = restored;
        }
    }

    /// <summary>Atomic inventory-to-equipment repair with compensation if the output cannot commit.</summary>
    public sealed class CraftingRepairTransaction
    {
        private readonly IReversibleSkillItemInventory _inventory;
        private readonly IFieldRepairEquipment _equipment;

        public CraftingRepairTransaction(IReversibleSkillItemInventory inventory, IFieldRepairEquipment equipment)
        {
            _inventory = inventory;
            _equipment = equipment;
        }

        public FieldRepairResult Validate(string kitId, float repairFraction, float capFraction)
            => Prepare(kitId, repairFraction, capFraction, out _, out _, out _);

        public FieldRepairResult TryCommit(string kitId, float repairFraction, float capFraction)
        {
            var prepared = Prepare(kitId, repairFraction, capFraction, out var target, out int amount, out int cap);
            if (!prepared.Success) return prepared;
            if (!_inventory.RemoveItem(kitId, 1))
                return Failed(FieldRepairFailure.InventoryCommitFailed, target.ItemInstanceId);
            if (_equipment.TryApplyExactRepair(target, amount, cap, out int restored) && restored > 0)
                return new FieldRepairResult(true, FieldRepairFailure.None, target.ItemInstanceId, restored);
            return _inventory.AddItem(kitId, 1)
                ? Failed(FieldRepairFailure.EquipmentChanged, target.ItemInstanceId)
                : Failed(FieldRepairFailure.RollbackFailed, target.ItemInstanceId);
        }

        private FieldRepairResult Prepare(string kitId, float repairFraction, float capFraction,
            out FieldRepairTarget target, out int amount, out int cap)
        {
            target = default;
            amount = 0;
            cap = 0;
            if (_inventory == null || _equipment == null || string.IsNullOrWhiteSpace(kitId) ||
                repairFraction < 0f || capFraction <= 0f || capFraction > 1f)
                return Failed(FieldRepairFailure.InvalidRequest);
            if (_inventory.GetAmount(kitId) < 1)
                return Failed(FieldRepairFailure.MissingKit);

            var targets = _equipment.GetEquippedTargets();
            bool sawBroken = false;
            bool sawArtifact = false;
            bool sawAtCap = false;
            double bestRatio = double.MaxValue;
            if (targets != null)
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    var candidate = targets[i];
                    if (candidate.IsArtifact) { sawArtifact = true; continue; }
                    if (candidate.IsBroken || candidate.CurrentDurability <= 0) { sawBroken = true; continue; }
                    if (candidate.MaxDurability <= 0 || string.IsNullOrWhiteSpace(candidate.ItemInstanceId)) continue;
                    int candidateCap = Math.Min(candidate.MaxDurability,
                        (int)Math.Floor(candidate.MaxDurability * (double)capFraction));
                    if (candidate.CurrentDurability >= candidateCap) { sawAtCap = true; continue; }
                    double ratio = (double)candidate.CurrentDurability / candidate.MaxDurability;
                    if (ratio < bestRatio || (Math.Abs(ratio - bestRatio) < 0.0000001 &&
                        (candidate.SlotIndex < target.SlotIndex ||
                         (candidate.SlotIndex == target.SlotIndex && string.CompareOrdinal(candidate.ItemInstanceId, target.ItemInstanceId) < 0))))
                    {
                        bestRatio = ratio;
                        target = candidate;
                        cap = candidateCap;
                    }
                }
            }
            if (string.IsNullOrEmpty(target.ItemInstanceId))
            {
                if (sawArtifact) return Failed(FieldRepairFailure.ArtifactEquipment);
                if (sawBroken) return Failed(FieldRepairFailure.BrokenEquipment);
                if (sawAtCap) return Failed(FieldRepairFailure.AtRepairCap);
                return Failed(FieldRepairFailure.NoRepairableEquipment);
            }
            amount = repairFraction > 0f
                ? Math.Max(1, (int)Math.Ceiling(target.MaxDurability * (double)repairFraction))
                : cap - target.CurrentDurability;
            amount = Math.Min(amount, cap - target.CurrentDurability);
            return new FieldRepairResult(true, FieldRepairFailure.None, target.ItemInstanceId, 0);
        }

        private static FieldRepairResult Failed(FieldRepairFailure failure, string id = null)
            => new FieldRepairResult(false, failure, id, 0);
    }
}
