namespace CindarsHope.Foundation
{
    /// <summary>Optional recovery port: stage equipment without publishing until inventory commits.</summary>
    public interface IEquipmentSilentMutationRuntime
    {
        bool TryEquipSilently(EquipmentSlot slot, string itemInstanceId);
        bool TryUnequipSilently(EquipmentSlot slot, string expectedItemInstanceId);
        void PublishCommittedEquip(EquipmentSlot slot, string itemInstanceId);
    }
}
