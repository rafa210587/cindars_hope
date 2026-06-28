using CindarsHope.Economy;

namespace CindarsHope.Save.Providers
{
    /// <summary>
    /// Provider de save do estoque das lojas (economia). Fonte do estado: <see cref="ShopManager"/>
    /// injetado via constructor. Fallback: seção existente do save ou estoque vazio quando o manager
    /// não está disponível (preservação entre cenas).
    /// </summary>
    public class EconomySectionProvider : ISaveSectionProvider
    {
        private readonly ShopManager _shopManager;

        public EconomySectionProvider(ShopManager shopManager)
        {
            _shopManager = shopManager;
        }

        public string ProviderId => "economy";

        public object Capture(GameSaveData existingSaveData)
        {
            var economyData = new EconomySaveData();

            if (_shopManager != null)
            {
                economyData.Shops = _shopManager.CaptureAllShopStock();
            }
            else if (existingSaveData?.Economy != null)
            {
                economyData.Shops = existingSaveData.Economy.Shops;
            }

            return economyData;
        }

        public void Restore(object sectionData)
        {
            if (_shopManager == null)
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
                _shopManager.LoadShopStock(shopStockData);
            }
        }
    }
}
