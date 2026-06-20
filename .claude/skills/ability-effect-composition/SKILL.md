---
name: ability-effect-composition
description: Projeta um sistema componível de ability/effect/modifier — abilities construídas a partir de effects pequenos e reutilizáveis (damage, heal, apply status, knockback) com validation/cost/cooldown/execution separados. Use ao projetar como abilities/spells/status effects se COMPÕEM, não ao fazer wiring de assets de combat data já existentes.
---

# Skill: Composição de Ability e Effect

Esta é a contraparte de **design** de duas skills de wiring: `combat-data-wiring` (popular `WeaponDatabase`/`SpellDatabase`/`StatusEffectDatabase`) e `player-ability-runtime` (abilities de movimento sem slot, como dash/block). Use esta skill quando a pergunta for "como as abilities devem ser *construídas a partir de peças reutilizáveis*" para que um designer consiga criar uma nova spell sem código novo.

## Procedimento

1. **Separe data de runtime state.** A *definition* da ability (id, cost, cooldown, lista de effects) é data (ScriptableObject, rule `data-driven-content`); o *runtime* (cooldown remaining, charges) é state por instância.
2. **Modele o execution context explicitamente.** Caster id, target id(s), position, time e **seed** viajam num context object — não puxados de globals. O seed torna os resultados determinísticos e replayable (skill `rng-and-determinism`).
3. **Divida o pipeline.** `CanExecute` (validation: target válido? in range?) → cost check → cooldown check → execute → apply effects. Cada estágio pode falhar como um expected failure de categoria 1 (rule `error-handling-resilience`): retorne `bool` + `FailureReason`, exponha via HUD event (skill `game-feel-checklist`), nunca lance exceção.
4. **Effects são pequenos e componíveis.** Unidades `IAbilityEffect.Apply(context)`: deal damage, heal, apply status, knockback, spawn projectile. Uma ability é uma lista ordenada de effects. A variação vem de data nova, não de classes de ability novas.
5. **Modifiers/buffs são decorators.** Buffs/debuffs/affixes envolvem ou ajustam os parâmetros de effect (rule `gameplay-design-patterns`, Decorator) e passam por `StatusEffectDatabase`. Defina as regras de stacking e expiry de antemão.
6. **Ordem de effect determinística.** Quando a ordem importa (DoT antes do death check, shield antes do damage), torne-a explícita e estável — não dependa da ordem de dictionary/iteration.
7. **Emita domain events.** Cada effect publica events do `GameEventBus` (`StatusAndDamageEvents`, events de hit/feedback) para que VFX/SFX/HUD reajam sem o código de combat tocar em presentation (rule `event-bus-only-gameplay-communication`).

## Saída esperada

- Data model: `AbilityDefinition` (id, cost, cooldown, `IReadOnlyList<IAbilityEffect>`) + runtime state.
- Execution context (caster/target/position/time/seed).
- Pipeline com pontos de falha e reasons explícitos.
- Interfaces de effect/modifier e a política de stacking/expiry.
- Events publicados para presentation.
- EditMode tests (skill `editmode-test-authoring`).

## Interação com save e testes

- Persista o **outcome** (cooldowns remaining, status ids ativos + remaining duration) como simple types/ids — nunca effect objects (rule `unity-architecture`; skill `save-load-pattern`).
- Testes: cooldown gates barram reuso; cost é consumido apenas no sucesso; mesmo context+seed → outcome de effect idêntico duas vezes; stacking/expiry de status; ordem de effect determinística; reload restaura cooldowns/statuses sem reaplicar seus effects de on-apply (idempotency).
