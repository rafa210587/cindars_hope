using CindarsHope.Cave.Data;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    public class CaveBiomeResolver
    {
        private readonly CaveBiomeRegistrySO _biomeRegistry;

        public CaveBiomeResolver(CaveBiomeRegistrySO biomeRegistry)
        {
            _biomeRegistry = biomeRegistry;
        }

        public CaveBiomeDataSO ResolveBiome(int caveLevel)
        {
            if (_biomeRegistry == null)
            {
                Debug.LogError("CaveBiomeResolver: BiomeRegistry not initialized");
                return null;
            }

            return _biomeRegistry.GetBiomeByLevel(caveLevel);
        }

        public bool IsBiomeForLevel(string biomeId, int caveLevel)
        {
            var biome = _biomeRegistry?.GetBiomeByLevel(caveLevel);
            return biome != null && biome.Id == biomeId;
        }
    }
}
