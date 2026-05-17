namespace CindarsHope.Core.Events
{
    /// <summary>
    /// Publicado após uma tentativa de save.
    /// SavePath pode ser usado para debug/log, mas UI normalmente só precisa de WasSuccessful.
    /// </summary>
    public readonly struct GameSavedEvent
    {
        public int Slot { get; }
        public string SavePath { get; }
        public bool WasSuccessful { get; }
        public string Message { get; }

        public GameSavedEvent(int slot, string savePath, bool wasSuccessful, string message)
        {
            Slot = slot;
            SavePath = savePath;
            WasSuccessful = wasSuccessful;
            Message = message;
        }
    }
}
