namespace CindarsHope.Audio
{
    /// <summary>
    /// fable_58 — categorias estáveis de SFX (ids estáveis para a tabela
    /// evento→categoria→clipe). A fase de arte troca o clipe placeholder por
    /// um asset real sem tocar em gameplay; o id é o contrato.
    ///
    /// IMPORTANTE: ordem/valor são estáveis (evite reordenar). Adicionar SEMPRE
    /// no fim. Strings estáveis em <see cref="SfxCategoryIds"/>.
    /// </summary>
    public enum SfxCategory
    {
        None = 0,
        Hit = 1,
        PerfectBlock = 2,
        PostureBreak = 3,
        Charged = 4,
        Status = 5,
        UiToast = 6,
        Pickup = 7,
        LevelUp = 8,
        DayStart = 9,
        Save = 10,
        Block = 11,
        EnemyKilled = 12,
        Craft = 13,
        Harvest = 14,
        Fish = 15
    }

    /// <summary>
    /// Ids estáveis (string) por categoria — usados em logs de wiring e no
    /// contrato com a fase de arte. Espelham o enum <see cref="SfxCategory"/>.
    /// </summary>
    public static class SfxCategoryIds
    {
        public const string Hit = "sfx_hit";
        public const string PerfectBlock = "sfx_perfect_block";
        public const string PostureBreak = "sfx_posture_break";
        public const string Charged = "sfx_charged";
        public const string Status = "sfx_status";
        public const string UiToast = "sfx_ui_toast";
        public const string Pickup = "sfx_pickup";
        public const string LevelUp = "sfx_levelup";
        public const string DayStart = "sfx_day_start";
        public const string Save = "sfx_save";
        public const string Block = "sfx_block";
        public const string EnemyKilled = "sfx_enemy_killed";
        public const string Craft = "sfx_craft";
        public const string Harvest = "sfx_harvest";
        public const string Fish = "sfx_fish";

        /// <summary>Id estável para a categoria, ou string vazia para None/desconhecido.</summary>
        public static string ToStableId(SfxCategory category)
        {
            switch (category)
            {
                case SfxCategory.Hit: return Hit;
                case SfxCategory.PerfectBlock: return PerfectBlock;
                case SfxCategory.PostureBreak: return PostureBreak;
                case SfxCategory.Charged: return Charged;
                case SfxCategory.Status: return Status;
                case SfxCategory.UiToast: return UiToast;
                case SfxCategory.Pickup: return Pickup;
                case SfxCategory.LevelUp: return LevelUp;
                case SfxCategory.DayStart: return DayStart;
                case SfxCategory.Save: return Save;
                case SfxCategory.Block: return Block;
                case SfxCategory.EnemyKilled: return EnemyKilled;
                case SfxCategory.Craft: return Craft;
                case SfxCategory.Harvest: return Harvest;
                case SfxCategory.Fish: return Fish;
                default: return string.Empty;
            }
        }
    }
}
