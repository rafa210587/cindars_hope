using CindarsHope.Combat;
using CindarsHope.Combat.StatusEffect;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.DebugTools;
using UnityEngine;
using CindarsHope.Foundation;

namespace CindarsHope.Enemy
{
    /// <summary>
    /// spec_enemy_attack_kits_v1 â€” runner destacado para uma zona de hazard persistente no chao
    /// (lava_bulwark/magma_slug/fungal_spreader/veilkin_pyromancer/cave_burrower_elite etc.), no
    /// mesmo padrao de sobrevivencia do <see cref="EnemyVolatileExplosionRunner"/> (elite affix
    /// Volatile): GameObject destacado, independente do inimigo que a criou (que pode morrer/ser
    /// desativado logo em seguida sem derrubar a zona). Tick por intervalo (nao por-frame arbitrario)
    /// via <see cref="EnemyActionExecution.ResolveHazardTickCount"/>; deteccao de overlap por
    /// distancia via <see cref="EnemyActionExecution.IsInsideHazard"/> (sem fÃ­sica/collider novo).
    /// Visual procedural minimo (LineRenderer circular colorido pelo elemento), sem asset de arte novo.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemyHazardZoneRunner : MonoBehaviour
    {
        private Vector2 _center;
        private float _radius;
        private float _durationSeconds;
        private float _tickSeconds;
        private int _damagePerTick;
        private string _statusId;
        private string _sourceEnemyId = "enemy";

        private float _elapsedTotal;
        private float _elapsedSinceLastTick;

        public static EnemyHazardZoneRunner Spawn(
            Vector2 center, float radius, float durationSeconds, float tickSeconds,
            int damagePerTick, string statusId, string sourceEnemyId)
        {
            var go = new GameObject($"HazardZone_{sourceEnemyId}");
            go.transform.position = center;
            var runner = go.AddComponent<EnemyHazardZoneRunner>();
            runner._center = center;
            runner._radius = Mathf.Max(0.1f, radius);
            runner._durationSeconds = Mathf.Max(0.1f, durationSeconds);
            runner._tickSeconds = Mathf.Max(0.1f, tickSeconds);
            runner._damagePerTick = Mathf.Max(0, damagePerTick);
            runner._statusId = statusId ?? string.Empty;
            runner._sourceEnemyId = string.IsNullOrWhiteSpace(sourceEnemyId) ? "enemy" : sourceEnemyId;
            runner.BuildVisual();
            return runner;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            _elapsedTotal += dt;
            _elapsedSinceLastTick += dt;

            if (EnemyActionExecution.IsHazardExpired(_elapsedTotal, _durationSeconds))
            {
                Destroy(gameObject);
                return;
            }

            int ticks = EnemyActionExecution.ResolveHazardTickCount(_elapsedSinceLastTick, _tickSeconds);
            if (ticks <= 0)
            {
                return;
            }

            _elapsedSinceLastTick -= ticks * _tickSeconds;
            for (int i = 0; i < ticks; i++)
            {
                ApplyTick();
            }
        }

        private void ApplyTick()
        {
            var playerManager = GameBootstrap.Instance?.PlayerManager as CindarsHope.Player.PlayerManager;
            if (playerManager == null)
            {
                return;
            }

            var playerObject = playerManager.gameObject;
            if (!EnemyActionExecution.IsInsideHazard(_center, _radius, playerObject.transform.position))
            {
                return;
            }

            if (_damagePerTick > 0)
            {
                int applied = PlayerDamageReceiver.ApplyDamage(playerManager, _damagePerTick, _sourceEnemyId, DamageType.Physical, gameObject);
                GameEventBus.Publish(new PlayerDamagedEvent(applied, transform.position, _sourceEnemyId, _sourceEnemyId));
                CombatLog.Log($"CombatLog: HazardZoneTick. EnemyId={_sourceEnemyId}, Damage={applied}, Radius={_radius:F2}.");
            }

            if (!string.IsNullOrWhiteSpace(_statusId))
            {
                var receiver = PlayerStatusReceiver.Instance;
                if (receiver != null)
                    receiver.TryApplyFromEnemyAction(_statusId, 1f);
            }
        }

        // Visual procedural minimo â€” circulo no chao (sem asset de arte novo), mesmo idioma do
        // RuntimeProjectileFactory/EnemyVolatileExplosionRunner de gerar visual em runtime.
        private void BuildVisual()
        {
            var lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = false;
            lineRenderer.loop = true;
            lineRenderer.widthMultiplier = 0.05f;
            lineRenderer.positionCount = 24;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = HazardVisualColor();
            lineRenderer.endColor = lineRenderer.startColor;

            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                float angle = i * Mathf.PI * 2f / lineRenderer.positionCount;
                lineRenderer.SetPosition(i, new Vector3(Mathf.Cos(angle) * _radius, Mathf.Sin(angle) * _radius, 0f));
            }
        }

        private Color HazardVisualColor()
        {
            // Cor por elemento (heuristica de StatusId), coerente com a convencao de telegraph/VFX
            // por elemento do ENEMY_ATTACK_IMPLEMENTATION_DIRECTION Â§2. Fallback laranja (fogo/lava â€”
            // maioria dos usuarios atuais de HazardZone).
            if (!string.IsNullOrWhiteSpace(_statusId))
            {
                if (_statusId.Contains("chill") || _statusId.Contains("frost")) return new Color(0.5f, 0.8f, 1f, 0.6f);
                if (_statusId.Contains("poison") || _statusId.Contains("toxic")) return new Color(0.2f, 0.9f, 0.2f, 0.6f);
                if (_statusId.Contains("slow")) return new Color(0.6f, 0.6f, 0.6f, 0.6f);
            }
            return new Color(1f, 0.4f, 0.1f, 0.6f);
        }
    }
}
