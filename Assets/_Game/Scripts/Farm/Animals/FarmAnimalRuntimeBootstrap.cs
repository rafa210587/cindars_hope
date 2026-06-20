using UnityEngine;

namespace CindarsHope.Farm.Animals
{
    /// <summary>
    /// fable_12 — garante que <see cref="FarmAnimalRegistry"/> exista em runtime e tenha as definições
    /// canônicas dos 3 animais registradas (build em memória a partir de <see cref="FarmAnimalCatalog"/>
    /// quando não há AnimalDataSO em cena). Padrão idêntico ao FarmDailyGoalRuntimeBootstrap.
    ///
    /// FindAnyObjectByType é permitido aqui (wiring/setup, não comunicação de gameplay) — mesma
    /// exceção documentada do FarmDailyGoalRuntimeBootstrap.
    /// </summary>
    public static class FarmAnimalRuntimeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            var registry = Object.FindAnyObjectByType<FarmAnimalRegistry>();
            if (registry == null)
            {
                var go = new GameObject("FarmAnimalRegistry");
                Object.DontDestroyOnLoad(go);
                registry = go.AddComponent<FarmAnimalRegistry>();
            }

            RegisterCanonicalDefinitions(registry);
        }

        /// <summary>
        /// Cria AnimalDataSO em memória para cada entrada do catálogo e registra no registry. Idempotente:
        /// RegisterDefinition sobrescreve por id, então AnimalDataSO de cena/bootstrap têm prioridade se
        /// registrados depois.
        /// </summary>
        public static void RegisterCanonicalDefinitions(FarmAnimalRegistry registry)
        {
            if (registry == null)
            {
                return;
            }

            foreach (var entry in FarmAnimalCatalog.GetAll())
            {
                var so = ScriptableObject.CreateInstance<AnimalDataSO>();
                so.Configure(
                    entry.AnimalId,
                    entry.DisplayName,
                    entry.Species,
                    entry.HousingType,
                    entry.PurchaseItemId,
                    entry.FeedItemId,
                    entry.ProductItemId,
                    entry.ProductIntervalDays,
                    FarmAnimalCatalog.NeglectDeathDays,
                    Color.white);
                registry.RegisterDefinition(so);
            }
        }
    }
}
