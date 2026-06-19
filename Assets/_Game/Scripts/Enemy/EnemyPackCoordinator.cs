using System.Collections.Generic;
using CindarsHope.Cave.Runtime;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Enemy
{
    /// <summary>
    /// fable_04 — runtime coordination of enemy packs (shared aggro + shared leash).
    ///
    /// Created and owned by <c>CaveRuntimeMaterializer</c> on the generated runtime root, so its
    /// lifecycle is tied to materialization (destroyed/recreated per level — no stale state, no
    /// scene search). Members register themselves at spawn with their <c>PackId</c> and spawn
    /// position. The coordinator:
    ///   - alerts every member of a pack when one engages or dies (PackAlert);
    ///   - exposes the deterministic pack anchor (centroid of spawn positions) for collective leash;
    ///   - tells the brain when the WHOLE pack is beyond leash so they reset together.
    ///
    /// No GameObject.Find / FindObjectsOfType: the registry is fed explicitly by the spawn path
    /// (rule: unity-architecture). The anchor is a pure function of the deterministic spawn plan
    /// (rule: cave-stable-run / ADR-0005) — revisiting a level reproduces the same anchors.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemyPackCoordinator : MonoBehaviour
    {
        private sealed class PackRecord
        {
            public readonly List<EnemyBrain> Members = new List<EnemyBrain>();
            public Vector2 SpawnPositionSum = Vector2.zero;
            public int SpawnCount;

            public Vector2 Anchor => SpawnCount > 0 ? SpawnPositionSum / SpawnCount : Vector2.zero;
        }

        private readonly Dictionary<string, PackRecord> _packs = new Dictionary<string, PackRecord>();
        private bool _subscribed;

        private void OnEnable()
        {
            if (!_subscribed)
            {
                GameEventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
                _subscribed = true;
            }
        }

        private void OnDisable()
        {
            if (_subscribed)
            {
                GameEventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
                _subscribed = false;
            }
        }

        /// <summary>
        /// Register a freshly spawned member. The spawn position contributes to the pack anchor;
        /// it is recorded once per registration so the centroid matches the deterministic plan.
        /// </summary>
        public void Register(string packId, EnemyBrain brain, Vector2 spawnPosition)
        {
            if (string.IsNullOrWhiteSpace(packId) || brain == null)
            {
                return;
            }

            if (!_packs.TryGetValue(packId, out var record))
            {
                record = new PackRecord();
                _packs[packId] = record;
            }

            if (!record.Members.Contains(brain))
            {
                record.Members.Add(brain);
            }

            record.SpawnPositionSum += spawnPosition;
            record.SpawnCount++;
        }

        /// <summary>Number of distinct packs currently registered (telemetry/tests).</summary>
        public int PackCount => _packs.Count;

        public int MemberCount(string packId)
        {
            return _packs.TryGetValue(packId, out var record) ? record.Members.Count : 0;
        }

        /// <summary>
        /// Put every living member of a pack on alert toward <paramref name="position"/>.
        /// Used when one member first detects the player or when a member dies. Alert does NOT
        /// propagate across packs (risk mitigation: never pull the whole floor).
        /// </summary>
        public void Alert(string packId, Vector2 position)
        {
            if (string.IsNullOrWhiteSpace(packId) || !_packs.TryGetValue(packId, out var record))
            {
                return;
            }

            for (int i = 0; i < record.Members.Count; i++)
            {
                var member = record.Members[i];
                if (member == null)
                {
                    continue;
                }

                member.OnPackAlert(position);
            }

            GameEventBus.Publish(new EnemyPackAlertedEvent(packId, position, record.Members.Count));
        }

        /// <summary>Deterministic pack anchor = centroid of the members' spawn positions.</summary>
        public Vector2 GetAnchor(string packId)
        {
            return _packs.TryGetValue(packId, out var record) ? record.Anchor : Vector2.zero;
        }

        /// <summary>
        /// True only when EVERY living member of the pack is farther than <paramref name="leashRange"/>
        /// from the pack anchor. A single member still inside the leash keeps the whole pack engaged,
        /// so the reset is collective and legible (they give up and walk back together).
        /// </summary>
        public bool IsWholePackBeyondLeash(string packId, float leashRange)
        {
            if (string.IsNullOrWhiteSpace(packId) || !_packs.TryGetValue(packId, out var record))
            {
                return false;
            }

            var anchor = record.Anchor;
            float leashSqr = leashRange * leashRange;
            bool anyAlive = false;

            for (int i = 0; i < record.Members.Count; i++)
            {
                var member = record.Members[i];
                if (member == null || !member.isActiveAndEnabled)
                {
                    continue;
                }

                anyAlive = true;
                float sqr = ((Vector2)member.transform.position - anchor).sqrMagnitude;
                if (sqr <= leashSqr)
                {
                    return false;
                }
            }

            return anyAlive;
        }

        // A member died: find which pack it belonged to (nearest registered member to the death
        // position) and alert the survivors. EnemyKilledEvent carries only the enemy TYPE id and a
        // death position, not the pack id, so position is the stable mapping key here.
        private void OnEnemyKilled(EnemyKilledEvent evt)
        {
            var deathPos = (Vector2)evt.DeathPosition;
            string bestPackId = null;
            float bestSqr = float.MaxValue;

            foreach (var kvp in _packs)
            {
                var record = kvp.Value;
                for (int i = 0; i < record.Members.Count; i++)
                {
                    var member = record.Members[i];
                    if (member == null)
                    {
                        continue;
                    }

                    float sqr = ((Vector2)member.transform.position - deathPos).sqrMagnitude;
                    if (sqr < bestSqr)
                    {
                        bestSqr = sqr;
                        bestPackId = kvp.Key;
                    }
                }
            }

            if (bestPackId != null)
            {
                Alert(bestPackId, deathPos);
            }
        }

        /// <summary>
        /// Pure, deterministic anchor for a pack from a spawn plan: centroid of the world
        /// positions of every entry sharing <paramref name="packId"/>. Mirrors the runtime
        /// <see cref="GetAnchor"/> so the same plan always yields the same anchor (ADR-0005).
        /// Returns false when the pack has no entries in the plan.
        /// </summary>
        public static bool ComputeAnchorFromPlan(CaveEnemySpawnPlan plan, string packId, out Vector2 anchor)
        {
            anchor = Vector2.zero;
            if (plan == null || plan.Entries == null || string.IsNullOrWhiteSpace(packId))
            {
                return false;
            }

            Vector2 sum = Vector2.zero;
            int count = 0;
            foreach (var entry in plan.Entries)
            {
                if (entry == null || entry.PackId != packId)
                {
                    continue;
                }

                sum += new Vector2(entry.WorldPosition.x, entry.WorldPosition.y);
                count++;
            }

            if (count == 0)
            {
                return false;
            }

            anchor = sum / count;
            return true;
        }
    }
}
