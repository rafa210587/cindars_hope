# spec_arch_equipment_save_cycle_reduction_v29

> **Status:** Implementado e BUILD_VALIDATED
> **Data:** 2026-07-08
> **Escopo:** quebrar o par mútuo `Equipment|Save` sem alterar gameplay, saves, cenas, prefabs, IDs ou
> balanceamento.

## Objetivo

Reduzir mais um ciclo residual da modularização ampla (Tier 3 do
`docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md`):

```text
Baseline informado no início da tarefa: MutualModulePairs=29 (com o par Equipment|Save presente).
Medido após o corte (única execução do scanner feita nesta sessão, já com todas as edições
aplicadas): MutualModulePairs=27, sem o par Equipment|Save.
```

Nota de honestidade: não rodei o scanner ANTES de começar a editar (o baseline de 29 veio do prompt
da tarefa, não de uma execução minha pré-corte). `Equipment|Save` está confirmadamente ausente do
snapshot pós-corte. A queda observada foi de 2 (29→27), não de 1 (29→28) — o segundo par que some
(`Crafting|Save`, não citado no escopo desta spec) é explicado por uma consequência mecânica do
mesmo corte (ver seção "Efeito colateral"), mas não tenho uma medição isolada "antes" para provar
que ele já não tinha desaparecido por outra razão concorrente na working tree. Reporto os dois
números tal como medidos.

## Objetivo de direção

`Save -> Equipment` (providers `EquipmentSectionProvider`/`EquipmentDurabilitySectionProvider`
consumindo `EquipmentManager` concreto) é layering correto e foi **preservado** (direção pesada,
permanece). Só a direção leve `Equipment -> Save` (`EquipmentManager.cs` e
`EquipmentDurabilityTracker.cs` importando `CindarsHope.Save` só para usar os DTOs
`EquipmentSaveData`/`EquipmentSlotSaveData`/`EquipmentDurabilitySaveData`) foi cortada.

## Diagnóstico (Passo 0 — verificação obrigatória, revalidada no disco)

Grep de `using CindarsHope.Save` em toda a pasta `Assets/_Game/Scripts/Equipment/` confirmou
exatamente **dois** arquivos: `Equipment/EquipmentManager.cs` e
`Equipment/EquipmentDurabilityTracker.cs` — confirma a hipótese do mapa.

O DTO `EquipmentSlotSaveData` tem um campo do tipo enum `EquipmentSlot` (`CindarsHope.Equipment`).
Para mover os DTOs para Foundation sem reintroduzir o ciclo (Foundation→Equipment), o enum
`EquipmentSlot` também precisou ir para Foundation. Grep repo-wide (`\bEquipmentSlot\b`, com word
boundary — o grep textual simples tinha falsos positivos com `EquipmentSlotSaveData`/
`EquipmentSlotChangedEvent`/`EquipmentSlotViewModel`/comentários) mapeou **28 arquivos de produção +
2 de teste** usando o tipo real do enum. Todos os usos foram auditados individualmente (using vs.
qualificação total, e se o arquivo também usa outro tipo do namespace `Equipment` como
`EquipmentManager`/`EquipmentDataSO`, que precisa continuar importando `CindarsHope.Equipment`).

`EquipmentSaveData` também referenciava, dentro do mesmo `Save/SaveData.cs`: `EquipmentUpgradeSaveData`
(DTO de upgrade, puro) e `WeaponInfusionSaveData` (já em `CindarsHope.Foundation.SaveSchema`, do corte
`Economy|Save`). `EquipmentDurabilitySaveData` referenciava `DurabilityEntryData` (puro). Ambos os
DTOs auxiliares tiveram que mover para Foundation junto, senão o DTO principal em Foundation voltaria
a referenciar `CindarsHope.Save` (novo ciclo).

Todos os tipos movidos são `[Serializable]` puros — `string`/`int`/`List<>`/enum — sem
`UnityEngine.*`, elegíveis para `CindarsHope.Foundation` (mesma técnica de `EconomySaveDtos.cs`/
`QuestSource.cs`/`HotbarState.cs`/`HotbarSaveData.cs`).

## Implementação

1. `EquipmentSlot.cs` (+`.meta`) movido via `git mv` de `Assets/_Game/Scripts/Equipment/` para
   `Assets/_Game/Scripts/Foundation/` — GUID preservado. Namespace trocado de `CindarsHope.Equipment`
   para `CindarsHope.Foundation`.
2. Novo arquivo `Assets/_Game/Scripts/Foundation/SaveSchema/EquipmentSaveDtos.cs` (+ `.meta` novo)
   com `EquipmentSaveData`, `EquipmentSlotSaveData`, `EquipmentUpgradeSaveData`,
   `EquipmentDurabilitySaveData`, `DurabilityEntryData` movidos de `CindarsHope.Save` — mesmo nome de
   classe/campo (JsonUtility serializa por nome de campo, sem migration).
3. `Save/SaveData.cs`: as 5 classes removidas; `using CindarsHope.Equipment;` removido (não sobrou
   nenhum outro uso do namespace no arquivo); `using CindarsHope.Foundation;` já existia.
4. `Equipment/EquipmentManager.cs` e `Equipment/EquipmentDurabilityTracker.cs`: `using
   CindarsHope.Save;` trocado por `using CindarsHope.Foundation;`.
5. Dentro do próprio namespace `CindarsHope.Equipment` (que antes enxergava `EquipmentSlot` sem
   `using`, por estar no mesmo namespace): `AccessoryEffectRouter.cs`, `AccessoryCatalog.cs` e
   `RepairKitManager.cs` ganharam `using CindarsHope.Foundation;`.
6. 13 arquivos que usavam `EquipmentSlot` **e** outro tipo de `CindarsHope.Equipment`
   (`EquipmentManager`/`EquipmentDataSO`) mantiveram `using CindarsHope.Equipment;` e ganharam
   `using CindarsHope.Foundation;`: `Core/Bootstrap/GameBootstrap.cs`,
   `UI/InventoryPanelController.cs`, `UI/Character/CharacterEquipmentPanelController.cs`,
   `Combat/PlayerCombatStatsProvider.cs`, `Combat/SpellCastService.cs`,
   `Combat/PlayerAttackController.cs`, `Combat/BowArrowAttackService.cs`, `UI/DebugHud.cs`,
   `Player/PlayerCombatController.cs`, `Player/DerivedStatsCalculator.cs`,
   `Combat/CombatActionContext.cs`, `Player/Death/CorpseRecoveryManager.cs` e
   `Tests/EditMode/Player/AccessoriesTests.cs`.
7. 8 arquivos que só usavam `EquipmentSlot` (nenhum outro tipo de `Equipment`) trocaram `using
   CindarsHope.Equipment;` por `using CindarsHope.Foundation;`: `Inventory/InventoryManager.cs`,
   `Combat/PlayerAttackController.Attacks.cs`, `Combat/EquippedItemResolver.cs`,
   `Inventory/Data/ItemDataSO.cs`, `Core/Events/EquipmentSlotChangedEvent.cs` e
   `Tests/EditMode/Combat/PlayerAttackCoreTests.cs`.
8. 3 arquivos referenciavam o enum totalmente qualificado (sem `using`, resolvido antes via namespace
   irmão `CindarsHope.Equipment`): `Combat/PlayerAttackCore.cs` (3 ocorrências),
   `Editor/Items/GenerateCanonicalItemCatalog.cs` (4 ocorrências) e
   `UI/Onboarding/OnboardingHintService.cs` (2 ocorrências) — todas trocadas de
   `CindarsHope.Equipment.EquipmentSlot` para `CindarsHope.Foundation.EquipmentSlot`.
9. `Save/Providers/EquipmentSectionProvider.cs`, `Save/Providers/EquipmentDurabilitySectionProvider.cs`
   e `Save/Migrations/SaveV2ToV3Migration.cs` ganharam `using CindarsHope.Foundation;` (resolviam os
   DTOs antes via namespace pai `CindarsHope.Save`, agora movido).
10. `Crafting/EquipmentUpgradeRegistry.cs` trocou `using CindarsHope.Save;` por `using
    CindarsHope.Foundation;` (único consumidor de `EquipmentUpgradeSaveData` fora de Equipment/Save).
11. `Tests/EditMode/Crafting/HighTierGearCraftingUpgradesTests.cs` e
    `Tests/EditMode/Economy/TemperingTests.cs` trocaram/removeram `using CindarsHope.Save;` (só
    usavam `EquipmentSaveData`/`EquipmentUpgradeSaveData`, agora em Foundation — `TemperingTests.cs`
    já tinha `using CindarsHope.Foundation;`).
12. `ArchitectureRatchetTests.FoundationAssembly_ContainsOnlyTheCuratedPureContracts`: allowlist
    ganhou `EquipmentSlot.cs` e `EquipmentSaveDtos.cs` (crescimento anunciado do escopo de Foundation).
13. `CindarsHope.Foundation.csproj`/`CindarsHope.Runtime.csproj`: `<Compile Include>` ajustado
    manualmente (Unity fechado neste ambiente); confirmado depois que a regeneração automática do
    Unity (rodada pelo `RunUnityEditModeTests.ps1`) produziu exatamente o mesmo resultado.
14. Nenhum schema/campo/valor/nome de classe alterado; nenhuma cena/prefab/asset editado
    manualmente; nenhum comportamento de gameplay/save alterado.

## Efeito colateral (não pertence ao escopo desta spec, registrado por transparência)

O único consumidor de `EquipmentUpgradeSaveData` fora de Equipment/Save era
`Crafting/EquipmentUpgradeRegistry.cs` (módulo `Crafting`, distinto do módulo `Craft`). Ao mover o
DTO para Foundation, esse arquivo deixou de ter `using CindarsHope.Save;` — o que também elimina a
única aresta `Crafting -> Save` encontrada. Não confirmei isoladamente (antes desta sessão) se
`Save -> Crafting` já existia como aresta pareada; o snapshot pós-corte não lista `Crafting|Save`,
então, SE ele existia, também foi removido como efeito colateral legítimo (mesma técnica, sem
gambiarra). Sinalizado aqui em vez de reivindicado silenciosamente.

## Erro corrigido durante a execução

Primeira rodada de EditMode teve 1 falha real:
`ArchitectureRatchetTests.FoundationAssembly_ContainsOnlyTheCuratedPureContracts` — "Foundation
source must not depend on UnityEngine" — porque os comentários novos em `EquipmentSlot.cs` e
`EquipmentSaveDtos.cs` continham a substring literal `"UnityEngine"` (dentro de "sem UnityEngine"),
disparando o guard textual do próprio teste. Corrigido trocando a frase para "sem refs Unity"
(mesmo padrão de texto usado nos comentários `arch:` anteriores). Rodada seguinte: 2747/2747 PASS.

## Evidência

```text
tools/architecture/Get-ModularizationDependencySnapshot.ps1
  MutualModulePairs: 29 (baseline informado) -> 27 (medido)
  Equipment|Save removido (confirmado)
  Crafting|Save some do snapshot pós-corte (efeito colateral, não medido isoladamente antes)
  nenhum par novo apareceu

tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1
  exit 0
  7/7 projetos
  0 warnings
  0 errors

tools/unity/RunUnityEditModeTests.ps1 -ResultsPath TestResults\cut-equipment-save-editmode2.xml -LogFile Logs\cut-equipment-save2.log
  exit 0
  2747/2747 PASS (0 failed)
  (1ª rodada, TestResults\cut-equipment-save-editmode.xml, teve 1 falha real do guard de
  Foundation/UnityEngine — corrigida e re-executada do zero, ver seção acima)
```

## Pendências

Esta spec-filha fecha apenas `Equipment|Save`. A modularização ampla não está concluída: ainda
restam pares mútuos para specs-filhas independentes (ver
`docs/architecture/MODULARIZATION_PAIR_BREAK_MAP.md`), incluindo `Core|Equipment` (mesma técnica de
mover `EquipmentSlot`, já feita aqui — o par `Core|Equipment` deve ser reavaliado à luz desta
mudança, já que o enum não vive mais em `CindarsHope.Equipment`).
