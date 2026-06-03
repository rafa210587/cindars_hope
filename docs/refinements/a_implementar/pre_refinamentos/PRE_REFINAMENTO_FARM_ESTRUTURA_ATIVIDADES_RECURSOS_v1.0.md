# Cindar's Hope — Pré-refinamento FARM: Estrutura, Atividades, Construções e Recursos v1.0

> **Status:** pré-refinamento para discussão e evolução documental  
> **Origem:** `PRE_REFINAMENTO_VISAO_GERAL_JOGO_v1.1.md` + specs implementadas/parciais de Farm, World Activities, Inventory, Crafting e Economy  
> **Destino:** `docs/refinements/a_implementar/pre_refinamentos/PRE_REFINAMENTO_FARM_ESTRUTURA_ATIVIDADES_RECURSOS_v1.0.md`  
> **Natureza:** refinamento de produto/sistemas; **não é spec implementável ainda**.

---

## 1. Objetivo

Refinar como a fazenda funciona em Cindar's Hope:

- tamanho e expansão;
- zonas internas;
- construções;
- atividades diárias;
- recursos produzidos;
- regras de solo, rega, crescimento, morte de planta, qualidade e fertilizante;
- árvores, lago, pesca e mineração leve;
- animais;
- automação por companions;
- economia e caixa de envio;
- save/data contracts futuros;
- separação entre MVP, V2 e FULL.

A fazenda deve ser tratada como **base viva do jogador**, não apenas como mapa inicial.

---

# PARTE A — Identidade da Fazenda

## 2. Função da fazenda

A fazenda é o centro seguro do jogador e cumpre quatro funções:

1. **Base produtiva:** gera comida, materiais, dinheiro e ingredientes.
2. **Base de preparação:** permite criar poções, ferramentas, equipamentos e recursos para a caverna.
3. **Base narrativa:** abriga a Fonte de Anya e vestígios da história de Cindar.
4. **Base social:** recebe companions, visitas de NPCs, animais e possíveis eventos.

A caverna fornece risco e recursos raros. A cidade fornece comércio, quests e relações. A fazenda transforma isso em progresso controlável.

## 3. Promessa da fazenda

> Começar com um pedaço de terra simples e transformá-lo em uma base produtiva, organizada, automatizável e conectada ao mistério de Anya.

A fazenda deve melhorar visualmente e mecanicamente conforme o jogador progride.

## 4. Regra de identidade

Toda feature de fazenda deve servir pelo menos uma destas funções:

- produzir recurso;
- preparar para a caverna;
- alimentar crafting;
- gerar economia;
- criar rotina diária interessante;
- sustentar relação com companions/NPCs;
- reforçar lore de Anya/Cindar/Fonte;
- desbloquear expansão ou progressão.

Se não servir nenhuma, deve ser decoração ou polish.

---

# PARTE B — Tamanho e Expansão

## 5. Decisão de escala

Separar duas coisas:

```text
MVP técnico = mapa pequeno de validação.
Fazenda canônica = mapa modular com expansões.
```

O MVP pode continuar com mapa reduzido. A fazenda final deve ser modular, maior que uma tela, mas não grande a ponto de tornar deslocamento cansativo.

## 6. Unidade de medida

- Tile base: `32x32px`.
- Player: `32x48px`.
- Câmera alvo: `1280x720`.
- Uma tela visível em 100% equivale a aproximadamente `40x22.5 tiles`.

## 7. Tiers de expansão propostos

Os nomes `1x`, `2x`, `3x`, `4x` devem ser entendidos como tiers de expansão, não como multiplicadores matemáticos exatos.

| Tier | Nome | Tamanho sugerido | Função |
|---:|---|---:|---|
| 0 | Slice/MVP | 20x16 tiles | Teste técnico de plantar, colher, vender e salvar |
| 1 | Fazenda Inicial | 40x32 tiles | Loop completo inicial, Fonte, lago, árvores, entrada da caverna |
| 2 | Fazenda Expandida | 56x40 tiles | Mais canteiros, pasto pequeno, mais árvores, 2º espaço de workshop |
| 3 | Fazenda Estabelecida | 72x52 tiles | Estufa, área animal maior, pedreira melhor, automação inicial |
| 4 | Fazenda Plena | 88x64 tiles | Layout completo, 4 workshops, pasto grande, decoração, automação avançada |

## 8. Regra de expansão

Cada expansão deve desbloquear pelo menos um novo tipo de decisão.

| Expansão | Deve desbloquear |
|---|---|
| Tier 1 | loop básico de farm, Fonte, lago, árvores, SellPoint, entrada da caverna |
| Tier 2 | pasto pequeno, mais canteiros, mais árvores, mais construção |
| Tier 3 | estufa, animais adicionais, automação inicial, pedreira melhor |
| Tier 4 | layout completo, produção avançada, 4 workshops, companion housing expandido |

## 9. Custos de expansão

Expansão deve custar sempre uma combinação de:

- ouro;
- madeira;
- pedra/minério;
- material especial conforme tier.

Tabela inicial de intenção:

| Expansão | Requisito | Recursos principais | Observação |
|---|---|---|---|
| Tier 1 → 2 | nível baixo/médio | ouro + madeira | libera pasto pequeno |
| Tier 2 → 3 | nível médio | ouro + madeira + minério + vidro | libera estufa |
| Tier 3 → 4 | nível alto | ouro + ingots + prata + vidro | libera produção plena |
| Pasto | sub-expansão | ouro + madeira + minério | independente da expansão principal |
| Estufa | construção | madeira + vidro + recurso raro | depende de terreno expandido |

## 10. Expansão visual

A expansão deve ser diegética:

- Hund & Gurd ou construtores equivalentes limpam área;
- cercas são movidas;
- entulho, arbustos e pedras somem;
- novas zonas ficam acessíveis;
- UI mostra custo, prévia e confirmação.

---

# PARTE C — Zonas da Fazenda

## 11. Zonas obrigatórias

| Zona | Função | Tier mínimo |
|---|---|---:|
| Casa | dormir, salvar, cozinhar, guardar, trocar aparência | 1 |
| Campo de plantio | canteiros, irrigação, fertilizante, colheita | 1 |
| Fonte de Anya | respec, ressurreição de companions, lore | 1 ou ativada por quest |
| Lago | pesca, água, recarga de watering can | 1 |
| Bosque | árvores, madeira, reflorestamento | 1 |
| SellPoint / Caixa de envio | vender produção | 1 |
| Entrada da caverna | preparação e acesso à CaveScene | 1 |
| Área de workshops | Forja, Alquimia, Carpintaria, Costura | 1–4 |
| Depósito / baús | organização de recursos | 1 |
| Pasto | animais grandes | 2 |
| Galinheiro / curral pequeno | animais pequenos | 2 ou 3 |
| Estufa | plantio fora de estação | 3 |
| Pedreira leve | pedra, cobre, argila inicial | 1–3 |
| Área de companions | moradia, tarefas, gestão | 2–4 |
| Área decorativa | cosmética, eventos, identidade visual | 3–4 |

## 12. Layout conceitual

```text
┌──────────────────────────────────────────────────────────────┐
│ Bosque / Árvores        | Pedreira leve / Rochas              │
│ Campo principal         | Lago + Fishing Dock                 │
│ Casa + Baús + Fonte     | Área de Workshops                   │
│ SellPoint / Caminho     | Entrada da Caverna                  │
│ Pasto / Animais         | Estufa / Área avançada              │
└──────────────────────────────────────────────────────────────┘
```

Regras de leitura:

- casa perto do centro seguro;
- campos próximos da casa;
- SellPoint perto do caminho da cidade;
- cave gate em uma borda;
- lago separado dos canteiros;
- animais em área própria;
- workshops agrupados.

---

# PARTE D — Construções

## 13. Categorias

1. **Essenciais:** casa, campo, SellPoint, Fonte, entrada da caverna.
2. **Produção:** estufa, pasto, galinheiro, depósitos, irrigação.
3. **Crafting:** Forja, Alquimia, Carpintaria, Costura.
4. **Suporte:** baús, poço, tanque de água, companion housing.
5. **Decorativas/Polish:** bancos, cercas, flores, estátuas, luzes.

## 14. Casa

Funções:

- dormir;
- avançar dia;
- salvar;
- restaurar stamina;
- organizar baús;
- cozinhar no futuro;
- trocar roupa/aparência no futuro;
- ver calendário/diário no futuro.

Upgrades:

| Nível | Função |
|---:|---|
| Casa 1 | cama, save, baú simples |
| Casa 2 | cozinha, mais espaço, decoração |
| Casa 3 | quarto de guest/companion, storage maior |
| Casa 4 | estação de planejamento, calendário, buffs domésticos |

## 15. SellPoint / Caixa de envio

Duas modalidades:

| Canal | Vantagem | Desvantagem |
|---|---|---|
| SellPoint/caixa | rápido, conveniente | preço base menor |
| Loja/NPC | preço melhor, reputação, quests | exige ir à cidade |
| Venda especial | maior lucro por demanda/evento | condicional |

Recomendação:

```text
MVP/V2: venda imediata por SellPoint ou loja.
FULL: caixa de envio na fazenda + lojas especializadas na cidade.
```

## 16. Fonte de Anya

A Fonte é construção/lugar essencial de identidade.

Funções futuras:

- respec;
- ressurreição de companions;
- eventos de lore;
- purificação de itens;
- interação com lagos da caverna;
- upgrades por fragmentos de Anya.

Estados possíveis:

```text
Dormant
Awakened
Strengthened
Purified
Unstable
```

## 17. Workshops

| Workshop | Produz | Entrada principal | Uso |
|---|---|---|---|
| Forja | ferramentas, armas, armaduras metálicas | minérios, madeira, carvão | caverna, farm, combate |
| Alquimia | poções, fertilizantes avançados, reagentes | plantas, cogumelos, peixe, minerais raros | cura, status, exploração |
| Carpintaria | estruturas, baús, irrigação, armadilhas, canas | madeira, pedra, vidro | fazenda, pesca, expansão |
| Costura | roupas, bolsas, capas, armaduras leves | lã, tecido, couro, reagentes | inventário, defesa, buffs |

Crafting deve acontecer nos workshops, não pela mochila.

## 18. Estufa

Funções:

- plantar no inverno;
- plantar sementes fora de estação;
- proteger de clima extremo;
- permitir sementes noturnas com luz artificial, se aprovado;
- servir como espaço de crops raros.

Regras:

- não entra no MVP;
- exige vidro e material de caverna/cidade;
- capacidade menor que campo aberto;
- produção estratégica, não substitui toda a fazenda.

## 19. Pasto, poço e depósito

Pasto:

- sub-expansão;
- pasto pequeno libera até 2 animais grandes;
- pasto grande libera até 6 animais grandes.

Poço/tanque:

- watering can recarrega no poço, lago ou fonte de água comum;
- FULL pode ter tanque, canais e sprinklers.

Depósito:

- baú simples no início;
- baús por categoria no futuro;
- depósito central no mid-game;
- definir depois se workshops puxam do depósito ou só do inventário.

---

# PARTE E — Plantio, Rega, Qualidade e Fertilizantes

## 20. Estados de solo

Base a manter:

```text
Raw
TilledDry
TilledWet
PlantedDry
PlantedWet
ReadyToHarvest
Blocked
Dead
```

Fluxo:

```text
Raw -> arar -> TilledDry
TilledDry -> molhar -> TilledWet
TilledDry/TilledWet -> plantar -> PlantedDry/PlantedWet
PlantedWet -> cresce ao avançar dia
ReadyToHarvest -> colher
Dead -> limpar
```

## 21. Rega

Decisão consolidada:

- planta precisa de água para crescer;
- se não for regada, não avança crescimento;
- se ficar 3 dias sem rega, morre no design final;
- no MVP técnico atual, planta seca não cresce e ainda não morre.

Regra FULL sugerida:

```text
Se RequiresWater == true:
  Se PlantedWet:
    cresce +1
    DaysWithoutWater = 0
    vira PlantedDry depois do processamento
  Se PlantedDry:
    não cresce
    DaysWithoutWater += 1
    se DaysWithoutWater >= 3:
      State = Dead
```

## 22. Chuva, season e período

Chuva:

- molha todos os plots externos;
- pode encher tanque de água;
- não destrói crops no jogo base.

Season:

- seed cresce apenas em estações válidas;
- inverno bloqueia solo externo comum;
- estufa ignora estação.

Período:

```text
Day
Night
Both
SpecialMoon
```

Alihana pode melhorar seeds noturnas; Nyx pode habilitar crops raras; Senya pode alterar variação de crops mágicos.

## 23. Rebrota

Campos necessários:

```text
IsRegrowable
InitialGrowthDays
RegrowDays
DiesAfterHarvest
```

Regras:

- sem rebrota: colheita retorna plot para `TilledDry`;
- com rebrota: colheita retorna para `PlantedDry` com `RegrowDays` reiniciado;
- planta de rebrota ainda precisa de água;
- fertilizante dura só um ciclo.

## 24. Qualidade

Qualidades recomendadas:

```text
Normal
Boa
Excelente
Rara
Lunar
```

Influenciada por:

- fertilizante;
- rega consistente;
- skill;
- estação correta;
- lua ativa;
- crop raro;
- estufa;
- eventos especiais.

Regra técnica recomendada:

```text
StackKey = ItemId + Quality
```

Evitar IDs separados por qualidade se o inventário suportar metadado.

## 25. Fertilizantes

| Tier | Nome funcional | Efeito |
|---:|---|---|
| 1 | Simples | reduz crescimento em 1 dia ou aumenta chance de qualidade Boa |
| 2 | Intermediário | aumenta yield |
| 3 | Avançado | melhora qualidade |
| 4 | Lunar/Especial | interage com lua/crops mágicos |

Fontes:

| Fonte | Fertilizante |
|---|---|
| Animais | material base / simples |
| Alquimia | intermediário/avançado |
| Caverna | reagentes raros |
| Cidade | compra limitada |
| Quests | receitas especiais |

Regras:

- 1 fertilizante por plot/ciclo;
- efeito salvo no plot;
- se planta morrer, fertilizante é perdido;
- fertilizante avançado depende de crafting/caverna.

---

# PARTE F — Seeds, Crops e Recursos

## 26. Sementes base

| Semente | Período | Função principal | Tipo sugerido |
|---|---|---|---|
| Trigo | Diurno | comida/farinha/venda | colheita única |
| Cenoura | Neutro | comida/poção de visão | colheita única |
| Erva Medicinal | Noturno | cura/alquimia | possível rebrota |
| Cristal-Flor | Noturno | reagente mágico | colheita única/rara |
| Batata | Neutro | comida/stamina | colheita única |
| Cogumelo-Sombra | Noturno | veneno/antídoto | rebrota |
| Girassol de Fogo | Diurno | fogo/poção de calor | colheita única |
| Flor-de-Gelo | Noturno | frio/poção de frio | colheita única |
| Pimenta-Vermelha | Diurno | buff físico/comida | rebrota |
| Musgo-Lunar | Noturno | mana/reagente lunar | rebrota |

## 27. Dados mínimos de SeedDataSO

```text
Id
DisplayName
Description
SeedItemId
HarvestItemId
GrowthDays
RegrowDays
RequiresWater
ValidSeasons
GrowthPeriod
MinYield
MaxYield
BaseQualityChance
CanGrowInGreenhouse
CanBeFertilized
StageSprites
```

## 28. Recursos da fazenda

| Categoria | Recursos |
|---|---|
| Crops | trigo, cenoura, ervas, flores, cogumelos, pimentas |
| Madeira | madeira comum, madeira especial futura |
| Peixe | peixe comum, peixes por estação/horário |
| Mineração leve | pedra, cobre, argila |
| Animais | leite, ovos, lã, fertilizante |
| Crafting | poções, ferramentas, estruturas, tecidos |
| Lore/Fonte | fragmentos/água especial em eventos futuros |

Regra econômica: nenhum recurso deve ter apenas um uso para sempre. Mesmo que no MVP seja vendido, no V2/FULL deve ganhar utilidade em receita, quest, construção, buff, upgrade, troca ou reputação.

---

# PARTE G — Atividades, Ferramentas e Stamina

## 29. Atividades diárias

- arar;
- molhar;
- plantar;
- fertilizar;
- colher;
- limpar planta morta;
- cortar árvores;
- pescar;
- minerar pedras leves;
- alimentar animais;
- coletar produtos animais;
- craftar;
- vender/enviar itens;
- dormir;
- gerenciar companions;
- organizar baús;
- preparar ida à caverna.

## 30. Ferramentas mínimas

| Ferramenta | Função |
|---|---|
| Hoe | arar solo |
| Watering Can | molhar solo |
| Axe | cortar árvores |
| Pickaxe | quebrar rochas/pedreira |
| Fishing Rod | pescar |
| Sickle | limpar plantas mortas/fibra, se aprovado |
| Hammer/Build Tool | mover/construir, se aprovado |

## 31. Stamina e fome

Separação recomendada:

```text
Stamina = recurso de ações ativas.
Fome = necessidade contínua / manutenção corporal.
HP = sobrevivência.
```

Custos provisórios:

| Ação | Custo |
|---|---:|
| Plantar | 1 |
| Colher | 1 |
| Arar | 2 |
| Molhar | 1 |
| Fertilizar | 1 |
| Cortar árvore | 4 |
| Minerar rocha | 4 |
| Pescar | 5 |
| Alimentar animal | 1 |
| Coletar produto animal | 1 |

---

# PARTE H — Árvores, Lago e Mineração Leve

## 32. Árvores

Regras:

- começa com 4 árvores;
- limite futuro pode ser 14;
- árvores dão madeira;
- árvores podem ter regrowth;
- ferramenta melhor aumenta eficiência/yield.

Tipos futuros:

| Tipo | Função |
|---|---|
| Carvalho | madeira comum |
| Pinheiro | madeira resinosa/crafting |
| Árvore Frutífera | fruta sazonal |
| Árvore Lunar | recurso mágico raro, FULL |

## 33. Lago

Funções:

1. pesca;
2. recarga de água;
3. identidade visual/cozy.

Evolução:

| Fase | Função |
|---|---|
| MVP | pescar Peixe Comum |
| V2 | peixes por horário/estação |
| FULL | peixes raros, eventos lunares, dock, quests |

## 34. Mineração leve

A fazenda pode ter pequena área de rocha para:

- pedra;
- cobre;
- argila;
- recurso básico para construção.

Materiais avançados vêm da caverna. A fazenda não deve substituir mineração de caverna.

---

# PARTE I — Animais

## 35. Papel dos animais

Animais entram pós-MVP e geram:

- fertilizante;
- alimento;
- matéria-prima;
- rotina;
- conexão com companions.

## 36. Animais propostos

| Animal | Produto | Construção | Fase |
|---|---|---|---|
| Vaca | leite + fertilizante base | pasto/curral | V2 |
| Galinha | ovos | galinheiro | V2/FULL |
| Ovelha | lã | curral/pasto | FULL |
| Animal fantástico pequeno | recurso raro | construção especial | FULL |

## 37. Rotina e estados

Rotina:

```text
1. Alimentar.
2. Verificar estado.
3. Coletar produto se disponível.
4. Aplicar produção de fertilizante.
5. Salvar estado.
```

Estados:

```text
Healthy
Hungry
Starving
Sick
Dead
```

Regra de tom: sem detalhamento pesado, sem visual gráfico, feedback simples e funcional. Se possível, preferir recuperação paga/tratamento antes de morte permanente.

---

# PARTE J — Companions na Fazenda

## 38. Jobs ligados à fazenda

| Job | Função | Limite |
|---|---|---|
| Plantador | planta seeds escolhidas | precisa de seeds e plano |
| Regador | molha plots | limitado por energia/ferramenta |
| Colhedor | colhe crops prontos | envia para baú/depósito |
| Lenhador | corta árvores | respeita limite de árvores |
| Pescador | pesca no lago | produção por dia |
| Minerador | coleta rochas leves | não substitui caverna |
| Artesão | opera workshop/fila | precisa de receita/ingredientes |
| Tratador | cuida de animais | precisa de ração/feno |
| Organizador | move itens para baús | FULL/opcional |

## 39. Regra de automação

Automação reduz repetição, mas mantém decisão.

O jogador define:

- companion;
- job;
- área;
- prioridade;
- se pode gastar recursos;
- onde entregar output.

---

# PARTE K — Economia e Rotina Diária

## 40. Fontes de renda

- crops;
- peixe;
- madeira/pedra excedente;
- produtos animais;
- itens craftados;
- quests;
- encomendas de NPCs;
- eventos sazonais.

## 41. Venda comum vs encomendas

Venda comum:

- sempre disponível;
- preço base;
- rápida.

Encomendas:

- pedido específico de NPC;
- prazo;
- recompensa maior;
- aumenta reputação.

## 42. Qualidade e preço

| Qualidade | Multiplicador inicial |
|---|---:|
| Normal | 1.0x |
| Boa | 1.25x |
| Excelente | 1.5x |
| Rara | 2.0x |
| Lunar | 2.5x ou efeito especial |

## 43. Ordem de processamento ao dormir/avançar dia

```text
1. Processar caixa de envio/vendas pendentes.
2. Processar crops: água, crescimento, dias secos, morte, reset.
3. Processar regrow de crops.
4. Processar árvores: regrowth e corte.
5. Processar lago: pool de pesca/eventos.
6. Processar rochas/pedreira: respawn parcial.
7. Processar animais: alimentação, produção, estado.
8. Processar companions: jobs e retornos.
9. Processar eventos: lua, estação, quests, NPCs.
10. Salvar estado.
11. Iniciar novo dia.
```

---

# PARTE L — Save e Dados

## 44. FarmSaveData futuro

```text
FarmSaveData
  ExpansionTier
  CurrentFarmDayState
  Plots[]
  Trees[]
  Rocks[]
  FishingSpots[]
  Buildings[]
  Animals[]
  CompanionAssignments[]
  ShippingBin
  FountainState
  IrrigationState
```

## 45. PlotSaveData futuro

```text
PlotId
Position
State
SeedId
GrowthProgressDays
DaysWithoutWater
FertilizerId
FertilizerRemainingMode
QualityRollState
RegrowRemainingDays
LastWateredDay
LastUpdatedDay
```

## 46. BuildingSaveData

```text
BuildingInstanceId
BuildingId
Position
Rotation
Level
State
ConstructionRemainingDays
AssignedCompanionId
InternalInventoryRef
```

## 47. AnimalSaveData

```text
AnimalInstanceId
AnimalDataId
Name
HomeBuildingId
Age
FedToday
DaysWithoutFood
HealthState
ProductReady
AffinityOrCareScore
```

## 48. Regras de save

- salvar IDs e tipos simples;
- não salvar ScriptableObject;
- não salvar GameObject/Transform/MonoBehaviour;
- migrations precisam de defaults seguros;
- se um ID sumir, logar erro claro e falhar de forma segura.

---

# PARTE M — ScriptableObjects sugeridos

## 49. FarmConfigSO

```text
FarmConfigSO
  TileSize
  StartingExpansionTier
  StartingPlots
  StartingTrees
  StartingFishingSpots
  StartingRocks
  MaxExpansionTier
  DayAdvanceRules
```

## 50. FarmExpansionDataSO

```text
FarmExpansionDataSO
  ExpansionTier
  DisplayName
  WidthTiles
  HeightTiles
  UnlockRequirements
  BuildCost
  UnlockedZones
  MaxPlots
  MaxTrees
  MaxAnimals
  MaxBuildingPads
```

## 51. FarmBuildingDataSO

```text
FarmBuildingDataSO
  BuildingId
  DisplayName
  Category
  SizeInTiles
  BuildCost
  BuildDays
  UpgradeLevels
  RequiredExpansionTier
  ProvidesFunction
  SpriteByLevel
```

## 52. AnimalDataSO

```text
AnimalDataSO
  AnimalId
  DisplayName
  Species
  RequiredBuildingType
  ProductItemId
  ProductIntervalDays
  FeedItemIds
  BaseValue
  MaxHealthState
```

## 53. FertilizerDataSO

```text
FertilizerDataSO
  FertilizerId
  DisplayName
  Tier
  GrowthDaysReduction
  YieldMultiplier
  QualityBonus
  ValidCropTags
```

---

# PARTE N — MVP, V2 e FULL

## 54. MVP técnico atual

Manter foco em:

- mover;
- interagir;
- arar;
- molhar;
- plantar;
- crescer com água;
- colher;
- inventário;
- vender;
- salvar/carregar;
- árvores básicas;
- pesca básica;
- crafting/economy conforme já implementado/parcial;
- sem qualidade complexa;
- sem fertilizante;
- sem animais;
- sem clima;
- sem expansão real.

## 55. V2 FARM

Adicionar:

- morte de planta após 3 dias sem água;
- fertilizante simples/intermediário;
- qualidade inicial;
- expansão Tier 2;
- pasto pequeno;
- vacas;
- caixa de envio;
- chuva;
- mais peixes;
- mais árvores;
- primeira estufa ou preparação para estufa;
- companion job simples: Regador/Colhedor/Lenhador.

## 56. FULL FARM

Adicionar:

- expansão completa Tier 4;
- estufa funcional;
- múltiplos animais;
- qualidade avançada;
- fertilizante avançado/lunar;
- irrigação automática;
- storage avançado;
- workshops nível 3;
- companion management completo;
- eventos sazonais/lunares;
- decoração;
- integração profunda com quests e cidade.

---

# PARTE O — Specs futuras derivadas

Este refinamento deve gerar specs separadas:

```text
spec_farm_expansion_layout_zones.md
spec_farm_buildings_construction_upgrades.md
spec_farm_crop_quality_fertilizers_death.md
spec_farm_animals_pasture_products.md
spec_farm_storage_shipping_bin_economy.md
spec_farm_weather_rain_irrigation_automation.md
spec_farm_companion_jobs_automation.md
```

Cada spec precisa declarar:

- escopo;
- fora de escopo;
- dependências;
- arquivos permitidos;
- arquivos proibidos;
- eventos;
- dados;
- save;
- UI;
- validação Unity;
- impacto em specs implementadas;
- riscos e rollback.

---

# PARTE P — Decisões pendentes

- A Fonte existe desde o dia 1 ou é ativada por quest?
- Rega consome stamina desde V2 ou só FULL?
- Chuva entra antes ou depois de fertilizante?
- Qualidade usa metadado ou IDs separados?
- Fertilizante simples reduz tempo ou melhora qualidade?
- Animais podem morrer permanentemente ou entram em estado recuperável?
- Workshops puxam recursos do inventário, baús ou depósito global?
- Caixa de envio paga no mesmo dia ou na manhã seguinte?
- Companion pode gastar seeds/fertilizante automaticamente?
- Expansão é comprada na cidade ou por construção na fazenda?
- Estufa permite sementes noturnas fora de horário?
- Lago recarrega watering can ou apenas poço?
- Caverna pode afetar diretamente a fazenda por eventos?

## 57. Próxima ação recomendada

Depois deste pré-refinamento, a sequência mais segura é:

```text
1. Refinar layout e expansões da fazenda.
2. Refinar construções e custos.
3. Refinar crops/fertilizantes/qualidade.
4. Refinar animais.
5. Refinar storage/economia/caixa de envio.
6. Só depois gerar specs implementáveis.
```

A primeira spec implementável futura provavelmente deve ser:

```text
spec_farm_crop_quality_fertilizers_death.md
```

Mas apenas depois de decidir:

- qualidade por metadado vs ID;
- morte de planta;
- fertilizante;
- compatibilidade com inventory/save atual.
