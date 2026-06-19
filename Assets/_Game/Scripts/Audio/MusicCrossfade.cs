namespace CindarsHope.Audio
{
    /// <summary>
    /// fable_58 EMENDA V3 — lógica PURA do crossfade de música (curva/duração).
    /// Sem AudioSource: dado o tempo decorrido na transição, devolve os ganhos
    /// normalizados [0,1] das faixas que SAI (out) e ENTRA (in). O AudioManager
    /// multiplica esses fatores pelo ganho do canal Music.
    ///
    /// Janela curta (~0,5–1,0s). Curva linear cruzada (out = 1-t, in = t), com
    /// soma de energia controlada — simples e suficiente para placeholders.
    /// Nunca lança; clampa t em [0,1].
    /// </summary>
    public static class MusicCrossfade
    {
        public const float DefaultDurationSeconds = 0.6f;
        public const float MinDurationSeconds = 0.05f;

        /// <summary>Progresso normalizado [0,1] dado o tempo decorrido e a duração total.</summary>
        public static float Progress(float elapsedSeconds, float durationSeconds)
        {
            if (durationSeconds <= MinDurationSeconds)
            {
                return 1f;
            }

            float t = elapsedSeconds / durationSeconds;
            if (t < 0f)
            {
                return 0f;
            }

            return t > 1f ? 1f : t;
        }

        /// <summary>Fator de ganho [0,1] da faixa que está SAINDO no progresso t.</summary>
        public static float FadeOutFactor(float progress)
        {
            float t = Clamp01(progress);
            return 1f - t;
        }

        /// <summary>Fator de ganho [0,1] da faixa que está ENTRANDO no progresso t.</summary>
        public static float FadeInFactor(float progress)
        {
            return Clamp01(progress);
        }

        /// <summary>Verdadeiro quando a transição terminou (faixa nova no volume cheio).</summary>
        public static bool IsComplete(float progress)
        {
            return Clamp01(progress) >= 1f;
        }

        private static float Clamp01(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }

            return value > 1f ? 1f : value;
        }
    }
}
