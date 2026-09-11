# Conífera do keyart — candidato fora de Assets

Escopo: uma única árvore da referência para comparação, sem promoção/importação. Todos os arquivos novos desta fatia usam prefixo `pine` nesta pasta. Nenhuma cópia para Assets, código, Unity ou `.meta` foi feita.

## Fonte e candidato

- Referência: `docs/art_catalog/_reference_keyart_GPT/farm_enclosed_valley_approved_v2.png`, 1536×1024.
- SHA256 da referência confirmado inalterado: `33DF838F664367A1B5559F39A54B76798F2C3990ADA9D03847145C6B1F2C0B9F`.
- Árvore escolhida: conífera ao lado da fonte, a oeste do pomar. Ponta, galhos inferiores e tronco/raízes visíveis na fonte.
- Crop completo: `(330,332,94,151)`; preservado em `pine-source-final.aseprite` e `pine-source-final.png`. A primeira tentativa tinha altura insuficiente; a fonte final foi ampliada antes da máscara.
- Candidato editável: `pine-keyart-candidate.aseprite`, RGB, um frame, duas camadas. Original do recorte oculto; silhueta manual em camada independente. Reabertura PASS.
- PNG: `pine-keyart-candidate.png`, 94×151, sem reamostragem/recoloração.
- Bounds opacos inclusivos: `(10,11)..(75,127)`; tamanho visual 66×117.
- Apoio recomendado: `(43,128)` local, origem superior esquerda, logo abaixo da base; equivalente ao pixel `(373,460)` na referência.
- Pivot normalizado proposto, caso futuramente aprovado para integração: `(0.457447,0.152318)`.

## Inspeção

Fonte nativa e ampliada, PNG candidato e composições em fundos claro/escuro examinados. Alpha medido por leitor independente: 3298 pixels opacos, 10896 transparentes, zero parcial. Não há retângulo de gramado nem checkerboard pintado. Tronco e raízes usam os pixels visíveis da fonte; não foram desenhados trechos ocultos.

`pine-review-backgrounds.png` mostra o candidato sobre fundos claro e escuro. `pine-comparison.png` mostra o pinheiro atual à esquerda e o candidato à direita com alturas visuais de 117 pixels. O asset atual `Assets/_Game/Art/Generated/World/trees/tree_pine.png` é 338×549; apenas seu preview foi reduzido via Aseprite para 72×117. O original permaneceu apenas como entrada de leitura. A comparação não altera a resolução de nenhuma fonte.

## Observação visual e limites

Na mesma altura visual, o candidato apresenta agrupamentos maiores de folhas, sombras mais profundas e brilho verde menos contínuo. O pinheiro atual tem muitos ramos finos luminosos e uma base mais larga na escala comparada. O candidato acompanha diretamente a aparência da árvore na referência, incluindo sua suavidade original; não representa um redesenho com clusters pixel art mais rígidos.

A sombra entre os galhos mantém os pixels escuros da própria fonte; pequenas sobreposições internas de folhagem não foram reconstruídas. A máscara manual recorta o contorno visível, sem preencher partes ausentes. A repetição e a escala em gameplay ainda não foram avaliadas com este candidato. Esta entrega permite comparar variedade; não declara aprovação humana, integração Unity ou substituição do catálogo.

Autoria: Aseprite 1.3.18.5, `pine-isolate.lua`. Comparação de revisão: `pine-compare.lua`. Scripts executados em batch com janela oculta; nenhum editor visual foi aberto.
