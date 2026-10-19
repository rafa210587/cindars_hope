using UnityEngine;

namespace CindarsHope.Core.Events
{
    /// <summary>
    /// fable_60 — uma armadilha disparou (após telegraph). HUD/toast/F58 (SFX) escutam. O dano/status
    /// em si entram pelos eventos EXISTENTES (PlayerDamagedEvent/StatusEffectAppliedEvent); este é o
    /// anúncio do gatilho. TrapKey é a chave canônica (ex.: "trap_spike_floor").
    /// </summary>
    public sealed class TrapTriggeredEvent
    {
        public string TrapInstanceId;
        public string TrapKey;
        public Vector2 Position;
        public int CaveLevel;

        public TrapTriggeredEvent(string trapInstanceId, string trapKey, Vector2 position, int caveLevel)
        {
            TrapInstanceId = trapInstanceId ?? string.Empty;
            TrapKey = trapKey ?? string.Empty;
            Position = position;
            CaveLevel = caveLevel;
        }
    }

    /// <summary>
    /// fable_60 — uma armadilha foi desarmada por interação (estado terminal Disarmed). HUD/toast/F58.
    /// </summary>
    public sealed class TrapDisarmedEvent
    {
        public string TrapInstanceId;
        public string TrapKey;
        public Vector2 Position;
        public int CaveLevel;

        public TrapDisarmedEvent(string trapInstanceId, string trapKey, Vector2 position, int caveLevel)
        {
            TrapInstanceId = trapInstanceId ?? string.Empty;
            TrapKey = trapKey ?? string.Empty;
            Position = position;
            CaveLevel = caveLevel;
        }
    }

    /// <summary>
    /// fable_60 — o efeito de detecção (amuleto de Nyx, F23) revelou uma armadilha num raio. Consumidor
    /// real da flag dormante: sem o efeito, este evento nunca é publicado. HUD mostra o aviso à distância.
    /// </summary>
    public sealed class TrapDetectedEvent
    {
        public string TrapInstanceId;
        public string TrapKey;
        public Vector2 Position;
        public int CaveLevel;

        public TrapDetectedEvent(string trapInstanceId, string trapKey, Vector2 position, int caveLevel)
        {
            TrapInstanceId = trapInstanceId ?? string.Empty;
            TrapKey = trapKey ?? string.Empty;
            Position = position;
            CaveLevel = caveLevel;
        }
    }
}
