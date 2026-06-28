using UnityEngine;

namespace CindarsHope.DebugTools
{
    /// <summary>
    /// Cria o <see cref="CollisionDebugOverlay"/> automaticamente em qualquer cena (Town, Cave...),
    /// dormente, alternável por F9. Não precisa regenerar cena. Dev-only.
    /// </summary>
    public static class CollisionDebugOverlayBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (Object.FindAnyObjectByType<CollisionDebugOverlay>() != null)
            {
                return;
            }

            var go = new GameObject("CollisionDebugOverlay");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<CollisionDebugOverlay>();
        }
    }
}
