# Primeira captura real — baseline salvo

Escopo SCOPED da spec `spec_farm_pixelart_cohesion_and_ingame_review_v1`. Unity 6000.5.7f1 executado com gráficos e sem -quit. Comando, inputs e exit em [process.json](process.json).

- Compile Editor: PASS; Tundra build success no [log](unity.log), sem erro CS. Warnings de APIs obsoletas de busca já presentes no código diagnóstico/validators/gerador.
- Scan único: PASS, exit 0, [log-scan.txt](log-scan.txt).
- Processo/capture metadata: FAIL, exit 1. Não converter este resultado histórico em PASS.
- Artefatos: 3/3 PNGs novos, spawn/bridge/cultivation em640×480, câmera real MainCamera ortho8.5.
- Runtime errors observados pela sessão: zero.
- Cena: hash antes/depois idêntico `dcd8bed38be0a7a07bdbf958f00a2136563ae5de1655819d03af83d18073036b`.
- SaveInput: um componente desativado em memória antes de PlayMode. Cena reaberta sem salvar.

O FAIL de integridade decorre de escopo excessivo do hash sobre persistentDataPath inteiro. [persistent-delta.json](persistent-delta.json) comprova mudanças exclusivamente em Unity/local.../Editor/Analytics:16 arquivos ArchivedEvents removidos e o arquivo values alterado pelo Editor. O arquivo TestResults.xml foi preservado. Nenhum arquivo de save de gameplay estava presente no inventário. Portanto não há evidência de alteração de save humano; houve alteração real em caches de telemetria. O implementador foi informado para distinguir arquivos de jogo dos arquivos de diagnóstico do engine, preservando esta rodada original.

As vistas servem para análise visual estática, sem comprovar caminhada, colisões, navegação ou qualidade das12poses do laboratório. O orquestrador identificou player oculto pelo shipping bin gigante no spawn; esse achado permanece problema de apresentação a investigar.

Regeneração NÃO executada nesta etapa; aguarda reconciliação do achado e autorização do owner. Backup anterior: art/farm-pixelart-review/backups/20260908_230040,184 arquivos copiados/hashes verificados e140 hashes dePNG World.
