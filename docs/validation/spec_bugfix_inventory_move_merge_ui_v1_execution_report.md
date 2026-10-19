---
spec_id: spec_bugfix_inventory_move_merge_ui_v1
status: BUILD_VALIDATED_WITH_WARNINGS
validated_adrs: []
validated_game_rules: []
---

# Execution report — mover e mesclar pilhas no inventário

## Implementação

- `InventoryManager.TryMoveOrMergeSlot` move a pilha para slot vazio, mescla pilhas iguais até `MaxStack` e troca itens distintos.
- Todas as validações de índice, origem, item conhecido, slot equipado e pilha cheia ocorrem antes de mutar os slots.
- O conteúdo dos slots é copiado, nunca as instâncias de `InventorySlot`; portanto o `SlotIndex` físico permanece na sua posição.
- O painel agora expõe `Mover/Mesclar`, pede um destino, aceita clique ou `WASD`/setas + `E`/Enter e permite cancelar com `Esc`.
- Foram adicionados seis testes EditMode de mover, merge parcial, merge cheio, troca, item equipado e índice inválido.
- `ItemUseManager` e `ItemDropSpawner` agora são inicializados pela composition root com o `InventoryManager` e o `ItemDatabaseSO` do `GameBootstrap`; antes eram apenas instalados, portanto não conseguiam usar/remover itens ou criar drops.
- As ações são desenhadas em um submodal IMGUI pequeno sobre a grade. A grade fica visível e bloqueada; `Esc` retorna à grade sem fechar o modal de inventário. `Usar` é desabilitado e rotulado como indisponível quando não há handler.
- O `ItemDropSpawner` agora configura cada pickup dinâmico na sorting layer `World`, acima do terreno, e adiciona um `Rigidbody2D` cinemático, sem gravidade e com rotação congelada. O `ItemPickup` existente continua a coletar por `IInteractable` e somente se desativa após `AddItem` bem-sucedido.

## Validação executada

| Comando | Resultado |
|---|---|
| `dotnet build .\CindarsHope.Runtime.csproj` | PASS — exit code 0; 0 erros, 6 warnings preexistentes. |
| `dotnet build .\CindarsHope.Editor.csproj` | PASS — exit code 0; 0 erros. |
| `dotnet build .\CindarsHope.Tests.EditMode.csproj` | PASS — exit code 0; 0 erros. |
| `dotnet build .\CindarsHope.Runtime.csproj` (adendo) | PASS — exit code 0; 0 erros, 6 warnings preexistentes. |
| `powershell -ExecutionPolicy Bypass -File .\tools\docs\validate_docs.ps1` | FAIL — exit code 1. Também acusa esta spec por cabeçalhos `Ordem de execucao`, `Depende de` e `Bloqueia` ausentes; há erros preexistentes em outras specs e no scanner de placeholders. |

## Atualizacao da validacao de documentacao

Depois da primeira execucao, esta spec recebeu os cabecalhos requeridos `Ordem de execucao`, `Depende de` e `Bloqueia`. A segunda execucao do mesmo comando continuou com exit code 1 exclusivamente por divida preexistente: specs futuras fora do escopo sem marcadores/cabecalhos e falsos positivos do scanner de placeholders. Nenhuma falha remanescente referencia esta spec.

## Testes pendentes

Unity EditMode: NOT RUN.

Motivo: o Editor Unity está aberto; não é seguro executar batchmode concorrente.

Risco residual: os testes novos compilam, mas sua execução no runner Unity e a interação de UI precisam da confirmação humana pelo cenário em `docs/validation/playmode/spec_bugfix_inventory_move_merge_ui_v1_human_test_scenario.md`.

## Testing Quality Gate — adendo

Teste de regressão do pickup dinâmico: JUSTIFIED manualmente. `TryDropItem` depende do singleton estático do spawner e de `InventoryManager`/`ItemDatabaseSO` inicializados pelo `GameBootstrap`; isolar esse caminho em EditMode exigiria alterar a API ou introduzir fixtures de runtime fora do escopo. O cenário humano atualizado cobre ícone visível no chão, prompt `Pegar` e coleta por `E`.

Classificação: Regression — os serviços foram instalados em mudança recente, mas seu binding obrigatório foi omitido.
