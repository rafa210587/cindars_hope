# /execute-spec-strict

Executa uma spec autorizada, com critérios e evidência rigorosos.

**Arguments:** `$ARGUMENTS` — path/ID da spec; next apenas se fila atual estiver definida.

## Quando usar

Implementação de uma spec. Fila vem de CURRENT_STATE e da tarefa, não de paths históricos.

## Procedimento

1. Ler AGENTS/CLAUDE, CURRENT_STATE, spec e arquivos pertinentes; seguir spec-execution.
2. Conferir branch/diff/dirty, ownership e autorização já existente na sessão.
3. Extrair critérios de aceite, anti-regressão e dependências. Reusar sistemas existentes.
4. Resolver dependências same-wave autorizadas e retornar à spec original; não avançar
   features futuras ou escopo proibido.
5. Implementar a fatia e testes de comportamento necessários. Testes podem ser EditMode,
   PlayMode ou tooling conforme responsabilidade; não limitar tudo a EditMode.
6. Selecionar gates/testes na SPEC_VALIDATION_MATRIX_MASTER. Reusar evidência verificável;
   não executar build por fase nem pedir reexecução só porque veio de subagent.
7. Usar /validate-spec: strict global no checkpoint; `-Gates` para seleção explícita.
   Com `-ScopePath`, conferir manifesto contra escopo autorizado e diff real.
8. Criar report individual com Acceptance criteria extracted, Existing systems audit,
   Spec Compliance Matrix, Validation e Honest status rationale.
9. Atualizar docs autorizadas e revalidar inputs documentais alterados. Conferir status
   pela matriz; /finish-spec decide promoção. Não fazer commit/push sem autorização aplicável.
10. Parar após a spec, salvo batch/continuidade já solicitados.

## Saída esperada

Spec, critérios atendidos, arquivos, comportamento/testes, modo e gates, comandos/inputs,
exit codes, paths reais de evidência, status e pendências. Não citar JSON inexistente.
SCOPED_PASS não é GLOBAL_PASS, BUILD_VALIDATED não é aceitação de gameplay.

## Quando parar e reportar

Fora de escopo, dependência indispensável ou gate obrigatório falho/sem evidência.
Corrigir/diagnosticar dentro da autorização vigente; manter resultados honestos.
Não tratar criação de asset/scene ou teste Unity como proibida quando autorizada pela spec.
