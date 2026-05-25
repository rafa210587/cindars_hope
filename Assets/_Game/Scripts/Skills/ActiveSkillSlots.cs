using System;
using CindarsHope.Core;
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

        private void Awake()
        {
            _slots[0] = new ActiveSkillSlot(KeyCode.R);
            _slots[1] = new ActiveSkillSlot(KeyCode.T);
            _slots[2] = new ActiveSkillSlot(KeyCode.Y);
            _slots[3] = new ActiveSkillSlot(KeyCode.G);

            if (_skillExecutor == null)
            {
                _skillExecutor = GetComponent<SkillActionExecutor>();
            }
            if (_skillExecutor == null)
            {
                _skillExecutor = FindFirstObjectByType<SkillActionExecutor>();
            }
        }

        private void Update()
        {
            foreach (var slot in _slots)
            {
                if (slot.CooldownRemaining > 0)
                {
                    slot.CooldownRemaining -= Time.deltaTime;
                }

                if (Input.GetKeyDown(slot.InputKey))
                {
                    TryActivateSlot(slot);
                }
            }
        }

        private void TryActivateSlot(ActiveSkillSlot slot)
        {
            if (!slot.IsReady || string.IsNullOrEmpty(slot.SkillActionId))
                return;

            if (_skillExecutor != null)
            {
                bool success = _skillExecutor.TryExecuteSkill(slot.SkillActionId);
                if (success)
                {
                    slot.CooldownRemaining = 1f;
                }
            }
            else
            {
                Debug.LogWarning("ActiveSkillSlots: SkillActionExecutor not found", this);
            }
        }

        public bool SetSkillInSlot(int slotIndex, string skillActionId, float cooldown)
        {
            if (slotIndex < 0 || slotIndex >= _slots.Length)
                return false;

            _slots[slotIndex].SkillActionId = skillActionId;
            _slots[slotIndex].CooldownRemaining = cooldown;
            GameEventBus.Publish(new CindarsHope.Core.Events.ActiveSkillSlotChangedEvent(slotIndex, skillActionId));
            return true;
        }

        public ActiveSkillSlot GetSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _slots.Length)
                return null;

            return _slots[slotIndex];
        }
    }
}
