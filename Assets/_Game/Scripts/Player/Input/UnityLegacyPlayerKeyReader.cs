using UnityEngine;

namespace CindarsHope.Player.InputRouting
{
    /// <summary>Default adapter that preserves Unity's legacy key and edge semantics.</summary>
    public sealed class UnityLegacyPlayerKeyReader : IPlayerKeyReader
    {
        public static UnityLegacyPlayerKeyReader Instance { get; } = new UnityLegacyPlayerKeyReader();

        private UnityLegacyPlayerKeyReader() { }

        public bool GetKey(KeyCode key) => UnityEngine.Input.GetKey(key);
        public bool GetKeyDown(KeyCode key) => UnityEngine.Input.GetKeyDown(key);
        public bool GetKeyUp(KeyCode key) => UnityEngine.Input.GetKeyUp(key);
    }
}
