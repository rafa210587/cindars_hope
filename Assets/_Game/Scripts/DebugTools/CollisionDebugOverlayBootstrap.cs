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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // Build guard: overlay so de debug tooling, nunca deve existir em build de producao final.
            // FindAnyObjectByType aqui e excecao documentada de DebugTools (nao gameplay code) para
            // evitar instanciar uma segunda instancia dormente do overlay.
            if (Object.FindAnyObjectByType<CollisionDebugOverlay>() != null)
            {
                return;
            }

            var go = new GameObject("CollisionDebugOverlay");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<CollisionDebugOverlay>();
#endif
        }
    }
}
