# SPEC — Quest Bestiary Discovery Objectives Future Hook

> **Spec ID:** `03_spec_quest_bestiary_discovery_objectives_future`  
> **Status:** A implementar  
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
