# Optional Aseprite adapters

Read only when evaluating or using an MCP/live bridge instead of direct CLI/Lua.

- Identify backend, pinned version, transport, executable, path restrictions and whether it opens
  a file in batch mode or mutates the active GUI document. Do not assume all bridges are headless.
- Before a live mutation, confirm absolute document path, dirty state, frame and layer. One owner
  writes the candidate; preserve the artist's unsaved document. Do not attach to an ambiguous tab.
- Translate frame indexing and duration units at the boundary. Lua uses one-based frames and
  seconds; a tool may use zero-based indices or milliseconds. Inspect its schema and roundtrip.
- Record baseline user data/custom properties and hidden layers. Some adapters persist selection
  or clipboard inside the sprite; inspect deltas. Never strip user metadata or all hidden layers
  as generic cleanup. Remove only known adapter-owned data when authorized and verified.
- After a timeout, inspect the saved/current candidate before replaying a mutation. Confirm an
  effect is absent or idempotent; frame insertion repeated blindly can corrupt sequence contracts.
- Tools that execute Lua or export arbitrary paths retain those side effects even over a local
  transport. Review operation scope and source; avoid unbounded scripts or implicit source overwrite.

## Candidate pilot

Use a separate native candidate representative of the required structures: hidden layer,
offset cel, linked cels, unequal durations, tag and slice/pivot when present in the real asset.
Apply one bounded correction, reopen, compare metadata/alpha/linking and original source hash,
then export a strip and timed preview. Record evidence and limitations; passing an API call or
pixel-difference metric does not approve art or animation.

Backend candidates and source URLs are research, not execution requirements. Compare only when
the current task needs an adapter; the direct CLI path remains available.
