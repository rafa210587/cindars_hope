# Validation baseline — 2026-09-08

Scope: pre-refactor current dirty working tree; no code/config/asset edits performed by validator.
Evidence directory: C:\Users\Rafa\AppData\Local\Temp\cindars-architecture-baseline-20260908

## Results

- Docs validation FAIL, exit 1, 73 ERROR lines (docs.log, docs-errors.txt): 4 implemented spec naming; 18 dependency header; 15 speckit marker; 26 placeholder matches; 5 required_adrs + 5 required_game_rules.
- Architecture ratchet PASS, exit 0 (architecture-ratchet.log). Debt is grandfathered: 62 singleton declarations, 33 forbidden global searches, 214 direct input, 4 Time.timeScale writes, 6 direct scene loads. PASS asserts no baseline increase.
- Dependency snapshot PASS execution, exit 0 (dependencies.log): CSharpFiles=1696; RuntimeModuleEdges=234; MutualModulePairs=0. This is a heuristic static report, not proof of SOLID.
- Initial dotnet --no-restore builds: all 7 failed exit 1 NETSDK1004 missing Temp/obj/project.assets.json. Classified setup; restored all 7 exit 0 and reran.
- Final dotnet builds: 7/7 PASS exit 0 (build-after-restore-exits.tsv, *.after-restore.log): Foundation, Runtime, Gameplay, Editor, Assembly-CSharp, Tests.EditMode, Tests.PlayMode.Composition. Runtime warnings: 5 UAC1001 nullable DialogueLineCondition fields skipped by serialization, 1 CS0649 SaveManager._corpseRecoveryManager unassigned. Editor: 16 CS0618 warnings. No C# errors.
- Unity compile PASS exit 0 (unity-compile-wrapper.log, unity-compile.log), Unity version 6000.5.7f1.
- Unity compile log scan PASS exit 0 (unity-compile-scan.log).
- EditMode FAIL: wrapper exit 1, Unity process exit 2; 2888 total / 2884 passed / 4 failed / 0 skipped (editmode.xml, editmode.log, editmode-wrapper.log, editmode-failures.txt). Failures preexist this architecture task and are farm tests; full assertions/stacks in editmode-failures.txt.
- Strict FAIL exit 1 (strict.log): UNITY_PROJECT_BUILD_FAILURE despite real build PASS. Existing harness output capture defect; docs failure also softened to legacy-only without evidence. Do not claim BUILD_VALIDATED.
- PlayMode runtime/visual: NOT RUN. Residual risk: interactive gameplay/scene behavior not validated in this baseline.
- Unity.exe processes: none before validation and none remaining after tests.
- Git status before / after compile / after EditMode identical (git-status-*.txt). No new asset status changes detected; no generators invoked; no reversions performed.

## Commands executed

All from D:\Projetos\Cindars_Hope\cindars_hope. Logs redirected to evidence directory.

pwsh -NoProfile -File tools/docs/validate_docs.ps1
pwsh -NoProfile -File tools/architecture/Test-ArchitectureRatchet.ps1
pwsh -NoProfile -File tools/architecture/Get-ModularizationDependencySnapshot.ps1
For each of the seven project names above: dotnet build <project>.csproj --no-restore; dotnet restore <project>.csproj; dotnet build <project>.csproj --no-restore
pwsh -NoProfile -File tools/unity/RunUnityCompileValidation.ps1 -UnityEditorPath 'C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe' -ProjectPath . -LogFile '<evidence directory>\unity-compile.log'
pwsh -NoProfile -File tools/unity/ScanUnityLogs.ps1 -LogFile '<evidence directory>\unity-compile.log'
pwsh -NoProfile -File tools/unity/RunUnityEditModeTests.ps1 -UnityPath 'C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe' -ProjectPath . -ResultsPath '<evidence directory>\editmode.xml' -LogFile '<evidence directory>\editmode.log'
pwsh -NoProfile -File tools/docs/run_strict_validation.ps1

Assembly-CSharp-Editor.csproj absent: used authoritative CindarsHope.Editor.csproj. Skills' default Unity 6000.4.7f1 not installed: passed real current Unity path explicitly.

Overall BLOCKED for clean closeout: 4 baseline test failures, docs failure and strict tooling failure remain. No corrections made by validation agent.
