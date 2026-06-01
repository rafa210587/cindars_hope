using UnityEngine;

namespace CindarsHope.Combat.Weapon
{
    /// <summary>
    /// SPEC_06: Service for spawning and initializing projectiles.
    /// Centralizes projectile instantiation and initialization logic.
    /// </summary>
    public static class ProjectileSpawnService
    {
        public static ProjectileSpawnResult SpawnProjectile(ProjectileSpawnRequest request)
        {
            // Validate prefab
            if (request.Prefab == null)
            {
                string errorMsg = "ProjectileSpawnService: Prefab is null";
                Debug.LogError($"CombatLog: ProjectileSpawned. Success=False, ErrorCode=PrefabNull, Message={errorMsg}");
                return ProjectileSpawnResult.CreateError("PrefabNull", errorMsg);
            }

            // Validate direction
            if (request.Direction.sqrMagnitude < 0.001f)
            {
                string errorMsg = "ProjectileSpawnService: Direction is zero";
                Debug.LogError($"CombatLog: ProjectileSpawned. Success=False, ErrorCode=DirectionZero, Message={errorMsg}");
                return ProjectileSpawnResult.CreateError("DirectionZero", errorMsg);
            }

            // Calculate spawn position
            Vector2 spawnPos = request.SourcePosition + request.Direction.normalized * request.SpawnOffset;

            // Instantiate projectile
            var projectile = Object.Instantiate(request.Prefab, spawnPos, Quaternion.identity);

            // Get ProjectileBehaviour component
            var projectileBehaviour = projectile.GetComponent<ProjectileBehaviour>();
            if (projectileBehaviour == null)
            {
                string errorMsg = $"ProjectileSpawnService: ProjectileBehaviour not found on projectile '{request.Prefab.name}'";
                Debug.LogError($"CombatLog: ProjectileSpawned. Success=False, ErrorCode=MissingProjectileBehaviour, Message={errorMsg}");
                Object.Destroy(projectile);
                return ProjectileSpawnResult.CreateError("MissingProjectileBehaviour", errorMsg);
            }

            // Initialize projectile with or without status effect
            if (request.StatusEffect != null && request.StatusApplyChance > 0f)
            {
                projectileBehaviour.InitializeWithStatus(
                    request.Direction,
                    request.Speed,
                    request.Range,
                    request.BaseDamage,
                    request.DamageType,
                    request.KnockbackForce,
                    request.StatusEffect,
                    request.StatusApplyChance
                );
            }
            else
            {
                projectileBehaviour.Initialize(
                    request.Direction,
                    request.Speed,
                    request.Range,
                    request.BaseDamage,
                    request.DamageType,
                    request.KnockbackForce
                );
            }

            return ProjectileSpawnResult.CreateSuccess(projectile);
        }
    }
}
