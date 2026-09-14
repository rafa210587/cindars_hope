# Skills 21G — ativação auditada de receitas de peixe e couro

> **Spec ID:** `spec_skills_21g_recipe_activation_v1`  
> **Status:** DRAFT — decisões de balanceamento pendentes; implementação NÃO autorizada; fora de `.specs/a_implementar/`  
> **Wave/Type/Domain/Priority:** SKILLS_SDD_V1 / Data+Runtime integration+Validation / Craft+Economy / P1  
> **Parallelizable / repo lock:** NÃO; RecipeDatabase, gerador Editor, crafting e testes de economia em sequência  
> **Depende de:** runtime/save v6 da `spec_skills_21f_purchased_origin_v1` T1–T6 com evidência focal; decisão humana de ativar conteúdo antes dormente. Não depende do aceite T7 da 21F, que esta spec ajuda a desbloquear.  
> **Bloqueia:** três cadeias nomeadas em AC04/AC07 da 21F e aceite econômico amplo da fase 21  
> **Ordem de execucao:** decisão de balance/conteúdo → registro seletivo idempotente → validação de catálogo → PlayMode Town/Farm → economia/save/rollback → revisão e aceite  
> **Scope:** somente `recipe_grilled_fish`, `recipe_leather_from_hide`, `recipe_leather_armor` já autoradas; seus itens e estações existentes  
> **Out of scope:** `recipe_processed_wood` legado, novos itens/estações, prêmio adicional por craft, mudança em preço de itens Earned ou em schema de save  
> **Validation level alvo:** Editor catálogo + EditMode + PlayMode em cenas reais + análise de renda; nenhum gate executado por este draft

Este pacote segue o contrato SDD local: `Spec` define o comportamento, `Plan` propõe o desenho e `Tasks` ordena a execução. A existência do draft **não** concede autorização para modificar assets/runtime.

# /speckit.specify

## Spec

O catálogo canônico já autorou as três receitas em `Assets/_Game/Data/Craft/Recipes/`, mas `Assets/_Game/Data/Registries/RecipeDatabase.asset` não contém seus GUIDs. `GenerateCanonicalItemCatalog` materializa receitas e sincroniza apenas o `ItemDatabase`; `CraftingRecipeInitializer` registra quatro receitas MVP e remove apenas a madeira legada. Os cinco itens envolvidos já estão no `ItemDatabase`. Há `farm_cooking_01` na Farm e estação Sewing `station_house_residential_3` na Town; a Town usa `CraftingManager` e a Farm usa `CraftingRuntime` para a rota física. Logo, o bloqueio observado é **registro de receitas**, não falta de item ou estação.

| Receita/asset | Contrato atual | GUID ausente no RecipeDatabase |
|---|---|---|
| `recipe_grilled_fish.asset` | `item_fish_river_perch` 1 → `item_consumable_food_grilled_fish` 1, Cooking, 0s/0 STA | `4a7aea4690a94294689c91a21e907a52` |
| `recipe_leather_from_hide.asset` | `item_material_hide` 2 → `item_material_leather` 1, Sewing, 0s/0 STA | `4817b9d37afc74b4c83f69bf427a4523` |
| `recipe_leather_armor.asset` | `item_material_leather` 4 → `item_armor_light_leather` 1, Sewing, 0s/0 STA | `80d385468f345154ebe2a4eceb3c5d59` |

Os `BaseValue` dos cinco itens são, respectivamente, perca 14g, peixe grelhado 40g, hide 8g, couro 15g e armadura 100g (`Data/Items/*.asset`). Sael oferta perca; Hess oferta hide, couro e armadura com `BuyPriceOverride=0` e multiplicador de compra 1. **Esses valores não são cotação transacional**: preço efetivamente pago muda por skill/estado de loja. A liquidação normal do SellPoint no checkout sem asset `EconomyBalanceConfig` usa multiplicador Shipping 1, mas a matriz deve medir as transações reais.

- **AC01 — ativação seletiva:** o registry contém exatamente uma referência válida a cada uma das três receitas após geração repetida. IDs, ingredientes, saída, tipo de estação e unlock autorados permanecem estáveis. O validador continua exigindo `recipe_processed_wood` legado ausente.
- **AC02 — origem e anti-arbitragem:** comprar perca/hide/couro por loja real, transformar e liquidar por NPC/SellPoint/Shipping não gera mais ouro do que o custo pago pela parcela comprada. A saída conserva `RestrictedDerived`; no caminho hide→leather→armor, a segunda receita **não** recalcula basis como `Earned` nem aumenta o teto agregado. Para um lote puramente comprado a custo total `P>0`, payout máximo é `<P`; se `P=0`, payout é 0. Em lote misto, o orçamento do próprio entra uma única vez pelo preço normal no início do craft, como determina a 21F.
- **AC03 — cenas e save:** Cooking da Farm e Sewing da Town apresentam somente receitas habilitadas no modal/serviço corretos, iniciam o craft pelo ponto físico e entregam saída única. Save/load v6 entre primeira e segunda transformação, troca de cena, respec e reabertura da estação mantêm IDs, quantidades e lotes de origem. A forja real `recipe_forge_iron_sword` (4s, minério comprado+próprio) permanece o comparador de trabalho positivo no mesmo protocolo; este draft não lhe acrescenta prêmio.
- **AC04 — falhas atômicas:** falta do segundo insumo, inventário cheio, estação errada, save corrompido e cancelamento/job (onde aplicável) não consomem material parcialmente, não entregam saída duplicada nem convertem `Invalid` em vendável. Saída `Earned` legítima conserva preço normal pré-21G na mesma loja/canal.
- **AC05 — aceitação de equilíbrio:** a ativação só pode ser aceita com matriz L20/L50/L54 de custo por cesta, utilidade de comida/equipamento e renda observada com/sem as receitas, além do gate `<20%` da fase 21. Cotação isolada e teste de uma unidade não bastam. Se a ativação tornar o loop repetível excessivo ou banalizar fishing/couro, retornar ao refinamento e decidir gates, tempo/STA ou preços antes de execução; não ajustar esses números por conveniência do teste.

**Decisões abertas, pertencentes ao refinamento humano:** (D1) ativar as três receitas na progressão atual; (D2) manter 0s/0 STA e unlock padrão ou introduzir trabalho/gate; (D3) limites de renda, disponibilidade de Sael/Hess e papel da armadura de couro na progressão. A política de procedência 21F impede lucro sem trabalho, mas não prova ritmo ou utilidade equilibrados. Até resolver D1–D3 e revisar o plano, este draft não entra na fila executável.

# /speckit.plan

## Plan — proposta técnica condicionada às decisões D1–D3

1. **Catálogo/Editor (AC01):** reutilizar `GenerateCanonicalItemCatalog.cs` e `RecipeDatabaseSO`, adicionando sincronização **allowlist** destas três receitas canônicas por ID/GUID após `UpsertRecipe`; não registrar todas as demais receitas dormentes. Preservar idempotência da segunda geração (0 duplicatas, 0 alterações). Não alterar `CraftingRecipeInitializer.RemoveLegacyRecipe` nem habilitar `recipe_processed_wood`.
2. **Validação de dados (AC01):** estender `CindarsHopeDataValidator.cs`/`ValidateCraftingSystem.cs` com referências exatas, unicidade, IDs no ItemDatabase, tipos Cooking/Sewing, rendimento e ausência do legado. Os assets em `Data/Craft/Recipes/` só mudam se D2 decidir novos custos/gates; nesse caso atualizar a fonte `CanonicalItemCatalog.cs` antes de regenerar.
3. **Serviços/cenas (AC02–AC04):** reaproveitar `CraftingManager` da Town, `CraftingRuntime` da Farm, `CraftingPoint`, `ItemOriginDerivation`, `ShopManager`, `SellPoint`, shipping e save v6; não criar novo manager. Primeiro testar que o ponto `farm_cooking_01` e `station_house_residential_3` alcançam a receita no registry. Se o modal Town não abrir via evento existente, documentar e corrigir somente o wiring aprovado, com ownership de cena/gerador explicitado antes da implementação. Nunca contornar a estação em um teste anunciado como jornada física.
4. **Evidência econômica (AC02–AC05):** capturar custo de compra real (não `BaseValue`) e lotes no início, comparar teto dos outputs após cada receita, depois payout e gold/day por canal. Para hide→leather→armor, comprar 8 hide para produzir 4 leather e então 1 armor; repetir com 4 leather comprados diretamente. Exigir `cap_armor <= cap_4_leather <= cap_8_hide`, com igualdade possível por arredondamento, e `< totalPaid` no lote puramente comprado. Separar lote Earned e mistura para preservar valor de trabalho. Reutilizar `Phase21PurchasedOriginCraftChainsPlayModeTests` para madeira/forja como controle já existente; adicionar testes específicos Town/Farm para 21G.
5. **Save/rollback (AC03–AC04):** serializar pelo SaveManager v6 vigente entre etapas, restaurar em nova sessão e comparar cada lote/cap; provocar capacidade cheia e erro de ingrediente e comparar snapshots exatos de inventário/carteira/job. Não alterar DTO/schema por estas receitas.

**Ownership proposto:** `Assets/_Game/Scripts/Editor/Items/GenerateCanonicalItemCatalog.cs`, catálogo/validadores EditMode, `Assets/_Game/Data/Registries/RecipeDatabase.asset` via gerador, testes EditMode/PlayMode de 21G; `TownScene`/gerador apenas se o PlayMode mostrar falha de ligação. `ShopManager`, `CraftingManager`, `CraftingRuntime`, `ItemOriginDerivation` e SaveManager são consumidores existentes e ficam fora do plano de edição inicial. O lock Unity/asset registry é serial.

# /speckit.tasks

## Tasks — todas pendentes

| ID | Dependência | Ação e ownership | AC / evidência esperada |
|---|---|---|---|
| T0 [ ] | 21F T1–T6 com runtime/save v6 e testes focais | Refinamento de D1–D3: acesso, custo de tempo/STA, utilidade de peixe/couro e teto de renda por estágio; revisar esta spec/plan/tasks | AC02, AC05; decisão explícita, sem números inventados |
| T1 [ ] | T0 | Implementar allowlist idempotente no gerador Editor e regenerar somente as três refs autorizadas | AC01; 3/3 GUIDs presentes uma vez, legado ausente, segunda execução 0 mudanças |
| T2 [ ] | T1 | Estender `CindarsHopeDataValidator`/`ValidateCraftingSystem` e EditMode `RecipeActivationCatalogTests` | AC01; IDs, quantidades, estação, ausência de duplicata/legado, `failed=0` |
| T3 [ ] | T2 | PlayMode `Phase21RecipeActivationTownFarmPlayModeTests`: ponto físico Cooking/Farm e Sewing/Town; compra real Sael/Hess, transformação e segunda receita | AC02, AC03; XML `failed=0`, logs sem Error inesperado, gold de venda `< paid` para lote comprado |
| T4 [ ] | T3 | PlayMode save/load v6, respec, inventário cheio e rollback; reexecutar forja mista 4s como controle de trabalho positivo | AC02–AC04; lotes/caps idênticos antes/depois, sem entrega ou ouro duplicado |
| T5 [ ] | T4 | Matriz de cesta e gold/day L20/L50/L54, inspeção de UI/estações, revisão independente de design e não-regressão; `/finish-spec` só com critérios fechados | AC05; relatório PASS/FAIL/PARTIAL com XML/CSV e limite global `<20%` observado |

**Consistency/readiness:** AC01→T1/T2; AC02→T0/T3/T4; AC03→T3/T4; AC04→T4; AC05→T0/T5. D1–D3 não resolvidas, nenhum teste 21G executado, implementação **NÃO autorizada**.
