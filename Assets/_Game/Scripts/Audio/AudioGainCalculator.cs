namespace CindarsHope.Audio
{
    /// <summary>
    /// fable_58 — cálculo PURO do ganho aplicado (linear 0..1) a partir dos volumes
    /// 0..100 (Master + canal). Testável sem AudioSource.
    ///
    /// Ganho efetivo = (Master/100) × (Canal/100), com clamp defensivo em [0,100]
    /// para nunca produzir ganho fora de faixa nem exceção.
    ///
    /// Defaults locais (usados enquanto F56/GameAudioSettings não existe):
    ///   Master 80, SFX 60 (conservador — evita beep-metralhadora), Music 70.
    /// </summary>
    public static class AudioGainCalculator
    {
        public const int DefaultMasterVolume = 80;
        public const int DefaultSfxVolume = 60;
        public const int DefaultMusicVolume = 70;

        public const int MinVolume = 0;
        public const int MaxVolume = 100;

        /// <summary>Garante um volume inteiro em [0,100].</summary>
        public static int ClampVolume(int volume)
        {
            if (volume < MinVolume)
            {
                return MinVolume;
            }

            if (volume > MaxVolume)
            {
                return MaxVolume;
            }

            return volume;
        }

        /// <summary>
        /// Ganho linear [0,1] = (master/100) × (channel/100), com clamp em ambos.
        /// </summary>
        public static float ComputeGain(int masterVolume, int channelVolume)
        {
            float master = ClampVolume(masterVolume) / 100f;
            float channel = ClampVolume(channelVolume) / 100f;
            return master * channel;
        }
    }
}
