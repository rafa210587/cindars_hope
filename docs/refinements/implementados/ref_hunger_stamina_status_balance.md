---
type: refinement
spec: spec_hunger_stamina_status_balance.md
status: implementado completo
phase: implementation
---

# Refinement - Hunger, stamina, status e time balance

## Contratos Fechados

- Fome zero aplica dano de HP em tick de tempo, nunca dreno de stamina por frame.
- Regeneracao de stamina: base `15/s`, delay `1s`, tiers `1.0`, `0.6`, `0.3` e taxa fixa `2/s` em fome zero.
- Fome critica (`1-9`) reduz movimento para `0.85x`.
- Status do player usam IDs e duracao, com refresh para reaplicacao do mesmo ID e eventos de lifecycle.
- Game time usa ciclo configuravel e pausa com modal ativo.
- Save/load preserva valores simples de stamina, game time e status effects.

## Integracao Runtime

- Bootstrap, installers e geradores conectam hunger, stamina, status e time nas tres cenas MVP.
- Acoes existentes de farm/world/craft/combat recebem a instancia persistente de stamina.
- `FoodConsumer` passou a respeitar restauracao de stamina e buffs do item consumido.
- HUD minimo apresenta hunger, stamina e status com duracao; layout final permanece na SPEC 17.

## Validacao

- Unity compile e scene wiring: PASS em batchmode.
- Regressao SPEC 06 e SPEC 07: PASS.
- Docs validator: PASS.
- Scanner de logs: FAIL documentado por padroes conhecidos de assemblies `firstpass` apesar de `Tundra build success`.
- Play Mode interativo: NOT RUN, com checklist em `docs/validation/SPEC_09_HUNGER_STAMINA_STATUS_TIME_VALIDATION_20260524.md`.
