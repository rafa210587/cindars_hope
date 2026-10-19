---
status: implemented
implemented_date: 2026-06-23
phase_status: BUILD_VALIDATED
evidence: docs/validation/fable_83_spec_enemy_signature_attacks_light_pathing_runtime_execution_report.md
human_validation: DEFERRED_TO_FINAL_HUMAN_VALIDATION
human_validation_checklist: docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
---

# SPEC — Inimigos: Ataques-Assinatura por Arquétipo + Pathing Leve

> **Spec ID:** `fable_83_spec_enemy_signature_attacks_light_pathing_runtime`
> **Status:** A implementar
> **Wave:** FABLE Batch 6
> **Priority:** P2
> **Type:** Runtime
> **Domain:** Cave / Enemy AI / Combat
> **Parallelizable:** NO (EnemyBrain/Action lock)
> **Parallel group:** N/A
> **Can run with:** N/A
> **Must not run with:** F04, F05, F24, F74, F82 (mesma cadeia EnemyBrain/Action)
> **Repo lock scope:** `Enemy/EnemyBrain.cs`, `Combat/Data/EnemyActionSO.cs`, EnemyActionType enum, helpers de pathing
> **Depends on:** F24 (action sets/telegraphs), F82 (evasão/reposicionamento), F80 (roster que referencia ataques)
> **Blocks:** —
> **Scope:** expandir o action database com ataques-assinatura por arquétipo (combos, AoE telegrafado, summon-add, charge multi-hit, debuff) e adicionar pathing leve de desvio de obstáculo (sem navmesh).
> **Out of scope:** navmesh/A* completo; novos moves de movimento (F24/F82); novas criaturas (F80); boss fights completos (F05).

required_adrs: [ADR-0005]
required_game_rules: [cave_rules.md, combat_rules.md]

---

# /speckit.specify

## Contexto

O action database (F24) tem 8 tipos (MeleeAttack, RangedProjectile, CastProjectile,
AreaPulse, SelfBuff, BurrowStrike, LeapStrike, BlinkStrike) selecionados por range+cooldown.
Bom como base, mas a maioria dos inimigos acaba usando 1 ataque melee genérico — falta
**ataque-assinatura** que dê identidade (combo de 3 golpes de duelista, AoE telegrafado de
conjurador, invocação de fodder, charge multi-hit de bruto, debuff de cultista). Além disso,
o movimento é em **linha reta/órbita sem desvio** — inimigos batem em parede/obstáculo, o que
empobrece perseguição e emboscada. O design pede "ataques únicos e especiais, diferentes uns
dos outros". Esta é a camada final: variedade de ataque + navegação minimamente inteligente.

## Problema

Sem ataques-assinatura, o roster expandido (F80) e os packs (F81) ficam visualmente variados
mas mecanicamente homogêneos no combate. Sem pathing de desvio, perseguições falham em
geometria simples (colunas, paredes), tornando emboscadas e flanqueios (F81/F82) pouco
confiáveis. Implementar A*/navmesh completo seria escopo grande e arriscado para stable-run;
o necessário é **desvio local barato**.

## Objetivo

Ao final desta spec:
1. O `EnemyActionType`/action database ganha ataques-assinatura por arquétipo (aditivos),
   cada um com telegraph legível e parâmetros em dados: **ComboStrike** (sequência multi-hit),
   **TelegraphedAoE** (zona marcada antes do dano), **SummonAdds** (invoca fodder determinístico
   por seed), **MultiHitCharge** (charge que acerta ao longo da linha), **DebuffStrike**
   (aplica status existente). Atribuídos às criaturas via EnemyActionSetSO (F80/F24).
2. O movimento ganha **pathing leve**: desvio local de obstáculo (steering com whisker/raycast
   curto + contorno) sem navmesh, preservando determinismo.
EditMode tests cobrem a seleção/execução pura dos ataques e a decisão de desvio; stable-run
intacto (summons e qualquer aleatoriedade são seeded).

## Fontes obrigatórias lidas

```text
Assets/_Game/Scripts/Combat/Data/EnemyActionSO.cs (EnemyActionType, timing, telegraph)
Assets/_Game/Scripts/Combat/Data/EnemyActionSetSO.cs (lista de ações por inimigo)
Assets/_Game/Scripts/Enemy/EnemyBrain.cs (SelectBestAction, BeginAction, ResolveAction)
Assets/_Game/Scripts/Combat/StatusEffect/** (status reusáveis p/ DebuffStrike)
.specs/a_implementar/fable/fable_24_spec_enemy_moves_elite_affixes_runtime.md
.specs/a_implementar/fable/fable_82_spec_enemy_reactive_evasion_runtime.md
.claude/rules/cave-stable-run.md ; .claude/rules/no-magic-balance-values.md
.claude/skills/ability-effect-composition/SKILL.md ; .claude/skills/enemy-ai-authoring/SKILL.md
.claude/skills/rng-and-determinism/SKILL.md ; .claude/skills/game-feel-checklist/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- EnemyActionSO/EnemyActionType (8 tipos) + EnemyActionSetSO + EnemyActionRuntime (cooldown);
- SelectBestAction (range+cooldown), BeginAction/ResolveAction, telegraph (F04);
- StatusEffect database (status reusáveis); EnemyProjectileBehaviour;
- steering direto/órbita (F24/F82) — base p/ desvio local.
Não existe:
- ataques-assinatura (combo/AoE telegrafado/summon/multi-hit charge/debuff);
- desvio local de obstáculo (pathing leve).
Auditar Fase 0:
- como ResolveAction aplica dano/projétil/status (ponto de extensão dos novos tipos);
- como summon poderia entrar no spawn determinístico sem quebrar stable-run;
- quais raycasts/whiskers de colisão já existem p/ reuso no desvio.
```

## Engineering stories

```text
Como jogador, quero que um duelista encadeie um combo, um conjurador marque uma zona antes do
  dano e um bruto invista atravessando, para que cada arquétipo lute diferente e legível.
Como jogador, quero que inimigos contornem colunas/paredes ao me perseguir, para que
  emboscadas e flanqueios funcionem.
Como designer, quero ataques-assinatura como dados (EnemyActionSO) atribuíveis por ficha.
Como stable-run, quero summons/aleatoriedade seeded (sem GUID/timestamp).
```

## Escopo

```text
Inclui:
- EnemyActionType += {ComboStrike, TelegraphedAoE, SummonAdds, MultiHitCharge, DebuffStrike}
  (aditivo no fim do enum);
- ResolveAction/BeginAction: execução de cada novo tipo, reusando telegraph (F04), projéteis,
  status existentes; parâmetros (nº de hits, raio, atraso de AoE, qtd de adds, status id) em
  EnemyActionSO (no-magic-balance-values);
- SummonAdds: invoca fodder do roster por spawn determinístico (seed por contexto), respeitando
  cap por sala (F81) e stable-run;
- pathing leve: desvio local de obstáculo (whisker/raycast curto + contorno) no movimento de
  Chase/Charge/aproximação — sem navmesh/A*; determinístico;
- atribuição: novos ataques entram via EnemyActionSetSO em criaturas do roster (F80) onde o
  arquétipo pede (duelista=Combo, conjurador=AoE, invocador=Summon, bruto=MultiHitCharge,
  cultista=Debuff);
- EditMode tests: seleção/execução pura por tipo (combo encadeia N, AoE telegrafa antes do
  dano, summon determinístico por seed, multi-hit acerta ao longo da linha, debuff aplica
  status), decisão de desvio pura (obstáculo à frente → contorno).
```

## Fora de escopo

```text
Não inclui:
- navmesh/A* completo (só desvio local barato);
- novos moves de movimento (F24/F82);
- novas criaturas (F80) ou novos status (reusar StatusEffect database);
- boss fights completos / fases (F05);
- VFX além do telegraph existente (F04);
- pooling de projéteis/adds (object-pooling é spec própria — usar spawn existente).
```

## Regras de não duplicação

```text
Estender EnemyActionSO/EnemyBrain únicos — NÃO criar segundo action system/brain.
Reusar telegraph (F04), projéteis, StatusEffect database — não duplicar.
SummonAdds reusa o spawn determinístico existente — sem GUID/timestamp.
Desvio local reusa raycasts/steering existentes — NÃO introduzir navmesh.
Parâmetros em dados (no-magic-balance-values), não literais no código.
```

## Critérios de aceite

### CA-1 Ataques-assinatura executáveis
- Os 5 novos EnemyActionType existem (enum aditivo) e executam: ComboStrike encadeia N hits;
  TelegraphedAoE marca zona antes do dano; SummonAdds invoca fodder; MultiHitCharge acerta ao
  longo da linha; DebuffStrike aplica status existente.
- Evidência: EditMode tests por tipo + build 0E.

### CA-2 Telegraph e legibilidade
- Todo ataque-assinatura ofensivo exibe telegraph antes do dano (reuso F04); AoE sempre marca
  a zona antes de resolver.
- Evidência: teste de sequência telegraph→dano por tipo.

### CA-3 Summon determinístico e capeado
- SummonAdds é seeded (mesma run → mesmos adds) e respeita cap por sala (F81); sem GUID/timestamp.
- Evidência: EditMode test de determinismo + cap.

### CA-4 Pathing leve funcional
- Inimigo em Chase/Charge contorna obstáculo simples à frente (whisker/raycast) em vez de
  encravar; determinístico; sem navmesh.
- Evidência: EditMode test da decisão de desvio + cenário humano (perseguição em torno de coluna).

### CA-5 Stable-run e regressão
- Replay validator PASS; ataques/moves anteriores (F24/F82) intactos; sem GameObject.Find;
  comunicação via GameEventBus.
- Evidência: replay validator + testes de regressão.

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Combat/Data/
  EnemyActionSO.cs                    (EnemyActionType += 5; params dos novos tipos)
Assets/_Game/Scripts/Enemy/
  EnemyBrain.cs                       (ResolveAction/BeginAction p/ novos tipos)
  EnemyActionExecution.cs             (NOVO — execução pura testável por tipo)
  EnemyLocalAvoidance.cs              (NOVO — desvio local determinístico, helpers puros)
Assets/_Game/Data/Enemies/ActionSets/ (atribuição via EnemyActionSetSO — evidência)
Assets/_Game/Tests/EditMode/Cave/
  EnemySignatureActionsTests.cs       (NOVO)
  EnemyLocalAvoidanceTests.cs         (NOVO)
docs/validation/
  fable_83_spec_enemy_signature_attacks_light_pathing_runtime_execution_report.md
```

## Contratos

### Data contracts
`EnemyActionType` += 5 valores no fim (aditivo, save-safe). `EnemyActionSO` += params dos
novos tipos (comboHits, aoeDelay, aoeRadius, summonCount, summonEnemyId, debuffStatusId) —
todos em dados (no-magic-balance-values). EnemyActionSetSO atribui aos inimigos do roster (F80).

### Runtime contracts
`EnemyActionExecution.Resolve<type>(...)` puros onde possível (cálculo de hits/zona/alvos);
EnemyBrain orquestra timing/telegraph. `EnemyLocalAvoidance.Steer(origin, desiredDir, hits)` →
dir corrigida (puro, determinístico). SummonAdds usa spawn determinístico (seed por contexto)
+ cap por sala (F81). RNG seeded (rng-and-determinism).

### Event contracts
Reusa eventos de combate/telegraph existentes. Adds via spawn existente. Nenhum evento alterado;
adicionar só se um "EnemyAddsSummonedEvent" aditivo for necessário p/ HUD/telemetria.

### Save contracts
N/A — combate não persiste; summons recomputáveis por seed (stable-run).

### UI contracts
Telegraph reusa F04. Nenhuma tela nova.

## Sistemas afetados

```text
Enemy actions (núcleo) | EnemyBrain (execução) | Spawn (summon determinístico, leitura F81)
Status effects (DebuffStrike consome) | Movement (desvio local) | Validation reports
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Combat/Data/EnemyActionSO.cs (enum/params aditivos)
Assets/_Game/Scripts/Enemy/EnemyBrain.cs
Assets/_Game/Scripts/Enemy/EnemyActionExecution.cs (novo)
Assets/_Game/Scripts/Enemy/EnemyLocalAvoidance.cs (novo)
Assets/_Game/Data/Enemies/ActionSets/** (atribuição via EnemyActionSetSO)
Assets/_Game/Scripts/Core/Events/EnemyEvents (EnemyAddsSummonedEvent aditivo, se necessário)
Assets/_Game/Tests/EditMode/Cave/**
docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab (edição manual)
Packages/** ; ProjectSettings/**
Navmesh/A* / pathfinding pesado ; CanonicalBestiaryCatalog (F80) ; SaveManager
StatusEffect database (consumir, não alterar) ; Telegraph system (F04 — consumir)
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
Pontos de extensão de ResolveAction; spawn determinístico p/ summon; raycasts reusáveis p/
desvio. Ler stable-run + rng-and-determinism.

### Fase 1 — Ataques-assinatura
EnemyActionType += 5 + params + EnemyActionExecution puro; telegraph reuso. EditMode tests.

### Fase 2 — Summon determinístico
SummonAdds seeded + cap por sala (F81). Teste de determinismo/cap.

### Fase 3 — Pathing leve
EnemyLocalAvoidance (whisker/raycast + contorno) no Chase/Charge. Teste de desvio.

### Fase 4 — Atribuição e validação
Atribuir ataques por arquétipo (EnemyActionSetSO do roster F80); replay; regressão F24/F82;
csproj; strict; report.
```

## Paralelização

- Parallelizable: NO — Must not run with F04/F05/F24/F74/F82 (cadeia EnemyBrain/Action).
- Lock: EnemyBrain, EnemyActionSO, EnemyActionType.
- Reason: edição concorrente do brain/action causaria conflito direto.

## Impacto em save/load

```text
Changes save schema? NO | Adds section? NO | Migration? NO | Persists Unity refs? NO
Enum/params aditivos (save-safe). Summons recomputáveis por seed.
```

## Impacto em eventos

```text
Adds events: CONDITIONAL — EnemyAddsSummonedEvent (aditivo) só se HUD/telemetria precisar
Changes existing events: NO | Requires unsubscribe pattern: YES (se evento novo)
```

## Impacto em UI/Unity

```text
Changes UI: NO (telegraph reusa F04) | Changes scenes: NO | Changes prefabs: NO
Changes assets: EnemyActionSO/ActionSets (atribuição) — evidência
Requires Play Mode final validation: YES (sentir combo/AoE/summon/charge + perseguição c/ desvio)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: AoE/combo injustos (sem leitura). Mitigação: telegraph obrigatório + params em dados.
Risco: summon descontrolado (enxame infinito). Mitigação: cap por sala (F81) + cooldown + seed.
Risco: desvio local oscila/trava (jitter). Mitigação: whisker curto + histerese; teste puro.
Risco: quebra de stable-run por summon/aleatoriedade. Mitigação: RNG seeded; replay validator.
Risco: brain Update inchado. Mitigação: execução pura extraída (EnemyActionExecution) + branches curtos.
```

## Rollback

```text
Novos action types só ativam se atribuídos em EnemyActionSetSO — assets antigos intactos.
Desabilitar desvio local volta ao steering reto (F24/F82). Enum aditivo no fim (save-safe).
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar ResolveAction, spawn determinístico p/ summon, raycasts reusáveis.
- [ ] T002 — EnemyActionType += 5 + params + EnemyActionExecution puro + telegraph + tests.
- [ ] T003 — SummonAdds seeded + cap por sala (F81) + testes determinismo/cap.
- [ ] T004 — EnemyLocalAvoidance (desvio local) no Chase/Charge + testes de desvio.
- [ ] T005 — Atribuir ataques por arquétipo (ActionSets F80); replay; regressão F24/F82; strict; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (execução de ataque, summon seeded, desvio local)
- Requires EditMode tests: YES (combo/AoE/summon/multi-hit/debuff puros + desvio + determinismo)
- Requires PlayMode/human scenario: YES (sentir ataques + perseguição com desvio — lote final)
- Requires regression test: YES (ações/moves F24/F82 intactos + replay stable-run)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano (combo, AoE telegrafado, summon, charge, desvio em torno de coluna)

## Definition of Done

```text
5 ataques-assinatura aditivos (combo/AoE/summon/multi-hit/debuff) com telegraph, params em
dados, atribuídos por arquétipo; pathing leve de desvio sem navmesh; summon determinístico
capeado; stable-run intacto (replay PASS); F24/F82 intactos. Builds 0E; strict exit 0; report.
```

## Anti-regressão

```text
Os 8 action types e moves F24/F82 não mudam de comportamento.
SummonAdds nunca usa GUID/timestamp; mesma run → mesmos adds (stable-run).
Telegraph obrigatório antes de qualquer dano de área/combo/charge.
Sem GameObject.Find; comunicação só via GameEventBus; sem navmesh.
```
