# Closeout — Dependency Cycle Reduction v3

Data: 2026-07-05
Status: `IMPLEMENTED_BUILD_VALIDATED`

## Mudança

O bloqueio de movimento causado pelo menu de plot deixou de atravessar diretamente `Player -> Farm`.
`FarmPlotMenuController` agora possui uma lease de `GameplayInputBlocker`; `PlayerController` consulta
somente o gate puro na assembly `CindarsHope.Gameplay`.

O contrato é tokenizado: cada owner libera apenas sua própria claim. Reset de sessão, dispose tardio,
dispose duplicado e leases aninhadas são cobertos por teste. A fachada legada
`FarmPlot.IsAnyActionMenuOpen` foi preservada.

## Evidências

- testes do gate: 3/3;
- integração real menu Farm→gate: 1/1;
- EditMode completa: 2.707/2.707 em `TestResults/dependency-v3-full.xml`;
- PlayMode: 2/2 em `TestResults/dependency-v3-playmode.xml`;
- seis assemblies: exit 0, 0 erros, 0 warnings;
- ratchet: PASS;
- snapshot: 1.572 arquivos, 218 edges, 48 pares mútuos, 29 tipos internal e 3 crossings.

O validador documental permanece em exit 1 exclusivamente pelas mesmas dívidas legadas registradas
no rework v2; nenhum arquivo desta spec apareceu entre as falhas.

## Não regressão

Nenhuma tecla, velocidade, regra de menu, cena, prefab, asset, save, ID, custo ou balanceamento foi
alterado. O par `Farm|Player` foi removido; não foi criado `Farm|UI` porque o contrato usa o namespace
da assembly Gameplay.
