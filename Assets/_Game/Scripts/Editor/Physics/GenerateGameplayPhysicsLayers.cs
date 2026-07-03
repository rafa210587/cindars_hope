using CindarsHope.Core.Physics;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Physics
{
    /// <summary>
    /// spec_codex_13: cria os 7 physics layers de gameplay via SerializedObject sobre o
    /// TagManager, de forma idempotente (nunca duplica, nunca falha se ja existirem).
    /// Sem [MenuItem] proprio — registrado como RunStep em CindarsHopeMenu.InicializarProjeto
    /// (rule editor-generation-orchestration). Nao edita ProjectSettings/TagManager.asset via
    /// Write/Edit direto; usa a API de editor padrao do Unity para adicionar layers.
    /// Nomes canonicos vivem em CindarsHope.Core.Physics.GameplayLayerNames (fonte unica,
    /// acessivel tambem de codigo runtime).
    /// </summary>
    public static class GenerateGameplayPhysicsLayers
    {
        // Reexpostas aqui por conveniencia de call site do editor (mesma fonte de verdade).
        public const string Player = GameplayLayerNames.Player;
        public const string Enemy = GameplayLayerNames.Enemy;
        public const string Npc = GameplayLayerNames.Npc;
        public const string WorldSolid = GameplayLayerNames.WorldSolid;
        public const string Interactable = GameplayLayerNames.Interactable;
        public const string Projectile = GameplayLayerNames.Projectile;
        public const string Hazard = GameplayLayerNames.Hazard;

        // YAGNI: 7 layers minimos (nao os 9 do wishlist original).
        public static readonly string[] AllLayers =
        {
            Player, Enemy, Npc, WorldSolid, Interactable, Projectile, Hazard
        };

        /// <summary>
        /// Cria os 7 layers se ausentes, no primeiro slot de usuario livre (8-31), sem
        /// reordenar ou sobrescrever layers built-in/existentes. Idempotente: rodar 2x
        /// nao duplica nem falha. Retorna quantos layers foram criados nesta chamada.
        /// </summary>
        public static int EnsureLayers()
        {
            var tagManagerAssets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (tagManagerAssets == null || tagManagerAssets.Length == 0)
            {
                Debug.LogError("GenerateGameplayPhysicsLayers: wiring-error. Scene=N/A, Object=N/A, " +
                    "Component=TagManager, Field=N/A, Reason=TagManager.asset nao encontrado via AssetDatabase.");
                return 0;
            }

            var tagManager = new SerializedObject(tagManagerAssets[0]);
            var layersProp = tagManager.FindProperty("layers");
            if (layersProp == null || !layersProp.isArray)
            {
                Debug.LogError("GenerateGameplayPhysicsLayers: wiring-error. Scene=N/A, Object=N/A, " +
                    "Component=TagManager, Field=layers, Reason=propriedade 'layers' ausente/nao-array na versao atual do Unity.");
                return 0;
            }

            int created = 0;
            foreach (var layerName in AllLayers)
            {
                if (LayerAlreadyExists(layersProp, layerName))
                {
                    continue;
                }

                if (!TryAssignToFirstFreeUserSlot(layersProp, layerName))
                {
                    Debug.LogWarning($"GenerateGameplayPhysicsLayers: wiring-error. Scene=N/A, Object=N/A, " +
                        $"Component=TagManager, Field=layers, Reason=nenhum slot de usuario livre (8-31) para '{layerName}'.");
                    continue;
                }

                created++;
            }

            if (created > 0)
            {
                tagManager.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
            }

            return created;
        }

        private static bool LayerAlreadyExists(SerializedProperty layersProp, string layerName)
        {
            for (int i = 0; i < layersProp.arraySize; i++)
            {
                var slot = layersProp.GetArrayElementAtIndex(i);
                if (slot.stringValue == layerName)
                {
                    return true;
                }
            }
            return false;
        }

        // Layers 0-7 sao built-in/reservados pelo Unity; slots de usuario vao de 8 a 31.
        private static bool TryAssignToFirstFreeUserSlot(SerializedProperty layersProp, string layerName)
        {
            for (int i = 8; i < layersProp.arraySize; i++)
            {
                var slot = layersProp.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(slot.stringValue))
                {
                    slot.stringValue = layerName;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Atribui o layer nomeado a um GameObject se o layer existir; caso contrario,
        /// nao altera o GameObject (mantem Default) e loga wiring-error one-shot por nome
        /// (via CindarsHope.Core.Physics.GameplayLayerNames — guard compartilhado com runtime).
        /// </summary>
        public static void TryAssignLayer(GameObject go, string layerName)
        {
            if (go == null) return;
            int layer = GameplayLayerNames.GetLayerIndexSafe(layerName);
            if (layer < 0) return;
            go.layer = layer;
        }
    }
}
