using System.Collections.Generic;
using CindarsHope.Cave.Generation;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    /// <summary>
    /// fable_09 — placement determinístico de um hazard de tile (CA-2). Tudo derivado por
    /// StableHash(worldSeed|runSeed|level|salt); nunca em entrance/exit, no caminho entrance↔exit,
    /// nem perto do spawn do player.
    /// </summary>
    public sealed class CaveHazardPlacement
    {
        public string HazardId = string.Empty;
        public Vector2Int GridPosition;
        public CaveHazardKind Kind;
    }

    /// <summary>
    /// fable_09 — placement determinístico de uma sala de tesouro (CA-3). Posição do baú +
    /// âncoras dos guardiões realocados do plano de spawn. ChestId é estável por run/level/posição.
    /// </summary>
    public sealed class CaveTreasureRoomPlacement
    {
        public string ChestId = string.Empty;
        public Vector2Int ChestGridPosition;
        public int LootSeed;
        public List<Vector2Int> GuardianGridPositions = new List<Vector2Int>();
    }

    /// <summary>
    /// fable_09 — resultado do planejamento determinístico de hazards e tesouro para um nível.
    /// </summary>
    public sealed class CaveHazardPlan
    {
        public List<CaveHazardPlacement> Hazards = new List<CaveHazardPlacement>();
        public CaveTreasureRoomPlacement TreasureRoom; // null quando o nível não sorteia tesouro
        public bool HasTreasureRoom => TreasureRoom != null;
    }
}
