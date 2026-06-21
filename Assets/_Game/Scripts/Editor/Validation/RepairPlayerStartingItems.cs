#if UNITY_EDITOR

using System.Collections.Generic;
using CindarsHope.Inventory.Data;
using CindarsHope.Player.Data;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.EditorTools.Repair
{
    /// <summary>
    /// Reparos idempotentes de PlayerDataSO.StartingItems.
    ///
    /// 1) <see cref="Repair"/>: remove entradas mortas (Item == null). Contexto: ao deletar assets
    ///    de item legados/duplicados, as refs serializadas ficam pendentes (dangling GUID) e o Unity
    ///    as resolve para null, disparando STARTING_ITEM_NULL (ERROR) no CombatDatabaseValidator.
    ///
    /// 2) <see cref="EnsureStartingItem"/> / <see cref="EnsureGoddessTears"/>: GARANTE que um item
    ///    esteja presente no inventario inicial com pelo menos N unidades. So ACRESCENTA (nunca
    ///    remove/substitui entradas existentes — outra feature adiciona flechas ao starter depois).
    ///    Se a entrada ja existe com Amount &gt;= desejado, e no-op.
    ///
    /// Ambos usam SerializedObject/SetDirty — NAO editam YAML manual (rule unity-assets).
    /// </summary>
    public static class RepairPlayerStartingItems
    {
        private const string PlayerDataPath = "Assets/_Game/Data/Config/PlayerData.asset";

        // Lagrima da Deusa: 2x no inventario inicial para testar o revive na tela de morte.
        private const string GoddessTearItemId = "item_goddess_tear";
        private const int GoddessTearStartingAmount = 2;

        // Flechas basicas: 30x no inventario inicial para testar o arco. O id e o mesmo dos hotbar
        // defaults do SaveManager (item_ammo_arrow_basic). Equipavel na mao (UseKind=EquipAmmo);
        // o arco na outra mao consome 1 por disparo (BowArrowAttackService).
        private const string BasicArrowItemId = "item_ammo_arrow_basic";
        private const int BasicArrowStartingAmount = 30;

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

        /// <summary>
        /// Conveniencia: garante 2x Lagrima da Deusa no inventario inicial. Chamado pelo menu
        /// Inicializar/Reparar APOS o gerador de itens ter criado o item_goddess_tear.asset.
        /// </summary>
        public static void EnsureGoddessTears()
        {
            EnsureStartingItem(GoddessTearItemId, GoddessTearStartingAmount);
        }

        /// <summary>
        /// Conveniencia: garante 30x Flechas Basicas no inventario inicial para testar o arco.
        /// Chamado pelo menu Inicializar/Reparar APOS o gerador de itens ter criado/atualizado o
        /// item_ammo_arrow_basic.asset (equipavel, UseKind=EquipAmmo). So ACRESCENTA; idempotente.
        /// </summary>
        public static void EnsureStartingArrows()
        {
            EnsureStartingItem(BasicArrowItemId, BasicArrowStartingAmount);
        }

        /// <summary>
        /// Garante que <paramref name="itemId"/> esteja presente em StartingItems com pelo menos
        /// <paramref name="minAmount"/> unidades. So ACRESCENTA (append) ou ajusta o Amount PARA CIMA
        /// de uma entrada existente do MESMO item — nunca toca em outras entradas. Idempotente.
        /// </summary>
        public static void EnsureStartingItem(string itemId, int minAmount)
        {
            if (string.IsNullOrWhiteSpace(itemId) || minAmount <= 0)
            {
                Debug.LogWarning($"[RepairPlayerStartingItems] EnsureStartingItem chamado com argumentos invalidos (id='{itemId}', amount={minAmount}).");
                return;
            }

            var playerData = AssetDatabase.LoadAssetAtPath<PlayerDataSO>(PlayerDataPath);
            if (playerData == null)
            {
                Debug.LogWarning($"[RepairPlayerStartingItems] PlayerData nao encontrado em {PlayerDataPath}; nao foi possivel garantir '{itemId}'.");
                return;
            }

            var itemAsset = FindItemDataSoById(itemId);
            if (itemAsset == null)
            {
                Debug.LogWarning(
                    $"[RepairPlayerStartingItems] ItemDataSO '{itemId}' nao encontrado. " +
                    "Rode o gerador de catalogo de itens (Inicializar) ANTES deste passo. Pulo idempotente.");
                return;
            }

            var current = playerData.StartingItems ?? System.Array.Empty<StartingItem>();
            var list = new List<StartingItem>(current);

            // Procura uma entrada existente para o MESMO item (por ref ou por Id).
            int existingIndex = -1;
            for (int i = 0; i < list.Count; i++)
            {
                var entryItem = list[i].Item;
                if (entryItem == null)
                {
                    continue;
                }
                if (entryItem == itemAsset || entryItem.Id == itemId)
                {
                    existingIndex = i;
                    break;
                }
            }

            if (existingIndex >= 0)
            {
                if (list[existingIndex].Amount >= minAmount)
                {
                    Debug.Log($"[RepairPlayerStartingItems] '{itemId}' ja presente com Amount {list[existingIndex].Amount} (>= {minAmount}); no-op.");
                    return;
                }

                var bumped = list[existingIndex];
                bumped.Item = itemAsset; // re-resolve a ref caso estivesse so por Id
                bumped.Amount = minAmount;
                list[existingIndex] = bumped;
                playerData.StartingItems = list.ToArray();
                EditorUtility.SetDirty(playerData);
                AssetDatabase.SaveAssets();
                Debug.Log($"[RepairPlayerStartingItems] '{itemId}' ajustado para Amount {minAmount} (era menor).");
                return;
            }

            // ACRESCENTA nova entrada no fim (sem tocar nas existentes).
            list.Add(new StartingItem { Item = itemAsset, Amount = minAmount });
            playerData.StartingItems = list.ToArray();
            EditorUtility.SetDirty(playerData);
            AssetDatabase.SaveAssets();
            Debug.Log($"[RepairPlayerStartingItems] '{itemId}' x{minAmount} ACRESCENTADO ao inventario inicial (total de entradas: {list.Count}).");
        }

        private static ItemDataSO FindItemDataSoById(string itemId)
        {
            foreach (var guid in AssetDatabase.FindAssets("t:ItemDataSO"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var item = AssetDatabase.LoadAssetAtPath<ItemDataSO>(path);
                if (item != null && item.Id == itemId)
                {
                    return item;
                }
            }
            return null;
        }
    }
}

#endif
