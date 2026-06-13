# SPEC — Fazenda: Processamento (Queijaria/Barril) + Estufa Mínima

> **Spec ID:** `fable_55_spec_farm_processing_greenhouse`
> **Status:** A implementar
> **Wave:** FABLE Batch 10
> **Priority:** P2
> **Type:** Integration / Runtime / Editor
> **Domain:** Farm / Crafting
> **Parallelizable:** CONDITIONAL (lock do gerador da FarmScene e da seção farm do save)
> **Parallel group:** fable_batch10_farm
> **Can run with:** F58, F59, F60 (locks disjuntos)
> **Must not run with:** F54 (mesmo gerador CreateMvpFarmScene + seção farm), F12, F41 (geradores)
> **Repo lock scope:** `CreateMvpFarmScene.cs`, Farm/Processing/**, Farm/Watering/GreenhouseContextProvider, sistema de crafting com tempo (Craft/**), seção farm do save
> **Depends on:**
> - F12 (E31 — animais; goat_milk como insumo da queijaria)
> - F32 (E18 — itens goat_cheese/vale_wine no catálogo)
> - F41 (E30 — padrão de lote/zona destravável da fazenda)
> **Blocks:** N/A
> **Scope:** 2 estações de processamento com job de N dias (queijaria, barril) + estufa pequena onde o plantio ignora estação.
> **Out of scope:** demais processadores do catálogo, estufa avançada, automação por companion, qualidade avançada de output.

required_adrs: []
required_game_rules: [farm_rules.md, save_rules.md]

---

# /speckit.specify

## Contexto

O ITEM_CATALOG v1.0 (§5) define dois consumíveis que dependem de PROCESSAMENTO físico na
fazenda: `item_consumable_food_goat_cheese` (65g — "goat_milk ×2 (queijaria)") e
`item_consumable_food_vale_wine` (120g — "grape ×5 (barril, 2 dias)"; presente amado). A
pendência do próprio catálogo entrega a bola para esta spec: "Receitas de processamento
(queijaria/barril) — números na spec de farm animals/crafting". FARM_DESIGN v1.3 lista
processadores como promessa do farm sim; FARM_LAYOUT_SCALE_BUILDINGS define a
`farm_greenhouse_small` (10x8 — "plantio fora de estação") como construção do nível 3.

O repo tem TRÊS superfícies sobrepostas para "transformar item com tempo":

1. `Craft/` (WI-14, VIVO): `CraftingStation` + `CraftingJob` (RemainingSeconds, save
   `CraftingJobSaveData`, restauração) — crafting com tempo já integrado e salvo.
2. `Crafting/` (ÓRFÃO): `CraftingService` + ProcessingJob por ticks de dia.
3. `Farm/Processing/` (ÓRFÃO): `ProcessableItem` + `FarmProcessingJob` (StartDay/
   FinishDay, estados Queued→Processing→ReadyToCollect→Collected, Collect idempotente).

E `Farm/Watering/GreenhouseContextProvider` (ÓRFÃO) já implementa exatamente o contrato
da estufa: `RegisterGreenhousePlot`, `SetGreenhouseUnlocked`, `CanOverrideSeason` e
`IsRainExcluded` — ninguém o instancia.

## Problema

Sem processamento, goat_cheese e vale_wine são inalcançáveis (receitas órfãs do catálogo)
e o leite/uva não têm sink de valor agregado; sem estufa, a sazonalidade vira só
restrição, sem a válvula prometida pelas directions ("estufa contorna sazonalidade" —
FARM_DESIGN §52). E o risco maior é arquitetural: com 3 superfícies de job sobrepostas,
qualquer implementação descuidada cria o QUARTO sistema paralelo — exatamente o que a
regra system-reuse-audit proíbe. A decisão de reuso precisa ser a Fase 0 desta spec, com
o duplicado explicitamente aposentado.

## Objetivo

Ao final desta spec, a fazenda deve ter 2 estações de processamento físicas geradas pelo
gerador (queijaria e barril) como interactables: depositar insumos (goat_milk ×2 /
grape ×5) inicia um job que termina em N dias (queijo 1 dia; vinho 2 dias — catálogo) e
fica `ReadyToCollect` para coleta idempotente; uma estufa pequena (zona/lote via gerador,
padrão F41) onde canteiros registrados no `GreenhouseContextProvider` plantam fora de
estação e não recebem rega de chuva; com UMA única superfície de job (decidida na Fase 0
entre Craft WI-14 e FarmProcessingJob, com o duplicado documentado como aposentado),
persistência por campos aditivos e save legado seguro.

## Fontes obrigatórias lidas

```text
docs/design/gameplay/loot_crafting_economy/ITEM_CATALOG_DIRECTION_v1.0.md (§5 receitas, §pendências)
docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md (§processadores, §52 estufa/sazonalidade)
docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md (farm_greenhouse_small 10x8)
.claude/rules/testing-quality-gate.md
.claude/skills/system-reuse-audit/SKILL.md
.claude/skills/scene-interactable-wiring/SKILL.md
.claude/skills/save-load-pattern/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- Craft/{CraftingStation, CraftingJob} (WI-14 — crafting com tempo VIVO, com save);
- Farm/Processing/{ProcessableItem, FarmProcessingJob} (ÓRFÃO — job por dia,
  Collect idempotente);
- Crafting/{CraftingService, ProcessingJob} (ÓRFÃO — segundo caminho por ticks);
- Farm/Watering/GreenhouseContextProvider (ÓRFÃO — CanOverrideSeason/IsRainExcluded);
- FarmPlot + FarmPlotRegistry (plantio; valida estação da semente);
- GameCalendarService (estação) / DayStartedEvent / TimeManager;
- CreateMvpFarmScene (gerador — estações e estufa entram aqui);
- padrão de lote destravável da F41 (cerca/conteúdo desativado/refs serializadas);
- InventoryManager (insumos/outputs); SaveManager + seção farm.
Não existe:
- estação de processamento física; receita de queijaria/barril ligada; estufa em cena;
  instância do GreenhouseContextProvider; FarmPlot consultando override de estação.
Auditar Fase 0 (OBRIGATÓRIA — system-reuse-audit):
- DECISÃO DE REUSO: jobs das estações via Craft WI-14 (tempo em segundos + save pronto)
  OU FarmProcessingJob (dias — semântica do catálogo "2 dias")? Critérios: persistência
  pronta, semântica de dias, menor diff. O caminho NÃO escolhido é declarado aposentado
  no report (e Crafting/CraftingService órfão registrado como delete candidate futuro);
- onde o FarmPlot valida estação da semente (ponto único para consultar
  GreenhouseContextProvider.CanOverrideSeason);
- como a RainIrrigationIntegration (F15) rega (ponto para IsRainExcluded);
- estufa: zona sempre presente ou lote destravável (padrão F41)? Decidir pelo layout
  atual da FarmScene (26x20) — v1 recomendado: zona pequena sempre presente.
```

## Engineering stories

```text
Como jogador, quero transformar leite em queijo e uva em vinho com tempo de espera real,
  para produção ter planejamento (e o vinho ser o presente amado do catálogo).
Como jogador, quero plantar fora de estação na estufa, para a sazonalidade ter válvula.
Como arquitetura, quero UMA superfície de job com tempo, para não nascer o 4º sistema
  paralelo de crafting.
Como sistema de save, quero jobs e estufa persistidos com tipos simples, para reload
  no meio do job não perder nem duplicar produção.
```

## Escopo

```text
Inclui:
- Fase 0 OBRIGATÓRIA: decisão de reuso (Craft WI-14 × FarmProcessingJob) com critérios
  e aposentadoria do duplicado documentadas no report;
- 2 estações físicas no gerador (Zone_Construction ou posição livre auditada):
  station_cheese_press ("Queijaria") e station_wine_barrel ("Barril de Vinho"),
  interactables com refs serializadas (sem Find);
- receitas: goat_milk ×2 → goat_cheese (1 dia); grape ×5 → vale_wine (2 dias) —
  números do catálogo §5; insumos consumidos no início; coleta idempotente no fim;
- avanço do job por DayStartedEvent (semântica de dias) na superfície escolhida;
- estufa mínima: área pequena no gerador com 4 canteiros FarmPlot registrados no
  GreenhouseContextProvider (instanciado em host runtime); plantio nesses canteiros
  ignora validação de estação (CanOverrideSeason) e chuva não os rega (IsRainExcluded);
- FarmPlot: consulta ao provider no ponto único de validação de estação (diff mínimo);
- save: campos aditivos na seção farm (jobs ativos por estação — ids/ints; flag/estado
  da estufa se destravável); save legado carrega sem jobs e sem estufa destravada;
- ProcessingJobCompletedEvent (NOVO — toast "Queijo pronto!" via feedback existente);
- EditMode tests: job round-trip (start → N dias → ready → collect 1×), consumo de
  insumos, coleta idempotente, save/load no meio do job, plantio fora de estação só na
  estufa, chuva não rega estufa, save legado.
```

## Fora de escopo

```text
Não inclui:
- demais processadores do catálogo (forno, prensa de óleo etc. — follow-up);
- farm_greenhouse_large / endgame agrícola;
- automação por companion (direction futura);
- qualidade do output derivada da qualidade do insumo (v1: qualidade base — documentar);
- venda/preço (itens já têm BV no catálogo F32);
- arte final das estações/estufa (placeholder do gerador).
```

## Regras de não duplicação

```text
PROIBIDO criar 4ª superfície de job — Fase 0 escolhe entre Craft WI-14 e
FarmProcessingJob; o perdedor é aposentado por documentação (não deletado nesta spec).
Não criar segundo provider de estufa — GreenhouseContextProvider órfão é o contrato.
Não criar segundo padrão de interactable/gerador — scene-interactable-wiring + F41.
Não criar seção de save nova — campos ADITIVOS na seção farm existente.
Não duplicar validação de estação — ponto único no FarmPlot consulta o provider.
```

## Critérios de aceite

### CA-1 Decisão de reuso documentada

- O report contém a decisão Craft WI-14 × FarmProcessingJob com critérios (persistência,
  semântica de dias, diff) e declara o caminho não escolhido como aposentado.
- Evidência: seção "Existing systems audit" do execution report.

### CA-2 Processamento funcional por dias

- Depositar goat_milk ×2 na queijaria inicia job que fica pronto no dia seguinte;
  grape ×5 no barril fica pronto em 2 dias; insumos são consumidos no início; coletar
  entrega o output exatamente 1× (2ª coleta falha).
- Evidência: EditMode tests de timing por DayStarted, consumo e idempotência.

### CA-3 Job sobrevive a save/load

- Salvar com job em andamento e recarregar preserva estação, receita e dias restantes;
  o job termina no dia correto; reload após pronto não duplica output.
- Evidência: EditMode test de round-trip no meio e no fim do job.

### CA-4 Estufa ignora estação e chuva

- Canteiro da estufa aceita semente fora de estação (CanOverrideSeason) e NÃO acorda
  regado em dia de chuva (IsRainExcluded); canteiro comum continua recusando semente
  fora de estação.
- Evidência: EditMode tests com provider sintético + regressão do canteiro comum.

### CA-5 Save legado seguro

- Save legado (sem os campos novos) carrega sem erro, sem jobs e com estações vazias.
- Evidência: EditMode test de seção legada (campos ausentes = defaults).

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Farm/Processing/
  FarmProcessingStationService.cs (NOVO — host bootstrap; estações, receitas, avanço por
                                   DayStarted; delega job à superfície escolhida na Fase 0)
  ProcessingStationInteractable.cs (NOVO — IInteractable: depositar/coletar)
  ProcessingRecipe.cs             (NOVO — inputItemId/qty → outputItemId/qty + dias)
Assets/_Game/Scripts/Farm/Watering/
  GreenhouseRuntimeHost.cs        (NOVO — instancia GreenhouseContextProvider, registra
                                   canteiros da estufa via refs do gerador)
Assets/_Game/Scripts/Farm/FarmPlot.cs (diff mínimo — consulta CanOverrideSeason no ponto
                                   único de validação de estação)
Assets/_Game/Scripts/Core/Events/ProcessingJobCompletedEvent.cs (NOVO)
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs (ATUALIZADO — 2 estações
                                   + área da estufa com 4 canteiros + refs serializadas)
seção farm do save               (campos aditivos: processingJobs)
Assets/_Game/Tests/EditMode/Farm/ProcessingGreenhouseTests.cs (NOVO)
docs/validation/fable_55_spec_farm_processing_greenhouse_execution_report.md
```

## Contratos

### Data contracts

- `ProcessingRecipe`: recipe_goat_cheese {goat_milk ×2 → goat_cheese ×1, 1 dia},
  recipe_vale_wine {grape ×5 → vale_wine ×1, 2 dias} — itens do catálogo F32.
- Estações com IDs estáveis: `station_cheese_press_01`, `station_wine_barrel_01`.
- Canteiros da estufa: IDs estáveis `greenhouse_plot_01..04` registrados no provider.

### Runtime contracts

- `FarmProcessingStationService`: dono das estações; `StartJob(stationId, recipeId)`
  consome insumos via InventoryManager e cria o job na superfície escolhida (Fase 0);
  em DayStartedEvent avança jobs (dias); `Collect(stationId)` idempotente → output no
  inventário + ProcessingJobCompletedEvent.
- `GreenhouseRuntimeHost`: instancia o provider, registra os 4 canteiros (refs do
  gerador), `SetGreenhouseUnlocked(true)` no v1 (zona sempre presente; se a Fase 0
  decidir lote destravável, segue padrão F41/UseDeed).
- `FarmPlot`: no ponto único de validação de estação, consulta
  `provider.CanOverrideSeason(plotId)` antes de recusar semente fora de estação;
  `RainIrrigationIntegration` (F15) pula canteiros com `IsRainExcluded`.

### Event contracts

- `ProcessingJobCompletedEvent` (NOVO — stationId, outputItemId; toast escuta).
- Consome: DayStartedEvent. Demais via eventos existentes (GameEventBus).

### Save contracts

- Campos ADITIVOS na seção farm: `processingJobs` (lista — stationId, recipeId,
  startDay, finishDay, state, outputCollected). Se a Fase 0 escolher Craft WI-14, a
  persistência reaproveita CraftingJobSaveData existente (documentar no report qual
  caminho persiste). Somente tipos simples; sem referências Unity.

### UI contracts

- Prompt de interação ("Depositar leite" / "Coletar queijo" / "Em produção: N dias")
  via InteractionPrompt existente; toast via feedback existente. Sem tela nova.

## Sistemas afetados

```text
Farm runtime (estações + estufa; 2 órfãos ganham host ou são aposentados)
Crafting (superfície de job reusada — decisão Fase 0)
Gerador da FarmScene (estações + estufa)
FarmPlot (validação de estação — diff mínimo)
RainIrrigationIntegration (exclusão da estufa)
Save (campos aditivos na seção farm)
Event bus (ProcessingJobCompletedEvent)
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Farm/Processing/** (service/interactable/recipe novos)
Assets/_Game/Scripts/Farm/Watering/GreenhouseRuntimeHost.cs (novo)
Assets/_Game/Scripts/Farm/FarmPlot.cs (diff mínimo no ponto de validação de estação)
Assets/_Game/Scripts/Farm/RainIrrigationIntegration.cs (pular IsRainExcluded — diff mínimo)
Assets/_Game/Scripts/Craft/** (SOMENTE se a Fase 0 escolher WI-14 — extensão aditiva)
Assets/_Game/Scripts/Core/Events/ProcessingJobCompletedEvent.cs (novo)
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs (atualizar)
seção farm do save (campos aditivos)
Assets/_Game/Tests/EditMode/Farm/** ; docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset por edição manual de YAML (cena SÓ via gerador)
Packages/** ; ProjectSettings/**
Crafting/CraftingService (órfão duplicado — não usar, não estender, não deletar aqui)
GreenhouseContextProvider.cs (contrato órfão — instanciar, não reescrever)
SaveManager core (apenas campos aditivos na seção farm)
CreateMvpTownScene / geradores de outras cenas
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria e decisão de reuso (OBRIGATÓRIA)
Craft WI-14 × FarmProcessingJob (critérios: persistência, semântica de dias, diff);
ponto de validação de estação no FarmPlot; ponto de rega da RainIrrigationIntegration;
estufa zona × lote F41. Tudo documentado no report ANTES de código.

### Fase 1 — Estações e receitas
ProcessingRecipe + FarmProcessingStationService + ProcessingStationInteractable +
2 estações no gerador + ProcessingJobCompletedEvent + testes (timing/consumo/
idempotência).

### Fase 2 — Persistência
Campos aditivos (processingJobs ou reuso CraftingJobSaveData) + round-trip no meio/fim
do job + seção legada + testes.

### Fase 3 — Estufa
GreenhouseRuntimeHost + área no gerador (4 canteiros, refs serializadas) + consulta no
FarmPlot + exclusão de chuva + testes (override de estação/chuva/regressão do canteiro
comum).

### Fase 4 — Fechamento
Regeneração da FarmScene com evidência; csproj; run_strict_validation; execution report
com a decisão de reuso e a aposentadoria do duplicado.
```

## Paralelização

- Parallelizable: CONDITIONAL.
- Parallel group: fable_batch10_farm.
- Can run with: F58, F59, F60 (locks disjuntos).
- Must not run with: F54 (mesmo CreateMvpFarmScene + seção farm), F12, F41 (geradores).
- Shared files/systems that require lock: CreateMvpFarmScene.cs, seção farm do save,
  FarmPlot.cs, Craft/** (se escolhido).
- Reason: edita gerador compartilhado, FarmPlot (superfície quente do farm loop) e a
  mesma seção de save da F54 — ordem serial dentro do grupo farm.

## Impacto em save/load

```text
Does this change save schema? YES (campos aditivos na seção farm: processingJobs;
ou reuso da persistência WI-14 — decisão Fase 0 documentada)
Does this add a save section? NO
Does this require migration? NO (campos ausentes = sem jobs; estufa default por decisão
da Fase 0 — zona sempre presente não persiste estado)
Does this persist Unity references? NO (IDs/ints)
Round-trip obrigatório: job no meio → save → load → termina no dia certo, coleta 1×.
```

## Impacto em eventos

```text
Adds events: YES — ProcessingJobCompletedEvent
Changes existing events: NO
Requires unsubscribe pattern: YES (service assina DayStartedEvent)
```

## Impacto em UI/Unity

```text
Changes UI: prompts de interação + toast (fluxos existentes; sem tela nova)
Changes scenes: YES — FarmScene regenerada VIA GERADOR (evidência obrigatória)
Changes prefabs: NO
Changes ScriptableObjects/assets: NO (receitas em código no v1)
Requires Play Mode final validation: YES (lote final)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: nascer o 4º sistema de job paralelo.
Mitigação: Fase 0 obrigatória com decisão e aposentadoria documentadas; revisão
non-regression confere que só UMA superfície cria jobs.

Risco: reload duplicar output (job pronto coletado 2×).
Mitigação: Collect idempotente (já existe em FarmProcessingJob/WI-14) + estado
persistido + teste de reload pós-pronto.

Risco: FarmPlot regredir (validação de estação é caminho quente de plantio).
Mitigação: diff mínimo em ponto único + teste de regressão do canteiro comum.

Risco: chuva regar estufa (F15 RainIrrigationIntegration ignora o provider).
Mitigação: exclusão via IsRainExcluded com teste; F15 já executada — diff mínimo.

Risco: insumos consumidos e job perdido em falha de start.
Mitigação: consumir SÓ após validação completa da receita; teste de falha devolve.

Risco: itens goat_cheese/vale_wine/goat_milk/grape ausentes do catálogo gerado.
Mitigação: Fase 0 audita F32/F12; ausência = BLOCKED_BY_DEPENDENCY_PENDING (não
inventar itens aqui).
```

## Rollback

```text
Estações/estufa não geram (gerador revertido) = fazenda atual intacta; service/host
removíveis; campos aditivos ignorados por loads antigos; FarmPlot reverte o diff
mínimo. Não apagar save real do usuário.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Fase 0: decisão de reuso (Craft WI-14 × FarmProcessingJob) + auditorias
        (FarmPlot, chuva, estufa zona×lote, itens F32/F12) documentadas.
- [ ] T002 — Receitas + service + interactable + 2 estações no gerador + evento +
        testes (timing/consumo/idempotência).
- [ ] T003 — Persistência dos jobs (aditiva ou WI-14) + round-trip + legado + testes.
- [ ] T004 — Estufa: host + área no gerador + consulta no FarmPlot + exclusão de chuva
        + testes (override/chuva/regressão).
- [ ] T005 — Regeneração com evidência; csproj; run_strict_validation; report com
        decisão de reuso.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (timing de job por dias, consumo/coleta, override de
  estação, save round-trip)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (lote final)
- Requires regression test: YES (plantio/rega/colheita do canteiro comum intactos;
  crafting WI-14 existente intacto)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano produzindo queijo
  (1 dia) e vinho (2 dias) e plantando fora de estação na estufa

## Definition of Done

```text
Queijaria e barril físicos funcionando com jobs de 1/2 dias (receitas do catálogo §5),
consumo no início e coleta idempotente; UMA superfície de job (decisão Fase 0 +
duplicado aposentado por documentação); estufa com 4 canteiros plantando fora de
estação e excluídos da chuva; persistência aditiva com round-trip e legado seguro;
builds 0E; run_strict_validation exit 0; execution report com decisão de reuso.
```

## Anti-regressão

```text
Canteiro comum continua recusando semente fora de estação e sendo regado pela chuva.
Crafting WI-14 existente sem mudança de contrato (extensão aditiva no máximo).
Nenhuma 4ª superfície de job criada; Crafting/CraftingService órfão intocado.
Save legado compatível (campos ausentes = sem jobs); só tipos simples no DTO.
Zero GameObject.Find em runtime (refs serializadas); eventos só via GameEventBus.
```
