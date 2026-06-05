# Cindar's Hope — Spec Source Map UI Menu Flows Addendum

> **Status:** addendum temporário de source map para menu screen flows  
> **Local:** `docs/design/SPEC_SOURCE_MAP_UI_MENU_FLOWS_ADDENDUM.md`  
> **Fonte principal relacionada:** `docs/design/SPEC_SOURCE_MAP.md`  
> **Fonte nova:** `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`  
> **Função:** registrar imediatamente a rastreabilidade do direction detalhado de menus e submenus enquanto o `SPEC_SOURCE_MAP.md` principal não for alterado via patch parcial seguro.  
> **Não é spec implementável.**

---

## 0. Regra de precedência

Este addendum deve ser tratado como extensão do `SPEC_SOURCE_MAP.md` até que o bloco equivalente seja aplicado diretamente no arquivo principal.

Toda spec que envolva menus, submenus, drawers, screen flows, inventory screen, storage/chest screen, equipment screen, weapon/armor detail, item detail, tooltip expandido, repair/upgrade UI, skill tree screen, skill node detail, active slot assignment, spell detail, shop buy/sell screen, crafting screen, quest detail, social/NPC detail future, calendar day detail, Fonte menu, confirmation modal, empty state, error state, focus order ou menu navigation deve ler obrigatoriamente:

```text
docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
```

---

## 1. Specs de inventário, storage e item detail

Specs que envolvam Inventory, ItemStack, ItemInstance, item grid, item detail drawer, storage/chest, split stack, move item, drop, sell context, favorited/locked future, quest/key protection, tooltip expandido ou item actions devem ler:

```text
docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
```

---

## 2. Specs de equipamentos, arma/armadura, repair e upgrade

Specs que envolvam Equipment UI, comparison drawer, weapon detail submenu, armor detail submenu, material/tier/quality/rarity presentation, durability UI, repair UI, upgrade UI, known enemy interactions, equip/unequip ou item comparison devem ler:

```text
docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md
docs/design/gameplay/equipment/EQUIPMENT_ENEMY_VULNERABILITY_ADAPTER.md
```

Regra:

```text
Equipment docs vencem para significado mecânico.
UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md vence para screen flow, comparison drawer, detail drawer, focus order, confirmations e empty/error states.
Bestiary/Knowledge Discovery futuro vence para o que já é conhecido pelo jogador.
```

---

## 3. Specs de skill tree, active slots e respec

Specs que envolvam Skill Tree UI, node detail drawer, node states, spending SkillPoint, preview de rank, capstone detail, active slot assignment, active skill equip/substitution ou respec UI devem ler:

```text
docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md
docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
```

Regra:

```text
Skill tree docs vencem para árvore, nodes, ranks, costs e unlocks.
UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md vence para node detail, preview, confirmation, active slot assignment e focus order.
Fonte/quest docs vencem para quando respec fica disponível.
```

---

## 4. Specs de magia e spell detail

Specs que envolvam Spell detail, spell source states, LearnableScroll, CastScroll, Wand/Staff/Focus provided spell, MP cost, Stamina cost, cast time, cooldown, spell shape/range, target errors ou equipped spell slots devem ler:

```text
docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md
```

---

## 5. Specs de shop buy/sell

Specs que envolvam Shop Buy/Sell, shop stock, player inventory sell view, empty states, price display, stock quantity, limited/unique stock, bulk buy/sell, item protection in sell UI ou shop category filters devem ler:

```text
docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
```

Anti-regressão:

```text
Buy e Sell nunca usam a mesma lista de dados.
Buy mostra estoque da loja.
Sell mostra inventário vendável do jogador.
Empty state precisa ser explícito.
```

---

## 6. Specs de crafting e processing

Specs que envolvam Crafting screen, recipe states, recipe detail drawer, material faltante, craft one/many, station requirement, processing timer ou ready to collect UI devem ler:

```text
docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
```

---

## 7. Specs de quest log, calendar, social future e Fonte menu

Specs que envolvam Quest Log detail, quest condition states, temporal/lunar conditions, Calendar Day Detail, Social/NPC detail future, Fonte Menu stages, Fonte functions, decision final modal ou spoiler control devem ler:

```text
docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
```

---

## 8. Specs futuras recomendadas

```text
spec_ui_menu_focus_stack_runtime.md
spec_ui_inventory_screen_flow_runtime.md
spec_ui_storage_chest_screen_flow_runtime.md
spec_ui_equipment_screen_compare_runtime.md
spec_ui_weapon_armor_detail_drawer_runtime.md
spec_ui_repair_upgrade_screen_flow_runtime.md
spec_ui_skill_tree_node_detail_runtime.md
spec_ui_active_slot_assignment_runtime.md
spec_ui_spell_magic_detail_runtime.md
spec_ui_shop_buy_sell_screen_runtime.md
spec_ui_crafting_recipe_screen_runtime.md
spec_ui_quest_log_detail_runtime.md
spec_ui_calendar_day_detail_runtime.md
spec_ui_fonte_menu_flow_runtime.md
spec_ui_empty_error_confirmation_patterns_runtime.md
spec_ui_menu_gamepad_navigation_future.md
```

---

## 9. Anti-regressão

```text
UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md é fonte canônica de menus, submenus, drawers, screen flows, detail panels, comparison drawers, focus order, confirmation patterns, empty states e error states.
UI_UX_FULL_GAMEPLAY_DIRECTION.md continua vencendo para princípios gerais de UI/UX, input routing, modal focus, HUD e feedback.
Todo menu modal bloqueia WASD e input de gameplay.
Diálogo não deixa o personagem andar.
Shop Sell sempre mostra inventário vendável do jogador ou empty state explícito.
Buy e Sell nunca usam a mesma lista de dados.
Skill node sempre tem detail drawer antes de gastar ponto.
Gastar SkillPoint exige confirmação visual clara.
Active skill comprada não equipa automaticamente em slot cheio.
Equipment compare nunca equipa por hover ou foco.
Repair e Upgrade são ações diferentes.
Quest/Key item não pode ser vendido/descartado por padrão.
Tooltip curto não substitui detail drawer para informação complexa.
Fonte Menu não mostra função ainda não desbloqueada por fragmento.
Calendar não revela segredo antes da descoberta.
Social future não transforma NPC em planilha.
HUD final não mostra debug.
```

---

## 10. Bloco a aplicar no SPEC_SOURCE_MAP.md principal

Quando houver patch parcial seguro, aplicar no `docs/design/SPEC_SOURCE_MAP.md` principal, preferencialmente na seção de UI/HUD ou como subseção logo após ela:

```md
## Specs de menu screen flows e submenus derivados

Fontes obrigatórias:

```text
docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
```

Specs que envolvam menus, submenus, drawers, screen flows, inventory screen, storage/chest screen, equipment screen, weapon/armor detail, item detail, tooltip expandido, repair/upgrade UI, skill tree screen, skill node detail, active slot assignment, spell detail, shop buy/sell screen, crafting screen, quest detail, social/NPC detail future, calendar day detail, Fonte menu, confirmation modal, empty state, error state, focus order ou menu navigation devem ler obrigatoriamente essas fontes.
```
