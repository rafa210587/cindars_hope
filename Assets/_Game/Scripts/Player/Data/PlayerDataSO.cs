using System.Collections.Generic;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using UnityEngine;

namespace CindarsHope.Player.Data
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "CindarsHope/Data/Player")]
    public class PlayerDataSO : ScriptableObject, IStartingItemsSource
    {
        public float MoveSpeed = 5f;
        public int BaseHP = 100;
        public int StartingGold = 50;
        public int MaxHunger = 100;
        public int StartingHunger = 100;
        public int StepsPerHungerTick = 10;
        public int HungerLossPerTick = 1;
        public int HungerLossPerDay = 10;
        public StartingItem[] StartingItems;

        // arch: quebra do ciclo mutuo Inventory|Player (spec_arch_inventory_player_pair_reduction) —
        // InventoryManager consome apenas este contrato neutro, nunca CindarsHope.Player.Data direto.
        IReadOnlyList<StartingItem> IStartingItemsSource.StartingItems => StartingItems;

        private void OnValidate()
        {
            MoveSpeed = Mathf.Max(0.1f, MoveSpeed);
            BaseHP = Mathf.Max(1, BaseHP);
            StartingGold = Mathf.Max(0, StartingGold);
            MaxHunger = Mathf.Max(1, MaxHunger);
            StartingHunger = Mathf.Clamp(StartingHunger, 0, MaxHunger);
            StepsPerHungerTick = Mathf.Max(1, StepsPerHungerTick);
            HungerLossPerTick = Mathf.Max(1, HungerLossPerTick);
            HungerLossPerDay = Mathf.Max(0, HungerLossPerDay);

            if (StartingItems == null)
            {
                return;
            }

            for (var index = 0; index < StartingItems.Length; index++)
            {
                StartingItems[index].Amount = Mathf.Max(1, StartingItems[index].Amount);
            }
        }
    }
}

namespace CindarsHope.Inventory.Data
{
    // arch: movido de CindarsHope.Player.Data para quebrar o par mutuo Inventory|Player —
    // struct de valor, layout de campos inalterado, sem impacto no YAML serializado do PlayerDataSO.
    [System.Serializable]
    public struct StartingItem
    {
        public ItemDataSO Item;
        public int Amount;
    }
}
