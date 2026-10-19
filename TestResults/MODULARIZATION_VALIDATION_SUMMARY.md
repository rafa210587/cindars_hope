# Modularization validation summary

Last verified: 2026-07-05

| Gate | Result | Evidence command |
|---|---:|---|
| EditMode | 2,726 / 2,726 PASS | `tools/unity/RunUnityEditModeTests.ps1` |
| PlayMode composition/scenes | 2 / 2 PASS | Unity batchmode, composition fixture |
| Explicit assemblies | 6 / 6, 0 errors, 0 warnings | `dotnet build` per generated project |
| Architecture ratchet | PASS | `tools/architecture/Test-ArchitectureRatchet.ps1` |
| Dependency snapshot | 1,573 files; 220 edges; 47 mutual pairs | `Get-ModularizationDependencySnapshot.ps1` |

Raw NUnit XML is intentionally local/CI-only. The compact dependency snapshots remain versioned.
