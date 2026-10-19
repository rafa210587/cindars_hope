using UnityEngine;

namespace CindarsHope.Camera
{
    /// <summary>Restores top-down depth sorting when this camera is enabled; camera overrides are not scene-persistent.</summary>
    [RequireComponent(typeof(UnityEngine.Camera))]
    [DisallowMultipleComponent]
    public sealed class CameraTransparencySort2D : MonoBehaviour
    {
        private void OnEnable()
        {
            var camera = GetComponent<UnityEngine.Camera>();
            camera.transparencySortMode = TransparencySortMode.CustomAxis;
            camera.transparencySortAxis = Vector3.up;
        }
    }
}
