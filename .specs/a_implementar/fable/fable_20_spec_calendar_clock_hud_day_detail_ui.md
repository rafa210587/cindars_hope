# SPEC — UI: Relógio/Calendário no HUD + Tela de Detalhe do Dia (refatoração da 02_spec_calendar_ui)

> **Spec ID:** `fable_20_spec_calendar_clock_hud_day_detail_ui`
> **Status:** A implementar
> **Wave:** FABLE — Gap Closure Bloco F (fundação/UI)
> **Priority:** P2
> **Type:** UI / Integration
> **Domain:** UI / Time
> **Parallelizable:** NO
> **Parallel group:** fable_bloco_F
> **Can run with:** N/A
> **Must not run with:** fable_14 (DEVE executar DEPOIS — reusa RuntimeUiBuilder/UiFocusController), fable_15
> **Repo lock scope:** GameplayHudCanvas, RuntimeUiBuilder
> **Depends on:** `fable_14` (builder), `fable_15` (WorldWeatherService), WAVE 02 (calendar/lunar services)
> **Blocks:** N/A
> **Scope:** widget HUD de relógio/data/clima/lua + tela modal de detalhe do dia, substituindo a spec antiga.
> **Out of scope:** quadro público da cidade (15_spec futura), aniversários, segredos lunares revelados cedo.
> **Supersedes:** `02_spec_calendar_ui_weather_lunar_display.md` (refatorada: a original não cita
> GameplayHudCanvas/ModalManager/WI-23 e antecede F14/F15; crosswalk na 00B).

required_adrs: []
required_game_rules: [time_rules.md, ui_rules.md]

---

# /speckit.specify

## Contexto

Única pendência real não-executada da fila antiga (WAVE 02 Spec 3, deferred). A direction
SEASONS exige apresentação de relógio/dia/estação/clima/lua; a regra canônica:
"Calendar não revela segredo antes da descoberta". Hoje só o DebugHud IMGUI mostra tempo.
Pré-requisitos novos já existem: GameplayHudCanvas (WI-23), RuntimeUiBuilder/foco (F14),
WorldWeatherService (F15), GameCalendarService/LunarCycleService (WAVE 02).

## Objetivo

Widget permanente no GameplayHudCanvas (hora, dia/estação/ano, ícone de clima de hoje,
fase lunar) + tecla C abrindo CalendarDayDetail modal (CalendarDayDetailModel WAVE 04
CONTRACT_ONLY consumido): previsão de amanhã, evento lunar ativo, festivais conhecidos —
sem revelar eventos não descobertos.

## Fontes obrigatórias lidas

```text
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
.claude/rules/testing-quality-gate.md
```

## Estado atual do repo

```text
Existe: GameCalendarService/Season/LunarCycleService/LunarPhase (WAVE 02);
CalendarDayDetailModel (WAVE 04 — consumir); GameplayHudCanvas+binder (WI-23);
RuntimeUiBuilder/UiFocusController (F14); WorldWeatherService (F15); GameplayInputRouter.
Não existe: widget de relógio Canvas; tela de detalhe; ModalType Calendar (auditar enum).
```

## Escopo

```text
Inclui: ClockCalendarHudWidget (canto superior, atualizado por evento/tick); tecla C →
CalendarDayDetailScreenView (RuntimeUiBuilder, ModalManager push, foco por teclado, Esc);
spoiler gate (festival/lunar oculto até flag de descoberta — QuestFlagService);
EditMode tests: formatação de data/hora, spoiler gate, binding do model.
```

## Fora de escopo / Não duplicação / Critérios

```text
Fora: quadro público, aniversários, secrets revelados.
Não duplicar: DebugHud permanece (debug); serviços de tempo intocados.
CA-1: widget reflete hora/dia/estação/clima/lua em tempo real.
CA-2: detalhe mostra amanhã/eventos CONHECIDOS; oculto permanece oculto (teste).
CA-3: modal bloqueia gameplay; Esc fecha; foco navegável.
```

# /speckit.plan

```text
Assets/_Game/Scripts/UI/Runtime/{ClockCalendarHudWidget, Screens/CalendarDayDetailScreenView}.cs
Assets/_Game/Tests/EditMode/UI/CalendarUiTests.cs
Paralelização: NO (canvas/builder locks). Save: NO. Eventos: consome existentes.
UI: YES; Play Mode final: YES; DEFERRED_TO_FINAL_VALIDATION.
Risco: tick por frame custoso → atualizar por minuto in-game/evento.
Rollback: remover widget/tela.
```

# /speckit.tasks

```md
- [ ] T001 — Auditar ModalType/input C livre + APIs de calendar/lunar.
- [ ] T002 — Widget HUD + binding por evento.
- [ ] T003 — Tela de detalhe + spoiler gate + foco.
- [ ] T004 — Testes; csproj; run_strict_validation; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (formatação/gates) | EditMode: YES | PlayMode/human: YES
- Regression: YES (DebugHud/input intactos) | DEFERRED_TO_FINAL_VALIDATION
- Minimum evidence for ACCEPTED: testes + cenário humano com widget e detalhe

## Definition of Done

```text
Widget + detalhe funcionais com spoiler gate; builds 0E; report; original 02_spec marcada absorvida.
```

## Anti-regressão

```text
Calendar não revela segredo (canon). Modal guard. Tecla C sem conflito (auditar router).
```


---

## EMENDA 2026-06-12 (pós-canonização dos catálogos — VINCULANTE)

```text
1. LUAS (decisão Q5.1): cada lua é visível 8-10 dias por estação com 1 noite de PICO —
   o widget mostra a lua visível atual e dias restantes; o detalhe do dia mostra picos
   CONHECIDOS (spoiler gate).
2. Layout/âncoras: HUD_LAYOUT_SCENES §2 vence (sup-dir, sob o relógio o minimapa F38).
3. Vira a aba Calendário do painel único F14 (além do widget HUD).
```


---

## EMENDA 2026-06-12-C (jornada do jogador — VINCULANTE)

```text
1. FEEDBACK DE PROGRESSÃO no HUD real: widget compacto de Nível/XP (barra fina) junto ao
   relógio + toast "Nível N!" via GameplayFeedbackService consumindo PlayerLevelChangedEvent
   (evento existe desde F42; hoje só o DebugHud consome). Gap achado na simulação da
   jornada (beat 6a): level up não tem feedback fora do debug.
```


---

## EMENDA 2026-06-12-D (Decisões v2 — VINCULANTE; fonte: FABLE_DECISOES_RESPOSTAS_v2.0.md)

```text
1. NOMES CANÔNICOS (6.1-A): estações Semeio / Brasa / Véu / Gelo; dias D1-D7 sem nome.
   O widget e o detalhe do dia usam esses nomes (mapear do enum Season existente).
2. As 3 luas visíveis no calendário DESDE O INÍCIO (6.3-A) — picos continuam spoiler-gated.
```
