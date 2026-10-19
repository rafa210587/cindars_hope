namespace CindarsHope.Core.Events
{
    /// <summary>
    /// fable_68 — publicado quando o jogador ora numa Marca dos Deuses e o resultado é resolvido
    /// (concedido, recusado, ou lore). Consumido por UI/toast/telemetria via GameEventBus (nenhuma
    /// chamada direta MonoBehaviour↔MonoBehaviour — rule unity-architecture §2). <see cref="ResultCode"/>
    /// é (int)GodMarkPrayResult para manter Core sem dependência de World.
    /// </summary>
    public readonly struct GodMarkPrayedEvent
    {
        public readonly string MarkId;
        public readonly int God;        // (int)MarkGod
        public readonly int ResultCode; // (int)GodMarkPrayResult
        public readonly int EffectType; // (int)MarkEffectType (None se recusado/lore)
        public readonly float Magnitude;

        public GodMarkPrayedEvent(string markId, int god, int resultCode, int effectType, float magnitude)
        {
            MarkId = markId ?? string.Empty;
            God = god;
            ResultCode = resultCode;
            EffectType = effectType;
            Magnitude = magnitude;
        }
    }
}
