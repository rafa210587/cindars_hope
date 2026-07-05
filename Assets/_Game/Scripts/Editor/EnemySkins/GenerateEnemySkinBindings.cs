using System.Collections.Generic;
using System.IO;
using System.Linq;
using CindarsHope.Enemy;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.EnemySkins
{
    /// <summary>
    /// Gera/atualiza (idempotente, ADITIVO) o JSON de bindings de skin de inimigo em
    /// Assets/_Game/Resources/EnemySkins/enemy_skin_bindings.json. Preserva as entradas curadas
    /// existentes e apenas ACRESCENTA um binding-identidade (enemyId -> proprio slug) para todo
    /// EnemyDataSO cujo PNG existe em Resources/EnemySprites e que ainda nao tem binding.
    ///
    /// Sem [MenuItem] (rule editor-generation-orchestration): roda como RunStep de
    /// "Inicializar Projeto". Para trocar/variar arte manualmente, edite o JSON — este passo
    /// nunca sobrescreve slugs ja definidos.
    /// </summary>
    public static class GenerateEnemySkinBindings
    {
        private const string JsonPath = "Assets/_Game/Resources/EnemySkins/enemy_skin_bindings.json";
        private const string SpritesDir = "Assets/_Game/Resources/EnemySprites";

        [System.Serializable]
        private sealed class Entry { public string id; public string[] slugs; }

        [System.Serializable]
        private sealed class File_ { public int schemaVersion = 1; public Entry[] bindings; }

        public static void Generate()
        {
            var dir = Path.GetDirectoryName(JsonPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            // 1) carrega bindings existentes (curados) para um dict preservando os slugs.
            var map = new Dictionary<string, string[]>(256);
            if (System.IO.File.Exists(JsonPath))
            {
                try
                {
                    var parsed = JsonUtility.FromJson<File_>(System.IO.File.ReadAllText(JsonPath));
                    if (parsed?.bindings != null)
                        foreach (var e in parsed.bindings)
                            if (e != null && !string.IsNullOrWhiteSpace(e.id) && e.slugs != null && e.slugs.Length > 0)
                                map[e.id] = e.slugs;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[GenerateEnemySkinBindings] JSON existente ilegivel, sera recriado: {ex.Message}");
                }
            }
            int existing = map.Count;

            // 2) slugs disponiveis (basenames dos PNG em EnemySprites).
            var available = new HashSet<string>(
                Directory.Exists(SpritesDir)
                    ? Directory.GetFiles(SpritesDir, "*.png").Select(Path.GetFileNameWithoutExtension)
                    : System.Array.Empty<string>());

            // 3) para cada EnemyDataSO sem binding, adiciona identidade se o PNG existir.
            int added = 0, skipped = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:EnemyDataSO"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var so = AssetDatabase.LoadAssetAtPath<CindarsHope.Combat.EnemyDataSO>(path);
                if (so == null || string.IsNullOrWhiteSpace(so.enemyId)) continue;
                if (map.ContainsKey(so.enemyId)) continue;

                var slug = so.enemyId.StartsWith("enemy_") ? so.enemyId.Substring("enemy_".Length) : so.enemyId;
                if (available.Contains(slug)) { map[so.enemyId] = new[] { slug }; added++; }
                else skipped++;
            }

            // 4) serializa ordenado por id (estavel) e grava.
            var file = new File_
            {
                schemaVersion = 1,
                bindings = map.OrderBy(kv => kv.Key)
                              .Select(kv => new Entry { id = kv.Key, slugs = kv.Value })
                              .ToArray()
            };
            System.IO.File.WriteAllText(JsonPath, JsonUtility.ToJson(file, true));
            AssetDatabase.ImportAsset(JsonPath);
            EnemySkinCatalog.Reload();

            Debug.Log($"[GenerateEnemySkinBindings] bindings: {existing} preservados + {added} novos (identidade) " +
                      $"= {map.Count} total. {skipped} EnemyDataSO sem PNG correspondente (sem binding). JSON: {JsonPath}");
        }
    }
}
