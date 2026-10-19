# Validacao SPEC 06 - Economy shop stock pricing UI

> Data: 2026-05-24
> Status: Validacao automatizada PASS; Play Mode interativo NOT RUN

## Evidencias automatizadas

| Validacao | Resultado | Evidencia |
|---|---|---|
| Unity compile | PASS | `Logs/unity-compile-spec06-corrected.log` contem `Tundra build success` e `return code 0`. |
| Shop assets/components | PASS | `Logs/spec06-shop-validation-final.log`: `24 passed, 0 failed`. |
| Scene wiring | PASS | `Logs/spec06-scene-validation-final.log`: TownScene e FarmScene aprovadas. |
| Farm commerce legado | PASS | Validador rejeita `SellPoint`/`SeedShopPoint`; a cena gerada nao os contem. |

## Observacao do scanner

`tools/unity/ScanUnityLogs.ps1` reportou assemblies `firstpass` invalidos apos uma compilacao bem sucedida. Como nao houve `error CS` e o build registrou `Tundra build success`, o resultado de compilacao e tratado como PASS conforme o harness operacional. O alerta permanece registrado para ajuste posterior do scanner.

## PLAY MODE TEST: SPEC 06 - Economy Shop, Stock, Pricing e UI

Scene used: `TownScene` e `FarmScene`

Steps executed: NOT RUN

Expected result:

1. Pip caminha ate perto do jogador, mostra abertura/despedida e nao abre loja.
2. Cada lojista abre dialogo, menu vertical `Comprar / Vender / Sair`, buy panel e sell panel.
3. Compra valida ouro, espaco e stock; venda paga 60% do valor base.
4. Save/load preserva stock restante e novo dia repoe stock uma unica vez.
5. FarmScene nao oferece compra/venda oficial.

Observed result: NOT RUN

Bugs found: Nenhum em validacao automatizada; fluxo interativo nao exercitado.

Passed: NOT RUN

Reason: validacao executada em Unity batchmode sem entrada interativa de Play Mode.

Residual risk: input, layout visual e sequencia completa de interacao dependem de validacao humana final.
