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
            // Validate direction
            if (request.Direction.sqrMagnitude < 0.001f)
            {
                string errorMsg = "ProjectileSpawnService: Direction is zero";
                Debug.LogError($"CombatLog: ProjectileSpawned. Success=False, ErrorCode=DirectionZero, Message={errorMsg}");
                return ProjectileSpawnResult.CreateError("DirectionZero", errorMsg);
            }

            // Calculate spawn position
            Vector2 spawnPos = request.SourcePosition + request.Direction.normalized * request.SpawnOffset;

            // Instantiate authored prefab when available; otherwise build a runtime projectile
            // with procedural visuals so bow/spell attacks never dead-end on missing art.
            GameObject projectile;
            if (request.Prefab != null)
            {
                projectile = Object.Instantiate(request.Prefab, spawnPos, Quaternion.identity);
            }
            else
            {
                projectile = RuntimeProjectileFactory.Create(request.VisualStyle, request.DamageType);
                projectile.transform.position = spawnPos;
            }

            // Get ProjectileBehaviour component
            var projectileBehaviour = projectile.GetComponent<ProjectileBehaviour>();
            if (projectileBehaviour == null)
            {
                string errorMsg = $"ProjectileSpawnService: ProjectileBehaviour not found on projectile '{projectile.name}'";
                Debug.LogError($"CombatLog: ProjectileSpawned. Success=False, ErrorCode=MissingProjectileBehaviour, Message={errorMsg}");
                Object.Destroy(projectile);
                return ProjectileSpawnResult.CreateError("MissingProjectileBehaviour", errorMsg);
            }

            projectileBehaviour.SetMaxHits(request.MaxHits);

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
