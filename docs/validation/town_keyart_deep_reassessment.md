# Town — reavaliação profunda e correção delegada

## Acceptance criteria extracted

Pedido humano: reavaliar a proximidade real, usar skills novas/Aseprite nos ajustes, executar por subagentes e manter raiz como orquestrador/validador. Meta anterior mantida: pelo menos80%, com NPCs e física preservados. Critérios operacionais: `docs/design/gameplay/city/TOWN_KEYART_ACCEPTANCE_RULE_v2.md`. A spec `town_keyart_fidelity_v1` permanece aberta.

## Existing systems audit

Comparação real da keyart1536×1024 SHA256 `3A2C9EA4D7ABD2E8E6AAEA81401D8CE9CBD8A00AE9F569AF58C1F76AC930E52B` com captura Town1536×1024 SHA256 `09BF8ECB6577F244C53751357FF60A3AEA5AB16E58AE9A91A33EABE99A0AB16F`.

Revisor visual independente `town_independent_visual_audit`: **36/100, faixa31–41**, estimativa perceptual, não algoritmo pixel-match. Raiz concorda com ordem de grandeza35–40%. O aceite82 foi retirado: presença de objeto, pivô, teste/import ou nome de GameObject não estabelece semelhança.

| Discrepância observada | Causa confirmada em código pelo auditor `town_integration_audit` | Correção pedida |
|---|---|---|
| Landmarks pequenos, floresta em moldura | Captura ortho60 abrange180×120; cidade120×90. Árvores seguem quatro retas. Relação largura landmark/distância entre centros também está errada, independente de zoom. | Antes/depois comparáveis e revisão de hierarquia interna/perímetro. |
| Praça sem estátua dominante | Guerreiro de quads em order0; fonte detalhada order6 sobrepõe a construção. | Peça de identidade real e composição anel/jardineiras/aberturas. |
| Mercado parece telhados coloridos | CreateMarketStall usa roof_A/B/C_aerial, com listras de quads. | Banca de lona3/4 editável em Aseprite, alpha verdadeiro e mercadorias/pernas. |
| Bandeiras ausentes e flores suspensas | Coordenadas locais passadas ao helper de posição mundial; flowers orders1/2 vencem hero roofs/tree order0. | Corrigir contrato de transformação e profundidade, validar em escala de jogo. |
| Lago pequeno, rio/moinho ausentes | Água18×16, cachoeira isolada; roda é quad marrom. | Hidrografia contínua e módulos individuais de moinho/cais/pontes. |
| Muralha invisível | Faixas de UISprite/quads cobertas por árvores. | Alvenaria/pilares/portões legíveis sem eliminar os bloqueios físicos. |
| Casas em grade e kit frontal | Fileiras forçadas por teste; telhados/parede com escala não uniforme; serviços reutilizam greenhouse/barn da fazenda. | Preservar IDs/interiores, reorganizar lotes e usar volumes compatíveis. |

## Riscos físicos concretos da baseline

- Árvores: offset mistura unidades locais e mundiais e aplica descida duas vezes; collider não fica no tronco.
- Orlan: work(-17.5,-2) coincide com parede direita da Inn.
- GateKeeper: lote3.6×2 com paredes de1 deixa interior vertical sem passagem.
- Mural da prefeitura: posiçãoy31.9 cai dentro do bloqueio sólido.
- Grafo lógico testa retângulos; não representa integralmente o caminho pintado nem a caixa do ator e colliders gerados. Contagem de Floor/Door/RoofReveal não comprova casa percorrível.

## Responsabilidades

- Raiz: critérios, direção e revisão de arte/capturas/diffs/evidência; sem implementar cena ou pixels.
- `town_aseprite_assets`: fontes/candidatos `.aseprite` editáveis e exports town-only, piloto antes de lote, hash/alpha/estrutura/preview.
- `town_composition_rebuild`: primeiro passe reprovado; após handoff, ownership limitado aos dois helpers de captura/física PlayMode, sem novos edits de composição.
- `town_corrective_integration`: segundo passe de layout/integração Editor/captura/validator e geração Unity, somente após liberação da janela Farm.
- `town_third_pass`: assume composição/integração corretiva após reprovação visual59/100 da revisão02. Os dois executores anteriores encerraram esse ownership.
- Revisores independentes: audit-only. Farm, Cave, runtime/save e alterações concorrentes permanecem fora do escopo.

## Spec Compliance Matrix

| Critério | Estado atual | Evidência necessária |
|---|---|---|
| Reavaliação honesta | PASS | Imagens abertas, divergências visíveis e causas de código separadas. |
| Fidelidade>=80 | FAIL revisão03 | Nova cena regenerada, revisão independente e bloqueadores resolvidos. |
| Aseprite editável/alpha | PASS de arte individual, não da cena | 15 candidatos reabertos, hashes e alpha binário; `art/town-keyart-rework/assets/FINAL_ASSET_MANIFEST.json`. |
| Escala/suporte em cena | PENDENTE novo estado | Capturas comparáveis e medidas reais dos sprites após Awake. |
| Preservação/NPCs/física | PENDENTE novo estado | IDs, testes e geometria materializada; comportamento separado. |
| Não regressão e escopo | PENDENTE integração | Diff próprio, cenário comparável e logs dos inputs finais. |

## Validation

Baseline visual: EXECUTADA. Auditoria técnica: estática, EXECUTADA. Unity/Test Runner/captura do novo estado: ainda não executados na abertura desta revisão. XMLs históricos não validam correções futuras. PlayMode/sorting em movimento: NOT RUN na baseline; imagens estáticas não comprovam esses itens.

### Escala: evidência adicional, sem inferir runtime da imagem

As novas capturas baseline ortho45 e ortho8 foram abertas pela raiz em `art/town-keyart-rework/evidence/baseline-20260909/`. O ator laranja pequeno da praça é o Player:70px opacos/128PPU×1.3=0.711u no EditMode. O perfil reaplicado em Awake leva a1.094u. NPCs Liora/Orlan:128px/234PPU×2=1.094u; walk de Orlan~1.145u. Portanto a captura estática subestima o Player em35% e não prova a escala jogável dele. A impressão inicial de todos os NPCs em0.78u estava incorreta.

Flor grande57×64px dimensionada pela largura2.4u chega a2.695u; banco84×64px com largura2.2u chega a1.676u. A correção autorizada imediata é normalizar props da Town. Perfis/Animator/colliders dos personagens compartilhados não devem ser alterados para maquiar a imagem.

### Revisão dos pilotos Aseprite

Fonte: a própria keyart, objetos individuais isolados com Aseprite1.3.18.5 e Lua; sem piso/bairro como decal. Camadas baseline ocultas preservadas; máscara e partes nomeadas editáveis. Raiz abriu referência ampliada e exports claros1:1 e ampliados.

| Piloto | Revisão | Observação | Decisão limitada |
|---|---|---|---|
| Banca azul95×110 | v01 | Lona/pernas/balcão reconhecíveis; piso bege anexado sob pé/barril direito. | REPROVADO para promoção; limpar borda. |
| Fonte/estátua130×211 | v01 | Identidade correta; piso bege junto braço e cabeça. | REPROVADO para promoção; limpar máscara. |
| Banca azul | v02 | Piso residual grande removido;3326px opacos e7124 transparentes, alpha binário,3 camadas/1frame reabertos segundo log inspecionado. | Liberado para TESTE de integração, não aceite final de cena. |
| Fonte/estátua | v02 | Faixas de piso removidas;10946px opacos e16484 transparentes,4 camadas/1frame; restam poucos pixels verdes soltos junto capa/escudo. | Integrar após limpeza local v03; revisar resultado real. |

A ampliação parece suave porque a fonte pode conter suavização; a causa do filtro não foi inferida da aparência. O autor verifica nearest por igualdade de blocos/cores. Fontes nativas pequenas continuam pequenas: ampliar não adiciona detalhe. O aceite em escala de jogo permanece pendente.

Fonte/banca v04: raiz abriu estátua3x, constatou limpeza dos resíduos grandes; logs finais registram10796/16634 pixels opacos/transparentes na estátua e3326/7124 na banca, alpha intermediário0. Nearest3x comprovado pelo autor: zero divergências em298278pixels; a suavização já pertence à fonte. Hash original confirmado inalterado.

Família de casas/moinho/muralha: raiz abriu galeriasv01,v02,v03. Reprovou fundo serrilhado junto telhado azul e piso bege sobre verde emv01; pediu margem no moinho. Galeriav03 limpa os principais contaminantes, preserva casas3/4 e água pode aparecer entre raios da roda. Arquivos liberados para integração, não aceite da cena; portas/padding/escala/sorting ainda requerem captura real. A muralha teve threshold agressivo emv02 rejeitado pelo próprio autor e highlights restaurados emv03.

## Honest status rationale

### Passe de integração01 — REPROVADO

Captura comparável `art/town-keyart-rework/evidence/revision-01/town-comparable45.png`, SHA256 `D5A1A25530A76E6A7504967061CFED562F4E5E06530F269BE5B552CD9DD9B09D`, aberta pela raiz. Banca/estátua e hierarquia melhoraram, mas o lago desapareceu e surgiu retângulo de água no centro; telhados cívicos cortados, pátios excessivos, floresta retangular e casas/celeiro antigos permanecem. Estimativa intermediária da raiz~43/100 (faixa38–48), sem aceite; não é score final da família Aseprite ainda não integrada.

O processo de geração/captura terminou exit0 (`Logs/town_keyart_rework_revision01.log`), mas audit de água registrou **lotes1, vias0: FAIL**. Exit0 não significa gates aprovados. Contagens reportadas:29NPCs,24casas,563árvores,23stalls,84anchors,2spawns. Testes novos e validator materializado não executados nesse passe.

Causa da água: mesmo Grid reutilizado para margem e água com cellSize/PPU diferentes, enquanto rasterização usa a escala da segunda sprite. A correção deve ser Town-only; não modificar `WorldTilemapGround` compartilhado com a fazenda. O primeiro executor parou edits e entregou handoff; `town_corrective_integration` assume o próximo passe conforme instrução humana de trocar executor após reprovação.

### Preparação do passe02 — ainda sem score

Os15 exports finais incluem três casas3/4, moinho, padaria, alquimia, dois abrigos, três bancas de tecido, estátua/fonte, muralha, pilar e pedra quente. Todos têm fonte/candidato Aseprite, baseline oculta e camadas de correção. Manifesto e handoff registram portas aproximadas, padding, hashes e limitações. Verificação do autor:15/15 reaberturas e zero diferença entre export e render nativo; a raiz abriu as galerias e aprovou somente o teste de integração.

Segundo executor preparou malha Town-only de1u, lago/river conectado, substituição dos kits genéricos, posicionamento pelas portas desenhadas, jardim/pátio aberto no curral e marcos de24u. Nada disso recebe pontos antes da captura real. Compilação gerada.NET reportada pelo executor:7/7 projetos, zero erros; não substitui compilação/captura Unity nem PlayMode. A janela Unity permanece coordenada com a tarefa Farm.

A raiz revisou integralmente os helpers de PlayMode: casts/overlaps usam o collider real, teleporte serve somente para enquadramento, budgets incompletos produzem PARTIAL e saves/cena são comparados por hash. Encontrou e devolveu ao autor um erro de censo:28 NPCs usam NpcShopController, enquanto a primeira versão só media NpcController. Correção exigida antes da execução, incluindo atividade e sprites válidos. Captura real e107 registros físicos (106 rotas mais conector) seguem NOT RUN nesta preparação.

NEEDS_REWORK. Este documento registra a abertura e diagnóstico, não uma entrega concluída. Resultados posteriores devem apontar às revisões preservadas e seus hashes, sem substituir ou atribuir a baseline o score de outra captura. Uma tool/skill terminar sem erro não é aceite artístico.

### Passe de integração02 — REPROVADO,59/100

Captura real `art/town-keyart-rework/evidence/revision-02/town-comparable45.png`, SHA256 `BE29732DB754021AA2460005FCDD9FB05B2A0223FBFFF06EC366A39335CD4B9E`. Pasta preserva14PNGs e inputs/hashes. Raiz abriu visão geral, praça, templo, moinho e portão com sobreposição de colliders. Revisor independente abriu visão geral e detalhes sem receber a estimativa da raiz.

| Categoria v2 | Independente | Raiz |
|---|---:|---:|
| Composição/hierarquia25 |17|14|
| Arquitetura/silhuetas20 |13|15|
| Praça/caminhos/muralha15 |7|8|
| Água/relevo/vegetação15 |8|8|
| Paleta/materiais15 |8|9|
| Microcomposição/vida10 |6|5.5|
| Total |**59/100**, faixa54–64|**59.5/100**, faixa55–65|

As avaliações convergem em aproximadamente60%, não80%. Ganhos reais: marcos com relação de largura/separação~0.44, estátua, bancas de tecido, moinho e lago conectado. Persistem vias em cruz/eixos90°, praça como disco vazio com pequenas jardineiras repetidas, rio reto/escuro e costa angular, casas norte competindo com marcos, cercas desproporcionais e fontes suavizadas misturadas a sprites nítidos. Softness foi verificada no render; nearest/import correto não recupera detalhe nativo ausente.

Bloqueios físicos/visuais confirmados pela raiz e auditor: padaria encobre porta do templo; banco interno aparece sobre fachada; quatro shells de12–16u ultrapassam gables limitados a10.5u; moinho alinha porta mas não corpo assimétrico; collider da água corta o cais; curral conserva divisória invisível/móveis domésticos; pilares sul ficam acima de seus colliders, deixando quadrados isolados e bloqueios sem suporte. Contagens/clearance lógico não invalidam esses achados.

Execução: tentativa01 Unity exit1 por referência inexistente `NpcDataSO.Id` no novo tooling; autor corrigiu para `NpcData.NpcId`. Tentativa02 exit0, compilação/geração/capturas concluídas; logs `Logs/town_keyart_rework_revision02_attempt02.log`. Auditorias lógicas: conflitos de lotes/vias/portas/água0; censo24casas/29NPCs/84anchors/23bancasNPC/2spawns. EditMode, validator físico e PlayMode permanecem **NOT RUN** nesta rodada, não PASS.

Não regressão: importerTown inicialmente amplo atingiu `locations/town_hall/town_hall.png` fora dos15exports. Corrigido para lista exata compartilhada com PrepareAssets antes da segunda tentativa. Auditor registrou meta HEAD128PPU/readable0 versus32/1, GUID preservado; não encontrou referência direta naFarm. Nenhum rollback manual foi feito. Verificar estado final e consumidores antes de aceitar scope; não alegar regressãoFarm observada.

Conforme pedido humano, outro executor (`town_third_pass`) assume correção após o segundo reprovar. Arte permanece separada: dois pilotos Aseprite de limpeza de clusters/paleta serão julgados antes de qualquer lote ou substituição dos exports. Farm retomou a janela Unity para suas validações; o terceiro passe Town é offline até liberação. Não houve alteração das skills compartilhadas: o problema demonstrado é promoção de arte extraída/integração sem prova de escala, não instrução defeituosa comprovada.

### Pilotos de acabamento após revisão02

Raiz abriu os triptychs Aseprite `town_statue_fountain_keyart_pixelclean_triptych_v01_light_2x.png` e `town_house_red_gable_keyart_pixelclean_triptych_v01_light_3x.png` em `assets/previews/`. Fonte com paleta86cores melhora parcialmente contorno; redesenho das estrelas perde identidade e foi reprovado. Casa quantizada perde nuances da fachada e não recupera detalhe ausente; lote de quantização **não autorizado**. PNGs Unity finais permanecem preservados.

Canal ChatGPT normal foi verificado pela interface, logado e com compositor vazio. O autor preparou o prompt; seu navegador não permitiu anexar arquivos. A raiz transportou o mesmo prompt e três referências pelo navegador já habilitado, sem alterar permissões: uma única submissão em Chat normal, sem API nativa ou ChatWork. Conversa: https://chatgpt.com/c/6aa217bd-de34-83e9-9523-122f53fbb7cb. A UI mostrou mensagem de erro junto de uma imagem efetivamente entregue; não houve repetição. Raw preservado em `art/world_gpt/raw/town_statue_native_detail_chatgpt_web_candidate_v01.png`, SHA256 `886B587A74A423557B86FE336B3CF64C535B9E7E1D0CD3A7A5BC4443E0F3108B`. Raiz abriu: detalhe mais definido, mas fundo/halo e interpretação de homem barbado de túnica exigem revisão. Não promovida. Alpha e acabamento Aseprite pendentes; não há alegação de economia medida ou fidelidade garantida.

Human test scenario preparado: `docs/validation/playmode/town_keyart_fidelity_v1_human_test_scenario.md`, status NOT RUN. Tooling do ciclo automatizado de quatro fases de porta foi aplicado e congelado pelo autor: interação pública, casts do collider real, RoofReveal observado após ticks físicos, restauração em memória. Execução ainda NOT RUN.

### Passe de integração03 — REPROVADO

Captura `art/town-keyart-rework/evidence/revision-03/town-comparable45.png`, SHA256 `B9905DCA4D4745F7E0587C863C1BDC6B2A0F2FBE32CBF745B4E24C1AEE55A2D2`; cena `FECBE73B0BFD970A3D2FBA740FF26BFD6E490B3C1B72340D6697A57D8703E6B7`. Raiz abriu referência, overview, praça, casas, lago, porta do templo/colliders e portão. Ganhos: casas cívicas secundárias menores/vermelhas, porta do templo desobstruída pela padaria, pisos interiores não expostos, muralha sul mais visível. O novo anel de jardim é feito de dezenas de muros curvos repetidos e parece um pente: regressão concreta de microcomposição.

Estimativa preliminar da raiz **64/100**, faixa59–69: composição16/25, arquitetura15/20, praça9/15, água8/15, materiais10/15, microcomposição6/10. Não é avaliação independente nem aceite. Persistem bloqueador de vias ortogonais dominantes, rio quase reto sem ponte claramente legível, costa angular/água plana, repetição de casas e moldura de floresta. Flores e cercas soltas ocupam avenidas. Game-scale evidencia grande diferença entre pixels suaves dos recortes ampliados e pixels definidos de NPCs/props; a proporção porta/ator e estátua/ator exige auditoria específica, sem mudar perfil global silenciosamente.

Unity gerou14 capturas com exit0 em `Logs/town_keyart_rework_revision03.log`; contagens não pontuam visualmente. Restauro do import do TownHall para128PPU/readable false feito pelo Editor com hash de fonte preservado e registro próprio. Medidas de alpha dos landmarks unreadable são fallback declarado, não medidas de alpha real. Test Runner posterior:35 casos,34PASS/1FAIL em `Logs/town_keyart_revision03_editmode.xml`; falha na fixture de footprints cívicos antigos (Manor16×10 versus lote9×8), não ocultada. Os13 testes geométricos passaram. Validação materializada e PlayMode ainda pendentes nesta atualização.

Validação posterior03: CitySchedule21PASS/0FAIL em `Logs/town_keyart_revision03_city_schedule.log`; cena salva177PASS/0FAIL em `Logs/town_keyart_revision03_saved_physics.log`. Raiz conferiu o resumo do log: círculos de raio0,35u, grid0,5u e amostragem de arestas0,1u,29002 células alcançáveis. É diagnóstico de colliders salvos, não prova do collider real em PlayMode. Cena permaneceu com o mesmo hash. Janela Unity devolvida à Farm após o último processo encerrar; Town Assets congelados enquanto ela valida.

Gate documental executado via `run_strict_validation.ps1 -Gates docs`: FAIL global de documentação, incluindo oito diagnósticos de estrutura da spec desta tarefa. Campos/cabeçalhos e lista de tarefas da spec Town foram corrigidos sem alterar os gates de fidelidade. Diagnósticos de specs alheias não autorizam edits fora de escopo nem alegação de docs global PASS. Reexecução final ainda pendente.

### Preparação04 — arte nativa e correção da hipótese de proporção

Dois pilotos efetivamente gerados no ChatGPT normal, um por objeto: fonte (conversa já registrada) e casa vermelha em https://chatgpt.com/c/6aa21e91-7950-83e9-be38-c39a95b96d31. A casa teve dois cliques de locator que deixaram o rascunho intacto; a ação AX posterior efetivamente enviou, com compositor vazio e resposta. Não houve segunda geração da casa. UI mostrou aviso de erro junto da imagem entregue, sem repetição. Os raw são preservados; alpha real semitransparente/halo foi convertido em candidata binária no Aseprite, sem alegar que o original tinha fundo opaco.

Raiz abriu comparação fonte antiga/nova, warm/cool, full/half, três versões dos arcos e casa/família. Aprovou apenas para teste04: fonte cool-stone512×768 (~424px ocupados/9u), quatro arcos contínuosv03, casa nativa half512×768 e variantes de telha azul/verde por máscara. Sources/full/crops antigos preservados. Manifestos Aseprite registram layers/alpha/hashes e limites. A pedra dos arcos ainda é mais regular que a keyart, e os novos sprites estáticos de casa ainda contêm porta pintada. Nenhum desses aceites individuais comprova cena>=80, movimento ou interior acessível.

Auditoria `art/evidence/proportion-audit03.md`: estátua/ator≈13,8 na03 contra≈6,3 na referência; NPCs~12–13px contra~30–32px na mesma resolução, não erro resolvível só reduzindo edifícios. Piloto explícito aprovado: perfil Town de fator2,25, atores comuns~2,46u; originais e física mundial preservados por compensação Editor. O helper e captura CA11 foram entregues, mas ainda NOT RUN nesta preparação. Nove testes adicionados cobrem somente comparador de snapshots, não aplicação/rollback reais.

Auditoria independente CA11: WARNING, sem erro matemático encontrado no conjunto suportado. Root/shape/offset e childbranch recebem compensação uma vez; clones foraData/Scale e ColliderScale0. Rollback é de atores em memória, não transacional para AssetDatabase: falha pode deixar clones locais já criados ou evidência parcial, sem limpeza automática. Reaplicação e falha controlada permanecem NOT RUN; Farm→Town→Farm também NOT RUN, não supridas por hashes ou inferência estática.

### Pedido funcional adicional de2026-09-10

Humano pediu casas nítidas, animação de portas, interiores visíveis e colisão dos itens. Registrados spec `spec_town_components_doors_and_collision_v1` e regra `TOWN_COMPONENT_BEHAVIOR_RULE_v1`. Censo exigirá cada objeto sólido e exceções decorativas explícitas; oito troncos existentes não bastam para declarar cobertura de toda cidade. Rasteiros/sombras/telhados não devem criar barreiras arbitrárias.

Diagnóstico de porta: desligar a leaf atual não remove a porta pintada na fachada. Arte05 separará vão/fachada/folha com dobradiça e sequência; API compartilhada opt-in está em elaboração pela tarefa Farm, que assumiu exclusivamente HouseDoorInteractable/RoofRevealController. Town fica com arte/wiring/evidência. ConfigurePresentation proposto usa frames fechado→aberto,80ms por frame como valor inicial, blocker libera no último frame e fechamento seguro. Roof occupancy proposto usa pés/corpo sólido, não o trigger de interação. API/arte/movimento/colisão nova ainda não entregues nem aceitos nesta atualização.

Atualização subsequente: a Farm aplicou os dois runtimes compartilhados durante sua janela exclusiva. A raiz leu os arquivos completos e os testes: a revisão estática não encontrou bloqueador na API opt-in, mas não é aceite funcional. Primeira fixture `docs/validation/farm_keyart_v4/editmode-door-api.xml`: 11 casos, 10 PASS e 1 FAIL, em `FeetMode_DisabledPlayerAndComponentRestoreExteriorWithoutChangingLeafEnabled`, linha 125, após desabilitar o componente no EditMode. Triagem permanece com o owner Farm; não adicionar ExecuteAlways ou alterar o runtime para satisfazer artificialmente a fixture. Animação no player loop e passagem automática de NPC ainda NOT RUN.

Raiz abriu o primeiro portal largo fechado, `assets/previews/town05_original-vs-wide-portal-v04.png`: reprovado para acabamento final/frames. O vão maior corrige a hipótese de largura, mas a folha ficou plana e quase quadrada, a moldura perdeu profundidade e a soleira não acompanha o vão. Autor deve revisar a arquitetura local e comparar dimensões com o NPC materializado04, preservando a casa nativa estática aprovada e todos os ensaios. Dimensão de PNG ou existência de camadas separadas não prova qualidade nem funcionamento.

Rerun compartilhado conferido pela raiz: `docs/validation/farm_keyart_v4/editmode-door-api-rerun.xml`, 11/11 PASS, 0 skipped, às 03:36:37 UTC. A falha anterior era a expectativa de callback automático no EditMode; fixture passou a invocar OnDisable como já fazia com Enter/Stay, mantendo o assert. Runtime não recebeu ExecuteAlways. API aceita para teste de integração, não para declarar C2–C4 cumpridos. Farm devolveu a janela Unity explicitamente; terceiro integrador assume geração/preview04, demais owners mantêm Assets congelados.

Censo final pre04 lido integralmente: `art/town-keyart-rework/evidence/component-physics-audit-pre04.md`. Cena materializada SHA256 `9F2E4F6C817465100D8F6F86BCED7D56F57972D686B8CEA22317BE4F4CAE336C`, distinta do hash no manifest03; não atribuir seus números retroativamente à captura03. Faltas concretas incluem 24 bancos/mesas externos, 13 caixas e 2 fardos; 117 móveis/estações são candidatos sólidos sem suporte dedicado, sujeitos à classificação efetiva de interior/curral. Árvores incluem famílias além de TownTree; oito troncos não demonstram cobertura. Bancas, canteiros, cais e muros têm apoios existentes a medir, não duplicar cegamente. Novo worker de componentes prepara candidatos fora Assets enquanto04 executa; ele não altera atores, runtime compartilhado, Farm ou arquivos do integrador.

### Passe04 materializado — ainda abaixo da meta

`revision-04-preview` foi avaliada independentemente em **67/100 (62–72)**: 18/25 composição, 14/20 arquitetura, 10/15 praça, 8/15 água/vegetação, 10/15 materiais, 7/10 microcomposição. São estimativas perceptuais explicitadas, não similaridade automatizada de pixels. Os quatro arcos contínuos, casas/fonte nativas e atores maiores representam avanço; casas recoloridas repetidas, vazios oliva e acabamento desigual continuam impedindo o aceite.

O hotfix final04 corrigiu a cascata relocada indevidamente e oito apoios do mercado. `art/town-keyart-rework/evidence/revision-04-final/` preserva 14 capturas, inputs, baseline de escala e `TownScene.source-snapshot.unity`. Snapshot, manifest e cena ao encerrar compartilham SHA256 `C4659BAF9F3AB65084744735D456769436C0724A78931DCB2FE6535F0CF30AC3`; overview SHA256 `2CDD898B2ED9878569E8A0871620F65E2DB525849106E668CAE88E68BAEC4993`. Raiz abriu overview comparável, referência e detalhes da praça/casas/porta do templo; nitidez das casas melhorou, mas bancas, padaria/moinho antigos e pilares ainda destoam. Banco sobre copa junto do templo e mesas junto do telhado da Inn exigem correção de apoio/sorting, mesmo fora da água.

Evidência técnica: `Logs/town_keyart_revision04_editmode.xml` **52/52 PASS**, sem skipped (24 layout, 19 geometria, 9 comparador de escala). Fixture de mínimos externos antigos foi substituída por contratos de IDs/archetypes, interior útil e hierarquia real, não apenas reduzida. `Logs/town_keyart_revision04final.log`: Unity exit0, física salva **187/187**, CitySchedule **21/21**. Esses validadores ainda contêm o contrato legado de oito troncos; não provam C-5 nem o corpo real em movimento. Play04 permanece NOT RUN nesta atualização.

CA11 aplicado na preview04: 30 atores, diferença máxima das formas físicas autorais de 0,000003814697265625u; a raiz comparou 62 stamps globais antes/depois e os arquivos correspondentes, sem divergência. JSON final04 novo: `actor-scale-authoring.json`, SHA256 `A7F835259CE88F7088790F8B7C28268D26D7D97B356A54495830EAF07A69D1AE`. Pós-Awake, reaplicação/falha controlada e Farm→Town→Farm continuam provas separadas pendentes.

O hash intermediário pre04 não foi explicado por uma operação identificada. Investigação dos runners e handoffs não comprovou salvamento Town pela Farm; não atribuir culpa nem restaurar YAML presumido. A evidência04 passa a manter snapshot externo exato para eliminar essa lacuna de proveniência nas próximas rodadas.

Complemento independente final04: o revisor abriu as 14 imagens finais, conferiu o snapshot e atribuiu **68/100 (63–73)**: 18/25 composição, 14/20 arquitetura, 10/15 praça, 9/15 água/vegetação, 10/15 materiais, 7/10 microcomposição. Comp/arquitetura continuam abaixo do mínimo individual de75%. Raiz estima aproximadamente67/100 nas vistas que abriu (17,14,10,9,10,7), sem reivindicar segunda inspeção integral das14. Ambas rejeitam o aceite. Achados adicionais do revisor: banca aparenta apoio no guarda-corpo do templo; banco sob bancada de ferramentas; identidade de banners do templo/prefeitura divergente das três estrelas; moinho e portão exigem prova de sorting em movimento. São observações visuais, não falhas de colisão confirmadas por movimento.

### Candidatos05 — arte e mecânicas, não promovidos

Após reprovar o portal v04 e corrigir largura/moldura/soleira, a raiz aprovou **somente para piloto Unity** o portal gable v08, 108×137px, folha em cinco poses registradas no canvas512×768, 80ms por etapa. Manifesto `TOWN05_GABLE_DOOR_PILOT_MANIFEST.json`: fachadas vermelha/azul/verde com alpha real no vão e último frame aberto ainda visível. Largura visual aberta1,8407u em casa8u, sem alterar sua proporção para ampliar só a porta. PNGs estáticos04 preservados. GIF de revisão existe; inspeção estática das poses não é reprodução temporal nem evidência Unity. C-2–C-4 permanecem pendentes.

Dois novos prédios foram gerados uma vez cada pelo **ChatGPT normal**, sem API/ChatWork: padaria (`6aa228c3-7790-83e9-9275-80785d56ce47`) e moinho (`6aa2292e-9e08-83e9-8f74-aeedab9a31e9`). Prompts do autor foram transportados literalmente pela raiz, com três referências e confirmação Chat1/Work0. Ambas as conversas mostraram aviso de erro junto de imagem efetivamente entregue; não houve repetição. Raw preservados, cleanup alpha e versões full/half em Aseprite; manifesto `TOWN05_NATIVE_SHOPS_STAGING_MANIFEST.json`. Raiz abriu comparações na mesma largura e aprovou apenas teste de integração. Padaria half543×724,505×649 ocupados,10u; moinho half627×627,564×585 ocupados,12u. Padaria16,84% mais baixa, moinho+0,98% em altura; não foram esticados. Portas desses sprites ainda estão pintadas: não pertencem ao piloto animado gable.

Candidato de física05 mede bases de16 fontes e classifica exceções. A raiz abriu árvores, banco, caixa, fardo, máquinas e seis sprites de interior: os nomes de mesa/cadeira/armário/tapete estão trocados em relação às imagens. Remap Town proposto preserva assets globais e dimensiona móveis em unidades do mundo. Revisão devolveu bloqueios antes de promoção: identificação de cena ainda não salva, propriedade/layer física, suportes duplicados, contagem de pending, escala negativa e sobreposição entre móveis. Não considerar o candidato compilado ou C-5 completo; todas as famílias ainda não medidas devem continuar explícitas.

Verificador offline do catálogo executado pela raiz: 16 fontes/16 suportes, hashes/dimensões/alpha e limites de retângulos confirmados, exit0. Não valida placement mundial. Gate documental repetido: FAIL global por specs alheias, sem diagnóstico das duas specs ativas deste rework; não houve reparo fora de escopo nem alegação de docs global PASS.

A tarefa Farm informou uma mudança adicional em PlayerWalkAnimator durante sua janela. Esse arquivo integra os hashes CA11; portanto uma modificação legítima também torna a prova04 insuficiente para iniciar o Play sob contrato novo. Será necessária geração Town com baseline novo após o freeze compartilhado, sem editar o JSON antigo nem ignorar divergência de hashes.

### Mudança de método — rodada limitada, 2026-09-10

Por pedido humano de reduzir gasto e aumentar progresso demonstrável, adotado o protocolo da tarefa **Investigar limite do Astra Ultra**, registrado em `.claude/rules/visual-iteration-budget.md`. A skill `delegated-execution` orienta ownership e reaproveitamento de evidência verificável; não exigir repetição de builds apenas por terem sido executados por outro agente.

Os três executores Town foram interrompidos. A janela Unity foi devolvida à Farm sem novo processo Town, geração ou SaveScene. Candidatos de arte, portas, composição urbana e componentes ficam preservados, sem novas frentes simultâneas. Os seis C# de componentes já promovidos permanecem congelados e **sem compilação Unity concluída**. A tentativa Farm posterior abortou por `DefaultSkillActionCatalog.cs:70`, CS0246 `DamageType`, reportado pelo owner como alheio a Town; isso não constitui PASS nem falha atribuída aos seis arquivos Town. O candidato compartilhado de PlayerWalkAnimator continua não aprovado.

**Baseline continua sendo revision-04-final**, com captura e hash registrados acima. Não há cena05 materializada. As notas históricas são avaliações subjetivas por rubrica, não percentuais medidos de semelhança. Nenhuma economia de tempo ou tokens foi medida nesta mudança de método.

As três discrepâncias visuais prioritárias da cena atual são: repetição de silhuetas das casas; nitidez desigual entre casas nativas e prédios/props antigos; objetos aparentando apoio incorreto em copas, telhados e guarda-corpos. Não tentar resolver as três junto das mecânicas de toda a cidade.

**Próxima fatia delimitada: uma casa funcional, `House_Residential_1`.** O pedido mais recente de portas/interiores será validado nessa amostra antes de sua expansão. Reutilizar a casa nativa e o portal gable v08 já preparados; nenhum novo lote de geração, padaria/moinho, composição urbana ou censo completo de colisões nesta rodada. Não creditá-la como solução da repetição arquitetônica global.

- Um executor existente integra somente o piloto; revisão independente no checkpoint integrado. Ownership exato deve ser fixado no handoff, preservando alterações dos demais owners. A orquestração não abre agentes apenas para supervisionar outros agentes.
- Aceite observável: antes/depois da mesma casa na mesma escala; fachada nítida e vão real; abertura/fechamento visíveis no jogo; exterior/interior respondendo à posição real dos pés; porta fechada bloqueando e aberta permitindo passagem sem prender o personagem. Preservar NPCs, anchors e runtime compartilhado. Falta de prova permanece pendente, não sucesso inferido.
- Uma integração coesa, seguida de compilação e verificações focadas no import, porta, acesso e interior afetados. Respeitar a janela Unity e renovar a baseline quando os inputs compartilhados mudarem. Evidência global04 válida não precisa ser refeita por mera alteração documental.
- Classificar qualquer falha como produto, instrumento de teste ou ambiente antes de corrigir. Não alterar comportamento de produção só para uma demonstração artificial funcionar; perda de frames na captura não prova defeito de animação.
- Limite de duas tentativas sem ganho observável nesta abordagem. Nesse caso, preservar e mostrar o resultado, explicar a causa e mudar o método; não iniciar uma cadeia automática de novos agentes.
- Atualizar este mesmo relatório com arquivos, tentativas, antes/depois e diferenças restantes. A captura atual e este relatório são a entrega de referência; não criar outro sistema de logs para acompanhar a economia.

Estado ao adotar o método: **planejamento da fatia registrado; implementação e validação do piloto ainda NOT RUN**. A solicitação atual foi de mudança de metodologia; não houve retomada automática dos executores nesta atualização. Portas nas demais casas, arquitetura variada, remap de móveis e colisão de todas as famílias seguem nas specs existentes, sem serem declarados concluídos.

### Rodada limitada de proporção — revision-06-scale20-r2, 2026-09-10

O pedido humano posterior substituiu a próxima fatia pelo aumento mínimo de 20% das construções, mantendo a Town em 120x90 e a câmera comparável em ortho45. Um único executor aplicou `VisualEnlargement = 1.20f` uniformemente nas 24 casas, Prefeitura e abrigo do curral, preservando footprints físicos, IDs, portas, interiores e NPCs. A primeira integração (`revision-06-scale20`) evidenciou sobreposição visual grave entre Prefeitura, Alquimia e Ferreiro; foi classificada como falha de composição, não de collider.

A segunda e última correção reposicionou somente esse agrupamento e controles locais de vias. A previsão por contornos alpha reduziu Hall–Alchemy de 72,9 para 19,5u² e Hall–Blacksmith de 66,1 para 10,9u². O primeiro gate materializado dessa correção foi rejeitado corretamente porque `npc_brumdar_work` caiu sobre `Wall_Top` (185 PASS/2 FAIL); o offset do mesmo anchor foi corrigido sem alterar novamente a composição. A geração final então passou com 24 prédios, clearance/lote/via/porta em zero falhas, `TownKeyartPhysics` **187/187 PASS** e CitySchedule **21/21 PASS**. EditMode focado: **271/271 PASS**, sem skipped, em `Logs/town_keyart_revision06_scale20_r2_anchor_editmode.xml`. PlayMode continua **NOT RUN**.

Captura final: `art/town-keyart-rework/evidence/revision-06-scale20-r2/town-comparable45.png`, SHA256 `76339BA3F10343D01A6C443FA86C3ED88EEDDEBED0033D48F81CBCD9D6F5E9E6`. Cena materializada SHA256 `20E53DE2D91F829F3E70FE0D76C94BC538774BAA2F1218D83274292B8E8684B0`. O revisor independente deu **ACCEPT somente para a rodada limitada de escala**: construções >=20%, melhor presença, três fachadas do cluster legíveis, sem corte grave e NPCs distinguíveis. A fidelidade global à keyart continua **FAIL/não demonstrada**; não foi inventado novo percentual.

Resíduos prioritários: malha ainda espaçada e ortogonal; padaria, alquimia e moinho mais suaves que a arte nativa; distrito animal pequeno e vazio. Novo upscale não é a próxima correção indicada. Portas animadas, interiores em uso e cobertura física dinâmica continuam pertencendo à spec de componentes e exigem prova PlayMode separada.
