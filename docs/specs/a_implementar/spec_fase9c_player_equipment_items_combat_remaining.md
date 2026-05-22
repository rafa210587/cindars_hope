# SPEC FUTURA — FASE9C Player Equipment Items Combat Remaining

> Origem histórica: $Source
> Status: A implementar / restante não implementado
> Observação: conteúdo histórico preservado; não duplicar capacidades já consolidadas em specs implementadas.

---

## Escopo preservado

# FASE 9C — Player Equipment, Item Use e Combat Loadout Spec v1.0

> **Status:** spec futura aprovada para orientar próximas waves.  
> **Base:** FASE 9C Tools/Farm/Combat + FASE 9D Enemy Architecture.  
> **Objetivo:** estruturar equipar/desequipar itens do jogador, seleção de item ativo, uso de consumíveis e três estilos iniciais de combate: espada, arco/flecha e magia de fogo com dano contínuo.

---

## 1. Objetivo

Adicionar ao jogador a capacidade de:

1. equipar item;
2. desequipar item;
3. trocar loadout;
4. selecionar item ativo;
5. usar item consumível, como poção;
6. atacar com espada;
7. atacar com arco e flecha;
8. usar magia de fogo;
9. aplicar status negativo em inimigos;
10. persistir equipamento no save/load.

---

## 2. Decisão de design

O jogador deve ter **loadouts restritos**, não equipar tudo ao mesmo tempo.

Loadouts válidos no MVP:

| Loadout | Permitido |
|---|---|
| Espada + Magia | Sim |
| Arco + Flechas | Sim |
| Espada + Arco | Não no MVP |
| Arco + Magia | Não no MVP |
| Magia sem arma primária | Opcional, pode ser permitido depois |

Regra central:

- Espada é arma de uma mão e permite magia equipada junto.
- Arco ocupa o estilo ofensivo principal e bloqueia magia equipada no MVP.
- Flecha pode ser consumida do inventário ou de um slot de munição simplificado.
- Consumíveis são independentes do loadout ofensivo.

---

## 3. Slots de equipamento

### 3.1 EquipmentSlotType

```csharp
public enum EquipmentSlotType
{
    None,
    PrimaryWeapon,
    Magic,
    Ammo,
    Tool,
    ConsumableHotbar
}
```

### 3.2 PlayerEquipmentLoadout

```csharp
[Serializable]
public class PlayerEquipmentLoadout
{
    public string EquippedPrimaryWeaponId;
    public string EquippedMagicId;
    public string EquippedAmmoItemId;
    public string EquippedToolId;
    public List<string> ConsumableHotbarItemIds = new List<string>();
    public int SelectedConsumableIndex;
}
```

### 3.3 Regras de slot

| Slot | Conteúdo | Observação |
|---|---|---|
| PrimaryWeapon | espada ou arco | define ataque principal |
| Magic | magia equipada | bloqueada se PrimaryWeapon for Bow |
| Ammo | flecha | exigida por arco se `AmmoCost > 0` |
| Tool | ferramenta ativa | Hoe/Axe/Sickle/etc. |
| ConsumableHotbar | poções/itens usáveis | não conflita com arma |

---

## 4. Categorias de item

Expandir `ItemCategory` futuramente:

```csharp
public enum ItemCategory
{
    Seed,
    Crop,
    Food,
    Material,
    Tool,
    Fish,
    Weapon,
    Magic,
    Ammo,
    Consumable,
    Misc
}
```

Observação: se preferirmos reduzir alteração inicial, `Weapon`, `Magic`, `Ammo` e `Consumable` podem começar como `Misc` + databases específicas. Mas o ideal é explicitar categorias.

---

## 5. Equipar e desequipar

### 5.1 EquipmentManager

`EquipmentManager` deve gerenciar:

- ferramenta equipada;
- arma primária equipada;
- magia equipada;
- munição equipada;
- consumíveis na hotbar;
- item consumível selecionado.

API sugerida:

```csharp
public class EquipmentManager : MonoBehaviour
{
    public string EquippedToolId { get; private set; }
    public string EquippedPrimaryWeaponId { get; private set; }
    public string EquippedMagicId { get; private set; }
    public string EquippedAmmoItemId { get; private set; }
    public int SelectedConsumableIndex { get; private set; }

    public bool TryEquipTool(string itemId);
    public bool TryEquipPrimaryWeapon(string itemId);
    public bool TryEquipMagic(string itemId);
    public bool TryEquipAmmo(string itemId);
    public bool TryEquipConsumable(string itemId, int hotbarIndex);

    public bool TryUnequip(EquipmentSlotType slotType);
    public bool TryUnequipConsumable(int hotbarIndex);

    public bool TrySelectConsumable(int hotbarIndex);
    public bool TryGetSelectedConsumable(out ItemDataSO itemData);

    public EquipmentSaveData CaptureSaveData();
    public void RestoreFromSaveData(EquipmentSaveData saveData);
}
```

### 5.2 Regras de equip

#### Equipar espada

- Valida item no inventário.
- Valida `WeaponDataSO.WeaponType == Melee`.
- Equipa em `PrimaryWeapon`.
- Não remove magia equipada.
- Se havia arco, troca para espada e libera slot de magia.

#### Equipar arco

- Valida item no inventário.
- Valida `WeaponDataSO.WeaponType == RangedPhysical`.
- Equipa em `PrimaryWeapon`.
- Desequipa magia automaticamente ou bloqueia equip se magia já equipada, decisão MVP abaixo.

Decisão MVP: ao equipar arco, desequipar magia automaticamente com evento e feedback no HUD.

#### Equipar magia

- Valida item no inventário ou spell aprendido, conforme fase.
- Valida `MagicDataSO` ou `WeaponDataSO.WeaponType == RangedMagic`.
- Só permite se `PrimaryWeapon` estiver vazio ou for Melee.
- Se arco estiver equipado, falha com mensagem: `Arco impede magia equipada no MVP.`

#### Equipar flecha

- Valida item no inventário.
- Valida categoria `Ammo`.
- Equipa em `Ammo`.
- Só é consumida ao disparar, não ao equipar.

#### Equipar consumível

- Valida item no inventário.
- Valida categoria `Consumable` ou `Food` com efeito de uso.
- Coloca em hotbar.
- Não remove item do inventário até usar.

### 5.3 Desequipar

Desequipar:

- limpa o slot;
- não remove item do inventário;
- publica evento;
- atualiza HUD/UI.

---

## 6. Dados de arma

### 6.1 WeaponType

```csharp
public enum WeaponType
{
    Unarmed,
    Melee,
    RangedPhysical,
    RangedMagic
}
```

### 6.2 WeaponDataSO

```csharp
[CreateAssetMenu(fileName = "WeaponData", menuName = "CindarsHope/Combat/Weapon")]
public class WeaponDataSO : ScriptableObject, IIdentifiedData
{
    public string Id;
    public ItemDataSO Item;
    public WeaponType WeaponType;

    [Header("Damage")]
    public int Damage = 1;
    public DamageElement Element = DamageElement.Physical;
    public float Range = 0.8f;
    public float CooldownSeconds = 0.4f;
    public float KnockbackForce = 0f;

    [Header("Projectile")]
    public string ProjectileId;
    public float ProjectileSpeed = 6f;
    public float ProjectileLifetimeSeconds = 2f;

    [Header("Costs")]
    public string RequiredAmmoItemId;
    public int AmmoCost = 0;
    public int MpCost = 0;
    public int HungerCost = 0;

    [Header("Status")]
    public StatusApplicationData[] StatusApplications;

    [Header("Slot Rules")]
    public bool BlocksMagicSlot;
}
```

---

## 7. Dados de magia

Para magia, há duas opções arquiteturais:

### Opção A — Magia como WeaponDataSO

Usar `WeaponDataSO.WeaponType = RangedMagic`.

Vantagem:

- menos classes;
- PlayerCombatController usa o mesmo fluxo;
- bom para MVP.

### Opção B — MagicDataSO separado

Criar `MagicDataSO`.

Vantagem:

- melhor quando houver árvore de magias, cooldowns próprios, escolas, upgrades, mana etc.

### Decisão MVP

Usar **WeaponDataSO para magia** no MVP.

Depois, se crescer, migrar para `SpellDataSO` ou `MagicDataSO`.

---

## 8. Status aplicado por ataque

### 8.1 StatusApplicationData

```csharp
[Serializable]
public struct StatusApplicationData
{
    public StatusEffectType StatusType;
    public float Chance;
    public float DurationSeconds;
    public int Power;
    public DamageElement Element;
}
```

### 8.2 Magia de fogo MVP

A magia de fogo deve aplicar dano contínuo por 3s.

```text
Item: item_magic_fire_spark
WeaponData: weapon_magic_fire_spark
WeaponType: RangedMagic
Damage: 1 inicial
Element: Fire
Range: 5.0
CooldownSeconds: 0.8
ProjectileId: projectile_fire_spark
ProjectileSpeed: 6.5
MpCost: 0 no MVP ou futuro > 0
StatusApplications:
  - Burn
    Chance: 1.0
    DurationSeconds: 3.0
    Power: 1
    Element: Fire
```

### 8.3 Burn

Burn deve:

- aplicar dano contínuo por 3s;
- no MVP: 1 dano por segundo por 3 ticks;
- usar elemento Fire;
- respeitar resistência/vulnerabilidade elemental futura;
- não stackar infinitamente.

Regra MVP de stack:

- se Burn já está ativo, renovar duração;
- não somar Power ainda.

---

## 9. Consumíveis e uso de item

### 9.1 ConsumableDataSO

```csharp
[CreateAssetMenu(fileName = "ConsumableData", menuName = "CindarsHope/Data/Consumable")]
public class ConsumableDataSO : ScriptableObject, IIdentifiedData
{
    public string Id;
    public ItemDataSO Item;

    public int RestoreHp;
    public int RestoreHunger;
    public int RestoreMp;

    public StatusEffectType RemovesStatus;
    public bool IsConsumedOnUse = true;
    public float UseCooldownSeconds = 0.3f;
}
```

### 9.2 ItemUseManager

Responsável por:

- usar item selecionado;
- validar se existe no inventário;
- aplicar efeito;
- remover 1 unidade se consumido;
- publicar evento.

API sugerida:

```csharp
public class ItemUseManager : MonoBehaviour
{
    public bool TryUseSelectedConsumable();
    public bool TryUseItem(string itemId);
}
```

### 9.3 Poção MVP

```text
Item: item_potion_small_hp
ConsumableData: consumable_potion_small_hp
RestoreHp: 10
IsConsumedOnUse: true
UseCooldownSeconds: 0.3
```

---

## 10. PlayerCombatController

### 10.1 Responsabilidade

Substituir gradualmente `PlayerAttackController`.

Responsável por:

- ler input de ataque;
- verificar arma primária;
- usar espada/arco/soco;
- verificar magia equipada e input de magia;
- consumir flecha se necessário;
- spawnar projétil;
- aplicar status;
- respeitar cooldowns.

### 10.2 Inputs MVP

| Input | Ação |
|---|---|
| J | ataque primário: soco/espada/arco |
| K | magia equipada |
| Q/E ou números | seleção de consumível/hotbar futura |
| H ou tecla futura | usar consumível selecionado |

Observação: enquanto não migrar para Input System completo, usar input legacy como o projeto já usa.

### 10.3 Ataque com espada

```text
Item: item_weapon_sword_basic
WeaponData: weapon_sword_basic
WeaponType: Melee
Damage: 3
Element: Physical
Range: 1.0
CooldownSeconds: 0.45
KnockbackForce: 2.0
BlocksMagicSlot: false
```

Regra:

- causa mais dano que soco;
- usa overlap melee;
- aplica `DamageRequest`;
- se inimigo está vulnerável, dano recebe multiplicador.

### 10.4 Ataque com arco

```text
Item: item_weapon_bow_basic
WeaponData: weapon_bow_basic
WeaponType: RangedPhysical
Damage: 2
Element: Physical
Range: 6.0
CooldownSeconds: 0.65
ProjectileId: projectile_arrow_basic
ProjectileSpeed: 7.0
RequiredAmmoItemId: item_ammo_arrow_basic
AmmoCost: 1
BlocksMagicSlot: true
```

Regra:

- dispara flecha em linha reta;
- consome 1 flecha se `AmmoCost > 0`;
- se não tiver flecha, ataque falha com feedback;
- enquanto Bow equipado, Magic slot fica bloqueado no MVP.

### 10.5 Magia de fogo

```text
Item: item_magic_fire_spark
WeaponData: weapon_magic_fire_spark
WeaponType: RangedMagic
Damage: 1
Element: Fire
Range: 5.0
CooldownSeconds: 0.8
ProjectileId: projectile_fire_spark
ProjectileSpeed: 6.5
Status: Burn por 3s
BlocksMagicSlot: false
```

Regra:

- só pode usar se equipada no slot Magic;
- só pode equipar junto de espada ou sem arma primária;
- ao acertar, aplica dano inicial + Burn;
- Burn causa dano contínuo por 3s.

---

## 11. Projectiles

### 11.1 ProjectileController

```csharp
public class ProjectileController : MonoBehaviour
{
    public void Initialize(ProjectileRuntimeData data);
}
```

### 11.2 ProjectileRuntimeData

```csharp
public readonly struct ProjectileRuntimeData
{
    public readonly string ProjectileId;
    public readonly string SourceWeaponId;
    public readonly Vector2 Direction;
    public readonly int Damage;
    public readonly DamageElement Element;
    public readonly float Speed;
    public readonly float LifetimeSeconds;
    public readonly StatusApplicationData[] StatusApplications;
}
```

### 11.3 Regras

- Projectile não depende de tag.
- Detecta `EnemyHealth`/`EnemyStatusReceiver` por componente.
- Aplica dano e status.
- Desativa/destroi ao bater ou expirar.
- Futuramente usar pool.

---

## 12. Eventos novos

### EquipmentChangedEvent

Payload:

- `SlotType`
- `ItemId`
- `IsEquipped`

### ItemEquippedEvent

Payload:

- `ItemId`
- `SlotType`

### ItemUnequippedEvent

Payload:

- `ItemId`
- `SlotType`

### ItemUsedEvent

Payload:

- `ItemId`
- `AmountConsumed`
- `Succeeded`

### PlayerPrimaryAttackEvent

Payload:

- `WeaponId`
- `WeaponType`
- `Damage`
- `Element`

### PlayerMagicCastEvent

Payload:

- `MagicId`
- `ProjectileId`
- `Element`

### StatusAppliedEvent

Payload:

- `SourceId`
- `TargetId`
- `StatusType`
- `DurationSeconds`
- `Power`

Regra: eventos usam tipos simples/IDs. Não carregar `GameObject`, `Transform`, `MonoBehaviour` ou `ScriptableObject`.

---

## 13. Save

### 13.1 EquipmentSaveData

```csharp
[Serializable]
public class EquipmentSaveData
{
    public string EquippedToolId;
    public string EquippedPrimaryWeaponId;
    public string EquippedMagicId;
    public string EquippedAmmoItemId;
    public List<string> ConsumableHotbarItemIds = new List<string>();
    public int SelectedConsumableIndex;
}
```

### 13.2 Compatibilidade

- Saves antigos sem Equipment devem carregar com slots vazios.
- IDs inválidos devem limpar slot e gerar warning.
- Equipamento não remove item do inventário.
- Se item equipado não existe mais no inventário, o slot deve ser limpo no load.

---

## 14. HUD debug MVP

Antes da UI final, DebugHud deve mostrar:

```text
Tool: Axe Basic
Weapon: Sword Basic
Magic: Fire Spark
Ammo: Arrow x12
Consumable: Small Potion x3
```

E mensagens:

```text
Equipped Sword Basic
Unequipped Fire Spark
Cannot equip magic while Bow is equipped
No arrows available
Used Small Potion +10 HP
Burn applied for 3s
```

---

## 15. PRs recomendados

### PR-123 — Equipment slot contracts

Criar:

- `EquipmentSlotType`
- `PlayerEquipmentLoadout`
- `EquipmentSaveData` expandido
- eventos de equip/unequip

Sem alterar gameplay.

### PR-124 — EquipmentManager equip/unequip

Criar regras:

- equipar/desequipar PrimaryWeapon;
- equipar/desequipar Magic;
- equipar/desequipar Ammo;
- equipar/desequipar Tool;
- equipar consumível na hotbar.

### PR-125 — Weapon/Magic/Consumable data assets

Criar:

- `item_weapon_sword_basic`
- `weapon_sword_basic`
- `item_weapon_bow_basic`
- `weapon_bow_basic`
- `item_ammo_arrow_basic`
- `item_magic_fire_spark`
- `weapon_magic_fire_spark`
- `item_potion_small_hp`
- `consumable_potion_small_hp`

### PR-126 — PlayerCombatController melee + bow

Implementar:

- espada causa mais dano que soco;
- arco dispara flecha;
- flecha consome ammo;
- bow bloqueia magic slot.

### PR-127 — Magic fire + Burn status

Implementar:

- magia de fogo equipada;
- projectile_fire_spark;
- Burn por 3s;
- integração com `EnemyStatusReceiver`.

### PR-128 — ItemUseManager + potion

Implementar:

- usar item selecionado;
- poção pequena restaura HP;
- remove 1 unidade do inventário;
- HUD/log de resultado.

### PR-129 — Debug equip selection MVP

Implementar seleção temporária:

- tecla debug para alternar equipamento;
- ou menu debug simples no HUD;
- suficiente até UI real.

### PR-130 — Validator equipment/items/combat loadout

Validar:

- WeaponDatabase;
- itens de arma/magia/ammo/consumível;
- loadout rules;
- consumable data;
- status Burn.

---

## 16. Critérios de aceite

Ao final do pacote:

- jogador equipa espada;
- jogador desequipa espada;
- espada causa mais dano que soco;
- jogador equipa arco;
- arco dispara flecha Ã  distância;
- flecha consome ammo;
- arco bloqueia magia no MVP;
- jogador equipa espada + magia;
- magia de fogo dispara projectile;
- magia aplica Burn;
- Burn causa dano contínuo por 3s;
- jogador equipa poção na hotbar;
- jogador usa poção;
- poção restaura HP e consome 1 item;
- save/load preserva equipamento;
- HUD debug exibe loadout;
- console sem erro vermelho.

---

## 17. Decisão final

O sistema de equipamento do jogador deve ser implementado como parte da FASE 9C, mas em subpacote próprio.

A ordem recomendada é:

1. contratos;
2. manager;
3. dados/assets;
4. espada/arco;
5. magia/status;
6. consumíveis;
7. seleção/debug UI;
8. validator.

Não implementar UI final antes de validar loadout e regras de equip/desequip.


## Regra de uso

Antes de implementar, reconciliar este material com docs/specs/implementados/, docs/refinements/implementados/ e o estado real do código.


