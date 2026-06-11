# WAVE_INTEGRATION_22 — Debt Deduplication Matrix

**Date:** 2026-06-10
**Status:** DEDUPLICATED

---

## Contexto

WAVE13-21 geraram múltiplos reports com debts parcialmente sobrepostos.
Esta matrix registra duplicidades encontradas e as resolve para um ID canônico.

---

## Duplicidades Resolvidas

| Entradas Duplicadas | Canonical DebtId | Decisão | Notas |
|---|---|---|---|
| "Quest save debt" em WAVE15 (QUEST_SAVE_LOAD_IN_MEMORY) + WAVE18 (implementado) | DEBT-SAVE-003 / OBSOLETE | WAVE15 debt foi implementado em WAVE18. Debt técnico = OBSOLETE. Permanece DEBT-TEST-001 (Play Mode ainda não executado). | WAVE15 deferred → WAVE18 resolve completamente |
| "Cave run save debt" em WAVE16 (CAVE_RUN_SAVE_LOAD_DEBT) + WAVE18 (ALREADY_IMPLEMENTED) | OBSOLETE | CaveRunManager.CaptureSaveData estava ALREADY_IMPLEMENTED. Cave run save é funcional. Debt = OBSOLETE. | Não há DebtId canônico; resolvido |
| "NPC schedule debt" em WAVE12 + WAVE20 (B011) | DEBT-NPC-001 | Consolidado em DEBT-NPC-001. Report WAVE12 é a fonte original; WAVE20 B011 é a citação. | Um único ID |
| "HUD IMGUI debt" em WAVE08/11/19 + WAVE20 (B008) | DEBT-UX-001 | Consolidado em DEBT-UX-001. Múltiplos waves citam o mesmo problema de HUD IMGUI vs Canvas final. | Um único ID |
| "Cave enemy HP save" em WAVE17 (PICKUP_SAVE_LOAD_DEBT) + WAVE18 (CAVE_ENEMY_HP_SAVE_DEBT) + WAVE20 (B006) | DEBT-SAVE-001 | Consolidado em DEBT-SAVE-001. A nomenclatura variou por wave mas é o mesmo debt. | Um único ID |
| "Companion save" em múltiplos waves + WAVE20 (B007) | DEBT-SAVE-002 | Consolidado em DEBT-SAVE-002. | Um único ID |
| "Equipment panel modal guard" em WAVE09 + WAVE20 (B014) | DEBT-UX-004 | Consolidado em DEBT-UX-004. WAVE09 identificou como debt; WAVE20 confirmou como P3. | Um único ID |
| "CraftingStation not in FarmScene" em WAVE14 + WAVE20 (B005) + WAVE21 (reclassificado P2) | DEBT-SCENE-005 | Consolidado em DEBT-SCENE-005. B005 reclassificado P2 em WAVE21. | Um único ID |
| "Visual art placeholders" em WAVE08/09/11/19 + WAVE20 (B015) | DEBT-UX-005 | Consolidado em DEBT-UX-005. Múltiplos waves citam placeholders. | Um único ID |
| "Inventory tooltip minimal" em WAVE09/11 + WAVE20 (B013) | DEBT-UX-003 | Consolidado em DEBT-UX-003. | Um único ID |
| "Smoke test recipes TEMPORARY" em WAVE14 + implícito em WAVE20 | DEBT-FARM-002 | WAVE14 marcou recipes como TEMPORARY_CRAFTING_TEST_RECIPE. Consolidado como DEBT-FARM-002. | Um único ID |

---

## Debts Marcados como OBSOLETE (Resolvidos em Wave Posterior)

| Debt Original | Wave Original | Wave Resolveu | Motivo |
|---|---|---|---|
| QUEST_SAVE_LOAD_IN_MEMORY (WAVE15) | WAVE15 | WAVE18 | QuestStateSectionSaveData criado; wired em SaveManager |
| CAVE_RUN_SAVE_LOAD_DEBT (WAVE16) | WAVE16 | WAVE18 | CaveRunManager.CaptureSaveData ALREADY_IMPLEMENTED |
| ACTIVE_SKILL_RUNTIME_DEFERRED (WAVE10) | WAVE10 | WAVE11 | Skill slots implementados em WAVE11 |
| INVENTORY_REWARD_DEBT (WAVE07) | WAVE07 | WAVE07 | Resolvido na mesma wave |
| ECONOMY_LOOP_DEFERRED (WAVE07) | WAVE07 | WAVE07 | SellPoint wired |
| QUEST_SAVE_DEFERRED (WAVE15) | WAVE15 | WAVE18 | Implementado |

---

## Debts Novos Identificados (não estavam no WAVE20 register)

| DebtId | Origem | Descrição |
|---|---|---|
| DEBT-FARM-002 | WAVE14 | Smoke test recipes marcadas TEMPORARY — não estava em WAVE20 explicitamente |
| DEBT-FARM-003 | WAVE05-07 | Farm loop depth — apenas 1 crop/resource type; não estava em WAVE20 |
| DEBT-CAVE-001 | WAVE17 | Apenas 1 enemy type/1 loot item — não estava em WAVE20 explicitamente |
| DEBT-COMBAT-001 | WAVE17 | Combat balance não verificado em Play Mode |
| DEBT-AUDIO-001 | WAVE_geral | Sem sistema de audio feedback — nunca foi scope |
| DEBT-TEST-001 | WAVE_geral | 18+ checklists PENDING_HUMAN_EXECUTION — consolidado agora |
| DEBT-TEST-002 | WAVE_geral | Sem automated Play Mode tests |
| DEBT-TEST-003 | FIX-001B | FIX-001B Play Mode checklist pendente |
| DEBT-QUEST-002 | WAVE15/18 | Quest save implementado mas Play Mode not verified |

---

## Mapeamento WAVE20 B-IDs → Canonical DebtIds

| WAVE20 BugId | Canonical DebtId | Mudança |
|---|---|---|
| B001 | DEBT-SCENE-001 | Renomeado |
| B002 | DEBT-SCENE-002 | Renomeado |
| B003 | DEBT-SCENE-003 | Renomeado |
| B004 | DEBT-SCENE-004 | Renomeado |
| B005 | DEBT-SCENE-005 | Renomeado + reclassificado P2 (WAVE21) |
| B006 | DEBT-SAVE-001 | Renomeado |
| B007 | DEBT-SAVE-002 | Renomeado |
| B008 | DEBT-UX-001 | Renomeado |
| B009 | DEBT-UX-002 | Renomeado |
| B010 | DEBT-CAVE-002 | Renomeado |
| B011 | DEBT-NPC-001 | Renomeado |
| B012 | DEBT-FARM-001 | Renomeado |
| B013 | DEBT-UX-003 | Renomeado |
| B014 | DEBT-UX-004 | Renomeado |
| B015 | DEBT-UX-005 | Renomeado |

---

## Resultado da Deduplicação

| Métrica | Antes | Depois |
|---|---|---|
| Entradas totais (WAVE20 register) | 15 | — |
| Debts novos identificados (WAVE13-19 extras) | — | +9 |
| Debts obsoletos removidos | — | -6 |
| Total canônico | — | **21** |
| Duplicidades resolvidas | 11 | — |
