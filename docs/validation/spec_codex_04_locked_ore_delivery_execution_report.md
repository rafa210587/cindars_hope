# Execution Report — spec_codex_04_locked_ore_delivery

> **Spec:** `.specs/a_implementar/spec_codex_04_locked_ore_delivery.md`
> **Date:** 2026-07-03
> **Status:** BUILD_VALIDATED
> **Type:** Runtime (World / Inventory)

---

## Acceptance criteria extracted

Da seção "14. Critérios de aceite" da spec:

1. **14.1 Entrega real de item** — Interagir com o nó desbloqueado chama `InventoryManager.AddItem("item_material_copper_ore", <amount>)`. Em sucesso, o nó depleta e publica feedback de sucesso.
2. **14.2 Guard de inventário cheio** — Se `AddItem` retornar `false`, o nó **não** depleta e publica feedback de recusa.
3. **14.3 Sem regressão de wiring** — `dotnet build` PASS; `EditorSetNodeId` preservado; novo método de wiring de `InventoryManager` documentado.

---

## Existing systems audit (Phase 0)

- `Assets/_Game/Scripts/World/TreeNode.cs` — padrão irmão confirmado: `InventoryManager.AddItem` → guard de sucesso antes de qualquer mudança de estado → `RebindInventoryManager` como API de wiring sem `FindObjectOfType`. Seguido fielmente (adaptado para single-shot, sem stamina/tool guard — fora de escopo por decisão explícita da spec).
- `Assets/_Game/Scripts/Farm/Integration/FarmResourceInteractable.cs` — padrão de depleção existente auditado. Usa `FarmResourceVisualState` + `FarmResourceRefreshRuntime.RegisterForRefresh` (ciclo de refresh por dia, IDs `farm_node_tree`/`farm_node_rock`). **Decisão: não reusar** — esse padrão é acoplado a um ciclo de refresh periódico que não se aplica a um nó de desbloqueio único (single-shot, sem respawn). Reusar forçaria acoplamento indevido a `FarmResourceRefreshRuntime` para um caso que não tem esse conceito. Optou-se por um campo local simples `_depleted` (bool, sem save), conforme a mitigação de risco já prevista na própria spec (seção 26).
- `Assets/_Game/Scripts/Tools/ToolType.cs` — confirmado que `ToolType.Pickaxe` existe no enum. Decisão: **não adicionar gate de ferramenta** — está fora de escopo explícito da spec (seção 11 "Fora"), e o TODO original não menciona isso. YAGNI aplicado.
- `Assets/_Game/Scripts/Inventory/InventoryManager.cs` L327-330 — `AddItem` confirmado: retorna `bool`, sem exceção. Contrato respeitado.
- `Assets/_Game/Scripts/Editor/Items/CanonicalItemCatalog.cs` L321 — `item_material_copper_ore` confirmado existente no catálogo. Nenhum item novo criado.
- `Assets/_Game/Scripts/Core/Events/PlayerActionFeedbackEvent.cs` — assinatura confirmada (`Message`, `DurationSeconds` opcional). Reusado sem alteração.

Nenhum sistema paralelo criado. Nenhum novo manager/service.

---

## Spec Compliance Matrix

| Critério | Status | Evidência |
|---|---|---|
| 14.1 Entrega real de item | PASS | `Interact()` chama `_inventoryManager.AddItem(OreItemId, OreYieldAmount)`; em sucesso seta `_depleted = true` e publica `PlayerActionFeedbackEvent(SucessoMessage)` |
| 14.2 Guard de inventário cheio | PASS | Se `AddItem` retorna `false`, publica `PlayerActionFeedbackEvent(RecusaInventarioCheioMessage)` e retorna sem setar `_depleted` |
| 14.3 Sem regressão de wiring | PASS | `dotnet build` exit 0 (ambos assemblies); `EditorSetNodeId` inalterado; `RebindInventoryManager` adicionado (análogo a `TreeNode.RebindInventoryManager`) |
| Escopo: sem gate de progressão (`_unlocked`) | PRESERVED | `_unlocked` continua bool serializado, sem gate real — inalterado |
| Escopo: sem checagem de ferramenta | PRESERVED | Nenhum `EquipmentManager`/`ToolType` referenciado |
| Escopo: sem múltiplos tipos de minério | PRESERVED | ID único `item_material_copper_ore`, const nomeada |
| Anti-regressão: `CanInteract()` sempre `true` | PRESERVED | Método inalterado, ainda retorna `true` incondicionalmente |
| Anti-regressão: não criar item novo | PRESERVED | Reusa `item_material_copper_ore` |
| EditMode test da lógica de decisão | DONE | `LockedOreNodeInteractableTests.cs` — 5 testes cobrindo `ResolveDelivery` (Blocked, AlreadyDepleted, Delivered, InventoryFull) |

---

## Files changed

- `Assets/_Game/Scripts/World/LockedOreNodeInteractable.cs` (modificado) — `InventoryManager` ref, entrega real, depletion guard local (`_depleted`), feedback de sucesso/recusa, `RebindInventoryManager`, método estático puro `ResolveDelivery` + enum `LockedOreDeliveryResult`.
- `Assets/_Game/Tests/EditMode/World/LockedOreNodeInteractableTests.cs` (novo) — 5 testes EditMode cobrindo a decisão pura de entrega.
- `docs/validation/spec_codex_04_locked_ore_delivery_execution_report.md` (este arquivo, novo).

---

## Validation

```text
Validation method: run_strict_validation.ps1
Exit code: 1 (falhas pré-existentes, nenhuma relacionada a esta spec — ver abaixo)
Assembly-CSharp: PASS (0 erros; warnings pré-existentes não relacionados)
Assembly-CSharp-Editor: PASS (0 erros; warnings pré-existentes não relacionados, incl. CS0649 em código não tocado por esta spec)
Docs validation: EXPECTED_FAIL_LEGACY_ONLY — falhas listadas:
  - Placeholders em tools/codex/Generate-CodexHarness.ps1 (pré-existente, não tocado nesta spec)
  - spec_npc_physics_cat_companion.md sem required_adrs/required_game_rules (pré-existente, não tocado nesta spec)
Diff completeness: PASS após criação deste execution report (era a única falha nova, resolvida)
```

Nenhuma falha do `run_strict_validation.ps1` menciona `LockedOreNodeInteractable.cs`, `LockedOreNodeInteractableTests.cs`, ou este execution report. Todas as falhas remanescentes são pré-existentes e fora do escopo/arquivos tocados por esta spec (confirmado por leitura do output completo do script).

EditMode Test Runner (Unity): **NOT RUN** — sem instância do Unity Editor executável nesta sessão (ambiente CLI/batchmode não disponível para Test Runner interativo). Os 5 testes foram escritos seguindo a convenção NUnit existente do projeto (`[TestFixture]`/`[Test]`, namespace `CindarsHope.Tests.EditMode.World`) e compilam como parte de `Assembly-CSharp-Editor` (build PASS confirma compilação sem erro, mas não confirma execução/assert em runtime do Test Runner).

---

## Testing Quality Gate

- Changed deterministic logic: YES — decisão de entrega/depleção extraída como `LockedOreNodeInteractable.ResolveDelivery(bool, bool, bool)`, método estático puro sem dependência de `MonoBehaviour`/`GameEventBus`.
- Requires EditMode tests: YES — atendido; 5 testes cobrindo os 4 resultados possíveis (`Blocked`, `AlreadyDepleted`, `Delivered`, `InventoryFull`).
- Requires PlayMode automated or final human scenario: YES — **DEFERRED_TO_FINAL_VALIDATION**. Cenário humano documentado abaixo.
- Requires regression test: NO (feature nova, não bugfix).
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION.
- Minimum validation evidence for ACCEPTED: dotnet build PASS (atendido) + teste da lógica de decisão (atendido) + cenário de Play Mode documentado (atendido, ver abaixo).

### Cenário de Play Mode (humano, deferred)

```text
1. Abrir a cena que contém um LockedOreNodeInteractable (ou instanciar um via prefab/editor).
2. No Inspector, forçar _unlocked = true (não há gate de progressão real — é limitação conhecida e documentada).
3. Garantir InventoryManager wired (via RebindInventoryManager no wiring de cena, ou arrastar no Inspector).
4. Interagir com o nó (tecla de interação padrão do projeto).
5. Confirmar: item_material_copper_ore x1 aparece no inventário; feedback "Minério coletado!" exibido;
   InteractionPrompt muda para "Veia esgotada"; interação seguinte não entrega novo item nem duplica feedback.
6. Repetir com inventário cheio (encher slots antes): confirmar que NÃO depleta, feedback "Inventário cheio"
   exibido, e o nó permanece interagível (nova tentativa após liberar espaço deve funcionar).
```

---

## Honest status rationale

- **BUILD_VALIDATED** (não `ACCEPTED`/`Play Mode PASS`): o `dotnet build` passou para ambos assemblies (0 erros) e a lógica de decisão tem cobertura EditMode por código, mas:
  - Não há nó `LockedOreNodeInteractable` com `_unlocked = true` acessível ao jogador hoje em nenhuma cena (o gate de progressão real não existe — é explicitamente fora de escopo e citado como "spec futura" no próprio TODO original). Isso torna o comportamento **não observável em Play Mode** até uma spec futura implementar o gate. Este é um risco conhecido, já antecipado na seção 26 da spec, e não introduzido por esta implementação.
  - O Unity Test Runner (EditMode) não foi executado nesta sessão (ambiente sem Unity Editor interativo disponível) — os testes compilam mas não foram rodados/afirmados como PASS em runtime real. Isso é reportado como `NOT RUN` e não como PASS, em conformidade com `validation-truth`.
  - Nenhuma cena/prefab/asset `.unity`/`.prefab`/`.asset` foi editado — fora do escopo de arquivos permitidos.

---

## What was NOT done (explicit)

- **Gate de progressão** que define `_unlocked = true` — explicitamente fora de escopo (spec futura citada no TODO original do código). `_unlocked` continua um bool serializado sem lógica de ativação real.
- **Checagem de ferramenta (picareta/`ToolType.Pickaxe`)** — `ToolType.Pickaxe` existe no enum, mas não foi adicionada checagem; decisão explícita de manter escopo mínimo (YAGNI), conforme a própria spec permite.
- **Suporte a múltiplos tipos de minério por nó** — fora de escopo; ID único `item_material_copper_ore` mantido.
- **Atualização do gerador de cena** (`Assets/_Game/Scripts/Editor/SceneCreation/**`) para chamar `RebindInventoryManager` automaticamente — fora dos arquivos permitidos desta spec (`Editor/SceneCreation/**` listado como proibido "a menos que autorizado após confirmação de necessidade"; não foi confirmada necessidade nem autorizado). O método de wiring foi criado e documentado para consumo futuro por esse gerador.
- **Unity Test Runner (EditMode) execução real** — NOT RUN, motivo: sem Unity Editor interativo disponível nesta sessão. Documentado como residual risk.
- **Save/load de estado `_depleted`** — não implementado; campo local em memória apenas (sem persistência), conforme decisão documentada na seção "Existing systems audit" (evita acoplamento indevido ao `FarmResourceRefreshRuntime`). Isso é consistente com a spec (seção 23: "Does this add a save section? NO").

---

## Rollback

```text
Reverter Assets/_Game/Scripts/World/LockedOreNodeInteractable.cs para o TODO original (apenas log).
Remover Assets/_Game/Tests/EditMode/World/LockedOreNodeInteractableTests.cs.
Remover este execution report.
```
