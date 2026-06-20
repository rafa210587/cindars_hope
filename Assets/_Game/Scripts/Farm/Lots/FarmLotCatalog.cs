using System.Collections.Generic;

namespace CindarsHope.Farm.Lots
{
    /// <summary>
    /// fable_41 — catálogo canônico (dado puro) dos 3 lotes de expansão e suas escrituras.
    ///
    /// Régua aprovada (spec §Objetivo + Data contracts; preços = sink de ouro 5.3):
    ///   lot_north → deed item North → 2500g (12 plots extras)
    ///   lot_east  → deed item East  → 4000g (pasto do 2º abrigo)
    ///   lot_west  → deed item West  → 3500g (pomar 4 árvores frutíferas sazonais)
    ///
    /// Os ids das escrituras seguem o catálogo F32 (CanonicalItemCatalog.AddKeys):
    /// item_key_lot_deed_north / _east / _west. KeyItem, BaseValue 0, não revendável.
    /// O preço aqui é o preço de COMPRA no ponto de venda (prefeitura/Veska), não o BaseValue.
    /// </summary>
    public static class FarmLotCatalog
    {
        public const string DeedItemNorth = "item_key_lot_deed_north";
        public const string DeedItemEast = "item_key_lot_deed_east";
        public const string DeedItemWest = "item_key_lot_deed_west";

        public const int PriceNorth = 2500;
        public const int PriceEast = 4000;
        public const int PriceWest = 3500;

        private static readonly Dictionary<string, FarmLotDefinition> ByLotId =
            new Dictionary<string, FarmLotDefinition>
            {
                {
                    FarmLotId.North,
                    new FarmLotDefinition(FarmLotId.North, DeedItemNorth, PriceNorth,
                        "Lote Norte - escritura na prefeitura")
                },
                {
                    FarmLotId.East,
                    new FarmLotDefinition(FarmLotId.East, DeedItemEast, PriceEast,
                        "Lote Leste - escritura na prefeitura")
                },
                {
                    FarmLotId.West,
                    new FarmLotDefinition(FarmLotId.West, DeedItemWest, PriceWest,
                        "Lote Oeste - escritura na prefeitura")
                },
            };

        private static readonly Dictionary<string, string> DeedItemToLot =
            new Dictionary<string, string>
            {
                { DeedItemNorth, FarmLotId.North },
                { DeedItemEast, FarmLotId.East },
                { DeedItemWest, FarmLotId.West },
            };

        public static IReadOnlyCollection<FarmLotDefinition> All => ByLotId.Values;

        public static bool TryGetByLotId(string lotId, out FarmLotDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(lotId))
            {
                return false;
            }

            return ByLotId.TryGetValue(lotId, out definition);
        }

        /// <summary>Resolve o lote destravado por uma escritura. Retorna null se o item não for escritura.</summary>
        public static string ResolveLotIdForDeed(string deedItemId)
        {
            if (string.IsNullOrWhiteSpace(deedItemId))
            {
                return null;
            }

            return DeedItemToLot.TryGetValue(deedItemId, out var lotId) ? lotId : null;
        }

        public static bool IsDeedItem(string itemId) => ResolveLotIdForDeed(itemId) != null;

        public static int GetDeedPrice(string lotId)
        {
            return TryGetByLotId(lotId, out var def) ? def.DeedPrice : 0;
        }
    }
}
