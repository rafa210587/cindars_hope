# Cindar's Hope — Player Skill Trees Direction

> **Status:** documento canônico de direção detalhada das skill trees do personagem  
> **Local:** `docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md`  
> **Complementa:** `docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md`  
> **Não é spec implementável.** Este documento define direção de design, mecânica, progressão, HUD e balanceamento das skills.

---

## 0. Decisão canônica: sem Breath/Fôlego

`Breath` / `Fôlego` foi removido do sistema.

```text
Não existe como atributo.
Não existe como barra.
Não existe como custo.
Não ocupa HUD.
Não aparece em skill tree.
Não aparece em save/load.
```

Distribuição das funções antigas:

```text
Stamina: custo direto de ações físicas.
Constituição: tolerância física, HP, estabilidade, resistência e menor cansaço.
Destreza: movimento, dodge, dash, timing e crítico condicional.
Vontade: resistência mental/espiritual, MP e MP Regen lenta.
Survival: fome, cansaço, runs, ambiente, Dash/Dodge melhorados.
Melee: Block, postura, stagger e defesa ativa.
```

---

# PARTE A — Regras globais das skill trees

## 1. Árvores canônicas

```text
1. Melee / Guerreiro
2. Ranged / Caçador
3. Magic / Arcano
4. Survival / Sobrevivente
5. Crafting / Produção
```

Sistemas sociais, romance, companions e pets são transversais neste momento.

## 2. Economia de SkillPoints

```text
SkillPoints máximos no endgame: 50.
SkillPoint: 1 ponto a cada 2 níveis, salvo ajuste futuro de curva.
Masterização funcional de uma árvore: ~30-34 pontos.
Completar tudo de uma árvore: ~40-46 pontos.
Com 50 pontos: 1 árvore principal masterizada + investimento parcial em outras.
Com 50 pontos: não deve ser possível masterizar duas árvores completas.
```

## 3. Tiers

| Tier | Pontos gastos na árvore | Papel |
|---:|---:|---|
| Tier 1 | 0 | fundamentos, ações básicas e passivas leves |
| Tier 2 | 5 | eficiência, conforto e desbloqueios de rotina |
| Tier 3 | 11 | técnicas que mudam a forma de jogar |
| Tier 4 | 18 | especialização forte e sinergias |
| Tier 5 | 26 | capstone e identidade final da árvore |

## 4. Rank cap por tier

| Profundidade atual | Rank máximo recomendado |
|---|---:|
| Tier 1 | Rank 2 |
| Tier 2 | Rank 3 |
| Tier 3 | Rank 4 |
| Tier 4 ou 5 | Rank 5 |

Regra:

```text
Uma skill pode ser aprendida cedo, mas seus ranks altos exigem investimento real na árvore.
Dash base não depende da árvore; Survival melhora Dash.
Dash longo/endgame depende de investimento real em Survival, gear leve/buffs ou efeitos específicos.
```

## 5. Active slots e input separado

```text
4 active slots são para habilidades equipáveis.
Dash não ocupa active slot.
Dodge não ocupa active slot.
Block não ocupa active slot.
Dash = Space + direção.
Dodge = double tap direcional.
Block = Left Shift.
```

Direção:

```text
Dodge existe desde o começo como ação base.
Dash deve ser desbloqueado por tutorial/progressão inicial.
Survival melhora Dash.
Block é desbloqueado/melhorado por Melee/Guerreiro.
```

## 6. Capstones divinos e exclusão mútua

```text
Capstones podem representar afinidade mecânica/narrativa com forças de Vaalara.
Capstone divino não transforma automaticamente o jogador em clérigo/devoto formal.
Quando houver duas opções exclusivas, escolher uma bloqueia a outra até respec na Fonte de Anya.
```

## 7. Miss, precisão e crítico

```text
Chance de miss existe, mas é superficial.
Skills de precisão pura devem ser evitadas.
“Ponto fraco” não exige mirar em parte específica do corpo.
Mecanicamente, ponto fraco vira abertura, vulnerabilidade, critical window, alvo marcado ou estado exposto.
```

---

# PARTE B — Melee / Guerreiro

## 8. Identidade

Foco:

```text
corpo a corpo
Block
postura/stagger
ataques pesados
controle de swarms próximos
crítico por abertura
sobrevivência em proximidade
escolha final entre Kanthor e Kaand
```

Sinergia:

```text
Força: dano pesado, stagger, armas grandes.
Constituição: HP, defesa, estabilidade, resistência física.
Destreza: timing, dodge follow-up, crítico por abertura.
```

## 9. Skills

| Skill | Tier | Ranks | Tipo | Mecânica in-game | HUD/Feedback |
|---|---:|---:|---|---|---|
| Treinamento Marcial | 1 | 1-5 | Passive | +2/4/6/8/10% dano com armas melee. | bônus no painel da arma |
| Ataque Pesado | 1 | 1-5 | Modifier | Charged attack causa +10/18/26/34/42% posture damage e +5/10/15/20/25% dano, com custo maior de Stamina. | charge na arma |
| Block / Bloqueio | 1 | 1-5 | Defensive Action | Habilita Block no rank 1. Usa Left Shift. Reduz dano frontal em 35/42/49/56/63%. Drena Stamina ao segurar e ao receber impacto. | escudo + dreno de Stamina |
| Guarda Firme | 2 | 1-5 | Passive | Reduz custo de Stamina do Block em 6/12/18/24/30% e reduz stagger recebido bloqueando. | escudo reforçado |
| Corte Amplo | 2 | 1-3 | Active Skill | Ataque em arco curto. Atinge até 2/3/4 inimigos próximos com dano reduzido nos secundários. | active slot + cooldown |
| Contra-Ataque | 3 | 1-3 | Modifier | Após block perfeito, próximo ataque melee em até 1.5s ganha +20/35/50% crit chance. | flash de contra-ataque |
| Quebra-Postura | 3 | 1-5 | Passive | +8/16/24/32/40% posture damage contra inimigos em telegraph/recovery/ataque pesado. | posture feedback |
| Lâmina de Abertura | 3 | 1-5 | Passive | Ataques melee contra vulneráveis/stunned/slowed/critical window ganham +5/10/15/20/25% crit chance. | brilho de abertura |
| Especialização: Arma Pesada | 3 | 1-5 | Passive | Hammer/Axe/Great Sword: +6/12/18/24/30% posture damage; +3/6/9/12/15% custo de Stamina. | marcador de especialização |
| Especialização: Arma Leve | 3 | 1-5 | Passive | Dagger/Sword/Spear curta: -4/8/12/16/20% recovery; +2/4/6/8/10% crit contra alvo marcado/vulnerável. | marcador de especialização |
| Pele de Batalha | 4 | 1-5 | Passive | Enquanto HP > 60%, reduz dano físico recebido em 3/6/9/12/15%. Não regenera HP. | buff passivo |
| Golpe de Ruptura | 4 | 1-3 | Active Skill | Golpe forte com alto posture damage e pequeno knockback. Custo alto, cooldown médio. | impacto forte |
| Capstone: Voto do Aço Profundo de Kanthor | 5 | 3 | Exclusive Capstone | Ao bloquear perfeitamente ou quebrar postura, ativa “Julgamento de Aço” por 8s: +15% dano melee, +25% Block Stability, +20% resistência a stagger; próximo hit em critical window cura 3% HP máximo ou recupera pequena Stamina. Bloqueia Kaand. | aura dourada/azul |
| Capstone: Voto do Aço Profundo de Kaand | 5 | 3 | Exclusive Capstone | Ao quebrar postura ou acertar crítico melee, ativa “Fúria de Aço” por 6s: +30% dano melee, +25% Crit Damage, +20% Stagger Power; Block Power cai 15% durante o efeito. Bloqueia Kanthor. | aura rubra |

---

# PARTE C — Ranged / Caçador

## 10. Identidade

Foco:

```text
arco/projéteis
controle de distância
marcação de alvo
crítico por abertura
interrupção
loot de caça
Caça das Três Luas
```

Sinergia:

```text
Destreza: velocidade, crit condicional, mobilidade.
Inteligência: leitura de padrões, marcação, armadilhas simples.
Vontade: foco sob medo/pressão e resistência mental.
```

## 11. Skills

| Skill | Tier | Ranks | Tipo | Mecânica in-game | HUD/Feedback |
|---|---:|---:|---|---|---|
| Treino de Arco | 1 | 1-5 | Passive | +2/4/6/8/10% dano com armas ranged. | bônus na arma |
| Marcador de Presa | 1 | 1-3 | Active Skill | Marca 1 alvo por 8/10/12s. Jogador e pet/companion causam +5/8/12% dano nele. | ícone no alvo |
| Disparo Carregado | 1 | 1-5 | Active Skill | Tiro com windup. +15/25/35/45/55% dano e +10/15/20/25/30% chance de stagger/interrupção leve em inimigos menores. | charge |
| Passo do Caçador | 2 | 1-5 | Passive | Após ataque ranged, próximo movimento em 0.8s recebe +4/8/12/16/20% velocidade curta. Não é Dash. | rastro nos pés |
| Leitura de Abertura | 2 | 1-5 | Passive | Contra alvo marcado, slowed, stunned ou em recovery: +4/8/12/16/20% crit chance. | brilho no alvo |
| Disparo de Interrupção | 3 | 1-3 | Active Skill | Interrompe casts fracos/ataques canalizados de não-boss. Em boss, reduz posture em 6/9/12%. | active slot |
| Flecha Perfurante | 3 | 1-3 | Active Skill | Projétil atravessa até 2/3/4 alvos em linha curta. Dano cai 20% por alvo atravessado. | linha de projétil |
| Armadilha de Caçador | 3 | 1-3 | Active Utility | Trap simples de slow/root curto por 1.0/1.5/2.0s. | trap no chão |
| Caçador de Voadores | 3 | 1-5 | Passive | Contra flying/floating: +4/8/12/16/20% dano e +5/10/15/20/25% posture damage. | ícone de asa |
| Flecha Preparada | 4 | 1-5 | Modifier/Unlock | Permite aplicar consumíveis simples à flecha: fogo/gelo/veneno leve. Consome item/carga. | munição preparada |
| Retirada Tática | 4 | 1-3 | Passive | Ao acertar inimigo marcado que se aproxima, 20/35/50% de chance de slow curto. Cooldown interno. | efeito de slow |
| Olho do Caçador | 4 | 1-5 | Passive | +3/6/9/12/15% chance de loot de caça adicional em criaturas orgânicas. | loot extra |
| Capstone: Marca da Caça das Três Luas | 5 | 3 | Capstone | Primeiro alvo marcado no encontro recebe “Marca das Três Luas”: +15% crit chance ranged contra ele. Primeiro crítico causa +45% Crit Damage e espalha marca menor para 1 inimigo próximo. Se morrer marcado, recupera pequena Stamina. | três luas na marca |

---

# PARTE D — Magic / Arcano

## 12. Identidade

Foco:

```text
MP
regen lenta de MP
magias elementais
cura limitada
barreiras/selos
purificação/corrupção
itens mágicos
Fruto de Mana
Fonte de Anya
escolha final entre Anya e Senya
```

## 13. Skills

| Skill | Tier | Ranks | Tipo | Mecânica in-game | HUD/Feedback |
|---|---:|---:|---|---|---|
| Canalização Serena | 1 | 1-5 | Passive | Reduz custo de MP de magias básicas em 4/8/12/16/20%. | custo menor |
| Foco Arcano | 1 | 1-5 | Passive | +3/6/9/12/15% dano/efeito de magias ofensivas e controle leve. | bônus mágico |
| Projétil Arcano | 1 | 1-5 | Active Skill | Magia básica de dano arcano. Custo baixo, dano médio. | active slot + MP |
| Fluxo Lento | 2 | 1-5 | Passive | MP Regen natural aumenta em +5/10/15/20/25% sobre a base de Vontade. Continua lenta. | regen discreta |
| Selo de Proteção | 2 | 1-3 | Active Skill | Barreira curta por 2/3/4s. Não bloqueia tudo. Custo médio/alto. | ward visual |
| Afinidade Elemental: Fogo | 2 | 1-5 | Modifier | Magias de fogo: +4/8/12/16/20% Burn/dano de fogo. | ícone fogo |
| Afinidade Elemental: Gelo | 2 | 1-5 | Modifier | Magias de gelo: +4/8/12/16/20% Chill/slow, sem stun abusivo. | ícone gelo |
| Afinidade Elemental: Raio | 3 | 1-5 | Modifier | Magias de raio: +4/8/12/16/20% chance de interrupção leve; bônus contra constructos/máquinas. | ícone raio |
| Afinidade Natural/Água | 3 | 1-5 | Modifier | Magias de água/natureza: melhora purificação leve, suporte e interação ambiental. | ícone água/folha |
| Toque Restaurador | 3 | 1-5 | Active Skill | Cura 8/12/16/20/24% do HP máximo. Custo alto, cooldown alto. | cura |
| Uso de Item Mágico | 3 | 1-5 | Unlock/Passive | Permite usar varinhas, selos, scrolls, amuletos e cargas mágicas de tiers maiores. | item mágico |
| Resistir Corrupção | 4 | 1-5 | Passive | Reduz efeito/duração de Corruption, Shadow/Nyx, Void e Blackstone em 5/10/15/20/25%. | resistência espiritual |
| Eco da Fonte | 4 | 1-5 | Passive/Lore | Melhora efeitos da Fonte de Anya, Água Viva, respec e cura especial após marcos narrativos. | HUD da Fonte |
| Capstone: Semente Arcana de Anya | 5 | 3 | Exclusive Capstone | Ao gastar 35% do MP máximo em sequência, cria “Semente de Anya” por 10s. Próxima magia espiritual/suporte custa -50% MP, cura/barreira/purificação +35%, e deixa eco de regen leve por 4s. Bloqueia Senya. | semente branca/azul |
| Capstone: Semente Arcana de Senya | 5 | 3 | Exclusive Capstone | Ao gastar 35% do MP máximo em sequência, cria “Semente de Senya” por 8s. Próxima magia ofensiva causa +35% Magic Damage, efeito elemental amplificado e +15% chance de crit mágico/overload. Custo +10% MP. Bloqueia Anya. | semente violeta/dourada |

---

# PARTE E — Survival / Sobrevivente

## 14. Identidade

Foco:

```text
cansar menos
sentir menos fome
runs mais longas
resistência ambiental e mental
loot/ouro/tesouros em caverna
mineração de run
melhorar Dash
melhorar Dodge
HP Regen fora de combate
Último Fôlego de Telisandra como nome de capstone, não como atributo Breath
```

## 15. Skills

| Skill | Tier | Ranks | Tipo | Mecânica in-game | HUD/Feedback |
|---|---:|---:|---|---|---|
| Estômago Forte | 1 | 1-5 | Passive | Reduz perda de fome em runs/caverna em 4/8/12/16/20%. | fome |
| Ritmo de Jornada | 1 | 1-5 | Passive | Reduz ganho de cansaço por movimento/exploração em 4/8/12/16/20%. | cansaço reduzido |
| Reflexo de Esquiva | 1 | 1-5 | Passive | Dodge continua base. Aumenta i-frame/reduz recovery em valores pequenos por rank. | dodge perfeito |
| Passo de Impulso | 2 | 1-5 | Movement Modifier | Melhora Dash já desbloqueado. Ranks baixos reduzem custo/cooldown/recovery e aumentam distância de forma moderada; ranks altos podem transformar Dash em mobilidade longa de build, respeitando o cap global de ~8 tiles definido no Combat Core. Dash longo não ganha i-frame relevante por padrão. | indicador de Dash |
| Ritmo Controlado | 2 | 1-5 | Passive | Reduz custo de Stamina de Dash, Dodge e corrida em 5/10/15/20/25%, e reduz cansaço gerado por essas ações. Substitui Respiração Controlada. | Stamina/cansaço |
| Saqueador Cuidadoso | 2 | 1-5 | Passive | +3/6/9/12/15% chance de ouro adicional em drops e tesouros. | ouro extra |
| Garimpo de Run | 2 | 1-5 | Passive | Nodes de minério na caverna têm +3/6/9/12/15% chance de minério extra. | minério extra |
| Descanso Curto | 3 | 1-5 | Utility/Passive | Fora de combate por X segundos, recupera pequena fração de Stamina e reduz cansaço levemente. | descanso válido |
| Regeneração Natural | 3 | 1-5 | Passive | HP Regen lenta e limitada fora de combate. Pausa ao tomar dano. Itens amplificam. | regen ativa |
| Resistência Ambiental | 3 | 1-5 | Passive | Reduz penalidades de frio, calor, gás, gelo ambiental e hazards em 5/10/15/20/25%. | proteção ambiental |
| Faro de Tesouro | 4 | 1-5 | Passive | Baús/tesouros já gerados têm +2/4/6/8/10% chance de rolar raridade superior. | brilho no baú |
| Mente Inabalável | 4 | 1-5 | Passive | Reduz duração/efeito de Fear, ConfusionLite, Nyx/Void leve em 5/10/15/20/25%. | ícone mental |
| Resistência de Run Profunda | 4 | 1-3 | Passive | Em run de caverna, após 10/20/30 min in-game, reduz penalidades de fome/cansaço em valor moderado. | buff de run |
| Capstone: Último Fôlego de Telisandra | 5 | 3 | Capstone | Uma vez por run, ao entrar em HP baixo, Stamina crítica ou cansaço alto, ativa “Fôlego de Telisandra” por 10s: -45% gasto de Stamina em Dash/Dodge/corrida, +30% resistência ambiental/mental, Dodge ganha pequena janela extra e HP Regen fora de combate dobra após sair do perigo. | brisa prateada/azulada |

---

# PARTE F — Crafting / Produção

## 16. Identidade

Foco:

```text
reduzir custo de ações agrícolas
reduzir cansaço indireto por menor gasto de Stamina
melhorar qualidade/yield produtivo
reduzir custo de construção/crafting
liberar tiers de material
armas/ferramentas melhores
cozinha, buffs e consumíveis
máquinas/processadores/oficinas
Forja Viva de Thoren
```

## 17. Skills

| Skill | Tier | Ranks | Tipo | Mecânica in-game | HUD/Feedback |
|---|---:|---:|---|---|---|
| Mãos de Lavrador | 1 | 1-5 | Passive | Reduz custo de Stamina ao arar/regar/plantar em 5/10/15/20/25%. | custo menor |
| Coleta Eficiente | 1 | 1-5 | Passive | +3/6/9/12/15% chance de recurso extra em colheita, madeira comum e coleta simples. | recurso extra |
| Lenhador Prático | 1 | 1-5 | Passive | Árvores/troncos exigem menos golpes e pequena chance de madeira extra. | golpes reduzidos |
| Prospector de Superfície | 1 | 1-5 | Passive | Mineração comum/baixa: +3/6/9/12/15% chance de minério extra. | minério extra |
| Cozinha Sustentadora | 2 | 1-5 | Passive/Crafting | Comidas recuperam +5/10/15/20/25% fome/Stamina ou reduzem cansaço em valor pequeno. | comida melhorada |
| Oficina Organizada | 2 | 1-5 | Passive | Reduz custo de construção/crafting estrutural em 3/6/9/12/15%. | custo reduzido |
| Ferramentas de Ferro | 2 | 1 | Unlock | Libera ferramentas de Ferro. | tier liberado |
| Ferramentas de Aço | 3 | 1 | Unlock | Libera ferramentas de Aço. | tier liberado |
| Forja de Prata | 3 | 1-3 | Unlock/Modifier | Libera armas de Prata. Ranks melhoram dano contra mortos-vivos/sombras/maldições em +5/10/15%. | ícone prata |
| Alquimia Prática | 3 | 1-5 | Crafting | Melhora poções, antídotos, bombas leves, óleos de arma e consumíveis de run. | consumível melhor |
| Trabalho em Mithril | 4 | 1-3 | Unlock/Modifier | Libera armas/armaduras/ferramentas de Mithril. Equipamentos tendem a ser leves, duráveis e com menor custo de Stamina. | tier mithril |
| Engenharia Bromeciana | 4 | 1-5 | Unlock/Crafting | Libera máquinas, processadores, irrigação avançada, mecanismos e itens técnicos por rank. | UI de máquina |
| Cultivo de Mana | 4 | 1-5 | Lore/Crafting | Aumenta chance de cultivar/estabilizar crops raras ligadas a Mana, sem garantir Fruto de Mana. | crop rara |
| Capstone: Forja Viva de Thoren | 5 | 3 | Capstone | Uma vez por dia, ao craftar/construir/colher lote relevante, recupera material comum, melhora qualidade ou gera output extra moderado. Em armas/ferramentas, chance baixa de propriedade temporária de durabilidade/eficiência. | bigorna luminosa |

---

# PARTE G — HUD e UI

## 18. HUD

Mostrar:

```text
HP
MP quando relevante
Stamina
Fome
Cansaço
4 active slots
hotbar
Dash cooldown
Dodge feedback
Block state
status negativos
critical window
```

Não mostrar:

```text
Breath
Fôlego
BR
```

## 19. Menu de skill tree

Mostrar:

```text
50 SkillPoints máximos
pontos restantes
pontos gastos por árvore
tier desbloqueado
rank atual
rank máximo permitido
pré-requisitos
active slot sim/não
tipo do node
capstone exclusivo e opção bloqueada
respec na Fonte de Anya
```

---

# PARTE H — Decisões fechadas

```text
Breath/Fôlego removido das skill trees.
Respiração Controlada foi substituída por Ritmo Controlado.
Dash custa Stamina.
Dodge custa Stamina.
Block drena Stamina.
Dash base não ocupa active slot e é melhorado por Survival/Passo de Impulso.
Dash longo/endgame pode chegar até ~8 tiles com cap global do Combat Core.
Dash longo não ganha i-frame relevante por padrão.
Constituição, Destreza, Vontade, Survival e Melee absorvem as funções antigas de Breath.
Melee tem capstones exclusivos de Kanthor e Kaand.
Magic tem capstones exclusivos de Anya e Senya.
Ranged usa Marca da Caça das Três Luas.
Survival usa Último Fôlego de Telisandra como nome narrativo de capstone, não como stat.
Crafting usa Forja Viva de Thoren.
```

---

# PARTE I — Pendências

```text
Definir custos finais de Stamina para Dash, Dodge, Block e corrida.
Definir Stamina Regen em combate, fora de combate, com fome e com cansaço.
Definir valores finais de cooldown, custo e duração.
Definir progressão final de Passo de Impulso para Dash base, Dash longo e cap de ~8 tiles.
Validar integração com HUD final.
Validar visual em 1280x720 e tela pequena.
```
