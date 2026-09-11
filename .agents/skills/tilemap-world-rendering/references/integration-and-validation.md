# Tilemap integration and validation

## Integração no projeto (editor generation)

- Construa os Tilemaps via editor API dentro dos scene creators (`CreateMvp*Scene.cs`); **sem `[MenuItem]` avulso** — registre no fluxo dos 3 comandos (`Inicializar/Validar/Reparar`), ver [editor-generation-orchestration](../../../rules/editor-generation-orchestration.md).
- Tiles/Rule Tiles são assets (`AssetDatabase.CreateAsset`) para persistir na cena; carregue sprites via `WorldSpriteLibrary` (Art/Generated/World).
- Arte de mundo staged por `art/world_gpt/_stage_world_to_assets.py`; tiles de chão passam por reseam (crop de moldura + blend de bordas) para tilar sem grade.

## Verificação

- Regenerar a cena (Unity `CindarsHope/Inicializar Projeto`) e conferir **no Play** (não só no editor): sem erro de 9-slice no console, chão sem grade/seam, player passando atrás/na frente das casas.
- Selecionar gates pela SPEC_VALIDATION_MATRIX_MASTER; se código do gerador mudou e .NET for aplicável, usar `Invoke-UnityGeneratedProjectsBuild.ps1`. Asset-only não exige .NET; reusar evidência verificada.

## Fontes

- Unity — Optimize 2D with Tilemap: https://unity.com/how-to/optimize-performance-2d-games-unity-tilemap
- NotSlot — Unity Tilemap tutorial (layers, rule tiles, Y-sort, prefab brush): https://notslot.com/tutorials/2022/10/unity-tilemap
- Unity Discussions — top-down RPG tilemaps/interiores (floor+wall separados, Y-sort): https://discussions.unity.com/threads/best-ways-to-create-tilemaps-for-top-down-rpg-interiors-maybe-rule-tiles.903038/
- Terresquall — fix gaps/seams (atlas, AA, pixel perfect): https://blog.terresquall.com/2023/03/how-to-fix-gaps-in-your-tiles-in-unitys-2d-tilemaps/
- Pav Creations — pixel-perfect graphics: https://pavcreations.com/pixel-perfect-graphics-in-unity-the-practical-guide/
- Unity Issue Tracker — Tiled >127 estoura 9-slice: https://issuetracker.unity3d.com/issues/sprite-renderer-spams-errors-after-play-or-reopening-scene-when-draw-mode-is-set-to-tiled-and-size-is-bigger-than-127x127

## Relacionados

- `scene-interactable-wiring` — objetos interativos na cena.
- `hud-canvas-binding` / `ui-projection-pattern` — UI (não é mundo).
- `chatgpt-web-sprite-gen` / `sprite-generation-pipeline` — geração da arte.
- rule `editor-generation-orchestration` — os 3 comandos canônicos.
