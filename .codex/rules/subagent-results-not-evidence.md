# Rule: Resultado de Subagent Não É Evidência

**Invariante: o resumo de um subagent é uma afirmação; o orquestrador confere artefatos,
diff e evidência antes de aceitar o resultado.**

## Conferência obrigatória

1. Confirmar arquivos prometidos no disco, conteúdo e respeito ao escopo/dirty preexistente.
2. Conferir critérios de aceite e anti-regressão, incluindo remoções e integrações.
3. Inspecionar a evidência real: comando, versão/configuração, exit code, log/XML,
   casos executados e inputs relevantes. Verificar que ela corresponde ao estado entregue.
4. Reexecutar somente quando faltar evidência verificável, inputs mudarem, houver
   integração ainda não validada ou preocupação nova. Delegar não invalida um teste.
5. Divergência do relato deve ser corrigida/reportada; não propagar claim incorreto.

## Delegação

Definir ownership, restrições e critérios concretos. Retorno deve listar arquivos,
comandos e resultados, artefatos e omissões. Não exigir runtime/editor build para
docs-only/harness nem build após cada fase. Usar a matriz canônica de validação.
Coordenação evita workers e orquestrador lançando Unity para o mesmo projeto.

## Enforcement

Revisão do orquestrador, `validation-truth` e skill `delegated-execution`.
Nenhum cache opaco ou relato sem artefatos substitui a conferência.
