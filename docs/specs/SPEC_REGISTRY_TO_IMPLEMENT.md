# Registry de specs a implementar

Fonte unica de specs futuras: `docs/specs/a_implementar/`. A pasta raiz `specs/` foi removida e nao deve ser recriada.

A antiga spec 00 de reconciliacao documental foi reclassificada como implementada/parcial em `docs/specs/implementados/spec_docs_001_single_source_specs_refinements_reconciliation_parcial.md`. Ela nao deve ser executada novamente.

Documentos canônicos de governança que não são specs executáveis:

```text
docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md
docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
```

| Ordem | Spec | Status | Dependencia | Observacao |
|---|---|---|---|---|
| 00.04 | [00_spec_existing_implementation_audit.md](a_implementar/00_spec_existing_implementation_audit.md) | A implementar - auditoria/governanca | 00 docs canonicos | Audita o estado real implementado/parcial/faltante do repo antes de gerar specs runtime para evitar duplicacao e definir residual/hardening/future. |
| 01.01 | [01_spec_stable_ids_registry_runtime.md](a_implementar/01_spec_stable_ids_registry_runtime.md) | A implementar - hardening/residual | 00.04 audit + spec_data_001 implementada | Audita e endurece IDs/registries existentes sem recriar arquitetura; bloqueia specs que persistem/referenciam IDs. |
| 01.02 | [01_spec_game_event_contracts_runtime.md](a_implementar/01_spec_game_event_contracts_runtime.md) | A implementar - hardening/residual | 01.01 + spec_core_001 implementada | Audita e endurece GameEventBus/event contracts existentes sem recriar barramento; define catálogo, payload rules e unsubscribe lifecycle. |
| 01.03 | [01_spec_save_restore_order_contract_runtime.md](a_implementar/01_spec_save_restore_order_contract_runtime.md) | A implementar - hardening/residual | 01.01 + 01.02 + save specs implementadas | Audita e endurece restore order/save sections existentes sem reescrever SaveManager; produz matriz de sections, defaults, dependencies e post-restore notifications. |
| 01.04 | [01_spec_save_section_ownership_registry.md](a_implementar/01_spec_save_section_ownership_registry.md) | A implementar - hardening/residual | 01.03 + save specs implementadas | Cria ou consolida ownership registry de save sections, owners, defaults, dependencies e migration responsibility sem migrar tudo para providers. |
| 01.05 | [01_spec_save_provider_architecture_runtime.md](a_implementar/01_spec_save_provider_architecture_runtime.md) | A implementar - gradual/residual | 01.04 + save ownership registry | Consolida arquitetura incremental de save providers sem substituir SaveManager, sem schema change e sem migração massiva de sections. |
| 01Q | [spec_test_harness_editmode_playmode_quality_gate.md](a_implementar/spec_test_harness_editmode_playmode_quality_gate.md) | A implementar - quality gate fundacional | 01 validacao Unity + rules/skills atuais | Deve ser executada antes das novas waves runtime para exigir EditMode tests, PlayMode/manual scenarios, regression tests e evidencia de risco residual. |
| 13 | [spec_enemy_ai_roster_bestiary_faction_locks_runtime.md](a_implementar/spec_enemy_ai_roster_bestiary_faction_locks_runtime.md) | Implementado parcial - escopo residual ativo | 00-12 | Enemy chase/patrol AI completo. Pendente: ranged enemies, faction locks, bestiary UI. Bloqueia: 14, 15, 17. |
| 14 | [spec_cave_runtime_generation_checkpoints_boss_gates.md](a_implementar/spec_cave_runtime_generation_checkpoints_boss_gates.md) | Implementado parcial - escopo residual ativo | 00-13 | Cave generation/runtime completo. Pendente: stable run guarantee, boss gates validation, confinement hardening. Bloqueia: 15, 17. |
| 17 | [spec_ui_ux_full_gameplay_inventory_hotbar_menus.md](a_implementar/spec_ui_ux_full_gameplay_inventory_hotbar_menus.md) | Implementacao parcial - MVP gameplay em codigo | 00-16 implementadas/parciais | Shops/sell/inventory/equipment/attributes/skills e input modal MVP entregues; Canvas final, pause/options, cave/corpse/toasts e Play Mode ainda pendentes. |

---

## Specs Geradas Pós-Refinamento — Registry Reconciliado

Após geração da batch 36 (~130 specs), o registry foi reconciliado. Specs estão todas presentes em `docs/specs/a_implementar/`, nenhuma foi movida para `implementados/` e nenhuma foi deletada.

### WAVE 00 — Governança & Auditoria

| Ordem | Spec | Status | Wave | Observacao |
|---|---|---|---|---|
| 00.04 | `00_spec_existing_implementation_audit.md` | A implementar - auditoria/governanca | 00 | Audita estado real antes de specs runtime; bloqueia 01+ |

### WAVE 01 — Hardening & Quality Gate

| Ordem | Spec | Status | Wave | Observacao |
|---|---|---|---|---|
| 01.01-01.05 | `01_spec_*.md` (5 specs) | A implementar - hardening/residual | 01 | Estabiliza IDs, eventos, save contracts; bloqueia WAVE 02+ |
| 01Q | `spec_test_harness_editmode_playmode_quality_gate.md` | A implementar - quality gate | 01 | Fundacional; deve executar antes de WAVE 02+ |

### WAVE 02-10 — Core Runtime

| Wave | Specs | Contagem | Status | Observacao |
|---|---|---|---|---|
| 02 | Time/Calendar/Weather/Lunar | ~8 | A implementar | Fundação de tempo; bloqueia WAVE 03+ |
| 03 | Quest/Objective/Event | ~10 | A implementar | Sistema de quests; bloqueia WAVE 06+ |
| 04 | UI Foundation | ~15 | A implementar | Modal/focus/HUD base |
| 05 | Inventory/Equipment/Hotbar | ~12 | A implementar | Item instance, equipment, hotbar |
| 06 | Economy/Crafting | ~8 | A implementar | Pricing, recipes, shops |
| 07 | Farm/Crops/Tools | ~11 | A implementar | Plantio, colheita, ferramentas |
| 08 | City/NPC/Dialogue | ~8 | A implementar | NPCs, diálogo, serviços |
| 09 | Player/Skills/Magic | ~11 | A implementar | Atributos, skills, magia |
| 10 | Cave/Combat/Enemies | ~12 | A implementar | Procedural cave, combat, AI |

**Total WAVE 02-10:** ~85 specs core/runtime

### WAVE 17-24 — Future Mapped

| Wave | Specs | Contagem | Status | Observacao |
|---|---|---|---|---|
| 17 | Social/Romance | ~4 | Future mapped | Não executar agora |
| 18 | UI Advanced | ~4 | Future mapped | Não executar agora |
| 19 | Endgame Level 100-101 | ~4 | Future mapped | Não executar agora |
| 20 | Mana/Lunar Deep | ~4 | Future mapped | Não executar agora |
| 21 | Farm Automation | ~4 | Future mapped | Não executar agora |
| 22 | Bestiary/Knowledge/Research | ~4 | Future mapped | Não executar agora |
| 23 | Pets | 4 | **HOLD / BLOCKED_SCOPE** | **NÃO EXECUTAR** — pets bloqueados até explícita autorização |
| 24 | Companions / Festival / Endgame | ~4 | Future mapped | Não executar agora |

**Total WAVE 17-24:** ~30 specs future; 4 pets bloqueados

### Resumo

- ✓ **Total:** ~130 specs geradas + reconciliadas
- ✓ **Core núcleo:** ~85 specs (WAVE 00-10)
- ✓ **Futures:** ~30 specs (WAVE 17-24)
- ✓ **Pets bloqueados:** 4 (WAVE 23)
- ✓ **Nenhuma duplicata ou obsoleta**
- ✓ **Nenhuma spec movida para implementados**
- ✓ **Pronto para executar WAVE 00 + 01**
