# refinamento_init_town_npc_dialogue_schedule_quests

> **Status:** Refinamento inicial a implementar
> **Spec futura sugerida:** `spec_town_npc_dialogue_schedule_quests.md`
> **Objetivo:** evoluir Town/Pip placeholder para NPCs com diÃ¡logo, agenda, loja e quests leves.

---

## 1. Estado atual

Town existe como cena/fluxo MVP com transiÃ§Ã£o Farm â†” Town e NPC Pip placeholder.

EvidÃªncia:

```text
Assets/_Game/Scripts/SceneManagement/**
Assets/_Game/Scripts/Economy/**
docs/specs/implementados/spec_town_001_town_scene_portais_npc_pip_comercio.md
```

---

## 2. Gaps

- NPC Pip Ã© placeholder.
- NÃ£o hÃ¡ `NpcDataSO` consolidado.
- NÃ£o hÃ¡ diÃ¡logo ramificado.
- NÃ£o hÃ¡ agenda por horÃ¡rio/dia/clima.
- NÃ£o hÃ¡ relationship/friendship.
- NÃ£o hÃ¡ quest board ou quests simples.
- NÃ£o hÃ¡ loja ligada formalmente a NPC/ShopDataSO.

---

## 3. Escopo esperado

### Dados

Criar:

```text
NpcDataSO
DialogueTreeSO
DialogueNode
NpcScheduleSO
NpcShopBinding
QuestDataSO futuro
```

### Runtime

Criar:

```text
NpcController
DialogueManager
NpcScheduleManager
QuestManager mÃ­nimo futuro
```

### UX

- InteraÃ§Ã£o abre diÃ¡logo.
- DiÃ¡logo pode abrir loja.
- NPC pode ter fala por estado do jogo.
- Agenda movimenta ou ativa/desativa NPC por local/horÃ¡rio.

---

## 4. Arquivos provÃ¡veis

```text
Assets/_Game/Scripts/Town/NpcController.cs
Assets/_Game/Scripts/Town/Data/NpcDataSO.cs
Assets/_Game/Scripts/Dialogues/**
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/Economy/Data/ShopDataSO.cs
Assets/_Game/Scripts/UI/Dialogues/**
```

---

## 5. Definition of Done

- [ ] Pip usa `NpcDataSO`.
- [ ] InteraÃ§Ã£o abre diÃ¡logo, nÃ£o apenas placeholder.
- [ ] DiÃ¡logo pode disparar loja configurada.
- [ ] Agenda mÃ­nima por dia/hora funciona ou fica preparada com dados.
- [ ] Estado de NPC/quest nÃ£o quebra save/load futuro.

---

## 6. ValidaÃ§Ã£o

1. Entrar em TownScene.
2. Interagir com Pip.
3. Ver diÃ¡logo.
4. Abrir loja a partir de diÃ¡logo.
5. Salvar/carregar sem perder estado essencial.
