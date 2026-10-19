# Skills 21E — reconciliar preços de compra e revenda entre canais

> **Spec ID:** spec_skills_21e_cross_channel_arbitrage_v1  
> **Status:** SUPERADA COMO POLÍTICA — teto universal por SKU rejeitado após decisão humana de 2026-09-13; somente diagnóstico histórico, implementação NÃO autorizada  
> **Wave/Type/Domain/Priority:** SKILLS_SDD_V1 / Runtime+Data+Validation / Economy+Skills / P0  
> **Parallelizable:** NO; lock conjunto em pricing, loja, venda e assets econômicos  
> **Parallel group / Can run with / Must not run with:** N/A / apenas documentação sem pricing / specs que alterem ShopManager, EconomyManager, SellPoint, Shipping ou ShopDataSO  
> **Repo lock scope:** `Assets/_Game/Scripts/Economy/{Pricing,ShopManager.cs,EconomyManager.cs,ItemPriceResolver.cs,SellPoint.cs}`, `Assets/_Game/Scripts/Farm/Shipping/**`, `Assets/_Game/Scripts/Editor/Economy/**`, `Assets/_Game/Data/Economy/**`, testes Phase21  
> **Depende de:** spec_skills_18_crafting_economy_passives_v1; diagnóstico AC05 da spec_skills_21_balance_acceptance_v1  
> **Bloqueia:** aceite AC05 da spec_skills_21_balance_acceptance_v1 e, portanto, UI da árvore  
> **Ordem de execucao:** decisão de preço → implementação de canais/assets → matriz de testes → PlayMode → revisão de design  
> **Refinamento:** `docs/refinements/a_implementar/ref_skills_phase21_cross_channel_arbitrage_v1.md`  
> **Scope:** contenção de revenda direta por teto unitário de SKU vendido nas lojas ligadas à Town, em quatro canais, com catálogo Editor verificável  
> **Out of scope:** procedência de inventário, transformação/crafting lucrativa, modificação de buy price, save schema, cenas e promoção à fila executável  
> **Validation level alvo / Executor:** PLAYMODE + EditMode + gate de renda e design / Claude ou Codex, após readiness  
> **Validação:** EditMode pricing + PlayMode Farm/Town + renda observada; não confundir cálculo com transação

> **Sucessora:** `spec_skills_21f_purchased_origin_v1` em `features_futuras/`; a direção escolhida é procedência de compra por lote. Nenhuma tarefa deste draft 21E deve ser executada.

required_adrs: [ADR-0010]
required_game_rules: [economy_rules.md, skill_tree_rules.md]

# /speckit.specify

## Spec

O foco de compra R3 de `crafting_shop_sense` reduz o pão do Renko de 20g para 17g.
R3 é um estado de preço válido para uma build Crafting dedicada em estágio alto;
**não é uma build legal de L20** (Tier 4 pede 18 pontos gastos antes dos três ranks,
enquanto L20 oferece 10 pontos-base).
O `SellPoint` materializado na Farm paga 20g após respec isolado. O teste PlayMode
`Phase21EconomyAcceptancePlayModeTests` confirmou **+3g** no ciclo controlado e falhou 0/1;
a auditoria estática encontrou margem em 76 dos 91 pares item/loja. São preços e regras já
existentes, não um novo bônus de skill. O asset de Renko está referenciado na TownScene,
mas a fixture inicializa sua sessão na Farm; acesso completo pela UI de Town continua
pendente. A spec 21 exige ausência de ciclo lucrativo, mas seu ownership de tuning não
inclui os executores de venda.

- **AC01 — sem ouro criado por revenda ordinária:** para cada item vendável listado numa loja
  acessível, compra com Senso R3 numa build legal de estágio alto seguida de venda no mesmo NPC, SellAll urbano,
  SellPoint da Farm ou envio da Farm não aumenta ouro. Incluir respec antes da venda, load do
  estoque/carteira e a liquidação do envio no dia seguinte. Registrar custo do respec, taxas,
  quantidade, canal, dia, estoque e ouro inicial/final; nenhuma margem positiva pode ser
  escondida por um custo de setup artificial ou por itens não acessíveis.
- **AC02 — desconto real:** os três ranks do foco `buy` continuam reduzindo o preço em 5/10/15%
  onde a política aprovada permitir; o foco `sell` continua exclusivo, até +15% na loja NPC.
  Qualquer piso que anule um rank em uma entrada autorada exige decisão de conteúdo explícita,
  tooltip correto e teste de benefício efetivo. Não aplicar a passiva ao SellPoint MVP por
  acidente; a exclusão da spec 18 permanece até uma decisão expressa de convergência.
- **AC03 — canais coerentes:** definir preço e arredondamento para compra, loja NPC, SellAll,
  SellPoint e Shipping com um único contrato verificável. `BuyPriceOverride`, qualidade,
  raridade, modificadores de evento, stock e story não podem reabrir ciclo repetível. O envio
  só contabiliza renda no dia do pagamento e não duplica a transação no resumo.
- **AC04 — progressão econômica preservada:** comparar compra de sementes, alimento e
  provisões de Cave antes/depois da política em L20/L50/L54; registrar custo por cesta e
  tempo/atividades necessários para pagá-la. Mudança acima de 10% em preço/cesta retorna à
  revisão de game design antes de alterar assets em lote.
- **AC05 — compatibilidade:** estoque finito, `DailyRestock`, proteção de itens de quest/chave,
  save/load de compras e respec, e vendas atômicas permanecem válidos. Não alterar save DTO,
  IDs de item/loja ou snapshots de Cave por conveniência da fixture.

## §9 — Estado real do repositório (Phase 0, 2026-09-13)

| Evidência | Estado observado | Limite |
|---|---|---|
| `TownScene.unity` + `NpcShopController` | 23/28 lojas têm `_shopData` ligado; 70 pares vendáveis, 27 SKUs. Renko é `shop_renko`; cinco lojas legadas sem cena não acrescentam SKU. | Horário limita NPCs e nove portas; vínculo não prova compra pela UI. |
| `Logs/skills-phase21-price-audit-20260913.csv` | 56/70 pares da Town têm margem neutra no buy R3→SellAll. O menor quote R3 dos 27 SKUs é igual com 23 ou 28 lojas; teto neutro atingiria 24/27. | Quotes e estoques são projeção, não renda observada. |
| `Logs/skills-phase21-renko-materialized-economy-playmode-20260913.xml` | 0/1 FAIL: compra controlada 17g, SellPoint materializado 20g. | Sessão de Renko e respec ainda são controlados na Farm; não é UI Town→Farm. |
| `Phase21CrossChannelPriceTests` | Desconto de pão 1/1 PASS; cinco cenários de arbitragem 0/5 FAIL em Unity. | Matriz de quotes; não prova transação de cada SKU nem evento vivo. |
| `FishingSpot` + `ShippingBinInteractable` | A captura retém qualidade/raridade só no evento; inventário guarda ID+quantidade e a caixa deposita `qualityTier=0`. | O teto não remove bônus de peixe atualmente pago, mas anularia o bônus se qualidade chegar futuramente ao shipping sem revisão da regra. |
| `Phase21ObservedEconomyIncomePlayModeTests` | PlayMode 1/1 PASS: L20/dia 1, lote controlado de 3 trigos do yield autorado no SellPoint real da Farm, carteira 0→36g e evento +36g; JSON/CSV e XML em `Logs/skills-phase21-observed-income-*`. | O lote foi injetado no inventário; plantio, passagem dos dias e colheita não foram observados. Mediana diária L20/L50/L54 = NOT RUN. |
| `Phase21ObservedEconomyIncomePlayModeTests` r3 | Dois processos batch separados, 1/1 PASS cada: no ciclo, Renko em sessão na Farm vendeu semente por 5g; `FarmPlot` materializado avançou D1→D4, colheu 5 trigos e SellPoint pagou 60g. A carteira isolada passou 5→0→60 (+55g); a carteira global passou 50→85 (+35g) por duas metas. XMLs em `Logs/skills-phase21-observed-income-r3-{payout,cycle}-results.xml`, JSON/CSV em `...l20-*` e 13/13 hashes em `...r3-input-hashes-20260913.csv`. | Ações pela ponte interna, sem menu/entrada/viagem Town. Cada processo termina após o caso, pois DayStarted avançou quests/clima sem rollback completo; teste restaura carteira/progressão/metas. O saldo isolado não é renda total nem mediana; AC05 continua FAIL/NOT RUN. |

### Cestas comparáveis e payout legítimo antes da mudança

| Estágio | Cesta explícita (não loadout canônico) | Compra atual | Compra com teto proposto |
|---|---|---:|---:|
| L20 | semente de trigo + pão + kit básico do Renko | 50g sem skill | 50g |
| L50 | pão + poção HP pequena + kit básico do Renko | 85g sem skill; 72g R3 | igual |
| L54 | 2 pães + poção HP pequena + kit padrão | 130g sem skill; 110g R3 | igual |

O cap não muda preços de compra; L20 usa apenas o preço sem R3. Os 50g iniciais cobrem a
cesta L20 exata, mas não há evidência de que ela seja necessária para entrar na Cave.
Na projeção de revenda, três peixes de Sael (um de cada) caem de 66g para 57g (−13,6%)
e pão produzido cai de 20g para 17g (−15%). Trigo colhido não está nas lojas e preserva
36g brutos por semente/3 dias (3 trigos × 12g), ou 31g líquidos após semente de 5g;
isso é cálculo do yield base por canteiro. A janela controlada L20 executada depois
observou cinco trigos `Excellent` por dia D3–D5, 60g de venda, 5g de próxima semente,
55g líquidos na carteira transacional e 35g/dia de metas globais: mediana composta
controlada de 90g/dia em `Logs/skills-phase21-steady-income-l20-three-plots.json`.
Ela não inclui renda de rollover, Town/UI, L50/L54 nem o limite `<20%`. A receita de bolso
de madeira ainda permite comprar wood por 3g, processar a custo de 0 tempo/STA e vender
processed wood por até 13g no teto hipotético; é dependência de correção separada.
Há mais duas cadeias autoradas de tempo/STA zero: perca comprada por 12g pode virar peixe
grelhado com quote de venda até 34g; quatro couros por 52g podem virar armadura de couro
com quote até 85g. Estação, ingredientes e venda ponta a ponta ainda não foram exercitados;
estes valores são **cotações condicionais**, não lucro observado. A matriz deve tratar
transformação separadamente de revenda direta e impedir que o aceite AC05 geral seja
declarado apenas pelo teto de SKU.
Nenhum número acima autoriza declarar o limite `<20%` de renda da fase 21 como PASS.
O primeiro payout controlado em Unity confirma que três unidades de trigo creditam 36g pelo
SellPoint; o segundo confirma que um plantio real regado por três viradas produz cinco
trigos e vende por 60g. Como o trigo cru não é SKU vendido nas lojas ligadas à Town,
esses lotes específicos não seriam limitados pelo teto proposto. A carteira isolada do
segundo diagnóstico mostra +55g após semente, **mas não inclui** metas diárias que pagaram
ouro/XP ao jogador global nem outros efeitos de DayStarted. Nenhum dos testes mede o efeito
do teto sobre a composição diária de renda ou sobre artesanato/peixe.
Entre os SKUs listados, a maior perda unitária proporcional na venda neutra projetada é
da semente de trigo (5→4g, −20%); o cap da semente não equivale à receita do trigo colhido.
Uma colheita regada por três dias rendeu cinco trigos no diagnóstico via ponte interna, pois a
qualidade `Excellent` acrescenta duas unidades ao yield base de três; o primeiro teste
de payout não simula esse percurso. `FarmPlot.Interact` só abre o menu e o menu usa
`UnityEngine.Input.GetKeyDown`, sem comando público de plantar/colher para a fixture atual.
Um ciclo isolado de três dias teria mediana zero se a renda se concentrar na colheita;
não seria uma base representativa para L20/L50/L54. Antes do gate `<20%`, definir agenda
canônica por estágio (área, atividades, lotes, insumos, despesas e janela) e observá-la
no jogo, com eventos e carteira, sem tomar o yield base como renda diária.

**Protocolo candidato, executado de forma controlada apenas em L20:** três dos dez canteiros da Farm,
trigo plantado em D0/D1/D2 e colhido, vendido cru e replantado em D3/D4/D5. Rega a cada
virada de dia, compra real das sementes em Renko, venda no SellPoint, mesmo espaço e ações
em L20/L50/L54, cronômetro de deslocamento e ações por menu, gastos de semente/STA e ouro
por dia. A fixture L20 executou pontes internas e produziu três colheitas sucessivas;
não demonstrou renda de todas as atividades do estágio nem passagem por Town/UI. Cinco
trigos × 12g = 60g brutos e 55g líquidos após a semente de 5g foram **observados** em
cada D3–D5. Senso R3 legal em L50/L54 projetaria semente de 4g e 56g líquidos (+1,82%)
nessa fonte, ainda sem execução. `farm_rules.md` ainda diz trigo
×3/50g e rega não implementada, em conflito com asset/runtime; sua faixa de 50–100g/dia
é orientação de design e não pode substituir a medição.

### Política de planejamento — impede promoção à fila executável até gate de renda

Após a proposta e a pergunta direta de aprovação, o usuário pediu continuidade em
2026-09-13. O planejamento segue com **teto de revenda por SKU vendido em loja** para
conter a arbitragem direta,
aplicado após bônus e arredondamento por item/lote. A proposta mantém os preços de compra
e os três ranks do foco `buy`; exige revisão da perda de payout e das lojas acessíveis
antes de editar runtime. Procedência por quantidade fica como alternativa de maior escopo
para spec própria se a perda legítima de renda tornar o teto inviável. A candidata inicial de
elevar o preço-base de compra de lojas comuns a `1,20 × BV` foi **rejeitada pelo parecer
independente**: deixa arbitragem com bônus reais e encarece a preparação inicial.
Na projeção estática de 91 pares, `max(BuyPriceMultiplier, 1,20)` elimina os 76 ciclos
lucrativos somente contra SellAll neutro, mas eleva o preço R3 de 77 pares em 25 lojas; pão passa de 17g a 20g e
poção de HP de 34g a 41g. O CSV em `Logs/skills-phase21-price-audit-20260913.csv` guarda
as duas colunas de cenário. **Esse candidato não fecha AC05:** SellAll ainda recebe até
`round(round(BaseValue × quantidade × 1,05) × 1,35)` com Anel de Finan, Lua Âmbar e
CropPriceSurge combinados no código; pão do exemplo poderia ser vendido por 28g.
A combinação temporal ainda requer PlayMode. Com estoque diário inteiro, a projeção
encontra 86/90 pares lucrativos apenas com Anel de Finan e 90/90 com Anel + Procura em Alta.
Para cobrir Anel + Lua Âmbar + Procura em Alta só por preço de compra, o limite contínuo
seria `BuyPriceMultiplier ≥ 1,4175/0,85 ≈ 1,66765`, antes de overrides/arredondamento.
Na General Store, uma unidade de cada produto sobe de 81g a 98g com piso 1,20 (+21%);
pão+kit de reparo sobe de 45g a 54g, superando os 50g iniciais. Nenhum multiplicador
universal está aprovado. O teto por SKU e a alternativa de procedência exigem definição
de eventos, lotes, transformações e feedback.
A projeção não substitui uma decisão de pacing nem cobre outros modificadores temporários.
Auditoria estrutural posterior encontrou 23 dos 28 `Shop_*.asset` referenciados como
`_shopData` na TownScene. São 70 pares vendáveis e os mesmos 27 SKUs vendáveis do catálogo;
os cinco assets sem vínculo em cena incluem General Store e não adicionam SKU exclusivo.
Recalcular o menor preço R3 só nas 23 lojas não mudou o mínimo de nenhum dos 27 SKUs:
24 seguem abaixo de `BaseValue` no teto proposto. Isso é vínculo estrutural, não prova de
compra simultânea: `NpcScheduleAvailabilityGate` restringe horários, e o acesso pela UI
de Town ainda requer PlayMode.
O parecer de design recomenda **teto por SKU para contenção v1**, condicionado a medir
a perda legítima de renda e a fechar a matriz; ainda não é autorização de código. Na
projeção de catálogo completo, sem filtro de acesso, dos 27 SKUs vendáveis listados,
24 teriam teto abaixo de `BaseValue` na venda neutra. Refazer a matriz com canais e
lojas acessíveis antes de usar esse número no plano executável.
Entre eles há três peixes e outputs de cozinha/crafting: pão produzido seria vendido por
até 17g em vez dos 20g atuais. Procedência preservaria essa venda, mas exigiria origem
por quantidade em split/merge, baú, consumo, shipping e save legado. Nenhuma alternativa
resolve sozinha a transformação instantânea de madeira BV3 em madeira processada BV15
pela receita atual; tratá-la como dependência explícita antes de declarar AC05 completo.
Reduzir só SellAll para `0,90 × BV` não fecha o ciclo contra compra a `0,85 × BV`; Shipping
`0,95 × BV` continua. Alternativas como procedência por lote exigiriam novo estado persistido
e devem ser avaliadas em proposta separada, não introduzidas incidentalmente aqui. Até o
gate de renda passar e o blueprint cumprir a profundidade exigida, esta spec permanece fora de
`.specs/a_implementar/`.

# /speckit.plan

## §13 — Reuso e limites de domínio

Reusar `ShopManager`, `EconomyPricingService.CalculatePrice(PriceRequest)`, `MarketSkillModifierProvider`,
`ItemPriceResolver`, `SellableItemPolicy`, `EconomyManager`, `ShippingPriceResolver` e
`FarmShippingService`; não criar outro manager, carteira, inventário ou segundo sistema de
modificadores. `PriceRequest`/`PriceResult` continuam contratos existentes. A spec 18
mantém o foco `sell` exclusivo de `buy`; o SellPoint não passa a receber a skill.
`OrderReward`, `FestivalReward` e EventStall não são revenda ordinária coberta pelo cap;
se qualquer um permitir converter diretamente SKU comprado em ouro repetível, reabrir AC01
e aprovar exceção explícita antes de executar.

## §15 — Arquitetura alvo (proposta, sujeita ao gate de §24)

**CRIAR, se a política for aprovada:**

| Arquivo | Responsabilidade |
|---|---|
| `Assets/_Game/Scripts/Economy/Pricing/RetailResaleCapCatalogSO.cs` | Asset de pares `ItemId`/`MaxUnitGold`, somente dados; sem origem por stack. |
| `Assets/_Game/Scripts/Economy/Pricing/RetailResaleCapResolver.cs` | Consulta pura de cap unitário e aplicação final ao quote; contrato §16. |
| `Assets/_Game/Scripts/Editor/Economy/GenerateRetailResaleCaps.cs` | Descobrir apenas as lojas ligadas à Town, calcular R3 por SKU e gerar/validar asset de forma idempotente. |
| `Assets/_Game/Data/Economy/Resources/RetailResaleCapCatalog.asset` | Catálogo gerado e carregável em runtime; path final sujeito à verificação do carregamento `Resources`. |

**MODIFICAR, somente após gate:**

| Arquivo | Edit concreto |
|---|---|
| `Pricing/EconomyPricingService.cs` | Aplicar cap no `PriceResult` apenas para `GenericShopBuyFromPlayer` e `SpecializedShopBuyFromPlayer`, depois de multiplicadores e arredondamento unitário. |
| `ShopManager.cs` | Preservar `CalculateBuyPrice`; passar `ItemId` e loja no quote de venda e fazer `TrySellItem` creditar o `TotalPrice` final. |
| `EconomyManager.cs` | Em `HandleSellAllRequested`, resolver bônus e cap por SKU antes de somar; texto/evento usam o mesmo total creditado. |
| `ItemPriceResolver.cs`, `SellPoint.cs` | Resolver base por unidade e aplicar cap por `itemId` após o arredondamento; `Interact` multiplica por quantidade após cap. |
| `ShippingPriceResolver.cs`, `FarmShippingService.cs`, `ShippingBinRuntimeService.cs` | Cotar valor unitário integral após qualidade/contrato/cap, preservar snapshot simples e liquidar `unitGold × quantidade` exatamente uma vez em D+1. |
| `Phase21CrossChannelPriceTests.cs`, `Phase21EconomyAcceptancePlayModeTests.cs`, testes de shipping | Trocar fixtures diagnósticas por asserts do contrato; adicionar cenários nomeados §22. |

`ShopDataSO`, seus 28 assets, `ItemDataSO`, `PriceRequest` e save DTOs não precisam de
novo campo na proposta. O gerador **lê** os 23 assets Town; não reescreve preços de loja.
Se o gate exigir exceção por SKU ou mudança de preço, registrar revisão de conteúdo e
alterar esta spec antes de tocar asset. Pattern: `(skill: economy-balance-tuning)`,
`(skill: data-catalog-authoring)`, `(skill: editmode-test-authoring)` e
`(skill: non-regression-review)`; a lógica de event bus/save segue regras do projeto.

## §16 — Contratos de preço propostos; assinaturas existentes identificadas

**Já existem:**

```csharp
// EconomyPricingService
public PriceResult CalculatePrice(PriceRequest req);
// ShopManager
public static int CalculateBuyPrice(ItemDataSO itemData, ShopItemEntry entry,
    ShopDataSO shopData, int amount = 1);
public static int CalculateSellPrice(ItemDataSO itemData, ShopDataSO shopData,
    int amount = 1);
// ItemPriceResolver
public static int ResolveSellingPrice(int baseValue, SellContext ctx,
    EconomyBalanceConfigSO config = null);
// ShippingPriceResolver
public float Resolve(ShippingPriceInput input);
// FarmShippingService
public ShippingBatch ProcessDayBatch(int currentDay);
```

**Adicionar se aprovado, mantendo sobrecargas existentes para call sites antigos:**

```csharp
// RetailResaleCapCatalogSO: serialização Unity de conteúdo, não DTO de save.
[Serializable] public struct RetailResaleCapEntry { public string ItemId; public int MaxUnitGold; }
public RetailResaleCapEntry[] Entries;

// RetailResaleCapResolver: SKU sem entrada => quote inalterado.
public static bool IsCatalogValid { get; }
public static bool TryGetCap(string itemId, out int maxUnitGold);
public static bool TryApply(string itemId, int roundedUnitGold,
    out int finalUnitGold, out string failureReason);
// EconomyPricingService: overload puro para geração; o overload existente preserva o provider vivo.
public PriceResult CalculatePrice(PriceRequest req, float marketSkillMultiplier);
// ShopManager: usa o overload explícito, sem atribuir MultiplierSource.
public static int CalculateBuyPriceForMarketFocus(ItemDataSO itemData,
    ShopItemEntry entry, ShopDataSO shopData, int rank, string variant, int amount = 1);
// ItemPriceResolver
public static bool TryResolveSellingPrice(string itemId, int baseValue, SellContext ctx,
    EconomyBalanceConfigSO config, out int finalUnitGold, out string failureReason);
// ShippingPriceResolver
public bool TryResolveUnitGold(ShippingPriceInput input,
    out int finalUnitGold, out string failureReason);
```

`RetailResaleCapResolver` carrega o catálogo gerado uma vez por `Resources.Load`, sem
`GameObject.Find`; arquivo ausente, ID duplicado, cap `<=0` e item sem registry são
falhas de integridade em runtime/Editor; hash de fontes divergente é erro do validador
Editor antes do build. `TryApply` testa a integridade
**antes** de buscar o SKU: catálogo ausente/inválido retorna `false`, `failureReason=
"RetailResaleCapCatalogUnavailable"`, mesmo para item que talvez não esteja listado.
Sem catálogo confiável não há como provar que um SKU é isento. Portanto NPC sell,
SellAll, SellPoint e Shipping recusam **toda** transação de venda ordinária, não removem
item nem creditam ouro; Shipping pendente continua pending e não emite resumo. Com
catálogo válido e SKU ausente, `TryApply` retorna `true` e quote inalterado; o teste de
integridade garante que nenhum SKU vendável das 23 lojas fique sem entrada. Não usar
retorno `0` ou fallback sem cap como indicação ambígua de falha.

`MinBuyR3(itemId) = min(ShopManager.CalculateBuyPriceForMarketFocus(item, entry,
shop, 3, MarketSkillModifierProvider.BuyVariantId, 1))` nas lojas **ligadas** à
`TownScene.unity`. A API proposta calcula o multiplicador explícito via
`MarketSkillModifierProvider.Resolve(rank, variant, BuyFromShop)` e passa esse `float`
ao novo overload de `EconomyPricingService.CalculatePrice`; jamais lê ou atribui
`MarketSkillModifierProvider.MultiplierSource`. Ela replica a construção atual de
`PriceRequest` de `ShopManager.CalculateBuyPrice`: `BaseValue = BuyPriceOverride > 0 ?
BuyPriceOverride : item.BaseValue`, multiplicador de canal `1` com override ou
`shop.BuyPriceMultiplier` sem override, `RoundingRule.Round` por unidade e
`TotalPrice = UnitPrice × amount`. Assim o piso de comparação usa exatamente o quote
R3 de compra sem alterar o estado da build corrente nem contaminá-lo entre
entradas/testes. A política proposta é `MaxUnitGold = MinBuyR3 - spreadAprovado`,
com `spreadAprovado >=1g` exigido pela
invariante atual. O cálculo de perda disponível no CSV foi feito com spread zero e
precisa ser refeito antes de gerar o catálogo. Esse bloqueio está em §24. O
gerador usa a
lista exata da cena, não a lista de sessões já abertas, e salva `ItemId` ordenado.
Disponibilidade de horário, `RequiredUnlockTag` e stock ficam em colunas de auditoria;
se uma loja estiver fechada em todo caminho legal, a inclusão conserva um cap conservador
até decisão de conteúdo. O contrato não altera preço de compra nem simula stock infinito.

Ordem por SKU/canal, sem cap no total da carteira:

```text
quoteRaw -> bônus de canal/skill/qualidade/raridade/evento elegível
         -> arredondamento monetário UMA vez por unidade segundo o canal
         -> capUnit = min(roundedUnit, MaxUnitGold[ItemId]) se SKU listado
         -> totalSku = checked(capUnit * quantidade)
         -> totalTransação = checked(soma(totalSku))
```

NPC: `EconomyPricingService` já usa `Floor` no sell e `Round` no buy, aplica `sell`
apenas no NPC e multiplica o valor unitário pela quantidade. SellPoint:
`ItemPriceResolver` usa `MidpointRounding.AwayFromZero` por unidade. SellAll hoje aplica
`AccessoryEffectRouter.ApplyGoldGain` e `WorldEventHooks.ApplySellGold` ao **total misto**;
a proposta os chama por SKU unitário, nessa ordem, **antes** do cap. Isso altera rounding
de lotes não listados: medir delta e aprovar no gate, ou revisar o contrato. Shipping hoje
guarda `BaseValueSnapshot`/`QualityTier`, `ShippingPriceResolver.Resolve` retorna `float`,
e `ShippingBinRuntimeService` arredonda o total do batch. A proposta acrescenta
`ResolveUnitGold` e liquida inteiro por unidade, preservando campos existentes de save;
`PricePreview` é informativo e deve bater com a liquidação se o mesmo contrato/qualidade
vale em D+1. `CropPriceSurge` hoje beneficia **todos** os itens no SellAll via
`WorldEventHooks`; restringi-lo a Crop exige decisão própria e teste de não-crop, não
é correção implícita deste teto. Qualidade/raridade futuras que elevem payout de SKU
listado serão truncadas pelo cap: decisão premium explicitamente aberta (§24).

Nenhum evento novo nem campo novo em `ShopStockSaveData`, `PendingShippingEntrySaveData`,
inventário ou save principal. O catálogo é derivado de assets autorados, não persistido.

## §18/19 — Escopo de edição na execução futura

Permitidos após aprovação: os arquivos da tabela §15, `Assets/_Game/Tests/EditMode/Skills/Phase21CrossChannelPriceTests.cs`,
`Assets/_Game/Tests/EditMode/Farm/FarmShippingServiceTests.cs`,
`Assets/_Game/Tests/PlayMode/Composition/Phase21EconomyAcceptancePlayModeTests.cs`,
testes novos correlatos em `Assets/_Game/Tests/EditMode/Economy/` e evidência em
`docs/validation/skills_sdd_v1/` ou `Logs/skills-phase21-*`.
Proibidos: `Assets/_Game/Scenes/**/*.unity`, `**/*.prefab`, `PlayerController`,
`SaveManager`, DTOs de save, snapshots de Cave, `Packages/**`, `ProjectSettings/**`,
`docs_old/**`, YAML manual de assets e outros `Shop_*.asset` sem decisão autorada.

## §20 — Estratégia por edição e ownership serial

1. **Gate de design primeiro (T00):** revalidar CSV/scene e medir renda observada antes/depois
   por estágio; resolver as decisões §24. Sem gate, as fases seguintes são apenas desenho.
2. **Catálogo (T01):** em `GenerateRetailResaleCaps.Generate()` descobrir GUIDs em
   `_shopData` da Town, resolver 23 shops e `ItemDatabase.asset`, filtrar com
   `SellableItemPolicy` equivalente validado, chamar
   `ShopManager.CalculateBuyPriceForMarketFocus(..., 3, "buy", 1)` para cada entrada,
   agrupar por `ItemId`, escolher mínimo e gravar via `SerializedObject`/`AssetDatabase`.
   Reexecução com input igual não altera conteúdo/GUID; erro com `shopId/itemId/path`
   para GUID faltante, ID duplicado, entry nula, `BaseValue<=0`/override inválido,
   cap zero e hash stale. O gerador retorna exit 0 com `23 shops, 27 SKUs, 0 errors`
   somente se reconfirmado; números atuais são baseline, não hardcode de runtime.
3. **Pricing NPC (T02):** extrair o miolo de `EconomyPricingService.CalculatePrice`
   para overload com `marketSkillMultiplier` explícito; o overload original continua
   lendo `MarketSkillModifierProvider.Resolve` apenas para quote vivo. Em
   `ShopManager.CalculateBuyPriceForMarketFocus`, montar o mesmo perfil/request de
   `CalculateBuyPrice` e passar o multiplicador R3 explícito, sem mutação global.
   No sell, depois de
   `ApplyRounding`/`MinPrice` e antes de `TotalPrice`, aplicar cap se `IsSellChannel`
   **e** canal é uma das duas vendas NPC; se `TryApply` falhar, devolver `PriceResult.Fail`;
   não aplicar em buy, serviço, order/festival.
   `ShopManager.TrySellItem` já consome `CalculateSellPrice`; manter rollback de remoção.
4. **SellAll e SellPoint (T02):** em `EconomyManager.HandleSellAllRequested`, pré-cotar
   o snapshot completo após `TryGetItemData`, resolver Finan→evento→`TryApply` por unidade;
   se qualquer lookup falhar, recusar o lote inteiro antes de remover itens,
   checar overflow da soma **antes da primeira remoção**, então remover e somar apenas
   SKUs removidos; remover os bônus pós-loop para
   não pagar duas vezes. Preservar licença urbana e proteção de quest/key. Em
   `SellPoint.Interact`, usar `ItemPriceResolver.TryResolveSellingPrice` com `itemId` e
   recusar o lote antes de remover se `TryApply` falhar; aplicar cap antes de `* amount`;
   feedback e `EconomyTransactionCompletedEvent.GoldDelta`
   devem ser exatamente o crédito. Não aplicar foco `sell` ao SellPoint.
5. **Shipping (T02):** em `ShippingPriceResolver.TryResolveUnitGold`, aplicar qualidade e
   contrato existentes, arredondar a unidade, aplicar cap; em `Deposit` validar preview
   e sellability antes da retirada pelo interactable, retornando `DepositResult.Fail`
   se o catálogo falhar; em `ProcessDayBatch` cotar com
   snapshot de base/qualidade e marcar processado só depois de preço válido. Em
   `ShippingBinRuntimeService.ProcessOvernight`, obter `PlayerManager` válido **antes**
   de processar/marcar o lote; sem carteira, manter pending. Crédito/evento/resumo usam
   o total inteiro; segunda chamada e reload não duplicam pagamento.
6. **Testes/evidência (T03–T05):** EditMode do gerador, matriz e rounding; PlayMode com
   árvore legal, NPC/Town e pontos Farm materializados, load/stock/envio D→D+1;
   relatórios XML/CSV/JSON/hashes e parecer de design. Um executor é owner de pricing,
   gerador e asset derivado; outro só pode assumir testes após congelar o contrato,
   sem coeditar arquivos. Revisor de design e validação são read-only.

## §21/22 — DoD binário e testes nomeados (esperado, nunca resultado atual)

Comando base do gate Unity: `Unity.exe -batchmode -nographics -projectPath . -runTests
-testPlatform EditMode|PlayMode -testFilter <classe> -testResults <XML> -quit`.
DoD por classe: XML contém `result="Passed"`, `failed="0"` e total não zero; fixture
`[Explicit]` só conta quando chamada pelo nome completo e realmente executada. O
gerador/validador via `-executeMethod CindarsHope.Editor.Economy.GenerateRetailResaleCaps.RunBatch`
deve imprimir literalmente `GenerateRetailResaleCaps PASS: 23 shops, 27 SKUs, 0 errors`
em input atual ou falhar com exit não-zero e diagnóstico por path/ID. A execução deve
recontar a Town; se os números mudarem, atualizar baseline/spec antes de aceitar.

| AC | Teste proposto, assertion literal | DoD adicional |
|---|---|---|
| AC01 | `RetailCapCatalog_TownLinkedShops_MarginPolicyPerSku`: 23 shops ligados, 27 SKUs vendáveis, `MaxUnitGold == MinBuyR3 - spreadAprovado` inclusive override; `spreadAprovado` deve ser `>=1g` sob a regra canônica, com política para item de compra 1g antes do código. Segunda geração não altera hash/GUID. `ExplicitBuyQuote_DoesNotReadOrMutateMarketSource`: trocar `MultiplierSource` entre duas funções divergentes conserva quote explícito R3=17g do pão e a mesma referência global antes/depois. | CSV de 23 lojas/70 pares e hashes; compra mínima estritamente maior que venda máxima nas quatro colunas de revenda direta para R1/R2/R3 e lotes 1/2/stock. |
| AC01 | `RenkoBread_R3BuyRespec_SellPoint_StrictMargin`: compra 17g, revenda `<17g`, inventário 1→0, delta carteira `<0`. | PlayMode Town→Farm materializado, ouro inicial/final/evento e respec registrados; teste diagnóstico atual 0/1 FAIL vira 1/1 PASS após decisão de spread. |
| AC02 | `ShopSense_BuyRanks_BreadQuotes_19_18_17`: com base 20g, quotes R1/R2/R3 estritamente menores; `SellVariant_OnlyNpcSellGetsBonus` confirma foco exclusivo. | Tooltip e preço exibido iguais à transação; qualquer rank neutralizado por cap em entrada autorada volta à decisão de conteúdo. |
| AC03 | `SellAll_MixedInventory_CapEachSkuBeforeSum`: item listado + não listado com Finan/Lua retorna soma dos dois SKUs após arredondamento e cap, `GoldDelta == CurrentGoldDelta`; `Shipping_DepositD_PaysOnceDPlus1` retorna 0g em D, valor limitado em D+1 e 0g na segunda chamada. | XML EditMode/PlayMode e CSV por `itemId,channel,qty,raw,rounded,cap,total`; preview, resumo e ouro pago coincidem. |
| AC04 | `EconomyBasket_L20L50L54_BuyQuotesUnchanged`: cestas 50/85/130 sem skill e 72/110 R3 nos estágios legais seguem iguais; qualquer variação >10% falha. | CSV de cesta + renda observada L20/L50/L54; mediana pós-política / pré-política entre 0,80 e 1,20, com agenda/seed/tempo/fontes explícitos e parecer de design. |
| AC05 | `RetailCap_MissingOrInvalidCatalog_AllOrdinaryChannelsFailClosed`: SKU listado **e** não listado retornam falha nos quatro canais, ouro/quantidade/pending intactos; `RetailCap_QuestKeyAndNonSellable_NoRemoval`, `ShopStock_SaveLoad_DailyRestock_Preserved`, `Shipping_SaveLoadPending_NoDoublePayment` preservam proteções, stock e pending sem mudar DTO. | PlayMode Farm/Town PASS e diff de DTO/IDs vazio; AC05 geral da fase 21 só PASS após dependências de transformação serem resolvidas e revalidadas. |

`Phase21CrossChannelPriceTests.ListedSellableItems_BuyFocusRanksCannotProfitAcrossOrdinaryResaleChannels`
hoje contém cinco casos `[Explicit]` 0/5 FAIL: remover `Explicit` só após fix e preservar
as cinco combinações neutral/Finan/Âmbar/CropSurge/combinada. A matriz atual inclui
28 shops; gerar segunda tabela para 23 Town e registrar ambos os denominadores.

## §23 — Edge cases e falhas

- SKU listado em duas lojas, um `BuyPriceOverride` menor: mínimo **por SKU**, não por
  loja de destino; repetir com stock finito/`DailyRestock` e respec, sem somar custo de
  setup ao quote.
- Valor de 1g, fração `.5`, lote 2/5/8 e carteira mista: arredondar por unidade no
  canal antes do cap e somar inteiros; overflow recusa a transação sem retirar item.
- `CropPriceSurge` aplicado hoje a item não-crop; registrar ambos cenários no CSV e
  bloquear aceite de regra premium até decisão de elegibilidade.
- Catálogo faltante/stale/duplicado: validador falha; com catálogo ausente ou inválido
  runtime recusa **todo** SKU nos quatro canais ordinários, inclusive SKU potencialmente
  isento, porque sua ausência na lista não é demonstrável; Shipping conserva pending.
- Shipping depositado em D com save/load antes de D+1: recalcular do snapshot simples,
  manter cap válido, processar uma vez e emitir um único resumo.
- Qualidade/raridade de peixe ou output crafted acima do cap: contabilizar perda real;
  não alegar que teto de revenda direta resolve madeira, perca ou couro transformados.

## §24 — Gate de renda e decisões abertas antes de implementar

**Bloqueio de invariante:** o teto `min(BuyR3)` permite empate na loja de menor preço:
pão Renko 17g de compra e 17g de revenda; semente de trigo 4g e 4g. Ele elimina
lucro direto, mas não satisfaz `docs/game_rules/economy_rules.md` (compra estritamente
maior que revenda) nem a skill `economy-balance-tuning`. Um spread inteiro, como
`min(BuyR3)-1g` quando positivo, teria perda legítima adicional. Na matriz atual,
os 27 SKUs têm mínimo de compra R3 de pelo menos 3g, mas um futuro item de compra
1g exige política própria. A projeção do spread estrito colocaria 26/27 SKUs
abaixo do valor-base na venda neutra: pão Renko 20→16g (−20%), semente de trigo
5→3g (−40%); os cinco SKUs vendáveis de Renko somariam 480→380g em lotes de estoque
(−20,8%). São cotações, não renda legítima observada. Recalcular toda a matriz
e medir payout antes de escolher
esse spread ou outra solução. O contrato de §16 continua **proposta não aprovada**.
O fail-closed de §16 também precisa de recuperação visível: catálogo inválido
impediria toda venda ordinária e poderia travar o loop econômico. Definir bloqueio
de build/inicialização e mensagem ao jogador, sem depender de recusa silenciosa em
produção. Se o payout de itens produzidos for inviável, procedência por quantidade
comprada é alternativa de spec própria, com split/merge, baú, crafting, shipping,
save legado e feedback; bloqueio por SKU sem procedência também pune produção legítima.
Um bugfix já restringiu a meta `daily_goal_sell_first_crop` em `ShopSell` a itens
`ItemCategory.Crop` (EditMode 22/22 e Thoren L50 PlayMode pós-fix); eventos
`SellPoint`, `SellAll` e `shipping` ainda agregam itens sem IDs e podem conceder
20g por venda sem colheita. O gate de renda pré/pós deve contabilizar esse prêmio
separadamente e não tratá-lo como receita própria da skill ou do item.

O CSV atual confirma perdas **hipotéticas de payout**, não renda diária: semente de trigo
(`shop_eiran`, lote 5) SellAll 25→20g neutro (−20%), 29→20g com Finan+Lua (−31,0%) e
35→20g com Finan+Lua+CropSurge (−42,9%); pão (`shop_renko`, lote 5) 100→85g
neutro (−15%), 116→85g com Finan+Lua (−26,7%), 142→85g combinado (−40,1%).
Ambas as lojas estão ligadas à TownScene. São colunas
`LotSellAll*`/`HypotheticalCapSellAll*` de `Logs/skills-phase21-price-audit-20260913.csv`,
não transações. Mesmo corrigir `CropPriceSurge` para só cultivo **não** garante perda
`<20%` de renda legítima. Gate obrigatório: agenda canônica L20/L50/L54, renda diária
pré/pós por fonte e sink, mediana observada, perda de peixe/crafting/produto premium,
e revisão independente; se alguma mediana cair mais de 20%, ou qualquer cesta subir
mais de 10%, parar e revisar a política, sem ajustar cap às cegas. A regra para valor
premium futuro (qualidade/raridade), elegibilidade do evento de cultivo, rounding de
SellAll misto e acesso UI efetivo das 23 lojas requerem decisão explícita documentada.
Procedência por lote continua alternativa de outra spec se o teto falhar no gate; esta
spec não a implementa. Lucro por transformação instantânea de madeira, perca e couro
permanece dependência separada; **cap por SKU não corrige esse caminho e AC05 da fase 21
continua FAIL/NOT RUN** até correção e evidência transacional integral.

# /speckit.tasks

## Tarefas ordenadas — blueprint em `features_futuras`, sem autorização de runtime

| Done | ID / owner serial | Entrega verificável / pré-requisito | Cobre |
|---|---|---|---|
| [ ] | T00 design + evidência | Recontar 23 vínculos, medir agenda/renda/cestas/payout premium pré/pós, resolver §24 e registrar aprovação de política. Gate antes de T01. | AC01–AC04 |
| [ ] | T01 Editor/data | Criar catálogo e gerador idempotente §16/§20; CSV 23/70 e 28/91; erros por path/ID e hash estável. | AC01, AC03 |
| [ ] | T02 pricing/runtime | Editar métodos nomeados §20 nos quatro canais, sem DTO/asset de loja/cena; quote, remoção e crédito atômicos. | AC01–AC03, AC05 |
| [ ] | T03 testes | Testes nomeados §22, casos `[Explicit]` executados por filtro, overflow/rounding/proteções/load. | AC01–AC03, AC05 |
| [ ] | T04 PlayMode + renda | Town→Farm real, NPC/SellAll/SellPoint/Shipping D→D+1, estoque e cesta L20/L50/L54; XML/CSV/JSON. | AC01–AC05 |
| [ ] | T05 revisão/closeout | Parecer independente, qualidade, hashes e relatório com PASS/FAIL/NOT RUN por AC; só reavaliar AC05 geral após transformação. | AC01–AC05 |

**Readiness:** o plano descreve contratos propostos, mas T00 e decisões §24 seguem
abertos. A spec permanece em `features_futuras/`; nenhuma checkbox é execução ou PASS.
