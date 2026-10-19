using System.Collections.Generic;

namespace CindarsHope.Quests.Flags
{
    public class QuestFlagRegistryQuery
    {
        public string FlagId { get; set; }
        public QuestFlagType? ExpectedType { get; set; }
        public QuestFlagVisibility? RequiredVisibility { get; set; }
        public string RequestingSystem { get; set; }
        public bool AllowHidden { get; set; } = false;
    }

    public class QuestFlagRegistryResult
    {
        public bool Found { get; set; }
        public QuestFlagDefinition Definition { get; set; }
        public string FailReason { get; set; }

        public static QuestFlagRegistryResult NotFound(string reason) =>
            new QuestFlagRegistryResult { Found = false, FailReason = reason };
    }

    public class QuestFlagRegistry
    {
        private readonly Dictionary<string, QuestFlagDefinition> _flags = new Dictionary<string, QuestFlagDefinition>();

        public void Register(QuestFlagDefinition def)
        {
            if (def == null || string.IsNullOrEmpty(def.FlagId)) return;
            _flags[def.FlagId] = def;
        }

        public bool TryGet(string flagId, out QuestFlagDefinition def) => _flags.TryGetValue(flagId, out def);

        public QuestFlagRegistryResult Query(QuestFlagRegistryQuery query)
        {
            if (query == null) return QuestFlagRegistryResult.NotFound("query is null");
            if (!_flags.TryGetValue(query.FlagId, out var def))
                return QuestFlagRegistryResult.NotFound($"Flag '{query.FlagId}' not registered");

            if (def.IsDeprecated)
                return QuestFlagRegistryResult.NotFound($"Flag '{query.FlagId}' is deprecated; use '{def.ReplacementFlagId ?? "none"}'");

            if (!query.AllowHidden && def.IsHidden())
                return QuestFlagRegistryResult.NotFound($"Flag '{query.FlagId}' is hidden from '{query.RequestingSystem}'");

            if (query.ExpectedType.HasValue && def.FlagType != query.ExpectedType.Value)
                return QuestFlagRegistryResult.NotFound($"Flag '{query.FlagId}' type mismatch: expected {query.ExpectedType.Value}, got {def.FlagType}");

            return new QuestFlagRegistryResult { Found = true, Definition = def };
        }

        public IEnumerable<QuestFlagDefinition> GetAllVisible()
        {
            foreach (var def in _flags.Values)
                if (!def.IsHidden() && !def.IsDeprecated) yield return def;
        }

        public bool IsRegistered(string flagId) => _flags.ContainsKey(flagId);
        public int Count => _flags.Count;
    }
}
