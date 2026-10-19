# spec_arch_cave_save_cycle_reduction_v28

> **Status:** Implementado e BUILD_VALIDATED
> **Data:** 2026-07-08
> **Escopo:** quebrar o par mútuo `Cave|Save` sem alterar gameplay, saves, cenas, prefabs, IDs ou
> balanceamento.

## Objetivo

Reduzir mais um ciclo residual da modularização ampla (Tier 3 do
`docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md`):

```text
Antes:
- MutualModulePairs=30
- Par presente: Cave|Save

Depois:
- MutualModulePairs=29
- Par removido: Cave|Save
```

## Objetivo de direção

`Save -> Cave.Runtime` (providers/migrations/SaveManager consumindo tipos concretos de Cave) é
layering correto e foi **preservado** (direção pesada, permanece). Só a direção leve `Cave -> Save`
(`CaveRunManager.cs` importando `CindarsHope.Save` só para usar o DTO `CaveSaveData`) foi cortada,
movendo o DTO para dentro do próprio domínio Cave — precedente já vivo: `CaveRunSaveData` já mora
em `Assets/_Game/Scripts/Cave/Runtime/`.

## Diagnóstico (Passo 0 — verificação obrigatória, revalidada no disco)

Grep de `using CindarsHope.Save` em toda a pasta `Assets/_Game/Scripts/Cave/` confirmou exatamente
**um** arquivo: `Cave/Runtime/CaveRunManager.cs` (usava `CaveSaveData` como tipo de parâmetro/retorno
de `CaptureSaveData()`/`RestoreFromSaveData(CaveSaveData)`).

`CaveSaveData.cs` (então em `Assets/_Game/Scripts/Save/`) foi lido por inteiro: `[Serializable]`
puro, campos `int`/`string`/`List<int>`/`List<string>`, e listas de tipos que já vivem em
`CindarsHope.Cave.Runtime` (`SerializedVisitedLevelSnapshot`, `CaveBossDefeatState`,
`VisitedLevelSnapshot`). O arquivo já importava `CindarsHope.Cave.Runtime` e `UnityEngine`
(`Vector2`/`Vector2Int`, tipos base da engine, não de outro módulo do jogo) — nenhuma outra
dependência de domínio. Elegível para mover para `Cave.Runtime` sem reintroduzir o ciclo.

Grep repo-wide de `CaveSaveData` mapeou os consumidores fora do próprio arquivo, todos já com
`using CindarsHope.Cave.Runtime;` pré-existente (usado por outros tipos de Cave, ex. `CaveRunSaveData`,
`VisitedLevelSnapshot`): `Save/SaveManager.cs`, `Save/SaveManager.Migration.cs`,
`Save/Migrations/SaveV3ToV4Migration.cs`, `Save/Providers/CaveSectionProvider.cs`. A única exceção
foi `Save/SaveData.cs` (declara o campo `public CaveSaveData Cave;`), que não tinha nenhum `using`
de Cave porque, antes do corte, `CaveSaveData` vivia no mesmo namespace `CindarsHope.Save`.

`Editor/Validation/ValidateSpec14BCaveSnapshotReplay.cs` tinha um check textual hardcoded no path
antigo (`AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/_Game/Scripts/Save/CaveSaveData.cs")`).

## Implementação

1. `CaveSaveData.cs` (+`.meta`) movido via `git mv` de `Assets/_Game/Scripts/Save/` para
   `Assets/_Game/Scripts/Cave/Runtime/` — GUID preservado. Namespace trocado de `CindarsHope.Save`
   para `CindarsHope.Cave.Runtime`; o `using CindarsHope.Cave.Runtime;` de topo do próprio arquivo
   foi removido por ficar redundante/auto-referente após a mudança de namespace.
2. `Cave/Runtime/CaveRunManager.cs`: `using CindarsHope.Save;` removido (não havia outro uso do
   namespace Save no arquivo) — `CaveSaveData` resolve agora pelo próprio namespace do arquivo
   (`CindarsHope.Cave.Runtime`).
3. `Save/SaveData.cs`: ganhou `using CindarsHope.Cave.Runtime;` (novo, para resolver o campo
   `public CaveSaveData Cave;`); já tinha fully-qualified `CindarsHope.Cave.Runtime.CaveRunSaveData`
   noutro campo, preservado sem alteração.
4. `Save/SaveManager.cs`, `Save/SaveManager.Migration.cs`, `Save/Migrations/SaveV3ToV4Migration.cs`
   e `Save/Providers/CaveSectionProvider.cs`: **nenhuma edição necessária** — já resolviam
   `CaveSaveData` via `using CindarsHope.Cave.Runtime;` preexistente (usado para outros tipos de
   Cave nesses mesmos arquivos).
5. `Editor/Validation/ValidateSpec14BCaveSnapshotReplay.cs`: path do check textual atualizado de
   `Assets/_Game/Scripts/Save/CaveSaveData.cs` para `Assets/_Game/Scripts/Cave/Runtime/CaveSaveData.cs`.
6. `CindarsHope.Runtime.csproj`: `<Compile Include>` do arquivo movido atualizado manualmente
   (`Save\CaveSaveData.cs` → `Cave\Runtime\CaveSaveData.cs`; Unity não estava aberto para regenerar
   o csproj neste ambiente; arquivo é gitignored, não commitado).
7. Nenhum schema/campo/valor/nome de classe alterado; nenhuma cena/prefab/asset editado
   manualmente; nenhum comportamento de gameplay/save alterado (JsonUtility serializa por nome de
   campo, não por namespace/type-name — transparente aos saves existentes no disco).

## Achado fora de escopo (não corrigido)

`Assets/_Game/Tests/EditMode/Cave/CaveSaveBackCompatTests.cs` já tinha `using CindarsHope.Save;`
sem nenhuma referência real a um tipo do namespace Save no corpo do arquivo (a substring
`CaveSaveData` só aparecia em comentários) — `using` já órfão antes deste corte. Não contribuía ao
par `Cave|Save` do scanner (o arquivo é `Tests`, não `Cave`) e não foi tocado, para manter o diff
mínimo desta spec.

## Evidência

```text
tools/architecture/Get-ModularizationDependencySnapshot.ps1
  MutualModulePairs: 30 -> 29
  Cave|Save removido
  nenhum par novo apareceu

tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1
  exit 0
  7/7 projetos
  0 warnings
  0 errors

tools/unity/RunUnityEditModeTests.ps1 -ResultsPath TestResults\cut-cave-save-editmode.xml -LogFile Logs\cut-cave-save.log
  exit 0
  2747/2747 PASS (0 failed)
```

## Pendências

Esta spec-filha fecha apenas `Cave|Save`. A modularização ampla não está concluída: ainda restam 29
pares mútuos para specs-filhas independentes (ver
`docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md`).
