namespace CindarsHope.UI.Runtime
{
    /// <summary>
    /// fable_14 EMENDA V3 (decision 4.1, Stardew-style): a SINGLE pause gate that freezes
    /// the day clock whenever any modal/panel/dialogue/shop is open, and resumes (from the
    /// exact same point, no jump) when the stack empties.
    ///
    /// Pure C# (no UnityEngine) so the gate logic is EditMode-testable. The runtime adapter
    /// feeds it the live <c>ModalManager.HasActiveModal</c> each frame; the gate computes a
    /// single boolean "should the clock be frozen" and exposes rising/falling edges so the
    /// adapter only touches <c>GameTimeManager</c> on transitions.
    ///
    /// Both MenuManager and PauseMenuController are meant to feed this ONE gate instead of
    /// each pausing the clock independently (resolves the documented duplicate pause paths).
    /// </summary>
    public sealed class ModalPauseGate
    {
        /// <summary>True when the day clock should currently be frozen.</summary>
        public bool IsClockFrozen { get; private set; }

        /// <summary>
        /// Update the gate from the current "any modal active" signal.
        /// Returns a transition describing whether the adapter must freeze, resume, or do nothing.
        /// </summary>
        public PauseTransition Evaluate(bool anyModalActive)
        {
            if (anyModalActive == IsClockFrozen)
            {
                return PauseTransition.None;
            }

            IsClockFrozen = anyModalActive;
            return anyModalActive ? PauseTransition.Freeze : PauseTransition.Resume;
        }

        /// <summary>Force the gate back to the running state (e.g. on scene reset).</summary>
        public void Reset()
        {
            IsClockFrozen = false;
        }
    }

    public enum PauseTransition
    {
        None,
        Freeze,
        Resume
    }
}
