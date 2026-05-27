# Registry de specs a implementar

Fonte unica de specs futuras: `docs/specs/a_implementar/`. A pasta raiz `specs/` foi removida e nao deve ser recriada.

A antiga spec 00 de reconciliacao documental foi reclassificada como implementada/parcial em `docs/specs/implementados/spec_docs_001_single_source_specs_refinements_reconciliation_parcial.md`. Ela nao deve ser executada novamente.

| Ordem | Spec | Status | Dependencia | Observacao |
|---|---|---|---|---|
| 13 | [spec_enemy_ai_roster_bestiary_faction_locks_runtime.md](a_implementar/spec_enemy_ai_roster_bestiary_faction_locks_runtime.md) | Implementado parcial - escopo residual ativo | 00-12 | Enemy chase/patrol AI completo. Pendente: ranged enemies, faction locks, bestiary UI. Bloqueia: 14, 15, 17. |
| 14 | [spec_cave_runtime_generation_checkpoints_boss_gates.md](a_implementar/spec_cave_runtime_generation_checkpoints_boss_gates.md) | Implementado parcial - escopo residual ativo | 00-13 | Cave generation/runtime completo. Pendente: stable run guarantee, boss gates validation, confinement hardening. Bloqueia: 15, 17. |
| 17 | [spec_ui_ux_full_gameplay_inventory_hotbar_menus.md](a_implementar/spec_ui_ux_full_gameplay_inventory_hotbar_menus.md) | Implementacao parcial - MVP gameplay em codigo | 00-16 implementadas/parciais | Shops/sell/inventory/equipment/attributes/skills e input modal MVP entregues; Canvas final, pause/options, cave/corpse/toasts e Play Mode ainda pendentes. |
| 17C | [spec_ui_gameplay_closeout_skill_shop_prompts_actions_hud.md](a_implementar/spec_ui_gameplay_closeout_skill_shop_prompts_actions_hud.md) | Implementado em codigo - Play Mode humano pendente | 15-17 implementadas/parciais | Gates automaticos registrados; manter ativo ate validar `U`, `K`, `L`, buy/sell e save/load em Play Mode. |
| 17D | [spec_ui_gameplay_shop_injection_equipment_slot_picker_closeout.md](a_implementar/spec_ui_gameplay_shop_injection_equipment_slot_picker_closeout.md) | Implementado em codigo - Unity/Play Mode pendentes | 17C implementada em codigo | Shop context validation e slot picker implementados; manter ativo ate batchmode/Play Mode validar buy/sell, `L` e save/load. |
| 17E | [spec_ui_gameplay_shop_session_lifecycle_npc_readiness_closeout.md](a_implementar/spec_ui_gameplay_shop_session_lifecycle_npc_readiness_closeout.md) | Implementado em codigo - Unity/Play Mode pendentes | 17D implementada em codigo | ShopManager persistente e readiness idempotente implementados; manter ativo ate validar sessoes, buy/sell e save/load em Play Mode. |
