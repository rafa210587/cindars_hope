# Cindar's Hope — Loot, Crafting & Economy Direction

> **Status:** documento canônico de direção de loot, crafting, receitas, processamento, lojas, encomendas, economia e fluxos de recursos  
> **Local:** `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/SPEC_SOURCE_MAP.md`  
> - `docs/design/SPECIFICATION_PROCESS.md`  
> - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`  
> - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`  
> - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`  
> - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`  
> - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`  
> - `docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md`  
> - `docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md`  
> - `docs/design/gameplay/equipment/EQUIPMENT_ENEMY_VULNERABILITY_ADAPTER.md`  
> - `docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_LEVEL_GENERATION_LAYOUT_BIOME_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_ENEMY_BEHAVIOR_ADAPTER.md`  
> - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`  
> **Função:** definir como itens entram, circulam, são processados, viram dinheiro, viram equipamento, viram preparo para a caverna, alimentam progressão da fazenda/cidade e não quebram o balanceamento.  
> **Não é spec implementável.** Specs futuras devem converter isto em dados, ScriptableObjects, tabelas e sistemas.

---

## 0. Regra anti-duplicação

Este documento **não** redefine:

```text
stats mecânicos de armas, armaduras, escudos, arrows, wands e scrolls;
fórmulas de dano/armor/Stamina/Block;
significado de status como Bleed/Burn/Chill/Poison;
vulnerabilidades por família de inimigos;
roster concreto de monstros;
layout da fazenda;
agenda/serviços detalhados de NPCs;
preços finais de todos os itens.
```

Fontes canônicas:

```text
EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
  direção de equipment, materiais, tiers, durabilidade, upgrade e tipos de gear.

EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md
  WeaponDamage, ASPD, StaminaCost, charged effects, armor, shields, bows, arrows, wands, scrolls, tomes e focuses.

EQUIPMENT_ENEMY_VULNERABILITY_ADAPTER.md
  matching entre tags de equipamento e vulnerabilidades de inimigos.

STATUS_EFFECTS_DIRECTION.md
  significado mecânico de status.

CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
  vulnerabilidades por família, janelas, TTK e active combat budget.

CAVE_MONSTER_ROSTER_DIRECTION.md
  monstros concretos, drops autorados, packs, bosses, stats e scaling.

FARM_DESIGN_DIRECTION_v1.3.md
  fazenda como base produtiva/econômica, SellPoint, encomendas, workshops, pedreira final e integração com caverna.

CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
  NPCs, serviços funcionais, comerciantes, artesãos, alquimistas, construtores, curandeiros e pesquisadores.
```

Regra:

```text
Este documento define fluxos, contratos, categorias, regras de economia e direção de balance econômico.
Documentos específicos vencem quando o assunto é fórmula, stat, vulnerabilidade, roster ou layout.
```

---

# PARTE A — Filosofia econômica

## 1. Papel de loot, crafting e economia

Loot, crafting e economia devem conectar os principais pilares do jogo:

```text
fazenda produz base diária;
cidade oferece serviços, receitas, compra/venda e progressão social;
caverna fornece risco, minério, componentes raros e materiais de poder;
equipamentos transformam recursos em estilo de combate;
comida/poções/consumíveis transformam rotina em preparo;
reputação e encomendas dão objetivos direcionados;
ouro organiza ritmo de expansão, não substitui exploração.
```

## 2. Regra principal

O jogo não deve ser apenas sobre vender tudo por ouro.

Cada item deve ter pelo menos uma função clara:

```text
vender por ouro;
usar em crafting;
usar em receita culinária;
usar em poção/fertilizante;
entregar em encomenda;
presentear NPC;
usar em upgrade/reparo;
usar como consumível;
guardar para progressão futura;
servir como item de lore/quest.
```

Itens sem função devem ser evitados.

## 3. Economia deve gerar decisões

Boa economia:

```text
vender agora vs guardar para craft;
usar crop em comida vs vender crop bruto;
usar minério em ferramenta vs arma;
usar componente raro em upgrade vs vender caro;
comprar receita vs investir em expansão;
explorar caverna para material raro vs focar produção segura;
entregar encomenda por reputação vs vender no SellPoint;
comprar consumíveis para avançar mais fundo vs economizar ouro.
```

Economia ruim:

```text
um item sempre tem uma única resposta correta;
vender tudo é sempre melhor;
craftear tudo é sempre melhor;
loja substitui exploração;
caverna substitui fazenda;
fazenda gera ouro infinito sem sinks;
preços escalam sem controle;
loot raro vira comum cedo demais.
```

---

# PARTE B — Loops econômicos

## 4. Loop da fazenda

```text
plantar / cuidar / colher
  -> vender bruto ou processar
  -> comida, poção, presente, encomenda ou craft
  -> ouro/reputação/preparo
  -> expansão, ferramentas, animais, workshops, gear e caverna mais profunda
```

A fazenda deve ser a economia segura e previsível.

Regras:

```text
Crops comuns geram estabilidade, não explosão de ouro.
Produtos processados têm valor maior, mas exigem tempo, construção, receita ou energia logística.
Produtos de qualidade devem ser melhores para encomendas/presentes/receitas.
Farm não deve substituir mineração profunda da caverna.
```

## 5. Loop da caverna

```text
entrar preparado
  -> combater / minerar / abrir tesouro / coletar componentes
  -> decidir recuar ou avançar
  -> converter loot em upgrades, gear, poções, crafting e ouro
  -> preparar próxima run
```

A caverna deve ser a economia de risco.

Regras:

```text
Materiais de equipamento avançado devem vir majoritariamente da caverna.
Bosses e elites devem ter drops mais direcionais, não apenas ouro.
Loot da caverna deve alimentar crafting, upgrades, poções, lore e encomendas raras.
Recompensa de caverna deve compensar risco, mas não invalidar fazenda.
```

## 6. Loop da cidade

```text
comprar semente/receita/serviço
  -> produzir/explorar
  -> vender/entregar/presentear
  -> ganhar ouro/reputação/desbloqueios
  -> abrir novos serviços/estoques/receitas/quests
```

A cidade deve ser a economia social e de serviços.

Regras:

```text
NPCs comerciantes não devem vender tudo desde o início.
Reputação pode liberar estoque, descontos, receitas e encomendas melhores.
Serviços devem ter identidade por NPC/classe funcional.
Lojas não devem substituir boss drops, minério raro ou item de lore.
```

## 7. Loop de crafting

```text
obter blueprint/receita
  -> reunir materiais de fazenda/caverna/cidade
  -> usar workshop/forja/alquimia/cozinha
  -> criar item, upgrade, consumível ou componente
  -> usar, vender, entregar ou equipar
```

Crafting deve ser o conversor entre sistemas.

Regras:

```text
Crafting deve exigir tipos diferentes de fonte: fazenda + caverna + cidade.
Crafting forte deve exigir receita, estação e material raro.
Crafting não deve ignorar skills e progressão.
Crafting não deve exigir grind excessivo de item raro de baixa chance sem alternativa.
```

---

# PARTE C — Moedas, valor e sinks

## 8. Moeda principal

```text
Gold/Ouro é a moeda principal.
```

Uso:

```text
compras comuns;
sementes;
ferramentas básicas;
serviços;
construções;
expansões;
reparo;
receitas;
upgrades;
animais;
decoração;
consumíveis;
licenças/contratos se existirem.
```

Regra:

```text
Ouro deve ser importante, mas não resolver todos os gates sozinho.
Progressão relevante deve combinar ouro + material + receita + reputação/progresso quando fizer sentido.
```

## 9. Reputação não é moeda comum

Reputação deve existir como desbloqueio social/econômico, não como dinheiro alternativo.

Pode afetar:

```text
estoque de loja;
descontos;
preço de venda em canais específicos;
encomendas melhores;
serviços especiais;
visitas à fazenda;
romance/casamento;
companion jobs;
receitas raras;
missões pessoais;
acesso a lore.
```

Regra:

```text
Reputação não deve ser gastável como ouro por padrão.
Ela libera confiança, não substitui economia.
```

## 10. Sinks econômicos

Sinks necessários:

```text
sementes;
animais;
ração;
construções;
expansões;
workshops;
receitas;
ferramentas;
armas;
armaduras;
reparo;
upgrades;
poções/consumíveis;
decoração;
serviços da cidade;
transporte/atalhos futuros;
licenças/contratos futuros;
respec, se houver custo além da Fonte de Anya.
```

Regra:

```text
Sinks devem ser úteis e desejáveis, não punições arbitrárias.
Reparo é sink válido, mas não deve travar o jogador.
Decoração é sink opcional de expressão, não progressão obrigatória.
```

## 11. Anti-inflação

A economia deve evitar ouro infinito cedo.

Ferramentas:

```text
preço de venda menor que valor processado;
processamento exige tempo/capacidade;
sementes e expansão consomem capital;
lojas têm estoque limitado/rotativo;
itens raros têm uso de craft maior que valor de venda;
produtos muito lucrativos exigem estação/receita/tempo;
comida/poções boas competem com venda direta;
mineração profunda exige preparo/custo/risco;
boss drops não são farmáveis infinitamente sem regra.
```

---

# PARTE D — Taxonomia de itens

## 12. Categorias principais

```text
RawResource
Crop
Seed
AnimalProduct
Fish
Forage
WoodResource
StoneResource
Ore
Ingot
Gem
MonsterPart
Reagent
Component
CraftedGood
Food
Potion
Fertilizer
Tool
Weapon
Armor
Shield
Accessory
ArrowAmmo
Wand
Scroll
Tome
Focus
Relic
QuestItem
LoreItem
KeyItem
Decoration
BuildingMaterial
```

Regra:

```text
Cada item deve ter Category e Tags.
Category define comportamento geral.
Tags definem usos específicos.
```

## 13. Tags de uso

```text
Sellable
CraftingMaterial
CookingIngredient
AlchemyIngredient
UpgradeMaterial
RepairMaterial
Giftable
QuestRequired
OrderEligible
FestivalEligible
EquipmentMaterial
Consumable
Placeable
BuildMaterial
LoreLocked
NonSellable
Unique
Stackable
Perishable futuro/opcional
```

Regra:

```text
QuestItem e KeyItem não devem ser vendíveis por padrão.
LoreItem pode ser vendível apenas se não quebrar progressão/lore.
Unique não deve ser perdido em venda acidental sem confirmação forte.
```

---

# PARTE E — Qualidade, raridade e tiers

## 14. Qualidade

Qualidade representa a excelência de produção/coleta.

Baseline:

```text
Q0 — Comum
Q1 — Boa
Q2 — Excelente
Q3 — Perfeita
Q4 — Lendária/Especial
```

Aplicável principalmente a:

```text
crops;
animal products;
fish;
forage;
crafted goods;
food;
potions;
fertilizers;
alguns materiais refinados.
```

Regra:

```text
Qualidade deve afetar venda, encomendas, presentes, potência de comida/poção e chance de craft melhor.
Qualidade não deve substituir tier de material de equipamento.
```

## 15. Raridade

Raridade representa disponibilidade e peso econômico.

```text
Common
Uncommon
Rare
Epic
Legendary
Unique
```

Uso:

```text
chance de drop;
estoque de loja;
custo de craft;
valor de venda;
restrição de encomenda;
importância de lore;
probabilidade de aparecer em treasure.
```

Regra:

```text
Rarity não é igual a Quality.
Um crop comum pode ter Q4.
Um item Unique pode não ter Quality.
```

## 16. Tier

Tier representa progressão técnica/material.

Fonte principal para equipamentos:

```text
EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md
```

Regra:

```text
Tier de equipamento deve conversar com progressão de caverna, mineração, crafting e lojas.
Tier maior não deve ser sempre melhor em tudo; materiais têm identidade e tradeoffs.
```

---

# PARTE F — Loot sources

## 17. Fontes de loot

```text
FarmCrops
FarmAnimals
Fishing
Foraging
Trees
Rocks
FarmQuarryLate
CaveMining
CaveEnemies
CaveElites
CaveBosses
CaveTreasure
SpecialRooms
Traps/TreasureTrap
CityShops
NpcOrders
NpcGifts
Festivals
QuestRewards
LoreEvents
FonteDeAnya
BromecianRuins
```

Regra:

```text
A fonte do item deve fazer sentido com sua função.
Componente de construct deve vir de construct/ruína, não de crop comum.
Ingrediente rural deve vir da fazenda/cidade/natureza, não de boss sem justificativa.
```

## 18. Loot da fazenda

Fazenda gera:

```text
crops;
seeds recuperadas em casos específicos;
frutas comuns;
madeira comum;
pedra comum;
produtos animais;
fish de lago;
itens processados;
fertilizantes;
comida;
reagentes rurais;
recursos leves;
pedreira final apenas no late/endgame.
```

Regras:

```text
Fruto de Mana não é crop comum.
Árvore de Mana é rara/consciente e não vira produção industrial.
Água Viva da Fonte é recurso raro, não commodity.
Pedreira final existe no endgame, mas não substitui mineração de caverna em toda a progressão.
```

## 19. Loot da caverna

Caverna gera:

```text
ore;
gems;
monster parts;
essences;
rare reagents;
construct cores;
blackstone shards/stabilized materials conforme gating;
arcane crystals;
boss components;
treasure gear;
recipes/blueprints raros;
lore items;
key items;
relic fragments.
```

Regras:

```text
Cada família de inimigo deve dropar componentes que façam sentido.
Boss drop deve ser mais direcional e memorável.
Treasure room deve recompensar risco, mas respeitar active budget/traps.
Drops de Pedra Negra devem ter gating e risco.
Nível 101 pode ter loot especial, mas deve preservar importância narrativa de Anya.
```

## 20. Loot de cidade

Cidade fornece:

```text
sementes;
receitas;
ferramentas básicas/intermediárias;
consumíveis;
serviços;
materiais comuns;
decoração;
animais;
licenças/contratos futuros;
blueprints por reputação;
itens raros limitados;
scrolls/pergaminhos em lojas específicas;
wands/focuses em lojas específicas se liberadas.
```

Regras:

```text
Cidade não deve vender boss drops por padrão.
Cidade pode vender materiais comuns e alguns raros em estoque limitado.
Reputação e quests devem abrir estoque avançado.
```

---

# PARTE G — Loot tables

## 21. Tipos de loot table

```text
LootTableSO
EnemyDropTableSO
BossDropTableSO
TreasureTableSO
MiningNodeTableSO
ForageTableSO
FishingTableSO
ShopInventoryTableSO
OrderRewardTableSO
QuestRewardTableSO
FestivalRewardTableSO
```

## 22. Campos mínimos de LootTableSO

```text
LootTableId
SourceType
AllowedBiomes
AllowedFloorRanges
RequiredProgressFlags
RequiredReputationFlags
GuaranteedDrops
WeightedDrops
RareDrops
UniqueDrops
GoldRange
QualityRollProfile
RarityRollProfile
QuantityRollProfile
FirstTimeBonus
RepeatFarmRules
PityRules optional
DebugTags
```

## 23. Garantido vs aleatório

```text
GuaranteedDrops
  recompensas fixas importantes para progressão.

WeightedDrops
  loot comum/variado.

RareDrops
  chance baixa, mas não deve bloquear progressão principal sem alternativa.

UniqueDrops
  item único, normalmente first-time, boss, quest ou lore.
```

Regra:

```text
Progressão principal não deve depender exclusivamente de drop raro aleatório.
Se item raro for necessário, deve haver pity, receita alternativa, compra tardia, quest ou boss guaranteed drop.
```

## 24. Drops de inimigos comuns

Inimigos comuns devem dropar:

```text
pequena chance de material comum;
chance moderada de componente temático;
baixa chance de componente raro;
ouros baixos/moderados conforme família;
itens de craft mais do que gear pronto.
```

Regra:

```text
Inimigo comum não deve ser a fonte principal de equipamento pronto raro.
Ele deve alimentar crafting e economia.
```

## 25. Drops de elites

Elites devem dropar:

```text
componente temático garantido ou quase garantido;
maior chance de componente raro;
ouro melhor;
chance de recipe/blueprint;
chance baixa de gear especial;
```

Regra:

```text
Elite deve parecer recompensa de risco, não apenas versão com mais HP.
```

## 26. Drops de bosses

Bosses devem dropar:

```text
first-time reward relevante;
componente único ou raro;
atalho de progressão de craft;
blueprint/recipe quando fizer sentido;
recurso de lore se aplicável;
ouro significativo, mas não principal recompensa;
```

Regra:

```text
Boss drop deve conectar mecânica, lore e progressão.
Boss não deve virar farm infinito de ouro sem limite.
```

---

# PARTE H — Crafting stations e processamento

## 27. Estações de crafting

```text
Workbench / Bancada
Forge / Forja
Anvil / Bigorna
Kitchen / Cozinha
AlchemyTable / Mesa de Alquimia
FertilizerBin / Composteira
Loom/Tannery futuro
ArcaneBench / Bancada Arcana
BromecianWorkbench / Oficina Bromeciana
RepairStation / Estação de Reparo
CookingPot / Panela
Preserves/Maker Stations
```

Regra:

```text
Cada estação deve ter identidade.
Não criar estação nova se uma existente cobre bem a função.
Não misturar alquimia, forja e culinária em uma estação universal cedo demais.
```

## 28. Tipos de receita

```text
CraftRecipe
CookingRecipe
PotionRecipe
FertilizerRecipe
UpgradeRecipe
RepairRecipe
RefinementRecipe
BuildingRecipe
DecorationRecipe
ArrowRecipe
WandRecipe
ScrollRecipe
FocusRecipe
BromecianRecipe
ArcaneRecipe
```

## 29. Campos mínimos de RecipeSO

```text
RecipeId
DisplayName
RecipeType
OutputItemId
OutputQuantity
RequiredStation
RequiredStationLevel
RequiredIngredients
OptionalIngredients
RequiredGold
RequiredTime
RequiredSkillTreeOrNode
RequiredRecipeUnlock
RequiredReputation
RequiredQuestFlag
RequiredFarmLevel
RequiredCaveProgress
QualityInfluenceRules
FailureRules optional
ByproductRules optional
DebugTags
```

Regra:

```text
Receita forte deve declarar gating.
Receita comum pode ser conhecida automaticamente ou comprada cedo.
Receita de lore/endgame deve depender de progressão, não só ouro.
```

## 30. Processamento

Processamento transforma item bruto em item de maior valor/uso.

Exemplos:

```text
crop -> food ingredient;
fruit -> jam/wine/juice futuro;
milk -> cheese/butter futuro;
egg -> cooked food/mayo futuro;
ore -> ingot;
hide -> leather;
monster part -> reagent;
crystal -> arcane component;
blackstone shard -> stabilized material, se permitido;
herb -> potion;
fish -> food/oil/fertilizer;
wood -> plank;
stone -> block.
```

Regras:

```text
Processamento deve exigir tempo/capacidade/estação.
Processamento aumenta valor, mas cria custo de oportunidade.
Processamento não deve ser obrigatório para todo item comum.
```

---

# PARTE I — Crafting por sistema

## 31. Crafting agrícola

Inclui:

```text
fertilizantes;
sementes especiais;
irrigação;
aspersores/canais/runas hidráulicas;
processadores;
ração;
itens de cuidado animal;
decoração rural;
construções simples;
```

Fontes:

```text
farm crops;
fish;
forage;
wood/stone comuns;
monster reagents específicos;
Bromecian components em automação avançada;
Água Viva/Fonte apenas em crafts raros.
```

## 32. Crafting de equipamento

Fonte canônica de stats:

```text
EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md
```

Este documento define apenas fluxo:

```text
ore -> ingot -> weapon/armor/shield;
hide -> leather -> light armor/accessory;
monster part -> special modifier;
crystal -> staff/wand/focus;
boss component -> unique upgrade;
blackstone stabilized material -> late/endgame gear;
```

Regra:

```text
WeaponDataSO/ArmorDataSO definem stats.
RecipeSO define custo, estação, tempo e gating.
```

## 33. Alquimia

Alquimia cria:

```text
potions;
antidotes;
oils;
elixirs;
fertilizers especiais;
arcane reagents;
purification items;
```

Regras:

```text
Poções fortes devem exigir reagente raro ou estação avançada.
Antídotos básicos devem ser acessíveis antes de veneno ser frequente.
Óleos elementais devem criar counterplay, não dano universal.
Purifying items devem ser raros e ligados a Anya/Light/Radiant quando fizer sentido.
```

## 34. Culinária

Culinária deve preparar o jogador.

Efeitos possíveis:

```text
recuperar HP;
recuperar Stamina;
reduzir fome;
reduzir cansaço futuro/indireto;
buff temporário de resistência;
buff leve de StaminaRegen;
buff de mining/farming/fishing;
buff de defesa contra clima/bioma;
buff social/presente.
```

Regras:

```text
Comida não deve substituir poção em cura instantânea forte.
Comida deve ser parte do preparo antes da run.
Comida de alta qualidade pode durar mais ou ter efeito melhor.
```

## 35. Scrolls, wands, tomes e focuses

Fonte canônica de stats/tags:

```text
EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md
```

Fluxo econômico:

```text
scrolls comuns podem ser comprados/craftados;
scrolls fortes são raros, consumíveis e com gating;
wands usam cargas e/ou MP;
tomes desbloqueiam spells/modificadores;
focuses são gear persistente, não consumível;
```

Regras:

```text
Scroll não deve invalidar skill tree de magia.
Wand não deve superar staff + build de magia.
Tome deve ser recompensa relevante, não item comum de loja inicial.
```

---

# PARTE J — Lojas e serviços

## 36. Tipos de loja/serviço

```text
GeneralStore
SeedShop
Blacksmith
Builder
Alchemist
Healer
AnimalRanch
Fishmonger futuro
ArcaneShop
BromecianResearchService futuro
TempleService
TravelingMerchant
NightShop/Nyx-related se permitido por cidade
```

## 37. Campos mínimos de ShopInventorySO

```text
ShopId
NpcOwnerId
BaseStock
SeasonalStock
ReputationStock
QuestUnlockedStock
CaveProgressStock
FarmLevelStock
LimitedStock
RestockRule
BuyPriceRules
SellPriceRules
DiscountRules
MarkupRules
ForbiddenItems
DebugTags
```

## 38. Regras de loja

```text
Loja vende conveniência, não substitui exploração.
Loja pode vender material comum.
Loja pode vender raro com limite, reputação ou evento.
Loja não vende item único de boss por padrão.
Loja pode comprar categorias específicas por preço melhor.
Loja deve ter identidade de NPC/serviço.
```

## 39. Serviços

Serviços possíveis:

```text
reparo;
upgrade;
construção;
movimento de construção;
compra/venda;
cura;
antídoto;
identificação de item/lore;
tradução de blueprint;
refino;
treinamento/receita;
licença/contrato;
resgate/recuperação futuro;
```

Regra:

```text
Serviço deve ter custo claro em ouro/material/tempo/reputação.
Serviço não deve substituir progressão de skill sem contrapartida.
```

---

# PARTE K — Encomendas, contratos e reputação

## 40. Encomendas

Encomendas são pedidos direcionados.

Tipos:

```text
CropOrder
AnimalProductOrder
FishOrder
ForageOrder
CraftedGoodOrder
PotionOrder
FoodOrder
EquipmentMaterialOrder
MonsterPartOrder
RareLoreOrder
FestivalOrder
LegendaryOrder
```

## 41. Campos mínimos de OrderSO

```text
OrderId
RequesterNpcId
RequiredItems
RequiredQuality
RequiredQuantity
Deadline optional
RewardGold
RewardReputation
RewardItems
UnlockFlags
Repeatable
Seasonal
StoryLocked
FailurePenalty
FlavorText
DebugTags
```

## 42. Regras de encomenda

```text
Encomenda deve pagar mais que venda comum quando exige qualidade, prazo ou item específico.
Encomenda pode pagar menos em ouro e mais em reputação/desbloqueio.
Encomenda de monstro/material raro não deve aparecer antes do jogador acessar a fonte.
Encomenda lendária deve ser late/endgame e pode exigir múltiplos sistemas.
```

## 43. Contratos futuros

Contratos são versões mais formais/complexas de encomendas.

Possíveis usos futuros:

```text
contrato de guilda/local;
contrato de cidade;
contrato de templo;
contrato de caverna;
contrato de pesquisa bromeciana;
contrato de proteção;
contrato de festival.
```

Regra:

```text
Contratos são futuro. Specs atuais podem preparar dados, mas não devem implementar sistema complexo sem roadmap.
```

---

# PARTE L — Preço e valor

## 44. Modelo de preço

Preço final deve considerar:

```text
BaseValue
CategoryMultiplier
QualityMultiplier
RarityMultiplier
ProcessingMultiplier
DemandMultiplier
ReputationModifier
SeasonModifier
ShopSpecificModifier
StoryFlagModifier
```

Regra:

```text
Specs podem começar com tabela simples, mas o modelo deve permitir expansão sem reescrever economia inteira.
```

## 45. Multiplicadores de qualidade

Baseline sugerido:

```text
Q0: x1.00
Q1: x1.15
Q2: x1.35
Q3: x1.70
Q4: x2.20
```

Regras:

```text
Q4 deve ser raro.
Q4 pode ser mais valioso para encomendas/presentes do que venda direta.
Não aplicar Q4 em todos os tipos de item se não fizer sentido.
```

## 46. Venda por canal

Canais:

```text
SellPoint / caixa de envio
loja específica
pedido/encomenda
venda direta para NPC futuro
festival/competição
contrato
```

Regras:

```text
SellPoint é simples e seguro, mas nem sempre melhor preço.
Loja específica pode pagar melhor por categoria.
Encomenda paga melhor quando exige condição.
Festival premia qualidade/especialização.
```

## 47. Compra vs venda

Regra geral:

```text
Preço de compra em loja deve ser maior que preço de venda do jogador para o mesmo item.
```

Exceções controladas:

```text
evento especial;
reputação alta;
contrato;
arbitragem limitada por estoque/tempo;
quest.
```

Regra:

```text
Não permitir loop infinito de comprar barato e vender caro sem limite.
```

---

# PARTE M — Inventário, stacks e armazenamento

## 48. Stack rules

Categorias sugeridas:

```text
Seeds: stack alto
Crops: stack médio/alto
Materials comuns: stack alto
Ores/Ingots: stack médio/alto
MonsterParts: stack médio
Food/Potions: stack baixo/médio
Equipment: não stacka por instance
Unique/Quest: não stacka ou stack especial
Arrows: stack alto
Scrolls: stack médio/baixo conforme raridade
Wands: não stacka por charges/instance
```

## 49. Item instance vs item stack

```text
ItemStack
  item comum com quantidade e talvez quality.

ItemInstance
  equipamento, wand, item com durabilidade, upgrade, charges, affixes ou estado próprio.
```

Regra:

```text
Equipamento, wands e itens com durabilidade/charges devem ser instanciados.
Crops de mesma qualidade podem stackar.
Quest items devem ter proteção contra venda descarte.
```

## 50. Storage

Tipos:

```text
player inventory;
hotbar;
chests;
category storage;
workshop input/output;
shipping bin;
quest/order delivery buffer;
city shop transaction buffer;
```

Regra:

```text
Crafting deve poder ler storage autorizado somente se a spec permitir.
No início, evitar automação excessiva de storage para não esconder aprendizado.
Late game pode permitir storage central/automação.
```

---

# PARTE N — Save/load

## 51. Dados persistidos

```text
ItemStacks
ItemInstances
Quality
Quantity
Durability
Charges
UpgradeLevel
Material/Tier
Affixes/modifiers futuro
KnownRecipes
UnlockedBlueprints
ShopStockState
OrderState
CraftingStationState
ProcessingTimers
ShippingBinContents
PendingPayments
PriceModifiers ativos
```

## 52. Dados recalculáveis

Não persistir como fonte primária:

```text
preço final atual, se pode ser recalculado;
stats derivados de equipamento;
bônus temporário expirável;
chance de drop;
valor de tooltip;
```

Regra:

```text
Persistir estado autorado/instanciado.
Recalcular derivados no load.
```

---

# PARTE O — Data assets esperados

## 53. Assets principais

```text
ItemDefinitionSO
ItemCategorySO optional
ItemTag enum/data
QualityProfileSO
RarityProfileSO
LootTableSO
EnemyDropTableSO
TreasureTableSO
MiningNodeTableSO
ForageTableSO
FishingTableSO
ShopInventorySO
ShopPriceRulesSO
RecipeSO
CraftingStationSO
ProcessingRecipeSO
OrderSO
RewardTableSO
ShippingPriceProfileSO
EconomyBalanceProfileSO
```

## 54. ItemDefinitionSO mínimo

```text
ItemId
DisplayName
Description
Category
Tags
Rarity
BaseValue
MaxStack
QualityEnabled
CanSell
CanGift
CanDiscard
CanCraftWith
CanCookWith
CanUseAsIngredient
IsQuestItem
IsUnique
IconId
SpriteId
LoreTags
DebugTags
```

## 55. EconomyBalanceProfileSO mínimo

```text
ProfileId
GlobalSellMultiplier
GlobalBuyMultiplier
QualityMultipliers
RarityMultipliers
ProcessingMultipliers
ShopCategoryModifiers
OrderRewardMultipliers
GoldDropRangesByTier
RepairCostRules
UpgradeCostRules
AntiInflationRules
DebugTags
```

---

# PARTE P — Regras de balance transversal

## 56. Fazenda vs caverna

```text
Fazenda = estabilidade, comida, consumíveis, ouro previsível, presentes, encomendas.
Caverna = risco, materiais raros, equipamento avançado, boss components, lore.
Cidade = serviços, receitas, lojas, reputação, compra/venda especializada.
```

Regra:

```text
Nenhum pilar deve substituir completamente os outros.
```

## 57. Crafting vs compra

```text
Comprar deve ser conveniente.
Craftar deve ser eficiente ou permitir itens melhores/específicos.
Drops raros devem alimentar crafting, não apenas venda.
```

Regra:

```text
Crafting deve ser uma opção estratégica, não obrigação para cada item trivial.
```

## 58. Loot raro

```text
Loot raro pode acelerar progressão ou abrir build.
Loot raro não deve bloquear caminho principal se chance for baixa.
Unique deve ter proteção contra perda acidental.
```

## 59. Reparo e durabilidade

Fonte canônica de durabilidade/equipment:

```text
EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md
```

Direção econômica:

```text
reparo usa ouro + material;
reparo cria sink;
reparo não deve punir exploração normal demais;
DurabilityStress pode aumentar custo/urgência, mas não destruir item sem sistema claro.
```

---

# PARTE Q — Roadmap de specs futuras

## 60. Specs recomendadas

```text
spec_item_definition_so_contract.md
spec_item_tags_categories_quality_rarity.md
spec_loot_table_so_contract.md
spec_enemy_drop_tables_by_family.md
spec_cave_treasure_mining_loot_tables.md
spec_recipe_so_and_crafting_station_contract.md
spec_processing_recipes_and_timers.md
spec_shop_inventory_price_rules.md
spec_shipping_bin_pending_payment_runtime.md
spec_orders_reputation_rewards_runtime.md
spec_inventory_stack_instance_storage_rules.md
spec_economy_balance_profile.md
spec_repair_upgrade_cost_rules.md
spec_bows_arrows_scrolls_wands_item_instances.md
```

---

# PARTE R — Decisões fechadas

```text
Ouro é moeda principal.
Reputação é desbloqueio social/econômico, não moeda comum.
Loot/crafting/economia devem conectar fazenda, cidade e caverna.
Fazenda gera estabilidade e preparo; caverna gera risco e materiais raros; cidade gera serviços e mercado.
Todo item deve ter função clara.
Venda direta não deve ser sempre melhor que craft/processamento/encomenda.
Progressão principal não deve depender só de drop raro aleatório.
Boss drop deve ser direcional, memorável e conectado a lore/progressão.
Lojas vendem conveniência, não substituem exploração.
Crafting forte exige estação, receita, material e gating.
Processamento aumenta valor, mas exige tempo/capacidade.
Qualidade é diferente de raridade.
Tier é diferente de qualidade e raridade.
Equipamento/wands/itens com durabilidade ou charges usam ItemInstance.
Crops/materiais comuns usam ItemStack.
QuestItem/Unique precisam proteção contra venda/descarte acidental.
Fruto de Mana, Água Viva da Fonte e Pedra Negra estabilizada não são commodities comuns.
Pedreira final da fazenda é late/endgame e não substitui toda mineração da caverna.
```

---

# PARTE S — Pendências de validação

```text
Validar preços reais contra tempo médio de dia/fazenda.
Validar gold/hour da fazenda vs gold/hour da caverna.
Validar custo de sementes vs lucro de crops comuns.
Validar valor de processamento vs tempo/capacidade.
Validar custo de reparo contra frequência de DurabilityStress.
Validar drops por família contra recipes de equipment.
Validar boss drops contra progressão de gear.
Validar loja para não substituir cave/farm.
Validar encomendas para não virarem melhor canal sempre.
Validar stack sizes e inventory pressure.
Validar save/load de pending payments, crafting timers e item instances.
```
