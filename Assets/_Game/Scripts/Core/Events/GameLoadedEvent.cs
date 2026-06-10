namespace CindarsHope.Core.Events
{
    /// <summary>
    /// Publicado após uma tentativa de load.
    /// WasSuccessful=false quando arquivo não existe, corrompido ou schema incompatível.
    /// </summary>
    public readonly struct GameLoadedEvent
    {
        public int Slot { get; }
        public string SavePath { get; }
        public bool WasSuccessful { get; }
        public string Message { get; }

        public GameLoadedEvent(int slot, string savePath, bool wasSuccessful, string message)
        {
            Slot = slot;
            SavePath = savePath;
            WasSuccessful = wasSuccessful;
            Message = message;
        }
    }
}
