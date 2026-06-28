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

        // Arco de madeira: 1x no inventario inicial. SEM o arco na outra mao o disparo de flecha
        // SEMPRE bloqueia em ArrowRequiresBowInOtherHand (BowArrowAttackService.TryFire) — entao o
        // starter precisa do arco para o arco/flecha ser testavel. WeaponId resolvido em runtime.
        private const string WoodBowItemId = "item_weapon_bow_wood";
        private const int WoodBowStartingAmount = 1;

        // Ferramentas de fazenda: enxada e regador para arar e regar o solo.
        // Necessarias para FarmTillingInputController (EquipmentManager.HasTool) apos Fase 8.
        // IDs reais dos ItemDataSO (o asset da enxada e item_shop_tool_hoe_basic / Item_Shop_Hoe_Basic.asset;
        // InferToolTypeFromId mapeia por substring "hoe"/"watering_can" -> ToolType.Hoe/WateringCan).
        private const string BasicHoeItemId = "item_shop_tool_hoe_basic";
        private const int BasicHoeStartingAmount = 1;
        private const string BasicWateringCanItemId = "item_shop_tool_watering_can_basic";
        private const int BasicWateringCanStartingAmount = 1;
        private const string BasicHoeAssetPath = "Assets/_Game/Data/Items/Item_Shop_Hoe_Basic.asset";
        private const string BasicWateringCanAssetPath = "Assets/_Game/Data/Items/Item_Shop_Watering_Can_Basic.asset";

        // Capacidade do inventario (espelha InventoryManager.MaxCapacity): o starter nao pode exceder.
        private const int InventoryCapacity = 40;

        // Kit de teste focado no pedido: plantio completo (todas as sementes — enxada/regador ja
        // estao no starter), magia (wand + pergaminho de cura; o item_spell_fireball_test ja vem no
        // starter para o ataque magico), escudo. Espada, arco, flecha, comida e pocao HP ja existem no
        // starter base. So ACRESCENTA; idempotente; capacity-aware (nao estoura 30 slots; loga skips).
        // Ordem: essenciais (escudo/magia) primeiro, depois as 12 sementes (preenchem o restante).
        private static readonly (string id, int amount)[] TestStarterKit =
        {
            ("item_shield_buckler", 1),                       // escudo (block)
            ("item_weapon_wand_fire", 1),                     // arma magica (cast)
            ("item_consumable_scroll_learn_minor_heal", 1),   // aprender magia de cura
            // Todas as sementes para plantar e coletar cada cultura (colheita gera a crop).
            ("item_seed_wheat", 5),
            ("item_seed_carrot", 5),
            ("item_seed_moonbean", 5),
            ("item_seed_sunpepper", 5),
            ("item_seed_crystal_berry", 5),
            ("item_seed_starroot", 5),
            ("item_seed_alihana_tear", 5),
            ("item_seed_senya_pepper", 5),
            ("item_seed_shadowroot", 5),
            ("item_seed_thandra_wheat", 5),
            ("item_seed_vale_pumpkin", 5),
            ("item_seed_brigandini_grape", 5),
        };

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
        /// Conveniencia: garante 1x Arco de Madeira no inventario inicial. Sem o arco equipado na
        /// outra mao o disparo de flecha sempre bloqueia (ArrowRequiresBowInOtherHand), entao o
        /// arco/flecha so e testavel com o arco presente. So ACRESCENTA; idempotente.
        /// </summary>
        public static void EnsureStartingBow()
        {
            EnsureStartingItem(WoodBowItemId, WoodBowStartingAmount);
        }

        /// <summary>
        /// Conveniencia: garante 1x Enxada Basica no inventario inicial.
        /// Necessaria para FarmTillingInputController apos Fase 8 (checagem real de ferramenta).
        /// So ACRESCENTA; idempotente.
        /// </summary>
        public static void EnsureStartingHoe()
        {
            EnsureStartingItem(BasicHoeItemId, BasicHoeStartingAmount);
        }

        /// <summary>
        /// Conveniencia: garante 1x Regador Basico no inventario inicial.
        /// Necessario para FarmTillingInputController (WaterTile via EquipmentManager.HasTool).
        /// So ACRESCENTA; idempotente.
        /// </summary>
        public static void EnsureStartingWateringCan()
        {
            EnsureStartingItem(BasicWateringCanItemId, BasicWateringCanStartingAmount);
        }

        /// <summary>
        /// Garante que o ItemDataSO do Regador Basico exista em Data/Items (clonando a Enxada Basica,
        /// que e hand-authored). Idempotente. DEVE rodar ANTES do "Gerar catalogo canonico de itens"
        /// (que escaneia a pasta e registra todos os ItemDataSO no ItemDatabase) e antes de
        /// EnsureStartingWateringCan. O id contem "watering_can" -> InferToolTypeFromId = WateringCan.
        /// </summary>
        public static void EnsureBasicWateringCanAsset()
        {
            if (FindItemDataSoById(BasicWateringCanItemId) != null)
            {
                return; // ja existe (por id) — idempotente
            }

            var hoe = AssetDatabase.LoadAssetAtPath<CindarsHope.Inventory.Data.ItemDataSO>(BasicHoeAssetPath);
            if (hoe == null)
            {
                Debug.LogWarning(
                    $"[RepairPlayerStartingItems] Enxada base nao encontrada em {BasicHoeAssetPath}; " +
                    "nao foi possivel clonar o Regador Basico. Pulo idempotente.");
                return;
            }

            // Clona a enxada para herdar Category/IsEquippable/MaxStack etc.; muda so id/nome/descricao.
            var canItem = Object.Instantiate(hoe);
            var so = new SerializedObject(canItem);
            var idProp = so.FindProperty("Id");
            var nameProp = so.FindProperty("DisplayName");
            var descProp = so.FindProperty("Description");
            if (idProp != null) idProp.stringValue = BasicWateringCanItemId;
            if (nameProp != null) nameProp.stringValue = "Regador Basico";
            if (descProp != null) descProp.stringValue = "Ferramenta basica para regar plantacoes.";
            so.ApplyModifiedPropertiesWithoutUndo();
            canItem.name = "Item_Shop_Watering_Can_Basic";

            AssetDatabase.CreateAsset(canItem, BasicWateringCanAssetPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[RepairPlayerStartingItems] Criado {BasicWateringCanAssetPath} (id {BasicWateringCanItemId}).");
        }

        /// <summary>
        /// Conveniencia: acrescenta 1 representante de cada sistema (arma, magia, armadura, escudo,
        /// acessorio, comida, pocoes, reparo, pergaminhos, semente, materiais) ao inventario inicial
        /// para testar tudo num jogo novo. Idempotente; capacity-aware (nao excede 30 slots).
        /// </summary>
        public static void EnsureTestStarterKit()
        {
            var playerData = AssetDatabase.LoadAssetAtPath<PlayerDataSO>(PlayerDataPath);
            if (playerData == null)
            {
                Debug.LogWarning($"[RepairPlayerStartingItems] PlayerData nao encontrado em {PlayerDataPath}; kit de teste nao aplicado.");
                return;
            }

            var list = new List<StartingItem>(playerData.StartingItems ?? System.Array.Empty<StartingItem>());
            int added = 0, bumped = 0, skippedFull = 0, skippedMissing = 0;

            foreach (var (id, amount) in TestStarterKit)
            {
                int existingIndex = IndexOfItem(list, id);
                if (existingIndex >= 0)
                {
                    if (list[existingIndex].Amount < amount)
                    {
                        var bumpedEntry = list[existingIndex];
                        bumpedEntry.Amount = amount;
                        list[existingIndex] = bumpedEntry;
                        bumped++;
                    }
                    continue;
                }

                if (list.Count >= InventoryCapacity)
                {
                    skippedFull++;
                    Debug.LogWarning($"[RepairPlayerStartingItems] Kit: '{id}' NAO adicionado — inventario inicial cheio ({list.Count}/{InventoryCapacity}).");
                    continue;
                }

                var itemAsset = FindItemDataSoById(id);
                if (itemAsset == null)
                {
                    skippedMissing++;
                    Debug.LogWarning($"[RepairPlayerStartingItems] Kit: ItemDataSO '{id}' nao encontrado. Rode 'Gerar catalogo canonico de itens' ANTES. Pulado.");
                    continue;
                }

                list.Add(new StartingItem { Item = itemAsset, Amount = amount });
                added++;
            }

            playerData.StartingItems = list.ToArray();
            EditorUtility.SetDirty(playerData);
            AssetDatabase.SaveAssets();
            Debug.Log($"[RepairPlayerStartingItems] Kit de teste: {added} adicionado(s), {bumped} ajustado(s), " +
                      $"{skippedFull} pulado(s) por inventario cheio, {skippedMissing} ausente(s). Total {list.Count}/{InventoryCapacity}.");
        }

        private static int IndexOfItem(List<StartingItem> list, string itemId)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var entryItem = list[i].Item;
                if (entryItem != null && entryItem.Id == itemId)
                {
                    return i;
                }
            }
            return -1;
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
