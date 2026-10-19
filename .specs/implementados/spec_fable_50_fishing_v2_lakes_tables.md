# SPEC — Pesca v2: Lagos (Caverna + Fazenda), Tabelas por Bioma/Estação/Clima e Minigame de Timing

> **Spec ID:** `fable_50_spec_fishing_v2_lakes_tables`
> **Status:** A implementar
> **Wave:** FABLE Batch 10
> **Priority:** P1
> **Type:** Runtime / Data / Integration
> **Domain:** Farm / Cave / World
> **Parallelizable:** NO (lock em FishingSpot + snapshot de caverna)
> **Parallel group:** N/A
> **Can run with:** specs de quest/festival que não tocam caverna/fazenda runtime (F51 só com coordenação — ver lock)
> **Must not run with:** F09, F44 (geração/save da caverna), F12, F41 (cena/runtime da fazenda), F37 (clima/eventos consumidos)
> **Repo lock scope:** FishingSpot (World), FarmFishingService, CaveSnapshotService (fishing hook), geradores de cena da fazenda
> **Depends on:**
> - `fable_09_spec_cave_biome_layout_variety_runtime` (E27 — 1 lago/5 níveis nas bandas aquáticas; esta spec CONSOME)
> - `fable_33_spec_bestiary_data_expansion_60_creatures` (E21 — criaturas [AQUÁTICA] e drops fish_pale/mirrorfin)
> - `fable_32_spec_item_catalog_data_expansion` (E18 — itens fish_*)
> - `fable_15_spec_world_weather_farm_orphan_systems_wiring` (EXECUTADA — clima/estação + wiring da fazenda)
> **Blocks:** receitas de peixe das cadeias (mirrorfin_sashimi), conteúdo de Moonless Pool rico (futuro)
> **Scope:** FishingTableSO por bioma/estação/clima consumido pelos spots de fazenda E caverna, reconciliação do FarmFishingService órfão, minigame simples de timing (barra), pesca de inverno/clima.
> **Out of scope:** geração de lagos (F09 é a dona), pesca com rede/armadilha, aquário/troféus, NPC de pesca dedicado.

required_adrs: [ADR-0005-cave-stable-run-and-replay.md, ADR-0006-save-data-contracts-simple-dtos.md]
required_game_rules: [farm_rules.md, cave_rules.md, save_rules.md]

---

# /speckit.specify

## Contexto

Três directions convergem na pesca e nenhuma tem dono runtime completo. O CAVE_DESIGN
(§17-19) lista "salas de pesca subterrânea" e a sala especial **Moonless Pool** ("água
escura de Nyx — pesca rara, evento noturno, perigo"), preservando a regra existente de
fishing spot (10% e máx. 1 por nível). O CAVE_BESTIARY exige lagos de verdade: criaturas
[AQUÁTICA] (Lake Lurker 5-9, Mirrorfin Shoal 27-34) "só existem em níveis com lago
subterrâneo" e a F09 já assumiu o contrato "1 lago a cada 5 níveis nas bandas aquáticas" —
esta spec CONSOME esses lagos, não os gera. O SEASONS_CALENDAR_WEATHER_LUNAR amarra clima:
inverno desloca o jogo para "caverna, mineração, PESCA e crafting", "pesca de inverno pode
ter tabela própria" e Storm "pode bloquear pesca específica". O LOOT_CRAFTING_ECONOMY
prevê o asset `FishingTableSO` na lista canônica de tabelas. O ITEM_CATALOG (§11) dá os
peixes: fish_common (comida inicial), fish_pale (drop do Lake Lurker) e mirrorfin (peixe
raro de menu, sashimi da cadeia da Yael).

O repo tem pesca v1 em três pedaços desconexos: `World/FishingSpot.cs` (IInteractable com
vara obrigatória, stamina, _fishItemId fixo + LootTableSO opcional e janela de timing
crua), `Farm/Fishing/FarmFishingService.cs` (serviço com estação/rod tier/limite diário —
mas com ResolveCatch hardcoded e wiring auditável pós-F15) e o hook de caverna no
`CaveSnapshotService.CreateFishingSpotHook` (10% determinístico por StableHash, máx. 1,
dentro do snapshot estável). Não existe FishingTableSO, não existe tabela por
bioma/estação/clima, e o minigame é um timing binário sem feedback.

## Problema

Sem esta spec, pescar dá sempre o mesmo peixe em qualquer lugar do mundo: o lago de gelo
da banda 26-40 não dá mirrorfin, a Moonless Pool não diferencia nada, inverno não muda a
pesca (contradizendo a direction sazonal), e o FarmFishingService segue meio órfão com
tabela hardcoded. Os peixes do catálogo ficam inalcançáveis fora de drop de criatura, e as
receitas que dependem deles (sashimi da Yael) quebram a cadeia.

## Objetivo

Ao final desta spec, pescar na fazenda e nos lagos da caverna deve consultar UMA fonte de
verdade (`FishingTableSO` por contexto: bioma/estação/clima/hora), resolver a captura de
forma determinística por seed, respeitar regras de inverno/clima (tabela própria; Storm
bloqueia spots externos), e passar por um minigame simples de barra de timing
(Perfect/Good/Miss → qualidade/raridade), preservando o stable-run da caverna (spot vem do
snapshot; nada reroll em revisita) e o limite diário do serviço de fazenda.

## Fontes obrigatórias lidas

```text
docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md (§17-19 elementos especiais, Moonless Pool)
docs/design/gameplay/cave/CAVE_BESTIARY_CATALOG_DIRECTION_v1.0.md (regra [AQUÁTICA], Lake Lurker, Mirrorfin)
docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md (pesca por estação/clima)
docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md (FishingTableSO)
docs/design/gameplay/loot_crafting_economy/ITEM_CATALOG_DIRECTION_v1.0.md (§11 peixes)
docs/decisions/ADR-0005-cave-stable-run-and-replay.md
.claude/rules/testing-quality-gate.md
.claude/skills/spec-execution/SKILL.md
.claude/skills/rng-and-determinism/SKILL.md
.claude/skills/scene-interactable-wiring/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- World/FishingSpot.cs (IInteractable: vara obrigatória, stamina 20, edge interaction,
  _castDelaySeconds/_timingWindowSeconds, _fishItemId + LootTableSO opcional);
- Farm/Fishing/FarmFishingService.cs + FarmFishingSpotDefinition (estação, rod tier,
  farm level, limite diário soft, ResolveCatch determinístico mas HARDCODED);
- CaveSnapshotService.CreateFishingSpotHook (10%/máx.1, StableHash worldSeed|runSeed|level,
  CaveFishingSpotSnapshotEntry no snapshot — stable run);
- clima/estação runtime (F15 executada — WorldWeatherService/calendário);
- LootTableSO (padrão de tabela ponderada existente);
- itens fish_common/fish_pale/mirrorfin (catálogo F32) e criaturas aquáticas (F33).
Não existe:
- FishingTableSO (bioma/estação/clima) e resolver único;
- consumo de tabela nos spots (farm usa hardcode; cave spot não materializa pesca rica);
- minigame de barra (timing atual binário sem feedback);
- regras de inverno/Storm aplicadas à pesca.
Auditar Fase 0:
- como o spot de caverna é materializado em cena a partir do CaveFishingSpotSnapshotEntry
  (existe interactable spawned? CaveRuntimeMaterializer);
- estado real do wiring do FarmFishingService pós-F15 (quem o instancia; spots da fazenda
  no gerador CreateMvpFarmScene);
- shape do clima/estação exposto (enum/ids) para chavear tabelas;
- se F09 já foi executada (lagos garantidos) — sem ela, spots de caverna continuam no hook
  10% atual e o vínculo lago↔spot fica documentado como integração.
```

## Engineering stories

```text
Como jogador, quero que o lago de gelo dê peixes diferentes do açude da fazenda.
Como jogador, quero que inverno mude a pesca (tabela própria) e Storm feche o açude externo.
Como jogador, quero um minigame curto de barra: acertar o centro = peixe melhor.
Como caverna, quero que o spot pescado de um nível revisitado seja o MESMO (stable run).
Como economia/quests, quero mirrorfin e fish_pale chegando pelos lagos certos das bandas certas.
```

## Escopo

```text
Inclui:
- FishingTableSO (NOVO): tableId + entradas {itemId, peso, raridade, estações permitidas,
  climas permitidos, janela horária opcional, qualidade base} — tipos simples e IDs;
- tabelas autoradas v1 (gerador Editor): farm_pond (4 estações; inverno = tabela própria),
  cave_lake_band_1_10 (fish_pale...), cave_lake_band_26_40 (mirrorfin...),
  moonless_pool (rara/noturna — consumida quando F09 materializar a sala; flag de contexto);
- FishingCatchResolver (NOVO, puro): (tableId, seed, dia, estação, clima, hora, timing
  grade) → {itemId, raridade, qualidade}; determinístico por StableHash(seed|dia|spotId|cast);
- FarmFishingService: substituir ResolveCatch hardcoded pelo resolver/tabela (mantendo
  limite diário, rod tier, farm level — contratos atuais preservados);
- World/FishingSpot: campo tableId (aditivo; _fishItemId vira fallback legado) + consumo
  do resolver + clima/estação lidos do serviço F15; Storm bloqueia spot externo
  (mensagem via PlayerActionFeedbackEvent existente);
- spot de caverna: materialização consome o CaveFishingSpotSnapshotEntry EXISTENTE
  (10%/máx.1 intocados) e escolhe tableId pelo bioma/banda do nível — determinístico,
  sem novo roll em revisita (ADR-0005);
- minigame de timing (barra): estados Cast → janela com cursor oscilante → input fecha em
  Perfect/Good/Miss; Perfect melhora raridade/qualidade 1 passo, Miss = sem captura e
  metade da stamina; implementação lógica + feedback por eventos (sem canvas novo — HUD
  prompt existente);
- save: nenhum estado novo além do que já persiste (limite diário do FarmFishingService —
  auditar onde vive; snapshot da caverna inalterado);
- EditMode tests: resolver determinístico (mesmo seed/dia = mesma captura), chaveamento
  por estação/clima (inverno/Storm), tabela por bioma, grades do minigame (Perfect/Good/
  Miss), regressão do hook 10%/máx.1 e do limite diário.
```

## Fora de escopo

```text
Não inclui:
- geração de lagos/salas (F09 é a dona — esta spec só consome bioma/banda/sala);
- evento noturno/perigo da Moonless Pool (apenas a tabela rara; evento fica com F37/futuro);
- pesca com rede/armadilha, aquário, troféus, NPC de pesca;
- receitas que usam peixe (catálogo/F49 — esta spec só entrega o peixe);
- UI canvas nova (barra é lógica + feedback em prompt/toast existente; visual rico = F14).
```

## Regras de não duplicação

```text
Uma ÚNICA fonte de verdade de captura: FishingTableSO + FishingCatchResolver — farm e cave
consomem o mesmo resolver (matar o hardcode do FarmFishingService, não criar um segundo).
Não duplicar clima/estação — ler do serviço F15 (nunca recalcular).
Não recriar o hook de spot da caverna — CaveFishingSpotSnapshotEntry/10%/máx.1 intocados.
Não recriar LootTableSO — FishingTableSO é específico (estação/clima); auditar na Fase 0
se estender LootTableSO cobre o caso antes de criar SO novo.
```

## Critérios de aceite

### CA-1 Tabela por contexto

- Pescar no açude da fazenda e num lago da banda 26-40 com o mesmo seed dá resultados de
  TABELAS diferentes (farm_pond vs cave_lake_band_26_40 com mirrorfin).
- Evidência: EditMode tests do resolver com os dois tableIds.

### CA-2 Determinismo e stable run

- Mesmo seed + dia + spot + cast = mesma captura; spot de caverna em nível revisitado não
  reroll (snapshot preservado); 10%/máx.1 inalterados.
- Evidência: EditMode tests de determinismo + regressão do CreateFishingSpotHook.

### CA-3 Estação e clima mandam

- Inverno usa a tabela de inverno do spot; Storm bloqueia spot externo com feedback;
  entrada com estação/clima incompatível nunca é sorteada.
- Evidência: EditMode tests de chaveamento (4 estações × climas relevantes).

### CA-4 Minigame com consequência

- Perfect melhora raridade/qualidade 1 passo; Miss não captura e consome metade da
  stamina; Good = captura padrão. Grades determinísticas dado o input.
- Evidência: EditMode tests das 3 grades no resolver.

### CA-5 Fazenda reconciliada

- FarmFishingService consome o resolver (sem hardcode), mantendo limite diário/rod tier/
  farm level; spots da fazenda apontam tableId.
- Evidência: EditMode tests de regressão dos contratos atuais + novo caminho de tabela.

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/World/Fishing/
  FishingTableSO.cs           (NOVO — entradas por estação/clima/hora, tipos simples)
  FishingCatchResolver.cs     (NOVO — puro, determinístico, grades de timing)
  FishingTimingMinigame.cs    (NOVO — máquina de estados da barra, sem MonoBehaviour pesado)
Assets/_Game/Scripts/World/FishingSpot.cs        (ADITIVO — tableId, clima, minigame, resolver)
Assets/_Game/Scripts/Farm/Fishing/FarmFishingService.cs (ADITIVO — resolver no lugar do hardcode)
Assets/_Game/Scripts/Cave/** (apenas materialização do spot existente → tableId por bioma)
Assets/_Game/Scripts/Editor/ (gerador GenerateFishingTables — AssetDatabase)
Assets/_Game/Tests/EditMode/World/FishingV2Tests.cs (NOVO)
docs/validation/fable_50_spec_fishing_v2_lakes_tables_execution_report.md
```

## Contratos

### Data contracts

- `FishingTableSO`: {tableId:string, entries[]: {itemId, weight:int, rarity, seasons[],
  weathers[], hourWindow?, baseQuality}} — só tipos simples/IDs (ADR-0006).
- Tabelas v1: farm_pond, farm_pond_winter, cave_lake_band_1_10, cave_lake_band_26_40,
  moonless_pool (IDs estáveis; conteúdo dos peixes do catálogo §11 + fish comuns).

### Runtime contracts

- `FishingCatchResolver.Resolve(table, contexto{seed, dia, spotId, castIndex, estação,
  clima, hora}, timingGrade)` — puro; filtro por contexto ANTES do roll ponderado;
  StableHash, zero Random.
- `FishingTimingMinigame`: `Start() → Tick(t) → Submit(input) → Grade {Perfect|Good|Miss}` —
  lógica testável fora de cena.
- FishingSpot/FarmFishingService consomem resolver; cave materializer escolhe tableId por
  banda/bioma do nível (função pura testável).

### Event contracts

- `FishCaughtEvent(spotId, itemId, rarity, grade)` (NOVO — HUD/quests/bestiário escutam).
- Feedbacks via PlayerActionFeedbackEvent existente (bloqueio por Storm/vara/stamina).

### Save contracts

- Sem seção nova. Limite diário do FarmFishingService continua no estado atual (auditar
  owner); snapshot de caverna INALTERADO (CaveFishingSpotSnapshotEntry preservado).

### UI contracts

- Sem canvas novo: barra de timing comunica estado por prompt/toast existentes; visual
  dedicado fica para F14/F20. Sem modal.

## Sistemas afetados

```text
World (FishingSpot, clima/estação consumidos)
Farm (FarmFishingService reconciliado)
Cave (materialização do spot existente — cirúrgico, stable run intocado)
Items/loot (peixes do catálogo entregues)
Event bus (FishCaughtEvent)
Editor (gerador de tabelas)
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/World/Fishing/** (novos)
Assets/_Game/Scripts/World/FishingSpot.cs (aditivo)
Assets/_Game/Scripts/Farm/Fishing/** (resolver no lugar do hardcode; contratos preservados)
Assets/_Game/Scripts/Cave/** (APENAS ponto de materialização do fishing spot → tableId)
Assets/_Game/Scripts/Editor/ (GenerateFishingTables + spots no gerador da fazenda se necessário)
Assets/_Game/Scripts/Core/Events/FishCaughtEvent.cs
Assets/_Game/Tests/EditMode/World/**
docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset por edição manual de YAML (assets/cena SÓ via gerador)
Packages/** ; ProjectSettings/**
Geração procedural da caverna (lagos/salas = F09; seeds/snapshot core = ADR-0005)
CaveSnapshotService.CreateFishingSpotHook (regra 10%/máx.1 INTOCADA)
WorldWeatherService/calendário (F15 — apenas leitura)
SaveManager core
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
Materialização atual do spot de caverna; wiring real do FarmFishingService pós-F15; shape
de clima/estação; LootTableSO cobre estação/clima? F09 executada?

### Fase 1 — Tabela e resolver
FishingTableSO + FishingCatchResolver + gerador das 5 tabelas v1 + testes de determinismo
e chaveamento por contexto.

### Fase 2 — Consumidores
FarmFishingService no resolver (regressão dos contratos) + FishingSpot com tableId/clima
(Storm bloqueia) + materialização cave → tableId por banda + FishCaughtEvent.

### Fase 3 — Minigame
FishingTimingMinigame (barra Perfect/Good/Miss) integrado ao FishingSpot + grades no
resolver + testes das 3 grades.

### Fase 4 — Fechamento
Regressões (hook 10%, limite diário); csproj; run_strict_validation; execution report.
```

## Paralelização

- Parallelizable: NO.
- Parallel group: N/A.
- Can run with: F53 (festival quests — superfícies disjuntas; coordenação apenas em docs).
- Must not run with: F09/F44 (caverna), F12/F41 (fazenda), F37 (clima/eventos), F48/F49
  (itens, se tocarem geradores simultaneamente).
- Shared files/systems that require lock: FishingSpot, FarmFishingService,
  CaveSnapshotService (leitura), geradores de cena da fazenda.
- Reason: toca runtime de fazenda E materialização de caverna ao mesmo tempo — vizinhos de
  cena/snapshot não podem rodar juntos.

## Impacto em save/load

```text
Does this change save schema? NO (snapshot de caverna inalterado; limite diário já existente)
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? NO (tabelas = SO consultados; capturas viram itens de
inventário pelo fluxo existente)
```

## Impacto em eventos

```text
Adds events: YES — FishCaughtEvent
Changes existing events: NO (PlayerActionFeedbackEvent reutilizado)
Requires unsubscribe pattern: NO (spot é interactable de cena com ciclo de vida próprio)
```

## Impacto em UI/Unity

```text
Changes UI: NO (prompt/toast existentes; barra visual rica deferida à F14/F20)
Changes scenes: via gerador apenas (spots da fazenda, se a Fase 0 indicar ausência)
Changes prefabs: NO
Changes ScriptableObjects/assets: via gerador apenas (FishingTableSO ×5)
Requires Play Mode final validation: YES (pescar na fazenda e num lago da caverna)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: violar stable run da caverna (reroll de spot/captura em revisita).
Mitigação: spot do snapshot intocado; resolver determinístico por StableHash(seed|dia|spot|
cast); regra cave-stable-run + teste de regressão.

Risco: duas fontes de verdade (FarmFishingService hardcode + tabela).
Mitigação: hardcode REMOVIDO na mesma spec; teste garante caminho único.

Risco: F09 não executada (sem lagos garantidos) no momento desta spec.
Mitigação: hook 10% atual continua válido como spot; vínculo lago↔spot documentado como
integração pendente no report (sem bloquear as tabelas).

Risco: minigame travar interação (estado preso em Cast).
Mitigação: máquina de estados com timeout para Miss + teste; interação atual permanece o
fallback se o minigame estiver desabilitado por flag.

Risco: tabela sem entrada válida para um contexto (inverno + Storm).
Mitigação: resolver retorna NoCatch explícito com feedback (nunca exception) + teste.
```

## Rollback

```text
Flag de minigame off + tableId vazio devolvem o comportamento v1 (_fishItemId fallback).
Remover resolver/tabelas restaura o fluxo atual; snapshot/limite diário intactos. Nenhum
save real afetado.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Fase 0: auditar materialização do spot cave, wiring FarmFishingService, shape clima/estação, F09.
- [ ] T002 — FishingTableSO + FishingCatchResolver + gerador das 5 tabelas + testes (determinismo/contexto).
- [ ] T003 — FarmFishingService → resolver (sem hardcode) + FishingSpot com tableId/clima/Storm + cave tableId por banda.
- [ ] T004 — FishingTimingMinigame (Perfect/Good/Miss) + grades no resolver + FishCaughtEvent + testes.
- [ ] T005 — Regressões (hook 10%/máx.1, limite diário, fallback legado); csproj; run_strict_validation; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (resolver, chaveamento por contexto, grades, tableId por
  banda)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (lote final — pescar fazenda +
  lago de caverna + inverno)
- Requires regression test: YES (hook 10%/máx.1; limite diário; fallback _fishItemId;
  stable run em revisita)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes EditMode + cenário humano com 1 captura
  na fazenda, 1 em lago de caverna e 1 bloqueio por Storm

## Definition of Done

```text
FishingTableSO + resolver único determinístico consumidos por fazenda E caverna; 5 tabelas
v1 geradas; inverno/Storm aplicados; minigame de barra com 3 grades e consequência;
FarmFishingService sem hardcode; stable run e limite diário intactos; builds 0E; execution
report com Spec Compliance Matrix.
```

## Anti-regressão

```text
CaveFishingSpotSnapshotEntry/10%/máx.1 intocados; nível revisitado nunca reroll (ADR-0005).
Contratos do FarmFishingService preservados (rod tier, farm level, limite diário).
Vara obrigatória/stamina/edge interaction do FishingSpot intactos.
Clima/estação SÓ lidos do serviço F15; zero Random em captura (StableHash).
Nenhum GameObject.Find; eventos só via GameEventBus; DTOs/SO só tipos simples + IDs.
```


---

## EMENDA 2026-06-12-D (Decisões v2 — VINCULANTE; fonte: FABLE_DECISOES_RESPOSTAS_v2.0.md)

```text
1. Mirrorfin Shoal / Lake Lurker no lago da FAZENDA (5.6-A): evento raríssimo (0,5%),
   somente à noite — entrada extra nas tabelas do lago da fazenda.
```
