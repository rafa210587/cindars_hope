using CindarsHope.Magic;

namespace CindarsHope.Save.Providers
{
    /// <summary>
    /// fable_07 — provider de save do grimório, no padrão de HotbarSectionProvider (ISaveSectionProvider).
    /// Fonte do estado: <see cref="PlayerSpellbook.Instance"/> (singleton runtime, padrão F13/F17).
    /// Fallback: preserva a seção existente do save quando o grimório ainda não foi inicializado.
    /// Default (sem instância e sem save): grimório vazio (saves legados OK, sem migration).
    /// </summary>
    public class SpellbookSectionProvider : ISaveSectionProvider
    {
        public string ProviderId => "spellbook";

        public object Capture(GameSaveData existingSaveData)
        {
            var spellbook = PlayerSpellbook.Instance;
            if (spellbook == null)
            {
                // Grimório não inicializado: preserva o que já estava no save.
                return existingSaveData?.Spellbook ?? new SpellbookSaveData();
            }

            return spellbook.CaptureSaveData();
        }

        public void Restore(object sectionData)
        {
            var spellbook = PlayerSpellbook.Instance;
            if (spellbook == null)
            {
                return;
            }

            // sectionData null (seção ausente em save legado) = grimório vazio.
            spellbook.RestoreFromSaveData(sectionData as SpellbookSaveData);
        }
    }
}
