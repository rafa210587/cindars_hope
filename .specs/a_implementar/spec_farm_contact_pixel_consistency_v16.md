# /speckit.specify

# Farm v16 — contato físico, pesca e coerência de pixels

> Revisão:1 · Status:INTEGRATED_SCOPED_PASS — aceitação residual pendente
> Autorização: pedido humano para consolidar spec/plan/tasks e executar este pacote.
> Tipo: manutenção visual + interação existente · Prioridade:alta · Domínio:Farm
> Baseline:stage15 + ambient_runtime15; não representa aceitação visual global.
> Depende de:ref_farm_contact_pixel_consistency_fishing_v1 (refinamento, não bloqueio de execução).
> Bloqueia:fechamento visual da Farm. Ordem de execucao:contatos → materiais → integração.
> Executor:root orquestra; candidatos independentes; integração Unity serial com freeze de Assets.

## Spec

Objetivo: remover contatos visualmente falsos e pesca no centro do cais, harmonizando a leitura
dos pixels sem aumentar indiscriminadamente a resolução nem mudar proporções/layout aprovados.

### Estado real

Fonte possui SolidBasin4.6×1.6 e trigger separado; pivot visual central fica1.865u acima do apoio.
Lago/rios já têm sólidos. Pesca usa ring±.96u em torno(13.56,-8.75), fora da ponta y=-10.798.
Dock/boat são filhos da mesma root escalada6; movê-la deslocaria arte. A observação anterior
comprovou cinco frames na fonte/cascata e dois saltos20s, mas não contato player/prop em movimento.
Chão/estrada apresentam granulação fina enquanto construções têm sombras suaves.
Audit gameplay15 (640×480, ortho8.5): pixels-fonte por unidade / pixels-tela por pixel-fonte:
player64/.441; grass128/.221; road32/.882; house18.342/1.539; barn18/1.569;
fonte19.571/1.443; dock17.273/1.635; cascata18.3/1.543. Player medido em runtime scale2.
Grama não sofre resize do importador; a câmera minifica4.53pixels-fonte por pixel de tela.

### Requisitos e aceitação

- AC1 Contatos: nas8aproximações da fonte e4da confluência, pés não entram em pedra/bacia/água;
  circulação externa sem enrosco; fonte interagível e respawn livre. Provar com collider real e PNGs.
- AC2 Profundidade: player à frente aparece à frente; estruturas altas podem ocultar ao passar
  atrás; água/espuma não sobrepõem player em chão seco. Todas5poses registradas, sem deslocar arte.
- AC3 Pesca: interação possível na ponta navegável e recusada na entrada/centro/laterais/água.
  Preservar ID, tabelas, stamina, clima e default de outros FishingSpots; simular seleção real.
- AC4 Densidade: tabela de canvas,PPU,scale e pixels efetivos para player,ground,road,house,barn,
  fonte,dock,cascade e vegetação discrepante; comparar a mesma câmera, sem confundir zoom de preview.
- AC5 Materiais: candidatos locais em Aseprite reduzem ruído isolado do chão/estrada e alinham
  clusters/sombras com atores/construções; sem blur ou aumento de detalhe artificial; inspecionar1×.
  Integrar somente candidato visualmente melhor que baseline, preservar originais e footprints.
  Incluir amostra localizada do celeiro para corrigir gradações embutidas, sem mudar seu canvas
  125×166, alpha ou escala. Se o candidato piorar leitura/identidade, manter fonte e registrar.
- AC6 Antirregressão:0cultivos iniciais, mesmos IDs/71árvores, deck/boat imóveis, save intacto;
  fonte5frames0.7s e peixe20s preservados. PNGPoint/None/no-mips, sem novo manager de gameplay.

Fora do escopo:Town,Skills,Cave procedural,PlayerWalkAnimator,novo minigame de pesca,expansão de
mapa,resize global,regeneração completa keyart,novos sistemas de save ou geração de conteúdo.

# /speckit.plan

## Plan

### P1 — Interação opt-in

Modificar `Assets/_Game/Scripts/World/FishingSpot.cs`: proposto
`ConfigureStanceZone(BoxCollider2D zone)` e campo serializado opcional. Quando presente, usar zona
para CanInteract e rechecá-la antes de iniciar cast em Interact; sem zona conservar anel atual.
Medir pés segundo contrato existente do player, sem GameObject.Find e sem transformar trigger
em obstáculo. Creator só instancia/wira a zona específica Farm. Padrão:scene-interactable-wiring.

Modificar `CreateMvpFarmScene.CreateFishingSpot`: substituir quatro triggersFarm por zona na ponta;
manter root/IDs/visualchildren em seus transformsworld. Registrar stance/casttarget separadamente
em `FarmLevel1LayoutContract` quando necessário. Não mover todo FishingSpot.

### P2 — Contato e ordenação

Modificar `CreateMvpFarmScene.CreateFonteAnya` e `FarmSettlementPhysicsContract` somente conforme
overlay medido. Criar child FountainDepthGroup em localzero com SortingGroup World/0;
reparent apenas Visual preservando worldtransform. Flores, sólidos e respawn permanecem fora.
Atualizar lookup exato em FarmAmbientAnimationAuthoring e FarmAmbientPlaybackCapture e demais
consumidores explícitos encontrados. Preservar clips/pivots; não afrouxar guard de subasset.
Compartilhar geometria física e não-arabilidade; corrigir respawn se a base corrigida o interceptar.
Incluir medição dos pés das quatro colunas isoladas: a bacia existente não as representa.
Adicionar somente bases baixas confirmadas pelo pixel de apoio, compartilhadas com não-arabilidade.

`FarmSceneSpatialContract`/`FarmLandscapeVisualComposer`: ajustar apenas seam comprovadamente
fora da barreira. Não acrescentar collider em fish/foam. Reusar padrões sprite-scene-integration.

### P3 — Materiais

Fontes: Assets/_Game/Art/Generated/World/tiles/ground_grass_keyart_v2.png (1254²,128PPU) e ground_path_aseprite_v1.png (64²,32PPU). Candidatos
em `dev/art/aseprite/keyart-v4/contact-v16/art/`; Assets finais versionados e wiring pelo gerador,
sem sobrescrever fontes GPT. Aseprite layered; estrada permanece64²/32PPU. Grama: comparar cleanup de contraste/cluster na resolução atual com candidato314² e PPU=314/(1254/128), preservando o footprint9.796875u. Selecionar uma versão pela comparação1× sem ruído subpixel excessivo; o candidato de menor resolução não é automaticamente melhor. Atualizar apenas FarmPathPalette.TexturePath, CreateFarmGroundTexture e referência de grama em FarmPerimeterVisualComposer para os novos assets Farm-specific. Não tocar fallbackTown. Assets novos ground_path_contact_v16.png e ground_grass_contact_v16.png com importerEditor próprio se necessário. Padrão:aseprite-authoring/sprite-scene-integration.

### P4 — Evidência proporcional

`GeneratedSpriteImporter.OnPreprocessTexture`: exceções por caminho EXATO dos dois novos materiais;
grama314/(1254/128)PPU, estrada32PPU. Preservar todos os outros defaults e fontes compartilhadas.
Não basta alterar meta manualmente: o importador atual reaplica128PPU no reimport.
Celeiro opcional após review: nova `props/barn_contact_v16.png`, apenas escolha de sprite em
CreateMvpFarmScene.CreateAnimalHousing; guardar identidade/canvas e escala atuais.

Estender probe Editor existente ou helper específico compacto para contato/seleção. Um root roda
EditMode direcionado e PlayMode isolado; sem repetir41s de peixe se seus inputs não mudarem.
Testes propostos `FishingStanceZoneTests`: tipaccepted/centerrejected/legacyunchanged/directcastguard.
Preservar testes existentes `FarmForageFishingTests`, `FishingV2Tests` e contratos físicos afetados.
Output `docs/validation/farm_keyart_v4/contact_v16/`:log,XML,metadata,antes/depois eHTMLcomparativo.

Riscos:viewport/escala da preview enganam medição; evitar screenpixelratio sem câmera igual.
Spritesubassetsnão podem ter pivot alterado silenciosamente; preferir groupouversão própria.
Root escalada exige conversão local/world. Relatórios PASS anteriores não certificam estética.

# /speckit.tasks

## Tasks

- [x] T1 Audit de densidade e contato; fechar sourcefiles e estratégia sorting(AC1,2,4).
- [x] T2 Revisar consistência, completar pacote e promover à fila; autorização humana já existe.
- [x] T3 Implementar zona de pesca opt-in e testes comportamentais(AC3,6), apósT2.
- [x] T4 Corrigir sorting/contato fonte e seams confirmados(AC1,2,6), apósT2 e medição.
- [x] T5 Produzir/revisar amostra Aseprite e integrar materiais aprovados no review(AC4,5), apósT2.
- [x] T6 Uma integração serial+gates afetados+capturas(AC1–6), apósT3–5 e freeze.
- [ ] T7 Revisão independente, HTML de entrega, relatório e finish-spec; não declarar95%global.

Autorização humana cobre execução deste pacote. Escrita compartilhada aguarda freeze;
candidatos offline são permitidos. Erros não autorizam sobrescrever trabalho de terceiros.
Agentes existentes reutilizados com modelo herdado: coordenação preserva contexto especializado;
nenhuma economia de preço/tokens é presumida. Root valida integração e evidência.


## Checkpoint de execução

Código T3/T4 e materiais T5 aplicados offline; testes escritos e revisão estática concluída.
T3–T5 permanecem sem fechamento até os respectivos gates. T6 bloqueada pelo Editor Untitled
aberto sob coordenação Town; não iniciar segundo Editor nem descartar cena. T7 sem promoção.
Relatório: docs/validation/farm_keyart_v4/contact_v16/REPORT.md.

## Checkpoint final vigente

Substitui bloqueio histórico: FarmScene v16 gerada/salva;53/53EditMode PASS; PlayModeD PASS em16contatos físicos e3seleções de pesca,5frames observados. Materiais revisados em câmera equivalente;71árvores preservadas. T3–T6 execução concluída no escopo registrado; isto não marca todos os AC integralmente aprovados.
T7: HTML/relatório e revisão independente disponíveis. Promoção NO: circulação completa, interação/respawn da fonte e aceitação artística final permanecem sem prova neste pacote. Não confundir seleção de pesca com cast/recompensa. Evidência e limitações: docs/validation/farm_keyart_v4/contact_v16/REPORT.md.

T7 encontrou residual AC1: pés sobre rochedo na margem oeste (confluence_1_0125). Correção localizada de apoio físico pendente; janela Editor+Assets cedida à Skills. Ver relatório final;16pushesPASS não equivale a16posesvisualPASS.

### Follow-up autorizado: apoio do rochedo oeste

Janela Unity retomada após devolução Skills. Corrigir somente o contorno físico medido na margem oeste da confluência, preservando arte/água/deck. Executar contrato de navegação afetado e captura real comparável; revisão independente decide se os pés deixaram a pedra. Não repetir testes de pesca ou41sambient sem alteração de suas entradas.

Follow-up concluído:rochedo corrigido e revisão visual localPASS;10/10navegação e19checksPlayPASS. Substitui pendência pontualT7acima, mantém critérios residuais de circulação/respawn/aceitação global. Evidência rock_fix/.
