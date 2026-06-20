# Human Play Mode Scenario — fable_11 (City Interiors, Doors & Schedule Anchors)

> Run AFTER regenerating TownScene (`CindarsHope/Create Scenes/Town Scene`) and the editor validator
> (`CindarsHope/Validate/Fable City Schedule (fable_11)`). Covers CA-1..CA-4 + anti-regression.
> Required for Phase 3 acceptance. Game rule: `city_rules.md` Rules 3/4/5/6.

## Preconditions

- TownScene regenerated; `ValidateFableCitySchedule` PASS (23 NPCs ≥3 anchors, 12 interiors y>+40,
  24 paired doors, ≥1 shop-gated door).
- Enter Play Mode in TownScene.

## Scenario A — Living town (CA-1)

1. Observe NPCs at their stalls/posts during the day (Work block).
2. Advance time toward evening (use the day-advance / time controls). Day shopkeepers should head to the
   tavern/plaza (Social), then home (Home) late at night.
   - PASS: NPCs visibly move between anchors across the cycle (not static all day).
   - PASS: NPCs blocked by a collider en route teleport to the anchor after ~5 s (no permanent stuck).

## Scenario B — Night vendor inversion (CA-4)

1. During the day, walk up to Yael (night market, SE/S) and to Maelor.
2. Press interact.
   - PASS: prompt reads "<name> nao esta disponivel agora."; shop/dialogue does NOT open; a toast shows
     the reason.
3. Advance to night.
   - PASS: Yael/Maelor are now at the night market and their shop/dialogue opens normally.

## Scenario C — Day shopkeeper unavailability (CA-4)

1. At deep night, approach a normal day shopkeeper (e.g. Renko/Brumdar) at/near their home.
2. Press interact.
   - PASS: unavailability prompt + toast; shop does not open.
3. During work hours, the same NPC opens shop normally.
   - PASS: normal buy/sell/dialogue flow intact (anti-regression).

## Scenario D — Doors & interiors (CA-3)

1. Walk to a house door (exterior). Press interact.
   - PASS: player teleports into a minimal one-room interior (offscreen band, y>+40); camera follows.
2. Inside, walk to the return door. Press interact.
   - PASS: player teleports back outside near the same house; camera follows.
3. Confirm no scene transition / loading occurred (same scene).
   - PASS: no scene load, playfield bounds/camera unaffected by the interior band.

## Scenario E — Closed-shop door blocked (CA-4 / decision 6.4-A)

1. Find a house door wired to a shop NPC while that vendor is closed (out of hours).
2. Press interact.
   - PASS: door is BLOCKED — player does NOT enter; prompt/toast shows the opening-hours notice.
3. When the vendor is open, the same door works normally.
   - PASS.

## Scenario F — Anti-regression

- Dialogue/shop/quest flows for available NPCs work exactly as before.
- Save/load: NPC positions persist; schedule block re-derives from the hour on load (no new save field).
- No NPC uses GameObject.Find at runtime; interiors do not interfere with the playfield camera/bounds.

## Result

- [ ] Scenario A PASS
- [ ] Scenario B PASS
- [ ] Scenario C PASS
- [ ] Scenario D PASS
- [ ] Scenario E PASS
- [ ] Scenario F PASS
- Overall: ____ (ACCEPTED only when all PASS)
