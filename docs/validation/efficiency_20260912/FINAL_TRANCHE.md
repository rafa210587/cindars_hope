# Fechamento dos experimentos pendentes

Estado: CONCLUÍDO no escopo autorizado, com limites explícitos abaixo. Autorização humana: continuar até finalizar; Serena 1.7.0 e dependências somente em ambiente isolado fora do projeto; branch e execução CI autorizadas posteriormente. Sem configuração global Codex.

## Fila e ownership

| Trabalho | Owner | Dependência | Estado |
|---|---|---|---|
| Entradas executáveis do benchmark de decisões v2 | Sol / benchmark_packets | Casos v1 insuficientemente específicos | CONCLUÍDO; prompts/rubrica congelados |
| Execuções baseline/candidato e avaliação | Orquestrador + executores; juiz Sol | Entradas/rubrica congeladas | CONCLUÍDO: 24 respostas; baseline 12/12, candidato 11/12 |
| Serena instalado e consultado de forma isolada | Sol / serena_isolated | Autorização de instalação recebida | CONCLUÍDO no fixture externo; Unity real NOT RUN |
| CI: preflight GitHub em branch isolada | Terra / ci_isolated; revisão root | Publicação autorizada; humano aceitou bloqueio de ativação | EXECUTADO; metadata/versão/imagem PASS; ativação FAIL; teste Unity NOT RUN |
| Stryker isolado | Não executado | Instalação não autorizada; fixture .NET independente exigida | DEFERRED; avaliação de viabilidade encerrada, não adotar nesta entrega |

Os agentes não compartilham arquivos de escrita. Pacotes de benchmark não contêm gabarito; rubrica fica separada. A versão v2 testa decisões sobre evidência fornecida, não a execução dos testes/alterações Unity previstos nos roteiros v1. Resultados não serão transferidos para v1 nem apresentados como economia de tokens faturados.

## CI — inspeção real

Consultas read-only com `gh api` em `rafa210587/cindars_hope`: repositório público, branch padrão `main`; Actions habilitado, `allowed_actions=all`; zero workflows e zero runners self-hosted. Autenticação do usuário disponível, sem ler valores de secrets. Checkout local não contém `.github/`.

`docker version --format '{{json .Server}}'`: exit 1; endpoint `dockerDesktopLinuxEngine` indisponível. Binário instalado não comprova daemon operacional. Não foi iniciado serviço nem baixada imagem.

Após autorização, foi publicada somente a branch `codex/efficiency-ci-pilot`, sem PR/merge. Dois commits alteram exclusivamente `.github/workflows/efficiency-pilot.yml`: `72ab0dceddbbd99bd44183c84439a87030f0c039` e `8d6f0d2c707e9e400f0d912e6991f64272fb23b5`. Nenhum secret foi criado.

[Primeira execução](https://github.com/rafa210587/cindars_hope/actions/runs/34724853766): FAIL no cleanup do checkout por gitlink existente sem `.gitmodules`; versão/imagem SKIPPED; ativação FAIL. Inspeção do HEAD confirmou um gitlink em `.claude/worktrees/confident-fermi-2e43ee` e ausência de `.gitmodules`. O piloto não removeu esse conteúdo. Isso permanece um bloqueio para checkout convencional de CI, não uma falha corrigida no projeto.

[Segunda execução](https://github.com/rafa210587/cindars_hope/actions/runs/34724928703): leitura de `ProjectVersion.txt` pela API no SHA exato, comparação Unity 6000.5.7f1 e `docker manifest inspect unityci/editor:ubuntu-6000.5.7f1-base-3.2.2` PASS. Não houve pull da imagem nem checkout/build do projeto. Somente ativação FAIL por ausência de mecanismo aprovado; conclusão GitHub é FAILURE, não PASS global. EditMode: NOT RUN. O humano autorizou encerrar com esse bloqueio documentado.

Preparação local: uma tentativa de checkout completo demorou e sua remoção forçada foi parcial. O processo foi encerrado antes da retomada e a pasta residual foi preservada. O segundo worktree usou `--no-checkout` e índice HEAD, com staged diff de um único workflow; não incorporou alterações sujas do principal. Os caminhos externos `D:\Projetos\Cindars_Hope\cindars_hope_ci_pilot` (resíduo sem registro) e `D:\Projetos\Cindars_Hope\cindars_hope_ci_pilot_index_20260912` (worktree da branch) foram retidos. Conteúdo da cópia inicial é recuperável do commit; não foi removido conteúdo do checkout principal. Não há autorização de limpeza/deleção das branches no fechamento.

## Limites

Resultados: [benchmark executado](benchmark_runs_v2/REPORT.md), [Serena](serena/REPORT.md). O piloto Serena comprovou consultas entre dois projetos externos, partial, overload e alias. `XML cref` é referência válida da API, não defeito. Não houve integração no Codex nem validação do projeto Unity real. Ambiente temporário retido em `C:\Users\Rafa\AppData\Local\Temp\cindars-serena-20260912-01`; servidores encerrados. JSON bruto das consultas foi preservado em `serena/raw-initial.jsonl` e `serena/raw-cross-project.jsonl`.

Autorização adicional registrada: publicar branch e executar preflight no GitHub. Humano respondeu que não fornecerá ativação Unity nesta entrega e pediu finalizar com bloqueio documentado. Somente a branch/workflow isolados foram publicados.

Não há autorização para editar gameplay, abrir PR, fazer merge, configurar o Codex global ou instalar Stryker. Push e execução estão autorizados somente para o workflow da branch isolada de CI. Custos/telemetria de modelos permanecem indisponíveis; não serão estimados por bytes.

## Decisão final

Conferência integrada: [relatório CI](ci/REPORT.md), respostas e consultas brutas inspecionadas; oito respostas arquivadas com hashes intactos, 24 scores válidos, links locais sem quebras. `tools/docs/validate_docs.ps1`: exit 1, 178 diagnósticos globais, nenhum desta entrega; não há GLOBAL_PASS. Diff check pertinente PASS; índice do checkout principal permaneceu sem alterações staged. Nenhuma evidência de Unity anterior foi promovida pelos testes de tooling ou pelo preflight.

Manter navegação simples e roteamento atual com gates por risco. Serena permanece instalado apenas no laboratório, sem adoção global: consultas de fixture passaram, compatibilidade Unity real não foi testada. Stryker fica adiado. CI demonstrou infraestrutura de preflight, não execução Unity; antes de produção, tratar separadamente gitlink inválido e ativação. Não ampliar gates, bibliotecas ou modelos obrigatórios com base nesta amostra.

O benchmark teve falha candidata e overhead do próprio orquestrador registrados, sem tentativa de transformar respostas curtas em economia comprovada. A skill de execução delegada/closeout levou à inspeção das respostas e logs reais, à revisão independente e à preservação dos FAIL/NOT RUN. Economia de tokens/tempo/custo total: NÃO DEMONSTRADA.
