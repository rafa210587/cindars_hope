using System.Collections.Generic;
using CindarsHope.Cave.Ecosystem;
using CindarsHope.Cave.Generation;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CindarsHope.Cave.Art
{
    /// <summary>
    /// spec_cave_biome_art_profiles_runtime (CV01) — resolve "banda -> profile de arte -> tile/sprite"
    /// numa chamada. C# puro (sem MonoBehaviour), 100% testável em EditMode. TODO TryGet* é null-safe:
    /// profile ausente/campo ausente = false, e o chamador mantém o placeholder atual (fallback-first,
    /// critério 14.2 da spec).
    ///
    /// Variação determinística de floor tile usa EXCLUSIVAMENTE
    /// CaveLayoutStableHash.Compute("worldSeed|runSeed|level|x|y") — nunca System.Random ou
    /// UnityEngine.Random (cave-stable-run / ADR-0005). Revisitar o mesmo nível produz o mesmo índice.
    /// </summary>
    public sealed class CaveBiomeArtResolver
    {
        // Chance determinística (não aleatória) de o detail tile aparecer numa célula: 1 em N por
        // hash, não RNG. Mantém baixa densidade de detalhe sem quebrar o stable-run.
        private const int FloorDetailHashModulo = 8;

        private readonly Dictionary<int, CaveBiomeArtProfileSO> _profilesByBand;

        public CaveBiomeArtResolver(IEnumerable<CaveBiomeArtProfileSO> profiles)
        {
            _profilesByBand = new Dictionary<int, CaveBiomeArtProfileSO>();
            if (profiles == null)
            {
                return;
            }

            foreach (var profile in profiles)
            {
                if (profile == null)
                {
                    continue;
                }

                // Primeira entrada vence em caso de banda duplicada (defensivo; gerador garante 1:1).
                if (!_profilesByBand.ContainsKey(profile.BandId))
                {
                    _profilesByBand[profile.BandId] = profile;
                }
            }
        }

        /// <summary>Hash determinístico e estável de uma célula da run (nunca Random/GetHashCode).</summary>
        public static long ComputeCellHash(string worldSeed, string runSeed, int caveLevel, int x, int y)
        {
            var key = $"{worldSeed}|{runSeed}|{caveLevel}|{x}|{y}";
            // FNV-1a 32-bit (CaveLayoutStableHash) pode retornar negativo; normaliza para uso em módulo.
            return (long)CaveLayoutStableHash.Compute(key) & 0x7FFFFFFFL;
        }

        public bool TryGetProfile(int bandId, out CaveBiomeArtProfileSO profile)
        {
            return _profilesByBand.TryGetValue(bandId, out profile) && profile != null;
        }

        public bool TryGetFloorTile(int bandId, long cellHash, out TileBase tile)
        {
            tile = null;
            if (!TryGetProfile(bandId, out var profile))
            {
                return false;
            }

            var floorTiles = profile.FloorTiles;
            if (floorTiles.Length == 0)
            {
                return false;
            }

            // Detail tile determinístico: raro (1/FloorDetailHashModulo) e só quando o profile o fornece.
            if (profile.FloorDetailTile != null && cellHash % FloorDetailHashModulo == 0)
            {
                tile = profile.FloorDetailTile;
                return true;
            }

            var index = (int)(cellHash % floorTiles.Length);
            tile = floorTiles[index];
            return tile != null;
        }

        public bool TryGetWallTiles(int bandId, out TileBase wallFaceTile, out TileBase wallTopTile)
        {
            wallFaceTile = null;
            wallTopTile = null;
            if (!TryGetProfile(bandId, out var profile))
            {
                return false;
            }

            wallFaceTile = profile.WallFaceTile;
            wallTopTile = profile.WallTopTile;
            return wallFaceTile != null || wallTopTile != null;
        }

        public bool TryGetTrapSprite(int bandId, string trapId, out Sprite sprite)
        {
            sprite = null;
            if (string.IsNullOrWhiteSpace(trapId) || !TryGetProfile(bandId, out var profile))
            {
                return false;
            }

            foreach (var entry in profile.TrapSprites)
            {
                if (entry.TrapId == trapId && entry.Sprite != null)
                {
                    sprite = entry.Sprite;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetHazardSprite(int bandId, CaveHazardKind kind, out Sprite sprite)
        {
            sprite = null;
            if (!TryGetProfile(bandId, out var profile))
            {
                return false;
            }

            sprite = kind switch
            {
                CaveHazardKind.ToxicPool => profile.ToxicPoolSprite,
                CaveHazardKind.IceSlick => profile.IceSlickSprite,
                CaveHazardKind.FallingRock => profile.FallingRockSprite,
                _ => null
            };

            return sprite != null;
        }

        public bool TryGetChestSprite(int bandId, CaveChestVisualState state, out Sprite sprite)
        {
            sprite = null;
            if (!TryGetProfile(bandId, out var profile))
            {
                return false;
            }

            sprite = state switch
            {
                CaveChestVisualState.Closed => profile.ChestClosedSprite,
                CaveChestVisualState.Open => profile.ChestOpenSprite,
                CaveChestVisualState.FalseChestRevealed => profile.FalseChestRevealedSprite,
                _ => null
            };

            return sprite != null;
        }

        public bool TryGetExitSprite(int bandId, bool isForwardExit, out Sprite sprite)
        {
            sprite = null;
            if (!TryGetProfile(bandId, out var profile))
            {
                return false;
            }

            sprite = isForwardExit ? profile.ExitDownSprite : profile.ExitUpSprite;
            return sprite != null;
        }

        /// <summary>spec_cave_decor_placement_runtime (CV02) — sprite real de decor ambiental
        /// (fable_78) para um Kind (DecorNonBlocking/DecorBlocking), escolhido deterministicamente
        /// pelo hash estável da posição do elemento (nunca Random/GetHashCode). Kinds que não são
        /// decor (WaterTile/MineableNode) sempre retornam false — fora de escopo desta spec (o
        /// materializer já tem caminho próprio para eles). Null-safe: profile ausente ou pool vazio
        /// = false, e o chamador mantém o fallback atual (prefab/builtin).</summary>
        public bool TryGetDecorSprite(int bandId, CaveEnvironmentElementKind kind, long stableHash, out Sprite sprite)
        {
            sprite = null;
            if (!TryGetProfile(bandId, out var profile))
            {
                return false;
            }

            IReadOnlyList<Sprite> pool = kind switch
            {
                CaveEnvironmentElementKind.DecorNonBlocking => profile.DecorNonBlockingSprites,
                CaveEnvironmentElementKind.DecorBlocking => profile.DecorBlockingSprites,
                _ => null
            };

            if (pool == null || pool.Count == 0)
            {
                return false;
            }

            var normalizedHash = stableHash & 0x7FFFFFFFL;
            var index = (int)(normalizedHash % pool.Count);
            sprite = pool[index];
            return sprite != null;
        }
    }

    /// <summary>Estado visual do baú de tesouro/baú falso para resolução de sprite (CV01).</summary>
    public enum CaveChestVisualState
    {
        Closed = 0,
        Open = 1,
        FalseChestRevealed = 2
    }
}
