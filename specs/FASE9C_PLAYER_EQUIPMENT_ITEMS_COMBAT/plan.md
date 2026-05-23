# Plan — FASE9C Player Equipment, Item Use e Combat Loadout

> **Feature:** FASE9C_PLAYER_EQUIPMENT_ITEMS_COMBAT  
> **Spec:** `specs/FASE9C_PLAYER_EQUIPMENT_ITEMS_COMBAT/spec.md`

---

## 1. Estratégia

Implementar em PRs pequenos e verificáveis.

Ordem:

1. contratos;
2. dados/assets;
3. manager;
4. save/load;
5. combate melee/bow;
6. magia/status;
7. consumíveis;
8. debug selection;
9. validator/handoff.

Não implementar UI final nesta fase.

---

## 2. Arquitetura proposta

### Novas pastas

```text
Assets/_Game/Scripts/Equipment/
Assets/_Game/Scripts/Combat/Weapons/
Assets/_Game/Scripts/Combat/Projectiles/
Assets/_Game/Scripts/Items/Consumables/
Assets/_Game/Data/Weapons/
Assets/_Game/Data/Equipment/
Assets/_Game/Data/Consumables/
Assets/_Game/Data/Projectiles/
```

### Novos contratos

- `EquipmentSlotType`
- `EquipmentSaveData`
- `WeaponType`
- `WeaponDataSO`
- `WeaponDatabaseSO`
- `ConsumableDataSO`
- `ConsumableDatabaseSO`
- `StatusApplicationData`
- `ProjectileRuntimeData`

### Novos managers/controllers

- `EquipmentManager`
- `PlayerCombatController`
- `ProjectileController`
- `ItemUseManager`

### Integrações

- `GameBootstrap` recebe `EquipmentManager` e `ItemUseManager`.
- `SaveData` recebe `EquipmentSaveData`.
- `SaveManager` captura/restaura equipamento.
- `DebugHud` mostra loadout.
- `InventoryManager` continua sendo fonte dos itens possuídos.

---

## 3. Fases técnicas

### Fase A — Contratos

Criar tipos e eventos sem alterar gameplay.

Aceite:

- compila;
- sem cena alterada;
- sem alteração de comportamento.

### Fase B — Assets iniciais

Criar dados para:

- espada;
- arco;
- flecha;
- magia de fogo;
- poção pequena.

Aceite:

- databases validam IDs;
- itens existem no ItemDatabase;
- dados estão em ScriptableObject.

### Fase C — EquipmentManager

Implementar equip/unequip e loadout rules.

Aceite:

- espada + magia permitido;
- arco remove/bloqueia magia;
- equip não consome item;
- unequip preserva item no inventário.

### Fase D — Save/load

Persistir slots.

Aceite:

- save/load restaura loadout;
- slot inválido é limpo;
- saves antigos carregam.

### Fase E — PlayerCombatController

Substituir gradualmente ataque hardcoded.

Aceite:

- Unarmed mantém comportamento atual;
- sword causa dano maior;
- bow dispara projectile e consome ammo.

### Fase F — Magia e Burn

Implementar projectile mágico e status Burn.

Aceite:

- Fire Spark acerta inimigo;
- aplica Burn por 3s;
- Burn causa dano contínuo.

### Fase G — Consumíveis

Implementar poção.

Aceite:

- poção restaura HP;
- remove 1 item;
- falha se não houver item.

### Fase H — Debug selection e validator

Criar forma mínima de equipar/usar sem UI final.

Aceite:

- debug hotkeys ou debug points permitem validar;
- validator cobre dados obrigatórios.

---

## 4. Riscos

| Risco | Mitigação |
|---|---|
| Misturar UI final cedo demais | usar DebugHud/hotkeys temporárias |
| Quebrar ataque atual | manter fallback Unarmed equivalente ao soco |
| Save quebrar saves antigos | EquipmentSaveData opcional e tolerante a null |
| Burn exigir sistema de status grande | MVP simples em EnemyStatusReceiver |
| Arco sem direção/facing robusto | usar última direção de movimento ou direção do player controller |

---

## 5. Testes manuais mínimos

1. Equipar espada.
2. Atacar Slime com espada.
3. Desequipar espada.
4. Equipar magia com espada.
5. Equipar arco e confirmar magia removida.
6. Disparar arco com flecha.
7. Disparar arco sem flecha.
8. Equipar espada + magia.
9. Lançar Fire Spark.
10. Confirmar Burn por 3s.
11. Usar poção.
12. Salvar/carregar e confirmar loadout.
13. Console sem erro vermelho.

---

## 6. Fora de escopo técnico

- UI final;
- animações;
- som;
- pooling;
- mana completo;
- árvore de magia;
- durabilidade;
- balanceamento final.

