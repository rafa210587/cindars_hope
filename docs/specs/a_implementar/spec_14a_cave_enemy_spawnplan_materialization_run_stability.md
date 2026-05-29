# SPEC 14A - Cave Enemy SpawnPlan, Materialization and Run Stability

> Spec ID: spec_14a_cave_enemy_spawnplan_materialization_run_stability
> Status: A implementar
> Ordem de execucao: 14A
> Tipo: Cave Runtime / Enemy Spawn / Run Stability / Bestiary Integration
> Fonte de refinamento: `docs/refinements/a_implementar/pre_refinamentos/refinamento_spec14a_cave_enemy_spawnplan_materialization.md`
> Spec ampla relacionada: `docs/specs/a_implementar/spec_cave_runtime_generation_checkpoints_boss_gates.md`
> Depende de: SPEC 13A-13F implementadas em codigo; SPEC 13G closeout recomendado antes ou junto.
> Fora de escopo: cave snapshot completo, respawn comum 2 dias, redistribuicao pos-morte, boss fights, boss rewards, checkpoint UI, Bestiary UI, Flying runtime, arte final.

---

# /speckit.specify

## O QUE

Implementar o primeiro elo da SPEC 14 para conectar os monstros data-driven da SPEC 13 à cave runtime.

Fluxo alvo:

```text
EnemySpawnResolver
-> CaveEnemySpawnPlan
-> CaveRuntimeMaterializer.MaterializeEnemies
-> Enemy runtime instances
-> EnemySpawnedEvent / EnemySeenEvent
-> BestiaryManager
```

## POR QUE

A SPEC 13 criou dados, AI, Bestiary e SpawnResolver. Porém a cave real ainda não materializa inimigos usando esse resolver.

Sem esta SPEC 14A:

- os novos monstros não aparecem na cave;
- o Bestiary não recebe FirstSeen vindo da cave real;
- a composição de inimigos ainda não varia por nova run;
- a mesma run ainda não garante plano estável de inimigos por level;
- a SPEC 18 fica mais difícil de testar em gameplay real.

## ESCOPO

Inclui:

- `CaveEnemySpawnPlan`;
- `CaveEnemySpawnPlanEntry`;
- `CaveEnemySpawnPlanner`;
- integração com `EnemySpawnResolver`;
- materialização de inimigos no `CaveRuntimeMaterializer`;
- geração determinística por `CaveWorldSeed + CaveRunSeed + CaveLevel`;
- eventos para Bestiary;
- validator Editor;
- documentação de validação.

Não inclui:

- snapshot completo de level;
- estado derrotado/respawn de inimigos;
- respawn após 2 dias;
- redistribuição pós-morte;
- boss fights;
- boss rewards;
- checkpoint UI;
- UI de Bestiary;
- Flying runtime;
- arte final.

---

# /speckit.clarify

## Perguntas resolvidas

### Esta conexão já está na SPEC 14 ampla?

Sim, mas somente como parte de um escopo grande. A SPEC 14 ampla menciona enemy spawn plan estável, snapshot com `EnemySpawnPlan[]`, respawn e redistribuição. Ela não é pequena o suficiente para execução segura de um agente.

### Por que criar SPEC 14A?

Porque o elo necessário agora é menor:

```text
Resolver -> SpawnPlan -> Materializer -> Events -> Bestiary
```

### A SPEC 14A fecha a SPEC 14 inteira?

Não. Ela apenas conecta inimigos à cave.

### O SpawnPlan precisa ser salvo agora?

Não obrigatoriamente. Nesta spec, ele pode ser reconstruído deterministicamente por seed.

### A mesma run precisa manter os mesmos inimigos?

Sim.

### Nova run precisa mudar os inimigos?

Sim, desde que `CaveRunSeed` mude.

### Níveis já visitados podem rerollar?

Não deveriam. Nesta 14A, se snapshot completo ainda não existir, o plano deve ser deterministicamente reconstruível. A SPEC 14B deve fechar snapshot/replay full.

---

# /speckit.plan

## Arquitetura alvo

```text
CaveRunManager
├── CaveWorldSeed
├── CaveRunSeed
└── CurrentCaveLevel

CaveGeneratedLevel
├── CaveLevel
├── BiomeId
├── WalkableTiles
├── Entrance
├── Exit
└── future EnemySpawnPoints/RoomGraph hooks

CaveEnemySpawnPlanner
├── Build EnemySpawnRequest
├── Call EnemySpawnResolver
├── Select valid grid/world positions
└── Return CaveEnemySpawnPlan

CaveRuntimeMaterializer
├── MaterializeFloor
├── MaterializeWalls
├── MaterializeEntranceAndExit
├── MaterializeResourceNodes
└── MaterializeEnemies

Enemy runtime instance
├── EnemyBrain
├── EnemyHealth
├── EnemyDataSO/EnemyId
├── EnemyInstanceId
└── Events -> BestiaryManager
```

## Contratos novos

### CaveEnemySpawnPlan

Campos mínimos:

```text
int CaveLevel
string BiomeId
string CaveWorldSeed
string CaveRunSeed
string LevelSeed
List<CaveEnemySpawnPlanEntry> Entries
List<string> Warnings
```

Campo opcional:

```text
string LayoutHash
```

### CaveEnemySpawnPlanEntry

Campos mínimos:

```text
string EnemyInstanceId
string EnemyId
string SpawnProfileId
string PackId
Vector2Int GridPosition
Vector3 WorldPosition
string RoomId
int SpawnIndex
bool IsElite
string SizeClass
string FactionId
```

### CaveEnemySpawnPlanner

Responsabilidades:

- construir `EnemySpawnRequest`;
- chamar `EnemySpawnResolver`;
- escolher posições válidas;
- gerar IDs determinísticos;
- retornar warnings claros;
- não instanciar GameObjects.

## Integrações

### CaveRuntimeMaterializer

Adicionar campo serializado ou wiring equivalente:

```text
CaveEnemySpawnPlanner _enemySpawnPlanner
EnemyDataDatabaseSO / EnemyRegistry / EnemyPrefabResolver conforme existir
Enemy prefab/fallback runtime seguro
```

Adicionar etapa:

```text
MaterializeEnemies(generatedLevel, spawnPlan)
```

Atualizar `CaveRuntimeMaterializationResult.CreatedEnemies`.

### Enemy runtime

Se prefab final existir:

- instanciar prefab;
- configurar EnemyBrain/EnemyHealth com EnemyDataSO;
- aplicar EnemyInstanceId.

Se prefab final não existir:

- criar fallback GameObject com componentes mínimos existentes;
- logar warning;
- registrar pendência na validação.

### Bestiary

Ao materializar inimigo, publicar evento já suportado pela SPEC 13E:

```text
EnemySpawnedEvent
ou EnemySeenEvent
```

Payload apenas com IDs/tipos simples.

---

# /speckit.tasks

## SPEC14A-C01 — Revalidar base existente

- [ ] Ler `CaveRunManager`.
- [ ] Ler `CaveRuntimeMaterializer`.
- [ ] Ler `CaveGeneratedLevel`.
- [ ] Ler `EnemySpawnResolver`.
- [ ] Ler eventos de Enemy/Bestiary.
- [ ] Confirmar se existem prefabs/databases de Enemy.

## SPEC14A-C02 — Contratos do SpawnPlan

- [ ] Criar `CaveEnemySpawnPlan`.
- [ ] Criar `CaveEnemySpawnPlanEntry`.
- [ ] Garantir que ambos usam tipos simples e IDs.
- [ ] Não criar save DTO obrigatório ainda, salvo hook seguro.

## SPEC14A-C03 — CaveEnemySpawnPlanner

- [ ] Criar `CaveEnemySpawnPlanner`.
- [ ] Construir `EnemySpawnRequest` a partir de cave level, biome, seeds, locks e room size fallback.
- [ ] Chamar `EnemySpawnResolver`.
- [ ] Selecionar 12-20 inimigos quando houver espaço/dados suficientes.
- [ ] Respeitar MaxEnemies.
- [ ] Gerar `EnemyInstanceId` determinístico.
- [ ] Retornar warnings quando não houver dados/posição suficiente.

## SPEC14A-C04 — Seleção de posições

- [ ] Preferir EnemySpawnPoints/RoomGraph se existirem.
- [ ] Se não existirem, derivar de WalkableTiles.
- [ ] Excluir Entrance.
- [ ] Excluir Exit.
- [ ] Excluir tiles próximos do player spawn.
- [ ] Excluir walls.
- [ ] Respeitar distância mínima entre inimigos.
- [ ] Evitar Large/Huge/Boss em espaço inadequado quando metadata existir.

## SPEC14A-C05 — Materialização de inimigos

- [ ] Adicionar `MaterializeEnemies` ao `CaveRuntimeMaterializer`.
- [ ] Criar parent `GeneratedEnemies`.
- [ ] Instanciar enemy prefab ou fallback seguro.
- [ ] Configurar EnemyId/EnemyDataSO.
- [ ] Configurar EnemyInstanceId.
- [ ] Posicionar em WorldPosition.
- [ ] Aplicar size/collider quando possível.
- [ ] Incrementar `CreatedEnemies`.

## SPEC14A-C06 — Eventos e Bestiary

- [ ] Publicar `EnemySpawnedEvent` ou `EnemySeenEvent` ao materializar.
- [ ] Garantir payload simples.
- [ ] Não criar Bestiary paralelo.
- [ ] Confirmar que BestiaryManager recebe FirstSeen em Play Mode ou validator lógico.

## SPEC14A-C07 — Run stability

- [ ] Mesmo `CaveWorldSeed + CaveRunSeed + CaveLevel` gera mesmo plano.
- [ ] Novo `CaveRunSeed` gera plano diferente quando houver candidatos suficientes.
- [ ] Save/load preserva seeds e reconstrói plano.
- [ ] Não usar `Guid.NewGuid()` nos SpawnPlanEntries.

## SPEC14A-C08 — Validator

Criar:

```text
Assets/_Game/Scripts/Editor/Validation/ValidateSpec14AEnemySpawnMaterialization.cs
```

Verificar:

- `CaveEnemySpawnPlan` existe;
- `CaveEnemySpawnPlanEntry` existe;
- `CaveEnemySpawnPlanner` existe;
- `CaveRuntimeMaterializer` possui etapa de enemies;
- planner usa `EnemySpawnResolver`;
- seed inclui world seed, run seed e cave level;
- EnemyInstanceId é determinístico;
- eventos usam payload simples;
- não há busca global proibida runtime;
- não há serialização de Unity refs em save DTO.

## SPEC14A-DOC — Documento de validação

Criar:

```text
docs/validation/SPEC14A_CAVE_ENEMY_SPAWNPLAN_MATERIALIZATION_VALIDATION_<YYYYMMDD>.md
```

Conteúdo:

- resumo;
- arquivos alterados;
- contratos criados;
- como seed é usada;
- como inimigos são materializados;
- como Bestiary recebe eventos;
- validações executadas;
- validações pendentes;
- riscos residuais;
- próximos recortes.

---

# /speckit.implement

## Prompt base para agente executor

```md
Estamos no projeto Cindar's Hope / repositório rafa210587/cindars_hope.

Branch alvo: dev.

Implemente somente:
SPEC 14A - Cave Enemy SpawnPlan, Materialization and Run Stability

Leia primeiro:
- AGENTS.md
- CLAUDE.md
- PROJECT_LOG.md
- docs/IMPLEMENTATION_STATUS.md
- docs/specs/a_implementar/spec_14a_cave_enemy_spawnplan_materialization_run_stability.md
- docs/refinements/a_implementar/pre_refinamentos/refinamento_spec14a_cave_enemy_spawnplan_materialization.md
- docs/specs/a_implementar/spec_enemy_ai_roster_bestiary_faction_locks_runtime.md
- docs/specs/a_implementar/spec_cave_runtime_generation_checkpoints_boss_gates.md
- docs/validation/SPEC13F_SPAWN_RESOLVER_ECOLOGY_VALIDATION_20260528.md

Objetivo:
Conectar EnemySpawnResolver à CaveRuntimeMaterializer via CaveEnemySpawnPlan, materializando inimigos na cave de forma determinística por CaveWorldSeed + CaveRunSeed + CaveLevel e publicando eventos para o Bestiary.

Proibido:
- Não implementar snapshot completo.
- Não implementar respawn 2 dias.
- Não implementar redistribuição pós-morte.
- Não implementar boss fights.
- Não implementar UI.
- Não criar sistema paralelo de enemy data, save, damage, status, loot ou event bus.
- Não usar GameObject.Find/FindObjectOfType/FindObjectsByType em runtime.
- Não serializar referências Unity em save.

Ao final, entregar:
- arquivos alterados;
- validações executadas;
- validações não executadas e motivo;
- riscos residuais;
- próximo recorte recomendado.
```

---

# Definition of Done

- Projeto compila.
- `CaveEnemySpawnPlan` existe.
- `CaveEnemySpawnPlanEntry` existe.
- `CaveEnemySpawnPlanner` existe.
- `CaveRuntimeMaterializer` materializa inimigos.
- `EnemySpawnResolver` é usado na cave.
- Inimigos aparecem na cave.
- Mesma seed/run/level gera mesmo plano.
- Nova run seed gera outro plano quando possível.
- Bestiary recebe FirstSeen via evento.
- `CreatedEnemies` é atualizado.
- Validator SPEC 14A existe.
- Documento de validação criado.

---

# Validações obrigatórias

Rodar, se disponível:

```powershell
.\tools\docs\validate_docs.ps1
.\tools\unity\RunUnityCompileValidation.ps1
.\tools\unity\ScanUnityLogs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
git diff --check
```

Checklist Play Mode mínimo:

1. Abrir CaveScene.
2. Entrar no nível 1 da cave.
3. Confirmar inimigos materializados.
4. Confirmar inimigos compatíveis com band 1-10.
5. Sair/entrar na mesma run e confirmar mesmo plano.
6. Gerar nova run seed e confirmar plano diferente.
7. Confirmar `EnemySpawnedEvent`/`EnemySeenEvent`.
8. Confirmar FirstSeen no Bestiary.
9. Confirmar console sem erros novos.

---

# Próximos recortes após 14A

Se 14A passar:

```text
14B - Cave snapshot/replay full com EnemySpawnPlan[]
14C - Enemy defeated/respawn 2 dias
14D - Death redistribution hooks
14E - Boss gates/boss AI/rewards closeout
18A - Projectile visual foundation
```

Recomendação: executar 14B/14C antes da SPEC 18 se quiser cave mais sólida; executar 18A depois da 14A se quiser priorizar sensação de combate.
