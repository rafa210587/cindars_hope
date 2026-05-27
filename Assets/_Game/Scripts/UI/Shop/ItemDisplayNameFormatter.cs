using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CindarsHope.Inventory.Data;

namespace CindarsHope.UI.Shop
{
    public static class ItemDisplayNameFormatter
    {
        private const int MaximumRowNameLength = 22;

        private static readonly Dictionary<string, string> Aliases = new Dictionary<string, string>
        {
            { "item_seed_wheat", "Semente de Trigo" },
            { "item_seed_carrot", "Semente de Cenoura" },
            { "item_seed_moonbean", "Semente de Feijão Lua" },
            { "item_seed_sunpepper", "Semente de Pimenta Sol" },
            { "item_crop_wheat", "Trigo" },
            { "item_crop_carrot", "Cenoura" },
            { "item_shop_weapon_sword_iron", "Espada de Ferro" },
            { "item_weapon_training_sword", "Espada de Treino" },
            { "item_shop_armor_leather", "Armadura de Couro" },
            { "item_shop_tool_hoe_basic", "Enxada Básica" },
            { "item_consumable_potion_hp_small", "Poção Pequena" },
            { "item_consumable_food_bread", "Pão" },
            { "item_consumable_repair_kit_basic", "Kit de Reparo Básico" },
            { "item_consumable_repair_kit_standard", "Kit de Reparo" },
            { "item_consumable_repair_kit_superior", "Kit de Reparo Superior" },
            { "item_material_wood", "Madeira" },
            { "item_material_iron_ore", "Minério de Ferro" },
            { "item_material_stone", "Pedra" },
            { "item_material_copper_ore", "Minério de Cobre" },
            { "item_tool_fishing_rod_basic", "Cana Básica" },
        };

        private static readonly string[] TechnicalPrefixes =
        {
            "item_shop_",
            "item_consumable_",
            "item_material_",
            "item_crop_",
            "item_seed_",
            "item_",
            "weapon_",
            "equipment_"
        };

        public static string GetShortName(ItemDataSO item)
        {
            var name = GetFullName(item);
            return name.Length <= MaximumRowNameLength
                ? name
                : name.Substring(0, MaximumRowNameLength - 3) + "...";
        }

        public static string GetFullName(ItemDataSO item)
        {
            if (item == null)
            {
                return "Item desconhecido";
            }

            if (Aliases.TryGetValue(item.Id ?? string.Empty, out var alias))
            {
                return alias;
            }

            if (!string.IsNullOrWhiteSpace(item.DisplayName))
            {
                return item.DisplayName.Trim();
            }

            var id = item.Id ?? string.Empty;
            foreach (var prefix in TechnicalPrefixes)
            {
                if (id.StartsWith(prefix))
                {
                    id = id.Substring(prefix.Length);
                    break;
                }
            }

            var words = id.Split('_');
            var formatted = new StringBuilder();
            foreach (var word in words)
            {
                if (string.IsNullOrWhiteSpace(word))
                {
                    continue;
                }

                if (formatted.Length > 0)
                {
                    formatted.Append(' ');
                }

                formatted.Append(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(word));
            }

            return formatted.Length > 0 ? formatted.ToString() : "Item";
        }

        public static string GetDetails(ItemDataSO item)
        {
            return GetTooltip(item);
        }

        public static string GetTooltip(ItemDataSO item)
        {
            if (item == null)
            {
                return string.Empty;
            }

            var description = string.IsNullOrWhiteSpace(item.Description) ? "Sem descrição." : item.Description.Trim();
            return $"{GetFullName(item)}\nCategoria: {item.Category}\nValor base: {item.BaseValue}g\n{description}";
        }
    }
}
