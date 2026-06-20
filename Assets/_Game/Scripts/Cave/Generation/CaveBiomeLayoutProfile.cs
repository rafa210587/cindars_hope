using System;
using UnityEngine;

namespace CindarsHope.Cave.Generation
{
    /// <summary>
    /// fable_09 — perfil ESTÁTICO de layout por banda de bioma (7 bandas), parametrizando a geração
    /// existente (CaveProceduralGenerator) sem reescrever o algoritmo. Cada banda tem identidade
    /// estrutural própria (contagem/tamanho de salas, largura de corredor) + parâmetros de
    /// hazards e sala de tesouro consumidos pelo materializer.
    ///
    /// Determinismo (ADR-0005 / cave-stable-run): o perfil é função PURA de caveLevel (a banda).
    /// O TAMANHO do mapa oscila por nível via StableHash(worldSeed|runSeed|level|"size") — a mesma
    /// run reproduz o mesmo tamanho em toda revisita; runs novas variam. Nada de GUID/timestamp.
    ///
    /// Bandas (idênticas ao switch canônico em CaveEnemySpawnPlanner.BuildBiomeTags):
    /// stone 1-10, fungal 11-25, ice 26-40, fire 41-55, ruins 56-70, deep 71-85, void 86-101+.
    /// </summary>
    public sealed class CaveBiomeLayoutProfile
    {
        // EMENDA 2026-06-12 (Q12.1): base 55×55; ~20% 42×42; ~20% 65×65; oscilação determinística.
        public const int BaseMapSize = 55;
        public const int SmallMapSize = 42;
        public const int LargeMapSize = 65;
        // Densidade re-escalada por área: count_final = count_base × (área / BaseArea).
        public const float BaseArea = BaseMapSize * BaseMapSize; // 3025

        public string BandId { get; }
        public int MinLevel { get; }
        public int MaxLevel { get; }

        /// <summary>Contagem-base de salas (na área base 55×55). Re-escalada por área no tamanho real.</summary>
        public int RoomCountBaseMin { get; }
        public int RoomCountBaseMax { get; }

        public int RoomMinWidth { get; }
        public int RoomMaxWidth { get; }
        public int RoomMinHeight { get; }
        public int RoomMaxHeight { get; }

        public int CorridorMinWidth { get; }
        public int CorridorMaxWidth { get; }

        public int ExtraConnectionChancePercent { get; }

        /// <summary>Hazards por nível: 0..MaxHazards, decidido por hash (CA-2).</summary>
        public int MaxHazards { get; }

        /// <summary>Tipos de hazard permitidos nesta banda (placeholder visual por banda).</summary>
        public CaveHazardKind[] AllowedHazards { get; }

        /// <summary>Chance da sala de tesouro por nível (~15% global; CA-3 mede 10-20% em 200 níveis).</summary>
        public int TreasureRoomChancePercent { get; }

        private CaveBiomeLayoutProfile(
            string bandId,
            int minLevel,
            int maxLevel,
            int roomCountBaseMin,
            int roomCountBaseMax,
            int roomMinWidth,
            int roomMaxWidth,
            int roomMinHeight,
            int roomMaxHeight,
            int corridorMinWidth,
            int corridorMaxWidth,
            int extraConnectionChancePercent,
            int maxHazards,
            CaveHazardKind[] allowedHazards,
            int treasureRoomChancePercent)
        {
            BandId = bandId;
            MinLevel = minLevel;
            MaxLevel = maxLevel;
            RoomCountBaseMin = roomCountBaseMin;
            RoomCountBaseMax = roomCountBaseMax;
            RoomMinWidth = roomMinWidth;
            RoomMaxWidth = roomMaxWidth;
            RoomMinHeight = roomMinHeight;
            RoomMaxHeight = roomMaxHeight;
            CorridorMinWidth = corridorMinWidth;
            CorridorMaxWidth = corridorMaxWidth;
            ExtraConnectionChancePercent = extraConnectionChancePercent;
            MaxHazards = maxHazards;
            AllowedHazards = allowedHazards ?? Array.Empty<CaveHazardKind>();
            TreasureRoomChancePercent = treasureRoomChancePercent;
        }

        // --- 7 perfis canônicos (estáticos em código, sem assets) ---------------------------------

        // stone 1-10: cavernas equilibradas, base do contrato (rollback = stone para tudo).
        public static readonly CaveBiomeLayoutProfile Stone = new CaveBiomeLayoutProfile(
            "stone", 1, 10,
            roomCountBaseMin: 8, roomCountBaseMax: 12,
            roomMinWidth: 10, roomMaxWidth: 20, roomMinHeight: 8, roomMaxHeight: 16,
            corridorMinWidth: 2, corridorMaxWidth: 3,
            extraConnectionChancePercent: 18,
            maxHazards: 1,
            allowedHazards: new[] { CaveHazardKind.FallingRock },
            treasureRoomChancePercent: 15);

        // fungal 11-25: labirinto apertado — MAIS salas, MENORES, corredor fino, hazard de poça tóxica.
        public static readonly CaveBiomeLayoutProfile Fungal = new CaveBiomeLayoutProfile(
            "fungal", 11, 25,
            roomCountBaseMin: 12, roomCountBaseMax: 18,
            roomMinWidth: 7, roomMaxWidth: 13, roomMinHeight: 6, roomMaxHeight: 11,
            corridorMinWidth: 1, corridorMaxWidth: 2,
            extraConnectionChancePercent: 28,
            maxHazards: 2,
            allowedHazards: new[] { CaveHazardKind.ToxicPool, CaveHazardKind.FallingRock },
            treasureRoomChancePercent: 15);

        // ice 26-40: cavernas abertas — POUCAS salas, GRANDES, gelo escorregadio.
        public static readonly CaveBiomeLayoutProfile Ice = new CaveBiomeLayoutProfile(
            "ice", 26, 40,
            roomCountBaseMin: 6, roomCountBaseMax: 9,
            roomMinWidth: 14, roomMaxWidth: 26, roomMinHeight: 12, roomMaxHeight: 22,
            corridorMinWidth: 3, corridorMaxWidth: 4,
            extraConnectionChancePercent: 14,
            maxHazards: 2,
            allowedHazards: new[] { CaveHazardKind.IceSlick, CaveHazardKind.FallingRock },
            treasureRoomChancePercent: 15);

        // fire 41-55: corredores quentes, salas médias, estalactite + poça (lava placeholder via FallingRock/ToxicPool).
        public static readonly CaveBiomeLayoutProfile Fire = new CaveBiomeLayoutProfile(
            "fire", 41, 55,
            roomCountBaseMin: 9, roomCountBaseMax: 14,
            roomMinWidth: 10, roomMaxWidth: 18, roomMinHeight: 9, roomMaxHeight: 15,
            corridorMinWidth: 2, corridorMaxWidth: 3,
            extraConnectionChancePercent: 20,
            maxHazards: 2,
            allowedHazards: new[] { CaveHazardKind.FallingRock, CaveHazardKind.ToxicPool },
            treasureRoomChancePercent: 16);

        // ruins 56-70: salas GRANDES (galerias), corredores largos, mais tesouro.
        public static readonly CaveBiomeLayoutProfile Ruins = new CaveBiomeLayoutProfile(
            "ruins", 56, 70,
            roomCountBaseMin: 7, roomCountBaseMax: 11,
            roomMinWidth: 16, roomMaxWidth: 28, roomMinHeight: 12, roomMaxHeight: 20,
            corridorMinWidth: 3, corridorMaxWidth: 4,
            extraConnectionChancePercent: 22,
            maxHazards: 2,
            allowedHazards: new[] { CaveHazardKind.FallingRock, CaveHazardKind.ToxicPool },
            treasureRoomChancePercent: 18);

        // deep 71-85: denso e claustrofóbico, todos os hazards.
        public static readonly CaveBiomeLayoutProfile Deep = new CaveBiomeLayoutProfile(
            "deep", 71, 85,
            roomCountBaseMin: 11, roomCountBaseMax: 16,
            roomMinWidth: 9, roomMaxWidth: 16, roomMinHeight: 8, roomMaxHeight: 14,
            corridorMinWidth: 2, corridorMaxWidth: 3,
            extraConnectionChancePercent: 26,
            maxHazards: 2,
            allowedHazards: new[] { CaveHazardKind.ToxicPool, CaveHazardKind.IceSlick, CaveHazardKind.FallingRock },
            treasureRoomChancePercent: 17);

        // void 86-101+: irregular, salas médias dispersas, todos os hazards, tesouro raro mas valioso.
        public static readonly CaveBiomeLayoutProfile Void = new CaveBiomeLayoutProfile(
            "void", 86, int.MaxValue,
            roomCountBaseMin: 9, roomCountBaseMax: 14,
            roomMinWidth: 11, roomMaxWidth: 20, roomMinHeight: 10, roomMaxHeight: 18,
            corridorMinWidth: 2, corridorMaxWidth: 3,
            extraConnectionChancePercent: 24,
            maxHazards: 2,
            allowedHazards: new[] { CaveHazardKind.ToxicPool, CaveHazardKind.IceSlick, CaveHazardKind.FallingRock },
            treasureRoomChancePercent: 16);

        private static readonly CaveBiomeLayoutProfile[] All =
        {
            Stone, Fungal, Ice, Fire, Ruins, Deep, Void
        };

        /// <summary>
        /// Perfil da banda do nível. Mesmo switch canônico de CaveEnemySpawnPlanner.BuildBiomeTags.
        /// Nunca retorna null (void cobre 86+).
        /// </summary>
        public static CaveBiomeLayoutProfile ForLevel(int caveLevel)
        {
            var level = Mathf.Max(1, caveLevel);
            for (var i = 0; i < All.Length; i++)
            {
                if (level >= All[i].MinLevel && level <= All[i].MaxLevel)
                {
                    return All[i];
                }
            }

            return Void;
        }

        /// <summary>
        /// Lado do mapa (quadrado) DETERMINÍSTICO para este nível/run. EMENDA Q12.1:
        /// ~20% 42×42, ~20% 65×65, ~60% 55×55. Mesma run/level → mesmo tamanho (cave-stable-run).
        /// </summary>
        public static int ResolveMapSize(string caveWorldSeed, string caveRunSeed, int caveLevel)
        {
            var hash = CaveLayoutStableHash.Compute($"{caveWorldSeed}|{caveRunSeed}|{Mathf.Max(1, caveLevel)}|size");
            var bucket = Mathf.Abs(hash % 100);
            if (bucket < 20)
            {
                return SmallMapSize;
            }

            if (bucket < 40)
            {
                return LargeMapSize;
            }

            return BaseMapSize;
        }

        /// <summary>
        /// Contagem de salas re-escalada pela área real (EMENDA: count_final = count_base × área/BaseArea),
        /// preservando min ≤ max e pelo menos 3 salas.
        /// </summary>
        public void ResolveScaledRoomCount(int mapWidth, int mapHeight, out int minRooms, out int maxRooms)
        {
            var areaFactor = Mathf.Max(0.4f, (mapWidth * (float)mapHeight) / BaseArea);
            minRooms = Mathf.Max(3, Mathf.RoundToInt(RoomCountBaseMin * areaFactor));
            maxRooms = Mathf.Max(minRooms, Mathf.RoundToInt(RoomCountBaseMax * areaFactor));
        }
    }
}
