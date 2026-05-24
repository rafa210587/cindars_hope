using System;
using CindarsHope.Combat;
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
        private ActiveSkillSlot[] _slots = new ActiveSkillSlot[4];

        private void Awake()
        {
            _slots[0] = new ActiveSkillSlot(KeyCode.R);
            _slots[1] = new ActiveSkillSlot(KeyCode.T);
            _slots[2] = new ActiveSkillSlot(KeyCode.Y);
            _slots[3] = new ActiveSkillSlot(KeyCode.G);
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

            Debug.Log($"Activating skill: {slot.SkillActionId}");
        }

        public bool SetSkillInSlot(int slotIndex, string skillActionId, float cooldown)
        {
            if (slotIndex < 0 || slotIndex >= _slots.Length)
                return false;

            _slots[slotIndex].SkillActionId = skillActionId;
            _slots[slotIndex].CooldownRemaining = cooldown;
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
