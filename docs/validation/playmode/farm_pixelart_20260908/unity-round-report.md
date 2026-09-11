# Validação Unity — revisão pixel art da fazenda

Escopo SCOPED da spec `spec_farm_pixelart_cohesion_and_ingame_review_v1`, executado em08/09/2026 local (09/09 UTC). Seleção pela matriz canônica: cena/geração/import com captura real; nenhuma suíte global nem compile-only redundante.

## Resultados vigentes

| Gate | Resultado | Evidência |
|---|---|---|
| Backup anterior | PASS:184 arquivos copiados e SHA256 comparados;140 hashes dePNG World | art/farm-pixelart-review/backups/20260908_230040/backup-manifest.json |
| Regeneração Farm focal | PASS, exit0 e3 mensagens de captura | [processo](after/regen-process.json), [log](after/regen-unity.log) |
| Compile Editor | PASS pela execução vigente Unity6000.5.7f1, Tundra build success | [captura final log](../farm_gameplay_review/after_real/unity.log) |
| Scan de logs | PASS; cada log executado recebeu exatamente um scan | [regen](after/log-scan.txt), [captura final](../farm_gameplay_review/after_real/log-scan.txt) |
| Captura real | PASS3/3, exit0, Main Camera640×480 ortho8.5 | [metadata](../farm_gameplay_review/after_real/capture-metadata.json), [processo](../farm_gameplay_review/after_real/process.json) |
| Cena e saves durante captura | PASS: cena idêntica, SaveInput1 desativado em memória; pasta real saves0→0 arquivos | [resultado](../farm_gameplay_review/after_real/validation-result.json) |
| Runtime errors da sessão | PASS: zero | metadata acima |
| Mistura do chão materializado | PASS: base12749, flower1451, pebble776; total14976; variantes A/Bzero | [inspeção read-only](after/ground-palette-validation.json) |
| Shipping bin em PlayMode | PASS para altura alvo:2.0u nas três vistas | metadata e resultado acima |
| PNGs World preservados | PASS140/140; somente FarmScene mudou entre184 inputs protegidos; nenhum tile/meta World criado | [delta](after/generated-delta.json) |
| PlayMode navegação/colisão/movimento e aceitação humana | NOT RUN | captura estática com teleporte; sem prova de travessia/fluidez |

A cena mudou somente na regeneração autorizada: SHA256 anteriorDCD8BED38BE0A7A07BDBF958F00A2136563AE5DE1655819D03AF83D18073036B → posterior1036E1D21A5B501218F86D1937D66D34C77D155525A2B78CFE2E4202402EDC74. A captura posterior preservou o segundo hash. Nenhum YAML foi editado manualmente.

A contagem de chão resolve o GameObject Ground, seu Tilemap, GUIDs reais da m_TileAssetArray e respectivos .asset.meta; cada m_TileIndex foi contado e comparado com m_RefCount. Essa inspeção prova os assets da cena materializada, sem presumir pesos exatos como porcentagens da amostra.

## Histórico de falhas preservado

1. [Baseline real](../farm_gameplay_review/baseline_real/validation-report.md):3/3 imagens e zero runtimeErrors, mas exit1 pelo hash amplo incluindo Unity/Editor/Analytics. Mudanças de cache foram identificadas; não houve save gameplay no inventário. O implementador restringiu a verificação ao diretório real SaveManager/saves, com escopo explícito na metadata.
2. [Primeiro after](../farm_gameplay_review/after_real_failure_empty_reason/process.json):3/3 imagens, cena/saves intactos e zero runtimeErrors, mas exit1 por comparação null de failure após domain reload. Root corrigiu para IsNullOrEmpty; repetiu-se apenas captura, sem segunda regeneração.
3. Captura final acima passou com os checks preservados. Não reclassificar os dois exits1 históricos como PASS.

## Inspeção e limites

A imagem spawn após correção foi inspecionada: personagem visível acima da caixa de envio; o quadriculado escuro do chão foi removido. A altura player medida é1.1875u, aproximadamente33.53pixels no render640×480, mantendo sprite existente76px,128PPU e escala2. Essa medida não aprova importação das novas12poses do laboratório.

Diagnósticos gerais/homestead/animais estão em after/. Capturas reais comparáveis estão em ../farm_gameplay_review/baseline_real/ e ../farm_gameplay_review/after_real/. A ferramenta documenta ausência de UI Screen Space Overlay no Camera.Render. Baselines, FAILs e comando/versionamento foram conservados.

Residual risk: câmera batch640×480 não substitui todas resoluções de destino. Captura não prova animação, circulação, física, ações ou aceitação estética da fazenda inteira. Falhas globais herdadas de CURRENT_STATE permanecem fora desta fatia. Nenhum teste humano ou PASS global foi alegado.
