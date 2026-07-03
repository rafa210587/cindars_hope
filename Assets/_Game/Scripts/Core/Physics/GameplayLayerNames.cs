using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Core.Physics
{
    /// <summary>
    /// spec_codex_13: nomes canonicos dos 7 physics layers de gameplay, acessiveis a partir
    /// de codigo runtime (Assets/_Game/Scripts/**). A CRIACAO dos layers e responsabilidade
    /// exclusiva de CindarsHope.Editor.Physics.GenerateGameplayPhysicsLayers (registrado como
    /// RunStep em CindarsHopeMenu.InicializarProjeto — rule editor-generation-orchestration).
    /// Ate o humano rodar esse passo no Unity Editor, os layers nao existem em runtime;
    /// GetMaskSafe resolve por nome com fallback seguro (mask 0 = nenhum filtro) e loga
    /// wiring-error one-shot por nome (categoria config-asset, nunca crash).
    /// </summary>
    public static class GameplayLayerNames
    {
        public const string Player = "Player";
        public const string Enemy = "Enemy";
        public const string Npc = "NPC";
        public const string WorldSolid = "WorldSolid";
        public const string Interactable = "Interactable";
        public const string Projectile = "Projectile";
        public const string Hazard = "Hazard";

        private static readonly HashSet<string> _warned = new HashSet<string>();

        /// <summary>
        /// Resolve a bitmask de um unico layer por nome. Retorna 0 (nenhum filtro/match) se o
        /// layer ainda nao foi materializado, sem lancar excecao e sem quebrar o caller.
        /// </summary>
        public static LayerMask GetMaskSafe(string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer < 0)
            {
                if (_warned.Add(layerName))
                {
                    Debug.LogWarning($"GameplayLayerNames: wiring-error. Scene=N/A, Object=N/A, " +
                        $"Component=LayerMask, Field={layerName}, Reason=layer ausente — rode " +
                        "CindarsHope/Inicializar Projeto no Unity Editor. Categoria=config-asset.");
                }
                return default;
            }
            return 1 << layer;
        }

        /// <summary>
        /// Resolve o indice numerico do layer (para GameObject.layer), ou -1 se ausente.
        /// </summary>
        public static int GetLayerIndexSafe(string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer < 0 && _warned.Add(layerName))
            {
                Debug.LogWarning($"GameplayLayerNames: wiring-error. Scene=N/A, Object=N/A, " +
                    $"Component=LayerMask, Field={layerName}, Reason=layer ausente — rode " +
                    "CindarsHope/Inicializar Projeto no Unity Editor. Categoria=config-asset.");
            }
            return layer;
        }

        /// <summary>
        /// Atribui o layer nomeado a um GameObject instanciado em runtime (ex.: inimigo/
        /// hazard/projetil criado via AddComponent, sem gerador de editor). Nao-op seguro
        /// se o layer ainda nao existir (mantem Default) — mesmo padrao do editor.
        /// </summary>
        public static void TryAssignRuntimeLayer(GameObject go, string layerName)
        {
            if (go == null) return;
            int layer = GetLayerIndexSafe(layerName);
            if (layer < 0) return;
            go.layer = layer;
        }
    }
}
