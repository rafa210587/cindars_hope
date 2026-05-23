# refinamento_init_world_activities_fishing_trees_pickups_loot

> **Status:** Refinamento inicial a implementar  
> **Spec futura sugerida:** `spec_world_activities_fishing_trees_pickups_loot.md`  
> **Objetivo:** completar árvores, pesca, pickups persistentes e loot tables de atividades do mundo.

---

## 1. Estado atual

O projeto já possui atividades MVP: cortar árvore, pescar peixe comum e coletar pickups persistentes.

Evidência:

```text
Assets/_Game/Scripts/World/**
Assets/_Game/Scripts/Fishing/**
Assets/_Game/Scripts/Save/SaveManager.cs
docs/specs/implementados/spec_farm_003_arvores_pesca_pickups_world_activities.md
docs/specs/implementados/spec_world_001_pickups_persistentes_save_load.md
```

---

## 2. Gaps

- Fishing ainda não tem minigame real.
- Fishing não usa loot tables por bioma/horário/clima/vara.
- Árvores não têm respawn/regrowth configurável completo.
- Pickups persistentes dependem de validação Unity/Play Mode.
- Loot tables ainda não são fonte unificada para árvores, pesca, cave resources e inimigos.
- Falta integração com stamina/durability.

---

## 3. Escopo esperado

### Fishing

- Criar `FishingSpotDataSO`.
- Criar `FishingLootTableSO` ou reutilizar `LootTableSO` oficial.
- Validar tool equipada: fishing rod.
- Criar minigame simples ou timing window.
- Resultado: catch, fail, rare catch.
- Eventos: `FishingStarted`, `FishCaught`, `FishingFailed`.

### Trees

- TreeDataSO por tipo.
- HP da árvore por tier de axe.
- Drops via loot table.
- Stump/regrowth opcional.
- Persistência de chopped/regrowth timer.

### Pickups

- Garantir ID estável por scene + index + itemId.
- Evitar destroy permanente antes de registrar estado.
- Save/load deve restaurar coletados e não coletados.

### Loot

- Unificar drops simples com `LootTableSO`.
- Permitir pesos, quantidade min/max e condições.

---

## 4. Arquivos prováveis

```text
Assets/_Game/Scripts/World/TreeNode.cs
Assets/_Game/Scripts/World/ItemPickup.cs
Assets/_Game/Scripts/World/ItemPickupRegistry.cs
Assets/_Game/Scripts/Fishing/**
Assets/_Game/Scripts/Loot/LootTableSO.cs
Assets/_Game/Scripts/Equipment/EquipmentManager.cs
Assets/_Game/Scripts/Save/SaveData.cs
```

---

## 5. Definition of Done

- [ ] Fishing usa tool equipada e loot table.
- [ ] Fishing tem outcome claro e eventos.
- [ ] Árvores usam HP/tier/drops configuráveis.
- [ ] Pickups persistem sem reaparecer indevidamente.
- [ ] Loot table é usada por pelo menos árvore, fishing ou enemy.
- [ ] Save/load preserva estado das world activities.

---

## 6. Validação

1. Cortar árvore, salvar, recarregar e validar estado.
2. Coletar pickup, salvar, recarregar e validar que não reaparece.
3. Pescar com e sem fishing rod.
4. Validar drops por loot table.
5. Testar em FarmScene e eventual CaveScene se aplicável.
