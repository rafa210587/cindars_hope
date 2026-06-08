# SPEC — Quest Bestiary Discovery Objectives Future Hook

> **Spec ID:** `03_spec_quest_bestiary_discovery_objectives_future`  
> **Status:** A implementar  
> **Revision:** EXPANDED_01_05_CORRECTED  
> **Execution priority:** DEFERRED / Future Hook  
> **Wave:** WAVE 03 — Quest / Objective / Event System  
> **Priority:** P2 / Future Hook  
> **Type:** Runtime Hook / Quest / Bestiary / Knowledge Discovery  
> **Domain:** Quests / Bestiary / Knowledge / Anti-spoiler  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_03_QUEST_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere BestiaryManager, EnemyKnowledgeState, enemy vulnerability data, QuestState, Quest Log visibility ou combat discovery events.  
> **Repo lock scope:** `Assets/_Game/Scripts/Quests/**`, `Assets/_Game/Scripts/Bestiary/**`, `Assets/_Game/Scripts/Enemies/**`, `docs/validation/03_spec_quest_bestiary_discovery_objectives_future_execution_report.md`  
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
  - `docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
  - `docs/specs/a_implementar/03_spec_quest_objective_event_contract_runtime.md`
  - `docs/specs/a_implementar/03_spec_quest_log_visibility_spoiler_runtime.md`
> **Blocks:**  
  - future bestiary runtime/UI;
  - enemy vulnerability discovery;
  - quest visibility/spoiler rules;
  - equipment/spell tooltip knowledge gates.
> **Scope:** preparar hooks futuros para objectives de descoberta de bestiário/conhecimento, sem implementar UI completa de bestiário.  
> **Out of scope:** Bestiary UI completa, Knowledge Log navegável, NPC research service, enemy stats final, vulnerability balance, boss spoiler data final.

---

# /speckit.specify

## 1. Contexto

O direction de Bestiary define que a primeira entrega não inclui UI completa de bestiário, mas permite preparar IDs estáveis, tags, campos vazios em save/load, hooks de evento e contratos conceituais.

O direction de Quest define objective types como `DiscoverBestiaryKnowledge`, `DiscoverWeakness`, `ReadLoreNote`, `ConfirmRumor`, `DocumentBehavior` e rewards como `KnowledgeUnlock`/`BestiaryEntryUnlock`.

Esta spec cria apenas o hook de quest para discovery, sem prometer tela final.

---

## 2. Problema

Sem hook de quest/bestiary:

```text
quest de pesquisa pode revelar spoiler cedo;
quest pode pedir fraqueza que bestiary não controla;
UI de equipamento/spell pode expor vulnerabilidade sem discovery;
reward de conhecimento pode ser duplicado;
save/load pode perder discovered knowledge;
boss/main quest podem ter spoiler por família.
```

---

## 3. Objetivo

Definir contratos para objectives futuras baseadas em knowledge discovery:

```text
Quest pode observar Bestiary/Knowledge events.
Quest não cria vulnerabilidades.
Quest não revela knowledge não descoberto.
Reward KnowledgeUnlock usa Bestiary/Knowledge service.
Quest Log mostra pistas, não respostas ocultas.
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
- docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md

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

- Objective types DiscoverBestiaryKnowledge, DiscoverWeakness, ReadLoreNote.
- Reward types KnowledgeUnlock and BestiaryEntryUnlock.
- Bestiary revela vulnerabilidades já existentes; não cria stats.
- Knowledge states Unknown/Seen/Fought/Defeated/Studied/FullyDocumented.
- UI só apresenta conhecimento conhecido.
- Boss/main quest têm spoiler control mais forte.

### Deferred / future from directions

- Bestiary UI completa.
- Knowledge Log navegável.
- NPC research services.
- EnemyBestiaryEntrySO final.
- Full EnemyKnowledgeState runtime.
- Achievements/collections.

### Explicitly not redefined here

- Enemy stats.
- Vulnerability balance.
- Equipment matching.
- Combat discovery runtime completo.
- Quest Log visual final.

## 4. Estado atual do repo

```text
Audit report indica BestiaryManager event-driven/save já existe parcialmente.
Bestiary UI/research service ainda future.
Quest system ainda está em WAVE 03.
```

A confirmar localmente:

```text
BestiaryManager;
GameSaveData.Bestiary;
EnemyKnowledgeState;
knowledge events;
EnemyId/FamilyId/VulnerabilityTag;
Quest visibility projection.
```

---

## 5. User stories

```text
Como jogador, quero que quest de pesquisa registre o que descobri sem revelar resposta cedo.
Como bestiary, quero controlar conhecimento conhecido.
Como quest, quero pedir discovery sem duplicar o knowledge system.
Como UI, quero mostrar rumor/pista sem spoiler.
Como save/load, quero preservar conhecimento desbloqueado.
```

---

## 6. Escopo

```text
Quest objective contracts for knowledge discovery;
condition/trigger hooks to bestiary events;
KnowledgeUnlock reward integration;
spoiler-safe visibility projection;
save/load compatibility through existing bestiary and QuestState sections;
validator for knowledge objective references.
```

---

## 7. Fora de escopo

```text
Bestiary UI final;
enemy stats/vulnerability authoring;
combat discovery algorithm final;
NPC research lab;
full Knowledge Log;
boss knowledge content.
```

---

## 8. Regras de não duplicação

```text
Quest não cria BestiaryManager.
Quest não salva Bestiary data dentro de QuestState.
Quest não revela vulnerability tag desconhecida.
Quest não define enemy stats.
Quest reward chama knowledge unlock service, não duplica state.
```

---

## 9. Critérios de aceite

- DiscoverBestiaryKnowledge objective pode observar knowledge events.
- DiscoverWeakness objective usa EnemyId/FamilyId/VulnerabilityTag estável.
- KnowledgeUnlock reward é idempotente.
- Quest Log visibility respeita knowledge known/unknown.
- Future hooks não prometem UI final.
- Execution report lista future gaps.

---

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/Quests/Conditions/BestiaryQuestConditions.cs
Assets/_Game/Scripts/Quests/Triggers/BestiaryQuestTriggers.cs
Assets/_Game/Scripts/Quests/Rewards/KnowledgeQuestRewards.cs
Assets/_Game/Scripts/Quests/Validation/BestiaryQuestObjectiveValidator.cs
Assets/_Game/Tests/EditMode/Quests/BestiaryQuestObjectiveTests.cs
```

---

## 11. Contratos

### Data

```text
Quest Objective references EnemyKnowledgeKey / EnemyId / EnemyFamilyId / VulnerabilityTag by stable ID.
```

### Runtime

```text
Bestiary/Knowledge system owns known state.
Quest consumes discovery events and applies objectives.
```

### Save

```text
Bestiary knowledge persists in Bestiary section.
Quest objective progress persists in QuestState.
```

### UI

```text
Quest Log shows only authorized hints.
```

---

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/Quests/**
Assets/_Game/Tests/EditMode/Quests/**
Assets/_Game/Scripts/Editor/Validation/**
docs/validation/03_spec_quest_bestiary_discovery_objectives_future_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/Bestiary/**
Assets/_Game/Scripts/Enemies/**
Assets/_Game/Scripts/Combat/**
Assets/_Game/Scripts/UI/**
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
1. Auditar BestiaryManager/knowledge state existente.
2. Se não houver runtime suficiente, criar contratos/hooks future-only.
3. Integrar Quest objective apenas por interface/event.
4. Adicionar validators para references e spoiler flags.
5. Criar report com future gaps.
```

---

## 15. Ordem segura

```text
Quest contracts -> Quest visibility -> Bestiary hooks -> future UI.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with:
  - bestiary runtime changes;
  - enemy data/vulnerability changes;
  - quest visibility;
  - reward idempotency.
- Reason:
  - Cross-domain spoiler and knowledge state.

---

## 17. Impacto save/load

```text
Does this change save schema? SHOULD BE NO.
Does this add save section? NO.
Does this require migration? NO unless new knowledge payload is added.
Does this persist Unity references? NO.
```

---

## 18. Impacto eventos

```text
Adds events: CONDITIONAL, only knowledge discovery events if absent and generic.
Changes existing events: SHOULD BE NO.
Requires unsubscribe pattern: YES if subscribers created.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: NO final.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: NOT REQUIRED unless integrated cave discovery scenario added; then DEFERRED.
```

---

## 20. Riscos

```text
Risco: revelar fraqueza cedo.
Mitigação: VisibilityPolicy + Bestiary known state.

Risco: duplicar knowledge state.
Mitigação: Bestiary owns knowledge.

Risco: future hook virar promessa de UI final.
Mitigação: marcar future-only no report.
```

---

## 21. Rollback

```text
Remover hooks/conditions/rewards/tests/report.
Sem alteração em enemy stats ou bestiary UI.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar bestiary/knowledge runtime.
- [ ] T003 — Definir objective hooks.
- [ ] T004 — Definir reward hook.
- [ ] T005 — Criar validators/tests quando praticável.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar execution report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Escopo | A execução ficou dentro de bestiary quest discovery hooks? | Arquivos alterados e justificativa. | PARTIAL |
| Save/load | Houve schema change? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/03_spec_quest_bestiary_discovery_objectives_future_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "Bestiary|Knowledge|DiscoverWeakness|DiscoverBestiary|EnemyKnowledge|Vulnerability|Quest|Reward" Assets/_Game/Scripts docs/design docs/specs
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
Given o sistema base relacionado a bestiary quest discovery hooks existe ou foi criado de forma mínima
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

- Revelar vulnerabilidade desconhecida.
- Salvar Bestiary state dentro de QuestState.
- Criar BestiaryManager paralelo.
- Reward de knowledge duplicado.
- UI final de bestiary entrar no escopo indevidamente.
- Enemy stats alterados pela quest.
- Boss/main quest spoiler por knowledge projection.
- Future hook tratado como feature final.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Quest Bestiary Discovery Objectives Future Hook

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

- Changed deterministic logic: YES if quest/bestiary hook logic is implemented.
- Requires EditMode tests: YES for deterministic objective/reward/visibility logic when harness is available.
- Requires PlayMode automated or final human scenario: NO for future hook only; DEFERRED if integrated cave discovery scenario is added.
- Requires regression test: YES if fixing known spoiler/duplicate knowledge bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver cenário integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode hook/visibility tests PASS or NOT RUN justified; no Bestiary UI promise; no knowledge spoiler leak.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/03_spec_quest_bestiary_discovery_objectives_future_execution_report.md.
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
