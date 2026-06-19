using UnityEngine;

namespace CindarsHope.Magic
{
    /// <summary>
    /// fable_07 — garante que <see cref="PlayerSpellbook"/> existe em runtime (DontDestroyOnLoad).
    /// Padrão F13/F16/F17 (RuntimeInitializeOnLoadMethod), mas SEM busca global de cena: usa o
    /// próprio singleton estático <see cref="PlayerSpellbook.Instance"/> como guarda de duplicata,
    /// evitando qualquer busca global de cena (rule no-runtime-global-search).
    /// </summary>
    public static class PlayerSpellbookRuntimeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (PlayerSpellbook.Instance != null)
            {
                return;
            }

            var go = new GameObject("PlayerSpellbook");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<PlayerSpellbook>();
            go.AddComponent<SpellItemUseController>();
            Debug.Log("[PlayerSpellbookRuntimeBootstrap] PlayerSpellbook instanciado via bootstrap.");
        }
    }
}
