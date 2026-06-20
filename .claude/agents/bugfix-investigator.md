---
name: bugfix-investigator
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
2. `docs/project/CURRENT_STATE.md` (checar se o bug é conhecido)
3. Arquivos que causam diretamente ou estão relacionados ao bug

**Condicionalmente:**
- Spec que originalmente introduziu a feature (se conhecida e relevante)
- Validation report anterior se o bug foi introduzido recentemente

**Nunca por padrão:**
- `PROJECT_LOG.md`
- `ROADMAP.md`
- `docs/IMPLEMENTATION_STATUS.md`
- Validation reports não relacionados
- `docs_old/**`

## Não ler por padrão

Ver acima.

## Edições permitidas

- Arquivos de código que causam diretamente o bug (mudança mínima)
- Sem refactoring do código ao redor

## Edições proibidas

- Arquivos não relacionados ao bug
- Movimentação de spec
- Mudanças de roadmap
- Adições de feature além do bug fix

## Validação

Depois do fix:
- `tools/docs/validate_docs.ps1` (se docs mudaram — raro)
- `dotnet build Assembly-CSharp.csproj` (sempre)
- `dotnet build Assembly-CSharp-Editor.csproj` (se arquivos de editor mudaram)
- `/review-non-regression` — verificar que o fix não introduziu violações

## Classificação de Bug

Depois do fix, classifique como:
- **Regression**: introduzido por uma spec recente — adicionar ao backlog se o padrão precisar de tratamento
- **Gap**: limitação conhecida não coberta por nenhuma spec — adicionar à prioridade de backlog apropriada

## Quando parar e reportar

- A root cause está em um arquivo fora do scope que o humano aprovou
- O fix exigiria mudar comportamento coberto por uma spec ainda não implementada
- O fix exigiria mudar o save schema

## Saída esperada

```markdown
## Bug Fix — <description>

**Root Cause:** <cause>
**Fix:** <what changed>
**Files Changed:** <list>
**Classification:** Regression / Gap

### Validation
| Level | Result |
|-------|--------|
| C# runtime | PASS 0E/0W |
| Non-regression | PASS |

### Residual Risk
[if any]
```
