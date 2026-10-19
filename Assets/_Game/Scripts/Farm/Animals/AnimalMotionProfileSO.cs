using UnityEngine;

namespace CindarsHope.Farm.Animals
{
    /// <summary>Content profile for one animal presentation, resolved by stable AnimalDataId.</summary>
    [CreateAssetMenu(fileName = "AnimalMotionProfile", menuName = "CindarsHope/Farm/Animal Motion Profile", order = 1)]
    public sealed class AnimalMotionProfileSO : ScriptableObject
    {
        [SerializeField] private string _animalDataId;
        [SerializeField, Min(0.05f)] private float _moveSpeed = 0.7f;
        [SerializeField, Min(0.05f)] private float _wanderRadius = 1f;
        [SerializeField, Min(0f)] private float _idleMin = 0.8f;
        [SerializeField, Min(0f)] private float _idleMax = 2.5f;
        [SerializeField, Min(0f)] private float _peckDuration = 1.1f;
        [SerializeField, Min(0f)] private float _restDuration = 1.8f;
        [SerializeField, Range(0f, 1f)] private float _walkChance = 0.58f;
        [SerializeField, Range(0f, 1f)] private float _peckChance = 0.27f;
        [SerializeField, Min(1)] private int _maxTargetAttempts = 6;
        [SerializeField, Min(0.01f)] private float _targetTolerance = 0.04f;
        [SerializeField, Min(0f)] private float _interactionFreezeSeconds = 0.45f;
        [SerializeField] private Vector2 _bodySize = new Vector2(0.55f, 0.42f);
        [SerializeField, Min(0f)] private float _boundsSkin = 0.03f;
        [SerializeField] private LayerMask _obstacleMask = ~0;
        [SerializeField, Min(0.01f)] private float _distancePerWalkFrame = 0.11f;
        [SerializeField, Min(0.01f)] private float _idleFrameSeconds = 0.4f;
        [SerializeField, Min(0.01f)] private float _peckFrameSeconds = 0.18f;
        [SerializeField, Min(0.01f)] private float _restFrameSeconds = 0.6f;

        [Header("Chicken v18 sprite sequences")]
        [SerializeField] private Sprite[] _idle;
        [SerializeField] private Sprite[] _walkDown;
        [SerializeField] private Sprite[] _walkUp;
        [SerializeField] private Sprite[] _walkSide;
        [SerializeField] private Sprite[] _peck;
        [SerializeField] private Sprite[] _rest;

        public string AnimalDataId => _animalDataId;
        public Vector2 BodySize => new Vector2(Mathf.Max(0.05f, _bodySize.x), Mathf.Max(0.05f, _bodySize.y));
        public float BoundsSkin => Mathf.Max(0f, _boundsSkin);
        public LayerMask ObstacleMask => _obstacleMask;
        public float InteractionFreezeSeconds => Mathf.Max(0f, _interactionFreezeSeconds);
        public float DistancePerWalkFrame => Mathf.Max(0.01f, _distancePerWalkFrame);
        public bool HasAnimatedPresentation => _idle != null && _idle.Length > 0;

        public float GetPoseFrameSeconds(AnimalMotionMode mode)
        {
            if (mode == AnimalMotionMode.Peck) return Mathf.Max(0.01f, _peckFrameSeconds);
            if (mode == AnimalMotionMode.Rest) return Mathf.Max(0.01f, _restFrameSeconds);
            return Mathf.Max(0.01f, _idleFrameSeconds);
        }

        public AnimalMotionSettings CreateSettings()
        {
            return new AnimalMotionSettings(
                _moveSpeed, _wanderRadius, _idleMin, _idleMax, _peckDuration, _restDuration,
                _walkChance, _peckChance, _maxTargetAttempts, _targetTolerance);
        }

        public Sprite[] GetFrames(AnimalMotionMode mode, AnimalFacingDirection direction)
        {
            if (mode == AnimalMotionMode.Walking)
            {
                if (direction == AnimalFacingDirection.Up) return _walkUp;
                if (direction == AnimalFacingDirection.Side) return _walkSide;
                return _walkDown;
            }

            if (mode == AnimalMotionMode.Peck) return _peck;
            if (mode == AnimalMotionMode.Rest) return _rest;
            return _idle;
        }

        /// <summary>Deterministic hook used only by the Editor authoring command and tests.</summary>
        public void Configure(
            string animalDataId,
            AnimalMotionSettings settings,
            Vector2 bodySize,
            float boundsSkin,
            LayerMask obstacleMask,
            float interactionFreezeSeconds,
            float distancePerWalkFrame,
            float idleFrameSeconds,
            float peckFrameSeconds,
            float restFrameSeconds,
            Sprite[] idle,
            Sprite[] walkDown,
            Sprite[] walkUp,
            Sprite[] walkSide,
            Sprite[] peck,
            Sprite[] rest)
        {
            _animalDataId = animalDataId;
            _moveSpeed = settings.MoveSpeed;
            _wanderRadius = settings.WanderRadius;
            _idleMin = settings.IdleMin;
            _idleMax = settings.IdleMax;
            _peckDuration = settings.PeckDuration;
            _restDuration = settings.RestDuration;
            _walkChance = settings.WalkChance;
            _peckChance = settings.PeckChance;
            _maxTargetAttempts = settings.MaxTargetAttempts;
            _targetTolerance = settings.TargetTolerance;
            _bodySize = bodySize;
            _boundsSkin = boundsSkin;
            _obstacleMask = obstacleMask;
            _interactionFreezeSeconds = interactionFreezeSeconds;
            _distancePerWalkFrame = distancePerWalkFrame;
            _idleFrameSeconds = idleFrameSeconds;
            _peckFrameSeconds = peckFrameSeconds;
            _restFrameSeconds = restFrameSeconds;
            _idle = idle;
            _walkDown = walkDown;
            _walkUp = walkUp;
            _walkSide = walkSide;
            _peck = peck;
            _rest = rest;
        }
    }
}
