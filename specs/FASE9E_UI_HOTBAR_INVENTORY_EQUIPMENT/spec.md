# SpecKit â€” FASE9E UI, Hotbar, Inventory e Equipment

> **Feature:** FASE9E_UI_HOTBAR_INVENTORY_EQUIPMENT  
> **Status:** especificaÃ§Ã£o funcional aprovada para planejamento.  
> **Fonte de design:** `docs_old/FASE9E_UI_HOTBAR_INVENTORY_EQUIPMENT_SPEC_v1.0.md`

---

## 1. User story

Como jogador, quero selecionar rapidamente ferramentas, seeds, armas, magia, flechas e consumÃ­veis para plantar, colher, lutar, usar poÃ§Ã£o e alternar aÃ§Ãµes sem depender de comandos debug escondidos.

---

## 2. Objetivos funcionais

### O1 â€” Hotbar de 6 slots

Criar hotbar visÃ­vel com 6 slots selecionÃ¡veis por `1â€“6`.

### O2 â€” MÃ£o esquerda e mÃ£o direita

Permitir item selecionado na mÃ£o esquerda e na mÃ£o direita.

- Q usa mÃ£o esquerda.
- E interage com mundo; sem interaÃ§Ã£o, usa mÃ£o direita.

### O3 â€” Seed na hotbar

Seed ativa deve ficar na hotbar e ser usada por FarmPlot.

### O4 â€” InventÃ¡rio OnGUI

InventÃ¡rio MVP deve ser OnGUI, navegar com WASD, nÃ£o pausar o jogo e permitir aÃ§Ãµes por teclado.

### O5 â€” Submenu de item

Com item selecionado, E abre submenu:

- Use;
- Drop;
- Split;
- Destroy;
- Cancel.

### O6 â€” Arco e flecha

Ao equipar arco:

- arco fica no Q;
- flecha fica no E;
- E dispara flecha;
- disparo consome 1 flecha;
- jogador inicia com 30 flechas para teste;
- flecha vai reta, rÃ¡pida e sem auto-target;
- magia Ã© removida automaticamente.

### O7 â€” Magia visual MVP

Magia de fogo deve usar projÃ©til visual simples mais lento que a flecha. Primeira magia tambÃ©m viaja em linha reta.

---

## 3. Non-goals

Fora de escopo:

- UI final;
- drag and drop;
- animaÃ§Ã£o de UI;
- suporte a gamepad;
- mÃºltiplas pÃ¡ginas de inventÃ¡rio;
- tooltips ricos;
- UI final de crafting/loja;
- arte final de Ã­cones.

---

## 4. Regras de negÃ³cio

### R1 â€” Hotbar

Hotbar tem 6 slots. NÃºmeros 1â€“6 atribuem ou selecionam slots conforme contexto.

### R2 â€” InventÃ¡rio

Com inventÃ¡rio aberto:

- WASD move seleÃ§Ã£o;
- 1â€“6 atribui item selecionado ao slot correspondente da hotbar;
- E abre submenu;
- I/Esc fecha inventÃ¡rio;
- jogo nÃ£o pausa.

### R3 â€” Use

Use aplica item diretamente do inventÃ¡rio se ele for usÃ¡vel. ConsumÃ­vel usado com sucesso remove 1 unidade.

### R4 â€” Drop

Drop remove item do inventÃ¡rio e cria ItemPickup prÃ³ximo ao jogador.

### R5 â€” Split

Split exige stack >= 2 e divide em duas pilhas iguais ou quase iguais.

### R6 â€” Destroy

Destroy remove permanentemente o item apÃ³s confirmaÃ§Ã£o simples.

### R7 â€” Arco

Arco remove magia automaticamente. Arco fica no Q; flecha fica no E. E dispara e consome flecha.

---

## 5. Entidades funcionais

- `HotbarSelectionData`
- `HandSlotType`
- `InventoryMvpOnGui`
- `InventoryItemActionMenu`
- `ActiveSelectionManager` ou expansÃ£o do `EquipmentManager`
- `ProjectileController`
- `ItemDropRequest`
- `ItemSplitRequest`
- `ItemDestroyRequest`

---

## 6. CritÃ©rios de aceite

### CA1 â€” HUD

HUD mostra hotbar de 6 slots, mÃ£o esquerda, mÃ£o direita, seed, weapon, magic, ammo e consumible.

### CA2 â€” InventÃ¡rio OnGUI

Tecla I abre/fecha inventÃ¡rio OnGUI sem pausar.

### CA3 â€” WASD navega

Com inventÃ¡rio aberto, W/A/S/D movem seleÃ§Ã£o.

### CA4 â€” NÃºmeros atribuem hotbar

Com item selecionado no inventÃ¡rio, 1â€“6 atribui item Ã  hotbar.

### CA5 â€” E abre submenu

Com item selecionado no inventÃ¡rio, E abre submenu contextual.

### CA6 â€” Use

Use aplica poÃ§Ã£o direto do inventÃ¡rio e consome 1 unidade quando bem-sucedido.

### CA7 â€” Drop

Drop cria ItemPickup prÃ³ximo ao jogador e remove item do inventÃ¡rio.

### CA8 â€” Split

Split divide stack >= 2 em duas pilhas iguais ou quase iguais.

### CA9 â€” Destroy

Destroy remove item permanentemente apÃ³s confirmaÃ§Ã£o.

### CA10 â€” Arco no Q e flecha no E

Ao equipar arco, HUD mostra arco na mÃ£o esquerda/Q e flecha na mÃ£o direita/E.

### CA11 â€” E dispara flecha

Sem interaÃ§Ã£o contextual em foco, E dispara flecha em linha reta e consome 1 flecha.

### CA12 â€” Flechas iniciais

Jogador inicia com 30 flechas para teste.

### CA13 â€” Flecha visual

Flecha aparece como projÃ©til simples rÃ¡pido, reto e sem auto-target.

### CA14 â€” Magia visual

Magia de fogo aparece como projÃ©til simples mais lento e reto no primeiro MVP.

### CA15 â€” Save/load

Save/load preserva hotbar, mÃ£os, seed ativa, weapon, magic, ammo e consumible selecionado.

---

## 7. DependÃªncias

- `InventoryManager`
- `ItemPickup`
- `EquipmentManager`
- `ItemUseManager`
- `PlayerCombatController`
- `ProjectileController`
- `InteractionSystem`
- `DebugHud`
- `SaveManager`
- `GameBootstrap`

---

## 8. Pronto para Plan quando

- Esta spec estiver aprovada.
- `docs_old/FASE9E_UI_HOTBAR_INVENTORY_EQUIPMENT_SPEC_v1.0.md` estiver lido.
- Estado real em `dev` tiver sido validado.

