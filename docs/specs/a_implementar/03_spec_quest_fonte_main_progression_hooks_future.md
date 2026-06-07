# SPEC — Quest Fonte/Main Progression Hooks Future

> **Spec ID:** `03_spec_quest_fonte_main_progression_hooks_future`  
> **Status:** A implementar  
> **Revision:** EXPANDED_01_05_CORRECTED  
> **Execution priority:** DEFERRED / Future Hook  
> **Wave:** WAVE 03 — Quest / Objective / Event System  
> **Priority:** P2 / Future Hook  
> **Type:** Runtime Hook / Quest / Main Progression / Fonte  
> **Domain:** Quests / Main Progression / Fonte de Anya / Fragment Gates  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_03_QUEST_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere MainProgressionSection, FonteAnyaSection, QuestState, QuestFlags, Fonte UI, respawn/respec/purification, fragment runtime ou main quest content.  
> **Repo lock scope:** `Assets/_Game/Scripts/Quests/**`, `Assets/_Game/Scripts/Fonte/**`, `Assets/_Game/Scripts/Progression/**`, `docs/validation/03_spec_quest_fonte_main_progression_hooks_future_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`
  - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `docs/specs/a_implementar/03_spec_quest_objective_event_contract_runtime.md`
  - `docs/specs/a_implementar/03_spec_quest_flags_registry_runtime.md`
> **Blocks:**  
  - main quest runtime specs;
  - FonteAnyaSection save/load;
  - Fonte UI progression;
  - respec/living water/purification unlocks;
  - fragment gates.
> **Scope:** preparar hooks entre Quest system, MainProgression e FonteAnya sem esconder todo estado da Fonte dentro do QuestState genérico.  
> **Out of scope:** implementar main quest completa, diálogos, cutscenes, bosses, Fonte UI final, Água Viva gameplay completo, respec/purification runtime final.

---

# /speckit.specify

## 1. Contexto

O direction de Quest define que o sistema genérico pode acionar e observar Fonte/Main objectives, mas não deve esconder todo estado da Fonte dentro de QuestState.

O direction de Main Progression define quatro atos e quatro fragmentos: Água, Memória, Vida e Esperança. Cada fragmento desbloqueia função da Fonte: Água Viva limitada, respec, purificação/cura avançada e decisão final.

O direction de Save exige seções separadas:

```text
QuestStateSection;
MainProgressionSection;
FonteAnyaSection.
```

Esta spec cria apenas hooks/future contract para que quest runtime converse com essas seções sem duplicar estado.

---

## 2. Problema

Sem hooks claros:

```text
main progression pode virar flags soltas;
Fonte state pode ficar escondido em QuestState;
Quest reward pode desbloquear respec/Água Viva duas vezes;
Quest Log pode revelar final/nível 101 cedo;
save/load pode perder fragmentos;
fragment unlock pode quebrar UI/Fonte.
```

---

## 3. Objetivo

Definir hooks future-safe:

```text
Quest observa e emite eventos para MainProgression/FonteAnya.
MainProgressionSection persiste atos/fragmentos/finais.
FonteAnyaSection persiste funções/estágio/cooldowns/corrupção.
QuestState guarda quest formal, não todo o estado profundo.
Rewards/Flags desbloqueiam funções de forma idempotente.
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

- docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
- docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md

### Required interpretation

```text
Esta spec é derivada dos directions/refinements canônicos.
Ela não substitui os directions.
Ela transforma parte do refinement em contrato implementável, com escopo, locks, validações e quality gate.
```

---

## Future Hook Execution Policy

```text
Execution priority: DEFERRED unless explicitly promoted by roadmap/registry final.
This spec may create only safe hooks, validators, projection contracts, or adapter seams.
It must not implement the full future feature.
It must not appear to the player as a broken runtime promise.
It must not change save schema unless a dedicated save/migration spec exists.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Fonte/Main objectives: UpgradeFonte, UnlockFonteFunction, UseLivingWater, ProtectFragment, SealFragment, UseFragment, MakeFinalChoice.
- Fragmentos Água, Memória, Vida, Esperança.
- Funções da Fonte: respawn, Água Viva, respec, purificação/cura avançada, decisão final.
- QuestState, MainProgression e FonteAnya são seções separadas.
- Anya não deve ser revelada cedo como NPC direto; aparece por Fonte, ecos, sonhos, águas e fragmentos.

### Deferred / future from directions

- Main quest completa.
- Cutscenes/timelines.
- Boss final e nível 101.
- Fonte UI final.
- Água Viva full gameplay.
- Respec/purification final.

### Explicitly not redefined here

- Quest system base.
- Save schema final das seções.
- Skill tree respec runtime.
- Death/respawn system.
- Fonte visual/art.

## 4. Estado atual do repo

```text
Quest/Fonte/Main progression ainda estão em desenho/refinement.
SkillTree/respec pela Fonte já aparece como parcial em specs antigas.
Save direction exige FonteAnyaSection e MainProgressionSection futuras.
```

A confirmar localmente:

```text
FonteManager/FonteAnya runtime existente;
SkillTree respec hooks;
Death/respawn via Fonte;
MainProgression fields;
QuestFlags relacionados;
GameSaveData sections existentes.
```

---

## 5. User stories

```text
Como jogador, quero que a Fonte desbloqueie funções por progressão sem spoiler.
Como quest system, quero acionar unlocks sem persistir todo Fonte state.
Como save/load, quero sections separadas para QuestState, MainProgression e FonteAnya.
Como UI, quero mostrar apenas funções desbloqueadas.
Como lore, quero revelar Anya gradualmente.
```

---

## 6. Escopo

```text
hook contracts para UnlockFonteFunction, Protect/Seal/UseFragment;
Quest reward/flag integration;
MainProgression/FonteAnya section reference;
visibility/spoiler policy;
validators para quest que tenta gravar Fonte em QuestState;
execution report.
```

---

## 7. Fora de escopo

```text
Implementar main quest full;
diálogos;
bosses;
cutscenes;
Fonte UI final;
Água Viva economy/full use;
respec implementation final;
purification runtime;
final choice runtime.
```

---

## 8. Regras de não duplicação

```text
Não persistir Fonte inteira dentro de QuestState.
Não persistir MainProgression como QuestFlag solta.
Não revelar final choice cedo.
Não desbloquear função da Fonte fora do reward/idempotency pipeline.
Não duplicar respec state se SkillTree já controla parte dele.
```

---

## 9. Critérios de aceite

- Hooks de quest para Fonte/Main definidos.
- Unlocks idempotentes.
- FonteAnya/MainProgression separados de QuestState.
- Visibility evita spoilers de fragmentos/final.
- Validators detectam quest critical sem anti-softlock ou section separation.
- Report documenta future gaps.

---

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/Quests/Rewards/FonteQuestRewards.cs
Assets/_Game/Scripts/Quests/Conditions/FonteQuestConditions.cs
Assets/_Game/Scripts/Quests/Triggers/FonteQuestTriggers.cs
Assets/_Game/Scripts/Quests/Validation/FonteMainProgressionQuestValidator.cs
Assets/_Game/Tests/EditMode/Quests/FonteMainProgressionHookTests.cs
```

---

## 11. Contratos

### Data

```text
Quest references FonteStageId/FragmentId by stable ID.
MainProgression owns acts/fragments/final-choice progress.
FonteAnya owns Fonte functions/stage/cooldowns/corruption.
```

### Runtime

```text
Quest reward asks Fonte/Main progression service to unlock/apply.
Service owns state and idempotency with reward pipeline.
```

### Save

```text
QuestState does not own Fonte state.
MainProgressionSection and FonteAnyaSection are future/required save sections.
```

### UI

```text
Fonte UI shows only unlocked functions.
Quest Log hides spoiler stages.
```

---

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/Quests/**
Assets/_Game/Tests/EditMode/Quests/**
Assets/_Game/Scripts/Editor/Validation/**
docs/validation/03_spec_quest_fonte_main_progression_hooks_future_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/Fonte/**
Assets/_Game/Scripts/Progression/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Skills/**
```

---

## 13. Arquivos proibidos

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

## 14. Estratégia

```text
1. Auditar Fonte/Main/SkillTree/Save existentes.
2. Se runtime Fonte existir, criar hooks residuais.
3. Se não existir, criar contratos/future interfaces sem gameplay final.
4. Integrar reward idempotency.
5. Criar validators para section separation/spoiler.
6. Criar report.
```

---

## 15. Ordem segura

```text
Quest contracts -> Reward idempotency -> QuestFlags -> Save sections -> Fonte/Main hooks.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with:
  - main quest runtime;
  - Fonte UI/runtime;
  - save section schema;
  - skill tree respec changes.
- Reason:
  - Hooks to central progression/lore systems.

---

## 17. Impacto save/load

```text
Does this change save schema? SHOULD BE NO unless creating sections; then STOP for save spec.
Does this add save section? NO in this hook spec.
Does this require migration? NO unless payload changes.
Does this persist Unity references? NO.
```

---

## 18. Impacto eventos

```text
Adds events: CONDITIONAL, only generic Fonte/Main progression events if absent.
Changes existing events: SHOULD BE NO.
Requires unsubscribe pattern: YES if subscribers are created.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: NO final.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: DEFERRED_TO_FINAL_VALIDATION for integrated Fonte unlock flow.
```

---

## 20. Riscos

```text
Risco: QuestState virar main progression.
Mitigação: section separation.

Risco: spoiler.
Mitigação: VisibilityPolicy and fragment gates.

Risco: duplicate unlock.
Mitigação: idempotency.
```

---

## 21. Rollback

```text
Remover hooks/validators/tests/report.
Sem alteração em Fonte gameplay final.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar Fonte/Main/SkillTree/Save.
- [ ] T003 — Definir hooks.
- [ ] T004 — Integrar reward/flag idempotente.
- [ ] T005 — Criar validators/tests.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Escopo | A execução ficou dentro de Fonte/MainProgression quest hooks? | Arquivos alterados e justificativa. | PARTIAL |
| Save/load | Houve schema change? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/03_spec_quest_fonte_main_progression_hooks_future_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "Fonte|Anya|MainProgression|Fragment|LivingWater|Respec|Purification|FinalChoice|Quest|Reward" Assets/_Game/Scripts docs/design docs/specs
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
Given o sistema base relacionado a Fonte/MainProgression quest hooks existe ou foi criado de forma mínima
When o fluxo principal desta spec é executado
Then o resultado segue o direction canônico
And nenhum sistema paralelo é criado
And o execution report registra evidência.
```

### Scenario 2 — Existing implementation is found

```text
Given existe implementação parcial ou completa no repo
When a execução audita o estado real
Then ela muda para REUSE_EXISTING ou HARDEN_EXISTING
And não recria arquitetura paralela
And documenta residual/future gaps.
```

### Scenario 3 — Missing dependency

```text
Given uma dependência runtime não existe no repo local
When a execução encontra essa ausência
Then ela não inventa uma solução massiva
And marca como MISSING_BUT_DEFER ou cria apenas adapter/validator mínimo se seguro
And registra risco residual.
```

### Scenario 4 — Save/load safety

```text
Given o fluxo envolve estado persistido direta ou indiretamente
When save/load ocorre após a ação
Then nenhum dado derivado de UI/debug substitui a fonte de verdade
And nenhum UnityEngine.Object é persistido
And schema change exige spec/migration separada.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige interação visual, PlayMode ou gameplay integrado
When a spec é concluída tecnicamente
Then o report registra cenário final em vez de pedir validação humana imediata
And o status máximo respeita SPEC_VALIDATION_MATRIX_MASTER.md.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Persistir Fonte inteira dentro de QuestState.
- Usar QuestFlag como MainProgression state.
- Revelar Anya/final/nível 101 cedo.
- Desbloquear respec/Água Viva/purificação duas vezes.
- Criar Fonte runtime completo fora do escopo.
- Alterar save schema sem spec/migration.
- Misturar skill respec backend com UI/hook future.
- Future hook tratado como feature final.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Quest Fonte/Main Progression Hooks Future

## Summary
- Spec:
- Wave: WAVE 03
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
- Existing implementation handling:
- Missing dependency:
- Edge cases:
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

Parar a execução e registrar `BLOCKED` se ocorrer qualquer um destes casos:

```text
1. A implementação exigir alterar Packages/ ou ProjectSettings/.
2. A implementação exigir scene/prefab/asset wiring fora do escopo.
3. A implementação exigir mudança de save schema sem migration spec.
4. A implementação exigir reescrever sistema canônico existente.
5. A implementação criar conflito com 01Q, input focus, save ownership ou registry.
6. A implementação executar WAVE 02+ em massa antes da 01Q ou exceção humana explícita.
7. Não for possível decidir se sistema existente é canônico ou obsoleto.
```

## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Quest|Objective|Condition|Trigger|Reward|Flag|FarmOrder|Festival|Bestiary|Fonte|MainProgression" Assets/_Game/Scripts docs/design docs/specs
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

- Changed deterministic logic: YES if hook/reward/validator logic is implemented.
- Requires EditMode tests: YES for idempotency/visibility/section separation tests when available.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for integrated Fonte unlock scenario.
- Requires regression test: YES if fixing known duplicate unlock/spoiler bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver cenário integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode hook/idempotency tests PASS or NOT RUN justified; no save schema change; no early spoiler; report created.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/03_spec_quest_fonte_main_progression_hooks_future_execution_report.md.
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
Não revelar spoilers antes de discovery/visibility policy.
Não pedir human test por spec.
Não executar runtime WAVE 03 em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
