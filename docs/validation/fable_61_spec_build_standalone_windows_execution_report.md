# Execution Report — fable_61 — Shipping: Build Standalone Windows + Registro de Cenas

> Spec: `.specs/a_implementar/fable/fable_61_spec_build_standalone_windows.md`
> Date: 2026-06-21
> Status: **BUILD_VALIDATED_WITH_WARNINGS** (Phase 0-1 complete; Phase 2 Unity batchmode + build/smoke DEFERRED — single coding session, Unity Editor not run by authorization)
> Domain: Build / Shipping (Editor / Tooling only — zero runtime diff)

---

## Acceptance criteria extracted

| ID | Critério | Implementação | Evidência | Resultado |
|----|----------|---------------|-----------|-----------|
| CA-1 | Cenas registradas via API (Farm/Town/Cave + título se existir), idempotente, gerado só pelo script editor | `BuildSceneRegistrar.cs`: lista canônica única; `EditorBuildSettings.scenes = ...` via API; idempotente (compara antes de escrever); valida existência no disco; log antes/depois | Código + log de menu/batch | OK (código); execução Unity DEFERIDA |
| CA-2 | Build standalone com exit code honesto (`Builds/Windows/*.exe` exit 0; falha → exit != 0) | `StandaloneBuildPipeline.cs` (`BuildPipeline.BuildPlayer` + `BuildReport.result` → `EditorApplication.Exit`); `RunStandaloneBuild.ps1` propaga exit code + valida existência do `.exe` | Código + script (artefato ausente = FAIL mesmo com exit 0) | OK (código); execução de build DEFERIDA |
| CA-3 | Identidade mínima aplicada (productName/companyName/bundleVersion ≠ default) via PlayerSettings API | `ApplyPlayerIdentity()`: product "Cindar's Hope", company "Cindar's Hope" (placeholder), version 0.1.0, res 1920×1080 fullscreen — via `PlayerSettings.*` API | Código + EMENDA 2026-06-12-D | OK (código); aplicação no .asset DEFERIDA (via API quando Unity rodar) |
| CA-4 | Smoke check pós-build (`-Smoke`: boot N s, Player.log sem Exception/Error novos → SMOKE_PASS) | Flag `-Smoke` em `RunStandaloneBuild.ps1`: inicia .exe, sobrevive ao boot, encerra limpo, escaneia Player.log em `persistentDataPath` (padrão ScanUnityLogs) | Código do script | OK (código); execução do smoke DEFERIDA (requer build real) |
| CA-5 | Pergunta humana de identidade registrada + placeholders | Seção "Pergunta humana de identidade" abaixo | Este report | OK |

---

## Existing systems audit

System-reuse audit (skill `system-reuse-audit`) — Fase 0, ANTES de criar qualquer tipo:

| Busca | Achado | Decisão |
|-------|--------|---------|
| `BuildSceneRegistrar` / `StandaloneBuildPipeline` / `BuildPipeline.BuildPlayer` / `EditorBuildSettings.scenes` | Nenhum (só a própria spec) | Criar — não há sistema paralelo |
| Scripts de build em `tools/unity/` | Só `RunUnityCompileValidation.ps1`, `ScanUnityLogs.ps1`, `RunUnityEditModeTests.ps1`, `GenerateSpawnEcologyAssets.ps1` (nenhum de build) | REUSAR esqueleto batchmode do `RunUnityCompileValidation.ps1` (Editor 6000.4.7f1, `Start-Process`, timeout, log, exit code) e o padrão de scan do `ScanUnityLogs.ps1` para o Player.log |
| Padrão de Editor MenuItem | `GenerateCanonicalStatusEffects.cs` etc. (`[MenuItem("CindarsHope/...")]`, `static`, log idempotente) | Seguido (namespace `CindarsHope.EditorTools.Build`, MenuItem `CindarsHope/Build/...`) |
| `LoadScene(<int>)` / `GetSceneByBuildIndex` / `sceneBuildIndex` em runtime | **NENHUM** — projeto carrega cenas por NOME | Ordem de build é segura; entry index 0 só importa para o boot do standalone |
| Cena de título (F56 / E56) | F56 entregou título como overlay programático (`TitleScreenController`), **não** criou `.unity`; `Assets/_Game/Scenes/*.unity` = {Farm, Town, Cave} | Entry provisório = FarmScene (débito documentado até cena de título existir); registrar é future-proof (pega `TitleScene.unity` se aparecer) |
| `.gitignore` Builds/ | `[Bb]uilds/` já ignorado; `*.csproj` já ignorado | `Builds/` já coberto. PORÉM a regra não-ancorada `[Bb]uild/` shadowava o source `Assets/_Game/Scripts/Editor/Build/` (git ignorava os .cs novos). Correção mínima: ancorar as regras de pasta gerada do Unity à raiz (`/[Bb]uild/` etc.) para que dirs de source aninhados não sejam ignorados; `Builds/`/`Build/` de output na raiz seguem ignorados. csproj segue regenerado pelo `CSharpProjectPostprocessor` (não editado) |

Nenhum manager/service/registry/scanner paralelo criado. Lista canônica de cenas existe em UM lugar (`BuildSceneRegistrar`); o pipeline a consome via `Register()`.

---

## Spec Compliance Matrix

| Requirement (spec) | Implementation |
|--------------------|----------------|
| `Editor/Build/BuildSceneRegistrar.cs` — MenuItem + batchmode, `EditorBuildSettings.scenes` via API, idempotente, log antes/depois, falha se cena ausente | `BuildSceneRegistrar.RegisterFromMenu()` / `RegisterBatch()` (Exit 1 em falha) / `Register()` (programático); `ScenesEqual` garante idempotência; `ResolveCanonicalScenePaths` valida disco |
| `Editor/Build/StandaloneBuildPipeline.cs` — `-executeMethod`, garante registro, identidade via PlayerSettings, `BuildPlayer` StandaloneWindows64 → `Builds/Windows/`, `BuildReport` → Exit(1) se != Succeeded, loga erros/warnings/tamanho | `StandaloneBuildPipeline.BuildBatch()` (delega ao registrar; `ApplyPlayerIdentity()`; `BuildPipeline.BuildPlayer`; `summary.result`/`totalErrors`/`totalWarnings`/`totalSize`) |
| `tools/unity/RunStandaloneBuild.ps1` — esqueleto do compile-validation, `-batchmode -quit -executeMethod`, log dedicado, exit code propagado, valida existência do .exe | Criado; `Logs/unity-standalone-build.log`; `-executeMethod ...BuildBatch`; `Test-Path` do .exe (artefato ausente = FAIL mesmo com exit 0); timeout 3600s |
| Smoke `-Smoke`: boot + Player.log scan, SMOKE_PASS/SMOKE_FAIL | `-Smoke`: limpa Player.log, inicia .exe, `WaitForExit(boot)`, encerra limpo, `Scan-PlayerLogForBootErrors` (padrão ScanUnityLogs) |
| Ícone só se houver arte; senão débito (nunca YAML manual) | Sem arte de ícone disponível → DÉBITO DE ARTE documentado; nenhum asset criado |
| Não duplicar esqueleto batchmode / scanner / lista de cenas | Derivado do compile-validation; scanner reusa padrão; lista canônica única no registrar |
| Zero diff de runtime (`Assets/_Game/Scripts/**` fora de `Editor/`) | Confirmado — só `Editor/Build/**` |
| ProjectSettings só via API (ADR-0008) | Nenhum YAML editado; `EditorBuildSettings.scenes`/`PlayerSettings.*` aplicam o diff via Unity (DEFERIDO até Unity rodar) |

---

## Validation

```
Validation method: dotnet build (--no-restore) + validate_docs.ps1 + check_spec_diff_completeness.ps1
Assembly-CSharp:        PASS — exit 0, 0 errors, 1 pre-existing warning (CombatTelemetrySession CS0649)
Assembly-CSharp-Editor: PASS — exit 0, 0 errors, 3 pre-existing warnings (não vindas dos arquivos desta spec)
Docs validation:        PASS — validate_docs.ps1 exit 0
Diff completeness:      PASS — check_spec_diff_completeness.ps1 (escopo Editor/ — sem runtime change)
run_strict_validation.ps1: NOT RUN nesta sessão — esperado exit 1 por 3 cenas .unity já modificadas no working tree (ambiental, alheio a esta spec; esta spec não toca .unity). O gate de build canônico (RunStandaloneBuild.ps1) é DEFERIDO (Unity Editor não executado por autorização).
```

Phase 2 (Unity batchmode: registro de cenas + build + smoke):
```
Unity validation: NOT RUN / DEFERRED
Reason: sessão única de código; Unity Editor / batchmode / Play Mode não executados por autorização do dono.
Command attempted (pendente humano):
  .\tools\unity\RunStandaloneBuild.ps1          # exit 0 + Builds/Windows/CindarsHope.exe
  .\tools\unity\RunStandaloneBuild.ps1 -Smoke   # SMOKE_PASS + Player.log limpo
  (ou menus: CindarsHope/Build/Register Build Scenes ; CindarsHope/Build/Build Standalone Windows)
Residual risk:
  - EditorBuildSettings.asset / ProjectSettings.asset ainda NÃO contêm os diffs gerados pela API
    (cenas + identidade) — serão gerados quando um humano rodar o registrar/pipeline no Unity, com a
    autorização permissions.ask concedida por instância (ADR-0008). Até lá: m_Scenes permanece [],
    identidade permanece DefaultCompany/cindars_hope/1.0.
  - Build/smoke reais não verificados; "compila" (dotnet) não é "buildou e abre".
```

---

## Honest status rationale

**BUILD_VALIDATED_WITH_WARNINGS.** As três peças (registrar, pipeline, script PS1) compilam (editor 0E) e estão completas e idempotentes no nível de código. A validação canônica desta spec — executar o build batchmode, gerar o `.exe`, aplicar os diffs de ProjectSettings via API e rodar o smoke — **requer o Unity Editor**, que NÃO foi executado nesta sessão por autorização explícita do dono (sessão única de código, Play Mode/Unity diferidos). Portanto:

- NÃO se afirma "build passou", "Unity validated", "SMOKE_PASS" ou "cenas registradas no .asset" — nenhum tem evidência no repo (validation-truth §4).
- O que tem evidência: builds dotnet 0E, docs PASS, diff completeness PASS, zero diff de runtime, zero YAML manual.
- Não promover a `implementados/` (não feito); não move spec. Phase 2 fica como NOT RUN honesto, pendente do humano rodar `RunStandaloneBuild.ps1` no Unity.

CA-1..CA-4 estão code-complete; sua **aceitação** depende da execução Unity (DEFERIDA). CA-5 satisfeito neste report.

---

## Pergunta humana de identidade (CA-5)

| Campo | Pergunta | Decisão / Placeholder usado | Fonte |
|-------|----------|-----------------------------|-------|
| productName | Nome comercial? | **"Cindar's Hope"** (decisão) | EMENDA 2026-06-12-D (Decisões v2) |
| bundleVersion | Versão inicial? | **"0.1.0"** (decisão) | EMENDA 2026-06-12-D |
| companyName | Empresa? | **"Cindar's Hope"** — PLACEHOLDER (a confirmar na Fase 0; emenda diz "a confirmar") | EMENDA 2026-06-12-D |
| Resolução | Alvo? | **1920×1080 / 16:9, FullScreenWindow, windowed redimensionável** (decisão) | EMENDA 2026-06-12-D |
| Ícone | Existe arte? | **NÃO** — sem asset de ícone; placeholder programático não aplicado para não criar asset manual → **DÉBITO DE ARTE** (ícone default do Unity até arte existir) | Spec §escopo (nunca YAML manual) |

Ação humana: confirmar `companyName` comercial e fornecer arte de ícone; ambos ajustáveis no `StandaloneBuildPipeline` (constantes) / `PlayerSettings` API sem mudança estrutural.

---

## Autorização permissions.ask (ProjectSettings)

ProjectSettings/** é superfície protegida (`permissions.ask`) e ADR-0008 proíbe YAML manual. Esta sessão **não** editou nenhum `.asset` (nem por API, pois o Unity não foi executado). Quando um humano rodar o registrar/pipeline no Unity Editor, o prompt `permissions.ask` para os diffs gerados em `EditorBuildSettings.asset` e `ProjectSettings.asset` **É** a autorização per-instance; aprovar lá conclui CA-1/CA-3. Nenhum diff de ProjectSettings consta neste commit.

---

## Testing Quality Gate

```
Changed runtime code: NO (apenas Editor/Build/** + tools/unity/)
Changed deterministic logic: NO (pipeline de build/tooling; não roda em EditMode)
Changed Unity scene/prefab/asset wiring: NO (registra cenas via API quando executado; nenhuma cena alterada)
Automated tests added/updated: NO
Automated tests command: NOT RUN
Automated tests not added: JUSTIFIED — pipeline de build não roda em EditMode; a validação canônica É a
  execução real do build batchmode + smoke com exit codes (evidência superior a teste sintético).
  Harness ausente nomeado: nenhum runner EditMode pode invocar BuildPipeline.BuildPlayer fora de batchmode.
Manual Play Mode scenario: NOT REQUIRED — smoke de BOOT standalone substitui Play Mode para esta spec;
  jornada de gameplay completa fica no checklist humano do lote final (DEFERRED_TO_FINAL_VALIDATION).
Justification if no automated tests: ver acima (build/tooling, validação por execução real).
Residual risk: build/smoke reais não executados nesta sessão (Unity diferido); diffs de ProjectSettings
  pendentes de geração via API por humano no Unity Editor.
```

validated_game_rules: [validation_acceptance_rules.md]
validated_adrs: [ADR-0008-unity-yaml-editing-policy.md, ADR-0004-validation-evidence-phase-gates.md]

---

## Arquivos alterados

```
Assets/_Game/Scripts/Editor/Build/BuildSceneRegistrar.cs      (NOVO)
Assets/_Game/Scripts/Editor/Build/StandaloneBuildPipeline.cs  (NOVO)
tools/unity/RunStandaloneBuild.ps1                            (NOVO)
docs/validation/fable_61_spec_build_standalone_windows_execution_report.md (NOVO)
.gitignore                                                    (1 linha conceitual: regras de pasta gerada do Unity ancoradas à raiz para não shadowar o source Editor/Build/)
```

(`*.csproj` ignorado/regenerado pelo postprocessor — sem mudança.)

## Remaining work (DEFERRED — ação humana no Unity)

1. Rodar `CindarsHope/Build/Register Build Scenes` (ou `RunStandaloneBuild.ps1`) no Unity → aprovar `permissions.ask` do `EditorBuildSettings.asset` (CA-1).
2. Rodar `.\tools\unity\RunStandaloneBuild.ps1` → exit 0 + `Builds/Windows/CindarsHope.exe` (CA-2) → aprovar `permissions.ask` do `ProjectSettings.asset` (CA-3).
3. Rodar `.\tools\unity\RunStandaloneBuild.ps1 -Smoke` → SMOKE_PASS + trecho do Player.log (CA-4).
4. Confirmar `companyName` comercial + arte de ícone (débito).
5. Registrar entry definitivo quando a cena de título (E56) existir como `.unity` (registrar já é future-proof).
