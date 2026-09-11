using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Editor.SceneCreation
{
    /// <summary>
    /// Reviewed-source, Town-only interior presentation candidate. Keeps prop roots/IDs/triggers and
    /// existing RoofReveal renderer references. Plans useful corridors before applying visual changes.
    /// </summary>
    public static class TownKeyartInteriorPresentation
    {
        [Serializable]
        public sealed class Placement
        {
            public string house, prop, source, state;
            public float visualWorldWidth;
            public Vector2 supportWorldCenter;
            public Rect[] supportWorldRects = Array.Empty<Rect>();
            [NonSerialized] internal SpriteRenderer renderer;
            [NonSerialized] internal Sprite sprite;
            [NonSerialized] internal TownKeyartComponentSupportCatalog.Recipe recipe;
        }

        [Serializable]
        public sealed class Plan
        {
            public string status = "CANDIDATE_NOT_OBSERVED_IN_UNITY";
            public List<Placement> placements = new List<Placement>();
            public List<string> conflicts = new List<string>();
            public List<string> pending = new List<string>();
        }

        /// <summary>No scene mutation. Checks candidate supports against authored useful room/routes.</summary>
        public static Plan BuildPlan(Scene scene)
        {
            TownKeyartComponentPreflight.RequireTown(scene);
            var plan = new Plan();
            var verifiedSources = new Dictionary<string,bool>();
            foreach (var root in scene.GetRootGameObjects())
            foreach (var house in root.GetComponentsInChildren<Transform>(true))
            {
                if (house.name == "House_AnimalYard" || !TownCityLayout.TryGetBuilding(house.name,out var lot)) continue;
                if (house.lossyScale != Vector3.one || !TownKeyartComponentPreflight.SupportedPresentationChain(house))
                { plan.conflicts.Add(house.name + ": transformed room is unsupported; require positive unrotated parent chain and unit world room scale."); continue; }
                foreach (Transform prop in house)
                {
                    var visual = prop.Find("Visual");
                    var renderer = visual == null ? null : visual.GetComponent<SpriteRenderer>();
                    if (renderer == null) continue;
                    if (!TownKeyartComponentPreflight.SupportedPresentationChain(renderer.transform))
                    { plan.conflicts.Add(house.name + "/" + prop.name + ": negative, degenerate or rotated transform chain is unsupported; no mutation."); continue; }
                    string source = TownKeyartComponentSupportCatalog.ProposedInteriorAsset(prop.name);
                    if (source == null)
                    {
                        plan.pending.Add(house.name + "/" + prop.name + ": matching source not measured; no substitution.");
                        continue;
                    }
                    var recipe = TownKeyartComponentSupportCatalog.Find(source);
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(source);
                    if (sprite == null || recipe == null || sprite.rect.width != recipe.Width || sprite.rect.height != recipe.Height ||
                        !SourceVerified(recipe,verifiedSources))
                    { plan.pending.Add(house.name + "/" + prop.name + ": source missing or native dimensions/hash changed."); continue; }
                    float width = TargetWidth(prop.name);
                    var center = (Vector2)prop.position;
                    // A central 1.4u aisle remains free. These are Visual offsets, not trigger/ID moves.
                    if (prop.name == "Furniture_KitchenCounter") center.x = house.position.x - 1.6f;
                    if (prop.name == "Furniture_Pew") center.x = house.position.x - 2.1f;
                    if (prop.name.StartsWith("Station_",StringComparison.Ordinal)) center += new Vector2(.3f,.65f);
                    // Candidate-only clearance for the Sewing station: preserve the authored
                    // scene placement while keeping its reviewed support lane off the counter.
                    if (prop.name == "Station_Sewing") center.x += .9f;
                    var placement = new Placement
                    {
                        house = house.name, prop = prop.name, source = source, state = "PLANNED",
                        visualWorldWidth = width, supportWorldCenter = center,
                        renderer = renderer, sprite = sprite, recipe = recipe
                    };
                    placement.supportWorldRects = PlannedRects(placement);
                    // Alchemy/Sewing use a reviewed source binding only. Their existing station
                    // trigger/solid remains the authoritative scene support; do not invent a new
                    // candidate footprint (or move authored furniture) for an unowned silhouette.
                    if (prop.name == "Station_Alchemy" || prop.name == "Station_Sewing")
                        placement.supportWorldRects = Array.Empty<Rect>();
                    plan.placements.Add(placement);
                    var useful = new Rect(lot.Center - (lot.Size - Vector2.one * 2f) * .5f, lot.Size - Vector2.one * 2f);
                    foreach (var support in placement.supportWorldRects)
                        if (!useful.Contains(support.min) || !useful.Contains(support.max))
                            plan.conflicts.Add(house.name + "/" + prop.name + ": support crosses the authored useful room boundary.");
                }
                CheckRoutes(plan,lot);
            }
            plan.conflicts.AddRange(PairwiseSupportConflicts(plan.placements));
            return plan;
        }

        /// <summary>Explicit opt-in generator call. Refuses conflicting candidate geometry before changes.</summary>
        public static Plan ApplyCandidate(Scene scene)
        {
            var plan = BuildPlan(scene);
            if (plan.conflicts.Count > 0)
                throw new InvalidOperationException("Town interior candidate has " + plan.conflicts.Count + " placement/aisle conflicts. Inspect BuildPlan; resolve content placement before physics.");
            // Complete transform preflight before the first renderer is changed, including double-negative
            // ancestors whose final lossyScale alone would misleadingly look positive.
            foreach (var placement in plan.placements)
                if (!TownKeyartComponentPreflight.SupportedPresentationChain(placement.renderer.transform))
                    throw new InvalidOperationException("Unsupported presentation chain " + placement.house + "/" + placement.prop);
            foreach (var placement in plan.placements)
            {
                var renderer = placement.renderer;
                var sprite = placement.sprite;
                var parent = renderer.transform.parent;
                float worldScale = placement.visualWorldWidth * sprite.pixelsPerUnit / sprite.rect.width;
                var parentScale = parent.lossyScale;
                renderer.sprite = sprite;
                renderer.color = Color.white;
                renderer.drawMode = SpriteDrawMode.Simple;
                renderer.transform.localScale = new Vector3(worldScale / parentScale.x,worldScale / parentScale.y,1f);
                var nativeCenter = SupportNativeCenter(placement.recipe);
                var local = new Vector2(nativeCenter.x - sprite.pivot.x,sprite.rect.height - nativeCenter.y - sprite.pivot.y) / sprite.pixelsPerUnit;
                if (renderer.flipX) local.x = -local.x;
                if (renderer.flipY) local.y = -local.y;
                renderer.transform.position = (Vector3)placement.supportWorldCenter - renderer.transform.TransformVector(local);
                // Preserve renderer.enabled and the same component referenced by RoofReveal.
                EditorUtility.SetDirty(renderer);
                EditorUtility.SetDirty(renderer.transform);
                placement.state = "APPLIED_PRESENTATION_CANDIDATE";
            }
            return plan;
        }

        /// <summary>Pure candidate geometry check, independent of authored routes. Flat rugs have no supports.</summary>
        public static List<string> PairwiseSupportConflicts(IReadOnlyList<Placement> placements)
        {
            var conflicts = new List<string>();
            for (int i = 0; i < placements.Count; i++)
            for (int j = i + 1; j < placements.Count; j++)
            {
                var a = placements[i]; var b = placements[j];
                if (a.house != b.house) continue;
                bool overlap = false;
                foreach (var supportA in a.supportWorldRects)
                foreach (var supportB in b.supportWorldRects)
                    overlap |= TownKeyartComponentPreflight.SupportsOverlap(supportA,supportB);
                if (overlap) conflicts.Add(a.house + ": bases of " + a.prop + " and " + b.prop + " overlap outside/inside routes. Reposition content before adding solids.");
            }
            return conflicts;
        }

        private static float TargetWidth(string name)
        {
            switch (name)
            {
                case "Bed": case "GuestBed_Inn": return 1.6f;
                case "Table": return 1.4f;
                case "Furniture_Shelf": return 1f;
                case "Furniture_Cupboard": return .8f;
                case "Furniture_Rug": return 2.1f;
                case "Furniture_KitchenCounter": return 1.8f;
                case "Furniture_Stove": return 1f;
                case "Furniture_Altar": return 1.8f;
                case "Furniture_Pew": return 1.8f;
                case "Furniture_ServiceCounter": return 2.2f;
                case "Station_Forge": case "Station_CookingStation": case "Station_Alchemy": return 1.6f;
                case "Station_Sewing": return 1.8f;
                case "Station_Carpentry": case "Station_Workbench": return 1.8f;
                default: throw new ArgumentException("No reviewed candidate sizing for " + name);
            }
        }

        private static Vector2 SupportNativeCenter(TownKeyartComponentSupportCatalog.Recipe recipe)
        {
            if (recipe.Supports.Length == 0) return new Vector2(recipe.Width * .5f,recipe.Height * .5f);
            var min = recipe.Supports[0].min; var max = recipe.Supports[0].max;
            foreach (var support in recipe.Supports) { min = Vector2.Min(min,support.min); max = Vector2.Max(max,support.max); }
            return (min + max) * .5f;
        }

        private static Rect[] PlannedRects(Placement placement)
        {
            var nativeCenter = SupportNativeCenter(placement.recipe);
            float unitsPerPixel = placement.visualWorldWidth / placement.recipe.Width;
            var result = new Rect[placement.recipe.Supports.Length];
            for (int i = 0; i < result.Length; i++)
            {
                var native = placement.recipe.Supports[i];
                var delta = new Vector2(native.center.x - nativeCenter.x,nativeCenter.y - native.center.y) * unitsPerPixel;
                if (placement.renderer.flipX) delta.x = -delta.x;
                if (placement.renderer.flipY) delta.y = -delta.y;
                var center = placement.supportWorldCenter + delta;
                var size = native.size * unitsPerPixel;
                result[i] = new Rect(center - size * .5f,size);
            }
            return result;
        }

        private static bool SourceVerified(TownKeyartComponentSupportCatalog.Recipe recipe, Dictionary<string,bool> verified)
        {
            if (verified.TryGetValue(recipe.Asset,out bool valid)) return valid;
            if (!File.Exists(recipe.Asset)) return false;
            using (var stream = File.OpenRead(recipe.Asset))
            using (var sha = SHA256.Create())
                valid = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "") == recipe.Sha256;
            verified[recipe.Asset] = valid; return valid;
        }

        private static void CheckRoutes(Plan plan, TownBuildingLot lot)
        {
            // Candidate paths use the conservative existing 0.35u body envelope and an extra aisle margin.
            var center = lot.Center;
            var entry = (Vector2)lot.DoorPosition;
            var insideDoor = new Vector2(entry.x,lot.MinY + 1.1f);
            CheckSegment(plan,lot.Name,"door-to-center",entry,insideDoor,.7f);
            CheckSegment(plan,lot.Name,"door-to-center",insideDoor,center,.7f);
            foreach (var target in plan.placements)
            {
                if (target.house != lot.Name || (target.prop != "Bed" && target.prop != "GuestBed_Inn" &&
                    !target.prop.StartsWith("Station_",StringComparison.Ordinal)) || target.supportWorldRects.Length == 0) continue;
                float front = float.MaxValue;
                foreach (var rect in target.supportWorldRects) front = Mathf.Min(front,rect.yMin);
                var approach = new Vector2(target.supportWorldCenter.x,front - .5f);
                var turn = new Vector2(center.x,approach.y);
                CheckSegment(plan,lot.Name,"center-to-front-of-" + target.prop,center,turn,.35f);
                CheckSegment(plan,lot.Name,"center-to-front-of-" + target.prop,turn,approach,.35f);
            }
        }

        private static void CheckSegment(Plan plan, string house, string route, Vector2 start, Vector2 end, float radius)
        {
            foreach (var placement in plan.placements)
            {
                if (placement.house != house) continue;
                foreach (var rect in placement.supportWorldRects)
                    if (TownKeyartGeometry.SegmentRectDistance(start,end,rect.center,rect.size) < radius)
                        plan.conflicts.Add(house + ": planned " + route + " intersects support of " + placement.prop + ". Reposition content; do not omit its collider.");
            }
        }
    }
}
