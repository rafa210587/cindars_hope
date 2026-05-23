# refinamento_init_town_npc_dialogue_schedule_quests

> **Status:** Refinamento inicial a implementar  
> **Spec futura sugerida:** `spec_town_npc_dialogue_schedule_quests.md`  
> **Objetivo:** evoluir Town/Pip placeholder para NPCs com diálogo, agenda, loja e quests leves.

---

## 1. Estado atual

Town existe como cena/fluxo MVP com transição Farm ↔ Town e NPC Pip placeholder.

Evidência:

```text
Assets/_Game/Scripts/SceneManagement/**
Assets/_Game/Scripts/Economy/**
docs/specs/implementados/spec_town_001_town_scene_portais_npc_pip_comercio.md
```

---

## 2. Gaps

- NPC Pip é placeholder.
- Não há `NpcDataSO` consolidado.
- Não há diálogo ramificado.
- Não há agenda por horário/dia/clima.
- Não há relationship/friendship.
- Não há quest board ou quests simples.
- Não há loja ligada formalmente a NPC/ShopDataSO.

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
QuestManager mínimo futuro
```

### UX

- Interação abre diálogo.
- Diálogo pode abrir loja.
- NPC pode ter fala por estado do jogo.
- Agenda movimenta ou ativa/desativa NPC por local/horário.

---

## 4. Arquivos prováveis

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
- [ ] Interação abre diálogo, não apenas placeholder.
- [ ] Diálogo pode disparar loja configurada.
- [ ] Agenda mínima por dia/hora funciona ou fica preparada com dados.
- [ ] Estado de NPC/quest não quebra save/load futuro.

---

## 6. Validação

1. Entrar em TownScene.
2. Interagir com Pip.
3. Ver diálogo.
4. Abrir loja a partir de diálogo.
5. Salvar/carregar sem perder estado essencial.
