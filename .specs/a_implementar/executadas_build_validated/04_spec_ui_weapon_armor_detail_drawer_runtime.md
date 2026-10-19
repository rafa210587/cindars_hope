# SPEC — UI Weapon Armor Detail Drawer Runtime

> **Spec ID:** `04_spec_ui_weapon_armor_detail_drawer_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 04 — UI / UX Foundation  
> **Priority:** P1  
> **Type:** Runtime / UI / Equipment Detail / Weapon Armor Drawer  
> **Domain:** UI / Equipment / Weapon Detail / Armor Detail / Known Interactions  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_04_UI_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere equipment backend, equipment compare, item tooltip, bestiary knowledge, durability/repair/upgrade, player derived stats or vulnerability adapters.  
> **Repo lock scope:** `Assets/_Game/Scripts/UI/**`, `Assets/_Game/Scripts/Equipment/**`, `Assets/_Game/Scripts/Inventory/**`, `Assets/_Game/Tests/EditMode/UI/**`, `docs/validation/04_spec_ui_weapon_armor_detail_drawer_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
  - `docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md`
  - `docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md`
  - `.specs/a_implementar/04_spec_ui_equipment_compare_runtime.md`
  - `.specs/a_implementar/04_spec_ui_inventory_items_tooltips_runtime.md`
> **Blocks:**  
  - equipment compare UI;
  - repair/upgrade UI;
  - known vulnerability projection;
  - inventory item detail;
  - bestiary knowledge UI.
> **Scope:** consolidar drawers de detalhe de armas/armaduras/acessórios com overview, stats, material, durabilidade, efeitos, tradeoffs e interações conhecidas sem revelar conhecimento oculto.  
> **Out of scope:** equipment backend rewrite, fórmulas finais de dano/armor, bestiary runtime completo, repair/upgrade mechanics, prefab final de detail drawer.

---

# /speckit.specify

## 1. Contexto

O direction de menu define que weapon/armor detail precisa separar overview, stats, effects, material, durability e known enemy interactions.

O direction de Equipment define papéis de armas, escudos, armor, acessórios, relíquias e materiais. Também deixa claro que tier maior não é sempre melhor e que equipamento deve ter identidade, tradeoff, escopo e não resolver todos os inimigos/biomas ao mesmo tempo.

Esta spec é detalhamento visual/projection. Ela não altera fórmulas nem backend.

---

## 2. Problema

Sem detail drawer robusto:

```text
item parece ser apenas +dano/+armor;
material/tier pode induzir conclusão falsa de "sempre melhor";
durabilidade/charges podem ficar invisíveis;
known enemy interactions podem revelar spoilers;
armor pesada/leve não comunica tradeoff;
relíquia divina pode parecer poder gratuito sem condição.
```

---

## 3. Objetivo

Criar/endurecer detail drawer:

```text
overview;
core stats;
attack/block/armor tradeoffs;
material/tier/quality/rarity;
durability/charges;
effects/passives/conditions;
known enemy interactions gated by bestiary;
requirements;
repair/upgrade hooks;
no backend formula duplication.
```

## Source Map Compliance

### Global sources read

- docs/design/SPEC_SOURCE_MAP.md
- docs/design/SPECIFICATION_PROCESS.md
- docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md
- .specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
- .specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
- .specs/SPEC_VALIDATION_MATRIX_MASTER.md
- .specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md
- docs/project/CURRENT_STATE.md

### Domain directions read

- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
- docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
- docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md

### Required interpretation

```text
Esta spec é derivada dos directions/refinements canônicos.
Ela não substitui os directions.
Ela transforma parte do refinement em contrato implementável com escopo, locks, validações e quality gate.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Weapon/armor detail separa overview, stats, effects, material, durability e known enemy interactions.
- Equipment UI só mostra interações com inimigos quando conhecimento estiver desbloqueado.
- Escudos afetam BlockPower, BlockStability, custo indireto de Block, GuardBreakResistance, PostureResistance, peso, movimento e StaminaRegen.
- Armor tem tipos Light/Medium/Heavy/Robe com tradeoffs de proteção, mobilidade, MP, magia e resistências.
- Peso afeta MovementSpeed, Dash comfort, Dodge recovery, StaminaRegen, Block stability e custos.
- Acessórios dão especialização, não bônus amplo demais.
- Relíquias divinas fortes precisam tradeoff, condição, cooldown ou escopo.
- Tier maior não deve ser sempre melhor em tudo.

### Deferred / future from directions

- Repair/upgrade mechanics.
- Full bestiary UI.
- Exact derived formulas.
- Item lore/source history UI future.
- Visual prefab final.

### Explicitly not redefined here

- Equipment backend.
- Derived attributes formulas.
- Bestiary knowledge state.
- Durability mechanics.
- Repair/upgrade backend.

## 4. Estado atual do repo

```text
Equipment UI e compare UI são parciais/novas specs.
Esta spec deve consumir compare/detail base e focar drawer detalhado.
```

A confirmar localmente:

```text
ItemDetailDrawer;
EquipmentCompareViewModel;
EquipmentDefinition;
Material/tier fields;
Durability/charges;
KnownEnemyInteractionService;
Bestiary knowledge.
```

---

## 5. User stories

```text
Como jogador, quero entender o tradeoff do equipamento.
Como jogador, quero saber material/tier/durabilidade.
Como jogador, quero ver interações com inimigos apenas quando descobertas.
Como jogador, quero saber se item pode ser reparado/upgradado futuramente.
```

---

## 6. Escopo

```text
weapon/armor/accessory detail drawer;
material/tier/properties projection;
durability/charges display;
known interaction gating;
repair/upgrade hooks;
requirements and blocked reasons;
tests/validators.
```

---

## 7. Fora de escopo

```text
equipment formulas;
repair/upgrade backend;
bestiar UI;
vulnerability balance;
prefab/art final.
```

---

## 8. Regras de não duplicação

```text
Não calcular fórmula final na UI.
Não criar EquipmentManager.
Não revelar unknown enemy interactions.
Não afirmar tier maior como universal.
Não transformar relíquia em bônus sem tradeoff.
```

---

## 9. Critérios de aceite

- Detail drawer mostra campos relevantes por tipo.
- Known interactions são gated.
- Material/tier/tradeoff claro.
- Durability/charges clear when available.
- Requirements blocked reasons shown.
- Report inclui PlayMode scenario.

---

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/UI/Equipment/EquipmentDetailDrawerViewModel.cs
Assets/_Game/Scripts/UI/Equipment/EquipmentMaterialTierPresenter.cs
Assets/_Game/Scripts/UI/Equipment/EquipmentKnownInteractionPresenter.cs
Assets/_Game/Tests/EditMode/UI/EquipmentDetailDrawerTests.cs
```

Consolidar existentes se houver.

---

## 11. Contratos

### Runtime

```text
UI consumes Equipment/Item snapshots.
Backend owns formulas and equip state.
Known interactions are filtered by knowledge service.
```

### Save

```text
No save schema change.
```

### UI

```text
Long detail drawer complements short tooltip.
```

---

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/UI/**
Assets/_Game/Tests/EditMode/UI/**
Assets/_Game/Scripts/Editor/Validation/**
docs/validation/04_spec_ui_weapon_armor_detail_drawer_runtime_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/Equipment/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Bestiary/**
Assets/_Game/Scripts/Player/**
```

---

## 13. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
.specs/SPEC_EXECUTION_ORDER.md
.specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

---

## 14. Estratégia

```text
1. Auditar equipment detail/tooltip/compare existentes.
2. Consolidar drawer view model.
3. Integrar knowledge-gated interactions.
4. Add tests for visibility/tradeoffs/fields.
5. Report.
```

---

## 15. Ordem segura

```text
Inventory detail -> Equipment compare -> Equipment detail drawer -> Repair/upgrade UI.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with equipment compare, repair/upgrade, bestiary knowledge or equipment backend.
- Reason: shared equipment projection.

---

## 17. Impacto save/load

```text
Does this change save schema? NO.
Does this add save section? NO.
Does this require migration? NO.
Does this persist Unity references? NO.
```

---

## 18. Impacto eventos

```text
Adds events: SHOULD BE NO.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if UI subscribers touched.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: YES view model/behavior.
Scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED.
```

---

## 20. Riscos

```text
Risco: spoiler.
Mitigação: knowledge gate tests.

Risco: formula drift.
Mitigação: UI consumes snapshots only.

Risco: material/tier misleading.
Mitigação: tradeoff labels.
```

---

## 21. Rollback

```text
Reverter equipment detail view model/tests/report.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar equipment detail/tooltip/compare.
- [ ] T003 — Consolidar detail drawer.
- [ ] T004 — Implementar known interaction gating.
- [ ] T005 — Criar tests.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Classes/sistemas existentes foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| UI focus | A tela usa focus/modal stack canônico? | Integração com input/modal foundation. | BUILD_VALIDATED no máximo |
| Spoiler/future | A spec respeita dados ocultos e features future? | Matriz de visibility/future no report. | PARTIAL |
| Save/load | A UI altera estado persistido ou só despacha comando? | Declaração de schema/no schema. | PARTIAL |
| Eventos | Subscriptions têm lifecycle? | Mapa de publishers/subscribers se houver. | PARTIAL |
| Testes | Há view model/projection determinística? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report criado? | `docs/validation/04_spec_ui_weapon_armor_detail_drawer_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar:

```bash
rg -n "Equipment|Weapon|Armor|Shield|Accessory|Relic|Material|Tier|Durability|KnownInteraction|Vulnerability|Bestiary|Tooltip|DetailDrawer" Assets/_Game/Scripts docs/design .specs
rg -n "TODO|FIXME|HACK|PARTIAL|DEFERRED|BUILD_VALIDATED|ACCEPTED" .specs docs/validation docs/IMPLEMENTATION_STATUS.md docs/project/CURRENT_STATE.md
```

Classificar achados como:

```text
EXISTING_CANONICAL
EXISTING_PARTIAL
MISSING_SAFE_TO_CREATE
MISSING_BUT_DEFER
CONFLICT
```

---

## 23C. Functional Acceptance Scenarios

### Scenario 1 — Happy path

```text
Given a tela/flow de weapon/armor detail drawer é aberto com dados válidos
When o jogador navega, seleciona elemento e executa ação segura
Then a UI mostra foco claro, detalhe correto e ações disponíveis
And gameplay input fica bloqueado se modal estiver aberto
And o domínio recebe apenas comando/intent válido, não mutação escondida de UI.
```

### Scenario 2 — Unknown/hidden/future data

```text
Given existe informação oculta, futura, bloqueada por história ou ainda não descoberta
When a UI renderiza a tela
Then ela não revela spoiler nem promessa quebrada
And mostra estado genérico/??? apenas quando o direction permitir
And registra no report qualquer placeholder/future hook.
```

### Scenario 3 — Missing requirement / unavailable state

```text
Given o jogador não cumpre requisito, não tem recurso ou não conhece a informação
When a ação ou detalhe é selecionado
Then a UI mostra motivo bloqueado
And não executa comando inválido
And o backend continua sendo fonte de verdade.
```

### Scenario 4 — Costly or irreversible action

```text
Given uma ação é custosa, destrutiva, irreversível ou protegida
When o jogador tenta executá-la
Then confirmação é exigida quando o direction manda
And foco inicial respeita regra de ação destrutiva
And cancel/back não executa ação de mundo.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige PlayMode ou interação visual integrada
When a spec termina tecnicamente
Then o report registra cenário final
And não pede validação humana imediata por spec
And status respeita SPEC_VALIDATION_MATRIX_MASTER.md.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Enemy interaction desconhecida revelada.
- Tier maior mostrado como universalmente melhor.
- Relíquia sem condição/tradeoff/cooldown visível.
- Durability/charges omitidas.
- Armor weight tradeoff omitido.
- Shield block tradeoff omitido.
- UI calculando dano/armor final.
- Repair/upgrade action mostrada sem backend.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — UI Weapon Armor Detail Drawer Runtime

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
- Unknown/hidden/future:
- Missing requirement:
- Protected/costly action:
- Negative cases:

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

```text
1. A implementação exigir Packages/ ou ProjectSettings/.
2. A implementação exigir scene/prefab/asset wiring fora do escopo.
3. A implementação exigir mudança de save schema.
4. A implementação exigir reescrever sistema canônico existente.
5. A implementação conflitar com input focus/modal foundation.
6. A implementação revelar spoiler/future state proibido pelo direction.
7. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Detail Drawer Field Matrix

| Item type | Required fields | Conditional fields |
|---|---|---|
| Weapon | Damage snapshot, speed/cost, material/tier, durability, tags | Known enemy interactions, scaling, special effects |
| Shield | BlockPower/Stability snapshot, weight, stamina impact | Arcane ward/MP interactions |
| Armor | Armor/resistance snapshot, weight, mobility/stamina tradeoffs | Biome/family resistances if known |
| Accessory | Specialized effect, condition/scope | Cooldown/trigger if applicable |
| Relic | Deity/lore flavor, effect, tradeoff/condition | Corruption/risk/cooldown |
| Staff/Focus | Magic amplification snapshot, MP/cast interactions | Spell source/basic bolt if applicable |

## 23H. Known Interaction Guardrails

```text
If knowledge is Unknown: do not show interaction.
If Seen/Fought only: show vague category at most.
If Studied/FullyDocumented: show specific vulnerability/resistance if allowed.
Equipment detail cannot be source of bestiary truth.
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Weapon|Armor|Repair|Upgrade|Spell|Magic|Gamepad|Navigation|Focus|Detail|Durability|Material|MP|Cooldown|Requirement" Assets/_Game/Scripts docs/design .specs
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
Quando houver cenário integrado, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES if drawer projection/visibility logic changes.
- Requires EditMode tests: YES for field projection and knowledge-gated interaction tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for detail drawer navigation in inventory/equipment.
- Requires regression test: YES if fixing known equipment spoiler/detail bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver cenário integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no unknown interaction spoiler; no formula duplication.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/04_spec_ui_weapon_armor_detail_drawer_runtime_execution_report.md.
Fonte/direction coverage preservado.
Validações obrigatórias PASS ou NOT RUN com motivo, impacto e mitigação.
Sem promoção indevida para ACCEPTED apenas por compile.
```

---

## 28. Anti-regressão

```text
Não duplicar sistemas canônicos existentes.
Não quebrar save/load.
Não usar UI como fonte de verdade.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
Não alterar scenes/prefabs/assets nesta spec.
Não revelar spoiler, future state ou função bloqueada.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
