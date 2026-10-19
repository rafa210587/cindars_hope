using System;
using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Audio
{
    /// <summary>
    /// fable_58 — gera os clipes PLACEHOLDER procedurais (AudioClip.Create) a partir
    /// dos <see cref="ProceduralSfxSpec"/>. Clipes cacheados (gerados 1× no boot).
    ///
    /// Os clipes são monofônicos, 2D (sem spatial), curtos. A geração de samples é
    /// PURA (<see cref="FillSamples"/>) e testável sem áudio. Nunca lança: spec
    /// inválida devolve null e quem chama trata como silêncio.
    /// </summary>
    public sealed class ProceduralSfxFactory
    {
        public const int SampleRate = 44100;

        private readonly Dictionary<SfxCategory, AudioClip> _sfxCache = new Dictionary<SfxCategory, AudioClip>();
        private readonly Dictionary<MusicState, AudioClip> _musicCache = new Dictionary<MusicState, AudioClip>();

        // Guard one-shot ESTÁTICO: AudioClip.Create pode falhar em ambientes sem
        // suporte de áudio (CI/headless). BuildClip roda 1× por categoria (cacheado),
        // então sem este guard o boot logaria N warnings (1 por categoria). Estático
        // garante NO MÁXIMO 1 log no processo inteiro. Nunca reseta.
        private static bool s_clipCreateFailureLogged;

        /// <summary>Gera (1×) e cacheia todos os clipes de SFX e de música placeholder.</summary>
        public void GenerateAll()
        {
            foreach (var category in ProceduralSfxLibrary.AllSfxCategories())
            {
                GetSfxClip(category);
            }

            foreach (var state in ProceduralSfxLibrary.AllMusicStates())
            {
                GetMusicClip(state);
            }
        }

        /// <summary>Clipe de SFX para a categoria (cacheado). Null = silêncio (sem clipe).</summary>
        public AudioClip GetSfxClip(SfxCategory category)
        {
            if (category == SfxCategory.None)
            {
                return null;
            }

            if (_sfxCache.TryGetValue(category, out var cached))
            {
                return cached;
            }

            var clip = BuildClip($"sfx_proc_{SfxCategoryIds.ToStableId(category)}", ProceduralSfxLibrary.ForSfx(category));
            _sfxCache[category] = clip;
            return clip;
        }

        /// <summary>Clipe de "faixa" placeholder para o estado de música (cacheado).</summary>
        public AudioClip GetMusicClip(MusicState state)
        {
            if (_musicCache.TryGetValue(state, out var cached))
            {
                return cached;
            }

            var clip = BuildMusicClip($"music_proc_{state}", ProceduralSfxLibrary.MusicPhraseFor(state));
            _musicCache[state] = clip;
            return clip;
        }

        /// <summary>Gera o clipe de música (faixa em loop) a partir de uma frase melódica.</summary>
        private static AudioClip BuildMusicClip(string name, MusicPhrase phrase)
        {
            if (!phrase.IsValid)
            {
                return null;
            }

            int sampleCount = Mathf.Max(1, Mathf.RoundToInt(phrase.TotalSeconds * SampleRate));
            var samples = new float[sampleCount];
            FillMusicSamples(samples, phrase, SampleRate);

            try
            {
                var clip = AudioClip.Create(name, sampleCount, 1, SampleRate, false);
                clip.SetData(samples, 0);
                return clip;
            }
            catch (Exception exception)
            {
                if (!s_clipCreateFailureLogged)
                {
                    s_clipCreateFailureLogged = true;
                    Debug.LogWarning($"[Audio] AudioClip.Create indisponivel neste ambiente; SFX em silencio. (logado 1x) {exception.Message}");
                }
                return null;
            }
        }

        public int SfxCacheCount => _sfxCache.Count;
        public int MusicCacheCount => _musicCache.Count;

        private static AudioClip BuildClip(string name, ProceduralSfxSpec spec)
        {
            if (!spec.IsValid)
            {
                return null;
            }

            int sampleCount = Mathf.Max(1, Mathf.RoundToInt(spec.DurationSeconds * SampleRate));
            var samples = new float[sampleCount];
            FillSamples(samples, spec, SampleRate);

            try
            {
                var clip = AudioClip.Create(name, sampleCount, 1, SampleRate, false);
                clip.SetData(samples, 0);
                return clip;
            }
            catch (Exception exception)
            {
                // Fallback silencioso: nunca quebrar boot por causa de placeholder.
                // Loga NO MÁXIMO 1× no processo inteiro (guard estático) para não
                // inundar o Console com 1 warning por categoria que falhar.
                if (!s_clipCreateFailureLogged)
                {
                    s_clipCreateFailureLogged = true;
                    Debug.LogWarning(
                        $"[Audio] AudioClip.Create indisponivel neste ambiente; SFX em silencio. (logado 1x) {exception.Message}");
                }

                return null;
            }
        }

        /// <summary>
        /// Preenche o buffer com a forma de onda PURA do spec (testável sem AudioClip).
        /// Aplica envelope ataque/decay curto para evitar clicks, sweep de pitch e
        /// divisão em notas (arpejo/blip duplo) quando NoteCount &gt; 1.
        /// </summary>
        public static void FillSamples(float[] samples, ProceduralSfxSpec spec, int sampleRate)
        {
            if (samples == null || samples.Length == 0 || sampleRate <= 0)
            {
                return;
            }

            int total = samples.Length;
            int notes = spec.NoteCount;
            int samplesPerNote = Mathf.Max(1, total / notes);

            // Semente determinística por spec para o ruído (mesmo clipe sempre).
            var rng = new System.Random(
                unchecked((int)(spec.StartFrequency * 7f) ^ (int)(spec.EndFrequency * 13f) ^ (int)(spec.DurationSeconds * 1000f) ^ ((int)spec.Shape << 8) ^ (notes << 16)));

            for (int i = 0; i < total; i++)
            {
                int noteIndex = Mathf.Min(notes - 1, i / samplesPerNote);
                int noteStart = noteIndex * samplesPerNote;
                int noteLen = (noteIndex == notes - 1) ? (total - noteStart) : samplesPerNote;
                if (noteLen <= 0)
                {
                    noteLen = 1;
                }

                int localIndex = i - noteStart;
                float notePhase = (float)localIndex / noteLen; // 0..1 dentro da nota

                // Pitch: sweep global de Start→End, com pequenos degraus por nota (arpejo).
                float globalT = (float)i / total;
                float baseFreq = Mathf.Lerp(spec.StartFrequency, spec.EndFrequency, globalT);
                float noteMultiplier = notes > 1 ? 1f + (0.18f * noteIndex) : 1f;
                float freq = baseFreq * noteMultiplier;

                float time = (float)i / sampleRate;
                float wave = Oscillator(spec.Shape, freq, time, rng);

                // Envelope: ataque 10% + decay até o fim da nota (evita clicks/cliques).
                float attack = 0.1f;
                float env;
                if (notePhase < attack)
                {
                    env = notePhase / attack;
                }
                else
                {
                    env = 1f - ((notePhase - attack) / (1f - attack));
                }

                if (env < 0f)
                {
                    env = 0f;
                }

                samples[i] = wave * spec.Amplitude * env;
            }
        }

        private static float Oscillator(WaveShape shape, float frequency, float time, System.Random rng)
        {
            float phase = frequency * time;
            switch (shape)
            {
                case WaveShape.Sine:
                    return Mathf.Sin(2f * Mathf.PI * phase);
                case WaveShape.Square:
                    return Mathf.Sin(2f * Mathf.PI * phase) >= 0f ? 1f : -1f;
                case WaveShape.Triangle:
                    float frac = phase - Mathf.Floor(phase);
                    return 4f * Mathf.Abs(frac - 0.5f) - 1f;
                case WaveShape.Noise:
                    return (float)(rng.NextDouble() * 2.0 - 1.0);
                default:
                    return 0f;
            }
        }

        /// <summary>
        /// Preenche o buffer com a frase melódica (<see cref="MusicPhrase"/>): cada nota é
        /// um sine puro em sequência, com envelope macio (ataque curto, sustain, release
        /// suave) para soar legato/ambiente — não um beep. Fase reinicia por nota (sin(0)=0)
        /// evitando clicks; um fade curto nas bordas suaviza o ponto de loop. Pura/testável.
        /// </summary>
        public static void FillMusicSamples(float[] samples, MusicPhrase phrase, int sampleRate)
        {
            if (samples == null || samples.Length == 0 || sampleRate <= 0 || !phrase.IsValid)
            {
                return;
            }

            int noteCount = phrase.Semitones.Count;
            int total = samples.Length;
            int samplesPerNote = Mathf.Max(1, total / noteCount);

            const float attack = 0.08f;       // 8% de ataque
            const float releaseStart = 0.68f; // release nos últimos 32%

            for (int n = 0; n < noteCount; n++)
            {
                float freq = phrase.FrequencyAt(n);
                int start = n * samplesPerNote;
                int len = (n == noteCount - 1) ? (total - start) : samplesPerNote;
                if (len <= 0)
                {
                    continue;
                }

                for (int i = 0; i < len; i++)
                {
                    int gi = start + i;
                    if (gi >= total)
                    {
                        break;
                    }

                    float t = (float)i / sampleRate; // tempo LOCAL à nota (fase começa em 0)
                    // fundamental + harmônico de oitava (0.3) = timbre mais quente, menos "beep".
                    float wave = Mathf.Sin(2f * Mathf.PI * freq * t)
                               + 0.3f * Mathf.Sin(2f * Mathf.PI * (freq * 2f) * t);

                    float p = (float)i / len; // 0..1 dentro da nota
                    float env;
                    if (p < attack)
                    {
                        env = p / attack;
                    }
                    else if (p > releaseStart)
                    {
                        env = 1f - ((p - releaseStart) / (1f - releaseStart));
                    }
                    else
                    {
                        env = 1f;
                    }

                    if (env < 0f)
                    {
                        env = 0f;
                    }

                    samples[gi] = wave * phrase.Amplitude * env;
                }
            }

            // Voz de BAIXO sustentada (drone) por baixo do arpejo: sine grave contínuo com
            // swell lento sobre o loop inteiro — dá o leito de "pad" que encorpa a faixa.
            if (phrase.HasBass)
            {
                float bassFreq = phrase.BassFrequency;
                for (int i = 0; i < total; i++)
                {
                    float t = (float)i / sampleRate;
                    float wave = Mathf.Sin(2f * Mathf.PI * bassFreq * t);

                    float p = (float)i / total; // 0..1 no loop inteiro
                    float env = 1f;
                    if (p < 0.12f)
                    {
                        env = p / 0.12f;
                    }
                    else if (p > 0.88f)
                    {
                        env = (1f - p) / 0.12f;
                    }
                    if (env < 0f)
                    {
                        env = 0f;
                    }

                    samples[i] += wave * phrase.BassAmplitude * env;
                }
            }

            // Fade curto (~50ms) nas bordas para suavizar o ponto de loop da faixa.
            int fade = Mathf.Min(total / 20, sampleRate / 20);
            for (int i = 0; i < fade; i++)
            {
                float g = (float)i / fade;
                samples[i] *= g;
                samples[total - 1 - i] *= g;
            }
        }
    }
}
