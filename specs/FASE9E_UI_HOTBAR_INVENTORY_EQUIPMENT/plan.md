# Plan — FASE9E UI, Hotbar, Inventory e Equipment

> **Feature:** FASE9E_UI_HOTBAR_INVENTORY_EQUIPMENT  
> **Spec:** `spec.md`

---

## 1. Estratégia

Implementar UI MVP antes de UI final, priorizando fluxo jogável e integração entre Inventory, Equipment, Farm, Combat e ItemPickup.

Ordem:

1. contratos de seleção/hotbar/mãos;
2. HUD/loadout;
3. inventário OnGUI navegável;
4. ações de item no inventário;
5. active seed e uso das mãos;
6. arco/flecha no modelo de mãos;
7. projéteis visuais MVP;
8. consumíveis;
9. save/load;
10. validator/handoff.

---

## 2. Arquitetura proposta

### Novas pastas

```text
Assets/_Game/Scripts/UI/InventoryMvp/
Assets/_Game/Scripts/UI/Hotbar/
Assets/_Game/Scripts/Items/Actions/
Assets/_Game/Scripts/Combat/Projectiles/
```

### Novos componentes/contratos

- `HandSlotType`
- `HotbarSelectionData`
- `ActiveSelectionManager`
- `InventoryMvpOnGui`
- `InventoryItemActionMenu`
- `InventoryItemActionType`
- `ItemDropRequest`
- `ItemSplitRequest`
- `ItemDestroyRequest`

---

## 3. Integrações

### InventoryManager

Precisa suportar ações:

- usar item;
- remover item para drop;
- remover item para destroy;
- split lógico de stack.

Observação: se inventário atual ainda for agregado por ID, split deve registrar limitação e criar estrutura mínima para stacks reais ou pendência explícita.

### ItemPickup

Drop deve criar pickup persistente perto do jogador.

### EquipmentManager

Hotbar e mãos devem conversar com equipamento ativo.

### InteractionSystem

E prioriza interação contextual. Sem interagível, E usa mão direita.

### PlayerCombatController

Arco/flecha e magia usam projectile visual MVP.

### SaveManager

Salvar/restaurar hotbar, mão esquerda, mão direita, seed ativa e consumível selecionado.

---

## 4. Riscos

| Risco | Mitigação |
|---|---|
| Inventário atual agregado por ID dificultar split | implementar split simples ou registrar limitação para stack model |
| E conflitar com interação | InteractionSystem tem prioridade sobre mão direita |
| OnGUI virar UI final acidental | documentar como MVP temporário |
| Drop duplicar itens | remover do inventário antes de criar pickup e validar resultado |
| Projectile sem sprite | usar ponto/linha placeholder |

---

## 5. Testes manuais mínimos

- Abrir inventário com I.
- Navegar com WASD.
- Enviar item para hotbar com 1–6.
- Abrir submenu com E.
- Usar poção direto do inventário.
- Dropar item e pegar novamente.
- Splitar stack par e ímpar.
- Destruir item com confirmação.
- Equipar arco.
- Confirmar arco no Q e flecha no E.
- Disparar flecha com E.
- Confirmar consumo de flecha.
- Confirmar flecha reta e rápida.
- Usar magia de fogo reta e mais lenta.
- Salvar/carregar hotbar/mãos.

---

## 6. Fora de escopo técnico

- Canvas final;
- drag and drop;
- ícones finais;
- animações de UI;
- gamepad;
- múltiplas páginas;
- UI final de shop/crafting.
