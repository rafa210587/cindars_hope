# /validate-spec

Obtém somente os níveis aplicáveis à spec e confere evidência já disponível.

**Arguments:** `$ARGUMENTS` — spec alvo.

## Procedimento

1. Ler spec, CURRENT_STATE e `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`.
2. Mapear critérios a testes/gates e declarar o escopo real; preservar dirty de outros owners.
3. Inspecionar evidência vigente. Reexecutar somente gates cujos inputs mudaram,
   que não tenham evidência verificável ou que respondam a preocupação nova.
4. Usar `run_strict_validation.ps1 -Gates ...` para a seleção explícita pertinente.
   Gates: corruption,docs,build,architecture,diff,quality. A matriz decide; o script
   não adivinha que essa seleção cobre a spec. Sem Gates, o modo é GLOBAL.
5. `-ScopePath` opcional informa arquivos exatos da tarefa para diff/quality.
   Conferir o manifesto contra autorização e diff; ele não concede autorização de assets.
6. Obter comportamento e integração Unity pelos runners/testes adequados.
   Resultado de Test Runner pode cobrir compile Editor; .NET não prova Unity/Player.
7. Registrar command/exit/log/XML, inputs e status específico. Docs posteriores invalidam
   somente a evidência documental pertinente. Não repetir docs fora e dentro do strict.
8. Executar o checkpoint global uma vez após integração ampla, mantendo FAIL global visível.

## Saída esperada

Execution report com gates selecionados/justificativa, modo GLOBAL/SCOPED, resultados,
níveis Unity/Player/humano, pendências e risco. Não citar JSON inexistente.

## Não fazer automaticamente

Não executar todos os níveis por extensão .md/.cs nem pedir humano por spec.
Não apresentar SCOPED_PASS como aprovação global; seguir /finish-spec para promoção.
