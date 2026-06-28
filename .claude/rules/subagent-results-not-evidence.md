# Rule: Resultado de Subagent Não É Evidência

O texto que um subagent retorna é uma **afirmação**, não prova. Subagents de execução já falharam, neste projeto, narrando intenção sem criar arquivos e reportando sucesso enquanto omitiam desvios (ver `feedback_subagent_honesty_farm` na memória). Portanto:

**Invariante:** o loop orquestrador NUNCA reporta uma tarefa delegada como concluída com base na narração do subagent. Antes de aceitar, é obrigatório:

1. **Verificar os artefatos no disco** — `Glob`/`Grep` confirmando que cada arquivo prometido existe e contém o esperado (não confiar na lista que o agente disse ter criado).
2. **Re-rodar o truth-gate você mesmo** — `dotnet build .\Assembly-CSharp.csproj --no-restore` e `.\Assembly-CSharp-Editor.csproj` com exit 0 conferido por você (não o exit code relatado pelo agente). Vale a `validation-truth`.
3. **Conferir contra os critérios de aceite e a anti-regressão da spec** — em especial remoções pedidas, requisitos novos do usuário e alinhamento de valores; o que é fácil o agente "esquecer".

Se a verificação divergir do relatado, trate o relato como incorreto e corrija — não propague o claim do agente.

## Lado da delegação

Todo prompt de execução para subagent deve: proibir spawnar sub-agentes ("faça você mesmo"), exigir um bloco de evidência no retorno (arquivos alterados + exit codes), e instruir "se não fez X, diga explicitamente — não narre intenção". Detalhe operacional na skill `delegated-execution`.

## Enforcement

Revisional (sem hook dedicado): é disciplina do orquestrador. Complementa `validation-truth` (exit code 0 ou não passou) e `feedback_subagent_honesty_farm`.
