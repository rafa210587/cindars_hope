# Cave Debug Skip & Confinement Fix

**Date**: 2026-05-21  
**Status**: Ready for validation in Unity Play Mode

---

## Correções Implementadas

### 1. CaveDebugLevelSkipController — Diagnóstico & Rebind

**Arquivo**: `Assets/_Game/Scripts/Cave/Runtime/CaveDebugLevelSkipController.cs`

**Melhorias**:

#### Start() — Diagnóstico Detalhado
```csharp
var hasRunManager = _caveRunManager != null;
var hasLevelController = _levelController != null;
Debug.Log($"CaveDebugLevelSkipController: enabled={_enableDebugLevelSkip}, key={_nextLevelKey}, bypassBossGate={_bypassBossGateForDebugSkip}, hasRunManager={hasRunManager}, hasLevelController={hasLevelController}.", this);
```

**Esperado no Console**:
```
CaveDebugLevelSkipController: enabled=True, key=P, bypassBossGate=True, hasRunManager=True, hasLevelController=True
```

#### Update() — Log ao Pressionar P
```csharp
if (Input.GetKeyDown(_nextLevelKey))
{
    Debug.Log("CaveDebugLevelSkipController: P pressed.", this);
    SkipToNextLevel();
}
```

**Esperado ao pressionar P**:
```
CaveDebugLevelSkipController: P pressed.
DEBUG ONLY: bypassing boss gate for level skip.
CaveDebugLevelSkipController: DEBUG: Level skip 1 -> 2
```

#### TryRebindLocalReferences() — Fallback Automático
```csharp
private bool TryRebindLocalReferences()
{
    if (_caveRunManager == null)
    {
        _caveRunManager = GetComponent<CaveRunManager>();
    }

    if (_levelController == null)
    {
        _levelController = GetComponent<CaveLevelRuntimeController>();
    }

    return _caveRunManager != null && _levelController != null;
}
```

**Comportamento**: Se as referências estiverem nulas no Inspector, tenta rebind via `GetComponent` no mesmo GameObject.

#### SkipToNextLevel() — Diagnóstico de Erro
Se refs ainda estiverem nulas após TryRebindLocalReferences:
```
CaveDebugLevelSkipController: Failed to bind references. hasRunManager=False, hasLevelController=False.
```

---

### 2. CavePlayerPathConfinement — Ajuste de Distância

**Arquivo**: `Assets/_Game/Scripts/Cave/Runtime/CavePlayerPathConfinement.cs`

**Mudança**:
```csharp
// ANTES:
var gridX = Mathf.RoundToInt(worldPos.x + offsetX);
var gridY = Mathf.RoundToInt(worldPos.y + offsetY);

// DEPOIS:
var gridX = Mathf.FloorToInt(worldPos.x + offsetX);
var gridY = Mathf.FloorToInt(worldPos.y + offsetY);
```

**Efeito**: 
- Player consegue encostar mais próximo da parede antes de ser bloqueado
- `RoundToInt` arredonda para cima/baixo no ponto meio (0.5), bloqueando cedo
- `FloorToInt` usa sempre o chão da coordenada, bloqueando mais tarde

---

### 3. CreateMvpCaveScene — Validação de Setup

**Arquivo**: `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpCaveScene.cs`

**Melhorias**:

#### Correção de Serialização — KeyCode.P
```csharp
// ANTES (problemático):
serializedDebugSkip.FindProperty("_nextLevelKey").enumValueIndex = (int)KeyCode.P;

// DEPOIS (correto):
serializedDebugSkip.FindProperty("_nextLevelKey").intValue = (int)KeyCode.P;
```

#### Logs de Criação
```csharp
Debug.Log($"CreateMvpCaveScene: CaveDebugLevelSkipController configured on {runtimeObject.name}. Key={KeyCode.P}, enableDebugLevelSkip=true, bypassBossGate=true.");
Debug.Log($"CreateMvpCaveScene: CavePlayerPathConfinement configured on {playerTransform.gameObject.name}. enableConfinement=true.");
```

**Esperado no Console durante CreateMvpCaveScene**:
```
CreateMvpCaveScene: CaveDebugLevelSkipController configured on CaveRuntime. Key=P, enableDebugLevelSkip=true, bypassBossGate=true.
CreateMvpCaveScene: CavePlayerPathConfinement configured on Player. enableConfinement=true.
```

---

## Instruções de Teste

### Pré-requisito: Regenerar CaveScene

**IMPORTANTE**: Após qualquer mudança em `CreateMvpCaveScene`, executar:

1. Abrir Unity Editor
2. Menu: **`CindarsHope/Scenes/Create MVP CaveScene`**
3. Aguardar criação (pode levar alguns segundos)
4. Verificar Console para logs de criação

### Teste 1: Diagnóstico de Startup

**Ação**: Entrar Play Mode

**Esperado no Console**:
```
CreateMvpCaveScene: CaveDebugLevelSkipController configured on CaveRuntime. Key=P, enableDebugLevelSkip=true, bypassBossGate=true.
CreateMvpCaveScene: CavePlayerPathConfinement configured on Player. enableConfinement=true.
CaveDebugLevelSkipController: enabled=True, key=P, bypassBossGate=True, hasRunManager=True, hasLevelController=True
CavePlayerPathConfinement: enabled. Player=Player, LevelController=CaveLevelRuntimeController.
```

**Verificação**:
- [ ] Todos os logs aparecem
- [ ] `hasRunManager=True`
- [ ] `hasLevelController=True`
- [ ] Sem erros vermelhos

---

### Teste 2: Hotkey P Funciona

**Ação**:
1. Play Mode ativo
2. Observar `CaveLevel` no HUD (deve ser 1)
3. Pressionar **P**

**Esperado no Console**:
```
CaveDebugLevelSkipController: P pressed.
DEBUG ONLY: bypassing boss gate for level skip.
CaveDebugLevelSkipController: DEBUG: Level skip 1 -> 2
```

**Verificação**:
- [ ] `CaveLevel` no HUD muda de 1 → 2
- [ ] Logs aparecem na ordem acima
- [ ] `RunSeed` no HUD NÃO muda
- [ ] Nenhum erro vermelho

---

### Teste 3: Player Consegue Encostar Mais Perto da Parede

**Ação**:
1. Play Mode ativo
2. Andar até a borda da área walkável
3. Pressionar para "dentro" da parede/bounds

**Esperado**:
- Player consegue chegar mais perto da parede (comparado a antes)
- Player NÃO passa através da parede
- Player é reverted para `_lastValidPosition`
- Log rate-limited: máx ~1 por segundo de confinement

**Verificação**:
- [ ] Player encostra mais próximo
- [ ] Player não atravessa parede
- [ ] Console não spamma logs de confinement

---

### Teste 4: P Não Marca Boss Derrotado

**Ação**:
1. Pressionar P até level 15
2. Observar boss spawn
3. Sacar console logs

**Esperado**:
- Level 15 spawna boss (red/fallback sprite)
- HUD mostra: `Boss Gates: boss_gate_level_15 (level 15): active` (não defeated)
- Log mostra: `Spawned boss ... at level 15`

**Verificação**:
- [ ] Boss aparece
- [ ] Status é `active`, não `defeated`
- [ ] Logs corretos no console

---

### Teste 5: P Não Desbloqueia Checkpoint

**Ação**:
1. Pressionar P até level 15
2. Sair da caverna (BackExit)
3. Re-entrar caverna
4. Observar checkpoint selection

**Esperado**:
- Checkpoint selection mostra APENAS Level 1
- Auto-seleciona Level 1 (única opção)
- Não mostra Level 15

**Verificação**:
- [ ] Checkpoint selection não inclui 15
- [ ] Auto-seleciona 1

---

## Critérios de Aceite (5/5 = PASS)

| # | Teste | Esperado | Status |
|---|-------|----------|--------|
| 1 | Startup logs | Console mostra diagnóstico completo com `hasRunManager=True` | ⬜ |
| 2 | P hotkey | Pressionar P → level sobe, logs aparecem, seed não muda | ⬜ |
| 3 | Parede | Player encostra mais perto, não atravessa | ⬜ |
| 4 | Boss não marked | Level 15 spawna boss como `active`, não `defeated` | ⬜ |
| 5 | Checkpoint não desbloqueado | Apenas Level 1 em seleção após P skip | ⬜ |

**Final**: ⬜ ___ / 5 PASS

---

## Troubleshooting

### Console: "CaveDebugLevelSkipController: P pressed." não aparece ao pressionar P

**Causa 1**: CaveDebugLevelSkipController não existe
- **Solução**: Regenerar cena via `CindarsHope/Scenes/Create MVP CaveScene`

**Causa 2**: P está mapeado para algo mais
- **Solução**: Verificar no Inspector > Project Settings > Input Manager

**Causa 3**: `_enableDebugLevelSkip = false`
- **Solução**: Inspector de CaveDebugLevelSkipController, marcar checkbox

### Console: "hasRunManager=False" ou "hasLevelController=False"

**Causa**: Referências não foram populadas pelo CreateMvpCaveScene
- **Solução**:
  1. Abrir CaveScene
  2. Procurar GameObject "CaveRuntime"
  3. Inspector > CaveDebugLevelSkipController
  4. Manualmente arrastar "CaveRuntime" para `_caveRunManager`
  5. Arrastar "CaveRuntime" para `_levelController`
  6. Save scene (Ctrl+S)

### Player não consegue encostar perto da parede como esperado

**Causa**: Código antigo com `RoundToInt` ainda em uso
- **Solução**:
  1. Verificar `CavePlayerPathConfinement.cs` linha ~72
  2. Confirmar que usa `FloorToInt`, não `RoundToInt`
  3. Regenerar cena via `CindarsHope/Scenes/Create MVP CaveScene`

### Console mostra erro: "CaveDebugLevelSkipController: Failed to bind references."

**Causa**: CaveRunManager ou CaveLevelRuntimeController não existem ou não estão no mesmo GameObject
- **Solução**:
  1. Verificar que CaveRuntime tem ambos componentes
  2. Se não, regenerar cena
  3. Se regenerar não ajuda, adicionar manualmente

---

## Notas para Merge/PR

- Todas as mudanças são backward-compatible
- Novos logs ajudam diagnóstico (não quebram funcionalidade)
- `FloorToInt` é mais preciso que `RoundToInt` para tile-based movement
- TryRebindLocalReferences é fallback defensivo (não substitui setup correto)

---

## Próximo Passo

Após confirmar que 5/5 testes passam:

1. ✅ Commit: `Fix cave debug skip controller installation and path confinement tolerance`
2. ✅ PR contra `dev`
3. ✅ Merge após review
