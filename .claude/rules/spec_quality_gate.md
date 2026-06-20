# Rule: Spec Quality Gate

Absorbs `spec-promotion-requires-evidence` (stub points here). Canonical status taxonomy lives in this file only.

## Regra Central

**Build passing is not enough.** Uma spec só pode ser `BUILD_VALIDATED` com evidência de que os critérios centrais foram implementados, auditados e documentados. E command failure não é spec failure (ver Environment abaixo).

---

## Status Taxonomy (canônica)

### Durante execução (permitidos)

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

### Proibidos durante execução

`ACCEPTED`, `PLAYMODE_VALIDATED`, `PARTIAL`, `COMPLETE`, `PENDING` (ambíguos ou exigem fase final).

### Fases de promoção (closeout)

`AUDITED` → `CODE_COMPLETE` → `BUILD_VALIDATED` → `UNITY_VALIDATED` → `PLAYMODE_VALIDATED` → `ACCEPTED`. Nenhuma spec move para `implementados/` sem a evidência do nível exigido por ela; specs docs-only podem declarar Phase 2-3 `NOT IN SCOPE` e promover após `BUILD_VALIDATED`.

---

## Checklist BUILD_VALIDATED (todos obrigatórios)

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

## Proibições Absolutas

- ❌ `BUILD_VALIDATED` sem report individual ou só porque compilou
- ❌ Commitar `.claude/*.lock`
- ❌ `*Tests.cs` em `Assets/_Game/Scripts/**` (hook `protected-path-guard` bloqueia)
- ❌ Executar próxima spec se atual é `NEEDS_REWORK`/`BLOCKED`
- ❌ Avançar wave com spec fundacional `CONTRACT_ONLY_NEEDS_INTEGRATION`
- ❌ Mover para `implementados/` com status `CONTRACT_ONLY*`, `DEFERRED_UI_VISUAL`, `NEEDS_REWORK`, `BLOCKED*`, `HOLD`, future, pets

---

## Evidência obrigatória em todo report

1. Acceptance criteria extraídos (tabela com evidência)
2. Existing systems audit (encontrado/reutilizado/criado)
3. Spec Compliance Matrix (requirement → implementation)
4. Validation (docs/build/editor/test)
5. Honest status rationale
6. Remaining work

---

## Loop Batch Policy (até 10 specs)

- UMA spec por iteração; report individual; validação individual (docs PASS, Assembly-CSharp 0E, Editor 0E, quality check PASS); commit individual.
- Máximo recomendado: 3 specs em wave nova/instável; 10 em wave estabelecida ou specs homogêneas; nunca >10 sem review externo.
- Parar IMEDIATAMENTE se: `BLOCKED`, `NEEDS_REWORK`, fundacional `CONTRACT_ONLY_NEEDS_INTEGRATION`/`DEFERRED_UI_VISUAL`, build falha, docs com erro novo, quality check crítico, arquivo proibido alterado, teste em pasta errada, report ausente, status inflado.
- Batch nunca produz `ACCEPTED` nem `PLAYMODE_VALIDATED`.

### Output obrigatório por spec no loop

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
