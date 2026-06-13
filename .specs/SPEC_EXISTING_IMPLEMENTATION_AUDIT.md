# Cindar's Hope — Existing Implementation Audit

> **Status:** auditoria documental executada parcialmente por ChatGPT/GitHub connector.  
> **Local:** `.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`  
> **Tipo:** relatório de auditoria; não é spec implementável.  
> **Branch auditada:** `dev`  
> **Data:** 2026-06-07  
> **Escopo:** mapear estado real documentado/registrado do repo antes de gerar specs runtime da WAVE 01+.  
> **Limitação:** a busca de código via conector GitHub retornou resultados vazios para termos de `Assets/`; portanto este relatório usa evidência documental/registry/status e marca auditoria local de código como obrigatória antes de executar runtime.

---

## 1. Objetivo e uso correto

Este relatório existe para evitar que as próximas specs sejam criadas como implementação do zero quando o repo já tem sistemas implementados ou parciais.

Uso correto:

```text
1. Usar este relatório para decidir se a próxima spec será nova, residual, hardening, reconciliation ou future.
2. Não usar este relatório como fonte de design de gameplay.
3. Não substituir `docs/design/SPEC_SOURCE_MAP.md` nem os direction docs.
4. Não usar este relatório para promover specs para implementados.
5. Não usar este relatório para liberar execução runtime em massa antes da 01Q ou exceção humana explícita.
```

---

## 2. Fontes lidas

```text
docs/project/CURRENT_STATE.md
docs/IMPLEMENTATION_STATUS.md
.specs/SPEC_REGISTRY_IMPLEMENTED.md
.specs/SPEC_REGISTRY_TO_IMPLEMENT.md
.specs/SPEC_GENERATION_ROADMAP_MASTER.md
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
.specs/SPEC_VALIDATION_MATRIX_MASTER.md
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
CLAUDE.md
.claude/commands/plan-wave.md
.claude/commands/finish-spec.md
```

Leitura não usada:

```text
PROJECT_LOG.md — não lido, porque a auditoria encontrou evidência suficiente nos registries/status para esta passada documental.
```

---

## 3. Auditoria de Código Local — Resultados

A auditoria de código local **foi concluída** em 2026-06-07 via `rg` local.

Sistemas encontrados:

```text
GameEventBus: FOUND — Assets/_Game/Scripts/Core/GameEventBus.cs
SaveManager: FOUND — Assets/_Game/Scripts/Save/SaveManager.cs
InventoryManager: FOUND — Assets/_Game/Scripts/Inventory/InventoryManager.cs
GameTimeManager: FOUND — Assets/_Game/Scripts/Core/GameTimeManager.cs
CaveRuntimeMaterializer: FOUND — Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs
EnemyBrain: FOUND — Assets/_Game/Scripts/Enemy/EnemyBrain.cs
ShopManager: FOUND — Assets/_Game/Scripts/Economy/ShopManager.cs
EconomyManager: FOUND — Assets/_Game/Scripts/Economy/EconomyManager.cs
SkillTreeManager: FOUND — Assets/_Game/Scripts/Skills/SkillTreeManager.cs
BestiaryManager: FOUND — Assets/_Game/Scripts/Enemy/BestiaryManager.cs
```

Todos os sistemas core existem no repositório. Classificações abaixo usam esta evidência + registries/status.

Status deste relatório:

```text
Documental audit: COMPLETE
Local code search audit: COMPLETE — all core systems confirmed
Unity validation: NOT REQUIRED (no code changes made)
Runtime changes: NONE
```

---

## 4. Estado global do repo

Estado consolidado a partir de `CURRENT_STATE.md` e `IMPLEMENTATION_STATUS.md`:

```text
MVP code-complete: YES
MVP build-validated C#: YES
Docs validation: YES
Phase 2 Unity validators: NOT RUN — pending human/local Unity
Phase 3 Play Mode: NOT RUN — pending human/local Unity
MVP final accepted: NOT YET
FASE 10+: BLOCKED until Phase 2-3 or explicit human decision
```

Interpretação para este fluxo:

```text
A solicitação humana atual autoriza continuar geração/auditoria de specs.
Ela não autoriza executar runtime em massa.
Ela não resolve Phase 2-3.
Ela não permite promover runtime/gameplay para ACCEPTED sem evidência.
```

---

## 5. Matriz por wave

| Wave | Área | Status real auditado | Evidência documental | Risco de duplicação | Recomendação |
|---|---|---|---|---|---|
| WAVE 00 | Governança/spec generation | PARTIAL/ACTIVE | Template, roadmap, wave protocol, validation matrix, final human checklist existem; esta auditoria agora existe como relatório. | Médio, se redirects antigos em `a_implementar` forem lidos por glob. | Tratar docs canônicos como guias, não specs. Deletar redirects se ainda existirem. |
| WAVE 01 | Stable IDs / data registries | IMPLEMENTED / RESIDUAL | `spec_data_001_ids_registries_e_scriptableobjects.md` implementado; pendência: completar taxonomia sem quebrar IDs. | Alto se criar registry paralelo. | Próxima spec deve ser audit/hardening de stable IDs, não implementação do zero. |
| WAVE 01 | Event bus / base events | IMPLEMENTED / RESIDUAL | `spec_core_001_event_bus_e_eventos_base.md` implementado; pendência: payloads futuros e unsubscribe. | Alto se criar novo bus/event contracts paralelos. | Gerar spec residual para contratos/event catalog, mantendo `GameEventBus`. |
| WAVE 01 | Save/load / restore order / providers | PARTIAL / RESIDUAL | Save/load JSON parcial; schema migration v2 completo; architecture reorg adicionou `ISaveSectionProvider` e provider piloto. | Alto se refatorar SaveManager sem compatibilidade. | Gerar specs de save como reconciliation/hardening; não substituir SaveManager. |
| WAVE 01 | Testing Quality Gate 01Q | MISSING / PENDING | `spec_test_harness_editmode_playmode_quality_gate.md` existe em `a_implementar`. | Médio se runtime specs pularem testes. | Executar 01Q antes de Waves 02+ runtime em massa. |
| WAVE 02 | Time/calendar/weather/lunar | PARTIAL | Hunger/stamina/status/time implementado como MVP; weather/lunar/calendar completo não comprovado. | Médio. | Gerar time/calendar specs como delta; confirmar código local antes. |
| WAVE 03 | Quest/objective/event | PARTIAL / UNKNOWN | Town NPC/dialogue schedule quests completo; quest core genérico não comprovado como sistema amplo. | Alto se NPC hooks forem confundidos com quest engine genérica. | Auditar localmente Quest/Objective/Reward antes de spec nova. |
| WAVE 04 | UI foundation/input/modal/HUD | PARTIAL / RESIDUAL | UI/input/shop/sell bundle completo; UI 17B implementou GameplayInputRouter, pause, toasts, hints, death screen, checkpoint menu; Play Mode pendente. | Alto se recriar modal/input stack. | Gerar residual/hardening, não nova fundação. |
| WAVE 05 | Inventory/equipment/hotbar/items | PARTIAL/COMPLETE MIXED | Inventory slots completo; tools/equipment/hotbar parcial; equipment durability partial. | Alto se separar ItemInstance/ItemData sem compatibilidade. | Gerar specs com audit local obrigatório; focar gaps: item instance mutável, durability, hotbar final. |
| WAVE 06 | UI screens finais | PARTIAL / RESIDUAL | Shops/sell/equipment/attributes/skills tiveram incrementos 17C-F; SPEC 17 ampla permanece aberta. | Alto se recriar telas já entregues. | Gerar specs por tela como closeout/residual baseado em SPEC 17 status. |
| WAVE 07 | Farm/economy/crafting | PARTIAL/COMPLETE MIXED | Farm planting/irrigation completo; economy shop/pricing completo; crafting complete; farm loop parcial. | Médio. | Gerar specs residuais para loops avançados; não refazer shop/crafting base. |
| WAVE 08 | City/NPC/dialogue/services | COMPLETE CODE / PENDING PLAY MODE | Town NPC/dialogue/schedule/quests completo em código; Play Mode final pendente. | Médio-alto se recriar NPC/dialogue stack. | Gerar specs de hardening/expansão apenas após audit local. |
| WAVE 09 | Player/stats/skills/magic | PARTIAL/RESIDUAL | SPEC 16 skill tree em código; progression partial; player combat/spells partial; hunger/stamina/time MVP. | Alto se reimplementar skill tree/active slots/respec. | Specs devem ser residual/hardening; validar current skill tree antes. |
| WAVE 10 | Cave/combat/enemies/death | PARTIAL/RESIDUAL ACTIVE | Cave runtime/procedural/stable run partial; SPEC 13A-F, 14A-B implementados em código; death/corpse partial. | Muito alto. | Não gerar implementação do zero. Gerar deltas: snapshot/respawn/boss gates/death recovery/hardening. |
| WAVE 11 | Bestiary/knowledge | PARTIAL/FUTURE HOOK | SPEC 13E implementou BestiaryManager event-driven/save; assets/Play Mode pendentes. | Alto se criar segundo bestiary. | Gerar specs como UI/research/future hooks ou hardening do BestiaryManager. |
| WAVE 12 | Future systems | FUTURE | Pets/companions/social/endgame marcados como futuro. | Baixo agora. | Não gerar para primeira execução, salvo decisão explícita. |

---

## 6. Inventário por domínio/sistema

### 6.1 Core/bootstrap/events

Status:

```text
IMPLEMENTED / RESIDUAL
```

Evidência documental:

```text
spec_core_001_event_bus_e_eventos_base.md — implementado
spec_core_002_bootstrap_managers_e_runtime_references.md — implementado parcial
Architecture reorg SPEC_04-11 adicionou installers/services/contracts, mas há backlog residual.
```

Recomendação:

```text
Não gerar GameEventBus novo.
Não gerar bootstrap novo.
Specs futuras devem auditar payload/event catalog, unsubscribe, installers e validators.
```

### 6.2 Data/IDs/registries

Status:

```text
IMPLEMENTED / RESIDUAL
```

Evidência documental:

```text
spec_data_001_ids_registries_e_scriptableobjects.md — implementado.
Pendência declarada: completar taxonomia sem quebrar IDs existentes.
```

Recomendação:

```text
A próxima spec de stable IDs deve ser residual/hardening: consolidar registry, validar IDs existentes, impedir duplicatas e documentar migration policy.
```

### 6.3 Save/load

Status:

```text
PARTIAL / RESIDUAL
```

Evidência documental:

```text
spec_save_001_json_save_load_cross_scene.md — parcial.
spec_save_002_schema_migration_v2.md — completo.
SaveManager v5 captura diversas seções, incluindo SkillTree, Death, Economy, Crafting, GameTime, StatusEffects, EquipmentDurability e Npcs.
Architecture reorg adicionou ISaveSectionProvider e HotbarSectionProvider piloto.
```

Recomendação:

```text
Não substituir SaveManager.
Gerar specs de save como restore-order/section-ownership/provider-scaling/hardening.
```

### 6.4 Inventory/items/equipment/hotbar

Status:

```text
PARTIAL/COMPLETE MIXED
```

Evidência documental:

```text
Inventory slots/capacity/UI final — completo.
Tools/equipment/hotbar — parcial.
Equipment durability/environment/loot — parcial.
Architecture reorg adicionou ItemUseKind, ItemUseContractResolver e campos em ItemDataSO.
```

Recomendação:

```text
Auditar código local antes de gerar specs de ItemInstance.
Priorizar deltas: item instance mutável, hotbar final, durability/repair/upgrades.
```

### 6.5 Farm/economy/crafting

Status:

```text
PARTIAL/COMPLETE MIXED
```

Evidência documental:

```text
Farm irrigation/soil/planting UI — completo.
Farm loop/world activities — parcial.
Economy/shop/stock/pricing/UI — completo.
Crafting queue/workstations/recipes/UI — completo.
```

Recomendação:

```text
Não refazer shop/crafting base.
Gerar specs residuais para farm world persistence, advanced crop rules, processing hardening e orders.
```

### 6.6 Time/calendar/weather/lunar

Status:

```text
PARTIAL / UNKNOWN
```

Evidência documental:

```text
Hunger/stamina/status/time — completo MVP.
Calendar/weather/lunar completo não comprovado nos registries/status lidos.
```

Recomendação:

```text
Antes de gerar specs WAVE 02, Claude Code deve auditar `GameTime`, calendar, weather, lunar e day transition no código.
```

### 6.7 Quest/objective/reward

Status:

```text
PARTIAL / UNKNOWN
```

Evidência documental:

```text
Town NPC/dialogue/schedule/quests — completo em código.
Quest engine genérica com condition/trigger/reward/idempotency não comprovada por este audit documental.
```

Recomendação:

```text
Auditar localmente QuestDefinition/QuestState/Objective/Reward/QuestFlag antes de gerar WAVE 03.
```

### 6.8 UI/input/modals/screens

Status:

```text
PARTIAL / RESIDUAL
```

Evidência documental:

```text
UI/input/shop/sell bugfix bundle completo.
SPEC 17C-F diversos closeouts completos com Play Mode humano registrado em 2026-05-26.
SPEC 17 ampla permanece aberta; Canvas final, pause/options, cave/corpse/toasts pendentes em parte.
UI 17B implementou GameplayInputRouter, PauseMenuController, NotificationToastController, ContextHintController, DeathScreenController e CaveCheckpointSideMenuController.
```

Recomendação:

```text
Não criar UI foundation do zero.
Gerar specs de tela como residual/closeout e validar contra ModalManager/InputRouter existentes.
```

### 6.9 City/NPC/dialogue/services

Status:

```text
COMPLETE CODE / PLAY MODE FINAL PENDING
```

Evidência documental:

```text
Town NPC/dialogue/schedule/quests — completo em código.
Play Mode humano final ainda indicado como pendente para UX/wandering/save-load interativo.
```

Recomendação:

```text
Gerar city/NPC futuras como hardening/expansion, não foundation.
```

### 6.10 Player/stats/skills/magic

Status:

```text
PARTIAL/RESIDUAL
```

Evidência documental:

```text
Progression partial.
SPEC 16 skill trees/active slots/respec Anya em código; Unity compile/Play Mode humano pendentes.
Player combat/weapons/spells/skill actions partial.
```

Recomendação:

```text
Não recriar skill tree.
Specs de WAVE 09 devem auditar e fechar gaps: magic, spell sources, UI details, capstones/future.
```

### 6.11 Cave/combat/enemies/death

Status:

```text
PARTIAL/RESIDUAL ACTIVE
```

Evidência documental:

```text
Enemy taxonomy/actions/brain/bestiary/spawn resolver SPEC 13A-F implementados em código com pendências de assets/wiring/Play Mode.
Cave spawn plan/materialization/run stability SPEC 14A implementado em código com pendências.
Snapshot replay SPEC 14B implementado em código; validators/Play Mode pendentes.
Combat/damage/status/elements partial.
Death/corpse base partial; Anya/respawn/corpse restore não fechado.
```

Recomendação:

```text
WAVE 10 deve ser quase toda residual/hardening/delta.
Não gerar cave/combat/enemy systems do zero.
Priorizar snapshot/respawn/redistribution/boss gates/death recovery/validator assets.
```

### 6.12 Bestiary/knowledge

Status:

```text
PARTIAL / FUTURE HOOK
```

Evidência documental:

```text
SPEC 13E implementou BestiaryManager event-driven com save/load por IDs, GameSaveData.Bestiary e GameBootstrap injection.
Assets de bestiary e Play Mode pendentes.
Bestiary UI/research service ainda future.
```

Recomendação:

```text
Não criar outro BestiaryManager.
Gerar specs de knowledge/UI/research como hardening/future hooks.
```

---

## 7. Locks fortes e paralelização

Locks fortes identificados:

```text
GameEventBus/event contracts;
SaveManager/GameSaveData/SaveData/migrations/save providers;
GameBootstrap/global managers/installers;
GameplayInputRouter/ModalManager/UIEvents;
InventoryManager/ItemDataSO/ItemUse contracts;
ShopManager/economy pricing/stock;
CraftingRuntime/recipes/workstations;
Quest state/objective/reward contracts;
Cave runtime/run seed/snapshot/materializer;
EnemyDataSO/EnemyBrain/action databases/spawn resolver;
BestiaryManager/GameSaveData.Bestiary;
Scenes/prefabs/ScriptableObjects/databases;
.specs registries/status files.
```

Paralelização segura antes de 01Q:

```text
Somente geração de specs e auditorias documentais.
Não executar runtime em massa.
```

Paralelização segura depois de 01Q e audit local:

```text
Docs-only/governance specs podem rodar em paralelo se não editarem registry/status.
Runtime specs devem ser 1 por vez quando tocarem save, event bus, bootstrap, UI global, scene/prefab, cave runtime ou contracts compartilhados.
```

---

## 8. Specs candidatas a residual/hardening

```text
01_spec_stable_ids_registry_runtime.md -> residual/hardening, não implementação do zero.
01_spec_game_event_contracts_runtime.md -> residual/event catalog hardening, não novo bus.
01_spec_save_restore_order_contract_runtime.md -> residual/hardening em SaveManager v5.
01_spec_save_section_ownership_registry.md -> residual/provider scaling e ownership, alinhado a ISaveSectionProvider piloto.
01_spec_save_provider_architecture_runtime.md -> residual/scaling, não refactor massivo.
05 inventory/equipment/hotbar specs -> residual por sistema existente.
06 UI screens specs -> residual/closeout por SPEC 17C-F e 17B existentes.
07 economy/shop/crafting specs -> residual/advanced systems, não base.
08 city/NPC/dialogue specs -> expansion/hardening.
09 skill tree/active slots/respec specs -> residual, não recriar.
10 cave/combat/enemy/death specs -> residual/hardening/delta.
11 bestiary specs -> residual/future hook.
```

---

## 9. Specs candidatas a future ou bloqueadas

```text
Pets;
companions;
social/romance/casamento/poliamor;
partner helper;
festival minigames;
endgame level 100/101;
final choice cinematic;
research service;
full accessibility settings.
```

Também manter como bloqueado para execução runtime em massa:

```text
Waves 02+ runtime antes da 01Q ou exceção humana explícita documentada.
```

---

## 10. Riscos de duplicação imediatos

```text
1. Gerar stable IDs como novo registry paralelo.
2. Gerar GameEventBus/event system novo em vez de catalogar/hardening eventos existentes.
3. Gerar SaveManager/provider architecture sem respeitar SaveManager v5 e ISaveSectionProvider piloto.
4. Gerar UI foundation sem considerar GameplayInputRouter/ModalManager/UIEvents já existentes.
5. Gerar SkillTree do zero apesar de SPEC 16 existir em código.
6. Gerar cave/enemy runtime do zero apesar de SPEC 13A-F e 14A-B existirem em código.
7. Gerar BestiaryManager novo apesar de SPEC 13E já existir.
```

---

## 11. Recommended Next Actions

### 11.1 Antes de gerar WAVE 01 runtime

```text
1. Executar auditoria local em Claude Code usando `rg` conforme 00_spec_existing_implementation_audit.md.
2. Confirmar se os dois redirects antigos de WAVE 00 foram fisicamente removidos.
3. Garantir que 01Q esteja pronta para execução antes das Waves 02+ runtime em massa.
```

### 11.2 Próxima spec a gerar

Se o usuário quiser continuar geração antes de auditoria local completa, gerar a próxima spec como **hardening/residual**, não como implementação do zero:

```text
01_spec_stable_ids_registry_runtime.md
```

Mas o título/escopo recomendado deve ser:

```text
Stable IDs Registry Audit and Hardening
```

Objetivo recomendado:

```text
consolidar registry existente, auditar IDs atuais, validar duplicatas e definir política de stable IDs sem quebrar IDs persistidos.
```

### 11.3 Specs que não devem ser geradas como implementação do zero

```text
GameEventBus;
SaveManager;
InventoryManager;
ShopManager;
CraftingRuntime;
Town NPC/dialogue base;
SkillTree runtime;
EnemyBrain/spawn resolver;
BestiaryManager;
Cave materializer/snapshot base.
```

---

## 12. Pendências de validação humana/Unity

Ainda pendente conforme status docs:

```text
Phase 2 Unity validators locais;
Phase 3 Play Mode humano;
MVP final acceptance.
```

Este relatório não altera esse status.

---

## 13. Conclusão

A próxima etapa pode continuar, mas com ajuste de estratégia:

```text
As specs WAVE 01+ não devem presumir sistemas inexistentes.
A maior parte da fundação já existe parcial ou completamente.
A próxima spec útil deve ser residual/hardening, começando por stable IDs, events e save.
```

Status final deste relatório:

```text
Audit report created: YES
Runtime/code changed: NO
Assets changed: NO
Packages changed: NO
ProjectSettings changed: NO
SPEC_EXECUTION_ORDER changed: NO
Docs validation: NOT RUN in ChatGPT/GitHub connector
Recommended max status before local validation: PARTIAL
```
