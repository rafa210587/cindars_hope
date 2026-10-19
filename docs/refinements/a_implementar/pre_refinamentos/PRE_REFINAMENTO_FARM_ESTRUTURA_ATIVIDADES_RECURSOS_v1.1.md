# Cindar's Hope — Pré-refinamento FARM: Estrutura, Atividades, Construções e Recursos v1.1

> **Status:** pré-refinamento vivo para orientar specs futuras  
> **Origem:** `PRE_REFINAMENTO_FARM_ESTRUTURA_ATIVIDADES_RECURSOS_v1.0.md` + refinamento do Rafa sobre roadmap, mineração, referências de farm sims e Fruto Mana  
> **Destino:** `docs/refinements/a_implementar/pre_refinamentos/PRE_REFINAMENTO_FARM_ESTRUTURA_ATIVIDADES_RECURSOS_v1.1.md`  
> **Natureza:** documento de direção de produto/sistemas. Não é spec implementável ainda.  
> **Decisão desta versão:** remover linguagem limitante de “MVP/V2/FULL” e organizar a fazenda como **roadmap progressivo de implementação**.

---

## 1. Mudança de abordagem

A partir desta versão, a fazenda não será descrita como “MVP, V2 e FULL”.

O refinamento passa a usar **roadmap de implementação por ondas**, mantendo ambição completa do jogo sem restringir a visão.

A lógica passa a ser:

```text
Roadmap 0 — Base já existente / reconciliada
Roadmap 1 — Fazenda jogável essencial
Roadmap 2 — Fazenda produtiva e expansível
Roadmap 3 — Fazenda viva, social e automatizável
Roadmap 4 — Fazenda avançada, rara e conectada à lore
Roadmap 5 — Fazenda final, integrada ao endgame
```

Essas ondas não são fases rígidas. Elas servem para ordenar dependências, reduzir retrabalho e permitir quebrar o refinamento em specs futuras.

---

## 2. Princípio central da fazenda

A fazenda é a **base viva do jogador**.

Ela deve cumprir cinco papéis:

1. **Produção:** crops, árvores, animais, pesca, recursos leves e itens processados.
2. **Preparação:** comida, poções, fertilizantes, ferramentas, equipamentos, buffs e logística para caverna.
3. **Economia:** venda comum, encomendas, produtos de qualidade, recursos raros e bens processados.
4. **Social:** NPCs, companions, visitas, ajuda, eventos e reputação.
5. **Lore:** Fonte de Anya, Fruto Mana, ecos de Cindar e conexão com recursos mágicos de Vaalara.

Regra: a fazenda deve sempre gerar decisões, não apenas tarefas repetitivas.

---

## 3. Referências externas úteis sem copiar identidade

Cindar's Hope pode aprender com farm sims clássicos sem copiar assets, personagens, paleta, UI ou progressão exata.

Mecânicas úteis observadas em farm sims:

| Mecânica | Valor para Cindar's Hope | Adaptação recomendada |
|---|---|---|
| Rega diária e chuva | cria rotina e planejamento | manter rega, chuva molha área externa, irrigação reduz repetição |
| Solo arado/molhado/plantado | clareza visual | já alinhado ao estado atual de FarmPlot |
| Sprinklers/irrigação | automação gradual | usar canais, aspersores, runas hidráulicas ou engenharia de Carpintaria |
| Fertilizantes | escolha estratégica por ciclo | usar tiers: crescimento, yield, qualidade e lunar |
| Qualidade de crops | valor econômico e progressão | usar `ItemId + Quality` ou metadado equivalente |
| Animais com cuidado/afeto | rotina e vínculo | usar cuidado diário, qualidade do produto e companion Tratador |
| Processadores/makers | transformação de matéria-prima | usar workshops e máquinas: moinho, prensa, fermentador, tear, queijaria |
| Estufa | quebra de sazonalidade | manter como construção avançada |
| Árvores frutíferas | planejamento de longo prazo | incluir pomar e Fruto Mana como cultivo raro |
| Caixa de envio | conveniência | usar com preço base; lojas/encomendas pagam melhor |
| Festivais/concursos | cidade viva e metas | adaptar para feiras de colheita, animais, pesca e lua |
| Espantalhos/proteção | risco leve no campo | avaliar como proteção contra pragas/criaturas, sem excesso de punição |
| Silos/feno | animais e inverno | incluir armazenamento de ração/feno |
| Melhorias de ferramentas | progressão material | ferramentas melhores reduzem custo, aumentam área e eficiência |
| Mina/caverna como fonte mineral | loop farm → cave → upgrade | manter mineração principal na caverna |

Regra de adaptação: usar apenas a função sistêmica. Não copiar apresentação, nomes, layout visual, fórmula exata ou identidade.

---

# PARTE A — Estrutura e tamanho da fazenda

## 4. Modelo de expansão

A fazenda deve crescer por **níveis de propriedade**, não por “versões do jogo”.

Cada nível deve:

- desbloquear nova área;
- aumentar capacidade produtiva;
- liberar pelo menos uma construção relevante;
- criar uma decisão nova;
- ter custo em ouro + materiais;
- exigir algum progresso de cidade/caverna/fazenda.

## 5. Níveis de propriedade

| Nível | Nome de trabalho | Função principal | Conteúdo liberado |
|---:|---|---|---|
| 1 | Terreno Inicial | aprender rotina e sustentar primeiros recursos | casa, campo inicial, lago, bosque, Fonte, SellPoint, entrada da caverna |
| 2 | Fazenda Aberta | aumentar produção e organizar rotina | mais canteiros, baús, pasto pequeno, primeira expansão de workshops |
| 3 | Fazenda Produtiva | introduzir produção animal/processamento | curral/galinheiro, estufa inicial, storage melhor, primeiros processadores |
| 4 | Fazenda Especializada | automação e produção rara | irrigação avançada, múltiplos workshops, companion jobs robustos, pomar |
| 5 | Fazenda Plena | integração com endgame/lore | pedreira final, área arcana, Fruto Mana, upgrades da Fonte, produção avançada |

## 6. Tamanho sugerido por nível

| Nível | Tamanho aproximado | Observação |
|---:|---:|---|
| 1 | 40x32 tiles | já maior que uma tela, mas fácil de ler |
| 2 | 56x40 tiles | abre lateral ou sul para pasto/canteiros |
| 3 | 72x52 tiles | permite estufa, animais e workshops |
| 4 | 88x64 tiles | layout robusto com zonas especializadas |
| 5 | 104x72 tiles | espaço final para pedreira, pomar raro e área arcana |

O tamanho é diretriz. A implementação pode usar tilemaps e zonas desbloqueáveis sem necessariamente redesenhar todo o mapa a cada expansão.

---

# PARTE B — Zonas da fazenda

## 7. Zonas obrigatórias

| Zona | Função | Nível recomendado |
|---|---|---:|
| Casa | dormir, salvar, cozinhar, storage, calendário | 1 |
| Campo inicial | plantio, rega, colheita | 1 |
| Fonte de Anya | lore, respec, ressurreição de companions | 1, dormente ou parcialmente ativa |
| Lago | pesca, recarga de água | 1 |
| Bosque | madeira, árvores, pomar futuro | 1 |
| SellPoint / Caixa de envio | venda rápida | 1 |
| Entrada da caverna | preparação para exploração | 1 |
| Área de workshops | crafting por construção | 1–4 |
| Depósito | baús e storage por categoria | 1–3 |
| Pasto/curral | animais grandes | 2 |
| Galinheiro | animais pequenos | 3 |
| Estufa | plantio fora de estação | 3 |
| Pomar | árvores frutíferas e Mana | 4–5 |
| Área de companions | moradia, job board, gerenciamento | 3–4 |
| Área arcana da Fonte | upgrades ligados a Anya/Mana | 5 |
| Pedreira da fazenda | recurso mineral tardio e limitado | 5 |

## 8. Decisão sobre pedreira/mineração

A pedreira da fazenda **não é fonte principal de mineração**.

A maioria dos minérios deve vir da caverna e dos níveis dela.

A pedreira da fazenda é um recurso tardio, liberado apenas no último nível da fazenda.

### Função da pedreira final

A pedreira deve servir para:

- gerar pequena renda passiva de pedra/minério básico;
- oferecer conveniência no endgame;
- alimentar construções finais sem obrigar runs longas para materiais simples;
- gerar raramente fragmentos específicos para crafting;
- nunca substituir exploração de caverna.

### Regra

```text
Mineração principal = caverna.
Pedreira da fazenda = conveniência tardia, limitada e controlada.
```

### Regras sugeridas da pedreira

- desbloqueada apenas no Nível 5 da fazenda;
- custo alto de construção/liberação;
- respawn lento;
- yield diário/semanal limitado;
- materiais avançados continuam exclusivos da caverna;
- pode receber upgrade final para quebrar rochas automaticamente com companion Minerador;
- pode servir de tutorial tardio para mineração automatizada, não para progressão principal.

---

# PARTE C — Roadmap de implementação

## 9. Roadmap 0 — Base já existente / reconciliada

Este bloco representa o que já aparece como implementado/parcial em specs ativas do repo e deve ser preservado.

Inclui:

- estados de solo/plot;
- arar;
- molhar;
- plantar;
- colher;
- crescimento condicionado por água;
- save de estado/seed/progresso/água/regrow;
- menu contextual agrícola;
- árvores/pesca/world activities em algum grau;
- inventário por slots/capacidade já evoluído em specs recentes;
- economy/shop/crafting em código/parcial conforme registries.

Direção: não reimplementar do zero. Refinar, ampliar e formalizar.

## 10. Roadmap 1 — Fazenda jogável essencial

Objetivo: deixar a fazenda com rotina diária completa e coerente.

Implementações:

- layout canônico Nível 1;
- casa com dormir/salvar;
- campo inicial organizado;
- lago funcional;
- bosque com árvores;
- SellPoint/caixa de venda;
- Fonte visível;
- entrada da caverna;
- ferramentas básicas;
- feedback visual de solo seco/molhado/plantado/pronto;
- morte de planta após 3 dias sem água;
- limpeza de planta morta;
- chuva molhando áreas externas;
- UI agrícola mais clara.

## 11. Roadmap 2 — Fazenda produtiva e expansível

Objetivo: criar crescimento de propriedade.

Implementações:

- expansão Nível 2;
- mais canteiros;
- construção de pasto pequeno;
- primeiros animais;
- baús por categoria;
- caixa de envio com pagamento na manhã seguinte;
- encomendas simples de NPCs;
- fertilizante simples;
- qualidade inicial;
- melhoria de ferramentas;
- início de produtos processados simples;
- primeira automação leve por companion.

## 12. Roadmap 3 — Fazenda viva, social e processadora

Objetivo: conectar fazenda, cidade e companions.

Implementações:

- expansão Nível 3;
- galinheiro/curral;
- estufa inicial;
- área de workshops mais robusta;
- processadores/makers;
- NPCs visitando a fazenda;
- companions com jobs de fazenda;
- animais com cuidado, alimentação e produto de qualidade;
- eventos sazonais simples;
- festivals/concursos de produção;
- pedidos de NPCs por qualidade/quantidade;
- chuva, season e calendário mais relevantes.

## 13. Roadmap 4 — Fazenda especializada e automatizável

Objetivo: reduzir repetição sem remover decisão.

Implementações:

- expansão Nível 4;
- irrigação avançada;
- aspersores/canais/runas hidráulicas;
- fertilizante intermediário/avançado;
- pomar;
- mais árvores frutíferas;
- storage central;
- workshops nível alto;
- companion job board;
- produção artesanal avançada;
- plantio por plano/blueprint de campo;
- rotinas automatizadas configuráveis;
- crops mágicas/lunares.

## 14. Roadmap 5 — Fazenda final e endgame

Objetivo: conectar fazenda ao endgame de Vaalara.

Implementações:

- expansão Nível 5;
- pedreira final;
- área arcana da Fonte;
- cultivo do Fruto Mana;
- fertilizante lunar;
- crops raras ligadas às luas;
- upgrades finais da Fonte de Anya;
- produção de poções e alimentos endgame;
- eventos raros de Alihana/Senya/Nyx;
- integração com nível 100/101 da caverna;
- encomendas lendárias;
- construção/decor final.

---

# PARTE D — Construções

## 15. Construções essenciais

| Construção | Função |
|---|---|
| Casa | dormir, salvar, cozinhar, storage, calendário |
| Fonte de Anya | respec, companions, lore, eventos |
| SellPoint/Caixa de envio | venda rápida |
| Entrada da caverna | preparação e acesso |
| Baú inicial | storage básico |

## 16. Construções produtivas

| Construção | Função | Roadmap |
|---|---|---:|
| Pasto | animais grandes | 2 |
| Galinheiro | aves e ovos | 3 |
| Estufa | crops fora de estação | 3 |
| Pomar | árvores frutíferas | 4 |
| Área arcana | Mana e crops raras | 5 |
| Pedreira final | mineral tardio limitado | 5 |

## 17. Workshops e processadores

Workshops principais:

- Forja;
- Alquimia;
- Carpintaria;
- Costura.

Processadores/makers possíveis:

| Processador | Entrada | Saída |
|---|---|---|
| Moinho | trigo | farinha |
| Prensa | frutas/sementes | óleo/suco |
| Fermentador | frutas/crops | bebida/essência |
| Queijaria | leite | queijo |
| Tear | lã/fibra | tecido |
| Secador | ervas/peixes | ingrediente concentrado |
| Destilador arcano | Mana/reagentes | essência mágica |

Regra: processadores devem ser derivados de workshops, não objetos soltos sem sistema.

---

# PARTE E — Plantio e solo

## 18. Estados de solo

Estados base:

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

Estados avançados opcionais:

```text
Fertilized
Infested
Frozen
Protected
LunarCharged
ArcaneSoil
```

## 19. Rega

Regras finais desejadas:

```text
Planta molhada cresce.
Planta seca não cresce.
Planta seca acumula DaysWithoutWater.
Com 3 dias sem água, vira Dead.
Chuva molha áreas externas.
Estufa pode usar irrigação própria.
Irrigação automatizada entra por construção/upgrade.
```

## 20. Ferramentas agrícolas

| Ferramenta | Função | Evolução |
|---|---|---|
| Hoe | arar solo | área maior por upgrade |
| Watering Can | molhar solo | mais carga/área por upgrade |
| Axe | cortar árvores | mais dano/yield |
| Fishing Rod | pescar | peixes raros/timing melhor |
| Sickle | limpar planta morta/fibra | área maior |
| Build Tool | mover/construir | modo edição |
| Fertilizer Applicator | aplicar fertilizante em área | avançado |

## 21. Clima e seasons

Clima útil:

- sol;
- chuva;
- tempestade;
- neblina;
- geada;
- evento lunar.

Seasons:

- determinam crops válidas;
- alteram pesca;
- alteram pedidos da cidade;
- alteram estética da fazenda;
- podem afetar animais;
- inverno bloqueia solo externo comum;
- estufa contorna sazonalidade.

---

# PARTE F — Qualidade, fertilizante e economia agrícola

## 22. Qualidade

Qualidades recomendadas:

```text
Normal
Boa
Excelente
Rara
Lunar
```

Critérios que influenciam qualidade:

- nível/habilidade agrícola;
- fertilizante;
- rega perfeita;
- estação correta;
- lua ativa;
- estufa;
- companion especializado;
- Fruto Mana/crops mágicas;
- eventos sazonais;
- solo arcano.

## 23. Representação técnica recomendada

Preferência:

```text
ItemId + Quality
```

Exemplo:

```text
item_crop_wheat + Normal
item_crop_wheat + Excelente
item_fruit_mana + Lunar
```

Motivo: evita criar IDs separados para cada qualidade e prepara o inventário para crops, produtos animais e processados de qualidade.

## 24. Fertilizantes

| Tier | Tipo | Função |
|---:|---|---|
| 1 | Simples | crescimento ou qualidade leve |
| 2 | Rico | yield ou qualidade média |
| 3 | Refinado | qualidade alta |
| 4 | Arcano/Lunar | crops mágicas, Mana, eventos de lua |

Fontes:

- animais;
- compostagem;
- alquimia;
- caverna;
- NPCs;
- quests;
- Fonte/área arcana.

Regras:

- um fertilizante por plot/ciclo;
- se a planta morrer, o fertilizante é perdido;
- fertilizante comum não deve produzir Mana;
- fertilizante arcano/lunar deve exigir materiais raros;
- fertilizante avançado não substitui cuidado diário.

---

# PARTE G — Fruto Mana

## 25. Decisão de lore/sistema

Existe em Vaalara um fruto chamado **Mana**.

Características:

- raro;
- difícil de fazer florescer;
- lembra um pêssego roxo;
- possui ligação natural com energia mágica;
- quando consumido por humanos, concede temporariamente status mais fortes;
- pode ser usado para curar ferimentos no contexto de gameplay;
- deve ser recurso avançado de fazenda/lore, não crop comum.

## 26. Nome e cuidado técnico

Há um possível conflito de nomenclatura entre:

```text
Fruto Mana = item/crop/lore
Pontos de Magia = recurso de personagem para habilidades mágicas
```

Recomendação:

```text
DisplayName do item: Mana
Nome documental quando precisar evitar ambiguidade: Fruto Mana
ID: item_fruit_mana
Recurso mágico do personagem: MagicPoints ou MP
DisplayName do recurso: Pontos de Magia
ID do stat: stat_magic_points
```

O sistema de Pontos de Magia deve ser refinado em documento de progressão/combate/magia, não neste refinamento de fazenda.

## 27. Como cultivar Mana

Mana deve ser mais próximo de uma árvore/crop rara de pomar arcano do que de uma semente comum.

Proposta:

```text
Mana Sapling / Muda de Mana
  -> plantada em Pomar Arcano ou Estufa Arcana
  -> exige solo especial
  -> exige água ou energia da Fonte
  -> exige fertilizante lunar/arcano
  -> só floresce sob condições específicas de lua
  -> demora muitos dias
  -> pode falhar em florescer sem matar a planta
```

## 28. Condições de florescimento

Condições candidatas:

- Fazenda nível 5;
- Fonte de Anya despertada ou fortalecida;
- Pomar Arcano construído;
- fertilizante lunar aplicado;
- irrigação com Água da Fonte ou Água Lunar;
- estação específica;
- Alihana ativa para florescimento;
- Nyx ativa para mutação rara;
- skill agrícola/mágica suficiente;
- companion com afinidade alta ajudando.

Não precisa exigir todas. A spec futura deve escolher combinação balanceada.

## 29. Usos do Fruto Mana

Usos possíveis:

| Uso | Efeito |
|---|---|
| Consumo direto | buff temporário em atributos |
| Poção | cura forte ou recuperação de MP |
| Receita culinária | buff misto de HP/stamina/status |
| Alquimia | ingrediente de elixir raro |
| Fonte de Anya | upgrade/purificação |
| Quest | item solicitado por NPCs específicos |
| Venda | alto valor, mas talvez melhor usar do que vender |
| Endgame | requisito para enfrentar áreas profundas |

## 30. Buffs possíveis

Fruto Mana pode conceder temporariamente:

- Força;
- Constituição;
- Destreza;
- Inteligência;
- Vontade;
- Carisma;
- regeneração leve de HP;
- recuperação de Pontos de Magia;
- resistência a status;
- bônus de cura recebida.

Regras:

- efeito temporário;
- não deve virar fonte infinita fácil de poder;
- cooldown ou saturação pode existir;
- uso em combate deve exigir escolha;
- versões de qualidade maior podem aumentar duração/intensidade.

## 31. Qualidades do Fruto Mana

| Qualidade | Efeito sugerido |
|---|---|
| Normal | buff baixo e cura leve |
| Boa | buff moderado |
| Excelente | buff forte |
| Rara | buff duplo ou cura melhor |
| Lunar | efeito mágico especial ou recuperação de MP |

Fruto Mana Lunar deve ser raríssimo.

---

# PARTE H — Animais e produtos

## 32. Animais

Animais devem produzir recursos, mas também criar rotina.

Animais planejados:

| Animal | Produto | Uso |
|---|---|---|
| Vaca | leite, fertilizante base | comida, queijo, alquimia, fertilizante |
| Galinha | ovos | comida, maionese, quests |
| Ovelha | lã | costura, tecido, roupas |
| Animal fantástico pequeno | recurso raro | late game |

## 33. Cuidado animal

Regras:

- alimentar diariamente;
- animal alimentado produz;
- animal sem comida para de produzir;
- cuidado/afeto melhora qualidade;
- companion Tratador pode automatizar parte da rotina;
- morte deve ser tratada com tom simples e recuperável quando possível.

## 34. Produtos processados

Produtos animais podem virar:

- queijo;
- manteiga;
- maionese;
- tecido;
- couro/alternativa de couro, se aprovado;
- fertilizante;
- ingredientes alquímicos.

---

# PARTE I — Automação, companions e rotina

## 35. Jobs de fazenda

| Job | Função |
|---|---|
| Plantador | planta seeds conforme plano |
| Regador | molha plots |
| Colhedor | colhe crops prontos |
| Lenhador | corta árvores selecionadas |
| Pescador | pesca no lago |
| Tratador | alimenta/coleta animais |
| Artesão | opera processadores/workshops |
| Organizador | move itens para storage |
| Minerador | usa pedreira final limitada |

## 36. Regra de automação

Automação deve exigir configuração.

O jogador define:

```text
Quem faz
Onde faz
Quando faz
Pode gastar quais recursos
Onde deposita output
Prioridade
Limite diário
```

Automação não deve:

- gastar Fruto Mana sem autorização;
- gastar fertilizante raro sem autorização;
- vender item raro automaticamente;
- desbloquear progressão sem decisão do jogador;
- substituir exploração da caverna.

---

# PARTE J — Economia e processamento

## 37. Venda

Canais:

| Canal | Característica |
|---|---|
| Caixa de envio | conveniência, pagamento no dia seguinte |
| Loja da cidade | preço melhor e reputação |
| Encomenda | preço alto e prazo |
| Festival/Concurso | prêmio e reputação |
| Craft/processamento | maior margem com tempo |

## 38. Produtos artesanais

A fazenda deve incentivar processar antes de vender.

Exemplos:

```text
Trigo -> Farinha -> Pão/Receita
Leite -> Queijo
Ovos -> Maionese
Fruta -> Suco/Fermentado
Erva -> Extrato/Poção
Mana -> Elixir/Essência de Mana
```

Regra: processamento aumenta valor, mas consome tempo, construção e planejamento.

---

# PARTE K — Recursos e mineração

## 39. Filosofia de mineração

A mineração deve ser principalmente da caverna.

A fazenda pode fornecer:

- pedra solta;
- madeira;
- argila;
- poucos recursos básicos;
- pedreira final limitada.

A caverna fornece:

- cobre;
- ferro;
- prata;
- cristais;
- minério arcano;
- reagentes raros;
- Pedra do Meteoro Preta;
- materiais de boss;
- recursos para upgrades finais.

## 40. Pedreira final

A pedreira final deve:

- aparecer apenas no nível 5 da fazenda;
- exigir caverna avançada para desbloquear;
- produzir materiais básicos e ocasionais;
- não gerar boss materials;
- não gerar recursos principais de progressão;
- servir como conveniência, não substituto.

---

# PARTE L — Save e dados futuros

## 41. FarmSaveData

```text
FarmSaveData
  FarmLevel
  UnlockedZones[]
  Plots[]
  Trees[]
  FruitTrees[]
  Buildings[]
  Animals[]
  ShippingBin
  CompanionJobs[]
  FountainState
  IrrigationNetwork
  QuarryState
  ManaCultivationState
```

## 42. PlotSaveData

```text
PlotId
Position
State
SeedId
GrowthProgressDays
DaysWithoutWater
FertilizerId
QualitySeed
RegrowRemainingDays
LastWateredDay
IsProtected
SpecialSoilState
```

## 43. ManaCultivationSaveData

```text
ManaPlantInstanceId
GrowthState
BloomAttempts
LastBloomDay
LunarCharge
FertilizerId
SourceWaterState
QualityBias
IsDormant
```

## 44. AnimalSaveData

```text
AnimalInstanceId
AnimalDataId
Name
HomeBuildingId
FedToday
DaysWithoutFood
HealthState
CareScore
ProductReady
ProductQualityBias
```

## 45. BuildingSaveData

```text
BuildingInstanceId
BuildingId
Position
Level
State
ConstructionRemainingDays
AssignedCompanionId
InternalInventoryRef
```

---

# PARTE M — Roadmap de specs futuras

## 46. Ordem recomendada de specs derivadas

1. `spec_farm_layout_expansion_zones.md`
2. `spec_farm_buildings_construction_workshops_storage.md`
3. `spec_farm_crop_death_quality_fertilizers.md`
4. `spec_farm_weather_rain_irrigation_automation.md`
5. `spec_farm_shipping_bin_orders_processing.md`
6. `spec_farm_animals_pasture_products_care.md`
7. `spec_farm_companion_jobs_automation.md`
8. `spec_farm_orchard_mana_fruit_arcane_soil.md`
9. `spec_farm_final_quarry_late_game_resources.md`

## 47. Relação com outros refinamentos

Este documento depende ou alimenta:

| Documento futuro | Relação |
|---|---|
| Cidade/NPCs | encomendas, compras, animais, visitas, reputação |
| Caverna | minerais, reagentes, Pedra Preta, desbloqueio de pedreira |
| Progressão/Magia | Pontos de Magia, buffs, skills agrícolas/mágicas |
| Companions | jobs, afinidade, automação |
| Crafting | processadores, fertilizantes, produtos |
| UI/UX | menus, hotbar, storage, shipping, layout |
| Lore | Fonte, Anya, Fruto Mana, Cindar |

---

# PARTE N — Decisões pendentes

## 48. Decisões de produto

- Qual é o nome oficial do nível máximo da fazenda?
- A Fonte existe ativa desde o começo ou começa dormente?
- Fazenda nível 5 exige qual marco da caverna?
- Fruto Mana floresce com Alihana, Nyx ou condição própria?
- Mana é árvore, arbusto ou planta arcana única?
- Fruto Mana pode ser vendido livremente ou deve ser protegido por lore/raridade?
- Animais podem morrer permanentemente ou entram em estado recuperável?
- Caixa de envio paga na manhã seguinte ou instantâneo?
- Processadores puxam itens do inventário, baús ou depósito central?
- Companion pode usar fertilizante raro automaticamente?
- Chuva pode falhar em eventos de seca?
- Pedreira final usa companion Minerador?

## 49. Decisões técnicas

- Qualidade será metadado no stack ou instância individual?
- `ItemId + Quality` é suficiente para todos os produtos?
- Fruto Mana precisa de item instanciado único?
- Save de irrigação será por plot ou rede de tiles?
- Construções serão posicionadas livremente ou em pads fixos?
- Expansão usa tilemap revelado ou cenas/áreas separadas?
- Pedreira final usa resource nodes persistentes ou geração diária simples?

---

## 50. Próximo passo recomendado

O próximo refinamento de fazenda deve ser:

```text
PRE_REFINAMENTO_FARM_LAYOUT_EXPANSAO_ZONAS_v1.0.md
```

Foco:

- desenhar níveis da fazenda;
- definir zonas por nível;
- definir unlocks;
- definir papel da Fonte;
- definir onde ficam campos, lago, casa, caverna, pasto, estufa, pomar, workshops e pedreira final.

Depois disso:

```text
PRE_REFINAMENTO_FARM_CROPS_QUALIDADE_FERTILIZANTES_MANA_v1.0.md
```

Foco:

- crops comuns;
- crops mágicas;
- qualidade;
- fertilizante;
- Fruto Mana;
- solo arcano;
- relação com Fonte/luas.
