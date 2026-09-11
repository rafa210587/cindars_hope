# Skills — fase mecânica 7B: passivas de crafting e economia

> **Spec ID:** spec_skills_18_crafting_economy_passives_v1  
> **Status:** PLAYMODE_VALIDATED — 2026-09-10  
> **Wave/Type/Domain/Priority:** SKILLS_SDD_V1 / Runtime+Data / Skills+Craft+Farm+Economy / P0  
> **Parallelizable:** NO; **Depends on:** spec_skills_17_combat_survival_passives_v1  
> **Blocks:** spec_skills_19_combat_capstones_v1 e UI  
> **Repo lock scope/Validation/Executor:** Ownership / Unity+EditMode+PlayMode / execute-spec-strict

# /speckit.specify

## Spec

- **AC01:** Mãos Ágeis reduz 10% tempo/rank; redução combinada não passa 60%.
- **AC02:** Cuidado no Reparo aumenta 10% restauração/rank, cap 30%, sem reduzir materiais.
- **AC03:** Olho de Material faz um roll seeded de 5%/rank e concede +1 recurso marcado explicitamente como comum no catálogo; não duplica variantes Silver/Gold, raro/quest. A chave inclui run/save scope, instância elegível e tentativa consumida, impedindo reroll por load. Quando a colheita base conclui, a tentativa é consumida mesmo se não houver espaço para o bônus; colheita cancelada/falha não consome.
- **AC04:** Foco de Bancada reduz 5% materiais comuns/rank somente na estação correta; por ingrediente comum calcula `max(1, ceil(quantidade × (1−redução)))`, não toca raro e prova fixtures 1/2/5/10.
- **AC05:** Salvage exige ao menos um rank em Salvage Method e só aceita item individual `MaxStack=1`, identificado, não equipado, com receita de output unitário e pelo menos 2 unidades de ingredientes explicitamente comuns. O retorno base determinístico é `floor(40%)` por ingrediente comum, com mínimo total de 1; nunca devolve o próprio item nem ingrediente raro. A passiva tem 5%/rank, cap 15%, de +1 no primeiro retorno elegível e usa chave seeded estável por instance ID/tentativa consumida, sem reroll por load. Cancelamento, inventário cheio ou falha não removem o item nem avançam o RNG.
- **AC06:** Acabamento grava +8% DurabilityMax/rank no item criado, cap 24%, persistido em tipos simples.
- **AC07:** Senso de Mercado escolhe compra -5%/rank ou venda +5%/rank até respec, cap 15%; nunca aplica ambos. O bônus não se aplica ao `SellPoint` MVP até ele convergir para `EconomyPricingService`; o teste anti-arbitragem atravessa respec e estoque preexistente.
- **AC08:** Mochila Ordenada tem `AuthoredMaxRank=1` e permanece `NotYetExecutable` até preset/reserva existirem na UI; funções básicas continuam gratuitas.
- **AC09:** Cuidado no Reparo, Acabamento e Senso de Mercado têm `AuthoredMaxRank=3`; nenhum rank comprado pode ser ornamental.

Fora: capstone Thoren, ação Marca, UI e aceitação econômica final.

# /speckit.plan

Criar consumers pequenos sobre os serviços existentes e usar RNG seeded. A escolha de mercado entra em
`SkillTreeState`/`SkillTreeSaveData` como string estável apenas se não couber no mecanismo de variante vivo.

Ownership futuro:

- `Assets/_Game/Scripts/Skills/DefaultSkillCatalog.cs`
- `Assets/_Game/Scripts/Skills/SkillEffectAggregator.cs`
- `Assets/_Game/Scripts/Skills/SkillModifierHooks.cs`
- `Assets/_Game/Scripts/Skills/SkillTreeState.cs` e `SkillTreeSaveData.cs` (somente escolha mercado)
- `Assets/_Game/Scripts/Craft/CraftingManager.cs`
- `Assets/_Game/Scripts/Craft/CraftingStation.cs`
- `Assets/_Game/Scripts/Craft/EquipmentUpgradeService.cs`
- `Assets/_Game/Scripts/Equipment/EquipmentManager.cs`
- `Assets/_Game/Scripts/Farm/Harvest/CropYieldResolver.cs`
- `Assets/_Game/Scripts/Farm/FarmPlot.Actions.cs` e `FarmTilledSoilService.cs` (commits vivos de colheita)
- `Assets/_Game/Scripts/Economy/ShopManager.cs`, `Economy/Pricing/EconomyPricingService.cs` e previews dos painéis de shop
- `Assets/_Game/Scripts/Save/SaveData.cs`, provider/registro de estado RNG e owner de identidade de item criado
- `Assets/_Game/Scripts/Inventory/InventorySlot.cs`, `InventorySaveData.cs`, `InventoryManager.cs` e operações de slot (identidade individual aditiva)
- `Assets/_Game/Scripts/Combat/EquippedItemResolver.cs`, `PlayerAttackController*.cs` e `BowArrowAttackService.cs` (resolver instance ID para definition ID)
- entry point runtime real de salvage, identificado/implementado antes de remover `EffectPending`
- `Assets/_Game/Scripts/Skills/Runtime/CraftingPassiveConsumers.cs` (CREATE)
- testes `CraftingPassiveConsumerTests.cs`, `CraftingPassiveSaveTests.cs`, `SkillCraftingPassivesPlayModeTests.cs` (CREATE)

Edge cases: lote, arredondamento, ingrediente raro, inventário cheio, mesma seed, respec e arbitragem
buy→sell. Nenhum roll usa `UnityEngine.Random`.

Identidade individual: toda entrada normal de item `MaxStack=1` recebe `itemId#inventory-<sequência>`
atômico e persistido; craft preserva `itemId#crafted-<sequência>`. Load legado materializa identidade
determinística antes de qualquer roll, e stacks comuns continuam sem instance ID.

# /speckit.tasks

| Done | ID | Edição | Cobre |
|---|---|---|---|
| [x] | T01 | Phase 0 dos consumers e matriz de fórmula | AC01–AC09 |
| [x] | T02 | Tempo/reparo/material/salvage seeded | AC01–AC05 |
| [x] | T03 | Durability snapshot e roundtrip | AC06 |
| [x] | T04 | Escolha de mercado e logística dormente | AC07–AC09 |
| [x] | T05 | Testes de caps, fixtures 1/2/5/10, seed/load, raros, save, respec/estoque e arbitragem | AC01–AC09 |
| [x] | T06 | Unity/EditMode/PlayMode `failed=0` + closeout | AC01–AC09 |
