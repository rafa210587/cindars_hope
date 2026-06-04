# Cindar's Hope — Economy Pricing & Stock Refresh Direction

> **Status:** documento canônico complementar de preço, valor base, compra/venda, refresh/restock de lojas e canais de venda  
> **Local:** `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`  
> **Complementa:** `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/SPEC_SOURCE_MAP.md`  
> - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`  
> - `docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md`  
> - `docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md`  
> - `docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md`  
> - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`  
> - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`  
> **Função:** declarar mecanicamente como calcular preço, valor base, compra, venda, SellPoint, shop stock, limited stock, refresh/restock e proteção anti-arbitragem.  
> **Não é spec implementável.** Specs futuras devem converter isto em dados, ScriptableObjects, serviços runtime e testes.

---

## 0. Regra anti-duplicação

Este documento **não** redefine:

```text
stats de equipamento;
dano, armor, ASPD, StaminaCost;
vulnerabilidades de inimigos;
loot table de cada monstro;
roster de NPCs;
layout de loja/cidade/fazenda;
receitas completas de cada item;
```

Fontes canônicas:

```text
LOOT_CRAFTING_ECONOMY_DIRECTION.md
  fluxos econômicos, categorias de itens, loot tables, recipes, shops, orders, storage e save/load.

ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
  preço, valor base, buy/sell, refresh/restock, stock state, SellPoint e anti-arbitragem.

EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md
  stats/tags de equipment e itens mágicos; preço usa esses dados, mas não redefine os stats.

MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md
  diferença entre LearnableScroll, CastScroll, Tome, Wand, Staff/Weapon spell e Focus.
```

Regra:

```text
Este documento é a fonte canônica de pricing/restock.
LOOT_CRAFTING_ECONOMY_DIRECTION continua fonte macro de loot/crafting/economia.
```

---

# PARTE A — Termos canônicos

## 1. Preços e valores

```text
BaseValue
  valor econômico bruto do item em Q0, sem canal, reputação, estação, demanda ou markup.

SellPrice
  preço recebido pelo jogador ao vender um item.

BuyPrice
  preço pago pelo jogador ao comprar um item de loja/serviço.

ShippingPrice
  preço recebido pela caixa de envio/SellPoint.

ShopBuyFromPlayerPrice
  preço que loja paga ao jogador.

ShopSellToPlayerPrice
  preço que loja cobra do jogador.

OrderRewardValue
  valor total de recompensa de encomenda, podendo misturar Gold, reputação, item e unlock.

ServicePrice
  custo de serviço: reparo, upgrade, cura, construção, tradução, refino etc.
```

## 2. Regra principal de preço

```text
ShopSellToPlayerPrice > ShopBuyFromPlayerPrice
```

Exceções só podem existir com limite claro:

```text
evento;
quest;
contrato;
reputação alta;
arbitragem limitada por estoque/tempo;
item único;
```

Regra:

```text
Nenhuma spec deve permitir loop infinito de comprar barato e vender caro sem limite.
```

---

# PARTE B — Valor base por categoria

## 3. ItemDefinitionSO deve ter BaseValue

Todo item vendável deve declarar:

```text
ItemId
Category
Rarity
BaseValue
QualityEnabled
CanSell
CanBuy
CanGift
CanDiscard
MaxStack
```

Regra:

```text
BaseValue é obrigatório para item vendável.
CanSell=false ignora preço de venda comum.
QuestItem/KeyItem/Unique devem ser NonSellable ou exigir confirmação forte.
```

## 4. Baseline de BaseValue por categoria

Valores iniciais em Gold/Ouro para Q0 e item comum.

| Categoria | BaseValue inicial | Observação |
|---|---:|---|
| WoodResource comum | 2-6 | recurso abundante |
| StoneResource comum | 2-6 | recurso abundante |
| Fiber/Forage comum | 4-14 | depende da estação/raridade |
| Seed comum | 5-80 | baseado em crop, estação e lucro esperado |
| Crop comum | 8-120 | depende de crescimento, yield e season |
| AnimalProduct comum | 20-180 | depende de animal, cuidado e qualidade |
| Fish comum | 12-160 | depende de raridade/local/clima |
| Ore comum | 8-80 | depende de profundidade/tier |
| Ingot comum | soma dos ores * 1.25-1.60 | processamento/refino |
| Gem comum | 30-250 | mais venda/encomenda/craft raro |
| MonsterPart comum | 8-120 | família/profundidade/risco |
| MonsterPart raro | 80-500 | elite/boss/recipe |
| Reagent comum | 8-80 | alquimia/crafting |
| Component técnico | 40-400 | construct/Bromecia/ruína |
| Food comum | soma dos inputs * 1.10-1.80 | valor depende de buff |
| Potion comum | soma dos inputs * 1.50-3.00 | valor depende do efeito |
| Fertilizer comum | inputs * 1.20-2.20 | valor depende de ganho agrícola |
| CraftedGood comum | inputs * 1.30-2.50 | tempo/estação importam |
| Tool | material + recipe + tier | não usar tabela simples |
| Weapon/Armor/Shield | material + tier + recipe + stats | Equipment docs definem stats |
| ArrowAmmo comum | 1-12 por unidade | stack alto |
| CastScroll T1-T2 | 40-180 | consumível mágico |
| LearnableScroll T1-T2 | 120-500 | ensina magia; mais caro que CastScroll |
| Tome/Grimório | 400-3000+ | persistente/rare/lore |
| Wand | 120-1500+ | item instance com charges |
| Focus/Relic | 250-3000+ | gear persistente |
| Decoration | 20-5000+ | sink opcional |
| BuildingMaterial | 5-150 | depende de tier |

Regra:

```text
Esses ranges são baseline de direção, não números finais de todas as instâncias.
Specs devem começar simples e ajustar por playtest de gold/hour, dia/fazenda/caverna e custos de expansão.
```

## 5. Crop base value

Crops devem ser balanceados por ciclo.

Campos recomendados:

```text
SeedCost
GrowthDays
RegrowDays optional
ExpectedYield
SeasonAvailability
WateringEffort
ProcessingOptions
OrderDemand
```

Fórmula direcional:

```text
CropQ0BaseValue = round((SeedCost * TargetGrossMultiplier) / ExpectedYieldPerSeedCycle)
```

TargetGrossMultiplier inicial:

```text
crop rápido comum: x1.25-x1.45
crop médio comum: x1.40-x1.75
crop longo comum: x1.70-x2.20
crop raro/sazonal: x2.00-x3.00, com gating/risco/tempo
```

Regra:

```text
Lucro de crop deve considerar tempo, água, seed cost, yield, processamento e alternativa de uso.
```

## 6. Processed goods base value

```text
ProcessedBaseValue = sum(InputBaseValues * InputQuantities)
                   * ProcessingMultiplier
                   + StationTierBonusFlat optional
```

Baseline:

```text
processamento simples: x1.20-x1.50
processamento médio: x1.50-x2.00
processamento avançado: x2.00-x3.00
processamento raro/lore: autorado
```

Regra:

```text
Processamento aumenta valor, mas exige tempo/capacidade/estação.
Não deve virar multiplicador infinito sem gargalo.
```

## 7. Equipment base value

Para equipment:

```text
EquipmentBaseValue = MaterialValue
                   + RecipeComplexityValue
                   + TierValue
                   + StatBudgetValue
                   + RarityValue
                   + UniqueModifier optional
```

Regra:

```text
Stats de equipment vêm dos documentos de equipment.
Preço usa tier/material/raridade/stats, mas não redefine WeaponDamage/ASPD/armor.
```

## 8. Magic item base value

```text
CastScrollValue < LearnableScrollValue < TomeValue
```

Regra:

```text
CastScroll consome e não ensina.
LearnableScroll ensina permanentemente se cumprir pré-requisitos.
Tome/Grimório desbloqueia spell, variante ou upgrade persistente.
Wand/Staff/Focus podem fornecer magia temporária ou modificar spell.
```

Baseline:

```text
CastScrollValue = SpellTierBase * ConsumableMultiplier
LearnableScrollValue = SpellTierBase * LearnMultiplier
TomeValue = SpellTierBase * PersistentUnlockMultiplier
WandValue = charges/power/tier/material + spell access value
FocusValue = persistent modifier value + material/tier
```

---

# PARTE C — Fórmulas de compra/venda

## 9. Fórmula geral de venda

```text
SellPrice = floor(BaseValue
                * ChannelSellMultiplier
                * QualityMultiplier
                * RaritySellModifier
                * DemandModifier
                * ReputationSellModifier
                * SeasonModifier
                * StoryFlagModifier)
```

Baseline:

```text
ChannelSellMultiplier:
  SellPoint: x0.90-x1.00
  Loja genérica: x0.75-x0.90
  Loja especializada: x0.95-x1.15 para categorias aceitas
  Encomenda: x1.20-x2.50 equivalente total
  Festival/competição: autorado
```

Regra:

```text
SellPoint é seguro/simples, mas nem sempre melhor.
Loja especializada pode pagar melhor por categoria.
Encomenda paga melhor quando exige qualidade/prazo/item específico.
```

## 10. Fórmula geral de compra

```text
BuyPrice = ceil(BaseValue
              * ShopBuyMultiplier
              * RarityBuyModifier
              * DemandModifier
              * ReputationDiscountModifier
              * SeasonModifier
              * StockScarcityModifier
              * StoryFlagModifier)
```

Baseline:

```text
ShopBuyMultiplier comum: x1.20-x1.80
materiais comuns: x1.15-x1.50
itens raros: x1.80-x3.50
magia/scroll/tome: x1.50-x4.00
itens únicos: autorado ou não vendável
```

Regra:

```text
BuyPrice deve ficar acima do preço de venda equivalente, salvo exceção limitada.
```

## 11. Qualidade

Usar o baseline já definido:

```text
Q0: x1.00
Q1: x1.15
Q2: x1.35
Q3: x1.70
Q4: x2.20
```

Regra:

```text
QualityMultiplier entra em venda, encomendas, presentes e potência de consumível.
Não deve substituir tier de equipamento.
```

## 12. Raridade no preço

Baseline sugerido:

```text
Common: x1.00
Uncommon: x1.20-x1.50
Rare: x1.75-x2.50
Epic: x3.00-x5.00
Legendary: autorado
Unique: autorado / geralmente NonSellable
```

Regra:

```text
Rarity afeta preço, estoque e disponibilidade.
Rarity não é Quality.
```

---

# PARTE D — Canais de venda

## 13. SellPoint / caixa de envio

Funcionamento:

```text
jogador deposita itens;
itens ficam em ShippingBinContents;
ao dormir/virar dia, sistema calcula ShippingPrice;
remove ShippingBinContents;
gera PendingPayments ou aplica GoldChanged;
mostra relatório de venda na manhã seguinte;
```

Campos:

```text
ShippingBinId
AcceptedCategories
RejectedTags
ShippingPriceProfileId
Contents
ProcessedAtDayStart
PendingPayments
LastShippingReport
```

Regras:

```text
SellPoint não compra QuestItem/KeyItem/Unique por padrão.
SellPoint não tem refresh de estoque; ele processa e limpa conteúdo.
SellPoint é canal seguro, não loja.
SellPoint pode pagar menos que loja especializada.
```

## 14. Loja genérica

```text
Compra várias categorias aceitas.
Paga preço menor.
Vende conveniência comum.
Tem restock diário/semanal simples.
```

## 15. Loja especializada

```text
Compra categorias específicas por preço melhor.
Vende itens do domínio do NPC.
Pode ter estoque por reputação, season, quest, farm level ou cave progress.
```

Exemplos:

```text
Blacksmith compra ore/ingot/material de equipamento melhor que loja genérica.
Alchemist compra reagents/potions melhor.
SeedShop vende seeds e compra crops específicos se permitido.
ArcaneShop vende scrolls/wands/tomes/focuses com gating.
```

## 16. Encomendas

```text
Encomenda não é loja.
Encomenda troca item específico por recompensa definida.
Pode pagar mais em valor total por exigir quantidade, qualidade, prazo, raridade ou progress flag.
```

Regra:

```text
Encomenda não deve virar melhor canal sempre.
Ela deve ser melhor quando atende uma condição específica.
```

---

# PARTE E — Shop inventory e refresh/restock

## 17. ShopInventorySO mínimo revisado

```text
ShopId
NpcOwnerId
BaseStockLines
SeasonalStockLines
ReputationStockLines
QuestUnlockedStockLines
CaveProgressStockLines
FarmLevelStockLines
LimitedStockLines
RotatingStockLines
PlayerSoldStockPolicy
RestockPolicy
BuyPriceRules
SellPriceRules
DiscountRules
MarkupRules
ForbiddenItems
ShopOpenScheduleRef
DebugTags
```

## 18. StockLineSO

```text
StockLineId
ItemId
StockType
MinQuantity
MaxQuantity
RestockToQuantity
RestockVariance
RestockChance
RequiredSeason
RequiredReputation
RequiredQuestFlag
RequiredCaveProgress
RequiredFarmLevel
RequiredStoryFlag
StartDay optional
EndDay optional
PurchaseLimitPerDay optional
PurchaseLimitPerWeek optional
PurchaseLimitLifetime optional
IsUniqueStock
CanBeSoldBackToShop
DebugTags
```

## 19. StockType

```text
BaseStock
SeasonalStock
ReputationStock
QuestUnlockedStock
CaveProgressStock
FarmLevelStock
LimitedStock
RotatingStock
UniqueStock
EventStock
TravelingMerchantStock
PlayerSoldStock optional/future
```

## 20. RestockPolicy

```text
DailyMorning
Weekly
SeasonStart
OnQuestFlagChanged
OnReputationTierChanged
OnCaveProgressChanged
OnFarmLevelChanged
EventStart
TravelingMerchantVisit
ManualStoryOnly
Never
```

Regra:

```text
Restock acontece em momentos definidos, normalmente no início do dia, antes da loja abrir.
Restock não deve acontecer a cada abertura de menu.
```

## 21. Restock behavior

```text
BaseStock
  repõe até RestockToQuantity conforme política.

SeasonalStock
  troca no início de estação ou quando a season muda.

ReputationStock
  aparece quando reputação mínima é atingida; pode continuar ou ser substituído por tiers maiores.

QuestUnlockedStock
  aparece após flag de quest/story.

CaveProgressStock
  aparece conforme profundidade/boss/progresso da caverna.

FarmLevelStock
  aparece conforme expansão/nível da fazenda.

LimitedStock
  quantidade limitada por dia/semana/estação ou lifetime.

RotatingStock
  sorteia entre uma pool em intervalos definidos.

UniqueStock
  compra única; não repõe.

TravelingMerchantStock
  existe apenas em visita/evento.
```

## 22. Estoque comprado pelo jogador

Quando jogador compra:

```text
CurrentQuantity -= QuantityPurchased
```

Se `CurrentQuantity = 0`:

```text
linha fica esgotada até próximo RestockPolicy válido;
ou fica esgotada permanentemente se UniqueStock/Lifetime limited.
```

## 23. Itens vendidos pelo jogador para loja

Baseline recomendado:

```text
Itens vendidos pelo jogador NÃO entram automaticamente no estoque de venda da loja.
```

Motivo:

```text
evita exploit;
evita buyback complexo cedo;
evita estoque infinito por revenda;
simplifica UX.
```

Opcional/futuro:

```text
PlayerSoldStockPolicy pode permitir buyback temporário em aba separada.
Buyback sempre deve custar mais do que o valor pago pela loja.
Buyback deve expirar ou ter limite.
```

## 24. Save/load de estoque

Persistir:

```text
ShopStockState
CurrentQuantity por StockLineId
UniqueStockPurchasedFlags
LimitedStockCounters
LastRestockDay
NextRestockDay optional
RotatingStockSeed/Selection
PlayerSoldStock optional
```

Não persistir como fonte primária:

```text
preço final atual, se recalculável;
estoque derivado de flags, se puder ser recalculado e não foi alterado por compra;
```

Regra:

```text
Se jogador comprou/esgotou estoque, persistir CurrentQuantity/counters.
Se loja só desbloqueou stock por flag, pode recalcular a disponibilidade no load.
```

---

# PARTE F — Refresh de loot, nodes e recursos

## 25. Resource node refresh

Para nodes como árvores, pedras, minério, forage e fishing spots:

```text
ResourceNodeId
SourceType
RefreshPolicy
RespawnChance
MinDaysToRespawn
MaxDaysToRespawn
SeasonRules
BiomeRules
CaveFloorRules
DepletedState
QualityRollProfile
QuantityRollProfile
```

## 26. RefreshPolicy de recurso

```text
DailyChance
FixedDays
SeasonStart
OnCaveRunGenerated
OnCaveFloorGenerated
NeverUntilStoryFlag
ManualAuthored
```

Regras:

```text
Fazenda persistente usa respawn por dias/season/story.
Caverna procedural usa geração por run/floor/snapshot conforme regras da caverna.
Node coletado não deve reaparecer no mesmo snapshot sem regra explícita.
```

## 27. Cave loot refresh

```text
Loot de caverna é ligado à run/snapshot.
Treasure, mining nodes e enemy drops devem respeitar seed/snapshot quando aplicável.
Boss first-time reward deve ser separado de repeat reward.
```

Regra:

```text
Boss não deve gerar first-time reward mais de uma vez.
Repeat reward precisa tabela própria, menor e controlada.
```

---

# PARTE G — Anti-arbitragem e economia segura

## 28. Regras anti-exploit

```text
loja compra por menos do que vende;
estoque limitado não reseta ao abrir menu;
restock ocorre por dia/semana/season/evento, não por reload simples;
unique stock tem flag persistida;
buyback, se existir, custa mais que preço pago pela loja;
SellPoint processa no day transition;
encomenda não deve aceitar item comprado na mesma loja com lucro infinito, salvo quest autorada;
rotating stock usa seed/counter persistido quando necessário;
```

## 29. Canais com melhor preço precisam de condição

```text
Loja especializada paga melhor por categoria, mas aceita menos itens.
Encomenda paga melhor, mas exige item/qualidade/quantidade/prazo.
Festival paga melhor, mas é sazonal/evento.
Contrato paga melhor, mas é futuro e limitado.
```

---

# PARTE H — Data assets esperados

## 30. PricingProfileSO

```text
ProfileId
BaseGlobalSellMultiplier
BaseGlobalBuyMultiplier
ChannelSellMultipliers
CategorySellMultipliers
CategoryBuyMultipliers
QualityMultipliers
RaritySellModifiers
RarityBuyModifiers
DemandModifiers
SeasonModifiers
ReputationSellModifiers
ReputationDiscountModifiers
StoryFlagModifiers
MinPrice
RoundingRule
DebugTags
```

## 31. ShopInventorySO

```text
ShopId
NpcOwnerId
StockLines
RestockPolicyId
PriceProfileId
AcceptedSellCategories
RejectedSellTags
SpecialBuyCategories
PlayerSoldStockPolicyId optional
OpenScheduleRef
DebugTags
```

## 32. RestockPolicySO

```text
RestockPolicyId
PolicyType
IntervalDays optional
DayOfWeek optional
SeasonRule optional
RestockAtDayStart
RestockBeforeShopOpen
UsesPersistentRandomSeed
DebugTags
```

## 33. ShippingPriceProfileSO

```text
ProfileId
AcceptedCategories
RejectedTags
ChannelSellMultiplier
QualityMultipliers
CategoryModifiers
ReportGroupingRules
PaymentTiming
DebugTags
```

## 34. ResourceRefreshProfileSO

```text
ProfileId
RefreshPolicy
MinDaysToRespawn
MaxDaysToRespawn
RespawnChance
SeasonRules
BiomeRules
SnapshotRules
DebugTags
```

---

# PARTE I — Specs futuras obrigatórias

```text
spec_pricing_profile_so_contract.md
spec_shop_inventory_stockline_restock_contract.md
spec_shop_buy_sell_price_runtime.md
spec_sellpoint_shipping_price_pending_payment_runtime.md
spec_resource_node_refresh_runtime.md
spec_cave_loot_refresh_snapshot_rules.md
spec_item_base_values_initial_tables.md
spec_economy_anti_arbitrage_tests.md
```

---

# PARTE J — Decisões fechadas

```text
BaseValue é obrigatório para item vendável.
Preço final é recalculável e não deve ser persistido como fonte primária.
ShopSellToPlayerPrice deve ser maior que ShopBuyFromPlayerPrice salvo exceção limitada.
SellPoint processa no day transition e não é loja.
SellPoint pode pagar menos que loja especializada.
Loja especializada pode pagar melhor por categoria, mas aceita menos itens.
Encomenda paga melhor apenas quando exige condição.
Restock acontece por política clara: diário, semanal, estação, quest, reputação, progresso, evento, visita ou nunca.
Restock não acontece ao abrir menu.
UniqueStock não repõe.
LimitedStock persiste counters.
Itens vendidos pelo jogador não entram automaticamente no estoque da loja no baseline.
Buyback é futuro/opcional e deve custar mais que o valor pago pela loja.
Resource nodes persistentes usam refresh por dias/season/story.
Cave loot usa run/snapshot/floor generation conforme regras da caverna.
Boss first-time reward deve ser separado de repeat reward.
```

---

# PARTE K — Pendências de validação

```text
Definir tabela inicial concreta de BaseValue por ItemId.
Validar CropQ0BaseValue contra SeedCost/GrowthDays/Yield.
Validar SellPoint multiplier vs loja especializada.
Validar BuyPrice/SellPrice para impedir arbitragem.
Validar restock diário/semanal/season em save/load.
Validar UniqueStockPurchasedFlags.
Validar ShopStockState após reload.
Validar ResourceNode refresh persistente na fazenda.
Validar Cave loot refresh com snapshot/replay.
Validar boss first-time reward vs repeat reward.
```
