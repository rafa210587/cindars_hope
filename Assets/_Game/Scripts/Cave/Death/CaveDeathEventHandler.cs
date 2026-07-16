using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Player.Death;
using UnityEngine;

namespace CindarsHope.Cave.Death
{
    public class CaveDeathEventHandler : MonoBehaviour
    {
        private CaveDeathPolicy _policy;
        private CaveDeathResolver _resolver;
        private CorpseRecoveryManager _recoveryManager;
        private AnyaRespawnService _respawnService;

        private void OnEnable()
        {
            GameEventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
        }

        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            if (!_resolver.IsDeathInCave(evt.SceneName))
            {
                return;
            }

            _resolver.ResolveCaveDeath(evt.SceneName);
        }

        public void Initialize()
        {
            var bootstrap = GameBootstrap.Instance;
            if (bootstrap == null)
            {
                Debug.LogError("[CaveDeathEventHandler] GameBootstrap not found");
                return;
            }

            _policy = new CaveDeathPolicy();
            // arch: quebra do par mutuo Core|Player (2026-07-15) — bootstrap.CorpseRecoveryManager/
            // PlayerManager/PlayerProgressionManager/StaminaManager/ManaManager agora retornam
            // object/MonoBehaviour (Core nao nomeia mais CindarsHope.Player); cast local para os
            // tipos concretos.
            _recoveryManager = bootstrap.CorpseRecoveryManager as CorpseRecoveryManager;
            var playerManager = bootstrap.PlayerManager as CindarsHope.Player.PlayerManager;

            // arch: Core|Equipment (spec_arch_core_equipment_cycle_reduction_v35) — EquipmentManager
            // resolvido via EquipmentManager.Instance (self-registro, molde Craft/Economy/Skills).
            // arch: quebra do par mutuo Core|Inventory (2026-07-15) — cast local para o tipo concreto
            // (bootstrap.InventoryManager agora retorna a porta IInventoryRuntime).
            _resolver = new CaveDeathResolver(
                _policy,
                CindarsHope.Cave.Runtime.CaveRunManager.Instance,
                playerManager,
                bootstrap.InventoryManager as CindarsHope.Inventory.InventoryManager,
                CindarsHope.Equipment.EquipmentManager.Instance,
                bootstrap.PlayerProgressionManager as CindarsHope.Player.Progression.PlayerProgressionManager,
                bootstrap.TimeManager
            );

            if (bootstrap.AnyaFountain != null)
            {
                _respawnService = new AnyaRespawnService(
                    playerManager,
                    bootstrap.StaminaManager as CindarsHope.Player.StaminaManager,
                    bootstrap.ManaManager as CindarsHope.Player.ManaManager,
                    bootstrap.AnyaFountain.RespawnPoint
                );
            }
        }
    }
}
