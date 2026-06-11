namespace CindarsHope.Core.Events
{
    /// <summary>
    /// Publicado quando um canteiro de cultivo é regado.
    /// plotId identifica o canteiro de forma estável.
    /// </summary>
    public readonly struct CropWateredEvent
    {
        public readonly string PlotId;

        public CropWateredEvent(string plotId)
        {
            PlotId = plotId ?? string.Empty;
        }
    }
}
