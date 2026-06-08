namespace CindarsHope.Economy.Validation
{
    public enum RestockPolicy { None = 0, Daily, Weekly, Seasonal, EventTriggered, ProgressTriggered, Unique, Limited, Rotating }

    public class AntiArbitrageCase
    {
        public string ItemId { get; set; }
        public string BuyChannel { get; set; }
        public string SellChannel { get; set; }
        public int BuyPrice { get; set; }
        public int SellPrice { get; set; }
        public int StockLimit { get; set; } = -1;
        public RestockPolicy RestockPolicy { get; set; } = RestockPolicy.None;
        public string ExceptionReason { get; set; }
        public bool IsBoundedException { get; set; } = false;

        public bool IsArbitrage => !IsBoundedException && SellPrice >= BuyPrice;
    }
}
