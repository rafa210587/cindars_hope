using System.Collections.Generic;

namespace CindarsHope.Foundation
{
    /// <summary>
    /// Port puro para persistir hints de onboarding sem acoplar Save ao módulo UI.
    /// </summary>
    public interface IOnboardingHintsRuntime
    {
        IReadOnlyList<string> GetSeenHintIds();
        void RestoreSeenHintIds(IReadOnlyList<string> seenHintIds);
    }
}
