---
name: editmode-test-authoring
description: Write EditMode tests following the project's existing conventions (62 tests, NUnit, domain folders). Use whenever the Testing Quality Gate requires automated tests for deterministic logic — save DTOs, quest conditions, economy pricing, combat formulas, calendar, crafting timing.
---

# Skill: EditMode Test Authoring

## Where tests live (mandatory)

```
Assets/_Game/Tests/EditMode/<Domain>/<Thing>Tests.cs
```

Existing domains: City, Combat, Companions, Core (Data/Events), Economy, Farm, Fonte, MainProgression, Player, Quests, UI (Calendar/Crafting/Dialogue/Input), World (Calendar).

NEVER under `Assets/_Game/Scripts/**` — the `protected-path-guard` hook blocks it and the spec_quality_gate forbids it.

## Template (matches existing 62 tests)

```csharp
using NUnit.Framework;
using CindarsHope.<Domain>.<Area>;

namespace CindarsHope.Tests.EditMode.<Domain>
{
    [TestFixture]
    public class <Thing>Tests
    {
        private <Service> _service;

        [SetUp]
        public void Setup() => _service = new <Service>();

        // Helper factories for state objects (see FarmWateringServiceTests pattern)
        private FarmPlotWaterState ExternalPlot(string id = "plot_01") =>
            new FarmPlotWaterState { PlotId = id, IsExternal = true };

        [Test]
        public void ManualWatering_MarksPlotWatered()   // naming: <Action>_<ExpectedOutcome>
        {
            var plot = ExternalPlot();
            _service.ApplyManualWatering(plot, 5);
            Assert.IsTrue(plot.IsWateredToday);
        }
    }
}
```

## Test family conventions already used in the project

| Suffix | Tests | Example |
|---|---|---|
| `*ContractTests` | DTO/interface shape, default values | `LootTableContractTests`, `QuestDefinitionContractTests` |
| `*IdempotencyTests` | applying twice = applying once | `QuestRewardIdempotencyTests` |
| `*ValidationTests` | data integrity across catalogs | `EconomyAntiArbitrageValidationTests`, `StableIdsValidationTests` |
| `*WiringTests` | registration completeness | `FatigueWiringTests`, `OrphanSystemsWiringTests` |
| `*SaveLoadTests` | round-trip + null/missing section | `QuestSaveLoadTests` |

## Rules

- Test pure C# services/DTOs — no scene objects, no MonoBehaviour, no Unity lifecycle.
- One behavior per test; helper factories for state setup; no shared mutable state between tests.
- Save-related tests must cover: defaults, null/missing section fallback, invalid ID fallback, idempotency after reload.
- Regression tests (bugfix): record evidence that it fails before the fix and passes after.

## Run & evidence

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
if ($LASTEXITCODE -ne 0) { exit 1 }
.\tools\unity\RunUnityEditModeTests.ps1
```

Report block (Testing Quality Gate format) goes in the execution report. Never claim "tests passed" without runner output (rule: validation-truth).
