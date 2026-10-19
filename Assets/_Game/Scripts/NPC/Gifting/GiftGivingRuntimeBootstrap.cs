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
        public static GiftGivingService Install(Transform owner)
        {
            var existing = Object.FindAnyObjectByType<GiftGivingService>();
            if (existing != null) return existing;

            var go = new GameObject("GiftGivingService");
            if (owner != null) go.transform.SetParent(owner, false);
            else Object.DontDestroyOnLoad(go);
            var service = go.AddComponent<GiftGivingService>();
            Debug.Log("[GiftGivingRuntimeBootstrap] GiftGivingService instanciado via bootstrap.");
            return service;
        }
    }
}
