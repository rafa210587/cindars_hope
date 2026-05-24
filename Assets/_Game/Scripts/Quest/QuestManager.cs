using System;
using System.Collections.Generic;
using System.Linq;
using CindarsHope.Core;
using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Quest
{
    [DisallowMultipleComponent]
    public class QuestManager : MonoBehaviour
    {
        [SerializeField] private QuestDatabaseSO _questDatabase;

        private Dictionary<string, QuestSaveData> _quests = new();
        private bool _isInitialized;

        public bool IsInitialized => _isInitialized;

        public void Initialize()
        {
            if (_isInitialized)
                return;

            if (_questDatabase == null)
            {
                Debug.LogError("QuestManager: Quest database not assigned");
                return;
            }

            _quests.Clear();
            _isInitialized = true;
        }

        public bool TryStartQuest(string questId)
        {
            if (!_isInitialized)
            {
                Debug.LogError("QuestManager not initialized");
                return false;
            }

            if (!_questDatabase.TryGetById(questId, out var questData))
            {
                Debug.LogWarning($"Quest {questId} not found in database");
                return false;
            }

            if (_quests.TryGetValue(questId, out var existingQuest))
            {
                if (existingQuest.IsCompleted || existingQuest.IsAbandoned)
                {
                    existingQuest.IsCompleted = false;
                    existingQuest.IsAbandoned = false;
                    existingQuest.Progress = 0;
                }
                else
                {
                    Debug.LogWarning($"Quest {questId} is already active");
                    return false;
                }
            }
            else
            {
                _quests[questId] = new QuestSaveData
                {
                    QuestId = questId,
                    IsCompleted = false,
                    IsAbandoned = false,
                    Progress = 0
                };
            }

            GameEventBus.Publish(new QuestStartedEvent(questId));
            return true;
        }

        public bool TryCompleteQuest(string questId)
        {
            if (!_quests.TryGetValue(questId, out var questData))
            {
                Debug.LogWarning($"Quest {questId} not found");
                return false;
            }

            if (questData.IsCompleted)
            {
                Debug.LogWarning($"Quest {questId} is already completed");
                return false;
            }

            if (!_questDatabase.TryGetById(questId, out var questDataSO))
            {
                Debug.LogWarning($"Quest {questId} not found in database");
                return false;
            }

            questData.IsCompleted = true;
            GameEventBus.Publish(new QuestCompletedEvent(questId, questDataSO.RewardXp, questDataSO.RewardGold));
            return true;
        }

        public bool TryAbandonQuest(string questId)
        {
            if (!_quests.TryGetValue(questId, out var questData))
            {
                Debug.LogWarning($"Quest {questId} not found");
                return false;
            }

            if (questData.IsAbandoned)
            {
                Debug.LogWarning($"Quest {questId} is already abandoned");
                return false;
            }

            questData.IsAbandoned = true;
            return true;
        }

        public bool IsQuestActive(string questId)
        {
            return _quests.TryGetValue(questId, out var questData) &&
                   !questData.IsCompleted && !questData.IsAbandoned;
        }

        public bool IsQuestCompleted(string questId)
        {
            return _quests.TryGetValue(questId, out var questData) && questData.IsCompleted;
        }

        public int GetQuestProgress(string questId)
        {
            return _quests.TryGetValue(questId, out var questData) ? questData.Progress : 0;
        }

        public void SetQuestProgress(string questId, int progress)
        {
            if (_quests.TryGetValue(questId, out var questData))
            {
                questData.Progress = Mathf.Max(0, progress);
            }
        }

        public QuestManagerSaveData CaptureSaveData()
        {
            var data = new QuestManagerSaveData();
            foreach (var kvp in _quests)
            {
                data.Quests.Add(kvp.Value);
            }
            return data;
        }

        public void LoadFromSaveData(QuestManagerSaveData saveData)
        {
            _quests.Clear();

            if (saveData?.Quests == null)
                return;

            foreach (var questData in saveData.Quests)
            {
                _quests[questData.QuestId] = questData;
            }
        }
    }

    [System.Serializable]
    public class QuestManagerSaveData
    {
        public List<QuestSaveData> Quests = new();
    }
}
