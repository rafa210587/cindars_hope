# SPEC — Test Harness / EditMode / PlayMode Quality Gate

> **Spec ID:** `spec_test_harness_editmode_playmode_quality_gate`  
> **Status:** A implementar  
> **Wave:** WAVE 01 — Core IDs / Events / Save baseline  
> **Priority:** P0 / 01Q  
> **Type:** Tooling / Validation / Test Strategy / Quality Gate  
> **Domain:** Testing / Unity Validation / Spec Execution / Quality Gate  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_01_CORE_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec runtime/code que dependa de quality gate, qualquer tarefa que altere `.claude/rules`, `.claude/skills`, `.claude/commands`, Unity test asmdefs ou `Assets/_Game/Tests/**`.  
> **Repo lock scope:** `.claude/rules/testing-quality-gate.md`, `.claude/rules/RULES.md`, `.claude/skills/spec-execution/SKILL.md`, `.claude/skills/unity-validation/SKILL.md`, `.claude/commands/validate-spec.md`, `.claude/commands/finish-spec.md`, `CLAUDE.md`, `AGENTS.md`, `Assets/_Game/Tests/**`, `docs/validation/spec_test_harness_editmode_playmode_quality_gate_execution_report.md`.  
> **Depends on:**  
> - `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`  
> - `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`  
> - `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`  
> - `.specs/SPEC_GENERATION_ROADMAP_MASTER.md`  
> - `docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md`  
> - `.specs/implementados/spec_unity_compile_validation_protocol_and_scripts.md`  
> - `.claude/rules/testing-quality-gate.md`  
> - `.claude/skills/spec-execution/SKILL.md`  
> - `.claude/skills/unity-validation/SKILL.md`  
> **Blocks:**  
> - execução em massa de specs runtime das Waves 02+;  
> - fechamento de status `ACCEPTED` para specs runtime/gameplay;  
> - padronização de reports de execução;  
> - Play Mode/final human validation por wave/lote.  
> **Scope:** consolidar o quality gate obrigatório para specs runtime, validar/criar estrutura mínima de testes Unity quando seguro e alinhar harness/skills/commands para impedir ACCEPTED sem evidência.  
> **Out of scope:** criar testes completos para todos os sistemas existentes, executar validação humana por spec, implementar CI remoto, reescrever gameplay, alterar scenes/prefabs ou converter todo Play Mode em automação agora.

---

# /speckit.specify

## 1. Contexto

O projeto já possui validação documental, Unity compile validation por batchmode e scanner de logs. Isso reduz risco de erro de compilação, import quebrado e falhas evidentes no Editor, mas não garante comportamento correto.

As próximas waves envolvem lógica determinística, save/load, eventos, UI, cena, input, prefabs, gameplay e regressões. Compile/build não basta para aceitar esse tipo de mudança.

Esta spec existe para consolidar o quality gate que todas as specs runtime futuras devem seguir.

Regra central:

```text
Não pedir validação humana spec a spec.
Quando Play Mode humano for necessário, registrar cenário final e marcar Human validation timing como DEFERRED_TO_FINAL_VALIDATION.
A validação humana real acontece no fim do lote/wave usando docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 2. Problema

Sem quality gate obrigatório:

```text
specs podem compilar mas quebrar comportamento;
save/load pode perder dados silenciosamente;
quest rewards podem aplicar duas vezes;
invalid IDs podem quebrar load;
event subscribers podem duplicar callbacks;
UI/modal/input pode bloquear gameplay incorretamente;
bugfixes podem voltar sem regressão;
agentes podem marcar ACCEPTED apenas com compile;
agentes podem pedir human test a cada spec, quebrando o fluxo combinado.
```

---

## 3. Objetivo

Ao final desta spec, o projeto deve ter um quality gate operacional e aplicável por agentes.

Resultado esperado:

```text
rules, skills e commands usam a mesma taxonomia de status;
toda spec runtime nova exige bloco Testing Quality Gate;
lógica determinística exige EditMode tests quando praticável;
UI/cena/input/prefab exige PlayMode automated ou cenário final humano documentado;
bugfix exige regression test ou risco residual documentado;
sem teste obrigatório nem justificativa, status máximo PARTIAL;
runtime/gameplay sem PlayMode/cenário final humano fica no máximo BUILD_VALIDATED;
validação humana por spec é proibida por padrão.
```

---

## 4. Fontes obrigatórias lidas

Para criar/ajustar esta spec foram consideradas:

```text
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
.specs/SPEC_VALIDATION_MATRIX_MASTER.md
.specs/SPEC_GENERATION_ROADMAP_MASTER.md
.specs/SPEC_REGISTRY_TO_IMPLEMENT.md
docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
.specs/implementados/spec_unity_compile_validation_protocol_and_scripts.md
docs/project/CURRENT_STATE.md
docs/IMPLEMENTATION_STATUS.md
.claude/rules/testing-quality-gate.md
.claude/rules/RULES.md
.claude/skills/spec-execution/SKILL.md
.claude/skills/unity-validation/SKILL.md
.claude/commands/validate-spec.md
.claude/commands/finish-spec.md
CLAUDE.md
AGENTS.md
```

A execução futura deve reler todos esses arquivos no repo local antes de aplicar patches.

---

## 5. Estado atual do repo

Estado comprovado documentalmente:

```text
- `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md` existe e define Testing Quality Gate obrigatório para toda spec nova.
- `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md` existe e define protocolo de execução por waves.
- `.specs/SPEC_VALIDATION_MATRIX_MASTER.md` existe e define status caps e evidência mínima por tipo de mudança.
- `docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md` existe e concentra validação humana final por wave/lote.
- `.claude/rules/testing-quality-gate.md` já existe.
- `.claude/skills/spec-execution/SKILL.md` e `.claude/skills/unity-validation/SKILL.md` já existem.
- A spec 01Q já está registrada no roadmap/registry como fundacional antes de execução em massa das Waves 02+.
```

Estado que precisa ser confirmado localmente:

```text
- se `Assets/_Game/Tests/EditMode/` existe;
- se `Assets/_Game/Tests/PlayMode/` existe;
- se há asmdefs de teste Unity já configurados;
- se há comando local estável para Unity Test Runner;
- se `validate-spec` e `finish-spec` estão 100% alinhados com DEFERRED_TO_FINAL_HUMAN_VALIDATION;
- se ainda existe qualquer referência antiga a `docs/05_VALIDATION` ou human test obrigatório por spec.
```

---

## 6. User stories / engineering stories

```text
Como maintainer, quero impedir ACCEPTED de runtime/gameplay apenas por compile.
Como agente executor, quero saber quando criar EditMode test, PlayMode test, cenário final humano ou risco residual.
Como revisor, quero reports comparáveis entre specs.
Como executor de bugfix, quero regressão obrigatória quando praticável.
Como usuário humano, quero validar no Unity só no final do lote/wave, não a cada spec.
Como pipeline futura, quero estrutura mínima para testes Unity sem quebrar imports/asmdefs.
```

---

## 7. Escopo

Inclui:

```text
- validar e ajustar `.claude/rules/testing-quality-gate.md`;
- garantir referência em `.claude/rules/RULES.md`;
- garantir que `CLAUDE.md`, commands e skills leem a matriz/protocolo quando aplicável;
- alinhar `spec-execution` e `unity-validation` com status caps;
- alinhar `validate-spec` e `finish-spec` com validação humana final deferida;
- auditar e, se seguro, criar estrutura mínima `Assets/_Game/Tests/EditMode/` e `Assets/_Game/Tests/PlayMode/`;
- criar smoke test/validator mínimo apenas se a estrutura Unity permitir sem quebrar imports;
- criar execution report da 01Q.
```

---

## 8. Fora de escopo

Não inclui:

```text
- criar testes completos para todos os sistemas;
- exigir Play Mode humano por spec;
- criar CI remoto;
- alterar gameplay para facilitar teste;
- mexer em scenes/prefabs/game assets;
- alterar Packages/ ou ProjectSettings/;
- reescrever skills/commands inteiros sem necessidade;
- mover specs para implementados;
- atualizar SPEC_EXECUTION_ORDER.md.
```

---

## 9. Regras de não duplicação

```text
Não criar segunda regra de quality gate se `.claude/rules/testing-quality-gate.md` já cobre o caso.
Não criar novo command se `validate-spec`/`finish-spec` puderem ser ajustados.
Não criar novo skill se `spec-execution`/`unity-validation` puderem ser ajustados.
Não duplicar o conteúdo inteiro da matriz de validação dentro de commands/skills; referenciar `SPEC_VALIDATION_MATRIX_MASTER.md`.
Não criar estrutura de testes Unity paralela fora de `Assets/_Game/Tests/**`.
Não criar Play Mode human scenario por spec como obrigação.
```

---

## 10. Critérios de aceite

### 10.1 Harness documental alinhado

- `.claude/rules/testing-quality-gate.md` existe e contém regra compatível com `SPEC_VALIDATION_MATRIX_MASTER.md`.
- `.claude/rules/RULES.md` referencia a regra.
- `CLAUDE.md` aponta para protocolo/matriz/template no fluxo de specs.
- `spec-execution` skill exige Testing Quality Gate e status caps.
- `unity-validation` skill declara que compile não substitui teste comportamental.
- `validate-spec` usa matriz/protocolo para validação.
- `finish-spec` permite `DEFERRED_TO_FINAL_HUMAN_VALIDATION` e não exige human test por spec.

### 10.2 Estrutura de testes auditada

A execução deve auditar:

```text
Assets/_Game/Tests/EditMode/
Assets/_Game/Tests/PlayMode/
asmdefs relacionados;
comandos Unity Test Runner disponíveis;
compatibilidade com Unity compile validation existente.
```

Se a estrutura não existir e for seguro criar, criar estrutura mínima.

Se criar estrutura/asmdef for arriscado, registrar `NOT IMPLEMENTED` com risco residual e manter a spec como quality gate documental/harness.

### 10.3 Categorias obrigatórias

A regra deve declarar:

```text
EditMode tests obrigatórios quando praticável para lógica determinística;
PlayMode automated ou cenário final humano documentado para UI/cena/input/prefab/gameplay lifecycle;
regression test obrigatório para bugfix quando praticável;
NOT RUN precisa de motivo, impacto, mitigação e status máximo;
sem teste obrigatório nem justificativa, status máximo PARTIAL;
runtime/gameplay sem PlayMode automated nem cenário final humano documentado fica no máximo BUILD_VALIDATED.
```

### 10.4 Validação humana deferida

A execução deve garantir:

```text
nenhum command/skill exige validação humana por spec;
cenários humanos finais apontam para `docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md`;
status humano real não pode ser alegado antes da execução final;
`DEFERRED_TO_FINAL_HUMAN_VALIDATION` é aceito como status intermediário.
```

### 10.5 Execution report

Criar:

```text
docs/validation/spec_test_harness_editmode_playmode_quality_gate_execution_report.md
```

O relatório deve incluir:

```text
arquivos de harness alterados;
estrutura de testes encontrada/criada;
commands/skills/rules auditados;
validações rodadas;
NOT RUN entries;
riscos residuais;
Testing Quality Gate aplicado à própria spec.
```

---

# /speckit.plan

## 11. Arquitetura alvo

### 11.1 Layers

```text
Rules layer:
  .claude/rules/testing-quality-gate.md
  .claude/rules/RULES.md

Execution layer:
  .claude/skills/spec-execution/SKILL.md
  .claude/skills/unity-validation/SKILL.md
  .claude/commands/validate-spec.md
  .claude/commands/finish-spec.md

Governance docs:
  .specs/SPEC_VALIDATION_MATRIX_MASTER.md
  .specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
  .specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
  docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md

Unity test layer, if safe:
  Assets/_Game/Tests/EditMode/**
  Assets/_Game/Tests/PlayMode/**

Evidence:
  docs/validation/spec_test_harness_editmode_playmode_quality_gate_execution_report.md
```

### 11.2 Test layers

```text
EditMode:
  Serviços puros, DTOs, rule engines, registries, save/load, conditions, rewards, formulas, validators.

PlayMode automated:
  Scene flow, UI, input, prefabs, gameplay lifecycle, player interaction, modal/focus.

Final human validation:
  Cenários integrados por wave/lote usando `FINAL_HUMAN_VALIDATION_BY_WAVE.md`, não por spec individual.
```

---

## 12. Contratos, dados e eventos

### 12.1 Data contracts

N/A — não altera dados de gameplay.

### 12.2 Runtime contracts

```text
Quality gate passa a ser contrato obrigatório de execução para specs runtime.
Commands/skills devem impedir promoção indevida sem evidência.
```

### 12.3 Event contracts

N/A — não altera eventos de gameplay.

### 12.4 Save contracts

N/A — não altera save schema.

### 12.5 UI contracts

N/A — não altera UI de jogo.

---

## 13. Sistemas afetados

```text
Claude harness;
spec execution workflow;
unity validation workflow;
docs validation workflow;
future Unity test structure;
execution reports;
final human validation workflow.
```

---

## 14. Arquivos permitidos

```text
.claude/rules/testing-quality-gate.md
.claude/rules/RULES.md
.claude/skills/spec-execution/SKILL.md
.claude/skills/unity-validation/SKILL.md
.claude/commands/validate-spec.md
.claude/commands/finish-spec.md
CLAUDE.md
AGENTS.md
Assets/_Game/Tests/**
docs/validation/spec_test_harness_editmode_playmode_quality_gate_execution_report.md
```

Leitura permitida, alteração apenas se explicitamente necessária e reportada:

```text
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
.specs/SPEC_VALIDATION_MATRIX_MASTER.md
docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
```

---

## 15. Arquivos proibidos

```text
Assets/_Game/Scripts/** fora de helpers/test-only code explícitos
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset fora de asmdef/test assets autorizados
Packages/**
ProjectSettings/**
.specs/SPEC_EXECUTION_ORDER.md
.specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
docs_old/**
docs/archive/**
```

---

## 16. Estratégia de implementação

### Fase 0 — Auditoria local obrigatória

```bash
rg -n "Testing Quality Gate|testing-quality-gate|DEFERRED_TO_FINAL_HUMAN_VALIDATION|docs/05_VALIDATION|human test|Play Mode humano|manual scenario|ACCEPTED|BUILD_VALIDATED|PARTIAL" CLAUDE.md AGENTS.md .claude .specs docs/validation
find Assets/_Game/Tests -maxdepth 3 -type f 2>/dev/null || true
```

### Fase 1 — Harness documental

```text
1. Validar/ajustar testing-quality-gate rule.
2. Validar/ajustar RULES.md.
3. Validar/ajustar CLAUDE.md routing.
4. Validar/ajustar spec-execution skill.
5. Validar/ajustar unity-validation skill.
6. Validar/ajustar validate-spec e finish-spec.
```

### Fase 2 — Estrutura técnica mínima

```text
1. Auditar estrutura de testes Unity.
2. Criar `Assets/_Game/Tests/EditMode/` e `Assets/_Game/Tests/PlayMode/` apenas se seguro.
3. Criar asmdef/test smoke apenas se não quebrar imports/compile.
4. Se não seguro, registrar explicitamente como backlog técnico e risco residual.
```

### Fase 3 — Evidence/report

```text
1. Rodar validações aplicáveis.
2. Criar execution report.
3. Não marcar final human validation como executada.
```

---

## 17. Ordem segura de execução

```text
1. Ler fontes obrigatórias.
2. Auditar harness atual.
3. Ajustar rules/skills/commands apenas onde necessário.
4. Auditar estrutura Unity tests.
5. Criar estrutura mínima/test smoke apenas se seguro.
6. Rodar docs validation.
7. Rodar Unity compile se Assets/_Game/Tests ou asmdef forem alterados.
8. Criar execution report.
9. Não atualizar SPEC_EXECUTION_ORDER.md nem promover specs.
```

---

## 18. Paralelização

- Parallelizable: NO
- Parallel group: WAVE_01_CORE_LOCKED
- Can run with:
  - N/A
- Must not run with:
  - qualquer spec runtime/code;
  - qualquer ajuste simultâneo em `.claude/rules`, `.claude/skills`, `.claude/commands`;
  - qualquer criação simultânea de tests/asmdefs.
- Shared files/systems that require lock:
  - Claude harness;
  - Unity test folders;
  - validation reports.
- Reason:
  - Esta spec define o gate que as demais specs precisam obedecer.

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
Changes ScriptableObjects/assets: CONDITIONAL — only test asmdef/test assets if safe.
Requires Play Mode final validation: NO for harness docs/tests setup.
Human validation timing: NOT REQUIRED.
```

---

## 22. Riscos técnicos

```text
Risco: criar asmdef de teste quebrando compile Unity.
Mitigação: criar apenas se estrutura/dependências forem claras; caso contrário registrar backlog técnico.

Risco: commands/skills duplicarem regras e divergirem da matriz.
Mitigação: referenciar SPEC_VALIDATION_MATRIX_MASTER.md em vez de copiar tudo.

Risco: agentes continuarem pedindo human test por spec.
Mitigação: auditar strings antigas e substituir por DEFERRED_TO_FINAL_HUMAN_VALIDATION.

Risco: runtime specs ainda alegarem ACCEPTED sem PlayMode/cenário final.
Mitigação: finish-spec deve aplicar status caps.
```

---

## 23. Rollback

```text
Reverter ajustes em rules/skills/commands/CLAUDE/AGENTS.
Remover estrutura/test smoke criada em Assets/_Game/Tests/**.
Remover execution report.
Nenhum código gameplay, scene, prefab, save ou asset deve ter sido alterado.
```

---

# /speckit.tasks

## 24. Tasks

- [ ] T001 — Ler fontes obrigatórias e confirmar branch.
- [ ] T002 — Auditar harness atual para referências antigas, human test por spec e status caps.
- [ ] T003 — Ajustar `.claude/rules/testing-quality-gate.md` se necessário.
- [ ] T004 — Ajustar `.claude/rules/RULES.md` se necessário.
- [ ] T005 — Ajustar `CLAUDE.md`, `validate-spec`, `finish-spec`, `spec-execution` e `unity-validation` se necessário.
- [ ] T006 — Auditar estrutura `Assets/_Game/Tests/**`.
- [ ] T007 — Criar estrutura/test smoke apenas se seguro; caso contrário registrar backlog técnico.
- [ ] T008 — Rodar docs validation.
- [ ] T009 — Rodar Unity compile/log scan se houve alteração em tests/asmdef.
- [ ] T010 — Criar `docs/validation/spec_test_harness_editmode_playmode_quality_gate_execution_report.md`.
- [ ] T011 — Reportar risco residual e status final.

---

## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca de alinhamento:

```bash
rg -n "docs/05_VALIDATION|human test por spec|Play Mode humano obrigatório por spec|manual scenario" CLAUDE.md AGENTS.md .claude .specs docs/validation
rg -n "Testing Quality Gate|DEFERRED_TO_FINAL_HUMAN_VALIDATION|SPEC_VALIDATION_MATRIX_MASTER" CLAUDE.md .claude .specs docs/validation
```

Unity compile, somente se houver mudança em `Assets/_Game/Tests/**`, asmdef ou Unity C#:

```powershell
.\tools\unity\RunUnityCompileValidation.ps1 -ProjectPath "." -LogFile ".\Logs\unity-compile-validation.log"
.\tools\unity\ScanUnityLogs.ps1 -LogFile ".\Logs\unity-compile-validation.log"
```

EditMode/PlayMode automated:

```text
Rodar apenas se o harness local já suportar comando estável.
Se não suportar, registrar NOT RUN com motivo e risco residual.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES if commands/validators/test smoke logic is changed; otherwise NO.
- Requires EditMode tests: YES if deterministic validator/test code is added and harness is available; otherwise document NOT RUN/NOT PRACTICAL with reason.
- Requires PlayMode automated or final human scenario: NO.
- Requires regression test: YES if fixing a known harness/status gating bug; otherwise NO.
- Human validation timing: NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; alignment search PASS or documented residuals; Unity compile PASS if Unity tests/asmdefs changed; execution report created; no human validation per spec required.

---

## 27. Definition of Done

```text
Rules/skills/commands aligned with quality gate and validation matrix.
No command/skill requires human Play Mode validation spec a spec.
DEFERRED_TO_FINAL_HUMAN_VALIDATION supported as intermediate state.
Testing folders audited; created only if safe.
No runtime gameplay code changed.
No scenes/prefabs/assets changed except explicit test assets/asmdefs if safe.
Execution report created.
SPEC_EXECUTION_ORDER.md unchanged.
```

---

## 28. Anti-regressão

```text
Compile validation não substitui comportamento.
EditMode não substitui PlayMode para cena/UI/input.
PlayMode humano final não substitui teste automatizado de lógica pura quando viável.
Bugfix sem regression test precisa risco residual.
Spec runtime sem testing gate fica no máximo PARTIAL ou BUILD_VALIDATED conforme caso.
Não pedir validação humana por spec.
Não declarar ACCEPTED antes da evidência compatível.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada antes de execução em massa das Waves 02+ runtime.

Depois dela, specs runtime futuras devem usar o bloco:

```md
## Testing Quality Gate

- Changed deterministic logic: YES/NO
- Requires EditMode tests: YES/NO
- Requires PlayMode automated or final human scenario: YES/NO
- Requires regression test: YES/NO
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION / NOT REQUIRED
- Minimum validation evidence for ACCEPTED: <text>
```

Se o harness Unity Test Runner ainda não puder ser criado com segurança, a execução deve deixar backlog técnico explícito, mas ainda assim alinhar rules/skills/commands/status caps.
