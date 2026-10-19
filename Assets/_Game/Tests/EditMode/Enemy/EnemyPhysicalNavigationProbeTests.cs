using System.Collections.Generic;
using CindarsHope.Combat;
using CindarsHope.Enemy;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Enemy
{
    /// <summary>Physics-backed probes for the movement pipeline, without loading or editing CaveScene.</summary>
    public sealed class EnemyPhysicalNavigationProbeTests
    {
        private readonly List<Object> _owned = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (var i = _owned.Count - 1; i >= 0; i--)
                if (_owned[i] != null) Object.DestroyImmediate(_owned[i]);
            _owned.Clear();
        }

        [Test]
        public void GroundChase_BodyWouldClipOffsetObstacle_AvoidsBeforeContact()
        {
            var enemy = NewBody("wide_enemy", Vector2.zero, 0.5f);
            var target = NewObject("target", new Vector2(4f, 0f));
            var obstacle = NewObject("offset_obstacle", new Vector2(1f, 0.4f));
            obstacle.layer = 8;
            obstacle.AddComponent<BoxCollider2D>().size = new Vector2(0.2f, 0.2f);
            Physics2D.SyncTransforms();
            var executor = NewExecutor(enemy, target, null);

            executor.ExecuteMovement(EnemyBrainState.Chase, EnemyMovementType.GroundChase);

            Assert.IsTrue(executor.IsAvoidingObstacle,
                "the center ray misses, but a 0.5-radius body clips this offset WorldSolid obstacle");
        }

        [Test]
        public void CircleStrafe_WorldSolidAhead_AvoidsWhileMaintainingOrbitSpeed()
        {
            var enemy = NewBody("orbit_enemy", Vector2.zero, 0.3f);
            var target = NewObject("target", new Vector2(4f, 0f));
            var obstacle = NewObject("orbit_obstacle", new Vector2(0f, 0.8f));
            obstacle.layer = 8;
            obstacle.AddComponent<BoxCollider2D>().size = new Vector2(0.8f, 0.2f);
            var profile = ScriptableObject.CreateInstance<EnemyMovementProfileSO>();
            _owned.Add(profile);
            profile.MovementType = EnemyMovementType.CircleStrafe;
            profile.PreferredDistance = 4f;
            Physics2D.SyncTransforms();
            var executor = NewExecutor(enemy, target, profile);

            executor.ExecuteMovement(EnemyBrainState.Chase, EnemyMovementType.CircleStrafe);

            Assert.IsTrue(executor.IsAvoidingObstacle,
                "orbit movement must account for the real solid wall in its tangential direction");
            Assert.Greater(enemy.linearVelocity.magnitude, 0f, "avoidance must not freeze the orbit");
        }

        private Rigidbody2D NewBody(string name, Vector2 position, float radius)
        {
            var go = NewObject(name, position);
            var body = go.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            go.AddComponent<CircleCollider2D>().radius = radius;
            return body;
        }

        private GameObject NewObject(string name, Vector2 position)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            _owned.Add(go);
            return go;
        }

        private static EnemyMovementExecutor NewExecutor(Rigidbody2D body, GameObject target,
            EnemyMovementProfileSO profile)
        {
            var executor = new EnemyMovementExecutor
            {
                EvasionRng = new System.Random(1),
                ObstacleLayerMask = 1 << 8
            };
            executor.Init(body, body.transform, null, profile, null,
                () => target, () => 2f,
                () => Vector2.Distance(body.position, target.transform.position),
                () => ((Vector2)target.transform.position - body.position).normalized,
                _ => { }, () => null, () => null);
            return executor;
        }
    }
}
