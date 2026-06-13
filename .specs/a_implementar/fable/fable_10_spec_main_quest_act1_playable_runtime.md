# SPEC — Main Quest: Ato 1 Jogável — "A Fonte do Esquecimento" (Fragmento da Água)

> **Spec ID:** `fable_10_spec_main_quest_act1_playable_runtime`
> **Status:** A implementar
> **Wave:** FABLE — Gap Closure Bloco E (conteúdo)
> **Priority:** P1
> **Type:** Runtime / Data / Integration
> **Domain:** Quest
> **Parallelizable:** CONDITIONAL
> **Parallel group:** fable_bloco_E
> **Can run with:** fable_01, fable_03, fable_09, fable_11, fable_12
> **Must not run with:** specs que alterem QuestRegistry/QuestService simultaneamente
> **Repo lock scope:** `Assets/_Game/Scripts/Quests/Runtime/**`, dialogue trees de Corvus/Thalindra/Maelor
> **Depends on:**
> - WAVE 09/WI-15/WI-26 (quest system + 7 tipos de objetivo)
> - WAVE 10 (MainProgression/FonteAnya state)
> - slice 2026-06-12 (TownNpcDialogueLibrary)
> **Blocks:** Atos 2-4 (promoção das specs WAVE 19 futuras)
> **Scope:** questline do Ato 1 (5 quests encadeadas) culminando no Fragmento da Água e 1º desbloqueio da Fonte.
> **Out of scope:** Atos 2-4, finais, cutscenes, novos sistemas de quest, level 100/101.

required_adrs: []
required_game_rules: [quest_rules.md, fonte_rules.md]

---

# /speckit.specify

## Contexto

`QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md` define 4 atos com fragmentos
(Água/Memória/Vida/Esperança); `QUESTS_MAIN_LORE_DIRECTION.md` define o elenco (Cindar,
Corvus, Vaelrion, Sethra, Arquivista) e a regra canônica: Anya não é restaurável — o jogador
protege/sela/usa fragmentos. A WAVE 10 implementou TODO o estado (atos, fragmentos,
FonteAnya functions: respawn → Água Viva → respec → purificação) e a WAVE 09/WI-26 deixou o
quest system com 7 tipos de objetivo prontos (CollectItem, SellItem, ReachCaveDepth,
HarvestCrop, TalkToNpc, DefeatEnemy, CraftItem). Mas o `QuestRegistry` tem só 3 quests
utilitárias — a main quest não existe como conteúdo jogável.

## Problema

O jogo tem motor narrativo sem narrativa: a Fonte (hub central do design) nunca evolui em
gameplay real, o jogador não tem razão de longo prazo para descer a caverna, e os hooks de
fragmento da WAVE 10 nunca disparam. Sem o Ato 1, as specs futuras dos Atos 2-4 não têm
baseline de integração validado.

## Objetivo

Ao final desta spec, o `QuestRegistry` deve conter a questline do Ato 1 (5 quests encadeadas
por PrerequisiteQuestIds), distribuída entre Corvus (templo), Thalindra (arquivo) e Maelor
(pistas noturnas), culminando em `ReachCaveDepth` nível 10 + `DefeatEnemy` (miniboss do gate
10) + entrega que chama `MainProgressionService` (WAVE 10, nome real auditado na Fase 0)
para conceder o Fragmento da Água e desbloquear Água Viva na Fonte — com diálogos de oferta
integrados ao padrão `QuestGiverInteractable`/árvore existente.

## Fontes obrigatórias lidas

```text
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
docs/design/gameplay/quests/QUESTS_LORE_WEAVING_BRIEF.md
docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md
.claude/rules/testing-quality-gate.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- QuestRegistry/QuestService/QuestRuntimeBootstrap/QuestProgressEventBridge (WI-15/26)
- QuestGiverInteractable (npc_thalindra wired; PrerequisiteQuestIds existe mas NÃO é
  enforced no giver — PREREQUISITE_UI_DEBT documentado em WI-26)
- QuestStateSection save (WI-18, idempotência por GrantedRewardIds)
- WAVE 10: acts/fragments/Fonte state + eventos (auditar nomes exatos:
  Assets/_Game/Scripts/MainProgression/** e Fonte/**)
- CaveBossGateRegistry (gate nível 10 existe? auditar CreateCaveBossAssets)
- TownNpcDialogueLibrary + NpcController OfferQuest action (DialogueActionType.OfferQuest)
Não existe:
- quests do Ato 1; enforcement de prerequisite no giver; ponte quest→MainProgression
```

## Engineering stories

```text
Como jogador novo, quero que Corvus me apresente a Fonte adormecida e me mande investigar.
Como jogador, quero uma cadeia clara: investigar → preparar → descer → vencer o guardião → entregar.
Como Fonte, quero receber o Fragmento da Água via quest e desbloquear Água Viva.
Como quest obrigatória, não posso expirar nem softlockar (regra canônica).
```

## Escopo

```text
Inclui:
- 5 quests no QuestRegistry (IDs estáveis):
  mq_act1_01_fonte_adormecida   — TalkToNpc Corvus→Thalindra (introdução; reward: flag);
  mq_act1_02_registros_perdidos — CollectItem item_material_stone x5 + TalkToNpc Maelor
                                  (Thalindra precisa de material p/ restaurar registros);
  mq_act1_03_eco_da_agua        — ReachCaveDepth 5 (sentir o eco; Maelor dá a pista);
  mq_act1_04_guardiao_da_agua   — ReachCaveDepth 10 + DefeatEnemy <bossId do gate 10>;
  mq_act1_05_fragmento_da_agua  — TalkToNpc Corvus (entrega; reward: fragmento via WAVE 10 API
                                  + 150 gold + flag mq_act1_complete);
- enforcement de PrerequisiteQuestIds no QuestGiverInteractable (fecha PREREQUISITE_UI_DEBT);
- MainProgressionQuestBridge (NOVO): consome QuestCompletedEvent → chama API WAVE 10 de
  fragmento (idempotente — fragmento 1x por GrantedRewardIds);
- QuestGiverInteractable em npc_corvus e npc_maelor via CreateMvpTownScene (fecha
  SCENE_WIRING_DEBT de WI-26 para esses NPCs);
- nós de diálogo de oferta (OfferQuest) adicionados ao TownNpcDialogueLibrary para
  Corvus/Maelor (padrão Thalindra);
- textos PT-BR alinhados à lore (Litania do Primeiro Retorno citada por Corvus);
- EditMode tests: cadeia de prerequisites, idempotência do fragmento, anti-softlock
  (main quest sem expiry), save round-trip da questline.
```

## Fora de escopo

```text
Não inclui: Atos 2-4; finais Proteger/Selar/Usar; cinemática; Vaelrion/Sethra como NPCs
novos em cena; UI de quest nova (QuestLogPanel atual exibe); balance de recompensas.
```

## Regras de não duplicação

```text
Não criar segundo registry/serviço de quest.
Não criar sistema de progressão paralelo — usar API WAVE 10 existente.
Não criar novo tipo de objetivo — os 7 existentes cobrem tudo.
Diálogos de oferta seguem o padrão Thalindra (runtime tree/OfferQuest), sem novo fluxo.
```

## Critérios de aceite

### CA-1 Cadeia jogável e travada
- Quest N+1 só é ofertada com N completa (prerequisite enforced no giver).
- Evidência: testes de enforcement + matriz de estados.

### CA-2 Fragmento concedido 1x
- Completar mq_act1_05 concede Fragmento da Água e desbloqueia Água Viva (estado WAVE 10);
  reload + re-turn-in NÃO duplica. Evidência: teste de idempotência via GrantedRewardIds.

### CA-3 Anti-softlock
- Nenhuma quest do ato expira; morrer/novo run de caverna não invalida progresso de
  ReachCaveDepth já registrado. Evidência: testes de persistência de objetivo.

### CA-4 Save round-trip
- Questline inteira sobrevive a save/load em qualquer ponto da cadeia (padrão WI-18).

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Quests/Runtime/QuestRegistry.cs            (+5 quests)
Assets/_Game/Scripts/Quests/Runtime/QuestGiverInteractable.cs   (prerequisite enforcement)
Assets/_Game/Scripts/Quests/Runtime/MainProgressionQuestBridge.cs (NOVO)
Assets/_Game/Scripts/NPC/TownNpcDialogueLibrary.cs              (nós OfferQuest Corvus/Maelor)
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs (QuestGiverInteractable nos 2 NPCs)
Assets/_Game/Tests/EditMode/Quests/MainQuestAct1Tests.cs
```

## Contratos

### Data contracts — quests em código no registry (padrão atual; sem assets novos).
### Runtime contracts — bridge consome QuestCompletedEvent (auditar nome exato WI-15).
### Event contracts — nenhum novo (reusa quest events + WAVE 10 events).
### Save contracts — N/A novo (QuestStateSection cobre; flags via QuestFlagService).
### UI contracts — QuestOfferPanel/QuestLogPanel existentes exibem.

## Sistemas afetados

```text
Quests, MainProgression/Fonte (WAVE 10), NPC dialogue, TownScene generator, Save (conteúdo, não schema)
```

## Arquivos permitidos

```text
Arquivos da arquitetura + docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity/*.prefab/*.asset; Packages/ProjectSettings; SaveManager/GameSaveData (schema);
QuestService.cs core (somente se enforcement exigir hook mínimo — justificar no report)
```

## Estratégia de implementação

```md
### Fase 0 — Auditar API WAVE 10 (nomes reais), bossId do gate 10, QuestCompletedEvent.
### Fase 1 — 5 quests no registry + testes de cadeia.
### Fase 2 — Prerequisite enforcement no giver (fecha debt WI-26) + testes.
### Fase 3 — Bridge de fragmento idempotente + diálogos OfferQuest + gerador de cena.
### Fase 4 — Round-trip/anti-softlock tests + validação estrita + report.
```

## Paralelização

- Parallelizable: CONDITIONAL
- Must not run with: specs tocando Quests/Runtime
- Reason: registry/giver compartilhados.

## Impacto em save/load

```text
Does this change save schema? NO (conteúdo novo na seção existente)
```

## Impacto em eventos

```text
Adds events: NO | Changes existing: NO | Requires unsubscribe: YES (bridge)
```

## Impacto em UI/Unity

```text
Changes UI: NO | Changes scenes: via gerador (humano regenera TownScene) | Prefabs: NO
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: gate 10 sem bossId estável. Mitigação: Fase 0 confirma; fallback DefeatEnemy de
qualquer inimigo no nível 10 com count 5 (documentado como TEMP até gate confirmado).
Risco: enforcement de prerequisite quebrar Thalindra Q1. Mitigação: quests sem prerequisites
continuam sempre ofertáveis (teste de regressão).
```

## Rollback

```text
Remover 5 quests/bridge/nós de diálogo; enforcement atrás de checagem de lista vazia (inerte
para quests existentes).
```

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar WAVE 10 API, gate 10, eventos de quest.
- [ ] T002 — Registrar 5 quests com prerequisites + testes de cadeia.
- [ ] T003 — Prerequisite enforcement no QuestGiverInteractable + regressão Thalindra.
- [ ] T004 — MainProgressionQuestBridge idempotente + testes.
- [ ] T005 — Nós OfferQuest (Corvus/Maelor) + gerador TownScene.
- [ ] T006 — Round-trip/anti-softlock; csproj; run_strict_validation; report.
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
- Requires PlayMode automated or final human scenario: YES (questline ponta a ponta)
- Requires regression test: YES (3 quests existentes intactas)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano completando o Ato 1

## Definition of Done

```text
Ato 1 jogável ponta a ponta com fragmento idempotente, prerequisites enforced, diálogos
integrados; builds 0E; report criado.
```

## Anti-regressão

```text
quest_first_supplies_for_cindar / tools_for_the_town / echo_from_the_cave intactas.
Main quest nunca expira. Fonte não libera função sem fragmento (canon).
Reward idempotente via GrantedRewardIds.
```

## Notas para execução posterior

```text
Atos 2-4 + finais: promover specs WAVE 19 futuras usando esta baseline.
Vaelrion/Sethra entram como NPCs no Ato 2.
```
