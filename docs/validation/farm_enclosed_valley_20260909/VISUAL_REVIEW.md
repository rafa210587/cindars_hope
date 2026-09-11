# Revisão visual — fazenda em vale fechado v2

Data: 2026-09-09. Escopo: composição visual, baseada na inspeção da [captura final geral](diagnostic_final/farm_capture_keyart_composition.png) e da [referência aprovada](../../art_catalog/_reference_keyart_GPT/farm_enclosed_valley_approved_v2.png). Esta revisão não representa aceite humano e não demonstra colisão, navegação, interação, persistência ou animação.

## Resultado observado

A cena organiza o pomar entre a fonte e os campos centrais, o pasto no sudoeste, a casa e a estufa no nordeste, os quatro edifícios ao sul e o lago a sudeste. A clareira norte está aberta entre a caverna, o poço e a casa; a massa de árvores que encobria essa região na primeira captura foi redistribuída. O caminho diante dos edifícios do sul aparece desobstruído de copas na vista geral final. O contorno florestal e rochoso fecha visualmente o vale; sua física exige evidência separada.

## Alterações desta fatia

- `FarmSettlementVisualComposer.cs`: nove macieiras decorativas em três fileiras; cercas norte/sul do pomar e vão de acesso sul; pasto com duas vacas, duas ovelhas, cocho, cercas e entrada leste; pequenos grupos de apoio reposicionados junto aos edifícios do sul. Os animais e as macieiras são cenário estático: não receberam produção, coleta, simulação animal, novos IDs econômicos ou save. Os marcadores `Visual_OrchardGate` e `Visual_PastureGate` indicam os acessos para a validação integrada.
- `FarmDecorationPlanner.cs`: caminhos estreitos definidos por polígonos conectados; reservas de pomar/pasto contra decoração aleatória; redistribuição das 68 posições do planner de árvores coletáveis para bolsões do oeste e do sul, mantendo a quantidade e a ordem dos IDs. A clareira norte e a projeção das copas sobre as fachadas do sul ganharam exclusões explícitas. Grupos baixos de flores/folhas substituem o antigo patch de pradaria que passou a cair dentro do pasto. Um limite determinístico distribui os cortes de decoração excedente sem elevar os máximos de cada bioma.
- `FarmLandscapeVisualComposer.cs`: pés dos módulos do paredão norte acompanham os segmentos do contorno compartilhado; pedras de margem deixam de ser criadas nas bordas internas da união entre rio e lago; lírios são restringidos à água. A vista final não mostra a pequena ilha de pedras que aparecia junto à desembocadura na captura inicial.
- `WorldTilemapGround.cs`: reutilizado sem alterações nesta fatia. Reposicionamento dos marcos, composição do perímetro, colisões e geração da cena pertencem à integração e ao outro owner.

## Diferenças que permanecem da referência

1. O paredão norte lê como uma faixa mais uniforme e repetida; a referência tem massas rochosas mais irregulares, com recuos e variação mais forte de altura.
2. Os caminhos continuam mais retos, largos e geométricos na vista geral. A referência tem curvas suaves, bordas irregulares e transições de terra mais delicadas.
3. Fonte e poço estão menores e têm menor destaque visual que na imagem aprovada. A clareira ao norte também tem menos pequenos elementos de apoio.
4. Os campos estão vazios por escolha humana nesta entrega. Não foram adicionadas plantas decorativas imutáveis sobre células utilizáveis, nem colheitas gratuitas ou estado inicial de cultivo. A imagem aprovada contém lavouras maduras; essa diferença é explícita e não deve ser descrita como reprodução integral da imagem.
5. Vacas e ovelhas usam a arte existente, pequena e frontal, enquanto a referência mostra animais laterais. São representações decorativas estáticas. Os [candidatos gerados e sua revisão](art_candidates/REVIEW.md) foram rejeitados para importação: halo evidente nas vacas e blocos de pixels grandes demais na ovelha. Nenhuma dessas imagens foi incorporada aos assets do jogo.
6. O cercamento lateral do pasto é menos contínuo e menos convincente em perspectiva que na referência; o pomar usa fileiras mais regulares e cercas frontal/traseira. A composição geral está reconhecível, mas esses detalhes ainda diferem.

Capturas regionais disponíveis para inspeção: [montanha](diagnostic_final/farm_capture_region_mountain.png), [cultivo](diagnostic_final/farm_capture_region_cultivation.png), [animais](diagnostic_final/farm_capture_region_animals.png), [casa e estufa](diagnostic_final/farm_capture_region_homestead.png), [água e ponte](diagnostic_final/farm_capture_region_water_bridge.png). Sua existência não equivale a aceite de cada detalhe.

## Conferência humana curta em jogo

1. Na câmera normal, caminhar da caverna ao poço e à casa: observar escala dos marcos, repetição do paredão e eventuais copas encobrindo o personagem.
2. Visitar a fonte e entrar no pomar pelo vão sul: conferir leitura das macieiras, cercas e tamanho da fonte em relação ao personagem.
3. Entrar no pasto pelo leste e passar diante dos quatro edifícios: avaliar tamanho/orientação dos animais, continuidade das cercas e legibilidade das fachadas.
4. Contornar os dois campos e seguir ao cais/ponte: conferir largura e encontros dos caminhos, transições de solo e ausência de pedras decorativas isoladas na água.
5. Comparar a experiência em câmera normal com a referência, registrando quais diferenças visuais são aceitáveis e quais precisam de nova iteração. Verificar ações de jogo em roteiro funcional separado; esta revisão não as valida.
