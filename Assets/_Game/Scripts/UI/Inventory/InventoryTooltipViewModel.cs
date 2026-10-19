namespace CindarsHope.UI.Inventory
{
    /// <summary>
    /// SPEC 04: Inventory item tooltip projection for hover display.
    /// </summary>
    public class InventoryItemTooltip
    {
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public string Rarity { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; }
        public string[] Properties { get; set; }
        public string Value { get; set; }
        public bool IsConsumable { get; set; }
        public bool IsEquipment { get; set; }
        public bool IsQuestItem { get; set; }
    }

    public class InventoryListViewModel
    {
        public string[] ItemIds { get; set; }
        public int SelectedIndex { get; set; } = 0;

        public string GetSelectedItemId()
        {
            if (ItemIds != null && SelectedIndex >= 0 && SelectedIndex < ItemIds.Length)
                return ItemIds[SelectedIndex];
            return null;
        }

        public void SelectNext()
        {
            if (ItemIds != null && ItemIds.Length > 0)
                SelectedIndex = (SelectedIndex + 1) % ItemIds.Length;
        }

        public void SelectPrevious()
        {
            if (ItemIds != null && ItemIds.Length > 0)
                SelectedIndex = (SelectedIndex - 1 + ItemIds.Length) % ItemIds.Length;
        }
    }
}
