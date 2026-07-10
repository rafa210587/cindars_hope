namespace CindarsHope.Foundation
{
    /// <summary>
    /// Core-side port for the crafting runtime lifecycle. Keeps GameBootstrap from depending on
    /// the Craft module while preserving the existing initialization/shutdown sequence.
    /// </summary>
    public interface ICraftingRuntimeManager
    {
        bool IsInitialized { get; }
        void Initialize();
        void Shutdown();
    }
}
