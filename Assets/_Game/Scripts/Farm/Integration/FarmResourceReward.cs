using System;
using UnityEngine;

namespace CindarsHope.Farm.Integration
{
    [Serializable]
    public sealed class FarmResourceReward
    {
        [SerializeField] private string _itemId = "item_wood";
        [SerializeField] private int _amount = 1;

        public string ItemId => _itemId;
        public int Amount => _amount;
    }
}
