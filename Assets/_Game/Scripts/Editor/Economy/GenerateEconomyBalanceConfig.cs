using CindarsHope.Economy;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Economy
{
    public static class GenerateEconomyBalanceConfig
    {
        private const string ConfigDir  = "Assets/_Game/Data/Config";
        private const string ConfigPath = ConfigDir + "/EconomyBalanceConfig.asset";

        [MenuItem("CindarsHope/Economy/Generate Economy Balance Config")]
        public static void Generate()
        {
            if (!AssetDatabase.IsValidFolder(ConfigDir))
            {
                var parent = "Assets/_Game/Data";
                AssetDatabase.CreateFolder(parent, "Config");
                Debug.Log($"[GenerateEconomyBalanceConfig] Created folder {ConfigDir}");
            }

            var existing = AssetDatabase.LoadAssetAtPath<EconomyBalanceConfigSO>(ConfigPath);
            if (existing != null)
            {
                Debug.Log($"[GenerateEconomyBalanceConfig] Asset already exists at {ConfigPath} — skipped.");
                Selection.activeObject = existing;
                return;
            }

            var config = ScriptableObject.CreateInstance<EconomyBalanceConfigSO>();
            AssetDatabase.CreateAsset(config, ConfigPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = config;
            Debug.Log($"[GenerateEconomyBalanceConfig] Created {ConfigPath}");
        }
    }
}
