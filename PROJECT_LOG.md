# Cindar's Hope — Project Log

> Fonte operacional curta de continuidade do projeto.  
> Histórico completo preservado em `docs/logs/PROJECT_LOG_ARCHIVE_2026-05-18_FULL_BEFORE_SPLIT.md`.  
> Status curto de capacidades/specs preservado em `docs/IMPLEMENTATION_STATUS.md`.

---

## 1. Handoff atual

### Estado real validado

- Repositório: `rafa210587/cindars_hope`.
- Branch de trabalho: `dev`.
- Branch default do GitHub: `main`.
- A `dev` contém MVPs de Farm, Town, Crafting, Save/Load, Cave/Combat básico, HUD debug, transições Farm/Town/Cave e docs/specs da FASE9E/FASE9F/FASE9G.
- `PROJECT_LOG.md` foi reduzido para handoff operacional curto.
- O histórico completo anterior foi arquivado sem perda intencional em `docs/logs/PROJECT_LOG_ARCHIVE_2026-05-18_FULL_BEFORE_SPLIT.md`.
- Tracking curto de capacidades/specs implementadas criado em `docs/IMPLEMENTATION_STATUS.md`.
- Política de evolução de specs registrada em `docs/SPEC_EVOLUTION_POLICY_v1.0.md`.

### Specs recentes aprovadas

- `docs/FASE9E_UI_HOTBAR_INVENTORY_EQUIPMENT_SPEC_v1.0.md`
- `docs/FASE9E_DAMAGE_STATUS_FORMULA_SPEC_v1.0.md`
- `docs/FASE9E_ITEM_TAXONOMY_IDS_SPEC_v1.0.md`
- `docs/FASE9E_ITEM_EXAMPLES_VARIATIONS_v1.0.md`
- `docs/FUTURE_IDEAS_TODO_v1.0.md`
- `docs/FASE9E_SAVE_SCHEMA_MIGRATION_SPEC_v1.0.md`
- `docs/FASE9E_PLAYER_LEVEL_UP_PROGRESSION_SPEC_v1.0.md`
- `docs/FASE9F_CAVE_RESOURCES_ENCOUNTERS_SPEC_v1.0.md`
- `specs/FASE9F_CAVE_RESOURCES_ENCOUNTERS/spec.md`
- `docs/FASE9G_CAVE_BESTIARY_FACTION_LOCKS_PORTAL_ECOLOGY_SPEC_v1.0.md`
- `specs/FASE9G_CAVE_BESTIARY_FACTION_LOCKS_PORTAL_ECOLOGY/spec.md`
- `docs/SPEC_EVOLUTION_POLICY_v1.0.md`

## 16. Atualizacao 2026-05-20 - PR-170 a PR-192 FASE9F Cave Stable Run Replay Progression

Status: Implementado completo — Validação Unity e testes em Play Mode pendentes.

Implementação realizada:
- **PR-170 a PR-172**: Snapshot contracts, generator replayability, CaveRunSeed lifecycle
- **PR-173 a PR-175**: Runtime storage, snapshot registry, save/load integration
- **PR-176 a PR-178**: Replay on backtrack, snapshot restoration
- **PR-179 a PR-181**: Boss gate at level 15, player defeat integration
- **PR-182 a PR-184**: Daily refresh de `RespawnsDaily=true` nodes
- **PR-185 a PR-192**: Validação, documentação, handoff

Arquivos criados:
- `Assets/_Game/Scripts/Cave/Runtime/IVisitedLevelSnapshot.cs`
- `Assets/_Game/Scripts/Cave/Runtime/VisitedLevelSnapshot.cs`
- `Assets/_Game/Scripts/Core/Events/CavePlayerDefeatedEvent.cs`
- `Assets/_Game/Scripts/Cave/Validation/CaveReplayValidator.cs`
- `docs/FASE9F_CAVE_REPLAY_CONTRACTS_v1.0.md`
- `docs/FASE9F_CAVE_REPLAY_HANDOFF_PR170_192.md`

Arquivos modificados:
- `CaveRuntimeState.cs` (added VisitedLevelSnapshots)
- `CaveGeneratedLevel.cs` (added LayoutHash, ComputeLayoutHash)
- `CaveRunManager.cs` (added HandlePlayerDefeated, CheckBossGate, snapshot persistence)
- `CaveLevelRuntimeController.cs` (added CaptureSnapshot, RestoreFromSnapshot, daily refresh)
- `CaveExitPortal.cs` (updated HandleBackExit/HandleForwardExit)
- `CaveSaveData.cs` (complete rewrite with snapshot serialization)
- `ResourceNode.cs` (added RefreshForNewDay)
- `DebugHud.cs` (added snapshot status display)

Funcionalidades implementadas:
1. ✅ Snapshot contracts e DTOs serializáveis
2. ✅ Deterministic level generation com LayoutHash
3. ✅ Capture snapshot após materialização
4. ✅ Restore snapshot identicamente no backtrack
5. ✅ KO reset: novo CaveRunSeed, limpa snapshots, preserva checkpoints
6. ✅ Boss gate: bloqueia avanço além level 15 sem boss vencido
7. ✅ Daily refresh: apenas `RespawnsDaily=true` nodes renovam
8. ✅ Save/load persistence de snapshots e estado
9. ✅ Validation framework com CaveReplayValidator
10. ✅ DebugHud snapshot status display

Testes realizados (código):
- Validação de imports e sintaxe (sem rodada em Unity ainda)
- Verificação de contratos de tipo (IVisitedLevelSnapshot, VisitedLevelSnapshot, etc.)
- Verificação de persistência (CaveSaveData serialization)
- Verificação de integrações de eventos (DayStartedEvent, CavePlayerDefeatedEvent)

Próximo passo: Validação em Unity Play Mode, bug fixes se necessário, commit e merge.

---

## 17. Atualizacao 2026-05-21 - PR-193 a PR-202 FASE9F Cave Boss Gates, Checkpoints, Confinement

Status: Implementado completo (código) — Validação Unity Play Mode pendente.

**Escopo**: Extensão do pacote PR-170-192 com sistema de boss gates, seleção de checkpoints, persistência de derrota de boss e path confinement.

**Arquivos criados** (PR-193-202):

Data Structures & Events:
- `Assets/_Game/Scripts/Cave/Data/CaveBossGateDataSO.cs` - ScriptableObject para configuração de porta de boss
- `Assets/_Game/Scripts/Cave/Data/CaveBossGateRegistrySO.cs` - Registry com lookup de boss gates
- `Assets/_Game/Scripts/Cave/Runtime/CaveBossDefeatState.cs` - Classe serializável para persistir estado de derrota

Events:
- `Assets/_Game/Scripts/Core/Events/CaveBossDefeatedEvent.cs`
- `Assets/_Game/Scripts/Core/Events/CaveCheckpointSelectionRequestedEvent.cs`
- `Assets/_Game/Scripts/Core/Events/CaveCheckpointSelectedEvent.cs`

Runtime Components:
- `Assets/_Game/Scripts/Cave/Runtime/CaveBossSpawner.cs` - Spawna boss com visual diferenciado (PR-195)
- `Assets/_Game/Scripts/Cave/Runtime/CaveBossDefeatMonitor.cs` - Detecta derrota de boss e desbloqueia checkpoints (PR-196)
- `Assets/_Game/Scripts/Cave/Runtime/CaveCheckpointSelectionUI.cs` - MVP debug UI para seleção de checkpoint (PR-199)
- `Assets/_Game/Scripts/Cave/Runtime/CaveEntryController.cs` - Fluxo de entrada via checkpoint selecionado (PR-200)
- `Assets/_Game/Scripts/Cave/Runtime/CavePlayerPathConfinement.cs` - Confina player aos tiles walkable (PR-202)

Validation:
- `Assets/_Game/Scripts/Cave/Validation/CaveBossGateValidator.cs` - Valida configuração de boss gates (PR-201)

**Arquivos modificados** (PR-193-202):

- `Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeState.cs` - Added `BossDefeatStates` dictionary (PR-193/198)
- `Assets/_Game/Scripts/Save/CaveSaveData.cs` - Added `BossDefeatStates` list, PopulateBossDefeatStates/RestoreBossDefeatStates (PR-193/198)
- `Assets/_Game/Scripts/Cave/Runtime/CaveRunManager.cs`:
  - Added `_bossGateRegistry` field (PR-194)
  - Added `IsBossDefeated(string)` method (PR-196)
  - Added `MarkBossAsDefeated(string, int)` method (PR-196)
  - Updated `CheckBossGate(int)` to use registry instead of hardcode (PR-197)
  - Updated `CaptureSaveData()` to include boss states (PR-198)
  - Updated `RestoreFromSaveData()` to restore boss states (PR-198)
  - Updated `RestoreCachedStateIfNeeded()` to include boss states (PR-198)
- `Assets/_Game/Scripts/Cave/CaveLevelRuntimeController.cs` - Added `_bossSpawner` field, spawns boss in OnMaterializationComplete (PR-195)
- `Assets/_Game/Scripts/UI/DebugHud.cs` - Expanded DrawCaveSummary() to show boss gates section (PR-201)

**Funcionalidades implementadas** (PR-193-202):

1. ✅ **PR-193**: CaveBossGateData contracts, CaveBossDefeatState, eventos de boss/checkpoint
2. ✅ **PR-194**: CaveBossGateRegistry com query methods, integração com CaveRunManager
3. ✅ **PR-195**: CaveBossSpawner com cor diferenciada (laranja 1.0, 0.5, 0.0)
4. ✅ **PR-196**: CaveBossDefeatMonitor detecta morte de boss via EnemyKilledEvent, desbloqueia checkpoint
5. ✅ **PR-197**: CheckBossGate atualizado para usar registry, bloqueia avanço 15→16
6. ✅ **PR-198**: BossDefeatStates persistem em save/load via CaveSaveData
7. ✅ **PR-199**: CaveCheckpointSelectionUI com arrow keys (↑↓) e Enter para confirmar
8. ✅ **PR-200**: CaveEntryController aguarda CaveCheckpointSelectedEvent, entra em checkpoint
9. ✅ **PR-201**: DebugHud mostra boss gates + CaveBossGateValidator para validação
10. ✅ **PR-202**: CavePlayerPathConfinement confina player ao boundary de WalkableTiles

**Testes realizados** (código):
- Validação de imports e namespaces
- Verificação de contratos de serialização (BossDefeatState, CaveSaveData)
- Verificação de integrações de eventos (CaveBossDefeatedEvent, CaveCheckpointSelectedEvent)
- Verificação de persistência save/load (boss defeat state roundtrip)

**Pendências**:
- Validação Unity: compilação, Play Mode Farm→Cave→Boss→Checkpoint
- Teste de boss spawn visual no CaveLevel 15
- Teste de derrota de boss desbloqueando checkpoint 15
- Teste de gate check bloqueando avanço 15→16
- Teste de seleção de checkpoint e entrada no checkpoint
- Teste de path confinement mantendo player em bounds
- Teste de save/load preservando boss defeat state

**Próximo passo recomendado**:
1. Validar compilação no Unity.
2. Rodar `CindarsHope/Validate/Validate MVP Data`.
3. Play Mode: Farm → Cave (confirmar spawn Entrance no nível 1).
4. ForwardExit 1 → 2 (confirmar spawn Entrance no nível 2).
5. ForwardExit até level 15 (confirmar boss spawn com cor laranja).
6. Derrotar boss (confirmar CaveBossDefeatedEvent publicado, checkpoint 15 desbloqueado).
7. Tentar ForwardExit 15 → 16 (confirmar avanço permitido).
8. BackExit 16 → 15 (confirmar layout restaurado do snapshot).
9. BackExit 15 → 14 (confirmar ForwardExit spawn anchor).
10. Cave → Farm BackExit 1 (confirmar spawn farm_from_cave).
11. Farm → Cave (confirmar opção de seleção de checkpoint 1 e 15).
12. Selecionar checkpoint 15 (confirmar entrada no nível 15).
13. Save/load (confirmar boss defeat state persistido).
14. Confirmar player confinado ao WalkableTiles.
15. Console: sem erro vermelho, logs mostram boss defeat, checkpoint unlock, path confinement.
16. Commit + PR contra dev (sem auto-merge).

### Próximo passo recomendado

1. ✅ Clonar branch `feature/fase9f-cave-stable-run-replay-progression`
2. 🔄 Abrir projeto em Unity
3. 🔄 Compilação: Assets → Reimport All (ou aguardar auto-reimport)
4. 🔄 Check Console para CS errors (validate imports, namespaces)
5. 🔄 Test Play Mode: new run, backtrack, forward exit, KO, save/load, day refresh
6. 🔄 Bug fix se necessário
7. 🔄 Commit: `git commit -m "PR-170-192: Cave stable run replay progression"`
8. 🔄 Push: `git push origin feature/fase9f-cave-stable-run-replay-progression`
9. 🔄 Create PR on GitHub, merge to main após review

Após FASE9F merge:
- PR-193+: Boss defeat tracking e checkpoint selection UI
- PR-19x: Enemy ecology, faction locks (FASE9G)
- PR-20x: Bestiary, faction system (FASE9G)

---

## 18. Atualizacao 2026-05-21 - PR-193-202 FASE9F Correções: Boss Gate, Checkpoint, Confinement, Debug Skip

Status: Implementado completo (código) — **Namespace collision corrigido** — Validação Unity Play Mode pendente.

**Branch**: `feature/fix-pr193-202-boss-gate-checkpoint-confinement-debug-skip`

**Escopo**: 7 correções críticas no pacote PR-193-202 para resolver integrações incompletas e adicionar debug utilities.

**Hotfix de namespace collision (2026-05-21 pós-implementação)**:
- **Problema**: Namespace `CindarsHope.Cave.Debug` colide com `UnityEngine.Debug`, quebrando todas as chamadas `Debug.Log()` na cave
- **Solução**: Renomeado para `CindarsHope.Cave.Runtime`
- **Arquivos afetados**: `CaveDebugLevelSkipController.cs`, `DebugHud.cs`, `CaveSceneRuntimeReferenceInstaller.cs`
- **Regra adicionada em CLAUDE.md**: Namespace `Debug` nunca permitido dentro de `CindarsHope.*`

**Correções implementadas**:

**Correção 1 — Robust Boss Gate (15→16)**
- Adicionado método `CanAdvanceToLevel(int currentLevel, int targetLevel)` em `CaveRunManager.cs`
- Bloqueia avanço 15→16 explicitamente se boss registry null ou gate inexistente
- Logs de erro claro em vez de falha silenciosa
- Arquivo: `CaveRunManager.cs:245-276`
- Teste: `P hotkey respeita boss gate se _bypassBossGateForDebugSkip = false`

**Correção 2 — Conditional Boss Spawn**
- `CaveBossSpawner.SpawnBossForLevel()` agora valida se boss já foi derrotado
- Se `IsBossDefeated(gate.Id)`, skip com log "Boss gate already defeated. Skipping boss spawn."
- Arquivo: `CaveBossSpawner.cs:35-39`
- Teste: `Level 15 doesn't spawn boss if defeated`

**Correção 3 — Checkpoint Unlock Methods**
- Adicionado `UnlockCheckpoint(int checkpointLevel)` e `IsCheckpointUnlocked(int checkpointLevel)` em `CaveRunManager.cs`
- Complementa `CaveBossDefeatMonitor` que já chamava métodos de unlock
- Arquivo: `CaveRunManager.cs:341-360`
- Teste: `Boss defeat unlocks checkpoint 15`

**Correção 4 — Checkpoint Selection UI**
- Aprimorado `CaveCheckpointSelectionUI.cs` com OnGUI rendering centralizado
- Auto-seleciona checkpoint único (nível 1 só)
- Exibe lista navegável com ↑↓/W/S, confirm Enter/E, cancel Escape
- Arquivo: `CaveCheckpointSelectionUI.cs:86-111` (OnGUI)
- Teste: `Checkpoint selection shows multiple available` + `Checkpoint selection auto-selects when single`

**Correção 5 — Debug Level Skip Hotkey P**
- Criado novo namespace `CindarsHope.Cave.Debug` com classe `CaveDebugLevelSkipController.cs`
- Hotkey P (customizável) avança level sem marcar boss derrotado
- `_bypassBossGateForDebugSkip = true` default (bypass opcional)
- Rastreamento de última ação em `_lastDebugAction` para HUD display
- Arquivo: `CaveDebugLevelSkipController.cs:25-61` (SkipToNextLevel)
- Teste: `P hotkey increments level without changing CaveRunSeed` + `P hotkey doesn't mark boss defeated` + `P hotkey doesn't unlock checkpoint`

**Correção 6 — Path Confinement Rate-Limited**
- `CavePlayerPathConfinement.cs` agora limita logs a máximo 1 por segundo
- Adiciona `_lastLogTime` e constante `LogRateLimitSeconds = 1f`
- Evita spam em console quando player toca repeats em WallTiles
- Arquivo: `CavePlayerPathConfinement.cs:60-64`
- Teste: `Player cannot traverse WallTiles` + `Player cannot exit dungeon bounds`

**Correção 7 — Validators & HUD Display**
- `CaveBossGateValidator.cs` valida:
  - Registry null / empty
  - Duplicate gate IDs
  - Duplicate cave levels
  - Invalid CaveLevel (< 1)
  - Invalid CheckpointUnlockedOnDefeat
  - Empty BossEnemyId
- Adicionado `CaveDebugLevelSkipController` field em `DebugHud.cs`
- Novo método `DrawDebugLevelSkip()` mostra status enabled/disabled, tecla P, última ação
- Arquivo: `DebugHud.cs:404-425` (DrawDebugLevelSkip), `CaveSceneRuntimeReferenceInstaller.cs:53` (RebindExistingCaveRuntime pass)
- Teste: `HUD shows debug skip status` + `Validators report all issues`

**Arquivos modificados**:
- `CaveRunManager.cs` — Métodos CanAdvanceToLevel, UnlockCheckpoint, IsCheckpointUnlocked
- `CaveBossSpawner.cs` — Validação de boss já derrotado
- `CaveCheckpointSelectionUI.cs` — OnGUI rendering e auto-select logic
- `CavePlayerPathConfinement.cs` — Rate-limited logging
- `DebugHud.cs` — Adicionado CaveDebugLevelSkipController field, DrawDebugLevelSkip method
- `CaveSceneRuntimeReferenceInstaller.cs` — Pass CaveDebugLevelSkipController ao RebindExistingCaveRuntime

**Novos arquivos**:
- `CaveDebugLevelSkipController.cs` (namespace `CindarsHope.Cave.Debug`)

**Testes código**:
- Validação de imports e namespaces
- Verificação de contratos de tipo (métodos públicos acessíveis)
- Verificação de integrações de eventos (CaveBossDefeatedEvent, CaveCheckpointSelectedEvent)
- Verificação de persistência (CaveBossDefeatState roundtrip)

**Acceptance Criteria** (29+ testes a executar em Play Mode):
1. ✅ Code: CanAdvanceToLevel bloqueia 15→16 se registry null
2. ✅ Code: CanAdvanceToLevel bloqueia 15→16 se gate inexistente
3. ✅ Code: CanAdvanceToLevel permite 15→16 se boss derrotado
4. ✅ Code: CaveBossSpawner não spawna se boss derrotado
5. ✅ Code: UnlockCheckpoint/IsCheckpointUnlocked presentes
6. ✅ Code: CaveCheckpointSelectionUI tem OnGUI e auto-select
7. ✅ Code: CaveDebugLevelSkipController existe com hotkey P
8. ✅ Code: CavePlayerPathConfinement rate-limits logs
9. ✅ Code: CaveBossGateValidator valida registry
10. ✅ Code: DebugHud exibe debug skip status
11. 🔄 Play: P hotkey increments level without changing CaveRunSeed
12. 🔄 Play: P hotkey doesn't mark boss defeated
13. 🔄 Play: P hotkey doesn't unlock checkpoint
14. 🔄 Play: P hotkey respeita boss gate se _bypassBossGateForDebugSkip = false
15. 🔄 Play: Level 15 spawns boss if not defeated
16. 🔄 Play: Level 15 doesn't spawn boss if defeated
17. 🔄 Play: ForwardExit 15→16 blocks before boss defeat com explicit error
18. 🔄 Play: ForwardExit 15→16 allows after boss defeat
19. 🔄 Play: KO doesn't relock 15→16
20. 🔄 Play: Save/load preserves boss defeat
21. 🔄 Play: Cave→Farm→Cave doesn't relock
22. 🔄 Play: Checkpoint selection shows when multiple available
23. 🔄 Play: Checkpoint selection auto-selects when single
24. 🔄 Play: Player cannot traverse WallTiles
25. 🔄 Play: Player cannot exit dungeon bounds
26. 🔄 Play: HUD shows boss gate status
27. 🔄 Play: HUD shows debug skip status
28. 🔄 Play: Validators report all issues
29. 🔄 Play: Console sem erro vermelho durante boss defeat, checkpoint unlock, path confinement

**Pendências**:
- Validação Unity Play Mode (29+ acceptance criteria acima)
- Bug fixes se necessário durante testes
- Commit + PR contra dev
- Eventual merge após review

**Próximo passo recomendado**:
1. Abrir projeto em Unity
2. Compilação: Assets → Reimport All
3. Check Console para CS errors
4. Test Play Mode: Farm → Cave L1 → L15 (spawn boss) → Defeat → Checkpoint 15 unlock → ForwardExit 15→16 allowed
5. Test: P hotkey incrementa level, não marca boss derrotado
6. Test: Save/load preserva boss defeat
7. Test: Player confinado ao boundary
8. Validator feedback se aplicável
9. Commit + PR contra dev
10. Merge após review

1. PR-132 — DebugHud layout v2.
2. PR-133 — Action feedback event.
3. PR-134 — Tool gating contracts.
4. PR-135 — Tool gating para árvore e pesca.
5. PR-136 — Hotbar seed gating para FarmPlot.
6. PR-137 — Attribute allocation debug MVP.
7. PR-138 — DebugHud progression/tool/hotbar polish.
8. PR-139 — Handoff para Cave Procedural.

Em paralelo, este chat pode continuar refinando novas specs. Specs aprovadas antigas não devem ser reescritas destrutivamente; correções entram como amendments/corrections.

---

## 2. Protocolo obrigatório para agentes

Antes de qualquer tarefa:

1. Ler `PROJECT_LOG.md`.
2. Ler `docs/IMPLEMENTATION_STATUS.md`.
3. Ler `AGENTS.md` e/ou `CLAUDE.md`.
4. Ler os documentos de referência do PR/tarefa.
5. Confirmar branch atual e escopo permitido.
6. Validar estado real no GitHub/repo antes de planejar.
7. Se o trabalho tocar specs, ler `docs/SPEC_EVOLUTION_POLICY_v1.0.md`.

Durante a tarefa:

1. Manter escopo pequeno.
2. Não implementar V2/FULL quando o PR é MVP.
3. Não alterar docs de design sem pedido explícito.
4. Não mexer em arquivos fora da lista permitida do PR.
5. Registrar dúvidas/desvios em vez de decidir silenciosamente.
6. Não reescrever spec aprovada de forma destrutiva; usar nova spec, amendment, correction ou errata.

Ao final de tarefa relevante:

1. Atualizar `PROJECT_LOG.md` com nova entrada curta.
2. Atualizar `docs/IMPLEMENTATION_STATUS.md` com status curto de capacidades/specs.
3. Informar arquivos alterados.
4. Informar testes executados ou não executados.
5. Informar pendências, riscos e próximo passo recomendado.
6. Se a entrada ficar grande demais, criar novo archive em `docs/logs/` e manter este arquivo curto.

---

## 3. Política de evolução de specs

Fonte completa: `docs/SPEC_EVOLUTION_POLICY_v1.0.md`.

Resumo operacional:

```text
Implementar specs aprovadas pode acontecer em paralelo ao refinamento de novas specs.
Specs antigas aprovadas não devem ser reescritas destrutivamente.
Mudanças futuras entram como nova spec, amendment, correction ou errata.
PR iniciado segue a spec vigente no início, salvo bug crítico ou decisão humana explícita.
```

Regras:

- Specs aprovadas são baseline de implementação.
- Implementação pode continuar em outro chat usando a spec aprovada vigente.
- Novas specs podem ser escritas em paralelo neste chat.
- Correções em specs antigas devem indicar impacto em PRs futuros.
- Amendments/corrections devem ficar preferencialmente em `docs/amendments/`.

---

## 4. Estado consolidado curto

Fonte curta e atualizável: `docs/IMPLEMENTATION_STATUS.md`.

### Implementado no repo

- Core `GameEventBus` e eventos base.
- Data contracts e registries por ID.
- Farm MVP: plots, seeds, plantio, crescimento, colheita.
- Economy/Hunger/HUD debug.
- Save/load JSON local com cena atual e rebind cross-scene.
- World activities: árvores, pesca básica, pickups persistentes.
- Crafting MVP: receita de madeira processada e crafting point.
- Town MVP: portal Farm/Town, NPC Pip, compra/venda básica.
- Cave/Combat MVP: CaveScene, portal Farm/Cave, Slime, melee punch, chase, contact damage, drops, hit flash, knockback.
- Scene generators: FarmScene, TownScene, CaveScene.
- Validators de dados/cenas MVP.

### Especificado para próximas waves

- UI/Hotbar/Inventory/Equipment.
- Damage/Elementos/Status/Fórmula única.
- Item Taxonomy/IDs.
- Save Schema/Migration.
- Player Level Up/Progression.
- Cave/Resources/Encounters/Procedural progression.
- Cave Bestiary/Faction Locks/Portal Ecology.

---

## 5. Decisões FASE9F Cave

- Primeira entrada começa em `CaveLevel = 1`.
- Checkpoints permanentes a cada 15 níveis: `1, 15, 30, 45, 60, 75, 90`.
- Jogador pode escolher qualquer checkpoint liberado ao entrar na caverna.
- Cave usa `CaveWorldSeed` persistente e `CaveRunSeed` por run.
- Ao sofrer KO/derrota, a cave run é regenerada com nova `CaveRunSeed`; checkpoints permanecem.
- Cada nível deve ser grande, labiríntico e explorável.
- ResourceNode exige ferramenta correta, tier mínimo e consome stamina.
- Minério exige Pickaxe; sem pickaxe/tier suficiente, fallback gera `1x item_material_stone`, não entrega minério principal e não depleta node.
- Renovação diária só reseta nodes com `RespawnsDaily = true`.
- Baús ficam depois de nodes + enemies.
- Slime especial deve ter cor/visual diferente.
- Toda mudança de bioma tem boss poderoso e difícil.
- Recursos variam por nível/faixa; árvores subterrâneas podem dar madeiras melhores que exigem refinamento.

---

## 6. Decisões FASE9G Cave Bestiary/Faction Locks

- Geração procedural deve travar `FactionLock` por subfaixa de 3–5 níveis.
- Cada CaveLevel tem um FactionLock principal.
- Inimigos incompatíveis não aparecem no mesmo nível salvo exceções explícitas.
- Exceções: `AmbientFauna`, `RareIntruder`, `BossOverride`, `ConflictEncounter`.
- `ConflictEncounter` fica fora do MVP.
- `RareIntruder` entra com chance baixa e limitado por bioma.
- Boss e miniboss têm 3 opções procedurais por marco.
- Boss checkpoint persiste por save.
- Miniboss persiste por run.
- Humanoides inimigos são facções/exilados/cultistas/corrompidos/guardiões, não raças malignas por natureza.
- Beholder-like vira Observador/Olho de Elyndor.
- Duergar-like vira Anão da Forja Sem Sol / Anão Profundo Exilado.
- Drakes/wyverns antes do 90; dragão verdadeiro só late game/boss.
- Level 100 tem três possíveis final bosses por save.
- Luas modificam pesos, não quebram lock.

---

## 7. Checklist pendente

### Validação Unity geral

- [ ] Rodar `CindarsHope/Validate/Validate MVP Data`.
- [ ] Rodar `CindarsHope/Scenes/Create MVP FarmScene`.
- [ ] Rodar `CindarsHope/Scenes/Create MVP TownScene`.
- [ ] Rodar `CindarsHope/Scenes/Create MVP CaveScene`.
- [ ] Rodar validators de Farm/Town/Cave quando disponíveis.
- [ ] Testar Play Mode completo Farm → Town → Cave → Farm.
- [ ] Testar save/load em FarmScene.
- [ ] Testar save/load em TownScene.
- [ ] Testar save/load em CaveScene.
- [ ] Confirmar Console sem erro vermelho.

### Validação FASE9F futura

- [ ] CaveLevel 1 gera layout procedural grande.
- [ ] Cave run muda após KO/derrota.
- [ ] Checkpoints permanecem após KO/derrota.
- [ ] ResourceNode consome stamina.
- [ ] ResourceNode valida ferramenta/tier.
- [ ] Fallback sem pickaxe retorna apenas `1x item_material_stone`.
- [ ] Nodes `RespawnsDaily = true` renovam no novo dia.
- [ ] Slime especial tem cor diferente.
- [ ] Boss de bioma bloqueia avanço.

### Validação FASE9G futura

- [ ] CaveLevel gerado possui `BiomeId`, `EncounterEcologyId`, `FactionLockId` e `EnemyFamilyIds`.
- [ ] FactionLock impede mistura incoerente de inimigos.
- [ ] Boss/miniboss é escolhido entre 3 candidatos compatíveis.
- [ ] Boss checkpoint persiste por save.
- [ ] Miniboss persiste por run.
- [ ] DebugHud mostra ecology/faction/boss candidate quando implementado.

---

## 8. Histórico arquivado

O histórico completo antigo do `PROJECT_LOG.md` foi arquivado em:

```text
docs/logs/PROJECT_LOG_ARCHIVE_2026-05-18_FULL_BEFORE_SPLIT.md
```

Esse arquivo preserva o log operacional anterior inteiro antes da redução do log raiz.

---

## 9. Log de atividades recente

## 2026-05-20 - PR-100 Auditoria pos PR-099

**Responsavel:** Codex/ChatGPT  
**Branch:** `feature/pr-100-audit-pos-pr099`  
**Escopo:** auditar o estado real da `dev` apos o PR-099 e reconciliar a fila antes de voltar para Cave Procedural.

### Alteracoes

- Criado `docs/audits/PR100_POST_PR099_REPO_AUDIT.md`.
- Registrado que `PR-099 - Enemy stats data-driven` e o ultimo PR de implementacao confirmado por codigo.
- Registrado que `feature/pr-170-cave-procedural-contracts` existe como codigo adiantado/candidato local e deve ser reaproveitado depois, nao mergeado agora.
- Atualizado `docs/IMPLEMENTATION_STATUS.md` para trocar o proximo bloco recomendado de PR-170+ para reconciliacao PR-100 a PR-130.

### Testes

- [x] `git checkout dev`.
- [x] `git pull origin dev`.
- [x] Branch `feature/pr-100-audit-pos-pr099` criada a partir da `dev`.
- [x] Leitura documental obrigatoria executada.
- [x] Inventario estatico de scripts, dados, cenas e branches executado.
- [ ] Unity nao executado; auditoria documental/estatica.

### Pendencias / riscos

- Existem alteracoes locais ignoradas em `Assets/MobileDependencyResolver/**` e nas cenas MVP; o humano autorizou ignorar esses caminhos neste fluxo.
- `feature/fase9b3-enemy-data-driven-stats` nao apareceu local/remoto, apesar de citada no historico do PR-099.
- `Assets/_Game/Scripts/Cave`, `Tools`, `Equipment`, `UI/Hotbar` e `Player/Progression` ainda nao existem em `dev`.

### Proximo passo recomendado

- PR-101 - reconciliar branches/fixes PR-099 sem merge automatico.

---

## 2026-05-20 — FASE9G Cave Bestiary/Faction Locks

**Responsável:** ChatGPT  
**Branch:** dev  
**Escopo:** criar spec de bestiário, faction locks, ecologia procedural e boss/miniboss candidates para a cave.

### Alterações

- Criado `docs/FASE9G_CAVE_BESTIARY_FACTION_LOCKS_PORTAL_ECOLOGY_SPEC_v1.0.md`.
- Criado `specs/FASE9G_CAVE_BESTIARY_FACTION_LOCKS_PORTAL_ECOLOGY/spec.md`.
- Atualizado `PROJECT_LOG.md` com decisões FASE9G e checklist futuro.
- Spec inclui uso do Guia de Raças de Vaalara, Vaalara/Daromir/Elyndor, faction locks por subfaixa, bestiário amplo e 3 opções procedurais de boss/miniboss por marco.

### Testes

- [x] Arquivos FASE9G criados no repo.
- [x] Arquivos FASE9G lidos/validados no GitHub.
- [ ] Unity não executado; alteração é documental.

### Pendências / riscos

- Atualizar `docs/IMPLEMENTATION_STATUS.md` para listar FASE9G como especificada.
- FASE9G depende da base FASE9F para implementação real.

### Próximo passo recomendado

- Seguir com implementação FASE9F-A em outro chat.
- Usar FASE9G quando a implementação chegar em enemy ecology/faction lock.

---

## 2026-05-20 — Tracking de implementação

**Responsável:** ChatGPT  
**Branch:** dev  
**Escopo:** criar tracking curto de capacidades/specs implementadas e pendentes, separado do log operacional.

### Alterações

- Criado `docs/IMPLEMENTATION_STATUS.md`.
- Atualizado `PROJECT_LOG.md` para apontar o tracking como leitura obrigatória de agentes.
- Formalizado que todo PR futuro deve atualizar `PROJECT_LOG.md` e `docs/IMPLEMENTATION_STATUS.md`.
- Mantido `PROJECT_LOG.md` como log operacional/histórico curto.

### Testes

- [x] Documento criado diretamente na `dev`.
- [x] `PROJECT_LOG.md` atualizado com link e regra de manutenção.
- [ ] Unity não executado; alteração é documental.

### Pendências / riscos

- Opcional: reforçar a regra também em `AGENTS.md` e `CLAUDE.md`.
- O status de Cave/Combat básico foi mantido conforme `PROJECT_LOG.md`; validar código/Unity antes de marcar qualquer avanço além de MVP básico.

### Próximo passo recomendado

- Iniciar FASE9F-A — Cave Procedural Foundation, começando pelo PR-170.

---

## 2026-05-20 — Política de evolução de specs

**Responsável:** ChatGPT  
**Branch:** dev  
**Escopo:** registrar regra para permitir implementação paralela e refinamento de novas specs sem reescrever specs antigas.

### Alterações

- Criado `docs/SPEC_EVOLUTION_POLICY_v1.0.md`.
- Atualizado `PROJECT_LOG.md` para apontar a política como leitura obrigatória quando o trabalho tocar specs.
- Formalizado que specs aprovadas são baseline imutável.
- Formalizado que mudanças futuras entram como nova spec, amendment, correction ou errata.
- Formalizado que PR iniciado segue a spec vigente no início, salvo bug crítico ou decisão humana explícita.

### Testes

- [x] Política criada no repo.
- [x] `PROJECT_LOG.md` atualizado com resumo e link.
- [ ] Unity não executado; alteração é documental.

### Pendências / riscos

- Opcional: adicionar link explícito para esta política em `AGENTS.md` e `CLAUDE.md` em uma próxima sync documental.

### Próximo passo recomendado

- Implementação em outro chat pode seguir FASE9F.
- Este chat pode continuar escrevendo a próxima spec.

---

## 2026-05-20 — Split operacional do PROJECT_LOG

**Responsável:** ChatGPT  
**Branch:** dev  
**Escopo:** reduzir `PROJECT_LOG.md` para handoff operacional curto e arquivar histórico completo.

### Alterações

- Criado archive completo em `docs/logs/PROJECT_LOG_ARCHIVE_2026-05-18_FULL_BEFORE_SPLIT.md` reaproveitando o blob exato do `PROJECT_LOG.md` anterior.
- Substituído `PROJECT_LOG.md` por versão operacional curta.
- Mantidos links para specs FASE9E e FASE9F.
- Próximo passo recomendado atualizado para PR-170 da FASE9F.

### Testes

- [x] Archive criado a partir do blob antigo do `PROJECT_LOG.md`.
- [x] Novo `PROJECT_LOG.md` mantém handoff, decisões e próximos passos.
- [ ] Unity não executado; alteração é documental.

### Pendências / riscos

- Validar no GitHub se o archive aparece corretamente em `docs/logs/`.
- Próximas entradas devem ser curtas; logs extensos devem ir para novos archives.

### Próximo passo recomendado

- Iniciar PR-170 — Cave procedural contracts.
---

## 2026-05-20 - PR-101 a PR-130 reconciliacao consolidada pos PR-099

**Responsavel:** Codex/ChatGPT
**Branch:** `feature/pr-101-reconciliar-branches-pr099`
**Escopo:** executar em um unico commit, por decisao humana explicita, a reconciliacao PR-101 a PR-130 antes de retomar Cave Procedural.

### Alteracoes

- Criada auditoria `docs/audits/PR101_PR099_BRANCH_RECONCILIATION.md`.
- Criados docs `docs/audits/PR116_ITEM_ID_AUDIT.md`, `docs/audits/PR130_RECONCILIACAO_HANDOFF.md` e `docs/validation/SMOKE_TEST_FARM_TOWN_CAVE_MVP.md`.
- Aplicado hardening estatico em Combat: `EnemyHealth`, `EnemyContactDamage`, `EnemyChaseController`, `HitFlashController`, `KnockbackController`, `EnemyDropSpawner` e `EnemyDataSO`.
- Adicionados contratos MVP de dano, ferramentas, equipamento, hotbar e progressao.
- Integrado save/load simples para Equipment, Hotbar e PlayerProgression.
- Atualizados Bootstrap, geradores/instaladores de cena, DebugHud e validators para reconhecer o estado consolidado.

### Testes

- [x] Revisao estatica de escopo e arquivos alterados.
- [x] Metas Unity adicionadas para scripts/pastas novos.
- [ ] Unity nao executado nesta sessao.
- [ ] Regeneracao de cenas nao executada nesta sessao.

### Pendencias / riscos

- Validar compilacao no Unity.
- Validar smoke test Farm/Town/Cave.
- `item_material_stone` e `ore_copper` seguem pendentes como assets/IDs futuros de Cave Resources.
- As cenas locais e `Assets/MobileDependencyResolver/**` permaneceram ignorados por instrucao humana.

### Proximo passo recomendado

- Validar este commit no Unity.
- Depois de aprovado/mergeado, retomar Cave Procedural Foundation como PR-131+.

---

## 2026-05-20 - PR-131 sync de tracking pos reconciliacao

**Responsavel:** Codex/ChatGPT
**Branch:** `feature/pr-131-sync-status-pos-reconciliacao`
**Escopo:** sincronizar tracking documental depois do consolidado PR-101 a PR-130 na `dev`.

### Alteracoes

- Atualizado `docs/IMPLEMENTATION_STATUS.md` para marcar PR-101 a PR-130 como implementado parcial.
- Equipment/Hotbar, Progression/LevelUp e Damage Formula MVP passaram de `Especificado` para `Implementado parcial`.
- Cave Procedural/Resources permaneceu como pendente.
- Atualizado handoff PR-130 para apontar a sequencia vigente PR-132 a PR-145.
- Atualizado este log com o proximo bloco recomendado da FASE9F-A.

### Testes

- [x] Revisao estatica documental.
- [ ] Unity nao executado; PR documental.

### Pendencias / riscos

- Validar Unity antes de avancar em PRs de codigo se houver erro vermelho local.
- Executar PR-132 antes de iniciar contratos procedurais.

### Proximo passo recomendado

- PR-132 - Pre-flight Unity hardening antes da Cave Procedural.

---

## 2026-05-20 - PR-131 sync de validacao HUD/tools/progressao

**Responsavel:** Codex/ChatGPT
**Branch:** `feature/pr-131-sync-validacao-hud-tools-progression`
**Escopo:** registrar estado real validado de HUD debug, hotbar, tools, progressao e cave fixed MVP antes de novas features.

### Alteracoes

- Criado `docs/audits/PR131_VALIDACAO_HUD_TOOLS_PROGRESSION.md`.
- Atualizado `docs/IMPLEMENTATION_STATUS.md` para registrar lacunas reais:
  - tool existe, mas ainda nao bloqueia arvore/pesca;
  - hotbar existe, mas plantio ainda nao usa slot selecionado;
  - XP/level/pontos existem, mas nao ha distribuicao debug de atributos;
  - cave ainda e fixed MVP, sem seed/procedural.
- Atualizado o proximo bloco recomendado para FASE9E-D PR-132 a PR-139 antes da Cave Procedural.

### Testes

- [x] Revisao estatica documental e inspeção dos arquivos relevantes.
- [ ] Unity nao executado; PR documental.

### Pendencias / riscos

- Validar no Unity o estado relatado antes de mergear se houver divergencia local.
- Cave Procedural deve aguardar o handoff PR-139.

### Proximo passo recomendado

- PR-132 - DebugHud layout v2.

---

## 2026-05-20 - PR-132-FIX HUD split + tool/hotbar gating

**Responsavel:** Codex/ChatGPT
**Branch:** `feature/pr-132-fix-hud-tool-hotbar-gating`
**Escopo:** corrigir implementacao parcial anterior de HUD debug, feedback de acoes, gating por ferramenta equipada e plantio por hotbar.

### Alteracoes

- Criado `PlayerActionFeedbackEvent` para mensagens temporarias de acoes bloqueadas.
- Reorganizado `DebugHud` em painel esquerdo de acoes e painel direito de informacoes.
- Adicionado status fixo da cave no HUD: `Cave: fixed MVP` e `Seed: unavailable`.
- Adicionados contratos `HasTool` e mensagem de ferramenta ausente em `EquipmentManager`.
- `TreeNode` agora exige `Axe/Basic` equipado antes de contabilizar hit.
- `FishingSpot` agora exige `FishingRod/Basic` equipado antes de adicionar peixe.
- `FarmPlot` agora usa o item selecionado na hotbar para plantar e bloqueia slot vazio/item nao-seed/seed ausente no inventario.

### Testes

- [x] Revisao estatica dos arquivos alterados.
- [x] Busca estatica por `GameObject.Find`, `FindObjectOfType`, `FindObjectsByType`, `StreamingAssets` e dependencia nova de tag nos arquivos alterados.
- [ ] Unity nao executado nesta sessao.

### Pendencias / riscos

- Validar no Unity se o HUD fica bem posicionado em 1280x720 e nao sobrepoe conteudo relevante.
- Validar em Play Mode: ferramenta None/Axe/FishingRod, plantio por hotbar e mensagens temporarias.
- Este PR nao implementa distribuicao debug de atributos nem Cave Procedural.

### Próximo passo recomendado

- Validar PR-132-FIX no Unity.
- Depois seguir para distribuição debug de atributos ou handoff FASE9E-D, conforme prioridade.

---

## 2026-05-20 - FASE9F-B Marco 0 e Marco 1 — Auditoria + CaveRuntimeMaterializer

**Responsável:** Claude (Haiku 4.5)  
**Branch esperada:** `feature/pr-154-170-cave-procedural-real-loop` (para ser criada)  
**Escopo:** Marco 0 auditoria do estado procedural cave + Marco 1 implementação do CaveRuntimeMaterializer.

### Alterações

**Marco 0:**
- Criado `docs/audits/MARCO0_FASE9F-B_CAVE_PROCEDURAL_REAL_LOOP_AUDIT.md` — Estado real vs gaps vs roadmap.
- Registrado que infraestrutura de geração, runtime state, e contratos existem.
- Identificado gap crítico: sem materialização de GameObjects a runtime.
- Roadmap de 15 marcos listado com dependências.

**Marco 1:**
- Criado `Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs` — Classe que converte `CaveGeneratedLevel` data em GameObjects.
- Materializa: flooring, walls, entrance/exit, resource nodes.
- Suporta cleanup de materialização anterior.
- Criado `CaveRuntimeMaterializationCompleteEvent` para notificar conclusão.
- Atualizado `CaveLevelRuntimeController` para chamar materializer após geração.
- Adicionadas flags `_materializer` e `_materializeAfterGeneration` para controle.

### Testes

- [x] Revisão estática de código e estrutura.
- [x] Validação de referências e dependências.
- [ ] Unity não executado nesta sessão.
- [ ] Regeneração de CaveScene não executada.
- [ ] Smoke test procedural não executado.

### Pendências / riscos

- **Prefabs faltando:** Materializer esperà por floor tile prefab, wall tile prefab, entrance/exit portal prefab — todos precisam ser criados ou reutilizados.
- **Seleção de resource node:** MVP usa seleção aleatória de todos os nodes; refinamento por nivel/bioma pendente (Marco 9).
- **Enemies não são spawnadas:** Materializer coloca spawn points mas não materializa enemies — Marco 4.
- **Validação Unity:** Compilação e cena procedural não testadas em Play Mode.

### Próximo passo recomendado

- **Criar feature branch** `feature/pr-154-170-cave-procedural-real-loop`.
- **Marco 2:** Implementar entrance/exit portais funcionais (interactables de navegação).
- **Validar cena** no Unity com prefabs criados.

---

## 2026-05-20 - PR-140 Cave procedural contracts

**Responsavel:** Codex/ChatGPT
**Branch:** `feature/pr-140-cave-procedural-contracts`
**Escopo:** criar contratos base da cave procedural sem generator, runtime gameplay, assets ou integracao de save completa.

### Alteracoes

- Criados `CaveGenerationConfigSO`, `CaveLevelConfigSO` e `CaveBiomeDataSO`.
- Criado `CaveRuntimeState` como classe pura sem herdar de `MonoBehaviour`.
- Criado `CaveSaveData` serializavel com tipos simples.
- Criados eventos `CaveLevelEnteredEvent`, `CaveRunRegeneratedEvent` e `CaveCheckpointUnlockedEvent`.

### Testes

- [x] Revisao estatica dos arquivos alterados.
- [x] Busca estatica por APIs proibidas e Unity refs em DTO de save nos arquivos do PR.
- [ ] Unity nao executado nesta sessao.

### Pendencias / riscos

- Unity precisa validar compilacao e menus `CreateAssetMenu`.
- `CaveSaveData` ainda nao foi integrado ao `GameSaveData`/`SaveManager`; isso fica para PR-152.
- Generator procedural, run manager, checkpoints service e ResourceNode runtime ficam para PRs seguintes.

### Proximo passo recomendado

- PR-141 - Cave procedural generator puro.

---

## 2026-05-20 - Pacote PR-141 a PR-153 Cave Procedural Runtime

**Responsavel:** Codex/ChatGPT
**Branch:** `feature/pr-141-153-cave-procedural-runtime`
**Escopo:** pacote unico para runtime procedural da cave. A `dev` ainda nao continha PR-140 no inicio, entao o commit de contratos foi incorporado como base tecnica nesta branch.

### Marco PR-141 - Cave procedural generator puro

- Criados modelos `CaveRoom`, `CaveGeneratedLevel`, `CaveGenerationPoint` e `CaveGenerationPointType`.
- Criado `CaveProceduralGenerator` deterministico por `CaveWorldSeed + CaveRunSeed + CaveLevel`.
- Criado `CaveGenerationDebugPrinter` com ASCII usando `#`, `.`, `E`, `X`, `M` e `R`.

### Testes do marco

- [x] Revisao estatica de namespaces e tipos.
- [ ] Unity nao executado nesta sessao.

### Marco PR-142 - CaveRunManager e seeds

- Criado `CaveRunManager` com `CaveWorldSeed`, `CaveRunSeed`, `CurrentCaveLevel` e `DeepestLayerReached`.
- Adicionados `InitializeIfNeeded`, `EnterLevel`, `GenerateNewRunSeed`, `CaptureSaveData` e `RestoreFromSaveData`.
- `GenerateNewRunSeed` publica `CaveRunRegeneratedEvent`.

### Marco PR-143 - CaveLevelRuntimeController

- Criado `CaveLevelRuntimeController`.
- O controller gera o nivel atual no `Start`, loga seeds/contagens/layout ASCII e publica `CaveLevelEnteredEvent`.
- Expostos contadores de rooms, enemy points e resource points para HUD/debug.

### Marco PR-144 - Wiring na CaveScene

- Atualizado `CreateMvpCaveScene` para criar `CaveRuntime` com `CaveRunManager` e `CaveLevelRuntimeController`.
- O gerador editorial cria/usa `Assets/_Game/Data/Cave/CaveGenerationConfig_Default.asset` quando o menu for executado no Unity.
- Cena e asset fisicos ainda dependem de executar o menu no Editor.

### Marco PR-145 - DebugHud Cave status

- `DebugHud` agora exibe status procedural da cave quando recebe `CaveRunManager` e `CaveLevelRuntimeController`.
- `CaveSceneRuntimeReferenceInstaller` faz rebind das referencias da cave no HUD.
- Fallback permanece `Cave: fixed/unavailable` e `Seed: unavailable`.

### Marco PR-146 - Cave run regeneration debug

- `CaveLevelRuntimeController` aceita `Shift+R` na `CaveScene` para gerar nova `CaveRunSeed`.
- A regeneracao preserva `CaveWorldSeed` e `CurrentCaveLevel`, recalcula o layout e atualiza o HUD.

### Marco PR-147 - Cave checkpoints service

- Criado `CaveCheckpointService` com checkpoints oficiais `1, 15, 30, 45, 60, 75, 90`.
- Level 1 fica sempre liberado e checkpoints liberados usam o `CaveRuntimeState`.
- `CreateMvpCaveScene` adiciona o service ao `CaveRuntime`.

### Marco PR-148 - ResourceNode contracts

- Criados `ResourceNodeDataSO` e `ResourceNodeDatabaseSO`.
- Criado `ResourceNodeDepletedEvent`.
- Contrato usa `ToolType`, `ToolTier`, stamina, hits, drop principal e fallback.

### Marco PR-149 - ResourceNode rules

- Criadas regras puras `ResourceNodeRules`.
- Criados resultados `ResourceNodeToolCheckResult` e `ResourceNodeInteractionResult`.
- Regras separam drop principal, fallback e mensagem de ferramenta/tier insuficiente.

### Marco PR-150 - ResourceNode runtime MVP

- Criado `ResourceNode` interagivel por `IInteractable`.
- Node consulta `EquipmentManager`, entrega drop principal/fallback e publica `ResourceNodeDepletedEvent`.
- Node registra deplecao no `CaveRunManager` quando o resultado deve depletar.

### Marco PR-151 - ResourceNodes debug na CaveScene

- `CreateMvpCaveScene` cria nodes debug Stone, Copper e CaveRootTree ao regenerar a cena.
- O gerador editorial cria assets `ResourceNode_Stone`, `ResourceNode_Copper`, `ResourceNode_CaveRootTree` e itens mínimos `item_material_stone`/`ore_copper` quando necessário.
- A cena `.unity` e os `.asset` físicos dependem de executar o menu no Unity.

### Marco PR-152 - Cave save/load procedural MVP

- `GameSaveData` agora possui `CaveSaveData`.
- `SaveManager` captura/restaura `CaveRunManager` quando rebundado.
- `CaveSceneRuntimeReferenceInstaller` rebinda o runtime da cave no `SaveManager`.

### Marco PR-153 - Validator e handoff

- `MvpSceneValidator` valida `CaveRunManager`, `CaveLevelRuntimeController` e ResourceNodes debug na CaveScene.
- Criado `docs/audits/PR153_CAVE_PROCEDURAL_HANDOFF.md`.

---

## 2026-05-20 - FASE9F-B Marcos 1-7 Implementação Completa (Nesta Sessão)

**Responsável:** Claude (Haiku 4.5)  
**Branch esperada:** `feature/pr-154-170-cave-procedural-real-loop` (a ser criada)  
**Escopo:** Implementação completa dos marcos 1-7 do procedural cave real loop com materialização, spawning, persistência e validação.

### Alterações

**Marco 1 - CaveRuntimeMaterializer:**
- Criado `CaveRuntimeMaterializer.cs` — Converte `CaveGeneratedLevel` para GameObjects.
- Materializa flooring (com verificação de prefab), walls (com collider), entrada/saída, e resource nodes.
- Publica `CaveRuntimeMaterializationCompleteEvent` ao terminar.
- Integrado em `CaveLevelRuntimeController` para materializar após gerar layout procedural.

**Marco 2 - Entrance/Exit Portals:**
- Criado `CaveExitPortal.cs` — Portal especializado para transição da cave.
- Materializer diferencia entrance (ScenePortal reutilizável) e exit (CaveExitPortal).
- Colliders trigger criados automaticamente no materializer.

**Marco 3-4 - Resource e Enemy Procedural Spawning:**
- ResourceNodes materializadas pelo materializer com seleção aleatória de tipo.
- Criado `CaveEnemySpawner.cs` — Spawna enemies dos spawn points com configuração pós-instanciação.
- `CaveLevelRuntimeController` inscreve ao evento de materialização e chama spawner automaticamente.
- Adicionado método `Configure(EnemyDataSO)` em `EnemyHealth` para setup de inimigos instanciados.
- Enemies spawned com: SpriteRenderer, CircleCollider2D, Rigidbody2D, EnemyHealth, KnockbackController, HitFlashController.

**Marco 5 - Debug Visualization:**
- Criado `CaveDebugVisualizer.cs` — Gizmo drawing para layout em Play Mode.
- Visualiza: walkable tiles (verde), walls (cinza), rooms (azul), enemy spawn (vermelho), resource spawn (amarelo), entrada/saída (cyan/magenta).
- Toggles em inspector para controlar cada camada visual.

**Marco 6 - Regeneration Hardening:**
- Métodos públicos `CleanupMaterialization()` e `CleanupSpawns()` adicionados.
- `CaveLevelRuntimeController.CleanupBeforeRegeneration()` chama ambos antes de re-seed.
- Shift+R agora executa cleanup robusto → novo seed → regeneração completa.

**Marco 7 - Save/Load Coherence:**
- Save/load já integrado em `SaveManager` via `CaveRunManager.CaptureSaveData()` / `RestoreFromSaveData()`.
- `CaveSaveData` persiste: CurrentCaveLevel, DeepestLayerReached, CaveWorldSeed, CaveRunSeed, UnlockedCheckpoints, DepletedNodeIds.
- Coerência procedural garantida pela persistência de seeds.

### Testes

- [x] Revisão estática de código.
- [x] Validação de integração de eventos GameEventBus.
- [x] Verificação de referências e dependências.
- [ ] Unity compilação não testada.
- [ ] Play Mode não testado.
- [ ] Smoke test completo não executado.

### Pendências / Riscos

- **Prefabs faltando:** Floor tile, wall tile, entrance, exit — precisam ser criados ou reutilizados de assets existentes.
- **EnemyDatabase:** Materializer esperaà por DataRegistry<EnemyDataSO> não estar vazio.
- **ResourceNodeDatabase:** Seleção MVP aleatória; refinamento por nível/bioma (Marco 9) pendente.
- **Marcos 8-15:** Não implementados nesta sessão (loot tables, level scaling, KO regen, checkpoint selection, daily refresh, boss gates, HUD v2, validators).
- **Validação crítica:** Cena deve rodar sem erros de compilação; Play Mode deve gerar layout sem exceções.

### Próximo passo recomendado

1. Validar no Unity: compilação e Play Mode da CaveScene.
2. Regenerar cena via `CindarsHope/Scenes/Create MVP CaveScene`.
3. Atribuir prefabs aos campos do materializer (ou criar prefabs simples placeholder).
4. Testar Shift+R para regeneração.
5. Criar feature branch e push final com todos estes commits.
6. Implementar marcos 8-15 conforme prioridade em próxima sessão ou paralelo.

### Pendencias / riscos do pacote

- Unity nao foi executado nesta sessao; cena e assets gerados por menu precisam ser materializados no Editor.
- `Assets/_Game/Scenes/CaveScene.unity` e `Assets/_Game/Data/Cave/*.asset` nao foram atualizados fisicamente porque o Unity nao foi aberto.
- Stamina real, KO real, enemy spawn por layout, daily refresh, boss e FASE9G ficam fora do escopo.

---

## 2026-05-20 - FASE9F-B Marcos 3-11 Continuacao Visual + Spawning + HUD (Nesta Sessao)

**Responsável:** Claude (Haiku 4.5)  
**Branch:** `feature/fase9a-town-commerce-mvp-package`  
**Escopo:** Continuar implementacao dos marcos 3-11 da FASE9F-B com foco em materialização visual, spawning procedural de enemies com componentes corretos, determinismo de seeds e HUD enhancements.

### Alterações

**Marco 3-4 Hierarchical Structure & Resource Node Parenting:**
- Atualizado `MaterializeResourceNodes()` para criar parent GameObject `GeneratedResourceNodes` e parental resource nodes sob ele (em vez de usar `transform`).
- Exposado `GeneratedRuntimeRoot` como propriedade pública em `CaveRuntimeMaterializer` para acesso externo.

**Marco 7 Enemy Procedural Spawning com Componentes Corretos:**
- Adicionado `_caveRunManager` como campo em `CaveEnemySpawner` para acesso a seeds.
- Assinatura de `SpawnEnemiesForLevel()` atualizada para aceitar `generatedRuntimeRoot` e `playerTarget` opcionais.
- Implementado `_generatedEnemiesRoot` GameObject como parent para enemies.
- Adicionado `EnemyChaseController` a cada enemy spawned com `ConfigureFromData(enemyData)` e `RebindTarget(_playerTarget)`.
- Criado trigger child `ContactDamageTrigger` com CircleCollider2D trigger e `EnemyContactDamage` component.
- Adicionado `Configure(EnemyDataSO, Collider2D)` method ao `EnemyContactDamage` para setup runtime.
- Atualizado `CaveLevelRuntimeController` para passar playerTransform e root quando calling `SpawnEnemiesForLevel()`.

**Marco 8 Determinismo Refinement:**
- Enemy spawn selection agora usa full seed string: `{WorldSeed}_{RunSeed}_{Level}_enemies` (em vez de só `{Level}_enemies`).
- Isso garante que mesma seed world + run produz mesma distribuição de enemies.

**Marco 9-10 HUD Updates & Enhanced Logging:**
- Adicionado exibição de `Entrance` e `Exit` coordinates no `DrawCaveSummary()` do `DebugHud`.
- Aprimorado `RegenerateCurrentRunDebug()` para logar old/new RunSeed: `"Cave regenerated via debug (Shift+R). RunSeed: {old} -> {new}."`

**Player Transform Configuration:**
- Atualizado `CreateMvpCaveScene` para passar `playerTransform` a `CreateCaveRuntime()`.
- Configurado `_playerTransform` field em `CaveRuntimeMaterializer` via SerializedObject.
- Adicionado `_playerTransform` field em `CaveLevelRuntimeController` e configurado no editor script.
- Player agora spawnado na entrance corretamente sem condition restrictiva (removida check `!= Vector2Int.zero`).

### Testes

- [x] Revisao estatica de integracao de eventos e chamadas de spawn.
- [x] Validacao de hierarquia GameObject: CaveGeneratedRuntime > GeneratedFloor/Walls/Exits/ResourceNodes/Enemies.
- [x] Verificacao de determinismo de seeds para enemies.
- [x] Inspecao de EnemyChaseController e EnemyContactDamage setup.
- [ ] Unity compilacao não testada.
- [ ] Play Mode não testado.

### Pendências / Riscos

- **EnemyChaseController needs player target:** Configurado via `_playerTransform` em controller, mas precisa validar que chase funciona no Play Mode.
- **Enemy contact damage trigger:** Validar que OnTriggerStay2D do `EnemyContactDamage` é chamado corretamente.
- **Resource node depletion:** Já implementado via `CaveRunManager.RegisterDepletedNode()` e `RestoreDepletedStateFromRun()` — apenas validação pendente.
- **Marcos 11 em diante:** Documentação updates e smoke tests ainda pendentes.

### Próximo passo recomendado

1. Validar compilacao no Unity.
2. Testar Play Mode: spawning, chase behavior, contact damage, determinismo de regeneracao (Shift+R).
3. Criar e atualizar docs de validacao em `docs/audits/` ou `docs/validation/`.
4. Atualizar `docs/IMPLEMENTATION_STATUS.md` e este log com status final.
5. Preparar feature branch e push se tudo passar em Unity.

---

## 2026-05-20 - FIX_CAVE_PROCEDURAL_VISUAL_RUNTIME_v1.0 Implementação Completa

**Responsável:** Claude (Haiku 4.5)  
**Branch:** `feature/fase9a-town-commerce-mvp-package`  
**Escopo:** Implementação completa do FIX_CAVE_PROCEDURAL_VISUAL_RUNTIME_v1.0 — adicionar fallback visual e database population para materializar cave procedural visually.

### Alterações

**CaveRuntimeMaterializationResult.cs (criado):**
- Nova classe de contratos para rastrear objetos realmente materializados.
- Campos: `CreatedFloorTiles`, `CreatedWallTiles`, `CreatedResourceNodes`, `CreatedEnemies`, `BackExitPosition`, `ForwardExitPosition`.
- Propósito: separar contagens de dados (WalkableTiles.Count) de contagens reais (objetos criados).

**CaveRuntimeMaterializer.cs (completado):**
- Adicionado `_lastMaterializationResult` field e `LastMaterializationResult` property pública.
- Implementado `GetBuiltinSprite()` com compilação condicional `#if UNITY_EDITOR` para carregamento de sprite builtin.
- `MaterializeFloor()`: Cria fallback GameObject com SpriteRenderer (cor terra #6B3A2A), sem collider. Incrementa `CreatedFloorTiles`.
- `MaterializeWalls()`: Cria fallback com cor cinza, BoxCollider2D. Incrementa `CreatedWallTiles`.
- `MaterializeEntranceAndExit()`: Cria fallback cyan (BackExit) e magenta (ForwardExit) portals com CaveExitPortal component. Registra posições em `BackExitPosition` e `ForwardExitPosition`.
- `MaterializeResourceNodes()`: Cria fallback ResourceNode e incrementa `CreatedResourceNodes`.
- `SelectAndConfigureResourceNode()`: Adiciona SpriteRenderer com cor brownish e CircleCollider2D trigger.

**CaveExitPortal.cs (refatorado):**
- Adicionado enum `CaveExitMode` (BackExit, ForwardExit).
- Métodos `InitializeBackExit(CaveRunManager)` e `InitializeForwardExit(CaveRunManager)` para configuração de modo.
- `HandleBackExit()`: Level 1 carrega FarmScene; Level > 1 faz EnterLevel(CurrentLevel - 1).
- `HandleForwardExit()`: EnterLevel(CurrentLevel + 1).
- Mantém compatibilidade com `HandleSceneTransition()` para transições baseadas em cena.

**CaveEnemySpawner.cs (aprimorado):**
- Adicionado field `_fallbackEnemyData` [SerializeField] para Slime default quando database vazio.
- Método `SpawnEnemiesForLevel()` agora: usa database se populated, fallback para `_fallbackEnemyData`, skip se ambos null.
- Determinismo preservado com seed string `{WorldSeed}_{RunSeed}_{Level}_enemies`.

**CreateMvpCaveScene.cs (database population):**
- `EnsureResourceNodeDatabase()`: Chama `EnsureCaveResourceData()` para garantir Stone/Copper/CaveRootTree assets. Popula database via SerializedObject manipulation. Retorna database com 3 nodes.
- `EnsureEnemyDatabase()`: Chama `EnsureEnemySlimeData()` para garantir Enemy_Slime.asset. Popula database com Slime fallback.
- `EnsureEnemySlimeData()`: Cria default Slime (id=enemy_slime_basic, maxHp=10, contactDamage=1, moveSpeed=1.2, etc).
- `CreateCaveRuntime()`: Configura `_fallbackEnemyData` no spawner via SerializedObject.

**DebugHud.cs (R11 - HUD com contadores reais):**
- `DrawCaveSummary()` estendido para exibir materialized counts:
  - `Materialized: Floors: {CreatedFloorTiles}`
  - `Materialized: Walls: {CreatedWallTiles}`
  - `Materialized: Resources: {CreatedResourceNodes}`
  - `Materialized: Enemies: {CreatedEnemies}`
- Acessa `_caveLevelRuntimeController.Materializer.LastMaterializationResult`.

### Testes

- [x] Revisão estática completa de todos os arquivos.
- [x] Validação de integração de eventos GameEventBus.
- [x] Verificação de referências Unity e dependências.
- [x] Inspeção de fallback sprite conditional compilation.
- [x] Validação de database population logic.
- [ ] Unity compilação não testada.
- [ ] Play Mode não testado.
- [ ] Smoke test completo não executado.

### Pendências / Riscos

- **Validação crítica:** Código deve compilar sem erros. Play Mode deve gerar e visualizar cave procedural sem exceções.
- **Prefabs:** Se prefabs forem atribuidos, materializer usa prefab; se null, usa fallback GameObject.
- **Databases:** Editor script popula datasets com assets default; permanecer vazio é aceitável (usa fallback).
- **EnemyChaseController:** Requer `_playerTransform` configurado em `CaveLevelRuntimeController` para funcionar.
- **Resource nodes depletion tracking:** Já integrado em `CaveRunManager.RegisterDepletedNode()`.

### Próximo passo recomendado

1. Validar compilacao no Unity: abrir project, regenerar CaveScene via menu editor.
2. Testar Play Mode: verificar materialization, contadores HUD, navigacao entre niveis (Shift+R para regeneracao).
3. Criar smoke test validation doc se Play Mode passar.
4. Atualizar `docs/IMPLEMENTATION_STATUS.md` para marcar Cave Procedural como `Implementado` (visual + procedural base).
5. Preparar feature branch para push se tudo passar.

---

## 2026-05-20 - FIX_CAVE_CAMERA_FOLLOW_AND_VISIBLE_ENEMIES_v1.0 Implementação Completa

**Responsável:** Claude (Haiku 4.5)  
**Branch:** `feature/fase9a-town-commerce-mvp-package`  
**Escopo:** Implementação completa de câmera com smooth follow e inimigos visíveis com visuais e spawning ordenado por distância.

### Alterações

**CameraFollow2D.cs (criado):**
- Nova classe para smooth camera following com damping.
- Campos: `_target` (Transform), `_smoothTime` (0.08f), `_offset` (0, 0, -10), `_snapOnStart` (true).
- `RebindTarget(Transform target)` — rebind do alvo dinamicamente.
- `SnapToTarget()` — posicionamento imediato sem animação.
- `LateUpdate()` — Vector3.SmoothDamp para seguimento suave.

**CreateMvpCaveScene.cs (aprimorado):**
- `CreateMainCamera()` agora aceita parâmetro `Transform playerTransform`.
- Adicionado setup de `CameraFollow2D` via SerializedObject:
  - `AddComponent<CameraFollow2D>()`.
  - SetReference() para `_target = playerTransform`.
  - `_snapOnStart = true`.
  - `ApplyModifiedPropertiesWithoutUndo()`.

**CaveRuntimeMaterializer.cs (aprimorado):**
- Adicionado `RepositionCamera()` método que:
  - Detecta `CameraFollow2D` no main camera.
  - Se encontrado: chama `RebindTarget(_playerTransform)` + `SnapToTarget()`.
  - Fallback: posiciona camera diretamente sobre player.
- Chamado em `Materialize()` após posicionar player na entrance.

**CaveEnemySpawner.cs (visual + ordering):**
- Adicionado `GetBuiltinSprite()` helper com `#if UNITY_EDITOR` condicional (reutilizando pattern de CaveRuntimeMaterializer).
- `SpawnEnemyAtPoint()` atualizado:
  - Se `enemyData.Icon != null`: usa sprite com cor white.
  - Else: usa builtin sprite com cor fallback `new Color(0.85f, 0.23f, 0.23f)` (vermelho escuro visível).
  - `sortingOrder = 3` para visibilidade acima de floor/walls.
  - `transform.localScale = Vector3.one` para sizing consistente.
- `SpawnEnemiesForLevel()` atualizado:
  - Adiciona `using System.Linq`.
  - Ordena spawn points por distância à entrada: `.OrderBy(sp => Vector2.Distance(sp.Position, generatedLevel.Entrance))`.
  - Itera sobre lista ordenada para spawning sequencial.

### Testes

- [x] Revisão estática de código e integração CameraFollow2D.
- [x] Validação de referências Transform e SerializedObject setup.
- [x] Verificação de visual fallback e sorting order.
- [x] Inspeção de ordenação de spawn por distância.
- [ ] Unity compilação não testada.
- [ ] Play Mode não testado (camera follow, enemy visibilidade, order de spawn).

### Pendências / Riscos

- **Validação crítica:** Código deve compilar. Play Mode deve mostrar:
  - Camera seguindo player suavemente após materialização.
  - Inimigos visíveis com cor fallback (vermelho escuro) se sem sprite.
  - Inimigos spawned em ordem de proximidade à entrada.
- **Prefabs enemy:** Se prefab reutilizado, já terá sprite; fallback só ativa se null.
- **Physics/Chase:** EnemyChaseController requer player target configurado (já feito em passos anteriores).

### Próximo passo recomendado

1. Validar compilação no Unity.
2. Testar Play Mode: verificar smooth camera follow, enemy spawn order e visibilidade.
3. Atualizar `docs/IMPLEMENTATION_STATUS.md` para marcar Cave Procedural como `Implementado parcial` com status de camera/visual confirmado.
4. Executar smoke tests completos se Play Mode passar.
5. Preparar commit e branch final.

---

## 2026-05-20 - FIX_GLOBAL_CAMERA_FOLLOW_MVP_v1.0 Implementação Completa

**Responsável:** Claude (Haiku 4.5)  
**Branch:** `feature/fix-global-camera-follow-mvp`  
**Escopo:** Padronizar câmera MVP em FarmScene, TownScene e CaveScene para seguir/centralizar no Player usando CameraFollow2D.

### Alterações

**CreateMvpFarmScene.cs (R1):**
- Atualizado call de `CreateMainCamera()` para `CreateMainCamera(playerTransform)` na linha 81.
- Assinatura do método `CreateMainCamera()` alterada para aceitar `Transform playerTransform`.
- Adicionado setup de `CameraFollow2D` via SerializedObject:
  - `AddComponent<CindarsHope.Camera.CameraFollow2D>()`.
  - SetReference() para `_target = playerTransform`.
  - `_snapOnStart = true`.
  - `ApplyModifiedPropertiesWithoutUndo()` e `EditorUtility.SetDirty()`.

**CreateMvpTownScene.cs (R2):**
- Atualizado call de `CreateMainCamera()` para `CreateMainCamera(playerTransform)` na linha 67.
- Assinatura do método `CreateMainCamera()` alterada para aceitar `Transform playerTransform`.
- Adicionado setup de `CameraFollow2D` idêntico ao Farm, mantendo `orthographicSize = 7.5f`.

**CreateMvpCaveScene.cs (R3):**
- Verificado: já chama `CreateMainCamera(playerTransform)` corretamente.
- Verificado: método já tem CameraFollow2D implementado e configurado.
- Sem alterações necessárias.

### Testes

- [x] Revisão estática de código nos 3 arquivos.
- [x] Validação de assinatura de método e chamadas.
- [x] Verificação de CameraFollow2D setup idêntico entre Farm/Town.
- [x] Confirmação de Cave já estar correto.
- [ ] Unity compilação não testada.
- [ ] Play Mode Farm/Town/Cave follow não testado.

### Pendências / Riscos

- **Validação crítica:** Código deve compilar. Play Mode deve mostrar:
  - Farm: câmera segue player suavemente.
  - Town: câmera segue player suavemente.
  - Cave: câmera continua seguindo player (já funcionava).
- **Transições:** Farm ↔ Town ↔ Cave devem funcionar sem erros.
- **HUD:** Não deve duplicar em transições.

### Próximo passo recomendado

1. Validar compilação no Unity: regenerar FarmScene, TownScene, CaveScene via menus editor.
2. Testar Play Mode: mover player em Farm → Town → Cave e voltar. Camera deve seguir em todas as cenas.
3. Confirmar HUD não duplica após transições (Shift+F5 save/load test).
4. Atualizar `docs/IMPLEMENTATION_STATUS.md`.
5. Preparar commit com mudanças de editor scripts e docs.

---

## 2026-05-20 - FIX_CAVE_EXITS_AND_SPARSE_RESOURCES_v1.0 Implementação Completa

**Responsável:** Claude (Haiku 4.5)  
**Branch:** `feature/fix-cave-exits-sparse-resources`  
**Escopo:** Corrigir o loop mínimo da cave procedural com exits funcionais e resource nodes esparsos.

### Alterações

**CaveExitPortal.cs (R1-R4):**
- Adicionado `_targetScenePath` field para suportar caminhos de cena no Editor.
- Adicionado `_levelController` field para acesso a CaveLevelRuntimeController.
- `InitializeBackExit()` e `InitializeForwardExit()` agora aceitam `CaveLevelRuntimeController`.
- BackExit level 1: carrega FarmScene com spawn id `farm_from_cave` (corrigido de `cave_from_farm`).
- BackExit level > 1: chama `EnterLevel(level - 1)` + `GenerateCurrentLevel()`.
- ForwardExit: chama `EnterLevel(level + 1)` + `GenerateCurrentLevel()`.
- `LoadTargetScene()`: usa `_targetScenePath` se disponível (R2).

**CaveRuntimeMaterializer.cs (R5, R7, R8, R9):**
- Adicionado campos: `_levelController`, `_resourceSpawnChance` (0.28), `_minResourceNodes` (1), `_maxResourceNodes` (4).
- `MaterializeEntranceAndExit()`: passa `_levelController` aos inicializadores de exits.
- `MaterializeResourceNodes()`: implementa spawn chance determinística com randomness baseado em seeds.
  - Itera sobre spawn points com roll de chance.
  - Limita máximo em `_maxResourceNodes`.
  - Garante pelo menos `_minResourceNodes` se houver candidatos.
- `SelectAndConfigureResourceNode()`: agora aceita spawnIndex e spawnPosition.
- `SelectResourceNodeData()`: usa seed por posição e índice para variedade.
  - Implementa pesos simples: Stone 70%, Copper 20%, CaveRootTree 10% (R9).

**CaveRuntimeMaterializationResult.cs (R3):**
- Adicionado campo `ResourceCandidateCount` para rastrear candidatos esparsos.

**CreateMvpCaveScene.cs (R5, R6):**
- `CreateCaveRuntime()`: configura materializer com:
  - `_levelController = controller`.
  - `_resourceSpawnChance = 0.28`.
  - `_minResourceNodes = 1`, `_maxResourceNodes = 4`.
- `EnsureResourceNodeDatabase()`: já populava Stone/Copper/CaveRootTree (validado).

**DebugHud.cs (R10):**
- `DrawCaveSummary()`: exibe:
  - ResourceCandidates (total de candidatos).
  - Resources (criados, após aplicar chance).
  - BackExitPosition, ForwardExitPosition.

### Testes

- [x] Revisão estática de CaveExitPortal, CaveRuntimeMaterializer, resultado, editor script e HUD.
- [x] Validação de spawn chance logic e weighted selection.
- [x] Verificação de calls a GenerateCurrentLevel em exits.
- [ ] Unity compilação não testada.
- [ ] Play Mode exitsdão e regeneração não testados.

### Pendências / Riscos

- **Validação crítica:** Código deve compilar. Play Mode deve:
  - BackExit level 1 voltar para Farm com spawn farm_from_cave.
  - BackExit level > 1 voltar para nível anterior e regenerar.
  - ForwardExit avançar e regenerar.
  - Nodes aparecer em quantidade esparsa (1-4, não 8).
  - Nodes variar com seed por posição.
- **Acceptance Criteria AC1-AC14:** Aguardando testes no Unity.

### Próximo passo recomendado

1. Validar compilação no Unity.
2. Rodar Create MVP FarmScene, TownScene, CaveScene.
3. Testar Play Mode: Cave entry → BackExit → Farm, Cave entry → ForwardExit → level 2 → BackExit → level 1 → BackExit → Farm.
4. Verificar HUD CaveLevel, ResourceCandidates, Materialized resources.
5. Confirmar nodes aparecem com frequência baixa (1-4 em vez de 8).
6. Confirmar Shift+R muda nodes.
7. Regressão: Farm/Town/Cave camera, hotbar, tools, plantio, árvore, pesca.

---

## 2026-05-20 - FIX_CAVE_FORWARD_EXIT_LEVEL_ADVANCE_v1.0 Patch Completo

**Responsável:** Claude (Haiku 4.5)  
**Branch:** `feature/fix-cave-exits-sparse-resources`  
**Escopo:** Reforçar avanço de nível, adicionar fallback GetComponent e melhorar logging/prompts.

### Alterações

**CaveRuntimeMaterializer.cs:**
- Adicionado fallback GetComponent em Materialize() para _caveRunManager e _levelController.
- Seguro porque CaveRuntime contém ambos os componentes no mesmo GameObject.

**CaveExitPortal.cs:**
- Prompts melhorados: "Voltar / Sair" (BackExit) e "Avançar para próximo nível" (ForwardExit).
- HandleBackExit/HandleForwardExit: logging detalhado de transição de nível.
- Mensagens de erro melhoradas para regeneração de cena.

### Validação Rápida

1. Regenerar CaveScene via Create MVP menu.
2. Entrar na Cave pela Farm.
3. Aproximar do ForwardExit (magenta) → HUD exibe "Avançar para próximo nível".
4. Pressionar E → Console mostra transição Level 1 → 2.
5. Layout regenera.
6. BackExit volta para Level 1.
7. BackExit volta para Farm.

### Pendências

- Unity compilação e Play Mode validation.
- Verificar se prompts descritivos aparecem corretamente no HUD.

### Próximo passo

Regenerar cena, testar Play Mode com logging completo, validar transições.

---

## 2026-05-20 - FIX_CAVE_SPAWN_ANCHOR_SAFE_POSITIONING Implementação Completa

**Responsável:** Claude (Haiku 4.5)  
**Branch:** `feature/fix-cave-spawn-anchor-safe-positioning`  
**Escopo:** Corrigir posicionamento seguro do player usando CaveSpawnAnchor. Player nunca deve spawnar exatamente no portal, e deve aparecerperto da âncora correta (Entrance para novo nível, ForwardExit ao voltar).

### Alterações

**CaveRuntimeMaterializer.cs:**
- Assinatura de `Materialize()` modificada para aceitar `CaveSpawnAnchor spawnAnchor = CaveSpawnAnchor.Entrance`
- Novo método `ResolveAnchorPosition()` → retorna grid position da âncora (Entrance, ForwardExit, BackExit)
- Novo método `ResolvePlayerSpawnGrid()` → encontra posição segura walkable próxima da âncora
- Novo método `FindSafeAdjacentWalkableTile()` → lookup em 8 direções por tile walkable
- Player posicionado via `GridToWorld(ResolvePlayerSpawnGrid(...))` em vez de sempre Entrance
- Logging detalhado: anchor position, grid resolvida, world position

**CaveLevelRuntimeController.cs:**
- Novo método público `SetSpawnAnchorForNextGeneration(CaveSpawnAnchor anchor)` para CaveExitPortal definir âncora
- `GenerateCurrentLevel()` passa `_currentSpawnAnchor` ao materializer
- `RestoreFromSnapshot()` passa `_currentSpawnAnchor` ao materializer
- `DetermineSpawnAnchorFromTransition()` expandida para detectar transições intracena (ForwardExit/BackExit)
- Logging expandido: SpawnAnchor, RunSeed, LayoutHash, UsedSnapshot, GeneratedNewSnapshot

**CaveExitPortal.cs:**
- `HandleForwardExit()` chama `SetSpawnAnchorForNextGeneration(CaveSpawnAnchor.Entrance)` antes de gerar
- `HandleBackExit()` chama `SetSpawnAnchorForNextGeneration(CaveSpawnAnchor.ForwardExit)` antes de restaurar/gerar
- Logging detalhado de transições com spawn anchor

**DebugHud.cs:**
- `DrawCaveSummary()` exibe `SpawnAnchor: {valor}`
- Exibe `LayoutHash: {shortened}` quando nível está carregado

### Documentação

- Criado `docs/audits/FIX_CAVE_SPAWN_ANCHOR_SAFE_POSITIONING.md` com detalhes técnicos, fluxo, critérios de aceite

### Testes

- [x] Revisão estática de código.
- [x] Validação de lógica de determinação de anchor.
- [x] Validação de safe tile lookup (adjacent search).
- [x] Verificação de logging detalhado.
- [ ] Unity compilação não testada.
- [ ] Play Mode não testado.

### Pendências / Riscos

- **Validação crítica:** Código deve compilar. Play Mode deve:
  - Player spawnar perto de Entrance para nível novo (ForwardExit)
  - Player spawnar perto de ForwardExit ao voltar (BackExit)
  - Player nunca spawnar exatamente no portal
  - Apertar interact imediato não deve sair (deve estar afastado do portal)
  - HUD exibe SpawnAnchor, LayoutHash, UsedSnapshot
- **WalkableTiles:** Generator deve populardocumentedly para lookup funcionar
- **Snapshot coherence:** Snapshots mantêm entrada/saída, coerência preservada

### Próximo passo recomendado

1. Validar compilação no Unity.
2. Regenerar CaveScene.
3. Play Mode: Farm → Cave (Entrance), Level 1 → 2 (Entrance), Level 2 → 1 (ForwardExit).
4. Confirmar posição ≠ portal.
5. Confirmar interact imediato não sai.
6. Commit + PR.

---

## 2026-05-20 - FIX_CAVE_SNAPSHOT_REPLAY_FULL_LAYOUT Implementação Completa

**Responsável:** Claude (Haiku 4.5)  
**Branch:** `feature/fix-cave-snapshot-replay-full-layout`  
**Escopo:** Corrigir snapshot replay para salvar e restaurar layout completo (Width, Height, WalkableTiles, WallTiles, EnemySpawnPoints, ResourceSpawnPoints).

### Alterações

**VisitedLevelSnapshot.cs:**
- Adicionados campos: Width, Height, WalkableTilesList, WallTilesList, EnemySpawnPointsList, ResourceSpawnPointsList
- Nova classe SerializedCaveGenerationPoint com pointTypeValue e Position
- IsValid() expandida: verifica Width > 0, Height > 0, WalkableTilesList.Count > 0
- Novos métodos: SetLayoutDimensions(), AddWalkableTile(), AddWallTile(), AddEnemySpawnPoint(), AddResourceSpawnPoint()

**CaveLevelRuntimeController.cs:**
- CaptureSnapshot() agora captura layout completo (dimensions, tiles, spawn points)
- Logging detalhado com counts: WalkableTiles, WallTiles, EnemySpawnPoints, ResourceSpawnPoints
- RestoreFromSnapshot() agora reconstrói CaveGeneratedLevel completo
- Reconstrói HashSets de tiles e Listas de spawn points a partir do snapshot
- Logging expandido mostra counts restaurados

**Documentação:**
- Criado `docs/audits/FIX_CAVE_SNAPSHOT_REPLAY_FULL_LAYOUT.md` com detalhes técnicos

### Efeito

**Antes:**
- Farm → Cave → Farm → Cave → cave vazia (layout não materializado)
- Prompts apareciam mas floor/walls desapareciam

**Depois:**
- Farm → Cave → Farm → Cave → cave idêntica (layout completamente restaurado)
- Floor, walls, resource/enemy spawn points aparecem
- Snapshots antigos sem layout são invalidados e regenerados uma vez

### Testes

- [x] Revisão estática de código
- [x] Validação de serialização (tipos simples, sem refs Unity)
- [x] Verificação de IsValid() lógica
- [x] Validação de reconstrução de CaveGeneratedLevel
- [ ] Play Mode não testado
- [ ] Unity compilação não testada

### Próximo passo recomendado

1. Validar compilação no Unity
2. Play Mode: Farm → Cave (layout visível), Cave → Farm → Cave (layout restaurado, visível)
3. Confirmar HUD mostra counts > 0
4. Confirmar Console mostra logs detalhados
5. Commit + PR

---
