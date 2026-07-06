using CindarsHope.Core.Bootstrap;
using CindarsHope.Quests.Flags;
using UnityEngine;

namespace CindarsHope.City.Services
{
    /// <summary>
    /// fable_19 — runtime bridge que liga a fachada estática <see cref="CityServiceAccess"/> ao
    /// <see cref="QuestFlagService"/> (posse por flag, SEM nova seção de save) e ao PlayerManager
    /// (ouro). Mesmo idioma dos demais *RuntimeBootstrap: singleton DontDestroyOnLoad criado via
    /// RuntimeInitializeOnLoad; nenhuma comunicação de gameplay direta — apenas injeta os predicados
    /// nos pontos únicos já existentes (urban sell gate, shipping resolver, NpcShopController).
    ///
    /// As flags de civic service são registradas num QuestFlagRegistry/Service próprios deste sistema
    /// (city_rules.md Rule 5: "persisted as quest flags via QuestFlagService" — não exige a MESMA
    /// instância das quests). A persistência standalone das flags por save é DIFERIDA para a validação
    /// final (Play Mode), consistente com o gate desta spec.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CityServiceRuntimeBootstrap : MonoBehaviour
    {
        private static CityServiceRuntimeBootstrap s_instance;

        private readonly QuestFlagRegistry _flagRegistry = new QuestFlagRegistry();
        private QuestFlagService _flagService;

        /// <summary>Serviço de flags dos civic services (exposto para save/load futuro e teste de integração).</summary>
        public static QuestFlagService FlagService => s_instance?._flagService;

        public static CityServiceRuntimeBootstrap Install(Transform owner)
        {
            if (s_instance != null)
            {
                s_instance.WireAccess();
                return s_instance;
            }

            var go = new GameObject("CityServiceRuntimeBootstrap");
            if (owner != null) go.transform.SetParent(owner, false);
            else Object.DontDestroyOnLoad(go);
            return go.AddComponent<CityServiceRuntimeBootstrap>();
        }

        private void Awake()
        {
            if (s_instance != null && s_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            s_instance = this;

            CityServiceFlags.RegisterFlags(_flagRegistry);
            _flagService = new QuestFlagService(_flagRegistry);
            WireAccess();
        }

        private void OnDestroy()
        {
            if (s_instance != this) return;
            s_instance = null;
            // Não limpamos os delegates aqui: se a cena recarregar, o novo bootstrap religa.
        }

        private void WireAccess()
        {
            // Posse por flag (fonte única consumida pelo urban sell gate e pelo shipping resolver).
            CityServiceAccess.OwnsServiceQuery = flag => _flagService != null && _flagService.IsSet(flag);

            // Concessão da flag de posse (compra via diálogo de Tovin/Mara).
            CityServiceAccess.GrantPossessionAction = flag =>
            {
                _flagService?.GrantFlag(flag, CityServiceFlags.CityServiceSetter);
            };

            // Ouro: ligado ao PlayerManager via GameBootstrap (sem busca global de cena).
            CityServiceAccess.CurrentGoldFunc = () =>
            {
                var pm = GameBootstrap.Instance?.PlayerManager;
                return pm != null ? pm.CurrentGold : 0;
            };

            CityServiceAccess.SpendGoldFunc = cost =>
            {
                var pm = GameBootstrap.Instance?.PlayerManager;
                return pm != null && pm.TrySpendGold(cost);
            };
        }
    }
}
