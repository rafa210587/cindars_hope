# Refinamentos implementados - mapa

| Fase/refinamento | Specs normalizadas |
|---|---|
| Fase 8 MVP Farm | `spec_farm_001`, `spec_farm_002`, `spec_farm_003`, `spec_inventory_001`, `spec_save_001` |
| Fase 9A Town/Crafting/Save | `spec_town_001`, `spec_craft_001`, `spec_economy_001`, `spec_save_001` |
| Fase 9B Cave/Combat MVP | `spec_cave_001`, `spec_combat_001`, `spec_damage_001` |
| Fase 9C Tools/Farm/Combat refinement | `spec_tools_001`, `spec_farm_003`, `spec_combat_001` |
| Fase 9E UI/Hotbar/Damage/Progression | `spec_ui_001`, `spec_tools_001`, `spec_damage_001`, `spec_progression_001` |
| Fase 9F Cave procedural/resources | `spec_cave_002`, `spec_cave_003`, `spec_cave_004` |
| Fase 9G Bestiary/Faction Locks/Ecology | dependencia futura sobre `spec_cave_004` e specs futuras |

## Fonte auxiliar de merge logico

A pasta local `spec/implementado/` foi comparada por tema com as novas specs normalizadas. Quando havia sobreposicao clara, o conteudo foi consolidado nas specs `spec_*.md` em vez de preservar arquivos duplicados com nomes antigos.
