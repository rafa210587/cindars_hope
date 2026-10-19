# WAVE INTEGRATION 25 - Human Play Mode Checklist

Status: PENDING_HUMAN_EXECUTION

Wave: WAVE_INTEGRATION_25 — MVP+ 03: Town NPC Schedules + Dialogue Expansion

## Prerequisites

- Unity Editor open with TownScene loaded
- Press Play
- Prior waves (20-24) can be run concurrently or independently

---

## Checklist

### Boot and Scene Load

- [ ] Open TownScene in Unity Editor: no red errors in console
- [ ] Press Play: player spawns correctly, camera follows player
- [ ] No exception stack traces at boot

### NPC Presence and Reachability

- [ ] Find Pip (TownEntrance/Market area): visible and reachable
- [ ] Find Thalindra (Archive area): visible and reachable
- [ ] Find Sylveth (SeedShop area): visible and reachable
- [ ] Find Brumdar (Forge area): visible and reachable
- [ ] Find Renko (GeneralStore area): visible and reachable
- [ ] Find at least 2 more NPCs from extended roster

### Dialogue — 5 Distinct NPCs

- [ ] Talk to Pip: distinct greeting and tutorial/delivery lines appear
- [ ] Talk to Sylveth: distinct seed/farm/crop tips appear
- [ ] Talk to Brumdar: distinct blacksmith/repair lines appear
- [ ] Talk to Renko: distinct general store/bargain lines appear
- [ ] Talk to Thalindra: quest offer + shop lines appear (see Thalindra section)
- [ ] Confirm all 5 dialogues are visually distinct (different text, different role)

### Thalindra Quest + Shop (MANDATORY)

- [ ] Approach Thalindra and press interact
- [ ] Opening dialogue appears
- [ ] After opening dialogue: "! Qual e a tarefa?", "Comprar", "Vender", "Adeus" choices visible
- [ ] Choose "! Qual e a tarefa?": quest offer panel opens (quest_first_supplies_for_cindar)
- [ ] Accept quest or close quest panel
- [ ] Re-interact with Thalindra
- [ ] Choose "Comprar": Buy panel opens, items visible
- [ ] Close Buy panel: returns to Thalindra dialogue choices
- [ ] Choose "Vender": Sell panel opens, inventory visible
- [ ] Close Sell panel: returns to Thalindra dialogue choices
- [ ] Choose "Adeus": interaction ends cleanly, input returns

### Shop Service NPC

- [ ] Interact with Sylveth (seed shop): service prompt clear, shop opens
- [ ] Buy panel shows items from shop_sylveth
- [ ] Sell panel shows player inventory
- [ ] Back button returns to shop menu
- [ ] Esc closes interaction cleanly

### NPC Movement (Wanderer NPCs)

- [ ] Observe a wanderer-mode NPC (e.g., Pip or Liora if visible): moves within bounds
- [ ] Interact with wandering NPC: NPC stops wandering during dialogue
- [ ] Close dialogue: NPC resumes wandering or stays put

### Schedule on Day Advance (if testable)

- [ ] Trigger day advance (TAB key or equivalent): verify NPCs teleport to home positions
- [ ] OR document TIME_BLOCK_DEBT: schedule advances only on DayStartedEvent, not tested intra-day
- [ ] NpcScheduleService: "Day N: moved npc_X to home position" log visible in console

### Save and Load NPC State

- [ ] Talk to at least 3 NPCs (set HasMet = true)
- [ ] Save game (F5 or save key)
- [ ] Quit to main menu (or stop/play)
- [ ] Load game
- [ ] Verify NPCs are at their saved positions
- [ ] Verify previously-met NPCs still show correct HasMet state (no first-time greeting again)

### Input Blocking (Modal Guards)

- [ ] Open dialogue with any NPC
- [ ] Try dash (Space): dash should be blocked during dialogue
- [ ] Try dodge (double-tap direction): should be blocked
- [ ] Try farming action: should be blocked
- [ ] Close dialogue: input returns, dash/dodge/farming work again
- [ ] Open Thalindra buy panel
- [ ] Try dash: blocked
- [ ] Try dodge/block: blocked
- [ ] Close shop: input returns

### NPC During Interaction (No Wander Away)

- [ ] Interact with a wandering NPC
- [ ] Wait 10+ seconds during dialogue
- [ ] Confirm NPC stays in place (wandering paused)
- [ ] Close dialogue: NPC correctly resumes wander mode

### Edge Cases

- [ ] Exit dialogue cleanly (Esc): no modal stuck, no input lost
- [ ] Open dialogue with NPC, open shop, Esc: modal stack clears correctly
- [ ] Interact with Alaric (dialogue-only NPC): dialogue shows, no shop opens

---

## Expected Outcome

All checklist items PASS before this wave can move to ACCEPTED status.

## Time Block Debt Note

NpcScheduleService advances only on DayStartedEvent (not per-hour period).
Intra-day schedule transitions (Morning/Midday/Evening/Night) are TIME_BLOCK_DEBT.
Document in test run: "TIME_BLOCK_DEBT confirmed — schedule transitions only at day boundary."
