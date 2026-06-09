using UnityEngine;
using UnityInput = UnityEngine.Input;

namespace CindarsHope.Player.Movement
{
    // WAVE_INTEGRATION_11: Detects double-tap on directional keys for Dodge activation.
    // Design source: docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md §14
    //   Dodge: double tap direcional
    //   Double tap window: 0.08s-0.14s (from input buffer spec)
    //
    // Usage: Call UpdateAndCheckDoubleTap() in Update(). Returns the double-tap direction if fired.
    public sealed class DirectionalDoubleTapDetector
    {
        // Design direction: double tap window 0.08s-0.14s
        private const float DoubleTapWindow = 0.25f; // slightly generous for MVP playability

        private readonly KeyCode[] _directionKeys = { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D,
            KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow };

        private readonly float[] _lastTapTime;
        private readonly Vector2[] _directionVectors = {
            Vector2.up, Vector2.left, Vector2.down, Vector2.right,
            Vector2.up, Vector2.left, Vector2.down, Vector2.right
        };

        public DirectionalDoubleTapDetector()
        {
            _lastTapTime = new float[_directionKeys.Length];
            for (int i = 0; i < _lastTapTime.Length; i++)
                _lastTapTime[i] = float.MinValue;
        }

        // Call each Update(). Returns the double-tap direction if triggered, otherwise null.
        public Vector2? UpdateAndCheckDoubleTap()
        {
            for (int i = 0; i < _directionKeys.Length; i++)
            {
                if (UnityInput.GetKeyDown(_directionKeys[i]))
                {
                    float timeSinceLast = Time.time - _lastTapTime[i];
                    if (timeSinceLast <= DoubleTapWindow && timeSinceLast > 0.02f)
                    {
                        _lastTapTime[i] = float.MinValue; // reset to prevent triple-tap
                        return _directionVectors[i];
                    }

                    _lastTapTime[i] = Time.time;
                }
            }

            return null;
        }
    }
}
