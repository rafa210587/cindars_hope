# Resultado da rodada v2 — decisões

Concluídas 24 respostas em oito batches: baseline 12/12 PASS; candidato roteado 11/12 PASS, 1/12 FAIL. [Julgamento independente](JUDGMENT.md), [24 scores](scores.json), [artefatos/metadados](metadata.json), [protocolo](PROTOCOL.md).

O FAIL B03 candidato foi preservado: o plano não exigiu verificar separadamente festival oculto descoberto/não descoberto, e a fronteira de seleção proposta não explicitou a entrada de descobertas. Não houve correção posterior do participante para elevar a nota. Isso sustenta a necessidade de critérios de comportamento claros em refactors, não uma conclusão geral sobre um modelo.

## O que foi medido — e o que não foi

Esta rodada executou decisões autocontidas sobre evidência fornecida, não implementações, EditMode ou revisões visuais dos 12 casos v1. Respostas PASS não aprovam código. A rubrica foi congelada antes da execução e revisada por um agente independente que não recebeu os nomes dos modelos. Os rótulos dos braços nos arquivos não foram cegados.

Modelos solicitados: baseline herdado; candidato Luna em G1, Terra em G2, Sol em G3 e Astra em G4. O host não expôs identificação efetiva independente nem uso nativo por chamada. `token_usage`, cache e custo permanecem null. Bytes arquivados não são tokens. Não houve coleta confiável de início/fim de cada batch; `elapsed_ms=null` é uma limitação em relação ao plano, e não se compara latência. Os batches compartilham contexto dentro do grupo; perguntas orientadoras favorecem efeito teto. Não há ranking de preço, habilidade ou economia demonstrada.

## Desvios e overhead preservados

- Um despacho baseline G2 tinha caminho duplicado por erro do orquestrador. Foi interrompido e substituído por contexto novo com o caminho correto; tentativa ABORTED registrada, não atribuída ao modelo nem apagada da avaliação operacional.
- Oito turnos adicionais arquivaram literalmente as respostas já concluídas. Esse overhead não integra uma alegação de eficiência; não foi medido em tokens. Na próxima rodada, definir a escrita do artefato de saída desde o despacho evita esses turnos.
- O checkout mudou por trabalho concorrente. A entrada comparada foi o pacote autocontido com hash, não uma promessa de filesystem global imutável. Não foram consultadas fontes mutáveis pelos executores.

## Decisão

Manter o roteamento atual como hipótese operacional, com revisão/testes por risco e escalada quando necessário. Não ampliar uso de modelos menores ou remover gates com base neste ensaio. Uma comparação econômica futura precisaria de telemetria nativa, casos de trabalho realmente executados, tempos coletados corretamente e contabilização da coordenação/retries. Isso é trabalho futuro, não resultado pendente a inventar nesta rodada.
