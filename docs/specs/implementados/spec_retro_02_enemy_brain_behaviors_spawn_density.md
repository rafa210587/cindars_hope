# Retro-Spec 02 — EnemyBrain Behaviors Variados + Densidade de Spawn por Profundidade

> **Spec ID:** `spec_retro_02_enemy_brain_behaviors_spawn_density`
> **Status:** RETRO_DOCUMENTED (código implementado em 2026-06; spec escrita a posteriori para reconstrutibilidade)
> **Tipo:** Retro-spec
> **Domínio:** Enemy AI / Cave runtime
> **Código que documenta:**
> - `Assets/_Game/Scripts/Enemy/EnemyBrain.cs` (estados/movimentos da slice 2026-06-12)
> - `Assets/_Game/Scripts/Cave/Runtime/CaveEnemySpawnPlanner.cs` (densidade por profundidade)
> **Evidência de execução:** `docs/validation/wave_gameplay_expansion_2026_06_12_execution_report.md`
> **Supersedida/complementada por:** `fable_01` (ApplyExternalBehaviorOverride — status effects), `fable_02`/`fable_27` (ApplyStun / posture), `fable_04` (threat/pack coordination futura), `fable_24` (moves/elite affixes futuros). Esses comportamentos NÃO são re-documentados aqui — autoridade nas specs fable.

---

# /speckit.specify

## Contexto

Antes da slice 2026-06-12, os inimigos da caverna tinham comportamento homogêneo (chase simples) e densidade baixa (14–24 por nível). O pedido humano exigiu: comportamentos variados por arquétipo de movimento, projéteis inimigos esquiváveis e mais inimigos por andar com escala de profundidade, tudo preservando o contrato de stable run (ADR-0005/FASE9F).

**Escopo desta retro-spec:** estados/movimentos adicionados na slice (Leaper, PhaseShortBlink, BurrowAmbush, Retreat low-HP, GuardHold com retorno ao posto, patrulha ancorada, re-check de range melee, projéteis inimigos) e a fórmula de densidade. O núcleo do EnemyBrain (SPEC 13D action sets, telegraph, vulnerability windows) e as adições posteriores `ApplyStun` (F02), `ApplyExternalBehaviorOverride` (F01) e posture (F27) pertencem às suas specs de origem.

## Comportamento implementado

### 1. Máquina de estados (`EnemyBrainState`)

`Idle, Patrol, Alert, Chase, AttackWindup, AttackRecover, Stunned, Dead, GuardHold, Kite, Burrow, SwarmGroup, Retreat, CastPrepare`.

- Tick de decisão: `_decisionTickSeconds = 0.3s` (sobrescrito por `MovementProfile.DecisionTickSeconds` quando > 0). `EvaluateState` roda no tick; `ExecuteMovement` roda todo frame.
- `EvaluateState` não interrompe `AttackWindup`/`AttackRecover`/`Stunned`.
- Transições base: `Idle` → `Alert` (se em detecção) ou `Patrol`; `Patrol`/`Alert` → engage quando `dist <= DetectionRange()`; estados de engage voltam a `Patrol` quando `dist > LeashRange()`.
- `DetectionRange()` = `MovementProfile.DetectionRange ?? EnemyData.detectionRadius ?? 10`; `LeashRange()` = `MovementProfile.LeashRange ?? DetectionRange()*3`.

### 2. Retreat por low-HP (papéis skittish)

- Apenas roles `Swarm`, `Ranged`, `Caster` (`EnemyDataSO.PrimaryRole`).
- Threshold `_lowHealthRetreatThreshold = 0.25` (HP atual / HP máx <= 25%), dentro do leash, dispara `Retreat` por `_retreatDurationSeconds = 2.5s`.
- Movimento: `-DirectionToPlayer() * (MoveSpeed() * 1.25)` (foge 25% mais rápido que anda).
- Ao expirar: re-engage se ainda detecta, senão Patrol.

### 3. Leaper (`EnemyMovementType.Leaper`)

- Lunge quando o player está na banda média: `attackRange <= dist <= attackRange * 3.5` (attackRange default 1.5).
- Burst: velocidade `baseSpeed * _leapSpeedMultiplier (3.2)` por `0.35s`; durante o leap a velocidade não é sobrescrita.
- Cooldown `_leapCooldownSeconds = 3.5s`. Publica `EnemyTelegraphStartedEvent` ao saltar.

### 4. PhaseShortBlink (`EnemyMovementType.PhaseShortBlink`)

- Teleporta ao flanco do player quando `dist >= preferred * 2.5` (preferred = `max(PreferredDistance, 0.9)`).
- Destino: `playerPos - toPlayer * preferred + perpendicular * flankSide * 0.5`.
- `flankSide` alterna globalmente entre brains spawnados (`s_nextBlinkFlankSide` estático invertido a cada OnEnable) — phase enemies não flanqueiam todos pelo mesmo lado.
- Cooldown `_blinkCooldownSeconds = 5s`; zera velocidade após blink; publica `EnemyTelegraphStartedEvent` no destino.

### 5. BurrowAmbush (`EnemyMovementType.BurrowAmbush` ou `MovementProfile.CanBurrow`)

- No engage, se `dist > _burrowEmergeDistance * 2` entra em `Burrow` (em vez de Chase) com visual submerso: alpha do sprite × 0.25.
- Movimento submerso: `DirectionToPlayer() * MoveSpeed() * _burrowSpeedMultiplier (1.7)`.
- Emerge quando `dist <= _burrowEmergeDistance (1.4)`: restaura alpha, vai a `Chase` e tenta ação imediatamente. Sai do leash → restaura alpha e `Patrol`.

### 6. GuardHold (`EnemyMovementType.GuardStationary`)

- Sem ação disponível, guarda fica em `GuardHold`: deriva de volta ao `_spawnAnchor` (posição de OnEnable) a `MoveSpeed() * 0.6` quando deslocado mais de 0.6 unidades (`sqrMagnitude > 0.36`), senão para.

### 7. Patrulha ancorada e Swarm

- Patrol: nova direção a cada `Random.Range(1.5, 3.5)s`, velocidade `MoveSpeed() * 0.4`. Se a distância ao `_spawnAnchor` excede `WanderRadius` (default 5, min 1), a direção força retorno ao anchor — inimigos idle ficam na própria sala.
- SwarmErratic: redireciona a cada `Random.Range(0.15, 0.5)s` com `(toPlayer * 0.6 + insideUnitCircle * 0.8).normalized * speed`.
- TankSlowPush: velocidade de chase limitada a 1.5.
- Kite (`KiteRanged`/`CasterKeepAway`): recua a velocidade cheia se `dist < preferred * 0.8`; aproxima a meia velocidade se `dist > preferred * 1.2`; senão para.

### 8. Resolução de ações (slice)

- `SelectBestAction`: primeira ação do action set pronta (cooldown) com `MinRange <= dist <= Range` (SelfBuff sempre elegível).
- **Projéteis inimigos**: `RangedProjectile`/`CastProjectile` disparam `EnemyProjectileBehaviour.SpawnTowards` (esquivável — ver retro-spec 01) com `speed = ProjectileSpeed > 0 ? ProjectileSpeed : 5`, `range = max(Range, 2)`, dano/knockback do action/data.
- **Re-check melee pós-windup**: ações melee/área re-medem a distância na resolução; se `dist > effectiveRange * 1.2` a ação ERRA (log `CombatLog: EnemyActionMissed`) — esquivar o telegraph funciona de verdade. `effectiveRange` = `AreaRadius` para `AreaPulse` com raio > 0, senão `Range`.
- Dano melee: `DamageCalculator.Calculate` + `PlayerDamageReceiver.ApplyDamage` (caminho central F03/F18/F27 — autoridade nas fable).

### 9. Densidade de spawn (`CaveEnemySpawnPlanner`)

Constantes: `MinEnemiesPerLevel = 16`, `MaxEnemiesPerLevel = 32`, `DefaultMaxEnemies = 32`, `DepthScalingHardCap = 44`, `MinDistanceFromEntrance = 5`, `MinDistanceFromExit = 2`, `MinDistanceBetweenEnemies = 2` (Manhattan).

Fórmula real (`ResolveTargetEnemyCount(levelSeed, configuredMaxEnemies, caveLevel)`):

```text
depth      = max(0, caveLevel)
minBound   = min(44, 16 + depth/12)          // +1 no mínimo a cada 12 níveis
maxBase    = max(32, configuredMaxEnemies)    // cenas antigas com _maxEnemiesPerLevel menor são elevadas a 32
maxBound   = min(44, maxBase + depth/8)       // +1 no máximo a cada 8 níveis
upperBound = max(minBound, maxBound)
range      = max(1, upperBound - minBound + 1)
count      = minBound + abs(levelSeed % range)
```

- `levelSeed = StableHash("{worldSeed}|{runSeed}|{caveLevel}|{biomeId}|enemy_spawn_plan")` — determinístico por nível e por run.
- Resolução em até 4 passes do `EnemySpawnResolver` (seed por pass: `levelSeed + pass * 13337`) até atingir o alvo; warning quando packs/profiles limitam abaixo do alvo.
- Spawn points: pontos explícitos válidos; se < 16, suplementa com walkable tiles válidos. Ordenação determinística por `StableHash("{seed}|{x}|{y}")` e seleção com espaçamento mínimo 2.
- `EnemyInstanceId` estável: `enemy_{level}_{roomId}_{index}_{enemyId}_{hash:x8}` com hash de `worldSeed|runSeed|level|roomId|index|enemyId`.
- `StableHash` = FNV-1a 32-bit (offset 2166136261, prime 16777619; `int.MinValue` → 0).
- Tags de bioma por faixa de nível: `<=10` stone, `<=25` fungal, `<=40` ice, `<=55` fire, `<=70` ruins, `<=85` deep, senão void; combinada com o biome normalizado do nível quando difere.

## Critérios de aceite (verificáveis no código atual)

1. Leaper salta apenas na banda `[attackRange, attackRange*3.5]`, com burst 3.2× por 0.35s e cooldown 3.5s.
2. Blink ocorre apenas com `dist >= preferred*2.5`, cooldown 5s, lados alternados entre instâncias.
3. Burrower aproxima submerso (alpha 25%, velocidade 1.7×) e emerge a 1.4 de distância.
4. Swarm/Ranged/Caster com HP <= 25% recuam por 2.5s a 1.25× da velocidade.
5. Guard estacionário retorna ao spawn anchor a 0.6× quando deslocado > 0.6.
6. Ações ranged/cast geram `EnemyProjectileBehaviour`; melee erra se o player saiu de `range*1.2` durante o windup.
7. `ResolveTargetEnemyCount(seed, 32, 0)` ∈ [16, 32]; profundidade 96+ atinge cap com min/max = min(44, 16+depth/12)/44; resultado idêntico para o mesmo seed.
8. Nenhum GUID/timestamp/`UnityEngine.Random` em IDs ou contagem de spawn (apenas em patrulha/jitter visual transiente).

---

# /speckit.plan

## Arquitetura real

| Arquivo | Responsabilidade |
|---|---|
| `Enemy/EnemyBrain.cs` | FSM de comportamento, seleção/execução de ações, movimentos por `EnemyMovementType` |
| `Cave/Runtime/CaveEnemySpawnPlanner.cs` | Plano determinístico de spawn (contagem, posições, IDs estáveis, facções, biome tags) |
| `Enemy/EnemyMovementProfileSO` (consumido) | MovementType, DetectionRange, LeashRange, PreferredDistance, WanderRadius, AttackRange, DecisionTickSeconds, CanBurrow |
| `Enemy/EnemyActionSO` / databases (SPEC 13D, consumidos) | Ações com Range/MinRange/Windup/Recover/Cooldown/ProjectileSpeed/DamageType |

## Contratos

- `EnemyBrain.ConfigureRuntime(enemyData, movementProfile, actionSetDb, actionDb, telegraphDb, vulnerabilityProfile)` — injeção pelo materializer da caverna (sem global search).
- Eventos GameEventBus: `EnemyActionStartedEvent`, `EnemyActionResolvedEvent`, `EnemyTelegraphStartedEvent`.
- `CaveEnemySpawnPlanner.CreatePlan(generatedLevel, runManager, profiles, packs, factionLocks, maxEnemies=32) : CaveEnemySpawnPlan`.
- `CaveEnemySpawnPlanner.StableHash(string) : int` — público, reutilizado por mercador errante (retro-spec 03) e testes.
- `CaveEnemySpawnPlanner.BuildEnemyInstanceId(...)` — público/estável.
- `EnemyBrain.SetState`, `CurrentState`, `HasResolvedActionSet` etc. — superfície de validação (SPEC 14A-FIX6).

## Decisões e invariantes

- **ADR-0005 / cave stable run**: contagem, composição, posições e IDs derivam exclusivamente de `CaveWorldSeed + CaveRunSeed + CaveLevel + salt` via StableHash FNV-1a; revisitar o mesmo nível na mesma run nunca rerola.
- **Escala de profundidade sem regenerar cenas**: `configuredMaxEnemies` serializado em cenas antigas é tratado como base (elevado a >= 32) e recebe o bônus de profundidade.
- **Esquiva real**: projéteis inimigos viajam (não hitscan) e melee re-checa range — counterplay por movimento.
- **Aleatoriedade de `UnityEngine.Random` restrita** a timing/direção de patrulha e jitter de swarm (transiente, não persistido).
- **Não copiar** o idioma `FindObjectOfType` de bootstraps em código novo (RULES — débito conhecido sob decisão).

---

# /speckit.tasks

## Reconstrução (passos para refazer do zero)

1. Definir `EnemyBrainState` com os 14 estados e o tick de decisão de 0.3s.
2. Implementar transições base + leash/detecção com fallbacks de `EnemyMovementProfileSO`/`EnemyDataSO`.
3. Implementar movimentos por `EnemyMovementType`: GroundChase, SwarmErratic, TankSlowPush, Leaper (TryLeap), PhaseShortBlink (TryBlink com flank alternado), BurrowAmbush (visual submerso), GuardStationary (GuardHold), Kite, com os números da seção de comportamento.
4. Implementar Retreat low-HP para Swarm/Ranged/Caster (25% HP, 2.5s, 1.25×).
5. Implementar pipeline de ação: SelectBestAction → BeginAction (windup + telegraph) → ResolveAction (projétil real OU melee com re-check `range*1.2`) → recover → cooldown.
6. No planner, implementar `ResolveTargetEnemyCount` com a fórmula exata (16/32/44, depth/12, depth/8) e o loop de até 4 passes com seed `levelSeed + pass*13337`.
7. Garantir suplementação de spawn points por walkable tiles quando os explícitos < 16, ordenação por StableHash e espaçamento Manhattan >= 2.

## Débitos conhecidos

- Testes EditMode existentes cobrem a densidade (`CaveEnemySpawnDensityTests`); comportamento físico (leap/blink/burrow) sem teste de caracterização — validação por cenário humano de Play Mode.
- `CastPrepare` e `SwarmGroup` existem no enum mas não têm lógica dedicada (reservados para fable_04/24).
- Patrol usa `UnityEngine.Random` (aceito como transiente); se algum dia patrulha precisar ser determinística, migrar para RNG seeded por instância.
- Threat/pack coordination, stun/posture e overrides de status: autoridade nas fable_01/02/04/27 — não duplicar aqui.
