---
doc_type: execution_report
spec_id: spec_codex_08_convergence_decisions
date: 2026-07-03
---

# Execution Report — spec_codex_08_convergence_decisions

## Acceptance criteria extracted

From `.specs/a_implementar/spec_codex_08_convergence_decisions.md` section 14 ("Critérios de
aceite"):

1. **14.1 — 6 ADR drafts criados**: 6 files exist in `docs/decisions/`, following the project's
   canonical ADR format, each with a "Requer decisão humana" section.
2. **14.2 — Nenhuma mudança de código**: `git status`/`git diff` confirms only files under
   `docs/decisions/**` were touched.
3. **14.3 — Docs validation**: `.\tools\docs\validate_docs.ps1` runs without introducing new
   errors.

The orchestrator additionally requested (within the spec's declared scope of "convergence
decisions"): 5 extra ADR drafts covering RNG domain separation, TimeScaleCoordinator tokens,
unified async scene flow, a single PPU/Y-sort contract (amending ADR-0011), and GameObject
pooling + Tilemap for the cave — all marked "Proposed" with an explicit human-decision section,
per the orchestrator's instructions.

## Existing systems audit

- `docs/decisions/` had **19 existing ADRs** (`ADR-0001`–`ADR-0019`) confirmed via
  `Get-ChildItem docs\decisions\ADR-*.md` before writing anything. Next free number confirmed:
  `ADR-0020`.
- `ADR-0008-unity-yaml-editing-policy.md` and `ADR-0011-pixel-art-scale-32px-per-tile.md` were
  read in full as format/content references before drafting; the new item (b) ADR (`ADR-0021`)
  explicitly extends ADR-0008 rather than duplicating it, per the spec's "Regras de não
  duplicação" (section 13); the new PPU ADR (`ADR-0029`) explicitly proposes amending ADR-0011
  rather than creating a conflicting standalone rule.
- The `runtime-bootstrap-pattern` skill was cited (not duplicated) by the item (c) ADR
  (`ADR-0022`), which reconciles with it rather than replacing the self-wiring idiom.
- `docs/project/DECISION_LOG.md` was read and updated (not recreated) to index the 11 new ADRs
  in the existing table format, consistent with how ADR-0010–ADR-0019 were already indexed.
- No code, `.unity`, `.prefab`, `.asset`, `Packages/`, or `ProjectSettings/` file was read for
  the purpose of editing — only read for **Phase 0 fact-gathering** (confirming counts/values
  cited in each ADR), which the spec's own section 9 and section 26 (risk mitigation) require
  ("cada ADR deve re-confirmar a contagem relevante... e citar a data da re-confirmação").

## Phase 0 — reconfirmed counts (2026-07-03, this session)

| Item | Prior audit figure | Reconfirmed this session | Method |
|---|---|---|---|
| `.asmdef` files | 0 | **0** | `Get-ChildItem -Recurse -Filter *.asmdef` under `Assets/` |
| `[RuntimeInitializeOnLoadMethod]` | 70 | **69 occurrences / 53 files** | `Select-String -Pattern "RuntimeInitializeOnLoadMethod"` |
| `CreateMvpTownScene.cs` line count | 3331 | **3331** (exact match) | `Get-Content ... \| Measure-Object -Line` |
| `TownScene` materialized GameObjects (~2826, ~1153 trees) | ~2826 / ~1153 | **NOT independently re-verified** (would require opening Unity/batchmode, out of scope for a doc-only session) — flagged explicitly in ADR-0023 as needing re-count by the implementer, alongside the unreconciled 497-tree figure from `spec_city_preservation_first_coherent_relayout` | N/A this session |
| `OnGUI` occurrences | 18 | **18** (exact match) | `Select-String -Pattern "void OnGUI" -List` |
| `Resources/` folder size | 54.6 MB | **54.62 MB** | `Get-ChildItem -Recurse -File \| Measure-Object -Property Length -Sum` |
| Addressables in `Packages/manifest.json` | not confirmed | **confirmed absent** | Direct read of `Packages/manifest.json` |
| `UnityEngine.Random` direct usage | 14 files (prompt) | **13 files** | `Select-String -Pattern "UnityEngine\.Random" -List` |
| `Time.timeScale =` direct writers | not enumerated | **2 files confirmed** (`CombatHitStopController.cs`, `DeathScreenCanvasController.cs`) | Direct file read |
| `LoadSceneAsync` occurrences | not enumerated | **0** (confirmed via grep across `Assets/_Game/Scripts/**`) | `Select-String -Pattern "SceneManager\.LoadScene\|EditorSceneManager\."` |
| PPU values in use | 32 (ADR-0011, contradicted) | **128 (default), 64 (modular houses), 234 (NPC body/walk), 100 (portraits)** | `GeneratedSpriteImporter.cs`, `CameraFollow2D.cs`, `CreateMvpTownScene.cs`, `AssignNpcBodySprites.cs`, `AssignNpcPortraitSprites.cs`, `GenerateNpcWalkAnimations.cs` |
| `sortingOrder =` manual writers | not enumerated | **25 files** | `Select-String -Pattern "sortingOrder\s*="` |
| `GameObject` pools | none known | **0 relevant to cave** (3 `*Pool` classes project-wide, none in `Cave/Runtime/**`) | `Select-String -Pattern "class\s+\w*Pool\b"` |
| `CaveTileMaterializer` per-tile `Instantiate` | assumed | **confirmed**, no pooling (lines 39, 83) | Direct file read |

## Spec Compliance Matrix

| Criterion | Status | Evidence |
|---|---|---|
| 14.1 — 6 ADR drafts created (spec scope) | PASS | `ADR-0020`–`ADR-0025` created in `docs/decisions/`, each with context/decision/alternatives/cost/risk/"Requer decisão humana" |
| Extra ADRs (a)-(e) requested by orchestrator, within spec's "convergence decisions" scope | PASS | `ADR-0026` (RNG), `ADR-0027` (TimeScaleCoordinator), `ADR-0028` (async scene flow), `ADR-0029` (PPU/Y-sort, amends ADR-0011), `ADR-0030` (cave pooling+Tilemap) |
| Every ADR marked "Proposed" status | PASS | All 11 new ADRs have `status: proposed` in front matter and "**proposed**" in the Status section |
| Every ADR has explicit "Requer decisão humana" section | PASS | Present in all 11 files, each with SIM/NÃO + motivo |
| ADR-0008 not duplicated (item b) | PASS | `ADR-0021` explicitly states relationship to ADR-0008 in its own "Relationship to ADR-0008" section; scopes itself to LFS/tracking only |
| `runtime-bootstrap-pattern` skill not duplicated (item c) | PASS | `ADR-0022` has a "Relationship to `runtime-bootstrap-pattern`" section citing, not replacing, the skill |
| ADR-0011 not silently contradicted, formally amended (extra item d) | PASS | `ADR-0029` explicit "amends ADR-0011" framing; ADR-0011 itself left untouched (still `accepted`) since amending it is itself gated behind human approval |
| 14.2 — No code change | PASS | `git status --short -- docs/decisions docs/project/DECISION_LOG.md` shows only the 11 new files + 1 modified index; no `Assets/**`, `Packages/**`, `ProjectSettings/**` touched by this task |
| 14.3 — Docs validation no new errors | PASS (with legacy pre-existing failures only) | See Validation section below |
| Next ADR number reconfirmed before writing (section 21, task T001) | PASS | Confirmed `ADR-0020` free via `Get-ChildItem` before any Write |
| DECISION_LOG.md updated (spec section did not explicitly require this, but orchestrator instruction did) | PASS | Table extended with all 11 new rows; header range updated `ADR-0001 to ADR-0030` |

## Validation

```text
Validation method: tools\docs\run_strict_validation.ps1
Exit code: 1
```

Full output captured to a scratch file and diffed for any error mentioning `docs/decisions/`,
the new `ADR-002x`/`ADR-003x` files, or `DECISION_LOG.md`. Result: **zero** new errors reference
any file created or modified by this task. The single positive match for `DECISION_LOG` is the
pre-existing informational line `OK: docs/project/DECISION_LOG.md exists.`

Exit code 1 is caused entirely by **pre-existing, already-known** failures, explicitly flagged as
expected by the orchestrator's instructions before this task started:

- `ERROR: Future spec missing marker` / `missing dependency header` — `spec_npc_physics_cat_companion.md` (5 errors, pre-existing, unrelated spec).
- `ERROR: Placeholder found` — `tools/codex/Generate-CodexHarness.ps1` (multiple lines, pre-existing generator scaffolding, unrelated to this task).
- Execution-report section-completeness warnings/errors across dozens of historical
  `docs/validation/*_execution_report.md` files (missing "Acceptance criteria extracted" /
  "Existing systems audit" / "Spec Compliance Matrix" / "Validation" / "Honest status rationale")
  — all pre-existing legacy reports, none touched by this task.
- `FAIL: Forbidden files altered` in the quality-check step — lists `Assets/_Game/Data/Enemies/Roster/*.asset`
  and other pre-session-modified files (confirmed present in `git status` **before** this task
  began, per the conversation's initial git status snapshot) — not created or touched by this
  task.

```text
Docs validation (validate_docs.ps1 portion): OK on all structural checks relevant to docs/decisions/ and DECISION_LOG.md
Overall run_strict_validation.ps1: EXPECTED_FAIL_LEGACY_ONLY (exit code 1; all failures pre-date and are unrelated to this task's file changes)
```

Assembly-CSharp / Assembly-CSharp-Editor dotnet builds: **NOT RUN** — this is a DOC-ONLY spec
(section 30, Testing Quality Gate: "Changed deterministic logic: NO... Human validation timing:
NOT REQUIRED"). No `.cs` file was touched, so a dotnet build adds no evidence value for this task
and was correctly skipped per the spec's own Testing Quality Gate declaration.

## Testing Quality Gate

```text
Changed deterministic logic: NO
Requires EditMode tests: NO
Requires PlayMode automated or final human scenario: NO
Requires regression test: NO
Human validation timing: NOT REQUIRED
Minimum validation evidence for ACCEPTED: docs validation PASS on all criteria touching this
  task's files; no file outside docs/decisions/** and docs/project/DECISION_LOG.md altered; all
  11 ADRs contain a filled "Requer decisão humana" section.
```

All minimum evidence requirements are met.

## Honest status rationale

**Status: BUILD_VALIDATED** (doc-only spec; "build" here means the docs-validation gate, since
no code was touched).

- All 6 spec-scoped ADRs (`ADR-0020`–`ADR-0025`) exist with the required structure.
- All 5 orchestrator-requested extra ADRs (`ADR-0026`–`ADR-0030`) exist, within the spec's
  declared scope ("Produzir 6 drafts de ADR... para débitos estruturais pesados" generalizes
  naturally to the additional convergence-audit findings named by the orchestrator — none of the
  5 extras required touching any file outside `docs/decisions/**`, so they fit inside the spec's
  `docs/decisions/**` repo-lock scope without expanding it).
- No ADR is marked accepted/decided — all 11 remain `status: proposed`, per the spec's explicit
  "Fora de escopo" instruction ("Marcar qualquer ADR como 'aceito'/'decidido'").
- `git status` confirms zero files outside `docs/decisions/**` and `docs/project/DECISION_LOG.md`
  were touched by this task.
- `run_strict_validation.ps1` exit code is 1, but every single failing check was independently
  verified (by grepping the full captured output) to reference files this task did not create or
  modify — all are pre-existing legacy failures the orchestrator explicitly named in advance as
  expected. No promotion of this spec to `implementados/` was performed (a check of this kind is
  the responsibility of `/finish-spec`, not this execution).
- No commit was made — per the orchestrator's explicit instruction, the orchestrator commits
  after independent verification.

## Not done

- The eventual "next number" for a 12th ADR is `ADR-0031` (not needed by this spec; noted for the
  next agent that creates an ADR).
- `ADR-0011`'s own file was **not modified** — `ADR-0029` proposes the amendment but does not
  apply it, since amending an already-`accepted` ADR is exactly the kind of decision this spec's
  human-approval gate exists for.
- No `.csproj`/dotnet build was run (correctly out of scope; see Testing Quality Gate above).
- No re-count of the ~1153 TownScene tree GameObjects was performed (would require Unity
  batchmode/Play Mode, out of scope for a doc-only spec); `ADR-0023` explicitly flags this as
  required before that ADR is acted upon.
- `/finish-spec` eligibility check for promoting this spec to `implementados/` was **not** run by
  this execution — per the agent's own operating rules, that is invoked separately.

---

*Execution completed: 2026-07-03*
