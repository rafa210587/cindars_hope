# /finish-spec

Closeout phase-aware com requisito de human test scenario. Promove a spec para `implementados/` somente quando a evidência exigida existe.

**Argumentos:** `$ARGUMENTS` — ID da spec (usado para localizar o execution report e determinar a elegibilidade de promoção)

---

## Objetivo

Checar evidência, determinar o status de fase, promover se elegível, atualizar a documentação.

---

## Leitura mínima

1. `CLAUDE.md`
2. `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md` — taxonomia de fases e regras de promoção
3. Execution report desta spec: `docs/validation/<spec_id>_execution_report.md`
4. O próprio arquivo da spec (para checar requisitos de Phase 2-3)

## Leitura condicional obrigatória

- `docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md` — se a spec de runtime exige validação humana de Phase 3
- Arquivo de human test scenario: `docs/validation/playmode/<spec_id>_human_test_scenario.md` (se runtime)

## Não ler por padrão

```
PROJECT_LOG.md (full)
ROADMAP.md
SPEC_EXECUTION_ORDER.md (full)
```

---

## Elegibilidade de promoção

### Spec docs-only (sem C#, sem Unity)

Exigido: `BUILD_VALIDATED` (docs PASS)
→ Nenhum test scenario exigido.
→ Pode promover depois que a docs validation passar.

### Spec de código (mudanças em C#, sem game objects do Unity)

Exigido: `BUILD_VALIDATED` (dotnet build 0E/0W + docs PASS)
→ Se a spec muda lógica puramente mecânica (sem UI, event, combat, save): Phase 3 opcional.
→ Se a spec muda UI state ou event publishing: human test scenario exigido.
→ Pode promover depois que a validação de Phase 1-2 passar + test scenario (se necessário).

### Spec de runtime (toca em gameplay, scene, prefab, comportamento de ScriptableObject)

Mínimo exigido: `BUILD_VALIDATED` ou `UNITY_VALIDATED` (evidência das fases aplicáveis)

**Regras de promoção:**

- Se Phase 2 (Unity validators) não foi rodada: promover só se `BUILD_VALIDATED` e nenhuma lacuna de fase documentada.
- Se Phase 2 (Unity validators) passou: pode promover para `UNITY_VALIDATED`.
- Se Phase 3 (Play Mode human validation) é necessária:
  - **Opção A:** completar Phase 3 agora → promover para `ACCEPTED`.
  - **Opção B:** deferir Phase 3 para o batch de fim de wave → promover para `DEFERRED_TO_FINAL_HUMAN_VALIDATION` se a evidência de Phase 1-2 estiver completa + a spec/report incluir referência ao checklist de validação final em `docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md`.
- NÃO promover sem evidência de Phase 1-2 aplicável ao tipo de mudança (conforme SPEC_VALIDATION_MATRIX_MASTER.md).

**Human test scenarios:**

- O human test scenario por spec é **opcional**, não obrigatório.
- Se um scenario por spec for criado: armazene em `docs/validation/playmode/<spec_id>_human_test_scenario.md` e referencie no execution report.
- Para validação em batch de fim de wave: use o checklist `docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md` no lugar.

#### Detecção de escopo

O escopo de runtime inclui:

- Qualquer mudança em comportamento de gameplay (movement, combat, interaction, progression)
- Qualquer mudança de UI state (hotbar, inventory, menus, modals)
- Qualquer lógica de persistência save/load
- Cave procedural generation ou snapshot runtime
- Event publishing ou mudanças em event handler
- Dados baseados em ScriptableObject que afetam o gameplay em runtime
- Mecânicas de farm, shop ou economy
- Equipment, skill tree ou character progression

Use a saída de `detect-change-scope.ps1` (change-scope.json) para confirmar o escopo.

---

## Checklist de promoção

- [ ] Execution report existe em `docs/validation/`
- [ ] Evidência de Phase 1 (build + docs) coletada ou NOT RUN documentado
- [ ] Evidência de Phase 2 (Unity) coletada OU NOT RUN documentado (se aplicável)
- [ ] Evidência de Phase 3 (Play Mode) coletada OU deferida para o batch de fim de wave (com referência a FINAL_HUMAN_VALIDATION_BY_WAVE.md)
- [ ] Tipo de escopo determinado (docs-only / code non-gameplay / runtime-gameplay)
- [ ] Elegibilidade de promoção confirmada conforme as regras acima

**Se NÃO elegível:** atualize o status do execution report para o nível de fase atual. Pare. Não promova.

**Se Phase 3 deferida:** o status passa a ser `DEFERRED_TO_FINAL_HUMAN_VALIDATION`. Garanta que o execution report referencie `docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md` e anote qual wave/domínio vai validar esta spec.

---

## Passos de closeout (quando elegível)

**Antes de mover arquivos (se for spec de runtime/gameplay):**

- Se Phase 3 (Play Mode) foi concluída com evidência: confirme que o execution report referencia os resultados.
- Se Phase 3 foi deferida para o fim de wave: confirme que o execution report referencia `docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md`.
- Se um human test scenario por spec foi criado: confirme que está armazenado em `docs/validation/playmode/<spec_id>_human_test_scenario.md` e referenciado no report (opcional).
- Se Phase 3 ainda não foi documentada: atualize o status do execution report para `BUILD_VALIDATED` ou `UNITY_VALIDATED` (dependendo da evidência de Phase 2) e marque o status de promoção conforme isso.

**Se elegível:**

1. Mova a spec: `.specs/a_implementar/<spec>.md` → `.specs/implementados/<spec>.md`
2. Adicione o header de evidência ao arquivo da spec:
   ```
   ---
   status: implemented
   implemented_date: YYYY-MM-DD
   phase_status: <BUILD_VALIDATED / ACCEPTED>
   evidence: docs/validation/<spec_id>_execution_report.md
   human_test_scenario: docs/validation/playmode/<spec_id>_human_test_scenario.md (if runtime)
   ---
   ```
3. Se existir refinement: mova de `docs/refinements/a_implementar/` para `docs/refinements/implementados/`
4. Atualize `PROJECT_LOG.md` — adicione uma entrada curta
5. Atualize `docs/IMPLEMENTATION_STATUS.md` — atualize o status da spec
6. Rode `tools/docs/validate_docs.ps1` — deve dar PASS

---

## Saída esperada

```markdown
## Closeout — <SPEC_ID>

**Date:** YYYY-MM-DD
**Phase Status:** <BUILD_VALIDATED / UNITY_VALIDATED / ACCEPTED / PARTIAL>
**Promoted:** YES / NO

### Promotion Decision
[Why promoted or not promoted]

### Evidence
[Execution report path]
[Validation results]

### Phase Summary
| Phase | Status |
|-------|--------|
| Phase 0 (Audit) | COMPLETE / NOT RUN |
| Phase 1 (Build) | PASS / FAIL / NOT RUN |
| Phase 2 (Unity) | PASS / FAIL / NOT RUN — PENDING |
| Phase 3 (Play Mode) | PASS / FAIL / NOT RUN — PENDING |

### Human Validation Plan (if Phase 3 applies)
[If spec is runtime/gameplay and Phase 3 applies:]
- **Option A (immediate):** Per-spec scenario at `docs/validation/playmode/<spec_id>_human_test_scenario.md` + test results documented in Phase 3 section.
- **Option B (deferred):** Wave-end batch validation via `docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md` checklist (reference in execution report).
- Expected scope: [domain/feature description]
- Status: [DEFERRED_TO_FINAL_HUMAN_VALIDATION if Option B, or ACCEPTED if Option A with evidence]

[If spec is docs-only or code-only non-gameplay:]
- Not applicable (Phase 3 not required)

### Expected Gameplay Behavior (if applicable)
[What the human should observe when testing:]
- Feature works without crash
- Expected visual/audio feedback
- Expected state changes
- No forbidden console errors
- Save/load persists feature state (if applicable)

### Next Step
[If Phase 2-3 pending: list required actions]
[If accepted: suggest next spec]
```

---

## Regras

- NÃO promova sem execution report
- NÃO afirme ACCEPTED sem evidência de Phase 2-3 se a spec exigir
- NÃO promova spec de runtime/gameplay sem arquivo de human test scenario
- NÃO pule a docs validation depois de mover arquivos
- NÃO faça push nem abra PR

## Notas de uso

**Determine o tipo da spec antes de rodar este command:**
- Use `detect-change-scope.ps1` para gerar `change-scope.json`
- Leia o execution report para entender o que mudou
- Cheque a saída dos validators de Phase 2-3 quanto a resultados de Unity compile/Play Mode
- Se runtime mudou e Phase 3 está ausente: atualize o report para `BUILD_VALIDATED` e pare

**Criação de test scenario:**
- Para specs de runtime: invoque a skill `/gameplay-test-scenario` antes de `/finish-spec`
- O tester humano deve seguir os passos do scenario e registrar os resultados
- A evidência do test scenario entra na seção Phase 3 do execution report
- Sem evidência de test scenario, o status de promoção é no máximo `BUILD_VALIDATED`, não `ACCEPTED`
