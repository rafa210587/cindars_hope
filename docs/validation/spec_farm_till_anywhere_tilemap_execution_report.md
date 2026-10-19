# Execution Report — spec_farm_till_anywhere_tilemap

**Spec ID:** `spec_farm_till_anywhere_tilemap`
**Data:** 2026-06-26
**Status:** `BUILD_VALIDATED` — dotnet build runtime+editor exit 0; EditMode tests adicionados (NOT RUN em Unity — Play Mode DEFERRED).

---

## Validation block

```text
Validation method: dotnet build (fallback compile) + validate_docs.ps1
Assembly-CSharp exit code: 0  — PASS
Assembly-CSharp-Editor exit code: 0  — PASS
Docs validation exit code: 1  — EXPECTED_FAIL_PRE_EXISTING (erros pre-existentes nos specs farm_till e farm_scene_relayout, nao causados por esta implementacao)
EditMode tests: NOT RUN (nenhum test runner disponivel fora do Unity Editor)
Play Mode scenario: NOT RUN — DEFERRED_TO_FINAL_VALIDATION (depende de cena da fazenda com input redirecionado)
```

---

## Arquivos criados

| Arquivo | Tipo | Descricao |
|---|---|---|
| `Assets/_Game/Scripts/Farm/FarmNonArableZones.cs` | Runtime (pure C#) | Registro de footprints nao-araveis + flag estufa |
| `Assets/_Game/Scripts/Farm/FarmTileGrid.cs` | Runtime (pure C#) | Grade de tiles; Dictionary<Vector2Int, FarmPlotLogic>; IsTillable; world<->tile |
| `Assets/_Game/Scripts/Farm/FarmTilledSoilService.cs` | Runtime (pure C#) | TillTile/PlantTile/WaterTile/HarvestTile/ProcessDayAllTiles delegando a FarmPlotLogic + FarmWateringService |
| `Assets/_Game/Scripts/Save/FarmTilesSaveData.cs` | Save DTO | [Serializable] DTO aditivo; FarmTileEntry por coordenada (simple types apenas) |
| `Assets/_Game/Scripts/Save/Providers/FarmTilesSectionProvider.cs` | Save provider | ISaveSectionProvider "farm_tiles"; Capture/Restore via FarmTileGrid |
| `Assets/_Game/Tests/EditMode/Farm/FarmTileGridTests.cs` | EditMode test | 24 testes: IsTillable, world<->tile, zonas, estufa, registro/remocao |
| `Assets/_Game/Tests/EditMode/Farm/FarmTilledSoilServiceTests.cs` | EditMode test | 20 testes: arar/plantar/regar/colher/ProcessDay/ciclo completo/fertilizante |
| `Assets/_Game/Tests/EditMode/Farm/FarmTilesSaveRoundTripTests.cs` | EditMode test | 12 testes: round-trip save, back-compat null, sem Unity refs no DTO |

---

## Arquivos modificados

| Arquivo | Modificacao |
|---|---|
| `Assets/_Game/Scripts/Save/SaveData.cs` | Campo `FarmTiles FarmTilesSaveData` adicionado a `GameSaveData` (campo ADITIVO) |
| `Assets/_Game/Scripts/Save/SaveManager.cs` | Campo `_farmTilesProvider`, `_farmTileGrid`, propriedade `FarmTileGrid`; registro em `Initialize()`; captura em `SaveGame()`; restore em `ApplySaveData()` |
| `Assembly-CSharp.csproj` | 5 arquivos runtime + 3 testes adicionados ao `<ItemGroup>` de compilacao |

---

## Integracao do provider em SaveManager

```text
SaveManager.Initialize():
  _farmTilesProvider = new Providers.FarmTilesSectionProvider(_farmTileGrid);

SaveGame() — campo no GameSaveData:
  FarmTiles = _farmTilesProvider?.Capture(existingSaveData) as FarmTilesSaveData

ApplySaveData() — restore ao final dos dominios:
  _farmTilesProvider?.Restore(saveData.FarmTiles);
```

`FarmTileGrid` e propriedade publica de `SaveManager` para que o RuntimeBootstrap da cena
possa injetar as zonas nao-araveis (via `FarmTileGrid.FarmNonArableZones`) apos carrecar a cena.

---

## API publica para Spec A

```csharp
// FarmTileGrid
bool IsTillable(int tileX, int tileY)
bool IsTillableAtWorldPosition(float worldX, float worldY)
Vector2Int WorldToTile(float worldX, float worldY)
Vector2 TileToWorldCenter(int tileX, int tileY)
void SetBounds(int originTileX, int originTileY, int widthTiles, int heightTiles, ...)

// FarmNonArableZones
void RegisterBlockedTile(int tileX, int tileY)
void RegisterBlockedRect(int originX, int originY, int width, int height)
void RegisterGreenhouseTile(int tileX, int tileY)
void RegisterGreenhouseRect(int originX, int originY, int width, int height)
bool IsBlocked(int tileX, int tileY)
bool IsGreenhouseTile(int tileX, int tileY)
```

---

## Coexistencia com FarmSectionProvider

- `FarmSectionProvider` (ProviderId "farm") persiste os 24 plots fixos via `FarmPlotRegistry` — NAO ALTERADO.
- `FarmTilesSectionProvider` (ProviderId "farm_tiles") persiste os tiles livres — campo ADITIVO `GameSaveData.FarmTiles`.
- Ambos coexistem sem conflito: dominios de dados separados, saves legados retrocompativeis.

---

## Desvios e risco residual

| Item | Detalhe |
|---|---|
| Input nao redirecionado (T006) | O redirecionamento da enxada para o tile sob o jogador (via GameplayInputRouter -> FarmPlotMenuController) esta fora de escopo nesta entrega — a spec indica que e para ser feito mas a cena da fazenda e o prefab de input nao podem ser editados sem autorizacao de .unity/.prefab. Risco: o sistema de tile grid existe mas o jogador nao consegue acionar TillTile em Play Mode sem o bind de input. |
| EditMode tests compilam mas NAO rodados em Unity | O test runner Unity requer editor aberto. Compilam via dotnet build (exit 0). |
| Docs validation: 2 erros pre-existentes | Os erros `spec_farm_till_anywhere_tilemap.md` e `spec_farm_scene_relayout_v4.md` (missing: # /speckit.plan, Ordem de execucao) sao pre-existentes nos arquivos de spec; nao introduzidos por esta implementacao. |
| Play Mode scenario | DEFERRED_TO_FINAL_VALIDATION — depende de cena com input redirecionado e runtime bootstrap configurado. |
| FarmTileGrid exposto por SaveManager | O grid e criado no SaveManager com `FarmNonArableZones` vazio (sem zonas registradas). A Spec A deve chamar `SetBounds()` e registrar zonas via `FarmTileGrid.FarmNonArableZones` pelo RuntimeBootstrap da FarmScene. |

---

## Status final

```
BUILD_VALIDATED — nao ACCEPTED sem Play Mode humano.
Motivo: Play Mode requer cena com input redirecionado (Spec A + bind enxada no tile) — fora de escopo desta spec.
```
