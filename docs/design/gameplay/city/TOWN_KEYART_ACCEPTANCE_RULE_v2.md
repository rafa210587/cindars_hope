# Town — aceite perceptual v2

Status: critérios operacionais da reavaliação solicitada pelo humano; NÃO representam aprovação humana do resultado. Substitui v1 e retira o aceite visual histórico de 82/100.

## Evidência e comparação

Referência: `docs/art_catalog/_reference_keyart_GPT/town_keyart_layout_chatgpt_web_candidate_v2.png`, 1536×1024. Baseline original: `docs/validation/playmode/town_capture_full.png`, SHA256 `09BF8ECB6577F244C53751357FF60A3AEA5AB16E58AE9A91A33EABE99A0AB16F`.

Preservar baseline e cada revisão em `art/town-keyart-rework/evidence/`. Capturas ortho60 e ortho45/
120×90 são diagnósticos históricos, sem valor para o score final. A comparação oficial usa centro (0,0),
**ortho58**, 1536×1024, enquadrando o mundo **160×112** pela regra `contain` de
`CITY_KEYART_PROPORTION_RULE_v1.md`. Manter um BEFORE comparável quando existir; zoom não ganha pontos.
Medidas relativas entre elementos devem melhorar. Nenhuma imagem de fundo, recorte de bairro ou chão
colado da keyart pode simular cena montada.

O percentual é estimativa perceptual auditável, não equivalência matemática de pixels. Registrar score por categoria, faixa de incerteza e defeitos localizados. Execução bem-sucedida de ferramentas, número de árvores, IDs e testes valem ZERO pontos visuais; são gates separados.

## Scorecard (100)

| Categoria | Peso | O que comparar, não apenas presença |
|---|---:|---|
| Composição e hierarquia | 25 | Relações entre escala dos landmarks, praça, núcleos e espaços negativos; templo NW/prefeitura NE; oeste comercial, leste ofícios, residências sul. |
| Arquitetura e silhuetas | 20 | Templo/prefeitura monumentais, estátua sobre fonte, casas 3/4 com gables/volumes, tavern/forja/alquimia/moinho reconhecíveis; sem kits frontais dominantes. |
| Praça, caminhos e muralha | 15 | Anel largo com jardineiras e aberturas, circulação bege interconectada, praças entre volumes, muralha de pedra/portões visíveis; sem grade dominante. |
| Água, relevo e vegetação | 15 | Rio esquerdo conectado a lago SW amplo, cascatas/pontes/moinho/cais; borda rochosa irregular, vegetação entre bairros em vez de moldura retangular. |
| Paleta e leitura de materiais | 15 | Pedra quente clara, madeira âmbar, azuis profundos e verdes diversos, hierarquia de contraste, sombras/suporte coerentes, pixels legíveis na escala de jogo. |
| Microcomposição e vida | 10 | Bancas de tecido listrado, mercadorias, muros baixos/canteiros/cercas coerentes, currais/animais; personagens legíveis e proporcionais. |

Âncoras da escala de cada categoria: 0% ausente/contraditório; 25% localização nominal sem aparência; 50% identidade reconhecível com diferenças grandes; 75% relações e silhuetas próximas com diferenças moderadas; 100% muito próxima sem discrepância relevante observada. Não arredondar para atingir a meta.

## Relações medidas na referência (aproximadas)

Coordenadas em pixels de referência, não promessas de precisão de 1px. Usar ocupação visível, não tamanho do canvas transparente.

- Templo: caixa aproximadamente (350,18)–(610,300); prefeitura (944,20)–(1198,315). Centros separados ~590px; largura de cada landmark ~0,43–0,45 dessa separação. Faixa de aceite 0,35–0,53.
- Praça: centro ~ (780,452), conjunto anel ~280×205px. A fonte/estátua estende-se ~ (726,286)–(831,473). A estátua é silhueta essencial, não opcional.
- Lago SW: água+borda domina região ~ (0,568)–(445,881), conectada ao rio da esquerda. Pequeno tanque isolado não equivale ao lago.
- Casas sul apresentam telhados de duas águas e jardins entre caminhos sinuosos; o curral tem galpões pequenos e cercas abertas, não um celeiro monumental competindo com o templo.
- Muralha: linha de pedra baixa contínua visível sobretudo ao sul, curva/irregular, abertura central em portão fortificado. Limite físico invisível não é equivalência visual.

## Bloqueadores visuais

Mesmo com soma >=80, reprovar se: estátua ausente; bancas predominantes parecem telhados/placas; lago continua isolado e sem moinho identificável; muralha/portão sul não legíveis; landmarks ocultos por copa/flor ou muito menores que a relação acima; predominância de vias ortogonais e fachadas repetidas; candidato contém alpha falso/fundo de recorte visível. Corrigir em outro passe, com ownership e déficits explícitos.

## Gates técnicos independentes

Preservar 24 House IDs, 28 NPCs canônicos (+ extra existente), 84 anchors, 23 bancas NPC, seis bancas mercado, dois spawns/portais e interiores acessíveis. Grafo de portas e lotes sem sobreposição é necessário mas não prova física real. Verificar suporte visual vs collider dos troncos, água/muralha/landmarks, passagem nas portas e anchors em área alcançável. Não eliminar collider para melhorar score. Importar Point/None/mipmaps off por Editor API. Não modificar Farm/Cave/runtime/save ou YAML manualmente.

## Decisão

Aceite requer que **cada um dos dois revisores** atribua >=80/100, cada categoria >=60% e composição/
arquitetura individualmente >=75%, com bloqueadores visuais resolvidos e gates técnicos comprovados.
Em divergência, usar o menor score; não escolher agregação após ver os resultados. Root decide após abrir
os artefatos. Pendência de PlayMode/sorting deve permanecer explícita; não declarar observação não
realizada. Manter histórico dos resultados reprovados e não sobrescrever capturas de evidência.
