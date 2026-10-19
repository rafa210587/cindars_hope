using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Enemy
{
    /// <summary>
    /// Catalogo data-driven de skins de inimigo. Mapeia enemyId -> lista de slugs de sprite
    /// (arquivos em Assets/_Game/Resources/EnemySprites/&lt;slug&gt;.png), permitindo trocar/variar a
    /// arte de qualquer inimigo por REFERENCIA CRUZADA, sem recompilar: basta editar o JSON
    /// Assets/_Game/Resources/EnemySkins/enemy_skin_bindings.json.
    ///
    /// O primeiro slug e o "primario"; os demais sao variancia. A escolha por instancia e
    /// DETERMINISTICA (hash estavel do seed), entao dentro do mesmo CaveRunSeed um inimigo
    /// revisitado mantem a mesma skin (respeita a rule cave-stable-run).
    /// </summary>
    public static class EnemySkinCatalog
    {
        public const string ResourcePath = "EnemySkins/enemy_skin_bindings";

        private static Dictionary<string, string[]> _map;
        private static bool _loadAttempted;

        [System.Serializable]
        private sealed class BindingEntry
        {
            public string id = null;
            public string[] slugs = null;
        }

        [System.Serializable]
        private sealed class BindingFile
        {
            public int schemaVersion = 0;
            public BindingEntry[] bindings = null;
        }

        /// <summary>Numero de ids com binding (0 se o JSON nao carregou). Diagnostico.</summary>
        public static int Count
        {
            get { EnsureLoaded(); return _map.Count; }
        }

        /// <summary>Forca recarga (usado por editor tooling apos regenerar o JSON).</summary>
        public static void Reload()
        {
            _loadAttempted = false;
            _map = null;
            EnsureLoaded();
        }

        private static void EnsureLoaded()
        {
            if (_loadAttempted && _map != null) return;
            _loadAttempted = true;
            _map = new Dictionary<string, string[]>(256);

            var textAsset = Resources.Load<TextAsset>(ResourcePath);
            if (textAsset == null)
            {
                Debug.LogWarning(
                    $"[EnemySkinCatalog] binding JSON nao encontrado em Resources/{ResourcePath}. " +
                    "Inimigos caem no fallback de Icon/autoload por id.");
                return;
            }

            BindingFile parsed = null;
            try { parsed = JsonUtility.FromJson<BindingFile>(textAsset.text); }
            catch (System.Exception e)
            {
                Debug.LogError($"[EnemySkinCatalog] falha ao parsear {ResourcePath}: {e.Message}");
                return;
            }

            if (parsed?.bindings == null) return;
            foreach (var entry in parsed.bindings)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.id) || entry.slugs == null || entry.slugs.Length == 0)
                    continue;
                _map[entry.id] = entry.slugs;
            }
        }

        /// <summary>Retorna a lista de skins de um enemyId, ou false se nao houver binding.</summary>
        public static bool TryGetSkins(string enemyId, out string[] slugs)
        {
            EnsureLoaded();
            if (!string.IsNullOrEmpty(enemyId) && _map.TryGetValue(enemyId, out slugs) && slugs.Length > 0)
                return true;
            slugs = null;
            return false;
        }

        /// <summary>
        /// Resolve deterministicamente qual slug usar para uma instancia. `stableSeed` deve ser
        /// estavel por instancia dentro do run (ex.: hash de enemyId + instanceId + caveLevel).
        /// Retorna null se nao houver binding para o enemyId.
        /// </summary>
        public static string ResolveSlug(string enemyId, int stableSeed)
        {
            if (!TryGetSkins(enemyId, out var slugs)) return null;
            if (slugs.Length == 1) return slugs[0];
            int idx = (int)((uint)stableSeed % (uint)slugs.Length);
            return slugs[idx];
        }

        /// <summary>
        /// Hash FNV-1a de 32 bits — deterministico entre execucoes/plataformas (ao contrario de
        /// string.GetHashCode), para a skin nao mudar em reload dentro do mesmo run.
        /// </summary>
        public static int StableHash(string a, string b, int c)
        {
            unchecked
            {
                const uint offset = 2166136261;
                const uint prime = 16777619;
                uint h = offset;
                h = FnvString(h, prime, a);
                h = (h ^ 0x2Cu) * prime; // separador
                h = FnvString(h, prime, b);
                h = (h ^ (uint)c) * prime;
                return (int)h;
            }
        }

        private static uint FnvString(uint h, uint prime, string s)
        {
            if (s == null) return h;
            unchecked
            {
                for (int i = 0; i < s.Length; i++)
                    h = (h ^ s[i]) * prime;
                return h;
            }
        }
    }
}
