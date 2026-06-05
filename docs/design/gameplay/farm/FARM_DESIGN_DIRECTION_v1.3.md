# Cindar's Hope — Farm Design Direction v1.3

> **Status:** direção ativa consolidada de gameplay/sistema  
> **Local:** `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`  
> **Substitui como direção ativa:** `FARM_DESIGN_DIRECTION_v1.2.md`  
> **Base de canon:** `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`  
> **Função:** grande descrição de como a fazenda deve funcionar no jogo.  
> **Não é spec implementável.** Specs futuras devem ser quebradas em `docs/specs/a_implementar/`.

---

## 0. Decisão desta versão

Esta versão preserva a estrutura da v1.2 e aplica correções de canon de Vaalara:

- Fruto Mana deixa de ser tratado como crop plantável comum.
- Árvore de Mana é consciente, rara e escolhe onde crescer.
- Introduz `Raiz Dormente de Mana` como opção canônica para endgame da fazenda.
- Introduz `Água Viva da Fonte` como recurso raro ligado a Anya.
- Corrige o uso de Nymirianos como povo antigo ligado a Anya.
- Separa agricultura cotidiana de Thandra da camada sagrada de Anya.
- Conecta Bromécia a tecnologia agrícola avançada, com risco e ambiguidade.
- Diferencia `Pedra de Meteoro Negra Estabilizada` de `Pedra Negra Cultista/Corrompida`.
- Reforça o tom: fazenda pastoral na superfície, horror antigo no subsolo.

Documentos históricos:

```text
docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.1.md
docs/design/gameplay/farm/FARM_DESIGN_DECISIONS_v1.2.md
docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.2.md
```

A direção ativa passa a ser este arquivo.

---

# PARTE A — Papel da fazenda

## 1. Identidade da fazenda

A fazenda é a **base viva do jogador**.

Ela conecta rotina, economia, cidade, companions, crafting, caverna, pets, progressão física/mágica e lore.

A fazenda deve cumprir sete papéis:

1. **Produção:** crops, árvores, animais, pesca, recursos leves e itens processados.
2. **Preparação:** comida, poções, fertilizantes, ferramentas, equipamentos, buffs e logística para caverna.
3. **Economia:** venda comum, encomendas, produtos de qualidade, recursos raros e bens processados.
4. **Social:** NPCs, companions, visitas, ajuda, eventos e reputação.
5. **Rotina corporal:** fome, stamina, cansaço, sono, cuidado animal e cuidado dos pets.
6. **Personalização:** layout livre, área arável ampla, construções móveis, decoração e organização própria.
7. **Lore:** Fonte de Anya, Água Viva, Fruto Mana, ecos de Cindar, Thandra, Bromécia e ruínas subterrâneas.

## 2. Regra principal

A fazenda deve sempre gerar decisões, não apenas tarefas repetitivas.

Toda mecânica de fazenda precisa servir pelo menos uma das funções abaixo:

- produzir recurso;
- preparar para a caverna;
- alimentar crafting;
- gerar economia;
- criar rotina diária interessante;
- sustentar relação com pets, companions ou NPCs;
- reforçar lore de Anya, Cindar, Fonte, Mana, Thandra ou Bromécia;
- desbloquear expansão ou progressão;
- permitir personalização/layout.

## 3. Regra de tom: pastoral acima, horror abaixo

A fazenda deve ser confortável na superfície, mas ligada a algo antigo no subsolo.

```text
Superfície:
  campo, animais, pets, casa, lago, árvores, rotina, comida, festivais e vida simples.

Subsolo:
  caverna, ruínas, cultos, portais, Bromécia, Elyndor, Pedras Negras e ecos de Anya.
```

O jogador começa com uma fantasia rural clara. A profundidade vem aos poucos.

---

# PARTE B — Roadmap conceitual

## 4. Roadmap de implementação

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

## 5. Roadmap 0 — Base já existente / reconciliada

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

## 6. Roadmap 1 — Fazenda jogável essencial

Objetivo: deixar a fazenda com rotina diária completa e coerente.

Implementações desejadas:

- layout canônico do primeiro nível da fazenda;
- casa com dormir/salvar;
- campo inicial organizado;
- grande área arável e customizável;
- lago funcional;
- bosque com árvores;
- SellPoint/caixa de venda;
- Fonte de Anya visível, ainda limitada ou parcialmente dormente;
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

## 7. Roadmap 2 — Fazenda produtiva e expansível

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

## 8. Roadmap 3 — Fazenda viva, social e processadora

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
- chuva, season e calendário mais relevantes;
- culto rural de Thandra como camada cultural cotidiana.

## 9. Roadmap 4 — Fazenda especializada e automatizável

Objetivo: reduzir repetição sem remover decisão.

Implementações desejadas:

- expansão para zonas especializadas;
- irrigação avançada;
- aspersores, canais, runas hidráulicas ou mecanismos equivalentes;
- fertilizante intermediário/avançado;
- pomar comum;
- árvores frutíferas não-Mana;
- storage central;
- workshops em nível alto;
- companion job board;
- produção artesanal avançada;
- plantio por plano/blueprint de campo;
- rotinas automatizadas configuráveis;
- pets com funções específicas;
- crops mágicas/lunares não equivalentes a Mana.

## 10. Roadmap 5 — Fazenda final e endgame

Objetivo: conectar fazenda ao endgame de Vaalara.

Implementações desejadas:

- expansão máxima da fazenda;
- pedreira final;
- área arcana da Fonte;
- Raiz Dormente de Mana ou evento equivalente;
- Água Viva da Fonte;
- fertilizante lunar/arcano;
- crops raras ligadas às luas;
- upgrades finais da Fonte de Anya;
- produção de poções e alimentos endgame;
- eventos raros de Alihana/Senya/Nyx;
- integração com nível 100/101 da caverna;
- encomendas lendárias;
- pets com habilidades avançadas, sem substituir companions;
- tecnologia agrícola bromeciana com custo/risco;
- construção/decor final.

---

# PARTE C — Estrutura, tamanho e liberdade de layout

## 11. Níveis de propriedade

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
| 4 | Fazenda Especializada | automação e produção rara | irrigação avançada, múltiplos workshops, companion jobs robustos, pomar comum |
| 5 | Fazenda Plena | integração com endgame/lore | pedreira final, área arcana, Raiz Dormente de Mana, upgrades da Fonte, produção avançada |

## 12. Tamanho sugerido por nível

| Nível | Tamanho aproximado | Observação |
|---:|---:|---|
| 1 | 40x32 tiles | maior que uma tela, mas fácil de ler |
| 2 | 56x40 tiles | abre lateral ou sul para pasto/canteiros |
| 3 | 72x52 tiles | permite estufa, animais e workshops |
| 4 | 88x64 tiles | layout robusto com zonas especializadas |
| 5 | 104x72 tiles | espaço final para pedreira, pomar raro e área arcana |

Esses tamanhos são diretrizes, não contrato técnico. A implementação pode usar tilemaps e zonas desbloqueáveis sem redesenhar toda a cena a cada expansão.

## 13. Liberdade de layout

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

Regra principal:

```text
Quase todo terreno de terra/grama válido pode virar área produtiva ou decorativa.
Apenas ampliações naturais, bordas, água, entradas fixas e marcos narrativos permanecem fixos.
```

## 14. Tiles fixos e tiles livres

### Tiles fixos

Não devem poder ser movidos livremente:

- Fonte de Anya;
- lago principal;
- entrada da caverna;
- saída para a cidade;
- bordas naturais;
- cliffs/rochas grandes de expansão;
- portais ou marcos de lore;
- área da pedreira final;
- área arcana diretamente ligada à Fonte;
- possível Raiz Dormente de Mana, se descoberta;
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
- construções em terreno válido.

## 15. Posicionamento e movimento de construções

O jogador pode mover:

- casa;
- SellPoint/caixa de envio;
- construções secundárias;
- baús;
- áreas de pet;
- workshops;
- pastos/currais/galinheiros;
- decoração;
- processadores.

A casa começa em posição fixa, mas pode ser movida depois que o modo construção/expansão permitir.

Construções devem poder ser posicionadas pelo jogador, respeitando:

- tamanho em tiles;
- colisão;
- acesso por caminho;
- distância mínima de água/entrada, se necessário;
- zona desbloqueada;
- terreno válido;
- ausência de obstáculos;
- custo pago;
- tempo de construção;
- segurança de save/load.

## 16. Modo construção

A fazenda deve ter um modo de construção/edição.

Funções:

- mostrar grid;
- selecionar construção;
- pré-visualizar footprint;
- validar terreno;
- rotacionar, quando fizer sentido;
- confirmar construção;
- mover construção;
- demolir/remover, com recuperação parcial de recursos;
- mostrar bloqueios claramente.

Estados de validação:

```text
ValidPlacement
BlockedByObject
BlockedByTerrain
BlockedByZone
BlockedByPath
BlockedByLoreAnchor
InsufficientResources
RequiresUpgrade
```

## 17. Movimento da casa

Mover a casa deve ser uma ação avançada, não necessariamente disponível no início.

Regras sugeridas:

- exige modo construção;
- pode ter custo em ouro/materiais;
- precisa preservar ponto de spawn do jogador;
- precisa atualizar portas, colisores, navegação e save;
- pode levar 1 dia de obra ou ser instantâneo após confirmação, a decidir em spec.

## 18. Movimento do SellPoint

SellPoint/caixa de envio pode ser movido com menos restrição que a casa.

Regras sugeridas:

- deve ficar em tile acessível;
- não pode bloquear entrada/saída;
- deve preservar itens pendentes de envio;
- mover não pode apagar conteúdo;
- pode ter custo baixo ou gratuito dentro do modo construção.

---

# PARTE D — Zonas da fazenda

## 19. Zonas

| Zona | Função | Nível recomendado | Flexibilidade |
|---|---|---:|---|
| Casa | dormir, salvar, cozinhar, storage, calendário | 1 | móvel após desbloqueio |
| Campo inicial | plantio, rega, colheita | 1 | livre dentro de terreno arável |
| Fonte de Anya | lore, respec, ressurreição de companions, Água Viva | 1 | fixa |
| Lago | pesca, recarga de água comum | 1 | fixo |
| Bosque | madeira, árvores, pomar futuro | 1 | parcialmente livre |
| SellPoint / Caixa de envio | venda rápida | 1 | móvel após desbloqueio |
| Entrada da caverna | preparação para exploração | 1 | fixa |
| Área de workshops | crafting por construção | 1–4 | livre em terreno válido |
| Depósito | baús e storage por categoria | 1–3 | livre |
| Pasto/curral | animais grandes | 2 | livre em área válida |
| Galinheiro | animais pequenos | 3 | livre em área válida |
| Estufa | plantio fora de estação | 3 | livre em área válida |
| Pomar comum | árvores frutíferas comuns e especiais não-Mana | 4 | livre |
| Área de companions | moradia, job board, gerenciamento | 3–4 | livre em área válida |
| Área de pets | descanso, alimentação e vínculo | 1–4 | livre, vinculada à casa ou pátio |
| Área arcana da Fonte | upgrades ligados a Anya, Água Viva e Mana | 5 | fixa ou perto da Fonte |
| Raiz Dormente de Mana | evento/lore/endgame | 5 | fixa após descoberta |
| Pedreira da fazenda | recurso mineral tardio e limitado | 5 | fixa |

---

# PARTE E — Cansaço, stamina, fome, sono e PlayerCondition

## 20. Decisão

O **Cansaço** deve ser implementado como sistema próprio.

Nome conceitual sugerido:

```text
FatigueSystem
```

ou:

```text
FatigueManager
```

Esse sistema não deve ser absorvido pelo `HungerManager`.

Ele também não deve ser apenas um campo solto dentro do `PlayerConditionManager`.

## 21. Relação com PlayerConditionManager

O `PlayerConditionManager` deve continuar sendo a camada agregadora/observável das condições do jogador.

Relação desejada:

```text
FatigueSystem
  calcula, altera e persiste regras específicas de cansaço

PlayerConditionManager
  expõe estado consolidado do jogador para UI, save, eventos e outros sistemas
```

Regra:

```text
Cansaço tem sistema próprio.
PlayerConditionManager é afetado por ele e deve refletir o estado final.
```

## 22. Diferença entre recursos corporais

| Recurso | Função |
|---|---|
| HP / Vida | sobrevivência e dano |
| Stamina | energia imediata para ações físicas |
| Fome | manutenção do corpo ao longo do tempo |
| Cansaço | desgaste acumulado do dia e necessidade de dormir |
| Pontos de Magia / MP | recurso para habilidades mágicas, refinado no design de magia/progressão |

## 23. Como o cansaço funciona

Cansaço representa desgaste acumulado.

Ele deve aumentar com o passar do tempo e acelerar quando o jogador:

- gasta stamina;
- fica com fome;
- usa ações físicas repetidas;
- passa muito tempo acordado;
- explora caverna;
- sofre status negativos;
- usa magia intensa, se isso for aprovado no sistema de MP.

Modelo recomendado:

```text
Cansaço = 0 a 100
0 = descansado
100 = exausto
```

## 24. Relação entre cansaço, stamina e fome

Stamina é gasto imediato. Cansaço é acúmulo de desgaste. Fome acelera o desgaste.

Regras sugeridas:

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

Exemplo de multiplicadores:

| Condição | Efeito |
|---|---|
| Fome normal | ganho normal de cansaço |
| Fome baixa | +25% ganho de cansaço |
| Fome crítica | +50% ganho de cansaço |
| Stamina zerada | ações físicas aumentam muito o cansaço |
| Caverna | ganho de cansaço maior que fazenda |
| Noite sob Nyx | pode reduzir ganho de cansaço em ações noturnas específicas |
| Dormir tarde | acorda com recuperação parcial ou penalidade leve |

## 25. Penalidades por cansaço

| Cansaço | Estado | Efeito sugerido |
|---:|---|---|
| 0–24 | Descansado | sem penalidade |
| 25–49 | Levemente cansado | feedback visual leve |
| 50–74 | Cansado | stamina recupera pior, ações custam mais |
| 75–89 | Muito cansado | movimento/ações sofrem penalidade, risco na caverna aumenta |
| 90–100 | Exausto | ações pesadas bloqueadas ou quase inviáveis; dormir é recomendado |

## 26. Sono

Dormir é a principal forma de reduzir cansaço.

Regras:

- dormir na cama reduz cansaço drasticamente;
- dormir cedo recupera melhor;
- dormir tarde pode recuperar menos;
- comida pode ajudar indiretamente, mas não substitui sono;
- poções ou Água Viva/Fruto Mana podem reduzir cansaço temporariamente, mas com limite;
- pets/companions podem dar buffs de descanso, mas não remover a necessidade de dormir.

## 27. Responsabilidades do FatigueSystem

O sistema de cansaço deve controlar:

- aumento natural de cansaço com o tempo;
- aumento adicional por gasto de stamina;
- aumento acelerado por fome;
- aumento diferenciado em caverna;
- efeitos de dormir cedo/tarde;
- penalidades por cansaço alto;
- recuperação ao dormir;
- modificadores vindos de comida, poções, pets, buffs, Água Viva ou Fruto Mana;
- eventos relacionados a mudança de faixa de cansaço.

## 28. Responsabilidades do PlayerConditionManager

O `PlayerConditionManager` deve refletir:

- HP/Vida;
- Stamina;
- Fome;
- Cansaço;
- Pontos de Magia/MP quando formalizado;
- estados derivados, como `Exhausted`, `Hungry`, `LowStamina`, `LowHP`.

Eventos candidatos:

```text
FatigueChangedEvent
FatigueThresholdReachedEvent
PlayerExhaustedEvent
PlayerRestedEvent
SleepQualityCalculatedEvent
PlayerConditionChangedEvent
```

---

# PARTE F — Fonte de Anya e Água Viva

## 29. Papel da Fonte

A Fonte de Anya é um marco fixo da fazenda.

Ela deve servir como:

- elemento visual forte;
- lugar de lore;
- respec;
- ressurreição de companions;
- purificação;
- cura limitada;
- conexão com Anya;
- possível chave para Mana e nível 100/101.

## 30. Estado inicial da Fonte

A Fonte pode existir desde o começo, mas não precisa estar plenamente ativa.

Estados possíveis:

```text
Dormant
Awakened
Strengthened
Purified
Unstable
```

## 31. Água Viva da Fonte

Adicionar recurso:

```text
Água Viva da Fonte
```

Funções possíveis:

- cura limitada;
- redução leve ou temporária de cansaço;
- ingrediente de poção avançada;
- purificação de item corrompido;
- ativação de ritual;
- tentativa de despertar Raiz Dormente de Mana;
- interação com eventos de Alihana.

Regras:

- recurso raro;
- não substitui poções comuns;
- não é coletável infinitamente;
- recarrega por tempo, lua, upgrade da Fonte ou evento;
- pode ser afetada por corrupção subterrânea;
- pode ser necessária para algumas quests de Anya/Cindar.

---

# PARTE G — Pets

## 32. Decisão

Pets são sistema próprio.

Pets não são subtipo de companion.

Nome conceitual sugerido:

```text
PetSystem
PetManager
```

## 33. Diferença entre pet e companion

| Aspecto | Pet | Companion |
|---|---|---|
| Papel | vínculo doméstico, bônus, suporte leve | personagem aliado com jobs, combate ou funções maiores |
| Origem | animal doméstico/familiar | NPC ou personagem recrutável |
| Slot de caverna | não ocupa slot principal | ocupa slot de companion ativo, quando aplicável |
| Progressão | vínculo/cuidado | nível, afinidade, jobs, habilidades |
| Morte permanente | não recomendada | pode ficar indisponível até ressurreição |
| Gestão | área de descanso, alimentação, seguir/ficar | job board, moradia, equipamento, caverna |

## 34. Pets iniciais

| Pet | Função principal | Possível bônus |
|---|---|---|
| Cachorro | proteção, combate leve, alerta | bônus em combate, detecção de inimigos, segurança na fazenda |
| Gato | descanso, sorte, controle de pragas | redução leve de cansaço ao dormir, chance de item pequeno, proteção de crops, reação lunar |

## 35. Regras de cuidado dos pets

Pets precisam de:

- nome;
- alimentação;
- área de descanso;
- vínculo/afinidade;
- rotina simples;
- feedback visual;
- estado salvo.

Estados possíveis:

```text
Healthy
Hungry
Tired
Resting
Following
Working
Unavailable
```

Recomendação: pets não devem ter morte permanente.

## 36. Área de descanso dos pets

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

## 37. Alimentação dos pets

Regras:

- pet alimentado tem vínculo/rotina normal;
- pet sem alimento perde eficiência/bônus;
- pet não deve morrer facilmente por falta de comida;
- alimentação pode usar ração, peixe, carne, leite ou comida específica;
- gato pode preferir peixe;
- cachorro pode preferir ração/carne;
- alimentos especiais podem dar buff temporário.

## 38. Vínculo com pets

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

## 39. Cachorro em combate

O cachorro é separado do companion.

Ele não ocupa o slot de companion de combate/caverna.

Fluxo desejado:

```text
Player
  + 1 Companion ativo, se houver
  + Cachorro, se estiver seguindo e disponível
```

O cachorro pode:

- atacar inimigos fracos;
- distrair inimigos;
- alertar emboscadas;
- detectar inimigos próximos;
- encontrar item após combate;
- proteger o jogador em situações leves/intermediárias.

O cachorro não deve:

- substituir companion de caverna;
- carregar combate sozinho;
- derrotar bosses sozinho;
- escalar como personagem principal;
- invalidar builds de jogador;
- remover risco da caverna.

Se o cachorro cair em combate:

```text
CombatAvailabilityState = Unavailable
```

Ele deve precisar de descanso, comida, cura simples ou retorno à fazenda.

## 40. Gato na fazenda

O gato deve ser mais doméstico/utilitário.

Funções possíveis:

- reduzir ganho de cansaço após dormir, por buff de descanso;
- aumentar pequena chance de sorte na manhã;
- trazer item pequeno ocasional;
- reduzir chance de pragas em crops;
- reagir a Nyx/Alihana;
- sinalizar evento lunar raro;
- reagir à Raiz Dormente de Mana, sem resolver o evento sozinho.

---

# PARTE H — Pedreira, mineração e Pedras Negras

## 41. Filosofia de mineração

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
- Pedra de Meteoro Negra Estabilizada;
- Pedra Negra Cultista/Corrompida;
- materiais de boss;
- recursos para upgrades finais.

## 42. Pedreira final

A pedreira da fazenda **não é fonte principal de mineração**.

A mineração principal deve vir da caverna e dos níveis dela.

A pedreira da fazenda é um recurso tardio, liberado apenas no último nível da fazenda.

Função da pedreira final:

- gerar pequena renda passiva de pedra/minério básico;
- oferecer conveniência no endgame;
- alimentar construções finais sem obrigar runs longas para materiais simples;
- gerar raramente fragmentos específicos para crafting;
- nunca substituir exploração de caverna.

Regra:

```text
Mineração principal = caverna.
Pedreira da fazenda = conveniência tardia, limitada e controlada.
```

Regras sugeridas:

- liberada apenas no nível 5 da fazenda;
- custo alto de construção/liberação;
- exige progresso avançado na caverna;
- respawn lento;
- yield diário/semanal limitado;
- materiais avançados continuam exclusivos da caverna;
- pode receber upgrade final para uso por companion Minerador;
- não gera materiais de boss nem recursos principais de progressão.

## 43. Distinção de Pedras Negras

Para evitar conflito de uso, distinguir:

### Pedra de Meteoro Negra Estabilizada

Uso:

- tecnologia de Elyndor;
- portal/checkpoint;
- estabilização de arco;
- pode exigir purificação/ativação;
- não drena almas por padrão.

### Pedra Negra Cultista / Corrompida

Uso:

- cultos;
- drenagem de alma;
- rituais proibidos;
- corrupção;
- horror subterrâneo;
- risco narrativo e mecânico.

Regra:

```text
Toda Pedra Negra Cultista pode ter origem em meteoro/portal corrompido.
Nem toda Pedra de Meteoro Negra é cultista ou drenadora.
```

---

# PARTE I — Referências externas úteis sem copiar identidade

## 44. Aprendizados de farm sims

Cindar's Hope pode aprender com farm sims clássicos sem copiar assets, personagens, paleta, UI ou progressão exata.

| Mecânica | Valor para Cindar's Hope | Adaptação recomendada |
|---|---|---|
| Rega diária e chuva | cria rotina e planejamento | manter rega, chuva molha área externa, irrigação reduz repetição |
| Solo arado/molhado/plantado | clareza visual | já alinhado ao estado atual de FarmPlot |
| Terreno livre para arar | liberdade criativa | quase todo solo válido pode ser preparado |
| Posicionamento de construções | personalização | construções livres em tiles válidos; expansões naturais fixas |
| Sprinklers/irrigação | automação gradual | usar canais, aspersores, runas hidráulicas ou engenharia de Carpintaria/Bromécia |
| Fertilizantes | escolha estratégica por ciclo | usar tiers: crescimento, yield, qualidade e lunar |
| Qualidade de crops | valor econômico e progressão | usar `ItemId + Quality` ou metadado equivalente |
| Animais com cuidado/afeto | rotina e vínculo | usar cuidado diário, qualidade do produto e companion Tratador |
| Pets | vínculo doméstico | cachorro/gato com alimentação, descanso e bônus |
| Processadores/makers | transformação de matéria-prima | usar workshops e máquinas: moinho, prensa, fermentador, tear, queijaria |
| Estufa | quebra de sazonalidade | manter como construção avançada |
| Árvores frutíferas | planejamento de longo prazo | usar para frutas comuns; Mana é exceção canônica consciente |
| Caixa de envio | conveniência | usar com preço base; lojas/encomendas pagam melhor |
| Festivais/concursos | cidade viva e metas | adaptar para feiras de colheita, animais, pesca e lua |
| Espantalhos/proteção | risco leve no campo | avaliar como proteção contra pragas/criaturas, sem excesso de punição |
| Silos/feno | animais e inverno | incluir armazenamento de ração/feno |
| Melhorias de ferramentas | progressão material | ferramentas melhores reduzem custo, aumentam área e eficiência |
| Mina/caverna como fonte mineral | loop farm → cave → upgrade | manter mineração principal na caverna |

Regra de adaptação: usar apenas a função sistêmica. Não copiar apresentação, nomes, layout visual, fórmula exata ou identidade.

---

# PARTE J — Construções e produção

## 45. Construções essenciais

| Construção | Função |
|---|---|
| Casa | dormir, salvar, cozinhar, storage, calendário |
| Fonte de Anya | respec, companions, lore, Água Viva, eventos |
| SellPoint/Caixa de envio | venda rápida |
| Entrada da caverna | preparação e acesso |
| Baú inicial | storage básico |
| Área de pet | descanso, alimentação e vínculo |

## 46. Construções produtivas

| Construção | Função | Roadmap |
|---|---|---:|
| Pasto | animais grandes | 2 |
| Galinheiro | aves e ovos | 3 |
| Estufa | crops fora de estação | 3 |
| Pomar comum | árvores frutíferas comuns/especiais | 4 |
| Área arcana da Fonte | Água Viva, purificação, Raiz Dormente de Mana | 5 |
| Pedreira final | mineral tardio limitado | 5 |

## 47. Workshops e processadores

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
| Destilador arcano | Água Viva/reagentes raros | essência mágica |

Regra: processadores devem ser derivados de workshops, não objetos soltos sem sistema.

## 48. Tecnologia bromeciana na fazenda

Bromécia pode aparecer em upgrades tardios, sempre com custo/risco.

Exemplos:

- irrigação rúnica;
- estufa com vidro arcano;
- runas contra pragas;
- depósito auto-organizável;
- estabilizador de Água Viva;
- fundação auto-reparável limitada;
- pedreira final sobre ruína antiga;
- mecanismos de checkpoint/portal com Elyndor.

Regra:

```text
Bromécia não é upgrade grátis.
Todo poder bromeciano deve ter risco, custo, manutenção, instabilidade ou ambiguidade.
```

---

# PARTE K — Plantio, solo, clima, qualidade e fertilizante

## 49. Estados de solo

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
AnyaBlessedSoil
BromecianRunedSoil
```

## 50. Regras finais de solo e rega

```text
Planta molhada cresce.
Planta seca não cresce.
Planta seca acumula DaysWithoutWater.
Com 3 dias sem água, vira Dead.
Chuva molha áreas externas.
Estufa pode usar irrigação própria.
Irrigação automatizada entra por construção/upgrade.
Água Viva não é usada para rega comum.
```

## 51. Ferramentas agrícolas

| Ferramenta | Função | Evolução |
|---|---|---|
| Hoe | arar solo | área maior por upgrade |
| Watering Can | molhar solo | mais carga/área por upgrade |
| Axe | cortar árvores | mais dano/yield |
| Fishing Rod | pescar | peixes raros/timing melhor |
| Sickle | limpar planta morta/fibra | área maior |
| Build Tool | mover/construir | modo edição |
| Fertilizer Applicator | aplicar fertilizante em área | avançado |

## 52. Clima, seasons e luas

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

- Alihana pode favorecer crops noturnas, Água Viva, sonhos, pistas de Anya e Mana;
- Senya pode intensificar crops caóticas, reagentes mágicos, mutações e festivais;
- Nyx pode favorecer crops sombrias, segredos, redução situacional de cansaço noturno e reações do gato.

## 53. Thandra e Anya na fazenda

Separação de domínios:

```text
Thandra:
  colheitas comuns, animais pacatos, estações, fertilidade rural, ritos agrícolas.

Anya:
  Fonte, cura, Água Viva, Mana, renascimento, esperança e mistério central.
```

Isso evita banalizar Anya como deusa genérica da lavoura.

## 54. Qualidade

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
- crops mágicas não-Mana;
- eventos sazonais;
- solo arcano;
- bênção de Thandra;
- evento raro da Fonte.

Representação técnica preferencial:

```text
ItemId + Quality
```

## 55. Fertilizantes

| Tier | Tipo | Função |
|---:|---|---|
| 1 | Simples | crescimento ou qualidade leve |
| 2 | Rico | yield ou qualidade média |
| 3 | Refinado | qualidade alta |
| 4 | Arcano/Lunar | crops mágicas, eventos de lua, preparação para rituais raros |

Fontes:

- animais;
- compostagem;
- alquimia;
- caverna;
- NPCs;
- quests;
- Thandra/rituais rurais;
- Fonte/área arcana.

Regras:

- um fertilizante por plot/ciclo;
- se a planta morrer, o fertilizante é perdido;
- fertilizante comum não produz Mana;
- fertilizante arcano/lunar não garante Mana;
- fertilizante avançado não substitui cuidado diário.

---

# PARTE L — Mana

## 56. Natureza do Fruto Mana

Existe em Vaalara um fruto chamado **Mana**.

Características:

- raro;
- sagrado;
- politicamente perigoso;
- ligado a energia mágica;
- parecido com pêssego roxo na direção visual atual do jogo;
- quando consumido por humanos, concede temporariamente status mais fortes;
- pode curar ferimentos ou alimentar poções raras;
- não é crop comum;
- não é commodity agrícola.

## 57. Mana não é plantável como seed comum

Regra canônica:

```text
O jogador não compra semente de Mana.
O jogador não planta Mana como crop comum.
O jogador não transforma Mana em produção infinita.
```

A Árvore de Mana é consciente e escolhe onde crescer.

Transplante comum falha.

A fazenda só pode se conectar a Mana por evento de endgame/lore.

## 58. Raiz Dormente de Mana

Opção recomendada para o jogo:

```text
Raiz Dormente de Mana
  -> descoberta na fazenda, caverna ou ruína ligada à Fonte
  -> parece morta ou mineralizada por muito tempo
  -> reage a Água Viva, Alihana, Fonte e solo arcano
  -> não pode ser movida depois de descoberta
  -> pode falhar em florescer sem morrer
  -> não gera safra comum anual sem consequência
```

A Raiz Dormente permite ao jogo ter Mana sem quebrar o canon.

## 59. Condições candidatas de florescimento

- fazenda em nível 5;
- Fonte de Anya despertada ou fortalecida;
- Água Viva disponível;
- área arcana da Fonte construída;
- fertilizante lunar/arcano;
- Alihana ativa para florescimento;
- Nyx ativa para mutação rara;
- estação específica;
- skill agrícola/mágica suficiente;
- companion com afinidade alta ajudando;
- gato com vínculo alto sinalizando evento lunar raro;
- marco avançado da caverna ou nível 100/101.

A spec futura deve escolher combinação balanceada.

## 60. Usos do Fruto Mana

| Uso | Efeito |
|---|---|
| Consumo direto | buff temporário em atributos |
| Poção | cura forte ou recuperação de MP |
| Receita culinária | buff misto de HP/stamina/status |
| Alquimia | ingrediente de elixir raro |
| Fonte de Anya | upgrade/purificação |
| Quest | item solicitado por NPCs específicos |
| Venda | valor altíssimo, mas moral/economia devem pesar |
| Endgame | requisito para enfrentar áreas profundas ou rituais |

Fruto Mana Lunar deve ser raríssimo.

## 61. Nomenclatura: Mana vs Pontos de Magia

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

---

# PARTE M — Animais

## 62. Animais planejados

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
- morte deve ser tratada com tom simples e recuperável quando possível;
- Thandra pode aparecer como referência cultural de cuidado animal e ritos rurais.

---

# PARTE N — Companion jobs na fazenda

## 63. Jobs

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
- gastar Água Viva sem autorização;
- gastar fertilizante raro sem autorização;
- vender item raro automaticamente;
- desbloquear progressão sem decisão do jogador;
- substituir exploração da caverna.

---

# PARTE O — Economia agrícola

## 64. Canais de venda

| Canal | Característica |
|---|---|
| Caixa de envio | conveniência, pagamento no dia seguinte |
| Loja da cidade | preço melhor e reputação |
| Encomenda | preço alto e prazo |
| Festival/Concurso | prêmio e reputação |
| Craft/processamento | maior margem com tempo |
| Contrato especial | demanda rara ligada a guilda, templo ou nobreza |

A fazenda deve incentivar processar antes de vender.

Exemplos:

```text
Trigo -> Farinha -> Pão/Receita
Leite -> Queijo
Ovos -> Maionese
Fruta -> Suco/Fermentado
Erva -> Extrato/Poção
Água Viva -> Essência/Purificação
Mana -> Elixir/Essência rara
```

Regra: processamento aumenta valor, mas consome tempo, construção e planejamento.

## 65. Economia de Mana

Mana não deve ser vendido como item agrícola comum.

Regras:

- venda de Mana deve ser evento importante;
- preço deve ser altíssimo;
- venda pode afetar reputação, política ou facções;
- pode atrair interesse de mercadores, cultos, nobres ou criminosos;
- usar Mana pode ser melhor que vender;
- vender Mana demais pode quebrar balanceamento e lore.

---

# PARTE P — Save e dados futuros

## 66. FarmSaveData

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
  LivingWaterState
  IrrigationNetwork
  QuarryState
  ManaRootState
```

## 67. PlotSaveData

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

## 68. ManaRootSaveData

```text
ManaRootInstanceId
DiscoveryState
DormancyState
BloomAttempts
LastBloomDay
LunarCharge
LivingWaterAppliedCount
FertilizerId
SourceWaterState
QualityBias
IsAwakened
IsProtected
```

## 69. FountainSaveData

```text
FountainState
LivingWaterCharges
LastLivingWaterRechargeDay
PurificationLevel
CorruptionLevel
UnlockedFunctions[]
```

## 70. AnimalSaveData

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

## 71. PetSaveData

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
CombatAvailabilityState
```

## 72. BuildingSaveData

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
CanMove
LastMovedDay
```

## 73. FarmHouseSaveData

```text
BuildingInstanceId
Position
DoorPosition
PlayerSpawnPosition
HouseLevel
InternalLayoutId
```

## 74. ShippingBinSaveData

```text
BuildingInstanceId
Position
PendingItems[]
LastShipmentDay
IsMovable
```

## 75. FatigueSaveData

```text
CurrentFatigue
LastUpdatedGameTime
LastSleepDay
LastSleepHour
SleepQualityModifier
TemporaryFatigueModifiers[]
```

## 76. PlayerConditionSaveData

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

# PARTE Q — Specs futuras derivadas

## 77. Ordem recomendada

1. `spec_farm_layout_expansion_zones_free_build.md`
2. `spec_player_condition_fatigue_sleep_hunger_stamina.md`
3. `spec_farm_buildings_construction_workshops_storage.md`
4. `spec_farm_crop_death_quality_fertilizers.md`
5. `spec_farm_weather_rain_irrigation_automation.md`
6. `spec_farm_shipping_bin_orders_processing.md`
7. `spec_farm_animals_pasture_products_care.md`
8. `spec_farm_pets_dog_cat_bond_buffs.md`
9. `spec_farm_companion_jobs_automation.md`
10. `spec_farm_fonte_anya_living_water.md`
11. `spec_farm_mana_root_arcane_soil_endgame.md`
12. `spec_farm_final_quarry_late_game_resources.md`

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

# PARTE R — Relação com outros design directions

| Documento futuro | Relação |
|---|---|
| Lore/Vaalara | canon de Mana, Anya, Nymirianos, Elyndor, Bromécia e Pedras Negras |
| Cidade/NPCs | encomendas, compras, animais, pets, visitas, reputação, Thandra, Finan e Merithus |
| Caverna | minerais, reagentes, Pedra Preta, desbloqueio de pedreira, cansaço na exploração, Água Viva |
| Progressão/Magia | Pontos de Magia, buffs, skills agrícolas/mágicas, cansaço, Mana |
| Companions | jobs, afinidade, automação, relação com pets |
| Crafting | processadores, fertilizantes, produtos, tecnologia bromeciana |
| UI/UX | menus, hotbar, storage, shipping, layout, modo construção, status bars |

---

# PARTE S — Decisões fechadas

## 78. Decisões fechadas neste documento

```text
Cansaço:
  Sistema próprio.
  Afeta e é refletido pelo PlayerConditionManager.
  Não pertence ao HungerManager.

Pets:
  Sistema próprio.
  Não são subtipo de companion.

Cachorro:
  Pet separado.
  Não ocupa slot de companion em combate/caverna.

Casa e SellPoint:
  Podem ser movidos pelo jogador após desbloqueio do modo construção.

Pedreira:
  Apenas no nível final da fazenda.
  Não substitui mineração da caverna.

Mana:
  Fruto raro, sagrado e perigoso.
  Não é crop comum.
  Não é plantável via seed comprável.
  Árvore de Mana é consciente e escolhe onde crescer.
  Raiz Dormente de Mana é a opção recomendada para endgame.

Anya:
  Fonte e Água Viva concentram a camada de cura, esperança e renascimento.

Thandra:
  Sustenta agricultura comum, animais pacatos, estações e ritos rurais.

Bromécia:
  Fornece tecnologia agrícola tardia, sempre com custo/risco.

Pedras Negras:
  Pedra de Meteoro Negra Estabilizada é diferente de Pedra Negra Cultista/Corrompida.
```

---

## 78.5 UI/HUD agrícola

Prompts, HUD agrícola, feedback visual, inventory/storage UI, shipping/sell feedback, notifications e Fonte UI são definidos em:

```text
docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
```

---

## 79. Pendências restantes

### Produto

- Qual é o nome oficial do nível máximo da fazenda?
- A Fonte existe ativa desde o começo ou começa dormente?
- Fazenda nível 5 exige qual marco da caverna?
- Raiz Dormente de Mana fica na fazenda desde o início ou é descoberta?
- Mana floresce com Alihana, Nyx ou condição própria?
- Fruto Mana pode ser vendido livremente ou deve ser protegido por lore/raridade?
- Animais podem morrer permanentemente ou entram em estado recuperável?
- Gato pode gerar bônus lunar ligado a Nyx/Alihana?
- Caixa de envio paga na manhã seguinte ou instantâneo?
- Processadores puxam itens do inventário, baús ou depósito central?
- Companion pode usar fertilizante raro automaticamente?
- Chuva pode falhar em eventos de seca?
- Pedreira final usa companion Minerador?
- Água Viva recarrega por dias, lua, Fonte ou fragmento de Anya?

### Técnicas

- Qualidade será metadado no stack ou instância individual?
- `ItemId + Quality` é suficiente para todos os produtos?
- Fruto Mana precisa de item instanciado único?
- Save de irrigação será por plot ou rede de tiles?
- Expansão usa tilemap revelado ou cenas/áreas separadas?
- Pedreira final usa resource nodes persistentes ou geração diária simples?
- PetSystem compartilha infraestrutura de movement/follow/combat com companion ou implementa própria camada?
- FatigueSystem publica eventos próprios ou apenas `PlayerConditionChangedEvent`?
- FountainSystem e LivingWaterSystem são um sistema único ou separados?

---

# PARTE T — Próximos documentos recomendados

A partir deste design direction, os próximos documentos devem ser:

```text
docs/design/gameplay/farm/FARM_LAYOUT_EXPANSION_ZONES_DIRECTION_v1.0.md
docs/design/gameplay/farm/FARM_CROPS_QUALITY_FERTILIZERS_DIRECTION_v1.0.md
docs/design/gameplay/farm/FARM_FONTE_ANYA_LIVING_WATER_DIRECTION_v1.0.md
docs/design/gameplay/farm/FARM_MANA_ROOT_DIRECTION_v1.0.md
docs/design/gameplay/farm/FARM_PETS_DIRECTION_v1.0.md
docs/design/gameplay/combat_magic_progression/COMBAT_MAGIC_PROGRESSION_DESIGN_DIRECTION_v1.0.md
```

Depois de consolidados, cada um deve alimentar specs menores em `docs/specs/a_implementar/`.
