# Post-refactor full validation

Evidence directory: C:\Users\Rafa\AppData\Local\Temp\cindars-architecture-baseline-20260908

- Full EditMode: FAIL, wrapper exit 1 / Unity exit 2; total 2904, passed 2900, failed 4. Exact failed fullnames match all 4 baseline failures; delta 0. The 16 additional tests passed. Evidence: post-refactor-full.xml, post-refactor-full.log, post-refactor-full-wrapper.log, full-failure-name-delta.txt.
- Raw full-test log scan: FAIL exit 1, only match Exception: Test exception at log line 2264. Triaged expected test log: GameEventBusTests.cs:158 throws deliberately and :163 uses LogAssert.Expect; ExceptionInHandler_IsLoggedButDoesNotStopOtherHandlers PASSED in XML. No error CS / Compilation failed.
- PlayMode composition: PASS exit 0, 2/2, no skips. Both RootAndTracker_RemainSingleAcrossPlayModeFrames and GameplayScenes_LoadWithoutMissingScripts passed (Farm/Town/Cave loaded). Evidence: post-refactor-playmode-composition.xml/.log/-exit.txt. Scan PASS exit 0.
- Final Unity compile: PASS wrapper exit 0; ScanUnityLogs PASS exit 0. Evidence post-refactor-unity-compile.log, post-refactor-unity-compile-wrapper.log, post-refactor-unity-compile-scan.log.
- Test-ClassContext.ps1: PASS exit 0, 12 contracts. Evidence class-context-contracts.log.
- Builds not repeated: previous post-refactor 7/7 PASS remains applicable to runtime code; subsequent runtime edits only comments.
- Docs/strict deferred this round to root because tooling is being edited concurrently. Existing docs baseline remains FAIL, not waived.
- No Unity processes remain.

## PlayMode side effects and restoration

ProjectSettings/ProjectSettings.asset was clean immediately before PlayMode. Unity upgraded serialization28 to29 and AndroidMinSdk25 to26 plus extra settings fields; file SHA256 became cc7299c00f65fb92108463c5cc8734b69f38fbaf0c3b16e53f778a56845462e0. This persisted after TestRunner exit. Preserved as ProjectSettings.after-playmode.asset in evidence directory. Under explicit root authorization, restored exact original LFS object bytes (no YAML edits). Verified original SHA256 4c1a9ac0195aa73ab48eafd226cae67f71812a79ec61bef7d1ae6c347bc02c8c and git diff --exit-code 0.

Untracked Assets/Resources/GDKEditionAutoGen.meta and directory disappeared during PlayMode. Baseline captured directory-level git status only; no per-file byte backup/GUID/hash exists, so exact restoration is NOT claimed. Root cause verified: Library/PackageCache/com.unity.microsoft.gdk@3b45a9994164/Editor/GdkApiPlaymodeStateListener.cs:23 calls RemoveGdkEditionAsset on ExitingPlayMode; :29 regenerates on ExitingEditMode using tracked GDK edition. GdkEditionAssetGenerator.cs removes asset/folder through AssetDatabase.DeleteAsset. GdkEditionAsset is SDK intermediate carrying a serialized int m_EditionNumber. Expected SDK lifecycle will regenerate automatically on next Play/build. No manual generator was invoked.

## Commands

pwsh -NoProfile -File tools/unity/RunUnityEditModeTests.ps1 -UnityPath 'C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe' -ProjectPath . -ResultsPath '<evidence>\post-refactor-full.xml' -LogFile '<evidence>\post-refactor-full.log'
Unity.exe -batchmode -nographics -projectPath D:\Projetos\Cindars_Hope\cindars_hope -runTests -testPlatform PlayMode -testFilter CindarsHope.Tests.PlayMode.Composition.GameRuntimeCompositionRootPlayModeTests -testResults '<evidence>\post-refactor-playmode-composition.xml' -logFile '<evidence>\post-refactor-playmode-composition.log'
pwsh -NoProfile -File tools/unity/RunUnityCompileValidation.ps1 -UnityEditorPath 'C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Unity.exe' -ProjectPath . -LogFile '<evidence>\post-refactor-unity-compile.log'
pwsh -NoProfile -File tools/unity/ScanUnityLogs.ps1 -LogFile '<each log above>'
pwsh -NoProfile -File tools/architecture/Test-ClassContext.ps1

Residual risk: full EditMode still fails on four preexisting farm assertions. Two composition PlayMode tests do not establish human gameplay/visual acceptance. This report does not claim clean BUILD_VALIDATED or full acceptance.
