// arch: quebra do par mutuo NPC|UI (2026-07-15) — enum puro (sem dependencia de engine) movido de
// CindarsHope.UI.Shop para CindarsHope.NPC (não Foundation — a fronteira Foundation é ratchet-tracked
// por ArchitectureRatchetTests.FoundationScope_ContainsOnlyCuratedPureTypes/
// Get-ModularizationDependencySnapshot; NPC já referencia UI->NPC de qualquer forma, então não há
// necessidade de crescer Foundation para este corte). NpcShopController/NpcShopTransactionFacade e a
// porta INpcShopMenuPresenter consultam a escolha do menu de loja sem referenciar CindarsHope.UI.Shop
// diretamente; ShopMenuModal (UI) referencia este tipo via CindarsHope.NPC (aresta UI->NPC já
// existente). Valores e ordem preservados.
namespace CindarsHope.NPC
{
    public enum ShopMenuOption
    {
        None,
        Buy,
        Sell,
        Exit
    }
}
