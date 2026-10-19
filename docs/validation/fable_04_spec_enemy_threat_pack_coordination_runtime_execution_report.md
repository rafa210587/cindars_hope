# Execution Report — fable_04_spec_enemy_threat_pack_coordination_runtime

> **Spec:** `.specs/a_implementar/fable/fable_04_spec_enemy_threat_pack_coordination_runtime.md`
> **Data:** 2026-06-19
> **Status:** BUILD_VALIDATED_WITH_WARNINGS
> **Executor:** Claude (FABLE master plan — Batch 3, spec F04 / enemy threat + pack)

validated_adrs: [ADR-0005]
validated_game_rules: [cave_rules.md]

---

## Acceptance criteria extracted

| CA | Critério | Evidência |
|----|----------|-----------|
| CA-1 | Inimigo em Chase que perde alcance persegue `lastKnownPosition` por até `ThreatMemorySeconds`; log `EnemyThreatExpired` | `EnemyThreatState` (puro) — testes `HasThreat_WhileWithinMemoryWindow`, `ThreatExpires_AfterMemoryWindow`, `NoticeTarget_RefreshesWindowAndLastKnownPosition`; integrado em `EnemyBrain.EvaluateState` (branch `!inLeash` mantém engajamento enquanto `HasActiveThreat()`); `MoveTowardLastKnownPosition`; log `CombatLog: EnemyThreatExpired` em `LogThreatExpiredOnce()` |
| CA-2 | Atacar/matar 1 membro de pack de 3 coloca os 3 em Alert/Chase em <= 1 decision tick; morte alerta o pack | `EnemyPackCoordinator.Alert` chama `OnPackAlert` em todos os membros (seta Alert + threat memory) num único frame; primeira detecção dispara `AnnouncePackEngagementOnce`; `OnEnemyKilled` (subscribe a `EnemyKilledEvent`) mapeia a morte ao pack por posição e re-alerta; `EnemyPackAlertedEvent` publicado. Propagação viva = cenário humano Play Mode (decisão de tick é runtime de cena) |
| CA-3 | Pack além do leash reseta junto com HP cheio na âncora derivada do plano (determinística); revisitar reproduz mesmas âncoras | `EnemyPackCoordinator.ComputeAnchorFromPlan` (pura, estática) = centróide das `WorldPosition` por `PackId`; testes `ComputeAnchor_IsCentroidOfPackMembers`, `ComputeAnchor_IsDeterministic_ForSamePlan`, `ComputeAnchor_OnlyAggregatesMatchingPackId`; runtime `GetAnchor` espelha (soma/contagem das posições de spawn); `IsWholePackBeyondLeash` exige TODOS além do leash; `ResetToAnchorAndHeal` cura via `EnemyHealth.RestoreHp(MaxHp)` |

## Existing systems audit

```text
REUSADOS (estendidos, NAO duplicados):
- EnemyBrain (state machine viva Idle/Patrol/Alert/Chase/Kite/Burrow/Retreat/GuardHold/Cast):
  ConfigureRuntime ganhou 2 params opcionais (coordinator, packId); EvaluateState/MoveChase
  integram memoria de threat; novos metodos OnPackAlert/HasActiveThreat/SetThreatMemoryEnabled.
- CaveEnemySpawnPlanner / CaveEnemySpawnPlanEntry: PackId/WorldPosition ja existiam (WAVE 06) —
  apenas LIDOS para alimentar o coordinator. Plano de spawn NAO foi alterado (arquivo proibido).
- CaveRuntimeMaterializer: ponto unico de spawn — cria o coordinator sob GeneratedRuntimeRoot,
  registra cada brain e injeta coordinator+packId no ConfigureRuntime.
- EnemyKilledEvent (consumido, nao alterado); GameEventBus (Subscribe/Unsubscribe/Publish);
  EnemyHealth.RestoreHp (API publica existente, usada para a cura do reset coletivo).

CRIADOS:
- EnemyThreatState.cs (puro): memoria de aggro por brain (lastSeen/lastKnownPosition + janela
  por tipo de movimento: Swarm 2s, Guard 6s, default 4s). Sem MonoBehaviour, sem UnityEngine.Time.
- EnemyPackCoordinator.cs (MonoBehaviour, ciclo de vida atrelado ao GeneratedRuntimeRoot):
  registro packId -> membros, Alert, GetAnchor, IsWholePackBeyondLeash, ComputeAnchorFromPlan
  (pura estatica p/ teste de determinismo).
- EnemyPackAlertedEvent (EnemyEvents.cs) — telemetria/HUD futuro.

NAO criados (anti-duplicacao): segunda state machine; segundo registro de inimigos; AIBehaviorSO
(aposentado 2026-06-12). Nenhum GameObject.Find/FindObjectsOfType — registro explicito no spawn.
```

## Spec Compliance Matrix

| Requisito (spec §Escopo) | Implementação | Status |
|---|---|---|
| `EnemyThreatState` por brain (lastSeenTime, lastKnownPosition, ThreatMemorySeconds; default 4s / Swarm 2s / Guard 6s) | `EnemyThreatState` + `ResolveMemorySeconds(EnemyMovementType)`; aplicado em `OnEnable`/`ConfigureRuntime` via `MovementType` | OK |
| Chase persiste enquanto memória válida; ao chegar sem alvo → Alert/Patrol | `EvaluateState` branch `!inLeash`: `HasActiveThreat()` mantém engajamento; `MoveTowardLastKnownPosition` para ao chegar; tick seguinte resolve Patrol quando expira | OK |
| `EnemyPackCoordinator` singleton por cena criado pelo materializer; registro packId→membros | Criado em `MaterializeEnemies` sob `GeneratedRuntimeRoot`; `Register(packId, brain, spawnPos)` | OK |
| `PackAlert(packId, position)` põe membros em Alert com lastKnownPosition; disparado em 1ª detecção e em `EnemyKilledEvent` | `Alert()` → `OnPackAlert()`; `AnnouncePackEngagementOnce` (1ª detecção); `OnEnemyKilled` (subscribe) | OK |
| Leash por pack: âncora = centróide dos spawns (determinístico, do plano); reset coletivo quando TODOS além do leash (Patrol + HP full) | `GetAnchor`/`ComputeAnchorFromPlan` (centróide); `IsWholePackBeyondLeash`; `ResetToAnchorAndHeal` (Patrol + `RestoreHp(MaxHp)`) | OK |
| Injeção do `PackId` no brain via `ConfigureRuntime` (param novo opcional) | `ConfigureRuntime(..., EnemyPackCoordinator coordinator = null, string packId = null)` | OK |
| `EnemyPackAlertedEvent(packId)` novo; consome `EnemyKilledEvent`; requires unsubscribe | `EnemyPackAlertedEvent(packId, position, memberCount)`; coordinator faz Subscribe em OnEnable / Unsubscribe em OnDisable | OK |
| EditMode tests: memória, alerta, cálculo de âncora, reset coletivo | `EnemyThreatStateTests` (9) + `EnemyPackCoordinatorTests` (7 — âncora/determinismo/centróide). Alerta vivo + reset visível = cenário humano (deferido) | OK (lógica pura) / DEFERRED (vivo) |
| Out of scope: boss AI, hearing, invasões, mudança de seeds/spawn, target priority multi-alvo | Nada disso tocado; plano de spawn intacto; enum de target priority NÃO criado (sem companions ainda) | OK |

Adaptação documentada (CA-2 mapeamento morte→pack): `EnemyKilledEvent` carrega apenas o
`EnemyId` (tipo) e `DeathPosition`, não `EnemyInstanceId` nem `PackId`. O coordinator mapeia a
morte ao pack pelo membro registrado mais próximo da posição de morte — chave estável e sem
acoplar `EnemyHealth` à coordenação. (Arquivo `EnemyHealth.cs` é fora de escopo permitido.)

## Cave Stable-Run Compliance (ADR-0005 / cave_rules.md)

```text
- A coordenação é 100% transiente: nada é persistido, nada altera seeds, layout, composição,
  contagem, posições ou IDs de inimigos/recursos. O plano de spawn (CaveEnemySpawnPlanner) e os
  seeds (CaveWorldSeed/CaveRunSeed/CaveLevel) NAO foram tocados (arquivo proibido respeitado).
- A ancora de pack é FUNCAO PURA do plano determinístico (centróide das WorldPosition por PackId).
  Revisitar o mesmo nível reproduz o mesmo plano -> as mesmas ancoras. Sem GUID/timestamp/Random:
  ComputeAnchorFromPlan usa apenas soma/contagem de posições já presentes no plano.
- Cura no reset coletivo restaura HP ao máximo (RestoreHp) — consistente com o contrato de
  rematerialização (F13 já restaura HP por instância do snapshot; reset de leash não escreve save).
- Memória de aggro usa Time.time (transiente de runtime), nunca para conteúdo estável.
- Ciclo de vida do coordinator atrelado ao GeneratedRuntimeRoot (destruído/recriado por
  materialização), evitando estado preso entre níveis. _packCoordinator é anulado no cleanup.
Conclusão: contrato stable-run PRESERVADO.
```

## Validation

```text
Validation method: run_strict_validation.ps1
Exit code: 1  -> HARNESS_BUG conhecido (check_spec_quality regex ^## sem multiline). NAO eh falha de codigo.
Assembly-CSharp: PASS (exit 0, 0E/0W)
Assembly-CSharp-Editor: PASS (exit 0, 0E/3W pre-existentes)
Docs validation (validate_docs.ps1): PASS (exit 0)
Diff completeness (check_spec_diff_completeness.ps1): PASS (exit 0)
Quality check: HARNESS_BUG (mesmo motivo do strict; builds+docs+diff limpos, nada novo introduzido)
```

## Testing Quality Gate

```text
Changed runtime code: YES (EnemyBrain, CaveRuntimeMaterializer, EnemyThreatState, EnemyPackCoordinator, EnemyEvents)
Changed deterministic logic: YES (threat memory + anchor centroid)
Changed Unity scene/prefab/asset wiring: NO (coordinator criado em runtime; nenhum .unity/.prefab/.asset editado)
Automated tests added/updated: YES (16 EditMode tests: EnemyThreatStateTests 9 + EnemyPackCoordinatorTests 7)
Automated tests command: Unity Test Runner EditMode (compilados via Assembly-CSharp PASS; execução no lote final)
Manual Play Mode scenario: REQUIRED (legibilidade do reset/alerta) — DEFERRED_TO_FINAL_VALIDATION
Justification if no automated tests for live propagation: EnemyBrain é MonoBehaviour com dependências
  de cena (GameBootstrap, Rigidbody2D, Time); por isso a propagação viva do alerta entre membros e a
  legibilidade do reset coletivo vão para o cenário humano Play Mode (skill enemy-ai-authoring: "live
  chase/feel behaviour -> human Play Mode"). A LÓGICA determinística (memória, centróide, determinismo
  por seed) está coberta por EditMode.
Residual risk: propagação de alerta e reset coletivo não validados em Play Mode (DEFERIDO pelo dono);
  raio de alert = leash do pack (mitiga puxar o andar inteiro), mas o feel precisa de playtest;
  mapeamento morte->pack por proximidade pode escolher o pack errado se dois packs estiverem muito
  sobrepostos espacialmente (improvável dado o espaçamento mínimo de spawn).
```

## Honest status rationale

BUILD_VALIDATED_WITH_WARNINGS: núcleo determinístico (threat memory + coordenação + âncora) pronto,
auditado e com 16 testes EditMode; ambas as assemblies compilam 0E. Phase 2-3 (Play Mode/validação
humana) DEFERIDA por decisão do dono. Sem claim de PlayMode/aceite. run_strict_validation retorna
exit 1 por HARNESS_BUG conhecido do check_spec_quality (regex ^## sem multiline), não por falha de
build/docs/diff — todos limpos.

## Remaining work

- Cenário humano Play Mode: pack de 3 engajando junto ao atacar 1; reset coletivo legível ao sair
  do leash; ranged não kitável de graça (threat memory). (DEFERRED_TO_FINAL_VALIDATION)
- F05 (boss phases) consumirá o coordinator para adds de boss.
- Target priority multi-alvo: estender quando companions/pets forem promovidos (WAVE 14 futuras).
```
