#if UNITY_EDITOR

using CindarsHope.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Editor.SceneCreation
{
    /// <summary>
    /// Aplica o valor canonico de regen base de mana (<see cref="ManaManager.DefaultManaRegenPerSecond"/>)
    /// as instancias do ManaManager nas 3 cenas de gameplay, via SerializedObject (NAO edita YAML manual
    /// — rule unity-assets). Idempotente: so escreve/salva a cena quando o valor difere.
    ///
    /// Necessario porque cada cena serializa o proprio _manaRegenPerSecond (override do default do codigo);
    /// mudar so o default do ManaManager nao afeta cenas ja autoradas.
    /// </summary>
    public static class ManaRegenSceneInitializer
    {
        private static readonly string[] ScenePaths =
        {
            "Assets/_Game/Scenes/FarmScene.unity",
            "Assets/_Game/Scenes/TownScene.unity",
            "Assets/_Game/Scenes/CaveScene.unity",
        };

        [MenuItem("CindarsHope/Dev/Aplicar Regen de Mana Canonica nas Cenas", priority = 42)]
        public static void ApplyToAllScenes()
        {
            var activeScenePath = SceneManager.GetActiveScene().path;
            int updated = 0;

            foreach (var path in ScenePaths)
            {
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                bool changed = ApplyToOpenScene();
                if (changed)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    updated++;
                    Debug.Log($"[ManaRegenSceneInitializer] {path}: regen de mana ajustada para {ManaManager.DefaultManaRegenPerSecond}/s.");
                }
                else
                {
                    Debug.Log($"[ManaRegenSceneInitializer] {path}: ja em {ManaManager.DefaultManaRegenPerSecond}/s (no-op).");
                }
            }

            // Reabre a cena que estava ativa (best-effort) para nao deixar o editor numa cena trocada.
            if (!string.IsNullOrEmpty(activeScenePath))
            {
                EditorSceneManager.OpenScene(activeScenePath, OpenSceneMode.Single);
            }

            Debug.Log($"[ManaRegenSceneInitializer] Concluido: {updated} cena(s) atualizada(s) para regen {ManaManager.DefaultManaRegenPerSecond}/s.");
        }

        // SerializedObject no _manaRegenPerSecond de cada ManaManager da cena. Retorna true se mudou.
        private static bool ApplyToOpenScene()
        {
            var managers = Object.FindObjectsByType<ManaManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            bool changed = false;

            foreach (var manager in managers)
            {
                var so = new SerializedObject(manager);
                var prop = so.FindProperty("_manaRegenPerSecond");
                if (prop == null)
                {
                    continue;
                }

                if (!Mathf.Approximately(prop.floatValue, ManaManager.DefaultManaRegenPerSecond))
                {
                    prop.floatValue = ManaManager.DefaultManaRegenPerSecond;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(manager);
                    changed = true;
                }
            }

            return changed;
        }
    }
}

#endif
