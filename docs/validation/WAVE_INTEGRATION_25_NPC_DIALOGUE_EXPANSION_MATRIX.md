# WAVE INTEGRATION 25 - NPC Dialogue Expansion Matrix

Status: ALL_23_DIALOGUE_SETS_ALREADY_IMPLEMENTED_IN_WAVE12C

All 23 canonical NPCs already have DialogueTreeSO assets with 10+ nodes from WAVE12C.
No new dialogue content required. WAVE25 creates NpcDialogueSetRegistry (C# registry class) to document coverage.

| NpcId | DisplayName | DialogueTreeSOAsset | NodeCount | HasGreeting | HasRole | HasService | HasTown | HasGoodbye | Status |
|---|---|---|---|---|---|---|---|---|---|
| npc_pip | Pip Semente-Solta | DialogueTree_Pip.asset | 10 | YES | YES | YES | YES | YES | COMPLETE |
| npc_sylveth | Sylveth | DialogueTree_Sylveth.asset | 10 | YES | YES | YES | YES | YES | COMPLETE |
| npc_brumdar | Brumdar Ferro-Quieto | DialogueTree_Brumdar.asset | 10 | YES | YES | YES | YES | YES | COMPLETE |
| npc_renko | Renko Tres-Sorrisos | DialogueTree_Renko.asset | 10 | YES | YES | YES | YES | YES | COMPLETE |
| npc_thalindra | Thalindra Veu-de-Lua | DialogueTree_Thalindra.asset | 10 | YES | YES | YES | YES | YES | COMPLETE |
| npc_zrix | Zrix das Estradas | DialogueTree_Zrix.asset | 10 | YES | YES | YES | YES | YES | COMPLETE |
| npc_nimble | Nimble Galhobaixo | DialogueTree_Nimble.asset | 10 | YES | YES | YES | YES | YES | COMPLETE |
| npc_corvus | Padre Corvus | DialogueTree_Corvus.asset | 10 | YES | YES | YES | YES | YES | COMPLETE |
| npc_mara | Mara Vellum | DialogueTree_Mara.asset | 10 | YES | YES | YES | YES | YES | COMPLETE |
| npc_gurd | Gurd Carvalho-Torto | DialogueTree_Gurd.asset | 10 | YES | YES | YES | YES | YES | COMPLETE |
| npc_hund | Hund Carvalho-Torto | DialogueTree_Hund.asset | 10 | YES | YES | YES | YES | YES | COMPLETE |
| npc_ozzra | Ozzra Fumacazul | DialogueTree_Ozzra.asset | 10 | YES | YES | YES | YES | YES | COMPLETE |
| npc_gruta | Gruta Panela-Funda | DialogueTree_Gruta.asset | 10 | YES | YES | YES | YES | YES | COMPLETE |
| npc_yael | Yael Noite-Mansa | DialogueTree_Yael.asset | 10 | YES | YES | YES | YES | YES | COMPLETE |
| npc_dagna | Dagna Rocha-Morna | DialogueTree_Dagna.asset | 10 | YES | YES | YES | YES | YES | COMPLETE |
| npc_alaric | Ser Alaric Veyr | DialogueTree_Alaric.asset | 10 | YES | YES | YES | YES | YES | COMPLETE |
| npc_mirela | Mirela dos Lacos | DialogueTree_Mirela.asset | 10 | YES | YES | YES | YES | YES | COMPLETE |
| npc_eiran | Eiran Valeclaro | DialogueTree_Eiran.asset | 10 | YES | YES | YES | YES | YES | COMPLETE |
| npc_liora | Liora Canta-Rio | DialogueTree_Liora.asset | 10 | YES | YES | YES | YES | YES | COMPLETE |
| npc_orlan | Orlan Pouso-Curto | DialogueTree_Orlan.asset | 10 | YES | YES | YES | YES | YES | COMPLETE |
| npc_savra | Savra Escama-Verde | DialogueTree_Savra.asset | 10 | YES | YES | YES | YES | YES | COMPLETE |
| npc_tovin | Tovin Maos-de-Selo | DialogueTree_Tovin.asset | 10 | YES | YES | YES | YES | YES | COMPLETE |
| npc_maelor | Maelor Cinza | DialogueTree_Maelor.asset | 10 | YES | YES | YES | YES | YES | COMPLETE |

## 5 MVP NPCs with Distinct Dialogue (spec requirement)

1. npc_pip — town guide/tutorial/delivery hook — 10 nodes
2. npc_sylveth — seed/farm/crop tips — 10 nodes
3. npc_brumdar — blacksmith/repair/cave preparation — 10 nodes
4. npc_renko — general store/bargain — 10 nodes
5. npc_thalindra — quest offer + archive lore — 10+ nodes (runtime quest tree also present)

All 5 confirmed DISTINCT with different greeting/role/service/goodbye lines.

## NpcDialogueSetRegistry

C# class created in WAVE25 mapping NpcId to line count.
Used by ValidateWave25TownNpcSchedulesDialogue for automated coverage checks.
