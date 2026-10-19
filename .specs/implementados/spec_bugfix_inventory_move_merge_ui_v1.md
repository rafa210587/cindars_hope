---
id: spec_bugfix_inventory_move_merge_ui_v1
title: "Bugfix — mover e mesclar pilhas no inventário"
status: implemented
date_implemented: 2026-08-13
type: bugfix_runtime_ui
depends_on: []
required_adrs: []
required_game_rules: []
evidence:
  - Código: InventoryManager.TryMoveOrMergeSlot(int, int, out string) em Assets/_Game/Scripts/Inventory/InventoryManager.cs:546; consumido por InventoryPanelController.cs:451.
  - Play Mode humano PASS 2026-08-13 (usuário confirmou os 11 fluxos de docs/validation/playmode/PLAYTEST_SIMPLES.md, incl. fluxo 4 — Inventário: split, mover para slot vazio, mesclar pilhas, cancelar com Esc).
  - Validação automatizada preexistente: ver docs/validation/spec_bugfix_inventory_move_merge_ui_v1_execution_report.md (build/EditMode) se presente.
---

## Ordem de execucao

**Executar após:** o provisionamento do loadout de playtest, que cria a pilha de sementes usada no cenário.
**Ordem:** bugfix localizado de inventário; não inicia nem desbloqueia outra spec.

## Depende de

- `InventoryManager`, `InventorySlot` e `InventoryPanelController` existentes.
- `ItemDatabaseSO` para resolver `MaxStack` e validar IDs.

## Bloqueia

- A confirmação humana do passo 4 do roteiro em `docs/validation/playmode/PLAYTEST_SIMPLES.md`.

# /speckit.specify

## Objetivo

Fechar o passo 4 do playtest: o jogador deve conseguir mover uma pilha para um slot vazio, mesclar pilhas do mesmo item e trocar itens diferentes pelo painel de inventário.

## Evidência do problema

O `InventoryManager` já adiciona itens com empilhamento automático e divide pilhas, mas não expõe uma operação entre dois slots. O `InventoryPanelController` só oferece `Split`; por isso pressionar `E` não apresenta uma opção para mover ou empilhar.

## Sistemas reutilizados

| Sistema | Decisão |
|---|---|
| `InventoryManager` | Estender com uma única transação atômica entre slots. |
| `InventoryPanelController` | Acrescentar o fluxo de selecionar origem e destino. |
| `InventorySlot` e save existente | Reutilizar sem alterar o schema. |
| `InventoryChangedEvent` | Publicar após sucesso para atualizar consumidores existentes. |

Não criar manager, modal, asset ou formato de save paralelo.

## Contrato

Adicionar ao `InventoryManager`:

```csharp
public bool TryMoveOrMergeSlot(int sourceSlotIndex, int destinationSlotIndex, out string failureReason);
```

Regras:

1. Índices inválidos, origem igual ao destino, origem vazia, item desconhecido ou qualquer slot equipado retornam `false`, preenchem `failureReason` e não alteram slots nem agregados.
2. Destino vazio move toda a origem e limpa a origem.
3. Mesmo `ItemId` transfere somente o espaço disponível até `MaxStack`; se não houver espaço, retorna `false` sem mutação. A transferência parcial conserva a quantidade total.
4. Itens diferentes trocam os conteúdos dos slots, preservando o `SlotIndex` de cada posição.
5. Em qualquer sucesso, reconstruir agregados e publicar uma atualização de inventário com delta zero para cada `ItemId` distinto afetado. Não publicar evento em falha.

## Fluxo de UI

1. Em uma pilha selecionada, as ações devem incluir `Mover/Mesclar`.
2. Essa ação entra no modo de destino e informa para selecionar o slot de destino; `Esc` cancela e retorna aos slots sem mutação.
3. Clique ou `E`/Enter no destino executa `TryMoveOrMergeSlot`, apresenta sucesso ou `failureReason` e volta ao modo de slots.
4. A navegação do destino segue as mesmas teclas de slots (`WASD`, setas, `E`/Enter). A interface não deve permitir acidentalmente abrir o menu de ações enquanto escolhe o destino.

## Adendo - acoes de item e submodal

1. A lista de acoes aparece em uma caixa menor centralizada sobre a grade do inventario; a grade permanece visivel e nao interativa ao fundo.
2. `Esc` fecha apenas essa caixa e retorna a grade selecionada; nao fecha o inventario.
3. `Usar` so fica habilitado para itens que `ItemUseManager.CanUseItem` reconhece. Para os demais, mostrar `Usar (indisponivel)` desabilitado, sem chamar handler inexistente.
4. `Dropar` deve funcionar com o `ItemDropSpawner` ja instalado. O composition root deve inicializar tanto o spawner quanto o `ItemUseManager` com o `InventoryManager` e `ItemDatabaseSO` atuais, apos ambos existirem.
5. Nenhuma mudanca de save, asset, prefab ou cena.

## Adendo - pickup no chao

1. Um item dropado deve criar um `ItemPickup` visivel acima do chao e interagivel por `E`.
2. O objeto dinamico recebe `Rigidbody2D` cinematico, sem gravidade, para assegurar os callbacks de trigger da area de interacao do player.
3. O `SpriteRenderer` do pickup usa ordem visual acima do terreno; a coleta continua via `IInteractable` e so desativa o pickup depois de `InventoryManager.AddItem` bem-sucedido.

# /speckit.plan

## Arquivos permitidos

- `Assets/_Game/Scripts/Inventory/InventoryManager.cs`
- `Assets/_Game/Scripts/UI/InventoryPanelController.cs`
- `Assets/_Game/Scripts/Composition/DomainRuntimeInstallers.cs`
- `Assets/_Game/Scripts/World/ItemDropSpawner.cs`
- `Assets/_Game/Tests/EditMode/UI/InventorySlotMoveMergeTests.cs`
- `Assets/_Game/Tests/EditMode/UI/InventorySlotMoveMergeTests.cs.meta`
- `docs/validation/playmode/PLAYTEST_SIMPLES.md`
- `docs/validation/playmode/spec_bugfix_inventory_move_merge_ui_v1_human_test_scenario.md`
- `docs/validation/spec_bugfix_inventory_move_merge_ui_v1_execution_report.md`
- esta spec

## Arquivos proibidos

- `Assets/**/*.unity`, `Assets/**/*.prefab`, `Assets/**/*.asset`
- `Assets/_Game/Scripts/Runtime/Save/**`
- `Packages/**`, `ProjectSettings/**`, `docs_old/**`

## Plano de edição

1. Implementar a transação no manager, sem referências Unity no estado de inventário e preservando os índices físicos dos slots.
2. Implementar o estado de seleção de destino no controller, inclusive retorno por `Esc` e feedback de erro.
3. Adicionar testes EditMode de mover, merge parcial, merge cheio, troca, slot equipado e índice inválido.
4. Tornar o passo 4 do guia reproduzível: dividir uma pilha, usar `Mover/Mesclar` e escolher a outra pilha.
5. Criar cenário humano e report honesto de validação.

# /speckit.tasks

## Adendo tasks

6. Fazer o binding explicito de `ItemUseManager` e `ItemDropSpawner`, e substituir a tela cheia de acoes por uma caixa filha sobre a grade.
7. Fazer o pickup dinamico do drop ter fisica de trigger e prioridade visual adequadas, sem editar scene ou prefab.

## Critérios de aceite

- [ ] Uma pilha pode ser movida para slot vazio sem perda ou duplicação.
- [ ] Duas pilhas iguais podem ser mescladas até `MaxStack`; o excedente fica na origem.
- [ ] Uma pilha destino cheia falha de modo atômico.
- [ ] Dois itens diferentes podem trocar de slot, preservando totais.
- [ ] Slots equipados e índices inválidos falham sem mutação.
- [ ] A UI oferece `Mover/Mesclar`, permite selecionar destino e `Esc` cancelar.
- [ ] O guia descreve o fluxo real.
- [ ] Acoes aparecem em submodal pequeno sobre a grade; `Esc` volta somente para a grade.
- [ ] `Usar` nao executa itens sem handler e comunica indisponibilidade.
- [ ] `Dropar` instancia pickup e remove a pilha somente apos o spawn bem-sucedido.
- [ ] Pickup dropado fica visivel no chao e pode ser recolhido por `E` sem perder item se o inventario estiver cheio.

## Validação obrigatória

```powershell
dotnet build .\CindarsHope.Runtime.csproj
dotnet build .\CindarsHope.Editor.csproj
dotnet build .\CindarsHope.Tests.EditMode.csproj
.\tools\docs\validate_docs.ps1
```

Unity EditMode e Play Mode não serão executados em batch enquanto o Editor estiver aberto. O cenário humano deve ser entregue para o usuário executar no Editor.
