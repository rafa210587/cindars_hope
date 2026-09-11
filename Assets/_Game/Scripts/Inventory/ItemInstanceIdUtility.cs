using System;

namespace CindarsHope.Inventory
{
    /// <summary>Canonical conversion between an individual inventory id and its content id.</summary>
    public static class ItemInstanceIdUtility
    {
        public const char Separator = '#';

        public static bool IsForItem(string itemId, string itemInstanceId)
        {
            return !string.IsNullOrWhiteSpace(itemId)
                && !string.IsNullOrWhiteSpace(itemInstanceId)
                && itemInstanceId.StartsWith(itemId + Separator, StringComparison.Ordinal);
        }

        public static string GetItemId(string itemInstanceId)
        {
            if (string.IsNullOrWhiteSpace(itemInstanceId)) return string.Empty;
            var separator = itemInstanceId.IndexOf(Separator);
            return separator > 0 ? itemInstanceId.Substring(0, separator) : itemInstanceId;
        }
    }
}
