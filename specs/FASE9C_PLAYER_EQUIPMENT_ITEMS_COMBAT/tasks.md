# Tasks — FASE9C Player Equipment, Item Use e Combat Loadout

> **Feature:** FASE9C_PLAYER_EQUIPMENT_ITEMS_COMBAT  
> **Spec:** `spec.md`  
> **Plan:** `plan.md`

---

## PR-123 — Equipment slot contracts

### Escopo

Criar contratos sem alterar gameplay.

### Arquivos esperados

- `Assets/_Game/Scripts/Equipment/EquipmentSlotType.cs`
- `Assets/_Game/Scripts/Equipment/EquipmentSaveData.cs`
- `Assets/_Game/Scripts/Core/Events/EquipmentChangedEvent.cs`
- `Assets/_Game/Scripts/Core/Events/ItemEquippedEvent.cs`
- `Assets/_Game/Scripts/Core/Events/ItemUnequippedEvent.cs`

### Critérios

- [ ] Compila.
- [ ] Eventos usam apenas tipos simples.
- [ ] Nenhuma cena alterada.
- [ ] Nenhum comportamento alterado.

---

## PR-124 — Weapon/Magic/Consumable contracts

### Escopo

Criar contratos de arma, consumível e status application.

### Arquivos esperados

- `Assets/_Game/Scripts/Combat/Weapons/WeaponType.cs`
- `Assets/_Game/Scripts/Combat/Weapons/WeaponDataSO.cs`
- `Assets/_Game/Scripts/Combat/Weapons/WeaponDatabaseSO.cs`
- `Assets/_Game/Scripts/Combat/Status/StatusApplicationData.cs`
- `Assets/_Game/Scripts/Items/Consumables/ConsumableDataSO.cs`
- `Assets/_Game/Scripts/Items/Consumables/ConsumableDatabaseSO.cs`

### Critérios

- [ ] CreateAssetMenu funciona.
- [ ] Databases usam IDs estáveis.
- [ ] Nenhuma lógica runtime integrada ainda.

---

## PR-125 — Data assets iniciais

### Escopo

Criar itens e assets MVP.

### Assets esperados

- `item_weapon_sword_basic`
- `weapon_sword_basic`
- `item_weapon_bow_basic`
- `weapon_bow_basic`
- `item_ammo_arrow_basic`
- `item_magic_fire_spark`
- `weapon_magic_fire_spark`
- `item_potion_small_hp`
- `consumable_potion_small_hp`

### Critérios

- [ ] ItemDatabase inclui todos os itens.
- [ ] WeaponDatabase inclui sword/bow/fire magic.
- [ ] ConsumableDatabase inclui potion.
- [ ] Validator cobre dados mínimos.

---

## PR-126 — EquipmentManager equip/unequip

### Escopo

Implementar equipar/desequipar com regras de loadout.

### Arquivos esperados

- `Assets/_Game/Scripts/Equipment/EquipmentManager.cs`
- alterações em `GameBootstrap.cs`
- alterações nos geradores de cena para incluir EquipmentManager
- alterações em DebugHud para exibir slots

### Critérios

- [ ] Equipar espada funciona.
- [ ] Desequipar espada funciona.
- [ ] Equipar arco remove magia.
- [ ] Equipar magia com arco falha.
- [ ] Equipar magia com espada funciona.
- [ ] Equipar não consome item.

---

## PR-127 — Save/load equipment

### Escopo

Persistir equipamento.

### Arquivos esperados

- `SaveData.cs`
- `SaveManager.cs`
- `EquipmentManager.cs`

### Critérios

- [ ] Save guarda slots.
- [ ] Load restaura slots.
- [ ] Save antigo sem Equipment carrega.
- [ ] Item inexistente limpa slot com warning.

---

## PR-128 — PlayerCombatController melee + bow

### Escopo

Criar combate por equipamento para espada e arco.

### Arquivos esperados

- `Assets/_Game/Scripts/Combat/PlayerCombatController.cs`
- `Assets/_Game/Scripts/Combat/Projectiles/ProjectileController.cs`
- alterações em CreateMvpCaveScene/CreateMvpFarmScene se necessário

### Critérios

- [ ] Unarmed mantém dano equivalente ao soco.
- [ ] Espada causa mais dano que soco.
- [ ] Arco dispara projectile.
- [ ] Arco consome flecha.
- [ ] Arco sem flecha falha sem erro vermelho.

---

## PR-129 — Magic fire + Burn status

### Escopo

Implementar magia de fogo e Burn.

### Arquivos esperados

- `Assets/_Game/Scripts/Combat/Status/StatusEffectType.cs`
- `Assets/_Game/Scripts/Combat/Status/EnemyStatusReceiver.cs`
- `ProjectileController.cs`
- `PlayerCombatController.cs`

### Critérios

- [ ] Fire Spark dispara projectile.
- [ ] Projectile aplica dano inicial.
- [ ] Projectile aplica Burn.
- [ ] Burn dura 3s.
- [ ] Burn causa dano contínuo.
- [ ] Reaplicar Burn renova duração.

---

## PR-130 — ItemUseManager + potion

### Escopo

Implementar uso de consumível.

### Arquivos esperados

- `Assets/_Game/Scripts/Items/ItemUseManager.cs`
- alterações em GameBootstrap
- alterações em DebugHud ou input debug

### Critérios

- [ ] Poção pode ser equipada na hotbar.
- [ ] Poção selecionada pode ser usada.
- [ ] HP aumenta.
- [ ] Inventário remove 1 poção.
- [ ] Sem poção, uso falha sem erro vermelho.

---

## PR-131 — Debug selection MVP

### Escopo

Criar forma mínima de seleção/equipamento antes da UI final.

### Opções aceitáveis

- hotkeys debug;
- pontos interagíveis debug;
- painel OnGUI temporário.

### Critérios

- [ ] Jogador consegue equipar espada no Play Mode.
- [ ] Jogador consegue equipar arco no Play Mode.
- [ ] Jogador consegue equipar magia no Play Mode.
- [ ] Jogador consegue usar poção no Play Mode.
- [ ] HUD mostra estado.

---

## PR-132 — Validator + handoff

### Escopo

Validar dados e documentar entrega.

### Critérios

- [ ] Validator confirma WeaponDatabase.
- [ ] Validator confirma ConsumableDatabase.
- [ ] Validator confirma IDs no ItemDatabase.
- [ ] Validator confirma Burn/status mínimo.
- [ ] Handoff documenta smoke test completo.

---

## Smoke test final

- [ ] Gerar cenas.
- [ ] Abrir CaveScene.
- [ ] Equipar espada.
- [ ] Atacar Slime com espada.
- [ ] Equipar magia junto da espada.
- [ ] Lançar Fire Spark.
- [ ] Confirmar Burn 3s.
- [ ] Equipar arco.
- [ ] Confirmar magia removida.
- [ ] Disparar flecha.
- [ ] Confirmar ammo consumida.
- [ ] Usar poção.
- [ ] Salvar/carregar.
- [ ] Confirmar loadout restaurado.
- [ ] Console sem erro vermelho.

