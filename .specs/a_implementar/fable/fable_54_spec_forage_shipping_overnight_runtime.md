# SPEC — Fazenda: Forrageio Sazonal + Shipping Bin Noturno (wiring de órfãos)

> **Spec ID:** `fable_54_spec_forage_shipping_overnight_runtime`
> **Status:** A implementar
> **Wave:** FABLE Batch 10
> **Priority:** P2
> **Type:** Integration / Runtime
> **Domain:** Farm / Economy
> **Parallelizable:** CONDITIONAL (lock do gerador da FarmScene e da seção farm do save)
> **Parallel group:** fable_batch10_farm
> **Can run with:** F58, F59, F60 (locks disjuntos)
> **Must not run with:** F55 (mesmo gerador CreateMvpFarmScene + seção farm), F12, F41 (geradores)
> **Repo lock scope:** `CreateMvpFarmScene.cs`, Farm/Forage/**, Farm/Shipping/**, seção farm do save (campos aditivos), consumidores de DayStartedEvent
> **Depends on:**
> - F15 (executada — WorldWeatherService/estação viva; padrão de hospedar órfãos)
> - F13 (executada — padrão de seção de save)
> - F32 (E18 — itens de forrageio no catálogo)
> **Blocks:** N/A
> **Scope:** spawns diários de forrageio por estação na FarmScene + shipping bin físico com venda batch overnight, persistência da batch pendente e meta diária.
> **Out of scope:** mover o SellPoint (FARM_DESIGN §18 — futuro), encomendas de NPC, forrageio fora da FarmScene, ForageTable por bioma de caverna.

required_adrs: []
required_game_rules: [farm_rules.md, event_rules.md, save_rules.md]

---

# /speckit.specify

## Contexto

A auditoria de completude encontrou DOIS módulos de farm completos, testáveis e ÓRFÃOS —
a mesma classe de problema que a F15 corrigiu para clima/qualidade/refresh:

1. `Farm/Forage/` (`ForageDefinition`, `ForageSpawnState`, `FarmForageSpawnService`) —
   coleta com idempotência, estações permitidas (`AllowedSeasons`), respawn por política
   (`RespawnAfterDays`/`CanRespawnSameSeason`) e zonas proibidas (`zone_fonte`,
   `zone_cave_entrance`, `zone_city_exit`, `zone_lore_reserved`). Ninguém o instancia.
2. `Farm/Shipping/` (`PendingShippingEntry`, `ShippingBatch`, `ShippingPriceResolver`,
   `FarmShippingService`) — `Deposit()` com bloqueio de quest/key items,
   `ProcessDayBatch()` idempotente com `ChannelMultiplier 0.95` e pagamento em
   `ProcessOnDay = currentDay + 1`. Ninguém o instancia.

O gerador `CreateMvpFarmScene.cs` já tem `Zone_Forage` em (-8,-2) e
`Zone_ShippingSellpoint` em (3.5,7.5), e o comentário em ~linha 1367-1370 declara o
débito: os interactables de smoke são `TODO_INTEGRATION_NOT_FINAL` e "Final wiring must
connect to TreeChopService / RockMiningService / FarmForageSpawnService".

As directions são vinculantes: SEASONS_CALENDAR_WEATHER_LUNAR define `ForageTableId`
como campo da estação (forrageio MUDA por estação); FARM_DESIGN v1.3 fixa a "caixa de
envio com pagamento na manhã seguinte" no MVP agrícola e o shape `ShippingBinSaveData`
(§74). A F15 (executada) deixou estação/clima vivos via `WorldWeatherService` e
`DayStartedEvent` — os gatilhos de que esta spec precisa já existem.

## Problema

Hoje o forrageio é um único `ForageResource_01` de smoke que dá `item_herbs` para sempre,
sem estação, sem respawn e sem usar o serviço pago; e a venda é só o SellPoint imediato —
não existe o canal "deposita hoje, recebe amanhã de manhã" prometido pela direction (e o
multiplicador 0.95 que diferencia conveniência de preço). Se cada um for re-implementado
ad hoc, duplica sistemas existentes (violação de system-reuse-audit) e perde as regras já
codificadas (zonas proibidas, bloqueio de quest items, idempotência de batch).

## Objetivo

Ao final desta spec, a FarmScene deve ter N pontos de forrageio por dia, sorteados
deterministicamente por estação a partir de uma tabela de forrageio (ForageTable por
season), coletáveis via `FarmForageSpawnService` (respawn por política); e um shipping
bin físico (interactable) na `Zone_ShippingSellpoint` onde o jogador deposita itens que
são vendidos em batch na manhã seguinte via `FarmShippingService.ProcessDayBatch` +
`ShippingPriceResolver` (crédito via EconomyManager, feedback via evento) — com a batch
pendente persistida em campos aditivos da seção farm e o depósito contando para a meta
diária de colheita (bd_harvest). Nenhum dos módulos órfãos é reescrito — apenas
hospedado e ligado (padrão F15).

## Fontes obrigatórias lidas

```text
docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md (ForageTableId por estação)
docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md (§caixa de envio overnight, §74 ShippingBinSaveData, §18 SellPoint)
.specs/a_implementar/fable/fable_15_spec_world_weather_farm_orphan_systems_wiring.md (padrão de wiring)
.claude/rules/testing-quality-gate.md
.claude/skills/runtime-bootstrap-pattern/SKILL.md
.claude/skills/scene-interactable-wiring/SKILL.md
.claude/skills/save-load-pattern/SKILL.md
.claude/skills/rng-and-determinism/SKILL.md
```

## Estado atual do repo

```text
Existe, completo e NÃO recriar (apenas HOSPEDAR/LIGAR — padrão F15):
- Farm/Forage/{ForageDefinition, ForageSpawnState, FarmForageSpawnService}
  (Collect idempotente, AllowedSeasons, RespawnAfterDays, zonas proibidas);
- Farm/Shipping/{PendingShippingEntry, ShippingBatch, ShippingPriceResolver,
  FarmShippingService} (Deposit com sellability, ProcessDayBatch idempotente,
  ChannelMultiplier 0.95, ProcessOnDay = dia+1);
- Farm/Runtime/ShippingSummaryService (feedback de venda via
  EconomyTransactionCompletedEvent — REUSAR para o feedback do batch);
- CreateMvpFarmScene (Zone_Forage (-8,-2), Zone_ShippingSellpoint (3.5,7.5),
  ForageResource_01 smoke TODO_INTEGRATION_NOT_FINAL ~linha 1367-1420);
- DayStartedEvent / TimeManager; estação via GameCalendarService (F15);
- FarmDailyGoalService (metas diárias; bd_harvest);
- EconomyManager / InventoryManager; SaveManager + seção farm.
Não existe:
- instância dos serviços de Forage/Shipping; tabela de forrageio por estação;
- bin físico interactable; persistência de PendingShippingEntry;
- spawns diários determinísticos de forrageio.
Auditar Fase 0:
- IDs reais das metas diárias no FarmDailyGoalService (bd_harvest existe? qual GoalId);
- como a F15 hospedou FarmResourceRefreshProcessor (mesmo host? novo bootstrap?);
- catálogo F32: quais itens de forrageio existem por estação (mín. 2/estação;
  se faltar, registrar follow-up no report — NÃO expandir catálogo aqui);
- ShippingEntryId usa Guid.NewGuid (módulo órfão) — aceitável para entry de save?
  (não é conteúdo de caverna; documentar decisão no report).
```

## Engineering stories

```text
Como jogador, quero encontrar forrageáveis diferentes a cada estação na fazenda, para a
  estação ter presença além das crops.
Como jogador, quero depositar a colheita na caixa de envio e acordar com o ouro creditado,
  para vender sem viagem à cidade (aceitando 0.95 do preço).
Como sistema de save, quero a batch pendente persistida com tipos simples, para não perder
  uma venda depositada ao recarregar antes da manhã.
Como FarmDailyGoalService, quero que o depósito no bin conte para a meta de colheita,
  para o loop diário fechar (WI-24).
```

## Escopo

```text
Inclui:
- FORAGE: FarmForageRuntimeService (NOVO host bootstrap — padrão F15/runtime-bootstrap):
  em DayStartedEvent, resolve a ForageTable da estação atual (tabela em código/dados:
  season → lista de ForageDefinition) e ativa/respawna pontos de forrageio via
  FarmForageSpawnService.TryRespawn; seleção do dia DETERMINÍSTICA
  (StableHash(worldSeed, day) — sem Random/GUID/timestamp);
- gerador: 4-6 pontos de forrageio fixos na Zone_Forage (posições do gerador,
  referências serializadas), interactable de coleta chamando
  FarmForageSpawnService.Collect (substitui o reward fixo do smoke ForageResource_01;
  o smoke é REMOVIDO/CONVERTIDO — não coexistem dois caminhos);
- SHIPPING: bin físico interactable na Zone_ShippingSellpoint (gerador): interagir abre
  fluxo de depósito (item da mão/hotbar ou painel simples reutilizando fluxo de venda
  existente — auditar SellPanel) → FarmShippingService.Deposit;
- overnight: em DayStartedEvent, ProcessDayBatch(diaAtual) → crédito único via
  EconomyManager + EconomyTransactionCompletedEvent (ShippingSummaryService existente
  dá o feedback) + ShippingBatchProcessedEvent (NOVO — resumo p/ HUD/toast);
- meta diária: depósito no bin progride a meta de colheita (GoalId auditado na Fase 0);
- save: campos aditivos na seção farm — pendingShipping (lista de entries com tipos
  simples: ids/ints/floats) e estado dos pontos de forrageio do dia (forageSpawns);
  save legado carrega sem batch e sem spawns (regenera no próximo DayStarted);
- EditMode tests: determinismo da seleção por estação/dia, respawn por política,
  Deposit/ProcessDayBatch round-trip com save, idempotência do batch (processar 2× não
  paga 2×), zona proibida nunca recebe spawn, meta diária progride no depósito.
```

## Fora de escopo

```text
Não inclui:
- mover o SellPoint/bin (FARM_DESIGN §18 — spec futura de construção);
- encomendas simples de NPC (canal próprio — quest/board F34);
- forrageio na caverna ou na cidade;
- ForageTable data-driven por SO (tabela em código basta no v1 — documentar);
- arte final do bin/forrageáveis (placeholder do gerador);
- balanceamento fino de preços (ShippingPriceResolver existente é a régua).
```

## Regras de não duplicação

```text
NUNCA reescrever FarmForageSpawnService/FarmShippingService — hospedar e ligar (F15).
Não criar segundo canal de venda — bin usa ShippingPriceResolver/EconomyManager existentes.
Não criar segundo feedback de venda — ShippingSummaryService existente escuta a transação.
Não criar segundo padrão de interactable — seguir scene-interactable-wiring (gerador).
Não criar seção de save nova — campos ADITIVOS na seção farm existente.
Smoke ForageResource_01 com reward fixo NÃO coexiste com o caminho novo.
```

## Critérios de aceite

### CA-1 Forrageio sazonal determinístico

- Em DayStartedEvent, os pontos de forrageio do dia são derivados de
  StableHash(worldSeed, day) e da ForageTable da estação atual; mesmo dia + mesmo seed =
  mesmos spawns após reload; estação errada nunca spawna item fora da tabela.
- Evidência: EditMode tests (mesmo dia = mesma seleção; estações distintas = tabelas
  distintas; zona proibida vazia).

### CA-2 Coleta via serviço órfão com respawn

- Coletar usa FarmForageSpawnService.Collect (2ª coleta no mesmo spawn falha com
  ForageNotAvailable); respawn ocorre após RespawnAfterDays quando CanRespawnSameSeason.
- Evidência: EditMode tests de idempotência e respawn por política.

### CA-3 Shipping overnight idempotente

- Depositar no bin cria PendingShippingEntry (ProcessOnDay = dia+1); na manhã seguinte o
  batch credita ouro 1× (preço via ShippingPriceResolver, canal 0.95); reprocessar o
  mesmo dia não paga de novo; quest/key items são recusados no depósito.
- Evidência: EditMode tests de Deposit/ProcessDayBatch/idempotência/sellability.

### CA-4 Persistência da batch pendente

- Salvar com entries pendentes e recarregar preserva a batch; o pagamento acontece na
  manhã correta após o load; save legado (sem os campos) carrega sem erro e sem batch.
- Evidência: EditMode test de round-trip da seção farm estendida + seção legada.

### CA-5 Meta diária integrada

- Depositar colheita no bin progride a meta diária de colheita (GoalId auditado).
- Evidência: EditMode test com depósito sintético progredindo a meta.

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Farm/Forage/
  FarmForageRuntimeService.cs    (NOVO — host bootstrap; DayStarted → tabela da estação
                                  + seleção determinística + TryRespawn)
  ForageSeasonTable.cs           (NOVO — season → ForageDefinitions; dados em código v1)
Assets/_Game/Scripts/Farm/Shipping/
  ShippingBinRuntimeService.cs   (NOVO — host bootstrap; DayStarted → ProcessDayBatch →
                                  EconomyManager + eventos; dono da lista pendente)
  ShippingBinInteractable.cs     (NOVO — IInteractable; depósito via fluxo existente)
Assets/_Game/Scripts/Core/Events/
  ShippingBatchProcessedEvent.cs (NOVO — resumo do batch da manhã)
Assets/_Game/Scripts/Editor/SceneCreation/
  CreateMvpFarmScene.cs          (ATUALIZADO — pontos de forrageio reais + bin físico;
                                  remoção/conversão do smoke ForageResource_01)
seção farm do save               (campos aditivos: pendingShipping, forageSpawns)
Assets/_Game/Tests/EditMode/Farm/ForageShippingWiringTests.cs (NOVO)
docs/validation/fable_54_spec_forage_shipping_overnight_runtime_execution_report.md
```

## Contratos

### Data contracts

- `ForageSeasonTable`: seasonId → lista de forageIds (consome ForageDefinition
  existente; itens do catálogo F32 — auditados na Fase 0).
- Pontos de forrageio: IDs estáveis `farm_forage_01..06` (posições do gerador).
- Bin: `farm_shipping_bin_01` na Zone_ShippingSellpoint (3.5,7.5).

### Runtime contracts

- `FarmForageRuntimeService` (DayStarted): seleção do dia =
  StableHash(worldSeed, day) sobre a tabela da estação; chama TryRespawn/ativa spawns;
  nunca spawna em zona proibida (regra do serviço existente é a fonte).
- `ShippingBinRuntimeService` (DayStarted): ProcessDayBatch(diaAtual) ANTES de qualquer
  consumo de ouro do dia; crédito único via EconomyManager; publica
  ShippingBatchProcessedEvent + EconomyTransactionCompletedEvent (feedback existente).
- `ShippingBinInteractable.Deposit(itemId, qty)`: remove do inventário → Deposit no
  serviço; recusa de sellability devolve o item com feedback.
- Meta diária: depósito → FarmDailyGoalService (GoalId de colheita — Fase 0).

### Event contracts

- `ShippingBatchProcessedEvent` (NOVO — totalGold, itemCount; HUD/toast escutam).
- Consome: DayStartedEvent. Demais comunicações via eventos existentes (GameEventBus).

### Save contracts

- Campos ADITIVOS na seção farm: `pendingShipping` (lista — entryId, sellPointId,
  itemId, quantity, qualityTier, baseValueSnapshot, depositedDay, processOnDay, state)
  e `forageSpawns` (lista — spawnId, forageId, state, spawnedDay, nextEligibleSpawnDay).
  Somente tipos simples; sem referências Unity; save legado = listas vazias.

### UI contracts

- Prompt de interação do bin ("Depositar para envio") via InteractionPrompt existente;
  feedback de venda via ShippingSummaryService/toast existentes. Sem tela nova.

## Sistemas afetados

```text
Farm runtime (2 hosts novos; serviços órfãos passam a vivos)
Gerador da FarmScene (pontos de forrageio + bin; smoke removido)
Economia (crédito do batch via EconomyManager)
Save (campos aditivos na seção farm)
Metas diárias (progresso por depósito)
Event bus (ShippingBatchProcessedEvent)
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Farm/Forage/** (host novo + tabela; serviço existente INTOCADO)
Assets/_Game/Scripts/Farm/Shipping/** (host/interactable novos; serviço existente INTOCADO)
Assets/_Game/Scripts/Core/Events/ShippingBatchProcessedEvent.cs (novo)
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs (atualizar)
seção farm do save (campos aditivos — owner auditado na Fase 0)
FarmDailyGoalService (hook de progresso — aditivo)
Assets/_Game/Tests/EditMode/Farm/** ; docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab / *.asset por edição manual de YAML (cena SÓ via gerador)
Packages/** ; ProjectSettings/**
FarmForageSpawnService.cs / FarmShippingService.cs / ShippingPriceResolver.cs
  (módulos órfãos — wrapper fino apenas; correção real exige diff mínimo documentado)
SaveManager core (apenas campos aditivos na seção farm)
CreateMvpTownScene / geradores de outras cenas
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
GoalIds reais das metas diárias; host da F15 (padrão a seguir); itens de forrageio
disponíveis no catálogo F32 por estação; fluxo de venda existente (SellPanel) para o
depósito; shape exato da seção farm do save.

### Fase 1 — Forrageio
ForageSeasonTable + FarmForageRuntimeService (DayStarted, StableHash, TryRespawn) +
pontos no gerador (refs serializadas) + remoção do smoke + testes de determinismo,
idempotência e respawn.

### Fase 2 — Shipping bin
ShippingBinRuntimeService + ShippingBinInteractable + bin no gerador +
ShippingBatchProcessedEvent + crédito via EconomyManager + testes de
Deposit/ProcessDayBatch/idempotência/sellability.

### Fase 3 — Persistência e meta diária
Campos aditivos (pendingShipping, forageSpawns) + round-trip + seção legada + hook da
meta de colheita + testes.

### Fase 4 — Fechamento
Regeneração da FarmScene com evidência; csproj; run_strict_validation; execution report.
```

## Paralelização

- Parallelizable: CONDITIONAL.
- Parallel group: fable_batch10_farm.
- Can run with: F58, F59, F60 (Audio/Telemetry/Cave — locks disjuntos).
- Must not run with: F55 (mesmo CreateMvpFarmScene e seção farm), F12, F41 (geradores).
- Shared files/systems that require lock: CreateMvpFarmScene.cs, seção farm do save,
  consumidores de DayStartedEvent.
- Reason: edita o gerador compartilhado da fazenda e estende a mesma seção de save que
  F55/F41 tocam — ordem serial dentro do grupo farm.

## Impacto em save/load

```text
Does this change save schema? YES (campos aditivos na seção farm: pendingShipping,
forageSpawns)
Does this add a save section? NO
Does this require migration? NO (campos ausentes = listas vazias; batch/spawns
regeneram no próximo DayStarted)
Does this persist Unity references? NO (IDs/ints/floats/strings)
Round-trip obrigatório: depositar → save → load → manhã seguinte paga 1×.
```

## Impacto em eventos

```text
Adds events: YES — ShippingBatchProcessedEvent
Changes existing events: NO
Requires unsubscribe pattern: YES (2 hosts assinam DayStartedEvent)
```

## Impacto em UI/Unity

```text
Changes UI: prompt do bin + feedback existente (sem tela nova)
Changes scenes: YES — FarmScene regenerada VIA GERADOR (evidência obrigatória)
Changes prefabs: NO
Changes ScriptableObjects/assets: NO (tabela em código no v1)
Requires Play Mode final validation: YES (lote final)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: API dos módulos órfãos incompatível com o runtime atual (escritos às cegas).
Mitigação: Fase 0 audita assinaturas; adaptar via wrapper fino SEM tocar no módulo;
correção real = diff mínimo documentado no report (padrão F15).

Risco: batch paga 2× (DayStarted duplicado, reload na manhã).
Mitigação: idempotência já existe no serviço (IsProcessed) + estado persistido +
teste de reprocessamento.

Risco: spawns driftando entre loads (Random/GUID).
Mitigação: StableHash(worldSeed, day) — regra rng-and-determinism; coberto por teste.

Risco: depósito perder item em recusa de sellability.
Mitigação: remover do inventário SÓ após Deposit Success; teste de recusa devolve.

Risco: itens de forrageio insuficientes no catálogo por estação.
Mitigação: Fase 0 audita; mínimo 2/estação ou follow-up documentado (sem expandir
catálogo nesta spec).

Risco: dois caminhos de coleta coexistindo (smoke + serviço).
Mitigação: smoke ForageResource_01 removido/convertido na mesma fase; anti-regressão.
```

## Rollback

```text
Remover os 2 hosts + interactable e regenerar a cena = forrageio/bin voltam a órfãos
(estado atual); campos aditivos de save são inofensivos vazios. Não apagar save real
do usuário.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar GoalIds, host F15, itens F32 por estação, fluxo de venda, seção farm.
- [ ] T002 — ForageSeasonTable + FarmForageRuntimeService + pontos no gerador + remoção
        do smoke + testes (determinismo/idempotência/respawn/zona proibida).
- [ ] T003 — ShippingBinRuntimeService + ShippingBinInteractable + bin no gerador +
        ShippingBatchProcessedEvent + crédito + testes (batch/idempotência/sellability).
- [ ] T004 — Save aditivo (pendingShipping/forageSpawns) + round-trip + legado + meta
        diária + testes.
- [ ] T005 — Regeneração com evidência; csproj; run_strict_validation; execution report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (seleção por seed/dia, batch overnight, respawn, save)
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES (lote final)
- Requires regression test: YES (coleta de recursos/venda imediata existentes intactas)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano coletando forrageio
  sazonal e recebendo pagamento do bin na manhã seguinte

## Definition of Done

```text
Forrageio sazonal vivo (tabela por estação, seleção determinística, respawn por política,
zonas proibidas respeitadas); shipping bin físico com venda batch overnight idempotente
(0.95, crédito único, feedback); batch pendente e spawns persistidos por campos aditivos
(round-trip + legado); depósito progride meta diária; zero reescrita dos módulos órfãos;
smoke substituído; builds 0E; run_strict_validation exit 0; execution report criado.
```

## Anti-regressão

```text
FarmForageSpawnService/FarmShippingService/ShippingPriceResolver sem mudança de contrato.
Venda imediata existente (SellPoint) intacta — bin é canal ADICIONAL.
DayStarted continua disparando daily goals/shop restock/clima (F15) sem regressão.
Zonas proibidas (fonte/caverna/saída/lore) jamais recebem spawn de forrageio.
Save legado carrega sem erro (campos ausentes = vazios); só tipos simples no DTO.
Zero GameObject.Find em runtime (refs serializadas pelo gerador); eventos só via GameEventBus.
```
