using System;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using UnityEngine;

namespace CindarsHope.Skills.Runtime
{
    [DisallowMultipleComponent]
    public sealed class EfficiencyMarkRuntimeCoordinator : MonoBehaviour,
        IWorkStaminaCostModifierRuntime
    {
        private static EfficiencyMarkRuntimeCoordinator _instance;
        private readonly FractionalWorkStaminaCredit _credit =
            new FractionalWorkStaminaCredit();

        private string _stationInstanceId = string.Empty;
        private Vector2 _anchor;
        private float _radius;
        private float _reductionFraction;
        private float _expiresAt;
        private Behaviour _stationOwner;

        public static EfficiencyMarkRuntimeCoordinator Instance => _instance;
        public bool IsActive => _stationOwner != null && _stationOwner.isActiveAndEnabled &&
            Time.time < _expiresAt;
        public string StationInstanceId => _stationInstanceId;
        public Vector2 Anchor => _anchor;
        public float RemainingSeconds => Mathf.Max(0f, _expiresAt - Time.time);
        public float FractionalCredit => _credit.Credit;

        public static EfficiencyMarkRuntimeCoordinator Install(Transform owner)
        {
            if (_instance != null) return _instance;
            var go = new GameObject(nameof(EfficiencyMarkRuntimeCoordinator));
            if (owner != null) go.transform.SetParent(owner, false);
            else if (Application.isPlaying) DontDestroyOnLoad(go);
            return go.AddComponent<EfficiencyMarkRuntimeCoordinator>();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void OnEnable()
        {
            if (_instance != this) return;
            WorkStaminaCostModifierProvider.Source = this;
            GameEventBus.Subscribe<SceneTransitionStartedEvent>(OnSceneTransitionStarted);
        }

        private void OnDisable()
        {
            if (_instance == this)
                GameEventBus.Unsubscribe<SceneTransitionStartedEvent>(OnSceneTransitionStarted);
            if (ReferenceEquals(WorkStaminaCostModifierProvider.Source, this))
                WorkStaminaCostModifierProvider.Source = null;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        public bool Activate(string stationInstanceId, Behaviour stationOwner,
            Vector2 anchor, float radius,
            float durationSeconds, float reductionFraction)
        {
            if (string.IsNullOrWhiteSpace(stationInstanceId) || stationOwner == null ||
                !stationOwner.isActiveAndEnabled || radius < 0f ||
                durationSeconds <= 0f || reductionFraction <= 0f)
                return false;

            _stationInstanceId = stationInstanceId;
            _stationOwner = stationOwner;
            _anchor = anchor;
            _radius = radius;
            _reductionFraction = Mathf.Clamp01(reductionFraction);
            _expiresAt = Time.time + durationSeconds;
            return true;
        }

        private void OnSceneTransitionStarted(SceneTransitionStartedEvent evt) => ClearActiveMark();

        public void ClearActiveMark()
        {
            _stationInstanceId = string.Empty;
            _stationOwner = null;
            _expiresAt = 0f;
            _radius = 0f;
            _reductionFraction = 0f;
        }

        public int PreviewCost(WorkStaminaChannel channel, int baseCost,
            float worldX, float worldY, string stationInstanceId = "")
            => IsEligible(worldX, worldY, stationInstanceId)
                ? _credit.PreviewCost(baseCost, _reductionFraction)
                : Mathf.Max(0, baseCost);

        public void CommitSpend(WorkStaminaChannel channel, int baseCost, int chargedCost,
            float worldX, float worldY, string stationInstanceId = "")
        {
            if (IsEligible(worldX, worldY, stationInstanceId))
                _credit.CommitSpend(baseCost, chargedCost, _reductionFraction);
        }

        private bool IsEligible(float worldX, float worldY, string stationInstanceId)
        {
            if (!IsActive) return false;
            if (!string.IsNullOrWhiteSpace(stationInstanceId) &&
                string.Equals(stationInstanceId, _stationInstanceId,
                    StringComparison.Ordinal)) return true;
            var point = new Vector2(worldX, worldY);
            return (point - _anchor).sqrMagnitude <= _radius * _radius;
        }
    }
}
