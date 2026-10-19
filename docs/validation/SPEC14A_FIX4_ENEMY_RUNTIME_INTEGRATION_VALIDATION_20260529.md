# SPEC 14A-FIX4 — Enemy Runtime Integration

**Data:** 2026-05-29  
**Branch:** `dev`  
**Sessão:** 29g  
**Status:** IMPLEMENTADO EM CÓDIGO — Unity batchmode pendente

---

## 1. Causa raiz dos bugs observados em Play Mode

### Bug A: Level 30 e 45 aparecem com Enemies: 0

- `BuildUnlockedFactionLockIds` em `CaveEnemySpawnPlanner` checava apenas `IsUnlockedByDefault` e `RequiredBossGateId`.
- `EnemyFactionLockSO.RequiredCaveLevelMin` existia mas **nunca era checado**.
- Locks `lock_after_gate_15` (min=16), `lock_after_gate_30` (min=31), `lock_after_gate_45` (min=46) ficavam fora de `request.UnlockedFactionLockIds`.
- `TryRejectProfile` rejeitava todos os perfis/packs daqueles bandas por FactionLockId não desbloqueado.

### Bug B: Todos os inimigos têm comportamento igual (chase/contact damage)

- `CaveRuntimeMaterializer.ConfigureEnemyRuntimeObject` chamava `brain.Configure(enemyData)` — não injetava `_actionSetDatabase`, `_actionDatabase` nem `_telegraphDatabase`.
- `EnemyBrain.InitActionSet()` retornava cedo quando `_actionSetDatabase == null` → nenhum action set inicializado.
- `EnemyChaseController` era **sempre adicionado e habilitado**, independente de `MovementProfileId`.

### Bug C: Todos os inimigos com escala visual igual

- `ConfigureEnemyRuntimeObject` aplicava `enemyData.VisualScale` diretamente (valor padrão 1.0 para todos).
- `EnemySizeProfileSO.SpriteScale` e `ColliderRadius` nunca eram consultados.
- Três database SOs necessários não existiam: `EnemyMovementProfileDatabaseSO`, `EnemyVulnerabilityProfileDatabaseSO`, `EnemySizeProfileDatabaseSO`.

---

## 2. Arquivos alterados

| Arquivo | Mudança |
|---------|---------|
| `Assets/_Game/Scripts/Cave/Runtime/CaveEnemySpawnPlanner.cs` | `BuildUnlockedFactionLockIds` recebe `int caveLevel`; checa `RequiredCaveLevelMin` |
| `Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs` | 6 novos `[SerializeField]` de databases; `ConfigureEnemyRuntimeObject` resolve profiles e usa `brain.ConfigureRuntime`; `EnemyChaseController` desabilitado quando movement profile disponível; log `CombatLog: EnemyRuntimeConfigured` |
| `Assets/_Game/Scripts/Enemy/EnemyBrain.cs` | Campo `_vulnerabilityProfile`; método `ConfigureRuntime(...)` injetando todos os databases; `TryOpenVulnerabilityWindow` usa `_vulnerabilityProfile` quando disponível |
| `Assets/_Game/Scripts/Combat/Data/EnemyMovementProfileDatabaseSO.cs` | Novo: `DataRegistrySO<EnemyMovementProfileSO>` |
| `Assets/_Game/Scripts/Combat/Data/EnemyVulnerabilityProfileDatabaseSO.cs` | Novo: `DataRegistrySO<EnemyVulnerabilityProfileSO>` |
| `Assets/_Game/Scripts/Combat/Data/EnemySizeProfileDatabaseSO.cs` | Novo: `DataRegistrySO<EnemySizeProfileSO>` |
| `Assets/_Game/Scripts/Editor/Validation/ValidateSpec14AEnemyRuntimeIntegration.cs` | Novo validator com MenuItem `CindarsHope/Validation/Validate SPEC 14A - Enemy Runtime Integration` |
| `Assembly-CSharp.csproj` | Adicionadas 3 entradas Compile para os novos database SOs |
| `Assembly-CSharp-Editor.csproj` | Adicionada entrada Compile para o novo validator |

---

## 3. Detalhes das correções

### Fix A — BuildUnlockedFactionLockIds com RequiredCaveLevelMin

**Antes:**
```csharp
private static List<string> BuildUnlockedFactionLockIds(
    CaveRunManager runManager,
    IEnumerable<EnemyFactionLockSO> factionLocks)
// Só checava IsUnlockedByDefault e RequiredBossGateId
```

**Depois:**
```csharp
private static List<string> BuildUnlockedFactionLockIds(
    CaveRunManager runManager,
    IEnumerable<EnemyFactionLockSO> factionLocks,
    int caveLevel)
// Adicional: if (RequiredCaveLevelMin > 0 && caveLevel >= RequiredCaveLevelMin) → unlock
```

Resultado esperado:
- Cave Level 30: desbloqueia `lock_after_gate_15` (min=16) → perfis da banda 11-25 aceitos
- Cave Level 30: desbloqueia `lock_after_gate_30` (min=31)? Não — 30 < 31. Mas com `lock_after_gate_15` desbloqueado há inimigos fungal.
- Cave Level 45: desbloqueia `lock_after_gate_15` (min=16) e `lock_after_gate_30` (min=31) e `lock_after_gate_45` (min=46)? Não — 45 < 46. Mas com bandas 11-25 e 26-40 desbloqueadas.

### Fix B — ConfigureRuntime com databases

`CaveRuntimeMaterializer` agora injeta todos os databases em `brain.ConfigureRuntime(...)`. Action sets e telegraphs inicializam corretamente.

`EnemyChaseController` agora é **desabilitado** quando `MovementProfile` está disponível, evitando conflito com `EnemyBrain.ExecuteMovement()`.

### Fix C — SizeProfile e collider

`ConfigureEnemyRuntimeObject` agora:
- Busca `EnemySizeProfileSO` via `_sizeProfileDatabase` usando `enemyData.SizeProfileId`.
- Aplica `sizeProfile.SpriteScale` para escala visual (fallback: `enemyData.VisualScale`).
- Aplica `sizeProfile.ColliderRadius` para o collider (fallback: switch por SizeClass string).

### Fix D — VulnerabilityProfile via dados

`TryOpenVulnerabilityWindow` em `EnemyBrain` usa `_vulnerabilityProfile.WindowDurationSeconds`, `.Multiplier` e `.CooldownSeconds` quando disponíveis (fallback: 1.5f, 1.5f, 10f).

---

## 4. Log esperado por inimigo

```
CombatLog: EnemyRuntimeConfigured. Name=Rato de Basalto, EnemyId=enemy_stone_rat,
InstanceId=L1_room_0_0_enemy_stone_rat_xxxxx, CaveLevel=1, DataLevel=enemy_data,
MovementType=GroundChase, ActionSetResolved=True, VulnerabilityResolved=True,
SizeClass=Small, VisualScale=0.75, HasEnemyBrain=True, HasLegacyChase=False
```

---

## 5. Validações executadas

| Validação | Resultado |
|-----------|-----------|
| `dotnet build Assembly-CSharp.csproj --no-restore` | PASSOU — 0 erros |
| `dotnet build Assembly-CSharp-Editor.csproj --no-restore` | PASSOU — 0 erros |
| `tools/docs/validate_docs.ps1` | PASSOU |
| Unity batchmode compile | BLOQUEADO (Unity Editor aberto durante sessão) |
| Play Mode level 1 enemies | PENDENTE (Unity Editor pendente) |
| Play Mode level 30 enemies | PENDENTE — esse era o bug principal |
| Play Mode level 45 enemies | PENDENTE |
| `CindarsHope/Validation/Validate SPEC 14A - Enemy Runtime Integration` | PENDENTE (Unity Editor) |

---

## 6. Ações pendentes pelo usuário em Unity Editor

1. Regenerar project files: abrir Unity → Assets → Open C# Project (ou aguardar auto-refresh).
2. Criar assets de database:
   - `EnemyMovementProfileDatabaseSO` em `Assets/_Game/Data/Combat/`
   - `EnemyVulnerabilityProfileDatabaseSO` em `Assets/_Game/Data/Combat/`
   - `EnemySizeProfileDatabaseSO` em `Assets/_Game/Data/Combat/`
3. Registrar `EnemyMovementProfileSO`, `EnemyVulnerabilityProfileSO`, `EnemySizeProfileSO` nos databases criados.
4. Wiring no Prefab/Scene: adicionar os 6 novos `[SerializeField]` no `CaveRuntimeMaterializer` Inspector.
5. Rodar `CindarsHope/Validation/Validate SPEC 14A - Enemy Runtime Integration`.
6. Play Mode em Cave Level 1: confirmar inimigos com diferentes velocidades/comportamentos.
7. Play Mode em Cave Level 30: confirmar inimigos presentes (era 0 antes).
8. Play Mode em Cave Level 45: confirmar inimigos presentes (era 0 antes).

---

## 7. Risco residual

- **Databases não criados como assets**: o código referencia os databases via `[SerializeField]`. Sem os assets Unity criados e wired no Inspector, os databases ficam null e o fallback legacy-chase fica ativo.
- **EnemyDataSO sem MovementProfileId/SizeProfileId**: se os assets de inimigo não tiverem esses campos preenchidos, o sistema cai no fallback. O validator reporta quais estão incompletos.
- **RequiredCaveLevelMin = 0 como padrão**: locks sem `RequiredCaveLevelMin` explícito não são desbloqueados por nível — necessário garantir que os locks da banda 26-40 e 41-55 tenham esse campo preenchido nos assets.
- **EnemyChaseController.enabled=false**: apenas desabilita o componente existente. Se o prefab do inimigo tiver `EnemyChaseController` pré-configurado e nenhum `MovementProfileSO` estiver disponível, o chase legacy continua.
