# Cindar's Hope — Player Core Systems Direction

> **Status:** documento canônico de direção ampla dos sistemas centrais do personagem  
> **Local:** `docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/SPEC_SOURCE_MAP.md`  
> - `docs/design/SPECIFICATION_PROCESS.md`  
> - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`  
> - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`  
> - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`  
> **Função:** consolidar atributos, status, progressão, skills, arquétipos inferidos, ferramentas, armas, magia, equipamentos, morte, companions, pets, fazenda, caverna, casamento, UI e save/load do personagem.  
> **Não é spec implementável.** Specs futuras devem ser quebradas em `docs/specs/a_implementar/`.

---

## 0. Regra de uso

Toda spec que tocar o personagem deve ler este documento.

Isso inclui:

```text
atributos
HP / MP / Stamina / Breath
fome
cansaço
level up
pontos de atributo
skills
skill levels
skill trees
active slots
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
caverna
UI/HUD
save/load
flerte / relacionamento / casamento
```

Regra principal:

```text
O jogador não possui classe fixa estilo D&D.
O jogador evolui livremente por atributos, skills, equipamentos, armas, ferramentas, magia e escolhas de gameplay.
O jogo pode inferir arquétipos/classes funcionais a partir da distribuição de skills e permitir que o jogador vista um arquétipo ativo para receber bônus.
```

---

# PARTE A — Visão geral do personagem

## 1. Fantasia de gameplay

O personagem é um habitante/adventurer de Vaalara capaz de combinar:

```text
vida de fazenda
exploração de caverna
combate
mineração
crafting
magia
relacionamentos
companions
pets
progressão de equipamentos
interação com a Fonte de Anya
```

O jogo deve permitir builds híbridas.

Exemplos:

```text
fazendeiro-mago
minerador-tanque
explorador arqueiro
combatente de duas armas
suporte com companions
criador de animais com pet de combate
artesão focado em economia
aventureiro de caverna com magia e resistência ambiental
```

## 2. Princípios de design

```text
Liberdade primeiro.
Classes fixas não devem limitar o jogador.
Atributos precisam ter sinergia real com skills e equipamentos.
Skills devem evoluir de nível 1 a 5.
Magia usa MP.
Ações físicas usam Stamina e/ou Breath.
Fome e cansaço afetam performance.
Companions e pets ajudam, mas não substituem o jogador.
A Fonte de Anya conecta morte, respec, cura, narrativa e progressão especial.
```

---

# PARTE B — Atributos principais

## 3. Atributos canônicos

Atributos principais:

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
Chance de crítico / precisão futura
Eficiência de ferramentas
Eficiência de magia
Eficiência social
```

## 4. Progressão de atributos

Regra canônica inicial:

```text
No level 1, o jogador começa com 1 ponto em cada atributo principal.
A cada level up, o jogador ganha +1 ponto para distribuir em um atributo principal.
```

Direção:

```text
A progressão deve ser lenta o bastante para escolhas importarem.
Equipamentos, alimentos, buffs, skills e arquétipos inferidos podem alterar temporariamente a performance.
```

## 5. O que cada atributo representa

### Força

Representa potência física.

Afeta:

```text
dano com armas pesadas
dano com ferramentas usadas ofensivamente
eficiência de mineração pesada
eficiência em cortar madeira
capacidade de quebrar rochas/troncos mais resistentes
stagger/knockback
algumas janelas de crítico com HeavyAttack
```

Sinergias:

```text
Força + Constituição = tanque físico / minerador pesado
Força + Destreza = combatente agressivo
Força + Breath = armas pesadas com melhor ritmo
Força + Ferramentas = melhor coleta pesada
```

### Constituição

Representa robustez, saúde e resistência corporal.

Afeta:

```text
HP máximo
resistência a dano físico
resistência a veneno/sangramento/frio/calor em menor grau
recuperação passiva limitada
resistência ao cansaço
capacidade de long runs na caverna
```

Sinergias:

```text
Constituição + Vontade = alta resistência geral
Constituição + Força = frontliner/tanque/minerador
Constituição + Farming/Animals = rotina pesada de fazenda
```

### Destreza

Representa coordenação, mobilidade, precisão e reflexo.

Afeta:

```text
velocidade de ataque leve
esquiva/dodge
precisão com arco/dagger/spear
chance ou qualidade de crítico, se definido em spec
interação com traps
movimentação em combate
janelas de backstab/positioning
```

Sinergias:

```text
Destreza + Força = melee rápido/agressivo
Destreza + Inteligência = trap/precision/magic hybrid
Destreza + Bow/Spear/Dagger = explorador ágil
Destreza + Pet = interrupções e flancos
```

### Inteligência

Representa conhecimento, técnica, magia estruturada e crafting avançado.

Afeta:

```text
MP máximo secundário ou scaling mágico, conforme spec
potência/eficiência de magia técnica
crafting avançado
identificação de monstros/traps/recursos
uso de tecnologia bromeciana
eficiência de skills mágicas ou técnicas
```

Sinergias:

```text
Inteligência + Vontade = mago consistente
Inteligência + Destreza = arqueiro/trapper/mago ágil
Inteligência + Crafting/Mining = engenheiro/artesão
Inteligência + Ruínas = bônus em conteúdo bromeciano/Elyndor
```

### Vontade

Representa força espiritual, foco, resistência mental e ligação com magia profunda.

Afeta:

```text
MP máximo principal ou regeneração, conforme spec
resistência a medo/confusão/Nyx/Void
força de magias espirituais
resistência a corrupção
interação com Fonte de Anya
estabilidade em eventos de lore
```

Sinergias:

```text
Vontade + Inteligência = magia forte e eficiente
Vontade + Constituição = resistência mental/física
Vontade + Carisma = liderança/companions/relações
Vontade + Fonte de Anya = respec/cura/lore futura
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

Sinergias:

```text
Carisma + Vontade = líder espiritual/suporte
Carisma + Inteligência = negociador/artesão/comerciante
Carisma + Pets/Animals = vínculo e bônus sociais
Carisma + City Reputation = acesso social e quests
```

---

# PARTE C — HP / MP / Stamina / Breath

## 6. HP

HP representa sobrevivência física.

Fontes:

```text
Constituição
equipamentos
alimentos/buffs
skills defensivas
arquétipo inferido ativo
Fonte de Anya/eventos especiais
```

Usos:

```text
combate
traps
hazards
caverna
queda/dano ambiental futuro
```

## 7. MP

MP representa energia mágica para habilidades mágicas.

Regra:

```text
MP é obrigatório para magia e skill actions mágicas.
Magia não deve consumir apenas stamina.
```

Fontes:

```text
Vontade
Inteligência
equipamentos mágicos
comida rara
Fruto de Mana
Fonte de Anya
skills mágicas
arquétipos inferidos mágicos
```

Usos:

```text
magias ofensivas
magias defensivas
cura limitada, se existir
skills de utilidade
interações arcanas
tecnologia/lore de Elyndor/Bromécia, quando aplicável
```

## 8. Stamina

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
dodge
correr, se definido
uso pesado de ferramentas
```

Stamina afeta cansaço:

```text
Gastar stamina acelera crescimento de cansaço.
Gastar stamina com fome acelera ainda mais.
```

## 9. Breath / Fôlego

Breath representa ritmo respiratório, explosão, sustentação e capacidade de manter esforço sob pressão.

Afeta:

```text
corrida/dash/dodge
ritmo de combate
uso de armas pesadas
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

# PARTE D — Fome e cansaço

## 10. Fome

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

## 11. Cansaço

Cansaço é sistema próprio, mas afeta `PlayerConditionManager`.

Representa desgaste acumulado do dia.

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
Extremo: jogador precisa dormir/retornar, risco de colapso conforme spec futura.
```

Reduz com:

```text
dormir
descanso específico
comida adequada
itens raros
Fonte de Anya em casos especiais
```

---

# PARTE E — Level up e pontos

## 12. Level up

O level representa experiência geral do personagem.

Ganha XP por:

```text
combate
mineração
fazenda
crafting
quests
pesca
relacionamentos/eventos, se definido
bosses
exploração
```

Regra:

```text
O jogo deve evitar que só combate seja caminho válido.
Fazenda e cidade também devem contribuir para progressão, mas a caverna concentra risco/recompensa maior.
```

## 13. Pontos por level

A cada level:

```text
+1 ponto de atributo principal para distribuir.
```

Pontos de skill podem seguir outra regra, a definir em spec.

Direção possível:

```text
SkillPoint a cada 2 níveis, se mantido do refinamento anterior.
Skill XP por uso para progressão interna das skills.
```

---

# PARTE F — Skills e níveis de skill

## 14. Regra geral de skills

Skills são competências de gameplay. Cada skill pode ter níveis de 1 a 5.

```text
Skill Level 1: desbloqueio básico
Skill Level 2: eficiência ou custo melhor
Skill Level 3: nova interação ou melhoria relevante
Skill Level 4: especialização forte
Skill Level 5: domínio / perk marcante / sinergia com tree
```

Skills podem evoluir por:

```text
uso repetido
pontos de skill
quests/treinadores
livros/blueprints
equipamentos
Fonte de Anya/respec
```

Specs futuras devem decidir quais skills usam XP por uso, pontos ou ambos.

## 15. Skills agrícolas

### Farming / Agricultura

Afeta:

```text
arar
plantar
regar
colher
qualidade de crops
chance de crop especial
uso de fertilizante
eficiência de stamina em ações agrícolas
```

Níveis:

```text
1: ações básicas de plantio.
2: menor custo de stamina ao arar/reguar.
3: melhor qualidade média de crops.
4: melhor uso de fertilizantes/irrigação.
5: chance de colheita rara/qualidade premium/sinergia com Mana.
```

### Animal Care / Criação de Animais

Afeta:

```text
alimentação
carinho/afinidade
qualidade de produtos animais
saúde animal
pets e vínculo, se compartilhado parcialmente
```

Níveis:

```text
1: cuidado básico.
2: menor perda de afinidade por rotina falha.
3: produtos melhores.
4: animais mais resistentes/felizes.
5: bônus forte de produção/vínculo raro.
```

### Cooking / Culinária

Afeta:

```text
comidas
buffs
recuperação de stamina/fome/cansaço
resistências temporárias
preparação para caverna
```

Níveis:

```text
1: receitas simples.
2: melhor recuperação.
3: buffs básicos.
4: buffs combinados.
5: pratos especiais, Fruto de Mana e receitas raras.
```

## 16. Skills de exploração/caverna

### Mining / Mineração

Afeta:

```text
quebrar pedras
minérios raros
eficiência da picareta
stamina por golpe
chance de gemas
salas de mineração
interação com Titã Escavador/rochas especiais, se definido
```

Níveis:

```text
1: mineração básica.
2: menor custo de stamina.
3: maior chance de minério raro.
4: quebra de nodes especiais.
5: leitura de veios raros e bônus em mining rooms.
```

### Exploration / Exploração

Afeta:

```text
map awareness
secret rooms
traps
landmarks
rotas alternativas
chance de encontrar special rooms
```

Níveis:

```text
1: percepção básica de landmarks.
2: melhor detecção de rotas/saídas.
3: chance de detectar secret rooms.
4: melhor leitura de traps.
5: bônus forte em exploração e recompensas ocultas.
```

### Survival / Sobrevivência

Afeta:

```text
fome
cansaço
hazards
frio/calor
long runs
consumo de recursos
```

Níveis:

```text
1: penalidades menores em ambiente leve.
2: menor cansaço por exploração.
3: melhor resistência ambiental.
4: melhor recuperação com comida/descanso.
5: long runs muito mais viáveis.
```

## 17. Skills de combate

### Melee Combat / Combate Corpo a Corpo

Afeta:

```text
dano físico
combos
janelas de crítico
armas corpo a corpo
stagger
uso de stamina/breath em ataque
```

Níveis:

```text
1: ataques básicos.
2: menor custo/maior precisão.
3: charged attack melhor.
4: melhor uso de critical windows.
5: perk marcante de estilo melee.
```

### Ranged Combat / Combate à Distância

Afeta:

```text
arcos/projéteis
precisão
distância
pontos fracos
inimigos flutuantes
traps à distância
```

Níveis:

```text
1: tiro básico.
2: melhor precisão.
3: dano em ponto fraco.
4: melhor crítico em janelas.
5: tiro especial/perk de elite.
```

### Defense / Defesa

Afeta:

```text
bloqueio
redução de dano
stagger resistance
defesa com escudo/armadura
recuperação após hit
```

Níveis:

```text
1: defesa básica.
2: menor dano recebido em bloqueio.
3: melhor resistência a stagger.
4: counter após bloqueio/perfect guard, se existir.
5: defesa avançada contra elites/bosses.
```

## 18. Skills mágicas

### Magic Control / Controle Mágico

Afeta:

```text
custo de MP
potência de magia
controle de projéteis/áreas
interações arcanas
```

Níveis:

```text
1: magia básica.
2: menor custo de MP.
3: maior potência/alcance.
4: novas propriedades/sinergias.
5: domínio mágico e skill action forte.
```

### Anya Affinity / Afinidade com Anya

Skill especial/lore. Não deve ser tratada como skill comum desde o início sem desbloqueio narrativo.

Afeta:

```text
Fonte de Anya
respec
cura especial
morte/retorno
Água Viva
eventos de lore
libertação parcial do poder de Anya
```

Níveis:

```text
1: percepção fraca da Fonte.
2: interações melhores com cura/respec.
3: resistência parcial a corrupção.
4: Água Viva/rituais especiais.
5: efeitos após nível 101/libertação parcial.
```

## 19. Skills sociais/econômicas

### Social / Socialização

Afeta:

```text
diálogos
reputação
amizade
flerte
eventos sociais
convites para fazenda
```

### Romance / Flerte

Afeta:

```text
opções românticas
eventos de relacionamento
interpretação de preferências
ritmo de vínculo
casamento
```

Regra:

```text
Romance deve depender de relação, quests, presentes, eventos e compatibilidade narrativa, não só de barra numérica.
```

### Trade / Comércio

Afeta:

```text
preços
shipping bin
ordens
contratos
negociação
reputação econômica
```

## 20. Skills técnicas

### Crafting / Artesanato

Afeta:

```text
receitas
equipamentos
processadores
workshops
qualidade de item
reparo
```

### Engineering / Engenharia Bromeciana

Skill avançada.

Afeta:

```text
tecnologia bromeciana
ruínas
constructos
mecanismos
irrigação avançada
máquinas
traps mecânicas
```

---

# PARTE G — Skill trees e active slots

## 21. Skill trees

Skill trees organizam progressão por tema.

Árvores iniciais sugeridas:

```text
Fazenda
Animais/Pets
Mineração
Exploração
Combate Corpo a Corpo
Combate à Distância
Defesa
Magia
Social/Romance
Crafting/Engenharia
Afinidade com Anya
```

Regra:

```text
Skill tree não deve impedir builds híbridas.
Capstones devem ser fortes, mas não obrigatórios para jogar.
```

## 22. Active slots

Active slots representam habilidades ativas equipadas.

Direção já consolidada anteriormente:

```text
4 active slots como base.
```

Regras:

```text
O jogador não pode equipar todas as skills ativas ao mesmo tempo.
Trocar active slots deve exigir menu/descanso/Fonte/fora de combate, conforme spec.
Skills passivas não ocupam active slot.
```

---

# PARTE H — Arquétipos inferidos / jobs vestíveis

## 23. Sem classes fixas

O jogo não implementa classes estilo D&D.

Regra:

```text
O jogador não escolhe uma classe permanente.
O jogo observa distribuição de atributos, skills, equipamentos e comportamento.
Com isso, pode inferir arquétipos/jobs funcionais.
O jogador pode vestir/ativar um arquétipo desbloqueado para receber bônus.
```

## 24. Arquétipos inferidos

Exemplos:

```text
Minerador
Explorador
Fazendeiro
Criador
Combatente
Arqueiro
Guardião
Mago
Artífice
Mercador
Companheiro/Líder
Devoto de Anya
```

## 25. Como desbloquear arquétipos

Um arquétipo pode exigir:

```text
skill levels mínimos
atributos mínimos
equipamento compatível
quests
marcos de caverna/fazenda/cidade
relacionamentos
Fonte de Anya
```

Exemplo:

```text
Minerador
Requisitos: Mining 3, Força 4, Constituição 3, picareta melhorada.
Bônus ativo: menor stamina em mineração, maior chance de minério raro, resistência leve a cave fatigue.
```

## 26. Vestir arquétipo

Regras:

```text
O jogador pode ter vários arquétipos desbloqueados.
Pode vestir/ativar um número limitado por vez.
Arquétipo ativo dá bônus, mas não bloqueia ações fora dele.
Troca de arquétipo deve ser feita fora de combate ou em locais seguros.
```

---

# PARTE I — Ferramentas, armas, magia e equipamentos

## 27. Ferramentas

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

Ferramentas afetam:

```text
fazenda
mineração
recursos
combate improvisado
stamina
cansaço
skills por uso
```

## 28. Armas

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
skills
vulnerabilidades de monstros
critical windows
stamina/breath
magia/equipamentos
```

## 29. Magia

Magia usa MP.

Fontes de magia:

```text
Inteligência
Vontade
skills mágicas
equipamentos
Fonte de Anya
Fruto de Mana
lore de Vaalara
```

Tipos iniciais possíveis:

```text
ofensiva
defensiva
suporte
utilidade
controle leve
interação com ambiente
```

Regra:

```text
Magia não deve resolver todos os sistemas sozinha.
Ela deve ter custo, limite, build e counterplay.
```

## 30. Equipamentos

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
```

---

# PARTE J — Resistências e status negativos

## 31. Resistências

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

## 32. Status negativos

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
DurabilityStress não destrói item permanentemente sem spec própria.
```

---

# PARTE K — Morte, derrota e Fonte de Anya

## 33. Derrota

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

## 34. Fonte de Anya

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
A libertação parcial do poder de Anya ocorre por conteúdo profundo da caverna/nivel 101.
```

---

# PARTE L — Companions, pets, fazenda e caverna

## 35. Companions

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

## 36. Pets

Pets são sistema próprio, separado de companion.

```text
Cachorro pode apoiar combate, detectar traps e ajudar contra swarms.
Gato pode apoiar sorte, detecção de segredo/anomalia e vínculo social/fazenda.
```

Cachorro não ocupa slot de companion.

## 37. Relação com fazenda

O personagem se conecta à fazenda por:

```text
stamina
fome
cansaço
ferramentas
skills agrícolas
pets
companions trabalhando
construções
Fruto de Mana
Fonte de Anya
```

## 38. Relação com caverna

O personagem se conecta à caverna por:

```text
HP
MP
Stamina
Breath
armas
magia
resistências
skills de combate/exploração/mineração
companions
pets
critical windows
traps
boss gates
checkpoints
morte/retorno
```

---

# PARTE M — Flerte, relacionamento e casamento

## 39. Relacionamentos

Relacionamentos devem considerar:

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

## 40. Flerte

Flerte deve ser opção explícita e respeitar disponibilidade do NPC.

Regras:

```text
NPCs casados não são candidatos a casamento.
NPCs disponíveis podem aceitar romance com jogador homem ou mulher, conforme design de personagem.
Romance não deve depender só de gifts repetidos.
Romance deve ter eventos, escolhas, quests e limites claros.
```

## 41. Casamento

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

# PARTE N — UI/HUD e save/load

## 42. UI/HUD do personagem

HUD deve mostrar claramente:

```text
HP
MP
Stamina
Breath
Fome
Cansaço
hotbar
active slots
status negativos
buffs
arma/ferramenta ativa
pet/companion status quando relevante
```

Menus necessários:

```text
atributos
skills
skill trees
arquétipo ativo
equipamentos
relacionamentos
pets
companions
status/resistências
```

## 43. Save/load

Save deve persistir:

```text
level
XP
atributos
HP/MP/Stamina/Breath atuais e máximos
fome
cansaço
skills e níveis
skill points
skill trees desbloqueadas
active slots
ações equipadas
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

# PARTE O — Roadmap de specs futuras

```text
spec_player_attributes_progression_points.md
spec_player_hp_mp_stamina_breath_formulas.md
spec_player_hunger_fatigue_condition_manager.md
spec_player_skills_level_1_5_progression.md
spec_player_skill_trees_active_slots.md
spec_player_inferred_jobs_archetypes.md
spec_player_tools_usage_upgrade_stamina.md
spec_player_weapons_damage_types_critical_windows.md
spec_player_magic_mp_skill_actions.md
spec_player_equipment_resistances_status.md
spec_player_death_defeat_anya_fountain.md
spec_player_companion_pet_integration.md
spec_player_relationship_flirt_marriage.md
spec_ui_player_hud_status_skills_equipment.md
spec_save_player_core_systems.md
```

---

# PARTE P — Decisões fechadas

```text
O jogador não terá classe fixa estilo D&D.
O jogador evolui livremente por atributos, skills, equipamentos, ferramentas, armas e magia.
Classes/jobs serão arquétipos inferidos por distribuição de skills/atributos e poderão ser vestidos/ativados para bônus.
Cada skill pode ter níveis de 1 a 5.
MP existe e é usado por magia.
Breath/Fôlego é stat separado de Stamina.
Cansaço é sistema próprio e afeta PlayerConditionManager.
Fome pressiona planejamento, mas não deve ser morte punitiva por padrão.
Pets são separados de companions.
Cachorro pode apoiar combate sem ocupar slot de companion.
Flerte/casamento fazem parte dos sistemas do personagem/social.
Fonte de Anya conecta morte, respec, cura especial e progressão de lore.
```

---

# PARTE Q — Pendências

```text
Definir fórmulas finais de HP/MP/Stamina/Breath.
Definir curva de XP e level cap.
Definir se skill XP é por uso, por ponto ou híbrido.
Definir número final de skill trees e capstones.
Definir quantos arquétipos/jobs podem ficar ativos ao mesmo tempo.
Definir se troca de active slots/jobs ocorre na Fonte, casa, menu ou checkpoint.
Definir dano base por arma/ferramenta/magia.
Definir sistema de romance/casamento por NPC.
Definir relação entre cônjuge, companion e NPC de serviço.
Definir save contracts concretos.
