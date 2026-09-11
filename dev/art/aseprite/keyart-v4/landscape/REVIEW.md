# Farm keyart v4 — árvore, rocha e animais

Escopo autorizado: dois recortes isolados do keyart aprovado, fontes Aseprite editáveis e PNGs com transparência real. Sem mudanças em cena, C#, YAML ou importadores.

Referência: `docs/art_catalog/_reference_keyart_GPT/farm_enclosed_valley_approved_v2.png`, 1536×1024; SHA256 `33DF838F664367A1B5559F39A54B76798F2C3990ADA9D03847145C6B1F2C0B9F`. Hash confirmado inalterado durante revisão final. Comparação de composição: `docs/validation/farm_aseprite_pilot/editor_v3/farm_capture_keyart_composition.png`.

## Direção e autoria

Observado na referência: folhagem com agrupamentos escuros, luz amarelo-esverdeada localizada e tronco discreto; rochas cinzentas com facetas e musgo. A composição anterior repete árvores maiores e mais luminosas. A escolha destes recortes pretende reduzir esse desvio mantendo pixels e cores da fonte.

Autoria executada no Aseprite 1.3.18.5-x64, CLI/Lua. `extract.lua` fez recortes retangulares em cópias; `isolate-landscape.lua` aplicou polígonos especificados manualmente, sem geração, recoloração ou alteração da resolução. Camada original do recorte preservada invisível; camada de silhueta visível separada. Candidatos reabertos e verificados: RGB, um frame, duas camadas.

| Asset | Recorte no keyart (x,y,w,h) | Bounds opacos inclusivos | Pivot recomendado, origem superior esquerda | Pivot Unity normalizado |
|---|---|---|---|---|
| tree_keyart_v4_01 | 295,261,117,119 | 19,6 → 102,110 | 59,108 | 0.5043,0.0924 |
| boulder_keyart_v4_01 | 78,502,97,90 | 6,11 → 87,80 | 47,77 | 0.4845,0.1444 |

Pivots são decisões propostas para o pé visual, não metadados preexistentes da imagem. A integração deve aplicar escala conforme a referência e conferir o contato com o terreno.

## Evidência

- Referência, baseline, recortes e PNGs finais abertos pelo visualizador em pixels nativos; recortes também inspecionados ampliados.
- PNG árvore: 5044 pixels alpha 255, 8879 alpha 0, zero alpha parcial.
- PNG rocha: 3821 pixels alpha 255, 4909 alpha 0, zero alpha parcial.
- Composições de revisão sobre fundos claro e escuro examinadas; pequenas flores do terreno junto às raízes e entre rochas removidas por máscaras locais adicionais.
- Contagem independente dos PNGs feita por leitura de pixels System.Drawing; sem alterações nas imagens por esse leitor.
- Fontes originais e camadas preservadas; reabertura dos candidatos passou.
- Silhuetas isoladas sem retângulo de terreno ou checkerboard pintado.
- Escala real em Unity e aceitação humana: não observadas nesta fatia; integração pertence ao owner da cena.

## Limites visuais

A árvore mantém sombras escuras internas entre os agrupamentos de folhas e a ramificação da fonte. Não se identificou gramado aberto entre galhos exigindo máscara interna adicional; não foi usado chroma key, que destruiria folhas. A rocha mantém musgo e pequenas oclusões de folhagem da referência no pé. Essas pequenas áreas integram a silhueta irregular e não constituem um patch retangular de gramado. O keyart contém pixels suaves na própria origem; não há alegação de redesenho em pixel art estrita nem de nova resolução nativa.

## Entrega

- `tree_keyart_v4_01.aseprite` e `boulder_keyart_v4_01.aseprite`: candidatos em camadas.
- `tree-source.aseprite` e `boulder-source.aseprite`: recortes intactos.
- PNGs aprovados nesta revisão técnica copiados para `Assets/_Game/Art/Generated/World/foliage/tree_keyart_v4_01.png` e `Assets/_Game/Art/Generated/World/props/boulder_keyart_v4_01.png`.
- Importação deve usar Point, Compression None e mipmaps desativados; owner da integração cuida desses contratos.

## Extensão autorizada — vaca e ovelha

O owner solicitou adicionalmente uma vaca lateral e uma ovelha extraídas do mesmo pasto do keyart. O diretório `Assets/_Game/Art/Generated/World/animals/` foi confirmado antes da gravação. Aseprite, preservação da camada original, máscara manual e reabertura seguiram o mesmo processo. PNGs e revisões em fundo claro/escuro examinados em pixels nativos; recortes examinados também ampliados.

| Asset | Recorte no keyart (x,y,w,h) | Bounds opacos inclusivos | Pivot recomendado, origem superior esquerda | Pivot Unity normalizado | Alpha 255 / 0 / parcial |
|---|---|---|---|---|---|
| cow_keyart_v4_01 | 209,596,59,46 | 3,8 → 50,36 | 28,36 | 0.4746,0.2174 | 779 / 1935 / 0 |
| sheep_keyart_v4_01 | 328,625,50,39 | 11,7 → 43,32 | 28,32 | 0.5600,0.1795 | 598 / 1352 / 0 |

Os corpos laterais mantêm tamanho e cor da referência. São sprites estáticos de um frame; esta entrega não cria animações. O recorte da vaca conserva cauda e pernas separadas na silhueta. A ovelha conserva o sombreado escuro original sob a lã. A contagem de alpha foi repetida por leitura independente dos PNGs finais. Hash da referência permanece igual ao registrado acima. Fontes `.aseprite`, crops, scripts, logs e previews ficam nesta pasta; PNGs finais estão em `World/animals/`. Nenhuma alteração em C#, YAML, `.meta` ou Unity foi feita nesta fatia.

## Extensão autorizada — casa e estufa

O owner autorizou adicionalmente as duas construções para corrigir proporções e fidelidade da fachada. O caminho final usa `World/building/`, conforme contrato singular de WorldSpriteLibrary.Building. As duas cópias inicialmente criadas em `buildings/` foram movidas para o diretório correto após verificar os caminhos absolutos dentro do workspace; nenhuma pasta existente foi removida. A autoria mantém os pixels do keyart, recorte original oculto e máscara manual em camada separada. Candidatos reabertos com sucesso; versões nativas e composições sobre fundos claro/escuro examinadas.

| Asset | Recorte no keyart (x,y,w,h) | Bounds opacos inclusivos | Apoio da entrada local, origem superior esquerda | Apoio na referência | Pivot Unity normalizado | Alpha 255 / 0 / parcial |
|---|---|---|---|---|---|---|
| farmhouse_keyart_v4 | 936,145,212,205 | 21,12 → 197,191 | 122,192 | 1058,337 | 0.57547,0.063415 | 23888 / 19572 / 0 |
| greenhouse_keyart_v4 | 1127,177,129,158 | 10,6 → 103,147 | 58,148 | 1185,325 | 0.44961,0.063291 | 11687 / 8695 / 0 |

A casa contém telhado, chaminé, fachada, varanda e escada completos. O apoio indicado é o centro da base da escada, logo abaixo do último pixel opaco. A estufa contém vidros, armação, fachada e base da entrada; o apoio indicado está logo abaixo da base da porta. Barris externos, cercas soltas e terreno foram excluídos da silhueta. A fachada conserva plantas de floreiras e pequenas oclusões florais presentes na fonte junto à base; não foi inventada arquitetura atrás dessas oclusões. Nenhuma dessas áreas é um patch retangular de chão.

PNGs finais: `Assets/_Game/Art/Generated/World/building/farmhouse_keyart_v4.png` e `Assets/_Game/Art/Generated/World/building/greenhouse_keyart_v4.png`. Dimensões não foram reamostradas. Alpha dos PNGs finais medido independentemente; hash da referência permanece igual ao registrado. Não foram alterados RoofRevealController, interior, porta, collider, C#, YAML, importadores ou `.meta`. A integração deve preservar o contrato de ocultação do exterior e posicionar porta/interior pelo apoio informado.

## Extensão autorizada — barril

`Assets/_Game/Art/Generated/World/props/barrel_keyart_v4.png` foi extraído do barril imediatamente à direita da escada da casa. Recorte da fonte `(1085,310,33,37)`, bounds opacos inclusivos `(6,7)..(26,31)`. Apoio local recomendado `(17,32)`, equivalente ao pixel `(1102,342)` da referência; pivot Unity `(0.51515,0.13514)`.

O barril conserva madeira, aros e abertura superior esverdeada visível na própria referência. Não foi inventada uma tampa. Silhueta revisada em pixels nativos sobre fundos claros e escuros; original ampliado também examinado. Aseprite RGB, um frame, duas camadas, original oculto preservado, reabertura PASS. Alpha final medido independentemente: 395 pixels opacos, 826 transparentes e zero parcial. Nenhum retângulo de terreno, código, `.meta` ou Unity foi alterado. Fontes, máscaras e previews permanecem nesta pasta.

## Extensão autorizada — quatro construções do sul, píer e barco

Seis peças adicionais foram extraídas do mesmo keyart, novamente aberto integralmente antes da escolha das regiões. Crops nativos, ampliações e revisões em fundos claro/escuro examinados. PNGs finais também abertos após a cópia. O processo continua usando exclusivamente Aseprite para autoria, máscara manual, original oculto e silhueta em camada separada. Todos os candidatos foram reabertos: RGB, um frame, duas camadas. Hash da referência continua igual ao registrado.

| Asset | Diretório World | Recorte fonte (x,y,w,h) | Bounds opacos inclusivos | Apoio local (x,y), origem superior esquerda | Apoio na referência | Pivot Unity normalizado | Alpha 255 / 0 / parcial |
|---|---|---|---|---|---|---|---|
| coop_keyart_v4 | building | 475,630,108,146 | 20,11 → 95,101 | 57,95 | 532,725 | 0.527778,0.349315 | 5417 / 10351 / 0 |
| barn_keyart_v4 | building | 575,614,125,166 | 11,7 → 111,132 | 63,131 | 638,745 | 0.504000,0.210843 | 10915 / 9835 / 0 |
| cheese_shed_keyart_v4 | building | 694,632,116,149 | 12,9 → 104,132 | 54,133 | 748,765 | 0.465517,0.107383 | 8809 / 8475 / 0 |
| wine_shed_keyart_v4 | building | 801,625,119,154 | 11,13 → 105,140 | 62,138 | 863,763 | 0.521008,0.103896 | 9971 / 8355 / 0 |
| dock_keyart_v4 | props | 977,601,93,137 | 12,7 → 84,123 | 48,74 | 1025,675 | 0.516129,0.459854 | 5316 / 7425 / 0 |
| boat_keyart_v4 | props | 1053,641,76,68 | 8,25 → 65,56 | 35,44 | 1088,685 | 0.460526,0.352941 | 1345 / 3823 / 0 |

Os apoios das construções indicam o centro da entrada/base do degrau. O apoio do píer indica o centro do deck; a entrada pela terra está em `(48,36)` local, pixel `(1025,637)` da referência, e a borda frontal do deck está em `(48,106)`, pixel `(1025,707)`. As estacas alcançam y123 local e permanecem visualmente separadas por alpha real. O apoio do barco indica o centro visual do casco sobre a água. Esses apoios são propostas de alinhamento, não colliders nem metadados herdados.

Detalhes e limites:

- Galinheiro: arquitetura, abertura e pequeno monte de palha encostado à fachada preservados. Cercado, galinhas e chão do pátio foram excluídos; continuam precisando de elementos físicos/visuais próprios na integração.
- Celeiro: telhado cinzento, fachada vermelha, janela e portas abertas preservados. Barris externos e pedras de caminho foram excluídos. O espaço sob a entrada aberta não foi preenchido com chão pintado.
- Queijaria: as duas chaminés, fachada, queijo visível na entrada e degraus preservados. Vasos externos foram excluídos; pequenas flores que já ocluem a base da fachada permanecem, sem inventar parede atrás delas.
- Adega: telhado, fachada aberta, barris internos, degrau e grande barril encostado à ala esquerda preservados. Esse barril oculta parte da fachada na referência e integra esta silhueta. Pedras soltas e flores externas foram excluídas.
- Píer: deck, grades, luminária e estacas mantidos; quatro aberturas da grade foram mascaradas individualmente. Chão da margem, rochas, barco e água externos foram excluídos. Pequenos contatos ocluídos da referência não foram reconstruídos.
- Barco: casco e bancos preservados, sem água. A borda da popa encoberta pelo píer não foi reconstruída. A vara/linha de pesca e o cabo sobre a água não integram este sprite de casco; foram excluídos para entregar uma peça independente sem fragmentos do píer.

Contagem de alpha dos seis PNGs finais repetida independentemente por leitura System.Drawing. Não houve reamostragem, geração de pixels novos, edição de arte preexistente, runtime, C#, YAML, importadores, `.meta` ou Unity. Não há evidência de escala integrada/Play Mode nesta fatia; o owner fará o wiring e validação na cena.

## Extensão autorizada — quadro, suporte de ferramentas e folhas aquáticas

A referência integral e as regiões individuais foram abertas antes da autoria. O quadro central e o suporte com três utensílios redondos usam os pixels exatos das peças do pátio. A pedido do reviewer, também foi isolado um par pequeno de folhas aquáticas verde-oliva para substituir o grupo pálido e grande do catálogo anterior. O primeiro crop sugerido cortava uma das folhas; a região foi ampliada para inspeção e o crop final reposicionado para conter ambas inteiras.

| PNG em World/props | Recorte fonte (x,y,w,h) | Bounds opacos inclusivos | Apoio local (x,y) | Apoio na referência | Pivot Unity normalizado | Alpha 255 / 0 / parcial |
|---|---|---|---|---|---|---|
| noticeboard_keyart_v4.png | 1080,349,79,69 | 8,9 → 69,60 | 39,61 | 1119,410 | 0.493671,0.115942 | 2311 / 3140 / 0 |
| tool_rack_keyart_v4.png | 1158,354,84,67 | 11,8 → 69,55 | 41,56 | 1199,410 | 0.488095,0.164179 | 2378 / 3250 / 0 |
| lilies_keyart_v4.png | 1240,636,38,29 | 5,5 → 30,21 | 18,14 | 1258,650 | 0.473684,0.517241 | 216 / 886 / 0 |

Os apoios do quadro e suporte indicam a base entre os pés. O apoio das folhas indica o centro do grupo sobre a água. As duas folhas têm polígonos separados, com alpha real nos intervalos e ao redor; nenhuma água ou retângulo de fundo acompanha o sprite. Os verdes e contornos escuros foram mantidos, sem recoloração ou escala. O espaço sob o quadro também é transparente; o suporte conserva seu painel de madeira atrás dos utensílios.

Processo e gates: Aseprite 1.3.18.5, fonte original escondida, silhueta separada, RGB, um frame, duas camadas, candidatos reabertos PASS. Originais ampliados, PNGs nativos e composições claro/escuro examinados. Alpha dos PNGs finais medido independentemente; source hash permanece o mesmo. Nenhuma alteração de runtime, código, Unity ou `.meta`. O quadro será usado pelo EvolutionSign existente e o suporte apenas substituirá o visual do PottingWorkbench; essa integração pertence ao owner.

## Extensão autorizada — vegetação discreta e pequena cascata

Foram examinados `docs/validation/farm_keyart_v4/gameplay_stage7/homestead.png` e `border_south.png`, além da referência integral. Os motivos antigos aparecem repetidos como arcos multicoloridos e arbustos compactos com contorno muito marcado. Esta fatia fornece folhagem e flores da própria referência em peças individuais para o owner redistribuir. A autoria não decide a densidade de repetição nem comprova a escala final na cena.

| PNG em World | Recorte fonte (x,y,w,h) | Bounds opacos inclusivos | Apoio local (x,y) | Apoio na referência | Pivot Unity normalizado | Alpha 255 / 0 / parcial |
|---|---|---|---|---|---|---|
| foliage/undergrowth_keyart_v4.png | 577,891,84,74 | 9,10 → 64,64 | 43,65 | 620,956 | 0.511905,0.121622 | 1855 / 4361 / 0 |
| foliage/wildflowers_keyart_v4.png | 958,315,43,36 | 1,4 → 33,29 | 21,30 | 979,345 | 0.488372,0.166667 | 350 / 1198 / 0 |
| props/river_cascade_keyart_v4.png | 1267,549,68,56 | 2,8 → 55,44 | 31,38 | 1298,587 | 0.455882,0.321429 | 944 / 2864 / 0 |

Undergrowth usa uma massa desigual de folhagem da borda sul, incluindo pequenos ramos inferiores. Não recebeu outline, recoloração ou forma circular artificial. Seu tamanho opaco é 56×55 pixels; deve ser integrado abaixo da escala visual das árvores para funcionar como vegetação baixa. A origem contém folhagem densa e sombras entre ramos; não foi inventada separação de folhas ocultas nem trocado o fundo interno por uma cor uniforme.

Wildflowers conserva hastes, folhas e pequenos acentos amarelos, brancos e rosados do jardim ao lado da casa. O crop foi ampliado à esquerda antes da entrega para recuperar uma pétala cortada na primeira tentativa. Polígonos distintos mantêm vazios transparentes entre as hastes. Não há vaso, barril, gramado retangular ou arco de flores reconstruído.

A cascata é uma peça estática de água caindo e espuma na confluência. Pedras, vegetação e margem foram excluídas. O apoio indicado refere-se ao centro da espuma, não ao chão; a crista visual está em `(34,17)` local, pixel `(1301,566)` da referência. Há três pequenos fragmentos separados de espuma. O alpha é binário e não foi criado fade artificial; a compatibilidade do azul e do alinhamento com o atlas de água deverá ser verificada pelo owner na cena.

Todas as fontes foram preservadas em camadas ocultas; candidatos Aseprite reabertos (RGB, um frame, duas camadas). Crops ampliados, previews claro/escuro e PNGs finais foram abertos e examinados. Alpha dos três PNGs finais medido por leitor independente; SHA256 da referência continua inalterado. Sem alteração de código, Unity, `.meta`, colliders, tilemap ou arte antiga. O agent de composição recebeu os apoios finais da vegetação e o root recebeu o apoio/crista da cascata.
