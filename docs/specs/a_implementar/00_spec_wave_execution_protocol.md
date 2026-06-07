# SPEC — Wave Execution Protocol

> **Spec ID:** `00_spec_wave_execution_protocol`  
> **Status:** A implementar  
> **Wave:** WAVE 00 — Roadmap / governança  
> **Priority:** P0  
> **Type:** Governance / Documentation / Agent Protocol  
> **Domain:** Specs / Governance / Agent Execution  
> **Parallelizable:** CONDITIONAL  
> **Parallel group:** WAVE_00_GOVERNANCE  
> **Can run with:** `00_spec_validation_matrix_master.md`, desde que não editem os mesmos arquivos finais no mesmo commit.  
> **Must not run with:** specs runtime das Waves 01+ que dependam do protocolo ainda não publicado.  
> **Repo lock scope:** `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`, `docs/specs/README.md`, `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`, `docs/specs/SPEC_EXECUTION_ORDER.md` se autorizado explicitamente.  
> **Depends on:**  
> - `docs/specs/SPEC_GENERATION_ROADMAP_MASTER.md`  
> - `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`  
> - `docs/design/SPECIFICATION_PROCESS.md`  
> - `docs/design/SPEC_SOURCE_MAP.md`  
> **Blocks:**  
> - execução organizada de specs em lote;  
> - paralelização segura de waves;  
> - geração de prompts de execução para Codex/Claude Code;  
> - atualização futura de `SPEC_EXECUTION_ORDER.md`.  
> **Scope:** criar o protocolo canônico de execução por waves, com regras de lote, paralelização, locks, status, reports, validação e handoff para agentes.  
> **Out of scope:** implementar código Unity, alterar assets, promover specs para `implementados/`, rodar Play Mode humano ou criar specs runtime.

---

# /speckit.specify

## 1. Contexto

O roadmap macro já define waves, dependências e paralelismo de alto nível. Ele declara que WAVE 00 prepara a geração massiva de specs sem duplicar fontes nem quebrar rastreabilidade.

A execução real por agentes ainda precisa de um protocolo operacional específico para responder:

```text
qual lote pode começar;
quais specs podem rodar em paralelo;
quem segura lock de arquivos compartilhados;
como registrar status intermediário;
quando parar;
quando atualizar registries;
quando não atualizar SPEC_EXECUTION_ORDER.md;
como lidar com validação humana final deferida.
```

Esta spec cria o contrato para publicar esse protocolo como documento canônico, sem alterar runtime.

---

## 2. Problema

Sem um protocolo de execução por waves, agentes podem:

```text
executar specs fora de ordem;
rodar specs paralelas que editam o mesmo contrato;
tratar roadmap como spec implementável;
atualizar SPEC_EXECUTION_ORDER.md antes de as specs existirem;
marcar spec como aceita sem evidência;
pedir validação humana a cada micro-spec;
ler contexto demais ou contexto errado;
usar PROJECT_LOG.md como fonte padrão sem justificativa;
duplicar sistemas que já existem no repo.
```

Isso aumenta retrabalho e risco de inconsistência entre specs, refinements e código.

---

## 3. Objetivo

Ao final desta spec, o projeto deve ter um documento canônico de execução por waves que permita a Codex/Claude Code executar specs em ordem, com escopo travado, paralelização explícita, locks por arquivo/sistema, status padronizado e validação humana final deferida.

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
.claude/rules/testing-quality-gate.md
docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
docs/refinements/a_implementar/ref_futuro_map.md
```

Observação:

```text
docs/operations/AGENT_EXECUTION_PROTOCOL.md não foi encontrado durante a geração desta spec.
docs/operations/READING_MATRIX.md não foi encontrado via busca durante a geração desta spec.
O caminho real de current state validado é docs/project/CURRENT_STATE.md.
```

---

## 5. Estado atual do repo

Estado validado durante geração:

```text
- O roadmap macro existe em docs/specs/SPEC_GENERATION_ROADMAP_MASTER.md.
- O template canônico de specs existe em docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md.
- O processo canônico de especificação existe em docs/design/SPECIFICATION_PROCESS.md.
- O mapa de fontes existe em docs/design/SPEC_SOURCE_MAP.md.
- A fonte única de specs aponta para docs/specs/.
- CURRENT_STATE real está em docs/project/CURRENT_STATE.md.
- CLAUDE.md instrui leitura de docs/project/CURRENT_STATE.md para implementação.
- A regra Testing Quality Gate existe em .claude/rules/testing-quality-gate.md.
- O checklist final humano por wave existe em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

Esta spec não deve recriar:

```text
SPEC_GENERATION_ROADMAP_MASTER.md;
SPEC_IMPLEMENTABLE_TEMPLATE.md;
SPECIFICATION_PROCESS.md;
SPEC_SOURCE_MAP.md;
FINAL_HUMAN_VALIDATION_BY_WAVE.md;
Testing Quality Gate rule.
```

Ela deve criar apenas o protocolo operacional que conecta esses documentos.

---

## 6. User stories / engineering stories

```text
Como maintainer, quero um protocolo por waves para saber qual lote pode começar e qual está bloqueado.
Como agente executor, quero saber quais arquivos/sistemas exigem lock antes de editar.
Como agente executor, quero saber quando uma spec pode rodar em paralelo com outra.
Como maintainer, quero impedir updates prematuros de SPEC_EXECUTION_ORDER.md.
Como maintainer, quero que validação humana continue deferida para o final do lote/wave.
Como agente executor, quero um padrão de report para status, riscos e evidências.
Como revisor, quero saber quando parar se CURRENT_STATE, roadmap e spec conflitarem.
```

---

## 7. Escopo

Inclui:

```text
- criar documento canônico `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`;
- definir status possíveis de uma wave/spec em execução;
- definir pré-condições para iniciar uma wave;
- definir política de paralelização e locks;
- definir política de batch size;
- definir quando atualizar registries;
- definir quando NÃO atualizar SPEC_EXECUTION_ORDER.md;
- definir como registrar execução, validação e risco residual;
- definir como usar validação humana final deferida;
- definir stop conditions para agentes.
```

---

## 8. Fora de escopo

Não inclui:

```text
- código Unity;
- alteração em Assets/, Packages/ ou ProjectSettings/;
- criação de specs runtime;
- execução de specs;
- promoção de spec para implementados;
- alteração de CURRENT_STATE.md, salvo se explicitamente autorizado em outra spec;
- atualização de SPEC_EXECUTION_ORDER.md como se as novas specs já estivessem prontas para execução;
- validação humana no Unity;
- alteração de regras do Testing Quality Gate.
```

---

## 9. Regras de não duplicação

```text
Não criar novo roadmap.
Não criar segundo template de specs.
Não criar outro checklist humano final.
Não recriar Testing Quality Gate.
Não substituir SPECIFICATION_PROCESS.md.
Não transformar este protocolo em spec implementável de runtime.
Não usar docs_old/ como fonte ativa.
```

O protocolo deve referenciar as fontes existentes em vez de reescrever integralmente suas regras.

---

## 10. Critérios de aceite

### 10.1 Documento canônico criado

- Existe `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`.
- O documento declara que é protocolo operacional, não spec implementável.
- O documento aponta para roadmap, template, source map, current state e testing gate.

### 10.2 Status padronizados

O protocolo define status para specs/waves, no mínimo:

```text
NOT_STARTED
READY_FOR_SPEC_GENERATION
SPEC_CREATED
READY_FOR_IMPLEMENTATION
IMPLEMENTING
BUILD_VALIDATED
UNITY_VALIDATED
DEFERRED_TO_FINAL_HUMAN_VALIDATION
ACCEPTED
PARTIAL
BLOCKED
```

### 10.3 Paralelização e locks

O protocolo define:

```text
Parallelizable YES/NO/CONDITIONAL;
parallel groups;
repo lock scope;
shared files/systems;
conflict handling;
regra de não editar mesmo arquivo em paralelo.
```

### 10.4 Batch size

O protocolo define recomendação de tamanho de lote:

```text
1 spec para runtime/core/save/event bus/scene/global UI;
2 specs para governança ou specs médias paralelizáveis;
3 specs no máximo para docs-only sem arquivos compartilhados;
4+ specs não recomendado.
```

### 10.5 Validação humana deferida

O protocolo deixa explícito:

```text
não pedir human test spec a spec;
marcar cenário humano como DEFERRED_TO_FINAL_VALIDATION quando aplicável;
usar docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md no final do lote/wave.
```

### 10.6 SPEC_EXECUTION_ORDER protegido

O protocolo declara que `SPEC_EXECUTION_ORDER.md` só deve ser atualizado quando:

```text
as specs concretas existirem;
o lote de execução estiver autorizado;
as dependências estiverem reconciliadas;
não houver bloqueador em CURRENT_STATE.md;
o usuário autorizar explicitamente atualização de ordem de execução.
```

---

# /speckit.plan

## 11. Arquitetura alvo

Documento alvo:

```text
docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
```

Estrutura esperada:

```text
1. Objetivo
2. Fontes obrigatórias
3. Diferença entre gerar spec e executar spec
4. Estados de wave/spec
5. Pré-condições para iniciar wave
6. Política de lote
7. Política de paralelização
8. Lock de arquivos/sistemas
9. Status e evidência
10. Validação automatizada
11. Validação humana final deferida
12. Atualização de registries
13. Política para SPEC_EXECUTION_ORDER.md
14. Stop conditions
15. Handoff para Codex/Claude Code
16. Checklist antes de iniciar execução
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
Spec generation;
Agent execution protocol;
Registry discipline;
Wave planning;
Validation reporting;
Parallel execution coordination.
```

---

## 14. Arquivos permitidos

```text
docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
docs/specs/README.md
docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md
```

`docs/specs/SPEC_EXECUTION_ORDER.md` só pode ser alterado se o usuário autorizar explicitamente nesta execução.

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
docs/specs/SPEC_GENERATION_ROADMAP_MASTER.md, salvo correção mínima explicitamente necessária
docs/design/SPEC_SOURCE_MAP.md, salvo correção mínima explicitamente necessária
PROJECT_LOG.md
docs/project/CURRENT_STATE.md
```

---

## 16. Estratégia de implementação

### Fase 0 — Auditoria

```text
1. Confirmar existência dos documentos fonte.
2. Confirmar se já existe SPEC_WAVE_EXECUTION_PROTOCOL.md.
3. Confirmar se README já aponta para o protocolo.
4. Confirmar se registry precisa registrar esta spec como criada/pendente.
```

### Fase 1 — Criar protocolo

```text
1. Criar docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md.
2. Referenciar roadmap, template, current state, testing gate e checklist final.
3. Definir status, locks, paralelização, batch size e stop conditions.
```

### Fase 2 — Atualização documental mínima

```text
1. Atualizar README se necessário.
2. Atualizar registry se necessário.
3. Não atualizar SPEC_EXECUTION_ORDER.md sem autorização explícita.
```

### Fase 3 — Validação

```text
1. Rodar docs validation.
2. Registrar NOT RUN se validação não puder ser executada.
3. Não rodar Unity compile porque não há alteração runtime.
```

---

## 17. Ordem segura de execução

```text
1. Ler fontes obrigatórias.
2. Auditar existência do protocolo.
3. Criar protocolo.
4. Atualizar README/registry se permitido.
5. Rodar docs validation.
6. Reportar arquivos alterados, status e pendências.
```

---

## 18. Paralelização

- Parallelizable: CONDITIONAL
- Parallel group: WAVE_00_GOVERNANCE
- Can run with:
  - `00_spec_validation_matrix_master.md`, se não editar os mesmos arquivos simultaneamente.
- Must not run with:
  - specs runtime/core que dependam da ordem final de execução;
  - qualquer tarefa que edite `docs/specs/README.md` ou `SPEC_REGISTRY_TO_IMPLEMENT.md` ao mesmo tempo sem lock.
- Shared files/systems that require lock:
  - `docs/specs/README.md`
  - `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`
  - `docs/specs/SPEC_EXECUTION_ORDER.md`
- Reason:
  - Spec docs-only, mas altera governança usada por todos os agentes.

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
Risco: criar protocolo que contradiz SPEC_IMPLEMENTABLE_TEMPLATE.md.
Mitigação: referenciar template como fonte superior para estrutura de spec.

Risco: atualizar SPEC_EXECUTION_ORDER.md cedo demais.
Mitigação: declarar arquivo proibido salvo autorização explícita.

Risco: protocolo incentivar leitura excessiva.
Mitigação: seguir CURRENT_STATE.md e CLAUDE.md para leitura mínima.

Risco: protocolo pedir validação humana por spec.
Mitigação: usar DEFERRED_TO_FINAL_VALIDATION e checklist final por wave.
```

---

## 23. Rollback

```text
Remover docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md.
Reverter README/registry se alterados.
Não tocar em código, save, cenas ou assets.
```

---

# /speckit.tasks

## 24. Tasks

- [ ] T001 — Auditar se `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md` já existe.
- [ ] T002 — Criar `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md` com status, locks, paralelização e protocolo de lote.
- [ ] T003 — Garantir que o protocolo referencia `SPEC_IMPLEMENTABLE_TEMPLATE.md` e `FINAL_HUMAN_VALIDATION_BY_WAVE.md`.
- [ ] T004 — Declarar proteção de `SPEC_EXECUTION_ORDER.md`.
- [ ] T005 — Atualizar `docs/specs/README.md` se necessário.
- [ ] T006 — Atualizar `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md` se necessário.
- [ ] T007 — Rodar docs validation ou registrar NOT RUN com motivo.
- [ ] T008 — Reportar arquivos alterados e pendências.

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
SPEC_WAVE_EXECUTION_PROTOCOL.md criado.
Protocolo declara batch size, paralelização, locks, status e stop conditions.
Protocolo preserva validação humana final deferida.
SPEC_EXECUTION_ORDER.md não foi alterado sem autorização explícita.
Nenhum arquivo runtime/Unity foi alterado.
Docs validation executada ou NOT RUN com justificativa.
```

---

## 28. Anti-regressão

```text
Não transformar roadmap em spec executável.
Não promover specs automaticamente.
Não exigir human test por spec.
Não permitir paralelização sem lock scope.
Não atualizar SPEC_EXECUTION_ORDER.md cedo demais.
Não usar PROJECT_LOG.md como leitura padrão.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada antes de abrir execução paralela de specs runtime.

A implementação desta spec é documental, mas afeta como todos os agentes devem operar nas waves seguintes.
