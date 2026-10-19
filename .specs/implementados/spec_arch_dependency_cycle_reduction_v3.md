# /speckit.specify — Dependency Cycle Reduction v3

Status: `IMPLEMENTED_BUILD_VALIDATED`

## Ordem de execucao

ARCH.RUNTIME.V3

## Depende de

- `.specs/implementados/spec_arch_runtime_maintainability_rework_v2.md`
- `docs/architecture/MODULARIZATION_REWORK_V2_CLOSEOUT.md`

## Bloqueia

- novos acessos diretos de Player a tipos concretos de Farm para bloquear input.

required_adrs: []
required_game_rules: [input_rules, event_rules]

## Objetivo

Eliminar o par mútuo `Farm|Player` do mapa de dependências sem mudar movimento, menu de plot,
teclas, cenas ou save. O estado compartilhado será um gate puro, tokenizado e hospedado na assembly
`CindarsHope.Gameplay`.

## Baseline

- dependency snapshot: 1.570 arquivos, 216 edges, 49 pares mútuos;
- `Farm -> Player`: `FarmTillingInputController`;
- `Player -> Farm`: `PlayerController` consulta `FarmPlot.IsAnyActionMenuOpen`;
- EditMode: 2.703/2.703; PlayMode: 2/2;
- RuntimeInitialize: 62; singleton: 55; direct input: 207.

## Não regressão

- menu da Farm continua bloqueando movimento enquanto aberto;
- leases aninhados e dispose duplicado não liberam gates de outros owners;
- `FarmPlot.IsAnyActionMenuOpen` permanece como fachada compatível;
- nenhuma cena, prefab, asset, ID, quantidade, custo ou save é alterado;
- mudanças paralelas do worktree ficam fora dos commits.

# /speckit.plan

1. criar contrato puro tokenizado;
2. integrar menu Farm e Player;
3. testar nested/dispose/reset e comportamento de movimento;
4. re-medir o grafo e exigir 48 pares mútuos;
5. executar gates completos e documentar.

# /speckit.tasks

- [x] gate puro implementado e coberto;
- [x] ciclo `Farm|Player` removido;
- [x] builds, EditMode, PlayMode e ratchet verdes;
- [x] snapshot e handoff atualizados;
- [x] spec promovida para implementados.

## Resultado

- EditMode 2.707/2.707; PlayMode 2/2;
- seis assemblies 0 erros/0 warnings;
- ratchet PASS, sem aumento de dívida monitorada;
- snapshot: 1.572 arquivos, 218 edges e 48 pares mútuos;
- `PlayerController` não referencia mais `CindarsHope.Farm`;
- menu Farm adquire/libera `GameplayInputBlocker` por lease idempotente.
