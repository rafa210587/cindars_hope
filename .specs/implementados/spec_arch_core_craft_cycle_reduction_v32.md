# spec_arch_core_craft_cycle_reduction_v32

> **Status:** Implementado e BUILD_VALIDATED (Fase 1 do plano de desacoplamento `static Instance`)
> **Data:** 2026-07-08
> **Escopo:** quebrar o par mútuo `Core|Craft` sem alterar gameplay, saves, cenas, prefabs, IDs ou
> balanceamento, e **sem regeneração de cena**.

## Objetivo

Terceiro corte da Fase 1 do plano "Desacoplar managers de domínio do GameBootstrap (Core|\*), sem
regen destrutiva" (após `Core|Enemy`). `CraftingManager` ganha `static Instance` self-registrado
(molde `Audio/AudioManager.cs`, já usado em `Core|Enemy`); `GameBootstrap` para de segurar
`[SerializeField] _craftingManager`.

```text
Snapshot medido ANTES da edição: MutualModulePairs=26 (Core|Craft presente).
Snapshot medido DEPOIS de todas as edições: MutualModulePairs=25, Core|Craft ausente do output.
Nenhum par novo apareceu.
```

## Direções cortadas (duas arestas, não uma)

O diagnóstico inicial assumia que `GameBootstrap.cs` era a única aresta do par (precedente
`Core|Enemy`). Não era: havia **duas** arestas independentes mantendo o par mútuo.

1. `Core -> Craft`: `GameBootstrap.cs` segurava `[SerializeField] CraftingManager _craftingManager`
   + property pública `CraftingManager` + `using CindarsHope.Craft`, usados em `InitializeManagers()`
   (`Initialize()`) e `Shutdown()`.
2. `Core -> Craft` (segunda ocorrência, **não relacionada ao GameBootstrap**):
   `Assets/_Game/Scripts/Core/Events/CraftingEvents.cs` (eventos de GameEventBus do domínio de
   crafting — `OpenCraftingStationRequestedEvent` etc.) vivia no módulo `Core` mas tinha
   `using CindarsHope.Craft.Data;` para o enum `WorkshopType`. Essa aresta sozinha já bastava para
   manter `Core|Craft` mútuo mesmo depois do corte 1 (confirmado por snapshot intermediário:
   `MutualModulePairs` continuou 26 após remover só o campo do `GameBootstrap`).
3. `Craft -> Core` (a aresta reversa que fecha o ciclo, já existente e não removida —
   `CraftingManager.cs`/`CraftingStation.cs`/`CraftingPoint.cs`/`CraftingModal.cs` usam
   `CindarsHope.Core`/`CindarsHope.Core.Events` para `GameEventBus` e outros eventos genéricos, o
   que é normal/esperado; direção pesada permanece).

## Diagnóstico (Passo 0)

Grep repo-wide por `CraftingManager` fora de `GameBootstrap.cs`/geradores de cena encontrou dois
consumidores de código: `SceneManagement/FarmSceneRuntimeReferenceInstaller.cs:70` (lia
`bootstrap.CraftingManager`) e `Editor/Validation/MvpSceneValidator.cs:195` (checava
`bootstrap.CraftingManager == null` para FarmScene). `SaveManager.cs` **não** referencia
`CraftingManager` (usa `CraftingRuntime`, uma classe diferente do mesmo namespace `CindarsHope.Craft`
— fora de escopo, conforme instrução da tarefa; `using CindarsHope.Craft` de `SaveManager.cs` foi
mantido).

## Implementação

1. **`Assets/_Game/Scripts/Craft/CraftingManager.cs`** — adicionado `public static CraftingManager
   Instance { get; }` setado em novo `Awake()` (guard de duplicata idêntico ao molde
   `AudioManager.cs`/`BestiaryManager.cs`) e limpo em novo `OnDestroy()`. Nenhuma lógica de crafting
   (`TryCraft`/`CanCraft`/inventário) alterada.
2. **`Core/Bootstrap/GameBootstrap.cs`** — removidos: `using CindarsHope.Craft;`, o campo
   `[SerializeField] _craftingManager`, a property pública `CraftingManager`. `InitializeManagers()`
   e `Shutdown()` chamam `CindarsHope.Craft.CraftingManager.Instance` (nome totalmente qualificado,
   sem `using`) em vez do campo. Confirmado por grep: `GameBootstrap.cs` só contém o token `Craft`
   dentro de `CindarsHope.Craft.CraftingManager.Instance`.
3. **`SceneManagement/FarmSceneRuntimeReferenceInstaller.cs`** — `craftingManager` resolvido via
   `CraftingManager.Instance` em vez de `bootstrap.CraftingManager`.
4. **`Editor/Validation/MvpSceneValidator.cs`** — o check `bootstrap.CraftingManager == null` (que
   lia o campo serializado do componente na cena, em modo editor — `Instance` estaria sempre null
   fora de Play Mode) foi trocado por `FindComponent<CraftingManager>(rootObjects) == null`, mesmo
   padrão já usado no arquivo para outros componentes de cena (`DebugHud` etc.). Preserva a mesma
   garantia: FarmScene precisa ter o componente `CraftingManager` na hierarquia.
5. **`Core/Events/CraftingEvents.cs` movido para `Craft/Events/CraftingEvents.cs`**, namespace
   `CindarsHope.Core.Events` → `CindarsHope.Craft.Events`. Precedente vivo: `World/Events`
   (`WorldEventHooks` etc.) já hospeda eventos de domínio fora de `Core.Events`. Consumidores
   atualizados (troca de `using CindarsHope.Core.Events;` por `using CindarsHope.Craft.Events;`,
   mantendo `using CindarsHope.Core.Events;` onde o arquivo também usa outro evento de Core não
   relacionado a crafting):
   - `Craft/CraftingStation.cs` — mantém `using CindarsHope.Core.Events;` (usa `ItemCraftedEvent`,
     que não é do domínio de crafting-estação) + novo `using CindarsHope.Craft.Events;`.
   - `Craft/CraftingPoint.cs` — troca completa (só usava `OpenCraftingStationRequestedEvent`).
   - `UI/Crafting/CraftingModal.cs` — troca completa (só usava os eventos de estação de crafting).
   - `Tests/EditMode/Craft/CraftingPointTests.cs` — troca completa.
6. **Geradores de cena** (`Editor/SceneCreation/CreateMvpFarmScene.cs` /
   `CreateMvpTownScene.cs`) **não foram editados** (diff mínimo, autorizado pela tarefa): a linha
   `SetReference(serializedBootstrap, "_craftingManager", ...)` no gerador de Farm agora aponta para
   um campo que não existe mais no `GameBootstrap` — vira no-op inofensivo (log de warning) na
   próxima regen (não executada nesta sessão). `AddComponent<CraftingManager>()` nos geradores de
   Farm/Town continua — o componente da cena se auto-registra como `Instance` via `Awake()`.
7. **Ratchet de arquitetura** — baseline `tools/architecture/architecture-ratchet-baseline.tsv`
   ganhou `SingletonDeclaration	Assets/_Game/Scripts/Craft/CraftingManager.cs	1` (ordem alfabética
   preservada), mesmo padrão já sancionado para `AudioManager`/`BestiaryManager`.
8. **`.csproj` gerado** — `CindarsHope.Runtime.csproj` (gitignored, não commitado) tinha o path
   antigo `Core\Events\CraftingEvents.cs` hardcoded (Unity fechado, sem regen automática de csproj);
   corrigido manualmente para `Craft\Events\CraftingEvents.cs` só para permitir o gate de build
   local — não versionado, próxima abertura do Unity regenera corretamente.
9. Nenhum schema/save/valor/comportamento de gameplay alterado; nenhuma cena/prefab/asset `.unity`
   editado manualmente; nenhuma regeneração de cena executada.

## Evidência (4 gates enxutos, nesta ordem)

```text
1) tools/architecture/Get-ModularizationDependencySnapshot.ps1
   MutualModulePairs: 26 -> 25
   Core|Craft ausente do output pós-corte; nenhum par novo.
   (Checkpoint intermediário: só o corte do GameBootstrap manteve 26 — a aresta de
   Core/Events/CraftingEvents.cs -> Craft.Data.WorkshopType precisou ser cortada também, item 5.)

2) tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1
   exit 0
   7/7 projetos (Editor, Runtime, Tests.EditMode, Gameplay, Foundation, Assembly-CSharp,
   Tests.PlayMode.Composition)
   0 warnings, 0 errors

3) tools/unity/RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Architecture"
   8/8 PASS, exit 0 (ratchet cobre o novo SingletonDeclaration)
   tools/unity/RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Save"
   69/69 PASS, exit 0
   tools/unity/RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Craft"
   26/26 PASS, exit 0 (cobre CraftingPointTests.cs pós-mudança de using)

4) Unity.exe -batchmode -nographics -runTests -testPlatform PlayMode
   (GameRuntimeCompositionRootPlayModeTests: GameplayScenes_LoadWithoutMissingScripts +
   RootAndTracker_RemainSingleAcrossPlayModeFrames)
   total=2 passed=2 failed=0, exit 0 (test-run xml). Log revisado: nenhuma ocorrência de
   "missing script"/exceção relacionada a CraftingManager. Warning pré-existente e não relacionado
   ("[CraftingStationRuntimeBootstrap] No CraftingRuntime found in scene") já aparecia antes do
   corte (classe diferente, CraftingRuntime != CraftingManager).
```

## Pendências

Fecha `Core|Craft`. O plano de desacoplamento continua com `Core|Economy`
(`EconomyManager`/`ShopManager`) na Fase 1, e Fases 2/3 para os managers de maior fan-out
(`Core|Skills`, `Core|Player`, `Core|Equipment`, `Core|Inventory`, `Core|UI`) — ver
`docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md` (linha `Core|Craft` marcada FEITO nesta sessão)
e o plano `C:\Users\Rafa\.claude\plans\replicated-juggling-axolotl.md`.

Regen de cena para remover o `AddComponent<CraftingManager>()` + `SetReference` stray dos geradores
fica como limpeza cosmética opcional (não bloqueadora), conforme o plano.
