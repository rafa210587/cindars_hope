using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Editor.SceneCreation
{
    /// <summary>
    /// fable_40 — Data-driven 48×42 town layout (canonical footprint Q12.1 / HUD_LAYOUT §3 /
    /// city_rules Rule 1-2). Pure C# (no UnityEditor / scene side effects) so the layout can be
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
        // ── Canonical footprint (48×42 tiles) ─────────────────────────────────────────
        // Bounds −24..24 / −21..21 (city_rules Rule 1). The legacy build was 36×30
        // (x = ±18.5, y = ±15) — see CreateMvpTownScene history and city_rules number table.
        // Footprint ampliado para 76×64 (spec_city_real_walkin_houses_no_teleport): cada residência é
        // um prédio FÍSICO percorrível e os marcos cívicos (igreja, câmara, mansão, mercado, praça de
        // eventos, cemitério) ganham espaço próprio. O placer determinístico em CreateMvpTownScene
        // distribui os 21 prédios sem sobreposição dentro deste footprint.
        public const float HalfWidth = 38f;   // x ∈ [−38, 38] → 76 tiles wide
        public const float HalfHeight = 32f;  // y ∈ [−32, 32] → 64 tiles tall
        public const float WidthTiles = HalfWidth * 2f;   // 56
        public const float HeightTiles = HalfHeight * 2f; // 48

        // Legacy interior half-extents the existing element coordinates were authored against.
        // Used only to derive the relayout scale; not a runtime value.
        public const float LegacyHalfWidth = 18.5f;
        public const float LegacyHalfHeight = 15f;

        // Uniform similarity scale old→new. A similarity transform can never introduce an
        // overlap that did not already exist, so element count and relative neighborhoods are
        // preserved by construction (CA-1). 1.3 keeps the furthest legacy element
        // (|x|≈16.5 → 21.5 < 24; |y|≈12.8 → 16.6 < 21) inside the new playfield with margin.
        public const float RelayoutScale = 1.3f;

        /// <summary>
        /// Maps a legacy element position (authored for the 36×30 field) into the canonical
        /// 48×42 footprint. Deterministic and total: every element keeps its z and is clamped
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
            new District(DistrictCentralPlaza, new Vector2(0f, 0f), new Vector2(14f, 14f)),
            // Mercado / comercial — oeste-centro.
            new District(DistrictMarketWest, new Vector2(-22f, 4f), new Vector2(16f, 22f)),
            // Residencial — leste.
            new District(DistrictResidentialEast, new Vector2(22f, 4f), new Vector2(16f, 22f)),
            // Templo / fonte — norte.
            new District(DistrictTempleNorth, new Vector2(0f, 23f), new Vector2(26f, 12f)),
            // Curral / entrada sul (estrada para a fazenda).
            new District(DistrictCorralSouth, new Vector2(0f, -24f), new Vector2(28f, 10f)),
            // Prefeitura + mural — canto nordeste.
            new District(DistrictTownHallNortheast, new Vector2(30f, 26f), new Vector2(12f, 9f)),
            // Lago / parque — canto sudoeste.
            new District(DistrictLakeParkSouthwest, new Vector2(-30f, -25f), new Vector2(12f, 10f)),
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
        public static Vector3 LakeCenter => ToVec3(DistrictLakeParkSouthwest, 0f, 0.5f);
        public static Vector3 LakeBenchWest => ToVec3(DistrictLakeParkSouthwest, -3f, -3f);
        public static Vector3 LakeBenchEast => ToVec3(DistrictLakeParkSouthwest, 3f, -3f);
        public static Vector3 TownHallCenter => ToVec3(DistrictTownHallNortheast, 0f, 0.5f);
        // Mural lives on the town-hall south wall (the public board moves to the plaza per CA-3).
        public static Vector3 TownHallMural => ToVec3(DistrictTownHallNortheast, 0f, -2.6f);

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
