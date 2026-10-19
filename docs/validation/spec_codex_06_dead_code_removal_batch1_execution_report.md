# Execution Report — spec_codex_06_dead_code_removal_batch1

> **Spec:** `.specs/a_implementar/spec_codex_06_dead_code_removal_batch1.md`
> **Status:** BUILD_VALIDATED
> **Type:** Governance / Runtime cleanup (pure file deletion, no new logic)
> **Date:** 2026-07-03

> **Nota sobre "PowerShell retry":** este report usa a palavra "Grep" no sentido do tool de busca
> (Grep tool / capability), não do binário Unix `grep`. Nenhum comando de shell Unix foi executado
> nesta spec — toda a verificação de referências foi feita via Grep tool (equivalente PowerShell-safe,
> não subprocess), e os builds/validação rodaram via `dotnet build` e `run_strict_validation.ps1`
> (PowerShell), conforme a rule `windows_powershell_only`. ENV_COMMAND_RETRY_REQUIRED: N/A.

---

## Acceptance criteria extracted

| # | Critério (spec seção 14) | Evidência |
|---|---|---|
| 14.1 | Deleção nominal autorizada dos 11 arquivos listados na seção 9 | Executado — ver seção 3 deste report |
| 14.2 | Build limpo pós-deleção: `Assembly-CSharp` e `Assembly-CSharp-Editor` exit 0, 0 erros novos | PASS — ver seção 4 |
| 14.3 | Grep de confirmação: zero referências `.cs` remanescentes aos 11 nomes de classe | PASS — ver seção 2 (pré) e seção 5 (pós) |
| 14.4 | Itens excluídos documentados (`StatusEffectManager` Combat, `City/Schedule/**`) | Ver seção 6 |

---

## Existing systems audit (re-verificação Phase 0, antes de deletar)

Cada um dos 11 arquivos foi re-checado com `Grep` sobre `Assets/**` (cobre `.cs`, `.unity`, `.prefab`, `.asset`) **antes** de qualquer deleção:

| # | Arquivo | Matches encontrados | Veredito |
|---|---|---|---|
| 1 | `Cave/CaveDebugVisualizer.cs` | 1 (própria declaração) | CONFIRMADO morto |
| 2 | `Combat/EnemyPatrolController.cs` | 1 (própria declaração) | CONFIRMADO morto |
| 3 | `Combat/TargetVulnerabilityState.cs` | 1 (própria declaração) | CONFIRMADO morto |
| 4 | `Player/EnvironmentalExposureManager.cs` | 1 (própria declaração) | CONFIRMADO morto |
| 5 | `Player/PlayerWeaponController.cs` | 1 (própria declaração) | CONFIRMADO morto |
| 6 | `UI/HUD/EquipmentHUD.cs` | 1 (própria declaração) | CONFIRMADO morto |
| 7 | `UI/HUD/ManaHUD.cs` | 1 (própria declaração) | CONFIRMADO morto |
| 8 | `UI/HUD/PlayerNeedsHUD.cs` | 1 (própria declaração) | CONFIRMADO morto |
| 9 | `UI/HUD/PlayerStatusHUD.cs` | 1 (própria declaração) | CONFIRMADO morto |
| 10 | `Player/PlayerDodgeController.cs` (órfão, namespace `CindarsHope.Player`) | 8 matches para a string `PlayerDodgeController`, mas todos os outros 7 referem-se ao arquivo canônico distinto `Player/Movement/PlayerDodgeController.cs` (namespace `CindarsHope.Player.Movement`), consumido por `PlayerMovementAbilityController.cs`, `PlayerMovementActionRuntimeBootstrap.cs`, 2 validators e 1 comentário. Nenhuma referência real ao arquivo órfão além da própria declaração. | CONFIRMADO morto (arquivo órfão distinto do canônico — canônico NÃO tocado) |
| 11 | `UI/Death/DeathScreenController.cs` | 3 matches: própria declaração, 1 comentário em `CaveRunManager.cs:255` ("DeathScreenController listens" — comentário desatualizado, não código), 1 comentário em `DeathScreenCanvasController.cs:16` (menciona o nome como contexto histórico). Classe já era `[Obsolete]` e vazia (`{ }`). | CONFIRMADO seguro para deletar (shim vazio; comentários não quebram build) |

**Itens fora da lista de deleção re-auditados e confirmados como NÃO mortos** (ver seção 6): `Combat/StatusEffect/StatusEffectManager.cs` e `City/Schedule/**` — corretamente excluídos pela spec, não tocados nesta execução.

---

## Spec Compliance Matrix

| Requirement (spec) | Implementation | Status |
|---|---|---|
| Deletar os 11 arquivos da lista consolidada (seção 9) | 11 arquivos `.cs` + 11 arquivos `.meta` deletados | OK |
| Não deletar `Combat/StatusEffect/StatusEffectManager.cs` | Não tocado | OK |
| Não deletar `City/Schedule/**` | Não tocado | OK |
| Não tocar `Movement/PlayerDodgeController.cs` (canônico) | Não tocado; confirmado intacto em `Assembly-CSharp.csproj` linha 981 | OK |
| Não tocar `DeathScreenCanvasController.cs` (substituto ativo) | Não tocado | OK |
| Não migrar/remover `CityLayoutScheduleValidationTests.cs` | Não tocado | OK |
| `dotnet build` Assembly-CSharp + Editor exit 0 | PASS — ver seção 4 | OK |
| Grep de confirmação pós-deleção | PASS — ver seção 5 | OK |

**Desvio operacional necessário (fora da lista original de arquivos permitidos, mas necessário para o build):** `Assembly-CSharp.csproj` continha `<Compile Include>` explícitos para os 11 arquivos deletados (o `.csproj` é gerado pelo Unity Editor e não estava atualizado desde a última abertura do Editor). Sem remover essas 11 linhas, `dotnet build` falhava com `CS2001: Arquivo de origem não pode ser encontrado` para cada um. As 11 linhas `<Compile Include>` correspondentes foram removidas do `Assembly-CSharp.csproj` (a linha do canônico `Movement\PlayerDodgeController.cs`, linha 981, foi verificada como intacta antes e depois). `Assembly-CSharp-Editor.csproj` não continha nenhuma dessas entradas (grep confirmou zero matches). Esta edição é necessária para o critério de aceite 14.2 (build limpo) e é normalmente regenerada automaticamente pelo Unity Editor na próxima abertura — a edição manual aqui apenas antecipa essa regeneração para permitir a validação via `dotnet build` fora do Editor.

---

## Validation

```text
Validation method: dotnet build (individual) + run_strict_validation.ps1 (full)
Assembly-CSharp.csproj:        PASS, exit code 0, 0 erros, 5 warnings pré-existentes (EnemySkinCatalog, CombatTelemetrySession — não relacionados a esta spec)
Assembly-CSharp-Editor.csproj: PASS, exit code 0, 0 erros, 7 warnings pré-existentes (CreateEnemyActionsAndSets, ValidateEnemySkinBindings, CSharpProjectPostprocessor — não relacionados a esta spec)
```

`run_strict_validation.ps1` full run — exit code 1, mas TODAS as falhas citam arquivos que esta spec NÃO tocou:

- Corruption guard: PASS (após `git add` das deleções — necessário para que `git ls-files` refletisse o estado real do working tree; sem isso o guard falsamente reportava os arquivos deletados como "sumido/ilegível", pois ainda constavam no índice do git antes do `add`)
- Docs validation: FAIL — mas 100% das linhas de erro citam `spec_npc_physics_cat_companion.md` (spec pré-existente sem markers `/speckit.*`) e `tools/codex/Generate-CodexHarness.ps1` (placeholders pré-existentes) — ambos citados explicitamente como falhas legadas conhecidas pelo orquestrador desta tarefa; nenhum arquivo tocado por esta spec aparece nesses erros.
- Assembly-CSharp build (dentro do harness): PASS, 0E/0W
- Assembly-CSharp-Editor build (dentro do harness): PASS, 0E/0W
- Spec diff completeness: FAIL/WARN — exigia execution report (este arquivo resolve) e apontava ausência de testes (ver seção Testing Quality Gate abaixo — justificado, spec é remoção pura sem lógica nova)

Nenhuma falha nova foi introduzida por esta spec nos dois builds C#.

---

## Grep de confirmação pós-deleção (zero referências remanescentes)

```text
Grep "CaveDebugVisualizer|EnemyPatrolController|TargetVulnerabilityState|EnvironmentalExposureManager|
      PlayerWeaponController|EquipmentHUD|ManaHUD|PlayerNeedsHUD|PlayerStatusHUD" em Assets/**
  → No files found (zero matches)

Grep "CindarsHope\.Player\.PlayerDodgeController|new PlayerDodgeController|AddComponent<PlayerDodgeController"
  em Assets/** → No files found
  (o canônico CindarsHope.Player.Movement.PlayerDodgeController permanece intacto e referenciado normalmente)

Grep "DeathScreenController" em Assets/** → 2 matches, ambos comentários informativos:
  - Assets/_Game/Scripts/Cave/Runtime/CaveRunManager.cs:255 (comentário desatualizado, não código executável)
  - Assets/_Game/Scripts/UI/Death/DeathScreenCanvasController.cs:16 (comentário histórico de contexto)
```

Nenhuma referência de código ativo restante. Os 2 comentários citados não afetam compilação nem comportamento; não foram editados por estarem fora do scope de arquivos permitidos desta spec (spec autoriza apenas deleção dos 11 arquivos, não edição de `CaveRunManager.cs`).

EditMode tests: Grep confirmou que nenhum teste em `Assets/_Game/Tests/EditMode/**` referencia qualquer um dos 11 nomes de classe deletados (nem o namespace órfão `CindarsHope.Player.PlayerDodgeController`, nem `DeathScreenController` fora de `DeathScreenCanvasController`). Nenhum teste precisou ser migrado ou removido.

---

## Itens explicitamente EXCLUÍDOS desta spec (não deletados)

1. **`Assets/_Game/Scripts/Combat/StatusEffect/StatusEffectManager.cs`** — NÃO é código morto. Uso real confirmado em `Assets/_Game/Scripts/Combat/EnemyHealth.cs` (campo privado `_statusEffects` linha 23 + propriedade pública `StatusEffects` linha 57) e em `Assets/_Game/Tests/EditMode/Core/StatusEffectCanonicalTests.cs` (linhas 30/120). É o gerenciador de status effects de **inimigos**, distinto e complementar ao `Player/StatusEffectManager.cs` (gerenciador do player) — coexistem por design.

2. **`Assets/_Game/Scripts/City/Schedule/**`** — uso misto, não morto por inteiro:
   - `SchedulePeriod.cs` (enum + helper) — **ativo**, consumido por `FarmVisitRule.cs`, `FarmVisitEligibilityResolver.cs`, `DialogueContext.cs`, `DialogueCondition.cs` e 2 testes EditMode reais. Nunca deletar.
   - `NpcScheduleResolver.cs` / `NpcScheduleDefinition.cs` — marcados `[Obsolete]`, mas ainda referenciados por `Assets/_Game/Tests/EditMode/City/CityLayoutScheduleValidationTests.cs`. Removê-los exigiria migrar esse teste — trabalho fora do escopo deste lote, deixado como candidato a um "lote 2" dedicado (conforme spec seção 33).

Nenhum destes foi tocado nesta execução.

---

## Testing Quality Gate

```text
Changed runtime code:           YES (11 arquivos deletados + 11 linhas .csproj removidas)
Changed deterministic logic:    NO (remoção pura de MonoBehaviours órfãos; nenhuma lógica nova ou alterada)
Changed Unity scene/prefab:     NO (nenhuma .unity/.prefab editada; 2 comentários residuais em .cs citando nome
                                 de classe deletada, sem impacto funcional)
Automated tests added/updated:  NO
Automated tests command:        NOT RUN — não aplicável (nenhuma lógica determinística nova)
Manual Play Mode scenario:      NOT REQUIRED — nenhum dos 11 arquivos estava instanciado em nenhuma cena
                                 (nenhum AddComponent<> encontrado para as 4 HUDs; MonoBehaviours nunca anexados)
Justification if no tests:      Remoção pura de código órfão sem consumidor; o próprio dotnet build (0 erros)
                                 é o regression test desta spec, conforme definido na seção 30 da spec
                                 ("Requires regression test: YES — o próprio dotnet build 0 erros É o regression
                                 test desta spec").
Residual risk:                  Nenhum componente MonoBehaviour deletado estava anexado a GameObjects em cena
                                 (confirmado por ausência de AddComponent<> em código); risco residual de
                                 referência "missing script" em .unity/.prefab não verificado nesta execução
                                 (fora do scope de edição manual de YAML) — resolve-se automaticamente na
                                 próxima abertura/save do Editor Unity, sem ação manual, exatamente como
                                 documentado na spec seção 26.
```

---

## Honest status rationale

## Status `BUILD_VALIDATED`

- Critério central (deleção dos 11 arquivos confirmados + build limpo) atendido integralmente.
- `Assembly-CSharp` e `Assembly-CSharp-Editor` compilam com exit code 0 e zero erros/warnings novos.
- Docs validation do harness completo (`run_strict_validation.ps1`) retorna exit code 1, mas **100% das falhas citam arquivos pré-existentes e não relacionados** (`spec_npc_physics_cat_companion.md`, `tools/codex/Generate-CodexHarness.ps1`) — confirmado como falha legada conhecida, não introduzida por esta spec.
- Phase 2-3 (Unity Editor / Play Mode) não é exigida por esta spec (spec seção 25: `Requires Play Mode final validation: NO`; `Human validation timing: NOT REQUIRED`).
- Não há claim de `ACCEPTED`/`PLAYMODE_VALIDATED` — não aplicável a esta spec de remoção pura.
- Esta spec NÃO foi movida para `implementados/` — aguarda o check de elegibilidade de `/finish-spec`.

---

## Remaining work

- Nenhum trabalho pendente dentro do escopo desta spec.
- Candidato a "lote 2" (fora de escopo, deixado documentado): migrar/remover `CityLayoutScheduleValidationTests.cs` para permitir a remoção futura de `NpcScheduleResolver.cs`/`NpcScheduleDefinition.cs`.
- Residual: 2 comentários (`CaveRunManager.cs:255`, `DeathScreenCanvasController.cs:16`) ainda citam `DeathScreenController` como texto informativo — não afetam build; podem ser limpos em uma spec de higiene de comentários futura, se desejado.
- Residual: `.unity`/`.prefab` não foram auditados via Unity Editor nesta execução (fora de escopo de edição manual de YAML); se algum deles carregar um `MonoBehaviour` órfão dos 11 deletados, o Unity Editor tratará como "missing script" na próxima abertura, sem erro de compilação.
