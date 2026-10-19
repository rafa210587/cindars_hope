namespace CindarsHope.Cave.Death
{
    public class CaveDeathPolicy
    {
        public bool RemoveAllInventoryItems => true;
        public bool RemoveAllEquipment => true;
        public bool RemoveAllGold => true;
        public bool ResetXpToLevelStart => true;
        public bool CreateCorpse => true;
        public bool RedistributeEnemies => true;
        public bool RespawnAtAnyaFountain => true;
    }
}
