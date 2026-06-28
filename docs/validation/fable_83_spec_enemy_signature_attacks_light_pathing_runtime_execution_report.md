# Execution Report — fable_83: Ataques-Assinatura por Arquétipo + Pathing Leve

validated_adrs: [ADR-0005]
validated_game_rules: [cave_rules.md, combat_rules.md]

**Spec:** `fable_83_spec_enemy_signature_attacks_light_pathing_runtime`
**Status:** BUILD_VALIDATED
**Data:** 2026-06-23
**Wave:** FABLE Batch 6
**Dependências:** F24 (action sets/telegraphs) ✓, F82 (evasão/reposicionamento) ✓, F80 (roster) ✓

---

## Acceptance criteria extracted

| CA | Descrição | Evidência |
|----|-----------|-----------|
| CA-1 | 5 novos EnemyActionType + execução por tipo | Enum aditivo (BlinkStrike+1..+5); ExecuteComboStrike, ExecuteTelegraphedAoE, ExecuteSummonAdds, ExecuteMultiHitCharge, ExecuteDebuffStrike em EnemyBrain; EnemyActionExecution pura |
| CA-2 | Telegraph antes de todo ataque ofensivo | BeginAction → StartTelegraph (F04) reutilizado; EnemyBrain.ResolveAction só chama Execute* após WindupSeconds expirar |
| CA-3 | SummonAdds seeded + cap por sala | EnemyActionExecution.DeriveSummonSeed (CaveRunSeed+level+summonerId); ResolveSummonCount capped ao MaxAddsPerRoom=8; sem GUID/timestamp |
| CA-4 | Pathing leve sem navmesh | EnemyLocalAvoidance.Steer (decisão pura) + SteerWithRaycast (3 whiskers Physics2D); integrado em MoveChase com histerese |
| CA-5 | Stable-run e regressão | Enum aditivo (save-safe); nenhum dos 8 tipos originais alterados; summons seeded; sem GameObject.Find; GameEventBus para EnemyAddsSummonedEvent |

---

## Existing systems audit

| Sistema | Status | Decisão |
|---------|--------|---------|
| EnemyActionSO / EnemyActionType | EXISTENTE | Estendido (5 valores aditivos + 6 params) |
| EnemyBrain.ResolveAction / BeginAction | EXISTENTE | Estendido (switch antes do guard de dano base) |
| EnemyActionSetSO | EXISTENTE | Não alterado; assignment via assets pelo humano |
| StatusEffect database (F01) | EXISTENTE | Consumido por DebuffStrike via PlayerStatusReceiver |
| Telegraph (F04) | EXISTENTE | Reutilizado via StartTelegraph/BeginAction |
| EnemyProjectileBehaviour | EXISTENTE | Reutilizado onde aplicável |
| Spawn determinístico | EXISTENTE | Padrão System.Random seeded replicado em EnemyActionExecution.GenerateSummonPositions |
| GameEventBus | EXISTENTE | EnemyAddsSummonedEvent publicado (aditivo) |
| EnemyMoveLogic.cs | EXISTENTE | Não alterado |
| evasão reativa (F82) | EXISTENTE | Não alterada; pathing leve é camada separada |

---

## Spec Compliance Matrix

| Requisito | Arquivo / Método | Status |
|-----------|-----------------|--------|
| EnemyActionType += 5 valores (aditivo no fim) | EnemyActionSO.cs linhas 68-84 | OK |
| Params em dados (no-magic-balance-values) | EnemyActionSO — ComboHits, AoeDelay, AoeRadius, SummonCount, SummonEnemyId, DebuffStatusId | OK |
| EnemyActionExecution puro testável | EnemyActionExecution.cs (namespace Enemy) | OK |
| EnemyBrain.ResolveAction roteado | Switch EnemyActionType.ComboStrike..DebuffStrike | OK |
| Telegraph antes do dano (CA-2) | BeginAction → WindupSeconds → ResolveAction | OK |
| AoE AoeDelay como telegraph delay | WindupSeconds do SO = aoeDelay; IsTelegraphDelayComplete testado | OK |
| SummonAdds seeded (ADR-0005) | DeriveSummonSeed + GenerateSummonPositions (System.Random) | OK |
| SummonAdds cap por sala | ResolveSummonCount(requested, current) capped a MaxAddsPerRoom=8 | OK |
| EnemyAddsSummonedEvent aditivo | EnemyEvents.cs — classe nova no fim | OK |
| EnemyLocalAvoidance.cs (puro + raycast) | Steer (puro) + SteerWithRaycast (Physics2D) | OK |
| Integração em MoveChase | `if (_obstacleLayerMask.value != 0) SteerWithRaycast(...)` | OK |
| Histerese (_isAvoidingObstacle) | EnemyBrain campo + reset em OnEnable | OK |
| ComboStrike hits agendados em TickActionTimers | _pendingComboHits / _nextComboHitTime / ComboHitIntervalSeconds | OK |
| ApplySingleHitToPlayer helper compartilhado | Reutilizado por todos os 5 tipos | OK |
| Reset de estado em OnEnable | _isAvoidingObstacle, _pendingComboHits, _nextComboHitTime | OK |
| EditMode tests em Assets/_Game/Tests/EditMode/Cave/ | EnemySignatureActionsTests.cs, EnemyLocalAvoidanceTests.cs | OK |
| Arquivos novos em Assembly-CSharp.csproj | EnemyActionExecution.cs, EnemyLocalAvoidance.cs, tests | OK |

---

## Atribuição de ataques por arquétipo (CA-1 / T005)

A spec exige atribuição via EnemyActionSetSO no roster F80. O Unity Editor não está disponível para geração de assets em batchmode nesta execução.

**Decisão**: O código de execução está completo; o designer/humano deve criar/editar os EnemyActionSetSO correspondentes e atribuir os novos action types às fichas de cada arquétipo:

| Arquétipo | Ataque-assinatura | EnemyActionType |
|-----------|-------------------|-----------------|
| Duelista | Combo 3 golpes | ComboStrike |
| Conjurador | Zona marcada antes do dano | TelegraphedAoE |
| Invocador | Invocar fodder por seed | SummonAdds |
| Bruto | Charge com hits ao longo da linha | MultiHitCharge |
| Cultista | Hit + aplicar status | DebuffStrike |

**Status da atribuição**: ASSET_ASSIGNMENT_BLOCKED_UNITY_UNAVAILABLE (runtime code completo; assets requerem ação humana no Unity Editor).

---

## Validation

```
Validation method: run_strict_validation.ps1
Exit code: 1 (EXPECTED_FAIL_LEGACY_ONLY)
Assembly-CSharp: PASS (0E, 1W pré-existente: CombatTelemetrySession._blocks)
Assembly-CSharp-Editor: PASS (0E, 3W pré-existentes)
Quality check: PASS (falha de spec diff de reports legados: npc_collision, city_artisan, city_walkin, closed_chains, village_economy — todos pré-existentes, sem relação com fable_83)
Docs validation: PASS (exit 0)
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json
```

Triagem dos WARNs de strict: todos os execution reports que acusam seções ausentes são de specs anteriores a esta wave (npc_collision_and_liveliness, spec_city_artisan_stations, spec_city_real_walkin_houses, spec_closed_chains_leather_cloth_wool, spec_village_economy_four_npcs). Nenhuma menção a fable_83. EXPECTED_FAIL_LEGACY_ONLY confirmado.

---

## Testing Quality Gate

```
Testing Quality Gate
────────────────────
Changed runtime code:           YES
Changed deterministic logic:    YES (EnemyActionExecution, EnemyLocalAvoidance)
Changed Unity scene/prefab:     NO
Automated tests added/updated:  YES
Automated tests command:        dotnet build Assembly-CSharp.csproj (compila os testes; Play Mode para execução)
Manual Play Mode scenario:      DEFERRED_TO_FINAL_VALIDATION (combat + desvio em coluna)
Justification if no tests:      N/A — 20+ testes EditMode criados
Residual risk:                  Play Mode necessário para validar combo feel, AoE visual, summon materialização,
                                MultiHitCharge visual e desvio real com Physics2D + LayerMask configurada no prefab
```

**EditMode tests criados:**
- `Assets/_Game/Tests/EditMode/Cave/EnemySignatureActionsTests.cs` — 20 testes (combo/AoE/summon/multi-hit/debuff puras)
- `Assets/_Game/Tests/EditMode/Cave/EnemyLocalAvoidanceTests.cs` — 12 testes (whiskers, Steer, histerese, constantes)

---

## Honest status rationale

`BUILD_VALIDATED` — não `ACCEPTED` porque:
1. Play Mode não foi executado (Unity indisponível)
2. Atribuição de EnemyActionSetSO requer ação humana no Unity Editor
3. Pathing leve ativa somente com `_obstacleLayerMask` configurado no prefab (requer setup Unity)

Status máximo alcançável sem Unity: `BUILD_VALIDATED`.

---

## Human Validation Plan (Phase 3 — deferida)

**Status:** DEFERRED_TO_FINAL_HUMAN_VALIDATION
**Checklist de fim de wave:** `docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md`
**Domínio:** Cave / Enemy Combat / Movement

Cenário esperado (a ser incluído no batch humano):
- Duelista executa ComboStrike: 3 hits encadeados com 0.15s de intervalo, cada hit causa dano independente
- Conjurador executa TelegraphedAoE: zona marcada (telegraph vermelho/cor) → após atraso, dano em área
- Invocador executa SummonAdds: adds aparecem nas posições geradas deterministicamente (mesma run → mesmas posições)
- Bruto executa MultiHitCharge: charge em linha reta, player na trajetória recebe o hit
- Cultista executa DebuffStrike: hit normal + status visual aplicado no player
- Inimigo com `_obstacleLayerMask` configurado desvia em torno de coluna/parede sem encravar

## Remaining work (para promoção a ACCEPTED)

- [ ] Humano configurar `_obstacleLayerMask` nos prefabs de inimigo (layer "Environment" ou equivalente)
- [ ] Humano criar/editar EnemyActionSetSO para duelista/conjurador/invocador/bruto/cultista com os novos action types
- [ ] Executar cenário Play Mode: combo encadeado (3 hits), AoE telegrafado (zona marcada → dano), summon (adds aparecem), MultiHitCharge (charge que acerta linha), DebuffStrike (status aplicado), perseguição com desvio em torno de coluna
- [ ] Rodar Unity Test Runner EditMode para confirmar 32 novos testes passando

## Dependency Chain

```
Original target: fable_83
Dependency chain: F24 (BUILD_VALIDATED) → F82 (BUILD_VALIDATED) → F83 (this)
Forbidden dependencies: none
Resolved depth: 3
Can continue: YES (spec é a última da cadeia)
```
