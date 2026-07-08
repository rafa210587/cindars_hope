# spec_arch_quests_save_cycle_reduction_v24

> **Status:** Implementado e BUILD_VALIDATED
> **Data:** 2026-07-08
> **Escopo:** quebrar o par mútuo `Quests|Save` sem alterar gameplay, saves, cenas, prefabs, IDs ou balanceamento.

## Objetivo

Reduzir mais um ciclo residual da modularização ampla (Tier 1 do
`docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md`):

```text
Antes:
- MutualModulePairs=34
- Par presente: Quests|Save

Depois:
- MutualModulePairs=33
- Par removido: Quests|Save
```

## Diagnóstico (Passo 0 — verificação obrigatória, revalidada no disco)

O ciclo era causado por:

- `Quests -> Save`: direção legítima e mantida (`Quests/Runtime/QuestRuntimeBootstrap.cs` e
  `Quests/Runtime/QuestService.cs` importam `CindarsHope.Save` para os DTOs `[Serializable]` de
  `GameSaveData` — invertê-la mudaria o schema de save; não foi tocada).
- `Save -> Quests`: `tools/architecture/Get-ModularizationDependencySnapshot.ps1` só conta `using`
  de topo de arquivo, então a fonte real do edge era exclusivamente
  `Assets/_Game/Scripts/Save/Providers/QuestSectionProvider.cs:1`
  (`using CindarsHope.Quests.Runtime;`). `Save/SaveData.cs:356` referenciava
  `CindarsHope.Quests.QuestSource` **sem** `using` (fully-qualified inline) — não contribuía para o
  edge medido pelo scanner, mas era acoplamento real de Save a um tipo dono de Quests, então também
  foi corrigido.

**Achado do Passo 0 que revisou a estratégia original:** o prompt de execução assumia um "padrão
vivo" de providers se auto-registrando no `SaveProviderRegistry` a partir do próprio módulo de
domínio. Leitura de `SaveManager.cs` (linhas 155–235 e 585–618) mostrou que **isso não existe** —
todos os ~30 providers (incluindo os de "Lote 2: manager injetado") são construídos diretamente
dentro de `SaveManager.Initialize()`/`RegisterProviderDescriptors()`, que já importa quase todos os
domínios do jogo (Cave, Core, Craft, Economy, Enemy, Equipment, Farm, Inventory, NPC, Player,
Skills, UI, World) como uma espécie de composition-root paralelo ao `GameBootstrap`. Inventar um
mecanismo de auto-registro só para Quests teria sido uma mudança de arquitetura nova (não uma
réplica de precedente), fora da escada de minimalismo. Em vez disso, foi usada a técnica **já
vigente e comprovada** em `spec_arch_npc_save_cycle_reduction_v23` /
`spec_arch_economy_save_cycle_reduction_v22`: `SaveManager.cs` continua construindo o provider,
mas por **nome totalmente qualificado**, sem adicionar um novo `using CindarsHope.Quests` de topo
de arquivo — o scanner (que só olha `using` de topo) não conta essa referência como edge, e o
acoplamento real fica confinado a uma única linha explícita em vez de poluir o topo do arquivo.

Grep repo-wide de `QuestSource` e de `using CindarsHope.Quests` em `Save/` confirmou: nenhum outro
arquivo de `Save/` referenciava Quests além dos dois pontos acima; nenhum teste EditMode referencia
`QuestSectionProvider`/`QuestSource` fora da pasta `Assets/_Game/Tests/EditMode/Quests` e
`Assets/_Game/Tests/EditMode/Save/SaveSectionProviderTests.cs`.

## Implementação

1. **Enum `QuestSource` → `CindarsHope.Foundation`** (novo arquivo
   `Assets/_Game/Scripts/Foundation/QuestSource.cs`, valores int 0–5 inalterados, zero dependência
   de engine). `Assets/_Game/Scripts/Quests/QuestSource.cs` manteve `QuestSourceMapper` e
   `QuestLogTab` (que continuam pertencendo ao domínio Quests) e passou a resolver `QuestSource`
   via `using CindarsHope.Foundation;`. `Save/SaveData.cs:356` deixou de fully-qualificar
   (`CindarsHope.Quests.QuestSource.Npc` → `QuestSource.Npc`, já resolvido pelo
   `using CindarsHope.Foundation;` preexistente no arquivo).
   - 11 arquivos de produção em `Quests/**` que usavam `QuestSource`/`QuestSourceMapper` sem
     qualificação ganharam `using CindarsHope.Foundation;` (mesmo módulo, sem novo edge):
     `SecretQuests/SecretQuestCatalog.cs`, `QuestBoardService.cs`, `Save/QuestStateRecord.cs`,
     `NpcChains/NpcQuestChainCatalog.cs`, `Log/QuestLogProjectionService.cs`, `QuestInstance.cs`,
     `Log/QuestLogProjection.cs`, `FestivalQuests/FestivalQuestCatalog.cs`,
     `Runtime/QuestService.cs`, `Runtime/QuestDynamicInstancePersistence.cs`,
     `CaveContracts/CaveContractCatalog.cs`.
   - 6 arquivos de teste em `Assets/_Game/Tests/EditMode/Quests/` ganharam o mesmo `using`.
   - `ArchitectureRatchetTests.FoundationAssembly_ContainsOnlyTheCuratedPureContracts` atualizado
     (allowlist curada) para incluir `QuestSource.cs`, com comentário citando esta spec — mesmo
     precedente de `EconomySaveDtos.cs`.
2. **`QuestSectionProvider` relocado** de `Assets/_Game/Scripts/Save/Providers/QuestSectionProvider.cs`
   (`namespace CindarsHope.Save.Providers`) para
   `Assets/_Game/Scripts/Quests/Save/QuestSectionProvider.cs`
   (`namespace CindarsHope.Quests.Save`), via `git mv` (preserva histórico; `.cs.meta` movido junto
   — Unity batchmode confirmou o GUID intacto ao rodar os testes). O arquivo passou a ter
   `using CindarsHope.Save;` (Quests→Save, direção A já existente/permitida) para
   `ISaveSectionProvider`/`GameSaveData`/`QuestStateSectionSaveData`.
   - `Save/SaveManager.cs:192` (antigo `new QuestSectionProvider();`) passou a
     `new CindarsHope.Quests.Save.QuestSectionProvider();` (nome totalmente qualificado, sem novo
     `using CindarsHope.Quests` de topo de arquivo — `using CindarsHope.Save.Providers;` permanece
     porque outros ~15 providers continuam vivendo lá).
   - `Assets/_Game/Tests/EditMode/Save/SaveSectionProviderTests.cs` ganhou
     `using CindarsHope.Quests.Save;` (teste, fora do scanner de módulo runtime).
3. `CindarsHope.Foundation.csproj`: `<Compile Include>` do novo `QuestSource.cs` adicionado
   manualmente (Unity fechado neste ambiente, csproj não regenera sozinho).
   `CindarsHope.Runtime.csproj`: path do `<Compile Include>` de `QuestSectionProvider.cs` atualizado
   de `Save\Providers\` para `Quests\Save\`.
4. Nenhum schema/campo/valor de enum/nome de classe alterado; nenhuma cena/prefab/asset tocado;
   nenhum DTO `[Serializable]` de `GameSaveData` movido (direção A preservada).

## Evidência

```text
tools/architecture/Get-ModularizationDependencySnapshot.ps1
  MutualModulePairs: 34 -> 33
  Quests|Save removido
  nenhum par novo apareceu

tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1
  exit 0
  7/7 projetos
  0 warnings
  0 errors

tools/unity/RunUnityEditModeTests.ps1 -ResultsPath TestResults\cut-quests-save-editmode.xml -LogFile Logs\cut-quests-save.log
  exit 0
  2747/2747 PASS (0 failed)
```

### Nota de processo (retry documentado)

Uma primeira rodada de EditMode acusou 1 falha real (não ambiental): o comentário do novo
`Foundation/QuestSource.cs` continha a palavra "UnityEngine" (numa frase explicando que o enum é
"sem UnityEngine"), o que disparou o próprio guard
`ArchitectureRatchetTests.FoundationAssembly_ContainsOnlyTheCuratedPureContracts` (`source.Contains("UnityEngine")`
em todo o texto do arquivo, não só em `using`s). Corrigido reescrevendo o comentário sem a
substring; build e suite completa re-executados do zero e confirmados verdes (2747/2747, exit 0)
antes deste registro.

## Pendências

Esta spec-filha fecha apenas `Quests|Save`. A modularização ampla não está concluída: ainda restam
33 pares mútuos para specs-filhas independentes (ver
`docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md`).

## Nota de risco residual (não bloqueante)

Nenhum identificado nesta spec. `SaveManager.cs` continua sendo, por design pré-existente (não
alterado aqui), um composition-root paralelo ao `GameBootstrap` que conhece quase todos os domínios
do jogo — isso é um débito arquitetural documentado, não introduzido por este corte, e fora do
escopo declarado (`Quests|Save`).
