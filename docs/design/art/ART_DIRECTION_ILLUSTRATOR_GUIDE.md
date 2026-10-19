# Cindar's Hope — Guia Visual para Ilustrador

> **Destinatário:** Ilustrador/pixel artist  
> **Propósito:** Descrição completa de cada componente visual do jogo para produção de sprites  
> **Última atualização:** 2026-06-22

---

## REFERÊNCIAS VISUAIS PRINCIPAIS

O jogo tem **dois registros visuais distintos**, inspirados em obras diferentes:

**Superfície (fazenda + cidade):**
→ Referência: **Stardew Valley**
Pixel art 2D top-down levemente isométrica. Cores quentes e saturadas. Tons de terra, verde e madeira. Luz solar suave e difusa. Personagens com expressão clara mesmo em poucos pixels. Tudo é acolhedor, legível, pastoral. Sombras simples e flat. Paletas harmônicas sem muita variação de valor num mesmo objeto — leitura imediata a distância.

**Subterrâneo (caverna):**
→ Referência: **Children of Morta**
Pixel art detalhada, atmosférica, com uso intenso de luz artificial. Fundos escuros com sombras profundas. Fontes de luz pontuais (tochas, brasas, cristais, bioluminescência). Inimigos com silhuetas limpas mas texturas densas. Partículas e efeitos de luz são parte do design. Paleta dominantemente fria com acentos quentes/mágicos.

**Regra geral de craft:**
- Pixel art genuíno: sem anti-alias em bordas, sem gradientes suaves
- Outline de 1 pixel (preto ou cor escura da própria paleta) em todos os personagens e criaturas
- Personagens: outline completo. Props: outline opcional dependendo de contraste com o fundo
- Animação mínima legível — idle com pelo menos 2-4 frames, ação com 4-8 frames
- Pivot: bottom-center em todos os personagens e criaturas

---

## UNIDADE BASE E ESCALA

```
Tile base: 32 × 32 pixels = 1 tile = 1 unidade de movimento do jogo
Player: 32 × 48 px (1 tile de largura, 1,5 tile de altura)
Câmera: exibe aprox. 20×12 a 24×14 tiles em tela
```

Todo elemento do jogo escala a partir dessa unidade. Um tile de 32px no mundo é equivalente a aproximadamente 1 metro ficcional.

---

# PARTE 1 — PERSONAGENS E NPCs

---

## PLAYER — Protagonista

**Dimensões:** 32 × 48 px | Collider: 20×12 px (pés)

**Descrição visual:**
Jovem adulto de aparência comum, sem armadura inicial. Roupa de trabalho rural — camisa de linho, calça reforçada, botas de couro. Cabelo curto ou preso. Expressão determinada mas cansada. O personagem carrega uma pequena mochila de viagem que cresce conforme o jogo progride. É o menor personagem "herói" — não tem aparência de guerreiro clássico.

**Cores (roupa inicial):**
- Camisa: bege/linho — `#C8A87A`
- Calça: marrom escuro — `#5C3D2E`
- Botas: couro âmbar — `#8B5E3C`
- Cabelo: castanho médio — `#7B4F2E`
- Pele: tom neutro-quente, 3 valores (luz, meio, sombra)

**Feeling:** Pessoa comum jogada numa situação maior que ela. Não é um herói épico, é alguém que sobrevive e cresce.

---

## NPCS — Raças e Tipos

### Humano / Elfo / Tiefling comum

**Dimensões:** 32 × 48 px | Collider: 20×12 px

**Descrição:**
Mesma proporção do player. Cada raça tem traços específicos:

- **Humano:** fisionomia neutra, orelhas arredondadas, maior variedade de tons de pele, roupa condizente com ofício (ferreiro tem avental, alquimista tem manto)
- **Elfo:** orelhas pontudas visíveis mesmo em 32px (5-6 pixels acima da linha da cabeça), sobrancelhas finas, olhos levemente amendoados, estatura ligeiramente mais esbelta, cabelos longos ou com trança
- **Tiefling:** chifres pequenos curvados saindo da cabeça (não exagerados), pele em tons incomuns (cinza-azulado, roxo-acinzentado, vermelho-acinzentado), cauda fina e pontiaguda visível atrás do corpo, olhos sólidos sem íris branca

**Paleta de pele por raça:**
- Humano claro: `#F5CBA7` luz / `#D4956A` meio / `#A0623A` sombra
- Humano médio: `#C68642` luz / `#9B6A2E` meio / `#6B4226` sombra
- Humano escuro: `#7C4B2A` luz / `#5C3218` meio / `#3E2010` sombra
- Elfo (pele fria): `#E8D5C4` luz / `#BFA08A` meio / `#8C6B55` sombra
- Tiefling (pele azulada): `#7E9EC4` luz / `#5A7A9C` meio / `#3A5A78` sombra
- Tiefling (pele roxa): `#9B7EC8` luz / `#7A5CA8` meio / `#5A3C84` sombra

---

### Halfling / Goblin NPC

**Dimensões:** 28–32 × 40 px | Collider: 18×10 px

**Halfling (NPC amigável):**
Figura baixa e rechonchuda. Pés grandes (desproporcional, característica visual importante). Orelhas levemente pontudas e largas. Cabelo cacheado ou encaracolado. Expressão naturalmente jovial. Roupas coloridas com bolsos. Anda com leveza, quase pulando.

**Cores Halfling:**
- Pele quente: `#EFBE90` luz / `#C89060` meio / `#9A6038` sombra
- Pé/calçado: raramente usa sapatos — pé peludo em marrom-claro `#B8895A`

**Goblin (NPC ou inimigo):**
Corpo magro e curvado. Cabeça grande em relação ao corpo. Nariz longo e protuberante. Orelhas pontudas e rasgadas. Dentes pequenos mas visíveis. Olhos grandes e amarelados. Roupa de refugo — pedaços de couro amarrado, pele de animais pequenos.

**Cores Goblin:**
- Pele: verde-acinzentado `#7A9B6A` luz / `#5A7A50` meio / `#3A5630` sombra
- Olhos: amarelo-âmbar `#D4A020`

---

### Anão (Dwarf)

**Dimensões:** 32 × 44 px | Collider: 22×14 px

**Descrição:**
Baixo e extremamente largo. Ombros quase tão largos quanto o sprite é alto. Barba espessa que desce pelo peito (visível e texturizada mesmo em 32px — use 2-3 tons de pixel para dar volume). Braços grossos. Mãos grandes. Expressão séria e prática. Roupa de trabalho pesada: avental de couro, botas reforçadas, fivelas de metal.

**Cores:**
- Pele: tom rosado-escuro `#C87A50` luz / `#9A5830` meio
- Barba: varia por personagem — castanho `#7A5030`, ruivo `#A8400A`, cinza `#888888`
- Metal (fivelas, ferramentas): cinza-azulado `#8898A8`

---

### Orc / Meio-Orc / Draconato

**Dimensões:** 36–40 × 52–56 px | Collider: 24×16 px

**Orc:**
Figura imponente que sobressai visualmente entre os NPCs. Ombros largos, postura levemente curvada para frente. Presas inferiores visíveis (dois dentes saindo para cima dos lábios). Nariz achatado. Sobrancelhas pesadas. Olhos fundos. Textura de pele com variação sutil — orc não é verde-uniforme, tem sombras profundas.

**Cores Orc:**
- Pele: verde-médio `#6A8A54` luz / `#4A6A38` meio / `#2E4A20` sombra
- Presas: marfim `#E8D8B0`
- Olhos: amarelo-escuro ou vermelho-acinzentado

**Meio-orc:**
Mistura visual — fisionomia mais próxima de humano mas com presas menores visíveis, pele com leve tom esverdeado, testa mais pesada.

**Draconato:**
Escamas visíveis (use pontilhado ou linhas diagonais em pixel para sugerir textura). Focinho curto mas protuberante. Olhos reptilianos verticais. Sem orelhas humanas. Cauda curta visível. Porte elegante mas intimidante. Cor das escamas varia: cobre, prata, azul, vermelho — cada personagem tem sua cor.

---

## NPCs NOMEADOS DA CIDADE

Cada NPC nomeado é um personagem único com aparência específica — não são instâncias de um template genérico. Devem ser imediatamente reconhecíveis na cena.

---

### Padre Corvus

**Dimensões:** 32 × 48 px | Raça: Humano de Mana, dornécio

**Descrição:**
Homem de meia-idade com autoridade tranquila. Pele morena-clara, cabelos grisalhos cortados curtos e presos no alto. Olhos castanhos firmes e sérios — olham direto para quem fala. Usa vestes longas de Kanthor, brancas com detalhe dourado nas bordas e colarinho alto. No peito, uma balança de prata pequena (símbolo de Kanthor) presa por corrente. Postura ereta, nunca apressada. Mãos sempre visíveis — não esconde as mãos.

**Cores:**
- Veste: branco-sujo `#E8E4D8` / detalhes dourados: `#C8A030`
- Pele: morena-clara `#B08060`
- Cabelo: grisalho `#909090`
- Balança (símbolo): prata `#B8B8C0`

---

### Mara Vellum

**Dimensões:** 32 × 48 px | Raça: Humana de Mana, dornécia

**Descrição:**
Mulher de expressão analítica e postura controlada. Pele oliva, cabelos pretos em coque severo e bem preso — nenhum fio solto, deliberadamente organizado. Olhos escuros e atentos que parecem estar lendo tudo ao redor. Veste casaco azul-ardósia, fechado até o pescoço, com fileira de botões de bronze. Carrega uma prancheta ou livro-registro embaixo do braço como idle.

**Cores:**
- Casaco: azul-ardósia `#4A5A6A`
- Botões: bronze `#9A6820`
- Pele: oliva `#9A8060`
- Cabelo: preto `#201A14`

---

### Sylveth

**Dimensões:** 32 × 48 px | Raça: Elfa Silvestre

**Descrição:**
Elfa de aparência orgânica — parece ter crescido junto com as plantas. Pele bronze-cobre, quente como terra seca ao sol. Cabelos castanho-musgo trançados com folhas secas e pequenas flores silvestres entrelaçadas (não decoração intencional — cresceram ali). Olhos verde-âmbar que mudam com a luz. Usa roupas naturais em camadas: blusa de linho verde-claro sobre uma camiseta de tom terra, cinturão de couro com bolsinhos de ervas. Orelhas pontudas visíveis entre os cabelos. Postura levemente inclinada para frente — curiosa, próxima.

**Cores:**
- Pele: bronze-cobre `#A07040`
- Cabelo: castanho-musgo `#5A6030` com mechões mais claros `#7A8040`
- Blusa: verde-claro `#7A9A5A`
- Cinturão: couro terra `#8A5830`
- Olhos: verde-âmbar `#7A9030`

---

### Brumdar Ferro-Quieto

**Dimensões:** 32 × 44 px (anão) | Raça: Anão de Khaz Baruk

**Descrição:**
Anão de presença física imediata — parece mais largo que alto. Pele curtida e marcada pelo tempo próximo de forjas. Barba cinza em três tranças grossas presas com argolas de ferro — simples, funcionais, não decorativas. Braços fora de proporção com o resto do corpo — anos de trabalho de ferreiro. Mãos queimadas visíveis em tom mais escuro nas palmas. Avental de couro grosso na frente. Sem capacete — vai contra a tradição por preferência. Expressão de poucas palavras.

**Cores:**
- Pele: curtida `#8A5A38`
- Barba: cinza-médio `#909090` com argolas de ferro `#707070`
- Avental: couro escuro marrom `#4A2A12`
- Cicatrizes de queimadura nas mãos: rosa-escuro `#A06050`

---

### Nimble Galhobaixo

**Dimensões:** 28 × 40 px (halfling) | Raça: Halfling Andarilho de Méritos

**Descrição:**
Halfling com energia permanentemente agitada. Cabelos castanhos claros, costeletas compridas e levemente encaracoladas. Pés grandes e peludos sempre sujos de serragem e terra — nunca usa calçados. Veste um colete de dezenas de bolsos preenchidos — ferramentas, amostras, notas dobradas. Sempre com serragem no cabelo ou na roupa. Expressão de quem acabou de ter uma boa ideia.

**Cores:**
- Pele: morena-clara `#C09060`
- Cabelo: castanho-claro `#A07840`
- Pelos dos pés: castanho-suave `#B89060`
- Colete: marrom-claro `#7A5030` com bolsos de cores variadas

---

### Gurd Carvalho-Torto

**Dimensões:** 36 × 52 px (meio-orc) | Raça: Meio-orc Clã da Fúria

**Descrição:**
Meio-orc que ocupa mais espaço do que parece necessário. Pele cinza-avermelhada. Presas evidentes — saem claramente dos lábios inferiores. Corpo largo como um barril, ombros que quase não passam em portas estreitas. Tatuagens geométricas nos ombros e braços — não decorativas, marcas do clã. Cicatrizes nos ombros de combates passados. Usa camisa de trabalho com as mangas sempre arregaçadas. Expressão brava mas não hostil — é seu rosto em repouso.

**Cores:**
- Pele: cinza-avermelhado `#8A6050`
- Presas: marfim `#E0D4A8`
- Tatuagens: preto-azulado `#202838`
- Cicatrizes: tom mais claro que a pele `#A08070`

---

### Hund Carvalho-Torto

**Dimensões:** 36 × 52 px (meio-orc) | Raça: Meio-orc Clã da Noite

**Descrição:**
Irmão de Gurd, mas visual completamente diferente — mais contido, mais sombrio. Pele cinza-azulada. Presas menores, quase discretas. Olhos escuros que se movem muito antes do corpo. Cabelo raspado nas laterais, mecha mais comprida no topo levemente inclinada. Usa casaco de couro pesado, escuro, sempre fechado. Postura de quem está sempre observando a saída de um cômodo.

**Cores:**
- Pele: cinza-azulado `#607080`
- Casaco: couro quase-preto `#2A2018`
- Olhos: cinza-escuro `#505868`

---

### Ozzra Fumaçazul

**Dimensões:** 28 × 40 px (goblin) | Raça: Goblin Zhak'thul

**Descrição:**
Goblin que transformou a alquimia em identidade visual completa. Pele laranja-acinzentada. Orelhas grandes, uma com um brinco de tubo de vidro. Cabelo azul — não tingido naturalmente, é resultado de um experimento que nunca desfez. Espetado para todos os lados como se pequenas explosões fossem o penteado. Olhos amarelos vivíssimos, sempre brilhando de curiosidade ou êxtase. Avental de couro cheio de manchas coloridas de diferentes experimentos. Múltiplos frascos pendurados por cordas no cinto e no colete — alguns borbulhando.

**Cores:**
- Pele: laranja-acinzentado `#B08050`
- Cabelo: azul-vibrante `#3060C0` com mechas mais claras `#5090E0`
- Olhos: amarelo `#E0C020`
- Avental: couro com manchas multicoloridas (roxo, verde, laranja)

---

### Gruta Panela-Funda

**Dimensões:** 36 × 52 px (orc) | Raça: Orc Clã da Chama Viva

**Descrição:**
Orc que comanda a taverna com a presença de quem já resolveu problemas maiores. Pele verde-escura com reflexos quentes — como se a luz das cozinhas fosse a luz natural dela. Presas fortes, não escondidas, não exageradas. Cabelo ruivo em trança grossa que vai para a frente de um ombro. Avental vermelho gasto com manchas de trabalho. Braços fortes, mãos calejadas. Expressão confiante e direta — sabe exatamente o que quer.

**Cores:**
- Pele: verde-escuro quente `#4A6A30`
- Presas: marfim-sujo `#D8C890`
- Cabelo: ruivo intenso `#C04010`
- Avental: vermelho gasto `#8A2010`

---

### Zrix das Estradas

**Dimensões:** 36 × 52 px (draconato) | Raça: Draconato Cinza do Julgamento, variante cobre

**Descrição:**
Draconato que passou anos em estradas e caminhos — o corpo carrega essa história. Escamas cobre-escurecido que perderam o brilho original pela exposição ao tempo. Placas cinza-pálidas no peito e pescoço (marca da variante do Julgamento). Chifres curtos e levemente quebrados na ponta. Olhos âmbar com pupila vertical. Cauda com marcas de cicatriz — resultado de anos de viagem. Veste roupas funcionais para estrada: manto com capuz (abaixado), botas pesadas, cinturão com equipamento de explorador.

**Cores:**
- Escamas: cobre-escuro `#8A5020` / placas cinza: `#909090`
- Olhos: âmbar `#D09020`
- Cauda/cicatrizes: tom mais claro `#A07040`
- Manto: marrom-viagem `#6A4820`

---

### Yael Noite-Mansa

**Dimensões:** 32 × 48 px | Raça: Elfa da Noite / Luandil

**Descrição:**
A NPC mais visualmente extraordinária da cidade — e está usando isso para se esconder em plain sight. Pele azul-noite, cor que não existe na natureza dornécia. Cabelos branco-prateados extremamente finos, caindo lisos. Olhos violeta que brilham levemente no escuro — não chamativo, mas visível. Tatuagens luminescentes discretas — no pescoço e nos antebraços, padrões de constelações que pulsam muito suavemente. Veste roupas comuns da cidade — deliberadamente não exóticas, para não chamar atenção. A contradição visual (ela é obviamente única) é parte do design.

**Cores:**
- Pele: azul-noite `#304050`
- Cabelo: branco-prata `#E0E8F0`
- Olhos: violeta `#9060D0`
- Tatuagens: azul-luminescente muito suave `#6090C0`
- Roupa: tons neutros intencionalmente comuns (marrom, bege)

---

### Thalindra Véu-de-Lua

**Dimensões:** 32 × 48 px | Raça: Ninrorin (elfa cinzenta)

**Descrição:**
Ninrorin de aparência que parece projetada para comunicar "autoridade intelectual". Pele cinza-clara, quase sem pigmento. Cabelos prata lisos que caem retos até os ombros. Olhos azul-gelo — não frios em personalidade, mas visualmente gelados. Túnica azul-cobalto com fios prateados bordados em padrões geométricos nas bordas. Postura muito ereta. Carrega sempre um pergaminho ou livro na mão.

**Cores:**
- Pele: cinza-claro `#C0C8D0`
- Cabelo: prata `#D0D8E0`
- Olhos: azul-gelo `#80B0D0`
- Túnica: azul-cobalto `#2040A0` com bordado prata `#C0C8D8`

---

### Dagna Rocha-Morna

**Dimensões:** 32 × 44 px (anã) | Raça: Anã de Khaz Baruk

**Descrição:**
Anã robusta que parece tanto mineradora quanto guerreira — porque é as duas. Pele bronzeada de tempo sob luz artificial e trabalho físico intenso. Cabelo preto com mechas brancas naturais — não por idade, por traço do clã. Barba curta trançada com um único anel de cobre. Olhos castanho-escuros determinados. Usa armadura de trabalho: couraça de couro reforçado, braçais de metal. Carrega picareta nas costas como ferramenta mas visualmente funciona como arma.

**Cores:**
- Pele: bronzeada `#9A6838`
- Cabelo: preto `#201818` com mechas brancas `#E0E0D8`
- Barba: preto com anel cobre `#9A6020`
- Couraça: couro escuro `#4A2818`

---

### Pip Semente-Solta

**Dimensões:** 20 × 32 px (criança halfling) | Raça: Halfling Sortudo de Finan

**Descrição:**
Halfling jovem — menor que os halflings adultos. Cabelos ruivos completamente bagunçados, sardas espalhadas pelo rosto. Pés peludos dourados, nunca com calçado. Mochila nas costas que é claramente muito grande para ele — inclina levemente para trás pelo peso. Expressão sempre animada. Comerciante em miniatura: sabe exatamente o valor de tudo que carrega.

**Cores:**
- Pele: pálida-clara `#E0C090`
- Cabelo: ruivo `#C04010`
- Sardas: pontilhados marrom-escuro `#7A3010`
- Pelos dos pés: dourado-claro `#C89050`
- Mochila: couro velho `#7A5030`

---

### Ser Alaric Veyr

**Dimensões:** 32 × 48 px | Raça: Humano de Mana, dornécio

**Descrição:**
Guarda-da-cidade que parece exatamente o que é. Alto, bronzeado pelo tempo ao ar livre. Cabelo castanho curto e bem cuidado. Barba aparada, simétrica. Olhos verdes claros com expressão que mistura cautela e abertura. Usa meia-armadura azul-ardósia — não armadura completa, mas mais que roupa comum — com o símbolo de Kanthor no ombro direito (escudo com estrelas). Postura de militar que ainda acredita no que protege.

**Cores:**
- Armadura: azul-ardósia `#4A5A70` com detalhes metálicos `#909898`
- Símbolo Kanthor: dourado `#C09020`
- Pele: bronzeada `#B07A4A`
- Cabelo: castanho `#6A4428`
- Olhos: verde-claro `#508050`

---

### Mirela dos Laços

**Dimensões:** 32 × 48 px | Raça: Humana de Mana

**Descrição:**
Alfaiate que usa a própria roupa como portfólio. Pele morena, cabelos cacheados escuros com fitas coloridas entrelaçadas — cada fita é de tecido diferente, amostras do trabalho. Olhos castanhos expressivos. Roupas bem ajustadas e estruturadas, diferentes de qualquer outro NPC — ela claramente sabe fazer roupas. Carrega fita métrica no pescoço como colar.

**Cores:**
- Pele: morena `#A07050`
- Cabelo: preto-cacheado com fitas multicoloridas (vermelho, azul, verde, amarelo)
- Roupas: combinação bem planejada — varia por decisão criativa do ilustrador dentro da paleta quente/saturada da cidade

---

### Renko Três-Sorrisos

**Dimensões:** 28 × 40 px (goblin) | Raça: Goblin Zhak'thul, comerciante integrado

**Descrição:**
Goblin que aprendeu que um sorriso abre mais portas que uma faca. Pele laranja-acinzentada. Olhos amarelos semicerrados em expressão permanentemente calculista-amigável. Dentes pequenos mas muitos — o sorriso é um pouco grande demais. Roupa de comerciante: colete com muitos botões, camisa de tecido de qualidade, chapéu pequeno levemente inclinado. Aparenta ser inofensivo. Provavelmente não é.

**Cores:**
- Pele: laranja-acinzentado `#B07850`
- Olhos: amarelo `#D0A010`
- Dentes: branco com espaços entre eles
- Colete: marrom-escuro `#5A3818` com botões dourados

---

### Eiran Valeclaro

**Dimensões:** 32 × 48 px | Raça: Meio-elfo de Thandra

**Descrição:**
Meio-elfo com presença tranquila e natural — parece em paz com os animais e com os silêncios. Pele bronzeada de quem vive ao ar livre. Orelhas levemente pontudas, menos que um elfo puro — precisa de atenção para notar. Cabelo castanho longo preso com um cordão verde. Olhos âmbar com paciência visível neles. Roupas de tratador: simples, práticas, com manchas de grama e ração. Geralmente tem um animal pequeno próximo no idle.

**Cores:**
- Pele: bronzeada `#A07040`
- Cabelo: castanho com cordão verde `#6A4020`
- Olhos: âmbar `#C08030`
- Roupas: verde-claro `#6A8A50` e marrom funcional

---

### Liora Canta-Rio

**Dimensões:** 32 × 48 px | Raça: Humana com sangue nymiriano distante

**Descrição:**
Música de aparência que transmite algo indefinível — aquela estranheza sutil de quem tem sangue antigo muito diluído. Pele dourada-clara, levemente brilhante em certos ângulos (simule com 1-2 pixels de destaque mais claro). Cabelos castanho-escuros ondulados, livres. Olhos azulados — incomuns para uma dornécia, mas não impossíveis. Expressão suave, voz visualmente presente (ela está sempre cantando ou humming no idle). Roupas de tecidos leves e coloridos.

**Cores:**
- Pele: dourada-clara `#D4A060` com destaques `#E0B870`
- Cabelo: castanho-escuro `#4A2A10` ondulado
- Olhos: azulado `#6080A0`

---

### Orlan Pouso-Curto

**Dimensões:** 32 × 48 px | Raça: Humano de Mana, dornécio

**Descrição:**
Homem baixo e largo, pele clara, de aparência gentil e levemente desajeitada. Cabelo castanho ralo, penteado mas sem sucesso real. Bigode curto, simétrico. Olhos gentis — a pessoa mais inofensiva da cidade, e de fato é. Carrega um molho de chaves no cinto que balança no idle. Roupa de taverneiro: camisa branca, colete escuro, avental.

**Cores:**
- Pele: clara `#E0C090`
- Cabelo: castanho-ralo `#7A5030`
- Chaves: metal envelhecido `#A09060`
- Avental: bege-sujo `#C8B080`

---

### Savra Escama-Verde

**Dimensões:** 32 × 52 px (draconata) | Raça: Draconata verde

**Descrição:**
Draconata de aparência que combina delicadeza e perigo. Escamas verde-escuras com manchas oliva irregulares — como uma cobra de floresta. Olhos amarelo-ouro com pupila vertical. Chifres curtos voltados para trás, suaves. Cauda fina e comprida que usa para equilíbrio e expressão (movimenta no idle). Usa roupas de herbálio: avental com bolsos de ervas, braçais leves. Mãos com escamas menores nos dedos — visíveis em close.

**Cores:**
- Escamas: verde-escuro `#2A5A28` / manchas oliva: `#4A6A30`
- Olhos: amarelo-ouro `#D0A010`
- Chifres: cinza-suave `#909888`
- Avental: linho-natural `#C8B880`

---

### Tovin Mãos-de-Selo

**Dimensões:** 24 × 36 px (gnomo) | Raça: Gnomo Artífice

**Descrição:**
Gnomo que parece ter sido construído para trabalho de escritório. Pequeno, pele clara rosada, cabelo castanho penteado com absoluta precisão (cada fio no lugar). Olhos enormes atrás de lentes redondas que ampliam demais — os olhos parecem desproporcionais mesmo para um gnomo. Colete cheio de carimbos, cada um diferente, cada um em seu bolso específico. Dedos longos para o corpo — eficazes em trabalho delicado.

**Cores:**
- Pele: claro-rosado `#E0B890`
- Óculos: armação dourada `#C09020`, lente cinza-translúcida
- Colete: azul-escuro `#2A3A5A` com carimbos coloridos nos bolsos
- Cabelo: castanho bem cuidado `#7A5030`

---

### Maelor Cinza

**Dimensões:** 32 × 48 px | Raça: Elfo da Noite / Luandil

**Descrição:**
Irmão (ou paralelo temático) de Yael, mas completamente diferente em presença. Pele cinza-carvão, mais escura que a de Yael. Cabelo azul-escuro quase negro — profundo. Olhos prateados que refletem luz como metais. Tatuagens luminescentes discretas no pescoço — em padrões mais lineares que os de Yael. Expressão fechada, difícil de ler. Roupas escuras de explorador. Aparece raramente.

**Cores:**
- Pele: cinza-carvão `#3A3A4A`
- Cabelo: azul-quase-preto `#161828`
- Olhos: prata `#B0B8C0` refletivos
- Tatuagens: azul-luminescente `#4060A0`

---

### Sael Maré-Quieta (NOVO — pescador)

**Dimensões:** 32 × 48 px | Raça: Tiefling (primeiro tiefling nomeado da vila)

**Descrição:**
Pescador de presença serena — parece ter absorvido a calma da água parada. Pele azul-acinzentada de tiefling. Chifres pequenos e curvados para trás, discretos. Cauda fina e pontiaguda visível atrás, que toca a água quando senta na doca. Cabelo preto liso, sempre úmido, preso atrás. Olhos âmbar-claro sólidos, sem branco — olham a superfície da água, não as pessoas. Roupa de pescador encerada verde-acinzentada, botas altas de couro, uma rede dobrada no ombro como idle. Fala pouco; lê a água.

**Cores:**
- Pele: azul-noite `#5A7A9C` luz / `#3A5A78` sombra
- Chifres/cauda: cinza-escuro `#3A4048`
- Cabelo: preto-molhado `#1A1A20`
- Olhos: âmbar-claro sólido `#D0B060`
- Roupa encerada: verde-acinzentado `#4A5A50` / botas couro escuro `#3A2A1E`

---

### Mella Forno-Quente (NOVO — padeira/moleira)

**Dimensões:** 32 × 48 px | Raça: Humana de Mana

**Descrição:**
Padeira robusta e calorosa — braços fortes de quem amassa pão todo dia. Pele morena-quente, corada pelo calor do forno (bochechas rosadas visíveis). Cabelo castanho preso num lenço vermelho-terra, com fios soltos polvilhados de farinha. Avental branco manchado de farinha sobre um vestido marrom-quente. Um pano de prato sempre no ombro. Sorriso fácil e direto. No idle, limpa as mãos no avental levantando uma nuvenzinha de farinha.

**Cores:**
- Pele: morena-quente `#C08858` com bochechas rosadas `#D89878`
- Cabelo: castanho `#6A4428` com lenço vermelho-terra `#A03828`
- Avental: branco-farinha `#E8E0D0` (manchas claras)
- Vestido: marrom-quente `#8A5A38`

---

### Hess Couro-Fundo (NOVO — curtidor)

**Dimensões:** 32 × 52 px (draconato) | Raça: Draconato terroso

**Descrição:**
O draconato mais velho e mais quieto da vila. Escamas marrom-terra **opacas, sem brilho** — perderam o lustre com décadas de tanino. Placas gastas no peito e nos antebraços. Focinho curto e pesado; olhos âmbar pálidos e fundos, de pálpebras lentas. Chifres curtos voltados para trás, lascados nas pontas. Cauda grossa que arrasta devagar. Avental de couro rígido, escurecido e manchado, que parece tão curtido quanto as peles que ele trabalha. Mãos enormes e calejadas. Move-se devagar, fala em provérbios. No idle, passa a mão numa pele esticada num bastidor como quem a agradece.

**Cores:**
- Escamas: marrom-terra `#6A5238` luz / `#4A3826` sombra
- Placas: cinza-terra `#8A7858`
- Olhos: âmbar pálido `#C0A050`
- Avental: couro escuro manchado `#3A2818`

---

### Tibbet Vela-Torta (NOVO — coveiro/coroinha; segredo: Nyx)

**Dimensões:** 24 × 36 px (gnomo) | Raça: Gnomo

**Descrição:**
Gnomo pequeno e pálido com cara de quem não dorme à noite — porque não dorme: vela os mortos. Olhos escuros enormes com olheiras profundas. Cabelo fino castanho-acinzentado, meio desgrenhado. Túnica branca de coroinha, larga demais para ele, com a **bainha SEMPRE manchada de terra de cova** (o detalhe que entrega que ele é mais coveiro que coroinha). Carrega uma vela grossa que vive entortando, com cera pingada endurecida na mão. **Sob a gola, escondido, um pingente de luna negra (símbolo de Nyx)** — visível só em retrato de "segredo"/close. Expressão doce e levemente culpada.

**Cores:**
- Pele: pálida `#E0D0C0`
- Olheiras: roxo-acinzentado `#887888`
- Túnica: branco-sujo `#E8E4DC` com bainha terra `#6A5238`
- Vela: bege `#E8D8B0` com chama quente; cera pingada `#F0E8D0`
- Pingente (Nyx): luna negra `#1A1A24` em fio de prata `#B8B8C0`

---

### Ancião Velorin (líder da aldeia)

**Dimensões:** 32 × 48 px (silhueta de elfo, mas a postura curvada o faz parecer mais baixo — desenhe ~4-6 px "comidos" pela curvatura das costas) | Collider: 20×12 px | Raça: Ninrorin (elfo cinzento), muito idoso

**Descrição:**
O NPC mais velho da vila — líder de Cindar's Hope há mais tempo do que a maioria está viva. Tudo nele comunica idade e autoridade tranquila, não fragilidade decorativa. Pele cinza-pálida **enrugada** — marque as rugas com 1-2 pixels mais escuros na testa, ao redor dos olhos e nas mãos (não é a pele lisa da Thalindra; é a mesma linhagem Ninrorin, porém décadas mais velha). Cabelos longos e barba comprida **brancos como cinza**, lisos, descendo pelo peito — dê volume com 2-3 tons de branco-acinzentado, sem brilho. Sobrancelhas brancas e espessas. Olhos âmbar **fundos e cansados**, de pálpebras pesadas — olham devagar, como quem já viu tudo. Orelhas pontudas de elfo, visíveis entre os cabelos longos.

Veste um **manto cívico** longo cinza-azulado (cor de cargo, não de luxo), fechado no peito por uma **fíbula de Kanthor** — a balança, em metal, marcando que ele responde à ordem e à lei. Empunha um **bastão de carvalho** nodoso, que usa de fato como apoio (não cetro cerimonial). Postura curvada para a frente mas **digna** — o peso dos anos, não derrota. No idle, apoia as duas mãos no topo do bastão; movimento lento e econômico. É a "memória viva" da fundação da aldeia — desenhe-o para parecer alguém que guarda segredos sobre a Fonte e sobre Cindar.

**Cores:**
- Pele: cinza-pálida `#C0C4C8` luz / `#9AA0A6` meio / `#788088` sombra (rugas)
- Cabelo/barba: branco-cinza `#E4E4E0` luz / `#C0C0BA` sombra
- Sobrancelhas: branco-acinzentado `#D0D0CA`
- Olhos: âmbar fundo `#A8772E`
- Manto cívico: cinza-azulado `#54627A` com forro/bordas mais escuras `#3A4658`
- Fíbula de Kanthor (balança): prata `#B8B8C0` com balança dourada `#C8A030`
- Bastão de carvalho: madeira `#6A4A28` com nós/veios escuros `#43301C`

**Feeling:** Velho rei sem coroa de uma aldeia de trabalho. Lento, pesado, mas a sala muda de tom quando ele fala.

---

### Criança / Jovem NPC

**Dimensões:** 24–28 × 36–40 px | Collider: 16×10 px

**Descrição:**
Cabeça levemente maior em proporção ao corpo (infantil clássico). Pernas curtas. Movimento mais rápido e desengonçado. Expressão curiosa e aberta. Roupas simples, remendadas, coloridas.

---

### Pet — Gato

**Dimensões:** 24 × 24 px | Collider: 14×10 px

**Descrição:**
Gato doméstico de pixel art no estilo Stardew Valley — corpo oval compact, cabeça redonda, orelhas triangulares, cauda longa curvada. Deve ser adorável mas ainda legível. Idle: orelha mexe ou cauda balança. Variedades de pelo: rajado, preto, laranja, branco-e-preto.

**Cores (gato rajado):**
- Base: laranja `#D4782A` / listras: marrom-escuro `#7A3A10` / barriga: creme `#EED8B0`

---

### Pet — Cachorro

**Dimensões:** 32 × 32 px | Collider: 18×12 px

**Descrição:**
Cão médio, peludo, rabo abanando sempre. Orelhas caídas ou em pé dependendo da raça. Focinho curto e simpático. Língua para fora no idle/feliz. Variedades: caramelo, preto, branco, mesclado.

**Cores (caramelo):**
- Pelo: `#C88040` luz / `#9A5C20` meio / Focinho/patas: creme `#E8C890`

---

# PARTE 2 — FAZENDA

**Tom visual geral:** Stardew Valley. Cores quentes e naturais. Luz de manhã suave. Sombras curtas e verdes. Tudo transmite vida, crescimento e trabalho manual honesto. A fazenda deve parecer um lugar bom de se viver.

---

## TERRENO E TILES DA FAZENDA

### Solo

**32 × 32 px por tile**

- **Grama:** Verde médio `#6AB04C` com variações aleatórias de 1-2 pixels mais escuros ou mais claros para dar textura orgânica. Não uniforme. Pequenas pedras, flores minúsculas e folhas espalhadas aleatoriamente (1-2 pixels por tile, esparsas).
- **Solo seco (arável, não plantado):** Terra marrom-ocre `#A07850` com textura de pedrinhas e torrões. 3-4 tons de marrom alternados.
- **Solo molhado (regado):** Marrom-escuro e rico `#6B4226`, levemente brilhante, textura mais lisa.
- **Solo plantado:** Como solo molhado + pequeno broto emergindo (2-4 pixels)
- **Pedra/caminho:** Cinza pedra `#8C8070` com juntas escuras `#5A5040`. Levemente irregular.
- **Madeira (deck/cerca):** Marrom-madeira `#8B5E3C` com veios horizontais mais claros.

---

## CULTIVOS (CROPS)

**Dimensões base:** 1×1 tile (32×32 px) em fases iniciais | 1×2 tiles (32×64 px) em fase adulta alta

Cada cultivo tem **5 fases visuais** mínimas:
1. Semente/broto (4-6 px de altura, simples)
2. Planta jovem (12-16 px)
3. Planta em crescimento (20-24 px)
4. Planta madura quase pronta (28-32 px ou invadindo segundo tile)
5. Planta pronta para colheita (colorida, brilhante, "chamando atenção")

**+ Estado especial:** Seca/morta (marrom opaco, caída)

### Exemplos de cultivos canônicos:

**Cenoura:**
- Fase 1: pequeno broto verde (2 px)
- Fase final: folhagem verde-escura com pontinha laranja emergindo do solo
- Cor: folha `#3A7A2A`, raiz `#E87820`

**Trigo/Centeio:**
- Fase final: espiga dourada inclinada levemente, espigas pequenas no topo
- Cor: palha `#D4A020` / talo `#8A7020`

**Abóbora:**
- Ocupa 1×1 tile mas a abóbora madura vaza levemente pelas bordas
- Cor: laranja-vibrante `#E86010` / folhas verdes grandes `#3A8A20`

**Flores (decoração/poção):**
- Pequenas, coloridas, delicadas — pétalas claramente distintas mesmo em 6-8 px

---

## ÁRVORES

**Árvore Jovem:** 1×2 tiles (32×64 px)
Tronco fino (4px), copa oval pequena, 3 tons de verde.

**Árvore Adulta (frutífera):** 2×3 tiles (64×96 px)
Tronco médio (8px), copa arredondada densa, frutos visíveis na fase de colheita (bolinhas coloridas de 2-3 px). Copa: 3 tons de verde (`#3A8A20`, `#4AAA30`, `#6AC040`). Tronco: marrom-escuro `#5C3D1E`.

**Árvore Grande (floresta/borda):** 3×4 tiles (96×128 px)
Tronco grosso (14px), copa massiva e irregular, musgo na base. Deve parecer antiga.

**Árvore de Fruto Mana (especial — 1 no jogo):**
5×5 tiles (160×160 px). **Não é cultivável — ela escolhe onde crescer.** Tronco prateado-pálido levemente brilhante. Copa azul-esverdeada com partículas de luz dourada flutuando. Frutos: pequenas esferas branco-douradas que brilham suavemente. Aura etérea ao redor — a única planta que parece ter consciência.

---

## ANIMAIS DA FAZENDA

### Galinha / Pato

**32 × 32 px**

**Galinha:**
Corpo oval compacto, crista vermelha pequena, bico curto e amarelo, patas finas. Animação idle: balanço da cabeça para frente e para trás, bicada no chão. Variedades: branca `#F0F0E0`, caramelo `#D4902A`, preta `#3A3028`.

**Pato:**
Similar mas com pescoço mais longo, bico achatado e mais largo, postura um pouco mais ereta. Macho tem cabeça verde-escura `#2A6030` e anel branco no pescoço.

---

### Ovelha / Cabra

**48 × 40 px**

**Ovelha:**
Corpo totalmente coberto de lã — forma blobosa, arredondada, quase sem pescoço visível. Pernas pequenas saindo embaixo. Cara pontuda com lã ao redor. Lã tosada: corpo menor e mais magro, pele rosada. Lã cheia: bola branca-creme densa (`#EEE8D8` luz / `#C8C0A8` sombra).

**Cabra:**
Corpo mais angular, pescoço visível, barbinha pequena no queixo, chifres curvados para trás (mesmo nas fêmeas, pequenos). Postura mais levantada que a ovelha. Cor: branca, marrom, mesclada.

---

### Vaca

**64 × 48 px**

Forma clássica: corpo grande e retangular, cabeça com focinho largo, chifres curtos, úbere visível embaixo (2-3 pixels). Padrão de manchas preto-e-branco ou marrom-e-branco. Rabo com tufo na ponta. Olhos grandes e dóceis. Cílios exagerados (pixel art expressivo).

**Cores:**
- Branco: `#F0EDE0`
- Manchas: preto `#2A2822` ou marrom `#6A3A1A`
- Focinho: rosa-claro `#E8A8A0`

---

## CONSTRUÇÕES DA FAZENDA

**Estilo:** Arquitetura rural de fantasia européia. Madeira, pedra, telhados de telha vermelha ou colmo. Janelas pequenas com molduras de madeira. Fumaça saindo de chaminés (partícula animada). Nada de concreto ou metal industrial.

### Casa da Fazenda

**10×8 tiles (320×256 px) exterior | 16×12 tiles (512×384 px) interior**

**Exterior:**
Chalé de dois andares. Paredes de madeira horizontais pintadas em bege-sujo `#D8C8A8` com ripas de madeira escura nas juntas `#7A5030`. Telhado de telha vermelha-tijolo `#A04030` com cumeeira de madeira escura. Chaminé de pedra `#8C8070` do lado direito. Janelas 2×2 tiles com moldura branca e vidro azul-verde translúcido `#A8C8B8` (3-4 pixels simulando reflexo de luz). Porta de madeira grossa `#7A5030` com ferragem escura, 2×2 tiles. Varanda pequena na entrada com dois degraus de pedra. Trepadeiras opcionais nas laterais.

**Interior:**
Piso de tábua de madeira `#C89060` com veios. Paredes de madeira mais clara. Lareira no canto com tijolo avermelhado. Cama simples de madeira com colcha colorida. Baú velho no canto. Candeia pendurada no teto (fonte de luz quente). Mesa pequena com banco.

---

### Galinheiro

**8×6 tiles (256×192 px) exterior | 14×10 tiles (448×320 px) interior**

**Exterior:**
Construção baixa de madeira, paredes de ripas com frestas (ventilar). Porta pequena de madeira — mais baixa que um humano, o player precisa agachar (visualmente). Teto de palha/colmo levemente inclinado. Ninhos visíveis através das frestas. Cercado externo de estacas de madeira com tela.

**Interior:**
Feno no chão, ninhos de palha nas laterais com ovos (bolinhas de 2-3 px). Bebedouro e comedouro. Andador horizontal para pousar. Cheirinho visual de caos organizado.

---

### Celeiro (Barn)

**10×8 tiles (320×256 px) exterior | 18×14 tiles (576×448 px) interior**

**Exterior:**
Grande construção com porta dupla dobrável (chave visual). Teto pontiagudo com claraboia no alto. Paredes de madeira pintadas de vermelho-escuro `#8A3020` com detalhes brancos `#E8E0D0`. Luz saindo da claraboia de dia.

**Interior:**
Chão de palha e terra batida. Baias separadas por grades de madeira para as vacas. Fardos de feno empilhados em um canto. Ferramentas penduradas na parede. Cheiro visual (vapor das bocas dos animais no frio, partículas de palha flutuando).

---

### Estufa Pequena

**10×8 tiles (320×256 px) exterior | 16×12 tiles (512×384 px) interior**

**Exterior:**
Armação de madeira com painéis de vidro (simula com pixels de verde-azulado translúcido `#B0D8C0` com brilhos brancos de 1px nas bordas). Porta de madeira com moldura de vidro. Vapores saindo por frestas (partícula branca suave). Planta crescendo visível dentro.

**Interior:**
Canteiros elevados em madeira, terra escura e úmida. Luz verde-dourada difusa (efeito estufa). Mais quente visualmente — paleta mais amarelada que o exterior.

---

### Oficina (Workshop)

**6×5 tiles pequeno (192×160 px) | 10×8 tiles grande (320×256 px)**

**Exterior:**
Construção robusta de pedra e madeira. Chaminé pequena (forja interna). Janela com grade de ferro. Ferramentas penduradas na fachada (machado, martelo). Fumaça constante da chaminé quando ativa. Porta mais larga que a casa.

---

### Fonte da Anya (ESPECIAL — ÚNICA, FIXA)

**6×6 tiles (192×192 px)**

**Descrição:**
A peça mais importante esteticamente da fazenda. Fonte circular de pedra branca-prateada, visivelmente mais antiga que tudo ao redor. A pedra tem veios brilhantes azuis `#60A8E0` como se a luz fluísse pela rocha. A bacia central contém **Água Viva** — água com leve brilho azul-dourado que pulsa suavemente (animada). Ao redor da borda: runas esculpidas que brilham fracamente em dourado `#E0C040`. Flores pequenas e raras crescem espontaneamente ao redor da fonte — não podem ser arrancadas. A água faz sons de fonte viva.

**Feeling:** Sagrado mas acessível. Não intimidante, mas claramente não é obra humana comum. Quem olha sente que há algo maior ali.

**Cores:**
- Pedra: branco-prateado `#E0E8F0` luz / `#B0B8C8` meio / `#8090A8` sombra
- Runas brilhantes: dourado `#E0C040`
- Água: azul translúcido `#60A8F0` com brilhos brancos animados
- Flores ao redor: brancas e douradas `#F0F0E0` / `#E0C040`

---

### Raiz Dormente de Mana (ESPECIAL — ENDGAME, FIXA)

**5×5 tiles (160×160 px)**

**Descrição:**
Estrutura de raiz gigante brotando do chão, visivelmente conectada a algo profundo abaixo. A raiz principal tem espessura de um tronco de árvore adulta mas é pálida, quase branca, com veios azuis e dourados. Ao redor, outras raízes menores emergem do solo. O ponto central tem um broto fechado, como um botão floral de luz — quando plenamente ativado, este botão se abre em uma flor de luz azul-branca que ilumina a área.

**Cores:**
- Raiz: branco-sujo `#E8E0D0` com veios azuis `#60A0E0` e dourados `#D4A820`
- Broto fechado: azul-cristal `#80C0F0`
- Broto aberto (endgame): branco-azulado brilhante com aura dourada

---

### Props Gerais da Fazenda

| Prop | Dimensões | Descrição visual |
|---|---|---|
| **Canteiro** | 32×32 | Retângulo de terra escura delimitado por tábuas de madeira. Simples. |
| **Cerca (segmento)** | 32×32 | Duas estacas de madeira com um ripão horizontal. Madeira clara desgastada. |
| **Portão de cerca** | 64×32 | Como dois segmentos com dobradiças metálicas. Abre/fecha animado. |
| **Poço** | 96×96 (3×3) | Cilindro de pedra, telhado triangular de madeira, balde pendurado em corda. |
| **Barril** | 32×32 | Barril de madeira com arcos de metal. Inclinado levemente. |
| **Caixote** | 32×32 | Caixa simples de madeira com tampa semi-aberta. |
| **Espantalho** | 32×64 | Estaca com chapéu de palha, roupa velha recheada de palha, braços abertos. Levemente torto. |
| **Composteira** | 64×64 | Caixa de madeira sem tampa, restos vegetais coloridos dentro. |
| **Lanterna de poste** | 32×64 | Poste de madeira, lanterna de vidro no topo. Emite luz laranja-quente à noite. |
| **Banco** | 64×32 | Banco simples de madeira, dois suportes, tábua horizontal. |
| **Sinal/Placa** | 32×32 | Poste de madeira com placa retangular. Texto gravado. |
| **Pedra pequena** | 32×32 | Pedra arredondada cinza, removível. |
| **Pedra média** | 64×64 | Amontoado de 2-3 pedras, mais resistente. |
| **Rocha grande** | 96×96 | Rocha maciça, impõe presença, bloqueia caminho. |
| **Toco de árvore** | 32×32 | Toco cortado, veios visíveis, musgo na borda. |
| **Tronco caído** | 64×64 | Tronco médio caído com galhos menores. |

---

# PARTE 3 — CIDADE (CINDAR'S HOPE)

**Tom visual:** Cidade medieval de fantasia funcional. Não é grandiosa nem rica — é uma cidade de trabalho, mistura de raças, vida real. Ruas de pedra com irregularidades, construções de dois andares, toldos coloridos de lojas, letreiros pintados à mão. Alguns enfeites de magia (globos de luz, runas nas portas) mas discretos. Muito pixel art denso e detalhado.

**Escala total:** 128×96 tiles (4096×3072 px)

---

## CONSTRUÇÕES DA CIDADE

### Templo de Kanthor

**Exterior:** 18×14 tiles (576×448 px) | **Interior:** 24×18 tiles (768×576 px)

**Exterior:**
O maior e mais imponente edifício da cidade. Arquitetura de pedra cinza-clara `#C0C0B0` com arcos góticos nas janelas. Porta central de 3×3 tiles — porta dupla de madeira escura com arco de pedra. Símbolos de Kanthor gravados na pedra acima da porta (escudo com três estrelas). Dois pilares na entrada. Teto escalonado com pequena torre sineira. Janelas altas e estreitas com vidro colorido (azul e dourado). Escadaria de 3 degraus na entrada. Sensação de autoridade e peso.

**Cores:**
- Pedra: `#C8C0B0` luz / `#909080` sombra
- Porta: madeira escura `#4A3020`
- Vidro: azul-royal `#3060A0` com dourado `#D0A020`

---

### Câmara Municipal

**Exterior:** 16×12 tiles (512×384 px) | **Interior:** 22×16 tiles (704×512 px)

**Exterior:**
Construção imponente mas acessível. Dois andares, bandeira da cidade no topo (mastro), janelas grandes com grades decorativas. Porta larga de dupla folha em madeira escura. Placa com o brasão da cidade. Escadaria de 2 degraus. Relógio na fachada superior (decorativo). Paredes de pedra de cantaria cinza `#A0A090` com janelas emolduradas em madeira.

---

### Taverna e Estalagem ("O Copo Rachado" ou similar)

**Exterior:** 20×14 tiles (640×448 px) | **Interior:** 28×20 tiles (896×640 px)

**Exterior:**
O mais animado visualmente da cidade. Letreiro colorido pintado à mão com ilustração de caneco de cerveja. Toldo listrado — vermelho e bege. Janelas grandes com luz quente saindo à noite (âmbar `#E09030`). Barris na calçada. Bêbado animado dormindo encostado na parede (NPC idle engraçado). Dois andares — segundo andar é a estalagem. Fumacinha saindo da cozinha.

**Interior:**
Salão térreo com mesas circulares, bancos de madeira, lareira central ou lateral, balcão de bar com barris atrás. Escada para o segundo andar visível no fundo. Luzes de vela. Piso de madeira gasto. Troféus de caça nas paredes.

---

### Ferraria de Brumdar

**Exterior:** 14×12 tiles (448×384 px) | **Interior:** 18×15 tiles (576×480 px)

**Exterior:**
Construção robusta de pedra escura e tijolo. Fumaça densa e constante saindo de chaminé larga. Bigorna exposta na entrada (como vitrine/convite). Janela estreita de grade com brilho de forja visto de fora. Ferramentas na fachada (martelo, tenaz). Barulho visual — partículas de faísca saindo da janela quando ativa.

**Interior:**
Forja central com brasa viva (brilho laranja pulsante animado). Bigorna ao lado. Ferramentas penduradas em todo lugar. Barris de água para temperar metal. Armas e ferramentas finalizadas em exposição. Piso de pedra com marcas de queimado.

**Cores internas:**
- Brilho da forja: laranja-vermelho `#E06010` com halo amarelo `#F0C040`
- Paredes: tijolo escuro `#6A3020`

---

### Loja de Sementes (Sylveth)

**Exterior:** 12×10 tiles (384×320 px) | **Interior:** 16×13 tiles (512×416 px)

**Exterior:**
Lojinha charmosa. Fachada verde-clara com flores na janela (vasinhos). Letreiro em madeira natural com semente esculpida ou pintada. Toldo verde. Aroma visual (flores coloridas na entrada, ervas penduradas).

**Interior:**
Prateleiras com sacos de sementes marcados. Vasos com plantas crescendo. Cheiro de terra (visual: solo escuro nas prateleiras). Balcão baixo de madeira clara.

---

### Alquimia de Ozzra

**Exterior:** 12×10 tiles (384×320 px) | **Interior:** 17×14 tiles (544×448 px)

**Exterior:**
Fachada levemente sinistra-charmosa. Vidraça com poções coloridas expostas (vermelho `#C02020`, azul `#2050C0`, verde `#20A040`, roxo `#8020C0`). Fumaça colorida saindo de algum tubo. Letreiro com ícone de frasco. Plantas estranhas nas janelas.

**Interior:**
Bancada de alquimia com destilador de vidro borbulhante. Prateleiras até o teto com frascos. Livros empilhados. Cabeça de esqueleto como decoração. Luz esverdeada proveniente de algum experimento.

---

### Arquivo (Thalindra)

**Exterior:** 14×12 tiles (448×384 px) | **Interior:** 20×16 tiles (640×512 px)

**Exterior:**
Construção sóbria e imponente. Pedra mais escura. Porta com tranca visível. Janelas pequenas com grades. Símbolo de livro ou coruja gravado. Parece um banco ou cartório.

**Interior:**
Estantes altas de livros. Escada de biblioteca em trilho. Mesa central com mapas espalhados. Velas em candelabros. Globo ou esfera mágica em exposição. Tons âmbar-escuro, madeira rica.

---

### Loja Noturna de Yael (ESPECIAL)

**Exterior:** 10×8 tiles (320×256 px) | **Interior:** 14×10 tiles (448×320 px)

**Exterior:**
Parece fechada de dia — porta sem letreiro, janelas cobertas. À noite: uma vela acende na janela, a porta está entreaberta, uma placa pequena aparece: "Aberto". Interior emite luz roxa-azulada fraca `#4020A0`.

**Interior:**
Bagunça intencional de objetos raros. Objetos flutuantes, caixas com correntes, frasco de olhos. Mesa baixa com itens misteriosos. Luz de vela roxa. Yael é uma presença que você sente antes de ver.

---

### Cabana de Pesca (Sael — NOVO)

**Exterior:** 12×10 tiles (384×320 px) | **Interior:** 16×12 tiles (512×384 px)

**Exterior:**
Cabana de madeira cinza-úmida sobre estacas à beira do **Lago/Parque (SW)**. Uma **doca de tábuas** avança sobre a água (onde Sael pesca no idle). Redes penduradas secando na lateral (bege-encardido). Um **defumador** ao lado — caixa de madeira baixa soltando fumaça branca suave e constante (conserva peixe). Barris de peixe na doca. Telhado de colmo levemente caído. Sensação úmida, calma, cheiro de água.

**Interior:**
Chão de tábua úmida com poça. Mesa de limpar peixe com faca. Ganchos no teto com peixes defumados pendurados. Baldes, varas encostadas na parede, caixas de isca. Luz fria-azulada entrando pela janela que dá para o lago.

**Cores:**
- Madeira: cinza-úmido `#6A6258` / colmo: palha-escura `#8A7848`
- Rede: bege-encardido `#B8A878` / fumaça do defumador: branco-suave
- Água (reflexo): azul-frio `#5A8AA8`

---

### Padaria e Moinho (Mella — NOVO)

**Exterior:** 14×11 tiles (448×352 px) | **Interior:** 18×14 tiles (576×448 px)

**Exterior:**
Construção de pedra clara e madeira perto do **distrito de mercado**, com um **moinho** anexo — roda de mó grande (de vento, ou d'água se houver riacho) girando devagar (animada). **Forno de pão de tijolo** com chaminé larga soltando fumaça e o "cheiro" visual (partículas douradas suaves) — o ponto mais acolhedor da cidade. Janela com **pães expostos** numa prateleira. Saco de farinha aberto encostado na porta (rastro branco no chão). Toldo bege-trigo.

**Interior:**
Forno de tijolo central com brasa viva (brilho laranja-quente). Mesa larga de amassar coberta de farinha. Prateleiras com pães, bolos e tortas. Sacos de grão e de farinha empilhados. A **mó** (pedra circular) num canto, com farinha caindo num cocho. Luz quente-dourada, ar com partículas de farinha flutuando.

**Cores:**
- Pedra: clara `#C8C0A8` / tijolo do forno: avermelhado `#A04830`
- Brasa do forno: laranja `#E06010` com halo `#F0C040`
- Farinha: branco-creme `#EEE8D8` / toldo: bege-trigo `#D8C088`

---

### Tanoaria (Hess — NOVO)

**Exterior:** 12×10 tiles (384×320 px) | **Interior:** 16×13 tiles (512×416 px)

**Exterior:**
Na **borda da cidade**, afastada do centro (pelo cheiro do ofício). Construção baixa de madeira escura e pedra. Na frente, **bastidores de pele esticada secando ao sol** — retângulos de couro em tons variados (creme, castanho, escuro). Ao lado, **tanques de tanino** (cubas de madeira com líquido marrom-escuro). Couros pendurados em varas. Telhado simples de tábua. Tom terroso, rústico, trabalhador.

**Interior:**
Bancada de curtir com facas de raspar (descarnar). Tanques de imersão no chão. Ferramentas penduradas. Pilhas de couro acabado num canto. Paleta marrom saturada (animal + tanino), luz baixa.

**Cores:**
- Madeira: escura `#4A3826` / pedra: cinza-terra `#7A6E5A`
- Couros: creme `#C8B088` a castanho `#7A5030` a escuro `#3A2818`
- Tanino: marrom-escuro `#2E1E12`

---

### Sacristia / Casinha do Coveiro (Tibbet — NOVO)

**Exterior:** 8×6 tiles (256×192 px) | **Interior:** 12×8 tiles (384×256 px)

**Exterior:**
Pequena construção de pedra **anexa ao Templo**, voltada para o cemitério — quase escondida atrás dele. Porta baixa de madeira. Uma única janelinha que à noite mostra luz de vela fraca (Tibbet velando os mortos). Pás e ferramentas de cova encostadas na parede externa. Discreta, humilde, sombreada.

**Interior:**
Minúsculo. Catre simples, prateleira cheia de velas (sempre uma acesa). Ferramentas de cova. **O segredo:** atrás de um pano na parede, uma pequena imagem de **Nyx** — uma luna negra com véu de estrelas — que Tibbet revela só à noite. Luz de vela quente contrastando com o ícone frio e escuro de Nyx.

**Cores:**
- Pedra: cinza-musgo `#6A6858` / madeira: escura `#3A2E20`
- Luz de vela: âmbar-quente `#E0A040`
- Ícone de Nyx (escondido): preto-noite `#16161E` com prata-estrela `#C0C8D8`

---

### Props da Cidade

| Prop | Dimensões | Descrição visual |
|---|---|---|
| **Lampião de rua** | 32×64 | Poste de ferro forjado, globo de vidro no topo. Luz laranja à noite. |
| **Tenda de feira** | 96×96 (3×3) | Tenda triangular com lona listrada, mesa exposta de produtos. |
| **Banco de praça** | 64×32 | Banco de pedra, dois braços laterais esculpidos. |
| **Fonte da praça** | 64×64 (2×2) | Fonte circular de pedra funcional, menor que a Fonte de Anya. |
| **Estatua (média)** | 64×96 (2×3) | Figura esculpida em pedra, levemente desgastada. |
| **Estatua (grande)** | 96×128 (3×4) | Estátua imponente, sugestão de Anya ou figura histórica. |
| **Árvore de praça** | 64×96 (2×3) | Árvore ornamental bem cuidada, formato oval bonito. |
| **Barril de rua** | 32×32 | Barril de madeira encostado na parede de alguma loja. |
| **Placa de loja** | 32×32 a 64×32 | Placa de madeira pendurada, ícone pintado identificando o serviço. |
| **Cachorro de rua** | 32×32 | NPC animal vagando, gordinho, sem coleira. |

---

## PORTÃO DA CAVERNA

**24×20 tiles (768×640 px) exterior**

**Descrição:**
Esta é a transição mais dramática do jogo. O portão da caverna fica no limite da cidade. Uma construção de pedra maciça — não foi construída pelos habitantes atuais, foi **encontrada já existente**. A pedra é mais escura que o restante da cidade, quase negra. Runas antigas ao redor da abertura que brilham fracamente em azul-frio `#4080C0`. Dois postos de guarda humanos dos lados (estrutura de madeira e pedra, mais recente). A abertura em si é um arco escuro com vento frio saindo — escuridão total além do portão. Correntes pesadas nas laterais (simbólicas, não fecham completamente).

**Feeling:** Conveniente mas levemente ominoso. A cidade normaliza a presença da caverna, mas o portão deixa claro que o que existe lá dentro não foi feito por humanos.

---

# PARTE 4 — CAVERNA

**Tom visual:** Children of Morta. Escuridão como elemento ativo. Luz artificial como recurso valioso. Inimigos que são silhuetas densas em fundo escuro. Cada bioma tem identidade visual própria — o jogador sabe onde está olhando para as cores.

**Regras visuais da caverna:**
- Fundo: sempre escuro (nunca branco ou neutro)
- Iluminação: o player tem raio de visão iluminado — tudo fora é escuro ou em silhueta
- Tiles de parede devem ter textura — não são blocks uniformes
- Cada bioma tem sua paleta de destaque que corre pelos tiles de parede, chão e inimigos

---

## TILES DE AMBIENTE POR BIOMA

### Bioma 1 — Caverna de Pedra (níveis 1-10)

**Chão:** Pedra basalto escura `#3A3830` com textura irregular, rachaduras pequenas, pequenas poças d'água preta refletindo a luz do player.
**Parede:** Basalto cinza-escuro `#4A4840` com estratos horizontais. Estalactites saindo do teto.
**Detalhe:** Musgo verde-escuro `#2A4020` nas juntas. Pingos de água ocasionais. Fungos bioluminescentes pequenos em cantos (`#80E080` — verde-limão suave).
**Luz ambiente:** Fria, azul-acinzentada muito fraca.

### Bioma 2 — Floresta Subterrânea (níveis 11-25)

**Chão:** Terra escura e úmida com raízes espalhadas. Musgo grosso em áreas. Cogumelos bioluminescentes como iluminação ambiente (`#A0E0A0` verde, `#E0C060` amarelo).
**Parede:** Pedra coberta de trepadeiras subterrâneas e raízes que penetram a rocha. Raízes em marrom-escuro `#4A2A10`.
**Detalhe:** Fungos gigantes (prop decorativo, 2×3 tiles), telas de aranha, agua correndo pela parede.
**Luz ambiente:** Verde-suave dos fungos, a mais "viva" dos biomas escuros.

### Bioma 3 — Caverna de Gelo (níveis 26-40)

**Chão:** Pedra coberta de gelo azul-translúcido. Reflexo do jogador no chão de gelo. Estalactites e estalagmites de gelo.
**Parede:** Pedra com gelo incrustado. Cristais azuis `#60A0E0` emergindo da rocha. Vapor frio saindo de fissuras.
**Detalhe:** Esqueletos/objetos congelados visíveis dentro do gelo (2-3 pixels de detalhe).
**Luz ambiente:** Azul-frio `#6090C0` muito fraca, faz o ambiente parecer letárgico.

### Bioma 4 — Caverna de Fogo (níveis 41-55)

**Chão:** Pedra negra com fissuras incandescentes `#C04010`. Cinzas espalhadas.
**Parede:** Rocha negra com lava visível em fissuras, rachaduras laranja-vermelhas.
**Detalhe:** Poças de lava como obstáculo/prop (brilho intenso, laranja `#E06010`). Fumaça subindo. Pilares de pedra negra.
**Luz ambiente:** Laranja-quente pulsante, a mais "quente" da caverna. Contraste dramático com o fundo negro.

### Bioma 5 — Ruínas Antigas (níveis 56-70)

**Chão:** Pedra polida de outra era — diferente da pedra bruta. Azulejos decorativos em padrão geométrico, parcialmente quebrados. Engrenagens quebradas no chão.
**Parede:** Alvenaria antiga de pedra trabalhada. Frescos e relevos nas paredes (parcialmente visíveis). Runas gravadas que pulsam levemente em azul-roxo `#6040C0`.
**Detalhe:** Constructos quebrados encostados nas paredes. Livros apodrecidos. Estantes de metal enferrujado.
**Luz ambiente:** Azul-roxo arcano muito fraco. Tecnológico e misterioso.

### Bioma 6 — Abismo Sombrio (níveis 71-85)

**Chão:** Pedra negra quase sem textura. Sombras que se movem independente da luz. Fissuras com vazio profundo.
**Parede:** Pedra negra com teias de sombra. Olhos que aparecem e desaparecem nas paredes (animação de idle ambiente — aterrorizante).
**Detalhe:** Portais de sombra pequenos como decoração (oval escuro com borda roxa). Cristais negros `#2A1A3A`.
**Luz ambiente:** Quase zero. O jogador vê apenas o seu raio de luz — o resto é escuridão absoluta com detalhes roxos-escuros.

### Bioma 7 — Núcleo Corrompido (níveis 86-99)

**Chão:** Pedra negra com veios de Pedra Negra (Blackstone) — brilho roxo-escuro `#6020A0` pulsando.
**Parede:** Blackstone maciço. Rosto de criatura esculpido (ou formado naturalmente) nas paredes — olhando para o jogador.
**Detalhe:** Cristais de corrupção negros e roxos, estruturas dracônicas parcialmente fundidas com a rocha.
**Luz ambiente:** Roxo-profundo `#4A1060` pulsante e doentio.

---

## INIMIGOS — CAVERNA DE PEDRA (Níveis 1-10)

---

### Ácaro da Fenda (`enemy_cave_mite`) — TINY 16×16 px

**Descrição:**
Inseto minúsculo que mal distingue-se individualmente — perigoso em grupos. Corpo oval achatado, marrom-cinzento, 6 patas curtas que se movem rapidamente. Cabeça quase inexistente — apenas mandíbulas apontadas para frente. Olhos compostos minúsculos com um único pixel branco de reflexo. O design inteiro tem que funcionar em 16×16 — priorize silhueta de inseto reconhecível.

**Cores:**
- Corpo: cinza-pedra `#5A5450` / marrom-escuro `#3A2A20`
- Patas: marrom-claro `#6A5040`
- Olhos: branco com 1 pixel `#F0F0F0`

**Animações essenciais:** idle tremendo (2 frames), corrida rápida para frente (4 frames), mordida (2 frames), morte esmagado (flat no chão).

---

### Rato de Basalto (`enemy_stone_rat`) — SMALL 24×32 px

**Descrição:**
Rato grande, não domesticável, claramente perigoso. Corpo baixo e alongado. O dorso é coberto por pequenas placas de pedra basalto fundidas com a pele — como uma armadura natural. Focinho pontudo e preto. Rabo grosso e comprido. Olhos âmbar refletem luz.

**Cores:**
- Dorso/placas: cinza-basalto `#6A6860` / linhas de separação: preto `#2A2822`
- Pelo do ventre/rosto: marrom-escuro `#4A3020`
- Focinho: preto `#201810`
- Olhos: âmbar `#D49020`

---

### Morcego de Fenda (`enemy_cave_bat`) — TINY 16×16 px

**Descrição:**
Morcego pequeno mas ágil. Asas proporcionalmente grandes para o corpo (são o que define a silhueta — o corpo é mínimo). Orelhas bem grandes e pontiagudas. Dentes de vampiro visíveis em 2 pixels. Cor escura com bordas azuladas para distinguir das trevas.

**Cores:**
- Asas: roxo-escuro `#3A2850` com bordas azuladas `#4A3870`
- Corpo: preto `#201820`
- Olhos: vermelho `#C02020` (2 pixels)
- Dentes: branco `#E0E0D8`

---

### Saqueador Grash'naar (`enemy_goblin_grashnaar_scavenger`) — SMALL 24×32 px

**Descrição:**
Goblin de caverna — oportunista, covarde, perigoso em grupo. Corpo curvado para frente, postura sempre agachada. Usa uma faca curta e enferrujada. Carrega uma mochila improvisada de sucata nas costas — pedaços de metal amarrados com couro. Roupa de trapos marrons. Nariz grande e torto. Orelhas rasgadas.

**Cores:**
- Pele: verde-acinzentado `#7A9B6A` / luz: `#9ABB8A`
- Trapos: marrom-sujo `#6A5030`
- Mochila de sucata: metais enferrujados `#7A6050`, cobre `#9A6820`
- Faca: metal opaco `#7A7A70` com ferrugem `#8A4020`
- Olhos: amarelo-âmbar `#D4A020`

---

### Broto Raiz-Negra (`enemy_blackroot_sprout`) — SMALL 24×32 px (fixo/semifixo)

**Descrição:**
Planta agressiva que brota do chão da caverna. Não é um animal — é uma raiz consciente. Aparece como uma mão vegetal emergindo do solo, com 3-4 "dedos" de raiz retorcida e enegrecida. Seiva roxa-escura pinga das pontas. Folhas pequenas e escurecidas ao redor da base. Quando ataca, dispara agulhas ou tenta agarrar.

**Cores:**
- Raiz: preto-marrom `#2A1810` / luz: `#3A2820`
- Seiva: roxo-escuro `#601860`
- Folhas: verde-muito-escuro `#1A3010`
- Agulha (projétil): marrom-escuro pontudo `#4A2810`

---

### Cubo de Lodo Translúcido (`enemy_translucent_sludge_cube`) — LARGE 48×48 px

**Descrição:**
Gelatina viva de cor verde translúcida. Forma de cubo/blob arredondado que se arrasta pelo chão. Dentro do corpo translúcido, pequenos objetos são visíveis — moedas, ossos, pedaços de armadura de aventureiros anteriores. O corpo pulsa levemente ao respirar. Ao avançar, deixa rastro brilhante no chão.

**Cores:**
- Corpo: verde-translúcido `#40C040` com alpha (simule com transparência por pontilhado em pixel)
- Brilho ácido nas bordas: verde-amarelado `#A0E040`
- Objetos internos: tons de marrom/dourado/cinza visíveis em silhueta
- Rastro: verde-sujo `#208020`

---

### Baú-Mordente (`enemy_chest_biter`) — SMALL 32×32 px

**Descrição:**
Esta é a ilusão mais importante do jogo — o Baú-Mordente parece 100% um baú comum até o momento em que o jogador se aproxima. Fase 1 (inativo): baú velho de madeira escura, ferragens enferrujadas, fechado. Fase 2 (ativado): a tampa se abre revelando dentes irregulares brancos, uma língua vermelha curta, e as "pernas" (pés do baú) ganham vida como patas. Os olhos aparecem no interior escuro.

**Cores (fase baú):**
- Madeira: marrom-escuro envelhecido `#4A3020` / veios: `#3A2010`
- Ferragens: enferrujado `#7A5030`

**Cores (fase monstro):**
- Interior: vermelho-escuro `#801020`
- Dentes: marfim irregular `#E0D8B0`
- Língua: vermelho-vivo `#C02030`
- Olhos: amarelo `#D0A010` (2 pixels cada)

---

## INIMIGOS — CAVERNA DE PEDRA (Níveis 1-10) — continuação

---

### Batedor Kobold (`enemy_kobold_scout`) — SMALL 24×32 px

**Descrição:**
Reptiliano pequeno de postura alerta. Corpo esguio com escamas de couro-terra, focinho longo e levemente curvado para baixo. Olhos amarelos e precisos — sempre calculando distância. Usa lança curta mais alta que ele, e uma bolsa de pedras no cinto para arremessar. Colete de couro com marcações do clã nas bordas. Idle: vira a cabeça de um lado para outro como lagarto, raramente fica parado.

**Cores:**
- Escamas: cobre-terra `#8A6030` / suaves: `#A07840`
- Couro: marrom-claro `#7A5030`
- Olhos: amarelo-vivo `#D0A010`
- Lança: madeira escura ponta cinza `#707870`

---

### Musguinho Errante (`enemy_mossling`) — SMALL 24×32 px

**Descrição:**
Bolota viva de musgo e fungo. Corpo quase esférico, mais largo que alto, com pernas curtas demais que mal sustentam o peso — cambaleia ao andar. Braços curtos terminam em "mãos" de raiz grossa. O corpo inteiro é coberto de musgo verde e fungos pequenos de diferentes tamanhos. Não tem rosto óbvio — dois pequenos orifícios que são os olhos, e uma fenda para a boca. Lento mas absorve golpes.

**Cores:**
- Corpo: verde-musgo `#4A6A30` / partes mais escuras: `#3A4A20`
- Fungos no corpo: laranja `#D06010`, branco `#E0D8C0`, marrom `#6A4020`
- Olhos: amarelo-esporo `#C0A020` (dois orifícios pequenos)

---

### Osso Rachado (`enemy_cracked_bone`) — MEDIUM 32×48 px

**Descrição:**
Esqueleto incompleto e assimétrico — não é o esqueleto clássico bem montado, é o que sobra depois que algo foi desmontado e remontado errado. Um braço é maior que o outro. A postura é torta, curvada para um lado. Rachaduras visíveis nos ossos, especialmente no crânio. Nos olhos: brilho frio azul-esbranquiçado, não a chama típica do morto-vivo — mais fraco e instável. Quando morre, desmonta peça por peça.

**Cores:**
- Osso: bege-envelhecido `#D0C490` / sombras nas juntas: `#8A7840`
- Rachaduras: preto `#201810`
- Brilho dos olhos: azul-frio `#80A0C0`

---

### Besouro Ferrugem (`enemy_rust_beetle`) — SMALL 24×32 px

**Descrição:**
Besouro largo e baixo com carapaça de aparência metálica, mas é biológica — apenas parece ferrugem. Antenas longas que se movem constantemente, farejando metal. Mandíbulas pequenas mas poderosas — desenhadas para mastigar metal, não carne. Pó metálico laranja-ferrugem fica ao redor onde ele passa. Seis patas visíveis, movimento lateral às vezes.

**Cores:**
- Carapaça: laranja-ferrugem `#B05818` / marcas: marrom-escuro `#5A2810`
- Antenas: marrom `#6A4020`
- Pó no chão: laranja-pálido `#C08040`
- Mandíbulas: cinza-metálico `#707870`

---

## INIMIGOS — FLORESTA SUBTERRÂNEA (Níveis 11-25)

---

### Armeiro Uru'dakh (`enemy_goblin_urudakh_trapper`) — SMALL 24×32 px

**Descrição:**
Goblin mais inteligente que o Grash'naar. Usa máscara parcial de couro cobrindo metade do rosto. Nas costas, uma estrutura de armadilhas — arminhas de metal pequenas visíveis como silhueta na costas. Mais robusto que o saqueador. Tom de pele mais escuro, marcas de guerra pintadas.

**Cores:**
- Pele: verde-escuro `#5A7A48`
- Máscara: couro preto `#2A1A10`
- Armadilhas (metal): cinza-azulado `#607080`
- Marcas de guerra: vermelho `#A02010`

---

### Cervino Oco (`enemy_hollow_stagling`) — LARGE 48×56 px

**Descrição:**
Cervo subterrâneo com algo profundamente errado. Corpo magro a ponto de contar os ossos. Pernas longas e finas. Chifres tortos e irregulares — nunca simétricos. O peito tem uma **cavidade aberta** — como se o coração tivesse sido removido e agora há um buraco com brilho verde interno (como se algo vivesse dentro). Olhos brancos sem íris.

**Cores:**
- Pelo: cinza-frio `#7A8090` / partes sem pelo (ossudas): cinza-pálido `#C0C0B0`
- Chifres: branco-sujo `#D8D0B0`
- Cavidade interna: verde-bioluminescente `#40C060`
- Olhos: branco-total `#F0F0E0`

---

### Baluarte Micélio (`enemy_mycobulwark`) — LARGE 48×56 px

**Descrição:**
Fungo vivo gigante com forma humanoide. O "escudo" é uma placa de fungo compacto crescendo do braço esquerdo. Corpo branco-sujo e esponjoso. Superfície irregular com texturas de fungo — não lisa. Pequenos cogumelos crescem nas costas e ombros. Quando ataca, solta aura de esporos amarelos.

**Cores:**
- Corpo: branco-sujo `#E0D8C8` / sombras: marrom-claro `#A08060`
- Placa/escudo: cinza-fungo `#8A8070`
- Cogumelos no corpo: carapaça marrom `#6A4A30` / chapéu: várias cores (laranja, vermelho)
- Esporos: amarelo-claro `#E8E040` (partícula)

---

### Mariposa de Nyx (`enemy_nyx_moth`) — SMALL 24×32 px

**Descrição:**
Mariposa grande com asas abertas e padrão de "olho" nas asas (padrão real de mariposa). O "olho" nas asas é sinistro — olha para o jogador. Corpo delicado mas aura de perigo. À noite da caverna, emite pó que blinda a visão.

**Cores:**
- Asas: azul-escuro `#1A2050` com padrão prata `#A0A8C0` e roxo `#602870`
- Padrão de olho: azul-claro `#6080C0` com pupila branca `#E0E8F0`
- Corpo: azul-escuro, antenitenas longas e arqueadas

---

### Diabrete de Esporo (`enemy_spore_imp`) — SMALL 24×32 px

**Descrição:**
Diabrete pequeno que cresceu num ambiente de fungos e absorveu sua natureza. Corpo magro e anguloso, quase frágil. No topo da cabeça, um chapéu fúngico natural — não colocado, cresceu ali, parte do crânio. Braços finos com dedos longos. Quando conjura, uma nuvem de esporos amarelos sai das mãos e do chapéu. Expressão de diversão maliciosa. Pula mais do que corre.

**Cores:**
- Pele: verde-pálido `#80A060`
- Chapéu fúngico: lilás `#9060A0` / manchas: amarelo-esporo `#D0C040`
- Olhos: amarelo-brilhante `#E0C020`
- Esporos (partícula): amarelo-claro `#E8E060`

---

### Garra-Raiz (`enemy_rootsnare`) — MEDIUM 32×48 px (fixa)

**Descrição:**
Não é um animal — é uma raiz viva emergindo do chão. Forma de garra ou mão de 3-4 "dedos" de raiz grossa e torta. Marrom-escuro com seiva preta pingando das juntas. Pontas dos dedos terminam em verde-vivo — a única parte ainda crescendo. Quando detecta o player, o chão ao redor treme levemente antes de emergir (telegrafação visual). Quando agarra, os dedos se fecham como uma garra real.

**Cores:**
- Raiz: marrom-escuro `#3A2010` / partes mais claras: `#5A3820`
- Seiva: preto-quase-roxo `#20101A`
- Pontas vivas: verde-vivo `#40A020`

---

### Espinhador Sombrio (`enemy_thorn_archer`) — SMALL 24×32 px

**Descrição:**
Arqueiro magro da floresta subterrânea. Corpo mais delgado que os goblins, talvez elfo distante corrompido ou hobgoblin menor. Arco feito de madeira morta e espinhos entrelaçados — não é um arco bonito, é funcional e sinistro. Olhos amarelos que brilham na escuridão. Pele esverdeada-escura que camufla bem no bioma. Postura de arqueiro: nunca de frente, sempre de perfil ou semi-coberto.

**Cores:**
- Pele: verde-escuro `#304A20`
- Arco: madeira negra `#2A1810` com espinhos `#4A3020`
- Olhos: amarelo `#C0A010`
- Flecha (projétil): espinho marrom-escuro `#4A2810`

---

### Espreitador Orc de Nyx (`enemy_orc_nyx_stalker`) — MEDIUM 32×48 px

**Descrição:**
Orc que fez pacto ou foi corrompido pela influência de Nyx. Pele cinza-esverdeada diferente dos orcs normais — mais fria, mais apagada. Capuz/manto escuro de couro e pele. Marcas azul-escuras no rosto e pescoço — não tatuagens intencionais, são marcas da influência de Nyx. Olhos escuros com íris que brilha levemente azul. Surge de sombras — idle quase não visível, revela-se ao atacar.

**Cores:**
- Pele: cinza-esverdeado `#506840`
- Marcas de Nyx: azul-escuro `#203060`
- Olhos: brilho azul `#4060A0`
- Manto: couro quase-preto `#1A1810`

---

### Urso-Coruja Raiz-Oca (`enemy_root_owlbear`) — LARGE 48×56 px

**Descrição:**
Chimera de bioma — metade urso, metade coruja, com raízes negras emergindo do dorso como coluna vertebral externa. Corpo massivo de urso com pelagem densa e pardacenta. Cabeça de coruja completa: bico curvo, olhos grandes amarelo-ouro, tufos de penas nas "orelhas". As raízes negras saem do dorso em arcos irregulares, algumas com folhas mortas presas. Quando ruge, as raízes se erguem.

**Cores:**
- Pelo: marrom-pardo `#7A5030` / sombras: `#4A3010`
- Penas da cabeça: cinza-pardo `#808070` / bico: cinza-amarelado `#C0A840`
- Olhos: amarelo-ouro `#D0A010`
- Raízes no dorso: preto-marrom `#201810`

---

### Lagarto Basilisco de Musgo (`enemy_basilisk_lizard`) — MEDIUM 32×48 px

**Descrição:**
Lagarto baixo e largo, movimento lento e calculado. Crista dorsal de escamas mais largas que o resto. Patas curtas e largas, adaptadas a terreno irregular. O elemento visual principal são os **olhos**: grandes, dourado-opacos, sem pupila visível — parecem pedras polidas mais do que olhos. Quando carrega o "olhar", os olhos pulsam levemente. Coloração mimetizada com o musgo.

**Cores:**
- Escamas: verde-musgo `#4A6A30` / crista: cinza-pedra `#6A7060`
- Barriga: verde-pálido `#7A9060`
- Olhos: dourado-opaco `#C0A030` (sem pupila — efeito de pedra)

---

### Pantera Distorcida (`enemy_panther_distorted`) — MEDIUM 32×48 px

**Descrição:**
Felino de tamanho médio com algo fundamentalmente errado com o espaço ao seu redor. Corpo de pantera negra real — não é o corpo que está distorcido, é a **sombra** e o **contorno**. A sombra no chão está deslocada ou duplicada. O contorno do sprite tem bordas ligeiramente duplicadas em azul-roxo, como se duas versões do mesmo felino estivessem sobrepostas com 2-3 pixels de offset. Cauda que parece dupla — ou talvez seja sombra, ou talvez não.

**Cores:**
- Corpo: preto `#181810` / pelo com luz: `#2A2820`
- Contorno duplo/distorção: roxo `#4A2060` e azul `#203060`
- Sombra no chão: azulada `#283050` (deslocada do corpo)
- Olhos: roxo-brilhante `#A050D0`

---

## INIMIGOS — CAVERNA DE GELO (Níveis 26-40)

---

### Escavador Duergar do Gelo (`enemy_duergar_frostdelver`) — MEDIUM 32×48 px

**Descrição:**
Anão subterrâneo — parente distante dos anões da superfície, mas sem a cultura deles. Corpo robusto e baixo, pele cinza-azulada fria. Barba longa com gelo preso nela — mechas congeladas formando picos. Usa picareta pesada como arma e ferramenta. Armadura de placas laminadas de pedra e gelo fundidos.

**Cores:**
- Pele: cinza-azulado `#7080A0` / luz: `#90A0C0`
- Barba: branco-gelo `#D0E0F0` com pontas azuladas `#80A0D0`
- Armadura: pedra escura `#404858` com gelo `#8AAAC0`
- Picareta: metal frio `#607080`

---

### Osso de Vidro (`enemy_glassbone`) — MEDIUM 32×48 px

**Descrição:**
Esqueleto feito de cristal de gelo — não osso. Translúcido e brilhante. Pontas quebradas em vários lugares, criando extremidades pontiagudas. Levemente brilhante com luz azul-fria interna. Quando se move, estala como vidro quebrando. Design mais anguloso que um esqueleto comum.

**Cores:**
- Corpo: translúcido-azulado `#80B0E0` (simule com pontilhado claro/escuro)
- Brilho interno: branco-azul `#C0E0F8`
- Pontas quebradas: azul-escuro nas bordas `#3060A0`
- Crack/fissuras: preto `#101828`

---

### Saltador Cristalino (`enemy_crystal_leaper`) — SMALL/MEDIUM 32×40 px

**Descrição:**
Criatura angular, quase geométrica. Corpo como um cristal que ganhou pernas — 4 pernas longas e pontiagudas saindo de um núcleo hexagonal. Quando está parado, parece uma formação cristalina do ambiente (camuflagem). Quando se move, revela a postura de ataque de salto.

**Cores:**
- Corpo/cristal: azul-médio `#6090C0` translúcido
- Bordas: azul-escuro `#2A5080`
- Núcleo: branco-azulado brilhante `#C0E0FF`
- Pernas: azul-transparente apontado

---

### Lamento Frio (`enemy_frost_wailer`) — MEDIUM 32×48 px

**Descrição:**
Morto-vivo enregelado. Figura humanoide com pele apergaminhada e cinza-azulada. Boca permanentemente aberta num grito silencioso (ou nem tão silencioso). Vapor frio constante saindo da boca e do corpo. Olhos brancos completamente. Roupa congelada colada ao corpo como segunda pele de gelo. Braços estendidos para frente.

**Cores:**
- Pele: cinza-azulado `#7080A0` / partes mais mortas: `#5A6890`
- Boca aberta: escuro interno `#2A3050`
- Vapor: branco semi-transparente (partícula animada)
- Olhos: branco `#E0E8F0`

---

### Roedor de Geada (`enemy_frost_gnawer`) — SMALL 24×32 px

**Descrição:**
Roedor grande — maior que um rato comum, menor que um castor. Pelo completamente congelado: mechas individuais de pelo são visíveis como agulhas de gelo, duras e pontiagudas. Dentes azulados — tingidos pelo frio permanente. Orelhas pequenas e enrijecidas. Mais rápido do que parece — o frio não o desacelera, faz parte dele. Deixa rastro de gelo fino no chão.

**Cores:**
- Pelo congelado: branco-azulado `#D0E0F0` com mechas-agulha mais escuras `#A0B8D0`
- Dentes: azul-frio `#70A0C0`
- Olhos: preto-brilhante `#101820`
- Rastro de gelo: branco translúcido no chão

---

### Quebra-Escudo Duergar (`enemy_duergar_shieldbreaker`) — LARGE 48×56 px

**Descrição:**
Variante especializada do Duergar, maior e mais brutal. Escudo rachado — o seu próprio — carregado no braço esquerdo, não como defesa mas como arma (golpeia com a borda quebrada). Martelo curto mas pesado na direita. Silhueta mais larga que o Escavador, postura defensiva mesmo avançando. Barba menor, mais prática. Armadura com amassados — resultado de batalhas contra outros escudos.

**Cores:**
- Pele: cinza-azulado frio `#6070A0`
- Escudo rachado: metal escuro `#404858` com rachaduras `#202830`
- Martelo: ferro escuro `#505860`
- Armadura: pedra-gelo fria `#5060A0`

---

### Sentinela Enregelado (`enemy_icebound_sentinel`) — LARGE 48×56 px

**Descrição:**
Constructo antigo — não é Bromeciano, é anterior. Formato humanoide mas claramente não orgânico: partes do corpo são placas de metal antigo e gelo fundidos. O núcleo no peito é visível — cristal azul-claro que pulsa. Movimentos rígidos e geométricos, sem naturalidade orgânica. Gelo cresce sobre o metal ao longo do tempo — algumas partes têm mais gelo acumulado que outras.

**Cores:**
- Metal antigo: cinza-azul `#5068A0` com oxidação `#384868`
- Gelo sobre metal: azul-translúcido `#80B0E0`
- Núcleo no peito: azul-claro pulsante `#A0D0F0`
- Rachaduras no gelo: branco `#E0F0F8`

---

### Acólito do Frio (`enemy_cold_cult_acolyte`) — MEDIUM 32×48 px

**Descrição:**
Humanoide que escolheu seguir algo no gelo. Encapuzado — capuz largo que esconde o rosto na maioria das poses (apenas o brilho dos olhos visível no escuro do capuz). Símbolos de frio gravados ou bordados nas vestes — padrões geométricos angulosos em azul sobre tecido cinza-escuro. Mãos azuladas visíveis — expostas ao frio por escolha, não por acidente. Quando conjura, padrões de gelo formam ao redor das mãos.

**Cores:**
- Capuz e veste: cinza-escuro-frio `#404858`
- Símbolos bordados: azul `#6090C0`
- Mãos expostas: azulado `#708090`
- Brilho dos olhos no capuz: azul-claro `#A0C8E0`

---

### Horror-Gancho de Gelo (`enemy_hook_horror_ice`) — LARGE 48×56 px

**Descrição:**
Criatura que parece construída para o terror auditivo — os ganchos de gelo no lugar de mãos fazem barulho ao roçar nas paredes de pedra. Corpo avestruzado: pernas compridas e articuladas para trás (como ave), torso compacto, cabeça pequena e quase sem pescoço. Dois braços terminando em ganchos curvos de gelo puro — translúcidos, afiados, tão frios que queimam. Olhos na lateral da cabeça, visão ampla.

**Cores:**
- Corpo: cinza-pedra fria `#5A6878`
- Ganchos de gelo: azul-translúcido `#80B0E0` com bordas brancas afiadas `#D0E8F8`
- Olhos: amarelo-opaco `#B0A030`
- Pernas articuladas: cinza mais escuro `#404858`

---

### Larva Devora-Mentes (`enemy_mind_eater_larva`) — SMALL 24×32 px

**Descrição:**
Criatura que parece errada em proporção deliberada. Corpo mole e pálido, quase larval — sem estrutura óssea aparente. Cabeça **desproporcional**: ocupa 40-50% do sprite total, muito maior que o corpo deveria comportar. Tentáculos curtos ao redor da boca — 4-6 tentáculos finos. Olhos quase invisíveis — apenas leve brilho em fendas oculares. Frágil visualmente, mas a cabeça enorme comunica perigo psíquico.

**Cores:**
- Corpo: branco-pálido-rosado `#E0C8C0`
- Cabeça (maior): rosado-pálido mais escuro `#C0A098`
- Tentáculos: rosado-cinza `#B09088`
- Brilho dos olhos: roxo muito suave `#907080`

---

## INIMIGOS — CAVERNA DE FOGO (Níveis 41-55)

---

### Carrapato de Brasa (`enemy_ember_tick`) — TINY 16×16 px

**Descrição:**
Como o Ácaro da Fenda mas de fogo. Abdômen incandescente — brilha de dentro, vermelho-laranja. Patas negras com pontinhas acinzentadas. O brilho do abdômen é o que identifica imediatamente este inimigo no escuro quente da caverna de fogo.

**Cores:**
- Patas/corpo: preto-cinza `#3A3028`
- Abdômen: laranja brilhante `#E06010` com halo amarelo `#E8A020`

---

### Berserker Orc de Kaand (`enemy_orc_kaand_berserker`) — LARGE 48×56 px

**Descrição:**
Orc de guerra enorme, marcado pelo fogo. Pele com marcas vermelhas de guerra pintadas — não tatuagem, tinta de batalha. Usa arma pesada (machado de lâmina larga ou maça). Postura agressiva permanente — curvado para frente, pronto para investir. Chifres decorativos queimados presos ao capacete. Cicatrizes de queimadura visíveis.

**Cores:**
- Pele: verde-médio `#5A7A48` com marcas vermelhas `#C02010`
- Cicatrizes: rosa-escuro `#A06050`
- Arma: metal enegrecido `#303028` com borda afiada cinza `#808070`
- Chifres decorativos: marrom-queimado `#4A2810`

---

### Guardião da Fornalha (`enemy_furnace_warden`) — HUGE 80×80 px

**Descrição:**
Constructo feito de metal de forja e rocha fundida. Forma humanoide mas massiva — parece uma fornalha que ganhou braços. O peito tem uma **porta de forno** real — metal enferrujado com luz laranja-brilhante saindo pelas frestas quando respira/ataca. Braços são enormes e terminam em punhos de rocha negra. Não tem cabeça visível — apenas uma grade na parte superior do "tronco" com brasa dentro.

**Cores:**
- Corpo/metal: cinza-escuro `#404038` / enferrujado `#6A4020`
- Frestas de calor: laranja-vivo `#E06010` brilhante
- Rocha fundida: preto com veios laranja `#C04010`
- Dentro da grade: vermelho-brasa `#C02000`

---

### Rastejante de Cinza (`enemy_ash_crawler`) — MEDIUM 32×48 px

**Descrição:**
Quadrúpede coberto de cinza — não como cor, como substância acumulada. O animal original (provavelmente um lagarto grande ou réptil) está completamente encoberto por camadas de cinza volcânica endurecida. A silhueta é irregular, empoeirada, quase indistinguível do chão de cinza quando parado. Deixa rastro escuro no chão. Quando abre a boca para atacar, a cinza racha e revela o vermelho-vivo interno.

**Cores:**
- Exterior (cinza): cinza-quente `#706860` com variações `#807870`
- Interior/boca: vermelho-brasa `#C03010`
- Rastro no chão: cinza-escuro `#504840`
- Olhos: laranja mínimo visível `#C06010`

---

### Chamador de Cinzas de Kaand (`enemy_orc_kaand_ashcaller`) — MEDIUM 32×48 px

**Descrição:**
Orc xamânico — menos musculoso que o Berserker, mais alto e anguloso. Ossos queimados usados como adornos: colares, braçais, e presos no cabelo. Cinzas flutuam ao redor das mãos enquanto conjura — não é partícula aleatória, é controlada, obedece aos gestos. Manto de couro queimado e penas chamuscadas. Cajado com crânio na ponta (orc ou animal). Expressão de êxtase ritual.

**Cores:**
- Pele: verde-cobre escurecido `#4A6030`
- Ossos decorativos: cinza-carvão `#505048`
- Cinzas nas mãos: cinza-claro flutuante `#C0B0A0`
- Manto: couro queimado `#3A2010` com penas chamuscadas
- Cajado: madeira carbonizada `#2A1A08`

---

### Baluarte de Lava (`enemy_lava_bulwark`) — LARGE 48×56 px

**Descrição:**
Elemental de terra e lava — não tem forma humana definida, mas tem orientação (frente/costas determinadas pela posição da maior concentração de lava no corpo). Corpo largo e irregular de rocha negra basáltica. Fissuras largas no corpo por onde a lava interna é visível — laranja-vermelho brilhante intenso, principal fonte de iluminação deste inimigo. Não tem rosto no sentido humano, mas tem dois pontos de brilho mais intenso que funcionam como "olhos". Movimento lento mas cada passo faz o chão rachar ao redor.

**Cores:**
- Rocha exterior: basalto negro `#201810`
- Fissuras de lava: laranja-vermelho `#D04010` / núcleo: amarelo `#E09020`
- Respingo no chão: laranja que escurece para vermelho e depois preto

---

### Cuspidor de Cinza (`enemy_cinder_spitter`) — MEDIUM 32×48 px

**Descrição:**
Besta de tamanho médio com mecanismo de ataque visual óbvio — a garganta inflada é o telegraf. Quadrúpede, placas de cinza endurecida cobrindo o dorso (armadura natural). A garganta infla visivelmente quando carrega o cuspe — de normal para duas vezes o tamanho, brilhando por dentro. A boca então abre e a cinza é disparada. Corpo compacto, pernas curtas, cauda curta.

**Cores:**
- Corpo/placas: cinza-escuro `#5A5048`
- Garganta (normal): cinza-mais-claro `#787068`
- Garganta (carregando): laranja brilhante `#D05010` expandida
- Olhos: amarelo-brasa `#D09010`

---

### Cultista Chamuscado (`enemy_scorched_cultist`) — MEDIUM 32×48 px

**Descrição:**
Humanoide que sobreviveu a algo que não deveria sobreviver, ou que está em processo de transformação por ritual de fogo. Pele queimada e apergaminhada — tom que vai de vermelho-escuro nas extremidades para marrom-acinzentado no centro do corpo. Manto rasgado, queimado nas bordas, com símbolo de fogo ainda visível mesmo com o tecido destruído. Olhos que brilham levemente em brasa — não é magia limpa, é possessão ou transformação incompleta.

**Cores:**
- Pele queimada: vermelho-escuro `#8A3020` / centro: `#6A4030`
- Manto rasgado: preto-queimado `#201810` com bordas chamuscadas `#3A2010`
- Símbolo de fogo (no manto): laranja-apagado `#A04810`
- Olhos: brasa `#C04010`

---

### Basilisco de Brasa (`enemy_fire_basilisk`) — LARGE 48×56 px

**Descrição:**
Versão de fogo do basilisco — mesma estrutura (lagarto baixo e largo, crista dorsal, olhos grandes), mas com adaptações do bioma. A crista dorsal é incandescente — plaquetas que brilham de dentro, laranja-vermelho. Escamas negras em vez de verdes, com fendas de calor entre elas. Olhos de brasa em vez de dourado-opaco — dois pontos de brilho intenso. O "olhar" carregado faz as fendas do corpo brilharem mais.

**Cores:**
- Escamas: negro-basalto `#201810`
- Fendas entre escamas: laranja `#C04010`
- Crista: laranja-vermelho incandescente `#E05010`
- Olhos: brasa brilhante `#E06010`

---

### Tubarão de Pedra (`enemy_stone_bulette`) — HUGE 80×80 px

**Descrição:**
Predador enorme que escava através da rocha como um tubarão nada na água. Silhueta de tubarão terrestre: corpo fusiforme largo, "nadadeiras" dorsais são placas de pedra que cortam o chão quando emerge. Mandíbula larga demais para o corpo — ocupa 30% do sprite quando aberta. Cabeça em cunha afiada para escavação. Olhos minúsculos irrelevantes — caça por vibração, não visão. Sem pescoço — a cabeça é contínua com o corpo.

**Cores:**
- Placas dorsais: cinza-basalto `#6A6858` / bordas: cinza-mais-claro `#8A8870`
- Barriga: cinza-pálido `#A0A090`
- Mandíbula: cinza-amarelado `#8A8460` / interior: vermelho-escuro `#6A2010`
- Olhos: amarelo-mínimo `#B09020` (quase invisíveis)

---

## INIMIGOS — RUÍNAS ANTIGAS (Níveis 56-70)

---

### Guarda de Corda (`enemy_clockwork_guard`) — MEDIUM 32×48 px

**Descrição:**
Constructo humanoide de engrenagens. Corpo de metal antigo — não moderno, não steampunk chamativo, mas tecnologia arcana esquecida. Engrenagens visíveis nos ombros e joelhos. Passos mecânicos e lentos. Olhos são dois cristais azuis encaixados no elmo. Segura uma alabarda de metal envelhecido.

**Cores:**
- Corpo: bronze-escuro `#8A6030` / cobre oxidado `#6A7040`
- Engrenagens expostas: cobre `#9A6820`
- Cristais (olhos): azul-arcano `#4080C0`
- Alabarda: metal escuro `#505048`

---

### Gnomo de Gema Enlouquecido (`enemy_gnome_gem_madcap`) — SMALL 24×32 px

**Descrição:**
Gnomo pequeno que ficou obcecado com as gemas das ruínas. Um ou ambos os olhos estão substituídos por gemas coloridas (lupa ampliada ou gema encaixada diretamente na órbita). Cabelo em pé, bagunçado, com partículas de poeira de gema. Carrega múltiplas ferramentas apontadas para frente. Expressão de insanidade curiosa — não raiva, mas perigoso.

**Cores:**
- Pele: marrom-claro `#C09060`
- Cabelo: branco-caótico com partículas coloridas
- Gema no olho: varia — vermelho `#C02020`, azul `#2050C0`, verde `#20A040`
- Roupa: avental de couro com bolsos cheios `#6A4020`

---

### Cavaleiro Selado (`enemy_sealed_knight`) — LARGE 48×56 px

**Descrição:**
Armadura completa antiga — ninguém sabe o que há dentro. O elmo é completamente fechado, sem viseira, sem abertura. No peito, um **selo brilhante** — ícone gravado em metal que pulsa com luz roxo-azulada. Parece um cavaleiro mas se move de forma ligeiramente errada — um pouco lento demais, um pouco firme demais. Espada de estilo arcaico, diferente das armas atuais.

**Cores:**
- Armadura: metal antigo azul-aço `#5060A0` / sombras: `#303868`
- Selo no peito: roxo-azulado brilhante `#7050E0`
- Espada: prata antiga `#A0A8B0`
- Detalhe: filamentos dourados antigos nas juntas `#C0A030`

---

### Lasca Rúnica (`enemy_rune_shard`) — TINY 16×16 px

**Descrição:**
Fragmento de pedra/cristal que flutua e orbita. Forma irregular — não é um cubo nem uma esfera, é literalmente um pedaço de algo maior que quebrou. Runa gravada em uma das faces, brilhando com luz arcana azul-roxo. Múltiplas lascas geralmente aparecem juntas orbitando um ponto. Idle: giro lento. Ataque: disparo em linha reta a alta velocidade.

**Cores:**
- Pedra: cinza-médio `#7A7888`
- Runa brilhante: azul-roxo `#6050C0`
- Brilho ao redor: halo roxo suave

---

### Gnomorin Runa-Torta (`enemy_gnomorin_rune_tinker`) — SMALL 24×32 px

**Descrição:**
Gnomorin é uma subespécie/variante de gnomo das ruínas — mais antigo, mais estranho. Corpo pequeno mas cabeça maior que um gnomo comum. Usa uma mochila inteira de peças mecânicas e arcanas que tilintam ao mover. Luvas grandes e rígidas — parecem protéticas, parte ferramenta, parte armadura de mão. Runas desenhadas no corpo (no próprio gnomorin) estão **tortas** — não são os padrões limpos das ruínas, são adaptações improvisadas. Expressão de concentração permanente.

**Cores:**
- Pele: cinza-rosado `#B09080`
- Mochila de peças: metal velho misturado — cobre `#9A6820`, prata opaca `#8A8890`, bronze `#8A7030`
- Luvas: metal escuro `#5A5A60`
- Runas tortas no corpo: azul-desbotado `#5060A0`

---

### Adepto do Espelho (`enemy_mirror_adept`) — MEDIUM 32×48 px

**Descrição:**
Humanoide coberto de fragmentos de espelho em vez de armadura. Os fragmentos refletem o ambiente ao redor — a identidade visual mais difícil de ler intencionalmente. Manto claro de um lado, escuro do outro (como se dois estados fossem o mesmo personagem). Pose elegante, dedos longos. Quando conjura, os fragmentos vibram e projetam imagens falsas ao redor.

**Cores:**
- Fragmentos (espelhos): prata `#C0C8D0` com reflexo do fundo (cor varia por ângulo)
- Manto claro: branco-sujo `#E0D8C8`
- Manto escuro (outro lado): cinza-escuro `#404048`
- Olhos: sem iris — espelho também `#D0D8E0`

---

### Golem de Enigma (`enemy_puzzle_golem`) — HUGE 80×80 px

**Descrição:**
Golem construído com blocos que se reorganizam. O corpo é composto de placas grandes de pedra/metal que se movem entre si — às vezes o braço muda de posição, às vezes o peito se abre. O **núcleo** fica exposto em ciclos de reorganização — cristal arcano no centro do torso, azul-roxo brilhante. Quando o núcleo está exposto, é o ponto fraco visualmente destacado. Sem rosto fixo — a "cabeça" é uma placa com runas.

**Cores:**
- Placas: pedra-antiga `#808070` / juntas: metal escuro `#404048`
- Núcleo exposto: azul-roxo brilhante `#7060D0`
- Runas nas placas: azul suave `#6070B0`

---

### Sombra Sem-Juramento (`enemy_oathless_shade`) — MEDIUM 32×48 px

**Descrição:**
Sombra de quem jurou algo e quebrou o juramento — uma forma de morto-vivo específica das ruínas de Elyndor. Humanoide com restos de armadura/vestes de ritual visíveis através da silhueta de sombra — como se as roupas ainda existissem mas o corpo fosse sombra. Semitransparente: o fundo é levemente visível através do sprite. Mãos com marcas brilhantes do juramento quebrado — runas visíveis que pulsam como ferida.

**Cores:**
- Corpo de sombra: cinza-escuro semitransparente `#303040` (simule com pontilhado)
- Restos de armadura visíveis: azul-antigo `#4050A0` em silhueta
- Runas de juramento quebrado nas mãos: roxo-branco `#A080E0` pulsante
- Olhos: branco-frio `#E0E0F0`

---

### Observador Menor da Ruína (`enemy_beholder_kin_lesser`) — MEDIUM 32×48 px (flutuante)

**Descrição:**
Corpo ocular flutuante — um grande olho central como núcleo do sprite. O olho central está **rachado** — não funcionando plenamente, o que é visualmente inquietante (uma racha vertical no olho central). Ao redor do corpo, 4-6 olhos menores em talos — estes funcionam e olham em direções independentes. Corpo irregular, não simétrico. Não tem boca, membros, ou nada humanóide.

**Cores:**
- Corpo ocular: roxo-acinzentado `#6050A0` / superfície irregular `#504880`
- Olho central rachado: íris azul-escuro `#3040A0` / pupila preta / racha: preto `#101018`
- Olhos menores em talos: dourado `#C0A020` / pupila preta
- Talos: cinza-escuro `#505068`

---

### Arsenal-Mordente (`enemy_mimic_armory`) — LARGE 48×56 px

**Descrição:**
Fase 1 (disfarce): armário ou rack de armas de parede — madeira escura, ganchos com espadas e lanças expostas, aparência completamente mundana numa sala de ruínas. Idêntico a um prop decorativo. Fase 2 (revelado): a estrutura se abre revelando dentes de metal na "boca" central (a abertura do armário), correntes que se estendem como membros, e as armas do rack passam a ser usadas como garras/tentáculos. Dois olhos pequenos amarelos aparecem no topo.

**Cores (disfarce):**
- Madeira: marrom-escuro `#3A2010`
- Armas expostas: cinza-metal `#707878`

**Cores (revelado):**
- Dentes de metal: cinza-brilhante `#A0A8B0` com bordas escuras
- Correntes: ferro escuro `#404848`
- Olhos: amarelo-âmbar `#C09010`
- Interior: vermelho-escuro `#601020`

---

### Geleia-Memória (`enemy_brain_jelly`) — LARGE 48×48 px

**Descrição:**
Lodo cerebral translúcido — visualmente baseado numa gelatina cerebral. Forma oval irregular com sulcos que lembram um cérebro. Dentro do corpo translúcido, **imagens e fragmentos** são visíveis — silhuetas de pessoas, objetos, cenas passadas (memórias absorvidas). O interior é mais denso e mais escuro que a periferia. Pulsa ritmicamente como se respirasse.

**Cores:**
- Exterior: roxo-rosado translúcido `#C080B0` (simule com pontilhado)
- Interior mais denso: roxo-escuro `#60408A`
- Fragmentos internos: sombras de formas `#402860` em silhueta
- Sulcos/superfície cerebral: bordas mais escuras `#A06090`

---

## INIMIGOS — ABISMO SOMBRIO (Níveis 71-85)

---

### Lâmina Sombria Drow (`enemy_drow_shadowblade`) — MEDIUM 32×48 px

**Descrição:**
Elfo das profundezas. Pele negra-azulada (não humano escuro — literalmente negra com reflexos azuis). Cabelo branco ou prata, geralmente preso ou curto na guerra. Olhos de íris vermelha ou roxa. Veste armadura leve de couro escuro com capuz. Porta lâmina curva — não uma espada, mas um sabre de curva elegante. Postura baixa, atlética. Silhueta esbelta mas definitivamente ameaçadora.

**Cores:**
- Pele: preto-azulado `#1A1A30` / reflexos: `#2A2A50`
- Cabelo: branco-prata `#D0D0E0`
- Olhos: vermelho `#C02040` ou roxo `#802090`
- Armadura: couro preto `#1A1818`
- Lâmina: prata-escura `#708090`

---

### Drow Conjurador Lunar (`enemy_drow_arcane_adept`) — MEDIUM 32×48 px

**Descrição:**
Drow caster — menos atlético que o guerreiro, mais elegante. Manto azul-prateado-escuro que flui. Segura um foco mágico — cristal ou orbe que brilha com luz lunar fria. Mãos rodeadas de energia azul-prata quando conjura. Postura ereta, quase arrogante.

**Cores:**
- Manto: azul-muito-escuro `#101828` / detalhes: prata `#A0A8C0`
- Energia mágica: azul-lunar `#6090D0` com brilho `#B0D0F0`
- Foco/cristal: branco-azulado pulsante `#C0D8F0`

---

### Conjurador do Vazio (`enemy_void_caster`) — MEDIUM 32×48 px

**Descrição:**
Figura humanoide que está se desfazendo. O rosto é quase apagado — feições borradas, olhos que são apenas dois pontos de luz escura. O manto/roupa flutua e tem partes que se dissolvem em pixels escuros. Mãos rodeadas de energia de vazio — não é magia colorida, é uma ausência de cor, um buraco de luz.

**Cores:**
- Corpo: roxo-muito-escuro `#201028` em gradiente para quase-transparente nas bordas
- Energia de vazio: preto absoluto `#000000` com borda roxa `#601080`
- Olhos: pontos de luz escura — o oposto de brilho, uma sombra mais escura que o fundo

---

### Devorador de Juramento (`enemy_oath_eater`) — LARGE 48×56 px

**Descrição:**
Aberração grande e perturbadora. O elemento mais assustador: tem uma **boca no peito ou na barriga** — não no rosto. O "rosto" acima é humanoide mas inerte, como máscara. A boca ventral é larga, com dentes, e se abre para consumir/atacar. Marcas de votos quebrados no corpo — runas rasgadas, correntes fantasmagóricas presas a ele.

**Cores:**
- Pele/corpo: cinza-azulado escuro `#404858`
- Boca ventral: interior vermelho-escuro `#6A1020` / dentes: marfim `#D0C8A8`
- Runas rasgadas: roxo-escuro pulsante `#501870`
- Correntes: cinza-pálido fantasmagórico `#9090A0`

---

### Batedor de Teia Drow (`enemy_drow_web_scout`) — SMALL 24×32 px

**Descrição:**
Drow menor e mais ágil que o guerreiro — scout de exploração. Postura permanentemente baixa, quase a rastejar. Usa arco curto e fios escuros — fios de teia para armar armadilhas ou imobilizar. Os fios são de cor negra-azulada e visíveis saindo de um dispositivo no pulso ou de uma bolsa. Capuz parcial que cobre apenas a parte de cima da cabeça. Movimentos rápidos e laterais.

**Cores:**
- Pele: preto-azulado `#1A1A2A`
- Roupa: cinza-escuro `#282838`
- Fios de teia: azul-escuro muito suave `#2A2A40`
- Arco: madeira escura `#2A1810`
- Olhos: vermelho `#C02030`

---

### Fagulha do Abismo (`enemy_abyss_wisp`) — TINY 16×16 px

**Descrição:**
Chama escura flutuante — o oposto visual de um wisp de luz. Centro azul-preto, bordas que se dissolvem em roxo-escuro. Sem forma definida — é uma chama que existe contra a lógica. Flutua em movimentos irregulares, nunca em linha reta. Multiplica ao ser atacado em vez de diminuir (telegraf visual: ao ser atingido, brilha mais e divide).

**Cores:**
- Centro: azul-preto `#101020`
- Bordas: roxo-escuro `#3A1860`
- Halo exterior: roxo suavíssimo `#4A2070`
- Ao ser atingido: flash branco-roxo `#C080E0`

---

### Cão Sem-Lua (`enemy_moonless_hound`) — MEDIUM 32×40 px

**Descrição:**
Cão magro de aparência doentia — não ferido, não moribundo, mas definitivamente **errado**. Pele escura com pouco pelo — quase calvo em áreas, pelo curto e aderente onde existe. Dentes brancos demasiado grandes para a mandíbula, visíveis mesmo com a boca fechada. Olhos sem reflexo — não brilham, são completamente opacos, como se a luz não os atingisse. Postura baixa e tensa, sempre.

**Cores:**
- Pele/pelo: preto-azulado `#181820`
- Áreas sem pelo: cinza-escuro `#303038`
- Dentes: branco-sujo `#E0D8C8` protuberantes
- Olhos: preto-opaco `#080810` (sem reflexo)

---

### Cultista da Lanterna Negra (`enemy_black_lantern_cultist`) — MEDIUM 32×48 px

**Descrição:**
Cultista de aparência humanoide que carrega uma lanterna que emite **luz negra** — não ilumina, apaga. O raio de luz da lanterna faz a área ao redor ficar mais escura, não mais clara. Manto completamente preto, fechado. Rosto encoberto por capuz, mas a lanterna ilumina de baixo, criando sombras invertidas no rosto. O símbolo na lanterna é uma lua sem luz.

**Cores:**
- Manto: preto absoluto `#101010`
- Lanterna: metal escuro `#303038` com "luz" negra ao redor (área escurecida no sprite)
- Símbolo da lanterna: cinza-escuro `#405060`
- Sombras invertidas no rosto: azul-escuro `#202840`

---

### Devora-Mentes Abissal (`enemy_mind_eater_adult`) — LARGE 48×56 px

**Descrição:**
Versão adulta da larva — completamente diferente em silhueta mas mesma linhagem. Cabeça ainda grande mas agora proporcional ao corpo adulto. Corpo alongado, quatro tentáculos curtos ao redor da boca (não braços — a boca é o centro). Olhos leitosos, com membrana. Manto orgânico crescido do próprio corpo — não tecido, é parte do ser. Postura ereta mas cabeça levemente inclinada para frente.

**Cores:**
- Cabeça: cinza-rosado `#9080A0`
- Olhos: leitoso-esbranquiçado `#D0C8D0` (sem pupila visível)
- Tentáculos da boca: rosado-cinza `#A09098`
- Manto orgânico: roxo-escuro `#3A2850`

---

### Tirano Ocular da Lua Negra (`enemy_eye_tyrant_blackmoon`) — HUGE 80×80 px

**Descrição:**
Grande esfera ocular flutuante — maior e mais ameaçador que o Observador Menor. O olho central é de Pedra Negra — não orgânico, mineral e opressivo. Aura lunar escura ao redor do corpo. 8-10 olhos menores em talos longos orbitando o corpo central, cada um independente, cada um perigoso. Os talos são flexíveis e se movem de forma não-sincronizada — assustador e hipnótico. O olho central não pisca. Nunca.

**Cores:**
- Corpo central: roxo-escuro `#3A2060`
- Olho central de Pedra Negra: preto `#0A0810` com íris roxo-escuro `#501880`
- Talos: roxo-médio `#5A3880`
- Olhos nos talos: dourado-corrompido `#A08010` com pupila preta
- Aura lunar: azul-escuro suave `#202840`

---

## INIMIGOS — NÚCLEO CORROMPIDO (Níveis 86-99)

---

### Massa Corrompida (`enemy_corrupt_hulk`) — HUGE 80×80 px

**Descrição:**
Gigante biológico com mineralização por Pedra Negra. O que era um ser vivo grande (humanoide ou animal) teve seu corpo invadido pela Pedra Negra que cresceu através da carne. O resultado é assimétrico: um braço é de carne e músculo, o outro é parcialmente ou totalmente de Pedra Negra — mais largo, mais longo, claramente mais perigoso. O peito tem cristais de Pedra Negra emergindo através da pele. A cabeça está parcialmente encoberta por Pedra Negra — metade rosto, metade mineral.

**Cores:**
- Carne: cinza-acinzentado `#6A5848`
- Pedra Negra: preto-roxo `#1A0C28` com brilho roxo nas arestas `#501870`
- Cristais emergindo: roxo-escuro `#3A1050`
- Braço de Pedra Negra: mais escuro e mais angular que o lado de carne

---

### Oráculo Ninrorin Quebrado (`enemy_ninrorin_broken_oracle`) — MEDIUM 32×48 px

**Descrição:**
Ninrorin (elfa cinzenta) antiga que sobreviveu além do tempo de seu corpo. Figura encurvada e fraca fisicamente, mas com presença visual que comunica algo imenso. Olhos completamente cobertos por tecido ritual — ela não precisa ver o que está diante dela para saber onde está. Ao redor da figura, fragmentos proféticos flutuam — pedaços de texto, imagens, visões materializadas como partículas azul-prateadas. Roupas rituais antigas, quase intactas mas claramente de outra era.

**Cores:**
- Pele: cinza muito pálido `#C0C8D0` quase translúcido
- Vestes rituais: azul-muito-escuro `#101828` com bordado prata desbotado
- Faixa nos olhos: prata `#C0C8D8`
- Fragmentos proféticos (partículas): azul-prata `#A0B8D0`

---

### Pseudodragão Corrompido (`enemy_corrupted_pseudodragon`) — SMALL 24×32 px

**Descrição:**
Pseudodragão — dragão em miniatura, normalmente elegante e delicado — completamente corrompido. Asas rasgadas em múltiplos pontos, com bordas enegrecidas. Escamas negras e roxas em vez das cores originais. Cauda com ferrão, mas o ferrão está negro e aumentado. Olhos que eram dourados agora são roxo-sólido sem pupila. Pequeno mas agressivo — movimentos irregulares e espásticos.

**Cores:**
- Escamas: negro `#151010` / roxo: `#3A1050`
- Asas rasgadas: preto com bordas roxas `#401060`
- Olhos: roxo-sólido `#7020A0`
- Ferrão: preto brilhante `#201018`

---

### Wyvern de Pedra Negra (`enemy_blackstone_wyvern`) — HUGE 80×80 px

**Descrição:**
Wyvern (dragão de duas patas + asas) coberto de placas de Pedra Negra. As placas negras cresceram sobre o corpo como armadura, mas de dentro para fora — são parte do wyvern agora. Uma das asas tem placas que interferem com o voo (asa ferida visualmente — dobras assimétricas). Fendas entre as placas brilham em roxo-escuro. Cabeça com placas de Pedra Negra formando "chifres" irregulares.

**Cores:**
- Placas de Pedra Negra: preto-mineral `#0E0C14`
- Fendas entre placas: roxo-escuro `#501878`
- Corpo sob as placas (visível em áreas): cinza-escuro `#404050`
- Asa com placa: assimétrica, algumas placas caídas/quebradas

---

### Paragon da Pedra Negra (`enemy_blackstone_cult_paragon`) — LARGE 48×56 px

**Descrição:**
Cultista avançado que completou a maior parte de sua transformação por Pedra Negra. Armadura ritual negra e roxa que incorpora cristais de Pedra Negra reais — não decorativos, estão fundidos com o corpo por baixo. No peito, um grande cristal de Pedra Negra como coração externo. Cajado ou bastão de Pedra Negra. Postura de liderança — este não recua.

**Cores:**
- Armadura: preto-mineral `#100C18` com cristais roxo-escuro `#3A1060`
- Cristal no peito: roxo intenso pulsante `#6020A0`
- Cajado: Pedra Negra pura `#0A0810` com topo cristalino `#5A1890`
- Rosto (visível): cinza com marcas roxas `#503860`

---

### Reflexo do Núcleo (`enemy_core_mirror`) — MEDIUM 32×48 px

**Descrição:**
Humanoide espelhado — cópia distorcida de alguém, sem identidade própria. O rosto é um espelho literalmente: superfície refletiva plana onde deveria haver feições. Movimentos duplicados com 1-2 frames de atraso — como se o sprite se repetisse offset. Quando ataca, o reflexo antecipa o movimento em direção oposta. Visualmente desconcertante por design.

**Cores:**
- Corpo: como uma silhueta cinza-médio `#808090`
- Rosto-espelho: prata `#C0C8D0` com reflexo do ambiente
- Duplicação de movimento: contorno secundário com 2px de offset em azul `#4060A0`

---

### Fera Distorcida por Mana (`enemy_mana_warped_beast`) — LARGE 48×56 px

**Descrição:**
Animal grande (felino ou urso) que absorveu Mana corrompida em excesso. A Mana modificou o corpo de forma não controlada: músculos de um lado são maiores que do outro, uma pata tem garras maiores que as demais, o pelo em partes está substituído por veios azul-roxo que pulsam através da pele visível. Não é elegante — é algo que está sofrendo sua própria transformação.

**Cores:**
- Pelo: marrom-escuro `#5A3A20` / partes sem pelo: cinza `#5A5868`
- Veios de Mana corrompida: azul `#4060D0` pulsando através da pele
- Olhos: azul-roxo `#6050D0` ambos do mesmo tom mas pupila diferente

---

### Eco Silencioso de Anya (`enemy_anya_silent_echo`) — MEDIUM 32×48 px

**Descrição:**
Este é o inimigo mais visualmente delicado do jogo. Não deve parecer um monstro — deve parecer algo **contido que não quer ser o que está sendo forçado a ser**. Figura luminosa, branco-azulada, com formas que sugerem uma figura feminina mas sem definição completa — mais luz que matéria. Correntes de Pedra Negra visíveis como sombras ao redor, tentando contê-la. A expressão (se visível) é de dor silenciosa, não de raiva. O jogador deve sentir pena, não apenas ameaça.

**Cores:**
- Corpo de luz: branco-azulado `#C0D8F0` com brilho suave
- Correntes de Pedra Negra: preto-roxo `#20101A` ao redor do corpo
- Luz interna (núcleo): branco quase puro `#F0F4F8`
- Expressão/silhueta: azul-médio suave `#80A0C0`

---

### Observador Tirano da Pedra Negra (`enemy_beholder_blackstone`) — HUGE 80×80 px

**Descrição:**
Versão corrompida do Tirano Ocular — o olho central foi substituído por Pedra Negra pura. Onde deveria haver um olho vivo, há um cristal negro opaco — mas que de alguma forma ainda "olha". Veios de Pedra Negra correm pelo corpo a partir do olho central, como raízes. Os olhos menores nos talos ainda são orgânicos mas com íris roxas. O olho de Pedra Negra emite um feixe negativo (suprime luz) em vez de um raio de luz.

**Cores:**
- Corpo: como o Tirano Ocular mas mais escuro `#281840`
- Olho central (Pedra Negra): preto mineral `#0A0810` sem reflexo
- Veios de Pedra Negra no corpo: preto-roxo `#201028`
- Olhos nos talos: roxo `#702890` com pupila preta

---

### Titã Escavador Quebra-Núcleo (`enemy_umber_hulk_corebreaker`) — HUGE 80×80 px

**Descrição:**
Monstro enorme de escavação — inimigo de presença física avassaladora. Quatro olhos dispostos em par (dois grandes acima, dois pequenos abaixo) — os olhos grandes têm efeito de confusão visual. Mandíbulas como duas caçambas mecânicas que se fecham. Braços de escavação com garras largas e chatas — feitas para arranhar e abrir rocha, não cortar. Placas negras de Pedra Negra reforçam o exoesqueleto natural.

**Cores:**
- Corpo/exoesqueleto: negro-azulado `#202030`
- Reforço de Pedra Negra: preto puro `#0A0810`
- Olhos grandes: amarelo-roxo `#A08010` com pupila multilobada
- Mandíbulas/garras: cinza-mineral `#505860`

---

### Lodo Devorador de Núcleo (`enemy_core_devourer_slime`) — HUGE 80×80 px

**Descrição:**
Lodo colossal escuro — versão corrompida e amplificada do Cubo de Lodo do bioma 1, mas enorme e sombrio. Corpo amorfo que preenche o espaço. Dentro, itens e ossos de aventureiros — mas também cristais de Pedra Negra parcialmente digeridos, que brilham através do corpo translúcido. O brilho interno não é verde como o cubo original — é roxo-negro. Uma "boca" indefinida que se abre quando vai engolir.

**Cores:**
- Corpo: preto-translúcido `#1A1020` (pontilhado para simular transparência)
- Brilho interno (Pedra Negra): roxo `#5020A0`
- Itens/ossos internos: silhuetas cinza `#404048`
- Bordas do corpo: roxo-escuro `#401060`

---

### Fragmento de Lich Corrompido (`enemy_corrupted_lich_shard`) — MEDIUM 32×48 px

**Descrição:**
Fragmento de um Lich destruído — não o lich inteiro, mas um pedaço de sua essência que ganhou forma autônoma. Esqueleto parcial envolto em energia roxo-negra. Partes do corpo estão "faltando" — braço que se dissolve, perna que é pura energia. Crânio visível mas com rachaduras por onde a energia negra escapa.

**Cores:**
- Osso: branco-amarelado `#E0D8B0` com rachaduras pretas
- Energia vazando: roxo-profundo `#5010A0` / bordas: preto `#1A0828`
- Olhos: roxo-brilhante `#A040E0`

---

### Espinhador Dracônico (`enemy_draconic_ashspitter`) — LARGE 48×56 px (nota: parte do roster dracônico)

**Descrição:**
Dragão menor — não um dragão adulto, mas claramente da linhagem. Quadrúpede com asas pequenas dobradas (não voa). Garganta inflada e incandescente — prestes a cuspir cinzas. Escamas enegrecidas com bordas cinza-quente, como carvão. Olhos vermelhos com pupila vertical.

**Cores:**
- Escamas: preto `#1A1810` com bordas cinza-quente `#706858`
- Garganta: laranja brilhante `#E06010` quando carregando ataque
- Olhos: vermelho-vivo `#C02010`
- Cinza cuspida (projétil): cinza com brilho `#808070`

---

### Wyrm do Vazio Dracônico (`enemy_draconic_void_wyrm`) — HUGE 80×80 px

**Descrição:**
Ser dracônico longo e sinuoso, como uma serpente com asas atrofiadas e membros pequenos. O corpo é parcialmente translúcido — o interior é escuridão do vazio. Não tem cor natural — é como uma silhueta de dragão com a realidade parcialmente apagada dentro. Os olhos são os únicos elementos brilhantes — branco-violeta intenso.

**Cores:**
- Silhueta exterior: roxo-muito-escuro `#200830` com escamas sugeridas
- Interior: vazio-preto translúcido (pixels pretos alternados — simule transparência)
- Olhos: branco-violeta intenso `#D0A0FF`
- Bordas do corpo: roxo levemente mais claro `#4A1880` onde a luz bate

---

# PARTE 5 — VARIANTES VISUAIS DE INIMIGOS

Para **qualquer** inimigo das classes acima, as seguintes variantes existem e devem ser sprites alternativos:

| Variante | Modificações visuais necessárias |
|---|---|
| **Veterano** | Cicatrizes visíveis nas áreas de pele/couro. Cor base levemente mais saturada-escura. Detalhe extra de equipamento (capacete riscado, escudo amassado). |
| **Elite** | Tamanho 10-20% maior. Aura curta visível (halo de 2-3 pixels da cor da magia do bioma). Ornamento dourado ou prateado em algum ponto. |
| **Corrompido** | Veios pretos visíveis na pele/couro. Olhos ou partes do rosto em roxo-escuro. Partículas de Pedra Negra flutuando ao redor (cristais roxos-pretos minúsculos). |
| **Lunar** | Tons de azul-escuro e prata substituem as cores primárias. Poeira lunar (partículas prata muito leves) ao redor. Sombras ligeiramente deslocadas. |
| **Ígneo** | Brasas visíveis na pele/armadura. Rachaduras incandescentes. Fumaça saindo. Olhos vermelhos mais intensos. |
| **Gélido** | Cristais de gelo crescendo no corpo. Vapor frio saindo. Tons azulados na pele. |
| **Raiz-Negra** | Raízes pretas emergindo do corpo. Seiva escura pingando. Folhas mortas presas. |

---

# PARTE 6 — BOSSES: BOSS GATES E NÍVEL 101

Bosses têm sprites únicos de 96×96 a 192×160 px. Cada boss é um projeto visual independente com **fases visuais** — o sprite muda ao mudar de fase (HP limiar). Ataques com telegrafação visual longa e clara. Silhueta deve ser imediatamente única e reconhecível. Pivot: bottom-center.

---

## BOSS GATES (Bosses de Checkpoint por Bioma)

---

### Matriarca Raiz-Negra (`boss_blackroot_matriarch`) — Gate Nível 15 | 128×128 px

**Descrição:**
Boss fixo — não se move pelo mapa, controla o espaço ao redor. Massa enorme de raízes e fungo interligados em forma de rainha vegetal: um núcleo central (corpo/cabeça) de massa fúngica branca-amarela encoberta por raízes negras que se estendem pelas paredes da sala. Do núcleo saem 6-8 "braços" de raiz que atacam independentemente. O núcleo tem uma abertura central — a "boca" ou "olho" — que abre e fecha expondo o ponto vulnerável. Fase 2: o núcleo sobe mais alto e mais raízes emergem do chão da sala.

**Fase 1 (HP alto):** Núcleo fechado, raízes espessas e ativas.
**Fase 2 (HP baixo):** Núcleo exposto permanentemente, mais raízes de chão, movimento mais caótico das raízes.

**Cores:**
- Raízes: preto-marrom `#1A0E08` com pontas de seiva roxa `#5A1860`
- Núcleo (fechado): massa branca-suja `#D0C8B0` com veios marrons
- Núcleo (aberto/vulnerável): interior vermelho-escuro `#801020` com brilho âmbar `#C09020`
- Fungos ao redor: branco, marrom, amarelo como o Baluarte Micélio

---

### Capitão Duergar do Gelo (`boss_duergar_frost_captain`) — Gate Nível 30 | 96×128 px

**Descrição:**
Duergar boss — claramente maior que qualquer duergar normal, mas não absurdamente (não é um golem, é um guerreiro grande). Armadura de gelo-metal completamente cobrindo o corpo. Escudo de gelo massivo que ocupa metade do sprite na fase 1 — tão grosso que projéteis não penetram. Martelo de guerra com cabeça de gelo. Barba em gelo sólido, longa, como chifres invertidos. Postura de quem não sabe o que é medo.

**Fase 1:** Escudo inteiro, corpo protegido, expressão controlada.
**Fase 2 (HP baixo):** Escudo **quebrado** — rachaduras e pedaços faltando. A armadura tem danos visíveis. Expressão muda — mais raiva, menos controle.

**Cores:**
- Armadura: azul-metal `#4060A0` / placas de gelo: azul-translúcido `#80B0E0`
- Escudo: gelo maciço `#A0C8E0` com rachaduras na fase 2
- Barba de gelo: branco-azulado `#C0E0F0`
- Olhos (visíveis no elmo): azul-frio brilhante `#60A0C0`

---

### Campeão Brasa de Kaand (`boss_kaand_ember_champion`) — Gate Nível 45 | 112×128 px

**Descrição:**
Orc enorme de Kaand que está em processo de transformação por fogo ritual — não queimando, **tornando-se fogo**. Fase 1: Orc enorme com marcas de guerra vermelho-intensas, arma de duas mãos incandescente, postura agressiva limpa. Fase 2: As marcas de guerra **brilham mais**, partes da pele começam a rachar e mostrar brasa interna, a arma está em chamas totais. Não é um elemental completo — é o caminho para se tornar um.

**Fase 1:** Orc reconhecível com marcas brilhantes. Expressão de guerreiro.
**Fase 2:** Rachaduras de brasa na pele. Arma em chamas. Expressão que não é mais completamente orc.

**Cores Fase 1:**
- Pele: verde-orc `#3A5828`
- Marcas de guerra: vermelho brilhante `#D02010`
- Arma: metal escuro com borda incandescente `#E04010`

**Cores Fase 2 (adições):**
- Rachaduras na pele: laranja-brasa `#E06010` brilhante
- Olhos: amarelo-fogo `#E0A010`

---

### Colosso de Enigma Bromeciano (`boss_bromecian_puzzle_colossus`) — Gate Nível 60 | 144×160 px

**Descrição:**
Constructo gigante da era de Bromécia — não é mágico, é tecnológico antigo. Corpo de blocos que se reorganizam continuamente: durante o combate, o Colosso literalmente desmonta e remonta partes de si mesmo para mudar forma de ataque. Fase 1: forma compacta de humanóide alto. Fase 2: se expande — braços mais longos, peito se abre revelando o **núcleo lógico** (cristal arcano de Bromécia, azul brilhante). Engrenagens visíveis nas articulações e nas transições de bloco.

**Fase 1:** Humanoide compacto. Núcleo não visível — coberto.
**Fase 2:** Expandido. Peito aberto com núcleo exposto. Mais peças em movimento.

**Cores:**
- Blocos de pedra: cinza-azulado `#5868A0`
- Metal das juntas: bronze antigo `#8A6020`
- Engrenagens: cobre `#9A7030`
- Núcleo lógico (exposto na fase 2): azul-brilhante `#4080E0` com aura `#80B0FF`

---

### Hierofante Sem-Lua Drow (`boss_moonless_drow_hierophant`) — Gate Nível 75 | 96×128 px

**Descrição:**
Drow ritualista de posição máxima — hierofante. Alto (topo da classe de tamanho Large/Boss). Manto lunar escuro que flutua e se move independente do corpo — animado por magia. Símbolos lunares orbitam ao redor do corpo como satélites. Não usa arma convencional — as mãos emitem magia lunar diretamente. Postura de sacerdote, não de guerreiro.

**Fase 1:** Símbolos lunares em órbita alta. Manto contido. Ataque de magia limpo.
**Fase 2:** Os símbolos caem em órbita mais baixa e caótica. O manto se expande e escurece a área ao redor. Expressão de ritual extremo.

**Cores:**
- Pele: azul-noite drow `#161825`
- Manto: azul-preto `#101520` com bordas azul-lunar `#3060A0`
- Símbolos lunares: prata `#B0C0D0` com brilho azul
- Energia mágica: azul-lunar `#4080C0`

---

### Wyvern de Pedra Negra (Boss Gate) (`boss_blackstone_wyvern`) — Gate Nível 90 | 160×128 px

**Descrição:**
Versão boss do Wyvern de Pedra Negra — maior, mais detalhado, com fases visíveis. A escala é boss: ocupa grande parte da tela. Fase 1: Wyvern completo, ambas as asas abertas em posição de ataque. Fase 2: **Uma asa está visivelmente danificada** — placas de Pedra Negra quebradas e caindo, abrindo o tecido de asa vivo embaixo. Torna-se mais agressivo e menos previsível.

**Fase 1:** Ambas as asas simétricas. Placas intactas.
**Fase 2:** Asa esquerda com placas quebradas/faltando, expondo membrana roxa abaixo. Movimento de asa irregular.

**Cores:**
- Placas de Pedra Negra: preto `#0E0C14`
- Fendas de roxo: `#501878`
- Asa danificada (membrana exposta): roxo-escuro `#401060` com vasos visíveis `#601880`

---

### Quebra-Juramento do Núcleo (`boss_core_oathbreaker`) — Gate Nível 100 | 160×160 px

**Descrição:**
Boss final antes do nível 101. Figura humanoide colossal que é simultaneamente de carne corrompida e de Pedra Negra — a fusão mais avançada de todas as áreas anteriores. Armadura de juramento antigo: o juramento está **quebrado visivelmente** — a armadura está em pedaços, cada pedaço flutuando em posição mas sem conectar. A Pedra Negra preenche os espaços entre os fragmentos de armadura como cola mineral. No peito, um cristal de Pedra Negra pulsando como coração.

**Fase 1:** Armadura em fragmentos flutuantes, mas controlados. Postura de poder.
**Fase 2:** Os fragmentos caem em órbita mais baixa, o núcleo de Pedra Negra brilha mais, o corpo subjacente à armadura começa a ser visível — cinza-apodrecido.

**Cores:**
- Armadura (fragmentos): azul-antigo `#3A4890` flutuando
- Pedra Negra entre fragmentos: preto `#0A0810`
- Núcleo no peito: roxo pulsante `#7020B0`
- Corpo subjacente (visível fase 2): cinza-corrompido `#504858`

---

## BOSSES DO NÍVEL 101 — CÂMARA FINAL

O nível 101 não é um dungeon tradicional — é a Câmara de Anya. Os "bosses" aqui são guardiões e instâncias finais antes do evento de conclusão.

---

### Guardião Primário de Pedra Negra (`boss_blackstone_warden_prime`) — 192×160 px

**Descrição:**
O guardião mais antigo da caverna — não construído por ninguém presente, encontrado já existente. Formato de sentinela: estático em posição de guarda até ser ativado. Quando ativado, os movimentos são **absolutamente opostos ao esperado** — lento quando devia ser rápido, rápido quando devia ser lento. Corpo de Pedra Negra pura, sem ornamentação, sem variação — monolítico. Dois olhos como fendas longas horizontais de luz roxa. Não tem boca. A ausência de detalhes é o detalhe.

**Cores:**
- Corpo: preto mineral absoluto `#080608`
- Olhos (fendas): roxo-branco `#9060E0`
- Arestas (única variação): roxo suavíssimo `#301848`

---

### Constructo Juramento de Elyndor (`boss_elyndor_oath_construct`) — 192×160 px

**Descrição:**
Constructo da era de Elyndor — a tecnologia mais avançada do mundo, agora sob corrupção parcial. Diferente de todos os outros constructos do jogo: este é **bonito**. Formas geométricas limpas, metal de cor azul-cobalto sem ferrugem, runas que ainda funcionam perfeitamente. A corrupção é visível mas parcial — Pedra Negra começando a crescer em uma extremidade (braço ou perna). A luta visual é entre a perfeição de Elyndor e o avanço da corrupção.

**Cores:**
- Metal de Elyndor: azul-cobalto `#2040A0` / detalhes: prata pura `#C0D0E0`
- Runas ativas: branco-azulado brilhante `#A0C8F0`
- Corrupção de Pedra Negra (crescendo): preto-roxo `#1A0828`
- Contraste intencional entre as duas partes

---

### Arauto Quebrado de Nyx (`boss_nyx_broken_herald`) — 160×160 px

**Descrição:**
Ser que foi mensageiro de Nyx e agora está partido — não morto, mas incapaz de completar sua função. Forma humanoide alongada, mais alto que deveria ser, como se o corpo tivesse sido esticado. Sombra viva que se move independente do corpo — não reflexo, outra entidade parcial. Fragmentos lunares ao redor — pedaços de lua materializada, como a Mariposa de Nyx mas em escala e peso completamente diferentes. Quando fala (visual de efeito sonoro), a sombra repete o movimento com atraso.

**Cores:**
- Corpo: azul-noite `#101825` alongado
- Sombra viva: mais escura e com bordas mais definidas `#080C15`
- Fragmentos lunares: prata-fria `#A0B0C8`
- Fissuras no corpo (quebrado): azul-branco `#80A8D0` nas rachaduras

---

### Remanescente Dracônico (`boss_draconic_core_remnant`) — 192×160 px

**Descrição:**
Não é um dragão com cabeça e garras. É o que sobrou de um dragão antigo depois da corrupção — um **núcleo vivo**. A forma é de um coração/órgão dracônico de tamanho colossal, pulsando, com fragmentos de escama e osso dracônico ao redor como órbita. Menos animal, mais estrutura. O núcleo em si é visualmente a combinação de lava (laranja-interno) e Pedra Negra (externo). Pulsa com batimentos lentos e irregulares.

**Cores:**
- Núcleo interno: laranja-lava `#C04010` pulsando para amarelo `#E09020`
- Envoltório de Pedra Negra: preto `#0A0810` com fendas de luz interna
- Fragmentos dracônicos em órbita: escamas negro-bronze `#3A2010`
- Ossada em órbita: branco-sujo `#D0C490`

---

### Eco de Anya Acorrentado (`boss_anya_bound_echo`) — 160×192 px

**Descrição:**
O momento final. Este sprite não deve parecer um inimigo. **Esta é Anya** — ou o que restou dela depois de séculos presa e corrompida pela Pedra Negra. A figura luminosa do Eco Silencioso de Anya mas em escala boss. Correntes de Pedra Negra visíveis e físicas — não são efeitos, são objetos que a prendem. A expressão é de **dor e reconhecimento** — ela vê o jogador e sabe quem é. O combate não é agressão dela — são as correntes agindo por conta própria, ou ela tentando se libertar mas causando dano no processo.

**Fase 1:** Correntes tensas e ativas. Figura luminosa tentando se conter. Luz quase cobre a Pedra Negra.
**Fase 2 (correntes quebradas parcialmente):** Mais luz visível. Mais beleza e mais dor. As correntes parcialmente quebradas flutuam como feridas abertas.

**Cores:**
- Figura de Anya: branco-azulado luminoso `#D0E8F8` com núcleo `#F0F8FF`
- Correntes de Pedra Negra: preto `#0A0810` com elos grandes
- Luz vazando pelas correntes quebradas: branco-dourado `#F0E8C0`
- Aura ao redor: azul-suave `#A0C0E0`

**Nota ao ilustrador:** Este é o sprite mais importante do jogo. Deve causar empatia, não medo. O jogador deve querer libertar ela, não destruir.

---

# RESUMO DE ENTREGAS ESPERADAS

## Personagens e NPCs

**Sprites genéricos (templates por raça):**
- [ ] Player (× 4 direções)
- [ ] NPC humano base (macho/fêmea)
- [ ] NPC elfo base
- [ ] NPC tiefling base
- [ ] NPC anão base
- [ ] NPC halfling base
- [ ] NPC orc base
- [ ] NPC meio-orc base
- [ ] NPC draconato base
- [ ] Criança NPC
- [ ] Pet gato (3 variações de cor)
- [ ] Pet cachorro (3 variações de cor)

**NPCs nomeados (sprites únicos):**
- [ ] Padre Corvus
- [ ] Mara Vellum
- [ ] Sylveth
- [ ] Brumdar Ferro-Quieto
- [ ] Nimble Galhobaixo
- [ ] Gurd Carvalho-Torto
- [ ] Hund Carvalho-Torto
- [ ] Ozzra Fumaçazul
- [ ] Gruta Panela-Funda
- [ ] Zrix das Estradas
- [ ] Yael Noite-Mansa
- [ ] Thalindra Véu-de-Lua
- [ ] Dagna Rocha-Morna
- [ ] Pip Semente-Solta
- [ ] Ser Alaric Veyr
- [ ] Mirela dos Laços
- [ ] Renko Três-Sorrisos
- [ ] Eiran Valeclaro
- [ ] Liora Canta-Rio
- [ ] Orlan Pouso-Curto
- [ ] Savra Escama-Verde
- [ ] Tovin Mãos-de-Selo
- [ ] Maelor Cinza

## Fazenda
- [ ] Tiles: grama (5 variações), solo seco, solo úmido, pedra/caminho, madeira
- [ ] Cultivos: cenoura, trigo, abóbora, flor — 5 fases cada
- [ ] Árvores: jovem, adulta (frutífera), grande, Mana (especial)
- [ ] Animais: galinha, pato, ovelha (lã cheia / tosada), cabra, vaca
- [ ] Construções: casa, galinheiro, celeiro, estufa, oficina
- [ ] Props: todos listados na tabela de props da fazenda
- [ ] Fonte de Anya (especial)
- [ ] Raiz Dormente de Mana (especial)

## Cidade
- [ ] Fachadas: todos os 17 prédios listados
- [ ] Props: todos listados na tabela de props da cidade
- [ ] Portão da Caverna

## Caverna — Tiles
- [ ] Bioma 1 (pedra): chão, parede, estalactites
- [ ] Bioma 2 (floresta): chão, parede, cogumelos props
- [ ] Bioma 3 (gelo): chão, parede, cristais
- [ ] Bioma 4 (fogo): chão, parede, lava
- [ ] Bioma 5 (ruínas): chão, parede, engrenagens props
- [ ] Bioma 6 (abismo): chão, parede, cristais negros
- [ ] Bioma 7 (corrompido): chão, parede, Blackstone

## Inimigos — Base
Todos os inimigos listados nas seções acima × 4 direções + variantes marcadas com prioridade

---

## Inimigos — Núcleo Corrompido (86-99)
- [ ] Massa Corrompida
- [ ] Oráculo Ninrorin Quebrado
- [ ] Pseudodragão Corrompido
- [ ] Wyvern de Pedra Negra (inimigo)
- [ ] Paragon da Pedra Negra
- [ ] Reflexo do Núcleo
- [ ] Fera Distorcida por Mana
- [ ] Eco Silencioso de Anya
- [ ] Observador Tirano da Pedra Negra
- [ ] Titã Escavador Quebra-Núcleo
- [ ] Lodo Devorador de Núcleo
- [ ] Fragmento de Lich Corrompido
- [ ] Espinhador Dracônico
- [ ] Wyrm do Vazio Dracônico

## Boss Gates
- [ ] Matriarca Raiz-Negra (nível 15)
- [ ] Capitão Duergar do Gelo (nível 30)
- [ ] Campeão Brasa de Kaand (nível 45)
- [ ] Colosso de Enigma Bromeciano (nível 60)
- [ ] Hierofante Sem-Lua Drow (nível 75)
- [ ] Wyvern de Pedra Negra (nível 90)
- [ ] Quebra-Juramento do Núcleo (nível 100)

## Bosses Nível 101
- [ ] Guardião Primário de Pedra Negra
- [ ] Constructo Juramento de Elyndor
- [ ] Arauto Quebrado de Nyx
- [ ] Remanescente Dracônico
- [ ] Eco de Anya Acorrentado *(sprite mais importante do jogo)*

---

*Documento produzido para produção de sprites pixel art de Cindar's Hope. Manter referência a Stardew Valley (superfície) e Children of Morta (caverna) como guias de mood e craft técnico.*
