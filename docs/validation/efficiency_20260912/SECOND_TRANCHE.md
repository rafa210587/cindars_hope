# E7/E8 — continuação da avaliação de eficiência

Estado: avaliação E7/E8 concluída; protótipo semântico PARTIAL para produção e não adotado. Manutenção autorizada na sessão de 2026-09-12; branch `dev`, HEAD observado `c5a1a61ac21fd814a337151e7794f68ac89733c1`, checkout com alterações concorrentes preservadas. [Fila](QUEUE.md).

## Coordenação e dependências

Sol implementa somente o piloto isolado e seus contratos; Terra avalia ferramentas externas por fontes primárias. Pesquisa externa independe da implementação. Comparação prática, decisão de adoção e fechamento dependem da evidência E7. Orquestrador integra documentação, verifica resultados e mantém instalações externas fora deste escopo.

## Sondagem das ferramentas existentes

Sondagem descritiva no mesmo host, incluindo inicialização do processo. Não é benchmark pareado de modelos nem substitui os 12 casos de [BENCHMARK.md](BENCHMARK.md). Os comandos respondem perguntas diferentes; não se pode atribuir superioridade geral por tempo ou bytes.

| Comando | Tempo de parede | Saída UTF-8, sem newline final | Resultado |
|---|---|---|---|
| `rg -n 'StableHash32\.Compute' Assets/_Game/Scripts -g '*.cs'` | 42,0 / 27,9 / 29,8 ms, três processos | 616 bytes / 5 linhas | Quatro invocações e um XML cref; busca textual não distingue semântica. Exit 0. |
| `pwsh -NoProfile -File tools/architecture/Get-ClassContext.ps1 -Type StableHash32 -Module Foundation -Members` | 617,2 ms, um processo | 739 bytes / uma linha JSON | Declaração e assinatura, sem callers por contrato. Exit 0. |
| `pwsh -NoProfile -File tools/architecture/semantic-pilot/Get-SemanticReferences.ps1 -Project CindarsHope.Runtime.csproj -Symbol 'M:CindarsHope.Foundation.StableHash32.Compute(System.String)' -MaxResults 20` | 8571,1 ms, um processo após correções | 3131 bytes / uma linha JSON | Quatro invocações, zero matches omitidos; INCOMPLETE, exit 2. |

Referências textuais observadas: `EnemyActionExecution.cs:141,143`, `EliteAffix.cs:108`, `CraftingPassiveRngState.cs:88`; `EliteAffix.cs:82` é documentação. O hash de fonte retornado pelo navegador existente foi `c2d9029cf8440c4fdfdc2b3f0bd5c550f536e1378e48a10a675672f7636c5951` para StableHash32. A ordem da saída paralela de rg variou, não seu conteúdo.

## Validação e limites

[Relatório do piloto](semantic/REPORT.md): 13 query cases PASS, exit 0, conferidos pelo orquestrador com `pwsh -NoProfile -File tools/architecture/semantic-pilot/Test-SemanticPilot.ps1` após congelar correções. Casos incluem posições/kinds exatos, overloads, alias/partial/extension, tipos de retorno/propriedades, tipo qualificado, truncagem, hash determinístico, erros e alvos não suportados. Não é cobertura universal de C#.

Probe final: hash agregado `582a3990807ca2b7fb96f5ea5a26cfee405b560d257f1a7a7f4b07e3f4c8b9de`; grafo Foundation/Gameplay/Runtime, 141 defines em cada projeto. Doze diagnósticos de constructs não suportados (Import, None, ProjectCapability mutation, analyzers/generators), nenhum omitido. Quatro invocações conferidas, XML cref excluído. Esse hash cobre scripts, Roslyn e inputs declarados; não constitui prova de compilação Unity.

Revisão Astra: quatro findings CLOSED após delta-review; revisão estática e de assertions, não execução de testes. [Viabilidade externa](EXTERNAL_FEASIBILITY.md) concluída como pesquisa, sem instalação ou execução dos candidatos. Docs: `pwsh -NoProfile -File tools/docs/validate_docs.ps1`, exit 1, 178 diagnósticos globais, nenhum apontando esta entrega. A primeira rodada apontou oito omissões de schema na spec nova, corrigidas antes da rodada final. Treze links locais do pacote conferidos, zero quebrados. Não há GLOBAL_PASS.

Unity validation: NOT RUN. Reason: piloto de tooling fora de Assets, sem impacto em runtime/Unity; gate não aplicável à entrega segundo a matriz. Command attempted: nenhum. Residual risk: Unity compile not validated locally.

`token_usage`, `cache_usage` e `cost`: indisponíveis nesta sondagem. Bytes e tempo não são tokens nem economia faturada. Não há recomendação de substituir rg por busca semântica em toda tarefa.

## Decisão de adoção

Não promover o navegador semântico próprio a ferramenta padrão. A primeira sondagem Runtime encontrou quatro invocações corretas, porém levou 8,6 segundos e retornou `complete=false` por analyzers/source generators não executados. Manter `rg` para descoberta textual e `Get-ClassContext.ps1` para declarações. O protótipo permanece isolado como laboratório de contratos, sem novo roteamento obrigatório ou cache.

Serena é candidato a um piloto separado, condicionado à autorização de instalação isolada e verificação de seus efeitos/configuração. GameCI depende de runner, imagem e ativação Unity aprovados. Stryker fica adiado até existir um teste .NET independente executável; não migrar testes Unity só para acomodar a ferramenta. Pesquisa concluída não significa compatibilidade, licença ou execução validadas. Fontes e pins estão em [EXTERNAL_FEASIBILITY.md](EXTERNAL_FEASIBILITY.md).

## Efeito da revisão

Sol implementou e Astra revisou o pequeno contrato semântico; o custo dessa coordenação não foi medido. A revisão identificou configurações ignoradas, tipos legítimos descartados, membros não cobertos aceitos e hash sem identidade do Roslyn. Todos os quatro findings foram corrigidos e revisados, com regressões focadas. Esta experiência reforça a necessidade de testar resultados negativos e completude, não apenas contar acertos numa consulta conhecida.

## Closeout

Sem promoção física da spec: produção PARTIAL, escopo experimental mantido. Nenhum gameplay, csproj real, package, configuração Unity ou navegador existente alterado por esta tranche; nenhum commit, push ou PR. A skill de closeout orientou reusar a revisão pertinente, executar contratos finais e manter incompletude e falhas globais visíveis.
