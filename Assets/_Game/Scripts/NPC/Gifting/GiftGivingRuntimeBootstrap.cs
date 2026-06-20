using UnityEngine;

namespace CindarsHope.NPC.Gifting
{
    /// <summary>
    /// fable_72 — garante que o GiftGivingService existe em runtime (singleton DontDestroyOnLoad).
    /// Mesmo padrão de FriendshipRuntimeBootstrap: FindAnyObjectByType é permitido aqui (wiring de
    /// setup, NÃO comunicação de gameplay) só para não duplicar o serviço quando já presente em cena.
    /// </summary>
    public static class GiftGivingRuntimeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (Object.FindAnyObjectByType<GiftGivingService>() != null)
            {
                return;
            }

            var go = new GameObject("GiftGivingService");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<GiftGivingService>();
            Debug.Log("[GiftGivingRuntimeBootstrap] GiftGivingService instanciado via bootstrap.");
        }
    }
}
