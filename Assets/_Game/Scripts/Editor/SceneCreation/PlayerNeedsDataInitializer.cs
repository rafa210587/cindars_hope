using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Data;
using CindarsHope.Core.Time;
using CindarsHope.Player;
using CindarsHope.UI.Modal;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.SceneCreation
{
    internal static class PlayerNeedsDataInitializer
    {
        private const string ConfigFolder = "Assets/_Game/Data/Config";
        private const string PlayerNeedsBalancePath = ConfigFolder + "/PlayerNeedsBalance.asset";
        private const string GameTimeBalancePath = ConfigFolder + "/GameTimeBalance.asset";

        // arch: quebra do ciclo Core|UI (spec_arch_core_ui_cycle_reduction_v38) — GameTimeManager nao
        // tem mais campo serializado _modalManager (resolve via DomainManagerRegistry); o parametro
        // modalManager permanece para nao quebrar a assinatura chamada pelos 3 geradores de cena, mas
        // nao eh mais usado para wiring do GameTimeManager.
        public static void ConfigureRuntimeManagers(GameBootstrap bootstrap, TimeManager timeManager, ModalManager modalManager)
        {
            var playerNeedsBalance = EnsureAsset<PlayerNeedsBalanceSO>(PlayerNeedsBalancePath, "PlayerNeedsBalance");
            var gameTimeBalance = EnsureAsset<GameTimeBalanceSO>(GameTimeBalancePath, "GameTimeBalance");
            var hungerManager = bootstrap.GetComponent<HungerManager>();
            var staminaManager = bootstrap.GetComponent<StaminaManager>();
            var gameTimeManager = bootstrap.GetComponent<GameTimeManager>();

            SetReference(hungerManager, "_playerNeedsBalance", playerNeedsBalance);
            SetReference(staminaManager, "_playerNeedsBalance", playerNeedsBalance);
            SetReference(staminaManager, "_hungerManager", hungerManager);
            SetReference(gameTimeManager, "_timeBalance", gameTimeBalance);
            SetReference(gameTimeManager, "_timeManager", timeManager);
        }

        public static void ConfigurePlayerController(PlayerController playerController)
        {
            SetReference(playerController, "_playerNeedsBalance", EnsureAsset<PlayerNeedsBalanceSO>(PlayerNeedsBalancePath, "PlayerNeedsBalance"));
        }

        private static T EnsureAsset<T>(string assetPath, string assetName) where T : ScriptableObject
        {
            EnsureConfigFolder();
            var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            asset.name = assetName;
            AssetDatabase.CreateAsset(asset, assetPath);
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static void SetReference(Object target, string propertyName, Object value)
        {
            if (target == null)
            {
                return;
            }

            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void EnsureConfigFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Game/Data"))
            {
                AssetDatabase.CreateFolder("Assets/_Game", "Data");
            }

            if (!AssetDatabase.IsValidFolder(ConfigFolder))
            {
                AssetDatabase.CreateFolder("Assets/_Game/Data", "Config");
            }
        }
    }
}
