---
name: editmode-test-authoring
description: Escreve EditMode tests seguindo as convenções existentes do projeto (62 tests, NUnit, domain folders). Use sempre que o Testing Quality Gate exigir testes automatizados para deterministic logic — save DTOs, quest conditions, economy pricing, combat formulas, calendar, crafting timing.
---

# Skill: EditMode Test Authoring

## Onde os testes vivem (obrigatório)

```
Assets/_Game/Tests/EditMode/<Domain>/<Thing>Tests.cs
```

Domains existentes: City, Combat, Companions, Core (Data/Events), Economy, Farm, Fonte, MainProgression, Player, Quests, UI (Calendar/Crafting/Dialogue/Input), World (Calendar).

NUNCA sob `Assets/_Game/Scripts/**` — o hook `protected-path-guard` bloqueia isso e o spec_quality_gate proíbe.

## Template (bate com os 62 tests existentes)

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

## Convenções de test family já usadas no projeto

| Suffix | Testes | Exemplo |
|---|---|---|
| `*ContractTests` | shape de DTO/interface, valores default | `LootTableContractTests`, `QuestDefinitionContractTests` |
| `*IdempotencyTests` | aplicar duas vezes = aplicar uma vez | `QuestRewardIdempotencyTests` |
| `*ValidationTests` | integridade de dados across catalogs | `EconomyAntiArbitrageValidationTests`, `StableIdsValidationTests` |
| `*WiringTests` | completude do registro | `FatigueWiringTests`, `OrphanSystemsWiringTests` |
| `*SaveLoadTests` | round-trip + section null/missing | `QuestSaveLoadTests` |

## Regras

- Teste C# puro de services/DTOs — sem scene objects, sem MonoBehaviour, sem Unity lifecycle.
- Um comportamento por test; helper factories para o setup do state; sem shared mutable state entre tests.
- Testes ligados a save devem cobrir: defaults, fallback de section null/missing, fallback de invalid ID, idempotency depois do reload.
- Regression tests (bugfix): registre evidência de que falha antes do fix e passa depois.

## Validação

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
if ($LASTEXITCODE -ne 0) { exit 1 }
.\tools\unity\RunUnityEditModeTests.ps1
```

O report block (formato Testing Quality Gate) vai no execution report. Nunca afirme "tests passed" sem o output do runner (rule: validation-truth).
