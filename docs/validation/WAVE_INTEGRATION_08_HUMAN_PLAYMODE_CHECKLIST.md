# WAVE_INTEGRATION_08 — Human Play Mode Checklist

Date created: 2026-06-08
Branch: dev
Status: PENDING

---

## Preconditions

- [ ] WAVE_INTEGRATION_07 Play Mode checklist passed (or Partial Pass accepted)
- [ ] Pull latest `dev` branch
- [ ] Run `CindarsHope > Advanced > Legacy > Scenes > Create MVP FarmScene` (regenerate scene)
- [ ] FarmScene opens with 0 red errors
- [ ] DebugHud is visible in hierarchy as `DebugHud` GameObject

---

## Step 1 — Enter Play Mode

- [ ] Press Play
- [ ] Console shows 0 red errors on startup
- [ ] Player spawns at expected position (~(-2, 0))
- [ ] Camera follows player

---

## Step 2 — Verify HUD Visible

- [ ] Left panel appears: "Actions" box on left side of screen
- [ ] Right panel appears: "Cindar's Hope - Debug Info" box on right side
- [ ] Both panels are scrollable if content overflows
- [ ] No red errors from DebugHud initialization

Left panel should show (at minimum):
- Interaction state line
- Commands list (WASD, E, Q, etc.)

Right panel should show (at minimum):
- "Day: X"
- "Gold: 0" (or loaded value)
- "HP: X/X"
- "Hunger: X/X"
- "Stamina: X/X"
- "Inventory:" section (empty or items)

---

## Step 3 — Test Interaction Prompt

- [ ] Walk player near FarmPlot_00 (around (0, -1.5))
- [ ] Left panel shows: "Interacao: [plot action prompt]" (e.g., "Arar", "Interagir")
- [ ] Walk player away from plot
- [ ] Left panel shows: "Interacao: nenhum alvo"
- [ ] No red errors

---

## Step 4 — Test Feedback from Farm Action

- [ ] Walk to FarmPlot_00 and interact (press E)
- [ ] After interaction, left panel shows "Feedback: [action result]" for ~3 seconds
- [ ] Feedback disappears after display time
- [ ] No red errors

---

## Step 5 — Provision Debug Loadout

- [ ] With game running, open menu: `CindarsHope > Integration > Debug > Provision Farm Smoke Loadout`
- [ ] Console shows: `[DebugLoadout] Farm Smoke Loadout complete.`
- [ ] Right panel "Inventory:" section now shows added items
- [ ] No red errors

---

## Step 6 — Test SellPoint Economy Feedback

- [ ] Walk player north to SellPoint zone (around (3.5, 7.5))
- [ ] Left panel shows: "Interacao: Vender"
- [ ] Press E to sell
- [ ] Left panel "Feedback:" shows: "Vendido! +X ouro." (for ~3 seconds)
- [ ] Right panel "Gold:" value increases by the sold amount
- [ ] Right panel "Economia:" shows: "Vendido! +X ouro."
- [ ] Right panel "Inventory:" shows removed items
- [ ] No red errors during sale

---

## Step 7 — Test Empty Inventory Sell Feedback

- [ ] Sell all items (repeat Step 6 until inventory empty)
- [ ] Interact with SellPoint on empty inventory
- [ ] Left panel "Feedback:" shows: "Nada para vender." (for ~2 seconds)
- [ ] Gold value unchanged
- [ ] No red errors

---

## Step 8 — Test Player Condition Visibility

- [ ] Note current Stamina and Hunger values in right panel
- [ ] Perform a FarmPlot action (e.g., till/water)
- [ ] Right panel "Stamina:" value decreases (if action has stamina cost)
- [ ] Alternatively: wait for hunger to tick if day advances
- [ ] No red errors

---

## Step 9 — Test Hotbar Display

- [ ] Right panel "Hotbar:" shows current selected slot and item
- [ ] Press 1-6 keys (hotbar slot select)
- [ ] Right panel "Hotbar:" updates to new slot index and item
- [ ] No red errors from hotbar state

---

## Step 10 — Verify Input Not Trapped

- [ ] While DebugHud is visible, player can still move (WASD)
- [ ] Player can still interact (E key)
- [ ] DebugHud does not block any gameplay keys
- [ ] No input routing errors in console

---

## Step 11 — Exit Play Mode

- [ ] Press Stop
- [ ] Console shows 0 red errors from shutdown
- [ ] Scene is not corrupted

---

## Pass Criteria

All items below must be checked to advance to WAVE_INTEGRATION_09:

- [ ] DebugHud visible (left + right panels) in Play Mode
- [ ] Interaction prompt updates when player approaches/leaves interactable
- [ ] Action feedback appears after farm interaction
- [ ] Sale feedback "Vendido! +X ouro." appears after selling
- [ ] Gold value increases after selling
- [ ] "Economia:" line in right panel shows last transaction
- [ ] Inventory list updates after sale/provision
- [ ] Player movement not blocked by HUD
- [ ] No red errors in any step

---

## Failure Criteria (blocks WAVE_INTEGRATION_09)

- [ ] DebugHud panels not visible in Play Mode
- [ ] Interaction prompt stays "nenhum alvo" even when next to interactable
- [ ] No feedback after sale (no "Vendido! +X" in left panel)
- [ ] Gold does not change after selling items
- [ ] Red exception from DebugHud, SellPoint, or InteractionSystem

---

## Acceptable Debt (does NOT block WAVE_INTEGRATION_09)

- Active skill slots not shown in DebugHud — deferred to WAVE_INTEGRATION_10/11
- Full 6-slot hotbar visual not shown — deferred
- "Economia: nenhuma transacao" if no sale was made yet — expected
- Some item IDs skipped during provisioning — acceptable (documented in 06A audit)

---

## Executor Notes

Date executed: ___________
Unity version: ___________
DebugHud panels visible: ___________
Interaction prompt working: ___________
Farm action feedback: ___________
Sale feedback "Vendido!": ___________
Gold after sale: ___________
Economia field: ___________
Inventory updated: ___________
Input not trapped: ___________
Red errors: ___________
Overall result: PASS / PARTIAL_PASS / FAIL
Can start WAVE_INTEGRATION_09: YES / NO
