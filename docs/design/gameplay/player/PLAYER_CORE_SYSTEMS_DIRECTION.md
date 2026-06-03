# Cindar's Hope — Player Core Systems Direction

> **Status:** documento canônico de direção ampla dos sistemas centrais do personagem  
> **Local:** `docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/SPEC_SOURCE_MAP.md`  
> - `docs/design/SPECIFICATION_PROCESS.md`  
> - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`  
> - `docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md`  
> - `docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md`  
> **Função:** consolidar a direção ampla do personagem jogável: criação inicial, atributos, HP, MP, Stamina, fome, cansaço, level up, skill trees, input de movimento, active slots, arquétipos inferidos, ferramentas, armas, magia, equipamentos, resistências, companions, pets, fazenda, cidade, caverna, UI/HUD e save/load.  
> **Não é spec implementável.** Este documento descreve como o sistema deve funcionar em visão de jogo.

---

## 0. Decisão canônica: Breath/Fôlego removido

`Breath` / `Fôlego` foi removido do core do personagem.

Não existe como:

```text
atributo central
atributo derivado
barra de HUD
custo de ação
campo de save/load
stat de monstro
```

Substituições:

```text
Stamina = recurso físico imediato gasto em ações.
Cansaço = desgaste acumulado de longo prazo.
Constituição = HP, Stamina, estabilidade, resistência e tolerância física.
Destreza = movimento, dodge, dash, timing, attack speed e crítico condicional.
Vontade = MP, MP Regen lenta, resistência mental/espiritual e pressão mágica.
Survival = runs longas, fome, cansaço, ambiente, Dash/Dodge melhorados.
Melee = Block, postura, stagger e defesa ativa.
Traits de monstro = movimento, perseguição, recuperação e pressão.
```

---

# PARTE A — Regras centrais

## 1. Direção geral

```text
O jogador não possui classe fixa estilo D&D.
O jogador evolui por atributos, skills, equipamentos, armas, ferramentas, magia e escolhas de gameplay.
O jogo usa 5 skill trees iniciais: Melee/Guerreiro, Ranged/Caçador, Magic/Arcano, Survival/Sobrevivente e Crafting/Produção.
Social, companions e pets são sistemas transversais.
Dash/Dodge/Block não ocupam os 4 active slots.
Active slots são reservados para habilidades equipáveis de combate, magia, suporte e utilidade.
O jogo pode inferir arquétipos/jobs funcionais a partir da distribuição de skills, atributos e equipamentos.
```

Detalhes canônicos:

```text
docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md
docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_TABLETOP_EXAMPLE.md
```

---

# PARTE B — Criação inicial do personagem

## 2. Escolhas iniciais

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
A customização deve ser limitada e sustentável para spritesheets.
```

## 3. Raças jogáveis iniciais

| Raça | Nome no jogo | Direção visual | Papel de gameplay |
|---|---|---|---|
| Humano de Dornécia | Humano | silhueta padrão 32x48 | versátil, social, sem especialização extrema |
| Anão de Khaz Baruk | Anão | corpo baixo/largo, barba/cabelo forte | mineração, Constituição, Força, ferramentas e resistência |

Raças futuras:

```text
Elfo da Noite / Luandil
Halfling
Tiefling
Meio-Orc
Draconato
Goblin
Gnomorin
Drow / linhagens sombrias
Nymirianos
```

---

# PARTE C — Atributos centrais

## 4. Atributos canônicos

```text
Força
Constituição
Destreza
Inteligência
Vontade
Carisma
```

Não existe:

```text
Breath
Fôlego
BR
```

## 5. Progressão de atributos

```text
No level 1, o jogador começa com 1 ponto em cada atributo principal.
A cada level up, o jogador ganha +1 ponto para distribuir em um atributo principal.
```

## 6. Função dos atributos

### Força

```text
dano físico pesado
stagger/knockback
ataques pesados
facilidade para quebrar rochas/troncos/obstáculos
não aumenta yield de recurso sozinha
```

### Constituição

```text
HP máximo
Stamina máxima
Stamina Regen em menor grau
resistência física
estabilidade de Block
resistência a stagger/knockdown
tolerância a cansaço
não gera HP Regen sozinha
```

### Destreza

```text
movimento
dodge
dash
attack speed
recovery
crítico condicional
timing
```

### Inteligência

```text
magia técnica
crafting avançado
leitura de monstros/traps/recursos
tecnologia bromeciana
ruínas/mecanismos
```

### Vontade

```text
MP máximo
MP Regen lenta
resistência mental/espiritual
resistência a corrupção/Nyx/Void
magias espirituais
Fonte de Anya
```

### Carisma

```text
relações sociais
reputação
preços/negociação futura
companions
pets
eventos sociais
```

---

# PARTE D — Recursos principais

## 7. HP

```text
Representa vida física.
Fonte principal: Constituição.
HP Regen não vem de Constituição pura.
HP Regen vem de skill, item, comida, magia, Fonte ou efeito explícito.
```

## 8. MP

```text
Representa energia mágica.
Fonte principal: Vontade.
Fonte secundária: Inteligência.
MP Regen natural é lenta e baseada principalmente em Vontade.
```

## 9. Stamina

```text
Representa energia física imediata.
É o único recurso físico gasto diretamente em ações.
```

Gasta em:

```text
ferramentas
mineração
corte de madeira
plantio/rega/colheita
pesca
ataques físicos
ataques carregados
Dash
Dodge
Block
corrida
```

## 10. Fome

```text
Reduz Stamina Regen.
Acelera cansaço.
Pressiona alimentação, cozinha, fazenda e planejamento.
Não deve matar o jogador de forma punitiva no design base.
```

## 11. Cansaço

```text
Representa desgaste acumulado.
Aumenta com tempo, gasto de Stamina, fome baixa, combate, ações pesadas, ambiente hostil e long runs.
Reduz recuperação, eficiência, movimento e segurança em combate quando alto.
É reduzido por sono, descanso, comida, Survival e alguns efeitos.
```

---

# PARTE E — Input de movimento e defesa

## 12. Dash

```text
Input: Space + direção.
Desbloqueio: tutorial/progressão inicial.
Custo: Stamina.
Não ocupa active slot.
Survival melhora custo, cooldown, recovery e distância.
```

## 13. Dodge

```text
Input: double tap direcional.
Existe desde o começo.
Custo: Stamina.
Não ocupa active slot.
Destreza e Survival melhoram janela/recovery/custo.
```

## 14. Block

```text
Input: Left Shift.
Desbloqueado/melhorado por Melee/Guerreiro.
Drena Stamina ao segurar e ao receber impacto.
Não ocupa active slot.
Constituição, escudo/equipamento e Melee melhoram Block Power/Stability.
```

---

# PARTE F — Skill trees

```text
Melee / Guerreiro
Ranged / Caçador
Magic / Arcano
Survival / Sobrevivente
Crafting / Produção
```

Regras:

```text
50 SkillPoints máximos.
5 tiers por árvore.
Tier 5 = capstone.
Capstones podem ser divinos e exclusivos.
Passivas não ocupam active slot.
4 active slots para habilidades equipáveis.
Dash/Dodge/Block não ocupam active slot.
Respec ocorre na Fonte de Anya.
```

---

# PARTE G — Ferramentas, armas, magia e equipamentos

## 15. Ferramentas

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
Força reduz esforço/golpes, mas não aumenta yield sozinha.
```

## 16. Armas

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

Conectam-se a:

```text
atributos
Melee/Ranged/Magic
vulnerabilidades de monstros
critical windows
Stamina
material/tier de crafting
```

## 17. Magia

```text
Magia usa MP.
Pode ser ofensiva, defensiva, suporte, cura limitada, utilidade, controle leve e interação ambiental.
```

---

# PARTE H — Resistências e status

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

---

# PARTE I — Sistemas conectados

## 18. Fonte de Anya

```text
retorno/ressurreição sistêmica
respec
cura especial
Água Viva
lore de Anya
eventos de nível 101
progressão espiritual
```

## 19. Companions e pets

```text
Companions podem ajudar em combate, fazenda, mineração, suporte, controle, quests e lore.
Pets são sistema próprio.
Cachorro pode apoiar combate e detecção de perigos.
Gato pode apoiar sorte, achados e vínculo social/fazenda.
```

## 20. Fazenda, cidade e caverna

Fazenda:

```text
Stamina, fome, cansaço, ferramentas, Crafting, Survival, pets, companions, Fruto de Mana, Fonte de Anya.
```

Cidade:

```text
reputação, serviços, lojas, festivais, visitas de NPCs, Carisma.
```

Caverna:

```text
HP, MP, Stamina, armas, magia, resistências, skill trees, companions, pets, critical windows, traps, bosses, checkpoints, retorno.
```

---

# PARTE J — UI/HUD

HUD base:

```text
HP
MP quando relevante
Stamina
Fome
Cansaço
hotbar
4 active slots
indicador de Dash/Dodge/Block
status negativos
buffs
arma/ferramenta ativa
pet/companion status quando relevante
```

Não mostrar:

```text
Breath
Fôlego
BR
```

---

# PARTE K — Save/load

Persistir:

```text
nome
raça
gênero/apresentação
sprite base
cabelo/cor/opções visuais
level
XP
atributos
HP/MP/Stamina atuais e máximos
fome
cansaço
SkillPoints
skill trees
nodes/ranks/capstones
active slots
dash/dodge/block unlocks e ranks quando necessário
arquétipos
equipamentos
resistências/status ativos
companions/pets
Fonte de Anya/respec flags
estado de retorno/recuperação sistêmica
```

Não persistir:

```text
Breath
Fôlego
BR
```

---

# PARTE L — Decisões fechadas

```text
Breath/Fôlego removido do jogo.
Stamina é o único recurso físico imediato.
Cansaço é o atrito de longo prazo.
Constituição aumenta HP, Stamina, estabilidade e tolerância física.
Destreza melhora movimento, dodge, dash, attack speed e crit condicional.
Vontade melhora MP, resistência mental/espiritual e MP Regen.
Dash = Space + direção, custa Stamina, não ocupa active slot.
Dodge = double tap direcional, custa Stamina, não ocupa active slot.
Block = Left Shift, drena Stamina, não ocupa active slot.
Força não aumenta yield de recurso sozinha.
HP Regen é skill/efeito explícito.
MP Regen é lenta e baseada principalmente em Vontade/Magic.
```

---

# PARTE M — Pendências

```text
Remover BR das tabelas de monstros.
Substituir BR por traits como MovementProfile, RecoveryProfile e PressureProfile.
Definir custos finais de Stamina para Dash, Dodge, Block, corrida e ferramentas.
Definir Stamina Regen em combate/fora de combate/fome/cansaço.
Definir fórmulas finais de HP/MP/Stamina.
Definir dano base por arma/ferramenta/magia.
Definir contratos concretos de save/load.
