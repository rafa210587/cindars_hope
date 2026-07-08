using UnityEngine;

namespace CindarsHope.Core.Respawn
{
    [DisallowMultipleComponent]
    public class AnyaFountain : MonoBehaviour, IAnyaFountainRespawnPoint
    {
        public string FountainId = "anya_fountain_farm";
        public Transform RespawnPoint { get; private set; }

        private void Awake()
        {
            RespawnPoint = GetComponent<Transform>();
            if (RespawnPoint == null)
            {
                Debug.LogError("[AnyaFountain] RespawnPoint Transform not found", this);
            }
        }
    }
}
