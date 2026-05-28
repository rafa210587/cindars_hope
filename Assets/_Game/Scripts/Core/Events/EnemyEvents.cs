using UnityEngine;

namespace CindarsHope.Core.Events
{
    public class EnemySpawnedEvent
    {
        public string EnemyId;
        public Vector2 Position;
        public EnemySpawnedEvent(string enemyId, Vector2 position)
        {
            EnemyId = enemyId;
            Position = position;
        }
    }

    public class EnemySeenEvent
    {
        public string EnemyId;
        public Vector2 Position;
        public EnemySeenEvent(string enemyId, Vector2 position)
        {
            EnemyId = enemyId;
            Position = position;
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

    public class EnemyDespawnedEvent
    {
        public string EnemyId;
        public EnemyDespawnedEvent(string enemyId)
        {
            EnemyId = enemyId;
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

    public class EnemyXPGrantedEvent
    {
        public string EnemyId;
        public int XPAmount;
        public EnemyXPGrantedEvent(string enemyId, int xpAmount)
        {
            EnemyId = enemyId;
            XPAmount = xpAmount;
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

    public class EnemyRespawnScheduledEvent
    {
        public string EnemyId;
        public int GameDayAtRespawn;
        public EnemyRespawnScheduledEvent(string enemyId, int gameDay)
        {
            EnemyId = enemyId;
            GameDayAtRespawn = gameDay;
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
}
