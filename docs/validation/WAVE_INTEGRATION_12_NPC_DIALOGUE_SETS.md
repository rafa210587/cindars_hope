# WAVE INTEGRATION 12 - NPC Dialogue Sets

Status: MVP_DIALOGUE_COVERAGE_PARTIAL_RUNTIME_FULL_AUTHORING_PLACEHOLDER

Rule: each MVP NPC has at least 10 entries/options documented. Pip and Wanderer have 10 runtime `DialogueTreeSO` nodes. Merchant NPCs currently use `NpcShopController` opening/closing lines plus shop menu; their 10-entry dialogue sets are authoring placeholders until merchant dialogue tree consumption is added.

## NPC: npc_pip_miudinho

| EntryId | Type | Text/Placeholder | Choice? | Condition | Next | Final? |
|---|---|---|---:|---|---|---:|
| pip_opening | Greeting | Welcome and topic hub | 1 | Always | role/shop/activity/close | 0 |
| pip_role | Role | Pip explains messenger/tutorial role | 1 | Always | pip_opening | 0 |
| pip_place | Place | Town/farm/cave orientation | 1 | Always | pip_opening | 0 |
| pip_shops | Shop tip | Points to seed/tool and weapon/armor shops | 1 | Always | pip_sell/pip_opening | 0 |
| pip_activities | Gameplay tip | Farm, fish, gather, sell, stamina | 1 | Always | pip_input_tip/pip_opening | 0 |
| pip_input_tip | Input/focus | Modal and input tip | 1 | Always | pip_opening | 0 |
| pip_rumor | Rumor | Non-spoiler memory/stone rumor | 1 | Always | pip_opening | 0 |
| pip_context | Context | Placeholder for weather/time | 1 | TEMPORARY_DIALOGUE_AUTHORING_PLACEHOLDER | pip_opening | 0 |
| pip_future_quest | Future quest | Placeholder for first delivery | 1 | TODO_NARRATIVE_FINALIZATION | pip_opening | 0 |
| pip_sell | Shop/sell | Explains sell tab/SellPoint | 1 | Always | pip_opening | 0 |

## NPC: npc_vaalara_wanderer_01

| EntryId | Type | Text/Placeholder | Choice? | Condition | Next | Final? |
|---|---|---|---:|---|---|---:|
| wanderer_opening | Greeting | Non-spoiler Vaalara road line | 1 | Always | role/place/rumor/close | 0 |
| wanderer_role | Role | Defines pilgrim/lore role | 1 | Always | wanderer_opening | 0 |
| wanderer_place | Place | Town as crossing/destination | 1 | Always | wanderer_opening | 0 |
| wanderer_gameplay_tip | Gameplay tip | Cave preparation tip | 1 | Always | wanderer_opening | 0 |
| wanderer_rumor | Rumor | Stones remember more than people | 1 | Always | wanderer_opening | 0 |
| wanderer_context | Context | Placeholder for weather/time/progress | 1 | TEMPORARY_DIALOGUE_AUTHORING_PLACEHOLDER | wanderer_opening | 0 |
| wanderer_shop_note | Shop/service | States no shop service | 1 | Always | wanderer_opening | 0 |
| wanderer_future_quest | Future quest | Placeholder for routes/ruins | 1 | TODO_NARRATIVE_FINALIZATION | wanderer_opening | 0 |
| wanderer_repeat | Fallback | Repeat/fallback line | 1 | Always | wanderer_opening | 0 |
| wanderer_goodbye | Goodbye | Closing line | 0 | Always | close | 1 |

## NPC: npc_shop_seeds_tools

| EntryId | Type | Text/Placeholder | Choice? | Condition | Next | Final? |
|---|---|---|---:|---|---|---:|
| seeds_greeting | Greeting | Welcome to seeds/tools shop | 0 | Runtime opening line | shop menu | 0 |
| seeds_role | Role | TEMPORARY_DIALOGUE_AUTHORING_PLACEHOLDER: merchant role | 0 | Deferred merchant tree | TBD | 0 |
| seeds_place | Place | TEMPORARY_DIALOGUE_AUTHORING_PLACEHOLDER: farming town note | 0 | Deferred merchant tree | TBD | 0 |
| seeds_gameplay_tip | Gameplay tip | TEMPORARY_DIALOGUE_AUTHORING_PLACEHOLDER: seeds/crops tip | 0 | Deferred merchant tree | TBD | 0 |
| seeds_rumor | Rumor | TODO_NARRATIVE_FINALIZATION | 0 | Deferred merchant tree | TBD | 0 |
| seeds_context | Context | Weather/time placeholder | 0 | Deferred merchant tree | TBD | 0 |
| seeds_shop | Shop/service | Shop menu opens buy/sell | 1 | Runtime shop menu | buy/sell/exit | 0 |
| seeds_future_quest | Future quest | TODO_NARRATIVE_FINALIZATION | 0 | Deferred merchant tree | TBD | 0 |
| seeds_repeat | Fallback | Repeat shop prompt | 0 | Deferred merchant tree | TBD | 0 |
| seeds_goodbye | Goodbye | Closing line | 0 | Runtime closing line | close | 1 |

## NPC: npc_shop_weapons_armor

| EntryId | Type | Text/Placeholder | Choice? | Condition | Next | Final? |
|---|---|---|---:|---|---|---:|
| weapons_greeting | Greeting | Welcome to weapons/armor shop | 0 | Runtime opening line | shop menu | 0 |
| weapons_role | Role | TEMPORARY_DIALOGUE_AUTHORING_PLACEHOLDER: equipment merchant role | 0 | Deferred merchant tree | TBD | 0 |
| weapons_place | Place | TEMPORARY_DIALOGUE_AUTHORING_PLACEHOLDER: town safety note | 0 | Deferred merchant tree | TBD | 0 |
| weapons_gameplay_tip | Gameplay tip | TEMPORARY_DIALOGUE_AUTHORING_PLACEHOLDER: repair/protection tip | 0 | Deferred merchant tree | TBD | 0 |
| weapons_rumor | Rumor | TODO_NARRATIVE_FINALIZATION | 0 | Deferred merchant tree | TBD | 0 |
| weapons_context | Context | Weather/time placeholder | 0 | Deferred merchant tree | TBD | 0 |
| weapons_shop | Shop/service | Shop menu opens buy/sell | 1 | Runtime shop menu | buy/sell/exit | 0 |
| weapons_future_quest | Future quest | TODO_NARRATIVE_FINALIZATION | 0 | Deferred merchant tree | TBD | 0 |
| weapons_repeat | Fallback | Repeat shop prompt | 0 | Deferred merchant tree | TBD | 0 |
| weapons_goodbye | Goodbye | Closing line | 0 | Runtime closing line | close | 1 |
