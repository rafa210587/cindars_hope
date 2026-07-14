using System.Collections.Generic;

namespace CindarsHope.Foundation
{
    public interface IShopStockRuntime
    {
        List<ShopStockSaveData> CaptureAllShopStock();
        void LoadShopStock(ShopStockSaveData stockData);
    }
}
