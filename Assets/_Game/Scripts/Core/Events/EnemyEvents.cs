using UnityEngine;
using CindarsHope.Foundation;

namespace CindarsHope.Core.Events
{
    public class EnemySpawnedEvent
    {
        public string EnemyId;
        public Vector2 Position;
        public string EnemyInstanceId;
        public int CaveLevel;
        public EnemySpawnedEvent(string enemyId, Vector2 position, string enemyInstanceId = "", int caveLevel = 0)
        {
            EnemyId = enemyId;
            Position = position;
            EnemyInstanceId = enemyInstanceId ?? string.Empty;
            CaveLevel = caveLevel;
        }
    }

    public class EnemySeenEvent
    {
        public string EnemyId;
        public Vector2 Position;
        public string EnemyInstanceId;
        public int CaveLevel;
        public EnemySeenEvent(string enemyId, Vector2 position, string enemyInstanceId = "", int caveLevel = 0)
        {
            EnemyId = enemyId;
            Position = position;
            EnemyInstanceId = enemyInstanceId ?? string.Empty;
            CaveLevel = caveLevel;
        }
    }

    public class EnemyDamagedEvent
    {
        public string EnemyId;
        public int DamageAmount;
        public string DamageType;
        public EnemyDamagedEvent(string enemyId, int damage, string damageType = "physical")
        {
            EnemyId = enemyId;
            DamageAmount = damage;
            DamageType = damageType;
        }
    }

    public class EnemyActionStartedEvent
    {
        public string EnemyId;
        public string ActionId;
        public EnemyActionStartedEvent(string enemyId, string actionId)
        {
            EnemyId = enemyId;
            ActionId = actionId;
        }
    }

    public class EnemyActionResolvedEvent
    {
        public string EnemyId;
        public string ActionId;
        public EnemyActionResolvedEvent(string enemyId, string actionId)
        {
            EnemyId = enemyId;
            ActionId = actionId;
        }
    }

    public class EnemyTelegraphStartedEvent
    {
        public string EnemyId;
        public Vector2 Position;
        public EnemyTelegraphStartedEvent(string enemyId, Vector2 position)
        {
            EnemyId = enemyId;
            Position = position;
        }
    }

    public class EnemyTelegraphEndedEvent
    {
        public string EnemyId;
        public EnemyTelegraphEndedEvent(string enemyId)
        {
            EnemyId = enemyId;
        }
    }

    public class BestiaryEntryUpdatedEvent
    {
        public string EnemyId;
        public string UpdateType;
        public string BestiaryEntryId;

        public BestiaryEntryUpdatedEvent(string enemyId, string updateType, string bestiaryEntryId = "")
        {
            EnemyId = enemyId ?? string.Empty;
            UpdateType = updateType ?? string.Empty;
            BestiaryEntryId = bestiaryEntryId ?? string.Empty;
        }
    }

    public class EnemyLootRolledEvent
    {
        public string EnemyId;
        public string ItemId;
        public int Amount;
        public EnemyLootRolledEvent(string enemyId, string itemId, int amount)
        {
            EnemyId = enemyId;
            ItemId = itemId;
            Amount = amount;
        }
    }

    public class EnemyVulnerabilityStartedEvent
    {
        public string EnemyId;
        public float Multiplier;
        public float Duration;
        public EnemyVulnerabilityStartedEvent(string enemyId, float multiplier, float duration)
        {
            EnemyId = enemyId;
            Multiplier = multiplier;
            Duration = duration;
        }
    }

    public class EnemyVulnerabilityEndedEvent
    {
        public string EnemyId;
        public EnemyVulnerabilityEndedEvent(string enemyId) { EnemyId = enemyId; }
    }

    public class EnemySpawnResolvedEvent
    {
        public int CaveLevel;
        public string[] BiomeTags;
        public string PackId;
        public string[] EnemyIds;
        public string[] Warnings;

        public EnemySpawnResolvedEvent(int caveLevel, string[] biomeTags, string packId, string[] enemyIds, string[] warnings)
        {
            CaveLevel = caveLevel;
            BiomeTags = biomeTags ?? System.Array.Empty<string>();
            PackId = packId ?? string.Empty;
            EnemyIds = enemyIds ?? System.Array.Empty<string>();
            Warnings = warnings ?? System.Array.Empty<string>();
        }
    }

    public class EnemySpawnResolverWarningEvent
    {
        public int CaveLevel;
        public string[] BiomeTags;
        public string[] Warnings;

        public EnemySpawnResolverWarningEvent(int caveLevel, string[] biomeTags, string[] warnings)
        {
            CaveLevel = caveLevel;
            BiomeTags = biomeTags ?? System.Array.Empty<string>();
            Warnings = warnings ?? System.Array.Empty<string>();
        }
    }

    // fable_04: a pack engaged or lost a member; survivors of the same PackId are alerted toward
    // the position. Telemetry/HUD hook only â€” coordination itself happens via EnemyBrain.OnPackAlert.
    public class EnemyPackAlertedEvent
    {
        public string PackId;
        public Vector2 Position;
        public int MemberCount;
        public string[] MemberInstanceIds;

        public EnemyPackAlertedEvent(string packId, Vector2 position, int memberCount,
            string[] memberInstanceIds = null)
        {
            PackId = packId ?? string.Empty;
            Position = position;
            MemberCount = memberCount;
            MemberInstanceIds = memberInstanceIds ?? System.Array.Empty<string>();
        }
    }

    public readonly struct EnemyPackLeashCompletedEvent
    {
        public readonly string PackId;
        public readonly string[] MemberInstanceIds;

        public EnemyPackLeashCompletedEvent(string packId, string[] memberInstanceIds = null)
        {
            PackId = packId ?? string.Empty;
            MemberInstanceIds = memberInstanceIds ?? System.Array.Empty<string>();
        }
    }

    public readonly struct EnemyAggroStartedEvent
    {
        public readonly string EnemyInstanceId;
        public readonly string PackId;

        public EnemyAggroStartedEvent(string enemyInstanceId, string packId)
        {
            EnemyInstanceId = enemyInstanceId ?? string.Empty;
            PackId = packId ?? string.Empty;
        }
    }

    public readonly struct EnemyLeashCompletedEvent
    {
        public readonly string EnemyInstanceId;

        public EnemyLeashCompletedEvent(string enemyInstanceId)
        {
            EnemyInstanceId = enemyInstanceId ?? string.Empty;
        }
    }

    // fable_24: a RetreatAndCall enemy (or a flanker whose leader just died) calls for help.
    // Allies within Radius of Position should regroup toward the caller. Additive event; the pack
    // coordinator and brains consume it. EmitterId is the calling enemy's type id (telemetry).
    public class EnemyCallForHelpEvent
    {
        public string EmitterId;
        public Vector2 Position;
        public float Radius;

        public EnemyCallForHelpEvent(string emitterId, Vector2 position, float radius)
        {
            EmitterId = emitterId ?? string.Empty;
            Position = position;
            Radius = radius;
        }
    }

    // fable_05: a cave boss crossed into a new phase (HP threshold). phaseIndex is 0-based into the
    // BossPhaseProfileSO.Phases array. HUD/telemetry hook only â€” the phase logic lives in
    // BossBrainController; this event just announces the transition (CA-1).
    public class BossPhaseChangedEvent
    {
        public string BossId;
        public int PhaseIndex;

        public BossPhaseChangedEvent(string bossId, int phaseIndex)
        {
            BossId = bossId ?? string.Empty;
            PhaseIndex = phaseIndex;
        }
    }

    public class EnemySpawnPackSelectedEvent
    {
        public int CaveLevel;
        public string[] BiomeTags;
        public string PackId;
        public string[] EnemyIds;

        public EnemySpawnPackSelectedEvent(int caveLevel, string[] biomeTags, string packId, string[] enemyIds)
        {
            CaveLevel = caveLevel;
            BiomeTags = biomeTags ?? System.Array.Empty<string>();
            PackId = packId ?? string.Empty;
            EnemyIds = enemyIds ?? System.Array.Empty<string>();
        }
    }

    /// <summary>
    /// fable_83 — Publicado pelo EnemyBrain ao executar SummonAdds.
    /// O materializer/spawner da caverna pode reagir e instanciar a criatura.
    /// Seed e posicao sao deterministas (ADR-0005; sem GUID/timestamp).
    /// </summary>
    public class EnemyAddsSummonedEvent
    {
        public string SummonerEnemyId;
        public string AddEnemyId;
        public Vector2 SpawnPosition;
        public int DeterministicSeed;

        public EnemyAddsSummonedEvent(string summonerEnemyId, string addEnemyId, Vector2 spawnPosition, int deterministicSeed)
        {
            SummonerEnemyId = summonerEnemyId ?? string.Empty;
            AddEnemyId = addEnemyId ?? string.Empty;
            SpawnPosition = spawnPosition;
            DeterministicSeed = deterministicSeed;
        }
    }
}
