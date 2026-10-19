using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Cave.Data
{
    /// <summary>
    /// fable_78 — registry dos perfis de elementos ambientais por bioma/banda. Reusa o padrão
    /// DataRegistrySO (indexação por Id estável, All, TryGetById) e adiciona lookup por banda/biome,
    /// como ResourceNodeDatabaseSO faz para nós. Sem RNG/segundo registry paralelo.
    /// </summary>
    [CreateAssetMenu(fileName = "CaveEnvironmentElementDatabase", menuName = "CindarsHope/Cave/Environment Element Database")]
    public sealed class CaveEnvironmentElementDatabaseSO : DataRegistrySO<CaveEnvironmentElementProfileSO>
    {
        /// <summary>Primeiro perfil cuja banda corresponde a <paramref name="band"/> (0..6).</summary>
        public bool TryGetByBand(int band, out CaveEnvironmentElementProfileSO profile)
        {
            foreach (var candidate in All)
            {
                if (candidate != null && candidate.Band == band)
                {
                    profile = candidate;
                    return true;
                }
            }

            profile = null;
            return false;
        }

        /// <summary>Primeiro perfil cujo BiomeId corresponde a <paramref name="biomeId"/>.</summary>
        public bool TryGetByBiome(string biomeId, out CaveEnvironmentElementProfileSO profile)
        {
            if (!string.IsNullOrWhiteSpace(biomeId))
            {
                foreach (var candidate in All)
                {
                    if (candidate != null && candidate.BiomeId == biomeId)
                    {
                        profile = candidate;
                        return true;
                    }
                }
            }

            profile = null;
            return false;
        }
    }
}
