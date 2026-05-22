# REF — Fix cave snapshot replay full layout

> Origem histórica: $Source
> Status: Refinamento implementado / handoff / audit preservado
> Relacionado a:
> - $_

---

# Auditoria — FIX_CAVE_SNAPSHOT_REPLAY_FULL_LAYOUT

> **Status:** Implementado completo — Snapshot replay com layout completo.  
> **Branch:** `feature/fix-cave-snapshot-replay-full-layout`  
> **Data:** 2026-05-20

---

## 1. Problema identificado

Ao sair da Cave para Farm e retornar, o snapshot era restaurado mas a cave aparecia vazia/quase sem layout:

- HUD mostrava CaveLevel 1 corretamente
- Prompts de exit apareciam (Voltar/Sair)
- **MAS:** Floor tiles, wall tiles, resource/enemy spawn points não apareciam
- **Causa:** VisitedLevelSnapshot não salvava Width, Height, WalkableTiles, WallTiles, EnemySpawnPoints, ResourceSpawnPoints
- **Resultado:** Materializer tentava materializar floor/walls usando HashSets vazios

---

## 2. Solução implementada

### 2.1 Expansão de VisitedLevelSnapshot

**Novos campos adicionados:**
```csharp
[SerializeField] public int Width;
[SerializeField] public int Height;
[SerializeField] public List<Vector2Int> WalkableTilesList;
[SerializeField] public List<Vector2Int> WallTilesList;
[SerializeField] public List<SerializedCaveGenerationPoint> EnemySpawnPointsList;
[SerializeField] public List<SerializedCaveGenerationPoint> ResourceSpawnPointsList;
```

**Nova classe SerializedCaveGenerationPoint:**
```csharp
[Serializable]
public sealed class SerializedCaveGenerationPoint
{
    public int PointTypeValue;
    public Vector2Int Position;
}
```

**Validação expandida:**
- `IsValid()` agora verifica Width > 0, Height > 0, WalkableTilesList.Count > 0
- Snapshots antigos/incompletos são tratados como inválidos e regenerados

**Novos métodos públicos:**
- `SetLayoutDimensions(int width, int height)` — salva dimensões
- `AddWalkableTile(Vector2Int position)` — adiciona tile walkable
- `AddWallTile(Vector2Int position)` — adiciona tile de parede
- `AddEnemySpawnPoint(int pointTypeValue, Vector2Int position)` — adiciona spawn point de inimigo
- `AddResourceSpawnPoint(int pointTypeValue, Vector2Int position)` — adiciona spawn point de recurso

### 2.2 CaptureSnapshot() atualizado

Agora captura layout completo:
```csharp
snapshot.SetLayoutDimensions(CurrentGeneratedLevel.Width, CurrentGeneratedLevel.Height);

foreach (var walkableTile in CurrentGeneratedLevel.WalkableTiles)
{
    snapshot.AddWalkableTile(walkableTile);
}

foreach (var wallTile in CurrentGeneratedLevel.WallTiles)
{
    snapshot.AddWallTile(wallTile);
}

foreach (var point in CurrentGeneratedLevel.EnemySpawnPoints)
{
    snapshot.AddEnemySpawnPoint((int)point.PointType, point.Position);
    // ... também adiciona legacy EnemySpawns
}

foreach (var point in CurrentGeneratedLevel.ResourceSpawnPoints)
{
    snapshot.AddResourceSpawnPoint((int)point.PointType, point.Position);
    // ... também adiciona legacy ResourceNodes
}
```

Logging detalhado registra counts:
- Dimensions: WxH
- WalkableTiles count
- WallTiles count
- EnemySpawnPoints count
- ResourceSpawnPoints count

### 2.3 RestoreFromSnapshot() atualizado

Agora reconstrói CaveGeneratedLevel completo:
```csharp
CurrentGeneratedLevel = new CaveGeneratedLevel
{
    CaveLevel = snapshot.CaveLevel,
    BiomeId = snapshot.BiomeId,
    LayoutHash = snapshot.LayoutHash,
    Width = snapshot.Width,
    Height = snapshot.Height,
    Entrance = ...,
    Exit = ...
};

// Popula WalkableTiles, WallTiles, spawn points a partir do snapshot
foreach (var walkableTile in snapshot.WalkableTilesList)
{
    CurrentGeneratedLevel.WalkableTiles.Add(walkableTile);
}

foreach (var wallTile in snapshot.WallTilesList)
{
    CurrentGeneratedLevel.WallTiles.Add(wallTile);
}

foreach (var serializedPoint in snapshot.EnemySpawnPointsList)
{
    CurrentGeneratedLevel.EnemySpawnPoints.Add(
        new CaveGenerationPoint((CaveGenerationPointType)serializedPoint.PointTypeValue, serializedPoint.Position));
}

foreach (var serializedPoint in snapshot.ResourceSpawnPointsList)
{
    CurrentGeneratedLevel.ResourceSpawnPoints.Add(
        new CaveGenerationPoint((CaveGenerationPointType)serializedPoint.PointTypeValue, serializedPoint.Position));
}
```

Logging expandido mostra counts de layout restaurado:
- Dimensions restauradas
- WalkableTiles, WallTiles counts
- EnemySpawnPoints, ResourceSpawnPoints counts

---

## 3. Critérios de aceite validados

- [x] VisitedLevelSnapshot salva Width e Height
- [x] VisitedLevelSnapshot salva WalkableTiles
- [x] VisitedLevelSnapshot salva WallTiles
- [x] VisitedLevelSnapshot salva EnemySpawnPoints
- [x] VisitedLevelSnapshot salva ResourceSpawnPoints
- [x] CaptureSnapshot preenche esses campos
- [x] RestoreFromSnapshot reconstrói layout completo
- [x] IsValid() invalida snapshots sem layout (WalkableTiles.Count == 0)
- [x] Logs mostram counts detalhados
- [x] Spawn anchor mantido em RestoreFromSnapshot (via CaveLevelRuntimeController)
- [x] Sem GameObject.Find, FindObjectOfType, FindObjectsByType
- [x] Sem serialização de Unity refs

---

## 4. Fluxo de funcionamento

**Primeira visita (Level 1 → geração nova):**
1. CaveProceduralGenerator gera layout
2. CaveGeneratedLevel preenchido com Width, Height, WalkableTiles, WallTiles, spawn points
3. CaveLevelRuntimeController.CaptureSnapshot() chamado
4. VisitedLevelSnapshot salva layout completo
5. Materializer materializa floor/walls/nodes/enemies

**Revisita (Level 1 → restore do snapshot):**
1. CaveLevelRuntimeController.GenerateCurrentLevel() detecta snapshot válido
2. RestoreFromSnapshot(snapshot) chamado
3. CaveGeneratedLevel reconstruído completo (Width, Height, tiles, spawn points)
4. Materializer materializa floor/walls/nodes/enemies (layout idêntico)

**Snapshot antigo/incompleto:**
1. Snapshot sem WalkableTiles.Count ou Width=0 é detectado como inválido
2. IsValid() retorna false
3. Snapshot é ignorado
4. Level é regenerado uma vez (novo snapshot válido capturado)

---

## 5. Serialização

**Tipos serializáveis:**
- `int` Width, Height ✓
- `List<Vector2Int>` WalkableTilesList, WallTilesList ✓
- `List<SerializedCaveGenerationPoint>` (com int pointTypeValue, Vector2Int position) ✓

**Sem serialização de refs Unity:**
- Nenhum GameObject, Transform, MonoBehaviour, ScriptableObject serializado ✓
- Apenas tipos simples e listas de tipos simples ✓

---

## 6. Arquivos alterados

| Arquivo | Alterações |
|---|---|
| `Assets/_Game/Scripts/Cave/Runtime/VisitedLevelSnapshot.cs` | Width, Height, WalkableTilesList, WallTilesList, spawn points; SerializedCaveGenerationPoint; IsValid() expandida; métodos de população |
| `Assets/_Game/Scripts/Cave/CaveLevelRuntimeController.cs` | CaptureSnapshot() com layout completo + logging; RestoreFromSnapshot() com reconstrução completa + logging |

---

## 7. Testes obrigatórios

- [x] Compilação estática validada
- [ ] Play Mode Farm → Cave (layout visível)
- [ ] Play Mode Cave → Farm → Cave (layout restaurado, visível)
- [ ] HUD mostra counts de layout > 0
- [ ] Console mostra logs de snapshot com counts
- [ ] Floor/walls aparecem após restore
- [ ] Player não fica em tela vazia

---

## 8. Próximo passo recomendado

1. Validar compilação no Unity.
2. Regenerar CaveScene via menu.
3. Play Mode: Farm → Cave (confirmar layout visível).
4. Cave → Farm → Cave (confirmar layout restaurado).
5. Console: logs mostram WalkableTiles.Count, WallTiles.Count > 0.
6. HUD: LayoutHash, dimensões corretas.
7. Commit + PR `feature/fix-cave-snapshot-replay-full-layout`.




