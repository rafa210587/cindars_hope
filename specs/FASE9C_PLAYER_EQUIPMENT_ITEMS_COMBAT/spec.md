# SpecKit — FASE9C Player Equipment, Item Use e Combat Loadout

> **Feature:** FASE9C_PLAYER_EQUIPMENT_ITEMS_COMBAT  
> **Status:** especificação funcional aprovada para planejamento.  
> **Fonte de design:** `docs/FASE9C_PLAYER_EQUIPMENT_ITEMS_COMBAT_SPEC_v1.0.md`  
> **Branch alvo:** `dev`

---

## 1. User story

Como jogador de Cindar's Hope, quero equipar e desequipar armas, magia, ferramentas, munição e consumíveis para alternar meu estilo de combate e sobrevivência sem depender apenas do soco básico.

---

## 2. Objetivos funcionais

### O1 — Equipar e desequipar itens

O jogador deve conseguir equipar e desequipar:

- arma primária;
- magia;
- munição;
- ferramenta;
- consumível em hotbar.

### O2 — Loadouts válidos

O MVP deve aceitar:

- espada + magia;
- arco + flechas;
- ferramenta ativa independente;
- consumível em hotbar independente.

O MVP deve bloquear:

- arco + magia;
- espada + arco simultaneamente.

### O3 — Espada

A espada básica deve causar mais dano que o soco.

### O4 — Arco e flecha

O arco deve atacar à distância com projectile e consumir flecha quando configurado com `AmmoCost > 0`.

### O5 — Magia de fogo

A magia de fogo deve lançar projectile e aplicar Burn por 3 segundos.

### O6 — Poção

A poção pequena deve poder ser equipada na hotbar e usada para restaurar HP, consumindo 1 item.

### O7 — Persistência

Save/load deve preservar o equipamento ativo e limpar slots inválidos de forma segura.

---

## 3. Non-goals

Fora de escopo desta feature:

- UI final de inventário;
- drag and drop;
- árvore de magia;
- mana system completo;
- balanceamento final;
- animações finais;
- pooling avançado de projectile;
- durabilidade de equipamento;
- múltiplos loadouts salvos.

---

## 4. Regras de negócio

### R1 — Espada + magia

Se `PrimaryWeapon` for melee, o slot `Magic` pode ficar equipado.

### R2 — Arco bloqueia magia

Se o jogador equipa arco:

- `PrimaryWeapon = Bow`;
- `Magic` deve ser limpo automaticamente.

### R3 — Magia bloqueada com arco

Se arco já está equipado, tentar equipar magia deve falhar com feedback.

### R4 — Equipar não consome item

Equipar arma, magia, munição, ferramenta ou consumível não remove item do inventário.

### R5 — Usar consumível consome item

Usar poção consome 1 unidade se o uso foi bem-sucedido.

### R6 — Flecha consome ammo

Disparar arco consome `AmmoCost` do `RequiredAmmoItemId`.

### R7 — Burn

Burn aplicado pela magia de fogo deve:

- durar 3 segundos;
- causar dano contínuo;
- no MVP: 1 dano por segundo;
- renovar duração se aplicado novamente;
- não stackar poder ainda.

---

## 5. Entidades funcionais

### EquipmentSlotType

Slots mínimos:

- `PrimaryWeapon`
- `Magic`
- `Ammo`
- `Tool`
- `ConsumableHotbar`

### WeaponType

Tipos mínimos:

- `Unarmed`
- `Melee`
- `RangedPhysical`
- `RangedMagic`

### Itens iniciais

| Item | Tipo | Regra |
|---|---|---|
| `item_weapon_sword_basic` | Weapon/Melee | dano maior que soco |
| `item_weapon_bow_basic` | Weapon/RangedPhysical | projectile + ammo |
| `item_ammo_arrow_basic` | Ammo | consumida pelo arco |
| `item_magic_fire_spark` | Magic/RangedMagic | projectile + Burn |
| `item_potion_small_hp` | Consumable | restaura HP |

---

## 6. Critérios de aceite

### CA1 — Equipar espada

Dado que o jogador possui `item_weapon_sword_basic`, quando equipa a espada, então `PrimaryWeapon` passa a ser `weapon_sword_basic` e o HUD debug mostra a espada.

### CA2 — Desequipar espada

Dado que a espada está equipada, quando o jogador desequipa `PrimaryWeapon`, então o slot fica vazio e o item permanece no inventário.

### CA3 — Espada causa mais dano que soco

Dado um inimigo com HP suficiente, quando o jogador ataca com espada, então o dano aplicado é maior que o dano do ataque desarmado.

### CA4 — Equipar arco remove magia

Dado que magia está equipada, quando o jogador equipa arco, então `Magic` fica vazio e `PrimaryWeapon` passa a ser arco.

### CA5 — Arco dispara projectile

Dado que arco e flecha estão equipados, quando o jogador ataca, então um projectile físico é criado e se move na direção do ataque.

### CA6 — Arco consome flecha

Dado que o arco tem `AmmoCost = 1`, quando o jogador dispara, então o inventário perde 1 flecha.

### CA7 — Arco sem flecha falha

Dado que arco está equipado e não há flecha, quando o jogador ataca, então nenhum projectile é criado e o HUD/log mostra feedback.

### CA8 — Magia bloqueada com arco

Dado que arco está equipado, quando o jogador tenta equipar magia, então a ação falha e o slot `Magic` continua vazio.

### CA9 — Espada + magia permitido

Dado que espada está equipada, quando o jogador equipa magia de fogo, então `PrimaryWeapon` continua espada e `Magic` passa a ser magia de fogo.

### CA10 — Magia aplica Burn

Dado que magia de fogo acerta um inimigo, então o inimigo recebe dano inicial e status Burn por 3 segundos.

### CA11 — Burn causa dano contínuo

Dado que inimigo está com Burn, então ele recebe dano contínuo durante 3 segundos.

### CA12 — Poção restaura HP

Dado que poção está na hotbar e o jogador perdeu HP, quando usa a poção, então HP aumenta e o inventário perde 1 poção.

### CA13 — Save/load preserva equipamento

Dado que jogador salva com espada, magia e poção equipada, quando carrega o save, então os slots retornam ao mesmo estado se os itens ainda existem no inventário.

### CA14 — Save/load limpa slot inválido

Dado um save com item equipado inexistente ou removido do inventário, quando carrega, então o slot é limpo com warning e o jogo continua.

---

## 7. Dependências

- `InventoryManager`
- `PlayerManager`
- `SaveManager`
- `DebugHud`
- `GameBootstrap`
- `DamageRequest`
- `EnemyHealth`
- futuro `EnemyStatusReceiver`
- `GameEventBus`

---

## 8. Observabilidade MVP

Debug HUD deve mostrar:

```text
Weapon: Sword Basic
Magic: Fire Spark
Ammo: Arrow x12
Tool: Axe Basic
Consumable: Small Potion x3
```

Logs esperados:

```text
Equipped Sword Basic
Unequipped Fire Spark
Cannot equip magic while Bow is equipped
No arrows available
Used Small Potion +10 HP
Burn applied for 3s
```

---

## 9. Pronto para Plan quando

- Esta spec estiver aprovada.
- `docs/FASE9C_PLAYER_EQUIPMENT_ITEMS_COMBAT_SPEC_v1.0.md` estiver lida.
- `docs/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.1_FASE9C_DELTA.md` estiver lido.
- Estado real do repo estiver validado na branch `dev`.
