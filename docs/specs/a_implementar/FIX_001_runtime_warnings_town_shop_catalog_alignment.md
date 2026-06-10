# FIX-001 — Cleanup de Runtime Warnings e Alinhamento Town Shop Catalog

Status: A_IMPLEMENTAR
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
