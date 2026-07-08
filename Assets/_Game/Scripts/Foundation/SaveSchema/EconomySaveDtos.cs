using System;
using System.Collections.Generic;

namespace CindarsHope.Foundation
{
    // fable_22: entrada aditiva de infusão (itemInstanceId → element/tier). Tipos simples, sem
    // refs Unity, compatível com JsonUtility. Ausência da entrada = arma sem têmpera.
    // Movido de CindarsHope.Save para CindarsHope.Foundation (arch: quebra do ciclo Economy|Save —
    // JsonUtility serializa por nome de campo, não por namespace, então o JSON no disco é idêntico).
    [Serializable]
    public class WeaponInfusionSaveData
    {
        public string ItemInstanceId;
        public string InfusionElement; // estável: fire/ice/toxic/lightning/arcane/void
        public int InfusionTier;        // 0 = sem; 1 ou 2
    }

    [Serializable]
    public class ShopStockSaveData
    {
        public string ShopId;
        public List<ShopItemStockEntry> Items = new List<ShopItemStockEntry>();
        public int LastRestockDay;
    }

    [Serializable]
    public class ShopItemStockEntry
    {
        public string ItemId;
        public int CurrentStock;
    }
}
