---
name: equipment-durability-repair
description: Consumo de durability por uso, repair e upgrade por tier (custo/material), compare de stats antes/depois e save de durability/tier por id, reusando EquipmentManager/EquipmentDurabilityTracker/EquipmentUpgradeRegistry existentes. Use em specs de repair/upgrade/compare (04_ui_repair_upgrade_screen_flow, 05_farm_tool_upgrade_repair_tier, 04_ui_equipment_compare) ou em qualquer mudança de durabilidade, reparo ou upgrade de gear.
---

# Skill: Durability, Repair e Upgrade de Equipment

O projeto já tem `EquipmentManager` (slots por `itemInstanceId`), `EquipmentDurabilityTracker` (durability por instância, `TakeDamage`/`Repair`/`FullRepair`, eventos `DurabilityChangedEvent`/`ItemBrokenEvent`/`ItemRepairedEvent`), `RepairKitManager` (reparo consumindo kit do inventory) e `EquipmentUpgradeRegistry` (upgrade focado +1/+2/+3 com teto §35). Esta skill garante que toda nova mecânica de durability/repair/upgrade reuse esses sistemas (NÃO crie um manager paralelo) e persista só id+estado simples.

## Quando usar

A tarefa toca:
- Consumo de durability por uso, item quebrado / auto-unequip
- Repair (kit, NPC, custo de gold/material) ou upgrade por tier
- Compare de stats antes/depois de equipar/upgradar
- Save do estado de durability/tier
- `EquipmentManager`, `EquipmentDurabilityTracker`, `DurabilityData`, `RepairKitManager`, `EquipmentUpgradeRegistry`
- Specs: `04_ui_repair_upgrade_screen_flow`, `05_farm_tool_upgrade_repair_tier`, `04_ui_equipment_compare`

## Quando NÃO usar

- Wiring de stats base de weapon/armor (databases de combat data) → use a skill `combat-data-wiring`.
- A tela/modal de repair/upgrade/compare em si (open/close/Esc/input block) → use a skill `ui-modal-stack`.
- Equipar/desequipar e roteamento de acessórios → já é o `EquipmentManager` direto (`EquipItem`/`TryEquipAccessory`).

## Leitura mínima

1. `CLAUDE.md`
2. Spec alvo
3. `Assets/_Game/Scripts/Equipment/EquipmentManager.cs` — `RegisterEquipmentUsage`, `RepairItem`, `GetItemDurability`, `CaptureSaveData`/`RestoreFromSaveData`
4. `Assets/_Game/Scripts/Equipment/EquipmentDurabilityTracker.cs` e `DurabilityData.cs`
5. `Assets/_Game/Scripts/Equipment/RepairKitManager.cs`
6. `Assets/_Game/Scripts/Crafting/EquipmentUpgradeRegistry.cs` — upgrade {level, focus} por instância

## Sistemas existentes (reusar, não duplicar)

| Preciso de… | Já existe | Não criar |
|---|---|---|
| Consumir durability por uso | `EquipmentManager.RegisterEquipmentUsage(itemInstanceId)` → `EquipmentDurabilityTracker.TryRegisterUsage` | desgaste manual fora do tracker |
| Ler durability | `EquipmentManager.GetItemDurability(id) → DurabilityData` (`CurrentDurability`/`MaxDurability`/`IsBroken`/`DurabilityPercent`) | cache próprio de durability |
| Item quebrado | `DurabilityData.IsBroken` + auto-unequip em `AutoUnequipBrokenItem` | checagem de "broken" espalhada |
| Reparar | `EquipmentManager.RepairItem(id, amount)` (aplica `RepairEfficiencyBonus`) ; `RepairKitManager.TryRepairEquipmentWithKit` | reparo que credite durability direto no `DurabilityData` |
| Upgrade por tier | `EquipmentUpgradeRegistry.Set/Get(itemInstanceId)` (teto +3, um foco por nível) | registro paralelo de tier |
| Save de durability/tier | `EquipmentManager.CaptureSaveData()`/`RestoreFromSaveData()` (`EquipmentSaveData` agrega slots + `Upgrades`) | novo schema de save |

Antes de criar qualquer classe nova, rode a skill `system-reuse-audit`.

## Regras de durability

- **Chave é `itemInstanceId`, não id de definição** — durability/upgrade são por instância. Inicialize com `EquipmentDurabilityTracker.InitializeEquipment(id, maxDurability)` antes do primeiro uso.
- **Cada uso consome 1** (`TakeDamage(1)`); o consumo efetivo pode ser reduzido por modifier determinístico (ex.: `ToolDurabilityModifier` do acessório de Thoren), que é um **ponto único** acumulador — não espalhe `if` de bônus.
- **Quebra dispara eventos e auto-unequip:** `TryRegisterUsage` publica `DurabilityChangedEvent` e (na transição) `ItemBrokenEvent`; `EquipmentManager.RegisterEquipmentUsage` então chama `AutoUnequipBrokenItem`. Não duplique esse fluxo.
- `DurabilityData.IsLowDurability` (<25%) é o gancho para warning de HUD.

## Repair e upgrade

- **Repair é clampado a `MaxDurability`** (`DurabilityData.Repair`); `RepairItem` aplica o `RepairEfficiencyBonus` derivado (F18) num ponto único — preserve isso.
- **Custo de repair/upgrade** (gold/material) é economia: balanceie sink vs. valor do item com a skill `economy-balance-tuning`; nunca hardcode preços avulsos no manager.
- **Upgrade tem teto +3 e um foco por nível** (`EquipmentUpgradeRegistry`, invariante §35). A curva de custo por tier segue a skill `progression-curve-design` (custo crescente por nível). NUNCA persista derivados (§45): stats/durability recalculam no load lendo o registro.
- Consumo do material/kit passa pelo inventory de forma **atômica** (ver `RepairKitManager`: repara e só então `RemoveItem`; em falha de remoção, reporta) — alinhe com a skill `inventory-transactions`.

## Compare (stats antes/depois)

O compare é uma projeção pura (sem mutar estado): leia os stats base do gear (skill `combat-data-wiring`), aplique o delta de upgrade/tier do `EquipmentUpgradeRegistry` e a durability atual, e produza um before/after. Mantenha o cálculo em C# puro para ser testável em EditMode; a view só renderiza (skill `ui-projection-pattern`).

## Interação com save

- Persistir **só id + estado simples**: durability via `DurabilityEntryData` (`ItemInstanceId`, `CurrentDurability`, `MaxDurability`); upgrade via `EquipmentUpgradeSaveData` (`ItemInstanceId`, `UpgradeLevel`, `UpgradeFocus` como string estável). Nenhum `ScriptableObject`/`GameObject`/`Sprite` (rule `save-load-pattern`).
- **Aditivo, sem migration:** lista null/ausente em save legado = sem upgrade/durability (level 0); entradas inválidas (level fora de 1–3, foco desconhecido) são **ignoradas com segurança** no restore (rule `error-handling-resilience`, categoria 2).
- Derivados (stats finais) **nunca** entram no save — recalcule no load lendo durability + upgrade.

## Testes

O `testing-quality-gate` exige EditMode tests para os cálculos determinísticos (skill `editmode-test-authoring`). Cubra no mínimo:
- Durability: N usos reduzem `CurrentDurability` em N (ou N×(1−bônus) com o modifier); `IsBroken` quando `<= 0`.
- Repair: clampa em `MaxDurability`; `RepairEfficiencyBonus` aumenta o restaurado; reparo em item já full não estoura.
- Upgrade: `Set` respeita teto +3 e um foco por nível; level 0 / foco None remove a entrada.
- Compare: before/after determinístico para o mesmo input (sem efeitos colaterais).
- Round-trip de save: durability + upgrade sobrevivem a `Capture`→`Restore`; entrada inválida é descartada sem afetar as válidas.

## Regressões comuns

- Usar id de definição em vez de `itemInstanceId` como chave → durability/upgrade do item errado.
- Reparar/upgradar sem publicar `DurabilityChangedEvent`/`ItemRepairedEvent` → HUD não atualiza.
- Persistir stats derivados (viola §45) ou serializar refs Unity (viola `save-load-pattern`).
- Adicionar migration onde o padrão é aditivo (default neutro) — quebra saves legados desnecessariamente.
- Hardcode de custo de repair/upgrade no manager em vez de passar pela economy.
- Espalhar `if` de bônus de durability em vez do ponto único (`ToolDurabilityModifier`).

## Onde se aplica

- `04_ui_repair_upgrade_screen_flow`
- `05_farm_tool_upgrade_repair_tier`
- `04_ui_equipment_compare`

## Relacionados

- skill `combat-data-wiring` — stats base de weapon/armor
- skill `economy-balance-tuning` — custo de repair/upgrade vs. valor do item
- skill `progression-curve-design` — curva de custo por tier
- rule `save-load-pattern` — durability/tier por id, sem Unity refs, aditivo
- skill `editmode-test-authoring`, rule `testing-quality-gate` — cálculo de durability, regras de repair/upgrade
- rule `error-handling-resilience` — restore tolerante a entrada inválida; falha de repair = `bool`/feedback
- skill `inventory-transactions` — consumo atômico de kit/material
- skill `system-reuse-audit` — antes de criar qualquer classe nova
