using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Editor.SceneCreation
{
    /// <summary>Conservative Editor preconditions plus deterministic policies testable without scene objects.</summary>
    public static class TownKeyartComponentPreflight
    {
        public const string TownScenePath = "Assets/_Game/Scenes/TownScene.unity";
        private static readonly string[] RequiredRoots = { "TownHouses", "TownTrees", "TownProps", "CentralPlaza" };

        public static void RequireTown(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded) throw new ArgumentException("A loaded scene must be explicitly supplied.");
            var roots = new List<string>(); var houses = new List<string>(); var expected = new List<string>();
            foreach (var root in scene.GetRootGameObjects())
            {
                roots.Add(root.name);
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                    if (t.name.StartsWith("House_",StringComparison.Ordinal))
                    {
                        if (t.parent == null || t.parent.name != "TownHouses" || t.parent.parent != null)
                            throw new ArgumentException("House ID outside canonical TownHouses root: " + t.name);
                        houses.Add(t.name);
                    }
            }
            foreach (var lot in TownCityLayout.AllBuildings) expected.Add(lot.Name);
            string issue = SceneIdentityIssue(scene.path,roots,houses,expected);
            if (issue != null) throw new ArgumentException(issue);
        }

        // An empty path is intentional: the creator must be able to run before the first save.
        public static string SceneIdentityIssue(string scenePath, IReadOnlyList<string> roots,
            IReadOnlyList<string> houses, IReadOnlyList<string> canonicalHouseIds)
        {
            if (!string.IsNullOrEmpty(scenePath) && scenePath != TownScenePath) return "Saved scene is not the canonical Town path.";
            if (roots == null || houses == null || canonicalHouseIds == null || canonicalHouseIds.Count != 24)
                return "Canonical Town identity requires 24 expected House IDs and explicit roots.";
            foreach (string required in RequiredRoots)
            {
                int count = 0; foreach (string root in roots) if (root == required) count++;
                if (count != 1) return "Town requires one root " + required + "; actual=" + count;
            }
            if (houses.Count != 24) return "Town requires exactly 24 materialized House IDs.";
            var remaining = new HashSet<string>(canonicalHouseIds,StringComparer.Ordinal);
            if (remaining.Count != 24) return "Canonical House ID list contains duplicates.";
            foreach (string house in houses) if (!remaining.Remove(house)) return "Unexpected or duplicate House ID: " + house;
            return remaining.Count == 0 ? null : "Canonical House IDs are missing.";
        }

        public struct BlockingFacts
        {
            public bool solidActive, solidEnabled, solidTrigger, solidHasRigidbody, solidComposite, solidEffector, solidAutoTiling;
            public bool playerActive, playerEnabled, playerTrigger, playerDynamicSimulated, playerComposite, playerEffector;
            public bool layerOverrides, layersIgnored, pairIgnored, solidLayerSimulated, playerLayerSimulated;
            public bool solidSendsToPlayer, playerReceivesFromSolid;
        }

        public static string BlockingIssue(BlockingFacts facts)
        {
            if (!facts.solidActive || !facts.solidEnabled || facts.solidTrigger) return "Support is inactive, disabled or a trigger.";
            if (facts.solidHasRigidbody) return "Support has unexpected Rigidbody ownership; this package owns plain static colliders only.";
            if (facts.solidComposite || facts.solidEffector) return "Support uses a composite or effector; effective shape/response requires a separate binding.";
            if (facts.solidAutoTiling) return "Support has unexpected BoxCollider2D.autoTiling; geometry must not be driven by tiling.";
            if (!facts.playerActive || !facts.playerEnabled || facts.playerTrigger || !facts.playerDynamicSimulated)
                return "Player must have its active, enabled non-trigger body attached to a simulated Dynamic Rigidbody2D.";
            if (facts.playerComposite || facts.playerEffector) return "Player composite/effector response is outside this package.";
            if (facts.layerOverrides) return "Non-default collider/Rigidbody layer overrides require explicit collision policy review.";
            if (facts.layersIgnored || facts.pairIgnored) return "Physics2D ignores the support/Player layer or collider pair.";
            if (!facts.solidLayerSimulated || !facts.playerLayerSimulated) return "Support or Player layer is excluded from Physics2D.simulationLayers.";
            if (!facts.solidSendsToPlayer || !facts.playerReceivesFromSolid) return "Force masks do not permit this static support to separate the dynamic Player.";
            return null;
        }

        public static Collider2D RequirePlayerBody(Scene scene)
        {
            Collider2D found = null; int players = 0;
            foreach (var root in scene.GetRootGameObjects())
            {
                if (!root.CompareTag("Player")) continue;
                players++;
                if (root.GetComponent<CindarsHope.Player.PlayerController>() == null)
                    throw new ArgumentException("Tagged Player root lacks the canonical PlayerController.");
                foreach (var collider in root.GetComponents<Collider2D>())
                {
                    if (collider.isTrigger) continue;
                    if (found != null) throw new ArgumentException("Player body is ambiguous; expected one root non-trigger collider.");
                    found = collider;
                }
            }
            if (players != 1 || found == null) throw new ArgumentException("Expected one tagged Player root with its non-trigger physical body.");
            return found;
        }

        public static string BlockingIssue(Collider2D solid, Collider2D player)
        {
            if (solid == null || player == null) return "Support/Player collider missing.";
            var body = player.attachedRigidbody;
            var facts = new BlockingFacts
            {
                solidActive = solid.gameObject.activeInHierarchy, solidEnabled = solid.enabled, solidTrigger = solid.isTrigger,
                solidHasRigidbody = solid.attachedRigidbody != null,
                solidComposite = solid.compositeOperation != Collider2D.CompositeOperation.None || solid.composite != null,
                solidEffector = HasEffector(solid.transform) || solid.usedByEffector,
                solidAutoTiling = solid is BoxCollider2D box && box.autoTiling,
                playerActive = player.gameObject.activeInHierarchy, playerEnabled = player.enabled, playerTrigger = player.isTrigger,
                playerDynamicSimulated = body != null && body.simulated && body.bodyType == RigidbodyType2D.Dynamic,
                playerComposite = player.compositeOperation != Collider2D.CompositeOperation.None || player.composite != null,
                playerEffector = HasEffector(player.transform) || player.usedByEffector,
                layerOverrides = HasLayerOverrides(solid) || HasLayerOverrides(player) ||
                    (body != null && (body.includeLayers.value != 0 || body.excludeLayers.value != 0)),
                layersIgnored = Physics2D.GetIgnoreLayerCollision(solid.gameObject.layer,player.gameObject.layer),
                pairIgnored = Physics2D.GetIgnoreCollision(solid,player),
                solidLayerSimulated = HasLayer(Physics2D.simulationLayers,solid.gameObject.layer),
                playerLayerSimulated = HasLayer(Physics2D.simulationLayers,player.gameObject.layer),
                solidSendsToPlayer = HasLayer(solid.forceSendLayers,player.gameObject.layer),
                playerReceivesFromSolid = HasLayer(player.forceReceiveLayers,solid.gameObject.layer)
            };
            return BlockingIssue(facts);
        }

        public static string NewSupportIssue(Transform renderer, Collider2D player)
        {
            if (renderer.GetComponentInParent<Rigidbody2D>() != null || HasEffector(renderer) ||
                renderer.GetComponentInParent<CompositeCollider2D>() != null)
                return "New support would inherit unexpected Rigidbody/composite/effector ownership.";
            var body = player.attachedRigidbody;
            if (!player.enabled || !player.gameObject.activeInHierarchy || player.isTrigger || body == null ||
                !body.simulated || body.bodyType != RigidbodyType2D.Dynamic || HasLayerOverrides(player) ||
                body.includeLayers.value != 0 || body.excludeLayers.value != 0 || player.usedByEffector ||
                HasEffector(player.transform) || player.composite != null || player.compositeOperation != Collider2D.CompositeOperation.None)
                return "Player collision configuration is outside the supported creation contract.";
            if (Physics2D.GetIgnoreLayerCollision(renderer.gameObject.layer,player.gameObject.layer) ||
                !HasLayer(Physics2D.simulationLayers,renderer.gameObject.layer) ||
                !HasLayer(Physics2D.simulationLayers,player.gameObject.layer) ||
                !HasLayer(player.forceReceiveLayers,renderer.gameObject.layer))
                return "New support layer does not provide enabled, simulated separation against Player.";
            return null;
        }

        private static bool HasLayerOverrides(Collider2D collider) =>
            collider.includeLayers.value != 0 || collider.excludeLayers.value != 0 || collider.layerOverridePriority != 0;
        private static bool HasLayer(LayerMask mask, int layer) => (mask.value & (1 << layer)) != 0;
        private static bool HasEffector(Transform t)
        {
            for (var p = t; p != null; p = p.parent) if (p.GetComponent<Effector2D>() != null) return true;
            return false;
        }

        public static bool SupportsOverlap(Rect a, Rect b, float epsilon = .001f) =>
            Mathf.Min(a.xMax,b.xMax) - Mathf.Max(a.xMin,b.xMin) > epsilon &&
            Mathf.Min(a.yMax,b.yMax) - Mathf.Max(a.yMin,b.yMin) > epsilon;

        public static bool IsKnownWalkableFloorAsset(string path) =>
            path == TownKeyartComponentSupportCatalog.WorldRoot + "tiles/ground_deck.png" ||
            path == TownKeyartComponentSupportCatalog.WorldRoot + "tiles/ground_grass.png";

        public static bool RequiresExplicitSupportBinding(bool ownDedicatedSupport, bool ancestorOrWithinProp,
            bool verifiedIndependentSupport, bool overlapsExpectedBase)
        {
            if (ownDedicatedSupport) return false;
            if (ancestorOrWithinProp) return true;
            return overlapsExpectedBase && !verifiedIndependentSupport;
        }

        public static bool SupportedLocalTransform(Vector3 scale, Quaternion rotation) =>
            FinitePositive(scale.x) && FinitePositive(scale.y) && FinitePositive(scale.z) &&
            Quaternion.Angle(rotation,Quaternion.identity) < .001f;

        public static bool SupportedPresentationChain(Transform t)
        {
            for (var p = t; p != null; p = p.parent)
                if (!SupportedLocalTransform(p.localScale,p.localRotation)) return false;
            return true;
        }
        private static bool FinitePositive(float value) => !float.IsNaN(value) && !float.IsInfinity(value) && value > .0001f;
    }
}
