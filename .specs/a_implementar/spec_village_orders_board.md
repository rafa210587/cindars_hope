# SPEC — Quadro de Encomendas da Vila (demanda: a vila pede, você fornece)

> **Spec ID:** `spec_village_orders_board`
> **Status:** A implementar
> **Wave:** WAVE VILLAGE ECONOMY — slice 1/5 (demanda)
> **Priority:** P1
> **Type:** Runtime
> **Domain:** Quest
> **Parallelizable:** CONDITIONAL
> **Parallel group:** village_economy
> **Can run with:** specs que não tocam Quests/Runtime, Economy nem Save providers
> **Must not run with:** specs que alterem QuestService/QuestBoardService/QuestRewardApplicator ou o save de quest
> **Repo lock scope:** `Assets/_Game/Scripts/Quests/**`, `Assets/_Game/Scripts/Core/Events/Village*`
> **Depende de (Depends on):**
> - Quest infra (QuestService, QuestRegistry, QuestInstance, QuestBoardService, QuestRewardApplicator) — já existe (fable_34)
> - Economy (PlayerManager.AddGold), Inventory (InventoryManager.RemoveItem), ItemDatabaseSO — já existem
> **Bloqueia (Blocks):**
> - `spec_village_reputation` (consome VillageOrderFulfilledEvent) — slice 2
> - `spec_village_supplier_chains`, `spec_town_development` — slices 3-4
> **Scope:** Um quadro de encomendas onde a vila publica pedidos rotativos de itens (colheita/minério/material/manufatura); o jogador entrega e recebe ouro + uma "contribuição" (evento p/ reputação futura), reutilizando 100% o pipeline de quest existente.
> **Out of scope:** Sistema de reputação da vila (slice 2), cadeias de fornecedor (slice 3), desenvolvimento da vila (slice 4), simulação de consumo de NPC (slice 5), UI rica nova (reusa a UI de quest/board existente).

required_adrs: []
required_game_rules: []

---

# /speckit.specify

## 5. Contexto

A cidade de Cindar's Hope tem ~24 NPCs com papéis ricos (ofício, comércio, governo, fé), mas hoje
ela só **serve** o jogador (lojas = sinks de ouro). A vila nunca **pede** nada — a produção do
jogador (fazenda/caverna) não tem demanda além de vender no balcão. Decisão do usuário: tornar a vila
**dependente do jogador** (demanda), depois **crescer com ele** e ter um **mínimo de simulação**. Este
é o **slice 1 (demanda)** da mini-wave de economia da vila; abre a porta para reputação/desenvolvimento.

A auditoria de Fase 0 confirmou: o sistema de quest já cobre todo o ciclo (gerar contrato determinístico
por dia, aceitar instância dinâmica, objetivo CollectItem, turn-in com recompensa idempotente de ouro).
O Quadro de Encomendas é uma **variante de fonte** (`QuestSource.Board`) — não um sistema paralelo.

## 6. Problema

Sem demanda, o loop econômico é raso: o jogador acumula colheita/minério sem motivo de gameplay além de
vender. A vila parece um conjunto de lojas, não um lugar que precisa dele. Criar um segundo sistema de
"contratos" paralelo ao QuestService seria duplicação proibida (system-reuse-audit / runtime-code-guard).

## 7. Objetivo

Ao final desta spec, a vila publica **encomendas rotativas** num quadro; o jogador aceita, entrega itens
e recebe **ouro** (pelo applicator de quest existente) e a vila emite um **VillageOrderFulfilledEvent**
com pontos de contribuição (para a reputação futura consumir), **sem** criar pipeline de quest paralelo,
sem alterar save schema de forma incompatível, e sem novo sistema de reputação (slice 2).

## 8. Fontes obrigatórias lidas

```text
docs/project/CURRENT_STATE.md
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
.claude/rules/testing-quality-gate.md
.claude/rules/unity-architecture.md
.claude/skills/quest-authoring/SKILL.md
.claude/skills/economy-balance-tuning/SKILL.md
.claude/skills/rng-and-determinism/SKILL.md
Assets/_Game/Scripts/Quests/** (QuestService, QuestRegistry, QuestInstance, QuestBoardService, QuestRewardApplicator) — auditar
Assets/_Game/Scripts/Quests/Runtime/QuestBoardInteractable.cs — auditar
Assets/_Game/Scripts/Player/PlayerManager.cs (AddGold) — auditar
Assets/_Game/Scripts/Inventory/InventoryManager.cs (RemoveItem/HasItem) — auditar
Assets/_Game/Scripts/Core/Data/ItemDatabaseSO.cs — auditar
```

## 9. Estado atual do repo (Fase 0 — auditado)

| Sistema | Existe? | Reuso |
|---|---|---|
| QuestService (accept/progress/turn-in, instâncias dinâmicas) | SIM | `RegisterDynamicInstance` / `AcceptDynamicInstance` / `TurnIn` |
| QuestBoardService (gera contratos determinísticos por dia, seed+day+level) | SIM | gerar as encomendas da vila (template novo, mesmo padrão) |
| QuestBoardInteractable (IInteractable; publica QuestGiverInteractedEvent) | SIM | quadro com `boardId="village_orders"` |
| QuestRewardApplicator (ouro/itens/flags idempotente) | SIM | recompensa de ouro da encomenda |
| PlayerManager.AddGold / Inventory.RemoveItem / ItemDatabaseSO.TryGetById | SIM | ouro + consumo de itens + validar item ids |
| QuestStateRecord (persiste instâncias dinâmicas, IsDynamicInstance) | SIM | persistência das encomendas aceitas — **sem novo save provider** |
| GameEventBus + eventos em Core/Events (readonly struct) | SIM | VillageOrderFulfilledEvent (novo) |
| **Reputação da vila (nível agregado)** | **NÃO** | **fora de escopo aqui** — só publico o evento; slice 2 cria o serviço |

Estado real deve ser reconfirmado na Phase 0 antes de implementar. **Não recriar** QuestService/Board.

## 10. Engineering stories

```text
Como jogador, quero ver um quadro de encomendas da vila e aceitar pedidos que rendem ouro.
Como jogador, quero que a vila peça o que eu PRODUZO (colheita, minério, material), dando propósito à produção.
Como designer, quero pedidos rotativos determinísticos (mesmo dia ⇒ mesmos pedidos) para balancear.
Como maintainer, quero que a encomenda seja uma QuestInstance no pipeline existente (sem sistema paralelo).
Como autor da reputação (futuro), quero um evento de contribuição publicado no cumprimento.
```

## 11. Escopo

```text
Inclui:
- VillageOrderTemplate + catálogo de templates (categoria de item, faixa de quantidade, fórmula de ouro, pontos de contribuição, texto/flavor, NPC solicitante opcional);
- VillageOrdersBoardService (C# puro, determinístico por dia: seed + day + playerLevel ⇒ N encomendas), modelado no QuestBoardService;
- conversão de cada encomenda em QuestInstance (objetivo CollectItem) registrada via QuestService;
- VillageOrdersBoardInteractable (ou reuso de QuestBoardInteractable com boardId="village_orders") colocado na Câmara/Mercado;
- recompensa de ouro via QuestRewardApplicator existente no turn-in;
- VillageOrderFulfilledEvent(orderId, contributionPoints) publicado no turn-in (para a reputação futura);
- seleção de itens válidos (somente ids presentes no ItemDatabaseSO e plausíveis de produzir);
- EditMode tests do service (determinismo, validade de item id, escala de recompensa, contribuição);
- cenário humano de Play Mode.
```

## 12. Fora de escopo

```text
Não inclui:
- VillageReputationService / barra de reputação (slice 2);
- cadeias de fornecedor / desbloqueio de tier de loja (slice 3);
- desenvolvimento/financiamento da vila (slice 4);
- consumo/escassez de NPC (slice 5);
- UI nova rica (reusa a UI de quest/board existente; ajuste mínimo se necessário, senão DEFERRED_UI);
- novos itens/assets de arte; balance final dos valores.
```

## 13. Regras de não duplicação

```text
Não criar segundo pipeline de "contrato/encomenda": encomenda = QuestInstance no QuestService.
Não criar segundo gerador de board: reusar/estender o padrão do QuestBoardService.
Não criar segundo applicator de recompensa: usar QuestRewardApplicator.
Não criar reputação aqui (slice 2 cria o serviço); apenas publicar o evento de contribuição.
Não usar GameObject.Find/FindObjectOfType; não serializar Unity refs em save; eventos só com tipos simples.
```

## 14. Critérios de aceite

### 14.1 Encomendas determinísticas e válidas
- `VillageOrdersBoardService.GenerateOrders(seed, day, playerLevel)` retorna N encomendas; mesmo (seed,day,level) ⇒ mesmas encomendas (sem `UnityEngine.Random`/`DateTime.Now`).
- Todo item pedido existe no `ItemDatabaseSO` (id válido).
- Evidência: EditMode tests (determinismo + validade).

### 14.2 Encomenda é uma QuestInstance no pipeline existente
- Aceitar uma encomenda cria/registra uma `QuestInstance` (objetivo CollectItem) via `QuestService`; nenhum pipeline paralelo.
- Entregar (turn-in) consome os itens (Inventory.RemoveItem) e concede ouro pelo `QuestRewardApplicator` (idempotente).
- Evidência: code review + cenário humano; reuso documentado no report.

### 14.3 Evento de contribuição publicado
- No turn-in de uma encomenda, publica `VillageOrderFulfilledEvent(orderId, contributionPoints)` (readonly struct, tipos simples).
- Evidência: EditMode/integração + inspeção; o evento existe em `Core/Events`.

### 14.4 Persistência sem schema novo incompatível
- Encomendas aceitas persistem como instâncias dinâmicas de quest (QuestStateRecord) — sem novo save provider obrigatório. Se um estado de board precisar persistir, usar ISaveSectionProvider (pattern existente) com tipos simples.
- Evidência: round-trip (aceitar → salvar → carregar → ainda ativa).

### 14.5 Acesso pela cidade
- Um quadro interagível (E) existe na cena da cidade (Câmara/Mercado) mostrando as encomendas do dia.
- Evidência: marcador/objeto na TownScene (via gerador) + cenário humano.

### 14.6 Build limpo
- `Assembly-CSharp` + `Assembly-CSharp-Editor` exit 0; `run_strict_validation.ps1` exit 0; EditMode tests passam no Unity Test Runner.

---

# /speckit.plan

## 15. Arquitetura alvo

```
Assets/_Game/Scripts/Quests/Village/
  VillageOrderTemplate.cs         (struct/def: categoria, qty range, gold, contribution, flavor)
  VillageOrderCatalog.cs          (catálogo de templates — C# puro, const ids)
  VillageOrdersBoardService.cs    (C# puro: GenerateOrders(seed,day,level) ⇒ List<VillageOrder>; System.Random seeded)
  VillageOrder.cs                 (DTO da encomenda gerada: id, requisitos[(itemId,qty)], gold, contribution, flavor)
  VillageOrderToQuest.cs          (adapter: VillageOrder ⇒ QuestInstance objetivo CollectItem + reward gold)
Assets/_Game/Scripts/Core/Events/
  VillageOrderFulfilledEvent.cs   (readonly struct: string orderId, int contributionPoints)
Assets/_Game/Scripts/Quests/Runtime/
  VillageOrdersBoardInteractable.cs (ou reuso de QuestBoardInteractable com boardId="village_orders")
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs
  (colocar o quadro na Câmara/Mercado — objeto interagível)
Assets/_Game/Tests/EditMode/Quests/Village/
  VillageOrdersBoardServiceTests.cs
docs/validation/spec_village_orders_board_execution_report.md
docs/validation/playmode/spec_village_orders_board_human_test_scenario.md
```

## 16. Contratos, dados e eventos

- **16.1 Data:** `VillageOrder` (id, IReadOnlyList<(string itemId,int qty)>, int goldReward, int contributionPoints, string flavor, string requestingNpcId?). `VillageOrderTemplate` (item category/tag, qty min/max, gold formula, contribution). Catálogo com const ids estáveis (id-stability).
- **16.2 Runtime:** `VillageOrdersBoardService.GenerateOrders(int seed,int day,int playerLevel)` puro/determinístico (System.Random seeded; rng-and-determinism). `VillageOrderToQuest.ToQuestInstance(order)` cria a instância para `QuestService.AcceptDynamicInstance`.
- **16.3 Event:** novo `VillageOrderFulfilledEvent(string orderId,int contributionPoints)`. Subscribe no futuro VillageReputationService (slice 2). Publicado pelo handler de turn-in das encomendas (escuta `QuestCompletedEvent`/`QuestRewardClaimedEvent` da fonte board-village e mapeia p/ contribuição).
- **16.4 Save:** encomendas aceitas = QuestInstance dinâmica (já persistida por QuestStateRecord). Se precisar persistir "board do dia", é determinístico por day ⇒ recomputado (sem save). Sem Unity refs.
- **16.5 UI:** reusa a UI de quest/board existente (lista de objetivos + recompensa). Sem nova tela; se faltar exibir "contribuição", é ajuste mínimo (senão DEFERRED_UI_VISUAL).

## 17. Sistemas afetados

```text
Quests (board, instâncias dinâmicas, reward)
Economy (gold reward)
Inventory (consumo no turn-in)
Event bus (evento novo)
Save (via quest state existente)
Unity scene wiring (quadro na cidade)
```

## 18. Arquivos permitidos

```text
Assets/_Game/Scripts/Quests/Village/**
Assets/_Game/Scripts/Quests/Runtime/VillageOrdersBoardInteractable.cs
Assets/_Game/Scripts/Core/Events/VillageOrderFulfilledEvent.cs
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs (colocar o quadro)
Assets/_Game/Tests/EditMode/Quests/Village/**
docs/validation/**
```

## 19. Arquivos proibidos

```text
QuestService.cs / QuestRegistry.cs / QuestRewardApplicator.cs — alterações estruturais (só usar a API pública; estender via instância dinâmica)
Save schema incompatível
Assets/**/*.unity / *.prefab / *.asset (YAML manual — cena via gerador)
Packages/** / ProjectSettings/**
```

## 20. Estratégia de implementação

```
Fase 0 — Reconfirmar API de QuestService/QuestBoardService/QuestInstance/QuestRewardApplicator + PlayerManager/Inventory/ItemDatabase. Não recriar.
Fase 1 — Data: VillageOrder + VillageOrderTemplate + VillageOrderCatalog (C# puro, ids estáveis).
Fase 2 — Service: VillageOrdersBoardService (determinístico) + VillageOrderToQuest (adapter p/ QuestInstance).
Fase 3 — Evento + hook de turn-in (publicar VillageOrderFulfilledEvent).
Fase 4 — Quadro interagível na cidade (gerador) com boardId="village_orders".
Fase 5 — EditMode tests + build + strict validation + report + cenário humano.
```

## 21. Ordem de execucao (ordem segura)

```text
1. Auditar a API real (Phase 0).
2. Data (VillageOrder/Template/Catalog).
3. Service determinístico + adapter para QuestInstance.
4. Evento + hook de contribuição no turn-in.
5. Quadro na cena (gerador).
6. EditMode tests; dotnet build runtime+editor; run_strict_validation.
7. Report + cenário humano.
```

## 22. Paralelização

```md
- Parallelizable: CONDITIONAL
- Parallel group: village_economy
- Must not run with: specs que alterem QuestService/Board/RewardApplicator ou save de quest
- Reason: usa a API pública de quest; conflita se outra spec mexer no core de quest ao mesmo tempo.
```

## 23. Impacto em save/load

```text
Does this change save schema? NO (usa QuestStateRecord existente para instâncias dinâmicas)
Does this add a save section? NO (a menos que um board-state precise persistir; se sim, ISaveSectionProvider com tipos simples)
Does this require migration? NO
Does this persist Unity references? NO
```

## 24. Impacto em eventos

```text
Adds events: YES — VillageOrderFulfilledEvent(string orderId, int contributionPoints)
Changes existing events: NO
Requires unsubscribe pattern: YES (subscribers do evento em OnDisable)
```

## 25. Impacto em UI/Unity

```text
Changes UI: MINIMAL (reusa UI de quest/board; possível ajuste de exibição da contribuição)
Changes scenes: YES (quadro de encomendas na cidade — via gerador, não YAML)
Changes prefabs: NO
Changes ScriptableObjects/assets: NO (catálogo em C# puro)
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## 26. Riscos técnicos

```text
Risco: pedir item que o jogador não consegue produzir → encomenda impossível. Mitigação: pool restrito a categorias produzíveis + validar id no ItemDatabaseSO; EditMode test.
Risco: duplicar pipeline de quest. Mitigação: encomenda = QuestInstance via API pública; code review + system-reuse-audit.
Risco: recompensa não-idempotente em turn-in repetido. Mitigação: usar QuestRewardApplicator (idempotente por GrantedRewardIds).
Risco: determinismo quebrado (UnityEngine.Random). Mitigação: System.Random seeded (rng-and-determinism); EditMode test de determinismo.
```

## 27. Rollback

```text
Remover Scripts/Quests/Village/** + VillageOrderFulfilledEvent + o quadro do gerador.
Nenhuma migração para reverter (sem schema novo). Instâncias dinâmicas antigas expiram pelo fluxo normal de quest.
```

---

# /speckit.tasks

## 28. Tasks

```md
- [ ] T001 — Phase 0: reconfirmar API QuestService/QuestBoardService/QuestInstance/QuestRewardApplicator + PlayerManager/Inventory/ItemDatabase.
- [ ] T002 — VillageOrder + VillageOrderTemplate + VillageOrderCatalog (C# puro, ids estáveis).
- [ ] T003 — VillageOrdersBoardService (determinístico, System.Random seeded).
- [ ] T004 — VillageOrderToQuest (adapter ⇒ QuestInstance CollectItem + reward gold).
- [ ] T005 — VillageOrderFulfilledEvent + hook de turn-in que publica a contribuição.
- [ ] T006 — VillageOrdersBoardInteractable + colocação na cidade (gerador, boardId="village_orders").
- [ ] T007 — EditMode tests (determinismo, validade de item id, escala de reward, contribuição).
- [ ] T008 — Build runtime+editor + run_strict_validation; execution report + cenário humano.
```

## 29. Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj
dotnet build .\Assembly-CSharp-Editor.csproj
.\tools\docs\run_strict_validation.ps1
```
EditMode: Unity Test Runner (VillageOrdersBoardServiceTests). Play Mode: cenário humano. NOT RUN registrado com motivo se algo não rodar.

## 30. Testing Quality Gate

```md
- Changed deterministic logic: YES (geração de encomendas, mapeamento p/ quest, escala de reward)
- Requires EditMode tests: YES (determinismo, validade de item, reward/contribuição)
- Requires PlayMode automated or final human scenario: YES (ler quadro → aceitar → entregar → ouro + evento)
- Requires regression test: NO (feature nova)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: build exit 0; EditMode tests passam; cenário humano confirma ler/aceitar/entregar/recompensa + evento publicado.
```

## 31. Definition of Done

```text
Encomendas determinísticas e válidas geradas; encomenda = QuestInstance no pipeline existente.
Turn-in consome itens + concede ouro (idempotente) + publica VillageOrderFulfilledEvent.
Quadro interagível na cidade. Sem pipeline/reputação paralelos. Sem schema de save incompatível.
Build runtime+editor exit 0; EditMode tests; report + cenário humano. Sem claim de ACCEPTED sem Play Mode humano.
```

## 32. Anti-regressão

```text
Não alterar o core de QuestService/Board/RewardApplicator (só API pública).
Não serializar Unity refs em save; eventos só com tipos simples.
Não usar GameObject.Find/FindObjectOfType; não usar UnityEngine.Random na geração.
Recompensa idempotente (não repagar em turn-in repetido).
```

## 33. Notas para execução posterior

```text
Slice 2 (spec_village_reputation) consome VillageOrderFulfilledEvent → barra de reputação + unlocks/descontos.
Slice 3 (supplier chains), 4 (town development), 5 (light sim) dependem da reputação.
UI rica de encomendas e balance final dos valores ficam para passes futuros.
```
