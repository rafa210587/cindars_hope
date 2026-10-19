using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Cave.Traps
{
    /// <summary>
    /// fable_60 — placement determinístico de UMA armadilha (CA-1). Tudo derivado por
    /// StableHash(worldSeed|runSeed|level|"traps"|...); nunca em entrance/exit, no caminho
    /// entrance↔exit, nem perto do spawn do player. <see cref="TrapInstanceId"/> é estável (hash
    /// posicional, sem GUID/timestamp) para a revisita reproduzir o mesmo id (cave-stable-run).
    /// </summary>
    public sealed class CaveTrapPlacement
    {
        public string TrapInstanceId = string.Empty;
        public TrapId TrapId;
        public Vector2Int Cell;

        /// <summary>Banda 1..7 do nível (stone=1 … void=7), usada para o dano por tier.</summary>
        public int Band;
    }

    /// <summary>
    /// fable_60 — resultado do planejamento determinístico de armadilhas de um nível. Plano vazio =
    /// nível sem armadilhas (rollback inofensivo). Re-derivado idêntico na revisita (ADR-0005).
    /// </summary>
    public sealed class CaveTrapPlan
    {
        public List<CaveTrapPlacement> Traps = new List<CaveTrapPlacement>();
        public bool HasTraps => Traps.Count > 0;
    }
}
