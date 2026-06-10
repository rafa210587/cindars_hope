# Fix — Thalindra Quest Dialogue Wiring

## Steps no Unity Editor

1. Menu: CindarsHope -> Setup -> Create Thalindra Quest DialogueTree
   - Gera: Assets/_Game/Data/NPC/Thalindra/DialogueTree_Thalindra_QuestOffer.asset

2. Abrir TownScene -> selecionar GameObject npc_thalindra

3. No NpcController:
   - Campo "Dialogue Tree" -> arraste DialogueTree_Thalindra_QuestOffer.asset

4. Remover QuestGiverInteractable do npc_thalindra (se presente como componente separado)
   - O quest offer agora passa pelo NpcController via OfferQuest choice

5. Press Play -> interagir com Thalindra -> dialogo aparece com opcoes:
   - "! Qual e a tarefa?" -> abre painel de aceitacao de quest
   - "Nao tenho tempo agora." -> fecha dialogo

6. Clicar "Aceitar" -> quest_first_supplies_for_cindar aceita

## Verificacao do Modal Guard

Apos o fix B, durante o dialogo ou o painel de quest offer:
- Dash (Shift duplo) nao deve funcionar
- Dodge (duplo tap direcional) nao deve funcionar
- Ao fechar o dialogo/painel, aguardar ~0.3s para nao disparar dodge por duplo tap acumulado
