# SPEC FUTURA — UI MENU SYSTEMS FINAL

> Origem histórica: conteúdo absorvido durante reorganização documental.
> Status: A implementar
> Tipo: Spec preparada / futura

---

# SPEC: UI Menu Systems Final (PENDING)

**Status**: Pendente (Debug HUD MVP implemented, full menus pending)  
**Date**: 2026-05  
**Version**: 1.0  
**Estimated Scope**: PR-###

---

## Summary

Main menu, pause menu, settings, and in-game UI screens replacing debug OnGUI implementation.

## Pending Work

- [ ] Main menu (start/load/settings/quit)
- [ ] Pause menu (resume/settings/save/quit)
- [ ] Settings UI (audio, graphics, controls)
- [ ] Inventory screen (full implementation)
- [ ] Character sheet (stats/equipment/skills)
- [ ] Quest log (if quests added)
- [ ] Map screen (cave layout visualization)
- [ ] Save/load dialog
- [ ] Confirmation dialogs
- [ ] Loading screen

## Key Files to Create

- `Assets/_Game/Scripts/UI/MainMenuUI.cs` (new)
- `Assets/_Game/Scripts/UI/PauseMenuUI.cs` (new)
- `Assets/_Game/Scripts/UI/SettingsUI.cs` (new)
- `Assets/_Game/Scripts/UI/CharacterSheetUI.cs` (new)
- Canvas and UI component hierarchy (new)

## Acceptance Criteria

- [ ] Main menu functional
- [ ] Can start new game from menu
- [ ] Can load save from menu
- [ ] Pause menu works in-game
- [ ] Settings changeable
- [ ] Inventory screen polished
- [ ] Character sheet readable
- [ ] No OnGUI debug panels in final build

## Dependencies

- Core UI framework (depends on core)
- Save/load system (implemented)
- Settings persistence (to be implemented)

## Next Steps

Implement after feature implementation wave (FASE9F complete).



