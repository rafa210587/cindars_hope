#if UNITY_EDITOR

using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Data;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CindarsHope.EditorTools.Repair
{
    /// <summary>
    /// Repairs TownScene combat bootstrap wiring by assigning missing databases and ManaManager to GameBootstrap.
    /// Residual fix for validator failures where WeaponDatabase, SpellDatabase, StatusEffectDatabase, ManaManager are null.
    /// </summary>
    public static class RepairTownSceneCombatBootstrapWiring
    {
        private const string TownScenePath = "Assets/_Game/Scenes/TownScene.unity";
        private const string BootstrapName = "_Bootstrap";
        private const string WeaponDatabasePath = "Assets/_Game/Data/Combat/WeaponDatabase.asset";
        private const string SpellDatabasePath = "Assets/_Game/Data/Combat/SpellDatabase.asset";
        private const string StatusEffectDatabasePath = "Assets/_Game/Data/Combat/StatusEffectDatabase.asset";

        [MenuItem("CindarsHope/Repair/Scenes/Repair TownScene Combat Bootstrap Wiring")]
        public static void Repair()
        {
            Debug.Log("RepairTownSceneCombatBootstrapWiring: Starting repair...");

            // Load TownScene
            var scene = EditorSceneManager.OpenScene(TownScenePath, OpenSceneMode.Single);
            if (string.IsNullOrEmpty(scene.name))
            {
                Debug.LogError($"RepairTownSceneCombatBootstrapWiring: Cannot open scene at {TownScenePath}");
                return;
            }

            // Find _Bootstrap GameObject
            var bootstrapObject = GameObject.Find(BootstrapName);
            if (bootstrapObject == null)
            {
                Debug.LogError($"RepairTownSceneCombatBootstrapWiring: Cannot find GameObject '{BootstrapName}' in scene.");
                return;
            }

            // Get or add ManaManager component
            var manaManager = bootstrapObject.GetComponent<ManaManager>();
            if (manaManager == null)
            {
                manaManager = bootstrapObject.AddComponent<ManaManager>();
                Debug.Log($"RepairTownSceneCombatBootstrapWiring: Added ManaManager component to {BootstrapName}");
                EditorUtility.SetDirty(bootstrapObject);
            }

            // Get GameBootstrap
            var bootstrap = bootstrapObject.GetComponent<GameBootstrap>();
            if (bootstrap == null)
            {
                Debug.LogError($"RepairTownSceneCombatBootstrapWiring: Cannot find GameBootstrap component on {BootstrapName}");
                return;
            }

            var serializedBootstrap = new SerializedObject(bootstrap);

            // Assign ManaManager
            var manaManagerProp = serializedBootstrap.FindProperty("_manaManager");
            if (manaManagerProp != null)
            {
                manaManagerProp.objectReferenceValue = manaManager;
                Debug.Log($"RepairTownSceneCombatBootstrapWiring: Assigned ManaManager to GameBootstrap._manaManager");
            }

            // Load and assign WeaponDatabase
            var weaponDatabase = AssetDatabase.LoadAssetAtPath<WeaponDatabaseSO>(WeaponDatabasePath);
            if (weaponDatabase != null)
            {
                var weaponDatabaseProp = serializedBootstrap.FindProperty("_weaponDatabase");
                if (weaponDatabaseProp != null)
                {
                    weaponDatabaseProp.objectReferenceValue = weaponDatabase;
                    Debug.Log($"RepairTownSceneCombatBootstrapWiring: Assigned WeaponDatabase to GameBootstrap._weaponDatabase");
                }
            }
            else
            {
                Debug.LogWarning($"RepairTownSceneCombatBootstrapWiring: WeaponDatabaseSO not found at {WeaponDatabasePath}. Assign it manually.");
            }

            // Load and assign SpellDatabase
            var spellDatabase = AssetDatabase.LoadAssetAtPath<SpellDatabaseSO>(SpellDatabasePath);
            if (spellDatabase != null)
            {
                var spellDatabaseProp = serializedBootstrap.FindProperty("_spellDatabase");
                if (spellDatabaseProp != null)
                {
                    spellDatabaseProp.objectReferenceValue = spellDatabase;
                    Debug.Log($"RepairTownSceneCombatBootstrapWiring: Assigned SpellDatabase to GameBootstrap._spellDatabase");
                }
            }
            else
            {
                Debug.LogWarning($"RepairTownSceneCombatBootstrapWiring: SpellDatabaseSO not found at {SpellDatabasePath}. Assign it manually.");
            }

            // Load and assign StatusEffectDatabase (optional)
            var statusEffectDatabase = AssetDatabase.LoadAssetAtPath<StatusEffectDatabaseSO>(StatusEffectDatabasePath);
            if (statusEffectDatabase != null)
            {
                var statusEffectDatabaseProp = serializedBootstrap.FindProperty("_statusEffectDatabase");
                if (statusEffectDatabaseProp != null)
                {
                    statusEffectDatabaseProp.objectReferenceValue = statusEffectDatabase;
                    Debug.Log($"RepairTownSceneCombatBootstrapWiring: Assigned StatusEffectDatabase to GameBootstrap._statusEffectDatabase");
                }
            }
            else
            {
                Debug.LogWarning($"RepairTownSceneCombatBootstrapWiring: StatusEffectDatabaseSO not found at {StatusEffectDatabasePath}. Optional; fallback will be used.");
            }

            // Apply changes
            serializedBootstrap.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(bootstrap);
            EditorUtility.SetDirty(bootstrapObject);

            // Mark scene dirty and save
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.Refresh();

            Debug.Log($"RepairTownSceneCombatBootstrapWiring: Repair completed successfully. TownScene saved.");
        }
    }
}

#endif
