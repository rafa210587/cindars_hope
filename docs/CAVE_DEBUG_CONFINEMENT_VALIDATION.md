# Cave Debug & Confinement Validation Checklist

**Date**: 2026-05-21  
**Scope**: Verificação de CaveDebugLevelSkipController (P hotkey) e CavePlayerPathConfinement (boundary check)  
**Status**: Ready for Unity Play Mode validation

---

## Implementation Summary

### 1. CaveDebugLevelSkipController
- **Namespace**: `CindarsHope.Cave.Runtime` (corrected from `CindarsHope.Cave.Debug`)
- **Hotkey**: `KeyCode.P` (configurável no Inspector)
- **Auto-created by**: `CreateMvpCaveScene` em `CreateCaveRuntime()`
- **Fields**:
  - `_enableDebugLevelSkip = true` (default)
  - `_nextLevelKey = KeyCode.P`
  - `_bypassBossGateForDebugSkip = true`
  - `_caveRunManager` (referência automática)
  - `_levelController` (referência automática)
- **Startup log**: `CaveDebugLevelSkipController: enabled=True, key=P, bypassBossGate=True.`

### 2. CavePlayerPathConfinement
- **Namespace**: `CindarsHope.Cave.Runtime`
- **Auto-created by**: `CreateMvpCaveScene` em `CreateCaveRuntime()`
- **Attach to**: Player GameObject
- **Fields**:
  - `_enableConfinement = true` (default)
  - `_playerTransform` (referência automática)
  - `_levelController` (referência automática)
- **Startup log**: `CavePlayerPathConfinement: enabled. Player=<name>, LevelController=<name>.`
- **Confinement behavior**:
  - Verifica cada frame se player está em `WalkableTiles`
  - Se não estiver, reverte para `_lastValidPosition`
  - Logs rate-limited (máx 1 por segundo)

### 3. Scene Setup Integration
- **CreateMvpCaveScene**:
  - Cria `CaveRuntime` GameObject com `CaveDebugLevelSkipController`
  - Cria `CavePlayerPathConfinement` no Player GameObject
  - Auto-popula referências via SerializedObject
  - Passa `debugSkipController` a `CreateSceneRuntimeInstaller`

- **CaveSceneRuntimeReferenceInstaller**:
  - Recebe `_caveDebugLevelSkipController` field
  - Passa a `DebugHud.RebindExistingCaveRuntime()` no Start

- **DebugHud**:
  - Exibe debug skip status em `DrawDebugLevelSkip()`
  - Mostra: `Debug Level Skip: enabled (Press P)`
  - Mostra: `Last: DEBUG: Level skip X -> Y` (se houver ação)

---

## Pre-Play Mode Checklist

- [ ] Abrir projeto em Unity
- [ ] Assets → Reimport All (ou aguardar auto-reimport)
- [ ] Abrir Console do Unity
- [ ] Verificar que não há erros vermelhos de CS

---

## Play Mode Testing Checklist

### A. Startup Logs
**Esperado no Console ao entrar CaveScene**:
```
CaveDebugLevelSkipController: enabled=True, key=P, bypassBossGate=True.
CavePlayerPathConfinement: enabled. Player=Player, LevelController=CaveLevelRuntimeController.
```

**Se faltarem logs**:
- ❌ CaveDebugLevelSkipController não foi criado ou não tem referências
- ❌ CavePlayerPathConfinement não foi criado ou não tem referências

### B. Debug Level Skip (Hotkey P)

**Setup**:
1. Entrar CaveScene
2. Verificar `CurrentCaveLevel` no HUD (deve ser 1)

**Test 1: P sobe o level**
- Ação: Apertar P
- Esperado:
  - `CurrentCaveLevel` sobe de 1 → 2
  - Log: `CaveDebugLevelSkipController: DEBUG: Level skip 1 -> 2`
  - HUD mostra: `Last: DEBUG: Level skip 1 -> 2`
- ✅ PASS / ❌ FAIL

**Test 2: P não muda CaveRunSeed**
- Ação: Apertar P
- Verificação:
  - `RunSeed` no HUD NÃO muda após o skip
  - Log deve dizer: `DEBUG ONLY: bypassing boss gate for level skip.`
- ✅ PASS / ❌ FAIL

**Test 3: P não marca boss defeated**
- Ação: Apertar P até level 15
- Verificação:
  - Level 15 spawn boss (se não foi derrotado antes)
  - HUD mostra: `Boss Gates: boss_gate_level_15 (level 15): active` (não defeated)
- ✅ PASS / ❌ FAIL

**Test 4: P não desbloqueia checkpoint**
- Ação: Apertar P até level 15, sair caverna, re-entrar
- Verificação:
  - Checkpoint selection NÃO mostra level 15 como opção
  - Deve auto-selecionar só level 1
- ✅ PASS / ❌ FAIL

**Test 5: P respeita boss gate se `_bypassBossGateForDebugSkip = false`**
- Setup: Entrar CaveScene, abrir Inspector de CaveDebugLevelSkipController
- Ação: Desmarcar `_bypassBossGateForDebugSkip`, derrotar boss no level 15, apertar P em level 15
- Verificação:
  - Sem o toggle marcado, P em 15→16 NÃO deve funcionar (bloqueia em log)
  - Log: `CaveDebugLevelSkipController: Cannot skip to level 16. Boss gate blocks advancement.`
- ✅ PASS / ❌ FAIL

### C. Path Confinement (WalkableTiles Boundary)

**Setup**:
1. Entrar CaveScene
2. Observar player em posição central (spawn Entrance)

**Test 6: Player não consegue sair de WalkableTiles**
- Ação: Tentar andar para fora do dungeon (para as paredes/bounds)
- Esperado:
  - Player tenta sair mas é repelido
  - Volta para última posição válida (`_lastValidPosition`)
  - Log: `CavePlayerPathConfinement: Confined player to last valid position ...` (máx 1 log por segundo)
- ✅ PASS / ❌ FAIL

**Test 7: Confinement não spamma console**
- Ação: Ficar pressionado contra a parede por 5 segundos
- Esperado:
  - Máximo ~5 logs de confinement (1 por segundo)
  - NÃO centenas de logs
- ✅ PASS / ❌ FAIL

### D. HUD Display

**Test 8: HUD mostra debug skip status**
- Ação: Abrir HUD debug (Corner direito da tela)
- Verificação:
  - Linha: `Debug Level Skip: enabled (Press P)`
  - Se P foi apertado: `Last: DEBUG: Level skip X -> Y`
- ✅ PASS / ❌ FAIL

**Test 9: HUD mostra boss gates**
- Ação: Abrir HUD debug
- Verificação:
  - Seção: `Boss Gates:`
  - Se level 15 tem boss não derrotado: `boss_gate_level_15 (level 15): active`
- ✅ PASS / ❌ FAIL

### E. Error Handling

**Test 10: Nenhum erro de null reference**
- Ação: Jogar normalmente por 1 minuto
- Verificação:
  - Console NÃO deve mostrar erros vermelhos tipo:
    - `PlayerTransform not assigned`
    - `LevelController not assigned`
    - `CaveRunManager not assigned`
- ✅ PASS / ❌ FAIL

---

## Full Integration Test Sequence

1. **Unity Open**: Abrir projeto
2. **Compile**: Assets → Reimport All, aguardar sem erros
3. **Create Scene**: Menu `CindarsHope/Scenes/Create MVP CaveScene` (ou já existe)
4. **Play Mode**: Apertar Play
5. **Check Logs**: Verificar startup logs (tests A)
6. **Test P Hotkey**: Apertar P, verificar level sobe (tests B1-B5)
7. **Test Boundaries**: Andar para fora da área, verificar confinement (tests C6-C7)
8. **Check HUD**: Abrir DebugHud, verificar displays (tests D8-D9)
9. **Error Check**: Procurar erros vermelhos (test E10)
10. **Result**: Pass/Fail summary

---

## Acceptance Criteria (Full Pass = 10/10)

| # | Test | Status | Notes |
|---|------|--------|-------|
| A1 | Startup logs aparecem | ⬜ | `CaveDebugLevelSkipController enabled` + `CavePlayerPathConfinement enabled` |
| B1 | P sobe level | ⬜ | Level 1→2 ao apertar P |
| B2 | P não muda seed | ⬜ | RunSeed igual antes/depois P |
| B3 | P não marca boss | ⬜ | Boss gate permanece `active` após P |
| B4 | P não desbloqueia CP | ⬜ | Checkpoint 15 não aparece em seleção |
| B5 | P respeita gate (toggle) | ⬜ | Se toggle=false, P em 15→16 bloqueia |
| C6 | Confinement funciona | ⬜ | Player reverte ao sair de bounds |
| C7 | Logs rate-limited | ⬜ | Max ~1 log/sec ao bater em parede |
| D8 | HUD mostra debug skip | ⬜ | `Debug Level Skip: enabled (Press P)` |
| D9 | HUD mostra boss gates | ⬜ | `Boss Gates: boss_gate_level_15 ... active` |
| E10 | Sem erros vermelhos | ⬜ | Console clean durante 1 min de gameplay |

**Final Result**: ⬜ ___ / 11 PASS

---

## Troubleshooting

### Console mostra "PlayerTransform not assigned"
- **Causa**: `CavePlayerPathConfinement` não tem referência a Player
- **Solução**:
  1. Abrir CaveScene no Editor
  2. Procurar `Player` GameObject
  3. Verificar componente `CavePlayerPathConfinement`
  4. Se falta, adicionar manualmente: Add Component → CavePlayerPathConfinement
  5. Arrastar `Player` para campo `_playerTransform`
  6. Arrastar `CaveRuntime` para campo `_levelController`

### Console mostra "LevelController not assigned"
- **Causa**: Similar a acima, mas para `_levelController`
- **Solução**: Ver passo acima, verificar ambas as referências

### P não funciona
- **Causa 1**: `CaveDebugLevelSkipController` não existe
  - **Solução**: Abrir `CaveRuntime` GameObject, verificar componente. Se falta, Add Component → CaveDebugLevelSkipController
- **Causa 2**: Tecla não é P
  - **Solução**: Inspector de `CaveDebugLevelSkipController`, verificar `_nextLevelKey` é `KeyCode.P`
- **Causa 3**: `_enableDebugLevelSkip = false`
  - **Solução**: Inspector, marcar checkbox `_enableDebugLevelSkip`

### Player consegue passar por paredes
- **Causa**: `CavePlayerPathConfinement` desativado ou sem referências
- **Solução**: Ver troubleshooting "PlayerTransform not assigned"

### HUD debug não mostra "Debug Level Skip"
- **Causa**: `DebugHud` não recebeu `CaveDebugLevelSkipController` na rebind
- **Solução**:
  1. Verificar `CaveSceneRuntimeReferenceInstaller` está na cena
  2. Verificar Inspector: `_caveDebugLevelSkipController` está referenciado
  3. Se vazio, arrastar `CaveRuntime` GameObject e pegar seu componente `CaveDebugLevelSkipController`

---

## Notes for Future PRs

- CaveDebugLevelSkipController agora está automaticamente criado por `CreateMvpCaveScene`
- CavePlayerPathConfinement agora está automaticamente criado por `CreateMvpCaveScene`
- Se future PRs modificam `CreateMvpCaveScene`, garantir que ambos permanecem criados e referenciados
- Namespace `Debug` nunca deve ser usado dentro de `CindarsHope.*` (causa collision com `UnityEngine.Debug`)
