using System.Collections.Generic;
using CindarsHope.World.Scale;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.ScaleSystem
{
    /// <summary>
    /// Editor-side lookup for VisualScaleProfileSO assets so scene-creation scripts can attach a
    /// <see cref="VisualScaleApplicator"/> wired to the right profile, instead of hardcoding localScale.
    /// This is the single bridge between the authored scale profiles (Assets/_Game/Data/Scale) and the
    /// scenes the editor generators build — keeping the "intended scale" in one data location.
    /// </summary>
    public static class ScaleProfileLibrary
    {
        private const string ScaleDataFolder = "Assets/_Game/Data/Scale";

        private static Dictionary<EntityScaleCategory, VisualScaleProfileSO> s_byCategory;

        public static VisualScaleProfileSO Get(EntityScaleCategory category)
        {
            EnsureLoaded();
            return s_byCategory.TryGetValue(category, out var profile) ? profile : null;
        }

        /// <summary>
        /// Adds (or reuses) a <see cref="VisualScaleApplicator"/> on <paramref name="target"/> and wires it
        /// to the profile for <paramref name="category"/>. Returns false (and logs a wiring error) when the
        /// profile asset is missing so the caller can keep its previous hardcoded fallback.
        /// </summary>
        public static bool AttachApplicator(GameObject target, EntityScaleCategory category, Transform visualRoot = null)
        {
            if (target == null)
            {
                return false;
            }

            var profile = Get(category);
            if (profile == null)
            {
                // Recoverable: the caller keeps its hardcoded fallback scale. A warning (not an error) — the
                // scene is still usable; the fix is just to run the scale-asset generator before the creators.
                Debug.LogWarning(
                    $"[ScaleProfileLibrary] No VisualScaleProfileSO for category '{category}' under {ScaleDataFolder}. " +
                    $"GameObject='{target.name}'. Run 'CindarsHope/Generate/Data/Create Default Scale Assets' first. " +
                    "Falling back to the caller's hardcoded scale.");
                return false;
            }

            var applicator = target.GetComponent<VisualScaleApplicator>();
            if (applicator == null)
            {
                applicator = target.AddComponent<VisualScaleApplicator>();
            }

            applicator.Configure(profile, visualRoot);
            // Bake the visual scale at author time too, so the object reads correctly in the Scene view
            // (the runtime Apply() on Awake keeps it authoritative in play mode).
            target.transform.localScale = Vector3.one * profile.VisualScale;
            return true;
        }

        /// <summary>Clears the cached lookup. Call after (re)generating scale assets in the same editor session.</summary>
        public static void InvalidateCache() => s_byCategory = null;

        private static void EnsureLoaded()
        {
            if (s_byCategory != null)
            {
                return;
            }

            s_byCategory = new Dictionary<EntityScaleCategory, VisualScaleProfileSO>();
            var guids = AssetDatabase.FindAssets("t:VisualScaleProfileSO", new[] { ScaleDataFolder });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var profile = AssetDatabase.LoadAssetAtPath<VisualScaleProfileSO>(path);
                if (profile == null)
                {
                    continue;
                }

                // First asset of each category wins; duplicates are reported so authoring stays clean.
                if (!s_byCategory.ContainsKey(profile.Category))
                {
                    s_byCategory[profile.Category] = profile;
                }
                else
                {
                    Debug.LogWarning(
                        $"[ScaleProfileLibrary] Duplicate VisualScaleProfileSO for category '{profile.Category}' at {path}. " +
                        "Using the first one found.");
                }
            }
        }
    }
}
