# Entrega inicial de eficiência do workflow

Continuação E7/E8: [SECOND_TRANCHE.md](SECOND_TRANCHE.md). Os estados abaixo descrevem a primeira entrega, não a fila posterior.

Manutenção autorizada em 2026-09-12. [Fila e dependências](QUEUE.md), [benchmark preparado](BENCHMARK.md) e [contrato do piloto semântico](SEMANTIC_PILOT.md). Esta entrega reúne contexto documental, descoberta de alterações e critérios de avaliação.

## Mudanças

- CURRENT_STATE consolidado com snapshot anterior preservado em `docs/archive/CURRENT_STATE_before_efficiency_20260912.md`. Mantém blockers, limites de validação, IDs das specs ativas e decisões FABLE vinculantes; histórico detalhado passa a ser consultado pelos links.
- Guia de custo passa a referenciar o routing vigente. Remove preços, aliases de outro provedor e TTL de cache sem evidência do host atual.
- Resumo Git com paths limitados, staged/unstaged/untracked, conflitos e renames; instruções e README orientam a descoberta limitada, seguida da inspeção dos diffs relevantes.
- Índice deixa de exigir LAST_VALIDATION_STATUS como leitura inicial; permanece referência de validação sob demanda.
- Benchmark de 12 exercícios com arquivos reais, resultados NOT RUN, comparação de todos os casos e telemetria opcional. Não houve execução comparativa de modelos; os casos são preparação para medir, não economia demonstrada.

## Coordenação

Luna: consolidação documental. Terra: resumo Git/testes e benchmark em fatias independentes. Sol: revisão integrada read-only. Orquestrador: escopo, conferência de artefatos, correções documentais, geração e validação final. Pacotes de delegação sem histórico completo e ownership exclusivo.

A revisão pediu proteção da exportação, tratamento de processos/temporários, preservação de referências omitidas na consolidação e protocolo de benchmark sem viés de selecionar apenas sucessos. Resultados anteriores válidos não foram reexecutados apenas por terem sido obtidos por outro agente.

## Evidência

| Gate | Resultado |
|---|---|
| Geração de cópias Codex | Duas execuções exit 0; 78 skills, 16 commands, 11 agents, 25 rules; zero problemas de frontmatter/cópia |
| Idempotência | 191 arquivos examinados; zero alterações na segunda geração |
| Test-CodexHarnessGeneration.ps1 | PASS, 38 assertions, exit 0 |
| Diff check dos arquivos rastreados desta entrega | PASS |
| Contratos do resumo Git | `pwsh -NoProfile -File tools/codex/Test-ChangeSummary.ps1`: PASS, 16 assertions, exit 0; conferência integrada pelo orquestrador |
| Links e JSONL do pacote | 35 links locais conferidos, zero quebrados; 12 IDs únicos e 42 referências existentes, casos NOT RUN; dois links do snapshot corrigidos para a nova localização |
| Docs global | Exit 1, 178 diagnósticos também na conferência final, nenhum apontando os arquivos desta entrega; não há GLOBAL_PASS |
| Revisão independente | Sol: sem novos bloqueadores após correções; revisão estática, distinta da execução de testes |

Unity validation: NOT RUN
Reason: manutenção exclusiva de documentação/harness/PowerShell, sem mudança em código C# ou assets.
Command attempted: nenhum; gate não aplicável segundo a matriz canônica.
Residual risk: Unity compile not validated locally; esta entrega não atualiza a evidência de gameplay.

## Estado

Entrega inicial concluída no escopo, com falha documental global registrada. Nenhuma spec anterior promovida, dependência externa instalada ou commit criado. E7/E8 são fila posterior e não foram implementados. Os dados de bytes/linhas descrevem volume de contexto; não equivalem a tokens faturados ou economia financeira.

Conferência local: CURRENT_STATE passou de 119 para 83 linhas, com histórico preservado semanticamente. Na consulta final havia 29 registros Git; `-MaxResults 3` mostrou três e declarou 26 omitidos, saída de 776 caracteres contra 1.456 na listagem completa equivalente. Esse checkout difere do snapshot da auditoria anterior que tinha milhares de registros. O limite é por quantidade de paths, não orçamento exato de tokens; JSON pode ser maior que porcelain em conjuntos pequenos. Não extrapolar essa amostra para economia de sessão ou faturamento.
