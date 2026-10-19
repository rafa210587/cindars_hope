# refinamento_init_ui_ux_full_gameplay_inventory_hotbar_menus

> Status: Refinamento inicial a implementar
> Spec futura relacionada: `.specs/a_implementar/spec_ui_ux_full_gameplay_inventory_hotbar_menus.md`
> Objetivo: completar UI/UX runtime real de gameplay: HUD, hotbar, inventory, equipment, crafting, skills, shop, cave/checkpoint/death/corpse, Anya, pause/options, modal stack, input routing e feedbacks.

---

## 1. Estado atual

O projeto possui HUD/debug e alguns managers/paineis minimos, mas ainda nao possui UI final integrada.

Evidencias principais:

```text
Assets/_Game/Scripts/UI/DebugHud.cs
Assets/_Game/Scripts/UI/MenuManager.cs
Assets/_Game/Scripts/UI/MenuSystemDataSO.cs
.specs/implementados/spec_ui_001_debug_hud_feedback_mvp.md
.specs/implementados/spec_ui_002_hud_tools_hotbar_progression_debug.md
.specs/implementados/spec_ui_menu_systems_final.md
.specs/a_implementar/spec_fase9l_ui_ux_full_gameplay.md
```

---

## 2. Decisoes aprovadas

- Spec 17 implementa UI runtime real, nao apenas documentacao.
- Spec 17 nao cria gameplay novo.
- UI final deve ser Canvas/UGUI ou sistema equivalente de UI runtime Unity.
- `OnGUI` so deve permanecer para debug/dev mode.
- Teclado e obrigatorio.
- Mouse e opcional para clique em botoes.
- Gamepad/controller fica hook futuro.
- DebugHud pode continuar, mas apenas por flag dev/debug.
- Journal/quests nao entram; quests ativas continuam fora do escopo.
- Bestiary UI final entra como hook/painel simples somente se spec 13 ja tiver runtime de Bestiary pronto; caso contrario, manter botao/aba desabilitada.
- Save slots multiplos avancados nao entram; PauseMenu possui Save/Load basico se SaveManager ja suportar.

---

## 3. Gaps

- Debug HUD nao e UI final.
- Inventory, hotbar, equipment, crafting, shop, skill tree, cave/checkpoint/death e pause estao fragmentados.
- Alguns paineis podem estar em `OnGUI`/debug e precisam migrar para UI runtime final com Canvas/UGUI.
- Falta modal stack consistente.
- Falta roteamento de input consistente entre gameplay e menus.
- Falta padrao de feedback para bloqueios, erros e confirmacoes.
- Falta HUD final que reflita HP, hunger, stamina, mana, gold, tempo, status, maos e active skills.
- Falta consolidar UI de morte/corpse/Fonte de Anya/checkpoints.
- Falta garantir que UI reaja a eventos, sem polling pesado e sem hardcode de gameplay.

---

## 4. Arquitetura UI

Criar/consolidar:

```text
UIRoot
HUDController
ModalStackManager
GameplayInputRouter
ContextHintController
NotificationToastController
ConfirmationDialogController
PauseMenuController
OptionsMenuController
```

Regras:

- UI nao deve conter regra pesada de gameplay.
- UI le estado de services/managers e reage a eventos via `GameEventBus` quando aplicavel.
- UI pode chamar comandos explicitos de services quando o jogador confirma uma acao.
- MonoBehaviours de UI devem ser bridges finos.
- Dados visuais/configuracoes podem ficar em `MenuSystemDataSO`/`UIThemeDataSO`/ScriptableObjects equivalentes.
- Nao usar `GameObject.Find()` ou `FindObjectOfType()`.
- Referencias devem vir por inspector, installer, constructor/service injection ou registry aprovado.

---

## 5. Modal stack

Criar/consolidar:

```text
ModalStackManager
ModalDescriptor
ModalInputScope
```

Regra central:

```text
Apenas 1 modal principal aberto por vez.
Overlays pequenos permitidos: tooltip, confirmation dialog e toast.
```

Prioridade:

```text
ConfirmationDialog > ActiveModal > PauseMenu > HUD
```

Regras:

- Abrir modal pausa gameplay/time conforme spec 09.
- Abrir modal bloqueia movimento/ataque/interacao do player.
- `Q/E/WASD/R/T/Y/G/K/I` dentro de modal obedecem ao modal atual e nao acionam gameplay.
- `Esc` fecha modal atual.
- Se nenhum modal estiver aberto, `Esc` abre PauseMenu.
- `I` abre/fecha InventoryPanel.
- `K` abre/fecha SkillTreePanel.
- Toast nao bloqueia gameplay.
- ConfirmationDialog bloqueia input do modal abaixo ate ser resolvido.

---

## 6. GameplayInputRouter

Responsavel por separar input de gameplay e input de UI.

Estados:

```text
Gameplay
ModalOpen
PauseOpen
ConfirmationOpen
DebugOnly opcional
```

Regras:

- Em `Gameplay`, inputs vao para gameplay:
  - `I` abre inventory;
  - `K` abre skill tree;
  - `Esc` abre pause;
  - `Q/E/R/T/Y/G` seguem specs 12 e 16.
- Em `ModalOpen`, inputs vao para o modal no topo.
- Em `PauseOpen`, inputs vao para PauseMenu.
- Em `ConfirmationOpen`, apenas confirm/cancel do dialogo ativo responde.

---

## 7. HUD gameplay

HUD permanente durante gameplay normal.

Layout minimo sugerido:

```text
Topo/esquerda:
- HP
- Stamina
- Mana
- Hunger
- Status effects

Topo/direita:
- Gold
- Dia
- Hora
- Indicador Dia/Noite

Baixo/centro:
- Hotbar/maos
- ActiveSkillSlots R/T/Y/G

Proximo ao player ou parte inferior:
- Context hint
```

HUD deve mostrar:

- HP atual/max;
- stamina atual/max;
- mana atual/max se ManaManager existir;
- hunger atual;
- status effects ativos com duracao/cargas quando disponivel;
- gold;
- dia/hora/fase dia/noite;
- mao esquerda `Q`;
- mao direita `E` quando nao houver interagivel;
- active skills R/T/Y/G;
- cooldowns;
- custos de stamina/mana quando disponiveis;
- durabilidade de ferramentas/armas quando aplicavel.

---

## 8. Context hints

Criar/consolidar:

```text
ContextHintController
InteractableHintProvider
```

Exemplos:

```text
E: Interagir
E: Usar mão direita
Q: Usar mão esquerda
E: Conversar
E: Plantar
E: Recuperar corpo
E: Abrir loja
E: Abrir bancada
K: Skills
I: Inventário
```

Regras:

- Hint deve refletir prioridade real do input.
- Se `E` vai interagir com NPC/tile/object, nao mostrar `E: usar RightHand`.
- Se modal estiver aberto, context hint de gameplay deve ocultar ou ficar desabilitado.

---

## 9. Hotbar / maos / active skills

Slots fixos de HUD:

```text
LeftHand = Q
RightHand = E quando nao houver interagivel
ActiveSkillSlot0 = R
ActiveSkillSlot1 = T
ActiveSkillSlot2 = Y
ActiveSkillSlot3 = G
```

Regras:

- Mostrar item/equipment da mao esquerda.
- Mostrar item/equipment da mao direita.
- Mostrar durabilidade se aplicavel.
- Mostrar broken state.
- Mostrar skill equipada em R/T/Y/G.
- Mostrar cooldown radial/barra ou overlay simples.
- Mostrar custo insuficiente com feedback visual simples.
- Slots vazios devem ser claros.
- Hotbar continua visivel durante gameplay normal.
- Hotbar pode escurecer quando modal estiver aberto.

---

## 10. Inventory UI

Acesso:

```text
I = abrir/fechar InventoryPanel
```

Layout:

```text
Grid inicial 3x6 = 18 slots
Expansivel para 5x6 = 30 slots via mochila/equipment futuro
```

Input minimo:

```text
W/A/S/D = navegar slots
E/Enter/Space = selecionar/confirmar
Esc/I = fechar/voltar
Q/E podem navegar abas/acoes somente dentro do modal se configurado
```

Acoes de item:

```text
Use
Equip
Drop
Destroy
Split
Cancel
```

Regras:

- `Use` aparece apenas para item usable/consumable.
- `Equip` aparece apenas para equippable/tool/weapon/armor/accessory compativel.
- `Drop` cria pickup persistente no mundo via sistema da spec 05.
- `Destroy` exige confirmation dialog.
- `Split` abre mini modal de quantidade para stacks > 1.
- Inventory cheio deve mostrar feedback claro.
- A UI nao deve duplicar/remover item ate o service confirmar sucesso.
- InventoryPanel respeita modal stack.
- InventoryPanel deve refletir save/runtime real, nao lista hardcoded.

---

## 11. Equipment / Character UI

Pode ser aba dentro de Inventory/Character UI, mas deve ter layout proprio.

Slots MVP:

```text
Armor
Accessory
LeftHand
RightHand
```

Se spec 10 tiver slots adicionais, exibir tambem:

```text
Head
Chest
Legs
Boots
Ring1
Ring2
```

Fluxo:

```text
Selecionar equipment slot
Abrir inventory filtrado por itens compativeis
Confirmar item
Equipar via service
Atualizar stats derivados
Atualizar HUD/hotbar/maos
```

Mostrar:

- item equipado;
- slot vazio;
- durabilidade;
- broken state;
- stats base;
- bonus de equipment;
- AttackSpeed;
- ToxicResistance;
- ColdResistance;
- HeatResistance;
- MaxHP/MaxStamina/Mana se existirem;
- comparacao simples ao selecionar item, se possivel.

---

## 12. Crafting UI

Abrir por:

```text
E em workstation
Craft de bolso se sistema permitir RequiredStationType=None
Tecla debug opcional somente com flag dev
```

Telas/areas:

```text
RecipeList
RecipeDetails
IngredientsPanel
CraftQueue
OutputCollection
CancelConfirm
```

Mostrar:

- recipes disponiveis;
- workstation atual;
- ingredientes necessarios;
- ingredientes possuidos;
- output;
- tempo de craft;
- status do job;
- output aguardando coleta;
- inventory full;
- botao/acao de cancelamento.

Regras:

- Crafting UI chama crafting service/runtime da spec 07.
- Nao consumir ingrediente pela UI diretamente.
- Cancelamento exige confirmation dialog se houver perda/efeito relevante.
- Output so sai da station se inventory aceitar.
- UI deve suportar Workbench, Forge e CookingStation.

---

## 13. Shop UI

Fluxo:

```text
DialogueModal
Comprar / Vender / Sair
ShopBuyPanel
ShopSellPanel
TransactionFeedback
```

Comprar mostra:

- nome do vendedor;
- opening/closing line quando aplicavel;
- item;
- descricao;
- estoque atual;
- preco;
- multiplicador de preco se houver;
- gold do jogador;
- quantidade;
- bloqueio por falta de gold;
- bloqueio por estoque insuficiente;
- bloqueio por inventory cheio.

Vender mostra:

- inventory do jogador;
- preco de venda = 60% do valor normal;
- quantidade;
- confirmation dialog quando aplicavel.

Regras:

- Compra so remove estoque/gold e adiciona item se a transacao completa for valida.
- Venda so remove item se gold for adicionado.
- UI deve refletir restock diario quando ocorrer.
- Shop UI respeita modal stack.
- Pip nao abre loja.

---

## 14. Dialogue UI

Criar/consolidar:

```text
DialogueModal
DialogueChoiceList
DialogueLinePresenter
```

Regras:

- DialogueModal nao sobrepoe Shop/Crafting/Inventory sem modal stack.
- NPC opening line aparece ao iniciar conversa.
- NPC closing line aparece ao fechar quando configurado.
- Choices navegam por WASD e confirmam com E/Enter.
- NPC sem choices pode mostrar fala simples e fechar com E/Esc.
- Quests ativas ficam fora do escopo.

---

## 15. SkillTree UI

Acesso:

```text
K = abrir/fechar SkillTreePanel
```

Mostrar:

- abas/arvores: Melee, Ranged, Magic, Survival, Crafting;
- SkillPoints disponiveis;
- nodes bloqueados, disponiveis e comprados;
- prerequisites basicos;
- custo;
- capstone;
- PassiveSkill vs EquippableSkill;
- active slots R/T/Y/G;
- feedback de compra falha;
- feedback de respec disponivel somente na Fonte de Anya.

Input:

```text
W/A/S/D = mover selecao
Q/E = trocar arvore dentro do modal
Enter/E = comprar/confirmar
R/T/Y/G = atribuir skill ativa quando em modo equipar
Esc/K = fechar/voltar
```

---

## 16. Cave/checkpoint UI

Criar/consolidar:

```text
CaveCheckpointSideMenu
CaveLevelIndicator
BossGateFeedback
CaveTransitionOverlay
```

CheckpointSideMenu mostra:

- checkpoint level;
- biome name;
- status Locked/Unlocked/Current;
- acao de teleportar;
- feedback de bloqueio.

Regras:

- Menu lateral respeita modal stack.
- Teleporte so para checkpoint unlocked.
- UI nao pode burlar boss gates.
- Boss gate locked deve mostrar feedback claro.
- Checkpoint unlocked deve gerar toast/feedback.

---

## 17. Death / corpse / Anya UI

Criar/consolidar:

```text
DeathScreen
CorpseRecoveryModal
AnyaFountainMenu
```

DeathScreen mostra:

- mensagem de morte;
- informacao de que itens/equipment/gold ficaram no corpse;
- informacao de que XP voltou ao inicio do nivel atual;
- informacao de que apenas o ultimo corpse existe;
- acao para respawn/continuar na Fonte de Anya.

CorpseRecoveryModal:

```text
Recuperar
Sair
Feedback de inventory cheio
Feedback de recovery parcial
Feedback de recovery completa
```

AnyaFountainMenu:

```text
Retomar cave / ir para checkpoint portal se aplicavel
Respec se spec 16 estiver implementada
Respec bloqueado/desabilitado se spec 16 nao estiver pronta
Sair
```

---

## 18. Pause / Options UI

Acesso:

```text
Esc sem modal aberto = PauseMenu
```

PauseMenu MVP:

```text
Resume
Save Game
Load Game
Options
Quit to Title futuro/desabilitado se nao existir fluxo
```

Options MVP:

```text
Master Volume
Music Volume
SFX Volume
Toggle Debug HUD
Close
```

---

## 19. Notifications / toasts

Criar/consolidar:

```text
NotificationToastController
NotificationQueue
```

Usos:

```text
Inventory full
Item added
Item removed
Gold changed
Skill unlocked
Skill point gained
Checkpoint unlocked
Boss defeated
Corpse recovered
Craft completed
Shop transaction completed
Not enough gold
Not enough stamina/mana
```

Regras:

- Toast nao bloqueia gameplay.
- Toast nao substitui modal quando decisao/confirmacao for necessaria.
- Toasts devem ser curtos e enfileirados.

---

## 20. Tooltips

Criar/consolidar tooltips simples para:

- item;
- equipment;
- skill node;
- active skill;
- recipe;
- status effect;
- shop item.

Mostrar:

- nome;
- descricao curta;
- custo/valor;
- stats relevantes;
- requisitos;
- locked reason se aplicavel.

---

## 21. Bestiary, Journal e Quests

Bestiary UI:

- Se spec 13 Bestiary runtime estiver pronto, criar painel simples de bestiary.
- Se Bestiary runtime nao estiver pronto, exibir entrada desabilitada/oculta e registrar pendencia.
- Nao criar sistema de bestiary paralelo.

Journal/Quests:

- Fora de escopo nesta spec.
- Nao implementar quest UI ativa.
- Nao reintroduzir QuestManager se ele foi removido/isolar por spec 08.
- Pode existir botao/aba futura desabilitada somente se nao confundir jogador.

---

## 22. Definition of Done

- [ ] UI final runtime existe sobre os sistemas 01-16.
- [ ] DebugHud fica isolado por flag/dev mode.
- [ ] ModalStackManager impede conflitos entre inventory, skills, shop, crafting, cave/death e pause.
- [ ] GameplayInputRouter separa gameplay input de modal input.
- [ ] HUD mostra HP, Stamina, Mana, Hunger, Gold, Dia/Hora, Status, maos Q/E e active slots R/T/Y/G.
- [ ] ContextHint reflete prioridade real de interacao.
- [ ] Inventory abre/fecha com `I`, mostra grid real, navega por WASD e suporta Use/Equip/Drop/Destroy/Split conforme item.
- [ ] Equipment UI mostra slots, durabilidade, broken state e stats derivados.
- [ ] Crafting UI mostra recipes, ingredientes, queue/job/output e inventory full.
- [ ] Shop UI suporta Comprar/Vender/Sair com estoque, preco, gold, quantidade e feedback.
- [ ] DialogueModal suporta opening/closing line e choices sem conflito de modal.
- [ ] SkillTreePanel abre/fecha com `K`, mostra arvores/nodes/passives/equippables/active slots.
- [ ] CheckpointSideMenu lista checkpoints e nao burla gates.
- [ ] DeathScreen e CorpseRecoveryModal refletem regras da spec 15.
- [ ] AnyaFountainMenu mostra respec quando spec 16 estiver pronto.
- [ ] PauseMenu MVP funciona com Resume/Save/Load/Options quando services existirem.
- [ ] Toasts e tooltips funcionam sem bloquear gameplay indevidamente.
- [ ] Journal/quests ativos nao sao implementados.
- [ ] UI usa eventos/bindings reais e nao hardcode de gameplay.
- [ ] Invariantes anti-regressao preservadas.

---

## 23. Validacao

1. Abrir jogo em gameplay normal e validar HUD principal.
2. Alterar HP/Stamina/Mana/Hunger/Gold/Status e validar atualizacao por eventos/bindings.
3. Pressionar `I` e validar InventoryPanel.
4. Navegar inventory com WASD.
5. Usar item consumivel.
6. Equipar item compativel.
7. Dropar item e validar pickup persistente.
8. Destruir item com confirmation dialog.
9. Splitar stack.
10. Abrir Equipment/Character panel e validar slots/stats/durabilidade.
11. Interagir com workstation e validar CraftingPanel.
12. Iniciar craft, cancelar craft, coletar output e validar inventory full.
13. Conversar com NPC e validar DialogueModal/opening/closing line.
14. Abrir loja, comprar, vender e validar estoque/gold/inventory.
15. Pressionar `K` e validar SkillTreePanel.
16. Comprar node, equipar skill em R/T/Y/G e validar HUD.
17. Pressionar R/T/Y/G em gameplay e validar cooldown/custo no HUD.
18. Abrir checkpoint portal e validar CaveCheckpointSideMenu.
19. Forcar boss gate locked e validar feedback.
20. Forcar morte na cave e validar DeathScreen.
21. Interagir com corpse e validar CorpseRecoveryModal parcial/completo.
22. Interagir com Fonte de Anya e validar AnyaFountainMenu/respec se spec 16 pronta.
23. Pressionar Esc sem modal e validar PauseMenu.
24. Abrir Options e validar volumes/debug toggle se suportados.
25. Validar que com modal aberto o player nao anda/ataca/interage.
26. Validar que Q/E dentro do modal nao usam maos do player.
27. Validar que DebugHud aparece apenas com flag dev/debug.
28. Validar que quest/journal ativo nao foi implementado.
29. Validar Unity compile validation e docs validation.
