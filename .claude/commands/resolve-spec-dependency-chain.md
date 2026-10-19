# /resolve-spec-dependency-chain

Resolve same-wave spec dependencies before execution; use as an internal workflow reference.

> **NOTA DE RECONCILIAÇÃO (2026-06-12):** a fila wave-based foi executada e movida para executadas_build_validated/. A fila ativa é .specs/a_implementar/fable/. Exemplos com paths NN_spec_* abaixo são históricos.


Command de referência para resolver dependências de spec na mesma wave.

## Propósito

Extrair e resolver a dependency chain de uma spec antes de prosseguir com sua execução.

Isto é chamado **automaticamente** por `/execute-spec-strict` e `/loop-spec-batch-strict` quando uma dependência same-wave é encontrada. **Não** é um command voltado ao usuário, e sim uma referência para entender o fluxo de resolução.

---

## Uso manual

```text
/resolve-spec-dependency-chain .specs/a_implementar/05_spec_companion_farm_job_board_automation_runtime_execution.md
```

---

## Fluxo automático (dentro de `/execute-spec-strict` ou `/loop`)

Quando `/execute-spec-strict <spec>` encontra uma dependência same-wave:

1. **Marque a spec atual** como `BLOCKED_BY_DEPENDENCY_PENDING`.
2. **Extraia as dependências** da spec:
   - Leia a spec por inteiro
   - Procure por: `Depends on`, `Dependencies`, `Required systems`, `Required specs`, acceptance criteria
3. **Procure specs same-wave**:
   - Consulte os arquivos `.specs/a_implementar/<wave>_spec_*.md`
   - Case os nomes extraídos com os paths dos arquivos
4. **Construa a dependency chain**:
   - Crie um DAG (directed acyclic graph) das dependências
   - Identifique a root (spec sem dependências)
5. **Execute a root primeiro**:
   - Chame `/execute-spec-strict <root_spec_path>`
   - Aguarde o resultado
6. **Propague para cima**:
   - Depois que a root concluir, execute a próxima spec da chain
   - Continue até alcançar a spec original
7. **Volte para a original**:
   - Depois que a chain for resolvida, execute a spec alvo original
   - Não pivote para specs não relacionadas

---

## Saída da dependency chain

Toda resolução de dependência deve produzir:

```text
DEPENDENCY_RESOLUTION_RESULT
═════════════════════════════════════════

Original spec:           <spec_id> (path)
Depends on (direct):     <list>
Recursive dependencies:  <full chain>
Root dependency:         <spec_id>
Forbidden dependencies:  YES/NO (<list if YES>)
Next spec to execute:    <path>
Depth:                   <N specs>
Plan file:               docs/validation/WAVE_<wave>_DEPENDENCY_RESOLUTION_PLAN.md
Batch state:             docs/validation/WAVE_<wave>_BATCH_STATE.md
Can continue:            YES/NO
Stop reason:             [none | forbidden scope | future wave | pets | blocked]
```

---

## Arquivos obrigatórios atualizados

1. **`docs/validation/WAVE_<wave>_DEPENDENCY_RESOLUTION_PLAN.md`**
   - Adicione uma linha para cada spec resolvida
   - Atualize `Status` e `Commit` após a execução

2. **`docs/validation/WAVE_<wave>_BATCH_STATE.md`**
   - Atualize `Current Dependency Stack`
   - Mova specs para `Executed This Batch` após a conclusão
   - Atualize `Next Action`

---

## Exemplos de forbidden scope

Pare a resolução se a dependência for:

```text
future_spec_companion_deep_romance_romance_system_design (WAVE 06+)
spec_cave_stable_run_codex_regen_replay (future/mapped)
spec_pets_companion_taming_system (pets scope)
```

---

## Exemplo de resolução same-wave

**Cenário:** o usuário executa `companion_farm_job_board`.

```
1. Read spec → found: "Depends on: farm_animals"
2. Search .specs/a_implementar/05_spec_*.md → find farm_animals spec
3. Check farm_animals → found: "Depends on: farm_buildings"
4. Check farm_buildings → found: "Depends on: farm_building_footprints"
5. Check farm_building_footprints → found: "Depends on: farm_level1_layout"
6. Check farm_level1_layout → found: "Depends on: farm_scale_tilemap"
7. Check farm_scale_tilemap → no dependencies (ROOT)

Execution order:
  1. farm_scale_tilemap (root)
     → returns BUILD_VALIDATED + commit abc1234
  2. farm_level1_layout (depends on root result)
     → returns BUILD_VALIDATED + commit def5678
  ... continue upward ...
  N. companion_farm_job_board (original target)
     → returns BUILD_VALIDATED + commit xyz9999

Return to original: YES ✓
Pivot to unrelated: NO ✓
```

---

## Política: nunca saltos multi-wave implícitos

Se resolver as dependências exigiria executar waves futuras:

```text
STOP immediately.
Status: BLOCKED_BY_FORBIDDEN_SCOPE (future wave).
Reason: companion_farm_job_board depends on structure_upgrades (WAVE 06).
Action: Return to user; cannot auto-resolve across waves.
```

---

## Política: sem zombie partial chains

Se a resolução falhar no meio do caminho:

```text
Partially resolved: farm_scale_tilemap ✓, farm_level1_layout ✓
Blocked at: farm_buildings (BLOCKED_BY_FORBIDDEN_SCOPE: requires scene creation)
Status: Original spec becomes BLOCKED_BY_FORBIDDEN_SCOPE (not PENDING)
Dependency chain state: saved in BATCH_STATE.md for human review
Next action: User must unblock farm_buildings manually
```

---

*Created: 2026-06-08 (Reference Command)*  
*Not a user-facing command, but reference for understanding automatic resolution in `/execute-spec-strict`.*
