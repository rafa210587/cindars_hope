using System;
using CindarsHope.Core;
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

    // fable_29 (emenda V3 item 7) — RETIRED legacy active-slot path (keys R/T/Y/G).
    //
    // The live, canonical active-slot path is ActiveSkillExecutionController (keys 1-4, patch
    // WI-11). This component no longer reads input or executes skills — its R/T/Y/G handling is
    // removed so it cannot compete with the 1-4 controller. The type and its slot/save accessors
    // are kept ONLY so the existing SaveManager reference and the legacy ActiveSkillSlots save
    // section keep compiling/round-tripping. Do not wire new active skills here.
    //
    // Retirement is BEHAVIORAL (no Update() input, no executor call) rather than via [Obsolete],
    // so the out-of-scope SaveManager that still references this type does not emit CS0618.
    [DisallowMultipleComponent]
    public class ActiveSkillSlots : MonoBehaviour
    {
        [SerializeField] private SkillActionExecutor _skillExecutor;
        private ActiveSkillSlot[] _slots = new ActiveSkillSlot[4];

        // Retained for save/state continuity; NO LONGER bound to runtime input (item 7).
        private static readonly KeyCode[] SlotKeys = { KeyCode.None, KeyCode.None, KeyCode.None, KeyCode.None };

        private void Awake()
        {
            for (int i = 0; i < SlotKeys.Length; i++)
                _slots[i] = new ActiveSkillSlot(SlotKeys[i]);
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

        // fable_29 (item 7): no Update() input. The keys-1-4 controller owns active-skill input.

        // Set a skill into a slot with validation against SkillTreeManager
        public bool TrySetSkillInSlot(int slotIndex, string skillActionId)
        {
            if (slotIndex < 0 || slotIndex >= _slots.Length) return false;

            var skillMgr = SkillTreeManager.Instance;
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
