using System.Collections.Generic;

namespace CindarsHope.Farm.Animals
{
    /// <summary>
    /// fable_12 — catálogo canônico dos 3 animais de fazenda no escopo (galinha/cabra/vaca) e seus
    /// IDs estáveis (ITEM_CATALOG §18). Fonte única compartilhada pelo registry de runtime e pelo
    /// gerador de assets/itens/shop — evita duplicação de IDs entre código e editor.
    ///
    /// Espécie Sheep (lã) existe no enum WAVE 05 mas está FORA do escopo desta spec (sem cabra
    /// substituída por ovelha; cabra usa Goat=produto goat_milk). Mapeamento: a cabra reusa a
    /// espécie Sheep do enum como "ruminante de leite menor" porque o enum WAVE 05 não tem Goat;
    /// o produto e o ID a distinguem (item_animal_goat_milk).
    /// </summary>
    public static class FarmAnimalCatalog
    {
        // IDs de animal (estáveis).
        public const string AnimalChicken = "animal_chicken";
        public const string AnimalGoat = "animal_goat";
        public const string AnimalCow = "animal_cow";
        public const string AnimalSheep = "animal_sheep"; // slice village_economy: lã → cadeia de tecido

        // Itens de filhote (compra no Eiran).
        public const string ItemChickChicken = "item_animal_chicken_chick";
        public const string ItemKidGoat = "item_animal_goat_kid";
        public const string ItemCalfCow = "item_animal_cow_calf";
        public const string ItemLambSheep = "item_animal_sheep_lamb";

        // Ração (genérica para os três; comprável/craftável).
        public const string ItemFeed = "item_animal_feed";

        // Produtos base (Normal) — ITEM_CATALOG §18.
        public const string ItemEgg = "item_animal_egg";
        public const string ItemGoatMilk = "item_animal_goat_milk";
        public const string ItemCowMilk = "item_animal_cow_milk";
        public const string ItemWool = "item_animal_wool";

        // Limiares de qualidade por dias consecutivos de FedToday (EMENDA 2026-06-12, §18).
        public const int SilverConsecutiveFedDays = 3;
        public const int GoldConsecutiveFedDays = 7;

        // Decisão Fase 0 (EMENDA 5.1-A): morte permanente após 7 dias consecutivos sem comida.
        public const int NeglectDeathDays = 7;

        public struct AnimalCatalogEntry
        {
            public string AnimalId;
            public string DisplayName;
            public FarmAnimalSpecies Species;
            public AnimalHousingBuildingType HousingType;
            public string PurchaseItemId;
            public string FeedItemId;
            public string ProductItemId;
            public int ProductIntervalDays;
        }

        public static IReadOnlyList<AnimalCatalogEntry> GetAll()
        {
            return new List<AnimalCatalogEntry>
            {
                new AnimalCatalogEntry
                {
                    AnimalId = AnimalChicken,
                    DisplayName = "Galinha",
                    Species = FarmAnimalSpecies.Chicken,
                    HousingType = AnimalHousingBuildingType.Coop,
                    PurchaseItemId = ItemChickChicken,
                    FeedItemId = ItemFeed,
                    ProductItemId = ItemEgg,
                    ProductIntervalDays = 1
                },
                new AnimalCatalogEntry
                {
                    AnimalId = AnimalGoat,
                    DisplayName = "Cabra",
                    Species = FarmAnimalSpecies.Sheep,
                    HousingType = AnimalHousingBuildingType.Barn,
                    PurchaseItemId = ItemKidGoat,
                    FeedItemId = ItemFeed,
                    ProductItemId = ItemGoatMilk,
                    ProductIntervalDays = 1
                },
                new AnimalCatalogEntry
                {
                    AnimalId = AnimalCow,
                    DisplayName = "Vaca",
                    Species = FarmAnimalSpecies.Cow,
                    HousingType = AnimalHousingBuildingType.Barn,
                    PurchaseItemId = ItemCalfCow,
                    FeedItemId = ItemFeed,
                    ProductItemId = ItemCowMilk,
                    ProductIntervalDays = 1
                },
                // Ovelha (slice village_economy) — produz lã, fechando a cadeia de tecido (Tear/Sewing).
                new AnimalCatalogEntry
                {
                    AnimalId = AnimalSheep,
                    DisplayName = "Ovelha",
                    Species = FarmAnimalSpecies.Sheep,
                    HousingType = AnimalHousingBuildingType.Barn,
                    PurchaseItemId = ItemLambSheep,
                    FeedItemId = ItemFeed,
                    ProductItemId = ItemWool,
                    ProductIntervalDays = 1
                }
            };
        }

        /// <summary>
        /// Resolve o item de produto final aplicando o sufixo de qualidade por dias consecutivos
        /// de alimentação (EMENDA §18): &gt;=7 = _gold, &gt;=3 = _silver, senão base (Normal).
        /// </summary>
        public static string ResolveProductItemId(string baseProductItemId, int consecutiveFedDays)
        {
            if (string.IsNullOrWhiteSpace(baseProductItemId))
            {
                return baseProductItemId;
            }

            if (consecutiveFedDays >= GoldConsecutiveFedDays)
            {
                return baseProductItemId + "_gold";
            }

            if (consecutiveFedDays >= SilverConsecutiveFedDays)
            {
                return baseProductItemId + "_silver";
            }

            return baseProductItemId;
        }
    }
}
