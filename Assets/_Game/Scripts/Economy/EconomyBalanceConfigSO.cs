using UnityEngine;

namespace CindarsHope.Economy
{
    [CreateAssetMenu(fileName = "EconomyBalanceConfig",
                     menuName = "CindarsHope/Economy/Economy Balance Config")]
    public sealed class EconomyBalanceConfigSO : ScriptableObject
    {
        [Header("Gold/hora por fonte de renda")]
        [SerializeField] public float FarmBasicGoldPerHour = 100f;
        [SerializeField] public float FarmPremiumGoldPerHour = 220f;
        [SerializeField] public float FishingFarmGoldPerHour = 80f;
        [SerializeField] public float FishingCaveGoldPerHour = 270f;
        [SerializeField] public float CaveShallowGoldPerHour = 200f;
        [SerializeField] public float CaveDeepGoldPerHour = 650f;

        [Header("Teto de balance")]
        [SerializeField] public float MaxSafeGoldPerHour = 800f;

        [Header("Multiplicadores de canal de venda")]
        [SerializeField] public float ShippingBuybackMultiplier = 1.0f;
        [SerializeField] public float NpcSellMultiplier = 0.9f;
        [SerializeField] public float NpcBuyMultiplier = 1.3f;

        [Header("Bounds do validator de ShopDataSO")]
        [SerializeField] public float ShopMaxPriceMultiplier = 3.0f;
        [SerializeField] public float ShopMinPriceMultiplier = 0.1f;
    }
}
