# refinamento_init_town_npc_dialogue_schedule_quests

> Status: Refinamento inicial a implementar
> Spec futura relacionada: `docs/specs/a_implementar/spec_town_npc_dialogue_schedule_quests.md`
> Objetivo: evoluir Town/Pip placeholder para NPCs formais com NpcDataSO, DialogueModal, dialogos simples/ramificados, posicoes fixas, um NPC ambulante de teste, shop binding e hooks futuros de agenda/quest.

---

## 1. Estado atual

Town existe como cena/fluxo MVP com transicao Farm <-> Town e NPC Pip placeholder.

Evidencia:

```text
Assets/_Game/Scripts/SceneManagement/**
Assets/_Game/Scripts/Economy/**
docs/specs/implementados/spec_town_001_town_scene_portais_npc_pip_comercio.md
```

A spec 06 formalizou lojas da cidade, `DialogueModal`, Pip como recepcao e dois lojistas. Esta spec 08 deve consolidar esses NPCs em `NpcDataSO`/dialogue runtime sem recriar sistemas paralelos.

---

## 2. Gaps

- Pip ainda e placeholder.
- Nao ha `NpcDataSO` consolidado.
- Nao ha `DialogueTreeSO` oficial.
- Nao ha dialogo com escolhas simples.
- Lojistas podem existir como shop runtime, mas precisam ser NPCs formais.
- Nao ha NPC ambulante para testar movimento simples de cidade.
- Nao ha estrutura de posicao fixa por NPC.
- Quests e relationship ainda nao devem entrar, mas precisam de hooks futuros sem contaminar o MVP.

---

## 3. Decisoes aprovadas

- Criar 1 NPC novo alem de Pip e dos 2 lojistas.
- Esse NPC novo anda pela cidade de forma aleatoria, em baixa velocidade, para teste de movement/pathing.
- Esse NPC novo possui 3 frases genericas sobre a lore de Vaalara e escolhe uma aleatoriamente ao interagir.
- Pip ganha dialogo adicional alem de recepcao: deve orientar o jogador sobre cidade, fazenda, lojas e cavernas, sem virar quest.
- Os 2 lojistas entram formalmente nesta spec como `NpcDataSO` e mantem shop binding.
- Agenda por enquanto e posicao fixa; nao implementar schedule completo por horario/dia/clima.
- Quests nao entram agora.
- Relationship/friendship nao entra agora.
- DialogueTreeSO deve suportar escolhas simples, mas sem sistema complexo.
- O sistema de dialogo deve reutilizar `DialogueModal` da spec 06, sem outro modal paralelo.

---

## 4. NPCs minimos

### Pip Miudinho

ID sugerido:

```text
npc_pip_miudinho
```

Papel:

```text
Recepcao da cidade
Guia leve inicial
Nao lojista
Nao quest giver nesta spec
```

Dialogo adicional sugerido:

```text
OpeningLine: "Bem-vindo de volta. Cindar's Hope ainda e pequena, mas cada canto daqui guarda um caminho."
Choice: "Onde compro coisas?" -> fala sobre os dois lojistas.
Choice: "O que posso fazer por aqui?" -> fala sobre fazenda, crafting e cavernas.
Choice: "O que e este lugar?" -> fala curta sobre Vaalara e a esperanca de reconstruir.
ClosingLine: "Va com calma. Vaalara recompensa quem observa antes de correr."
```

Regras:

- Pip nao abre loja.
- Pip nao cria quest nesta spec.
- Se o comportamento de recepcao/movimento da spec 06 existir, preservar.
- O dialogo deve usar `DialogueModal` e nao sobrepor outros modais.

### Lojista de armas e armaduras

ID sugerido:

```text
npc_shop_weapons_armor
```

Regras:

- Deve ter `NpcDataSO`.
- Deve ter `OpeningLine` e `ClosingLine`.
- Deve ter `ShopBinding` para `shop_weapons_armor`.
- Dialogo pode abrir a loja via escolha/menu da spec 06.
- Posicao fixa por enquanto.

### Lojista de sementes e utensilios

ID sugerido:

```text
npc_shop_seeds_tools
```

Regras:

- Deve ter `NpcDataSO`.
- Deve ter `OpeningLine` e `ClosingLine`.
- Deve ter `ShopBinding` para `shop_seeds_tools`.
- Dialogo pode abrir a loja via escolha/menu da spec 06.
- Posicao fixa por enquanto.

### NPC ambulante de teste

ID sugerido:

```text
npc_vaalara_wanderer_01
```

Papel:

```text
NPC generico de lore/teste de cidade
Nao lojista
Nao quest giver
Movimento aleatorio lento
```

Movimento:

- Anda pela cidade de forma aleatoria.
- Baixa velocidade.
- Deve respeitar area navegavel/limites da TownScene.
- Nao deve bloquear portais, entrada/saida, NPCs lojistas ou pontos criticos.
- Pode pausar entre movimentos.
- Se nao houver pathfinding robusto, usar wandering simples dentro de uma area delimitada.

Frases aleatorias de lore:

```text
1. "Dizem que Vaalara nasceu de caminhos antigos, e que alguns deles ainda acordam quando ninguem esta olhando."
2. "As pedras deste mundo lembram mais do que os homens. As vezes, escutar e mais seguro do que cavar."
3. "Cindar's Hope parece pequena, mas em Vaalara ate uma vila pode estar sobre uma historia enorme."
```

Ao interagir:

- escolher aleatoriamente uma das 3 frases;
- exibir em `DialogueModal`;
- ao fechar, usar despedida simples especifica ou encerrar sem abrir loja;
- nao criar quest.

---

## 5. Dados esperados

Criar ou equivalente:

```text
NpcDataSO
DialogueTreeSO
DialogueNode
DialogueChoice
NpcShopBinding
NpcPositionData
NpcWanderData
```

Campos minimos de `NpcDataSO`:

```text
NpcId
DisplayName
OpeningLine
ClosingLine
DialogueTreeId opcional
ShopId opcional
DefaultSceneId
DefaultPositionId
MovementMode
WanderData opcional
```

`MovementMode` minimo:

```text
Static
RandomWander
```

Campos minimos de `DialogueTreeSO`:

```text
DialogueTreeId
StartNodeId
Nodes[]
```

Campos minimos de `DialogueNode`:

```text
NodeId
Text
Choices[]
RandomLinePool opcional
```

Campos minimos de `DialogueChoice`:

```text
Label
NextNodeId opcional
ActionType opcional
ActionPayload opcional
```

ActionType minimo:

```text
None
OpenShop
CloseDialogue
```

Quests ficam fora agora; nao criar `StartQuest` como comportamento ativo nesta spec.

---

## 6. DialogueModal

Regras:

- Usar o mesmo `DialogueModal`/modal stack da spec 06.
- Nao criar outro modal de dialogo paralelo.
- Nao sobrepor ShopMenu, BuyPanel, SellPanel, CraftingModal ou InventoryPanel.
- Movimento do player fica bloqueado enquanto o dialogo esta aberto.
- `E`, `Enter` ou `Space` avancam/confirmam.
- `W/S` navegam escolhas quando houver escolhas.
- `Esc` fecha com comportamento seguro.

---

## 7. Posicao fixa / schedule minimo

Por enquanto, nao implementar schedule completo.

Implementar apenas:

```text
DefaultSceneId
DefaultPositionId
MovementMode
```

Regras:

- Pip e lojistas usam `MovementMode=Static`, salvo recepcao de Pip ja definida na spec 06.
- NPC ambulante usa `MovementMode=RandomWander`.
- Agenda por Morning/Afternoon/Night, dia da semana e clima ficam fora do MVP.
- Estrutura pode deixar hooks futuros, mas sem comportamento ativo.

---

## 8. Quests e relationship

Fora do escopo desta spec:

```text
Quests ativas
Quest board
Quest progress/rewards
Delivery/fetch quests
Relationship/friendship
Affinity
Romance
Gifts
```

Permitido apenas preparar hooks de dados sem comportamento ativo.

---

## 9. Save/load

Persistir somente estado minimo necessario:

```text
NpcId
SceneId
PositionId opcional
CurrentPosition opcional para RandomWander, se necessario
HasMet opcional
DialogueFlags[] opcional
```

Regras:

- Falas e arvores de dialogo sao conteudo, nao precisam persistir.
- Shop stock continua sendo responsabilidade da spec 06.
- Quests nao entram agora.
- Nao serializar `NpcDataSO`, `DialogueTreeSO`, `GameObject`, `Transform`, `MonoBehaviour`, `Sprite`, `Collider` ou `Rigidbody`.

---

## 10. Invariantes anti-regressao

Esta spec nao pode quebrar:

- DialogueModal/modal stack da spec 06;
- lojas e shop binding da spec 06;
- Pip recepcionista da spec 06;
- crafting modal da spec 07;
- inventory/hotbar/HUD;
- interacao `E` fora de NPCs;
- portais Farm <-> Town;
- movimento do player quando nenhum modal esta aberto;
- regra de nao usar `GameObject.Find()` ou `FindObjectOfType()`;
- regra de nao serializar referencias Unity em DTOs.

---

## 11. Arquivos provaveis

```text
Assets/_Game/Scripts/Town/**
Assets/_Game/Scripts/NPC/**
Assets/_Game/Scripts/Dialogues/**
Assets/_Game/Scripts/UI/Dialogue/**
Assets/_Game/Scripts/UI/Modal/**
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Data/NPCs/**
Assets/_Game/Data/Dialogues/**
Assets/_Game/Scenes/TownScene.unity
```

---

## 12. Definition of Done

- [ ] Pip usa `NpcDataSO`.
- [ ] Pip possui dialogo simples com pelo menos 3 opcoes de orientacao.
- [ ] Pip nao abre loja e nao cria quest.
- [ ] Os 2 lojistas usam `NpcDataSO` e mantem `ShopBinding`.
- [ ] NPC ambulante `npc_vaalara_wanderer_01` existe.
- [ ] NPC ambulante anda aleatoriamente em baixa velocidade dentro da cidade.
- [ ] NPC ambulante nao bloqueia pontos criticos.
- [ ] NPC ambulante fala aleatoriamente 1 de 3 frases de lore de Vaalara.
- [ ] DialogueTreeSO suporta nos simples e escolhas curtas.
- [ ] DialogueModal e usado para todos os dialogos desta spec.
- [ ] Quests nao sao implementadas como gameplay ativo.
- [ ] Relationship/friendship nao e implementado.
- [ ] Save/load nao serializa referencias Unity.
- [ ] Invariantes anti-regressao preservadas.

---

## 13. Validacao

1. Entrar em TownScene.
2. Interagir com Pip e validar 3 opcoes de orientacao.
3. Confirmar que Pip nao abre loja.
4. Interagir com lojista de armas/armaduras e validar shop binding.
5. Interagir com lojista de sementes/utensilios e validar shop binding.
6. Validar que todos os dialogos usam DialogueModal.
7. Validar que DialogueModal nao sobrepoe ShopModal/CraftingModal/InventoryPanel.
8. Validar NPC ambulante andando lentamente dentro de area segura.
9. Interagir 5 vezes com NPC ambulante e confirmar variacao aleatoria entre as 3 frases.
10. Validar que NPC ambulante nao bloqueia portais ou NPCs de loja.
11. Salvar/carregar e validar que a cena nao perde estado essencial.
12. Validar Unity compile validation e docs validation.
