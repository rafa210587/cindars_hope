using CindarsHope.Combat;

namespace CindarsHope.Combat.Bestiary
{
    /// <summary>
    /// fable_33 — one canonical creature ficha from CAVE_BESTIARY_CATALOG_DIRECTION_v1.0.
    /// Pure data (no Unity refs), so it can be consumed by BOTH the editor asset generator
    /// (GenerateCanonicalBestiary) and EditMode tests (BestiaryDataTests) without an Editor
    /// dependency. Numbers come from the catalog tables (valued at the band's MINIMUM level);
    /// per-level scaling (+12% HP / +8% DMG) is applied by CaveBandScaling at spawn, never baked here.
    /// </summary>
    public struct BestiaryCreatureDef
    {
        /// <summary>Stable id (lowercase_snake). BestiaryEntryId == EnemyId per catalog §2.5.</summary>
        public string EnemyId;

        /// <summary>English display name (catalog name policy: trademarks renamed, rest kept).</summary>
        public string DisplayName;

        /// <summary>Theme band 1..7 (Stone, Fungal, Ice, Fire, Ruins, Deep, Void).</summary>
        public int Band;

        /// <summary>Inclusive minimum spawn level of the ficha's faixa (base stats are valued here).</summary>
        public int MinLevel;

        /// <summary>Inclusive maximum spawn level of the ficha's faixa.</summary>
        public int MaxLevel;

        /// <summary>Family tag from the catalog (Insect, Plant, Humanoid, Beast, Undead, Elemental,
        /// Construct, Aberration, Dragon, Dragon-kin, Elemental...). Drives F06 vulnerability matrix.</summary>
        public string Family;

        public BestiarySizeClass Size;
        public EnemyRole Role;

        /// <summary>Primary canonical Move (fable_24 enum). Must be one of the 22 moves.</summary>
        public EnemyMovementType MovePrimary;

        /// <summary>Optional secondary Move (GroundChase = unset). Must be one of the 22 moves.</summary>
        public EnemyMovementType MoveSecondary;

        // Base stat block at MinLevel (multipliers per TYPE already applied in the catalog numbers).
        public int Hp;
        public int Damage;
        public int Defense;
        public int Xp;

        /// <summary>SpoilerTier 0..4 (commons 0-1, miniboss 2, gate boss 3, the Four 4).</summary>
        public int SpoilerTier;

        public bool IsMiniBoss;
        public bool IsBoss;

        /// <summary>Primary loot id used as EnemyDataSO.dropItemId — MUST be an existing item_* id
        /// (fable_32). Full per-family loot tables/weights are authored by F06; here this single id
        /// keeps the F30 drop cross-ref green.</summary>
        public string PrimaryDropItemId;

        /// <summary>Primary damage type hint (physical/fire/ice/poison/arcane...).</summary>
        public string PrimaryDamageTypeId;

        /// <summary>Family vulnerability matrix profile id (F06). Empty = neutral fallback.</summary>
        public string VulnerabilityMatrixProfileId;

        /// <summary>Cross-cutting catalog markers: [NOTURNA], [AQUATICA], [NAO-AGRESSIVA].</summary>
        public bool IsNocturnal;
        public bool IsAquatic;
        public bool IsNonAggressive;

        /// <summary>Dormant behaviour / extra ficha context recorded as INFO (never a new Move).</summary>
        public string Notes;
    }
}
