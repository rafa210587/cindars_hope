# SMOKE_TEST_CAVE_PROCEDURAL_VISUAL — FIX_CAVE_PROCEDURAL_VISUAL_RUNTIME_v1.0

## Contexto

Este documento captura a validação de aceitação do FIX_CAVE_PROCEDURAL_VISUAL_RUNTIME_v1.0, que implementa materialização visual completa da cave procedural com fallback GameObjects, database population e HUD com contadores reais.

**Branch esperada:** `feature/fase9a-town-commerce-mvp-package` (ou `feature/pr-154-170-cave-procedural-real-loop` se criada)  
**Data esperada:** 2026-05-20

---

## Escopo de Validação

### R1-R3: Fallback Visual Creation (Floor e Walls)

**Critério de aceição:** Floor tiles e wall tiles são criados como GameObjects com fallback sprites quando prefabs são null.

- [ ] `MaterializeFloor()` cria GameObjects quando `_floorTilePrefab` é null.
- [ ] Floor tiles possuem SpriteRenderer com builtin sprite (terra/brown).
- [ ] Floor tiles NÃƒO possuem collider.
- [ ] Wall tiles possuem SpriteRenderer com builtin sprite (cinza).
- [ ] Wall tiles possuem BoxCollider2D com `isTrigger=false` e `size=Vector2.one`.
- [ ] Contagem `CreatedFloorTiles` é incrementada por cada floor tile criado.
- [ ] Contagem `CreatedWallTiles` é incrementada por cada wall tile criado.

**Comandos de teste no Play Mode:**
```
1. Entrar na CaveScene.
2. Inspecionar Hierarchy: CaveGeneratedRuntime > GeneratedFloor deve conter floor GameObjects.
3. Inspecionar Hierarchy: CaveGeneratedRuntime > GeneratedWalls deve conter wall GameObjects.
4. Console: Procurar por log "CaveRuntimeMaterializer: Materialized level...".
5. DebugHud: Verificar que "Materialized: Floors: N" e "Materialized: Walls: N" aparecem com contagens > 0.
```

---

### R4-R5: Fallback Exit Portals

**Critério de aceição:** Portais de entrada e saída são criados com CaveExitPortal component e navegação funcional.

- [ ] `MaterializeEntranceAndExit()` cria GameObjects quando `_exitPortalPrefab` é null.
- [ ] BackExit portal (entrada) é criado na posição entrance com cor cyan (0, 1, 1).
- [ ] ForwardExit portal (saída) é criado na posição exit com cor magenta (1, 0, 1).
- [ ] Ambos possuem BoxCollider2D com `isTrigger=true`.
- [ ] BackExit é inicializado com `InitializeBackExit(CaveRunManager)`.
- [ ] ForwardExit é inicializado com `InitializeForwardExit(CaveRunManager)`.
- [ ] `_lastMaterializationResult.BackExitPosition` é setado corretamente.
- [ ] `_lastMaterializationResult.ForwardExitPosition` é setado corretamente.

**Comandos de teste no Play Mode:**
```
1. Entrar na CaveScene.
2. Inspecionar Hierarchy: CaveGeneratedRuntime > GeneratedExits > GeneratedBackExit e GeneratedForwardExit.
3. Mover player até BackExit, pressionar E: deve voltar um nível (ou FarmScene se nível 1).
4. Mover player até ForwardExit, pressionar E: deve avançar um nível.
5. DebugHud: Verificar posições de Entrance e Exit exibidas.
```

---

### R6: Fallback Resource Nodes

**Critério de aceição:** Resource nodes são criados como GameObjects com fallback sprite e collider trigger.

- [ ] `MaterializeResourceNodes()` cria GameObjects quando `_resourceNodePrefab` é null.
- [ ] Resource nodes possuem SpriteRenderer com builtin sprite (brownish).
- [ ] Resource nodes possuem CircleCollider2D com `radius=0.4f` e `isTrigger=true`.
- [ ] `SelectAndConfigureResourceNode()` configura o node com dados e sprite.
- [ ] Contagem `CreatedResourceNodes` é incrementada por cada node criado.

**Comandos de teste no Play Mode:**
```
1. Entrar na CaveScene.
2. Inspecionar Hierarchy: CaveGeneratedRuntime > GeneratedResourceNodes.
3. Verificar que nodes têm SpriteRenderer e CircleCollider2D.
4. DebugHud: Verificar que "Materialized: Resources: N" aparece com contagem > 0.
5. Interagir com resource node: deve tentar minerar ou exibir feedback de ferramenta ausente.
```

---

### R7-R8: Database Population

**Critério de aceição:** ResourceNodeDatabase e EnemyDatabase são populadas com assets default quando vazios.

- [ ] `CreateMvpCaveScene` cria/popula ResourceNodeDatabase com Stone, Copper, CaveRootTree.
- [ ] `CreateMvpCaveScene` cria/popula EnemyDatabase com Slime fallback.
- [ ] Editor script evita duplicatas ao popular databases.
- [ ] Se database for null, não causa erro; usa fallback.

**Comandos de teste no Editor:**
```
1. Menu CindarsHope > Scenes > Create MVP CaveScene.
2. Verificar que Assets/_Game/Data/Cave/ResourceNodeDatabase_MVP.asset existe.
3. Verificar que Assets/_Game/Data/Cave/EnemyDatabase_MVP.asset existe.
4. Inspeccionar databases: devem conter entries.
```

---

### R9-R10: Enemy Spawning com Determinismo

**Critério de aceição:** Enemies são spawnados no runtime com componentes corretos e determinismo de seeds.

- [ ] `CaveEnemySpawner.SpawnEnemiesForLevel()` recebe database e fallback enemy data.
- [ ] Se database vazio, usa `_fallbackEnemyData`.
- [ ] Se ambos null, skip spawning (sem erro).
- [ ] Seed string é `{WorldSeed}_{RunSeed}_{Level}_enemies`.
- [ ] Enemies possuem SpriteRenderer, CircleCollider2D, Rigidbody2D, EnemyHealth, EnemyChaseController, EnemyContactDamage com trigger child.
- [ ] Mesma seed world+run produz mesma distribuição de enemies (determinismo).

**Comandos de teste no Play Mode:**
```
1. Entrar na CaveScene.
2. Verificar Shift+R regenera cave sem duplicar enemies (limpa antes).
3. Duas cenas com mesma world seed + run seed devem ter enemies nas mesmas posições.
4. DebugHud pode exibir seed para validação.
5. Inspecionar console: logs de "CaveEnemySpawner: Spawned N enemies".
```

---

### R11: HUD com Contadores Reais

**Critério de aceição:** DebugHud DrawCaveSummary exibe contadores reais de objetos materializados.

- [ ] `DrawCaveSummary()` acessa `_caveLevelRuntimeController.Materializer.LastMaterializationResult`.
- [ ] HUD exibe `Materialized: Floors: {CreatedFloorTiles}`.
- [ ] HUD exibe `Materialized: Walls: {CreatedWallTiles}`.
- [ ] HUD exibe `Materialized: Resources: {CreatedResourceNodes}`.
- [ ] HUD exibe `Materialized: Enemies: {CreatedEnemies}`.
- [ ] Contadores refletem objetos realmente criados, não contagens de dados.
- [ ] Contadores atualizam após Shift+R regeneration.

**Comandos de teste no Play Mode:**
```
1. Entrar na CaveScene.
2. Verificar DebugHud painel direito: Materialized counts devem aparecer.
3. Pressionar Shift+R.
4. Contadores devem permanecer os mesmos (novo layout, mesmas contagens).
5. Comparar contadores com geração anterior: devem ser idênticos para mesma seed.
```

---

## Testes Críticos

### Teste A: Compilação e Import

```
1. Abrir projeto no Unity Editor.
2. Aguardar compilação completa.
3. Verificar que Console não tem erros vermelho.
4. Menu Assets > CindarsHope > Validate > Validate MVP Data.
   Resultado esperado: sem erros críticos.
```

### Teste B: Play Mode Inicial

```
1. Abrir CaveScene manualmente ou via portal.
2. Verificar que cena carrega sem exceções.
3. Inspecionar Hierarchy:
   - CaveGeneratedRuntime raiz deve existir.
   - GeneratedFloor, GeneratedWalls, GeneratedExits, GeneratedResourceNodes, GeneratedEnemies sub-parents devem existir.
4. Aguardar ~2s para finalizar spawning.
5. DebugHud deve exibir contadores não-zero.
```

### Teste C: Navegação e Regeneração

```
1. Mover player até BackExit, pressionar E: deve transicionar conforme modo (nível anterior ou FarmScene).
2. Mover player até ForwardExit, pressionar E: deve avançar nível.
3. Pressionar Shift+R: deve regenerar cave mantendo level, world seed e checkpoints.
4. Verificar DebugHud: RunSeed deve mudar, contadores devem permanecer.
```

### Teste D: Determinismo

```
1. Iniciar nova cave run. Anotar WorldSeed, RunSeed, CaveLevel.
2. Anotar posição de enemies no layout.
3. Fazer Shift+R (novo RunSeed).
4. Comparar layout: deve mudar.
5. Fazer Ctrl+Shift+R (reset world seed) se houver shortcut. Senão:
   - Sair da cave, voltar.
   - Verificar que nova entrada tem mesmo WorldSeed, novo RunSeed.
6. Layout deve ser novo, determinístico por seeds.
```

---

## Checklist de Conclusão

- [ ] Compilação sem erros.
- [ ] Play Mode CaveScene abre sem exceção.
- [ ] Contadores HUD aparecem e refletem objetos reais.
- [ ] Navegação BackExit/ForwardExit funciona.
- [ ] Shift+R regenera sem duplicar/vazar.
- [ ] Enemies spawnados corretamente com componentes.
- [ ] Determinismo de seeds validado.
- [ ] Databases (Resource/Enemy) populadas com defaults.
- [ ] Logs no Console confirmam materialização.
- [ ] Sem erros de referência nula ou dados ausentes.

---

## Pendências Pós-Fix

Os seguintes marcos permanecem pendentes:

- Marco 8: Loot tables e XP drops.
- Marco 9: Level scaling de resources.
- Marco 10: KO regeneration.
- Marco 11: Checkpoint selection UI.
- Marco 12: Boss gates.
- FASE9G: Faction locks e enemy ecology.

---

## Assinatura

**Criado:** 2026-05-20  
**Responsável:** Claude (Haiku 4.5)  
**Status:** Pronto para validação no Unity.

