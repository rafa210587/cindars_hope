# Cindar's Hope — Player Core Systems Direction

> **Status:** documento canônico de direção ampla dos sistemas centrais do personagem  
> **Local:** `docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/SPEC_SOURCE_MAP.md`  
> - `docs/design/SPECIFICATION_PROCESS.md`  
> - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`  
> - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`  
> - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`  
> - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`  
> **Função:** consolidar a direção ampla do personagem jogável: criação inicial, raças iniciais, gênero, aparência por sprites, atributos, HP/MP/Stamina/Breath, fome, cansaço, level up, skill trees existentes, active slots, arquétipos inferidos, ferramentas, armas, magia, equipamentos, resistências, morte, Fonte de Anya, companions, pets, fazenda, cidade, caverna, flerte, casamento, UI/HUD e save/load.  
> **Não é spec implementável.** Este documento descreve como o sistema deve funcionar em visão de jogo. Specs futuras quebram partes disso quando entrarmos em execução.

---

## 0. Regra de uso

Este é o documento canônico de direção para o personagem jogável.

Ele deve orientar refinamentos futuros de:

```text
criação do personagem
raças jogáveis iniciais
raças futuras
gênero / apresentação
aparência em sprites
atributos
HP / MP / Stamina / Breath
fome
cansaço
level up
pontos de atributo
50 SkillPoints máximos
5 skill trees existentes
active slots
capstones
respec na Fonte de Anya
arquétipos inferidos / jobs vestíveis
ferramentas
armas
magia
equipamentos
resistências
status negativos
morte / derrota / Fonte de Anya
companions
pets
fazenda
cidade
caverna
UI/HUD
save/load
flerte / relacionamento / casamento
```

Regra principal:

```text
O jogador não possui classe fixa estilo D&D.
O jogador evolui livremente por atributos, skills, equipamentos, armas, ferramentas, magia e escolhas de gameplay.
O jogo usa 5 skill trees iniciais já consolidadas: Melee/Guerreiro, Ranged/Caçador, Magic/Arcano, Survival/Sobrevivente e Crafting/Produção.
Social, romance, companions e pets são sistemas transversais neste momento; não substituem uma das 5 árvores existentes.
O jogo pode inferir arquétipos/jobs funcionais a partir da distribuição de skills, atributos e equipamentos.
O jogador pode vestir/ativar um arquétipo desbloqueado para receber bônus, sem ficar preso a uma classe permanente.
```

Referência de inspiração:

```text
As skill trees podem se inspirar em arquétipos de fantasia clássica/D&D, como guerreiro, caçador, mago, curandeiro, sobrevivente e artesão.
Não devem copiar nomes proprietários, textos, progressões oficiais ou features fechadas.
A adaptação deve ser própria de Cindar's Hope e Vaalara.
```

---

# PARTE A — Visão geral do personagem

## 1. Fantasia de gameplay

O personagem é um habitante/adventurer de Vaalara que pode combinar vida rural, exploração e crescimento pessoal.

O jogo deve permitir builds híbridas como:

```text
fazendeiro-mago
minerador-tanque
explorador arqueiro
combatente de armas pesadas
criador de animais com pet de combate
artesão focado em economia
aventureiro de caverna com magia e resistência ambiental
personagem ligado à Fonte de Anya
```

A identidade do personagem deve surgir de escolhas mecânicas e sociais, não de uma classe inicial fixa.

## 2. Princípios de design

```text
Liberdade primeiro.
Classes fixas não devem limitar o jogador.
Raça/gênero/aparência não devem impedir acesso a conteúdo central.
Atributos definem aptidão bruta, não substituem skills.
Skills e skill trees definem domínio, eficiência, rendimento, técnicas e especializações.
O jogador terá no máximo 50 SkillPoints para distribuir.
A economia de pontos deve permitir masterizar uma árvore e investir parcialmente em outras.
Não deve ser possível dominar todas as árvores em uma única build.
Magia usa MP.
Ações físicas usam Stamina e/ou Breath.
Fome e cansaço afetam performance.
Companions e pets ajudam, mas não substituem o jogador.
Flerte e casamento respeitam NPCs, disponibilidade e narrativa.
A Fonte de Anya conecta morte, respec, cura, narrativa e progressão especial.
```

---

# PARTE B — Criação inicial do personagem

## 3. Objetivo da criação de personagem

A criação inicial deve ser simples, legível e compatível com produção em sprites.

O jogador escolhe:

```text
nome
raça inicial
gênero/apresentação
sprite base de corpo
tipo de cabelo
cor de cabelo
cor/variação visual básica, quando suportado pela raça
```

Regra:

```text
O jogo não terá editor profundo de corpo/rosto.
A customização deve ser limitada, clara e sustentável para spritesheets.
```

## 4. Raças jogáveis iniciais

Para o início do jogo, apenas duas raças serão jogáveis.

| Raça | Nome no jogo | Direção visual | Papel de gameplay |
|---|---|---|---|
| Humano de Dornécia | Humano | silhueta padrão 32x48, maior variação de cabelo/pele | versátil, social, sem especialização extrema |
| Anão de Khaz Baruk | Anão | corpo baixo/largo, barba/cabelo forte, base visual própria | mineração, Constituição, Força, ferramentas e resistência |

Regras:

```text
A versão inicial deve focar em Humano e Anão para reduzir custo de sprites, animações e balanceamento.
Raça inicial pode dar identidade visual, pequenos bônus e pequenos traços passivos.
Raça inicial não deve bloquear romances, profissões, magia, fazenda ou final do jogo.
Bônus raciais devem ser menores que escolhas de build, skills, equipamentos e arquétipos.
```

## 5. Raças futuras

As demais raças existem em Vaalara e podem ser consideradas no futuro, mas não devem ser implementadas como jogáveis no início.

| Raça futura | Observação |
|---|---|
| Elfo da Noite / Luandil | exige orelhas/paleta/animações próprias; bom para Destreza/Vontade/exploração |
| Halfling | exige sprite pequeno e validação de hitbox/ferramentas |
| Tiefling | exige chifres/cauda compatíveis com cabelo/animações |
| Meio-Orc | exige silhueta mais robusta e leitura própria |
| Draconato | exige cabeça/escamas/crista/cauda e sprites próprios extensivos |
| Goblin | exige base pequena, orelhas grandes e animações rápidas |
| Gnomorin / gnomos técnicos | bom para tecnologia bromeciana, mas futuro |
| Drow / linhagens sombrias | dependem de direção narrativa de Nyx/Abismo |
| Nymirianos | ligados a Anya/Cindar/Água Viva/Fonte; devem permanecer lore profunda, não raça inicial comum |

Regra:

```text
Raças futuras devem ser tratadas como expansão de direção/escopo.
Não devem entrar de forma oportunista sem sprites, animações, passivas, UI e save/load definidos.
```

## 6. Gênero / apresentação

Direção inicial:

```text
Masculino
Feminino
Neutro/Indefinido, se a UI e os textos suportarem
```

Regras:

```text
Gênero/apresentação não altera atributos.
Gênero/apresentação não bloqueia romance por padrão.
O efeito principal é visual, pronome/texto, animação base se necessário e leitura social.
```

## 7. Aparência em sprites

Por ser um jogo em sprites, a customização deve ser controlada.

O jogador escolhe:

```text
tipo de cabelo
cor de cabelo
variação básica de pele, quando suportada
roupa inicial simples, se suportada
```

Não teremos no início:

```text
editor de rosto detalhado
altura customizada livre
peso/corpo customizado livre
morfologia facial detalhada
múltiplas camadas complexas de roupa
```

Tipos iniciais de cabelo:

```text
curto simples
médio simples
longo simples
preso/rabo de cavalo
cacheado/volumoso
careca/sem cabelo
```

Cores iniciais:

```text
preto
castanho
loiro
ruivo
branco/cinza
azul escuro/fantasia
roxo/fantasia
verde escuro/fantasia
```

Regras:

```text
Humano pode usar todos os cabelos iniciais.
Anão deve suportar cabelo/barba como marcador visual forte.
Cada opção visual precisa ser sustentável em spritesheets de idle, walk, tool use, combat, hit, sleep/defeat e social interaction.
```

## 8. Sprite base por raça inicial

| Raça | Base sprite | Observação |
|---|---|---|
| Humano | Medium 32x48 | base padrão |
| Anão | Short/Stocky 32x40 ou 32x44 | collider/footbox deve continuar consistente |

Regra:

```text
Collider/footbox deve preservar justiça de gameplay.
Anão não deve ter vantagem abusiva de hitbox.
Humano não deve ser punido por sprite maior.
```

---

# PARTE C — Atributos principais

## 9. Atributos canônicos

```text
Força
Constituição
Destreza
Inteligência
Vontade
Carisma
```

Stats derivados:

```text
HP
MP
Stamina
Breath / Fôlego
Resistências
Carga / peso futuro
Precisão / crítico futuro
Eficiência de ferramentas
Eficiência de magia
Eficiência social
```

## 10. Progressão de atributos

Regra canônica inicial:

```text
No level 1, o jogador começa com 1 ponto em cada atributo principal.
A cada level up, o jogador ganha +1 ponto para distribuir em um atributo principal.
```

Direção:

```text
Atributo representa aptidão bruta.
Skill representa domínio técnico.
Equipamento representa capacidade material.
Buff representa vantagem temporária.
```

## 11. Regra contra sobreposição atributo/skill

```text
Força não aumenta quantidade de recursos coletados.
Constituição não gera regeneração passiva de vida por si só.
Destreza não transforma dodge em skill; dodge é ação base.
Inteligência não substitui Crafting/Produção.
Vontade pode influenciar regeneração lenta de MP, mas magia forte depende de Magic/Arcano, equipamento e custo.
Carisma não substitui reputação, quests e relação real com NPCs.
```

Rendimento extra de recursos deve vir de:

```text
skills
nodes de skill tree
ferramentas melhores
buffs de comida
companions/pets
arquétipos ativos
eventos raros
```

## 12. O que cada atributo representa

### Força

Representa potência física e capacidade de aplicar força.

Afeta:

```text
dano com armas pesadas
facilidade para derrubar árvores resistentes
facilidade para quebrar rochas resistentes
redução de número de golpes necessários em alguns obstáculos
stagger/knockback
uso de HeavyAttack
```

Não afeta diretamente:

```text
quantidade de madeira obtida
quantidade de minério obtida
chance de drop raro
qualidade de recurso
```

Esses ganhos pertencem a skills, ferramentas, buffs ou nodes.

### Constituição

Representa robustez, saúde e tolerância corporal.

Afeta:

```text
HP máximo
resistência a dano físico
resistência a veneno/sangramento/frio/calor em menor grau
limite de cansaço antes de penalidades fortes
capacidade de long runs na caverna
resistência a stagger/knockdown, se definido
```

Não afeta diretamente:

```text
regeneração passiva de vida
cura automática forte
ignorar fome/cansaço
```

Regeneração de vida, se existir, deve vir de:

```text
Survival/Sobrevivente
comida/poção
Fonte de Anya
gear
buffs específicos
```

### Destreza

Representa coordenação, mobilidade, precisão e reflexo.

Afeta:

```text
velocidade de ataque leve
precisão com arco/dagger/spear
interação com janelas de crítico
movimentação em combate
backstab/positioning
reação contra traps, se houver skill adequada
```

Regra:

```text
Dodge é ação base, não skill.
Skills podem melhorar o dodge aumentando janela de invulnerabilidade, custo, recovery ou consistência.
```

### Inteligência

Representa conhecimento, técnica, raciocínio, magia estruturada e produção avançada.

Afeta:

```text
eficiência de magia técnica
crafting avançado
identificação de monstros/traps/recursos
uso de tecnologia bromeciana
leitura de ruínas/mecanismos
```

### Vontade

Representa força espiritual, foco, resistência mental e ligação com magia profunda.

Afeta:

```text
MP máximo principal ou secundário, conforme fórmula futura
regeneração muito lenta de MP
resistência a medo/confusão/Nyx/Void
força de magias espirituais
resistência a corrupção
interação com Fonte de Anya
estabilidade em eventos de lore
```

Regra de MP regen:

```text
MP regenera naturalmente de forma lenta.
Vontade melhora essa regeneração lentamente.
Nodes da árvore Magic/Arcano podem melhorar regeneração ou reduzir custo.
Regeneração rápida de MP deve depender de comida, poção, Fonte, equipamento, Fruto de Mana ou capstone específico.
```

### Carisma

Representa presença, comunicação, empatia, influência social e liderança.

Afeta:

```text
relacionamentos
flerte
casamento
reputação
preços/negociação futura
companions
moral/afinidade de party
eventos sociais
festivais
alguns arquétipos sociais/suporte
```

---

# PARTE D — HP / MP / Stamina / Breath

## 13. HP

HP representa sobrevivência física.

Fontes:

```text
Constituição
equipamentos
alimentos/buffs
nodes de Survival/Sobrevivente
nodes defensivos de Melee/Guerreiro
arquétipo inferido ativo
Fonte de Anya/eventos especiais
```

Regra:

```text
HP não regenera automaticamente por Constituição.
Regeneração de HP deve ser desbloqueada/ativada por skill, item, equipamento, comida ou Fonte.
Itens podem amplificar regeneração de HP, mas a base mecânica deve vir de skill/efeito explícito.
```

## 14. MP

MP representa energia mágica para habilidades mágicas.

Fontes:

```text
Vontade
Inteligência
equipamentos mágicos
comida rara
Fruto de Mana
Fonte de Anya
Magic/Arcano
arquétipos inferidos mágicos
```

Regra:

```text
MP é obrigatório para magia e skill actions mágicas.
MP regenera naturalmente de forma lenta baseada principalmente em Vontade.
Magia não deve consumir apenas stamina.
```

## 15. Stamina

Stamina representa energia física de ação.

Consome em:

```text
arar
regar
minerar
cortar madeira
pescar, se aplicável
ataques físicos
charged attacks
block
skills físicas
dodge
dash, após desbloqueio
correr, se definido
uso pesado de ferramentas
```

Stamina afeta cansaço:

```text
Gastar stamina acelera crescimento de cansaço.
Gastar stamina com fome acelera ainda mais.
```

## 16. Breath / Fôlego

Breath representa ritmo respiratório, explosão, sustentação e capacidade de manter esforço sob pressão.

Afeta:

```text
corrida
dash
dodge
ritmo de combate
uso de armas pesadas
block sustentado, se definido
resistência a ambientes de calor/frio/gás
recuperação de stamina em combate, se definido
capacidade de long fights
fuga e reposicionamento
```

Diferença entre Stamina e Breath:

```text
Stamina = energia disponível para executar ações.
Breath = capacidade de sustentar ritmo, explosão e recuperação sob esforço.
```

---

# PARTE E — Fome, cansaço, level e respec

## 17. Fome

Fome representa nutrição/energia alimentar.

Efeitos possíveis:

```text
reduz regeneração de stamina
acelera cansaço
reduz eficiência de ações físicas
piora desempenho em long runs
pode afetar humor/rotina social levemente, se definido
```

Regra:

```text
Fome não deve matar o jogador de forma punitiva no design base.
Fome deve pressionar planejamento, alimentação, fazenda e cozinha.
```

## 18. Cansaço

Cansaço é sistema próprio, mas afeta `PlayerConditionManager`.

Aumenta com:

```text
passagem do tempo
uso de stamina
ações pesadas
combate
fome
long runs na caverna
noite avançada
status negativos
```

Efeitos por estágio:

```text
Leve: redução pequena de recuperação.
Moderado: ações consomem mais stamina/breath.
Alto: menor dano/eficiência, movimento pior, risco em combate.
Extremo: jogador precisa dormir/retornar, risco de colapso conforme direção futura.
```

## 19. Level up e pontos

Regra canônica inicial:

```text
A cada level: +1 ponto de atributo principal.
SkillPoint: 1 ponto a cada 2 níveis.
SkillPoints máximos esperados no endgame: 50.
```

O level representa experiência geral e pode vir de:

```text
combate
mineração
fazenda
crafting/produção
quests
pesca
exploração
bosses
relacionamentos/eventos, se definido
```

## 20. Respec

Respec é feito na Fonte de Anya.

Regras:

```text
Permite redistribuir pontos de skill e/ou atributos, conforme escopo futuro.
Custo cresce com o nível do que está sendo respeccado.
Não deve ser tão barato que escolhas não importem.
Não deve ser tão caro que impeça experimentação.
Explicação diegética: a Fonte reorganiza ecos de crescimento do personagem.
```

---

# PARTE F — Economia das 5 Skill Trees

## 21. Regra geral

O modelo inicial mantém 5 árvores:

```text
Melee / Guerreiro
Ranged / Caçador
Magic / Arcano
Survival / Sobrevivente
Crafting / Produção
```

Regras consolidadas:

```text
5 tiers por árvore.
Tier 5 = capstone.
4 active slots.
Passivas não ocupam active slot.
SkillPoint a cada 2 níveis.
Máximo esperado: 50 SkillPoints.
Respec na Fonte de Anya.
```

## 22. Economia de pontos

A árvore precisa ser grande o bastante para dar escolha real, mas não grande demais para virar ruído.

Direção:

```text
Cada árvore deve ter aproximadamente 10-12 nodes principais + 1 capstone.
Cada árvore deve ter 3-5 active skills possíveis.
Cada árvore deve ter passivas, melhorias de eficiência e pelo menos uma escolha de especialização.
Masterizar uma árvore deve custar aproximadamente 28-32 pontos.
Completar absolutamente tudo de uma árvore pode custar 38-45 pontos, mas isso não deve ser necessário.
Com 50 pontos, o jogador deve conseguir masterizar uma árvore e investir 15-20 pontos em outras.
Com 50 pontos, o jogador não deve conseguir masterizar duas árvores completas.
```

Modelo de desbloqueio sugerido:

| Tier | Requisito de pontos gastos na árvore | Função |
|---:|---:|---|
| Tier 1 | 0 | fundamentos e primeiras ações |
| Tier 2 | 4 | eficiência e custo |
| Tier 3 | 9 | novas técnicas relevantes |
| Tier 4 | 16 | especialização forte |
| Tier 5 | 24 | capstone |

Regras:

```text
Node simples pode custar 1 ponto.
Node escalável pode ter ranks 1-5.
Active skill forte pode exigir pré-requisito e custar 1-3 pontos/ranks.
Capstone deve custar 3-5 pontos e exigir Tier 5.
```

## 23. Tiers vs ranks

```text
Tree Tier = posição/desbloqueio na árvore.
Skill Rank = melhoria interna da habilidade específica.
```

Exemplo:

```text
Dash pode ser desbloqueado em Survival Tier 2.
Dash Rank 1 habilita a ação.
Dash Ranks 2-5 reduzem custo, cooldown, recovery ou melhoram distância de forma controlada.
```

---

# PARTE G — Melee / Guerreiro

## 24. Função da árvore

Melee/Guerreiro cobre:

```text
combate corpo a corpo
armas físicas próximas
block
defesa física ativa
stagger
heavy attacks
critical windows corpo a corpo
sobrevivência em proximidade
```

Atributos principais:

```text
Força
Constituição
Destreza
Breath/Fôlego como stat derivado importante
```

## 25. Skills sugeridas

| Skill/Node | Tipo | Ranks | Função |
|---|---|---:|---|
| Ataque Pesado | active/modifier | 1-5 | melhora golpes carregados, dano de postura e custo controlado |
| Block / Bloqueio | active | 1-5 | habilita e melhora bloqueio com arma/escudo, reduz dano frontal |
| Guarda Firme | passive | 1-5 | reduz custo de stamina/breath ao bloquear e melhora resistência a stagger |
| Contra-Ataque | active/reaction | 1-3 | permite resposta após block perfeito ou defesa bem-sucedida |
| Quebra-Postura | passive | 1-5 | aumenta stagger contra inimigos vulneráveis ou após telegraph pesado |
| Lâmina Precisa | passive | 1-5 | melhora dano em critical windows corpo a corpo |
| Golpe Circular | active | 1-3 | ataque em área curta para lidar com swarms |
| Especialização: Arma Pesada | passive | 1-5 | melhora hammer/axe/sword pesada, custo alto e dano alto |
| Especialização: Arma Leve | passive | 1-5 | melhora dagger/sword leve/spear curta, timing e velocidade |
| Tenacidade de Batalha | passive | 1-5 | reduz penalidade de hit/stagger sem regenerar vida diretamente |
| Executor de Janelas | passive | 1-3 | melhora recompensa por atacar durante janela vulnerável |
| Capstone: Guerreiro das Profundezas | capstone | 3-5 | fortalece postura, critical windows e sustain físico sem trivializar bosses |

Regras:

```text
Block é skill e precisa ser habilitada.
Dodge não pertence a Melee como skill base.
Melee pode melhorar punição, postura e defesa, mas não deve substituir Survival em mobilidade/long runs.
```

HUD relacionado:

```text
ícone de Block habilitado
feedback de block perfeito
barra/indicador de stamina/breath durante block sustentado
ícone de stagger aplicado no inimigo
feedback de critical window corpo a corpo
cooldown de Contra-Ataque/Golpe Circular se forem active skills
```

---

# PARTE H — Ranged / Caçador

## 26. Função da árvore

Ranged/Caçador cobre:

```text
combate à distância
arcos/projéteis
pontos fracos
inimigos voadores/flutuantes
controle de distância
interrupção de casters
abertura de combate segura
```

Atributos principais:

```text
Destreza
Inteligência
Vontade secundária para foco/controle
```

## 27. Skills sugeridas

| Skill/Node | Tipo | Ranks | Função |
|---|---|---:|---|
| Mira Estável | passive | 1-5 | melhora precisão e reduz penalidade de movimento |
| Tiro em Ponto Fraco | passive | 1-5 | aumenta dano em partes vulneráveis/olhos/asas/core exposed |
| Disparo Carregado | active | 1-5 | tiro forte com windup, bom contra armor/voadores |
| Disparo de Interrupção | active | 1-3 | interrompe casts fracos ou abre pequena janela em casters |
| Passo do Caçador | passive | 1-5 | melhora reposicionamento após tiro sem virar dash principal |
| Flecha Perfurante | active | 1-3 | projétil atravessa linha curta, útil contra packs estreitos |
| Marcador de Presa | active/debuff | 1-3 | marca alvo, melhora dano próprio/companion/pet contra aquele inimigo |
| Caçador de Voadores | passive | 1-5 | melhora dano/precisão contra inimigos floating/flying |
| Armadilha de Caçador | active/utility | 1-3 | trap simples de slow/root curto, sem substituir traps do mundo |
| Flecha Elemental Simples | hybrid | 1-3 | usa item/consumível ou MP leve para dano elemental controlado |
| Olho na Escuridão | passive | 1-5 | melhora leitura de inimigos em caverna/sombra, sem revelar tudo |
| Capstone: Predador Silencioso | capstone | 3-5 | melhora abertura de combate, pontos fracos e interrupção sem matar elites/bosses sem risco |

Regras:

```text
Ranged não deve virar magia gratuita.
Efeitos elementais fortes devem consumir itens, preparo ou MP.
Ranged deve ser forte contra voadores, casters e pontos fracos, mas vulnerável a pressão corpo a corpo.
```

HUD relacionado:

```text
retículo/mira simples quando arma ranged equipada
ícone de alvo marcado
feedback visual de ponto fraco
contador de munição, se existir
cooldowns de Disparo de Interrupção, Marcador de Presa e Armadilha de Caçador
ícone de flecha/munição elemental, se ativa
```

---

# PARTE I — Magic / Arcano

## 28. Função da árvore

Magic/Arcano cobre:

```text
MP
magias ofensivas
magias defensivas
magias elementais
magias espirituais/divinas
cura limitada
suporte
controle leve
uso de itens mágicos
Fonte de Anya
Mana
corrupção
Blackstone/Nyx/Void
```

Atributos principais:

```text
Vontade
Inteligência
Carisma secundário para suporte/liderança espiritual, se existir
```

## 29. Escolas/tipos de magia do jogo

O jogo pode organizar magia por tipos próprios:

```text
Elemental
  fogo, gelo, raio, água/natureza, pedra/terra se fizer sentido.

Arcana
  projéteis, barreiras, selos, utilidade, manipulação leve.

Espiritual/Divina
  cura limitada, purificação, proteção, Fonte de Anya, Água Viva.

Sombria/Corrompida
  relacionada a Nyx/Void/Blackstone; deve ser controlada por lore, risco e progressão.
```

Regra:

```text
Magia espiritual/divina não significa que o jogador serve diretamente a Anya no início.
A Fonte de Anya deve continuar sendo mistério/lore progressiva.
```

## 30. Skills sugeridas

| Skill/Node | Tipo | Ranks | Função |
|---|---|---:|---|
| Canalização Serena | passive | 1-5 | reduz custo de MP de magias simples |
| Fluxo Lento | passive | 1-5 | melhora levemente regen natural de MP baseada em Vontade |
| Foco Arcano | passive | 1-5 | melhora potência/precisão/alcance de magia técnica |
| Afinidade Elemental: Fogo | passive/active unlock | 1-5 | fortalece Burn/fogo e interação com inimigos vulneráveis |
| Afinidade Elemental: Gelo | passive/active unlock | 1-5 | fortalece Chill/slow/control leve |
| Afinidade Elemental: Raio | passive/active unlock | 1-5 | fortalece interrupção/overload contra constructos |
| Afinidade Natural/Água | passive/active unlock | 1-5 | suporte, purificação leve, utilidade ambiental |
| Selo de Proteção | active | 1-3 | barreira/ward curto, custo de MP, cooldown |
| Toque Restaurador | active | 1-5 | cura limitada, custo alto, não substitui comida/poções |
| Resistir Corrupção | passive | 1-5 | reduz efeito de Blackstone/Corruption/Nyx/Void |
| Uso de Item Mágico | passive/utility | 1-5 | permite usar melhor varinhas, selos, amuletos, scrolls/itens raros |
| Eco da Fonte | passive/lore | 1-5 | melhora interação com Fonte/Água Viva/respec/cura especial após marcos |
| Capstone: Fragmento Desperto | capstone | 3-5 | fortalece interação com Fonte, Mana e magia de suporte após marcos narrativos profundos |

Regras:

```text
MP regenera lentamente por base.
Vontade melhora regen lentamente.
Magic/Arcano pode reduzir custo, melhorar eficiência e dar acesso a cura/suporte.
Cura mágica deve ter custo, cooldown e limite para não invalidar comida, poções e Survival.
Magia elemental deve criar escolhas de build, não desbloquear tudo barato.
Uso de itens mágicos pode exigir rank mínimo.
```

HUD relacionado:

```text
barra de MP
indicador de regen lenta de MP
ícones de elemento ativo/preparado
cooldowns de magia
ícone de corrupção/resistência espiritual
feedback de cura recebida
ícone de item mágico equipado/usável
estado da Fonte de Anya quando interagindo com ela
```

---

# PARTE J — Survival / Sobrevivente

## 31. Função da árvore

Survival/Sobrevivente cobre:

```text
long runs
cansaço
fome
hazards
frio/calor/gás
traps
secret rooms
recuperação fora de combate
mobilidade de exploração
dash
melhoria de dodge
regen de HP por skill
resistência a medo/corrupção em menor grau
```

Atributos principais:

```text
Constituição
Destreza
Vontade
Inteligência secundária para leitura de ambiente
```

## 32. Skills sugeridas

| Skill/Node | Tipo | Ranks | Função |
|---|---|---:|---|
| Dash | active/movement | 1-5 | habilita dash; ranks reduzem custo/cooldown/recovery e melhoram distância moderadamente |
| Reflexo de Esquiva | passive | 1-5 | dodge continua ação base, mas ganha janela de invulnerabilidade maior ou recovery menor |
| Fôlego de Jornada | passive | 1-5 | reduz gasto de Breath em corrida/dash/exploração |
| Passo Seguro | passive | 1-5 | reduz chance de ativar traps simples e melhora leitura de chão perigoso |
| Olho de Explorador | passive | 1-5 | melhora detecção de secret rooms, atalhos e landmarks |
| Sangue Frio | passive | 1-5 | reduz penalidades de frio/calor/gás/hazards ambientais |
| Descanso Curto | passive/utility | 1-5 | permite pequena recuperação fora de combate; pode incluir HP regen leve |
| Regeneração Natural | passive | 1-5 | HP regen lenta e limitada fora de combate; itens podem amplificar |
| Estômago Forte | passive | 1-3 | reduz penalidade de fome e melhora comida simples |
| Resistência Mental | passive | 1-5 | reduz Fear/ConfusionLite/Nyx/Void de forma moderada |
| Desarmar Armadilha | utility | 1-5 | permite desarmar traps por tipo/tier, possivelmente com ferramenta |
| Sobrevivência de Caverna | passive | 1-5 | reduz cansaço acumulado em long runs |
| Capstone: Sobrevivente das Profundezas | capstone | 3-5 | reduz cansaço de exploração, melhora leitura de perigos e sustenta long runs sem remover risco |

Regras:

```text
Dash é skill e precisa ser habilitada.
Dodge não é skill; é ação base.
Survival pode melhorar dodge, mas não transformar o jogador em invulnerável.
HP regen é skill, não efeito automático de Constituição.
Itens podem amplificar HP regen, mas não criar regen forte sem base/efeito claro.
```

HUD relacionado:

```text
ícone/cooldown de Dash
feedback de dodge bem-sucedido
indicador sutil de janela de invulnerabilidade melhorada, se necessário
ícone de HP regen ativa
ícones de fome/cansaço
ícones de proteção ambiental
alerta visual/sonoro de trap detectada
indicador de secret room/landmark apenas quando descoberta ou suspeita
```

---

# PARTE K — Crafting / Produção

## 33. Função da árvore

Crafting/Produção cobre:

```text
fazenda
ferramentas
mineração/coleta como rendimento
madeira/minério/crops/produtos
crafting
cozinha
processadores
oficinas
engenharia bromeciana inicial/futura
qualidade e yield de recursos
desbloqueio de tiers de material
armas e ferramentas por material
```

Atributos principais:

```text
Inteligência
Força
Constituição
Destreza
Carisma secundário para comércio/animais
Vontade secundária para Mana/Fonte/crops raras
```

## 34. Tiers de material

Crafting/Produção deve controlar acesso a materiais e equipamentos melhores.

Tiers iniciais sugeridos:

```text
Tier 1 — Madeira / Pedra / Cobre
Tier 2 — Ferro / Couro tratado / Madeira boa
Tier 3 — Aço / Prata
Tier 4 — Mithril / Liga Bromeciana
Tier 5 — Pedra Negra Estabilizada / Material especial de Anya ou Mana, conforme lore
```

Regras:

```text
Prata pode ser material importante contra mortos-vivos, sombras, maldições ou criaturas específicas.
Mithril pode ser material leve/forte para equipamentos avançados.
Liga Bromeciana pode depender de ruínas/engenharia.
Pedra Negra comum é perigosa/corrompida.
Pedra Negra Estabilizada é recurso raro/endgame e não deve banalizar corrupção.
Material ligado a Anya/Mana deve depender de lore profunda, Fonte, Água Viva ou nível 101.
```

## 35. Skills sugeridas

| Skill/Node | Tipo | Ranks | Função |
|---|---|---:|---|
| Mãos de Lavrador | passive | 1-5 | reduz custo de stamina em ações agrícolas |
| Colheita Cuidadosa | passive | 1-5 | melhora qualidade/yield de crops por skill |
| Lenhador Eficiente | passive | 1-5 | melhora rendimento de madeira via skill e reduz desperdício |
| Prospector | passive | 1-5 | melhora chance de minério extra/raro em nodes |
| Pescador Paciente | passive | 1-5 | melhora pesca e chance de espécies raras |
| Cozinha Sustentadora | passive/crafting | 1-5 | melhora comida, buffs, fome, stamina e cansaço |
| Oficina Organizada | passive | 1-5 | reduz custo/desperdício em crafting básico |
| Ferramentas de Ferro | unlock | 1 | libera crafting/upgrade de ferramentas de Ferro |
| Forja de Aço e Prata | unlock | 1-3 | libera armas/ferramentas de Aço e Prata |
| Trabalho em Mithril | unlock | 1-3 | libera equipamentos avançados leves/fortes |
| Engenharia Bromeciana | utility/crafting | 1-5 | libera máquinas, processadores, mecanismos e itens técnicos |
| Estabilização Rara | endgame | 1-5 | trabalha com materiais perigosos/especiais de forma limitada |
| Capstone: Mestre da Produção | capstone | 3-5 | melhora rendimento, qualidade e crafting avançado sem quebrar economia |

Regras:

```text
Força ajuda a quebrar rochas/árvores com menos esforço, mas yield extra vem de Crafting/Produção, ferramenta, buff ou companion.
Crafting de material superior exige node/tier, recursos e estação adequada.
Crafting não deve liberar material endgame antes da progressão de caverna/fazenda/cidade.
```

HUD relacionado:

```text
ícone de ferramenta ativa e tier do material
feedback de qualidade/yield extra
marcador de node raro detectado/coletado
menu de crafting com tier bloqueado/desbloqueado
ícones de material: cobre, ferro, aço, prata, mithril, bromeciano, Pedra Negra Estabilizada
prévia de custo/benefício de upgrade
avisos de estação necessária/oficina necessária
```

---

# PARTE L — Sistemas transversais não tratados como árvore inicial

## 36. Social, romance, companions e pets

Esses sistemas são importantes, mas não substituem uma das 5 árvores iniciais.

Direção:

```text
Social/romance/casamento deve existir como sistema próprio.
Companions devem ter afinidade, função e evolução própria.
Pets devem ter vínculo, cuidado e utilidade própria.
Esses sistemas podem receber bônus de Carisma, Vontade, Inteligência, traits raciais, quests, itens e arquétipos.
Eles podem futuramente ganhar árvore própria, mas não devem substituir Melee/Ranged/Magic/Survival/Crafting no modelo inicial.
```

---

# PARTE M — Active slots, capstones e arquétipos

## 37. Active slots

Direção consolidada:

```text
4 active slots como base.
```

Regras:

```text
O jogador não pode equipar todas as skills ativas ao mesmo tempo.
Passivas não ocupam active slot.
Trocar active slots deve exigir menu/descanso/Fonte/fora de combate, conforme decisão futura.
Active skills podem vir de qualquer uma das 5 árvores.
Dash, Block, Disparo de Interrupção, magias e técnicas fortes podem ocupar active slots.
```

HUD:

```text
4 slots visíveis na HUD.
Cada slot mostra ícone, cooldown, custo principal e disponibilidade.
Skills sem recurso suficiente devem aparecer esmaecidas.
Skills bloqueadas por estado, como silêncio/stun/fadiga extrema, devem comunicar motivo.
```

## 38. Capstones

Regras:

```text
Cada árvore tem Tier 5 como capstone.
Capstone recompensa especialização.
Capstone não deve ser obrigatório para zerar o jogo.
Capstone não deve invalidar outras árvores.
Capstone pode desbloquear arquétipos inferidos.
```

## 39. Arquétipos inferidos / jobs vestíveis

O jogo não implementa classes estilo D&D.

Regra:

```text
O jogador não escolhe uma classe permanente.
O jogo observa distribuição de atributos, skills, equipamentos e comportamento.
Com isso, pode inferir arquétipos/jobs funcionais.
O jogador pode vestir/ativar um arquétipo desbloqueado para receber bônus.
```

Arquétipos iniciais possíveis:

| Arquétipo | Origem principal | Bônus sugerido |
|---|---|---|
| Guerreiro | Melee + Força/Constituição | corpo a corpo, block, stagger e defesa |
| Caçador | Ranged + Destreza/Inteligência | precisão, pontos fracos e abertura segura |
| Arcano | Magic + Vontade/Inteligência | custo, MP, magia e Fonte |
| Sobrevivente | Survival + Constituição/Destreza/Vontade | dash, long runs, traps e hazards |
| Produtor | Crafting + Inteligência/Força | ferramentas, yield via skill e qualidade |
| Minerador | Crafting + Força/Constituição | mineração eficiente e menos desgaste |
| Artífice | Crafting + Inteligência | tecnologia, ruínas e equipamentos |
| Devoto de Anya | Magic + Survival + eventos | Fonte, Água Viva, purificação/cura limitada |
| Líder | Carisma + companions/quests | companions/pets e relações, sem ser árvore inicial |
| Mercador | Carisma + Crafting/economia | preços, contratos e reputação econômica |

---

# PARTE N — Ferramentas, armas, magia e equipamentos

## 40. Ferramentas

Ferramentas principais:

```text
enxada
regador
picareta
machado
vara de pesca
foice, se existir
martelo/ferramenta técnica futura
```

Regra:

```text
Ferramenta melhor pode aumentar yield.
Crafting/Produção pode aumentar yield.
Força pode reduzir esforço/golpes, mas não aumenta yield sozinha.
```

## 41. Armas

Categorias possíveis:

```text
Sword
Axe
Hammer
Spear
Bow
Dagger
Staff
ToolAttack
```

Armas se conectam a:

```text
atributos
Melee/Ranged/Magic
vulnerabilidades de monstros
critical windows
stamina/breath
magia/equipamentos
material/tier de crafting
```

## 42. Magia

Magia usa MP.

Tipos iniciais possíveis:

```text
ofensiva
defensiva
suporte
cura limitada
utilidade
controle leve
interação com ambiente
```

Regra:

```text
Magia não deve resolver todos os sistemas sozinha.
Ela deve ter custo, limite, build e counterplay.
```

## 43. Equipamentos

Slots possíveis:

```text
arma
ferramenta ativa
armadura/roupa
acessório 1
acessório 2
anel/amuletos futuros
trinket/lore item futuro
```

Equipamentos podem dar:

```text
atributos
resistências
skill bonus
redução de custo
bônus de crit window
bônus social/econômico
bônus de pet/companion
regen HP/MP específica, se rara e balanceada
amplificador de HP regen quando skill apropriada existe
amplificador de MP regen quando Magic/Vontade sustentam
```

---

# PARTE O — Resistências e status negativos

## 44. Resistências

Resistências principais:

```text
Physical
Fire
Ice
Lightning
Water
Poison
Bleed
Shadow/Nyx
Arcane
Blackstone/Corruption
Fear/Mental
Heat
Cold
```

Fontes:

```text
Constituição
Vontade
equipamentos
comida
skills
arquétipos
companions/pets
Fonte de Anya
```

## 45. Status negativos

Status possíveis:

```text
Burn
Poison
Bleed
Slow
Stun
Chill
Root
Fear
ConfusionLite
DurabilityStress
Hunger
Fatigue
HeatStress
ColdStress
Corruption
```

Regra:

```text
Status forte precisa de telegraph, duração curta ou counterplay.
ConfusionLite não tira controle total do jogador.
DurabilityStress não destrói item permanentemente sem direção específica.
```

---

# PARTE P — Morte, derrota e Fonte de Anya

## 46. Derrota

Derrota deve ser integrada a:

```text
caverna
perda/risco de itens
corpse recovery
Fonte de Anya
companions
pets
checkpoints
cansaço
```

Regra:

```text
Morte/derrota deve ter consequência, mas não apagar progresso de forma injusta.
```

## 47. Fonte de Anya

A Fonte de Anya é eixo de:

```text
retorno/ressurreição
respec
cura especial
Água Viva
lore de Anya
eventos de nível 101
progressão espiritual
```

Regras já consolidadas:

```text
Anya não tem altar construível na fazenda.
A Fonte é a representação física ativa de Anya na fazenda.
A libertação parcial do poder de Anya ocorre por conteúdo profundo da caverna/nível 101.
```

---

# PARTE Q — Companions, pets, fazenda, cidade e caverna

## 48. Companions

Companions podem ajudar em:

```text
combate
fazenda
mineração
suporte
cura limitada
controle
quests
relacionamentos
lore
```

Não devem:

```text
substituir o jogador
resolver boss sozinho
invalidar pet
invalidar build do personagem
```

## 49. Pets

Pets são sistema próprio, separado de companion.

```text
Cachorro pode apoiar combate, detectar traps e ajudar contra swarms.
Gato pode apoiar sorte, detecção de segredo/anomalia e vínculo social/fazenda.
```

Cachorro não ocupa slot de companion.

## 50. Relação com fazenda

O personagem se conecta à fazenda por:

```text
stamina
fome
cansaço
ferramentas
Crafting/Produção
Survival/Sobrevivente
pets
companions trabalhando
construções
Fruto de Mana
Fonte de Anya
```

## 51. Relação com cidade

O personagem se conecta à cidade por:

```text
reputação
relacionamentos
amizade
flerte
casamento
serviços
lojas
festivais
contratos
visitas de NPCs à fazenda
Carisma
arquétipos sociais inferidos
```

## 52. Relação com caverna

O personagem se conecta à caverna por:

```text
HP
MP
Stamina
Breath
armas
magia
resistências
Melee/Guerreiro
Ranged/Caçador
Magic/Arcano
Survival/Sobrevivente
Crafting/Produção para mineração/coleta
companions
pets
critical windows
traps
boss gates
checkpoints
morte/retorno
```

---

# PARTE R — Flerte, relacionamento e casamento

## 53. Relacionamentos

Relacionamentos consideram:

```text
amizade
reputação
quests pessoais
presentes
conversas
eventos
festivais
visitas à fazenda
compatibilidade narrativa
```

## 54. Flerte

Regras:

```text
Flerte deve ser opção explícita.
NPCs casados não são candidatos a casamento.
NPCs disponíveis podem aceitar romance com jogador homem ou mulher, conforme design de personagem.
Romance não deve depender só de gifts repetidos.
Romance deve ter eventos, escolhas, quests e limites claros.
```

## 55. Casamento

Casamento pode desbloquear:

```text
rotina do cônjuge
mudança para fazenda ou rotina híbrida
cama casal
eventos de casal
ajuda leve na fazenda
bônus emocional/social
quests pós-casamento
```

Não deve:

```text
transformar NPC em ferramenta de produção sem personalidade
apagar rotina/socialidade do NPC
quebrar serviços essenciais da cidade
```

---

# PARTE S — UI/HUD e feedback

## 56. HUD base do personagem

HUD deve mostrar claramente:

```text
HP
MP
Stamina
Breath
Fome
Cansaço
hotbar
4 active slots
status negativos
buffs
arma/ferramenta ativa
pet/companion status quando relevante
```

## 57. Feedback de combate

HUD/feedback de combate deve comunicar:

```text
block disponível/ativo
block perfeito, se existir
stamina/breath drenando durante block
cooldown de Dash
dodge bem-sucedido
critical window no inimigo
hit crítico automático
stagger/posture do inimigo, se visível
alvo marcado por Ranged
ponto fraco revelado
magia preparada/elemento ativo
MP insuficiente
```

## 58. Feedback de sobrevivência

HUD/feedback de sobrevivência deve comunicar:

```text
nível de fome
nível de cansaço
HP regen ativa/inativa
condição para HP regen fora de combate
proteção contra frio/calor/gás
trap detectada
secret room suspeita/descoberta
Breath baixo em corrida/dash/combate
```

## 59. Feedback de produção/crafting

HUD/feedback de produção deve comunicar:

```text
tier da ferramenta ativa
material/tier do item equipado
qualidade do recurso coletado
yield extra por skill/ferramenta
crafting desbloqueado/bloqueado
material faltante
estação necessária
upgrade disponível
risco de usar material corrompido/especial
```

## 60. Menus necessários

Menus necessários:

```text
criação do personagem
raça/gênero/aparência
atributos
5 skill trees
SkillPoints gastos / restantes / máximo 50
pré-requisitos de tier
capstone bloqueado/desbloqueado
arquétipo ativo
equipamentos
relacionamentos
pets
companions
status/resistências
```

---

# PARTE T — Save/load

## 61. Save/load

Save deve persistir:

```text
nome
raça
gênero/apresentação
sprite base
cabelo/cor/opções visuais
level
XP
atributos
HP/MP/Stamina/Breath atuais e máximos
fome
cansaço
SkillPoints ganhos/gastos/restantes
5 skill trees
nodes desbloqueados
skill ranks
active slots
ações equipadas
capstones desbloqueados
arquétipos desbloqueados/ativo
ferramentas
armas
equipamentos
resistências temporárias/permanentes
status ativos
relacionamentos
romances
casamento
pet state
companion state
Fonte de Anya/respec flags
morte/corpse recovery state
```

---

# PARTE U — Roadmap de direção futura

Antes de implementação, ainda precisamos refinar documentos/seções específicas de direção para:

```text
nodes completos das 5 skill trees existentes
custos finais dos nodes
capstones completos das 5 skill trees existentes
balanceamento de 50 SkillPoints
sinergia final com atributos
fórmulas de atributos e stats derivados
curva de XP e level cap
progressão de skill ranks dentro das trees
arquétipos inferidos/jobs vestíveis
ferramentas e upgrades
armas e dano
magia e MP
resistências e status
morte/derrota/Fonte de Anya
criação visual do personagem em sprites
passivas raciais de Humano e Anão
raças futuras
companions e pets integrados ao personagem
flerte, romance e casamento
UI/HUD do personagem
save/load do personagem
```

---

# PARTE V — Decisões fechadas

```text
Raças jogáveis iniciais: Humano e Anão.
Demais raças de Vaalara ficam como futuras/expansíveis.
O jogador não terá classe fixa estilo D&D.
O jogador evolui livremente por atributos, skills, equipamentos, ferramentas, armas e magia.
As 5 skill trees iniciais são: Melee/Guerreiro, Ranged/Caçador, Magic/Arcano, Survival/Sobrevivente e Crafting/Produção.
Social/romance/companions/pets são sistemas transversais, não a quinta árvore inicial.
Cada árvore tem 5 tiers.
Tier 5 é capstone.
O jogador terá no máximo 50 SkillPoints no endgame.
Masterizar uma árvore deve custar aproximadamente 28-32 pontos.
O jogador deve conseguir masterizar uma árvore e investir parcialmente em outras.
O jogador não deve conseguir masterizar duas árvores completas com 50 pontos.
Skills individuais podem ter ranks internos de 1 a 5, se isso não conflitar com tiers.
SkillPoint a cada 2 níveis continua como direção.
4 active slots continua como direção.
Passivas não ocupam active slot.
Dash é skill desbloqueável, provavelmente em Survival/Sobrevivente.
Dodge é ação base, não skill.
Skills podem melhorar dodge aumentando janela de invulnerabilidade/recovery/custo.
Block é skill desbloqueável, provavelmente em Melee/Guerreiro.
HP regen é skill/efeito explícito, não atributo gratuito.
Itens podem amplificar HP regen, mas não substituem a base de skill/efeito.
Crafting/Produção libera tiers de material como Ferro, Aço, Prata, Mithril, Liga Bromeciana e materiais especiais.
Magic/Arcano pode liberar melhorias elementais, espirituais/divinas, cura limitada e uso de itens mágicos.
Respec ocorre na Fonte de Anya.
Classes/jobs serão arquétipos inferidos por distribuição de skills/atributos e poderão ser vestidos/ativados para bônus.
MP existe e é usado por magia.
MP regenera naturalmente de forma lenta baseada principalmente em Vontade.
Breath/Fôlego é stat separado de Stamina.
Cansaço é sistema próprio e afeta PlayerConditionManager.
Fome pressiona planejamento, mas não deve ser morte punitiva por padrão.
Força não aumenta yield de recurso sozinha; ela reduz esforço/golpes/dificuldade física.
Constituição não concede regeneração passiva de HP sozinha.
Pets são separados de companions.
Cachorro pode apoiar combate sem ocupar slot de companion.
Flerte/casamento fazem parte dos sistemas do personagem/social.
Fonte de Anya conecta morte, respec, cura especial e progressão de lore.
Criação do personagem terá raça, gênero/apresentação e aparência limitada por sprites.
Raça/gênero/aparência não devem bloquear build, romance ou conteúdo central.
HUD precisa comunicar HP, MP, Stamina, Breath, Fome, Cansaço, active slots, Dash, Block, Dodge feedback, regen, skill costs, crafting tiers e critical windows.
```

---

# PARTE W — Pendências

```text
Validar no repo os nomes exatos de classes/arquivos/assets das skill trees implementadas.
Validar lista final de nodes já implementados contra este documento.
Definir passivas raciais finais de Humano e Anão.
Definir se gênero e pronome serão campos separados.
Definir sprites base finais de Humano e Anão.
Definir paleta final de cabelo/pele/barba.
Definir fórmulas finais de HP/MP/Stamina/Breath.
Definir curva de XP e level cap.
Definir custos finais de cada node.
Definir progressão final de skill rank vs SkillPoint.
Definir nodes completos de cada uma das 5 skill trees.
Definir capstones completos das 5 skill trees.
Definir quantos arquétipos/jobs podem ficar ativos ao mesmo tempo.
Definir se troca de active slots/jobs ocorre na Fonte, casa, menu ou checkpoint.
Definir dano base por arma/ferramenta/magia.
Definir sistema de romance/casamento por NPC.
Definir relação entre cônjuge, companion e NPC de serviço.
Definir contratos concretos de save/load.
