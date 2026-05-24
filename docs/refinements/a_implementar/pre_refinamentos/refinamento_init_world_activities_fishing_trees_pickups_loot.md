# refinamento_init_world_activities_fishing_trees_pickups_loot

> **Status:** Refinamento inicial a implementar
> **Spec futura sugerida:** `spec_world_activities_fishing_trees_pickups_loot.md`
> **Objetivo:** completar Ã¡rvores, pesca, pickups persistentes e loot tables de atividades do mundo.

---

## 1. Estado atual

O projeto jÃ¡ possui atividades MVP: cortar Ã¡rvore, pescar peixe comum e coletar pickups persistentes.

EvidÃªncia:

```text
Assets/_Game/Scripts/World/**
Assets/_Game/Scripts/Fishing/**
Assets/_Game/Scripts/Save/SaveManager.cs
docs/specs/implementados/spec_farm_003_arvores_pesca_pickups_world_activities.md
docs/specs/implementados/spec_world_001_pickups_persistentes_save_load.md
```

---

## 2. Gaps

- Fishing ainda nÃ£o tem minigame real.
- Fishing nÃ£o usa loot tables por bioma/horÃ¡rio/clima/vara.
- Ãrvores nÃ£o tÃªm respawn/regrowth configurÃ¡vel completo.
- Pickups persistentes dependem de validaÃ§Ã£o Unity/Play Mode.
- Loot tables ainda nÃ£o sÃ£o fonte unificada para Ã¡rvores, pesca, cave resources e inimigos.
- Falta integraÃ§Ã£o com stamina/durability.

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
- HP da Ã¡rvore por tier de axe.
- Drops via loot table.
- Stump/regrowth opcional.
- PersistÃªncia de chopped/regrowth timer.

### Pickups

- Garantir ID estÃ¡vel por scene + index + itemId.
- Evitar destroy permanente antes de registrar estado.
- Save/load deve restaurar coletados e nÃ£o coletados.

### Loot

- Unificar drops simples com `LootTableSO`.
- Permitir pesos, quantidade min/max e condiÃ§Ãµes.

---

## 4. Arquivos provÃ¡veis

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
- [ ] Ãrvores usam HP/tier/drops configurÃ¡veis.
- [ ] Pickups persistem sem reaparecer indevidamente.
- [ ] Loot table Ã© usada por pelo menos Ã¡rvore, fishing ou enemy.
- [ ] Save/load preserva estado das world activities.

---

## 6. ValidaÃ§Ã£o

1. Cortar Ã¡rvore, salvar, recarregar e validar estado.
2. Coletar pickup, salvar, recarregar e validar que nÃ£o reaparece.
3. Pescar com e sem fishing rod.
4. Validar drops por loot table.
5. Testar em FarmScene e eventual CaveScene se aplicÃ¡vel.
