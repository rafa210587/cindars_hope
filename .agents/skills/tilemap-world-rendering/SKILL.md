---
name: tilemap-world-rendering
description: Renderizar o MUNDO 2D top-down (chão, cidade, fazenda, interiores) do jeito certo em Unity — Tilemap + Rule Tiles para chão, casas como prefab/sprite com volume (não quad esticado), Y-sort para profundidade, import pixel-art correto e colisão por CompositeCollider2D. Usar ao wire/gerar arte de cena de mundo, ao ver seams/grade no chão, ou ao encontrar o erro "Cannot generate 9 slice ... size too big".
---

# Skill: Renderização de Mundo 2D (Tilemap + Sprites)

Guia de melhores práticas para materializar o mundo (FarmScene, TownScene, CaveScene, interiores) com boa aparência e sem os erros clássicos. Compilado de fontes oficiais/comunidade (links no fim). Nasceu de uma tentativa ruim: texturizar chão grande com `SpriteRenderer` em modo **Tiled**, que gerou grade feia, "grama estranha ao andar" e o erro de mesh.

## Quando usar

- Wire ou geração de arte de cena de mundo (chão/tiles, casas, props, vegetação, interiores) nos scene creators (`CreateMvp*Scene.cs`).
- Grama/chão aparece como **grade** ou com **seams/linhas** entre tiles, ou "estranho ao andar".
- Erro no console: `Cannot generate 9 slice most likely because the size is too big. Requires N vertices...`.
- Casas/prédios parecem **retângulos chapados** (parede/telhado esticados).

## Quando NÃO usar

- Sprites de personagem/NPC/monstro (pipeline próprio — ver `sprite-generation-pipeline`, `pixel-art-prompt-authoring`).
- UI/HUD (ver `hud-canvas-binding`, `ui-projection-pattern`).

## Regra de ouro (o erro que originou esta skill)

**NUNCA use `SpriteRenderer` com `drawMode = Tiled` para cobrir áreas grandes de chão.** O Tiled/9-slice estoura acima de ~**127 tiles por eixo** (`Cannot generate 9 slice ... too big`) e, mesmo abaixo disso, dá repetição chapada. Tiled só serve para peças pequenas (piso de uma casa, uma parede). Chão grande = **Tilemap**.

> Guard temporário no código: se precisar de um SpriteRenderer de chão, caia para `Simple` (esticado) quando `área/tileUnits > ~100` por eixo (ver `WorldSpriteLibrary.ConfigureGround`). Isso só evita o erro — a solução boa é Tilemap.

## Chão: Tilemap + Rule Tiles

1. Um `Grid` na cena; sob ele, **Tilemaps separados por camada**, cada um com seu `TilemapRenderer` e `sortingOrder` incremental:
   - `Ground` (base, modo **Chunk**), `Path/Deck`, `Water`, `Decoration` (flora transparente), `Obstacles` (colisão, modo **Individual** p/ Y-sort), `Foreground`.
2. **Rule Tiles** (pacote **2D Tilemap Extras**) para o chão externo: transições automáticas grama↔caminho↔água (bordas) e **saída "Random"** para variar o sprite por célula → acaba a repetição chapada. Interiores: pintura **manual**, `floor` e `wall` em tilemaps separados (preenche e apaga), não rule tiles.
3. `cellSize` do Grid casado com o tile (ver PPU abaixo): tile de 64px @ PPU 64 → cellSize = 1.

## Import pixel-art (obrigatório p/ não borrar / não dar seam)

- **PPU = tamanho do tile em px** (tile 64px → PPU 64; tile 32px → PPU 32). Consistência de PPU entre tiles evita desalinhamento. (No projeto, o `GeneratedSpriteImporter` força PPU 128 — faça **carve-out** para `Art/Generated/World/tiles` usarem PPU do tamanho do tile.)
- `Filter Mode = Point`, `Compression = None`, `Mip Maps = off`.
- `Mesh Type = Full Rect` (Tight quebra Tiled e pode dar gap).
- Pivô: `Pivot Unit Mode = Pixels`, base (BottomCenter) para objetos em pé.

## Seams/gaps entre tiles (causas e correções)

- **Anti-aliasing**: desligar na Camera (ou Quality). AA cria linhas entre tiles.
- **Sprite Atlas** com `Filter=Point` e `Compression=None` (solução "extremamente confiável" para gaps) — e/ou **extrude/padding** nos tiles.
- **Pixel Perfect Camera** com **Pixel Snapping ON**: snap dos renderers a uma grade em world-space, elimina gaps/seams e blur.

## Profundidade (player passa atrás/na frente das casas)

- Render Pipeline / Graphics: `Transparency Sort Mode = Custom Axis`, eixo **Y** (Z/Y trocados).
- Nos renderers: `Sprite Sort Point = Pivot`, pivô na **base**.
- Tilemaps de obstáculo/casas: `Renderer Mode = Individual` (não Chunk) para Y-sort por peça.

## Casas / objetos grandes = prefab/sprite com volume (NÃO quad esticado)

- Casas, árvores e prédios grandes entram como **prefab** (GameObject Brush) ou objeto com **sprite de volume**, com collider próprio — **nunca** um `SpriteRenderer` de cor/textura esticado por `localScale` (o que dava o "retângulo chapado").
- Casa modular = peça de **parede frontal** + **telhado inclinado (3/4)** + **porta** compостas, Y-sorted, com o telhado sumindo ao entrar (`RoofRevealController` já existe no projeto).
- Telhado NÃO é textura tileável: é peça inteira; não tilar.

## Cave procedural: 3 camadas (decisão 2026-07-03)

A CaveScene é gerada por seed (grid de células), então o terreno dela segue **3 camadas** — não
tentar "moldes" como base do terreno:

1. **Chão/parede = Tilemap + Rule Tiles** pintados em runtime pelo materializer a partir do grid
   do level plan. Variação de tile por hash determinístico da célula (`worldSeed|runSeed|level|x|y`)
   — nunca `Random` (rule `cave-stable-run`). Tiles/sprites vêm do `CaveBiomeArtProfileSO` da banda
   (spec CV01), com fallback aos placeholders se o profile estiver vazio.
2. **Props grandes = sprites livres** (cogumelo gigante, pilar, estátua): Y-sort + collider
   próprio, posicionados pelo planner — não são tiles.
3. **Stamps (moldes autorados) só para set-pieces** (sala de tesouro, arena do boss, entrada) —
   exceção, nunca a base do terreno.

Referência de direção: `docs/design/gameplay/cave/CAVE_BIOME_VISUAL_REFERENCE.md` §3.9.

## Colisão

- `TilemapCollider2D` + `CompositeCollider2D` na camada de obstáculo (Rigidbody2D **Static**, "Used By Composite"). Tiles transparentes/decoração: `Collider Type = None`.

## Integração no projeto (editor generation)

- Construa os Tilemaps via editor API dentro dos scene creators (`CreateMvp*Scene.cs`); **sem `[MenuItem]` avulso** — registre no fluxo dos 3 comandos (`Inicializar/Validar/Reparar`), ver [editor-generation-orchestration](../../rules/editor-generation-orchestration.md).
- Tiles/Rule Tiles são assets (`AssetDatabase.CreateAsset`) para persistir na cena; carregue sprites via `WorldSpriteLibrary` (Art/Generated/World).
- Arte de mundo staged por `art/world_gpt/_stage_world_to_assets.py`; tiles de chão passam por reseam (crop de moldura + blend de bordas) para tilar sem grade.

## Verificação

- Regenerar a cena (Unity `CindarsHope/Inicializar Projeto`) e conferir **no Play** (não só no editor): sem erro de 9-slice no console, chão sem grade/seam, player passando atrás/na frente das casas.
- `dotnet build` das duas assemblies (exit 0) para o código do gerador.

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
