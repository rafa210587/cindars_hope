using UnityEngine;

namespace CindarsHope.UI.Death
{
    /// <summary>
    /// DEPRECATED — superseded by <see cref="DeathScreenCanvasController"/> (fable_64).
    ///
    /// The death screen is now a keyboard-navigable Canvas self-wired via
    /// RuntimeInitializeOnLoadMethod. This shim remains only so existing scene references to
    /// the old component stay valid; it owns no IMGUI, subscribes to no events, and publishes
    /// nothing. All death-screen behaviour lives in DeathScreenCanvasController.
    ///
    /// Safe to remove from scenes once they are regenerated.
    /// </summary>
    [DisallowMultipleComponent]
    [System.Obsolete("Replaced by DeathScreenCanvasController (fable_64). Empty no-op shim kept for scene compatibility.")]
    public sealed class DeathScreenController : MonoBehaviour
    {
    }
}
