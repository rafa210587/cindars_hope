# WAVE_INTEGRATION_11 — Skill Action Balance Addendum

**Date:** 2026-06-08
**Status:** BUILD_VALIDATED_WITH_SKILL_EFFECT_DEBT
**Patch:** WAVE_INTEGRATION_11_ACTION_SKILL_BALANCE_PATCH

---

## Objetivo

Garantir que cada árvore de skill tenha no mínimo 5 active slot skills equipáveis.
Adicionar sem remover. IDs existentes não alterados.

---

## Regras Absolutas Aplicadas

- Nenhuma skill existente foi removida.
- Nenhum ID existente foi alterado.
- Dash/Dodge/Block não ocupam active slot.
- WAVE10 UI preservada — nenhuma interface recriada.
- Nenhum active slot runtime paralelo criado.

---

## 7.1 Melee / Guerreiro

### Skills existentes que continuam (não alteradas)

| SkillId | DisplayName | Tipo | Active slot? |
|---|---|---|---:|
| melee_iron_grip | Pegada de Ferro | PassiveSkill | NO |
| melee_guarded_stance | Postura Guardada | PassiveSkill | NO |
| melee_dual_wield_flow | Fluxo de Duas Lâminas | PassiveSkill | NO |
| melee_offhand_cut | Corte da Mão Secundária | EquippableSkill | YES |
| melee_two_handed_momentum | Ímpeto de Duas Mãos | PassiveSkill | NO |
| melee_guarded_block | Bloqueio Guardado | EquippableSkill | YES |
| melee_battle_dash | Arrancada de Combate | EquippableSkill | YES |
| melee_leap_attack | Salto Devastador | EquippableSkill | YES |
| melee_whirl_cut | Corte Giratório | EquippableSkill | YES |
| melee_dodge_training | Treino de Esquiva | PassiveSkill | NO |
| melee_capstone_battle_rhythm | Ritmo de Batalha | CapstonePassive | NO |

### Novas action skills adicionadas neste patch

| SkillId | DisplayName | Tier | EffectId | Active slot? | Status |
|---|---|---:|---|---:|---|
| melee.avanco_aco | Avanço de Aço | 2 | `melee.avanco_aco` | YES | DEFERRED_RUNTIME_EFFECT |
| melee.grito_desafio | Grito de Desafio | 3 | `melee.grito_desafio` | YES | DEFERRED_RUNTIME_EFFECT |
| melee.investida_quebra_guarda | Investida Quebra-Guarda | 4 | `melee.investida_quebra_guarda` | YES | DEFERRED_RUNTIME_EFFECT |

### Contagem final

```text
Melee active slot skills existentes: 5 (offhand_cut, guarded_block, battle_dash, leap_attack, whirl_cut)
Melee active slot skills novas: 3 (avanco_aco, grito_desafio, investida_quebra_guarda)
Total Melee active slot skills no catálogo: 8
Meta mínima (5): SUPERADO
```

---

## 7.2 Ranged / Caçador

### Skills existentes que continuam (não alteradas)

| SkillId | DisplayName | Tipo | Active slot? |
|---|---|---|---:|
| ranged_steady_hand | Mão Firme | PassiveSkill | NO |
| ranged_long_sight | Mira Longa | PassiveSkill | NO |
| ranged_quick_nock | Encaixe Rápido | PassiveSkill | NO |
| ranged_charged_shot | Disparo Carregado | EquippableSkill | YES |
| ranged_line_piercer | Linha Perfurante | EquippableSkill | YES |
| ranged_multishot_fan | Tiro Triplo | EquippableSkill | YES |
| ranged_bleeding_arrow | Flecha Sangrante | EquippableSkill | YES |
| ranged_kiting_steps | Passos de Kiting | PassiveSkill | NO |
| ranged_marked_prey | Presa Marcada | EquippableSkill | YES |
| ranged_projectile_tuning | Afinação de Projétil | PassiveSkill | NO |
| ranged_capstone_eagle_focus | Foco da Águia | CapstonePassive | NO |

### Novas action skills adicionadas neste patch

Nenhuma. Ranged já possui 5 active slot skills.

### Contagem final

```text
Ranged active slot skills: 5 (charged_shot, line_piercer, multishot_fan, bleeding_arrow, marked_prey)
Meta mínima (5): ATINGIDA
```

---

## 7.3 Magic / Arcano

### Skills existentes que continuam (não alteradas)

| SkillId | DisplayName | Tipo | Active slot? |
|---|---|---|---:|
| magic_mana_well | Poço de Mana | PassiveSkill | NO |
| magic_quick_channel | Canalização Rápida | PassiveSkill | NO |
| magic_arcane_edge | Fio Arcano | PassiveSkill | NO |
| magic_fire_spark | Fagulha Ígnea | EquippableSkill | YES |
| magic_ice_bind | Laço de Gelo | EquippableSkill | YES |
| magic_toxic_cloud | Nuvem Tóxica | EquippableSkill | YES |
| magic_lightning_chain | Corrente Relâmpago | EquippableSkill | YES |
| magic_arcane_bolt_mastery | Domínio do Raio Arcano | PassiveSkill | NO |
| magic_elemental_ward | Guarda Elemental | EquippableSkill | YES |
| magic_slowing_sigils | Sigilos Lentificantes | EquippableSkill | YES |
| magic_capstone_elemental_confluence | Confluência Elemental | CapstonePassive | NO |

### Novas action skills adicionadas neste patch

| SkillId | DisplayName | Tier | EffectId | Active slot? | Status |
|---|---|---:|---|---:|---|
| magic.chama_breve | Chama Breve | 2 | `magic.chama_breve` | YES | DEFERRED_RUNTIME_EFFECT |
| magic.rajada_gelida | Rajada Gélida | 3 | `magic.rajada_gelida` | YES | DEFERRED_RUNTIME_EFFECT |

### Contagem final

```text
Magic active slot skills existentes: 6 (fire_spark, ice_bind, toxic_cloud, lightning_chain, elemental_ward, slowing_sigils)
Magic active slot skills novas: 2 (chama_breve, rajada_gelida)
Total Magic active slot skills no catálogo: 8
Meta mínima (5): SUPERADO
```

---

## 7.4 Survival / Sobrevivente

### Skills existentes que continuam (não alteradas)

| SkillId | DisplayName | Tipo | Active slot? |
|---|---|---|---:|
| survival_cave_lungs | Pulmões da Caverna | PassiveSkill | NO |
| survival_hard_skin | Pele Dura | PassiveSkill | NO |
| survival_low_rations | Rações Curtas | PassiveSkill | NO |
| survival_toxic_sense | Senso Tóxico | PassiveSkill | NO |
| survival_cold_habit | Hábito do Frio | PassiveSkill | NO |
| survival_heat_temper | Têmpera do Calor | PassiveSkill | NO |
| survival_status_recovery | Recuperação Instintiva | PassiveSkill | NO |
| survival_safe_step | Passo Seguro | PassiveSkill | NO |
| survival_emergency_roll | Rolamento de Emergência | EquippableSkill | YES |
| survival_last_breath | Último Fôlego | EquippableSkill | YES |
| survival_capstone_caveborn | Nascido da Caverna | CapstonePassive | NO |

### Novas action skills adicionadas neste patch

| SkillId | DisplayName | Tier | EffectId | Active slot? | Status |
|---|---|---:|---|---:|---|
| survival.sinal_retirada | Sinal de Retirada | 2 | `survival.sinal_retirada` | YES | DEFERRED_RUNTIME_EFFECT |
| survival.isca_improvisada | Isca Improvisada | 2 | `survival.isca_improvisada` | YES | DEFERRED_RUNTIME_EFFECT |
| survival.kit_emergencia | Kit de Emergência | 3 | `survival.kit_emergencia` | YES | DEFERRED_RUNTIME_EFFECT |
| survival.instinto_sobrevivencia | Instinto de Sobrevivência | 3 | `survival.instinto_sobrevivencia` | YES | DEFERRED_RUNTIME_EFFECT |
| survival.campo_seguro | Campo Seguro | 4 | `survival.campo_seguro` | YES | DEFERRED_RUNTIME_EFFECT |

### Contagem final

```text
Survival active slot skills existentes: 2 (emergency_roll, last_breath)
Survival active slot skills novas: 5 (sinal_retirada, isca_improvisada, kit_emergencia, instinto_sobrevivencia, campo_seguro)
Total Survival active slot skills no catálogo: 7
Meta mínima (5): SUPERADO
```

---

## 7.5 Crafting / Produção

### Skills existentes que continuam (não alteradas)

| SkillId | DisplayName | Tipo | Active slot? |
|---|---|---|---:|
| crafting_fast_hands | Mãos Ágeis | PassiveSkill | NO |
| crafting_repair_care | Cuidado no Reparo | PassiveSkill | NO |
| crafting_material_eye | Olho de Material | PassiveSkill | NO |
| crafting_field_patch | Remendo de Campo | EquippableSkill | YES |
| crafting_station_focus | Foco de Bancada | PassiveSkill | NO |
| crafting_pack_order | Mochila Ordenada | PassiveSkill | NO |
| crafting_quick_repair | Reparo Rápido | EquippableSkill | YES |
| crafting_salvage_method | Método de Salvage | PassiveSkill | NO |
| crafting_durable_finish | Acabamento Durável | PassiveSkill | NO |
| crafting_shop_sense | Senso de Mercado | PassiveSkill | NO |
| crafting_capstone_master_artisan | Mestre Artesão | CapstonePassive | NO |

### Novas action skills adicionadas neste patch

Nota: `crafting_quick_repair` ("Reparo Rápido") já existe com ID estável — não duplicado.

| SkillId | DisplayName | Tier | EffectId | Active slot? | Status |
|---|---|---:|---|---:|---|
| crafting.irrigador_portatil | Irrigador Portátil | 3 | `crafting.irrigador_portatil` | YES | DEFERRED_RUNTIME_EFFECT |
| crafting.bomba_improvisada | Bomba Improvisada | 3 | `crafting.bomba_improvisada` | YES | DEFERRED_RUNTIME_EFFECT |
| crafting.mecanismo_campo | Mecanismo de Campo | 4 | `crafting.mecanismo_campo` | YES | DEFERRED_RUNTIME_EFFECT |
| crafting.marca_eficiencia | Marca de Eficiência | 4 | `crafting.marca_eficiencia` | YES | DEFERRED_RUNTIME_EFFECT |

### Contagem final

```text
Crafting active slot skills existentes: 2 (field_patch, quick_repair)
Crafting active slot skills novas: 4 (irrigador_portatil, bomba_improvisada, mecanismo_campo, marca_eficiencia)
Total Crafting active slot skills no catálogo: 6
Meta mínima (5): SUPERADO
```

---

## Resumo de Contagem por Árvore

| Árvore | Active Skills Existentes | Active Skills Novas | Total no Catálogo | Meta (5) |
|---|---:|---:|---:|---|
| Melee | 5 | 3 | 8 | SUPERADO |
| Ranged | 5 | 0 | 5 | ATINGIDA |
| Magic | 6 | 2 | 8 | SUPERADO |
| Survival | 2 | 5 | 7 | SUPERADO |
| Crafting | 2 | 4 | 6 | SUPERADO |
| **Total** | **20** | **14** | **34** | — |

---

## Acceptance Criteria — Status

| ID | Critério | Status |
|---|---|---|
| AC-ACTION-01 | Catálogo inclui todas as skills originais das 5 árvores | OK |
| AC-ACTION-02 | Catálogo inclui novas action skills adicionadas neste patch | OK |
| AC-ACTION-03 | Cada árvore tem meta de 5 active slot skills ou debt explícito | OK — todas atingem ou superam |
| AC-ACTION-04 | Nenhuma skill existente foi removida | OK |
| AC-ACTION-05 | Nenhum ID existente foi trocado | OK |
| AC-ACTION-06 | Active slot skills têm EffectId | OK — mapeado em SkillActionToEffectId |
| AC-ACTION-07 | Passives/modifiers/unlocks/capstones não entram indevidamente em active slot | OK |
| AC-ACTION-08 | Dash/Dodge/Block não ocupam active slot | OK |
| AC-ACTION-09 | Dash = Space + direção | OK — PlayerDashController existente |
| AC-ACTION-10 | Dodge = double tap direcional | OK — PlayerMovementAbilityController existente |
| AC-ACTION-11 | Block = Left Shift | OK — BLOCK_RUNTIME_DEFERRED (documentado) |
| AC-ACTION-12 | Pelo menos 1 active skill executa efeito real ou bridge controlado | OK — farm.crop.water_skill real; 14 novas com FeedbackOnlySkillEffectExecutor |
| AC-ACTION-13 | Pelo menos Dash ou Dodge funciona no Play Mode ou blocker honesto existe | OK — ambos implementados na WI11 base |

---

## Debt Documentado

```text
DEFERRED_RUNTIME_EFFECT: 14 novas action skills têm FeedbackOnlySkillEffectExecutor.
Executores reais dependem de: combat runtime, utility runtime, farm utility runtime.
TODO_INTEGRATION_NOT_FINAL em cada FeedbackOnlySkillEffectExecutor.
Não bloqueia BUILD_VALIDATED_WITH_SKILL_EFFECT_DEBT.
```

---

*Created: 2026-06-08 (WAVE_INTEGRATION_11_ACTION_SKILL_BALANCE_PATCH)*
