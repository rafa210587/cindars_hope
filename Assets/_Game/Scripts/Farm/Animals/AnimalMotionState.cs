using System;
using UnityEngine;

namespace CindarsHope.Farm.Animals
{
    public enum AnimalMotionMode
    {
        Idle = 0,
        Walking = 1,
        Peck = 2,
        Rest = 3,
        Stopped = 4
    }

    public enum AnimalFacingDirection
    {
        Down = 0,
        Up = 1,
        Side = 2
    }

    public readonly struct AnimalMotionSettings
    {
        public AnimalMotionSettings(
            float moveSpeed,
            float wanderRadius,
            float idleMin,
            float idleMax,
            float peckDuration,
            float restDuration,
            float walkChance,
            float peckChance,
            int maxTargetAttempts,
            float targetTolerance)
        {
            MoveSpeed = Mathf.Max(0.05f, moveSpeed);
            WanderRadius = Mathf.Max(0.05f, wanderRadius);
            IdleMin = Mathf.Max(0f, idleMin);
            IdleMax = Mathf.Max(IdleMin, idleMax);
            PeckDuration = Mathf.Max(0f, peckDuration);
            RestDuration = Mathf.Max(0f, restDuration);
            WalkChance = Mathf.Clamp01(walkChance);
            PeckChance = Mathf.Clamp(peckChance, 0f, 1f - WalkChance);
            MaxTargetAttempts = Mathf.Max(1, maxTargetAttempts);
            TargetTolerance = Mathf.Max(0.005f, targetTolerance);
        }

        public float MoveSpeed { get; }
        public float WanderRadius { get; }
        public float IdleMin { get; }
        public float IdleMax { get; }
        public float PeckDuration { get; }
        public float RestDuration { get; }
        public float WalkChance { get; }
        public float PeckChance { get; }
        public int MaxTargetAttempts { get; }
        public float TargetTolerance { get; }

        public static AnimalMotionSettings Default => new AnimalMotionSettings(
            0.7f, 1f, 0.8f, 2.5f, 1.1f, 1.8f, 0.58f, 0.27f, 6, 0.04f);
    }

    /// <summary>
    /// Deterministic presentation-only motion decisions. Physics stays in FarmAnimalRuntime and
    /// gameplay/save state never enters this object.
    /// </summary>
    public sealed class AnimalMotionState
    {
        private readonly AnimalMotionSettings _settings;
        private uint _randomState;
        private float _modeTimer;
        private float _freezeTimer;
        private bool _healthStopped;

        public AnimalMotionState(AnimalMotionSettings settings, string animalInstanceId, string animalDataId)
        {
            _settings = settings;
            _randomState = unchecked((uint)CreateStableSeed(animalInstanceId, animalDataId));
            if (_randomState == 0) _randomState = 0x9E3779B9u;
            Mode = AnimalMotionMode.Idle;
            FacingDirection = AnimalFacingDirection.Down;
            _modeTimer = NextRange(_settings.IdleMin, _settings.IdleMax);
        }

        public AnimalMotionMode Mode { get; private set; }
        public AnimalFacingDirection FacingDirection { get; private set; }
        public Vector2 Target { get; private set; }
        public bool IsFrozen => _freezeTimer > 0f;
        public bool IsPaused => _healthStopped || IsFrozen || Mode != AnimalMotionMode.Walking;
        public int LastTargetAttempts { get; private set; }

        public Vector2 Advance(
            float deltaTime,
            Vector2 currentPosition,
            Rect allowedCenterBounds,
            Func<Vector2, Vector2, bool> canMove)
        {
            float dt = Mathf.Max(0f, deltaTime);
            if (_freezeTimer > 0f)
            {
                _freezeTimer = Mathf.Max(0f, _freezeTimer - dt);
                return Vector2.zero;
            }
            if (_healthStopped)
            {
                return Vector2.zero;
            }

            if (Mode != AnimalMotionMode.Walking)
            {
                _modeTimer -= dt;
                if (_modeTimer > 0f)
                {
                    return Vector2.zero;
                }
                SelectNextMode(currentPosition, allowedCenterBounds, canMove);
                if (Mode != AnimalMotionMode.Walking)
                {
                    return Vector2.zero;
                }
            }

            Vector2 remaining = Target - currentPosition;
            if (remaining.sqrMagnitude <= _settings.TargetTolerance * _settings.TargetTolerance)
            {
                EnterIdle();
                return Vector2.zero;
            }

            float maxDistance = _settings.MoveSpeed * dt;
            Vector2 displacement = remaining.normalized * Mathf.Min(maxDistance, remaining.magnitude);
            if (canMove != null && !canMove(currentPosition, displacement))
            {
                if (!TryChooseWalkTarget(currentPosition, allowedCenterBounds, canMove))
                {
                    EnterIdle();
                    return Vector2.zero;
                }

                remaining = Target - currentPosition;
                displacement = remaining.normalized * Mathf.Min(maxDistance, remaining.magnitude);
                if (canMove != null && !canMove(currentPosition, displacement))
                {
                    EnterIdle();
                    return Vector2.zero;
                }
            }

            SetFacing(displacement);
            return displacement;
        }

        public void Freeze(float seconds)
        {
            _freezeTimer = Mathf.Max(_freezeTimer, Mathf.Max(0f, seconds));
        }

        public void SetHealthStopped(bool stopped)
        {
            if (_healthStopped == stopped) return;
            _healthStopped = stopped;
            if (stopped)
            {
                Mode = AnimalMotionMode.Stopped;
                _modeTimer = 0f;
            }
            else
            {
                EnterIdle();
            }
        }

        public static int CreateStableSeed(string animalInstanceId, string animalDataId)
        {
            unchecked
            {
                uint hash = 2166136261u;
                AppendStable(ref hash, animalInstanceId);
                hash ^= 0xFFu;
                hash *= 16777619u;
                AppendStable(ref hash, animalDataId);
                return (int)hash;
            }
        }

        private static void AppendStable(ref uint hash, string value)
        {
            if (value == null) return;
            for (int i = 0; i < value.Length; i++)
            {
                hash ^= value[i];
                hash *= 16777619u;
            }
        }

        private void SelectNextMode(Vector2 currentPosition, Rect bounds, Func<Vector2, Vector2, bool> canMove)
        {
            float roll = Next01();
            if (roll < _settings.WalkChance)
            {
                if (TryChooseWalkTarget(currentPosition, bounds, canMove)) Mode = AnimalMotionMode.Walking;
                else EnterIdle();
                return;
            }
            if (roll < _settings.WalkChance + _settings.PeckChance)
            {
                Mode = AnimalMotionMode.Peck;
                _modeTimer = _settings.PeckDuration;
                return;
            }

            Mode = AnimalMotionMode.Rest;
            _modeTimer = _settings.RestDuration;
        }

        private bool TryChooseWalkTarget(Vector2 currentPosition, Rect bounds, Func<Vector2, Vector2, bool> canMove)
        {
            LastTargetAttempts = 0;
            for (int attempt = 0; attempt < _settings.MaxTargetAttempts; attempt++)
            {
                LastTargetAttempts++;
                float angle = NextRange(0f, Mathf.PI * 2f);
                float distance = NextRange(_settings.TargetTolerance * 2f, _settings.WanderRadius);
                Vector2 candidate = currentPosition + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
                candidate.x = Mathf.Clamp(candidate.x, bounds.xMin, bounds.xMax);
                candidate.y = Mathf.Clamp(candidate.y, bounds.yMin, bounds.yMax);
                Vector2 firstStep = candidate - currentPosition;
                if (firstStep.sqrMagnitude <= _settings.TargetTolerance * _settings.TargetTolerance)
                {
                    continue;
                }
                firstStep = firstStep.normalized * Mathf.Min(_settings.TargetTolerance * 2f, firstStep.magnitude);
                if (canMove != null && !canMove(currentPosition, firstStep))
                {
                    continue;
                }

                Target = candidate;
                SetFacing(candidate - currentPosition);
                Mode = AnimalMotionMode.Walking;
                return true;
            }
            return false;
        }

        private void EnterIdle()
        {
            Mode = AnimalMotionMode.Idle;
            _modeTimer = NextRange(_settings.IdleMin, _settings.IdleMax);
        }

        private void SetFacing(Vector2 direction)
        {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y)) FacingDirection = AnimalFacingDirection.Side;
            else if (direction.y > 0f) FacingDirection = AnimalFacingDirection.Up;
            else if (direction.y < 0f) FacingDirection = AnimalFacingDirection.Down;
        }

        private float NextRange(float min, float max)
        {
            if (max <= min) return min;
            return min + (max - min) * Next01();
        }

        private float Next01()
        {
            uint x = _randomState;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            _randomState = x;
            return (x & 0x00FFFFFFu) / 16777216f;
        }
    }

    public static class AnimalMotionBounds
    {
        public static bool CanContain(Rect outerBounds, Vector2 bodySize, float skin)
        {
            float requiredWidth = Mathf.Max(0f, bodySize.x) + Mathf.Max(0f, skin) * 2f;
            float requiredHeight = Mathf.Max(0f, bodySize.y) + Mathf.Max(0f, skin) * 2f;
            return outerBounds.width >= requiredWidth && outerBounds.height >= requiredHeight;
        }

        public static Rect Contract(Rect outerBounds, Vector2 bodySize, float skin)
        {
            float insetX = Mathf.Max(0f, bodySize.x * 0.5f + skin);
            float insetY = Mathf.Max(0f, bodySize.y * 0.5f + skin);
            float minX = outerBounds.xMin + insetX;
            float maxX = outerBounds.xMax - insetX;
            float minY = outerBounds.yMin + insetY;
            float maxY = outerBounds.yMax - insetY;
            if (maxX < minX) minX = maxX = outerBounds.center.x;
            if (maxY < minY) minY = maxY = outerBounds.center.y;
            return Rect.MinMaxRect(minX, minY, maxX, maxY);
        }
    }
}
