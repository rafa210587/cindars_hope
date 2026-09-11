# Fazenda: produção modular de pixel art com GPT

Status: pesquisa e síntese do método concluídas em 2026-09-08; execução incremental autorizada; fatia inicial de geração/captura/chão/ShippingBin validada; arte regional e aprovação visual final pendentes.
Spec executável: [coesão e revisão in-game](../../.specs/a_implementar/spec_farm_pixelart_cohesion_and_ingame_review_v1.md).

## O que aproveitar de outros jogos e artistas

| Fonte primária | Método documentado | Aplicação em Cindar's Hope |
|---|---|---|
| [Witchbrook — Express Yourself](https://www.witchbrook.com/dev-blog-express-yourself/) | Caminhada com seis frames; cinco direções desenhadas e espelhamento para cobrir oito; máscaras reutilizadas na personalização de roupas. | Definir poses e direções necessárias, reutilizar corpo/roupa quando viável. Seis frames já podem comunicar caminhada: doze não são um requisito universal. |
| [Slynyrd — Top Down Character Sprites](https://www.slynyrd.com/blog/2019/10/21/pixelblog-22-top-down-character-sprites) | Escala definida em relação aos tiles e um dummy antes dos detalhes. | Fixar personagem, tile e câmera juntos antes de escolher resolução nativa. |
| [Slynyrd — Top Down Tiles Part 2](https://www.slynyrd.com/blog/2023/3/26/pixelblog-43-top-down-tiles-part-2) | Base repetível nos quatro lados, transições com bordas/cantos e overlays; verificação das conexões. | Montar kits de chão/água/rocha e testar suas junções, preservando o Tilemap existente. |
| [Slynyrd — Top Down Character Attack Animation](https://www.slynyrd.com/blog/2025/5/23/pixelblog-56-top-down-character-attack-animation) | Ataques de seis frames com fases e durações próprias. | Melhorar antecipação, contato e recuperação antes de aumentar contagem; não impor FPS uniforme às armas. |
| [Unity — Skul: The Hero Slayer](https://create.unity.com/skul-the-hero-slayer-case-study) | Uso de Tilemap, Atlas, Pixel Perfect e importer no processo de produção. | Reutilizar as ferramentas Unity existentes. É um jogo de plataforma; não é evidência de um layout de farming. |
| [Aseprite — CLI](https://www.aseprite.org/docs/cli/) | Exportação de spritesheets e JSON, com suporte a tags. | Exportação determinística opcional, se já instalado; nenhuma compra ou dependência obrigatória. |

Essas referências não demonstram que uma imagem gerada por GPT chega pronta para um jogo. A adaptação abaixo é uma decisão nossa de produção, não uma prática atribuída aos autores.

## Contrato visual antes do lote

1. Referência principal: `docs/art_catalog/_reference_keyart_GPT/farm_keyart_layout_aprovado_v1.png`. Ler a imagem junto das capturas reais; extrair paleta, perspectiva, luz, proporção e densidade, não copiar seu mapa como textura única.
2. Escolher o grid nativo após medir personagem e tiles na câmera de gameplay. Os 48/72/96/144 px do laboratório são tamanhos de exibição; não demonstram resolução nativa nem quantidade de detalhe novo.
3. Registrar modelo/referência, paleta observada, perspectiva, direção da luz, sombra, outline, tamanho de tile, PPU, dimensões do corpo e ponto de apoio. Campos ainda não medidos ficam `UNMEASURED`, jamais valores inventados.
4. Reutilizar o corpo de referência em idle/caminhada/armas. Espelhar somente quando roupa, mão dominante, arma e iluminação tolerarem a inversão.

## Pipeline adaptado ao GPT

`captura real → contrato → família pequena → PNG bruto + prompt → inspeção → laboratório modular → seção real → expansão`

- Pedir uma família por vez: grama, margens, árvores, casa ou poses. Anexar a referência aprovada e conservar a identidade do modelo ao iterar. O prompt especifica grid, escala, perspectiva, paleta, posições e a finalidade de cada peça.
- Preservar PNG bruto e prompt exatos em `art/farm-pixelart-review/raw/` e `prompts/`. Registrar hash, dimensões, referência, decisão e limitações em `manifest.json`. Esse diretório local pode estar ignorado pelo Git; não representa entrega clonável até uma promoção explícita e inspecionada.
- GPT pode errar alpha, número de células, anatomia, transições e consistência. Contar e inspecionar os resultados efetivos. Fundo quadriculado desenhado não é transparência.
- Corrigir arte raster pela ferramenta de imagem; usar scripts para métricas, validação e exportação autorizada, sem confundir processamento com aprovação artística. Não fazer redimensionamentos sucessivos nem interpolação suave de pixel art.
- Para animação: verificar ordem, silhueta, pé de apoio, identidade, trajetória da arma e duração por pose. Não normalizar o tamanho de cada frame pelo bounding box da arma: a lâmina alteraria o corpo e produziria pulsação.
- Para tiles: repetir uma base em mosaico, testar cada borda e canto aplicável, vizinhos alternados e transições de água/chão. Peças isoladas bonitas não provam continuidade.
- Para props: conferir ponto de apoio, sorting e relação entre massa visual, passagem e collider. Nenhum detalhe visual pode ocupar silenciosamente uma área arável ou acesso funcional.
- Importação futura: Point, Compression None, mipmaps false, PPU/pivot documentados. Não substituir imagens em `Assets/` antes de inspecionar o candidato e definir o contrato de importação.

## Fazenda inteira, em ordem de dependência

| Região | Kit e revisão | Prova na cena |
|---|---|---|
| Chão e cultivo | Base pouco ruidosa, variações controladas, caminhos e bordas do solo | Player legível; sem grade dominante; áreas aráveis e acessos preservados |
| Água e ponte | Água repetível, margens/cantos, ponte proporcional | Sem emendas; passagem livre e água sólida conforme contrato |
| Montanha | Base rochosa, bordas e sobreposições | Massa norte reconhecível; entrada da cave acessível |
| Homestead | Casa, estufa, craft e pequenos props coerentes | Casa não domina o mapa; entradas/prompts livres; sorting correto |
| Bosque | Árvores por família, arbustos e chão mais contido | Vegetação fora do cultivo/corredores; oclusão previsível |
| Animais e sul | Cercas, instalações e decoração do conjunto | Acesso aos animais/interações; mesma escala e perspectiva |

Primeira seção real: trecho de spawn/homestead, ponte e borda de cultivo. Aprovar o conjunto câmera + personagem + chão + uma família de props antes de multiplicar variantes pela fazenda. O gerador existente, seus contratos espaciais e Tilemap permanecem donos do layout.

## Decisões para animação e resolução

Doze frames de caminhada são um candidato em avaliação; corrigir duas poses que ainda sugerem parada e a mudança de rosto antes de adotá-los. Oito e doze devem ser comparados no mesmo ciclo, inclusive em deslocamento real, para não confundir velocidade com qualidade. Idle e armas recebem primeiro correção de poses e timing. Aumentar resolução é justificável quando o detalhe adicional permanece legível na câmera real; ampliar uma imagem no preview não fornece essa evidência.

## Gates proporcionais

- Geração de PNG ou edição de documentação: inspeção visual e métricas de arquivo; nenhuma suíte Unity global.
- Captura Editor/Play Mode: compile efetivo da ferramenta, arquivos/metadata, ausência de alteração de cena/saves. Captura automática não prova navegação física ou aceitação humana.
- Mudança de layout/contrato: testes existentes diretamente afetados e validators da região, após geração via Editor API com backup. Não ajustar testes apenas para esconder divergência.
- Integração: percurso real spawn → ponte → cultivo → casa → animais, leitura em movimento, sorting, colisão e interações. Registrar PASS/FAIL/NOT RUN individualmente.

## Estado de evidência

Há capturas diagnósticas da cena salva e laboratório externo de animações. A revisão inicial identificou chão quadriculado/brilhante, árvores no cultivo e casa visualmente dominante. Essas observações são de câmera diagnóstica e não confirmam a apresentação na câmera de gameplay. Continuação: três capturas reais foram obtidas e a correção focal foi verificada; detalhes no relatório individual. Nova arte integrada e aceitação humana continuam pendentes.

### Notas do baseline técnico atual

A grama observada possui64×64px,128PPU e célula0.5unidade; o comentário48px do contrato de escala não substitui a medição do asset. Câmera ortho8.5/snap128 deve ser confirmada na captura real. Não instalar Pixel Perfect ou2D Extras/Rule Tiles como solução automática: esses pacotes não foram encontrados no manifest auditado, e o projeto já possui ferramentas de Tilemap/transição. Aseprite não foi localizado no PATH auditado; seu uso continua opcional. Uma correção inicial de pesos de grama somente na Farm pode reduzir a alternância escura, mas não substitui a autoria e a prova de conexão do kit final.


### Achado na primeira captura de gameplay

No enquadramento640×480 com ortho8.5, o player observado (`idle_right_04`,41×76px,128PPU, escala2) mede1.1875unidade de altura, aproximadamente33.53px projetados. O ShippingBin cresce em PlayMode e oculta o player no spawn(24,3). A causa identificada é a reaplicação do perfil de escala no root, substituindo o ajuste visual feito no Editor. A prioridade imediata é restabelecer a altura visual2unidades e a visibilidade do personagem, preservando root/colisão/interação; aumentar resolução não resolve essa oclusão. A correção foi executada e a captura final comprovou2u para o ShippingBin e player visível no spawn. Evidência no relatório individual abaixo.

### Resultado da primeira fatia e decisão artística

[Relatório individual](../validation/spec_farm_pixelart_cohesion_and_ingame_review_v1_execution_report.md): três capturas reais passaram, com cena e pasta de saves preservadas durante captura. A mistura de chão materializada removeu A/B e o ShippingBin manteve altura2u em PlayMode. Essas correções não encerram as seis regiões da fazenda.

Um candidato GPT de grama foi gerado e rastreado:1254×1254 RGB,18615cores, FAIL_NATIVE_GRID e REVIEWED_REJECTED. Na comparação em14.12 e64px por célula trouxe mais ruído/periodicidade sem ganho de leitura do personagem. Não foi importado. Kit de bordas/cantos e exportação nativa permanecem por fazer. O próximo passo artístico é um kit pequeno com grid/paleta/transições coerentes e inspeção antes de expandir; para personagem, silhueta/poses antes de contagem maior. Captura estática não aprova a caminhada12frames, navegação ou aceitação humana.
