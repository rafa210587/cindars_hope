# REF — FASE9F-B cave procedural real loop audit

> Origem histórica: $Source
> Status: Refinamento implementado / handoff / audit preservado
> Relacionado a:
> - $_

---

# Marco 0 — FASE9F-B Cave Procedural Real Loop Audit

**Data:** 2026-05-20  
**Responsável:** Claude (Haiku 4.5)  
**Branch esperada:** `feature/pr-154-170-cave-procedural-real-loop` (a ser criada)  
**Escopo:** Auditar estado real da implementação procedural da cave e registrar gaps antes de iniciar marcos 1-15.

---

## 1. Estado atual da cave procedural

### 1.1 Infraestrutura implementada ✓

| Componente | Status | Localização | Observação |
|---|---|---|---|
| Geração de layout (BSP) | ✓ Implementado | `CaveProceduralGenerator.cs` | Gera salas, conecta, coloca entrada/saída, placed spawn points. |
| Rooms conexão | ✓ Implementado | `CaveProceduralGenerator.cs` | Conecta salas via corredores. |
| Entrance/Exit placement | ✓ Implementado | `CaveProceduralGenerator.cs` | Coloca entrada e saída no layout gerado. |
| Generation points | ✓ Implementado | `CaveGenerationPoint.cs` | Suporta 4 tipos: ENEMY, RESOURCE, BOSS, ITEM. |
| State runtime | ✓ Implementado | `CaveRuntimeState.cs` | Rastreia nivel, seeds, checkpoints, nodes depletados. |
| Run manager | ✓ Implementado | `CaveRunManager.cs` | Gerencia world/run seeds, checkpoints, save/load. |
| Checkpoint service | ✓ Implementado | `CaveCheckpointService.cs` | Controla checkpoints desbloqueados por nivel. |
| Level runtime controller | ✓ Implementado | `CaveLevelRuntimeController.cs` | Chama geração, event publish, debug regeneration (Shift+R). |
| Resource node MVP | ✓ Implementado | `ResourceNode.cs` | Suporta interação, tool check, depletion, visuals. |
| Combat MVP | ✓ Implementado | Combat scripts (vide PR-099) | Slime, melee punch, knockback, hit flash, drops. |
| Scene generation | ✓ Implementado | `CreateMvpCaveScene.cs` | Cria cena com bootstrap, player, enemies, resources (STATIC). |
| Save/Load contracts | ✓ Implementado | `CaveSaveData.cs` | Serializa nivel, seeds, checkpoints, depleted nodes. |

### 1.2 Gaps críticos — O que falta ✗

| Gap | Impacto | Necesário para | Marco |
|---|---|---|---|
| **Não há materialização de GameObjects a runtime** | Sem isso, o layout é gerado mas nunca se torna actual GameObjects. | Qualquer gameplay procedural real. | Marco 1 |
| **Tiles/flooring não instanciados** | Layout gerado é data pura. Sem tilemap runtime. | Visualização e navegação do jogador. | Marco 1/5 |
| **Enemies não são spawnados do layout procedural** | Enemies são criadas manualmente em CreateMvpCaveScene. | Sistema de enemies procedural. | Marco 4 |
| **ResourceNodes não são instanciadas do layout** | ResourceNodes são criadas manualmente em CreateMvpCaveScene. | Sistema de recursos procedural. | Marco 3 |
| **Entrance/exit não são GameObjects válidos** | Placement é calculado, mas não há portais/interactables. | Navegação e transição de niveis. | Marco 2 |
| **Boss gates não implementados** | Sem gates de checkpoint/progressão. | Blocagem de avanço procedural. | Marco 13 |
| **Loot tables não existem** | Drops são hardcoded no Combat. | Sistema de itens procedural por nivel. | Marco 8 |
| **Escalagem de enemies por nível não existe** | Stats são fixos em EnemyDataSO. | Progression desafiador (Marco 9). |Marco 9 |
| **KO não regenera run** | Sem lógica de re-seed ao morrer. | Regeneração de cave após derrota (Marco 10). | Marco 10 |
| **Daily refresh não implementado** | ResourceNodes não renovam daily. | Loop de jogo persistente (Marco 12). | Marco 12 |
| **Validação procedural inexistente** | Sem smoke tests ou validators específicos. | QA do sistema procedural. | Marco 15 |

---

## 2. Fluxo atual (MVP fixo vs. procedural esperado)

### 2.1 Fluxo MVP atual (STATIC)

```
CaveScene carregada
    → CreateMvpCaveScene roda editor script
    → Cria Player, Ground, Bounds, Portals (MANUAL)
    → CreateCaveRuntime() → CaveLevelRuntimeController
    → GenerateCurrentLevel() → Layout gerado
    → CreateResourceNodes() → LOOP MANUAL sobre dados
    → CreateEnemies() → Hardcoded Slime position
    → Jogador entra, fights hardcoded enemies, coleta hardcoded resources
    → Nenhum procedural GameObject é materializado
```

### 2.2 Fluxo procedural esperado (FASE9F-B)

```
CaveScene carregada (play mode ou transição)
    → CaveLevelRuntimeController.GenerateCurrentLevel()
    → CaveProceduralGenerator.Generate() → CaveGeneratedLevel data
    → CaveRuntimeMaterializer.Materialize(generatedLevel)
        → Instancia Tilemap/flooring para cada tile walkable
        → Instancia paredes para cada tile solido
        → Instancia entrance/exit GameObjects
        → Loop sobre ResourceSpawnPoints
            → Instancia ResourceNode prefab em cada ponto
            → Configura com node data, managers, instancia ID
        → Loop sobre EnemySpawnPoints
            → Seleciona inimigo procedural por nivel/bioma/faction
            → Instancia Enemy prefab
            → Configura scaling por CaveLevel
    → Materializer publica CaveRuntimeMaterializationCompleteEvent
    → Sistemas subscribem e finalizem seus setup (enemies posição, etc)
    → Jogador pode agora navegar, fights procedural enemies, resources procedural
    → ao sofrer KO → CaveRunManager.GenerateNewRunSeed("ko_defeat")
    → Layout se regenera na próxima entrada
```

---

## 3. Componentes necessários (por marco)

### Marco 1 — CaveRuntimeMaterializer
- **Responsabilidade:** Converter CaveGeneratedLevel data pura em GameObjects instanciados.
- **Inputs:** `CaveGeneratedLevel`, prefabs (tilemap, wall, entrance, exit), managers (inventory, equipment, player).
- **Outputs:** Materialização no scene, evento de conclusão.
- **Pendências:** Prefabs tilemap/wall, entrance/exit prefab, parâmetros de escala.

### Marco 2 — Entrance/Exit
- **Responsabilidade:** Portais funcionais proceduralmente colocados.
- **Inputs:** Tile position do layout gerado.
- **Outputs:** Interactables que permitem navegação.
- **Pendências:** Portal prefab reutilizável, lógica de checkpoint selection.

### Marco 3 — ResourceNode instantiation
- **Responsabilidade:** Instanciar ResourceNodes do layout procedural.
- **Inputs:** ResourceSpawnPoint data do layout.
- **Outputs:** Nodes configuradas, rebindadas, tracking.
- **Pendências:** Selecção de resource type por nivel/bioma.

### Marco 4 — Enemy spawning procedural
- **Responsabilidade:** Instanciar enemies conforme CaveGeneratedLevel data.
- **Inputs:** EnemySpawnPoint data do layout.
- **Outputs:** Enemies configuradas, escaladas por nivel.
- **Pendências:** Seleção procedural de enemy type (FASE9G).

### Marco 5 — Debug visuals
- **Responsabilidade:** Exibição visual do layout gerado (grid, salas, pontos).
- **Inputs:** CaveGeneratedLevel.
- **Outputs:** Debug gizmos/lines no editor/game.
- **Pendências:** Integração com CaveGenerationDebugPrinter.

### Marco 6 — Regeneration hardening
- **Responsabilidade:** Limpeza robusta ao re-seed via Shift+R ou KO.
- **Inputs:** Command de limpeza.
- **Outputs:** Scene limpa, pronta para nova materialização.
- **Pendências:** Tracking de instantiados, delete order, edge cases.

### Marco 7 — Save/Load integration
- **Responsabilidade:** Persistir e restaurar procedural state.
- **Inputs:** CaveSaveData com layout state.
- **Outputs:** Cena restaurada coerentemente.
- **Pendências:** Node depletion persistence, enemy progression state.

### Marco 8 — Loot tables MVP
- **Responsabilidade:** Tabelas de itens por nivel/bioma.
- **Inputs:** CaveLevel, BiomeId, DamageType.
- **Outputs:** ItemDataSO selecionado para drop.
- **Pendências:** Data design, item pool definição.

### Marco 9 — Enemy scaling
- **Responsabilidade:** Escalar stats de enemy por CaveLevel.
- **Inputs:** EnemyDataSO base, CaveLevel.
- **Outputs:** Enemy com HP/damage/XP escalado.
- **Pendências:** Fórmula de escalagem, difficulty/rarity flags.

### Marco 10 — KO regeneration
- **Responsabilidade:** Nova run seed ao sofrer derrota.
- **Inputs:** PlayerDiedEvent ou similar.
- **Outputs:** CaveRunManager.GenerateNewRunSeed().
- **Pendências:** Hook em morte do jogador.

### Marco 11 — Checkpoint entry debug
- **Responsabilidade:** UI debug para selecting checkpoint ao entrar na cave.
- **Inputs:** UnlockedCheckpoints.
- **Outputs:** Selection UI, CaveRunManager.EnterLevel().
- **Pendências:** Debug UI componente, input handling.

### Marco 12 — Daily refresh
- **Responsabilidade:** Resetar nodes marked `RespawnsDaily` ao novo dia.
- **Inputs:** DayStartedEvent, DepletedNodeIds.
- **Outputs:** Nodes reativo se `RespawnsDaily = true`.
- **Pendências:** Flag `RespawnsDaily` em ResourceNodeDataSO, day subscription.

### Marco 13 — Boss gate MVP
- **Responsabilidade:** Gate de progressão em checkpoint 15 e maiores.
- **Inputs:** CaveLevel >= 15, BiomeId.
- **Outputs:** Boss inimigo + gate lógica.
- **Pendências:** Boss enemy data, gate collision/interaction.

### Marco 14 — Cave Debug HUD v2
- **Responsabilidade:** Exibição de estado procedural em HUD.
- **Inputs:** CaveRuntimeState, CaveGeneratedLevel.
- **Outputs:** HUD com nivel, seed, checkpoints, materials, enemies count.
- **Pendências:** Layout HUD, data binding.

### Marco 15 — Validators e smoke tests
- **Responsabilidade:** QA documentation para sistema procedural.
- **Inputs:** Specification FASE9F-B.
- **Outputs:** Validators + smoke test doc.
- **Pendências:** Checklist de testes, expected behaviors.

---

## 4. Dependências de dados externa

| Dado | Localização esperada | Status | Nota |
|---|---|---|---|
| Tilemap prefab (caverna) | `Assets/_Game/Prefabs/` | ✗ Não existe | Será criado ou reutilizado. |
| Wall prefab (caverna) | `Assets/_Game/Prefabs/` | ✗ Não existe | Será criado ou reutilizado. |
| Entrance/Exit portal | `Assets/_Game/Prefabs/` | ✗ Não existe | Será criado, pode reusar ScenePortal. |
| ResourceNode prefab | `Assets/_Game/Prefabs/Cave/` | ✗ Não existe | ResourceNode.cs existe, mas sem prefab. |
| Enemy prefabs (Slime, etc) | `Assets/_Game/Prefabs/Creatures/` | ✓ Existem | Reutilizar, adicionar mais conforme FASE9G. |
| BiomeDataSO | `Assets/_Game/Data/Cave/` | ✓ Existe | `CaveBiomeDataSO.cs` contrato existe. |
| GenerationConfig | `Assets/_Game/Data/Cave/` | ✓ Existe | `CaveGenerationConfigSO.asset` em uso. |
| ResourceNodeDataSO | `Assets/_Game/Data/Cave/` | ✓ Existe | Exist (Stone, Copper, CaveRootTree). |
| ItemDatabase | `Assets/_Game/Data/Registries/` | ✓ Existe | Item registry existe. |
| EnemyDatabase | `Assets/_Game/Data/Combat/` | ✓ Existe | Enemy registry existe. |

---

## 5. Checklist pre-Marco 1

- [x] Entender CaveProceduralGenerator e output CaveGeneratedLevel.
- [x] Entender CaveRuntimeState, CaveRunManager, checkpoints.
- [x] Identificar gaps (Materializer, prefabs, etc).
- [x] Confirmar branch: `dev` atual, criar feature branch para FASE9F-B.
- [x] Validar que cena MVP roda Play Mode sem erro.
- [ ] Criar feature branch.
- [ ] Iniciar Marco 1: CaveRuntimeMaterializer.

---

## 6. Referências

- **Geração:** `Assets/_Game/Scripts/Cave/Generation/CaveProceduralGenerator.cs`
- **Runtime state:** `Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeState.cs`
- **Run manager:** `Assets/_Game/Scripts/Cave/Runtime/CaveRunManager.cs`
- **Scene gen:** `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpCaveScene.cs`
- **Spec FASE9F:** `docs/FASE9F_CAVE_RESOURCES_ENCOUNTERS_SPEC_v1.0.md`
- **Spec FASE9G:** `docs/FASE9G_CAVE_BESTIARY_FACTION_LOCKS_PORTAL_ECOLOGY_SPEC_v1.0.md`

---

## 7. Próximos passos

1. **Aprovação desta auditoria** — Validar com humano que gaps e marcos estão corretos.
2. **Criação feature branch** — `feature/pr-154-170-cave-procedural-real-loop`.
3. **Marco 1 — CaveRuntimeMaterializer** — Converter dados em GameObjects.
4. **Marcos 2-15** — Implementar sequencialmente conforme este roadmap.
5. **PR final** — Single PR com múltiplos commits, push e review.

---

**Fim do Marco 0 Audit.**



