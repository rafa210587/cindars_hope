using System.Collections;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Locations;
using CindarsHope.Player;
using CindarsHope.Player.Death;
using UnityEngine;

namespace CindarsHope.Cave.Death
{
    /// <summary>
    /// Processa PlayerDiedEvent: cria corpo (na caverna) e respawna o jogador.
    ///
    /// Nasce sozinho via self-bootstrap estatico (idiom *RuntimeBootstrap do projeto):
    /// GameObject DontDestroyOnLoad + singleton guard. Em vez de inicializar no Awake (onde
    /// GameBootstrap.Instance ainda pode ser null por ordem de boot), uma coroutine BindWhenReady
    /// espera GameBootstrap e seus managers (PlayerManager, CorpseRecoveryManager) ficarem prontos.
    ///
    /// A AnyaFountain e objeto POR CENA, entao a ref de GameBootstrap pode ficar stale apos trocar de
    /// cena. O respawn re-resolve a Fonte da cena ATIVA no momento da morte (FindAnyObjectByType e
    /// permitido aqui: e wiring de setup do fluxo de respawn, NAO comunicacao de gameplay).
    /// </summary>
    [DisallowMultipleComponent]
    public class DeathSystemBootstrap : MonoBehaviour
    {
        private const int MaxBindAttempts = 120;

        private static DeathSystemBootstrap _instance;

        private CaveDeathPolicy _policy;
        private CaveDeathResolver _resolver;
        private CorpseRecoveryManager _recoveryManager;
        private PlayerManager _playerManager;
        private bool _initialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (_instance != null)
            {
                return;
            }

            var go = new GameObject("DeathSystemBootstrap");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<DeathSystemBootstrap>();
        }

        private void Awake()
        {
            // Singleton guard: descarta duplicado se uma cena trouxer outro componente.
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
            GameEventBus.Subscribe<CorpseRecoveredEvent>(OnCorpseRecovered);
            GameEventBus.Subscribe<CorpseReplacedEvent>(OnCorpseReplaced);
            StartCoroutine(BindWhenReady());
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
            GameEventBus.Unsubscribe<CorpseRecoveredEvent>(OnCorpseRecovered);
            GameEventBus.Unsubscribe<CorpseReplacedEvent>(OnCorpseReplaced);
        }

        private IEnumerator BindWhenReady()
        {
            if (_initialized)
            {
                yield break;
            }

            for (var attempt = 0; attempt < MaxBindAttempts; attempt++)
            {
                var bootstrap = GameBootstrap.Instance;
                if (bootstrap == null || bootstrap.PlayerManager == null || bootstrap.CorpseRecoveryManager == null)
                {
                    yield return null;
                    continue;
                }

                Initialize(bootstrap);
                yield break;
            }

            Debug.LogError(
                "[DeathSystemBootstrap] GameBootstrap/PlayerManager/CorpseRecoveryManager nao ficaram prontos " +
                $"apos {MaxBindAttempts} frames. Sistema de morte NAO inicializado (morte->corpo->respawn indisponivel).");
        }

        private void Initialize(GameBootstrap bootstrap)
        {
            _policy = new CaveDeathPolicy();
            _recoveryManager = bootstrap.CorpseRecoveryManager;
            _playerManager = bootstrap.PlayerManager;

            _resolver = new CaveDeathResolver(
                _policy,
                bootstrap.CaveRunManager,
                bootstrap.PlayerManager,
                bootstrap.InventoryManager,
                bootstrap.EquipmentManager,
                bootstrap.PlayerProgressionManager,
                bootstrap.TimeManager
            );

            _initialized = true;
            Debug.Log("[DeathSystemBootstrap] Death system initialized");
        }

        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            if (!_initialized || _resolver == null)
            {
                Debug.LogError("[DeathSystemBootstrap] Morte recebida antes do sistema inicializar (deps ausentes).");
                return;
            }

            if (_resolver.IsDeathInCave(evt.SceneName))
            {
                Debug.Log($"[DeathSystemBootstrap] Player died in cave ({evt.SceneName}), resolving death...");
                _resolver.ResolveCaveDeath(evt.SceneName);

                if (_recoveryManager != null && _resolver.LastCreatedCorpse != null)
                {
                    _recoveryManager.SetActiveCorpse(_resolver.LastCreatedCorpse);
                    Debug.Log($"[DeathSystemBootstrap] Corpse {_resolver.LastCreatedCorpse.CorpseId} set as active");
                }
            }
            else
            {
                // Morte fora da caverna (Farm/Town): sem corpo, mas o jogador NAO pode ficar preso morto.
                Debug.Log($"[DeathSystemBootstrap] Death outside cave ({evt.SceneName}), no corpse created");
            }

            // Respawn/revive sempre acontece (dentro ou fora da caverna) para nao travar o jogador.
            Respawn();
        }

        /// <summary>
        /// Re-resolve a AnyaFountain da cena ATIVA no momento do respawn (a ref pode estar stale por
        /// causa do DontDestroyOnLoad) e respawna ali. Se nenhuma Fonte existir na cena atual, faz o
        /// minimo seguro: restaura o HP no lugar (revive) para o jogador nunca ficar preso morto.
        /// </summary>
        private void Respawn()
        {
            var fountain = Object.FindAnyObjectByType<AnyaFountain>();
            if (fountain != null && fountain.RespawnPoint != null)
            {
                var bootstrap = GameBootstrap.Instance;
                var respawnService = new AnyaRespawnService(
                    _playerManager,
                    bootstrap != null ? bootstrap.StaminaManager : null,
                    bootstrap != null ? bootstrap.ManaManager : null,
                    fountain.RespawnPoint);

                respawnService.RespawnAtAnyaFountain();
                Debug.Log("[DeathSystemBootstrap] Player respawned at Anya's Fountain");
                return;
            }

            // Fallback seguro: sem Fonte na cena atual (respawn cross-cena nao resolvido neste slice).
            // Restaura HP no lugar para reviver o jogador (anti-softlock). Sem inventar regra de balance nova.
            if (_playerManager != null)
            {
                _playerManager.SetHP(_playerManager.MaxHP);
                Debug.LogWarning(
                    "[DeathSystemBootstrap] Nenhuma AnyaFountain na cena atual. Revivendo o jogador no lugar " +
                    "(HP restaurado). Respawn cross-cena na Fonte nao implementado neste slice.");
            }
        }

        private void OnCorpseRecovered(CorpseRecoveredEvent evt)
        {
            Debug.Log($"[DeathSystemBootstrap] Corpse {evt.CorpseId} fully recovered");
        }

        private void OnCorpseReplaced(CorpseReplacedEvent evt)
        {
            Debug.Log($"[DeathSystemBootstrap] Corpse {evt.OldCorpseId} replaced by {evt.NewCorpseId}");
        }
    }
}
