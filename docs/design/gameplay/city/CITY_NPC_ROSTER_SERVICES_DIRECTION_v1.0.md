# Cindar's Hope — City NPC Roster & Services Direction v1.0

> **Status:** direção ativa de elenco, serviços e relações da cidade  
> **Local:** `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.0.md`  
> **Depende de:** `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.1.md`  
> **Canon obrigatório:** `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`  
> **Função:** consolidar cidadãos principais, raça, subraça/origem, classe/arquétipo, serviço, background, relações, visitas à fazenda e tramas simples.  
> **Não é spec implementável.** Specs futuras devem ser quebradas em `docs/specs/a_implementar/`.

---

## 1. Objetivo

Este documento define um roster inicial completo de cidadãos para Cindar's Hope.

Ele deve servir como base para:

- NPCs principais;
- serviços e lojas;
- agenda diária;
- reputação;
- quests pessoais;
- visitas à fazenda;
- companions;
- festivais;
- tramas locais;
- integração com fazenda, cidade e caverna.

Os nomes, classes e relações ainda podem ser ajustados antes de virar spec.

---

## 2. Regra de diversidade de Vaalara

Cindar's Hope é uma cidade rural de Dornécia, mas fica em rota de comércio e perto de uma caverna importante.

Por isso, a cidade pode ter:

- humanos de Dornécia;
- humanos de outras regiões de Vaalara;
- halflings;
- anões;
- tieflings;
- elfos;
- elfos da noite/Luandil, com cuidado de lore;
- goblins integrados ao comércio/ofícios;
- meio-orcs;
- draconatos;
- raças raras em baixa quantidade;
- Nymirianos apenas como mistério, ruína, sangue distante ou revelação tardia.

Regra:

```text
A cidade é diversa, mas não cosmopolita demais.
A maioria ainda deve parecer rural/local.
Raças raras precisam ter razão para estar ali.
```

---

## 3. Classes/arquetipos

A “classe” de cada cidadão deve ser entendida como arquétipo de RPG, não necessariamente como classe jogável plena.

Exemplos:

- Commoner;
- Expert;
- Merchant;
- Cleric;
- Paladin;
- Fighter;
- Ranger;
- Rogue;
- Bard;
- Artificer;
- Alchemist;
- Wizard;
- Druid;
- Monk;
- Barbarian;
- Warlock, apenas se houver justificativa narrativa;
- Sorcerer, raro.

Regra:

```text
Classe informa personalidade, serviço, quests e possível companion mode.
Não significa que todo NPC seja combatente.
```

---

# PARTE A — Roster principal sugerido

## 4. Resumo do roster

| ID | Nome | Raça/origem | Classe/arquétipo | Função |
|---|---|---|---|---|
| npc_corvus | Padre Corvus | Humano dornécio | Cleric/Paladin de Kanthor | templo principal, ordem, juramentos |
| npc_mara | Mara Vellum | Humana dornécia | Magistrate/Expert | cartório, licenças, reputação |
| npc_sylveth | Sylveth | Elfa rural | Druid/Expert | sementes, Thandra, crops |
| npc_brumdar | Brumdar Ferro-Quieto | Anão | Fighter/Smith | ferreiro, ferramentas, armas |
| npc_nimble | Nimble Galhobaixo | Halfling | Carpenter/Expert | carpintaria, construções |
| npc_gurd | Gurd Carvalho-Torto | Meio-orc | Laborer/Fighter | obras, madeira, força |
| npc_hund | Hund Carvalho-Torto | Meio-orc | Laborer/Guardian | obras, transporte, segurança |
| npc_ozzra | Ozzra Fumaçazul | Goblin | Alchemist/Artificer | alquimia, poções, fertilizantes |
| npc_gruta | Gruta Panela-Funda | Halfling | Bard/Cook | taverna, comida, rumores |
| npc_zrix | Zrix das Estradas | Draconato cobre | Ranger/Merchant | Guilda das Estradas, caverna |
| npc_yael | Yael Noite-Mansa | Tiefling | Rogue/Occult Trader | loja noturna, Nyx, segredos |
| npc_thalindra | Thalindra Véu-de-Lua | Elfa cinzenta | Wizard/Archivist | arquivo, Cindar, história |
| npc_dagna | Dagna Rocha-Morna | Anã | Miner/Expert | minérios, caverna, contratos |
| npc_pip | Pip Semente-Solta | Halfling jovem | Commoner/Scout | tutorial, entregas, humor |
| npc_ser_alaric | Ser Alaric Veyr | Humano | Fighter/Guard Captain | guarda, Kanthor, segurança |
| npc_mirela | Mirela dos Laços | Humana dornécia | Tailor/Expert | costura, bolsas, roupas |
| npc_renko | Renko Três-Sorrisos | Goblin | Merchant | loja geral, barganhas |
| npc_eiran | Eiran Valeclaro | Meio-elfo | Animal Handler/Ranger | animais, ração, pets |
| npc_liora | Liora Canta-Rio | Humana com sangue nymiriano distante | Bard/Seer | música, sonhos, Alihana |
| npc_orlan | Orlan Pouso-Curto | Humano dornécio | Innkeeper/Commoner | hospedagem, notícias |
| npc_savra | Savra Escama-Verde | Draconata verde | Herbalist/Ranger | ervas, floresta, riscos naturais |
| npc_tovin | Tovin Mãos-de-Selo | Gnomo ou halfling | Scribe/Expert | contratos, impostos, Merithus |
| npc_maelor | Maelor Cinza | Elfo da noite/Luandil | Monk/Rogue | estranho, memória de Nyx, trama oculta |
| npc_anya_statue | Estátuas de Anya | não NPC vivo | Lore Anchor | memória, investigação, mistério |

Total: 23 cidadãos/personagens principais + 1 âncora de lore.

---

# PARTE B — Cidadãos detalhados

## 5. Padre Corvus

```text
ID: npc_corvus
Raça: Humano dornécio
Classe/arquétipo: Cleric/Paladin de Kanthor
Função: sacerdote do templo principal
Local: Templo de Kanthor
Visita a fazenda: sim, em eventos de juramento, proteção ou alerta
Companion: não combatente regular; pode dar bênçãos/serviços
```

Background:

Corvus é a face pública da ordem em Cindar's Hope. Ele acredita que a cidade só sobreviveu porque manteve leis, pactos e juramentos mesmo quando a coroa estava distante.

Relações:

- respeita Mara, mas discorda da frieza burocrática dela;
- confia em Ser Alaric;
- desconfia de Yael;
- evita falar sobre as estátuas antigas de Anya;
- trata Liora com cautela por causa dos sonhos dela.

Trama simples:

Corvus sabe que o templo de Kanthor foi construído sobre fundações mais antigas. Ele não sabe tudo sobre Anya, mas sabe que mexer no passado pode abalar a ordem da cidade.

Quest pessoal:

- recuperar um sino quebrado do templo;
- investigar um juramento falso;
- decidir se revela ou oculta uma inscrição antiga encontrada sob o templo.

---

## 6. Mara Vellum

```text
ID: npc_mara
Raça: Humana dornécia
Classe/arquétipo: Magistrate/Expert
Função: cartório, licenças, reputação urbana, impostos
Local: Prefeitura/Cartório
Visita a fazenda: sim, para inspeção de construção/licença
Companion: serviço/informação
```

Background:

Mara administra permissões, registros, licenças e contratos. Ela é devota prática de Merithus/Meritos Tolerus e acredita que uma cidade sem registro vira presa fácil de mercadores, cultos e nobres distantes.

Relações:

- trabalha com Corvus em disputas formais;
- vive em tensão com Nimble e Gurd por obras não declaradas;
- respeita Tovin;
- acha Zrix útil, mas informal demais.

Trama simples:

Mara descobriu que alguns registros sobre a origem da cidade foram removidos do cartório. Ela suspeita de contrabando, não de lore antiga.

Quest pessoal:

- recuperar livros de registro;
- validar licença para altares do jogador;
- escolher entre burocracia rígida ou ajudar uma família pobre.

---

## 7. Sylveth

```text
ID: npc_sylveth
Raça: Elfa rural
Subraça/origem: elfa de comunidade agrícola de Dornécia
Classe/arquétipo: Druid/Expert
Função: sementes, crops, calendário agrícola, Thandra
Local: Loja de sementes / horta comunitária
Visita a fazenda: sim, quando relação cresce ou para quests agrícolas
Companion: fazenda/serviço, não caverna inicialmente
```

Background:

Sylveth conhece o ritmo das estações e mantém um pequeno altar de Thandra perto da loja. Para ela, a terra fala em padrões simples: chuva, lua, raiz e paciência.

Relações:

- amiga de Eiran;
- fornece ervas para Ozzra, mas teme os experimentos dela;
- respeita Thalindra, embora ache arquivo demais e terra de menos;
- gosta de Pip, que ajuda em entregas.

Trama simples:

Sylveth percebe que algumas plantas estão reagindo a algo vindo de baixo da cidade. Ela acha que é solo antigo, não culto.

Quest pessoal:

- recuperar sementes antigas;
- preparar festival de Thandra;
- investigar crop que cresce fora de estação perto das estátuas.

---

## 8. Brumdar Ferro-Quieto

```text
ID: npc_brumdar
Raça: Anão
Classe/arquétipo: Fighter/Smith
Função: ferreiro, ferramentas, armas, reparo
Local: Forja
Visita a fazenda: sim, para entrega de ferramenta/upgrade
Companion: pode ser mentor de combate/serviço, não companion regular cedo
```

Background:

Brumdar é um anão de poucas palavras. Veio para Cindar's Hope por causa da qualidade estranha do minério trazido da caverna. Ele não gosta de magia instável nem de metal sem procedência.

Relações:

- grande respeito por Dagna;
- rivalidade amigável com Ozzra;
- desconfia de Zrix quando ele traz materiais sem explicar origem;
- conversa pouco, mas protege Pip.

Trama simples:

Brumdar encontrou marcas bromecianas em um lingote antigo. Ele guarda isso porque não quer que a cidade entre em pânico.

Quest pessoal:

- testar minério de caverna;
- forjar ferramenta especial;
- decidir se destrói ou preserva metal de origem duvidosa.

---

## 9. Nimble Galhobaixo

```text
ID: npc_nimble
Raça: Halfling
Classe/arquétipo: Carpenter/Expert
Função: carpintaria, construções, móveis, modo construção
Local: Carpintaria
Visita a fazenda: sim, frequentemente por obras
Companion: serviço/fazenda
```

Background:

Nimble é pequeno, rápido e cheio de opiniões sobre onde cada cerca deveria ficar. Gosta de layout livre, mas odeia quando o jogador coloca construções bloqueando caminho.

Relações:

- trabalha com Hund & Gurd;
- briga com Mara por licenças;
- gosta de Gruta porque come de graça quando conserta a taverna;
- teme madeira marcada por runas antigas.

Trama simples:

Nimble encontrou madeira que se auto-repara perto da ruína discreta. Ele quer usar em construções, mas não entende o risco bromeciano.

Quest pessoal:

- liberar modo construção;
- mover SellPoint/casa;
- decidir se usa madeira antiga perigosa ou material comum.

---

## 10. Gurd e Hund Carvalho-Torto

```text
ID: npc_gurd / npc_hund
Raça: Meio-orcs
Classe/arquétipo: Laborer/Fighter e Laborer/Guardian
Função: obras pesadas, transporte, limpeza de expansão
Local: Carpintaria / canteiro de obras
Visita a fazenda: sim, para expansão e construção
Companion: Gurd pode virar fazenda/combate leve; Hund serviço/defesa
```

Background:

Irmãos meio-orcs, fortes e práticos. Gurd é impaciente e gosta de resolver tudo no braço. Hund é mais cuidadoso e observa o ambiente antes de agir.

Relações:

- trabalham para Nimble;
- respeitam Brumdar;
- Gurd gosta da taverna de Gruta;
- Hund desconfia de ruídos vindos do poço antigo.

Trama simples:

Durante obras, os irmãos encontraram uma parede que não deveria existir. Eles cobriram por medo de perder trabalho.

Quest pessoal:

- limpar expansão da fazenda;
- remover rochas grandes;
- investigar parede antiga sem alertar toda cidade.

---

## 11. Ozzra Fumaçazul

```text
ID: npc_ozzra
Raça: Goblin
Classe/arquétipo: Alchemist/Artificer
Função: alquimia, poções, fertilizantes avançados, reagentes
Local: Alquimia
Visita a fazenda: sim, para testar solo/reagentes, com autorização
Companion: serviço/crafting; caverna situacional
```

Background:

Ozzra é brilhante, caótica e perigosa na medida certa. Ela trata cada planta, peixe e cristal como possível explosão útil.

Relações:

- rivalidade técnica com Brumdar;
- compra ervas de Sylveth;
- deve dinheiro a Renko;
- acha Yael fascinante e suspeita.

Trama simples:

Ozzra sabe que alguns reagentes da caverna não são naturais. Ela pode ser a primeira a falar de “energia drenada”, sem entender ainda Pedra Negra corrompida.

Quest pessoal:

- criar fertilizante raro;
- testar poção de cansaço;
- investigar reação entre Água Viva e reagente subterrâneo.

---

## 12. Gruta Panela-Funda

```text
ID: npc_gruta
Raça: Halfling
Classe/arquétipo: Bard/Cook
Função: taverna, comida, descanso, rumores
Local: Taverna
Visita a fazenda: sim, para comprar ingredientes ou cozinhar em evento
Companion: social/serviço; pode dar buffs por comida
```

Background:

Gruta sabe mais do que parece porque todo mundo fala demais na taverna. Ela canta mal de propósito para as pessoas rirem e falarem mais.

Relações:

- amiga de Nimble;
- protege Pip;
- sabe quando Corvus está escondendo preocupação;
- troca rumores com Zrix.

Trama simples:

Gruta ouviu três versões conflitantes sobre uma estátua antiga que chorou água durante uma noite de Alihana.

Quest pessoal:

- criar prato para festival;
- pedir peixe raro;
- descobrir quem está deixando símbolos sob as mesas da taverna.

---

## 13. Zrix das Estradas

```text
ID: npc_zrix
Raça: Draconato cobre
Classe/arquétipo: Ranger/Merchant
Função: Guilda das Estradas, caverna, mapas, caravanas
Local: Guilda / Estrada / entrada da caverna
Visita a fazenda: sim, para contratos de exploração ou entrega de mapa
Companion: informação/caverna; pode virar companion de caverna
```

Background:

Zrix conhece rotas, atalhos e histórias de viajantes. Tem humor seco e fala como se já tivesse morrido duas vezes e achado inconveniente.

Relações:

- troca rumores com Gruta;
- respeita Ser Alaric, mas acha os guardas lentos;
- negocia com Renko;
- evita falar com Yael em público.

Trama simples:

Zrix sabe que alguns checkpoints antigos não são naturais. Ele reconhece símbolos de Elyndor em mapas antigos.

Quest pessoal:

- mapear entrada da caverna;
- recuperar placa de estrada antiga;
- identificar diferença entre Pedra Estabilizada e Pedra Corrompida.

---

## 14. Yael Noite-Mansa

```text
ID: npc_yael
Raça: Tiefling
Classe/arquétipo: Rogue/Occult Trader
Função: loja noturna, Nyx, rumores e itens raros
Local: beco/loja noturna
Visita a fazenda: sim, raramente, à noite, se relação alta
Companion: informação/segredos; caverna situacional
```

Background:

Yael vende coisas que “não existem” para pessoas que “não perguntaram”. Ela não é vilã por padrão, mas sabe se proteger.

Relações:

- Corvus desconfia dela;
- Ozzra quer estudar seus itens;
- Maelor a conhece de antes;
- Liora sente que Yael esconde medo real.

Trama simples:

Yael sabe que alguém está comprando fragmentos escuros e pagando caro demais. Ela pode ajudar o jogador ou vender a informação a outro.

Quest pessoal:

- encontrar loja noturna;
- recuperar item roubado;
- decidir se entrega comprador de Pedra Negra ao templo ou à guilda.

---

## 15. Thalindra Véu-de-Lua

```text
ID: npc_thalindra
Raça: Elfa cinzenta
Classe/arquétipo: Wizard/Archivist
Função: arquivo, lore, Cindar, Bromécia, Elyndor
Local: arquivo/biblioteca
Visita a fazenda: sim, se Fonte ou ruína for descoberta
Companion: informação/lore
```

Background:

Thalindra é metódica, fria e obcecada por versões corretas dos fatos. Ela não acredita em lenda sem inscrição, mas tem medo das inscrições que encontrou.

Relações:

- debate com Corvus sobre fé vs registro;
- depende de Mara para acessar documentos;
- acha Pip barulhento;
- respeita Liora, mas não entende sonhos como fonte.

Trama simples:

Thalindra tem uma cópia incompleta de um texto que menciona Cindar, Anya e uma “esperança enterrada”.

Quest pessoal:

- restaurar pergaminho;
- identificar símbolo nymiriano;
- abrir arquivo lacrado sem causar escândalo.

---

## 16. Dagna Rocha-Morna

```text
ID: npc_dagna
Raça: Anã
Classe/arquétipo: Miner/Expert
Função: minérios, contratos de coleta, avaliação de rocha
Local: depósito mineral / guilda / forja
Visita a fazenda: sim, para avaliar pedreira final ou rochas estranhas
Companion: mineração/caverna; possível companion funcional
```

Background:

Dagna é mineradora experiente e pragmática. Ela sabe que a caverna dá riqueza, mas também muda quem passa tempo demais lá.

Relações:

- amiga de Brumdar;
- discorda de Zrix sobre risco;
- acha Ozzra irresponsável;
- respeita Ser Alaric.

Trama simples:

Dagna perdeu um parente em uma galeria que não consta em mapa nenhum.

Quest pessoal:

- avaliar minério;
- encontrar marca de mineração antiga;
- decidir se reabre galeria perigosa.

---

## 17. Pip Semente-Solta

```text
ID: npc_pip
Raça: Halfling jovem
Classe/arquétipo: Commoner/Scout
Função: tutorial, entregas, humor, cidade viva
Local: praça/mercado/taverna
Visita a fazenda: sim, cedo, para tutorial e pequenos recados
Companion: não inicialmente
```

Background:

Pip corre mais do que pensa e ouve mais do que entende. Serve como ponto leve da cidade e pode introduzir sistemas sem parecer tutorial artificial.

Relações:

- protegido por Gruta;
- ajuda Sylveth;
- admira Ser Alaric;
- tem medo de Yael, mas compra doces dela.

Trama simples:

Pip viu alguém entrando no Jardim das Estátuas de madrugada, mas acha que era fantasma.

Quest pessoal:

- entregar primeira carta;
- procurar item perdido;
- contar rumor errado que vira pista certa depois.

---

## 18. Ser Alaric Veyr

```text
ID: npc_ser_alaric
Raça: Humano dornécio
Classe/arquétipo: Fighter/Guard Captain
Função: segurança, guarda, missões contra monstros
Local: posto de guarda / templo de Kanthor / entrada da caverna
Visita a fazenda: sim, em alerta de ameaça
Companion: possível companion de caverna em eventos específicos
```

Background:

Alaric acredita em ordem, patrulha e resposta direta. É respeitado, mas não entende bem ameaças antigas.

Relações:

- leal a Corvus;
- trabalha com Mara;
- respeita Dagna;
- não confia em Yael;
- vê Zrix como útil, mas indisciplinado.

Trama simples:

Alaric encobriu um incidente menor na caverna para evitar pânico. Isso pode voltar contra ele.

Quest pessoal:

- patrulha de estrada;
- investigar sumiço de ferramenta;
- enfrentar criatura que saiu da caverna.

---

## 19. Mirela dos Laços

```text
ID: npc_mirela
Raça: Humana dornécia
Classe/arquétipo: Tailor/Expert
Função: costura, bolsas, roupas, capas, acessórios
Local: ateliê de costura
Visita a fazenda: sim, para encomenda de lã/tecido
Companion: serviço/crafting
```

Background:

Mirela transforma tecido em utilidade. Acha que aparência também é ferramenta: uma boa capa muda como a cidade recebe alguém.

Relações:

- compra lã de Eiran;
- conversa com Gruta sobre moda e fofoca;
- acha Mara rígida demais;
- tem medo de ruídos atrás do ateliê.

Trama simples:

Mirela encontrou um tecido antigo que não apodrece e se repara devagar. Pode ser bromeciano.

Quest pessoal:

- criar primeira bolsa maior;
- preparar roupa para festival;
- investigar tecido auto-reparável.

---

## 20. Renko Três-Sorrisos

```text
ID: npc_renko
Raça: Goblin
Classe/arquétipo: Merchant
Função: loja geral, barganhas, itens comuns e oportunidades
Local: loja geral
Visita a fazenda: sim, como mercador ambulante em relação alta
Companion: serviço/economia
```

Background:

Renko sorri quando compra, quando vende e quando mente. Nem sempre mente por maldade; às vezes mente porque acha o mundo mais eficiente assim.

Relações:

- Ozzra deve dinheiro a ele;
- negocia com Zrix;
- respeita Mara por medo de multas;
- tenta vender quinquilharias para Pip.

Trama simples:

Renko comprou sem saber um item ligado à Pedra Negra. Agora quer se livrar dele sem perder dinheiro.

Quest pessoal:

- primeira compra/venda;
- recuperar mercadoria perdida;
- decidir se lucra ou entrega item perigoso.

---

## 21. Eiran Valeclaro

```text
ID: npc_eiran
Raça: Meio-elfo
Classe/arquétipo: Animal Handler/Ranger
Função: animais, ração, pets, cuidado animal
Local: rancho / mercado animal
Visita a fazenda: sim, para entregar animal, tratar pet ou inspeção
Companion: fazenda/animal/pet
```

Background:

Eiran entende animais melhor que pessoas. É paciente, observador e odeia quando chamam cuidado de bicho de “trabalho menor”.

Relações:

- amigo de Sylveth;
- compra tecido de Mirela;
- respeita Thandra;
- confia pouco em Ozzra perto dos animais.

Trama simples:

Animais estão evitando uma trilha perto da cidade. Eiran quer saber por quê antes que alguém mande Alaric resolver com espada.

Quest pessoal:

- adotar primeiro pet;
- comprar primeiro animal;
- investigar comportamento estranho dos bichos.

---

## 22. Liora Canta-Rio

```text
ID: npc_liora
Raça: Humana com sangue nymiriano distante, não revelado cedo
Classe/arquétipo: Bard/Seer
Função: música, sonhos, Alihana, pistas sutis
Local: praça/taverna/jardim antigo
Visita a fazenda: sim, em noites de Alihana ou relação alta
Companion: informação/lore; possível suporte raro
```

Background:

Liora canta canções que não aprendeu. Ela acha que são sonhos, mas algumas melodias reagem às estátuas antigas e à Fonte.

Relações:

- Gruta dá espaço para ela cantar;
- Corvus observa com cautela;
- Thalindra quer registrar as músicas;
- Maelor parece reconhecer uma das melodias.

Trama simples:

Liora sonha com uma mulher sem nome perto de água clara. O sonho muda conforme o jogador avança na caverna.

Quest pessoal:

- tocar na Noite de Alihana;
- encontrar versos perdidos;
- descobrir se seus sonhos são memória, profecia ou herança.

---

## 23. Orlan Pouso-Curto

```text
ID: npc_orlan
Raça: Humano dornécio
Classe/arquétipo: Innkeeper/Commoner
Função: hospedagem, notícias, viajantes
Local: estalagem/taverna
Visita a fazenda: raramente, para evento social
Companion: não
```

Background:

Orlan mantém camas, contas e histórias de viajantes. Ele não investiga nada, mas sabe quem chegou, quem partiu e quem mentiu sobre isso.

Relações:

- trabalha com Gruta;
- informa Mara sobre viajantes suspeitos;
- conhece Finan por ditados e superstições;
- desconfia de Maelor.

Trama simples:

Orlan hospedou alguém que desapareceu antes do amanhecer, deixando um mapa incompleto.

Quest pessoal:

- recuperar item de hóspede;
- identificar viajante falso;
- abrir rota de caravanas.

---

## 24. Savra Escama-Verde

```text
ID: npc_savra
Raça: Draconata verde
Classe/arquétipo: Herbalist/Ranger
Função: ervas, floresta, antídotos, ameaças naturais
Local: borda da cidade / herbalismo
Visita a fazenda: sim, para ervas, pragas ou plantas estranhas
Companion: floresta/caverna situacional
```

Background:

Savra veio de uma linhagem dracônica verde, mas rejeita a leitura simplista de que verde significa traição. Conhece venenos, antídotos e trilhas.

Relações:

- troca ervas com Sylveth;
- vende reagentes para Ozzra;
- irrita Corvus por rir de solenidades;
- é respeitada por Eiran.

Trama simples:

Savra encontra fungos de caverna crescendo onde não deveriam. Ela suspeita que o subsolo está respirando para cima.

Quest pessoal:

- criar antídoto;
- conter praga vegetal;
- mapear trilha afetada por Nyx.

---

## 25. Tovin Mãos-de-Selo

```text
ID: npc_tovin
Raça: Gnomo ou halfling, decidir depois
Classe/arquétipo: Scribe/Expert
Função: contratos, impostos, Merithus, registros
Local: cartório
Visita a fazenda: sim, para contrato/licença/altar
Companion: serviço/burocracia
```

Background:

Tovin acredita que todo problema tem formulário, e se não tiver é porque alguém falhou moralmente em desenhá-lo.

Relações:

- subordinado de Mara;
- irrita Nimble;
- admira Corvus;
- desconfia de Renko.

Trama simples:

Tovin achou um selo antigo que não pertence a nenhum registro atual. Ele quer catalogar antes que alguém perceba.

Quest pessoal:

- registrar construção;
- liberar altar;
- encontrar dono de selo antigo.

---

## 26. Maelor Cinza

```text
ID: npc_maelor
Raça: Elfo da noite / Luandil
Classe/arquétipo: Monk/Rogue
Função: memória de Nyx, segredo, observação, trama oculta
Local: aparece em horários incomuns; beco, jardim, estrada
Visita a fazenda: sim, raramente e à noite, se confiança alta
Companion: informação/segredo; combatente situacional tardio
```

Background:

Maelor parece estar sempre chegando de algum lugar que ninguém viu. Ele não é hostil, mas raramente responde à pergunta feita.

Relações:

- conhece Yael de antes;
- evita Corvus;
- escuta Liora cantar;
- observa o Jardim das Estátuas.

Trama simples:

Maelor sabe que a cidade esqueceu algo de propósito. Ele não sabe se deve ajudar o jogador a lembrar.

Quest pessoal:

- seguir sombra sem ser visto;
- recuperar memória em noite de Nyx;
- escolher entre revelar segredo ou preservar silêncio.

---

# PARTE C — Âncoras de lore

## 27. Estátuas de Anya

```text
ID: lore_anya_statues
Tipo: Lore Anchor
Local: Jardim das Estátuas Antigas
Função: memória apagada de Anya e Cindar
```

Descrição:

Conjunto de estátuas gastas, parcialmente cobertas por musgo. A população as trata como relíquia antiga sem utilidade clara. Alguns dizem que são santas antigas, outros dizem que são apenas ornamento de fundadores.

Regras:

- não explicar cedo;
- reagir a Alihana em eventos raros;
- Liora pode cantar perto delas;
- Thalindra pode pesquisar inscrições;
- Corvus evita discussão pública;
- Maelor observa em silêncio;
- podem se conectar com Água Viva/Fonte no futuro.

---

# PARTE D — Relações principais

## 28. Grupos sociais

### Ordem pública

- Padre Corvus
- Mara Vellum
- Ser Alaric
- Tovin

Tensão: querem manter a cidade estável, mas podem esconder problemas para evitar pânico.

### Ofícios e construção

- Nimble
- Gurd
- Hund
- Brumdar
- Mirela
- Ozzra

Tensão: progresso material vs risco de usar artefatos/recursos antigos.

### Rotina rural

- Sylveth
- Eiran
- Savra
- Gruta
- Pip

Tensão: a natureza e os animais percebem problemas antes da autoridade.

### Comércio e estrada

- Renko
- Zrix
- Orlan
- Dagna

Tensão: lucro, rotas, caverna e segurança.

### Segredos e memória

- Yael
- Thalindra
- Liora
- Maelor
- Estátuas de Anya

Tensão: revelar a verdade pode ajudar ou quebrar a ordem da cidade.

---

# PARTE E — Trama local simples

## 29. Trama base

A cidade tem três camadas de conflito:

```text
Camada pública:
  Kanthor, ordem, comércio, festivais, fazendas e rotina.

Camada social:
  disputas de licença, medo da caverna, sumiço de itens, tensão entre guilda e templo.

Camada oculta:
  estátuas de Anya, registros removidos, Pedra Negra corrompida, ruína bromeciana e símbolos de Elyndor.
```

## 30. Linha de trama sugerida

### Ato urbano 1 — Coisas pequenas fora do lugar

- Pip vê alguém no Jardim das Estátuas.
- Renko compra item perigoso sem entender.
- Sylveth nota planta crescendo fora de estação.
- Eiran nota animais evitando trilha.
- Corvus tenta manter calma.

### Ato urbano 2 — Registros e ruínas

- Mara encontra lacunas no cartório.
- Thalindra acha trecho sobre Cindar.
- Dagna identifica rocha que não pertence ao solo local.
- Nimble encontra madeira/estrutura auto-reparável.
- Zrix reconhece símbolo de Elyndor.

### Ato urbano 3 — Culto ou comprador oculto

- Yael revela que alguém compra fragmentos escuros.
- Alaric quer prender suspeitos rápido.
- Corvus quer proteger a cidade de pânico.
- Maelor sugere que a cidade escolheu esquecer.
- Liora sonha com água, estátua e raiz.

## 31. Resultado desejado

A trama deve levar o jogador a perceber que:

```text
Cindar's Hope não é só cidade rural.
A ordem pública de Kanthor está sobre uma memória anterior.
Anya foi quase apagada.
A caverna não é acidente geográfico.
O jogador pode restaurar, explorar, vender, esconder ou revelar partes desse passado.
```

---

# PARTE F — Visitas à fazenda

## 32. Regras por NPC

| NPC | Pode visitar? | Motivo |
|---|---|---|
| Corvus | Sim | bênção, alerta, juramento, investigação |
| Mara | Sim | licença, inspeção, reputação |
| Sylveth | Sim | crops, sementes, Thandra, plantas estranhas |
| Brumdar | Sim | ferramentas, minérios, upgrades |
| Nimble | Sim | construção, layout, mover casa/SellPoint |
| Gurd/Hund | Sim | obra, expansão, limpeza pesada |
| Ozzra | Sim | alquimia, solo, fertilizante, teste autorizado |
| Gruta | Sim | ingredientes, evento social, comida |
| Zrix | Sim | contrato de caverna, mapa, alerta |
| Yael | Sim, raro | noite, segredo, item oculto |
| Thalindra | Sim | Fonte, ruína, inscrição, pesquisa |
| Dagna | Sim | rochas, pedreira, minério |
| Pip | Sim | tutorial, carta, recado |
| Alaric | Sim | ameaça, guarda, investigação |
| Mirela | Sim | lã, roupa, encomenda |
| Renko | Sim | mercador ambulante, negócio |
| Eiran | Sim | animal, pet, ração, tratamento |
| Liora | Sim | sonho, Alihana, música, estátuas/Fonte |
| Orlan | Raro | hóspede, notícia, evento |
| Savra | Sim | ervas, pragas, trilha |
| Tovin | Sim | licença, contrato, altar |
| Maelor | Sim, raro | Nyx, segredo, observação |

---

# PARTE G — Serviços e companion modes

## 33. Companion/service mode sugerido

| NPC | Serviço principal | Pode virar companion? |
|---|---|---|
| Corvus | bênção/ordem | não, serviço religioso |
| Mara | licenças/reputação | não, serviço burocrático |
| Sylveth | farm/crops | sim, fazenda/serviço |
| Brumdar | forja | não regular, mentor/serviço |
| Nimble | construção | sim, serviço/fazenda |
| Gurd | força/obra | sim, fazenda/combate leve |
| Hund | obra/defesa | sim, fazenda/guarda |
| Ozzra | alquimia | sim, crafting/situacional |
| Gruta | comida/rumores | serviço/social |
| Zrix | guilda/caverna | sim, caverna/informação |
| Yael | segredo/loja | sim, informação/situacional |
| Thalindra | lore/arquivo | serviço/informação |
| Dagna | mineração | sim, mineração/caverna |
| Pip | tutorial/entrega | não inicialmente |
| Alaric | guarda | sim, eventos de combate |
| Mirela | costura | serviço/crafting |
| Renko | loja geral | serviço/economia |
| Eiran | animais/pets | sim, fazenda/animais |
| Liora | música/sonhos | sim, suporte/lore raro |
| Orlan | hospedagem | não |
| Savra | ervas/trilhas | sim, floresta/caverna situacional |
| Tovin | contratos | serviço/burocracia |
| Maelor | segredos/Nyx | sim, tardio/situacional |

---

# PARTE H — Pendências

## 34. Pendências de roster

- Validar quais nomes ficam definitivos.
- Definir retratos/visual de cada raça.
- Definir idade aproximada de cada NPC.
- Definir favoritos/presentes de cada NPC.
- Definir agenda diária de cada um.
- Definir casa/local de descanso de cada NPC.
- Definir quais NPCs terão romance/casamento, se esse sistema existir.
- Definir quais NPCs podem morrer/ficar indisponíveis em eventos, se aplicável.
- Definir quais classes são apenas flavor e quais afetam combate/serviço.
- Definir se Liora realmente tem sangue nymiriano ou só sensibilidade espiritual.
- Definir se Maelor é Luandil fixo ou outra origem ligada a Nyx.
- Definir se Tovin será gnomo ou halfling.

---

## 35. Próximos documentos recomendados

```text
docs/design/gameplay/city/CITY_LAYOUT_ZONES_DIRECTION_v1.0.md
docs/design/gameplay/city/CITY_RELATIONSHIP_VISITS_REPUTATION_DIRECTION_v1.0.md
docs/design/gameplay/city/CITY_ALTARS_DEITIES_BLESSINGS_DIRECTION_v1.0.md
```
