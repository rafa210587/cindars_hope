# Cindar's Hope — Validation Matrix Master

> **Status:** matriz canônica de validação por tipo de mudança.  
> **Local:** `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`  
> **Tipo:** governança/validação; não é spec implementável.  
> **Função:** definir evidência mínima, validações obrigatórias e status máximo para specs e execuções.  
> **Regra:** este documento orienta validação; nenhum agente deve implementar código a partir dele isoladamente.

---

## 1. Fontes relacionadas

```text
docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
docs/specs/SPEC_GENERATION_ROADMAP_MASTER.md
.claude/rules/testing-quality-gate.md
.claude/skills/spec-execution/SKILL.md
.claude/skills/unity-validation/SKILL.md
docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
```

A regra `.claude/rules/testing-quality-gate.md` é a fonte operacional para quality gate de specs com código/runtime.

---

## 2. Taxonomia de status

```text
BUILD_VALIDATED
  Docs/build básicos passaram. Não prova comportamento.

UNITY_VALIDATED
  Unity compile/validators passaram. Não prova Play Mode completo.

PLAYMODE_VALIDATED
  Play Mode automatizado ou humano final passou com evidência.

DEFERRED_TO_FINAL_HUMAN_VALIDATION
  Cenário humano está documentado, mas será executado no fim do lote/wave.

ACCEPTED
  Todas as validações obrigatórias foram satisfeitas ou existe exceção humana explícita.

PARTIAL
  Entrega parcial ou validação obrigatória ausente com justificativa/risco.

BLOCKED
  Não pode prosseguir sem decisão/correção/dependência.
```

---

## 3. Matriz resumida

| Tipo de mudança | Docs validation | dotnet build | Unity compile | EditMode | PlayMode automated/final human scenario | Regression | Status máximo sem evidência completa |
|---|---:|---:|---:|---:|---:|---:|---|
| Docs-only/governance | YES | NO | NO | NO | NO | NO | ACCEPTED se docs PASS |
| Registry/spec docs | YES | NO | NO | NO | NO | NO | ACCEPTED se links/paths consistentes |
| Pure C# deterministic logic | YES se docs | YES | YES quando Unity code | YES quando praticável | NO, salvo gameplay | YES se bugfix | PARTIAL sem teste/justificativa |
| Runtime manager/service | YES se docs | YES | YES | YES se lógica determinística | CONDITIONAL | YES se bugfix | BUILD_VALIDATED sem PlayMode quando gameplay |
| Save/load schema/provider | YES | YES | YES | YES | CONDITIONAL | YES se bugfix | PARTIAL sem round-trip/justificativa |
| Event contracts | YES | YES | YES | YES | NO, salvo integração em cena | YES se bugfix | PARTIAL sem teste/justificativa |
| UI/menu/modal/HUD | YES | YES se C# | YES | CONDITIONAL | YES | YES se bugfix | BUILD_VALIDATED sem cenário |
| Scene/prefab/asset wiring | YES se docs | NO/YES se C# | YES | NO | YES | YES se bugfix | BUILD_VALIDATED sem cenário |
| Quest/objective/reward | YES | YES | YES | YES | CONDITIONAL | YES se bugfix | PARTIAL sem idempotência |
| Economy/inventory/equipment | YES | YES | YES | YES | CONDITIONAL | YES se bugfix | PARTIAL sem teste/justificativa |
| Combat/status/skill rules | YES | YES | YES | YES | YES se gameplay feel/cena | YES se bugfix | BUILD_VALIDATED sem cenário |
| Cave procedural/runtime | YES | YES | YES | YES para seed/rules | YES | YES se bugfix | BUILD_VALIDATED sem cenário |
| Bugfix | YES se docs | YES se C# | YES se Unity code | YES quando praticável | YES se runtime/UI/cena | YES ou risco residual | PARTIAL sem regressão/justificativa |
| Asset-only/import-only | YES se docs | NO | YES | NO | YES se wiring visual/cena | CONDITIONAL | UNITY_VALIDATED ou BUILD_VALIDATED |

---

## 4. Status caps obrigatórios

```text
Compile/build não é suficiente para ACCEPTED em spec runtime/gameplay.
Mudança de lógica determinística exige EditMode tests quando praticável.
Mudança de UI/cena/input/prefab exige PlayMode automatizado ou cenário humano final documentado.
Bugfix exige regression test ou risco residual documentado.
Sem teste obrigatório nem justificativa, status máximo é PARTIAL.
Runtime/gameplay sem PlayMode automatizado nem cenário humano final documentado fica no máximo BUILD_VALIDATED.
Validação humana real fica deferida para o final do lote/wave, salvo exceção explícita do usuário.
```

---

## 5. Evidência mínima para ACCEPTED

### Docs-only/governance

```text
Docs validation PASS;
links e paths consistentes;
nenhum runtime/code alterado.
```

### Runtime sem gameplay visível

```text
dotnet build PASS;
Unity compile PASS quando aplicável;
EditMode tests PASS quando houver lógica determinística testável;
execution report com Testing Quality Gate.
```

### Runtime/gameplay/UI/cena

```text
dotnet build PASS quando houver C#;
Unity compile PASS;
EditMode tests PASS quando houver lógica determinística;
PlayMode automated PASS ou cenário humano final documentado;
status humano real DEFERRED_TO_FINAL_VALIDATION até execução final.
```

### Save/load

```text
round-trip ou justificativa explícita;
defaults/missing sections testados quando aplicável;
invalid ID fallback testado quando aplicável;
sem Unity references em DTOs;
restore order documentado quando alterado.
```

### Quest/reward

```text
condition/trigger testável;
reward idempotente;
quest state persistível se aplicável;
spoiler visibility respeitada quando houver UI/log.
```

---

## 6. Como registrar NOT RUN

Quando validação não rodar:

```text
Validation: NOT RUN
Reason: <motivo objetivo>
Impact: <risco>
Mitigation: <como será coberto depois>
Max status: <status permitido>
```

Motivos aceitáveis:

```text
ambiente local sem Unity Editor;
harness ainda não implementado;
change docs-only;
automação PlayMode instável e cenário final humano documentado;
validação bloqueada por erro anterior reportado.
```

Motivos não aceitáveis:

```text
não rodei porque parecia simples;
compile passou então basta;
não havia tempo;
não sei testar e não registrei risco.
```

---

## 7. Como registrar risco residual

Formato obrigatório:

```text
Residual risk:
- What could still be broken:
- Why it was not fully validated:
- How final validation will catch it:
- Max allowed status before final validation:
```

---

## 8. Relação com validação humana final

`docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md` concentra os passos humanos finais.

Regra:

```text
Não pedir ao usuário para validar spec a spec.
Specs runtime podem criar ou apontar cenário final.
A validação real fica DEFERRED_TO_FINAL_VALIDATION até o fim do lote/wave.
Não declarar Play Mode humano PASS antes de execução real.
```

---

## 9. Testing Quality Gate em specs futuras

Toda spec deve conter:

```md
## Testing Quality Gate

- Changed deterministic logic: YES/NO
- Requires EditMode tests: YES/NO
- Requires PlayMode automated or final human scenario: YES/NO
- Requires regression test: YES/NO
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION / NOT REQUIRED
- Minimum validation evidence for ACCEPTED: <text>
```

---

## 10. Checklist de report

Execution report deve incluir:

```text
Spec ID;
Branch/commit;
Files changed;
Forbidden files touched: YES/NO;
Docs validation;
dotnet build;
Unity compile;
EditMode tests;
PlayMode automated;
Final human scenario;
Testing Quality Gate;
NOT RUN entries;
Residual risk;
Final status;
```

---

## 11. Anti-regressão

```text
Não aceitar runtime só por compile.
Não pedir Play Mode humano por spec.
Não duplicar regras em várias fontes quando basta referenciar.
Não promover spec sem evidência compatível.
Não esconder validação ausente atrás de ACCEPTED.
Não atualizar status final se CURRENT_STATE.md aponta blocker relevante.
```
