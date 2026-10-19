# WAVE INTEGRATION 12 - NPC Dialogue Sets

Status: MVP_DIALOGUE_COVERAGE_RUNTIME_PLACEHOLDER_COMPLETE

Rule:
- Each MVP NPC has at least 10 `DialogueTreeSO` nodes.
- Text is `TEMPORARY_DIALOGUE_AUTHORING_PLACEHOLDER` / `TODO_NARRATIVE_FINALIZATION`, not final narrative.
- Merchant NPCs have dialogue trees for authoring coverage and use `NpcShopController` for the shop flow.

## NPC: npc_pip_miudinho

Runtime asset: `Assets/_Game/Data/Dialogues/DialogueTree_Pip.asset`

| EntryId | Type | Text/Placeholder | Choice? | Condition | Next | Final? |
|---|---|---|---:|---|---|---:|
| pip_opening | Greeting | Existing runtime opening hub | 1 | Always | role/shop/activity/close | 0 |
| pip_role | Role | Tutorial messenger role | 1 | Always | pip_opening | 0 |
| pip_place | Place | Town/farm/cave orientation | 1 | Always | pip_opening | 0 |
| pip_shops | Shop tip | Points to shops | 1 | Always | pip_sell/pip_opening | 0 |
| pip_activities | Gameplay tip | Farm/fish/gather/sell/stamina | 1 | Always | pip_input_tip/pip_opening | 0 |
| pip_input_tip | Input/focus | Modal/input tip | 1 | Always | pip_opening | 0 |
| pip_rumor | Rumor | Non-spoiler rumor | 1 | Always | pip_opening | 0 |
| pip_context | Context | Weather/time placeholder | 1 | TEMPORARY_DIALOGUE_AUTHORING_PLACEHOLDER | pip_opening | 0 |
| pip_future_quest | Future quest | First delivery placeholder | 1 | TODO_NARRATIVE_FINALIZATION | pip_opening | 0 |
| pip_sell | Shop/sell | Sell tab/SellPoint explanation | 1 | Always | pip_opening | 0 |

## NPC: npc_sylveth

Runtime asset: `Assets/_Game/Data/Dialogues/DialogueTree_Sylveth.asset`

| EntryId | Type | Text/Placeholder | Choice? | Condition | Next | Final? |
|---|---|---|---:|---|---|---:|
| sylveth_opening | Greeting | Seed/farm merchant hub | 1 | Always | role/tip/shop/close | 0 |
| sylveth_role | Role | Seed/farm merchant role | 1 | Always | sylveth_opening | 0 |
| sylveth_place | Place | Town shop/farm route note | 1 | Always | sylveth_opening | 0 |
| sylveth_gameplay_tip | Gameplay tip | Buy/plant/water/sell loop | 1 | Always | sylveth_opening | 0 |
| sylveth_rumor | Rumor | Seasonal value future hint | 1 | Always | sylveth_opening | 0 |
| sylveth_context | Context | Weather/calendar placeholder | 1 | TODO_NARRATIVE_FINALIZATION | sylveth_opening | 0 |
| sylveth_shop | Shop/service | shop_seeds_tools explanation | 1 | Runtime shop through NpcShopController | sylveth_opening | 0 |
| sylveth_future_quest | Future quest | Farm tutorial delivery hook | 1 | TODO_NARRATIVE_FINALIZATION | sylveth_opening | 0 |
| sylveth_repeat | Fallback | Return to shop menu note | 1 | Always | sylveth_opening | 0 |
| sylveth_goodbye | Goodbye | Close line | 0 | Always | close | 1 |

## NPC: npc_brumdar

Runtime asset: `Assets/_Game/Data/Dialogues/DialogueTree_Brumdar.asset`

| EntryId | Type | Text/Placeholder | Choice? | Condition | Next | Final? |
|---|---|---|---:|---|---|---:|
| brumdar_opening | Greeting | Blacksmith hub | 1 | Always | role/tip/shop/close | 0 |
| brumdar_role | Role | Blacksmith/repair role | 1 | Always | brumdar_opening | 0 |
| brumdar_place | Place | Forge placeholder | 1 | Always | brumdar_opening | 0 |
| brumdar_gameplay_tip | Gameplay tip | Cave/equipment prep | 1 | Always | brumdar_opening | 0 |
| brumdar_rumor | Rumor | Ore/cave future hint | 1 | Always | brumdar_opening | 0 |
| brumdar_context | Context | Forge hours placeholder | 1 | TODO_NARRATIVE_FINALIZATION | brumdar_opening | 0 |
| brumdar_shop | Shop/service | shop_blacksmith explanation | 1 | Runtime shop through NpcShopController | brumdar_opening | 0 |
| brumdar_future_quest | Future quest | Tool upgrade/ore hook | 1 | TODO_NARRATIVE_FINALIZATION | brumdar_opening | 0 |
| brumdar_repeat | Fallback | Check stock/gold/inventory | 1 | Always | brumdar_opening | 0 |
| brumdar_goodbye | Goodbye | Close line | 0 | Always | close | 1 |

## NPC: npc_renko

Runtime asset: `Assets/_Game/Data/Dialogues/DialogueTree_Renko.asset`

| EntryId | Type | Text/Placeholder | Choice? | Condition | Next | Final? |
|---|---|---|---:|---|---|---:|
| renko_opening | Greeting | General merchant hub | 1 | Always | role/tip/shop/close | 0 |
| renko_role | Role | General merchant role | 1 | Always | renko_opening | 0 |
| renko_place | Place | Central plaza shop note | 1 | Always | renko_opening | 0 |
| renko_gameplay_tip | Gameplay tip | Food/common item prep | 1 | Always | renko_opening | 0 |
| renko_rumor | Rumor | Future route rumor | 1 | Always | renko_opening | 0 |
| renko_context | Context | Reputation discount placeholder | 1 | TODO_NARRATIVE_FINALIZATION | renko_opening | 0 |
| renko_shop | Shop/service | shop_general_store explanation | 1 | Runtime shop through NpcShopController | renko_opening | 0 |
| renko_future_quest | Future quest | Delivery/bargain hook | 1 | TODO_NARRATIVE_FINALIZATION | renko_opening | 0 |
| renko_repeat | Fallback | Stock/gold fallback | 1 | Always | renko_opening | 0 |
| renko_goodbye | Goodbye | Close line | 0 | Always | close | 1 |

## NPC: npc_thalindra

Runtime asset: `Assets/_Game/Data/Dialogues/DialogueTree_Thalindra.asset`

| EntryId | Type | Text/Placeholder | Choice? | Condition | Next | Final? |
|---|---|---|---:|---|---|---:|
| thalindra_opening | Greeting | Library/quest hook hub | 1 | Always | role/place/close | 0 |
| thalindra_role | Role | Library/quest NPC role | 1 | Always | thalindra_opening | 0 |
| thalindra_place | Place | Archive location note | 1 | Always | thalindra_opening | 0 |
| thalindra_gameplay_tip | Gameplay tip | Read markers/reports | 1 | Always | thalindra_opening | 0 |
| thalindra_rumor | Rumor | Non-spoiler archive rumor | 1 | Always | thalindra_opening | 0 |
| thalindra_context | Context | Progress/calendar placeholder | 1 | TODO_NARRATIVE_FINALIZATION | thalindra_opening | 0 |
| thalindra_shop_note | Shop/service | No shop now | 1 | Always | thalindra_opening | 0 |
| thalindra_future_quest | Future quest | Archive/Anya hook | 1 | TODO_NARRATIVE_FINALIZATION | thalindra_opening | 0 |
| thalindra_repeat | Fallback | Technical authoring placeholder | 1 | Always | thalindra_opening | 0 |
| thalindra_goodbye | Goodbye | Close line | 0 | Always | close | 1 |

## NPC: npc_zrix

Runtime asset: `Assets/_Game/Data/Dialogues/DialogueTree_Zrix.asset`

| EntryId | Type | Text/Placeholder | Choice? | Condition | Next | Final? |
|---|---|---|---:|---|---|---:|
| zrix_opening | Greeting | Cave rumor/supplies hub | 1 | Always | role/tip/shop/close | 0 |
| zrix_role | Role | Cave/adventurer rumor role | 1 | Always | zrix_opening | 0 |
| zrix_place | Place | Road/cave route note | 1 | Always | zrix_opening | 0 |
| zrix_gameplay_tip | Gameplay tip | Bring food/potion/repair | 1 | Always | zrix_opening | 0 |
| zrix_rumor | Rumor | Cave entrance pending WAVE13 | 1 | Always | zrix_opening | 0 |
| zrix_context | Context | Risk/time route placeholder | 1 | TODO_NARRATIVE_FINALIZATION | zrix_opening | 0 |
| zrix_shop | Shop/service | shop_cave_supplies explanation | 1 | Runtime shop through NpcShopController | zrix_opening | 0 |
| zrix_future_quest | Future quest | Map/checkpoint hook | 1 | TODO_NARRATIVE_FINALIZATION | zrix_opening | 0 |
| zrix_repeat | Fallback | Does not start cave procedural | 1 | Always | zrix_opening | 0 |
| zrix_goodbye | Goodbye | Close line | 0 | Always | close | 1 |

## NPC: npc_nimble

Runtime asset: `Assets/_Game/Data/Dialogues/DialogueTree_Nimble.asset`

| EntryId | Type | Text/Placeholder | Choice? | Condition | Next | Final? |
|---|---|---|---:|---|---|---:|
| nimble_opening | Greeting | Workshop/crafting hub | 1 | Always | role/tip/close | 0 |
| nimble_role | Role | Crafting/workshop NPC role | 1 | Always | nimble_opening | 0 |
| nimble_place | Place | Workshop placeholder note | 1 | Always | nimble_opening | 0 |
| nimble_gameplay_tip | Gameplay tip | Crafting uses recipe/inventory | 1 | Always | nimble_opening | 0 |
| nimble_rumor | Rumor | Future structures rumor | 1 | Always | nimble_opening | 0 |
| nimble_context | Context | Construction hours placeholder | 1 | TODO_NARRATIVE_FINALIZATION | nimble_opening | 0 |
| nimble_shop_note | Shop/service | No shop now | 1 | Always | nimble_opening | 0 |
| nimble_future_quest | Future quest | Workshop/building hook | 1 | TODO_NARRATIVE_FINALIZATION | nimble_opening | 0 |
| nimble_repeat | Fallback | Service deferred note | 1 | Always | nimble_opening | 0 |
| nimble_goodbye | Goodbye | Close line | 0 | Always | close | 1 |

## Existing extra: npc_vaalara_wanderer_01

Runtime asset: `Assets/_Game/Data/Dialogues/DialogueTree_Wanderer.asset`

Status: retained existing temporary extra, not counted in MVP completeness.
