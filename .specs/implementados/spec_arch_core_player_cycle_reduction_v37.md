# spec_arch_core_player_cycle_reduction_v37

> **Status:** Implementado e BUILD_VALIDATED
> **Data:** 2026-07-09
> **Escopo:** quebrar o par mútuo `Core|Player` sem alterar gameplay, saves, cenas, prefabs, IDs
> ou balanceamento; usar a infraestrutura `DomainManagerRegistry` (Foundation, construída na spec
> `Core|Inventory` v36) para o manager cujo ratchet proíbe `static Instance`, e o padrão
> `static Instance` self-registrado (molde Craft/Economy/Skills/Equipment) para os demais.

## Objetivo

Fechar a Fase 2 do plano `Desacoplar managers de domínio do GameBootstrap (Core|*), sem regen
destrutiva` (`docs/architecture/CLAUDE_MODULARIZATION_HANDOFF.md`), reduzindo mais um ciclo residual
da modularização ampla (Tier 3 do `docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md`).

```text
Medido antes desta sessão: MutualModulePairs=21 (com Core|Player presente).
Medido após o corte: MutualModulePairs=20, sem o par Core|Player. Nenhum par novo apareceu.
```

## Diagnóstico (Passo 0 — verificação obrigatória, revalidada no disco)

Grep de `CindarsHope.Player` em toda a pasta `Assets/_Game/Scripts/Core/` confirmou 4 arquivos com
aresta real (via `using` de topo de arquivo — o único formato que
`Get-ModularizationDependencySnapshot.ps1` conta como edge):

1. `Core/Bootstrap/GameBootstrap.cs` — 4 `using CindarsHope.Player*` (`Player`, `Player.Data`,
   `Player.Death`, `Player.Progression`). Campos serializados `_playerManager` (`PlayerManager`),
   `_progressionManager` (`PlayerProgressionManager`), `_statusEffectManager`
   (`StatusEffectManager`), e também `_staminaManager` (`StaminaManager`), `_manaManager`
   (`ManaManager`), `_hungerManager` (`HungerManager`), `_playerData` (`PlayerDataSO`) — todos tipos
   de `CindarsHope.Player`/`CindarsHope.Player.Data`. Campo privado `_corpseRecoveryManager`
   (`CorpseRecoveryManager`, de `CindarsHope.Player.Death`).
2. `Core/Bootstrap/Installers/CombatRuntimeInstallContext.cs` — `using CindarsHope.Player;`, campos
   `StaminaManager`/`ManaManager` (lidos de verdade por `CombatRuntimeInstaller.Install`, não
   mortos).
3. `Core/Events/PlayerAttributeChangedEvent.cs` — `using CindarsHope.Player.Progression;` para o
   enum `PlayerAttributeType`.
4. `Core/Events/EnvironmentalExposureEvents.cs` — `using CindarsHope.Player;` para o enum
   `HazardType`.

Os 2 enums (`HazardType`, `PlayerAttributeType`) são tipos puros sem nenhuma dependência de engine —
elegíveis para `CindarsHope.Foundation` (mesma técnica já usada em `EquipmentSlot`/`QuestSource`/
`HotbarState`). Os 3 managers (`PlayerManager`, `PlayerProgressionManager`, `StatusEffectManager`)
seguem o molde `static Instance`/`DomainManagerRegistry` das specs-irmãs `Core|Craft`/`Core|Economy`/
`Core|Skills`/`Core|Equipment`/`Core|Inventory`. `StaminaManager`/`ManaManager`/`HungerManager`/
`PlayerDataSO` **não** precisavam virar `Instance`/registry — o escopo pedido cobria só os 3 managers
citados; esses 4 tipos permaneceram como `[SerializeField]` no `GameBootstrap`, apenas com o nome
totalmente qualificado (sem `using`), quebrando a aresta sem remover a referência direta (mesmo
padrão já usado para `_itemDatabase`/`ItemDatabaseSO` no corte v36).

## Implementação

1. **`HazardType.cs`** e **`PlayerAttributeType.cs`** (+`.meta` cada) movidos via `git mv` de
   `Assets/_Game/Scripts/Player/HazardType.cs` e
   `Assets/_Game/Scripts/Player/Progression/PlayerAttributeType.cs` para
   `Assets/_Game/Scripts/Foundation/` — GUID preservado. Namespace trocado para
   `CindarsHope.Foundation`.
2. **Consumidores dos 2 enums reapontados** (~9 arquivos, nenhum novo `using CindarsHope.Player*`
   introduzido):
   - `Combat/Weapon/WeaponDataSO.cs` — `CindarsHope.Player.Progression.PlayerAttributeType` (fully
     qualified) → `CindarsHope.Foundation.PlayerAttributeType`.
   - `Combat/PlayerCombatStatsProvider.cs`/`Combat/PlayerAttackController.cs` — já tinham
     `using CindarsHope.Foundation;`; `Player.Progression.PlayerAttributeType` simplificado para
     `PlayerAttributeType` bare.
   - `Editor/Combat/ApplyWeaponMechanicalBaselines.cs` e
     `Tests/EditMode/Core/WeaponBaselineAndArmorTests.cs` — `using CindarsHope.Player.Progression;`
     trocado por `using CindarsHope.Foundation;`.
   - `Player/Progression/PlayerProgressionManager.cs` — ganhou `using CindarsHope.Foundation;`
     (referências bare ao enum, agora resolvido de Foundation em vez do próprio namespace).
   - `UI/Character/CharacterEquipmentPanelController.cs` — **não precisou de edição**: já tinha
     `using CindarsHope.Foundation;` preexistente que resolve o bare `PlayerAttributeType`; manteve
     `using CindarsHope.Player.Progression;` (ainda necessário para `PlayerProgressionManager`, tipo
     que não moveu).
   - `Core/Events/PlayerAttributeChangedEvent.cs`/`Core/Events/EnvironmentalExposureEvents.cs` —
     `using CindarsHope.Player.Progression;`/`using CindarsHope.Player;` trocados por
     `using CindarsHope.Foundation;`.
3. **`ArchitectureRatchetTests.FoundationAssembly_ContainsOnlyTheCuratedPureContracts`** allowlist
   ganhou `HazardType.cs` e `PlayerAttributeType.cs`.
4. **`PlayerManager.cs`**: ganhou `Awake`/`OnDestroy` que chamam
   `DomainManagerRegistry.Register(this)`/`DomainManagerRegistry.Unregister<PlayerManager>()`.
   **Nenhum `static Instance`/`Active` foi adicionado** — ratchet `GlobalGoldAccess`
   (`\b(?:PlayerManager|GoldManager|EconomyManager)\.(?:Instance|Active|ActiveInstance)\b`) proíbe
   esse padrão para este tipo. Ganhou `using CindarsHope.Foundation;`.
5. **`PlayerProgressionManager.cs`**: ganhou `public static PlayerProgressionManager Instance { get; private set; }`
   e o guard de duplicata em `Awake`/`OnDestroy` (molde Craft/Economy/Skills/Equipment) — nenhum
   ratchet bloqueia esse tipo.
6. **`StatusEffectManager.cs`**: mesmo padrão — `static Instance`, `Awake`/`OnDestroy` com guard de
   duplicata.
7. **`GameBootstrap.cs`**:
   - Os 4 `using CindarsHope.Player*` removidos.
   - Campos `_playerManager`, `_progressionManager`, `_statusEffectManager` **removidos**.
   - Properties públicas `PlayerManager`/`PlayerProgressionManager`/`StatusEffectManager`
     **mantidas com a mesma assinatura**, mas agora resolvem via
     `DomainManagerRegistry.Get<CindarsHope.Player.PlayerManager>()`/
     `CindarsHope.Player.Progression.PlayerProgressionManager.Instance`/
     `CindarsHope.Player.StatusEffectManager.Instance` (fully-qualified, sem `using`) — os
     consumidores existentes de `bootstrap.PlayerManager`/`bootstrap.PlayerProgressionManager`/
     `bootstrap.StatusEffectManager` não precisaram ser tocados (mesmo padrão do v36 para
     `InventoryManager`).
   - Cada método interno que usava os campos diretamente (`InitializeManagers`,
     `EquipStarterCombatLoadout` — via `inventoryManager`/`equipmentManager`, não tocava
     `_playerManager`, `InitializeDeathSystem`, `ShutdownManagers`) passou a resolver uma variável
     local (`playerManager`, `progressionManager`, `statusEffectManager`) pelo mesmo padrão no início
     do método.
   - Campos `_staminaManager`, `_manaManager`, `_hungerManager`, `_playerData` **mantidos como
     `[SerializeField]`** — só o tipo trocou para totalmente qualificado
     (`CindarsHope.Player.StaminaManager`, `CindarsHope.Player.ManaManager`,
     `CindarsHope.Player.HungerManager`, `CindarsHope.Player.Data.PlayerDataSO`); as properties
     públicas correspondentes idem. `GetComponent<ManaManager>()`/`AddComponent<ManaManager>()` em
     `EnsureCombatRuntimeReferences()` também fully-qualified.
   - Campo privado `_corpseRecoveryManager` e a construção `new CorpseRecoveryManager(...)` em
     `InitializeDeathSystem()` fully-qualified para `CindarsHope.Player.Death.CorpseRecoveryManager`.
8. **`CombatRuntimeInstallContext.cs`**: `using CindarsHope.Player;` removido; campos
   `StaminaManager`/`ManaManager` trocaram para `CindarsHope.Player.StaminaManager`/
   `CindarsHope.Player.ManaManager` fully-qualified (mantidos — `CombatRuntimeInstaller.Install` os
   lê de verdade, não eram campo morto como o `InventoryManager` no v36).
9. `CindarsHope.Runtime.csproj`/`CindarsHope.Foundation.csproj`: `<Compile Include>` ajustado
   manualmente (Unity fechado neste ambiente); confirmado depois que a regeneração automática do
   Unity (rodada por `RunUnityEditModeTests.ps1`) produziu exatamente o mesmo resultado (arquivos
   movidos para `CindarsHope.Foundation.csproj`).
10. Nenhum schema/campo/valor/nome de classe de save alterado; nenhuma cena/prefab/asset editado
    manualmente; nenhum comportamento de gameplay alterado.

## Erro corrigido durante a execução (mesmo padrão do v36/v29/v24/v22)

1ª rodada do filtro `CindarsHope.Tests.EditMode.Architecture` teve 1 falha real:
`FoundationAssembly_ContainsOnlyTheCuratedPureContracts` — os comentários novos em
`HazardType.cs`/`PlayerAttributeType.cs` continham a substring literal `"UnityEngine"` (dentro de
"sem UnityEngine"), disparando o guard textual do próprio teste (`Foundation source must not depend
on UnityEngine`). Corrigido reescrevendo o comentário para "sem dependencia de engine" (sem a
substring); 2ª rodada: 8/8 PASS.

## Risco residual documentado (ordem de `Awake`, mesmo padrão do v36)

`PlayerManager.Awake()` registra no `DomainManagerRegistry`; `GameBootstrap.Awake()` chama
`InitializeManagers()`, que resolve `DomainManagerRegistry.Get<PlayerManager>()`. A ordem relativa de
`Awake` entre GameObjects distintos não é garantida pelo Unity sem `Script Execution Order` explícito.
Se `PlayerManager.Awake()` rodar **depois** de `GameBootstrap.InitializeManagers()`, a resolução
retorna `null` e o código já trata esse caso com o mesmo padrão de warning que tratava
`_playerManager == null` antes desta spec. Validado empiricamente: o teste PlayMode
`GameplayScenes_LoadWithoutMissingScripts` mostra no log `GameBootstrap: loadout inicial equipado
(arco 'item_weapon_bow_wood' RightHand, flecha 'item_ammo_arrow_basic' LeftHand)` e **não** mostra o
novo warning `GameBootstrap: PlayerManager.Awake ainda nao registrou no DomainManagerRegistry`,
confirmando que `PlayerManager` já está registrado no momento em que `GameBootstrap` inicializa (mesma
ordem de spawn/Awake das specs-irmãs). Risco residual: se uma cena futura instanciar o `PlayerManager`
GameObject depois do `GameBootstrap` GameObject, essa ordem pode inverter — não observado nas 3 cenas
MVP atuais.

O log do PlayMode também mostra `[DeathSystemBootstrap] GameBootstrap/PlayerManager/
CorpseRecoveryManager nao ficaram prontos apos 120 frames. Sistema de morte NAO inicializado`. Isso
**não é uma regressão desta spec** — `GameRuntimeCompositionRootPlayModeTests.cs:101-103` já tem
`LogAssert.Expect(LogType.Error, new Regex("^\\[DeathSystemBootstrap\\] GameBootstrap/PlayerManager/
CorpseRecoveryManager nao ficaram prontos"))` com o comentário explícito de que essa mensagem é
esperada — o `DeathSystemBootstrap` é `DontDestroyOnLoad` e esgota o bind na cena vazia que o Test
Framework restaura ao final do teste (sem `GameBootstrap`), não numa transição real do jogo.

## Evidência

```text
tools/architecture/Get-ModularizationDependencySnapshot.ps1
  MutualModulePairs: 21 (medido nesta sessão, antes de qualquer edição) -> 20 (medido após)
  Core|Player removido (confirmado)
  nenhum par novo apareceu

tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1
  exit 0
  7/7 projetos, 0 warnings, 0 errors
  (1ª rodada falhou por csproj desatualizado — Unity fechado, 2 .cs movidos — corrigido com patch
  manual do <Compile Include> em CindarsHope.Runtime.csproj/CindarsHope.Foundation.csproj antes de
  reexecutar)

tools/unity/RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Architecture"
  TestResults\cut-core-player-arch2.xml, Logs\cut-core-player-arch2.log
  exit 0, 8/8 PASS
  (1ª rodada teve 1 falha real: FoundationAssembly_ContainsOnlyTheCuratedPureContracts — comentários
  dos 2 enums novos continham a substring literal "UnityEngine" dentro de "sem UnityEngine",
  disparando o guard textual do próprio teste. Corrigido reescrevendo os comentários; 2ª rodada: 8/8
  PASS)

tools/unity/RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Save"
  TestResults\cut-core-player-save.xml, Logs\cut-core-player-save.log
  exit 0, 69/69 PASS

tools/unity/RunUnityEditModeTests.ps1 -TestFilter "CindarsHope.Tests.EditMode.Player"
  TestResults\cut-core-player-pl.xml, Logs\cut-core-player-pl.log
  exit 0, 192/192 PASS

Unity.exe -batchmode -nographics -runTests -testPlatform PlayMode
  TestResults\cut-core-player-playmode.xml, Logs\cut-core-player-playmode.log
  2/2 PASS (GameRuntimeCompositionRootPlayModeTests: RootAndTracker_RemainSingleAcrossPlayModeFrames +
  GameplayScenes_LoadWithoutMissingScripts nas 3 cenas MVP); único LogError do log é o esperado,
  consumido por LogAssert.Expect no próprio teste (ver seção de risco residual acima); log confirma o
  loadout inicial de arco/flecha equipado, provando PlayerManager resolvido via DomainManagerRegistry
  a tempo do bootstrap.
```

## Pendências

Esta spec-filha fecha `Core|Player` e, com ele, todos os pares `Core|*` de fan-out médio listados na
Fase 2 do plano (`Core|Enemy`, `Core|Craft`, `Core|Economy`, `Core|Skills`, `Core|Equipment`,
`Core|Inventory`, `Core|Player`). A modularização ampla **não está concluída**: ainda restam 20 pares
mútuos, incluindo os de fan-out alto da Fase 3 (`Inventory|Player`, `Player|Skills`, `Player|UI`,
`Player|World`, `UI|World`, `Craft|UI`, `Core|Save`, `Core|UI`, etc. — ver
`docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md` e a lista completa no output do
`Get-ModularizationDependencySnapshot.ps1` desta sessão).
