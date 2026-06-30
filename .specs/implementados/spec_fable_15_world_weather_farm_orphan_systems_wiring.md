# SPEC — Wiring de Sistemas Órfãos: Clima/Chuva, Refresh de Recursos, Qualidade/Fertilizante

> **Spec ID:** `fable_15_spec_world_weather_farm_orphan_systems_wiring`
> **Status:** BUILD_VALIDATED (executada — E01; evidência: docs/validation/fable_15_spec_world_weather_farm_orphan_systems_wiring_execution_report.md)
> **Wave:** FABLE — Corretivas de Aderência (auditoria 00B)
> **Priority:** P0 (corretiva — sistemas pagos e não entregues)
> **Type:** Integration / Runtime
> **Domain:** Time / Farm
> **Parallelizable:** CONDITIONAL
> **Parallel group:** fable_corretivas
> **Can run with:** fable_04, fable_09
> **Must not run with:** fable_12, fable_16 (FarmScene generator/DayStarted consumers compartilhados)
> **Repo lock scope:** `FarmPlot.cs`, `TimeManager`/day-transition consumers, `CreateMvpFarmScene.cs`
> **Depends on:** WAVE 02/05 (módulos órfãos existentes)
> **Blocks:** specs futuras 15_ (modificadores sazonais)
> **Scope:** instanciar e ligar 5 módulos órfãos confirmados na auditoria 00B ao loop diário real.
> **Out of scope:** criar lógica nova de clima/qualidade (já existe), neve/tempestade visual, estufa.

required_adrs: []
required_game_rules: [farm_rules.md, time_rules.md]

---

# /speckit.specify

## Contexto

Auditoria 00B: `WeatherGenerator` (estático), `RainIrrigationIntegration` (MonoBehaviour
jamais adicionado a cena), `FarmResourceRefreshProcessor` (construtor jamais chamado),
`CropQualityResolver` e `FertilizerApplicationService` (não referenciados pelo `FarmPlot`)
estão completos e testados desde as WAVES 02/05 — e mortos. As directions
SEASONS_CALENDAR_WEATHER_LUNAR e FARM v1.3 dependem deles.

## Problema

Chuva nunca rega, clima não existe em gameplay, nós de recurso não renovam por política,
toda colheita tem qualidade única e fertilizante não tem efeito. São 5 promessas centrais
do farm sim pagas em código e não entregues ao jogador.

## Objetivo

Ao final desta spec, deve existir um `WorldWeatherService` runtime (bootstrap pattern) que
fixa o clima do dia no `DayStartedEvent` via `WeatherGenerator` e publica
`WeatherChangedEvent`; `RainIrrigationIntegration` deve estar na FarmScene (gerador) regando
canteiros em dia de chuva; `FarmResourceRefreshProcessor` deve rodar no day transition;
`FarmPlot` deve consultar `CropQualityResolver` (com fertilizante via
`FertilizerApplicationService`) na colheita — sem reescrever nenhum dos 5 módulos.

## Fontes obrigatórias lidas

```text
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
.specs/a_implementar/fable/fable_00B_adherence_audit_queue_triage.md
docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
.claude/rules/testing-quality-gate.md
```

## Estado atual do repo

```text
Existe, testado e NÃO recriar (apenas LIGAR):
- World/Weather/{WeatherGenerator, WeatherType}
- Farm/RainIrrigationIntegration (auditar API: como referencia FarmPlotRegistry)
- Farm/Resources/FarmResourceRefreshProcessor (+ ResourceNodeDefinition)
- Farm/Crops/CropQualityResolver ; Farm/Fertilizer/{FertilizerDefinition, FertilizerApplicationService}
- DayStartedEvent / TimeManager / GameCalendarService (estação)
- FarmPlot (plantar/regar/colher) + FarmPlotRegistry; itens de fertilizante? (auditar ItemDatabase)
Não existe:
- qualquer instanciação dos 5 módulos; WeatherChangedEvent; uso de fertilizante via item
```

## Escopo

```text
Inclui:
- WorldWeatherService (NOVO, bootstrap RuntimeInitializeOnLoadMethod): clima do dia
  determinístico (WeatherGenerator(today)), CurrentWeather/TomorrowWeather expostos,
  WeatherChangedEvent no DayStarted;
- RainIrrigationIntegration adicionada pelo CreateMvpFarmScene + rebind ao FarmPlotRegistry;
  chuva rega todos os canteiros plantados no início do dia;
- FarmResourceRefreshProcessor instanciado pelo serviço de farm existente (auditar dono:
  FarmDailyGoalRuntimeBootstrap como host) consumindo DayStartedEvent;
- FarmPlot.Harvest: qualidade via CropQualityResolver (entradas: regas, fertilizante,
  estação via GameCalendarService) e item de colheita com qualidade (auditar: ItemStack
  suporta qualidade? Se não, qualidade vira multiplicador de quantidade/preço — documentar decisão);
- uso de item fertilizante em canteiro (IInteractable path existente do FarmPlot) via
  FertilizerApplicationService; 2 itens de fertilizante no ItemDataInitializer se ausentes;
- HUD: DebugHud mostra clima do dia (1 linha);
- EditMode tests: determinismo do clima por dia, rega por chuva, qualidade com/sem fertilizante,
  refresh por política.
```

## Fora de escopo

```text
Não inclui: neve/tempestade/efeitos visuais; estufa; modificadores de schedule por clima
(15_spec futuras); previsão na UI (F20); crops morrendo sem água (auditar se já existe — se
não, registrar como follow-up, não expandir escopo).
```

## Regras de não duplicação

```text
Proibido reescrever os 5 módulos — somente instanciar/ligar.
Proibido segundo serviço de clima ou segundo caminho de qualidade.
```

## Critérios de aceite

### CA-1 Clima vivo e determinístico
- Mesmo dia → mesmo clima após reload; WeatherChangedEvent publicado no day transition.
### CA-2 Chuva rega
- Dia chuvoso: canteiros plantados acordam regados (estado idêntico ao de rega manual).
### CA-3 Qualidade e fertilizante reais
- Colheita fertilizada+bem regada > qualidade base (resultado observável em item/quantidade/preço).
### CA-4 Refresh de nós
- Nó depletado renova conforme política da definição após N dias.
- Evidência (todos): EditMode tests + cenário humano do lote.

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/World/Weather/WorldWeatherService.cs   (NOVO host)
Assets/_Game/Scripts/Core/Events/WeatherChangedEvent.cs     (NOVO)
Assets/_Game/Scripts/Farm/FarmPlot.cs                       (qualidade/fertilizante na colheita)
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs (RainIrrigationIntegration)
Assets/_Game/Tests/EditMode/Farm/OrphanSystemsWiringTests.cs
```

Save: clima re-derivável por dia (determinístico) — sem seção nova. Fertilizante aplicado:
auditar se FarmPlot save section comporta campo aditivo (bool/string fertilizerId).

## Paralelização

- Parallelizable: CONDITIONAL — locks: FarmPlot/FarmScene generator/DayStarted.

## Impacto em save/load

```text
Schema change: campo aditivo no DTO de FarmPlot (fertilizerId, default vazio) — sem migration.
```

## Impacto em eventos

```text
Adds: WeatherChangedEvent | Changes: NO | Unsubscribe: YES
```

## Impacto em UI/Unity

```text
Scenes via gerador; Play Mode final: YES; Human timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos

```text
Risco: API órfã incompatível com runtime atual (escrita às cegas em 2026-06-0x).
Mitigação: Fase 0 audita assinaturas; adaptar via wrapper fino SEM tocar no módulo se possível;
se módulo exigir correção real, documentar diff mínimo no report.
```

## Rollback

```text
Remover service/wiring — módulos voltam a órfãos (estado atual).
```

# /speckit.tasks

```md
- [ ] T001 — Auditar assinaturas dos 5 módulos + ItemStack/qualidade + save do FarmPlot.
- [ ] T002 — WorldWeatherService + WeatherChangedEvent + testes de determinismo.
- [ ] T003 — RainIrrigationIntegration no gerador + rega por chuva + testes.
- [ ] T004 — FarmResourceRefreshProcessor hospedado + testes.
- [ ] T005 — Qualidade/fertilizante no FarmPlot.Harvest + itens + testes.
- [ ] T006 — DebugHud clima; csproj; run_strict_validation; report.
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
- Requires PlayMode automated or final human scenario: YES
- Requires regression test: YES (plantar/regar/colher manual intactos)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano com dia de chuva e colheita fertilizada

## Definition of Done

```text
5 módulos órfãos vivos no loop diário; zero reescrita; farm loop manual intacto; builds 0E; report.
```

## Anti-regressão

```text
Rega manual/colheita atuais intactas. DayStarted continua disparando daily goals/shop restock.
Sem GameObject.Find. Save compatível (campo aditivo).
```
