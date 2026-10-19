# Phase 10V — survival and crafting integrated validation

**Spec:** `spec_skills_20_survival_crafting_capstones_v1`  
**Validation date:** 2026-09-10  
**Unity:** 6000.5.7f1  
**Result:** PASS — automated mechanics gate complete

## Acceptance coverage

| Criterion | Evidence | Result |
|---|---|---|
| AC01–AC02 Telisandra, run identity and Last Breath ordering | Earlier Phase 20S focused EditMode 9/9 and PlayMode 4/4; integrated Skills regression below | PASS |
| AC03 Thoren daily transaction and functional outputs | Focused EditMode 29/29 and Living Forge PlayMode 5/5 | PASS |
| AC04 failure/cancel/load/respec and structural economy cap | Focused tests plus live pricing paths; every variant keeps base sale value and SaveCommon saves at most one unit/day | PASS |
| AC05 simple save DTOs and cave stable-run isolation | Survival/crafting providers persist IDs, numbers and enum values; no cave generation path changed | PASS |

## Fresh evidence

- Canonical catalog generation: exit 0, 66 nodes, 31 actions and all five tree counts PASS —
  `Logs/skills-phase20-amendment-canonical-generate-r2.log`.
- Living Forge generation final repeat: exit 0, 34 unchanged and `noChanges=True` —
  `Logs/skills-phase20-amendment-livingforge-generate-r3.log`.
- Focused EditMode: 29/29 PASS —
  `TestResults/skills-phase20-livingforge-final-editmode.xml`.
- Integrated Skills EditMode: 257/257 PASS —
  `TestResults/skills-phase20-integrated-editmode-r3.xml`.
- Living Forge PlayMode: 5/5 PASS —
  `TestResults/skills-phase20-livingforge-final-playmode.xml`.
- Caveborn/Telisandra PlayMode: 4/4 PASS —
  `TestResults/skills-phase20-caveborn-playmode-fresh.xml`.

Core inputs were identified by SHA-256 after the final test edits: variant catalog
`EAC0F4848168B6BCC337CBC6323C1B1F9F792E92EECC974AF6EFC0411BCDEE52`, resolver
`A7D61677FBF10D520FD696E57F9A429CE05201D82FBC813C82B7926A33E55EC6`, station
`2C7A2DA24F25276E5BD3A56E7AA06487A25D763F23D04583F62BC7D99A4CE723`, runtime
`48B6A3F24E24DB78F79DBB7EA0E2E5B63CF6A51730CA778A18F98A398EC9EEBA`, focused
EditMode tests `5C3157689FDAEE396B905D2ADF7C6BE50292883C669D09C686712E08139953F2` and
`923476ED208D839236F26BC023E2B90FD63DBD27023E6A9DA7970287A42DC838`, and PlayMode
test `EC48CBD0C850031C8C714B46457EC2BC9EBC1EDCED2EBDDF5C0C71076BF3A1A8`.

The first integrated run exposed a stale save-version expectation (3 versus the current DTO version
4). The isolated reproduction failed the same way; the expectation was corrected without changing
runtime, and the final integrated run passed 257/257. The first Living Forge PlayMode run also
revealed that a real-time wait allowed an unrelated bootstrap to reach its 120-frame error. The test
now advances the pure crafting station deterministically and the complete class passes 5/5.

## Economy delta

| Source | Maximum Phase 20 delta | Sink affected | Verdict |
|---|---:|---|---|
| Q1/Q2/potency sale value | 0g across SellPoint, shop and shipping resolvers | None | PASS |
| SaveCommonMaterial | One eligible common unit per game day, minimum one still consumed | Crafting material sink | PASS structural |
| Consumable potency | +15%/+35%, with R3 +8% composed from the base payload; batch 1–5 | Provision consumption | PASS structural |
| Equipment quality | +5%/+10% max durability; R3 composes +8% once | Repair/replacement cadence | PASS structural |

The empirical requirement that the daily benefit remain below 20% of median stage income belongs to
`spec_skills_21_balance_acceptance_v1`. No Phase 20 result claims that measurement.

## Independent review

The first final review required fresh Caveborn evidence, which then passed 4/4. A narrower second
review found that live pricing-service parity covered base/Q1/Q2 but did not route both R3 variant
identities through every service. It also found an unused legacy `BuffDurationMultiplier` field in
the variant descriptor. Both were removed from the final code: all five identities now traverse all
four pricing paths, and the stale descriptor member no longer exists. The focused re-audit returned
PASS without blockers and T06 is complete.

## Runtime and human boundary

Automated PlayMode proves the crafting lifecycle and state transitions. It does not constitute human
or visual acceptance of the future skill-tree UI, loadout UI or animations. Those remain in their
ordered later phases.
