namespace CindarsHope.Foundation
{
    /// <summary>
    /// Port puro para consumidores de cena perguntarem se um NPC rastreado está disponível,
    /// sem referenciar o runtime concreto de agenda do módulo NPC.
    /// </summary>
    public interface INpcScheduleAvailabilityRuntime
    {
        bool TryGetTrackedAvailability(string npcId, out bool isAvailable);
    }
}
