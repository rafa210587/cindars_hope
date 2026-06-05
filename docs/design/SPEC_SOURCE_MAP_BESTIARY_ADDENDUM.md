# Cindar's Hope — Spec Source Map Bestiary Addendum

> **Status:** addendum temporário de source map para Bestiary & Knowledge Discovery  
> **Local:** `docs/design/SPEC_SOURCE_MAP_BESTIARY_ADDENDUM.md`  
> **Fonte principal relacionada:** `docs/design/SPEC_SOURCE_MAP.md`  
> **Fonte nova:** `docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md`  
> **Função:** registrar imediatamente a rastreabilidade do direction de bestiário e descoberta de conhecimento enquanto o `SPEC_SOURCE_MAP.md` principal não for alterado via patch parcial seguro.  
> **Não é spec implementável.**

---

## 0. Regra de precedência

Este addendum deve ser tratado como extensão do `SPEC_SOURCE_MAP.md` até que o bloco equivalente seja aplicado diretamente no arquivo principal.

Toda spec que envolva bestiário, conhecimento de inimigos, descoberta de vulnerabilidades, identificação de criaturas, knowledge states, enemy knowledge save/load, drops conhecidos, resistências conhecidas, imunidades conhecidas, behavior windows descobertas, lore notes de criaturas, NPC/livro/quest desbloqueando conhecimento, equipment tooltip com known enemy interactions, spell tooltip com known effectiveness, Bestiary HUD futura, Bestiary Menu futuro ou spoiler control de inimigos deve ler obrigatoriamente:

```text
docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
```

---

## 1. Specs de inimigos/caverna

Specs que envolvam EnemyBestiaryEntrySO, EnemyKnowledgeState, EnemyDataSO com BestiaryEntryId, enemy families, cave roster conversion, boss entries, creature variants, faction knowledge, behavior observed, attacks observed, boss phase observed ou spoiler tier devem ler também:

```text
docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
docs/design/gameplay/enemies/ENEMY_BEHAVIORS_DIRECTION.md
docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
```

---

## 2. Specs de combate/vulnerabilidades

Specs que envolvam descoberta de ElementVulnerability, StatusVulnerability, AttackTypeVulnerability, WeaponVulnerability, MaterialVulnerability, BehavioralVulnerabilityWindow, ResistanceTags, ImmunityTags ou feedback de efetividade devem ler:

```text
docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
docs/design/gameplay/equipment/EQUIPMENT_ENEMY_VULNERABILITY_ADAPTER.md
docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md
```

Regra:

```text
Bestiary não cria vulnerabilidade; apenas revela conhecimento sobre vulnerabilidades já autoradas.
```

---

## 3. Specs de equipment/spell tooltips

Specs que envolvam known enemy interactions em Equipment UI, Weapon Detail, Armor Detail, Material interaction display, Spell known effectiveness, tooltip expandido, comparison drawer ou ocultação de vulnerabilidades desconhecidas devem ler:

```text
docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
docs/design/gameplay/equipment/EQUIPMENT_ENEMY_VULNERABILITY_ADAPTER.md
docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
```

Regra:

```text
Equipment UI e Spell UI só mostram interações conhecidas ou autorizadas por fonte legítima de conhecimento.
```

---

## 4. Specs de loot/crafting/economy

Specs que envolvam drops conhecidos, rare drops ocultos, origem conhecida de material, fonte de crafting item, boss first-time/repeat rewards revelados ou quest/encomenda que revela drop source devem ler:

```text
docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
```

---

## 5. Specs de quests/main quest/lore

Specs que envolvam quest de pesquisa, identificar criatura, coletar amostra, descobrir fraqueza, confirmar rumor, documentar comportamento, pesquisar Pedra Negra, entender criatura corrompida, Arquivista do Silêncio, nível 100/101, boss spoiler control ou conhecimento concedido por NPC/livro/ruína devem ler:

```text
docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
```

---

## 6. Specs de UI/HUD futura

Specs que envolvam Bestiary Menu, Bestiary HUD, Knowledge Log, Bestiary notifications, Bestiary entry detail, filters, KnownEnemyInteractions, Bestiary spoiler control, Knowledge confidence ou enemy entry cards devem ler:

```text
docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
```

Regra:

```text
Bestiary UI/HUD completa é futura e não entra na primeira entrega executável atual.
```

---

## 7. Specs de pets/companions/NPC knowledge

Specs que envolvam pet hints, companion knowledge hints, NPC teaching, books, research service, rumor confidence, partial/confirmed knowledge ou social unlocks de conhecimento devem ler:

```text
docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
docs/design/gameplay/pets/PETS_DIRECTION.md
docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md
```

---

## 8. Specs futuras recomendadas

```text
spec_bestiary_entry_data_contract_future.md
spec_enemy_knowledge_state_save_load_future.md
spec_bestiary_discovery_events_runtime_future.md
spec_bestiary_ui_menu_future.md
spec_bestiary_hud_notifications_future.md
spec_equipment_tooltip_known_interactions_future.md
spec_spell_tooltip_known_effectiveness_future.md
spec_bestiary_npc_books_quest_unlocks_future.md
spec_bestiary_boss_spoiler_control_future.md
spec_bestiary_pet_companion_hints_future.md
spec_bestiary_research_service_future.md
```

---

## 9. Anti-regressão

```text
BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md é fonte canônica de bestiário, descoberta de conhecimento, estados de conhecimento, revelação de vulnerabilidades, drops conhecidos, spoiler control e HUD/UI futura de bestiário.
Bestiary não entra na primeira entrega executável atual.
Bestiary é sistema de descoberta, não lista completa revelada de início.
Bestiary não cria vulnerabilidade; apenas revela conhecimento sobre vulnerabilidades já autoradas.
Equipment UI não revela vulnerabilidade desconhecida.
Spell UI não revela resistência/imunidade desconhecida.
Drop raro não descoberto não aparece com nome completo.
Derrotar uma criatura uma vez não precisa revelar tudo.
Ver uma criatura não revela fraquezas automaticamente.
NPC/livro/quest pode revelar rumor, partial ou confirmed knowledge.
Pet/companion pode dar hint, mas não completa bestiário sozinho.
Bosses e main quest têm spoiler control.
Arquivista do Silêncio não aparece completo antes da revelação apropriada.
Pedra Negra não revela natureza completa cedo.
Conhecimento descoberto deve ser persistido em save/load.
HUD de bestiário futura não deve virar planilha em combate.
Bestiary UI futura deve mostrar apenas conhecimento conhecido ou autorizado pelo SpoilerTier.
```

---

## 10. Bloco a aplicar no SPEC_SOURCE_MAP.md principal

Quando houver patch parcial seguro, aplicar no `docs/design/SPEC_SOURCE_MAP.md` principal, preferencialmente antes da anti-regressão ou como nova seção de Bestiary/Knowledge:

```md
# PARTE L — Bestiary / Knowledge Discovery

## Specs de bestiário e descoberta de conhecimento

Fontes obrigatórias:

```text
docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
```

Specs que envolvam bestiário, conhecimento de inimigos, descoberta de vulnerabilidades, identificação de criaturas, knowledge states, enemy knowledge save/load, drops conhecidos, resistências conhecidas, imunidades conhecidas, behavior windows descobertas, lore notes de criaturas, NPC/livro/quest desbloqueando conhecimento, equipment tooltip com known enemy interactions, spell tooltip com known effectiveness, Bestiary HUD futura, Bestiary Menu futuro ou spoiler control de inimigos devem ler obrigatoriamente esta fonte.
```
