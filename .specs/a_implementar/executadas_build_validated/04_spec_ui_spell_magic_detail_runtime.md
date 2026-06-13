# SPEC — UI Spell Magic Detail Runtime

> **Spec ID:** `04_spec_ui_spell_magic_detail_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 04 — UI / UX Foundation  
> **Priority:** P1  
> **Type:** Runtime / UI / Magic / Spell Detail / Active Slot Assignment  
> **Domain:** UI / Magic / Spell Detail / MP / Cooldown / Requirements / Active Slots  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_04_UI_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere spell runtime, magic skill tree, active slot backend, MP formulas, equipment spell sources, scroll/wand/tome runtime or combat casting.  
> **Repo lock scope:** `Assets/_Game/Scripts/UI/**`, `Assets/_Game/Scripts/Magic/**`, `Assets/_Game/Scripts/Skills/**`, `Assets/_Game/Tests/EditMode/UI/**`, `docs/validation/04_spec_ui_spell_magic_detail_runtime_execution_report.md`  
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
  - `docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md`
  - `docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md`
  - `.specs/a_implementar/04_spec_ui_skill_tree_active_slots_runtime.md`
> **Blocks:**  
  - spell runtime/data assets;
  - magic skill tree;
  - active slot assignment;
  - equipment spell sources;
  - combat cast HUD.
> **Scope:** consolidar Spell/Magic Detail UI com categoria, shape, MP, cooldown, cast/recovery, requisitos, active slot assignment e warnings de tradeoff sem implementar spell runtime.  
> **Out of scope:** spell runtime final, formulas de MP/dano, VFX/SFX, combat casting implementation, scroll/wand/tome backend, visual prefab final.

---

# /speckit.specify

## 1. Contexto

O direction de Magic define SpellActionDataSO com campos mínimos como SpellId, categoria, shape, damage/status/utility tags, scaling, MP cost, cooldown, cast time, recovery, range, requisitos, equipment/story flags, vulnerability tags, HUD icon e VFX/SFX IDs.

Também define que spells equipáveis ocupam active slots, o jogador tem 4 active slots, e Dash/Dodge/Block não ocupam active slot.

Esta spec cria a UI/detail/projection de spell, sem implementar runtime de cast.

---

## 2. Problema

Sem spell detail UI:

```text
jogador não entende custo de MP/cooldown/cast;
spell forte pode parecer sem risco;
pré-requisito de skill/tome/equipment/story pode ser oculto;
spell pode equipar automaticamente em active slot;
Dash/Dodge/Block podem ser confundidos como slots;
spell de deus pode parecer escola mecânica separada;
unknown vulnerability pode vazar via tooltip.
```

---

## 3. Objetivo

Criar/endurecer spell detail:

```text
display category/shape/tier;
MP cost/cooldown/cast/recovery;
range/area/targeting;
scaling summary;
requirements;
allowed/forbidden capstone;
active slot assignment;
source: skill/tome/equipment/scroll/focus;
risk/interruption warning;
no formula duplication.
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
- docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
- docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md

### Required interpretation

```text
Esta spec é derivada dos directions/refinements canônicos.
Ela não substitui os directions.
Ela transforma parte do refinement em contrato implementável com escopo, locks, validações e quality gate.
```

---

## Direction / Refinement Coverage

### Covered from directions

- SpellActionDataSO tem campos mínimos como SpellId, DisplayName, Tier, SpellCategory, SpellShape, DamageTypes, StatusTags, UtilityTags, scaling, MP cost, cooldown, cast/recovery, range, requirements, tags, HUDIcon, VFX/SFX IDs.
- Categorias mecânicas: Damage, Healing, Buff, Barrier, Utility, Purification, ControlLight, RiskCorruption.
- Shapes: Self, AllyTarget, AutoTargetEnemy, AimProjectile, Cone, Line, PlacedAoE, Chain, AuraShort, GroundSeal.
- Toda spell gasta MP salvo scroll consumível com redução/substituição parcial.
- Spell forte precisa custo/risco: cast time, cooldown, recovery, posição arriscada ou pré-requisito.
- Spells equipáveis ocupam active slots; jogador tem 4 active slots.
- Dash, Dodge e Block não ocupam active slot.
- Nome de deus é identidade/lore, não escola mecânica separada.

### Deferred / future from directions

- Spell runtime/casting.
- VFX/SFX final.
- MP/damage formula final.
- Scroll/wand/tome runtime.
- Full magic balancing.
- Combat HUD cast bar final.

### Explicitly not redefined here

- Magic skill tree backend.
- Active slot backend.
- MP formulas.
- Combat core casting.
- Equipment spell sources.
- Bestiary vulnerabilities.

## 4. Estado atual do repo

```text
Magic/spells podem ainda não existir como runtime completo.
Skill Tree UI/Active Slots já foram especificados.
Esta spec deve ser projection/detail/future hook se spell runtime estiver ausente.
```

A confirmar localmente:

```text
MagicManager;
SpellActionDataSO;
Spell runtime;
Magic skill tree nodes;
ActiveSlot runtime;
Equipment/tome/scroll spell sources;
MP fields.
```

---

## 5. User stories

```text
Como jogador, quero entender custo/risco de uma magia.
Como jogador, quero saber requisito faltante.
Como jogador, quero equipar magia em um dos 4 active slots com confirmação.
Como jogador, quero distinguir magia aprendida, item mágico e spell de equipamento.
```

---

## 6. Escopo

```text
spell detail drawer;
requirement projection;
MP/cooldown/cast/range display;
shape/category tags;
active slot assignment projection;
source display;
hidden/locked spell policy;
tests/validators.
```

---

## 7. Fora de escopo

```text
spell runtime;
cast implementation;
VFX/SFX;
magic formulas;
spell content final;
scroll/wand/tome backend.
```

---

## 8. Regras de não duplicação

```text
Não criar MagicManager se ausente sem spec própria.
Não calcular damage/MP formulas na UI.
Não equipar spell automaticamente em slot cheio.
Não tratar deus no nome como escola mecânica.
Não mostrar unknown vulnerability interactions.
```

---

## 9. Critérios de aceite

- Spell detail displays cost/risk/shape/requirements.
- Locked/hidden spells do not reveal spoilers.
- Active slot assignment obeys 4-slot rule.
- Dash/Dodge/Block not listed as active slot spells.
- UI dispatches command only.
- Report records future gaps if runtime missing.

---

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/UI/Magic/SpellDetailDrawerViewModel.cs
Assets/_Game/Scripts/UI/Magic/SpellRequirementPresenter.cs
Assets/_Game/Scripts/UI/Magic/SpellActiveSlotAssignmentViewModel.cs
Assets/_Game/Tests/EditMode/UI/SpellDetailProjectionTests.cs
```

Consolidar existentes se houver.

---

## 11. Contratos

### Runtime

```text
UI consumes spell definitions/snapshots.
Backend owns cast and slot assignment validation.
```

### Save

```text
No save schema change.
```

### UI

```text
Detail drawer complements skill tree/active slots.
```

---

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/UI/**
Assets/_Game/Tests/EditMode/UI/**
Assets/_Game/Scripts/Editor/Validation/**
docs/validation/04_spec_ui_spell_magic_detail_runtime_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/Magic/**
Assets/_Game/Scripts/Skills/**
Assets/_Game/Scripts/Player/**
Assets/_Game/Scripts/Equipment/**
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
1. Auditar magic/spell runtime.
2. If absent, create projection/future hook only.
3. Consolidar spell detail view model.
4. Add active slot and requirement tests.
5. Report.
```

---

## 15. Ordem segura

```text
Magic data/runtime -> Skill tree active slots -> Spell detail UI.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with magic runtime, skill tree, active slot or equipment spell source changes.
- Reason: shared active slot and magic contracts.

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
Risco: UI promete magic runtime inexistente.
Mitigação: hidden/future hook only.

Risco: formula drift.
Mitigação: snapshots/backend owns.

Risco: active slot confusion.
Mitigação: 4-slot validation and Dash/Dodge/Block exclusion.
```

---

## 21. Rollback

```text
Reverter spell detail UI/view model/tests/report.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar magic/skill/slot runtime.
- [ ] T003 — Consolidar spell detail projection.
- [ ] T004 — Implement active slot requirement hardening.
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
| Report | Execution report criado? | `docs/validation/04_spec_ui_spell_magic_detail_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar:

```bash
rg -n "Magic|Spell|SpellActionData|MPCost|Cooldown|CastTime|Recovery|SpellShape|SpellCategory|ActiveSlot|Dash|Dodge|Block|Tome|Scroll|Wand|Focus" Assets/_Game/Scripts docs/design .specs
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
Given a tela/flow de spell/magic detail UI é aberto com dados válidos
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

- Spell runtime ausente mostrado como feature final.
- MP/cooldown/cast/recovery omitidos.
- Spell forte sem risco visível.
- Prerequisite oculto ou incorreto.
- Dash/Dodge/Block listados como spell active slots.
- Spell equipada automaticamente em slot cheio.
- Nome de deus interpretado como escola mecânica.
- UI calcula dano/MP final com fórmula própria.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — UI Spell Magic Detail Runtime

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


## 23G. Spell Detail Field Matrix

| Field | Required | Notes |
|---|---:|---|
| SpellId/DisplayName | YES | Stable ID and label |
| Tier | YES | Access tier |
| Category | YES | Damage/Healing/Buff/etc |
| Shape | YES | Self/Cone/Line/etc |
| MP cost | YES | Unless consumable source modifies |
| Cooldown | YES | Risk/balance clarity |
| Cast/Recovery | YES | If applicable |
| Range/Radius/Cone/Line | SHOULD | Based on shape |
| Scaling summary | SHOULD | Snapshot, not formula |
| Requirements | YES | Skill/tome/equipment/story |
| Source | YES | Learned/item/equipment/scroll |
| Active slot status | YES | 4-slot rule |
| Interruption/risk | SHOULD | Strong spells |
| VFX/SFX IDs | Debug/future | Not required for player UI |

## 23H. Active Slot Guardrails

```text
Exactly 4 active skill slots.
Spells occupy active slots only if equipable.
Dash/Dodge/Block are not active slot spells.
StaffBasicBolt may be weapon/focus action, not necessarily active slot.
Replacing occupied slot requires confirmation.
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

- Changed deterministic logic: YES if spell detail/slot projection logic changes.
- Requires EditMode tests: YES for spell projection and active slot tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for spell detail and assignment flow.
- Requires regression test: YES if fixing known spell slot/detail bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver cenário integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no magic runtime promise if absent; no active slot confusion.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/04_spec_ui_spell_magic_detail_runtime_execution_report.md.
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
