# WAVE_INTEGRATION_15 — Human Play Mode Checklist

Date: 2026-06-10
Status: PENDING_HUMAN_PLAYMODE

---

## Pre-Requisites

- [ ] WAVE14 human wiring applied (FarmScene has crafting stations)
- [ ] WAVE13 human wiring applied (scene transitions work)
- [ ] QuestGiverInteractable added to npc_thalindra in TownScene (see wiring instructions)
- [ ] QuestRuntimeBootstrap creates automatically (verify in console on Play)

---

## Checklist

### Setup
1. [ ] Open TownScene in Unity Editor
2. [ ] Press Play — player spawns
3. [ ] Observe Console — "[QuestRuntimeBootstrap] Quest runtime initialized" appears
4. [ ] Confirm "Registry quests: 1" in log (quest_first_supplies_for_cindar registered)

### Quest Offer
5. [ ] Approach npc_thalindra — interaction prompt appears ("Interagir com npc_thalindra")
6. [ ] Interact with Thalindra — QuestGiverInteractedEvent published
7. [ ] Quest offer panel appears (IMGUI box titled "QUEST OFFER")
8. [ ] Panel shows: title "Suprimentos para Cindar", description, objectives (wood x2, stone x2), reward (Ouro: 50)
9. [ ] Press "Aceitar" — quest accepted
10. [ ] Console shows "[QuestService] Quest accepted: quest_first_supplies_for_cindar"

### Quest Log
11. [ ] Press J — Quest Log opens
12. [ ] Active quest "Suprimentos para Cindar" visible with objectives
13. [ ] Objectives show "○ item_material_wood: 0/2" and "○ item_material_stone: 0/2"
14. [ ] Press Escape — Quest Log closes

### Objective Progress
15. [ ] Travel to FarmScene (if scene transitions wired) or spawn items via Debug menu
16. [ ] Collect 2x item_material_wood from TreeResource
17. [ ] Open Quest Log (J) — "✓ item_material_wood: 2/2" shows as completed
18. [ ] Collect 2x item_material_stone from RockResource
19. [ ] Quest log shows "[PRONTA PARA ENTREGAR]" indicator (yellow)
20. [ ] Console: "[QuestService] Quest ready to complete: quest_first_supplies_for_cindar"

### Turn-In and Reward
21. [ ] Return to TownScene, approach npc_thalindra
22. [ ] Interaction prompt changes to "Entregar quest para npc_thalindra"
23. [ ] Interact — turn-in triggers automatically
24. [ ] Console: "[QuestService] Quest completed: quest_first_supplies_for_cindar. Gold: 50."
25. [ ] Player gold increased by 50
26. [ ] Quest Log shows quest in CONCLUÍDAS section

### Idempotency
27. [ ] Interact with Thalindra again — no second reward dialog
28. [ ] Quest state = Completed; no re-apply of gold

---

## Pass Criteria

ALL 28 steps checked = PLAYMODE_VALIDATED
PARTIAL (steps 1-14 checked) = PARTIAL — objective tracking and reward pending
NOT RUN = PENDING_HUMAN_PLAYMODE (current status)

---

## Known Issues / Residual Risks

- QuestStateSection is in-memory: quest state lost on Stop → Play
- Scene transitions to FarmScene may require WAVE13 wiring to be complete
- If npc_thalindra wiring not done, checklist stops at step 5
- IMGUI panels may overlap with other IMGUI panels (DebugHud, InventoryPanel)
