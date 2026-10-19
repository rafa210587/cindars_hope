# spec_arch_inventory_save_cycle_reduction_v25

> **Status:** Implementado e BUILD_VALIDATED
> **Data:** 2026-07-08
> **Escopo:** quebrar o par mútuo `Inventory|Save` sem alterar gameplay, saves, cenas, prefabs, IDs ou balanceamento.

## Objetivo

Reduzir mais um ciclo residual da modularização ampla (Tier 2 do
`docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md`):

```text
Antes:
- MutualModulePairs=33
- Par presente: Inventory|Save

Depois:
- MutualModulePairs=32
- Par removido: Inventory|Save
```

## Diagnóstico

O ciclo era causado por:

- `Save -> Inventory`: direção legítima (`SaveData.cs` compunha
  `InventorySaveData`/`InventorySlotSaveData`/`InventoryItemSaveData`; `SaveManager.cs`,
  `SaveManager.Migration.cs`, `Save/Providers/InventorySectionProvider.cs`,
  `Save/Migrations/InventorySlotsV1ToV2Migration.cs` e `Save/DeathSaveData.cs` já importavam ou
  precisavam do namespace `CindarsHope.Inventory` para o domínio Inventory).
- `Inventory -> Save`: `Assets/_Game/Scripts/Inventory/InventoryManager.cs` importava
  `CindarsHope.Save` apenas para tipar os 3 DTOs (definidos em
  `Assets/_Game/Scripts/Save/SaveData.cs`).

Verificação de Passo 0 (obrigatória antes do corte): grep de `using CindarsHope.Save` em toda a
pasta `Assets/_Game/Scripts/Inventory/` confirmou que `InventoryManager.cs` era o **único**
arquivo de Inventory referenciando Save. Grep repo-wide dos 3 tipos confirmou os consumidores de
produção: `SaveData.cs`, `SaveManager.cs`, `SaveManager.Migration.cs`,
`Save/Providers/InventorySectionProvider.cs`, `Save/DeathSaveData.cs`,
`Save/Migrations/InventorySlotsV1ToV2Migration.cs`, `InventoryManager.cs` e o teste
`Assets/_Game/Tests/EditMode/Save/Editor/SaveProviderRegistryTests.cs` (usa `InventorySaveData`
como stub genérico de payload de provider).

Os 3 DTOs têm apenas `int`/`string`/`bool`/`List<>` — elegíveis para o namespace de domínio
`CindarsHope.Inventory` (já usado por vários consumidores). Precedente vivo: `NpcManagerSaveData`/
`NpcSaveData` movidos para `CindarsHope.NPC` (`spec_arch_npc_save_cycle_reduction_v23`).

`SaveManager.cs`, `SaveManager.Migration.cs`, `InventorySectionProvider.cs` e
`InventorySlotsV1ToV2Migration.cs` **já** tinham `using CindarsHope.Inventory;` antes desta spec
(usado por outros tipos Inventory) — nenhum `using` novo foi necessário nesses 4 arquivos.

## Implementação

- Novo arquivo `Assets/_Game/Scripts/Inventory/InventorySaveData.cs` (namespace
  `CindarsHope.Inventory`) com os 3 DTOs movidos **sem alterar nome de classe/campo** (JsonUtility
  serializa por nome de campo — save no disco fica idêntico, sem migration): `InventorySaveData`,
  `InventorySlotSaveData`, `InventoryItemSaveData`.
- `Assets/_Game/Scripts/Save/SaveData.cs`: removidas as 3 classes; adicionado
  `using CindarsHope.Inventory;` (o arquivo ainda referencia `InventorySaveData` no campo
  `GameSaveData.Inventory`).
- `Assets/_Game/Scripts/Inventory/InventoryManager.cs`: removido `using CindarsHope.Save;` (não
  havia outro uso do namespace no arquivo; o tipo `InventorySaveData` agora está no mesmo
  namespace `CindarsHope.Inventory` do próprio arquivo, sem necessidade de `using`).
- `Assets/_Game/Scripts/Save/DeathSaveData.cs` e
  `Assets/_Game/Tests/EditMode/Save/Editor/SaveProviderRegistryTests.cs`: adicionado
  `using CindarsHope.Inventory;` (referenciavam os DTOs implicitamente pelo mesmo namespace de
  `SaveData.cs` antes do corte).
- `CindarsHope.Runtime.csproj`: `<Compile Include>` do novo arquivo adicionado manualmente (Unity
  não estava aberto para regenerar o csproj neste ambiente; entrada não commitada — `.csproj` é
  gitignored).
- Nenhum outro DTO foi movido; nenhum schema/campo de save foi alterado; nenhuma cena/prefab/asset
  tocado; `SaveManager.cs`, `SaveManager.Migration.cs`, `InventorySectionProvider.cs` e
  `InventorySlotsV1ToV2Migration.cs` não precisaram de edição (já resolviam os tipos via
  `using CindarsHope.Inventory;` preexistente).

## Evidência

```text
tools/architecture/Get-ModularizationDependencySnapshot.ps1
  MutualModulePairs: 33 -> 32
  Inventory|Save removido
  nenhum par novo apareceu

tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1
  exit 0
  7/7 projetos
  0 warnings
  0 errors

tools/unity/RunUnityEditModeTests.ps1 -ResultsPath TestResults\cut-inventory-save-editmode.xml -LogFile Logs\cut-inventory-save.log
  exit 0
  2747/2747 PASS
  Resultados: TestResults/cut-inventory-save-editmode.xml
  Log: Logs/cut-inventory-save.log
```

## Pendências

Esta spec-filha fecha apenas `Inventory|Save`. A modularização ampla não está concluída: ainda
restam 32 pares mútuos para specs-filhas independentes.

## Nota de risco residual (não bloqueante)

Nenhuma encontrada. Nenhum validator arquivado ou teste faz assert textual sobre a localização
física destes 3 DTOs em `Save/SaveData.cs` (diferente do achado documentado em
`spec_arch_npc_save_cycle_reduction_v23`).
