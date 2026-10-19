# Comparacao de investigacao: Serena versus shell

Decisao: **NAO APROVADO para ativacao automatica por economia de custo**.
Ambos concluiram a investigacao real; Serena aumentou tokens de entrada e latencia
sem ganho relevante de qualidade nesta amostra. A configuracao local permanece
`enabled = false`. Nao houve alteracao global nem de gameplay.

## Metodo e acesso

Um par, duas perguntas identicas, snapshot identico de 1.390 fontes C# do projeto,
modelo `gpt-5.6-sol`, reasoning `medium`, sessoes CLI independentes e efemeras.
Ordem predefinida: baseline primeiro, candidato somente apos acesso/usage validos.
Serena foi o navegador principal do candidato, com fallback shell permitido.
Nao e um ensaio de Serena puro, nem uma comparacao entre modelos.

O runner anterior ignorava a configuracao de usuario e perdia a selecao do sandbox
Windows. Foi explicitado `windows.sandbox="elevated"` em ambos os bracos, mantendo
`read-only`, `approval_policy="never"` e configuracao isolada. Antes dos modelos,
o sandbox nativo leu uma interface real e negou a criacao de um arquivo novo no
snapshot descartavel: ambos PASS em `preflight.json`. Nenhuma permissao global foi
relaxada. O baseline agora executou comandos reais; a falha anterior continua
registrada em `../agent-cost-0c097f2399/REVIEW.md`, sem substituir seus custos por zero.

## Contadores nativos do turno

| Medida | Sem Serena | Com Serena | Variacao |
| --- | ---: | ---: | ---: |
| Input tokens | 276.325 | 472.454 | +71,0% |
| Cached input tokens, incluidos no input | 230.144 | 393.728 | +71,1% |
| Input sem cache, calculado por subtracao | 46.181 | 78.726 | +70,5% |
| Output tokens | 4.999 | 4.178 | -16,4% |
| Reasoning output, reportado separadamente | 1.231 | 808 | nao somar novamente ao output |
| Tempo total, incluindo inicializacao | 111,76 s | 340,71 s | 3,05 vezes |
| Comandos shell concluidos | 31 | 3 | contagem de operacoes |
| Chamadas MCP concluidas | 0 | 16 | contagem de operacoes |

Ambos exit 0 e `turn.completed`, sem eventos `turn.failed`/`error`.
Baseline teve um comando com exit 1 por caminho incorreto, depois corrigido pelo
proprio agente. As 16 chamadas MCP registradas concluiram sem erro. O candidato
relatou tambem tentativa de `initial_instructions` indisponivel; ela nao aparece
como chamada MCP concluida, portanto nao e contabilizada como sucesso adicional.

Os numeros sao `turn.completed.usage`, nao estimativas de caracteres ou somente
respostas das ferramentas. Entrada acumulada inclui contexto reprocessado.
Cache write foi zero em ambos. Nao foram inferidos preco monetario, consumo da
cota da assinatura ou equivalencia entre tokens cached e uncached. A reducao de
821 tokens de saida nao demonstra compensacao dos 32.545 tokens adicionais de
entrada sem cache e dos 163.584 adicionais de entrada com cache.

## Revisao de qualidade contra chave independente

- Ambos: duas implementacoes gameplay corretas, ambas sealed; construcao de
  `SeededGameplayRandomSource` em `CraftingPassiveRngState.cs:89` e singleton de
  `UnityGameplayRandomSource` em `RandomSources.cs:48`; world/visual separados.
- Ambos: lista correta dos 13 preparables e dos tres executores nao preparables
  (`FarmCrop`, `FeedbackOnly`, `SelfRestore`).
- Ambos: API real `Validate`, validacao antes de criar cast e novamente antes de
  `Execute`, recusa/cancelamento em falha de validacao e cooldown apos sucesso.
  Ambos reconheceram a possivel recusa adicional `Interrupted` no cancelamento.
- Limites compartilhados: nao explicitaram a etapa opcional `TickBeforeCommit`
  do lifecycle nem detalharam a falha de `Execute`. Logo, nao e um PASS de
  completude integral da cadeia; e equivalencia dos achados principais.
- Pequena imprecisao do candidato: cita `SeededWorldRandomSource:17`, mas a
  declaracao fica em 18. Nao foi demonstrado ganho de precisao com Serena.
- Ambos limitaram a conclusao a analise estatica de `Assets`; nao houve teste
  de comportamento em Unity ou comprovacao de heranca/construcao dinamica.

## Limites e decisao operacional

Somente um par, duas questoes, ordem fixa, cache nao controlado e nenhum intervalo
estatistico. O teto solicitado de 16 chamadas era soft; as operacoes registradas
excedem 16 em ambos os bracos (wrappers/agrupamento nao equivalem necessariamente
a chamadas superiores). Nao alegar que houve teto rigido de tokens ou operacoes.
O timeout imposto foi 600 s por braco, nao atingido; nao houve repeticao seletiva.

A medicao cobre cada agente investigativo, nao o custo desta orquestracao, da
chave independente, da instalacao ou das tentativas anteriores. Nao e um total
do projeto. Tambem nao prova que Serena nunca compensa em outra tarefa.

Para esta configuracao/proxy e estes cenarios, a evidencia rejeita promover
Serena como otimizacao padrao. Manter shell/rg como padrao e Serena automatico
desabilitado; nao iniciar mais rodadas automaticamente. Uma futura reavaliacao
precisaria justificar mudanca concreta e medir o custo total novamente.

Evidencia bruta: `protocol.txt`, `preflight.json`, `summary.json`, invocacoes,
JSONL completos e stderr de ambos os bracos nesta pasta. O snapshot foi conferido
novamente apos os turnos: 1.390 fontes, zero divergencias SHA-256. Nenhum processo
Serena/proxy/LSP correspondente permaneceu no inventario final.

Unity validation: NOT RUN
Reason: investigacao estatica de navegacao/custo, sem alteracao de runtime.
Command attempted: nenhum; compilacao nao integra este ensaio.
Residual risk: comportamento e compilacao Unity nao validados por este benchmark.
