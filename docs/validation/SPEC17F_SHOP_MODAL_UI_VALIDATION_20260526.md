# SPEC 17F - Shop Modal UI Validation - 2026-05-26

## Status

Implementado em codigo. Compile Unity e fluxos Play Mode permanecem pendentes.

## Causa raiz

- `NpcShopController.BeginCloseInteraction()` chamava `BuyPanel.Hide()` e `SellPanel.Hide()` sem considerar `ModalManager.CurrentModal`.
- `BuyPanel.Hide()` e `SellPanel.Hide()` tentavam remover seu tipo da stack mesmo quando outro painel era o ativo.
- A row de item incorporava a descricao no nome; a cena legada usava lista sem viewport rolavel e sem detalhe separado.

## Correcoes implementadas

- `ModalManager.TryPopIfCurrent(ModalType)` evita warnings ao remover somente a modal ativa.
- `BuyPanel`, `SellPanel` e `ShopMenuModal` expoem `HideVisualOnly()`; inicializacao nao altera mais a stack.
- `NpcShopController` encerra explicitamente `Buy`, `Sell` ou `ShopMenu` conforme o topo atual.
- `ItemDisplayNameFormatter` resolve aliases/fallback e limita o texto da row a 22 caracteres.
- `BuyPanelItem` e `SellPanelItem` exibem nome curto e atualizam detalhes ao hover/selecao.
- `ShopPanelLayoutUtility` acrescenta scroll e detalhe em runtime quando a cena ainda possui layout legado.
- `CreateMvpTownScene` passa a gerar modais de 620x560 com `ScrollRect`, detalhe separado, feedback fixo e rows compactas.
- `ValidateShopModalFlow` foi criado para validar pop condicional e encerramento dos dois tipos de transacao.

## Gates automaticos executados

| Gate | Resultado | Evidencia |
|---|---|---|
| Revisao estatica modal | PASS | Fechamento usa `CurrentModal`, `HideVisualOnly` e `TryPopIfCurrent`; nao ha chamada cega aos dois pops. |
| Busca runtime proibida no recorte | PASS | Nenhuma busca global nova nos arquivos alterados. |
| Diff whitespace validation | PASS | `git diff --check`. |
| Docs validation | PASS | `.\tools\docs\validate_docs.ps1`. |
| Runtime compile fallback | BLOCKED | `dotnet build .\Assembly-CSharp.csproj --no-restore` falhou antes da compilacao: acesso negado a `Temp\obj\...\CoreCompileInputs.cache`. |
| Editor compile fallback | BLOCKED | `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore` encontrou o mesmo bloqueio de cache. |
| Commit local | BLOCKED | `git add` falhou: `Unable to create '.git/index.lock': Permission denied`. |

## Gates Unity pendentes

| Gate | Status |
|---|---|
| Unity compile/batchmode | NOT RUN: tentativa abortada por database read-only e outra instancia Unity com o projeto aberto. |
| Menu `Cindar's Hope/Validation/Validate Shop Modal Flow` | Pendente no Unity. |
| TownScene buy/back/sell/back/buy/exit | Pendente no Play Mode. |
| Confirmacao visual de scroll/detalhes em resolucao menor | Pendente no Play Mode. |
| Compra/venda atualizando gold/inventory | Pendente no Play Mode. |

Comando Unity tentado:

```powershell
.\tools\unity\RunUnityCompileValidation.ps1 -UnityEditorPath 'C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe' -ProjectPath '.' -LogFile '.\Logs\spec17f-unity-compile-validation.log' -TimeoutSeconds 180
```

Resultado: `attempt to write a readonly database` e `Multiple Unity instances cannot open the same project.` O bloqueio ocorreu antes de executar compile/validators.

## Layout entregue

- Modal alvo: largura `620 px`, altura `560 px`, preservando o `CanvasScaler` existente em `1280x720`.
- Header de ouro fixo; lista em viewport rolavel; detalhes fixos abaixo; feedback e voltar no rodape.
- A cena existente recebe scroll/detalhes via adaptador runtime; a proxima geracao de `TownScene` persiste a estrutura responsiva diretamente.

## Criterio de fechamento

A SPEC 17F nao esta fechada. Promover somente apos Unity confirmar ausencia de `Modal type mismatch`, funcionamento repetido de buy/sell e renderizacao legivel do layout responsivo.
