using UnityEngine;

namespace CindarsHope.UI.SystemTab
{
    /// <summary>
    /// fable_56: adapter de runtime do <see cref="ISettingsStore"/> sobre PlayerPrefs. UI state
    /// (volumes/video) fica fora do GameSaveData (SAVE_LOAD §19). Unico ponto que toca
    /// PlayerPrefs para essas chaves.
    /// </summary>
    public sealed class PlayerPrefsSettingsStore : ISettingsStore
    {
        public int GetInt(string key, int defaultValue) => PlayerPrefs.GetInt(key, defaultValue);

        public void SetInt(string key, int value) => PlayerPrefs.SetInt(key, value);

        public void Save() => PlayerPrefs.Save();
    }
}
