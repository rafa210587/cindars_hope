# Skills — fase mecânica 8B: capstones de sobrevivência e crafting

> **Spec ID:** spec_skills_20_survival_crafting_capstones_v1  
> **Status:** APROVADA — autorização humana de 2026-09-10  
> **Wave/Type/Domain/Priority:** SKILLS_SDD_V1 / Runtime+Save / Skills+Cave+Craft / P0  
> **Parallelizable:** NO; **Depends on:** spec_skills_19_combat_capstones_v1  
> **Blocks:** spec de UI de skill tree  
> **Repo lock scope/Validation/Executor:** Ownership / Unity+EditMode+PlayMode+Cave stable-run / execute-spec-strict

# /speckit.specify

## Spec

### Decisões de implementação após audit de reuse

- A fase é executada em três slices sequenciais: **20S Telisandra**, **20F fundação de outputs
  aprimorados** e **20T Thoren**. Qualidade e potência não podem ser apenas rótulos: o inventário
  atual não persiste traits de stack, portanto cada variante precisa de identidade e payload funcionais.
- Telisandra reutiliza `SurvivalSkillRuntimeCoordinator`; não cria outro tracker. A precedência
  sobre Último Fôlego ocorre no mesmo handler de HP. O capstone consulta somente o `RunId`
  existente e nunca altera seed, snapshot, materialização, inimigos ou recursos da cave.
- A regeneração natural de HP passa a ter owner real data-driven em `PlayerNeedsBalanceSO`: taxa
  base inicial de **1 HP/s**, somente fora de combate. O R3 dobra essa taxa pelo restante do buff
  depois da primeira saída de combate. A fase 21 mede sustain/TTK e pode recalibrar o valor.
- Resistência temporária é percentual, aplicada a Cold/Ice, Heat/Fire, Toxic/Poison, Fear e
  Confusion. Não afeta Physical, Lightning, Arcane, Stun, Root, Burn ou Corruption.
- A fundação de Thoren usa IDs canônicos de variantes de output em vez de ampliar o schema de
  `InventorySlot`. Variantes nunca se misturam ao stack base, preservam payload e têm uma cadeia
  explícita Q0→Q1→Q2. A qualidade da Forja Viva melhora somente utilidade: mantém o `BaseValue` do
  item base e nunca cria prêmio de venda. Equipamento escala durabilidade; consumível escala um
  payload numérico real (`HungerRestore`, `StaminaRestore` ou `DurabilityRestoreAmount`) em lote 1–5.
  Output sem campo funcional que aumente após arredondamento é inelegível.
- O caminho jogável canônico é `CraftingRuntime → CraftingStation`, inclusive pocket crafting.
  `CraftingManager.TryCraft` permanece compatibilidade legacy sem duplicar a transação.

- **AC01:** Nascido da Caverna ativa automaticamente uma vez por `runId` quando HP<25%, STA<15% ou Exausto. R1/R2/R3: 8/9/10 s, custo run/dodge -30/-38/-45% e resistência +20/+25/+30% somente às famílias ambiental/mental (Cold, Heat, Toxic/Poison, Fear e Confusion). Dodge ganha +0,4 tile de distância sem i-frame extra. Em R3, depois que `CombatStateTracker` sair de combate, a regen natural de HP dobra pelo restante do buff; encerra no fim do buff/run.
- **AC02:** checkpoint/load/revisita não rearmam Telisandra; nova run ou derrota canônica rearma. No cruzamento de HP, Telisandra resolve antes da janela de Último Fôlego.
- **AC03:** Forja Viva ativa uma vez por dayIndex. A opção e o material comum são selecionados/reservados antes do commit; a carga diária é consumida somente após um output existir. O player pode fabricar normalmente sem gastar a carga. R1 sobe utilidade para Q1; R2 escolhe Q2 ou economiza exatamente um material comum mantendo mínimo um consumido; R3 mantém a escolha e adiciona +8% DurabilityMax ao equipamento, ou +8% potência ao payload do lote consumível de até cinco, sem valor de venda adicional. No caminho `SaveCommonMaterial`, receita sem output elegível ao bônus adicional continua economizando uma unidade para não regredir do R2. Um craft/batch consome no máximo uma carga.
- **AC04:** cancelamento/falha não consome uso diário; respec/load não restauram carga nem rearmam Telisandra/Thoren. Nesta fase, Q1/Q2/R3 provam delta de venda zero e `SaveCommonMaterial` fica limitado a uma unidade comum por dia. A fase 21 mede o valor médio adicional contra 20% da renda diária mediana do estágio.
- **AC05:** estados usam tipos simples e preservam cave stable-run.

Fora: UI, animação, geração procedural, mudança de preços-base e capstones adicionais.

# /speckit.plan

Antes de executar, ler os dois documentos FASE9F. Estender o estado/provider criado na spec 15 e criar
`CraftingSkillState`, `CraftingSkillSaveData` e `CraftingSkillSectionProvider` explícitos, compartilhados
com a spec 18 em vez de colocar estado em `SkillTreeSaveData`. Criar `CavebornCapstoneResolver` e `LivingForgeCapstoneResolver` puros;
bridges assinam eventos de vitals/craft/day.

Ownership futuro:

- `Assets/_Game/Scripts/Skills/Runtime/CavebornCapstoneResolver.cs` (CREATE)
- `Assets/_Game/Scripts/Skills/Runtime/LivingForgeCapstoneResolver.cs` (CREATE)
- `Assets/_Game/Scripts/Skills/Runtime/SurvivalSkillState.cs` e `SurvivalSkillSaveData.cs`
- `Assets/_Game/Scripts/Skills/Runtime/CraftingPassiveConsumers.cs`
- `Assets/_Game/Scripts/Skills/Runtime/CraftingSkillState.cs` (CREATE)
- `Assets/_Game/Scripts/Skills/Runtime/CraftingSkillSaveData.cs` (CREATE)
- `Assets/_Game/Scripts/Save/Providers/CraftingSkillSectionProvider.cs` (CREATE)
- `Assets/_Game/Scripts/Craft/CraftingManager.cs`
- `Assets/_Game/Scripts/Cave/Runtime/CaveRunManager.cs` (somente evento/consulta de run existente)
- `Assets/_Game/Scripts/Core/Time/TimeManager.cs` (somente day index via evento existente)
- providers/DTOs criados nas specs 15 e 18, sem nova seção concorrente
- testes `SurvivalCraftingCapstoneTests.cs`, `CapstoneStableRunSaveTests.cs`, `SkillCapstonesPlayModeTests.cs` (CREATE)

Edge cases: HP e STA cruzam juntos, load no buff, craft em lote, inventário cheio após output, virada do dia,
respec no mesmo dia e retorno a nível visitado.

Ordem refinada:

1. **20S:** estado/save, resolver Telisandra, regen natural de HP, dodge e resistência temporária.
2. **20F:** catálogo/gerador e contratos persistentes de variantes Q1/Q2/potência, com roundtrip real.
3. **20T:** reserva/commit diário de Thoren no `CraftingStation`, usando a fundação 20F.
4. **20V:** PlayMode Cave/Craft, limite econômico e revisão independente.

# /speckit.tasks

| Done | ID | Edição | Cobre |
|---|---|---|---|
| [x] | T01 | Reusar/estender estados e save | AC02, AC04, AC05 |
| [x] | T02 | Resolver Telisandra e precedência com Último Fôlego | AC01–AC02 |
| [x] | T03 | Resolver Thoren no commit do output | AC03–AC04 |
| [x] | T04 | Testes de valor diário, respec/load e stable-run | AC01–AC05 |
| [x] | T05 | PlayMode cave+craft e validação `failed=0` | AC01–AC05 |
| [x] | T06 | Revisão independente e closeout | AC01–AC05 |
