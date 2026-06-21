# fable_20 — Calendar/Clock HUD + Day Detail UI — Execution Report

> **Spec:** `fable_20_spec_calendar_clock_hud_day_detail_ui`
> **Status:** BUILD_VALIDATED_WITH_WARNINGS (lógica + testes EditMode; binding visual de scene/canvas DEFERIDO)
> **Date:** 2026-06-21
> **Branch:** dev
> **Executor:** Claude Opus 4.8 (spec-implementer)

---

## Acceptance criteria extracted

| ID | Critério | Onde atendido | Evidência |
|----|----------|---------------|-----------|
| CA-1 | Widget reflete hora/dia/estação/clima/lua em tempo real | `ClockCalendarHudWidgetModel` (projeção pura) + `ClockCalendarHudWidgetView` (adapter por evento) | Tests `WidgetModel_SetDate_*`, `WidgetModel_Phase_TogglesLabel`, `WidgetModel_Weather_*`, `WidgetModel_Lunar_*`, `WidgetModel_Level_*` |
| CA-2 | Detalhe mostra amanhã/eventos CONHECIDOS; oculto permanece oculto | `CalendarDayDetailProjectionBuilder` + `LunarVisibilityProjection` (spoiler gate reusa `CalendarEventVisibilityPolicy`) | Tests `DayDetail_Build_UnknownLunarPeak_StaysHidden`, `DayDetail_Build_KnownLunarPeak_AppearsInModel`, `LunarPeaks_KnownPeak_IsShown_UnknownPeak_IsHidden`, `DayDetail_Build_NoForecast_HidesTomorrowWeather` |
| CA-3 | Modal bloqueia gameplay; Esc fecha; foco navegável | `CalendarDayDetailScreenView` (ModalManager push/pop + `UiFocusController` F14) | Tests `Screen_FocusOrder_*`, `Screen_FocusController_WrapsLinearly`; modal push/pop reusa `ModalManager` existente (relógio pausa pelo gate único de time_rules Rule 2) |
| EMENDA-D 6.1-A | Estações Semeio/Brasa/Véu/Gelo; dias D1-D7 sem nome | `CalendarDisplayNames.SeasonName/WeekdayLabel` | Test `SeasonName_MapsEnumToCanonicalVaalaranNames_WithoutReordering`, `WeekdayLabel_HasNoProperName_UsesDIndex_AndClamps` |
| EMENDA-D 6.3-A | 3 luas visíveis desde o início; picos spoiler-gated | `LunarVisibilityProjection` (3 luas nomeadas + picos D7/D14/D21) | Tests `LunarPeaks_PeakDays_MapToThreeNamedMoons`, `LunarPeaks_NonPeakDay_*` |
| EMENDA-C | Nível/XP no widget (barra fina) via `PlayerLevelChangedEvent` | `ClockCalendarHudWidgetModel.SetLevel/XpPercent/LevelLabel` + `ClockCalendarHudWidgetView.OnLevelChanged` | Test `WidgetModel_Level_XpPercentClampsAndLabel` |

---

## Existing systems audit

Auditoria pré-criação (skill `system-reuse-audit`). REUSADO, não duplicado:

| Sistema existente | Uso | Decisão |
|---|---|---|
| `CalendarDayDetailModel` (SPEC 04, CONTRACT_ONLY) | Modelo consumido e preenchido pelo novo builder | REUSE |
| `CalendarEventVisibilityPolicy` (SPEC 04) | Spoiler gate lunar (`CanShowLunarEvent`) | REUSE (sem política nova) |
| `CalendarBirthdayProjectionBuilder` (fable_57) | Aniversários do dia no detalhe | REUSE |
| `GameCalendarService.CurrentDate` / `GameDate` | Fonte de data (valores) | REUSE read-only |
| `LunarCycleService.CurrentCycle` / `LunarCycle` / `LunarPhase` | Fonte de fase lunar | REUSE read-only |
| `WorldWeatherService.CurrentWeather/TomorrowWeather` / `WeatherGenerator.GetWeatherDescription` | Fonte/descrição de clima | REUSE read-only |
| `GameEventBus` + `GamePhaseChangedEvent` / `WeatherChangedEvent` / `PlayerLevelChangedEvent` | Atualização do widget por evento | REUSE |
| `ModalManager` / `ModalType` | Push/pop do modal de detalhe (bloqueio de gameplay + pause de relógio) | REUSE (sem ModalType novo) |
| `UiFocusController` (fable_14) | Ordem de foco navegável + Esc/back | REUSE |
| `GameplayScreenTab.Calendar` (fable_14) | O detalhe é a aba Calendário do painel único F14 | REUSE (EMENDA ponto 3) |

**Nada criado paralelo:** sem novo manager/service/registry de tempo. Lógica de domínio é C# puro; MonoBehaviour é adapter fino.

**Auditoria de input (router):** a tecla `C` JÁ está ocupada pelo Craft de bolso (`CraftingModal._pocketCraftKey = KeyCode.C`). Para evitar conflito, o Calendário NÃO usa novo global key — é alcançado como aba do painel único F14 (`GameplayScreenTab.Calendar`), conforme EMENDA 2026-06-12 ponto 3. O widget HUD é sempre-on (sem tecla). Anti-regressão da spec ("Tecla C sem conflito") atendida ao não introduzir bind de C.

---

## Spec Compliance Matrix

| Requisito da spec | Implementação | Status |
|---|---|---|
| `ClockCalendarHudWidget` (canto sup-dir, atualizado por evento/tick) | `ClockCalendarHudWidgetModel` + `ClockCalendarHudWidgetView` (atualiza data/lua só na virada de dia; fase/clima/nível por evento) | OK |
| Tela `CalendarDayDetailScreenView` (ModalManager push, foco por teclado, Esc) | `CalendarDayDetailScreenView` | OK |
| Spoiler gate (festival/lunar oculto até descoberta) | `LunarVisibilityProjection` via `CalendarEventVisibilityPolicy`; predicate `isLunarPeakKnown` | OK |
| Nomes Vaalaran de estação (mapear do enum sem reordenar) | `CalendarDisplayNames.SeasonName` (Rule 4: ordem preservada) | OK |
| 3 luas desde o início; picos D7/D14/D21 gated | `LunarVisibilityProjection` | OK |
| Nível/XP no HUD (EMENDA-C) | `ClockCalendarHudWidgetModel.SetLevel` | OK |
| EditMode tests: formatação de data/hora, spoiler gate, binding do model | `CalendarUiTests` (21 testes) | OK |
| Não duplicar DebugHud; serviços de tempo intocados | Nenhum serviço de tempo alterado; DebugHud não tocado | OK |
| Binding visual de scene/canvas (sprites, Text/Image, prefab) | Views expõem o Model; binding no Editor | DEFERIDO (DEFERRED_UI_VISUAL) |

---

## Validation

```
Validation method: builds individuais + docs + diff completeness (PowerShell, $LASTEXITCODE checado)
Assembly-CSharp:        PASS (0 Erros, 1 Aviso pré-existente) — exit 0
Assembly-CSharp-Editor: PASS (0 Erros, 3 Avisos pré-existentes) — exit 0
Docs validation:        PASS (validate_docs.ps1 exit 0)
Diff completeness:      PASS (check_spec_diff_completeness.ps1 exit 0)
```

`run_strict_validation.ps1`: NÃO usado como gate de bloqueio — pode dar exit 1 por 3 cenas `.unity` já modificadas no working tree (CaveScene/FarmScene/TownScene), alteração ambiental pré-existente NÃO desta spec. Builds e gates escopados acima cobrem o critério.

Arquivos `.cs` mudaram → `dotnet build` é o sinal de compile fallback (autoritativo seria Unity batchmode, NÃO executado — ver Testing Quality Gate). Nenhum import/namespace/type error novo.

---

## Honest status rationale

`BUILD_VALIDATED_WITH_WARNINGS`: o núcleo determinístico (projeções, spoiler gate, nomes, foco) está implementado, auditado e coberto por 21 EditMode tests; ambos os builds passam com 0 erros; gates de docs e diff-completeness passam. O **binding visual** (sprites de clima/lua, Text/Image do widget, layout do prefab no GameplayHudCanvas, e o wiring da aba Calendário no painel único F14 — que ainda não tem View MonoBehaviour) fica DEFERIDO para a validação final, alinhado com o estado atual de F14 (o painel é CONTRACT_ONLY de View) e com a política do dono (Play Mode/validação humana DIFERIDOS). Os EditMode tests NÃO foram executados pelo Unity Test Runner (sandbox sem Unity) — ver risco residual abaixo.

Não promovido a `implementados/` (status `_WITH_WARNINGS` + visual deferido). Sem claim de Play Mode/ACCEPTED.

---

## Testing Quality Gate

```
Changed runtime code: YES
Changed deterministic logic: YES (formatação de data/estação/lua, spoiler gate, ordem de foco, Xp%)
Changed Unity scene/prefab/asset wiring: NO
Automated tests added/updated: YES (Assets/_Game/Tests/EditMode/UI/Calendar/CalendarUiTests.cs — 21 testes)
Automated tests command: dotnet build .\Assembly-CSharp.csproj --no-restore (compila os testes; Unity Test Runner NOT RUN — sandbox sem Unity)
Manual Play Mode scenario: docs/validation/playmode/fable_20_human_test_scenario.md
Justification if no automated tests: N/A (testes adicionados)
Residual risk: (1) EditMode tests compilam mas não foram executados pelo Unity Test Runner — risco de falha de asserção runtime é baixo (lógica pura, sem dependências de cena). (2) Binding visual do widget/aba e o comportamento em cena (anchors sup-dir, pause de relógio com modal, foco visível, sem movimento com modal aberto) só são verificáveis em Play Mode — cobertos pelo cenário humano, DEFERIDOS.
```

`validated_game_rules: [time_rules.md, ui_rules.md]`
- time_rules.md: Rule 4 (nomes Vaalaran mapeados sem reordenar o enum — resolve pendência aberta de fable_20), Rule 5/6 (3 luas nomeadas + cadência de picos D7/D14/D21, spoiler-gated), Rule 8 (calendar não revela segredo antes da descoberta), Rule 2 (pause de relógio via modal único — reusa ModalManager, sem flag paralela).
- ui_rules.md: Rule 2 (projection/ViewModel puro + View fina, rebuild por evento sem polling), Rule 3 (widget clock/date/weather/lunar no sup-dir, valores donos de time_rules), Rule 8 (sem pixel absoluto — anchors no binding visual deferido).

---

## Remaining work (deferido)

1. Binding visual: prefab do widget no GameplayHudCanvas (anchors sup-dir, sob o minimapa F38), Text/Image, sprites de clima/fase lunar, barra fina de XP.
2. Wiring da aba Calendário no painel único F14 (depende da View MonoBehaviour do `GameplayScreensPanelModel`, hoje CONTRACT_ONLY).
3. Provider de XP corrente/threshold para `SetLevel` (o evento de nível só carrega o nível novo).
4. Execução do cenário humano de Play Mode (`docs/validation/playmode/fable_20_human_test_scenario.md`) na validação final.
5. Execução dos EditMode tests pelo Unity Test Runner.

---

## Files changed

```
Assets/_Game/Scripts/UI/Calendar/CalendarDisplayNames.cs                 (NEW)
Assets/_Game/Scripts/UI/Calendar/LunarVisibilityProjection.cs            (NEW)
Assets/_Game/Scripts/UI/Calendar/CalendarDayDetailProjectionBuilder.cs   (NEW)
Assets/_Game/Scripts/UI/Runtime/ClockCalendarHudWidgetModel.cs           (NEW)
Assets/_Game/Scripts/UI/Runtime/ClockCalendarHudWidgetView.cs            (NEW)
Assets/_Game/Scripts/UI/Runtime/Screens/CalendarDayDetailScreenView.cs   (NEW)
Assets/_Game/Tests/EditMode/UI/Calendar/CalendarUiTests.cs               (NEW)
docs/validation/fable_20_spec_calendar_clock_hud_day_detail_ui_execution_report.md (NEW)
docs/validation/playmode/fable_20_human_test_scenario.md                 (NEW)
```

(Assembly-CSharp.csproj recebeu os 7 `<Compile Include>` para o build local — gitignored, não commitado.)
