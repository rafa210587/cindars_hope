#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CindarsHope.EditorTools
{
    /// <summary>
    /// Dev-only: faz o Play Mode do Editor SEMPRE comecar na FarmScene (entry do jogo), independente
    /// de qual cena esteja aberta. Sem isto, apertar Play com a CaveScene aberta inicia na caverna.
    ///
    /// Build standalone ja inicia na FarmScene (BuildSceneRegistrar registra FarmScene como index 0);
    /// este setter cobre apenas a conveniencia do Editor via EditorSceneManager.playModeStartScene.
    ///
    /// Toggle em CindarsHope/Dev (default ON). Desligue para testar uma cena especifica direto no Play.
    /// </summary>
    [InitializeOnLoad]
    public static class PlayModeStartSceneSetter
    {
        private const string FarmScenePath = "Assets/_Game/Scenes/FarmScene.unity";
        private const string EnabledPrefKey = "CindarsHope.PlayModeStartFarm.Enabled";
        private const string MenuPath = "CindarsHope/Dev/Play Mode comeca na FarmScene";

        static PlayModeStartSceneSetter()
        {
            // delayCall: AssetDatabase pode nao estar pronto no construtor estatico do domain reload.
            EditorApplication.delayCall += Apply;
        }

        private static bool Enabled
        {
            get => EditorPrefs.GetBool(EnabledPrefKey, true);
            set => EditorPrefs.SetBool(EnabledPrefKey, value);
        }

        private static void Apply()
        {
            if (!Enabled)
            {
                EditorSceneManager.playModeStartScene = null;
                return;
            }

            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(FarmScenePath);
            if (sceneAsset == null)
            {
                Debug.LogWarning($"[PlayModeStart] FarmScene nao encontrada em {FarmScenePath}; Play Mode start nao definido.");
                return;
            }

            EditorSceneManager.playModeStartScene = sceneAsset;
        }

        [MenuItem(MenuPath, priority = 41)]
        private static void Toggle()
        {
            Enabled = !Enabled;
            Apply();
            Debug.Log($"[PlayModeStart] Play Mode comeca na FarmScene: {(Enabled ? "ON" : "OFF")}.");
        }

        [MenuItem(MenuPath, validate = true)]
        private static bool ToggleValidate()
        {
            Menu.SetChecked(MenuPath, Enabled);
            return true;
        }
    }
}

#endif
