using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Editor.SceneCreation
{
    /// <summary>
    /// Preservation-first 76×64 town bounds and stable district/spawn contracts. Pure C#
    /// (no UnityEditor / scene side effects) so the layout can be
    /// exercised by EditMode tests: bounds, the seven named district rectangles, the
    /// element→district table (no orphan), the scale transform that repositions every existing
    /// element into the larger footprint, and the stable schedule-anchor / spawn-point ID lists.
    ///
    /// The generator (<see cref="CreateMvpTownScene"/>) consumes this for the new perimeter, the
    /// wander clamps, and the two new districts (lake/park SW, town hall NE + mural). It is the
    /// single source of truth for the relayout geometry — no second generator, no manual YAML.
    /// </summary>
    public static class TownDistrictLayout
    {
        // ── Canonical materialized footprint (76×64 tiles) ─────────────────────────────
        // Kept stable by spec_city_preservation_first_coherent_relayout. Buildings are placed by
        // TownCityLayout lots; this class remains the bounds/district/stable-ID contract.
        public const float HalfWidth = 60f;
        public const float HalfHeight = 45f;
        public const float WidthTiles = HalfWidth * 2f;
        public const float HeightTiles = HalfHeight * 2f;

        // Legacy interior half-extents the existing element coordinates were authored against.
        // Used only to derive the relayout scale; not a runtime value.
        public const float LegacyHalfWidth = 18.5f;
        public const float LegacyHalfHeight = 15f;

        // Uniform similarity scale old→new. A similarity transform can never introduce an
        // overlap that did not already exist, so element count and relative neighborhoods are
        // preserved by construction (CA-1). 1.3 keeps the furthest legacy element
        // (|x|≈16.5 → 21.5 < 24; |y|≈12.8 → 16.6 < 21) inside the new playfield with margin.
        public const float RelayoutScale = 2f;

        /// <summary>
        /// Maps a legacy element position (authored for the 36×30 field) into the canonical
        /// 76×64 footprint. Deterministic and total: every element keeps its z and is clamped
        /// to stay just inside the perimeter wall so nothing lands on or past a border collider.
        /// </summary>
        public static Vector3 Reposition(Vector3 legacyPosition)
        {
            float x = Mathf.Clamp(legacyPosition.x * RelayoutScale, -(HalfWidth - 1.5f), HalfWidth - 1.5f);
            float y = Mathf.Clamp(legacyPosition.y * RelayoutScale, -(HalfHeight - 1.5f), HalfHeight - 1.5f);
            return new Vector3(x, y, legacyPosition.z);
        }

        /// <summary>Axis-aligned named district rectangle inside the bounds.</summary>
        public readonly struct District
        {
            public District(string id, Vector2 center, Vector2 size)
            {
                Id = id;
                Center = center;
                Size = size;
            }

            public string Id { get; }
            public Vector2 Center { get; }
            public Vector2 Size { get; }

            public float MinX => Center.x - Size.x * 0.5f;
            public float MaxX => Center.x + Size.x * 0.5f;
            public float MinY => Center.y - Size.y * 0.5f;
            public float MaxY => Center.y + Size.y * 0.5f;

            public bool Contains(Vector2 p) =>
                p.x >= MinX && p.x <= MaxX && p.y >= MinY && p.y <= MaxY;

            public bool WithinBounds() =>
                MinX >= -HalfWidth && MaxX <= HalfWidth &&
                MinY >= -HalfHeight && MaxY <= HalfHeight;
        }

        // ── Canonical district map (HUD_LAYOUT §3 / city_rules Rule 1) ────────────────
        // Seven breathing districts with an internal ring road. Centers/sizes chosen so each
        // sits inside the bounds and the two new districts (lake/park SW, town hall NE) occupy
        // the corner space the relayout scale frees up. Districts may touch but are laid out so
        // their cores do not overlap (validated by tests).
        public const string DistrictCentralPlaza = "central_plaza";
        public const string DistrictMarketWest = "market_west";
        public const string DistrictResidentialEast = "residential_east";
        public const string DistrictTempleNorth = "temple_fountain_north";
        public const string DistrictCorralSouth = "corral_south_gate";
        public const string DistrictTownHallNortheast = "town_hall_northeast";
        public const string DistrictLakeParkSouthwest = "lake_park_southwest";

        // Footprint 76×64: distritos reescalados. O placer de prédios (CreateMvpTownScene) usa zonas
        // reservadas próprias; estes retângulos servem para classificação de social anchors, os
        // landmarks de canto (lago SW, prefeitura NE) e os testes de WithinBounds/cores.
        private static readonly District[] Districts =
        {
            // Praça central (estátua + quadro público).
            new District(DistrictCentralPlaza, TownCityLayout.CentralPlazaCenter, TownCityLayout.CentralPlazaSize),
            // Mercado / comercial — oeste-centro.
            new District(DistrictMarketWest, new Vector2(-38f, 4f), new Vector2(34f, 30f)),
            // Residencial — leste.
            new District(DistrictResidentialEast, new Vector2(38f, 2f), new Vector2(34f, 40f)),
            // Templo / fonte — norte.
            new District(DistrictTempleNorth, new Vector2(-24f, 35f), new Vector2(68f, 16f)),
            // Curral / entrada sul (estrada para a fazenda).
            new District(DistrictCorralSouth, new Vector2(0f, -35f), new Vector2(88f, 14f)),
            // Prefeitura + mural — canto nordeste.
            new District(DistrictTownHallNortheast, new Vector2(12f, 42f), new Vector2(18f, 5.5f)),
            // Lago / parque — canto sudoeste.
            new District(DistrictLakeParkSouthwest, new Vector2(-45f, -14f), new Vector2(22f, 14f)),
        };

        public static IReadOnlyList<District> AllDistricts => Districts;

        public static bool TryGetDistrict(string id, out District district)
        {
            foreach (var d in Districts)
            {
                if (d.Id == id)
                {
                    district = d;
                    return true;
                }
            }

            district = default;
            return false;
        }

        // ── New-district landmark anchors (relayout-only) ─────────────────────────────
        // Lake/park water body + benches (SW); town hall building + mural wall (NE). These are
        // the elements city_rules Rule 1/Rule 8 say did not exist before fable_40.
        public static Vector3 LakeCenter => ToVec3(DistrictLakeParkSouthwest, 0f, 0f);
        public static Vector3 LakeBenchWest => ToVec3(DistrictLakeParkSouthwest, -7f, -6f);
        public static Vector3 LakeBenchEast => ToVec3(DistrictLakeParkSouthwest, 7f, -6f);
        public static Vector3 TownHallCenter => ToVec3(DistrictTownHallNortheast, 0f, 0f);
        // Mural lives on the town-hall south wall (the public board moves to the plaza per CA-3).
        public static Vector3 TownHallMural => ToVec3(DistrictTownHallNortheast, 0f, -2.1f);

        private static Vector3 ToVec3(string districtId, float offsetX, float offsetY)
        {
            TryGetDistrict(districtId, out var d);
            return new Vector3(d.Center.x + offsetX, d.Center.y + offsetY, 0f);
        }

        // ── Stable IDs (must not change across relayouts — CA-2 / CA-4) ───────────────
        // Spawn-point IDs consumed by SceneSpawnInstaller / portals; schedule-anchor suffixes
        // consumed by NpcScheduleAnchor (full id = npc_<id>_<suffix>). The relayout moves the
        // positions only — these strings are frozen.
        public static readonly string[] StableSpawnIds =
        {
            "town_default",
            "town_from_farm",
        };

        public static readonly string[] StableScheduleAnchorSuffixes =
        {
            "work",
            "social",
            "home",
        };
    }
}
