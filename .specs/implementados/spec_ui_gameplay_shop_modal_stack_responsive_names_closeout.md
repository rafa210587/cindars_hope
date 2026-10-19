# SPEC 17F - Modal Stack Hygiene, Responsive Shop UI e Short Item Names

**Status:** Implementado completo - Play Mode humano validado em 2026-05-26
**Data de implementacao:** 2026-05-26
**Data de fechamento:** 2026-05-26
**Ordem de execucao:** 17F
**Depende de:** SPEC 17E implementada em codigo
**Bloqueia:** Nenhuma

## /speckit.specify

Eliminar `Modal type mismatch` em compra/venda e tornar os paines de shop legiveis em resolucoes menores, com nomes curtos nas linhas e detalhes separados.

## /speckit.plan

- Adicionar pop condicional da modal ativa e fechamento visual sem mutar stack.
- Corrigir `NpcShopController` para encerrar apenas o painel modal ativo.
- Adicionar formatacao curta/tooltip de itens e painel de detalhes para buy/sell.
- Atualizar o gerador da Town com scroll e layout compacto responsivo.
- Validar o contrato modal por Editor utility e registrar gates executados.

## /speckit.tasks

- [x] Registrar causa raiz e baseline.
- [x] Implementar higiene da modal stack.
- [x] Implementar nomes curtos/detalhes e layout de shop.
- [x] Implementar validator Editor de modal flow.
- [x] Executar validacoes automaticas disponiveis.
- [x] Registrar pendencias de Play Mode final.

## Evidence / Closeout

**Implementado em codigo em:** 2026-05-26 (commit `87f1f0b`)

**Causa raiz:** `BeginCloseInteraction()` escondia `BuyPanel` e `SellPanel` em sequencia independentemente do modal ativo; cada `Hide()` tentava remover seu proprio tipo do topo, gerando `Modal type mismatch` quando `Sell` estava ativo e `BuyPanel.Hide()` era chamado.

**Evidencia de codigo:**

- `Assets/_Game/Scripts/UI/Modal/ModalManager.cs` — `TryPopIfCurrent()` e `HideVisualOnly()`: pop condicional evita mismatch; `HideVisualOnly()` oculta sem mutar stack
- `Assets/_Game/Scripts/NPC/NpcShopController.cs` — fecha somente o painel do `CurrentModal`; demais ficam ocultos visualmente
- `Assets/_Game/Scripts/UI/Shop/BuyPanel.cs`, `SellPanel.cs` — usam `TryPopIfCurrent` e `HideVisualOnly` no ciclo de vida
- `Assets/_Game/Scripts/UI/Shop/BuyPanelItem.cs`, `SellPanelItem.cs` — exibem nome curto, preco e quantidade; painel de detalhes atualizado por hover/selecao
- `Assets/_Game/Scripts/UI/Shop/ItemDisplayNameFormatter.cs` — aliases completos para todos os IDs de item; truncamento a 22 chars com `...`; `GetTooltip()` para detalhes
- `Assets/_Game/Scripts/UI/Shop/ShopPanelLayoutUtility.cs` — viewport com scroll e painel de detalhes para UI legada; sizing responsivo 75% da tela (min 720, max 1100px)
- `Assets/_Game/Scripts/Editor/Validation/ValidateShopModalFlow.cs` — cobre pops condicionais e encerramento com `Buy`/`Sell` ativo

**Gates automaticos executados:**

| Gate | Resultado |
|---|---|
| dotnet build runtime | PASS (0 erros, 5 warnings legados) |
| dotnet build editor | PASS (0 erros) |
| tools/docs/validate_docs.ps1 | PASS |
| git diff --check | PASS |
| Unity compile/batchmode | NOT RUN (Unity Editor aberto; nao e bloqueador de fechamento — Play Mode validado diretamente) |

**Validacao humana:** PASS em 2026-05-26 — modal stack sem mismatch; nomes curtos nas linhas de shop; painel de detalhes por hover; layout responsivo confirmado. Buy/Back/Sell/Back/Exit sem warnings. Sem erros reportados.

**Evidencia documental:** `docs/validation/SPEC17F_SHOP_MODAL_UI_VALIDATION_20260526.md`
