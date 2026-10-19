# Relatório da Fase 4 — Composition Root incremental

Data: 2026-07-05

Status: `COMPLETE_WITH_PLAYMODE_RUNNER_BLOCKED`

## Mudança

Foi criado `GameRuntimeCompositionRoot`, persistente e idempotente, com estado explícito
`NotInstalled`, `Installing` e `Ready`. O primeiro serviço migrado foi `CombatStateTracker`:

- o atributo de auto-bootstrap saiu de `CombatStateTrackerBootstrap`;
- `Install()` passou a ser uma operação idempotente chamada pelo composition root;
- o total de `RuntimeInitializeOnLoadMethod` permaneceu 63;
- o singleton adicional é somente o composition root central e está documentado no ratchet.

O root normaliza referências Unity destruídas, permite recriação limpa e não usa busca global.

## Evidência

- Unity compile: exit 0;
- EditMode direcionado: 2/2 PASS;
- architecture ratchet: PASS;
- nenhuma alteração de cena, save, conteúdo ou balanceamento.

## PlayMode

Foram tentadas execuções PlayMode com teste na assembly Editor predefinida, com assembly de teste
isolada e, depois, com referência ao runtime explícito. Em todos os casos o Unity 6000.4.7f1 criou
uma `InitTestScene`, entrou no jogo, não iniciou o caso de teste e não gerou XML. Os processos de
validação foram encerrados isoladamente e os assets temporários foram removidos.

Esse bloqueio é do runner PlayMode em batch desta workspace; não foi convertido em PASS. O teste
permanece versionado para nova execução após o fechamento da infraestrutura de assemblies.
