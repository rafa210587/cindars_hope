using UnityEditor;
using UnityEngine;
using CindarsHope.Combat;

namespace CindarsHope.Editor
{
    /// <summary>
    /// Dev tool: atribui o campo Icon de cada EnemyDataSO ao sprite gerado correspondente
    /// em Assets/_Game/Art/Generated/Enemies, casando por id (enemyId 'enemy_&lt;slug&gt;' -> '&lt;slug&gt;.png').
    /// Via Editor API (SetDirty/SaveAssets); nao edita YAML na mao.
    /// </summary>
    public static class AssignEnemySprites
    {
        private const string EnemyFolder = "Assets/_Game/Art/Generated/Enemies";

        [MenuItem("CindarsHope/Dev/Assign Enemy Sprites")]
        public static void Assign()
        {
            int assigned = 0, missing = 0, noId = 0;
            var guids = AssetDatabase.FindAssets("t:EnemyDataSO");
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var so = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(path);
                if (so == null) continue;

                var id = so.enemyId;
                if (string.IsNullOrEmpty(id)) { noId++; continue; }

                var slug = id.StartsWith("enemy_") ? id.Substring("enemy_".Length) : id;
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(EnemyFolder + "/" + slug + ".png");
                if (sprite != null)
                {
                    so.Icon = sprite;
                    EditorUtility.SetDirty(so);
                    assigned++;
                }
                else
                {
                    missing++;
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[AssignEnemySprites] Atribuidos {assigned} sprites a EnemyDataSO. " +
                      $"Sem sprite correspondente: {missing}. Sem id: {noId}. " +
                      $"Aperte Play na CaveScene e entre na caverna pra ver os sprites.");
        }
    }
}
