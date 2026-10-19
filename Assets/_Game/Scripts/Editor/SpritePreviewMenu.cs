using System.IO;
using UnityEditor;
using UnityEngine;
using CindarsHope.Visual;

namespace CindarsHope.Editor
{
    /// <summary>
    /// Dev tool: instancia os sprites gerados (Assets/_Game/Art/Generated/Preview)
    /// numa fileira na cena ativa, cada um com SpriteJuice, pra ver o juice em Play Mode.
    /// Nao integra com catalogos de gameplay; e so visualizacao isolada.
    /// </summary>
    public static class SpritePreviewMenu
    {
        private const string Folder = "Assets/_Game/Art/Generated/Preview";

        [MenuItem("CindarsHope/Dev/Preview Generated Sprites")]
        public static void Preview()
        {
            var guids = AssetDatabase.FindAssets("t:Sprite", new[] { Folder });
            if (guids.Length == 0)
            {
                Debug.LogWarning("[SpritePreview] nenhum sprite em " + Folder);
                return;
            }

            var existing = GameObject.Find("_SpritePreview");
            if (existing != null) Object.DestroyImmediate(existing);

            var root = new GameObject("_SpritePreview");
            Undo.RegisterCreatedObjectUndo(root, "Preview Generated Sprites");

            float x = 0f;
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite == null) continue;

                var go = new GameObject(Path.GetFileNameWithoutExtension(path));
                go.transform.SetParent(root.transform);
                go.transform.localPosition = new Vector3(x, 0f, 0f);

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = 10;

                go.AddComponent<SpriteJuice>();
                x += 2f;
            }

            Selection.activeGameObject = root;
            SceneView.FrameLastActiveSceneView();
            Debug.Log("[SpritePreview] " + guids.Length + " sprites instanciados sob '_SpritePreview'. Aperte Play pra ver o juice (idle bob).");
        }
    }
}
