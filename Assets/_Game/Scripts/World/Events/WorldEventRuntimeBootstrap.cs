using UnityEngine;

namespace CindarsHope.World.Events
{
    /// <summary>
    /// fable_37 — garante o WorldEventService em runtime (mesmo idioma do WorldWeatherRuntimeBootstrap /
    /// FarmDailyGoalRuntimeBootstrap). FindAnyObjectByType é permitido aqui: wiring de bootstrap, não
    /// comunicação de gameplay. Após criar/achar o serviço, resolve o dia atual a partir do TimeManager
    /// para que os hooks fiquem coerentes desde o primeiro frame (sem esperar o próximo DayStartedEvent).
    /// </summary>
    public static class WorldEventRuntimeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            var service = WorldEventService.Instance;
            if (service == null)
            {
                service = Object.FindAnyObjectByType<WorldEventService>();
            }

            if (service == null)
            {
                var go = new GameObject("WorldEventService");
                Object.DontDestroyOnLoad(go);
                service = go.AddComponent<WorldEventService>();
                Debug.Log("[WorldEventRuntimeBootstrap] WorldEventService instanciado via bootstrap.");
            }

            // Resolve a âncora da praça caso o marcador de cena tenha despertado antes do serviço existir
            // (a barraca nasce na origem se não houver âncora). FindAnyObjectByType: wiring de bootstrap.
            var anchor = Object.FindAnyObjectByType<FestivalStallAnchor>();
            if (anchor != null)
            {
                service.SetPlazaAnchor(anchor.transform);
            }

            var timeManager = Object.FindAnyObjectByType<CindarsHope.Core.Time.TimeManager>();
            if (timeManager != null && timeManager.IsInitialized)
            {
                service.ResolveForDay(timeManager.CurrentDay);
            }
        }
    }
}
