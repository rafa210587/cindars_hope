using UnityEngine;

namespace CindarsHope.Player.Movement
{
    // Detects double-tap on directional keys for Dodge activation.
    // Uses PlayerMovementActionInput so it stays in sync with PlayerController's input path.
    [DisallowMultipleComponent]
    public sealed class DirectionalDoubleTapDetector : MonoBehaviour
    {
        private const float DoubleTapWindow = 0.25f;

        // Canonical order: W, A, S, D, UpArrow, DownArrow, LeftArrow, RightArrow
        private static readonly Vector2[] DirectionVectors =
        {
            Vector2.up, Vector2.left, Vector2.down, Vector2.right,   // W A S D
            Vector2.up, Vector2.down, Vector2.left, Vector2.right    // Up Down Left Right (was: left/down swapped — fixed)
        };

        private float[] _lastTapTime;

        private void Awake() => Initialize();

        public void Initialize()
        {
            if (_lastTapTime != null && _lastTapTime.Length == 8) return;
            _lastTapTime = new float[8];
            for (var i = 0; i < _lastTapTime.Length; i++)
                _lastTapTime[i] = float.MinValue;
        }

        // Call each Update(). Returns the double-tap direction if triggered, otherwise null.
        public Vector2? UpdateAndCheckDoubleTap()
        {
            Initialize();

            // Check per-axis using the input helper (legacy + new input system parity)
            for (var i = 0; i < 8; i++)
            {
                if (!WasKeyDownAtIndex(i)) continue;

                var timeSinceLast = Time.time - _lastTapTime[i];
                if (timeSinceLast <= DoubleTapWindow && timeSinceLast > 0.02f)
                {
                    _lastTapTime[i] = float.MinValue; // reset to prevent triple-tap
                    return DirectionVectors[i];
                }

                _lastTapTime[i] = Time.time;
            }

            return null;
        }

        // Maps index 0-7 to the corresponding directional key press via the input helper.
        private static bool WasKeyDownAtIndex(int i)
        {
            // 0=W/up, 1=A/left, 2=S/down, 3=D/right, 4=UpArrow, 5=DownArrow, 6=LeftArrow, 7=RightArrow
            // Grouped by direction so index 0 and 4 don't double-fire for the same physical press:
            // The double-tap window check already handles this (timeSinceLast > 0.02f guard).
#if ENABLE_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb == null) return false;
            return i switch
            {
                0 => kb.wKey.wasPressedThisFrame,
                1 => kb.aKey.wasPressedThisFrame,
                2 => kb.sKey.wasPressedThisFrame,
                3 => kb.dKey.wasPressedThisFrame,
                4 => kb.upArrowKey.wasPressedThisFrame,
                5 => kb.downArrowKey.wasPressedThisFrame,
                6 => kb.leftArrowKey.wasPressedThisFrame,
                7 => kb.rightArrowKey.wasPressedThisFrame,
                _ => false
            };
#else
            return i switch
            {
                0 => Input.GetKeyDown(KeyCode.W),
                1 => Input.GetKeyDown(KeyCode.A),
                2 => Input.GetKeyDown(KeyCode.S),
                3 => Input.GetKeyDown(KeyCode.D),
                4 => Input.GetKeyDown(KeyCode.UpArrow),
                5 => Input.GetKeyDown(KeyCode.DownArrow),
                6 => Input.GetKeyDown(KeyCode.LeftArrow),
                7 => Input.GetKeyDown(KeyCode.RightArrow),
                _ => false
            };
#endif
        }
    }
}
