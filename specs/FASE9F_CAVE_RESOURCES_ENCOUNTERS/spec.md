# SpecKit â€” FASE9F Cave, Resources, Encounters e Loot Progression

> **Feature:** `FASE9F_CAVE_RESOURCES_ENCOUNTERS`  
> **Status:** especificaÃ§Ã£o funcional aprovada para planejamento.  
> **Fonte de design:** `docs_old/FASE9F_CAVE_RESOURCES_ENCOUNTERS_SPEC_v1.0.md`

---

## 1. User story

Como jogador, quero explorar nÃ­veis grandes e labirÃ­nticos da caverna, coletar recursos exclusivos, enfrentar criaturas e bosses, liberar checkpoints e obter materiais melhores para craftar equipamentos mais fortes e avanÃ§ar cada vez mais fundo.

---

## 2. Objetivos funcionais

### O1 â€” CaveLevel inicial

A primeira entrada na caverna comeÃ§a em `CaveLevel = 1`.

### O2 â€” Checkpoints

A cada 15 nÃ­veis, o jogador libera um checkpoint permanente. Ao entrar na caverna, pode escolher qualquer checkpoint liberado.

### O3 â€” Procedural por run

A cave usa `CaveWorldSeed` persistente e `CaveRunSeed` da run atual. Ao sofrer KO/derrota, a cave run Ã© regenerada com nova `CaveRunSeed`; checkpoints permanecem.

### O4 â€” NÃ­veis grandes e labirÃ­nticos

Cada nÃ­vel deve ter mÃºltiplas salas, corredores, becos sem saÃ­da, caminhos alternativos, Ã¡reas opcionais, clusters de recursos e entrada/saÃ­da distantes.

### O5 â€” Biomas e bosses

Cada faixa de 15 nÃ­veis tem um bioma e um boss poderoso no fim da faixa.

### O6 â€” ResourceNodes

Nodes exigem ferramenta, tier mÃ­nimo, stamina e hits. Nodes podem representar minÃ©rio, pedra, Ã¡rvore subterrÃ¢nea, cristal, fungo ou recurso especial.

### O7 â€” Fallback sem pickaxe/tier

MinÃ©rio sem pickaxe/tier suficiente gera apenas `1x item_material_stone`, consome stamina, nÃ£o entrega minÃ©rio principal e nÃ£o depleta o node principal.

### O8 â€” Recursos por faixa

Cada bioma/faixa possui recursos especÃ­ficos, incluindo madeiras melhores que precisam ser refinadas para craftar itens de nÃ­veis melhores.

### O9 â€” Spawn por CaveLevel

Spawn respeita:

```text
70%â€“80%: EnemyLevel = CaveLevel
10%â€“20%: EnemyLevel = CaveLevel + 1
10%: Special EnemyLevel = CaveLevel + 2
```

### O10 â€” Slime especial visual

Slime especial deve ter cor/sprite diferente, XP maior e drop melhor.

### O11 â€” Loot tables

Inimigos e nodes devem usar loot table por ID, nÃ£o apenas drop fixo hardcoded.

### O12 â€” Save/load

Save/load deve persistir seeds, nÃ­vel atual, deepest layer, checkpoints, depleted nodes e bosses persistentes quando aplicÃ¡vel.

---

## 3. Non-goals

Fora do primeiro pacote FASE9F:

- arte final da cave;
- todos os 100 nÃ­veis implementados manualmente;
- boss final do nÃ­vel 100;
- puzzles completos;
- armadilhas complexas;
- companion ativo na cave;
- mercadores da Guilda das Estradas;
- baÃºs no primeiro slice;
- minimap;
- iluminaÃ§Ã£o/fog final;
- ItemRarity;
- balanceamento final;
- animaÃ§Ãµes finais dos bosses.

---

## 4. Regras de negÃ³cio

### R1 â€” Checkpoints

Checkpoints oficiais:

```text
1, 15, 30, 45, 60, 75, 90
```

Level 1 Ã© sempre disponÃ­vel.

### R2 â€” Boss bloqueia bioma seguinte

Boss de fim de faixa bloqueia o avanÃ§o para o prÃ³ximo bioma.

### R3 â€” KO/derrota regenera run

Ao sofrer KO/derrota:

- gerar nova `CaveRunSeed`;
- layout dos nÃ­veis muda;
- conteÃºdo da run muda;
- checkpoints liberados permanecem;
- deepest layer permanece.

### R4 â€” ResourceNode exige ferramenta e stamina

Ferramenta correta + tier suficiente + stamina suficiente permitem coletar loot principal.

### R5 â€” Fallback sem ferramenta correta

Fallback existe para minÃ©rio/pedra e deve ser explÃ­cito no `ResourceNodeDataSO`.

### R6 â€” RenovaÃ§Ã£o diÃ¡ria

Apenas nodes com `RespawnsDaily = true` renovam no `DayStartedEvent`.

### R7 â€” Save sem Unity refs

Save nÃ£o pode carregar `GameObject`, `Transform`, `MonoBehaviour`, `ScriptableObject`, `Sprite`, `Collider` ou `Rigidbody`.

### R8 â€” Sem busca global runtime

Sistemas novos nÃ£o podem usar `GameObject.Find`, `FindObjectOfType` ou `FindObjectsByType` em runtime.

---

## 5. Entidades funcionais

- `CaveGenerationConfigSO`
- `CaveLevelConfigSO`
- `CaveBiomeDataSO`
- `ResourceNodeDataSO`
- `LootTableSO vNext`
- `LootEntry`
- `CaveRuntimeState`
- `CaveSaveData vNext`
- `CaveRunRegeneratedEvent`
- `CaveLevelEnteredEvent`
- `ResourceNodeDepletedEvent`
- `CaveCheckpointUnlockedEvent`
- `CaveBossDefeatedEvent`

---

## 6. CritÃ©rios de aceite

### CA1 â€” Entrada inicial

Novo jogo entra na cave em `CaveLevel = 1`.

### CA2 â€” Checkpoint 15

ApÃ³s liberar checkpoint 15, ele aparece como opÃ§Ã£o de entrada.

### CA3 â€” Checkpoint persiste

ApÃ³s KO/derrota, checkpoint liberado permanece disponÃ­vel.

### CA4 â€” Run muda layout

ApÃ³s KO/derrota, `CaveRunSeed` muda e o layout dos nÃ­veis muda.

### CA5 â€” Save/load

Save/load preserva `CaveWorldSeed`, `CaveRunSeed`, `CurrentLayer`, `DeepestLayerReached` e checkpoints.

### CA6 â€” Level grande

CaveLevel gerado tem mÃºltiplas salas, corredores, entrada, saÃ­da e Ã¡reas opcionais.

### CA7 â€” Ferramenta/tier

ResourceNode valida ferramenta e tier antes de entregar loot principal.

### CA8 â€” Stamina

ResourceNode consome stamina por hit/aÃ§Ã£o.

### CA9 â€” Fallback stone

Minerar sem pickaxe/tier suficiente gera `1x item_material_stone`, nÃ£o depleta node principal e nÃ£o entrega minÃ©rio.

### CA10 â€” Daily refresh

Novo dia restaura somente nodes `RespawnsDaily = true`.

### CA11 â€” Spawn rule

Spawn respeita distribuiÃ§Ã£o base/+1/especial +2.

### CA12 â€” Slime especial

Slime especial tem cor/sprite diferente.

### CA13 â€” Boss de bioma

NÃ­vel final da faixa possui boss difÃ­cil e bloqueia o prÃ³ximo bioma.

### CA14 â€” Loot table

Inimigos e nodes usam loot table por ID.

### CA15 â€” Save seguro

Save de cave contÃ©m apenas DTOs e tipos simples.

### CA16 â€” Sem busca global

Nenhum sistema novo usa busca global runtime proibida.

---

## 7. DependÃªncias

- `SaveManager`
- `SaveData`
- `GameBootstrap`
- `GameEventBus`
- `PlayerManager`
- `InventoryManager`
- `EquipmentManager`
- `ToolDataSO`
- `ItemDatabaseSO`
- `EnemyDataSO`
- `EnemyHealth`
- `PlayerProgressionManager`
- `DamageCalculator`
- `DebugHud`

---

## 8. Plano sugerido de PRs

| PR | Escopo |
|---|---|
| PR-170 | Cave procedural contracts: configs, runtime state e eventos |
| PR-171 | Cave procedural generator MVP: rooms, corridors, entrada, saÃ­da, spawn/resource points |
| PR-172 | Cave run regeneration on player defeat/KO |
| PR-173 | Cave checkpoints a cada 15 nÃ­veis e entrada por checkpoint |
| PR-174 | ResourceNode contracts com ferramenta, tier, stamina e fallback |
| PR-175 | ResourceNode runtime MVP: Stone, CopperOre, CaveRootTree |
| PR-176 | Cave save/load: seeds, current layer, deepest layer, checkpoints, depleted nodes |
| PR-177 | Enemy spawn by CaveLevel com Slime especial colorido |
| PR-178 | Loot tables para inimigos e nodes |
| PR-179 | XP integration com EnemyLevel Ã— DifficultyMultiplier |
| PR-180 | Daily cave refresh apenas para `RespawnsDaily = true` |
| PR-181 | Biome boss MVP no CaveLevel 15 |
| PR-182 | Cave validator |

---

## 9. Pronto para Plan quando

- `docs_old/FASE9F_CAVE_RESOURCES_ENCOUNTERS_SPEC_v1.0.md` estiver lido.
- Estado real em `dev` tiver sido validado.
- FASE9E dependencies relevantes estiverem consideradas: Item Taxonomy, Save/Migration, Player Progression, Damage/Status e UI/Equipment.

