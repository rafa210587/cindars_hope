# SPEC - Town NPC dialogue, schedule e quests

> Spec ID: spec_town_npc_dialogue_schedule_quests
> Status: Implementado
> Ordem de execucao: 08
> Data: 2026-05-24
> Evidencia: Assets/_Game/Scripts/NPC/NpcDataSO.cs, DialogueTreeSO.cs, DialogueNode.cs, DialogueChoice.cs, NpcController.cs, NpcWanderer.cs, NpcManager.cs, Assets/_Game/Data/NPCs/*.asset, Assets/_Game/Data/Dialogues/*.asset, Assets/_Game/Scripts/Save/SaveData.cs (NpcSaveData)

## Resumo de Implementacao

### Implementado:
- `NpcDataSO` com campos: NpcId, DisplayName, OpeningLine, ClosingLine, DialogueTree, ShopId, DefaultPosition, MovementMode, WanderData
- `DialogueTreeSO` com Nodes e StartNodeId
- `DialogueNode` com Text, Choices, RandomLinePool para linhas aleatorias
- `DialogueChoice` com Label, NextNodeId, ActionType (None/OpenShop/CloseDialogue), ActionPayload
- `NpcController` para gerenciar interacoes e dialogos via IInteractable
- `NpcWanderer` para movimento aleatorio com pause e velocity, respeitando Rigidbody2D
- `NpcManager` para coordenacao central de NPCs
- DialogueModal enhanceado com suporte completo a choices:
  - ShowWithChoices para mostrar opcoes
  - W/S/Up/Down para navegacao entre choices
  - E/Enter/Space para confirmar
  - Esc para fechar
  - Highlight visual para selected choice
- 4 NPCs data assets em Assets/_Game/Data/NPCs/:
  - Pip Miudinho (receptionist, static, sem loja)
  - npc_shop_weapons_armor (static lojista, shop binding)
  - npc_shop_seeds_tools (static lojista, shop binding)
  - npc_vaalara_wanderer_01 (random wander, lore speaker)
- DialogueTree_Pip com 4 escolhas (lojas, atividades, Vaalara, sair)
- DialogueTree_Wanderer com 3 frases aleatorias de lore
- NpcSaveData, NpcManagerSaveData em SaveData.cs para persistencia minima
- Integração com GameEventBus para eventos futuros

### Nao implementado (futuro):
- Agenda completa por horario/dia/clima
- Quests ativas como gameplay
- Relationship/friendship/affinity
- Complete schedule/snapshot system
- Visual positioning/pathfinding robusto para wanderer
- Integração final em TownScene Unity (posicionamento de GameObjects)
- Animacoes de movimento

### Validacao pendente:
- Unity compile validation
- Play mode test em TownScene
