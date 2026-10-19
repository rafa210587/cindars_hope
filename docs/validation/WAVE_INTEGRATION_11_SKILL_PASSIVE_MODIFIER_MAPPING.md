# WAVE INTEGRATION 11 — Skill Passive/Modifier Mapping

**Date:** 2026-06-08
**Status:** BUILD_VALIDATED_WITH_SKILL_EFFECT_DEBT
**Patch:** WAVE_INTEGRATION_11_ACTION_SKILL_BALANCE_PATCH

---

## Passive and modifier skills

| Tree | SkillId | Skill name | Type | EffectId | Runtime hook target | ImplementNow | DeferredReason |
|---|---|---|---|---|---|---:|---|
| melee | melee_iron_grip | Pegada de Ferro | Passive | AttackFlat +1 | SkillPassiveApplicator | PARTIAL | modifier applied via SkillPassiveApplicator; combat stat hook deferred |
| melee | melee_guarded_stance | Postura Guardada | Passive | DefenseFlat +1 | SkillPassiveApplicator | PARTIAL | modifier applied; combat stat hook deferred |
| melee | melee_dual_wield_flow | Fluxo de Duas Lâminas | Modifier | DualWieldAttackSpeedBonus +0.1 | SkillPassiveApplicator | PARTIAL | modifier registered; dual wield combat hook deferred |
| melee | melee_two_handed_momentum | Ímpeto de Duas Mãos | Modifier | TwoHandedDamageBonus +1 | SkillPassiveApplicator | PARTIAL | modifier registered; two-handed combat hook deferred |
| melee | melee_dodge_training | Treino de Esquiva | Modifier | DodgeCostReduction -0.1 | PlayerMovementAbilityController | PARTIAL | dodge cost reduction registered; final wiring deferred |
| melee | melee_capstone_battle_rhythm | Ritmo de Batalha | Exclusive Capstone | (no flat modifier) | combat runtime | NO | COMBAT_RUNTIME_PENDING |
| ranged | ranged_steady_hand | Mão Firme | Passive | BowDamageFlat +1 | SkillPassiveApplicator | PARTIAL | modifier registered; ranged combat hook deferred |
| ranged | ranged_long_sight | Mira Longa | Modifier | BowRangeFlat +0.5 | SkillPassiveApplicator | PARTIAL | modifier registered; ranged combat hook deferred |
| ranged | ranged_quick_nock | Encaixe Rápido | Modifier | AttackSpeedBonus +0.1 | SkillPassiveApplicator | PARTIAL | modifier registered; combat hook deferred |
| ranged | ranged_kiting_steps | Passos de Kiting | Modifier | MoveSpeedBonus +0.05 | PlayerController | PARTIAL | modifier registered; movement speed hook deferred |
| ranged | ranged_projectile_tuning | Afinação de Projétil | Modifier | BowProjectileSpeedFlat +1 | SkillPassiveApplicator | PARTIAL | modifier registered; projectile hook deferred |
| ranged | ranged_capstone_eagle_focus | Foco da Águia | Capstone | BowRangeFlat +1, BowProjectileSpeedFlat +1 | SkillPassiveApplicator | PARTIAL | modifiers registered; ranged combat hook deferred |
| magic | magic_mana_well | Poço de Mana | Passive | MaxManaFlat +10 | StaminaManager (mana) | PARTIAL | modifier registered; mana system hook deferred |
| magic | magic_quick_channel | Canalização Rápida | Modifier | ManaRegenFlat +1 | StaminaManager (regen) | PARTIAL | modifier registered; mana regen hook deferred |
| magic | magic_arcane_edge | Fio Arcano | Passive | AttackFlat +1 | SkillPassiveApplicator | PARTIAL | modifier registered; magic damage hook deferred |
| magic | magic_arcane_bolt_mastery | Domínio do Raio Arcano | Upgrade/Modifier | AttackFlat +1 | SkillPassiveApplicator | PARTIAL | upgrade modifier registered; spell system hook deferred |
| magic | magic_capstone_elemental_confluence | Confluência Elemental | Capstone | AttackFlat +1, ManaRegenFlat +1 | SkillPassiveApplicator | PARTIAL | modifiers registered; elemental system hook deferred |
| survival | survival_cave_lungs | Pulmões da Caverna | Passive | MaxStaminaFlat +10 | StaminaManager | PARTIAL | modifier registered; stamina hook deferred |
| survival | survival_hard_skin | Pele Dura | Passive | MaxHPFlat +5 | HealthManager | PARTIAL | modifier registered; health hook deferred |
| survival | survival_low_rations | Rações Curtas | Modifier | HungerDrainReduction -0.1 | HungerManager | PARTIAL | modifier registered; hunger system hook deferred |
| survival | survival_toxic_sense | Senso Tóxico | Passive | ToxicResistanceBonus +1 | StatusEffectSystem | PARTIAL | modifier registered; status system hook deferred |
| survival | survival_cold_habit | Hábito do Frio | Passive | ColdResistanceBonus +1 | StatusEffectSystem | PARTIAL | modifier registered; status system hook deferred |
| survival | survival_heat_temper | Têmpera do Calor | Passive | HeatResistanceBonus +1 | StatusEffectSystem | PARTIAL | modifier registered; status system hook deferred |
| survival | survival_status_recovery | Recuperação Instintiva | Modifier | StatusDurationReduction -0.1 | StatusEffectSystem | PARTIAL | modifier registered; status system hook deferred |
| survival | survival_safe_step | Passo Seguro | Modifier | MoveSpeedBonus +0.05 | PlayerController | PARTIAL | modifier registered; movement hook deferred |
| survival | survival_capstone_caveborn | Nascido da Caverna | Capstone | ToxicRes+1, ColdRes+1, HeatRes+1, MaxStamina+5 | SkillPassiveApplicator | PARTIAL | modifiers registered; multiple hooks deferred |
| crafting | crafting_fast_hands | Mãos Ágeis | Modifier | CraftTimeReductionPercent -0.1 | CraftingSystem | PARTIAL | modifier registered; crafting system hook deferred |
| crafting | crafting_repair_care | Cuidado no Reparo | Modifier | RepairEfficiencyBonus +0.1 | RepairSystem | PARTIAL | modifier registered; repair system hook deferred |
| crafting | crafting_material_eye | Olho de Material | Modifier | (future resource bonus) | ResourceSystem | NO | FUTURE_DESIGN_PENDING |
| crafting | crafting_station_focus | Foco de Bancada | Modifier | CraftTimeReductionPercent -0.05 | CraftingSystem | PARTIAL | modifier registered; crafting system hook deferred |
| crafting | crafting_pack_order | Mochila Ordenada | Modifier | (future inventory sort) | InventoryManager | NO | FUTURE_DESIGN_PENDING |
| crafting | crafting_salvage_method | Método de Salvage | Modifier | (better salvage return) | LootSystem | NO | FUTURE_DESIGN_PENDING |
| crafting | crafting_durable_finish | Acabamento Durável | Modifier | (durability bonus) | CraftingSystem | NO | FUTURE_DESIGN_PENDING |
| crafting | crafting_shop_sense | Senso de Mercado | Modifier | (buy/sell bonus) | ShopManager | NO | FUTURE_DESIGN_PENDING |
| crafting | crafting_capstone_master_artisan | Mestre Artesão | Capstone | CraftTime -0.15, RepairEfficiency +0.15 | SkillPassiveApplicator | PARTIAL | modifiers registered; crafting/repair hooks deferred |

---

## Notes

```text
PARTIAL = modifier/stat registered in SkillPassiveApplicator or SkillEffectRegistry but
          the consuming system (combat, crafting, etc.) has not yet read it.
          The data is committed; integration is a future wave task.

NO = modifier not yet registered because design is not finalized or system does not exist.

FUTURE_DESIGN_PENDING = design intent exists but final values/triggers not defined.
```

---

*Created: 2026-06-08 (WAVE_INTEGRATION_11_ACTION_SKILL_BALANCE_PATCH)*
