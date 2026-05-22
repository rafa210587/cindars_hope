# SpecKit â€” FASE9C Player Equipment, Item Use e Combat Loadout

> **Feature:** FASE9C_PLAYER_EQUIPMENT_ITEMS_COMBAT  
> **Status:** especificaÃ§Ã£o funcional aprovada para planejamento.  
> **Fonte de design:** `docs_old/FASE9C_PLAYER_EQUIPMENT_ITEMS_COMBAT_SPEC_v1.0.md`  
> **Branch alvo:** `dev`

---

## 1. User story

Como jogador de Cindar's Hope, quero equipar e desequipar armas, magia, ferramentas, muniÃ§Ã£o e consumÃ­veis para alternar meu estilo de combate e sobrevivÃªncia sem depender apenas do soco bÃ¡sico.

---

## 2. Objetivos funcionais

### O1 â€” Equipar e desequipar itens

O jogador deve conseguir equipar e desequipar:

- arma primÃ¡ria;
- magia;
- muniÃ§Ã£o;
- ferramenta;
- consumÃ­vel em hotbar.

### O2 â€” Loadouts vÃ¡lidos

O MVP deve aceitar:

- espada + magia;
- arco + flechas;
- ferramenta ativa independente;
- consumÃ­vel em hotbar independente.

O MVP deve bloquear:

- arco + magia;
- espada + arco simultaneamente.

### O3 â€” Espada

A espada bÃ¡sica deve causar mais dano que o soco.

### O4 â€” Arco e flecha

O arco deve atacar Ã  distÃ¢ncia com projectile e consumir flecha quando configurado com `AmmoCost > 0`.

### O5 â€” Magia de fogo

A magia de fogo deve lanÃ§ar projectile e aplicar Burn por 3 segundos.

### O6 â€” PoÃ§Ã£o

A poÃ§Ã£o pequena deve poder ser equipada na hotbar e usada para restaurar HP, consumindo 1 item.

### O7 â€” PersistÃªncia

Save/load deve preservar o equipamento ativo e limpar slots invÃ¡lidos de forma segura.

---

## 3. Non-goals

Fora de escopo desta feature:

- UI final de inventÃ¡rio;
- drag and drop;
- Ã¡rvore de magia;
- mana system completo;
- balanceamento final;
- animaÃ§Ãµes finais;
- pooling avanÃ§ado de projectile;
- durabilidade de equipamento;
- mÃºltiplos loadouts salvos.

---

## 4. Regras de negÃ³cio

### R1 â€” Espada + magia

Se `PrimaryWeapon` for melee, o slot `Magic` pode ficar equipado.

### R2 â€” Arco bloqueia magia

Se o jogador equipa arco:

- `PrimaryWeapon = Bow`;
- `Magic` deve ser limpo automaticamente.

### R3 â€” Magia bloqueada com arco

Se arco jÃ¡ estÃ¡ equipado, tentar equipar magia deve falhar com feedback.

### R4 â€” Equipar nÃ£o consome item

Equipar arma, magia, muniÃ§Ã£o, ferramenta ou consumÃ­vel nÃ£o remove item do inventÃ¡rio.

### R5 â€” Usar consumÃ­vel consome item

Usar poÃ§Ã£o consome 1 unidade se o uso foi bem-sucedido.

### R6 â€” Flecha consome ammo

Disparar arco consome `AmmoCost` do `RequiredAmmoItemId`.

### R7 â€” Burn

Burn aplicado pela magia de fogo deve:

- durar 3 segundos;
- causar dano contÃ­nuo;
- no MVP: 1 dano por segundo;
- renovar duraÃ§Ã£o se aplicado novamente;
- nÃ£o stackar poder ainda.

---

## 5. Entidades funcionais

### EquipmentSlotType

Slots mÃ­nimos:

- `PrimaryWeapon`
- `Magic`
- `Ammo`
- `Tool`
- `ConsumableHotbar`

### WeaponType

Tipos mÃ­nimos:

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

## 6. CritÃ©rios de aceite

### CA1 â€” Equipar espada

Dado que o jogador possui `item_weapon_sword_basic`, quando equipa a espada, entÃ£o `PrimaryWeapon` passa a ser `weapon_sword_basic` e o HUD debug mostra a espada.

### CA2 â€” Desequipar espada

Dado que a espada estÃ¡ equipada, quando o jogador desequipa `PrimaryWeapon`, entÃ£o o slot fica vazio e o item permanece no inventÃ¡rio.

### CA3 â€” Espada causa mais dano que soco

Dado um inimigo com HP suficiente, quando o jogador ataca com espada, entÃ£o o dano aplicado Ã© maior que o dano do ataque desarmado.

### CA4 â€” Equipar arco remove magia

Dado que magia estÃ¡ equipada, quando o jogador equipa arco, entÃ£o `Magic` fica vazio e `PrimaryWeapon` passa a ser arco.

### CA5 â€” Arco dispara projectile

Dado que arco e flecha estÃ£o equipados, quando o jogador ataca, entÃ£o um projectile fÃ­sico Ã© criado e se move na direÃ§Ã£o do ataque.

### CA6 â€” Arco consome flecha

Dado que o arco tem `AmmoCost = 1`, quando o jogador dispara, entÃ£o o inventÃ¡rio perde 1 flecha.

### CA7 â€” Arco sem flecha falha

Dado que arco estÃ¡ equipado e nÃ£o hÃ¡ flecha, quando o jogador ataca, entÃ£o nenhum projectile Ã© criado e o HUD/log mostra feedback.

### CA8 â€” Magia bloqueada com arco

Dado que arco estÃ¡ equipado, quando o jogador tenta equipar magia, entÃ£o a aÃ§Ã£o falha e o slot `Magic` continua vazio.

### CA9 â€” Espada + magia permitido

Dado que espada estÃ¡ equipada, quando o jogador equipa magia de fogo, entÃ£o `PrimaryWeapon` continua espada e `Magic` passa a ser magia de fogo.

### CA10 â€” Magia aplica Burn

Dado que magia de fogo acerta um inimigo, entÃ£o o inimigo recebe dano inicial e status Burn por 3 segundos.

### CA11 â€” Burn causa dano contÃ­nuo

Dado que inimigo estÃ¡ com Burn, entÃ£o ele recebe dano contÃ­nuo durante 3 segundos.

### CA12 â€” PoÃ§Ã£o restaura HP

Dado que poÃ§Ã£o estÃ¡ na hotbar e o jogador perdeu HP, quando usa a poÃ§Ã£o, entÃ£o HP aumenta e o inventÃ¡rio perde 1 poÃ§Ã£o.

### CA13 â€” Save/load preserva equipamento

Dado que jogador salva com espada, magia e poÃ§Ã£o equipada, quando carrega o save, entÃ£o os slots retornam ao mesmo estado se os itens ainda existem no inventÃ¡rio.

### CA14 â€” Save/load limpa slot invÃ¡lido

Dado um save com item equipado inexistente ou removido do inventÃ¡rio, quando carrega, entÃ£o o slot Ã© limpo com warning e o jogo continua.

---

## 7. DependÃªncias

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
- `docs_old/FASE9C_PLAYER_EQUIPMENT_ITEMS_COMBAT_SPEC_v1.0.md` estiver lida.
- `docs/architecture/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.1_FASE9C_DELTA.md` estiver lido.
- Estado real do repo estiver validado na branch `dev`.

