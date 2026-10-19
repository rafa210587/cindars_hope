# Human Play Mode Scenario — fable_64 (Death Screen Canvas + Corpse Recovery Messaging)

> Status: PENDING (DEFERRED_TO_FINAL_VALIDATION). Execute in Unity Play Mode.

## Goal

Verify the canvas death screen exhibits the correct death/corpse/XP information, is keyboard-navigable, blocks gameplay input, does not close on Esc, and that the real respawn flow is unchanged.

## Preconditions

- Open the relevant scenes (FarmScene/TownScene for overworld; CaveScene for cave).
- The old `DeathScreenController` component (now a no-op shim) may still be on a scene object — harmless. The new `DeathScreenCanvas` is created automatically at runtime (RuntimeInitializeOnLoadMethod).

## Scenario A — Cave death with items (full loop)

1. Enter the cave; collect a few items and some gold; descend at least to cave level 2.
2. Let the player be defeated by enemies/hazard.
3. EXPECTED — Canvas death screen appears (no IMGUI window):
   - Headline "Voce caiu na caverna." and `Local: Caverna — nivel N`.
   - Corpse line: `Corpo: <N> itens + <X> ouro deixados`.
   - `Nivel do corpo: <N>`.
   - Recovery instruction: "Seus itens estao num corpo no nivel N — volte para recupera-los."
   - If XP was lost: `XP perdido: <amount>`.
4. EXPECTED — gameplay input is blocked (player does not move; modal active).
5. Press Esc → screen DOES NOT close (death requires a choice).
6. Use arrow keys → focus highlight stays on "Renascer na Fonte de Anya" (future slot is disabled/greyed).
7. Press Enter (or click Respawn) → screen closes; player is at Anya's Fountain at full HP (respawn already applied at death-time).
8. Return to cave level N → recover the corpse via the existing CorpseRecovery flow → items/gold restored.

## Scenario B — Overworld death (empty state)

1. Be defeated outside the cave (e.g., FarmScene).
2. EXPECTED — Canvas death screen shows `Voce morreu em <SceneName>.`, `Local: <SceneName>`, no corpse section, no XP section, "Nada foi deixado para tras."
3. Esc does not close; Enter/click Respawn closes; player restored.

## Checklist

- [ ] Open: screen opens on cave defeat and on overworld death (once per death).
- [ ] No IMGUI window anywhere (no "YOU DIED" GUI.Window).
- [ ] Input blocking: player cannot move while screen is open.
- [ ] Esc/back: does NOT close the death screen.
- [ ] Focus: keyboard focus on Respawn; future slot disabled with honest label.
- [ ] Empty state: overworld death shows coherent no-corpse message.
- [ ] Error/edge: two quick deaths do not stack/duplicate the screen.
- [ ] No movement while modal open.
- [ ] Respawn action: closes screen; real respawn unchanged (HP/stamina/mana restored, moved to fountain).
- [ ] Corpse recovery: returning to corpse level restores items/gold (flow unchanged).
