# SPEC — UI Social NPC Detail Future Runtime

> **Spec ID:** `04_spec_ui_social_npc_detail_future_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 04 — UI / UX Foundation  
> **Priority:** P2 / Future Hook  
> **Type:** Runtime Hook / UI / Social / NPC Detail / Future  
> **Domain:** UI / Social Log / NPC Detail / Future Hooks / Anti-spreadsheet  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_04_UI_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que implemente RelationshipSystem, friendship/gift/romance runtime, companion runtime, pet runtime, NPC schedule runtime ou social save schema.  
> **Repo lock scope:** `Assets/_Game/Scripts/UI/**`, `Assets/_Game/Scripts/NPC/**`, `Assets/_Game/Scripts/Social/**`, `Assets/_Game/Tests/EditMode/UI/**`, `docs/validation/04_spec_ui_social_npc_detail_future_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
  - `docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md`
  - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md`
  - `docs/design/gameplay/pets/PETS_DIRECTION.md`
  - `docs/specs/a_implementar/04_spec_ui_input_focus_modal_routing_runtime.md`
> **Blocks:**  
  - future RelationshipSystem;
  - future Social Log;
  - future companion UI;
  - future pet UI;
  - NPC roster/schedule UI integration.
> **Scope:** preparar Social/NPC Detail Screen future hook, exibindo apenas dados conhecidos e evitando transformar NPC em planilha ou implementar social runtime agora.  
> **Out of scope:** RelationshipSystem runtime, romance/casamento/poliamor runtime, gifts completos, companion runtime, pet runtime, NPC schedules finais, social save/load.

---

# /speckit.specify

## 1. Contexto

O direction de menu reserva Social/NPC Detail Screen future. O NPC card futuro pode mostrar nome, retrato, ocupação/serviço, local conhecido, horário geral conhecido, estado social geral, preferências descobertas, presente dado hoje/semana, quest pessoal, romance eligibility quando descoberto, partner status future e companion eligibility quando descoberto.

O direction social é explícito: esta feature não entra nas specs executáveis atuais. Ela serve para não bloquear estados futuros e não transformar romance/casamento em caminho obrigatório de poder.

O direction de pets é ainda mais restritivo: pets estão deferidos e não devem gerar specs/runtime/UI/save/load agora.

---

## 2. Problema

Sem um hook seguro de Social/NPC Detail:

```text
UI pode revelar preferências e romance cedo demais;
NPC pode virar planilha completa;
romance pode parecer caminho obrigatório de poder;
companion/pet/social podem se misturar;
campos futuros podem entrar no save incompletos;
feature future pode aparecer em runtime final como promessa quebrada.
```

---

## 3. Objetivo

Criar apenas contrato UI/future hook:

```text
mostrar NPC conhecido sem planilha;
mostrar dados sociais conhecidos, se existirem;
ocultar preferências/romance/companion eligibility não descobertos;
não implementar RelationshipSystem;
não implementar pets;
não persistir social state incompleto;
não tornar romance/casamento/companion obrigatório.
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

- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
- docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md
- docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
- docs/design/gameplay/pets/PETS_DIRECTION.md

### Required interpretation

```text
Esta spec é derivada dos directions/refinements canônicos.
Ela não substitui os directions.
Ela transforma parte do refinement em contrato implementável com escopo, locks, validações e quality gate.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Social/NPC Detail Screen é future.
- NPC card pode mostrar nome, retrato, ocupação/serviço, local conhecido, horário geral, estado social geral e preferências descobertas.
- Não mostrar NPC como planilha completa.
- Preferências são descobertas gradualmente.
- Romance/casamento são opcionais e não caminho obrigatório de poder.
- Companion é sistema separado de romance e pet.
- Pets estão explicitamente deferidos e não entram em specs/runtime/UI/save agora.

### Deferred / future from directions

- RelationshipSystem runtime.
- Friendship UI completa.
- Gift runtime.
- Romance/casamento/poliamor.
- Partner helper automation.
- Companion runtime/UI completo.
- Pet runtime/UI completo.
- Social save/load.

### Explicitly not redefined here

- NPC roster concreto.
- NPC schedule final.
- Companion system.
- Pet system.
- Economy/shop services.
- Quest/social quest runtime.

## 4. Estado atual do repo

```text
Social system é future direction.
Pets estão deferidos.
Companions podem ter roadmap próprio, mas não devem ser misturados com romance/pets.
Esta spec só prepara UI-safe hooks.
```

A confirmar localmente:

```text
NPC data/roster;
NPC schedule known fields;
existing UI Social/Journal tabs;
companion eligibility fields, se houver;
pet UI references, se houver.
```

---

## 5. User stories

```text
Como jogador, quero ver informações conhecidas de um NPC sem receber spoiler.
Como jogador, não quero que romance seja apresentado como progressão obrigatória.
Como UI, quero reservar espaço para future relationship sem implementar runtime.
Como dev, quero impedir que pet/companion/social se misturem indevidamente.
```

---

## 6. Escopo

```text
NPC detail future projection;
known/unknown social fields;
anti-spoiler rules;
future-hidden states;
no-pets/no-relationship-runtime guardrails;
tests/validators.
```

---

## 7. Fora de escopo

```text
RelationshipSystem;
gift runtime;
romance/casamento;
companion runtime;
pet runtime;
social save/load;
NPC schedules finais.
```

---

## 8. Regras de não duplicação

```text
Não criar RelationshipSystem.
Não criar Pet UI/runtime.
Não criar Companion UI/runtime.
Não persistir social state parcial.
Não revelar romance eligibility antes da descoberta.
Não transformar NPC em spreadsheet.
```

---

## 9. Critérios de aceite

- NPC detail mostra apenas dados conhecidos.
- Future fields não aparecem como promessa quebrada.
- Pets não entram no runtime.
- Companion/romance/pet separados.
- Social runtime não é criado.
- Report inclui future gaps.

---

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/UI/Social/NpcDetailFutureViewModel.cs
Assets/_Game/Scripts/UI/Social/SocialFieldVisibilityPolicy.cs
Assets/_Game/Tests/EditMode/UI/NpcDetailFutureProjectionTests.cs
```

Criar apenas se não houver projection equivalente e se for seguro.

---

## 11. Contratos

### Runtime

```text
Projection is read-only.
Unknown/future social fields are hidden or shown as undiscovered only when allowed.
```

### Save

```text
No save schema change.
No social state persistence in this spec.
```

### UI

```text
Social future fields must not become runtime promise.
```

---

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/UI/**
Assets/_Game/Tests/EditMode/UI/**
Assets/_Game/Scripts/Editor/Validation/**
docs/validation/04_spec_ui_social_npc_detail_future_runtime_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/NPC/**
Assets/_Game/Scripts/Social/**
Assets/_Game/Scripts/Companions/**
Assets/_Game/Scripts/Pets/**
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
1. Auditar NPC/social/companion/pet references.
2. Se social runtime não existe, criar apenas projection/future policy se útil.
3. Garantir pets remain deferred.
4. Add tests for known/unknown/future field projection.
5. Report future gaps.
```

---

## 15. Ordem segura

```text
NPC roster/schedule -> future social projection -> relationship runtime future.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with social/companion/pet runtime specs.
- Reason: future hooks cannot drift into runtime features.

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
Adds events: NO.
Changes events: NO.
Requires unsubscribe pattern: NO unless UI subscribers are added.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: CONDITIONAL projection only.
Scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: NO unless Social tab is wired; then DEFERRED.
```

---

## 20. Riscos

```text
Risco: implementar social runtime sem querer.
Mitigação: future hook only.

Risco: pet entrar no escopo.
Mitigação: pets explicitly deferred.

Risco: romance virar poder obrigatório.
Mitigação: optional/future visibility.
```

---

## 21. Rollback

```text
Remover projection/policy/tests/report.
Sem alteração de social save/runtime.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar NPC/social/companion/pet references.
- [ ] T003 — Criar projection/future policy apenas se seguro.
- [ ] T004 — Criar tests.
- [ ] T005 — Rodar validações.
- [ ] T006 — Criar report.

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
| Report | Execution report criado? | `docs/validation/04_spec_ui_social_npc_detail_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar:

```bash
rg -n "Social|Relationship|NPC|NpcDetail|Friendship|Romance|Marriage|Companion|Pet|Gift|Schedule|KnownPreference" Assets/_Game/Scripts docs/design docs/specs
rg -n "TODO|FIXME|HACK|PARTIAL|DEFERRED|BUILD_VALIDATED|ACCEPTED" docs/specs docs/validation docs/IMPLEMENTATION_STATUS.md docs/project/CURRENT_STATE.md
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
Given a tela/flow de social/NPC detail future projection é aberto com dependências válidas
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

### Scenario 3 — Empty / unavailable / blocked state

```text
Given não há dados disponíveis ou requisito está ausente
When a tela abre ou item/entry/função/dia é selecionado
Then a UI mostra empty state ou motivo bloqueado
And nenhuma ação inválida é executada
And o report registra o estado esperado.
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

- Romance eligibility revelada cedo.
- Preferência de presente revelada sem descoberta.
- NPC card vira planilha completa.
- Pet UI/runtime criado apesar de deferido.
- Companion e romance misturados indevidamente.
- Social field persistido em save sem schema.
- Feature future aparece em runtime final como promessa quebrada.
- RelationshipSystem criado fora do escopo.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — UI Social NPC Detail Future Runtime

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
- Empty/unavailable:
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


## 23G. Social Field Visibility Matrix

| Field | Estado inicial | Quando mostrar |
|---|---|---|
| Name | Known if NPC discovered | Após encontrar/conhecer NPC |
| Portrait | Known if NPC discovered | Após encontrar/conhecer NPC |
| Occupation/service | Known if service discovered | Quando NPC/serviço for conhecido |
| Known location | Known/Unknown | Só local descoberto |
| General schedule | Unknown/Partial | Gradual, sem planilha completa |
| Relationship state | Hidden/Future | Apenas quando RelationshipSystem existir |
| Preferences | Unknown | Somente preferências descobertas |
| Romance eligibility | Hidden | Somente se explicitamente descoberto/permitido |
| Companion eligibility | Hidden/Future | Somente se companion system existir |
| Pet fields | Hidden/Not applicable | Pets não entram nesta spec |

## 23H. Future Feature Guardrails

```text
Social tab may reserve structure, not promise runtime.
RelationshipSystem is not created here.
Pet runtime/UI is blocked.
Companion runtime/UI is blocked unless explicit future hook read-only.
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Social|NPC|Calendar|Fonte|Confirmation|EmptyState|BlockedState|Focus|Modal|Known|Hidden|Future" Assets/_Game/Scripts docs/design docs/specs
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

- Changed deterministic logic: YES if projection/visibility policy logic is created.
- Requires EditMode tests: YES for known/unknown/future field projection tests.
- Requires PlayMode automated or final human scenario: NO unless Social tab is wired; if wired, DEFERRED.
- Requires regression test: YES if fixing known spoiler/social field bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver cenário integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode projection tests PASS or NOT RUN justified; no social runtime; pets remain deferred.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/04_spec_ui_social_npc_detail_future_runtime_execution_report.md.
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
