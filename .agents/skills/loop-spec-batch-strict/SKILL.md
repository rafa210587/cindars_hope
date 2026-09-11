---
name: loop-spec-batch-strict
description: "Executa um batch autorizado preservando critérios individuais e evidência integrada."
---

# /loop-spec-batch-strict

Executa um batch autorizado preservando critérios individuais e evidência integrada.

**Arguments:** `$ARGUMENTS` — specs/limite/escopo do batch informado pela tarefa.

## Quando usar

Batch já solicitado, com dependências e ownership definidos. Não iniciar wave futura.

## Procedimento

1. Ler CURRENT_STATE, specs do batch e a matriz canônica.
2. Uma spec por iteração, usando execute-spec-strict e report individual.
3. Resolver dependências same-wave autorizadas depth-first e voltar à spec original.
4. Executar testes/gates pertinentes por fatia; reutilizar evidência verificável sobre
   os mesmos inputs. Não repetir strict global e builds idênticos por spec.
5. Coordenar uma rodada integrada Unity/build/global no checkpoint do batch.
   Se algum gate aplicável não estiver coberto, obtê-lo antes do claim correspondente.
6. Manter plano/estado do batch quando necessário para retomada: specs resolvidas,
   dependências, resultados e próxima ação. Não reexecutar spec concluída.
7. Commits/promoção seguem autorização da sessão e /finish-spec, sem automatismo.

## Saída esperada

Por spec: critérios, report, arquivos, testes/gates/mode/exit/evidência, status e pendências.
No checkpoint: resultado integrado separado dos resultados das fatias.
Batch concluído não prova aceitação humana da wave.

## Quando parar e reportar

Dependência fundacional sem integração, NEEDS_REWORK, escopo proibido ou falha obrigatória
que impede próximo consumidor. Continuar diagnóstico/trabalho independente autorizado.
Não mascarar baseline global FAIL; não exigir humano por spec quando cenário final cobre.
