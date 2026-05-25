using System.Collections.Generic;
using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Cave.Data
{
    [CreateAssetMenu(fileName = "CaveBiomeRegistry", menuName = "CindarsHope/Cave/Biome Registry")]
    public sealed class CaveBiomeRegistrySO : DataRegistrySO<CaveBiomeDataSO>
    {
        private static readonly BiomeDefinition[] DefaultBiomes = new[]
        {
            new BiomeDefinition { Id = "biome_stone_cavern", DisplayName = "Caverna de Pedra", MinLevel = 1, MaxLevel = 10 },
            new BiomeDefinition { Id = "biome_forest", DisplayName = "Floresta Subterrânea", MinLevel = 11, MaxLevel = 25 },
            new BiomeDefinition { Id = "biome_ice", DisplayName = "Caverna de Gelo", MinLevel = 26, MaxLevel = 40 },
            new BiomeDefinition { Id = "biome_fire", DisplayName = "Caverna de Fogo", MinLevel = 41, MaxLevel = 55 },
            new BiomeDefinition { Id = "biome_ruins", DisplayName = "Ruínas Antigas", MinLevel = 56, MaxLevel = 70 },
            new BiomeDefinition { Id = "biome_abyss", DisplayName = "Abismo Sombrio", MinLevel = 71, MaxLevel = 85 },
            new BiomeDefinition { Id = "biome_core", DisplayName = "Núcleo Corrompido", MinLevel = 86, MaxLevel = 99 },
            new BiomeDefinition { Id = "biome_final", DisplayName = "Boss Final", MinLevel = 100, MaxLevel = 100 },
        };

        private Dictionary<int, CaveBiomeDataSO> _biomesByLevel = new Dictionary<int, CaveBiomeDataSO>();
        private bool _runtimeFallbackLogged;

        public CaveBiomeDataSO GetBiomeByLevel(int caveLevel)
        {
            if (_biomesByLevel.TryGetValue(caveLevel, out var biome) && biome != null)
            {
                return biome;
            }

            foreach (var item in All)
            {
                if (item != null && item.MinLevel <= caveLevel && caveLevel <= item.MaxLevel)
                {
                    _biomesByLevel[caveLevel] = item;
                    return item;
                }
            }

            return GetRuntimeDefaultBiome(caveLevel);
        }

        public CaveBiomeDataSO GetBiomeById(string biomeId)
        {
            if (TryGetById(biomeId, out var biome))
            {
                return biome;
            }
            return null;
        }

        private CaveBiomeDataSO GetRuntimeDefaultBiome(int caveLevel)
        {
            foreach (var defaultBiome in DefaultBiomes)
            {
                if (defaultBiome.MinLevel <= caveLevel && caveLevel <= defaultBiome.MaxLevel)
                {
                    var biome = CreateInstance<CaveBiomeDataSO>();
                    biome.name = $"RuntimeDefaultBiome_{defaultBiome.Id}";
                    biome.Id = defaultBiome.Id;
                    biome.DisplayName = defaultBiome.DisplayName;
                    biome.MinLevel = defaultBiome.MinLevel;
                    biome.MaxLevel = defaultBiome.MaxLevel;

                    _biomesByLevel[caveLevel] = biome;

                    if (!_runtimeFallbackLogged)
                    {
                        _runtimeFallbackLogged = true;
                        Debug.LogWarning(
                            "CaveBiomeRegistrySO: using runtime fallback biome set. Regenerate CaveScene or populate CaveBiomeRegistry with CaveBiomeDataSO assets to remove this warning.",
                            this);
                    }

                    return biome;
                }
            }

            Debug.LogError($"CaveBiomeRegistrySO: No biome found for level {caveLevel}.", this);
            return null;
        }

        private struct BiomeDefinition
        {
            public string Id;
            public string DisplayName;
            public int MinLevel;
            public int MaxLevel;
        }
    }
}
