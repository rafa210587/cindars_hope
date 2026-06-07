# SPEC — Validation Matrix Master

> **Spec ID:** `00_spec_validation_matrix_master`  
> **Status:** A implementar  
> **Wave:** WAVE 00 — Roadmap / governança  
> **Priority:** P0  
> **Type:** Governance / Validation / Documentation  
> **Domain:** Specs / Validation / Quality Gate  
> **Parallelizable:** CONDITIONAL  
> **Parallel group:** WAVE_00_GOVERNANCE  
> **Can run with:** `00_spec_wave_execution_protocol.md`, desde que não editem os mesmos arquivos finais no mesmo commit.  
> **Must not run with:** specs runtime das Waves 01+ que dependam da matriz ainda não publicada.  
> **Repo lock scope:** `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`, `docs/specs/README.md`, `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`.  
> **Depends on:**  
> - `docs/specs/SPEC_GENERATION_ROADMAP_MASTER.md`  
> - `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`  
> - `docs/design/SPECIFICATION_PROCESS.md`  
> - `.claude/rules/testing-quality-gate.md`  
> - `docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md`  
> **Blocks:** critérios consistentes de aceite por tipo de spec e execução segura das specs WAVE 01+.  
> **Scope:** criar uma matriz canônica que mapeia tipo de mudança para validação obrigatória, evidência mínima, status máximo e validação humana final.  
> **Out of scope:** implementar testes, criar harness Unity, alterar runtime, alterar cenas ou validar Play Mode humano agora.

---

# /speckit.specify

## 1. Contexto

O projeto já possui roadmap macro de specs, template canônico, regra Testing Quality Gate e checklist humano final por wave. Ainda falta uma matriz central que diga, por tipo de mudança, quais validações são obrigatórias, qual evidência mínima precisa existir e qual status máximo é permitido se alguma validação não rodou.

---

## 2. Problema

Sem matriz master, agentes podem interpretar o quality gate de forma diferente: spec docs-only tentando rodar Unity sem necessidade, spec runtime sendo aceita só com compile, spec de save sem round-trip ou justificativa, spec de UI sem cenário final humano documentado, bugfix sem regressão ou risco residual, e reports inconsistentes entre agentes.

---

## 3. Objetivo

Ao final desta spec, o projeto deve ter `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`, permitindo classificar uma spec, determinar validações obrigatórias, registrar status correto e saber quando a validação humana final entra.

---

## 4. Fontes obrigatórias lidas

```text
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
docs/specs/SPEC_GENERATION_ROADMAP_MASTER.md
docs/specs/SPEC_SOURCE_OF_TRUTH.md
docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md
docs/project/CURRENT_STATE.md
CLAUDE.md
.claude/skills/spec-execution/SKILL.md
.claude/skills/unity-validation/SKILL.md
.claude/rules/testing-quality-gate.md
docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
docs/refinements/a_implementar/ref_futuro_map.md
```

Esta spec é de governança/validação e não precisa ler direction de gameplay específico. Os refinamentos vivos atuais são runtime/domínio específico e continuam como futuros consumidores da matriz.

---

## 5. Estado atual do repo

Validado durante a geração:

```text
- `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md` exige Testing Quality Gate em toda spec nova.
- `.claude/rules/testing-quality-gate.md` define que compile não basta para ACCEPTED em runtime/gameplay.
- `.claude/skills/spec-execution/SKILL.md` já contém taxonomia BUILD_VALIDATED, UNITY_VALIDATED, PLAYMODE_VALIDATED, ACCEPTED, PARTIAL e BLOCKED.
- `docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md` define validação humana final por wave e impede validação humana intermediária spec a spec.
- `docs/project/CURRENT_STATE.md` registra que Phase 2/3 Play Mode ainda depende de execução humana local.
```

Esta spec não deve recriar essas fontes. Deve criar uma matriz de decisão que aponte para elas.

---

## 6. User stories / engineering stories

```text
Como agente executor, quero saber quais validações rodar a partir do tipo de mudança.
Como maintainer, quero impedir ACCEPTED sem evidência suficiente.
Como agente executor, quero saber o status máximo quando uma validação não rodou.
Como maintainer, quero que bugfix tenha regression test ou risco residual documentado.
Como usuário humano, quero validar Play Mode só no final do lote/wave.
Como revisor, quero uma matriz única para auditar reports inconsistentes.
```

---

## 7. Escopo

Inclui:

```text
- criar `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`;
- mapear tipos de spec para validações obrigatórias;
- definir evidência mínima por tipo de mudança;
- definir status máximo quando validação está ausente;
- definir quando usar docs validation, dotnet build, Unity compile, log scan, EditMode, PlayMode automated e cenário final humano;
- definir como registrar NOT RUN, BLOCKED e residual risk;
- definir relação com FINAL_HUMAN_VALIDATION_BY_WAVE.md;
- definir regras para bugfix/regression.
```

---

## 8. Fora de escopo

Não inclui:

```text
- criar testes automatizados;
- criar ou ajustar asmdef;
- alterar tools de validação;
- alterar runtime;
- alterar scenes/prefabs/assets;
- executar Unity;
- executar validação humana;
- promover specs para implementados;
- alterar SPEC_EXECUTION_ORDER.md.
```

---

## 9. Regras de não duplicação

```text
Não duplicar integralmente .claude/rules/testing-quality-gate.md.
Não duplicar integralmente FINAL_HUMAN_VALIDATION_BY_WAVE.md.
Não redefinir taxonomia da spec-execution skill de forma incompatível.
Não criar outro template de execution report.
Não criar matriz separada por domínio antes desta matriz master.
```

---

## 10. Critérios de aceite

### 10.1 Documento canônico criado

- Existe `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`.
- O documento declara que é matriz de validação, não spec implementável.
- O documento aponta para Testing Quality Gate, template e checklist final humano.

### 10.2 Matriz por tipo de mudança

A matriz cobre no mínimo:

```text
docs-only/governance;
data-only docs/spec registry;
pure C# deterministic logic;
runtime manager/service;
save/load schema/provider;
event contracts;
UI/menu/modal/HUD;
scene/prefab/asset wiring;
quest/objective/reward;
economy/inventory/equipment;
combat/status/skill rules;
cave procedural/runtime;
bugfix;
asset-only/import-only.
```

### 10.3 Evidência mínima

Para cada tipo, a matriz define:

```text
docs validation required? YES/NO;
dotnet build required? YES/NO;
Unity compile required? YES/NO;
EditMode tests required? YES/NO/WHEN PRACTICAL;
PlayMode automated or final human scenario required? YES/NO;
regression test required? YES/NO;
human validation timing;
minimum evidence for ACCEPTED;
maximum status if missing evidence.
```

### 10.4 Status caps

A matriz reforça:

```text
Docs-only pode ser ACCEPTED com docs validation PASS.
Runtime/gameplay não pode ser ACCEPTED só por build/compile.
Sem teste obrigatório nem justificativa, status máximo PARTIAL.
Runtime/gameplay sem PlayMode automated ou cenário final humano documentado fica no máximo BUILD_VALIDATED.
Validação humana real fica deferida ao final e não pode ser alegada antes de ocorrer.
```

---

# /speckit.plan

## 11. Arquitetura alvo

Documento alvo:

```text
docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md
```

Estrutura esperada:

```text
1. Objetivo
2. Fontes obrigatórias
3. Status taxonomy
4. Matriz resumida por tipo de mudança
5. Status caps
6. Como registrar NOT RUN
7. Como registrar residual risk
8. Como relacionar com final human validation
9. Como specs futuras devem preencher Testing Quality Gate
10. Checklist de report
```

---

## 12. Contratos, dados e eventos

### 12.1 Data contracts

N/A — docs-only.

### 12.2 Runtime contracts

N/A — não altera runtime.

### 12.3 Event contracts

N/A — não altera eventos.

### 12.4 Save contracts

N/A — não altera save.

### 12.5 UI contracts

N/A — não altera UI.

---

## 13. Sistemas afetados

```text
Docs governance;
Spec validation planning;
Testing quality gate;
Execution reports;
Final human validation planning;
Agent closeout discipline.
```

---

## 14. Arquivos permitidos

```text
docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md
docs/specs/README.md
docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md
```

---

## 15. Arquivos proibidos

```text
Assets/**
Packages/**
ProjectSettings/**
docs_old/**
docs/archive/**
docs/specs/implementados/**
docs/refinements/implementados/**
docs/specs/SPEC_EXECUTION_ORDER.md
PROJECT_LOG.md
docs/project/CURRENT_STATE.md
```

---

## 16. Estratégia de implementação

### Fase 0 — Auditoria

```text
1. Confirmar existência de Testing Quality Gate rule.
2. Confirmar existência de FINAL_HUMAN_VALIDATION_BY_WAVE.md.
3. Confirmar existência de SPEC_IMPLEMENTABLE_TEMPLATE.md.
4. Confirmar se SPEC_VALIDATION_MATRIX_MASTER.md já existe.
```

### Fase 1 — Criar matriz

```text
1. Criar documento master.
2. Definir matriz resumida.
3. Definir status caps.
4. Definir relação com reports e human validation final.
```

### Fase 2 — Atualização documental mínima

```text
1. Atualizar README se necessário.
2. Atualizar registry se necessário.
3. Não alterar SPEC_EXECUTION_ORDER.md.
```

### Fase 3 — Validação

```text
1. Rodar docs validation.
2. Registrar NOT RUN se não puder rodar.
3. Não rodar Unity compile porque é docs-only.
```

---

## 17. Ordem segura de execução

```text
1. Ler fontes obrigatórias.
2. Auditar documento existente.
3. Criar matriz master.
4. Atualizar README/registry se permitido.
5. Rodar docs validation.
6. Reportar status.
```

---

## 18. Paralelização

- Parallelizable: CONDITIONAL
- Parallel group: WAVE_00_GOVERNANCE
- Can run with:
  - `00_spec_wave_execution_protocol.md`, se não editar os mesmos arquivos compartilhados simultaneamente.
- Must not run with:
  - specs runtime que estejam tentando aplicar validações antes da matriz existir;
  - qualquer spec que edite Testing Quality Gate rule sem lock explícito;
  - qualquer spec que edite `FINAL_HUMAN_VALIDATION_BY_WAVE.md` sem lock explícito.
- Shared files/systems that require lock:
  - `docs/specs/README.md`
  - `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`
  - `.claude/rules/testing-quality-gate.md` se alguma correção for proposta
  - `docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md` se alguma correção for proposta
- Reason:
  - A matriz é docs-only, mas define regras usadas por todas as specs runtime futuras.

---

## 19. Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? NO
```

---

## 20. Impacto em eventos

```text
Adds events: NO
Changes existing events: NO
Requires unsubscribe pattern: NO
```

---

## 21. Impacto em UI/Unity

```text
Changes UI: NO
Changes scenes: NO
Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: NO
Human validation timing: NOT REQUIRED
```

---

## 22. Riscos técnicos

```text
Risco: matriz contradizer Testing Quality Gate.
Mitigação: matriz deve referenciar a rule como fonte superior para runtime/code.

Risco: matriz liberar ACCEPTED indevido.
Mitigação: status caps explícitos.

Risco: matriz voltar a exigir human test por spec.
Mitigação: usar DEFERRED_TO_FINAL_VALIDATION e checklist final por wave.

Risco: matriz virar redundante e divergente.
Mitigação: manter tabela de decisão e links, não duplicar todos os textos das regras.
```

---

## 23. Rollback

```text
Remover docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md.
Reverter README/registry se alterados.
Não tocar em código, save, scenes ou assets.
```

---

# /speckit.tasks

## 24. Tasks

- [ ] T001 — Auditar se `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md` já existe.
- [ ] T002 — Criar `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`.
- [ ] T003 — Adicionar matriz resumida por tipo de mudança.
- [ ] T004 — Adicionar status caps e evidência mínima.
- [ ] T005 — Adicionar regra de validação humana final deferida.
- [ ] T006 — Adicionar relação com execution reports.
- [ ] T007 — Atualizar `docs/specs/README.md` se necessário.
- [ ] T008 — Atualizar `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md` se necessário.
- [ ] T009 — Rodar docs validation ou registrar NOT RUN.
- [ ] T010 — Reportar arquivos alterados e pendências.

---

## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Não rodar Unity compile por esta spec ser docs-only.

---

## 26. Testing Quality Gate

- Changed deterministic logic: NO
- Requires EditMode tests: NO
- Requires PlayMode automated or final human scenario: NO
- Requires regression test: NO
- Human validation timing: NOT REQUIRED
- Minimum validation evidence for ACCEPTED: docs validation PASS; links/paths consistent; no runtime/code changed.

---

## 27. Definition of Done

```text
SPEC_VALIDATION_MATRIX_MASTER.md criado.
Matriz cobre docs-only, runtime, save, events, UI, scene/prefab, quest, economy, combat, cave, bugfix e asset-only.
Status caps compatíveis com Testing Quality Gate.
Validação humana final deferida preservada.
SPEC_EXECUTION_ORDER.md não alterado.
Nenhum arquivo Unity/runtime alterado.
Docs validation executada ou NOT RUN com justificativa.
```

---

## 28. Anti-regressão

```text
Não aceitar runtime só por compile.
Não pedir Play Mode humano por spec.
Não duplicar Testing Quality Gate inteiro.
Não criar regra conflitante com spec-execution skill.
Não alterar FINAL_HUMAN_VALIDATION_BY_WAVE.md sem necessidade explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada antes de iniciar execução ampla das specs runtime.

Após implementada, toda spec futura deve referenciar a matriz na seção `Fontes obrigatórias lidas` quando a implementação envolver validação, runtime, testes, closeout ou status.
