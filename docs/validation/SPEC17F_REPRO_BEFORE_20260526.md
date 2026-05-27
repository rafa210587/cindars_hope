# SPEC 17F - Repro Before - 2026-05-26

## Erro observado no Play Mode

Ao encerrar interacao com `Sell` ativo, o console registra:

```text
Modal type mismatch: expected Buy, but top is Sell
```

## Diagnostico no codigo

- `NpcShopController.BeginCloseInteraction()` chama `_buyPanel.Hide()` e `_sellPanel.Hide()` sempre.
- `BuyPanel.Hide()` sempre chama `TryPopModal(Buy)`.
- `SellPanel.Hide()` sempre chama `TryPopModal(Sell)`.
- Com `Sell` no topo, a chamada anterior a `BuyPanel.Hide()` tenta remover o tipo errado e produz o warning.

## Baseline de UI

- `ShopCanvas` ja usa `CanvasScaler.ScaleWithScreenSize` em `1280x720`.
- `BuyPanelItem` e `SellPanelItem` concatenam descricao no texto de nome, ampliando cada row e prejudicando legibilidade.
- O gerador da `TownScene` usa `VerticalLayoutGroup` sem `ScrollRect` nem painel separado de detalhes.
