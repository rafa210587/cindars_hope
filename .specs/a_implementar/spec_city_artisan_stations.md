# SPEC — Estações de Ofício nas Casas (forja, alambique, tear, fogão… interagíveis)

> **Spec ID:** `spec_city_artisan_stations`
> **Status:** A implementar
> **Wave:** WAVE VILLAGE ECONOMY — slice 1 (estações)
> **Priority:** P1
> **Type:** Runtime + Tooling (gerador de cena)
> **Domain:** City
> **Parallelizable:** CONDITIONAL
> **Parallel group:** village_economy
> **Can run with:** specs que não toquem Craft/CraftingModal nem CreateMvpTownScene
> **Must not run with:** specs que alterem CraftingModal/CraftingRuntime ou o gerador da cidade
> **Repo lock scope:** `Assets/_Game/Scripts/Craft/**`, `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs`, `Assets/_Game/Scripts/Core/Events/CraftingEvents.cs`
> **Depende de (Depends on):**
> - Craft runtime (CraftingRuntime.GetOrCreateStation/GetRecipesForStation, CraftingModal.Open, WorkshopType) — já existe
> - InteractionSystem / IInteractable — já existe
> - Casas percorríveis (CreateWalkInHouse) — já existe
> **Bloqueia (Blocks):**
> - `spec_closed_chains_leather_cloth_wool` (usa a estação Sewing/Tanoaria)
> **Scope:** Cada casa-ofício ganha uma estação FÍSICA interagível que, ao apertar E, abre o craft filtrado pelo WorkshopType dela (forja→Forge, alambique→Alchemy, tear→Sewing, fogão→CookingStation, bancada→Carpentry/Workbench), via evento desacoplado.
> **Out of scope:** Receitas novas (couro/tecido/lã = slice 2), novos WorkshopTypes (ex.: Tanning), NPCs novos, balance.

required_adrs: []
required_game_rules: []

---

# /speckit.specify

## 5. Contexto

A direção de autossuficiência (`CITY_SELF_SUFFICIENCY_DIRECTION_v1.0.md`) define "as casas têm a mesma
lógica": toda casa-ofício deve ter a **estação** do seu ofício, visível e funcional. Hoje o craft só abre
por tecla global (C, pocket) — não há nenhum objeto de estação na cena. A auditoria de Fase 0 confirmou
que `CraftingModal.Open(CraftingStation)` já filtra por `WorkshopType` e que `CraftingRuntime` já cria
estações por id; falta só a **ponte de interação** (apertar E numa estação → abrir o craft daquele tipo).

## 6. Problema

Sem estações na cena, os ofícios são nominais: a vila "diz" que tem ferreiro/alquimista, mas não há forja
nem alambique para o jogador usar. Abrir o craft só por tecla global esvazia o sentido das casas-ofício e
contradiz a direção aprovada. Criar um segundo fluxo de craft seria duplicação proibida.

## 7. Objetivo

Ao final desta spec, cada casa-ofício tem uma **estação interagível** que abre o craft **filtrado pelo
WorkshopType** correspondente, reutilizando `CraftingModal`/`CraftingRuntime` via um **evento novo**
(`OpenCraftingStationRequestedEvent`) — sem chamada direta MonoBehaviour→MonoBehaviour, sem novo fluxo de
craft, sem receitas novas.

## 8. Fontes obrigatórias lidas

```text
docs/design/gameplay/city/CITY_SELF_SUFFICIENCY_DIRECTION_v1.0.md
.claude/rules/unity-architecture.md (event bus, no global search)
.claude/skills/crafting-recipe-authoring/SKILL.md
.claude/skills/scene-interactable-wiring/SKILL.md
Assets/_Game/Scripts/Craft/CraftingRuntime.cs / CraftingStation.cs / Data/WorkshopType.cs — auditar
Assets/_Game/Scripts/UI/Crafting/CraftingModal.cs — auditar (Open + subscribe)
Assets/_Game/Scripts/Core/Events/CraftingEvents.cs — auditar (adicionar evento)
Assets/_Game/Scripts/Interaction/IInteractable.cs / InteractionSystem.cs — auditar
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs (CreateInteriorProp/CreateWalkInHouse) — auditar
```

## 9. Estado atual do repo (Fase 0 — auditado)

| Sistema | Existe? | Reuso |
|---|---|---|
| `CraftingModal.Open(CraftingStation)` (filtra por StationType) | SIM | abrir o craft da estação |
| `CraftingRuntime.GetOrCreateStation(id, WorkshopType)` / `GetRecipesForStation` | SIM | obter a estação |
| `WorkshopType` (None/Workbench/Forge/CookingStation/Alchemy/Carpentry/Sewing) | SIM | tipo da estação |
| `IInteractable` + `InteractionSystem` (E, prompt, Interact) | SIM | a estação é IInteractable |
| `CreateInteriorProp` (prop andável; retorna GameObject) | SIM | base visual da estação |
| Evento para abrir craft externamente | **NÃO** | **criar** `OpenCraftingStationRequestedEvent` |
| Objeto de estação na cena | **NÃO** | **criar** props de estação no gerador |

Não recriar fluxo de craft. Reconfirmar API na Phase 0.

## 10. Engineering stories

```text
Como jogador, quero apertar E na forja da ferraria e abrir o craft de forja (só receitas de forja).
Como jogador, quero que cada casa-ofício tenha a estação do seu ofício (alambique, tear, fogão, bancada).
Como maintainer, quero que a estação abra o craft via EVENTO (sem MonoBehaviour chamando MonoBehaviour direto).
```

## 11. Escopo

```text
Inclui:
- Evento OpenCraftingStationRequestedEvent(string stationInstanceId, WorkshopType workshopType);
- CraftingStationInteractable : IInteractable — no Interact publica o evento; Configure(id, type, label);
- CraftingModal assina o evento e chama Open(GetOrCreateStation(id, type));
- Gerador: CreateCraftingStation(house, localPos, color, id, WorkshopType, label) + colocar as estações nas casas:
  Blacksmith→Forge, AlchemyLab→Alchemy, Inn→CookingStation, Workshop→Carpentry, Mirela(House_Residential_3)→Sewing, MarketHall→Workbench (geral);
- EditMode test do filtro (estação X só lista receitas X) e do evento;
- cenário humano de Play Mode.
```

## 12. Fora de escopo

```text
Não inclui:
- receitas novas (couro/tecido/lã = slice 2) nem novos WorkshopTypes (Tanning);
- NPCs novos (slice 3); UI nova de craft (reusa CraftingModal);
- balance; arte final (props são placeholders coloridos).
```

## 13. Regras de não duplicação

```text
Não criar segundo fluxo/serviço de craft: usar CraftingRuntime + CraftingModal existentes.
Não chamar CraftingModal direto da estação: publicar evento e o modal assina (event-bus).
Não usar GameObject.Find/FindObjectOfType; estação resolve tudo via evento/serviço.
Não criar novo WorkshopType aqui.
```

## 14. Critérios de aceite

### 14.1 Estação abre o craft filtrado
- Apertar E numa estação publica `OpenCraftingStationRequestedEvent(id, type)`; o `CraftingModal` abre com
  `GetRecipesForStation(type)` — só receitas daquele WorkshopType.
- Evidência: code review + cenário humano; EditMode confirma o filtro por tipo.

### 14.2 Cada casa-ofício tem sua estação
- Props de estação existem dentro de: Blacksmith(Forge), AlchemyLab(Alchemy), Inn(CookingStation),
  Workshop(Carpentry), Mirela(Sewing), MarketHall(Workbench), com prompt de interação.
- Evidência: gerador + inspeção (objetos `Station_*` nas casas).

### 14.3 Desacoplado
- A estação NÃO referencia `CraftingModal` diretamente; comunica por `OpenCraftingStationRequestedEvent`.
- Evidência: code review (sem ref direta), evento em Core/Events.

### 14.4 Build limpo
- `Assembly-CSharp` + `Assembly-CSharp-Editor` exit 0; `run_strict_validation.ps1` exit 0; EditMode passa.

---

# /speckit.plan

## 15. Arquitetura alvo

```
Assets/_Game/Scripts/Core/Events/CraftingEvents.cs
  + struct OpenCraftingStationRequestedEvent(string StationInstanceId, WorkshopType WorkshopType)
Assets/_Game/Scripts/Craft/CraftingStationInteractable.cs   (IInteractable; publica o evento)
Assets/_Game/Scripts/UI/Crafting/CraftingModal.cs           (OnEnable: assina; handler: Open(GetOrCreateStation))
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs
  + CreateCraftingStation(...) e colocação das 6 estações nas casas
Assets/_Game/Tests/EditMode/Craft/CraftingStationFilterTests.cs
docs/validation/spec_city_artisan_stations_execution_report.md
docs/validation/playmode/spec_city_artisan_stations_human_test_scenario.md
```

## 16. Contratos, dados e eventos

- **16.3 Event (novo):** `OpenCraftingStationRequestedEvent` — readonly struct, `string StationInstanceId`,
  `WorkshopType WorkshopType` (enum = tipo simples). Publicado por `CraftingStationInteractable.Interact`;
  assinado por `CraftingModal`.
- **16.2 Runtime:** `CraftingStationInteractable : MonoBehaviour, IInteractable` com `Configure(string id,
  WorkshopType type, string label)`; `InteractionPrompt => label`; `Interact` publica o evento.
- **16.4 Save:** N/A (estações são determinísticas da cena; CraftingRuntime já persiste jobs).
- **16.5 UI:** reusa `CraftingModal` (só adiciona o subscribe).

## 17. Sistemas afetados

```text
Craft (modal + runtime — reuso)
Interaction (IInteractable — reuso)
Event bus (evento novo)
Unity scene wiring (estações via gerador)
```

## 18. Arquivos permitidos

```text
Assets/_Game/Scripts/Core/Events/CraftingEvents.cs
Assets/_Game/Scripts/Craft/CraftingStationInteractable.cs
Assets/_Game/Scripts/UI/Crafting/CraftingModal.cs (só subscribe + handler)
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs (estações)
Assets/_Game/Tests/EditMode/Craft/**
docs/validation/**
```

## 19. Arquivos proibidos

```text
CraftingRuntime.cs / CraftingStation.cs (estruturas — só usar API pública)
Assets/**/*.unity / *.prefab / *.asset (YAML manual)
Packages/** / ProjectSettings/**
```

## 20. Estratégia de implementação

```
Fase 0 — Reconfirmar CraftingModal.Open + CraftingRuntime API + WorkshopType + IInteractable.
Fase 1 — Evento OpenCraftingStationRequestedEvent.
Fase 2 — CraftingStationInteractable (publica o evento).
Fase 3 — CraftingModal assina o evento e abre a estação.
Fase 4 — Gerador: CreateCraftingStation + 6 estações nas casas.
Fase 5 — EditMode test (filtro) + build + strict + report + cenário humano.
```

## 21. Ordem de execucao (ordem segura)

```text
1. Auditar API (Phase 0).
2. Evento.
3. Interactable.
4. CraftingModal subscribe.
5. Estações no gerador.
6. dotnet build runtime+editor; run_strict_validation.
7. Report + cenário humano.
```

## 22. Paralelização

```md
- Parallelizable: CONDITIONAL
- Must not run with: specs que alterem CraftingModal/CraftingRuntime ou CreateMvpTownScene
- Reason: usa API pública de craft + edita o gerador da cidade.
```

## 23. Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? NO
```

## 24. Impacto em eventos

```text
Adds events: YES — OpenCraftingStationRequestedEvent(string, WorkshopType)
Changes existing events: NO
Requires unsubscribe pattern: YES (CraftingModal des-assina em OnDisable)
```

## 25. Impacto em UI/Unity

```text
Changes UI: MINIMAL (CraftingModal só ganha subscribe/handler)
Changes scenes: YES (estações nas casas — via gerador)
Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## 26. Riscos técnicos

```text
Risco: CraftingModal inativo quando o evento dispara (não assina). Mitigação: assinar em OnEnable; o controlador de UI fica sempre ativo (já roda Update p/ tecla C).
Risco: estação sem receitas (lista vazia) p/ um tipo. Mitigação: aceitável (abre vazia); slice 2 preenche couro/tecido. EditMode cobre o filtro.
Risco: colisão da estação trancando o cômodo. Mitigação: collider de interação é trigger; o prop não bloqueia (ou bloqueio pequeno encostado na parede).
Risco: acoplamento direto. Mitigação: evento (event-bus), não ref direta.
```

## 27. Rollback

```text
Remover CraftingStationInteractable + o evento + as estações do gerador + o subscribe do CraftingModal.
Sem migração. Craft segue acessível por tecla C (pocket) como antes.
```

---

# /speckit.tasks

## 28. Tasks

```md
- [ ] T001 — Phase 0: reconfirmar CraftingModal.Open / CraftingRuntime / WorkshopType / IInteractable.
- [ ] T002 — Evento OpenCraftingStationRequestedEvent em CraftingEvents.cs.
- [ ] T003 — CraftingStationInteractable (Configure + Interact publica o evento).
- [ ] T004 — CraftingModal assina o evento (OnEnable/OnDisable) + handler que abre a estação.
- [ ] T005 — Gerador: CreateCraftingStation + 6 estações (Forge/Alchemy/Cooking/Carpentry/Sewing/Workbench).
- [ ] T006 — EditMode test do filtro por WorkshopType + build + strict; report + cenário humano.
```

## 29. Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj
dotnet build .\Assembly-CSharp-Editor.csproj
.\tools\docs\run_strict_validation.ps1
```
EditMode (filtro por estação) no Unity Test Runner. Play Mode: cenário humano. NOT RUN com motivo se algo não rodar.

## 30. Testing Quality Gate

```md
- Changed deterministic logic: SIM (filtro de receitas por estação — já existe; cobrir com teste)
- Requires EditMode tests: SIM (GetRecipesForStation só retorna receitas do tipo)
- Requires PlayMode automated or final human scenario: SIM (E na estação abre o craft do tipo)
- Requires regression test: NO
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: build exit 0; EditMode passa; cenário humano confirma E→craft filtrado por estação.
```

## 31. Definition of Done

```text
Estações interagíveis nas 6 casas-ofício; E abre o craft filtrado pelo WorkshopType, via evento.
Sem fluxo de craft paralelo; sem ref direta MonoBehaviour→MonoBehaviour.
Build runtime+editor exit 0; EditMode; report + cenário humano. Sem ACCEPTED sem Play Mode humano.
```

## 32. Anti-regressão

```text
Tecla C (pocket craft) continua funcionando.
Não alterar o core de CraftingRuntime/CraftingStation (só API pública).
Não usar GameObject.Find/FindObjectOfType; eventos só com tipos simples.
Não quebrar o audit de relayout (estações têm nomes Station_*, não House_*/TownTree_*).
```

## 33. Notas para execução posterior

```text
Slice 2 adiciona receitas de couro/tecido/lã (e talvez WorkshopType Tanning) — usa a estação Sewing/Tanoaria.
Props de estação são placeholders; arte final vira pass próprio (guia de imagens já lista as estações).
```
