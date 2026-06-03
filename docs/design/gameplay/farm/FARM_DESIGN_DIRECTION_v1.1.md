# Cindar's Hope — Farm Design Direction v1.1

> **Status:** direção ativa de gameplay/sistema  
> **Local:** `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.1.md`  
> **Substitui como direção ativa:** `FARM_DESIGN_DIRECTION_v1.0.md`  
> **Função:** grande descrição de como a fazenda deve funcionar no jogo.  
> **Não é spec implementável.** Specs futuras devem ser quebradas em `docs/specs/a_implementar/`.

---

## 0. Alterações da v1.1

Esta versão adiciona três decisões estruturais:

1. **Status de Cansaço** como sistema transversal ligado a tempo, stamina, fome, sono e rotina diária.
2. **Liberdade de layout da fazenda**, permitindo arar e preparar quase todo terreno válido, além de posicionar construções livremente dentro de regras claras.
3. **Pets**, como cachorro e gato, com alimentação, área de descanso, vínculo, buffs e possível participação do cachorro em combate.

---

## 1. Papel da fazenda no jogo

A fazenda é a **base viva do jogador**.

Ela conecta rotina, economia, cidade, companions, crafting, caverna, pets, progressão física/mágica e lore.

A fazenda deve cumprir seis papéis:

1. **Produção:** crops, árvores, animais, pesca, recursos leves e itens processados.
2. **Preparação:** comida, poções, fertilizantes, ferramentas, equipamentos, buffs e logística para caverna.
3. **Economia:** venda comum, encomendas, produtos de qualidade, recursos raros e bens processados.
4. **Social:** NPCs, companions, visitas, ajuda, eventos e reputação.
5. **Rotina corporal:** fome, stamina, cansaço, sono, cuidado animal e cuidado dos pets.
6. **Lore:** Fonte de Anya, Fruto Mana, ecos de Cindar e conexão com recursos mágicos de Vaalara.

Regra: a fazenda deve sempre gerar decisões, não apenas tarefas repetitivas.

---

# PARTE A — Roadmap conceitual

## 2. Roadmap de implementação

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

## 4. Roadmap 1 — Fazenda jogável essencial

Objetivo: deixar a fazenda com rotina diária completa e coerente.

Implementações desejadas:

- layout canônico do primeiro nível da fazenda;
- casa com dormir/salvar;
- campo inicial organizado;
- grande área arável e customizável;
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
- status de Cansaço;
- cama/sono como recuperação principal;
- primeiro pet com área de descanso;
- UI agrícola mais clara.

## 5. Roadmap 2 — Fazenda produtiva e expansível

Objetivo: criar crescimento real de propriedade.

Implementações desejadas:

- expansão de propriedade;
- mais terreno arável;
- posicionamento livre de construções em tiles válidos;
- construção de pasto pequeno;
- primeiros animais;
- baús por categoria;
- caixa de envio com pagamento na manhã seguinte;
- encomendas simples de NPCs;
- fertilizante simples;
- qualidade inicial;
- melhoria de ferramentas;
- produtos processados simples;
- primeira automação leve por companion;
- pet com vínculo e bônus leve.

## 6. Roadmap 3 — Fazenda viva, social e processadora

Objetivo: conectar fazenda, cidade, pets e companions.

Implementações desejadas:

- nova expansão de propriedade;
- galinheiro/curral;
- estufa inicial;
- área de workshops mais robusta;
- processadores/makers;
- NPCs visitando a fazenda;
- companions com jobs de fazenda;
- animais com cuidado, alimentação e produto de qualidade;
- pets com rotina, alimentação, descanso e vínculo;
- eventos sazonais simples;
- festivais/concursos de produção;
- pedidos de NPCs por qualidade/quantidade;
- chuva, season e calendário mais relevantes.

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
- pets com funções específicas;
- crops mágicas/lunares.

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
- pets com habilidades avançadas, sem substituir companions;
- construção/decor final.

---

# PARTE B — Estrutura, tamanho e liberdade de layout

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
| 2 | Fazenda Aberta | aumentar produção e organizar rotina | mais terreno arável, baús, pasto pequeno, primeira expansão de workshops |
| 3 | Fazenda Produtiva | introduzir produção animal/processamento | curral/galinheiro, estufa inicial, storage melhor, primeiros processadores |
| 4 | Fazenda Especializada | automação e produção rara | irrigação avançada, múltiplos workshops, companion jobs robustos, pomar |
| 5 | Fazenda Plena | integração com endgame/lore | pedreira final, área arcana, Fruto Mana, upgrades da Fonte, produção avançada |

## 10. Tamanho sugerido por nível

| Nível | Tamanho aproximado | Observação |
|---:|---:|---|
| 1 | 40x32 tiles | maior que uma tela, mas fácil de ler |
| 2 | 56x40 tiles | abre lateral ou sul para pasto/canteiros |
| 3 | 72x52 tiles | permite estufa, animais e workshops |
| 4 | 88x64 tiles | layout robusto com zonas especializadas |
| 5 | 104x72 tiles | espaço final para pedreira, pomar raro e área arcana |

Esses tamanhos são diretrizes, não contrato técnico. A implementação pode usar tilemaps e zonas desbloqueáveis sem redesenhar toda a cena a cada expansão.

## 11. Regra de liberdade de layout

A fazenda deve dar liberdade para o jogador criar layouts diferentes.

A maior parte do terreno natural da fazenda deve poder ser:

- limpa;
- arada;
- preparada;
- irrigada;
- plantada;
- decorada;
- usada para caminhos;
- usada como área de construção, se for tile válido.

### Regra principal

```text
Quase todo terreno de terra/grama válido pode virar área produtiva ou decorativa.
Apenas ampliações naturais, bordas, água, entradas fixas e marcos narrativos permanecem fixos.
```

## 12. Tiles fixos e tiles livres

### Tiles fixos

Não devem poder ser movidos livremente:

- casa inicial, salvo upgrade visual/posicional muito controlado;
- Fonte de Anya;
- lago principal;
- entrada da caverna;
- saída para a cidade;
- bordas naturais;
- cliffs/rochas grandes de expansão;
- portais ou marcos de lore;
- áreas de expansão ainda bloqueadas.

### Tiles livres

Podem ser modificados ou usados conforme regras:

- grama comum;
- terra comum;
- solo arável;
- árvores removíveis;
- pedras pequenas removíveis;
- arbustos;
- caminhos;
- cercas;
- decoração;
- canteiros;
- construções em pads válidos ou terreno válido.

## 13. Posicionamento de construções

Construções devem poder ser posicionadas pelo jogador, respeitando:

- tamanho em tiles;
- colisão;
- acesso por caminho;
- distância mínima de água/entrada, se necessário;
- zona desbloqueada;
- terreno válido;
- ausência de obstáculos;
- custo pago;
- tempo de construção.

### Regra

As ampliações naturais da fazenda são fixas, mas o layout interno deve ser personalizável.

Exemplos:

```text
A expansão sul sempre libera a área sul.
Dentro dela, o jogador decide onde colocar pasto, caminho, árvore, decoração ou construção permitida.
```

## 14. Modo construção

A fazenda deve ter um modo de construção/edição no futuro.

Funções:

- mostrar grid;
- selecionar construção;
- pré-visualizar footprint;
- validar terreno;
- rotacionar, quando fizer sentido;
- confirmar construção;
- mover construção, se permitido;
- demolir/remover, com recuperação parcial de recursos;
- mostrar bloqueios claramente.

### Estados de validação

```text
ValidPlacement
BlockedByObject
BlockedByTerrain
BlockedByZone
BlockedByPath
InsufficientResources
RequiresUpgrade
```

---

# PARTE C — Zonas da fazenda

## 15. Zonas

| Zona | Função | Nível recomendado | Flexibilidade |
|---|---|---:|---|
| Casa | dormir, salvar, cozinhar, storage, calendário | 1 | fixa ou semimóvel |
| Campo inicial | plantio, rega, colheita | 1 | livre dentro de terreno arável |
| Fonte de Anya | lore, respec, ressurreição de companions | 1 | fixa |
| Lago | pesca, recarga de água | 1 | fixo |
| Bosque | madeira, árvores, pomar futuro | 1 | parcialmente livre |
| SellPoint / Caixa de envio | venda rápida | 1 | semimóvel ou upgradeável |
| Entrada da caverna | preparação para exploração | 1 | fixa |
| Área de workshops | crafting por construção | 1–4 | livre em terreno válido |
| Depósito | baús e storage por categoria | 1–3 | livre |
| Pasto/curral | animais grandes | 2 | livre em área válida |
| Galinheiro | animais pequenos | 3 | livre em área válida |
| Estufa | plantio fora de estação | 3 | livre em área válida |
| Pomar | árvores frutíferas e Fruto Mana | 4–5 | livre, com restrições especiais |
| Área de companions | moradia, job board, gerenciamento | 3–4 | livre em área válida |
| Área arcana da Fonte | upgrades ligados a Anya/Mana | 5 | fixa ou perto da Fonte |
| Pedreira da fazenda | recurso mineral tardio e limitado | 5 | fixa |

---

# PARTE D — Cansaço, stamina, fome e sono

## 16. Status de Cansaço

A fazenda deve considerar um status chamado **Cansaço**.

Cansaço é diferente de stamina.

### Diferença entre recursos corporais

| Recurso | Função |
|---|---|
| HP / Vida | sobrevivência e dano |
| Stamina | energia imediata para ações físicas |
| Fome | manutenção do corpo ao longo do tempo |
| Cansaço | desgaste acumulado do dia e necessidade de dormir |
| Pontos de Magia / MP | recurso para habilidades mágicas, refinado em outro documento |

## 17. Como o cansaço funciona

Cansaço representa desgaste acumulado.

Ele deve aumentar com o passar do tempo e acelerar quando o jogador:

- gasta stamina;
- fica com fome;
- usa ações físicas repetidas;
- passa muito tempo acordado;
- explora caverna;
- sofre status negativos;
- usa magia intensa, se isso for aprovado no sistema de MP.

### Modelo recomendado

```text
Cansaço = 0 a 100
0 = descansado
100 = exausto
```

Ou, se preferir positivo:

```text
Descanso = 100 a 0
100 = descansado
0 = exausto
```

Recomendação: usar `Fatigue`/`Cansaço` acumulando de 0 a 100, porque é mais claro para penalidades.

## 18. Relação entre cansaço, stamina e fome

Stamina é gasto imediato.

Cansaço é acúmulo de desgaste.

Fome acelera o desgaste.

### Regras sugeridas

```text
Tempo passando:
  Cansaço aumenta lentamente.

Ação que gasta stamina:
  Cansaço aumenta proporcionalmente ao custo da ação.

Jogador com fome baixa:
  multiplicador de ganho de Cansaço aumenta.

Jogador muito cansado:
  recuperação de stamina fica pior.

Jogador exausto:
  ações podem ser bloqueadas, reduzidas ou muito penalizadas.
```

### Exemplo de multiplicadores

| Condição | Efeito |
|---|---|
| Fome normal | ganho normal de cansaço |
| Fome baixa | +25% ganho de cansaço |
| Fome crítica | +50% ganho de cansaço |
| Stamina zerada | ações físicas aumentam muito o cansaço |
| Caverna | ganho de cansaço maior que fazenda |
| Dormir tarde | acorda com recuperação parcial ou penalidade leve |

## 19. Penalidades por cansaço

| Cansaço | Estado | Efeito sugerido |
|---:|---|---|
| 0–24 | Descansado | sem penalidade |
| 25–49 | Levemente cansado | feedback visual leve |
| 50–74 | Cansado | stamina recupera pior, ações custam mais |
| 75–89 | Muito cansado | movimento/ações sofrem penalidade, risco na caverna aumenta |
| 90–100 | Exausto | ações pesadas bloqueadas ou quase inviáveis; dormir é recomendado |

## 20. Sono

Dormir é a principal forma de reduzir cansaço.

Regras:

- dormir na cama reduz cansaço drasticamente;
- dormir cedo recupera melhor;
- dormir tarde pode recuperar menos;
- comida pode ajudar indiretamente, mas não substitui sono;
- poções ou Fruto Mana podem reduzir cansaço temporariamente, mas com limite;
- pets/companions podem dar buffs de descanso, mas não remover a necessidade de dormir.

## 21. Save de cansaço

Cansaço deve ser persistido.

Campos sugeridos:

```text
PlayerConditionSaveData
  CurrentFatigue
  CurrentStamina
  CurrentHunger
  LastSleepDay
  LastSleepHour
  SleepQualityModifier
```

Observação: esse contrato é transversal e deve ser refinado no documento de Progressão/Status/Magia, mas a fazenda precisa referenciar porque a rotina agrícola depende dele.

---

# PARTE E — Pets

## 22. Papel dos pets

Pets são companheiros domésticos da fazenda.

Eles não substituem companions humanos/NPCs.

Eles servem para:

- criar vínculo afetivo com a fazenda;
- dar pequenos bônus;
- participar de rotinas;
- reforçar vida doméstica;
- ajudar em combate ou exploração de forma limitada, no caso do cachorro;
- interagir com animais, campo e casa.

## 23. Pets iniciais

| Pet | Função principal | Possível bônus |
|---|---|---|
| Cachorro | proteção, combate leve, alerta | bônus em combate, detecção de inimigos, segurança na fazenda |
| Gato | descanso, sorte, controle de pragas | redução leve de cansaço ao dormir, chance de item pequeno, proteção de crops |

## 24. Regras de cuidado dos pets

Pets precisam de:

- nome;
- alimentação;
- área de descanso;
- vínculo/afinidade;
- rotina simples;
- feedback visual;
- estado salvo.

### Estados possíveis

```text
Healthy
Hungry
Tired
Resting
Following
Working
Unavailable
```

Evitar morte permanente de pets, salvo decisão futura muito cuidadosa. A recomendação atual é não usar morte permanente para pets.

## 25. Área de descanso dos pets

A fazenda deve ter uma área para pets descansarem.

Pode começar simples:

- cama de pet dentro ou perto da casa;
- casinha de cachorro;
- almofada/cesto para gato;
- tigela de comida;
- área de água.

Upgrades possíveis:

- casinha melhor;
- área cercada;
- acessórios;
- brinquedos;
- cama mágica/abençoada;
- buff de descanso.

## 26. Alimentação dos pets

Regras:

- pet alimentado tem vínculo/rotina normal;
- pet sem alimento perde eficiência/bônus;
- pet não deve morrer facilmente por falta de comida;
- alimentação pode usar ração, peixe, carne, leite ou comida específica;
- gato pode preferir peixe;
- cachorro pode preferir ração/carne;
- alimentos especiais podem dar buff temporário.

## 27. Vínculo com pets

O vínculo pode crescer com:

- alimentar;
- acariciar/interagir;
- levar junto em atividades;
- vencer combate com cachorro;
- deixar descansar;
- dar comida favorita;
- construir upgrades.

Efeitos por vínculo:

| Vínculo | Efeito |
|---|---|
| Baixo | pet aparece na fazenda, poucos bônus |
| Médio | pet segue o jogador e dá pequenos buffs |
| Alto | pet ajuda em tarefas específicas |
| Máximo | habilidade especial ou evento próprio |

## 28. Cachorro em combate

O cachorro pode participar de combate, mas com limites.

Funções possíveis:

- atacar inimigos fracos;
- distrair inimigos;
- latir/alertar emboscada;
- detectar inimigos próximos;
- encontrar item após combate;
- ajudar a proteger o jogador em cave runs iniciais/intermediárias.

Regras:

- cachorro não deve substituir companion de caverna;
- cachorro não deve carregar o combate sozinho;
- cachorro pode ter HP/estado, mas evitar morte permanente;
- se cair em combate, fica `Unavailable` até descansar/tratar;
- pode exigir comida, descanso e vínculo para acompanhar.

## 29. Gato na fazenda

O gato deve ser mais doméstico/utilitário.

Funções possíveis:

- reduzir ganho de cansaço após dormir, por buff de descanso;
- aumentar pequena chance de sorte na manhã;
- trazer item pequeno ocasional;
- reduzir chance de pragas em crops;
- interagir com Fruto Mana/crops lunares de forma rara;
- reagir a Nyx/Alihana.

Regras:

- gato não deve virar combatente principal;
- gato pode ser buff passivo da fazenda;
- gato pode ser forte em eventos de lua/lore.

## 30. Save de pets

Campos sugeridos:

```text
PetSaveData
  PetInstanceId
  PetType
  Name
  BondLevel
  HungerState
  RestState
  IsFollowingPlayer
  AssignedPetBedId
  LastFedDay
  LastInteractionDay
  TemporaryBuffs[]
```

---

# PARTE F — Pedreira e mineração

## 31. Pedreira e mineração

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

# PARTE G — Referências externas úteis sem copiar identidade

## 32. Aprendizados de farm sims

Cindar's Hope pode aprender com farm sims clássicos sem copiar assets, personagens, paleta, UI ou progressão exata.

| Mecânica | Valor para Cindar's Hope | Adaptação recomendada |
|---|---|---|
| Rega diária e chuva | cria rotina e planejamento | manter rega, chuva molha área externa, irrigação reduz repetição |
| Solo arado/molhado/plantado | clareza visual | já alinhado ao estado atual de FarmPlot |
| Terreno livre para arar | liberdade criativa | quase todo solo válido pode ser preparado |
| Posicionamento de construções | personalização | construções livres em tiles válidos; expansões naturais fixas |
| Sprinklers/irrigação | automação gradual | usar canais, aspersores, runas hidráulicas ou engenharia de Carpintaria |
| Fertilizantes | escolha estratégica por ciclo | usar tiers: crescimento, yield, qualidade e lunar |
| Qualidade de crops | valor econômico e progressão | usar `ItemId + Quality` ou metadado equivalente |
| Animais com cuidado/afeto | rotina e vínculo | usar cuidado diário, qualidade do produto e companion Tratador |
| Pets | vínculo doméstico | cachorro/gato com alimentação, descanso e bônus |
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

# PARTE H — Construções e produção

## 33. Construções essenciais

| Construção | Função |
|---|---|
| Casa | dormir, salvar, cozinhar, storage, calendário |
| Fonte de Anya | respec, companions, lore, eventos |
| SellPoint/Caixa de envio | venda rápida |
| Entrada da caverna | preparação e acesso |
| Baú inicial | storage básico |
| Área de pet | descanso, alimentação e vínculo |

## 34. Construções produtivas

| Construção | Função | Roadmap |
|---|---|---:|
| Pasto | animais grandes | 2 |
| Galinheiro | aves e ovos | 3 |
| Estufa | crops fora de estação | 3 |
| Pomar | árvores frutíferas | 4 |
| Área arcana | Fruto Mana e crops raras | 5 |
| Pedreira final | mineral tardio limitado | 5 |

## 35. Workshops e processadores

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

# PARTE I — Plantio, solo, clima, qualidade e fertilizante

## 36. Estados de solo

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

## 37. Regras finais de solo e rega

```text
Planta molhada cresce.
Planta seca não cresce.
Planta seca acumula DaysWithoutWater.
Com 3 dias sem água, vira Dead.
Chuva molha áreas externas.
Estufa pode usar irrigação própria.
Irrigação automatizada entra por construção/upgrade.
```

## 38. Ferramentas agrícolas

| Ferramenta | Função | Evolução |
|---|---|---|
| Hoe | arar solo | área maior por upgrade |
| Watering Can | molhar solo | mais carga/área por upgrade |
| Axe | cortar árvores | mais dano/yield |
| Fishing Rod | pescar | peixes raros/timing melhor |
| Sickle | limpar planta morta/fibra | área maior |
| Build Tool | mover/construir | modo edição |
| Fertilizer Applicator | aplicar fertilizante em área | avançado |

## 39. Clima, seasons e luas

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

## 40. Qualidade

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
- pet, em bônus leve e raro;
- Fruto Mana/crops mágicas;
- eventos sazonais;
- solo arcano.

Representação técnica preferencial:

```text
ItemId + Quality
```

## 41. Fertilizantes

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

# PARTE J — Fruto Mana

## 42. Fruto Mana

Existe em Vaalara um fruto chamado **Mana**.

Características:

- raro;
- difícil de fazer florescer;
- lembra um pêssego roxo;
- possui ligação natural com energia mágica;
- quando consumido por humanos, concede temporariamente status mais fortes;
- pode ser usado para curar ferimentos no contexto de gameplay;
- deve ser recurso avançado de fazenda/lore, não crop comum.

## 43. Nomenclatura: Mana vs Pontos de Magia

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

## 44. Cultivo do Fruto Mana

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
- companion com afinidade alta ajudando;
- gato com vínculo alto pode sinalizar uma condição lunar rara, se aprovado.

## 45. Usos do Fruto Mana

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

# PARTE K — Animais

## 46. Animais planejados

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

# PARTE L — Companion jobs na fazenda

## 47. Jobs

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

# PARTE M — Economia agrícola

## 48. Canais de venda

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

# PARTE N — Save e dados futuros

## 49. FarmSaveData

```text
FarmSaveData
  FarmLevel
  UnlockedZones[]
  Plots[]
  Trees[]
  FruitTrees[]
  Buildings[]
  Animals[]
  Pets[]
  ShippingBin
  CompanionJobs[]
  FountainState
  IrrigationNetwork
  QuarryState
  ManaCultivationState
```

## 50. PlotSaveData

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

## 51. ManaCultivationSaveData

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

## 52. AnimalSaveData

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

## 53. PetSaveData

```text
PetInstanceId
PetType
Name
BondLevel
HungerState
RestState
IsFollowingPlayer
AssignedPetBedId
LastFedDay
LastInteractionDay
TemporaryBuffs[]
```

## 54. BuildingSaveData

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

## 55. PlayerConditionSaveData

```text
CurrentFatigue
CurrentStamina
CurrentHunger
CurrentHP
CurrentMP
LastSleepDay
LastSleepHour
SleepQualityModifier
```

Observação: `CurrentMP` deve ser definido no design direction de progressão/magia; aqui aparece só porque a fazenda referencia Fruto Mana e recuperação de recursos.

---

# PARTE O — Specs futuras derivadas

## 56. Ordem recomendada

1. `spec_farm_layout_expansion_zones_free_build.md`
2. `spec_player_condition_fatigue_sleep_hunger_stamina.md`
3. `spec_farm_buildings_construction_workshops_storage.md`
4. `spec_farm_crop_death_quality_fertilizers.md`
5. `spec_farm_weather_rain_irrigation_automation.md`
6. `spec_farm_shipping_bin_orders_processing.md`
7. `spec_farm_animals_pasture_products_care.md`
8. `spec_farm_pets_dog_cat_bond_buffs.md`
9. `spec_farm_companion_jobs_automation.md`
10. `spec_farm_orchard_mana_fruit_arcane_soil.md`
11. `spec_farm_final_quarry_late_game_resources.md`

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

# PARTE P — Relação com outros design directions

| Documento futuro | Relação |
|---|---|
| Cidade/NPCs | encomendas, compras, animais, pets, visitas, reputação |
| Caverna | minerais, reagentes, Pedra Preta, desbloqueio de pedreira, cansaço na exploração |
| Progressão/Magia | Pontos de Magia, buffs, skills agrícolas/mágicas, cansaço |
| Companions | jobs, afinidade, automação, relação com pets |
| Crafting | processadores, fertilizantes, produtos |
| UI/UX | menus, hotbar, storage, shipping, layout, modo construção, status bars |
| Lore | Fonte, Anya, Fruto Mana, Cindar |

---

# PARTE Q — Decisões pendentes

## 57. Produto

- Qual é o nome oficial do nível máximo da fazenda?
- A Fonte existe ativa desde o começo ou começa dormente?
- Fazenda nível 5 exige qual marco da caverna?
- Fruto Mana floresce com Alihana, Nyx ou condição própria?
- Mana é árvore, arbusto ou planta arcana única?
- Fruto Mana pode ser vendido livremente ou deve ser protegido por lore/raridade?
- Animais podem morrer permanentemente ou entram em estado recuperável?
- Pets podem morrer? Recomendação atual: não.
- Cachorro ocupa slot de companion em combate ou é pet separado?
- Gato pode gerar bônus lunar ligado a Nyx/Alihana?
- Caixa de envio paga na manhã seguinte ou instantâneo?
- Processadores puxam itens do inventário, baús ou depósito central?
- Companion pode usar fertilizante raro automaticamente?
- Chuva pode falhar em eventos de seca?
- Pedreira final usa companion Minerador?
- O jogador pode mover casa/SellPoint ou apenas construções secundárias?

## 58. Técnicas

- Qualidade será metadado no stack ou instância individual?
- `ItemId + Quality` é suficiente para todos os produtos?
- Fruto Mana precisa de item instanciado único?
- Save de irrigação será por plot ou rede de tiles?
- Construções serão posicionadas livremente ou em pads fixos?
- Expansão usa tilemap revelado ou cenas/áreas separadas?
- Pedreira final usa resource nodes persistentes ou geração diária simples?
- Cansaço fica em PlayerConditionManager, HungerManager ou sistema novo?
- Pet é sistema próprio ou subtipo de companion?

---

## 59. Próximos documentos recomendados

A partir deste design direction, os próximos documentos de design/refinamento devem ser:

```text
docs/design/gameplay/farm/FARM_LAYOUT_EXPANSION_ZONES_DIRECTION_v1.0.md
docs/design/gameplay/farm/FARM_CROPS_QUALITY_FERTILIZERS_MANA_DIRECTION_v1.0.md
docs/design/gameplay/farm/FARM_PETS_DIRECTION_v1.0.md
docs/design/gameplay/combat_magic_progression/COMBAT_MAGIC_PROGRESSION_DESIGN_DIRECTION_v1.0.md
```

Depois de consolidados, cada um deve alimentar specs menores em `docs/specs/a_implementar/`.
