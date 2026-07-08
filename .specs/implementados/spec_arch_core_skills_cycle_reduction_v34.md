# spec_arch_core_skills_cycle_reduction_v34

> **Status:** Implementado e BUILD_VALIDATED (Fase 2 do plano de desacoplamento `static Instance`)
> **Data:** 2026-07-08
> **Escopo:** quebrar o par mútuo `Core|Skills` sem alterar gameplay, saves, cenas, prefabs, IDs ou
> balanceamento, e **sem regeneração de cena**.

## Objetivo

Primeiro corte da Fase 2 (médio fan-out) do plano "Desacoplar managers de domínio do GameBootstrap
(Core|\*), sem regen destrutiva" (após `Core|Enemy`, `Core|Craft`, `Core|Economy` na Fase 1).
`SkillTreeManager` ganha `static Instance` self-registrado (molde `Audio/AudioManager.cs`/
`Craft/CraftingManager.cs`/`Economy/ShopManager.cs`); `SkillActionDatabaseSO` (a segunda aresta,
diferente do corte Economy) é relocado para o namespace `CindarsHope.Skills`; `GameBootstrap` para de
segurar `[SerializeField] _skillTreeManager`/`_skillActionDatabase`.

```text
Snapshot medido DEPOIS de todas as edições: MutualModulePairs=23, Core|Skills ausente do output.
Nenhum par novo apareceu (24 -> 23).
```

## Diagnóstico (Passo 0)

Grep repo-wide em `Assets/_Game/Scripts/Core/` por `CindarsHope.Skills` encontrou **duas** arestas
`Core -> Skills` (diferente do corte `Core|Economy`, que teve só uma):

1. `Core/Bootstrap/GameBootstrap.cs` — `using CindarsHope.Skills;` + campo
   `[SerializeField] SkillActionDatabaseSO _skillActionDatabase` + campo
   `[SerializeField] Skills.SkillTreeManager _skillTreeManager` + as 2 properties públicas.
2. `Core/Data/SkillActionDatabaseSO.cs` — `class SkillActionDatabaseSO : DataRegistrySO<SkillActionSO>`
   com `using CindarsHope.Skills;` (o tipo genérico `SkillActionSO` é de Skills).

## Implementação

1. **`Assets/_Game/Scripts/Skills/SkillActionDatabaseSO.cs`** (novo path; movido via `git mv` de
   `Core/Data/SkillActionDatabaseSO.cs` + `.meta`, GUID preservado). Namespace
   `CindarsHope.Core.Data` → `CindarsHope.Skills`. Conteúdo inalterado além do namespace/using
   (`using CindarsHope.Core.Data;` adicionado para o `DataRegistrySO<T>` base, que continua em
   `Core.Data`).
2. **`Assets/_Game/Scripts/Skills/SkillActionExecutor.cs`** — removido `using CindarsHope.Core.Data;`
   (ficou redundante: `SkillActionDatabaseSO` agora está no mesmo namespace `CindarsHope.Skills` do
   arquivo).
3. **`Assets/_Game/Scripts/Skills/SkillTreeManager.cs`** — adicionado
   `public static SkillTreeManager Instance { get; private set; }` setado em `Awake()` (guard de
   duplicata idêntico ao molde AudioManager/CraftingManager/ShopManager: `Destroy(gameObject)` se já
   existir outra instância) e limpo em novo `OnDestroy()`. Nenhuma lógica de skill tree (compra,
   rank-up, respec, slots ativos, save/load) alterada. Diferente do corte `Core|Economy`
   (`EconomyManager` não pôde ganhar `Instance` por causa da regra `GlobalGoldAccess`), aqui **nenhuma**
   regra `GlobalXAccess` do `architecture-ratchet-rules.tsv` cobre `SkillTreeManager` — só existem
   `GlobalInventoryAccess`/`GlobalGoldAccess`, nenhuma para Skills — então o padrão `Instance` puro
   aplicou sem desvio.
4. **`Core/Bootstrap/GameBootstrap.cs`** — removidos: `using CindarsHope.Skills;`, os campos
   `[SerializeField] _skillActionDatabase`/`_skillTreeManager`, as properties públicas
   `SkillActionDatabase`/`SkillTreeManager`. `InitializeManagers()` resolve
   `var skillTreeManager = CindarsHope.Skills.SkillTreeManager.Instance;` (nome totalmente
   qualificado, sem `using`, para não recriar a aresta) e usa essa variável local tanto no
   `RebindProgressionManager(...)` quanto na chamada a
   `_saveManager.RebindOptionalRuntimeManagers(...)` (que já tinha o parâmetro `SkillTreeManager` na
   assinatura de `SaveManager` — `Save -> Skills` está fora de escopo desta spec). Confirmado por grep:
   `GameBootstrap.cs` só contém o token `Skills` dentro de `CindarsHope.Skills.SkillTreeManager`
   totalmente qualificado.
5. **`SceneManagement/FarmSceneRuntimeReferenceInstaller.cs` / `TownSceneRuntimeReferenceInstaller.cs` /
   `CaveSceneRuntimeReferenceInstaller.cs`** — a chamada a `saveManager.RebindOptionalRuntimeManagers(...)`
   troca `bootstrap.SkillTreeManager` por `CindarsHope.Skills.SkillTreeManager.Instance` (fully-qualified;
   nenhum desses 3 arquivos tinha `using CindarsHope.Skills` antes, então evitou introduzir aresta nova
   em `SceneManagement`).
6. **Consumidores de `bootstrap.SkillTreeManager`/`GameBootstrap.Instance?.SkillTreeManager`**
   reapontados para `SkillTreeManager.Instance` (12 arquivos que já tinham `using CindarsHope.Skills`
   ou já estavam dentro do namespace `CindarsHope.Skills`/`CindarsHope.Skills.Runtime.Effects`):
   `UI/DebugHud.cs`, `Combat/PlayerAttackController.cs`, `Player/InferredClassRuntime.cs` (2 pontos),
   `UI/HUD/GameplayHudRuntimeBinder.cs`, `UI/Locations/AnyaFountainMenu.cs` (2 pontos),
   `UI/Skills/SkillTreeGameplayPanelController.cs` (5 pontos), `UI/Skills/SkillTreePanel.cs`,
   `Skills/Runtime/Effects/ActiveSkillExecutionController.cs`, `Skills/ActiveSkillSlots.cs`. Nos 4
   arquivos sem esse `using` (`Player/PlayerVitalsApplier.cs`, `Combat/PlayerDamageReceiver.cs`,
   `Fonte/FonteInteractable.cs`, `Magic/SpellItemUseController.cs`), usado
   `CindarsHope.Skills.SkillTreeManager.Instance` fully-qualified — `Player|Skills` e `Combat|Skills`
   já tinham aresta de outros arquivos do mesmo módulo (não seria par novo de qualquer forma), mas
   `Fonte|Skills`/`Magic|Skills` não tinham nenhuma, então fully-qualified evitou criar aresta nova
   nesses dois módulos.
7. **`Editor/Validation/ValidateSkillTreeRuntimeBinding.cs`** — o check
   `bootstrap.SkillTreeManager != null` (que lia o antigo serialized field, válido também em Edit
   Mode) foi substituído por um check gateado em `Application.isPlaying`:
   `SkillTreeManager.Instance != null` só é significativo em Play Mode (o `Instance` só se popula em
   `Awake`, que não roda em Edit Mode); fora de Play Mode agora reporta SKIP em vez de um FAIL falso.
   O check de `bootstrap.GetComponent<SkillTreeManager>()` (componente presente na cena, válido em
   Edit Mode) foi mantido sem alteração.
8. **`Save/SaveManager.cs`** — **não tocado**, conforme instrução (`Save|Skills` fora de escopo deste
   corte): mantém `[SerializeField] Skills.SkillTreeManager _skillTreeManager` e o parâmetro
   `Skills.SkillTreeManager skillTreeManager = null` em `RebindOptionalRuntimeManagers(...)`.
9. **Geradores de cena** (`CreateMvpFarmScene.cs`/`CreateMvpTownScene.cs`/`CreateMvpCaveScene.cs`,
   `Editor/Spec17CSceneWiringInitializer.cs`) **não foram editados** (regen fora de escopo, per
   instrução da tarefa): `AddComponent<SkillTreeManager>()` continua; a linha
   `SetReference(serializedBootstrap, "_skillTreeManager", ...)` agora aponta para um campo que não
   existe mais em `GameBootstrap` — vira no-op silencioso (`SetReference` usa `SerializedObject.
   FindProperty` por nome de string, loga warning e retorna, não quebra compilação) na próxima regen
   (não executada nesta sessão).
10. **Csproj gerado** — `CindarsHope.Runtime.csproj` ainda listava o path antigo
    `Assets\_Game\Scripts\Core\Data\SkillActionDatabaseSO.cs` (Unity estava fechado e não regenerou o
    csproj após o `git mv`). Corrigido manualmente para `Assets\_Game\Scripts\Skills\
    SkillActionDatabaseSO.cs` — **não commitado** (Unity regenera esse arquivo na próxima abertura do
    editor; mudança local necessária só para o gate `dotnet build` rodar nesta sessão).
11. **Ratchet de arquitetura** — baseline `tools/architecture/architecture-ratchet-baseline.tsv` ganhou
    `SingletonDeclaration	Assets/_Game/Scripts/Skills/SkillTreeManager.cs	1` (ordem alfabética
    preservada, após `Skills/Runtime/Effects/ActiveSkillExecutionController.cs`).
12. Nenhum schema/save/valor/comportamento de gameplay alterado; nenhuma cena/prefab/asset `.unity`
    editado manualmente; nenhuma regeneração de cena executada.

## Evidência (gates, nesta ordem)

```text
1) tools/architecture/Get-ModularizationDependencySnapshot.ps1
   MutualModulePairs: 24 -> 23
   Core|Skills ausente do output pós-corte; nenhum par novo (lista completa dos 23 pares confere:
   Cave|Combat, Cave|Core, Cave|Enemy, Cave|SceneManagement, Combat|Core, Combat|Enemy, Combat|Player,
   Combat|Skills, Core|Equipment, Core|Inventory, Core|Player, Core|Save, Core|UI, Craft|UI, Farm|Save,
   Inventory|Player, NPC|Quests, NPC|UI, NPC|World, Player|Skills, Player|UI, Player|World, UI|World).

2) tools/architecture/Test-ArchitectureRatchet.ps1
   exit 0. SingletonDeclaration: current=59 baseline_max=59 (novo entry SkillTreeManager.cs coberto).
   GlobalGoldAccess/GlobalInventoryAccess: 0/0, inalterado.

3) tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1
   exit 0
   7/7 projetos (Editor, Runtime, Tests.EditMode, Gameplay, Foundation, Assembly-CSharp,
   Tests.PlayMode.Composition)
   0 warnings, 0 errors (após corrigir o path stale no CindarsHope.Runtime.csproj, ver item 10)

4) tools/unity/RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Architecture"
   8/8 PASS, exit 0
   tools/unity/RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Save"
   69/69 PASS, exit 0
   tools/unity/RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Skills"
   28/28 PASS, exit 0

5) Unity.exe -batchmode -nographics -runTests -testPlatform PlayMode
   (GameRuntimeCompositionRootPlayModeTests: GameplayScenes_LoadWithoutMissingScripts +
   RootAndTracker_RemainSingleAcrossPlayModeFrames)
   total=2 passed=2 failed=0, exit 0 (test-run xml). Log revisado: sem exceção/missing-script
   relacionado a SkillTree/SkillAction; stack traces vistas são só de `RuntimeInitializeOnLoad`
   normal (`SkillTreeGameplayPanelController.Install/Awake`).
```

## Pendências

Fecha `Core|Skills`. A Fase 2 do plano continua com `Core|Player`
(`ProgressionManager`/`StatusEffectManager`) e `Core|Equipment` (`EquipmentManager`) — ver
`docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md` e
`docs/architecture/CLAUDE_MODULARIZATION_HANDOFF.md` (entrada 2026-07-08 Core/Skills v34) e o plano
`C:\Users\Rafa\.claude\plans\replicated-juggling-axolotl.md`. Fase 3 (Inventory/Player/UI, alto
fan-out) fica para depois.

Regen de cena para remover o `AddComponent<SkillTreeManager>()` + `SetReference` stray dos geradores
fica como limpeza cosmética opcional (não bloqueadora), conforme o plano. `Save|Skills` permanece um
par mútuo separado (fora de escopo desta spec) — `SaveManager.cs` continua com seu próprio campo
serializado de `SkillTreeManager`.
