// arch: quebra do ciclo mutuo Equipment|Save (spec_arch_equipment_save_cycle_reduction_v29) — enum
// puro (sem refs Unity) movido de CindarsHope.Equipment para CindarsHope.Foundation, decisao
// explicita de arquitetura (o DTO de save EquipmentSlotSaveData referencia este enum; para o DTO
// residir em Foundation sem puxar Equipment de volta, o enum tambem precisa estar em Foundation).
namespace CindarsHope.Foundation
{
    public enum EquipmentSlot
    {
        None,
        LeftHand,
        RightHand,
        Head,
        Chest,
        Legs,
        Boots,
        Ring1,
        Ring2,
        Accessory
    }
}
