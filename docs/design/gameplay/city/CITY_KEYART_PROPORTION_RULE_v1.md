# Cindar's Hope — Regra de proporção da key art para a TownScene v1

> **Status:** direção operacional, revisada na reabertura da fidelidade; avaliação visual pela regra de aceite v2  
> **Data:** 2026-09-11 — reconciliada após expansão aprovada  
> **Referência visual candidata:** `docs/art_catalog/_reference_keyart_GPT/town_keyart_layout_chatgpt_web_candidate_v2.png`  
> **Footprint materializado:** `TownDistrictLayout.WidthTiles = 160`, `HeightTiles = 112`  
> **Módulo nominal do guia:** tile32×32px e personagem32×48px; medir ocupação/importação/perfil reais separadamente

## 1. Regra central

A key art é um **mapa de composição**, não um background para esticar na cena.

Toda reconstrução deve preservar três contratos separadamente:

1. **posição relativa:** distritos e landmarks seguem a composição da key art;
2. **escala jogável:** tamanhos e distâncias são definidos em tiles, não pelos pixels aparentes da imagem;
3. **footprint físico:** colisores, portas e corredores continuam legíveis e navegáveis para o player 32×48 px.

Nunca redimensionar a PNG inteira para 160×112 células e usá-la como chão. Chão grande usa Tilemap; prédios e props grandes usam sprites/prefabs com pivô na base e footprint próprio.

## 2. Encaixe matemático da imagem

A imagem candidata mede 1536×1024 px (3:2), enquanto a área lógica da TownScene mede 160×112 tiles
(10:7). Para não deformar a arte, a sobreposição de planejamento usa `contain`:

```text
pixelsPorTile = min(1536 / 160, 1024 / 112)
pixelsPorTile = 9,142857...

larguraJogavelNaImagem = 160 × pixelsPorTile = 1462,857 px
alturaJogavelNaImagem  = 112 × pixelsPorTile = 1024 px
margemHorizontal       = (1536 - 1462,857) / 2 = 36,571 px por lado
```

Portanto, o frame lógico 160×112 ocupa `x = 36,571..1499,429` e `y = 0..1024` da key art. As faixas
laterais representam contexto visual externo e não ganham área jogável.

Conversão de um ponto da imagem para coordenada lógica, com origem Unity no centro da cidade:

```text
worldX = (pixelX - 768) / 9,142857
worldY = (512 - pixelY) / 9,142857
```

Conversão inversa:

```text
pixelX = 768 + worldX × 9,142857
pixelY = 512 - worldY × 9,142857
```

Os pontos convertidos devem ser ajustados para o tile inteiro ou meio tile mais próximo, e depois validados contra lotes, vias e áreas de aproximação existentes. A key art nunca vence um contrato estável de spawn, porta, schedule ou save.

## 3. Módulo de proporção jogável

`1 unidade Unity = 1 tile lógico`;32px é módulo nominal de autoria, não prova da ocupação ou PPU de cada sprite existente.

| Elemento | Medida obrigatória na cena |
|---|---:|
| Player/NPC comum | baseline≈1,09u após perfil; piloto Town-only≈2,46u mediante perfis próprios e física mundial preservada; medir após Awake |
| Rua principal / anel | 3 tiles livres mínimos |
| Rua secundária | 2 tiles livres mínimos |
| Aproximação diante de porta | 2 tiles de profundidade, sem prop bloqueando |
| Porta comum | 2×2 tiles |
| Casa comum | lote-base 8×7 tiles |
| Loja/ofício | 8–14 tiles de largura; 7–11 de profundidade |
| Landmark cívico/religioso | footprint-base 14–16×10–11 tiles, ajustável com validação; ocupação VISUAL não limitada a esse footprint |
| Praça central | 26 tiles de diâmetro lógico |
| Árvore urbana média | 2×3 tiles visuais; collider somente no tronco/base |
| Árvore urbana grande | 3×4 tiles visuais; collider somente no tronco/base |
| Separação entre footprints sólidos | 3 tiles desejados; nunca menos de 2 |

Telhados, copas, fumaça, bandeiras e marquises podem ultrapassar visualmente o lote. A base física e a passagem livre não podem ultrapassá-lo.

Auditoria de escala da reabertura: Player da captura EditMode tinha0,711u e o perfil em Awake leva a≈1,094u; NPCs comuns medidos≈1,094u e walk≈1,145u. Ajustar props da Town em relação a esses atores; não alterar Animator/escala raiz/perfis compartilhados para satisfazer a estimativa antiga. Flores decorativas não devem parecer mais altas que pessoas; avaliar bancos/cercas/portas pela ocupação visível, não pelo padding do PNG. A diferença EditMode/PlayMode exige evidência de ambos.

**Revisão03/04 — correção explícita dessa hipótese:** preservar o ator em1,09u mostrou-se incompatível com a proporção da referência. Na mesma captura, NPC comum ocupa12–13px contra30–32px na keyart; estátua ocupa~172px contra~196px. A razão estátua/ator ficou13,8 em vez de~6,3. Auditoria `art/evidence/proportion-audit03.md` fundamenta piloto de fator2,25 somente na Town (perfil comum/Player2→4,5, altura opaca~2,46u), mantendo diferenças raciais. Não é autorizado alterar perfis compartilhados ou runtime. O root pode mudar SOMENTE nesse piloto explícito, com contraescala das formas/offsets/filhos físicos para preservar dimensões mundiais e confirmação pós-Awake. Perfis locais fora de `Data/Scale`, referências explícitas, ColliderScale0; não compensar shape e transform duas vezes. Bancos/assentos/postes precisam ser reavaliados em conjunto; flores e edifícios não aumentam automaticamente. A orientação anterior de conservar1,09u não é um gate artístico vigente.

Portas comuns da própria referência têm altura desenhada próxima de uma pessoa (~0,8–1,0), enquanto a porta do templo tem~1,7–1,9. Não impor universalmente1,5–2 alturas de ator às portas comuns, pois isso seria uma nova direção estética sem base na keyart. Exigir abertura e travessia visual plausíveis nos frames reais, passagem física existente preservada e ausência de clipping grosseiro; aplicar ajuste localizado quando a evidência mostrar necessidade. O piloto permanece não aceito até a comparação real.

Na reavaliação, usar 14–16 tiles também como largura visual deixou os landmarks pequenos demais. Medir largura ocupada do templo/prefeitura dividida pela distância entre seus centros: alvo aproximado 0,43, faixa 0,35–0,53. Aplicar escala uniforme ao sprite, mantendo suporte no chão e acesso à porta; não esticar eixos. O tamanho final emerge da posição e da ocupação real, não de um número isolado de tiles. A keyart sugere volumes de cerca de 22–24 tiles visuais, dependentes da composição final.

Captura de composição: 1536×1024, centro (0,0), ortho58; contém o mundo160×112 com margem vertical
de2u e sem deformação. Ortho45 é baseline histórica de120×90 e não serve para score final da cidade
expandida. Câmera normal de jogo é separada e precisa de recortes próprios para avaliar NPC/porta/pixels.
Aproximar o enquadramento sozinho não representa ganho de fidelidade.

## 4. Hierarquia e densidade

### Densidade de pixels não é escala do Tilemap

Rasterizar curvas em células de0,5u não autoriza encaixar uma textura inteira de64px em cada meia célula: isso altera o tamanho aparente da pedra. Na revisão03, sub-sprites16×16 a32PPU preservam a repetição completa64px em2u. Esta é uma decisão de textura, separada da geometria. Import Point/None não remove suavização já presente no arquivo. Recortes de casas com~100px úteis ampliados para~8–10u precisam de inspeção na câmera real e de autoria nativa quando falta detalhe; quantização ou nearest sozinhos não comprovam qualidade.

Os atores existentes medidos~1,09u não garantem que as proporções do cenário sejam corretas. A revisão03 abriu auditoria de relações porta/ator, banco/ator e estátua/ator; não declarar esse gate atendido enquanto a discrepância visual persistir. Preservar perfis compartilhados e collider dos personagens durante a investigação.

O mapa deve parecer denso sem perder navegação. Para os 17.920 tiles lógicos da cena, usar como orçamento-alvo:

- **24%**: footprints sólidos de prédios, muralha e obstáculos;
- **26%**: ruas, praça, pontes e áreas de aproximação navegáveis;
- **15%**: lago, curral, jardins e set-pieces sem rota principal;
- **35%**: respiro, vegetação, borda e transições entre distritos.

Esses percentuais são orçamento de planejamento NÃO MEDIDO, não gates já atingidos. Só registrar cobertura como medida quando houver máscara/censo com classificação exclusiva explícita; nunca estimar ocupação pelo número de árvores. A comparação perceptual e os corredores livres prevalecem sobre atingir um orçamento arbitrário.

Leitura de escala:

- prefeitura e templo formam o primeiro nível de hierarquia;
- praça, taverna, mercado e ferraria formam o segundo;
- casas e oficinas secundárias formam o terceiro;
- props nunca competem em tamanho ou contraste com landmarks.

## 5. Âncoras da composição 160×112

Usar os sete distritos estáveis já existentes:

| Distrito | Função visual |
|---|---|
| `temple_fountain_north` | templo no noroeste e faixa cívica ao norte |
| `town_hall_northeast` | prefeitura/relógio como landmark nordeste |
| `central_plaza` | praça circular, fonte, estátua, board e espaço de festival |
| `market_west` | mercado, padaria e taverna |
| `lake_park_southwest` | lago, pescaria/moinho, cais, ponte e parque |
| `residential_east` | ferraria, alquimia, carpintaria e curtume |
| `corral_south_gate` | residências, jardins, curral, guarita e saída da fazenda |

A reconstrução pode melhorar curvas, vegetação e silhuetas, mas não deve trocar esses distritos de lado nem quebrar os IDs estáveis.

## 6. Gates antes de materializar a cena

- Sobrepor grid 160×112 ao frame 3:2 por `contain`, sem deformação.
- Converter os centros dos landmarks pela fórmula e arredondar para 0,5 tile.
- Confirmar todos os lotes dentro de `x = -80..80`, `y = -56..56`.
- Confirmar 3 tiles nas vias principais e 2 nas secundárias.
- Confirmar área de 2 tiles diante de cada porta.
- Confirmar player/NPC na escala real após Awake, registrar ocupação e relação com portas/props, sem prédios miniaturizados.
- Preservar nomes `House_*`, spawn IDs e anchors de NPC.
- Importar sprites com Point, Compression None e mipmaps desligados.
- Materializar por `CreateMvpTownScene`/Editor API; não editar YAML manualmente.
- Validar no Play Mode; a key art isolada não comprova navegação, sorting ou colisão.

## 7. Reconciliação documental necessária

`CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md` e a versão anterior desta regra registravam 128×96 e
120×90. A spec aprovada `spec_town_spatial_expansion_and_access_v1` substituiu esses footprints por
**160×112**, preservando os sete distritos, IDs e escala visual1,20. Valores históricos continuam úteis
somente para comparação; não podem reger uma nova regeneração.
