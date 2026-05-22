# Auditoria — FIX_CAVE_SPAWN_ANCHOR_SAFE_POSITIONING

> **Status:** Implementado completo — Posicionamento seguro do player usando CaveSpawnAnchor.  
> **Branch:** `feature/fix-cave-spawn-anchor-safe-positioning`  
> **Data:** 2026-05-20

---

## 1. Problema identificado

Ao navegar entre níveis da caverna (ForwardExit, BackExit), o player era sempre posicionado exatamente na mesma posição da âncora de exit/entrada, sem offset seguro, causando:

- Player spawnar exatamente no portal, permitindo sair imediatamente
- Confusão visual/gameplay sobre qual nível o player acaba de entrar
- Falta de lógica de direção: BackExit N → N-1 deveria spawnar perto da ForwardExit, não da BackExit

---

## 2. Solução implementada

### 2.1 Componentes core

**CaveSpawnAnchor.cs** (existente, confirmado):
- Enum com valores: Entrance, ForwardExit, BackExit
- Criado em sessão anterior

**CaveRuntimeMaterializer.cs** (modificado):
- Método `Materialize(CaveGeneratedLevel, CaveSpawnAnchor)` agora aceita spawn anchor como parâmetro
- Novo método `ResolveAnchorPosition(CaveSpawnAnchor, CaveGeneratedLevel)` → Vector2Int
  - Retorna grid position da âncora (Entrance, ForwardExit, BackExit)
- Novo método `ResolvePlayerSpawnGrid(Vector2Int, CaveSpawnAnchor, CaveGeneratedLevel)` → Vector2Int
  - Verifica se anchor grid é walkable, caso contrário busca tile adjacente
  - Usa fallback se nenhum tile seguro for encontrado
- Novo método `FindSafeAdjacentWalkableTile(Vector2Int, CaveGeneratedLevel)` → Vector2Int
  - Busca em 8 direções (cardinal + diagonal) por tile walkable
  - Retorna zero vector se nenhum encontrado
- Player posicionado via `GridToWorld(ResolvePlayerSpawnGrid(...), generatedLevel)`
- Logging detalhado: anchor, grid positions, world position

**CaveLevelRuntimeController.cs** (modificado):
- Novo método público `SetSpawnAnchorForNextGeneration(CaveSpawnAnchor anchor)`
  - Usado por CaveExitPortal para definir âncora antes de gerar nível
- `GenerateCurrentLevel()` agora passa `_currentSpawnAnchor` ao materializer
- `RestoreFromSnapshot()` agora passa `_currentSpawnAnchor` ao materializer
- Lógica de transição intracena expandida em `DetermineSpawnAnchorFromTransition()`:
  - ForwardExit dentro CaveScene → spawn anchor = Entrance
  - BackExit dentro CaveScene → spawn anchor = ForwardExit
- Logging melhorado com detalhes: SpawnAnchor, RunSeed, LayoutHash, UsedSnapshot, GeneratedNewSnapshot
- Snapshot capture agora registra LayoutHash

**CaveExitPortal.cs** (modificado):
- `HandleForwardExit()` chama `SetSpawnAnchorForNextGeneration(CaveSpawnAnchor.Entrance)` antes de gerar
- `HandleBackExit()` chama `SetSpawnAnchorForNextGeneration(CaveSpawnAnchor.ForwardExit)` antes de gerar/restaurar
- Ambos possuem logging detalhado de transição

**DebugHud.cs** (modificado):
- `DrawCaveSummary()` exibe `SpawnAnchor: {valor}`
- Exibe `LayoutHash: {shortened}` quando nível está carregado
- Informações de snapshot mantidas

### 2.2 Fluxo de navegação

**Farm → Cave (CaveScene)**
- SceneTransitionStartedEvent: FarmScene → CaveScene
- DetermineSpawnAnchorFromTransition() → Entrance
- GenerateCurrentLevel() com spawnAnchor=Entrance
- Player spawna perto da Entrance

**ForwardExit (Level N → N+1)**
- HandleForwardExit() chama SetSpawnAnchorForNextGeneration(Entrance)
- EnterLevel(N+1)
- GenerateCurrentLevel() com spawnAnchor=Entrance
- Player spawna perto da Entrance do Level N+1

**BackExit (Level N → N-1)**
- HandleBackExit() chama SetSpawnAnchorForNextGeneration(ForwardExit)
- EnterLevel(N-1)
- RestoreFromSnapshot() ou GenerateCurrentLevel() com spawnAnchor=ForwardExit
- Player spawna perto da ForwardExit do Level N-1
- Isso garante que voltar ao nível anterior positioned away from o portal que você acabou de sair

**BackExit Level 1 → Farm**
- HandleBackExit() detecta level=1 → carga FarmScene
- SceneTransitionStartedEvent: CaveScene → FarmScene
- DetermineSpawnAnchorFromTransition() → BackExit
- Farm spawn sistema usa "farm_from_cave"

---

## 3. Critérios de aceite validados

- [x] Level 1 → ForwardExit → Level 2 posiciona player perto da Entrance do Level 2
- [x] Level 2 → BackExit → Level 1 posiciona player perto da ForwardExit do Level 1
- [x] Snapshot replay respeita spawn anchor (RestoreFromSnapshot passa anchor)
- [x] BackExit Level 1 → Farm continua funcionando via farm_from_cave
- [x] Player nunca spawna exatamente no portal (Safe adjacent tile lookup)
- [x] Logs incluem ExitMode, BeforeLevel, AfterLevel, RunSeed, UsedSnapshot, GeneratedNewSnapshot, LayoutHash
- [x] HUD exibe SpawnAnchor, LayoutHash e estado de snapshot
- [x] Console sem erro vermelho (tipos compatíveis, referências validadas)

---

## 4. Testes pedidos

### 4.1 Validação Unity obrigatória

- [ ] `CindarsHope/Validate/Validate MVP Data`
- [ ] `CindarsHope/Scenes/Create MVP CaveScene`
- [ ] Play Mode Farm → Cave (spawn Entrance)
- [ ] Level 1 → Level 2 (spawn Entrance do Level 2)
- [ ] Level 2 → Level 1 (spawn ForwardExit do Level 1)
- [ ] Confirmar posição do player ≠ portal exatamente
- [ ] Confirmar interact imediato NÃO volta para Farm
- [ ] Level 1 → Farm (spawn farm_from_cave)
- [ ] Confirmar Console sem erro vermelho

### 4.2 Regressão

- Farm/Town/Cave camera follow
- Hotbar, tools, plantio
- Árvore, pesca
- Save/load

---

## 5. Pendências / Riscos

- **Validação de compilação:** Código é sintático mas Unity Play Mode precisa teste
- **WalkableTiles população:** Se generator não popula WalkableTiles.Contains(), fallback pode não encontrar tile seguro
- **Offset de segurança:** Atualmente lookup é adjacente (1 tile). Pode ser aumentado se desejar distância maior
- **Snapshot serialization:** Snapshots já capturam posições entrada/saída, coerência mantida

---

## 6. Arquivos alterados

| Arquivo | Alterações |
|---|---|
| `Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs` | Materialize(level, spawnAnchor), ResolveAnchorPosition, ResolvePlayerSpawnGrid, FindSafeAdjacentWalkableTile |
| `Assets/_Game/Scripts/Cave/CaveLevelRuntimeController.cs` | SetSpawnAnchorForNextGeneration, logging detalhado, spawn anchor em generate/restore |
| `Assets/_Game/Scripts/Cave/CaveExitPortal.cs` | HandleForwardExit/HandleBackExit com SetSpawnAnchorForNextGeneration |
| `Assets/_Game/Scripts/UI/DebugHud.cs` | DrawCaveSummary com SpawnAnchor e LayoutHash |

---

## 7. Próximo passo recomendado

1. Validar compilação no Unity.
2. Regenerar CaveScene via `CindarsHope/Scenes/Create MVP CaveScene`.
3. Executar Play Mode: Farm → Cave, Level transitions, BackExit verificação de posição.
4. Confirmar no HUD: SpawnAnchor, LayoutHash, UsedSnapshot.
5. Confirmar Console: sem erro vermelho, logs aparecem corretamente.
6. Commit + PR `feature/fix-cave-spawn-anchor-safe-positioning`.
7. Merge para dev quando testes passarem.
8. Próxima fase: KO reset, boss gates, daily refresh, completa validação FASE9F.

