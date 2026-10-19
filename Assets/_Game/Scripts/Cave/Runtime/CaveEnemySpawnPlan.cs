using System;
using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    [Serializable]
    public sealed class CaveEnemySpawnPlan
    {
        public int CaveLevel;
        public string BiomeId = string.Empty;
        public string CaveWorldSeed = string.Empty;
        public string CaveRunSeed = string.Empty;
        public string LevelSeed = string.Empty;
        public string LayoutHash = string.Empty;
        public List<CaveEnemySpawnPlanEntry> Entries = new List<CaveEnemySpawnPlanEntry>();
        public List<string> Warnings = new List<string>();

        public bool IsValid => CaveLevel > 0 && Entries != null && Entries.Count > 0;
    }

    [Serializable]
    public sealed class CaveEnemySpawnPlanEntry
    {
        public string EnemyInstanceId = string.Empty;
        public string EnemyId = string.Empty;
        public string SpawnProfileId = string.Empty;
        public string PackId = string.Empty;
        public Vector2Int GridPosition;
        public Vector3 WorldPosition;
        public string RoomId = string.Empty;
        public int SpawnIndex;
        public bool IsElite;
        public string SizeClass = string.Empty;
        public string FactionId = string.Empty;

        // fable_24: named-elite affix decided deterministically per slot by the planner
        // (StableHash, 8% from level 6+). EliteAffix.None when the slot is not a named elite.
        // EliteDisplayName carries the prefixed name for the floating label (e.g. "Frenzied Wisp").
        public Enemy.EliteAffix EliteAffix = Enemy.EliteAffix.None;
        public string EliteDisplayName = string.Empty;
    }
}
