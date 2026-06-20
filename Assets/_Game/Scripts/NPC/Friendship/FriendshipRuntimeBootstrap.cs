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
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (Object.FindAnyObjectByType<FriendshipService>() != null)
            {
                return;
            }

            var go = new GameObject("FriendshipService");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<FriendshipService>();
            Debug.Log("[FriendshipRuntimeBootstrap] FriendshipService instanciado via bootstrap.");
        }
    }
}
