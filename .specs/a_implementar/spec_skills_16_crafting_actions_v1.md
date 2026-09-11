# Skills — fase mecânica 6: ações de crafting e fazenda

> **Spec ID:** spec_skills_16_crafting_actions_v1  
> **Status:** PLAYMODE_VALIDATED — implementação e validação automatizada concluídas em 2026-09-10; aceite humano agrupado no fechamento da wave  
> **Wave/Type/Domain/Priority:** SKILLS_SDD_V1 / Runtime+Data / Skills+Crafting+Farm / P0  
> **Parallelizable:** NO; **Depends on:** spec_skills_15_survival_actions_v1  
> **Blocks:** spec_skills_17_passive_consumers_v1 e UI  
> **Repo lock scope/Validation/Executor:** Ownership / Unity+EditMode+PlayMode / execute-spec-strict

# /speckit.specify

## Spec

**Veredito de equilíbrio pré-implementação:** `SOUND_WITH_AMENDMENTS`. Os cooldowns,
o uso exclusivamente de campo dos kits, a exigência de regador, o crédito fracionário
da Marca e os sinks de crafting abaixo são vinculantes. Eles preservam a bancada como rota
superior e evitam que uma skill seja pior que consumir o mesmo kit diretamente.

- **AC01:** Remendo tem cooldown 20 s, consome um `item_consumable_repair_kit_basic` e repara 10/15/20% até 60% Max; recusa quebrado/artefato sem consumo. Quando não há seleção explícita de equipamento, escolhe o equipado elegível com menor percentual de durabilidade, desempate estável por slot e ID; a UI deve mostrar esse alvo antes do commit. Kits não são consumíveis portáteis de reparo direto: fora das skills, seu uso pertence ao fluxo de bancada. Remendo não recebe bônus de bancada/`RepairEfficiencyBonus`.
- **AC02:** Reparo Rápido tem cooldown 30 s, canaliza 1,2/1,0/0,8 s fora de combate, consome um `item_consumable_repair_kit_standard` e repara até 85%; recusa quebrado/artefato e não recebe bônus de bancada/`RepairEfficiencyBonus`. A bancada permanece a única rota para 100%, quebrados e artefatos.
- **AC03:** Irrigador tem cooldown 10 s, custa 18 STA + `item_consumable_irrigator_charge`, exige regador equipado e rega linha de 3/4/5 solos secos; nenhum solo válido recusa sem gasto. Receita inicial: um metal comum + uma unidade de água geram duas cargas.
- **AC04:** Bomba tem cooldown 6 s e consome um ID estável entre `item_consumable_bomb_physical`, `_fire`, `_frost`, `_shock` e `_toxic`; trajetória em arco, raio 1,8, 18/21/24, máximo quatro e um hit/alvo. O tipo é derivado do ID persistido no inventário, sem metadata transitória. Até a tela de loadout oferecer seleção de payload, escolhe deterministicamente o primeiro disponível na ordem Physical/Fire/Frost/Shock/Toxic e a UI deve exibir o payload resolvido. Physical é a única receita inicial; variantes elementais exigem seus unlocks estáveis e uma essência correspondente. Receita Physical: um minério comum + uma fibra + um reagente explosivo.
- **AC05:** Marca tem cooldown 30 s, custa 6 STA e dura 12/16/20 s em estação/raio 2,5; reduz 20/25/30% da STA agrícola/ofício, uma ativa, sem mudar material/tempo. Usa crédito fracionário determinístico entre ações para que quatro ações-base de 8 STA tenham saldo líquido exato de +0,4/+2,0/+3,6 STA por rank; o benefício ainda exige uso real durante a janela.
- **AC06:** toda mutação de inventário/reparo é atômica e só inicia cooldown após output.

Fora: passivas, capstone Forja Viva, qualidade extra, UI e arte.

# /speckit.plan

Criar executores por semântica (`FieldRepairSkillEffectExecutor`, `IrrigationLineSkillEffectExecutor`,
`CraftBombSkillEffectExecutor`, `EfficiencyMarkSkillEffectExecutor`) e reusar o `SkillItemTransaction` da fase 15.
Reusar `RepairKitManager`, `EquipmentManager`, `InventoryManager`, watering e crafting existentes.

Ownership futuro:

- `Assets/_Game/Scripts/Skills/Runtime/Effects/FarmCropSkillEffectExecutor.cs`
- `Assets/_Game/Scripts/Skills/Runtime/Effects/FeedbackOnlySkillEffectExecutor.cs`
- `Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutorCatalog.cs`
- quatro executores acima em `Assets/_Game/Scripts/Skills/Runtime/Effects/` (CREATE)
- `Assets/_Game/Scripts/Skills/Runtime/SkillItemTransaction.cs`
- `Assets/_Game/Scripts/Skills/Runtime/Effects/ActiveSkillExecutionController.cs` (somente resolução genérica de timing por rank)
- `Assets/_Game/Scripts/Equipment/RepairKitManager.cs`
- `Assets/_Game/Scripts/Equipment/EquipmentManager.cs`
- `Assets/_Game/Scripts/Inventory/InventoryManager.cs` (somente porta transacional existente, sem duplicar operações)
- `Assets/_Game/Scripts/Editor/Items/CanonicalItemCatalog.cs` e `GenerateCanonicalItemCatalog.cs`
- `Assets/_Game/Scripts/Skills/DefaultSkillActionCatalog.cs`
- testes `CraftingSkillActionTests.cs`, `CraftingSkillTransactionsTests.cs`, `SkillCraftingPlayModeTests.cs` (CREATE)

Edge cases: item some entre readiness/commit, linha parcialmente válida, bomba sem alvo, equipamento quebrado,
estação destruída e marca substituída. Falha mantém inventário, durabilidade, STA e cooldown.

# /speckit.tasks

| Done | ID | Edição | Cobre |
|---|---|---|---|
| [x] | T01 | Contrato transacional e itens/receitas | AC01–AC06 |
| [x] | T02 | Remendo/Reparo Rápido | AC01, AC02, AC06 |
| [x] | T03 | Irrigador/Bomba | AC03, AC04, AC06 |
| [x] | T04 | Marca e integração nos consumidores de STA | AC05, AC06 |
| [x] | T05 | EditMode atômico + PlayMode alvos válidos/inválidos | AC01–AC06 |
| [x] | T06 | Validar `failed=0` e closeout | AC01–AC06 |
