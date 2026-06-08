using System.Collections.Generic;

namespace CindarsHope.UI.Notifications
{
    public enum NotificationPriority
    {
        Low = 0, Normal = 1, Important = 2, CriticalModal = 3, DebugOnly = 4
    }

    public enum NotificationStackPolicy
    {
        NoStack = 0, StackCount = 1, MergeIdentical = 2, ReplaceLatest = 3
    }

    public class NotificationViewModel
    {
        public string NotificationId { get; set; }
        public string Type { get; set; }
        public NotificationPriority Priority { get; set; }
        public string TextKey { get; set; }
        public string IconId { get; set; }
        public float Duration { get; set; }
        public bool BlocksInput { get; set; }
        public bool RequiresConfirmation { get; set; }
        public int SpoilerTier { get; set; }
        public NotificationStackPolicy StackPolicy { get; set; }
        public List<string> DebugTags { get; set; } = new List<string>();

        public bool IsDebugOnly => Priority == NotificationPriority.DebugOnly;
        public bool IsAllowedInFinalHud => Priority != NotificationPriority.DebugOnly;
    }

    public class NotificationQueuePolicy
    {
        public int MaxVisibleLow { get; set; } = 3;
        public int MaxVisibleNormal { get; set; } = 5;
        public float DefaultLowDuration { get; set; } = 3f;
        public float DefaultNormalDuration { get; set; } = 5f;
        public float DefaultImportantDuration { get; set; } = 8f;

        // CriticalModal uses the modal/focus system — duration is indefinite until confirmed
        public bool CriticalModalUsesInputBlock { get; set; } = true;

        // DebugOnly notifications never appear in final HUD
        public bool DebugOnlyFilteredInFinalBuild { get; set; } = true;

        // Queue cap — prevents notification spam
        public int HardQueueCap { get; set; } = 20;

        public static NotificationQueuePolicy Default() => new NotificationQueuePolicy();

        public bool IsAllowedForFinalHud(NotificationViewModel notification) =>
            notification != null && notification.Priority != NotificationPriority.DebugOnly;

        public bool CanStack(NotificationViewModel a, NotificationViewModel b)
        {
            if (a == null || b == null) return false;
            if (a.Type != b.Type || a.TextKey != b.TextKey) return false;
            return a.StackPolicy == NotificationStackPolicy.StackCount
                || a.StackPolicy == NotificationStackPolicy.MergeIdentical;
        }
    }
}
