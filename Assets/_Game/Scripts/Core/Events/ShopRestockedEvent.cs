namespace CindarsHope.Core.Events
{
    public class ShopRestockedEvent
    {
        public string ShopId { get; }

        public ShopRestockedEvent(string shopId)
        {
            ShopId = shopId;
        }
    }
}
