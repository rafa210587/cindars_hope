#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Data;
using CindarsHope.Economy;
using CindarsHope.NPC;
using CindarsHope.UI.Shop;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Editor.Validation
{
    public static class ValidateTownShopWiring
    {
        private const string TownScenePath = "Assets/_Game/Scenes/TownScene.unity";

        [MenuItem("Cindar's Hope/Validation/Validate Town Shop Wiring")]
        public static void Validate()
        {
            var scene = EditorSceneManager.OpenScene(TownScenePath, OpenSceneMode.Single);
            var errors = new List<string>();
            ValidateMissingScripts(scene, errors);

            var bootstrap = FindOneInScene<GameBootstrap>(scene, errors);
            var manager = FindOneInScene<ShopManager>(scene, errors);
            var buyPanel = FindOneInScene<BuyPanel>(scene, errors);
            var sellPanel = FindOneInScene<SellPanel>(scene, errors);
            var controllers = FindAllInScene<NpcShopController>(scene);

            if (bootstrap != null && manager != null)
            {
                RequireSameReference(bootstrap, "_shopManager", manager, errors);
            }
            if (buyPanel != null)
            {
                ValidatePanelReferences(buyPanel, new[] { "_canvasGroup", "_itemsContainer", "_itemPrefab", "_backButton" }, errors);
            }
            if (sellPanel != null)
            {
                ValidatePanelReferences(sellPanel, new[] { "_canvasGroup", "_itemsContainer", "_itemPrefab", "_backButton" }, errors);
            }
            if (controllers.Length != 2)
            {
                errors.Add($"Scene '{scene.path}' component '{nameof(NpcShopController)}' requires exactly two NPC shops; found {controllers.Length}.");
            }

            ItemDatabaseSO database = null;
            foreach (var controller in controllers)
            {
                database = ValidateController(controller, manager, buyPanel, sellPanel, database, errors);
            }

            if (manager != null && database != null && errors.Count == 0)
            {
                manager.Configure(database);
                foreach (var controller in controllers)
                {
                    var shopData = new SerializedObject(controller).FindProperty("_shopData").objectReferenceValue as ShopDataSO;
                    if (shopData == null || !manager.InitializeShop(shopData) || !manager.HasSession(shopData.Id))
                    {
                        errors.Add(Context(controller, "_shopManager", shopData != null ? shopData.Id : "<null>") + " cannot initialize its session.");
                    }
                }

                if (!manager.HasSession("shop_weapons_armor") || !manager.HasSession("shop_seeds_tools"))
                {
                    errors.Add($"Scene '{scene.path}' component '{nameof(ShopManager)}' does not register both required shop sessions. {manager.GetDiagnosticSummary()}");
                }
                else
                {
                    Debug.Log($"[SPEC17E] {manager.GetDiagnosticSummary()}");
                }
            }

            if (errors.Count > 0)
            {
                foreach (var error in errors)
                {
                    Debug.LogError($"[SPEC17E] {error}");
                }

                throw new InvalidOperationException($"[SPEC17E] Town shop wiring validation failed with {errors.Count} error(s).");
            }

            Debug.Log($"[SPEC17E] Validate Town Shop Wiring passed. {manager.GetDiagnosticSummary()}");
        }

        private static ItemDatabaseSO ValidateController(NpcShopController controller, ShopManager manager, BuyPanel buyPanel, SellPanel sellPanel, ItemDatabaseSO expectedDatabase, List<string> errors)
        {
            var serialized = new SerializedObject(controller);
            foreach (var field in new[] { "_npcData", "_shopData", "_shopManager", "_playerManager", "_inventoryManager", "_itemDatabase", "_modalManager", "_dialogueModal", "_shopMenuModal", "_buyPanel", "_sellPanel" })
            {
                RequireReference(serialized, controller, field, errors);
            }

            var shopData = serialized.FindProperty("_shopData")?.objectReferenceValue as ShopDataSO;
            var database = serialized.FindProperty("_itemDatabase")?.objectReferenceValue as ItemDatabaseSO;
            var shopId = shopData != null ? shopData.Id : "<null>";
            if (manager != null && serialized.FindProperty("_shopManager")?.objectReferenceValue != manager)
            {
                errors.Add(Context(controller, "_shopManager", shopId) + " does not reference the single scene ShopManager.");
            }
            if (buyPanel != null && serialized.FindProperty("_buyPanel")?.objectReferenceValue != buyPanel)
            {
                errors.Add(Context(controller, "_buyPanel", shopId) + " does not reference the visible BuyPanel.");
            }
            if (sellPanel != null && serialized.FindProperty("_sellPanel")?.objectReferenceValue != sellPanel)
            {
                errors.Add(Context(controller, "_sellPanel", shopId) + " does not reference the visible SellPanel.");
            }
            if (expectedDatabase != null && database != expectedDatabase)
            {
                errors.Add(Context(controller, "_itemDatabase", shopId) + " does not share ItemDatabaseSO with the other NPC shop.");
            }

            if (shopData == null || string.IsNullOrWhiteSpace(shopData.Id) || shopData.Items == null || shopData.Items.Length == 0)
            {
                errors.Add(Context(controller, "_shopData", shopId) + " requires non-empty Id and Items.");
                return expectedDatabase ?? database;
            }
            if (database == null)
            {
                return expectedDatabase;
            }

            foreach (var entry in shopData.Items)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.ItemId) || !database.TryGetById(entry.ItemId, out var item) || item == null || (entry.BuyPriceOverride <= 0 && item.BaseValue <= 0))
                {
                    errors.Add(Context(controller, "_shopData.Items", shopId) + $" has invalid priced item '{entry?.ItemId ?? "<null>"}'.");
                }
            }

            return expectedDatabase ?? database;
        }

        private static void ValidatePanelReferences(Component panel, string[] fields, List<string> errors)
        {
            var serialized = new SerializedObject(panel);
            foreach (var field in fields)
            {
                RequireReference(serialized, panel, field, errors);
            }
        }

        private static void RequireReference(SerializedObject serialized, Component component, string field, List<string> errors)
        {
            var property = serialized.FindProperty(field);
            if (property == null || property.objectReferenceValue == null)
            {
                errors.Add($"Scene '{component.gameObject.scene.path}' GameObject '{component.gameObject.name}' component '{component.GetType().Name}' field '{field}' is missing.");
            }
        }

        private static void RequireSameReference(Component target, string field, UnityEngine.Object expected, List<string> errors)
        {
            var property = new SerializedObject(target).FindProperty(field);
            if (property == null || property.objectReferenceValue != expected)
            {
                errors.Add($"Scene '{target.gameObject.scene.path}' GameObject '{target.gameObject.name}' component '{target.GetType().Name}' field '{field}' does not reference the single ShopManager.");
            }
        }

        private static T FindOneInScene<T>(Scene scene, List<string> errors) where T : Component
        {
            var values = FindAllInScene<T>(scene);
            if (values.Length != 1)
            {
                errors.Add($"Scene '{scene.path}' component '{typeof(T).Name}' requires exactly one instance; found {values.Length}.");
                return null;
            }

            return values[0];
        }

        private static T[] FindAllInScene<T>(Scene scene) where T : Component
        {
            var results = new List<T>();
            foreach (var root in scene.GetRootGameObjects())
            {
                results.AddRange(root.GetComponentsInChildren<T>(true));
            }

            return results.ToArray();
        }

        private static void ValidateMissingScripts(Scene scene, List<string> errors)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                ValidateMissingScripts(root, root.name, scene.path, errors);
            }
        }

        private static void ValidateMissingScripts(GameObject gameObject, string objectPath, string scenePath, List<string> errors)
        {
            var count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
            if (count > 0)
            {
                errors.Add($"Scene '{scenePath}' GameObject '{objectPath}' component 'MissingScript' has {count} broken reference(s).");
            }

            foreach (Transform child in gameObject.transform)
            {
                ValidateMissingScripts(child.gameObject, $"{objectPath}/{child.name}", scenePath, errors);
            }
        }

        private static string Context(NpcShopController controller, string field, string shopId)
        {
            return $"Scene '{controller.gameObject.scene.path}' GameObject '{controller.gameObject.name}' component '{nameof(NpcShopController)}' field '{field}' shopId '{shopId}'";
        }
    }
}
#endif
