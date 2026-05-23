# REF FUTURO — FASE9E UI hotbar inventory equipment final

> Origem histórica: `docs_old/FASE9E_UI_HOTBAR_INVENTORY_EQUIPMENT_SPEC_v1.0.md`
> Status: Refinamento futuro preservado
> Spec futura relacionada: $Spec

---

## Decisões preservadas

Conteúdo histórico preservado abaixo para evitar perda operacional de decisões, escopo e pendências.

---

# FASE 9E — UI, Hotbar, Inventory e Equipment Spec v1.0

> **Status:** spec aprovada para orientar próximas waves.  
> **Feature:** FASE9E_UI_HOTBAR_INVENTORY_EQUIPMENT  
> **Base:** FASE9C Tools/Farm/Combat + Player Equipment/Items/Combat Loadout.  
> **Objetivo:** definir como o jogador seleciona ferramentas, seeds, armas, magia, flechas e consumíveis antes da UI final.

---

## 1. Problema

O projeto passa a ter ferramentas equipáveis, seed ativa, espada, arco, magia, flechas e poções. Sem uma regra clara de UI/hotbar, cada sistema poderia resolver seleção de item de forma diferente, criando drift.

Esta spec define uma UI MVP funcional, simples e testável, antes da UI final.

---

## 2. User story

Como jogador, quero selecionar rapidamente ferramenta, seed, arma, magia e consumível para plantar, colher, lutar, usar poção e alternar ações sem depender de comandos debug escondidos.

---

## 3. Decisões fechadas

| Pergunta | Decisão |
|---|---|
| Hotbar | 6 slots |
| Seed ativa | fica na hotbar |
| Ferramentas | selecionadas por número, sem Q/R para ciclo |
| Consumível | único no MVP |
| Inventário MVP | OnGUI |
| Inventário aberto | não pausa o jogo |
| Navegação no inventário | WASD move seleção |
| Hotbar pelo inventário | números 1–6 atribuem item selecionado |
| Submenu do inventário | E abre Use, Drop, Split, Destroy, Cancel |
| Drop | larga item no chão como ItemPickup |
| Split | divide stack em duas pilhas iguais/quase iguais |
| Destroy | remove item permanentemente com confirmação |
| Equipar arco | remove magia automaticamente |
| Mãos | Q usa mão esquerda; E usa mão direita ou interage |
| Arco nas mãos | arco fica no Q; flecha fica no E |
| Disparo de arco | E dispara e consome flecha |
| Flechas iniciais | jogador inicia com 30 flechas para teste |
| Flecha | projétil rápido em linha reta, sem auto-target |
| Magia de fogo | projétil mais lento; primeiro MVP também em linha reta |

---

## 4. Objetivos funcionais

### O1 — Hotbar principal

Criar hotbar visível com 6 slots selecionáveis por números `1–6`.

### O2 — Modelo de mãos

Permitir item selecionado na mão esquerda e na mão direita.

- `Q` usa mão esquerda.
- `E` interage com mundo; se não houver interação contextual, usa mão direita.

### O3 — Seed ativa na hotbar

Plantio deve usar a seed selecionada na hotbar. `FarmPlot` não deve escolher seed automaticamente por ordem fixa.

### O4 — Equipar arma/magia/consumível

Jogador deve conseguir equipar espada, arco, magia, flecha e poção.

### O5 — Arco e flecha

Ao equipar arco:

- arco fica na mão esquerda/Q;
- flecha fica na mão direita/E;
- E dispara flecha em linha reta;
- cada disparo consome 1 flecha;
- jogador inicia com 30 flechas para teste;
- magia equipada é removida automaticamente.

### O6 — Projéteis visuais MVP

Antes da arte final:

- flecha aparece como ponto/linha pequena rápida;
- magia de fogo aparece como ponto/bolinha mais lenta;
- flecha e primeira magia viajam em linha reta;
- flecha não tem auto-target;
- magia futura pode ganhar comportamento diferente, mas a primeira é reta.

### O7 — Usar consumível

Jogador deve conseguir usar consumível único selecionado, como poção.

### O8 — Inventário navegável por teclado

Com inventário aberto:

- WASD move seleção;
- números `1–6` atribuem item selecionado à hotbar;
- E abre submenu do item selecionado;
- inventário não pausa o jogo.

### O9 — Submenu de item

Submenu mínimo:

```text
Use
Drop
Split
Destroy
Cancel
```

### O10 — Feedback claro

UI deve mostrar mensagens para falhas: sem seed, sem ferramenta, sem flecha, arco bloqueia magia, item inválido, mão vazia, split impossível etc.

---

## 5. Non-goals

Fora de escopo:

- drag and drop final;
- animação de UI;
- arte final de ícones;
- layout definitivo;
- suporte a controle/gamepad;
- múltiplas páginas de inventário;
- tooltips ricos;
- comparação avançada de equipamento;
- UI final de crafting;
- UI final de loja.

---

## 6. Layout HUD MVP

```text
HP: 90/100   Hunger: 72/100   Gold: 120

Left Hand: Axe Basic        Right Hand: Wheat Seed x8
Weapon: Sword Basic         Magic: Fire Spark
Ammo: Arrow x12             Consumable: Small Potion x3

Hotbar: [1] Hoe  [2] Axe  [3] Sickle  [4] Wheat Seed  [5] Fire Spark  [6] Potion
Q: use left hand            E: use right hand / interact
```

---

## 7. Hotbar MVP

A hotbar tem 6 slots.

| Slot | Uso esperado |
|---:|---|
| 1 | ferramenta, exemplo Hoe |
| 2 | ferramenta, exemplo Axe |
| 3 | ferramenta, exemplo Sickle |
| 4 | seed ativa, exemplo Wheat Seed |
| 5 | magia ou item especial |
| 6 | consumível único, exemplo Small Potion |

---

## 8. Modelo de mãos

| Mão | Input | Uso |
|---|---|---|
| Mão esquerda | Q | usa item/ferramenta da mão esquerda |
| Mão direita | E | usa item/ferramenta da mão direita ou interage |

Regras:

- Números `1–6` selecionam hotbar.
- Item da hotbar pode ser atribuído à mão esquerda ou direita.
- `Q` sempre tenta usar mão esquerda.
- `E` prioriza interação de mundo quando houver interagível em foco.
- Sem interagível, `E` usa mão direita.

### 8.1 Regra especial: arco e flecha

Ao equipar arco:

- arco fica associado à mão esquerda/Q;
- flecha fica associada à mão direita/E;
- disparo acontece com E;
- cada disparo consome 1 flecha;
- se não houver flecha, não dispara e mostra feedback;
- direção usa facing/última direção do jogador;
- flecha vai reta e não persegue alvo.

---

## 9. Input MVP

| Input | Ação |
|---|---|
| 1–6 | selecionar hotbar slot |
| Q | usar mão esquerda |
| E | interagir; se não houver interação, usar mão direita; com arco, disparar flecha |
| J | ataque primário |
| K | magia equipada, se existir e não estiver bloqueada |
| H | usar consumível selecionado, se mantido como atalho temporário |
| Tab | avançar dia, enquanto for debug MVP |
| I | abrir/fechar inventário MVP |

### 9.1 Inputs com inventário aberto

| Input | Ação |
|---|---|
| W | mover seleção para cima |
| A | mover seleção para esquerda |
| S | mover seleção para baixo |
| D | mover seleção para direita |
| 1–6 | atribuir item selecionado ao slot correspondente da hotbar |
| E | abrir submenu do item selecionado / confirmar ação |
| Esc ou I | fechar inventário |

---

## 10. Inventory MVP

Inventário MVP será OnGUI, sem pausa.

Motivo:

- valida fluxo rapidamente;
- reduz dependência de arte final;
- permite testar equipar/desequipar/usar antes da UI definitiva.

Cada item deve mostrar:

- nome;
- quantidade;
- categoria;
- se é equipável;
- ação disponível.

Exemplo:

```text
Inventory
> Sword Basic x1       [E: Menu] [1-6: Hotbar]
  Bow Basic x1         [E: Menu] [1-6: Hotbar]
  Fire Spark x1        [E: Menu] [1-6: Hotbar]
  Arrow Basic x30      [E: Menu] [1-6: Hotbar]
  Small Potion x3      [E: Menu] [1-6: Hotbar]
  Wheat Seed x8        [E: Menu] [1-6: Hotbar]
  Axe Basic x1         [E: Menu] [1-6: Hotbar]
```

---

## 11. Submenu de item

Ao selecionar item e pressionar E:

```text
Use
Drop
Split
Destroy
Cancel
```

| Ação | Regra |
|---|---|
| Use | usa item diretamente do inventário, se for usável |
| Drop | larga item no chão próximo ao player |
| Split | divide stack em duas pilhas iguais ou quase iguais |
| Destroy | remove item permanentemente, com confirmação simples |
| Cancel | fecha submenu |

### 11.1 Use

- Se item for poção/consumível, aplica efeito diretamente.
- Se item não for usável, mostrar `Item cannot be used.`
- Se uso for bem-sucedido e item for consumível, remover 1 unidade.
- Usar item pelo inventário não exige equipar na hotbar.

### 11.2 Drop

- Item dropado vira `ItemPickup` no chão perto do player.
- Drop remove quantidade do inventário.
- Para stack, MVP pode dropar a pilha inteira, salvo se jogador tiver feito Split antes.
- Item dropado deve respeitar save/load de pickups persistentes já existente.

### 11.3 Split

- Split só aparece habilitado quando stack amount >= 2.
- Divide em duas pilhas iguais quando par.
- Quando ímpar, uma pilha fica com `floor(amount / 2)` e a outra com `ceil(amount / 2)`.
- Exemplo: 5 flechas → pilhas 2 e 3.
- Se o inventário ainda for por ID agregado, registrar pendência para inventário por stacks reais.

### 11.4 Destroy

- Destroy remove item permanentemente.
- Deve pedir confirmação simples:

```text
Destroy this item? Yes / No
```

- Destroy não cria pickup no mundo.

---

## 12. Regras de equip via UI

### R1 — Equipar tool

Valida item no inventário, valida ToolDataSO, equipa no slot de ferramenta/mão e atualiza HUD.

### R2 — Selecionar seed

Seed fica na hotbar. Quando selecionada/usada, define `ActiveSeedId`. FarmPlot usa essa seed.

### R3 — Equipar espada

`PrimaryWeapon = Sword`. Magic pode continuar equipado.

### R4 — Equipar arco

`PrimaryWeapon = Bow`; Magic é removido automaticamente; arco vai para Q; flecha vai para E; E dispara e consome flecha.

### R5 — Equipar magia

Se PrimaryWeapon for Bow, falha. Se PrimaryWeapon for Sword/Unarmed/vazio, equipa.

### R6 — Equipar consumível

Coloca consumível no slot único de quick item. Não consome item ao equipar.

### R7 — Usar consumível

Valida quantidade, aplica efeito, remove 1 unidade se uso foi bem-sucedido.

---

## 13. Feedback e mensagens

| Caso | Mensagem |
|---|---|
| Sem seed ativa | `No seed selected.` |
| Seed sem quantidade | `You do not have this seed.` |
| Sem tool correta | `Wrong tool.` |
| Fallback usado | `Improvised action used.` |
| Arco sem flecha | `No arrows equipped or available.` |
| Arco remove magia | `Bow equipped. Magic slot disabled.` |
| Magia bloqueada por arco | `Bow blocks magic slot in this version.` |
| Flecha disparada | `Arrow fired.` |
| Magia disparada | `Fire Spark cast.` |
| Poção usada | `Used Small Potion.` |
| HP cheio | `HP already full.` |
| Item não equipável | `Item cannot be equipped.` |
| Item não usável | `Item cannot be used.` |
| Item dropado | `Item dropped.` |
| Stack splitado | `Stack split.` |
| Split impossível | `Cannot split this stack.` |
| Destroy confirmado | `Item destroyed.` |
| Destroy cancelado | `Destroy cancelled.` |
| Slot limpo | `Unequipped item.` |
| Mão vazia | `No item in this hand.` |
| Interação priorizada | `Interacting.` |

---

## 14. Critérios de aceite

- HUD mostra hotbar de 6 slots, mão esquerda, mão direita, seed, weapon, magic, ammo e consumible.
- Tecla I abre/fecha inventário OnGUI sem pausar.
- Inventário navega com WASD.
- Com item selecionado no inventário, números 1–6 atribuem o item ao slot correspondente da hotbar.
- Com item selecionado no inventário, E abre submenu contextual.
- Submenu oferece Use, Drop, Split, Destroy e Cancel.
- Use aplica item usável direto do inventário.
- Drop remove item do inventário e cria ItemPickup próximo ao player.
- Split divide stack >= 2 em duas pilhas iguais/quase iguais.
- Destroy remove item permanentemente após confirmação simples.
- Jogador equipa Axe/Hoe/Sickle pelo inventário.
- Jogador coloca seed na hotbar e define active seed.
- FarmPlot usa active seed.
- Jogador equipa espada.
- Jogador equipa magia junto da espada.
- Jogador equipa arco e magic slot é removido automaticamente.
- Ao equipar arco, arco fica no Q e flecha no E.
- Pressionar E dispara flecha quando não houver interação contextual.
- Flecha consome 1 ammo.
- Jogador inicia com 30 flechas para teste.
- Flecha é rápida, reta e sem auto-target.
- Magia de fogo é mais lenta e inicialmente também reta.
- Jogador usa poção selecionada e HP aumenta.
- Ações inválidas geram feedback claro.
- Save/load preserva hotbar, mãos, seed ativa, weapon, magic, ammo e consumible selecionado.

---

## 15. Tasks sugeridas

- PR-133 — UI/selection contracts.
- PR-134 — Loadout HUD MVP.
- PR-135 — Inventory MVP OnGUI.
- PR-135.1 — Inventory item actions MVP.
- PR-136 — Active seed + hand use integration.
- PR-136.1 — Bow/Arrow hand mapping MVP.
- PR-136.2 — Projectile visual MVP.
- PR-137 — Consumable UI integration.
- PR-138 — Save/load UI selection.
- PR-139 — UI validator/handoff.

---

## 16. Decisão final

Esta feature deve ser implementada antes da UI final. Ela define o contrato mínimo de seleção ativa para ferramentas, seeds, arco, flechas, magia, consumíveis e inventário OnGUI, reduzindo drift entre Farm, Combat, Equipment, Inventory e ItemPickup.


## Itens que devem virar implementação

- [ ] Refinar em spec executável antes de código, quando aplicável.
- [ ] Validar dependências contra specs implementadas atuais.

## Fora de escopo / cuidado

- Não tratar este refinement como autorização automática de implementação.
- Não sobrescrever specs implementadas sem amendment/correction explícito.





