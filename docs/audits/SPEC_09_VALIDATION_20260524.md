# SPEC 09 - Validação Completa de Wiring
Data: 2026-05-24 16:20 UTC  
Status: Validação em Progresso (Código OK, Scene Manual Pending)

---

## ✅ BOOTSTRAP INTEGRATION - VERIFICADO

### GameBootstrap.cs (Assets/_Game/Scripts/Core/Bootstrap/)
- ✅ **L25**: `[SerializeField] private GameTimeManager _gameTimeManager;` - campo adicionado
- ✅ **L43**: `public GameTimeManager GameTimeManager => _gameTimeManager;` - propriedade pública
- ✅ **L142-156**: `Initialize()` chama `_gameTimeManager.Initialize()` condicionalmente
  - **L143**: `if (_modalManager != null) { _gameTimeManager.Initialize(); }`
  - Fallback: loga warning se ModalManager ausente
- ✅ **L202-205**: `Shutdown()` chama `_gameTimeManager.Shutdown()` antes de `_timeManager`
- ✅ **L191**: `RebindOptionalRuntimeManagers()` chamado com `_gameTimeManager` como terceiro param

### SaveManager.cs (Assets/_Game/Scripts/Save/)
- ✅ **Signature**: `public void RebindOptionalRuntimeManagers(EquipmentManager equipmentManager, PlayerProgressionManager progressionManager, Core.GameTimeManager gameTimeManager = null)`
- ✅ **L262-265**: Implementação armazena GameTimeManager:
  ```csharp
  if (gameTimeManager != null)
  {
      _gameTimeManager = gameTimeManager;
  }
  ```
- ✅ **L906-923**: `CaptureGameTimeSaveData()` consulta `_gameTimeManager`:
  ```csharp
  if (_gameTimeManager == null) { return default GameTimeSaveData; }
  return new GameTimeSaveData { CurrentPhase = (int)_gameTimeManager.CurrentPhase, ... };
  ```
- ✅ **ApplySaveData()**: Restaura via `_gameTimeManager.RestoreFromSaveData(saveData.GameTime)`

### GameTimeManager.cs (Assets/_Game/Scripts/Core/)
- ✅ **L23-25**: Fallback constants definidas:
  - `DefaultDayDurationSeconds = 600f` (10min)
  - `DefaultNightDurationSeconds = 300f` (5min)
- ✅ **L41-60**: `Initialize()` loga warning se `_timeBalance` null, mas NÃO falha
- ✅ **L84-85**: Pause-aware time:
  ```csharp
  if (_modalManager != null && _modalManager.HasActiveModal) return;
  ```
- ✅ **L96-108**: Phase duration lógica usa fallback quando `_timeBalance` null
- ✅ **L90-93**: GameTimeTickEvent publicado a cada 1 segundo
- ✅ **L129**: GamePhaseChangedEvent publicado na transição

---

## ⚠️ SCENE WIRING - REQUER VALIDAÇÃO MANUAL

### FarmScene, TownScene, CaveScene
**STATUS**: Wiring foi atualizado no CODE para passar GameTimeManager, MAS precisa confirmação em cena (Unity Editor).

#### FarmSceneRuntimeReferenceInstaller.cs
- ✅ **L121**: Atualizado para passar `bootstrap.GameTimeManager`:
  ```csharp
  saveManager.RebindOptionalRuntimeManagers(
      bootstrap.EquipmentManager, 
      bootstrap.PlayerProgressionManager,
      bootstrap.GameTimeManager);
  ```

#### TownSceneRuntimeReferenceInstaller.cs
- ✅ **L39**: Atualizado para passar `bootstrap.GameTimeManager`

#### CaveSceneRuntimeReferenceInstaller.cs
- ✅ **L47**: Atualizado para passar `bootstrap.GameTimeManager`

### Editor Scene Creation Scripts (MVP generation)
- ✅ **CreateMvpFarmScene.cs L155-157**: Atualizado
- ✅ **CreateMvpTownScene.cs L174-176**: Atualizado
- ✅ **CreateMvpCaveScene.cs L162-164**: Atualizado

**CAMPOS QUE DEVEM ESTAR ATRIBUÍDOS NA CENA** (verificar manualmente no Unity Editor):

| Campo | Tipo | Scene | Status |
|-------|------|-------|--------|
| `GameBootstrap._gameTimeManager` | GameTimeManager | Farm/Town/Cave | ⏳ MANUAL CHECK REQUIRED |
| `GameBootstrap._modalManager` | ModalManager | Farm/Town/Cave | ⏳ MANUAL CHECK REQUIRED |
| `GameTimeManager._timeBalance` | GameTimeBalanceSO | GameTimeManager | ⏳ MANUAL CHECK REQUIRED (pode ser null, fallback 10min/5min) |
| `GameTimeManager._timeManager` | TimeManager | GameTimeManager | ⏳ MANUAL CHECK REQUIRED |
| `GameTimeManager._modalManager` | ModalManager | GameTimeManager | ⏳ MANUAL CHECK REQUIRED |

---

## ✅ RUNTIME BEHAVIOR - CÓDIGO VERIFICADO

### GameTimeTickEvent
- ✅ Publicado a cada 1 segundo via `GameTimeManager.Update()`
- ✅ Evento recebido por: `GameTimeTickEvent.cs` subscribers (ex: HUD, systems)

### GamePhaseChangedEvent
- ✅ Publicado ao transicionar Day → Night ou Night → Day
- ✅ Evento recebido por: Systems que need phase change notification
- ✅ `DayStartedEvent` compatível: `HandleDayStarted()` reseta `_phaseTimer`

### Pause-Aware Time
- ✅ `if (_modalManager != null && _modalManager.HasActiveModal) return;` - pula tick se modal ativo
- ✅ Stamina regen respeita pausa (via GameTimeTickEvent subscribers que checam ModalManager)

### Save/Load
- ✅ `CaptureGameTimeSaveData()` serializa `CurrentPhase` + `PhaseElapsedSeconds`
- ✅ `RestoreFromSaveData()` restaura ambos
- ✅ SaveData schema incluído em `ApplySaveData()` chain

---

## ❌ COMPILATION STATUS

**Pre-existing errors** (NOT caused by SPEC 09 wiring changes):
- HazardType not found (EnvironmentalExposureEvents.cs)
- ManaManager not found in CindarsHope.Player (PlayerSpellCaster.cs)

**My changes** (FarmSceneRuntimeReferenceInstaller, etc.):
- ✅ Syntax valid
- ✅ No new compilation errors introduced

---

## 📋 MANUAL VALIDATION CHECKLIST (Unity Editor Required)

Before marking SPEC 09 as 100% complete, verify:

1. **[ ] GameBootstrap Prefab/Scene**
   - `_gameTimeManager` referencia um GameObject com GameTimeManager.cs
   - `_modalManager` referencia um GameObject com ModalManager.cs

2. **[ ] GameTimeManager GameObject**
   - `_timeBalance` atribuído (ou vazio = fallback 10min/5min é OK)
   - `_timeManager` atribuído
   - `_modalManager` atribuído

3. **[ ] Play Mode Test**
   - Entrar em qualquer cena (Farm/Town/Cave)
   - Abrir Console
   - Verificar: `GameTimeTickEvent` aparece a cada ~1 segundo
   - Verificar: `GamePhaseChangedEvent` aparece na troca (ex: após 10min→noite)
   - Verificar: Abrir modal → tempo PARA; fechar modal → tempo continua

4. **[ ] Save/Load Test**
   - Play mode, avançar 2-3 dias
   - Save manualmente
   - Verificar arquivo save contém `GameTimeSaveData` com fase atual + timer
   - Load file
   - Verificar: fase + timer restaurados, conta continua de onde parou

---

## RESULTADO

| Critério | Status | Notas |
|----------|--------|-------|
| Bootstrap código | ✅ 100% | GameTimeManager init/shutdown wired |
| SaveManager rebinding | ✅ 100% | Todas as 3 scenes + 3 MVP scripts atualizados |
| Fallback seguro | ✅ 100% | 10min/5min default se ScriptableObject vazio |
| Pause-aware | ✅ 100% | ModalManager.HasActiveModal check presente |
| Events | ✅ 100% | Tick + Phase publicados corretamente |
| Scene wiring | ⏳ MANUAL | Código OK, precisa confirmar fields atribuídos |
| Compile validation | ⚠️ FAILS | Pre-existing errors (HazardType, ManaManager) - unrelated to SPEC 09 |

---

## PRÓXIMO PASSO

1. **Validação Manual** (1-2h): Abrir cada scene em Unity Editor, confirmar que campos estão atribuídos, fazer play mode test
2. **Se passou**: Marcar SPEC 09 como 100% completa, ir para SPEC 10
3. **Se falhou**: Corrigir fields ausentes em cena, re-testar

**IMPORTANTE**: Compile validation falhando é OK neste momento - são erros pre-existentes fora do scope de SPEC 09. Só marcar SPEC 09 como completa após validação manual em Play Mode.
