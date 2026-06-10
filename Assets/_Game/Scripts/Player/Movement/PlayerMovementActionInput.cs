using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace CindarsHope.Player.Movement
{
    // Centralizes input reads for movement actions so legacy and new Input System
    // stay in sync with PlayerController's own input path.
    internal static class PlayerMovementActionInput
    {
        public static bool WasDashPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            return kb != null && kb.spaceKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Space);
#endif
        }

        public static bool IsBlockHeld()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            return kb != null && kb.leftShiftKey.isPressed;
#else
            return Input.GetKey(KeyCode.LeftShift);
#endif
        }

        public static Vector2 GetMoveDirectionHeld()
        {
            float x = 0f, y = 0f;
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb == null) return Vector2.zero;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) y -= 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) y += 1f;
#else
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) y -= 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) y += 1f;
#endif
            var dir = new Vector2(x, y);
            return dir.sqrMagnitude > 1f ? dir.normalized : dir;
        }

        public static bool WasDirectionalKeyPressed(out Vector2 direction)
        {
            direction = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb == null) return false;
            if (kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame)   { direction = Vector2.up;    return true; }
            if (kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame) { direction = Vector2.down;  return true; }
            if (kb.aKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame) { direction = Vector2.left;  return true; }
            if (kb.dKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame){ direction = Vector2.right; return true; }
#else
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))    { direction = Vector2.up;    return true; }
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))  { direction = Vector2.down;  return true; }
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))  { direction = Vector2.left;  return true; }
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) { direction = Vector2.right; return true; }
#endif
            return false;
        }
    }
}
