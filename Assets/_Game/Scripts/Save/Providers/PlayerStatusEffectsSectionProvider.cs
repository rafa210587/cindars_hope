using CindarsHope.Player;

namespace CindarsHope.Save.Providers
{
    /// <summary>
    /// Provider de save dos efeitos de status ativos no jogador. Fonte do estado:
    /// <see cref="StatusEffectManager"/> injetado via constructor. Fallback: lista vazia (sem efeitos
    /// ativos) quando o manager não está disponível ou não foi inicializado.
    /// </summary>
    public class PlayerStatusEffectsSectionProvider : ISaveSectionProvider
    {
        private readonly StatusEffectManager _statusEffectManager;

        public PlayerStatusEffectsSectionProvider(StatusEffectManager statusEffectManager)
        {
            _statusEffectManager = statusEffectManager;
        }

        public string ProviderId => "player_status_effects";

        public object Capture(GameSaveData existingSaveData)
        {
            var data = new PlayerStatusEffectsSaveData();

            if (_statusEffectManager == null || !_statusEffectManager.IsInitialized)
            {
                return data;
            }

            foreach (var kvp in _statusEffectManager.ActiveEffects)
            {
                if (kvp.Value != null && kvp.Value.IsActive)
                {
                    data.ActiveEffects.Add(new StatusEffectEntryData
                    {
                        EffectId = kvp.Key,
                        RemainingSeconds = kvp.Value.RemainingSeconds
                    });
                }
            }

            return data;
        }

        public void Restore(object sectionData)
        {
            if (_statusEffectManager == null)
            {
                return;
            }

            var data = sectionData as PlayerStatusEffectsSaveData;
            if (data == null)
            {
                return;
            }

            _statusEffectManager.Shutdown();
            _statusEffectManager.Initialize();
            foreach (var effectEntry in data.ActiveEffects)
            {
                if (!string.IsNullOrWhiteSpace(effectEntry.EffectId) && effectEntry.RemainingSeconds > 0)
                {
                    _statusEffectManager.TryAddEffect(effectEntry.EffectId, effectEntry.RemainingSeconds);
                }
            }
        }
    }
}
