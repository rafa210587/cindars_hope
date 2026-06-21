using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Farm;
using CindarsHope.Farm.Data;
using CindarsHope.Farm.Scene;
using CindarsHope.Inventory;
using CindarsHope.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CindarsHope.Editor.SceneCreation
{
    public static class RewireFarmSceneIntegrationSafe
    {
        private const string ScenePath = "Assets/_Game/Scenes/FarmScene.unity";
        private const string SeedDatabasePath = "Assets/_Game/Data/Registries/SeedDatabase.asset";
        private const string ItemDatabasePath = "Assets/_Game/Data/Registries/ItemDatabase.asset";
        private const string WeaponDatabasePath = "Assets/_Game/Data/Combat/WeaponDatabase.asset";
        private const string SpellDatabasePath = "Assets/_Game/Data/Combat/SpellDatabase.asset";
        private const string StatusEffectDatabasePath = "Assets/_Game/Data/Combat/StatusEffectDatabase.asset";
        private const string BuiltinSpritePath = "UI/Skin/UISprite.psd";
        private const string ZoneParentName = "FarmSceneFoundationZones";

        public static void Rewire()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Exit Play Mode before rewiring FarmScene.");
                return;
            }

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                Debug.LogError($"Could not open {ScenePath}.");
                return;
            }

            RecreateZoneMarkers();
            RewirePlots();
            RewireBootstrapRuntimeDatabases();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            Debug.Log("FarmScene WAVE04/05 wiring reapplied safely through Unity APIs.");
        }

        private static void RecreateZoneMarkers()
        {
            var parent = GameObject.Find(ZoneParentName);
            if (parent != null)
            {
                Object.DestroyImmediate(parent);
            }

            parent = new GameObject(ZoneParentName);

            foreach (var zone in Zones())
            {
                var go = new GameObject(zone.Name);
                go.transform.SetParent(parent.transform);
                go.transform.position = zone.Position;
                go.transform.localScale = zone.Size;

                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(BuiltinSpritePath);
                renderer.color = zone.Color;
                renderer.sortingOrder = -10;

                var collider = go.AddComponent<BoxCollider2D>();
                collider.isTrigger = true;
                collider.size = Vector2.one;

                var marker = go.AddComponent<FarmSceneZoneMarker>();
                var serializedMarker = new SerializedObject(marker);

                var zoneType = serializedMarker.FindProperty("zoneType");
                if (zoneType != null)
                {
                    zoneType.enumValueIndex = (int)zone.Type;
                }

                var stableId = serializedMarker.FindProperty("stableId");
                if (stableId != null)
                {
                    stableId.stringValue = zone.StableId;
                }

                serializedMarker.ApplyModifiedPropertiesWithoutUndo();

                EditorUtility.SetDirty(go);
                EditorUtility.SetDirty(marker);
            }
        }

        private static void RewirePlots()
        {
            var plots = FindSceneObjects<FarmPlot>().OrderBy(GetPlotIndex).ToArray();
            if (plots.Length == 0)
            {
                Debug.LogWarning("No FarmPlot found. Run CreateMvpFarmScene through Unity if this baseline lacks plots.");
                return;
            }

            var inventory = FindSceneObjects<InventoryManager>().FirstOrDefault();
            var stamina = FindSceneObjects<StaminaManager>().FirstOrDefault();
            var seedDatabase = AssetDatabase.LoadAssetAtPath<CindarsHope.Core.Data.SeedDatabaseSO>(SeedDatabasePath);

            for (var i = 0; i < plots.Length; i++)
            {
                var plot = plots[i];
                var index = GetPlotIndex(plot);
                if (index < 0)
                {
                    index = i;
                }

                var serializedPlot = new SerializedObject(plot);
                SetRef(serializedPlot, "_inventoryManager", inventory);
                SetRef(serializedPlot, "_seedDatabase", seedDatabase);
                SetRef(serializedPlot, "_staminaManager", stamina);
                SetBool(serializedPlot, "_temporarySequentialSliceMode", index == 0);
                SetString(serializedPlot, "_temporarySequentialSeedId", "seed_carrot");
                serializedPlot.ApplyModifiedPropertiesWithoutUndo();

                plot.Configure(index, inventory, seedDatabase);
                plot.RebindStaminaManager(stamina);
                EditorUtility.SetDirty(plot);
            }

            var registry = FindSceneObjects<FarmPlotRegistry>().FirstOrDefault();
            if (registry != null)
            {
                registry.Configure(plots);
                EditorUtility.SetDirty(registry);
            }
        }

        private static void RewireBootstrapRuntimeDatabases()
        {
            var bootstrap = FindSceneObjects<GameBootstrap>().FirstOrDefault();
            if (bootstrap == null)
            {
                Debug.LogWarning("GameBootstrap not found. Combat databases were not rebound.");
                return;
            }

            var serializedBootstrap = new SerializedObject(bootstrap);
            SetRef(serializedBootstrap, "_itemDatabase", AssetDatabase.LoadAssetAtPath<Object>(ItemDatabasePath));
            SetRef(serializedBootstrap, "_weaponDatabase", AssetDatabase.LoadAssetAtPath<Object>(WeaponDatabasePath));
            SetRef(serializedBootstrap, "_spellDatabase", AssetDatabase.LoadAssetAtPath<Object>(SpellDatabasePath));
            SetRef(serializedBootstrap, "_statusEffectDatabase", AssetDatabase.LoadAssetAtPath<Object>(StatusEffectDatabasePath));
            serializedBootstrap.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(bootstrap);
        }

        private static IEnumerable<Zone> Zones()
        {
            yield return new Zone("Zone_PlayerSpawn", "farm_zone_player_spawn", FarmSceneZoneType.PlayerSpawn, new Vector3(0f, 0f, 0f), new Vector3(1.5f, 1.5f, 1f), new Color(0.2f, 0.45f, 1f, 0.35f));
            yield return new Zone("Zone_CropField", "farm_zone_crop_field", FarmSceneZoneType.CropField, new Vector3(-4.75f, 1f, 0f), new Vector3(5f, 5f, 1f), new Color(0.55f, 0.32f, 0.16f, 0.35f));
            yield return new Zone("Zone_ResourceTrees", "farm_zone_resource_trees", FarmSceneZoneType.ResourceTrees, new Vector3(7f, 2.5f, 0f), new Vector3(5f, 4f, 1f), new Color(0.1f, 0.6f, 0.18f, 0.35f));
            yield return new Zone("Zone_ResourceRocks", "farm_zone_resource_rocks", FarmSceneZoneType.ResourceRocks, new Vector3(-9f, 5f, 0f), new Vector3(4f, 3f, 1f), new Color(0.45f, 0.45f, 0.45f, 0.35f));
            yield return new Zone("Zone_Forage", "farm_zone_forage", FarmSceneZoneType.Forage, new Vector3(-9f, 0f, 0f), new Vector3(4f, 4f, 1f), new Color(0.45f, 0.55f, 0.18f, 0.35f));
            yield return new Zone("Zone_LakeFishing", "farm_zone_lake_fishing", FarmSceneZoneType.LakeFishing, new Vector3(7.8f, -2.8f, 0f), new Vector3(5f, 3f, 1f), new Color(0.1f, 0.45f, 0.9f, 0.35f));
            yield return new Zone("Zone_ShippingSellpoint", "farm_zone_shipping_sellpoint", FarmSceneZoneType.ShippingSellpoint, new Vector3(4.5f, 4.5f, 0f), new Vector3(3f, 2f, 1f), new Color(0.95f, 0.72f, 0.12f, 0.35f));
            yield return new Zone("Zone_Construction", "farm_zone_construction", FarmSceneZoneType.Construction, new Vector3(0f, 5f, 0f), new Vector3(5f, 3f, 1f), new Color(0.62f, 0.42f, 0.25f, 0.35f));
            yield return new Zone("Zone_HouseEntrance", "farm_zone_house_entrance", FarmSceneZoneType.HouseEntrance, new Vector3(-8.5f, -6f, 0f), new Vector3(2.5f, 2f, 1f), new Color(0.6f, 0.34f, 0.18f, 0.35f));
            yield return new Zone("Zone_TownExit", "farm_zone_town_exit", FarmSceneZoneType.TownExit, new Vector3(-8.25f, -4.75f, 0f), new Vector3(2f, 2f, 1f), new Color(1f, 0.86f, 0.15f, 0.35f));
            yield return new Zone("Zone_CaveEntrance", "farm_zone_cave_entrance", FarmSceneZoneType.CaveEntrance, new Vector3(-5.5f, 0f, 0f), new Vector3(2f, 2f, 1f), new Color(0.55f, 0.25f, 0.8f, 0.35f));
        }

        private static T[] FindSceneObjects<T>() where T : Component
        {
            var result = new List<T>();
            foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                result.AddRange(root.GetComponentsInChildren<T>(true));
            }

            return result.ToArray();
        }

        private static int GetPlotIndex(FarmPlot plot)
        {
            if (plot == null || !plot.name.StartsWith("FarmPlot_", StringComparison.OrdinalIgnoreCase))
            {
                return -1;
            }

            return int.TryParse(plot.name.Substring("FarmPlot_".Length), NumberStyles.Integer, CultureInfo.InvariantCulture, out var index) ? index : -1;
        }

        private static void SetRef(SerializedObject obj, string name, Object value)
        {
            var prop = obj.FindProperty(name);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
            }
        }

        private static void SetBool(SerializedObject obj, string name, bool value)
        {
            var prop = obj.FindProperty(name);
            if (prop != null)
            {
                prop.boolValue = value;
            }
        }

        private static void SetString(SerializedObject obj, string name, string value)
        {
            var prop = obj.FindProperty(name);
            if (prop != null)
            {
                prop.stringValue = value;
            }
        }

        private readonly struct Zone
        {
            public readonly string Name;
            public readonly string StableId;
            public readonly FarmSceneZoneType Type;
            public readonly Vector3 Position;
            public readonly Vector3 Size;
            public readonly Color Color;

            public Zone(string name, string stableId, FarmSceneZoneType type, Vector3 position, Vector3 size, Color color)
            {
                Name = name;
                StableId = stableId;
                Type = type;
                Position = position;
                Size = size;
                Color = color;
            }
        }
    }
}