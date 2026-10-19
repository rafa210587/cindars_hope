# /bugfix

Restore an existing contract with the smallest justified correction and relevant evidence.

**Arguments:** `$ARGUMENTS` — observed defect, reproduction or target text/file.

## Short journey

1. Read CLAUDE, CURRENT_STATE and directly affected files. Establish expected versus observed
   behavior; read an existing spec only if that contract is unclear. Preserve concurrent work.
2. Reproduce the defect or identify decisive evidence. A typo needs inspection, not a runtime
   reproduction. Inspect callers only as needed; do not load roadmap or unrelated history.
3. Correct the cause within scope. The owner may implement and test the fix; no mandatory
   subagent, new spec, standalone plan, template, report or commit.
4. Select the smallest sufficient existing check from the validation matrix. For behavior,
   use a focused regression or a concrete limitation/risk; do not rely on compilation alone.
   Test Runner may prove compile and behavior together. A reversible wording change needs
   diff/link review, not a new automated test or global docs/build suite.
5. Inspect the diff and report the fix, obtained evidence and residual risk. Reuse valid
   evidence on equivalent inputs; do not rerun gates for each reviewer.

## Escalation by risk

- Save/schema, stable IDs, atomic transactions, cross-system or lifecycle changes require
  contract analysis and the affected compatibility/integration checks, even for one line.
- If the request introduces behavior rather than restores it, use the approved feature spec
  or clarify the new requirement before implementing it. Do not hide scope expansion as a fix.
- A wider architecture change uses the existing spec or a bounded amendment/spec when needed.
  Escalate only the affected slice; continue independent authorized work.
- Missing reproduction is uncertainty, not permission to rewrite neighboring systems.

## Boundaries and output

Do not refactor unrelated code, move specs, update roadmap/backlog or alter permissions.
Update status documentation only when needed and authorized. No automatic commit/push.
Return a short cause/fix and `PASS / FAIL / NOT RUN` with command/artifact when applicable;
state remaining risk honestly. Use `/validate-spec` and `/finish-spec` only for an actual target
spec; they are not prerequisites for this short journey.
