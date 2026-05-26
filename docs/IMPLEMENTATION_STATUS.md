# Cindar's Hope - Implementation Status

> Status: tracking reconciliado pos-overnight.
> Fonte oficial de specs: `docs/specs/`.
> A pasta raiz `specs/` foi removida e nao deve ser recriada.

## Resumo

| Area | Status | Spec |
|---|---|---|
| Claude Code project structure (.claude/) | Implementado completo | `.claude/settings.json`, `.claude/commands/`, `.claude/skills/`, `.claude/agents/`, `.claude/hooks/` |
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
| Game time / day-night cycles / hunger-stamina balance | Implementado completo | `docs/specs/implementados/spec_hunger_stamina_status_balance.md` |
| Economy/hunger/crafting/town | Implementado parcial | `docs/specs/implementados/spec_economy_001_compra_venda_gold_e_sellables.md`, `docs/specs/implementados/spec_hunger_001_fome_comida_e_hp_por_fome.md`, `docs/specs/implementados/spec_craft_001_crafting_mvp.md`, `docs/specs/implementados/spec_town_001_town_scene_portais_npc_pip_comercio.md` |
| Combat/damage/enemy stats | Implementado parcial | `docs/specs/implementados/spec_combat_001_slime_melee_contact_damage_drops.md`, `docs/specs/implementados/spec_combat_002_enemy_data_driven_stats.md`, `docs/specs/implementados/spec_damage_001_damage_formula_mvp.md` |
| UI/tools/hotbar/progression debug | Implementado parcial | `docs/specs/implementados/spec_ui_001_debug_hud_e_feedback_mvp.md`, `docs/specs/implementados/spec_ui_002_hud_tools_hotbar_progression_debug.md`, `docs/specs/implementados/spec_tools_001_tools_equipment_hotbar_parcial.md`, `docs/specs/implementados/spec_progression_001_xp_level_atributos_parcial.md` |
| Cave runtime/procedural/stable run/boss gates | Implementado em codigo - validacao Unity pendente | `docs/specs/implementados/spec_cave_001_cave_scene_portal_e_runtime_basico.md` ate `spec_cave_008_debug_skip_confinement_wall_distance_hardening.md` |
| Overnight 2026-05-23 | Executado parcialmente | `docs/IMPLEMENTATION_DELIVERY_20260523.md`, `docs/refinements/implementados/ref_stabilizacao_overnight_specs_20260523.md` |
| Economy/shop/stock/pricing/UI (Spec 06) | Implementado completo | `docs/specs/implementados/spec_economy_shop_stock_pricing_ui.md` |
| Crafting queue/workstations/recipes/UI (Spec 07) | Implementado completo | `docs/specs/implementados/spec_crafting_queue_workstations_recipes_ui.md` |
| Town NPC/dialogue/wanderer/save hooks (Spec 08) | Implementado completo | `docs/specs/implementados/spec_town_npc_dialogue_schedule_quests.md` |
| Hunger/stamina/status/time (Spec 09) | Implementado completo | `docs/specs/implementados/spec_hunger_stamina_status_balance.md` |
| Equipment/durability/environment/loot (Spec 10) | Implementado parcial | `docs/specs/implementados/spec_equipment_durability_environment_loot_runtime.md` |
| Damage/status/elements/resistances (Spec 11) | Implementado parcial | `docs/specs/implementados/spec_damage_status_elements_resistances_runtime.md` |
| Player Combat/weapons/spells/skill actions (Spec 12) | Implementado parcial | `docs/specs/implementados/spec_player_combat_weapons_spells_skill_actions_runtime.md` |
| UI/Input/Shop/Sell Bugfix Bundle (Post-SPEC 12) | Implementado completo | `docs/specs/implementados/spec_bugfix_ui_input_shop_sell_bundle.md` |
| Enemy AI/roster/bestiary/faction locks (Spec 13) | Implementado parcial | `docs/specs/implementados/spec_enemy_ai_roster_bestiary_faction_locks_runtime.md` |
| Skill trees/active slots/respec Anya (Spec 16) | Implementado em codigo - Play Mode humano pendente | `docs/specs/implementados/spec_skill_trees_active_slots_respec_anya_runtime.md` |
| UI Gameplay MVP: shops/sell/equipment/attributes/skills (Spec 17 incremento) | Implementado em codigo - Play Mode humano pendente; SPEC 17B ampla permanece aberta | `docs/specs/a_implementar/spec_ui_ux_full_gameplay_inventory_hotbar_menus.md`, `docs/validation/SPEC17_UI_GAMEPLAY_MVP_VALIDATION_20260526.md` |

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

### Game Time / Hunger-Stamina Balance (SPEC 09) - Historico parcial superado

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

### Fechamento Game Time / Hunger-Stamina Balance (SPEC 09)

Status: Implementado completo em 2026-05-24.

- GameTime, stamina, hunger e status estao ligados em `GameBootstrap`, installers, geradores e nas tres cenas MVP.
- `PlayerNeedsBalanceSO` define tiers: regen `1.0/1.0/0.6/0.3`, regeneracao fixa `2/s` em hunger zero e movimento `0.85x` no tier critico.
- Hunger zero aplica dano de HP por `GameTimeTickEvent`; nao drena stamina por frame.
- Food aplica hunger, stamina e status; status publicam lifecycle events e persistem como ID/duracao.
- Save/load inclui stamina, game time e status em DTOs simples.
- HUD minimo funcional exibe hunger, stamina e status com duracao; Canvas final permanece na SPEC 17.
- Evidencia: `docs/validation/SPEC_09_HUNGER_STAMINA_STATUS_TIME_VALIDATION_20260524.md`.

### Progression

SkillPoint a cada 2 niveis: implementado no codigo.
Regra atual: +1 SkillPoint em niveis pares, comecando no level 2.
AttributePoint: +1 por level up.
Pendentes: gasto/distribuicao final de atributos, skill trees completas, active slots, capstones, respec Fonte de Anya e save/load completo.

### Cave entry, death, Anya e corpse recovery (SPEC 15) - Status 2026-05-26

Status: Implementado em codigo; compile validation PASS esperado; Play Mode humano pendente.

SPEC 15 implementacao e finalizacao concluidas (2026-05-25 a 2026-05-26):
- Death flow: PlayerDeathController, CaveDeathResolver, DeathSystemBootstrap
- Corpse recovery: CorpseRecoveryManager, CorpseInteractable, CorpseSpawner
- Anya respawn: AnyaFountain, AnyaRespawnService, AnyaFountainInteractable
- ModalBase abstract class criado; ModalManager.OpenModal<T>() implementado
- Input bloqueado (R, T, Y, G) quando modal ativo
- IInteractable contract em todos os interactables

Correcoes finais de compilacao (2026-05-26):
- PlayerProgressionEvents.cs esvaziado (remocao de classes duplicadas de PlayerXpChangedEvent e PlayerLevelChangedEvent)
- Causa raiz confirmada: sealed class vs readonly struct no mesmo namespace CindarsHope.Core.Events
- CS0246 de DeathSaveData era cascata dos duplicados; DeathSaveData esta correto em CorpseSaveData.cs
- Canonico: PlayerXpChangedEvent.cs e PlayerLevelChangedEvent.cs como readonly structs

Pendentes:
- Fechar Unity Editor e rodar RunUnityCompileValidation.ps1 para confirmar PASS
- Play Mode humano (morte na cave, respawn Anya, recuperar corpse)
- UI polish final (SPEC 17)

Evidencia: `docs/validation/SPEC15_FINALIZATION_SPEC16_PHASE0_VALIDATION_20260525.md`

### Skill trees, active slots, respec e Anya (SPEC 16) - Status 2026-05-26

Status: Implementado em codigo; compile validation PASS esperado; Play Mode humano pendente.

SPEC 16 implementada em 2026-05-26:
- SkillNodeDataSO expandido: NodeType, SkillCategory, IsCapstone, PrerequisiteNodeIds, PassiveModifiers
- SkillTreeDataSO expandido: CapstoneNodeId, Nodes list
- SkillEnums: SkillNodeType, SkillCategory, SkillModifierType, SkillTreeId
- DefaultSkillCatalog: 55 nodes / 5 arvores (Melee, Ranged, Magic, Survival, Crafting) criados via codigo
- SkillTreeRegistrySO e SkillNodeDatabaseSO: extencoes de DataRegistrySO para inspector-wired assets
- SkillTreeManager reescrito como MonoBehaviour com DefaultSkillCatalog fallback
- SkillPurchaseService: validacao de custo, prerequisites, level minimo, capstone rules
- SkillRespecService: full respec, primeiro gratuito, seguintes custam 250g configuravel
- SkillPassiveApplicator: aplica modificadores passivos aos derived stats
- DerivedStatsCalculator expandido: aceita IList<SkillPassiveModifier>
- SkillTreePanel (CindarsHope.UI.Skills): modal com tecla K, abas Q/E, nav W/S, compra, equipar R/T/Y/G
- SkillTreeInputHandler: handler dedicado para tecla K abrir SkillTreePanel
- AnyaFountainMenu: respec button habilitado, custo exibido, integrado com SkillRespecService
- ActiveSkillSlots: subscriber de ActiveSkillSlotAssignedEvent/ActiveSkillSlotClearedEvent
- SkillTreeSaveData e SaveV4ToV5Migration: persistencia de PurchasedNodeIds, ActiveSkillSlots, RespecCount
- SaveManager v5: captura e restaura SkillTree, registra SaveV4ToV5Migration
- GameBootstrap: expoe SkillTreeManager
- SkillTreeEvents: 14 eventos novos

Pendentes:
- Fechar Unity Editor e rodar RunUnityCompileValidation.ps1
- Play Mode humano (subir level par, comprar nodes, equipar slot, respec na Anya)
- UI polish final (SPEC 17)

Evidencia: `docs/validation/SPEC16_SKILL_TREES_VALIDATION_20260526.md`

## Specs futuras

A ordem oficial esta em `docs/specs/SPEC_EXECUTION_ORDER.md` e o registry futuro em `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`.

A primeira spec futura executavel passa a ser:

```text
docs/specs/a_implementar/spec_player_combat_weapons_spells_skill_actions_runtime.md
```

Os prompts operacionais 09-11 podem ainda exigir reconciliacao das implementacoes parciais existentes antes da spec 12. Antes de executar runtime futuro, as specs devem ser enriquecidas usando seus pre-refinamentos relacionados.

FASE9H/I/J/K/L nao devem ser tratadas como completas. As proximas implementacoes devem seguir somente `docs/specs/`.
