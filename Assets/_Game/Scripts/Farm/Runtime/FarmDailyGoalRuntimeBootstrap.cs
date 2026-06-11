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
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            // Se já existe (wired in scene), não duplicar
            if (Object.FindAnyObjectByType<FarmDailyGoalService>() != null)
            {
                return;
            }

            var go = new GameObject("FarmDailyGoalService");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<FarmDailyGoalService>();
            Debug.Log("[FarmDailyGoalRuntimeBootstrap] FarmDailyGoalService instanciado via bootstrap.");
        }
    }
}
