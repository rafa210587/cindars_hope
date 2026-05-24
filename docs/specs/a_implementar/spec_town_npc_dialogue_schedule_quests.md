# SPEC - Town NPC dialogue, schedule e quests

> Spec ID: spec_town_npc_dialogue_schedule_quests
> Status: A implementar
> Ordem de execucao: 08
> Depende de: 00-07
> Bloqueia: 15, 17
> Tipo: Runtime/UI
> Fonte: docs/specs/ como fonte unica; fontes absorvidas listadas abaixo.
> Escopo: Evoluir Town/Pip placeholder para NPCs formais com NpcDataSO, DialogueModal, dialogos simples/ramificados, posicoes fixas, um NPC ambulante de teste, shop binding e hooks futuros de agenda/quest.
> Fora de escopo: quests ativas, relationship/friendship, agenda completa por horario/dia/clima, clima, cutscenes, romance, UI final consolidada, Packages, ProjectSettings e docs_old.

Fontes absorvidas:
- docs/refinements/a_implementar/pre_refinamentos/refinamento_init_town_npc_dialogue_schedule_quests.md
- decisoes da spec 06 sobre `DialogueModal`, `OpeningLine`, `ClosingLine`, lojistas e Pip recepcionista.

---

# /speckit.specify

## Contexto

Town existe como cena/fluxo MVP com transicao Farm <-> Town e NPC Pip placeholder.

Evidencia:

```text
Assets/_Game/Scripts/SceneManagement/**
Assets/_Game/Scripts/Economy/**
docs/specs/implementados/spec_town_001_town_scene_portais_npc_pip_comercio.md
```

A spec 06 formalizou lojas da cidade, `DialogueModal`, Pip como recepcao e dois lojistas. Esta spec 08 deve consolidar esses NPCs em `NpcDataSO`/dialogue runtime sem recriar sistemas paralelos.

## Pre-condicoes

Implementar runtime somente depois de specs 02-07 estarem realmente implementadas:

```text
02 - save schema migration
03 - inventory slots/capacity/painel de itens
04 - farm irrigacao/solo/menu contextual
05 - world activities/fishing/trees/pickups/loot
06 - economy/shop/dialogue modal
07 - crafting/workstations/modal stack
```

Antes de alterar codigo, revalidar:

```text
Assets/_Game/Scripts/Town/**
Assets/_Game/Scripts/NPC/**
Assets/_Game/Scripts/UI/Dialogue/**
Assets/_Game/Scripts/UI/Modal/**
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Data/NPCs/**
```

Se `DialogueModal`/modal stack da spec 06 ainda nao existir, nao implementar runtime desta spec; registrar bloqueio.

## Problema

Gaps atuais:

- Pip ainda e placeholder.
- Nao ha `NpcDataSO` consolidado.
- Nao ha `DialogueTreeSO` oficial.
- Nao ha dialogo com escolhas simples.
- Lojistas podem existir como shop runtime, mas precisam ser NPCs formais.
- Nao ha NPC ambulante para testar movimento simples de cidade.
- Nao ha estrutura de posicao fixa por NPC.
- Quests e relationship ainda nao devem entrar, mas precisam de hooks futuros sem contaminar o MVP.

## Objetivo

Implementar camada minima de NPCs da cidade:

- formalizar Pip + 2 lojistas + 1 NPC ambulante de teste em `NpcDataSO`;
- usar `DialogueModal` como UI oficial de fala;
- usar `DialogueTreeSO` simples com nos e escolhas curtas;
- Pip ganha dialogo simples adicional para orientar o jogador;
- lojistas entram tambem nesta spec como NPCs formais, mantendo shop binding da spec 06;
- NPC ambulante anda pela cidade de forma aleatoria, lenta e segura;
- NPC ambulante diz aleatoriamente 1 de 3 frases genericas sobre a lore de Vaalara;
- por enquanto, NPCs usam posicao fixa, exceto o NPC ambulante de teste;
- quests e relationship ficam fora do MVP.

## Decisoes aprovadas

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

## NPCs minimos desta spec

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

## Dados

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

## DialogueModal

Regras:

- Usar o mesmo `DialogueModal`/modal stack da spec 06.
- Nao criar outro modal de dialogo paralelo.
- Nao sobrepor ShopMenu, BuyPanel, SellPanel, CraftingModal ou InventoryPanel.
- Movimento do player fica bloqueado enquanto o dialogo esta aberto.
- `E`, `Enter` ou `Space` avancam/confirmam.
- `W/S` navegam escolhas quando houver escolhas.
- `Esc` fecha com comportamento seguro.

## Fluxo de interacao

### NPC com dialogo simples

```text
Player pressiona E em NPC
Abrir DialogueModal
Mostrar OpeningLine ou node inicial
Se houver escolhas, listar opcoes
Confirmar escolha
Executar ActionType se houver
Mostrar ClosingLine ao encerrar quando aplicavel
Fechar modal
Restaurar input
```

### NPC lojista

```text
Player pressiona E em lojista
Abrir DialogueModal com OpeningLine
Escolha ou acao OpenShop encaminha para fluxo de loja da spec 06
Ao sair da loja/conversa, mostrar ClosingLine em DialogueModal
```

### NPC ambulante

```text
Player pressiona E em npc_vaalara_wanderer_01
Selecionar uma fala aleatoria de lore
Abrir DialogueModal com a fala
Fechar ao confirmar/Esc
```

## Posicao fixa / schedule minimo

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

## Quests

Fora do escopo desta spec.

Permitido apenas preparar hooks de dados sem comportamento ativo:

```text
QuestId opcional futuro
DialogueActionType futuro
```

Nao criar quest board, quest progress, rewards ou delivery nesta spec.

## Relationship/friendship

Fora do escopo desta spec.

Nao criar pontos de friendship, affinity, romance ou gifts agora.

## Save/load

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

## Eventos

Usar existentes se houver equivalentes. Criar somente se necessario:

```text
NpcInteractionStartedEvent
NpcInteractionEndedEvent
DialogueNodeShownEvent opcional
DialogueChoiceSelectedEvent opcional
NpcShopRequestedEvent opcional
NpcWanderTargetChangedEvent opcional
```

Nao duplicar eventos de UI/modal ja existentes.

## Invariantes anti-regressao

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

Se algum comportamento de loja ou modal ainda nao estiver implementado, bloquear/registrar pendencia em vez de criar sistema paralelo.

## Criterios de aceite

- Pip usa `NpcDataSO`.
- Pip possui dialogo simples com pelo menos 3 opcoes de orientacao.
- Pip nao abre loja e nao cria quest.
- Os 2 lojistas usam `NpcDataSO` e mantem `ShopBinding`.
- NPC ambulante `npc_vaalara_wanderer_01` existe.
- NPC ambulante anda aleatoriamente em baixa velocidade dentro da cidade.
- NPC ambulante nao bloqueia pontos criticos.
- NPC ambulante fala aleatoriamente 1 de 3 frases de lore de Vaalara.
- DialogueTreeSO suporta nos simples e escolhas curtas.
- DialogueModal e usado para todos os dialogos desta spec.
- Quests nao sao implementadas como gameplay ativo.
- Relationship/friendship nao e implementado.
- Save/load nao serializa referencias Unity.
- Nenhum item das invariantes anti-regressao e quebrado.
- Validacao documental e Unity compile validation registradas.

---

# /speckit.plan

## Arquitetura

Sistemas afetados:

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
```

Managers/bridges Unity devem ser finos. Dados de NPC/dialogo ficam em ScriptableObject. Runtime resolve IDs, modal stack e acoes.

## Ordem segura de implementacao

1. Revalidar TownScene, Pip, NPC/shop da spec 06 e DialogueModal.
2. Criar `NpcDataSO` e `DialogueTreeSO` sem trocar runtime ainda.
3. Migrar Pip para `NpcDataSO`.
4. Formalizar os 2 lojistas como `NpcDataSO` com `ShopBinding`.
5. Criar NPC ambulante de teste com `RandomWander`.
6. Implementar dialogue runtime simples usando `DialogueModal` existente.
7. Integrar choices simples e `OpenShop`.
8. Implementar save/load minimo, se necessario.
9. Validar anti-regressao e atualizar tracking.

## Fluxos

### Interagir com Pip

```text
E em Pip
Abrir DialogueModal
Mostrar abertura
Exibir escolhas: lojas / atividades / Vaalara / sair
Navegar W/S
Confirmar E/Enter/Space
Mostrar resposta
Fechar com despedida
```

### Interagir com lojista

```text
E em lojista
DialogueModal OpeningLine
Opcao/acao OpenShop
Fluxo de loja da spec 06
DialogueModal ClosingLine ao sair
```

### Interagir com NPC ambulante

```text
E em wanderer
Escolher aleatoriamente uma frase de lore
Mostrar em DialogueModal
Fechar
```

### Random wander

```text
A cada intervalo
Escolher ponto aleatorio dentro de area delimitada
Mover em baixa velocidade
Pausar ao chegar
Nao atravessar bloqueios/pontos criticos
Pausar movimento se player interagir
Retomar depois
```

## Riscos de regressao

- Criar sistema paralelo de dialogo sem usar `DialogueModal`.
- Lojistas perderem shop binding da spec 06.
- NPC ambulante bloquear portal ou player.
- Pip voltar a ser lojista por fallback antigo.
- Modal de dialogo sobrepor shop/crafting/inventory.
- Quests entrarem parcialmente sem save/load.

## Mitigacao

- Reutilizar modal stack da spec 06.
- Lojistas mantem `ShopId` estavel.
- Wander area delimitada e velocidade baixa.
- Quests/relationship explicitamente fora do escopo.
- Fallback antigo de Pip como vendedor deve ser removido/deprecated se existir.

---

# /speckit.tasks

## Tasks

- [ ] Revalidar estado real de Town/NPC/Dialogue/Shop antes de alterar runtime.
- [ ] Confirmar specs 02-07 implementadas antes de runtime.
- [ ] Criar/ajustar `NpcDataSO`.
- [ ] Criar/ajustar `DialogueTreeSO`, `DialogueNode`, `DialogueChoice`.
- [ ] Criar/ajustar `NpcController`.
- [ ] Criar/ajustar `DialogueManager` usando `DialogueModal` existente.
- [ ] Migrar Pip para `NpcDataSO` com dialogo de orientacao.
- [ ] Formalizar 2 lojistas como `NpcDataSO` com shop binding.
- [ ] Criar `npc_vaalara_wanderer_01` com RandomWander lento.
- [ ] Implementar 3 frases aleatorias de lore para o NPC ambulante.
- [ ] Garantir que quests e relationship nao entram como gameplay ativo.
- [ ] Persistir estado minimo de NPC se necessario.
- [ ] Atualizar docs implementados/status/registries/refinements/log.

## Arquivos permitidos

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
docs/specs/**
docs/refinements/**
docs/IMPLEMENTATION_STATUS.md
PROJECT_LOG.md
```

## Arquivos proibidos

```text
docs_old/**
Packages/**
ProjectSettings/**
```

## Definition of Done

- NPCs formalizados via `NpcDataSO`.
- Pip tem dialogo adicional de orientacao e nao e lojista.
- Lojistas mantem shop binding.
- NPC ambulante random/lento funciona.
- DialogueModal e usado sem sobreposicao de modais.
- Quests/relationship continuam fora do runtime.
- Invariantes anti-regressao preservadas.
- Validacao documental e Unity registrada.

## Validacao obrigatoria

Rodar:

```powershell
.\tools\docs\validate_docs.ps1
.\tools\unity\RunUnityCompileValidation.ps1
.\tools\unity\ScanUnityLogs.ps1
```

Play Mode minimo:

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
