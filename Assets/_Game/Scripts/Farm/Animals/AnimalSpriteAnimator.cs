using UnityEngine;

namespace CindarsHope.Farm.Animals
{
    /// <summary>Small sprite presenter; walk frames advance only from observed world travel.</summary>
    [DisallowMultipleComponent]
    public sealed class AnimalSpriteAnimator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private AnimalMotionProfileSO _profile;

        private AnimalMotionMode _lastMode = (AnimalMotionMode)(-1);
        private AnimalFacingDirection _lastDirection = (AnimalFacingDirection)(-1);
        private float _frameProgress;
        private int _frameIndex;

        public AnimalMotionProfileSO Profile => _profile;
        public int CurrentFrameIndex => _frameIndex;

        public void Configure(SpriteRenderer spriteRenderer, AnimalMotionProfileSO profile)
        {
            _renderer = spriteRenderer;
            _profile = profile;
            _lastMode = (AnimalMotionMode)(-1);
            _lastDirection = (AnimalFacingDirection)(-1);
            _frameProgress = 0f;
            _frameIndex = 0;
            Sprite[] initialFrames = profile != null ? profile.GetFrames(AnimalMotionMode.Idle, AnimalFacingDirection.Down) : null;
            if (_renderer != null && initialFrames != null && initialFrames.Length > 0)
                _renderer.sprite = initialFrames[0];
        }

        public void UpdatePresentation(
            float deltaTime,
            AnimalMotionMode mode,
            AnimalFacingDirection direction,
            float observedTravel,
            float signedHorizontalMotion)
        {
            if (_renderer == null || _profile == null || !_profile.HasAnimatedPresentation) return;

            // Dead/Unavailable owns a frozen silhouette; it must not blink through living idle poses.
            if (mode == AnimalMotionMode.Stopped)
            {
                _lastMode = AnimalMotionMode.Stopped;
                return;
            }

            AnimalMotionMode visualMode = mode;
            Sprite[] frames = _profile.GetFrames(visualMode, direction);
            if (frames == null || frames.Length == 0) return;

            if (_lastMode != visualMode || _lastDirection != direction)
            {
                _lastMode = visualMode;
                _lastDirection = direction;
                _frameProgress = 0f;
                _frameIndex = 0;
            }

            if (visualMode == AnimalMotionMode.Walking)
            {
                _frameProgress += Mathf.Max(0f, observedTravel);
                while (_frameProgress >= _profile.DistancePerWalkFrame)
                {
                    _frameProgress -= _profile.DistancePerWalkFrame;
                    _frameIndex = (_frameIndex + 1) % frames.Length;
                }
            }
            else if (frames.Length > 1)
            {
                _frameProgress += Mathf.Max(0f, deltaTime);
                float frameSeconds = _profile.GetPoseFrameSeconds(visualMode);
                while (_frameProgress >= frameSeconds)
                {
                    _frameProgress -= frameSeconds;
                    _frameIndex = (_frameIndex + 1) % frames.Length;
                }
            }

            if (_frameIndex >= frames.Length) _frameIndex = 0;
            _renderer.sprite = frames[_frameIndex];
            if (direction == AnimalFacingDirection.Side && Mathf.Abs(signedHorizontalMotion) > 0.0001f)
            {
                _renderer.flipX = signedHorizontalMotion < 0f;
            }
            else if (direction != AnimalFacingDirection.Side)
            {
                _renderer.flipX = false;
            }
        }
    }
}
