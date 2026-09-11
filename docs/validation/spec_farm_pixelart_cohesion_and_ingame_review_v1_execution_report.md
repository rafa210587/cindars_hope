# Execução — coesão pixel art da fazenda e revisão in-game

Spec: [spec_farm_pixelart_cohesion_and_ingame_review_v1](../../.specs/a_implementar/spec_farm_pixelart_cohesion_and_ingame_review_v1.md). Data:08–09/09/2026. **Entrega vigente: revisão I, SCOPED_PASS técnico**, `dock_path_pass/regen_01` e `gameplay_01`. A rodada de proporções G foi executada; o cais foi corrigido nesta I, junto ao acabamento das trilhas solicitado após H. O root inspecionou a entrega I; o aceite humano específico desta última versão permanece pendente. Spec: **IN_PROGRESS**. Promoção: **NO**.
**Histórico anterior a G/H — rejeição da primeira reconstrução:** o usuário considerou aquela entrega muito incompleta. Os vereditos técnicos de aproximação daquela rodada não atenderam ao objetivo visual e motivaram a revisão de montanha, bosque, lago e composição. Esse registro não é o status atual da revisão I.

**Histórico anterior a G/H — correção da ponte:** o frame de gameplay04 daquela rodada mostrava o jogador em(27,2.5) sobre água abaixo do desenho da ponte, apesar do PASS técnico. A correção visual foi comprovada em regeneração06/gameplay05 daquela rodada. G posteriormente substituiu a arte por uma ponte reta, preservada e inspecionada em H; a evidência vigente está identificada acima.

As seções1–5 preservam a primeira fatia histórica. Resultados e pendências ali descritos pertencem àquela versão e não validam nem descrevem automaticamente a cena atual. O fechamento vigente I está no fim deste relatório, com provas e limites específicos.
## Acceptance criteria extracted

Backup íntegro antes da geração; regeneração exclusiva da Farm; três capturas com câmera real; cena/saves preservados durante captura; mistura de chão sem variantes A/B; ShippingBin com altura visual2.0±0.02u e player visível no spawn; candidato GPT com PNG/prompt/hash e decisão explícita. Kit completo, integração artística regional, caminhada em movimento e aceitação humana são critérios posteriores ainda pendentes.

## Existing systems audit

Reutilizados `CreateMvpFarmScene`, `WorldTilemapGround`, perfis de escala e contratos existentes. A mistura85/10/5 foi limitada a `CreateFarmGroundTexture`; PaintGrass global permaneceu intacto. ShippingBin usa filho Visual para separar tamanho da arte do perfil do root, preservando interação/colisão. `FarmSceneCapture` e a sessão Editor assíncrona capturam a câmera real. Nenhum novo sistema de gameplay, pacote ou regra/skill foi necessário. [Workflow pesquisado](../art_catalog/FARM_GPT_PIXEL_ART_WORKFLOW.md) orienta geração por família e avaliação antes de expansão.

## Spec Compliance Matrix

| Requirement | Implementação/evidência | Estado |
|---|---|---|
| Backup e integridade |184 arquivos protegidos,140 PNGs World preservados | OK |
| Geração focal |Uma regeneração Farm via Unity API, exit0, três vistas diagnósticas | OK |
| Captura real |Spawn/ponte/cultivo, Main Camera640×480, ortho8.5 | OK |
| Persistência |SaveInput1 desativado em memória; saves0→0; hash da cena preservado durante captura | OK |
| Mistura de chão |12749 base,1451 flower,776 pebble; total14976; A/Bzero | OK |
| ShippingBin |Altura2.0u nas três vistas; player visível no spawn inspecionado | OK |
| Medidas de escala |Tile64px/128PPU=.5u→14.12px; player1.1875u→33.53px projetados | OK |
| Proveniência do piloto |[Manifest](../../art/farm-pixelart-review/manifest.json), raw e prompt preservados | OK |
| Candidato GPT aprovado |Grass1254×1254 RGB,18615cores; FAIL_NATIVE_GRID e REVIEWED_REJECTED | FAIL |
| Kit de bordas/cantos e exportação nativa |Não produzido; candidato rejeitado não foi importado | DEFERRED |
|12poses integradas e avaliadas em movimento |Não integrado; imagens atuais são estáticas | DEFERRED |
| Navegação física e aceitação humana |NOT RUN | DEFERRED |

| Região | Achado atual e trabalho restante | Aceitação regional |
|---|---|---|
| Chão/cultivo |Quadriculado escuro melhorou; caminhos ruidosos e bordas duras persistem | PENDING |
| Água/ponte |Água brilhante/cinza e margens em degraus | PENDING |
| Montanha |Faixa repetitiva exige revisão das peças e transições | PENDING |
| Homestead |ShippingBin corrigido; escala/composição casa–estufa pendente | PENDING |
| Bosque |Árvores ainda invadem visualmente o cultivo | PENDING |
| Animais/sul |Prédios e árvores com oclusões a revisar | PENDING |

## Validation

[Relatório Unity da rodada](playmode/farm_pixelart_20260908/unity-round-report.md) contém comandos, versão6000.5.7f1, inputs, processos/exit codes, scans e histórico. [Metadata final](playmode/farm_gameplay_review/after_real/capture-metadata.json) e [resultado final](playmode/farm_gameplay_review/after_real/validation-result.json) comprovam três vistas, zero runtime errors, altura do bin e integridade. [Spawn final](playmode/farm_gameplay_review/after_real/spawn.png), [ponte](playmode/farm_gameplay_review/after_real/bridge.png) e [cultivo](playmode/farm_gameplay_review/after_real/cultivation.png) são capturas reais; não são prova de movimento ou travessia.

Compile Editor comprovado pela execução Unity vigente; nenhum compile-only ou suíte global repetido. Baseline real teve exit1 pelo hash amplo incluindo Analytics; primeiro after teve exit1 por comparação null após domain reload. Ambos permanecem FAIL históricos. A captura final corrigida passou; somente captura foi repetida, sem segunda regeneração. Cena mudou na geração autorizada e permaneceu idêntica durante a captura. Nenhum PNG World foi substituído.

O orquestrador inspecionou [preview comparativo](http://127.0.0.1:8767/preview.html) em14.12 e64px por célula, com player na escala medida e antes/depois; console sem erros. O candidato GPT trouxe ruído e periodicidade sem melhorar a leitura do personagem, além de não cumprir grid nativo64×64. Foi rejeitado e não importado. [Preview local](../../art/farm-pixelart-review/preview.html) e [PNG bruto](../../art/farm-pixelart-review/raw/grass-base-v1.png) preservam o experimento; redução no canvas não é exportação nativa.

```text
Unity validation: PASS — SCOPED, geração/capturas e critérios desta fatia
Gameplay movement/navigation validation: NOT RUN
Reason: a ferramenta captura poses estáticas com teleporte; não executa a rota física nem avalia ciclos em movimento.
Command attempted: captura automatizada documentada no relatório da rodada; roteiro humano não executado.
Residual risk: colisão, circulação, ações, novas animações e aceitação estética integral não validadas nesta fatia.
```

A renderização Camera.Render não inclui UI Screen Space Overlay.640×480 não cobre todas as resoluções-alvo. Falhas globais herdadas de CURRENT_STATE continuam fora do escopo e não foram reclassificadas.

## Honest status rationale

**IN_PROGRESS; promoção NO**, conforme finish-spec: há evidência real da correção focal de chão/ShippingBin e da ferramenta de captura, mas os critérios regionais/artísticos centrais ainda estão incompletos. Pesquisa concluída; piloto documentado e rejeitado é aprendizado verificável, não kit pronto. Não mover a spec nem declarar BUILD_VALIDATED global/aceitação humana.

Próximas fatias: corrigir silhueta/poses antes de aumentar frames; produzir um kit pequeno de chão com grid, paleta e transições inspecionados; provar uma seção real antes de expandir para as seis regiões. A fazenda inteira, navegação e aprovação humana permanecem pendentes. Nenhum status/log global foi alterado por este relatório.

## 6. Reconstrução integral pela keyart — continuação de09/09/2026

### Escopo e critérios desta entrega

A continuação substitui o objetivo restrito de baseline por reconstrução materializada das seis regiões da FarmScene. Reutiliza gerador, contratos espaciais, Tilemaps e sprites existentes; adiciona candidatos GPT de casa, píer e cachoeira, preservando PNG bruto e prompt. A grama rejeitada da primeira fatia continua rejeitada e não foi importada.

As âncoras agora colocam casa em(16,9), estufa em(23,8), ponte em(27,3), quatro prédios sul alinhados e bosque restrito ao oeste. A composição inclui cultivo em dois blocos, caminhos conectados, margem de lago, montanha e cachoeira. Posições decorativas não representam crops de gameplay fictícios. A extensão opcional de RoofReveal liga/desliga renderers interiores da Farm; consumidores legados de Town continuam sem essa lista.

Critérios verificáveis: geração Unity exclusiva; nove diagnósticos incluindo seis regiões; oito capturas de câmera real;16 aproximações conectadas usando collider real; import Point/None/noMip e PNG raw idêntico ao asset; cena/saves preservados durante captura; testes dos contratos e do novo comportamento RoofReveal; revisão visual explícita por região. Nenhum desses resultados, isoladamente, equivale a aceite artístico humano.

### Evidência final — regeneração06 e gameplay05

O [relatório Unity da reconstrução](playmode/farm_keyart_delivery_20260909/validation-report.md) é o registro proprietário de comandos, hashes de inputs, versões, exit codes e arquivos por execução. Baselines permanecem em `before_diagnostic` e `before_gameplay`. A regeneração01 foi tecnicamente executada, mas sua composição foi reprovada na inspeção; a02 melhorou a composição e motivou correções adicionais. Essas rodadas não são reclassificadas retroativamente.

| Critério | Estado vigente | Evidência/limite |
|---|---|---|
| Backup anterior | PASS |191 arquivos verificados e140 hashes PNG World no relatório proprietário |
| Reconstrução das seis regiões | PASS de implementação/revisão |Regeneração06; inspeção real05 confirmou ponte alinhada sob os pés do jogador |
| EditMode pertinente anterior | PASS329/329, anterior à última extensão |Não comprova os novos testes RoofReveal; resultado final aguardado |
| Captura gameplay01 | FAIL preservado |8imagens; gate físico não executou por guard de null após domain reload |
| Captura gameplay02 | FAIL preservado |Probe detectou lookup incorreto de Workbench; não houve prova de conectividade |
| Correção de domínio da ferramenta | PASS em gameplay03 |Flag serializada `physicalRoutesEvaluated`; probe executou6833nós,16/16PASS |
| Câmera real final8/8 | PASS em gameplay05 |Oito PNGs, zero runtimeErrors, exit0; cena e saves preservados |
| Conectividade física16/16 | PASS em gameplay05 |6833nós; BFS único de0.5u, Collider2D.Cast/OverlapCollider, máscara de colisão real e restauração de estado |
| Casa/píer/cachoeira — bytes e import | PASS |[Import audit](playmode/farm_keyart_delivery_20260909/import-validation.json): três raw=asset, Point/None/noMip,128PPU |
| Novos testes RoofReveal | PASS2/2 no filtro final6/6 |[XML focal](playmode/farm_keyart_delivery_20260909/focal-tests_02.xml); inclui4testes de composição afetados |
| Preview final rastreável | PASS de construção/proveniência |[Comparativo final05](../../art/farm-pixelart-review/delivery.html),11PNGs originais, reconstruído após inspeção da ponte05 |
| Aceite humano/ações/animação em movimento | NOT RUN |As capturas e sweeps não simulam input nem ciclo das novas animações |

O FAIL de gameplay01 veio de `JsonUtility` reidratar uma referência null como objeto Evidence default; o guard impediu o probe. A flag persistida corrige a execução e o gate exige resultado explícito. O erro anterior permanece documentado. Hashes de persistência cobrem todos os arquivos sob `persistentDataPath/saves`, o diretório real de SaveManager, separando `persistentRoot` e os arquivos de Analytics do Editor.

Gameplay02 executou o setup do collider, mas falhou ao resolver `Workbench`: os três crafts materializados usam prefixo `CraftingStation_`. Os três lookups foram corrigidos após conferir gerador e cena salva. Auditoria conjunta confirmou exatamente uma ocorrência para os oito nomes usados pelo probe; os outros destinos derivam dos contratos. O diagnóstico agora agrega todos os nomes ausentes/ambíguos antes de falhar, sem iniciar BFS com destinos inventados. O FAIL02 permanece histórico.

Gameplay03 passou com8/8PNGs,16/16aproximações,6833nós,zero runtimeErrors e cena/saves preservados. A última alteração visual de sorting motivou regeneração05 e gameplay04;03 permanece evidência de seu estado anterior. O primeiro teste RoofReveal falhou1/2 por `SendMessage` disparar assertion `ShouldRunBehaviour()` em EditMode. A invocação de teste passou a chamar o handler por reflexão; o filtro final passou6/6, incluindo os dois casos RoofReveal e os quatro contratos de composição. Nenhum log foi suprimido para converter a falha em PASS.

Evidência técnica04 conferida diretamente: [processo04](playmode/farm_keyart_delivery_20260909/gameplay_04/process.json) exit0, [metadata04](playmode/farm_keyart_delivery_20260909/gameplay_04/capture-metadata.json) PASS8/8 e16/16, zero runtimeErrors, saves0→0. O hash SHA-256 da FarmScene antes/depois da captura e do arquivo materializado nessa rodada coincidiu: `9f0e5b000c5dfe76dbbfbfa20fd82443efbde1d3081c8e2061795b3a423fd837`. O preview04 foi reconstruído após essa conferência e posteriormente marcado HOLD pela inspeção visual; embute PNGs sem alterá-los. Parsing Python e JavaScript do builder passou; inspeção da interface não é substituída por esse parsing.

As imagens04 imutáveis são [spawn](playmode/farm_keyart_delivery_20260909/gameplay_04/spawn.png), [ponte](playmode/farm_keyart_delivery_20260909/gameplay_04/bridge.png), [cultivo](playmode/farm_keyart_delivery_20260909/gameplay_04/cultivation.png), [casa](playmode/farm_keyart_delivery_20260909/gameplay_04/homestead.png), [bosque](playmode/farm_keyart_delivery_20260909/gameplay_04/forest.png), [montanha](playmode/farm_keyart_delivery_20260909/gameplay_04/mountain.png), [animais](playmode/farm_keyart_delivery_20260909/gameplay_04/animals.png) e [lago](playmode/farm_keyart_delivery_20260909/gameplay_04/lake.png). [Diagnósticos05](playmode/farm_keyart_delivery_20260909/regen_05/) têm enquadramento Editor e não devem ser confundidos com essas vistas de PlayMode.

**Evidência final vigente05:** [processo](playmode/farm_keyart_delivery_20260909/gameplay_05/process.json) exit0 e [metadata](playmode/farm_keyart_delivery_20260909/gameplay_05/capture-metadata.json) conferidos diretamente:8/8PNGs,16/16aproximações,6833nós,zero runtimeErrors, saves0→0 e hashes preservados. O SHA-256 da cena atual coincide com o antes/depois dessa captura: `c9796a69045c3c27cafa23128db14fed74b01701a6ec184b66c09256bce1df6e`. A [ponte final05](playmode/farm_keyart_delivery_20260909/gameplay_05/bridge.png) foi inspecionada pelo orquestrador e liberada: pés sobre madeira, player visível e desenho alinhado. O [comparativo final](../../art/farm-pixelart-review/delivery.html) foi reconstruído após essa liberação, removendo o aviso de HOLD04 e incorporando as [oito vistas05](playmode/farm_keyart_delivery_20260909/gameplay_05/) e a cena dos [diagnósticos06](playmode/farm_keyart_delivery_20260909/regen_06/), sem alterar os PNGs. Veredito final: **Unity SCOPED_PASS e revisão visual de implementação PASS com diferenças artísticas declaradas; aceite humano NOT RUN**.

### Revisão regional histórica06/05 — entrega posteriormente rejeitada pelo humano

| Região | Implementação a confrontar com a keyart | Veredito final |
|---|---|---|
| Chão/cultivo |Dois blocos funcionais vazios, caminhos e paleta local; sem plantar sprites fictícios para simular produção |PASS de implementação visual; não é aceite humano |
| Água/ponte |Frame real05 mostra pés sobre o deck de madeira, personagem visível e ponte alinhada; rio/píer/cachoeira aproximam a referência |PASS de implementação visual; FAIL04 histórico preservado |
| Montanha |Encaixe e escala corrigidos; textura e borda em degraus permanecem estilizadas |PASS com diferença artística explícita |
| Homestead |Casa/estufa e layout aproximam a referência; interior ocultado por RoofReveal e bin separado |PASS de implementação visual |
| Bosque |Árvores a oeste; inspeção05 confirmou folhagem corrigida, sem flutuar sobre copas |PASS de implementação visual |
| Animais/sul |Quatro prédios visíveis e alinhados, com acessos recompostos |PASS de implementação visual |

Os vereditos acima preservam a revisão histórica do orquestrador e não são o estado visual vigente: **a entrega foi rejeitada pelo humano e as seis regiões estão reabertas na revisão global**. A correção da ponte05 continua tecnicamente comprovada; não demonstra fidelidade suficiente do restante da fazenda. A geometria em tiles, escala das massas, densidade e bordas têm diferenças relevantes, que não devem ser reduzidas a ressalvas de uma entrega artística concluída.

### Limites da evidência e decisão de status

Os sweeps usam o collider físico real, incluindo offset/escala, com orçamento de45s e12mil nós; comprovam conectividade no grid amostrado até aproximações com tolerância1.25u. Não comprovam apertar E, abrir portas, colher, pescar, atravessar portais ou o alcance exato de cada interação. O poço decorativo permanece identificado como `well_visual`. A câmera real preserva resolução/aspect/ortho e não inclui UI Screen Space Overlay. Métricas de sprite bounds incluem geometria/transparência, não somente a silhueta opaca.

Revisão audit-only da extensão RoofReveal não encontrou regressão confirmada no wiring atual: Town usa configuração legada; Leaf/Threshold da Farm não têm SpriteRenderer e portanto não disputam `enabled` com o novo array. Os testes escritos cobrem enable do interior até o último collider sair e mantêm a cor original. Reativação e interação física pela porta não foram executadas por esses testes.

**Estado vigente: NOVA VERSÃO EM REVISÃO; spec IN_PROGRESS; promoção NO.** A versão anterior foi rejeitada; esse veredito não é automaticamente atribuído às novas imagens ainda não avaliadas pelo humano. Os resultados técnicos históricos permanecem, mas não encerram a entrega artística. Ações por input e animação em movimento permanecem NOT RUN. Não há GLOBAL_PASS nem integração do estudo12frames.

O closeout aplica `finish-spec`: reusa a [spec de aceitação humana Farm](../../.specs/a_implementar/spec_farm_scene_keyart_playmode_acceptance_v1.md) e o [registro humano por wave](FINAL_HUMAN_VALIDATION_BY_WAVE.md), sem criar uma segunda suíte obrigatória. A rota e as ações executadas pelo humano devem usar as âncoras atuais da reconstrução; o estado desses cenários continua NOT RUN nesta entrega. As capturas automatizadas não encerram essa aceitação nem integram o estudo12frames.

## 7. Reabertura visual — auditoria independente após rejeição humana

Comparação read-only entre a keyart aprovada e o diagnóstico `regen_06/farm_capture.png`. Os enquadramentos têm aspect e zoom diferentes; não se usa diferença bruta de pixels como medida de escala. Os achados são de silhueta, relação entre objetos, continuidade e distribuição:

1. **Montanha:** a cena usa uma faixa fina de textura pequena repetida com recortes retangulares. A referência tem grandes volumes irregulares, fissuras e sombras, com uma base integrada ao terreno. A correção deve reconstruir massas e peças; apenas cor ou escala da textura não resolve a composição.
2. **Bosque:** a cena forma colunas/muralhas de copas, com trechos quase retangulares e alguns verdes ciano destoantes. A referência apresenta profundidade, alturas variadas, clareira orgânica, sub-bosque e chão sombreado. Romper as bordas retas e agrupar por silhueta tem prioridade sobre adicionar árvores aleatórias.
3. **Lago e rio:** água uniforme, pedregulhos periódicos e bordas em escada criam leitura de recorte. A referência usa orlas irregulares, manchas de profundidade, vegetação aquática e conexão entre rocha e água. Corrigir transições, variação e agrupamentos da margem, preservando o corredor físico.
4. **Trilhas e cultivo:** caminhos largos com ângulos retos/degraus e textura fina repetitiva dos campos ainda parecem layout técnico. A referência tem bordas orgânicas, ilhas de grama e solo legível. Preservar os dois campos reais; não plantar sprites fictícios apenas para imitar a imagem.
5. **Escala relativa e densidade:** poço/crafts/fonte têm pouco peso visual perante a casa; pequenos props se dispersam como pontos, deixando grandes vazios. A referência compõe conjuntos de cercas, barris, arbustos e sombras, com crafts relevantes. Ajustar grupos e escala relativa antes de preencher o mapa por quantidade.

O builder do comparativo foi preparado para mostrar referência/cena em revisão/entrega rejeitada06 e destacar três diagnósticos: montanha, bosque e lago/rio. O preview antigo recebeu aviso de rejeição. Após a rodada atual, foi reconstruído com as novas capturas e o estado **Nova versão em revisão**, sem atribuir automaticamente a rejeição anterior à nova imagem. Nenhum C# ou asset Unity foi alterado por esta auditoria documental.

### Primeira captura da continuação — revisão ainda aberta

A inspeção independente de `continuation_art_01/regen_01/farm_capture.png` identificou melhora concreta nas grandes massas da montanha e na cor/profundidade da água. O bosque deixou de formar colunas rígidas, mas as copas broadleaf arredondadas ainda dominam e repetem a silhueta. O maior defeito atual é a borda nua grama→água no rio e nas margens sul/leste do lago. Também requerem revisão a origem da cachoeira na rocha, os caminhos retangulares e grupos florais em diagonais artificiais. Um strip novo de margem está em integração; não há veredito final desta continuação.

[Proveniência da geração landscape](../../art/farm-pixelart-review/prompts/landscape-revision-generation.md) confere seis pares raw/asset por SHA-256, mede dimensões do header PNG e distingue os resumos de prompt de transcrições literais. O registro foi atualizado após o executor acrescentar strip de margem e grama nova. Igualdade binária não representa aprovação de grade, transições ou qualidade artística.

Por autorização posterior explícita, `FarmSceneCapture` recebeu **uma vista adicional** `farm_capture_keyart_composition.png`:1600×1000, ortho22, centro(0,0), cobrindo70.4×44unidades. O aspect1.6 é diferente dos aproximadamente1.777 da keyart; backdrop acima do mapa pode ficar fora desse enquadramento. A mudança não altera a câmera real nem remove o diagnóstico1600×1200. A execução atual gerou essa vista, que é usada no comparativo com o diagnóstico legado acessível como alternativa. Inspeção independente observou também parte superior do portal da caverna recortada nessa vista fixa; o diagnóstico automático preserva sua extensão completa. Isso é limitação do enquadramento, não remoção da estrutura.

Os diagnósticos1600×1200 usam enquadramento automático pelos bounds dos renderers. Alteração na extensão do Tilemap ou de sprites pode mudar ortho/centro, mesmo mantendo a resolução; portanto antes/depois são rotulados como comparação aproximada, não mesma câmera. A vista adicional ortho22/centro0 tem configuração fixa, mas não existe retroativamente para o baseline rejeitado06.

### Correção focal de tint runtime das árvores

O orquestrador identificou que `TreeNode.UpdateVisual()` sobrescrevia a cor saudável de sprites autorados com o verde de placeholder(0.24,0.48,0.22), inclusive em Configure/Awake/restore/regrowth. A correção autorizada introduz `_healthyTint` serializado com esse mesmo default legado e argumento opcional `Color? healthyTint = null` em Configure. A Farm opta por Color.white no gerador; outros consumidores mantêm o default. A interpolação por dano, cor de árvore cortada, HP, drops, regeneração e DTO de save não foram alterados.

Não havia fixture de TreeNode; `TreeChopServiceTests` cobre outro sistema. A nova `TreeNodeVisualTests` tem três casos: branco sobrevive Configure/Awake/restore saudável; ausência de override conserva cores legadas saudável/danificada; dano/corte permanecem e dois dias de regeneração restauram o branco e HP. Os handlers são invocados diretamente por reflexão, sem SendMessage. **TreeNode3/3PASS**, conferido no [XML focal da continuação](playmode/farm_keyart_delivery_20260909/continuation_art_01/focal-tests_01.xml). O filtro completo teve10/11, com uma falha distinta de densidade Homestead; portanto não é PASS global.

### Estado intermediário da continuação (histórico)

[Focal01](playmode/farm_keyart_delivery_20260909/continuation_art_01/focal-tests_01.xml): TreeNode3/3 e composição4/4PASS; planner3/4, com `Plan_AllBiomesAreWithinDeclaredRanges` FAIL em Homestead. O executor corrigiu depois a densidade junto à casa, sem reduzir o mínimo do teste. Regeneração03 teve exit0 e foi inspecionada pelo orquestrador: junções de margem limpas, cap azul superior removido, minérios no chão e melhora material de bosque/grama/cliff/lago. Naquele momento, a rodada04, o rerun Planner e PlayMode8/rotas16 ainda estavam pendentes; os resultados posteriores estão na seção seguinte. A nova versão ainda não foi avaliada pelo humano.

O [registro de bounds do chão](playmode/farm_keyart_delivery_20260909/continuation_art_01/ground-bounds-review.json) confirma mudança real do enquadramento automático: célula9.796875u,48tiles, cobertura visual78.375×58.78125u e ortho automático28.5→31.4. O diagnóstico novo e o anterior não têm zoom idêntico. A vista adicional1600×1000 mantém ortho22/centro0, cobrindo a área do mapa; a câmera de gameplay permaneceu inalterada. Isso documenta escala/enquadramento, sem afirmar que o novo PNG representa grade nativa aprovada.

### Resultado da continuação atual — nova versão entregue para revisão

| Evidência atual | Resultado conferido |
|---|---|
| Regeneração04 da continuação |Exit0,10PNGs diagnósticos, incluindo vista adicional fixa |
| TreeNodeVisualTests |3/3PASS no focal01; comportamento legado, branco, dano/corte e regrowth |
| FarmSceneCompositionContractTests |4/4PASS no focal01 |
| FarmDecorationPlannerTests |4/4PASS no [rerun02](playmode/farm_keyart_delivery_20260909/continuation_art_01/planner-tests_02.xml); mínimo Homestead preservado |
| Câmera de gameplay |[Processo](playmode/farm_keyart_delivery_20260909/continuation_art_01/gameplay_01/process.json) exit0; [metadata](playmode/farm_keyart_delivery_20260909/continuation_art_01/gameplay_01/capture-metadata.json)8/8vistas,zero runtimeErrors |
| Conectividade física |16/16aproximações,6529nós,collider real; não equivale a ações por input |
| Integridade da captura |Cena antes/depois/atual idêntica; saves0→0 |
| Comparativo atual |[delivery.html](../../art/farm-pixelart-review/delivery.html),15PNGs originais embutidos, estados e enquadramentos explicitados |

Hash atual da FarmScene conferido diretamente com a metadata: `7e2ffd6919591e634444d943ae8eb26d1eff4aeca886a8f03f0b8cbe60a2ea2c`. O focal01 de10/11 continua histórico FAIL; o rerun Planner04/04 comprova a correção sem apagar a primeira falha. As evidências TreeNode/composição foram reutilizadas porque seus inputs permaneceram equivalentes, sem suíte extra por cerimônia.

O orquestrador inspecionou a composição da [regeneração04](playmode/farm_keyart_delivery_20260909/continuation_art_01/regen_04/) e as [oito imagens reais](playmode/farm_keyart_delivery_20260909/continuation_art_01/gameplay_01/): player visível no spawn/bosque/caverna, pés apoiados na ponte, margens/junções corrigidas e canteiros vazios intencionais. A auditoria independente conferiu a vista fixa e frames de bosque/lago: melhora material de cliff, margens e massa irregular do bosque, tint das árvores preservado. Permanecem diferenças de composição, props e vegetação perante a referência; copas/frutas/grama têm contraste diferente. Campos vazios não demonstram maturidade de crops nem substituem jogar o loop de cultivo. O preview começa pela visão automática completa, que inclui o portal inteiro; o recorte fixo ortho22 fica como alternativa claramente identificada.

**Impacto material de densidade:** o [audit de identidade da cena](playmode/farm_keyart_delivery_20260909/continuation_art_01/scene-identity-review.json) registra50→71TreeNodes. Todos os50índices anteriormente materializados foram retidos;21índices antes ausentes agora estão ativos. Não é a mesma quantidade nem somente mudança cosmética: há mais árvores/recursos disponíveis na nova cena. O conjunto total difere (`treeIndicesPreserved=false`), enquanto o subconjunto anterior foi mantido (`allPriorTreeIndicesRetained=true`). Os71healthyTint são brancos e Homestead tem19decorações no intervalo5..48. Saves existentes não foram carregados nem alterados na captura; economia de madeira e loop prolongado não foram reavaliados nesta evidência.

**Finish-spec desta continuação: evidência Unity SCOPED_PASS; nova versão EM REVISÃO; spec IN_PROGRESS; promoção NO.** A versão anterior foi rejeitada pelo humano; a nova aguarda seu veredito. Movimento real por input, interações, animações em movimento e integração12frames continuam NOT RUN. Nenhum cenário humano foi marcado como concluído e nenhuma spec foi movida.

## 8. Rodada coordenada F01–F05 — evidência técnica da rodada02

O snapshot `continuation_art_01/regen_04` acima permanece como baseline desta rodada. A autorização humana posterior pediu subtasks até fechar a FarmScene; o snapshot técnico anterior não foi tratado como encerramento artístico.

F03 introduz `FarmSettlementVisualComposer.Create(Transform)`, integrado pelo owner do gerador sob FarmDecoration. São 25 visuais existentes: grupos baixos junto à casa/estufa, quatro apoios fora do solo arável e detalhes nos flancos dos edifícios sul. Os painéis novos não fecham os pátios nem acrescentam colisão; coop conserva seus painéis anteriores. Nenhum componente de gameplay, recurso, planta madura fictícia ou PNG novo foi criado por esse helper. Revisão cruzada moveu os apoios da estufa para fora do rio e as caixas norte para fora da trilha antes do freeze.

F05 acrescenta um smoke de seleção real, separado da prova de16aproximações: porta FarmHouse/Door e três CraftingPoint (Workbench, Forge, CookingStation). Após oito fotos, o helper usa endpoints alcançados pelo BFS e até12 alternativas locais com sweeps/overlaps do collider real, em raio2u. Aguarda pelo menos dois passos reais de física e consulta `InteractionSystem.GetCurrentInteractable()`; não executa `Interact`, não injeta candidatos e não altera o alcance. Cada alvo produz status, posição, tentativas, esperado/selecionado e motivo. Posição/velocidade são restauradas; hashes de cena/saves continuam obrigatórios. Essa prova cobre seleção, não abertura da porta ou execução de receitas.

O builder lê os diretórios explícitos desta rodada, preserva o overview completo primeiro, destaca cultivo/lago/casa e mostra quatro seleções com sua limitação. A preparação precedeu a execução; os resultados atuais estão abaixo. Aceite humano segue NOT RUN, spec IN_PROGRESS e promoção NO.

### Primeira execução F05 — falha de seleção da porta preservada

`closing_pass/gameplay_01/capture-metadata.json` registra **FAIL**, apesar de8/8imagens,16/16aproximações físicas,6529nós,zero erros runtime e hashes preservados. O seletor real reconheceu3/3estações, mas não a porta:13tentativas, resultado `none`. Isso confirma por que conectividade até1.25u não bastava para declarar interação disponível.

A auditoria do gerador e da geometria registrada identifica uma janela frontal extremamente estreita: blocker da porta começa emY5.88, corpo do player se estende1.125u acima dos pés, logo limite teórico do rootY4.755. O trigger da porta começa emY5.18; a seleção usa origem nos pés e alcance0.45, exigindo rootY≥4.73. A interseção é somente0.025u antes da margem de contato (`Physics2DSettings` registra0.01). A amostragem0.25u pode não alcançar essa faixa; não é prova de porta absolutamente impossível, mas evidencia uma aproximação frágil. Por decisão do orquestrador, o probe/alcance/budget ficam intactos e o owner corrige apenas a área frontal do trigger, preservando blocker. A execução seguinte deverá comprovar seleção normal, sem candidato injetado.

### Correção e evidência vigente — closing_pass02

O gerador ampliou somente a altura do trigger da porta de1.62 para2.22u, mantendo centro, blocker e alcance global. A faixa frontal passou a começar emY4.88; seleção teórica a partir de4.43 amplia a aproximação sem exigir precisão subcentimétrica. O smoke manteve passo0.25u e orçamento anterior. A [metadata02](playmode/farm_keyart_delivery_20260909/closing_pass/gameplay_02/capture-metadata.json) confirma a porta selecionada em(16,4.5) na segunda tentativa. A falha01 permanece preservada.

| Gate desta rodada | Evidência conferida |
|---|---|
| Regeneração02 |[Processo](playmode/farm_keyart_delivery_20260909/closing_pass/regen_02/process.json) exit0;11PNGs diagnósticos |
| PlayMode |[Processo](playmode/farm_keyart_delivery_20260909/closing_pass/gameplay_02/process.json) exit0;8/8vistas e0runtimeErrors |
| Aproximações físicas |16/16PASS,6529nós, collider real; tolerância1.25u preservada |
| Seleção real |4/4PASS: porta, bancada, forja e fogão; duas posições avaliadas por alvo |
| Integridade |SHA da cena antes/depois/arquivo atual idêntico; saves preservados |
| Evidência focal reutilizada |[Revisão por hashes](playmode/farm_keyart_delivery_20260909/closing_pass/reused-evidence-review.json):0drift em6inputs de testes,0drift em6imports e149PNGs baseline |

Hash vigente da cena: `a135037f8cace731aa7f257cd6f24a375cb77ec79e4b28487d0686a69c048a8c`. Os quatro resultados comprovam seleção pelo sistema original após callbacks reais da física; nenhuma porta foi aberta, receita executada ou botão simulado. A evidência focal anterior é reutilizada por equivalência de inputs, não apresentada como uma nova execução. A implementação F03 não acrescentou árvores; o impacto50→71 da continuação anterior permanece documentado e não é revertido nem omitido.

Revisão visual do orquestrador na primeira captura desta rodada: overview, seis regiões e oito vistas sem defeito visual bloqueante; solo2u mantido, detalhes sem cobrir portas, ponte/personagem alinhados. Auditoria independente conferiu casa, animais e cultivo: apoios fora de água/solo, entradas livres e sulcos mais legíveis. Os PNGs diagnósticos02 não são byte-idênticos aos01; a inspeção atual dos oito frames pelo orquestrador confirmou uma diferença material nas copas do bosque. **Fechamento visual e build final suspensos para diagnóstico de reprodução/spriteGUIDs**, apesar do PASS técnico. Não há equivalência visual por hash nem aceite antecipado; nenhum Unity adicional foi aberto pelo owner documental.

**Resultado técnico: Unity SCOPED_PASS; quatro seleções PASS.** Campos novos continuam vazios enquanto a referência mostra cultivos maduros; alguns props/vegetação e densidade de pixels diferem da keyart. Não é réplica pixel a pixel. Aceite humano, ações por input, animação em movimento e estudo12frames continuam NOT RUN; spec IN_PROGRESS e promoção NO.

### Oclusão das copas — correção local de câmera, em validação

O diagnóstico identificou `TransparencySortMode.Orthographic` global (valor2), no qual o eixoY gravado não estabelece ordenação das árvores emWorld/order0/Z0. Não se trata de prova de novos sprites entre01/02: o owner comparou71TreeIndex do snapshot pré-closing com a cena02 e não encontrou diferenças de GUID/posição/escala, mas não existe snapshot estrutural `.unity` de closing01 para afirmar equivalência01→02.

O fix autorizado define `CustomAxis`/`Vector3.up` somente na MainCamera da Farm e na câmera temporária de diagnóstico; ProjectSettings, Town e runtime genérico ficam intactos. Cada View da metadata passa a registrar modo/eixo reais da câmera. O probe de seleção não mudou. Essa alteração exige novas imagens e revisão de oclusão; o build final permanece suspenso até a evidência atual.


## Fechamento F01–F05 — adapter runtime e evidência final pendente

Esta seção supersede somente os claims de estado vigente anteriores; todas as falhas históricas permanecem preservadas. F01 terreno/montanha/água, F02 bosque/acessos, F03 assentamento e F04 integração foram implementados. F05 está aguardando a confirmação do root sobre `closing_pass/regen_04`, `closing_pass/gameplay_04` e `closing_pass/replay_01`; não atribuir PASS a esses resultados antes de conferir seus artefatos.

- **F01:** margem contínua com pedras pequenas e grupos maiores, exclusão de junções internas, cap azul removido acima da cascata; colisão canônica preservada.
- **F02:** 71 árvores mantidas nesta rodada, sem mudança de IDs/posição/escala; tint local de folhosas e clareiras/acessos preservados. A diferença aparente entre copas veio de oclusão, não de sorteio de espécies comprovado.
- **F03:** `FarmSettlementVisualComposer` compõe 25 detalhes de casa, apoios de cultivo e edifícios sul. Revisão pré-integração retirou objetos de cima do rio e da trilha. Nenhum recurso coletável ou collider novo; campos continuam livres para cultivo real.
- **F04:** helper integrado sob FarmDecoration; VisualFieldGrid2u torna os sulcos legíveis sem mudar FarmTileGrid. Porta recebeu correção local do trigger: janela frontal útil de0.025u para0.325u, sem alterar blocker nem alcance global.
- **F05:** seleção real original, física e sorting documentados separadamente. O novo gate exige oito Views com CustomAxis/Y, sem configurar a câmera no teste.

### Falhas preservadas e causa

`closing_pass/gameplay_01` falhou na seleção da porta, com três estações de craft PASS. O trigger deixava uma janela quase inexistente diante do corpo físico do jogador; a correção Editor local foi validada nas execuções seguintes. Seleção PASS não significa abrir porta, executar receita ou testar input humano.

`closing_pass/gameplay_03` reportou Orthographic nas oito Views mesmo depois dos setters Editor; seu PASS antigo não cobria a ordenação correta e não é aceito como evidência final de sorting. O override de Camera não persistiu efetivamente na cena carregada. O novo `CameraTransparencySort2D`, anexado só à MainCamera Farm, reaplica CustomAxis/Y em OnEnable. Reuse audit encontrou apenas helpers Editor e adapters de movimento/zoom; nenhum adapter de sorting runtime existente. ProjectSettings e Town permanecem intactos. A câmera de diagnóstico Editor usa o mesmo contrato explicitamente.

### Reuso e limites

Os 11 testes focais anteriores (Planner4, Composition4 e TreeNode3) são reutilizáveis por equivalência dos seis inputs em [reused-evidence-review.json](playmode/farm_keyart_delivery_20260909/closing_pass/reused-evidence-review.json), sem apresentá-los como nova execução. O novo adapter exige evidência própria em PlayMode; não é coberto retroativamente por esses testes. Snapshot pré-closing versus cena02 preserva71TreeIndex/spriteGUID/posição/escala; ausência de snapshot estrutural01 impede afirmar comparação YAML01→02.

Enquanto os resultados finais não forem confirmados, estado **IN_PROGRESS / F05 PENDING**, promoção **NO**. Não há aceite humano, prova de caminhada por input, execução de crafting, teste de todas as interações ou aceitação das animações nesta entrega.


## Resultado vigente — fechamento técnico F01–F05 confirmado

Em09/09/2026, o root confirmou a inspeção do overview e das oito vistas finais: oclusão, apoio dos pés e entradas aprovados nesta revisão de implementação. **F01–F05: SCOPED_PASS técnico; cena entregue para revisão humana.** Spec **IN_PROGRESS**, promoção **NO** por aceite humano pendente. Esta seção substitui os estados pendentes acima, sem reclassificar execuções antigas.

| Gate atual | Evidência e resultado |
|---|---|
| Regeneração | [regen_04/process.json](playmode/farm_keyart_delivery_20260909/closing_pass/regen_04/process.json): exit0, sem timeout; scan0 confirmado pelo owner |
| PlayMode | [gameplay_04/capture-metadata.json](playmode/farm_keyart_delivery_20260909/closing_pass/gameplay_04/capture-metadata.json): PASS,8Views,0runtimeErrors,16rotas e4seleções reais PASS |
| Sorting efetivo | As8Views registram CustomAxis/Y; adapter runtime presente e configuração não é forçada pela captura |
| Integridade | Hash da FarmScene antes/depois/arquivo conferido: `8e68982b60724243fbef52823cf6b1c9343797788c4237b03166b9c3f940ab6c`; saves preservados |
| Repetição diagnóstica | [replay_01/process.json](playmode/farm_keyart_delivery_20260909/closing_pass/replay_01/process.json): exit0, sem timeout; root conferiu8/10PNGs byte-idênticos e todas as6regiões idênticas, incluindo bosque |
| Testes focais |11 anteriores reutilizados, com6/6hashes dos inputs reconferidos iguais à evidência; não foi executada nova suíte |

Limite de reprodução: dois overviews diferem em142/200pixels, na pequena árvore sudoeste com empateY; boundingbox relatado281,941–293,972 no overview. A diferença residual está fora do bosque revisado. **Não se alega determinismo visual integral.** Esse resíduo foi aceito pelo root nesta revisão técnica, não por um humano usando o jogo.

A FarmScene atual materializa o conjunto de seis regiões com arte GPT versionada, terreno modular, bosque, casa/estufa, canteiros, prédios sul e lago. Diferenças da keyart permanecem explícitas: campos começam vazios para cultivo real, composição não é réplica pixel a pixel e alguns assets/densidades diferem. Teste humano por input, execução de interações/receitas, animação em movimento e aceite artístico final não foram realizados por esses gates. Não há GLOBAL_PASS do projeto nem release build declarado. Nenhum C#/Unity adicional foi executado para este fechamento documental.


## Revisão G — proporções, ponte e bordas (histórico concluído)

Nova orientação humana sucede F01–F05: corrigir proporções perante o personagem, melhorar a ponte, distribuir árvores e fechar visualmente limites inacessíveis. G01–G03 implementados; root revisou overview e8frames da primeira execução e considerou a direção boa. G04 foi concluído com `proportion_pass/regen_02` e `gameplay_02`, após retirada dos mini-penhascos repetidos da borda; os resultados conferidos constam no fechamento histórico abaixo.

| Elemento | Decisão / altura visível medida em01 |
|---|---|
| Player | Mantido1.1875u; câmera mantida |
| Casa | Mantida largura alvo8.5u, altura visível7.017u: reduzir apertaria a porta e cobertura do footprint |
| Estufa | Target5.8u, alpha visível5.624u |
| Barn / coop | Targets6.2/4.5u; alpha visível6.024/4.353u |
| Processing | Target4.9u, redução somente no filho visual |
| Bancada / forja / fogão | Target1.5u; alpha visível1.440/1.446/1.442u; renderers em filhos e geometria local recomposta |
| ShippingBin | Target1.25u; alpha visível1.200u, root funcional mantido |
| Poço / fonte | Targets2.2/3.5u; alpha visível2.200/3.397u |
| Ponte nova | PNG RGBA byte-exato, alvo visível5.6×1.40u; medida alpha05 5.606×1.401u. Uniforme, Ground5, deck apoiado em(27,2.5), corredor físico preservado |

As medidas de01 estão em [after-scale-inventory.json](playmode/farm_keyart_delivery_20260909/proportion_pass/after-scale-inventory.json), alpha≥0.05; pequenos deltas perante bboxalpha>128 usado no alinhamento não são distorção de escala. A cena/hash final02 consta no fechamento histórico abaixo.

Seis TreeNodes existentes (IDs62–67) foram distribuídos em três pequenos grupos com rejeição de footprints/trilhas e separação mínima; total71IDs preservado, sem novos recursos. `FarmPerimeterVisualComposer` acrescenta árvores de fundo inteiramente fora das faces jogáveis, mantendo abertura de Town e sem TreeNode/collider. A última revisão remove os mini-penhascos repetidos, preservando o perímetro vegetal.

[Focal-tests_01](playmode/farm_keyart_delivery_20260909/proportion_pass/focal-tests_01.xml) comprova9/9PASS (Planner5 e Composition4), não11testes antigos. Reuso para02 depende dos quatro hashes em [focal-inputs.json](playmode/farm_keyart_delivery_20260909/proportion_pass/focal-inputs.json), reconferidos no fechamento; mudança visual isolada do perímetro não exige duplicar esses testes.

Aceite humano, comandos por input, abertura/execução das interações, receitas e animações em movimento permanecem pendentes. Spec não será promovida automaticamente por captura/rotas/seleção PASS.


### Fechamento histórico G01–G04 — SCOPED_PASS técnico

A cena final desta entrega é a de `proportion_pass/regen_02`, com hash **eb848c096e5e13cd015e61516ee0e7e11374708cbb22b37cf22cb9515f723d16**. Hash do arquivo atual e antes/depois de PlayMode conferidos. [Metadata02](playmode/farm_keyart_delivery_20260909/proportion_pass/gameplay_02/capture-metadata.json) e processos [regen02](playmode/farm_keyart_delivery_20260909/proportion_pass/regen_02/process.json)/[gameplay02](playmode/farm_keyart_delivery_20260909/proportion_pass/gameplay_02/process.json) confirmam exit0 sem timeout,8vistas,16rotas PASS (6559nós),4seleções reais PASS,0runtimeErrors e CustomAxis/Y nas8câmeras. Saves preservados. Scans sem erros confirmados pelo owner.

Root inspecionou overview e animais02, confirmou a ponte02 e revisou os oito frames01. A alteração visual exclusiva entre01/02 foi remover mini-penhascos repetidos da borda; não se afirma uma segunda inspeção individual de todos os oito frames02. Ponte reta nova e proporções foram aprovadas nesta revisão técnica, sem equivalência a aceite humano.

Os9testes focais01 permanecem válidos para02: [focal-reuse-review.json](playmode/farm_keyart_delivery_20260909/proportion_pass/focal-reuse-review.json) registra quatro inputs idênticos, também reconferidos diretamente; nenhuma nova execução fictícia. TreeNode3 anterior reutilizado pelo owner por dois inputs equivalentes, separadamente dos9focais. [Inventário final de escala](playmode/farm_keyart_delivery_20260909/proportion_pass/after-scale-inventory.json) e [identidade de cena](playmode/farm_keyart_delivery_20260909/proportion_pass/scene-identity-review.json) preservam a medição e71IDs, com somente seis posições redistribuídas. Perímetro novo não acrescenta collider nem TreeNode.

**G01–G04 concluídos tecnicamente. Spec IN_PROGRESS, promoçãoNO.** Permanecem fora desta prova: aceite artístico/humano final, caminhar/interagir por input, abrir portas e executar receitas, animações em movimento. Os campos começam vazios para cultivo real; a cena é adaptação da keyart, não réplica pixel a pixel. A diferença residual de replay da rodadaF é histórica e não prova determinismo da nova rodadaG, que não teve replay dedicado. Nenhum claim GLOBAL_PASS ou release build é feito.

## Revisão H — trilhas, clareira da caverna e mural

Esta rodada sucede G e trata o pedido humano de ruas mais naturais, vegetação sem bloquear a passagem e entrada exterior mais próxima da keyart. A boca passou de(-28,18.5) para(-19.5,17), retorno de(-23,15) para(-19.5,14.5) e mural para(-16.5,15.5), com constantes compartilhadas pelo gerador, trigger, footprint e provas. CaveScene, geração procedural, snapshots/stable run e esquema de save não foram alterados.

Os caminhos agora usam polígonos ramificados e VisualPathGrid exclusivo de0.25u; o gerador deixou de criar a praça retangular de crafting. Essa escala é apenas de renderização: o grid de cultivo e a física foram preservados. Quinze visuais baixos de jardim foram materializados sem Collider/TreeNode. O mural roxo provisório foi substituído por sprite GPT RGBA versionado, filho World0 com altura alpha real1.6u e apoio medido, preservando root/profile/trigger/postings. A reserva de copas protege boca, trilha diagonal e mural; a auditoria final preserva71IDs, com29posições alteradas perante G e sem mudança de sprite, escala ou TreeData.

Histórico desta revisão: regen01 falhou ao colocar Tree43 na bolsa preferida; o fallback determinístico mantém as primeiras160tentativas e percorre outras bolsas sem reduzir separação/máscaras. Regen02 gerou a cena, mas o root rejeitou os degraus de1u e copas sobre a diagonal. Focal01 teve30/31PASS e Meadow11 abaixo do mínimo12; um grupo intencional no sudoeste corrigiu a composição sem afrouxar teste. Play01 teve8vistas/16rotas/5seleçõesPASS, incluindo cave, mas não validava as correções visuais posteriores. Esses resultados permanecem históricos; o fechamento vigente está comprovado por regen04 e Play02 abaixo.

### Fechamento histórico H01–H04 — SCOPED_PASS técnico

Cena final: **1c6311a582901d2f69a60a3d3e90e43940e95c3d18e8d1485ca3c7944f764844**, conferida no arquivo atual, snapshot de regen04 e antes/depois de Play02. Processos [regen04](playmode/farm_keyart_delivery_20260909/path_cave_pass/regen_04/process.json) e [Play02](playmode/farm_keyart_delivery_20260909/path_cave_pass/gameplay_02/process.json) têm exit0 sem timeout; scans sem erros. [Metadata final](playmode/farm_keyart_delivery_20260909/path_cave_pass/gameplay_02/capture-metadata.json):8vistas CustomAxis/Y,16rotas físicas PASS com6455nós,5seleções reais PASS e0runtimeErrors. Cena/saves preservados. A quinta seleção prova a entrada exterior da cave em(-19,15.5), sem executar transição.

Testes: [Planner03](playmode/farm_keyart_delivery_20260909/path_cave_pass/planner-tests_03.xml)5/5PASS na fonte final;26casos de Layout/Spatial/Navigation mantidos por [sete inputs equivalentes](playmode/farm_keyart_delivery_20260909/path_cave_pass/focal-unaffected-reuse.json). Total31casos válidos, sem alegar uma execução única31/31; focal01 mantém seu FAIL histórico. [Auditoria de identidade](playmode/farm_keyart_delivery_20260909/path_cave_pass/scene-identity-review.json) registra71IDs e29posições realocadas perante G, sem drift de sprite/escala/dados. Os15jardins decorativos não têm Collider/TreeNode. [Import do mural](playmode/farm_keyart_delivery_20260909/path_cave_pass/notice-board-import-review.json): PNG/raw byte-exatos, Point, None e sem mipmaps.

**Spec IN_PROGRESS, promoçãoNO.** Entrega técnica H concluída para revisão humana. Não se afirma aceite humano, caminhar por input, execução de quests/receitas/interações, animações em movimento, transição de cave ou determinismo de replay nesta rodada. A composição é adaptação da referência, não réplica pixel a pixel; campos permanecem livres para cultivo real. Não há GLOBAL_PASS do projeto, e seus demais baselines permanecem intactos.

Veredito visual do root: overview de regen04 e todos os oito frames de Play02 inspecionados, adaptação aprovada para entrega. Posição/clareira/mural da cave coerentes com a referência; diagonal visível, caminhos mais finos sem os grandes degraus anteriores, canteiros/portas/ponte preservados. Diferenças restantes: caminhos autorados ainda mais regulares que a keyart, menor densidade de flores/clutter e cultivo vazio intencional. Esta revisão pelo orquestrador não equivale a aceite artístico humano.

## Revisão I — cais e acabamento das trilhas

Continuação solicitada pelo humano após H: corrigir cais/barco fora da água e refinar estradas. O mesmo PNG foi mantido upright, reduzido de5 para4.2u de altura alpha visível e alinhado pelo apoio medido da passarela norte em(10,-8). O deck segue ao sul sobre o lago; o barco permanece sobre água bloqueada. Não houve rotação da perspectiva nem nova geração de arte nesta rodada. Os demais alvos de proporção da revisão G permanecem os mesmos.

LakeCollisionPath abre somente passarela/deck, compartilhado pelo gerador físico e raster de navegação; o polígono de água visual permanece intacto. FishingSpot passou de(6.7,-9.5) para(10,-10), preservando explicitamente farm_spot_7_-10, ID que o fallback antigo resolvia por arredondamento. Sem alteração de save/schema/CaveScene ou alcance global. Caminhos ganharam chanfros locais, ramal terrestre até a passarela e bordas calculadas apenas nos trechos externos da união; visual grid0.25u preservado.

Root inspecionou overview e região água/ponte de regen01 e aprovou o encaixe inicial do cais e o acabamento dos caminhos. A prova final de Play01 abaixo confirma player sobre o deck, água adjacente bloqueada e seleção real de FishingSpot; as capturas iniciais não foram usadas como substitutas desses gates.

### Fechamento vigente I01–I04 — SCOPED_PASS técnico

Cena final **751a9263324f2a322135e9900e969fc5b22f73955db31d629a592296ffacf803**, conferida no arquivo atual e antes/depois de Play01. [Processo regen01](playmode/farm_keyart_delivery_20260909/dock_path_pass/regen_01/process.json) e [Play01](playmode/farm_keyart_delivery_20260909/dock_path_pass/gameplay_01/process.json): exit0 sem timeout, scans sem erros críticos. [XML focal](playmode/farm_keyart_delivery_20260909/dock_path_pass/focal-tests_01.xml):32/32PASS em execução atual. [Metadata](playmode/farm_keyart_delivery_20260909/dock_path_pass/gameplay_01/capture-metadata.json):8vistas,16rotas(6465nós),6seleções reais,3consultas de água adjacente bloqueada e0runtimeErrors; cena/saves preservados. O deck foi alcançado com tolerância0.1u, sem ampliar o collider ou o alcance global. A sexta seleção é FishingSpot; nenhum Interact foi chamado e nenhum peixe foi obtido pelo teste.

[Identidade](playmode/farm_keyart_delivery_20260909/dock_path_pass/scene-identity-review.json):71árvores sem qualquer alteração de posição/sprite/escala/dados perante H,15jardins preservados. ID de pesca legado explicitamente serializado. [Medição de água](playmode/farm_keyart_delivery_20260909/dock_path_pass/dock-after-water-review.json) indica100% da ROI aproximada do casco sobre o lago; a ROI não é máscara exata nem critério visual suficiente por si só.

Root inspecionou overview, região água/ponte e todos os oito frames Play01: adaptação aprovada para entrega, com cais/barco sobre água, escala e pés no deck coerentes, bordas finas contínuas e chanfros melhores. Persistem trechos de estrada autorados mais regulares e terra uniforme perante a keyart. Isso não equivale a perfeição estética global nem a aceite humano.

**Spec IN_PROGRESS, promoçãoNO.** Rodada de proporções executada; cais corrigido nesta I. Permanecem pendentes aceite humano, controles por input, ação de pesca/receitas/quests e animações em movimento. Não há claim de peixe capturado, interação executada, replay determinístico, GLOBAL_PASS ou alteração do stable run da cave. Esta edição documental não exigiu repetir Unity.
