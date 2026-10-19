using UnityEngine;

namespace CindarsHope.NPC.Friendship
{
    /// <summary>
    /// fable_26 — garante que o FriendshipService existe em runtime (singleton DontDestroyOnLoad).
    /// Mesmo padrão de FarmDailyGoalRuntimeBootstrap: FindAnyObjectByType é permitido aqui (wiring de
    /// setup, NÃO comunicação de gameplay) só para não duplicar o serviço quando já presente em cena.
    /// </summary>
    public static class FriendshipRuntimeBootstrap
    {
        public static FriendshipService Install(Transform owner)
        {
            var existing = Object.FindAnyObjectByType<FriendshipService>();
            if (existing != null) return existing;

            var go = new GameObject("FriendshipService");
            if (owner != null) go.transform.SetParent(owner, false);
            else Object.DontDestroyOnLoad(go);
            var service = go.AddComponent<FriendshipService>();
            Debug.Log("[FriendshipRuntimeBootstrap] FriendshipService instanciado via bootstrap.");
            return service;
        }
    }
}
