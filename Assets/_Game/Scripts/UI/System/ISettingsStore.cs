namespace CindarsHope.UI.SystemTab
{
    /// <summary>
    /// fable_56: armazenamento de UI state (volumes / video), abstraido para testar a logica
    /// de persistencia sem PlayerPrefs real. O adapter de runtime usa PlayerPrefs; os testes
    /// usam um store em memoria. UI state e separado do GameSaveData (SAVE_LOAD §19): NUNCA
    /// entra no schema de save.
    /// </summary>
    public interface ISettingsStore
    {
        int GetInt(string key, int defaultValue);
        void SetInt(string key, int value);
        void Save();
    }
}
