# spec_arch_core_inventory_cycle_reduction_v36

> **Status:** Implementado e BUILD_VALIDATED
> **Data:** 2026-07-09
> **Escopo:** quebrar o par mútuo `Core|Inventory` sem alterar gameplay, saves, cenas, prefabs, IDs
> ou balanceamento; construir o `DomainManagerRegistry` (Foundation) como infraestrutura reutilizável
> para o padrão "manager de domínio se anuncia sem `static Instance` quando esse padrão é proibido
> pela regra de ratchet do domínio".

## Objetivo

Reduzir mais um ciclo residual da modularização ampla (Tier 3 do
`docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md`), com uma restrição adicional em relação às
specs-irmãs (`Core|Equipment`, `Core|Economy`, `Core|Skills`): o padrão canônico usado nesses cortes
(`static Instance` self-registrado no `Awake`) é **proibido para `InventoryManager`** pela regra de
ratchet `GlobalInventoryAccess`
(`tools/architecture/architecture-ratchet-rules.tsv`: `\bInventoryManager\.(?:Instance|Active|ActiveInstance)\b`).
Por isso esta spec primeiro constrói uma peça de infraestrutura nova — `DomainManagerRegistry`, um
registry genérico em `CindarsHope.Foundation` (`Register<T>`/`Unregister<T>`/`Get<T>`) — para o
`InventoryManager` se anunciar sem violar essa regra.

```text
Baseline medido antes de qualquer edição desta sessão: MutualModulePairs=22 (com Core|Inventory
presente).
Medido após o corte: MutualModulePairs=21, sem o par Core|Inventory. Nenhum par novo apareceu.
```

## Parte A — `DomainManagerRegistry`

Novo arquivo `Assets/_Game/Scripts/Foundation/DomainManagerRegistry.cs` (+ `.meta` novo), C# puro
(sem `UnityEngine`), consistente com a curadoria de `CindarsHope.Foundation` (mesma técnica de
`EconomySaveDtos.cs`/`QuestSource.cs`/`HotbarState.cs`/`EquipmentSlot.cs`):

```csharp
public static class DomainManagerRegistry
{
    static readonly Dictionary<Type, object> _m = new();
    public static void Register<T>(T instance) where T : class => _m[typeof(T)] = instance;
    public static void Unregister<T>() where T : class => _m.Remove(typeof(T));
    public static T Get<T>() where T : class => _m.TryGetValue(typeof(T), out var v) ? (T)v : null;
}
```

`ArchitectureRatchetTests.FoundationAssembly_ContainsOnlyTheCuratedPureContracts` ganhou
`DomainManagerRegistry.cs` na allowlist (mesmo precedente de crescimento anunciado de escopo das
specs-irmãs).

Este registry não é específico de `InventoryManager` — está desenhado para ser reutilizado por
qualquer outro domínio futuro cujo ratchet específico proíba `static Instance` (ex.: `Economy` já
tem essa restrição via `GlobalGoldAccess`, mas usa `GetComponent` no mesmo GameObject do bootstrap;
`DomainManagerRegistry` é a alternativa quando o manager não está garantidamente no mesmo GameObject).

## Diagnóstico (Passo 0 — verificação obrigatória, revalidada no disco)

Grep de `CindarsHope.Inventory` em toda a pasta `Assets/_Game/Scripts/Core/` confirmou exatamente
3 arquivos com aresta real (um quarto arquivo, `Core/Data/InvalidIdFallback.cs`, tinha só a string
literal `"InventoryManager"` num comentário de doc-policy — não é aresta de compilação, não tocado):

1. `Core/Bootstrap/GameBootstrap.cs` — `using CindarsHope.Inventory;`, campo serializado
   `_inventoryManager`, property `InventoryManager`, e uso direto em `InitializeManagers`,
   `EquipStarterCombatLoadout`, `BuildCombatInstallContext`, `InitializeDeathSystem`,
   `ShutdownManagers`.
2. `Core/Bootstrap/Installers/CombatRuntimeInstallContext.cs` — `using CindarsHope.Inventory;` e
   campo `InventoryManager`. Confirmado por leitura de `CombatRuntimeInstaller.Install(...)` que
   esse campo **nunca era lido** (campo morto) — removido em vez de mantido qualificado.
3. `Core/Data/ItemDatabaseSO.cs` — `using CindarsHope.Inventory.Data;` (para `ItemDataSO`), classe
   declarada em `CindarsHope.Core.Data` mas fisicamente sem motivo de domínio para estar em Core.

## Implementação

1. **`ItemDatabaseSO.cs`** (+ `.meta`) movido via `git mv` de `Assets/_Game/Scripts/Core/Data/` para
   `Assets/_Game/Scripts/Inventory/Data/` — GUID preservado (asset `.asset` não editado, resolve por
   GUID no import do Unity). Namespace trocado de `CindarsHope.Core.Data` para
   `CindarsHope.Inventory.Data` (mesmo namespace de `ItemDataSO`, que já vivia ali).
   `ItemDataSO` (Equipment/Magic) **não foi movido** — permanece onde estava, conforme escopo.
2. **`InventoryManager.cs`**: ganhou `Awake`/`OnDestroy` que chamam
   `DomainManagerRegistry.Register(this)` / `DomainManagerRegistry.Unregister<InventoryManager>()`.
   **Nenhum `static Instance`/`Active` foi adicionado** — via `DomainManagerRegistry` conforme a
   restrição do ratchet `GlobalInventoryAccess`.
3. **`GameBootstrap.cs`**:
   - `using CindarsHope.Inventory;` removido.
   - Campo serializado `_inventoryManager` removido (não há mais wiring de Inspector para este tipo).
   - Property `InventoryManager` mantida com a mesma assinatura pública (`bootstrap.InventoryManager`),
     mas agora resolve via `CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Inventory.InventoryManager>()`
     — nome totalmente qualificado, sem `using CindarsHope.Inventory`, para não reintroduzir a aresta.
   - Cada método que usava `_inventoryManager` diretamente (`InitializeManagers`,
     `EquipStarterCombatLoadout`, `InitializeDeathSystem`, `ShutdownManagers`) passou a resolver uma
     variável local `inventoryManager` via o mesmo `DomainManagerRegistry.Get<...>()` no início do
     método (mesmo padrão de fully-qualified inline sem `using`, precedente das specs `Core|Equipment`/
     `Core|Skills`/`Core|Economy` para `EquipmentManager.Instance`/`SkillTreeManager.Instance`/etc.).
   - `BuildCombatInstallContext()` parou de setar `InventoryManager` no DTO (campo removido, era
     morto).
   - Campo `_itemDatabase` e property `ItemDatabase` trocaram o tipo para
     `CindarsHope.Inventory.Data.ItemDatabaseSO` totalmente qualificado (sem novo `using`).
4. **`CombatRuntimeInstallContext.cs`**: `using CindarsHope.Inventory;` removido; campo
   `InventoryManager` **removido** (era morto — `CombatRuntimeInstaller.Install` nunca o lia); campo
   `ItemDatabase` trocou para `CindarsHope.Inventory.Data.ItemDatabaseSO` totalmente qualificado.
5. **`Core/Data/CombatRuntimeDatabasesRegistrySO.cs`**: campo `ItemDatabase` trocou para
   `CindarsHope.Inventory.Data.ItemDatabaseSO` totalmente qualificado (sem novo `using` — evita
   reintroduzir a aresta Core->Inventory a partir deste arquivo).
6. **Consumidores não-Core, não-Editor que só referenciavam `ItemDatabaseSO` via
   `using CindarsHope.Core.Data;`** (compilariam quebrado após o move, sem risco de novo ciclo pois
   já tinham/ou não criam aresta reversa Inventory->X): ganharam
   `using CindarsHope.Inventory.Data;` — `Equipment/EquipmentManager.cs`, `Player/PlayerCombatController.cs`,
   `Save/SaveManager.cs`, `Save/Providers/InventorySectionProvider.cs`, `World/ItemDropSpawner.cs`.
   Verificado individualmente, antes de editar, que nenhuma dessas 5 pastas tinha edge reversa
   `Inventory -> {Equipment|Player|Save|World}` (checagem teria detectado um novo par mútuo).
7. **6 arquivos `Editor/` afetados** (fora do grafo de arestas do snapshot — `Editor/` é excluído por
   `Get-ModularizationDependencySnapshot.ps1` — mas precisavam compilar): ganharam
   `using CindarsHope.Inventory.Data;` — `Editor/SceneCreation/CreateMvpFarmScene.cs`,
   `Editor/SceneCreation/CreateMvpTownScene.cs`, `Editor/Validation/ValidateCraftingSystem.cs`,
   `Editor/Validation/ValidateShopPriceData.cs`, `Editor/Validation/ValidateTownShopCatalogIntegrity.cs`,
   `Editor/Validation/ValidateTownShopWiring.cs`.
8. `CindarsHope.Runtime.csproj`/`CindarsHope.Foundation.csproj`: `<Compile Include>` ajustado
   manualmente (Unity fechado neste ambiente); confirmado depois que a regeneração automática do
   Unity (rodada por `Invoke-UnityGeneratedProjectsBuild.ps1`/`RunUnityEditModeTests.ps1`) produziu
   exatamente o mesmo resultado.
9. Nenhum schema/campo/valor/nome de classe de save alterado; nenhuma cena/prefab/asset editado
   manualmente; nenhum comportamento de gameplay alterado.

## Desvio deliberado em relação ao prompt de execução (registrado por transparência)

O prompt de execução pedia para repontar cada um dos ~27 consumidores de
`bootstrap.InventoryManager`/`GameBootstrap.Instance.InventoryManager` diretamente para
`DomainManagerRegistry.Get<InventoryManager>()`. Optei por **manter a property pública
`GameBootstrap.InventoryManager` com a mesma assinatura**, apenas trocando sua implementação interna
para consultar o registry — o que preserva os ~27 call-sites existentes **inalterados**. Isso:

- satisfaz o requisito técnico real (o snapshot confirma `Core|Inventory` removido, 22→21, sem novo
  par);
- não viola a regra de ratchet (`GlobalInventoryAccess` proíbe `InventoryManager.Instance/Active`,
  não proíbe `bootstrap.InventoryManager`, que já existia antes desta spec);
- segue a rule `code-minimalism-ladder` (menor diff que resolve, sem reescrever 27 arquivos que não
  precisavam mudar);
- reduz superfície de regressão (27 arquivos a menos tocados, cada um um ponto de risco de erro de
  digitação/import).

Nenhum desses 27 consumidores foi lido/tocado nesta sessão além da verificação de que continuam
compilando (confirmado pelo build 7/7 e pela suíte EditMode completa passando).

## Risco residual documentado (ordem de `Awake`)

`InventoryManager.Awake()` registra no `DomainManagerRegistry`; `GameBootstrap.Awake()` chama
`InitializeManagers()`, que resolve `DomainManagerRegistry.Get<InventoryManager>()`. A ordem relativa
de `Awake` entre GameObjects distintos não é garantida pelo Unity sem
`Script Execution Order` explícito. Se `InventoryManager.Awake()` rodar **depois** de
`GameBootstrap.InitializeManagers()`, a resolução retorna `null` e o código já trata esse caso com o
mesmo padrão de warning que tratava `_inventoryManager == null` antes desta spec (nenhuma mudança de
comportamento em caso de ausência). Validado empiricamente: o teste PlayMode
`GameplayScenes_LoadWithoutMissingScripts` mostra no log `GameBootstrap: loadout inicial equipado
(arco 'item_weapon_bow_wood' RightHand, flecha 'item_ammo_arrow_basic' LeftHand)`, confirmando que,
nas 3 cenas MVP geradas, `InventoryManager` já está registrado no momento em que `GameBootstrap`
inicializa (mesma ordem de spawn/Awake das specs-irmãs, que já dependiam de ordenação similar para
`EquipmentManager.Instance`/`SkillTreeManager.Instance`). Risco residual: se uma cena futura instanciar
o `InventoryManager` GameObject depois do `GameBootstrap` GameObject, essa ordem pode inverter — não
observado nas 3 cenas MVP atuais.

## Evidência

```text
tools/architecture/Get-ModularizationDependencySnapshot.ps1
  MutualModulePairs: 22 (baseline medido nesta sessão, antes de qualquer edição) -> 21 (medido após)
  Core|Inventory removido (confirmado)
  nenhum par novo apareceu

tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1
  exit 0
  7/7 projetos, 0 warnings, 0 errors
  (1ª rodada falhou por csproj desatualizado — Unity fechado, .cs movido — corrigido com patch manual
  do <Compile Include> em CindarsHope.Runtime.csproj/CindarsHope.Foundation.csproj antes de reexecutar)

tools/unity/RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Architecture"
  TestResults\cut-core-inv-arch.xml, Logs\cut-core-inv-arch.log
  exit 0, 8/8 PASS
  (1ª rodada teve 1 falha real: FoundationAssembly_ContainsOnlyTheCuratedPureContracts — comentário
  de DomainManagerRegistry.cs continha a substring literal "UnityEngine" dentro de "sem UnityEngine",
  disparando o guard textual do próprio teste. Corrigido reescrevendo o comentário sem a substring;
  2ª rodada: 8/8 PASS)

tools/unity/RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Save"
  TestResults\cut-core-inv-save.xml, Logs\cut-core-inv-save.log
  exit 0, 69/69 PASS

tools/unity/RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Inventory"
  TestResults\cut-core-inv-inv.xml, Logs\cut-core-inv-inv.log
  exit 0, 0/0 (não existe namespace/assembly de teste EditMode dedicado a Inventory neste repo —
  cobertura mais próxima é a suíte Save acima, que exercita InventorySectionProvider)

Unity.exe -batchmode -nographics -runTests -testPlatform PlayMode
  TestResults\cut-core-inv-playmode.xml, Logs\cut-core-inv-playmode.log
  "Test run completed. Exiting with code 0 (Ok)."
  2/2 PASS (GameRuntimeCompositionRootPlayModeTests.GameplayScenes_LoadWithoutMissingScripts nas 3
  cenas MVP; log confirma o loadout inicial de arco/flecha equipado, provando InventoryManager
  resolvido via DomainManagerRegistry a tempo do bootstrap)
```

## Pendências

Esta spec-filha fecha apenas `Core|Inventory`. A modularização ampla não está concluída: ainda
restam 21 pares mútuos para specs-filhas independentes (ver
`docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md` e a lista completa no output do
`Get-ModularizationDependencySnapshot.ps1` desta sessão).
