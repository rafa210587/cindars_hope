using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Player.Death;
using UnityEngine;

namespace CindarsHope.Cave.Death
{
    [DisallowMultipleComponent]
    public class DeathSystemBootstrap : MonoBehaviour
    {
        private CaveDeathPolicy _policy;
        private CaveDeathResolver _resolver;
        private CorpseRecoveryManager _recoveryManager;
        private AnyaRespawnService _respawnService;

        private void Awake()
        {
            InitializeDeathSystem();
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
            GameEventBus.Subscribe<CorpseRecoveredEvent>(OnCorpseRecovered);
            GameEventBus.Subscribe<CorpseReplacedEvent>(OnCorpseReplaced);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
            GameEventBus.Unsubscribe<CorpseRecoveredEvent>(OnCorpseRecovered);
            GameEventBus.Unsubscribe<CorpseReplacedEvent>(OnCorpseReplaced);
        }

        private void InitializeDeathSystem()
        {
            var bootstrap = GameBootstrap.Instance;
            if (bootstrap == null)
            {
                Debug.LogError("[DeathSystemBootstrap] GameBootstrap not found");
                return;
            }

            _policy = new CaveDeathPolicy();
            _recoveryManager = bootstrap.CorpseRecoveryManager;

            if (_recoveryManager == null)
            {
                Debug.LogError("[DeathSystemBootstrap] CorpseRecoveryManager not found in bootstrap");
                return;
            }

            _resolver = new CaveDeathResolver(
                _policy,
                bootstrap.CaveRunManager,
                bootstrap.PlayerManager,
                bootstrap.InventoryManager,
                bootstrap.EquipmentManager,
                bootstrap.PlayerProgressionManager,
                bootstrap.TimeManager
            );

            if (bootstrap.AnyaFountain != null)
            {
                _respawnService = new AnyaRespawnService(
                    bootstrap.PlayerManager,
                    bootstrap.StaminaManager,
                    bootstrap.ManaManager,
                    bootstrap.AnyaFountain.RespawnPoint
                );
            }
            else
            {
                Debug.LogWarning("[DeathSystemBootstrap] AnyaFountain not found. Respawn service not initialized.");
            }

            Debug.Log("[DeathSystemBootstrap] Death system initialized successfully");
        }

        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            if (_resolver == null)
            {
                Debug.LogError("[DeathSystemBootstrap] Resolver not initialized on death event");
                return;
            }

            if (!_resolver.IsDeathInCave(evt.SceneName))
            {
                Debug.Log($"[DeathSystemBootstrap] Death outside cave ({evt.SceneName}), no corpse created");
                return;
            }

            Debug.Log($"[DeathSystemBootstrap] Player died in cave ({evt.SceneName}), resolving death...");
            _resolver.ResolveCaveDeath(evt.SceneName);

            // Set active corpse in recovery manager
            if (_recoveryManager != null && _resolver.LastCreatedCorpse != null)
            {
                _recoveryManager.SetActiveCorpse(_resolver.LastCreatedCorpse);
                Debug.Log($"[DeathSystemBootstrap] Corpse {_resolver.LastCreatedCorpse.CorpseId} set as active");
            }

            // Trigger respawn
            if (_respawnService != null)
            {
                _respawnService.RespawnAtAnyaFountain();
                Debug.Log("[DeathSystemBootstrap] Player respawned at Anya's Fountain");
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
