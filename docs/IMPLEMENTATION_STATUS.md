# Cindar's Hope - Implementation Status

> Status: tracking reconciliado pos-overnight.
> Fonte oficial de specs: `docs/specs/`.
> A pasta raiz `specs/` foi removida e nao deve ser recriada.

## Resumo

| Area | Status | Spec |
|---|---|---|
| Governanca documental / fonte unica | Implementado documental parcial | `docs/specs/implementados/spec_docs_001_single_source_specs_refinements_reconciliation_parcial.md` |
| Unity compile validation protocol | Implementado completo | `docs/specs/implementados/spec_unity_compile_validation_protocol_and_scripts.md` |
| Core/event bus/bootstrap | Implementado parcial | `docs/specs/implementados/spec_core_001_event_bus_e_eventos_base.md`, `docs/specs/implementados/spec_core_002_bootstrap_managers_e_runtime_references.md` |
| Data/IDs/registries | Implementado | `docs/specs/implementados/spec_data_001_ids_registries_e_scriptableobjects.md` |
| Save/load JSON cross-scene | Implementado parcial | `docs/specs/implementados/spec_save_001_json_save_load_cross_scene.md` |
| Save schema migration v2 | Implementado completo | `docs/specs/implementados/spec_save_002_schema_migration_v2.md` |
| Inventory slots/capacidade/UI minima | Implementado completo | `docs/specs/implementados/spec_inventory_002_slots_capacity_ui_final.md` |
| Farm irrigacao/solo/planting UI | Implementado completo | `docs/specs/implementados/spec_farm_004_irrigacao_solo_planting_ui.md` |
| World activities/fishing/trees/loot | Implementado completo | `docs/specs/implementados/spec_world_002_activities_fishing_trees_pickups_loot.md` |
| Farm loop/world activities | Implementado parcial | `docs/specs/implementados/spec_farm_001_farm_scene_movimento_interacao.md`, `docs/specs/implementados/spec_farm_002_plots_seeds_growth_harvest.md`, `docs/specs/implementados/spec_farm_003_arvores_pesca_pickups_world_activities.md` |
| Game time / day-night cycles / hunger-stamina balance | Implementado parcial | `docs/specs/implementados/spec_hunger_stamina_status_balance.md` |
| Economy/hunger/crafting/town | Implementado parcial | `docs/specs/implementados/spec_economy_001_compra_venda_gold_e_sellables.md`, `docs/specs/implementados/spec_hunger_001_fome_comida_e_hp_por_fome.md`, `docs/specs/implementados/spec_craft_001_crafting_mvp.md`, `docs/specs/implementados/spec_town_001_town_scene_portais_npc_pip_comercio.md` |
| Combat/damage/enemy stats | Implementado parcial | `docs/specs/implementados/spec_combat_001_slime_melee_contact_damage_drops.md`, `docs/specs/implementados/spec_combat_002_enemy_data_driven_stats.md`, `docs/specs/implementados/spec_damage_001_damage_formula_mvp.md` |
| UI/tools/hotbar/progression debug | Implementado parcial | `docs/specs/implementados/spec_ui_001_debug_hud_e_feedback_mvp.md`, `docs/specs/implementados/spec_ui_002_hud_tools_hotbar_progression_debug.md`, `docs/specs/implementados/spec_tools_001_tools_equipment_hotbar_parcial.md`, `docs/specs/implementados/spec_progression_001_xp_level_atributos_parcial.md` |
| Cave runtime/procedural/stable run/boss gates | Implementado em codigo - validacao Unity pendente | `docs/specs/implementados/spec_cave_001_cave_scene_portal_e_runtime_basico.md` ate `spec_cave_008_debug_skip_confinement_wall_distance_hardening.md` |
| Overnight 2026-05-23 | Executado parcialmente | `docs/IMPLEMENTATION_DELIVERY_20260523.md`, `docs/refinements/implementados/ref_stabilizacao_overnight_specs_20260523.md` |
| Economy/shop/stock/pricing/UI (Spec 06) | Implementado completo | `docs/specs/implementados/spec_economy_shop_stock_pricing_ui.md` |
| Crafting queue/workstations/recipes/UI (Spec 07) | Implementado completo | `docs/specs/implementados/spec_crafting_queue_workstations_recipes_ui.md` |

## Correcoes de tracking obrigatorias

### Unity compile validation protocol

Implementado completo em:

```text
docs/specs/implementados/spec_unity_compile_validation_protocol_and_scripts.md
tools/unity/RunUnityCompileValidation.ps1
tools/unity/ScanUnityLogs.ps1
tools/docs/validate_docs.ps1 (corrigido)
```

**Status 2026-05-24:**
- Scripts PowerShell: Funcionais e operáveis
- Validação documental: PASS
- Validação Unity batchmode: NOT RUN (sandbox; aceitável)
- Gaps de Play Mode/Advanced Scanners: Reclassificados para futuro

Specs futuras dedicadas aos scanners avançados (Missing Script, Scene References, Data IDs) podem ser implementadas quando necessário, sem bloquear SPECS 02-17.

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

### Game Time / Hunger-Stamina Balance (SPEC 09)

GameTimeManager entregue com:
- Ciclo dia/noite configurable (DayDurationSeconds=600, NightDurationSeconds=300)
- GameTimeTickEvent publicado a cada 1 segundo para sistemas dependentes
- Pause-aware via ModalManager.HasActiveModal
- Save/load com GameTimeSaveData (CurrentDay, CurrentPhase, PhaseElapsedSeconds)

Stamina regeneration integrada:
- 4 fome tiers com modifiers: Normal (1.0), Fome (0.6), Fome Crítica (0.3), Vazio (0.0 com damaged mode)
- Modo danificado com -2 stamina/s quando fome=0
- PlayerNeedsBalanceSO drive configuração

SaveData v3 migration (SaveV2ToV3Migration):
- Converte EquipmentDurabilityData de Dictionary para List<DurabilityEntryData>
- Inicializa GameTimeSaveData e PlayerStatusEffectsSaveData para saves v2 legados
- Sem perda de dados

StatusEffectManager básico:
- Tracking de efeitos ativos por ID
- Evento GameTimeTickEvent para decay/expiry
- Save/load com List<StatusEffectEntryData>

PlayerNeedsHUD mínimo:
- Barra de fome (Hunger%)
- Barra de stamina (Stamina%)
- Texto de efeitos ativos (até 3)
- Subscriber de HungerChangedEvent e StaminaChangedEvent

Pendentes (spec 17):
- Canvas consolidado final com styling/layout
- Painel de dificuldade
- HUD com fonts/spacing definidos

### Progression

SkillPoint a cada 2 niveis: implementado no codigo.
Regra atual: +1 SkillPoint em niveis pares, comecando no level 2.
AttributePoint: +1 por level up.
Pendentes: gasto/distribuicao final de atributos, skill trees completas, active slots, capstones, respec Fonte de Anya e save/load completo.

## Specs futuras

A ordem oficial esta em `docs/specs/SPEC_EXECUTION_ORDER.md` e o registry futuro em `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`.

A primeira spec futura executavel passa a ser:

```text
docs/specs/a_implementar/spec_town_npc_dialogue_schedule_quests.md
```

Antes de executar runtime, as specs futuras devem ser enriquecidas usando seus pre-refinamentos relacionados.

FASE9H/I/J/K/L nao devem ser tratadas como completas. As proximas implementacoes devem seguir somente `docs/specs/`.
