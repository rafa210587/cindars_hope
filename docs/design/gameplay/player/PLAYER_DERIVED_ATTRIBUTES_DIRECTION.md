# Cindar's Hope — Player Derived Attributes Direction

> **Status:** documento canônico de direção dos atributos derivados do personagem  
> **Local:** `docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md`  
> **Complementa:**  
> - `docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md`  
> - `docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md`  
> **Não é spec implementável.** Este documento define quais atributos derivados existem, o que representam, como se conectam aos atributos centrais, skills, equipamentos, HUD e combate.

---

## 0. Objetivo

Este documento define os atributos derivados do jogador.

Atributos centrais:

```text
Força
Constituição
Destreza
Inteligência
Vontade
Carisma
```

Atributos derivados:

```text
HP máximo
MP máximo
Stamina máxima
Breath/Fôlego máximo
Base Attack
Attack Damage
Magic Power
Healing Power
Defense
Armor
Elemental Resistance
Status Resistance
Crit Chance
Crit Damage
Attack Speed
Cast Speed
Movement Speed
Dash Distance
Dodge Invulnerability Window
Block Power
Block Stability
Stagger Power
Posture Damage
Posture Resistance
Resource Yield Bonus
Gathering Efficiency
Crafting Efficiency
Loot Bonus
Gold Bonus
Hunger Resistance
Fatigue Resistance
MP Regen
HP Regen
Carry Weight / Inventory Load futuro
Social Influence
Companion Command futuro
Pet Bond Effect futuro
```

Regra central:

```text
Atributo central dá aptidão bruta.
Atributo derivado traduz aptidão em gameplay.
Skill tree dá domínio e desbloqueios.
Equipamento dá capacidade material.
Buff dá modificação temporária.
```

---

# PARTE A — Regras gerais

## 1. Fórmulas finais não estão fechadas

Este documento não fecha números finais.

Ele define:

```text
o que cada stat derivado significa
quais atributos centrais influenciam
quais skills/equipamentos podem modificar
onde aparece na HUD/menu
quais cuidados de balanceamento existem
```

Fórmulas finais devem ser especificadas depois.

## 2. Evitar atributos derivados redundantes

O jogo deve evitar stats que fazem a mesma coisa com nomes diferentes.

Exemplo:

```text
Attack Damage = dano final físico ofensivo.
Base Attack = valor base antes de arma/skill/modificadores.
Stagger Power = capacidade de quebrar postura.
Posture Damage = dano aplicado à barra de postura do inimigo.
```

Se dois derivados forem indistinguíveis em gameplay, devem ser fundidos.

## 3. Regra de clareza para o jogador

Nem todo derivado precisa aparecer na HUD durante gameplay.

```text
HUD mostra estado e recursos imediatos.
Menu de personagem mostra atributos detalhados.
Tooltips mostram impactos relevantes.
```

Exemplo:

```text
HP, MP, Stamina e Breath aparecem na HUD.
Crit Chance, Attack Speed e Block Stability aparecem no menu/status/tooltip.
Resource Yield aparece em tooltip de skill/ferramenta, não como barra permanente.
```

---

# PARTE B — Recursos principais

## 4. HP máximo

Representa vida máxima.

Influenciado por:

```text
Constituição principalmente
equipamentos
buffs de comida
skills defensivas
arquétipos
Fonte de Anya/eventos especiais
```

Não representa:

```text
regeneração de vida
redução direta de dano
```

HUD:

```text
Barra principal sempre visível.
```

## 5. MP máximo

Representa reserva mágica.

Influenciado por:

```text
Vontade
Inteligência em menor grau
equipamentos mágicos
Magic/Arcano
Fruto de Mana
Fonte de Anya
```

HUD:

```text
Aparece quando magia/item mágico for desbloqueado ou equipado.
```

## 6. Stamina máxima

Representa energia física disponível para ações.

Influenciado por:

```text
Constituição
Força em menor grau para ações pesadas
skills de Survival e Crafting
equipamentos
comida
```

Usos:

```text
ferramentas
ataques físicos
block
dodge
dash
corrida
pesca/mineração/corte/plantio
```

HUD:

```text
Barra secundária sempre visível.
```

## 7. Breath / Fôlego máximo

Representa capacidade de sustentar esforço sob pressão.

Influenciado por:

```text
Constituição
Destreza
Vontade em menor grau
Survival/Sobrevivente
Melee/Guerreiro para block/armas pesadas
```

Usos:

```text
corrida
dash
dodge
block sustentado
ritmo de combate
long fights
ambiente hostil
```

HUD:

```text
Medidor compacto, expandido em combate/caverna/movimento intenso.
```

---

# PARTE C — Ofensivos físicos

## 8. Base Attack

Representa potência ofensiva base antes de arma e skill.

Influenciado por:

```text
Força principalmente
Destreza em armas leves/ranged
atributo racial/passivo leve
equipamento
```

Uso:

```text
base para Attack Damage físico.
```

Menu:

```text
Aparece no painel de status, não na HUD.
```

## 9. Attack Damage

Representa dano físico final após arma, material, skill, buff, resistência inimiga e contexto.

Influenciado por:

```text
Base Attack
arma equipada
material da arma
Melee/Guerreiro
Ranged/Caçador
buffs
critical windows
resistência/vulnerabilidade do inimigo
```

Regras:

```text
Força não aumenta yield de recurso.
Força aumenta dano físico e facilidade contra obstáculos físicos.
Attack Damage não deve substituir Stagger/Posture.
```

## 10. Attack Speed

Representa velocidade de execução/recuperação de ataques físicos.

Influenciado por:

```text
Destreza
tipo de arma
peso/material da arma
Melee/Guerreiro
Ranged/Caçador
Breath em situações de ritmo sustentado
```

Regras:

```text
Attack Speed não deve ficar alto a ponto de remover leitura de animação.
Armas pesadas devem continuar pesadas, mesmo com build forte.
```

## 11. Crit Chance

Representa chance de dano crítico.

Influenciado por:

```text
Destreza
Ranged/Caçador
Melee/Guerreiro
estado do inimigo
critical window
marcação
buffs/equipamento
```

Regras:

```text
Crítico deve ser mais relevante em abertura/vulnerabilidade do que em spam aleatório.
Chance de crítico base deve ser moderada.
Skills devem aumentar crit em condições claras.
```

## 12. Crit Damage

Representa multiplicador de dano crítico.

Influenciado por:

```text
arma
skills
capstones
equipamentos
estado de critical window
```

Regras:

```text
Crit Damage alto deve exigir condição ou especialização.
Não deve tornar ataques comuns explosivos o tempo todo.
```

## 13. Stagger Power

Representa capacidade de abalar o inimigo.

Influenciado por:

```text
Força
arma pesada
Melee/Guerreiro
ataques carregados
material da arma
```

Uso:

```text
aplica pressão na postura do inimigo.
```

## 14. Posture Damage

Representa dano aplicado à barra/estado de postura do inimigo.

Influenciado por:

```text
Stagger Power
Ataque Pesado
Quebra-Postura
Disparo Carregado
Disparo de Interrupção
vulnerabilidade do inimigo
```

Regras:

```text
Posture Damage deve ser importante contra elites/bosses.
Nem todo inimigo precisa ter barra visível de postura.
```

---

# PARTE D — Ofensivos mágicos

## 15. Magic Power

Representa potência mágica geral.

Influenciado por:

```text
Inteligência
Vontade
Magic/Arcano
equipamentos mágicos
Fruto de Mana
Fonte de Anya
```

Uso:

```text
dano mágico
duração/potência de controle leve
barreiras
purificação
```

## 16. Cast Speed

Representa velocidade de conjuração/canalização.

Influenciado por:

```text
Inteligência
Destreza em menor grau
Magic/Arcano
equipamento
```

Regras:

```text
Cast Speed não deve permitir spam sem custo de MP.
Magias fortes devem manter windup/cooldown.
```

## 17. Healing Power

Representa potência de cura.

Influenciado por:

```text
Vontade
Magic/Arcano
Fonte de Anya
Água Viva
itens de cura
equipamentos espirituais
```

Regras:

```text
Cura mágica deve ser limitada, cara e com cooldown.
Healing Power não deve invalidar comida, poções e Survival.
```

## 18. MP Regen

Representa regeneração natural de MP.

Influenciado por:

```text
Vontade principalmente
Fluxo Lento
Magic/Arcano
equipamentos
comida/poções
Fonte de Anya
```

Regras:

```text
MP Regen natural é lenta.
Vontade melhora a regeneração, mas não a torna rápida sozinha.
Regeneração rápida depende de efeito explícito.
```

HUD:

```text
Indicador discreto; não precisa número flutuante constante.
```

---

# PARTE E — Defesa e sobrevivência

## 19. Defense

Representa redução geral de dano físico antes/depois de armadura, conforme fórmula futura.

Influenciado por:

```text
Constituição
equipamento
armadura
Melee/Guerreiro
Survival/Sobrevivente
buffs
```

## 20. Armor

Representa proteção material do equipamento.

Influenciado por:

```text
armadura equipada
material
qualidade
Crafting/Produção
```

Diferença:

```text
Defense = defesa total do personagem.
Armor = contribuição do equipamento.
```

## 21. Elemental Resistance

Representa resistência a elementos.

Tipos:

```text
Fire
Ice
Lightning
Water/Nature
Arcane
Shadow/Nyx
Blackstone/Corruption
Heat
Cold
```

Influenciado por:

```text
Constituição
Vontade
equipamentos
comida
Survival
Magic
bioma/ambiente
```

## 22. Status Resistance

Representa resistência a status negativos.

Status relevantes:

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
Corruption
HeatStress
ColdStress
Fatigue
```

Influenciado por:

```text
Constituição para físico/tóxico
Vontade para mental/espiritual
Survival para ambiente/run
Magic para corrupção/Nyx/Void
```

## 23. Posture Resistance

Representa resistência a stagger/knockback/quebra de postura.

Influenciado por:

```text
Constituição
Força em menor grau
equipamento pesado
Melee/Guerreiro
Block ativo
```

## 24. Block Power

Representa quanto dano o Block reduz.

Influenciado por:

```text
Block rank
escudo/arma
Força
Constituição
Melee/Guerreiro
material do equipamento
```

Input:

```text
Left Shift.
```

HUD:

```text
Ícone defensivo separado dos active slots.
```

## 25. Block Stability

Representa quanto Stamina/Breath o Block consome ao receber impacto.

Influenciado por:

```text
Guarda Firme
Constituição
Breath
escudo/arma
equipamento
```

Regras:

```text
Block forte reduz dano, mas não deve ser gratuito.
Impactos fortes drenam recurso.
Sem recurso, Block quebra ou perde eficiência.
```

---

# PARTE F — Movimento

## 26. Movement Speed

Representa velocidade base de deslocamento.

Influenciado por:

```text
Destreza em menor grau
equipamento/peso
cansaço
fome
status negativos
buffs
```

Regras:

```text
Movement Speed não deve variar demais para não quebrar mapas, câmera e colisão.
```

## 27. Dash Distance

Representa distância do Dash.

Influenciado por:

```text
Dash base desbloqueado por tutorial/progressão
Passo de Impulso
Destreza
Breath
status/equipamento
```

Input:

```text
Space + direção.
```

## 28. Dodge Invulnerability Window

Representa janela curta de invulnerabilidade/evitação do Dodge.

Influenciado por:

```text
Dodge base
Reflexo de Esquiva
Destreza
Breath
status negativos
```

Input:

```text
double tap direcional.
```

Regras:

```text
Dodge não deve tornar o jogador invulnerável continuamente.
Janela deve ser curta e legível.
```

---

# PARTE G — Produção, loot e economia

## 29. Resource Yield Bonus

Representa chance/quantidade extra de recurso.

Influenciado por:

```text
Crafting/Produção
Coleta Eficiente
Prospector de Superfície
Garimpo de Run, para caverna
ferramenta/material
buffs
companions/pets
```

Não influenciado diretamente por:

```text
Força sozinha.
```

## 30. Gathering Efficiency

Representa custo/tempo/golpes para coletar.

Influenciado por:

```text
Força para obstáculos físicos
ferramenta
Crafting/Produção
Stamina
cansaço
```

Diferença:

```text
Gathering Efficiency = coletar com menos esforço.
Resource Yield Bonus = ganhar mais recurso.
```

## 31. Crafting Efficiency

Representa custo, qualidade, velocidade ou desperdício em crafting/construção.

Influenciado por:

```text
Inteligência
Crafting/Produção
Oficina Organizada
material
estação de trabalho
```

## 32. Loot Bonus

Representa chance de loot melhor.

Influenciado por:

```text
Survival
Faro de Tesouro
Olho do Caçador
Ranged para criaturas orgânicas
itens raros
buffs
```

Regras:

```text
Loot Bonus deve ser moderado para não quebrar economia.
Não cria loot inexistente em boss/quest único.
```

## 33. Gold Bonus

Representa chance/quantidade extra de ouro em drops/tesouros.

Influenciado por:

```text
Saqueador Cuidadoso
Carisma/economia em sistemas futuros
itens/buffs
```

Regras:

```text
Gold Bonus deve ser baixo/moderado.
Economia da cidade e fazenda não pode ser quebrada por farming de ouro.
```

---

# PARTE H — Fome e cansaço

## 34. Hunger Resistance

Representa resistência à perda de fome.

Influenciado por:

```text
Estômago Forte
comida
Constituição em menor grau
Survival
status negativos
```

## 35. Fatigue Resistance

Representa resistência ao acúmulo de cansaço.

Influenciado por:

```text
Ritmo de Jornada
Survival
Constituição
Stamina gasta
fome
sono
buffs
```

Regras:

```text
Fatigue Resistance reduz pressão, mas não elimina necessidade de dormir.
```

## 36. HP Regen

Representa regeneração de vida.

Influenciado por:

```text
Regeneração Natural
Descanso Curto
itens amplificadores
comida/poções
Fonte de Anya
Magic em cura ativa
```

Não influenciado diretamente por:

```text
Constituição sozinha.
```

Regras:

```text
HP Regen passivo deve ser lento e geralmente fora de combate.
HP Regen forte deve exigir item, Fonte, magia, comida ou condição.
```

---

# PARTE I — Social, pets e companions

## 37. Social Influence

Representa influência social geral.

Influenciado por:

```text
Carisma
reputação
presentes
quests
romance
itens sociais
arquétipos inferidos
```

Uso:

```text
amizade
preços
contratos
eventos
flerte/casamento
```

## 38. Companion Command

Stat futuro.

Representa eficiência em coordenar companions.

Influenciado por:

```text
Carisma
Vontade
relacionamento com companion
arquétipo Líder
quests
```

Direção:

```text
Não implementar como número exposto cedo se companions ainda estiverem simples.
```

## 39. Pet Bond Effect

Stat futuro.

Representa força dos bônus de pet.

Influenciado por:

```text
cuidado
alimentação
vínculo
Carisma
Animal/Pet systems futuros
itens
```

Direção:

```text
Pode afetar detecção de traps, ajuda contra swarms, achados, sorte e companhia na fazenda.
```

---

# PARTE J — HUD e menus

## 40. HUD gameplay

Mostrar sempre ou quase sempre:

```text
HP
Stamina
Fome/Cansaço em ícones compactos
```

Mostrar quando relevante:

```text
MP
Breath
Dash cooldown
Dodge feedback
Block state
HP Regen ativa
MP Regen discreta
status negativos
critical window
```

Não mostrar sempre:

```text
Crit Chance
Attack Speed
Loot Bonus
Gold Bonus
Crafting Efficiency
Resource Yield Bonus
Social Influence
```

## 41. Menu de atributos

Menu deve mostrar:

```text
atributos centrais
stats derivados principais
fonte dos bônus
efeito de equipamento atual
efeito de skills
efeitos temporários
```

Sugestão de grupos:

```text
Recursos: HP, MP, Stamina, Breath
Ofensivo: Attack Damage, Magic Power, Crit, Attack Speed
Defensivo: Defense, Armor, Resistances, Block
Movimento: Movement, Dash, Dodge
Produção: Yield, Gathering, Crafting
Exploração: Loot, Gold, Hunger/Fatigue
Social: Social Influence, Companions, Pets
```

---

# PARTE K — Decisões fechadas

```text
Atributos derivados existem para traduzir atributos centrais em gameplay.
Força melhora dano físico, stagger e esforço contra obstáculos, não yield direto.
Constituição aumenta HP e tolerância, não HP regen sozinha.
Vontade influencia MP e regen lenta de MP.
Destreza influencia timing, ataque leve, dodge e crit condicional.
Inteligência influencia magia técnica, crafting e leitura de sistemas.
Carisma influencia relação social, companions e economia social.
Dash Distance é derivado de movimento e Survival, não active slot.
Dodge Invulnerability Window é derivado de movimento e Reflexo de Esquiva.
Block Power e Block Stability são derivados defensivos ligados a Left Shift, Melee e equipamento.
Resource Yield Bonus é derivado de skills/ferramentas/buffs, não Força pura.
HP Regen é derivado de skill/efeito explícito, não Constituição pura.
MP Regen é lenta e ligada principalmente a Vontade/Magic.
Nem todo derivado aparece na HUD; muitos aparecem só em menu/tooltip.
```

---

# PARTE L — Pendências

```text
Definir fórmulas finais.
Definir nomes finais em PT-BR/EN para cada stat.
Definir quais stats aparecem no menu inicial.
Definir quais stats são ocultos e só afetam internamente.
Definir se Defense e Armor serão separados ou fundidos na implementação.
Definir se Base Attack precisa existir exposto ou apenas como cálculo interno.
Definir valores base por level.
Definir impacto de cada atributo central por ponto.
Definir impacto de equipamentos e materiais.
Definir limites máximos/caps de Crit Chance, Attack Speed, Movement Speed e HP/MP Regen.
Definir como buffs temporários aparecem no menu/HUD.
Definir contratos de save/load para buffs e stats permanentes.
