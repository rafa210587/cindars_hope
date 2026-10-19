---
name: localization-authoring
description: Add or migrate player-facing text through the existing localization keys and service. Use for UI, dialogue, quest and interaction text; preserve system IDs and visible missing-key fallback.
---

# Skill: Localization authoring

New player-facing text uses the existing localization table/service. Do not introduce another
framework or change quest, NPC, item or save IDs while adding presentation keys.

## Essential workflow
1. Identify every player-visible string in scope and its owning domain. Debug/editor logs and
   internal IDs are not localized.
2. Inspect current `LocalizationStringTable` and `LocalizationService` APIs before editing;
   do not assume historical methods or seed layout.
3. For new keys, lookup/fallback, placeholders and tests, read
   [runtime usage](references/runtime-usage.md).
4. Read [migration and closeout](references/migration-and-closeout.md) only for an explicitly
   scoped hardcode sweep or compatibility review. Pre-existing debt is not incidental scope.
5. New keys or changed lookup/formatting require relevant success, missing-key and parameter
   tests. For a typo in an existing value, preserve key/placeholders and inspect the diff;
   reuse unchanged lookup coverage. Inspect rendered glyphs/wrapping through `hud-canvas-binding`
   when the change can affect font coverage, expansion or layout, not for every punctuation fix.

Keys use stable domain-based names; dynamic values are formatted after resolving the key.
Deliver new/migrated keys, call sites, fallback behavior, tests and pending visual coverage.
External guidance does not authorize adding Unity Localization or Addressables.
