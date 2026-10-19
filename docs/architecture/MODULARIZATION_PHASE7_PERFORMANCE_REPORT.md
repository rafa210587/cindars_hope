# Fase 7 — Performance medida

Data: 2026-07-05
Branch: `dev`

## Instrumentação

Foram adicionados `ProfilerMarker` nos caminhos acordados:

- `CindarsHope.EventBus.Publish`;
- `CindarsHope.NpcSchedule.ResolveAll`;
- `CindarsHope.Cave.Materialize` e `CindarsHope.Cave.Cleanup`;
- `CindarsHope.Save.CaptureSerializeWrite` e `CindarsHope.Save.Restore`;
- `CindarsHope.UI.Minimap.Render`;
- `CindarsHope.SceneTransition.Execute`.

## Otimização comprovada

`GameEventBus.Publish` deixou de executar `ToArray()` a cada dispatch. Cada tipo de evento mantém um
snapshot imutável, reconstruído somente quando subscriptions mudam. O contrato de mutation during
dispatch e o isolamento de exceções foram preservados.

O teste de alocação aquece o bucket e mede 1.000 dispatches normais com
`GC.GetAllocatedBytesForCurrentThread`: resultado 0 bytes, 1/1 PASS em
`TestResults/modularization-phase7-eventbus-allocation.xml`.

Pools, Addressables, Tilemap/chunking e scene loading assíncrono não foram aplicados sem baseline de
frame/fluxo e sem PlayMode confiável. Isso evita transformar hipóteses em regressões. O lote anterior
já removeu as queries físicas alocantes registradas pelo ratchet.

## Gates

- ratchets: 4/4 PASS;
- assemblies: 6/6, 0 warnings, 0 erros;
- suíte completa: nenhuma falha nova;
- build Windows: `Succeeded`, 0 erros, 4 warnings, executável de 535,66 MB;
- smoke do executável: processo vivo após 12 s, `Player.log` com 0 erros críticos.
