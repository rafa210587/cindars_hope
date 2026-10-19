using CindarsHope.Skills;

namespace CindarsHope.Save.Providers
{
    /// <summary>
    /// Provider de save da árvore de habilidades (nós comprados, pontos gastos, tier).
    /// Fonte do estado: <see cref="SkillTreeManager"/> injetado via constructor. A restauração
    /// depende do nível do jogador — obtido de <see cref="ProgressionSectionProvider"/> que deve
    /// ser executado antes. Fallback: árvore vazia quando o manager não está disponível.
    /// </summary>
    public class SkillTreeSectionProvider : ISaveSectionProvider
    {
        private readonly SkillTreeManager _skillTreeManager;
        private readonly System.Func<int> _getLevelFunc;

        /// <param name="skillTreeManager">Manager da árvore de habilidades.</param>
        /// <param name="getLevelFunc">Função que retorna o nível atual do jogador (para restore).</param>
        public SkillTreeSectionProvider(SkillTreeManager skillTreeManager, System.Func<int> getLevelFunc)
        {
            _skillTreeManager = skillTreeManager;
            _getLevelFunc = getLevelFunc;
        }

        public string ProviderId => "skill_tree";

        public object Capture(GameSaveData existingSaveData)
        {
            return _skillTreeManager != null
                ? _skillTreeManager.CaptureSaveData()
                : new SkillTreeSaveData();
        }

        public void Restore(object sectionData)
        {
            if (_skillTreeManager == null)
            {
                return;
            }

            var data = sectionData as SkillTreeSaveData;
            if (data != null)
            {
                int level = _getLevelFunc != null ? _getLevelFunc() : 1;
                _skillTreeManager.RestoreFromSaveData(data, level);
            }
        }
    }
}
