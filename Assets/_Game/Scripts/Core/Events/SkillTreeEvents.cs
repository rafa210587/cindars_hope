namespace CindarsHope.Core.Events
{
    public readonly struct SkillPointGrantedEvent
    {
        public int Amount { get; }
        public int TotalAvailable { get; }
        public int PlayerLevel { get; }

        public SkillPointGrantedEvent(int amount, int totalAvailable, int playerLevel)
        {
            Amount = amount;
            TotalAvailable = totalAvailable;
            PlayerLevel = playerLevel;
        }
    }

    public readonly struct SkillNodePurchaseRequestedEvent
    {
        public string NodeId { get; }

        public SkillNodePurchaseRequestedEvent(string nodeId)
        {
            NodeId = nodeId;
        }
    }

    public readonly struct SkillNodePurchasedEvent
    {
        public string NodeId { get; }
        public string TreeId { get; }
        public int RemainingSkillPoints { get; }

        public SkillNodePurchasedEvent(string nodeId, string treeId, int remainingSkillPoints)
        {
            NodeId = nodeId;
            TreeId = treeId;
            RemainingSkillPoints = remainingSkillPoints;
        }
    }

    public readonly struct SkillPurchaseFailedEvent
    {
        public string NodeId { get; }
        public string Reason { get; }

        public SkillPurchaseFailedEvent(string nodeId, string reason)
        {
            NodeId = nodeId;
            Reason = reason;
        }
    }

    public readonly struct SkillPassiveAppliedEvent
    {
        public string NodeId { get; }

        public SkillPassiveAppliedEvent(string nodeId)
        {
            NodeId = nodeId;
        }
    }

    public readonly struct SkillPassiveRemovedEvent
    {
        public string NodeId { get; }

        public SkillPassiveRemovedEvent(string nodeId)
        {
            NodeId = nodeId;
        }
    }

    public readonly struct ActiveSkillSlotAssignRequestedEvent
    {
        public string SkillActionId { get; }
        public int SlotIndex { get; }

        public ActiveSkillSlotAssignRequestedEvent(string skillActionId, int slotIndex)
        {
            SkillActionId = skillActionId;
            SlotIndex = slotIndex;
        }
    }

    public readonly struct ActiveSkillSlotAssignedEvent
    {
        public int SlotIndex { get; }
        public string SkillActionId { get; }

        public ActiveSkillSlotAssignedEvent(int slotIndex, string skillActionId)
        {
            SlotIndex = slotIndex;
            SkillActionId = skillActionId;
        }
    }

    public readonly struct ActiveSkillSlotClearedEvent
    {
        public int SlotIndex { get; }

        public ActiveSkillSlotClearedEvent(int slotIndex)
        {
            SlotIndex = slotIndex;
        }
    }

    public readonly struct SkillTreeOpenedEvent { }

    public readonly struct SkillTreeClosedEvent { }

    public readonly struct SkillTreeRespecRequestedEvent { }

    public readonly struct SkillTreeRespecCompletedEvent
    {
        public int RestoredSkillPoints { get; }
        public int RespecCount { get; }

        public SkillTreeRespecCompletedEvent(int restoredSkillPoints, int respecCount)
        {
            RestoredSkillPoints = restoredSkillPoints;
            RespecCount = respecCount;
        }
    }

    public readonly struct SkillTreeRespecFailedEvent
    {
        public string Reason { get; }

        public SkillTreeRespecFailedEvent(string reason)
        {
            Reason = reason;
        }
    }

    public readonly struct SkillDerivedStatsChangedEvent { }
}
