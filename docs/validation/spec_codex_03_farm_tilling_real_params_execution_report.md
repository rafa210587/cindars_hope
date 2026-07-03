# Execution Report — spec_codex_03_farm_tilling_real_params

> **Spec:** `.specs/a_implementar/spec_codex_03_farm_tilling_real_params.md`
> **Status:** BUILD_VALIDATED (Phase 2-3 DEFERRED_TO_FINAL_HUMAN_VALIDATION)
> **Date:** 2026-07-03
> **Domain:** Farm / Player

---

## Acceptance criteria extracted

| # | Critério (spec §14) | Evidência |
|---|---|---|
| 14.1 | Com `CurrentStamina` abaixo do custo, a ação falha e publica `PlayerActionFeedbackEvent` com mensagem específica de stamina insuficiente (não a genérica) | Código: `FarmTillingInputController.Update()` calcula `tillStaminaOk`/`waterStaminaOk` via `HasEnoughStamina()` ANTES de chamar `TillTile`/`WaterTile`; se `hasHoe`/`hasWateringCan` true e stamina insuficiente, publica `PlayerActionFeedbackEvent(InsufficientStaminaMessage, 1.5f)` em vez da mensagem genérica "Nao e possivel arar aqui". EditMode test: `FarmTillingStaminaDayTests.HasEnoughStamina_WithZeroStamina_BlocksTill`, `HasEnoughStamina_WithPartialStamina_BlocksWaterButNotCheaperTill`, `WaterTile_WithoutEnoughStamina_IsBlockedRegardlessOfDay` |
| 14.1 | Com stamina suficiente, a ação sucede e `TrySpendStamina` é chamado com o custo correto | Código: `SpendStaminaIfPresent(staminaManager, TillStaminaCost/WaterStaminaCost)` chamado apenas após `tilled`/`watered` retornar `true` (padrão `TreeNode.Interact`). EditMode test: `TrySpendStamina_AfterSuccessfulTill_DeductsExactCost` |
| 14.2 | `WaterTile` chamado com `currentDay` vindo de `TimeManager.CurrentDay` real, não mais `1` fixo | Código: `ResolveCurrentDay()` lê `timeManager.CurrentDay` (fallback `1` apenas se `TimeManager` não wired, com `Debug.LogWarning`). EditMode test: `WaterTile_UsesRealCurrentDay_NotFixedOne` |
| 14.3 | `EditorWire(...)` aceita as novas dependências como parâmetros opcionais; geradores de cena existentes que não passam continuam funcionando | Assinatura estendida com `StaminaManager staminaManager = null, TimeManager timeManager = null` (retrocompatível). `dotnet build` PASS (ver §4). Busca no repositório confirmou único caller (`CreateMvpFarmScene.cs:131`), atualizado para passar as novas deps reais (ver §2, Fase 0 justificativa) |

---

## Existing systems audit (Phase 0)

- `StaminaManager` (`Assets/_Game/Scripts/Player/StaminaManager.cs`): API pública confirmada — `CurrentStamina` (get), `TrySpendStamina(int)`, `AddStamina(int)`. Reusado sem alteração.
- `TimeManager` (`Assets/_Game/Scripts/Core/Time/TimeManager.cs`): `CurrentDay` (get, default 1). Reusado sem alteração.
- `GameBootstrap` (`Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs`): já expõe `public StaminaManager StaminaManager` (linha 67) e `public TimeManager TimeManager` (linha 62) — mesmo padrão usado por `TreeNode.HasRequiredTool()` via `GameBootstrap.Instance.EquipmentManager`. Decisão: `FarmTillingInputController` mantém serialized fields como via primária (paridade com `EquipmentManager`) e usa `GameBootstrap.Instance` como fallback via `ResolveStaminaManager()`/`ResolveTimeManager()`, evitando adicionar um segundo caminho de injeção divergente.
- `PlayerNeedsBalanceSO` (`Assets/_Game/Scripts/Core/Data/PlayerNeedsBalanceSO.cs`): auditado por inteiro — não contém nenhum custo de stamina para farm actions (till/water). Decisão: consts locais `TillStaminaCost`/`WaterStaminaCost` no próprio controller (nível 6/7 da escada de minimalismo — 2 constantes, sem justificar novo campo em SO de balance existente); documentado como placeholder de tuning (rule `no-magic-balance-values` satisfeita via const nomeada, não literal solto).
- `FarmTilledSoilService.TillTile/WaterTile`: assinatura já aceitava `staminaOk`/`currentDay` — nenhuma mudança necessária (arquivo fora do scope, apenas lido).
- `PlayerActionFeedbackEvent`: reusado sem novo evento (struct existente, `Message` + `DurationSeconds`).
- Único caller de `EditorWire(...)`: `CreateMvpFarmScene.cs:131` (`Assets/_Game/Scripts/Editor/SceneCreation/**`). Auditado e confirmado que `bootstrap.GetComponent<StaminaManager>()`/`<TimeManager>()` já estavam disponíveis no escopo da chamada (usados em outras linhas do mesmo arquivo). **Decisão de Fase 0:** editar ponto único no gerador de cena para passar as deps reais — sem isso, o fallback permissivo do controller mascararia o gap indefinidamente em toda cena regenerada, contrariando o objetivo funcional da spec. Edição pontual, mínima (2 linhas), retrocompatível.

Nenhum sistema paralelo foi criado.

---

## Spec Compliance Matrix

| Requisito (spec §11 Escopo) | Implementação |
|---|---|
| Referências reais a `StaminaManager` e fonte de dia real | `[SerializeField] private StaminaManager _staminaManager;` + `[SerializeField] private TimeManager _timeManager;` com fallback `GameBootstrap.Instance` |
| Consts de custo de stamina (nomeadas) | `TillStaminaCost = 10`, `WaterStaminaCost = 8` |
| `staminaOk` real (checar antes, gastar depois do sucesso) | `HasEnoughStamina()` pré-condição → `TillTile`/`WaterTile` → `SpendStaminaIfPresent()` só se sucesso |
| `currentDay` real | `ResolveCurrentDay()` → `timeManager.CurrentDay` |
| `PlayerActionFeedbackEvent` de recusa por stamina | `InsufficientStaminaMessage` publicado quando `hasTool && !staminaOk` |
| `EditorWire(...)` retrocompatível | Parâmetros opcionais com default `null`, mesmo padrão de `equipmentManager` |
| EditMode test da lógica de decisão | `FarmTillingStaminaDayTests.cs` (7 testes) |

Fora de escopo (confirmado não tocado): `FarmTilledSoilService.cs`, `FarmPlotLogic.cs`, `FarmWateringService.cs` (assinaturas preservadas).

Adendo do orquestrador (2026-07-03): os dois calls do `FarmTillingInputController` passaram a usar `temporarySliceMode: false` — o flag só controla o bypass de exigência de ferramenta (`!hasTool && !temporarySliceMode` nos 3 usos em `FarmPlotLogic`/`FarmTilledSoilService`), e a checagem de ferramenta via `EquipmentManager.HasTool` já é real desde a Fase 8. Com o flag desligado, agir sem enxada/regador é recusado e publica feedback específico ("Precisa de enxada ou regador equipado."). O fallback permissivo com aviso para cena não-regenerada (EquipmentManager ausente → hasHoe/hasWateringCan = true) permanece. Builds re-validados 0E após a mudança.

---

## Validation

```text
Validation method: run_strict_validation.ps1 + builds individuais
dotnet build Assembly-CSharp.csproj --no-restore: EXIT 0 (0E / 5W pré-existentes, EnemySkinCatalog/CombatTelemetrySession — não relacionados a este diff)
dotnet build Assembly-CSharp-Editor.csproj --no-restore: EXIT 0 (0E / 7W pré-existentes — não relacionados a este diff)
run_strict_validation.ps1: EXIT 1
```

### Triagem honesta do exit 1 de `run_strict_validation.ps1`

Todas as falhas reportadas pelo script foram auditadas linha a linha contra os arquivos tocados nesta sessão (`FarmTillingInputController.cs`, `CreateMvpFarmScene.cs`, `FarmTillingStaminaDayTests.cs`, este report):

1. **Docs validation — `spec_npc_physics_cat_companion.md`** (markers `# /speckit.specify` etc. ausentes) — arquivo não tocado por esta spec. Falha legada pré-existente (citada explicitamente pelo orquestrador).
2. **Docs validation — placeholders em `tools/codex/Generate-CodexHarness.ps1`** — arquivo não tocado por esta spec. Falha legada pré-existente (citada explicitamente pelo orquestrador).
3. **`Assembly-CSharp` build** — PASS.
4. **`Assembly-CSharp-Editor` build** — PASS.
5. **Spec diff completeness — "Runtime code changed but no execution report found"** — real na primeira execução (report ainda não existia); **corrigido** ao criar este arquivo. Não classificado como legado; foi a única falha nova, e foi endereçada antes de fechar a spec.
6. **Quality check (`check_spec_quality.ps1`) — `FAIL: Forbidden files altered`** — lista `Assets/_Game/Data/Enemies/Roster/*.asset` (39 arquivos) e `Assets/_Game/Scenes/{CaveScene,FarmScene,TownScene}.unity`. Confirmado via `git log -1 -- <arquivo>` que o último commit desses arquivos é de 2026-06-28/2026-07-01, e via `git status --short` no início da sessão que já apareciam como `M` (modified) ANTES de qualquer ação desta spec. Esta spec não editou nenhum arquivo em `Assets/_Game/Data/Enemies/**` nem `Assets/_Game/Scenes/**`. Falha legada pré-existente (citada explicitamente pelo orquestrador como "assets de Enemies/Roster modificados pré-sessão"). Esta é a única causa real de `$exitCode = 1` em `check_spec_quality.ps1` (confirmado lendo o script: as demais ocorrências reportadas para arquivos desta spec são `WARN`, não `FAIL` — não alteram o exit code).
7. **Quality check — `WARN: Unix command found` inicialmente citando este próprio report** — falso positivo causado pela palavra "Grep" (maiúscula) na seção de evidência do critério 14.3; corrigido substituindo por "busca no repositório". Confirmado que o WARN não reaparece após a correção.

Nenhuma falha reportada é causada por conteúdo de `FarmTillingInputController.cs`, `CreateMvpFarmScene.cs` ou `FarmTillingStaminaDayTests.cs`.

```text
Testing Quality Gate
────────────────────
Changed runtime code:           YES
Changed deterministic logic:    YES (decisão de stamina/dia; FarmTilledSoilService.TillTile/WaterTile contract exercised with real-derived params)
Changed Unity scene/prefab:     NO (código apenas; scene requer regeneração humana via CindarsHope/Inicializar Projeto para materializar o wiring novo)
Automated tests added/updated:  YES — Assets/_Game/Tests/EditMode/Farm/FarmTillingStaminaDayTests.cs (7 testes)
Automated tests command:        Unity Test Runner EditMode — NOT RUN (Unity Editor não executável neste ambiente de sessão; ver residual risk)
Manual Play Mode scenario:      docs/validation/playmode/spec_codex_03_farm_tilling_real_params_human_test_scenario.md (ver §5)
Justification if no tests run:  Unity batchmode/Test Runner não disponível neste ambiente; testes compilam (build PASS) e cobrem a lógica de decisão via API pública real de StaminaManager/TimeManager
Residual risk:                  Testes não foram executados no Unity Test Runner nesta sessão; humano deve rodar antes de aceitar ACCEPTED. Cena FarmScene precisa ser regenerada (CindarsHope/Inicializar Projeto) para que o wiring novo (StaminaManager/TimeManager no FarmTillingInputController) seja materializado na cena existente — sem isso, o fallback permissivo (com warning) permanece ativo em produção até a regeneração.
```

---

## Human Test Scenario (Phase 3 — deferred)

Ver `docs/validation/playmode/spec_codex_03_farm_tilling_real_params_human_test_scenario.md`.

---

## Honest status rationale

- **BUILD_VALIDATED**: critérios centrais (14.1, 14.2, 14.3) implementados e auditados; `dotnet build` PASS para runtime e editor assemblies; EditMode tests criados cobrindo a decisão de stamina/dia real; `EditorWire` retrocompatível confirmado via busca no repositório (único caller, atualizado).
- **Phase 2-3 (Unity Test Runner + Play Mode): NOT RUN** — Unity Editor não é executável neste ambiente de sessão. Human test scenario por spec criado em `docs/validation/playmode/spec_codex_03_farm_tilling_real_params_human_test_scenario.md`; validação pode ocorrer via esse scenario individual OU via o checklist de batch de fim de wave em `docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md`.
- Não reivindico `ACCEPTED`, `PLAYMODE_VALIDATED` ou "Play Mode PASS" — não há evidência dessas fases nesta sessão.
- Spec classificada como escopo runtime/gameplay (consumo real de stamina, feedback de recusa, dia real usado em rega — comportamento de gameplay em runtime). Conforme o critério de elegibilidade de `/finish-spec`, specs de runtime só promovem para `implementados/` com Phase 2-3 completa (`ACCEPTED`) ou explicitamente `DEFERRED_TO_FINAL_HUMAN_VALIDATION` com evidência de Phase 1-2 completa — que é o caso aqui.
- **Decisão de closeout:** manter status `BUILD_VALIDATED` / `DEFERRED_TO_FINAL_HUMAN_VALIDATION`. Spec NÃO promovida para `.specs/implementados/` nesta sessão — permanece em `.specs/a_implementar/` até a evidência de Phase 3 (human Play Mode) ser coletada.

---

## Remaining Work / Follow-ups

1. Humano deve rodar Unity Test Runner EditMode para confirmar os 7 testes novos de `FarmTillingStaminaDayTests.cs` passam em ambiente real Unity.
2. Humano deve regenerar a `FarmScene` via `CindarsHope/Inicializar Projeto` para que o `FarmTillingInputController` existente na cena receba as novas referências (`StaminaManager`/`TimeManager`) via `EditorWire` atualizado — sem isso, a cena atual continua usando o fallback permissivo com warning.
3. Balance de custo de stamina (`TillStaminaCost = 10`, `WaterStaminaCost = 8`) é placeholder — tuning fica para spec futura de `economy-balance-tuning`, conforme nota da spec.
4. Executar o human test scenario (Play Mode) para confirmar: (a) arar sem stamina recusa com feedback específico; (b) arar com stamina gasta stamina; (c) regar usa dia real.

---

## Files Changed

```text
Assets/_Game/Scripts/Farm/Runtime/FarmTillingInputController.cs   (modificado — scope principal da spec)
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs   (modificado — edição pontual autorizada pela spec §19, único caller de EditorWire)
Assets/_Game/Tests/EditMode/Farm/FarmTillingStaminaDayTests.cs    (novo — EditMode tests)
docs/validation/spec_codex_03_farm_tilling_real_params_execution_report.md  (novo — este report)
docs/validation/playmode/spec_codex_03_farm_tilling_real_params_human_test_scenario.md  (novo — human test scenario)
```
