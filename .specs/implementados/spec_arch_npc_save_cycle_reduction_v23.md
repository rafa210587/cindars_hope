# spec_arch_npc_save_cycle_reduction_v23

> **Status:** Implementado e BUILD_VALIDATED
> **Data:** 2026-07-08
> **Escopo:** quebrar o par mútuo `NPC|Save` sem alterar gameplay, saves, cenas, prefabs, IDs ou balanceamento.

## Objetivo

Reduzir mais um ciclo residual da modularização ampla (Tier 1 do
`docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md`):

```text
Antes:
- MutualModulePairs=35
- Par presente: NPC|Save

Depois:
- MutualModulePairs=34
- Par removido: NPC|Save
```

## Diagnóstico

O ciclo era causado por:

- `Save -> NPC`: direção legítima (`SaveData.cs` compunha `NpcManagerSaveData`/`NpcSaveData`;
  `SaveManager.cs`, `SaveManager.Migration.cs` e `Save/Providers/NpcsSectionProvider.cs` já
  importavam `CindarsHope.NPC` para outros tipos do domínio NPC).
- `NPC -> Save`: `Assets/_Game/Scripts/NPC/NpcManager.cs` importava `CindarsHope.Save` apenas para
  tipar `NpcManagerSaveData`/`NpcSaveData` (definidos em `Assets/_Game/Scripts/Save/SaveData.cs`).

Verificação de Passo 0 (obrigatória antes do corte): grep de `using CindarsHope.Save` e dos tipos
`NpcManagerSaveData`/`NpcSaveData` em toda a pasta `Assets/_Game/Scripts/NPC/` confirmou que
`NpcManager.cs` era o **único** arquivo de NPC referenciando Save. Grep repo-wide dos 2 tipos
confirmou os consumidores de produção: `SaveData.cs`, `SaveManager.cs`,
`SaveManager.Migration.cs`, `Save/Providers/NpcsSectionProvider.cs` e `NpcManager.cs` — nenhum
teste EditMode referencia esses tipos diretamente.

Os 2 DTOs têm apenas `string`/`bool`/`List<>` e `UnityEngine.Vector2` (campo `Position`) —
elegíveis para o namespace de domínio `CindarsHope.NPC` (que já referencia Unity), não para
`CindarsHope.Foundation` (que exige `noEngineReferences: true`). Precedente vivo: `FriendshipSaveData`
(`NPC/Friendship/FriendshipSaveData.cs`) e `NpcServicesSaveData`
(`NPC/Services/NpcServicesSaveData.cs`) já vivem em `CindarsHope.NPC.*` e são embutidos no
`GameSaveData` da mesma forma.

`SaveManager.cs`, `SaveManager.Migration.cs` e `Save/Providers/NpcsSectionProvider.cs` **já**
tinham `using CindarsHope.NPC;` antes desta spec (usado por outros tipos NPC) — nenhum `using`
novo foi necessário nesses 3 arquivos.

## Implementação

- Novo arquivo `Assets/_Game/Scripts/NPC/NpcManagerSaveData.cs` (namespace `CindarsHope.NPC`) com
  os 2 DTOs movidos **sem alterar nome de classe/campo** (JsonUtility serializa por nome de
  campo — save no disco fica idêntico, sem migration): `NpcManagerSaveData`, `NpcSaveData`.
- `Assets/_Game/Scripts/Save/SaveData.cs`: removidas as 2 classes; o campo `GameSaveData.Npcs`
  passou a ser qualificado como `CindarsHope.NPC.NpcManagerSaveData` (sem novo `using`, para não
  poluir o topo do arquivo com um domínio referenciado uma única vez).
- `Assets/_Game/Scripts/NPC/NpcManager.cs`: removido `using CindarsHope.Save;` (não havia outro
  uso do namespace no arquivo).
- `CindarsHope.Runtime.csproj`: `<Compile Include>` do novo arquivo adicionado manualmente (Unity
  não estava aberto para regenerar o csproj neste ambiente).
- Nenhum outro DTO foi movido; nenhum schema/campo de save foi alterado; nenhuma cena/prefab/asset
  tocado; `SaveManager.cs`, `SaveManager.Migration.cs` e `NpcsSectionProvider.cs` não precisaram de
  edição (já resolviam os tipos via `using CindarsHope.NPC;` preexistente).

## Evidência

```text
tools/architecture/Get-ModularizationDependencySnapshot.ps1
  MutualModulePairs: 35 -> 34
  NPC|Save removido
  nenhum par novo apareceu

tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1
  exit 0
  7/7 projetos
  0 warnings
  0 errors

tools/unity/RunUnityEditModeTests.ps1 -ResultsPath TestResults\cut-npc-save-editmode.xml -LogFile Logs\cut-npc-save.log
  exit 0
  2747/2747 PASS
  Resultados: TestResults/cut-npc-save-editmode.xml
  Log: Logs/cut-npc-save.log
```

## Pendências

Esta spec-filha fecha apenas `NPC|Save`. A modularização ampla não está concluída: ainda restam
34 pares mútuos para specs-filhas independentes.

## Nota de risco residual (não bloqueante)

`Assets/_Game/Scripts/Editor/Validation/ValidateWave25TownNpcSchedulesDialogue.cs` — validator
arquivado (`CindarsHope/Archive/...`, não invocado pelos 3 comandos canônicos de
`editor-generation-orchestration`) faz um check textual `ContainsText(".../Save/SaveData.cs",
"HasMet")`, que hoje falharia (o campo `HasMet` saiu de `SaveData.cs`). Não corrigido nesta spec
por estar fora do escopo declarado (par `NPC|Save`) e por não ser executado por nenhum dos 3
comandos canônicos (`Inicializar Projeto`/`Validar Projeto`/`Reparar e Reconstruir`) nem pelos
gates desta spec. Residual risk: se alguém rodar esse menu arquivado manualmente, verá 1 falha
textual espúria sem relação com regressão real.
