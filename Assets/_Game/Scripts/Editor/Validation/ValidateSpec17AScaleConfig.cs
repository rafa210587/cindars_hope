using CindarsHope.Camera;
using CindarsHope.Cave.Data;
using CindarsHope.Core.Data;
using CindarsHope.World.Scale;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    public static class ValidateSpec17AScaleConfig
    {
        private const string CaveConfigPath = "Assets/_Game/Data/Cave/CaveGenerationConfig_Default.asset";
        private const string GameScaleConfigPath = "Assets/_Game/Data/Config/GameScaleConfig.asset";

        [MenuItem("CindarsHope/Validation/Validate Spec 17A - Scale Config")]
        public static void Run()
        {
            var errors = 0;
            var warnings = 0;

            // VisualScaleProfileSO assets
            var profileGuids = AssetDatabase.FindAssets("t:VisualScaleProfileSO", new[] { "Assets/_Game/Data/Scale" });
            if (profileGuids.Length == 0)
            {
                Debug.LogWarning("[17A] No VisualScaleProfileSO assets found in Assets/_Game/Data/Scale/. Run 'CindarsHope/Generate/Data/Create Default Scale Assets' first.");
                warnings++;
            }
            else
            {
                Debug.Log($"[17A] Found {profileGuids.Length} VisualScaleProfileSO asset(s).");
                foreach (var guid in profileGuids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var profile = AssetDatabase.LoadAssetAtPath<VisualScaleProfileSO>(path);
                    if (profile == null) continue;

                    if (string.IsNullOrEmpty(profile.ProfileId))
                    {
                        Debug.LogError($"[17A] {path} has empty ProfileId.");
                        errors++;
                    }

                    if (profile.VisualScale < 0.1f)
                    {
                        Debug.LogError($"[17A] {path} VisualScale={profile.VisualScale} is below minimum 0.1.");
                        errors++;
                    }
                }
            }

            // CameraScaleConfigSO asset
            var cameraGuids = AssetDatabase.FindAssets("t:CameraScaleConfigSO", new[] { "Assets/_Game/Data/Camera" });
            if (cameraGuids.Length == 0)
            {
                Debug.LogWarning("[17A] No CameraScaleConfigSO asset found in Assets/_Game/Data/Camera/. Run 'CindarsHope/Generate/Data/Create Default Scale Assets' first.");
                warnings++;
            }
            else
            {
                var path = AssetDatabase.GUIDToAssetPath(cameraGuids[0]);
                var config = AssetDatabase.LoadAssetAtPath<CameraScaleConfigSO>(path);
                if (config != null)
                {
                    if (config.FarmOrthographicSize <= 0f || config.TownOrthographicSize <= 0f ||
                        config.CaveOrthographicSize <= 0f || config.BossArenaOrthographicSize <= 0f)
                    {
                        Debug.LogError($"[17A] {path} has zero or negative orthographic size values.");
                        errors++;
                    }
                    else
                    {
                        Debug.Log($"[17A] CameraScaleConfigSO OK — Farm={config.FarmOrthographicSize} Town={config.TownOrthographicSize} Cave={config.CaveOrthographicSize} Boss={config.BossArenaOrthographicSize}");
                    }
                }
            }

            // CameraScaleController in scene cameras
            var controllers = Object.FindObjectsByType<CameraScaleController>(FindObjectsInactive.Include);
            if (controllers.Length == 0)
            {
                Debug.LogWarning("[17A] No CameraScaleController found in open scene. Wire it to the main camera in each scene.");
                warnings++;
            }
            else
            {
                foreach (var c in controllers)
                {
                    Debug.Log($"[17A] CameraScaleController found on '{c.gameObject.name}'.");
                }
            }

            // Cave generation config — numeric targets
            var caveConfig = AssetDatabase.LoadAssetAtPath<CaveGenerationConfigSO>(CaveConfigPath);
            if (caveConfig == null)
            {
                Debug.LogWarning($"[17A] CaveGenerationConfig_Default not found at {CaveConfigPath}.");
                warnings++;
            }
            else
            {
                if (caveConfig.TargetWidth < 160)
                {
                    Debug.LogError($"[17A] CaveGenerationConfig TargetWidth={caveConfig.TargetWidth}, expected >= 160 (2x baseline).");
                    errors++;
                }
                if (caveConfig.TargetHeight < 96)
                {
                    Debug.LogError($"[17A] CaveGenerationConfig TargetHeight={caveConfig.TargetHeight}, expected >= 96 (2x baseline).");
                    errors++;
                }
                if (caveConfig.CorridorMinWidth < 2)
                {
                    Debug.LogError($"[17A] CaveGenerationConfig CorridorMinWidth={caveConfig.CorridorMinWidth}, expected >= 2.");
                    errors++;
                }
                if (caveConfig.CorridorMaxWidth < 3)
                {
                    Debug.LogError($"[17A] CaveGenerationConfig CorridorMaxWidth={caveConfig.CorridorMaxWidth}, expected >= 3.");
                    errors++;
                }
                if (caveConfig.MinRoomWidth < 12)
                {
                    Debug.LogError($"[17A] CaveGenerationConfig MinRoomWidth={caveConfig.MinRoomWidth}, expected >= 12 (2x baseline).");
                    errors++;
                }
                Debug.Log($"[17A] CaveGenerationConfig OK — Width={caveConfig.TargetWidth} Height={caveConfig.TargetHeight} CorridorMin={caveConfig.CorridorMinWidth} CorridorMax={caveConfig.CorridorMaxWidth} RoomW={caveConfig.MinRoomWidth}-{caveConfig.MaxRoomWidth} RoomH={caveConfig.MinRoomHeight}-{caveConfig.MaxRoomHeight} Version={caveConfig.GenerationConfigVersion}");
            }

            // GameScaleConfigSO — boss, tree, lake targets
            var scaleConfig = AssetDatabase.LoadAssetAtPath<GameScaleConfigSO>(GameScaleConfigPath);
            if (scaleConfig == null)
            {
                Debug.LogWarning($"[17A] GameScaleConfig not found at {GameScaleConfigPath}. Run 'CindarsHope/Generate/Data/Create Default Scale Assets' first.");
                warnings++;
            }
            else
            {
                if (scaleConfig.BossScale < 2f)
                {
                    Debug.LogError($"[17A] GameScaleConfig BossScale={scaleConfig.BossScale}, expected >= 2.0 (2x player).");
                    errors++;
                }
                if (scaleConfig.TreeScale < 3f)
                {
                    Debug.LogError($"[17A] GameScaleConfig TreeScale={scaleConfig.TreeScale}, expected >= 3.0 (~3x player).");
                    errors++;
                }
                if (scaleConfig.LakeScale < 6f)
                {
                    Debug.LogError($"[17A] GameScaleConfig LakeScale={scaleConfig.LakeScale}, expected >= 6.0 (~6x player).");
                    errors++;
                }
                Debug.Log($"[17A] GameScaleConfig OK — Boss={scaleConfig.BossScale} (min={scaleConfig.BossMinScale} max={scaleConfig.BossMaxScale}) Tree={scaleConfig.TreeScale} Lake={scaleConfig.LakeScale}");
            }

            if (errors == 0 && warnings == 0)
            {
                Debug.Log("[17A] PASS — Scale config validation complete. 0 errors, 0 warnings.");
            }
            else
            {
                Debug.LogWarning($"[17A] Validation finished: {errors} error(s), {warnings} warning(s).");
            }
        }
    }
}
