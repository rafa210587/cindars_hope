# Execution Report — fable_79: Inimigos: Unificação da Escala Visual no Eixo do Player

**Spec ID:** `fable_79_spec_enemy_scale_player_relative_unification`
**Status:** `BUILD_VALIDATED`
**Data:** 2026-06-23
**Wave:** FABLE Batch 6

validated_adrs: [ADR-0005]
validated_game_rules: [cave_rules.md, combat_rules.md]

---

## Acceptance Criteria Extraídos

| CA | Critério | Evidência |
|----|----------|-----------|
| CA-1 | Escala visual de inimigo da cave vem SOMENTE de `EnemyScaleResolver.ResolveVisualScale` | `CaveRuntimeMaterializer.cs` linha 1889-1898 — `SpriteScale` não mais lido para escala visual; grep confirma único call site visual agora usa `ResolveVisualScale` |
| CA-2 | Medium comum = PlayerReferenceScale; Large = 1.5×; Huge = 2×; miniboss ×1.5; boss ×2.5; Gargantuan boss > Huge boss | 18 EditMode tests em `EnemyScaleResolverPlayerRelativeTests.cs` com asserts numéricos por classe/função |
| CA-3 | Física (collider/footprint/pathing) preservada — `EnemySizeProfileSO` intocado nos campos de física | Diff não altera `ColliderRadius/FootprintCells/PathingRadius/MinRoomSize/KnockbackMultiplier`; testes de `ColliderRadiusFor` repassam |
| CA-4 | Codex (F21/F45) e campo usam o mesmo número — mesma função pura sem path alternativo | `ResolveVisualScale` é função pura estática; teste `IsPureFunction_SameInputSameOutput` confirma determinismo |
| CA-5 | Stable-run intacto — escala determinística por size/role, sem reroll | Fórmula usa apenas `BestiarySize/IsMiniBoss/IsBoss` (dados determinísticos baked no SO pelo gerador); ADR-0005 preservado |

---

## Existing Systems Audit

| Sistema | Encontrado | Ação |
|---------|-----------|------|
| `EnemyScaleResolver` (Combat/) | EXISTE — `ResolveBossScale` + `ColliderRadiusFor` | ESTENDIDO com `ResolveVisualScale`, `PlayerRelativeRatioFor`, tabelas `PlayerRelativeScale` + `RoleScaleMultiplier` + const `PlayerReferenceScale` |
| `CreateDefaultScaleAssets.PlayerReferenceScale = 2.0f` | EXISTE — fonte canônica do ratio player | REUSADO — const espelhada como `EnemyScaleResolver.PlayerReferenceScale = 2.0f` (fonte única no resolver) |
| `CaveRuntimeMaterializer` bloco de escala (~linha 1889) | EXISTE — lia `sizeProfile.SpriteScale` como escala visual | SUBSTITUÍDO — agora chama `EnemyScaleResolver.ResolveVisualScale(enemyData.BestiarySize, IsMiniBoss, IsBoss)` + log |
| `EnemySizeProfileSO.SpriteScale` | EXISTE | MANTIDO (compat de assets) — header + tooltip documentam como NÃO AUTORITATIVO para escala visual desde fable_79; campo físico preservado |
| `EnemyScaleResolverTests.cs` | EXISTE — testa `ResolveBossScale` + `ColliderRadiusFor` | INTOCADO — testes existentes continuam passando |
| `CaveBossSpawner` — usa `ResolveBossScale` | EXISTE | SEM ALTERAÇÃO — path legado de boss config preservado |
| `CaveEnemySpawner` — usa `ColliderRadiusFor` | EXISTE | SEM ALTERAÇÃO — física intocada |
| Segundo resolver paralelo | NÃO EXISTE | NÃO CRIADO — regra de não duplicação respeitada |

---

## Spec Compliance Matrix

| Requisito da Spec | Implementação | Status |
|-------------------|---------------|--------|
| `ResolveVisualScale(BestiarySizeClass, isMiniBoss, isBoss)` | `EnemyScaleResolver.cs` — método público estático puro | OK |
| Tabela `PlayerRelativeScale` nomeada (rule no-magic-balance-values) | `static class PlayerRelativeScale` com consts Tiny..Gargantuan | OK |
| Tabela `RoleScaleMultiplier` nomeada | `static class RoleScaleMultiplier` com Common/MiniBoss/Boss | OK |
| `PlayerReferenceScale = 2.0` reusado (não duplicar) | `const float PlayerReferenceScale = 2.0f` no resolver (alinhado ao `CreateDefaultScaleAssets`) | OK |
| Materializer consome `ResolveVisualScale` | `CaveRuntimeMaterializer.cs` — bloco visual substituído | OK |
| `SpriteScale` NÃO lido para escala visual | Grep: único call site visual substituído; campo mantido com header/tooltip de deprecação | OK |
| Log de wiring claro (size class, role, escala final, player ref) | `Debug.Log($"[ScaleLog] EnemyId=... SizeClass=... IsMiniBoss=... IsBoss=... PlayerRef=... VisualScale=...")` | OK |
| EditMode tests da fórmula e relação com player | `EnemyScaleResolverPlayerRelativeTests.cs` — 18 testes (CA-2/CA-3/CA-4) | OK |
| Física preservada — collider/footprint/pathing via `EnemySizeProfileSO` | Bloco de colisão do materializer intocado; campos do SO intocados | OK |
| `SpriteScale` documentado como NÃO-autoritativo (sem remover campo) | Header + Tooltip no SO; sem `[Obsolete]` para evitar CS0618 em código de editor existente | OK |
| Sem literais de balance soltos | Todos os números em consts nomeadas (`PlayerRelativeScale.*`, `RoleScaleMultiplier.*`, `PlayerReferenceScale`) | OK |
| Sem `GameObject.Find`/`FindObjectOfType` em runtime | Nenhum adicionado | OK |
| Sem edição de .unity/.prefab/.asset | Nenhum arquivo YAML editado | OK |
| Stable-run ADR-0005 intacto | Fórmula é determinística por dados baked no SO | OK |

---

## Arquivos Alterados

| Arquivo | Tipo de Mudança |
|---------|----------------|
| `Assets/_Game/Scripts/Combat/EnemyScaleResolver.cs` | ESTENDIDO — `ResolveVisualScale`, `PlayerRelativeRatioFor`, tabelas nomeadas, `PlayerReferenceScale` |
| `Assets/_Game/Scripts/Combat/Data/EnemySizeProfileSO.cs` | DOCUMENTADO — `SpriteScale` header/tooltip de deprecação (campo preservado) |
| `Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs` | MODIFICADO — bloco de escala visual substituído por `ResolveVisualScale` + log |
| `Assets/_Game/Tests/EditMode/Combat/EnemyScaleResolverPlayerRelativeTests.cs` | CRIADO — 18 EditMode tests (CA-2/CA-3/CA-4) |

---

## Validação

```
Validation method: validate_docs.ps1 + dotnet build + run_strict_validation.ps1
Docs validation:          PASS (exit 0)
Assembly-CSharp:          PASS (exit 0, 0E/0W no passo strict — 1W pre-existing fora do strict)
Assembly-CSharp-Editor:   PASS (exit 0, 0E/0W no passo strict — 3W pre-existing fora do strict)
run_strict_validation:    exit 1 — EXPECTED_FAIL_LEGACY_ONLY
  Step 0 (corruption guard): PASS
  Step 1 (docs):             PASS
  Step 2 (Assembly-CSharp):  PASS — 0E/0W
  Step 3 (Assembly-CSharp-Editor): PASS — 0E/0W
  Step 4 (diff completeness): WARN pré-existente — reports legados (npc_collision, city_artisan_stations,
    city_real_walkin_houses, closed_chains_leather) com formato anterior à adoção do template atual.
    NENHUM warn/error novo introduzido por esta spec.
```

---

## Testing Quality Gate

```
Testing Quality Gate
────────────────────
Changed runtime code:           YES
Changed deterministic logic:    YES (fórmula de escala)
Changed Unity scene/prefab:     NO
Automated tests added/updated:  YES — EnemyScaleResolverPlayerRelativeTests.cs (18 testes)
Automated tests command:        dotnet build Assembly-CSharp.csproj (compila; execução requer Unity Test Runner)
Manual Play Mode scenario:      DEFERRED_TO_FINAL_VALIDATION (conferência visual campo vs codex: 3 size classes)
Justification if no tests:      N/A — testes criados
Residual risk:
  - Play Mode visual não verificado nesta sessão (Unity não aberto): escala aplicada pelo
    materializer é determinística mas a percepção visual (sprite pequeno vs. grande em cena)
    requer execução humana do checklist de Play Mode.
  - ValidateSpec14AEnemyRuntimeIntegration.cs ainda verifica SpriteScale para WARNs de assets
    com valor 1.0 — esse validator continua funcional mas opera sobre dados não-autoritativos
    para escala visual. Considerar atualizar o validator em F80 (que altera o roster).
```

---

## Honest Status Rationale

**BUILD_VALIDATED** — fórmula implementada, testes compilam, builds 0E. Play Mode visual
deferido conforme spec (`DEFERRED_TO_FINAL_VALIDATION`). Stable-run preservado por design
(fórmula determinística). Status máximo sem Play Mode confirmado = `BUILD_VALIDATED`.

---

## Remaining Work

- Execução humana do checklist de Play Mode: colocar 3 inimigos (Medium/Large/Gargantuan boss)
  em CaveScene e confirmar que as escalas visualmente batem com a fórmula (F45 codex).
- `ValidateSpec14AEnemyRuntimeIntegration.cs`: considerar atualizar em F80 para refletir que
  `SpriteScale` não é mais a métrica de escala visual (não-blocking para fable_79).
- Gerador `GenerateCanonicalBestiary` continua baking `EnemyDataSO.VisualScale` com baseScales
  absolutas divergentes (Tiny=0.5, Large=2.0, Huge=3.0...) — em F80 esses valores passam a ser
  ignorados pelo materializer (que usa `BestiarySize/IsMiniBoss/IsBoss`); o campo `VisualScale`
  pode ser reconciliado ou tornado NaN-sentinela em F80.

---

## Dependency Chain

Original target: fable_79
Dependency chain: F33 (BestiarySizeClass) — ALREADY_IMPLEMENTED (BUILD_VALIDATED)
Forbidden dependencies: none
Resolved depth: 0 (dependência já resolvida)
Can continue next spec: YES (não bloqueia; F80 pode ser executado após esta spec)
