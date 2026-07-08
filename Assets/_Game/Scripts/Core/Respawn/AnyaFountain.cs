using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Core.Respawn
{
    [DisallowMultipleComponent]
    public class AnyaFountain : MonoBehaviour, IAnyaFountainRespawnPoint
    {
        // Registro estatico de instancias ativas, mesmo padrao de EnemyHealth.ActiveInstances —
        // evita FindObjectsByType em runtime (rule unity-architecture #1).
        public static readonly List<IAnyaFountainRespawnPoint> ActiveInstances = new List<IAnyaFountainRespawnPoint>();

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

        private void OnEnable()
        {
            if (!ActiveInstances.Contains(this))
            {
                ActiveInstances.Add(this);
            }
        }

        private void OnDisable()
        {
            ActiveInstances.Remove(this);
        }
    }
}
