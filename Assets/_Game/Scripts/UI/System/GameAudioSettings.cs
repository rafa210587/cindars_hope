namespace CindarsHope.UI.SystemTab
{
    /// <summary>
    /// fable_56 (EMENDA v3 4.4): UI state de audio e video da aba Sistema, persistido FORA do
    /// GameSaveData (SAVE_LOAD §19). Expoe TRES volumes (Master / SFX / Music) para a F58
    /// (AudioManager) consumir por canal, alem de modo de tela (fullscreen) e indice de
    /// resolucao. Logica pura testavel; o store (PlayerPrefs) e injetado.
    ///
    /// Sem dificuldade (ADR-0014), sem rebind (ADR-0013), sem idioma no v1.
    /// </summary>
    public static class GameAudioSettings
    {
        public const string MasterVolumeKey = "ch_volume_master";
        public const string SfxVolumeKey = "ch_volume_sfx";
        public const string MusicVolumeKey = "ch_volume_music";
        public const string FullscreenKey = "ch_video_fullscreen";
        public const string ResolutionIndexKey = "ch_video_resolution_index";

        // Defaults conservadores alinhados ao AudioGainCalculator (F58).
        public const int DefaultMasterVolume = 80;
        public const int DefaultSfxVolume = 60;
        public const int DefaultMusicVolume = 70;

        public const int MinVolume = 0;
        public const int MaxVolume = 100;

        private static ISettingsStore _store;

        /// <summary>Liga o store concreto (PlayerPrefs em runtime; em memoria nos testes).</summary>
        public static void Bind(ISettingsStore store)
        {
            _store = store;
        }

        public static bool IsBound => _store != null;

        public static int MasterVolume
        {
            get => Read(MasterVolumeKey, DefaultMasterVolume);
            set => Write(MasterVolumeKey, Clamp(value));
        }

        public static int SfxVolume
        {
            get => Read(SfxVolumeKey, DefaultSfxVolume);
            set => Write(SfxVolumeKey, Clamp(value));
        }

        public static int MusicVolume
        {
            get => Read(MusicVolumeKey, DefaultMusicVolume);
            set => Write(MusicVolumeKey, Clamp(value));
        }

        public static bool Fullscreen
        {
            get => Read(FullscreenKey, 1) != 0;
            set => Write(FullscreenKey, value ? 1 : 0);
        }

        public static int ResolutionIndex
        {
            get => Read(ResolutionIndexKey, 0);
            set => Write(ResolutionIndexKey, value < 0 ? 0 : value);
        }

        public static int Clamp(int volume)
        {
            if (volume < MinVolume)
            {
                return MinVolume;
            }

            return volume > MaxVolume ? MaxVolume : volume;
        }

        private static int Read(string key, int defaultValue)
        {
            return _store != null ? _store.GetInt(key, defaultValue) : defaultValue;
        }

        private static void Write(string key, int value)
        {
            if (_store == null)
            {
                return;
            }

            _store.SetInt(key, value);
            _store.Save();
        }
    }
}
