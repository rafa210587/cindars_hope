using CindarsHope.Foundation;

namespace CindarsHope.Save.Providers
{
    /// <summary>
    /// Provider de save do estoque das lojas (economia). Fonte do estado: <see cref="IShopStockRuntime"/>
    /// injetado via constructor. Fallback: seção existente do save ou estoque vazio quando o manager
    /// não está disponível (preservação entre cenas).
    /// </summary>
    public class EconomySectionProvider : ISaveSectionProvider
    {
        private readonly IShopStockRuntime _shopStockRuntime;

        public EconomySectionProvider(IShopStockRuntime shopStockRuntime)
        {
            _shopStockRuntime = shopStockRuntime;
        }

        public string ProviderId => "economy";

        public object Capture(GameSaveData existingSaveData)
        {
            var economyData = new EconomySaveData();

            if (_shopStockRuntime != null)
            {
                economyData.Shops = _shopStockRuntime.CaptureAllShopStock();
            }
            else if (existingSaveData?.Economy != null)
            {
                economyData.Shops = existingSaveData.Economy.Shops;
            }

            return economyData;
        }

        public void Restore(object sectionData)
        {
            if (_shopStockRuntime == null)
            {
                return;
            }

            var data = sectionData as EconomySaveData;
            if (data?.Shops == null)
            {
                return;
            }

            foreach (var shopStockData in data.Shops)
            {
                _shopStockRuntime.LoadShopStock(shopStockData);
            }
        }
    }
}
