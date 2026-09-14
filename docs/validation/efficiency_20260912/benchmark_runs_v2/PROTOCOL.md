# Rodada de decisões v2 — congelamento

Snapshot: HEAD `c5a1a61ac21fd814a337151e7794f68ac89733c1`; checkout concorrente, não congelado globalmente. Os executores recebem exclusivamente os pacotes autocontidos congelados, sem consultar fontes mutáveis. Inventário inicial em `dirty.json`. Nenhuma alteração do código é autorizada.

Prompt source SHA-256: `AE68677179FECC28C30082D7030D5BDAFF94A617CE0C31BE5B5E415B67A6800D`.
Rubric SHA-256: `73516A4131011846C1FAB4227C84D1DA8FA4DA5C6F99C61319095223AE97748E`.

Grupos idênticos nos dois braços: G1 B01/B08/B09/B11; G2 B02/B03/B05/B10; G3 B06/B07/B12; G4 B04. Baseline sem override, modelo efetivo desconhecido. Candidatos Luna/Terra/Sol/Astra respectivamente; nomes são pedidos de seleção, não preços comprovados. Ordens de despacho por grupo: G1 baseline primeiro; G2 candidato primeiro; G3 baseline primeiro; G4 candidato primeiro. Execuções podem se sobrepor, não há isolamento de carga do host.

Mesmo comando de tarefa por grupo, sem revelar braço/modelo/gabarito. Cada executor lê somente as seções indicadas de prompts.md, sem rubric.json, README ou respostas de outro agente. Retorna decisão e status por caso, até 120 palavras por caso. Sem ferramentas de implementação. Respostas são guardadas pelo orquestrador sem revisão corretiva do executor; primeira tentativa é a rodada.

Tempo será medido por batch, incluindo delegação/leitura/resposta; não dividir por caso nem confundir espera de orquestração com latência de inferência. Sem telemetria nativa: token_usage, cache_usage, cost e modelo efetivo não confirmado ficam null/desconhecido. Rubrica binária congelada, todas as exigências por caso; respostas ausentes NOT_RUN, falhas não excluídas. Resultado testa aderência a perguntas orientadoras, não implementação, visão artística, segurança universal ou ranking de modelos. B11/B12 não são holdout cego. Não preencher resultados v1 com esta rodada.
