using UnityEngine;

namespace CindarsHope.NPC.Schedule
{
    /// <summary>
    /// Ensures NpcScheduleService singleton exists after scene load.
    /// Scans for all NpcScheduleAnchor instances and registers them.
    /// FindObjectOfType is permitted in bootstrap (not runtime gameplay communication).
    /// </summary>
    public static class NpcScheduleRuntimeBootstrap
    {
#pragma warning disable CS0618
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (NpcScheduleService.Instance != null)
            {
                RegisterAnchors(NpcScheduleService.Instance);
                return;
            }

            var go = new GameObject("NpcScheduleService");
            Object.DontDestroyOnLoad(go);
            var service = go.AddComponent<NpcScheduleService>();
            RegisterAnchors(service);
            Debug.Log("[NpcScheduleRuntimeBootstrap] NpcScheduleService created.");
        }
#pragma warning restore CS0618

        private static void RegisterAnchors(NpcScheduleService service)
        {
#pragma warning disable CS0618
            var anchors = Object.FindObjectsOfType<NpcScheduleAnchor>();
#pragma warning restore CS0618
            foreach (var anchor in anchors)
            {
                service.RegisterAnchor(anchor);
            }

            if (anchors.Length > 0)
            {
                Debug.Log($"[NpcScheduleRuntimeBootstrap] Registered {anchors.Length} NpcScheduleAnchor(s).");
            }
        }
    }
}
