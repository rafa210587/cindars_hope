# REF — CRAFTING PR046 052

> Origem histórica: conteúdo absorvido durante reorganização documental.
> Status: refinamento implementado absorvido.
> Spec consolidada relacionada: `docs/specs/implementados/spec_craft_001_crafting_mvp.md`

---

# SPEC: Crafting (PR-046 to PR-052)

**Status**: Implementado MVP  
**Date**: 2026-05  
**Version**: 1.0  
**Relevant PRs**: PR-046 to PR-052

---

## Summary

Crafting system: recipes, ingredients, output, crafting manager, and interactive crafting points in world.

## Scope

- ✅ RecipeSO data contracts
- ✅ CraftingManager (recipe lookup, ingredient validation)
- ✅ CraftingPoint (world interaction point)
- ✅ Recipe execution (consume ingredients, produce output)
- ✅ Inventory integration
- ⚠️ Queue/async crafting (not implemented)
- ⚠️ Multiple recipe UI (debug only)

## Architecture

### Recipe System
- **RecipeSO**: ID, display name, ingredients (item + qty), output (item + qty)
- **RecipeDatabase**: Registry of all recipes
- **Common Recipe**: "processed_wood" — wood → processed_wood

### Crafting Workflow
1. Player interacts with CraftingPoint (E key)
2. UI shows available recipes (debug OnGUI)
3. Player selects recipe
4. CraftingManager validates ingredients
5. Consumes ingredients, produces output
6. Updates inventory

### CraftingManager
- **Query**: Has recipe with ID?
- **Validate**: Player has all ingredients?
- **Execute**: Consume ingredients, add output
- **Events**: CraftingCompletedEvent published

## Key Files

- `Assets/_Game/Scripts/Crafting/RecipeSO.cs` — Recipe contracts
- `Assets/_Game/Scripts/Crafting/RecipeDatabase.cs` — Recipe registry
- `Assets/_Game/Scripts/Crafting/CraftingManager.cs` — Recipe execution
- `Assets/_Game/Scripts/Crafting/CraftingPoint.cs` — World interaction
- `Assets/_Game/Scripts/Core/Events/CraftingCompletedEvent.cs` — Event

## Acceptance Criteria

| # | Criterion | Status |
|---|-----------|--------|
| 1 | RecipeSO defines ingredients & output | ✅ |
| 2 | CraftingManager validates ingredients | ✅ |
| 3 | CraftingPoint opens recipe UI | ✅ |
| 4 | Crafting consumes ingredients | ✅ |
| 5 | Crafting produces output | ✅ |
| 6 | Inventory updated correctly | ✅ |
| 7 | CraftingCompletedEvent published | ✅ |
| 8 | Multiple recipes work | ✅ |

## Pending

- Async/queue crafting with progress bar
- Full recipe database (currently 1 example recipe)
- UI polish (not OnGUI)
- Recipe balancing

## Next Steps

Continue to Town (PR-053 to PR-063).



