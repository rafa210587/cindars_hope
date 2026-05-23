# SpecKit — FASE9J Cave Run Entry, Loadout, HUD & Failure Flow

> **Feature:** `FASE9J_CAVE_RUN_ENTRY_LOADOUT_HUD_AND_FAILURE_FLOW`  
> **Status:** especificação funcional aprovada para planejamento.  
> **Fonte de design:** `docs_old/FASE9J_CAVE_RUN_ENTRY_LOADOUT_HUD_AND_FAILURE_FLOW_SPEC_v1.0.md`

---

## 1. User story

Como jogador, quero entrar na cave por checkpoints seguros, preparar meu loadout, entender os riscos, explorar, sair vivo com loot ou recuperar meu corpo após morrer, para que a run da cave tenha tensão, consequência e progressão justa.

---

## 2. Objetivos funcionais

### O1 — Cave entry menu

Ao interagir com a entrada da cave, abrir menu com checkpoints liberados, loadout e warnings.

### O2 — Checkpoint safe room

Entrar por checkpoint começa exatamente no CaveLevel do checkpoint, mas sempre em sala segura.

### O3 — Loadout validation

Menu deve mostrar arma, shield/offhand, armor, accessory, ammo, magic item, hotbar, consumables, tools, food e return item.

### O4 — CaveRunHud

Criar HUD OnGUI MVP próprio da cave, separado conceitualmente do DebugHud.

### O5 — Controlled exit

Jogador só pode sair por pontos de saída ou item de retorno.

### O6 — Return item

Item de retorno permite sair sem morte, preserva loot e não troca CaveRunSeed.

### O7 — Failure flow

Ao morrer na cave, jogador volta para a Fonte de Anya na Farm, perde itens/gold carregados para corpo recuperável e perde XP acumulado no nível atual.

### O8 — Corpse recovery

Só existe um corpo recuperável: o da última morte. Nova morte substitui o corpo anterior.

### O9 — CaveRunSeed rules

Sair vivo não muda CaveRunSeed. Morrer/KO muda CaveRunSeed. Novo jogo cria nova seed.

### O10 — Boss unlock

Boss derrotado libera checkpoint/avanço imediatamente e o unlock persiste após morte.

---

## 3. Regras de negócio

### R1 — Death loss

Ao morrer na cave:

```text
LostGold
LostInventoryItems
LostEquipmentItems
LostAmmo
```

vão para `PlayerCorpseRecoverySaveData`.

### R2 — XP loss

Ao morrer:

```text
XpInCurrentLevel = 0
Level permanece igual
Attribute/Skill points já ganhos/gastos permanecem
```

### R3 — Progression protected

Morte não remove:

```text
unlocked checkpoints
defeated boss flags
deepest level reached
recipes unlocked
quest flags
skills learned
```

### R4 — Corpse accessibility

Como morte troca CaveRunSeed, corpse deve aparecer em `CorpseRecoveryRoom` garantida no mesmo CaveLevel.

### R5 — Broken items

Durability 0 impede uso.

### R6 — Cave HUD

CaveRunHud deve mostrar dados jogáveis mínimos, não apenas debug.

---

## 4. Entidades funcionais

- `CaveEntryMenu`
- `CaveRunHud`
- `AnyaFountain`
- `PlayerCorpseRecovery`
- `CaveRunFailureSaveData`
- `PlayerCorpseRecoverySaveData`
- `CorpseRecoveryRoom`
- `ReturnItem`
- `CheckpointSafeRoom`

---

## 5. Critérios de aceite

### CA1 — Checkpoints

CaveEntryMenu mostra checkpoints liberados.

### CA2 — Safe room

Checkpoint inicia no CaveLevel exato em sala segura.

### CA3 — Loadout

Loadout mostra arma, shield/offhand, armor, accessory, ammo, magic item, tools e consumables.

### CA4 — Warnings

CaveEntryMenu mostra warnings de resistência, tool tier, durability, ammo e retorno.

### CA5 — HUD

CaveRunHud mostra HP, Mana, Stamina, Hunger, CaveLevel, Ammo, Durability e Status.

### CA6 — Sair vivo

Ao sair vivo, CaveRunSeed permanece.

### CA7 — Morte

Ao morrer, player respawna na Fonte de Anya na Farm.

### CA8 — Corpse

Ao morrer, gold/inventory/equipment/ammo carregados vão para corpo recuperável.

### CA9 — XP

Ao morrer, XP do nível atual zera.

### CA10 — Seed

Ao morrer, CaveRunSeed muda.

### CA11 — Progression persists

Checkpoints e boss defeated persistem após morte.

### CA12 — Último corpo

Só o último corpo pode ser recuperado.

### CA13 — Corpo acessível

Corpo é acessível no CaveLevel da morte mesmo após regenerar run.

### CA14 — Return item

Item de retorno permite sair sem morte.

### CA15 — Broken item

Durabilidade 0 impede uso.

### CA16 — Boss unlock

Boss derrotado libera checkpoint/avanço imediatamente.

---

## 6. Non-goals

Fora desta spec:

- UI final polida;
- animação final de morte;
- cutscene da Fonte de Anya;
- sistema completo de storage na safe room;
- merchant da cave;
- campfire/rest completo;
- perda parcial configurável por dificuldade;
- multiplayer/co-op corpse recovery;
- snapshot completo de layout antigo para corpse.

---

## 7. MVP recomendado

1. CaveEntryMenu OnGUI simples com checkpoints e warnings.
2. CaveRunHud OnGUI separado do DebugHud.
3. Fonte de Anya como respawn point na Farm.
4. Failure flow: morte → corpse save → XP current level zerado → Farm/Fonte de Anya.
5. CorpseRecoveryRoom no mesmo CaveLevel após nova run seed.
6. Return item simples.
7. Durability 0 bloqueia uso.
8. Boss defeated libera checkpoint imediatamente.

---

## 8. Dependências

- FASE9F Cave Resources/Encounters.
- FASE9G Cave Bestiary/Faction Locks.
- FASE9H Cave Loot/Crafting/Equipment Progression.
- FASE9I Player Combat/Weapons/Magic/Skill Trees.
- Save Schema/Migration da FASE9E.

---

## 9. Pronto para Plan quando

- Cave procedural foundation existir ou estiver planejada.
- Save schema suportar cave state e player progression.
- Equipment/inventory data estiver apto a representar itens carregados/equipados.


