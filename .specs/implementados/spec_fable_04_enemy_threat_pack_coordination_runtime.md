# SPEC — Enemy AI: Threat/Aggro, Target Priority e Coordenação de Pack/Leash

> **Spec ID:** `fable_04_spec_enemy_threat_pack_coordination_runtime`
> **Status:** A implementar
> **Wave:** FABLE — Gap Closure Bloco B (inimigos)
> **Priority:** P2
> **Type:** Runtime
> **Domain:** Cave / Combat
> **Parallelizable:** CONDITIONAL
> **Parallel group:** fable_bloco_B
> **Can run with:** fable_01, fable_09, fable_11, fable_12
> **Must not run with:** fable_02 (EnemyBrain compartilhado), fable_05
> **Repo lock scope:** `Assets/_Game/Scripts/Enemy/EnemyBrain.cs`, `CaveRuntimeMaterializer.cs`
> **Depends on:**
> - slice 2026-06-12 (EnemyBrain estendido)
> - cave_001-008 + CaveEnemySpawnPlanner (PackId já existe no plano de spawn)
> **Blocks:**
> - `fable_05`
> **Scope:** aggro com memória, alerta de pack e leash compartilhado por grupo de spawn.
> **Out of scope:** boss AI (F05), invasões de fazenda/cidade (backlog futuro), hearing (não existe por decisão).

required_adrs: [ADR-0005]
required_game_rules: [cave_rules.md]

---

# /speckit.specify

## Contexto

`ENEMY_BEHAVIORS_DIRECTION.md` define threat/aggro com memória de combate, alerta de pack
(um membro detecta → grupo engaja), target priority e leash rules por grupo. O `EnemyBrain`
atual usa apenas distância instantânea (`DetectionRange`/`LeashRange`): sair do raio um frame
faz o inimigo esquecer o jogador; packs do `CaveEnemySpawnPlanner` (que já gera `PackId` por
entrada do plano!) não se coordenam — cada membro age sozinho.

## Problema

Combate na caverna vira "puxar um por um" sem risco: ranged kita infinitamente porque o
inimigo desiste na borda do raio; o conceito de pack do roster (investimento de WAVE 06 em
EnemySpawnPackSO) não tem efeito em runtime. A densidade aumentada (16-32) precisa de
coordenação para virar desafio em vez de fila.

## Objetivo

Ao final desta spec, inimigos devem manter aggro por N segundos após perder linha de
distância (threat memory), membros do mesmo PackId devem ser alertados quando um engaja
ou morre, e o leash deve ser por âncora de pack (retornam juntos, com cura ao resetar),
preservando o contrato stable-run (nenhuma mudança em seeds/spawn).

## Fontes obrigatórias lidas

```text
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
docs/design/gameplay/enemies/ENEMY_BEHAVIORS_DIRECTION.md
docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_ENEMY_BEHAVIOR_ADAPTER.md
docs/game_rules/cave_rules.md
docs/decisions/ADR-0005-cave-stable-run-and-replay.md
.claude/rules/cave-stable-run.md
.claude/rules/testing-quality-gate.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- EnemyBrain (state machine com Patrol/Chase/Kite/Burrow/Retreat/GuardHold etc.)
- CaveEnemySpawnPlanner/CaveEnemySpawnPlan (PackId, RoomId, EnemyInstanceId por entrada)
- CaveRuntimeMaterializer (configura brains; ponto único de spawn)
- EnemySpawnPackSO (dados de pack WAVE 06)
- EnemyKilledEvent (GameEventBus)
Não existe:
- threat/aggro com memória; coordenação por PackId em runtime; leash compartilhado
Auditar Fase 0:
- como o materializer expõe PackId ao objeto do inimigo (provavelmente só no plano — precisa injetar)
```

## Engineering stories

```text
Como inimigo, quero lembrar do alvo por ThreatMemorySeconds após perder alcance, para não ser kitado de graça.
Como pack, quero que o primeiro membro a detectar/morrer alerte os demais do mesmo PackId.
Como pack fora de leash, queremos resetar juntos para a âncora e recuperar HP.
Como jogador, quero que o reset seja legível (inimigos desistem visivelmente e voltam).
```

## Escopo

```text
Inclui:
- EnemyThreatState (NOVO, por brain): lastSeenTime, lastKnownPosition, ThreatMemorySeconds
  (default 4s; SwarmErratic 2s; GuardStationary 6s);
- Chase persiste enquanto memória válida (persegue lastKnownPosition; ao chegar sem alvo → Alert → Patrol);
- EnemyPackCoordinator (NOVO, singleton por cena criado pelo materializer): registro
  packId → membros; PackAlert(packId, position) coloca membros em Alert com lastKnownPosition;
  disparado em primeira detecção e em EnemyKilledEvent de membro;
- leash por pack: âncora = centróide dos spawns do pack (determinístico, vindo do plano);
  quando TODOS os membros estão além do leash, reset coletivo (estado Patrol + HP full);
- injeção do PackId no brain via ConfigureRuntime (parâmetro novo opcional);
- EditMode tests: memória de threat, alerta de pack, cálculo de âncora, reset coletivo.
```

## Fora de escopo

```text
Não inclui: boss phases (F05); hearing; invasões; mudanças em spawn plan/seeds;
target priority multi-alvo (companions/pets futuros — projetar enum, não implementar).
```

## Regras de não duplicação

```text
Não criar segunda state machine — estender EnemyBrain.
Não criar segundo registro de inimigos — coordinator alimenta-se do materializer.
Não usar GameObject.Find/FindObjectsOfType — registro explícito no spawn.
```

## Critérios de aceite

### CA-1 Threat memory
- Inimigo em Chase que perde alcance persegue lastKnownPosition por até ThreatMemorySeconds.
- Evidência: testes de EnemyThreatState (puro) + log `CombatLog: EnemyThreatExpired`.

### CA-2 Pack alert
- Atacar 1 membro de um pack de 3 coloca os 3 em Alert/Chase em <= 1 decision tick.
- Morte de membro alerta o pack. Evidência: teste do coordinator com brains fake + log.

### CA-3 Leash coletivo + stable run
- Pack além do leash reseta junto com HP cheio na âncora derivada do plano (determinística).
- Revisitar o nível reproduz mesmas âncoras (nenhum novo random). Evidência: teste de
  determinismo da âncora a partir de um CaveEnemySpawnPlan sintético.

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Enemy/
  EnemyThreatState.cs       (NOVO — puro)
  EnemyPackCoordinator.cs   (NOVO — MonoBehaviour registrado pelo materializer)
  EnemyBrain.cs             (integração: memória, alert externo, leash de pack)
Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs (injetar PackId/registrar no coordinator)
Assets/_Game/Tests/EditMode/World/{EnemyThreatStateTests, EnemyPackCoordinatorTests}.cs
```

## Contratos

### Runtime contracts
`EnemyThreatState`: `NoticeTarget(pos,time)`, `bool HasThreat(time)`, `Vector2 LastKnownPosition`.
`EnemyPackCoordinator`: `Register(packId, EnemyBrain, spawnPos)`, `Alert(packId, pos)`,
`bool IsWholePackBeyondLeash(packId)`, `Vector2 GetAnchor(packId)`.
`EnemyBrain.ConfigureRuntime(..., string packId = null)` + `OnPackAlert(Vector2 pos)`.

### Event contracts
Consome `EnemyKilledEvent` existente. Novo: `EnemyPackAlertedEvent(packId)` (telemetria/HUD futuro).

### Save contracts
N/A — aggro transiente; posições/IDs continuam do plano estável.

## Sistemas afetados

```text
Enemy AI, Cave materialization, Event bus
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Enemy/{EnemyThreatState.cs,EnemyPackCoordinator.cs,EnemyBrain.cs}
Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs
Assets/_Game/Scripts/Core/Events/EnemyEvents.cs (evento novo)
Assets/_Game/Tests/EditMode/World/** ; docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
CaveEnemySpawnPlanner.cs (plano não muda) ; *.unity/*.prefab/*.asset ; Packages/ProjectSettings
SaveManager/GameSaveData
```

## Estratégia de implementação

```md
### Fase 0 — Auditar fluxo materializer→brain e disponibilidade do PackId no objeto.
### Fase 1 — EnemyThreatState puro + integração no Chase + testes.
### Fase 2 — Coordinator + registro no spawn + alert por detecção/morte.
### Fase 3 — Leash coletivo por âncora determinística + reset com cura.
### Fase 4 — Testes, validação estrita, report (incluir seção cave-stable-run compliance).
```

## Paralelização

- Parallelizable: CONDITIONAL
- Must not run with: fable_02/fable_05 (EnemyBrain lock)
- Reason: mesmo arquivo central de IA.

## Impacto em save/load

```text
Does this change save schema? NO
```

## Impacto em eventos

```text
Adds events: YES (EnemyPackAlertedEvent) | Changes existing: NO | Requires unsubscribe: YES (coordinator)
```

## Impacto em UI/Unity

```text
Changes UI/scenes/prefabs/assets: NO
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: pack alert em cadeia puxar o andar inteiro. Mitigação: alert não propaga entre packs;
raio de alert = leash do pack.
Risco: coordinator sobreviver à rematerialização. Mitigação: ciclo de vida atrelado ao
GeneratedRuntimeRoot (destruído/recriado pelo materializer).
```

## Rollback

```text
Remover coordinator/threat state; EnemyBrain volta a distância instantânea (flag de feature
interna para desligar memória se necessário durante testes).
```

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar materializer→brain e PackId disponível.
- [ ] T002 — EnemyThreatState + testes.
- [ ] T003 — Integrar memória no Chase/EvaluateState.
- [ ] T004 — EnemyPackCoordinator + registro no spawn + testes.
- [ ] T005 — Alert por detecção e por EnemyKilledEvent.
- [ ] T006 — Leash coletivo determinístico + reset/cura + testes.
- [ ] T007 — csproj; run_strict_validation; report com compliance ADR-0005.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (legibilidade do reset/alerta)
- Requires regression test: YES (comportamentos do slice 2026-06-12 intactos)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes EditMode + cenário humano com pack engajando junto

## Definition of Done

```text
Memória de threat, alerta e leash de pack funcionais e determinísticos; stable-run preservado;
builds 0E; report com seção de compliance.
```

## Anti-regressão

```text
Leaper/Blink/Burrow/Retreat continuam funcionando.
Nenhum reroll de composição/posição (ADR-0005).
Sem FindObjectsOfType.
```

## Notas para execução posterior

```text
F05 usa o coordinator para adds de boss.
Target priority multi-alvo será estendida quando companions forem promovidos (WAVE 14 futuras).
```
