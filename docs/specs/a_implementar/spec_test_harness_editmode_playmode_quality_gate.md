# SPEC — Test Harness / EditMode / PlayMode Quality Gate

> **Spec ID:** `spec_test_harness_editmode_playmode_quality_gate`  
> **Status:** A implementar  
> **Ordem sugerida:** WAVE 01 — fundação técnica, antes das specs runtime de domínio  
> **Tipo:** Tooling / Validation / Test Strategy / Quality Gate  
> **Depende de:**  
> - `docs/specs/implementados/spec_unity_compile_validation_protocol_and_scripts.md`  
> - `.claude/rules/testing-quality-gate.md`  
> - `.claude/skills/spec-execution/SKILL.md`  
> - `.claude/skills/unity-validation/SKILL.md`  
> **Bloqueia:** execução segura das specs runtime das próximas waves  
> **Escopo:** definir e implementar o harness mínimo para testes automatizados e cenários Play Mode, sem reescrever gameplay existente.  
> **Fora de escopo:** criar testes completos para todos os sistemas existentes em uma única spec; implementar CI remoto; converter todo Play Mode manual para automatizado.

---

# /speckit.specify

## 1. Contexto

O projeto já possui validação documental, compile validation via Unity batchmode e log scanner.

Isso reduz risco de erro de compilação, imports quebrados e falhas Unity evidentes, mas não garante comportamento correto.

As próximas waves terão mudanças em:

```text
stable IDs;
event bus;
save/load;
quest conditions/triggers/rewards;
time/calendar/weather/lunar;
inventory/equipment;
economy/crafting;
combat/status;
bestiary;
UI flows;
```

Esses domínios precisam de quality gate de teste antes de execução em massa.

---

## 2. Problema

Sem uma regra obrigatória de testes:

```text
specs podem compilar mas quebrar comportamento;
imports podem passar em dotnet e falhar no Unity;
quest rewards podem aplicar duas vezes;
save/load pode perder dados silenciosamente;
invalid IDs podem quebrar load;
UI pode bloquear input incorretamente;
bugfixes podem voltar depois;
```

Compile validation não substitui teste de regra de negócio.

---

## 3. Objetivo

Criar o quality gate mínimo para que toda spec runtime futura declare e execute validação apropriada.

O resultado deve deixar claro:

```text
quando EditMode tests são obrigatórios;
quando PlayMode automated tests são desejáveis;
quando cenário humano Play Mode é aceito;
quando bugfix exige regression test;
como registrar NOT RUN/BLOCKED;
como impedir ACCEPTED sem evidência;
como reportar risco residual.
```

---

## 4. User stories / engineering stories

```text
Como maintainer, quero que toda mudança de lógica determinística tenha teste automatizado quando viável.
Como maintainer, quero que toda mudança de gameplay em cena tenha cenário Play Mode documentado.
Como agente, quero saber quando criar EditMode tests, PlayMode tests ou só checklist humano.
Como agente, quero um bloco padrão de relatório para evidenciar cobertura de teste.
Como maintainer, quero impedir que uma spec seja aceita apenas por compilar.
Como maintainer, quero regression tests para bugfixes sempre que possível.
```

---

## 5. Critérios de aceite

### 5.1 Regra de quality gate

- `.claude/rules/testing-quality-gate.md` existe.
- `.claude/rules/RULES.md` referencia a nova regra.
- `CLAUDE.md` cita a regra nos invariants/stop conditions ou fluxo de spec.
- `.claude/skills/spec-execution/SKILL.md` exige bloco de testing quality gate em reports.
- `.claude/skills/unity-validation/SKILL.md` deixa claro que compile validation não substitui testes.

### 5.2 Estrutura de testes

A spec deve definir ou validar estrutura mínima para testes Unity:

```text
Assets/_Game/Tests/EditMode/
Assets/_Game/Tests/PlayMode/
```

Se a estrutura já existir, não duplicar.

Se não existir, criar com assembly definitions apropriadas apenas quando necessário e seguro.

### 5.3 Categorias obrigatórias

A documentação deve declarar que EditMode tests são obrigatórios para lógica determinística, incluindo:

```text
save/load;
quest conditions/triggers/rewards;
economy;
inventory transactions;
combat formulas;
status effects;
skill tree rules;
time/weather/lunar rules;
event bus contracts;
registries/IDs;
validators;
```

### 5.4 Play Mode / cenário humano

A documentação deve declarar que mudanças dependentes de cena/UI/input/prefab exigem:

```text
PlayMode automated test quando viável;
ou cenário humano em docs/validation/playmode/<spec_id>_human_test_scenario.md.
```

### 5.5 Execution report

O execution report de specs com código deve incluir:

```text
Testing Quality Gate
Changed runtime code
Changed deterministic logic
Changed Unity scene/prefab/asset wiring
Automated tests added/updated
Automated tests command
Manual Play Mode scenario
Justification if no automated tests
Residual risk
```

### 5.6 Status gating

- Sem teste obrigatório ou justificativa, status máximo: `PARTIAL`.
- Runtime/gameplay sem Play Mode automated/manual scenario: status máximo `BUILD_VALIDATED`.
- `ACCEPTED` exige evidência compatível com o tipo de mudança ou exceção aprovada pelo humano.

---

# /speckit.plan

## 6. Arquitetura alvo

### 6.1 Camadas

```text
Rules layer:
  .claude/rules/testing-quality-gate.md

Execution layer:
  .claude/skills/spec-execution/SKILL.md
  .claude/skills/unity-validation/SKILL.md

Spec layer:
  docs/specs/a_implementar/spec_test_harness_editmode_playmode_quality_gate.md

Validation layer:
  tools/docs/validate_docs.ps1
  tools/unity/RunUnityCompileValidation.ps1
  tools/unity/ScanUnityLogs.ps1
  Unity Test Runner / EditMode tests / PlayMode tests future

Evidence layer:
  docs/validation/<spec_id>_execution_report.md
  docs/validation/playmode/<spec_id>_human_test_scenario.md
```

### 6.2 Padrão esperado para testes automatizados futuros

```text
EditMode:
  usado para serviços puros, DTOs, rule engines, registries, save/load, conditions, rewards, formulas.

PlayMode:
  usado para scene flow, UI, input, prefabs, gameplay object lifecycle, player interaction.

Human Play Mode:
  usado quando automação ainda não é estável ou custosa demais para o MVP.
```

---

## 7. Sistemas afetados

```text
.claude/rules/**
.claude/skills/spec-execution/SKILL.md
.claude/skills/unity-validation/SKILL.md
CLAUDE.md
AGENTS.md se necessário
docs/specs/SPEC_GENERATION_ROADMAP_MASTER.md
docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md
docs/specs/a_implementar/spec_test_harness_editmode_playmode_quality_gate.md
docs/validation/** futuros
Assets/_Game/Tests/** se implementado nesta spec
```

---

## 8. Arquivos permitidos

```text
.claude/rules/**
.claude/skills/spec-execution/SKILL.md
.claude/skills/unity-validation/SKILL.md
CLAUDE.md
AGENTS.md
docs/specs/**
docs/validation/**
tools/**
Assets/_Game/Tests/**
```

---

## 9. Arquivos proibidos

```text
Assets/_Game/Scripts/** fora de helpers de teste explícitos
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset fora de asmdef/test assets autorizados
Packages/**
ProjectSettings/**
docs_old/**
docs/archive/**
```

---

## 10. Strategy de implementação

### Fase A — Documentação/rules

```text
1. Criar/validar .claude/rules/testing-quality-gate.md.
2. Atualizar RULES.md.
3. Atualizar CLAUDE.md com a regra resumida.
4. Atualizar spec-execution skill para exigir testing gate.
5. Atualizar unity-validation skill para explicitar que compile não substitui testes.
```

### Fase B — Roadmap/registry

```text
1. Registrar esta spec no roadmap master como WAVE 01 fundacional.
2. Registrar esta spec em SPEC_REGISTRY_TO_IMPLEMENT.md.
```

### Fase C — Harness técnico mínimo futuro

```text
1. Auditar se existe Assets/_Game/Tests/EditMode e PlayMode.
2. Se não existir, criar estrutura mínima.
3. Se asmdef for necessário, criar com dependências mínimas e validar Unity compile.
4. Criar pelo menos um teste smoke de harness somente se a estrutura Unity permitir sem quebrar imports.
```

Observação:

```text
A criação real de EditMode/PlayMode tests pode ser feita nesta spec ou em uma spec imediatamente seguinte, conforme auditoria local do projeto Unity.
```

---

# /speckit.tasks

## 11. Tasks

- [ ] Garantir `.claude/rules/testing-quality-gate.md`.
- [ ] Atualizar `.claude/rules/RULES.md`.
- [ ] Atualizar `CLAUDE.md` com invariant resumido.
- [ ] Atualizar `.claude/skills/spec-execution/SKILL.md`.
- [ ] Atualizar `.claude/skills/unity-validation/SKILL.md`.
- [ ] Atualizar `docs/specs/SPEC_GENERATION_ROADMAP_MASTER.md`.
- [ ] Atualizar `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`.
- [ ] Auditar existência de estrutura de testes Unity.
- [ ] Criar estrutura de testes se autorizado pela spec e seguro.
- [ ] Rodar docs validation.
- [ ] Rodar Unity compile validation se houver alteração Unity/test asmdef.
- [ ] Registrar resultado e risco residual.

---

## 12. Validações obrigatórias

### Documentação

```powershell
.\tools\docs\validate_docs.ps1
```

### Busca de regras

```bash
grep -n "testing-quality-gate" .claude/rules/RULES.md CLAUDE.md .claude/skills/spec-execution/SKILL.md .claude/skills/unity-validation/SKILL.md
grep -n "Testing Quality Gate" .claude/skills/spec-execution/SKILL.md .claude/rules/testing-quality-gate.md
```

### Unity, se houver mudança em Assets/_Game/Tests ou asmdef

```powershell
.\tools\unity\RunUnityCompileValidation.ps1 -ProjectPath "." -LogFile ".\Logs\unity-compile-validation.log"
.\tools\unity\ScanUnityLogs.ps1 -LogFile ".\Logs\unity-compile-validation.log"
```

### Test Runner futuro

Quando harness estiver pronto, comandos esperados podem incluir:

```text
Unity Test Runner — EditMode
Unity Test Runner — PlayMode
```

O comando final deve ser definido após auditoria local da versão Unity e estrutura de asmdefs.

---

## 13. Definition of Done

```text
Regra de testing quality gate criada e indexada.
Skills de execução/validação exigem evidência de teste ou justificativa.
Roadmap e registry conhecem esta spec como fundação de Wave 01.
Nenhuma spec runtime futura pode alegar ACCEPTED só por compilar.
Runtime deterministic logic passa a exigir EditMode tests quando viável.
Gameplay/UI/scene flow passa a exigir PlayMode ou cenário humano.
Bugfix passa a exigir regression test ou justificativa explícita.
```

---

## 14. Anti-regressão

```text
Compile validation não substitui testes.
Unit/EditMode tests não substituem Play Mode quando feature depende de cena/UI/input.
Play Mode manual não substitui teste automatizado para lógica pura testável.
Bugfix sem regression coverage deve registrar risco residual.
Spec runtime sem testing gate fica no máximo PARTIAL ou BUILD_VALIDATED conforme caso.
Não criar testes frágeis que dependem de ordem global não controlada.
Não alterar gameplay fora do escopo para facilitar teste.
```

---

## 15. Notas para execução posterior

Esta spec deve ser executada antes da geração/implementação em massa das specs das Waves 02+.

Se a auditoria local mostrar que o projeto ainda não suporta Unity Test Runner/asmdef de testes sem ajustes maiores, a primeira entrega deve pelo menos:

```text
criar a regra;
atualizar skills;
criar o padrão de reports;
definir o caminho de testes;
criar backlog explícito para harness técnico.
```

Não forçar criação de asmdef/testes se isso quebrar o projeto.