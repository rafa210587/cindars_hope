# /speckit.specify — Modularization Residual v5

Status: `APPROVED_BY_HUMAN_IN_EXECUTION`

## Ordem de execucao

ARCH.RUNTIME.V5

## Depende de

- `.specs/implementados/spec_arch_runtime_maintainability_rework_v2.md`
- `.specs/implementados/spec_arch_dependency_cycle_reduction_v3.md`
- `.specs/implementados/spec_arch_narrative_quest_cycle_reduction_v4.md`

## Bloqueia

- promoção da modularização ampla como concluída;
- novos self-bootstraps persistentes sem ownership/teardown;
- novos magic values de skills fora do catálogo canônico;
- novos XMLs brutos de Test Runner versionados.

required_adrs: []
required_game_rules: [input_rules, save_rules, stable_id_rules, event_rules]

## Objetivo

Fechar as ressalvas residuais verificadas em 2026-07-05 sem alterar gameplay, saves, IDs, cenas,
prefabs, preços, quantidades, diálogos ou balanceamento.

## Baseline verificado

- `NpcShopController`: 1.050 linhas / 46 métodos;
- `EnemyBrain`: 925 linhas / 45 métodos;
- 49 atributos reais `RuntimeInitializeOnLoadMethod`, 48 fora do composition root;
- composition root instala 3 serviços e executa 2 resets;
- 47 pares mútuos no dependency snapshot;
- EditMode 2.707/2.707; PlayMode 2/2;
- 12 XMLs adicionados pelo rework local, 141.331 linhas; 48 XMLs rastreados no total;
- branch `dev` nove commits à frente de `origin/dev` no início desta spec;
- working tree contém mudanças paralelas de arte/animação/ProjectSettings/tools, fora do escopo.

## Não regressão

- valores atuais são canônicos e devem ser preservados byte-for-byte quando virarem catálogo;
- MonoBehaviours mantêm campos serializados e APIs externas durante extrações;
- bootstraps só migram após classificação de ownership, ordem, teardown e fallback;
- cada ciclo removido precisa de snapshot antes/depois e não pode criar ciclo substituto;
- mudanças paralelas nunca entram nos commits desta spec.

# /speckit.plan

1. higiene de artefatos e comentário;
2. catálogos tipados para skill tuning e reward IDs;
3. decomposição do shop por collaborators puros;
4. decomposição do brain por state/targeting/config collaborators;
5. installers de domínio para self-bootstraps persistentes;
6. ciclos pequenos restantes;
7. gates e closeout.

# /speckit.tasks

- [x] Lote 1 — higiene de TestResults/comentário;
- [x] Lote 2 — catálogos tipados;
- [x] Lote 3 — NpcShopController;
- [x] Lote 4 — EnemyBrain;
- [x] Lote 5 — bootstrap ownership/installers (49→3 `RuntimeInitializeOnLoadMethod`; installers de domínio/player/apresentação/audio/diagnóstico no `GameRuntimeCompositionRoot`; os 3 restantes ficam por design/spec própria — ver `docs/architecture/RUNTIME_BOOTSTRAP_OWNERSHIP_V5.md`; verificado build 7/7, EditMode 2747/2747, PlayMode 2/2);
- [x] Lote 6 — ciclos residuais selecionados;
- [x] Lote 7 — validação e closeout.

## Evidência de closeout — 2026-07-06

Status final: `IMPLEMENTED_BUILD_VALIDATED`.

### Lote 6 — ciclos selecionados

Snapshot inicial do lote:

- `MutualModulePairs=47`;
- `RuntimeModuleEdges=235`;
- `InternalTypes=44`;
- `InternalTypeTestCrossings=3` (todos Cave preexistentes).

Pares removidos, um por commit:

1. `Farm|Interaction` — `InteractionSystem` deixou de consultar `FarmPlot.IsAnyActionMenuOpen` e
   passou a consultar o contrato puro já existente `GameplayInputBlocker.IsBlockedBy(FarmActionMenu)`.
   O menu da fazenda já adquiria essa lease; comportamento preservado. Commit:
   `f5f4ffb9 refactor(arquitetura): desacoplar interacao da fazenda`.
2. `Quests|UI` — criação dos controllers de quest UI saiu de `QuestRuntimeBootstrap` e passou para
   `PresentationRuntimeInstaller`, com `Install(Transform owner)` em `QuestOfferPanelController` e
   `QuestLogPanelController`. O bootstrap de quest ficou dono apenas do domínio/serviço de quest.
   Commit: `b2a2e399 refactor(arquitetura): separar ui de quests do bootstrap`.

Snapshot final do lote:

- `MutualModulePairs=45`;
- `RuntimeModuleEdges=234`;
- nenhum par novo apareceu;
- `Farm|Interaction` e `Quests|UI` não aparecem mais na lista;
- `RuntimeInitializeOnLoadMethod` real permanece em 3 atributos por design:
  `GameRuntimeCompositionRoot`, `SceneTransitionRouter`, `CollisionDebugOverlayBootstrap`.

Pares explicitamente avaliados e pulados:

- `Equipment|Inventory`: o ciclo passa por `EquipmentSlot`, usado em muitos módulos e campos de
  dados; mover agora teria risco alto de regressão serializada.
- `Craft|UI`: `CraftingPoint` ainda possui referência serializada direta opcional ao `CraftingModal`;
  trocar para fluxo somente por evento mudaria fallback de cena.
- `Quests|Save`: envolve DTOs de save de quest em `SaveData`; deixado para spec-filha de persistência.

### Gates

- `tools/architecture/Get-ModularizationDependencySnapshot.ps1`: exit 0, `47 -> 45`.
- `dotnet build .\CindarsHope.Runtime.csproj`: exit 0, 0 warnings, 0 erros após cada par.
- `tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1`: exit 0, 7/7 projetos, 0 warnings, 0 erros.
- `tools/unity/RunUnityEditModeTests.ps1 -ResultsPath TestResults\v5-lote6-editmode.xml -LogFile Logs\v5-lote6.log`:
  exit 0, 2747/2747 pass, 0 failed.
- PlayMode composição `CindarsHope.Tests.PlayMode.Composition.GameRuntimeCompositionRootPlayModeTests`:
  exit 0, 2/2 pass, 0 failed (`Logs\v5-lote6-playmode-composition.xml`).

### Escopo restante

A modularização residual v5 está concluída para o escopo aprovado desta spec. Não declarar
"modularização ampla concluída": ainda restam 45 pares mútuos, que devem virar specs-filhas
independentes com recortes pequenos, validação e não-regressão próprias.
