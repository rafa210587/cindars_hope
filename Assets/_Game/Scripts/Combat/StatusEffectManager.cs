using System;
using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Combat
{
    public class StatusEffectManager : MonoBehaviour
    {
        private Dictionary<string, List<StatusEffectInstance>> _targetStatuses = new();
        private float _updateTimer = 0f;

        private void Update()
        {
            _updateTimer += UnityEngine.Time.deltaTime;

            if (_updateTimer >= 0.1f)
            {
                UpdateAllStatuses(_updateTimer);
                _updateTimer = 0f;
            }
        }

        public void ApplyStatus(StatusEffectSO effectData, string targetId, string sourceId = "")
        {
            if (effectData == null || string.IsNullOrEmpty(targetId))
                return;

            if (!_targetStatuses.ContainsKey(targetId))
            {
                _targetStatuses[targetId] = new List<StatusEffectInstance>();
            }

            var existingStatus = _targetStatuses[targetId].Find(s => s.EffectData?.StatusId == effectData.StatusId);

            if (existingStatus != null)
            {
                // Refresh existing status
                existingStatus.Refresh(effectData);
                GameEventBus.Publish(new StatusRefreshedEvent(targetId, effectData.StatusId, effectData.DurationSeconds));
            }
            else
            {
                // Apply new status
                var instance = new StatusEffectInstance(effectData, targetId, sourceId);
                _targetStatuses[targetId].Add(instance);
                GameEventBus.Publish(new StatusAppliedEvent(targetId, effectData.StatusId, sourceId, effectData.DurationSeconds));
            }
        }

        public void RemoveStatus(string targetId, string statusId)
        {
            if (!_targetStatuses.ContainsKey(targetId))
                return;

            var toRemove = _targetStatuses[targetId].Find(s => s.EffectData?.StatusId == statusId);
            if (toRemove != null)
            {
                _targetStatuses[targetId].Remove(toRemove);
                GameEventBus.Publish(new StatusRemovedEvent(targetId, statusId));
            }

            if (_targetStatuses[targetId].Count == 0)
            {
                _targetStatuses.Remove(targetId);
            }
        }

        public bool HasStatus(string targetId, string statusId)
        {
            if (!_targetStatuses.ContainsKey(targetId))
                return false;

            return _targetStatuses[targetId].Exists(s => s.EffectData?.StatusId == statusId);
        }

        public StatusEffectInstance GetStatus(string targetId, string statusId)
        {
            if (!_targetStatuses.ContainsKey(targetId))
                return null;

            return _targetStatuses[targetId].Find(s => s.EffectData?.StatusId == statusId);
        }

        public List<StatusEffectInstance> GetAllStatusesFor(string targetId)
        {
            if (!_targetStatuses.ContainsKey(targetId))
                return new List<StatusEffectInstance>();

            return new List<StatusEffectInstance>(_targetStatuses[targetId]);
        }

        public void ClearAllStatusesFor(string targetId)
        {
            if (_targetStatuses.ContainsKey(targetId))
            {
                _targetStatuses.Remove(targetId);
            }
        }

        private void UpdateAllStatuses(float deltaTime)
        {
            var targetIds = new List<string>(_targetStatuses.Keys);

            foreach (var targetId in targetIds)
            {
                var statuses = _targetStatuses[targetId];
                var expiredStatuses = new List<StatusEffectInstance>();

                foreach (var status in statuses)
                {
                    if (status.IsExpired)
                    {
                        expiredStatuses.Add(status);
                        continue;
                    }

                    // Update tick progress
                    status.TickProgress += deltaTime;

                    // Handle tick
                    if (status.ShouldTick)
                    {
                        HandleStatusTick(status);
                        status.ResetTickProgress();
                    }

                    // Update duration
                    status.RemainingDuration -= deltaTime;
                }

                // Remove expired statuses
                foreach (var expired in expiredStatuses)
                {
                    statuses.Remove(expired);
                    GameEventBus.Publish(new StatusExpiredEvent(targetId, expired.EffectData.StatusId));
                }

                // Cleanup empty lists
                if (statuses.Count == 0)
                {
                    _targetStatuses.Remove(targetId);
                }
            }
        }

        private void HandleStatusTick(StatusEffectInstance status)
        {
            if (status?.EffectData == null)
                return;

            var effectData = status.EffectData;

            if (effectData.StatusType == StatusType.Burn)
            {
                ApplyStatusDamage(status, effectData.DamageType, "Burn");
            }
            else if (effectData.StatusType == StatusType.Poison)
            {
                ApplyStatusDamage(status, effectData.DamageType, "Poison");
            }
            else if (effectData.StatusType == StatusType.Bleed)
            {
                ApplyStatusDamage(status, effectData.DamageType, "Bleed");
            }
        }

        private void ApplyStatusDamage(StatusEffectInstance status, DamageType damageType, string sourceName)
        {
            int damageAmount = status.Power;
            GameEventBus.Publish(new StatusTickedEvent(status.TargetId, status.EffectData.StatusId, damageAmount));
        }
    }
}
