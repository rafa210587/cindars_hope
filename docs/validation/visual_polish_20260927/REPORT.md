# Primeira rodada de polimento visual — Fazenda, Cidade e Cave

**Estado:** entrega técnica parcial. Capturas e testes sem controle da tela; Unity batch oculto em prioridade reduzida para não interferir no teste do outro jogo. Estudos gerados pelo ChatGPT foram usados como direção de paleta, não como atlas final. Os assets finais foram polidos no Aseprite 1.3.18.6 em 64×64 ou 128×128, com original oculto em camada editável. [Prompts completos](PROMPTS.md).

## Resultado por área

| Área | Mudança aplicada | Evidência e julgamento |
|---|---|---|
| Cidade | Nova variante `town_grass_harmony_v1.png`, derivada da grama escura existente. A média RGB passou de `(61,8; 76,5; 5,5)` a `(79,8; 94,5; 3,6)`, próxima da grama base `(84,0; 99,9; 3,1)`. O gerador usa a variante apenas na base e nos bolsões da Cidade; pesos antigos e geometria dos caminhos foram preservados. Novo tile sem collider foi criado via Editor API, e `TownScene.unity` regenerada pelo comando canônico. | [Praça antes](evidence/town-before-base-grass.png) / [depois](evidence/town-after-base-grass.png): num recorte de grama `x=1060..1279, y=450..629`, pixels verdes muito escuros caíram de 12,1% para 3,6%; medição limitada ao recorte. [Vista geral atual](evidence/town-overview-after.png) mostra praça circular e bairros, mas ruas ainda serrilhadas e alguns vazios/repetições. **Melhora parcial.** |
| Cave | Três pisos mantiveram desenho, dimensões, GUIDs e pivôs; apenas a paleta foi aproximada no Aseprite. Sombra da célula junto à parede suavizada de `(0,62; 0,60; 0,58)` para `(0,78; 0,76; 0,74)`. | [Antes](evidence/cave-before-palette.png) / [depois](evidence/cave-after-palette.png) são seeds/layouts distintos; não servem como comparação pixel a pixel. O novo frame não tem lacunas de render, mas células retangulares ainda são visíveis. **Polimento de piso ainda parcial.** |
| Fazenda | Atlas nativo de caminho `ground_path_contact_v16.png` teve apenas paleta ajustada (`-12R, -5G, +12B`), preservando as 256 células 4×4, UVs, GUID e física. | [Antes](evidence/farm-before-path.png) / [depois](evidence/farm-after-path.png) usam o mesmo enquadramento. O trecho de caminho `x=200..999, y=280..389` mudou de RGB médio `(128,7; 104,3; 23,4)` para `(121,8; 101,4; 30,4)`. Caminho continua largo/geométrico; este ajuste reduz apenas o amarelo dominante. **Melhora parcial.** |

Os estudos e fontes editáveis estão em [candidates](candidates/): três PNGs de conceito ChatGPT, uma `.aseprite` da Cidade, três da Cave e uma da Fazenda, com previews e scripts Lua. A arte gerada não foi colada como fundo, não criou colisores e não substituiu sprites de personagens.

## Validação

- Unity gráfico oculto: captura PlayMode **1/1 PASS** após as mudanças em cada área; logs/XML em `TestResults/{town,cave,farm}-visual-polish-20260927/`. A captura final da Fazenda também compila as mudanças anteriores de Cidade/Cave.
- Cave: `CaveBiomeArtProfilesTests;CaveLightShaftPresentationTests` **31/31 PASS**; `CaveStableHashUsageTests;CaveRuntimeMaterializerDecompositionTests` **20/20 PASS**. Geração, `CaveRunSeed`, snapshots, saídas, inimigos, resource nodes e save não foram editados; revisita em jogo **NOT RUN**.
- Cidade: `ValidateFableCitySchedule` **28/28 PASS**; `TownKeyartPhysics` **195/195 PASS** na cena nova. `ValidateTownKeyartScene` global **FAIL 6/8** por acesso interno em `House_GateKeeper` e conectividade de trabalho **14/28**. A cena anterior, restaurada temporariamente e validada com o mesmo código, reproduziu exatamente os mesmos dois diagnósticos; a cena nova foi recolocada com hash conferido. `TownLayoutTests;TownKeyartGeometryTests` **46/47**, falha em `CivicHierarchy_AnnexesRemainSubordinateWhileLandmarkAndPlazaReserveStayIntact` (esperado 9,6; obtido 8,0), fora dos arquivos visuais alterados; baseline desse teste não reexecutado.
- PNGs de jogo: Point, mipmaps off, compressão efetiva Default None; tiles novos da Cidade com `colliderType=None`. Fontes originais preservadas nas camadas ocultas das `.aseprite` e nos respectivos arquivos prévios do Git. Cenas Fazenda/Cave não foram regeneradas nem salvas nesta rodada.

## Próxima correção visual

1. **Cidade:** suavizar as bordas do caminho em raster de 0,5u e preencher bolsões vazios sem mudar a circulação, usando a spec `spec_town_keyart_fidelity_v1`.
2. **Cave:** substituir a escolha de três texturas completas por base contínua e detalhes irregulares/determinísticos, ou gerar variantes com bordas compatíveis; testar pelo menos duas seeds e contraste dos inimigos. A paleta atual sozinha não resolve os quadrados. Preservar stable run.
3. **Fazenda:** rever polígonos/largura das rotas e a junção caminho-grama pela spec `spec_farm_pixelart_cohesion_and_ingame_review_v1`. A cor já é menos dominante; a geometria segue pendente.

Aceite humano, animação/oclusão em movimento, combate percebido e passeio por todo o mapa permanecem **NOT RUN**. Nenhuma interação com o jogo ou com a tela ocorreu nesta rodada.
