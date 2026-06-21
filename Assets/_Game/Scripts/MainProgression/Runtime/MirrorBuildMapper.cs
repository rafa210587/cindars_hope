using System.Collections.Generic;
using CindarsHope.Skills;

namespace CindarsHope.MainProgression.Runtime
{
    /// <summary>
    /// fable_43 — Cindrathel ("the spirit that mirrors the player's build") mechanic. PURE and
    /// DETERMINISTIC: given a snapshot of the player's REAL loadout (inferred dominant tree, equipped
    /// weapon family and the active skill ids in the slots), it produces a stable boss config that
    /// the F05 boss-phase AI consumes. No Unity reference, no randomness, no saved state — the same
    /// input always yields the same config (testable in EditMode).
    ///
    /// The adapter reads the live loadout at encounter start and feeds it here once; the mapper never
    /// reads global state. An empty/unknown loadout falls back to the canonical "basic warrior"
    /// mirror so the fight is always valid (risk mitigation: build vazio).
    /// </summary>
    public enum MirrorWeaponFamily
    {
        Unknown = 0,
        Sword = 1,
        Bow = 2,
        Staff = 3,
        Dagger = 4,
        Hammer = 5,
        Unarmed = 6
    }

    /// <summary>Immutable snapshot of the player's build (simple types only — no Unity refs).</summary>
    public readonly struct MirrorBuildInput
    {
        public readonly SkillTreeId DominantTree;
        public readonly bool HasDominant;
        public readonly MirrorWeaponFamily WeaponFamily;
        public readonly IReadOnlyList<string> ActiveSkillIds;

        public MirrorBuildInput(
            SkillTreeId dominantTree,
            bool hasDominant,
            MirrorWeaponFamily weaponFamily,
            IReadOnlyList<string> activeSkillIds)
        {
            DominantTree = dominantTree;
            HasDominant = hasDominant;
            WeaponFamily = weaponFamily;
            ActiveSkillIds = activeSkillIds ?? System.Array.Empty<string>();
        }
    }

    /// <summary>Which broad combat archetype Cindrathel adopts to mirror the player.</summary>
    public enum MirrorArchetype
    {
        Bruiser = 0,    // melee / sword / hammer
        Marksman = 1,   // ranged / bow
        Caster = 2,     // magic / staff
        Skirmisher = 3, // dagger / survival
        Support = 4     // crafting / bond
    }

    /// <summary>
    /// Deterministic boss config for the mirror fight. Phase count and mirrored skill list feed the
    /// F05 boss AI; the archetype selects which move set the AI uses.
    /// </summary>
    public sealed class MirrorBuildConfig
    {
        public MirrorArchetype Archetype { get; }
        public int PhaseCount { get; }
        public IReadOnlyList<string> MirroredSkillIds { get; }
        public bool UsedFallback { get; }

        public MirrorBuildConfig(
            MirrorArchetype archetype, int phaseCount, IReadOnlyList<string> mirroredSkillIds, bool usedFallback)
        {
            Archetype = archetype;
            PhaseCount = phaseCount;
            MirroredSkillIds = mirroredSkillIds ?? System.Array.Empty<string>();
            UsedFallback = usedFallback;
        }
    }

    public static class MirrorBuildMapper
    {
        public const int MinPhases = 2;
        public const int MaxPhases = 4;

        /// <summary>The canonical "basic warrior" fallback used when the loadout is empty/unknown.</summary>
        public static MirrorBuildConfig BasicWarriorFallback() =>
            new MirrorBuildConfig(MirrorArchetype.Bruiser, MinPhases, System.Array.Empty<string>(), true);

        /// <summary>
        /// Deterministic mapping loadout -> mirror config. With no dominant tree AND no weapon family
        /// AND no active skills, returns the basic-warrior fallback. Otherwise the archetype is chosen
        /// from the dominant tree (preferred) or the weapon family, and the phase count scales with how
        /// many active skills the player actually slotted (clamped to [2,4]).
        /// </summary>
        public static MirrorBuildConfig Map(MirrorBuildInput input)
        {
            bool emptyLoadout = !input.HasDominant
                                && input.WeaponFamily == MirrorWeaponFamily.Unknown
                                && input.ActiveSkillIds.Count == 0;
            if (emptyLoadout)
                return BasicWarriorFallback();

            var archetype = input.HasDominant
                ? ArchetypeForTree(input.DominantTree)
                : ArchetypeForWeapon(input.WeaponFamily);

            // Phase count scales with slotted skills (more build invested => longer mirror fight).
            int phases = MinPhases + (input.ActiveSkillIds.Count >= 4 ? 2 : input.ActiveSkillIds.Count >= 2 ? 1 : 0);
            if (phases > MaxPhases) phases = MaxPhases;

            // Mirror the player's slotted skills verbatim (stable order, de-duplicated).
            var mirrored = new List<string>();
            foreach (var id in input.ActiveSkillIds)
            {
                if (string.IsNullOrEmpty(id)) continue;
                if (!mirrored.Contains(id)) mirrored.Add(id);
            }

            return new MirrorBuildConfig(archetype, phases, mirrored, false);
        }

        public static MirrorArchetype ArchetypeForTree(SkillTreeId tree)
        {
            switch (tree)
            {
                case SkillTreeId.Melee: return MirrorArchetype.Bruiser;
                case SkillTreeId.Ranged: return MirrorArchetype.Marksman;
                case SkillTreeId.Magic: return MirrorArchetype.Caster;
                case SkillTreeId.Survival: return MirrorArchetype.Skirmisher;
                case SkillTreeId.Crafting: return MirrorArchetype.Support;
                default: return MirrorArchetype.Bruiser;
            }
        }

        public static MirrorArchetype ArchetypeForWeapon(MirrorWeaponFamily family)
        {
            switch (family)
            {
                case MirrorWeaponFamily.Sword:
                case MirrorWeaponFamily.Hammer:
                case MirrorWeaponFamily.Unarmed:
                    return MirrorArchetype.Bruiser;
                case MirrorWeaponFamily.Bow: return MirrorArchetype.Marksman;
                case MirrorWeaponFamily.Staff: return MirrorArchetype.Caster;
                case MirrorWeaponFamily.Dagger: return MirrorArchetype.Skirmisher;
                default: return MirrorArchetype.Bruiser;
            }
        }
    }
}
