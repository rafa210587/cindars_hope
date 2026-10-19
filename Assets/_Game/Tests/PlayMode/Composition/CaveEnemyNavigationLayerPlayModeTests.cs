using System.Collections;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Physics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.PlayMode.Composition
{
    /// <summary>Checks the actual generated Cave wall collider against the EnemyBrain obstacle mask.</summary>
    public sealed class CaveEnemyNavigationLayerPlayModeTests
    {
        [UnityTest]
        public IEnumerator GeneratedWallCollider_IsIncludedInEnemyWorldSolidQuery()
        {
            // The PlayMode runner's temporary backup scene can emit this exact delayed
            // diagnostic before CaveScene loads; it is not a Cave navigation result.
            if (GameBootstrap.Instance == null)
                LogAssert.Expect(LogType.Error,
                    "[DeathSystemBootstrap] GameBootstrap/PlayerManager/CorpseRecoveryManager nao ficaram prontos apos 120 frames. Sistema de morte NAO inicializado (morte->corpo->respawn indisponivel).");
            yield return SceneManager.LoadSceneAsync("CaveScene", LoadSceneMode.Single);
            var cave = SceneManager.GetActiveScene();
            Assert.That(cave.name, Is.EqualTo("CaveScene"));

            BoxCollider2D wall = null;
            for (var frame = 0; frame < 100 && wall == null; frame++)
            {
                var colliders = Object.FindObjectsByType<BoxCollider2D>(
                    FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                for (var i = 0; i < colliders.Length; i++)
                {
                    var candidate = colliders[i];
                    if (candidate != null && candidate.gameObject.scene == cave &&
                        candidate.name.StartsWith("WallTile_"))
                    {
                        wall = candidate;
                        break;
                    }
                }
                if (wall == null) yield return null;
            }
            Assert.That(wall, Is.Not.Null, "CaveScene must materialize real WallTile collider(s)");

            var enemyObstacleMask = GameplayLayerNames.GetMaskSafe(GameplayLayerNames.WorldSolid);
            Assert.That(enemyObstacleMask.value, Is.Not.EqualTo(0), "WorldSolid must exist for EnemyBrain");
            Physics2D.SyncTransforms();
            var point = (Vector2)wall.bounds.center;
            var physical = Physics2D.OverlapPointAll(point);
            Assert.That(physical, Does.Contain(wall), "chosen wall must participate in Physics2D");
            var visibleToEnemy = Physics2D.OverlapPointAll(point, enemyObstacleMask);
            Assert.That(visibleToEnemy, Does.Contain(wall),
                $"generated Cave wall '{wall.name}' layer={LayerMask.LayerToName(wall.gameObject.layer)} " +
                "must be included in EnemyBrain's WorldSolid avoidance query");
        }
    }
}
