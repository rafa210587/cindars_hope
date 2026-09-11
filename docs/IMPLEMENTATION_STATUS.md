# Cindar's Hope - Implementation Status

## Fazenda keyart v4 — 2026-09-10

IN_PROGRESS, promoção NO: o aceite visual anterior foi reaberto pelo humano. Stage12 integra porta animada,
interior funcional, água de6quadros,19bases sólidas de props e correções Farm-side dos acessos à cidade/caverna.
Unity81/81EditMode;18vistas,19rotas,488bordas,24sólidos específicos,4acessos,78tiles e37imagens temporizadas PASS.
O percurso usa física controlada, sem provar input humano nem executar transições. Sorting da fachada na escada,
animação durante deslocamento controlado e acabamento do interior continuam em revisão.
Novo jogo continua vazio. Evidência e falhas intermediárias: [relatório v4](validation/farm_keyart_v4/REPORT.md).

## Harness Unity/Aseprite — 2026-09-10

ADR-0031 e refatoração de skills/agentes concluídos no escopo de instruções. Duas skills novas,
geração/paridade/idempotência verificadas; docs global FAIL (74 no baseline, 59 no final,
zero novos; redução fora deste escopo durante trabalho concorrente).
Progressive disclosure reforçado: seis entradas pesadas reduzidas em 85,6%–91,2%, contratos
movidos para referências condicionais e commands com gatilho funcional no início da descrição;
38 assertions do gerador e 136 arquivos de skill em paridade.
MCP não instalado nem validado em execução; nenhum gameplay/asset alterado nesta entrega.
[Evidência e limites](validation/HARNESS_TOOLING_REFACTOR_20260910.md).

## Piloto Aseprite — 2026-09-09

Skill com progressive disclosure e exemplos sincronizada; Aseprite aberto, roundtrip Lua comprovado.
Estrada/grama editáveis integradas e cercas alinhadas aos bloqueios, com ordenação local corrigida.
SCOPED_PASS: geração Unity, 18 capturas, 19 rotas, 480 bordas, 27 marcos físicos, 7 seleções, zero erros.
Aceite visual humano e fidelidade da composição geral pendentes; sem promoção.
[Evidência e limites](validation/farm_aseprite_pilot/REPORT.md).

> **Agents executing a spec should NOT read this full file.**
> For current project state and active queue, read `docs/project/CURRENT_STATE.md` instead.
> Read this file only for: full implementation history, audit, or explicit human request.

> Status: tracking reconciliado por validacao estatica de codigo em 2026-05-26.
> Fonte oficial de specs: `.specs/`.
> A pasta raiz `specs/` foi removida e nao deve ser recriada.

## 0.0.0 Fazenda — vale fechado v2 (2026-09-09)

CODE_COMPLETE / SCOPED_PASS; DEFERRED_TO_FINAL_HUMAN_VALIDATION. Substitui a direção de periferia aberta por floresta/escarpa com28faixas físicas, envelope72×50, pomar/pasto e marcos reposicionados. Novo jogo sem cultivos por decisão humana. Unity:36/36EditMode,18capturas,19rotas,7seleções,480bloqueios de perímetro e42células da casa não aráveis PASS. Não é equivalência pixel-a-pixel nem execução humana de transições. [Relatório e riscos](validation/farm_enclosed_valley_20260909/REPORT.md).

Refino posterior: fileira sul recalibrada pelos pixels opacos (galinheiro5.1u, celeiro7u, processamentos5.5u); casa/estufa mantidas. Corrente visual de pedras ao norte removida sem alterar a fronteira física; pomar ganhou laterais esparsas. Rule canônica agora valida proporção contra player e padrões de borda. EditMode19/19 e PlayMode18vistas/19rotas/7seleções/480bordas PASS,0erros.

## 0.0.0 Fazenda — paisagem exterior (2026-09-09)

CODE_COMPLETE; validação humana pendente. Solo ampliado e periferia variada nos quatro lados, com continuação visual da estrada leste. Unity: 16 capturas, 16 rotas e 6 seleções PASS; 171 componentes físicos preservados. [Relatório](validation/farm_outskirts_20260909/REPORT.md). Pendências físicas da auditoria anterior e polimento P3 da curva exterior permanecem.

## 0.0.0 Harness progressivo e workflows visuais (2026-09-09)

**Status:** CODE_COMPLETE; evidência e limites no relatório vinculado, sem promoção global.

- Quatro skills visuais e um agente audit-only; catálogo e referências sob demanda.
- Continuação: 10 templates + 10 exemplos em dez skills; 88 links e paridade conferidos.
- Piloto inglês: dez skills traduzidas; medição local/review em [relatório](validation/ENGLISH_HARNESS_PILOT_20260909.md).
- Gerador com recursos recursivos; delegação proporcional e validação por comportamento.
- Hooks sem resultados presumidos e sync por identidade dos inputs; contratos 36+14 PASS.
- Unity não aplicável a esta alteração de harness. Falhas anteriores de farm/wiring/docs preservadas.
- [Relatório de integração](validation/PROGRESSIVE_HARNESS_VISUAL_WORKFLOWS_20260909.md).

## 0.0.0 Qualidade — melhorias executadas, falhas globais explícitas (2026-09-08)

**Status:** CODE_COMPLETE; sem promoção ou aceitação humana.

- Add/remove/capacidade no core existente; prontidão de UI compartilhada e ID capturado
  antes do consumo. Validators/menu propagam falhas reais e preservam ownership de cenas.
- Cortados17testes redundantes e3scans duplicados; quatro restores inválidos fortalecidos.
- Matriz canônica, evidência reutilizável e runners endurecidos;95contratos+30gerador PASS.
- Final2911/2915 com mesmas4falhas farm; afetados126/126; build7/7 ePlayMode2/2 PASS.
- Batch21FAIL por wiring de picaretas/Fireball/gates/anchors; docs47diagnósticos reais
  após eliminar26falsos positivos do detector. Sem claim GLOBAL_PASS.
- [Relatório integrado](validation/QUALITY_IMPROVEMENTS_EXECUTION.md). Specs de manutenção
  permanecem em a_implementar; sem commit/push/merge.

## 0.0.0 SOLID / contexto AI — refatoração verificada, gates globais pendentes (2026-09-08)

**Status:** `CODE_COMPLETE_WITH_GLOBAL_GATES_FAILING`; nenhuma promoção/aceitação humana.

- Auditoria da superfície própria e revisão de hotspots, harness e testes com baseline.
- Rule `solid-and-ai-context`, skill `solid-refactoring`, três agentes existentes atualizados;
  contratos de hooks/gerador/strict testados, paridade gerada e config local preservada.
- InventorySlotOperations separa split/move/merge/swap; SaveBackupService concentra escrita
  segura; três consumidores usam QuestRuntimeIds canônico. Nenhuma mudança de schema/cena.
- Contexto por classe via Roslyn com filtros; XML útil em vez de frontmatter repetitivo em massa.
- CURRENT_STATE reduzido545→78linhas; histórico preservado integralmente em docs/archive.
- Sete builds e Unity compile PASS; afetados36/36 e PlayMode composição2/2 PASS;
  full2900/2904 com as mesmas quatro falhas de farm do baseline2884/2888.
- Docs final:73 diagnósticos iguais ao baseline após normalizar números de linha;
  strict retorna FAIL verdadeiro por docs. Sem claim BUILD_VALIDATED.
- [Auditoria e evidência](validation/SOLID_AI_PROJECT_AUDIT.md). Specs SOLID_AI permanecem em a_implementar.

## 0.0.0 Modularização residual — 25 pares mútuos → 0 (2026-07-16)

**Status:** `BUILD_VALIDATED_PENDING_HUMAN_PLAYMODE`

- As 7 specs de corte residual do lote ARCH_RESIDUAL (`spec_arch_core_boundary_residual_v1`,
  `spec_arch_save_ownership_residual_v1`, `spec_arch_ui_boundary_residual_v1`,
  `spec_arch_npc_quest_boundary_residual_v1`, `spec_arch_player_gameplay_boundary_residual_v1`,
  `spec_arch_combat_boundary_residual_v1`, `spec_arch_cave_integration_boundary_residual_v1`)
  foram implementadas em 25 commits (`ee45510e`..`a4203461`, branch `dev`), um por par mútuo.
- `tools/architecture/Get-ModularizationDependencySnapshot.ps1` reporta `MutualModulePairs=0` no
  HEAD `a4203461` — zero pares mútuos remanescentes no runtime (antes: 25).
- Build: `Invoke-UnityGeneratedProjectsBuild.ps1` PASS (exit 0, 7/7 projetos).
- EditMode: `RunUnityEditModeTests.ps1` PASS 2837/2837 (exit 0) nos 25 cortes; 2845/2845 (exit 0)
  após os 8 testes de caracterização que fecharam o critério 14.1 da spec de NPC|Quests.
- Técnica dominante: portas mínimas em `CindarsHope.Foundation` + `DomainManagerRegistry`, e o
  adapter `[SerializeField] MonoBehaviour` + `is IInterface` (molde `CraftingPoint`), preservando
  100% das referências de cena sem regen.
- Desvio detectado no closeout e FECHADO na mesma sessão: `spec_arch_npc_quest_boundary_residual_v1`
  cortou o par `NPC|Quests` apenas pela direção `Quests → NPC` (`1b92c6eb`), deixando em aberto os
  critérios 14.1 (testes de caracterização, bloqueantes) e 14.2 (consumir contrato em vez do
  `QuestRuntimeBootstrap.QuestService` concreto). Ambos fechados em seguida: contrato mínimo
  `IQuestInteractionQuery` em `CindarsHope.Quests.Runtime`, decisão extraída para a policy pura
  `NpcQuestInteractionPolicy` e 8 testes de caracterização (ver header de evidência da spec). A
  aresta unidirecional `NPC → Quests` permanece por design — não é ciclo e não afeta
  `MutualModulePairs=0`.
- Play Mode humano **NOT RUN** — pendente, coberto por `spec_validation_human_playmode_smoke_v1`
  (segue em `.specs/a_implementar/`).

Evidence: headers de evidência das 7 specs em `.specs/implementados/spec_arch_*_boundary_residual_v1.md`
e `.specs/implementados/spec_arch_save_ownership_residual_v1.md`.

---

## 0.0.1 TownScene preservation-first relayout (2026-07-01)

**Status:** `BUILD_VALIDATED_PENDING_HUMAN_PLAYMODE`

- TownScene reorganizada por 24 lotes determinísticos e malha de vias sem overlap.
- Baseline preservado: 24 casas/portas/telhados, 29 barracas, 1153 árvores (497 era o piso v8), 28 NPCs e 84 anchors.
- Papéis e destinos de trabalho/social/home reconciliados; `Stationary` agora respeita expediente.
- EditMode 22/22, validator 21/21, builds runtime/editor exit 0.
- Play Mode visual pendente: `docs/validation/playmode/spec_city_preservation_first_coherent_relayout_human_test_scenario.md`.

Evidence: `docs/validation/spec_city_preservation_first_coherent_relayout_execution_report.md`.

---

## 1. Resumo executivo — SPEC_29 Consolidation (2026-06-01)

**MVP Status: CODE-COMPLETE AND BUILD-VALIDATED ✓**

A consolidacao de SPEC_18-28 confirma que o projeto possui implementacao completa do MVP:

- Fase 0: 11/11 specs auditadas, zero gaps criticos
- Fase 1: Build PASS 0E/0W runtime, 0E/0W editor, docs PASS 14/14
- Fase 2-3: Pendente execucao humana Play Mode (validators e checklist de ~2h em Unity Editor)

MVP definition fully satisfied. Ready for Phase 2-3 human Play Mode validation in local Unity Editor.

---

## 0.1. Post-SPEC_29B Consolidation Status (2026-06-01)

**CRITICAL RECONCILIATION COMPLETED:**

The MVP Closeout package (SPEC_18-28) was designed to address earlier partial/residual specs (SPEC_10-17).

**Current Status:**
- **SPEC_18-28 Code:** ✓ Phase 0-1 complete (code-ready, build-validated 0E/0W, docs 14/14)
- **SPEC_18-28 Play Mode:** ✗ Phase 2-3 NOT YET EXECUTED (checklists prepared, awaiting human in Unity Editor)
- **SPEC_10-17 Status:** Addressed by closeout but status changes PENDING Phase 2-3 evidence
- **MVP Acceptance:** **PENDING** (requires Phase 2-3 completion before final promotion)

**Do NOT move specs to `implementados/` yet.** Physical spec moves deferred until Phase 2-3 human evidence collected.

See `docs/validation/spec_29b_phase0_human_acceptance_reconciliation_audit_matrix.md` for audit details.

**Estimated Next Actions:** 2-2.5 hours human execution in Unity Editor (Phase 2 validators + Phase 3 Play Mode)

---

## 0.2. Prior Status (Reference)

A validacao de codigo ate a SPEC 16 confirma que o projeto possui baseline suficiente para seguir para a proxima etapa de UI/closeout, desde que as proximas execucoes nao tratem specs parciais como completas.

Nao ha erro CS0023 ativo em `ValidateItemAndShopData.cs`: o validador usa `entry.Item == null` para `StartingItem` struct.

Principais conclusoes:

- SPECS 00-09: base documental/tooling/runtime MVP majoritariamente implementada, com algumas areas historicas ainda parciais.
- SPECS 10-14: implementacao parcial/residual ativa; nao tratar como completas.
- SPEC 15: implementacao parcial em codigo. Existem death DTOs, `PlayerDeathController`, `Corpse`, `CorpseRecoveryManager` e captura de `DeathSaveData`, mas a orquestracao completa de cave death/Anya/corpse spawn/restore nao esta fechada no codigo validado.
- SPEC 16: implementada em codigo. Skill trees, 5 arvores/55 nodes, compra, slots ativos, respec service, save v5 e manager existem; Unity compile e Play Mode humano seguem pendentes.
- SPEC 17+: ja recebeu incrementos, mas permanece etapa de UI/closeout e validacao final.

## 2. Resumo por area

| Area | Status real | Evidencia / observacao |
|---|---|---|
| Claude Code project structure (.claude/) | Implementado completo | `.claude/settings.json`, `.claude/commands/`, `.claude/skills/`, `.claude/agents/`, `.claude/hooks/` |
| Governanca documental / fonte unica | Implementado documental parcial | `.specs/implementados/spec_docs_001_single_source_specs_refinements_reconciliation_parcial.md` |
| Unity compile validation protocol | Implementado completo | `.specs/implementados/spec_unity_compile_validation_protocol_and_scripts.md` |
| Core/event bus/bootstrap | Implementado parcial | `.specs/implementados/spec_core_001_event_bus_e_eventos_base.md`, `.specs/implementados/spec_core_002_bootstrap_managers_e_runtime_references.md` |
| Data/IDs/registries | Implementado | `.specs/implementados/spec_data_001_ids_registries_e_scriptableobjects.md` |
| Save/load JSON cross-scene | Implementado parcial | `.specs/implementados/spec_save_001_json_save_load_cross_scene.md` |
| Save schema migration v2 | Implementado completo | `.specs/implementados/spec_save_002_schema_migration_v2.md` |
| Inventory slots/capacidade/UI minima | Implementado completo | `.specs/implementados/spec_inventory_002_slots_capacity_ui_final.md` |
| Farm irrigacao/solo/planting UI | Implementado completo | `.specs/implementados/spec_farm_004_irrigacao_solo_planting_ui.md` |
| World activities/fishing/trees/loot | Implementado completo | `.specs/implementados/spec_world_002_activities_fishing_trees_pickups_loot.md` |
| Farm loop/world activities | Implementado parcial | `.specs/implementados/spec_farm_001_farm_scene_movimento_interacao.md`, `.specs/implementados/spec_farm_002_plots_seeds_growth_harvest.md`, `.specs/implementados/spec_farm_003_arvores_pesca_pickups_world_activities.md` |
| Game time / day-night cycles / hunger-stamina balance | Implementado completo | `.specs/implementados/spec_hunger_stamina_status_balance.md` |
| Economy/hunger/crafting/town | Implementado parcial | `.specs/implementados/spec_economy_001_compra_venda_gold_e_sellables.md`, `.specs/implementados/spec_hunger_001_fome_comida_e_hp_por_fome.md`, `.specs/implementados/spec_craft_001_crafting_mvp.md`, `.specs/implementados/spec_town_001_town_scene_portais_npc_pip_comercio.md` |
| Combat/damage/enemy stats | Implementado parcial | `.specs/implementados/spec_combat_001_slime_melee_contact_damage_drops.md`, `.specs/implementados/spec_combat_002_enemy_data_driven_stats.md`, `.specs/implementados/spec_damage_001_damage_formula_mvp.md` |
| UI/tools/hotbar/progression debug | Implementado parcial | `.specs/implementados/spec_ui_001_debug_hud_e_feedback_mvp.md`, `.specs/implementados/spec_ui_002_hud_tools_hotbar_progression_debug.md`, `.specs/implementados/spec_tools_001_tools_equipment_hotbar_parcial.md`, `.specs/implementados/spec_progression_001_xp_level_atributos_parcial.md` |
| Cave runtime/procedural/stable run/boss gates | Implementado em codigo - validacao Unity pendente | `.specs/implementados/spec_cave_001_cave_scene_portal_e_runtime_basico.md` ate `spec_cave_008_debug_skip_confinement_wall_distance_hardening.md` |
| Overnight 2026-05-23 | Executado parcialmente | `PROJECT_LOG.md` (Sessao 2026-05-23 Overnight), `docs/refinements/implementados/ref_stabilizacao_overnight_specs_20260523.md` |
| Economy/shop/stock/pricing/UI (Spec 06) | Implementado completo | `.specs/implementados/spec_economy_shop_stock_pricing_ui.md` |
| Crafting queue/workstations/recipes/UI (Spec 07) | Implementado completo | `.specs/implementados/spec_crafting_queue_workstations_recipes_ui.md` |
| Town NPC/dialogue/wanderer/save hooks (Spec 08) | Implementado completo | `.specs/implementados/spec_town_npc_dialogue_schedule_quests.md` |
| Hunger/stamina/status/time (Spec 09) | Implementado completo | `.specs/implementados/spec_hunger_stamina_status_balance.md` |
| Equipment/durability/environment/loot (Spec 10) | Implementado parcial | `.specs/implementados/spec_equipment_durability_environment_loot_runtime.md` |
| Damage/status/elements/resistances (Spec 11) | Implementado parcial | `.specs/implementados/spec_damage_status_elements_resistances_runtime.md` |
| Player Combat/weapons/spells/skill actions (Spec 12) | Implementado parcial | `.specs/implementados/spec_player_combat_weapons_spells_skill_actions_runtime.md` |
| UI/Input/Shop/Sell Bugfix Bundle (Post-SPEC 12) | Implementado completo | `.specs/implementados/spec_bugfix_ui_input_shop_sell_bundle.md` |
| Enemy AI/roster/bestiary/faction locks (Spec 13) | Implementado parcial | `.specs/implementados/spec_enemy_ai_roster_bestiary_faction_locks_runtime.md` |
| Skill trees/active slots/respec Anya (Spec 16) | Implementado em codigo - Play Mode humano pendente | `.specs/implementados/spec_skill_trees_active_slots_respec_anya_runtime.md` |
| UI Gameplay MVP: shops/sell/equipment/attributes/skills (Spec 17 incremento) | Implementado em codigo - Play Mode humano pendente; SPEC 17 ampla permanece aberta | `.specs/a_implementar/spec_ui_ux_full_gameplay_inventory_hotbar_menus.md`, `docs/validation/SPEC17_UI_GAMEPLAY_MVP_VALIDATION_20260526.md` |
| UI Gameplay closeout: skills/shop/K-L/prompts/actions (Spec 17C) | Implementado completo - Play Mode humano validado 2026-05-26 | `.specs/implementados/spec_ui_gameplay_closeout_skill_shop_prompts_actions_hud.md`, `docs/validation/SPEC17C_CLOSEOUT_VALIDATION_20260526.md` |
| UI Gameplay shop injection/equipment slot picker (Spec 17D) | Implementado completo - Play Mode humano validado 2026-05-26 | `.specs/implementados/spec_ui_gameplay_shop_injection_equipment_slot_picker_closeout.md`, `docs/validation/SPEC17D_CLOSEOUT_VALIDATION_20260526.md` |
| UI Gameplay ShopSession lifecycle/readiness (Spec 17E) | Implementado completo - Play Mode humano validado 2026-05-26 | `.specs/implementados/spec_ui_gameplay_shop_session_lifecycle_npc_readiness_closeout.md`, `docs/validation/SPEC17E_SHOP_SESSION_FIX_VALIDATION_20260526.md` |
| UI Gameplay shop modal/responsive/names (Spec 17F) | Implementado completo - Play Mode humano validado 2026-05-26 | `.specs/implementados/spec_ui_gameplay_shop_modal_stack_responsive_names_closeout.md`, `docs/validation/SPEC17F_SHOP_MODAL_UI_VALIDATION_20260526.md` |
| Visual scale / world scale / camera profiles (Spec 17A) | Implementado em codigo completo (17A-FIX aplicado) - Play Mode humano pendente | `.specs/implementados/spec_visual_world_scale_camera_sprite_profiles.md` |
| GameScaleConfigSO central config + valores reais 2x/3x/6x (17A-FIX) | Implementado - cave 160x96, boss 2.5x, arvores 3x, lago 6x, boss gates 15-90 no registry, enemy_meteor_ooze_king real | commit `bd06a3a` |
| UI/UX infraestrutura 17B: input routing, pause, toasts, context hint, death screen, checkpoint menu (Spec 17B parcial) | Implementado em codigo - GameplayInputRouter, PauseMenuController, NotificationToastController, ContextHintController, DeathScreenController, CaveCheckpointSideMenuController; UIEvents.cs; ModalType Pause/Death/CaveCheckpoint; SkillTreeGameplayPanelController yield a GameplayInputRouter. Play Mode humano pendente. | branch `dev` 2026-05-27 |
| SPEC 13A - Enemy taxonomy, profiles e contracts | Implementado em codigo - LoreTagline em EnemyDataSO; MinimumRoomSize em EnemySizeProfileSO; 4 novos VulnerabilityTriggerMode; CreateDefaultEnemyProfiles editor (16 factions, 6 size, 10 movement, 10 vuln); ValidateSpec13EnemyTaxonomyProfiles; 0 erros build. Assets gerados pelo menu Unity pendentes. | branch `dev` 2026-05-27 |
| SPEC 13B - Roster 40 EnemyDataSO | Implementado em codigo - PrimaryDamageTypeId em EnemyDataSO; CreateRoster40EnemyData editor (44 entries: 7 band1, 9 band2, 9 band3, 8 band4, 6 band5, 5 bosses); ValidateSpec13EnemyRoster; 0 erros build. Assets gerados pelo menu Unity pendentes. NOTA: roster alternativo (enemy_verdant_mite etc.) - reconciliacao com roster canonico (SPEC 13C) pendente. | branch `dev` 2026-05-27 |
| SPEC 13C - Enemy actions/action sets | Implementado em codigo - EnemyActionSO estendido (StatusApplyChance, VulnerabilityWindowTrigger, MinRange, MaxTargets, RequiresLineOfSight, IsInterruptible); EnemyActionSetSO estendido (FallbackActionId, RoleTags, Notes); CreateEnemyActionsAndSets editor (8 telegraph profiles, 71 EnemyActionSO, 40 EnemyActionSetSO); ValidateSpec13EnemyActions; 0 erros build. Assets gerados pelo menu Unity pendentes. | branch `dev` 2026-05-27 |
| SPEC 13D - EnemyBrain runtime MVP | Implementado em codigo - EnemyBrain reescrito (state machine data-driven: Idle/Patrol/Alert/Chase/Kite/GuardHold/AttackWindup/AttackRecover/Stunned/Dead); EnemyActionRuntime (cooldown por acao); EnemyVulnerabilityState (janela de vulnerabilidade); 3 database SOs (EnemyActionDatabaseSO, EnemyActionSetDatabaseSO, EnemyTelegraphProfileDatabaseSO); EnemyVulnerabilityStartedEvent/EndedEvent; ValidateSpec13EnemyBrainRuntime; 0 erros build. Wiring no Unity Editor (prefabs + databases) pendente. Roster 13B vs 13C reconciliacao ainda pendente. | branch `dev` 2026-05-27 |
| SPEC 13E - Bestiary runtime/save | Implementado em codigo - BestiaryManager event-driven com save/load por IDs; GameSaveData.Bestiary; SaveManager captura/restaura; GameBootstrap injeta BestiaryManager; EnemyBestiaryEntrySO; CreateBestiaryEntries40; ValidateSpec13BestiaryRuntimeSave; builds C# runtime/editor com 0 erros. Assets de bestiary gerados via menu Unity e Play Mode humano pendentes. SPEC 13F nao implementada. | branch `dev` 2026-05-27 |
| SPEC 13F - Spawn resolver/ecology/faction locks | Implementado em codigo - EnemySpawnProfileSO, EnemySpawnPackSO, EnemyFactionLockSO, EnemySpawnRequest/Result/Candidate, EnemySpawnResolver deterministico por seed, eventos EnemySpawnResolved/Warning/PackSelected, CreateEnemySpawnEcologyData, ValidateSpec13SpawnResolverEcology; builds C# runtime/editor com 0 erros. Assets gerados via menu Unity e validator Unity pendentes. SPEC 14 materializacao/snapshot/respawn nao implementada. | branch `dev` 2026-05-28 |
| SPEC 14A - Cave Enemy SpawnPlan / Materialization / Run Stability | Implementado em codigo - CaveEnemySpawnPlan/Entry, CaveEnemySpawnPlanner e CaveRuntimeMaterializer conectam EnemySpawnResolver a inimigos materializados por WorldSeed+RunSeed+CaveLevel+BiomeId; CreatedEnemies atualizado; EnemySpawned/Seen alimenta Bestiary FirstSeen. Fix 2026-05-29: EnemyBrain.Configure(EnemyDataSO) adicionado (compile error); Execute() batchmode entry adicionado; 64 assets YAML criados (40 profiles, 17 packs, 7 locks) em Assets/_Game/Data/EnemySpawn/. CaveScene rewire ainda pendente — rodar menu `CindarsHope/SPEC 13/Generate And Wire SPEC 13G Assets` para completar. FIX2 2026-05-29: spawn density 12-20→14-24; CombatLog com SourceName+SourceEnemyId; PlayerDamagedEvent criado; FloatingDamageNumberDisplayer exibe numero vermelho acima do player. FIX3 2026-05-29: BuildBiomeTags multi-tag (fix level-15 zero enemies); ResolveSpawnPoints fallback em walkable tiles; loop multi-pass 4x com seed deterministico; packs rebalanceados MaxTotal 4→10-14; pack_low_undead e pack_beast_mid adicionados; diagnostico BuildDiagnosticSummary no resolver; menus consolidados Cindar's Hope→CindarsHope (8 arquivos). FIX4 2026-05-29: BuildUnlockedFactionLockIds verifica RequiredCaveLevelMin (fix level-30/45 zero enemies); EnemyBrain.ConfigureRuntime injeta todos os databases; EnemyChaseController desabilitado quando MovementProfile disponivel; SizeProfile aplicado para escala/collider; 3 novos database SOs criados (EnemyMovementProfileDatabaseSO, EnemyVulnerabilityProfileDatabaseSO, EnemySizeProfileDatabaseSO); ValidateSpec14AEnemyRuntimeIntegration; builds C# 0 erros. Wiring dos 6 novos databases no Inspector e criacao de assets database pendentes. Regenerar assets via `CindarsHope/Generate/Enemy/Generate And Wire SPEC 13G Assets` e validar Play Mode. SPEC 14 completa permanece aberta para snapshot replay, respawn, redistribuicao, boss gates e rewards. | branch `dev` 2026-05-29 |
| SPEC 14B - Cave Snapshot Replay with EnemySpawnPlan | Implementado em codigo - VisitedLevelSnapshot/CaveLevelSnapshot consolidado com CaveEnemySpawnPlan, ResourceNodeStates e FishingSpotState; CaveSnapshotService captura/restaura snapshot por RunSeed+Level; CaveRuntimeMaterializer usa MaterializeFromSnapshot sem rerodar EnemySpawnResolver quando plano existe. Builds C# runtime/editor OK em 2026-05-29. Unity validator e Play Mode humano pendentes. SPEC 14C/14D continuam abertas para respawn e redistribuicao. | branch `dev` 2026-05-29 |
| Architecture Reorganization / Validator Foundation (SpecKit SPEC_00-03) | Implementado completo | SPEC_00: estrategia sequencial, anti-drift rules; SPEC_01: fundacao ValidationSeverity/ValidationIssue/ValidationReport/IProjectValidator/ProjectValidationRunner editor-only; SPEC_02 (projectile prefab validator) + SPEC_03 (combat database validators) merged em dev 2026-06-01; paths alinhados Assets/_Game/Data/Combat/{WeaponDatabase,SpellDatabase}.asset (2026-06-01 micro-closeout); docs/validation/spec_arch_reorg_*.md com execution reports. Builds C# 0E/0W runtime, 0E/2W editor (pre-existentes); Unity compile validation PASS 2026-06-01 (exit code 1 post-compile curl error, nao relacionado a codigo). |
| Architecture Reorganization Combat Services & Item Contracts & Status Effect & Save Providers & Bootstrap Installers (SpecKit SPEC_04-11) | Code-complete; validacoes C# PASS, Unity batchmode PARTIAL, Play Mode pendente | SPEC_04 (wave 1 legacy audit); SPEC_05 (CombatActionContext+EquippedItemResolver+CooldownHelper); SPEC_05B (RefreshItemResolver fix); SPEC_06 (ProjectileSpawnService+ProjectileSpawnRequest/Result); SPEC_07 (BowArrowAttackService+SpellCastService+AttackResult); SPEC_07B (RebindStaminaManager + bow bloqueio pós-resolution); SPEC_08 (ItemUseKind enum + ItemUseContractResolver + ItemDataSO 4 campos + CombatDatabaseValidator validacoes); SPEC_09 (StatusEffectDatabaseSO registry + GameBootstrap wiring + SpellCastService/EnemyStatusRuntimeTicker/PlayerAttackController integração + CombatDatabaseValidator validação status effect refs + fallback Resources.Load preservado); SPEC_10 (ISaveSectionProvider interface + HotbarSectionProvider piloto + SaveManager integrado com fallback + GameSaveData inalterado + schema v5 preservado + migrations preservadas); SPEC_11 (CombatRuntimeInstallContext POCO [Serializable] + CombatRuntimeInstaller static class Install() com LogError/LogWarning por campo + GameBootstrap.BuildCombatInstallContext() + chamada Install() em InitializeManagers() + MvpSceneValidator.ValidateSpec11CombatDatabases() + Assembly-CSharp.csproj atualizado; FR-004 sem fallback silencioso; lifecycle GameBootstrap preservado). SPEC_12 closeout: dotnet build PASS 0E/0W runtime, 0E/2W editor (pre-existentes); validate_docs.ps1 PASS 14/14; Unity batchmode C# compile OK (exit code 1 due licensing wrapper); Play Mode validation e editor validators pendentes. Backlog residual: StatusEffectDatabase wiring, save provider scaling, installer scaling, service/contract validators, architecture diagram. Code 100% comportamento gameplay preservado. |
| Architecture Reorganization Closeout Validation (SpecKit SPEC_12) | Completed — validation & documentation | SPEC_12 Wave7: fechar SPEC_04-11 com validacoes integradas, documentacao factual, backlog residual. Executadas: dotnet build PASS (0E/0W runtime, 0E/2W editor pre-existentes), validate_docs.ps1 PASS (14/14), Unity batchmode C# compile OK (wrapper exit code 1 due licensing callback, nao relacionado a codigo). NOT RUN: editor validators (requerem editor interativo), Play Mode checklist (requerem gameplay). Status real: code-complete sem regressao. Backlog criado: docs/backlog/reorg_architecture_residual_backlog.md (8 items: Play Mode validation, editor validators, StatusEffectDatabase wiring, save provider scaling, installer scaling, gamebootstrap monolithic, validator coverage gaps, docs gaps). Próximas actions: Play Mode humano (30min), editor validator audit (15min), StatusEffectDatabase wiring (20min). | branch `dev` 2026-06-01 |
| Input Manager | Debito tecnico futuro | Input Manager legado ativo; migracao para Input System requer spec propria para evitar regressao de gameplay/UI. |

## 3. Status oficial por SPEC/prompt ate 16

| Spec | Status real apos validacao de codigo | Observacao |
|---|---|---|
| 00 | Implementado documental parcial | Nao reexecutar spec antiga. |
| 01 | Implementado completo | Tooling minimo existe. |
| 02 | Implementado parcial | Infra de migration existe; manter parcial conforme ordem oficial. |
| 03 | Implementado parcial | Inventory tem slots e capacidade, mas ainda ha pendencias funcionais/UI. |
| 04 | Implementado parcial | Farm planting/irrigacao existem, mas Play Mode/polimento pendem. |
| 05 | Implementado parcial | World activities existem parcialmente; residual ativo. |
| 06 | Implementado completo em codigo | Shop/economy implementado; Play Mode final ainda depende das specs de closeout. |
| 07 | Implementado completo em codigo | Crafting/workstations/recipes em codigo; Play Mode final pendente. |
| 08 | Implementado completo em codigo | Town/NPC/dialogue em codigo; Play Mode final pendente. |
| 09 | Implementado completo MVP | Hunger/stamina/status/time fechados como MVP. |
| 10 | Implementado parcial | Nao tratar como completo. |
| 11 | Implementado parcial | Nao tratar como completo. |
| 12 | Implementado parcial | Nao tratar como completo. |
| 13 | Implementado parcial - residual ativo | Ainda listado como residual ativo. |
| 14 | Implementado parcial - residual ativo | Ainda listado como residual ativo. |
| 15 | Implementado parcial em codigo | Death/corpse base existe; Anya/orquestracao/spawn/restore incompletos no codigo validado. |
| 16 | Implementado em codigo | Skill tree stack existe; Unity compile/Play Mode humano pendentes. |

## 4. Evidencias de codigo validadas

### 4.1 Erro CS0023 ja corrigido

`Assets/_Game/Scripts/Editor/Validation/ValidateItemAndShopData.cs` usa:

```csharp
var entry = playerData.StartingItems[i];
if (entry.Item == null)
```

Como `StartingItem` e struct, isso esta correto. Nao ha mais `entry?.Item` nesse trecho.

### 4.2 SaveManager v5 / migrations

`SaveManager` esta em schema v5 e registra:

- `InventorySlotsV1ToV2Migration`
- `SaveV2ToV3Migration`
- `SaveV3ToV4Migration`
- `SaveV4ToV5Migration`

Tambem captura `SkillTree`, `ActiveSkillSlots`, `Death`, `Economy`, `Crafting`, `Stamina`, `GameTime`, `StatusEffects`, `EquipmentDurability` e `Npcs`.

## 5. SPEC 15 — Cave entry, death, Anya e corpse recovery

Status real: **implementado parcial em codigo**.

### 5.1 Confirmado em codigo

Arquivos/elementos encontrados e coerentes:

- `PlayerDeathController`: escuta `HPChangedEvent` e publica `PlayerDiedEvent` quando HP chega a zero.
- `Corpse`: modelo runtime com id, status, run/cave data, posicao, gold e listas de itens/equipment.
- `CorpseRecoveryManager`: controla active corpse, recuperacao de gold, itens e equipment, e publica eventos de recovery/parcial/replaced.
- `CorpseSaveData`, `CorpseItemSaveData`, `DeathStatsSaveData`, `DeathSaveData`: DTOs serializaveis.
- `SaveManager.CaptureDeathSaveData`: captura/preserva `DeathStats` e `ActiveCorpse` existente.

### 5.2 Nao encontrado / nao fechado no codigo validado

Nao foram encontrados no `dev` durante esta validacao:

- `CaveDeathResolver`
- `DeathSystemBootstrap`
- `CorpseInteractable`
- `CorpseSpawner`
- `AnyaFountain`
- `AnyaRespawnService`
- `AnyaFountainInteractable`

Alem disso, `SaveManager.RestoreDeathSaveData` ainda contem TODO e nao restaura active corpse para o runtime.

Conclusao: SPEC 15 nao deve ser tratada como completa. Ela e suficiente como fundacao parcial, mas nao como fluxo funcional fechado de morte -> respawn Anya -> corpse persistente -> recover.

## 6. SPEC 16 — Skill trees, active slots e respec Anya

Status real: **implementado em codigo; Unity compile/Play Mode humano pendentes**.

Confirmado em codigo/documentacao:

- `SkillTreeManager` como `MonoBehaviour`.
- Fallback `DefaultSkillCatalog` com 5 arvores / 55 nodes.
- `SkillPurchaseService` para custo, prerequisites, level minimo e capstone rules.
- `SkillRespecService` com primeiro respec gratuito e custo padrao posterior.
- `SkillPassiveApplicator` e eventos de derived stats.
- `SkillTreeSaveData` com `PurchasedNodeIds`, `ActiveSkillSlots` e `RespecCount`.
- `SaveV4ToV5Migration` inicializa `SkillTreeSaveData`.
- `SaveManager` captura e restaura `SkillTree`.
- `SkillTreeInputHandler` abre skill tree em `U`.

Pendencias:

- Rodar Unity compile real.
- Play Mode humano: subir level par, comprar node, equipar slot, respec, salvar/carregar.
- Validar a integracao real com Fonte de Anya em cena, porque SPEC 15/Anya ainda esta parcial no codigo validado.

## 7. Pendencias que nao devem bloquear a proxima etapa de UI/closeout

As pendencias abaixo nao impedem iniciar a proxima etapa, desde que fiquem declaradas como riscos/residuais:

- SPEC 10-14 continuam parciais/residuais.
- SPEC 15 nao fecha orquestracao/Anya/corpse restore.
- SPEC 16 precisa Unity/Play Mode humano.
- SPEC 17 ampla ainda precisa Canvas final, pause/options, fluxos cave/corpse/toasts e validacao final.

## 8. Proximo passo recomendado

Seguir para SPEC 17/UI/UX/closeout, com guardrails:

- Nao marcar 10-15 como completas.
- Nao depender de Anya/corpse restore como pronto.
- Validar UI em cima do que existe em codigo.
- Deixar claro que a validacao humana sera feita no final do pacote.
- Antes de fechamento final, executar Unity compile, scanners e Play Mode.


### 2026-09-09 — FarmScene, fechamento técnico F01–F05

Reconstrução pixelart integrada e entregue para revisão humana. Closing regen04/gameplay04/replay01: SCOPED_PASS,8vistas,16rotas físicas,4seleções reais,8câmerasCustomAxis/Y e0runtimeErrors;11testes focais reutilizados por6inputs de hashes equivalentes. Trigger local da porta e adapter CameraTransparencySort2D corrigem aproximação e persistência do sorting, sem mudar alcance global/ProjectSettings. Todas6regiões do replay idênticas; dois overviews mantêm resíduo de142/200pixels em pequena árvoreSW, sem alegação de determinismo integral. Spec IN_PROGRESS, promoçãoNO, aceite humano/input/animação pendentes; demais falhas históricas do projeto não são reclassificadas. Evidência: docs/validation/spec_farm_pixelart_cohesion_and_ingame_review_v1_execution_report.md.


### 2026-09-09 — FarmScene revisão G, proporções e perímetro

G01–G04 SCOPED_PASS técnico: props/prédios calibrados ao player, casa mantida para preservar porta, ponte reta RGBA integrada,6árvores redistribuídas mantendo71IDs e perímetro visual externo sem recursos novos. Evidência vigente proportion_pass/regen_02/gameplay_02:8vistas,16rotas,4seleçõesreais,0erros,CustomAxis/Y e cena/saves preservados;9focais01 reutilizados por4inputs iguais. CenaSHA eb848c096e5e13cd015e61516ee0e7e11374708cbb22b37cf22cb9515f723d16. SpecIN_PROGRESS/promoçãoNO, aceitehumano/inputs/animação pendentes. Evidência: docs/validation/spec_farm_pixelart_cohesion_and_ingame_review_v1_execution_report.md. Demais baselines do projeto não foram reclassificadas.

### 2026-09-09 — FarmScene revisão H, trilhas e entrada exterior

H01–H04 SCOPED_PASS técnico: trilhas ramificadas em raster visual0.25u, cave exterior(-19.5,17) com retorno canônico, clareira e mural GPT;15jardins sem colisão.71IDs preservados,29árvores realocadas perante G sem drift de sprite/escala/dados. path_cave_pass/regen_04/gameplay_02:8vistas,16rotas(6455nós),5seleções reais,0erros e cena/saves preservados;31casos válidos(5Planner03+26reuso7inputs). CenaSHA1c6311a582901d2f69a60a3d3e90e43940e95c3d18e8d1485ca3c7944f764844. Falhas intermediárias Tree43/Meadow11/raster1u permanecem históricas. CaveScene/procedural/save não alterados. Spec IN_PROGRESS/promoçãoNO; aceite humano/input/animação pendentes. Evidência: docs/validation/spec_farm_pixelart_cohesion_and_ingame_review_v1_execution_report.md. Sem reclassificar baselines globais.

### 2026-09-09 — FarmScene revisão I, cais e trilhas

I01–I04 SCOPED_PASS técnico: cais upright reduzido para4.2u e apoiado na margem, deck caminhável, barco/água adjacente bloqueados; pesca movida preservando IDlegado, trilhas com chanfros/bordas externas finas. dock_path_pass/regen_01/gameplay_01:32/32focais,8vistas,16rotas(6465nós),6seleções reais,3águas bloqueadas,0erros e cena/saves preservados.71árvores semdelta vsH. CenaSHA751a9263324f2a322135e9900e969fc5b22f73955db31d629a592296ffacf803. Root revisouoverview/água+8frames; adaptação aprovada para entrega, humano/input/ações/animação pendentes. Não houve ação de pesca nem peixe obtido pelo teste. Spec IN_PROGRESS/promoçãoNO. Evidência: docs/validation/spec_farm_pixelart_cohesion_and_ingame_review_v1_execution_report.md. Sem reclassificar baselines globais.

## Fundação de habilidades — 2026-09-10

SCOPED_PASS técnico: avatar, status, lunge, ledger de pontos/save v3 e D01 cooldown compartilhado. 131/131 EditMode integrados + cinco cenários PlayMode PASS. Sem aceite humano, sem promoção de UI/pixel art/equilíbrio. [Relatório](validation/skills_sdd_v1/execution/FOUNDATION_REPORT.md).


## Farm v4 — rodada visual 14 (2026-09-10)

Comparativo atualizado: docs/validation/farm_keyart_v4/progress-review.html. Uma integração visual após dois bloqueios de compilação da fase de skills; stage14c.log exit0 e EditMode14/14 PASS. Árvores preservadas na comparação de71 transformações. Revisão visual aceita apenas o ganho local de vegetação; caminhos/composição permanecem pendentes. Regra visual-iteration-budget adicionada e gerada para Codex. IN_PROGRESS, sem promoção. Evidência completa no REPORT.md da farm_keyart_v4.

Farm v15 (2026-09-10): ambient details/spacing integrated;33/33EditMode,18/18PlayMode views/routes PASS. Scope/limits: docs/validation/farm_keyart_v4/REPORT.md. Overall spec IN_PROGRESS.

Farm ambient follow-up:41s nativePlayModePASS (two fish jumps20.000061s apart); timestamp HTML in farm_keyart_v4/ambient_runtime15. No gameplay/art changes, global spec stillIN_PROGRESS.

Farm v16 — código e materiais aplicados offline; diff check/revisão estática concluídos. Unity/EditMode/PlayMode NOT RUN; cena permanece15. Spec única: spec_farm_contact_pixel_consistency_v16. Relatório farm_keyart_v4/contact_v16/REPORT.md.

Farm v16 integrado: geração Unity e53/53EditMode PASS; PlayModeD16contatos+3seleções PASS,5frames e71árvores preservados. Comparativo stage16 e galeria em docs/validation/farm_keyart_v4/contact_v16/index.html. Promoção final pendente dos critérios residuais explicitados no REPORT.md; bloqueio Unity anterior resolvido.

Farm v16:follow-up do rochedo oeste integrado;10/10navegação e19checksPlayPASS, comparação visual local aprovada. Arte e71árvores preservadas; evidência contact_v16/rock_fix/.

Farm v17:popa do barco reconstruída emAseprite e integrada; geraçãoUnity exit0 e revisão visualPASS. Outros props semamputação confirmada no conjunto examinado; emenda degrama no gate leste pendente. docs/validation/farm_keyart_v4/boat_v17/index.html.

Farm v18 piloto integrado: barco4poses5s e galinha19frames com movimento seguro, interação e restore.62/62EditMode +PlayC60sPASS, artevisívelrevisada. Vaca/ovelha/cabra semciclosnovos; fullherd/humano pendentes. docs/validation/farm_keyart_v4/motion_v18/index.html eREPORT.md.

Farm v19 integrado: vaca/ovelha/cabra com 19 poses cada e perfis por ID; cordeiro ausente corrigido no catálogo gerado. 65/65 EditMode e Play B com quatro animais por 60s PASS, interação/restore/limites verificados. Proporções revisadas, 71 árvores preservadas. Aceitação humana/input pendente. Evidência: docs/validation/farm_keyart_v4/herd_v19/REPORT.md e index.html.


Farm v20 integrado: +27 arbustos baixos e54 tufos decorativos, Ground/4 semcolliders; árvores eacessos preservados. GeraçãoUnity exit0 e15/15testesdecoração/navegação PASS. Comparativo/evidência: docs/validation/farm_keyart_v4/groundcover_v20/index.html eREPORT.md. Aceitaçãovisualhumana pendente.

