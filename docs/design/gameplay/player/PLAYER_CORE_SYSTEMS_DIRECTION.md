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
> **Função:** consolidar a direção ampla do personagem jogável: criação inicial, raças, gênero, aparência por sprites, atributos, HP/MP/Stamina/Breath, fome, cansaço, level up, skill trees existentes, arquétipos inferidos, ferramentas, armas, magia, equipamentos, resistências, morte, Fonte de Anya, companions, pets, fazenda, cidade, caverna, flerte, casamento, UI e save/load.  
> **Não é spec implementável.** Este documento descreve como o sistema deve funcionar em visão de jogo. Specs futuras quebram partes disso quando entrarmos em execução.

---

## 0. Regra de uso

Este é o documento canônico de direção para o personagem jogável.

Ele deve orientar refinamentos futuros de:

```text
criação do personagem
raças jogáveis iniciais
gênero / apresentação
aparência em sprites
atributos
HP / MP / Stamina / Breath
fome
cansaço
level up
pontos de atributo
5 skill trees existentes
skill levels
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
socializador com forte rede de NPCs
aventureiro de caverna com magia e resistência ambiental
líder de companions
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

Lista inicial sugerida:

| Raça | Nome no jogo | Direção visual | Papel de gameplay |
|---|---|---|---|
| Humano de Dornécia | Humano | silhueta padrão, maior variação de cabelo/pele | versátil, social, sem especialização extrema |
| Elfo da Noite / Luandil | Elfo da Noite | orelhas longas, tons frios, postura elegante | Destreza, Vontade, exploração/noturno |
| Anão de Khaz Baruk | Anão | corpo baixo/largo, barba/cabelo forte | mineração, Constituição, Força, ferramentas |
| Halfling | Halfling | pequeno, ágil, expressivo | sorte, social, agricultura, esquiva |
| Tiefling | Tiefling | chifres pequenos/médios, cauda opcional se suportada | magia, Carisma, resistência a efeitos |
| Meio-Orc | Meio-Orc | corpo forte, presas pequenas, postura robusta | Força, Constituição, combate físico |
| Draconato | Draconato | cabeça/escamas dracônicas, cauda opcional se suportada | Breath, resistências, presença física |
| Goblin | Goblin | pequeno, orelhas grandes, postura esperta | Destreza, tecnologia/traps, exploração |

Regras:

```text
Raça inicial pode dar identidade visual, pequenos bônus e pequenos traços passivos.
Raça inicial não deve bloquear romances, profissões, magia, fazenda ou final do jogo.
Bônus raciais devem ser menores que escolhas de build, skills, equipamentos e arquétipos.
```

## 5. Raças não iniciais ou futuras

```text
Nymirianos
  ligados a Anya, Cindar, Água Viva e Fonte.
  Devem ser lore profunda, não raça comum inicial.

Gnomorin / gnomos técnicos
  podem aparecer em ruínas, cidade, tecnologia bromeciana e NPCs.
  Podem virar jogáveis no futuro, mas exigem visual e animações próprias.

Drow / linhagens sombrias específicas
  podem aparecer no Abismo Sem-Lua/caverna/Nyx.
  Não devem ser jogáveis inicialmente sem direção narrativa clara.

Outras linhagens regionais de Vaalara
  podem ser adicionadas depois, se fizerem sentido no recorte de Cindar's Hope.
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
variação básica de pele/escama, se suportada
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
Draconatos podem usar crista/chifre/escama no lugar de cabelo comum.
Tieflings precisam compatibilizar cabelo com chifres.
Goblin/halfling/anão podem reaproveitar cabelo com escala ajustada.
Cada opção visual precisa ser sustentável em spritesheets de idle, walk, tool use, combat, hit, sleep/defeat e social interaction.
```

## 8. Sprite base por raça

| Raça | Base sprite | Observação |
|---|---|---|
| Humano | Medium 32x48 | base padrão |
| Elfo da Noite | Medium 32x48 | orelhas e paleta diferenciadas |
| Anão | Short/Stocky 32x40 ou 32x44 | collider deve continuar consistente |
| Halfling | Small 24x32 ou 28x36 | precisa validar leitura com ferramentas |
| Tiefling | Medium 32x48 | chifres/cauda não podem quebrar animações |
| Meio-Orc | Medium/Large visual 32x48 | corpo mais largo sem alterar collider injustamente |
| Draconato | Medium/Large visual 32x48 | cabeça/escamas demandam sprites próprios |
| Goblin | Small 24x32 ou 28x36 | animações rápidas e orelhas grandes |

Regra:

```text
Collider/footbox deve preservar justiça de gameplay.
Raça pequena não deve ter vantagem abusiva de hitbox, salvo decisão explícita.
Raça grande não deve ser punida por sprite maior, salvo se houver compensação mecânica clara.
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
Destreza não substitui skill de exploração/traps.
Inteligência não substitui skill de crafting/produção.
Vontade pode influenciar regeneração lenta de MP, mas magia forte depende de skill/equipamento.
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
esquiva/dodge
precisão com arco/dagger/spear
interação com janelas de crítico
movimentação em combate
backstab/positioning
reação contra traps, se houver skill adequada
```

### Inteligência

Representa conhecimento, técnica, raciocínio, magia estruturada e crafting avançado.

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
dodge
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
SkillPoint: 1 ponto a cada 2 níveis, conforme direção consolidada de FASE9K/Spec 16.
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

# PARTE F — 5 Skill Trees existentes

## 21. Regra geral

O modelo inicial deve manter as 5 árvores já consolidadas no jogo/documentos anteriores:

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
Respec na Fonte de Anya.
```

## 22. Estrutura de tiers

```text
Tier 1: fundamentos da árvore.
Tier 2: eficiência e custo.
Tier 3: nova técnica/interação relevante.
Tier 4: especialização forte.
Tier 5: capstone.
```

Skills individuais podem ter ranks/níveis internos de 1 a 5, desde que não conflitem com os tiers da árvore.

```text
Tree Tier = posição/desbloqueio na árvore.
Skill Rank = melhoria da habilidade específica.
```

## 23. Melee / Guerreiro

Função:

```text
combate corpo a corpo
armas físicas próximas
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

Sinergia correta:

```text
Força aumenta potência, stagger e eficiência contra obstáculos/inimigos resistentes.
Constituição aumenta margem de erro e resistência.
Destreza melhora timing, dodge e janelas de crítico.
Breath sustenta ritmo de combate e armas pesadas.
```

Não deve fazer:

```text
aumentar yield de madeira/minério por Força pura
substituir Survival em long runs
substituir Crafting/Produção em melhoria de ferramentas
```

Possíveis nodes:

```text
Golpe Pesado
Guarda Firme
Quebra-Postura
Contra-Ataque
Lâmina Precisa
Executor de Janelas
```

Capstone sugerido:

```text
Guerreiro das Profundezas
  melhora punição de janelas vulneráveis e sustain em combate físico sem trivializar bosses.
```

## 24. Ranged / Caçador

Função:

```text
combate à distância
arcos/projéteis
pontos fracos
inimigos voadores/flutuantes
controle de distância
traps à distância
abertura de combate segura
```

Atributos principais:

```text
Destreza
Inteligência
Vontade secundária para foco/controle
```

Sinergia correta:

```text
Destreza melhora precisão, timing, dodge e uso de arco.
Inteligência melhora leitura de pontos fracos, traps e padrões.
Vontade ajuda foco sob medo/pressão e precisão em situações de controle mental.
```

Não deve fazer:

```text
virar magia sem custo de MP
substituir Exploração/Sobrevivência inteira
invalidar melee em todos os cenários
```

Possíveis nodes:

```text
Mira Estável
Tiro em Ponto Fraco
Passo do Caçador
Olho na Escuridão
Disparo de Interrupção
Caçador de Voadores
```

Capstone sugerido:

```text
Predador Silencioso
  melhora abertura de combate, pontos fracos e interrupção sem permitir matar elites/bosses sem risco.
```

## 25. Magic / Arcano

Função:

```text
magias ofensivas
magias defensivas
controle leve
suporte mágico
interações com Fonte de Anya
MP
Mana
corrupção
Blackstone/Nyx/Void
```

Atributos principais:

```text
Vontade
Inteligência
Carisma secundário para magia social/suporte, se existir
```

Sinergia correta:

```text
Vontade influencia MP, regen lenta, resistência espiritual e Fonte.
Inteligência influencia controle técnico, custo e potência estruturada.
Carisma pode afetar suporte/liderança mágica, mas não dano bruto.
```

Regra de MP:

```text
MP regenera lentamente por base.
Vontade melhora regen lentamente.
Nodes de Magic/Arcano podem reduzir custo, melhorar regen ou aumentar eficiência.
Regeneração rápida depende de comida, poção, Fonte, equipamento, Fruto de Mana ou capstone.
```

Não deve fazer:

```text
resolver todos os sistemas sozinho
substituir ferramentas
substituir social/relacionamento
curar sem custo relevante
```

Possíveis nodes:

```text
Canalização Serena
Foco Arcano
Fluxo Lento
Resistir Corrupção
Selo de Anya
Eco da Fonte
```

Capstone sugerido:

```text
Fragmento Desperto
  fortalece interação com Fonte, Mana e magia de suporte após marcos narrativos profundos.
```

## 26. Survival / Sobrevivente

Função:

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
resistência a medo/corrupção em menor grau
```

Atributos principais:

```text
Constituição
Destreza
Vontade
Inteligência secundária para leitura de ambiente
```

Sinergia correta:

```text
Constituição aumenta tolerância física e margem de erro.
Destreza melhora reação, dodge e interação com traps.
Vontade melhora resistência mental/espiritual.
Inteligência melhora leitura de ruínas, armadilhas e ambiente.
```

Regra de HP regen:

```text
Constituição não dá regen passiva de HP.
Se houver regeneração de HP, ela entra por node de Survival/Sobrevivente, comida, poção, gear ou Fonte.
Regeneração deve ser lenta, limitada e fora de combate, salvo efeito raro.
```

Possíveis nodes:

```text
Passo Seguro
Olho de Explorador
Fôlego de Jornada
Sangue Frio
Descanso Curto
Leitura de Ruína
```

Capstone sugerido:

```text
Sobrevivente das Profundezas
  reduz cansaço de exploração, melhora leitura de perigos e sustenta long runs sem remover risco.
```

## 27. Crafting / Produção

Função:

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

Sinergia correta:

```text
Força reduz esforço/golpes em árvore/rocha, mas yield extra vem desta árvore, ferramentas ou buffs.
Constituição reduz desgaste/cansaço em rotina produtiva, mas não regenera HP.
Destreza ajuda timing, pesca e uso eficiente de ferramentas.
Inteligência melhora crafting, processadores, qualidade e tecnologia.
Carisma pode ajudar animais, troca, venda e reputação produtiva.
Vontade interage com Mana, Fonte e crops raras.
```

Possíveis nodes:

```text
Mãos de Lavrador
Colheita Cuidadosa
Lenhador Eficiente
Prospector
Pescador Paciente
Cozinha Sustentadora
Oficina Organizada
Engenharia Prática
```

Capstone sugerido:

```text
Mestre da Produção
  melhora rendimento geral de produção/coleta e qualidade alta sem quebrar economia.
```

---

# PARTE G — Sistemas transversais não tratados como árvore inicial

## 28. Social, romance, companions e pets

Esses sistemas são importantes, mas não substituem uma das 5 árvores iniciais.

Direção:

```text
Social/romance/casamento deve existir como sistema próprio.
Companions devem ter afinidade, função e evolução própria.
Pets devem ter vínculo, cuidado e utilidade própria.
Esses sistemas podem receber bônus de Carisma, Vontade, Inteligência, traits raciais, quests, itens e arquétipos.
Eles podem futuramente ganhar árvore própria, mas não devem substituir Melee/Ranged/Magic/Survival/Crafting no modelo inicial.
```

Interações possíveis:

```text
Carisma melhora ganho de relação, negociação e liderança.
Vontade melhora vínculo profundo e confiança.
Inteligência melhora negociação/contratos/leitura social.
Crafting/Produção pode melhorar presentes, comida e economia.
Survival pode melhorar pets na caverna.
Melee/Ranged/Magic podem abrir estilos de companion em combate.
```

---

# PARTE H — Active slots, capstones e arquétipos

## 29. Active slots

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
```

## 30. Capstones

Regras:

```text
Cada árvore tem Tier 5 como capstone.
Capstone recompensa especialização.
Capstone não deve ser obrigatório para zerar o jogo.
Capstone não deve invalidar outras árvores.
Capstone pode desbloquear arquétipos inferidos.
```

## 31. Arquétipos inferidos / jobs vestíveis

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
| Guerreiro | Melee + Força/Constituição | corpo a corpo, stagger e defesa |
| Caçador | Ranged + Destreza/Inteligência | precisão, pontos fracos e abertura segura |
| Arcano | Magic + Vontade/Inteligência | custo, MP, magia e Fonte |
| Sobrevivente | Survival + Constituição/Destreza/Vontade | long runs, traps e hazards |
| Produtor | Crafting + Inteligência/Força | ferramentas, yield via skill e qualidade |
| Minerador | Crafting + Força/Constituição | mineração eficiente e menos desgaste |
| Artífice | Crafting + Inteligência | tecnologia, ruínas e equipamentos |
| Devoto de Anya | Magic + Survival + eventos | Fonte, Água Viva, purificação/cura limitada |
| Líder | Carisma + companions/quests | companions/pets e relações, sem ser árvore inicial |
| Mercador | Carisma + Crafting/economia | preços, contratos e reputação econômica |

---

# PARTE I — Ferramentas, armas, magia e equipamentos

## 32. Ferramentas

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

Regra:

```text
Ferramenta melhor pode aumentar yield.
Crafting/Produção pode aumentar yield.
Força pode reduzir esforço/golpes, mas não aumenta yield sozinha.
```

## 33. Armas

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
```

## 34. Magia

Magia usa MP.

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

## 35. Equipamentos

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
```

---

# PARTE J — Resistências e status negativos

## 36. Resistências

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

## 37. Status negativos

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

# PARTE K — Morte, derrota e Fonte de Anya

## 38. Derrota

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

## 39. Fonte de Anya

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

# PARTE L — Companions, pets, fazenda, cidade e caverna

## 40. Companions

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

## 41. Pets

Pets são sistema próprio, separado de companion.

```text
Cachorro pode apoiar combate, detectar traps e ajudar contra swarms.
Gato pode apoiar sorte, detecção de segredo/anomalia e vínculo social/fazenda.
```

Cachorro não ocupa slot de companion.

## 42. Relação com fazenda

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

## 43. Relação com cidade

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

## 44. Relação com caverna

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

# PARTE M — Flerte, relacionamento e casamento

## 45. Relacionamentos

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

## 46. Flerte

Regras:

```text
Flerte deve ser opção explícita.
NPCs casados não são candidatos a casamento.
NPCs disponíveis podem aceitar romance com jogador homem ou mulher, conforme design de personagem.
Romance não deve depender só de gifts repetidos.
Romance deve ter eventos, escolhas, quests e limites claros.
```

## 47. Casamento

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

## 48. UI/HUD do personagem

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
criação do personagem
raça/gênero/aparência
atributos
5 skill trees
arquétipo ativo
equipamentos
relacionamentos
pets
companions
status/resistências
```

## 49. Save/load

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
skill points
5 skill trees
nodes desbloqueados
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

# PARTE O — Roadmap de direção futura

Este documento é a visão ampla. Antes de implementação, ainda precisamos refinar documentos/seções específicas de direção para:

```text
nodes completos das 5 skill trees existentes
capstones completos das 5 skill trees existentes
sinergia final com atributos
fórmulas de atributos e stats derivados
curva de XP e level cap
progressão de skills/ranks dentro das trees
arquétipos inferidos/jobs vestíveis
ferramentas e upgrades
armas e dano
magia e MP
resistências e status
morte/derrota/Fonte de Anya
criação visual do personagem em sprites
raças jogáveis e passivas raciais
companions e pets integrados ao personagem
flerte, romance e casamento
UI/HUD do personagem
save/load do personagem
```

---

# PARTE P — Decisões fechadas

```text
O jogador não terá classe fixa estilo D&D.
O jogador evolui livremente por atributos, skills, equipamentos, ferramentas, armas e magia.
As 5 skill trees iniciais são: Melee/Guerreiro, Ranged/Caçador, Magic/Arcano, Survival/Sobrevivente e Crafting/Produção.
Social/romance/companions/pets são sistemas transversais, não a quinta árvore inicial.
Cada árvore tem 5 tiers.
Tier 5 é capstone.
Skills individuais podem ter ranks internos de 1 a 5, se isso não conflitar com tiers.
SkillPoint a cada 2 níveis continua como direção.
4 active slots continua como direção.
Passivas não ocupam active slot.
Respec ocorre na Fonte de Anya.
Classes/jobs serão arquétipos inferidos por distribuição de skills/atributos e poderão ser vestidos/ativados para bônus.
MP existe e é usado por magia.
MP regenera naturalmente de forma lenta baseada principalmente em Vontade.
Breath/Fôlego é stat separado de Stamina.
Cansaço é sistema próprio e afeta PlayerConditionManager.
Fome pressiona planejamento, mas não deve ser morte punitiva por padrão.
Força não aumenta yield de recurso sozinha; ela reduz esforço/golpes/dificuldade física.
Constituição não concede regeneração passiva de HP sozinha.
Regeneração de HP, se existir, vem de Survival, item, equipamento, comida ou Fonte.
Pets são separados de companions.
Cachorro pode apoiar combate sem ocupar slot de companion.
Flerte/casamento fazem parte dos sistemas do personagem/social.
Fonte de Anya conecta morte, respec, cura especial e progressão de lore.
Criação do personagem terá raça, gênero/apresentação e aparência limitada por sprites.
Raça/gênero/aparência não devem bloquear build, romance ou conteúdo central.
```

---

# PARTE Q — Pendências

```text
Validar no repo os nomes exatos de classes/arquivos/assets das skill trees implementadas.
Validar se a quinta árvore aparece no código/assets como Crafting, Produção, Production ou nome equivalente.
Validar lista final de nodes já implementados contra este documento.
Validar lista final de raças jogáveis iniciais contra o canon de raças de Vaalara.
Definir passivas raciais finais.
Definir se gênero e pronome serão campos separados.
Definir sprites base finais por raça e gênero/apresentação.
Definir paleta final de cabelo/pele/escamas.
Definir fórmulas finais de HP/MP/Stamina/Breath.
Definir curva de XP e level cap.
Definir progressão final de skill rank vs SkillPoint.
Definir nodes completos de cada uma das 5 skill trees.
Definir capstones completos das 5 skill trees.
Definir quantos arquétipos/jobs podem ficar ativos ao mesmo tempo.
Definir se troca de active slots/jobs ocorre na Fonte, casa, menu ou checkpoint.
Definir dano base por arma/ferramenta/magia.
Definir sistema de romance/casamento por NPC.
Definir relação entre cônjuge, companion e NPC de serviço.
Definir contratos concretos de save/load.
