using CindarsHope.Cave.Data;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.EditorTools.Cave
{
    /// <summary>
    /// fable_78 (slice 6) — cria <c>Assets/_Game/Data/Cave/CaveEcosystemBalance.asset</c> com os
    /// defaults da seção 16.1 da spec. Os valores tunáveis já vivem nos <c>[SerializeField]</c> de
    /// <see cref="CaveEcosystemBalanceSO"/> (rule no-magic-balance-values), então criar o asset basta —
    /// nenhum número é escrito aqui. Idempotente: se o asset já existe, NÃO sobrescreve (preserva tuning
    /// manual). Best-effort, loga o que fez.
    ///
    /// Bugfix 2026-07-12: o fallback de runtime não deve adicionar mais <c>Resources.Load</c> no
    /// materializer. A cena gerada deve serializar a referência explícita para este asset; cenas antigas
    /// sem wiring usam fallback default em memória com warning.
    /// </summary>
    public static class GenerateCaveEcosystemBalance
    {
        private const string Tag = "fable_78/EcosystemBalance";
        private const string DataRoot = "Assets/_Game/Data";
        private const string CaveDir = DataRoot + "/Cave";
        private const string AssetPath = CaveDir + "/CaveEcosystemBalance.asset";

        public static void Generate()
        {
            EnsureCaveFolder();

            var balance = AssetDatabase.LoadAssetAtPath<CaveEcosystemBalanceSO>(AssetPath);
            if (balance == null)
            {
                balance = ScriptableObject.CreateInstance<CaveEcosystemBalanceSO>();
                AssetDatabase.CreateAsset(balance, AssetPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"[{Tag}] Criado {AssetPath} com defaults da secao 16.1.");
            }
            else
            {
                Debug.Log($"[{Tag}] Asset ja existe em {AssetPath} — preservado (idempotente).");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = balance;
            Debug.Log($"[{Tag}] Asset pronto em {AssetPath}. Rode CindarsHope/Inicializar Projeto para " +
                      "serializar esta referência no campo _ecosystemBalance da CaveScene.");
        }

        private static void EnsureCaveFolder()
        {
            if (!AssetDatabase.IsValidFolder(DataRoot))
            {
                AssetDatabase.CreateFolder("Assets/_Game", "Data");
            }

            if (!AssetDatabase.IsValidFolder(CaveDir))
            {
                AssetDatabase.CreateFolder(DataRoot, "Cave");
                Debug.Log($"[{Tag}] Pasta criada: {CaveDir}");
            }
        }
    }
}
