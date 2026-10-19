# Cave Floor Variety — Fix (2026-07-10)

## Problema

Play Mode confirmou que o chão da cave `biome_stone_cavern` era monótono: `floor_a` e `floor_b`
vinham de crops da MESMA imagem raw (`cave_stone_floor_cobble.png`, apenas deslocados
0.50/0.50 vs 0.35/0.60), então a variação por hash no pool `floorTiles` não aparecia visualmente.

## Arte reusada vs. nova

- **Reusada:** `cave_stone_floor_cobble.png` (já usada para `floor_a`/`floor_detail`/`wall_face`/
  `wall_top` antes desta mudança).
- **Nova (já estava no disco em `art/world_gpt/raw/cave_tiles/`, não regerada nesta tarefa):**
  `cave_stone_floor_dirt.png` (terra marrom com cascalho disperso), `cave_stone_floor_cracked.png`
  (pedra escura rachada, highlights azulados), `cave_stone_ground_litter.png` (folha-guia 3x3,
  fundo cinza uniforme, 9 peças de detrito de chão).

## Mudança 1 — 3 variantes de chão distintas

`art/world_gpt/_reseam_cave_tiles.py`: MANIFEST alterado de `floor_a`/`floor_b`/`floor_detail`
todos derivados de `cave_stone_floor_cobble` para:

| destino | fonte raw | seamless |
|---|---|---|
| `floor_a` | `cave_stone_floor_cobble` (crop central 0.50/0.50) | sim |
| `floor_b` | `cave_stone_floor_dirt` (crop central 0.50/0.50) | sim |
| `floor_c` (novo) | `cave_stone_floor_cracked` (crop central 0.50/0.50) | sim |
| `floor_detail` | `cave_stone_floor_cobble` (crop deslocado 0.65/0.30, inalterado) | sim |

Rodado com `py .\art\world_gpt\_reseam_cave_tiles.py .` — exit code 0, 6 processados / 0 faltando.
Confirmado visualmente (Read das 3 imagens 128x128 geradas): `floor_a` claro/lajota cinza-tan,
`floor_b` marrom escuro com pedrinhas dispersas, `floor_c` pedra escura rachada com traços de
fenda — as 3 são visualmente distintas entre si.

`Assets/_Game/Scripts/Editor/Cave/GenerateCaveBiomeArtProfiles.cs` (`PopulateFromConvention`):
adicionado carregamento de `floor_c.png` (null-safe, mesmo padrão dos demais — ausente na pasta
não gera erro) e inclusão no pool `floorTiles` junto de `floor_a`/`floor_b`.

## Mudança 2 — Fatiamento do cascalho (ground litter)

`art/world_gpt/_slice_cave_guides.py`: adicionada uma nova fonte `TILES_DIR` (raw/cave_tiles) e
uma entrada de `process_sheet` para `cave_stone_ground_litter.png` (grade 3x3, chroma-key padrão
tol=42, mesmo algoritmo já usado nas demais folhas-guia). Mapeamento de célula → nome definido por
inspeção visual da grade:

| célula | conteúdo visual | arquivo |
|---|---|---|
| (0,0) | cluster de pedras médias | `litter_rocks.png` |
| (0,1) | pedrinhas pequenas dispersas | `litter_pebbles.png` |
| (0,2) | rachadura ramificada (variante A) | `litter_crack_a.png` |
| (1,0) | monte de cascalho/poeira | `litter_gravel.png` |
| (1,1) | mancha de musgo | `litter_moss.png` |
| (1,2) | par de cogumelos pequenos | `litter_mushrooms_small.png` |
| (2,0) | rocha única | `litter_rock_single.png` |
| (2,1) | osso | `litter_bone.png` |
| (2,2) | rachadura ramificada (variante B) | `litter_crack_b.png` |

Rodado com `py .\art\world_gpt\_slice_cave_guides.py` — exit code 0. As 9 saídas `litter_*.png`
foram geradas em `Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/`, RGBA com fundo
transparente (confirmado por leitura visual de 4 amostras: `litter_rocks`, `litter_pebbles`,
`litter_mushrooms_small`, `litter_bone` — todas com fundo removido, sem halo cinza residual).

**Órfãos-de-propósito (intencional):** `CaveBiomeArtProfileSO` não tem campo para litter hoje;
os 9 sprites ficam prontos no disco para uma spec futura de placement/densidade de detrito de chão
(mesmo padrão já usado para `chunk_*`/`wall_edge_*` gerados em lotes anteriores). Nenhum wiring de
colocação foi feito nesta tarefa.

## Fora de escopo (confirmado NÃO feito)

- Rule Tile de autotile para `wall_edge_*` — não tocado.
- Placement/densidade do cascalho no planner/materializer da cave — não tocado.
- CV03 / planner / snapshot — não tocado.
- `.unity`/`.prefab` YAML — não tocado.
- `Random`/`GameObject.Find` — não usado (scripts Python de arte + generator C# usam apenas
  `AssetDatabase` determinístico, sem RNG nem busca de cena).

## Validação (rodada pelo agente, exit codes reais)

```
py .\art\world_gpt\_reseam_cave_tiles.py .          -> EXIT: 0 (6 processados, 0 faltando)
py .\art\world_gpt\_slice_cave_guides.py            -> EXIT: 0 (29 arquivos OK, incl. 9 litter_*)
dotnet build .\Assembly-CSharp.csproj --no-restore  -> EXIT: 0 (0 erros)
dotnet build .\Assembly-CSharp-Editor.csproj         -> EXIT: 0 (0 erros; 1101 warnings CS0436
                                                          pré-existentes, tipos duplicados entre
                                                          Assembly-CSharp-Editor.csproj e
                                                          CindarsHope.Editor — não relacionados a
                                                          esta mudança, não introduzidos por ela)
```

Assets no disco confirmados por leitura visual: `floor_a.png`, `floor_b.png`, `floor_c.png`,
`floor_detail.png` (distintos entre si) e os 9 `litter_*.png` (RGBA, fundo transparente) em
`Assets/_Game/Art/Generated/World/cave/biome_stone_cavern/`.

## Ação humana pendente

Rodar **`CindarsHope/Inicializar Projeto`** uma vez no Unity Editor (regenera o
`CaveBiomeArtProfileSO` de `biome_stone_cavern` para incluir `floor_c` no pool `floorTiles` e
materializa o registry em Resources) e então validar em Play Mode que o chão da cave varia
visualmente entre lajota clara / terra escura / pedra rachada.

Inspector wiring required (human action in Unity Editor):
- Nenhum wiring manual de Inspector é necessário — `GenerateCaveBiomeArtProfiles` popula
  `floorTiles` via convenção de pasta (`AssetDatabase.LoadAssetAtPath`) e materializa o registry
  Resources automaticamente ao rodar `CindarsHope/Inicializar Projeto`.
