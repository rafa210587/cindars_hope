using CindarsHope.Camera;
using CindarsHope.World.Scale;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    public static class ValidateSpec17AScaleConfig
    {
        [MenuItem("Cindar's Hope/Validation/Validate Spec 17A - Scale Config")]
        public static void Run()
        {
            var errors = 0;
            var warnings = 0;

            // VisualScaleProfileSO assets
            var profileGuids = AssetDatabase.FindAssets("t:VisualScaleProfileSO", new[] { "Assets/_Game/Data/Scale" });
            if (profileGuids.Length == 0)
            {
                Debug.LogWarning("[17A] No VisualScaleProfileSO assets found in Assets/_Game/Data/Scale/. Run 'Cindar's Hope/Scale/Create Default Scale Assets' first.");
                warnings++;
            }
            else
            {
                Debug.Log($"[17A] Found {profileGuids.Length} VisualScaleProfileSO asset(s).");
                foreach (var guid in profileGuids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var profile = AssetDatabase.LoadAssetAtPath<VisualScaleProfileSO>(path);
                    if (profile == null)
                    {
                        continue;
                    }

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
                Debug.LogWarning("[17A] No CameraScaleConfigSO asset found in Assets/_Game/Data/Camera/. Run 'Cindar's Hope/Scale/Create Default Scale Assets' first.");
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
            var controllers = Object.FindObjectsByType<CameraScaleController>(FindObjectsSortMode.None);
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
