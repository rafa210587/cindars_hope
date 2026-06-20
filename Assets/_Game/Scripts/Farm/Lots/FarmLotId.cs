namespace CindarsHope.Farm.Lots
{
    /// <summary>
    /// fable_41 — IDs estáveis dos 3 lotes de expansão da fazenda (HUD_LAYOUT_SCENES §4).
    /// Norte = 12 plots extras; Leste = pasto do 2º abrigo; Oeste = pomar (4 árvores frutíferas).
    /// Strings literais persistidas no save (ownedLots) — NUNCA renomear sem migração.
    /// </summary>
    public static class FarmLotId
    {
        public const string North = "lot_north";
        public const string East = "lot_east";
        public const string West = "lot_west";

        public static readonly string[] All = { North, East, West };

        public static bool IsKnown(string lotId)
        {
            if (string.IsNullOrWhiteSpace(lotId))
            {
                return false;
            }

            return lotId == North || lotId == East || lotId == West;
        }
    }
}
