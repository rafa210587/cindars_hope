# Bounded visual iteration

Apply when iterating a scene or asset toward approved reference art.

- Start with the actual latest capture and reference at a declared framing/scale. Show the
  current result before another substantial revision; label concepts, integrated captures,
  failed probes and intentional gameplay differences separately. Do not invent likeness percentages.
- Rank at most three visible discrepancies. Select one cohesive correction round and explicit
  observable acceptance criteria. Keep unrelated gameplay features in the existing backlog.
- Reuse approved art for composition changes. Create an Aseprite candidate only when pixels
  actually need editing; do not require a new art pipeline or mockup for position-only changes.
- Default to one executor and one independent reviewer at the integrated checkpoint. Delegate
  additional work only when it is independent and saves more coordination than it creates.
- Integrate the coherent candidate once, then run the applicable gates from the validation
  matrix. Visual-only edits need current images/import checks; geometry needs affected access
  and collision checks; behavior needs its focused tests. Reuse evidence over equivalent inputs.
- In a shared checkout, coordinate a source freeze with active affected writers before Unity
  compilation, not just exclusive Editor ownership. Report unrelated compile failures to their
  owner and wait for a changed, ready input state rather than retrying the same blocked build.
- A probe failure must be classified as product, instrumentation or environment before fixing.
  Never change production behavior solely to make a nonrepresentative demonstration look correct.
- After two attempts without observable improvement, stop that approach, preserve evidence,
  show the result and revise the method. This is not task cancellation or permission escalation.
- Record the baseline, findings, changed files, attempts, results and next decision in the
  existing report. Record elapsed time and usage only when measured; no new logging system.
- Keep a latest-result HTML or equivalent delivery current. A green test suite does not prove
  visual acceptance. Close the round with before/after and explicit residual differences.

Example: moving existing decorative flowers does not require new generated textures or a full
gameplay suite. Moving a solid shoreline requires matching collision changes and local checks.
