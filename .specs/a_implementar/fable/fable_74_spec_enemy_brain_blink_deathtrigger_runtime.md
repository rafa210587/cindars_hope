# SPEC — EnemyBrain: Blink Runtime + Death-Trigger Hook (SPEC 13D)

> **Spec ID:** `fable_74_spec_enemy_brain_blink_deathtrigger_runtime`
> **Status:** A implementar
> **Wave:** FABLE Batch 12
> **Priority:** P2
> **Type:** Runtime
> **Domain:** Cave / Combat
> **Parallelizable:** CONDITIONAL
> **Parallel group:** fable_bloco_combat_13D
> **Can run with:** fable_71, fable_58, fable_60
> **Must not run with:** fable_04 (EnemyBrain compartilhado), fable_05
> **Repo lock scope:**
>   `Assets/_Game/Scripts/Enemy/EnemyBrain.cs`,
>   `Assets/_Game/Scripts/Enemy/EnemyMoveLogic.cs`,
>   `Assets/_Game/Scripts/Enemy/EnemyBrainActionExecutor.cs` (ou equivalente),
>   `Assets/_Game/Scripts/Editor/EnemyTaxonomy/CreateEnemyActionsAndSets.cs` (remover guards 13D),
>   `Assets/_Game/Scripts/Editor/Validation/ValidateSpec13EnemyBrainRuntime.cs`
> **Depends on:**
>   - fable_04 (EnemyBrain state machine e aggro com memória)
>   - fable_01 (status_slow aplicado pela ação `oathless_shade_shadow_step`)
>   - fable_06 (loot tables/vulnerability — death-trigger interage com EnemyKilledEvent)
> **Blocks:**
>   - fable_05 (boss phases podem usar blink como ability especial)
>   - fable_59 (telemetria de combate registra blink como hit-type distinto)
> **Scope:** ativar o runtime de blink (teleporte de curta distância) e o hook de
> death-trigger (ação disparada na morte do inimigo) para os inimigos que já têm essas
> ações nos seus ActionSets mas que hoje resolvem como melee ou ficam inertes (cooldown=999).
> **Out of scope:** novo sistema de pathfinding, physic-based dash, boss teleport de longa
> distância (F05), novos inimigos fora do catálogo atual, arte/animação de blink.

required_adrs: [ADR-0005-cave-stable-run-and-replay.md, ADR-0007-event-bus-gameplay-communication.md]
required_game_rules: [cave_rules.md, combat_rules.md]

---

# /speckit.specify

## Contexto

O `CreateEnemyActionsAndSets.cs` define actions com `ActionType=Blink` (mirror_adept,
oathless_shade) e uma death-trigger action (ember_tick `action_ember_tick_death_pop`) com
`cooldown=999` — os guards explícitos no código dizem:

```
// Note: blink runtime is SPEC 13D; this action uses melee resolution until then
// Death-trigger activation is SPEC 13D; cooldown=999 keeps it inactive in normal combat
```

O `EnemyBrain.cs` já tem `[Header("Databases (SPEC 13D)")]` para os SerializeFields de
`EnemyActionSetDatabaseSO` e `EnemyActionDatabaseSO`. O validator `ValidateSpec13EnemyBrainRuntime.cs`
(menu `CindarsHope > Validation > Validate SPEC 13D - EnemyBrain Runtime`) já existe e
testa 10 condições; a condição 8 declara "SPEC 13D (EnemyBrain runtime) not yet implemented".

## Problema

Dois comportamentos do catálogo de inimigos não existem em runtime:

1. **Blink curto (Phase Blink / Shadow Step):** `mirror_adept` e `oathless_shade` têm
   ações que deveriam teleportar o inimigo para perto do player e aplicar efeitos de status
   (status_slow). Hoje resolvem como melee normal — inimigos com blink não são diferentes
   dos sem blink.

2. **Death-trigger:** `ember_tick` tem `action_ember_tick_death_pop` (explosão de chama ao
   morrer, `cooldown=999`). Hoje nunca dispara. O hook de morte do `EnemyBrain` não existe.

Sem esses dois comportamentos, o catálogo de 60 criaturas tem inimigos prometidos que são
indistinguíveis em combate — o `ValidateSpec13EnemyBrainRuntime` já reporta isso como WARN.

## Objetivo

Ao final desta spec:

1. **Blink runtime:** ações do `EnemyBrain` com `ActionType == Blink` executam um
   teleporte instantâneo dentro do `BlinkRange` em direção ao player, seguido da aplicação
   dos `StatusIds` e do dano canônico da ação. O teleporte respeita o stable-run (não usa
   `UnityEngine.Random` — usa o seed determinístico do `EnemyBrain`).

2. **Death-trigger hook:** quando o `EnemyHealth` publica `EnemyDiedEvent` (ou equivalente),
   o `EnemyBrain` verifica se existe uma action com `IsDeathtrigger == true` (flag nova no
   DTO de action) e a executa uma única vez, mesmo que `cooldown=999`. O ember_tick dispara
   a explosão (dano em AoE no raio da action).

3. **Guards removidos:** os comentários `// blink runtime is SPEC 13D` e
   `// Death-trigger activation is SPEC 13D` nos ActionSets e no validator são removidos;
   o validator passa de WARN para PASS nas condições relevantes.

4. **Build PASS:** Assembly-CSharp 0E/0W; run_strict_validation exit 0.

## Fontes obrigatórias lidas

```text
Assets/_Game/Scripts/Enemy/EnemyBrain.cs
Assets/_Game/Scripts/Enemy/EnemyMoveLogic.cs
Assets/_Game/Scripts/Editor/EnemyTaxonomy/CreateEnemyActionsAndSets.cs (guards a remover)
Assets/_Game/Scripts/Editor/Validation/ValidateSpec13EnemyBrainRuntime.cs
Assets/_Game/Scripts/Combat/EnemyDataSO.cs (EnemyNaturalSizeClass, ActionType)
docs/game_rules/combat_rules.md
docs/decisions/ADR-0005-cave-stable-run-and-replay.md
.claude/rules/testing-quality-gate.md
```

## Estado atual do repo

```text
EnemyBrain.cs          — state machine com aggro/detection; SerializeFields para SPEC 13D;
                         sem executor de blink e sem hook de death-trigger.
EnemyMoveLogic.cs      — move logic: Chase/Idle/Patrol/Retreat/GuardHold; nenhum blink.
CreateEnemyActionsAndSets.cs
  mirror_adept           ActionType=Melee guard "blink runtime is SPEC 13D"
  oathless_shade         ActionType=Melee guard "full blink resolution is SPEC 13D"
  ember_tick             action_death_pop cooldown=999 "death-trigger hook for SPEC 13D"
ValidateSpec13EnemyBrainRuntime.cs
  Condição 8: "SPEC 13D (EnemyBrain runtime) not yet implemented — correct for 13C" (PASS falso)
  Condição 10: OpenWindow method em EnemyVulnerabilityState (já PASS)
EnemyKilledEvent       — publicado por EnemyHealth.Die(); campos: EnemyId, Position, Level
status_slow            — definido em fable_01; aplicado via StatusEffectDatabase
```

## Tarefas

### T1 — Audit (Phase 0)

- [ ] Listar todos os ActionSets que têm `ActionType == Blink` ou `IsDeathtrigger` no catálogo
- [ ] Confirmar campos disponíveis no DTO de action (BaseDamage, Range, StatusIds, StatusChance)
- [ ] Confirmar API de aplicação de status (fable_01) e de dano (PlayerDamageReceiver / EnemyDamagePipeline)
- [ ] Criar `docs/validation/fable_74_phase0_audit_matrix.md`

### T2 — Novo campo no DTO de action

- [ ] Adicionar `bool IsDeathtrigger` ao modelo de action (EnemyActionDataSO ou DTO equivalente)
- [ ] `CreateEnemyActionsAndSets.cs`: setar `IsDeathtrigger = true` na action `ember_tick_death_pop`;
      mudar `ActionType` de `mirror_adept` e `oathless_shade` blink actions de `Melee` → `Blink`
- [ ] Remover os comentários guard `// SPEC 13D`

### T3 — Blink executor no EnemyBrain

- [ ] Criar `EnemyBlinkExecutor` (C# puro, sem MonoBehaviour) com método
      `TryExecuteBlink(EnemyBrainContext ctx, EnemyActionData action) : BlinkResult`
- [ ] Lógica: calcular posição de destino = posição do player + offset de `BlinkRange`
      usando hash determinístico (`StableHash(enemyId | frameCount)` — sem `UnityEngine.Random`)
- [ ] Aplicar teleporte via `transform.position` (ou via `Rigidbody2D.MovePosition` se o
      inimigo usa rigidbody)
- [ ] Após teleporte: executar dano e aplicar StatusIds/StatusChance da action
- [ ] Integrar no loop de execução do `EnemyBrain` (quando a action selecionada tem `ActionType == Blink`)

### T4 — Death-trigger hook

- [ ] Subscribir `EnemyDiedEvent` (ou `EnemyKilledEvent`) no `EnemyBrain` (ou em listener leve)
- [ ] Ao receber: verificar se `ActionSet` tem action com `IsDeathtrigger == true`
- [ ] Se sim: executar a action UMA VEZ independente do cooldown (bypass `cooldown=999`)
- [ ] ember_tick death_pop: dano em AoE no raio da action (OverlapCircleAll), applica
      StatusIds (status_burn se existir), publica `DamageAppliedEvent` via GameEventBus
- [ ] Guard idempotência: flag `_deathtriggerFired` no brain — nunca dispara duas vezes

### T5 — Validator atualizado

- [ ] `ValidateSpec13EnemyBrainRuntime.cs`: atualizar condição 8 para verificar que
      `EnemyBrain` tem a lógica de blink e death-trigger (reflection check ou flag canônica)
- [ ] Condição nova: pelo menos 1 ActionSet tem `ActionType == Blink` no catálogo
- [ ] Condição nova: pelo menos 1 action tem `IsDeathtrigger == true` no catálogo
- [ ] Remover texto "not yet implemented"

### T6 — EditMode tests

- [ ] `EnemyBlinkExecutorTests.cs` em `Assets/_Game/Tests/EditMode/`:
      - blink com hash determinístico (mesmo seed → mesma posição relativa)
      - posição de destino dentro do BlinkRange
      - StatusIds aplicados após blink
- [ ] `EnemyDeathTriggerTests.cs`:
      - death-trigger dispara exatamente 1 vez
      - flag `_deathtriggerFired` impede segunda execução
      - action com `cooldown=999` é executada se `IsDeathtrigger == true`

### T7 — Gate de build

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore -clp:ErrorsOnly
if ($LASTEXITCODE -ne 0) { exit 1 }
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore -clp:ErrorsOnly
if ($LASTEXITCODE -ne 0) { exit 1 }
.\tools\docs\validate_docs.ps1
if ($LASTEXITCODE -ne 0) { exit 1 }
```

## Riscos de regressão

| Risco | Mitigação |
|-------|-----------|
| Blink usa UnityEngine.Random → quebra stable-run | Usar StableHash determinístico |
| Death-trigger dispara fora da caverna | Guard: só executa se `CaveRunManager.Instance != null` |
| Teleporte atravessa paredes | Fallback: se posição de destino colidir, mover para o ponto mais próximo livre |
| AoE do ember_tick causa dano ao próprio grupo | Filtrar por layer de inimigos no OverlapCircleAll |

## Critérios de aceitação

1. `ValidateSpec13EnemyBrainRuntime` PASS sem WARNs sobre "not yet implemented"
2. mirror_adept e oathless_shade têm `ActionType == Blink` no catálogo (detectável pelo validator)
3. ember_tick death_pop tem `IsDeathtrigger == true`
4. EditMode tests passam (blink determinístico + death-trigger idempotente)
5. Assembly-CSharp 0E/0W; Assembly-CSharp-Editor 0E/0W; validate_docs exit 0
6. Em Play Mode: matar um ember_tick aplica dano/status ao player próximo; mirror_adept
   teletransporta para perto do player ao usar a action de blink

## Stop conditions

- Build falha → parar e reportar
- `EnemyBrain` compartilhado com fable_04 ainda não executada → reportar dependência bloqueante
- Blink usa `UnityEngine.Random` → parar (violaria ADR-0005)
- AoE morte causar dano ao player fora da caverna → parar

## Report obrigatório

Criar: `docs/validation/fable_74_enemybrain_blink_deathtrigger_execution_report.md`

```text
Testing Quality Gate
────────────────────
Changed runtime code:           YES
Changed deterministic logic:    YES (blink hash)
Changed Unity scene/prefab:     NO
Automated tests added/updated:  YES
Automated tests command:        dotnet test (EditMode)
Manual Play Mode scenario:      docs/validation/fable_74_playmode_scenario.md
Justification if no tests:      N/A
Residual risk:                  blink visual sem arte (placeholder transform)
```
