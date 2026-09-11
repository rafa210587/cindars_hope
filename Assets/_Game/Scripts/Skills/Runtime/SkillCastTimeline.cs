using System;
using UnityEngine;

namespace CindarsHope.Skills.Runtime
{
    public enum SkillCastPhase
    {
        Idle,
        Windup,
        Active,
        Recovery,
        Cancelled
    }

    public readonly struct SkillCastTimelineResult
    {
        public bool ShouldCommit { get; }
        public bool Completed { get; }
        public bool Cancelled { get; }

        public SkillCastTimelineResult(bool shouldCommit, bool completed, bool cancelled)
        {
            ShouldCommit = shouldCommit;
            Completed = completed;
            Cancelled = cancelled;
        }
    }

    /// <summary>Pure scaled-time state machine. It owns no Unity objects or gameplay mutation.</summary>
    public sealed class SkillCastTimeline
    {
        private const float Epsilon = .00001f;
        private float _windupSeconds;
        private float _activeSeconds;
        private float _recoverySeconds;
        private float _remainingSeconds;
        private float _deferredAfterCommitSeconds;
        private bool _commitRequested;
        private bool _commitAcknowledged;

        public SkillCastPhase Phase { get; private set; } = SkillCastPhase.Idle;
        public bool HasCommitted => _commitAcknowledged;
        public bool IsRunning => Phase != SkillCastPhase.Idle && Phase != SkillCastPhase.Cancelled;
        public event Action<SkillCastPhase, float> PhaseChanged;

        public SkillCastTimelineResult Begin(float windupSeconds, float activeSeconds, float recoverySeconds)
        {
            _windupSeconds = Mathf.Max(0f, windupSeconds);
            _activeSeconds = Mathf.Max(0f, activeSeconds);
            _recoverySeconds = Mathf.Max(0f, recoverySeconds);
            _commitRequested = false;
            _commitAcknowledged = false;
            _deferredAfterCommitSeconds = 0f;
            Enter(SkillCastPhase.Windup, _windupSeconds);
            return Tick(0f);
        }

        public SkillCastTimelineResult Tick(float scaledDeltaSeconds)
        {
            if (!IsRunning)
                return default;

            if (_commitRequested && !_commitAcknowledged)
                return default;

            float available = _deferredAfterCommitSeconds + Mathf.Max(0f, scaledDeltaSeconds);
            _deferredAfterCommitSeconds = 0f;
            bool shouldCommit = false;
            bool completed = false;

            while (IsRunning)
            {
                if (_remainingSeconds > Epsilon)
                {
                    if (available + Epsilon < _remainingSeconds)
                    {
                        _remainingSeconds -= available;
                        break;
                    }

                    available = Mathf.Max(0f, available - _remainingSeconds);
                    _remainingSeconds = 0f;
                }

                switch (Phase)
                {
                    case SkillCastPhase.Windup:
                        if (!_commitRequested)
                        {
                            _commitRequested = true;
                            shouldCommit = true;
                        }
                        _deferredAfterCommitSeconds = available;
                        return new SkillCastTimelineResult(shouldCommit, false, false);
                    case SkillCastPhase.Active:
                        Enter(SkillCastPhase.Recovery, _recoverySeconds);
                        break;
                    case SkillCastPhase.Recovery:
                        Enter(SkillCastPhase.Idle, 0f);
                        completed = true;
                        break;
                }

                if (available <= Epsilon && _remainingSeconds > Epsilon)
                    break;
            }

            return new SkillCastTimelineResult(shouldCommit, completed, false);
        }

        /// <summary>Acknowledges a successful atomic commit, then enters Active and resumes the same tick.</summary>
        public SkillCastTimelineResult ContinueAfterCommit()
        {
            if (!_commitRequested || _commitAcknowledged || Phase != SkillCastPhase.Windup)
                return default;

            _commitAcknowledged = true;
            Enter(SkillCastPhase.Active, _activeSeconds);
            return Tick(0f);
        }

        public SkillCastTimelineResult Cancel()
        {
            if (!IsRunning)
                return default;

            Enter(SkillCastPhase.Cancelled, 0f);
            return new SkillCastTimelineResult(false, false, true);
        }

        public void Reset()
        {
            _commitRequested = false;
            _commitAcknowledged = false;
            _deferredAfterCommitSeconds = 0f;
            Enter(SkillCastPhase.Idle, 0f);
        }

        private void Enter(SkillCastPhase phase, float durationSeconds)
        {
            Phase = phase;
            _remainingSeconds = durationSeconds;
            PhaseChanged?.Invoke(phase, durationSeconds);
        }
    }
}
