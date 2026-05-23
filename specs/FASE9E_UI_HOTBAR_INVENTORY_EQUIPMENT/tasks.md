# Tasks — FASE9E UI, Hotbar, Inventory e Equipment

> **Feature:** FASE9E_UI_HOTBAR_INVENTORY_EQUIPMENT  
> **Spec:** `spec.md`  
> **Plan:** `plan.md`

---

## PR-133 — UI/selection contracts

### Escopo

Criar contratos para hotbar, mãos, active seed, quick item e mensagens de UI.

### Arquivos esperados

- `HandSlotType.cs`
- `HotbarSelectionData.cs`
- `InventoryItemActionType.cs`
- eventos de hotbar/hand/selection quando necessário

### Critérios

- [ ] Compila.
- [ ] Nenhum gameplay alterado.
- [ ] Nenhuma cena alterada.

---

## PR-134 — Loadout HUD MVP

### Escopo

Mostrar estado do loadout no HUD/debug HUD.

### Critérios

- [ ] HUD mostra 6 slots de hotbar.
- [ ] HUD mostra mão esquerda.
- [ ] HUD mostra mão direita.
- [ ] HUD mostra weapon/magic/ammo/consumable.

---

## PR-135 — Inventory MVP OnGUI

### Escopo

Criar inventário OnGUI navegável.

### Critérios

- [ ] I abre/fecha inventário.
- [ ] Inventário não pausa o jogo.
- [ ] WASD move seleção.
- [ ] 1–6 atribui item selecionado à hotbar.
- [ ] E abre submenu do item.

---

## PR-135.1 — Inventory item actions MVP

### Escopo

Implementar submenu de item.

### Critérios

- [ ] Use aplica item usável direto do inventário.
- [ ] Drop cria ItemPickup próximo ao player.
- [ ] Drop remove item do inventário.
- [ ] Split divide stack >= 2 em duas pilhas iguais/quase iguais.
- [ ] Destroy pede confirmação simples.
- [ ] Destroy remove item permanentemente sem criar pickup.

---

## PR-136 — Active seed + hand use integration

### Escopo

Integrar hotbar/mãos com FarmPlot e InteractionSystem.

### Critérios

- [ ] Seed na hotbar define active seed.
- [ ] FarmPlot usa active seed.
- [ ] Q usa mão esquerda.
- [ ] E prioriza interação contextual.
- [ ] Sem interação, E usa mão direita.

---

## PR-136.1 — Bow/Arrow hand mapping MVP

### Escopo

Mapear arco e flecha para o modelo de mãos.

### Critérios

- [ ] Ao equipar arco, magia é removida.
- [ ] Arco fica na mão esquerda/Q.
- [ ] Flecha fica na mão direita/E.
- [ ] Jogador inicia com 30 flechas para teste.
- [ ] E dispara flecha quando não houver interação contextual.
- [ ] Cada disparo consome 1 flecha.

---

## PR-136.2 — Projectile visual MVP

### Escopo

Criar projéteis visuais simples para flecha e magia.

### Critérios

- [ ] Flecha aparece como ponto/linha pequena rápida.
- [ ] Flecha viaja reta.
- [ ] Flecha não tem auto-target.
- [ ] Magia de fogo aparece como ponto/bolinha mais lenta.
- [ ] Primeira magia viaja reta.
- [ ] Ambos aplicam dano se atingirem inimigo.

---

## PR-137 — Consumable UI integration

### Escopo

Integrar consumível único com hotbar e inventário.

### Critérios

- [ ] Poção pode ir para hotbar.
- [ ] Poção pode ser usada direto do inventário.
- [ ] Poção pode ser usada como consumível selecionado.
- [ ] HP aumenta.
- [ ] Inventário remove 1 poção.

---

## PR-138 — Save/load UI selection

### Escopo

Persistir seleção ativa.

### Critérios

- [ ] Save guarda hotbar.
- [ ] Save guarda mão esquerda.
- [ ] Save guarda mão direita.
- [ ] Save guarda active seed.
- [ ] Save guarda consumível selecionado.
- [ ] Load restaura seleção.
- [ ] IDs inválidos limpam slots com warning.

---

## PR-139 — UI validator/handoff

### Escopo

Validar dados e documentar entrega.

### Critérios

- [ ] Validator detecta hotbar com ID inválido.
- [ ] Validator detecta consumível inexistente.
- [ ] Validator detecta ammo inexistente.
- [ ] Handoff contém smoke test.

---

## Smoke test final

- [ ] Abrir inventário com I.
- [ ] Navegar com WASD.
- [ ] Mandar item para hotbar com 1–6.
- [ ] Abrir submenu com E.
- [ ] Usar poção direto do inventário.
- [ ] Dropar item e pegar novamente.
- [ ] Splitar stack par e ímpar.
- [ ] Destruir item com confirmação.
- [ ] Equipar arco.
- [ ] Confirmar arco no Q e flecha no E.
- [ ] Disparar flecha com E.
- [ ] Confirmar ammo consumida.
- [ ] Confirmar flecha reta e rápida.
- [ ] Usar magia de fogo reta e lenta.
- [ ] Salvar/carregar hotbar/mãos.
- [ ] Console sem erro vermelho.

