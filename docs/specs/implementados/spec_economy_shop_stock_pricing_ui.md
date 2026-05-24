# SPEC - Economy shop stock, pricing e UI

> Spec ID: spec_economy_shop_stock_pricing_ui
> Status: Implementado completo
> Ordem de execucao: 06
> Data de fechamento: 2026-05-24
> Origem: `docs/specs/a_implementar/spec_economy_shop_stock_pricing_ui.md`
> Refinement: `docs/refinements/implementados/ref_economy_shop_stock_pricing_ui.md`

## Estado implementado

- `ShopDataSO` define lojista, multiplicadores `BuyPriceMultiplier = 1.0` e `SellPriceMultiplier = 0.6`, stock diario e itens finitos.
- `ShopManager` executa compra/venda atomica, precificacao, stock restante, restock por `DayStartedEvent` e captura/restaura `ShopStockSaveData`.
- `DialogueModal`, `ShopMenuModal`, `BuyPanel`, `SellPanel` e `ModalManager` implementam a conversa e loja sem sobrepor modais interativos.
- `NpcShopController` opera Pip sem loja e dois lojistas especializados com falas de abertura/despedida.
- `PipReceptionController` move Pip ate proximo do jogador na entrada da cidade.
- `TownScene` contem os dois lojistas, Pip, UI de shop e managers necessarios.
- `FarmScene` nao contem mais `SellPoint` nem `SeedShopPoint` como fluxos oficiais concorrentes.
- Itens e shops de conteudo foram materializados via Editor script idempotente em `Assets/_Game/Data/`.

## Regras confirmadas

- Compra valida stock, ouro e espaco antes de modificar o estado.
- Venda remove item somente quando a transacao e valida e paga `floor(BaseValue * 0.6)`, com minimo 1 para valor positivo.
- Stock persistido usa apenas `ShopId`, `ItemId`, `CurrentStock` e `LastRestockDay`.
- Restock e idempotente por loja/dia.
- `Esc` encerra conversa de shop exibindo a despedida.

## Evidencia principal

```text
Assets/_Game/Scripts/Economy/ShopManager.cs
Assets/_Game/Scripts/Economy/ShopDataSO.cs
Assets/_Game/Scripts/NPC/NpcShopController.cs
Assets/_Game/Scripts/NPC/PipReceptionController.cs
Assets/_Game/Scripts/UI/Dialogue/DialogueModal.cs
Assets/_Game/Scripts/UI/Modal/ModalManager.cs
Assets/_Game/Scripts/UI/Shop/*.cs
Assets/_Game/Scenes/TownScene.unity
Assets/_Game/Scenes/FarmScene.unity
Assets/_Game/Data/Economy/*.asset
Assets/_Game/Data/NPCs/Npc_*.asset
```

## Validacao

- Unity compile: PASS por evidencia `Tundra build success` e encerramento `return code 0` em `Logs/unity-compile-spec06-corrected.log`.
- Shop asset/component validation: PASS, `24 passed, 0 failed` em `Logs/spec06-shop-validation-final.log`.
- Scene wiring validation: PASS para TownScene e FarmScene em `Logs/spec06-scene-validation-final.log`.
- Docs validation: executada no fechamento documental.
- Play Mode interativo: NOT RUN; validacao humana final permanece necessaria.

## Risco residual

`tools/unity/ScanUnityLogs.ps1` sinaliza assemblies `Assembly-CSharp-Editor-firstpass.dll` e `Assembly-CSharp-firstpass.dll` invalidos mesmo quando o build registra sucesso. O fechamento usa o criterio operacional existente de `Tundra build success`, mantendo o alerta registrado para hardening futuro do scanner.
