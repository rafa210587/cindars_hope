# Farm v16 — pacote de contato e coerência de pixels

## Escopo e estado

Spec/plan/tasks: `.specs/a_implementar/spec_farm_contact_pixel_consistency_v16.md`.
Execução autorizada. Código e materiais aplicados offline ao projeto; integração da cena Unity
permanece bloqueada pelo Editor aberto em Untitled, sob coordenação da tarefa Town.
Nenhuma captura stage16 existe. Stage15 continua sendo a última cena comprovada.

## Auditoria e decisões

- Fonte: referência de profundidade deslocada1.865u e quatro pés de colunas ausentes da bacia
  central. Agrupar somente o renderer da fonte no apoio e adicionar bases baixas medidas.
- Pesca: anel atual não alcança a ponta. Zona opt-in com coordenadas dos pés, corpo em escala
  runtime e margem0.12u; preservar root/dock/boat/IDs e default de outros FishingSpots.
- Resolução efetiva: grass128px/u, estrada32, player64(runtime), props17–20. Grass314² em
  32.05103668PPU conserva9.796875u e diminui minificação; não redimensionar player/props globalmente.
- Importador reaplica128PPU: duas exceções por caminho exato de novos materiais Farm são necessárias.
- Road64²: cleanup de clusters; grama314²: resample curado e áreas de menor contraste. Candidatos
  aprovados para integração pela comparação offline, não certificados em jogo.
- Barn125×166: candidato limitado a248pixels do frontão; ganho pouco perceptível. NÃO selecionado
  para integração. Fonte original preservada; não há alegação de harmonização completa dos prédios.

## Evidência offline

- `dev/art/aseprite/keyart-v4/contact-v16/pixel-audit.md`: dimensões, transformações, câmera e limites.
- `art/manifest.json` no mesmo diretório: fontes/hash, dimensões, alpha, bordas e PPU.
- `art/preview.html`: comparação simulada na densidade da câmera; NÃO captura Unity.
- Aseprite em camadas reaberto pelo executor; root examinou board de materiais e celeiro.
- Revisão independente: runtime opt-in preserva legacy; importador restrito; aplicação deve ocorrer
  código→materiais para não sobrescrever CreateMvpFarmScene. Teste de guard precisa observar eventbus.

## Gates

Unity validation: NOT RUN
Reason: Editor aberto em Untitled; estado não salvo não verificável pela conexão disponível.
Command attempted: nenhum batch Farm iniciado; impedimento identificado pela tarefa Town.
Residual risk: Unity compile not validated locally; cena/contatos/pesca ainda não validados com v16.

Próxima execução após liberação: regenerar Farm, EditMode direcionado FishingStanceZone/FishingV2/
FarmForageFishing e contratos afetados; modo de captura de contatos isolado; comparar1× e antes/depois.
Não repetir41s do peixe com inputs equivalentes. Sem promoção de spec/global PASS.

## Aplicação realizada

- Aplicador de código com hashes:10candidatos integrados; backup em
  `contact-v16/code/backup-20260910T184645564831Z`.
- Materiais integrados depois do código, preservando ambos os edits em CreateMvpFarmScene;
  hashes em `contact-v16/material-integration-backup/hashes.json`.
- Fonte: SortingGroup dedicado contém só Visual;4bases medidas e máscara compartilhada.
- Pesca: zona opcional, legacy preservado, guard antes do novo cast. Seis testes preparados;
  o teste de bloqueio observa PlayerActionFeedbackEvent e faz unsubscribe em finally.
- `CINDARS_FARM_CONTACT=1` usa sessão isolada para pushes/controlador físico e seleção; não
  produz falso atestado de cast/recompensa. Cast real permanece cenário de validação pendente.
- Grama314×314 e estrada64×64 adicionadas com novos nomes; originais e Town preservados.
- `git diff --check` direcionado: exit0. Revisão estática independente sem blocker após ajuste
  do teste. Compilação/EditMode/PlayMode permanecem NOT RUN, não inferidos deste check.
- Não houve regeneração, edição manual de cena/YAML nem alegação de stage16 entregue.


## Evidência Unity atual — substitui o bloqueio histórico acima

- Geração: PASS (generate.log); FarmScene salva em v16, imagens reais em ../stage16/.
- EditMode: 53/53 PASS (editmode.xml/log): pesca e contratos Farm afetados.
- 71 árvores: snapshot de nomes e transformações preservado; trees.json SHA256 487BF17A4A6103C6A0B1180EC03BFE0A0D743ED72B337C53CF6661C244A7F7FD.
- Revisão independente de materiais: ganho parcial na grama/estrada também em 640x480. Tufos repetidos e bordas duras permanecem; não há alegação de uniformidade artística completa.
- Tentativas de contato A/B falharam na preparação da posição. C comprovou oito aproximações da fonte e colunas NW/NE, mas a abordagem sul da coluna SW foi bloqueada pelo barril existente. Nenhuma dessas execuções certifica todo o fluxo; os logs/metadata são preservados.
- Sem novos erros runtime observados em C; hashes da cena antes/depois iguais. Instrumentação em revisão para abordar a coluna por um lado acessível.

## Ajustes adicionais encontrados e decisão

- Construções e bancas mantêm gradientes suaves já presentes nos pixels de origem; Point/None não remove esse acabamento. Não aumentar resolução nem escala para tentar corrigi-lo. A amostra do celeiro não justificou substituir a fonte aprovada.
- Vegetação: tufos e alguns acentos repetem padrões perceptíveis; próximo trabalho artístico deve variar poucos clusters, comparando na câmera de gameplay, sem refazer a composição inteira.
- Estrada: bordas ainda têm segmentos muito retos. Cleanup desta entrega reduz ruído interno; não equivale a redesenho de todas as transições.
- Fonte: revisão independente dos PNGs C5/6/7 aprovou ordenação frontal, sem água sobrepondo corpo em chão seco. Aproximações individuais NW/NE também coerentes.
- Eficiência: falhas do probe foram de preparação, não evidência de atravessamento. Preservar logs, corrigir setup por geometria e evitar regenerar arte/cena ou repetir os53testes por alteração exclusiva do helper Editor.

## Resultado final automatizado — 2026-09-10

RunD: PASS, exit0 (contact-d.log; gameplay_d/capture-metadata.json).
16 pushes reais bloqueados sem penetração:8bacia,4colunas,4confluência.
Seleção real: ponta=True, centro=False, entrada=False. Corpo chegou à ponta pelo deck sem overlap.
Cinco sprites nativos da fonte observados;0runtimeErrors; hashes da cena e saves preservados.
As falhas A/B/C ficam como histórico de instrumentação, substituídas por D para estes checks.
Galeria: index.html. Comparação geral: ../progress-review.html (stage16 vs keyart e stage15).

Limites: este probe não certifica input, cast/recompensa, toda circulação, interação/respawn da fonte ou aceitação artística humana. Não há95%global. A implementação está integrada e os gates direcionados passaram; promoção final depende dos critérios residuais, sem repetir os gates já válidos.
Lock Editor+Assets devolvido explicitamente para Skills após exit0; próximos edits desta tarefa são somente documentação.

## Revisão visual final independente

Pesca na ponta: PASS visual (fishing_tip_0152), pés sobre tábuas. SW/SE: oclusão coerente atrás das colunas. Confluência0/2/3 sem água/espuma sobre o ator.
Residual concreto: confluence_1_0125 mostra pés sobre a silhueta cinza de um rochedo da margem oeste, embora a barreira do rio bloqueie sem penetração física. AC1 não está integralmente resolvido visualmente nesse trecho. Próxima correção limitada: medir apoio do rochedo e ajustar sólido de margem correspondente pelo contrato compartilhado; não mover a água inteira. Requires próxima janela Editor+Assets, atualmente cedida à tarefa Skills. Nenhuma alegação de contato visual global PASS.

## Follow-up do rochedo — resolvido na pose observada

Substitui a pendência local acima. Vértice físico(26.70,-4.45) acrescentado apenas na margem oeste; raster abaixo da ponte acompanha a geometria. Arte/água/deck preservados. Teste novo bloqueia o ponto observado e mantém passagem vizinha.
Geração exit0;10/10FarmSceneNavigationContractTests PASS; PlayMode19checks PASS (16pushes+3seleções),0runtimeErrors. Logs/XML/metadata/source-hashes em rock_fix/. Snapshot das71árvores mantémSHA anterior.
Review independente de confluence_1_0125: pés antes26.9577,-4.4387 sobre tampo cinza; agora26.8261,-3.9214 na faixa de terra a oeste das pedras, sem tocar água. Finding local resolvido nessa pose; não equivale a varrer toda a margem.
Galeria atualizada com antes/depois e capturas finais. Critérios de circulação completa, interação/respawn e aceitação artística global continuam separados e sem alegação dePASS.
