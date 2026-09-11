# Inventory save, validation and regression checks

## Interação com save

- O snapshot persiste **só id + quantidade por slot** (`InventorySlotSaveData`: `SlotIndex`, `ItemId`, `Amount`, `IsEquipped`, `EquipmentBindingId`). Nenhum `ItemDataSO`, `Sprite`, `GameObject` (rule `save-dto-simple-types-only`).
- `RestoreFromSaveData` resolve cada id pelo `ItemDatabaseSO` (`TryGetItemData`) e **descarta com warning** ids desconhecidos — fallback seguro, sem crash (rule `error-handling-resilience`, categoria 2).
- Save legado sem `Slots` cai no path `RestoreLegacyItems` (lista id+amount). Preserve esse fallback ao mudar o schema.
- Mudança de schema de inventory → seguir a skill `save-load-pattern` (version + migration) e documentar em `docs/validation/`.

## Testes

O `testing-quality-gate` exige EditMode tests para deterministic logic de inventory transactions (skill `editmode-test-authoring`). Cubra no mínimo:
- Stack: add além de `MaxStack` cria/preenche o número correto de slots; merge em stack parcial antes de slot novo.
- Capacidade cheia: `TryAddItem` retorna `Success=false` e `AddedAmount=0` **sem** mutar slots.
- Split: `SplitSlot` com `Amount>=2` e slot livre divide pela metade; sem slot livre retorna `false` sem mutar.
- Atomicidade: transferência com destino cheio devolve tudo à origem (contagem total conservada).
- Round-trip de save: `CaptureSaveData()` → `RestoreFromSaveData()` reproduz contagens e bindings; id desconhecido é ignorado sem perder os válidos.

## Regressões comuns

- Mutar `_slots` e esquecer `RebuildAggregate()` → `Items`/`GetAmount` ficam dessincronizados.
- Add/remove sem publicar `InventoryChangedEvent` → HUD e `EquipmentManager` (que faz subscribe) não atualizam.
- Mutar slots antes de validar capacidade → item duplicado ou perdido em falha parcial.
- Serializar `ItemDataSO`/`Sprite` no snapshot → viola `save-dto-simple-types-only`.
- Consumidor (quest/economy) chamando o `InventoryManager` direto em vez de via adapter/interface.

## Onde se aplica

- `04_ui_inventory_items_tooltips`
- `04_ui_storage_chest_transfer`
- `06_item_definition_tags_quality_rarity`

## Relacionados

- skill `save-load-pattern`, rule `save-dto-simple-types-only` — snapshot id+qty, sem Unity refs
- rule `error-handling-resilience` — transação inválida = `bool`+`FailureReason`, não exception
- skill `editmode-test-authoring`, rule `testing-quality-gate` — stack/split/move, atomicidade, round-trip
- skill `ui-modal-stack` — a tela/modal de inventory
- skill `system-reuse-audit` — antes de criar qualquer classe nova
