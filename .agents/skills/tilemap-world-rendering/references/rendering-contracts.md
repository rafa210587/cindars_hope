# Tilemap world rendering contracts

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
