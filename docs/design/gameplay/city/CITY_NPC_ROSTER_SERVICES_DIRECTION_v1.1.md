# Cindar's Hope — City NPC Roster & Services Direction

> **Status:** direção ativa de elenco, serviços, romance, stats e quests da cidade  
> **Local:** `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`  
> **Depende de:** `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`  
> **Canon obrigatório:** `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`  
> **Fontes de referência:** `RACAS_DE_VAALARA_Guia_Completo.md`, `Deuses_de_Vaalara.md`, `DORNECIA_Guia_Completo.md`, `GDD_v2.6.md`  
> **Não é spec implementável.** Specs futuras devem ser quebradas em `docs/specs/a_implementar/`.

---

## 0. Correção estrutural

Este documento usa **classes funcionais do jogo**, não classes de D&D.

Classes como `Cleric`, `Paladin`, `Fighter`, `Wizard`, `Rogue`, `Bard`, etc. não são classes mecânicas de Cindar's Hope.

Elas podem inspirar tom narrativo, mas não devem aparecer como classe de gameplay.

---

# PARTE A — Taxonomia correta do jogo

## 1. Classes funcionais usadas no roster

| Classe funcional | Função no jogo |
|---|---|
| Plantador | planta sementes conforme área/plano permitido |
| Colhedor | coleta crops maduras |
| Pescador | pesca no lago/rios |
| Lenhador | corta árvores e coleta madeira |
| Minerador | coleta rochas/minérios em áreas válidas |
| Artesão | opera workshops e crafting autorizado |
| Explorador | vai à caverna buscar materiais e informações |
| Construtor | constrói, move e expande estruturas |
| Tratador | cuida de animais e pets |
| Comerciante | vende, compra, negocia, altera estoque/preço |
| Escriba | contratos, licenças, registros e reputação |
| Curandeiro | cura, antídotos, recuperação e suporte |
| Guardião | defesa, patrulha, proteção e escolta |
| Combatente | combate direto em eventos ou caverna |
| Pesquisador | lore, ruínas, tradução e tecnologia antiga |
| Músico | buffs sociais, eventos, sonhos e pistas |
| Alquimista | poções, fertilizantes, reagentes e transformação |

Cada NPC pode ter:

```text
Classe Primária
Classe Secundária
Tags de serviço
```

## 2. Atributos e stats usados

Atributos e stats do jogo:

```text
HP
MP
Stamina
Breath/Fôlego
Força
Constituição
Destreza
Inteligência
Vontade
Carisma
```

Uso:

| Stat | Função |
|---|---|
| HP | vida/sobrevivência |
| MP | Pontos de Magia, se aplicável |
| Stamina | energia para ações físicas |
| Breath/Fôlego | ritmo de ação, esforço contínuo, caverna e combate prolongado |
| Força | dano físico, coleta pesada, carga |
| Constituição | HP, resistência física, status |
| Destreza | movimento, esquiva, precisão |
| Inteligência | crafting, poções, pesquisa, técnica |
| Vontade | MP, resistência a medo, foco |
| Carisma | descontos, venda, influência social e companions |

Escala de atributos:

```text
1 = muito baixo
2 = baixo
3 = comum
4 = bom
5 = ótimo
6 = excepcional local
```

Status negativos considerados:

```text
Fome
Exaustão
Frio
Calor
Veneno
Medo
Morte
```

## 3. Religião pessoal dos NPCs

Cada NPC deve declarar:

```text
Deus cultuado/principal
Deuses com simpatia
Deuses de que não gosta/desconfia
```

Isso afeta:

- diálogo;
- reputação;
- presentes;
- festivais;
- reações a altares do jogador;
- conflitos entre NPCs;
- quests pessoais;
- reações à Fonte de Anya.

---

# PARTE B — Regras sociais

## 4. Estados de relacionamento

```text
MarriedToNpc
RomanceEligibleAnyPlayerGender
UnavailableForRomance
TooYoungOrNarrativelyBlocked
LateRomanceEligible
```

Quando um NPC é `RomanceEligibleAnyPlayerGender`, ele pode se relacionar e casar com o jogador independentemente do gênero escolhido pelo jogador.

## 5. Casais fixos

| Casal | Status |
|---|---|
| Nimble Galhobaixo + Mirela dos Laços | casados |
| Gruta Panela-Funda + Orlan Pouso-Curto | casados |
| Mara Vellum + Tovin Mãos-de-Selo | casados |

## 6. Candidatos a relacionamento/casamento

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

---

# PARTE C — Tabela-mestra

| ID | Nome | Raça/subraça | Classe primária | Classe secundária | Romance | Deus principal |
|---|---|---|---|---|---|---|
| npc_corvus | Padre Corvus | Humano de Mana | Curandeiro | Guardião | não | Kanthor |
| npc_mara | Mara Vellum | Humana de Mana | Escriba | Comerciante | casada | Merithus |
| npc_sylveth | Sylveth | Elfa Silvestre | Plantador | Curandeiro | sim | Thandra |
| npc_brumdar | Brumdar Ferro-Quieto | Anão de Khaz Baruk | Artesão | Combatente | não | Thoren |
| npc_nimble | Nimble Galhobaixo | Halfling Andarilho de Méritos | Construtor | Artesão | casado | Merithus |
| npc_gurd | Gurd Carvalho-Torto | Meio-orc Clã da Fúria | Construtor | Combatente | não | Kaand |
| npc_hund | Hund Carvalho-Torto | Meio-orc Clã da Noite | Guardião | Construtor | não | Kanthor |
| npc_ozzra | Ozzra Fumaçazul | Goblin Zhak'thul | Alquimista | Artesão | sim | Senya |
| npc_gruta | Gruta Panela-Funda | Orc Clã da Chama Viva | Comerciante | Músico | casada | Senya |
| npc_zrix | Zrix das Estradas | Draconato Cinza do Julgamento, cobre | Explorador | Comerciante | sim | Finan |
| npc_yael | Yael Noite-Mansa | Elfa da Noite / Luandil | Comerciante | Explorador | sim | Nyx |
| npc_thalindra | Thalindra Véu-de-Lua | Ninrorin | Pesquisador | Alquimista | sim | Alihana |
| npc_dagna | Dagna Rocha-Morna | Anã de Khaz Baruk | Minerador | Combatente | sim | Thoren |
| npc_pip | Pip Semente-Solta | Halfling Sortudo de Finan | Comerciante | Explorador | não | Finan |
| npc_alaric | Ser Alaric Veyr | Humano de Mana | Guardião | Combatente | sim | Kanthor |
| npc_mirela | Mirela dos Laços | Humana de Mana | Artesão | Comerciante | casada | Merithus |
| npc_renko | Renko Três-Sorrisos | Goblin Zhak'thul | Comerciante | Artesão | não | Finan |
| npc_eiran | Eiran Valeclaro | Meio-elfo de Thandra | Tratador | Plantador | sim | Thandra |
| npc_liora | Liora Canta-Rio | Humana de Mana com sangue nymiriano distante | Músico | Pesquisador | sim | Alihana |
| npc_orlan | Orlan Pouso-Curto | Humano de Mana | Comerciante | Escriba | casado | Finan |
| npc_savra | Savra Escama-Verde | Draconata verde | Curandeiro | Explorador | sim | Tandra/Telisandra |
| npc_tovin | Tovin Mãos-de-Selo | Gnomo Artífice | Escriba | Artesão | casado | Merithus |
| npc_maelor | Maelor Cinza | Elfo da Noite / Luandil | Explorador | Pesquisador | tardio | Nyx |

---

# PARTE D — NPCs detalhados

## 7. Padre Corvus

```text
ID: npc_corvus
Gênero: homem
Raça/subraça: Humano de Mana, dornécio
Classe Primária: Curandeiro
Classe Secundária: Guardião
Tags: kanthor, templo, juramento, cura, ordem
Relacionamento: UnavailableForRomance
Deus cultuado: Kanthor
Simpatia: Merithus, Thoren
Não gosta/desconfia: Nyx, Kaand, cultos de Senya
Visita fazenda: sim, por juramento, alerta, proteção ou investigação
HP 120 | MP 45 | Stamina 70 | Breath 65
Força 3 | Constituição 4 | Destreza 2 | Inteligência 4 | Vontade 6 | Carisma 5
Resiste: Medo, Exaustão leve
Vulnerável: Veneno, dúvida moral prolongada
```

Aparência:

Humano de Mana de pele morena clara, cabelos grisalhos presos curtos, olhos castanhos firmes. Usa vestes brancas e douradas de Kanthor com balança de prata no peito.

Background:

Corvus é a face pública da ordem. Sabe que o templo de Kanthor foi construído sobre fundações antigas, mas não entende toda a ligação com Anya.

Relações:

- confia em Ser Alaric;
- respeita Mara;
- desconfia de Yael;
- evita Liora quando ela fala de sonhos.

Quests:

1. **O Sino que Não Toca** — reparar o sino rachado do templo com ajuda de Brumdar. Recompensa: bênção menor de Kanthor.
2. **Juramento Partido** — investigar contrato falsificado entre Mara, Tovin e Renko. Recompensa: contratos melhores.
3. **O Nome Sob a Pedra** — decidir se uma inscrição antiga sob o templo deve ser ocultada ou revelada. Recompensa: avanço de lore Anya/Cindar ou reputação com Kanthor.

---

## 8. Mara Vellum

```text
ID: npc_mara
Gênero: mulher
Raça/subraça: Humana de Mana, dornécia
Classe Primária: Escriba
Classe Secundária: Comerciante
Tags: cartorio, licenca, contrato, reputacao, merithus
Relacionamento: MarriedToNpc
Cônjuge: Tovin Mãos-de-Selo
Deus cultuado: Merithus/Meritos
Simpatia: Kanthor, Finan quando dentro da lei
Não gosta/desconfia: Kaand, Nyx, Senya sem controle
Visita fazenda: sim, inspeção e licenças
HP 75 | MP 20 | Stamina 55 | Breath 45
Força 1 | Constituição 2 | Destreza 3 | Inteligência 6 | Vontade 5 | Carisma 4
Resiste: Medo burocrático, Manipulação social
Vulnerável: Combate direto, Veneno
```

Aparência:

Humana de Mana de pele oliva, cabelos pretos em coque severo, olhos escuros atentos e casaco azul-ardósia com botões de bronze.

Background:

Mara mantém a cidade de pé por registros, licenças e contratos. Acredita que civilização é aquilo que pode ser assinado e cobrado.

Relações:

- casada com Tovin;
- trabalha com Corvus;
- discute com Nimble;
- suspeita de Renko.

Quests:

1. **Licença de Primeira Obra** — registrar a primeira construção da fazenda. Recompensa: desbloqueio de construção formal.
2. **As Páginas Arrancadas** — recuperar registros antigos removidos do cartório. Recompensa: flag de segredo urbano.
3. **Lei ou Compaixão** — resolver caso de família sem recursos para licença. Recompensa: reputação variável.

---

## 9. Sylveth

```text
ID: npc_sylveth
Gênero: mulher
Raça/subraça: Elfa Silvestre
Classe Primária: Plantador
Classe Secundária: Curandeiro
Tags: sementes, thandra, crops, estacoes, ervas
Relacionamento: RomanceEligibleAnyPlayerGender
Deus cultuado: Thandra
Simpatia: Alihana, Anya como mistério de cura
Não gosta/desconfia: Kaand, tecnologia bromeciana sem cuidado
Visita fazenda: sim, crops, sementes e eventos de Thandra
HP 82 | MP 55 | Stamina 80 | Breath 75
Força 2 | Constituição 3 | Destreza 5 | Inteligência 4 | Vontade 5 | Carisma 4
Resiste: Veneno vegetal, Exaustão leve
Vulnerável: Calor extremo, Fogo
```

Aparência:

Elfa Silvestre de pele bronze-cobre, cabelos castanho-musgo trançados com folhas secas e olhos verde-âmbar. Usa roupas naturais em tons de terra e verde.

Background:

Sylveth cuida da loja de sementes e mantém vivos os ritos rurais de Thandra.

Relações:

- amiga de Eiran;
- fornece ervas para Ozzra;
- respeita Savra;
- acha Thalindra distante da terra.

Quests:

1. **Sementes de Retorno** — recuperar sementes antigas em trilha tomada por ervas agressivas. Recompensa: nova semente da estação.
2. **Festival de Thandra** — fornecer crops de qualidade para a feira rural. Recompensa: fertilizante simples e reputação rural.
3. **Raízes que Ouvem** — investigar plantas reagindo à Fonte e às estátuas antigas. Recompensa: pista sobre Anya.

Romance:

- Quest de vínculo: **A Terra Escolhe Devagar**.
- Após casamento: visita a fazenda e ajuda com crops em eventos específicos, sem automatizar tudo.

---

## 10. Brumdar Ferro-Quieto

```text
ID: npc_brumdar
Gênero: homem
Raça/subraça: Anão de Khaz Baruk
Classe Primária: Artesão
Classe Secundária: Combatente
Tags: forja, ferramentas, armas, minerio, thoren
Relacionamento: UnavailableForRomance
Deus cultuado: Thoren
Simpatia: Kanthor, Merithus
Não gosta/desconfia: Senya, Nyx, Finan quando vira trapaça
Visita fazenda: sim, upgrades e ferramentas
HP 150 | MP 10 | Stamina 90 | Breath 80
Força 5 | Constituição 6 | Destreza 2 | Inteligência 4 | Vontade 5 | Carisma 2
Resiste: Calor, Medo, Exaustão
Vulnerável: Medo mental/ilusões
```

Aparência:

Anão robusto de pele curtida, barba cinza em três tranças com argolas de ferro, braços grossos e mãos queimadas pela forja.

Background:

Brumdar veio por causa dos minérios estranhos da caverna. Não confia em metal sem procedência.

Relações:

- amigo de Dagna;
- rival técnico de Ozzra;
- respeita Corvus;
- desconfia de Zrix quando ele traz material sem origem clara.

Quests:

1. **A Primeira Ferramenta Séria** — melhorar ferramenta com cobre/ferro. Recompensa: primeiro upgrade.
2. **Metal que Sussurra** — analisar lingote que vibra perto de Pedra Negra. Recompensa: minério especial rastreado.
3. **O Martelo do Aprendiz** — recuperar martelo antigo em galeria. Recompensa: receita de ferramenta intermediária.

---

## 11. Nimble Galhobaixo

```text
ID: npc_nimble
Gênero: homem
Raça/subraça: Halfling Andarilho de Méritos
Classe Primária: Construtor
Classe Secundária: Artesão
Tags: carpintaria, construcao, layout, mover, casa, sellpoint
Relacionamento: MarriedToNpc
Cônjuge: Mirela dos Laços
Deus cultuado: Merithus/Meritos
Simpatia: Finan, Thandra
Não gosta/desconfia: Kaand, burocracia excessiva mesmo cultuando Merithus
Visita fazenda: sim, obras e movimentação de estruturas
HP 70 | MP 10 | Stamina 75 | Breath 70
Força 2 | Constituição 3 | Destreza 5 | Inteligência 5 | Vontade 3 | Carisma 4
Resiste: Exaustão leve, Medo por sorte
Vulnerável: Dano direto
```

Aparência:

Halfling de cabelos castanhos claros, costeletas compridas, pés peludos sempre sujos de serragem e colete cheio de bolsos.

Background:

Nimble adora medir, mover, construir e reclamar de layout ruim.

Relações:

- casado com Mirela;
- trabalha com Gurd e Hund;
- briga com Mara por licenças;
- gosta de Gruta.

Quests:

1. **Madeira, Pedra e Assinatura** — tutorial de construção. Recompensa: primeira construção secundária.
2. **Mover é Mais Difícil que Erguer** — liberar movimentação de casa/SellPoint. Recompensa: modo mover construção.
3. **A Tábua que Cura** — decidir destino de madeira auto-reparável bromeciana. Recompensa: pista tecnológica.

---

## 12. Gurd Carvalho-Torto

```text
ID: npc_gurd
Gênero: homem
Raça/subraça: Meio-orc Clã da Fúria
Classe Primária: Construtor
Classe Secundária: Combatente
Tags: obra, forca, limpeza, expansao
Relacionamento: UnavailableForRomance
Deus cultuado: Kaand
Simpatia: Thoren, Senya
Não gosta/desconfia: Kanthor quando vira controle, Merithus
Visita fazenda: sim, obras e expansão
HP 145 | MP 0 | Stamina 95 | Breath 85
Força 6 | Constituição 5 | Destreza 2 | Inteligência 2 | Vontade 3 | Carisma 3
Resiste: Medo, Exaustão
Vulnerável: Controle mental, Veneno
```

Aparência:

Meio-orc de pele cinza-avermelhada, presas evidentes, corpo largo, tatuagens geométricas e cicatrizes nos ombros.

Background:

Gurd resolve problemas carregando, quebrando ou encarando. Direto, mas não cruel.

Relações:

- irmão de Hund;
- trabalha com Nimble;
- bebe na taverna de Gruta;
- respeita Brumdar.

Quests:

1. **Pedra Grande, Martelo Maior** — remover obstáculos da fazenda. Recompensa: limpeza pesada.
2. **A Parede que Não Quebrou** — investigar parede antiga resistente. Recompensa: pista de ruína.
3. **Força sem Fúria** — resolver briga em festival de Senya. Recompensa: relação e job de força.

---

## 13. Hund Carvalho-Torto

```text
ID: npc_hund
Gênero: homem
Raça/subraça: Meio-orc Clã da Noite
Classe Primária: Guardião
Classe Secundária: Construtor
Tags: defesa, transporte, obra, vigilancia
Relacionamento: UnavailableForRomance
Deus cultuado: Kanthor
Simpatia: Nyx, Thoren
Não gosta/desconfia: Kaand sem disciplina, Senya caótica
Visita fazenda: sim, obras e ameaça
HP 135 | MP 10 | Stamina 85 | Breath 90
Força 5 | Constituição 5 | Destreza 3 | Inteligência 3 | Vontade 4 | Carisma 2
Resiste: Medo, Frio noturno
Vulnerável: Calor extremo
```

Aparência:

Meio-orc de pele cinza-azulada, presas menores, olhos escuros atentos, cabelo raspado nas laterais e casaco de couro pesado.

Background:

Hund observa antes de agir. Percebe ruídos, sombras e rotas de fuga.

Relações:

- irmão de Gurd;
- protetor de Pip;
- respeita Ser Alaric;
- desconfia de Maelor.

Quests:

1. **A Entrega que Não Chegou** — recuperar carga de madeira sumida. Recompensa: materiais.
2. **Barulho no Poço** — investigar ruído sob a cidade. Recompensa: flag de subsolo.
3. **Guardar sem Mandar** — montar patrulha informal entre cidade e fazenda. Recompensa: proteção leve.

---

## 14. Ozzra Fumaçazul

```text
ID: npc_ozzra
Gênero: mulher
Raça/subraça: Goblin Zhak'thul, clã do Grito Livre
Classe Primária: Alquimista
Classe Secundária: Artesão
Tags: alquimia, pocao, fertilizante, reagente, senya
Relacionamento: RomanceEligibleAnyPlayerGender
Deus cultuado: Senya
Simpatia: Finan, Nyx, Anya como energia curativa misteriosa
Não gosta/desconfia: Kanthor rígido, Merithus quando trava experimento
Visita fazenda: sim, testes autorizados
HP 68 | MP 60 | Stamina 70 | Breath 60
Força 1 | Constituição 3 | Destreza 5 | Inteligência 6 | Vontade 3 | Carisma 4
Resiste: Veneno, Calor leve
Vulnerável: Medo institucional, Frio
```

Aparência:

Goblin de pele laranja-acinzentada, orelhas grandes, cabelo azul espetado por tintura alquímica, olhos amarelos vivos e avental cheio de frascos.

Background:

Ozzra enxerga cada coisa como mistura possível. Seu laboratório é ameaça pública parcialmente útil.

Relações:

- deve dinheiro a Renko;
- rivaliza com Brumdar;
- compra ervas de Sylveth e Savra;
- acha Yael fascinante.

Quests:

1. **Explode Só um Pouco** — tutorial de poção. Recompensa: receita de poção simples.
2. **Fertilizante Fumaçazul** — testar fertilizante na fazenda. Recompensa: fertilizante avançado.
3. **Reagente que Bebe Luz** — analisar cristal que drena luz/cansaço. Recompensa: pista de Pedra Negra.

Romance:

- Quest de vínculo: **A Fórmula do Afeto Improvável**.
- Após casamento: bancada de alquimia na fazenda com regras de segurança.

---

## 15. Gruta Panela-Funda

```text
ID: npc_gruta
Gênero: mulher
Raça/subraça: Orc Clã da Chama Viva
Classe Primária: Comerciante
Classe Secundária: Músico
Tags: taverna, comida, rumores, buffs, senya
Relacionamento: MarriedToNpc
Cônjuge: Orlan Pouso-Curto
Deus cultuado: Senya
Simpatia: Finan, Thandra
Não gosta/desconfia: Merithus burocrático, Nyx quando ameaça clientes
Visita fazenda: sim, ingredientes/evento social
HP 130 | MP 15 | Stamina 90 | Breath 80
Força 5 | Constituição 5 | Destreza 2 | Inteligência 3 | Vontade 4 | Carisma 5
Resiste: Calor, Medo
Vulnerável: Frio
```

Aparência:

Orc de pele verde-escura com reflexos quentes, presas fortes, cabelo ruivo em trança grossa e avental vermelho gasto.

Background:

Gruta comanda a taverna como quartel acolhedor. Alimenta, intimida e escuta todos.

Relações:

- casada com Orlan;
- protege Pip;
- troca rumores com Zrix;
- provoca Corvus.

Quests:

1. **Sopa para um Dia Ruim** — coletar ingredientes para comida que reduz cansaço. Recompensa: receita.
2. **Rumor Queimado** — investigar símbolos sob mesas. Recompensa: pista de culto.
3. **Banquete de Festival** — preparar comida de festival. Recompensa: buff social.

---

## 16. Zrix das Estradas

```text
ID: npc_zrix
Gênero: homem
Raça/subraça: Draconato Cinza do Julgamento, cobre
Classe Primária: Explorador
Classe Secundária: Comerciante
Tags: guilda, estrada, mapa, caverna, checkpoint, finan
Relacionamento: RomanceEligibleAnyPlayerGender
Deus cultuado: Finan
Simpatia: Kanthor, Thoren
Não gosta/desconfia: Nyx quando esconde rota, Kaand quando cria guerra inútil
Visita fazenda: sim, contratos/mapas
HP 125 | MP 25 | Stamina 85 | Breath 95
Força 4 | Constituição 5 | Destreza 4 | Inteligência 4 | Vontade 4 | Carisma 4
Resiste: Medo, Calor leve
Vulnerável: Frio intenso
```

Aparência:

Draconato de escamas cobre escurecido com placas cinza, chifres curtos, olhos âmbar e cauda marcada por cicatrizes de estrada.

Background:

Zrix conhece rotas e atalhos. Trata a caverna como estrada ruim: perigosa, mas mapeável.

Relações:

- troca rumores com Gruta;
- negocia com Renko;
- respeita Alaric;
- evita Yael em público.

Quests:

1. **Mapa de Entrada** — tutorial da Guilda e primeiros contratos. Recompensa: mapa parcial.
2. **Sinal de Elyndor** — identificar símbolo de portal. Recompensa: investigação de checkpoint.
3. **A Estrada que Desce** — contrato em nível perigoso. Recompensa: possível companion Explorador.

Romance:

- Quest de vínculo: **O Caminho de Volta**.
- Após casamento: mantém Guilda e visita a fazenda em dias definidos.

---

## 17. Yael Noite-Mansa

```text
ID: npc_yael
Gênero: mulher
Raça/subraça: Elfa da Noite / Luandil
Classe Primária: Comerciante
Classe Secundária: Explorador
Tags: loja_noturna, nyx, segredo, item_raro
Relacionamento: RomanceEligibleAnyPlayerGender
Deus cultuado: Nyx
Simpatia: Finan, Alihana
Não gosta/desconfia: Kanthor quando persegue sombra, Merithus quando cataloga demais
Visita fazenda: sim, rara, à noite
HP 88 | MP 45 | Stamina 80 | Breath 75
Força 2 | Constituição 3 | Destreza 6 | Inteligência 5 | Vontade 4 | Carisma 5
Resiste: Medo, Exaustão noturna
Vulnerável: Senya/festa intensa, exposição pública
```

Aparência:

Elfa da Noite de pele azul-noite, cabelos branco-prateados, olhos violeta brilhando no escuro e tatuagens luminescentes discretas.

Background:

Yael vende itens que “não existem” para pessoas que “não perguntaram”. Não é vilã, mas sabe lucrar com segredos.

Relações:

- Corvus desconfia dela;
- conhece Maelor;
- Ozzra quer estudar seus itens;
- Renko finge não ter negócios com ela.

Quests:

1. **Aberto Depois da Meia-Noite** — encontrar loja noturna. Recompensa: estoque de Nyx.
2. **Comprador de Fragmentos** — investigar compra de pedras escuras. Recompensa: avanço de culto.
3. **Preço do Silêncio** — proteger ou revelar segredo de Yael. Recompensa: item raro noturno.

Romance:

- Quest de vínculo: **Confiança no Escuro**.
- Após casamento: mantém rotina noturna.

---

## 18. Thalindra Véu-de-Lua

```text
ID: npc_thalindra
Gênero: mulher
Raça/subraça: Ninrorin, elfa cinzenta
Classe Primária: Pesquisador
Classe Secundária: Alquimista
Tags: arquivo, lore, cindar, anya, bromecia, elyndor
Relacionamento: RomanceEligibleAnyPlayerGender
Deus cultuado: Alihana
Simpatia: Anya, Merithus
Não gosta/desconfia: Senya, Kaand, Finan quando distorce registro
Visita fazenda: sim, por Fonte/inscrição/ruína
HP 72 | MP 80 | Stamina 55 | Breath 50
Força 1 | Constituição 2 | Destreza 3 | Inteligência 6 | Vontade 5 | Carisma 3
Resiste: Medo arcano, Frio mental
Vulnerável: Dano físico, Exaustão
```

Aparência:

Ninrorin de pele cinza clara, cabelos prata lisos, olhos azul-gelo e túnica azul-cobalto com fios prateados.

Background:

Thalindra quer a versão correta dos fatos, mesmo quando a verdade ameaça a cidade.

Relações:

- debate com Corvus;
- depende de Mara;
- ouve Liora com interesse cético;
- teme Bromécia, mas pesquisa mesmo assim.

Quests:

1. **Poeira no Arquivo** — restaurar registros. Recompensa: acesso ao arquivo restrito.
2. **A Palavra Nymiriana** — traduzir inscrição antiga. Recompensa: pista de Anya/Cindar.
3. **O Mapa Que Não Deveria Existir** — conectar cidade e caverna. Recompensa: pista Elyndor/Bromécia.

Romance:

- Quest de vínculo: **O Que a História Não Diz**.
- Após casamento: mesa de pesquisa na fazenda.

---

## 19. Dagna Rocha-Morna

```text
ID: npc_dagna
Gênero: mulher
Raça/subraça: Anã de Khaz Baruk
Classe Primária: Minerador
Classe Secundária: Combatente
Tags: minerio, caverna, pedreira, thoren
Relacionamento: RomanceEligibleAnyPlayerGender
Deus cultuado: Thoren
Simpatia: Kanthor, Thandra
Não gosta/desconfia: Nyx, Senya, cultos subterrâneos
Visita fazenda: sim, pedreira/rochas/minério
HP 155 | MP 10 | Stamina 95 | Breath 90
Força 5 | Constituição 6 | Destreza 2 | Inteligência 4 | Vontade 5 | Carisma 3
Resiste: Calor, Exaustão, Medo
Vulnerável: Medo mental/ilusões
```

Aparência:

Anã robusta de pele bronzeada, cabelo preto com mechas brancas, barba curta trançada e olhos castanho-escuros.

Background:

Dagna conhece pedra viva, pedra morta e pedra que mente.

Relações:

- amiga de Brumdar;
- discorda de Zrix sobre risco;
- respeita Alaric;
- suspeita de Renko.

Quests:

1. **Veio de Cobre** — ensinar avaliação de minério. Recompensa: preço melhor por minério.
2. **Galeria Sem Nome** — procurar galeria onde perdeu parente. Recompensa: sala especial.
3. **A Rocha Quente Demais** — remover pedra estranha da fazenda. Recompensa: pista da pedreira final.

Romance:

- Quest de vínculo: **Pedra Também Guarda Luto**.
- Após casamento: avalia nodes da fazenda/caverna.

---

## 20. Pip Semente-Solta

```text
ID: npc_pip
Gênero: homem
Raça/subraça: Halfling Sortudo de Finan
Classe Primária: Comerciante
Classe Secundária: Explorador
Tags: tutorial, entrega, recado, humor, finan
Relacionamento: TooYoungOrNarrativelyBlocked
Deus cultuado: Finan
Simpatia: Thandra, Senya
Não gosta/desconfia: Kanthor quando dá sermão, Nyx quando assusta
Visita fazenda: sim, cedo e frequentemente
HP 45 | MP 0 | Stamina 65 | Breath 55
Força 1 | Constituição 2 | Destreza 5 | Inteligência 2 | Vontade 2 | Carisma 5
Resiste: Sorte contra Medo leve
Vulnerável: Combate real, Veneno
```

Aparência:

Halfling pequeno, cabelos ruivos bagunçados, sardas, pés peludos dourados e mochila grande demais.

Background:

Pip corre pela cidade levando cartas, compras e confusão.

Relações:

- protegido por Gruta e Hund;
- ajuda Sylveth;
- admira Alaric;
- teme e admira Yael.

Quests:

1. **Primeira Entrega** — levar jogador à loja de sementes. Recompensa: tutorial social.
2. **Vi o Fantasma** — relatar figura no Jardim das Estátuas. Recompensa: pista noturna.
3. **A Carta Errada** — carta entregue no destino errado revela tensão. Recompensa: pistas urbanas.

---

## 21. Ser Alaric Veyr

```text
ID: npc_alaric
Gênero: homem
Raça/subraça: Humano de Mana, dornécio
Classe Primária: Guardião
Classe Secundária: Combatente
Tags: guarda, patrulha, kanthor, seguranca, combate
Relacionamento: RomanceEligibleAnyPlayerGender
Deus cultuado: Kanthor
Simpatia: Thoren, Merithus
Não gosta/desconfia: Nyx, Finan oportunista, Kaand
Visita fazenda: sim, alerta e ameaça
HP 140 | MP 15 | Stamina 90 | Breath 85
Força 5 | Constituição 5 | Destreza 4 | Inteligência 3 | Vontade 5 | Carisma 4
Resiste: Medo, Exaustão
Vulnerável: Ilusão, Veneno
```

Aparência:

Humano alto, pele bronzeada, cabelo castanho curto, barba aparada, olhos verdes e meia-armadura azul-ardósia com símbolo de Kanthor.

Background:

Alaric acredita em patrulha, lâmina e responsabilidade.

Relações:

- leal a Corvus;
- trabalha com Mara;
- respeita Dagna;
- não confia em Yael.

Quests:

1. **Patrulha da Estrada Baixa** — afastar criaturas perto da caverna. Recompensa: reputação com guarda.
2. **Relatório Incompleto** — decidir se encobre incidente para evitar pânico. Recompensa variável.
3. **A Justiça Não Basta** — lidar com suspeito inocente, mas envolvido em segredo. Recompensa: bifurcação moral.

Romance:

- Quest de vínculo: **O Peso do Escudo**.
- Após casamento: protege a fazenda em eventos de ameaça.

---

## 22. Mirela dos Laços

```text
ID: npc_mirela
Gênero: mulher
Raça/subraça: Humana de Mana
Classe Primária: Artesão
Classe Secundária: Comerciante
Tags: costura, bolsa, roupa, acessorio, merithus
Relacionamento: MarriedToNpc
Cônjuge: Nimble Galhobaixo
Deus cultuado: Merithus/Meritos
Simpatia: Finan, Thandra
Não gosta/desconfia: Kaand, Senya quando vira desordem
Visita fazenda: sim, lã/tecido/encomenda
HP 68 | MP 15 | Stamina 60 | Breath 55
Força 1 | Constituição 2 | Destreza 5 | Inteligência 5 | Vontade 3 | Carisma 5
Resiste: Medo social, Frio leve por roupas
Vulnerável: Combate direto
```

Aparência:

Humana de pele morena, cabelos cacheados escuros com fitas coloridas, olhos castanhos vivos e roupas bem ajustadas.

Background:

Mirela vê roupa como ferramenta social e mecânica.

Relações:

- casada com Nimble;
- amiga de Gruta;
- compra lã de Eiran;
- irrita Mara com licenças estéticas.

Quests:

1. **Bolsa de Trabalho** — criar upgrade de inventário. Recompensa: bolsa maior.
2. **Fio que Não Rasga** — investigar tecido auto-reparável. Recompensa: roupa especial futura.
3. **Roupa de Festival** — preparar traje. Recompensa: buff social.

---

## 23. Renko Três-Sorrisos

```text
ID: npc_renko
Gênero: homem
Raça/subraça: Goblin Zhak'thul, comerciante integrado
Classe Primária: Comerciante
Classe Secundária: Artesão
Tags: loja_geral, barganha, item_comum, finan
Relacionamento: UnavailableForRomance
Deus cultuado: Finan
Simpatia: Senya, Merithus quando dá lucro
Não gosta/desconfia: Kanthor, Thoren rígido demais
Visita fazenda: sim, mercador ambulante em reputação alta
HP 65 | MP 10 | Stamina 70 | Breath 55
Força 1 | Constituição 3 | Destreza 5 | Inteligência 5 | Vontade 2 | Carisma 6
Resiste: Barganha, Medo leve
Vulnerável: Contratos rígidos de Kanthor/Merithus
```

Aparência:

Goblin de pele laranja-acinzentada, olhos amarelos semicerrados, dentes pequenos e sorriso constante.

Background:

Renko compra, vende e sorri. Considera preço fixo uma ofensa criativa.

Relações:

- Ozzra deve dinheiro a ele;
- negocia com Zrix;
- teme multas de Mara;
- vende quinquilharias para Pip.

Quests:

1. **Preço de Amigo** — tutorial de loja geral. Recompensa: desconto pequeno.
2. **Mercadoria Sem Dono** — decidir destino de item perigoso. Recompensa: pista de Pedra Negra.
3. **Três Sorrisos, Uma Mentira** — descobrir qual versão dele é real. Recompensa: estoque raro.

---

## 24. Eiran Valeclaro

```text
ID: npc_eiran
Gênero: homem
Raça/subraça: Meio-elfo de Thandra
Classe Primária: Tratador
Classe Secundária: Plantador
Tags: animais, pets, racao, thandra
Relacionamento: RomanceEligibleAnyPlayerGender
Deus cultuado: Thandra
Simpatia: Tandra/Telisandra, Anya como cura
Não gosta/desconfia: Ozzra perto de animais, Kaand
Visita fazenda: sim, animal/pet/tratamento
HP 95 | MP 30 | Stamina 80 | Breath 75
Força 3 | Constituição 4 | Destreza 4 | Inteligência 3 | Vontade 5 | Carisma 4
Resiste: Medo animal, Veneno leve
Vulnerável: Ruído/caos urbano
```

Aparência:

Meio-elfo de pele bronzeada, orelhas levemente pontudas, cabelo castanho longo preso com cordão verde e olhos âmbar calmos.

Background:

Eiran entende animais melhor que pessoas. Calmo, firme e observador.

Relações:

- amigo de Sylveth;
- respeita Savra;
- compra tecido de Mirela;
- evita Ozzra perto dos animais.

Quests:

1. **Primeira Tigela** — tutorial de pet. Recompensa: desbloqueio de pet.
2. **Animal Assustado** — investigar trilha evitada por animais. Recompensa: pista natural.
3. **Cuidado Não é Fraqueza** — tratar animal doente. Recompensa: remédio animal.

Romance:

- Quest de vínculo: **Ficar é Também Cuidar**.
- Após casamento: ajuda com pets/animais sem ocupar slot de companion.

---

## 25. Liora Canta-Rio

```text
ID: npc_liora
Gênero: mulher
Raça/subraça: Humana de Mana com sangue nymiriano distante
Classe Primária: Músico
Classe Secundária: Pesquisador
Tags: musica, sonho, alihana, anya, pistas
Relacionamento: RomanceEligibleAnyPlayerGender
Deus cultuado: Alihana
Simpatia: Anya, Senya em arte
Não gosta/desconfia: Nyx quando silencia memórias, Kaand
Visita fazenda: sim, Alihana/relação alta
HP 70 | MP 70 | Stamina 55 | Breath 65
Força 1 | Constituição 2 | Destreza 4 | Inteligência 4 | Vontade 6 | Carisma 6
Resiste: Medo por sonho, Exaustão mental leve
Vulnerável: Nyx/cansaço mental
```

Aparência:

Humana de pele dourada clara, cabelos castanho-escuros ondulados, olhos azulados incomuns e voz suave.

Background:

Liora canta músicas que não aprendeu. Algumas reagem ao Jardim das Estátuas e à Fonte.

Relações:

- Gruta dá espaço para ela cantar;
- Corvus observa com cautela;
- Thalindra registra suas canções;
- Maelor reconhece uma melodia.

Quests:

1. **Canção sem Autor** — encontrar origem de melodia. Recompensa: evento de Alihana.
2. **A Estátua que Escutou** — música reage à estátua antiga. Recompensa: pista de Anya.
3. **Sonho de Água Clara** — comparar sonho com mapa real. Recompensa: pista de Água Viva.

Romance:

- Quest de vínculo: **A Voz que Fica**.
- Após casamento: eventos musicais raros na fazenda.

---

## 26. Orlan Pouso-Curto

```text
ID: npc_orlan
Gênero: homem
Raça/subraça: Humano de Mana, dornécio
Classe Primária: Comerciante
Classe Secundária: Escriba
Tags: hospedagem, viajante, noticia, finan
Relacionamento: MarriedToNpc
Cônjuge: Gruta Panela-Funda
Deus cultuado: Finan
Simpatia: Kanthor, Merithus
Não gosta/desconfia: Nyx, Kaand
Visita fazenda: raro, evento social
HP 80 | MP 5 | Stamina 60 | Breath 55
Força 2 | Constituição 3 | Destreza 2 | Inteligência 4 | Vontade 4 | Carisma 5
Resiste: Exaustão social, Medo leve
Vulnerável: Combate real
```

Aparência:

Humano baixo e largo, pele clara, cabelo castanho ralo, bigode curto e olhos gentis. Carrega chaves no cinto.

Background:

Orlan sabe quem chegou, quem partiu e quem mentiu sobre isso.

Relações:

- casado com Gruta;
- informa Mara;
- respeita Corvus;
- desconfia de Maelor.

Quests:

1. **Quarto de Viajante** — preparar hospedagem para caravana. Recompensa: notícia de estrada.
2. **Hóspede Sem Sombra** — investigar viajante desaparecido. Recompensa: mapa incompleto.
3. **Conta Aberta** — cobrar dívida sem conflito. Recompensa: reputação social.

---

## 27. Savra Escama-Verde

```text
ID: npc_savra
Gênero: mulher
Raça/subraça: Draconata verde
Classe Primária: Curandeiro
Classe Secundária: Explorador
Tags: ervas, antidoto, floresta, veneno, telisandra
Relacionamento: RomanceEligibleAnyPlayerGender
Deus cultuado: Tandra/Telisandra
Simpatia: Thandra, Nyx em trilhas noturnas
Não gosta/desconfia: Kanthor quando simplifica natureza, Kaand predatório
Visita fazenda: sim, ervas/pragas/plantas estranhas
HP 118 | MP 35 | Stamina 85 | Breath 80
Força 4 | Constituição 5 | Destreza 4 | Inteligência 4 | Vontade 4 | Carisma 3
Resiste: Veneno, Calor úmido
Vulnerável: Frio intenso
```

Aparência:

Draconata de escamas verde-escuras com manchas oliva, olhos amarelo-ouro, chifres curtos voltados para trás e cauda fina.

Background:

Savra entende venenos, trilhas e plantas hostis. Rejeita a ideia de que linhagem verde define caráter.

Relações:

- troca ervas com Sylveth;
- vende reagentes a Ozzra;
- irrita Corvus com sarcasmo;
- é respeitada por Eiran.

Quests:

1. **Antídoto Amargo** — coletar ervas para antídoto. Recompensa: receita de antídoto.
2. **Praga que Anda** — investigar praga vegetal noturna. Recompensa: defesa contra pragas.
3. **O Fungo de Baixo** — identificar fungos de caverna. Recompensa: pista de bioma subterrâneo.

Romance:

- Quest de vínculo: **Não Sou Meu Sangue**.
- Após casamento: ajuda a detectar pragas e venenos.

---

## 28. Tovin Mãos-de-Selo

```text
ID: npc_tovin
Gênero: homem
Raça/subraça: Gnomo Artífice
Classe Primária: Escriba
Classe Secundária: Artesão
Tags: contrato, imposto, registro, altar, merithus
Relacionamento: MarriedToNpc
Cônjuge: Mara Vellum
Deus cultuado: Merithus/Meritos
Simpatia: Kanthor, Finan quando registrado
Não gosta/desconfia: Senya, Kaand, Nyx sem documento
Visita fazenda: sim, licenças/altares permitidos
HP 58 | MP 40 | Stamina 50 | Breath 45
Força 1 | Constituição 2 | Destreza 4 | Inteligência 6 | Vontade 4 | Carisma 3
Resiste: Confusão legal, Medo burocrático
Vulnerável: Dano direto
```

Aparência:

Gnomo pequeno, pele clara, cabelo castanho penteado, olhos enormes atrás de lentes redondas e colete cheio de carimbos.

Background:

Tovin acredita que todo problema tem formulário. Se não tem, a civilização falhou.

Relações:

- casado com Mara;
- irrita Nimble;
- admira Corvus;
- desconfia de Renko.

Quests:

1. **Carimbo de Propriedade** — registrar fazenda expandida. Recompensa: licença.
2. **Altar Permitido** — explicar altares e bloquear Anya como construção livre. Recompensa: altar permitido.
3. **Selo Sem Reino** — identificar selo antigo. Recompensa: pista Bromécia/Elyndor.

---

## 29. Maelor Cinza

```text
ID: npc_maelor
Gênero: homem
Raça/subraça: Elfo da Noite / Luandil
Classe Primária: Explorador
Classe Secundária: Pesquisador
Tags: nyx, segredo, memoria, noite, ruina
Relacionamento: LateRomanceEligible
Deus cultuado: Nyx
Simpatia: Alihana, Anya como memória silenciada
Não gosta/desconfia: Kanthor público, Merithus que registra tudo, Kaand
Visita fazenda: sim, raro, à noite
HP 105 | MP 50 | Stamina 80 | Breath 85
Força 3 | Constituição 4 | Destreza 6 | Inteligência 5 | Vontade 5 | Carisma 3
Resiste: Medo, Exaustão noturna, Frio leve
Vulnerável: Senya/exposição pública
```

Aparência:

Elfo da Noite de pele cinza-carvão, cabelo azul-escuro quase negro, olhos prateados e tatuagens luminescentes discretas no pescoço.

Background:

Maelor sabe que a cidade esqueceu algo de propósito. Ele não sabe se deve ajudar o jogador a lembrar.

Relações:

- conhece Yael;
- evita Corvus;
- escuta Liora;
- observa as estátuas antigas.

Quests:

1. **Passos Onde Não Há Luz** — encontrar Maelor sem ser visto por guardas. Recompensa: rumor de Nyx.
2. **Memória Que Escolheu Sumir** — descobrir que a cidade esqueceu algo. Recompensa: pista Anya/Cindar.
3. **O Silêncio Também Protege** — revelar ou preservar segredo. Recompensa: confiança de Maelor.

Romance:

- Quest tardia: **O Nome que a Noite Não Levou**.
- Só disponível após parte da trama de Nyx/Anya.

---

# PARTE E — Fonte de Ressurreição / Anya

```text
ID: lore_anya_fountain
Tipo: Lore Anchor / Sistema
Local: fazenda
Função: ressurreição de companions, respec, Água Viva, cura rara, mistério de Anya
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

1. **Água que Lembra** — Fonte recupera Água Viva após evento de Alihana.
2. **Nome Apagado** — Thalindra, Liora ou Corvus reagem a símbolo próximo à Fonte.
3. **Ressurreição Dolorosa** — companion retorna com custo progressivo e possível diálogo.

---

# PARTE F — Próximas specs

```text
spec_city_npc_data_roster_stats.md
spec_city_deity_preferences_and_reputation.md
spec_city_relationship_romance_marriage.md
spec_city_farm_visits_schedule.md
spec_city_personal_quests_batch_01.md
spec_city_kanthor_temple_and_altars.md
spec_farm_fountain_anya_no_buildable_statue.md
```
