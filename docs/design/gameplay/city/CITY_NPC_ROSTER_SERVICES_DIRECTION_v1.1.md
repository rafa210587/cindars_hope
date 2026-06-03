# Cindar's Hope — City NPC Roster & Services Direction v1.1

> **Status:** direção ativa de elenco, serviços, romance, stats e quests da cidade  
> **Local:** `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`  
> **Substitui como direção ativa:** `CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.0.md`  
> **Depende de:** `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`  
> **Canon obrigatório:** `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`  
> **Fontes de referência:** `RACAS_DE_VAALARA_Guia_Completo.md`, `Deuses_de_Vaalara.md`, `DORNECIA_Guia_Completo.md`, `GDD_v2.6.md`  
> **Não é spec implementável.** Specs futuras devem ser quebradas em `docs/specs/a_implementar/`.

---

## 0. Alterações da v1.1

Esta versão adiciona:

- subraça/origem para cada NPC;
- classe/arquétipo clara;
- aparência baseada nas raças/subraças de Vaalara;
- HP, MP e atributos;
- resistências/status relevantes;
- estado civil;
- NPCs casados entre si;
- NPCs disponíveis para relacionamento/casamento com jogador de qualquer gênero;
- pelo menos 3 quests por NPC;
- regra de visita à fazenda por reputação/relacionamento/quest/casamento;
- compatibilidade com o templo de Kanthor e com a regra de Anya na Fonte.

---

# PARTE A — Regras gerais de NPC

## 1. Atributos usados

Cada NPC usa os atributos do projeto:

```text
FOR = Força
CON = Constituição
DES = Destreza
INT = Inteligência
VON = Vontade
CAR = Carisma
```

Escala recomendada para NPCs da cidade:

```text
1 = muito baixo
2 = baixo
3 = comum
4 = bom
5 = ótimo
6 = excepcional local
```

HP base é orientativo para design e balanceamento futuro. Não é contrato final de combate.

## 2. Estado de relacionamento

```text
MarriedToNpc
RomanceEligibleAnyPlayerGender
UnavailableForRomance
TooYoungOrNarrativelyBlocked
LateRomanceEligible
```

Quando um NPC é `RomanceEligibleAnyPlayerGender`, ele pode se relacionar e casar com o jogador independentemente do gênero escolhido pelo jogador.

## 3. Casais fixos

| Casal | Status |
|---|---|
| Nimble Galhobaixo + Mirela dos Laços | casados |
| Gruta Panela-Funda + Orlan Pouso-Curto | casados |
| Mara Vellum + Tovin Mãos-de-Selo | casados |

Esses NPCs não são candidatos de romance.

## 4. Candidatos a relacionamento/casamento

| NPC | Gênero | Observação |
|---|---|---|
| Sylveth | mulher | romance rural/fazenda |
| Ozzra | mulher | romance caótico/alquimia |
| Zrix | homem | romance estrada/caverna |
| Yael | mulher | romance noturno/segredos |
| Thalindra | mulher | romance arquivo/lore |
| Dagna | mulher | romance mineração/forja |
| Ser Alaric Veyr | homem | romance guarda/honra |
| Eiran Valeclaro | homem | romance animais/pets |
| Liora Canta-Rio | mulher | romance música/sonhos |
| Savra Escama-Verde | mulher | romance ervas/floresta |
| Maelor Cinza | homem | romance tardio/Nyx/memória |

## 5. Regra de Anya

```text
Nenhum NPC oferece construção de estátua de Anya na fazenda.
Nenhum NPC vende altar independente de Anya.
Anya só se manifesta na fazenda pela Fonte de Ressurreição / Fonte de Anya.
```

NPCs podem falar de Anya, pesquisar Anya, reagir às estátuas antigas da cidade ou à Fonte, mas não transformar Anya em decoração comum.

---

# PARTE B — Tabela-mestra

| ID | Nome | Raça/subraça | Classe clara | Serviço | Romance | HP/MP |
|---|---|---|---|---|---|---|
| npc_corvus | Padre Corvus | Humano de Mana | Cleric 5 / Paladin 2 de Kanthor | templo, juramentos | não | 120/45 |
| npc_mara | Mara Vellum | Humana de Mana | Expert 6 / Magistrate | cartório, reputação | casada | 75/20 |
| npc_sylveth | Sylveth | Elfa Silvestre | Druid 5 / Herbal Expert | sementes, Thandra | sim | 82/55 |
| npc_brumdar | Brumdar Ferro-Quieto | Anão de Khaz Baruk | Fighter 5 / Smith | forja, ferramentas | não | 150/10 |
| npc_nimble | Nimble Galhobaixo | Halfling Andarilho de Méritos | Expert 5 / Carpenter | construção | casado | 70/10 |
| npc_gurd | Gurd Carvalho-Torto | Meio-orc Clã da Fúria | Fighter 4 / Laborer | obras pesadas | não | 145/0 |
| npc_hund | Hund Carvalho-Torto | Meio-orc Clã da Noite | Guardian 4 / Laborer | defesa/obras | não | 135/10 |
| npc_ozzra | Ozzra Fumaçazul | Goblin Zhak'thul | Alchemist 5 / Artificer 2 | alquimia | sim | 68/60 |
| npc_gruta | Gruta Panela-Funda | Orc Clã da Chama Viva | Cook 4 / Bard 2 / Brawler | taverna/comida | casada | 130/15 |
| npc_zrix | Zrix das Estradas | Draconato Cinza do Julgamento, cobre | Ranger 5 / Merchant | guilda/caverna | sim | 125/25 |
| npc_yael | Yael Noite-Mansa | Elfa da Noite | Rogue 6 / Occult Trader | loja noturna | sim | 88/45 |
| npc_thalindra | Thalindra Véu-de-Lua | Ninrorin | Wizard 6 / Archivist | arquivo/lore | sim | 72/80 |
| npc_dagna | Dagna Rocha-Morna | Anã de Khaz Baruk | Miner 5 / Fighter 3 | minérios | sim | 155/10 |
| npc_pip | Pip Semente-Solta | Halfling Sortudo de Finan | Scout 1 / Commoner | tutorial/entregas | não | 45/0 |
| npc_alaric | Ser Alaric Veyr | Humano de Mana | Fighter 6 / Guard Captain | guarda | sim | 140/15 |
| npc_mirela | Mirela dos Laços | Humana de Mana | Tailor 5 / Expert | costura | casada | 68/15 |
| npc_renko | Renko Três-Sorrisos | Goblin Zhak'thul | Merchant 5 / Rogue 2 | loja geral | não | 65/10 |
| npc_eiran | Eiran Valeclaro | Meio-elfo de Thandra | Ranger 4 / Animal Handler | animais/pets | sim | 95/30 |
| npc_liora | Liora Canta-Rio | Humana de Mana com sangue nymiriano distante | Bard 5 / Seer | música/sonhos | sim | 70/70 |
| npc_orlan | Orlan Pouso-Curto | Humano de Mana | Innkeeper 4 / Commoner | hospedagem | casado | 80/5 |
| npc_savra | Savra Escama-Verde | Draconata oficial verde | Ranger 5 / Herbalist | ervas/antídotos | sim | 118/35 |
| npc_tovin | Tovin Mãos-de-Selo | Gnomo Artífice | Scribe 5 / Expert | contratos | casado | 58/40 |
| npc_maelor | Maelor Cinza | Elfo da Noite / Luandil | Monk 5 / Rogue 3 | segredos/Nyx | tardio | 105/50 |
| lore_anya_fountain | Fonte de Ressurreição | Lore Anchor | Sistema de Anya | respec/ressurreição | não | n/a |

---

# PARTE C — NPCs detalhados

## 6. Padre Corvus

```text
ID: npc_corvus
Gênero: homem
Idade narrativa: adulto maduro
Raça: Humano
Subraça/origem: Humano de Mana, dornécio
Classe clara: Cleric 5 / Paladin 2 de Kanthor
Estado relacionamento: UnavailableForRomance
Serviço: Templo de Kanthor, juramentos, bênçãos de ordem, mediação civil
Visita à fazenda: sim, por alerta, juramento, cerimônia ou investigação
HP: 120
MP: 45
FOR 3 | CON 4 | DES 2 | INT 4 | VON 6 | CAR 5
Resistências/status: +Medo, +Exaustão; fraco contra dúvida moral prolongada
```

Aparência:

Humano de Mana de pele morena clara, cabelos grisalhos presos curtos, olhos castanhos firmes. Usa vestes brancas e douradas de Kanthor com uma pequena balança de prata no peito. Sua postura é sempre controlada, mas os olhos denunciam cansaço.

Background:

Corvus é a face pública da ordem em Cindar's Hope. Ele acredita que a cidade só sobrevive porque mantém juramentos, contratos e lei mesmo longe da capital.

Relações:

- confia em Ser Alaric;
- respeita Mara;
- desconfia de Yael;
- evita Liora quando ela fala de sonhos;
- sabe que Thalindra pesquisa coisas que podem causar pânico.

Quests:

1. **O Sino que Não Toca**  
   O sino do templo de Kanthor racha antes de um festival. O jogador precisa coletar metal bom com Brumdar e convencer Corvus a aceitar ajuda de um ferreiro anão, não apenas rito religioso.  
   Recompensa: reputação com templo, bênção menor de Kanthor.

2. **Juramento Partido**  
   Um contrato público foi falsificado. O jogador investiga Mara, Tovin e Renko para descobrir se foi crime comum ou tentativa de desacreditar Kanthor.  
   Recompensa: acesso a contratos melhores e desconto em licenças.

3. **O Nome Sob a Pedra**  
   Uma obra revela uma inscrição anterior ao templo. Corvus pede discrição. O jogador decide ocultar, mostrar a Thalindra ou confrontar Corvus.  
   Recompensa: avanço na trama de Cindar/Anya ou reputação com Kanthor.

---

## 7. Mara Vellum

```text
ID: npc_mara
Gênero: mulher
Idade narrativa: adulta
Raça: Humana
Subraça/origem: Humana de Mana, dornécia
Classe clara: Expert 6 / Magistrate
Estado relacionamento: MarriedToNpc
Cônjuge: Tovin Mãos-de-Selo
Serviço: cartório, licenças, reputação, impostos, registros
Visita à fazenda: sim, para inspeções e licenças
HP: 75
MP: 20
FOR 1 | CON 2 | DES 3 | INT 6 | VON 5 | CAR 4
Resistências/status: +Medo burocrático, +Persuasão institucional; fraca contra Veneno/combate
```

Aparência:

Humana de Mana de pele oliva, cabelos pretos presos em coque severo, olhos escuros atentos. Usa casaco azul-ardósia com botões de bronze e carrega sempre um livro de registros.

Background:

Mara administra permissões e contratos. Para ela, papel assinado é o que separa uma comunidade de um bando assustado.

Relações:

- casada com Tovin;
- trabalha com Corvus;
- briga com Nimble por obras informais;
- suspeita que Renko subdeclara estoque.

Quests:

1. **Licença de Primeira Obra**  
   Ensina o jogador a registrar uma construção. Exige madeira, ouro e assinatura de Nimble.  
   Recompensa: desbloqueio formal de construção avançada.

2. **As Páginas Arrancadas**  
   Registros antigos da cidade desapareceram. Mara acha que é fraude fiscal; a investigação aponta para Cindar e o Jardim das Estátuas.  
   Recompensa: primeira flag de segredo urbano.

3. **Lei ou Compaixão**  
   Uma família não consegue pagar licença. O jogador escolhe pagar, negociar serviço comunitário ou aplicar a lei de forma dura.  
   Recompensa: altera reputação com Mara, Corvus e moradores.

---

## 8. Sylveth

```text
ID: npc_sylveth
Gênero: mulher
Idade narrativa: adulta jovem
Raça: Elfa
Subraça/origem: Elfa Silvestre, comunidade rural de Dornécia
Classe clara: Druid 5 / Herbal Expert
Estado relacionamento: RomanceEligibleAnyPlayerGender
Serviço: sementes, calendário agrícola, Thandra, crops raras não-Mana
Visita à fazenda: sim, por crops, sementes e eventos de Thandra
HP: 82
MP: 55
FOR 2 | CON 3 | DES 5 | INT 4 | VON 5 | CAR 4
Resistências/status: +Veneno vegetal, +Frio leve; fraca contra Fogo/Calor
```

Aparência:

Elfa Silvestre de pele bronze-cobre, cabelos castanho-musgo trançados com folhas secas e olhos verde-âmbar. Usa roupas naturais em tons de terra e verde, com pequenas contas de madeira.

Background:

Sylveth cuida da loja de sementes e mantém os ritos de Thandra vivos sem transformá-los em dogma público.

Relações:

- amiga de Eiran;
- fornece ervas para Ozzra;
- respeita Savra;
- acha Thalindra distante demais da terra.

Quests:

1. **Sementes de Retorno**  
   Sylveth pede ajuda para recuperar sementes antigas em uma trilha tomada por ervas agressivas.  
   Recompensa: novo seed comum da estação.

2. **Festival de Thandra**  
   O jogador fornece crops de qualidade para a Feira da Primeira Colheita e ajuda Sylveth a montar um altar rural temporário.  
   Recompensa: reputação rural, fertilizante simples.

3. **Raízes que Ouvem**  
   Algumas plantas reagem à Fonte e às estátuas antigas. Sylveth teme que não seja bênção de Thandra.  
   Recompensa: pista sobre Anya sem revelar demais.

Romance:

- Quest de vínculo: **A Terra Escolhe Devagar**.
- Após casamento, pode visitar a fazenda em dias de feira e ajudar com crops sem automatizar tudo.

---

## 9. Brumdar Ferro-Quieto

```text
ID: npc_brumdar
Gênero: homem
Idade narrativa: adulto maduro
Raça: Anão
Subraça/origem: Anão de Khaz Baruk
Classe clara: Fighter 5 / Smith
Estado relacionamento: UnavailableForRomance
Serviço: forja, ferramentas, armas, reparo
Visita à fazenda: sim, para entrega de upgrades
HP: 150
MP: 10
FOR 5 | CON 6 | DES 2 | INT 4 | VON 5 | CAR 2
Resistências/status: +Calor, +Medo, +Exaustão; fraco contra magia mental
```

Aparência:

Anão de Khaz Baruk de pele curtida, barba cinza longa em três tranças com argolas de ferro, braços grossos e mãos marcadas por queimaduras antigas. Usa avental escuro, martelo curto no cinto e olhos de brilho mineral.

Background:

Brumdar veio para Cindar's Hope pela qualidade estranha dos minérios da caverna. Ele não confia em metal sem história.

Relações:

- grande amigo de Dagna;
- rival técnico de Ozzra;
- respeita Corvus;
- acha Zrix informal demais.

Quests:

1. **A Primeira Ferramenta Séria**  
   O jogador traz cobre/ferro para melhorar uma ferramenta. Brumdar explica durabilidade e cuidado.  
   Recompensa: primeiro upgrade de ferramenta.

2. **Metal que Sussurra**  
   Um lingote vibra perto de uma Pedra Negra corrompida. Brumdar pede análise antes que Mara confisque.  
   Recompensa: desbloqueio de minério especial rastreado.

3. **O Martelo do Aprendiz**  
   Brumdar perdeu um martelo antigo em uma galeria. Dagna sabe mais do que conta.  
   Recompensa: receita de arma/ferramenta intermediária.

---

## 10. Nimble Galhobaixo

```text
ID: npc_nimble
Gênero: homem
Idade narrativa: adulto
Raça: Halfling
Subraça/origem: Andarilho de Méritos
Classe clara: Expert 5 / Carpenter
Estado relacionamento: MarriedToNpc
Cônjuge: Mirela dos Laços
Serviço: carpintaria, construções, modo construção
Visita à fazenda: sim, frequentemente por obras
HP: 70
MP: 10
FOR 2 | CON 3 | DES 5 | INT 5 | VON 3 | CAR 4
Resistências/status: +Medo por sorte, +Exaustão leve; fraco contra dano direto
```

Aparência:

Halfling de cabelos castanhos claros, costeletas compridas, dedos rápidos e pés peludos sempre sujos de serragem. Usa colete azul gasto cheio de bolsos, fita métrica no pescoço e lápis atrás da orelha.

Background:

Nimble adora construir, mover, medir e reclamar do layout dos outros.

Relações:

- casado com Mirela;
- trabalha com Gurd e Hund;
- discute com Mara;
- gosta de Gruta.

Quests:

1. **Madeira, Pedra e Assinatura**  
   Tutorial de construção: juntar materiais, pagar licença e escolher local válido.  
   Recompensa: primeira construção secundária.

2. **Mover é Mais Difícil que Erguer**  
   Libera movimentação de SellPoint/casa. Exige resolver conflito com Mara sobre registro de planta da fazenda.  
   Recompensa: modo mover construção.

3. **A Tábua que Cura**  
   Nimble encontra madeira auto-reparável bromeciana. O jogador decide usar, guardar ou entregar a Thalindra.  
   Recompensa: avanço em tecnologia bromeciana.

---

## 11. Gurd Carvalho-Torto

```text
ID: npc_gurd
Gênero: homem
Idade narrativa: adulto jovem
Raça: Meio-orc
Subraça/origem: Clã da Fúria, sangue integrado em Dornécia
Classe clara: Fighter 4 / Laborer
Estado relacionamento: UnavailableForRomance
Serviço: obra pesada, limpeza de expansão, força bruta
Visita à fazenda: sim, para obras/expansões
HP: 145
MP: 0
FOR 6 | CON 5 | DES 2 | INT 2 | VON 3 | CAR 3
Resistências/status: +Medo, +Exaustão; fraco contra Encanto/controle mental
```

Aparência:

Meio-orc de pele cinza-avermelhada, presas evidentes, corpo largo, tatuagens geométricas nos ombros e cicatrizes visíveis. Usa camisa sem mangas, botas pesadas e luvas de obra.

Background:

Gurd resolve problemas levantando, quebrando ou carregando. Ele é direto, mas não cruel.

Relações:

- irmão de Hund;
- trabalha com Nimble;
- bebe na taverna de Gruta;
- respeita Brumdar.

Quests:

1. **Pedra Grande, Martelo Maior**  
   Ajuda a remover obstáculos da fazenda.  
   Recompensa: desbloqueia limpeza de área pesada.

2. **A Parede que Não Quebrou**  
   Gurd encontra uma parede antiga que resiste a golpes. Precisa decidir chamar Thalindra ou tentar quebrar.  
   Recompensa: pista sobre ruína subterrânea.

3. **Força sem Fúria**  
   Gurd se envolve em briga durante festival de Senya. O jogador pode acalmá-lo, apoiá-lo ou entregá-lo a Corvus.  
   Recompensa: melhora relação e possível job de força na fazenda.

---

## 12. Hund Carvalho-Torto

```text
ID: npc_hund
Gênero: homem
Idade narrativa: adulto jovem
Raça: Meio-orc
Subraça/origem: Clã da Noite, sangue integrado em Dornécia
Classe clara: Guardian 4 / Laborer
Estado relacionamento: UnavailableForRomance
Serviço: transporte, defesa de obras, guarda informal
Visita à fazenda: sim, obras e eventos de ameaça
HP: 135
MP: 10
FOR 5 | CON 5 | DES 3 | INT 3 | VON 4 | CAR 2
Resistências/status: +Medo, +Frio noturno; fraco contra Calor
```

Aparência:

Meio-orc de pele cinza-azulada, presas menores que as de Gurd, olhos escuros atentos e cabelo raspado nas laterais. Usa casaco de couro pesado e carrega cordas e ferramentas de içamento.

Background:

Hund observa antes de agir. Ele é mais silencioso que Gurd e percebe detalhes que outros ignoram.

Relações:

- irmão de Gurd;
- protetor de Pip;
- respeita Ser Alaric;
- desconfia de Maelor.

Quests:

1. **A Entrega que Não Chegou**  
   Uma carga de madeira desaparece no caminho da fazenda. Hund pede ajuda discreta.  
   Recompensa: materiais de construção.

2. **Barulho no Poço**  
   Hund ouviu algo sob a cidade durante uma obra. O jogador investiga à noite.  
   Recompensa: flag de subsolo.

3. **Guardar sem Mandar**  
   Hund quer proteger a cidade sem virar guarda oficial. O jogador ajuda a definir patrulha de fazenda/cidade.  
   Recompensa: possível proteção contra eventos negativos na fazenda.

---

## 13. Ozzra Fumaçazul

```text
ID: npc_ozzra
Gênero: mulher
Idade narrativa: adulta jovem
Raça: Goblin
Subraça/origem: Zhak'thul, clã do Grito Livre
Classe clara: Alchemist 5 / Artificer 2
Estado relacionamento: RomanceEligibleAnyPlayerGender
Serviço: alquimia, poções, fertilizantes, reagentes
Visita à fazenda: sim, para testes autorizados
HP: 68
MP: 60
FOR 1 | CON 3 | DES 5 | INT 6 | VON 3 | CAR 4
Resistências/status: +Veneno, +Caos/Senya; fraca contra Medo institucional
```

Aparência:

Goblin de pele laranja-acinzentada, orelhas grandes, cabelo azul espetado por tintura alquímica, sorriso amplo e olhos amarelos vivos. Usa avental cheio de frascos e luvas grandes demais.

Background:

Ozzra vê cada coisa como possível mistura útil. Seu laboratório é metade oficina, metade ameaça pública.

Relações:

- deve dinheiro a Renko;
- rivaliza com Brumdar;
- compra ervas de Sylveth e Savra;
- acha Yael fascinante.

Quests:

1. **Explode Só um Pouco**  
   Tutorial de poção: coletar ervas, água e frasco.  
   Recompensa: receita de poção simples.

2. **Fertilizante Fumaçazul**  
   Ozzra cria fertilizante que pode melhorar qualidade, mas precisa teste seguro na fazenda.  
   Recompensa: primeiro fertilizante avançado.

3. **Reagente que Bebe Luz**  
   Um cristal da caverna absorve luz e deixa pessoas cansadas.  
   Recompensa: pista sobre Pedra Negra corrompida.

Romance:

- Quest de vínculo: **A Fórmula do Afeto Improvável**.
- Após casamento, pode montar uma bancada de alquimia na fazenda com regras de segurança.

---

## 14. Gruta Panela-Funda

```text
ID: npc_gruta
Gênero: mulher
Idade narrativa: adulta
Raça: Orc
Subraça/origem: Clã da Chama Viva, integrada em Dornécia
Classe clara: Cook 4 / Bard 2 / Brawler
Estado relacionamento: MarriedToNpc
Cônjuge: Orlan Pouso-Curto
Serviço: taverna, comida, descanso, rumores
Visita à fazenda: sim, para ingredientes ou evento social
HP: 130
MP: 15
FOR 5 | CON 5 | DES 2 | INT 3 | VON 4 | CAR 5
Resistências/status: +Calor, +Medo; fraca contra Frio
```

Aparência:

Orc de pele verde-escura com reflexos quentes, presas fortes, cabelo ruivo preso em trança grossa e braços poderosos. Usa avental vermelho gasto, colheres de madeira no cinto e gargalhada alta.

Background:

Gruta comanda a taverna como se fosse um quartel acolhedor. Alimenta, intimida e escuta a cidade inteira.

Relações:

- casada com Orlan;
- protege Pip;
- troca rumores com Zrix;
- provoca Corvus para ele rir ao menos uma vez.

Quests:

1. **Sopa para um Dia Ruim**  
   Coletar ingredientes básicos para prato que reduz cansaço.  
   Recompensa: receita simples de comida.

2. **Rumor Queimado**  
   Símbolos aparecem sob mesas da taverna. Gruta quer saber quem marcou sem assustar clientes.  
   Recompensa: pista de culto.

3. **Banquete de Festival**  
   Preparar comida para festival de Senya ou Thandra.  
   Recompensa: reputação social e buff temporário.

---

## 15. Zrix das Estradas

```text
ID: npc_zrix
Gênero: homem
Idade narrativa: adulto
Raça: Draconato
Subraça/origem: Cinza do Julgamento, escamas cobre/cinzentas
Classe clara: Ranger 5 / Merchant
Estado relacionamento: RomanceEligibleAnyPlayerGender
Serviço: Guilda das Estradas, mapas, caverna, checkpoints
Visita à fazenda: sim, por contratos e mapas
HP: 125
MP: 25
FOR 4 | CON 5 | DES 4 | INT 4 | VON 4 | CAR 4
Resistências/status: +Medo, +Veneno leve; fraco contra Frio intenso
```

Aparência:

Draconato de escamas cobre escurecido com placas cinza, chifres curtos polidos, olhos âmbar e cauda marcada por cicatrizes de estrada. Usa capa de viagem e mapas enrolados em tubos de couro.

Background:

Zrix conhece rotas, atalhos, mapas e perigos. Trata a caverna como estrada ruim: não é segura, mas dá para aprender.

Relações:

- troca rumores com Gruta;
- negocia com Renko;
- respeita Alaric;
- evita Yael em público.

Quests:

1. **Mapa de Entrada**  
   Tutorial de Guilda e primeiros contratos de caverna.  
   Recompensa: mapa parcial do nível inicial.

2. **Sinal de Elyndor**  
   Zrix reconhece símbolo de portal em pedra antiga.  
   Recompensa: desbloqueia investigação de checkpoint.

3. **A Estrada que Desce**  
   Um contrato pede item de nível perigoso. Zrix oferece acompanhar se relação for alta.  
   Recompensa: possível companion de caverna.

Romance:

- Quest de vínculo: **O Caminho de Volta**.
- Após casamento, mantém Guilda, mas visita a fazenda em dias definidos.

---

## 16. Yael Noite-Mansa

```text
ID: npc_yael
Gênero: mulher
Idade narrativa: adulta
Raça: Elfa
Subraça/origem: Elfa da Noite, Luandil
Classe clara: Rogue 6 / Occult Trader
Estado relacionamento: RomanceEligibleAnyPlayerGender
Serviço: loja noturna, Nyx, itens raros, rumores
Visita à fazenda: sim, raro, à noite
HP: 88
MP: 45
FOR 2 | CON 3 | DES 6 | INT 5 | VON 4 | CAR 5
Resistências/status: +Medo, +Furtividade, +Nyx; fraca sob Senya/festa intensa
```

Aparência:

Elfa da Noite de pele azul-noite, cabelos branco-prateados, olhos violeta que brilham no escuro e pequenas tatuagens luminescentes nos pulsos. Usa roupas escuras com detalhes prateados e capuz leve.

Background:

Yael vende coisas raras e informações discretas. Ela não é vilã, mas sabe lucrar com segredos.

Relações:

- Corvus desconfia dela;
- conhece Maelor;
- Ozzra quer estudar seus itens;
- Renko finge não ter negócios com ela.

Quests:

1. **Aberto Depois da Meia-Noite**  
   Encontrar a loja noturna pela primeira vez.  
   Recompensa: acesso a estoque de Nyx.

2. **Comprador de Fragmentos**  
   Yael revela que alguém compra pedras escuras. O jogador decide contar a Alaric, Corvus ou ninguém.  
   Recompensa: avanço de culto/Pedra Negra.

3. **Preço do Silêncio**  
   Um segredo de Yael ameaça vir à tona. Ajudá-la fortalece vínculo, mas reduz confiança com Corvus se descoberto.  
   Recompensa: item raro noturno.

Romance:

- Quest de vínculo: **Confiança no Escuro**.
- Após casamento, não vira moradora comum diurna; mantém rotina noturna.

---

## 17. Thalindra Véu-de-Lua

```text
ID: npc_thalindra
Gênero: mulher
Idade narrativa: adulta
Raça: Elfa
Subraça/origem: Ninrorin, elfa cinzenta
Classe clara: Wizard 6 / Archivist
Estado relacionamento: RomanceEligibleAnyPlayerGender
Serviço: arquivo, história, Cindar, Bromécia, Elyndor
Visita à fazenda: sim, quando há Fonte/ruína/inscrição
HP: 72
MP: 80
FOR 1 | CON 2 | DES 3 | INT 6 | VON 5 | CAR 3
Resistências/status: +Medo arcano, +Frio mental; fraca contra dano físico
```

Aparência:

Ninrorin de pele cinza clara, cabelos prata lisos, olhos azul-gelo e porte elegante. Usa túnica azul-cobalto com fios prateados e luvas para manusear pergaminhos.

Background:

Thalindra quer versão correta dos fatos. O problema é que os fatos corretos podem desestabilizar a cidade.

Relações:

- debate com Corvus;
- depende de Mara para acesso a documentos;
- ouve Liora com interesse cético;
- teme tecnologia bromeciana, mas não consegue ignorá-la.

Quests:

1. **Poeira no Arquivo**  
   Restaurar registros danificados com itens simples e ajuda de Tovin.  
   Recompensa: entrada no arquivo restrito.

2. **A Palavra Nymiriana**  
   Identificar inscrição no Jardim das Estátuas.  
   Recompensa: primeira tradução parcial sobre Anya/Cindar.

3. **O Mapa Que Não Deveria Existir**  
   Um mapa aponta conexão entre cidade e caverna.  
   Recompensa: pista de Elyndor/Bromécia.

Romance:

- Quest de vínculo: **O Que a História Não Diz**.
- Após casamento, pode instalar pequena mesa de pesquisa na fazenda.

---

## 18. Dagna Rocha-Morna

```text
ID: npc_dagna
Gênero: mulher
Idade narrativa: adulta madura
Raça: Anã
Subraça/origem: Anã de Khaz Baruk
Classe clara: Miner 5 / Fighter 3
Estado relacionamento: RomanceEligibleAnyPlayerGender
Serviço: minérios, avaliação de rocha, contratos de mineração
Visita à fazenda: sim, pedreira/rochas/minério
HP: 155
MP: 10
FOR 5 | CON 6 | DES 2 | INT 4 | VON 5 | CAR 3
Resistências/status: +Calor, +Exaustão, +Medo; fraca contra magia psíquica
```

Aparência:

Anã robusta de pele bronzeada, cabelo preto com mechas brancas, barba curta trançada, olhos castanho-escuros e braços marcados por pó de minério. Usa botas reforçadas e capacete pendurado no cinto.

Background:

Dagna conhece pedra viva, pedra morta e pedra que mente. Ela respeita a caverna porque sabe que ela muda.

Relações:

- amiga íntima de Brumdar;
- discorda de Zrix sobre risco;
- respeita Alaric;
- suspeita de Renko.

Quests:

1. **Veio de Cobre**  
   Ensina avaliação de minério e contratos de mineração.  
   Recompensa: preço melhor por minérios comuns.

2. **Galeria Sem Nome**  
   Dagna perdeu alguém numa galeria que não aparece nos mapas.  
   Recompensa: acesso a sala especial da caverna.

3. **A Rocha Quente Demais**  
   Uma pedra da fazenda reage à noite. Dagna pede ajuda para remover sem quebrar.  
   Recompensa: caminho para pedreira final futura.

Romance:

- Quest de vínculo: **Pedra Também Guarda Luto**.
- Após casamento, pode avaliar nodes de mineração na fazenda/caverna.

---

## 19. Pip Semente-Solta

```text
ID: npc_pip
Gênero: homem
Idade narrativa: adolescente/jovem mensageiro
Raça: Halfling
Subraça/origem: Sortudo de Finan
Classe clara: Scout 1 / Commoner
Estado relacionamento: TooYoungOrNarrativelyBlocked
Serviço: tutorial, entregas, recados, humor
Visita à fazenda: sim, cedo e com frequência
HP: 45
MP: 0
FOR 1 | CON 2 | DES 5 | INT 2 | VON 2 | CAR 5
Resistências/status: +Sorte/Medo leve; fraco contra qualquer combate real
```

Aparência:

Halfling pequeno, cabelos ruivos bagunçados, sardas abundantes, pés com pelos dourados e sorriso de quem acabou de fazer algo errado. Usa mochila grande demais.

Background:

Pip corre pela cidade levando cartas, compras e confusão.

Relações:

- protegido por Gruta e Hund;
- ajuda Sylveth;
- admira Alaric;
- tem medo e fascínio por Yael.

Quests:

1. **Primeira Entrega**  
   Pip leva o jogador até a loja de sementes e praça.  
   Recompensa: tutorial social.

2. **Vi o Fantasma**  
   Pip viu alguém no Jardim das Estátuas. Ele acha que era fantasma.  
   Recompensa: primeira pista de evento noturno.

3. **A Carta Errada**  
   Pip entrega carta no destino errado e expõe pequena tensão entre Mara, Renko e Yael.  
   Recompensa: reputação com Pip e pistas urbanas.

---

## 20. Ser Alaric Veyr

```text
ID: npc_alaric
Gênero: homem
Idade narrativa: adulto
Raça: Humano
Subraça/origem: Humano de Mana, dornécio
Classe clara: Fighter 6 / Guard Captain
Estado relacionamento: RomanceEligibleAnyPlayerGender
Serviço: guarda, patrulha, segurança, combate
Visita à fazenda: sim, alertas e ameaças
HP: 140
MP: 15
FOR 5 | CON 5 | DES 4 | INT 3 | VON 5 | CAR 4
Resistências/status: +Medo, +Exaustão; fraco contra magia ilusória
```

Aparência:

Humano alto, pele clara bronzeada pelo sol, cabelo castanho curto, barba aparada e olhos verdes. Usa meia-armadura azul-ardósia com símbolo de Kanthor no ombro.

Background:

Alaric acredita em patrulha, lâmina e responsabilidade. Ele é honesto, mas tende a simplificar problemas antigos como crimes comuns.

Relações:

- leal a Corvus;
- trabalha com Mara;
- respeita Dagna;
- não confia em Yael;
- considera Zrix útil, mas indisciplinado.

Quests:

1. **Patrulha da Estrada Baixa**  
   O jogador ajuda Alaric a afastar criaturas perto da entrada da caverna.  
   Recompensa: reputação com guarda.

2. **Relatório Incompleto**  
   Alaric omitiu um incidente para evitar pânico. O jogador decide cobrir ou revelar.  
   Recompensa: confiança de Alaric ou reputação pública.

3. **A Justiça Não Basta**  
   Um suspeito de culto é inocente de um crime, mas sabe algo perigoso.  
   Recompensa: bifurcação entre Kanthor/segredo.

Romance:

- Quest de vínculo: **O Peso do Escudo**.
- Após casamento, pode proteger a fazenda em eventos de ameaça.

---

## 21. Mirela dos Laços

```text
ID: npc_mirela
Gênero: mulher
Idade narrativa: adulta
Raça: Humana
Subraça/origem: Humana de Mana
Classe clara: Tailor 5 / Expert
Estado relacionamento: MarriedToNpc
Cônjuge: Nimble Galhobaixo
Serviço: costura, bolsas, roupas, capas, acessórios
Visita à fazenda: sim, encomendas de lã/tecido
HP: 68
MP: 15
FOR 1 | CON 2 | DES 5 | INT 5 | VON 3 | CAR 5
Resistências/status: +Medo social, +Frio leve com roupas; fraca contra combate direto
```

Aparência:

Humana de pele morena, cabelos cacheados escuros com fitas coloridas, olhos castanhos vivos e roupas sempre bem ajustadas. Usa tesoura pequena no cinto e agulhas em estojo de madeira.

Background:

Mirela acredita que roupa também é ferramenta: muda proteção, presença e pertencimento.

Relações:

- casada com Nimble;
- amiga de Gruta;
- compra lã de Eiran;
- irrita Mara com pedidos de licença estética.

Quests:

1. **Bolsa de Trabalho**  
   Criar primeira expansão de inventário com tecido e couro simples.  
   Recompensa: upgrade de bolsa.

2. **Fio que Não Rasga**  
   Mirela encontra tecido auto-reparável possivelmente bromeciano.  
   Recompensa: unlock de roupa especial futura.

3. **Roupa de Festival**  
   Preparar traje para festival escolhido.  
   Recompensa: buff social temporário.

---

## 22. Renko Três-Sorrisos

```text
ID: npc_renko
Gênero: homem
Idade narrativa: adulto
Raça: Goblin
Subraça/origem: Zhak'thul, comerciante integrado
Classe clara: Merchant 5 / Rogue 2
Estado relacionamento: UnavailableForRomance
Serviço: loja geral, barganhas, itens comuns
Visita à fazenda: sim, mercador ambulante em reputação alta
HP: 65
MP: 10
FOR 1 | CON 3 | DES 5 | INT 5 | VON 2 | CAR 6
Resistências/status: +Sorte/Finan, +Enganação; fraco contra Kanthor/contratos rígidos
```

Aparência:

Goblin de pele laranja-acinzentada, olhos amarelos semicerrados, dentes pequenos e sorriso constante. Usa colete cheio de moedas falsas e verdadeiras misturadas.

Background:

Renko compra, vende e sorri. Nem sempre mente por maldade; às vezes mente porque considera preço fixo uma ofensa criativa.

Relações:

- Ozzra deve dinheiro a ele;
- negocia com Zrix;
- teme multas de Mara;
- vende besteiras para Pip.

Quests:

1. **Preço de Amigo**  
   Tutorial de loja geral e barganha.  
   Recompensa: desconto pequeno.

2. **Mercadoria Sem Dono**  
   Renko comprou item perigoso sem saber. O jogador decide vender, entregar ou investigar.  
   Recompensa: pista de Pedra Negra.

3. **Três Sorrisos, Uma Mentira**  
   Descobrir qual das três versões de Renko é verdade.  
   Recompensa: estoque raro ou multa reduzida.

---

## 23. Eiran Valeclaro

```text
ID: npc_eiran
Gênero: homem
Idade narrativa: adulto jovem
Raça: Meio-elfo
Subraça/origem: Meio-elfo de Thandra
Classe clara: Ranger 4 / Animal Handler
Estado relacionamento: RomanceEligibleAnyPlayerGender
Serviço: animais, ração, pets, cuidado animal
Visita à fazenda: sim, entrega animal/pet/tratamento
HP: 95
MP: 30
FOR 3 | CON 4 | DES 4 | INT 3 | VON 5 | CAR 4
Resistências/status: +Medo animal, +Veneno leve; fraco contra ruído/caos urbano
```

Aparência:

Meio-elfo de pele bronzeada, orelhas levemente pontudas, cabelo castanho longo preso com cordão verde e olhos âmbar calmos. Usa roupas de couro macio e sempre carrega ração.

Background:

Eiran entende animais melhor que pessoas. Ele é calmo, firme e muito observador.

Relações:

- amigo próximo de Sylveth;
- respeita Savra;
- compra tecido de Mirela;
- evita Ozzra perto dos animais.

Quests:

1. **Primeira Tigela**  
   Tutorial de pet: comida, cama e vínculo.  
   Recompensa: desbloqueio de pet.

2. **Animal Assustado**  
   Animais evitam uma trilha. O jogador investiga sem violência primeiro.  
   Recompensa: reputação com Eiran e pista de ameaça natural.

3. **Cuidado Não é Fraqueza**  
   Um animal doente precisa de ervas, poção e rotina.  
   Recompensa: desbloqueio de remédio animal.

Romance:

- Quest de vínculo: **Ficar é Também Cuidar**.
- Após casamento, pode ajudar com pets/animais sem ocupar slot de companion.

---

## 24. Liora Canta-Rio

```text
ID: npc_liora
Gênero: mulher
Idade narrativa: adulta jovem
Raça: Humana
Subraça/origem: Humana de Mana com sangue nymiriano distante, não revelado cedo
Classe clara: Bard 5 / Seer
Estado relacionamento: RomanceEligibleAnyPlayerGender
Serviço: música, sonhos, Alihana, pistas sutis
Visita à fazenda: sim, em noites de Alihana ou relação alta
HP: 70
MP: 70
FOR 1 | CON 2 | DES 4 | INT 4 | VON 6 | CAR 6
Resistências/status: +Medo por sonho, +Alihana; fraca contra Nyx/cansaço mental
```

Aparência:

Humana de pele dourada clara, cabelo castanho-escuro ondulado, olhos azulados incomuns e voz suave. Usa vestidos simples com fitas brancas e pequenos pingentes de rio.

Background:

Liora canta músicas que não aprendeu. Algumas reagem ao Jardim das Estátuas e, futuramente, à Fonte.

Relações:

- Gruta dá espaço para ela cantar;
- Corvus observa com cautela;
- Thalindra quer registrar suas canções;
- Maelor reconhece uma melodia.

Quests:

1. **Canção sem Autor**  
   Liora pede ajuda para encontrar a origem de uma melodia.  
   Recompensa: evento de Alihana.

2. **A Estátua que Escutou**  
   Uma estátua antiga parece reagir à música.  
   Recompensa: pista de Anya/Cindar.

3. **Sonho de Água Clara**  
   Liora sonha com a Fonte e a caverna. O jogador compara sonho com mapa real.  
   Recompensa: pista de nível especial/Água Viva.

Romance:

- Quest de vínculo: **A Voz que Fica**.
- Após casamento, pode gerar eventos musicais raros na fazenda.

---

## 25. Orlan Pouso-Curto

```text
ID: npc_orlan
Gênero: homem
Idade narrativa: adulto
Raça: Humano
Subraça/origem: Humano de Mana, dornécio
Classe clara: Innkeeper 4 / Commoner
Estado relacionamento: MarriedToNpc
Cônjuge: Gruta Panela-Funda
Serviço: hospedagem, notícias, viajantes
Visita à fazenda: raro, eventos sociais
HP: 80
MP: 5
FOR 2 | CON 3 | DES 2 | INT 4 | VON 4 | CAR 5
Resistências/status: +Medo social, +Exaustão; fraco contra combate real
```

Aparência:

Humano baixo e largo, pele clara, cabelo castanho ralo, bigode curto e olhos gentis. Usa camisa azul, avental limpo demais para uma taverna e carrega chaves no cinto.

Background:

Orlan cuida das camas, contas e viajantes. Ele sabe quem chegou e quem partiu antes do amanhecer.

Relações:

- casado com Gruta;
- informa Mara sobre viajantes suspeitos;
- respeita Corvus;
- desconfia de Maelor.

Quests:

1. **Quarto de Viajante**  
   Ajudar a preparar hospedagem para caravana.  
   Recompensa: notícia de estrada.

2. **Hóspede Sem Sombra**  
   Um viajante desaparece deixando mapa incompleto.  
   Recompensa: mapa/rumor de caverna.

3. **Conta Aberta**  
   Orlan precisa cobrar dívida sem criar briga com Renko.  
   Recompensa: reputação social.

---

## 26. Savra Escama-Verde

```text
ID: npc_savra
Gênero: mulher
Idade narrativa: adulta
Raça: Draconata
Subraça/origem: Draconata verde, linhagem oficial cromática domesticada/integrada
Classe clara: Ranger 5 / Herbalist
Estado relacionamento: RomanceEligibleAnyPlayerGender
Serviço: ervas, antídotos, floresta, pragas
Visita à fazenda: sim, por ervas/pragas/plantas estranhas
HP: 118
MP: 35
FOR 4 | CON 5 | DES 4 | INT 4 | VON 4 | CAR 3
Resistências/status: +Veneno, +Calor úmido; fraca contra Frio intenso
```

Aparência:

Draconata de escamas verde-escuras com manchas oliva, olhos amarelo-ouro, chifres curtos voltados para trás e cauda fina. Usa capas de couro vegetal e frascos de antídoto.

Background:

Savra entende venenos, trilhas e plantas hostis. Ela rejeita a ideia de que linhagem verde define caráter.

Relações:

- troca ervas com Sylveth;
- vende reagentes a Ozzra;
- irrita Corvus com sarcasmo;
- é respeitada por Eiran.

Quests:

1. **Antídoto Amargo**  
   Coletar ervas para primeiro antídoto.  
   Recompensa: receita de antídoto.

2. **Praga que Anda**  
   Uma praga vegetal se move de noite.  
   Recompensa: defesa contra pragas na fazenda.

3. **O Fungo de Baixo**  
   Fungos de caverna aparecem perto da cidade.  
   Recompensa: pista de bioma subterrâneo.

Romance:

- Quest de vínculo: **Não Sou Meu Sangue**.
- Após casamento, pode ajudar a detectar pragas e venenos.

---

## 27. Tovin Mãos-de-Selo

```text
ID: npc_tovin
Gênero: homem
Idade narrativa: adulto
Raça: Gnomo
Subraça/origem: Gnomo Artífice
Classe clara: Scribe 5 / Expert
Estado relacionamento: MarriedToNpc
Cônjuge: Mara Vellum
Serviço: contratos, impostos, registros, Merithus
Visita à fazenda: sim, licenças e altares permitidos
HP: 58
MP: 40
FOR 1 | CON 2 | DES 4 | INT 6 | VON 4 | CAR 3
Resistências/status: +Medo burocrático, +Confusão legal; fraco contra dano direto
```

Aparência:

Gnomo pequeno, pele clara, cabelo castanho muito penteado, olhos enormes atrás de lentes redondas. Usa colete com carimbos, penas e selos em bolsos separados por finalidade.

Background:

Tovin acredita que todo problema tem formulário. Se não tem, a civilização falhou.

Relações:

- casado com Mara;
- irrita Nimble;
- admira Corvus;
- desconfia de Renko.

Quests:

1. **Carimbo de Propriedade**  
   Registrar oficialmente a fazenda expandida.  
   Recompensa: licença de expansão.

2. **Altar Permitido**  
   Explica regras de altares construíveis e bloqueia Anya como construção livre.  
   Recompensa: desbloqueio de altar permitido.

3. **Selo Sem Reino**  
   Um selo antigo não pertence à Dornécia atual.  
   Recompensa: pista de Bromécia/Elyndor.

---

## 28. Maelor Cinza

```text
ID: npc_maelor
Gênero: homem
Idade narrativa: adulto indefinido
Raça: Elfo
Subraça/origem: Elfo da Noite / Luandil
Classe clara: Monk 5 / Rogue 3
Estado relacionamento: LateRomanceEligible
Serviço: segredo, Nyx, observação, memória oculta
Visita à fazenda: sim, raro, à noite
HP: 105
MP: 50
FOR 3 | CON 4 | DES 6 | INT 5 | VON 5 | CAR 3
Resistências/status: +Medo, +Nyx, +Furtividade; fraco sob Senya e exposição pública
```

Aparência:

Elfo da Noite de pele cinza-carvão, cabelo azul-escuro quase negro, olhos prateados e tatuagens luminescentes discretas no pescoço. Usa roupas simples, sem metal aparente.

Background:

Maelor parece sempre saber onde a sombra será antes dela chegar. Ele não é vilão, mas não entende transparência como virtude.

Relações:

- conhece Yael;
- evita Corvus;
- escuta Liora;
- observa as estátuas antigas.

Quests:

1. **Passos Onde Não Há Luz**  
   Encontrar Maelor em uma rota noturna sem ser visto por guardas.  
   Recompensa: acesso a rumor de Nyx.

2. **Memória Que Escolheu Sumir**  
   Maelor revela que a cidade esqueceu algo de propósito.  
   Recompensa: pista de Anya/Cindar.

3. **O Silêncio Também Protege**  
   O jogador escolhe revelar ou preservar um segredo que pode ferir a cidade.  
   Recompensa: desbloqueia confiança de Maelor.

Romance:

- Quest de vínculo tardia: **O Nome que a Noite Não Levou**.
- Só disponível após resolver parte da trama de Nyx/Anya.

---

# PARTE D — Âncora de lore: Fonte de Ressurreição

## 29. Fonte de Ressurreição / Fonte de Anya

```text
ID: lore_anya_fountain
Tipo: Lore Anchor / Sistema
Local: fazenda
Função: ressurreição de companions, respec, Água Viva, cura rara, mistério de Anya
Romance: não
HP/Stats: n/a
```

Regras:

```text
É a única representação física de Anya na fazenda.
Não é uma estátua decorativa.
Não é altar construível.
Não pode ser substituída por construção comum.
Pode evoluir por progresso narrativo e sistema próprio.
```

Quests/sistemas ligados:

1. **Água que Lembra**  
   A Fonte recupera uma carga de Água Viva após evento de Alihana.

2. **Nome Apagado**  
   Thalindra, Liora ou Corvus reagem a símbolo próximo à Fonte.

3. **Ressurreição Dolorosa**  
   Companion caído retorna, mas com custo progressivo e possível diálogo.

---

# PARTE E — Próximos documentos/specs

```text
spec_city_npc_data_roster_stats.md
spec_city_relationship_romance_marriage.md
spec_city_farm_visits_schedule.md
spec_city_personal_quests_batch_01.md
spec_city_kanthor_temple_and_altars.md
spec_farm_fountain_anya_no_buildable_statue.md
```
