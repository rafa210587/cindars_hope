using System.Collections.Generic;

namespace CindarsHope.Cave.Ecosystem
{
    /// <summary>
    /// fable_78 — plano de conflito inter-monstro de UMA visita ao nível (resultado puro do
    /// <see cref="CaveEcosystemConflictPlanner"/>). Conflito é COMPORTAMENTO por visita, não composição
    /// (ADR-0018): não altera quais inimigos existem, contagem, posições ou IDs. Quando inativo,
    /// nenhum lado é populado. As listas de instâncias por lado podem ficar vazias nesta slice — o
    /// preenchimento por instância acontece na materialização (fase posterior).
    /// </summary>
    public sealed class CaveEcosystemConflictPlan
    {
        private readonly List<string> _factionAInstanceIds;
        private readonly List<string> _factionBInstanceIds;

        public bool ConflictActive { get; }
        public string FactionAEnemyId { get; }
        public string FactionBEnemyId { get; }
        public IReadOnlyList<string> FactionAInstanceIds => _factionAInstanceIds;
        public IReadOnlyList<string> FactionBInstanceIds => _factionBInstanceIds;

        private CaveEcosystemConflictPlan(
            bool conflictActive,
            string factionAEnemyId,
            string factionBEnemyId,
            List<string> factionAInstanceIds,
            List<string> factionBInstanceIds)
        {
            ConflictActive = conflictActive;
            FactionAEnemyId = factionAEnemyId ?? string.Empty;
            FactionBEnemyId = factionBEnemyId ?? string.Empty;
            _factionAInstanceIds = factionAInstanceIds ?? new List<string>();
            _factionBInstanceIds = factionBInstanceIds ?? new List<string>();
        }

        public static CaveEcosystemConflictPlan Inactive()
        {
            return new CaveEcosystemConflictPlan(false, string.Empty, string.Empty, null, null);
        }

        public static CaveEcosystemConflictPlan Active(
            string factionAEnemyId,
            string factionBEnemyId,
            List<string> factionAInstanceIds = null,
            List<string> factionBInstanceIds = null)
        {
            return new CaveEcosystemConflictPlan(
                true,
                factionAEnemyId,
                factionBEnemyId,
                factionAInstanceIds,
                factionBInstanceIds);
        }
    }
}
