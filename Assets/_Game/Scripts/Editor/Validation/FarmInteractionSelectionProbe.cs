using System;
using System.Collections.Generic;
using System.Linq;
using CindarsHope.Craft;
using CindarsHope.Cave.Runtime;
using CindarsHope.Interaction;
using CindarsHope.Player;
using CindarsHope.SceneManagement;
using CindarsHope.World;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    /// <summary>Waits for real trigger callbacks and asks the live selector; never executes an interaction.</summary>
    internal sealed class FarmInteractionSelectionProbe : IDisposable
    {
        private const float SearchRadius = 2f;
        private const float SearchStep = 0.25f;
        private const int CandidateLimit = 12;
        [Serializable] internal sealed class Selection
        {
            public string id, status = "FAIL", expected, selected, reason;
            public Vector2 reachableEndpoint, checkedPosition;
            public int attempts;
        }
        [Serializable] internal sealed class Evidence
        {
            public string status = "RUNNING";
            public string method = "Real InteractionSystem.GetCurrentInteractable after >=2 physics steps, from BFS endpoint or a collider-swept local position within 2u; no Interact or injected candidates. House door, three craft stations, exterior farm cave entrance FishingSpot and town portal; decorative well excluded. Selection never executes a scene transition or fishing action.";
            public List<Selection> selections = new List<Selection>();
        }
        private readonly PlayerController player;
        private readonly InteractionSystem selector;
        private readonly Collider2D solid;
        private readonly Rigidbody2D body;
        private readonly Vector3 originalPosition;
        private readonly Vector2 originalVelocity;
        private readonly ContactFilter2D filter;
        private readonly Collider2D[] overlaps = new Collider2D[64];
        private readonly RaycastHit2D[] hits = new RaycastHit2D[64];
        private readonly MonoBehaviour[] targets;
        private readonly List<Vector2> candidates = new List<Vector2>();
        private int targetIndex, candidateIndex;
        private float positionedFixedTime;
        private bool positioned, expanded, disposed;
        internal Evidence Result { get; } = new Evidence();

        internal FarmInteractionSelectionProbe(PlayerController player, FarmPhysicalRouteProbe.Evidence routes)
        {
            this.player = player;
            originalPosition = player.transform.position;
            body = player.GetComponent<Rigidbody2D>();
            originalVelocity = body != null ? body.linearVelocity : Vector2.zero;
            selector = player.GetComponentsInChildren<InteractionSystem>().Single();
            solid = player.GetComponentsInChildren<Collider2D>().Single(c => c.enabled && !c.isTrigger);
            filter = new ContactFilter2D { useTriggers = false };
            filter.SetLayerMask(Physics2D.GetLayerCollisionMask(solid.gameObject.layer));
            var components = player.gameObject.scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<MonoBehaviour>(true)).ToArray();
            targets = new MonoBehaviour[]
            {
                components.OfType<HouseDoorInteractable>().Single(c => c.transform.parent != null && c.transform.parent.name == "FarmHouse"),
                components.OfType<CraftingPoint>().Single(c => c.name == "CraftingStation_Workbench"),
                components.OfType<CraftingPoint>().Single(c => c.name == "CraftingStation_Forge"),
                components.OfType<CraftingPoint>().Single(c => c.name == "CraftingStation_CookingStation"),
                components.OfType<CaveEntranceInteractable>().Single(c => c.name == "CaveEntrance"),
                components.OfType<FishingSpot>().Single(c => c.name == "FishingSpot"),
                components.OfType<ScenePortal>().Single(c => c.name == "Portal_Farm_To_Town")
            };
            var ids = new[] { "house", "workbench", "forge", "cooking", "cave_entrance_approach", "fishing_approach", "town_exit" };
            for (var i = 0; i < ids.Length; i++)
            {
                var route = routes.routes.Single(r => r.id == ids[i] && r.status == "PASS");
                Result.selections.Add(new Selection { id = ids[i], expected = targets[i].name + "/" + targets[i].GetType().Name,
                    reachableEndpoint = route.reachedApproach });
            }
        }

        internal bool Tick()
        {
            if (targetIndex == targets.Length) return true;
            var selection = Result.selections[targetIndex];
            if (!positioned)
            {
                candidates.Clear();
                candidates.Add(selection.reachableEndpoint);
                candidateIndex = 0;
                expanded = false;
                PositionCandidate();
                positioned = true;
                return false;
            }
            if (Time.fixedTime - positionedFixedTime + 0.0001f < Time.fixedDeltaTime * 2f) return false;
            selection.attempts++;
            selection.checkedPosition = player.transform.position;
            var selected = selector.GetCurrentInteractable();
            selection.selected = selected is Component component ? component.name + "/" + component.GetType().Name : "none";
            if (ReferenceEquals(selected, targets[targetIndex]))
            {
                selection.status = "PASS";
                selection.reason = "Expected target selected by live trigger/CanInteract/distance logic.";
                return FinishTarget();
            }
            if (!expanded)
            {
                AddReachableCandidates(selection.reachableEndpoint, targets[targetIndex].transform.position);
                expanded = true;
            }
            if (++candidateIndex < candidates.Count)
            {
                PositionCandidate();
                return false;
            }
            selection.reason = "Target not selected at endpoint or bounded physically connected local candidates.";
            return FinishTarget();
        }

        private bool FinishTarget()
        {
            targetIndex++;
            positioned = false;
            if (targetIndex < targets.Length) return false;
            Result.status = Result.selections.All(s => s.status == "PASS") ? "PASS" : "FAIL";
            Dispose();
            return true;
        }

        private void AddReachableCandidates(Vector2 endpoint, Vector2 target)
        {
            var options = new List<Vector2>();
            try
            {
                for (var x = -8; x <= 8; x++)
                for (var y = -8; y <= 8; y++)
                {
                    var delta = new Vector2(x, y) * SearchStep;
                    if (delta == Vector2.zero || delta.magnitude > SearchRadius) continue;
                    SetPosition(endpoint);
                    var count = solid.Cast(delta.normalized, filter, hits, delta.magnitude);
                    if (count == hits.Length) throw new InvalidOperationException("Interaction sweep buffer saturated.");
                    var blocked = false;
                    for (var i = 0; i < count; i++)
                        if (hits[i].collider != null && !hits[i].collider.transform.IsChildOf(player.transform)) { blocked = true; break; }
                    if (blocked) continue;
                    var position = endpoint + delta;
                    SetPosition(position);
                    count = solid.OverlapCollider(filter, overlaps);
                    if (count == overlaps.Length) throw new InvalidOperationException("Interaction overlap buffer saturated.");
                    for (var i = 0; i < count; i++)
                        if (!overlaps[i].transform.IsChildOf(player.transform)) { blocked = true; break; }
                    if (!blocked) options.Add(position);
                }
            }
            finally { SetPosition(endpoint); }
            options.Sort((a, b) => (a - target).sqrMagnitude.CompareTo((b - target).sqrMagnitude));
            for (var i = 0; i < Math.Min(CandidateLimit, options.Count); i++) candidates.Add(options[i]);
        }

        private void PositionCandidate()
        {
            SetPosition(candidates[candidateIndex]);
            positionedFixedTime = Time.fixedTime;
        }

        private void SetPosition(Vector2 position)
        {
            player.transform.position = new Vector3(position.x, position.y, originalPosition.z);
            if (body != null) { body.position = position; body.linearVelocity = Vector2.zero; }
            Physics2D.SyncTransforms();
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            if (player == null) return;
            SetPosition(originalPosition);
            if (body != null) body.linearVelocity = originalVelocity;
        }
    }
}
