# Execution Report — fable_82: Inimigos: Esquiva/Dash Reativo e Reposicionamento

**Spec ID:** `fable_82_spec_enemy_reactive_evasion_runtime`
**Status:** `BUILD_VALIDATED`
**Date:** 2026-06-23
**Wave:** FABLE Batch 6

validated_adrs: [ADR-0005]
validated_game_rules: [cave_rules.md, combat_rules.md]

---

## Acceptance Criteria Extracted

| Critério | Implementação | Status |
|----------|---------------|--------|
| CA-1: ReactiveSidestep detecta windup do player em raio, hop curto legível com cooldown e chance | `EnemyBrain.TryReactiveSidestep()` + `PlayerAttackWindupEvent` + `EnemyEvasionDecision.ShouldEvade()` | OK |
| CA-2: RepositionDash gated por role — ranged/caster evadem; tanks/swarm/guard não | `EnemyBrain.TryRepositionDash()` + `EnemyEvasionDecision.RoleCanEvade()` gate | OK |
| CA-3: EvasiveLeapToPlayer usa Leap existente como gap-closer (reuso, não novo sistema) | `EnemyBrain.TryGapCloserLeap()` chama `TryLeap()` diretamente | OK |
| CA-4: Sem magic numbers — constantes em SO de balance | Todos os parâmetros em `EnemyMovementProfileSO` (campos aditivos) | OK |
| CA-5: Stable-run intacto; sem GameObject.Find; GameEventBus para comunicação | GameEventBus para windup; RNG seeded por posição de spawn; estado não persiste | OK |

---

## Existing Systems Audit

| Sistema | Encontrado? | Ação |
|---------|-------------|------|
| EnemyBrain (states, TryLeap, TryBlink, steering) | SIM — 1864 linhas, 13 states | Estendido com branches fable_82 |
| EnemyMovementProfileSO | SIM — campos Speed/Detection/Leap/Blink/Burrow | Estendido com 10 campos aditivos de evasão |
| GameEventBus | SIM | Reutilizado para PlayerAttackWindupEvent |
| PlayerAttackController | SIM — Q/E KeyDown/KeyUp para charge | Publicação de windup no KeyDown |
| PlayerChargedAttackEvent | SIM — publicado no resolve do ataque | NÃO substitui nem altera — windup é adicional (KeyDown) |
| EnemyTelegraphStartedEvent | SIM | Reutilizado nos telegraphs de sidestep/dash |
| System.Random seeded | SIM — padrão existente no projeto | Reutilizado (seed por hash de posição de spawn) |

**Não foram criados sistemas paralelos.** EnemyBrain único estendido. Movimento via steering existente (Rigidbody2D.linearVelocity).

---

## Spec Compliance Matrix

| Requirement | Implementation | OK? |
|-------------|----------------|-----|
| CanReactiveEvade(bool) no profile | `EnemyMovementProfileSO.CanReactiveEvade` | OK |
| EvadeChance no profile | `EnemyMovementProfileSO.EvadeChance` | OK |
| EvadeCooldown no profile | `EnemyMovementProfileSO.EvadeCooldown` | OK |
| EvadeReactionRadius no profile | `EnemyMovementProfileSO.EvadeReactionRadius` | OK |
| EvadeSidestepDistance/Duration no profile | `EnemyMovementProfileSO.EvadeSidestepDistance/Duration` | OK |
| RepositionDashEnabled(bool) no profile | `EnemyMovementProfileSO.RepositionDashEnabled` | OK |
| DashCooldown no profile | `EnemyMovementProfileSO.DashCooldown` | OK |
| DashDistance/Duration no profile | `EnemyMovementProfileSO.DashDistance/Duration` | OK |
| GapCloserLeap(bool) no profile | `EnemyMovementProfileSO.GapCloserLeap` | OK |
| EnemyEvasionDecision.cs (novo, estático puro) | `Assets/_Game/Scripts/Enemy/EnemyEvasionDecision.cs` | OK |
| EnemyBrain: assina PlayerAttackWindupEvent | `OnEnable` subscribe, `OnDisable` unsubscribe | OK |
| EnemyBrain: ReactiveSidestep | `TryReactiveSidestep()` chamado em ExecuteMovement | OK |
| EnemyBrain: RepositionDash | `TryRepositionDash()` chamado em ExecuteMovement | OK |
| EnemyBrain: EvasiveLeapToPlayer | `TryGapCloserLeap()` reusa `TryLeap()` | OK |
| Gate por role: tanks/swarm/guard NÃO evadem | `EnemyEvasionDecision.RoleCanEvade()` | OK |
| Default OFF quando evasão desligada | `CanReactiveEvade=false` → comportamento idêntico ao atual | OK |
| RNG seeded sem GUID/timestamp | `new System.Random(spawnHash)` em `OnEnable` | OK |
| PlayerAttackWindupEvent (aditivo) | `Assets/_Game/Scripts/Core/Events/PlayerAttackWindupEvent.cs` | OK |
| Publicado no KeyDown do player | `PlayerAttackController.Update()` — Q e E KeyDown | OK |
| EditMode tests em `Assets/_Game/Tests/EditMode/Cave/` | `EnemyReactiveEvasionTests.cs` (20 testes) | OK |

---

## Phase 0 — Auditoria (Resultados)

### Sinalização de windup do player

- `PlayerChargedAttackEvent` existe mas é publicado no `KeyUp` (resolve) — pós-ataque, não windup.
- `PlayerAttackWindupEvent` NÃO existia. Criado como evento aditivo mínimo.
- Publicado no `GetKeyDown(Q)` e `GetKeyDown(E)` (sem candidato de interação para E).
- O brain assina via `GameEventBus.Subscribe` e mantém flag com janela de 0.5s.

### Steering reusável

- Sidestep: `_rb.linearVelocity = sideDir * speed` (mesmo padrão de `MoveRetreat/MoveChase`).
- Dash: `_rb.linearVelocity = dashDir * speed` — mesma mecânica de ChargeLine.
- Nenhuma nova física introduzida.

### Parametrização

- Todos os valores em `EnemyMovementProfileSO` (10 campos aditivos).
- `OnValidate` com clamping adicionado.
- Nenhum literal numérico de balance no código de gameplay.

---

## Validation

```text
Validation method:   run_strict_validation.ps1
Exit code:           1 (EXPECTED_FAIL_LEGACY_ONLY)
Assembly-CSharp:     PASS (0E, 1W pré-existente — CombatTelemetrySession._blocks)
Assembly-CSharp-Editor: PASS (0E, 3W pré-existentes)
Quality check:       PASS (warns são de reports legados com formato antigo)
Docs validation:     PASS (exit 0)

Warnings legados confirmados como pré-existentes (não desta spec):
  - npc_collision_and_liveliness_execution_report.md (formato antigo)
  - spec_city_artisan_stations_execution_report.md (formato antigo)
  - spec_city_real_walkin_houses_no_teleport_execution_report.md (formato antigo)
  - spec_closed_chains_leather_cloth_wool_execution_report.md (formato antigo)
  - spec_village_economy_four_npcs_execution_report.md (formato antigo)
  - Bestiary .asset files modificados (sessão anterior)
```

---

## Testing Quality Gate

```text
Testing Quality Gate
────────────────────
Changed runtime code:           YES
Changed deterministic logic:    YES (ShouldEvade, gate por role, cooldown, RNG seeded)
Changed Unity scene/prefab:     NO
Automated tests added/updated:  YES
Automated tests command:        dotnet build .\Assembly-CSharp.csproj --no-restore (compila testes)
Manual Play Mode scenario:      DEFERRED_TO_FINAL_VALIDATION (sentir esquiva/dash em duelo)
Justification if no tests:      N/A — testes criados
Residual risk:                  Play Mode deferred — feedback sensorial (sidestep legível, cooldown justo)
                                  requer validação humana em duelo; comportamento default OFF para roles
                                  não elegíveis garante que nenhum inimigo existente muda comportamento.
```

**EditMode tests criados (20 testes em `EnemyReactiveEvasionTests.cs`):**
- ShouldEvade retorna false: profile desabilitado, sem windup, cooldown ativo, role bloqueado (Tank/Swarm/Guard), player fora do raio, chance insuficiente
- ShouldEvade retorna true: Ranged e Caster com todas condições
- RoleCanEvade: cobertura abrangente de todos os 8 roles
- ShouldRepositionDash: disabled, role bloqueado (Tank), player longe, sucesso (Ranged próximo)
- Determinismo: mesmo seed → mesma sequência de rolls → mesma decisão
- SidestepDirection: perpendicular ao vetor player, inversão por flankSign

---

## Dependency Chain

```text
Original target:     fable_82
Dependency chain:    F24 (Leap/Blink/Charge + states) — BUILD_VALIDATED ✓
                     F04 (threat/pack) — BUILD_VALIDATED ✓
                     F02 (telegraph/postura) — BUILD_VALIDATED ✓
Forbidden deps:      none
Resolved depth:      1 (deps já resolvidas)
Can continue:        YES
```

---

## Files Changed

```text
NOVO:
  Assets/_Game/Scripts/Core/Events/PlayerAttackWindupEvent.cs
  Assets/_Game/Scripts/Enemy/EnemyEvasionDecision.cs
  Assets/_Game/Tests/EditMode/Cave/EnemyReactiveEvasionTests.cs
  docs/validation/fable_82_spec_enemy_reactive_evasion_runtime_execution_report.md

MODIFICADO:
  Assets/_Game/Scripts/Combat/Data/EnemyMovementProfileSO.cs (10 campos aditivos + OnValidate)
  Assets/_Game/Scripts/Combat/PlayerAttackController.cs (publica PlayerAttackWindupEvent no KeyDown)
  Assets/_Game/Scripts/Enemy/EnemyBrain.cs (campos de evasão, subscribe/unsubscribe, TryReactiveSidestep/TryRepositionDash/TryGapCloserLeap)
  Assembly-CSharp.csproj (3 novos arquivos incluídos)
```

---

## Honest Status Rationale

**Status: `BUILD_VALIDATED`**

- Todos os critérios de aceite implementados e verificados estaticamente.
- Builds 0E runtime e editor.
- 20 EditMode tests cobrem a lógica determinística pura.
- Play Mode deferred por design (spec explicita "DEFERRED_TO_FINAL_VALIDATION").
- Rollback garantido: `CanReactiveEvade=false` (default) → comportamento idêntico ao F24 existente.
- Stable-run intacto: reações não persistem; RNG seeded por posição de spawn.
- Nenhum arquivo proibido alterado.

**Residual risk (Play Mode):**
- Sidestep pode parecer muito agressivo ou muito passivo dependendo de tuning de EvadeChance/EvadeCooldown.
- Telegraph de 0.5s de janela pode ser rápido demais ou lento demais.
- Ambos endereçáveis via tuning nos SOs de profile sem recompilar.

---

## Remaining Work

- Tuning de EvadeChance/EvadeCooldown/EvadeReactionRadius nos EnemyMovementProfileSO assets (por role — Play Mode).
- Criação de profiles com CanReactiveEvade=true para inimigos Ranged/Caster elegíveis.
- Human Play Mode: duelo com assassino/arqueiro para confirmar legibilidade do sidestep.
- fable_83 (pathfinding) pode consumir GapCloserLeap para aproveitamento mais sofisticado.
