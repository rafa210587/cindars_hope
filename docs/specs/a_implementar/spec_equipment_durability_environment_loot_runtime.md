# SPEC - Equipment, durability, environment e loot runtime

> Spec ID: spec_equipment_durability_environment_loot_runtime
> Status: A implementar
> Ordem de execucao: 10
> Depende de: 00-09
> Bloqueia: 11, 12, 13, 14, 17
> Tipo: Runtime/UI
> Fonte: docs/specs/ como fonte unica; fontes absorvidas listadas abaixo.
> Escopo: Completar equipment slots posicionais, mao esquerda/direita, tools como equipamento de mao, durabilidade por item instance, repair kit, stats derivados, AttackSpeed, resistencias ambientais e loot runtime com equipment instances.
> Fora de escopo: affixes/raridade final, combate final completo, magia/mana final, dual wield avancado, skill trees, UI final consolidada, sprint/dodge final, Packages, ProjectSettings e docs_old.

Fontes absorvidas:
- specs/FASE9H_CAVE_LOOT_CRAFTING_EQUIPMENT_PROGRESSION/spec.md
- docs/refinements/a_implementar/pre_refinamentos/refinamento_init_equipment_durability_environment_loot.md

---

# /speckit.specify

## Contexto

`EquipmentManager` atual e parcial/debug: guarda tool equipada, weapon equipada e infere tool por ID. Ja existem dados/managers iniciais para equipment, durability, environmental resistance e loot, mas ainda nao formam runtime completo.

Evidencia:

```text
Assets/_Game/Scripts/Equipment/EquipmentManager.cs
Assets/_Game/Scripts/Equipment/EquipmentDataSO.cs
Assets/_Game/Scripts/Equipment/DurabilityManager.cs
Assets/_Game/Scripts/Equipment/EnvironmentalResistanceManager.cs
Assets/_Game/Scripts/Loot/LootTableSO.cs
docs/specs/implementados/spec_tools_001_tools_equipment_hotbar_parcial.md
```

Specs 03, 06 e 09 definiram inventory slots, item actions, modal stack, stamina e HUD. Esta spec deve completar equipment/durability sem quebrar inventory, hotbar/action slots, stamina ou sistemas futuros de combate.

## Pre-condicoes

Implementar runtime somente depois de specs 02-09 estarem realmente implementadas:

```text
02 - save schema migration
03 - inventory slots/capacity/painel de itens
04 - farm irrigacao/solo/menu contextual
05 - world activities/fishing/trees/pickups/loot
06 - economy/shop/dialogue modal
07 - crafting/workstations/modal stack
08 - town/npc/dialogue
09 - hunger/stamina/status/time
```

Antes de alterar codigo, revalidar:

```text
Assets/_Game/Scripts/Equipment/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Loot/**
Assets/_Game/Scripts/Player/**
Assets/_Game/Scripts/UI/**
Assets/_Game/Scripts/UI/Modal/**
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/Combat/**
Assets/_Game/Scripts/Core/Events/**
```

Se inventory ainda nao suporta item instance/non-stackable, esta spec deve implementar a migracao necessaria de forma compativel com a spec 03/02.

## Problema

Gaps atuais:

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

## Objetivo

Implementar equipment runtime MVP com:

- HUD/painel posicional de equipamento;
- slots de mao esquerda e mao direita;
- tool ocupando uma das maos;
- selecao de slot de equipamento abrindo inventory HUD/modal para escolher item valido;
- hotkeys 1-6 preservando uso/selecoes ja definidos para a mao ativa/quick slots;
- equipamentos non-stackable com `ItemInstanceId`;
- durabilidade por item instance;
- broken state e auto-unequip;
- repair kit MVP;
- stats derivados com modificadores flat;
- AttackSpeed base e modificadores por tipo de arma/atributos;
- environmental resistance MVP;
- LootTableSO gerando stackables e equipment instances;
- save/load seguro.

## Decisoes aprovadas

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

## Equipment slots e HUD posicional

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

## Maos, tools e hotkeys

### LeftHand / RightHand

- Armas e tools ocupam uma das maos.
- Uma tool equipada em mao deve ser tratada como equipamento e como ferramenta ativa quando selecionada/acionada.
- `LeftHand` e `RightHand` nao podem conter dois itens iguais por duplicacao de binding.
- Se item for removido do inventory, slot deve limpar binding de forma segura.

### Hotkeys 1-6

- Hotkeys `1,2,3,4,5,6` devem continuar respeitando o comportamento ja definido nas specs anteriores.
- Esta spec nao deve remover hotbar/quick slots existentes.
- A mao/slot ativo usado por esses atalhos deve ser resolvido sem duplicar item.
- Se a implementacao atual usa hotbar para tool/item selection, equipment deve se integrar sem criar um segundo inventario.
- Se houver conflito entre hotbar e mao equipada, registrar regra clara: hotkey seleciona item/slot ativo e acao usa o item se ele estiver equipado ou puder ser equipado no slot de mao valido.

## Inventory e ItemInstanceId

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

## Stats derivados

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

Hooks futuros:

```text
Mana
ElementalResistance
CritChance
Luck
AttackSpeed final tuning
PercentModifiers genericos
Affixes
Rarity
```

Regras:

- Equipamento soma modificadores flat para stats MVP, exceto AttackSpeed que usa formula especifica abaixo.
- Ao equipar/unequip, recalcular stats derivados.
- Alteracoes de MaxHP/MaxStamina devem ajustar valores atuais de forma segura sem extrapolar maximo.
- Stats base do player devem continuar definidos fora do equipment.

## AttackSpeed

Modelo MVP:

```text
BaseAttackSpeed = 1.0
WeaponTypeAttackSpeedMultiplier = percentual por tipo de arma
AttributeAttackSpeedBonus = percentual por atributo relevante
FinalAttackSpeed = BaseAttackSpeed * WeaponTypeMultiplier * AttributeMultiplier
```

Categorias:

```text
LightWeapon
HeavyWeapon
NeutralWeapon
ToolAsWeapon opcional
```

Regras:

- Armas leves usam Dexterity para bonus percentual de AttackSpeed.
- Armas pesadas usam Strength para bonus percentual de AttackSpeed.
- Armas neutras usam apenas weapon type multiplier no MVP.
- Se atributos Strength/Dexterity ainda nao existirem no runtime, preparar campos/hooks e usar valor default 0.
- AttackSpeed nao deve quebrar punch/combat MVP; se combate final ainda nao existir, expor o stat derivado para spec 12.
- Valores finais de balance podem mudar; contrato/formula precisa existir.

Campos sugeridos:

```text
WeaponWeightClass
AttackSpeedMultiplier
DexterityAttackSpeedPercentPerPoint
StrengthAttackSpeedPercentPerPoint
```

## Durability

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

## Repair MVP

MVP usa `RepairKit`.

Regras:

- Consumir `RepairKit` em um item selecionado restaura 50% da durabilidade maxima.
- Nao ultrapassar `DurabilityMax`.
- RepairKit so e consumido se reparo aplicar algum ganho real.
- Broken item reparado deixa de estar broken se `DurabilityCurrent > 0`.
- Repair por vendedor/forge/workstation fica futuro.

## Environmental resistance

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

Regras:

- Player soma resistencias ambientais dos equipamentos.
- Se resistencia total do tipo >= RequiredResistance, nao aplica efeito negativo.
- Se resistencia total < RequiredResistance, aplica `EnvironmentalExposure`/status/debuff MVP.
- Nao implementar pipeline completo de dano elemental/resistencias da spec 11 aqui.

Cenario minimo testavel:

```text
Cave test EnvironmentZone
HazardType = Toxic
RequiredResistance = 5
Sem resistencia: aplica debuff/damage over time leve
Com ToxicResistance >= 5: bloqueia efeito
```

## Loot runtime

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

## Save/load

Persistir usando IDs e tipos simples.

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

Environment/status save, se ativo:

```text
EnvironmentalExposureSaveData opcional
- HazardType
- RemainingSeconds
- SourceZoneId opcional
```

Regras:

- Nao serializar `EquipmentDataSO`, `ItemDataSO`, `LootTableSO`, `GameObject`, `Transform`, `MonoBehaviour`, `Sprite`, `Collider` ou `Rigidbody`.
- Derived stats sao recalculados no load.
- Se `ItemInstanceId` equipado nao existir no inventory ao carregar, limpar slot e logar warning.
- Se item esta broken ao carregar, nao deve ficar equipado.

## Eventos

Usar existentes se houver equivalentes. Criar somente se necessario:

```text
EquipmentSlotChangedEvent
EquipmentStatsChangedEvent
ItemEquippedEvent
ItemUnequippedEvent
DurabilityChangedEvent
ItemBrokenEvent
ItemRepairedEvent
EnvironmentalExposureStartedEvent
EnvironmentalExposureEndedEvent
LootRolledEvent
LootSpawnedEvent
InventoryChangedEvent
StaminaChangedEvent opcional dependente
```

Nao duplicar eventos de inventory/loot existentes.

## UI/modal

Equipment HUD/panel:

- mostra slots posicionais;
- mostra item equipado por slot;
- mostra durability quando item possui durability;
- mostra broken state;
- mostra stats derivados basicos;
- selecionar slot abre inventory modal filtrado;
- respeita modal stack;
- nao sobrepoe DialogueModal, ShopModal, CraftingModal ou InventoryPanel sem controle.

Debug minimo permitido, mas deve refletir runtime real.

## Invariantes anti-regressao

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

Se uma integracao ainda nao existir, bloquear com feedback/pendencia clara em vez de criar fallback que duplica item, perde durabilidade ou quebra save.

## Criterios de aceite

- Equipment HUD posicional existe e mostra slots MVP.
- LeftHand e RightHand existem.
- Tool pode ocupar uma das maos.
- Selecionar slot abre inventory filtrado para equipar item valido.
- Hotkeys `1-6` preservam comportamento existente.
- Equipaveis/duraveis sao non-stackable e possuem `ItemInstanceId`.
- Equipar item nao remove item do inventory, apenas cria binding.
- Unequip limpa binding e mantem item no inventory.
- Stats derivados flat recalculam ao equipar/unequip.
- AttackSpeed base 1.0 existe e considera tipo de arma + Strength/Dexterity quando disponivel.
- Durability reduz por uso efetivo.
- Durability 0 deixa item broken, auto-unequipa e bloqueia uso/equip.
- RepairKit restaura 50% da durabilidade maxima sem ultrapassar max.
- Environmental resistance Toxic/Cold/Heat existe.
- Cave toxic test zone aplica efeito sem resistencia e bloqueia com resistencia suficiente.
- LootTableSO gera stackables e equipment instances.
- Save/load preserva equipment slots, item instances, durability e broken state.
- Nenhum item das invariantes anti-regressao e quebrado.
- Validacao documental e Unity compile validation registradas.

---

# /speckit.plan

## Arquitetura

Sistemas afetados:

```text
Assets/_Game/Scripts/Equipment/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Loot/**
Assets/_Game/Scripts/Player/**
Assets/_Game/Scripts/Combat/**
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/UI/Equipment/**
Assets/_Game/Scripts/UI/Modal/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Data/Equipment/**
Assets/_Game/Data/Loot/**
```

Managers/bridges Unity devem ser finos. Equipment validation, stat calculation, durability, repair e loot instance generation devem ficar fora de MonoBehaviour pesado quando possivel.

## Ordem segura de implementacao

1. Revalidar EquipmentManager, InventorySlot, ItemDataSO, LootTableSO e SaveData atuais.
2. Confirmar specs 02-09 implementadas antes de runtime.
3. Adicionar suporte a ItemInstanceId/non-stackable no inventory/save com migration segura.
4. Criar EquipmentSlot/EquipmentSaveData.
5. Implementar equip/unequip sem remover item do inventory.
6. Implementar stat calculation flat e AttackSpeed MVP.
7. Implementar durability loss/broken/auto-unequip.
8. Implementar RepairKit MVP.
9. Implementar environmental resistance MVP.
10. Implementar equipment instance generation no loot runtime.
11. Implementar Equipment HUD posicional e inventory filtered selection.
12. Validar anti-regressao e atualizar tracking.

## Fluxos

### Equipar via Equipment HUD

```text
Abrir Equipment HUD
Selecionar slot posicional
Abrir InventoryPanel filtrado por slot/categoria
Selecionar item valido
Validar ItemInstanceId, categoria e not broken
Criar binding EquipmentSlot -> ItemInstanceId
Recalcular stats
Publicar eventos
```

### Usar tool/arma equipada

```text
Hotkey/acao seleciona item/mao conforme regras existentes
Validar item equipado ou ativo
Validar stamina/target
Executar acao
Reduzir durability somente se acao efetiva
Se durability chega a 0, marcar broken e auto-unequip
```

### RepairKit

```text
Selecionar RepairKit/use
Selecionar item danificado/broken valido
Validar que reparo aumenta durability
Consumir RepairKit
Aumentar durability em 50% max
Atualizar broken state
Publicar eventos
```

### Environmental zone

```text
Player entra em EnvironmentZone
Somar resistencia ambiental dos equipamentos
Se resistencia insuficiente, aplicar exposure status/debuff por tick
Se resistencia suficiente, nao aplicar ou remover exposure
```

### Loot equipment

```text
LootTableSO rola entry de equipment
Criar ItemInstanceId
Setar durability 100%
Adicionar inventory ou spawn pickup persistente
Salvar instance quando necessario
```

## Riscos de regressao

- Duplicar item ao equipar.
- Remover item do inventory e quebrar save/hotbar.
- Stackar weapons/tools com durability.
- Hotkeys 1-6 perderem comportamento existente.
- Durability reduzir em acao falha.
- Broken item continuar equipado.
- AttackSpeed afetar combate MVP de forma instavel.
- Equipment HUD sobrepor modais sem controle.
- Loot equipment sem instance/durability persistente.

## Mitigacao

- Equip por binding, nao por transferencia de item.
- Equipaveis non-stackable com ItemInstanceId obrigatorio.
- Validar acao antes de reduzir durability.
- Auto-unequip atomico em broken.
- AttackSpeed exposto como stat derivado, consumo final por combate pode ficar para spec 12.
- Modal stack para HUD/panels.
- Save/load recalcula derived stats.

---

# /speckit.tasks

## Tasks

- [ ] Revalidar Equipment/Inventory/Loot/Save/UI reais antes de alterar runtime.
- [ ] Confirmar specs 02-09 implementadas antes de runtime.
- [ ] Implementar `ItemInstanceId` para equipaveis/duraveis.
- [ ] Marcar equipaveis/duraveis como non-stackable.
- [ ] Criar/ajustar EquipmentSlot enum com LeftHand/RightHand e slots de armor/acessorio.
- [ ] Implementar equip/unequip por binding no inventory.
- [ ] Implementar Equipment HUD posicional.
- [ ] Implementar selecao de slot abrindo inventory filtrado.
- [ ] Preservar hotkeys `1-6` e comportamento existente.
- [ ] Implementar stat calculation flat.
- [ ] Implementar AttackSpeed base/tipo de arma/Strength/Dexterity hooks.
- [ ] Implementar durability loss por uso efetivo.
- [ ] Implementar Broken state e auto-unequip.
- [ ] Implementar RepairKit MVP.
- [ ] Implementar EnvironmentalResistance Toxic/Cold/Heat.
- [ ] Implementar EnvironmentZone Toxic testavel.
- [ ] Implementar LootTableSO gerando equipment instances.
- [ ] Implementar save/load de equipment slots e item instances.
- [ ] Atualizar docs implementados/status/registries/refinements/log.

## Arquivos permitidos

```text
Assets/_Game/Scripts/Equipment/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Loot/**
Assets/_Game/Scripts/Player/**
Assets/_Game/Scripts/Combat/**
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/UI/Equipment/**
Assets/_Game/Scripts/UI/Modal/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Data/Equipment/**
Assets/_Game/Data/Loot/**
docs/specs/**
docs/refinements/**
docs/IMPLEMENTATION_STATUS.md
PROJECT_LOG.md
```

## Arquivos proibidos

```text
docs_old/**
Packages/**
ProjectSettings/**
```

## Definition of Done

- Equipment runtime funcional com slots posicionais.
- Durability e broken state funcionam.
- RepairKit MVP funciona.
- Stats derivados e AttackSpeed expostos.
- Environmental resistance MVP funciona.
- Loot runtime gera equipment instances.
- Save/load preserva estado.
- Invariantes anti-regressao preservadas.
- Validacao documental e Unity registrada.

## Validacao obrigatoria

Rodar:

```powershell
.\tools\docs\validate_docs.ps1
.\tools\unity\RunUnityCompileValidation.ps1
.\tools\unity\ScanUnityLogs.ps1
```

Play Mode minimo:

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
