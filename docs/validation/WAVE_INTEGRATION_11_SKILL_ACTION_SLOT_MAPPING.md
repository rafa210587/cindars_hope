# WAVE INTEGRATION 11 — Skill Action Slot Mapping

**Date:** 2026-06-08
**Status:** BUILD_VALIDATED_WITH_SKILL_EFFECT_DEBT
**Patch:** WAVE_INTEGRATION_11_ACTION_SKILL_BALANCE_PATCH

---

## Rules

- Active slots count: 4
- Dash uses: Space + direction (NOT a skill slot)
- Dodge uses: double tap directional (NOT a skill slot)
- Block uses: Left Shift (NOT a skill slot)
- Dash/Dodge/Block do not occupy active slots

---

## Active slot eligible skills

| Tree | SkillId | Skill name | EffectId | Slot eligible | TargetType | ImplementNow | DeferredReason |
|---|---|---|---|---:|---|---:|---|
| melee | melee_offhand_cut | Corte da Mão Secundária | combat.melee.offhand_cut | YES | Enemy | NO | COMBAT_RUNTIME_PENDING |
| melee | melee_guarded_block | Bloqueio Guardado | combat.melee.block | YES | Self | NO | COMBAT_RUNTIME_PENDING |
| melee | melee_battle_dash | Arrancada de Combate | combat.melee.battle_dash | YES | Self | NO | COMBAT_RUNTIME_PENDING |
| melee | melee_leap_attack | Salto Devastador | combat.melee.leap_attack | YES | Enemy | NO | COMBAT_RUNTIME_PENDING |
| melee | melee_whirl_cut | Corte Giratório | combat.melee.whirl_cut | YES | Area | NO | COMBAT_RUNTIME_PENDING |
| melee | melee.avanco_aco | Avanço de Aço | melee.avanco_aco | YES | Enemy | FEEDBACK_ONLY | DEFERRED_RUNTIME_EFFECT |
| melee | melee.grito_desafio | Grito de Desafio | melee.grito_desafio | YES | Area | FEEDBACK_ONLY | DEFERRED_RUNTIME_EFFECT |
| melee | melee.investida_quebra_guarda | Investida Quebra-Guarda | melee.investida_quebra_guarda | YES | Enemy | FEEDBACK_ONLY | DEFERRED_RUNTIME_EFFECT |
| ranged | ranged_charged_shot | Disparo Carregado | combat.ranged.charged_shot | YES | Enemy | NO | COMBAT_RUNTIME_PENDING |
| ranged | ranged_line_piercer | Linha Perfurante | combat.ranged.line_piercer | YES | Enemy | NO | COMBAT_RUNTIME_PENDING |
| ranged | ranged_multishot_fan | Tiro Triplo | combat.ranged.multishot_fan | YES | Area | NO | COMBAT_RUNTIME_PENDING |
| ranged | ranged_bleeding_arrow | Flecha Sangrante | combat.ranged.bleeding_arrow | YES | Enemy | NO | COMBAT_RUNTIME_PENDING |
| ranged | ranged_marked_prey | Presa Marcada | combat.ranged.marked_prey | YES | Enemy | NO | COMBAT_RUNTIME_PENDING |
| magic | magic_fire_spark | Fagulha Ígnea | combat.magic.fire_spark | YES | Enemy | NO | COMBAT_RUNTIME_PENDING |
| magic | magic_ice_bind | Laço de Gelo | combat.magic.ice_bind | YES | Enemy | NO | COMBAT_RUNTIME_PENDING |
| magic | magic_toxic_cloud | Nuvem Tóxica | combat.magic.toxic_cloud | YES | Area | NO | COMBAT_RUNTIME_PENDING |
| magic | magic_lightning_chain | Corrente Relâmpago | combat.magic.lightning_chain | YES | Enemy | NO | COMBAT_RUNTIME_PENDING |
| magic | magic_elemental_ward | Guarda Elemental | combat.magic.elemental_ward | YES | Self | NO | COMBAT_RUNTIME_PENDING |
| magic | magic_slowing_sigils | Sigilos Lentificantes | combat.magic.slowing_sigils | YES | Area | NO | COMBAT_RUNTIME_PENDING |
| magic | magic.chama_breve | Chama Breve | magic.chama_breve | YES | Enemy | FEEDBACK_ONLY | DEFERRED_RUNTIME_EFFECT |
| magic | magic.rajada_gelida | Rajada Gélida | magic.rajada_gelida | YES | Enemy | FEEDBACK_ONLY | DEFERRED_RUNTIME_EFFECT |
| survival | survival_emergency_roll | Rolamento de Emergência | (debug: farm.crop.water_skill) | YES | Self | BRIDGE | DEMO_MAPPING |
| survival | survival_last_breath | Último Fôlego | (not mapped) | YES | Self | NO | NO_EFFECT_DEFINED |
| survival | survival.sinal_retirada | Sinal de Retirada | survival.sinal_retirada | YES | Self | FEEDBACK_ONLY | DEFERRED_RUNTIME_EFFECT |
| survival | survival.isca_improvisada | Isca Improvisada | survival.isca_improvisada | YES | WorldPoint | FEEDBACK_ONLY | DEFERRED_RUNTIME_EFFECT |
| survival | survival.kit_emergencia | Kit de Emergência | survival.kit_emergencia | YES | Self | FEEDBACK_ONLY | DEFERRED_RUNTIME_EFFECT |
| survival | survival.instinto_sobrevivencia | Instinto de Sobrevivência | survival.instinto_sobrevivencia | YES | Area | FEEDBACK_ONLY | DEFERRED_RUNTIME_EFFECT |
| survival | survival.campo_seguro | Campo Seguro | survival.campo_seguro | YES | WorldPoint | FEEDBACK_ONLY | DEFERRED_RUNTIME_EFFECT |
| crafting | crafting_field_patch | Remendo de Campo | (debug: farm.crop.water_skill) | YES | CurrentInteractable | BRIDGE | DEMO_MAPPING |
| crafting | crafting_quick_repair | Reparo Rápido | crafting.quick_repair (not mapped) | YES | Self | NO | NO_EFFECT_DEFINED |
| crafting | crafting.irrigador_portatil | Irrigador Portátil | crafting.irrigador_portatil | YES | Area | FEEDBACK_ONLY | DEFERRED_RUNTIME_EFFECT |
| crafting | crafting.bomba_improvisada | Bomba Improvisada | crafting.bomba_improvisada | YES | WorldPoint | FEEDBACK_ONLY | DEFERRED_RUNTIME_EFFECT |
| crafting | crafting.mecanismo_campo | Mecanismo de Campo | crafting.mecanismo_campo | YES | WorldPoint | FEEDBACK_ONLY | DEFERRED_RUNTIME_EFFECT |
| crafting | crafting.marca_eficiencia | Marca de Eficiência | crafting.marca_eficiencia | YES | Area | FEEDBACK_ONLY | DEFERRED_RUNTIME_EFFECT |

---

## Non-slot actions

| Action | Input | Source | Uses active slot? | Runtime system | Notes |
|---|---|---|---:|---|---|
| Dash | Space + direction | canonical direction | NO | PlayerDashController | costs 40 Stamina; 3.5 tiles; BUILD_VALIDATED |
| Dodge | double tap directional | canonical direction | NO | PlayerMovementAbilityController | costs 40 Stamina; 1.5 tiles; BUILD_VALIDATED |
| Block | Left Shift | canonical direction | NO | BLOCK_RUNTIME_DEFERRED | Melee/Block rank 1 unlocks; deferred to combat runtime |

---

## Notes

```text
ImplementNow values:
  FEEDBACK_ONLY  = FeedbackOnlySkillEffectExecutor registered; returns success + message; no gameplay effect
  BRIDGE         = debug mapping to farm.crop.water_skill for vertical slice demonstration
  NO             = EffectId registered in SkillActionToEffectId but no executor in registry (falls to "Deferred")
  YES            = fully implemented executor (only farm.crop.water_skill as of this patch)
```

---

*Created: 2026-06-08 (WAVE_INTEGRATION_11_ACTION_SKILL_BALANCE_PATCH)*
