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

---

## (movido de rules/spec_quality_gate.md) Spec Quality Gate

Absorbs `spec-promotion-requires-evidence` (stub points here). Canonical status taxonomy lives in this file only.

### Regra Central

**Build passing is not enough.** Uma spec só pode ser `BUILD_VALIDATED` com evidência de que os critérios centrais foram implementados, auditados e documentados. E command failure não é spec failure (ver Environment abaixo).

---

### Status Taxonomy (canônica)

#### Durante execução (permitidos)

| Status | Quando usar | Bloqueia próxima? |
|--------|-------------|---|
| `BUILD_VALIDATED` | Critérios centrais atendidos + report + validações exit 0 | não |
| `BUILD_VALIDATED_WITH_WARNINGS` | Núcleo pronto; PlayMode/visual/integração deferida | não |
| `CONTRACT_ONLY` | Só DTO/model/enum/interface; sem integração nem lógica operacional | não* |
| `CONTRACT_ONLY_NEEDS_INTEGRATION` | Contrato pronto; integração com sistema real deferida | sim, se spec fundacional |
| `DEFERRED_UI_VISUAL` | Lógica pronta; visual/scene/prefab fora de escopo | sim, se fundacional |
| `NEEDS_REWORK` | P0/P1 não atende critério central; stub; ou criou sistema paralelo em vez de reusar | **sim** |
| `BLOCKED` | Exige Packages/, ProjectSettings/, scene/prefab/asset, wave futura, pets, rewrite amplo | **sim** |
| `BLOCKED_BY_DEPENDENCY_PENDING` | TEMP — aguardando dependência same-wave (não é falha; resolver e voltar) | não (resolve) |
| `BLOCKED_BY_FORBIDDEN_SCOPE` / `_FUTURE_SCOPE` / `_PETS_SCOPE` / `_WAVE_ORDER` | Dependência fora de escopo permitido | **sim** |
| `ENV_COMMAND_RETRY_REQUIRED` | Comando Unix falhou no Windows; retry PowerShell pendente | TEMP |
| `ENV_COMMAND_FAILURE` | Retry PowerShell também falhou; falha ambiental, não da spec | só se fundamental |
| `EXPECTED_FAIL_LEGACY_ONLY` | Falha esperada de docs/config legado, já documentada | não |

#### Proibidos durante execução

`ACCEPTED`, `PLAYMODE_VALIDATED`, `PARTIAL`, `COMPLETE`, `PENDING` (ambíguos ou exigem fase final).

#### Fases de promoção (closeout)

`AUDITED` → `CODE_COMPLETE` → `BUILD_VALIDATED` → `UNITY_VALIDATED` → `PLAYMODE_VALIDATED` → `ACCEPTED`. Nenhuma spec move para `implementados/` sem a evidência do nível exigido por ela; specs docs-only podem declarar Phase 2-3 `NOT IN SCOPE` e promover após `BUILD_VALIDATED`.

---

### Checklist BUILD_VALIDATED (todos obrigatórios)

1. Execution report individual em `docs/validation/<spec_id>_execution_report.md`
2. Spec lida por inteiro; acceptance criteria extraídos e documentados
3. Sistemas existentes auditados (não criar paralelos — ver skill `system-reuse-audit`)
4. Código atende todos os critérios centrais; Spec Compliance Matrix preenchida com OK
5. `.\tools\docs\validate_docs.ps1` PASS e `.\tools\docs\run_strict_validation.ps1` exit 0
6. `Assembly-CSharp` e `Assembly-CSharp-Editor` PASS (exit 0)
7. Nenhum arquivo proibido alterado (Packages/, ProjectSettings/, scenes, prefabs, assets)
8. Testes (se criados) só em `Assets/_Game/Tests/EditMode/**`
9. Report com "Honest status rationale" + validation method documentado

---

### Proibições Absolutas

- ❌ `BUILD_VALIDATED` sem report individual ou só porque compilou
- ❌ Commitar `.claude/*.lock`
- ❌ `*Tests.cs` em `Assets/_Game/Scripts/**` (hook `protected-path-guard` bloqueia)
- ❌ Executar próxima spec se atual é `NEEDS_REWORK`/`BLOCKED`
- ❌ Avançar wave com spec fundacional `CONTRACT_ONLY_NEEDS_INTEGRATION`
- ❌ Mover para `implementados/` com status `CONTRACT_ONLY*`, `DEFERRED_UI_VISUAL`, `NEEDS_REWORK`, `BLOCKED*`, `HOLD`, future, pets

---

### Evidência obrigatória em todo report

1. Acceptance criteria extraídos (tabela com evidência)
2. Existing systems audit (encontrado/reutilizado/criado)
3. Spec Compliance Matrix (requirement → implementation)
4. Validation (docs/build/editor/test)
5. Honest status rationale
6. Remaining work

---

### Loop Batch Policy (até 10 specs)

- UMA spec por iteração; report individual; validação individual (docs PASS, Assembly-CSharp 0E, Editor 0E, quality check PASS); commit individual.
- Máximo recomendado: 3 specs em wave nova/instável; 10 em wave estabelecida ou specs homogêneas; nunca >10 sem review externo.
- Parar IMEDIATAMENTE se: `BLOCKED`, `NEEDS_REWORK`, fundacional `CONTRACT_ONLY_NEEDS_INTEGRATION`/`DEFERRED_UI_VISUAL`, build falha, docs com erro novo, quality check crítico, arquivo proibido alterado, teste em pasta errada, report ausente, status inflado.
- Batch nunca produz `ACCEPTED` nem `PLAYMODE_VALIDATED`.

#### Output obrigatório por spec no loop

```text
SPEC_RESULT:
Spec:
Status:
Docs validation:
Assembly-CSharp:
Assembly-CSharp-Editor:
Quality check:
Commit:
Can continue next spec: YES/NO
Can start next wave: NO (always no during loop)
```

---

*Updated: 2026-06-12 (consolidação do harness — versão íntegra anterior no git history)*

---

## (movido de rules/testing-quality-gate.md) Testing Quality Gate

**Compile passing não é suficiente.** Mudanças de código exigem evidência de teste ou justificativa explícita.

---

### Report block obrigatório (em todo implementation report)

```text
Testing Quality Gate
────────────────────
Changed runtime code:           YES/NO
Changed deterministic logic:    YES/NO
Changed Unity scene/prefab:     YES/NO
Automated tests added/updated:  YES/NO
Automated tests command:        <cmd ou NOT RUN>
Manual Play Mode scenario:      <path ou NOT REQUIRED>
Justification if no tests:      <texto ou N/A>
Residual risk:                  <texto>
```

---

### Quando usar EditMode tests (obrigatório)

Lógica determinística que roda fora de scene interaction exige EditMode tests em `Assets/_Game/Tests/EditMode/`:

```
save/load DTO normalization + migrations + section providers;
stable IDs + registries + invalid ID fallback;
quest conditions/triggers/reward idempotency/flags/objective progress;
economy pricing + shop stock + inventory transactions/stack/split/move;
equipment durability + repair/upgrade;
combat formulas + status effect rules;
skill tree purchase/respec + active slot validation;
calendar/day transition + weather + lunar cycle;
crafting/processing job timing;
bestiary knowledge state;
event bus publish/subscribe contracts;
parsers, adapters, pure services, validators, rule engines.
```

### Quando exigir Play Mode ou human scenario

Behavior que depende de scene/lifecycle/prefab/input exige Play Mode automatizado ou human scenario em `docs/validation/playmode/<spec_id>_human_test_scenario.md`:

```
scene objects, Unity lifecycle, input, UI canvas, modal stack;
prefabs, ScriptableObject asset wiring, physics/colliders;
NPC schedules, cave procedural runtime, player movement/feel;
shop/dialogue interaction, corpse recovery flow.
```

---

### Expectativas por domínio (cobrir ou justificar ausência)

| Domínio | Pontos obrigatórios |
|---------|---------------------|
| **save/load** | DTO defaults; legacy/missing section; invalid ID fallback; round-trip; sem Unity refs; reward idempotency |
| **quest** | condition eval; trigger; reward idempotency; flag set/clear; objective progress; spoiler visibility; anti-softlock; save/load |
| **UI** | open/close; Esc/back; input blocking; focus order; confirmation modal; empty/error state; sem movimento durante modal |

---

### Bugfix

Todo bugfix exige uma das opções: regression test que falha antes e passa depois; ou validator/checklist quando automação não é prática; ou justificativa escrita de por que não é possível. Bugfix sem cobertura = residual risk explícito.

### Justificativas permitidas para pular automação

- mudança docs-only ou asset-only com manual scenario
- wiring de scene/prefab que exige inspeção no Editor
- harness ausente sendo fechado por spec futura (nomear o blocker)

---

### Closeout impact

| Situação | Status máximo |
|----------|--------------|
| Testes ausentes sem justificativa | `PARTIAL` |
| Runtime/gameplay sem Play Mode ou human scenario | `BUILD_VALIDATED` |
| Evidência completa | `ACCEPTED` (se nível de validação exigido satisfeito) |

---

### Never claim sem evidência no repo

`unit tests passed` / `EditMode tests passed` / `PlayMode tests passed` / `regression covered` / `feature accepted`

---

## (movido de rules/spec_dependency_resolution.md) Resolução de Dependência de Spec

### Regra Central

Quando uma spec depende de outra **spec não resolvida na mesma wave**, resolva a cadeia **automaticamente**: não pergunte ao usuário, não pivote aleatoriamente, não marque BLOCKED final a menos que a dependência seja forbidden/out-of-scope.

### Algoritmo

1. Marque a spec atual como `BLOCKED_BY_DEPENDENCY_PENDING` (temporário — nunca uma falha final).
2. Empilhe a dependência no dependency stack; recurse até a raiz (depth-first).
3. Execute a raiz primeiro via `/execute-spec-strict`, depois suba de volta pela cadeia.
4. **Return-to-origin:** depois que a cadeia se resolve, execute a spec-alvo ORIGINAL antes de qualquer outra coisa. Nunca pivote para specs não relacionadas enquanto uma cadeia está aberta.

### Pare imediatamente (`BLOCKED_BY_FORBIDDEN_SCOPE`) se a dependência for

wave futura/mapeada (WAVE 06+), pets, HOLD, BLOCKED_SCOPE, ou exigir: Packages/, ProjectSettings/, criação ou edição de scene/prefab/asset, Unity Test Runner, Play Mode.

### Extração de dependência

Leia estas seções da spec: `Depends on`, `Dependencies`, `Required systems`, `Required specs`, `Blocks`, `Permissions`, e acceptance criteria que nomeiam sistemas/specs.

Statuses de dependência que permitem continuar: `READY` (resolver primeiro), `BUILD_VALIDATED[_WITH_WARNINGS]` (usar resultado), `CONTRACT_ONLY[_NEEDS_INTEGRATION]` (condicional — specs fundacionais devem esperar). Taxonomia completa: `.claude/rules/spec_quality_gate.md`.

### Artefatos obrigatórios (persistir estado entre invocações)

- `docs/validation/WAVE_<wave>_DEPENDENCY_RESOLUTION_PLAN.md` — tabela: Order | Spec | Depends On | Reason | Status | Commit
- `docs/validation/WAVE_<wave>_BATCH_STATE.md` — stack atual, executadas neste batch, specs pendentes, próxima ação

### Cláusula obrigatória nos execution reports

```text
## Dependency Chain
Original target: <spec>
Dependency chain: 1..N with statuses
Forbidden dependencies: [none | listed]
Resolved depth: <N>
Plan file / Batch state: paths above
Can continue original target: YES/NO
```

---

*Updated: 2026-06-12 (condensed — full prior text in git history). Applies to /loop-spec-batch-strict and /execute-spec-strict.*
