# FABLE — Índice e Gap Analysis (Design Directions vs. Implementação)

> **Tipo:** índice de lote / análise de lacunas — NÃO é spec executável.
> **Data:** 2026-06-12
> **Gerado por:** revisão completa de `docs/design/**` (42 directions) contra o código real
> (`Assets/_Game/Scripts/**`), waves executadas (00-12 + WAVE_INTEGRATION 01-26) e
> `SPEC_REGISTRY_TO_IMPLEMENT.md`.
> **Local das specs:** `docs/specs/a_implementar/fable/` — fora da fila executável automática
> (mesmo padrão de `features_futuras/`). Promover ao nível raiz + registry quando autorizado.

---

## 1. Veredicto por direction

### Implementadas (núcleo coberto por waves; sem spec fable necessária)

| Direction | Cobertura |
|---|---|
| SEASONS_CALENDAR_WEATHER_LUNAR | WAVE 02 runtime completo; UI de calendário já tem spec na fila (`02_spec_calendar_ui_weather_lunar_display.md`) |
| QUEST_OBJECTIVE_EVENT_SYSTEM | WAVE 03/09 + WI-15/26 (7 tipos de objetivo, reward idempotente, save WI-18) |
| FARM_DESIGN v1.0/v1.3 (núcleo) | WAVE 05 (20 specs) + WI-24 (daily goals) + slice 2026-06-12 (24 canteiros) |
| FARM_LAYOUT_SCALE_BUILDINGS | WAVE 05 + geradores de cena; construção via CraftingPoint (WI-14) |
| LOOT_CRAFTING_ECONOMY (núcleo) | WAVE 06 (recipes, shops, crafting stations, sell) |
| ECONOMY_PRICING_STOCK_REFRESH (núcleo) | WAVE 06 + FarmShippingService/ShippingPriceResolver + FIX-001B (25 ShopDataSO) |
| CITY_DESIGN / CITY_NPC_ROSTER v1.1 | WAVE 08 + WI-12/12C (23 NPCs) + WI-25 (schedules) + slice 2026-06-12 (distritos/diálogos 23×13) |
| QUESTS_MAIN_LORE / MAIN_PROGRESSION (estado) | WAVE 10 (atos, fragmentos, Fonte/Anya state) — conteúdo jogável é lacuna (ver F10) |
| SAVE_LOAD_FULL_STATE (núcleo) | WAVE 01 + WI-18 (quests) — débitos residuais são lacuna (ver F13) |
| PLAYER_CORE_SYSTEMS | HP/stamina/mana/fome/fadiga/sono (WAVE 05) + Dash/Dodge/Block (WI-11) |
| PLAYER_SKILL_TREES | 69 nodes / 5 árvores + 21 executores reais (slice 2026-06-12) |
| UI_UX_CENTRALIZATION_SOURCE_REFS | governança apenas — aplicada nas regras de leitura |
| CAVE_DESIGN / CAVE_LEVEL_GENERATION (núcleo) | cave_001-008 (estável por seed, 101 níveis, checkpoints, gates) — variedade de bioma é lacuna (ver F09) |
| CAVE_MONSTER_ROSTER (dados) | geradores CreateRoster40EnemyData/CreateEnemyActionsAndSets/CreateDefaultEnemyProfiles existem — consumo de loot tables é lacuna (ver F06) |
| ENEMY_BEHAVIORS (movimento) | EnemyBrain com 10 MovementTypes + projéteis inimigos (slice 2026-06-12) — threat/pack/boss são lacunas (F04/F05) |

### Cobertas por specs já existentes em `features_futuras/` (NÃO duplicadas aqui)

| Direction | Specs existentes | Decisão pendente |
|---|---|---|
| BESTIARY_KNOWLEDGE_DISCOVERY | 4 specs WAVE 13 (`13_spec_bestiary_*`) + 4 WAVE 22 | Humano deve mover de volta à fila |
| COMPANIONS | 4 specs WAVE 14 (`14_spec_companion_*`) | Humano deve mover de volta à fila |
| PETS | 4 specs WAVE 23 (`23_spec_pet_*`) | **HOLD/BLOCKED_SCOPE — não executar** |
| SOCIAL_RELATIONSHIP_ROMANCE | 4 specs WAVE 17 (`17_spec_social_*`) | Futuro por decisão do SPEC_SOURCE_MAP |
| Nível 100/101 / finais | 4 specs WAVE 19 + 4 WAVE 16 (`19_spec_level_*`, `16_spec_*`) | Futuro |
| Mana cultivável / endgame farm | 4 specs WAVE 20 (`20_spec_mana_*`) | Futuro |
| Automação total da fazenda | 4 specs WAVE 21 (`21_spec_farm_automation_*`) | Futuro |
| Festivais/minigames/economia sazonal | 4 specs WAVE 15 + 1 WAVE 24 | Futuro |
| CAVE_MONSTER_VISUAL_SPRITE | — | **Bloqueada por fase de arte 2D (não iniciada)** |

### Lacunas reais → specs FABLE geradas neste lote

| # | Spec | Direction(s) fonte | Lacuna confirmada no código |
|---|---|---|---|
| F01 | `fable_01_spec_status_effects_canonical_set_runtime.md` | STATUS_EFFECTS | `StatusEffectType` tem só Poison/Burn/Bleed/Stun; faltam Chill/Root/Fear/ConfusionLite/DurabilityStress/Corruption; skills/spells não aplicam status (hook `statusEffectId` sem assets/IDs) |
| F02 | `fable_02_spec_combat_weapon_actions_derived_stats_runtime.md` | COMBAT_CORE, PLAYER_DERIVED_ATTRIBUTES | `PlayerAttackController` usa `weapon.BaseDamage` cru; `DerivedStatsCalculator` existe mas Attack/AttackSpeed não entram no combate; sem light/heavy/charged nem stagger |
| F03 | `fable_03_spec_equipment_mechanical_baselines_runtime.md` | EQUIPMENT_MECHANICAL_BASELINES, EQUIPMENT_WEAPONS_ARMOR_MATERIALS | WeaponDataSO sem ASPD/scaling/charged profile; armor/shield baselines não aplicados a dano recebido |
| F04 | `fable_04_spec_enemy_threat_pack_coordination_runtime.md` | ENEMY_BEHAVIORS | EnemyBrain não tem threat/aggro/target priority nem coordenação de pack/leash compartilhado |
| F05 | `fable_05_spec_cave_boss_phase_ai_runtime.md` | ENEMY_BEHAVIORS, CAVE_COMBAT_BALANCE | CaveBoss* cobre gates/defeat/spawn; nenhuma IA de fases (thresholds de HP, action sets por fase, vulnerability windows por fase) |
| F06 | `fable_06_spec_enemy_loot_tables_vulnerability_tags_runtime.md` | LOOT_CRAFTING_ECONOMY, EQUIPMENT_ENEMY_VULNERABILITY_ADAPTER, CAVE_MONSTER_ROSTER | `EnemyDataSO.lootTableId` existe mas `EnemyDropSpawner` usa `dropItemId` direto; LootTableSO não consumido por inimigos; Material/Element vulnerability tags sem matching equipment↔enemy |
| F07 | `fable_07_spec_magic_learning_unlock_sources_runtime.md` | MAGIC_LEARNING_UNLOCKS_SOURCES | Nenhum knownSpellIds/LearnableScroll/CastScroll/Tome/Focus; magia hoje = item Magic equipado apenas |
| F08 | `fable_08_spec_magic_spell_shapes_targeting_runtime.md` | MAGIC_SPELLS_ACTIONS | Toda magia é projétil único linear; sem shapes (cone/nova/self/barrier/heal) nem cast time/interrupt |
| F09 | `fable_09_spec_cave_biome_layout_variety_runtime.md` | CAVE_LEVEL_GENERATION_LAYOUT_BIOME | Bandas de bioma existem só como tags de spawn; layout/hazards/salas de tesouro não variam por bioma |
| F10 | `fable_10_spec_main_quest_act1_playable_runtime.md` | QUESTS_MAIN_LORE, QUESTS_MAIN_PROGRESSION, QUESTS_LORE_WEAVING | Estado de atos/fragmentos (WAVE 10) sem quests jogáveis; QuestRegistry tem só 3 quests utilitárias |
| F11 | `fable_11_spec_city_interiors_doors_schedule_anchors_scene.md` | CITY_LAYOUT_BUILDINGS_SCHEDULE | Casas são fachadas sem porta funcional/interior; NpcScheduleAnchor nunca colocado em cena (SCENE_WIRING_DEBT WI-25); schedule sem blocos por hora (TIME_BLOCK_DEBT) |
| F12 | `fable_12_spec_farm_animals_runtime_scene_integration.md` | FARM_DESIGN v1.3 | Só `AnimalHousingCapacityState` existe; sem FarmAnimal runtime, alimentação, produtos, coleta, cena |
| F13 | `fable_13_spec_save_debt_closure_runtime.md` | SAVE_LOAD_FULL_STATE | Débitos documentados: CAVE_ENEMY_HP_SAVE_DEBT (WI-18), SAVE_LOAD_DAILY_GOAL_DEBT (WI-24), CaveRun via cache não persistido no SaveManager (WI-16) |
| F14 | `fable_14_spec_ui_canvas_screens_integration_runtime.md` | UI_UX_FULL_GAMEPLAY, UI_UX_MENU_SCREEN_FLOWS | 11 specs WAVE 04 CONTRACT_ONLY; WI-23 criou views headless sem Canvas; inventário/equipment/skill tree são IMGUI debug |

---

## 1B. CORREÇÃO PÓS-AUDITORIA (fable_00B, 2026-06-12)

A tabela "Implementadas" acima foi parcialmente invalidada por auditoria de código real:
vários módulos das waves estão **órfãos** (jamais instanciados). Ver
`fable_00B_adherence_audit_queue_triage.md`. Corretivas: **F15** (clima/chuva/refresh/
qualidade/fertilizante), **F16** (fadiga/sono/colapso), **F17** (Fonte física), **F18**
(vitals derivados), **F19** (dedup schedule + serviços urbanos), **F20** (calendar UI,
absorve a 02_spec da fila antiga). Ordem corrigida: executar F15-F17 (P0) antes dos blocos originais.

## 2. Ordem recomendada e dependências

```text
Bloco A (combate, sequencial): F01 → F02 → F03 → F06
Bloco B (inimigos, sequencial): F04 → F05            (B pode rodar em paralelo com A até F06)
Bloco C (magia, sequencial):    F07 → F08            (depende de F01 para status de spells)
Bloco D (mundo, paralelo entre si): F09, F11, F12
Bloco E (conteúdo): F10                              (depende apenas do quest system atual)
Bloco F (fundação, sem paralelo): F13 (save schema), F14 (UI compartilhada)
```

Regra: F13 nunca em paralelo com qualquer spec que adicione estado persistido (F07, F12).
F14 nunca em paralelo com specs que toquem HUD/modal compartilhado.

## 3. Como promover

1. Humano autoriza o lote (ou subconjunto).
2. Mover `fable_NN_spec_*.md` para `docs/specs/a_implementar/` renomeando para `spec_fable_NN_*.md`
   (prefixo `spec_` exigido pelo `validate_docs.ps1` no nível raiz) OU manter na subpasta e
   executar por caminho explícito.
3. Registrar no `SPEC_REGISTRY_TO_IMPLEMENT.md` (seção FABLE já criada).
4. Executar via `/implement-spec` com validação `run_strict_validation.ps1`.
