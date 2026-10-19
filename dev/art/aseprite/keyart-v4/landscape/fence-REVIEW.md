# Cerca do keyart — trecho horizontal e diagonal

Escopo autorizado: novos PNGs `World/props/fence_rail_keyart_v4*.png` e fontes/candidatos de cerca nesta pasta. Sem alteração de C#, Unity, `.meta`, física, cenas ou arte preexistente. O owner valida e outro executor integra somente após o handoff.

## Evidência visual e direção

Referência integral aberta: `docs/art_catalog/_reference_keyart_GPT/farm_enclosed_valley_approved_v2.png`, 1536×1024. Também foram abertos `docs/validation/farm_keyart_v4/gameplay_stage9/pasture.png` e `orchard.png`. As travessas atuais aparecem finas em relação aos postes; o trecho da referência junto ao poço mantém duas travessas contínuas com aproximadamente três pixels de espessura nativa. Não foram engrossadas ou recoloridas por algoritmo.

O horizontal vem da pequena cerca à direita do poço. O diagonal vem da borda superior direita do pasto, com inclinação existente na referência; ele foi considerado útil para curvas da cerca sem deformar a peça horizontal. Um crop comparativo do pasto horizontal também foi examinado, mas não virou uma variante extra.

## Entregas e apoios

| PNG final em Assets/_Game/Art/Generated/World/props | Crop fonte (x,y,w,h) | Bounds opacos inclusivos | Dimensão opaca | Apoio local, origem superior esquerda | Apoio na referência | Pivot Unity normalizado |
|---|---|---|---|---|---|---|
| fence_rail_keyart_v4.png | 667,214,45,39 | 5,6 → 37,26 | 33×21 | 22,27 | 689,241 | 0.488889,0.307692 |
| fence_rail_keyart_v4_diagonal.png | 335,562,46,48 | 5,5 → 38,36 | 34×32 | 22,32 | 357,594 | 0.478261,0.333333 |

Pontos para união de segmentos:

- Horizontal: pé do poste esquerdo `(8,26)` local / `(675,240)` na referência; direito `(35,27)` / `(702,241)`. Distância horizontal entre centros: 27 pixels. A base varia um pixel em altura na fonte.
- Diagonal: pé esquerdo `(8,27)` / `(343,589)`; direito `(35,37)` / `(370,599)`. Vetor entre postes `(27,10)` em coordenadas de imagem; y cresce para baixo. O apoio central usa o ponto entre esses pés.

Os apoios são propostos para alinhamento visual. Cada PNG inclui os dois postes; a integração deve usar os centros indicados para controlar as junções. Não há slicing, auto-trim ou metadados de importação nesta entrega.

## Autoria e verificação

Aseprite 1.3.18.5-x64 em batch, scripts `extract.lua` existente e `fence-isolate.lua` novo. Arquivos `.aseprite` têm RGB, um frame e duas camadas: recorte original oculto e máscara manual de madeira visível. Ambos foram reabertos com sucesso. O original plano não foi descrito como uma reconstrução de camadas históricas.

Os postes e cada travessa foram mascarados por polígonos separados. O chão entre as travessas e abaixo delas é transparente. Crops ampliados, previews nativos sobre fundos claro/escuro e PNGs finais copiados foram abertos e examinados.

Leitura independente dos PNGs finais:

| Sprite | Alpha 255 | Alpha 0 | Alpha parcial |
|---|---:|---:|---:|
| Horizontal | 320 | 1435 | 0 |
| Diagonal | 359 | 1849 | 0 |

Amostras do horizontal: `(20,12)` e `(20,20)` nas travessas são alpha255; `(20,16)` no vão e `(20,26)` abaixo da travessa são alpha0. No diagonal: `(22,17)` e `(22,26)` nas travessas são alpha255, `(22,21)` no vão é alpha0. Isso confirma recorte real, sem retângulo de gramado nem checkerboard pintado.

SHA256 da referência confirmado inalterado: `33DF838F664367A1B5559F39A54B76798F2C3990ADA9D03847145C6B1F2C0B9F`.

## Limites

As cores e a suavidade de pixels da referência foram mantidas. Estes trechos não reconstroem madeira oculta nem acrescentam perspectiva ausente na fonte. A diagonal representa a inclinação específica `(27,10)`; os ajustes geométricos da cena pertencem à integração. Espessura, repetição, contatos de postes e leitura com câmera real ainda precisam ser verificados pelo owner no Unity. Execução Aseprite e revisão dos PNGs não equivalem a aceitação humana da cerca integrada.

Arquivos de revisão: `fence_rail_keyart_v4-review.png`, `fence_rail_keyart_v4_diagonal-review.png`. Fontes completas: `fence-source-horizontal.aseprite`, `fence-source-diagonal-final.aseprite`. Candidatos editáveis: `fence_rail_keyart_v4.aseprite`, `fence_rail_keyart_v4_diagonal.aseprite`.
