# Handoff 2026-07-12 - Modularizacao e ratchet Cave

Use este arquivo junto de `CLAUDE_MODULARIZATION_HANDOFF.md` e `CLAUDE_MODULARIZATION_REMAINING_PLAN.md`.

## Estado validado antes da mudanca

```text
Branch=dev
origin/dev...dev=0 2
UnityGeneratedProjectsBuild=PASS, 7/7, 0W/0E
EditMode=FAILED, 2836/2837 PASS
Snapshot:
  UsingOnlyModuleEdges=213
  UsingOnlyMutualModulePairs=18
  RuntimeModuleEdges=248
  MutualModulePairs=32
```

Falha unica do EditMode:

```text
ArchitectureRatchetTests.RuntimeSource_DoesNotIncreaseTrackedArchitecturalDebt
ResourcesLoad: Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs has 5 occurrence(s); baseline allows 4.
```

## Decisao aplicada

Nao aumentar baseline do ratchet e nao mover o `Resources.Load` para outro arquivo so para esconder debt.

Mudancas feitas:

1. `CreateMvpCaveScene` serializa `CaveEcosystemBalanceSO` em:
   - `CaveRuntimeMaterializer._ecosystemBalance`
   - `CaveEnemySpawner._ecosystemBalance`
   - `CaveBossSpawner._ecosystemBalance`
2. `CaveRuntimeMaterializer.ResolveEcosystemBalance()` remove o novo `Resources.Load<CaveEcosystemBalanceSO>`.
3. Se uma cena antiga ainda estiver sem wiring, `CaveRuntimeMaterializer` cria uma instancia default em memoria e loga warning.
4. `GenerateCaveEcosystemBalance` volta a manter somente o asset canonico em `Assets/_Game/Data/Cave/CaveEcosystemBalance.asset`; runtime nao depende mais de `Resources/CaveEcosystemBalance`.

## Validacao executada

Executado apos a correcao:

```text
ResourcesLoad em CaveRuntimeMaterializer.cs=4 (baseline preservado)
Get-ModularizationDependencySnapshot.ps1=PASS
  RuntimeModuleEdges=248
  MutualModulePairs=32
Invoke-UnityGeneratedProjectsBuild.ps1=PASS, 7/7, 0W/0E
RunUnityEditModeTests.ps1=PASS, 2837/2837
  Results=TestResults/modularization-ratchet-fix-20260712-editmode.xml
  Log=Logs/modularization-ratchet-fix-20260712-editmode.log
```

Nao use `git add -A` nem `git add .`; ha muitas mudancas concorrentes de Cave/Enemy/assets na working tree.

## Corte adicional 2026-07-12 - Core|Economy

Objetivo: remover a direcao `Core -> Economy` restante sem alterar gameplay, save schema, cenas ou dados.

Mudancas:

1. Criado `IGameBootstrapRuntimeService` em `Core.Bootstrap`.
   - `GameBootstrap` inicializa/desliga servicos no mesmo GameObject via interface.
   - `GameBootstrap` nao referencia mais `CindarsHope.Economy.EconomyManager` nem `CindarsHope.Economy.ShopManager`.
2. `EconomyManager` implementa `IGameBootstrapRuntimeService`.
3. `ShopManager` implementa `IGameBootstrapRuntimeService` e `IShopStockRuntime`.
4. Criado `IShopStockRuntime` em `Foundation`.
   - `EconomySectionProvider` usa `IShopStockRuntime` em vez de `ShopManager`.
   - `SaveManager` resolve `IShopStockRuntime` via `DomainManagerRegistry`, com fallback para o campo serializado legado `_shopManager`.
5. `ShopManager` registra/desregistra `IShopStockRuntime` no `DomainManagerRegistry`.

Evidencia:

```text
Snapshot apos corte:
  RuntimeModuleEdges=247
  MutualModulePairs=31
  Par removido: Core|Economy

Invoke-UnityGeneratedProjectsBuild.ps1=PASS, 7/7, 0W/0E
RunUnityEditModeTests.ps1=PASS, 2837/2837
  Results=TestResults/modularization-core-economy-20260712-editmode.xml
  Log=Logs/modularization-core-economy-20260712-editmode.log
```

## Corte adicional 2026-07-12 - Core|Equipment

Objetivo: remover a direcao `Core -> Equipment` sem tentar resolver toda a fronteira Save/Combat/Player
na mesma rodada.

Mudancas:

1. Criado `IEquipmentRuntime` em `Foundation`.
2. `EquipmentManager` implementa `IEquipmentRuntime` e registra/desregistra no `DomainManagerRegistry`.
3. `GameBootstrap` usa `DomainManagerRegistry.Get<IEquipmentRuntime>()` para:
   - equipar loadout inicial de arco/flecha;
   - criar `CorpseRecoveryManager`;
   - parar de passar `EquipmentManager.Instance` em `RebindOptionalRuntimeManagers`.
4. `CorpseRecoveryManager` passou a depender de `IEquipmentRuntime`.
5. `CombatRuntimeInstaller` valida `IEquipmentRuntime` em vez de `EquipmentManager.Instance`.

Evidencia:

```text
Snapshot apos corte:
  RuntimeModuleEdges=246
  MutualModulePairs=30
  Par removido: Core|Equipment

Invoke-UnityGeneratedProjectsBuild.ps1=PASS, 7/7, 0W/0E
RunUnityEditModeTests.ps1=PASS, 2837/2837
  Results=TestResults/modularization-core-equipment-20260712-editmode.xml
  Log=Logs/modularization-core-equipment-20260712-editmode.log
```

Nao interpretar isto como resolucao completa de Equipment:

- `Save|Equipment`, `Combat|Equipment`, `Player|Equipment`/`Equipment|Player` e `Economy|Equipment`
  ainda podem existir por dependencias reais fora de Core.
- O corte fechado aqui foi apenas a direcao Core -> Equipment.

## Corte adicional 2026-07-12 - NPC|World

Objetivo: remover a direcao `World -> NPC` sem alterar comportamento de portas, agenda, dialogo,
calendario ou clima.

Mudancas:

1. Criado `INpcScheduleAvailabilityRuntime` em `Foundation`.
   - `DoorInteractable` consulta disponibilidade de loja pelo port via `DomainManagerRegistry`.
   - `NpcScheduleService` implementa o port e registra/desregistra em `Awake`/`OnDestroy`.
2. Criado `INpcDoorTraveler` em `Foundation`.
   - `NpcDweller` implementa o marker.
   - `HouseDoorInteractable` detecta `INpcDoorTraveler` em vez de `NpcDweller`.
3. `World` nao referencia mais `CindarsHope.NPC` nem `CindarsHope.NPC.Schedule`.
4. A direcao `NPC -> World` permanece, porque dialogue/aniversario ainda dependem de calendario/clima
   de World; isso e esperado. O par mutuo caiu porque a direcao oposta saiu.

Evidencia:

```text
Snapshot apos corte:
  RuntimeModuleEdges=246
  MutualModulePairs=29
  Par removido: NPC|World

Invoke-UnityGeneratedProjectsBuild.ps1=PASS, 7/7, 0W/0E
RunUnityEditModeTests.ps1=PASS, 2837/2837
  Results=TestResults/modularization-npc-world-20260712-editmode.xml
  Log=Logs/modularization-npc-world-20260712-editmode.log
```

Alerta para continuidade:

- Specs antigas em `.specs/implementados/` declaram alguns `Core|*` como feitos, mas o snapshot real
  ainda lista `Core|Inventory`, `Core|Player`, `Core|Save`, `Core|Skills` e `Core|UI`.
- Antes de continuar, use `tools/architecture/Get-ModularizationDependencySnapshot.ps1` como fonte de
  verdade, nao a narrativa dessas specs.

## Corte adicional 2026-07-12 - Farm|UI

Objetivo: remover a direcao `Farm -> UI` sem duplicar estilo visual nem alterar comportamento de menus.

Mudancas:

1. `MenuGuiStyle` foi extraido do final de `UI/InventoryPanelController.cs` para
   `Core/MenuGuiStyle.cs`.
2. Consumidores UI, Farm, Cave, Fonte e Narrative passaram a chamar `CindarsHope.Core.MenuGuiStyle.Apply()`.
3. `FarmPlotMenuController` deixou de referenciar `CindarsHope.UI`.
4. A direcao `UI -> Farm` permanece por HUD/projecoes que leem dados de farm; o par mutuo caiu porque
   a direcao oposta saiu.

Evidencia:

```text
Snapshot apos corte:
  RuntimeModuleEdges=244
  MutualModulePairs=28
  Par removido: Farm|UI

Invoke-UnityGeneratedProjectsBuild.ps1=PASS, 7/7, 0W/0E
RunUnityEditModeTests.ps1=PASS, 2837/2837
  Results=TestResults/modularization-npc-world-farm-ui-20260712-editmode.xml
  Log=Logs/modularization-npc-world-farm-ui-20260712-editmode.log
```

## Corte adicional 2026-07-12 - Craft|UI

Objetivo: remover a direcao `Craft -> UI` preservando o modo direto de abertura do modal quando a cena
ja possui referencias plugadas.

Mudancas:

1. Criado `ICraftingStationModal` em `Craft`.
2. `CraftingPoint` removeu `using CindarsHope.UI.Crafting`.
3. O campo serializado `_craftingModal` foi mantido com o mesmo nome, mas o tipo passou de
   `CraftingModal` para `MonoBehaviour`.
4. Em runtime, `CraftingPoint` abre diretamente quando `_craftingModal is ICraftingStationModal`; caso
   contrario preserva o fallback por `OpenCraftingStationRequestedEvent`.
5. `CraftingModal` implementa `ICraftingStationModal`.

Evidencia:

```text
Snapshot apos corte:
  RuntimeModuleEdges=243
  MutualModulePairs=27
  Par removido: Craft|UI

Invoke-UnityGeneratedProjectsBuild.ps1=PASS, 7/7, 0W/0E
RunUnityEditModeTests.ps1=PASS, 2837/2837
  Results=TestResults/modularization-craft-ui-20260712-editmode.xml
  Log=Logs/modularization-craft-ui-20260712-editmode.log
```

## Corte adicional 2026-07-12 - Save|UI

Objetivo: remover a direcao `Save -> UI` preservando a persistencia dos hints de onboarding.

Mudancas:

1. Criado `IOnboardingHintsRuntime` em `Foundation`.
2. `OnboardingHintService` implementa o port e registra/desregistra no `DomainManagerRegistry`.
3. Criado provider canonico `Save.Providers.OnboardingHintsSectionProvider`.
4. `SaveManager` passou a instanciar o provider de `Save.Providers`, sem FQN para
   `CindarsHope.UI.Onboarding.Save`.
5. O provider legado em `UI.Onboarding.Save` permanece no disco por compatibilidade/testes existentes,
   mas nao e mais usado pelo `SaveManager`.
6. `SaveSectionProviderTests` removeu o using legado para resolver o provider canonico de Save.

Evidencia:

```text
Snapshot apos corte:
  RuntimeModuleEdges=242
  MutualModulePairs=26
  Par removido: Save|UI

Invoke-UnityGeneratedProjectsBuild.ps1=PASS, 7/7, 0W/0E
RunUnityEditModeTests.ps1=PASS, 2837/2837
  Results=TestResults/modularization-save-ui-20260712-editmode.xml
  Log=Logs/modularization-save-ui-20260712-editmode.log
```

## Corte adicional 2026-07-12 - Economy|Equipment

Objetivo: remover a direcao `Economy -> Equipment` preservando o bonus de ouro de acessorios/reliquias
na venda.

Mudancas:

1. Criado `IGoldGainModifierRuntime` em `Foundation`.
2. `EquipmentManager` implementa o port e registra/desregistra no `DomainManagerRegistry`.
3. `EconomyManager` deixou de chamar `CindarsHope.Equipment.AccessoryEffectRouter.ApplyGoldGain`.
4. O ponto unico de aplicacao do bonus foi preservado: Economy consulta o port e aplica o retorno;
   sem runtime registrado, o valor fica neutro.

Evidencia:

```text
Snapshot apos corte:
  RuntimeModuleEdges=241
  MutualModulePairs=25
  Par removido: Economy|Equipment

Invoke-UnityGeneratedProjectsBuild.ps1=PASS, 7/7, 0W/0E
RunUnityEditModeTests.ps1=PASS, 2837/2837
  Results=TestResults/modularization-economy-equipment-20260712-editmode.xml
  Log=Logs/modularization-economy-equipment-20260712-editmode.log
```
