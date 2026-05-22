# Game Design Document — Cindar's Hope v2.6

> **Projeto:** Cindar's Hope
> **Mundo:** Vaalara (Dornecia — região de Cindar's Hope)
> **Engine:** Unity LTS | **Arte:** Aseprite + IA assistida, sprites 32x32px, 1280x720
> **Última atualização:** 2026-05-16
> **Fase atual:** Fase 8 — Implementação do MVP Fazenda

---

## SEÇÃO DE HANDOFF — Para continuação por outra LLM

### Plano macro completo

| # | Fase | Status | Resumo |
|---|---|---|---|
| 1 | **Ideação** | ✅ Concluída | Visão geral, sistemas, stack e loop de gameplay definidos |
| 2 | **Refinamento por Sistema** | ✅ Concluída | Farm, caverna, companions, combate, status/fome e progressão detalhados |
| 3 | **Identidade** | ✅ Concluída | Nome, mundo, lore, NPCs, raças, paleta e tom definidos |
| 4 | **Arquitetura Técnica** | ✅ Base aprovada | Ver `ARCH_fase4_v2.1.md` — código, eventos, SOs, save, pipeline arte→Unity |
| 5 | **Estruturação do Ambiente** | ✅ Documento pronto / execução local a validar | Ver `FASE5_ambiente_v1.1.md` — setup Windows, Unity, Git LFS, Aseprite, SpecKit, Codex/Claude |
| 6 | **Quebra de Histórias** | ✅ FARM concluído / demais épicos esboçados | Ver `FASE6_FARM_backlog_v1.1.md` e `FASE6_INDEX_global_v1.1.md` |
| 7 | **Spec Kit — MVP Fazenda** | ✅ Specs completas | Ver `FASE7_SPEC_MVP_FARM_v2.1.md` — 22 specs individuais, ~247 tasks rastreáveis |
| 8 | **Implementação do MVP Fazenda** | 🔄 Próxima fase prática | Implementar plantar → colher → vender → salvar → reabrir com estado restaurado |
| 9 | **Execução Iterativa Pós-MVP** | ⏳ Pendente | Cidade, craft, caverna, combate, companions e progressão |
| 10 | **Arte e Polish** | ⏳ Pendente | Sprites finais, animações, VFX, audio e UI final |
| 11 | **Testes** | ⏳ Pendente | Funcional, playtest, balanceamento e regressão |
| 12 | **Build Local Final** | ⏳ Pendente | Executável local, save path, packaging |
| 13 | **Iteração Pós-Launch** | ⏳ Pendente | Feedback, ajustes e novos conteúdos |

## 0.1 Decisão atual de arte e sprites

**Ferramenta principal:** Aseprite.

- Motivo: ferramenta especializada em pixel art, spritesheets, animação frame-a-frame, onion skinning, tiled mode, exportação PNG/JSON e automação via CLI.
- Distribuição: disponível na Steam e no site oficial; o código-fonte é público/source-available, mas a versão atual não deve ser tratada como software livre/FOSS.
- Alternativas aceitas: Pixelorama e LibreSprite para fluxo 100% gratuito/open-source, desde que exportem PNG com a mesma nomenclatura e configurações de importação.
- Direção visual: **cozy farm pixel art 32x32 inspirado por Harvest Moon/Stardew Valley**, mas com identidade própria de Vaalara. Não copiar assets, paletas proprietárias, personagens ou UI de Stardew Valley.

**Pipeline obrigatório:** conceito/rascunho → Aseprite → PNG/spritesheet → Unity com Filter Mode Point + Compression None → referência via ScriptableObject.


### O que foi feito (detalhe)

**Fase 1 — Ideação:**
Visão geral do jogo, personagem com 6 atributos, 7 status negativos, ciclo dia/noite + 4 seasons, sistema de farm (10 sementes, pesca, árvores, mineração), 4 workshops com 3 níveis, sistema de caverna (100 níveis procedurais, checkpoints), árvore de habilidades em 3 ramos, loop de gameplay definido, stack técnica escolhida (Unity LTS, C#, Aseprite, ScriptableObjects, BSP+CA, JSON save).

**Fase 2 — Refinamento:**
- Farm: custos de expansão definidos (4 estágios + sub-expansão de pasto), árvores (5 dias, 14 máx, 4 níveis de corte, 120 madeiras/árvore), vacas (2 sem pasto / 6 com), fertilizante (vacas + caverna), estufa como construção separada
- Caverna: seed fixa por save, renovação de recursos a cada 1 dia, checkpoints permanentes, saída por Pedra de Retorno ou caminho de volta
- Companions: ilimitados na fazenda, 1 ativo na caverna, jobs manuais (7 tipos), Explorador vai até seu nível, morte com ressurreição paga na Fonte da Fazenda
- Combate: tempo real estilo Stardew, físico + mágico livres, inimigos dropam loot
- Fome: 1u/passo fora, 3u/passo em combate, lentidão em 30%, HP lento ao zerar, companion sem fome
- Morte: respawn na fazenda, perde itens não equipados + metade do ouro

**Fase 3 — Identidade:**
- Nome: Cindar's Hope, mundo Vaalara, região Dornecia
- Raças jogáveis: Anão (+2 CON, +1 VON, bônus Forja) e Humano (+2 INT, +1 DES, bônus Coleta)
- 3 luas com ciclo mensal e efeitos mecânicos (Alihana = sementes/itens raros; Senya = magia/caos; Nyx = criaturas noturnas de dia na caverna)
- 10 deuses definidos, Anya morta como plot hook passivo
- NPCs de Cindar's Hope: 13 NPCs com raça, papel, afiliação e personalidade
- Guilda das Estradas: 6 mercantes da caverna (Sarto, Parto, Carto, Larto, Farto, Tarto) com mesmo sprite e humor de quarta parede
- Paleta: fazenda laranja/bronze, cidade azulado, caverna cinza/frio/roxo
- Tom: fantasia leve com humor sutil


**Fase 4 — Arquitetura Técnica:**
Arquitetura documentada com GameEventBus, catálogo de eventos, TimeManager, ScriptableObjects, state machine do jogador, SaveManager, estrutura de cenas, pipeline gráfico, geração de sprites, IA assistida e padrões para agentes.

**Fase 5 — Ambiente:**
Documento de setup escrito com instalação e configuração de Windows, Git, Git LFS, Unity LTS, VS Code, Codex/Claude, Aseprite, alternativas open-source e SpecKit. A execução local ainda precisa ser validada pelo checklist do ambiente.

**Fase 6 — Backlog:**
Backlog FARM detalhado com stories MVP/V2/FULL e índice global criado. Os demais épicos — CHAR, CITY, CRAFT, CAVE, COMBAT, COMP e UI — ainda estão em esboço.

**Fase 7 — Specs MVP Fazenda:**
Specs individuais do MVP Fazenda criadas, cobrindo código, arte, animação, VFX, UI e testes. O milestone mínimo validado é: BootScene → inventário com sementes → plantar → avançar dias → colher → vender → salvar → reabrir → estado restaurado.

### Todas as decisões técnicas fechadas

- Unity LTS, C#, Codex/Claude com `CLAUDE.md` próprio do projeto
- ScriptableObjects para todos os dados (itens, sementes, criaturas, receitas, companions, árvores)
- **TimeManager único com Event Bus (Opção A)** — sistemas se inscrevem em eventos de tempo, não fazem polling
- Save duplo: manual + automático por mudança de cena, formato JSON local
- Git + Git LFS no mesmo repositório (`*.png`, `*.aseprite`, `*.wav`)
- Audio adiado para fase de polish
- Resolução 1280x720, pixel art 32x32
- Aseprite como ferramenta principal de sprite/animação; Pixelorama/LibreSprite como fallback open-source
- Procedural: BSP + Cellular Automata, seed fixa por save

### Perguntas ainda em aberto (não bloqueiam Fase 8)

| Sistema | Pergunta |
|---|---|
| Lore | O que existe no nível 100? (deferido) |
| Lore | Como hints de Anya aparecem no jogo? |
| Farm | A partir de qual expansão o pasto fica disponível? |
| Craft | Lista completa de receitas por workshop e nível |
| Craft | Custo de construção e upgrade de cada workshop |
| Balanceamento | XP por atividade, duração exata do ciclo dia/noite |
| Companion | Job Explorador: retorna automaticamente? Tem inventário próprio? |
| Companion | Sistema de afinidade (deferido) |
| Personagem | Bônus de Forja do Anão: só passivo inicial ou evolui na skill tree? |

---

## 1. Visão Geral

**Cindar's Hope** é um jogo single-player 2D pixel art que mistura **farm sim** com **RPG dungeon crawler**, ambientado no mundo de **Vaalara**. O jogador vive nos arredores da cidade de Cindar's Hope, na região de Dornecia — gerencia uma fazenda, constrói workshops, recruta companions, interage com a cidade e usa tudo isso para progredir numa caverna procedural de 100 níveis.

**Referências visuais:** Harvest Moon / Stardew Valley como referência de gênero e legibilidade; arte final deve ter identidade própria de Vaalara
**Resolução:** 1280x720 (HD), pixel art 32x32
**Engine:** Unity LTS | **Linguagem:** C#
**Audio:** fase de polish

---

## 1.5 Mundo — Vaalara

O jogo se passa no mundo de **Vaalara**, na região de **Dornecia**. O jogador vive nos arredores de **Cindar's Hope**, uma cidade da Dornecia que dá nome ao jogo.

### As Três Luas
As luas de Vaalara não são sazonais — cada uma fica **1 mês in-game no céu**, ciclando continuamente. Todas as luas passam por todas as seasons.

| Lua | Domínio | Efeito mecânico |
|---|---|---|
| **Alihana** (branca) | Mistério, música, profecias | Sementes noturnas têm yield maior; Mercadores Viajantes na caverna têm itens proféticos/raros; chance de eventos misteriosos na fazenda |
| **Senya** | Paixão, festas, caos, magia | Magias causam mais dano; criaturas de caos mais frequentes na caverna; preços na cidade sobem (festa = demanda alta) |
| **Nyx** | Noite, segredos, morte | Criaturas noturnas aparecem também de dia na caverna; chance de encontrar itens secretos em locais normalmente vazios; exaustão drena mais devagar (noite favorece quem trabalha nas sombras) |

> O ciclo das luas cria variação de dificuldade e oportunidade semana a semana, sem depender só das seasons.

### Os Deuses de Vaalara

| Divindade | Domínio |
|---|---|
| **Kaand** | Destruição |
| **Kanthor** | Justiça |
| **Alihana** | Mistério, música, profecias (lua) |
| **Senya** | Paixão, festas, caos, magia (lua) |
| **Nyx** | Noite, segredos, morte (lua) |
| **Finan** | Sorte, ladinagem |
| **Meritos Tolerus** | Ordem, burocracia, mercantes |
| **Telisandra** | Selvageria, bestas, florestas profundas |
| **Tandra** | Bosques, vales, animais pacatos |
| **Toren** | Forja, anões, compromisso, minérios |
| **Anya** | Cura, esperança, renascimento — **está morta**; só pode aparecer como hints, ecos, vestígios |

> Anya sendo morta é um plot hook rico: itens de cura podem ter fragmentos de sua essência, a caverna pode ter ruínas de um templo a ela, NPCs podem mencioná-la com reverência e tristeza.

---

Escolha de gênero: menino ou menina.

### 2.0 Raças Jogáveis

| Raça | Bônus de Atributo | Bônus de Habilidade |
|---|---|---|
| **Anão** | +2 Constituição, +1 Vontade | +bônus passivo em Forja (itens de qualidade maior desde o início) |
| **Humano** | +2 Inteligência, +1 Destreza | +bônus passivo em Coleta (mais rendimento por ação desde o início) |

> Outras raças existem no mundo (Elfos, Goblins, Gnomos, Orcs, Gigantes, Kobolds, Halflings) mas não são jogáveis — aparecem como NPCs e companions.



| Atributo | Efeito principal |
|---|---|
| **Força** | Dano físico + velocidade de coleta |
| **Constituição** | Hit Points + resistência a status negativos |
| **Destreza** | Velocidade de movimento + dano a distância + chance de esquiva |
| **Inteligência** | Dano mágico + eficiência de poções |
| **Vontade** | Pontos de Magia (MP) + resistência a Medo |
| **Carisma** | Desconto em compras + bônus em vendas + influência com companions |

### 2.2 Status Negativos

| Status | Origem | Efeito | Cura |
|---|---|---|---|
| **Fome** | Barra contínua — drena por passo (3x em combate) | Lentidão progressiva → perda lenta de HP ao esvaziar | Comer alimentos |
| **Exaustão** | Ficar acordado além do limite | Reduz Destreza e velocidade | Dormir |
| **Frio** | Caverna gelada / noite fria | Reduz Vontade e MP | Poção de calor, fogueira |
| **Calor** | Bioma de fogo | Reduz Constituição | Poção de frio, água |
| **Veneno** | Criaturas / armadilhas | Drena HP ao longo do tempo | Antídoto |
| **Medo** | Criaturas especiais | Reduz Vontade, pode causar fuga involuntária | Vontade alta, poção de coragem |
| **Morte** | HP zerado | Respawn na fazenda com penalidade | — |

**Penalidade de morte:** perde itens não equipados + metade do ouro carregado. XP e checkpoints mantidos.

**Fome — detalhe:**
- Drena 1 unidade por passo fora de combate
- Drena 3 unidades por passo em combate
- Ao chegar em ~30% da barra: personagem fica mais lento
- Ao zerar: começa a perder HP lentamente (não instantâneo)
- Companion não possui barra de fome

---

## 3. Ciclo de Tempo

### 3.1 Dia e Noite
- Ciclo in-game de ~20 min reais (configurável por save)
- Sementes diurnas, noturnas e neutras
- Criaturas com período — dentro da caverna o ciclo continua (criaturas "sentem" o ciclo)
- Lojas da cidade têm horário de funcionamento
- Dormir reseta exaustão e avança o dia

### 3.2 Estações
- 4 estações: Primavera, Verão, Outono, Inverno
- Plantio por estação; Inverno = sem plantio em solo descoberto
- Estufa permite plantar no Inverno (ver seção 4)
- Preços na cidade flutuam por estação
- Comportamento de criaturas na caverna muda por estação

---

## 4. Sistema de Farm

### 4.1 Expansão da Fazenda

| Expansão | Req. Nível | Ouro | Madeira | Outros |
|---|---|---|---|---|
| 1x → 2x | 5 | 800 | 200 | — |
| 2x → 3x | 15 | 2.000 | 450 | 100 minérios de metal, 40 vidros |
| 3x → 4x | 30 | 4.000 | 650 | 150 ingots de metal, 100 minérios de prata, 120 vidros |
| **Sub-expansão: Pasto** | — | 1.000 | 200 | 100 minérios de metal |
| 4x → (futuro) | — | 8.000 | 1.300 | 300 ingots de metal, 200 minérios de prata, 240 vidros |

> A 4ª expansão custa o dobro da 3ª em todos os recursos.
> A sub-expansão de pasto é independente, pode ser comprada após a 1ª expansão (a confirmar). Libera capacidade de vacas de 2 → 6.

### 4.2 Estufa
- É uma **construção paga sobre expansão de terreno** (não é workshop)
- Permite plantar qualquer semente durante o Inverno
- Também pode ser usada para cultivar sementes noturnas sob luz artificial (a definir)
- Construída com madeira + vidro (material a obter na caverna)

### 4.3 Plantio — 10 Sementes Base

| # | Semente | Período | Uso |
|---|---|---|---|
| 1 | Trigo | Diurno | Comida, farinha |
| 2 | Cenoura | Neutro | Comida, poção de visão |
| 3 | Erva Medicinal | Noturno | Poções de cura |
| 4 | Cristal-Flor | Noturno | Reagente mágico |
| 5 | Batata | Neutro | Comida, stamina |
| 6 | Cogumelo-Sombra | Noturno | Poção de veneno, antídoto |
| 7 | Girassol de Fogo | Diurno | Reagente de fogo, poção de calor |
| 8 | Flor-de-Gelo | Noturno | Reagente de gelo, poção de frio |
| 9 | Pimenta-Vermelha | Diurno | Comida (+Força temporário) |
| 10 | Musgo-Lunar | Noturno | Poção de mana, reagente mágico |

### 4.4 Fertilizante
Duas fontes de obtenção:
- **Animais na fazenda:** vacas compradas na cidade produzem fertilizante passivamente
  - Sem expansão de pasto: máximo **2 vacas**
  - Com expansão de pasto: máximo **6 vacas**
- **Caverna:** cocos de morcego encontrados no chão de certos níveis
- Aplicado no solo antes de plantar para acelerar crescimento ou aumentar rendimento

### 4.5 Pesca
- Lago pequeno na fazenda (expande com a fazenda)
- Peixe como matéria-prima para comida e poções
- Período e estação influenciam espécies disponíveis
- Cana de pescar upgradável via Carpintaria (3 tiers)

### 4.6 Árvores
- Crescem automaticamente a cada **5 dias in-game**
- Máximo de **14 árvores** na fazenda; começa com **4**
- O jogador planta as demais (muda comprada na cidade ou obtida)
- Cada árvore tem **4 níveis de corte**, dando até **120 madeiras no total**
  - Cada corte rende até 30 madeiras (começa em 10 com ferramenta básica)
  - Ferramentas melhores aumentam o yield por corte
- Companion Lenhador pode cortar automaticamente quando alocado

### 4.7 Mineração na Fazenda
- Área de rocha inicial: pedra e cobre
- Materiais avançados (ferro, cristal, minério arcano) apenas na caverna

---

## 5. Workshops

Construídos com madeira + minérios. Evoluem em **3 níveis**.
Crafting **somente nos workshops** — sem crafting por mochila.

| Workshop | Nível 1 | Nível 2 | Nível 3 |
|---|---|---|---|
| **Forja** | Ferramentas e armas básicas | Armaduras e armas de ferro | Equipamentos lendários, runas |
| **Alquimia** | Poções de cura simples | Poções de status, antídotos | Poções arcanas, elixires |
| **Carpintaria** | Estruturas básicas, cana de pesca | Armadilhas, móveis, estufa | Engenhocas, itens especiais |
| **Costura** | Roupas simples | Capas e bolsas com atributos | Armaduras de couro/tecido encantadas |

---

## 6. A Cidade — Cindar's Hope

Cindar's Hope é uma cidade pequena mas movimentada da Dornecia, perto de terrenos abertos ideais para fazendas — e perigosamente próxima de uma caverna que ninguém de bom senso visita mais de uma vez.

**Tom de escrita:** fantasia leve com humor sutil. NPCs têm personalidade, comentam o dia a dia, reclamam de imposto, fazem piada sobre a caverna. Nada de drama épico sem ironia.

- Carisma influencia preços de compra e venda
- NPCs têm horário de funcionamento e diálogos contextuais (mudam com lua, season, reputação)
- Animais (vacas) comprados aqui

### NPCs de Cindar's Hope

**Artesãos e Especialistas**

| NPC | Raça | Papel | Afiliação |
|---|---|---|---|
| **Brumdar Pedreiro** | Anão | Ferreiro — ferramentas, armas básicas, blueprints de forja | Devoto de Toren |
| **Sylveth** | Elfo da Floresta | Sementes — comuns e raras por estação | Devota de Tandra |
| **Ozzra** | Goblin | Alquimista — poções, ingredientes, blueprints | Altar escondido de Finan |
| **Nimble** | Gnomo | Carpinteiro — blueprints de carpintaria, materiais de construção | Admirador informal de Toren |
| **Thalindra Cinzenta** | Elfo Cinzento | Biblioteca / Quests — blueprints raros, missões de exploração | Estudiosa de Alihana |
| **Padre Corvus** | Humano | Templo de Kanthor — curas pagas, bênçãos (+resistência a status) | Menciona Anya com tristeza |

**Mercantes e Comércio**

| NPC | Raça | Papel | Afiliação |
|---|---|---|---|
| **Pip Miudinho** | Halfling | Mercador geral — compra e vende quase tudo; preços medianos | Devoto de Meritos Tolerus (torce pra Finan) |
| **Dagna Brasão** | Anã | Mercante de metais — minérios, ingots, vidros | Devota de Toren |
| **Yael** | Elfo da Noite | Mercante de tecidos e reagentes raros — só abre à noite | Devota de Nyx; pouco comunicativa |
| **A Guilda das Estradas** | Humanos | Mercantes itinerantes da caverna — ver seção 8.6 | Devotos de Finan (claro) |

**Construtores e Serviços**

| NPC | Raça | Papel | Afiliação |
|---|---|---|---|
| **Hund e Gurd** | Irmãos Gnomos | Construtores — expandem a fazenda, constroem a Estufa e estruturas externas; cobram ouro + materiais | Devotos de Meritos Tolerus (tem contrato pra tudo) |
| **Marta Argila** | Humana | Ferragem e reparos — vende materiais básicos de construção, conserta ferramentas | Não tem devoção, tem agenda |
| **Gruta** | Orc | Estalagem — comida, descanso pago (reseta exaustão mais rápido), rumores da cidade | Não tem tempo pra deuses |
| **Zrix** | Kobold | Entrada da caverna — vende Pedras de Retorno, dicas de andares | Vive com medo de Kaand |

> **Companions recrutáveis:** Ozzra (Artesão de Alquimia), Zrix (Explorador), Gruta (Combate), Dagna (Minerador). Outros possíveis via quests na Fase 6.

### Sistema de Reputação com Cindar's Hope

| Threshold | Efeito |
|---|---|
| 25+ | Desconto de 5% nas lojas |
| 50+ | Acesso a itens raros, quests exclusivas |
| 75+ | NPCs dão dicas sobre andares específicos da caverna |
| 100 | Título "Protetor de Cindar's Hope" + buff passivo de Carisma |

Reputação aumenta: vendendo, completando quests, interagindo com NPCs.
Reputação cai: sumir por muitos dias, abandonar quests.

---

## 6.5 Identidade Visual — Paleta de Cindar's Hope

### Fazenda
Tom base **laranja/bronze** — transmite calor, colheita, trabalho manual.
- Solo: marrom avermelhado
- Plantas: verdes com reflexo dourado
- Céu diurno: laranja suave → amarelo
- Noite na fazenda: azul escuro com reflexo das luas
- Madeira: caramelo / mogno claro

### Cidade (Cindar's Hope)
Tom base **azulado** — pedra, arquitetura densa, vida urbana mais fria que a fazenda.
- Paredes: cinza azulado, pedra lavrada
- Telhados: azul ardósia, terracota envelhecida
- Iluminação de lanternas: âmbar quente contrastando com o fundo frio
- NPCs: paleta de roupa variada, mas tons de azul, verde-escuro e cinza dominam

### Caverna
Tom base **frio, escuro** com variação por bioma.
- Pedra base: cinza carvão, preto com brilho úmido
- Acentos: roxo escuro em veios de cristal, fungos bioluminescentes
- Caverna de gelo (26–40): azul gelo, branco
- Caverna de fogo (41–55): laranja e vermelho — único bioma quente
- Abismo sombrio (71–85): preto dominante, roxo intenso, sem fonte de luz natural
- UI da caverna: bordas mais escuras, menos saturadas que na fazenda

---

## 7. Sistema de Companions

### 7.1 Recrutamento
- Recrutados via quest, interação ou reputação com a cidade
- Múltiplos companions podem viver na fazenda simultaneamente — **sem limite**
- Carisma do jogador influencia facilidade de recrutamento e efetividade

### 7.2 Alocação de Jobs na Fazenda
O jogador define manualmente o job de cada companion inativo:

| Job | O que o companion faz |
|---|---|
| **Plantador** | Planta sementes nos canteiros disponíveis |
| **Colhedor** | Coleta plantas maduras |
| **Pescador** | Pesca no lago da fazenda |
| **Lenhador** | Corta árvores para obter madeira |
| **Minerador** | Vai à área de mineração da fazenda |
| **Artesão** | Trabalha nos workshops (crafting automático de receitas pré-definidas) |
| **Explorador** | Vai à caverna buscar materiais — alcança até o nível equivalente ao seu próprio nível |

### 7.3 Companion Ativo (Caverna)
- Apenas 1 companion ativo acompanha o jogador na caverna
- Segue automaticamente, sem microgerenciamento
- Pode ser equipado com itens
- Tem seu próprio HP e atributos (evoluem com nível)

### 7.4 Morte do Companion
- Companion pode morrer na caverna ou na fazenda
- Para ressuscitá-lo: pagar ouro na **Fonte da Fazenda** (estrutura permanente)
- Sem penalidade de XP ou perda de itens do companion

### 7.5 Afinidade (Futuro)
- Sistema de afinidade deferido para versão futura
- Quando implementado: afinidade alta acelera ganho de XP do companion e aumenta coleta
- Pode ser desenvolvida por interação e presentes

---

## 8. Sistema de Caverna

### 8.1 Geração Procedural
- Seed gerada uma única vez ao criar novo jogo
- Mantida em todas as sessões do mesmo save
- Algoritmo: BSP (layout) + Cellular Automata (variação orgânica)

### 8.2 Estrutura dos 100 Níveis

| Níveis | Bioma | Mecânica especial |
|---|---|---|
| 1–10 | Caverna de pedra | Tutorial de combate |
| 11–25 | Floresta subterrânea | Armadilhas de planta |
| 26–40 | Caverna de gelo | Status: Frio |
| 41–55 | Caverna de fogo | Status: Calor |
| 56–70 | Ruínas antigas | Puzzles e armadilhas |
| 71–85 | Abismo sombrio | Status: Medo intensificado |
| 86–99 | Núcleo corrompido | Criaturas de elite |
| 100 | [Boss final / lore a definir] | — |

- **Checkpoints permanentes** a cada 10 níveis (mantidos entre sessões)
- Chefes nos níveis 10, 25, 40, 55, 70, 85 e 100
- Ciclo dia/noite afeta criaturas dentro da caverna

### 8.3 Recursos e Renovação
- Recursos (minério, plantas, loot de criaturas) se renovam a cada **1 dia in-game**
- Seed do layout não muda, mas posição de recursos pode variar na renovação

### 8.4 Saída da Caverna
- Saída via **item de retorno** (ex: Pedra de Retorno — crafted ou comprada)
- Ou retornando pelo caminho de entrada (andando de volta)
- Não é possível sair em qualquer ponto sem um desses meios

### 8.5 A Guilda das Estradas — Mercantes da Caverna

Mercantes itinerantes que aparecem aleatoriamente nos níveis da caverna. Todos são membros da **Guilda das Estradas**, todos têm o mesmo sobrenome, todos têm o mesmo sprite base com leves variações de cor e detalhe — e todos têm respostas prontas pra quando o jogador notar.

**Comportamento:** aparecem em posições aleatórias no mapa do nível, andam pelo mapa, podem ser mortos por criaturas (o jogador precisa achá-los antes). Vendem itens especiais não disponíveis na cidade.

| Nome | Variação visual | Especialidade | Comentário característico |
|---|---|---|---|
| **Sarto das Estradas** | Chapéu vermelho, avental marrom | Armas e ferramentas raras | *"Não, eu não sou o Parto. O Parto usa chapéu azul. Completamente diferente."* |
| **Parto das Estradas** | Chapéu azul, avental marrom | Poções e reagentes | *"Me disseram que tenho um irmão aqui na caverna. Nunca o conheci. Estranha coincidência."* |
| **Carto das Estradas** | Chapéu verde, avental marrom | Blueprints e receitas | *"Na minha cidade natal, todos os cartógrafos têm exatamente esse rosto. É cultural."* |
| **Larto das Estradas** | Chapéu roxo, avental marrom | Armaduras e materiais defensivos | *"Olha, a Guilda tem um processo seletivo rigoroso. Às vezes saem produtos... similares."* |
| **Farto das Estradas** | Chapéu amarelo, avental marrom | Comida, consumíveis, sementes raras | *"Sim, me chamo Farto. Não, não tem história engraçada. Pode comprar alguma coisa?"* |
| **Tarto das Estradas** | Chapéu laranja, avental marrom | Itens mágicos e reagentes arcanos | *"Você já se perguntou se em todas as cidades as enfermeiras são irmãs idênticas? Só curiosidade."* |

> Se o jogador encontrar dois membros da guilda no mesmo nível, eles fingem não se conhecer e ficam levemente constrangidos se o jogador mencionar a semelhança. Um possível evento raro: dois deles aparecem lado a lado num checkpoint, discutindo em voz baixa e parando imediatamente quando o jogador se aproxima.

> **Pedra de Retorno:** item de uso único, craftável na Carpintaria ou comprável na cidade e com qualquer membro da Guilda das Estradas.

---

## 9. Sistema de Combate

- **Tempo real**, estilo Stardew Valley
- Jogador usa armas físicas e magias livremente, sem troca de modo
- Companion combate automaticamente ao lado do jogador
- Inimigos **dropam loot** ao morrer
- Hit, esquiva e dano calculados pelos atributos do personagem

---

## 10. Árvore de Habilidades

### Ramo 1 — Produção & Coleta
Plantio e colheita acelerados, pesca melhorada (espécies raras), blacksmithing de tier alto, alquimia avançada, corte de árvore eficiente, mineração com mais rendimento.

### Ramo 2 — Combate Físico
Combos, ataques em área, passivas de Força e Destreza, especialização por tipo de arma (espada, arco, machado), bloqueio e esquiva aprimorados.

### Ramo 3 — Combate Mágico
Magias elementais (fogo, gelo, raio, sombra), buffs e debuffs, regeneração de MP em combate, suporte e potencialização de companion.

---

## 11. Loop de Gameplay

```
[Fazenda — contínuo]
  → plantar / colher / pescar / cortar árvore / minerar
  → gerenciar fome e exaustão
  → alocar companions para jobs
  → craftar em workshops
  → cuidar dos animais (vaca → fertilizante)

[Cidade]
  → vender produção e itens craftados
  → comprar sementes, blueprints, consumíveis, animais
  → quests e interação com NPCs
  → recrutar companions

[Caverna]
  → explorar nível procedural (seed fixa por save)
  → combate em tempo real com companion ativo
  → coletar recursos exclusivos (renovam a cada dia in-game)
  → interagir com Mercadores Viajantes
  → gerenciar fome (3x mais rápida em combate), status e HP
  → checkpoint permanente a cada 10 níveis
  → sair via Pedra de Retorno ou caminho de entrada

[Progressão]
  → XP de todas as atividades
  → level up → pontos de atributo + pontos de habilidade
  → workshops: nível 1 → 3
  → fazenda: 1x → 4x (requer nível + ouro + materiais)
  → objetivo final: nível 100 da caverna
```

---

## 12. Stack Técnica

| Área | Decisão |
|---|---|
| Engine | Unity LTS |
| Linguagem | C# |
| Geração de código | Codex / Claude com CLAUDE.md específico do projeto |
| Arte | Aseprite como ferramenta principal; Pixelorama/LibreSprite como fallback; sprites 32x32px, personagens 32x48px, 1280x720 |
| Mapas | Tiled + SuperTiled2Unity |
| Iluminação | Unity URP 2D Light (dia/noite) |
| Dados de jogo | ScriptableObjects (itens, sementes, criaturas, receitas) |
| Geração procedural | BSP + Cellular Automata |
| Save | JSON local, duplo (manual + automático por mudança de cena) |
| Versionamento | Git + Git LFS (binários no mesmo repo) |
| Audio | Adiado para fase de polish |
| Assets de prototipagem | itch.io, Kenney.nl |

---

## 13. Pipeline de Etapas

| # | Fase | Status | Resumo |
|---|---|---|---|
| 1 | **Ideação** | ✅ Concluída | Visão geral, sistemas, stack e loop de gameplay definidos |
| 2 | **Refinamento por Sistema** | ✅ Concluída | Farm, caverna, companions, combate, status/fome e progressão detalhados |
| 3 | **Identidade** | ✅ Concluída | Nome, mundo, lore, NPCs, raças, paleta e tom definidos |
| 4 | **Arquitetura Técnica** | ✅ Base aprovada | Ver `ARCH_fase4_v2.1.md` — código, eventos, SOs, save, pipeline arte→Unity |
| 5 | **Estruturação do Ambiente** | ✅ Documento pronto / execução local a validar | Ver `FASE5_ambiente_v1.1.md` — setup Windows, Unity, Git LFS, Aseprite, SpecKit, Codex/Claude |
| 6 | **Quebra de Histórias** | ✅ FARM concluído / demais épicos esboçados | Ver `FASE6_FARM_backlog_v1.1.md` e `FASE6_INDEX_global_v1.1.md` |
| 7 | **Spec Kit — MVP Fazenda** | ✅ Specs completas | Ver `FASE7_SPEC_MVP_FARM_v2.1.md` — 22 specs individuais, ~247 tasks rastreáveis |
| 8 | **Implementação do MVP Fazenda** | 🔄 Próxima fase prática | Implementar plantar → colher → vender → salvar → reabrir com estado restaurado |
| 9 | **Execução Iterativa Pós-MVP** | ⏳ Pendente | Cidade, craft, caverna, combate, companions e progressão |
| 10 | **Arte e Polish** | ⏳ Pendente | Sprites finais, animações, VFX, audio e UI final |
| 11 | **Testes** | ⏳ Pendente | Funcional, playtest, balanceamento e regressão |
| 12 | **Build Local Final** | ⏳ Pendente | Executável local, save path, packaging |
| 13 | **Iteração Pós-Launch** | ⏳ Pendente | Feedback, ajustes e novos conteúdos |

### Próxima entrega prática

A próxima entrega não é escrever mais visão geral. É executar a **Fase 8 — MVP Fazenda**, usando `FASE7_SPEC_MVP_FARM_v2.1.md` como backlog técnico rastreável.

Critério mínimo de sucesso do MVP:

```text
BootScene → FarmScene → inventário inicial com sementes → plantar → avançar dias → colher → vender → salvar → fechar → reabrir → estado restaurado
```


## 14. Perguntas Abertas Restantes

Estas questões não bloqueiam o MVP Fazenda, mas devem ser resolvidas antes dos épicos FULL:

| Sistema | Pergunta |
|---|---|
| Lore | O que existe no nível 100? (deferido) |
| Lore | Anya morta: como os hints aparecem? (itens, ruínas, diálogos?) |
| Farm | A partir de qual expansão o pasto pode ser comprado? |
| Craft | Lista completa de receitas por workshop e nível |
| Craft | Custo de construção e upgrade de cada workshop |
| Balanceamento | XP por atividade (colheita, pesca, combate, craft, mineração) |
| Balanceamento | Duração exata do ciclo dia/noite in-game |
| Companion | Job Explorador: retorna automaticamente ao encher inventário? Tem inventário próprio? |
| Companion | Sistema de afinidade (deferido) |
| Personagem | Bônus de Forja do Anão: só passivo inicial ou evolui na skill tree? |

---

*v2.5 — GDD consolidado como fonte de visão/design. Status alinhado com Fase 7 concluída para MVP Fazenda e Fase 8 como próxima execução prática.*


---

## 0.2 Decisão atual — implementação com Codex e produção de sprites com IA

A partir da Fase 8, o projeto deve ser executado por **fatias pequenas de PR**, não por specs inteiras de uma vez.

### Estratégia de código

- O Codex deve ser usado como agente principal de implementação dentro do VS Code ou fluxo local/cloud, sempre com contexto explícito.
- Cada tarefa do Codex deve receber:
  - documentos de referência;
  - spec alvo;
  - arquivos permitidos;
  - arquivos proibidos;
  - critérios de aceite;
  - teste manual obrigatório;
  - regra de não ampliar escopo.
- O Codex **não deve** receber as 22 specs de uma vez.
- O Codex **não deve** decidir arquitetura nova sem atualizar `ARCH_fase4_v2.2.md` e pedir revisão humana.

Documento operacional: `FASE8_EXECUTION_PLAN_CODEX_v1.0.md`.

### Estratégia de arte/sprites

A arte final continua sendo **Aseprite-first**. IA ajuda em conceito, variação e rascunho, mas não substitui acabamento manual.

| Tipo de asset | Ferramenta recomendada | Observação |
|---|---|---|
| Ícones simples 32x32 | ChatGPT/DALL-E + Aseprite | Bom para sementes, madeira, peixe, moeda; sempre retocar |
| Sprites de personagens 32x48 | PixelLab opcional + Aseprite | Melhor quando há necessidade de poses/direções consistentes |
| Tilesets 32x32 | PixelLab/Scenario opcional + Aseprite | Exige revisão manual de bordas, autotile e repetição |
| Art bible / consistência de estilo | Scenario opcional | Útil se o projeto crescer e precisar de lote grande |
| Sprite final do jogo | Aseprite | Fonte final de verdade visual |

Documento operacional: `SPRITE_PIPELINE_AI_ASEPRITE_v1.0.md`.

### Estratégia de contratos técnicos

Antes de implementar save, inventário, plantio e crescimento, devem existir contratos mínimos para:

- eventos;
- IDs estáveis de itens/sementes;
- registries de ScriptableObjects;
- save por `Application.persistentDataPath`;
- serialização por ID, não por referência direta de Unity.

Documento operacional: `CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.0.md`.

---

## 0.3 Fonte de verdade documental após v2.6

| Tema | Fonte primária |
|---|---|
| Visão do jogo, lore, sistemas e decisões macro | `GDD_v2.6.md` |
| Arquitetura técnica, eventos, SOs, cenas, save e pipeline arte↔Unity | `ARCH_fase4_v2.2.md` |
| Setup local Windows/Unity/Codex/Aseprite/SpecKit | `FASE5_ambiente_v1.2.md` |
| Roadmap e estado dos épicos | `FASE6_INDEX_global_v1.2.md` |
| Stories FARM | `FASE6_FARM_backlog_v1.2.md` |
| Specs implementáveis MVP Fazenda | `FASE7_SPEC_MVP_FARM_v2.2.md` |
| Execução com Codex por PR | `FASE8_EXECUTION_PLAN_CODEX_v1.0.md` |
| Contratos core para eventos/save/IDs | `CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.0.md` |
| Pipeline de sprites com IA + Aseprite | `SPRITE_PIPELINE_AI_ASEPRITE_v1.0.md` |
| Regras para agentes | `CLAUDE_v1.2.md` e `AGENTS.md` |

---

## 0.4 Escopo rígido do MVP Fazenda

O MVP Fazenda não é “fazer a fazenda completa”. É provar o loop mínimo jogável:

```text
BootScene → FarmScene → inventário inicial → plantar → avançar dias → colher → vender → salvar → fechar → reabrir → estado restaurado
```

### Dentro do MVP

- Personagem placeholder se move.
- Fazenda placeholder tem chão, borda, 9 canteiros, 4 árvores, lago e ponto de venda.
- Inventário textual funciona.
- Trigo e Cenoura funcionam como sementes e colheitas.
- Tempo avança por botão de dormir/TAB.
- Plantas crescem por `DayStartedEvent`.
- Colheita adiciona itens no inventário.
- Venda converte item em ouro.
- Save/load restaura dia, inventário, ouro, fome e canteiros.

### Fora do MVP

- Animação final de personagem.
- Arte final completa.
- Cidade navegável.
- NPCs reais.
- Crafting.
- Caverna.
- Combate.
- Companions.
- Skill tree.
- Seasons completas.
- Luas com efeito final.
- Balanceamento econômico definitivo.

### Regra de proteção de escopo

Se uma tarefa do Codex tentar implementar algo de V2/FULL junto com MVP, ela deve ser recusada ou revertida. O MVP é vertical, pequeno e testável.

