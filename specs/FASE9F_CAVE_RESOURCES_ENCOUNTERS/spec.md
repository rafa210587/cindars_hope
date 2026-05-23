# SpecKit — FASE9F Cave, Resources, Encounters e Loot Progression

> **Feature:** `FASE9F_CAVE_RESOURCES_ENCOUNTERS`  
> **Status:** especificação funcional aprovada para planejamento.  
> **Fonte de design:** `docs_old/FASE9F_CAVE_RESOURCES_ENCOUNTERS_SPEC_v1.0.md`

---

## 1. User story

Como jogador, quero explorar níveis grandes e labirínticos da caverna, coletar recursos exclusivos, enfrentar criaturas e bosses, liberar checkpoints e obter materiais melhores para craftar equipamentos mais fortes e avançar cada vez mais fundo.

---

## 2. Objetivos funcionais

### O1 — CaveLevel inicial

A primeira entrada na caverna começa em `CaveLevel = 1`.

### O2 — Checkpoints

A cada 15 níveis, o jogador libera um checkpoint permanente. Ao entrar na caverna, pode escolher qualquer checkpoint liberado.

### O3 — Procedural por run

A cave usa `CaveWorldSeed` persistente e `CaveRunSeed` da run atual. Ao sofrer KO/derrota, a cave run é regenerada com nova `CaveRunSeed`; checkpoints permanecem.

### O4 — Níveis grandes e labirínticos

Cada nível deve ter múltiplas salas, corredores, becos sem saída, caminhos alternativos, áreas opcionais, clusters de recursos e entrada/saída distantes.

### O5 — Biomas e bosses

Cada faixa de 15 níveis tem um bioma e um boss poderoso no fim da faixa.

### O6 — ResourceNodes

Nodes exigem ferramenta, tier mínimo, stamina e hits. Nodes podem representar minério, pedra, árvore subterrânea, cristal, fungo ou recurso especial.

### O7 — Fallback sem pickaxe/tier

Minério sem pickaxe/tier suficiente gera apenas `1x item_material_stone`, consome stamina, não entrega minério principal e não depleta o node principal.

### O8 — Recursos por faixa

Cada bioma/faixa possui recursos específicos, incluindo madeiras melhores que precisam ser refinadas para craftar itens de níveis melhores.

### O9 — Spawn por CaveLevel

Spawn respeita:

```text
70%–80%: EnemyLevel = CaveLevel
10%–20%: EnemyLevel = CaveLevel + 1
10%: Special EnemyLevel = CaveLevel + 2
```

### O10 — Slime especial visual

Slime especial deve ter cor/sprite diferente, XP maior e drop melhor.

### O11 — Loot tables

Inimigos e nodes devem usar loot table por ID, não apenas drop fixo hardcoded.

### O12 — Save/load

Save/load deve persistir seeds, nível atual, deepest layer, checkpoints, depleted nodes e bosses persistentes quando aplicável.

---

## 3. Non-goals

Fora do primeiro pacote FASE9F:

- arte final da cave;
- todos os 100 níveis implementados manualmente;
- boss final do nível 100;
- puzzles completos;
- armadilhas complexas;
- companion ativo na cave;
- mercadores da Guilda das Estradas;
- baús no primeiro slice;
- minimap;
- iluminação/fog final;
- ItemRarity;
- balanceamento final;
- animações finais dos bosses.

---

## 4. Regras de negócio

### R1 — Checkpoints

Checkpoints oficiais:

```text
1, 15, 30, 45, 60, 75, 90
```

Level 1 é sempre disponível.

### R2 — Boss bloqueia bioma seguinte

Boss de fim de faixa bloqueia o avanço para o próximo bioma.

### R3 — KO/derrota regenera run

Ao sofrer KO/derrota:

- gerar nova `CaveRunSeed`;
- layout dos níveis muda;
- conteúdo da run muda;
- checkpoints liberados permanecem;
- deepest layer permanece.

### R4 — ResourceNode exige ferramenta e stamina

Ferramenta correta + tier suficiente + stamina suficiente permitem coletar loot principal.

### R5 — Fallback sem ferramenta correta

Fallback existe para minério/pedra e deve ser explícito no `ResourceNodeDataSO`.

### R6 — Renovação diária

Apenas nodes com `RespawnsDaily = true` renovam no `DayStartedEvent`.

### R7 — Save sem Unity refs

Save não pode carregar `GameObject`, `Transform`, `MonoBehaviour`, `ScriptableObject`, `Sprite`, `Collider` ou `Rigidbody`.

### R8 — Sem busca global runtime

Sistemas novos não podem usar `GameObject.Find`, `FindObjectOfType` ou `FindObjectsByType` em runtime.

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

## 6. Critérios de aceite

### CA1 — Entrada inicial

Novo jogo entra na cave em `CaveLevel = 1`.

### CA2 — Checkpoint 15

Após liberar checkpoint 15, ele aparece como opção de entrada.

### CA3 — Checkpoint persiste

Após KO/derrota, checkpoint liberado permanece disponível.

### CA4 — Run muda layout

Após KO/derrota, `CaveRunSeed` muda e o layout dos níveis muda.

### CA5 — Save/load

Save/load preserva `CaveWorldSeed`, `CaveRunSeed`, `CurrentLayer`, `DeepestLayerReached` e checkpoints.

### CA6 — Level grande

CaveLevel gerado tem múltiplas salas, corredores, entrada, saída e áreas opcionais.

### CA7 — Ferramenta/tier

ResourceNode valida ferramenta e tier antes de entregar loot principal.

### CA8 — Stamina

ResourceNode consome stamina por hit/ação.

### CA9 — Fallback stone

Minerar sem pickaxe/tier suficiente gera `1x item_material_stone`, não depleta node principal e não entrega minério.

### CA10 — Daily refresh

Novo dia restaura somente nodes `RespawnsDaily = true`.

### CA11 — Spawn rule

Spawn respeita distribuição base/+1/especial +2.

### CA12 — Slime especial

Slime especial tem cor/sprite diferente.

### CA13 — Boss de bioma

Nível final da faixa possui boss difícil e bloqueia o próximo bioma.

### CA14 — Loot table

Inimigos e nodes usam loot table por ID.

### CA15 — Save seguro

Save de cave contém apenas DTOs e tipos simples.

### CA16 — Sem busca global

Nenhum sistema novo usa busca global runtime proibida.

---

## 7. Dependências

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
| PR-171 | Cave procedural generator MVP: rooms, corridors, entrada, saída, spawn/resource points |
| PR-172 | Cave run regeneration on player defeat/KO |
| PR-173 | Cave checkpoints a cada 15 níveis e entrada por checkpoint |
| PR-174 | ResourceNode contracts com ferramenta, tier, stamina e fallback |
| PR-175 | ResourceNode runtime MVP: Stone, CopperOre, CaveRootTree |
| PR-176 | Cave save/load: seeds, current layer, deepest layer, checkpoints, depleted nodes |
| PR-177 | Enemy spawn by CaveLevel com Slime especial colorido |
| PR-178 | Loot tables para inimigos e nodes |
| PR-179 | XP integration com EnemyLevel × DifficultyMultiplier |
| PR-180 | Daily cave refresh apenas para `RespawnsDaily = true` |
| PR-181 | Biome boss MVP no CaveLevel 15 |
| PR-182 | Cave validator |

---

## 9. Pronto para Plan quando

- `docs_old/FASE9F_CAVE_RESOURCES_ENCOUNTERS_SPEC_v1.0.md` estiver lido.
- Estado real em `dev` tiver sido validado.
- FASE9E dependencies relevantes estiverem consideradas: Item Taxonomy, Save/Migration, Player Progression, Damage/Status e UI/Equipment.


