# Cindar's Hope — Farm Design Direction v1.0

> **Status:** direção ativa de gameplay/sistema  
> **Local:** `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.0.md`  
> **Função:** grande descrição de como a fazenda deve funcionar no jogo.  
> **Não é spec implementável.** Specs futuras devem ser quebradas em `docs/specs/a_implementar/`.

---

## 1. Papel da fazenda no jogo

A fazenda é a **base viva do jogador**.

Ela conecta rotina, economia, cidade, companions, crafting, caverna e lore.

A fazenda deve cumprir cinco papéis:

1. **Produção:** crops, árvores, animais, pesca, recursos leves e itens processados.
2. **Preparação:** comida, poções, fertilizantes, ferramentas, equipamentos, buffs e logística para caverna.
3. **Economia:** venda comum, encomendas, produtos de qualidade, recursos raros e bens processados.
4. **Social:** NPCs, companions, visitas, ajuda, eventos e reputação.
5. **Lore:** Fonte de Anya, Fruto Mana, ecos de Cindar e conexão com recursos mágicos de Vaalara.

Regra: a fazenda deve sempre gerar decisões, não apenas tarefas repetitivas.

---

## 2. Roadmap conceitual de implementação

Este documento não usa separação `MVP/V2/FULL`.

A evolução da fazenda deve seguir um roadmap de implementação por ondas funcionais:

```text
Roadmap 0 — Base já existente / reconciliada
Roadmap 1 — Fazenda jogável essencial
Roadmap 2 — Fazenda produtiva e expansível
Roadmap 3 — Fazenda viva, social e automatizável
Roadmap 4 — Fazenda avançada, rara e conectada à lore
Roadmap 5 — Fazenda final, integrada ao endgame
```

Essas ondas não são fases rígidas. Elas ordenam dependências, reduzem retrabalho e ajudam a quebrar specs futuras.

---

## 3. Roadmap 0 — Base já existente / reconciliada

Representa o que já aparece como implementado/parcial no repo e deve ser preservado:

- estados de solo/plot;
- arar;
- molhar;
- plantar;
- colher;
- crescimento condicionado por água;
- save de estado/seed/progresso/água/regrow;
- menu contextual agrícola;
- árvores/pesca/world activities em algum grau;
- inventário por slots/capacidade evoluído em specs recentes;
- economy/shop/crafting em código/parcial conforme registries.

Direção: não reimplementar do zero. Refinar, ampliar e formalizar.

---

## 4. Roadmap 1 — Fazenda jogável essencial

Objetivo: deixar a fazenda com rotina diária completa e coerente.

Implementações desejadas:

- layout canônico do primeiro nível da fazenda;
- casa com dormir/salvar;
- campo inicial organizado;
- lago funcional;
- bosque com árvores;
- SellPoint/caixa de venda;
- Fonte de Anya visível;
- entrada da caverna;
- ferramentas básicas;
- feedback visual de solo seco/molhado/plantado/pronto;
- morte de planta após 3 dias sem água;
- limpeza de planta morta;
- chuva molhando áreas externas;
- UI agrícola mais clara.

---

## 5. Roadmap 2 — Fazenda produtiva e expansível

Objetivo: criar crescimento real de propriedade.

Implementações desejadas:

- expansão de propriedade;
- mais canteiros;
- construção de pasto pequeno;
- primeiros animais;
- baús por categoria;
- caixa de envio com pagamento na manhã seguinte;
- encomendas simples de NPCs;
- fertilizante simples;
- qualidade inicial;
- melhoria de ferramentas;
- produtos processados simples;
- primeira automação leve por companion.

---

## 6. Roadmap 3 — Fazenda viva, social e processadora

Objetivo: conectar fazenda, cidade e companions.

Implementações desejadas:

- nova expansão de propriedade;
- galinheiro/curral;
- estufa inicial;
- área de workshops mais robusta;
- processadores/makers;
- NPCs visitando a fazenda;
- companions com jobs de fazenda;
- animais com cuidado, alimentação e produto de qualidade;
- eventos sazonais simples;
- festivais/concursos de produção;
- pedidos de NPCs por qualidade/quantidade;
- chuva, season e calendário mais relevantes.

---

## 7. Roadmap 4 — Fazenda especializada e automatizável

Objetivo: reduzir repetição sem remover decisão.

Implementações desejadas:

- expansão para zonas especializadas;
- irrigação avançada;
- aspersores, canais, runas hidráulicas ou mecanismos equivalentes;
- fertilizante intermediário/avançado;
- pomar;
- árvores frutíferas;
- storage central;
- workshops em nível alto;
- companion job board;
- produção artesanal avançada;
- plantio por plano/blueprint de campo;
- rotinas automatizadas configuráveis;
- crops mágicas/lunares.

---

## 8. Roadmap 5 — Fazenda final e endgame

Objetivo: conectar fazenda ao endgame de Vaalara.

Implementações desejadas:

- expansão máxima da fazenda;
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

## 9. Níveis de propriedade

A fazenda deve crescer por **níveis de propriedade**.

Cada nível deve:

- desbloquear nova área;
- aumentar capacidade produtiva;
- liberar pelo menos uma construção relevante;
- criar uma decisão nova;
- ter custo em ouro + materiais;
- exigir progresso de cidade, caverna, fazenda ou lore.

| Nível | Nome de trabalho | Função principal | Conteúdo liberado |
|---:|---|---|---|
| 1 | Terreno Inicial | aprender rotina e sustentar primeiros recursos | casa, campo inicial, lago, bosque, Fonte, SellPoint, entrada da caverna |
| 2 | Fazenda Aberta | aumentar produção e organizar rotina | mais canteiros, baús, pasto pequeno, primeira expansão de workshops |
| 3 | Fazenda Produtiva | introduzir produção animal/processamento | curral/galinheiro, estufa inicial, storage melhor, primeiros processadores |
| 4 | Fazenda Especializada | automação e produção rara | irrigação avançada, múltiplos workshops, companion jobs robustos, pomar |
| 5 | Fazenda Plena | integração com endgame/lore | pedreira final, área arcana, Fruto Mana, upgrades da Fonte, produção avançada |

---

## 10. Tamanho sugerido por nível

| Nível | Tamanho aproximado | Observação |
|---:|---:|---|
| 1 | 40x32 tiles | maior que uma tela, mas fácil de ler |
| 2 | 56x40 tiles | abre lateral ou sul para pasto/canteiros |
| 3 | 72x52 tiles | permite estufa, animais e workshops |
| 4 | 88x64 tiles | layout robusto com zonas especializadas |
| 5 | 104x72 tiles | espaço final para pedreira, pomar raro e área arcana |

Esses tamanhos são diretrizes, não contrato técnico. A implementação pode usar tilemaps e zonas desbloqueáveis sem redesenhar toda a cena a cada expansão.

---

## 11. Zonas da fazenda

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
| Pomar | árvores frutíferas e Fruto Mana | 4–5 |
| Área de companions | moradia, job board, gerenciamento | 3–4 |
| Área arcana da Fonte | upgrades ligados a Anya/Mana | 5 |
| Pedreira da fazenda | recurso mineral tardio e limitado | 5 |

---

## 12. Pedreira e mineração

A pedreira da fazenda **não é fonte principal de mineração**.

A mineração principal deve vir da caverna e dos níveis dela.

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

### Regras sugeridas

- liberada apenas no nível 5 da fazenda;
- custo alto de construção/liberação;
- exige progresso avançado na caverna;
- respawn lento;
- yield diário/semanal limitado;
- materiais avançados continuam exclusivos da caverna;
- pode receber upgrade final para uso por companion Minerador;
- não gera materiais de boss nem recursos principais de progressão.

---

## 13. Referências externas úteis sem copiar identidade

Cindar's Hope pode aprender com farm sims clássicos sem copiar assets, personagens, paleta, UI ou progressão exata.

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

## 14. Construções essenciais

| Construção | Função |
|---|---|
| Casa | dormir, salvar, cozinhar, storage, calendário |
| Fonte de Anya | respec, companions, lore, eventos |
| SellPoint/Caixa de envio | venda rápida |
| Entrada da caverna | preparação e acesso |
| Baú inicial | storage básico |

## 15. Construções produtivas

| Construção | Função | Roadmap |
|---|---|---:|
| Pasto | animais grandes | 2 |
| Galinheiro | aves e ovos | 3 |
| Estufa | crops fora de estação | 3 |
| Pomar | árvores frutíferas | 4 |
| Área arcana | Fruto Mana e crops raras | 5 |
| Pedreira final | mineral tardio limitado | 5 |

## 16. Workshops e processadores

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

## 17. Plantio e solo

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

---

## 18. Ferramentas agrícolas

| Ferramenta | Função | Evolução |
|---|---|---|
| Hoe | arar solo | área maior por upgrade |
| Watering Can | molhar solo | mais carga/área por upgrade |
| Axe | cortar árvores | mais dano/yield |
| Fishing Rod | pescar | peixes raros/timing melhor |
| Sickle | limpar planta morta/fibra | área maior |
| Build Tool | mover/construir | modo edição |
| Fertilizer Applicator | aplicar fertilizante em área | avançado |

---

## 19. Clima, seasons e luas

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

Luas:

- Alihana pode favorecer crops noturnas, Fruto Mana e eventos de esperança/cura;
- Senya pode intensificar crops caóticas, reagentes mágicos e variações instáveis;
- Nyx pode favorecer crops sombrias, segredos e mutações raras.

---

## 20. Qualidade

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

Representação técnica preferencial:

```text
ItemId + Quality
```

Exemplo:

```text
item_crop_wheat + Normal
item_crop_wheat + Excelente
item_fruit_mana + Lunar
```

---

## 21. Fertilizantes

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

## 22. Fruto Mana

Existe em Vaalara um fruto chamado **Mana**.

Características:

- raro;
- difícil de fazer florescer;
- lembra um pêssego roxo;
- possui ligação natural com energia mágica;
- quando consumido por humanos, concede temporariamente status mais fortes;
- pode ser usado para curar ferimentos no contexto de gameplay;
- deve ser recurso avançado de fazenda/lore, não crop comum.

## 23. Nomenclatura: Mana vs Pontos de Magia

Há possível conflito entre:

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

O sistema de Pontos de Magia deve ser refinado em documento de progressão/combate/magia, não neste documento de fazenda.

## 24. Cultivo do Fruto Mana

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

Condições candidatas:

- fazenda em nível 5;
- Fonte de Anya despertada ou fortalecida;
- Pomar Arcano construído;
- fertilizante lunar aplicado;
- irrigação com Água da Fonte ou Água Lunar;
- estação específica;
- Alihana ativa para florescimento;
- Nyx ativa para mutação rara;
- skill agrícola/mágica suficiente;
- companion com afinidade alta ajudando.

## 25. Usos do Fruto Mana

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

Fruto Mana Lunar deve ser raríssimo.

---

## 26. Animais

Animais planejados:

| Animal | Produto | Uso |
|---|---|---|
| Vaca | leite, fertilizante base | comida, queijo, alquimia, fertilizante |
| Galinha | ovos | comida, maionese, quests |
| Ovelha | lã | costura, tecido, roupas |
| Animal fantástico pequeno | recurso raro | late game |

Regras:

- alimentar diariamente;
- animal alimentado produz;
- animal sem comida para de produzir;
- cuidado/afeto melhora qualidade;
- companion Tratador pode automatizar parte da rotina;
- morte deve ser tratada com tom simples e recuperável quando possível.

---

## 27. Companion jobs na fazenda

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

Automação deve exigir configuração:

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

## 28. Economia agrícola

Canais de venda:

| Canal | Característica |
|---|---|
| Caixa de envio | conveniência, pagamento no dia seguinte |
| Loja da cidade | preço melhor e reputação |
| Encomenda | preço alto e prazo |
| Festival/Concurso | prêmio e reputação |
| Craft/processamento | maior margem com tempo |

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

## 29. Save e dados futuros

### FarmSaveData

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

### PlotSaveData

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

### ManaCultivationSaveData

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

### AnimalSaveData

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

### BuildingSaveData

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

Regras:

- salvar IDs e tipos simples;
- não salvar ScriptableObject;
- não salvar GameObject, Transform ou MonoBehaviour;
- migrations precisam de defaults seguros;
- se um ID sumir, logar erro claro e falhar de forma segura.

---

## 30. Specs futuras derivadas

Ordem recomendada:

1. `spec_farm_layout_expansion_zones.md`
2. `spec_farm_buildings_construction_workshops_storage.md`
3. `spec_farm_crop_death_quality_fertilizers.md`
4. `spec_farm_weather_rain_irrigation_automation.md`
5. `spec_farm_shipping_bin_orders_processing.md`
6. `spec_farm_animals_pasture_products_care.md`
7. `spec_farm_companion_jobs_automation.md`
8. `spec_farm_orchard_mana_fruit_arcane_soil.md`
9. `spec_farm_final_quarry_late_game_resources.md`

Cada spec futura deve declarar:

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

## 31. Relação com outros design directions

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

## 32. Decisões pendentes

### Produto

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

### Técnicas

- Qualidade será metadado no stack ou instância individual?
- `ItemId + Quality` é suficiente para todos os produtos?
- Fruto Mana precisa de item instanciado único?
- Save de irrigação será por plot ou rede de tiles?
- Construções serão posicionadas livremente ou em pads fixos?
- Expansão usa tilemap revelado ou cenas/áreas separadas?
- Pedreira final usa resource nodes persistentes ou geração diária simples?

---

## 33. Próximos documentos recomendados

A partir deste design direction, os próximos documentos de design/refinamento devem ser:

```text
docs/design/gameplay/farm/FARM_LAYOUT_EXPANSION_ZONES_DIRECTION_v1.0.md
docs/design/gameplay/farm/FARM_CROPS_QUALITY_FERTILIZERS_MANA_DIRECTION_v1.0.md
docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.0.md
docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION_v1.0.md
docs/design/gameplay/combat_magic_progression/COMBAT_MAGIC_PROGRESSION_DESIGN_DIRECTION_v1.0.md
```

Depois de consolidados, cada um deve alimentar specs menores em `docs/specs/a_implementar/`.
