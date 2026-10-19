namespace CindarsHope.Save
{
    /// <summary>
    /// Contract for save provider adapters. Enables SaveManager to delegate capture/restore
    /// of specific domains without requiring all managers to be wired to SaveManager directly.
    /// SPEC_10: Incremental refactor to reduce SaveManager coupling.
    /// </summary>
    public interface ISaveSectionProvider
    {
        /// <summary>
        /// Unique identifier for this provider (e.g., "hotbar", "bestiary").
        /// Used in logging and fallback decisions.
        /// </summary>
        string ProviderId { get; }

        /// <summary>
        /// Capture domain-specific save data.
        /// May use existing runtime manager state or fallback to provided GameSaveData.
        /// </summary>
        /// <param name="existingSaveData">Existing GameSaveData for fallback (may be null on new game).</param>
        /// <returns>Domain-specific DTO matching a GameSaveData public field, or null to skip.</returns>
        object Capture(GameSaveData existingSaveData);

        /// <summary>
        /// Restore domain state from captured data.
        /// Must have null guard for null sectionData (provider was skipped).
        /// </summary>
        /// <param name="sectionData">Domain-specific DTO from GameSaveData, may be null.</param>
        void Restore(object sectionData);
    }
}
