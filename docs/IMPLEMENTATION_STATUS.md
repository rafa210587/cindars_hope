# Cindar's Hope - Implementation Status

> Status: tracking reconciliado pos-overnight.
> Fonte oficial de specs: `docs/specs/`.
> A pasta raiz `specs/` foi removida e nao deve ser recriada.

## Resumo

| Area | Status | Spec |
|---|---|---|
| Governanca documental / fonte unica | Implementado documental parcial | `docs/specs/implementados/spec_docs_001_single_source_specs_refinements_reconciliation_parcial.md` |
| Unity compile validation protocol | Implementado parcial | `docs/specs/implementados/spec_unity_compile_validation_protocol_and_scripts.md` |
| Core/event bus/bootstrap | Implementado parcial | `docs/specs/implementados/spec_core_001_event_bus_e_eventos_base.md`, `docs/specs/implementados/spec_core_002_bootstrap_managers_e_runtime_references.md` |
| Data/IDs/registries | Implementado | `docs/specs/implementados/spec_data_001_ids_registries_e_scriptableobjects.md` |
| Save/load JSON cross-scene | Implementado parcial | `docs/specs/implementados/spec_save_001_json_save_load_cross_scene.md` |
| Save schema migration v2 | Implementado parcial | `docs/specs/implementados/spec_save_002_schema_migration_v2.md` |
| Inventory slots/capacidade/UI minima | Implementado parcial | `docs/specs/implementados/spec_inventory_002_slots_capacity_ui_final.md` |
| Farm loop/world activities | Implementado parcial | `docs/specs/implementados/spec_farm_001_farm_scene_movimento_interacao.md`, `docs/specs/implementados/spec_farm_002_plots_seeds_growth_harvest.md`, `docs/specs/implementados/spec_farm_003_arvores_pesca_pickups_world_activities.md` |
| Economy/hunger/crafting/town | Implementado parcial | `docs/specs/implementados/spec_economy_001_compra_venda_gold_e_sellables.md`, `docs/specs/implementados/spec_hunger_001_fome_comida_e_hp_por_fome.md`, `docs/specs/implementados/spec_craft_001_crafting_mvp.md`, `docs/specs/implementados/spec_town_001_town_scene_portais_npc_pip_comercio.md` |
| Combat/damage/enemy stats | Implementado parcial | `docs/specs/implementados/spec_combat_001_slime_melee_contact_damage_drops.md`, `docs/specs/implementados/spec_combat_002_enemy_data_driven_stats.md`, `docs/specs/implementados/spec_damage_001_damage_formula_mvp.md` |
| UI/tools/hotbar/progression debug | Implementado parcial | `docs/specs/implementados/spec_ui_001_debug_hud_e_feedback_mvp.md`, `docs/specs/implementados/spec_ui_002_hud_tools_hotbar_progression_debug.md`, `docs/specs/implementados/spec_tools_001_tools_equipment_hotbar_parcial.md`, `docs/specs/implementados/spec_progression_001_xp_level_atributos_parcial.md` |
| Cave runtime/procedural/stable run/boss gates | Implementado em codigo - validacao Unity pendente | `docs/specs/implementados/spec_cave_001_cave_scene_portal_e_runtime_basico.md` ate `spec_cave_008_debug_skip_confinement_wall_distance_hardening.md` |
| Overnight 2026-05-23 | Executado parcialmente | `docs/IMPLEMENTATION_DELIVERY_20260523.md`, `docs/refinements/implementados/ref_stabilizacao_overnight_specs_20260523.md` |

## Correcoes de tracking obrigatorias

### Unity compile validation protocol

Implementado parcial em:

```text
docs/specs/implementados/spec_unity_compile_validation_protocol_and_scripts.md
tools/unity/RunUnityCompileValidation.ps1
tools/unity/ScanUnityLogs.ps1
```

Pendentes: MissingScriptScanner, SceneReferenceValidator, DataIdValidator e Play Mode automatizado completo.

Validacao local nesta entrega: Unity batchmode bloqueado por outra instancia do Unity aberta no mesmo projeto; compile Unity ainda nao validado localmente.

### Governanca documental

A antiga spec 00 de reconciliacao documental foi reclassificada como implementado documental parcial.

Nao executar novamente:

```text
docs/specs/a_implementar/spec_docs_single_source_specs_refinements_reconciliation_v1.md
docs/refinements/a_implementar/pre_refinamentos/refinamento_init_tracking_documental_status_specs.md
```

Usar como fonte ativa:

```text
docs/specs/implementados/spec_docs_001_single_source_specs_refinements_reconciliation_parcial.md
docs/refinements/implementados/ref_docs_single_source_tracking_reconciliation_parcial.md
```

### Inventory

O inventory atual usa slots reais com capacidade inicial 18 e limite 30, mantendo `Items` agregado apenas como compatibilidade para sistemas antigos.
Existem multiplas stacks por item e migration `v1 -> v2` para `InventorySaveData`.
Pendentes: `Use` especifico por tipo de item, Drop transacional com spawner persistente, drag/drop, sort/auto-organize, UI Canvas final e binding completo por `ItemInstanceId`.

### Progression

SkillPoint a cada 2 niveis: implementado no codigo.
Regra atual: +1 SkillPoint em niveis pares, comecando no level 2.
AttributePoint: +1 por level up.
Pendentes: gasto/distribuicao final de atributos, skill trees completas, active slots, capstones, respec Fonte de Anya e save/load completo.

## Specs futuras

A ordem oficial esta em `docs/specs/SPEC_EXECUTION_ORDER.md` e o registry futuro em `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`.

A primeira spec futura executavel passa a ser:

```text
docs/specs/a_implementar/spec_farm_irrigacao_solo_planting_ui.md
```

Antes de executar runtime, as specs futuras devem ser enriquecidas usando seus pre-refinamentos relacionados.

FASE9H/I/J/K/L nao devem ser tratadas como completas. As proximas implementacoes devem seguir somente `docs/specs/`.
