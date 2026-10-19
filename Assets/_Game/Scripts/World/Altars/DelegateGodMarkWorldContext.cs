using System;

namespace CindarsHope.World.Altars
{
    /// <summary>
    /// fable_68 — implementação de <see cref="IGodMarkWorldContext"/> baseada em delegates injetáveis.
    /// Mantém o <see cref="GodMarkService"/> desacoplado das fontes concretas (TimeManager, calendário,
    /// inventário, equipamento): o dono de runtime (bootstrap) liga cada delegate ao seu ponto único de
    /// leitura existente (ex.: <c>GameTimeManager.CurrentPhase == Night</c>, <c>WorldEventResolver</c>,
    /// <c>InventoryManager.RemoveItem</c>, deus da relíquia via <c>AccessoryCatalog</c>). Testável: os
    /// testes injetam funções determinísticas. Nenhum delegate ⇒ valor neutro (sem condição satisfeita).
    /// </summary>
    public sealed class DelegateGodMarkWorldContext : IGodMarkWorldContext
    {
        public Func<int> DayNumberSource;
        public Func<bool> IsNightSource;
        public Func<bool> IsAlihanaLunarPeakSource;
        public Func<bool> IsSenyaFestivalOrPeakSource;
        public Func<bool> HasCropOfferingSource;
        public Func<bool> ConsumeCropOfferingSource;
        public Func<MarkGod> EquippedRelicGodSource;

        public int CurrentDayNumber => DayNumberSource?.Invoke() ?? 0;
        public bool IsNight => IsNightSource?.Invoke() ?? false;
        public bool IsAlihanaLunarPeak => IsAlihanaLunarPeakSource?.Invoke() ?? false;
        public bool IsSenyaFestivalOrPeak => IsSenyaFestivalOrPeakSource?.Invoke() ?? false;
        public bool HasCropOffering => HasCropOfferingSource?.Invoke() ?? false;
        public bool TryConsumeCropOffering() => ConsumeCropOfferingSource?.Invoke() ?? false;
        public MarkGod EquippedRelicGod => EquippedRelicGodSource?.Invoke() ?? MarkGod.None;
    }
}
