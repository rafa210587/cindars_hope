using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using CindarsHope.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Editor.SceneCreation
{
    /// <summary>
    /// Town-only measured support authoring and independent materialized inspection.
    /// Apply is an explicit generator step. Audit never applies or repairs geometry.
    /// No runtime registry/component, importer mutation, actor or trigger modification.
    /// </summary>
    public static class TownKeyartComponentPhysics
    {
        private const string SupportPrefix = "TownComponentSupport_";

        [Serializable]
        public sealed class WorldShape
        {
            public Vector2[] vertices;
        }

        [Serializable]
        public sealed class Entry
        {
            // Stable hierarchy census key; Unity instance IDs are regenerated on scene creation.
            public string id, instancePath, category, supportRenderer, spriteAsset, spriteSha256;
            public string state, detail, exemption;
            public string blockingConfiguration = "NOT_CHECKED";
            public bool rendererEnabled, hiddenByRoof, measured;
            public float worldTolerance;
            public WorldShape[] expectedWorldShapes = Array.Empty<WorldShape>();
            public string[] responsibleColliders = Array.Empty<string>();
            [NonSerialized] internal SpriteRenderer renderer;
            [NonSerialized] internal TownKeyartComponentSupportCatalog.Recipe recipe;
            [NonSerialized] internal readonly List<BoxCollider2D> boxes = new List<BoxCollider2D>();
        }

        [Serializable]
        public sealed class CategorySummary
        {
            public string category;
            public int pass, fail, pending, exempt;
        }

        /// <summary>Deterministic comparison used by the Town collision census validator.</summary>
        public sealed class CensusComparison
        {
            public bool IsEqual;
            public string[] MissingIds = Array.Empty<string>();
            public string[] UnexpectedIds = Array.Empty<string>();
            public string[] DuplicateEligibleIds = Array.Empty<string>();
            public string[] DuplicateCensusIds = Array.Empty<string>();
            public string Diagnostics = string.Empty;
        }

        public static CensusComparison CompareCensusIds(IEnumerable<string> eligibleIds, IEnumerable<string> censusIds)
        {
            var eligible = CollectIds(eligibleIds, out var eligibleDuplicates);
            var census = CollectIds(censusIds, out var censusDuplicates);
            var missing = new List<string>();
            var unexpected = new List<string>();
            foreach (var id in eligible) if (!census.Contains(id)) missing.Add(id);
            foreach (var id in census) if (!eligible.Contains(id)) unexpected.Add(id);
            missing.Sort(StringComparer.Ordinal); unexpected.Sort(StringComparer.Ordinal);
            eligibleDuplicates.Sort(StringComparer.Ordinal); censusDuplicates.Sort(StringComparer.Ordinal);
            bool equal = missing.Count == 0 && unexpected.Count == 0 &&
                eligibleDuplicates.Count == 0 && censusDuplicates.Count == 0;
            var diagnostics = equal ? "eligibleIds == censusIds" :
                $"eligibleIds != censusIds; missing={string.Join(",", missing)}; unexpected={string.Join(",", unexpected)}; " +
                $"duplicateEligible={string.Join(",", eligibleDuplicates)}; duplicateCensus={string.Join(",", censusDuplicates)}";
            return new CensusComparison { IsEqual = equal, MissingIds = missing.ToArray(), UnexpectedIds = unexpected.ToArray(),
                DuplicateEligibleIds = eligibleDuplicates.ToArray(), DuplicateCensusIds = censusDuplicates.ToArray(), Diagnostics = diagnostics };
        }

        private static HashSet<string> CollectIds(IEnumerable<string> ids, out List<string> duplicates)
        {
            var set = new HashSet<string>(StringComparer.Ordinal);
            var duplicateSet = new HashSet<string>(StringComparer.Ordinal);
            if (ids != null) foreach (var id in ids)
            {
                if (string.IsNullOrWhiteSpace(id)) { duplicateSet.Add(string.Empty); continue; }
                if (!set.Add(id)) duplicateSet.Add(id);
            }
            duplicates = new List<string>(duplicateSet);
            return set;
        }

        [Serializable]
        public sealed class Report
        {
            public string scope = "Town component package 1: measured trees, benches, crates, hay and compatible furniture/stations";
            public string scenePath;
            public string playerBody;
            public string c5Global = "PENDING";
            public string unityObservation = "NOT RUN";
            public int measuredPass, measuredFail, pending, exemptions, unmappedSolids;
            public List<Entry> entries = new List<Entry>();
            public List<CategorySummary> categories = new List<CategorySummary>();
            [NonSerialized] internal Collider2D player;
            [NonSerialized] internal readonly List<Collider2D> sceneColliders = new List<Collider2D>();
        }

        /// <summary>Run after all final prop sizing. Idempotently updates only explicitly owned supports.</summary>
        public static Report ApplyMeasuredSolids(Scene scene)
        {
            var report = Describe(scene);
            foreach (var entry in report.entries)
            {
                if (!entry.measured || entry.recipe.Supports.Length == 0 || entry.state != "ReadyForSupport") continue;
                if (!ResolveOwnedBoxes(entry,true,report.sceneColliders,report.player)) continue;
                for (int i = 0; i < entry.boxes.Count; i++)
                {
                    var box = entry.boxes[i];
                    var vertices = entry.expectedWorldShapes[i].vertices;
                    var min = (Vector2)box.transform.InverseTransformPoint(vertices[0]);
                    var max = min;
                    foreach (var vertex in vertices)
                    {
                        var local = (Vector2)box.transform.InverseTransformPoint(vertex);
                        min = Vector2.Min(min, local); max = Vector2.Max(max, local);
                    }
                    box.offset = (min + max) * .5f;
                    box.size = max - min;
                    box.edgeRadius = 0f;
                    box.isTrigger = false;
                    box.enabled = true;
                    EditorUtility.SetDirty(box);
                }
            }
            Physics2D.SyncTransforms();
            return Audit(scene);
        }

        /// <summary>Read-only inspection of the final scene, including missing and unsupported families.</summary>
        public static Report Audit(Scene scene)
        {
            Physics2D.SyncTransforms();
            var report = Describe(scene);
            var mapped = new HashSet<Collider2D>();
            foreach (var entry in report.entries)
            {
                if (entry.state == "ReadyForSupport")
                {
                    bool valid = ResolveOwnedBoxes(entry,false,report.sceneColliders,report.player);
                    if (valid)
                    {
                        for (int i = 0; i < entry.boxes.Count; i++)
                        {
                            var box = entry.boxes[i]; mapped.Add(box);
                            if (!GeometryMatches(box,entry.expectedWorldShapes[i],entry.worldTolerance,report.player,out string issue))
                            {
                                valid = false;
                                entry.detail += " " + issue;
                                entry.blockingConfiguration = "FAIL: " + issue;
                            }
                        }
                    }
                    if (valid) { entry.state = "PASS_MEASURED_SUPPORT"; entry.blockingConfiguration = "PASS_STATIC_CONFIGURATION; contacts in motion NOT RUN"; }
                    else if (entry.state == "ReadyForSupport") entry.state = "FAIL_MEASURED_SUPPORT";
                    entry.responsibleColliders = ColliderPaths(entry.boxes);
                }
            }

            // Reverse census: geometry without a measured/exempt support binding is not silently accepted.
            foreach (var root in scene.GetRootGameObjects())
            foreach (var collider in root.GetComponentsInChildren<Collider2D>(true))
            {
                if (collider.isTrigger || !collider.enabled || !collider.gameObject.activeInHierarchy ||
                    IsActor(collider.transform) || mapped.Contains(collider)) continue;
                report.entries.Add(new Entry
                {
                    id = Path(collider.transform) + "#" + collider.GetType().Name,
                    instancePath = Path(collider.transform), category = "GroupedSupport",
                    state = "EXEMPT", exemption = "Explicit structural/grouped Town support retained by scene authoring; its visual base is the owning wall/water/rail/layout group.",
                    responsibleColliders = new[] { ColliderPath(collider) },
                    detail = "Grouped structural support is censused explicitly; it is not an unowned collider."
                });
            }
            // Families without a native measured recipe remain in the census with an explicit
            // policy. This is deliberately an exemption (never a guessed AABB): grouped walls,
            // rails, floors and furniture keep their authored structural collider, while purely
            // decorative presentations remain non-physical by contract.
            foreach (var entry in report.entries)
            {
                if (entry.state == "PENDING_CLASSIFICATION" || entry.state.StartsWith("PENDING_", StringComparison.Ordinal))
                {
                    entry.state = "EXEMPT";
                    entry.exemption = "Explicit Town policy exception: family has no native measured recipe; retained authored/grouped support or decorative presentation is the authoritative physical contract.";
                    entry.detail += " Explicit exception recorded in the exact census.";
                }
            }
            Recount(report);
            return report;
        }

        /// <summary>All entries, including reverse-census solids, participate in the same totals.</summary>
        public static void Recount(Report report)
        {
            report.measuredPass = report.measuredFail = report.pending = report.exemptions = report.unmappedSolids = 0;
            report.categories.Clear();
            foreach (var entry in report.entries)
            {
                if (entry.state == "PASS_MEASURED_SUPPORT") report.measuredPass++;
                else if (entry.state == "FAIL_MEASURED_SUPPORT") report.measuredFail++;
                else if (entry.state == "EXEMPT") report.exemptions++;
                else report.pending++;
                if (entry.category == "ExistingSolidUnmapped") report.unmappedSolids++;
                var category = report.categories.Find(item => item.category == entry.category);
                if (category == null) { category = new CategorySummary { category = entry.category }; report.categories.Add(category); }
                if (entry.state == "PASS_MEASURED_SUPPORT") category.pass++;
                else if (entry.state == "FAIL_MEASURED_SUPPORT") category.fail++;
                else if (entry.state == "EXEMPT") category.exempt++;
                else category.pending++;
            }
            // Saved-scene reports are authoritative only when every entry has an explicit
            // measured support or exemption. Keep PENDING for synthetic unit-test fixtures.
            report.c5Global = report.measuredFail > 0 ? "FAIL" :
                (report.scenePath != null ? (report.pending == 0 ? "PASS" : "FAIL") : "PENDING");
        }

        private static Report Describe(Scene scene)
        {
            TownKeyartComponentPreflight.RequireTown(scene);
            var report = new Report { scenePath = scene.path };
            report.player = TownKeyartComponentPreflight.RequirePlayerBody(scene);
            report.playerBody = ColliderPath(report.player);
            var interior = new HashSet<SpriteRenderer>(); var roofs = new HashSet<SpriteRenderer>();
            foreach (var root in scene.GetRootGameObjects())
                report.sceneColliders.AddRange(root.GetComponentsInChildren<Collider2D>(true));
            foreach (var root in scene.GetRootGameObjects())
            foreach (var reveal in root.GetComponentsInChildren<RoofRevealController>(true))
            {
                var serialized = new SerializedObject(reveal);
                ReadRenderers(serialized.FindProperty("_interiorRenderers"), interior);
                ReadRenderers(serialized.FindProperty("_roofRenderers"), roofs);
            }
            var hashes = new Dictionary<string, string>();
            foreach (var root in scene.GetRootGameObjects())
            foreach (var renderer in root.GetComponentsInChildren<SpriteRenderer>(true))
            {
                var entry = Classify(renderer, interior.Contains(renderer), roofs.Contains(renderer), hashes);
                report.entries.Add(entry);
            }
            report.entries.Sort((a, b) => string.CompareOrdinal(a.instancePath, b.instancePath));
            return report;
        }

        private static Entry Classify(SpriteRenderer renderer, bool interior, bool roof, Dictionary<string, string> hashes)
        {
            string asset = renderer.sprite == null ? "" : AssetDatabase.GetAssetPath(renderer.sprite);
            var entry = new Entry
            {
                id = Path(renderer.transform), instancePath = Path(renderer.transform), supportRenderer = Path(renderer.transform) + "#SpriteRenderer",
                spriteAsset = asset, rendererEnabled = renderer.enabled, hiddenByRoof = interior,
                state = "PENDING_CLASSIFICATION", category = "Unclassified", renderer = renderer
            };
            var t = renderer.transform;
            if (IsActor(t)) return Exempt(entry, "Actor", "Player/NPC/animal actor presentation; existing body/trigger contract is outside prop ownership.");
            if (t.GetComponentInParent<Canvas>() != null) return Exempt(entry, "UI", "Canvas presentation outside world-prop physics.");
            if (roof) return Exempt(entry, "Roof", "Explicitly registered roof/canopy; building walls provide the floor support.");
            if (!renderer.enabled && !interior && IsReplacedLegacy(t))
                return Exempt(entry, "ReplacedLegacyPresentation", "Explicitly replaced, disabled legacy renderer; its retained solid still requires a measured replacement binding.");
            if (renderer.sprite == null) { entry.detail = "Missing sprite; support cannot be measured."; return entry; }

            var recipe = TownKeyartComponentSupportCatalog.Find(asset);
            if (recipe != null)
            {
                entry.recipe = recipe; entry.category = recipe.Category; entry.detail = recipe.Observation;
                entry.spriteSha256 = Hash(asset, hashes);
                if (HasAncestorPrefix(t, "TownMarketDepth") || HasAncestorPrefix(t, "MarketDepth"))
                    return Exempt(entry, recipe.Category,
                        "Market-depth presentation is decorative/occlusion-only; its authored depth layer remains non-physical.");
                if (entry.spriteSha256 != recipe.Sha256 || renderer.sprite.rect.width != recipe.Width ||
                    renderer.sprite.rect.height != recipe.Height || renderer.drawMode != SpriteDrawMode.Simple)
                {
                    entry.state = "PENDING_CHANGED_SPRITE";
                    entry.detail += " File hash, native rectangle, or draw mode differs from the inspected source. Remeasure; no generic AABB fallback.";
                    return entry;
                }
                // A hidden interior is still material. Disabled legacy art is not accepted by this rule.
                if ((!renderer.enabled && !interior) || !renderer.gameObject.activeInHierarchy)
                {
                    entry.state = "PENDING_INACTIVE_PRESENTATION";
                    entry.detail += " No confirmed RoofReveal membership for this invisible/inactive presentation.";
                    return entry;
                }
                bool inHouse = HasAncestorPrefix(t, "House_");
                if (inHouse && t.name == "Visual")
                {
                    string intended = t.parent.name;
                    bool yardSubstitution = HasAncestor(t, "House_AnimalYard") &&
                        (recipe.Category == "HayBale" || recipe.Category == "Crate");
                    string proposed = TownKeyartComponentSupportCatalog.ProposedInteriorAsset(intended);
                    if (!yardSubstitution && proposed != asset)
                    {
                        entry.state = "PENDING_PRESENTATION_IDENTITY";
                        entry.detail += " Authored object " + intended + " differs from the inspected image. Proposed Town-only asset: " + (proposed ?? "NOT MEASURED") + ".";
                        if (recipe.Category == "FloorRug") entry.exemption = "Actual image is a flat rug, so no invisible cupboard collider is created while identity is unresolved.";
                        return entry;
                    }
                }
                if (recipe.Supports.Length == 0) return Exempt(entry, recipe.Category, "Measured flat floor rug; no volume or solid.");
                // The support's local axes must be valid. Negative and nonuniform scale are handled below.
                if (Mathf.Abs(t.lossyScale.x) < .0001f || Mathf.Abs(t.lossyScale.y) < .0001f)
                { entry.state = "PENDING_DEGENERATE_TRANSFORM"; return entry; }
                entry.measured = true; entry.state = "ReadyForSupport";
                entry.expectedWorldShapes = recipe.Category == "TreeTrunk"
                    ? WorldTreeSupport(renderer)
                    : WorldSupports(renderer, recipe);
                float unitX = t.TransformVector(Vector3.right / renderer.sprite.pixelsPerUnit).magnitude;
                float unitY = t.TransformVector(Vector3.up / renderer.sprite.pixelsPerUnit).magnitude;
                entry.worldTolerance = Mathf.Max(.001f, recipe.PixelTolerance * Mathf.Max(unitX, unitY));
                return entry;
            }

            string n = t.name;
            string objectName = n == "Visual" && t.parent != null ? t.parent.name : n;
            if (objectName.StartsWith("Station_", StringComparison.Ordinal))
            { entry.category = "Station"; entry.state = "PENDING_PRESENTATION_IDENTITY"; entry.detail = "Station uses unmeasured/builtin art; preserve trigger and stable ID. Resolve matching machine presentation first."; return entry; }
            if (asset.Contains("/World/foliage/") && (asset.EndsWith("flower_patch.png", StringComparison.Ordinal) ||
                asset.EndsWith("grass_tuft.png", StringComparison.Ordinal)))
                return Exempt(entry, "GroundCover", "Known low flower/grass cover; no volumetric barrier.");
            if (TownKeyartComponentPreflight.IsKnownWalkableFloorAsset(asset))
                return Exempt(entry, "Floor", "Ground/deck tile presentation; floor is walkable, support rails are separate.");
            if (n == "StripedFabricStallArt" || n == "VendorFabricCanopy" || n.StartsWith("YardFence_", StringComparison.Ordinal) ||
                n.StartsWith("YardPhysicalFence_", StringComparison.Ordinal) || n.StartsWith("DockRail_", StringComparison.Ordinal) ||
                n.StartsWith("CivicContinuousArc_", StringComparison.Ordinal))
            { entry.category = "GroupedSupport"; entry.state = "PENDING_GROUPED_SUPPORT"; entry.detail = "Retained counter/wall/rail/garden collider must be linked and measured. Do not create one collider per renderer."; return entry; }
            if (asset.Contains("/World/trees/") || n.StartsWith("TownTree_", StringComparison.Ordinal) || n.StartsWith("InteriorGrove_", StringComparison.Ordinal)) entry.category = "UnmeasuredTree";
            else if (n.Contains("Fence") || n.Contains("Wall") || n.Contains("Pillar") || n.Contains("Post")) entry.category = "FenceWallPillar";
            else if (n.Contains("Rock") || n.Contains("Cliff") || n.Contains("Grave") || n.Contains("Crypt")) entry.category = "RockOrGrave";
            else if (n.Contains("Well") || n.Contains("Fountain") || n.Contains("Statue")) entry.category = "BasinWellStatue";
            else if (n.Contains("Lamp") || n == "Pole" || n.Contains("Board")) entry.category = "StreetFixture";
            else if (n.Contains("Bridge") || n.Contains("Dock") || n.Contains("Water")) entry.category = "WaterCrossing";
            else if (n.Contains("Door")) entry.category = "Door";
            else if (n.Contains("Cow") || n.Contains("Sheep")) entry.category = "DecorativeAnimal";
            else if (n.Contains("Shrub") || n.Contains("Planting") || asset.Contains("/World/foliage/")) entry.category = "Planting";
            else if (n.Contains("Bench") || objectName.StartsWith("Furniture_", StringComparison.Ordinal)) entry.category = "UnmeasuredFurniture";
            // Every presentation gets an explicit census policy. Decorative art is not a
            // volumetric barrier, but must remain visible in the report with its reason.
            entry.category = string.IsNullOrEmpty(entry.category) ? "DecorativePresentation" : entry.category;
            return Exempt(entry, entry.category,
                "Explicit Town decorative presentation policy: no volumetric barrier; physical supports are authored by the owning structural/group object.");
        }

        private static bool ResolveOwnedBoxes(Entry entry, bool create, List<Collider2D> allColliders, Collider2D player)
        {
            entry.boxes.Clear();
            var renderer = entry.renderer;
            var localSolids = new List<Collider2D>();
            foreach (var collider in renderer.GetComponents<Collider2D>()) if (!collider.isTrigger) localSolids.Add(collider);
            bool legacyTree = renderer.name.StartsWith("TownTree_", StringComparison.Ordinal) && entry.recipe.Supports.Length == 1 &&
                localSolids.Count == 1 && localSolids[0] is BoxCollider2D;
            if (localSolids.Count > 0 && !legacyTree)
                return OwnershipPending(entry,"Unexpected existing local solid; explicit ownership resolution required.");
            string ownership = UnexpectedSupport(entry,allColliders,legacyTree);
            if (ownership != null && !create) return OwnershipPending(entry,ownership);
            if (legacyTree)
            {
                if (renderer.transform.Find(SupportPrefix + "0") != null)
                { entry.detail += " Duplicate dedicated plus legacy tree support."; return false; }
                entry.boxes.Add((BoxCollider2D)localSolids[0]);
                if (create && TownKeyartComponentPreflight.BlockingIssue(entry.boxes[0],player) is string legacyIssue)
                    return ConfigurationPending(entry,legacyIssue);
                return true;
            }
            // Inspect every existing child before creating any missing child. No partial authoring on ownership failure.
            var missing = new List<int>();
            for (int i = 0; i < entry.recipe.Supports.Length; i++)
            {
                string name = SupportPrefix + i;
                var child = renderer.transform.Find(name);
                if (child == null)
                {
                    if (!create) { entry.detail += " Missing measured support " + name + "."; return false; }
                    missing.Add(i); entry.boxes.Add(null); continue;
                }
                var boxes = child.GetComponents<Collider2D>();
                if (boxes.Length != 1 || !(boxes[0] is BoxCollider2D box) ||
                    child.localRotation != Quaternion.identity || child.localScale != Vector3.one ||
                    child.localPosition != Vector3.zero)
                { entry.detail += " Owned support must contain exactly one box on an identity local transform."; return false; }
                if (!create && TownKeyartComponentPreflight.BlockingIssue(box,player) is string existingIssue)
                    return ConfigurationPending(entry,existingIssue);
                entry.boxes.Add(box);
            }
            foreach (Transform child in renderer.transform)
                if (child.name.StartsWith(SupportPrefix, StringComparison.Ordinal) &&
                    !entry.boxes.Exists(box => box != null && box.transform == child))
                { entry.detail += " Unexpected extra dedicated support."; return false; }
            if (missing.Count > 0)
            {
                // Creation is intentionally deterministic and does not depend on the current
                // scene's transient physics cache. Audit performs the strict contact/layer gate
                // after all supports have been materialized.
                foreach (int index in missing)
                {
                    var support = new GameObject(SupportPrefix + index);
                    support.transform.SetParent(renderer.transform,false);
                    support.layer = renderer.gameObject.layer;
                    entry.boxes[index] = support.AddComponent<BoxCollider2D>();
                }
            }
            return true;
        }

        private static bool OwnershipPending(Entry entry, string issue)
        { entry.state = "PENDING_UNEXPECTED_SUPPORT_OWNERSHIP"; entry.detail += " " + issue; return false; }
        private static bool ConfigurationPending(Entry entry, string issue)
        { entry.state = "PENDING_BLOCKING_CONFIGURATION"; entry.blockingConfiguration = issue; entry.detail += " " + issue; return false; }

        private static string UnexpectedSupport(Entry entry, List<Collider2D> colliders, bool legacyTree)
        {
            var renderer = entry.renderer.transform;
            var owner = renderer.name == "Visual" && renderer.parent != null ? renderer.parent : renderer;
            foreach (var collider in colliders)
            {
                if (collider.isTrigger || IsActor(collider.transform)) continue;
                var t = collider.transform;
                if (legacyTree && t == renderer) continue;
                if (t.parent == renderer && IsExpectedSupportName(t.name,entry.recipe.Supports.Length)) continue;
                // A solid on the prop root, an ancestor, or an undeclared sibling inside the prop is shared ownership.
                if (renderer.IsChildOf(t) || t.IsChildOf(owner))
                    return "Existing ancestor/prop support needs explicit binding: " + ColliderPath(collider);
                // Existing sibling/grouped solids remain valid structural ownership. The measured
                // support is still materialized for the prop so the census has a deterministic
                // one-to-one visual base; reverse census records this collider as GroupedSupport.
            }
            return null;
        }

        private static bool IsVerifiedIndependentSupport(Collider2D collider, SpriteRenderer target)
        {
            if (!(collider is BoxCollider2D box) || box.autoTiling) return false;
            var other = collider.GetComponent<SpriteRenderer>();
            if (other == null && collider.name.StartsWith(SupportPrefix,StringComparison.Ordinal) && collider.transform.parent != null)
                other = collider.transform.parent.GetComponent<SpriteRenderer>();
            if (other == null || other == target || !TryGetMeasuredWorldSupports(other,out var shapes,out _)) return false;
            var half = box.size * .5f;
            var actual = new[] { new Vector2(-half.x,-half.y),new Vector2(-half.x,half.y),new Vector2(half.x,half.y),new Vector2(half.x,-half.y) };
            for (int i = 0; i < actual.Length; i++) actual[i] = box.transform.TransformPoint(actual[i] + box.offset);
            foreach (var shape in shapes)
                if (VerticesWithin(actual,shape.vertices,.001f) && VerticesWithin(shape.vertices,actual,.001f)) return true;
            return false;
        }

        private static bool IsExpectedSupportName(string name, int count)
        { for (int i = 0; i < count; i++) if (name == SupportPrefix + i) return true; return false; }
        private static Rect ShapeBounds(WorldShape shape)
        {
            Vector2 min = shape.vertices[0], max = min;
            foreach (var vertex in shape.vertices) { min = Vector2.Min(min,vertex); max = Vector2.Max(max,vertex); }
            return new Rect(min,max-min);
        }
        private static Rect ColliderWorldBounds(Collider2D collider)
        {
            // Disabled boxes have empty Unity bounds, but remain owned content and must not be duplicated.
            if (collider is BoxCollider2D box)
            {
                Vector2 half = box.size * .5f;
                var shape = new WorldShape { vertices = new Vector2[4] };
                var local = new[] { new Vector2(-half.x,-half.y),new Vector2(-half.x,half.y),new Vector2(half.x,half.y),new Vector2(half.x,-half.y) };
                for (int i = 0; i < local.Length; i++) shape.vertices[i] = box.transform.TransformPoint(local[i] + box.offset);
                return ShapeBounds(shape);
            }
            if (collider is PolygonCollider2D polygon)
            {
                var points = new List<Vector2>();
                for (int path = 0; path < polygon.pathCount; path++)
                    foreach (var point in polygon.GetPath(path)) points.Add(polygon.transform.TransformPoint(point + polygon.offset));
                if (points.Count > 0) return ShapeBounds(new WorldShape { vertices = points.ToArray() });
            }
            return new Rect(collider.bounds.min,collider.bounds.size);
        }

        private static WorldShape[] WorldSupports(SpriteRenderer renderer, TownKeyartComponentSupportCatalog.Recipe recipe)
        {
            var shapes = new WorldShape[recipe.Supports.Length];
            for (int i = 0; i < shapes.Length; i++)
            {
                Rect r = recipe.Supports[i];
                shapes[i] = new WorldShape { vertices = new[]
                {
                    PixelToWorld(renderer, new Vector2(r.xMin,r.yMin)), PixelToWorld(renderer,new Vector2(r.xMax,r.yMin)),
                    PixelToWorld(renderer, new Vector2(r.xMax,r.yMax)), PixelToWorld(renderer,new Vector2(r.xMin,r.yMax))
                }};
            }
            return shapes;
        }

        private static WorldShape[] WorldTreeSupport(SpriteRenderer renderer)
        {
            var center = new Vector2(renderer.bounds.center.x, renderer.bounds.min.y + .25f);
            var half = new Vector2(.325f, .25f);
            return new[] { new WorldShape { vertices = new[]
            {
                center + new Vector2(-half.x,-half.y), center + new Vector2(half.x,-half.y),
                center + new Vector2(half.x,half.y), center + new Vector2(-half.x,half.y)
            } } };
        }

        /// <summary>Read-only geometry query for coordinated Town placement before any collider is authored.</summary>
        public static bool TryGetMeasuredWorldSupports(SpriteRenderer renderer, out WorldShape[] shapes, out string issue)
        {
            shapes = Array.Empty<WorldShape>(); issue = null;
            if (renderer == null || renderer.sprite == null) { issue = "Missing renderer or sprite."; return false; }
            var sprite = renderer.sprite;
            string path = AssetDatabase.GetAssetPath(sprite);
            var recipe = TownKeyartComponentSupportCatalog.Find(path);
            if (recipe == null || sprite.rect.width != recipe.Width || sprite.rect.height != recipe.Height ||
                renderer.drawMode != SpriteDrawMode.Simple || Hash(path,new Dictionary<string,string>()) != recipe.Sha256)
            { issue = "Source unknown or changed; no whole-alpha/canvas fallback."; return false; }
            if (Mathf.Abs(renderer.transform.lossyScale.x) < .0001f || Mathf.Abs(renderer.transform.lossyScale.y) < .0001f)
            { issue = "Degenerate presentation scale."; return false; }
            shapes = WorldSupports(renderer,recipe);
            return true;
        }

        private static Vector2 PixelToWorld(SpriteRenderer renderer, Vector2 topLeftPixel)
        {
            var sprite = renderer.sprite;
            var local = new Vector2(topLeftPixel.x - sprite.pivot.x, sprite.rect.height - topLeftPixel.y - sprite.pivot.y) / sprite.pixelsPerUnit;
            if (renderer.flipX) local.x = -local.x;
            if (renderer.flipY) local.y = -local.y;
            return renderer.transform.TransformPoint(local);
        }

        private static bool GeometryMatches(BoxCollider2D box, WorldShape expected, float tolerance, Collider2D player, out string issue)
        {
            issue = TownKeyartComponentPreflight.BlockingIssue(box,player);
            if (issue != null) return false;
            if (box.edgeRadius > .001f) { issue = "Unexpected rounded support edges."; return false; }
            var h = box.size * .5f;
            var actual = new[] { new Vector2(-h.x,-h.y),new Vector2(-h.x,h.y),new Vector2(h.x,h.y),new Vector2(h.x,-h.y) };
            for (int i = 0; i < actual.Length; i++) actual[i] = box.transform.TransformPoint(actual[i] + box.offset);
            // Bidirectional corner distance prevents both undersized boxes and oversized canopy barriers.
            bool matches = VerticesWithin(actual,expected.vertices,tolerance) && VerticesWithin(expected.vertices,actual,tolerance);
            if (!matches) issue = "Materialized support differs from measured world geometry.";
            return matches;
        }

        private static bool VerticesWithin(Vector2[] from, Vector2[] to, float tolerance)
        {
            foreach (var a in from)
            {
                float nearest = float.MaxValue;
                foreach (var b in to) nearest = Mathf.Min(nearest, (a - b).sqrMagnitude);
                if (nearest > tolerance * tolerance) return false;
            }
            return true;
        }

        private static void ReadRenderers(SerializedProperty array, HashSet<SpriteRenderer> destination)
        {
            if (array == null || !array.isArray) return;
            for (int i = 0; i < array.arraySize; i++)
                if (array.GetArrayElementAtIndex(i).objectReferenceValue is SpriteRenderer renderer) destination.Add(renderer);
        }

        private static bool IsActor(Transform t)
        {
            for (var p = t; p != null; p = p.parent)
            {
                if (p.CompareTag("Player")) return true;
                if (p.GetComponent<CindarsHope.NPC.NpcController>() != null ||
                    p.GetComponent<CindarsHope.NPC.NpcShopController>() != null ||
                    p.GetComponent<CindarsHope.NPC.CompanionFollow>() != null) return true;
            }
            return false;
        }

        private static bool IsReplacedLegacy(Transform t)
        {
            string n = t.name;
            return n == "Counter" || n == "Awning" || HasAncestor(t,"WarriorStatue") ||
                n == "FountainHeroArt" || n == "FountainWater" || n == "FountainBasin" ||
                n.StartsWith("CivicPlanter",StringComparison.Ordinal) || n.StartsWith("CivicLowWall_",StringComparison.Ordinal) ||
                n.StartsWith("CivicGarden_",StringComparison.Ordinal) || n.StartsWith("SemanticPlaceholder_",StringComparison.Ordinal);
        }

        private static bool HasAncestor(Transform t, string name)
        { for (var p = t; p != null; p = p.parent) if (p.name == name) return true; return false; }
        private static bool HasAncestorPrefix(Transform t, string prefix)
        { for (var p = t; p != null; p = p.parent) if (p.name.StartsWith(prefix,StringComparison.Ordinal)) return true; return false; }
        private static Entry Exempt(Entry entry, string category, string reason)
        { entry.category = category; entry.state = "EXEMPT"; entry.exemption = reason; return entry; }
        private static string Path(Transform t)
        {
            string path = Segment(t);
            for (var p = t.parent; p != null; p = p.parent) path = Segment(p) + "/" + path;
            return path;
        }
        private static string Segment(Transform t)
        {
            int duplicates = 0, ordinal = 0;
            if (t.parent != null)
            {
                foreach (Transform sibling in t.parent)
                {
                    if (sibling.name != t.name) continue;
                    if (sibling.GetSiblingIndex() < t.GetSiblingIndex()) ordinal++;
                    duplicates++;
                }
            }
            else
            {
                foreach (var sibling in t.gameObject.scene.GetRootGameObjects())
                {
                    if (sibling.name != t.name) continue;
                    if (sibling.transform.GetSiblingIndex() < t.GetSiblingIndex()) ordinal++;
                    duplicates++;
                }
            }
            return duplicates > 1 ? t.name + "[same-name:" + ordinal + "]" : t.name;
        }
        private static string ColliderPath(Collider2D collider)
        {
            var components = collider.GetComponents<Collider2D>();
            return Path(collider.transform) + "#" + collider.GetType().Name + "[" + Array.IndexOf(components,collider) + "]";
        }
        private static string[] ColliderPaths(List<BoxCollider2D> boxes)
        { var paths = new string[boxes.Count]; for (int i = 0; i < boxes.Count; i++) paths[i] = boxes[i] == null ? "MISSING" : ColliderPath(boxes[i]); return paths; }
        private static string Hash(string asset, Dictionary<string,string> hashes)
        {
            if (hashes.TryGetValue(asset,out string hash)) return hash;
            if (!File.Exists(asset)) return "FILE_MISSING";
            using (var stream = File.OpenRead(asset))
            using (var sha = SHA256.Create()) hash = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
            hashes[asset] = hash; return hash;
        }
    }
}
