# spec_arch_player_save_cycle_reduction_v20

> **Status:** Implementado e BUILD_VALIDATED
> **Data:** 2026-07-07
> **Escopo:** quebrar o par mútuo `Player|Save` sem alterar gameplay, saves, cenas, prefabs, IDs ou balanceamento.

## Objetivo

Reduzir mais um ciclo residual da modularização ampla (Tier 1 do
`docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md`):

```text
Antes:
- MutualModulePairs=38
- RuntimeModuleEdges=227
- Par presente: Player|Save

Depois:
- MutualModulePairs=37
- RuntimeModuleEdges=226
- Par removido: Player|Save
```

## Diagnóstico

O ciclo era causado por:

- `Save -> Player`: direção legítima. `PlayerSectionProvider` (em `CindarsHope.Save.Providers`) já
  injeta `PlayerManager`, `HungerManager` e `ManaManager` via construtor.
- `Player -> Save`: `PlayerManager.cs` importava `CindarsHope.Save` só para tipar dois métodos
  (`CaptureSaveData`/`RestoreFromSaveData`) com o DTO `PlayerSaveData`.

Verificação de Passo 0 (obrigatória antes do corte): grep de `CindarsHope.Save` em toda a pasta
`Assets/_Game/Scripts/Player/` confirmou que `PlayerManager.cs` era o **único** arquivo de Player
referenciando Save. Outros tipos `*SaveData` usados por managers do domínio Player
(`ManaManagerSaveData` em `Player/ManaManager.cs`, `PlayerProgressionSaveData` em
`Player/Progression/`) já vivem no próprio namespace `CindarsHope.Player*`, não em
`CindarsHope.Save` — não geravam aresta.

## Implementação

- `PlayerManager.CaptureSaveData(int, int, Vector2)` foi removido. `PlayerSectionProvider.Capture`
  agora monta `new PlayerSaveData { ... }` diretamente, lendo os getters públicos já existentes
  (`CurrentHP`, `MaxHP`, `CurrentGold`) mais os parâmetros externos (fome, posição) que já vinham de
  fora do `PlayerManager`.
- `PlayerManager.RestoreFromSaveData(PlayerSaveData)` foi substituído por
  `PlayerManager.RestoreState(int maxHP, int currentHP, int gold)` — corpo idêntico ao método
  anterior (mesmo clamp, mesmos deltas, mesmos eventos `GoldChangedEvent`/`HPChangedEvent`), só
  trocando o parâmetro DTO por três primitivos. `PlayerSectionProvider.Restore` agora desempacota o
  `PlayerSaveData` e chama `RestoreState(data.MaxHP, data.CurrentHP, data.Gold)`; o antigo guard de
  "saveData nulo" foi movido para o provider (mesma mensagem de warning, comportamento idêntico:
  restore ignorado quando a seção é nula).
- `using CindarsHope.Save;` removido de `PlayerManager.cs`.
- `PlayerSaveData` **não foi movido** — continua em `CindarsHope.Save` (`Assets/_Game/Scripts/Save/SaveData.cs`), sem alteração de schema/campos.
- Nenhum outro caller de `PlayerManager.CaptureSaveData`/`RestoreFromSaveData` foi encontrado no
  repositório (grep confirmado); `PlayerSectionProvider` era o único.

## Evidência

```text
tools/architecture/Get-ModularizationDependencySnapshot.ps1
  exit 0
  MutualModulePairs: 38 -> 37
  RuntimeModuleEdges: 227 -> 226
  Player|Save removido
  nenhum par novo apareceu

tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1
  exit 0
  7/7 projetos
  0 warnings
  0 errors

tools/unity/RunUnityEditModeTests.ps1 -ResultsPath TestResults\cut-player-save-editmode.xml -LogFile Logs\cut-player-save.log
  exit 0
  2747/2747 PASS
  Resultados: TestResults/cut-player-save-editmode.xml
  Log: Logs/cut-player-save.log
```

## Pendências

Esta spec-filha fecha apenas `Player|Save`. A modularização ampla não está concluída: ainda restam
37 pares mútuos para specs-filhas independentes.
