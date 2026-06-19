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

            var clip = BuildClip($"music_proc_{state}", ProceduralSfxLibrary.ForMusic(state));
            _musicCache[state] = clip;
            return clip;
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
                Debug.LogWarning($"[Audio] ProceduralSfxFactory: falha ao criar clipe '{name}'. Categoria ficará em silêncio. {exception.Message}");
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
    }
}
