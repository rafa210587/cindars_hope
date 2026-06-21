#if UNITY_EDITOR

using System.Collections.Generic;
using CindarsHope.Player.Data;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.EditorTools.Repair
{
    /// <summary>
    /// Remove entradas mortas (Item == null) de PlayerDataSO.StartingItems.
    ///
    /// Contexto: ao deletar assets de item legados/duplicados (Item_Trigo/Item_Cenoura/Semente_*
    /// via "Reparar e Reconstruir"), as referências serializadas em StartingItems ficam pendentes
    /// (dangling GUID) e o Unity as resolve para null. Isso dispara STARTING_ITEM_NULL (ERROR) no
    /// CombatDatabaseValidator. Este reparo é idempotente: só reescreve o array quando há entradas
    /// null para remover. Usa SerializedObject/SetDirty — NÃO edita YAML manual (rule unity-assets).
    /// </summary>
    public static class RepairPlayerStartingItems
    {
        private const string PlayerDataPath = "Assets/_Game/Data/Config/PlayerData.asset";

        public static void Repair()
        {
            var playerData = AssetDatabase.LoadAssetAtPath<PlayerDataSO>(PlayerDataPath);
            if (playerData == null)
            {
                Debug.LogWarning($"[RepairPlayerStartingItems] PlayerData nao encontrado em {PlayerDataPath}; nada a reparar.");
                return;
            }

            var current = playerData.StartingItems;
            if (current == null || current.Length == 0)
            {
                Debug.Log("[RepairPlayerStartingItems] StartingItems vazio/null; nada a reparar.");
                return;
            }

            var kept = new List<StartingItem>(current.Length);
            int removed = 0;
            for (int i = 0; i < current.Length; i++)
            {
                if (current[i].Item == null)
                {
                    removed++;
                    Debug.Log($"[RepairPlayerStartingItems] StartingItems[{i}] tinha Item == null (ref pendente); removido.");
                    continue;
                }

                kept.Add(current[i]);
            }

            if (removed == 0)
            {
                Debug.Log("[RepairPlayerStartingItems] Nenhuma entrada null em StartingItems; nada a reparar.");
                return;
            }

            playerData.StartingItems = kept.ToArray();
            EditorUtility.SetDirty(playerData);
            AssetDatabase.SaveAssets();
            Debug.Log($"[RepairPlayerStartingItems] {removed} entrada(s) null removida(s) de StartingItems ({kept.Count} mantida(s)).");
        }
    }
}

#endif
