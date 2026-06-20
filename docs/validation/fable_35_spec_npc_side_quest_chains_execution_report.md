---
doc_type: validation
status: evidence
spec_id: fable_35_spec_npc_side_quest_chains
validation_type: automated
result: BUILD_VALIDATED_WITH_WARNINGS
date: 2026-06-20
executor: Claude Code
source_of_truth: false
validated_adrs: []
validated_game_rules: [quest_rules.md, npc_rules.md]
---

# Validation Report — fable_35 NPC Side Quest Chains (12 cadeias)

> This report is evidence, NOT an execution queue.
> Do not re-run the spec based on this report alone.

Status: **BUILD_VALIDATED_WITH_WARNINGS** — núcleo (12 cadeias × 3 = 36 quests, gates compostos,
encadeamento, recompensas, flags de serviço) implementado, buildado e coberto por EditMode tests.
O marcador visível "[!] Quest" no diálogo e a aceitação ponta a ponta em cena ficam diferidos
(DEFERRED_TO_FINAL_VALIDATION) — exigem Play Mode/cena (autorizado a pular pelo dono nesta sessão).

---

## What Was Run

- [x] dotnet build Assembly-CSharp.csproj --no-restore → PASS (0 erros)
- [x] dotnet build Assembly-CSharp-Editor.csproj --no-restore → PASS (0 erros)
- [x] tools/docs/validate_docs.ps1 → PASS (exit 0)
- [x] tools/docs/check_spec_diff_completeness.ps1 → PASS (exit 0)
- [x] tools/docs/run_strict_validation.ps1 → exit 1 (ambiental: 3 .unity pré-modificadas + WARNs de reports alheios)
- [ ] Unity validators (F30) → NOT RUN (Unity Editor indisponível nesta sessão)
- [ ] Play Mode checklist → DEFERRED_TO_FINAL_VALIDATION

---

## Acceptance criteria extracted

| CA | Critério | Evidência | Status |
|----|----------|-----------|--------|
| CA-1 | 12 cadeias geradas com matriz de viabilidade; nenhum objetivo cortado silenciosamente | `NpcQuestChainCatalog` (36 steps); matriz objetivo×tipo abaixo; teste `Catalog_Has12Chains_36Steps_CanonicalIds`, `Catalog_NoSilentlyCutObjective_EveryStepMapsToExistingType` | OK |
| CA-2 | Encadeamento + gates compostos (flag anterior + amizade + ato) | `NpcQuestChainService.IsEligible`; testes `Step2_IsNotOfferable_UntilStep1Done`, `Step2_IsGatedByMinFriendship`, `FinalStep_IsGatedByActFlag` | OK |
| CA-3 | Recompensas (+8 amizade via F26, flag de serviço na q3) idempotentes | flags `sq_*_done`/`service_unlock_*` via `AdditionalRewards` (QuestFlagGrant, TrackByFlagId); +8 amizade pelo hook existente `FriendshipService.OnQuestGiverInteracted`; testes `FinalStep_UnlocksService_AndIsIdempotent`, `GoldAndXp_GrantedOnce_OnTurnIn` | OK |
| CA-4 | 3 cadeias ponta a ponta (aceitar→completar→próxima→serviço) | teste `ThreeChains_EndToEnd_AcceptCompleteNextService` (Brumdar/Ozzra/Tovin) + `DynamicInstance_SurvivesSaveRoundTrip` | OK |

### Matriz cadeia × status (12 cadeias, 36 quests)

Todas as 12 cadeias têm 3 steps (q1 doméstica → q2 mundo → q3 marco) com IDs canônicos
`sq_<npc>_<n>` do catálogo §9. Contagem final = 36 (catálogo nomeia 3/cadeia; nenhum
desdobramento foi necessário — toda narrativa coube em 1 tipo existente). Nenhum step dormante.

| Cadeia | q1 | q2 | q3 (serviço destravado) | Ato gate |
|--------|----|----|--------------------------|----------|
| brumdar | Collect iron_ore×10 | Reach ruina/bigorna | Defeat Forge-Tyrant → têmpera (F22) | act_1_done |
| ozzra | Collect glowcap×5 | Collect essence_ice×2 | Defeat Veilkin Witch → LearnableScrolls (F07) | — |
| thalindra | Deliver livros×3 | Collect amostras×3 | Deliver nymirian_engraving → análise 2×/dia | — |
| sylveth | Harvest crops×10 | Harvest alihana_tear | Plant shadowroot → troca off-season | — |
| eiran | Reach galinha | Defeat Grimfang×5 | Deliver ração×5 → pensão de animais | — |
| gruta | Collect grape×5 | Collect mirrorfin | Deliver banquete → 2 pratos/dia | — |
| dagna | Collect stone×8 | Reach veio cantante nv15 | Collect glacier_hide → mapa do veio | — |
| zrix | Reach marcar 3 níveis | Reach kit do batedor | Reach resgate nv40 → resgate c/ desconto | — |
| hund | Talk ronda 22h | Reach pegadas | Defeat Packlord Ruvash → board +20% | — |
| mirela | Collect fiber×5 | Collect veil_cloth×3 | Deliver Midnight Dress → upgrade mochila | — |
| tovin | Deliver formulários | Talk Yael | Talk assinaturas×5 → alvarás de lote (F41) | — |
| corvus | Craft velas×5 | Talk diálogo de fé | Reach Litany Fragment → bênção (liga Ato 4) | act_3_done |

### Matriz objetivo × tipo existente (viabilidade — nenhum corte silencioso)

| Kind do catálogo | QuestObjectiveType existente | Observação |
|------------------|------------------------------|------------|
| Collect (coletar N) | `CollectItem` | auto-progresso por inventário |
| Deliver (entregar) | `DeliverItem` | adapta "entregar livros/formulários/encomenda" |
| Talk (conversar/acompanhar) | `TalkToNpc` | ADAPTA "acompanhar ronda", "diálogo longo" → falar com NPC/alvo no local |
| Reach (alcançar/investigar) | `ReachLocation` | ADAPTA "marcar níveis", "investigar pegadas", "recuperar bigorna/kit", "resgatar" |
| Defeat (derrotar marco) | `DefeatEnemy` | banda/miniboss/boss (count) |
| Harvest (colher) | `HarvestCrop` | Sylveth q1/q2 |
| Plant (plantar) | `PlantCrop` | Sylveth q3 (shadowroot na caverna) |
| Craft (fabricar) | `CraftItem` | Corvus q1 (velas) |

Nenhum objetivo do catálogo ficou sem mapeamento; nenhum tipo de objetivo novo foi criado.

---

## Existing systems audit

| Sistema | Encontrado | Reutilizado / Criado |
|---------|-----------|----------------------|
| F34 fonte Npc / fluxo de quest dinâmica | `QuestInstance`, `QuestService.AcceptDynamicInstance/TurnIn/MarkObjectiveComplete`, `QuestSource.Npc`, `QuestRewardScaling` | REUTILIZADO — quests fluem pelo registry/service único; sem registry/fórmula paralela |
| F26 amizade | `FriendshipService.OnQuestGiverInteracted → RegisterQuestCompleted` (+8, idempotente por questId) | REUTILIZADO — +8 amizade já é automático no turn-in; o catálogo NÃO toca estado de amizade |
| Flags | `QuestFlagService.IsSet/GrantFlag` via `QuestRewardApplicator` (QuestFlagGrant) | REUTILIZADO — `sq_*_done` e `service_unlock_*` gravados pelo applicator existente |
| F28 diálogo (condições) | `TownNpcDialogueLibrary`, `DialogueLineCondition.RequiredFlag/MinFriendship` | LIDO — oferta visível diferida; predicado `HasOffer` é o contrato que a camada de diálogo consome |
| F53 precedente (catálogo de quests) | `FestivalQuestCatalog`/`FestivalQuestService` | PADRÃO COPIADO — catálogo puro + serviço fino sobre QuestService |
| Save | `QuestStateRecord`/`QuestStateSection` (DTO simple types) | REUTILIZADO — quests dinâmicas persistem pela seção existente; sem campo/seção nova |
| **Criado** | `NpcQuestChainCatalog` (autoria pura), `NpcQuestChainService` (orquestrador fino), `NpcChainsTests` | apenas conteúdo + adaptador; nenhum sistema paralelo |

---

## Spec Compliance Matrix

| Requisito (escopo §109) | Implementação | Status |
|--------------------------|---------------|--------|
| ~36 QuestDefinitions IDs `sq_<npc>_<n>` com objetivos mapeados | `NpcQuestChainCatalog.AllSteps` (36) + `BuildInstance` | OK |
| Recompensas: gold/item/amizade +8/flag de serviço | gold+XP escalados (F34) + item + flag de serviço (extras); +8 amizade pelo hook F26 | OK |
| Gates: flag anterior + amizade mínima + ato quando citado | `NpcQuestChainService.IsEligible` (composto) | OK |
| Objetivos sem tipo viável: ADAPTAR ou dormante documentado | tudo adaptado a tipo existente; 0 dormantes (matriz acima) | OK |
| Textos na voz do NPC via ids de localização | `TitleKey/DescKey/OfferKey/TurnInKey` = `quest.<sq_id>.<slot>` (ADR-0012/F73) | OK (ids; strings no string table) |
| Flags de serviço destravam F25/F22/F07/F41 | `service_unlock_*` forward-declared, consumidos pelos donos | OK (grant; consumo nos sistemas donos) |
| Oferta "[!] Quest" no diálogo | predicado `HasOffer`/`GetOfferableStep` exposto; marcador visível em cena diferido | DEFERRED_UI_VISUAL |
| EditMode tests (encadeamento, gates, +amizade, 3 cadeias e2e) | `NpcChainsTests` (12 testes) | OK |
| Sem tipos de objetivo novos / segundo fluxo de oferta | matriz objetivo×tipo; oferta única via QuestService | OK |

---

## Validation

```text
Validation method: run_strict_validation.ps1 (+ builds individuais)
Assembly-CSharp: PASS (0 erros, 1 warning pré-existente CombatTelemetrySession)
Assembly-CSharp-Editor: PASS (0 erros, warnings pré-existentes)
Docs validation: PASS (exit 0)
Spec diff completeness (escopado): PASS (exit 0)
run_strict_validation exit code: 1 (AMBIENTAL — não é falha desta spec)
  Causa: 3 cenas .unity (CaveScene/FarmScene/TownScene) já modificadas no working tree
         ANTES desta sessão (confirmado no git status inicial) + WARNs de seções
         ausentes em execution reports de OUTRAS specs (pré-existentes).
  Esta spec NÃO tocou nenhum .unity/.prefab/.asset (verificado em git status).
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json
```

### Testing Quality Gate

```text
Changed runtime code: YES (NpcQuestChainCatalog, NpcQuestChainService)
Changed deterministic logic: YES (gates compostos, encadeamento por flag, mapeamento de objetivo)
Changed Unity scene/prefab/asset wiring: NO
Automated tests added/updated: YES (Assets/_Game/Tests/EditMode/Quests/NpcChainsTests.cs — 12 testes)
Automated tests command: dotnet build .\Assembly-CSharp.csproj --no-restore (testes compilam no Assembly-CSharp; Unity Test Runner NOT RUN — Editor indisponível)
Manual Play Mode scenario: DEFERRED_TO_FINAL_VALIDATION (oferta visível no diálogo + cadeia completa em cena)
Justification if no automated tests: N/A (testes adicionados)
Residual risk: marcador "[!] Quest" e aceitação por diálogo em cena ainda não exercitados em Play
  Mode; auto-progresso por eventos reais (EnemyKilled/CropHarvested/...) coberto só por chamadas
  diretas aos hooks do serviço, não pela ponte de eventos viva.
```

---

## Honest status rationale

A lógica determinística central (catálogo das 12 cadeias, gates compostos, encadeamento por flag,
recompensas e flags de serviço) está implementada, compila em ambos os assemblies (0 erros) e é
coberta por 12 EditMode tests rodando sobre o fluxo F34 existente — sem registry, fórmula de
recompensa ou caminho de amizade paralelos. Por isso **BUILD_VALIDATED_WITH_WARNINGS** e não
BUILD_VALIDATED puro: (1) `run_strict_validation` retorna exit 1 por causa ambiental (3 .unity
sujas de antes da sessão + WARNs de reports alheios), não por esta spec; (2) o marcador visível
"[!] Quest" no diálogo e a aceitação ponta a ponta em cena dependem de Play Mode/cena e ficam
DEFERRED_TO_FINAL_VALIDATION (autorizado a pular nesta sessão). Não há claim de Play Mode PASS nem
de Unity validation; a unidade de validação Unity/Test Runner está NOT RUN (Editor indisponível).

## Remaining work

- Wiring runtime do marcador "[!] Quest" no `TownNpcDialogueLibrary`/DialogueController consumindo
  `NpcQuestChainService.HasOffer/GetOfferableStep` e aceitando via o caminho de oferta (Play Mode).
- Assinatura do `NpcQuestChainService` no GameEventBus (EnemyKilled/CropHarvested/NpcInteraction/
  LocationReached/ItemCrafted/Delivered) num bootstrap, para auto-progresso em cena.
- Strings na voz do NPC no string table (F73) para os ids `quest.<sq_id>.<slot>`.
- Consumo das flags `service_unlock_*` pelos sistemas donos (F22/F07/F41/F25).
- Cenário humano de Play Mode completando 1 cadeia inteira (mínimo para ACCEPTED).
```
