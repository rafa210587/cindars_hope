# Specs Executadas (BUILD_VALIDATED) — Aguardando Validação Humana Final

> **Movidas em 2026-06-12** (triagem FABLE 00B) do nível raiz de `a_implementar/`.
> **Estado:** código entregue e build-validado nas WAVES 00-12 + WAVE_INTEGRATION 01-26 +
> FIX-001/001B + test harness. **NÃO REEXECUTAR.**
> **Por que não estão em `implementados/`:** a regra `spec-promotion-requires-evidence`
> exige evidência Phase 2-3 (Unity validators + Play Mode humano), ainda pendente.
> Após a validação humana final por wave (`docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md`),
> promover via `/finish-spec`.

## Conteúdo

- `00_*` a `12_*` — specs das waves core (status por wave no `SPEC_REGISTRY_TO_IMPLEMENT.md`)
- `spec_wave_integration_*` — slices de integração (reports em `docs/validation/`)
- `spec_test_harness_editmode_playmode_quality_gate.md` — 01Q, executada
- FIX_001: DELETADA em 2026-06-12 (batch FABLE-2 autorizado)
- `02_spec_calendar_ui_weather_lunar_display.md` — NUNCA executada; **absorvida por**
  `fable/fable_20_spec_calendar_clock_hud_day_detail_ui.md` (crosswalk na fable_00B)

## Avisos da auditoria FABLE 00B

Vários módulos entregues por estas specs estão **órfãos** (compilam/testam mas nada os
instancia). As specs corretivas `fable_15`-`fable_19` fazem o wiring. Consultar
`fable/fable_00B_adherence_audit_queue_triage.md` antes de qualquer claim de aderência.

## Duplicatas resolvidas

A série `03_spec_quest_*` (duplicata literal da `09_spec_quest_*`) foi DELETADA em
2026-06-12 (batch FABLE-2 autorizado). A série 09 permanece como registro da execução.