---
name: save-section-provider
description: Extract a save domain from the monolithic SaveManager into an ISaveSectionProvider, following the HotbarSectionProvider precedent. Use for fable_13 save debt closure or any spec adding/refactoring save sections.
---

# Skill: Save Section Provider

**Project state:** `ISaveSectionProvider` exists (`Assets/_Game/Scripts/Save/ISaveSectionProvider.cs`, SPEC_10) with exactly ONE implementation (`Providers/HotbarSectionProvider.cs`). The rest of save capture/restore is monolithic inside `SaveManager`. `SaveProviderArchitectureRoadmap.cs` documents the intended migration. fable_13 (save debt closure) is in the queue.

## The contract (follow exactly)

```csharp
public interface ISaveSectionProvider
{
    string ProviderId { get; }                    // "hotbar", "bestiary", ... lowercase stable id
    object Capture(GameSaveData existingSaveData); // null = skip section; use existing data as fallback
    void Restore(object sectionData);              // MUST null-guard (provider may have been skipped)
}
```

## Extraction recipe (one domain per step)

1. Locate the domain's capture/restore code inside `SaveManager` and its DTO field on `GameSaveData`.
2. Create `Assets/_Game/Scripts/Save/Providers/<Domain>SectionProvider.cs`:
   - constructor-inject the runtime state holder (like `HotbarSectionProvider(HotbarState)`) — never locate it via scene search (rule: unity-architecture);
   - `Capture`: if the state holder is null, fall back to `existingSaveData?.<Section> ?? new <Section>SaveData()` (preserves data when the system isn't loaded);
   - `Restore`: null-guard both the holder and `sectionData`; `as`-cast the DTO, ignore on mismatch.
3. Register the provider where SaveManager builds its provider list, **preserving the documented restore order** (registries/IDs before consumers — see save restore order contract spec in `executadas_build_validated/`).
4. Delete the now-dead inline code from SaveManager in the same change (no dual path).
5. DTO rules: simple types + stable IDs only (rule: unity-architecture §3).

## Mandatory tests (skill: editmode-test-authoring)

- Capture with live state → DTO matches state.
- Capture with null holder → falls back to existing save data.
- Restore with null section → no-op, no throw.
- Restore with wrong DTO type → no-op, no throw.
- Round-trip capture→restore → state equal.
- Legacy save without the section → defaults applied.

## Closeout

Execution report documents: section ownership (which provider owns which `GameSaveData` field), restore order position, and backward compatibility with pre-provider saves.
