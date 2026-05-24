# refinamento_init_world_activities_fishing_trees_pickups_loot

> Status: Refinamento inicial a implementar
> Spec futura relacionada: `docs/specs/implementados/spec_world_002_activities_fishing_trees_pickups_loot.md`
> Objetivo: completar atividades world com pesca, arvores e loot.

---

## 1. Estado atual

World ja existe com pickups persistentes. Necessario completar pesca, arvores farmeaveis e loot tables.

---

## 2. Gaps

- Pesca sem mecanica/timing.
- Arvores sem HP/regrowth.
- LootTable nao gera instances com durabilidade.
- Spawner dinamico nao implementado.
- Cave fishing ainda pendente.

---

## 3. Decisoes aprovadas

- Pesca com timing minimo (catch window).
- Arvores com HP, regrowth, loot garantido.
- LootTable com stackables e equipment.
- Persistencia de pickups entre cenas.
- Spawner dinamico fica para futuro.
