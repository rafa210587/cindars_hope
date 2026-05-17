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
        public StartingItem[] StartingItems;

        private void OnValidate()
        {
            MoveSpeed = Mathf.Max(0.1f, MoveSpeed);
            BaseHP = Mathf.Max(1, BaseHP);
            StartingGold = Mathf.Max(0, StartingGold);

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
