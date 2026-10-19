# WAVE_INTEGRATION_10 — Human Play Mode Checklist: Skill Tree UI + Active Skill Equip

**Date:** 2026-06-08
**Branch:** dev
**Validator:** (human)
**Status:** PENDING_HUMAN_EXECUTION

---

## Prerequisites

1. Human has run `CindarsHope/Create Scenes/Farm Scene` in Unity Editor to regenerate FarmScene (required to pick up WAVE_INTEGRATION_09/10 changes).
2. Human enters Play Mode in the FarmScene.

---

## Checklist

### AC-01: Skill tree opens with U key

- [ ] Press U while in FarmScene Play Mode (no modal open)
- [ ] Skill tree IMGUI panel opens showing "Skill Trees" header
- [ ] Panel shows "Skill Points disponíveis: X"

### AC-02: Skill tree closes with Esc or U

- [ ] Press Esc while skill tree is open — panel closes, player regains movement
- [ ] Re-open with U, press U again — panel closes

### AC-03: Player movement is blocked while skill tree is open

- [ ] With skill tree open, press WASD — player does NOT move (ModalManager.HasActiveModal blocks movement input)

### AC-04: Skill tree shows tree list (home screen)

- [ ] Home screen shows 5 tree names: Melee, Ranged, Magic, Survival, Crafting
- [ ] Each tree shows its display name and description

### AC-05: Navigate into a skill tree

- [ ] Click/button on a tree name — moves to tree detail view
- [ ] Tree detail shows node list (at least 5-11 nodes per tree)
- [ ] "Voltar" button returns to home screen

### AC-06: Node info is shown

- [ ] Each node shows DisplayName, status ([Comprado]/[Disponível]/[Bloqueado]/[Sem pontos]), cost (X SP)
- [ ] Node shows description text
- [ ] Node shows requirements (prerequisites / min level)

### AC-07: Purchase a node (if skill points available)

- [ ] Start with skill points > 0 (use debug O key to grant XP and level up, or check initial points)
- [ ] Find an unlocked node — status [Disponível] — click "Comprar"
- [ ] Node becomes [Comprado]
- [ ] Skill points count decreases by node cost

### AC-08: Cannot purchase without points

- [ ] With 0 skill points, "Comprar" button is greyed out
- [ ] Status shows [Sem pontos]

### AC-09: Prerequisites enforced

- [ ] Node with unmet prerequisites shows [Bloqueado]
- [ ] "Comprar" is not available for blocked nodes

### AC-10: Purchase equippable skill auto-assigns active slot

- [ ] Purchase an EquippableSkill node (e.g. melee_offhand_cut, ranged_charged_shot, any UnlockSkillAction)
- [ ] Feedback message shows "Skill ativa alocada em R." (or T/Y/G for next available slot)

### AC-11: Active slot display inside skill tree panel

- [ ] In tree detail view, "Active slots:" row shows R/T/Y/G and their assigned skill action IDs (or "vazio")
- [ ] After purchase + auto-assign, slot shows the skillActionId

### AC-12: Active slot display in DebugHud (WAVE_INTEGRATION_10 new feature)

- [ ] With skill tree panel closed, DebugHud info panel shows "Active Skills:" section
- [ ] Shows [R]: [T]: [Y]: [G]: lines
- [ ] After purchasing and equipping a skill, the relevant slot shows the skillActionId (not "vazio")

### AC-13: Manual active slot assignment

- [ ] Open skill tree, navigate to a purchased equippable skill
- [ ] Press R — feedback "Skill ativa alocada em R." or slot assignment
- [ ] Open again — check active slots row shows the assignment

### AC-14: Close and reopen preserves state

- [ ] Open skill tree, purchase a node
- [ ] Close skill tree, reopen
- [ ] Previously purchased node still shows [Comprado]
- [ ] Active slots still show assigned skills

### AC-15: Save and load preserves skill tree (if save is tested)

- [ ] Press F5 (save), F9 (load)
- [ ] After load, purchased nodes and active slots are preserved
- [ ] Skill points are correct

### AC-16: Skill points update on level up

- [ ] Press O multiple times to trigger level up
- [ ] DebugHud shows increased "Skill points: X"
- [ ] Open skill tree — "Skill Points disponíveis: X" matches DebugHud

### AC-17: Respec (if tested)

- [ ] In tree detail, if respec is available (gold >= respecCost), check that reset works
- [ ] All nodes become unpurchased after respec
- [ ] Points are restored

---

## Known Debt (not tested in this checklist)

- Canvas SkillTreePanel (prefab-based) not wired — IMGUI only in MVP
- Active skill slot HUD strip (Canvas) not implemented — DebugHud shows text only
- Skill gameplay effects (combat/farm) deferred to WAVE_INTEGRATION_11

---

## Sign-off

| Item | Result | Notes |
|------|--------|-------|
| AC-01 open | | |
| AC-02 close | | |
| AC-03 movement blocked | | |
| AC-04 tree list | | |
| AC-05 tree navigation | | |
| AC-06 node info | | |
| AC-07 purchase | | |
| AC-08 no points | | |
| AC-09 prerequisites | | |
| AC-10 auto-equip | | |
| AC-11 active slots in panel | | |
| AC-12 active slots in DebugHud | | |
| AC-13 manual slot assign | | |
| AC-14 state preserved | | |
| AC-15 save/load | | |
| AC-16 level up points | | |
| AC-17 respec | | |

**Overall result:** [ ] PASS [ ] PARTIAL [ ] FAIL

**Validated by:** _______________
**Date:** _______________
