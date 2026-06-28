# SPEC — Cave: Enriquecimento de Packs Procedurais Temáticos

> **Spec ID:** `fable_81_spec_procedural_pack_enrichment`
> **Status:** A implementar
> **Wave:** FABLE Batch 6
> **Priority:** P2
> **Type:** Data / Runtime
> **Domain:** Cave / Spawn
> **Parallelizable:** CONDITIONAL
> **Parallel group:** fable_batch_6
> **Can run with:** specs que não tocam spawn planner/packs
> **Must not run with:** F80 (roster), F24, F33, F78 (mesma cadeia spawn/ecosystem)
> **Repo lock scope:** `Enemy/EnemySpawnPackSO.cs`, `Enemy/EnemySpawnResolver.cs`, `Cave/Runtime/CaveEnemySpawnPlanner.cs`, packs em `Data/EnemySpawn/Packs/`
> **Depends on:** F80 (roster +40), F24 (PackLeader/PackFlanker/RetreatAndCall), F04 (PackCoordinator)
> **Blocks:** —
> **Scope:** montar packs procedurais temáticos por bioma (líder + flankers + apoio) usando o roster expandido, com weighting por raridade/role, preservando o stable-run.
> **Out of scope:** novo comportamento de AI (F24/F82); novas criaturas (F80); pathfinding (F83).

required_adrs: [ADR-0005]
required_game_rules: [cave_rules.md]

---

# /speckit.specify

## Contexto

Os packs hoje são montados pelo `EnemySpawnResolver` (tenta pack válido, senão perfis
individuais) sobre `EnemySpawnPackSO` (PackId, faixa de nível, BiomeTags, Entries com
MinCount/MaxCount/Weight/IsRequired, MaxTotalEnemies). A seleção é seeded (FNV-1a,
stable-run). A liderança é emergente do role (PackLeader/PackFlanker da F24 + PackCoordinator
da F04). Com o roster expandido (F80) há agora criaturas suficientes para packs **temáticos
e legíveis** por bioma — em vez de repetir meia dúzia de inimigos.

## Problema

Sem packs curados, o roster expandido aparece como mistura aleatória sem identidade tática:
faltam composições "líder + flankers + apoio", weighting por raridade e temas de bioma
(warband orc, colônia fúngica, culto frio, ninhada draconica, coro do vazio). Isso desperdiça
a variedade de F80 e enfraquece a leitura de combate de grupo.

## Objetivo

Ao final desta spec, cada banda tem packs temáticos curados como `EnemySpawnPackSO`
(líder com PackLeader, 1–N flankers com PackFlanker, apoio caster/ranged, fodder swarm),
com weighting por raridade/role e MaxTotalEnemies coerente com o tamanho de sala (F79
footprint). A seleção continua determinística (stable-run), minibosses/bosses seguem fora
dos rolls regulares, e o validador de cobertura PASS. Combate de grupo ganha identidade por
bioma sem novo comportamento de AI.

## Fontes obrigatórias lidas

```text
Assets/_Game/Scripts/Enemy/EnemySpawnPackSO.cs (estrutura de pack/entry)
Assets/_Game/Scripts/Enemy/EnemySpawnResolver.cs (seleção seeded pack vs perfil)
Assets/_Game/Scripts/Cave/Runtime/CaveEnemySpawnPlanner.cs (4 passes, MaxEnemies)
Assets/_Game/Scripts/Cave/Runtime/CaveBandSpawnTable.cs (pool por banda, weights)
Assets/_Game/Scripts/Enemy/EnemyPackCoordinator.cs (anchor centroid, alerta coletivo)
.specs/a_implementar/fable/fable_80_spec_bestiary_expansion_40_creatures_vaalara.md (roster)
.specs/a_implementar/fable/fable_24_spec_enemy_moves_elite_affixes_runtime.md (pack moves)
.claude/rules/cave-stable-run.md ; .claude/skills/loot-table-authoring/SKILL.md (weighting seeded)
.claude/skills/rng-and-determinism/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- EnemySpawnPackSO / EnemySpawnPackEntry; EnemySpawnResolver (pack vs perfil, seeded);
- CaveEnemySpawnPlanner (4 passes, seed determinístico); CaveBandSpawnTable (weights 10/3);
- EnemyPackCoordinator (anchor = centroide; alerta/leash coletivo);
- moves de pack F24 (PackLeader/PackFlanker/RetreatAndCall).
Não existe:
- packs temáticos curados por bioma usando o roster F80;
- weighting/curadoria por raridade/role no nível de pack (só weights de pool hoje).
Auditar Fase 0:
- como packs são descobertos/carregados (database/pasta) pelo resolver;
- limites de MaxTotalEnemies vs tamanho de sala (F79 footprint/MinRoomSize);
- como a vaga de elite (F24) interage com membros de pack.
```

## Engineering stories

```text
Como jogador, quero que um pack tenha papéis legíveis (um líder que coordena, flankers que
  esperam o líder, um caster atrás, fodder na frente), para ter counterplay (matar o líder).
Como designer, quero packs temáticos por bioma (warband orc, colônia fúngica, culto frio,
  ninhada draconica, coro do vazio) reusando o roster F80.
Como stable-run, quero que a escolha de pack e composição continue determinística por seed.
```

## Escopo

```text
Inclui:
- N packs temáticos novos como EnemySpawnPackSO (≥1–2 por banda), cada um com:
  líder (role MiniBoss/Elite ou criatura com PackLeader), flankers (PackFlanker),
  apoio (caster/ranged), fodder (swarm) — Entries com MinCount/MaxCount/Weight/IsRequired;
- weighting por raridade/role no pack (líder IsRequired; apoio/fodder opcionais por weight);
- MaxTotalEnemies coerente com MinRoomSize/footprint (F79) — packs grandes só em salas grandes;
- ajuste mínimo no resolver/planner SE necessário para respeitar líder-obrigatório e
  cap por tamanho de sala (sem novo comportamento; só regra de composição);
- EditMode tests: composição determinística por seed; líder presente quando pack tem líder;
  cap por sala respeitado; minibosses/bosses nunca em rolls regulares.
```

## Fora de escopo

```text
Não inclui:
- novo comportamento de AI / novos moves (F24/F82);
- novas criaturas (F80);
- pathfinding (F83);
- wandering minibosses / gate bosses (sistemas separados);
- ecossistema/rival AI (F78).
```

## Regras de não duplicação

```text
Reusar EnemySpawnPackSO/Resolver/Planner — NÃO criar segundo sistema de pack/spawn.
Reusar moves de pack F24 e PackCoordinator F04 — NÃO reimplementar coordenação.
Seleção seeded (StableHash/FNV-1a) — sem GUID/timestamp (stable-run).
Líder/flanker = roles/moves existentes; pack apenas COMPÕE, não cria comportamento.
```

## Critérios de aceite

### CA-1 Packs temáticos por bioma
- Cada banda tem ≥1 pack temático curado (líder + flankers + apoio/fodder) usando o roster F80.
- Evidência: assets de pack + teste de existência por banda.

### CA-2 Papéis legíveis e líder-obrigatório
- Pack com líder sempre inclui o líder (IsRequired); flankers usam PackFlanker (esperam líder);
  matar o líder dispara RetreatAndCall (comportamento F24, aqui só composto).
- Evidência: teste de composição (líder presente) + referência ao teste de pack-leader-morto da F24.

### CA-3 Determinismo e cap por sala
- Mesmo seed → mesma composição; MaxTotalEnemies respeita MinRoomSize/footprint (F79).
- Evidência: EditMode test de determinismo + teste de cap por tamanho de sala.

### CA-4 Minibosses/bosses fora dos rolls
- Nenhum miniboss/boss aparece em pack regular (continuam em gate/wandering).
- Evidência: teste que verifica exclusão.

### CA-5 Stable-run e cobertura PASS
- Replay validator e ValidateEnemyCaveSpawnCoverage PASS.
- Evidência: validadores registrados no report.

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Data/EnemySpawn/Packs/      (novos EnemySpawnPackSO temáticos — evidência)
Assets/_Game/Scripts/Enemy/
  EnemySpawnResolver.cs                  (ajuste mínimo: líder-obrigatório + cap por sala)
Assets/_Game/Scripts/Cave/Runtime/
  CaveEnemySpawnPlanner.cs               (cap por MinRoomSize, se necessário)
Assets/_Game/Tests/EditMode/Cave/
  ProceduralPackEnrichmentTests.cs       (NOVO)
docs/validation/
  fable_81_spec_procedural_pack_enrichment_execution_report.md
```

## Contratos

### Data contracts
Novos `EnemySpawnPackSO` (data assets) por bioma. Sem novo campo no SO se os existentes
bastarem; se um campo de "role-slot" ajudar a curadoria, adicioná-lo de forma aditiva e
documentada. EnemyIds referenciam o roster F80 (id-stability).

### Runtime contracts
Resolver: garantir membro líder (IsRequired) e respeitar cap por sala (MinRoomSize/footprint).
Nenhum comportamento novo de AI — composição apenas. Seleção seeded inalterada na natureza.

### Save contracts
N/A — composição recomputada por seed (stable-run).

### UI contracts
Nenhuma.

## Sistemas afetados

```text
Spawn packs (núcleo) | Resolver/Planner (regra de composição) | PackCoordinator (leitura)
Validation reports
```

## Arquivos permitidos

```text
Assets/_Game/Data/EnemySpawn/Packs/** (assets de pack)
Assets/_Game/Scripts/Enemy/EnemySpawnResolver.cs
Assets/_Game/Scripts/Enemy/EnemySpawnPackSO.cs (campo aditivo se necessário)
Assets/_Game/Scripts/Cave/Runtime/CaveEnemySpawnPlanner.cs (cap por sala)
Assets/_Game/Tests/EditMode/Cave/**
docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab (edição manual)
Packages/** ; ProjectSettings/**
EnemyBrain / moves (F24/F82) ; CanonicalBestiaryCatalog (F80) ; SaveManager
Gate boss / wandering miniboss systems
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
Como packs são carregados; interação vaga-elite × membro de pack; limites por sala (F79).

### Fase 1 — Curadoria de packs
Autorar packs temáticos por banda (líder/flankers/apoio/fodder) com weighting/IsRequired.

### Fase 2 — Regra de composição
Ajuste mínimo no resolver/planner: líder-obrigatório + cap por MinRoomSize. Sem AI nova.

### Fase 3 — Validação
Testes (determinismo/líder/cap/exclusão de boss); replay; coverage; csproj; strict; report.
```

## Paralelização

- Parallelizable: CONDITIONAL — Must not run with F80/F24/F33/F78.
- Lock: EnemySpawnPackSO, EnemySpawnResolver, CaveEnemySpawnPlanner, packs.
- Reason: editam a mesma cadeia de spawn que F80/F24/F78.

## Impacto em save/load

```text
Changes save schema? NO | Adds section? NO | Migration? NO | Persists Unity refs? NO
```

## Impacto em eventos

```text
Adds events: NO | Changes events: NO (reusa EnemyCallForHelpEvent da F24)
```

## Impacto em UI/Unity

```text
Changes UI: NO | Changes scenes/prefabs: NO
Changes assets: novos EnemySpawnPackSO (evidência de criação)
Requires Play Mode final validation: YES (sentir 2–3 packs temáticos em cave)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: pack grande em sala pequena (sobreposição/soft-block). Mitigação: cap por MinRoomSize.
Risco: pack sem líder por roll. Mitigação: líder IsRequired + teste.
Risco: quebra de determinismo. Mitigação: seleção seeded inalterada; replay validator.
Risco: elite + líder empilhando dificuldade. Mitigação: documentar interação; teste de sanidade.
```

## Rollback

```text
Desabilitar/remover os novos packs (IsEnabled=false) volta ao spawn anterior (mesmo seed →
composição base). Reverter o ajuste do resolver restaura regra de composição prévia.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar carregamento de packs, interação elite×pack, limites por sala (F79).
- [ ] T002 — Autorar packs temáticos por banda (líder/flankers/apoio/fodder + weighting).
- [ ] T003 — Regra de composição: líder-obrigatório + cap por MinRoomSize (ajuste mínimo).
- [ ] T004 — Testes (determinismo/líder/cap/exclusão boss); replay; coverage; strict; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (regra de composição de pack)
- Requires EditMode tests: YES (determinismo, líder presente, cap por sala, exclusão de boss)
- Requires PlayMode/human scenario: YES (sentir packs temáticos — lote final)
- Requires regression test: YES (replay stable-run + coverage)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano com 2–3 packs temáticos

## Definition of Done

```text
Packs temáticos por banda (líder+flankers+apoio+fodder) com weighting; líder-obrigatório;
cap por sala; minibosses/bosses fora de rolls; determinismo preservado (replay+coverage PASS).
Builds 0E; run_strict_validation exit 0; report criado.
```

## Anti-regressão

```text
Mesmo CaveRunSeed → mesma composição/posições ao revisitar.
Sem GUID/timestamp; sem novo comportamento de AI; PackCoordinator/F24 intocados.
Minibosses/bosses nunca em packs regulares.
```
