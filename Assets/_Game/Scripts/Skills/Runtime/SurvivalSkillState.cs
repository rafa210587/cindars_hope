using System;
using System.Collections.Generic;
using CindarsHope.Foundation;

namespace CindarsHope.Skills.Runtime
{
    /// <summary>
    /// Pure authority for one player survival encounter and its run-bound idempotency flags.
    /// Unity adapters own event subscriptions and scene objects; this state only applies domain rules.
    /// </summary>
    public sealed class SurvivalSkillState
    {
        private const int FirstEncounterOrdinal = 1;

        public enum EncounterMembershipChange
        {
            Rejected = 0,
            Started = 1,
            Joined = 2,
            AlreadyMember = 3
        }

        private readonly HashSet<string> _activeEnemyInstanceIds =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _leashedEnemyInstanceIds =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _consumedLastBreathEncounterIds =
            new HashSet<string>(StringComparer.Ordinal);

        public string ActiveRunId { get; private set; } = string.Empty;
        public int ActiveCaveLevel { get; private set; }
        public string ActiveEncounterId { get; private set; } = string.Empty;
        public int ActiveEncounterOrdinal { get; private set; }
        public string EncounterOrdinalRunId { get; private set; } = string.Empty;
        public int NextEncounterOrdinal { get; private set; } = FirstEncounterOrdinal;
        public IReadOnlyCollection<string> ActiveEnemyInstanceIds => _activeEnemyInstanceIds;
        public IReadOnlyCollection<string> LeashedEnemyInstanceIds => _leashedEnemyInstanceIds;
        public IReadOnlyCollection<string> ConsumedLastBreathEncounterIds =>
            _consumedLastBreathEncounterIds;
        public bool HasActiveEncounter => !string.IsNullOrWhiteSpace(ActiveEncounterId);
        public float EncounterQuietRemainingSeconds { get; private set; }
        public string LunarConsumedEncounterId { get; private set; } = string.Empty;
        public string LunarConsumedFirstTargetInstanceId { get; private set; } = string.Empty;
        public string ActiveLunarTargetInstanceId { get; private set; } = string.Empty;
        public string ActiveLunarVariant { get; private set; } = string.Empty;
        public int ActiveLunarRank { get; private set; }
        public float ActiveLunarRemainingSeconds { get; private set; }
        public string ActiveLunarActionToken { get; private set; } = string.Empty;
        public DamageType LastOffensiveMagicDamageType { get; private set; }
        public bool HasActiveLunarFocus => HasActiveEncounter &&
            ActiveLunarRemainingSeconds > 0f &&
            !string.IsNullOrWhiteSpace(ActiveLunarTargetInstanceId);
        public bool IsRestoreInProgress { get; private set; }
        public float LastBreathArmedRemainingSeconds { get; private set; }
        public string CampUsedRunId { get; private set; } = string.Empty;
        public string ActiveCampRunId { get; private set; } = string.Empty;
        public int ActiveCampCaveLevel { get; private set; }
        public float ActiveCampRemainingSeconds { get; private set; }
        public float ActiveCampPositionX { get; private set; }
        public float ActiveCampPositionY { get; private set; }
        public string PendingChannelActionId { get; private set; } = string.Empty;
        public string CavebornConsumedRunId { get; private set; } = string.Empty;
        public string ActiveCavebornRunId { get; private set; } = string.Empty;
        public int ActiveCavebornRank { get; private set; }
        public float ActiveCavebornRemainingSeconds { get; private set; }
        public bool CavebornWasInCombat { get; private set; }
        public bool CavebornRegenDoubled { get; private set; }
        public bool IsCavebornActive => ActiveCavebornRemainingSeconds > 0f &&
            !string.IsNullOrWhiteSpace(ActiveCavebornRunId);

        public bool BeginOrJoinEncounter(string runId, int caveLevel, string enemyInstanceId)
            => RegisterEnemyAggro(runId, caveLevel, enemyInstanceId) ==
               EncounterMembershipChange.Started;

        public EncounterMembershipChange RegisterEnemyAggro(
            string scopeId,
            int caveLevel,
            string enemyInstanceId)
        {
            if (string.IsNullOrWhiteSpace(scopeId) || caveLevel < 0 ||
                string.IsNullOrWhiteSpace(enemyInstanceId))
            {
                return EncounterMembershipChange.Rejected;
            }

            if (HasActiveEncounter &&
                string.Equals(ActiveRunId, scopeId, StringComparison.Ordinal) &&
                ActiveCaveLevel == caveLevel)
            {
                CancelEncounterQuietResolution();
                return _activeEnemyInstanceIds.Add(enemyInstanceId)
                    ? EncounterMembershipChange.Joined
                    : EncounterMembershipChange.AlreadyMember;
            }

            ClearActiveEncounter();
            if (!string.Equals(EncounterOrdinalRunId, scopeId, StringComparison.Ordinal))
            {
                EncounterOrdinalRunId = scopeId;
                NextEncounterOrdinal = FirstEncounterOrdinal;
            }

            ActiveRunId = scopeId;
            ActiveCaveLevel = caveLevel;
            ActiveEncounterOrdinal = Math.Max(FirstEncounterOrdinal, NextEncounterOrdinal);
            NextEncounterOrdinal = ActiveEncounterOrdinal + 1;
            ActiveEncounterId = ComposeEncounterId(scopeId, caveLevel, ActiveEncounterOrdinal);
            _activeEnemyInstanceIds.Add(enemyInstanceId);
            return EncounterMembershipChange.Started;
        }

        public bool JoinActiveEncounter(string enemyInstanceId)
        {
            if (!HasActiveEncounter || string.IsNullOrWhiteSpace(enemyInstanceId) ||
                !_activeEnemyInstanceIds.Add(enemyInstanceId))
            {
                return false;
            }

            CancelEncounterQuietResolution();
            return true;
        }

        public bool IsActiveEncounterScope(string scopeId, int caveLevel)
        {
            return HasActiveEncounter &&
                   string.Equals(ActiveRunId, scopeId, StringComparison.Ordinal) &&
                   ActiveCaveLevel == caveLevel;
        }

        public bool IsActiveEncounterEnemy(string enemyInstanceId)
            => HasActiveEncounter && !string.IsNullOrWhiteSpace(enemyInstanceId) &&
               _activeEnemyInstanceIds.Contains(enemyInstanceId);

        public bool HasConsumedLunarFirstTargetForActiveEncounter
            => HasActiveEncounter && string.Equals(
                LunarConsumedEncounterId, ActiveEncounterId, StringComparison.Ordinal) &&
               !string.IsNullOrWhiteSpace(LunarConsumedFirstTargetInstanceId);

        public bool TryActivateLunarFocus(
            string targetInstanceId,
            string variant,
            int rank,
            float durationSeconds,
            string actionToken)
        {
            if (IsRestoreInProgress || !HasActiveEncounter ||
                !IsActiveEncounterEnemy(targetInstanceId) ||
                !RangedLunarRules.IsKnownVariant(variant) || rank <= 0 ||
                durationSeconds <= 0f ||
                HasConsumedLunarFirstTargetForActiveEncounter)
            {
                return false;
            }

            LunarConsumedEncounterId = ActiveEncounterId;
            LunarConsumedFirstTargetInstanceId = targetInstanceId;
            ActiveLunarTargetInstanceId = targetInstanceId;
            ActiveLunarVariant = variant;
            ActiveLunarRank = Math.Max(1, Math.Min(3, rank));
            ActiveLunarRemainingSeconds = Math.Min(
                RangedLunarRules.MaxFocusDurationSeconds, durationSeconds);
            ActiveLunarActionToken = actionToken ?? string.Empty;
            return true;
        }

        public bool AdvanceLunarFocusTime(float deltaSeconds, out string expiredTargetInstanceId)
        {
            expiredTargetInstanceId = string.Empty;
            if (deltaSeconds <= 0f || IsRestoreInProgress || !HasActiveLunarFocus)
                return false;

            ActiveLunarRemainingSeconds = Math.Max(0f, ActiveLunarRemainingSeconds - deltaSeconds);
            if (ActiveLunarRemainingSeconds > 0f)
                return false;

            expiredTargetInstanceId = ActiveLunarTargetInstanceId;
            ClearActiveLunarFocus();
            return true;
        }

        public bool ClearActiveLunarFocus()
        {
            bool changed = HasActiveLunarFocus ||
                !string.IsNullOrWhiteSpace(ActiveLunarTargetInstanceId);
            ActiveLunarTargetInstanceId = string.Empty;
            ActiveLunarVariant = string.Empty;
            ActiveLunarRank = 0;
            ActiveLunarRemainingSeconds = 0f;
            ActiveLunarActionToken = string.Empty;
            return changed;
        }

        public void RecordOffensiveMagicDamageType(DamageType damageType)
        {
            if (damageType != DamageType.Physical)
                LastOffensiveMagicDamageType = damageType;
        }

        public bool MarkEnemyLeashed(string enemyInstanceId, float quietSeconds)
        {
            if (!HasActiveEncounter || string.IsNullOrWhiteSpace(enemyInstanceId) ||
                !_activeEnemyInstanceIds.Contains(enemyInstanceId))
            {
                return false;
            }

            _leashedEnemyInstanceIds.Add(enemyInstanceId);
            return TryStartQuietResolution(quietSeconds);
        }

        public bool MarkEnemiesLeashed(IEnumerable<string> enemyInstanceIds, float quietSeconds)
        {
            if (!HasActiveEncounter || enemyInstanceIds == null)
            {
                return false;
            }

            foreach (string enemyInstanceId in enemyInstanceIds)
            {
                if (!string.IsNullOrWhiteSpace(enemyInstanceId) &&
                    _activeEnemyInstanceIds.Contains(enemyInstanceId))
                {
                    _leashedEnemyInstanceIds.Add(enemyInstanceId);
                }
            }

            return TryStartQuietResolution(quietSeconds);
        }

        public void CancelEncounterQuietResolution()
        {
            EncounterQuietRemainingSeconds = 0f;
            _leashedEnemyInstanceIds.Clear();
        }

        public bool AdvanceEncounterQuietTime(
            float deltaSeconds,
            out string resolvedEncounterId)
        {
            resolvedEncounterId = string.Empty;
            if (deltaSeconds <= 0f || IsRestoreInProgress || !HasActiveEncounter ||
                EncounterQuietRemainingSeconds <= 0f)
            {
                return false;
            }

            EncounterQuietRemainingSeconds =
                Math.Max(0f, EncounterQuietRemainingSeconds - deltaSeconds);
            if (EncounterQuietRemainingSeconds > 0f)
            {
                return false;
            }

            resolvedEncounterId = ResolveActiveEncounter();
            return true;
        }

        public bool RemoveEnemyAndResolveIfEmpty(string enemyInstanceId, out string resolvedEncounterId)
            => RemoveEnemyAndResolveIfEmpty(enemyInstanceId, 0f, out resolvedEncounterId);

        public bool RemoveEnemyAndResolveIfEmpty(
            string enemyInstanceId,
            float quietSeconds,
            out string resolvedEncounterId)
        {
            resolvedEncounterId = string.Empty;
            if (!HasActiveEncounter || string.IsNullOrWhiteSpace(enemyInstanceId) ||
                !_activeEnemyInstanceIds.Remove(enemyInstanceId))
            {
                return false;
            }

            _leashedEnemyInstanceIds.Remove(enemyInstanceId);
            if (string.Equals(ActiveLunarTargetInstanceId, enemyInstanceId,
                    StringComparison.Ordinal))
                ClearActiveLunarFocus();

            if (_activeEnemyInstanceIds.Count > 0)
            {
                TryStartQuietResolution(quietSeconds);
                return false;
            }

            resolvedEncounterId = ResolveActiveEncounter();
            return true;
        }

        public string ResolveActiveEncounter()
        {
            string resolvedEncounterId = ActiveEncounterId;
            ClearActiveEncounter();
            return resolvedEncounterId;
        }

        public bool TryConsumeLastBreathForActiveEncounter()
        {
            if (!HasActiveEncounter || IsRestoreInProgress)
            {
                return false;
            }

            bool consumed = _consumedLastBreathEncounterIds.Add(ActiveEncounterId);
            if (consumed)
            {
                LastBreathArmedRemainingSeconds = 0f;
            }

            return consumed;
        }

        public bool IsLastBreathConsumed(string encounterId)
        {
            return !string.IsNullOrWhiteSpace(encounterId) &&
                   _consumedLastBreathEncounterIds.Contains(encounterId);
        }

        public bool TryArmLastBreath(float remainingSeconds)
        {
            if (IsRestoreInProgress || !HasActiveEncounter || remainingSeconds <= 0f ||
                IsLastBreathConsumed(ActiveEncounterId))
            {
                return false;
            }

            LastBreathArmedRemainingSeconds = remainingSeconds;
            return true;
        }

        public bool TryMarkCampUsed(string runId)
        {
            if (string.IsNullOrWhiteSpace(runId) ||
                string.Equals(CampUsedRunId, runId, StringComparison.Ordinal))
            {
                return false;
            }

            CampUsedRunId = runId;
            return true;
        }

        public void ActivateCamp(
            string runId,
            int caveLevel,
            float remainingSeconds,
            float positionX,
            float positionY)
        {
            if (string.IsNullOrWhiteSpace(runId) || caveLevel < 1 || remainingSeconds <= 0f)
            {
                ClearActiveCamp();
                return;
            }

            ActiveCampRunId = runId;
            ActiveCampCaveLevel = caveLevel;
            ActiveCampRemainingSeconds = remainingSeconds;
            ActiveCampPositionX = positionX;
            ActiveCampPositionY = positionY;
        }

        public void DissolveCamp()
        {
            ClearActiveCamp();
        }

        public void AdvanceTime(float deltaSeconds)
        {
            if (deltaSeconds <= 0f || IsRestoreInProgress)
            {
                return;
            }

            if (LastBreathArmedRemainingSeconds > 0f)
            {
                LastBreathArmedRemainingSeconds =
                    Math.Max(0f, LastBreathArmedRemainingSeconds - deltaSeconds);
            }

            if (ActiveCampRemainingSeconds > 0f)
            {
                ActiveCampRemainingSeconds =
                    Math.Max(0f, ActiveCampRemainingSeconds - deltaSeconds);
                if (ActiveCampRemainingSeconds <= 0f)
                {
                    ClearActiveCamp();
                }
            }

            if (ActiveCavebornRemainingSeconds > 0f)
            {
                ActiveCavebornRemainingSeconds =
                    Math.Max(0f, ActiveCavebornRemainingSeconds - deltaSeconds);
                if (ActiveCavebornRemainingSeconds <= 0f)
                    ClearActiveCaveborn();
            }
        }

        public bool TryActivateCaveborn(string runId, int rank, bool isInCombat)
        {
            rank = CavebornCapstoneResolver.ClampRank(rank);
            if (IsRestoreInProgress || rank <= 0 || string.IsNullOrWhiteSpace(runId) ||
                string.Equals(CavebornConsumedRunId, runId, StringComparison.Ordinal))
                return false;

            CavebornConsumedRunId = runId;
            ActiveCavebornRunId = runId;
            ActiveCavebornRank = rank;
            ActiveCavebornRemainingSeconds = CavebornCapstoneResolver.DurationSeconds(rank);
            CavebornWasInCombat = isInCombat;
            CavebornRegenDoubled = false;
            return true;
        }

        public bool ObserveCavebornCombatState(bool isInCombat)
        {
            if (!IsCavebornActive) return false;
            bool exitedCombat = CavebornWasInCombat && !isInCombat;
            CavebornWasInCombat = isInCombat;
            if (exitedCombat && ActiveCavebornRank >= 3)
                CavebornRegenDoubled = true;
            return exitedCombat;
        }

        public void ClearActiveCaveborn()
        {
            ActiveCavebornRunId = string.Empty;
            ActiveCavebornRank = 0;
            ActiveCavebornRemainingSeconds = 0f;
            CavebornWasInCombat = false;
            CavebornRegenDoubled = false;
        }

        public void OnCavebornRespec() => ClearActiveCaveborn();

        public void BeginTransientChannel(string actionId)
        {
            PendingChannelActionId = actionId ?? string.Empty;
        }

        public void CancelTransientChannel()
        {
            PendingChannelActionId = string.Empty;
        }

        public void BeginRestore()
        {
            IsRestoreInProgress = true;
            CancelTransientChannel();
        }

        public void EndRestore()
        {
            IsRestoreInProgress = false;
        }

        public SurvivalSkillSaveData CaptureSaveData()
        {
            var data = new SurvivalSkillSaveData
            {
                ActiveRunId = ActiveRunId,
                ActiveCaveLevel = ActiveCaveLevel,
                ActiveEncounterId = ActiveEncounterId,
                ActiveEncounterOrdinal = ActiveEncounterOrdinal,
                EncounterOrdinalRunId = EncounterOrdinalRunId,
                NextEncounterOrdinal = NextEncounterOrdinal,
                EncounterQuietRemainingSeconds = EncounterQuietRemainingSeconds,
                LunarConsumedEncounterId = LunarConsumedEncounterId,
                LunarConsumedFirstTargetInstanceId = LunarConsumedFirstTargetInstanceId,
                ActiveLunarTargetInstanceId = ActiveLunarTargetInstanceId,
                ActiveLunarVariant = ActiveLunarVariant,
                ActiveLunarRank = ActiveLunarRank,
                ActiveLunarRemainingSeconds = ActiveLunarRemainingSeconds,
                ActiveLunarActionToken = ActiveLunarActionToken,
                LastOffensiveMagicDamageType = (int)LastOffensiveMagicDamageType,
                LastBreathArmedRemainingSeconds = LastBreathArmedRemainingSeconds,
                CampUsedRunId = CampUsedRunId,
                ActiveCampRunId = ActiveCampRunId,
                ActiveCampCaveLevel = ActiveCampCaveLevel,
                ActiveCampRemainingSeconds = ActiveCampRemainingSeconds,
                ActiveCampPositionX = ActiveCampPositionX,
                ActiveCampPositionY = ActiveCampPositionY,
                CavebornConsumedRunId = CavebornConsumedRunId,
                ActiveCavebornRunId = ActiveCavebornRunId,
                ActiveCavebornRank = ActiveCavebornRank,
                ActiveCavebornRemainingSeconds = ActiveCavebornRemainingSeconds,
                CavebornWasInCombat = CavebornWasInCombat,
                CavebornRegenDoubled = CavebornRegenDoubled
            };

            data.ActiveEnemyInstanceIds.AddRange(_activeEnemyInstanceIds);
            data.ActiveEnemyInstanceIds.Sort(StringComparer.Ordinal);
            data.LeashedEnemyInstanceIds.AddRange(_leashedEnemyInstanceIds);
            data.LeashedEnemyInstanceIds.Sort(StringComparer.Ordinal);
            data.ConsumedLastBreathEncounterIds.AddRange(_consumedLastBreathEncounterIds);
            data.ConsumedLastBreathEncounterIds.Sort(StringComparer.Ordinal);
            return data;
        }

        public void RestoreFromSaveData(
            SurvivalSkillSaveData data,
            string expectedRunId,
            int expectedCaveLevel)
        {
            CancelTransientChannel();
            ResetPersistentState();
            if (data == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(expectedRunId) &&
                string.Equals(data.EncounterOrdinalRunId, expectedRunId, StringComparison.Ordinal))
            {
                EncounterOrdinalRunId = data.EncounterOrdinalRunId;
                NextEncounterOrdinal = Math.Max(FirstEncounterOrdinal, data.NextEncounterOrdinal);
            }
            CampUsedRunId = data.CampUsedRunId ?? string.Empty;
            CavebornConsumedRunId = data.CavebornConsumedRunId ?? string.Empty;

            if (data.ConsumedLastBreathEncounterIds != null)
            {
                foreach (string encounterId in data.ConsumedLastBreathEncounterIds)
                {
                    if (!string.IsNullOrWhiteSpace(encounterId))
                    {
                        _consumedLastBreathEncounterIds.Add(encounterId);
                    }
                }
            }

            bool sameContext = !string.IsNullOrWhiteSpace(expectedRunId) &&
                string.Equals(data.ActiveRunId, expectedRunId, StringComparison.Ordinal) &&
                data.ActiveCaveLevel == expectedCaveLevel;
            if (sameContext && !string.IsNullOrWhiteSpace(data.ActiveEncounterId))
            {
                EncounterOrdinalRunId = expectedRunId;
                ActiveRunId = data.ActiveRunId;
                ActiveCaveLevel = data.ActiveCaveLevel;
                ActiveEncounterId = data.ActiveEncounterId;
                ActiveEncounterOrdinal = Math.Max(FirstEncounterOrdinal, data.ActiveEncounterOrdinal);
                NextEncounterOrdinal = Math.Max(NextEncounterOrdinal, ActiveEncounterOrdinal + 1);

                if (data.ActiveEnemyInstanceIds != null)
                {
                    foreach (string enemyInstanceId in data.ActiveEnemyInstanceIds)
                    {
                        if (!string.IsNullOrWhiteSpace(enemyInstanceId))
                        {
                            _activeEnemyInstanceIds.Add(enemyInstanceId);
                        }
                    }
                }

                if (_activeEnemyInstanceIds.Count == 0)
                {
                    ClearActiveEncounter();
                }
                else
                {
                    if (data.LeashedEnemyInstanceIds != null)
                    {
                        foreach (string enemyInstanceId in data.LeashedEnemyInstanceIds)
                        {
                            if (_activeEnemyInstanceIds.Contains(enemyInstanceId))
                            {
                                _leashedEnemyInstanceIds.Add(enemyInstanceId);
                            }
                        }
                    }

                    if (_leashedEnemyInstanceIds.Count == _activeEnemyInstanceIds.Count)
                    {
                        EncounterQuietRemainingSeconds =
                            Math.Max(0f, data.EncounterQuietRemainingSeconds);
                    }

                    if (!IsLastBreathConsumed(ActiveEncounterId))
                    {
                        LastBreathArmedRemainingSeconds =
                            Math.Max(0f, data.LastBreathArmedRemainingSeconds);
                    }


                    if (string.Equals(data.LunarConsumedEncounterId, ActiveEncounterId,
                            StringComparison.Ordinal) &&
                        !string.IsNullOrWhiteSpace(data.LunarConsumedFirstTargetInstanceId))
                    {
                        LunarConsumedEncounterId = data.LunarConsumedEncounterId;
                        LunarConsumedFirstTargetInstanceId =
                            data.LunarConsumedFirstTargetInstanceId;
                    }

                    bool restoresActiveLunar =
                        string.Equals(data.ActiveLunarTargetInstanceId,
                            LunarConsumedFirstTargetInstanceId, StringComparison.Ordinal) &&
                        _activeEnemyInstanceIds.Contains(data.ActiveLunarTargetInstanceId) &&
                        RangedLunarRules.IsKnownVariant(data.ActiveLunarVariant) &&
                        data.ActiveLunarRank > 0 && data.ActiveLunarRemainingSeconds > 0f;
                    if (restoresActiveLunar)
                    {
                        ActiveLunarTargetInstanceId = data.ActiveLunarTargetInstanceId;
                        ActiveLunarVariant = data.ActiveLunarVariant;
                        ActiveLunarRank = Math.Max(1, Math.Min(3, data.ActiveLunarRank));
                        ActiveLunarRemainingSeconds = Math.Min(
                            RangedLunarRules.MaxFocusDurationSeconds,
                            data.ActiveLunarRemainingSeconds);
                        ActiveLunarActionToken = data.ActiveLunarActionToken ?? string.Empty;
                    }

                    if (Enum.IsDefined(typeof(DamageType), data.LastOffensiveMagicDamageType))
                        LastOffensiveMagicDamageType =
                            (DamageType)data.LastOffensiveMagicDamageType;
                }
            }

            bool sameCampContext = !string.IsNullOrWhiteSpace(expectedRunId) &&
                string.Equals(data.ActiveCampRunId, expectedRunId, StringComparison.Ordinal) &&
                data.ActiveCampCaveLevel == expectedCaveLevel &&
                data.ActiveCampRemainingSeconds > 0f;
            if (sameCampContext)
            {
                ActivateCamp(
                    data.ActiveCampRunId,
                    data.ActiveCampCaveLevel,
                    data.ActiveCampRemainingSeconds,
                    data.ActiveCampPositionX,
                    data.ActiveCampPositionY);
            }

            bool sameCavebornRun = !string.IsNullOrWhiteSpace(expectedRunId) &&
                string.Equals(data.ActiveCavebornRunId, expectedRunId, StringComparison.Ordinal) &&
                string.Equals(data.CavebornConsumedRunId, expectedRunId, StringComparison.Ordinal) &&
                data.ActiveCavebornRemainingSeconds > 0f && data.ActiveCavebornRank > 0;
            if (sameCavebornRun)
            {
                ActiveCavebornRunId = expectedRunId;
                ActiveCavebornRank = CavebornCapstoneResolver.ClampRank(data.ActiveCavebornRank);
                ActiveCavebornRemainingSeconds = Math.Min(
                    CavebornCapstoneResolver.DurationSeconds(ActiveCavebornRank),
                    Math.Max(0f, data.ActiveCavebornRemainingSeconds));
                CavebornWasInCombat = data.CavebornWasInCombat;
                CavebornRegenDoubled = ActiveCavebornRank >= 3 && data.CavebornRegenDoubled;
            }
        }

        public static string ComposeEncounterId(string runId, int caveLevel, int ordinal)
        {
            if (string.IsNullOrWhiteSpace(runId))
            {
                throw new ArgumentException("RunId is required.", nameof(runId));
            }

            if (caveLevel < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(caveLevel));
            }

            if (ordinal < FirstEncounterOrdinal)
            {
                throw new ArgumentOutOfRangeException(nameof(ordinal));
            }

            return $"{runId}|level:{caveLevel}|encounter:{ordinal}";
        }

        private void ResetPersistentState()
        {
            ClearActiveEncounter();
            _consumedLastBreathEncounterIds.Clear();
            NextEncounterOrdinal = FirstEncounterOrdinal;
            EncounterOrdinalRunId = string.Empty;
            CampUsedRunId = string.Empty;
            ClearActiveCamp();
            CavebornConsumedRunId = string.Empty;
            ClearActiveCaveborn();
        }

        private void ClearActiveEncounter()
        {
            ActiveRunId = string.Empty;
            ActiveCaveLevel = 0;
            ActiveEncounterId = string.Empty;
            ActiveEncounterOrdinal = 0;
            LastBreathArmedRemainingSeconds = 0f;
            EncounterQuietRemainingSeconds = 0f;
            _activeEnemyInstanceIds.Clear();
            _leashedEnemyInstanceIds.Clear();
            LunarConsumedEncounterId = string.Empty;
            LunarConsumedFirstTargetInstanceId = string.Empty;
            ClearActiveLunarFocus();
            LastOffensiveMagicDamageType = default;
        }

        private bool TryStartQuietResolution(float quietSeconds)
        {
            if (quietSeconds <= 0f || _activeEnemyInstanceIds.Count == 0 ||
                _leashedEnemyInstanceIds.Count != _activeEnemyInstanceIds.Count ||
                EncounterQuietRemainingSeconds > 0f)
            {
                return false;
            }

            EncounterQuietRemainingSeconds = quietSeconds;
            return true;
        }

        private void ClearActiveCamp()
        {
            ActiveCampRunId = string.Empty;
            ActiveCampCaveLevel = 0;
            ActiveCampRemainingSeconds = 0f;
            ActiveCampPositionX = 0f;
            ActiveCampPositionY = 0f;
        }

    }
}
