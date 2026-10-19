# spec_arch_core_equipment_cycle_reduction_v35

> **Status:** Implementado e BUILD_VALIDATED (Fase 2 do plano de desacoplamento `static Instance`)
> **Data:** 2026-07-08
> **Escopo:** quebrar o par mútuo `Core|Equipment` sem alterar gameplay, saves, cenas, prefabs, IDs
> ou balanceamento, e **sem regeneração de cena**.

## Objetivo

Segundo corte da Fase 2 (médio fan-out) do plano "Desacoplar managers de domínio do GameBootstrap
(Core|\*), sem regen destrutiva" (após `Core|Skills` v34). `EquipmentManager` ganha
`static Instance` self-registrado (molde `Craft/CraftingManager.cs`/`Economy/ShopManager.cs`/
`Skills/SkillTreeManager.cs`); `GameBootstrap` para de segurar `[SerializeField] _equipmentManager`.

```text
Snapshot medido DEPOIS de todas as edições: MutualModulePairs=22, Core|Equipment ausente do output.
Nenhum par novo apareceu (23 -> 22).
```

## Diagnóstico (Passo 0)

Grep repo-wide em `Assets/_Game/Scripts/Core/` por `CindarsHope.Equipment` encontrou **duas**
arestas `Core -> Equipment` (o enum `EquipmentSlot` já havia sido movido para
`CindarsHope.Foundation` no corte `Equipment|Save` v29 — confirmado antes de iniciar):

1. `Core/Bootstrap/GameBootstrap.cs` — `using CindarsHope.Equipment;` + campo
   `[SerializeField] EquipmentManager _equipmentManager` + property pública `EquipmentManager` +
   usos internos em `EquipStarterCombatLoadout()`, `BuildCombatInstallContext()`,
   `InitializeDeathSystem()` e na chamada a `_saveManager.RebindOptionalRuntimeManagers(...)`.
2. `Core/Bootstrap/Installers/CombatRuntimeInstallContext.cs` — `using CindarsHope.Equipment;` +
   campo público `EquipmentManager EquipmentManager`, consumido por `CombatRuntimeInstaller.cs`
   (mesma pasta, mas classe estática sem `using` próprio — lia via `context.EquipmentManager`).

## Implementação

1. **`Assets/_Game/Scripts/Equipment/EquipmentManager.cs`** — adicionado
   `public static EquipmentManager Instance { get; private set; }`, setado em `Awake()` (guard de
   duplicata idêntico ao molde Craft/Economy/Skills: `Destroy(gameObject)` se já existir outra
   instância) e limpo em novo `OnDestroy()`. Nenhuma lógica de equipamento (slots, durabilidade,
   infusão, upgrade, acessórios, save/restore) alterada. Nenhuma regra `GlobalXAccess` do
   `architecture-ratchet-rules.tsv` cobre `EquipmentManager` (só existem
   `GlobalInventoryAccess`/`GlobalGoldAccess`) — o padrão `Instance` puro aplicou sem desvio, sem
   precisar do fallback `GetComponent` usado no corte `Core|Economy`.
2. **`Core/Bootstrap/GameBootstrap.cs`** — removidos: `using CindarsHope.Equipment;`, o campo
   `[SerializeField] _equipmentManager`, a property pública `EquipmentManager`. `InitializeManagers()`
   e os métodos privados (`EquipStarterCombatLoadout`, `BuildCombatInstallContext`,
   `InitializeDeathSystem`) resolvem `CindarsHope.Equipment.EquipmentManager.Instance` (nome
   totalmente qualificado, sem `using`, para não recriar a aresta) em variável local. Confirmado por
   grep: `GameBootstrap.cs` só contém o token `Equipment` dentro de
   `CindarsHope.Equipment.EquipmentManager.Instance` totalmente qualificado ou em comentários.
3. **`Core/Bootstrap/Installers/CombatRuntimeInstallContext.cs`** — removido `using
   CindarsHope.Equipment;` e o campo `EquipmentManager EquipmentManager` do DTO (não populado mais
   por `GameBootstrap.BuildCombatInstallContext()`).
4. **`Core/Bootstrap/Installers/CombatRuntimeInstaller.cs`** — a validação que antes lia
   `context.EquipmentManager` agora resolve `CindarsHope.Equipment.EquipmentManager.Instance` (fully
   qualified) numa variável local, reusada no log de erro e no log de resumo final.
5. **`SceneManagement/FarmSceneRuntimeReferenceInstaller.cs` / `TownSceneRuntimeReferenceInstaller.cs`
   / `CaveSceneRuntimeReferenceInstaller.cs`** — a chamada a
   `saveManager.RebindOptionalRuntimeManagers(...)` troca `bootstrap.EquipmentManager` por
   `CindarsHope.Equipment.EquipmentManager.Instance` (fully-qualified; nenhum desses 3 arquivos tinha
   `using CindarsHope.Equipment` antes, então evitou introduzir aresta nova em `SceneManagement`).
6. **Consumidores de `bootstrap.EquipmentManager`/`GameBootstrap.Instance?.EquipmentManager`**
   reapontados para `EquipmentManager.Instance` (arquivos que já tinham
   `using CindarsHope.Equipment`): `World/TreeNode.cs`, `World/FishingSpot.cs`,
   `Cave/Resources/ResourceNode.cs`, `Combat/PlayerAttackController.cs`, `Farm/FarmPlot.cs`,
   `UI/Character/CharacterEquipmentPanelController.cs` (2 pontos), `UI/DebugHud.cs` (2 pontos),
   `UI/InventoryPanelController.cs`, `Editor/Validation/DebugLoadoutProvisioner.cs`. Nos arquivos
   sem esse `using` (`Cave/Traps/TrapBehaviour.cs`, `Cave/Death/DeathSystemBootstrap.cs`,
   `Cave/Death/CaveDeathEventHandler.cs`), usado `CindarsHope.Equipment.EquipmentManager.Instance`
   fully-qualified para não criar aresta nova nesses módulos. `Combat/DamageCalculator.cs` não
   precisou de mudança — o `EquipmentManager` ali é um **parâmetro** do método estático
   `CalculateDirectDamage(...)`, não uma leitura de `GameBootstrap`.
7. **`Save/SaveManager.cs`** — **não tocado**, conforme instrução (`Save|Equipment` fora de escopo
   deste corte — já era `Foundation`/DTO puro desde o v29, não expõe campo de `EquipmentManager`
   diretamente): a assinatura de `RebindOptionalRuntimeManagers(EquipmentManager equipmentManager,
   ...)` permanece igual, só o argumento passado mudou nos 4 call sites.
8. **Geradores de cena** (`CreateMvpFarmScene.cs`/`CreateMvpTownScene.cs`/`CreateMvpCaveScene.cs`)
   **não foram editados** (regen fora de escopo, per instrução da tarefa): `AddComponent<
   EquipmentManager>()` continua; a linha `SetReference(serializedBootstrap, "_equipmentManager",
   ...)` agora aponta para um campo que não existe mais em `GameBootstrap` — vira no-op silencioso
   (`SetReference` usa `SerializedObject.FindProperty` por nome de string, loga warning e retorna,
   não quebra compilação) na próxima regen (não executada nesta sessão).
9. **Ratchet de arquitetura** — baseline `tools/architecture/architecture-ratchet-baseline.tsv`
   ganhou `SingletonDeclaration	Assets/_Game/Scripts/Equipment/EquipmentManager.cs	1` (ordem
   alfabética preservada, após `Equipment/AccessoryEffectRouter.cs`).
10. Nenhum schema/save/valor/comportamento de gameplay alterado; nenhuma cena/prefab/asset `.unity`
    editado manualmente; nenhuma regeneração de cena executada; nenhum arquivo movido (diferente do
    corte `Core|Skills`, que exigiu `git mv`).

## Evidência (gates, nesta ordem)

```text
1) tools/architecture/Get-ModularizationDependencySnapshot.ps1
   MutualModulePairs: 23 -> 22
   Core|Equipment ausente do output pós-corte; nenhum par novo (lista completa dos 22 pares confere:
   Cave|Combat, Cave|Core, Cave|Enemy, Cave|SceneManagement, Combat|Core, Combat|Enemy, Combat|Player,
   Combat|Skills, Core|Inventory, Core|Player, Core|Save, Core|UI, Craft|UI, Farm|Save,
   Inventory|Player, NPC|Quests, NPC|UI, NPC|World, Player|Skills, Player|UI, Player|World, UI|World).

2) tools/unity/RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Architecture"
   1ª rodada: 7/8 PASS (falha esperada de ratchet — SingletonDeclaration novo não coberto pela
   baseline ainda). Baseline atualizada (item 9 acima). 2ª rodada: 8/8 PASS, exit 0.

3) tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1
   exit 0
   7/7 projetos (Editor, Runtime, Tests.EditMode, Gameplay, Foundation, Assembly-CSharp,
   Tests.PlayMode.Composition)
   0 warnings, 0 errors (nenhum path stale de csproj desta vez — nenhum arquivo foi movido)

4) tools/unity/RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Save"
   69/69 PASS, exit 0
   tools/unity/RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Equipment"
   0/0 (nenhum teste casa esse namespace — não existe suíte EditMode dedicada a
   `CindarsHope.Tests.EditMode.Equipment`; cobertura de equipamento existe em
   `Assets/_Game/Tests/EditMode/Player/AccessoriesTests.cs`, fora deste filtro). Exit 0 (filtro sem
   match não é falha).

5) Unity.exe -batchmode -nographics -runTests -testPlatform PlayMode
   (GameRuntimeCompositionRootPlayModeTests: GameplayScenes_LoadWithoutMissingScripts +
   RootAndTracker_RemainSingleAcrossPlayModeFrames)
   total=2 passed=2 failed=0, exit 0 (test-run xml). Log revisado: sem exceção/missing-script
   relacionado a Equipment; stack traces vistas são de rotina (SfxEventBridge, CavePlayerPath
   Confinement, CaveRuntimeBridge).
```

## Pendências

Fecha `Core|Equipment`. A Fase 2 do plano continua com `Core|Player`
(`ProgressionManager`/`StatusEffectManager`) — ver
`docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md` e
`docs/architecture/CLAUDE_MODULARIZATION_HANDOFF.md` (entrada 2026-07-08 Core/Equipment v35) e o
plano `C:\Users\Rafa\.claude\plans\replicated-juggling-axolotl.md`. Fase 3 (Inventory/Player/UI,
alto fan-out) fica para depois.

Regen de cena para remover o `AddComponent<EquipmentManager>()` + `SetReference` stray dos
geradores fica como limpeza cosmética opcional (não bloqueadora), conforme o plano. `Save|Equipment`
não é um par mútuo separado no snapshot atual (o schema de save de equipment já mora em
`CindarsHope.Foundation` desde o v29).
