using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.EnemySkins
{
    /// <summary>
    /// Validador SOMENTE-LEITURA dos bindings de skin de inimigo. Checa:
    ///  1) todo slug referenciado no JSON tem PNG em Resources/EnemySprites;
    ///  2) todo EnemyId usado por um EnemySpawnProfileSO (o que de fato spawna) tem binding
    ///     OU pelo menos um PNG-identidade como fallback.
    /// Nao gera, nao repara, nao muta. Roda como RunStep de "Validar Projeto".
    /// </summary>
    public static class ValidateEnemySkinBindings
    {
        private const string JsonPath = "Assets/_Game/Resources/EnemySkins/enemy_skin_bindings.json";
        private const string SpritesDir = "Assets/_Game/Resources/EnemySprites";

        [System.Serializable]
        private sealed class Entry { public string id = null; public string[] slugs = null; }

        [System.Serializable]
        private sealed class File_ { public int schemaVersion = 0; public Entry[] bindings = null; }

        public static void Validate()
        {
            if (!System.IO.File.Exists(JsonPath))
            {
                Debug.LogError($"[ValidateEnemySkinBindings] FAIL — JSON nao encontrado: {JsonPath}");
                return;
            }

            File_ parsed;
            try { parsed = JsonUtility.FromJson<File_>(System.IO.File.ReadAllText(JsonPath)); }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ValidateEnemySkinBindings] FAIL — JSON ilegivel: {ex.Message}");
                return;
            }

            var map = new Dictionary<string, string[]>(256);
            if (parsed?.bindings != null)
                foreach (var e in parsed.bindings)
                    if (e != null && !string.IsNullOrWhiteSpace(e.id) && e.slugs != null && e.slugs.Length > 0)
                        map[e.id] = e.slugs;

            var available = new HashSet<string>(
                Directory.Exists(SpritesDir)
                    ? Directory.GetFiles(SpritesDir, "*.png").Select(Path.GetFileNameWithoutExtension)
                    : System.Array.Empty<string>());

            // (1) slugs referenciados que nao existem em disco.
            var missingSlugs = new List<string>();
            foreach (var kv in map)
                foreach (var slug in kv.Value)
                    if (!available.Contains(slug))
                        missingSlugs.Add($"{kv.Key} -> {slug}");

            // (2) profiles de spawn sem cobertura (nem binding, nem PNG-identidade).
            var uncovered = new List<string>();
            foreach (var guid in AssetDatabase.FindAssets("t:EnemySpawnProfileSO"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var so = AssetDatabase.LoadAssetAtPath<CindarsHope.Enemy.EnemySpawnProfileSO>(path);
                if (so == null || string.IsNullOrWhiteSpace(so.EnemyId)) continue;

                bool boundOk = map.TryGetValue(so.EnemyId, out var slugs) && slugs.Any(available.Contains);
                var identity = so.EnemyId.StartsWith("enemy_") ? so.EnemyId.Substring("enemy_".Length) : so.EnemyId;
                bool identityOk = available.Contains(identity);
                if (!boundOk && !identityOk) uncovered.Add(so.EnemyId);
            }

            bool ok = missingSlugs.Count == 0 && uncovered.Count == 0;
            Debug.Log($"[ValidateEnemySkinBindings] bindings={map.Count}, slugs disponiveis={available.Count}, " +
                      $"slugs quebrados={missingSlugs.Count}, profiles sem cobertura={uncovered.Count}. " +
                      (ok ? "PASS" : "FAIL"));
            if (missingSlugs.Count > 0)
                Debug.LogError("[ValidateEnemySkinBindings] slugs sem PNG: " + string.Join(", ", missingSlugs.Take(40)));
            if (uncovered.Count > 0)
                Debug.LogWarning("[ValidateEnemySkinBindings] profiles sem binding nem PNG-identidade: " +
                                 string.Join(", ", uncovered.Take(40)));
        }
    }
}
