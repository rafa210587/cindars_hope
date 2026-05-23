using System;
using UnityEngine;

namespace CindarsHope.Player.Death
{
    [CreateAssetMenu(fileName = "CorpseRecovery", menuName = "CindarsHope/Player/CorpseRecovery")]
    public class CorpseRecoverySO : ScriptableObject
    {
        public float CorpseDecayTimeHours = 24f;
        public int MaxCorpsesPerLocation = 3;
        public bool ItemsDropOnDeath = true;
        public bool EquipmentDropsOnDeath = false;

        private void OnValidate()
        {
            CorpseDecayTimeHours = Mathf.Max(1f, CorpseDecayTimeHours);
            MaxCorpsesPerLocation = Mathf.Max(1, MaxCorpsesPerLocation);
        }
    }

    [Serializable]
    public class CorpseSaveData
    {
        public string LocationSceneName;
        public Vector3 Position;
        public long CreatedAtUtcTicks;
        public string ItemIdList;
        public string EquipmentIdList;
    }
}
