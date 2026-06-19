---
doc_type: validation
status: evidence
spec_id: fable_69_spec_combat_sprint_runtime
validation_type: automated
result: BUILD_VALIDATED_WITH_WARNINGS
date: 2026-06-19
executor: Claude Code
source_of_truth: false
validated_adrs: []
validated_game_rules: [combat_rules.md]
---

# Validation Report — fable_69 Player: Sprint em Combate

> **This report is evidence, NOT an execution queue.**
> **Do not re-run the spec based on this report alone.**

Sprint segurável (Left Ctrl) que mantém a velocidade de fora-de-combate DENTRO de combate,
ao custo de 8 stamina/s, via fator nomeado no `PlayerSpeedComposer` (F47). Cancelado por
stun (F01), modal e falta de stamina; no-op silencioso fora de combate.

---

## Acceptance criteria extracted

| ID | Critério (spec) | Implementação | Evidência | Status |
|----|------------------|---------------|-----------|--------|
| CA-1 | Em combate, segurar sprint mantém velocidade plena e consome 8 stamina/s; soltar restaura a penalidade de combate | `PlayerSprintController` aplica fator `CombatMobility=1.0` ao sprintar (e `=0.9` ao soltar, ainda em combate); drena 8/s via `SprintRules.AdvanceDrain` (acumulador, padrão do block) | EditMode: `Mobility_InCombatSprinting_RestoresFullSpeed`, `Mobility_ReleaseSprint_RestoresCombatPenalty`, `Drain_OneFullSecond_SpendsEight`, `Drain_FractionalAccumulates_NoLoss`, `Composer_SprintFactorRestoresFullSpeed_OverCombatPenalty` | OK |
| CA-2 | Sem stamina → sprint cai sozinho; Stun/modal cancelam; fora de combate é no-op | `ShouldSprint` retorna false p/ `!hasStamina`, `stunned`, `modalOpen`, `!inCombat`; `DrainStamina` chama `StopSprint()` quando `TrySpendStamina` falha; fora de combate o controller limpa o fator | EditMode: `ShouldSprint_NoStamina_IsFalse`, `ShouldSprint_Stunned_IsFalse`, `ShouldSprint_ModalOpen_IsFalse`, `ShouldSprint_OutOfCombat_IsFalse_NoOp`, `Mobility_OutOfCombat_IsFull_NoOp` | OK |

---

## Existing systems audit (Phase 0 — system-reuse)

| Sistema | Existe? | Encontrado em | Decisão | Nota |
|---------|---------|---------------|---------|------|
| `PlayerSpeedComposer` (fator nomeado) | SIM (F47) | `Player/Movement/PlayerSpeedComposer.cs` | REUSADO | Adicionado `SpeedFactorKind.CombatMobility`; nenhuma escrita direta em `SpeedMultiplier` |
| Padrão de drenagem por acumulador | SIM | `Player/Movement/PlayerBlockController.cs` (18 STA/s) | COPIADO | `SprintRules.AdvanceDrain` espelha `DrainStamina` (FloorToInt + resto no acumulador) |
| `StaminaManager.TrySpendStamina` | SIM | `Player/StaminaManager.cs` | REUSADO | Mesmo gasto inteiro; mesmo fallback "sem manager = debt" do block |
| Detecção "em combate" / `CombatStateTracker` | **NÃO** | (busca `InCombat`/`CombatState` = 0 arquivos) | CRIADO LEVE | `Combat/CombatStateTracker.cs` consome `DamageAppliedEvent` (dano dado) + `PlayerDamagedEvent` (dano recebido); janela 4s |
| Stun bloqueia ação (F01) | SIM | `Combat/StatusEffect/PlayerStatusReceiver.cs` (`IsActionBlocked`) | REUSADO | Consultado para cancelar sprint sob Stun/Root |
| Penalidade de velocidade de combate (3.4-3.8) | **NÃO (canon sem runtime)** | `docs/.../COMBAT_CORE_DIRECTION.md` linhas 310-311, 1252-1253 | MODELADO no fator | Sprint é o único dono de `CombatMobility`: penalidade 0.9 quando em combate sem sprint, 1.0 sprintando, ausente fora de combate |
| Modal cancela ação | SIM | `GameBootstrap.ModalManager.HasActiveModal` | REUSADO | Mesmo check do block |
| Tecla livre (input_map F67) | SIM | `docs/game_rules/input_map.md` | Left Ctrl | Livre (não-canônica e não-debug); sem conflito com Space=dodge, LeftShift=block, duplo-toque=dash |

**Conclusão:** nenhum sistema paralelo criado. Um único tracker leve novo (não existia) e um
fator novo no composer existente. Velocidade exclusivamente via composer.

---

## Spec Compliance Matrix

| Requisito (spec) | Implementação | Status |
|------------------|---------------|--------|
| `PlayerSprintController` em `Player/Movement/` | Criado | OK |
| Hold tecla → se InCombat e stamina>0, registra fator "sprint" no composer | `Update()` → `ShouldSprint` → `ApplyMobilityFactor(1.0)` | OK |
| Fator neutraliza a penalidade de combate (resultado 3.8-4.2) | `CombatMobility=1.0` sprintando vs `0.9` (≈3.6/4.0) sem sprint | OK |
| Drena 8 stamina/s (acumulador) | `SprintRules.StaminaDrainPerSecond = 8` + `AdvanceDrain` | OK |
| Solta/sem stamina → remove | `StopSprint()` + fator volta à penalidade (em combate) ou some (fora) | OK |
| `CombatStateTracker` leve SE não existir (dano dado/recebido nos últimos 4s) | Criado; `CombatWindowRules.CombatWindowSeconds = 4` | OK |
| Fora de combate: no-op silencioso | `if (!inCombat) { StopSprint(); ClearMobilityFactor(); return; }` | OK |
| Stun (F01) e modal cancelam | `PlayerStatusReceiver.IsActionBlocked` + `ModalManager.HasActiveModal` em `ShouldSprint` | OK |
| Fator nomeado no composer, nunca escrita direta em `SpeedMultiplier` | Só `SetFactor/ClearFactor(SpeedFactorKind.CombatMobility)` | OK |
| Consome `DamageAppliedEvent`/`PlayerDamagedEvent` (existentes), nenhum evento novo | `CombatStateTracker` subscreve ambos; zero novos eventos | OK |
| Block/dash/dodge inalterados; sem novo i-frame | Nenhum arquivo de block/dash/dodge editado; sem i-frame | OK |
| EditMode tests: drenagem/s, fator aplicado/removido, no-op fora de combate, cancelamento por stun/stamina | `SprintTests.cs` (24 testes) | OK |

---

## What Was Run

- [x] dotnet build Assembly-CSharp.csproj --no-restore
- [x] dotnet build Assembly-CSharp-Editor.csproj --no-restore
- [x] tools/docs/validate_docs.ps1
- [x] tools/docs/check_spec_diff_completeness.ps1
- [x] tools/docs/run_strict_validation.ps1
- [ ] Unity validators (N/A — sem asset/scene/prefab novo)
- [ ] Play Mode checklist (DIFERIDO — autorizado pelo dono; feel de sprint validável só em Play Mode)

## What Was NOT Run

- [ ] Unity Editor / Play Mode — autorizado pular (DIFERIDO). Sprint depende de input em tempo
      real + integração de cena; lógica determinística coberta por EditMode.
- [ ] EditMode no Unity Test Runner — não executável fora do Editor; testes compilam em
      `Assembly-CSharp` (build PASS) e usam apenas C# puro/NUnit.

---

## Validation

```
Validation method: builds + validate_docs + check_spec_diff_completeness + run_strict_validation
Assembly-CSharp: PASS (exit 0, 0E/0W)
Assembly-CSharp-Editor: PASS (exit 0, 0E/3W pré-existentes)
Docs validation: PASS (validate_docs.ps1 exit 0)
Spec diff completeness: PASS (check_spec_diff_completeness.ps1 exit 0)
Strict validation: EXIT 1 — HARNESS_BUG (check_spec_quality regex ^## sem multiline reprova todo report); builds+docs+diff limpos, nada novo
```

## Results

| Check | Result | Notes |
|-------|--------|-------|
| C# runtime build | PASS — 0E/0W | `Assembly-CSharp.csproj`, exit 0 |
| C# editor build | PASS — 0E/3W | `Assembly-CSharp-Editor.csproj`, exit 0; 3 warnings pré-existentes (CreateEnemyActionsAndSets, CSharpProjectPostprocessor — não tocados) |
| Docs validation | PASS | `validate_docs.ps1` exit 0 |
| Spec diff completeness | PASS | `check_spec_diff_completeness.ps1` exit 0 (após este report) |
| Strict validation | EXIT 1 — HARNESS_BUG | `run_strict_validation.ps1` reprova por bug conhecido do `check_spec_quality` (regex `^##` sem multiline reprova todo report); builds+docs+diff limpos; nada novo |
| Unity validators | NOT RUN | Sem asset/scene novo a validar |
| Play Mode | NOT RUN (DEFERRED) | Autorizado pelo dono; cenário de feel |

---

## ADRs / Game Rules Validated

| Item | Status | Notes |
|---|---|---|
| combat_rules.md | PASS (consistência) | Sprint respeita o canon de combate; não altera dano/status; usa eventos canônicos de dano para a janela de combate |
| COMBAT_CORE_DIRECTION.md | PASS (consistência) | Faixas 3.8-4.2 (fora) / 3.4-3.8 (combate) modeladas no fator `CombatMobility` (1.0 vs 0.9) |

---

## Testing Quality Gate

```
Changed runtime code: YES
Changed deterministic logic: YES (janela de combate, decisão de sprint, fator de mobilidade, drenagem por acumulador)
Changed Unity scene/prefab/asset wiring: NO
Automated tests added/updated: YES (Assets/_Game/Tests/EditMode/Player/SprintTests.cs — 24 testes)
Automated tests command: dotnet build .\Assembly-CSharp.csproj --no-restore (compila os testes; exit 0). Unity Test Runner NOT RUN (sem Editor).
Manual Play Mode scenario: NOT REQUIRED para BUILD_VALIDATED; feel de sprint DEFERIDO ao Play Mode final (autorizado)
Justification if no automated tests: N/A (testes adicionados)
Residual risk: feel real (sensação de mobilidade, sobreposição com block/dash em runtime, leitura de Left Ctrl no Input System) só verificável em Play Mode; a penalidade de combate (0.9) é introduzida por este controller e só se manifesta em combate — caso uma spec futura adicione uma penalidade de combate separada, deduplicar para o fator único CombatMobility.
```

---

## Errors Found

```
(nenhum — 0 erros de compilação em ambos os assemblies)
```

## Warnings (pre-existing)

```
CreateEnemyActionsAndSets.cs(45,36) CS0649 RequiresLos — pré-existente, arquivo não tocado
CreateEnemyActionsAndSets.cs(36,55) CS0649 MinRange — pré-existente, arquivo não tocado
CSharpProjectPostprocessor.cs(18,28) UNT0006 — pré-existente, arquivo não tocado
```

---

## Evidence

Files changed:
```
Assets/_Game/Scripts/Combat/CombatStateTracker.cs                (NOVO)
Assets/_Game/Scripts/Player/Movement/PlayerSprintController.cs    (NOVO)
Assets/_Game/Scripts/Player/Movement/SpeedFactorKind.cs           (+ CombatMobility = 7)
Assets/_Game/Scripts/Player/Movement/PlayerMovementActionInput.cs (+ IsSprintHeld — Left Ctrl)
Assets/_Game/Tests/EditMode/Player/SprintTests.cs                 (NOVO — 24 testes)
Assembly-CSharp.csproj                                            (+ 3 Compile Include)
docs/validation/fable_69_spec_combat_sprint_runtime_execution_report.md (este report)
```

Files NOT changed (protected / out of scope):
```
PlayerController.cs                  (não editado — sem escrita direta em SpeedMultiplier)
PlayerBlockController.cs             (não editado — padrão copiado, não alterado)
PlayerDodgeController.cs / dash      (não editado — sem novo i-frame)
StaminaManager.cs                    (não editado — só consumido)
*.unity / *.prefab / *.asset         (nenhum)
Packages/** / ProjectSettings/**     (nenhum)
.claude/rules/build_validation_truth_gate.md (não tocado)
```

---

## Honest status rationale

**Status: BUILD_VALIDATED_WITH_WARNINGS.**

- Núcleo (CA-1, CA-2) implementado e auditado; Spec Compliance Matrix toda OK.
- Ambos os assemblies compilam (0E); editor com 3 warnings pré-existentes não relacionados.
- `validate_docs.ps1` e `check_spec_diff_completeness.ps1` exit 0.
- `run_strict_validation.ps1` retorna exit 1 por **HARNESS_BUG conhecido** do `check_spec_quality.ps1`
  (regex `^##` sem flag multiline reprova todos os reports). Não é falha desta spec: builds, docs
  e diff-completeness estão limpos e nenhum erro novo foi introduzido.
- `_WITH_WARNINGS`: PlayMode/feel deferido (autorizado pelo dono). EditMode no Test Runner não
  executado fora do Editor — testes compilam no build e são C# puro/NUnit.
- Não promovido a `implementados/`. Sem Play Mode, não há claim de `ACCEPTED`/`PLAYMODE_VALIDATED`.

---

## Phase Status

| Phase | Status | Date |
|-------|--------|------|
| Phase 0 (Audit) | COMPLETE | 2026-06-19 |
| Phase 1 (Automated) | PASS (builds 0E + docs + diff exit 0; strict = HARNESS_BUG) | 2026-06-19 |
| Phase 2 (Unity validators) | NOT RUN (sem asset/scene novo) | — |
| Phase 3 (Play Mode) | DEFERRED (autorizado) | — |

---

## Remaining work

```
- Play Mode (feel): segurar Left Ctrl em combate mantém a velocidade plena, drena ~8/s,
  cancela ao soltar / sob stun / com modal aberto; conferir leitura de Left Ctrl no Input System ativo.
- (Se uma spec futura adicionar penalidade de combate separada) deduplicar para o fator único CombatMobility.
```

---

## Next Action

```
Humano: executar cenário de feel em Play Mode (sprint em combate) antes de qualquer promoção a ACCEPTED.
Permanece BUILD_VALIDATED_WITH_WARNINGS; não mover para implementados/.
```

---

*Report generated: 2026-06-19 (fable_69)*
