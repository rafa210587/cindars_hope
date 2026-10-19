# WAVE_INTEGRATION_14 — Crafting Station + Processing Jobs

<!-- /speckit.specify -->
# /speckit.specify

<!-- /speckit.plan -->
# /speckit.plan

<!-- /speckit.tasks -->
# /speckit.tasks

required_adrs: []
required_game_rules: []

## Ordem de execucao

WAVE_INTEGRATION_14 — executa após WAVE_INTEGRATION_13.

## Depende de

- WAVE_INTEGRATION_13: CODE_READY_HUMAN_UNITY_ACTION_REQUIRED_BUILD_VALIDATED (commit 315cfda)
- WAVE_INTEGRATION_12: BUILD_VALIDATED (NPC/Shop/Dialogue)

## Bloqueia

- WAVE_INTEGRATION_15 (fase futura — não planejada)

---

> **Projeto:** Cindar's Hope
> **Wave:** WAVE_INTEGRATION — Scene Wiring e Playable Runtime Bridge
> **Spec:** 14 — Crafting Station + Processing Jobs
> **Tipo:** Integração Unity / Crafting Runtime / Processing Jobs / Station Interaction / Recipe UI / Inventory Bridge
> **Status inicial:** A implementar
> **Prioridade:** P0 — transforma crafting/processing em loop jogável e visível
> **Dependência direta:** WAVE_INTEGRATION_13 (CODE_READY_HUMAN_UNITY_ACTION_REQUIRED — done); usar FarmScene como target temporário com debt explícito enquanto wiring humano pendente.
> **Resultado esperado:** o jogador consegue interagir com uma estação real de crafting/processing, abrir UI, ver receitas, validar ingredientes reais, craftar item, iniciar um processing job, acompanhar/visualizar o job e coletar output sem duplicação.

## Status
A_IMPLEMENTAR

## Dependências confirmadas
- WAVE_INTEGRATION_13: CODE_READY_HUMAN_UNITY_ACTION_REQUIRED_BUILD_VALIDATED (commit 315cfda)
- WAVE_INTEGRATION_12: BUILD_VALIDATED (NPC/Shop/Dialogue)
- WAVE_INTEGRATION_11B/C: BUILD_VALIDATED (Dash/Dodge/Block)

## Design principles (section 2 of original spec)
1. Crafting somente em workshops/estações
2. Data-driven por ScriptableObject/config/registry
3. Recipe valida estação, ingredientes, unlocks
4. Processing idempotente — não coleta duas vezes
5. UI não manipula inventory diretamente — service é dono
6. MVP usa dados temporários marcados TEMPORARY_CRAFTING_TEST_RECIPE

## Acceptance criteria
AC-01: Baseline não BLOCKED
AC-02-03: Builds antes/depois passam
AC-04: Design/Direction Compliance Matrix preenchida
AC-05-08: Audits de crafting/inventory/UI/processing feitos
AC-09: Station existe ou wiring humano claro
AC-10: UI abre por interação
AC-11: Recipe list/detail aparece
AC-12: Ingredientes validados antes de consumo
AC-13: Craft consome input e cria output sem bypass
AC-14: Processing job inicia ou debt/blocker explícito
AC-15: Processing job não duplica output
AC-16: Save/load debt de processing documentado
AC-17: Authoring model criado
AC-18: Human checklist criado
AC-19: Final Design/Direction Revalidation
AC-20: Completeness Revalidation Pass 2

## Termination statuses
- BUILD_VALIDATED_SCENE_WIRED: estação na cena, loop completo
- BUILD_VALIDATED_WITH_CRAFTING_PROCESSING_DEBT: crafting instantâneo ok, processing com dívidas
- CODE_READY_HUMAN_UNITY_ACTION_REQUIRED: código pronto, cena precisa de wiring humano
- BLOCKED: inventory API inexistente, build falha, design conflict
