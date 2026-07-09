# Prompt de Continuação para Claude — Rework Modular

## 2026-07-09 — Core/Save cycle reduction v39 (último `Core|*`, shim fully-qualified sem port novo)

- Spec implementada: `.specs/implementados/spec_arch_core_save_cycle_reduction_v39.md`.
- Último corte do plano `Desacoplar managers de domínio do GameBootstrap (Core\|*)` — fecha a lista
  inteira de pares `Core\|*` iniciada em `Core\|Enemy` v31.
- Passo 0 (verificação obrigatória): grep de `CindarsHope.Save` em `Assets/_Game/Scripts/Core/`
  confirmou exatamente 2 arestas — `Core/Bootstrap/GameBootstrap.cs` (`using CindarsHope.Save;` +
  `[SerializeField] SaveManager _saveManager` + property pública, 9 consumidores fora de Core) e
  `Core/GameTimeManager.cs` (`using CindarsHope.Save;` + `RestoreFromSaveData(GameTimeSaveData
  saveData)`, só lia `CurrentPhase`/`PhaseElapsedSeconds`).
- Mudança:
  - `GameTimeManager.cs`: `using CindarsHope.Save;` removido; `RestoreFromSaveData(GameTimeSaveData)`
    virou `RestorePhaseState(int currentPhase, float phaseElapsedSeconds)` — mesmo corpo
    (`Mathf.Clamp`/`Mathf.Max`), só a origem dos 2 valores muda de "campos do DTO" para "parâmetros
    primitivos".
  - `Save/Providers/GameTimeSectionProvider.cs` (já em `CindarsHope.Save.Providers`, já importa
    `CindarsHope.Core`/`CindarsHope.Core.Time`): `Restore()` continua fazendo o cast para
    `GameTimeSaveData`, mas chama `_gameTimeManager.RestorePhaseState(data.CurrentPhase,
    data.PhaseElapsedSeconds)` em vez de passar o DTO inteiro. Schema de save (`GameTimeSaveData` em
    `Save/SaveData.cs`) **não foi movido nem alterado**.
  - `GameBootstrap.cs`: `using CindarsHope.Save;` removido; campo `_saveManager` e property pública
    `SaveManager` **mantidos** (mesma assinatura), só com o tipo totalmente qualificado
    `CindarsHope.Save.SaveManager` — mesma técnica de `_itemDatabase`/`ItemDatabaseSO` (v36),
    `_staminaManager`/`StaminaManager` (v37) e `_modalManager`/`ModalManager` (v38). Os 9
    consumidores de `bootstrap.SaveManager`/`GameBootstrap.Instance.SaveManager` não precisaram ser
    tocados.
  - Nenhum port novo (`ISaveService`) criado — a ideia original do prompt de execução foi trocada
    pelo padrão fully-qualified shim já provado, por ser estritamente menor (rule
    `code-minimalism-ladder`: o scanner só conta `using`s de topo de arquivo, então fully-qualify
    sozinho já resolve o edge sem abstração nova).
- Sem erro corrigido durante a execução — 1ª rodada de todos os gates passou verde (diferente dos
  cortes v36-v38, que tiveram 1 falha real de `FoundationAssembly_ContainsOnlyTheCuratedPureContracts`
  por comentário com a substring "UnityEngine" — esta spec não criou arquivo novo em `Foundation/`).
- Risco residual documentado: (1) sem teste automatizado dedicado ao comportamento de
  `RestorePhaseState` (mesmo estado de antes — nunca existiu suíte EditMode específica para
  `GameTimeManager`); (2) sem teste de round-trip de save isolado para a seção `game_time` (schema
  inalterado, mesmo risco de antes).
- Gates: snapshot `MutualModulePairs` 19→18 (`Core\|Save` some, nenhum par novo); build 7/7 exit 0
  0W/0E (1ª rodada já verde); EditMode Architecture 8/8, **Save 69/69 (crítico)**, Core 81/81; PlayMode
  composição 2/2 PASS, log confirma "Test run completed. Exiting with code 0 (Ok)", sem
  missing-script/exceção relacionado a Save/GameTime.
- Pendência: este era o **último** par `Core\|*` do plano — a lista `Core\|*` está fechada. Fase 3
  segue com os pares de alto fan-out não-`Core` (`Craft\|UI`, `NPC\|UI`, `Player\|UI`, `UI\|World`,
  `Cave\|*`, `Combat\|*`, `Inventory\|Player`, `NPC\|Quests`, `NPC\|World`, `Player\|Skills`,
  `Player\|World`, `Farm\|Save`), fora do escopo desta spec.

## 2026-07-09 — Core/UI cycle reduction v38 (Fase 3/Tier 2, port `IModalStateProvider`)

- Spec implementada: `.specs/implementados/spec_arch_core_ui_cycle_reduction_v38.md`.
- Primeiro corte da Fase 3 (alto fan-out) do plano `Desacoplar managers de domínio do GameBootstrap
  (Core\|*)`, após `Core\|Player` v37.
- Passo 0 (verificação obrigatória): grep de `CindarsHope.UI` em `Assets/_Game/Scripts/Core/`
  confirmou exatamente 2 arestas — `Core/GameTimeManager.cs` (`using CindarsHope.UI.Modal;` +
  `[SerializeField] ModalManager _modalManager` lido em `Update()` p/ pausar o tick quando há modal
  ativo) e `Core/Bootstrap/GameBootstrap.cs` (`using CindarsHope.UI.Modal;` + `[SerializeField]
  _modalManager` + property pública, ~40 consumidores). `HotbarState.SlotCount` citado no prompt
  como candidato **não gerava aresta** — já morava em `CindarsHope.Foundation` desde `Save\|UI` v26,
  referenciado fully-qualified em `GameBootstrap.cs:223`; confirmado por leitura antes de editar.
- Mudança:
  - Novo port puro `Foundation/IModalStateProvider.cs` (`bool HasActiveModal { get; }`).
    `ArchitectureRatchetTests` allowlist ganhou `IModalStateProvider.cs`.
  - `ModalManager.cs` ganhou `IModalStateProvider` na assinatura + `Awake`/`OnDestroy` que chamam
    `DomainManagerRegistry.Register<IModalStateProvider>(this)`/`Unregister<IModalStateProvider>()`
    (molde Core\|Inventory/Core\|Player — registry genérico, pedido explicitamente pelo prompt de
    execução em vez de `static Instance`).
  - `GameTimeManager.cs`: `using CindarsHope.UI.Modal;` e o campo `_modalManager` removidos;
    `Update()` resolve `DomainManagerRegistry.Get<IModalStateProvider>()` fully-qualified a cada
    frame — mesma condição de early-return, só a origem do `HasActiveModal` muda.
  - `GameBootstrap.cs`: `using CindarsHope.UI.Modal;` removido; campo `_modalManager` e property
    `ModalManager` **mantidos** (mesma assinatura), só com o tipo totalmente qualificado
    `CindarsHope.UI.Modal.ModalManager` — os ~40 consumidores de `bootstrap.ModalManager` não
    precisaram ser tocados (mesmo padrão do `_itemDatabase`/`ItemDatabaseSO` no v36).
  - `Editor/SceneCreation/PlayerNeedsDataInitializer.cs`: removida a linha
    `SetReference(gameTimeManager, "_modalManager", modalManager)` — campo não existe mais em
    `GameTimeManager`; teria lançado `NullReferenceException` (`FindProperty` retornando `null`) na
    próxima regen se mantida. Assinatura pública inalterada (parâmetro `modalManager` fica sem uso
    interno, sem erro de compilação).
  - `Editor/Validation/MvpSceneValidator.cs` (`ValidateSpec09Bootstrap`): removida a checagem
    `gameTime.FindProperty("_modalManager")` pelo mesmo motivo — teria quebrado `Validar Projeto`.
  - `CindarsHope.Foundation.csproj`: `<Compile Include>` do novo arquivo ajustado manualmente
    (Unity fechado); confirmado depois que a regeneração automática do Unity produziu o mesmo
    resultado.
- Erro corrigido durante a execução (mesmo padrão do v37/v36/v29/v24/v22): 1ª rodada de
  `ArchitectureRatchetTests.FoundationAssembly_ContainsOnlyTheCuratedPureContracts` falhou — o
  comentário novo em `IModalStateProvider.cs` continha a substring literal "UnityEngine" (dentro de
  "sem UnityEngine"); reescrito para "port C# puro, independente de engine"; 2ª rodada 8/8 PASS.
- Risco residual documentado: (1) sem dependência crítica de ordem de `Awake` — o consumo é em
  `Update()` (todo frame), não só na inicialização, então um `ModalManager.Awake()` atrasado só
  atrasa 1 frame a pausa por modal, sem exceção; (2) sem teste automatizado dedicado ao
  comportamento "tick pausa com modal ativo" (não existia antes desta spec também — não há suíte
  EditMode para `GameTimeManager` no repo).
- Gates: snapshot `MutualModulePairs` 20→19 (`Core\|UI` some, nenhum par novo); build 7/7 exit 0
  0W/0E (1ª rodada falhou por csproj desatualizado — corrigido com patch manual do
  `<Compile Include>` em `CindarsHope.Foundation.csproj`); EditMode Architecture 8/8 (após o fix do
  comentário), Save 69/69, UI 366/366; PlayMode composição 2/2 PASS, log sem
  missing-script/exception relacionado a Modal.
- Pendência: Fase 3 continua com `Craft\|UI`, `NPC\|UI`, `Player\|UI`, `UI\|World` (alto fan-out),
  fora do escopo desta spec.

## 2026-07-09 — Core/Player cycle reduction v37 (Fase 2, misto `DomainManagerRegistry` + `static Instance`)

- Spec implementada: `.specs/implementados/spec_arch_core_player_cycle_reduction_v37.md`.
- Último corte declarado da Fase 2 do plano `Desacoplar managers de domínio do GameBootstrap
  (Core\|*)`, após `Core\|Inventory` v36.
- Passo 0 (verificação obrigatória): grep de `CindarsHope.Player` em `Assets/_Game/Scripts/Core/`
  confirmou 4 arestas — `GameBootstrap.cs` (`_playerManager`/`_progressionManager`/
  `_statusEffectManager` + properties + 4 `using CindarsHope.Player*`, e mais `_staminaManager`/
  `_manaManager`/`_hungerManager`/`_playerData`, também tipos `CindarsHope.Player*` mas mantidos como
  campo — ver abaixo), `CombatRuntimeInstallContext.cs` (campos `StaminaManager`/`ManaManager`),
  `Core/Events/PlayerAttributeChangedEvent.cs` (`Player.Progression.PlayerAttributeType`),
  `Core/Events/EnvironmentalExposureEvents.cs` (`Player.HazardType`).
- Mudança:
  - `HazardType.cs` e `PlayerAttributeType.cs` (2 enums puros, +`.meta`) movidos via `git mv` para
    `CindarsHope.Foundation` (GUID preservado). ~9 consumidores reapontados (`WeaponDataSO.cs`
    fully-qualified; `PlayerCombatStatsProvider.cs`/`PlayerAttackController.cs` simplificados para
    bare `PlayerAttributeType` — já tinham `using CindarsHope.Foundation;`;
    `ApplyWeaponMechanicalBaselines.cs`/`WeaponBaselineAndArmorTests.cs` trocaram o `using`;
    `PlayerProgressionManager.cs` ganhou `using CindarsHope.Foundation;`;
    `CharacterEquipmentPanelController.cs` não precisou de edição — já tinha `using
    CindarsHope.Foundation;` preexistente cobrindo o bare `PlayerAttributeType`).
    `ArchitectureRatchetTests` allowlist ganhou `HazardType.cs`/`PlayerAttributeType.cs`.
  - `PlayerManager.cs` ganhou `Awake`/`OnDestroy` que chamam
    `DomainManagerRegistry.Register(this)`/`Unregister<PlayerManager>()` — **sem** `static Instance`
    (ratchet `GlobalGoldAccess` proíbe `PlayerManager.(Instance|Active|ActiveInstance)`).
  - `PlayerProgressionManager.cs`/`StatusEffectManager.cs` ganharam `static Instance`
    self-registrado (Awake/OnDestroy, guard de duplicata, molde Craft/Economy/Skills/Equipment) —
    nenhum ratchet bloqueia esses dois tipos.
  - `GameBootstrap.cs`: os 4 `using CindarsHope.Player*` removidos. Campos `_playerManager`/
    `_progressionManager`/`_statusEffectManager` **removidos** (propriedades viraram shims que
    resolvem `DomainManagerRegistry.Get<PlayerManager>()`/`PlayerProgressionManager.Instance`/
    `StatusEffectManager.Instance` fully-qualified); cada método interno que os usava resolve uma
    variável local pelo mesmo padrão. Campos `_staminaManager`/`_manaManager`/`_hungerManager`/
    `_playerData` **mantidos como `[SerializeField]`** (não pedidos para remoção pelo prompt), só com
    o tipo totalmente qualificado (`CindarsHope.Player.StaminaManager` etc.) em vez de bare — mesmo
    padrão do `_itemDatabase`/`ItemDatabaseSO` no corte v36. `GetComponent<ManaManager>()`/
    `AddComponent<ManaManager>()` em `EnsureCombatRuntimeReferences` também fully-qualified.
  - `CombatRuntimeInstallContext.cs`: `using CindarsHope.Player;` removido; campos
    `StaminaManager`/`ManaManager` fully-qualified (mantidos, não eram mortos —
    `CombatRuntimeInstaller.Install` os lê).
- Erro corrigido durante a execução (mesmo padrão do v36/v29/v24/v22): 1ª rodada de
  `ArchitectureRatchetTests.FoundationAssembly_ContainsOnlyTheCuratedPureContracts` falhou — os
  comentários novos em `HazardType.cs`/`PlayerAttributeType.cs` continham a substring literal
  "UnityEngine" (dentro de "sem UnityEngine"); reescritos para "sem dependencia de engine"; 2ª
  rodada 8/8 PASS.
- Risco residual documentado (ordem de `Awake`, mesmo padrão do v36): `PlayerManager.Awake()`
  registra no `DomainManagerRegistry` antes de `GameBootstrap.Awake()` chamar
  `InitializeManagers()` nas 3 cenas MVP atuais — validado empiricamente pelo log do PlayMode
  (`GameBootstrap: loadout inicial equipado`, que depende de `playerManager != null` resolvido a
  tempo) e pela ausência do novo warning `GameBootstrap: PlayerManager.Awake ainda nao registrou`.
  `[DeathSystemBootstrap] ... nao ficaram prontos apos 120 frames` no log do PlayMode é **esperado**
  e pré-existente — `GameRuntimeCompositionRootPlayModeTests.cs:101-103` já tem
  `LogAssert.Expect(...)` para essa mensagem exata (mensagem da cena de teardown vazia do test
  framework, não uma transição real do jogo).
- Gates: snapshot `MutualModulePairs` 21→20 (`Core\|Player` some, nenhum par novo); build 7/7 exit 0
  0W/0E (1ª rodada falhou por csproj desatualizado — Unity fechado, 2 `.cs` movidos — corrigido com
  patch manual do `<Compile Include>` em `CindarsHope.Runtime.csproj`/`CindarsHope.Foundation.csproj`
  antes de reexecutar); EditMode Architecture 8/8 (após o fix do comentário), Save 69/69, Player
  192/192; PlayMode composição 2/2 PASS, único `LogError` do log é o esperado/consumido pelo
  `LogAssert.Expect` do próprio teste.
- Pendência: Fase 2 do plano fecha com este corte (Player era o último `Core\|*` de fan-out médio
  listado); Fase 3 (Inventory/Player/UI, alto fan-out — pares `Inventory\|Player`, `Player\|Skills`,
  `Player\|UI`, `Player\|World`, `UI\|World`, `Craft\|UI`, `Core\|Save`, `Core\|UI` etc.) segue
  pendente, fora do escopo desta spec.

## 2026-07-09 — Core/Inventory cycle reduction v36 (Fase 2, padrão `DomainManagerRegistry` — não `static Instance`)

- Spec implementada: `.specs/implementados/spec_arch_core_inventory_cycle_reduction_v36.md`.
- Corte da Fase 2 do plano `Desacoplar managers de domínio do GameBootstrap (Core\|*)`, após
  `Core\|Equipment` v35. Diferente das specs-irmãs anteriores: `InventoryManager` **não pode** usar o
  padrão `static Instance`/`Active` — a regra de ratchet `GlobalInventoryAccess`
  (`tools/architecture/architecture-ratchet-rules.tsv`) proíbe explicitamente
  `\bInventoryManager\.(?:Instance|Active|ActiveInstance)\b`.
- Infra nova construída primeiro: `Assets/_Game/Scripts/Foundation/DomainManagerRegistry.cs` — registry
  genérico C# puro (`Register<T>`/`Unregister<T>`/`Get<T>` sobre `Dictionary<Type, object>`), reutilizável
  por qualquer domínio futuro cujo ratchet proíba `static Instance`. Allowlist de
  `ArchitectureRatchetTests.FoundationAssembly_ContainsOnlyTheCuratedPureContracts` atualizada.
- Passo 0 (verificação obrigatória): grep de `CindarsHope.Inventory` em `Assets/_Game/Scripts/Core/`
  confirmou 3 arestas — `GameBootstrap.cs` (`_inventoryManager` + property + `using`),
  `CombatRuntimeInstallContext.cs` (campo `InventoryManager`, confirmado morto — nunca lido por
  `CombatRuntimeInstaller.Install`) e `Core/Data/ItemDatabaseSO.cs` (`using CindarsHope.Inventory.Data`
  para `ItemDataSO`).
- Mudança:
  - `InventoryManager.cs` ganhou `Awake`/`OnDestroy` que chamam
    `DomainManagerRegistry.Register(this)`/`Unregister<InventoryManager>()` — **sem** `static Instance`.
  - `ItemDatabaseSO.cs` (+`.meta`) movido via `git mv` p/ `Assets/_Game/Scripts/Inventory/Data/`,
    namespace `CindarsHope.Core.Data` → `CindarsHope.Inventory.Data` (GUID preservado; `ItemDataSO`
    não movido).
  - `GameBootstrap.cs` perdeu `[SerializeField] _inventoryManager` + `using CindarsHope.Inventory`;
    a property pública `InventoryManager` foi **mantida** (mesma assinatura,
    `bootstrap.InventoryManager` inalterado nos ~27 consumidores) mas passou a resolver via
    `CindarsHope.Foundation.DomainManagerRegistry.Get<CindarsHope.Inventory.InventoryManager>()`
    fully-qualified. Cada método interno que usava o campo direto
    (`InitializeManagers`/`EquipStarterCombatLoadout`/`InitializeDeathSystem`/`ShutdownManagers`)
    resolve uma variável local pelo mesmo registry. `_itemDatabase`/`ItemDatabase` trocaram para
    `CindarsHope.Inventory.Data.ItemDatabaseSO` fully-qualified.
  - `CombatRuntimeInstallContext.cs` perdeu o campo morto `InventoryManager` + `using`; campo
    `ItemDatabase` fully-qualified.
  - `Core/Data/CombatRuntimeDatabasesRegistrySO.cs`: campo `ItemDatabase` fully-qualified (sem
    `using` novo).
  - 5 arquivos não-Core/não-Editor (`Equipment/EquipmentManager.cs`, `Player/PlayerCombatController.cs`,
    `Save/SaveManager.cs`, `Save/Providers/InventorySectionProvider.cs`, `World/ItemDropSpawner.cs`)
    ganharam `using CindarsHope.Inventory.Data;` — verificado individualmente que nenhum cria aresta
    reversa nova.
  - 6 arquivos `Editor/` ganharam o mesmo `using` (fora do grafo de arestas, só para compilar).
  - Desvio deliberado do prompt de execução: os ~27 consumidores de `bootstrap.InventoryManager` NÃO
    foram reapontados individualmente para `DomainManagerRegistry.Get<...>()` — a property do
    `GameBootstrap` absorve essa indireção, reduzindo a superfície tocada em ~27 arquivos sem abrir mão
    do requisito técnico (rule `code-minimalism-ladder`).
- Gates: snapshot `MutualModulePairs` 22→21 (`Core\|Inventory` some, nenhum par novo); build 7/7 exit 0
  0W/0E (1ª rodada falhou por csproj desatualizado com Unity fechado — corrigido com patch manual do
  `<Compile Include>`, depois confirmado idêntico pela regeneração automática do Unity); EditMode
  Architecture 8/8 (1ª rodada teve 1 falha real — comentário novo continha a substring literal
  "UnityEngine", corrigido), Save 69/69, filtro `CindarsHope.Tests.EditMode.Inventory` deu 0/0 (não
  existe essa suíte dedicada); PlayMode composição 2/2 PASS, log confirma loadout inicial de
  arco/flecha equipado (prova de que `InventoryManager` resolve via registry a tempo do bootstrap).
- Pendência: Fase 2 continua só com `Core\|Player` (ProgressionManager/StatusEffectManager); Fase 3
  (Player/UI, alto fan-out) para depois.

## 2026-07-08 — Core/Equipment cycle reduction v35 (Fase 2, padrão `static Instance`)

- Spec implementada: `.specs/implementados/spec_arch_core_equipment_cycle_reduction_v35.md`.
- Segundo corte da Fase 2 do plano `Desacoplar managers de domínio do GameBootstrap (Core\|*), sem
  regen destrutiva` (após `Core\|Skills` v34).
- Passo 0 (verificação obrigatória): grep de `CindarsHope.Equipment` em `Assets/_Game/Scripts/Core/`
  confirmou exatamente 2 arestas — `GameBootstrap.cs` (`_equipmentManager` + property + `using`) e
  `Core/Bootstrap/Installers/CombatRuntimeInstallContext.cs` (campo `EquipmentManager`). O enum
  `EquipmentSlot` já morava em `CindarsHope.Foundation` desde o corte `Equipment\|Save` v29 — não
  havia mais nenhuma aresta de tipo puro a mover, só a referência de manager concreto.
- Mudança:
  - `Equipment/EquipmentManager.cs` ganhou `static Instance` (Awake/OnDestroy, guard de duplicata,
    molde Craft/Economy/Skills). Nenhum ratchet `GlobalXAccess` bloqueia esse padrão para Equipment
    (só existe `GlobalGoldAccess`/`GlobalInventoryAccess`), então não precisou do fallback
    `GetComponent` usado no corte Economy.
  - `GameBootstrap.cs` perdeu `[SerializeField] _equipmentManager` + a property pública +
    `using CindarsHope.Equipment`; resolve `CindarsHope.Equipment.EquipmentManager.Instance`
    (fully-qualified, sem `using`) em `EquipStarterCombatLoadout`, `BuildCombatInstallContext`,
    `InitializeDeathSystem` e na chamada a `RebindOptionalRuntimeManagers`.
  - `CombatRuntimeInstallContext.cs` perdeu o campo `EquipmentManager` + `using`;
    `CombatRuntimeInstaller.cs` resolve via `EquipmentManager.Instance` fully-qualified em vez de
    `context.EquipmentManager`.
  - ~20 consumidores de `bootstrap.EquipmentManager`/`GameBootstrap.Instance?.EquipmentManager`
    reapontados para `EquipmentManager.Instance` (arquivos já com `using CindarsHope.Equipment`) ou
    `CindarsHope.Equipment.EquipmentManager.Instance` fully-qualified (`TrapBehaviour`,
    `DeathSystemBootstrap`, `CaveDeathEventHandler`, os 3 `*SceneRuntimeReferenceInstaller`) para não
    introduzir aresta nova nesses módulos.
  - `SaveManager.cs` **não foi tocado** — a assinatura de `RebindOptionalRuntimeManagers(...)`
    permanece igual; só o argumento passado nos 4 call sites mudou.
  - Geradores de cena (`CreateMvp*Scene.cs`) **não foram editados** (regen fora de escopo, per
    plano); `SetReference(..., "_equipmentManager", ...)` vira no-op silencioso na próxima regen.
  - Baseline de ratchet: `SingletonDeclaration Assets/_Game/Scripts/Equipment/EquipmentManager.cs 1`
    adicionada a `tools/architecture/architecture-ratchet-baseline.tsv`.
- Gates: snapshot `MutualModulePairs` 23→22 (`Core\|Equipment` some, nenhum par novo); ratchet PASS
  (após atualizar baseline); build 7/7 exit 0 0W/0E (nenhum arquivo movido, sem stale csproj path);
  EditMode filtrado (Architecture 8/8, Save 69/69; filtro `CindarsHope.Tests.EditMode.Equipment` deu
  0/0 — não existe essa suíte dedicada); PlayMode composição 2/2 PASS, log sem exceção/missing-script
  relacionado a Equipment.
- Pendência: Fase 2 continua só com `Core\|Player` (ProgressionManager/StatusEffectManager); Fase 3
  (Inventory/Player/UI, alto fan-out) para depois.

## 2026-07-08 — Core/Skills cycle reduction v34 (Fase 2, padrão `static Instance`)

- Spec implementada: `.specs/implementados/spec_arch_core_skills_cycle_reduction_v34.md`.
- Primeiro corte da Fase 2 (médio fan-out) do plano
  `Desacoplar managers de domínio do GameBootstrap (Core\|*), sem regen destrutiva`.
- Passo 0 (verificação obrigatória): grep de `CindarsHope.Skills` em `Assets/_Game/Scripts/Core/`
  confirmou exatamente 2 arestas: `GameBootstrap.cs` (`_skillTreeManager` + `_skillActionDatabase`
  + `using CindarsHope.Skills`) e `Core/Data/SkillActionDatabaseSO.cs` (`class SkillActionDatabaseSO
  : DataRegistrySO<SkillActionSO>`, tipo `SkillActionSO` é de Skills).
- Mudança:
  - `SkillActionDatabaseSO.cs` (+`.meta`) movido via `git mv` de `Core/Data/` para `Skills/`, GUID
    preservado. Namespace `CindarsHope.Core.Data` → `CindarsHope.Skills`. `SkillActionExecutor.cs`
    perdeu o `using CindarsHope.Core.Data;` (agora redundante — mesmo namespace).
  - `SkillTreeManager.cs` ganhou `static Instance` (Awake/OnDestroy, guard de duplicata, molde
    Craft/Economy/Enemy). Nenhum ratchet `GlobalXAccess` bloqueia esse padrão para Skills (só existe
    `GlobalGoldAccess`/`GlobalInventoryAccess`), então não precisou do fallback `GetComponent` usado
    no Economy.
  - `GameBootstrap.cs` perdeu `[SerializeField] _skillTreeManager` + `_skillActionDatabase` + as 2
    properties + `using CindarsHope.Skills`; resolve via `CindarsHope.Skills.SkillTreeManager.Instance`
    (fully-qualified, sem `using`, para não recriar a aresta) no `RebindProgressionManager`/
    `RebindOptionalRuntimeManagers`.
  - ~15 consumidores de `bootstrap.SkillTreeManager`/`GameBootstrap.Instance?.SkillTreeManager`
    reapontados para `SkillTreeManager.Instance` (arquivos já em `CindarsHope.Skills`/já com
    `using CindarsHope.Skills` existente) ou `CindarsHope.Skills.SkillTreeManager.Instance`
    fully-qualified (arquivos sem o using — `PlayerVitalsApplier`, `PlayerDamageReceiver`,
    `FonteInteractable`, `SpellItemUseController`, os 3 `*SceneRuntimeReferenceInstaller`) para não
    introduzir aresta nova nesses módulos.
  - `SaveManager.cs` **não foi tocado** — mantém seu próprio `[SerializeField] _skillTreeManager` e o
    parâmetro `skillTreeManager` em `RebindOptionalRuntimeManagers(...)` (par `Save\|Skills` fora de
    escopo deste corte). Os 3 `*SceneRuntimeReferenceInstaller.cs` passam
    `CindarsHope.Skills.SkillTreeManager.Instance` nesse parâmetro em vez de `bootstrap.SkillTreeManager`.
  - `Editor/Validation/ValidateSkillTreeRuntimeBinding.cs`: o check de
    `bootstrap.SkillTreeManager != null` (serialized field) foi trocado por um check gateado em
    `Application.isPlaying` de `SkillTreeManager.Instance != null` — em Edit Mode o `Instance` é null
    (Awake só roda em Play Mode), então virou SKIP fora de Play Mode em vez de FAIL falso.
  - Geradores de cena (`CreateMvp*Scene.cs`) **não foram editados** (regen fora de escopo, per plano);
    `SetReference(..., "_skillTreeManager", ...)` vira no-op silencioso na próxima regen.
  - Csproj: `CindarsHope.Runtime.csproj` tinha o path stale
    `Core\Data\SkillActionDatabaseSO.cs` (Unity fechado, não regenerou); patched para
    `Skills\SkillActionDatabaseSO.cs` (não commitado — Unity regenera na próxima abertura).
  - Baseline de ratchet: `SingletonDeclaration Assets/_Game/Scripts/Skills/SkillTreeManager.cs 1`
    adicionada a `tools/architecture/architecture-ratchet-baseline.tsv`.
- Gates: snapshot `MutualModulePairs` 24→23 (`Core\|Skills` some, nenhum par novo); ratchet PASS;
  build 7/7 exit 0 0W/0E; EditMode filtrado (Architecture 8/8, Save 69/69, Skills 28/28); PlayMode
  composição 2/2 PASS, log sem exceção/missing-script relacionado a SkillTree.
- Pendência: Fase 2 continua com `Core\|Player` (ProgressionManager/StatusEffectManager) e
  `Core\|Equipment` (EquipmentManager); Fase 3 (Inventory/Player/UI, alto fan-out) para depois.

## 2026-07-08 — Core/Enemy cycle reduction v31 (piloto Fase 1, padrão `static Instance`)

- Spec implementada: `.specs/implementados/spec_arch_core_enemy_cycle_reduction_v31.md`.
- Piloto do plano `Desacoplar managers de domínio do GameBootstrap (Core\|*), sem regen destrutiva`
  (`C:\Users\Rafa\.claude\plans\replicated-juggling-axolotl.md`). Objetivo: provar que managers já
  `AddComponent`ados na cena podem perder o `[SerializeField]` do `GameBootstrap` via
  self-registro `static Instance` (molde `Audio/AudioManager.cs`), **sem regenerar as 3 cenas**.
- Mudança: `BestiaryManager.cs` ganhou `static Instance` (Awake/OnDestroy, guard de duplicata,
  molde AudioManager). `GameBootstrap.cs` perdeu `[SerializeField] _bestiaryManager` + property +
  `EnsurePersistentBestiaryManager()` + `using CindarsHope.Enemy`. Consumidor
  (`NpcServiceRuntime.ResolveBestiary()`) e `SaveManager.RebindOptionalRuntimeManagers(...)` passaram
  a resolver via `BestiaryManager.Instance`. Geradores de cena (`CreateMvp*Scene.cs`) **não foram
  editados** (diff mínimo) — a linha `SetReference(..., "_bestiaryManager", ...)` do gerador de Farm
  vira no-op na próxima regen (não executada).
- Achado de processo: o ratchet de arquitetura (`ArchitectureRatchetTests.
  RuntimeSource_DoesNotIncreaseTrackedArchitecturalDebt`) rejeitou o novo `SingletonDeclaration` em
  `BestiaryManager.cs` até o baseline (`tools/architecture/architecture-ratchet-baseline.tsv`) ganhar
  a linha explícita permitindo 1 ocorrência — mesmo padrão já permitido para `AudioManager.cs`. Ao
  repetir este padrão em managers futuros (Fase 1: Economy/Shop, Craft), espere a mesma exigência de
  atualizar o baseline.
- Achado de ferramenta: `tools/unity/RunUnityEditModeTests.ps1 -TestFilter "NS1,NS2"` (múltiplos
  namespaces separados por vírgula) **não funciona como OR** — retornou 0 testes rodados nos dois
  casos testados. Rode um namespace por invocação.
- Gates: snapshot `MutualModulePairs` 27→26 (Core\|Enemy some, nenhum par novo); build 7/7 exit 0
  0W/0E; EditMode filtrado (Save 69/69, Architecture 8/8 pós-fix do baseline, World 210/210 — cobre
  `BestiaryKnowledgeTests`, já que não existe namespace `CindarsHope.Tests.EditMode.Enemy`); PlayMode
  composição 2/2 PASS (`GameplayScenes_LoadWithoutMissingScripts`), sem erro de missing
  script/BestiaryManager no log.
- Pendência: Fase 1 continua com `Core\|Economy` (`EconomyManager`/`ShopManager`) e `Core\|Craft`
  (`CraftingManager`); Fases 2/3 (Skills/Player/Equipment/Inventory/UI) para depois.

## 2026-07-08 — Equipment/Save cycle reduction v29 (Tier 3, large-spec por causa do enum)

- Spec implementada: `.specs/implementados/spec_arch_equipment_save_cycle_reduction_v29.md`.
- Objetivo: quebrar o par mútuo `Equipment|Save` (Tier 3 do
  `docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md`), sem alterar gameplay, saves, cenas,
  prefabs, IDs ou balanceamento.
- Passo 0 (verificação obrigatória): grep de `using CindarsHope.Save` em
  `Assets/_Game/Scripts/Equipment/` confirmou 2 arquivos (`EquipmentManager.cs`,
  `EquipmentDurabilityTracker.cs`). O DTO `EquipmentSlotSaveData` tem campo do tipo enum
  `EquipmentSlot` — mover o DTO para Foundation exigiu mover o enum também (senão Foundation
  voltaria a referenciar Equipment). Grep repo-wide com word boundary (`\bEquipmentSlot\b`, para
  evitar falso positivo com `EquipmentSlotSaveData`/`EquipmentSlotChangedEvent`/
  `EquipmentSlotViewModel`) mapeou 28 arquivos de produção + 2 de teste usando o enum de verdade.
  `EquipmentSaveData` também referenciava `EquipmentUpgradeSaveData` (puro, precisou mover junto) e
  `WeaponInfusionSaveData` (já em Foundation, do corte Economy|Save); `EquipmentDurabilitySaveData`
  referenciava `DurabilityEntryData` (puro, moveu junto).
- Mudança:
  - `EquipmentSlot.cs` (+`.meta`) movido via `git mv` de `Equipment/` para `Foundation/`, GUID
    preservado. Namespace `CindarsHope.Equipment` → `CindarsHope.Foundation`.
  - Novo arquivo `Foundation/SaveSchema/EquipmentSaveDtos.cs` com 5 DTOs movidos de
    `CindarsHope.Save`: `EquipmentSaveData`, `EquipmentSlotSaveData`, `EquipmentUpgradeSaveData`,
    `EquipmentDurabilitySaveData`, `DurabilityEntryData` — mesmo nome de classe/campo, sem
    migration.
  - `SaveData.cs`: as 5 classes removidas; `using CindarsHope.Equipment;` removido.
  - `EquipmentManager.cs`/`EquipmentDurabilityTracker.cs`: `using CindarsHope.Save;` →
    `using CindarsHope.Foundation;`.
  - 3 arquivos internos de `Equipment/` (`AccessoryEffectRouter.cs`, `AccessoryCatalog.cs`,
    `RepairKitManager.cs`) ganharam `using CindarsHope.Foundation;` (antes viam `EquipmentSlot` sem
    `using`, por estarem no mesmo namespace).
  - 13 arquivos que usam `EquipmentSlot` **e** outro tipo de Equipment (`EquipmentManager`/
    `EquipmentDataSO`) mantiveram `using CindarsHope.Equipment;` e ganharam
    `using CindarsHope.Foundation;` (GameBootstrap, InventoryPanelController,
    CharacterEquipmentPanelController, PlayerCombatStatsProvider, SpellCastService,
    PlayerAttackController, BowArrowAttackService, DebugHud, PlayerCombatController,
    DerivedStatsCalculator, CombatActionContext, CorpseRecoveryManager, AccessoriesTests).
  - 8 arquivos que só usavam `EquipmentSlot` trocaram `using CindarsHope.Equipment;` por
    `using CindarsHope.Foundation;` (InventoryManager, PlayerAttackController.Attacks,
    EquippedItemResolver, ItemDataSO, EquipmentSlotChangedEvent, PlayerAttackCoreTests).
  - 3 arquivos com referência totalmente qualificada (`CindarsHope.Equipment.EquipmentSlot`, sem
    `using`, resolvida antes via namespace irmão) tiveram o texto trocado para
    `CindarsHope.Foundation.EquipmentSlot` (PlayerAttackCore.cs, GenerateCanonicalItemCatalog.cs,
    OnboardingHintService.cs).
  - `EquipmentSectionProvider.cs`/`EquipmentDurabilitySectionProvider.cs`/`SaveV2ToV3Migration.cs`
    ganharam `using CindarsHope.Foundation;` (resolviam os DTOs antes via namespace pai Save).
  - `Crafting/EquipmentUpgradeRegistry.cs` trocou `using CindarsHope.Save;` por
    `using CindarsHope.Foundation;` (único consumidor de `EquipmentUpgradeSaveData` fora de
    Equipment/Save — efeito colateral: também derruba a única aresta `Crafting -> Save`).
  - `ArchitectureRatchetTests` allowlist ganhou `EquipmentSlot.cs` e `EquipmentSaveDtos.cs`.
  - `CindarsHope.Foundation.csproj`/`CindarsHope.Runtime.csproj`: `<Compile Include>` ajustado
    manualmente (Unity fechado); a regeneração automática do Unity (via
    `RunUnityEditModeTests.ps1`) confirmou o mesmo resultado depois.
- Erro corrigido durante a execução: 1ª rodada de EditMode teve 1 falha real — os comentários novos
  continham a substring literal "UnityEngine" (dentro de "sem UnityEngine"), disparando o guard de
  `FoundationAssembly_ContainsOnlyTheCuratedPureContracts`. Corrigido para "sem refs Unity" (mesmo
  padrão dos comentários `arch:` anteriores); 2ª rodada 2747/2747 PASS.
- Gates:
  - `tools/architecture/Get-ModularizationDependencySnapshot.ps1`: baseline informado
    `MutualModulePairs=29` (não medi eu mesmo antes de editar); medido depois `MutualModulePairs=27`.
    `Equipment|Save` confirmadamente ausente. `Crafting|Save` também some do snapshot pós-corte
    (efeito colateral do item acima, não isolado antes) — reportado com honestidade em vez de
    reivindicado como "exatamente -1".
  - `tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1`: exit 0, 7/7 projetos, 0 warnings, 0 erros.
  - `tools/unity/RunUnityEditModeTests.ps1 -ResultsPath TestResults\cut-equipment-save-editmode2.xml -LogFile Logs\cut-equipment-save2.log`:
    exit 0, 2747/2747 PASS, 0 failed.
- Pendência anotada na spec: `Core|Equipment` deve ser reavaliado à luz desta mudança (o enum não
  vive mais em `CindarsHope.Equipment`; `GameBootstrap.cs` ainda importa `CindarsHope.Equipment` por
  causa de `EquipmentManager`, então o par pode continuar mútuo por outro motivo).
- Ainda não declarar modularização ampla concluída: restam pares mútuos para specs-filhas.
- Sem push. Working tree segue com mudanças concorrentes de arte/animação/ProjectSettings/tools
  fora do escopo desta spec (não tocadas/incluídas).

## 2026-07-08 — Cave/Save cycle reduction v28 (microcut Tier 3)

- Spec implementada: `.specs/implementados/spec_arch_cave_save_cycle_reduction_v28.md`.
- Objetivo: quebrar o par mútuo `Cave|Save` (Tier 3 do
  `docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md`), sem alterar gameplay, saves, cenas,
  prefabs, IDs ou balanceamento.
- Passo 0 (verificação obrigatória): grep de `using CindarsHope.Save` em toda a pasta
  `Assets/_Game/Scripts/Cave/` confirmou que `Cave/Runtime/CaveRunManager.cs` era o único arquivo de
  Cave referenciando Save (via `CaveSaveData`). `CaveSaveData.cs` foi lido por inteiro: DTO puro
  (`int`/`string`/`List<>` + `UnityEngine.Vector2`/`Vector2Int`), já importava
  `CindarsHope.Cave.Runtime` e não tinha nenhuma outra dependência de domínio — elegível para mover
  para Cave sem reintroduzir o ciclo. Precedente vivo: `CaveRunSaveData` já mora em
  `Cave/Runtime/`.
- Mudança:
  - `CaveSaveData.cs` (+`.meta`) movido via `git mv` de `Assets/_Game/Scripts/Save/` para
    `Assets/_Game/Scripts/Cave/Runtime/`, GUID preservado. Namespace trocado de `CindarsHope.Save`
    para `CindarsHope.Cave.Runtime`; `using CindarsHope.Cave.Runtime;` removido do topo do próprio
    arquivo por ficar redundante/auto-referente após a mudança de namespace.
  - `CaveRunManager.cs`: `using CindarsHope.Save;` removido (sem outro uso do namespace).
  - `Save/SaveData.cs`: ganhou `using CindarsHope.Cave.Runtime;` (novo, para resolver o campo
    `public CaveSaveData Cave;`).
  - `SaveManager.cs`, `SaveManager.Migration.cs`, `Migrations/SaveV3ToV4Migration.cs` e
    `Providers/CaveSectionProvider.cs`: nenhuma edição necessária — já resolviam `CaveSaveData` via
    `using CindarsHope.Cave.Runtime;` preexistente (usado para outros tipos de Cave nesses mesmos
    arquivos).
  - `Editor/Validation/ValidateSpec14BCaveSnapshotReplay.cs`: path do check textual atualizado
    (`Assets/_Game/Scripts/Save/CaveSaveData.cs` → `Assets/_Game/Scripts/Cave/Runtime/CaveSaveData.cs`).
  - `CindarsHope.Runtime.csproj`: `<Compile Include>` do arquivo movido atualizado manualmente
    (Unity não estava aberto para regenerar o csproj neste ambiente; csproj é gitignored, não
    commitado).
  - Nenhum schema/campo/valor/nome de classe alterado; nenhuma cena/prefab/asset editado
    manualmente.
- Achado não bloqueante (fora de escopo, não corrigido): `Assets/_Game/Tests/EditMode/Cave/
  CaveSaveBackCompatTests.cs` já tinha `using CindarsHope.Save;` órfão (só citava `CaveSaveData` em
  comentário) antes deste corte — não gera edge no scanner (é `Tests`, não `Cave`) e não foi tocado
  para manter o diff mínimo.
- Gates:
  - `tools/architecture/Get-ModularizationDependencySnapshot.ps1`: `MutualModulePairs=30 -> 29`,
    `Cave|Save` removido, nenhum par novo apareceu.
  - `tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1`: exit 0, 7/7 projetos, 0 warnings, 0 erros.
  - `tools/unity/RunUnityEditModeTests.ps1 -ResultsPath TestResults\cut-cave-save-editmode.xml -LogFile Logs\cut-cave-save.log`:
    exit 0, 2747/2747 PASS, 0 failed.
- Ainda não declarar modularização ampla concluída: restam 29 pares mútuos para specs-filhas.
- Sem push. Working tree segue com mudanças concorrentes de arte/animação/ProjectSettings/tools
  fora do escopo desta spec (não tocadas/incluídas).

## 2026-07-08 — Core/Locations cycle reduction v27 (microcut Tier 2)

- Spec implementada: `.specs/implementados/spec_arch_core_locations_cycle_reduction_v27.md`.
- Objetivo: quebrar o par mútuo `Core|Locations` (Tier 2 do
  `docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md`), sem alterar gameplay, saves, cenas,
  prefabs, IDs ou balanceamento.
- Passo 0 (verificação obrigatória): grep de `using CindarsHope.Locations`/`CindarsHope.Locations.`
  em toda a pasta `Assets/_Game/Scripts/Core/` confirmou que `Core/Bootstrap/GameBootstrap.cs` era
  o único arquivo de Core referenciando Locations (`[SerializeField] private AnyaFountain
  _anyaFountain;` + propriedade). `AnyaFountain.cs` foi lido e confirmado como MonoBehaviour trivial
  — só referenciava `CindarsHope.Core.Respawn` (a interface `IAnyaFountainRespawnPoint` que já
  implementa) e `UnityEngine`, sem nenhum tipo de `CindarsHope.Locations` — elegível para mover para
  Core sem reintroduzir o ciclo.
- Mudança:
  - `AnyaFountain.cs` (+`.meta`) movido via `git mv` de `Assets/_Game/Scripts/Locations/` para
    `Assets/_Game/Scripts/Core/Respawn/`, GUID preservado. Namespace trocado de
    `CindarsHope.Locations` para `CindarsHope.Core.Respawn` (mesmo namespace da interface que já
    implementa; `using CindarsHope.Core.Respawn;` removido do topo do arquivo por ficar
    redundante/auto-referente).
  - `GameBootstrap.cs`: `using CindarsHope.Locations;` removido; `using CindarsHope.Core.Respawn;`
    adicionado (Core→Core interno, sem novo edge cruzado).
  - `AnyaFountainInteractable.cs` (permanece em `CindarsHope.Locations`, referenciava `AnyaFountain`
    sem qualificador por estar no mesmo namespace antes do corte): ganhou
    `using CindarsHope.Core.Respawn;` (Locations→Core, direção já existente e correta).
  - `Editor/SceneCreation/CreateMvpFarmScene.cs`: referência fully-qualified
    `CindarsHope.Locations.AnyaFountain` atualizada para `CindarsHope.Core.Respawn.AnyaFountain`.
  - `CindarsHope.Runtime.csproj`: `<Compile Include>` do arquivo movido atualizado manualmente
    (Unity não estava aberto para regenerar o csproj neste ambiente; csproj é gitignored, não
    commitado).
  - Nenhum schema/campo/valor/nome de classe alterado; nenhuma cena/prefab/asset editado
    manualmente.
- Achado não bloqueante (fora de escopo, não corrigido): `Assets/_Game/Scripts/Cave/Death/
  DeathSystemBootstrap.cs` tem `using CindarsHope.Locations;` morto (não referencia nenhum tipo de
  Locations) — não gera edge `Core|Locations` porque é `Cave`, não `Core`; `Cave/**` está fora do
  escopo declarado e das proibições desta execução, não tocado.
- Risco residual (não bloqueante, sem edição de YAML): `Assets/_Game/Scenes/FarmScene.unity:14589`
  ainda tem `m_EditorClassIdentifier: Assembly-CSharp::CindarsHope.Locations.AnyaFountain` (campo
  cosmético do editor Unity, não usado para resolver a referência serializada — resolução real é
  via `m_Script: {fileID: 11500000, guid: 30374dbf686cfec41a14bbdf3ada1eac, type: 3}`, GUID
  preservado pelo `git mv` do `.meta`). Não editado manualmente (regra `unity-assets`); Unity deve
  regravar esse campo automaticamente na próxima vez que a cena for salva pelo editor. Build 7/7 e
  EditMode 2747/2747 confirmam que a referência de GUID resolve corretamente.
- Gates:
  - `tools/architecture/Get-ModularizationDependencySnapshot.ps1`: `MutualModulePairs=31 -> 30`,
    `Core|Locations` removido, nenhum par novo apareceu.
  - `tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1`: exit 0, 7/7 projetos, 0 warnings, 0 erros.
  - `tools/unity/RunUnityEditModeTests.ps1 -ResultsPath TestResults\cut-core-locations-editmode.xml -LogFile Logs\cut-core-locations.log`:
    exit 0, 2747/2747 PASS, 0 failed.
- Ainda não declarar modularização ampla concluída: restam 30 pares mútuos para specs-filhas.
- Sem push. Working tree segue com mudanças concorrentes de arte/animação/ProjectSettings/tools
  fora do escopo desta spec (não tocadas/incluídas).

## 2026-07-08 — Inventory/Save cycle reduction v25 (microcut Tier 2)

- Spec implementada: `.specs/implementados/spec_arch_inventory_save_cycle_reduction_v25.md`.
- Objetivo: quebrar o par mútuo `Inventory|Save` (Tier 2 do
  `docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md`), sem alterar gameplay, saves, schema,
  cenas, prefabs ou IDs.
- Passo 0 (verificação obrigatória): grep de `using CindarsHope.Save` em toda a pasta
  `Assets/_Game/Scripts/Inventory/` confirmou que `InventoryManager.cs` era o único arquivo de
  Inventory referenciando Save. Grep repo-wide dos 3 tipos (`InventorySaveData`,
  `InventorySlotSaveData`, `InventoryItemSaveData`) confirmou 8 consumidores de código
  (`SaveData.cs`, `SaveManager.cs`, `SaveManager.Migration.cs`,
  `Save/Providers/InventorySectionProvider.cs`, `Save/DeathSaveData.cs`,
  `Save/Migrations/InventorySlotsV1ToV2Migration.cs`, `InventoryManager.cs` e
  `Tests/EditMode/Save/Editor/SaveProviderRegistryTests.cs`) — a maioria já tinha
  `using CindarsHope.Inventory;` preexistente (usado por outros tipos Inventory).
- Mudança:
  - Novo arquivo `Assets/_Game/Scripts/Inventory/InventorySaveData.cs` (namespace
    `CindarsHope.Inventory`) com os 3 DTOs movidos de `CindarsHope.Save`, mesmo nome de
    classe/campo — JsonUtility serializa por nome de campo, sem migration. Precedente vivo:
    `NpcManagerSaveData`/`NpcSaveData` (`spec_arch_npc_save_cycle_reduction_v23`).
  - `SaveData.cs` removeu as 3 classes e ganhou `using CindarsHope.Inventory;`.
  - `InventoryManager.cs` removeu `using CindarsHope.Save;` (não havia outro uso do namespace).
  - `Save/DeathSaveData.cs` e `Tests/EditMode/Save/Editor/SaveProviderRegistryTests.cs` ganharam
    `using CindarsHope.Inventory;` (referenciavam os DTOs implicitamente antes do corte).
  - `SaveManager.cs`, `SaveManager.Migration.cs`, `InventorySectionProvider.cs` e
    `InventorySlotsV1ToV2Migration.cs` não precisaram de edição (já resolviam os tipos via
    `using CindarsHope.Inventory;` preexistente).
  - `CindarsHope.Runtime.csproj`: `<Compile Include>` do novo arquivo adicionado manualmente
    (Unity não estava aberto para regenerar o csproj neste ambiente; não commitado).
- Gates:
  - `tools/architecture/Get-ModularizationDependencySnapshot.ps1`: `MutualModulePairs=33 -> 32`,
    `Inventory|Save` removido, nenhum par novo apareceu.
  - `tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1`: exit 0, 7/7 projetos, 0 warnings, 0
    erros.
  - `tools/unity/RunUnityEditModeTests.ps1 -ResultsPath TestResults\cut-inventory-save-editmode.xml -LogFile Logs\cut-inventory-save.log`:
    exit 0, 2747/2747 PASS, 0 failed.
- Ainda não declarar modularização ampla concluída: restam 32 pares mútuos para specs-filhas.
- Sem push. Working tree segue com mudanças concorrentes de arte/animação/ProjectSettings/tools
  fora do escopo desta spec (não tocadas/incluídas).

## 2026-07-08 — Quests/Save cycle reduction v24 (microcut Tier 1)

- Spec implementada: `.specs/implementados/spec_arch_quests_save_cycle_reduction_v24.md`.
- Objetivo: quebrar o par mútuo `Quests|Save` (microcut Tier 1 do
  `docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md`), sem alterar gameplay, saves, schema,
  cenas, prefabs ou IDs.
- Passo 0 (verificação obrigatória) revisou a estratégia planejada: o "padrão vivo" de providers se
  auto-registrando no `SaveProviderRegistry` **não existe** — todos os ~30 providers são construídos
  diretamente dentro de `SaveManager.Initialize()`, que já importa quase todos os domínios do jogo
  (composition-root paralelo ao `GameBootstrap`). Em vez de inventar um mecanismo de auto-registro
  novo só para Quests, foi usada a técnica já vigente em `NPC|Save`/`Economy|Save`: nome totalmente
  qualificado no ponto único de construção, sem novo `using` de topo de arquivo (o scanner de
  dependência só conta `using`s de topo).
- Mudança:
  - Enum `QuestSource` movido de `CindarsHope.Quests` para `CindarsHope.Foundation` (novo arquivo
    `Assets/_Game/Scripts/Foundation/QuestSource.cs`); `Quests/QuestSource.cs` manteve
    `QuestSourceMapper`/`QuestLogTab` e ganhou `using CindarsHope.Foundation;`; 11 arquivos de
    produção + 6 de teste em `Quests/**` ganharam o mesmo `using`.
  - `QuestSectionProvider` relocado de `Save/Providers/` para `Quests/Save/` (namespace
    `CindarsHope.Quests.Save`, `.cs.meta` movido junto via `git mv`).
  - `SaveManager.cs:192` passou a `new CindarsHope.Quests.Save.QuestSectionProvider();` (nome
    totalmente qualificado, sem novo `using CindarsHope.Quests`).
  - `ArchitectureRatchetTests.FoundationAssembly_ContainsOnlyTheCuratedPureContracts` allowlist
    ganhou `QuestSource.cs`.
  - `CindarsHope.Foundation.csproj`/`CindarsHope.Runtime.csproj`: `<Compile Include>` ajustado
    manualmente (Unity fechado neste ambiente).
- Gates:
  - `tools/architecture/Get-ModularizationDependencySnapshot.ps1`: `MutualModulePairs=34 -> 33`,
    `Quests|Save` removido, nenhum par novo apareceu.
  - `tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1`: exit 0, 7/7 projetos, 0 warnings, 0 erros.
  - `tools/unity/RunUnityEditModeTests.ps1 -ResultsPath TestResults\cut-quests-save-editmode.xml -LogFile Logs\cut-quests-save.log`:
    exit 0, 2747/2747 PASS, 0 failed (1ª rodada acusou 1 falha real — comentário no novo arquivo
    continha a substring "UnityEngine", disparando o próprio guard de allowlist; corrigido e
    re-executado do zero).
- Ainda não declarar modularização ampla concluída: restam 33 pares mútuos para specs-filhas.
- Sem push. Working tree segue com mudanças concorrentes de arte/animação/ProjectSettings/tools
  fora do escopo desta spec (não tocadas/incluídas).

## 2026-07-08 — NPC/Save cycle reduction v23 (microcut Tier 1)

- Spec implementada: `.specs/implementados/spec_arch_npc_save_cycle_reduction_v23.md`.
- Objetivo: quebrar o par mútuo `NPC|Save` (microcut Tier 1 do
  `docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md`), sem alterar gameplay, saves, schema,
  cenas, prefabs ou IDs.
- Passo 0 (verificação obrigatória): grep de `using CindarsHope.Save`/`NpcManagerSaveData`/
  `NpcSaveData` em toda a pasta `Assets/_Game/Scripts/NPC/` confirmou que `NpcManager.cs` era o
  único arquivo de NPC referenciando Save. Os 2 DTOs têm `string`/`bool`/`List<>` e
  `UnityEngine.Vector2` (campo `Position`) — elegíveis para o namespace de domínio
  `CindarsHope.NPC` (não para `CindarsHope.Foundation`, que exige `noEngineReferences: true`).
- Mudança:
  - Novo arquivo `Assets/_Game/Scripts/NPC/NpcManagerSaveData.cs` com os 2 DTOs movidos de
    `CindarsHope.Save` para `CindarsHope.NPC` (mesmo nome de classe/campo — JsonUtility serializa
    por nome de campo, sem migration). Precedente vivo: `FriendshipSaveData`/`NpcServicesSaveData`
    já vivem em `CindarsHope.NPC.*`.
  - `SaveData.cs` removeu as 2 classes; o campo `GameSaveData.Npcs` passou a ser qualificado como
    `CindarsHope.NPC.NpcManagerSaveData` (sem novo `using`).
  - `NpcManager.cs` removeu `using CindarsHope.Save;` (não havia outro uso do namespace).
  - `SaveManager.cs`, `SaveManager.Migration.cs` e `Save/Providers/NpcsSectionProvider.cs` **não**
    precisaram de edição — já tinham `using CindarsHope.NPC;` preexistente (usado por outros tipos
    NPC), então continuaram compilando sem mudança.
  - `CindarsHope.Runtime.csproj`: `<Compile Include>` do novo arquivo adicionado manualmente
    (Unity não estava aberto para regenerar o csproj neste ambiente).
- Gates:
  - `tools/architecture/Get-ModularizationDependencySnapshot.ps1`: `MutualModulePairs=35 -> 34`,
    `NPC|Save` removido, nenhum par novo apareceu.
  - `tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1`: exit 0, 7/7 projetos, 0 warnings, 0 erros.
  - `tools/unity/RunUnityEditModeTests.ps1 -ResultsPath TestResults\cut-npc-save-editmode.xml -LogFile Logs\cut-npc-save.log`:
    exit 0, 2747/2747 PASS, 0 failed.
- Risco residual não bloqueante: `ValidateWave25TownNpcSchedulesDialogue.cs` (validator arquivado,
  `CindarsHope/Archive/...`, fora dos 3 comandos canônicos) tem um check textual
  `ContainsText(SaveData.cs, "HasMet")` que hoje falharia — documentado na spec, não corrigido por
  estar fora do escopo declarado do par `NPC|Save` e por não rodar em nenhum gate desta spec.
- Ainda não declarar modularização ampla concluída: restam 34 pares mútuos para specs-filhas.
- Sem push. Working tree segue com mudanças concorrentes de arte/animação/ProjectSettings/tools
  fora do escopo desta spec (não tocadas/incluídas).

## 2026-07-07 — Economy/Save cycle reduction v22 (microcut Tier 1)

- Spec implementada: `.specs/implementados/spec_arch_economy_save_cycle_reduction_v22.md`.
- Objetivo: quebrar o par mútuo `Economy|Save` (microcut Tier 1 do
  `docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md`), sem alterar gameplay, saves, schema,
  cenas, prefabs ou IDs.
- Passo 0 (verificação obrigatória): grep de `using CindarsHope.Save`/`SaveData` em toda a pasta
  `Assets/_Game/Scripts/Economy/` confirmou que `WeaponInfusionRegistry.cs` e `ShopManager.cs` eram
  os únicos arquivos de Economy referenciando Save. Os 3 DTOs (`WeaponInfusionSaveData`,
  `ShopStockSaveData`, `ShopItemStockEntry`) só têm campos `string`/`int`/`List<>` — sem
  `UnityEngine.*` — elegíveis para `CindarsHope.Foundation`.
- Mudança:
  - Novo arquivo `Assets/_Game/Scripts/Foundation/SaveSchema/EconomySaveDtos.cs` com os 3 DTOs
    movidos de `CindarsHope.Save` para `CindarsHope.Foundation` (mesmo nome de classe/campo —
    JsonUtility serializa por nome de campo, sem migration).
  - `SaveData.cs` removeu as 3 classes e passou a usar `using CindarsHope.Foundation;`.
  - `WeaponInfusionRegistry.cs`/`ShopManager.cs` trocaram `using CindarsHope.Save;` por
    `using CindarsHope.Foundation;`.
  - `using CindarsHope.Economy;` morto removido de `SaveManager.Migration.cs`.
  - `ArchitectureRatchetTests.FoundationAssembly_ContainsOnlyTheCuratedPureContracts` atualizado
    (allowlist explícita) para incluir `EconomySaveDtos.cs` — crescimento anunciado do escopo de
    Foundation, não regressão.
- Gates:
  - `tools/architecture/Get-ModularizationDependencySnapshot.ps1`: exit 0,
    `MutualModulePairs=36 -> 35`, `Economy|Save` removido, nenhum par novo apareceu.
  - `tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1`: exit 0, 7/7 projetos, 0 warnings, 0 erros.
  - `tools/unity/RunUnityEditModeTests.ps1 -ResultsPath TestResults\cut-economy-save-editmode-2.xml -LogFile Logs\cut-economy-save-2.log`:
    exit 0, 2747/2747 PASS, 0 failed (1ª rodada teve 1 falha esperada no ratchet de Foundation,
    corrigida atualizando a allowlist).
- Ainda não declarar modularização ampla concluída: restam 35 pares mútuos para specs-filhas.
- Sem push. Working tree segue com mudanças concorrentes de arte/animação/ProjectSettings/tools
  fora do escopo desta spec (não tocadas/incluídas).

## 2026-07-07 — Save/World cycle reduction v21 (microcut Tier 1)

- Spec implementada: `.specs/implementados/spec_arch_save_world_cycle_reduction_v21.md`.
- Objetivo: quebrar o par mútuo `Save|World` (microcut Tier 1 do
  `docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md`), sem alterar gameplay, saves, schema,
  cenas, prefabs ou IDs.
- Passo 0 (verificação obrigatória): grep de `using CindarsHope.Save` em toda a pasta
  `Assets/_Game/Scripts/World/` confirmou que `TreeRegistry.cs` e `Calendar/GameCalendarService.cs`
  eram os únicos arquivos de World importando o namespace Save. Outros 9 arquivos que casavam com
  o grep textual `SaveData` (`GodMarkSaveData`, `TreeSaveData`, `ItemPickupSaveData` etc.) já são
  DTOs owned pelo próprio `CindarsHope.World` — não geravam aresta.
- Achado extra: `GameCalendarService.RestoreFromSaveData(CalendarSaveData)` não tinha nenhum caller
  em todo o repositório (grep completo confirmado) — código morto isolado; `CalendarSaveData` também
  não pertence ao `GameSaveData` raiz.
- Mudança:
  - `TreeRegistry.RestoreFromSaveData(FarmSaveData saveData)` virou
    `TreeRegistry.RestoreFromSaveData(IReadOnlyList<TreeSaveData> trees)` — mesmo corpo, só troca o
    parâmetro DTO por uma lista de `TreeSaveData` (já `CindarsHope.World`).
  - `WorldSectionProvider.Restore` e `FarmSceneRuntimeStateCache.TryRestore` (2 callers reais,
    ambos identificados por grep) pararam de montar um `FarmSaveData` wrapper só para carregar
    `Trees`; chamam `RestoreFromSaveData(trees)` direto.
  - `GameCalendarService.RestoreFromSaveData(CalendarSaveData saveData)` virou
    `GameCalendarService.RestoreFromAbsoluteDay(int absoluteDayIndex)` (sem caller, estreitado
    conforme a estratégia do mapa).
  - `using CindarsHope.Save;` removido de `TreeRegistry.cs` e `Calendar/GameCalendarService.cs`.
  - Nenhum DTO foi movido de namespace; nenhum schema/campo de save foi alterado.
- Gates:
  - `tools/architecture/Get-ModularizationDependencySnapshot.ps1`: exit 0,
    `MutualModulePairs=37 -> 36`, `RuntimeModuleEdges=226 -> 225`, `Save|World` removido, nenhum
    par novo apareceu.
  - `tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1`: exit 0, 7/7 projetos, 0 warnings, 0 erros.
  - `tools/unity/RunUnityEditModeTests.ps1 -ResultsPath TestResults\cut-save-world-editmode.xml -LogFile Logs\cut-save-world.log`:
    exit 0, 2747/2747 PASS, 0 failed.
- Ainda não declarar modularização ampla concluída: restam 36 pares mútuos para specs-filhas.
- Sem push. Working tree segue com mudanças concorrentes de arte/animação/ProjectSettings/tools
  fora do escopo desta spec (não tocadas/incluídas).

## 2026-07-07 — Player/Save cycle reduction v20 (microcut Tier 1)

- Spec implementada: `.specs/implementados/spec_arch_player_save_cycle_reduction_v20.md`.
- Objetivo: quebrar o par mútuo `Player|Save` (microcut Tier 1 do
  `docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md`), sem alterar gameplay, saves, schema,
  cenas, prefabs ou IDs.
- Passo 0 (verificação obrigatória): grep de `CindarsHope.Save` em toda a pasta
  `Assets/_Game/Scripts/Player/` confirmou que `PlayerManager.cs` era o único arquivo do domínio
  Player referenciando Save (via `CaptureSaveData`/`RestoreFromSaveData` tipados por
  `PlayerSaveData`). `ManaManagerSaveData` e `PlayerProgressionSaveData` já vivem no próprio
  namespace `CindarsHope.Player*`, não geravam aresta.
- Mudança:
  - `PlayerManager.CaptureSaveData(int, int, Vector2)` removido; `PlayerSectionProvider.Capture`
    monta `new PlayerSaveData { ... }` direto via getters públicos (`CurrentHP`, `MaxHP`,
    `CurrentGold`).
  - `PlayerManager.RestoreFromSaveData(PlayerSaveData)` virou
    `PlayerManager.RestoreState(int maxHP, int currentHP, int gold)` — corpo idêntico (mesmo clamp,
    mesmos deltas, mesmos eventos `GoldChangedEvent`/`HPChangedEvent`), só trocando o parâmetro DTO
    por três primitivos; `PlayerSectionProvider.Restore` desempacota o DTO e chama o novo método.
  - `using CindarsHope.Save;` removido de `PlayerManager.cs`.
  - `PlayerSaveData` **não foi movido** — schema/campos preservados em `CindarsHope.Save`.
- Gates:
  - `tools/architecture/Get-ModularizationDependencySnapshot.ps1`: exit 0,
    `MutualModulePairs=38 -> 37`, `RuntimeModuleEdges=227 -> 226`, `Player|Save` removido, nenhum
    par novo apareceu.
  - `tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1`: exit 0, 7/7 projetos, 0 warnings, 0 erros.
  - `tools/unity/RunUnityEditModeTests.ps1 -ResultsPath TestResults\cut-player-save-editmode.xml -LogFile Logs\cut-player-save.log`:
    exit 0, 2747/2747 PASS, 0 failed.
- Ainda não declarar modularização ampla concluída: restam 37 pares mútuos para specs-filhas.
- Sem push. Working tree segue com mudanças concorrentes de arte/animação/ProjectSettings/tools
  fora do escopo desta spec (não tocadas/incluídas).

## 2026-07-07 — EnemyBrain debug telemetry v19 (Fase E)

- Spec implementada: `.specs/implementados/spec_arch_enemybrain_debug_telemetry_v19.md`.
- Objetivo: extrair a telemetria de diagnóstico (logs one-shot e avisos) do `EnemyBrain` para
  `EnemyDebugTelemetry`, sem alterar dano, cooldown, range, stun, posture, movimento, seleção de
  ação, loot, cave scaling, saves, cenas, prefabs, IDs ou balanceamento.
- Mudança:
  - criado `Assets/_Game/Scripts/Enemy/EnemyDebugTelemetry.cs` (classe C# pura, instanciada por
    `EnemyBrain`);
  - migrados `LogThreatExpiredOnce` (com o guard one-shot `_threatExpiredLogged`),
    `Debug.LogWarning("BossSwapActionSetMissing")`, o log de `EliteWardedResistedStatus`, o log de
    `EliteVolatileExploding` e o log de `EnemyPackLeashReset`;
  - `EnemyBrain` chama `_telemetry.ResetThreatExpiredLog()` nos três pontos onde o guard antigo era
    resetado (`OnEnable`, reengajamento em `EvaluateState`, `OnPackAlert`);
  - registrado `EnemyDebugTelemetry.cs` em `CindarsHope.Runtime.csproj` (lista explícita de
    `<Compile Include>`, sem glob).
- **Desvio deliberado do pedido**: `AnnouncePackEngagementOnce` foi citado no pedido como exemplo de
  método a extrair, mas na leitura do código real ele não é um log — chama
  `_packCoordinator.Alert(...)`, ou seja, é gameplay real (acorda o pack), guardado por
  `_packEngagedAnnounced` que também controla comportamento. Permaneceu no `EnemyBrain`, sem
  alteração. Pelo mesmo motivo, `_wardedStatusConsumed` e `_volatileExploded` (guards de
  gameplay real: resistir ao primeiro status / explodir uma vez) não foram movidos — só as
  chamadas de log correspondentes foram extraídas (stateless), preservando texto e ponto de disparo.
- Gates:
  - `tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1`: exit 0, 7/7 projetos, 0 warnings, 0 errors.
  - `tools/unity/RunUnityEditModeTests.ps1`: exit 0, 2747/2747 PASS,
    `TestResults/enemybrain-debug-telemetry-editmode.xml`.
  - `tools/architecture/Get-ModularizationDependencySnapshot.ps1`: exit 0,
    `RuntimeModuleEdges=227`, `MutualModulePairs=38`, sem novo ciclo (idêntico antes/depois).
- Fase E do plano fecha com este recorte: os itens anteriores da lista (targeting, tuning,
  action-selection, config) já haviam sido cobertos por specs anteriores.

## 2026-07-07 — NPC debug expression policy v18

- Spec implementada: `.specs/implementados/spec_arch_npc_debug_expression_policy_v18.md`.
- Objetivo: remover duplicação de debug expression entre `NpcController` e `NpcShopController`,
  sem alterar UI, eventos publicados, diálogos, saves, cenas, prefabs, IDs ou gameplay.
- Mudança:
  - criado `NpcDebugExpressionChoicePolicy`;
  - `NpcController` e `NpcShopController` usam a mesma policy para `dbg_open`, `dbg_back`,
    escolhas de expressão e parse de `NpcExpression`;
  - `NpcShopChoiceUiAdapter` converte as escolhas para `DialogueChoice`.
- Gates:
  - `dotnet build .\CindarsHope.Runtime.csproj`: exit 0, 0 warnings, 0 errors.
  - `tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1`: exit 0, 7/7 projetos, 0 warnings, 0 errors.
  - `tools/unity/RunUnityEditModeTests.ps1`: exit 0, 2747/2747 PASS,
    `TestResults/npc-debug-expression-policy-editmode.xml`.
  - `tools/architecture/Get-ModularizationDependencySnapshot.ps1`: exit 0,
    `RuntimeModuleEdges=227`, `MutualModulePairs=38` sem novo ciclo.
- Próximos recortes do `NpcShopController` ainda pendentes: dialog/tree presenter, gifting,
  quest bridge, transaction facade e availability/schedule presentation.

## 2026-07-07 — NpcShopController city service policy v17

- Spec implementada: `.specs/implementados/spec_arch_npcshop_city_service_policy_v17.md`.
- Objetivo: executar mais um recorte seguro da fase `NpcShopController`, sem alterar gameplay,
  saves, cenas, prefabs, IDs, horários, diálogos, preços, quantidades ou compra de serviços.
- Mudança:
  - criado `NpcCityServiceChoicePolicy`;
  - `NpcShopController.TryGetCityServiceChoice` delega regra de provedor, `serviceId` e label;
  - `NpcShopController.PurchaseCityServiceForThisNpc` delega compra idempotente e fallback de mensagem;
  - preservados `ChoiceId = "service"`, `"Servico (ja contratado)"`, `"Servico indisponivel."`
    e uso canônico de `CityServiceCatalog`/`CityServiceAccess`.
- Gates:
  - `dotnet build .\CindarsHope.Runtime.csproj`: exit 0, 0 warnings, 0 errors.
  - `tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1`: exit 0, 7/7 projetos, 0 warnings, 0 errors.
  - `tools/unity/RunUnityEditModeTests.ps1`: exit 0, 2747/2747 PASS,
    `TestResults/npcshop-city-service-policy-editmode.xml`.
  - `tools/architecture/Get-ModularizationDependencySnapshot.ps1`: exit 0,
    `RuntimeModuleEdges=227`, `MutualModulePairs=38` sem novo ciclo.
- Próximos recortes do `NpcShopController` ainda pendentes: debug expression menu, gifting,
  quest bridge, transaction facade e dialog/tree presenter.

## 2026-07-07 — NpcShopController special identity policy v16

- Spec implementada: `.specs/implementados/spec_arch_npcshop_special_identity_policy_v16.md`.
- Objetivo: executar mais um recorte seguro da fase `NpcShopController`, sem alterar gameplay,
  saves, cenas, prefabs, IDs, horários, diálogos, preços ou quantidades.
- Mudança:
  - criado `NpcSpecialIdentityPolicy`;
  - `NpcShopController.IsThalindra` e `NpcShopController.IsBrumdar` delegam para a policy;
  - preservadas comparações por `NpcId` e fallback por `DisplayName`, case-insensitive.
- Gates:
  - `dotnet build .\CindarsHope.Runtime.csproj`: exit 0, 0 warnings, 0 errors.
  - `tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1`: exit 0, 7/7 projetos, 0 warnings, 0 errors.
  - `tools/unity/RunUnityEditModeTests.ps1`: exit 0, 2747/2747 PASS,
    `TestResults/npcshop-special-identity-policy-editmode.xml`.
- Próximos recortes do `NpcShopController` ainda pendentes: diálogo, gifting, quest bridge,
  transaction facade e schedule/presentation adapter.

## 2026-07-07 — NpcShopController choice UI adapter v15

- Spec implementada: `.specs/implementados/spec_arch_npcshop_choice_ui_adapter_v15.md`.
- Objetivo: executar mais um recorte seguro da fase `NpcShopController`, sem alterar gameplay,
  saves, cenas, prefabs, IDs, horários, diálogos, preços ou quantidades.
- Mudança:
  - criado `NpcShopChoiceUiAdapter` em `CindarsHope.UI.Dialogue`;
  - `NpcShopController` usa `NpcShopChoiceUiAdapter.ToUiChoices`;
  - método privado `ToUiChoices` foi removido do controller;
  - labels e choice IDs são preservados.
- Gates:
  - `dotnet build .\CindarsHope.Runtime.csproj`: exit 0, 0 warnings, 0 errors.
  - `tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1`: exit 0, 7/7 projetos, 0 warnings, 0 errors.
  - `tools/unity/RunUnityEditModeTests.ps1`: exit 0, 2747/2747 PASS,
    `TestResults/npcshop-choice-ui-adapter-editmode.xml`.
- Próximos recortes do `NpcShopController` ainda pendentes: diálogo, gifting, quest bridge,
  transaction facade e schedule/presentation adapter.

## 2026-07-07 — NpcShopController service choice builder v14

- Spec implementada: `.specs/implementados/spec_arch_npcshop_service_choice_builder_v14.md`.
- Objetivo: executar mais um recorte seguro da fase `NpcShopController`, sem alterar gameplay,
  saves, cenas, prefabs, IDs, horários, diálogos, preços, quantidades ou execução de serviços.
- Mudança:
  - criado `NpcShopServiceChoiceBuilder`;
  - `NpcShopController.BuildNpcServiceChoices` delega para o builder;
  - preservado comportamento anterior: uma opção por serviço único, serviços gated continuam
    aparecendo, `ServiceId` vazio continua ignorado e labels continuam vindo de
    `NpcServiceAccess.BuildOptions`.
- Gates:
  - `dotnet build .\CindarsHope.Runtime.csproj`: exit 0, 0 warnings, 0 errors.
  - `tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1`: exit 0, 7/7 projetos, 0 warnings, 0 errors.
  - `tools/unity/RunUnityEditModeTests.ps1`: exit 0, 2747/2747 PASS,
    `TestResults/npcshop-service-choice-builder-editmode.xml`.
- Próximos recortes do `NpcShopController` ainda pendentes: diálogo, gifting, quest bridge,
  transaction facade e schedule/presentation adapter.

## 2026-07-07 — NpcShopController initialization guard v13

- Spec implementada: `.specs/implementados/spec_arch_npcshop_initialization_guard_v13.md`.
- Objetivo: executar um recorte seguro da fase `NpcShopController` do plano restante de
  modularização, sem alterar gameplay, saves, cenas, prefabs, IDs, horários, diálogos, preços,
  quantidades ou fluxos de shop.
- Mudança:
  - criado `NpcShopInitializationGuard`;
  - `NpcShopController.TryEnsureShopInitialized` delega validação de referências obrigatórias ao guard;
  - método local `ValidateReference` foi removido;
  - preservado comportamento anterior de short-circuit: mesma ordem de validação e só a primeira
    referência ausente é logada.
- Gates:
  - `dotnet build .\CindarsHope.Runtime.csproj`: exit 0, 0 warnings, 0 errors.
  - `tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1`: exit 0, 7/7 projetos, 0 warnings, 0 errors.
  - `tools/unity/RunUnityEditModeTests.ps1`: exit 0, 2747/2747 PASS,
    `TestResults/npcshop-initialization-guard-editmode.xml`.
- Próximos recortes do `NpcShopController` ainda pendentes: diálogo, gifting, quest bridge,
  transaction facade e schedule/presentation adapter.

## 2026-07-07 — Core/NPC cycle reduction v12

- Spec-filha implementada: `.specs/implementados/spec_arch_core_npc_cycle_reduction_v12.md`.
- Objetivo: remover o par mútuo `Core|NPC` sem alterar gameplay, saves, cenas, prefabs,
  IDs, balanceamento ou payloads de evento.
- Mudança:
  - `NpcExpressionOverrideEvent`, `NpcGiftReactionEvent` e os eventos de romance foram movidos de
    `CindarsHope.Core.Events` para `CindarsHope.NPC.Events`.
  - Os `.meta` foram movidos junto com os `.cs` para preservar GUID.
  - Consumidores em NPC, Quests, UI e testes importam `CindarsHope.NPC.Events`.
  - Payloads, nomes de eventos, semântica de publicação/subscription e eventos observáveis foram
    preservados.
- Snapshot:
  - antes: `MutualModulePairs=39`, `RuntimeModuleEdges=228`;
  - depois: `MutualModulePairs=38`, `RuntimeModuleEdges=227`;
  - `Core|NPC` removido; nenhum par novo apareceu.
- Gates:
  - `tools/architecture/Get-ModularizationDependencySnapshot.ps1`: exit 0.
  - `tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1`: exit 0, 7/7 projetos, 0 warnings, 0 errors.
  - `tools/unity/RunUnityEditModeTests.ps1`: exit 0, 2747/2747 PASS,
    `TestResults/modularization-core-npc-editmode.xml`.
- Ainda não declarar modularização ampla concluída: restam 38 pares mútuos para specs-filhas.

## 2026-07-07 — Core/World cycle reduction v11

- Spec-filha implementada: `.specs/implementados/spec_arch_core_world_cycle_reduction_v11.md`.
- Objetivo: remover o par mútuo `Core|World` sem alterar gameplay, saves, cenas, prefabs,
  IDs, balanceamento ou eventos observáveis.
- Mudança:
  - `WeatherChangedEvent` foi movido de `CindarsHope.Core.Events` para `CindarsHope.World.Weather`.
  - O `.meta` foi movido junto com o `.cs` para preservar GUID.
  - `WorldWeatherService` continua publicando o mesmo payload via `GameEventBus`.
  - Nenhum valor de `WeatherType`, save/schema/cena/prefab/asset foi alterado.
- Snapshot:
  - antes: `MutualModulePairs=40`, `RuntimeModuleEdges=229`;
  - depois: `MutualModulePairs=39`, `RuntimeModuleEdges=228`;
  - `Core|World` removido; nenhum par novo apareceu.
- Gates:
  - `tools/architecture/Get-ModularizationDependencySnapshot.ps1`: exit 0.
  - `tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1`: exit 0, 7/7 projetos, 0 warnings, 0 errors.
  - `tools/unity/RunUnityEditModeTests.ps1`: exit 0, 2747/2747 PASS,
    `TestResults/modularization-core-world-editmode.xml`.
- Ainda não declarar modularização ampla concluída: restam 39 pares mútuos para specs-filhas.

## 2026-07-07 — Core/Farm cycle reduction v10

- Spec-filha implementada: `.specs/implementados/spec_arch_core_farm_cycle_reduction_v10.md`.
- Objetivo: remover o par mútuo `Core|Farm` sem alterar gameplay, saves, cenas, prefabs,
  IDs, balanceamento ou assets.
- Mudança:
  - `SeedDatabaseSO` foi movido de `CindarsHope.Core.Data` para `CindarsHope.Farm.Data`.
  - O `.meta` foi movido junto com o `.cs` para preservar GUID do ScriptableObject.
  - A base `DataRegistrySO<SeedDataSO>` continua em `Core.Data`; consumidores editor/runtime usam
    `CindarsHope.Farm.Data`.
  - Nenhum ID de seed, asset, cena, prefab, save/schema ou balanceamento foi alterado.
- Snapshot:
  - antes: `MutualModulePairs=41`, `RuntimeModuleEdges=230`;
  - depois: `MutualModulePairs=40`, `RuntimeModuleEdges=229`;
  - `Core|Farm` removido; nenhum par novo apareceu.
- Gates:
  - `tools/architecture/Get-ModularizationDependencySnapshot.ps1`: exit 0.
  - `tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1`: exit 0, 7/7 projetos, 0 warnings, 0 errors.
  - `tools/unity/RunUnityEditModeTests.ps1`: exit 0, 2747/2747 PASS,
    `TestResults/modularization-core-farm-editmode.xml`.
- Ainda não declarar modularização ampla concluída: restam 40 pares mútuos para specs-filhas.

## 2026-07-07 — SceneManagement/World cycle reduction v9

- Spec-filha implementada: `.specs/implementados/spec_arch_scene_world_cycle_reduction_v9.md`.
- Objetivo: remover o par mútuo `SceneManagement|World` sem alterar gameplay, saves, cenas,
  prefabs, IDs, balanceamento ou fluxo de transição.
- Mudança:
  - `SceneNames`, `SceneTransitionState` e o legacy `SceneSpawnPoint` foram movidos para
    `CindarsHope.World.Scenes`.
  - Os `.meta` foram movidos junto com os `.cs` para preservar GUID de scripts Unity.
  - `SceneTransitionRouter`, `SceneId` e `PlayerSpawnResolver` deixaram de importar
    `CindarsHope.SceneManagement`.
  - `SceneManagement` continua consumindo `World.Scenes` onde necessário; nenhum ID/campo
    serializado/save/schema/cena/prefab foi alterado.
- Snapshot:
  - antes: `MutualModulePairs=42`, `RuntimeModuleEdges=231`;
  - depois: `MutualModulePairs=41`, `RuntimeModuleEdges=230`;
  - `SceneManagement|World` removido; nenhum par novo apareceu.
- Gates:
  - `tools/architecture/Get-ModularizationDependencySnapshot.ps1`: exit 0.
  - `tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1`: exit 0, 7/7 projetos, 0 warnings, 0 errors.
  - `tools/unity/RunUnityEditModeTests.ps1`: exit 0, 2747/2747 PASS,
    `TestResults/modularization-scene-world-editmode.xml`.
- Ainda não declarar modularização ampla concluída: restam 41 pares mútuos para specs-filhas.

## 2026-07-07 — Craft/Save cycle reduction v8

- Spec-filha implementada: `.specs/implementados/spec_arch_craft_save_cycle_reduction_v8.md`.
- Objetivo: remover o par mútuo `Craft|Save` sem alterar gameplay, saves, cenas, prefabs,
  IDs, balanceamento ou schema.
- Mudança:
  - Removido `using CindarsHope.Save` morto de `CraftingStation.cs`.
  - Os DTOs de crafting continuam no domínio `Craft`; nenhum tipo/campo foi movido ou renomeado.
- Snapshot:
  - antes: `MutualModulePairs=43`, `RuntimeModuleEdges=232`;
  - depois: `MutualModulePairs=42`, `RuntimeModuleEdges=231`;
  - `Craft|Save` removido; nenhum par novo apareceu.
- Gates:
  - `tools/architecture/Get-ModularizationDependencySnapshot.ps1`: exit 0.
  - `tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1`: exit 0, 7/7 projetos, 0 warnings, 0 errors.
  - `tools/unity/RunUnityEditModeTests.ps1`: exit 0, 2747/2747 PASS,
    `TestResults/modularization-craft-save-editmode.xml`.
- Ainda não declarar modularização ampla concluída: restam 42 pares mútuos para specs-filhas.

## 2026-07-07 — Locations/Player cycle reduction v7

- Spec-filha implementada: `.specs/implementados/spec_arch_locations_player_cycle_reduction_v7.md`.
- Objetivo: remover o par mútuo `Locations|Player` sem alterar gameplay, saves, cenas, prefabs,
  IDs, balanceamento ou fluxo de respawn.
- Mudança:
  - Criado `IAnyaFountainRespawnPoint` em `CindarsHope.Core.Respawn`.
  - `AnyaFountain` implementa o contrato e preserva o mesmo `RespawnPoint`.
  - `AnyaFountainRespawnFlow` deixou de importar `CindarsHope.Locations` e resolve a âncora ativa
    pelo contrato.
  - `CindarsHope.Runtime.csproj` inclui o novo arquivo runtime.
- Snapshot:
  - antes: `MutualModulePairs=44`, `RuntimeModuleEdges=233`;
  - depois: `MutualModulePairs=43`, `RuntimeModuleEdges=232`;
  - `Locations|Player` removido; nenhum par novo apareceu.
- Gates:
  - `tools/architecture/Get-ModularizationDependencySnapshot.ps1`: exit 0.
  - `tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1`: exit 0, 7/7 projetos, 0 warnings, 0 errors.
  - `tools/unity/RunUnityEditModeTests.ps1`: exit 0, 2747/2747 PASS,
    `TestResults/modularization-locations-player-editmode.xml`.
- Ainda não declarar modularização ampla concluída: restam 43 pares mútuos para specs-filhas.

## 2026-07-07 — Fonte/MainProgression cycle reduction v6

- Spec-filha implementada: `.specs/implementados/spec_arch_fonte_mainprogression_cycle_reduction_v6.md`.
- Objetivo: remover o par mútuo `Fonte|MainProgression` sem alterar gameplay, saves, cenas, prefabs,
  IDs, balanceamento ou fluxo de final choice.
- Mudança:
  - `FinalChoiceService` e `FinalChoiceRuntimeAdapter` deixaram de depender diretamente de
    `CindarsHope.Fonte`.
  - `MainProgression` agora define o contrato pequeno `IFinalChoiceFonteStateSink`.
  - `FonteAnyaSection` implementa esse contrato e mantém o mapeamento concreto para `FonteState`.
- Snapshot:
  - antes: `MutualModulePairs=45`, `RuntimeModuleEdges=234`;
  - depois: `MutualModulePairs=44`, `RuntimeModuleEdges=233`;
  - `Fonte|MainProgression` removido; nenhum par novo apareceu.
- Gates:
  - `tools/architecture/Get-ModularizationDependencySnapshot.ps1`: exit 0.
  - `tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1`: exit 0, 7/7 projetos, 0 warnings, 0 errors.
  - `tools/unity/RunUnityEditModeTests.ps1 -Filter "CindarsHope.Tests.EditMode.MainProgression"`:
    exit 0, 2747/2747 PASS, `TestResults/modularization-fonte-mainprogression-editmode.xml`.
- Observação operacional: primeira tentativa de build direto `dotnet build --no-restore` falhou por
  ausência de `Temp/obj/*/project.assets.json`; após `dotnet restore .\CindarsHope.Runtime.csproj`,
  o build wrapper oficial passou.
- Ainda não declarar modularização ampla concluída: restam 44 pares mútuos para specs-filhas.

## 2026-07-06 — V5 Lote 6/7: ciclos selecionados + closeout

- Spec `spec_arch_modularization_residual_v5.md` foi concluída e movida de
  `.specs/a_implementar/` para `.specs/implementados/`; registry atualizado em
  `.specs/SPEC_REGISTRY_IMPLEMENTED.md`. `SPEC_REGISTRY_TO_IMPLEMENT.md` NÃO foi tocado porque segue
  em concorrência com a sessão Cave.
- Snapshot inicial do lote: `MutualModulePairs=47`, `RuntimeModuleEdges=235`.
- Snapshot final: `MutualModulePairs=45`, `RuntimeModuleEdges=234`, sem par novo.
- Pares removidos:
  - `Farm|Interaction`: `InteractionSystem` usa `GameplayInputBlocker.IsBlockedBy(FarmActionMenu)`
    em vez de `FarmPlot.IsAnyActionMenuOpen`. Commit `f5f4ffb9`.
  - `Quests|UI`: Quest UI passou a ser instalada por `PresentationRuntimeInstaller`; `QuestRuntimeBootstrap`
    não instancia mais controllers de UI. Commit `b2a2e399`.
- Pares avaliados e pulados por risco: `Equipment|Inventory` (`EquipmentSlot` muito espalhado),
  `Craft|UI` (referência serializada opcional ao `CraftingModal`) e `Quests|Save` (DTOs de save).
- Gates executados pelo Codex:
  - `Invoke-UnityGeneratedProjectsBuild.ps1`: exit 0, 7/7, 0 warnings, 0 erros.
  - `RunUnityEditModeTests.ps1 -ResultsPath TestResults\v5-lote6-editmode.xml -LogFile Logs\v5-lote6.log`:
    exit 0, 2747/2747, 0 failed.
  - PlayMode composição `GameRuntimeCompositionRootPlayModeTests`: exit 0, 2/2, 0 failed
    (`Logs\v5-lote6-playmode-composition.xml`).
- Importante: não declarar “modularização ampla concluída”. A spec v5 está concluída no escopo
  aprovado, mas ainda restam 45 pares mútuos para specs-filhas.
- Não houve push. Working tree ainda tem mudanças concorrentes Cave/arte/docs que não pertencem a
  este rework.

## 2026-07-06 — V5 Batch 6: diagnóstico NPC (×1), feito no loop principal

- Spec ativa: `.specs/a_implementar/spec_arch_modularization_residual_v5.md`.
- Migrado `NpcDialogueExpansionBootstrap` de `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]` para
  `NpcDialogueExpansionBootstrap.Install()`, chamado por `NpcRuntimeInstaller` no `Start()` do
  `GameRuntimeCompositionRoot`.
- O bootstrap é diagnóstico sem estado: não cria `GameObject`, não assina eventos, não altera cena,
  assets, save ou gameplay; apenas loga cobertura de diálogo/roster e warning se a cobertura mínima
  não for atingida. Mudança feita só para ownership centralizado de chamada.
- Contagem `[RuntimeInitializeOnLoadMethod]`: 4 → **3**. Restam apenas:
  `GameRuntimeCompositionRoot.Bootstrap` (`BeforeSceneLoad`, manter), `SceneTransitionRouter`
  (`SubsystemRegistration`, manter por design) e `CollisionDebugOverlayBootstrap` (debug opt-in,
  só mexer com spec própria de define/config).
- Arquivos alterados neste batch: `Assets/_Game/Scripts/NPC/NpcDialogueExpansionBootstrap.cs`,
  `Assets/_Game/Scripts/Composition/DomainRuntimeInstallers.cs`,
  `docs/architecture/RUNTIME_BOOTSTRAP_OWNERSHIP_V5.md` e este handoff.
- Validação feita neste batch: contagem por grep antes/depois e inspeção do código. Ainda não rodei
  Unity/EditMode/PlayMode depois deste batch por limite de contexto; próximo executor deve rodar ao
  menos build das assemblies + EditMode/PlayMode de composição antes de consolidar/pushar.
- Atenção: a working tree segue misturada com mudanças concorrentes de Cave/docs/agentes/arte. Não
  stagear/commitar esses arquivos como parte deste batch sem autorização explícita.

## 2026-07-06 — V5 Batch 5: audio (×2), feito e verificado no loop principal

- Spec ativa: `.specs/a_implementar/spec_arch_modularization_residual_v5.md`.
- Migrados `AudioManager` e `SfxEventBridge` de `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]`
  para `AudioRuntimeInstaller.Install(owner)`, chamado por ÚLTIMO no `Start()` do
  `GameRuntimeCompositionRoot` (AudioManager antes do bridge). `SetParent(owner)` + `DontDestroyOnLoad`,
  lógica interna intacta (voices, crossfade, `EnsureAudioListener`, cooldown, subscribe simétrico).
- Refutada a hipótese antiga de "precisa de estágio AfterSceneLoad novo no root": `Start()` já é
  pós-cena. PlayMode 2/2 + Player.log confirmam UM AudioListener (criado só quando a cena não tem),
  idêntico ao original. Removido `using CindarsHope.Core;` morto (CS8019) do AudioManager.
- Contagem `[RuntimeInitializeOnLoadMethod]`: 6 → **4**. Restam: root (`BeforeSceneLoad`, nunca migra),
  `SceneTransitionRouter` (`SubsystemRegistration`, por design), `CollisionDebugOverlayBootstrap` (debug)
  e `NpcDialogueExpansionBootstrap` (diagnóstico) — os 2 últimos só via spec própria de define/config.
- Gates (rodados no loop principal, não delegados): build 7/7 0W/0E, EditMode 2747/2747, PlayMode 2/2.
- NOTA de execução: batches 2-4 foram delegados a subagents; o de UI spawnou um ghost child (violando
  a proibição) que, apesar disso, completou o batch 4 corretamente e commitou. Batch 5 foi feito
  direto no loop principal para evitar o ghost. Sessão CONCORRENTE (`spec_cave_decor_composition_runtime`)
  segue mutando a mesma working tree — atribuir falhas de Cave a ela, não ao rework.
- Sem push. `dev` à frente do remoto; publicar só com autorização explícita.

## 2026-07-06 — V5 Batch 4: apresentação (UI/cena, ×7)

- Spec ativa: `.specs/a_implementar/spec_arch_modularization_residual_v5.md`.
- Objetivo: migrar os 7 serviços de UI/cena listados em `RUNTIME_BOOTSTRAP_OWNERSHIP_V5.md` (última
  categoria "UI/cena") de auto-bootstrap `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]` para
  `PresentationRuntimeInstaller`, instalado no `Start()` do composition root, preservando 100% do
  comportamento observável.
- Aviso de concorrência respeitado: nenhuma mudança tocou `Cave/**`, `Enemy/EnemyAnimator.cs`,
  `Editor/Enemy/GenerateEnemyWalkAnimations.cs`, `tools/enemy_anim/*`, `tools/aseprite/*`,
  `ProjectSettings/*` ou `cindars_hope.slnx` (alterados concorrentemente por outra sessão).
- Arquivos alterados:
  - `Assets/_Game/Scripts/Narrative/IntroSequenceController.cs` — `EnsureInstance()` (estático,
    `AfterSceneLoad`) virou `public static void Install(Transform owner)`. Criava
    `new GameObject("IntroSequenceController")` + `DontDestroyOnLoad`: host novo, `SetParent(owner)`
    adicionado. Singleton guard (`Instance`), subscribes `OnEnable`/`OnDisable`, `Update`/`OnGUI` e
    `IntroSequenceModel` inalterados.
  - `Assets/_Game/Scripts/UI/Character/CharacterEquipmentPanelController.cs` —
    `EnsureRuntimeInstance()` virou `Install(Transform owner)`. Host novo
    (`new GameObject("CharacterEquipmentPanelController")` + `DontDestroyOnLoad`): `SetParent(owner)`
    adicionado. Resto do controller (painel de atributos/equipamento, navegação por teclado,
    subscribes) inalterado.
  - `Assets/_Game/Scripts/UI/Death/DeathScreenCanvasController.cs` — `EnsureInstance()` virou
    `Install(Transform owner)`. Host novo (`new GameObject("DeathScreenCanvas")` +
    `DontDestroyOnLoad`): `SetParent(owner)` adicionado. Construção do canvas programático, pause
    token, fluxo de revive/respawn e subscribes de evento inalterados.
  - `Assets/_Game/Scripts/UI/HUD/GameplayHudBootstrap.cs` — `EnsureInstance()` (método estático de
    classe `static`, sem instância própria) virou `Install(Transform owner)`. Host novo
    (`new GameObject(HudGameObjectName)` + `DontDestroyOnLoad`): `SetParent(owner)` adicionado. O
    segundo GameObject interno (`GameplayHudTextOverlay`, criado sem parent, `DontDestroyOnLoad`
    próprio) foi preservado exatamente como estava — não recebeu `SetParent`, pois no original também
    não tinha relação de parentesco com o primeiro host (raiz própria intencional, evita colisão de
    Views no mesmo transform, conforme comentário original). Log message atualizada de "via
    RuntimeInitializeOnLoadMethod" para "via composition root Install" (cosmético, sem efeito de
    comportamento).
  - `Assets/_Game/Scripts/UI/InventoryPanelController.cs` — `EnsureRuntimeInstance()` virou
    `Install(Transform owner)`. Host novo (`new GameObject("InventoryPanelController")` +
    `DontDestroyOnLoad`): `SetParent(owner)` adicionado. Resto (slots, actions, destroy confirm,
    equipment selection, `OpenForEquipmentSelection` estático) inalterado.
  - `Assets/_Game/Scripts/UI/Skills/SkillTreeGameplayPanelController.cs` —
    `EnsureRuntimeInstance()` virou `Install(Transform owner)`. Host novo
    (`new GameObject("SkillTreeGameplayPanelController")` + `DontDestroyOnLoad`): `SetParent(owner)`
    adicionado. Navegação de árvore/nó, compra, equip em slot ativo e subscribes inalterados.
  - `Assets/_Game/Scripts/World/Scenes/SceneFadeOverlayBootstrap.cs` — `EnsureInstance()` (classe
    `static`) virou `Install(Transform owner)`. Host novo (`new GameObject("SceneFadeOverlay")` +
    `DontDestroyOnLoad`): `SetParent(owner)` adicionado. Construção do canvas de fade (imagem preta,
    `CanvasGroup`, `GraphicRaycaster` desabilitado) inalterada.
  - `Assets/_Game/Scripts/Composition/DomainRuntimeInstallers.cs` — novo
    `PresentationRuntimeInstaller.Install(Transform owner)`, chamando os 7 `Install(owner)` acima na
    ordem listada.
  - `Assets/_Game/Scripts/Composition/GameRuntimeCompositionRoot.cs` — `Start()` passou a chamar
    `PresentationRuntimeInstaller.Install(transform)` por último, após
    `PlayerLifecycleRuntimeInstaller` (apresentação depende de player/lifecycle já instalados).
  - `Assets/_Game/Tests/EditMode/World/SceneFadeOverlayTests.cs` — o teste
    `SceneFadeOverlayBootstrap_HasRuntimeInitializeAttribute` (que asserta a presença do atributo
    removido) foi reescrito como `SceneFadeOverlayBootstrap_HasInstallMethod`, verificando a presença
    do novo contrato público `Install(Transform)` — mesma cobertura de "o bootstrap tem um ponto de
    entrada estático", adaptada ao novo padrão.
  - `docs/architecture/RUNTIME_BOOTSTRAP_OWNERSHIP_V5.md` — categoria "UI/cena" zerada; contagem
    13 → 6.
- Todos os 7 seguiam o MESMO padrão de host novo cross-scene (`DontDestroyOnLoad`) — nenhum caso era
  canvas/objeto de cena já existente adotado sem reparent; por isso todos receberam `SetParent(owner)`,
  sem exceção.
- Comportamento preservado: momento de instalação (`AfterSceneLoad` → `Start()` do root, mesmo estágio
  observável pós-carregamento de cena), guards de singleton estático, `DontDestroyOnLoad`, ordem de
  criação de canvas/UI, todos os subscribes de `GameEventBus`, toda a lógica de `Update`/`OnGUI`,
  coroutines e nenhuma mudança de gameplay/save/IDs/balance/cena/prefab.
- Validação:
  - `Invoke-UnityGeneratedProjectsBuild.ps1`: exit 0, 7/7 projetos, 0 warnings/0 erros (rodado duas
    vezes — antes e depois do ajuste do teste `SceneFadeOverlayTests`; ambas exit 0).
  - `RunUnityEditModeTests.ps1`: primeira rodada (`TestResults/residual-v5-batch4-editmode.xml`) deu
    exit 1, 2746/2747 PASS — 1 falha em
    `SceneFadeOverlayTests.SceneFadeOverlayBootstrap_HasRuntimeInitializeAttribute` (teste
    pré-existente que asserta o atributo removido, esperado pela migração). Corrigido o teste;
    segunda rodada (`TestResults/residual-v5-batch4-editmode-2.xml`,
    `Logs/residual-v5-batch4-2.log`): exit 0, 2747/2747 PASS, 0 failed.
  - Contagem `[RuntimeInitializeOnLoadMethod]` real (excluindo comentários/strings em arquivos
    Editor que apenas mencionam o atributo) em `Assets/_Game/Scripts/**`: 13 → 6. Os 6 restantes:
    `GameRuntimeCompositionRoot.Bootstrap` (`BeforeSceneLoad`, o próprio root — nunca migra),
    `AudioManager`, `SfxEventBridge` (Audio, deferido), `CollisionDebugOverlayBootstrap` (debug),
    `NpcDialogueExpansionBootstrap` (diagnóstico), `SceneTransitionRouter`
    (`SubsystemRegistration`, reset de subsistema).
  - Nenhuma alteração em `Cave/**` foi necessária; nenhuma falha de teste pertence ao domínio Cave
    concorrente.
- Não executado / risco residual: PlayMode manual não foi rodado nesta sessão (smoke visual por tela
  — intro sequence, painéis de character/inventory/skill tree, death screen, HUD, fade de cena). A
  mudança é puramente "quem chama o método estático de instalação e quando o host reparenta", sem
  tocar lógica interna de nenhum dos 7 controllers; risco mitigado mas não eliminado sem confirmação
  humana em Play Mode.
- Commits (português, sem push):
  - `refactor(bootstrap): centralizar apresentacao de ui` (7 controllers + installers + root).
  - commit de docs (`RUNTIME_BOOTSTRAP_OWNERSHIP_V5.md` + este handoff).
  - hashes reais: ver `git log --oneline -3` na branch `dev` — confirme no Git antes de confiar nesta
    nota.
- Próximo passo: Audio (`AudioManager`, `SfxEventBridge`) exige criar um estágio `AfterSceneLoad`
  explícito no `GameRuntimeCompositionRoot` antes de migrar (instalar Audio cedo demais afeta a
  decisão de criar `AudioListener` — este estágio ainda não existe). Depois disso, debug/diagnóstico
  (`CollisionDebugOverlayBootstrap`, `NpcDialogueExpansionBootstrap`) podem ser isolados por
  define/config, fora do fluxo de composition.

## 2026-07-05 — Maintainability Rework v2, lote 1

- Spec ativa: `.specs/a_implementar/spec_arch_runtime_maintainability_rework_v2.md`.
- `GameRuntimeCompositionRoot` agora instala um `GameplayInputRouter` único e persistente.
- Decisão de atalhos extraída para `GameplayShortcutDecision` puro (Command decision).
- Inventory, Equipment e Skill Tree passaram a consumir os eventos do router; os atalhos diretos
  ficaram somente como fallback quando o router não existe.
- Corrigido gap anterior: `InventoryPanelOpenedEvent`, `EquipmentPanelOpenedEvent` e
  `ModalCloseRequestedEvent` eram publicados sem consumidores nos painéis principais.
- Validação: 6/6 testes de decisão, 2678/2678 EditMode, 2/2 PlayMode, seis builds sem warnings e
  ratchet sem aumento.
- Próximo passo: decompor `QuestService` por progress dispatcher/persistence mapper mantendo sua API.
- Commit e publicação deste lote ainda devem ser verificados no Git; não confie nesta nota como prova.

## 2026-07-05 — reconciliação canônica e correções pós-modularização

Antes de continuar, valide o diff e os comandos; este relato não substitui evidência local.

- O código/assets atuais foram declarados fonte de verdade sobre specs antigas.
- Cânone medido: 178 EnemyDataSO/IDs únicos; catálogo tipado 117; itens 213/245; ammo 7;
  28 NPCs canônicos + 1 legacy; 23 cadeias/69 etapas; Town v9 120x90; Farm v7 64x44.
- Criada a spec retroativa
  `.specs/implementados/spec_runtime_canon_reconciliation_2026_07_05.md`.
- Criado o mapa `docs/architecture/MODULARIZATION_IMPLEMENTED_ARCHITECTURE_GUIDE.md`.
- `CURRENT_STATE`, registry de specs, plano e README de arquitetura foram reconciliados.
- Bugs corrigidos: metadata/rewards/flags de quests dinâmicas e restore; `act_1_done`; expansão da
  fazenda; arredondamentos; imunidade; perfect block; gifts; save defaults; HUD/bestiário; agenda;
  destruição de traps; dados de bestiário e contratos de testes obsoletos.
- EditMode passou 2672/2672 em `TestResults/canon-reconciliation-final.xml`.
- Alterações concorrentes excluídas permanecem:
  `GenerateEnemyWalkAnimations.cs`, `EnemyAnimator.cs`, `normalize_enemy_sheets.py` e os dois scripts
  locais em `tools/aseprite/`.
- Próximos gates obrigatórios: builds das seis assemblies, PlayMode de composição, scanner de missing
  scripts, docs validator, revisão do diff, commit e push. Não declare conclusão antes deles.

### Gates executados neste marco

- seis projetos gerados: PASS, 0 warnings/0 erros;
- ratchet arquitetural: PASS, nenhuma dívida monitorada aumentou;
- EditMode: 2672/2672 PASS;
- PlayMode composition/scenes: 2/2 PASS, incluindo varredura de missing scripts em Farm/Town/Cave;
- docs validator: exit 1 por dívidas preexistentes em specs futuras e falso positivo do scanner de
  placeholders; a nova spec e os novos documentos não aparecem nas falhas;
- commit técnico: `aabfecbb` (`fix(runtime): reconciliar canone e fechar gaps de save`).
- publicação verificada: commits `aabfecbb` e `972a7f43` enviados a `origin/dev`; após fetch,
  `HEAD == origin/dev == 972a7f43a6bce19819db0285c80386376dd091d0` e divergência `0 0`.

Copie todo o conteúdo deste documento para uma nova sessão do Claude Code quando a sessão atual
estiver próxima do limite de contexto/tokens.

---

## 2026-07-05 — Rework v2, lotes incrementais do Codex

### Lote 1 — lifecycle e input (commit `972ab169`)

- `GameRuntimeCompositionRoot` passou a instalar o `GameplayInputRouter` central.
- Inventário, equipamento e skill tree consomem comandos publicados e mantêm polling direto apenas
  como fallback quando o router não existe.
- Regra pura de decisão de atalhos isolada em `GameplayShortcutDecision`.
- Evidência: EditMode 2.678/2.678 e PlayMode 2/2.

### Lote 2A — quests (implementado, commit ainda deve ser conferido no Git)

- `QuestObjectiveProgressDispatcher` concentra seleção e roteamento de todos os eventos de progresso;
  `QuestService` não cria mais listas temporárias nem repete uma varredura por tipo de objetivo.
- `QuestDynamicInstancePersistence` concentra a conversão entre instância dinâmica e save simples,
  preservando recompensas genéricas e os fallbacks legados de NPC/caverna.
- Testes novos cobrem matching por tipo/alvo/profundidade e roundtrip/fallback das recompensas.
- Evidência atual: EditMode completa 2.689/2.689; seis projetos modulares/runtime compilam com
  0 erros e 0 warnings. `Assembly-CSharp-Editor.csproj` também termina com exit 0, mas ainda emite
  1.101 warnings CS0436 preexistentes por dupla inclusão entre a assembly legada e a asmdef Editor.
- Próximo passo: extrair o estado/roteamento de interação de `NpcShopController` sem mudar catálogo,
  preços, amizade, flags, diálogos ou transações.
- Mudanças concorrentes de animação, sprites, ProjectSettings, `.slnx` e ferramentas devem permanecer
  fora dos commits deste rework.

### Lote 2B — interação de loja/NPC (implementado, commit ainda deve ser conferido no Git)

- `NpcShopInteractionSession` aplica State Pattern puro ao lifecycle abrir/fechar/handoff de quest;
  callbacks Unity e modais continuam no controller, mas reentrada e fechamento duplicado são
  rejeitados em um único lugar.
- `ThalindraQuestDialoguePolicy` elimina a decisão duplicada entre renderização da opção e publicação
  do `QuestGiverInteractionMode`.
- Nenhum preço, item, amizade, flag, catálogo, árvore de diálogo ou transação foi alterado.
- Evidência: testes NPC focados 40/40; EditMode completa 2.695/2.695; projetos Runtime, EditMode e
  PlayMode compilaram com 0 erros/0 warnings; architecture ratchet PASS.
- Próximo passo: lote 3, substituir a seleção/execução monolítica de ações e movimento inimigo por
  registries de strategy preservando a estratégia injetável e todos os timings atuais.

### Lote 3 — strategies de ações e movimento inimigo (implementado, commit pendente de conferência)

- `EnemyActionExecutionStrategyRegistry` substitui o switch das famílias especiais (`SelfBuff`,
  combos, AoE, summon, charge e debuff); melee, projétil, rival e blink continuam no pipeline comum.
- `EnemyMovementStrategyRegistry` substitui o switch dos movimentos especiais; tipos não registrados,
  inclusive `PackLeader`, continuam caindo no chase compartilhado.
- Os dois registries são compartilhados, stateless e indexados diretamente pelo enum. Não criam
  dictionaries, wrappers ou delegates para cada instância de inimigo.
- Seleção de ação, cooldown, windup/recover, dano, animação, velocidades e timings não foram alterados.
- Evidência: seis testes novos de dispatch; EditMode completa 2.701/2.701; Runtime e EditMode build
  0 erros/0 warnings; architecture ratchet PASS.
- Próximo passo: lote 4, definir a primeira fronteira de assembly pura apenas onde a direção de
  dependência puder ser comprovada sem referências Unity reversas.

### Lote 4 — assembly pura de gameplay (implementado, commit pendente de conferência)

- Criada `CindarsHope.Gameplay` em `Assets/_Game/Scripts/Gameplay/`, com `noEngineReferences: true`
  e nenhuma referência de assembly.
- Movidos com namespace e GUID preservados: `GameplayShortcutDecision`,
  `NpcShopInteractionSession` e `ThalindraQuestDialoguePolicy`.
- `QuestGiverInteractionMode` saiu do arquivo de eventos Runtime e passou a ser contrato simples da
  nova assembly, mantendo o namespace público `CindarsHope.Core.Events` e os valores 0/1/2.
- `CindarsHope.Runtime` depende de Gameplay; não existe referência reversa. EditMode referencia as
  duas explicitamente.
- Novo teste arquitetural verifica nome da assembly, conjunto curado de quatro fontes e ausência de
  `UnityEngine` nas referências e no código.
- Snapshot re-medido: 1.570 arquivos C#, 216 edges por pasta, 49 pares mútuos, 29 tipos internal e
  3 crossings de teste. O scanner por pasta conta Gameplay como módulo novo; pares mútuos ficaram em
  49. A direção real da asmdef é unidirecional.
- Evidência: build das seis assemblies explícitas 0 erros/0 warnings; EditMode 2.702/2.702; ratchet
  PASS; resultado em `TestResults/maintainability-gameplay-full.xml`.
- Próximo passo: lote 5, tratar UI/data legacy somente com equivalência testável; não apagar telas ou
  assets apenas por grep.

### Lote 5 — UI/data legacy seguro (implementado, commit pendente de conferência)

- Os 30 mapeamentos action-skill→effect saíram de `ActiveSkillExecutionController` para
  `SkillActionEffectCatalog`, read-only e dentro da assembly pura Gameplay.
- O controller mantém `TryGetEffectIdForValidation` como fachada compatível. Os testes agora leem o
  catálogo público e não usam reflection em um dictionary privado.
- Comentário incorreto de custo pendente foi removido: custos são aplicados pelos executores
  concretos atuais.
- `ActiveSkillExecutionController` perdeu o `RuntimeInitializeOnLoadMethod` próprio e agora é
  instalado por `GameRuntimeCompositionRoot`. O componente legado presente na Farm continua aceito
  pelo guard de singleton; nenhuma cena/YAML foi alterada.
- `RuntimeInitialize` caiu de 63 para 62; os demais ratchets não devem ser elevados.
- Telas `OnGUI`, placeholders e objetos de cena sem substituto comprovado não foram apagados. Eles
  permanecem dívida intencional até spec visual/PlayMode por tela.
- Evidência: catálogo 4/4; EditMode 2.703/2.703; PlayMode composição/cenas 2/2.
- Próximo passo: lote 6, builds finais, ratchets, snapshot, documentação de closeout e verificação do
  worktree/commits. Não publicar sem autorização explícita.

### Lote 6 — closeout (concluído localmente)

- Spec movida de `a_implementar` para `implementados` e registrada no registry canônico.
- Commits do rework, em ordem: `972ab169`, `016b04fc`, `dafa05de`, `3ee1918a`, `457a69bd`,
  `bca06f94` e o commit documental de closeout que deve ser conferido no Git.
- Gates finais: seis assemblies 0E/0W; EditMode 2.703/2.703; PlayMode 2/2; ratchet PASS;
  dependency snapshot 1.570/216/49/29/3.
- Docs validator: exit 1 somente pelas dívidas preexistentes de headers em quatro specs e falsos
  positivos históricos de placeholder; nenhum documento novo do rework foi listado.
- O branch é `dev`. Nenhum push foi realizado nesta execução porque não houve autorização explícita
  de publicação no turno atual.
- Alterações concorrentes listadas no fechamento continuam fora dos commits.

## 2026-07-05 — Dependency Cycle Reduction v3

- Eliminado o par mútuo `Farm|Player`: `PlayerController` deixou de consultar
  `FarmPlot.IsAnyActionMenuOpen` e passou a depender do contrato puro `GameplayInputBlocker`.
- `FarmPlotMenuController` adquire uma lease ao abrir e a libera ao fechar/desabilitar. Dispose
  duplicado e leases aninhadas são seguros.
- O gate vive em `CindarsHope.Gameplay.Input`, evitando converter o ciclo removido em `Farm|UI`.
- `FarmPlot.IsAnyActionMenuOpen` foi preservado para consumidores legados.
- Snapshot: 1.572 arquivos C#, 218 edges, 48 pares mútuos, 29 internal, 3 crossings.
- Gates: EditMode 2.707/2.707; PlayMode 2/2; seis assemblies 0E/0W; ratchet PASS.
- Spec promovida para `.specs/implementados/spec_arch_dependency_cycle_reduction_v3.md`.
- Próximo recorte deve escolher um dos pares pequenos restantes e manter uma spec por ciclo.

## 2026-07-05 — Narrative/Quest Cycle Reduction v4

- `NarrativeIds` foi movido para a assembly pura Gameplay e passou ao namespace
  `CindarsHope.Gameplay.Narrative`.
- GUID `0c1174d84fa2a024f860a149754e8754` e as sete strings persistidas foram preservados.
- Consumers Narrative, Quest, World e testes foram atualizados; bootstrap, flags, ordem de oferta e
  save não mudaram.
- O par `Narrative|Quests` foi removido sem novo par mútuo: snapshot 1.573/220/47/29/3.
- Gates: 13/13 narrativa; EditMode 2.707/2.707; PlayMode 2/2; seis assemblies 0E/0W; ratchet PASS.
- Próximo recorte pequeno candidato: `Locations|Player`, mas exige ports de vitais/respawn e deve ser
  tratado em spec própria, sem mover serviços apenas para esconder o ciclo.

## 2026-07-05 — Modularization Residual v5 (EM EXECUÇÃO PELO CODEX)

- Spec ativa: `.specs/a_implementar/spec_arch_modularization_residual_v5.md`.
- Baseline revalidado: NpcShopController 1.050/46; EnemyBrain 925/45; 49 atributos reais de runtime
  init (48 fora do root); root instala 3 serviços + 2 resets; 47 pares mútuos.
- Higiene: o intervalo local do rework adicionou 12 XMLs/141.331 linhas; política será migrar
  resultados brutos para artefatos locais/CI e manter evidência compacta versionada.
- Working tree paralelo a preservar: animação inimiga, ProjectSettings, `.slnx`, ferramenta Python,
  XMLs canon/Claude não rastreados e ferramentas Aseprite.
- Ordem autorizada: higiene → catálogos → shop → brain → bootstraps → ciclos → closeout.
- Regra de continuidade: ao fim de cada lote, registrar neste arquivo commit, arquivos, gates,
  riscos residuais e próximo comando. Não publicar sem autorização explícita.

### V5 Lote 1 — higiene (implementado, commit pendente de conferência)

- Corrigido comentário do `GameplayInputRouter`: o router trata I/K/U/Esc, não C.
- Adicionada política `/TestResults/*.xml` no `.gitignore` e `TestResults/README.md`.
- Removidos do HEAD 48 XMLs brutos rastreados (200.666 linhas / ~23,5 MB no checkout).
- Evidência versionada agora é compacta em `TestResults/MODULARIZATION_VALIDATION_SUMMARY.md` e
  snapshots `.txt`. XML continua gerado localmente/CI.
- Não houve history rewrite: blobs antigos continuam no histórico. Reescrever nove commits locais
  foi rejeitado neste lote por risco ao branch compartilhado/worktree misto.
- Código funcional não mudou; nenhum gate runtime precisa ser refeito por este lote documental.
- Próximo passo: V5 Lote 2, extrair tuning de active skills e IDs/reward builders mantendo todos os
  valores atuais e testes de equivalência.

### V5 Lote 2 — catálogos tipados (implementado, commit pendente de conferência)

- Criado `ActiveSkillExecutorCatalog`: composição/tuning dos 30 efeitos saiu integralmente do
  `ActiveSkillExecutionController`; nenhum número, ID, texto ou executor foi alterado.
- Controller usa `ActiveSkillExecutorCatalog.CreateRegistry()` e caiu de 438 para 365 linhas.
  Teste de cobertura consulta o registry real; a lista espelhada foi removida.
- `CaveContractCatalog` agora possui `NoHitCharmItemId` e builders canônicos de reward IDs.
  `QuestDynamicInstancePersistence` não contém mais os quatro literals/reward formats apontados.
- Testes focados: skill mapping 4/4; cave contracts 20/20.
- Gates: EditMode 2.708/2.708; seis assemblies 0E/0W; architecture ratchet PASS.
- XMLs destes testes foram gerados localmente e ignorados conforme a política do Lote 1.
- Próximo passo: V5 Lote 3, decompor `NpcShopController` preservando seus campos serializados e
  extraindo primeiro choices/services puros antes de transaction/UI wiring.

### V5 Lote 3A — política de choices do shop (commit `31014bb5`)

- Criado `NpcShopDialogueChoicePolicy` no assembly puro `CindarsHope.Gameplay`.
- A política é dona da ordem, rótulos fixos e IDs dos menus raiz e Thalindra; o controller resolve
  apenas gates/estado runtime, adapta para `DialogueChoice` e mantém os handlers existentes.
- Nenhum rótulo, ID, ordem, gate, transação ou handler foi removido.
- Testes da política: 3/3; ratchet do Gameplay: 1/1.
- Builds: `Assembly-CSharp.csproj` e `CindarsHope.Tests.EditMode.csproj`, 0 erros/0 warnings.
- Commit: `31014bb5`.

### V5 Lote 3B — readiness de transação (commit `fe7016cd`)

- Criado `NpcShopTransactionReadinessPolicy`, uma máquina de decisão pura para falha de contexto,
  inicialização do controller, recuperação da sessão, painel inválido ou abertura.
- `NpcShopController` continua executando todos os efeitos Unity, inicializações, logs e abertura de
  `BuyPanel`/`SellPanel`; preços, inventário, wallet, eventos e wiring não mudaram.
- Controller passou de 1.050/46 para 1.007 linhas/44 métodos no recorte completo do Lote 3.
- Testes focados: readiness 9/9; choices 3/3.
- EditMode completo: 2.720/2.720 PASS.
- Seis assemblies explícitos: 6/6, 0 erros/0 warnings.
- Ratchet do assembly Gameplay permanece coberto pelo EditMode completo.
- Lote 3 marcado como concluído na spec residual.
- Próximo passo: V5 Lote 4, decompor `EnemyBrain` por state/targeting/config sem alterar decisões,
  timings, ranges, RNG, animação, dano, drops ou serialização.

### V5 Lote 4 — EnemyBrain (commit `a6f6bef3`)

- A state machine pura existente (`EnemyDecisionCore`) foi preservada como autoridade de decisão.
- Criado `EnemyTargetingController`: resolução do player visível/fallback, rival de conflito,
  distância e direção saíram do MonoBehaviour.
- Criado `EnemyBrainTuningResolver`: precedência exata profile → EnemyData → default para detection,
  leash e move speed ficou centralizada e testável.
- Criado `EnemyBrainConfigurationPolicy`: normalização de `packId` e override positivo do decision
  tick saíram dos caminhos `OnEnable`/`ConfigureRuntime`.
- Nenhum campo serializado, assinatura pública, state, timing, range, multiplicador, RNG, ação,
  animação, dano, drop ou evento foi alterado.
- `EnemyBrain` caiu de 925 para 890 linhas; colaboradores anteriores de movement/action/conflict
  permanecem intactos.
- Testes novos: tuning 2/2 e configuração 4/4 dentro do EditMode completo.
- Gates: EditMode 2.726/2.726; seis assemblies 6/6, 0 erros/0 warnings.
- Próximo passo: V5 Lote 5, inventariar/classificar os 48 runtime-init externos ao root e migrar
  apenas bootstraps persistentes com ownership/teardown inequívocos para installers de domínio.

### V5 Lote 5A — ownership e installers piloto (commit `c85676cf`)

- Inventário autoritativo: `docs/architecture/RUNTIME_BOOTSTRAP_OWNERSHIP_V5.md`.
- A contagem caiu de 49 para 41 atributos reais; 40 continuam fora do root.
- Migrados para ownership central: first-kill crafting, cave conflict feedback, friendship, gifting,
  NPC services, city services, weather e world events.
- Installers adicionados: `CraftingRuntimeInstaller`, `CaveRuntimeInstaller`, `NpcRuntimeInstaller`
  e `WorldRuntimeInstaller` em `DomainRuntimeInstallers.cs`.
- Serviços dependentes da cena são instalados no `Start` do root; novos hosts ficam filhos do root;
  instâncias de cena existentes são adotadas sem reparenting.
- Teardown do hook estático funciona também quando a referência Unity destruída é normalizada; o
  cave bridge usa o lifecycle do filho para unsubscribe.
- Gates: root EditMode 2/2; EditMode completo 2.726/2.726; PlayMode root 1/1; seis assemblies 6/6,
  0 erros/0 warnings; architecture ratchet PASS.
- Lote 5 permanece ABERTO: faltam 24 serviços persistentes, 7 UI/cena, 6 player-attached, 1 debug,
  1 diagnóstico e 1 reset de subsistema classificado como intencional.
- Próximo recorte seguro: NPC schedule + World item drop, depois Farm; não mover UI/player/audio
  diretamente para o estágio `BeforeSceneLoad`.

### V5 Lote 5B — schedule e item drop (commit `e1801721`)

- `NpcScheduleRuntimeBootstrap` e `ItemDropSpawner` perderam seus auto-bootstraps e agora são
  instalados pelos installers NPC/World no `Start` do root.
- Hosts novos são filhos do root; fallback sem owner mantém `DontDestroyOnLoad`; instâncias existentes
  continuam adotadas. O schedule preserva a varredura de anchors/controllers e o item drop preserva
  índice, lista dinâmica, save e dependências.
- Contagem atual: 39 atributos reais, 38 fora do root (baseline v5: 49).
- Gates: EditMode 2.726/2.726; PlayMode root com os 8 serviços migrados 1/1; ratchet PASS.
- Próximo recorte: installers Farm. Lote 5 continua aberto; não marcar a spec como concluída.

### Ponto de retomada exato após `e1801721`

- Branch: `dev`; nenhum push executado nesta sequência.
- Commits v5 desta execução, em ordem:
  - `ce63534e` higiene de XMLs;
  - `0d30bf31` catálogos de skills/rewards;
  - `31014bb5` choices do shop;
  - `fe7016cd` readiness de transação;
  - `a6f6bef3` targeting/tuning/config do EnemyBrain;
  - `c85676cf` installers piloto;
  - `e1801721` schedule e item drops.
- Spec: lotes 1–4 concluídos; lote 5 aberto; lotes 6–7 ainda não iniciados.
- Próxima ação técnica: caracterizar e migrar separadamente os seis bootstraps Farm listados em
  `RUNTIME_BOOTSTRAP_OWNERSHIP_V5.md`, começando pelo daily goal e resource refresh.
- Gate por recorte Farm: testes focados do serviço, EditMode completo, ratchet, PlayMode root e smoke
  FarmScene. Não elevar baseline e não alterar lógica diária, IDs, quantidades, save ou scene YAML.
- Depois do Farm: demais 16 serviços persistentes; player/UI/audio exigem installers de lifecycle
  próprios e não devem ser chamados diretamente no `BeforeSceneLoad`.
- Ao fechar o lote 5, recontar atributos reais, atualizar spec/inventário/resumo e somente então
  iniciar o lote 6 de ciclos residuais.

### V5 Lote 5C — installers Farm (commit `d926f059`)

- Criado `FarmRuntimeInstaller` no estágio `Start` do composition root.
- Migrados seis auto-bootstraps: daily goal, resource refresh, animal registry, forage, shipping e
  farm lots. Todos mantêm `Install(owner)` com fallback `DontDestroyOnLoad` quando não há owner.
- Serviços criados pelo root ficam sob seu transform; serviços já presentes em cena são adotados sem
  reparenting. `FarmLotService` criado pelo bootstrap usa o mesmo owner do bootstrap.
- Não foram alteradas regras diárias, catálogos, IDs, quantidades, save, handlers de escritura ou
  scene YAML.
- Contagem: 49 baseline → 33 atuais; 32 permanecem fora do root.
- Gates: architecture ratchet PASS; EditMode 2.726/2.726; PlayMode composition + smoke
  Farm/Town/Cave 2/2 PASS.
- Próximo recorte: os 16 serviços persistentes restantes, começando por Cave e inventory/items.

### V5 Lote 5D — inventory/items/magic (commit `dea417ce`)

- Criado `ItemRuntimeInstaller` no `Start` do root.
- Migrados: `ItemUseManager`, consumables, magic items/passive tracker, player spellbook/spell item
  controller e crafting station.
- Retry, registro de handlers, passive tracker, scene-loaded rebind e APIs singleton foram mantidos.
- Hosts criados pelo root ficam sob seu transform; fallback standalone mantém `DontDestroyOnLoad`.
- Contagem: 49 baseline → 28 atuais; 27 permanecem fora do root.
- Gates: ratchet PASS; EditMode 2.726/2.726; PlayMode composition + cenas 2/2 PASS.
- Próximo recorte: Cave, telemetry, Fonte, narrative, player condition/inferred class e quest.

### V5 Lote 5E — Fonte e player services (commit `4b6a8767`)

- Criado `PlayerServiceRuntimeInstaller`.
- Migrados `FonteRuntimeBootstrap`, `PlayerConditionRuntimeBootstrap` e
  `InferredClassRuntimeBootstrap`.
- Fonte/condition ficam sob o root; inferred class continua anexada ao `GameBootstrap` somente
  quando ele existe, preservando o gate e sem criar fallback paralelo.
- Configuração de time/player do condition service e buscas de wiring foram preservadas.
- Contagem: 49 baseline → 25 atuais; 24 permanecem fora do root.
- Gates: ratchet + EditMode 2.726/2.726; PlayMode composition/cenas 2/2 PASS.
- Próximo recorte: Cave, telemetry, narrative e quest (8 serviços persistentes).

### Ponto de retomada após `4b6a8767`

- Contagem comprovada: 25 atributos reais; 24 fora do root.
- Oito serviços persistentes ainda no lote 5:
  - Audio: `AudioManager`, `SfxEventBridge` (migrar por último por causa do `AudioListener`);
  - Cave: `DeathSystemBootstrap`, `CaveRuntimeBridge`, `CaveWanderingMerchant`;
  - Combat: `CombatTelemetryService.Bootstrap`;
  - Narrative: `NarrativeRuntimeBootstrap`;
  - Quest: `QuestRuntimeBootstrap`.
- Casos restantes fora desse grupo: 6 player-attached, 7 UI/cena, 1 debug, 1 diagnóstico e o reset
  `SceneTransitionRouter` em `SubsystemRegistration`; não contar esses 16 como serviços globais
  comuns nem movê-los diretamente para `Start`/`BeforeSceneLoad`.
- Últimos gates: EditMode 2.726/2.726; PlayMode composition + Farm/Town/Cave 2/2; ratchet PASS.
- Próximo passo seguro: caracterizar ordem Quest → Narrative e os três hosts Cave; migrar em um
  commit com PlayMode 2/2. Telemetry pode acompanhar Cave se mantiver seu toggle/teardown.
- Branch `dev` está 20 commits à frente de `origin/dev`; nenhum push foi executado.

## 2026-07-05 — Autorização integral das Fases 4 a 8

- Decisão humana: continuar até terminar todo o rework, sem parar entre fases.
- Branch: `dev`; preflight em `0f2ab017`, sincronizado com `origin/dev` (`0 0`).
- Escopo liberado: Fases 4, 5, 6, 7 e 8 do plano autoritativo, sempre pelos gates definidos.
- Restrições mantidas: nenhuma regressão de gameplay/save/cena/conteúdo; commits pequenos; nenhum
  baseline elevado para mascarar falha; performance somente com medição.
- Alterações concorrentes que continuam excluídas dos commits:
  - `Assets/_Game/Scripts/Editor/Enemy/GenerateEnemyWalkAnimations.cs`;
  - `Assets/_Game/Scripts/Enemy/EnemyAnimator.cs`;
  - `tools/enemy_anim/normalize_enemy_sheets.py`.
- Próximo passo: inventariar composition roots e selecionar o primeiro bootstrap migrável da Fase 4.

### 2026-07-05 — Fase 4: composition root piloto

- Criado `GameRuntimeCompositionRoot` com readiness explícito e lifecycle idempotente.
- `CombatStateTrackerBootstrap` migrado de auto-bootstrap para `Install()` centralizado.
- EditMode: 2/2 PASS; compile Unity: exit 0; ratchet: PASS.
- PlayMode batch: bloqueado pelo runner, que entra no jogo sem iniciar o teste ou gerar XML; não
  considerar PASS. Artefatos `InitTestScene*` foram removidos.
- Relatório: `docs/architecture/MODULARIZATION_PHASE4_COMPOSITION_REPORT.md`.

### 2026-07-05 — Fase 5: runtime/editor/tests explícitos

- Assemblies: `CindarsHope.Runtime`, `CindarsHope.Editor`, `CindarsHope.Tests.EditMode` e
  `CindarsHope.Tests.PlayMode.Composition`, todas sobre `CindarsHope.Foundation`.
- O runtime amplo é intencional enquanto os 49 pares mútuos não forem removidos por ports.
- Builder usa `cindars_hope.slnx`; 6 projetos ativos compilam com 0 warnings e 0 erros.
- Arquitetura: 7/7 PASS; save fixtures: 6/6 PASS; ratchet: PASS.
- EditMode real: 2.579/2.660 PASS, 81 falhas antes invisíveis.
- Relatório: `docs/architecture/MODULARIZATION_PHASE5_ASSEMBLIES_REPORT.md`.
- Próximo passo: publicar checkpoints das Fases 4/5 e iniciar Fase 6 sem tratar testes obsoletos
  como autorização para regredir conteúdo atual.

## 2026-07-05 — Autorização e preflight da Fase 3

- Decisão humana: seguir para a próxima fase na branch `dev`.
- Escopo: criar somente a primeira assembly pequena `CindarsHope.Foundation` com tipos puros.
- Preflight: `origin/dev` e `HEAD` em `63abc536`, divergência `0 0`.
- Alterações externas preservadas e excluídas dos commits:
  - `Assets/_Game/Scripts/Editor/Enemy/GenerateEnemyWalkAnimations.cs`;
  - `Assets/_Game/Scripts/Enemy/EnemyAnimator.cs`;
  - `tools/enemy_anim/normalize_enemy_sheets.py`.
- Restrições: preservar namespaces e GUIDs, usar `noEngineReferences`, não alterar gameplay, saves,
  cenas, assets, balanceamento ou conteúdo.
- Gate para encerrar: assembly gerada pelo Unity; todos os projetos compilando; testes de
  arquitetura e save passando; nenhuma falha EditMode nova; nenhuma referência ou script ausente.
- Fase 4: não autorizada.

### 2026-07-05 — Fase 3.1: primeira fronteira Foundation

- Tipos selecionados: `IIdentifiedData` e `IDataRegistry<T>`; ambos são contratos puros baseados
  somente na BCL e tiveram namespaces preservados.
- Movimento: fontes e respectivos `.meta` migrados de `Core/Data` para `Foundation/Data`.
- GUIDs preservados: `319077d6cfe6c8f4192c1c6588d41076` e
  `e17342ebda143d849a3f7f1063674f53`.
- Assembly criada: `CindarsHope.Foundation`, `autoReferenced: true`,
  `noEngineReferences: true`, sem referências explícitas.
- Unity gerou `CindarsHope.Foundation.csproj` e `Library/ScriptAssemblies/CindarsHope.Foundation.dll`.
- Primeiro processo de importação retornou exit 1 pelo wrapper apesar de o log terminar em return
  code 0; a repetição independente após a importação retornou exit 0.
- Builder multi-project: 3 projetos descobertos e compilados, 0 warnings e 0 erros.
- Testes de arquitetura: 7/7 PASS.
- Save fixtures: 6/6 PASS.
- EditMode completa: 69/82 PASS, com as mesmas 13 falhas da Fase 2 e nenhuma nova.
- Ratchet: PASS; 19 GUIDs protegidos; nenhum limite elevado.
- Snapshot: 0 hardcodes, 18 tipos internos, 3 cruzamentos de teste, 206 arestas e 49 pares mútuos.
- Logs: nenhum erro C#, missing script ou `MissingReferenceException`.
- Docs validator: exit 1 somente pelas specs futuras e pelo harness já conhecidos; nenhum erro
  restante pertence aos arquivos da Fase 3.
- Infraestrutura corrigida:
  - `RunUnityEditModeTests.ps1` não usa mais `-quit`, exige XML válido e propaga resultado real;
  - scanners de arquitetura agora resolvem `-ProjectRoot .` antes de calcular paths relativos.
- Relatório: `docs/architecture/MODULARIZATION_PHASE3_FOUNDATION_REPORT.md`.
- Status técnico: `COMPLETE`.
- Commits técnicos: `8e99b658` e `1fcfde72`.
- Publicação técnica: `origin/dev` verificado em
  `1fcfde72f6c174e3da39fcf2cf0a8ed8e1a2c25e`, divergência `0 0`.
- Próximo passo: publicar este fechamento documental e parar antes da Fase 4.

## 2026-07-05 — V5 Lote 5F (batch 2): Cave, Combat Telemetry, Narrative e Quest

- Objetivo: migrar os 6 serviços persistentes de domínio restantes (Cave×3, Combat Telemetry,
  Narrative, Quest) de auto-bootstrap `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]` para o
  composition root, preservando 100% do comportamento observável.
- Arquivos alterados:
  - `Assets/_Game/Scripts/Cave/Death/DeathSystemBootstrap.cs` — `EnsureInstance()` virou
    `public static void Install(Transform owner)`; host passa a ser filho do root (mantém
    `DontDestroyOnLoad`, singleton guard, `BindWhenReady` coroutine inalterados).
  - `Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeBridge.cs` — mesmo padrão; `Install(Transform
    owner)`.
  - `Assets/_Game/Scripts/Cave/Runtime/CaveWanderingMerchant.cs` — é `static class` (não
    MonoBehaviour); `Bootstrap()` virou `public static void Install()` sem parâmetro (o merchant root
    por nível continua nascendo/morrendo fora da hierarquia do composition root — não muda).
  - `Assets/_Game/Scripts/Combat/Telemetry/CombatTelemetryService.cs` — a nested `Bootstrap.EnsureInstance()`
    virou `Bootstrap.Install(Transform owner)`; **o gating do toggle debug (`DebugEnabled` OFF por
    default, `ApplyToggle`/`Subscribe`/`Unsubscribe`) não foi tocado** — só mudou quem chama
    `Install`, a instância continua sempre criada mas sem assinaturas até o toggle ligar.
  - `Assets/_Game/Scripts/Narrative/NarrativeRuntimeBootstrap.cs` — `Install(Transform owner)`.
  - `Assets/_Game/Scripts/Quests/Runtime/QuestRuntimeBootstrap.cs` — `Install(Transform owner)`.
  - `Assets/_Game/Scripts/Composition/DomainRuntimeInstallers.cs` — 4 installers novos:
    `CaveSceneRuntimeInstaller` (os 3 Cave), `CombatTelemetryRuntimeInstaller`,
    `NarrativeRuntimeInstaller`, `QuestRuntimeInstaller`.
  - `Assets/_Game/Scripts/Composition/GameRuntimeCompositionRoot.cs` — `Start()` passou a chamar os 4
    installers novos, na ordem Cave → Combat Telemetry → Narrative → Quest, após os installers
    já existentes (Npc/World/Farm/Item/PlayerService).
  - `docs/architecture/RUNTIME_BOOTSTRAP_OWNERSHIP_V5.md` — contagem 25→19, migração documentada.
- Comportamento preservado: momento de instalação (`AfterSceneLoad` → agora `Start()` do root, que
  roda após a cena carregar, mesmo estágio observável), singleton guards, `DontDestroyOnLoad`,
  coroutines de bind-when-ready (Death system, Narrative, Quest), gating do toggle de
  `CombatTelemetryService`, ordem de dependência Quest→Narrative (Narrative consome
  `QuestRuntimeBootstrap.QuestService` via polling, inalterado), UI controllers do Quest
  (`QuestOfferPanelController`/`QuestLogPanelController`), notice board/cave contracts/secret quests
  do Quest, e o determinismo do `CaveWanderingMerchant` (seeds, stable-run contract).
- Hosts novos (Death, CaveRuntimeBridge, CombatTelemetry, Narrative, Quest) ficam filhos do
  `GameRuntimeCompositionRoot.transform`; `CaveWanderingMerchant` não tem host fixo (spawna por nível,
  sem owner, como já era).
- Validação:
  - `Invoke-UnityGeneratedProjectsBuild.ps1`: exit 0, 7/7 projetos, 0 warnings/0 erros.
  - `RunUnityEditModeTests.ps1` (`TestResults/residual-v5-batch2-editmode.xml`,
    `Logs/residual-v5-batch2.log`): exit 0, 2726/2726 PASS, 0 failed.
  - Contagem `[RuntimeInitializeOnLoadMethod]` em `Assets/_Game/Scripts/**`: 25 → 19 (medido via
    `Get-ChildItem ... | Select-String '^\s*\[RuntimeInitializeOnLoadMethod'`).
  - Ratchet arquitetural (`architecture-ratchet-baseline.tsv`): não precisou de edição — o ratchet é
    per-file com teto (`max_count`); reduzir a contagem de um arquivo para 0 fica sempre abaixo do
    teto. Nenhum arquivo teve sua contagem aumentada.
- Não executado / não aplicável: PlayMode manual não foi rodado nesta sessão (Unity fechado, sem
  Editor interativo disponível); risco residual: comportamento em Play Mode real (spawn do merchant,
  fluxo de morte/corpo, telemetria ligada via toggle) não foi confirmado visualmente nesta sessão —
  mitigado pelo fato de a mudança ser puramente de "quem chama o método estático de instalação",
  sem tocar a lógica interna de nenhum dos 6 serviços.
- Commits (português, um por domínio, sem push):
  - `refactor(bootstrap): centralizar servicos de cave` (DeathSystemBootstrap, CaveRuntimeBridge,
    CaveWanderingMerchant, CaveSceneRuntimeInstaller, root).
  - `refactor(bootstrap): centralizar telemetria de combate` (CombatTelemetryService,
    CombatTelemetryRuntimeInstaller, root).
  - `refactor(bootstrap): centralizar servico de narrativa` (NarrativeRuntimeBootstrap,
    NarrativeRuntimeInstaller, root).
  - `refactor(bootstrap): centralizar servico de quest` (QuestRuntimeBootstrap, QuestRuntimeInstaller,
    root).
  - commit de docs/handoff (este bloco).
  - hashes reais: ver `git log --oneline -6` na branch `dev` no momento do fechamento — não incluídos
    aqui porque os commits foram criados após a escrita deste parágrafo; confirme no Git antes de
    confiar nesta nota.
- Próximo passo: Audio (`AudioManager`, `SfxEventBridge`) exige criar um estágio `AfterSceneLoad`
  explícito no `GameRuntimeCompositionRoot` antes de migrar (instalar Audio cedo demais afeta a
  decisão de criar `AudioListener`). Depois disso: player-lifecycle (precisa de
  `PlayerRuntimeInstaller` ligado a spawn/despawn) e apresentação/UI (7 casos) só com PlayMode +
  smoke visual por cena.

## 2026-07-05 — Início do rework modular (histórico)

Você está continuando o rework modular do projeto Unity **Cindar's Hope**.

## Objetivo

Modularizar o projeto incrementalmente, aplicando DRY, SOLID, Ports and Adapters, Composition Root,
Strategy, State/Presenter, Command/Unit of Work e assemblies explícitas, sem alterar o comportamento
do jogo.

O plano autoritativo desta execução é:

`docs/architecture/MODULARIZATION_REWORK_PLAN.md`

Leia esse arquivo por completo antes de propor ou alterar qualquer código.

## Regra obrigatória de continuidade

Antes de continuar, **valide de forma independente tudo que o Codex afirma ter feito**. Não use este
handoff, mensagens anteriores ou relatórios como prova. Verifique no disco, no Git e nos resultados
reais dos comandos.

No início de toda nova sessão execute:

```powershell
git fetch origin dev
git branch --show-current
git status --short
git log --oneline -12
git rev-list --left-right --count origin/dev...HEAD
```

Pare e reporte se:

- a branch não for a esperada;
- houver alterações não documentadas neste handoff;
- o working tree estiver misturado com arte/conteúdo de outra tarefa;
- commits citados aqui não existirem;
- o código contradizer o plano;
- algum gate de validação produzir resultado diferente do registrado.

## Estado registrado pelo Codex

### Concluído nesta preparação

- Auditoria arquitetural e de performance realizada no working tree de 2026-07-04.
- Decisão humana recebida para iniciar modularização em branch dedicada.
- Plano detalhado criado em `docs/architecture/MODULARIZATION_REWORK_PLAN.md`.
- Este handoff vivo foi criado.
- O baseline completo foi consolidado no commit `a8fec139` e publicado em `origin/dev`.
- O push enviou 904 objetos Git LFS (150 MB) e terminou com exit 0.
- A validação do estado remoto ainda deve ser refeita no início da próxima sessão; não confie apenas
  neste registro.
- Preflight de 2026-07-05 confirmou branch `dev`, working tree limpo, `origin/dev...HEAD = 0 0` e
  HEAD `aed4f35c`.
- O responsável pelo projeto autorizou executar a Fase 0 na própria `dev`; a exceção foi registrada
  no plano e não autoriza iniciar as fases estruturais seguintes na mesma branch automaticamente.

### Ainda não executado

- Branch `rework/modular-architecture` não foi criada; a Fase 0 foi explicitamente autorizada em
  `dev`.
- Nenhum `.asmdef` foi criado por este rework.
- Nenhum código runtime foi refatorado por este rework.
- Nenhuma fachada ou API antiga foi removida.
- Fase 0 foi concluída no commit `8a887244` e publicada em `origin/dev`; valide o hash e a divergência
  no Git antes de confiar neste registro.

## Baseline publicado

- Branch observada: `dev`.
- HEAD observado antes dos novos commits: `f8892874`.
- Commit principal publicado: `a8fec139` (`feat(projeto): consolidar lote pendente e preparar rework modular`).
- O commit principal contém 1.821 arquivos alterados, 84.855 inserções e 1.541 remoções.
- Antes do push, `dev` estava 21 commits à frente de `origin/dev` e sem commits remotos exclusivos
  após fetch.
- O usuário autorizou explicitamente incluir **todas** as alterações pendentes no commit/push de
  baseline.
- O lote pendente contém código, assets, sprites, cenas, geradores, documentação e ferramentas de
  trabalhos anteriores; não atribua tudo ao rework modular.

## Achados que devem ser revalidados

- zero `.asmdef`;
- aproximadamente 51 arquivos com auto-bootstrap;
- referências hardcoded a `Assembly-CSharp` em geradores/validadores;
- `CSharpProjectPostprocessor.OnGeneratedCSProject` com warning `UNT0006`;
- `_blocks` da telemetria nunca incrementado;
- views de HUD com `Update()` sem trabalho útil;
- `OverlapCircleAll` residual em dois executores de skill;
- `GameEventBus.Publish` usando `handlers.ToArray()`;
- dezenas de acessos globais, inputs diretos e mutações distribuídas de inventário/ouro;
- `GameBootstrap` e `SaveManager` como principais hubs de dependência.

## Sequência obrigatória

1. Verificar este handoff contra Git/disco.
2. Confirmar que o baseline foi publicado e que `dev` está sincronizada.
3. Revalidar os artefatos e resultados da Fase 0 registrados abaixo.
4. Não iniciar Fase 1 ou criar `.asmdef` sem nova decisão explícita sobre branch.
5. Não criar `.asmdef` antes de remover dependências hardcoded e atualizar validators.
6. Não começar por Editor/Tests enquanto runtime continuar preso à `Assembly-CSharp`.
7. Migrar Foundation primeiro.
8. Atualizar este handoff após cada mudança material.

## Atualização obrigatória deste arquivo

Após **cada mudança material**, edite este documento antes do próximo commit. Acrescente uma entrada
ao log abaixo contendo:

- data/hora;
- objetivo;
- arquivos alterados;
- comportamento preservado;
- validações executadas e exit codes;
- testes não executados e motivo;
- commit produzido;
- riscos residuais;
- próximo passo exato.

Não declare PASS por compilação textual ou por relato de outro agente. Use exit code real.

## Log de execução

### 2026-07-05 — Autorização e preflight das Fases 1 e 2 na `dev`

- Objetivo: executar toda a Fase 1 e, após seu fechamento, iniciar a Fase 2 sem criar `.asmdef`.
- Decisão humana: manter os dois lotes na branch `dev`.
- Preflight: `git fetch origin dev`, branch `dev`, working tree limpo, divergência `0 0`, HEAD
  `807cee4a`.
- Limites: sem mudanças de balanceamento, conteúdo, save, cenas ou comportamento observável; Fase 2
  somente depois do commit/push da Fase 1.
- Arquivos alterados neste marco:
  - `docs/architecture/MODULARIZATION_REWORK_PLAN.md`;
  - `docs/architecture/CLAUDE_MODULARIZATION_HANDOFF.md`.
- Validação: preflight Git concluído; validações de código ainda pendentes.
- Commit da autorização/preflight: `1de0e1b9` (junto da correção Fase 1.1).
- Próximo passo: auditar as cinco correções locais enumeradas na Fase 1 e definir testes de
  não-regressão para cada uma.

### 2026-07-05 — Fase 1.1: callback de geração de projeto C#

- Objetivo: corrigir a assinatura Unity inválida de `OnGeneratedCSProject` sem mudar o XML gerado.
- Alterações:
  - callback agora retorna `string`, conforme o contrato do Unity;
  - transformação retorna o conteúdo modificado em vez de escrever o `.csproj` diretamente;
  - guards preservam conteúdo vazio/não-editor;
  - três testes cobrem projeto não-editor, inclusão da referência e idempotência.
- Comportamento preservado: `Assembly-CSharp-Editor.csproj` continua recebendo uma única referência
  privada-falsa a `Assembly-CSharp.dll`.
- Validação:
  - `dotnet build Assembly-CSharp-Editor.csproj`: exit 0; warning `UNT0006` eliminado;
  - Unity EditMode `CSharpProjectPostprocessorTests`: 3/3 PASS;
  - evidência: `TestResults/modularization-phase1-postprocessor.xml`.
- Commit: `1de0e1b9` (`fix(editor): corrigir callback de geração do csproj`).
- Próximo passo: commitar esta correção isolada e então corrigir a telemetria de block.

### 2026-07-05 — Fase 1.2: contador de block da telemetria

- Objetivo: fazer `_blocks` representar blocks normais reais, sem alterar dano ou timing de block.
- Alterações:
  - `PlayerNormalBlockEvent` publicado depois da mitigação já existente;
  - `CombatTelemetryService` assina/desassina o evento;
  - `CombatTelemetrySession.RecordBlock` incrementa o contador;
  - gap obsoleto de evento inexistente removido;
  - testes cobrem agregação e payload de dano antes/depois da mitigação.
- Comportamento preservado: fórmula, perfect block, dano final e feedback permanecem inalterados; o
  evento é observacional.
- Validação:
  - builds runtime/editor: exit 0;
  - Unity EditMode `CombatTelemetryPhaseOneTests`: 2/2 PASS;
  - warning de `_blocks` eliminado;
  - evidência: `TestResults/modularization-phase1-telemetry.xml`.
- Alteração concorrente detectada: `Assets/_Game/Scripts/Enemy/EnemyAnimator.cs` passou a conter um
  lote de idle animation durante a execução Unity. Não pertence à Fase 1, não foi revertida e deve
  ser excluída dos commits desta modularização.
- Commit: `bc60bcbb` (`fix(telemetria): contabilizar blocks normais`).
- Próximo passo: commitar somente telemetria/eventos/testes/handoff e preservar o arquivo externo.

### 2026-07-05 — Fase 1.3: `Update()` vazio nas views de HUD

- Objetivo: remover callbacks Unity por frame que apenas chamavam métodos vazios.
- Alterações: `InteractionPromptHudView`, `StatusBarsHudView` e `QuestTrackerHudView` não registram
  mais `Update()`; a API pública `Initialize(GameplayHudViewModel)` foi preservada.
- Comportamento preservado: os métodos removidos não alteravam UI nem estado; inscrição de
  visibilidade e ativação dos GameObjects permanecem iguais.
- Teste: reflexão exige ausência de `Update` e presença de `Initialize` nas três views.
- Validação:
  - builds runtime/editor: exit 0;
  - Unity EditMode `HudPhaseOneTests`: 3/3 PASS;
  - evidência: `TestResults/modularization-phase1-hud.xml`.
- Alterações concorrentes preservadas e excluídas do commit:
  - `Assets/_Game/Scripts/Enemy/EnemyAnimator.cs`;
  - `Assets/_Game/Scripts/Editor/Enemy/GenerateEnemyWalkAnimations.cs`;
  - `tools/enemy_anim/normalize_enemy_sheets.py`.
- Commit: `436b4063` (`perf(hud): remover updates vazios das views`).
- Próximo passo: commitar o lote de HUD isoladamente e então substituir `OverlapCircleAll`.

### 2026-07-05 — Fase 1.4: queries circulares sem alocação recorrente

- Objetivo: remover os dois `Physics2D.OverlapCircleAll` residuais dos executores de skill.
- Alterações:
  - `Physics2DOverlapBuffer` reutilizável, com crescimento somente quando a capacidade é atingida;
  - melee strike e slow field usam `ContactFilter2D.noFilter`, preservando layers/triggers;
  - loops indexados substituem arrays alocados por execução;
  - teste cria 40 colliders, força crescimento a partir de capacidade 4 e confirma reutilização.
- Comportamento preservado: centro, raio, filtro amplo e processamento de todos os colliders são os
  mesmos; o buffer cresce para não truncar resultados.
- Validação:
  - runtime build: exit 0;
  - ratchet: exit 0, `PhysicsAllQuery` caiu de 2 para 0;
  - Unity EditMode `Physics2DOverlapBufferTests`: 1/1 PASS;
  - evidência: `TestResults/modularization-phase1-physics.xml`.
- Commit: `3672dd28` (`perf(skills): reutilizar buffer nas queries circulares`).
- Próximo passo: commitar somente helper/executores/teste/evidência/handoff e preservar o lote
  concorrente de animação.

### 2026-07-05 — Fase 1.5: warnings e comentários obsoletos

- Objetivo: zerar warnings conhecidos do lote sem suprimir diagnósticos.
- Alterações:
  - DTOs privados lidos por `JsonUtility` receberam defaults explícitos idênticos aos defaults CLR;
  - campos de gerador nunca configurados foram substituídos pelos mesmos defaults diretamente no
    asset gerado (`MinRange = 0`, `RequiresLineOfSight = false`);
  - comentários que ainda citavam `OverlapCircleAll` foram atualizados para o buffer atual.
- Comportamento preservado: valores produzidos e fallback JSON permanecem idênticos.
- Validação:
  - `dotnet build Assembly-CSharp.csproj`: exit 0, 0 warnings, 0 erros;
  - `dotnet build Assembly-CSharp-Editor.csproj`: exit 0, 0 warnings, 0 erros.
- Commit: `416a6d87` (`chore(codigo): eliminar warnings e comentarios obsoletos`).
- Próximo passo: commitar apenas os cinco arquivos de warning/comentário e o handoff, preservando o
  lote concorrente de animação.

### 2026-07-05 — Fechamento e publicação da Fase 1

- Status: `COMPLETE`.
- Commits da fase: `1de0e1b9`, `bc60bcbb`, `436b4063`, `3672dd28`, `416a6d87`.
- Builds finais: runtime/editor exit 0, 0 warnings, 0 erros.
- Ratchet: PASS; `PhysicsAllQuery` reduziu de 2 para 0.
- EditMode completa: 80 testes, 67 PASS, 13 FAIL; falhas adicionadas = 0, removidas = 0 em relação
  ao baseline pré-modularização.
- Docs validator: exit 1 pelas mesmas dívidas preexistentes de specs/harness.
- PlayMode: não executado; risco residual observacional documentado no relatório.
- Relatório: `docs/architecture/MODULARIZATION_PHASE1_REPORT.md`.
- Alterações concorrentes de animação permanecem fora dos commits.
- Commit de fechamento: `1248bb3c` (`docs(arquitetura): fechar validacao da fase 1`).
- Publicação: `origin/dev` verificado em `1248bb3c79e7376790236c48757615c5b46051e1`, divergência
  `0 0`.
- Próximo passo: iniciar a Fase 2 sem `.asmdef`, mantendo os três arquivos concorrentes fora dos
  commits da modularização.

### 2026-07-05 — Fase 2.1: remover acoplamento C# ao nome da assembly predefinida

- Objetivo: remover resolução de tipos/projetos por nome fixo de assembly no código C#.
- Alterações em andamento:
  - gerador da Farm usa tipos concretos para dash, movement ability e active skill controller;
  - validador WAVE17 busca tipos nas assemblies carregadas;
  - teste social filtra tipos pelo namespace `CindarsHope`, não pelo nome da assembly;
  - postprocessor deriva runtime/editor pelo nome do projeto recebido;
  - teste arquitetural impede reintrodução do nome predefinido em `Scripts/**` e `Tests/**`;
  - comentários dependentes do nome antigo foram generalizados.
- `.asmdef`: nenhum criado.
- Validação:
  - runtime/editor builds: exit 0, 0 warnings, 0 erros;
  - ratchet CLI: PASS;
  - busca `Assembly-CSharp` em C# de `Scripts/**` e `Tests/**`: zero ocorrências;
  - Unity EditMode `ArchitectureRatchetTests`: 3/3 PASS;
  - Unity EditMode `CSharpProjectPostprocessorTests`: 3/3 PASS;
  - evidências: `TestResults/modularization-phase2-csharp-architecture.xml` e
    `TestResults/modularization-phase2-postprocessor.xml`.
- Alterações concorrentes de animação continuam preservadas e fora deste lote.
- Commit: `19b34573` (`refactor(arquitetura): remover nomes fixos de assembly do CSharp`).
- Próximo passo: commitar o lote C# e depois tornar os scripts de build/hook multi-project.

### 2026-07-05 — Fase 2.2: validação multi-project

- Objetivo: impedir que os gates assumam exatamente dois `.csproj` gerados pelo Unity.
- Alterações em andamento:
  - novo `Invoke-UnityGeneratedProjectsBuild.ps1` descobre, restaura e compila todos os `.csproj` da
    raiz com exit codes reais;
  - `run_strict_validation.ps1` usa o builder descoberto em um único gate;
  - hook `check-csproj-includes.ps1` aceita o arquivo C# em qualquer projeto gerado;
  - scanner de logs não trata o simples nome de uma assembly como erro crítico.
- `.asmdef`: nenhum criado.
- Validação:
  - builder descobriu 2 projetos, restaurou e compilou ambos: exit 0, 0 warnings, 0 erros;
  - hook confirmou todos os C# alterados presentes em um dos 2 projetos: exit 0;
  - scanner processou log Unity válido contendo nomes de assemblies: exit 0, sem falso positivo.
- Commit: `12c067db` (`build(unity): descobrir e validar projetos gerados`).
- Próximo passo: commitar ferramentas/hook/handoff e então produzir os mapas de `internal`, ciclos e
  referências permitidas exigidos pelo restante da Fase 2.

### 2026-07-05 — Fase 2.3: mapa pré-asmdef

- Objetivo: fechar o mapa de acessos internos, ciclos e referências permitidas antes da Fase 3.
- Artefatos:
  - `tools/architecture/Get-ModularizationDependencySnapshot.ps1`;
  - `docs/architecture/MODULARIZATION_PHASE2_DEPENDENCY_MAP.md`.
- Snapshot inicial: 0 hardcodes em C#, 18 tipos internos, 2 tipos internos realmente consumidos por
  testes, 206 arestas lexicais e 49 pares de dependência mútua.
- Decisão: não criar `.asmdef` por pasta atual; primeira assembly será uma Foundation curada.
- `.asmdef`: nenhum criado.
- Validação:
  - dependency snapshot: exit 0;
  - builder multi-project: 2 projetos, exit 0, 0 warnings, 0 erros;
  - architecture ratchet: PASS;
  - Unity EditMode completa: 81 testes, 68 PASS, 13 FAIL;
  - comparação com baseline: falhas adicionadas = 0, removidas = 0;
  - `.asmdef`: 0.
- Commit: pendente neste registro.
- Próximo passo: commitar mapa/script/evidência/handoff e publicar a Fase 2. Não iniciar Fase 3 sem
  nova autorização explícita.

### 2026-07-05 — Fechamento e publicação da Fase 2

- Status: `COMPLETE`.
- Commits técnicos: `19b34573`, `12c067db`.
- Hardcode da assembly predefinida em C#: zero.
- Projetos descobertos/compilados: 2/2, 0 warnings, 0 erros.
- Mapa: 18 tipos internos, 2 tipos consumidos por 3 testes, 206 arestas, 49 pares mútuos.
- EditMode: 68/81 PASS; somente as mesmas 13 falhas preexistentes.
- Alterações concorrentes de animação permanecem fora dos commits.
- Commit de fechamento: `02a26d75` (`docs(arquitetura): fechar preparacao pre-asmdef da fase 2`).
- Publicação: `origin/dev` verificado em `02a26d751a741b65515779a230c058305617943f`, divergência
  `0 0`.
- Próximo passo: parar antes da Fase 3 e aguardar autorização explícita para criar a primeira
  `.asmdef`.

### 2026-07-05 — Início controlado da Fase 0 na `dev`

- Objetivo: fechar baseline e proteção arquitetural sem alterar gameplay.
- Preflight: `git fetch origin dev`, branch `dev`, working tree limpo, divergência `0 0`, HEAD
  `aed4f35c`.
- Decisão humana: executar a Fase 0 na branch atual; fases de assemblies/composition continuam fora
  deste escopo.
- Arquivos alterados neste marco:
  - `docs/architecture/MODULARIZATION_REWORK_PLAN.md`;
  - `docs/architecture/CLAUDE_MODULARIZATION_HANDOFF.md`.
- Comportamento preservado: nenhuma alteração runtime, de cena, asset, save ou gameplay.
- Validação deste marco: preflight Git concluído; builds/testes serão executados após os artefatos da
  Fase 0 estarem implementados.
- Commit: pendente até o fechamento do primeiro lote documental/técnico.
- Implementado após o preflight:
  - baseline detalhado em `docs/architecture/MODULARIZATION_PHASE0_BASELINE.md`;
  - dez regras de ratchet compartilhadas por CLI e EditMode;
  - limites por arquivo para impedir aumento da dívida arquitetural;
  - baseline de path/GUID para 16 arquivos Unity sensíveis;
  - cinco fixtures de save, v1 a v5;
  - testes EditMode de ratchet, GUIDs e migração/round-trip de save.
- Ratchet CLI: exit 0; contagens atuais iguais ao baseline registrado.
- Comportamento preservado: nenhum arquivo runtime, cena, prefab, asset ou `ProjectSettings` foi
  alterado.
- Validações concluídas:
  - ratchet CLI: exit 0; dez regras dentro dos limites e 16 GUIDs preservados;
  - runtime build: exit 0, 5 warnings preexistentes;
  - editor build: exit 0, 7 warnings preexistentes;
  - EditMode arquitetura/GUID: 2/2 PASS;
  - EditMode save fixtures: 6/6 PASS;
  - EditMode completa: 58/71 PASS, com as mesmas 13 falhas anteriores; zero falhas novas;
  - docs validator: exit 1 apenas pelas dívidas já registradas de specs/harness;
  - PlayMode: não executado porque o lote altera somente testes, ferramentas e documentação.
- Observação de ambiente: após o Unity encerrar e limpar `Temp/obj`, builds `--no-restore` retornam
  `NETSDK1004`; com restore habilitado, runtime e editor passaram.
- Commit técnico/documental: `8a887244` (`test(arquitetura): fechar baseline e ratchets da fase 0`).

---

## 2026-07-08 — Corte `Save|UI` (spec_arch_save_ui_cycle_reduction_v26)

- Objetivo: quebrar o par mútuo `Save|UI` (`docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md`,
  Tier 2), preservando `UI -> Save` (layering correto) e cortando as duas sub-arestas de
  `Save -> UI`.
- Passo 0 (verificado no disco): grep de `using CindarsHope.UI` em `Save/**` confirmou exatamente
  6 arquivos, cobertos por duas sub-arestas — tipos de Hotbar (`HotbarState`/`HotbarSaveData`,
  puros) e o provider `OnboardingHintsSectionProvider`. Nenhuma outra fonte de `Save -> UI` foi
  encontrada.
- Arquivos alterados:
  - `Assets/_Game/Scripts/UI/Hotbar/{HotbarState,HotbarSaveData}.cs` → `git mv` para
    `Assets/_Game/Scripts/Foundation/{HotbarState,HotbarSaveData}.cs`; namespace
    `CindarsHope.UI.Hotbar` → `CindarsHope.Foundation`.
  - `Assets/_Game/Scripts/Save/{SaveData.cs,SaveManager.cs,SaveManager.Migration.cs,
    Providers/HotbarSectionProvider.cs,Providers/InventorySectionProvider.cs}` — `using
    CindarsHope.UI.Hotbar;` trocado por `using CindarsHope.Foundation;` (ou removido quando já
    presente).
  - `Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs` — referência fully-qualified inline
    `CindarsHope.UI.Hotbar.HotbarState.SlotCount` atualizada para
    `CindarsHope.Foundation.HotbarState.SlotCount`.
  - `Assets/_Game/Scripts/Save/Providers/OnboardingHintsSectionProvider.cs` → `git mv` para
    `Assets/_Game/Scripts/UI/Onboarding/Save/OnboardingHintsSectionProvider.cs`; namespace
    `CindarsHope.Save.Providers` → `CindarsHope.UI.Onboarding.Save`. `Save/SaveManager.cs`
    passou a construir `new CindarsHope.UI.Onboarding.Save.OnboardingHintsSectionProvider()` por
    nome totalmente qualificado (mesma técnica de `spec_arch_quests_save_cycle_reduction_v24`),
    sem novo `using CindarsHope.UI` de topo.
  - `Assets/_Game/Tests/EditMode/Architecture/Editor/ArchitectureRatchetTests.cs` — allowlist do
    `FoundationAssembly_ContainsOnlyTheCuratedPureContracts` ganhou `HotbarState.cs`/
    `HotbarSaveData.cs`.
  - `Assets/_Game/Tests/EditMode/Save/Editor/SaveProviderRegistryTests.cs`,
    `Assets/_Game/Tests/EditMode/Save/SaveSectionProviderTests.cs`,
    `Assets/_Game/Tests/EditMode/UI/OnboardingHintTests.cs` — `using` atualizados para
    `CindarsHope.Foundation`/`CindarsHope.UI.Onboarding.Save`.
  - `CindarsHope.Foundation.csproj`/`CindarsHope.Runtime.csproj` — `<Compile Include>` movidos
    entre projetos conforme os `git mv` acima (Unity, disponível nesta sessão, regenerou ambos os
    `.csproj` de forma consistente ao rodar os testes).
- Comportamento preservado: nenhum schema/campo/valor/nome de classe alterado; nenhuma
  cena/prefab/asset tocado.
- Validação:
  - `tools/architecture/Get-ModularizationDependencySnapshot.ps1`: `MutualModulePairs` 32 → 31,
    `Save|UI` removido, nenhum par novo apareceu.
  - `tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1`: exit 0, 7/7 projetos, 0 warnings/0 erros
    (rodado duas vezes — antes e depois da regeneração automática dos `.csproj` pelo Unity).
  - `tools/unity/RunUnityEditModeTests.ps1`
    (`TestResults/cut-save-ui-editmode.xml`, `Logs/cut-save-ui.log`): exit 0, 2747/2747 PASS.
- Não executado / não aplicável: PlayMode manual não foi rodado nesta sessão; risco residual:
  mitigado porque a mudança é puramente de namespace/localização de arquivo (JsonUtility
  serializa por nome de campo, GUID preservado via `git mv` do `.meta`), sem alterar lógica.
- Próximo passo: seguir o Tier 2/3 do `docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md` para os
  31 pares mútuos restantes.
- Publicação: `git push origin dev`, exit 0; verificação posterior confirmou HEAD e `origin/dev` em
  `8a8872445230ccf58c09b446fe3842741a945151`, divergência `0 0`.
- Risco residual: as 13 falhas EditMode e os warnings preexistentes permanecem fora do escopo.
- Próximo passo: não repetir a Fase 0. Antes da Fase 1, decidir a branch, revalidar o baseline e
  escolher uma correção local isolada com teste próprio.

### 2026-07-04 — Preparação do rework pelo Codex

- Objetivo: documentar a modularização e preparar o baseline para publicação.
- Arquivos criados:
  - `docs/architecture/MODULARIZATION_REWORK_PLAN.md`;
  - `docs/architecture/CLAUDE_MODULARIZATION_HANDOFF.md`.
- Código runtime alterado pelo rework: nenhum.
- Validações executadas:
  - `dotnet build Assembly-CSharp.csproj`: exit 0, 0 erros, 0 warnings;
  - `dotnet build Assembly-CSharp-Editor.csproj`: exit 0, 0 erros, 0 warnings;
  - `tools/docs/validate_docs.ps1`: exit 1 por dívidas já presentes no lote de trabalho
    (`spec_enemy_attack_kits_v1`, specs legadas de NPC/Town e falsos positivos de placeholder em
    `Generate-CodexHarness.ps1`). Os dois documentos novos não apareceram entre as falhas.
  - Unity Test Runner EditMode em batch: execução concluída com exit 2; 63 testes descobertos,
    50 passaram e 13 falharam. Resultado salvo em
    `TestResults/baseline-before-modularization.xml`.
  - Falhas EditMode observadas: 7 contratos de layout da cidade, 2 testes do
    `ProjectValidationRunner` sem `LogAssert.Expect` e 4 contratos de catálogo de itens/flechas.
    Não corrigir essas falhas dentro de um commit de modularização sem spec/escopo próprio.
- Verificação adicional: `git diff --cached --check` apontou whitespace em arquivos `.meta` gerados
  pelo Unity. Isso foi registrado como ruído preexistente/gerado e não foi normalizado mecanicamente
  neste lote para evitar alterar milhares de metadados sem validação do Editor.
- Commit principal: `a8fec139`.
- Publicação principal: `git push -u origin dev`, exit 0; `a8fec139` enviado para `origin/dev`.
- Risco residual: working tree contém um lote grande de trabalhos anteriores autorizado para commit.
- Próximo passo histórico supersedido pela decisão de 2026-07-05 e pelo marco acima.

## Gates mínimos por mudança

```powershell
dotnet build .\Assembly-CSharp.csproj
dotnet build .\Assembly-CSharp-Editor.csproj
```

Além disso:

- rodar EditMode tests relevantes;
- executar validators afetados;
- criar/rodar PlayMode quando lifecycle, bootstrap, cena, UI ou gameplay observável mudar;
- verificar GUIDs antes/depois de mover/deletar scripts;
- verificar fixtures de save quando tipos, providers ou ordem de restore mudarem;
- usar profiler antes/depois de alegar melhoria de performance.

## Limites absolutos

- Não usar `git reset --hard` ou checkout destrutivo.
- Não apagar alterações do usuário.
- Não editar YAML Unity manualmente.
- Não alterar gameplay junto com arquitetura.
- Não renomear campo serializado sem migration Unity.
- Não mudar IDs, quantidades, balanceamento ou starter items.
- Não promover spec sem evidência exigida.
- Não realizar migração big-bang.

Ao final de cada sessão, deixe este arquivo suficiente para que outra sessão consiga continuar sem
depender do histórico da conversa.

## 2026-07-05 — Fechamento das Fases 6 a 8

- Status do código: `COMPLETE` para os gates compilados, EditMode, save e standalone.
- Commit técnico: `a21cbf0a` (`refactor(arquitetura): desacoplar runtime e medir hotspots`).
- Fase 6:
  - Strategy injetável em `EnemyActionRunner`;
  - `IGameClock` e RNG por finalidade em Foundation;
  - pause coordenado por tokens;
  - compra atômica inventory/wallet;
  - registry tipado de providers de save, com hotbar como piloto;
  - correções confirmadas de anchor IDs e preço 90%.
- Fase 7:
  - markers em EventBus, schedule, cave, save, minimap e scene transition;
  - EventBus com snapshot por mutation e 0 bytes em 1.000 dispatches aquecidos.
- Fase 8:
  - regra `unity-architecture` sincronizada em `.codex` e `.claude`;
  - pipeline Canvas/projection existente reconhecido como canônico;
  - nenhum legado apagado sem o gate PlayMode.
- Validação:
  - 6/6 projetos Unity compilam, 0 warnings, 0 erros;
  - ratchets 4/4, save fixtures 6/6;
  - EditMode 2.593/2.672 PASS, 79 falhas; baseline 81, falhas novas 0, removidas 2;
  - build Windows `Succeeded`, 0 erros, 4 warnings;
  - executável vivo após 12 s, `Player.log` sem erro crítico.
  - scanner Editor abriu Farm/Town/Cave e encontrou 0 missing scripts.
- PlayMode corrigido:
  - causa: `PlayModeStartSceneSetter` forçava Farm e substituía a `InitTestScene` do runner;
  - guard `-runTests` coberto por EditMode 2/2;
  - composition + smoke Farm/Town/Cave: PlayMode 2/2 PASS;
  - cada cena foi carregada e sua hierarquia varrida por missing scripts;
  - evidências: `modularization-phase8-playmode-start-guard.xml` e
    `modularization-phase8-playmode.xml`.
- Mudanças concorrentes a preservar fora dos commits:
  - `Assets/_Game/Scripts/Editor/Enemy/GenerateEnemyWalkAnimations.cs`;
  - `Assets/_Game/Scripts/Enemy/EnemyAnimator.cs`;
  - `tools/enemy_anim/normalize_enemy_sheets.py`.
- Também não absorver ruído de Unity em `ProjectSettings/*.asset` ou reorder de `.slnx`.
- Relatórios: `MODULARIZATION_PHASE6_DECOUPLING_REPORT.md`,
  `MODULARIZATION_PHASE7_PERFORMANCE_REPORT.md`, `MODULARIZATION_PHASE8_UI_LEGACY_REPORT.md`.
- Próximo passo de outra sessão: tratar as 79 falhas de conteúdo/spec por lotes próprios; não elevar
  baseline. Remoções de UI antiga ainda exigem spec visual por tela, apesar do smoke de cenas passar.

---

## 2026-07-06 — V5 Lote 5F (batch 3): componentes anexados ao player

- Objetivo: migrar os 6 serviços player-attached restantes de auto-bootstrap
  `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]` para o composition root, seguindo exatamente o
  padrão do batch 2, sem alterar comportamento observável.
- Arquivos alterados:
  - `Assets/_Game/Scripts/Combat/StatusEffect/PlayerStatusReceiver.cs` —
    `PlayerStatusReceiverBootstrap.EnsureInstance()` virou `public static void Install(Transform
    owner)`; o `owner` é ignorado porque o attach original é `AddComponent` no GameObject do
    `Player.StatusEffectManager` já existente na cena (não cria host novo).
  - `Assets/_Game/Scripts/Player/Death/AnyaFountainRespawnFlow.cs` — `EnsureInstance()` virou
    `Install(Transform owner)`; cria `new GameObject("AnyaFountainRespawnFlow")` standalone com
    `DontDestroyOnLoad`; adicionado `go.transform.SetParent(owner)` logo após a criação, no mesmo
    padrão do batch 2.
  - `Assets/_Game/Scripts/Player/Death/PlayerDeathController.cs` — mesmo padrão de
    `AnyaFountainRespawnFlow`: host novo `new GameObject("PlayerDeathController")` +
    `SetParent(owner)` + `DontDestroyOnLoad`.
  - `Assets/_Game/Scripts/Player/Movement/PlayerMovementActionRuntimeBootstrap.cs` — mesmo padrão:
    host novo `new GameObject("PlayerMovementActionRuntimeBootstrap")` + `SetParent(owner)` +
    `DontDestroyOnLoad`.
  - `Assets/_Game/Scripts/Player/Movement/PlayerSprintController.cs` —
    `PlayerSprintControllerBootstrap.EnsureInstance()` virou `Install(Transform owner)`; `owner`
    ignorado porque o attach original é `AddComponent` no GameObject do
    `PlayerController.ActiveInstance` já existente (não cria host novo).
  - `Assets/_Game/Scripts/Player/PlayerVitalsApplier.cs` —
    `PlayerVitalsApplierBootstrap.EnsureInstance()` virou `Install(Transform owner)`; `owner`
    ignorado porque o attach original é `AddComponent` no GameObject do `GameBootstrap` já existente
    (não cria host novo).
  - `Assets/_Game/Scripts/Composition/DomainRuntimeInstallers.cs` — novo
    `PlayerLifecycleRuntimeInstaller.Install(Transform owner)`, chamando os 6 `Install(owner)` na
    mesma ordem listada acima.
  - `Assets/_Game/Scripts/Composition/GameRuntimeCompositionRoot.cs` — `Start()` passou a chamar
    `PlayerLifecycleRuntimeInstaller.Install(transform)` por último, após os installers já
    existentes (Npc/World/Farm/Item/PlayerService/Cave/CombatTelemetry/Narrative/Quest).
  - `docs/architecture/RUNTIME_BOOTSTRAP_OWNERSHIP_V5.md` — contagem 19→13, classe "Componentes
    anexados ao player" zerada, migração documentada.
- Comportamento preservado: em cada um dos 6 serviços, a re-vinculação per-scene (coroutines
  `BindWhenReady`, `SceneManager.sceneLoaded`, guards de singleton `_instance`) permanece 100%
  intacta — só mudou quem chama a criação inicial do host. Nos 3 casos que criam host novo
  (`AnyaFountainRespawnFlow`, `PlayerDeathController`, `PlayerMovementActionRuntimeBootstrap`), o
  novo GameObject vira filho do `GameRuntimeCompositionRoot.transform`, mas continua
  `DontDestroyOnLoad` e com o mesmo singleton guard. Nos 3 casos que fazem `AddComponent` num
  GameObject já existente (`PlayerStatusReceiver`, `PlayerSprintController`,
  `PlayerVitalsApplier`), nenhum host novo é criado e nenhum `SetParent` é chamado — o `owner`
  passado pelo installer é recebido no parâmetro mas ignorado no corpo do método, exatamente
  reproduzindo o `EnsureInstance()` original.
- Validação:
  - `Invoke-UnityGeneratedProjectsBuild.ps1`: exit 0, 7/7 projetos, 0 warnings/0 erros.
  - `RunUnityEditModeTests.ps1` (`TestResults/residual-v5-batch3-editmode.xml`,
    `Logs/residual-v5-batch3.log`): exit code do runner 1 (2 testes falharam de 2.745); as 2 falhas
    são `CindarsHope.Tests.EditMode.Cave.CaveDecorClusterTests.Build_FloorClusterPlacements_HaveNoWallNeighbor`
    e `...Build_FloorClusters_FormGroupsOfTwoToFourAdjacentCells`, ambos parte da spec concorrente
    `spec_cave_decor_composition_runtime` (arquivos `Cave/Ecosystem/CaveDecorContextClassifier.cs`,
    `Cave/Ecosystem/CaveDecorPlacementContext.cs` e outros já modificados no working tree por outra
    sessão, fora do escopo deste lote). Confirmado por `git diff --stat` restrito aos 8 arquivos
    deste lote: nenhum toca `Cave/**` ou o arquivo de teste. Nenhuma falha nova foi introduzida pela
    migração de bootstrap; risco residual: as 2 falhas pré-existentes de outra spec continuam
    abertas e não foram meu escopo corrigir.
  - Contagem `[RuntimeInitializeOnLoadMethod]` em `Assets/_Game/Scripts/**`: 19 → 13 (medido via
    `Get-ChildItem ... | Select-String '^\s*\[RuntimeInitializeOnLoadMethod'`), conforme esperado.
- Não executado / não aplicável: PlayMode manual não foi rodado nesta sessão (Unity fechado, sem
  Editor interativo disponível); risco residual: comportamento em Play Mode real (dash/dodge/block
  do player, sprint, status effects, morte/respawn na Fonte, vitals) não foi confirmado visualmente
  nesta sessão — mitigado pelo fato de a mudança ser puramente de "quem chama o método estático de
  instalação e, nos 3 casos com host novo, quem é o pai do GameObject", sem tocar a lógica interna
  de nenhum dos 6 serviços.
- Commits (português, sem push): ver `git log --oneline -3` na branch `dev` no momento do fechamento
  para os hashes reais — não incluídos aqui porque foram criados após a escrita deste parágrafo.
- Próximo passo: apresentação/UI (×7 casos: `IntroSequenceController`,
  `CharacterEquipmentPanelController`, `DeathScreenCanvasController`, `GameplayHudBootstrap`,
  `InventoryPanelController`, `SkillTreeGameplayPanelController`, `SceneFadeOverlayBootstrap`) só com
  PlayMode + smoke visual por cena; depois disso, Audio (`AudioManager`, `SfxEventBridge`) exige criar
  primeiro um estágio `AfterSceneLoad` explícito no `GameRuntimeCompositionRoot`.

---
