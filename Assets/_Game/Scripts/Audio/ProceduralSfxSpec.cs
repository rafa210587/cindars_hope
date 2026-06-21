using System.Collections.Generic;

namespace CindarsHope.Audio
{
    /// <summary>
    /// fable_58 — forma de onda PROCEDURAL (placeholder) de uma categoria de SFX.
    /// Parâmetros puros (sem AudioClip) → testáveis em EditMode. A
    /// <see cref="ProceduralSfxFactory"/> consome estes specs para gerar os clipes.
    ///
    /// Cada categoria/estado tem parâmetros distintos (frequência, duração, forma,
    /// sweep, número de notas) para que SOEM diferentes mesmo como beeps.
    /// </summary>
    public enum WaveShape
    {
        Sine = 0,
        Square = 1,
        Triangle = 2,
        Noise = 3
    }

    public readonly struct ProceduralSfxSpec
    {
        /// <summary>Frequência inicial (Hz).</summary>
        public float StartFrequency { get; }

        /// <summary>Frequência final (Hz) — diferente de Start para sweeps/quedas de pitch.</summary>
        public float EndFrequency { get; }

        /// <summary>Duração total do clipe (segundos). Curto por design (&lt; ~1.2s).</summary>
        public float DurationSeconds { get; }

        public WaveShape Shape { get; }

        /// <summary>Amplitude de pico [0,1].</summary>
        public float Amplitude { get; }

        /// <summary>
        /// Número de "notas" sequenciais (arpejo/acorde simples). 1 = tom único.
        /// Usado por levelup (3 notas), save (2 notas), status (blip duplo).
        /// </summary>
        public int NoteCount { get; }

        public ProceduralSfxSpec(float startFrequency, float endFrequency, float durationSeconds, WaveShape shape, float amplitude, int noteCount)
        {
            StartFrequency = startFrequency;
            EndFrequency = endFrequency;
            DurationSeconds = durationSeconds < 0.01f ? 0.01f : durationSeconds;
            Shape = shape;
            Amplitude = amplitude < 0f ? 0f : (amplitude > 1f ? 1f : amplitude);
            NoteCount = noteCount < 1 ? 1 : noteCount;
        }

        public bool IsValid => DurationSeconds > 0f && StartFrequency > 0f && Amplitude > 0f;
    }

    /// <summary>
    /// Frase melódica procedural placeholder: uma nota raiz + uma sequência de offsets em
    /// SEMITONS (12 = uma oitava) tocados em sequência e em loop como "faixa". Pura/testável
    /// (sem AudioClip). Em vez de um tom único, isto produz um ARPEJO — o estilo "prelúdio"
    /// de JRPG (Final Fantasy VII/VIII): acorde quebrado em cascata, em tom menor para o
    /// clima melancólico, tocado em sine suave. Continua placeholder até a trilha real (arte).
    /// </summary>
    public readonly struct MusicPhrase
    {
        /// <summary>Frequência da nota raiz (Hz) — semitom 0.</summary>
        public float RootFrequency { get; }

        /// <summary>Duração de cada nota (segundos).</summary>
        public float NoteSeconds { get; }

        /// <summary>Amplitude de pico [0,1] — baixa por design (música é fundo).</summary>
        public float Amplitude { get; }

        /// <summary>Sequência de offsets em semitons a partir da raiz (12 = oitava). Faz loop.</summary>
        public IReadOnlyList<int> Semitones { get; }

        public MusicPhrase(float rootFrequency, float noteSeconds, float amplitude, IReadOnlyList<int> semitones)
        {
            RootFrequency = rootFrequency;
            NoteSeconds = noteSeconds < 0.02f ? 0.02f : noteSeconds;
            Amplitude = amplitude < 0f ? 0f : (amplitude > 1f ? 1f : amplitude);
            Semitones = semitones;
        }

        public bool IsValid => RootFrequency > 0f && Amplitude > 0f && Semitones != null && Semitones.Count > 0;

        /// <summary>Frequência (Hz) da nota no índice: root × 2^(semitom/12).</summary>
        public float FrequencyAt(int index)
        {
            return RootFrequency * (float)System.Math.Pow(2.0, Semitones[index] / 12.0);
        }

        /// <summary>Duração total da frase (loop) em segundos.</summary>
        public float TotalSeconds => NoteSeconds * (Semitones?.Count ?? 0);
    }

    /// <summary>
    /// Tabela canônica de specs procedurais por categoria de SFX e por estado de música.
    /// É o "contrato de entrega" para a fase de arte (cada linha vira um asset real).
    /// </summary>
    public static class ProceduralSfxLibrary
    {
        /// <summary>Spec procedural placeholder para cada categoria de SFX (todas distintas).</summary>
        public static ProceduralSfxSpec ForSfx(SfxCategory category)
        {
            switch (category)
            {
                // hit = burst curto grave
                case SfxCategory.Hit:
                    return new ProceduralSfxSpec(180f, 110f, 0.09f, WaveShape.Square, 0.55f, 1);
                // perfect_block = ping agudo
                case SfxCategory.PerfectBlock:
                    return new ProceduralSfxSpec(1320f, 1320f, 0.16f, WaveShape.Sine, 0.6f, 1);
                // posture_break = queda de pitch
                case SfxCategory.PostureBreak:
                    return new ProceduralSfxSpec(640f, 180f, 0.30f, WaveShape.Triangle, 0.6f, 1);
                // charged = sweep ascendente
                case SfxCategory.Charged:
                    return new ProceduralSfxSpec(220f, 880f, 0.28f, WaveShape.Sine, 0.5f, 1);
                // status = blip duplo
                case SfxCategory.Status:
                    return new ProceduralSfxSpec(720f, 720f, 0.14f, WaveShape.Square, 0.45f, 2);
                // ui_toast = blip suave
                case SfxCategory.UiToast:
                    return new ProceduralSfxSpec(560f, 560f, 0.10f, WaveShape.Sine, 0.35f, 1);
                // pickup = blip curto
                case SfxCategory.Pickup:
                    return new ProceduralSfxSpec(880f, 990f, 0.08f, WaveShape.Triangle, 0.4f, 1);
                // levelup = arpejo 3 notas
                case SfxCategory.LevelUp:
                    return new ProceduralSfxSpec(523f, 784f, 0.45f, WaveShape.Sine, 0.55f, 3);
                // day_start = acorde suave
                case SfxCategory.DayStart:
                    return new ProceduralSfxSpec(392f, 523f, 0.40f, WaveShape.Sine, 0.4f, 2);
                // save = confirmação 2 notas
                case SfxCategory.Save:
                    return new ProceduralSfxSpec(660f, 880f, 0.22f, WaveShape.Triangle, 0.45f, 2);
                // block (normal) = thud médio (categoria reservada; consumidor futuro DamageBlockedEvent)
                case SfxCategory.Block:
                    return new ProceduralSfxSpec(300f, 240f, 0.10f, WaveShape.Square, 0.5f, 1);
                // enemy_killed = queda curta grave
                case SfxCategory.EnemyKilled:
                    return new ProceduralSfxSpec(420f, 160f, 0.18f, WaveShape.Triangle, 0.55f, 1);
                // craft = clique duplo médio
                case SfxCategory.Craft:
                    return new ProceduralSfxSpec(500f, 620f, 0.16f, WaveShape.Square, 0.45f, 2);
                // harvest = blip orgânico
                case SfxCategory.Harvest:
                    return new ProceduralSfxSpec(440f, 560f, 0.12f, WaveShape.Triangle, 0.45f, 1);
                // fish = splash (ruído curto)
                case SfxCategory.Fish:
                    return new ProceduralSfxSpec(300f, 300f, 0.16f, WaveShape.Noise, 0.4f, 1);
                default:
                    return default;
            }
        }

        /// <summary>
        /// Spec placeholder de "faixa" por estado de música — pensada como FUNDO SUAVE.
        /// Sempre <see cref="WaveShape.Sine"/> (onda mais macia, sem aspereza de
        /// square/triangle), amplitude baixa (fica embaixo dos SFX) e loops LONGOS com
        /// poucas notas, formando um arpejo lento e consonante em vez de um beep repetindo.
        /// Calmo: grave/sereno; Combate: leve movimento; Boss: bem grave e lento; Festival:
        /// claro mas gentil. O envelope por nota (ataque curto + decay longo) faz cada nota
        /// "respirar", soando ambiente — não um tom contínuo.
        /// </summary>
        public static ProceduralSfxSpec ForMusic(MusicState state)
        {
            switch (state)
            {
                // calmo = pad sereno em registro grave, arpejo lento de 4 notas
                case MusicState.Calmo:
                    return new ProceduralSfxSpec(220f, 220f, 6.0f, WaveShape.Sine, 0.13f, 4);
                // combate = leve subida/movimento, ainda macio (sem triangle)
                case MusicState.Combate:
                    return new ProceduralSfxSpec(196f, 247f, 4.0f, WaveShape.Sine, 0.16f, 4);
                // boss = muito grave e lento, ominoso mas SEM square (sem zumbido)
                case MusicState.Boss:
                    return new ProceduralSfxSpec(110f, 110f, 8.0f, WaveShape.Sine, 0.15f, 3);
                // festival = arpejo claro e gentil de 5 notas
                case MusicState.Festival:
                    return new ProceduralSfxSpec(294f, 392f, 4.5f, WaveShape.Sine, 0.14f, 5);
                default:
                    return default;
            }
        }

        /// <summary>
        /// Frase melódica (arpejo) por estado de música — estilo "prelúdio" de JRPG
        /// (Final Fantasy VII/VIII). Calmo/Combate em Lá menor (melancólico), Boss grave e
        /// ominoso, Festival em Dó maior (alegre). É o que o <see cref="ProceduralSfxFactory"/>
        /// usa de fato para a música; <see cref="ForMusic"/> fica como spec legada de tom único.
        /// </summary>
        public static MusicPhrase MusicPhraseFor(MusicState state)
        {
            switch (state)
            {
                // calmo = arpejo de Lá menor em cascata (estilo "Prelude"), lento e macio
                case MusicState.Calmo:
                    return new MusicPhrase(220f, 0.50f, 0.14f,
                        new[] { 0, 3, 7, 12, 7, 3, 0, 7, 10, 15, 10, 7 });
                // combate = Lá menor com tensão (b6 = 8) e notas mais rápidas
                case MusicState.Combate:
                    return new MusicPhrase(220f, 0.26f, 0.16f,
                        new[] { 0, 3, 7, 10, 7, 3, 5, 8, 5, 3, 0, -2 });
                // boss = registro grave, lento e ominoso (b2 = 1 cria tensão), ainda sine
                case MusicState.Boss:
                    return new MusicPhrase(110f, 1.10f, 0.15f,
                        new[] { 0, 1, 0, 3, 2, 0 });
                // festival = Dó maior, arpejo claro e animado
                case MusicState.Festival:
                    return new MusicPhrase(262f, 0.24f, 0.14f,
                        new[] { 0, 4, 7, 12, 7, 4, 9, 5, 0, 4, 7, 12 });
                default:
                    return default;
            }
        }

        /// <summary>Todas as categorias de SFX que têm placeholder (exclui None).</summary>
        public static IEnumerable<SfxCategory> AllSfxCategories()
        {
            yield return SfxCategory.Hit;
            yield return SfxCategory.PerfectBlock;
            yield return SfxCategory.PostureBreak;
            yield return SfxCategory.Charged;
            yield return SfxCategory.Status;
            yield return SfxCategory.UiToast;
            yield return SfxCategory.Pickup;
            yield return SfxCategory.LevelUp;
            yield return SfxCategory.DayStart;
            yield return SfxCategory.Save;
            yield return SfxCategory.Block;
            yield return SfxCategory.EnemyKilled;
            yield return SfxCategory.Craft;
            yield return SfxCategory.Harvest;
            yield return SfxCategory.Fish;
        }

        /// <summary>Todos os estados de música.</summary>
        public static IEnumerable<MusicState> AllMusicStates()
        {
            yield return MusicState.Calmo;
            yield return MusicState.Combate;
            yield return MusicState.Boss;
            yield return MusicState.Festival;
        }
    }
}
