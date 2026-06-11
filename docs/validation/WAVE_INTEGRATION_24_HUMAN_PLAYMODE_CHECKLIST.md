# WAVE_INTEGRATION_24 — Human Play Mode Checklist

**Date:** 2026-06-11
**Wave:** WAVE_INTEGRATION_24 — Farm Loop Depth + Daily Goals
**Status:** PENDING HUMAN VALIDATION

This checklist must be completed by a human in Unity Play Mode before this wave can be marked ACCEPTED.

---

## Prerequisites

Before starting this checklist:
- Open Unity Editor (URP/LTS)
- Ensure FarmScene is loaded (or open via CindarsHope/Repair and Validate Project)
- Press Play

---

## Step 1 — Scene Boot

| # | Action | Expected | Pass? |
|---|--------|----------|-------|
| 1.1 | Open FarmScene in Unity Editor | Scene loads without errors in Console | |
| 1.2 | Press Play | No red errors in Console; player is present | |
| 1.3 | Check Console for [FarmDailyGoalRuntimeBootstrap] | Log: "FarmDailyGoalService instanciado via bootstrap." | |

---

## Step 2 — HUD / Feedback Visible

| # | Action | Expected | Pass? |
|---|--------|----------|-------|
| 2.1 | Observe screen | HUD/feedback toast area visible (WAVE23 HUD) | |
| 2.2 | DebugHud visible | IMGUI overlay shows player stats/inventory | |

---

## Step 3 — Daily Goal Presence

| # | Action | Expected | Pass? |
|---|--------|----------|-------|
| 3.1 | Check Console on scene boot | No daily goal error messages | |
| 3.2 | Observe feedback area | Goal awareness available (no required UI display for MVP+) | |

---

## Step 4 — Plant Seed

| # | Action | Expected | Pass? |
|---|--------|----------|-------|
| 4.1 | Walk to a plot (Raw or TilledDry) | Interaction prompt shows ("Arar" or "Cultivar") | |
| 4.2 | Interact (E key) with Raw plot | Plot → TilledDry; feedback shown | |
| 4.3 | Interact with TilledDry plot | Menu shows plant/water options | |
| 4.4 | Select plant seed option (seed in inventory) | Seed consumed; plot → PlantedDry or PlantedWet | |
| 4.5 | Try to plant without seeds | Feedback "No seeds in inventory." or "Seed not in inventory." | |

---

## Step 5 — Water Crop (Invalid and Valid)

| # | Action | Expected | Pass? |
|---|--------|----------|-------|
| 5.1 | Try to water PlantedWet plot | Feedback "Cannot water this plot." or "Ja irrigado" | |
| 5.2 | Interact with PlantedDry plot | Menu shows water option | |
| 5.3 | Select water | Plot → PlantedWet; feedback "Crop watered." | |

---

## Step 6 — Advance Day / Grow

| # | Action | Expected | Pass? |
|---|--------|----------|-------|
| 6.1 | Advance day (TAB key or sleep/wait mechanism) | DayStartedEvent fires; plot grows if PlantedWet | |
| 6.2 | Check plot state after day advance | If PlantedWet → DaysGrown+1; if DaysGrown >= GrowthDays → ReadyToHarvest | |
| 6.3 | Advance enough days for mature crop | Plot → ReadyToHarvest; prompt changes to "Colher" | |

---

## Step 7 — Harvest Mature Crop

| # | Action | Expected | Pass? |
|---|--------|----------|-------|
| 7.1 | Interact with ReadyToHarvest plot | Select "Colher" | |
| 7.2 | Execute harvest | Item(s) added to inventory; feedback "Colheita: [item] adicionado ao inventário." | |
| 7.3 | Check inventory | Harvested item appears | |

---

## Step 8 — Daily Goal: First Harvest

| # | Action | Expected | Pass? |
|---|--------|----------|-------|
| 8.1 | After harvest (step 7) | Feedback "Meta diária concluída!" visible (or progress toast "Meta [daily_goal_first_harvest]: 1/1") | |
| 8.2 | Check Console | [FarmDailyGoalService] log: "Goal 'daily_goal_first_harvest' completed on day X." | |
| 8.3 | Harvest again | No duplicate completion toast | |

---

## Step 9 — Sell Crop

| # | Action | Expected | Pass? |
|---|--------|----------|-------|
| 9.1 | Walk to SellPoint (Zone_ShippingSellpoint) | Interaction prompt shows | |
| 9.2 | Interact with SellPoint | Sells item; gold increases | |
| 9.3 | Feedback | "Vendido: [item] por Xg" shown via ShippingSummaryService | |

---

## Step 10 — Daily Goal: Sell First Crop

| # | Action | Expected | Pass? |
|---|--------|----------|-------|
| 10.1 | After selling (step 9) | Feedback "Meta diária concluída!" (daily_goal_sell_first_crop) | |
| 10.2 | Check Console | [FarmDailyGoalService] log: "Goal 'daily_goal_sell_first_crop' completed on day X." | |

---

## Step 11 — Gold Updates

| # | Action | Expected | Pass? |
|---|--------|----------|-------|
| 11.1 | After sell | Gold display updated in DebugHud | |
| 11.2 | GoldChangedEvent published | Console log or HUD gold counter changes | |

---

## Step 12 — Save

| # | Action | Expected | Pass? |
|---|--------|----------|-------|
| 12.1 | Save game (F5 or SaveManager.SaveGame) | Feedback "Jogo salvo." | |
| 12.2 | No errors in Console | Save completes successfully | |

---

## Step 13 — Load

| # | Action | Expected | Pass? |
|---|--------|----------|-------|
| 13.1 | Load game (F9 or SaveManager.LoadGame) | Feedback "Jogo carregado." | |
| 13.2 | Check plot states | All plots restore to saved state | |
| 13.3 | Check inventory | Items persist after load | |
| 13.4 | Check gold | Gold persists after load | |

---

## Step 14 — Daily Goals Persistence (limited)

| # | Action | Expected | Pass? |
|---|--------|----------|-------|
| 14.1 | After load, complete a goal | Goal still tracks correctly | |
| 14.2 | Note: goals do NOT persist across game restart (SAVE_LOAD_DAILY_GOAL_DEBT) | Expected — documented debt | EXPECTED |

---

## Step 15 — Try Duplicate Harvest

| # | Action | Expected | Pass? |
|---|--------|----------|-------|
| 15.1 | Try to harvest a non-ready plot | Feedback "Crop is not ready." | |
| 15.2 | Harvest a crop that was already harvested | Plot resets; no duplication | |

---

## Results Summary

| Category | Pass? | Notes |
|----------|-------|-------|
| Scene boot + bootstrap | | |
| HUD/feedback visible | | |
| Plant/water/grow/harvest | | |
| Daily goal: first harvest | | |
| Daily goal: sell first crop | | |
| Shipping/gold | | |
| Save/load crop state | | |
| No duplication | | |

**Overall Pass?** ___

**Executor:** ___
**Date executed:** ___
**Unity version:** ___
