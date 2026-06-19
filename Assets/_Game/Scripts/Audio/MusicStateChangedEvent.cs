namespace CindarsHope.Audio
{
    /// <summary>
    /// fable_58 EMENDA V3 (decisão 4.9) — ÚNICO evento próprio publicado por esta spec.
    ///
    /// Exceção explícita e autorizada pela EMENDA V3 à cláusula "Adds events: NO":
    /// quando o MusicState muda, o áudio publica esta transição interna. O payload é
    /// simples (dois enums, sem refs Unity), e NENHUM sistema de gameplay assina este
    /// evento — ele é interno ao domínio de áudio (o crossfade nasce SOMENTE daqui).
    ///
    /// Não existe segundo caminho de troca de faixa: a mudança de faixa-placeholder
    /// é disparada exclusivamente por esta transição de estado.
    /// </summary>
    public readonly struct MusicStateChangedEvent
    {
        public MusicState Previous { get; }
        public MusicState Next { get; }

        public MusicStateChangedEvent(MusicState previous, MusicState next)
        {
            Previous = previous;
            Next = next;
        }
    }
}
