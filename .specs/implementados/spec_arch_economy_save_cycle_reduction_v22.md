# spec_arch_economy_save_cycle_reduction_v22

> **Status:** Implementado e BUILD_VALIDATED
> **Data:** 2026-07-07
> **Escopo:** quebrar o par mútuo `Economy|Save` sem alterar gameplay, saves, cenas, prefabs, IDs ou balanceamento.

## Objetivo

Reduzir mais um ciclo residual da modularização ampla (Tier 1 do
`docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md`):

```text
Antes:
- MutualModulePairs=36
- Par presente: Economy|Save

Depois:
- MutualModulePairs=35
- Par removido: Economy|Save
```

## Diagnóstico

O ciclo era causado por:

- `Save -> Economy`: direção legítima (`SaveData.cs` compõe `EconomySaveData` que referenciava
  `ShopStockSaveData`, próprio de `Save`).
- `Economy -> Save`: `Economy/WeaponInfusionRegistry.cs` e `Economy/ShopManager.cs` importavam
  `CindarsHope.Save` apenas para tipar os DTOs `WeaponInfusionSaveData`, `ShopStockSaveData` e
  `ShopItemStockEntry` (definidos em `Assets/_Game/Scripts/Save/SaveData.cs`).

Verificação de Passo 0 (obrigatória antes do corte): grep de `using CindarsHope.Save`/`SaveData` em
toda a pasta `Assets/_Game/Scripts/Economy/` confirmou que `WeaponInfusionRegistry.cs` e
`ShopManager.cs` eram os **únicos** arquivos de Economy referenciando Save. Grep repo-wide dos 3
tipos (`WeaponInfusionSaveData`, `ShopStockSaveData`, `ShopItemStockEntry`) confirmou apenas 3
consumidores em código de produção (`SaveData.cs`, `ShopManager.cs`, `WeaponInfusionRegistry.cs`) +
1 teste (`Assets/_Game/Tests/EditMode/Economy/TemperingTests.cs`).

Os 3 DTOs têm apenas campos `string`/`int`/`List<>` (sem nenhum tipo `UnityEngine.*`) — elegíveis
para `CindarsHope.Foundation` (que tem `noEngineReferences: true`).

`using CindarsHope.Economy;` em `Assets/_Game/Scripts/Save/SaveManager.Migration.cs:11` estava
morto (nenhum tipo de `Economy` referenciado no arquivo) — confirmado por grep antes da remoção.

## Implementação

- Novo arquivo `Assets/_Game/Scripts/Foundation/SaveSchema/EconomySaveDtos.cs`
  (namespace `CindarsHope.Foundation`) com os 3 DTOs movidos **sem alterar nome de classe/campo**
  (JsonUtility serializa por nome de campo — save no disco fica idêntico, sem migration):
  `WeaponInfusionSaveData`, `ShopStockSaveData`, `ShopItemStockEntry`.
- `Assets/_Game/Scripts/Save/SaveData.cs`: removidas as 3 classes; adicionado
  `using CindarsHope.Foundation;`; `EquipmentSaveData.Infusions` e `EconomySaveData.Shops`
  continuam com os mesmos nomes de tipo (agora resolvidos via Foundation).
- `Assets/_Game/Scripts/Economy/WeaponInfusionRegistry.cs` e `Economy/ShopManager.cs`: trocado
  `using CindarsHope.Save;` por `using CindarsHope.Foundation;`.
- `Assets/_Game/Scripts/Save/SaveManager.Migration.cs`: removido `using CindarsHope.Economy;` morto.
- `Assets/_Game/Tests/EditMode/Economy/TemperingTests.cs`: adicionado `using CindarsHope.Foundation;`
  (mantém `using CindarsHope.Save;` para `EquipmentSaveData`, ainda em Save).
- `CindarsHope.Foundation.csproj`: `<Compile Include>` do novo arquivo adicionado manualmente antes
  do Unity reabrir o projeto (Unity, já aberto, regenerou o csproj de forma consistente logo em
  seguida — confirmado sem divergência).
- `Assets/_Game/Tests/EditMode/Architecture/Editor/ArchitectureRatchetTests.cs`: o ratchet
  `FoundationAssembly_ContainsOnlyTheCuratedPureContracts` mantém uma allowlist explícita dos
  arquivos de `Foundation/`; adicionado `EconomySaveDtos.cs` à lista esperada (decisão explícita de
  arquitetura desta spec, não um crescimento não intencional).
- Nenhum outro DTO foi movido; nenhum schema/campo de save foi alterado; nenhuma cena/prefab/asset
  tocado.

## Evidência

```text
tools/architecture/Get-ModularizationDependencySnapshot.ps1
  exit 0
  MutualModulePairs: 36 -> 35
  Economy|Save removido
  nenhum par novo apareceu

tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1
  exit 0
  7/7 projetos
  0 warnings
  0 errors

tools/unity/RunUnityEditModeTests.ps1 -ResultsPath TestResults\cut-economy-save-editmode-2.xml -LogFile Logs\cut-economy-save-2.log
  exit 0
  2747/2747 PASS
  Resultados: TestResults/cut-economy-save-editmode-2.xml
  Log: Logs/cut-economy-save-2.log
```

Nota: uma primeira rodada (`TestResults/cut-economy-save-editmode.xml`,
`Logs/cut-economy-save.log`) apresentou 1 falha esperada — o ratchet
`FoundationAssembly_ContainsOnlyTheCuratedPureContracts` rejeitando o crescimento não anunciado do
escopo de `Foundation/`. Corrigido atualizando a allowlist do teste (decisão explícita de
arquitetura, documentada acima); a segunda rodada fechou 2747/2747 PASS.

## Pendências

Esta spec-filha fecha apenas `Economy|Save`. A modularização ampla não está concluída: ainda restam
35 pares mútuos para specs-filhas independentes.
