#if UNITY_EDITOR
using System;
using CindarsHope.Core.Bootstrap;
using CindarsHope.NPC;
using CindarsHope.Player.Progression;
using CindarsHope.Save;
using CindarsHope.Skills;
using CindarsHope.UI.Shop;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CindarsHope.EditorTools
{
    public static class Spec17CSceneWiringInitializer
    {
        private static readonly string[] GameplayScenes =
        {
            "Assets/_Game/Scenes/FarmScene.unity",
            "Assets/_Game/Scenes/TownScene.unity",
            "Assets/_Game/Scenes/CaveScene.unity"
        };

        public static void ApplySceneWiring()
        {
            foreach (var scenePath in GameplayScenes)
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                ApplySkillTreeWiring(scene);
                if (scene.name == "TownScene")
                {
                    RepairTownShopTemplates(scene);
                    ValidateTownShopWiring(scene);
                }

                EditorSceneManager.SaveScene(scene);
                VerifyPersistedSkillTreeWiring(scenePath);
                Debug.Log($"[SPEC17C] Wiring validated for scene '{scenePath}'.");
            }

            AssetDatabase.SaveAssets();
        }

        private static void ApplySkillTreeWiring(Scene scene)
        {
            var bootstrap = FindInScene<GameBootstrap>(scene);
            if (bootstrap == null)
            {
                throw new InvalidOperationException($"[SPEC17C] Scene '{scene.path}' has no GameBootstrap.");
            }

            var skillTreeManager = bootstrap.GetComponent<SkillTreeManager>();
            if (skillTreeManager == null)
            {
                skillTreeManager = bootstrap.gameObject.AddComponent<SkillTreeManager>();
                EditorUtility.SetDirty(bootstrap.gameObject);
            }

            var progressionManager = bootstrap.GetComponent<PlayerProgressionManager>();
            if (progressionManager == null)
            {
                throw new InvalidOperationException($"[SPEC17C] Scene '{scene.path}' bootstrap has no PlayerProgressionManager.");
            }

            SetReference(bootstrap, "_skillTreeManager", skillTreeManager, scene.path);
            SetReference(skillTreeManager, "_progressionManager", progressionManager, scene.path);

            var saveManager = bootstrap.GetComponent<SaveManager>();
            if (saveManager == null)
            {
                throw new InvalidOperationException($"[SPEC17C] Scene '{scene.path}' bootstrap has no SaveManager.");
            }

            SetReference(saveManager, "_skillTreeManager", skillTreeManager, scene.path);
            EditorUtility.SetDirty(bootstrap.gameObject);
            EditorSceneManager.MarkSceneDirty(scene);
        }

        private static void VerifyPersistedSkillTreeWiring(string scenePath)
        {
            var reopenedScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var bootstrap = FindInScene<GameBootstrap>(reopenedScene);
            var skillTreeManager = bootstrap != null ? bootstrap.GetComponent<SkillTreeManager>() : null;
            if (bootstrap == null || skillTreeManager == null)
            {
                throw new InvalidOperationException($"[SPEC17C] Scene '{scenePath}' did not persist its SkillTreeManager component.");
            }

            RequireReference(bootstrap, "_skillTreeManager", skillTreeManager, scenePath);
            RequireReference(bootstrap.GetComponent<SaveManager>(), "_skillTreeManager", skillTreeManager, scenePath);
        }

        private static void ValidateTownShopWiring(Scene scene)
        {
            foreach (var controller in FindAllInScene<NpcShopController>(scene))
            {
                var serializedController = new SerializedObject(controller);
                foreach (var fieldName in new[]
                {
                    "_npcData", "_shopData", "_playerManager", "_inventoryManager", "_itemDatabase",
                    "_shopManager", "_dialogueModal", "_shopMenuModal", "_buyPanel", "_sellPanel", "_modalManager"
                })
                {
                    var property = serializedController.FindProperty(fieldName);
                    if (property == null || property.objectReferenceValue == null)
                    {
                        throw new InvalidOperationException(
                            $"[SPEC17C] Scene '{scene.path}' GameObject '{controller.name}' component '{nameof(NpcShopController)}' missing reference '{fieldName}'.");
                    }
                }
            }
        }

        private static void RepairTownShopTemplates(Scene scene)
        {
            var buyPanel = FindInScene<BuyPanel>(scene);
            var sellPanel = FindInScene<SellPanel>(scene);
            var buyTemplate = FindFirstGameObject(
                scene,
                "ShopCanvas/BuyPanel/ItemsScroll/Content/BuyItemTemplate",
                "ShopCanvas/BuyPanel/Items/BuyItemTemplate");
            var sellTemplate = FindFirstGameObject(
                scene,
                "ShopCanvas/SellPanel/ItemsScroll/Content/SellItemTemplate",
                "ShopCanvas/SellPanel/Items/SellItemTemplate");
            if (buyPanel == null || sellPanel == null || buyTemplate == null || sellTemplate == null)
            {
                throw new InvalidOperationException($"[SPEC17C] Scene '{scene.path}' shop panels or templates were not found.");
            }

            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(buyTemplate);
            var buyItem = buyTemplate.GetComponent<BuyPanelItem>() ?? buyTemplate.AddComponent<BuyPanelItem>();
            SetReference(buyItem, "_itemNameText", RequireChild<Text>(buyTemplate, "Name", scene.path), scene.path);
            SetReference(buyItem, "_priceText", RequireChild<Text>(buyTemplate, "Price", scene.path), scene.path);
            SetReference(buyItem, "_stockText", RequireChild<Text>(buyTemplate, "Stock", scene.path), scene.path);
            SetReference(buyItem, "_amountInput", RequireChild<InputField>(buyTemplate, "Amount", scene.path), scene.path);
            SetReference(buyItem, "_buyButton", RequireChild<Button>(buyTemplate, "Buy", scene.path), scene.path);
            SetReference(buyPanel, "_itemPrefab", buyItem, scene.path);

            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(sellTemplate);
            var sellItem = sellTemplate.GetComponent<SellPanelItem>() ?? sellTemplate.AddComponent<SellPanelItem>();
            SetReference(sellItem, "_itemNameText", RequireChild<Text>(sellTemplate, "Name", scene.path), scene.path);
            SetReference(sellItem, "_priceText", RequireChild<Text>(sellTemplate, "Price", scene.path), scene.path);
            SetReference(sellItem, "_amountText", RequireChild<Text>(sellTemplate, "AmountOwned", scene.path), scene.path);
            SetReference(sellItem, "_amountInput", RequireChild<InputField>(sellTemplate, "Amount", scene.path), scene.path);
            SetReference(sellItem, "_sellButton", RequireChild<Button>(sellTemplate, "Sell", scene.path), scene.path);
            SetReference(sellPanel, "_itemPrefab", sellItem, scene.path);

            EditorUtility.SetDirty(buyTemplate);
            EditorUtility.SetDirty(sellTemplate);
            EditorSceneManager.MarkSceneDirty(scene);
        }

        private static T FindInScene<T>(Scene scene) where T : Component
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var component = root.GetComponentInChildren<T>(true);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }

        private static T[] FindAllInScene<T>(Scene scene) where T : Component
        {
            var results = new System.Collections.Generic.List<T>();
            foreach (var root in scene.GetRootGameObjects())
            {
                results.AddRange(root.GetComponentsInChildren<T>(true));
            }

            return results.ToArray();
        }

        private static GameObject FindGameObject(Scene scene, string path)
        {
            var separatorIndex = path.IndexOf('/');
            var rootName = separatorIndex >= 0 ? path.Substring(0, separatorIndex) : path;
            var childPath = separatorIndex >= 0 ? path.Substring(separatorIndex + 1) : string.Empty;
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name != rootName)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(childPath))
                {
                    return root;
                }

                return root.transform.Find(childPath)?.gameObject;
            }

            return null;
        }

        private static GameObject FindFirstGameObject(Scene scene, params string[] paths)
        {
            foreach (var path in paths)
            {
                var value = FindGameObject(scene, path);
                if (value != null)
                {
                    return value;
                }
            }

            return null;
        }

        private static T RequireChild<T>(GameObject parent, string childName, string scenePath) where T : Component
        {
            var child = parent.transform.Find(childName);
            var component = child != null ? child.GetComponent<T>() : null;
            if (component == null)
            {
                throw new InvalidOperationException(
                    $"[SPEC17C] Scene '{scenePath}' GameObject '{parent.name}' missing child component '{childName}/{typeof(T).Name}'.");
            }

            return component;
        }

        private static void SetReference(UnityEngine.Object target, string fieldName, UnityEngine.Object value, string scenePath)
        {
            var serializedTarget = new SerializedObject(target);
            var property = serializedTarget.FindProperty(fieldName);
            if (property == null)
            {
                throw new InvalidOperationException(
                    $"[SPEC17C] Scene '{scenePath}' component '{target.GetType().Name}' has no serialized field '{fieldName}'.");
            }

            property.objectReferenceValue = value;
            serializedTarget.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void RequireReference(UnityEngine.Object target, string fieldName, UnityEngine.Object expected, string scenePath)
        {
            if (target == null)
            {
                throw new InvalidOperationException($"[SPEC17C] Scene '{scenePath}' has no target for required field '{fieldName}'.");
            }

            var property = new SerializedObject(target).FindProperty(fieldName);
            if (property == null || property.objectReferenceValue != expected)
            {
                throw new InvalidOperationException($"[SPEC17C] Scene '{scenePath}' did not persist required reference '{fieldName}'.");
            }
        }
    }
}
#endif
