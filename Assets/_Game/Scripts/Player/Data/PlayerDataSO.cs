using CindarsHope.Inventory.Data;
using UnityEngine;

namespace CindarsHope.Player.Data
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "CindarsHope/Data/Player")]
    public class PlayerDataSO : ScriptableObject
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

    [System.Serializable]
    public struct StartingItem
    {
        public ItemDataSO Item;
        public int Amount;
    }
}
