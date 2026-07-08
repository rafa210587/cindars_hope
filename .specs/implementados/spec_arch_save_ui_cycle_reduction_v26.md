# spec_arch_save_ui_cycle_reduction_v26

> **Status:** Implementado e BUILD_VALIDATED
> **Data:** 2026-07-08
> **Escopo:** quebrar o par mútuo `Save|UI` sem alterar gameplay, saves, cenas, prefabs, IDs ou balanceamento.

## Objetivo

Reduzir mais um ciclo residual da modularização ampla (Tier 1/2 do
`docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md`):

```text
Antes:
- MutualModulePairs=32
- Par presente: Save|UI

Depois:
- MutualModulePairs=31
- Par removido: Save|UI
```

## Objetivo de direção

`UI -> Save` (adapters de apresentação consumindo `SaveManager`/DTOs) é layering correto e foi
**preservado**. Só a direção `Save -> UI` (persistência dependendo de apresentação) foi cortada,
via duas sub-arestas independentes que compunham o ciclo.

## Diagnóstico (Passo 0 — verificação obrigatória, revalidada no disco)

Grep de `using CindarsHope.UI` em todo `Assets/_Game/Scripts/Save/**` confirmou exatamente 6
arquivos, cobertos integralmente pelas duas sub-arestas abaixo — nenhuma outra fonte de
`Save -> UI` foi encontrada:

1. **Tipos de Hotbar** (`CindarsHope.UI.Hotbar.HotbarState` / `HotbarSaveData`, ambos puros —
   `[Serializable]`, zero `UnityEngine`): referenciados via `using CindarsHope.UI.Hotbar;` em
   `Save/SaveData.cs`, `Save/SaveManager.cs`, `Save/SaveManager.Migration.cs`,
   `Save/Providers/HotbarSectionProvider.cs`, `Save/Providers/InventorySectionProvider.cs`.
2. **Provider de Onboarding**: `Save/Providers/OnboardingHintsSectionProvider.cs` fazia
   `using CindarsHope.UI.Onboarding;` para consumir `OnboardingHintService.Instance`.

Grep repo-wide de `HotbarState`/`HotbarSaveData`/`OnboardingHintsSectionProvider` mapeou todos os
consumidores fora de `Save/`: `Core/Bootstrap/GameBootstrap.cs` (referência fully-qualified inline
`CindarsHope.UI.Hotbar.HotbarState.SlotCount`, sem `using` de topo — não contribuía ao edge medido
pelo scanner, mas precisava ser atualizada para compilar), `UI/DebugHud.cs`,
`Editor/Validation/DebugLoadoutProvisioner.cs` e `UI/Hotbar/HotbarDebugInput.cs` (todos acessam só
via propriedade `SaveManager.HotbarState`, sem referenciar o nome do tipo — não precisaram de
`using` novo), e os testes `SaveProviderRegistryTests.cs`, `SaveSectionProviderTests.cs`,
`OnboardingHintTests.cs`.

## Implementação

1. **`HotbarState`/`HotbarSaveData` → `CindarsHope.Foundation`** (`git mv` de
   `Assets/_Game/Scripts/UI/Hotbar/{HotbarState,HotbarSaveData}.cs` para
   `Assets/_Game/Scripts/Foundation/`, preservando `.meta`/GUID). Ambos os arquivos só usavam
   `System`/`System.Collections.Generic` — confirmados puros antes da mudança. Namespace alterado
   de `CindarsHope.UI.Hotbar` para `CindarsHope.Foundation`.
   - `ArchitectureRatchetTests.FoundationAssembly_ContainsOnlyTheCuratedPureContracts` atualizado
     (allowlist curada) incluindo `HotbarState.cs`/`HotbarSaveData.cs`, mesmo precedente de
     `EconomySaveDtos.cs`/`QuestSource.cs`.
   - 5 consumidores em `Save/` trocaram `using CindarsHope.UI.Hotbar;` por
     `using CindarsHope.Foundation;` (`SaveData.cs` já tinha o using de Foundation — só removeu o
     de UI.Hotbar).
   - `Core/Bootstrap/GameBootstrap.cs`: referência fully-qualified inline atualizada para
     `CindarsHope.Foundation.HotbarState.SlotCount`.
   - Testes `SaveProviderRegistryTests.cs`/`SaveSectionProviderTests.cs`: `using` trocado para
     `CindarsHope.Foundation`.
   - `DebugHud.cs`, `DebugLoadoutProvisioner.cs`, `HotbarDebugInput.cs`: nenhuma mudança de `using`
     necessária (acesso só via propriedade `SaveManager.HotbarState`, sem nomear o tipo).
2. **`OnboardingHintsSectionProvider` relocado** de
   `Assets/_Game/Scripts/Save/Providers/OnboardingHintsSectionProvider.cs`
   (`namespace CindarsHope.Save.Providers`) para
   `Assets/_Game/Scripts/UI/Onboarding/Save/OnboardingHintsSectionProvider.cs`
   (`namespace CindarsHope.UI.Onboarding.Save`), via `git mv` (preserva histórico e GUID). O
   arquivo passou a usar `using CindarsHope.Save;` (UI→Save, direção correta e já existente) e
   `using CindarsHope.UI.Onboarding;` (mesmo módulo UI — não gera edge cruzado, já que origem e
   alvo são ambos `UI`) para resolver `OnboardingHintService`.
   - `Save/SaveManager.cs:167` (antigo `new OnboardingHintsSectionProvider();`) passou a
     `new CindarsHope.UI.Onboarding.Save.OnboardingHintsSectionProvider();` (nome totalmente
     qualificado, sem novo `using CindarsHope.UI` de topo — mesma técnica de
     `spec_arch_quests_save_cycle_reduction_v24`; `using CindarsHope.Save.Providers;` permanece
     porque outros providers continuam lá).
   - `Tests/EditMode/Save/SaveSectionProviderTests.cs` e `Tests/EditMode/UI/OnboardingHintTests.cs`
     ganharam `using CindarsHope.UI.Onboarding.Save;`.
3. `CindarsHope.Foundation.csproj`: `<Compile Include>` de `HotbarState.cs`/`HotbarSaveData.cs`
   adicionado. `CindarsHope.Runtime.csproj`: entradas antigas de `UI\Hotbar\{HotbarState,
   HotbarSaveData}.cs` removidas e path de `OnboardingHintsSectionProvider.cs` atualizado de
   `Save\Providers\` para `UI\Onboarding\Save\`. (Unity estava disponível nesta sessão e
   regenerou ambos os `.csproj` automaticamente ao rodar os testes — a edição manual documentada
   acima foi confirmada consistente com a regeneração.)
4. Nenhum schema/campo/valor/nome de classe alterado; nenhuma cena/prefab/asset tocado; nenhum
   comportamento de gameplay alterado.

## Evidência

```text
tools/architecture/Get-ModularizationDependencySnapshot.ps1
  MutualModulePairs: 32 -> 31
  Save|UI removido
  nenhum par novo apareceu

tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1
  exit 0
  7/7 projetos
  0 warnings
  0 errors

tools/unity/RunUnityEditModeTests.ps1 -ResultsPath TestResults\cut-save-ui-editmode.xml -LogFile Logs\cut-save-ui.log
  exit 0
  2747/2747 PASS (0 failed)
```

## Pendências

Esta spec-filha fecha apenas `Save|UI`. A modularização ampla não está concluída: ainda restam
31 pares mútuos para specs-filhas independentes (ver
`docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md`).

## Nota de risco residual (não bloqueante)

Nenhum identificado nesta spec. `SaveManager.cs` continua sendo, por design pré-existente (não
alterado aqui), um composition-root paralelo ao `GameBootstrap` que conhece quase todos os
domínios do jogo — débito documentado, não introduzido por este corte, fora do escopo declarado
(`Save|UI`).
