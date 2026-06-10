# Fix — Thalindra Quest Dialogue Wiring

## Status

RUNTIME_AUTO_WIRED

A quest da Thalindra não deve mais exigir wiring manual no Unity apenas para teste.

## O que ficou automático

1. `NpcController` agora cria uma `DialogueTreeSO` runtime para `npc_thalindra` quando:
   - o NPC não tem `DialogueTree` no `NpcDataSO`; ou
   - a `DialogueTree` existente não contém uma choice `OfferQuest` para `quest_first_supplies_for_cindar`.

2. A choice aparece no diálogo com prefixo:

```text
! Qual é a tarefa?
```

3. Ao escolher a opção, o `NpcController` publica `QuestGiverInteractedEvent` com:

```text
NpcId = npc_thalindra
QuestId = quest_first_supplies_for_cindar
Mode = Offer / TurnIn / NoQuest, conforme QuestService
```

4. `QuestGiverInteractable` também tem fallback runtime para Thalindra:
   - se `_npcId` estiver vazio, lê o `NpcId` do `NpcController`;
   - se `_offeredQuestIds` estiver vazio e o NPC for `npc_thalindra`, usa `quest_first_supplies_for_cindar`.

Isso evita a necessidade de remover `QuestGiverInteractable` da cena só para o fluxo funcionar.

## O que não precisa mais fazer no Unity

Não é mais obrigatório:

```text
CindarsHope -> Setup -> Create Thalindra Quest DialogueTree
TownScene -> npc_thalindra -> NpcController -> arrastar asset DialogueTree
Remover QuestGiverInteractable de npc_thalindra
```

A ferramenta editor `CreateThalindraQuestDialogueTree` pode continuar existindo como opção de authoring, mas não é necessária para Play Mode smoke test.

## Verificação no Play Mode

1. Abrir `TownScene`.
2. Press Play.
3. Interagir com `npc_thalindra`.
4. Confirmar que o diálogo abre com:

```text
! Qual é a tarefa?
```

5. Selecionar essa opção.
6. Confirmar que o painel de aceitação da quest aparece.
7. Clicar em `Aceitar`.
8. Confirmar no console:

```text
[QuestOfferPanelController] Quest accepted: quest_first_supplies_for_cindar
```

## Verificação do Modal Guard

Durante diálogo ou painel de quest offer:

```text
Dash não deve executar.
Double tap dodge não deve executar.
Block/input de movimento defensivo não deve furar modal.
```

Ao fechar diálogo/painel, aguardar uma fração de segundo e confirmar que os inputs voltam ao normal.

## Resultado esperado

```text
Thalindra quest offer funciona sem asset manual e sem alteração manual da cena.
```

## Dívida restante

```text
AUTHORING_FINAL_STILL_RECOMMENDED
```

A árvore runtime é fallback para teste/jogabilidade. Depois, quando fecharmos narrativa final da Thalindra, pode ser criado/atribuído um asset definitivo de dialogue tree com todas as falas finais.
