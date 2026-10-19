using CindarsHope.Combat.StatusEffect;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.EditorTools.Cave
{
    /// <summary>
    /// fable_78 (slice 6) — gerador OPCIONAL do <see cref="StatusEffectSO"/> "Ferido" (defesa reduzida
    /// transitória aplicada a um monstro que apanha de um rival no conflito inter-monstro).
    ///
    /// IMPORTANTE — leia antes de rodar:
    /// O runtime atual (slice 4) NÃO resolve "Ferido" via StatusEffectSO/StatusEffectDatabase. "Ferido" é
    /// uma JANELA RUNTIME transitória em <c>EnemyHealth</c> (<c>_woundedUntil</c> + a leitura de
    /// <c>WoundedDefenseMultiplier</c> do <see cref="CindarsHope.Cave.Data.CaveEcosystemBalanceSO"/>),
    /// consumida pelo <c>DamageCalculator</c> — o mesmo idioma de <c>_stunUntil</c>/janela de
    /// vulnerabilidade. Esse caminho NÃO precisa de um asset de status nem de registro no database, e é o
    /// caminho ativo da spec (sem sistema paralelo, sem dependência de Unity em runtime).
    ///
    /// Portanto, este gerador é um NO-OP para o pipeline atual: ele só existe caso, no futuro, se decida
    /// promover "Ferido" a um status CANÔNICO resolvido via SO (ex.: para HUD/ícone/telemetria do fable_01).
    /// Ele cria/atualiza o asset com defaults coerentes (defesa 0.85 ~ <c>StatusEffectType.Weakness</c>,
    /// duração ~6s) mas NÃO o registra automaticamente no StatusEffectDatabase, para não alterar o
    /// comportamento canônico de status do fable_01 sem decisão explícita. Idempotente por Id.
    /// </summary>
    public static class GenerateCaveWoundedStatusEffect
    {
        private const string Tag = "fable_78/Wounded";
        private const string StatusFolder = "Assets/_Game/Data/Combat/StatusEffects";
        private const string WoundedId = "status_wounded";
        private const string AssetPath = StatusFolder + "/" + WoundedId + ".asset";

        [MenuItem("CindarsHope/Cave/Ecosystem/Generate Wounded Status Effect (optional)")]
        public static void Generate()
        {
            Debug.Log($"[{Tag}] AVISO: runtime atual usa janela transitoria de 'Ferido' em EnemyHealth " +
                      "(NAO depende deste SO). Este gerador e OPCIONAL — so cria o asset canonico para uso futuro " +
                      "e NAO o registra no StatusEffectDatabase.");

            EnsureFolder();

            var asset = AssetDatabase.LoadAssetAtPath<StatusEffectSO>(AssetPath);
            var created = false;
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<StatusEffectSO>();
                AssetDatabase.CreateAsset(asset, AssetPath);
                created = true;
            }

            asset.Id = WoundedId;
            asset.DisplayName = "Ferido";
            asset.Description = "Defesa reduzida por uma janela curta apos apanhar de uma criatura rival (conflito inter-monstro).";
            // Weakness = vulnerabilidade defensiva no vocabulario do fable_01; o multiplicador real de defesa
            // vive em CaveEcosystemBalanceSO.WoundedDefenseMultiplier (a janela runtime e quem o aplica hoje).
            asset.Type = StatusEffectType.Weakness;
            asset.DurationTurns = 3; // ~6s a 2s/turno; referencia, nao consumido pelo runtime atual.
            asset.DamagePerTurn = 0;
            asset.MoveSpeedMultiplier = 1f;
            asset.BehaviorOverrideSeconds = 0f;
            asset.DurabilityWearMultiplier = 1f;
            asset.VisualColor = new Color(0.85f, 0.35f, 0.35f);
            EditorUtility.SetDirty(asset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = asset;
            Debug.Log($"[{Tag}] {(created ? "Criado" : "Atualizado")} {AssetPath} (Id={WoundedId}). " +
                      "Registro no StatusEffectDatabase NAO feito por design (decisao futura).");
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Game/Data"))
            {
                AssetDatabase.CreateFolder("Assets/_Game", "Data");
            }

            if (!AssetDatabase.IsValidFolder("Assets/_Game/Data/Combat"))
            {
                AssetDatabase.CreateFolder("Assets/_Game/Data", "Combat");
            }

            if (!AssetDatabase.IsValidFolder(StatusFolder))
            {
                AssetDatabase.CreateFolder("Assets/_Game/Data/Combat", "StatusEffects");
                Debug.Log($"[{Tag}] Pasta criada: {StatusFolder}");
            }
        }
    }
}
