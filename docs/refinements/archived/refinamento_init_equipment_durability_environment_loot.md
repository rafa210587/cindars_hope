# refinamento_init_equipment_durability_environment_loot

> Status: Refinamento inicial a implementar
> Spec futura relacionada: `.specs/a_implementar/spec_equipment_durability_environment_loot_runtime.md`
> Objetivo: completar equipment, durabilidade, HUD posicional, maos, resistencias ambientais e loot runtime.

---

## 1. Estado atual

`EquipmentManager` atual e parcial/debug: guarda tool equipada, weapon equipada e infere tool por ID. Ja existem dados/managers iniciais para equipment, durability, environmental resistance e loot, mas ainda nao formam runtime completo.

Evidencia:

```text
Assets/_Game/Scripts/Equipment/EquipmentManager.cs
Assets/_Game/Scripts/Equipment/EquipmentDataSO.cs
Assets/_Game/Scripts/Equipment/DurabilityManager.cs
Assets/_Game/Scripts/Equipment/EnvironmentalResistanceManager.cs
Assets/_Game/Scripts/Loot/LootTableSO.cs
.specs/implementados/spec_tools_001_tools_equipment_hotbar_parcial.md
```

---

## 2. Gaps

- EquipmentManager nao aplica stats no player.
- Equipamento nao possui slots formais/posicionais.
- Nao ha mao esquerda/direita como slots de uso/equipamento.
- Tool ainda nao ocupa slot de mao como equipamento.
- Nao ha `ItemInstanceId` para durabilidade por unidade.
- Equipaveis/duraveis ainda podem ser tratados como stackables indevidamente.
- Durabilidade nao e consumida por tool use/combat.
- Item quebrado/repair nao tem fluxo.
- Resistencias ambientais nao afetam cave/bioma/status.
- LootTableSO ainda nao gera equipment instances com durabilidade.
- Save/load de equipment/durability ainda e parcial.
- UI de equipamento nao existe.

---

## 3. Decisoes aprovadas

- HUD de equipamento deve ser posicional.
- Equipment HUD deve mostrar armadura, acessorio, mao esquerda e mao direita.
- Tool ocupa uma das maos.
- Ao selecionar um slot de equipamento, abrir inventory HUD/modal filtrado para escolher item compativel e ocupar aquele slot.
- Mao esquerda responde as hotkeys `1,2,3,4,5,6` conforme uso/selecoes ja definidos no projeto; esta spec nao deve quebrar o comportamento existente desses atalhos.
- Slots MVP:

```text
LeftHand
RightHand
Head
Chest
Legs
Boots
Ring1
Ring2
Accessory
```

- Equipamento permanece no inventory; equipment slot guarda binding para `ItemInstanceId` ou inventory slot compativel.
- Equipaveis/duraveis sao non-stackable.
- Cada equipavel/duravel tem `ItemInstanceId`.
- Stackables continuam por `ItemId + Amount`.
- Stats MVP usam soma flat.
- AttackSpeed entra agora.
- AttackSpeed base por enquanto e `1.0`.
- Cada tipo de arma altera AttackSpeed por percentual para mais ou para menos.
- Destreza aumenta percentual de velocidade de armas leves por ponto.
- Forca aumenta percentual de velocidade de armas pesadas por ponto.
- Percent modifiers genericos ficam hook futuro; nesta spec, apenas AttackSpeed usa formula percentual especifica.
- Durabilidade 0 deixa item `Broken`, auto-unequipa e bloqueia uso/equip.
- RepairKit MVP restaura 50% da durabilidade maxima.
- EnvironmentalResistance MVP: Toxic, Cold, Heat.
- LootTableSO deve gerar stackables e equipment instances.
- Equipamento dropado/looteado comeca com 100% de durability por padrao.
- Rarity/affixes ficam como hook futuro, sem roll final.

---

## 4. Equipment slots e HUD posicional

Slots MVP:

```text
LeftHand
RightHand
Head
Chest
Legs
Boots
Ring1
Ring2
Accessory
```

Layout conceitual da HUD:

```text
          Head
Accessory Chest Ring1 Ring2
          Legs
          Boots
LeftHand        RightHand
```

Regras:

- HUD/painel de equipment e modal/painel interativo e deve respeitar modal stack.
- Ao selecionar um slot, abrir inventory modal/HUD filtrado para itens compativeis.
- Se usuario seleciona item valido, equipar no slot.
- Se usuario cancela, nada muda.
- Se item for invalido, broken, sem instance ou de categoria errada, bloquear com feedback.
- `Esc` fecha ou volta para HUD anterior.
- HUD final consolidada fica para spec 17; esta spec entrega UI minima funcional/debug.

---

## 5. Maos, tools e hotkeys

- Armas e tools ocupam uma das maos.
- Uma tool equipada em mao deve ser tratada como equipamento e como ferramenta ativa quando selecionada/acionada.
- `LeftHand` e `RightHand` nao podem conter dois itens iguais por duplicacao de binding.
- Se item for removido do inventory, slot deve limpar binding de forma segura.
- Hotkeys `1,2,3,4,5,6` devem continuar respeitando o comportamento ja definido nas specs anteriores.
- Esta spec nao deve remover hotbar/quick slots existentes.
- Se houver conflito entre hotbar e mao equipada, registrar regra clara: hotkey seleciona item/slot ativo e acao usa o item se ele estiver equipado ou puder ser equipado no slot de mao valido.

---

## 6. Inventory e ItemInstanceId

Equipaveis/duraveis exigem item instance.

Dados minimos de inventory slot para non-stackable:

```text
InventorySlotId ou SlotIndex
ItemInstanceId
ItemId
Amount = 1
DurabilityCurrent
DurabilityMax
IsBroken
EquippedSlot opcional
```

Regras:

- Weapons, armor, tools, rings e accessories sao non-stackable.
- Seeds, materials, food, fish, wood, stone continuam stackable.
- EquipmentSlot deve apontar para `ItemInstanceId` estavel.
- Derived stats nao sao persistidos; devem ser recalculados no load/equip/unequip.
- Migracao de saves existentes deve gerar instances para itens equipaveis/duraveis quando necessario.

---

## 7. Stats e AttackSpeed

Stats MVP:

```text
MaxHP
Attack
Defense
MoveSpeed
MaxStamina
StaminaRegen
AttackSpeed
ToxicResistance
ColdResistance
HeatResistance
```

Regras:

- Equipamento soma modificadores flat para stats MVP, exceto AttackSpeed.
- `BaseAttackSpeed = 1.0`.
- Cada tipo de arma altera AttackSpeed por percentual.
- Armas leves usam Dexterity para bonus percentual de AttackSpeed.
- Armas pesadas usam Strength para bonus percentual de AttackSpeed.
- Armas neutras usam apenas weapon type multiplier no MVP.
- Se Strength/Dexterity ainda nao existirem no runtime, preparar hooks e usar 0 como default.
- AttackSpeed nao deve quebrar punch/combat MVP; se combate final ainda nao existir, expor stat derivado para spec 12.

Campos sugeridos:

```text
WeaponWeightClass
AttackSpeedMultiplier
DexterityAttackSpeedPercentPerPoint
StrengthAttackSpeedPercentPerPoint
```

---

## 8. Durability

Perda de durabilidade:

```text
Tool: perde por uso efetivo.
Weapon: perde por ataque/hit efetivo.
Armor: perde ao receber dano.
Rings/accessory: nao perdem durabilidade no MVP.
```

Regras:

- Durability so reduz quando acao efetivamente acontece.
- Se acao falhar por stamina, inventory, target invalido ou estado bloqueado, nao reduzir durability.
- Durability nunca fica abaixo de 0.
- Quando chega a 0:

```text
IsBroken = true
Auto-unequip se equipado
Nao pode usar
Nao pode equipar
Pode reparar
```

---

## 9. Repair MVP

MVP usa `RepairKit`.

Regras:

- Consumir `RepairKit` em um item selecionado restaura 50% da durabilidade maxima.
- Nao ultrapassar `DurabilityMax`.
- RepairKit so e consumido se reparo aplicar algum ganho real.
- Broken item reparado deixa de estar broken se `DurabilityCurrent > 0`.
- Repair por vendedor/forge/workstation fica futuro.

---

## 10. Environmental resistance

Tipos MVP:

```text
Toxic
Cold
Heat
```

Modelo:

```text
EnvironmentZone
HazardType
RequiredResistance
ExposureStatusEffectId
DamagePerTick opcional
StaminaRegenPenalty opcional
MoveSpeedPenalty opcional
TickIntervalSeconds
```

Cenario minimo testavel:

```text
Cave test EnvironmentZone
HazardType = Toxic
RequiredResistance = 5
Sem resistencia: aplica debuff/damage over time leve
Com ToxicResistance >= 5: bloqueia efeito
```

---

## 11. Loot runtime

`LootTableSO` deve suportar:

```text
Stackable item drop
Equipment instance drop
MinAmount
MaxAmount
Weight
Chance
RequiredTags opcional
```

Equipment instance drop deve gerar:

```text
ItemInstanceId
ItemId
DurabilityCurrent = DurabilityMax por padrao
DurabilityMax
IsBroken = false
ModifierIds opcional futuro
Rarity opcional futuro
```

Regras:

- Looted equipment comeca com 100% durability por padrao.
- Rarity/affixes nao rolam efeitos reais nesta spec.
- Enemy final fica para spec 13, mas loot runtime deve estar pronto para enemy/resource/cave usar.
- Integrar ao menos um drop runtime testavel de equipment/resource/cave sem invadir enemy final.

---

## 12. Save/load

Equipment save:

```text
EquipmentSaveData
- Slots[]

EquipmentSlotSaveData
- SlotType
- ItemInstanceId
```

Inventory item instance save:

```text
ItemInstanceSaveData
- ItemInstanceId
- ItemId
- DurabilityCurrent
- DurabilityMax
- IsBroken
- EquippedSlot opcional
```

Regras:

- Nao serializar `EquipmentDataSO`, `ItemDataSO`, `LootTableSO`, `GameObject`, `Transform`, `MonoBehaviour`, `Sprite`, `Collider` ou `Rigidbody`.
- Derived stats sao recalculados no load.
- Se `ItemInstanceId` equipado nao existir no inventory ao carregar, limpar slot e logar warning.
- Se item esta broken ao carregar, nao deve ficar equipado.

---

## 13. Invariantes anti-regressao

Esta spec nao pode quebrar:

- inventory slots/capacity da spec 03;
- item equip mantendo item no inventory;
- hotbar/action slots e hotkeys `1-6` existentes;
- stamina/hunger da spec 09;
- crafting outputs da spec 07;
- shop equipment sales da spec 06;
- world loot/pickups da spec 05;
- combat futuro da spec 12;
- save migration e DTOs simples;
- modal stack;
- regra de nao usar `GameObject.Find()` ou `FindObjectOfType()`;
- regra de nao serializar referencias Unity em DTOs.

---

## 14. Definition of Done

- [ ] Equipment HUD posicional existe e mostra slots MVP.
- [ ] LeftHand e RightHand existem.
- [ ] Tool pode ocupar uma das maos.
- [ ] Selecionar slot abre inventory filtrado para equipar item valido.
- [ ] Hotkeys `1-6` preservam comportamento existente.
- [ ] Equipaveis/duraveis sao non-stackable e possuem `ItemInstanceId`.
- [ ] Equipar item nao remove item do inventory, apenas cria binding.
- [ ] Unequip limpa binding e mantem item no inventory.
- [ ] Stats derivados flat recalculam ao equipar/unequip.
- [ ] AttackSpeed base 1.0 existe e considera tipo de arma + Strength/Dexterity quando disponivel.
- [ ] Durability reduz por uso efetivo.
- [ ] Durability 0 deixa item broken, auto-unequipa e bloqueia uso/equip.
- [ ] RepairKit restaura 50% da durabilidade maxima sem ultrapassar max.
- [ ] Environmental resistance Toxic/Cold/Heat existe.
- [ ] Cave toxic test zone aplica efeito sem resistencia e bloqueia com resistencia suficiente.
- [ ] LootTableSO gera stackables e equipment instances.
- [ ] Save/load preserva equipment slots, item instances, durability e broken state.
- [ ] Invariantes anti-regressao preservadas.

---

## 15. Validacao

1. Abrir Equipment HUD e validar slots posicionais.
2. Selecionar LeftHand e equipar tool pelo inventory filtrado.
3. Selecionar RightHand e equipar arma pelo inventory filtrado.
4. Validar que itens continuam no inventory e nao duplicam.
5. Validar hotkeys `1-6` com comportamento preservado.
6. Equipar armadura e validar stats derivados.
7. Validar AttackSpeed base 1.0 e modificador por arma leve/pesada.
8. Usar tool e validar perda de durability apenas em uso efetivo.
9. Reduzir durability a 0 e validar broken + auto-unequip.
10. Usar RepairKit e validar restaura 50% max.
11. Entrar em toxic zone sem resistencia e validar debuff/damage leve.
12. Equipar item com ToxicResistance >= 5 e validar bloqueio do efeito.
13. Gerar loot de equipment e validar ItemInstanceId + durability 100%.
14. Salvar/carregar equipment slots, item instances, durability e broken state.
15. Validar que Equipment HUD nao sobrepoe Dialogue/Shop/Crafting/Inventory sem controle.
