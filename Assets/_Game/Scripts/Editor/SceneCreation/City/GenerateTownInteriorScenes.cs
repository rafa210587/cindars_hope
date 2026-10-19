using System;
using System.Collections.Generic;
using CindarsHope.Editor.Art;
using CindarsHope.NPC;
using CindarsHope.NPC.Schedule;
using CindarsHope.SceneManagement;
using CindarsHope.World;
using CindarsHope.World.Scenes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Editor.SceneCreation.City
{
    /// <summary>
    /// Bounded S04 pilot authoring. This generator owns only the generated child roots of Renko's
    /// existing S02 prefab; the marker, entry/exit anchors and transition threshold stay intact.
    /// </summary>
    public static class GenerateTownInteriorScenes
    {
        private const string RenkoHouseId = "House_Residential_2";
        private const string RenkoPrefabPath = "Assets/_Game/Prefabs/City/Interiors/House_Residential_2.prefab";
        private const string TownScenePath = "Assets/_Game/Scenes/TownScene.unity";
        private const float PlayerBodyWidth = .65f;
        private const float RequiredClearance = .30f;
        // Generated interior sprites are 128 PPU / mostly 64px tiles. Render tiled surfaces at
        // 48 pixels/world-unit, then inverse their renderer size so authored dimensions stay exact.
        private const float TiledVisualScale = 128f / 48f;
        // Measured from TownScene: Player visual 0.5625 * 4.5 = 2.53u, Renko visual
        // 0.547 * 3.825 = 2.09u. Furniture deliberately reads at that shared scale.
        private static readonly Vector2 RenkoSolidSize = new Vector2(.55f, .40f);
        private static readonly Vector2 RenkoSolidOffset = new Vector2(0f, .273f);

        [MenuItem("Cindar's Hope/Town Interiors/Generate Renko Furnished Pilot")]
        public static void GenerateRenkoPilot()
        {
            var prefabRoot = PrefabUtility.LoadPrefabContents(RenkoPrefabPath);
            try
            {
                var instance = prefabRoot.GetComponent<TownInteriorInstance>();
                if (instance == null || instance.HouseId != RenkoHouseId)
                    throw new InvalidOperationException("Renko pilot prefab is missing its stable TownInteriorInstance marker.");
                if (instance.EntryAnchor == null || instance.ExitAnchor == null)
                    throw new InvalidOperationException("Renko pilot must preserve the S02 entry and exit anchors.");

                var floor = ResetGeneratedRoot(prefabRoot.transform, "Floor");
                var walls = ResetGeneratedRoot(prefabRoot.transform, "Walls");
                var rooms = ResetGeneratedRoot(prefabRoot.transform, "Rooms");
                var props = ResetGeneratedRoot(prefabRoot.transform, "Props");
                var anchors = ResetGeneratedRoot(prefabRoot.transform, "Anchors");

                BuildFloor(floor);
                BuildWalls(walls);
                BuildRooms(rooms);
                var manifest = new List<string>();
                BuildProps(props, manifest);
                var workAnchor = CreateAnchor(anchors, "npc_renko_work", new Vector2(-3.50f, -1.25f));
                var homeAnchor = CreateAnchor(anchors, "npc_renko_home", new Vector2(-3.25f, 3.20f));
                instance.ConfigureNpcAnchors(new[]
                {
                    new TownInteriorInstance.NpcAnchorBinding
                    {
                        NpcId = "npc_renko", AnchorId = "npc_renko_work", Anchor = workAnchor
                    },
                    new TownInteriorInstance.NpcAnchorBinding
                    {
                        NpcId = "npc_renko", AnchorId = "npc_renko_home", Anchor = homeAnchor
                    }
                });

                ValidatePilotLayout(instance, props, workAnchor, homeAnchor);
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, RenkoPrefabPath);
                AssetDatabase.SaveAssets();
                Debug.Log("[GenerateTownInteriorScenes] Renko pilot generated.\n" + string.Join("\n", manifest));
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        /// <summary>
        /// Explicit, narrow Town wiring command. It changes only Renko's opt-in references, adapter and
        /// disabled Renko transition components; never regenerates Town or mutates streets/NPC placement.
        /// Invoke only after the runtime integration owner has approved the pilot wiring.
        /// </summary>
        [MenuItem("Cindar's Hope/Town Interiors/Enable Renko Pilot (Integration Approval Required)")]
        public static void EnableRenkoPilot()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(TownScenePath, OpenSceneMode.Single);
            var installer = FindSceneComponent<TownSceneRuntimeReferenceInstaller>(scene);
            var renko = FindRenkoShop(scene);
            if (installer == null || renko == null)
                throw new InvalidOperationException("TownScene must contain its installer and existing npc_renko shop controller.");

            var serializedInstaller = new SerializedObject(installer);
            var enabled = serializedInstaller.FindProperty("_enableInteriorPilot");
            var renkoShop = serializedInstaller.FindProperty("_renkoShop");
            var visits = serializedInstaller.FindProperty("_interiorVisits");
            if (enabled == null || renkoShop == null || visits == null)
                throw new InvalidOperationException("Renko pilot installer fields are not available; wait for approved runtime wiring.");
            var controller = visits.objectReferenceValue as TownInteriorVisitController;
            var threshold = FindNamedSceneComponent<TownInteriorThreshold>(scene, "RenkoInteriorEntryThreshold_DISABLED");
            if (controller == null || controller.gameObject.name != "RenkoInteriorPilot_DISABLED" || threshold == null ||
                threshold.GetComponent<BoxCollider2D>() == null)
                throw new InvalidOperationException("Renko pilot preflight requires its explicit visit controller and entry threshold.");
            var thresholdData = new SerializedObject(threshold).FindProperty("_houseId");
            if (thresholdData == null || thresholdData.stringValue != RenkoHouseId)
                throw new InvalidOperationException("Renko entry threshold identity does not match House_Residential_2.");

            enabled.boolValue = true;
            renkoShop.objectReferenceValue = renko;
            serializedInstaller.ApplyModifiedPropertiesWithoutUndo();
            if (renko.GetComponent<NpcInteriorPresentationAdapter>() == null)
                renko.gameObject.AddComponent<NpcInteriorPresentationAdapter>();
            controller.gameObject.SetActive(true);
            controller.enabled = true;
            threshold.gameObject.SetActive(true);
            threshold.enabled = true;
            threshold.GetComponent<BoxCollider2D>().enabled = true;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[GenerateTownInteriorScenes] Enabled only the Renko pilot installer, adapter and entry transition in TownScene.");
        }

        private static Transform ResetGeneratedRoot(Transform parent, string name)
        {
            var existing = parent.Find(name);
            if (existing != null) UnityEngine.Object.DestroyImmediate(existing.gameObject);
            var root = new GameObject(name).transform;
            root.SetParent(parent, false);
            return root;
        }

        private static void BuildFloor(Transform parent)
        {
            var floor = CreateVisual(parent, "FloorPlanks", WorldSpriteLibrary.Building("wall_plank"),
                Vector2.zero, new Vector2(16f, 12f), true, WorldSortingLayers.Ground);
            floor.color = new Color(.72f, .60f, .43f, 1f);
        }

        private static void BuildWalls(Transform parent)
        {
            AddWall(parent, "BackWall", WorldSpriteLibrary.Building("wall_plaster"), new Vector2(0f, 5.5f), new Vector2(16f, 1f));
            var sideWall = WorldSpriteLibrary.Building("wall_plank");
            AddWall(parent, "LeftWall", sideWall, new Vector2(-7.88f, 0f), new Vector2(.24f, 12f));
            AddWall(parent, "RightWall", sideWall, new Vector2(7.88f, 0f), new Vector2(.24f, 12f));
            // Camera sees through the front. Solid side returns leave exactly the 2u ExitThreshold opening.
            AddFrontReturn(parent, "FrontReturnWest", new Vector2(-4.5f, -5.80f), new Vector2(7f, .40f));
            AddFrontReturn(parent, "FrontReturnEast", new Vector2(4.5f, -5.80f), new Vector2(7f, .40f));
        }

        private static void BuildRooms(Transform parent)
        {
            CreateRoom(parent, "SalesSouth", new Vector2(0f, -3.5f), new Vector2(16f, 5f));
            CreateRoom(parent, "StockAndKitchenCenter", new Vector2(0f, .5f), new Vector2(16f, 3f));
            CreateRoom(parent, "RenkoBedroomNorth", new Vector2(0f, 4f), new Vector2(16f, 4f));
        }

        private static void BuildProps(Transform parent, ICollection<string> manifest)
        {
            var cupboard = WorldSpriteLibrary.Interior("chair"); // inspected: this asset is a cupboard with dishes.
            var bed = WorldSpriteLibrary.Interior("bed");
            var table = WorldSpriteLibrary.Interior("kitchen_counter"); // inspected: this asset is a wood dining table.
            var chair = WorldSpriteLibrary.Interior("rug"); // inspected: this asset is a small wooden chair.
            var crate = WorldSpriteLibrary.Prop("crate");
            var washTub = WorldSpriteLibrary.Prop("barrel_keyart_v4");

            AddCounter(parent, "SalesCounter", table, new Vector2(-3.50f, -2.35f), 2.20f, new Vector2(2.40f, .52f), manifest);
            AddProp(parent, "DisplayShelfWest", cupboard, new Vector2(-6.45f, -4.10f), 2.05f, new Vector2(.90f, .45f), "sales", "display", manifest);
            AddProp(parent, "DisplayShelfEastA", cupboard, new Vector2(6.45f, -4.10f), 2.05f, new Vector2(.90f, .45f), "sales", "display", manifest);
            AddProp(parent, "DisplayShelfEastB", cupboard, new Vector2(6.45f, -1.85f), 2.05f, new Vector2(.90f, .45f), "sales", "display", manifest);

            AddProp(parent, "StockCrateA", crate, new Vector2(-6.15f, -.05f), 1.55f, new Vector2(.95f, .52f), "stock", "stock", manifest);
            AddProp(parent, "StockCrateB", crate, new Vector2(-5.00f, -.05f), 1.35f, new Vector2(.85f, .48f), "stock", "stock", manifest);
            AddProp(parent, "StockCrateC", crate, new Vector2(-5.55f, 1.35f), 1.35f, new Vector2(.85f, .48f), "stock", "stock", manifest);
            AddProp(parent, "KitchenTable", table, new Vector2(4.85f, .20f), 2.00f, new Vector2(1.25f, .52f), "kitchen", "meal", manifest);
            AddProp(parent, "KitchenChair", chair, new Vector2(3.25f, -.20f), 1.30f, new Vector2(.55f, .36f), "kitchen", "seat", manifest);

            AddProp(parent, "RenkoBedSmallfolk", bed, new Vector2(-5.25f, 4.05f), 2.25f, new Vector2(1.35f, .58f), "bedroom", "sleep", manifest);
            AddProp(parent, "RenkoWardrobe", cupboard, new Vector2(-6.45f, 5.15f), 2.15f, new Vector2(.95f, .45f), "bedroom", "storage", manifest);
            AddProp(parent, "RenkoWashTubProvisional", washTub, new Vector2(5.90f, 4.75f), 1.35f, new Vector2(.70f, .38f), "bedroom", "wash", manifest);
        }

        private static Transform CreateAnchor(Transform parent, string id, Vector2 localPosition)
        {
            var anchor = new GameObject(id).transform;
            anchor.SetParent(parent, false);
            anchor.localPosition = localPosition;
            return anchor;
        }

        private static void AddWall(Transform parent, string id, Sprite sprite, Vector2 position, Vector2 size)
        {
            var wall = new GameObject(id).transform;
            wall.SetParent(parent, false);
            wall.localPosition = position;
            CreateVisual(wall, "Visual", sprite, Vector2.zero, size, true, WorldSortingLayers.World);
            var support = new GameObject("Support", typeof(BoxCollider2D)).transform;
            support.SetParent(wall, false);
            support.GetComponent<BoxCollider2D>().size = size;
        }

        private static void AddFrontReturn(Transform parent, string id, Vector2 position, Vector2 size)
        {
            var wall = new GameObject(id).transform;
            wall.SetParent(parent, false);
            wall.localPosition = position;
            // This is an intentional collision-only camera cutaway, not a missing-art fallback.
            wall.gameObject.AddComponent<BoxCollider2D>().size = size;
        }

        private static void CreateRoom(Transform parent, string name, Vector2 center, Vector2 size)
        {
            var room = new GameObject(name).transform;
            room.SetParent(parent, false);
            room.localPosition = center;
            // The room root is semantic layout only. It intentionally has no floor collider.
            if (size.x <= 0f || size.y <= 0f) throw new InvalidOperationException("Room size must be positive.");
        }

        private static void AddCounter(Transform parent, string id, Sprite sprite, Vector2 position, float visualHeight,
            Vector2 supportSize,
            ICollection<string> manifest)
        {
            var root = new GameObject(id).transform;
            root.SetParent(parent, false);
            root.localPosition = position;
            var visual = CreateVisual(root, "Visual", sprite, Vector2.zero, Vector2.zero, false, WorldSortingLayers.World);
            float scale = sprite != null && sprite.bounds.size.y > 0f ? visualHeight / sprite.bounds.size.y : 1f;
            visual.transform.localScale = new Vector3(scale, scale, 1f);
            CenterVisual(visual.transform, Vector2.zero);
            var support = new GameObject("Support", typeof(BoxCollider2D)).transform;
            support.SetParent(root, false);
            support.GetComponent<BoxCollider2D>().size = supportSize;
            var spriteBounds = sprite != null ? sprite.bounds : default;
            support.localPosition = visual.transform.localPosition + new Vector3(spriteBounds.center.x * scale,
                spriteBounds.min.y * scale + supportSize.y * .5f);
            manifest.Add($"{id}|sales|counter|{AssetDatabase.GetAssetPath(sprite)}|height={visualHeight:0.##}|collider={supportSize.x:0.##}x{supportSize.y:0.##}");
        }

        private static void AddProp(Transform parent, string id, Sprite sprite, Vector2 position, float visualHeight,
            Vector2 supportSize, string room, string purpose, ICollection<string> manifest)
        {
            var root = new GameObject(id).transform;
            root.SetParent(parent, false);
            root.localPosition = position;
            var visual = CreateVisual(root, "Visual", sprite, Vector2.zero, Vector2.zero, false, WorldSortingLayers.World);
            float scale = 1f;
            if (sprite != null && sprite.bounds.size.y > 0f)
            {
                scale = visualHeight / sprite.bounds.size.y;
                visual.transform.localScale = new Vector3(scale, scale, 1f);
            }
            CenterVisual(visual.transform, Vector2.zero);
            var support = new GameObject("Support", typeof(BoxCollider2D)).transform;
            support.SetParent(root, false);
            support.GetComponent<BoxCollider2D>().size = supportSize;
            var visualBounds = sprite != null ? sprite.bounds : default;
            support.localPosition = visual.transform.localPosition + new Vector3(visualBounds.center.x * scale,
                visualBounds.min.y * scale + supportSize.y * .5f);
            var source = AssetDatabase.GetAssetPath(sprite);
            manifest.Add($"{id}|{room}|{purpose}|{source}|height={visualHeight:0.##}|collider={supportSize.x:0.##}x{supportSize.y:0.##}");
        }

        private static SpriteRenderer CreateVisual(Transform parent, string name, Sprite sprite, Vector2 localPosition,
            Vector2 tiledSize, bool tiled, string sortingLayer)
        {
            var visual = new GameObject(name).transform;
            visual.SetParent(parent, false);
            visual.localPosition = localPosition;
            var renderer = visual.gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = sortingLayer;
            renderer.sortingOrder = 0;
            renderer.spriteSortPoint = SpriteSortPoint.Pivot;
            if (tiled && sprite != null)
            {
                renderer.drawMode = SpriteDrawMode.Tiled;
                renderer.tileMode = SpriteTileMode.Continuous;
                renderer.size = tiledSize / TiledVisualScale;
                visual.localScale = new Vector3(TiledVisualScale, TiledVisualScale, 1f);
            }
            CenterVisual(visual, localPosition);
            return renderer;
        }

        private static void CenterVisual(Transform visual, Vector2 authoredCenter)
        {
            var renderer = visual.GetComponent<SpriteRenderer>();
            var boundsCenter = renderer.localBounds.center;
            visual.localPosition = authoredCenter - Vector2.Scale((Vector2)boundsCenter,
                new Vector2(visual.localScale.x, visual.localScale.y));
        }

        private static void ValidatePilotLayout(TownInteriorInstance instance, Transform props,
            Transform workAnchor, Transform homeAnchor)
        {
            if (!instance.TryValidate(RenkoHouseId, out var reason)) throw new InvalidOperationException(reason);
            if (workAnchor == null || homeAnchor == null) throw new InvalidOperationException("Renko NPC anchors were not generated.");
            ValidateNpcAnchorClear(workAnchor, props, "work");
            ValidateNpcAnchorClear(homeAnchor, props, "home");
            var centralClearance = MeasureCentralClearance(props, -5.2f, 3f);
            if (centralClearance < PlayerBodyWidth + RequiredClearance)
                throw new InvalidOperationException("Renko central passage is narrower than Player plus clearance.");
            foreach (var collider in props.GetComponentsInChildren<BoxCollider2D>(true))
                if (collider.isTrigger) throw new InvalidOperationException("Renko furniture supports must be solid bases, not triggers.");
        }

        private static void ValidateNpcAnchorClear(Transform anchor, Transform props, string label)
        {
            var footprint = new Bounds((Vector2)anchor.localPosition + RenkoSolidOffset, RenkoSolidSize);
            foreach (var collider in props.GetComponentsInChildren<BoxCollider2D>(true))
                if (collider.bounds.Intersects(footprint))
                    throw new InvalidOperationException($"Renko {label} anchor overlaps furniture support '{collider.transform.parent.name}'.");
        }

        private static float MeasureCentralClearance(Transform props, float minY, float maxY)
        {
            float leftEdge = float.NegativeInfinity;
            float rightEdge = float.PositiveInfinity;
            foreach (var collider in props.GetComponentsInChildren<BoxCollider2D>(true))
            {
                var bounds = collider.bounds;
                if (bounds.max.y < minY || bounds.min.y > maxY) continue;
                if (bounds.center.x <= 0f) leftEdge = Mathf.Max(leftEdge, bounds.max.x);
                else rightEdge = Mathf.Min(rightEdge, bounds.min.x);
            }
            if (float.IsNegativeInfinity(leftEdge) || float.IsPositiveInfinity(rightEdge)) return float.PositiveInfinity;
            return rightEdge - leftEdge;
        }

        private static T FindSceneComponent<T>(Scene scene) where T : Component
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var result = root.GetComponentInChildren<T>(true);
                if (result != null) return result;
            }
            return null;
        }

        private static T FindNamedSceneComponent<T>(Scene scene, string objectName) where T : Component
        {
            foreach (var root in scene.GetRootGameObjects())
                foreach (var component in root.GetComponentsInChildren<T>(true))
                    if (component != null && component.gameObject.name == objectName) return component;
            return null;
        }

        private static NpcShopController FindRenkoShop(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
                foreach (var shop in root.GetComponentsInChildren<NpcShopController>(true))
                    if (shop != null && shop.NpcData != null && shop.NpcData.NpcId == "npc_renko") return shop;
            return null;
        }
    }
}
