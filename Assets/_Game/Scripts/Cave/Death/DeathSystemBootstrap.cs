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
    /// Processa PlayerDiedEvent: cria o corpo (na caverna). NAO respawna mais automaticamente —
    /// a acao agora e do jogador, pela DeathScreenCanvasController (Lagrima da Deusa = reviver
    /// no lugar; ou Respawnar na Fonte da Anya, possivelmente cross-cena).
    ///
    /// Nasce sozinho via self-bootstrap estatico (idiom *RuntimeBootstrap do projeto):
    /// GameObject DontDestroyOnLoad + singleton guard. Em vez de inicializar no Awake (onde
    /// GameBootstrap.Instance ainda pode ser null por ordem de boot), uma coroutine BindWhenReady
    /// espera GameBootstrap e seus managers (PlayerManager, CorpseRecoveryManager) ficarem prontos.
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

        public static void Install(Transform owner)
        {
            if (_instance != null)
            {
                return;
            }

            var go = new GameObject("DeathSystemBootstrap");
            go.transform.SetParent(owner);
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

                // O resolver e DontDestroyOnLoad e nasceu antes de qualquer run de caverna; pega a
                // referencia VIVA do CaveRunManager agora (so existe durante um run de caverna).
                _resolver.SetCaveRunManager(GameBootstrap.Instance?.CaveRunManager);

                _resolver.ResolveCaveDeath(evt.SceneName);

                if (_recoveryManager != null && _resolver.LastCreatedCorpse != null)
                {
                    _recoveryManager.SetActiveCorpse(_resolver.LastCreatedCorpse);
                    Debug.Log($"[DeathSystemBootstrap] Corpse {_resolver.LastCreatedCorpse.CorpseId} set as active");
                }
            }
            else
            {
                // Morte fora da caverna (Farm/Town): sem corpo.
                Debug.Log($"[DeathSystemBootstrap] Death outside cave ({evt.SceneName}), no corpse created");
            }

            // NAO respawna mais aqui. A DeathScreenCanvasController abre a tela modal e o jogador
            // escolhe: Lagrima da Deusa (reviver no lugar) ou Respawnar na Fonte da Anya (anti-softlock,
            // cross-cena se preciso). Manter o respawn aqui dispararia uma acao antes da escolha.
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
