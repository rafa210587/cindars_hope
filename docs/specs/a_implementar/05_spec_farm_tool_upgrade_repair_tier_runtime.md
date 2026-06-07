# SPEC — Farm Tool Upgrade Repair Tier Runtime

> **Spec ID:** `05_spec_farm_tool_upgrade_repair_tier_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 05 — Farm Construction / Tools / Conditions  
> **Priority:** P1  
> **Type:** Runtime / Farm / Tools / Upgrade / Repair / Tier  
> **Domain:** Farm / Tools / Hoe / Axe / Pickaxe / WateringCan / FishingRod / Upgrade / Repair  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_05_FARM_CONSTRUCTION_TOOLS_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere equipment backend, durability, repair/upgrade UI, economy costs, cave materials, tool actions, crop/resource nodes, player stamina/fatigue formulas ou save schema.  
> **Repo lock scope:** `Assets/_Game/Scripts/Farm/**`, `Assets/_Game/Scripts/Equipment/**`, `Assets/_Game/Scripts/Tools/**`, `Assets/_Game/Scripts/Economy/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Farm/**`, `docs/validation/05_spec_farm_tool_upgrade_repair_tier_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md`
  - `docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md`
  - `docs/specs/a_implementar/04_spec_ui_repair_upgrade_screen_flow_runtime.md`
  - `docs/specs/a_implementar/05_spec_farm_trees_wood_stumps_regrowth_runtime.md`
  - `docs/specs/a_implementar/05_spec_farm_rocks_stone_light_mining_runtime.md`
> **Blocks:**  
  - tool actions;
  - repair/upgrade UI;
  - farm construction/material economy;
  - resource nodes requiring tool tiers;
  - city blacksmith service.
> **Scope:** definir/endurecer upgrade/repair/tier de ferramentas de fazenda, conectando material tiers, custo, ação e progressão sem reescrever equipment backend.  
> **Out of scope:** combat weapons, armor formulas, city blacksmith content, full durability balance, prefab/tool animation, final economy values.

---

# /speckit.specify

## 1. Contexto

Roadmap 2 inclui melhoria de ferramentas. Equipment Direction define Tools como Hoe, AxeTool, Pickaxe, WateringCan, FishingRod e HammerTool futuro. Também define tiers de material e regra: tier maior não deve ser sempre melhor em tudo; deve abrir identidade, eficiência, resistência ou especialização.

Esta spec cobre ferramentas agrícolas como equipamento utilitário, não armas nem combat balance.

---

## 2. Problema

Sem contrato de tool upgrade/repair:

```text
tool tier pode ser só número sem efeito;
upgrade pode ser grátis ou duplicado após reload;
tool upgrade pode ignorar materiais da caverna;
watering can pode quebrar economia de irrigação;
axe/pickaxe podem coletar recurso de tier alto cedo;
repair pode se confundir com upgrade;
tier maior pode ser sempre melhor sem tradeoff;
durability pode quebrar farm loop se mal calibrada.
```

---

## 3. Objetivo

Criar/endurecer:

```text
FarmToolDefinition;
ToolTier;
ToolUpgradeDefinition;
ToolCapability;
ToolActionEfficiency;
Repair vs Upgrade separation;
Upgrade material/gold cost;
Tool tier gates for resources;
Save/load tool tier/durability;
No free duplicate upgrade.
```

---

## 4. Regras de design

```text
Ferramenta melhora rotina e desbloqueia eficiência.
Upgrade deve exigir ouro + materiais + progresso.
Materiais avançados vêm majoritariamente da caverna.
Tier maior pode reduzir stamina/fatigue, aumentar área, hits ou eficiência.
ToolAttack é emergência/utilidade, não substitui arma dedicada.
Repair e Upgrade são ações diferentes.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero melhorar ferramentas para reduzir repetição e liberar recursos.
Como sistema de recursos, quero exigir tool tier correto.
Como economy, quero que upgrade seja sink de ouro/materiais.
Como save/load, quero preservar tier/durability.
Como equipment, quero não misturar ferramenta com active skill.
```

---

## 6. Escopo

Inclui:

```text
farm tool definitions;
tool tier/capability contract;
upgrade requirements/costs;
repair vs upgrade state;
resource node tier gates;
stamina/fatigue efficiency hooks;
save/load safety;
tests/validators.
```

Não inclui:

```text
combat weapons;
blacksmith NPC content final;
tool animation/prefabs;
full durability balance;
city shop UI;
exact upgrade prices.
```

## Source Map Compliance

### Global sources read

- docs/design/SPEC_SOURCE_MAP.md
- docs/design/SPECIFICATION_PROCESS.md
- docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md
- docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
- docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
- docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md
- docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md
- docs/project/CURRENT_STATE.md

### Domain directions read

- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
- docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
- docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md

### Required interpretation

```text
Esta spec é derivada dos directions/refinements canônicos.
Ela não substitui os directions.
Ela transforma parte do refinement em contrato implementável com escopo, locks, validações e quality gate.
Quando houver divergência entre esta spec e os directions, o executor deve parar e registrar CONFLICT no execution report.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Roadmap 2 pede melhoria de ferramentas.
- Equipment Direction define Tools: Hoe, AxeTool, Pickaxe, WateringCan, FishingRod e HammerTool futuro.
- Tiers de material vão de Madeira/Pedra a Meteórico/Mana/Pedra Negra estabilizada/Relíquias.
- Tier maior não deve ser sempre melhor; deve abrir identidade, eficiência, resistência ou especialização.
- Madeira, Pedra, Cobre, Ferro e Aço têm papéis de materiais comuns/early/mid.
- Materiais avançados devem vir majoritariamente da caverna e não ser substituídos pela fazenda.

### Deferred / future from directions

- Final upgrade price table.
- Blacksmith/city service content.
- Tool animations/prefabs.
- Full durability balance.
- Meteórico/Mana/Pedra Negra tool upgrades final.
- Combat use of tools.

### Explicitly not redefined here

- Equipment backend.
- Player derived stamina formulas.
- Repair/upgrade UI.
- Resource node definitions.
- Inventory/crafting material schema.

## 7. Modelo de domínio

### 7.1 FarmToolType

```text
Hoe
AxeTool
Pickaxe
WateringCan
FishingRod
HammerToolFuture
```

### 7.2 ToolTier

```text
Tier0_Improvised_Wood_Stone
Tier1_Copper_Bone_Leather
Tier2_Iron
Tier3_Steel
Tier4_Silver_RefinedSteel_Reinforced
Tier5_Mithril_ArcaneCrystal_Bromecian
Tier6_Meteoric_Mana_StabilizedBlackstone_Relic
```

### 7.3 FarmToolDefinition

```text
ToolId
ToolType
Tier
MaterialTags
DurabilityMax optional
BaseStaminaCostModifier
BaseFatigueCostModifier
AreaOfEffect optional
ActionSpeedModifier optional
RequiredPlayerLevel optional
RequiredFarmLevel optional
RequiredQuestFlag optional
CanRepair
CanUpgrade
CanBeUsedAsEmergencyAttack
```

### 7.4 ToolUpgradeDefinition

```text
UpgradeId
FromToolId
ToToolId
RequiredGold
RequiredMaterials[]
RequiredServiceId optional
RequiredRecipeId optional
RequiredFarmLevel
RequiredCaveDepth optional
BuildTimeDays optional
ResultPolicy: ReplaceTool | ModifyInstance | AddCapability
```

---

## 8. Tool capabilities

```text
Hoe:
  - till area/cost efficiency.

AxeTool:
  - chop tree tiers, stump removal, wood efficiency.

Pickaxe:
  - break rock tiers, light ore access, not cave rare bypass.

WateringCan:
  - water capacity/area, not advanced irrigation replacement by itself.

FishingRod:
  - catch table access, stamina/fatigue/time efficiency, rare fish gates.

HammerToolFuture:
  - construction/repair/future; not current unless existing.
```

---

## 9. Repair vs Upgrade

```text
Repair restores durability/condition.
Upgrade changes tier/capability.
Repair cost should be smaller and repeatable.
Upgrade cost is progression and should happen once per tier path.
UI may show both, but backend commands are separate.
```

---

## 10. Economy/material guardrails

```text
No upgrade with negative/zero cost unless tutorial explicitly.
No free repeated upgrade.
No rare/cave materials replaced by farm-only loop.
Tool upgrade should not bypass skill/farm/city/cave progression.
Tier 6 materials are endgame/special and not current common path.
```

---

## 11. Save/load

Must preserve:

```text
ToolId or ToolInstanceId
Tier
Durability/condition if supported
Upgrade path/current capability
Material/quality if supported
```

Must not persist:

```text
tool prefab reference;
UI selected upgrade;
temporary preview as applied upgrade.
```

If tool instances/tier persistence is absent, STOP before schema change.

---

## 12. Criteria

```text
Tool tier/capability is explicit.
Upgrade requires cost/material/progression.
Repair and upgrade are separate commands.
Resource nodes can require tool tier.
Upgrade cannot apply twice after reload.
Tool state persists or blocks with migration requirement.
Tests cover upgrade, insufficient materials, duplicate prevention, tier gates and repair separation.
```

---

# /speckit.plan

## 13. Arquitetura alvo

```text
Assets/_Game/Scripts/Farm/Tools/FarmToolType.cs
Assets/_Game/Scripts/Farm/Tools/FarmToolDefinition.cs
Assets/_Game/Scripts/Farm/Tools/ToolTier.cs
Assets/_Game/Scripts/Farm/Tools/ToolUpgradeDefinition.cs
Assets/_Game/Scripts/Farm/Tools/FarmToolUpgradeService.cs
Assets/_Game/Scripts/Farm/Tools/FarmToolCapabilityResolver.cs
Assets/_Game/Tests/EditMode/Farm/FarmToolUpgradeTests.cs
```

Consolidar existentes se houver.

---

## 14. Arquivos permitidos

```text
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/Tools/**
Assets/_Game/Scripts/Equipment/**
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Crafting/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Farm/**
docs/validation/05_spec_farm_tool_upgrade_repair_tier_runtime_execution_report.md
```

---

## 15. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

---

## 16. Estratégia

```text
1. Auditar tools/equipment/upgrade systems.
2. Consolidar FarmToolDefinition/ToolTier.
3. Implementar/harden upgrade service.
4. Separar repair/upgrade commands.
5. Integrar resource node tier gates.
6. Criar tests.
7. Criar report.
```

---

## 17. Paralelização

- Parallelizable: NO
- Must not run with:
  - equipment backend;
  - repair/upgrade UI;
  - resource nodes;
  - economy/material costs;
  - player stamina/fatigue.
- Reason: tools bridge farm actions, economy, equipment and player conditions.

---

## 18. Impacto save/load

```text
Does this change save schema? SHOULD BE NO if tool instances exist; otherwise STOP.
Does this add save section? NO unless dedicated save spec approves.
Does this require migration? NO unless adding tool tier persistence; then STOP.
Does this persist Unity references? NO.
```

---

## 19. Impacto eventos

```text
Adds events: CONDITIONAL, e.g. ToolUpgradedEvent, ToolRepairedEvent if event contracts allow.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if subscribers are created.
```

---

## 20. Impacto UI/Unity

```text
Changes UI: NO final.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for upgrade/use tool flow.
```

---

## 21. Riscos

```text
Risco: upgrade duplicado.
Mitigação: idempotency and save tests.

Risco: tool tier bypasses cave/economy.
Mitigação: required materials/progression gates.

Risco: durability breaks farm loop.
Mitigação: repair separation and balance deferred.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar tools/equipment/upgrade.
- [ ] T003 — Consolidar tool definitions/tier.
- [ ] T004 — Implementar/harden upgrade service.
- [ ] T005 — Implementar repair/upgrade separation.
- [ ] T006 — Integrar resource node tier gates.
- [ ] T007 — Criar tests.
- [ ] T008 — Rodar validações.
- [ ] T009 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a farm tool upgrade/repair/tier foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Escopo | A execução ficou dentro de farm tool upgrade/repair/tier? | Arquivos alterados e justificativa. | PARTIAL |
| Economia | A spec não cria ouro infinito, bypass de custo ou upgrade grátis? | Anti-exploit tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/05_spec_farm_tool_upgrade_repair_tier_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "Tool|FarmTool|Hoe|AxeTool|Pickaxe|WateringCan|FishingRod|ToolUpgrade|ToolTier|Repair|Durability" Assets/_Game/Scripts docs/design docs/specs
rg -n "Building|Construction|Workshop|Storage|Fertilizer|Tool|Upgrade|Fatigue|Sleep|Hunger|Stamina|PlayerCondition|Save|Economy|Inventory" Assets/_Game/Scripts docs/design docs/specs
rg -n "TODO|FIXME|HACK|PARTIAL|DEFERRED|BUILD_VALIDATED|ACCEPTED" docs/specs docs/validation docs/IMPLEMENTATION_STATUS.md docs/project/CURRENT_STATE.md
```

A execução deve classificar cada achado como:

```text
EXISTING_CANONICAL
  Sistema já existe e deve ser reaproveitado/endurecido.

EXISTING_PARTIAL
  Sistema existe, mas precisa hardening/delta.

MISSING_SAFE_TO_CREATE
  Sistema não existe e criação é pequena, isolada e dentro do escopo.

MISSING_BUT_DEFER
  Sistema não existe, mas criação exigiria outro domínio/spec.

CONFLICT
  Há dois caminhos possíveis ou contrato divergente. Parar e reportar.
```

---

## 23C. Functional Acceptance Scenarios

### Scenario 1 — Happy path

```text
Given o sistema base relacionado a farm tool upgrade/repair/tier existe ou foi criado de forma mínima
When o jogador executa o fluxo principal desta spec
Then o comportamento segue o direction canônico
And save/load, economia, inventário e estado do jogador permanecem consistentes
And nenhum sistema paralelo é criado
And o execution report registra evidência.
```

### Scenario 2 — Existing partial implementation

```text
Given já existe implementação parcial no repo
When a execução audita o sistema
Then ela escolhe HARDEN_EXISTING em vez de recriar do zero
And registra divergências do direction
And altera apenas o menor conjunto seguro de arquivos.
```

### Scenario 3 — Missing dependency

```text
Given uma dependência runtime não existe no repo local
When a execução encontra essa ausência
Then ela não inventa uma solução massiva
And marca como MISSING_BUT_DEFER ou cria apenas adapter/validator mínimo se seguro
And registra risco residual.
```

### Scenario 4 — Economy/save invalid state

```text
Given custo, material, upgrade, construção, condition ou save payload inválido
When validator/teste é executado
Then a violação é reportada com motivo claro
And o runtime não duplica item/ouro, não aplica upgrade grátis e não corrompe save.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual de FarmScene/UI/gameplay
When a spec termina tecnicamente
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Upgrade aplicado duas vezes após reload.
- Tool tier não persiste.
- Repair e upgrade misturados.
- Upgrade grátis/sem custo.
- Rare/cave material bypassado.
- Tier maior universalmente melhor sem tradeoff.
- Resource node ignora tool tier.
- ToolAttack vira arma melhor que weapon.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Farm Tool Upgrade Repair Tier Runtime

## Summary
- Spec:
- Branch:
- Executor:
- Date:
- Final status:

## Sources read
- ...

## Local audit
- Commands executed:
- Existing systems found:
- Existing partial systems found:
- Missing systems:
- Conflicts:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial implementation:
- Missing dependency:
- Economy/save invalid state:
- Save/load safety:
- Visual/final scenario:

## Validation
- Docs validation:
- C# build:
- Unity compile:
- EditMode tests:
- PlayMode automated:
- Final human scenario:

## Testing Quality Gate
- Changed deterministic logic:
- Requires EditMode tests:
- Requires PlayMode automated or final human scenario:
- Requires regression test:
- Human validation timing:
- Minimum validation evidence for ACCEPTED:

## Residual risks
- ...

## Next specs impacted
- ...
```

---

## 23F. Stop Conditions

Parar a execução e registrar `BLOCKED` se ocorrer qualquer um destes casos:

```text
1. A implementação exigir alterar Packages/ ou ProjectSettings/.
2. A implementação exigir scene/prefab/asset wiring fora do escopo.
3. A implementação exigir mudança de save schema sem migration spec.
4. A implementação exigir reescrever Farm/Economy/Inventory/PlayerCondition system canônico existente.
5. A implementação criar conflito com day transition, save ownership, stable IDs ou Testing Quality Gate.
6. A implementação criar pets/companions/animals runtime fora do escopo desta wave.
7. A implementação criar invasão/defesa/inimigos/dano a crops na fazenda, proibido no roadmap atual.
8. A implementação permitir ouro infinito, item duplication, upgrade grátis, custo negativo ou recovery exploit.
9. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Tool Capability Matrix

| Tool | Main upgrade effect | Must not do |
|---|---|---|
| Hoe | till area/cost efficiency | plant automatically without spec |
| AxeTool | tree tier/hit efficiency | replace combat axe |
| Pickaxe | rock tier/hit efficiency | replace cave mining progression |
| WateringCan | capacity/area | replace irrigation entirely |
| FishingRod | catch table/efficiency | unlock endgame fish early |
| HammerToolFuture | building/repair future | appear as current full system |

## 23H. Tier Guardrails

```text
Tier 0-3 can support early/mid farm.
Tier 4+ should require stronger progression/materials.
Tier 5+ should not be farm-only.
Tier 6 is endgame/special and not common path.
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Building|Construction|Workshop|Storage|Fertilizer|Tool|Upgrade|Fatigue|Sleep|Hunger|Stamina|PlayerCondition|Save|Economy|Inventory" Assets/_Game/Scripts docs/design docs/specs
```

C# runtime/editor quando houver alteração C#:

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
```

Unity compile quando houver alteração Unity C#:

```powershell
.\tools\unity\RunUnityCompileValidation.ps1 -ProjectPath "." -LogFile ".\Logs\unity-compile-validation.log"
.\tools\unity\ScanUnityLogs.ps1 -LogFile ".\Logs\unity-compile-validation.log"
```

EditMode tests quando lógica determinística for criada/alterada:

```text
Unity Test Runner — EditMode, ou comando local equivalente disponível no repo.
```

PlayMode/final human validation:

```text
Não pedir validação humana por spec.
Quando houver cenário visual/gameplay de construção, upgrade, fertilizante, ferramenta, sono/cansaço/fome/stamina, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, upgrade/repair/tier-gate logic is deterministic.
- Requires EditMode tests: YES for upgrade/insufficient materials/duplicate/reload/tier gate tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for tool upgrade and use flow.
- Requires regression test: YES if fixing existing tool upgrade/durability bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver FarmScene/UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no free duplicate upgrade; no save schema change.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/05_spec_farm_tool_upgrade_repair_tier_runtime_execution_report.md.
Fonte/direction coverage preservado.
Validações obrigatórias PASS ou NOT RUN com motivo, impacto e mitigação.
Sem promoção indevida para ACCEPTED apenas por compile.
```

---

## 28. Anti-regressão

```text
Não duplicar sistemas canônicos existentes.
Não quebrar save/load.
Não persistir Unity references.
Não usar UI como fonte de verdade.
Não implementar pets/companions/animals runtime.
Não criar invasão/defesa/inimigos na fazenda.
Não criar item/ouro/upgrade infinito.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
