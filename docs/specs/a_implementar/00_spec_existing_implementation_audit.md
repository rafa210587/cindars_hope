# SPEC — Existing Implementation Audit

> **Spec ID:** `00_spec_existing_implementation_audit`  
> **Status:** A implementar  
> **Wave:** WAVE 00 — Roadmap / governança  
> **Priority:** P0  
> **Type:** Governance / Audit / Documentation  
> **Domain:** Specs / Repo Audit / Implementation Mapping  
> **Parallelizable:** NO  
> **Parallel group:** N/A  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec runtime/code em execução simultânea; qualquer tarefa que atualize registries/status globais.  
> **Repo lock scope:** `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`, `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`, `docs/specs/SPEC_REGISTRY_IMPLEMENTED.md`, `docs/specs/SPEC_GENERATION_ROADMAP_MASTER.md`, `docs/IMPLEMENTATION_STATUS.md`, `docs/project/CURRENT_STATE.md` somente para leitura salvo autorização explícita.  
> **Depends on:**  
> - `docs/specs/SPEC_GENERATION_ROADMAP_MASTER.md`  
> - `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`  
> - `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`  
> - `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`  
> - `docs/design/SPEC_SOURCE_MAP.md`  
> **Blocks:**  
> - geração segura das specs runtime da WAVE 01+;  
> - decisão sobre specs a fundir, renomear, marcar como residual ou future;  
> - paralelização segura de specs por domínio.  
> **Scope:** auditar o estado real implementado/parcial/faltante do repo e produzir um mapa de sistemas existentes para evitar reimplementação nas próximas specs.  
> **Out of scope:** alterar código Unity, criar sistemas, mover specs para `implementados/`, executar Play Mode, atualizar `SPEC_EXECUTION_ORDER.md` ou fechar Phase 2-3 do MVP.

---

# /speckit.specify

## 1. Contexto

O roadmap macro lista dezenas de specs futuras. Antes de criar specs runtime da WAVE 01+, é necessário auditar o que já existe no repo para evitar:

```text
recriar sistemas já implementados;
gerar specs que contradizem implementação parcial;
marcar como novo algo que deve ser hardening/residual;
executar specs em paralelo que tocam o mesmo contrato;
ignorar blockers do CURRENT_STATE.md.
```

O `CURRENT_STATE.md` ainda registra Phase 2-3 do MVP como pendente de validação humana local e FASE 10+ como bloqueada. Esta spec deve tratar a solicitação humana atual como exceção apenas para **geração e auditoria documental de specs**, não como autorização para executar runtime em massa nem para marcar gameplay como aceito.

---

## 2. Problema

Sem auditoria de implementação existente, as próximas specs podem nascer com premissas falsas.

Riscos concretos:

```text
uma spec criar segundo SaveManager ou registry paralelo;
uma spec redefinir GameEventBus já existente;
uma spec reimplementar inventory/equipment/shop/skill tree já parcial;
uma spec tratar cave/combat/UI como inexistente quando há escopo residual ativo;
uma spec abrir alterações em Assets/ sem saber quais contratos já estão em uso;
uma spec marcar item como núcleo quando deveria ser future ou residual.
```

---

## 3. Objetivo

Ao final desta spec, o projeto deve ter um relatório canônico de auditoria em:

```text
docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md
```

Esse relatório deve mapear sistemas existentes, parciais, ausentes, duplicados/superseded e riscos por wave/domínio, servindo como pré-requisito para gerar specs runtime sem duplicação.

---

## 4. Fontes obrigatórias lidas

Para gerar esta spec, foram consideradas as fontes canônicas de geração e governança.

A execução futura da auditoria deve ler:

```text
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
docs/specs/SPEC_GENERATION_ROADMAP_MASTER.md
docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md
docs/specs/SPEC_SOURCE_OF_TRUTH.md
docs/specs/SPEC_REGISTRY_IMPLEMENTED.md
docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md
docs/specs/SPEC_EXECUTION_ORDER.md
docs/project/CURRENT_STATE.md
docs/IMPLEMENTATION_STATUS.md
CLAUDE.md
AGENTS.md
.claude/rules/RULES.md
.claude/commands/start-spec.md
.claude/commands/plan-wave.md
.claude/commands/implement-spec.md
.claude/skills/spec-execution/SKILL.md
```

Leitura condicional permitida durante execução da auditoria:

```text
PROJECT_LOG.md — apenas para reconciliar divergência entre registry/status/código.
docs/validation/** — apenas reports diretamente citados por registries, CURRENT_STATE ou implementação parcial.
docs/specs/implementados/** — para validar sistemas já implementados/parciais.
docs/refinements/implementados/** — apenas quando uma spec parcial/residual apontar refinamento como fonte.
```

---

## 5. Estado atual do repo

Estado validado durante geração desta spec:

```text
- `CLAUDE.md` já referencia leituras para spec planning/generation e execution.
- `SPEC_GENERATION_ROADMAP_MASTER.md` lista `00_spec_existing_implementation_audit.md` na WAVE 00 como auditoria antes de runtime.
- `SPEC_REGISTRY_TO_IMPLEMENT.md` lista 01Q e specs residuais/parciais antigas, mas ainda não lista esta spec no momento inicial da geração.
- `CURRENT_STATE.md` registra MVP code/build/docs validated, mas Phase 2-3 Play Mode pendente.
- `CURRENT_STATE.md` registra FASE 10+ bloqueada até Phase 2-3 ou decisão humana explícita.
- Existem documentos canônicos de governança: template, protocolo de waves, matriz de validação e checklist humano final.
```

Observação crítica:

```text
A execução desta spec é auditoria documental. Ela não libera execução runtime das Waves 02+ nem substitui a 01Q.
```

---

## 6. User stories / engineering stories

```text
Como gerador de specs, quero saber quais sistemas já existem para não escrever specs duplicadas.
Como executor, quero saber quais specs futuras são novas, residuais, hardening ou future.
Como revisor, quero uma tabela por domínio com status real: implemented, partial, missing, superseded ou unknown.
Como maintainer, quero saber quais arquivos/contratos são locks fortes antes de paralelizar specs.
Como usuário humano, quero evitar retrabalho antes de enviar specs ao Claude Code/Codex.
```

---

## 7. Escopo

Inclui:

```text
- auditar documentos de status, registries, specs implementadas e specs a implementar;
- auditar presença de sistemas principais no código por busca, sem alterar código;
- mapear cada wave do roadmap para estado real: implemented, partial, missing, residual, future ou unknown;
- identificar specs que devem ser geradas como hardening/residual em vez de implementação do zero;
- identificar possíveis duplicações entre specs futuras e sistemas existentes;
- identificar locks fortes por domínio;
- identificar leituras obrigatórias por wave antes de gerar specs runtime;
- criar `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`;
- sugerir ajustes futuros no roadmap/registry, sem aplicar automaticamente.
```

---

## 8. Fora de escopo

Não inclui:

```text
- alterar código em Assets/;
- alterar Packages/;
- alterar ProjectSettings/;
- criar ou corrigir sistemas runtime;
- criar specs runtime individuais;
- executar Play Mode;
- executar Unity compile;
- alterar `SPEC_EXECUTION_ORDER.md`;
- mover specs para `implementados/`;
- atualizar CURRENT_STATE.md ou IMPLEMENTATION_STATUS.md salvo autorização explícita posterior;
- resolver divergências encontradas; esta spec apenas reporta.
```

---

## 9. Regras de não duplicação

```text
Não criar segundo documento de roadmap.
Não criar segundo registry.
Não reescrever specs implementadas.
Não transformar relatório de auditoria em fonte de gameplay design.
Não usar audit report para substituir direction docs.
Não marcar sistema como ausente sem busca mínima no código e docs.
Não marcar sistema como implementado apenas por existir arquivo; exigir evidência documental/código mínima.
```

---

## 10. Critérios de aceite

### 10.1 Relatório criado

- Existe `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`.
- O relatório declara data, branch, escopo, fontes lidas e limitações.
- O relatório declara que não é spec implementável e não substitui source map/directions.

### 10.2 Matriz por wave

O relatório contém matriz para WAVE 01–12 com colunas mínimas:

```text
Wave;
Sistema/spec planejada;
Status real: IMPLEMENTED / PARTIAL / MISSING / RESIDUAL / FUTURE / UNKNOWN;
Evidência encontrada;
Risco de duplicação;
Recomendação: gerar nova / gerar residual / fundir / renomear / marcar future / bloquear.
```

### 10.3 Inventário de sistemas existentes

O relatório deve cobrir no mínimo:

```text
Core/bootstrap;
GameEventBus/events;
Save/load/GameSaveData;
Stable IDs/registries;
Inventory/items/equipment/hotbar;
Farm/crops/resources/shipping/shop/economy;
UI/HUD/modal/input/shop/crafting/equipment/skill tree;
Quest/objective/reward/flags;
Time/calendar/weather/lunar;
NPC/city/dialogue/services;
Player stats/resources/skills/magic;
Cave/procedural/checkpoints/run seed;
Combat/enemies/status/death/corpse;
Bestiary/knowledge.
```

### 10.4 Locks e paralelização

O relatório identifica locks fortes por sistema, no mínimo:

```text
GameEventBus/event contracts;
SaveManager/GameSaveData/save providers;
Bootstrap/global managers;
Input/modal stack;
shared ScriptableObjects/data registries;
scenes/prefabs/assets;
registries de specs/status.
```

### 10.5 Recomendações acionáveis

O relatório deve produzir uma seção `Recommended Next Actions` com:

```text
- specs seguras para gerar imediatamente;
- specs que exigem auditoria mais profunda antes de gerar;
- specs que devem ser residuais/hardening;
- specs que devem ficar future;
- specs que não devem ser executadas até 01Q ou exceção humana.
```

---

# /speckit.plan

## 11. Arquitetura alvo

Arquivo principal a criar:

```text
docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md
```

Estrutura esperada:

```text
1. Objetivo e escopo
2. Fontes lidas
3. Limitações da auditoria
4. Estado global do repo
5. Matriz por wave
6. Inventário por domínio/sistema
7. Locks fortes e paralelização
8. Specs candidatas a residual/hardening
9. Specs candidatas a future
10. Riscos de duplicação
11. Recommended Next Actions
12. Pendências de validação humana/Unity
```

---

## 12. Contratos, dados e eventos

### 12.1 Data contracts

N/A — auditoria documental; não altera dados runtime.

### 12.2 Runtime contracts

N/A — não altera contratos runtime.

### 12.3 Event contracts

N/A — não altera eventos.

### 12.4 Save contracts

N/A — não altera save.

### 12.5 UI contracts

N/A — não altera UI.

---

## 13. Sistemas afetados

```text
Spec governance;
Roadmap planning;
Registry discipline;
Parallel execution planning;
Future runtime spec generation.
```

Nenhum sistema runtime deve ser alterado.

---

## 14. Arquivos permitidos

```text
docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md
docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md
```

Atualização de `SPEC_REGISTRY_TO_IMPLEMENT.md` é permitida apenas para registrar esta spec como backlog de auditoria, se ainda não estiver listada.

---

## 15. Arquivos proibidos

```text
Assets/**
Packages/**
ProjectSettings/**
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
docs/IMPLEMENTATION_STATUS.md
PROJECT_LOG.md
```

`PROJECT_LOG.md` pode ser lido de forma condicional para reconciliação, mas não alterado por esta spec.

---

## 16. Estratégia de implementação

### Fase 0 — Preparação

```text
1. Confirmar branch e estado do repo.
2. Ler CURRENT_STATE.md e registries.
3. Confirmar que auditoria é documental e não altera runtime.
4. Confirmar se SPEC_EXISTING_IMPLEMENTATION_AUDIT.md já existe.
```

### Fase 1 — Auditoria documental

```text
1. Ler registries de specs.
2. Ler roadmap master.
3. Ler implemented specs relevantes.
4. Ler status docs e validation reports diretamente relacionados.
5. Mapear specs futuras vs implementadas/parciais.
```

### Fase 2 — Auditoria por código

Usar busca local, preferencialmente no Claude Code:

```bash
rg -n "class GameEventBus|struct .*Event|record .*Event" Assets
rg -n "class SaveManager|GameSaveData|SaveProvider|ISave" Assets
rg -n "Inventory|ItemInstance|Equipment|Hotbar" Assets
rg -n "Crop|Farm|Shipping|Shop|Economy|Price|Stock" Assets
rg -n "Quest|Objective|Reward|QuestFlag" Assets
rg -n "Calendar|Weather|Lunar|DayStarted|Season" Assets
rg -n "NPC|Dialogue|Schedule|City" Assets
rg -n "Skill|Spell|Mana|Stamina|Hunger" Assets
rg -n "Cave|RunSeed|Checkpoint|BossGate|Corpse|Death" Assets
rg -n "Enemy|Combat|StatusEffect|Damage" Assets
rg -n "Bestiary|Knowledge|Vulnerability" Assets
```

Também auditar docs:

```bash
rg -n "IMPLEMENTED|PARTIAL|BUILD_VALIDATED|UNITY_VALIDATED|DEFERRED_TO_FINAL_HUMAN_VALIDATION|ACCEPTED|BLOCKED" docs/specs docs/validation docs/IMPLEMENTATION_STATUS.md docs/project/CURRENT_STATE.md
```

### Fase 3 — Classificação

Classificar cada sistema/spec planejada como:

```text
IMPLEMENTED — existe e há evidência suficiente.
PARTIAL — existe, mas pendências relevantes permanecem.
RESIDUAL — já existe base e próxima spec deve ser hardening/delta.
MISSING — não há evidência de implementação.
FUTURE — intencionalmente fora da primeira entrega.
UNKNOWN — auditoria não encontrou evidência suficiente; exige inspeção manual.
```

### Fase 4 — Relatório

Criar `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md` com evidências e recomendações.

---

## 17. Ordem segura de execução

```text
1. Ler fontes obrigatórias.
2. Confirmar que não há implementação/runtime em escopo.
3. Auditar docs/status/registries.
4. Auditar código por busca local.
5. Classificar sistemas por wave/domínio.
6. Criar relatório canônico.
7. Atualizar registry se necessário.
8. Rodar docs validation.
9. Reportar limitações e próximos passos.
```

---

## 18. Paralelização

- Parallelizable: NO
- Parallel group: N/A
- Can run with:
  - N/A
- Must not run with:
  - geração simultânea de specs runtime da WAVE 01+;
  - updates simultâneos em registries;
  - closeout/promotions de specs;
  - qualquer tarefa que altere `docs/IMPLEMENTATION_STATUS.md` ou `CURRENT_STATE.md`.
- Shared files/systems that require lock:
  - `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`
  - `docs/specs/SPEC_REGISTRY_IMPLEMENTED.md`
  - `docs/specs/SPEC_GENERATION_ROADMAP_MASTER.md`
  - `docs/IMPLEMENTATION_STATUS.md`
  - `docs/project/CURRENT_STATE.md`
- Reason:
  - Auditoria precisa de snapshot estável do repo e dos registries.

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
Risco: auditoria marcar sistema como ausente porque busca textual não encontrou nome esperado.
Mitigação: usar múltiplos termos e marcar UNKNOWN quando não houver certeza.

Risco: auditoria ler PROJECT_LOG em excesso e consumir contexto desnecessário.
Mitigação: usar PROJECT_LOG apenas para reconciliar divergência concreta.

Risco: auditoria virar implementação disfarçada.
Mitigação: Assets/, Packages/ e ProjectSettings proibidos.

Risco: relatório virar fonte de design.
Mitigação: relatório informa estado real do repo; design continua nos directions/source map.
```

---

## 23. Rollback

```text
Remover `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`.
Reverter entrada desta spec no `SPEC_REGISTRY_TO_IMPLEMENT.md`, se adicionada.
Nenhum código/runtime/save/assets deve ter sido alterado.
```

---

# /speckit.tasks

## 24. Tasks

- [ ] T001 — Confirmar branch e ler `CURRENT_STATE.md`.
- [ ] T002 — Ler roadmap, template, protocolo de waves e matriz de validação.
- [ ] T003 — Ler registries de specs implementadas e a implementar.
- [ ] T004 — Auditar specs implementadas/parciais relevantes.
- [ ] T005 — Auditar código com buscas por domínio em `Assets/` sem modificar arquivos.
- [ ] T006 — Classificar sistemas por wave/domínio como IMPLEMENTED/PARTIAL/RESIDUAL/MISSING/FUTURE/UNKNOWN.
- [ ] T007 — Identificar locks fortes e riscos de paralelização.
- [ ] T008 — Identificar specs a gerar como novas, residual/hardening, fundidas, renomeadas ou future.
- [ ] T009 — Criar `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`.
- [ ] T010 — Atualizar `SPEC_REGISTRY_TO_IMPLEMENT.md` se necessário.
- [ ] T011 — Rodar docs validation ou registrar NOT RUN com motivo.
- [ ] T012 — Reportar limitações e próximos passos.

---

## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Não rodar Unity compile por esta spec ser docs/audit e não alterar runtime.

Se docs validation não puder rodar:

```text
Validation: NOT RUN
Reason: ambiente sem shell/local repo ou validação indisponível
Impact: links/paths não verificados automaticamente
Mitigation: executar `tools/docs/validate_docs.ps1` no Claude Code antes de considerar audit report aceito
Max status: PARTIAL
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: NO
- Requires EditMode tests: NO
- Requires PlayMode automated or final human scenario: NO
- Requires regression test: NO
- Human validation timing: NOT REQUIRED
- Minimum validation evidence for ACCEPTED: docs validation PASS; audit report created; no runtime/code changed; registry updated only if needed.

---

## 27. Definition of Done

```text
Spec executada sem alterações em Assets/, Packages/ ou ProjectSettings/.
SPEC_EXISTING_IMPLEMENTATION_AUDIT.md criado.
Relatório cobre waves 01-12 e domínios mínimos definidos.
Relatório classifica sistemas como IMPLEMENTED/PARTIAL/RESIDUAL/MISSING/FUTURE/UNKNOWN.
Relatório identifica locks fortes e recomendações de próxima geração de specs.
SPEC_REGISTRY_TO_IMPLEMENT.md atualizado se necessário.
Docs validation executada ou NOT RUN documentado.
```

---

## 28. Anti-regressão

```text
Não alterar runtime.
Não alterar cenas/prefabs/assets.
Não atualizar SPEC_EXECUTION_ORDER.md.
Não promover specs.
Não declarar Phase 2-3 como resolvida.
Não tratar auditoria como autorização para executar runtime em massa.
Não gerar specs runtime individuais durante a execução desta auditoria.
```

---

## 29. Notas para execução posterior

Esta spec pode ser gerada por ChatGPT, mas sua execução é mais adequada para Claude Code porque precisa de buscas locais amplas no repo.

Resultado esperado depois da execução:

```text
A próxima rodada de geração de specs deve usar o audit report para decidir se cada spec será nova, residual/hardening, fundida, renomeada ou future.
```
