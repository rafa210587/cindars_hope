# SPEC — Cave Boss: IA de Fases (Thresholds, Action Sets por Fase, Janelas de Vulnerabilidade)

> **Spec ID:** `fable_05_spec_cave_boss_phase_ai_runtime`
> **Status:** A implementar
> **Wave:** FABLE — Gap Closure Bloco B (inimigos)
> **Priority:** P2
> **Type:** Runtime / Data
> **Domain:** Cave / Combat
> **Parallelizable:** NO
> **Parallel group:** fable_bloco_B
> **Can run with:** N/A
> **Must not run with:** fable_02, fable_04 (EnemyBrain/posture locks)
> **Repo lock scope:** `EnemyBrain.cs`, `CaveBossSpawner.cs`, `CaveBossGateDataSO`
> **Depends on:**
> - `fable_02` (posture/stagger), `fable_04` (coordinator para adds)
> **Blocks:** promoção futura das specs WAVE 19 (nível 100/101)
> **Scope:** fases de boss data-driven sobre o EnemyBrain existente, com adds e janelas por fase.
> **Out of scope:** boss final/Arquivista do Silêncio (nível 100/101 — specs WAVE 19 futuras), cutscenes, arte.

required_adrs: [ADR-0005]
required_game_rules: [cave_rules.md]

---

# /speckit.specify

## Contexto

A infraestrutura de boss existe e está validada: `CaveBossGateDataSO`/`CaveBossGateRegistrySO`
(gates por nível), `CaveBossSpawner`, `CaveBossDefeatMonitor`/`CaveBossDeathReporter`
(estado de derrota persistido por run). Mas o boss spawnnado é um inimigo comum maior:
usa o mesmo `EnemyBrain` sem fases. `ENEMY_BEHAVIORS_DIRECTION.md` e
`CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md` pedem bosses com fases por threshold de
HP, action sets que mudam por fase, janelas de vulnerabilidade pós-padrão e invocação de adds.

## Problema

Bosses e minibosses (gates a cada N níveis) são paredes de HP sem leitura tática — o
investimento em telegraphs/vulnerability windows (SPEC 13/14A) não muda entre 100% e 10%
de HP. Sem contrato de fase, as specs futuras do nível 100/101 (WAVE 19) não têm base.

## Objetivo

Ao final desta spec, deve existir `BossPhaseProfileSO` (fases com threshold, ActionSetId,
multiplicadores e padrão de adds) consumido por um `BossBrainController` que envolve o
EnemyBrain, trocando action set/movimento por fase e abrindo vulnerability window na
transição — gerado por editor script para os bosses existentes do `CreateCaveBossAssets`.

## Fontes obrigatórias lidas

```text
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
docs/design/gameplay/enemies/ENEMY_BEHAVIORS_DIRECTION.md
docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
docs/game_rules/cave_rules.md
docs/decisions/ADR-0005-cave-stable-run-and-replay.md
.claude/rules/testing-quality-gate.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- CaveBossGateDataSO/RegistrySO, CaveBossSpawner, CaveBossDefeatMonitor/DeathReporter/DefeatState
- EnemyBrain (ConfigureRuntime com action set databases), EnemyActionSetSO/EnemyActionSO
- EnemyVulnerabilityState.OpenWindow, EnemyTelegraphController
- EnemyPostureState (F02), EnemyPackCoordinator (F04)
- Editor/CaveData/CreateCaveBossAssets.cs (gera bosses)
Não existe:
- conceito de fase; troca de action set em runtime; adds por fase
```

## Engineering stories

```text
Como boss, quero trocar de action set ao cruzar 66%/33% de HP com transição telegrafada.
Como jogador, quero janela de vulnerabilidade clara após a transição de fase.
Como fase final, quero invocar 2 adds do pack do boss uma única vez (idempotente por fase).
Como specs WAVE 19, quero um contrato de fase reutilizável para o boss final.
```

## Escopo

```text
Inclui:
- BossPhaseProfileSO (NOVO): lista de BossPhase {HpThresholdPercent, ActionSetId,
  MoveSpeedMultiplier, DamageMultiplier, AddsEnemyId, AddsCount, VulnerabilityWindowSeconds};
- BossBrainController (NOVO MonoBehaviour): observa EnemyHealth, resolve fase corrente,
  chama EnemyBrain.SwapActionSet(actionSetId) (método NOVO no brain) e aplica multiplicadores;
  transição: telegraph 1s + OpenWindow + spawn de adds via materializer/coordinator (1x por fase);
- adds determinísticos: EnemyId fixo do profile, posições derivadas de
  StableHash(runSeed|level|bossId|phase|i) em tiles walkable próximos (ADR-0005);
- gerador editor: anexar BossPhaseProfile padrão (3 fases) aos bosses de CreateCaveBossAssets;
- integração no CaveBossSpawner (AddComponent BossBrainController quando profile presente);
- EditMode tests: resolução de fase por HP, idempotência de adds, determinismo de posições.
```

## Fora de escopo

```text
Não inclui: boss final 100/101 (WAVE 19 futuras); barra de HP de boss em Canvas (F14);
novos bosses; balance fino; drops de boss (first/repeat — F06 cobre tabela).
```

## Regras de não duplicação

```text
Não criar segunda state machine de boss — wrapper sobre EnemyBrain.
Não duplicar spawn de inimigos — adds via caminho do materializer/coordinator.
Não recriar defeat tracking — CaveBossDefeatMonitor permanece dono.
```

## Critérios de aceite

### CA-1 Fases data-driven
- Boss com profile de 3 fases troca ActionSet e multiplicadores em 66%/33%.
- Evidência: testes do resolver de fase (puro) + log `CombatLog: BossPhaseChanged`.

### CA-2 Transição legível
- Cada transição: telegraph >= 1s sem dano do boss + vulnerability window do profile.
- Evidência: teste de sequência de transição + cenário humano.

### CA-3 Adds idempotentes e estáveis
- Fase com adds invoca exatamente AddsCount uma única vez por fase, em posições determinísticas
  pela seed da run; re-entrar no nível NÃO re-invoca adds de fases já passadas
  (estado de fase corrente derivado do HP atual do boss — documentar limite: HP de boss não
  persiste entre visitas, coberto por F13).
- Evidência: testes de idempotência/determinismo.

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Cave/Data/BossPhaseProfileSO.cs       (NOVO)
Assets/_Game/Scripts/Cave/Runtime/BossBrainController.cs   (NOVO)
Assets/_Game/Scripts/Enemy/EnemyBrain.cs                   (+ SwapActionSet)
Assets/_Game/Scripts/Cave/Runtime/CaveBossSpawner.cs       (integração)
Assets/_Game/Scripts/Editor/CaveData/AttachDefaultBossPhaseProfiles.cs (NOVO)
Assets/_Game/Tests/EditMode/World/BossPhaseTests.cs
```

## Contratos

### Data contracts
`BossPhaseProfileSO` com `IIdentifiedData` (`boss_phase_<bossId>`); fases ordenadas por threshold desc.
### Runtime contracts
`EnemyBrain.SwapActionSet(string actionSetId)`; `BossBrainController.CurrentPhaseIndex`.
### Event contracts
`BossPhaseChangedEvent(bossId, phaseIndex)` — NOVO.
### Save contracts
N/A nesta spec (HP de boss entre visitas = débito tratado em F13).

## Sistemas afetados

```text
Cave boss runtime, Enemy AI, Editor tooling, Event bus
```

## Arquivos permitidos

```text
Arquivos da arquitetura + Assets/_Game/Scripts/Core/Events/EnemyEvents.cs
Assets/_Game/Tests/EditMode/World/** ; docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity/*.prefab; *.asset manual (gerador apenas); Packages/ProjectSettings;
SaveManager/GameSaveData; CaveEnemySpawnPlanner.cs
```

## Estratégia de implementação

```md
### Fase 0 — Auditar CaveBossSpawner/CreateCaveBossAssets (formato real dos bosses).
### Fase 1 — SO + resolver puro de fases + testes.
### Fase 2 — SwapActionSet no brain + controller + multiplicadores.
### Fase 3 — Transição (telegraph/window) + adds determinísticos idempotentes.
### Fase 4 — Gerador editor + integração spawner + validação estrita + report.
```

## Paralelização

- Parallelizable: NO
- Reason: EnemyBrain lock compartilhado com F02/F04 (devem estar concluídas).

## Impacto em save/load

```text
Does this change save schema? NO
```

## Impacto em eventos

```text
Adds events: YES (BossPhaseChangedEvent) | Changes existing: NO | Requires unsubscribe: YES
```

## Impacto em UI/Unity

```text
Changes assets: YES (gerador) | Scenes/prefabs: NO
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: SwapActionSet no meio de AttackWindup corromper timers. Mitigação: swap agendado
para fim do recover; teste de transição durante windup.
Risco: adds excederem budget de inimigos. Mitigação: cap global = DepthScalingHardCap.
```

## Rollback

```text
Remover controller/SO/gerador; bosses voltam a brain simples. SwapActionSet sem chamadores é inerte.
```

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar bosses gerados e spawner.
- [ ] T002 — BossPhaseProfileSO + resolver puro + testes.
- [ ] T003 — EnemyBrain.SwapActionSet (seguro em qualquer estado).
- [ ] T004 — BossBrainController (fases, multiplicadores, evento).
- [ ] T005 — Transições (telegraph + window) + adds determinísticos.
- [ ] T006 — Gerador AttachDefaultBossPhaseProfiles + integração spawner.
- [ ] T007 — Testes EditMode; csproj; run_strict_validation; report (compliance ADR-0005).
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
- Requires PlayMode automated or final human scenario: YES (luta de boss legível)
- Requires regression test: YES (boss sem profile = comportamento atual)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano de uma luta com 3 fases

## Definition of Done

```text
Fases data-driven com transição legível e adds idempotentes/determinísticos; bosses sem
profile inalterados; builds 0E; report com compliance.
```

## Anti-regressão

```text
CaveBossDefeatMonitor continua a fonte de derrota.
Gates/checkpoints intactos. Sem reroll de conteúdo estável (ADR-0005).
```

## Notas para execução posterior

```text
WAVE 19 (nível 100/101) deve reutilizar BossPhaseProfileSO/BossBrainController.
HP de boss persistente entre visitas: tratado em F13 (CAVE_ENEMY_HP_SAVE_DEBT).
```


---

## EMENDA 2026-06-12 (pós-canonização dos catálogos — VINCULANTE)

```text
1. As fases CONCRETAS de cada boss vêm do CAVE_BESTIARY_CATALOG (fichas ★): Mite Queen,
   Fungal Patriarch, Rimelock Colossus, Cindershard Wyrm, Artificer Lord, Draconic
   Guardian, Abyssal Gatekeeper, Void Herald, Draconic Elder — o gerador de profiles
   autora ESSES, não 3 fases genéricas.
2. Cindershard Wyrm e Draconic Elder têm fase de VOO (FloatingOrbit) — o controller deve
   suportar troca de Move por fase além de ActionSet.
3. Gates duplos (Gravelborn Twins) ficam para F24/F33 (minibosses), fora desta spec.
```
