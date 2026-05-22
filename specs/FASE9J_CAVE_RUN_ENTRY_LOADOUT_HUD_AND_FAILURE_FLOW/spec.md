# SpecKit â€” FASE9J Cave Run Entry, Loadout, HUD & Failure Flow

> **Feature:** `FASE9J_CAVE_RUN_ENTRY_LOADOUT_HUD_AND_FAILURE_FLOW`  
> **Status:** especificaÃ§Ã£o funcional aprovada para planejamento.  
> **Fonte de design:** `docs_old/FASE9J_CAVE_RUN_ENTRY_LOADOUT_HUD_AND_FAILURE_FLOW_SPEC_v1.0.md`

---

## 1. User story

Como jogador, quero entrar na cave por checkpoints seguros, preparar meu loadout, entender os riscos, explorar, sair vivo com loot ou recuperar meu corpo apÃ³s morrer, para que a run da cave tenha tensÃ£o, consequÃªncia e progressÃ£o justa.

---

## 2. Objetivos funcionais

### O1 â€” Cave entry menu

Ao interagir com a entrada da cave, abrir menu com checkpoints liberados, loadout e warnings.

### O2 â€” Checkpoint safe room

Entrar por checkpoint comeÃ§a exatamente no CaveLevel do checkpoint, mas sempre em sala segura.

### O3 â€” Loadout validation

Menu deve mostrar arma, shield/offhand, armor, accessory, ammo, magic item, hotbar, consumables, tools, food e return item.

### O4 â€” CaveRunHud

Criar HUD OnGUI MVP prÃ³prio da cave, separado conceitualmente do DebugHud.

### O5 â€” Controlled exit

Jogador sÃ³ pode sair por pontos de saÃ­da ou item de retorno.

### O6 â€” Return item

Item de retorno permite sair sem morte, preserva loot e nÃ£o troca CaveRunSeed.

### O7 â€” Failure flow

Ao morrer na cave, jogador volta para a Fonte de Anya na Farm, perde itens/gold carregados para corpo recuperÃ¡vel e perde XP acumulado no nÃ­vel atual.

### O8 â€” Corpse recovery

SÃ³ existe um corpo recuperÃ¡vel: o da Ãºltima morte. Nova morte substitui o corpo anterior.

### O9 â€” CaveRunSeed rules

Sair vivo nÃ£o muda CaveRunSeed. Morrer/KO muda CaveRunSeed. Novo jogo cria nova seed.

### O10 â€” Boss unlock

Boss derrotado libera checkpoint/avanÃ§o imediatamente e o unlock persiste apÃ³s morte.

---

## 3. Regras de negÃ³cio

### R1 â€” Death loss

Ao morrer na cave:

```text
LostGold
LostInventoryItems
LostEquipmentItems
LostAmmo
```

vÃ£o para `PlayerCorpseRecoverySaveData`.

### R2 â€” XP loss

Ao morrer:

```text
XpInCurrentLevel = 0
Level permanece igual
Attribute/Skill points jÃ¡ ganhos/gastos permanecem
```

### R3 â€” Progression protected

Morte nÃ£o remove:

```text
unlocked checkpoints
defeated boss flags
deepest level reached
recipes unlocked
quest flags
skills learned
```

### R4 â€” Corpse accessibility

Como morte troca CaveRunSeed, corpse deve aparecer em `CorpseRecoveryRoom` garantida no mesmo CaveLevel.

### R5 â€” Broken items

Durability 0 impede uso.

### R6 â€” Cave HUD

CaveRunHud deve mostrar dados jogÃ¡veis mÃ­nimos, nÃ£o apenas debug.

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

## 5. CritÃ©rios de aceite

### CA1 â€” Checkpoints

CaveEntryMenu mostra checkpoints liberados.

### CA2 â€” Safe room

Checkpoint inicia no CaveLevel exato em sala segura.

### CA3 â€” Loadout

Loadout mostra arma, shield/offhand, armor, accessory, ammo, magic item, tools e consumables.

### CA4 â€” Warnings

CaveEntryMenu mostra warnings de resistÃªncia, tool tier, durability, ammo e retorno.

### CA5 â€” HUD

CaveRunHud mostra HP, Mana, Stamina, Hunger, CaveLevel, Ammo, Durability e Status.

### CA6 â€” Sair vivo

Ao sair vivo, CaveRunSeed permanece.

### CA7 â€” Morte

Ao morrer, player respawna na Fonte de Anya na Farm.

### CA8 â€” Corpse

Ao morrer, gold/inventory/equipment/ammo carregados vÃ£o para corpo recuperÃ¡vel.

### CA9 â€” XP

Ao morrer, XP do nÃ­vel atual zera.

### CA10 â€” Seed

Ao morrer, CaveRunSeed muda.

### CA11 â€” Progression persists

Checkpoints e boss defeated persistem apÃ³s morte.

### CA12 â€” Ãšltimo corpo

SÃ³ o Ãºltimo corpo pode ser recuperado.

### CA13 â€” Corpo acessÃ­vel

Corpo Ã© acessÃ­vel no CaveLevel da morte mesmo apÃ³s regenerar run.

### CA14 â€” Return item

Item de retorno permite sair sem morte.

### CA15 â€” Broken item

Durabilidade 0 impede uso.

### CA16 â€” Boss unlock

Boss derrotado libera checkpoint/avanÃ§o imediatamente.

---

## 6. Non-goals

Fora desta spec:

- UI final polida;
- animaÃ§Ã£o final de morte;
- cutscene da Fonte de Anya;
- sistema completo de storage na safe room;
- merchant da cave;
- campfire/rest completo;
- perda parcial configurÃ¡vel por dificuldade;
- multiplayer/co-op corpse recovery;
- snapshot completo de layout antigo para corpse.

---

## 7. MVP recomendado

1. CaveEntryMenu OnGUI simples com checkpoints e warnings.
2. CaveRunHud OnGUI separado do DebugHud.
3. Fonte de Anya como respawn point na Farm.
4. Failure flow: morte â†’ corpse save â†’ XP current level zerado â†’ Farm/Fonte de Anya.
5. CorpseRecoveryRoom no mesmo CaveLevel apÃ³s nova run seed.
6. Return item simples.
7. Durability 0 bloqueia uso.
8. Boss defeated libera checkpoint imediatamente.

---

## 8. DependÃªncias

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

