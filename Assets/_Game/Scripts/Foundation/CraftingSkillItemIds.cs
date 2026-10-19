using System;
using System.Collections.Generic;

namespace CindarsHope.Foundation
{
    public static class CraftingSkillItemIds
    {
        public const string IrrigatorCharge = "item_consumable_irrigator_charge";
        public const string BombPhysical = "item_consumable_bomb_physical";
        public const string BombFire = "item_consumable_bomb_fire";
        public const string BombFrost = "item_consumable_bomb_frost";
        public const string BombShock = "item_consumable_bomb_shock";
        public const string BombToxic = "item_consumable_bomb_toxic";

        private static readonly IReadOnlyList<string> BombIdList = Array.AsReadOnly(new[]
        {
            BombPhysical, BombFire, BombFrost, BombShock, BombToxic
        });

        public static IReadOnlyList<string> BombIds => BombIdList;

        public static bool TryResolveBombDamageType(string itemId, out DamageType damageType)
        {
            switch (itemId)
            {
                case BombPhysical: damageType = DamageType.Physical; return true;
                case BombFire: damageType = DamageType.Fire; return true;
                case BombFrost: damageType = DamageType.Ice; return true;
                case BombShock: damageType = DamageType.Lightning; return true;
                case BombToxic: damageType = DamageType.Toxic; return true;
                default: damageType = DamageType.Physical; return false;
            }
        }
    }
}
