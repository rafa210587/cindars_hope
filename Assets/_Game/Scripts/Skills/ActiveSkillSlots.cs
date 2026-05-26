using System;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Skills
{
    [Serializable]
    public class ActiveSkillSlot
    {
        public KeyCode InputKey;
        public string SkillActionId;
        public float CooldownRemaining;

        public ActiveSkillSlot(KeyCode key, string skillId = null)
        {
            InputKey = key;
            SkillActionId = skillId;
            CooldownRemaining = 0f;
        }

        public bool IsReady => CooldownRemaining <= 0f;
    }

    [DisallowMultipleComponent]
    public class ActiveSkillSlots : MonoBehaviour
    {
        [SerializeField] private SkillActionExecutor _skillExecutor;
        private ActiveSkillSlot[] _slots = new ActiveSkillSlot[4];

        private static readonly KeyCode[] SlotKeys = { KeyCode.R, KeyCode.T, KeyCode.Y, KeyCode.G };

        private void Awake()
        {
            for (int i = 0; i < SlotKeys.Length; i++)
                _slots[i] = new ActiveSkillSlot(SlotKeys[i]);

            if (_skillExecutor == null)
                _skillExecutor = GetComponent<SkillActionExecutor>();

            if (_skillExecutor == null)
                Debug.LogWarning("ActiveSkillSlots: SkillActionExecutor not found.", this);
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<ActiveSkillSlotAssignedEvent>(OnSlotAssigned);
            GameEventBus.Subscribe<ActiveSkillSlotClearedEvent>(OnSlotCleared);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<ActiveSkillSlotAssignedEvent>(OnSlotAssigned);
            GameEventBus.Unsubscribe<ActiveSkillSlotClearedEvent>(OnSlotCleared);
        }

        private void Update()
        {
            var modalManager = GameBootstrap.Instance?.ModalManager;
            bool isModalActive = modalManager != null && modalManager.HasActiveModal;

            foreach (var slot in _slots)
            {
                if (slot.CooldownRemaining > 0)
                    slot.CooldownRemaining -= Time.deltaTime;

                if (!isModalActive && Input.GetKeyDown(slot.InputKey))
                    TryActivateSlot(slot);
            }
        }

        private void TryActivateSlot(ActiveSkillSlot slot)
        {
            if (!slot.IsReady || string.IsNullOrEmpty(slot.SkillActionId)) return;

            if (_skillExecutor == null)
            {
                Debug.LogWarning("ActiveSkillSlots: SkillActionExecutor not found.", this);
                return;
            }

            bool success = _skillExecutor.TryExecuteSkill(slot.SkillActionId);
            if (success)
                slot.CooldownRemaining = 1f;
        }

        // Set a skill into a slot with validation against SkillTreeManager
        public bool TrySetSkillInSlot(int slotIndex, string skillActionId)
        {
            if (slotIndex < 0 || slotIndex >= _slots.Length) return false;

            var skillMgr = GameBootstrap.Instance?.SkillTreeManager;
            if (skillMgr != null && !string.IsNullOrEmpty(skillActionId))
            {
                bool isUnlocked = false;
                foreach (var node in skillMgr.NodeIndex.Values)
                {
                    if (node.UnlockedSkillActionId == skillActionId
                        && node.SkillCategory == SkillCategory.EquippableSkill
                        && skillMgr.IsNodePurchased(node.SkillNodeId))
                    {
                        isUnlocked = true;
                        break;
                    }
                }

                if (!isUnlocked)
                {
                    Debug.LogWarning($"ActiveSkillSlots: SkillAction '{skillActionId}' is not unlocked, slot not set.", this);
                    return false;
                }
            }

            _slots[slotIndex].SkillActionId = skillActionId;
            return true;
        }

        // Legacy method kept for compatibility
        public bool SetSkillInSlot(int slotIndex, string skillActionId, float cooldownOverride = 0f)
        {
            if (slotIndex < 0 || slotIndex >= _slots.Length) return false;
            _slots[slotIndex].SkillActionId = skillActionId;
            _slots[slotIndex].CooldownRemaining = cooldownOverride;
            GameEventBus.Publish(new ActiveSkillSlotChangedEvent(slotIndex, skillActionId));
            return true;
        }

        public ActiveSkillSlot GetSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _slots.Length) return null;
            return _slots[slotIndex];
        }

        private void OnSlotAssigned(ActiveSkillSlotAssignedEvent evt)
        {
            if (evt.SlotIndex < 0 || evt.SlotIndex >= _slots.Length) return;
            _slots[evt.SlotIndex].SkillActionId = evt.SkillActionId;
        }

        private void OnSlotCleared(ActiveSkillSlotClearedEvent evt)
        {
            if (evt.SlotIndex < 0 || evt.SlotIndex >= _slots.Length) return;
            _slots[evt.SlotIndex].SkillActionId = null;
        }
    }
}
