# SPEC â€” LockedOreNodeInteractable Entrega MinÃ©rio Real ao Desbloquear

> **Spec ID:** `spec_codex_04_locked_ore_delivery`
> **Status:** A implementar
> **Wave:** WAVE CODEX CONVERGENCE â€” Honestidade de ValidaÃ§Ã£o
> **Priority:** P1
> **Type:** Runtime
> **Domain:** World / Inventory
> **Parallelizable:** YES
> **Parallel group:** codex_convergence
> **Can run with:** spec_codex_01, spec_codex_02, spec_codex_03, spec_codex_05, spec_codex_06, spec_codex_07, spec_codex_08
> **Must not run with:** N/A
> **Repo lock scope:** `Assets/_Game/Scripts/World/LockedOreNodeInteractable.cs`
> **Depends on:**
> - Nenhuma
> **Depends on (Depende de):**
> - Nenhuma spec deste lote. Depende apenas de sistemas existentes: `InventoryManager.AddItem`, o item jÃ¡ catalogado `item_material_copper_ore` (`CanonicalItemCatalog`), e o pipeline `PlayerActionFeedbackEvent` jÃ¡ usado por `TreeNode`.
> **Blocks:**
> - Nenhuma
> **Blocks (Bloqueia):**
> - Nenhuma. NÃ£o bloqueia nenhuma outra spec do lote `codex_convergence`.
> **Scope:** NÃ³ de minÃ©rio desbloqueado (`_unlocked == true`) entrega `item_material_copper_ore` real via `InventoryManager.AddItem`, com stamina-guard opcional (se aplicÃ¡vel ao padrÃ£o de mineraÃ§Ã£o do projeto), depletion guard (sÃ³ depleta se a entrega tiver sucesso) e feedback via `PlayerActionFeedbackEvent`.
> **Out of scope:** Implementar o gate de progressÃ£o que define quando `_unlocked` vira `true` (isso Ã© "spec futura" citada no prÃ³prio cÃ³digo, TODO explÃ­cito, fora do pedido); criar tipos de minÃ©rio variados por nÃ³ (usa o ID Ãºnico jÃ¡ confirmado).

required_adrs: []
required_game_rules: []

---

# /speckit.specify

## 5. Contexto

Achado verificado: `LockedOreNodeInteractable.cs` L33-37:

```csharp
if (_unlocked)
{
    // TODO (spec futura): entregar item de minerio ao jogador.
    Debug.Log($"[LockedOreNode] {_nodeId} desbloqueado â€” coleta nao implementada ainda.");
    return;
}
```

O nÃ³, uma vez desbloqueado, nÃ£o entrega nada â€” apenas loga. Esta spec fecha esse gap funcional especÃ­fico (entrega de item), sem tocar no gate de progressÃ£o (que Ã© explicitamente "spec futura" separada, fora de escopo aqui).

## 6. Problema

Um jogador que atinja a condiÃ§Ã£o de desbloqueio do nÃ³ de minÃ©rio (`_unlocked = true`, hoje setÃ¡vel sÃ³ via editor/inspector jÃ¡ que nÃ£o hÃ¡ gate real) interage e nÃ£o recebe nada â€” o nÃ³ simplesmente loga e nÃ£o muda de estado, entÃ£o pode ser minerado indefinidamente sem nunca dar recompensa real, quebrando a expectativa bÃ¡sica de "minerar dÃ¡ minÃ©rio".

## 7. Objetivo

Ao final desta spec, interagir com um `LockedOreNodeInteractable` desbloqueado entrega `item_material_copper_ore` real ao inventÃ¡rio do jogador (via `InventoryManager.AddItem`, mesmo padrÃ£o de `TreeNode`), publica feedback de sucesso, e sÃ³ transiciona/depleta se a entrega tiver sucesso (guard contra perda silenciosa de recompensa se o inventÃ¡rio estiver cheio).

## 8. Fontes obrigatÃ³rias lidas

```text
CLAUDE.md
docs/project/CURRENT_STATE.md
.claude/skills/scene-interactable-wiring/SKILL.md
.claude/skills/inventory-transactions/SKILL.md
.claude/skills/action-feedback-pipeline/SKILL.md
```

## 9. Estado atual do repo (Phase 0 â€” auditado nesta sessÃ£o)

- `Assets/_Game/Scripts/World/LockedOreNodeInteractable.cs` (52 linhas): `MonoBehaviour, IInteractable`; campos `_nodeId` (string, serializado), `_unlocked` (bool, serializado, default false). `Interact(GameObject interactor)` hoje sÃ³ loga quando `_unlocked`. Sem `InventoryManager` referenciado hoje â€” precisa ser adicionado.
- PadrÃ£o irmÃ£o real e funcional: `Assets/_Game/Scripts/World/TreeNode.cs` â€” `[SerializeField] private InventoryManager _inventoryManager;` + `[SerializeField] private StaminaManager _staminaManager;`, e em `Interact()`:
  1. `CanInteract` guard;
  2. checagem de ferramenta via `EquipmentManager.HasTool(...)` (via `GameBootstrap.Instance`, sem `FindObjectOfType`);
  3. checagem de stamina **antes** da aÃ§Ã£o (`_staminaManager.CurrentStamina < cost`) com feedback + early return se insuficiente;
  4. `_inventoryManager.AddItem(itemId, amount)` â€” se falhar (`false`), loga warning e **nÃ£o** muda estado (early return, preservando a Ã¡rvore intacta);
  5. sÃ³ depois do `AddItem` ter sucesso, gasta stamina via `TrySpendStamina`; se o spend falhar (race rara), reverte o item via `RemoveItem` e retorna;
  6. sÃ³ entÃ£o incrementa `HitsTaken`/muda estado (`IsChopped` na finalizaÃ§Ã£o).
  - Este Ã© o padrÃ£o canÃ´nico de "entrega de recompensa + depletion guard" a seguir para `LockedOreNodeInteractable`, adaptado (nÃ³ de minÃ©rio Ã© single-shot: uma interaÃ§Ã£o = uma entrega + depleÃ§Ã£o, ao contrÃ¡rio da Ã¡rvore que tem mÃºltiplos hits).
- `InventoryManager.AddItem(string itemId, int amount)` (`Assets/_Game/Scripts/Inventory/InventoryManager.cs` L327-330): `public bool AddItem(string itemId, int amount) => TryAddItem(itemId, amount).Success;` â€” retorna `false` se nÃ£o coube (inventÃ¡rio cheio/capacidade), sem lanÃ§ar exceÃ§Ã£o. Este Ã© o contrato a respeitar (guard: sÃ³ depletar/mudar estado se `true`).
- ID real de item de minÃ©rio confirmado em `Assets/_Game/Scripts/Editor/Items/CanonicalItemCatalog.cs` L321: `Mat("item_material_copper_ore", "Copper Ore", 8);` â€” este Ã© o ID canÃ´nico a usar (`item_material_copper_ore`), jÃ¡ existente no catÃ¡logo gerado (nÃ£o precisa criar item novo).
- NÃ£o hÃ¡ referÃªncia serializada a `StaminaManager` nem `EquipmentManager` em `LockedOreNodeInteractable` hoje â€” a spec nÃ£o exige adicionar checagem de ferramenta/stamina (o TODO original nÃ£o menciona isso), mas por consistÃªncia com o padrÃ£o de `TreeNode` e para nÃ£o regredir expectativa de progressÃ£o, avaliar na Fase 0 de implementaÃ§Ã£o se o nÃ³ deve exigir uma picareta (`ToolType.Pickaxe`, se existir no enum `ToolType` â€” Grep antes de assumir) antes de entregar; se `ToolType` nÃ£o tiver esse valor, **nÃ£o inventar** â€” manter escopo mÃ­nimo (YAGNI): entrega simples sem gate de ferramenta, documentando a decisÃ£o.
- Sem `IInteractable.CanInteract` alterado â€” hoje sempre retorna `true` (para mostrar o feedback de recusa mesmo bloqueado); este comportamento Ã© preservado.

## 10. User stories / engineering stories

```text
Como jogador, quero que minerar um nÃ³ de minÃ©rio desbloqueado realmente me dÃª minÃ©rio, para a progressÃ£o fazer sentido.
Como sistema de inventÃ¡rio, quero que a entrega falhe graciosamente (sem perder o nÃ³) se o inventÃ¡rio estiver cheio.
```

## 11. Escopo

Inclui:
- Adicionar `[SerializeField] private InventoryManager _inventoryManager;` a `LockedOreNodeInteractable`.
- Definir a quantidade entregue por interaÃ§Ã£o (const nomeada, ex.: `private const int OreYieldAmount = 1;` â€” YAGNI: quantidade fixa simples, sem RNG/range, a menos que o padrÃ£o de `TreeNode` (`RollWoodAmount`) seja considerado necessÃ¡rio aqui; documentar a decisÃ£o de manter simples).
- Alterar `Interact()`: quando `_unlocked`, chamar `_inventoryManager.AddItem("item_material_copper_ore", OreYieldAmount)`; se `true`, publicar `PlayerActionFeedbackEvent` de sucesso (ex.: "MinÃ©rio coletado!") e marcar o nÃ³ como depletado (novo campo `_depleted` bool, ou reusar convenÃ§Ã£o equivalente jÃ¡ usada por outro nÃ³ do projeto se existir â€” auditar `FarmResourceInteractable`/`FarmResourceRefreshRuntime` antes de inventar um campo novo de depleÃ§Ã£o, para nÃ£o duplicar um padrÃ£o jÃ¡ existente de "recurso esgotado"); se `false` (inventÃ¡rio cheio), publicar feedback de recusa (ex.: "InventÃ¡rio cheio") e **nÃ£o** depletar o nÃ³.
- MÃ©todo `EditorSetNodeId` existente preservado; adicionar `EditorWire`/`RebindInventoryManager` anÃ¡logo ao padrÃ£o de `TreeNode.RebindInventoryManager` para permitir wiring pelo gerador de cena sem `FindObjectOfType`.
- EditMode test se a lÃ³gica puder ser extraÃ­da como mÃ©todo puro (ex.: "dado unlocked=true e AddItem retorna true/false, qual o resultado?") â€” se o acoplamento a `MonoBehaviour`/`GameEventBus` estÃ¡tico tornar isso impraticÃ¡vel em EditMode puro, documentar como residual e citar no report (mas tentar extrair ao menos a decisÃ£o "entregar ou nÃ£o" como funÃ§Ã£o pura testÃ¡vel).

Fora:
- Implementar o gate de progressÃ£o (`_unlocked` seguir controlado sÃ³ por inspector/editor, como jÃ¡ estÃ¡).
- Checagem de ferramenta (picareta) â€” decisÃ£o explÃ­cita de manter fora, a menos que a Fase 0 encontre um padrÃ£o de progressÃ£o que exija isso (documentar se mudar de ideia).
- Suporte a mÃºltiplos tipos de minÃ©rio por nÃ³.

## 12. Fora de escopo

```text
NÃ£o inclui: sistema de gate de progressÃ£o; UI nova; variaÃ§Ã£o de minÃ©rio por bioma/tier.
```

## 13. Regras de nÃ£o duplicaÃ§Ã£o

```text
NÃ£o criar um segundo padrÃ£o de "recurso depletÃ¡vel" â€” reusar convenÃ§Ã£o jÃ¡ existente (auditar FarmResourceInteractable/FarmResourceRefreshRuntime antes de adicionar campo novo).
NÃ£o criar item novo â€” usar item_material_copper_ore jÃ¡ existente no catÃ¡logo.
```

## 14. CritÃ©rios de aceite

### 14.1 Entrega real de item

- Interagir com o nÃ³ desbloqueado chama `InventoryManager.AddItem("item_material_copper_ore", <amount>)`.
- Em sucesso, o nÃ³ depleta (nÃ£o pode mais ser minerado / interaÃ§Ã£o seguinte nÃ£o entrega de novo, ou reusa a convenÃ§Ã£o de refresh do projeto se aplicÃ¡vel) e publica feedback de sucesso.
- EvidÃªncia esperada: leitura do cÃ³digo + teste (se extraÃ­vel).

### 14.2 Guard de inventÃ¡rio cheio

- Se `AddItem` retornar `false`, o nÃ³ **nÃ£o** depleta e publica feedback de recusa (ex.: "InventÃ¡rio cheio").
- EvidÃªncia esperada: leitura do cÃ³digo + teste (se extraÃ­vel).

### 14.3 Sem regressÃ£o de wiring

- `dotnet build` PASS; `EditorSetNodeId` preservado; novo mÃ©todo de wiring de `InventoryManager` documentado para o gerador de cena consumir (mesmo que o gerador em si nÃ£o seja atualizado nesta spec, se estiver fora do escopo de arquivos permitidos).

---

# /speckit.plan

## 15. Arquitetura alvo

```text
Assets/_Game/Scripts/World/
  LockedOreNodeInteractable.cs   (+ InventoryManager ref, entrega real, depletion guard, feedback)

Assets/_Game/Tests/EditMode/World/
  LockedOreNodeInteractableTests.cs (novo, se lÃ³gica extraÃ­vel)
```

## 16. Contratos, dados e eventos

### 16.1 Data contracts
Nenhum SO novo â€” reusa `item_material_copper_ore` do `ItemDatabase` existente.

### 16.2 Runtime contracts
`LockedOreNodeInteractable.Interact(GameObject)` â€” mesma assinatura pÃºblica (via `IInteractable`), lÃ³gica interna muda.

### 16.3 Event contracts
Reusa `PlayerActionFeedbackEvent` existente; nenhum evento novo.

### 16.4 Save contracts
N/A â€” a menos que o padrÃ£o de depleÃ§Ã£o reusado (FarmResourceInteractable) jÃ¡ persista estado de depleÃ§Ã£o no save; se sim, seguir o mesmo contrato sem inventar um novo.

### 16.5 UI contracts
N/A â€” feedback via pipeline de toast jÃ¡ existente.

## 17. Sistemas afetados

```text
World (interactables)
Inventory (AddItem)
Action feedback pipeline
```

## 18. Arquivos permitidos

```text
Assets/_Game/Scripts/World/LockedOreNodeInteractable.cs
Assets/_Game/Tests/EditMode/World/**
docs/validation/**
```

## 19. Arquivos proibidos

```text
Assets/_Game/Scripts/Farm/Integration/FarmResourceInteractable.cs (leitura ok, ediÃ§Ã£o nÃ£o)
Assets/_Game/Scripts/Editor/SceneCreation/** (a menos que autorizado apÃ³s confirmaÃ§Ã£o de necessidade)
Assets/**/*.unity, *.prefab, *.asset
```

## 20. EstratÃ©gia de implementaÃ§Ã£o

```md
### Fase 0 â€” Auditoria: confirmar padrÃ£o de depleÃ§Ã£o existente (FarmResourceInteractable) e ToolType.Pickaxe (se existir)
### Fase 1 â€” Wiring de InventoryManager + const de yield
### Fase 2 â€” Entrega real + depletion guard + feedback
### Fase 3 â€” Testes/validaÃ§Ã£o
### Fase 4 â€” RelatÃ³rio
```

## 21. Ordem de execucao (ordem segura)

```text
1. Auditar padrÃ£o de depleÃ§Ã£o/refresh jÃ¡ existente (nÃ£o duplicar).
2. Adicionar InventoryManager ref + mÃ©todo de wiring (RebindInventoryManager-like).
3. Reescrever Interact() com entrega real, guard e feedback.
4. Extrair lÃ³gica de decisÃ£o pura se possÃ­vel; escrever EditMode test.
5. dotnet build + EditMode tests.
6. Registrar relatÃ³rio.
```

## 22. ParalelizaÃ§Ã£o

```md
- Parallelizable: YES
- Parallel group: codex_convergence
- Can run with: demais specs do lote
- Must not run with: N/A
- Shared files/systems that require lock: World/LockedOreNodeInteractable.cs (lock local)
- Reason: escopo isolado; Ãºnico ponto de contato com Inventory Ã© uma chamada de API pÃºblica jÃ¡ estÃ¡vel
```

## 23. Impacto em save/load

```text
Does this change save schema? CONDITIONAL (sÃ³ se reusar um padrÃ£o de depleÃ§Ã£o que jÃ¡ persiste estado â€” documentar se ocorrer; se o nÃ³ nunca teve estado salvo antes, isso Ã© gap prÃ©-existente, nÃ£o desta spec introduzir sozinha sem necessidade)
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? MUST BE NO (garantido â€” sem novo DTO)
```

## 24. Impacto em eventos

```text
Adds events: NO
Changes existing events: NO
Requires unsubscribe pattern: NO
```

## 25. Impacto em UI/Unity

```text
Changes UI: NO (reusa toast existente)
Changes scenes: NO
Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: YES (entrega real de item Ã© gameplay observÃ¡vel)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## 26. Riscos tÃ©cnicos

```text
Risco: nenhum nÃ³ real estÃ¡ colocado em cena hoje com _unlocked = true acessÃ­vel ao jogador (gate de progressÃ£o nÃ£o existe), tornando o comportamento nÃ£o observÃ¡vel em Play Mode atÃ© uma spec futura ligar o gate.
MitigaÃ§Ã£o: documentar isso explicitamente no execution report; a spec ainda fecha o TODO de entrega, e o teste automatizado cobre a lÃ³gica isoladamente.

Risco: se um padrÃ£o de depleÃ§Ã£o existente (FarmResourceInteractable) tiver save schema associado, reusar mal pode introduzir acoplamento indevido.
MitigaÃ§Ã£o: preferir um campo local simples (_depleted bool, sem save) se o padrÃ£o existente nÃ£o se encaixar 1:1; documentar a decisÃ£o.
```

## 27. Rollback

```text
Reverter LockedOreNodeInteractable.cs para o TODO original (apenas log).
Remover testes novos.
```

---

# /speckit.tasks

## 28. Tasks

```md
- [ ] T001 â€” Auditar padrÃ£o de depleÃ§Ã£o existente (FarmResourceInteractable) e ToolType.Pickaxe.
- [ ] T002 â€” Adicionar InventoryManager ref + mÃ©todo de wiring.
- [ ] T003 â€” Reescrever Interact() com entrega real (item_material_copper_ore) + depletion guard + feedback de sucesso/recusa.
- [ ] T004 â€” Extrair lÃ³gica de decisÃ£o pura se praticÃ¡vel; escrever EditMode test.
- [ ] T005 â€” Gerar execution report (citando se o gate de progressÃ£o real ainda nÃ£o existe, tornando o comportamento nÃ£o observÃ¡vel em Play Mode sem debug manual do campo _unlocked).
```

## 29. ValidaÃ§Ãµes obrigatÃ³rias

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\validate_docs.ps1
```

Unity Test Runner â€” EditMode: rodar se praticÃ¡vel; senÃ£o `NOT RUN` com motivo.

## 30. Testing Quality Gate

```md
## Testing Quality Gate

- Changed deterministic logic: YES (decisÃ£o de entrega/depleÃ§Ã£o)
- Requires EditMode tests: YES (se extraÃ­vel; senÃ£o documentar residual)
- Requires PlayMode automated or final human scenario: YES (entrega de item Ã© gameplay observÃ¡vel, mesmo que hoje sÃ³ acessÃ­vel via inspector com _unlocked=true forÃ§ado)
- Requires regression test: NO (feature nova, nÃ£o bugfix)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: dotnet build PASS + teste da lÃ³gica de decisÃ£o (se extraÃ­vel) + cenÃ¡rio de Play Mode documentado (forÃ§ar _unlocked=true via inspector, interagir, confirmar item no inventÃ¡rio e depleÃ§Ã£o).
```

## 31. Definition of Done

```text
LockedOreNodeInteractable entrega item_material_copper_ore real ao desbloquear.
Depletion guard implementado (nÃ£o depleta se AddItem falhar).
Feedback de sucesso/recusa publicado.
Execution report criado, citando a ausÃªncia do gate de progressÃ£o real como contexto.
```

## 32. Anti-regressÃ£o

```text
NÃ£o alterar CanInteract() (deve continuar sempre true, para mostrar feedback de recusa mesmo bloqueado).
NÃ£o alterar EditorSetNodeId existente.
NÃ£o criar item novo â€” usar item_material_copper_ore.
```

## 33. Notas para execuÃ§Ã£o posterior

```text
O gate de progressÃ£o que define _unlocked = true continua nÃ£o implementado â€” isso Ã© explicitamente uma spec futura separada (citada no TODO original do cÃ³digo), nÃ£o desta spec.
Se o padrÃ£o de depleÃ§Ã£o reusado tiver save schema, documentar a decisÃ£o e o que fica pendente para uma spec de save-section-provider, se necessÃ¡rio.
```
