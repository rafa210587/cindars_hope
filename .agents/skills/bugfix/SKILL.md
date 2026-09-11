---
name: bugfix
description: "Corrige um bug específico dentro do scope dos arquivos afetados. Contexto mínimo. Sem roadmap."
---

# /bugfix

Corrige um bug específico dentro do scope dos arquivos afetados. Contexto mínimo. Sem roadmap.

**Arguments:** `$ARGUMENTS` — descrição ou referência do bug (ex.: `"save not persisting health"`)

---

## Objetivo

Identificar a root cause, corrigir de forma mínima, validar, documentar. Não refatore além da correção.

---

## Leitura mínima

1. `CLAUDE.md`
2. `docs/project/CURRENT_STATE.md` — verifique se o bug é um blocker conhecido
3. Arquivos diretamente relacionados ao bug

## Leitura opcional

- Spec que originalmente implementou a feature com bug (se conhecida)
- Prior validation report se o bug foi introduzido por uma mudança recente

## Não ler por padrão

```
PROJECT_LOG.md
ROADMAP.md
docs/IMPLEMENTATION_STATUS.md
SPEC_EXECUTION_ORDER.md
unrelated validation reports
```

---

## Procedimento

1. Leia o bug report / descrição
2. Identifique os arquivos afetados
3. Rastreie a root cause
4. Implemente a correção mínima
5. Rode a validação:
   - Docs: se algum .md mudou
   - C# build: se algum .cs mudou
   - Non-regression review: verifique que a correção não introduz novas violações
6. Classifique: regression (introduzida por spec recente) vs. gap (limitação conhecida)
7. Crie o commit com mensagem em português descrevendo a correção

---

## Edições permitidas

- Source files que diretamente causam o bug
- Docs se o bug report ou validation doc precisar de atualização

## Edições proibidas

- Nenhum refactoring além da correção
- Nenhum scope creep
- Nenhuma movimentação de spec
- Nenhuma mudança de roadmap

---

## Saída esperada

```markdown
## Bug Fix — <description>

**Root Cause:** <what caused the bug>
**Fix:** <what was changed>
**Classification:** Regression / Gap

### Files Changed
[list]

### Validation
| Level | Result |
|-------|--------|
| Docs | PASS/NE |
| C# build | PASS/NE |
| Non-regression | PASS/WARNING |

### Residual Risk
[if any]
```
