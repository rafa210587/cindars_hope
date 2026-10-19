namespace CindarsHope.NPC
{
    /// <summary>
    /// Keeps civic city-service dialogue rules out of the shop controller while preserving the
    /// existing CityServiceCatalog/CityServiceAccess ownership for labels, idempotency and purchase.
    /// </summary>
    public static class NpcCityServiceChoicePolicy
    {
        public const string ChoiceId = "service";

        public static bool TryBuildChoice(string npcId, out NpcShopChoiceDefinition choice)
        {
            choice = default;
            if (!CindarsHope.City.Services.CityServiceCatalog.IsServiceProvider(npcId))
            {
                return false;
            }

            var serviceId = CindarsHope.City.Services.CityServiceCatalog.ServiceIdFor(npcId);
            if (string.IsNullOrEmpty(serviceId))
            {
                return false;
            }

            var label = CindarsHope.City.Services.CityServiceAccess.OwnsService(serviceId)
                ? "Servico (ja contratado)"
                : CindarsHope.City.Services.CityServiceCatalog.DisplayLabelFor(serviceId);

            if (string.IsNullOrEmpty(label))
            {
                return false;
            }

            choice = new NpcShopChoiceDefinition(label, ChoiceId);
            return true;
        }

        public static string PurchaseMessageForProvider(string npcId)
        {
            var serviceId = CindarsHope.City.Services.CityServiceCatalog.ServiceIdFor(npcId);
            if (string.IsNullOrEmpty(serviceId))
            {
                return null;
            }

            var result = CindarsHope.City.Services.CityServiceAccess.TryPurchase(serviceId);
            return result != null && !string.IsNullOrEmpty(result.Message)
                ? result.Message
                : "Servico indisponivel.";
        }
    }
}
