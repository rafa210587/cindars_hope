# Cindar's Hope — City Design Direction v1.1

> **Status:** direção ativa de gameplay/sistema  
> **Local:** `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.1.md`  
> **Substitui como direção ativa:** `CITY_DESIGN_DIRECTION_v1.0.md`  
> **Base de canon obrigatória:** `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`  
> **Documento relacionado:** `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.0.md`  
> **Função:** grande descrição de como a cidade de Cindar's Hope deve funcionar no jogo.  
> **Não é spec implementável.** Specs futuras devem ser quebradas em `docs/specs/a_implementar/`.

---

## 0. Alterações da v1.1

Esta versão ajusta a leitura religiosa e social da cidade:

- o templo público principal da cidade é de **Kanthor**;
- Anya existe apenas como memória obscura, passagem antiga, estátuas gastas e menções indiretas;
- poucos cidadãos sabem qualquer coisa real sobre Anya;
- Thandra mantém importância rural e pode ter festival, ritos e pequenos altares sazonais;
- outros deuses continuam podendo aparecer por símbolos, NPCs, festivais, lojas, rumores ou altares menores;
- o jogador pode erguer um altar para qualquer deus relevante, recebendo bônus diferentes;
- cidadãos podem visitar a fazenda conforme reputação, relacionamento, quests e eventos;
- o roster de cidadãos passa a ser documento próprio, com raça, subraça, classe, função, background, relações e tramas.

---

# PARTE A — Papel da cidade

## 1. Identidade da cidade

Cindar's Hope é uma vila/cidade rural de Dornécia, importante regionalmente, mas não uma metrópole.

Ela funciona como:

1. **Hub econômico:** lojas, venda, compra, upgrades, contratos, encomendas e licenças.
2. **Hub social:** NPCs, reputação, amizades, visitas à fazenda, companions e rumores.
3. **Hub narrativo:** Kanthor como ordem pública; Cindar e Anya como memória escondida; Bromécia/Elyndor/cultos como segredos subterrâneos.
4. **Hub de progressão:** desbloqueio de ferramentas, construções, animais, quests, preparação de caverna e serviços.
5. **Hub cultural de Dornécia:** religião pública, guildas, comércio, festivais, burocracia e autonomia local.
6. **Contraponto da caverna:** superfície segura e cotidiana acima de um subsolo perigoso.

## 2. Regra principal

A cidade deve parecer viva mesmo quando o jogador não está fazendo quest.

Ela precisa ter:

- horários;
- rotinas;
- relações entre NPCs;
- pequenas mudanças por dia, estação e lua;
- serviços úteis;
- rumores;
- festivais;
- visitas à fazenda;
- respostas ao progresso do jogador;
- segredos que só aparecem depois.

A cidade não deve ser apenas uma coleção de lojas estáticas.

---

# PARTE B — Tom e canon de Vaalara

## 3. Regra de tom: pastoral acima, horror abaixo

Cindar's Hope começa como cidade pastoral:

- praça;
- mercado;
- ferreiro;
- carpinteiro;
- taverna;
- templo de Kanthor;
- pequenos altares rurais;
- casas;
- animais;
- músicos;
- festivais;
- moradores com problemas simples.

Aos poucos, revela:

- estátuas antigas de Anya;
- inscrições apagadas sobre Cindar;
- ruínas sob a cidade;
- documentos escondidos;
- cultos discretos;
- Pedras Negras corrompidas;
- tecnologia bromeciana;
- portais de Elyndor;
- NPCs que sabem mais do que dizem.

## 4. Recorte político

A cidade fica em Dornécia, mas não depende da capital em tempo real.

Regras:

- autoridade local importa mais que autoridade distante;
- ordens da coroa demoram;
- o templo de Kanthor sustenta a ideia pública de lei e ordem;
- guildas, comerciantes e famílias locais têm poder prático;
- problemas pequenos não são resolvidos por exércitos;
- cultos podem operar nas bordas sem controle central;
- a economia local depende de fazendas, rotas, mineração indireta e caverna.

## 5. Origem do nome Cindar's Hope

A cidade carrega o nome de Cindar por causa de uma esperança antiga ligada a Anya.

Formulação de direção:

```text
Cindar's Hope foi renomeada ou preservada ao redor da esperança deixada por Cindar: recuperar, libertar ou proteger um fragmento do poder de Anya.
```

No início, a maioria dos moradores conhece apenas versões folclóricas.

Poucos NPCs sabem que o nome também funciona como aviso.

---

# PARTE C — Estrutura urbana

## 6. Escala da cidade

A cidade deve ser compacta, memorizável e funcional.

Direção:

```text
Cidade compacta, com 5 a 7 zonas principais.
Cada zona precisa ter função clara.
```

Evitar deslocamento longo demais para ações diárias.

## 7. Zonas principais

| Zona | Função |
|---|---|
| Praça Central | ponto de chegada, calendário, festivais, encontros, quadro público e rumores |
| Mercado / Rua Comercial | sementes, loja geral, animais, comida, itens comuns e encomendas |
| Distrito de Ofícios | ferreiro, carpintaria, alquimia, costura, upgrades e construção |
| Taverna / Estalagem | comida, descanso, rumores, música, contratos simples e socialização |
| Templo de Kanthor | lei, proteção, justiça, cerimônias, disputas, bênçãos de ordem |
| Jardim das Estátuas Antigas | estátuas gastas de Anya e Cindar; passagem antiga pouco compreendida |
| Prefeitura / Cartório / Guilda | contratos, licenças, reputação, impostos, construção, registros |
| Casas dos NPCs | rotina, visitas, relações e quests pessoais |
| Caminho da Fazenda | ligação cidade ↔ fazenda |
| Estrada / Caravançará | mercadores, Finan, Guilda das Estradas e visitantes raros |
| Entrada / Advertência da Caverna | preparação, alertas, missões, boatos e Guilda |
| Zona Noturna / Beco / Loja Oculta | Nyx, segredos, rumores e itens raros |
| Ruína discreta / Poço / Subsolo | conexão gradual com Bromécia, Elyndor, Anya e cultos |

## 8. Layout conceitual

```text
[ Estrada / Caravançará ]
          |
[ Mercado ] --- [ Praça Central ] --- [ Templo de Kanthor ]
     |                |                        |
[ Ofícios ]      [ Taverna ]        [ Jardim das Estátuas ]
     |                |                        |
[ Caminho da Fazenda ] --- [ Entrada/Trilha da Caverna ] --- [ Ruína discreta ]
```

Regras:

- Praça Central deve ser fácil de encontrar.
- Mercado e ofícios devem ficar próximos para reduzir atrito de rotina.
- Taverna deve funcionar como hub de rumores.
- Templo de Kanthor deve ser visualmente público e institucional.
- Jardim das Estátuas deve parecer antigo, silencioso e parcialmente esquecido.
- Zona noturna deve ser opcional e descoberta por rotina/rumores.
- Ruínas não devem dominar visualmente a cidade no início.

---

# PARTE D — Referências de farm sims sem copiar identidade

## 9. Aprendizados úteis

| Mecânica comum em farm sims | Valor para Cindar's Hope | Adaptação recomendada |
|---|---|---|
| Lojas com horário | cria rotina e planejamento | cada loja tem horário e dias fechados simples |
| NPCs com agenda | cidade parece viva | rotinas leves por dia/período/lua |
| Presentes/favoritos | cria vínculo | usar reputação individual, mas evitar grind excessivo |
| Quests pequenas | tutorial e progressão social | encomendas, pedidos, rumores e favores pessoais |
| Festivais | metas e identidade cultural | festas de colheita, pesca, animais, Senya, Alihana, Thandra e Kanthor |
| Loja/mercador raro | surpresa e economia | caravanas de Finan e loja noturna de Nyx |
| Centro comunitário/upgrades | meta de longo prazo | reconstruções locais, guilda, praça, estrada, poço e jardim antigo |
| Profissões claras | memória fácil dos NPCs | cada NPC deve ter papel mecânico + personalidade |
| Segredos da cidade | retenção narrativa | ruínas, cultos, documentos, portas, poço, subsolo |
| Romance/família | vínculo social | não definir agora; avaliar depois se combina com escopo |
| Concursos | recompensa por qualidade | crops, animais, pesca, culinária, artesanato |
| Calendário | estrutura de tempo | eventos por estação, lua e dia da semana |
| Bulletin board | objetivos curtos | quadro de pedidos e contratos |

Regra de adaptação: usar apenas funções sistêmicas adaptadas a Vaalara. Não copiar personagens, mapa, eventos específicos, UI ou fórmula de amizade de outros jogos.

---

# PARTE E — Religião, templo e altares

## 10. Templo principal: Kanthor

O templo público principal de Cindar's Hope é dedicado a **Kanthor**.

Funções narrativas:

- representa lei, justiça e ordem;
- legitima autoridade local;
- media disputas entre cidadãos;
- protege contratos e juramentos;
- dá sensação de segurança pública;
- contrasta com os segredos escondidos sob a cidade.

Funções mecânicas possíveis:

- bênção de proteção;
- redução leve de penalidade em morte/queda, se aprovado em spec;
- juramentos/contratos de quest;
- resolução de disputas;
- reputação com guardas/cidadãos ordeiros;
- quests contra cultos ou corrupção.

## 11. Anya na cidade

Anya não tem templo ativo conhecido pela população.

Ela aparece apenas por:

- estátuas antigas;
- nomes apagados;
- passagens fragmentadas;
- símbolos de água/esperança;
- menções confusas em registros;
- falas de poucos NPCs sensíveis;
- Jardim das Estátuas Antigas;
- ruínas/subsolo;
- relação indireta com Cindar.

Regra:

```text
Ninguém na cidade comum sabe muito sobre Anya.
Anya é uma ausência, não uma religião pública funcional.
```

A descoberta de Anya deve ser lenta e conectada a Fonte, Cindar, caverna, Nymirianos e Água Viva.

## 12. Thandra

Thandra não domina a cidade politicamente, mas é forte no cotidiano rural.

Ela aparece em:

- festival de colheita;
- bênçãos de estação;
- cuidado animal;
- pequenos altares rurais;
- loja de sementes;
- falas de camponeses;
- concursos agrícolas.

## 13. Outros deuses

Outros deuses podem aparecer por símbolos, NPCs, eventos, lojas ou altares menores.

| Deus | Presença urbana possível |
|---|---|
| Finan | caravanas, sorte, mercadores raros, variação de preço |
| Merithus/Meritos | cartório, contratos, licenças, impostos, caixa de envio |
| Thoren | ferreiro, ferramentas, armas, metalurgia |
| Nyx | loja noturna, segredos, memória, morte, cultos |
| Senya | festas, magia, emoção, caos social |
| Alihana | sonhos, música, profecias, pistas e eventos silenciosos |
| Kaand | conflitos, brigas, força, ameaças de violência |
| Tandra/Telisandra | natureza selvagem, caça, bestas, floresta |

## 14. Sistema de altares do jogador

O jogador pode erguer um altar dedicado a qualquer um dos deuses relevantes.

Local possível:

- fazenda;
- cidade, mediante licença;
- área de devoção pública;
- casa/fazenda como altar privado;
- reconstrução futura de espaço sagrado antigo.

Regra:

```text
Templo público principal = Kanthor.
Altares do jogador = escolha customizável com bônus diferentes.
```

### Bônus sugeridos por altar

| Deus | Bônus possível |
|---|---|
| Kanthor | proteção, estabilidade, defesa, reputação com ordem/guardas |
| Anya | cura, Água Viva, ressurreição/recuperação, eventos raros; desbloqueio tardio |
| Thandra | crops, animais, fertilidade, qualidade agrícola |
| Finan | sorte, achados, preços, mercadores raros |
| Merithus/Meritos | contratos melhores, taxa menor, caixa de envio, licenças |
| Thoren | ferramenta, forja, upgrade, durabilidade |
| Alihana | sonhos, sementes raras, pistas de lore, eventos de Fonte |
| Senya | magia, mutação, festivais, buffs temporários |
| Nyx | segredos, loja noturna, stealth, redução situacional de cansaço noturno |
| Tandra/Telisandra | madeira, caça, animais selvagens, resistência natural |
| Kaand | dano, força, risco/recompensa em combate |

Regras:

- altar deve exigir construção, recurso e licença ou ritual;
- bônus não deve ser permanente sem custo/limite;
- alguns altares podem exigir reputação ou descoberta de lore;
- altar de Anya não deve estar disponível cedo sem descoberta narrativa;
- erguer altar pode afetar reputação com certos NPCs.

---

# PARTE F — NPCs, raças e classes

## 15. Documento de roster

O elenco detalhado de cidadãos fica em:

```text
docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.0.md
```

Este documento de cidade define regras e sistemas. O roster define indivíduos.

## 16. Regra de NPC

Cada NPC principal deve ter:

- nome;
- raça;
- subraça/origem cultural quando aplicável;
- classe/arquétipo RPG;
- profissão;
- serviço ou função clara;
- rotina simples;
- local de trabalho;
- casa ou local de descanso;
- relação com pelo menos 2 outros NPCs;
- relação com uma camada do mundo: fazenda, cidade, caverna, religião, comércio, guilda, lore ou lua;
- reputação individual;
- possível visita à fazenda;
- pelo menos 1 quest/favor pessoal;
- uma pequena tensão ou segredo.

## 17. Quantidade alvo

A cidade completa deve trabalhar com:

```text
20 a 28 cidadãos principais
```

Esse número permite diversidade de raças, serviços e relações sem virar cidade grande demais.

---

# PARTE G — Visitas à fazenda

## 18. Regra geral

Cidadãos podem visitar a fazenda conforme:

- reputação geral da cidade;
- relacionamento individual;
- quest ativa;
- serviço contratado;
- festival;
- horário;
- estação;
- lua ativa;
- progresso na caverna;
- evento de companion;
- nível da fazenda.

## 19. Tipos de visita

| Tipo | Exemplo |
|---|---|
| Serviço | carpinteiro inspeciona obra, criador entrega animal |
| Social | NPC amigo visita, comenta layout, traz presente simples |
| Comércio | mercador ambulante, sementes raras, encomenda |
| Quest | pedido pessoal, alerta, evento de relação |
| Lore | NPC percebe algo na Fonte, estátua, Mana ou sonho |
| Emergência | aviso de cultos, caverna, sumiço ou ameaça |
| Festival | convite, entrega de prêmio, inspeção de concurso |

## 20. Regras de visita

- visita deve ter motivo;
- visita não deve atrapalhar controle do jogador;
- NPC não deve bloquear área produtiva;
- visita deve respeitar colisão/pathfinding;
- NPC deve ir embora em horário lógico;
- visita pode gerar diálogo, presente, quest, inspeção ou serviço;
- visita deve ser persistível apenas quando necessário.

## 21. Gatilhos sugeridos

```text
NpcRelationship >= 25:
  pode comentar fazenda ou enviar carta.

NpcRelationship >= 50:
  pode visitar em evento simples.

NpcRelationship >= 75:
  pode ajudar, entregar item, oferecer serviço especial ou iniciar quest pessoal.

TownReputation alta:
  aumenta chance de visitas positivas.

CultActivity alta:
  aumenta chance de visita de alerta/investigação.
```

---

# PARTE H — Serviços da cidade

## 22. Lojas e serviços mínimos

| Serviço | O que vende/faz | Progressão vinculada |
|---|---|---|
| Loja de sementes | seeds, fertilizante simples, calendário agrícola | farm/crops/Thandra |
| Loja geral | comida, itens básicos, ração, ferramentas simples | rotina |
| Ferreiro | upgrade de ferramenta, armas, ingots, reparo | caverna/combate/Thoren |
| Carpintaria | construções, mover casa/SellPoint, upgrades | fazenda/layout/Merithus |
| Alquimia | poções, buffs, reagentes, fertilizantes raros | caverna/Anya/Mana |
| Criador de animais | animais, ração, remédios, produtos | fazenda/animais/Thandra |
| Costura | bolsas, roupas, capas, acessórios | inventário/status |
| Taverna | comida, descanso, rumores, quadro de pedidos | social/quests |
| Prefeitura/cartório | contratos, licenças, impostos, reputação | Merithus/economia |
| Templo de Kanthor | ordem, juramentos, proteção, disputas | Kanthor/cidade |
| Jardim das Estátuas | lore indireta, Anya/Cindar, eventos raros | Anya/Cindar |
| Guilda das Estradas | cave prep, mapas, checkpoints, caravanas | caverna/Elyndor/Finan |
| Loja noturna | itens raros, segredos, rumores | Nyx |

## 23. Lojas com horário

Cada loja deve ter:

```text
OpenHour
CloseHour
ClosedDays[]
SpecialMoonBehavior
ReputationDiscountRules
StockRules
```

Regras:

- horários devem criar rotina, não frustração extrema;
- serviços essenciais devem ter alternativas ou janelas amplas;
- loja noturna pode abrir apenas sob condições;
- festivais podem fechar lojas, mas compensar com serviços/eventos especiais.

---

# PARTE I — Reputação, amizade e cidade viva

## 24. Sistemas sociais

Separar dois sistemas:

```text
TownReputation
  reputação geral da cidade com o jogador.

NpcRelationship
  relação individual com cada NPC.
```

## 25. TownReputation

Representa como Cindar's Hope vê o jogador.

Sobe com:

- vender produção localmente;
- completar contratos;
- ajudar NPCs;
- desbloquear melhorias;
- participar de festivais;
- derrotar ameaças;
- doar itens úteis;
- resolver problemas da cidade;
- respeitar contratos de Kanthor/Merithus.

Pode cair com:

- falhar contratos importantes;
- vender itens suspeitos;
- ignorar eventos urgentes;
- tomar decisões que prejudiquem moradores;
- mexer com cultos/ruínas sem cuidado;
- usar Mana de forma predatória publicamente.

Efeitos:

- descontos;
- contratos melhores;
- acesso a construção;
- convites para eventos;
- confiança sobre segredos;
- permissões da prefeitura/guilda;
- visitas mais frequentes à fazenda.

## 26. NpcRelationship

Relação individual com NPCs.

Faixas sugeridas:

| Faixa | Nome | Efeito |
|---:|---|---|
| -100 a -51 | Hostil | recusa ajuda, preços piores, diálogo frio |
| -50 a -1 | Desconfiado | sem quests pessoais, pouca informação |
| 0 a 24 | Neutro | serviços normais |
| 25 a 49 | Amigável | pequenos descontos, cartas, comentários e presentes simples |
| 50 a 74 | Confiável | quests pessoais, informações úteis, visitas ocasionais |
| 75 a 99 | Aliado | ajuda na fazenda/caverna/serviços especiais |
| 100 | Vínculo máximo | habilidade, evento ou bônus único |

## 27. Relação com companions

Nem todo NPC vira companion de combate.

Tipos de ajuda:

| Tipo | Efeito |
|---|---|
| Companion de Fazenda | ajuda com plantio, rega, colheita, animais |
| Companion de Caverna | acompanha em combate/exploração |
| Companion de Serviço | melhora loja, crafting, preço ou fila |
| Companion de Informação | revela atalhos, segredos, biomas, rumores |
| Companion de Evento | participa de quest/festival/ritual |

Regra:

```text
Todo NPC pode ter vínculo útil.
Nem todo NPC precisa lutar ou morar na fazenda.
```

---

# PARTE J — Rotina, quests, festivais e luas

## 28. Agenda dos NPCs

Cada NPC deve ter agenda simples baseada em:

```text
Dia da semana
Período do dia
Estação
Lua ativa
Evento/festival
Quest state
Reputação
```

Períodos básicos:

```text
Morning
Afternoon
Evening
Night
LateNight
```

## 29. Trilha inicial de quests

Primeiras quests devem ensinar:

1. visitar cidade;
2. conhecer templo de Kanthor e praça;
3. conhecer loja de sementes;
4. comprar ou receber sementes;
5. plantar/regar/colher;
6. vender no SellPoint ou loja;
7. conhecer carpinteiro;
8. conhecer taverna/quadro de pedidos;
9. conhecer ferreiro;
10. ouvir primeiro rumor sobre caverna;
11. ver estátuas antigas de Anya sem explicação completa;
12. receber primeira pista sobre Cindar;
13. desbloquear preparação básica para caverna.

## 30. Festivais recomendados

| Festival | Tema | Função |
|---|---|---|
| Juramento de Kanthor | ordem, justiça, comunidade | contratos públicos, reputação, defesa |
| Feira da Primeira Colheita | Thandra/agricultura | concurso de crop, sementes, reputação |
| Noite de Alihana | sonhos, música, mistério | pistas de Anya/Cindar, itens raros |
| Festa de Senya | caos, dança, magia | buffs, preços altos, minigames, mutações |
| Vigília de Nyx | segredos e memória | loja noturna, rumores, escolhas estranhas |
| Concurso de Animais | fazenda/Thandra | qualidade animal, afeto, ração |
| Torneio de Pesca | lago/rios | pesca, comida, comércio |
| Feira dos Ofícios | crafting | ferreiro, carpintaria, costura, alquimia |
| Dia dos Viajantes | Finan/Guilda | caravanas, mercadores raros, contratos |

## 31. Luas na cidade

| Lua | Efeito urbano |
|---|---|
| Alihana | sonhos, música, profecias, pistas, sementes raras, estátuas de Anya reagindo sutilmente |
| Senya | festas, magia, preços, humor dos NPCs, pequenos conflitos e crafting instável |
| Nyx | loja noturna, rumores, cultos, becos, memória, segredos e gato reagindo a algo oculto |

---

# PARTE K — Segredos, cultos e trama simples

## 32. Trama simples da cidade

A cidade parece protegida por Kanthor, mas sua ordem pública cobre três tensões:

```text
1. O nome Cindar's Hope é mais antigo do que os registros oficiais admitem.
2. O Jardim das Estátuas guarda símbolos de Anya que quase ninguém entende.
3. Um pequeno grupo tenta encontrar uma Pedra Negra corrompida escondida sob a cidade.
```

A trama deve começar simples:

- itens somem;
- um poço é lacrado;
- um NPC evita falar das estátuas;
- a loja noturna sabe demais;
- a Guilda das Estradas quer mapear o subsolo;
- o templo de Kanthor tenta manter calma pública;
- o jogador encontra pistas só se aumentar relação e explorar a caverna.

## 33. Cultos

Cultos não devem dominar a cidade cedo.

Eles aparecem por:

- rumores;
- símbolos;
- desaparecimento de itens;
- NPC nervoso;
- eventos de Nyx;
- missões de investigação;
- ligação gradual com caverna.

## 34. Bromécia e Elyndor sob a cidade

Bromécia pode aparecer como ruína sob fundações, mecanismo antigo ou porta que só abre com condição lunar.

Elyndor pode aparecer como mapa antigo, arco quebrado, pedra estabilizada ou símbolo nos checkpoints.

Regra:

```text
Todo poder bromeciano deve ter custo, risco ou ambiguidade.
Elyndor não deve virar fast travel comum cedo.
```

---

# PARTE L — Save e dados futuros

## 35. CitySaveData

```text
CitySaveData
  TownReputation
  UnlockedServices[]
  UnlockedBuildings[]
  CompletedCityUpgrades[]
  ActiveFestivals[]
  CityEventFlags[]
  SecretDiscoveryFlags[]
  GuildProgress
  CultActivityState
  PublicReligionState
  PlayerBuiltAltars[]
```

## 36. NpcSaveData

```text
NpcId
RelationshipScore
RelationshipTier
KnownByPlayer
CurrentScheduleState
CompletedPersonalQuests[]
ActivePersonalQuestId
IsAvailableAsCompanion
CompanionModeUnlocked
CanVisitFarm
LastFarmVisitDay
LastTalkedDay
GiftHistory[]
```

## 37. AltarSaveData

```text
AltarInstanceId
DeityId
Location
BuildState
BlessingState
LastUsedDay
ReputationImpactFlags[]
```

## 38. ShopSaveData

```text
ShopId
CurrentStock[]
UnlockedStockTiers[]
PriceModifiers[]
LastRestockDay
SpecialMoonStockState
```

## 39. ContractSaveData

```text
ContractId
SourceNpcOrBoardId
Type
RequiredItems[]
DeadlineDay
Reward
ReputationReward
Status
```

## 40. FestivalSaveData

```text
FestivalId
Season
Day
RequiredUnlocks[]
ParticipatedThisYear
BestScore
RewardsClaimed[]
```

---

# PARTE M — Specs futuras derivadas

## 41. Ordem recomendada

1. `spec_city_layout_zones_navigation.md`
2. `spec_city_npc_roster_services_schedule.md`
3. `spec_city_shops_stock_prices_reputation.md`
4. `spec_city_relationship_town_reputation_farm_visits.md`
5. `spec_city_quests_contracts_bulletin_board.md`
6. `spec_city_festivals_calendar_moon_events.md`
7. `spec_city_religion_kanthor_altars_deities.md`
8. `spec_city_lore_cindar_anya_statues.md`
9. `spec_city_guild_roads_cave_prep.md`
10. `spec_city_hidden_secrets_bromecia_elyndor.md`

---

# PARTE N — Relação com outros design directions

| Documento | Relação |
|---|---|
| `VAALARA_GAME_CANON_DIRECTION_v1.0.md` | canon obrigatório para cidade |
| `FARM_DESIGN_DIRECTION_v1.3.md` | sementes, animais, construções, encomendas, visitas |
| `CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.0.md` | cidadãos, raças, classes, serviços, relações e tramas |
| `CAVE_DESIGN_DIRECTION_v1.0.md` | guilda, contratos, rumores, cultos, materiais |
| `COMBAT_MAGIC_PROGRESSION_DESIGN_DIRECTION_v1.0.md` | reputação, carisma, MP, buffs, cansaço |
| `COMPANIONS_DESIGN_DIRECTION_v1.0.md` | NPCs recrutáveis, vínculos, jobs |
| `UI_UX_DESIGN_DIRECTION_v1.0.md` | lojas, diálogo, calendário, reputação, mapa |

---

# PARTE O — Decisões fechadas

## 42. Decisões deste documento

```text
Cidade é hub social, econômico, narrativo e de progressão.
Cidade deve ser pastoral na superfície e ter segredos abaixo.
Templo público principal é de Kanthor.
Anya não tem templo ativo conhecido; aparece em estátuas, passagens e memória obscura.
Thandra mantém força rural e pode ter festival próprio.
Jogador pode erguer altar para qualquer deus relevante, com bônus diferentes.
Cidadãos podem visitar a fazenda conforme reputação, relação, quests e eventos.
NPCs devem ter raça, subraça/origem, classe, função, rotina, relações e trama.
TownReputation e NpcRelationship são sistemas separados.
Nem todo NPC vira combatente; todo NPC pode ter vínculo útil.
Lojas têm horários, estoque variável e relação com reputação.
Festivais são parte importante da cultura local.
Luas afetam cidade, economia, NPCs e eventos.
Kanthor sustenta a face pública da cidade; Anya sustenta mistério profundo.
```

---

# PARTE P — Pendências

## 43. Pendências de produto

- Definir nome oficial das zonas da cidade.
- Definir mapa/layout final.
- Validar roster oficial de NPCs.
- Definir quantos NPCs podem virar companions de caverna.
- Definir quais NPCs ajudam só por serviço/fazenda/informação.
- Definir se existe romance/casamento ou não.
- Definir calendário de estações e tamanho do ano.
- Definir quais festivais entram primeiro.
- Definir bônus finais de cada altar.
- Definir custo/requisito para erguer altar.
- Definir se altar fica na fazenda, cidade ou ambos.
- Definir como a cidade reage ao nível de caverna alcançado.
- Definir se cultos aparecem como questline principal ou sidequest.
- Definir se há prefeito, conselho, magistrado ou guilda dominante.
- Definir se a cidade tem guarda formal ou milícia local.
- Definir onde fica a ligação física com ruínas/bromecia/poço/subsolo.

## 44. Pendências técnicas

- Agenda de NPC será por data/período/estado de quest ou behavior tree simples?
- Shop stock será recalculado por dia ou salvo por loja?
- Reputação será int simples ou sistema com tiers/event flags?
- Quadro de pedidos gera contratos proceduralmente ou lista curada?
- Festivais são cenas separadas ou variações da CityScene?
- NPCs visitando fazenda usam schedule real ou eventos instanciados?
- Map markers serão manuais ou derivados de registry de local?
- Lojas fechadas bloqueiam interação ou mostram UI reduzida?
- Altares usam sistema próprio ou BuildingSystem + BlessingSystem?

---

## 45. Próximo documento recomendado

O próximo documento de cidade é:

```text
docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.0.md
```

Ele consolida cidadãos, raças, subraças, classes, serviços, relações, visitas à fazenda e trama simples da cidade.
