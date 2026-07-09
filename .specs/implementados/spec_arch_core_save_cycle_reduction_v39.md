# spec_arch_core_save_cycle_reduction_v39

> **Status:** Implementado e BUILD_VALIDATED
> **Data:** 2026-07-09
> **Escopo:** quebrar o par mútuo `Core|Save` — o último `Core|*` residual — sem alterar gameplay,
> saves, cenas, prefabs, IDs ou balanceamento; `GameTimeManager` passa a expor um setter primitivo
> em vez de receber o DTO `GameTimeSaveData`; `GameBootstrap` mantém o campo/property `SaveManager`
> como shim fully-qualified (precedente `_itemDatabase`/`_modalManager`/`_staminaManager`).

## Objetivo

Fechar o último par `Core|*` do plano `Desacoplar managers de domínio do GameBootstrap (Core|*), sem
regen destrutiva` (`C:\Users\Rafa\.claude\plans\replicated-juggling-axolotl.md`) e do Tier 3 de
`docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md`.

```text
Medido antes desta sessão: MutualModulePairs=19 (com Core|Save presente).
Medido após o corte: MutualModulePairs=18, sem o par Core|Save. Nenhum par novo apareceu.
```

## Diagnóstico (Passo 0 — verificação obrigatória, revalidada no disco)

Grep de `CindarsHope.Save` em toda a pasta `Assets/_Game/Scripts/Core/` confirmou exatamente 2
arestas (via `using` de topo de arquivo — o único formato que
`Get-ModularizationDependencySnapshot.ps1` conta como edge):

1. `Core/Bootstrap/GameBootstrap.cs` — `using CindarsHope.Save;` + `[SerializeField] SaveManager
   _saveManager;` + property pública `SaveManager` (tipo `CindarsHope.Save.SaveManager`), consumida
   por 9 arquivos fora de Core (`SceneManagement/*RuntimeReferenceInstaller`, `UI/Title/
   TitleScreenController.cs`, `UI/System/SystemTabController.cs`, 4 validators de Editor) via
   `bootstrap.SaveManager`/`GameBootstrap.Instance.SaveManager`.
2. `Core/GameTimeManager.cs` — `using CindarsHope.Save;` + `public void
   RestoreFromSaveData(GameTimeSaveData saveData)`, que só lia dois campos primitivos do DTO
   (`CurrentPhase`, `PhaseElapsedSeconds`) para atualizar `_currentPhase`/`_phaseTimer`.

Nenhuma outra aresta encontrada — confirmado que o grep cobre toda a árvore `Core/` recursivamente.

## Implementação

1. **`Core/GameTimeManager.cs`**: `using CindarsHope.Save;` removido. `RestoreFromSaveData
   (GameTimeSaveData saveData)` substituído por `RestorePhaseState(int currentPhase, float
   phaseElapsedSeconds)` — mesmo corpo (`Mathf.Clamp`/`Mathf.Max`), só troca a origem dos dois
   valores de "campos do DTO" para "parâmetros primitivos". `GameTimeManager` não referencia mais
   nenhum tipo de `CindarsHope.Save`.
2. **`Save/Providers/GameTimeSectionProvider.cs`** (já mora em `CindarsHope.Save.Providers` e já
   importa `CindarsHope.Core`/`CindarsHope.Core.Time`): `Restore(object sectionData)` faz o cast para
   `GameTimeSaveData` como antes, mas agora chama
   `_gameTimeManager.RestorePhaseState(data.CurrentPhase, data.PhaseElapsedSeconds)` em vez de passar
   o DTO inteiro. `Capture` não mudou. `GameTimeSaveData` (schema de save) **não foi movido nem
   alterado** — continua em `Save/SaveData.cs`, mesmos 3 campos (`CurrentDay`, `CurrentPhase`,
   `PhaseElapsedSeconds`).
3. **`Core/Bootstrap/GameBootstrap.cs`**: `using CindarsHope.Save;` removido. O campo
   `[SerializeField] _saveManager` e a property pública `SaveManager` **foram mantidos** (mesma
   assinatura funcional), só com o tipo totalmente qualificado `CindarsHope.Save.SaveManager` — mesma
   técnica de `_itemDatabase`/`ItemDatabaseSO` (v36), `_staminaManager`/`StaminaManager` (v37) e
   `_modalManager`/`ModalManager` (v38). Isso evita reapontar os 9 consumidores existentes de
   `GameBootstrap.Instance.SaveManager`/`bootstrap.SaveManager` (rule `code-minimalism-ladder`); o
   campo continua wireado pelos 3 geradores de cena (`SetReference(bootstrap, "_saveManager",
   saveManager)`), inalterado.

Nenhum port novo (`ISaveService`) foi criado — a estratégia originalmente cogitada no prompt de
execução foi substituída pelo padrão fully-qualified shim já provado nos 3 cortes irmãos anteriores
(`Core|Inventory`/`Core|Player`/`Core|UI`), por ser estritamente menor (rule
`code-minimalism-ladder`: nenhuma abstração nova onde o fully-qualify já resolve o edge medido pelo
scanner, que só conta `using`s de topo de arquivo).

## Erro corrigido durante a execução

Nenhum. Primeira rodada de todos os gates passou verde (diferente dos cortes v36-v38, que tiveram 1
falha real de `FoundationAssembly_ContainsOnlyTheCuratedPureContracts` por causa da substring
"UnityEngine" em comentário novo — esta spec não criou nenhum arquivo novo em `Foundation/`, então
esse guard nunca disparou).

## Risco residual documentado

1. **Sem teste automatizado dedicado ao comportamento de restore de fase/tempo**: mesmo estado de
   antes desta spec — não existe suíte EditMode específica para `GameTimeManager.RestorePhaseState`
   (só `ArchitectureRatchetTests.cs` e `FoundationPortsTests.cs` mencionam `GameTimeManager`, nenhum
   teste comportamental). O comportamento foi preservado por leitura de código (mesmo
   `Mathf.Clamp`/`Mathf.Max`, só a origem dos dois valores mudou de "campos de DTO" para
   "parâmetros") e por PlayMode de composição (carrega sem erro).
2. **Round-trip de save não testado isoladamente para `game_time`**: a suíte `CindarsHope.Tests.
   EditMode.Save` (69/69 PASS) cobre o comportamento geral de save/load, mas não há um teste dedicado
   a "salvar → carregar → `GameTimeManager._currentPhase`/`_phaseTimer` batem". Como
   `GameTimeSaveData` não mudou de schema/campo e `GameTimeSectionProvider.Capture` não mudou, o risco
   é o mesmo de antes desta spec (nenhum teste dedicado já existia).

## Evidência

```text
tools/architecture/Get-ModularizationDependencySnapshot.ps1
  MutualModulePairs: 19 (medido nesta sessão, antes de qualquer edição) -> 18 (medido após)
  Core|Save removido (confirmado)
  nenhum par novo apareceu

tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1
  exit 0
  7/7 projetos, 0 warnings, 0 errors (1ª rodada já passou verde)

tools/unity/RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Architecture"
  TestResults\cut-core-save-arch.xml, Logs\cut-core-save-arch.log
  exit 0, 8/8 PASS

tools/unity/RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Save"
  TestResults\cut-core-save-save.xml, Logs\cut-core-save-save.log
  exit 0, 69/69 PASS (crítico — round-trip de save)

tools/unity/RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Core"
  TestResults\cut-core-save-core.xml, Logs\cut-core-save-core.log
  exit 0, 81/81 PASS

Unity.exe -batchmode -nographics -runTests -testPlatform PlayMode
  TestResults\cut-core-save-playmode.xml, Logs\cut-core-save-playmode.log
  2/2 PASS (RootAndTracker_RemainSingleAcrossPlayModeFrames +
  GameplayScenes_LoadWithoutMissingScripts nas 3 cenas MVP); log confirma
  "Test run completed. Exiting with code 0 (Ok)."; sem missing-script/
  NullReferenceException/erro relacionado a Save/GameTime.
```

## Pendências

`Core|Save` sai da lista de pares mútuos — este era o **último** par `Core|*` do plano de
desacoplamento do `GameBootstrap`. A modularização ampla **não está concluída**: restam 18 pares
mútuos, todos de fan-out alto/misto da Fase 3 (`Cave|Combat`, `Cave|Core`, `Cave|Enemy`,
`Cave|SceneManagement`, `Combat|Core`, `Combat|Enemy`, `Combat|Player`, `Combat|Skills`, `Craft|UI`,
`Farm|Save`, `Inventory|Player`, `NPC|Quests`, `NPC|UI`, `NPC|World`, `Player|Skills`, `Player|UI`,
`Player|World`, `UI|World` — ver `docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md` e a lista
completa no output do `Get-ModularizationDependencySnapshot.ps1` desta sessão).
