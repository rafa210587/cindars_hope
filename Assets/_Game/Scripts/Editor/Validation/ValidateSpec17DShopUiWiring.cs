#if UNITY_EDITOR
using System;
using System.Collections.Generic;
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
    public static class ValidateSpec17DShopUiWiring
    {
        private const string TownScenePath = "Assets/_Game/Scenes/TownScene.unity";

        [MenuItem("Cindar's Hope/Validation/Validate SPEC 17D Shop UI Wiring")]
        public static void ValidateTownShopUi()
        {
            var scene = EditorSceneManager.OpenScene(TownScenePath, OpenSceneMode.Single);
            var errors = new List<string>();

            ValidateMissingScripts(scene, errors);
            var managers = FindAllInScene<ShopManager>(scene);
            if (managers.Length != 1)
            {
                errors.Add($"Scene '{scene.path}' component '{nameof(ShopManager)}' requires exactly one instance; found {managers.Length}.");
            }

            var buyPanels = FindAllInScene<BuyPanel>(scene);
            var sellPanels = FindAllInScene<SellPanel>(scene);
            if (buyPanels.Length != 1)
            {
                errors.Add($"Scene '{scene.path}' component '{nameof(BuyPanel)}' requires exactly one instance; found {buyPanels.Length}.");
            }
            if (sellPanels.Length != 1)
            {
                errors.Add($"Scene '{scene.path}' component '{nameof(SellPanel)}' requires exactly one instance; found {sellPanels.Length}.");
            }

            if (buyPanels.Length == 1)
            {
                ValidatePanelReferences(buyPanels[0], new[] { "_canvasGroup", "_itemsContainer", "_itemPrefab", "_backButton" }, errors);
            }
            if (sellPanels.Length == 1)
            {
                ValidatePanelReferences(sellPanels[0], new[] { "_canvasGroup", "_itemsContainer", "_itemPrefab", "_backButton" }, errors);
            }

            var controllers = FindAllInScene<NpcShopController>(scene);
            if (controllers.Length == 0)
            {
                errors.Add($"Scene '{scene.path}' component '{nameof(NpcShopController)}' requires at least one shop NPC.");
            }

            foreach (var controller in controllers)
            {
                ValidateController(controller, managers, buyPanels, sellPanels, errors);
            }
            ValidateSharedPanelContext(controllers, errors);

            if (errors.Count > 0)
            {
                foreach (var error in errors)
                {
                    Debug.LogError($"[SPEC17D] {error}");
                }

                throw new InvalidOperationException($"[SPEC17D] Town shop UI wiring validation failed with {errors.Count} error(s).");
            }

            Debug.Log($"[SPEC17D] Town shop UI wiring passed: {controllers.Length} NPC shop(s), one manager, one BuyPanel and one SellPanel.");
        }

        private static void ValidateController(
            NpcShopController controller,
            ShopManager[] managers,
            BuyPanel[] buyPanels,
            SellPanel[] sellPanels,
            List<string> errors)
        {
            var serialized = new SerializedObject(controller);
            foreach (var field in new[]
            {
                "_npcData", "_shopData", "_shopManager", "_playerManager", "_inventoryManager", "_itemDatabase",
                "_modalManager", "_dialogueModal", "_shopMenuModal", "_buyPanel", "_sellPanel"
            })
            {
                RequireReference(serialized, controller, field, errors);
            }

            var shopData = serialized.FindProperty("_shopData")?.objectReferenceValue as ShopDataSO;
            var database = serialized.FindProperty("_itemDatabase")?.objectReferenceValue as ItemDatabaseSO;
            var shopId = shopData != null ? shopData.Id : "<null>";
            if (managers.Length == 1 && serialized.FindProperty("_shopManager")?.objectReferenceValue != managers[0])
            {
                errors.Add(Context(controller, "_shopManager", shopId) + " does not reference the scene ShopManager.");
            }
            if (buyPanels.Length == 1 && serialized.FindProperty("_buyPanel")?.objectReferenceValue != buyPanels[0])
            {
                errors.Add(Context(controller, "_buyPanel", shopId) + " does not reference the visible BuyPanel.");
            }
            if (sellPanels.Length == 1 && serialized.FindProperty("_sellPanel")?.objectReferenceValue != sellPanels[0])
            {
                errors.Add(Context(controller, "_sellPanel", shopId) + " does not reference the visible SellPanel.");
            }

            if (shopData == null || string.IsNullOrWhiteSpace(shopData.Id) || shopData.Items == null || shopData.Items.Length == 0)
            {
                errors.Add(Context(controller, "_shopData", shopId) + " requires non-empty Id and Items.");
                return;
            }

            if (database == null)
            {
                return;
            }

            foreach (var entry in shopData.Items)
            {
                if (entry == null
                    || string.IsNullOrWhiteSpace(entry.ItemId)
                    || !database.TryGetById(entry.ItemId, out var item)
                    || item == null
                    || (entry.BuyPriceOverride <= 0 && item.BaseValue <= 0))
                {
                    errors.Add(Context(controller, "_shopData.Items", shopId) + $" has invalid priced item '{entry?.ItemId ?? "<null>"}'.");
                }
            }
        }

        private static void ValidatePanelReferences(Component panel, string[] fields, List<string> errors)
        {
            var serialized = new SerializedObject(panel);
            foreach (var field in fields)
            {
                RequireReference(serialized, panel, field, errors);
            }
        }

        private static void ValidateSharedPanelContext(NpcShopController[] controllers, List<string> errors)
        {
            if (controllers.Length < 2)
            {
                return;
            }

            var expected = new SerializedObject(controllers[0]);
            foreach (var controller in controllers)
            {
                var serialized = new SerializedObject(controller);
                foreach (var field in new[]
                {
                    "_shopManager", "_playerManager", "_inventoryManager", "_itemDatabase",
                    "_modalManager", "_shopMenuModal", "_buyPanel", "_sellPanel"
                })
                {
                    if (serialized.FindProperty(field)?.objectReferenceValue != expected.FindProperty(field)?.objectReferenceValue)
                    {
                        errors.Add(
                            $"Scene '{controller.gameObject.scene.path}' GameObject '{controller.gameObject.name}' component '{nameof(NpcShopController)}' field '{field}' must match shared shop UI context.");
                    }
                }
            }
        }

        private static void RequireReference(SerializedObject serialized, Component component, string field, List<string> errors)
        {
            var property = serialized.FindProperty(field);
            if (property == null || property.objectReferenceValue == null)
            {
                errors.Add(
                    $"Scene '{component.gameObject.scene.path}' GameObject '{component.gameObject.name}' component '{component.GetType().Name}' field '{field}' is missing.");
            }
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

        private static T[] FindAllInScene<T>(Scene scene) where T : Component
        {
            var results = new List<T>();
            foreach (var root in scene.GetRootGameObjects())
            {
                results.AddRange(root.GetComponentsInChildren<T>(true));
            }

            return results.ToArray();
        }
    }
}
#endif
