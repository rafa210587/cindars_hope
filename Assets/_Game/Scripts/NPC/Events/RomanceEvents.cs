using CindarsHope.NPC.Friendship;

namespace CindarsHope.NPC.Events
{
    /// <summary>
    /// fable_46 — evento aditivo de mudança de estágio de romance (confissão e cada avanço posterior).
    /// Consumido por HUD/toast. Sem refs Unity/MonoBehaviour/SO (event-bus-only rule): só id + enum/int.
    /// </summary>
    public readonly struct RomanceStageChangedEvent
    {
        public readonly string NpcId;
        public readonly RomanceStage Stage;
        public readonly RomanceStage PreviousStage;

        public RomanceStageChangedEvent(string npcId, RomanceStage stage, RomanceStage previousStage)
        {
            NpcId = npcId;
            Stage = stage;
            PreviousStage = previousStage;
        }
    }

    /// <summary>
    /// fable_46 — evento aditivo de recusa de confissão (limite de 2 parceiros, gate não atendido, etc.).
    /// Carrega o motivo determinístico para um toast digno ("não posso me dividir mais"). Só id + enum.
    /// </summary>
    public readonly struct RomanceConfessionRejectedEvent
    {
        public readonly string NpcId;
        public readonly RomanceConfessionRejection Reason;

        public RomanceConfessionRejectedEvent(string npcId, RomanceConfessionRejection reason)
        {
            NpcId = npcId;
            Reason = reason;
        }
    }
}
