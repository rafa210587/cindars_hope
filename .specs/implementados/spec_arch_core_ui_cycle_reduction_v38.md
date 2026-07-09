# spec_arch_core_ui_cycle_reduction_v38

> **Status:** Implementado e BUILD_VALIDATED
> **Data:** 2026-07-09
> **Escopo:** quebrar o par mútuo `Core|UI` sem alterar gameplay, saves, cenas, prefabs, IDs ou
> balanceamento; port `IModalStateProvider` em `CindarsHope.Foundation` + `DomainManagerRegistry`
> (infra construída na spec `Core|Inventory` v36) para `Core.GameTimeManager` consultar
> `HasActiveModal` sem referenciar `CindarsHope.UI.Modal`.

## Objetivo

Fechar mais um par residual da modularização ampla (Fase 3, alto fan-out, do plano
`Desacoplar managers de domínio do GameBootstrap (Core|*), sem regen destrutiva`
(`C:\Users\Rafa\.claude\plans\replicated-juggling-axolotl.md`) e do Tier 2 de
`docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md`).

```text
Medido antes desta sessão: MutualModulePairs=20 (com Core|UI presente).
Medido após o corte: MutualModulePairs=19, sem o par Core|UI. Nenhum par novo apareceu.
```

## Diagnóstico (Passo 0 — verificação obrigatória, revalidada no disco)

Grep de `CindarsHope.UI` em toda a pasta `Assets/_Game/Scripts/Core/` confirmou exatamente 2
arestas (via `using` de topo de arquivo — o único formato que
`Get-ModularizationDependencySnapshot.ps1` conta como edge):

1. `Core/GameTimeManager.cs` — `using CindarsHope.UI.Modal;` + `[SerializeField] ModalManager
   _modalManager;`, lido em `Update()` (`_modalManager != null && _modalManager.HasActiveModal`)
   para pausar o tick de tempo enquanto um modal está aberto.
2. `Core/Bootstrap/GameBootstrap.cs` — `using CindarsHope.UI.Modal;` + `[SerializeField]
   ModalManager _modalManager;` + property pública `ModalManager`, consumida por ~40 arquivos via
   `GameBootstrap.Instance.ModalManager`/`bootstrap.ModalManager`.

`HotbarState.SlotCount` citado no prompt como candidato **já não gera aresta**: `HotbarState.cs` já
mora em `CindarsHope.Foundation` desde o corte `Save|UI` v26; `GameBootstrap.cs:223` já o referencia
como `CindarsHope.Foundation.HotbarState.SlotCount` (fully-qualified, sem `using CindarsHope.UI.*`).
Confirmado por leitura direta do arquivo antes de qualquer edição.

## Implementação

1. **Novo port puro** `Assets/_Game/Scripts/Foundation/IModalStateProvider.cs`
   (`CindarsHope.Foundation`, `bool HasActiveModal { get; }`) — molde `DomainManagerRegistry`
   (v36). Allowlist de `ArchitectureRatchetTests.FoundationAssembly_ContainsOnlyTheCuratedPureContracts`
   ganhou `IModalStateProvider.cs`.
2. **`UI/Modal/ModalManager.cs`**: ganhou `IModalStateProvider` na assinatura da classe e
   `Awake`/`OnDestroy` que chamam `DomainManagerRegistry.Register<IModalStateProvider>(this)`/
   `DomainManagerRegistry.Unregister<IModalStateProvider>()` (molde Core|Inventory/Core|Player —
   registry genérico, não `static Instance`, já que `ModalManager` não tinha essa restrição de
   ratchet mas o port é o padrão pedido explicitamente pelo prompt de execução). Ganhou
   `using CindarsHope.Foundation;`.
3. **`Core/GameTimeManager.cs`**: `using CindarsHope.UI.Modal;` removido; campo
   `[SerializeField] ModalManager _modalManager;` removido. `Update()` resolve
   `CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Foundation.IModalStateProvider>()`
   fully-qualified a cada frame e usa `HasActiveModal` no lugar do campo — mesmo comportamento de
   pausa (early-return quando há modal ativo), só a origem da referência muda.
4. **`Core/Bootstrap/GameBootstrap.cs`**: `using CindarsHope.UI.Modal;` removido. O campo
   `[SerializeField] _modalManager` e a property pública `ModalManager` **foram mantidos** (mesma
   assinatura funcional), só com o tipo totalmente qualificado
   `CindarsHope.UI.Modal.ModalManager` — mesma técnica de `_itemDatabase`/`ItemDatabaseSO` (v36) e
   `_staminaManager`/`StaminaManager` (v37). Isso evita reapontar os ~40 consumidores de
   `GameBootstrap.Instance.ModalManager`/`bootstrap.ModalManager` (rule `code-minimalism-ladder`);
   o campo continua wireado pelos 3 geradores de cena (`SetReference(serializedBootstrap,
   "_modalManager", modalManager)`), inalterado.
5. **`Editor/SceneCreation/PlayerNeedsDataInitializer.cs`**: removida a linha
   `SetReference(gameTimeManager, "_modalManager", modalManager)` — o campo não existe mais em
   `GameTimeManager`; a chamada `SerializedObject.FindProperty("_modalManager")` retornaria `null`
   e lançaria `NullReferenceException` na próxima regen se a linha permanecesse. A assinatura
   pública `ConfigureRuntimeManagers(GameBootstrap, TimeManager, ModalManager)` foi mantida
   inalterada (chamada pelos 3 geradores de cena) — o parâmetro `modalManager` fica sem uso interno
   nesta função, sem quebrar compilação (parâmetro não utilizado não é erro em C#).
6. **`Editor/Validation/MvpSceneValidator.cs`** (`ValidateSpec09Bootstrap`): removida a checagem
   `gameTime.FindProperty("_modalManager").objectReferenceValue == null` pelo mesmo motivo (campo
   removido de `GameTimeManager`) — teria lançado `NullReferenceException` na próxima execução de
   `CindarsHope/Validar Projeto`. O wiring do `ModalManager` no bootstrap continua coberto por
   `bootstrap.ModalManager` (property mantida) em outros validators que já checam essa referência.
7. `CindarsHope.Foundation.csproj`: `<Compile Include>` do novo arquivo ajustado manualmente
   (Unity fechado neste ambiente); confirmado depois que a regeneração automática do Unity (rodada
   por `RunUnityEditModeTests.ps1`) produziu exatamente o mesmo resultado.
8. Nenhum schema/campo/valor/nome de classe de save alterado; nenhuma cena/prefab/asset editado
   manualmente; nenhum comportamento de gameplay alterado; `SaveManager.cs` não tocado.

## Erro corrigido durante a execução (mesmo padrão do v37/v36/v29/v24/v22)

1ª rodada do filtro `CindarsHope.Tests.EditMode.Architecture` teve 1 falha real:
`FoundationAssembly_ContainsOnlyTheCuratedPureContracts` — o comentário novo em
`IModalStateProvider.cs` continha a substring literal `"UnityEngine"` (dentro de "sem UnityEngine"),
disparando o guard textual do próprio teste (`Foundation source must not depend on UnityEngine`).
Corrigido reescrevendo o comentário para "port C# puro, independente de engine" (sem a substring);
2ª rodada: 8/8 PASS.

## Risco residual documentado

1. **Ordem de `Awake`** (mesmo padrão dos cortes irmãos v36/v37): `ModalManager.Awake()` registra
   no `DomainManagerRegistry` antes de `GameTimeManager.Update()` precisar dele — não há
   dependência de ordem de inicialização crítica aqui porque o consumo é em `Update()` (roda todo
   frame, não só no primeiro), então mesmo que `ModalManager.Awake()` rode um frame depois de
   `GameTimeManager.Update()` começar, o `Get<IModalStateProvider>()` simplesmente retorna `null`
   nesse frame (early-return não aciona, tick não pausa) até o registro completar — sem exceção,
   sem quebra visível, diferente do padrão de `GameBootstrap.InitializeManagers()` (que só roda
   uma vez em `Awake`).
2. **Sem teste automatizado dedicado ao comportamento "tick pausa com modal ativo"**: não existe
   suíte EditMode para `GameTimeManager` neste repositório (grep confirmou só
   `ArchitectureRatchetTests.cs` e `FoundationPortsTests.cs` mencionam `GameTimeManager`, nenhum
   teste comportamental). O comportamento foi preservado por leitura de código (mesma condição de
   early-return, só a fonte do `HasActiveModal` mudou) e por PlayMode de composição (carrega sem
   erro), mas não há cobertura automatizada regressiva desse comportamento específico — mesmo
   estado de antes desta spec (o campo serializado antigo também não tinha teste dedicado).

## Evidência

```text
tools/architecture/Get-ModularizationDependencySnapshot.ps1
  MutualModulePairs: 20 (medido nesta sessão, antes de qualquer edição) -> 19 (medido após)
  Core|UI removido (confirmado)
  nenhum par novo apareceu

tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1
  exit 0
  7/7 projetos, 0 warnings, 0 errors
  (1ª rodada falhou por csproj desatualizado — Unity fechado, 1 .cs novo — corrigido com patch
  manual do <Compile Include> em CindarsHope.Foundation.csproj antes de reexecutar)

tools/unity/RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Architecture"
  TestResults\cut-core-ui-arch.xml, Logs\cut-core-ui-arch.log
  exit 0, 8/8 PASS
  (1ª rodada teve 1 falha real: FoundationAssembly_ContainsOnlyTheCuratedPureContracts — comentário
  novo continha a substring literal "UnityEngine" dentro de "sem UnityEngine", disparando o guard
  textual do próprio teste. Corrigido reescrevendo o comentário; 2ª rodada: 8/8 PASS)

tools/unity/RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Save"
  TestResults\cut-core-ui-save.xml, Logs\cut-core-ui-save.log
  exit 0, 69/69 PASS

tools/unity/RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.UI"
  TestResults\cut-core-ui-ui.xml, Logs\cut-core-ui-ui.log
  exit 0, 366/366 PASS

Unity.exe -batchmode -nographics -runTests -testPlatform PlayMode
  TestResults\cut-core-ui-playmode.xml, Logs\cut-core-ui-playmode.log
  2/2 PASS (RootAndTracker_RemainSingleAcrossPlayModeFrames +
  GameplayScenes_LoadWithoutMissingScripts nas 3 cenas MVP); log sem
  missing-script/NullReferenceException/erro relacionado a Modal.
```

## Pendências

`Core|UI` sai da lista de pares mútuos, mas a modularização ampla **não está concluída**: restam
19 pares mútuos, todos de fan-out alto/misto da Fase 3, incluindo `Craft|UI`, `NPC|UI`, `Player|UI`,
`UI|World` (ver `docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md` e a lista completa no output do
`Get-ModularizationDependencySnapshot.ps1` desta sessão).
