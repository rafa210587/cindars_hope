namespace CindarsHope.Foundation
{
    public interface IEquipmentRuntime
    {
        string GetEquippedItem(EquipmentSlot slot);
        void EquipItem(EquipmentSlot slot, string itemInstanceId);
    }
}
