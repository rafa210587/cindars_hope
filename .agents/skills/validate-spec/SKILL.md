---
name: validate-spec
description: "Comando de workflow do projeto (equivalente ao /validate-spec do Claude Code). Roda as validações após a implementação. Registra cada nível separadamente. Sempre documenta NOT RUN com motivo."
---

# /validate-spec

Roda as validações após a implementação. Registra cada nível separadamente. Sempre documenta NOT RUN com motivo.

**Arguments:** `$ARGUMENTS` — ID ou nome da spec (usado para identificar o que mudou)

---

## Objetivo

Executar os validation levels adequados ao que foi mudado. Registrar os resultados com honestidade.

---

## Leitura mínima

1. `CLAUDE.md`
2. Spec alvo (para determinar os validation levels exigidos)
3. `.specs/SPEC_VALIDATION_MATRIX_MASTER.md` — validation levels exigidos por tipo de mudança
4. `docs/project/CURRENT_STATE.md` (para contexto do que mudou)

## Não ler por padrão

```
PROJECT_LOG.md
ROADMAP.md
full IMPLEMENTATION_STATUS.md
```

---

## Níveis de validação (executar apenas o que se aplica)

### Nível 1 — Validação de docs (se algum arquivo .md mudou)

```powershell
.\tools\docs\validate_docs.ps1
```

Esperado: PASS 14/14

### Nível 2 — C# Runtime Build (se algum .cs em Assets/ mudou)

```powershell
dotnet restore .\Assembly-CSharp.csproj
dotnet build .\Assembly-CSharp.csproj --no-restore
```

Esperado: 0 errors, 0 new warnings

### Nível 3 — C# Editor Build (se algum editor .cs mudou)

```powershell
dotnet restore .\Assembly-CSharp-Editor.csproj
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
```

Esperado: 0 errors (warnings preexistentes são aceitáveis)

### Nível 4 — Unity Validators (Phase 2 — exige Unity Editor local)

```
CindarsHope/Repair and Validate Project
CindarsHope/Validate/Combat/Validate Combat Databases
```

Resultado: PASS / FAIL / NOT RUN

### Nível 5 — Play Mode (Phase 3 — exige humano no Unity Editor)

Conforme o checklist no execution report da spec.

Resultado: PASS / FAIL / NOT RUN

---

## Documentação de NOT RUN

Se um validation level não puder rodar, registre:

```
<Level>: NOT RUN
Reason: <specific blocker — Unity lock, sandbox, timeout, no Unity license, docs-only spec>
Command attempted: <command>
Residual risk: <what is unvalidated>
```

---

## Saída esperada

```
Validation Results — <SPEC_ID>

| Level | Type | Result | Duration | Notes |
|-------|------|--------|----------|-------|
| 1 | Docs validation | PASS 14/14 / FAIL / NE | Xs | |
| 2 | C# runtime build | PASS 0E/0W / FAIL / NE | Xs | |
| 3 | C# editor build | PASS 0E/XW / FAIL / NE | Xs | |
| 4 | Unity validators | PASS / FAIL / NOT RUN | — | Phase 2 |
| 5 | Play Mode | PASS / FAIL / NOT RUN | — | Phase 3 — human |

NE = Not Executed (not applicable for this change type)
```

---

## Quando parar e reportar

- Qualquer C# build error: pare, reporte, não alegue BUILD_VALIDATED
- Falha de docs validation: pare, reporte
- Se Level 4-5 forem NOT RUN: registre explicitamente e NÃO alegue UNITY_VALIDATED ou ACCEPTED
