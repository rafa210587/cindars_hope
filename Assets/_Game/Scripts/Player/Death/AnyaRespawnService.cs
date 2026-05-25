using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Player;
using System;
using UnityEngine;

namespace CindarsHope.Player.Death
{
    public class AnyaRespawnService
    {
        private readonly PlayerManager _playerManager;
        private readonly StaminaManager _staminaManager;
        private readonly ManaManager _manaManager;
        private readonly Transform _respawnPoint;

        public AnyaRespawnService(
            PlayerManager playerManager,
            StaminaManager staminaManager,
            ManaManager manaManager,
            Transform respawnPoint)
        {
            _playerManager = playerManager ?? throw new ArgumentNullException(nameof(playerManager));
            _staminaManager = staminaManager;
            _manaManager = manaManager;
            _respawnPoint = respawnPoint ?? throw new ArgumentNullException(nameof(respawnPoint));
        }

        public void RespawnAtAnyaFountain()
        {
            // Restore HP
            _playerManager.SetHP(_playerManager.MaxHP);

            // Restore Stamina
            if (_staminaManager != null)
            {
                _staminaManager.RestoreStamina(_staminaManager.MaxStamina);
            }

            // Restore Mana if exists
            if (_manaManager != null && _manaManager.MaxMana > 0)
            {
                _manaManager.RestoreMana(_manaManager.MaxMana);
            }

            // Move player to respawn point
            var playerTransform = _playerManager.gameObject.transform;
            playerTransform.position = _respawnPoint.position;

            // Publish respawn completed event
            GameEventBus.Publish(new AnyaRespawnCompletedEvent());
        }
    }
}
