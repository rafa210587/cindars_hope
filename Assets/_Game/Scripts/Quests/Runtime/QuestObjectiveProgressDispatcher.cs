using System;
using CindarsHope.Quests.Save;

namespace CindarsHope.Quests.Runtime
{
    public enum QuestProgressApplication
    {
        RecheckInventory = 0,
        Complete = 1,
        Increment = 2
    }

    public readonly struct QuestProgressSignal
    {
        public readonly QuestObjectiveType ObjectiveType;
        public readonly QuestProgressApplication Application;
        public readonly string PrimaryTargetId;
        public readonly string SecondaryTargetId;
        public readonly int NumericValue;
        public readonly int Amount;

        public QuestProgressSignal(QuestObjectiveType objectiveType,
            QuestProgressApplication application, string primaryTargetId,
            string secondaryTargetId = null, int numericValue = 0, int amount = 1)
        {
            ObjectiveType = objectiveType;
            Application = application;
            PrimaryTargetId = primaryTargetId;
            SecondaryTargetId = secondaryTargetId;
            NumericValue = numericValue;
            Amount = amount < 1 ? 1 : amount;
        }
    }

    /// <summary>Pure matching rules shared by every quest progress event adapter.</summary>
    public static class QuestObjectiveProgressMatcher
    {
        public static bool Matches(QuestObjective objective, QuestProgressSignal signal)
        {
            if (objective == null || objective.ObjectiveType != signal.ObjectiveType) return false;
            string target = objective.TargetId;
            if (string.IsNullOrEmpty(target)) return false;
            if (target == "any") return true;

            if (signal.ObjectiveType == QuestObjectiveType.ReachCaveDepth)
            {
                if (target == signal.PrimaryTargetId) return true;
                return int.TryParse(target, out var requiredDepth) && requiredDepth <= signal.NumericValue;
            }

            return target == signal.PrimaryTargetId ||
                   (!string.IsNullOrEmpty(signal.SecondaryTargetId) && target == signal.SecondaryTargetId);
        }
    }

    /// <summary>
    /// Single allocation-free traversal for typed quest progress. The service owns state mutation;
    /// this dispatcher owns selection and routing only.
    /// </summary>
    public sealed class QuestObjectiveProgressDispatcher
    {
        private readonly QuestRegistry _registry;
        private readonly QuestStateSection _saveSection;
        private readonly Action<string> _recheckInventory;
        private readonly Action<string, string> _complete;
        private readonly Action<string, string, int> _increment;

        public QuestObjectiveProgressDispatcher(QuestRegistry registry, QuestStateSection saveSection,
            Action<string> recheckInventory, Action<string, string> complete,
            Action<string, string, int> increment)
        {
            _registry = registry;
            _saveSection = saveSection;
            _recheckInventory = recheckInventory;
            _complete = complete;
            _increment = increment;
        }

        public void Dispatch(QuestProgressSignal signal)
        {
            if (_registry == null || _saveSection == null) return;

            foreach (QuestStateRecord record in _saveSection.QuestStates)
            {
                if (record == null) continue;
                var status = (QuestStateStatus)record.State;
                bool acceptsSignal = status == QuestStateStatus.Active ||
                    (signal.Application == QuestProgressApplication.RecheckInventory &&
                     status == QuestStateStatus.ReadyToComplete);
                if (!acceptsSignal) continue;
                var objectives = _registry.GetObjectives(record.QuestId);
                for (int index = 0; index < objectives.Count; index++)
                {
                    QuestObjective objective = objectives[index];
                    if (!QuestObjectiveProgressMatcher.Matches(objective, signal)) continue;

                    switch (signal.Application)
                    {
                        case QuestProgressApplication.RecheckInventory:
                            _recheckInventory?.Invoke(record.QuestId);
                            index = objectives.Count;
                            break;
                        case QuestProgressApplication.Complete:
                            _complete?.Invoke(record.QuestId, objective.ObjectiveId);
                            break;
                        case QuestProgressApplication.Increment:
                            _increment?.Invoke(record.QuestId, objective.ObjectiveId, signal.Amount);
                            break;
                    }
                }
            }
        }
    }
}
