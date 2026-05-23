# refinamento_init_ui_ux_full_gameplay_inventory_hotbar_menus

> **Status:** Refinamento inicial a implementar  
> **Spec futura sugerida:** `spec_ui_ux_full_gameplay_inventory_hotbar_menus.md`  
> **Objetivo:** completar UI/UX de gameplay: HUD, hotbar, inventory, equipment, crafting, skills, shop, cave/death e menus.

---

## 1. Estado atual

O projeto possui HUD/debug e alguns managers/painéis mínimos, mas ainda não possui UI final integrada.

Evidências principais:

```text
Assets/_Game/Scripts/UI/DebugHud.cs
Assets/_Game/Scripts/UI/MenuManager.cs
Assets/_Game/Scripts/UI/MenuSystemDataSO.cs
docs/specs/implementados/spec_ui_001_debug_hud_feedback_mvp.md
docs/specs/implementados/spec_ui_002_hud_tools_hotbar_progression_debug.md
docs/specs/implementados/spec_ui_menu_systems_final.md
docs/specs/a_implementar/spec_fase9l_ui_ux_full_gameplay.md
```

---

## 2. Gaps

- Debug HUD não é UI final.
- Inventory não tem grid/slots/drag/drop.
- Hotbar não está completa visualmente.
- Equipment screen não está completa.
- Crafting UI não está completa.
- Skill tree UI não existe de forma final.
- Shop UI não está completa.
- Cave/death/corpse UI não está completa.
- Pause/options/save slot UI ainda precisa consolidação.
- Falta navegação consistente por teclado/controle/mouse.

---

## 3. Escopo esperado

### HUD gameplay

- HP.
- Hunger.
- Stamina/Mana quando implementadas.
- Gold.
- Day/time/moon/season.
- Equipped tool/weapon.
- Active skill slots.
- Context hint.

### Menus

```text
Inventory
Equipment
Crafting
Skills
Shop
Bestiary
Journal/Quests futuro
Pause
Save slots futuro
Cave entry
Death/corpse recovery
```

### UX rules

- Um menu modal por vez, salvo overlays explícitos.
- Player movement bloqueado quando menu modal abre.
- ESC fecha menu atual antes de abrir pause.
- Hotbar continua visível durante gameplay normal.
- Debug HUD pode existir, mas isolado por flag/dev mode.

### Data binding

UI deve reagir a eventos:

```text
InventoryChangedEvent
GoldChangedEvent
HungerChangedEvent
PlayerXpChangedEvent
PlayerLevelChangedEvent
ToolEquippedEvent
WeaponEquippedEvent
SkillSlotChangedEvent futuro
```

---

## 4. Arquivos prováveis

```text
Assets/_Game/Scripts/UI/MenuManager.cs
Assets/_Game/Scripts/UI/MenuSystemDataSO.cs
Assets/_Game/Scripts/UI/HUD/**
Assets/_Game/Scripts/UI/Inventory/**
Assets/_Game/Scripts/UI/Equipment/**
Assets/_Game/Scripts/UI/Crafting/**
Assets/_Game/Scripts/UI/Skills/**
Assets/_Game/Scripts/UI/Shop/**
Assets/_Game/Scripts/UI/Cave/**
Assets/_Game/Scripts/UI/Death/**
Assets/_Game/Scripts/Input/**
```

---

## 5. Fora de escopo

- Arte final de todos os ícones.
- Controller remapping avançado.
- Acessibilidade completa.
- Localização multilíngue completa.

---

## 6. Definition of Done

- [ ] HUD final substitui ou encapsula DebugHud.
- [ ] Inventory abre/fecha e mostra slots/itens reais.
- [ ] Hotbar mostra itens/actions equipados.
- [ ] Equipment UI mostra slots e stats.
- [ ] Crafting UI mostra recipes e ingredientes.
- [ ] Skill tree UI mostra pontos, nodes e active slots.
- [ ] Shop UI permite comprar/vender com feedback.
- [ ] Cave/death UI mostra entrada, morte e recovery.
- [ ] MenuManager impede conflitos de menus.
- [ ] UI atualiza via eventos, não polling pesado.

---

## 7. Validação

1. Abrir/fechar cada menu por input.
2. Validar que movimento do player bloqueia em menu modal.
3. Alterar inventory/gold/hunger/XP e observar HUD.
4. Comprar/vender em Shop UI.
5. Craftar via Crafting UI.
6. Equipar item e validar Equipment + Hotbar.
7. Comprar skill/equipar active slot e validar UI.
8. Entrar/morrer na cave e validar UI de death/recovery.
