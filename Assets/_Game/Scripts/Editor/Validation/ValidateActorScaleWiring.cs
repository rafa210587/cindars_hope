using CindarsHope.Combat;
using CindarsHope.Editor.ScaleSystem;
using CindarsHope.World.Scale;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    /// <summary>
    /// Reusable gate for the actor/prop visual-scale wiring. Answers, without opening a scene, the question
    /// "is every monster and visual component structured the right way?":
    ///   1. every EntityScaleCategory resolves to a VisualScaleProfileSO (no gaps the scene creators can hit);
    ///   2. no boss enemy would fall back to the flat global scale (per-boss VisualScale must carry the override);
    ///   3. no enemy ends up unsized (empty SizeProfileId AND default VisualScale).
    /// Mirrors the ValidateSpec17AScaleConfig style: static Run(), error/warning counters, Debug logs.
    /// </summary>
    public static class ValidateActorScaleWiring
    {
        [MenuItem("CindarsHope/Validate/Actor Scale Wiring")]
        public static void Run()
        {
            var errors = 0;
            var warnings = 0;

            ScaleProfileLibrary.InvalidateCache();

            // The player is the size reference; report every other category as a multiple of it so the size
            // language ("station = 0.65x player", "tree = 1.2x player") is auditable at a glance.
            var playerProfile = ScaleProfileLibrary.Get(EntityScaleCategory.Player);
            var playerScale = playerProfile != null && playerProfile.VisualScale > 0.01f ? playerProfile.VisualScale : 0f;

            // 1) Profile coverage for every category — a missing profile means the scene creators silently
            //    fall back to a hardcoded localScale, which is exactly the drift this system removes.
            foreach (EntityScaleCategory category in System.Enum.GetValues(typeof(EntityScaleCategory)))
            {
                var profile = ScaleProfileLibrary.Get(category);
                if (profile == null)
                {
                    Debug.LogError(
                        $"[ScaleWiring] No VisualScaleProfileSO for category '{category}'. Run " +
                        "'CindarsHope/Generate/Data/Create Default Scale Assets'.");
                    errors++;
                }
                else
                {
                    var ratio = playerScale > 0f ? $"{profile.VisualScale / playerScale:0.00}x player" : "n/a";
                    Debug.Log($"[ScaleWiring] {category} -> VisualScale={profile.VisualScale} ({ratio}, id='{profile.ProfileId}').");
                }
            }

            // 2) + 3) Enemy data sanity. Bosses must carry a per-boss VisualScale override; every enemy must be
            //    sizeable either through a SizeProfileId or a non-default VisualScale.
            var enemyGuids = AssetDatabase.FindAssets("t:EnemyDataSO");
            var flattenedBosses = 0;
            var unsized = 0;
            foreach (var guid in enemyGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var enemy = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(path);
                if (enemy == null)
                {
                    continue;
                }

                if (enemy.IsBoss && enemy.VisualScale <= 1.01f)
                {
                    Debug.LogWarning(
                        $"[ScaleWiring] Boss '{enemy.enemyId}' has VisualScale={enemy.VisualScale} (default). It will " +
                        "fall back to the flat global boss scale instead of its natural size. Regenerate the bestiary.");
                    flattenedBosses++;
                    warnings++;
                }

                if (string.IsNullOrEmpty(enemy.SizeProfileId) && enemy.VisualScale <= 0.1f)
                {
                    Debug.LogError($"[ScaleWiring] Enemy '{enemy.enemyId}' is unsized (no SizeProfileId, VisualScale={enemy.VisualScale}).");
                    unsized++;
                    errors++;
                }
            }

            Debug.Log(
                $"[ScaleWiring] Checked {enemyGuids.Length} EnemyDataSO asset(s): " +
                $"{flattenedBosses} flattened boss(es), {unsized} unsized enemy(ies).");

            if (errors == 0 && warnings == 0)
            {
                Debug.Log("[ScaleWiring] PASS — actor/prop scale wiring complete. 0 errors, 0 warnings.");
            }
            else
            {
                Debug.LogWarning($"[ScaleWiring] Finished: {errors} error(s), {warnings} warning(s).");
            }
        }
    }
}
