using System.Collections.Generic;

namespace CindarsHope.UI.Runtime
{
    /// <summary>
    /// fable_14: Deterministic linear keyboard focus order for a Canvas screen.
    ///
    /// Pure C# (no UnityEngine dependency) so it is EditMode-testable in isolation.
    /// A ScreenView feeds it the ordered list of focusable element ids; the controller
    /// tracks the focused index and resolves arrow/Tab/Enter/Esc navigation.
    ///
    /// Wrapping is linear (Stardew-style menus): Down/Right/Tab advances and wraps to the
    /// first element; Up/Left/ShiftTab retreats and wraps to the last element. This gives
    /// the "focus order navigable by keyboard" + "visual outline" requirement (CA-2).
    /// </summary>
    public sealed class UiFocusController
    {
        public enum FocusInput
        {
            None,
            Next,       // Down / Right / Tab
            Previous,   // Up / Left / Shift+Tab
            Confirm,    // Enter / Space / E
            Cancel      // Esc / Back
        }

        public enum FocusResult
        {
            Moved,
            Confirmed,
            Cancelled,
            NoOp
        }

        private readonly List<string> _elements = new();
        private int _focusedIndex = -1;

        /// <summary>Ordered focusable element ids currently registered.</summary>
        public IReadOnlyList<string> Elements => _elements;

        public int Count => _elements.Count;

        public int FocusedIndex => _focusedIndex;

        public bool HasFocus => _focusedIndex >= 0 && _focusedIndex < _elements.Count;

        /// <summary>Id of the focused element, or null when nothing is focusable.</summary>
        public string FocusedElementId => HasFocus ? _elements[_focusedIndex] : null;

        /// <summary>
        /// Replace the ordered focus list. Focus snaps to the first element (index 0)
        /// when at least one element exists, otherwise to -1 (no focus / empty state).
        /// </summary>
        public void SetElements(IReadOnlyList<string> orderedElementIds)
        {
            _elements.Clear();
            if (orderedElementIds != null)
            {
                for (var i = 0; i < orderedElementIds.Count; i++)
                {
                    var id = orderedElementIds[i];
                    if (!string.IsNullOrEmpty(id))
                    {
                        _elements.Add(id);
                    }
                }
            }

            _focusedIndex = _elements.Count > 0 ? 0 : -1;
        }

        /// <summary>Clear all elements (used when the screen closes).</summary>
        public void Clear()
        {
            _elements.Clear();
            _focusedIndex = -1;
        }

        /// <summary>
        /// Explicitly focus an element by id. Returns false if the id is unknown
        /// (focus is left unchanged).
        /// </summary>
        public bool FocusElement(string elementId)
        {
            var index = _elements.IndexOf(elementId);
            if (index < 0)
            {
                return false;
            }

            _focusedIndex = index;
            return true;
        }

        /// <summary>
        /// Returns true if the given element id currently holds focus — drives the
        /// visual outline in the view (CA-2 "outline visual").
        /// </summary>
        public bool IsFocused(string elementId)
        {
            return HasFocus && _elements[_focusedIndex] == elementId;
        }

        /// <summary>
        /// Apply one navigation input. Movement wraps around the linear order.
        /// Confirm/Cancel do not move focus; the caller decides what they mean for the
        /// focused element. NoOp when there is nothing to navigate.
        /// </summary>
        public FocusResult Apply(FocusInput input)
        {
            if (_elements.Count == 0)
            {
                return input == FocusInput.Cancel ? FocusResult.Cancelled : FocusResult.NoOp;
            }

            if (_focusedIndex < 0)
            {
                _focusedIndex = 0;
            }

            switch (input)
            {
                case FocusInput.Next:
                    _focusedIndex = (_focusedIndex + 1) % _elements.Count;
                    return FocusResult.Moved;
                case FocusInput.Previous:
                    _focusedIndex = (_focusedIndex - 1 + _elements.Count) % _elements.Count;
                    return FocusResult.Moved;
                case FocusInput.Confirm:
                    return FocusResult.Confirmed;
                case FocusInput.Cancel:
                    return FocusResult.Cancelled;
                default:
                    return FocusResult.NoOp;
            }
        }
    }
}
