# FIX-001 — Cleanup de Runtime Warnings e Alinhamento Town Shop Catalog

Status: IMPLEMENTADA_VIA_FIX001B
Domínio: Integration / Runtime Quality / City / Economy
Camada: MVP hardening

## Objetivo

Corrigir os warnings CS0618 causados por APIs Unity obsoletas e corrigir os erros vermelhos de shop
na TownScene causados por ItemId ausente no ItemDatabaseSO.

## Escopo

- Remover uso runtime/bootstrap de Object.FindObjectsOfType<T>(), FindObjectOfType<T>(),
  Object.FindFirstObjectByType<T>()
- Auditar todos os ShopDataSO usados pelos NPCs da TownScene
- Comparar todos os ShopDataSO.Items[*].ItemId contra ItemDatabaseSO
- Corrigir os shops afetados: shop_thalindra, shop_corvus, shop_savra, shop_mirela, shop_hund
- Criar validator editor-only para impedir regressão
- Atualizar report e CURRENT_STATE.md

## Fora de escopo

- Reputation/Schedule/Relationship/Quest/Balance/UI final
- Novos NPCs além dos já referenciados
- Edição manual de YAML .unity, .prefab ou .asset
- Silenciar warnings com pragma

## Hotfix: FIX-001B (2026-06-10)

FIX-001 foi complementado por FIX-001B que:
- Substituiu FindObjectsOfType em CraftingStationRuntimeBootstrap por CraftingRuntime.ActiveInstances (registro estático)
- Substituiu FindAnyObjectByType em CaveRuntimeBridge por CaveRunManager.Instance (singleton)
- Adicionou guard de destruição de duplicata em QuestOfferPanelController e QuestLogPanelController
- Expandiu ValidateTownShopCatalogIntegrity para todos 25 ShopDataSO em Data/Economy
- Criou docs: FIX_001_RUNTIME_WARNINGS_SHOP_CATALOG_DECISION.md, FIX_001_RUNTIME_WARNINGS_SHOP_CATALOG_REPORT.md, FIX_001B_HUMAN_PLAYMODE_CHECKLIST.md

Assembly-CSharp: PASS (0E/0W) | Assembly-CSharp-Editor: PASS (0E/3W pre-existentes)
Play Mode checklist: docs/validation/FIX_001B_HUMAN_PLAYMODE_CHECKLIST.md
