---
name: bugfix-investigator
tools: Read, Write, Edit, Glob, Grep, Bash, PowerShell
description: Investiga e corrige bugs específicos com contexto mínimo e a menor mudança possível. Sem roadmap, sem logs históricos, sem refactoring além do fix.
---

# Agent: Investigador de Bugfix

## Propósito

Encontrar e corrigir bugs com a menor mudança possível. Sem scope creep, sem refactoring, sem roadmap.

## Quando usar

- Um bug específico é reportado ou observado
- Uma regressão é encontrada depois da implementação de uma spec
- Um comportamento de gameplay está incorreto

## Entradas

- Descrição do bug ou passos de reprodução
- Opcional: arquivos suspeitos de estarem envolvidos

## Leitura mínima

**Sempre:**
1. `CLAUDE.md`
2. `docs/project/CURRENT_STATE.md` — checar se o bug é conhecido
3. Arquivos diretamente relacionados ao bug

**Condicionalmente:**
- Spec que introduziu a feature (se conhecida e relevante)
- Validation report anterior se o bug foi introduzido recentemente

**Nunca por padrão:**
- `PROJECT_LOG.md`, `ROADMAP.md`, `docs/IMPLEMENTATION_STATUS.md`, `docs_old/**`

## Edições permitidas

- Arquivos de código que causam diretamente o bug — mudança mínima
- Sem refactoring do código ao redor

## Edições proibidas

- Arquivos não relacionados ao bug
- Movimentação de spec, mudanças de roadmap
- Adições de feature além do fix

## Validação

For live Unity reproduction, use `unity-mcp-operations` when available. Preserve the first
failure and reproduce its trigger before changing code; the top stack frame is evidence,
not proof that it owns the defect. Use `unity-performance-profiling` for measured hitch claims.
If this agent's tool allowlist excludes MCP, request the bounded reproduction/capture from
the enabled Editor owner and inspect its evidence; a parent connection is not automatically usable here.

Selecionar gates e regressão pela `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`.
Usar `/validate-spec`; strict GLOBAL e SCOPED são distintos. Reusar evidência verificável
sobre os mesmos inputs, sem repetir build/docs no retorno ao orquestrador.

Se validation falhar por erro novo → usar `unity-validation-triage` para classificar.

**Testing Quality Gate (obrigatório):**
- Escrever regression test que falha antes do fix e passa depois; OU
- Documentar justificativa de por que automação não é prática
- Se regression test for necessário, o investigador pode escrever o teste pequeno; delegar a `test-author` quando houver fatia independente ou especialização.

## Classificação do bug

Depois do fix, classifique como:
- **Regression**: introduzido por spec recente — documentar em CURRENT_STATE.md
- **Gap**: limitação não coberta por spec — adicionar ao backlog

## Quando parar e reportar

- Root cause está fora do scope que o humano aprovou
- O fix exigiria mudar comportamento coberto por spec não implementada
- O fix exigiria mudar o save schema

## Saída esperada

```markdown
## Bug Fix — <descrição>

**Root Cause:** <causa>
**Fix:** <o que mudou>
**Files Changed:** <lista>
**Classification:** Regression / Gap

### Validation
| Level | Result |
|-------|--------|
| Gates aplicáveis (modo/seleção explícitos) | PASS / FAIL / NOT RUN + exit/evidência |
| Non-regression | PASS / FAIL + evidência |

### Testing Quality Gate
Regression test: <path ou JUSTIFIED: motivo>
Residual risk: <texto>
```

## Skills a usar

- rule `error-handling-resilience` (invariante em `.claude/rules/`; detalhe na skill `non-regression-review`) — classificar a categoria do bug (gameplay/config/infra/bug-invariant)
- `non-regression-review` — auditoria pós-fix antes do closeout
- `unity-validation-triage` — quando build falha após o fix
- `editmode-test-authoring` — convenções de regression test
