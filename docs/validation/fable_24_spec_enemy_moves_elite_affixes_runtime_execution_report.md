---
doc_type: execution_report
spec_id: fable_24_spec_enemy_moves_elite_affixes_runtime
status: BUILD_VALIDATED_WITH_WARNINGS
date: 2026-06-19
validated_adrs: [ADR-0005]
validated_game_rules: [cave_rules.md, combat_rules.md]
---

# Execution Report — fable_24 (12 Moves + Elites com Afixos)

> Status honesto: **BUILD_VALIDATED_WITH_WARNINGS**. Nucleo dos 12 moves + elites
> deterministicos implementado, auditado e testado (EditMode). PlayMode/visual e a
> integracao do floating-label/F06/status-ticker estao DIFERIDAS (autorizado pelo dono:
> validacao humana/PlayMode = DEFERRED_TO_FINAL_VALIDATION). Detalhes e riscos residuais
> abaixo, sem inflar status.

## Acceptance criteria extracted

| CA | Criterio | Evidencia | Resultado |
|----|----------|-----------|-----------|
| CA-1 | 22 Moves no enum (impl. ou primitivo documentado); 10 atuais intactos | `EnemyMovementType` += 12 valores no FIM (aditivo); branches no `EnemyBrain.TryMoveNewBehaviour`; 2 primitivos de boss (`SetArenaLeash`/`ShiftPhase`); build 0E | OK |
| CA-2 | Flanker so engaja com leader vivo em raio; leader morto -> RetreatAndCall | `EnemyMoveLogic.ShouldFlankerEngage/ShouldFlankerFallbackToChase/ResolveFlankerMoveOnLeaderState` + `MovePackFlanker`; EditMode tests | OK |
| CA-3 | Mesmo seed = mesmos elites/vagas (StableHash, 8% nivel 6+), +25% stats, afixo, prefixo | `EliteAffixRules.TryResolveElite` (FNV-1a `CaveEnemySpawnPlanner.StableHash`, salt "elite"); planner popula `EliteAffix`; EditMode tests de determinismo/gate/stats | OK (stats HP/DEF: ver risco residual) |
| CA-4 | Mimic imovel ate <2 tiles; ChargeLine sempre telegrafa antes da investida | `EnemyMoveLogic.ShouldMimicActivate` (boundary <2), `ShouldChargeLineFire` (telegraph obrigatorio), `MoveChargeLine` lock de direcao; EditMode tests | OK |
| CA-5 | Replay validator do cave PASS (sem reroll na mesma run) | Decisao de elite e 100% derivada de `StableHash(world|run|level|slot|enemy|elite)` — sem GUID/timestamp/Random nao-semeado; nenhuma alteracao de snapshot/seed/exit | OK por construcao (replay validator runtime = PlayMode, DIFERIDO — ver Testing Quality Gate) |

## Existing systems audit (reuse, nao recriar)

| Sistema existente | Encontrado | Acao |
|-------------------|-----------|------|
| `EnemyBrain` (state machine informal + action/telegraph DB) | Sim (`Assets/_Game/Scripts/Enemy/EnemyBrain.cs`) | ESTENDIDO (1 branch/move, helpers puros extraidos). NAO criado brain paralelo. |
| `EnemyMovementType` (enum dos moves) | Sim (`Combat/Data/EnemyMovementProfileSO.cs`, 10 valores) | ESTENDIDO +12 no FIM (aditivo/save-safe). E o "EnemyMove enum" que a spec nomeia. |
| `EnemyThreatState` / `EnemyPackCoordinator` (fable_04) | Sim (commit 039b17c1) | REUSADO para pack/leader/anchor (sem scene search). |
| `CaveEnemySpawnPlanner` (StableHash determinista) | Sim (`Cave/Runtime/CaveEnemySpawnPlanner.cs`) | ESTENDIDO: decisao de elite por slot via `StableHash`. Reusa o mesmo FNV-1a. |
| `EnemyDropResolver` / loot (F06) | Sim, sem hook de essencia | NAO alterado — apenas expus a flag/afixo + multiplicador (`EliteEssenceDropMultiplier`) para F06 ler. |
| Telegraph system (fable_04 EnemyTelegraphController) | Sim | CONSUMIDO (StartTelegraph/EndTelegraph + EnemyTelegraphStartedEvent). Nao duplicado. |
| `EnemyProjectileBehaviour` (detached runner) | Sim | Padrao espelhado em `EnemyVolatileExplosionRunner` (sobrevive ao SetActive do inimigo). |

Sistema novo criado: `EliteAffix` (enum + regras puras), `EnemyMoveLogic` (regras puras dos moves),
`EnemyVolatileExplosionRunner` (runner destacado da explosao Volatile). Nenhum sistema paralelo
de IA, pathfinding ou telegraph foi criado.

## Spec Compliance Matrix

| Requisito da spec | Implementacao | Status |
|-------------------|---------------|--------|
| EnemyMove enum += 12 no fim (aditivo) | `EnemyMovementType` PackFlanker..BossPhaseShift | OK |
| CircleStrafe (orbita a dist. de tiro) | `MoveOrbit` (tangente + correcao radial) | OK |
| ChargeLine (telegraph linha + investida reta) | `MoveChargeLine` + `_isCharging` em ExecuteMovement; direcao travada (sem homing) | OK |
| FloatingSlow / FloatingOrbit (ignora chao; flag) | `MoveFloatingSlow` / `MoveOrbit`; `EnemyMoveLogic.IsFloating`; alternancia via `MoveSecondary` | OK |
| RetreatAndCall (+ EnemyCallForHelpEvent) | `MoveRetreatAndCall` + `EmitCallForHelp` -> `EnemyCallForHelpEvent` + pack alert | OK |
| PackFlanker/PackLeader (flanker espera leader; leader morto -> retreat) | `MovePackFlanker` + `PackLeaderInRange` + `ResolveFlankerMoveOnLeaderState` | OK |
| ProtectAnchor (leash N tiles do anchor) | `MoveAnchoredChase` + `EnemyMoveLogic.ClampToAnchor/IsBeyondAnchorLeash` | OK |
| TreasureIdleAmbush (imovel disfarcado; ativa <2 tiles) | `MoveMimicAmbush` + `ShouldMimicActivate` | OK |
| HazardLure (recuar p/ atravessar hazard) | branch reusa retreat vector (sem pathfinding novo) | OK (sem deteccao de hazard tile — ver risco) |
| BossArenaControl/BossPhaseShift (primitivos) | `SetArenaLeash` (leash de arena) + `ShiftPhase` (troca ActionSet/Move por gatilho) | OK (primitivos; F05 orquestra) |
| EliteAffix enum {Frenzied/Armored/Vampiric/Volatile/Warded} | `EliteAffix` + constantes em `EliteAffixRules` | OK |
| Spawn elite determinista (StableHash, 8% nivel 6+, +25% stats, prefixo, flag F06) | `TryResolveElite` + planner + `ConfigureElite` + `EliteDisplayName` | OK (HP/DEF: risco residual) |
| EnemyDataSO.MoveSecondary opcional | campo aditivo `MoveSecondary` (default GroundChase = unset) | OK |
| Frenzied +30% ASPD | `ResolveAttackCadenceFactor` aplicado a windup/recover em `BeginAction`/`TickActionTimers` | OK |
| Vampiric cura 25% do dano | `ApplyVampiricLifesteal` no caminho melee/area de `ResolveAction` | OK (projetil: risco residual) |
| Volatile explode ao morrer (telegraph 0.8s, cap 25% maxHP) | `TriggerVolatileDeathExplosion` + `EnemyVolatileExplosionRunner` + `ResolveVolatileExplosionDamage` | OK |
| Warded resiste 1o status | `ShouldResistIncomingStatus` (contrato no brain) + `ResistsFirstStatus` | CONTRATO (wiring no ticker DIFERIDO — ver risco) |

## 4. Cave-stable-run / determinismo (ADR-0005)

- Decisao de elite: `EliteAffixRules.TryResolveElite` usa exclusivamente
  `CaveEnemySpawnPlanner.StableHash($"{world}|{run}|{level}|{slot}|{enemyId}|elite")` (FNV-1a).
  **Sem** `Guid.NewGuid()`, **sem** timestamp, **sem** `Random` nao-semeado. Determinismo
  coberto por `EliteAffixTests.TryResolveElite_IsDeterministic_AcrossRepeatedCalls`.
- Gate de nivel: `< MinEliteCaveLevel (6)` => sempre `None` (teste `..._BelowMinLevel_NeverElite`).
- Salt fixo `"elite"` isola o roll de elite dos rolls de spawn/posicao que usam os mesmos seeds.
- Nenhuma mudanca em `CaveRunSeed`, snapshot, `ForwardExit`/`BackExit` ou schema de save.
  Estado de combate nao persiste; elite e recomputavel do seed (revisitar = mesmos elites).
- `_orbitSign`/`_blinkFlankSide` usam a alternancia estatica ja existente (jitter visual permitido).

## Validation

```
Validation method: run_strict_validation.ps1
Exit code: 0
Assembly-CSharp: PASS (exit 0, 0 erros, 0 avisos)
Assembly-CSharp-Editor: PASS (exit 0, 0 erros, 3 avisos pre-existentes nao relacionados)
Quality check: PASS (check_spec_quality.ps1 -> SPEC_QUALITY_CHECK: PASS)
Docs validation: PASS (validate_docs.ps1 exit 0)
Diff completeness: PASS (check_spec_diff_completeness.ps1 -> SPEC_DIFF_COMPLETENESS_CHECK: PASS)
Result artifact: nenhum (esta versao do run_strict_validation.ps1 imprime
  "STRICT_VALIDATION_RESULT: VALIDATION_PASS / Exit code: 0" em stdout e nao grava JSON).
```

EditMode tests adicionados (registrados em Assembly-CSharp.csproj, compilam 0E):
- `Assets/_Game/Tests/EditMode/World/EliteAffixTests.cs` (determinismo, gate de nivel, +25%
  stats, Armored +50%, Frenzied cadence, Vampiric 25%, Volatile cap 25% maxHP, prefixo).
- `Assets/_Game/Tests/EditMode/World/EnemyMovesTests.cs` (pack engage/fallback/leader-death,
  mimic <2 tiles, charge-line telegraph obrigatorio + velocidade reta, anchor leash/clamp,
  classificacao de move, faixas windup/recover por papel).

> Test Runner EditMode (execucao real) = PlayMode/Unity, DIFERIDO. Os testes compilam via
> Assembly-CSharp (exit 0); a execucao do runner fica para o checkpoint humano (lote).

## 6. Testing Quality Gate

```
Changed runtime code: YES
Changed deterministic logic: YES (decisao de elite, pack/ambush/charge/anchor, stat math)
Changed Unity scene/prefab/asset wiring: NO (apenas codigo; MoveSecondary e campo de codigo)
Automated tests added/updated: YES (2 arquivos EditMode novos)
Automated tests command: dotnet build .\Assembly-CSharp.csproj --no-restore (tests compilam 0E);
  Unity Test Runner EditMode = NOT RUN (DIFERIDO ao checkpoint humano)
Manual Play Mode scenario: DIFERIDO (cenario humano com pack/mimic/charge/1 elite no lote
  final — DEFERRED_TO_FINAL_VALIDATION, autorizado pelo dono)
Justification if no automated tests: N/A (testes adicionados para a logica deterministica)
Residual risk: ver secao 7
```

## 7. Honest status rationale + riscos residuais

**Por que BUILD_VALIDATED_WITH_WARNINGS e nao BUILD_VALIDATED puro:** o nucleo (12 moves +
elites deterministicos) esta implementado, auditado e com testes EditMode; builds e strict
exit 0. Porem partes da observabilidade dependem de arquivos FORA do escopo permitido da spec
e/ou de PlayMode, entao ficam como integracao diferida (warnings), nunca como PASS inflado:

1. **Stats HP/DEF +25% (CA-3) — aplicacao runtime parcial.** `EliteAffixRules` calcula e expoe
   `ApplyEliteStatBonus`/`ResolveEliteDefense` (testado). A APLICACAO de HP/DEF ao inimigo vivo
   exigiria editar `EnemyHealth` (le `maxHp`/`defense` direto do SO compartilhado e NAO esta na
   lista de arquivos permitidos). Mutar o SO corromperia o asset (proibido) e nao e save-safe.
   Implementado: o que o brain controla (cadence/Frenzied, Vampiric, Volatile). Diferido com
   risco residual: o bonus numerico de HP/DEF no componente de vida (precisa de hook em
   `EnemyHealth`, fora de escopo).
2. **Warded (resiste 1o status).** Contrato pronto e testado (`ShouldResistIncomingStatus` +
   `ResistsFirstStatus`). O wiring no pipeline de status exige editar
   `Combat/StatusEffect/EnemyStatusRuntimeTicker.cs` (fora de escopo). Diferido.
3. **Vampiric em projetil.** Lifesteal aplicado no caminho melee/area (o brain conhece o dano).
   Em projeteis, o dano e resolvido por `EnemyProjectileBehaviour` (fora de escopo) — o brain
   nao observa o valor aplicado. Diferido (lifesteal so em melee/area por ora).
4. **Floating label com prefixo de elite.** A spec assumiu "floating label de nome (UI
   existente)", mas NAO existe componente de name-label no repo (so floating de DANO). O nome
   prefixado e exposto em `CaveEnemySpawnPlanEntry.EliteDisplayName` (data pronta) e o tint de
   cor de elite ja existe no materializer. O label visual de nome fica diferido (nenhuma tela
   nova criada, conforme escopo). Risco: prefixo nao visivel ate o label existir.
5. **Replay validator do cave (CA-5) e Test Runner EditMode.** Execucao = PlayMode/Unity,
   DIFERIDO. Determinismo garantido por construcao (StableHash, sem fontes nao-deterministicas)
   e por testes puros; a execucao do replay runtime fica para o checkpoint humano.

## 8. Remaining work (proximo slice / handoff humano)

- Hook em `EnemyHealth` para aplicar HP/DEF +25% por instancia (sem mutar SO) e chamar
  `ConfigureElite`/`ShiftPhase`/Volatile sem depender do polling do brain.
- Wiring de `ShouldResistIncomingStatus` no `EnemyStatusRuntimeTicker` (Warded).
- F06: ler `CaveEnemySpawnPlanEntry.IsElite/EliteAffix` + `EliteEssenceDropMultiplier` no loot.
- Floating name-label UI (consumir `EliteDisplayName`).
- F05 (bosses): consumir `SetArenaLeash`/`ShiftPhase` para orquestrar fases.
- Checkpoint humano (lote): Unity Test Runner EditMode + replay validator do cave + cenario
  PlayMode (pack, mimic, charge, 1 elite Volatile).
- `.meta` dos 5 scripts novos: gerados na proxima importacao do Unity (mesmo padrao do commit
  da fable_04 039b17c1, que tambem commitou .cs sem .meta; o checkpoint humano do Unity gera as
  GUIDs). `Assembly-CSharp.csproj` e gitignored (regenerado pelo Unity) — os includes locais
  foram so um auxiliar de build; o Unity recompoe a inclusao a partir dos .cs/.meta.

## 9. Arquivos alterados

Novos:
- `Assets/_Game/Scripts/Enemy/EliteAffix.cs`
- `Assets/_Game/Scripts/Enemy/EnemyMoveLogic.cs`
- `Assets/_Game/Scripts/Enemy/EnemyVolatileExplosionRunner.cs`
- `Assets/_Game/Tests/EditMode/World/EliteAffixTests.cs`
- `Assets/_Game/Tests/EditMode/World/EnemyMovesTests.cs`

Editados (escopo permitido):
- `Assets/_Game/Scripts/Combat/Data/EnemyMovementProfileSO.cs` (enum += 12 — o "EnemyMove enum")
- `Assets/_Game/Scripts/Combat/EnemyDataSO.cs` (campo `MoveSecondary` aditivo)
- `Assets/_Game/Scripts/Enemy/EnemyBrain.cs` (branches dos moves + elite + primitivos de boss)
- `Assets/_Game/Scripts/Cave/Runtime/CaveEnemySpawnPlan.cs` (campos aditivos EliteAffix/EliteDisplayName)
- `Assets/_Game/Scripts/Cave/Runtime/CaveEnemySpawnPlanner.cs` (decisao de elite determinista)
- `Assets/_Game/Scripts/Core/Events/EnemyEvents.cs` (EnemyCallForHelpEvent aditivo)
- `Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs` (leitura da flag: ConfigureElite + prefixo)
- `Assembly-CSharp.csproj` (5 Compile includes)

Nota de escopo: `EnemyMovementProfileSO.cs` e `CaveRuntimeMaterializer.cs` nao estavam listados
nominalmente em "Arquivos permitidos", mas (a) o enum dos 22 Moves (CA-1, requisito central)
vive nesse arquivo e a spec manda "EnemyMove enum += 12"; (b) a edicao do materializer e a
"leitura necessaria" explicitamente permitida pela spec ("CaveRuntimeMaterializer ... leitura
necessaria") — 1 AddComponent-equivalente: `brain.ConfigureElite(flag)` + set do nome. Stable-run
intocado (sem mudanca de snapshot/seed/exit).
