using UnityEngine;

namespace CindarsHope.Farm.Runtime
{
    /// <summary>
    /// Garante que FarmDailyGoalService existe em runtime.
    /// Usa RuntimeInitializeOnLoadMethod para instanciar automaticamente.
    /// FindObjectOfType é permitido neste bootstrap (é wiring/setup, não gameplay communication).
    /// </summary>
    public static class FarmDailyGoalRuntimeBootstrap
    {
        public static FarmDailyGoalService Install(Transform owner)
        {
            // Se já existe (wired in scene), não duplicar
            var existing = Object.FindAnyObjectByType<FarmDailyGoalService>();
            if (existing != null)
            {
                return existing;
            }

            var go = new GameObject("FarmDailyGoalService");
            if (owner != null) go.transform.SetParent(owner, false);
            else Object.DontDestroyOnLoad(go);
            var service = go.AddComponent<FarmDailyGoalService>();
            Debug.Log("[FarmDailyGoalRuntimeBootstrap] FarmDailyGoalService instanciado via bootstrap.");
            return service;
        }
    }
}
