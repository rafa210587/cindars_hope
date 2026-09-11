# SPEC — Operações de slots e prontidão das ações de inventário

> **Spec ID:** `spec_inventory_operations_and_action_readiness_v1`
> **Status:** CODE_COMPLETE
> **Wave:** QUALITY_IMPROVEMENTS
> **Type:** Runtime / Maintenance
> **Depends on:** `docs/validation/SOLID_AI_PROJECT_AUDIT.md`, `spec_solid_inventory_save_refactor_v1`
> **Scope:** manutenção/refatoração e correção de feedback autorizadas pelo pedido humano de 2026-09-08.
> **Ordem de execucao:** caracterização → extração → validação integrada → revisão
> **Depende de:** baseline em `TestResults/quality-improvements/20260908-000202/`
> **Bloqueia:** closeout das melhorias de inventário

required_adrs: []
required_game_rules: []

# /speckit.specify

## Problema e critérios de aceite

InventoryManager ainda mistura distribuição/retirada de pilhas com catálogo, aggregate e publicação. InventoryPanelController duplica prontidão de uso e lê ItemId de um slot mutável depois do consumo, produzindo `Used .` ao consumir a última unidade.

- Add preenche pilhas parciais antes dos slots vazios, sempre da esquerda para a direita. Recusa por capacidade não muta nada. MaxStack mínimo 1; mantém tratamento atual de equipados/metadados.
- Remove retira da última pilha para a primeira, incluindo equipados. Retirada parcial preserva metadados; esgotamento chama Clear e mantém SlotIndex.
- Catálogo, EnsureCapacity, aggregate e eventos permanecem no manager. Um evento por sucesso, depois da reconstrução; recusa não publica.
- Endurecimento explícito: quando objetos de slot expostos forem adulterados e não houver quantidade real suficiente, Remove recusa antes de qualquer mutação, mesmo se o aggregate antigo indicar disponibilidade. Não reconstruir silenciosamente dados corrompidos neste slice.
- A policy de Use compartilha precedência seleção → serviço → item utilizável entre botão e execução. Não reimplementa handlers ou consumo.
- Capturar o ItemId antes do uso corrige o feedback da última unidade. Uso segue consumindo uma unidade por ItemId; drop continua removendo a pilha selecionada somente após spawn aceito.
- Sem mudança de schema, IDs, serialized fields, GUIDs, input/lifecycle, equip/destroy ou fluxos de cena.

## Existing systems audit

Reusar InventorySlotOperations e InventoryAddResult; nenhum novo inventário/serviço. ItemUseManager já possui handlers/consumo/ItemUsedEvent; InventoryManager já possui drop. Não criar facade pass-through nem protocolo request/response para esta fatia. ProtectedItemActionGuard e ItemUseContractResolver têm responsabilidades distintas; não alterar política de itens protegidos incidentalmente.

# /speckit.plan

## Arquitetura e contratos

- InventorySlotOperations: `GetAvailableCapacityFor(IReadOnlyList<InventorySlot>, string, int)`, `TryAddItem(...)` retorna InventoryAddResult e `TryRemoveItem(IReadOnlyList<InventorySlot>, string, int)` retorna bool. Core sem Unity, catálogo ou eventos. A lista vem do owner e contém instâncias distintas; o core não altera a coleção.
- InventoryManager delega os algoritmos e mantém integração. Clear/restore/snapshot não serão refatorados; Clear possui ordem legada de eventos própria que esta tarefa não altera.
- InventoryItemActionPolicy: função pura `EvaluateUse(bool hasSelectedItem, bool useSystemAvailable, bool itemCanBeUsed)` retorna `InventoryItemUseBlockReason` (None/EmptySlot/UseSystemUnavailable/ItemCannotBeUsed).
- InventoryPanelController lê uma seleção por valores, compartilha prontidão entre botão e execução e mantém apresentação/mensagens. Chamadas legadas diretas entre UI e owners continuam dívida explícita; não alegar adesão total ao event bus.

## Arquivos permitidos

- `Assets/_Game/Scripts/Inventory/InventoryManager.cs`
- `Assets/_Game/Scripts/Inventory/InventorySlotOperations.cs`
- `Assets/_Game/Scripts/UI/InventoryPanelController.cs`
- `Assets/_Game/Scripts/UI/Inventory/InventoryItemActionPolicy.cs` e `.meta`
- `Assets/_Game/Tests/EditMode/UI/InventorySlotOperationsTests.cs`
- `Assets/_Game/Tests/EditMode/UI/InventorySlotMoveMergeTests.cs`
- `Assets/_Game/Tests/EditMode/UI/InventoryItemActionTests.cs` e `.meta`
- Esta spec e execution report; logs/status/audit para closeout.

Leitura adicional: InventorySlot, InventoryAddResult, ItemUseManager, ItemUseHandler, ItemDatabaseSO, ItemDataSO e eventos de inventário/uso. Proibidos edits em assets/cenas, farm/keyart, cave procedural, bootstrap e save schema.

# /speckit.tasks

## Execução e validação

1. Caracterizar Add/Remove na fachada e efeitos de Use antes de extrair os algoritmos; registrar falha esperada que reproduz o feedback da última unidade.
2. Extrair distribuição/retirada/capacidade para o core existente; cobrir limites de pilhas, recusa sem mutação, ordem e metadados em testes puros.
3. Unificar prontidão e capturar ItemId antes da mutação; testes de recusa/serviço/seleção e quantidade/eventos antes de ItemUsed.
4. Uma execução integrada dos testes pertinentes após mudanças; suíte completa e PlayMode de composição no fechamento amplo autorizado. Não duplicar builds apenas por delegação ou edits de documentação.
5. Registrar gates globais e as quatro falhas de farm separadamente; não promover a spec nem alegar teste humano sem evidência.
