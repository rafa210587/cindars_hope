using UnityEngine;

namespace CindarsHope.Player.InputRouting
{
    /// <summary>Frame-stable keyboard state consumed by the player movement and attack controllers.</summary>
    public interface IPlayerKeyReader
    {
        bool GetKey(KeyCode key);
        bool GetKeyDown(KeyCode key);
        bool GetKeyUp(KeyCode key);
    }
}
