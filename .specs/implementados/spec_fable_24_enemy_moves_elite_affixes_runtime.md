# SPEC — Inimigos: 12 Moves Canônicos Faltantes + Elites com Afixos

> **Spec ID:** `fable_24_spec_enemy_moves_elite_affixes_runtime`
> **Status:** A implementar
> **Wave:** FABLE Batch 3
> **Priority:** P1
> **Type:** Runtime
> **Domain:** Cave / Enemy AI
> **Parallelizable:** NO (EnemyBrain lock)
> **Parallel group:** N/A
> **Can run with:** N/A
> **Must not run with:** F04, F05 (mesma cadeia EnemyBrain — ordem F04 → F24 → F05)
> **Repo lock scope:** `Enemy/EnemyBrain.cs`, EnemyMove enum, CaveEnemySpawnPlanner
> **Depends on:**
> - F02 (postura/stagger)
> - F04 (action sets/telegraphs)
> **Blocks:**
> - F05 (bosses usam BossArenaControl/BossPhaseShift)
> - F33 (fichas referenciam moves)
> **Scope:** completar os 22 Moves oficiais (COMBAT_CORE §27) + elites nomeados com afixos.
> **Out of scope:** moves de boss COMPLETOS (F05 orquestra fases; aqui só os 2 primitivos), pack AI avançada com pathfinding novo.

required_adrs: [ADR-0005]
required_game_rules: [cave_rules.md, combat_rules.md]

---

# /speckit.specify

## Contexto

A tabela canônica de Moves oficiais (COMBAT_CORE §27) define 22 Moves com faixas de
velocidade e uso (TankSlowPush, GroundPatrol, GuardStationary, GroundChase, SwarmErratic,
KiteRanged, CasterKeepAway, Leaper, BurrowAmbush, PhaseShortBlink + os 12 faltantes).
O EnemyBrain hoje cobre 10. Faltam 12: PackFlanker, PackLeader, RetreatAndCall,
FloatingSlow, FloatingOrbit, CircleStrafe, ChargeLine, TreasureIdleAmbush, HazardLure,
ProtectAnchor, BossArenaControl, BossPhaseShift.

O bestiário canônico (`CAVE_BESTIARY_CATALOG_DIRECTION_v1.0.md`) atribui esses moves a
criaturas concretas (Veilkin Scout=CircleStrafe, Mirelurk=TreasureIdleAmbush,
Wisp=FloatingOrbit, Grimfang Packleader=PackLeader...) — sem eles, parte das 60 fichas
(F33) não é implementável. Decisão humana Q7.2: sim, implementar os 12 com as faixas do
COMBAT_CORE, com balance/ativação cuidadosos (windup/recovery por papel: 0.5/0.5 comum,
0.7/0.6 elite, 0.9-1.2/0.8 boss). Também por decisão (DECISOES §B): elites nomeados
(+25% stats + 1 afixo) por banda, com spawn determinístico que respeita o contrato
stable-run da caverna (FASE9F — nenhum reroll dentro do mesmo CaveRunSeed).

## Problema

Sem os 12 moves, 22 Moves canônicos viram 10 e o roster das 60 criaturas degrada para
meia dúzia de comportamentos repetidos — o combate da caverna perde leitura tática
(packs, emboscadas, ancoragem). Sem os 2 primitivos de boss, a F05 não tem base para
orquestrar fases. Sem elites determinísticos, o drop bônus de essência (F06 lê a flag)
e os contratos de caça não têm alvo. Implementar com aleatoriedade não-determinística
(GUID/timestamp) quebraria o stable-run da caverna — regra invariante.

## Objetivo

Ao final desta spec, o projeto deve ter os 22 Moves oficiais no enum com implementação
(ou primitivo documentado para os 2 de boss), elites determinísticos por seed (+25%
stats, 1 afixo, nome com prefixo, 8% das vagas a partir do nível 6) integrados ao
CaveEnemySpawnPlanner — permitindo que F05 (bosses) e F33 (fichas) avancem, sem
recriar o brain, sem pathfinding novo e com replay validator do cave passando.

## Fontes obrigatórias lidas

```text
docs/design/gameplay/combat/COMBAT_CORE_MECHANICS_DIRECTION.md (§27 — tabela dos 22)
docs/design/gameplay/cave/CAVE_BESTIARY_CATALOG_DIRECTION_v1.0.md (move por criatura, afixos §elites)
docs/design/gameplay/cave/CAVE_LEVELS_QUOTAS (escala por banda)
docs/design/FABLE_DECISOES_RESPOSTAS_v1.0.md (§7-8 moves, windup/recovery por papel)
.claude/rules/cave-stable-run.md
.claude/rules/testing-quality-gate.md
.claude/skills/cave-stable-run-guard/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- EnemyBrain com states (Idle/Patrol/Chase/Attack/Leaper/Blink/Burrow/Retreat/GuardHold);
- EnemyProjectileBehaviour;
- CaveEnemySpawnPlanner determinístico (StableHash — padrão stable-run);
- EnemyDataSO com Move enum parcial;
- floating label de nome (UI existente para o prefixo de elite).
Não existe:
- os 12 moves acima; conceito de elite/afixo; coordenação de pack (flanker espera
  leader); ancoragem (ProtectAnchor); idle disfarçado (mimic).
Auditar Fase 0:
- shape exato do enum atual e do switch de estados do EnemyBrain;
- como o CaveEnemySpawnPlanner enumera vagas (onde entra a vaga de elite);
- quais helpers de steering existem (reuso para CircleStrafe/Orbit).
```

## Engineering stories

```text
Como jogador, quero packs com leitura tática (flanker espera o líder; líder morto =
  bando recua e chama ajuda), para que o combate de grupo tenha counterplay.
Como F05 (bosses), quero os primitivos BossArenaControl/BossPhaseShift prontos
  (leash de arena + troca de ActionSet por gatilho) para orquestrar fases sem
  reimplementar movimento.
Como stable-run, quero elites decididos por StableHash do seed (nunca GUID/timestamp),
  para que revisitar o nível na mesma run mostre os MESMOS elites.
Como F06 (loot), quero a flag de elite no spawn para aplicar drop de essência +25%.
```

## Escopo

```text
Inclui:
- EnemyMove enum += 12 valores (fim do enum — aditivo, save-safe);
- implementações no EnemyBrain (1 estado/branch por move, reusando primitivas):
  CircleStrafe (orbita a distância de tiro), ChargeLine (telegraph linha + investida reta),
  FloatingSlow/FloatingOrbit (ignora obstáculo de chão — flag), RetreatAndCall (foge e
  emite EnemyCallForHelpEvent → aliados em raio agrupam), PackFlanker/PackLeader
  (flanker só engaja se leader vivo em raio; leader morto → flankers RetreatAndCall),
  ProtectAnchor (não se afasta >N tiles do anchor; anchor = spawn point/objeto),
  TreasureIdleAmbush (imóvel disfarçado; ativa por proximidade <2 tiles), HazardLure
  (recuar tentando atravessar hazard tile), BossArenaControl/BossPhaseShift (primitivos:
  travar leash na arena + trocar ActionSet/Move por gatilho — F05 orquestra);
- elites: EliteAffix enum {Frenzied(+30% ASPD), Armored(+50% DEF/postura), Vampiric(cura
  25% do dano), Volatile(explode ao morrer — telegraph 0.8s), Warded(resiste 1º status)};
  spawn determinístico: StableHash(...|"elite") — 8% das vagas a partir do nível 6, nome
  com prefixo, +25% stats, drop de essência +25% (F06 lê flag);
- EnemyDataSO: campo MoveSecondary opcional (voadores alternam Slow→Orbit em combate);
- EditMode tests: transições puras por move (helpers estáticos extraídos), elite hash
  determinístico, pack leader-morto → retreat, ambush ativa por distância.
```

## Fora de escopo

```text
Não inclui:
- moves de boss COMPLETOS (F05 orquestra fases; aqui só os 2 primitivos);
- pack AI avançada com pathfinding novo (steering atual apenas);
- autoração das 60 fichas (F33);
- balance fino por criatura (faixas do COMBAT_CORE são canônicas);
- persistência de estado de combate (não persiste; elite deriva de seed);
- VFX/arte de telegraphs além do sistema existente (F04).
```

## Regras de não duplicação

```text
Não criar segundo brain — estender o EnemyBrain existente (1 estado/branch por move).
Não usar GUID/timestamp para decisão de elite/conteúdo (stable-run — StableHash apenas).
Não implementar fases de boss completas (F05 é a dona da orquestração).
Pathfinding novo: NÃO (reusar steering atual).
Não duplicar telegraphs — usar o sistema da F04.
```

## Critérios de aceite

### CA-1 22 Moves completos

- Os 22 Moves oficiais existem no enum com implementação ou primitivo documentado
  (BossArenaControl/BossPhaseShift); os 10 atuais seguem intactos.
- Evidência: enum + branch por move; testes de transição puros; build 0E.

### CA-2 Coordenação de pack

- Flanker não engaja sem leader vivo em raio; leader morto → bando entra em
  RetreatAndCall (teste puro com helpers estáticos).
- Evidência: EditMode tests da máquina de decisão de pack.

### CA-3 Elites determinísticos

- Mesmo run (mesmo seed) = mesmos elites nas mesmas vagas (StableHash, 8% a partir do
  nível 6); +25% stats; afixo ativo e observável; nome com prefixo.
- Evidência: EditMode test de hash determinístico (repetição) + teste de stats/afixo.

### CA-4 Ambush e charge legíveis

- Mimic (TreasureIdleAmbush) permanece imóvel até jogador <2 tiles; ChargeLine sempre
  exibe telegraph de linha antes da investida.
- Evidência: EditMode tests de ativação por distância e de sequência telegraph→investida.

### CA-5 Stable-run preservado

- Replay validator do cave PASS após as mudanças (nenhum reroll de composição/posição
  dentro do mesmo CaveRunSeed).
- Evidência: execução do replay validator com resultado registrado no report.

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Enemy/
  EnemyBrain.cs                  (estados novos + helpers estáticos testáveis)
  EliteAffix.cs                  (NOVO — enum + aplicação de afixo/stats)
Assets/_Game/Scripts/Cave/Runtime/
  CaveEnemySpawnPlanner.cs       (vagas de elite por StableHash)
Assets/_Game/Scripts/Core/Events/
  EnemyEvents (EnemyCallForHelpEvent — aditivo)
Assets/_Game/Tests/EditMode/Cave/
  EnemyMovesTests.cs             (NOVO)
  EliteAffixTests.cs             (NOVO)
docs/validation/
  fable_24_spec_enemy_moves_elite_affixes_runtime_execution_report.md
```

## Contratos

### Data contracts

`EnemyMove` enum += 12 valores no FIM (aditivo). `EliteAffix` enum {Frenzied(+30% ASPD),
Armored(+50% DEF/postura), Vampiric(cura 25% do dano causado), Volatile(explode ao
morrer — telegraph 0.8s, dano cap 25% maxHP), Warded(resiste 1º status)}.
`EnemyDataSO.MoveSecondary` opcional (voadores alternam FloatingSlow→FloatingOrbit em
combate). Faixas de velocidade por move = tabela canônica do COMBAT_CORE.

### Runtime contracts

EnemyBrain: 1 estado/branch por move com helpers estáticos puros extraídos (decisão de
engajamento de pack, ativação de ambush por distância, leash de anchor/arena) para
teste EditMode. CaveEnemySpawnPlanner: vaga elite decidida por
`StableHash(CaveWorldSeed|CaveRunSeed|level|slot|"elite")` — 8% das vagas a partir do
nível 6; flag de elite exposta para F06 (drop de essência +25%) e para o floating label
(prefixo no nome).

### Event contracts

Novo (aditivo em EnemyEvents): `EnemyCallForHelpEvent` (emissor, posição, raio) —
aliados em raio agrupam. Nenhum evento existente muda.

### Save contracts

N/A — estado de combate não persiste; flag de elite deriva do seed (recomputável).
Justificativa: stable-run garante recomposição determinística ao revisitar.

### UI contracts

Nome com prefixo de elite no floating label existente. Nenhuma tela nova.

## Sistemas afetados

```text
Enemy AI (EnemyBrain — núcleo da spec)
Cave spawn planning (vagas de elite determinísticas)
Loot (F06 lê flag de elite — integração de leitura)
Event bus (+1 evento)
UI floating label (prefixo)
Validation reports
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Enemy/EnemyBrain.cs
Assets/_Game/Scripts/Enemy/EliteAffix.cs (novo)
Assets/_Game/Scripts/Cave/Runtime/CaveEnemySpawnPlanner.cs
Assets/_Game/Scripts/Core/Events/EnemyEvents (aditivo)
EnemyDataSO (campo MoveSecondary aditivo)
Assets/_Game/Tests/EditMode/Cave/**
docs/validation/**
csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset (edição manual de YAML — proibida)
Packages/**
ProjectSettings/**
Sistema de telegraphs da F04 (consumir, não alterar)
CaveRuntimeMaterializer/snapshot além da leitura necessária (stable-run intocado)
SaveManager (nenhuma persistência nova)
Pathfinding/steering core (reusar, não reescrever)
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
Auditar enum/switch atuais do EnemyBrain, enumeração de vagas do CaveEnemySpawnPlanner
e helpers de steering reusáveis. Ler regras stable-run (mandatório).

### Fase 1 — Moves de movimento (6)
CircleStrafe, ChargeLine, FloatingSlow, FloatingOrbit, RetreatAndCall (+evento),
HazardLure — com helpers estáticos puros + testes.

### Fase 2 — Coordenação e ancoragem (4)
PackFlanker/PackLeader (decisão de engajamento testável), ProtectAnchor (leash N tiles),
TreasureIdleAmbush (ativação <2 tiles).

### Fase 3 — Primitivos de boss (2)
BossArenaControl (leash de arena) + BossPhaseShift (troca de ActionSet/Move por
gatilho) — interface clara para a F05.

### Fase 4 — Elites
EliteAffix + StableHash nas vagas (8% nível 6+) + stats +25% + prefixo + flag p/ F06.

### Fase 5 — Validação e fechamento
EditMode tests; replay validator do cave; csproj; run_strict_validation; report.
```

## Paralelização

- Parallelizable: NO
- Parallel group: N/A
- Must not run with: F04, F05 (mesma cadeia EnemyBrain — ordem obrigatória F04 → F24 → F05)
- Shared files/systems that require lock: `Enemy/EnemyBrain.cs`, EnemyMove enum,
  CaveEnemySpawnPlanner
- Reason: F04 e F05 editam o mesmo brain/planner; execução simultânea geraria conflito
  direto de merge e de contrato de estados.

## Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? N/A (nada persiste)
Estado de combate não persiste; flag de elite deriva do seed (stable-run).
Enum aditivo no fim (save-safe para EnemyDataSO serializado).
```

## Impacto em eventos

```text
Adds events: YES — EnemyCallForHelpEvent (aditivo em EnemyEvents)
Changes existing events: NO
Requires unsubscribe pattern: YES (aliados assinam/desassinam no ciclo de vida do brain)
```

## Impacto em UI/Unity

```text
Changes UI: YES — prefixo de elite no floating label existente (mínimo)
Changes scenes: NO | Changes prefabs: NO | Changes ScriptableObjects/assets: NO
  (MoveSecondary é campo de código; autoração de assets é F33)
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: explosão de complexidade no Update do EnemyBrain.
Mitigação: switch por move com métodos curtos + helpers estáticos puros extraídos.
Risco: Volatile matar o player injustamente.
Mitigação: telegraph 0.8s + dano cap 25% maxHP.
Risco: quebra do stable-run (reroll de elites/posições).
Mitigação: StableHash com salt fixo; replay validator obrigatório (CA-5).
Risco: pack deadlock (flankers esperando leader inexistente).
Mitigação: fallback — sem leader em raio, flanker age como GroundChase após timeout;
  teste puro da decisão.
Risco: ChargeLine atravessar paredes/hazards.
Mitigação: investida reta com checagem de colisão do steering atual (sem pathfinding novo).
```

## Rollback

```text
Moves novos só ativam se EnemyDataSO usar — assets antigos intactos (rollback natural).
Remover EliteAffix/vagas elite restaura spawn anterior (mesmo seed → mesma composição
base). Reverter branches do brain não afeta os 10 moves atuais.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar enum/switch/spawn planner atuais + steering reusável.
- [ ] T002 — 6 moves de movimento (CircleStrafe/ChargeLine/Floating×2/Retreat&Call/
        HazardLure) + EnemyCallForHelpEvent + testes.
- [ ] T003 — 4 moves de coordenação/ancoragem (Pack×2/ProtectAnchor/TreasureIdleAmbush)
        + testes puros.
- [ ] T004 — 2 primitivos de boss (ArenaControl/PhaseShift) p/ F05.
- [ ] T005 — Elites (afixos+hash+stats+nome) + integração planner/F06.
- [ ] T006 — Testes; replay validator; csproj; run_strict_validation; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (decisões de pack/ambush/leash, hash de elite)
- Requires EditMode tests: YES (transições puras por move, elite hash determinístico,
  pack leader-morto → retreat, ambush por distância)
- Requires PlayMode automated or final human scenario: YES (lote)
- Requires regression test: YES (10 moves atuais intactos + replay validator stable-run)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano com pack, mimic,
  charge e 1 elite

## Definition of Done

```text
22/22 Moves no enum (implementados ou primitivo documentado); elites determinísticos
(+25% stats, afixo, prefixo, 8% das vagas nível 6+); stable-run intacto (replay PASS);
brain único estendido. EditMode tests passando.
Builds 0E; run_strict_validation exit 0; execution report criado.
```

## Anti-regressão

```text
Os 10 moves atuais não mudam de comportamento.
Nenhum GUID/timestamp em conteúdo de runtime da caverna (stable-run).
Mesmo CaveRunSeed → mesma composição/posições/elites ao revisitar nível.
ForwardExit/BackExit nunca mudam CaveRunSeed.
Telegraph obrigatório antes de ChargeLine e da explosão Volatile.
Nenhum GameObject.Find em runtime; eventos só via GameEventBus.
```
