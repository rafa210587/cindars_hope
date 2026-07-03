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
                // Prefabs autorados podem ter um sprite built-in placeholder que nao resolve em runtime
                // (flecha invisivel). Garante um sprite visivel sem alterar o prefab no disco.
                RuntimeProjectileFactory.EnsureVisibleSprite(projectile, request.VisualStyle, request.DamageType);
            }
            else
            {
                projectile = RuntimeProjectileFactory.Create(request.VisualStyle, request.DamageType);
                projectile.transform.position = spawnPos;
            }

            // spec_codex_13: layer de gameplay do projetil (prefab autorado ou procedural).
            CindarsHope.Core.Physics.GameplayLayerNames.TryAssignRuntimeLayer(
                projectile, CindarsHope.Core.Physics.GameplayLayerNames.Projectile);

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
            // fable_48: tags de material/elemento da munição (flecha) anexadas ANTES do Initialize;
            // viajam ao DamageRequest no impacto (matching de vulnerabilidade F06). Null = sem tags.
            projectileBehaviour.SetAppliedTags(request.AppliedTags);

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
                    request.StatusApplyChance,
                    request.SpeedDecayToFraction,
                    request.HomingRange
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
                    request.KnockbackForce,
                    request.SpeedDecayToFraction,
                    request.HomingRange
                );
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            LogSpawnDiagnostics(projectile, request, spawnPos);
#endif

            return ProjectileSpawnResult.CreateSuccess(projectile);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // Diagnostico de visibilidade do projetil: caminho (prefab/factory), posicao, e estado de
        // render (sprite, renderer ativo, sorting, escala, cor). Uma linha responde "por que invisivel?".
        private static void LogSpawnDiagnostics(GameObject projectile, ProjectileSpawnRequest request, Vector2 spawnPos)
        {
            if (projectile == null) return;

            var sr = projectile.GetComponentInChildren<SpriteRenderer>();
            string spriteName = sr != null && sr.sprite != null ? sr.sprite.name : "<null>";
            string texName = sr != null && sr.sprite != null && sr.sprite.texture != null ? sr.sprite.texture.name : "<null>";
            string matName = sr != null && sr.sharedMaterial != null ? sr.sharedMaterial.name : "<null>";
            string rendererState = sr != null
                ? $"enabled={sr.enabled}, sprite='{spriteName}', tex='{texName}', mat='{matName}', " +
                  $"color={sr.color}, sortLayer={sr.sortingLayerID}, sortOrder={sr.sortingOrder}"
                : "<no SpriteRenderer>";

            Debug.Log($"CombatLog: ProjectileSpawned. Success=True, " +
                      $"Path={(request.Prefab != null ? "AuthoredPrefab" : "RuntimeFactory")}, " +
                      $"VisualStyle={request.VisualStyle}, DamageType={request.DamageType}, " +
                      $"RequestSourcePos={request.SourcePosition}, SpawnPos={spawnPos}, " +
                      $"ProjectilePos={(Vector2)projectile.transform.position}, " +
                      $"Scale={projectile.transform.localScale}, Direction={request.Direction}, " +
                      $"Renderer[{rendererState}]");
        }
#endif
    }
}
