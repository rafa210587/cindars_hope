---
name: spec-execution
description: Executa uma spec respeitando scope, arquivos permitidos, validações obrigatórias e closeout phase-gated. Use ao implementar qualquer spec de .specs/a_implementar/.
---

# Skill: Execução de Spec

Esta skill cobre a implementação e o avanço de uma spec da fila `.specs/a_implementar/`, do Phase 0 ao closeout.

## Quando usar

A tarefa envolve implementar ou avançar uma spec de `.specs/a_implementar/`.

## Leitura mínima

1. `CLAUDE.md`
2. `docs/project/CURRENT_STATE.md`
3. `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md` — execution phases e governança
4. `.specs/SPEC_VALIDATION_MATRIX_MASTER.md` — requisitos de validação por tipo de mudança
5. Spec alvo
6. Arquivos explicitamente no scope da spec

## Não ler por padrão

```
PROJECT_LOG.md
docs/IMPLEMENTATION_STATUS.md
SPEC_EXECUTION_ORDER.md (full — use CURRENT_STATE.md queue)
ROADMAP.md
memory/ (unless spec cites prior pattern)
```

## Leitura mínima condicional (specs de runtime/código)

Se for implementar uma spec que muda comportamento runtime ou código:

- `.claude/rules/testing-quality-gate.md` — requisitos de testing e regras de evidência
- `docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md` — checklist de human validation

## Procedimento

### Phase 0: Preparação

1. Leia o CURRENT_STATE.md — cheque por blockers
2. Leia a spec alvo — identifique scope, dependencies, requisitos de validação
3. Cheque se as dependencies estão resolvidas
4. Identifique as sub-skills aplicáveis (veja abaixo)
5. Se bloqueado: pare e reporte

### Phase 1: Scope Lock

- [ ] Arquivos permitidos listados
- [ ] Arquivos proibidos listados (docs_old, root specs, arquivos fora do scope da spec)
- [ ] Validações obrigatórias identificadas
- [ ] Requisito de Phase 2-3 determinado (a spec precisa de Unity validators / Play Mode?)

### Phase 2: Implementação

- [ ] Implemente exatamente conforme a spec
- [ ] Sem amplificação de scope
- [ ] Sem introdução de pattern proibido (GameObject.Find, chamadas diretas de gameplay)
- [ ] Faça commit com frequência em português
- [ ] NÃO mova a spec para implementados/ ainda

### Phase 3: Validação

Rode `/validate-spec`:
- [ ] Docs validation (se docs mudaram)
- [ ] C# runtime build (se .cs mudou)
- [ ] C# editor build (se editor .cs mudou)
- [ ] Registre NOT RUN com o motivo para Phase 4-5

Rode `/review-non-regression`:
- [ ] Sem patterns proibidos
- [ ] Sem edições de arquivos fora de scope
- [ ] Sem Unity refs no save

### Phase 4: Execution Report & Test Scenario

Crie `docs/validation/<spec_id>_execution_report.md`.

Registre o status de fase usando a taxonomy:

| Status | Significado |
|--------|---------|
| `BUILD_VALIDATED` | dotnet build + docs PASS |
| `UNITY_VALIDATED` | Unity validators PASS |
| `PLAYMODE_VALIDATED` | Play Mode checklist PASS |
| `ACCEPTED` | Todas as fases obrigatórias completas |
| `DEFERRED_TO_FINAL_HUMAN_VALIDATION` | Código completo; Phase 3 human validation adiada para o batch de fim de wave (conforme FINAL_HUMAN_VALIDATION_BY_WAVE.md) |
| `PARTIAL` | Algumas fases completas, outras não |
| `BLOCKED` | Não é possível prosseguir |

**Se houver mudanças de runtime/gameplay (Phase 2-3 obrigatória):**

Antes do closeout da Phase 5, invoque a skill `/gameplay-test-scenario` para:
- [ ] Criar o human test scenario: `docs/validation/playmode/<spec_id>_human_test_scenario.md`
- [ ] Documentar na seção Phase 3 do execution report: como o tester humano vai verificar a feature
- [ ] Linkar o arquivo de test scenario no report

Sem evidência de test scenario para specs de runtime, o status de closeout fica limitado a `BUILD_VALIDATED`.

### Phase 5: Closeout (via /finish-spec)

NÃO mova a spec para implementados/ automaticamente.

Chame `/finish-spec`, que checa a elegibilidade de promoção:
- Spec docs-only: promove após `BUILD_VALIDATED`
- Spec de código (sem gameplay): promove após `BUILD_VALIDATED`
- Spec de runtime/gameplay: promove só após `ACCEPTED` (Phase 2-3 obrigatória)

## Validação

```powershell
# Docs
.\tools\docs\validate_docs.ps1

# C# runtime
dotnet build .\Assembly-CSharp.csproj --no-restore

# C# editor
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
```

## Regressões comuns

- Mover a spec para implementados/ antes de coletar a evidência de Phase 2-3
- Reivindicar ACCEPTED com base apenas no build da Phase 1
- Ler o PROJECT_LOG.md como contexto padrão
- Amplificar o scope além da spec

## Quando parar e reportar

- A spec e o CURRENT_STATE.md conflitam
- O scope da spec está ambíguo após leitura cuidadosa
- Dependency bloqueada (conforme CURRENT_STATE.md)
- Validação obrigatória falha sem caminho documentado para seguir
- Qualquer arquivo proibido seria modificado

## Saída esperada

Execution report em `docs/validation/<spec_id>_execution_report.md` + histórico de commits.

## Sub-skills aplicáveis

- `gameplay-test-scenario` — se houver mudanças de runtime/gameplay (cria o test plan da Phase 3)
- `unity-validation` — se houver mudanças de runtime
- `save-load-pattern` — se persistência estiver no scope
- `event-bus-pattern` — se comunicação de gameplay estiver no scope
- `bootstrap-wiring` — se manager wiring estiver no scope
- `non-regression-review` — antes do closeout
- `docs-migration` — ao promover uma spec elegível
- `implementation-closeout` — checklist final
