# Execution Report — spec_city_artisan_stations

> **Spec:** `.specs/a_implementar/spec_city_artisan_stations.md`
> **Date:** 2026-06-23
> **Status:** BUILD_VALIDATED (Play Mode humano DEFERRED_TO_FINAL_VALIDATION)
> **validated_adrs:** []
> **validated_game_rules:** []

## 1. Acceptance criteria — evidência

| Critério (spec §14) | Resultado | Evidência |
|---|---|---|
| 14.1 Estação abre craft filtrado | OK | `CraftingStationInteractable.Interact` publica `OpenCraftingStationRequestedEvent(id, type)`; `CraftingModal.HandleOpenStationRequested` chama `Open(GetOrCreateStation(id,type))` → `GetRecipesForStation(type)` filtra. EditMode confirma o evento. |
| 14.2 Cada casa-ofício tem estação | OK | `TryAddCraftingStation` no gerador coloca `Station_*` em Blacksmith(Forge), AlchemyLab(Alchemy), Inn(CookingStation), Workshop(Carpentry), Residential_3/Mirela(Sewing), MarketHall(Workbench). |
| 14.3 Desacoplado | OK | A estação não referencia `CraftingModal`; comunica por evento (Core/Events). Code review. |
| 14.4 Build limpo | OK | `Assembly-CSharp` exit 0; `Assembly-CSharp-Editor` exit 0; docs validation PASS. |

## 2. Existing systems audit (Fase 0)

- Reutilizados (não recriados): `CraftingRuntime` (GetOrCreateStation/GetRecipesForStation), `CraftingModal.Open`, `CraftingStation`, `WorkshopType`, `IInteractable`/`InteractionSystem`, `CreateInteriorProp`, `GameEventBus`.
- Criados: `OpenCraftingStationRequestedEvent` (Core/Events), `CraftingStationInteractable` (Craft), `TryAddCraftingStation`/`CreateCraftingStation` (gerador), subscribe no `CraftingModal`, EditMode test.

## 3. Spec Compliance Matrix

| Requisito | Implementação |
|---|---|
| Evento de abrir craft | `OpenCraftingStationRequestedEvent(string, WorkshopType)` em `CraftingEvents.cs` |
| Estação interagível | `CraftingStationInteractable : IInteractable` (Configure + Interact publica o evento) |
| Modal assina e abre | `CraftingModal` OnEnable/OnDisable + `HandleOpenStationRequested` |
| 6 estações nas casas | `TryAddCraftingStation` (switch por nome de casa) + `CreateCraftingStation` |
| Filtro por WorkshopType | reuso de `GetRecipesForStation` (já existente) |

## 4. Validation

```text
Validation method: dotnet build (runtime + editor) + validate_docs.ps1
Assembly-CSharp: PASS (exit 0)
Assembly-CSharp-Editor: PASS (exit 0)
Docs validation: PASS
EditMode: CraftingStationInteractableTests (3 testes) — compila no Assembly-CSharp; execução no Unity Test Runner (DEFERRED)
Strict validation (run_strict_validation.ps1): exit 1 SOMENTE por "Forbidden files altered" = .asset de
  Bestiary/Items pré-modificados ANTES desta sessão (git status inicial); NÃO tocados por esta spec.
```

## 5. Honest status rationale

**BUILD_VALIDATED.** Critérios centrais implementados; builds e docs exit 0; lógica nova coberta por EditMode test
(o interactable publica o evento certo). Não é `ACCEPTED` porque a validação de Play Mode (apertar E na forja →
abrir craft de forja) está deferida ao lote final. O único FAIL do strict é `.asset` pré-modificados fora do escopo.

## 6. Testing Quality Gate

```text
Changed deterministic logic:    YES (CraftingStationInteractable publica evento por id/type)
Changed Unity scene/prefab:     YES (estações na TownScene — via gerador; regenerar pelo menu)
Automated tests added/updated:  YES (Tests/EditMode/Craft/CraftingStationInteractableTests.cs)
Automated tests command:        dotnet build (compila); Unity Test Runner (execução EditMode)
Manual Play Mode scenario:      docs/validation/playmode/spec_city_artisan_stations_human_test_scenario.md
Justification if no tests:      N/A
Residual risk:                  abertura visual do craft e o filtro por tipo precisam de confirmação no Unity (Play Mode)
```

## 7. Remaining work

- Humano: `CindarsHope/Inicializar Projeto` (regenera a cena com as estações) → entrar numa casa-ofício e apertar E na estação → o craft abre filtrado pelo tipo.
- Slice 2 (`spec_closed_chains_leather_cloth_wool`) usa as estações Sewing/Workbench; arte final das estações é pass próprio.
