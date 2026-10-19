# spec_arch_core_economy_cycle_reduction_v33

> **Status:** Implementado e BUILD_VALIDATED (Fase 1 do plano de desacoplamento `static Instance`)
> **Data:** 2026-07-08
> **Escopo:** quebrar o par mútuo `Core|Economy` sem alterar gameplay, saves, cenas, prefabs, IDs ou
> balanceamento, e **sem regeneração de cena**.

## Objetivo

Quarto corte da Fase 1 do plano "Desacoplar managers de domínio do GameBootstrap (Core|\*), sem regen
destrutiva" (após `Core|Enemy` e `Core|Craft`). `ShopManager` ganha `static Instance` self-registrado
(molde `Audio/AudioManager.cs`, já usado em `Core|Enemy`/`Core|Craft`); `EconomyManager` **não** ganha
esse padrão (ver seção "Desvio do molde"); `GameBootstrap` para de segurar
`[SerializeField] _economyManager`/`_shopManager` e o fallback `EnsurePersistentShopManager`.

```text
Snapshot medido ANTES da edição: MutualModulePairs=25 (Core|Economy presente).
Snapshot medido DEPOIS de todas as edições: MutualModulePairs=24, Core|Economy ausente do output.
Nenhum par novo apareceu.
```

## Diagnóstico (Passo 0)

Grep repo-wide em `Assets/_Game/Scripts/Core/` por `CindarsHope.Economy` encontrou **uma única aresta**
`Core -> Economy`: `Core/Bootstrap/GameBootstrap.cs` (`using CindarsHope.Economy;` + campos
`[SerializeField] EconomyManager _economyManager` / `ShopManager _shopManager` + properties públicas +
método `EnsurePersistentShopManager()` que fazia `AddComponent<ShopManager>()` como fallback). Diferente
do corte `Core|Craft` (que teve uma segunda aresta em `Core/Events/CraftingEvents.cs`), aqui **não havia
segunda ocorrência** — `Core/Data/InvalidIdFallback.cs:173` só cita "ShopManager" dentro de uma string de
comentário/log, sem `using` nem tipo referenciado. A aresta reversa `Economy -> Core` (via
`CindarsHope.Core`/`CindarsHope.Core.Events` para `GameEventBus`) já existia e permanece — normal e
esperada (comunicação via event bus), não é o alvo do corte.

## Desvio do molde (achado durante a execução — importante para specs futuras)

O plano original previa aplicar `static Instance` a **ambos** `EconomyManager` e `ShopManager`, igual ao
molde `Core|Craft`. Ao rodar o gate de Architecture Ratchet após a primeira tentativa (ambos com
`Instance`), o teste `RuntimeSource_DoesNotIncreaseTrackedArchitecturalDebt` falhou:

```text
GlobalGoldAccess: Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs has 3 occurrence(s); baseline allows 0.
```

O ratchet `tools/architecture/architecture-ratchet-rules.tsv` já tinha uma regra `GlobalGoldAccess`
(`\b(?:PlayerManager|GoldManager|EconomyManager)\.(?:Instance|Active|ActiveInstance)\b`) com baseline 0
em todo o repo — um guard-rail pré-existente e intencional contra acesso estático global ao manager de
ouro. Correção: **reverti** o `static Instance` de `EconomyManager` (mantendo só em `ShopManager`, que
não está na lista da regra) e troquei os dois pontos de uso em `GameBootstrap.cs`
(`InitializeManagers()`/`ShutdownManagers()`) por `GetComponent<CindarsHope.Economy.EconomyManager>()` no
mesmo GameObject — o gerador de cena já faz `bootstrapObject.AddComponent<EconomyManager>()`, então o
componente sempre está no mesmo object do `GameBootstrap`. `GetComponent` no mesmo object é exceção
sancionada da regra "comunicação só via GameEventBus" (unity-architecture §2) e não casa com o padrão
`GlobalGoldAccess`. Baseline do ratchet: adicionada só `SingletonDeclaration` para `ShopManager.cs`, não
para `EconomyManager.cs`.

## Implementação

1. **`Assets/_Game/Scripts/Economy/ShopManager.cs`** — adicionado `public static ShopManager Instance
   { get; }` setado em novo `Awake()` (guard de duplicata idêntico ao molde `AudioManager.cs`/
   `CraftingManager.cs`) e limpo em novo `OnDestroy()`. Nenhuma lógica de loja (`TryBuyItem`/
   `TrySellItem`/sessões) alterada.
2. **`Assets/_Game/Scripts/Economy/EconomyManager.cs`** — **não** ganhou `static Instance` (ver seção
   acima); comentário registrando o motivo (ratchet `GlobalGoldAccess`) para não repetir a tentativa em
   specs futuras.
3. **`Core/Bootstrap/GameBootstrap.cs`** — removidos: `using CindarsHope.Economy;`, os campos
   `[SerializeField] _economyManager`/`_shopManager`, as properties públicas `EconomyManager`/
   `ShopManager`, e o método `EnsurePersistentShopManager()` (chamado em `Awake()`). `InitializeManagers()`
   resolve `EconomyManager` via `GetComponent<CindarsHope.Economy.EconomyManager>()` (nome totalmente
   qualificado, sem `using`) e `ShopManager` via `CindarsHope.Economy.ShopManager.Instance`;
   `ShutdownManagers()` faz o mesmo. A chamada a `_saveManager.RebindOptionalRuntimeManagers(...)` passa
   `CindarsHope.Economy.ShopManager.Instance` no lugar do campo removido (parâmetro `ShopManager` já
   existia na assinatura — `Save -> Economy` está fora de escopo desta spec, conforme instrução).
   Confirmado por grep: `GameBootstrap.cs` só contém o token `Economy` dentro de
   `CindarsHope.Economy.EconomyManager`/`CindarsHope.Economy.ShopManager` totalmente qualificados.
4. **`SceneManagement/FarmSceneRuntimeReferenceInstaller.cs` / `TownSceneRuntimeReferenceInstaller.cs` /
   `CaveSceneRuntimeReferenceInstaller.cs`** — a chamada a `saveManager.RebindOptionalRuntimeManagers(...)`
   troca `bootstrap.ShopManager` por `ShopManager.Instance` (`using CindarsHope.Economy;` adicionado nos
   dois installers que ainda não tinham).
5. **`NPC/NpcShopController.cs`** (já tinha `using CindarsHope.Economy;`) — `AdoptPersistentBootstrapReferences`
   e `RequiresPersistentBootstrapRebind` trocam `bootstrap.ShopManager` por `ShopManager.Instance`.
6. **`Editor/Validation/MvpSceneValidator.cs`** (Editor, fora do módulo `Core`, mas consumidor de
   `bootstrap.EconomyManager`) — os dois checks `bootstrap.EconomyManager == null` (FarmScene/TownScene)
   foram trocados por `FindComponent<EconomyManager>(rootObjects) == null`, mesmo padrão já usado no
   arquivo para `CraftingManager` no corte `Core|Craft`. Preserva a mesma garantia: a cena precisa ter o
   componente `EconomyManager` na hierarquia.
7. **Geradores de cena** (`CreateMvpFarmScene.cs`/`CreateMvpTownScene.cs`/`CreateMvpCaveScene.cs`) **não
   foram editados** (diff mínimo, autorizado pela tarefa): as linhas `SetReference(serializedBootstrap,
   "_economyManager", ...)` e `"_shopManager"` agora apontam para campos que não existem mais no
   `GameBootstrap` — viram no-op inofensivo na próxima regen (não executada nesta sessão).
   `AddComponent<EconomyManager>()`/`AddComponent<ShopManager>()` nos geradores continuam — os
   componentes da cena se auto-resolvem via `GetComponent` (Economy) e `Instance` (Shop).
8. **Ratchet de arquitetura** — baseline `tools/architecture/architecture-ratchet-baseline.tsv` ganhou
   `SingletonDeclaration	Assets/_Game/Scripts/Economy/ShopManager.cs	1` (ordem alfabética preservada,
   entre `Crafting/RecipeUnlockService.cs` e `Economy/WeaponInfusionRegistry.cs`). **Nenhuma entrada
   `GlobalGoldAccess` foi adicionada** — o objetivo era manter essa contagem em 0, não abrir exceção.
9. Nenhum schema/save/valor/comportamento de gameplay alterado; nenhuma cena/prefab/asset `.unity`
   editado manualmente; nenhuma regeneração de cena executada.

## Evidência (gates, nesta ordem)

```text
1) tools/architecture/Get-ModularizationDependencySnapshot.ps1
   MutualModulePairs: 25 -> 24
   Core|Economy ausente do output pós-corte; nenhum par novo.

2) tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1
   exit 0
   7/7 projetos (Editor, Runtime, Tests.EditMode, Gameplay, Foundation, Assembly-CSharp,
   Tests.PlayMode.Composition)
   0 warnings, 0 errors

3) tools/unity/RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Architecture"
   8/8 PASS, exit 0 (ratchet cobre o novo SingletonDeclaration; GlobalGoldAccess permanece 0 —
   corrigido depois de uma primeira tentativa falhar com 1/8, ver "Desvio do molde")
   tools/unity/RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Save"
   69/69 PASS, exit 0
   tools/unity/RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Economy"
   124/124 PASS, exit 0

4) Unity.exe -batchmode -nographics -runTests -testPlatform PlayMode
   (GameRuntimeCompositionRootPlayModeTests: GameplayScenes_LoadWithoutMissingScripts +
   RootAndTracker_RemainSingleAcrossPlayModeFrames)
   total=2 passed=2 failed=0, exit 0 (test-run xml). Log revisado: todas as 21 lojas de NPC da
   TownScene (`shop_yael`, `shop_thalindra`, `shop_renko`, etc.) inicializaram sessão via
   `ShopManager.Instance` normalmente; nenhuma ocorrência de "missing script"/exceção relacionada a
   Economy/Shop. Warning pré-existente e não relacionado ("[DeathSystemBootstrap] ... nao ficaram
   prontos apos 120 frames") já aparecia antes do corte (cena de teste sintética sem player completo).
```

## Pendências

Fecha `Core|Economy`. O plano de desacoplamento continua com o restante da lista "GameBootstrap
serializa `[SerializeField] Manager`" — `Core|Enemy` e `Core|Craft` e `Core|Economy` já feitos;
restam `Core|Equipment`, `Core|Inventory`, `Core|Player`, `Core|Skills`, `Core|UI` (ModalManager),
`Core|Save` (SaveManager) — ver `docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md` (linha
`Core|Economy` marcada FEITO nesta sessão) e o plano
`C:\Users\Rafa\.claude\plans\replicated-juggling-axolotl.md`. Nota para as próximas specs desse plano:
**checar a regra `GlobalGoldAccess`/regras equivalentes em `architecture-ratchet-rules.tsv` antes de
aplicar `static Instance` a qualquer manager** — nem todo manager pode receber o molde sem violar um
guard-rail já existente.

Regen de cena para remover o `AddComponent<EconomyManager>()`/`AddComponent<ShopManager>()` +
`SetReference` stray dos geradores fica como limpeza cosmética opcional (não bloqueadora), conforme o
plano.
