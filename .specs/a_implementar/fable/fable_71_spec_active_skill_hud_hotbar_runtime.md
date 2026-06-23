# SPEC — HUD de Active Skills (Hotbar 1–4 com Cooldown, Custo e Botão de Uso)

> **Spec ID:** `fable_71_spec_active_skill_hud_hotbar_runtime`
> **Status:** A implementar
> **Wave:** WAVE FABLE — Skill System Polish
> **Priority:** P1
> **Type:** UI + Runtime
> **Domain:** UI / Player / Skills
> **Parallelizable:** CONDITIONAL
> **Parallel group:** skills-polish
> **Can run with:** specs que não toquem `Assets/_Game/Scripts/UI/HUD/**` nem o controller de skill
> **Must not run with:** `fable_70` (precede esta), specs que alterem `GameplayHudViewModel`/`GameplayHudRuntimeBinder`/`ActiveSkillExecutionController`
> **Repo lock scope:** `Assets/_Game/Scripts/UI/HUD/**`, `ActiveSkillExecutionController.cs` (só leitura de cooldown), GameplayHudCanvas
> **Depends on:**
> - `fable_70` (catálogo saneado + custo cobrado; sem isso a HUD exibe slots quebrados)
> - `docs/design/SKILL_CATALOG_DESIGN_REVIEW_v1.0.md`
> **Blocks:** nenhuma
> **Scope:** Exibir os 4 active slots na HUD com ícone, cooldown (fill radial + segundos), custo (stamina/mana), estado bloqueado, e um botão clicável que dispara a mesma execução das teclas 1–4.
> **Out of scope:** rebind de teclas, gamepad, cooldown persistente entre cenas, animação de cast/channel.

---

# /speckit.specify

## 5. Contexto

O lado runtime das active skills está ~85% pronto: input 1–4, resolução, execução, cooldown interno e feedback funcionam (`ActiveSkillExecutionController`). O lado UI está ~30%: o ViewModel `ActiveSkillSlotViewModel` já tem os campos (`Cooldown`, `CostMp`, `CostStamina`, `IsUnlocked`, `IsEquipped`, `IsUsableInContext`, `BlockedReason`), mas o `GameplayHudRuntimeBinder` só preenche `SlotIndex`/`SkillId`/`IsEquipped`; a view (`ActiveSkillSlotsHudView`) é headless (sem botões, sem cooldown visual). Esta spec fecha o "DEFERRED: binding do canvas" para as active skills.

## 6. Problema

Sem a HUD, o jogador não vê o que equipou, quanto custa, nem se está em cooldown — e não há botão de uso (só as teclas 1–4, sem affordance visual). O cooldown vive privado em `_slotCooldowns[4]` e nunca chega à tela. Isso torna o sistema de skills opaco e pouco usável, especialmente em telas touch/sem teclado numérico.

## 7. Objetivo

Ao final desta spec, a HUD deve mostrar 4 slots com ícone, estado (pronto/cooldown/bloqueado/sem recurso), fill radial + segundos de cooldown e custo; e clicar num slot deve disparar a **mesma** execução das teclas 1–4 (sem duplicar a lógica de execução). Sem alterar a lógica de catálogo nem o save schema.

## 8. Fontes obrigatórias lidas

```text
docs/design/SKILL_CATALOG_DESIGN_REVIEW_v1.0.md
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
.claude/rules/testing-quality-gate.md
.claude/skills/ui-projection-pattern/SKILL.md
.claude/skills/hud-canvas-binding/SKILL.md
.claude/skills/input-gamepad-routing/SKILL.md
.claude/skills/action-feedback-pipeline/SKILL.md
```

## 9. Estado atual do repo

| Item | Estado | Path |
|---|---|---|
| ViewModel de slot ativo | Campos definidos, **não preenchidos** | `Assets/_Game/Scripts/UI/HUD/HotbarSlotViewModel.cs` (`ActiveSkillSlotViewModel`) |
| ViewModel da HUD | `List<ActiveSkillSlotViewModel> ActiveSkillSlots` | `…/UI/HUD/HUDGameplayViewModel.cs` |
| Binder | Preenche só SlotIndex/SkillId/IsEquipped | `…/UI/HUD/GameplayHudRuntimeBinder.cs` |
| View (canvas) | Headless (sem botões/cooldown) | `…/UI/HUD/Views/ActiveSkillSlotsHudView.cs` |
| Canvas controller | Existe | `…/UI/HUD/GameplayHudCanvasController.cs` |
| Guard validator | max 4 slots; sem Dash/Dodge/Block puros | `…/UI/HUD/FinalHudGuardValidator.cs` |
| Execução (cooldown privado) | Funciona; cooldown não exposto | `…/Skills/Runtime/Effects/ActiveSkillExecutionController.cs` (`_slotCooldowns`) |

> Fase 0 deve confirmar onde estão custo e cooldown autoritativos pós-fable_70 (custo provavelmente no nó/ação; cooldown no controller). Não recriar ViewModel — preencher o existente.

## 10. Engineering stories

```text
Como jogador, quero ver minhas 4 skills equipadas com ícone e cooldown, para saber o que posso usar.
Como jogador, quero clicar num slot para usar a skill, além das teclas 1–4.
Como jogador, quero ver custo e "sem mana/stamina" para entender por que falhou.
Como UI, quero ler cooldown/custo do runtime sem duplicar a lógica de execução.
```

## 11. Escopo

```text
Inclui:
- Expor cooldown do controller (API pública read-only: GetSlotCooldown(i)/SlotCooldownSeconds e o total para fill).
- Binder preenche Cooldown, CostStamina, CostMp, IsUnlocked, IsUsableInContext, BlockedReason a cada refresh.
- View com 4 botões: ícone, overlay de cooldown (Image.fillAmount radial) + label de segundos, label/ícone de custo, estado bloqueado (cinza/locked).
- Clique no slot → dispara a MESMA rota de TryExecuteSlot(i) do controller (expor método público de execução por slot; teclas e clique convergem).
- BlockedReason mapeado: vazio/locked/cooldown/sem recurso.
- Respeitar FinalHudGuardValidator (max 4; sem Dash/Dodge/Block puros).
```

## 12. Fora de escopo

```text
Não inclui:
- Rebind/configuração de teclas; gamepad.
- Cooldown persistente entre cenas.
- Animação de cast/channel/charge na HUD (charge é tratado no controller/skill em fable_70).
- Drag-and-drop de skills nos slots (equipar é na skill tree, U).
- Arte final de ícones (placeholder/ID de ícone ok).
```

## 13. Regras de não duplicação

```text
Não duplicar a lógica de execução — clique e tecla chamam o mesmo método do controller.
Não criar segundo ViewModel — preencher ActiveSkillSlotViewModel existente.
Não criar segundo cálculo de cooldown — ler do controller (fonte única).
Não ler estado por GameObject.Find — usar GameBootstrap/refs serializadas (ui-projection-pattern).
```

## 14. Critérios de aceite

### 14.1 Cooldown visível e correto
- Usar uma skill mostra fill radial decrescente + segundos; ao zerar, o slot fica "pronto".
- Cooldown lido do controller (fonte única), não recalculado na UI.
- Evidência: EditMode test de projeção (cooldown restante → fillAmount/label); Play Mode no lote final.

### 14.2 Botão de uso
- Clicar num slot equipado e pronto executa a skill (mesmo efeito da tecla numérica); slot vazio/cooldown/bloqueado dá o feedback correto e não executa.
- Evidência: EditMode test do mapeamento clique→TryExecuteSlot; Play Mode final.

### 14.3 Custo e bloqueio
- Slot mostra custo (stamina/mana) e, quando inutilizável, o `BlockedReason` correto (vazio/locked/cooldown/sem recurso).
- Evidência: EditMode test de projeção de BlockedReason por cenário.

### 14.4 Guard respeitado
- `FinalHudGuardValidator.Validate` retorna sem erros para a projeção final (≤4 slots; sem IDs proibidos).
- Evidência: test existente estendido.

---

# /speckit.plan

## 15. Arquitetura alvo

```text
Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutionController.cs
  (EDIT — API pública: bool TryUseSlot(int) [renomeia/exposição de TryExecuteSlot];
          float GetSlotCooldownRemaining(int); float GetSlotCooldownTotal(int))

Assets/_Game/Scripts/UI/HUD/
  GameplayHudRuntimeBinder.cs       (EDIT — RefreshActiveSkillSlots preenche todos os campos)
  Views/ActiveSkillSlotsHudView.cs  (EDIT — 4 botões: ícone, cooldown overlay, custo, blocked)
  ActiveSkillSlotProjection.cs      (NOVO opcional — projeção pura C# do estado do slot p/ EditMode)

Assets/_Game/Tests/EditMode/UI/
  ActiveSkillSlotProjectionTests.cs (NOVO)

GameplayHudCanvas (cena/prefab)     (binding via hud-canvas-binding — pode exigir autorização)

docs/validation/fable_71_execution_report.md
docs/validation/playmode/fable_71_human_test_scenario.md
```

## 16. Contratos, dados e eventos

### 16.1 Data contracts — N/A (sem novo dado persistido).
### 16.2 Runtime contracts
- Controller expõe leitura de cooldown e um método único de uso por slot. Teclas 1–4 e clique chamam o mesmo método.
### 16.3 Event contracts
- Reusar `PlayerActionFeedbackEvent` para feedback de falha de clique. Opcional: assinar evento de cooldown se preferir push em vez de poll (auditar; poll por Update já é suficiente).
### 16.4 Save contracts
```text
Changes save schema? NO · Adds section? NO · Migration? NO · Unity refs? NO
```
### 16.5 UI contracts
- `ActiveSkillSlotViewModel` preenchido por completo; View consome o ViewModel (projection pattern), sem lógica de gameplay na View.

## 17. Sistemas afetados
```text
HUD (binder/view/canvas), Skills (controller — exposição read-only), Input (clique converge com teclas),
EditMode tests, Validation reports.
```

## 18. Arquivos permitidos
```text
Assets/_Game/Scripts/UI/HUD/**
Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutionController.cs  (apenas exposição read-only + método único de uso)
Assets/_Game/Tests/EditMode/UI/**
docs/validation/**
```

## 19. Arquivos proibidos
```text
Assets/**/*.unity, Assets/**/*.prefab  (sem autorização; binding de canvas via hud-canvas-binding pode exigir aprovação → registrar)
Assets/_Game/Scripts/Skills/DefaultSkillCatalog.cs  (catálogo é fable_70)
docs_old/**, docs/archive/**, Packages/**, ProjectSettings/**
```

## 20. Estratégia de implementação
```md
### Fase 0 — Auditoria
- Confirmar custo/cooldown autoritativos pós-fable_70; confirmar que TryExecuteSlot pode virar público sem quebrar input.

### Fase 1 — Exposição read-only no controller
- API pública de cooldown (restante + total) e método único TryUseSlot(i); teclas passam a chamar TryUseSlot.

### Fase 2 — Projeção + binder
- Projeção pura (BlockedReason/cooldown/custo/usável) testável em EditMode; binder preenche o ViewModel.

### Fase 3 — View/canvas
- 4 botões com ícone, cooldown overlay (fillAmount + label), custo, estado bloqueado; OnClick → TryUseSlot(i).
- Binding na GameplayHudCanvas (hud-canvas-binding); se exigir prefab/scene, registrar autorização/evidência.

### Fase 4 — Testes + validação
- EditMode da projeção; guard estendido; Play Mode scenario.

### Fase 5 — Report.
```

## 21. Ordem segura de execução
```text
1. Auditoria. 2. Exposição no controller (teclas convergem). 3. Projeção+binder. 4. View+canvas. 5. Tests+validação. 6. Report.
```

## 22. Paralelização
```md
- Parallelizable: CONDITIONAL
- Parallel group: skills-polish
- Can run with: specs fora de UI/HUD e fora do controller
- Must not run with: fable_70 (precede), specs que mexam no ViewModel/binder/controller
- Shared files/systems that require lock: Assets/_Game/Scripts/UI/HUD/**, ActiveSkillExecutionController.cs
- Reason: edita HUD central e expõe API do controller.
```

## 23. Impacto em save/load
```text
Changes save schema? NO · Adds section? NO · Migration? NO · Persists Unity refs? NO
```

## 24. Impacto em eventos
```text
Adds events? NO (poll de cooldown via Update) — opcional evento de cooldown se preferir push.
Changes existing events? NO · Requires unsubscribe pattern? YES se assinar algo (HUD deve desinscrever em OnDisable).
```

## 25. Impacto em UI/Unity
```text
Changes UI? YES · Changes scenes? CONDITIONAL (binding do canvas) · Changes prefabs? CONDITIONAL
Changes ScriptableObjects/assets? NO
Requires Play Mode final validation? YES (clique, cooldown visual, blocked states)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## 26. Riscos técnicos
```text
Risco: binding de canvas exige scene/prefab. Mitigação: hud-canvas-binding com autorização/evidência; lógica em projeção pura testável sem cena.
Risco: poll de cooldown por Update custa por frame. Mitigação: leitura O(4) trivial; sem alloc/LINQ no Update.
Risco: clique e tecla divergirem. Mitigação: ambos chamam TryUseSlot — único ponto de verdade.
Risco: tornar TryExecuteSlot público vazar API. Mitigação: expor só TryUseSlot(i) + getters de cooldown.
```

## 27. Rollback
```text
Reverter binder/view/controller-exposição e tests. Manter ViewModel intacto. Sem impacto em save.
```

# /speckit.tasks

## 28. Tasks
```md
- [ ] T001 — Fase 0: auditar custo/cooldown autoritativos e viabilidade de expor TryUseSlot.
- [ ] T002 — Controller: API pública GetSlotCooldownRemaining/Total + TryUseSlot(i); teclas 1–4 chamam TryUseSlot.
- [ ] T003 — Projeção pura ActiveSkillSlotProjection (BlockedReason/cooldown/custo/usável).
- [ ] T004 — Binder: preencher Cooldown/CostStamina/CostMp/IsUnlocked/IsUsableInContext/BlockedReason.
- [ ] T005 — View: 4 botões (ícone, cooldown overlay+label, custo, blocked); OnClick→TryUseSlot.
- [ ] T006 — Binding na GameplayHudCanvas (hud-canvas-binding) com evidência/autorização se necessário.
- [ ] T007 — EditMode tests de projeção + guard estendido.
- [ ] T008 — Play Mode human scenario (docs/validation/playmode/).
- [ ] T009 — Execution report.
```

## 29. Validações obrigatórias
```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
.\tools\docs\run_strict_validation.ps1   # exit 0
```
EditMode (Unity Test Runner). Binding de canvas: evidência se tocar scene/prefab. `NOT RUN` com motivo se algum comando não rodar.

## 30. Testing Quality Gate
```md
- Changed deterministic logic: YES (projeção de slot)
- Requires EditMode tests: YES (projeção BlockedReason/cooldown/custo; guard)
- Requires PlayMode automated or final human scenario: YES (clique, cooldown visual, blocked) → human scenario
- Requires regression test: NO
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: EditMode PASS + Play Mode scenario executado no lote final.
```

## 31. Definition of Done
```text
4 slots na HUD com ícone, cooldown (fill+segundos), custo e estado bloqueado.
Clique no slot usa a skill via TryUseSlot (mesma rota das teclas).
Cooldown lido do controller (fonte única); guard sem erros.
EditMode PASS; build PASS; docs/strict validation PASS.
Play Mode scenario documentado. Sem claim ACCEPTED sem Play Mode final.
```

## 32. Anti-regressão
```text
Não duplicar lógica de execução (clique e tecla convergem).
Não recalcular cooldown na UI.
Não usar GameObject.Find/FindObjectOfType na HUD.
Não deixar gameplay input ativo durante modal (input-gamepad-routing).
Não exceder 4 slots nem aceitar Dash/Dodge/Block puros (guard).
```

## 33. Notas para execução posterior
```text
Gamepad e rebind ficam para input-gamepad-routing futuro.
Cooldown persistente entre cenas é decisão futura (hoje zera ao trocar cena).
Se o binding de canvas exigir scene/prefab, registrar autorização e evidência (hud-canvas-binding).
```
