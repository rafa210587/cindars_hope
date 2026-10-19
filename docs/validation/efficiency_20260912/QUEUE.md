# Fila de eficiência do workflow

Continuação autorizada até o fechamento: [FINAL_TRANCHE.md](FINAL_TRANCHE.md). A instalação Serena isolada foi autorizada explicitamente; o limite de não instalação abaixo descreve as entregas anteriores.

Autorização: pedido humano de 2026-09-12 para iniciar as melhorias, delegar por modelo e risco e coordenar dependências. Escopo inicial: manutenção documental, ferramentas locais e avaliação; cada piloto posterior depende dos critérios abaixo.

Evidência da primeira entrega: [REPORT.md](REPORT.md). Continuação E7/E8: [SECOND_TRANCHE.md](SECOND_TRANCHE.md). O benchmark de modelos está preparado; rodadas dos 12 casos permanecem NOT RUN. A sondagem de ferramentas E7 não equivale a esse benchmark.

| ID | Entrega | Owner/modelo | Dependência | Estado |
|---|---|---|---|---|
| E1 | Consolidar CURRENT_STATE, preservar snapshot e atualizar guia de custo | docs_efficiency / Luna | nenhuma | CONCLUÍDO |
| E2 | Resumo Git com saída limitada e testes de contrato | output_summary / Terra | nenhuma | CONCLUÍDO; 16 assertions PASS |
| E3 | Benchmark com 12 casos verificáveis e métricas honestas | benchmark_design / Terra; execução/julgamento posteriores | nenhuma | v1 NOT RUN; v2 DECISION_ONLY executado: 24 respostas, baseline 12/12 e candidato 11/12; sem métrica econômica |
| E4 | Integrar descoberta do resumo e política de output nas instruções existentes | orquestrador / modelo atual | E2 | CONCLUÍDO |
| E5 | Conferir diffs, links, contratos; gerar cópias após congelar fontes | orquestrador | E1–E4 | CONCLUÍDO no escopo; docs global FAIL |
| E6 | Revisão independente da entrega integrada e correções pertinentes | revisor / Sol | E5 | CONCLUÍDO; sem bloqueadores novos |
| E7 | Piloto de navegação semântica, comparado ao rg/Roslyn atual | semantic_navigation / Sol; revisão Astra; integração orquestrador | E3 preparado + contrato SEMANTIC_PILOT | AVALIADO; 13 casos PASS; Runtime INCOMPLETE; não adotado como padrão |
| E8 | Comparar Serena read-only, Stryker restrito e viabilidade de CI | external_feasibility / Terra; pilotos isolados posteriores | Pesquisa independente; execução depende de autorização/ambiente | AVALIAÇÃO ENCERRADA: Serena fixture PASS; Unity real NOT RUN; Stryker DEFERRED; CI metadata/imagem PASS, ativação FAIL — FINAL_TRANCHE.md |

E1–E3 possuem arquivos exclusivos. Somente o orquestrador altera roteamento, gera cópias e fecha esta fila. Os implementadores executam contratos locais; o orquestrador inspeciona artefatos e reexecuta apenas quando inputs ou riscos justificarem. Não há Unity nesta entrega sem mudança em código C#/assets.

Modelos foram escolhidos por risco e clareza, conforme a regra vigente; nenhuma economia faturada é presumida. Subagentes recebem pacotes curtos sem histórico completo. O benchmark preparado não equivale a rodadas executadas nem autoriza resultados fictícios.

Limites: preservar trabalho concorrente; sem instalação externa, push, PR ou alteração de gameplay nesta fatia. Pilotos de dependências precisam primeiro demonstrar utilidade e compatibilidade em escopo isolado. A fila posterior não é marcada como concluída pela entrega inicial.
