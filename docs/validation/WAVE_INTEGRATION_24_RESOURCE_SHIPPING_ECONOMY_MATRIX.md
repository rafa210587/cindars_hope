# WAVE_INTEGRATION_24 — Resource / Shipping / Economy Matrix

**Date:** 2026-06-11

## Resource Nodes

| Feature | Exists? | Path | Status |
|---------|---------|------|--------|
| FarmResourceNodeService | YES | Farm/Resources/FarmResourceNodeService.cs | REUSED |
| FarmResourceRefreshProcessor | YES | Farm/Resources/FarmResourceRefreshProcessor.cs | REUSED |
| ResourceNode (tree/rock/forage) | YES | World/ResourceNode.cs | REUSED |
| ResourceNodeDefinition | YES | Farm/Resources/ResourceNodeDefinition.cs | REUSED |
| ResourceNodeDepletedEvent | YES | Core/Events/ResourceNodeDepletedEvent.cs | REUSED |
| TreeChopService | YES | Farm/Trees/TreeChopService.cs | REUSED |
| RockMiningService | YES | Farm/Mining/RockMiningService.cs | REUSED |
| FarmFishingService | YES | Farm/Fishing/FarmFishingService.cs | REUSED |
| FarmForageSpawnService | YES | Farm/Forage/FarmForageSpawnService.cs | REUSED |

**Resource collection flow:** IInteractable → AddItem to InventoryManager → ResourceNodeDepletedEvent → FarmResourceRefreshProcessor (refreshes after N days)

**No new resource code was created** for WAVE24 — all resource systems are WAVE05/06/07 pre-existing.

## Shipping

| Feature | Exists? | Path | Status |
|---------|---------|------|--------|
| FarmShippingService | YES | Farm/Shipping/FarmShippingService.cs | REUSED |
| ShippingPriceResolver | YES | Farm/Shipping/ShippingPriceResolver.cs | REUSED |
| PendingShippingEntry | YES | Farm/Shipping/PendingShippingEntry.cs | REUSED |
| ShippingBatch | YES | Farm/Shipping/ShippingBatch.cs | REUSED |
| SellPoint (IInteractable) | YES | Economy/SellPoint.cs | REUSED |
| SellAllPoint | YES | Economy/SellAllPoint.cs | REUSED |
| SellableItemPolicy | YES | Economy/SellableItemPolicy.cs | REUSED |

**Shipping flow:** SellPoint.Interact → InventoryManager.RemoveItem + EconomyManager.AddGold + EconomyTransactionCompletedEvent published

**ShippingSummaryService (new):** Subscribes EconomyTransactionCompletedEvent → publishes PlayerActionFeedbackEvent("Vendido: X por Yg")

## Economy

| Feature | Exists? | Path | Status |
|---------|---------|------|--------|
| EconomyManager | YES | Economy/EconomyManager.cs | REUSED |
| EconomyPricingService | YES | Economy/EconomyPricingService.cs | REUSED |
| GoldChangedEvent | YES | Core/Events/GoldChangedEvent.cs | REUSED |
| EconomyTransactionCompletedEvent | YES | Core/Events/EconomyTransactionCompletedEvent.cs | REUSED |
| ShopManager | YES | Economy/ShopManager.cs | REUSED |
| BuyItemPoint | YES | Economy/BuyItemPoint.cs | REUSED |
| SeedShopPoint | YES | Economy/SeedShopPoint.cs | REUSED |

**No new Economy/ShopManager/EconomyManager code was created** — all are WAVE05/06/08 pre-existing.

## Economy Save/Load

| Case | Expected | Actual |
|------|----------|--------|
| Shop stock save | Shops captured via ShopManager | SaveManager.CaptureEconomySaveData |
| Shop stock restore | Shops restored | SaveManager.RestoreEconomySaveData |
| Gold save | Gold in PlayerSaveData | PlayerManager.CaptureSaveData |
| Gold restore | Gold restored from PlayerSaveData | PlayerManager.RestoreFromSaveData |
