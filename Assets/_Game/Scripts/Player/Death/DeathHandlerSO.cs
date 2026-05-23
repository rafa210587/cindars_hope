using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Player.Death
{
    [CreateAssetMenu(fileName = "DeathHandler", menuName = "CindarsHope/Player/DeathHandler")]
    public class DeathHandlerSO : ScriptableObject, IIdentifiedData
    {
        public string Id = "death_handler_default";
        public int XpLossPercentageOnDeath = 0;
        public int GoldLossPercentageOnDeath = 10;
        public bool AllowCorpseRecovery = true;
        public int CorpseRecoveryTimeHours = 1;
        public bool RespawnAtLastSafeLocation = true;
        public int RespawnHPPercentage = 50;
        public bool RespawnAtCaveEntrance = false;

        string IIdentifiedData.Id => Id;

        private void OnValidate()
        {
            XpLossPercentageOnDeath = Mathf.Clamp(XpLossPercentageOnDeath, 0, 100);
            GoldLossPercentageOnDeath = Mathf.Clamp(GoldLossPercentageOnDeath, 0, 100);
            CorpseRecoveryTimeHours = Mathf.Max(1, CorpseRecoveryTimeHours);
            RespawnHPPercentage = Mathf.Clamp(RespawnHPPercentage, 1, 100);
        }
    }
}
