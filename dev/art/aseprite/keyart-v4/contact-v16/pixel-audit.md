# Auditoria de densidade de pixels — contato v16

Auditoria somente leitura de fontes, importação, wiring e evidência stage15. Nenhum Asset alterado; Unity não executado por esta auditoria.

## Medidas

`gameplay_stage15/capture-metadata.json`: captura real 640×480, câmera ortográfica 8,5; densidade de tela = **28,235294 pixels/unidade**. Razão abaixo = pixels de tela por pixel da fonte, não tamanho perceptivo dos clusters.

| Elemento | Canvas da fonte | PPU efetivo do sprite | Escala mundial | Pixels fonte/unidade | Tela/fonte no gameplay |
|---|---:|---:|---:|---:|---:|
| Player idle | 41×76 | 128 | 2 | 64,000 | 0,441 |
| Grama | 1254×1254 | 128 | 1 | 128,000 | 0,221 |
| Estrada | atlas 64×64, fatias 4×4 | 32 nas fatias | 1 | 32,000 | 0,882 |
| Casa exterior | 212×205 | 128 | 6,9785304 | 18,342 | 1,539 |
| Celeiro | 125×166 | 128 | 7,111111 | 18,000 | 1,569 |
| Fonte | 170×159/frame | 128 | 6,540146 | 19,571 | 1,443 |
| Cais | 93×137 | 128 | 7,4105268 | 17,273 | 1,635 |
| Cascata | 68×56/frame | 128 | 6,994536 | 18,300 | 1,543 |

O Player usa a escala 2 registrada na captura runtime. A escala 1,3 salva no editor precede o ScaleApplicator e não representa este gameplay. Escalas dos demais props vêm dos transforms e ancestrais do FarmScene.unity vigente; a metadata não enumera todos esses renderers. Cais: filho 1,2350878 × ancestral 6.

A grama ocupa **9,796875 unidades por célula**, calculadas em CreateFarmGroundTexture → WorldTilemapGround.SpriteWorldSize: 1254/128. Tile tem transform identidade. Não são 1254 pixels comprimidos em uma unidade. A estrada usa células de 0,125 unidade com 4 pixels de fonte; FarmPathPalette cria as fatias a 32 PPU, independentemente do PPU 128 atualmente registrado no PNG pai. A medida da estrada se apoia nesse contrato de geração.

Não comparar diretamente o número de pixels das capturas regionais com gameplay: composição 1536×1024/ortho28 = 18,286 px/u; animais 1600×1200/ortho12 = 50 px/u; água 1600×1200/ortho14 = 42,857 px/u. As imagens regionais ampliam a inspeção; não provam essa resolução na câmera do jogador.

## Diagnóstico e três correções de maior retorno

1. **Grama: candidato curado 314×314 com PPU 32,05103668.** Preserva exatamente a célula de 9,796875 unidades e produz cerca de 0,881 pixel de tela por pixel de fonte. A fonte atual exige cerca de 4,53 pixels de origem por pixel de tela em cada eixo. Reduzir apenas microcontraste pode aliviar ruído, mas mantém essa minificação. O candidato deve reconstruir clusters após a redução e preservar continuidade de bordas, alpha opaco, tint e geometria. Aceitar somente se o gameplay 1× melhorar; o tamanho nominal não mede o tamanho dos tufos, que já abrangem vários pixels. Não prescrever uma redução global dos demais sprites.
2. **Estrada: manter resolução e geometria; limpar microcontraste.** Sua amostragem já está próxima de um pixel de tela por pixel de fonte. Reduzir pontos isolados muito claros/escuros e reunir textura em poucos agrupamentos irregulares tem menor custo que mudar atlas, células ou PPU. Preservar o alpha de FinishEdges e o centro opaco. A captura real homestead mostra ruído espalhado também pelo centro da estrada.
3. **Props principais: limpeza localizada da arte, começando por uma amostra.** Casa, celeiro, fonte, cais e cascata estão próximos entre si, cerca de 17–20 pixels de origem/unidade. Não há evidência para um resize global. O PNG nativo do celeiro já contém gradações finas e contornos suavizados; Point não transforma esses valores em clusters deliberados. Testar limpeza localizada de material/contorno na fonte original, preservando canvas, alpha, pivot, escala e correspondência entre frames. Avaliar no tamanho real antes de estender. Não redimensionar o Player para igualar números de PPU: sua proporção corporal é uma questão separada.

## Importação e limites

As fontes verificadas usam Point, sem mipmaps e sem compressão efetiva do perfil padrão. Overrides de plataforma com `overridden: 0` não ativam suas opções. A grama 1254² cabe no maxTextureSize 2048; não há evidência de redução pelo limite do importador. Assim, o problema medido da grama é principalmente amostragem na câmera, além da organização artística da textura.

Suavidade visível em um PNG já está codificada na fonte. Esta auditoria não identifica retrospectivamente qual filtro ou processo produziu cada gradiente; não atribui bilinear ao importador nem certifica um resize upstream sem evidência. Escalas fracionárias dos props também implicam repetição desigual de pixels com Point. As fontes e a câmera mostram incompatibilidade de densidade nominal; não demonstram que toda diferença seja indesejada artisticamente.

Evidência direta: `docs/validation/farm_keyart_v4/gameplay_stage15/homestead.png`, metadata correspondente; capturas regionais stage15 de animais e água; PNG nativo barn_keyart_v4; metas das fontes; FarmScene.unity; CreateMvpFarmScene/CreateFarmGroundTexture; WorldTilemapGround; FarmPathPalette; FarmSceneCapture; ambient-validation.json stage15. Revisão estática não certifica movimento contínuo, ausência de cintilação em caminhada ou fidelidade percentual. Esses pontos exigem comparação real após integrar o candidato aprovado.
