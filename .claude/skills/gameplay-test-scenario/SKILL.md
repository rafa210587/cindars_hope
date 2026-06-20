---
name: gameplay-test-scenario
description: Gera um human test scenario para implementação de gameplay/runtime. Exigido para o closeout de Phase 3. Use depois de implementar qualquer spec que mude comportamento de gameplay, UI, combat, save, cave, farm, shop, equipment, skill tree ou asset.
---

# Skill: Cenário de Teste de Gameplay

Testes automatizados verificam "compila e não crasha?"; o human test scenario verifica "está com o feel certo e funciona como pretendido?". Os dois são necessários — esta skill cobre o segundo.

## Quando usar

A implementação da spec toca:
- Comportamento de jogo (combat, movement, animation)
- UI state (modals, hotbar, inventory, equipment)
- Mecânicas de save/load
- Geração procedural ou runtime da cave
- Economy de farm/shop
- Event publishing ou comunicação de gameplay
- Dados baseados em ScriptableObject que afetam o gameplay

## Quando NÃO usar

- Spec é docs-only (sem mudanças de gameplay)
- Spec é refactor puro de código (sem mudança de comportamento)
- Spec é wiring puro de asset (sem mudança de comportamento de gameplay) — talvez; incerto

## Procedimento

Depois da implementação completa, antes do `/finish-spec`:

### 1. Analise o que mudou

A partir do execution report:
- Quais arquivos de gameplay foram modificados?
- Qual era o objetivo da mudança?
- Que novo comportamento deve existir?
- Que comportamento existente pode ter sofrido regressão?

### 2. Crie o human test scenario

Escreva o arquivo: `docs/05_VALIDATION/playmode/<spec_id>_human_test_scenario.md`

Use o template: `docs/05_VALIDATION/playmode/PLAYMODE_TEST_SCENARIO_TEMPLATE.md`

### 3. Documente cada seção

**Feature Summary:** 1-2 frases do que o humano deve testar

**Scenes:** Quais scenes carregar / criar para o teste (ex.: "Town com player no shop")

**Required Initial State:** Que save game state ou setup é necessário
- Items de inventory
- Stats de character
- Flags de game progression
- Estado do map

**Scenario 1 — Happy Path:** A feature principal funcionando corretamente
- Instruções passo a passo
- Resultado visual/áudio esperado
- Logs esperados (none/some/specific)

**Scenario 2 — Negative/Edge Path:** Casos de erro, condições de boundary
- O que pode dar errado?
- Como disparar o erro?
- Comportamento esperado (graceful fail? warning log?)

**Scenario 3 — Save/Load:** Se a spec toca save/persistence
- Execute a ação do Scenario 1
- Salve o jogo
- Carregue o jogo
- Verifique que o state persistiu corretamente

**Expected Results:** Resumo do que deve acontecer
- O jogo não crasha
- Sem errors inesperados
- Feedback visual/áudio correto
- Mudanças de state corretas
- Persistência de save correta

**Console Expectations:** O que deve (e o que NÃO deve) aparecer nos logs
- Warnings esperados: none / liste-os
- Errors esperados: none / liste-os
- Errors proibidos: qualquer console ERROR é um fail

**Pass/Fail Checklist:**
- [ ] Feature executa sem crash
- [ ] Logs esperados aparecem
- [ ] Logs proibidos não aparecem
- [ ] Feedback visual correto
- [ ] Save/load (se aplicável) funciona
- [ ] Sem regressões em outras features

**Notes:** Qualquer coisa especial
- Limitações conhecidas
- Pule o Scenario 3 se o save não for testado
- Tested on: Mac / Windows / both
- Requer debug mode ou settings específicos

### 4. Referencie no Execution Report

Em `docs/validation/<spec_id>_execution_report.md`:

Adicione a seção:
```
## How to Test

Human test scenario: docs/05_VALIDATION/playmode/<spec_id>_human_test_scenario.md

Expected test time: X minutes
Tester: [human] (not automated)
Status: [NOT RUN until Phase 3]
```

### 5. Link a partir do output do `/finish-spec`

Quando o `/finish-spec` processa uma runtime spec:
- Verifique se existe arquivo de test scenario
- Se encontrado: referencie-o no Output em "Expected Gameplay Behavior"
- Se não encontrado: error (se runtime mudou) ou warning (se incerto)

## Regras

- NÃO pule para mudanças "simples" — humanos são o juiz final
- O test scenario é escrito EM INGLÊS ou PORTUGUÊS dependendo do idioma da spec
- Cada scenario deve levar de 1 a 5 minutos para executar
- Seja específico: "click inventory" e não "explore menu"
- Inclua os debug helpers necessários (cheat codes, spawn items, etc.)
- Console errors bloqueiam o PASS de Phase 3

## Validação

Depois dos testes humanos:
- Preencha o Pass/Fail Checklist
- Registre a data e o nome do tester
- Se houver qualquer FAIL: volte ao debugging
- Se tudo PASS: pronto para o signoff de Phase 3

## Quando parar e reportar

- Não dá para identificar o que testar (objetivo da spec incerto)
- O teste exigiria modificar o escopo da spec (ex.: "build 50 items to test")
- O test scenario levaria >30 minutos (quebre em passos menores)

## Exemplo de saída

```markdown
# Human Test Scenario — SPEC_18 Hotbar Management

## Feature Summary
Test the hotbar UI allows equipping/unequipping weapons and using hotkey switches.

## Scenes
- Town (default spawn location)

## Required Initial State
- Fresh save game
- Player has: wooden sword, iron sword, staff, healing potion
- Hotbar empty

## Scenario 1 — Happy Path
1. Open hotbar UI (press H)
2. Drag wooden sword to slot 1
3. Press 1 — verify wooden sword is held
4. Drag iron sword to slot 2
5. Press 2 — verify iron sword is held
6. Switch back to slot 1 — verify wooden sword returned
7. Close hotbar (press H again or click X)
8. Verify equipped weapon visible in hand

Expected logs: none

## Scenario 2 — Negative Path
1. Open hotbar
2. Try to drag non-weapon item (healing potion) to weapon slot
3. Verify drag is rejected (returns to original position or shows "invalid" feedback)
4. Try to equip slot 3 when empty
5. Verify hotkey 3 has no effect (or shows "empty" UI feedback)

Expected logs: none (or optional "slot empty" debug log)

## Scenario 3 — Save/Load
1. Perform Scenario 1 (equip sword 1 in slot 1)
2. Save game
3. Load game
4. Verify hotbar state restored (sword 1 still in slot 1)
5. Verify equipped weapon is still in hand

Expected logs: none

## Expected Results
- Hotbar UI functions without crashes
- Equipping/unequipping weapons works
- Hotkey switches equipment instantly
- Invalid equipment rejected gracefully
- Save/load preserves hotbar state

## Console Expectations
- Errors: NONE
- Warnings: none (optional "slot empty" is acceptable)
- Forbidden: Any ERROR or Exception

## Pass/Fail Checklist
- [x] Feature executes without crash
- [x] Expected behavior observed
- [x] Forbidden logs absent
- [x] Save/load works
- [ ] OTHER (specify)

**Overall:** PASS / FAIL

## Notes
- Tested on: Windows
- Tested by: human QA
- Date: 2026-06-01
- Known limitation: hotbar UI does not yet support drag-reorder (future SPEC)
```
