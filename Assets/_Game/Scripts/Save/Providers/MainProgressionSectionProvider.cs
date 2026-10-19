using CindarsHope.Fonte;

namespace CindarsHope.Save.Providers
{
    /// <summary>
    /// Provider de save do estado de progressão principal (Ato 5 / endgame). Fonte do estado:
    /// <see cref="FonteRuntimeService.Instance"/>.Progression. Deve ser restaurado APÓS
    /// <see cref="FonteSectionProvider"/>, pois depende da seção recriada por RestoreFromSave.
    /// Fallback: endgame não iniciado (defaults) em saves legados sem a seção.
    /// </summary>
    public class MainProgressionSectionProvider : ISaveSectionProvider
    {
        public string ProviderId => "main_progression";

        public object Capture(GameSaveData existingSaveData)
        {
            var fonte = FonteRuntimeService.Instance;
            if (fonte == null || fonte.Progression == null)
            {
                return existingSaveData?.MainProgression ?? new MainProgressionSaveData();
            }

            var prog = fonte.Progression;
            return new MainProgressionSaveData
            {
                CurrentAct = (int)prog.CurrentAct,
                Level100GateState = (int)prog.Level100GateState,
                Level101AccessState = (int)prog.Level101AccessState,
                FinalChoiceState = (int)prog.FinalChoiceState,
                PostGameWorldState = prog.PostGameWorldState
            };
        }

        public void Restore(object sectionData)
        {
            var fonte = FonteRuntimeService.Instance;
            if (fonte == null)
            {
                return;
            }

            // sectionData null (save legado) = endgame não iniciado (defaults).
            var data = sectionData as MainProgressionSaveData;
            if (data == null)
            {
                return;
            }

            fonte.RestoreEndgameState(
                data.CurrentAct,
                data.Level100GateState,
                data.Level101AccessState,
                data.FinalChoiceState,
                data.PostGameWorldState);
        }
    }
}
