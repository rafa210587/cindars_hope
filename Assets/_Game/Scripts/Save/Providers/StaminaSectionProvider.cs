using CindarsHope.Player;

namespace CindarsHope.Save.Providers
{
    /// <summary>
    /// Provider de save da stamina do jogador. Fonte do estado: <see cref="StaminaManager"/>
    /// injetado via constructor. Fallback: stamina cheia (100/100) quando o manager não está disponível.
    /// </summary>
    public class StaminaSectionProvider : ISaveSectionProvider
    {
        private readonly StaminaManager _staminaManager;

        public StaminaSectionProvider(StaminaManager staminaManager)
        {
            _staminaManager = staminaManager;
        }

        public string ProviderId => "stamina";

        public object Capture(GameSaveData existingSaveData)
        {
            if (_staminaManager == null)
            {
                return new StaminaSaveData { CurrentStamina = 100, MaxStamina = 100 };
            }

            return new StaminaSaveData
            {
                CurrentStamina = _staminaManager.CurrentStamina,
                MaxStamina = _staminaManager.MaxStamina
            };
        }

        public void Restore(object sectionData)
        {
            if (_staminaManager == null)
            {
                return;
            }

            var data = sectionData as StaminaSaveData;
            if (data != null)
            {
                _staminaManager.Initialize(data.MaxStamina, data.CurrentStamina);
            }
        }
    }
}
