# SpecKit — FASE9E UI, Hotbar, Inventory e Equipment

> **Feature:** FASE9E_UI_HOTBAR_INVENTORY_EQUIPMENT  
> **Status:** especificação funcional aprovada para planejamento.  
> **Fonte de design:** `docs_old/FASE9E_UI_HOTBAR_INVENTORY_EQUIPMENT_SPEC_v1.0.md`

---

## 1. User story

Como jogador, quero selecionar rapidamente ferramentas, seeds, armas, magia, flechas e consumíveis para plantar, colher, lutar, usar poção e alternar ações sem depender de comandos debug escondidos.

---

## 2. Objetivos funcionais

### O1 — Hotbar de 6 slots

Criar hotbar visível com 6 slots selecionáveis por `1–6`.

### O2 — Mão esquerda e mão direita

Permitir item selecionado na mão esquerda e na mão direita.

- Q usa mão esquerda.
- E interage com mundo; sem interação, usa mão direita.

### O3 — Seed na hotbar

Seed ativa deve ficar na hotbar e ser usada por FarmPlot.

### O4 — Inventário OnGUI

Inventário MVP deve ser OnGUI, navegar com WASD, não pausar o jogo e permitir ações por teclado.

### O5 — Submenu de item

Com item selecionado, E abre submenu:

- Use;
- Drop;
- Split;
- Destroy;
- Cancel.

### O6 — Arco e flecha

Ao equipar arco:

- arco fica no Q;
- flecha fica no E;
- E dispara flecha;
- disparo consome 1 flecha;
- jogador inicia com 30 flechas para teste;
- flecha vai reta, rápida e sem auto-target;
- magia é removida automaticamente.

### O7 — Magia visual MVP

Magia de fogo deve usar projétil visual simples mais lento que a flecha. Primeira magia também viaja em linha reta.

---

## 3. Non-goals

Fora de escopo:

- UI final;
- drag and drop;
- animação de UI;
- suporte a gamepad;
- múltiplas páginas de inventário;
- tooltips ricos;
- UI final de crafting/loja;
- arte final de ícones.

---

## 4. Regras de negócio

### R1 — Hotbar

Hotbar tem 6 slots. Números 1–6 atribuem ou selecionam slots conforme contexto.

### R2 — Inventário

Com inventário aberto:

- WASD move seleção;
- 1–6 atribui item selecionado ao slot correspondente da hotbar;
- E abre submenu;
- I/Esc fecha inventário;
- jogo não pausa.

### R3 — Use

Use aplica item diretamente do inventário se ele for usável. Consumível usado com sucesso remove 1 unidade.

### R4 — Drop

Drop remove item do inventário e cria ItemPickup próximo ao jogador.

### R5 — Split

Split exige stack >= 2 e divide em duas pilhas iguais ou quase iguais.

### R6 — Destroy

Destroy remove permanentemente o item após confirmação simples.

### R7 — Arco

Arco remove magia automaticamente. Arco fica no Q; flecha fica no E. E dispara e consome flecha.

---

## 5. Entidades funcionais

- `HotbarSelectionData`
- `HandSlotType`
- `InventoryMvpOnGui`
- `InventoryItemActionMenu`
- `ActiveSelectionManager` ou expansão do `EquipmentManager`
- `ProjectileController`
- `ItemDropRequest`
- `ItemSplitRequest`
- `ItemDestroyRequest`

---

## 6. Critérios de aceite

### CA1 — HUD

HUD mostra hotbar de 6 slots, mão esquerda, mão direita, seed, weapon, magic, ammo e consumible.

### CA2 — Inventário OnGUI

Tecla I abre/fecha inventário OnGUI sem pausar.

### CA3 — WASD navega

Com inventário aberto, W/A/S/D movem seleção.

### CA4 — Números atribuem hotbar

Com item selecionado no inventário, 1–6 atribui item à hotbar.

### CA5 — E abre submenu

Com item selecionado no inventário, E abre submenu contextual.

### CA6 — Use

Use aplica poção direto do inventário e consome 1 unidade quando bem-sucedido.

### CA7 — Drop

Drop cria ItemPickup próximo ao jogador e remove item do inventário.

### CA8 — Split

Split divide stack >= 2 em duas pilhas iguais ou quase iguais.

### CA9 — Destroy

Destroy remove item permanentemente após confirmação.

### CA10 — Arco no Q e flecha no E

Ao equipar arco, HUD mostra arco na mão esquerda/Q e flecha na mão direita/E.

### CA11 — E dispara flecha

Sem interação contextual em foco, E dispara flecha em linha reta e consome 1 flecha.

### CA12 — Flechas iniciais

Jogador inicia com 30 flechas para teste.

### CA13 — Flecha visual

Flecha aparece como projétil simples rápido, reto e sem auto-target.

### CA14 — Magia visual

Magia de fogo aparece como projétil simples mais lento e reto no primeiro MVP.

### CA15 — Save/load

Save/load preserva hotbar, mãos, seed ativa, weapon, magic, ammo e consumible selecionado.

---

## 7. Dependências

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


