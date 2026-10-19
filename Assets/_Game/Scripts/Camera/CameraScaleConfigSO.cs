using UnityEngine;

namespace CindarsHope.Camera
{
    [CreateAssetMenu(fileName = "CameraScaleConfig", menuName = "CindarsHope/Camera/Camera Scale Config")]
    public sealed class CameraScaleConfigSO : ScriptableObject
    {
        [Header("Orthographic Sizes per Context")]
        [Tooltip("Default orthographic size used when no context is active.")]
        public float DefaultOrthographicSize = 8.5f;
        [Tooltip("Orthographic size for the Farm scene.")]
        public float FarmOrthographicSize = 8.5f;
        [Tooltip("Orthographic size for the Town scene.")]
        public float TownOrthographicSize = 8f;
        [Tooltip("Orthographic size for the Cave scene (standard levels).")]
        public float CaveOrthographicSize = 7f;
        [Tooltip("Orthographic size for boss arena encounters (slight zoom-out).")]
        public float BossArenaOrthographicSize = 10f;

        [Header("Limits")]
        public float MinOrthographicSize = 3f;
        public float MaxOrthographicSize = 20f;

        [Header("Follow")]
        [Tooltip("Additional Y offset from the player (positive = camera above player).")]
        public float FollowOffsetY = 0f;
        [Tooltip("Dead zone radius: camera only starts moving when target moves outside this range.")]
        public float DeadZoneRadius = 0f;

        [Header("Transition")]
        [Tooltip("Seconds to smoothly transition between orthographic sizes.")]
        public float SizeTransitionTime = 0.5f;

        private void OnValidate()
        {
            DefaultOrthographicSize = Mathf.Clamp(DefaultOrthographicSize, MinOrthographicSize, MaxOrthographicSize);
            FarmOrthographicSize = Mathf.Clamp(FarmOrthographicSize, MinOrthographicSize, MaxOrthographicSize);
            TownOrthographicSize = Mathf.Clamp(TownOrthographicSize, MinOrthographicSize, MaxOrthographicSize);
            CaveOrthographicSize = Mathf.Clamp(CaveOrthographicSize, MinOrthographicSize, MaxOrthographicSize);
            BossArenaOrthographicSize = Mathf.Clamp(BossArenaOrthographicSize, MinOrthographicSize, MaxOrthographicSize);
            SizeTransitionTime = Mathf.Max(0f, SizeTransitionTime);
        }
    }
}
