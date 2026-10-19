# Fase 21F — T7: matriz de renda e transformação

**Estado em 2026-09-13: PARCIAL.** Este documento separa observações anteriores à ativação de procedência do reteste exigido por AC05/AC07. Nenhum valor anterior certifica a política 21F nem a renda global `<20%`.

## Renda observada antes da 21F

Os JSONs `Logs/skills-phase21-steady-income-l20-three-plots.json` e `Logs/skills-phase21-high-income-l{50,54}-*-three-plots.json` registraram D0–D5 de trigo em três canteiros. Medianas de D3–D5:

| Estágio/build | Compra de semente | Venda de trigo | Líquido transacional/dia | Metas autorais/dia | Composto/dia |
|---|---:|---:|---:|---:|---:|
| L20 base | 5g | 60g | 55g | 35g | 90g |
| L50 base | 5g | 60g | 55g | 35g | 90g |
| L50 Senso R1 + Thoren R1 | 5g | 60g | 55g | 35g | 90g |
| L54 base | 5g | 60g | 55g | 35g | 90g |
| L54 Senso R3 + Thoren R3 | 4g | 60g | 56g | 35g | 91g |

Esta janela usa pontes internas de `FarmPlot`, Renko inicializado na Farm e `SellPoint`; não inclui Town/UI, Cave, comida, crafting, outras fontes de renda ou rollover. Thoren não foi acionado. O incremento agrícola observado foi 0% em L50 e 1,11% em L54, mas **isso não mede o teto global de 20%**. Esses JSON/XML precedem o runtime 21F.

## Reteste agrícola 21F

`Phase21SteadyIncomeWindowPlayModeTests` (L20) e `Phase21HighLevelIncomeWindowPlayModeTests` (L50 base/R1, L54 base/R3) verificam, em cada dia, que a semente comprada mantém `Purchased` e a base igual ao débito real, e que a safra crescida/colhida vira `Earned` antes da venda. Os cinco processos pós-21F passaram 1/1 cada, com scan PASS: `Logs/skills21f-income-l20-r1.xml`, `Logs/skills21f-income-l50_baseline_threeplotincomewindow-r1.xml`, `Logs/skills21f-income-l50_thorenr1_threeplotincomewindow-r1.xml`, `Logs/skills21f-income-l54_baseline_threeplotincomewindow-r1.xml` e `Logs/skills21f-income-l54_thorenr3_threeplotincomewindow-r1.xml`. Os valores da cesta agrícola permaneceram 90/90/91g compostos; Senso R3 + Thoren R3 acrescentou 1,11% nessa rotina. A compra da semente está na despesa; a renda global segue **NOT RUN**.

## Cestas e canais ainda necessários

| Cesta/canal | Fonte e transformação | Condição de aceite | Estado |
|---|---|---|---|
| Pão comprado, NPC/SellAll/SellPoint/Shipping | Comprar com Senso R3; revender após respec, com reload no Shipping | Cada unidade paga no máximo `paidBasis-1`, sem duplicar entrada; comparar pão `Earned` no mesmo canal | **PARCIAL**: Farm SellPoint+Shipping, save/load e pagamento único PlayMode `Logs/skills21f-economy-playmode-r5.xml` 1/1 PASS; NPC/SellAll só têm contrato EditMode, sem jornada integrada |
| Madeira comprada → processada | `recipe_workbench_processed_wood`: `item_material_wood` 2→`item_material_processed_wood` 2, bancada física da Farm | Saída `RestrictedDerived`, teto ≤ soma das bases compradas menos perda; SellPoint não devolve custo | **PASS 1/1** junto da forja em `Logs/skills21f-craft-chains-playmode-r3.xml` (scan PASS, exit 0); 10g de base comprada → 8g de venda |
| Peixe perch comprado → grelhado | `recipe_grilled_fish`, `item_fish_river_perch` 1→`item_consumable_food_grilled_fish` 1 | Nenhuma volta ao ouro maior que o pago | **BLOQUEADO NO CATÁLOGO**: asset existe em `Assets/_Game/Data/Craft/Recipes/`, mas GUID `4a7aea4690a94294689c91a21e907a52` ausente de `Assets/_Game/Data/Registries/RecipeDatabase.asset`; cooking físico existe na Farm |
| Couro de hide comprado → armadura | `recipe_leather_from_hide` 2→1, `recipe_leather_armor` 4→1 | Segunda receita mantém teto da primeira, job salvo sem lavar origem | **BLOQUEADO NO CATÁLOGO**: assets existem, mas GUIDs `4817b9d37afc74b4c83f69bf427a4523` e `80d385468f345154ebe2a4eceb3c5d59` ausentes do registry. Estação Sewing física consta na Town; `CraftingRuntime` serializado consta na Farm, não na TownScene |
| Receita de trabalho positivo, insumos mistos | `recipe_forge_iron_sword`, minério próprio + comprado, 4s, forja física da Farm | Valor normal do próprio entra uma vez; restrito não ganha prêmio; job preserva snapshot | **PASS 1/1** no mesmo XML r3; job de 4s reteve orçamento de 23g (8g restrito + 15g próprio) no save e na coleta |
| Comida e Cave L20/L50/L54 | Compra/uso, drops, despesas, gold/day | Mesma cesta e janela por estágio/build; fontes/sinks observados | **NOT RUN** |

O PlayMode de economia na Farm mede pão, SellPoint e Shipping; a rota natural Town→Farm não foi executada. `Logs/skills21f-sell-panel-playmode-r2.xml` passou 1/1 para preview/commit da UI real de venda, mas não mede renda. `Logs/skills21f-t1-t6-editmode-r10.xml` passou 200/200, scan PASS, para contratos locais. O aceite AC05/AC07 ainda requer as cadeias nomeadas fora do catálogo, renda combinada e inspeção gráfica; até lá, **PARCIAL / teto global NOT RUN**.

O primeiro runner de craft (`Logs/skills21f-craft-chains-playmode-r1.xml`) falhou apenas porque o teste esperava um Error de boot que não ocorreu; os asserts de craft chegaram ao fim, mas **r1 é FAIL**. A expectativa foi removida e r2 passou 1/1; após eliminar três `Resources.Load` novos, r3 também passou 1/1 com scanner PASS/exit 0. Isso não cobre as receitas ausentes do registry.

O asset legado `Assets/_Game/Data/Craft/Recipes/recipe_processed_wood.asset` (`item_wood` 3→`item_processed_wood` 2) também está fora do registry por decisão explícita de `CraftingRecipeInitializer.RemoveLegacyRecipe` e `ValidateCraftingSystem`: o ingrediente legado não deve ser reinserido. A rota de madeira executável usa os IDs de material da receita `Assets/_Game/Data/Recipes/Recipe_Workbench_ProcessedWood.asset`. Nenhuma receita registrada consome `item_material_processed_wood` como segunda etapa; a segunda transformação nomeada de couro depende de nova spec de catálogo/wiring. O teste de madeira/forja usa serviços e estações reais, mas adiciona lotes `Purchased` por API de inventário, não por compra em loja; por isso não prova a jornada Town→Farm.
