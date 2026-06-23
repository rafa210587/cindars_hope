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
    /// DEFERRED_UNITY: o asset gerado deve ser ligado no campo <c>_ecosystemBalance</c> do
    /// CaveRuntimeMaterializer na CaveScene (sem balance ligado, o roll de conflito é no-op seguro).
    /// </summary>
    public static class GenerateCaveEcosystemBalance
    {
        private const string Tag = "fable_78/EcosystemBalance";
        private const string DataRoot = "Assets/_Game/Data";
        private const string CaveDir = DataRoot + "/Cave";
        private const string AssetPath = CaveDir + "/CaveEcosystemBalance.asset";

        [MenuItem("CindarsHope/Cave/Ecosystem/Generate Ecosystem Balance")]
        public static void Generate()
        {
            EnsureCaveFolder();

            var existing = AssetDatabase.LoadAssetAtPath<CaveEcosystemBalanceSO>(AssetPath);
            if (existing != null)
            {
                Debug.Log($"[{Tag}] Asset ja existe em {AssetPath} — preservado (idempotente).");
                Selection.activeObject = existing;
                return;
            }

            var balance = ScriptableObject.CreateInstance<CaveEcosystemBalanceSO>();
            AssetDatabase.CreateAsset(balance, AssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = balance;
            Debug.Log($"[{Tag}] Criado {AssetPath} com defaults da secao 16.1. " +
                      "DEFERRED_UNITY: ligar em CaveRuntimeMaterializer._ecosystemBalance na CaveScene.");
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
