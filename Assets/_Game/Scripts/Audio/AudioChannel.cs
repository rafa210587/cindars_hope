namespace CindarsHope.Audio
{
    /// <summary>
    /// fable_58 — canais de áudio do AudioManager.
    /// Sfx = efeitos curtos (pool de vozes round-robin).
    /// Music = trilha de estado com crossfade (dois AudioSources em reuso).
    /// </summary>
    public enum AudioChannel
    {
        Sfx = 0,
        Music = 1
    }
}
