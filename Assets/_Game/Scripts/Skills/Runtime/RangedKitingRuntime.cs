using System;
using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.Player;
using CindarsHope.Player.Movement;
using UnityEngine;

namespace CindarsHope.Skills.Runtime
{
    [DisallowMultipleComponent]
    public sealed class RangedKitingRuntime : MonoBehaviour
    {
        public const float WindowSeconds = 2f;
        public const float AwayDotThreshold = 0.5f;
        public const float ThreatSearchRadius = 20f;

        public readonly struct ThreatSnapshot
        {
            public readonly string EnemyInstanceId;
            public readonly Vector2 Position;

            public ThreatSnapshot(string enemyInstanceId, Vector2 position)
            {
                EnemyInstanceId = enemyInstanceId ?? string.Empty;
                Position = position;
            }
        }

        private PlayerController _player;
        private Func<float> _bonusSource;
        private Func<Vector2, ThreatSnapshot?> _nearestThreat;
        private Func<string, ThreatSnapshot?> _liveThreat;
        private ThreatSnapshot _threat;
        private float _expiresAt;
        private bool _active;

        public bool IsActive => _active;

        public void Configure(
            PlayerController player,
            Func<float> bonusSource,
            Func<Vector2, ThreatSnapshot?> nearestThreat,
            Func<string, ThreatSnapshot?> liveThreat)
        {
            _player = player;
            _bonusSource = bonusSource;
            _nearestThreat = nearestThreat;
            _liveThreat = liveThreat;
            Clear();
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<PlayerBowShootEvent>(OnBowShot);
            GameEventBus.Subscribe<PlayerOffensiveActionCommittedEvent>(OnOffensiveAction);
            GameEventBus.Subscribe<SceneTransitionStartedEvent>(OnSceneTransition);
            GameEventBus.Subscribe<SkillDerivedStatsChangedEvent>(OnDerivedStatsChanged);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<PlayerBowShootEvent>(OnBowShot);
            GameEventBus.Unsubscribe<PlayerOffensiveActionCommittedEvent>(OnOffensiveAction);
            GameEventBus.Unsubscribe<SceneTransitionStartedEvent>(OnSceneTransition);
            GameEventBus.Unsubscribe<SkillDerivedStatsChangedEvent>(OnDerivedStatsChanged);
            Clear();
        }

        private void Update() => Tick(Time.time);

        public void Tick(float now)
        {
            if (!_active || _player == null || now >= _expiresAt)
            {
                if (_active) Clear();
                return;
            }

            var currentThreat = _liveThreat?.Invoke(_threat.EnemyInstanceId);
            if (!currentThreat.HasValue)
            {
                Clear();
                return;
            }
            _threat = currentThreat.Value;

            float bonus = Mathf.Max(0f, _bonusSource?.Invoke() ?? 0f);
            if (bonus <= 0f || !ShouldApply(_player.MoveInput, transform.position, _threat.Position))
            {
                _player.SpeedComposer.ClearFactor(SpeedFactorKind.Kiting);
                return;
            }

            _player.SpeedComposer.SetFactor(SpeedFactorKind.Kiting, 1f + bonus);
        }

        public static bool ShouldApply(Vector2 movement, Vector2 playerPosition, Vector2 threatPosition)
        {
            Vector2 away = playerPosition - threatPosition;
            if (movement.sqrMagnitude <= 0.0001f || away.sqrMagnitude <= 0.0001f)
                return false;
            return Vector2.Dot(movement.normalized, away.normalized) >= AwayDotThreshold;
        }

        private void OnBowShot(PlayerBowShootEvent evt)
        {
            float bonus = Mathf.Max(0f, _bonusSource?.Invoke() ?? 0f);
            var threat = _nearestThreat?.Invoke(transform.position);
            if (bonus <= 0f || !threat.HasValue)
            {
                Clear();
                return;
            }

            _threat = threat.Value;
            _expiresAt = Time.time + WindowSeconds;
            _active = true;
            Tick(Time.time);
        }

        private void OnOffensiveAction(PlayerOffensiveActionCommittedEvent evt)
        {
            if (string.Equals(evt.SourceKind, "Melee", StringComparison.Ordinal))
                Clear();
        }

        private void OnSceneTransition(SceneTransitionStartedEvent evt) => Clear();

        private void OnDerivedStatsChanged(SkillDerivedStatsChangedEvent evt)
        {
            if ((_bonusSource?.Invoke() ?? 0f) <= 0f)
                Clear();
        }

        private void Clear()
        {
            _active = false;
            _expiresAt = 0f;
            if (_player != null)
                _player.SpeedComposer.ClearFactor(SpeedFactorKind.Kiting);
        }
    }
}
