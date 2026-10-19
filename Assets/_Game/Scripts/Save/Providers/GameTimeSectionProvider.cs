using CindarsHope.Core;
using CindarsHope.Core.Time;
using UnityEngine;

namespace CindarsHope.Save.Providers
{
    /// <summary>
    /// Provider de save do tempo de jogo (fase do dia, tempo decorrido na fase, dia corrente).
    /// Fontes do estado: <see cref="GameTimeManager"/> e <see cref="TimeManager"/>, ambos injetados
    /// via constructor. Fallback: dia 1, fase 0, tempo 0 quando os managers não estão disponíveis.
    /// </summary>
    public class GameTimeSectionProvider : ISaveSectionProvider
    {
        private readonly GameTimeManager _gameTimeManager;
        private readonly TimeManager _timeManager;

        public GameTimeSectionProvider(GameTimeManager gameTimeManager, TimeManager timeManager)
        {
            _gameTimeManager = gameTimeManager;
            _timeManager = timeManager;
        }

        public string ProviderId => "game_time";

        public object Capture(GameSaveData existingSaveData)
        {
            if (_gameTimeManager == null)
            {
                return new GameTimeSaveData
                {
                    CurrentDay = _timeManager != null ? _timeManager.CurrentDay : 1,
                    CurrentPhase = 0,
                    PhaseElapsedSeconds = 0f
                };
            }

            return new GameTimeSaveData
            {
                CurrentDay = _timeManager != null ? _timeManager.CurrentDay : 1,
                CurrentPhase = (int)_gameTimeManager.CurrentPhase,
                PhaseElapsedSeconds = _gameTimeManager.PhaseTimer
            };
        }

        public void Restore(object sectionData)
        {
            if (_gameTimeManager == null)
            {
                return;
            }

            var data = sectionData as GameTimeSaveData;
            if (data != null)
            {
                _gameTimeManager.RestorePhaseState(data.CurrentPhase, data.PhaseElapsedSeconds);
            }
        }
    }
}
