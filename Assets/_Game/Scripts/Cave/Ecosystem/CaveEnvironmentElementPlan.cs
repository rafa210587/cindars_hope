using System.Collections.Generic;

namespace CindarsHope.Cave.Ecosystem
{
    /// <summary>
    /// fable_78 — plano DETERMINÍSTICO de elementos ambientais de um nível (resultado puro do
    /// <see cref="CaveEnvironmentElementPlanner"/>). Expõe apenas leitura; a lista interna não é mutável
    /// pelo caller. Revisitar o mesmo (worldSeed, runSeed, caveLevel) reproduz exatamente o mesmo plano.
    /// </summary>
    public sealed class CaveEnvironmentElementPlan
    {
        private readonly List<CaveEnvironmentElementPlacement> _placements;

        public int CaveLevel { get; }
        public string BiomeId { get; }
        public bool HasWater { get; }
        public IReadOnlyList<CaveEnvironmentElementPlacement> Placements => _placements;

        public CaveEnvironmentElementPlan(
            int caveLevel,
            string biomeId,
            bool hasWater,
            List<CaveEnvironmentElementPlacement> placements)
        {
            CaveLevel = caveLevel;
            BiomeId = biomeId ?? string.Empty;
            HasWater = hasWater;
            _placements = placements ?? new List<CaveEnvironmentElementPlacement>();
        }

        public static CaveEnvironmentElementPlan Empty(int caveLevel, string biomeId)
        {
            return new CaveEnvironmentElementPlan(caveLevel, biomeId, false, new List<CaveEnvironmentElementPlacement>());
        }
    }
}
