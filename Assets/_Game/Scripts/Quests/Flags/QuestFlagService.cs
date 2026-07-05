using System.Collections.Generic;

namespace CindarsHope.Quests.Flags
{
    public class QuestFlagSetResult
    {
        public bool Success { get; set; }
        public string FlagId { get; set; }
        public string FailReason { get; set; }
        public bool WasAlreadySet { get; set; }

        public static QuestFlagSetResult Fail(string flagId, string reason) =>
            new QuestFlagSetResult { Success = false, FlagId = flagId, FailReason = reason };
    }

    public class QuestFlagService
    {
        private readonly QuestFlagRegistry _registry;
        // flagId → serialized string value
        private readonly Dictionary<string, string> _activeFlags = new Dictionary<string, string>();
        // tracks all flag ids set via GrantedFlagIds (idempotency for rewards)
        private readonly HashSet<string> _grantedFlagIds = new HashSet<string>();

        public QuestFlagService(QuestFlagRegistry registry)
        {
            _registry = registry;
        }

        /// <summary>Registers a stable flag authored by a dynamic quest reward.</summary>
        public void EnsureRewardFlagRegistered(string flagId, string ownerSystem = "QuestRuntime")
        {
            if (string.IsNullOrEmpty(flagId) || _registry == null || _registry.IsRegistered(flagId)) return;
            _registry.Register(new QuestFlagDefinition
            {
                FlagId = flagId,
                OwnerSystem = ownerSystem,
                Scope = QuestFlagScope.QuestLocal,
                Visibility = QuestFlagVisibility.HiddenInternal,
                CanAppearInQuestLog = false,
                CanBeGrantedByReward = true,
                Persists = true
            });
        }

        public QuestFlagSetResult SetFlag(string flagId, string value, string setterSystem)
        {
            if (!_registry.TryGet(flagId, out var def))
                return QuestFlagSetResult.Fail(flagId, $"Flag '{flagId}' not registered");

            if (def.IsDeprecated)
                return QuestFlagSetResult.Fail(flagId, $"Flag '{flagId}' is deprecated");

            if (def.AllowedSetters.Count > 0 && !def.AllowedSetters.Contains(setterSystem))
                return QuestFlagSetResult.Fail(flagId, $"Setter '{setterSystem}' not authorized for flag '{flagId}'");

            bool alreadySet = _activeFlags.ContainsKey(flagId);
            _activeFlags[flagId] = value ?? def.DefaultValue;
            return new QuestFlagSetResult { Success = true, FlagId = flagId, WasAlreadySet = alreadySet };
        }

        public QuestFlagSetResult GrantFlag(string flagId, string setterSystem)
        {
            // idempotent: second grant is success but WasAlreadySet=true
            _grantedFlagIds.Add(flagId);
            return SetFlag(flagId, "true", setterSystem);
        }

        public QuestFlagSetResult ClearFlag(string flagId, string clearerSystem)
        {
            if (!_registry.TryGet(flagId, out var def))
                return QuestFlagSetResult.Fail(flagId, $"Flag '{flagId}' not registered");

            if (def.AllowedClearers.Count > 0 && !def.AllowedClearers.Contains(clearerSystem))
                return QuestFlagSetResult.Fail(flagId, $"Clearer '{clearerSystem}' not authorized for flag '{flagId}'");

            _activeFlags.Remove(flagId);
            return new QuestFlagSetResult { Success = true, FlagId = flagId };
        }

        public bool IsSet(string flagId) => _activeFlags.ContainsKey(flagId);

        public string GetValue(string flagId) =>
            _activeFlags.TryGetValue(flagId, out var val) ? val : null;

        public bool WasGranted(string flagId) => _grantedFlagIds.Contains(flagId);

        public bool IsVisibleToUi(string flagId)
        {
            if (!_registry.TryGet(flagId, out var def)) return false;
            return !def.IsHidden() && def.CanAppearInQuestLog;
        }

        public IReadOnlyDictionary<string, string> GetAllActive() => _activeFlags;
    }
}
