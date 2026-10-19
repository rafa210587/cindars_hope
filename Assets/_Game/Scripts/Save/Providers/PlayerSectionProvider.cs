using CindarsHope.Player;
using CindarsHope.Player.Conditions;
using UnityEngine;

namespace CindarsHope.Save.Providers
{
    /// <summary>
    /// Provider de save do estado do jogador (posição, HP, fome, mana, fadiga). Fonte do estado:
    /// <see cref="PlayerManager"/>, <see cref="HungerManager"/> e <see cref="ManaManager"/>,
    /// injetados via constructor. A fadiga é acessada via singleton <see cref="Conditions.PlayerConditionService"/>.
    /// Nota: o restore da posição do Transform permanece no SaveManager (que detém a ref serializada
    /// ao Transform — não exposta neste provider para evitar callback desnecessário).
    /// Fallback: seção null quando o PlayerManager não está disponível.
    /// </summary>
    public class PlayerSectionProvider : ISaveSectionProvider
    {
        private readonly PlayerManager _playerManager;
        private readonly HungerManager _hungerManager;
        private readonly ManaManager _manaManager;
        private readonly System.Func<Vector2> _getPlayerPosition;

        /// <param name="playerManager">Manager do jogador — obrigatório para capture.</param>
        /// <param name="hungerManager">Manager de fome — pode ser null (fallback 0/1).</param>
        /// <param name="manaManager">Manager de mana — pode ser null (fallback 0).</param>
        /// <param name="getPlayerPosition">Função que retorna a posição atual do player transform.</param>
        public PlayerSectionProvider(
            PlayerManager playerManager,
            HungerManager hungerManager,
            ManaManager manaManager,
            System.Func<Vector2> getPlayerPosition)
        {
            _playerManager = playerManager;
            _hungerManager = hungerManager;
            _manaManager = manaManager;
            _getPlayerPosition = getPlayerPosition;
        }

        public string ProviderId => "player";

        public object Capture(GameSaveData existingSaveData)
        {
            if (_playerManager == null)
            {
                Debug.LogWarning("PlayerSectionProvider: PlayerManager não injetado. Seção do jogador omitida.");
                return null;
            }

            var currentHunger = _hungerManager != null ? _hungerManager.CurrentHunger : 0;
            var maxHunger = _hungerManager != null ? _hungerManager.MaxHunger : 1;
            if (_hungerManager == null)
            {
                Debug.LogWarning("PlayerSectionProvider: HungerManager não injetado. Fome usou fallbacks seguros.");
            }

            var playerPosition = _getPlayerPosition != null ? _getPlayerPosition() : Vector2.zero;
            var playerData = new PlayerSaveData
            {
                CurrentHP = _playerManager.CurrentHP,
                MaxHP = _playerManager.MaxHP,
                Gold = _playerManager.CurrentGold,
                CurrentHunger = currentHunger,
                MaxHunger = maxHunger,
                HungerFractionalDrainAccumulator = _hungerManager != null
                    ? _hungerManager.FractionalDrainAccumulator
                    : 0f,
                PlayerPosition = playerPosition
            };

            if (playerData != null && _manaManager != null)
            {
                playerData.CurrentMana = _manaManager.CurrentMana;
                playerData.MaxMana = _manaManager.MaxMana;
            }

            // F16: fadiga persistida via serviço runtime (campo aditivo; default 0 em saves legados).
            var conditionService = PlayerConditionService.Instance;
            if (playerData != null && conditionService != null)
            {
                playerData.Fatigue = conditionService.CurrentFatigue;
            }

            return playerData;
        }

        public void Restore(object sectionData)
        {
            var data = sectionData as PlayerSaveData;

            if (_playerManager != null)
            {
                if (data == null)
                {
                    Debug.LogWarning("PlayerSectionProvider: PlayerSaveData nula; restore do jogador ignorado.");
                }
                else
                {
                    _playerManager.RestoreState(data.MaxHP, data.CurrentHP, data.Gold);
                }
            }
            else
            {
                Debug.LogWarning("PlayerSectionProvider: PlayerManager não injetado; restore do jogador ignorado.");
            }

            if (data != null && _hungerManager != null)
            {
                _hungerManager.RestoreFromSaveData(
                    data.CurrentHunger,
                    data.MaxHunger,
                    data.HungerFractionalDrainAccumulator);
            }

            // F16: restaura fadiga (saves legados carregam com 0).
            if (data != null && PlayerConditionService.Instance != null)
            {
                PlayerConditionService.Instance.SetFatigue(data.Fatigue);
            }

            if (data != null && _manaManager != null)
            {
                _manaManager.RestoreFromSaveData(new ManaManagerSaveData
                {
                    CurrentMana = data.CurrentMana,
                    MaxMana = data.MaxMana
                });
            }

            // Nota: restore da posição do Transform permanece no SaveManager (detentor da ref serializada).
        }
    }
}
