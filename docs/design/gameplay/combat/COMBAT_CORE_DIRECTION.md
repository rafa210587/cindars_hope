# Cindar's Hope — Combat Core Direction

> **Status:** documento canônico de direção ampla do combate  
> **Local:** `docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/SPEC_SOURCE_MAP.md`  
> - `docs/design/SPECIFICATION_PROCESS.md`  
> - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`  
> - `docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md`  
> - `docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md`  
> - `docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_MONSTER_VISUAL_SPRITE_DIRECTION.md`  
> **Função:** consolidar a visão de combate do jogo: ação, movimento, Stamina, HP, MP, armas, magia, block, dodge, dash, janelas, vulnerabilidades, inimigos, companions, pets, bosses, HUD e validação.  
> **Não é spec implementável.** Este documento define direção de design. Specs futuras devem transformar isto em código/data assets.

---

## 0. Referência externa de design — Children of Morta

`Children of Morta` é uma referência útil de feeling e arquitetura, não uma fonte para copiar sistemas.

Elementos observados como referência:

```text
action RPG / hack'n'slash com abordagem roguelite
incursões em dungeons proceduralmente geradas
personagens/playstyles distintos
combate com ataques corpo a corpo, ranged, magia, block, stun, cura, evasão e passivas
progressão entre runs
identidade narrativa forte conectada à base/casa/família
```

Adaptação para Cindar's Hope:

```text
Cindar's Hope não terá personagens fixos como os Bergsons.
O jogador é um personagem livre, com build construída por atributos, skill trees, gear, armas, magia, companions e pets.
O feeling desejado é similar no ritmo de dungeon action: leitura de inimigos, hordas controladas, skills claras, evasão, block, janelas de punição e progressão persistente.
A estrutura emocional/narrativa vem de Vaalara, fazenda, Fonte de Anya, cidade, companions, pets e caverna.
```

Regra:

```text
Usar Children of Morta como inspiração de ritmo, clareza, run loop e diversidade de kits.
Não copiar nomes, personagens, habilidades, valores, textos, bosses, mapas ou sistemas proprietários.
```

---

# PARTE A — Visão geral do combate

## 1. Fantasia de combate

O combate de Cindar's Hope deve ser:

```text
tático
rápido o suficiente para parecer action RPG
legível o suficiente para depender de leitura, não reflexo impossível
punitivo quando o jogador erra repetidamente
possível quando o jogador entende inimigo, Stamina, janelas e preparação
fortemente conectado a build, gear, companions, pets e consumíveis
```

O combate não deve ser:

```text
spam de ataque sem custo
spam de dodge/dash sem custo
HP sponge sem janelas
stunlock permanente
controle injusto do jogador
dano inevitável sem telegraph
arena lotada sem counterplay
```

## 2. Pilares

```text
1. Stamina é decisão.
2. HP é consequência de erro, build, armor e preparo.
3. MP abre magia, suporte, dano e controle, mas com custo claro.
4. Block é defesa forte, mas cara.
5. Dodge/Dash são fortes, caros e devem ser usados com intenção.
6. Vulnerabilidades e janelas recompensam leitura.
7. Companions/pets ajudam, mas não jogam pelo jogador.
8. Bosses são fases, telegraphs e escolhas, não só vida alta.
9. A caverna deve ficar mais difícil naturalmente, sem reduzir densidade por causa de Stamina cara.
```

## 3. Loop de combate desejado

```text
entrar na sala
ler composição do pack
identificar ameaça principal
usar posicionamento normal antes de gastar Dash/Dodge
atacar com ritmo
bloquear quando fizer sentido
usar Dodge/Dash para evitar ataques críticos ou reposicionar
explorar MinorOpening/CriticalWindow/CoreExposed
usar companion/pet/consumível quando necessário
coletar recompensa ou decidir recuar
```

---

# PARTE B — Inputs centrais

## 4. Inputs de combate

```text
Movimento: WASD ou direcional equivalente
Ataque leve: botão primário
Ataque pesado/charged: segurar ou botão secundário conforme spec final
Dash: Space + direção
Dodge: double tap direcional
Block: Left Shift
Active skills: 4 slots equipáveis
Interação/loot: botão dedicado
Troca de arma/ferramenta: hotbar/atalhos
```

Regra:

```text
Dash, Dodge e Block não ocupam active slot.
Active slots são para skills equipáveis: ataques especiais, magias, suporte, utilidade ou técnicas.
```

## 5. Resposta de input

O combate precisa de responsividade alta.

Regras:

```text
input buffer curto para ataque, dodge, dash e block
cancelamento limitado, nunca irrestrito
prioridade de Dodge/Dash deve ser clara
block deve levantar rápido, mas não instantâneo se o jogador estiver em recovery pesado
ataques pesados devem comprometer o jogador
```

Direção:

```text
O jogador deve sentir que perdeu por decisão ruim, posicionamento ruim ou leitura ruim, não por input engolido.
```

---

# PARTE C — Recursos principais em combate

## 6. HP

HP representa margem de erro.

Fórmula canônica do jogador:

```text
HPMax = BaseHP + Level*2 + Constituição*5 + EquipmentHP + SkillHP + BuffHP
```

Regras:

```text
HP não deve escalar alto demais por Constituição.
HP Regen não vem de Constituição pura.
HP Regen vem de skill, item, comida, magia, Fonte ou efeito explícito.
```

## 7. MP

MP representa reserva mágica.

```text
MPMax = BaseMP
      + Level*MPPerLevel
      + Vontade*MPPerWill
      + Inteligência*MPPerInt
      + EquipmentMP
      + SkillMP
      + BuffMP
```

Regras:

```text
MP Regen natural é lenta.
Vontade melhora MP e MP Regen, mas não gera spam mágico sozinha.
Magia forte exige build, item, custo, cooldown e telegraph.
```

## 8. Stamina

Stamina é o recurso físico imediato do combate.

Fórmula canônica:

```text
StaminaMax = BaseStamina
           + Level*0.6
           + Constituição*2.0
           + Força*1.5
           + Destreza*1.0
           + EquipmentStamina
           + SkillStamina
           + BuffStamina
```

Valores de referência:

```text
Light melee com Espada de Aço: 25 Stamina
Heavy melee com Espada de Aço: 40 Stamina
Dash: 40 Stamina
Dodge: 40 Stamina
Block hold: 18 Stamina/s
```

Regras:

```text
Stamina deve ser restritiva no começo.
Com tempo, skills, gear, comida, companions e domínio reduzem a fricção.
Mesmo no late game, Stamina não deve virar irrelevante.
```

## 9. Stamina Regen

```text
StaminaRegenOutOfCombat = BaseRegen
                         + Constituição*0.15
                         + Destreza*0.20
                         + SurvivalBonus
                         + EquipmentBonus
                         + FoodBuff
                         - ArmorPenalty

StaminaRegenInCombat = StaminaRegenOutOfCombat * CombatMultiplier
```

Direção:

```text
CombatMultiplier = 0.35 a 0.50 inicialmente.
Fome e Cansaço reduzem regen.
Armadura pesada pode reduzir regen ou aumentar custo.
```

---

# PARTE D — Movimento em combate

## 10. Movimento normal

Movimento normal é a primeira defesa.

Regras:

```text
O jogador deve conseguir evitar parte de ataques simples só andando bem.
Nem todo ataque deve exigir Dodge ou Dash.
Ataques fortes devem ter telegraph e área clara.
```

## 11. Dash

```text
Input: Space + direção
Custo base: 40 Stamina
Função: reposicionamento forte
Não ocupa active slot
```

Influenciado por:

```text
Destreza
Survival / Passo de Impulso
Ritmo Controlado
equipamento leve/pesado
cansaço/status
```

Regras:

```text
Dash não é esquiva universal.
Dash serve para reposicionar, sair de zona, atravessar pequena distância ou criar espaço.
Dash pode ter i-frame baixo ou nenhum, conforme spec final.
Dash deve ter cooldown/recovery.
```

## 12. Dodge

```text
Input: double tap direcional
Custo base: 40 Stamina
Função: evasão de timing
Não ocupa active slot
```

Influenciado por:

```text
Destreza
Survival / Reflexo de Esquiva
Ritmo Controlado
cansaço/status
```

Regras:

```text
Dodge deve ter janela de invulnerabilidade curta.
Dodge deve ter recovery.
Dodge não deve substituir movimentação normal.
Dodge caro exige telegraph justo dos inimigos.
```

## 13. Collision e body blocking

Direção:

```text
Player e inimigos têm footbox clara.
Inimigos pequenos podem cercar, mas não travar completamente o jogador sem rota.
Inimigos grandes podem bloquear passagem como papel tático.
Bosses usam colliders customizados.
```

---

# PARTE E — Ataques do jogador

## 14. Ataque leve

Função:

```text
dano principal de baixo compromisso
combos simples ou cadência curta
baixo/moderado posture damage
custo relevante de Stamina
```

Regras:

```text
Ataque leve não deve ser gratuito.
Ataque leve deve ter recovery suficiente para punir spam em inimigos perigosos.
Armas leves podem atacar mais rápido, mas ainda gastam Stamina.
```

## 15. Ataque pesado / charged attack

Função:

```text
alto dano
alto posture damage
maior risco
maior custo de Stamina
boa ferramenta contra armor, constructos, elites e janelas
```

Regras:

```text
Ataque pesado deve comprometer movimento/recovery.
Ataque pesado deve ser recompensador em CriticalWindow/CoreExposed.
Ataque pesado não deve ser sempre melhor que ataque leve.
```

## 16. Combos

Direção inicial:

```text
Combos devem ser simples.
Não criar lista longa de inputs.
Progressão vem mais de skills, armas, janelas e timing do que de combo complexo.
```

Possibilidades futuras:

```text
terceiro hit com stagger maior
ataque leve -> pesado como finisher
ataque pós-block perfeito
ataque pós-dodge bem sucedido
ataque pós-companion setup
```

---

# PARTE F — Block

## 17. Block base

```text
Input: Left Shift
Custo: hold + impacto
Não ocupa active slot
Desbloqueio/melhoria: Melee / Guerreiro
```

Regras:

```text
Block é forte, mas caro.
Block reduz dano, mas drena Stamina.
Sem Stamina, Block quebra ou perde eficiência.
Block não deve resolver todo tipo de ataque.
```

## 18. Block Power

```text
BlockedDamage = IncomingDamage * (1 - BlockPower)
```

Influenciado por:

```text
Block rank
escudo/arma
Melee/Guerreiro
material do equipamento
capstone Kanthor/Kaand
```

## 19. Block impact

O custo de Stamina por impacto bloqueado usa dano pós-armadura/mitigação.

```text
MitigatedDamageForStamina = max(MinDamageForStamina, IncomingRawDamage - ArmorMitigationValue)
IncomingDamageRatio = MitigatedDamageForStamina / PlayerMaxHP
BlockImpactStaminaCost = PlayerMaxStamina * IncomingDamageRatio * BlockImpactMultiplier * (1 - BlockStability)
```

Se houver resistência percentual e armor flat:

```text
MitigatedDamageForStamina = max(
  MinDamageForStamina,
  (IncomingRawDamage * (1 - PhysicalResistance)) - ArmorFlatMitigation
)
```

Regras:

```text
Golpes que ameaçam muito a vida depois da armadura drenam muita Stamina.
Golpes que a armadura absorve bem drenam menos Stamina.
Armor é valiosa para builds de Block.
```

## 20. Perfect Block / Counter futuro

Direção:

```text
Perfect Block pode existir como skill/upgrade.
Deve exigir timing curto.
Pode reduzir Stamina drain, abrir MinorOpening/CriticalWindow ou ativar Contra-Ataque.
Não deve ser obrigatório no early game.
```

---

# PARTE G — Dano, armor e resistências

## 21. Dano físico

```text
BaseAttack = BaseAttackValue
           + Força*BaseAttackPerStr
           + Destreza*BaseAttackPerDexSmall
           + Level*BaseAttackPerLevel

AttackDamage = (BaseAttack + WeaponDamage + EquipmentFlatDamage)
             * WeaponScaling
             * (1 + SkillDamageBonus)
             * (1 + BuffDamageBonus)
             * ContextMultiplier
             * EnemyResistanceMultiplier
```

Regra:

```text
Força domina dano bruto.
Destreza ajuda armas leves, cadência, recovery e crítico condicional.
Arma/material/skill devem importar mais que atributo isolado.
```

## 22. Armor e mitigação

```text
Defense = BaseDefense + Armor + Constituição*0.75 + SkillDefense + BuffDefense
```

Regras:

```text
Armor é a principal mitigação flat.
Constituição não substitui equipamento.
Defense/Armor não deve zerar dano de packs inteiros sem regra de dano mínimo.
```

## 23. Resistências

Tipos principais:

```text
Physical
Fire
Ice
Lightning
Water/Nature
Poison
Bleed
Shadow/Nyx
Arcane
Blackstone/Corruption
Fear/Mental
Heat
Cold
```

Regras:

```text
Resistência reduz dano/efeito.
Imunidade deve ser rara e justificada por boss/lore.
Vulnerabilidade deve abrir counterplay, não ser requisito único.
```

---

# PARTE H — Crit, janelas e vulnerabilidades

## 24. Tipos de abertura

```text
MinorOpening
  abertura comum/curta.
  Não garante crítico automático.
  Pode dar +crit chance, +dano moderado, +posture damage ou oportunidade tática.

CriticalWindow
  janela clara, mais rara ou mais arriscada.
  Pode garantir crítico automático.
  Exige telegraph claro ou execução.

CoreExposed / BossMechanicWindow / StaggeredWindow
  janela especial de mecânica, exposição de núcleo, quebra de postura ou fase de boss.
  Pode garantir crítico automático + bônus moderado.
```

## 25. Critical Hit

Direção:

```text
Crítico normal depende de chance.
CriticalWindow pode garantir crítico.
CoreExposed pode garantir crítico + bônus moderado.
Crítico nunca deve virar dano infinito.
```

Multiplicadores conceituais:

```text
MinorOpening damage: x1.10 a x1.25 ou +15% a +35% crit chance.
Vulnerability damage: x1.25 a x1.50.
Critical hit: x1.50 a x2.00 conforme arma/build.
Critical + vulnerability: x2.00 a x2.50.
```

## 26. Vulnerabilidades

Cada inimigo deve declarar pelo menos um caminho de counterplay:

```text
ElementVulnerability
StatusVulnerability
AttackTypeVulnerability
WeaponVulnerability
BehavioralVulnerabilityWindow
```

Regra:

```text
Nem todo inimigo precisa ser vulnerável a tudo.
Todo inimigo precisa de pelo menos uma leitura/recompensa clara.
```

## 27. Telegraphs

Todo ataque relevante deve ter:

```text
windup
sinal visual/sonoro
execução
recovery
janela de vulnerabilidade quando aplicável
categoria da janela
```

Bosses precisam de telegraph mais claro que mobs comuns.

---

# PARTE I — Status e controle

## 28. Status negativos permitidos

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

Regras:

```text
ConfusionLite não remove controle total do jogador.
Controle permanente não existe.
Stun forte exige telegraph/cooldown.
Status de boss deve ter counterplay.
DurabilityStress não destrói item permanentemente sem spec própria.
```

## 29. Posture / stagger

Posture representa estabilidade do alvo.

```text
PostureDamage = StaggerPower
              * AttackPostureMultiplier
              * VulnerabilityMultiplier
              * SkillPostureMultiplier
```

Regras:

```text
Ataques pesados causam mais posture damage.
Hammer/Axe/Pickaxe podem ser melhores contra armor/constructos.
Posture break pode abrir CriticalWindow ou StaggeredWindow.
Bosses podem ter posture por fase, não barra única simples.
```

## 30. Guard Break

GuardBreak é ferramenta contra Block/Shield.

Usos:

```text
inimigos anti-block
elites tank
bosses
skills do jogador
```

Regras:

```text
GuardBreak precisa de telegraph claro.
GuardBreak não deve ignorar toda defesa sem aviso.
```

---

# PARTE J — Armas e estilos

## 31. Tipos iniciais

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

## 32. Papéis

```text
Sword: equilíbrio, bom contra aberturas médias.
Axe: dano forte, bom contra madeira/raízes/armadura média.
Hammer: posture, constructos, armor, stagger.
Spear: alcance, pierce, asas/pontos expostos.
Bow: distância, flying/floating, marcação.
Dagger: velocidade, crítico condicional, risco alto.
Staff: magia, suporte, dano arcano, defesa mágica.
ToolAttack: utilitário, emergencial, não substitui arma dedicada.
```

## 33. Materiais e tiers

Direção:

```text
Tiers maiores aumentam dano/eficiência/utilidade, mas não eliminam custo.
Prata deve ser boa contra mortos-vivos/sombras/maldições.
Mithril tende a ser leve/durável/eficiente.
Materiais raros podem alterar stamina cost, damage type ou vulnerability interaction.
```

---

# PARTE K — Magia

## 34. Magia em combate

Magia usa MP e deve ter função clara.

Tipos:

```text
projétil ofensivo
zona elemental
barreira/selo
cura limitada
purificação
controle leve
interação ambiental
buff/debuff
```

Regras:

```text
Magia não substitui todos os estilos físicos.
Magia forte exige MP, cooldown, cast time ou risco.
Cura mágica deve ser limitada, cara e com cooldown.
Barreiras não bloqueiam tudo.
```

## 35. Anya e Senya

```text
Semente Arcana de Anya: suporte, cura, purificação, barreira, eco de recuperação.
Semente Arcana de Senya: dano mágico, efeitos elementais, overload/crit mágico controlado.
```

Regra:

```text
Capstones são escolhas fortes e exclusivas.
Não devem invalidar comida, poções, companions ou gear.
```

---

# PARTE L — Inimigos, packs e bosses

## 36. Monstros comuns

Função:

```text
ensinar padrões
pressionar posicionamento
formar packs
criar atrito de HP/Stamina
proteger recursos/tesouros
```

Regras:

```text
Comum isolado não deve ser trivial sempre.
Comum em pack deve mudar o problema tático.
Comum não deve exigir execução perfeita.
```

## 37. Elites

Função:

```text
testar uma mecânica
forçar leitura
punir spam
recompensar vulnerabilidade/janela
criar mini-clímax de sala
```

Regras:

```text
Elite deve ter pelo menos 2 ações relevantes.
Elite deve ter janela clara.
Elite pode exigir uso de skill, comida, companion/pet ou recuo.
Elite não deve ser apenas comum com HP alto.
```

## 38. Packs

Packs precisam ter lógica:

```text
mesma facção
ecosistema
predador/presa
culto/ritual
guardião/recurso
constructo/ruína
mímico/tesouro
Pedra Negra/corrupção
```

Regras:

```text
Packs devem ser densos e desafiadores.
O active budget não deve ser reduzido só porque Stamina é cara.
Skills, companions, pets, consumíveis e domínio do jogador mitigam naturalmente a pressão.
```

## 39. Bosses

Bosses devem ter:

```text
2-3 fases
ataques bem telegráficos
mudança de movimento ou arena
janelas claras
vulnerabilidades por fase
pressão de Stamina/HP/MP
momentos de recuperação controlados
```

Bosses não devem depender só de:

```text
HP alto
dano inevitável
adds infinitos
controle permanente
```

---

# PARTE M — Companions e pets

## 40. Companions

Papéis possíveis:

```text
Tank
Ranged
Healer/Support
Controller
Miner/Hybrid
```

Regras:

```text
Companion ajuda, mas não joga sozinho.
Companion pode abrir MinorOpening/CriticalWindow em casos específicos.
Companion pode segurar pressão, mas não substituir decisão do jogador.
```

## 41. Pets

Pets são sistema próprio.

Cachorro:

```text
apoio contra swarm
interrupção leve
marcação
atração curta de pequenos inimigos
detecção de perigo/treasure trap
```

Gato:

```text
sorte/achados
detecção de anomalia
bônus indiretos
vínculo social/fazenda
```

Regras:

```text
Cachorro não ocupa slot de companion.
Pet não deve tankar boss.
Pet não deve ser obrigatório no início.
```

---

# PARTE N — Consumíveis e preparo

## 42. Consumíveis

Tipos:

```text
comida de Stamina
comida de HP
poção de cura
poção de MP
antídoto
resistência elemental
óleo de arma
bombas leves futuras
Fruto de Mana raro
```

Regras:

```text
Consumível deve ser parte do preparo de run.
Consumível não deve trivializar boss.
Comida deve conversar com fazenda/cozinha.
Fruto de Mana é raro e especial, não item comum de spam.
```

## 43. Loadout de entrada

Antes da caverna, o jogador deve considerar:

```text
arma
ferramenta
comida
poções
resistência elemental
companion
pet
skills ativas
equipamento
objetivo da run
```

---

# PARTE O — HUD e feedback

## 44. HUD de combate

Mostrar:

```text
HP
MP quando relevante
Stamina
Fome/Cansaço compacto
active slots
hotbar
Dash/Dodge feedback
Block state
status negativos
buffs
arma/ferramenta ativa
companion/pet state quando relevante
```

Não mostrar:

```text
Breath/Fôlego como atributo/recurso
BR
números internos demais na HUD principal
```

## 45. Feedback visual

O jogador precisa ler:

```text
hit recebido
block bem sucedido
block quase quebrando
Stamina insuficiente
MinorOpening
CriticalWindow
CoreExposed
vulnerabilidade elemental
status aplicado
boss phase transition
companion setup
pet interrupt
```

Direção:

```text
Pixel art deve ter telegraph claro por cor, silhueta, antecipação e efeitos.
Feedback não pode poluir tela pequena.
```

## 46. Feedback sonoro

Usar áudio para:

```text
windup de elite/boss
block impact
Stamina baixa
critical hit
vulnerability hit
cast perigoso
pet/companion trigger
```

---

# PARTE P — Integração com caverna

## 47. Procedural e combate

A geração procedural deve respeitar:

```text
active enemy budget
spawn anchors seguros
rotas de fuga
salas de tesouro com risco
packs com lógica
hazards por bioma
boss gates
checkpoints
```

## 48. Biomas

Cada bioma deve influenciar combate:

```text
frio: Chill, ColdStress, menor regen, resistência necessária
fogo: Burn, HeatStress, zona de dano
ruínas: constructs, traps, overcharge, puzzles
abismo: Fear, Shadow/Nyx, controllers
núcleo: corrupção, Pedra Negra, bosses/endgame
```

## 49. Nível 101

```text
conteúdo endgame
bosses em sequência
informação final de Anya
libertação parcial do poder de Anya
recuperação controlada entre encontros
sem farm trivial
```

---

# PARTE Q — Telemetria e validação

## 50. Métricas de Play Mode

Registrar em playtest:

```text
TTK por inimigo/papel
Stamina gasta por categoria
Stamina recuperada
HP perdido
MP gasto
número de Dodges/Dashes
tempo segurando Block
Block impacts
hits em MinorOpening/CriticalWindow/CoreExposed
consumíveis usados
companion/pet triggers
deaths/retreats
```

Regra:

```text
Não transformar consumo de Stamina em percentual fixo de spec.
Usar telemetria para detectar extremos: trivial, injusto, esponja, sem counterplay ou stamina irrelevante.
```

## 51. Validação humana

A validação humana deve observar:

```text
se o combate parece justo
se o custo de Stamina é sentido
se Dodge/Dash são escolhas fortes
se Block é útil, mas não dominante
se telegraphs são legíveis
se packs são desafiadores sem avalanche injusta
se companions/pets ajudam sem resolver tudo
se bosses têm fases claras
```

---

# PARTE R — Decisões fechadas

```text
Combat Core deve seguir referência de feeling similar a Children of Morta: action RPG/hack'n'slash roguelite, runs, kits claros, dungeons perigosas e progressão persistente.
Cindar's Hope não copia personagens fixos; usa personagem livre com build.
Breath/Fôlego não existe como atributo/recurso.
Breath pode existir em nome de ataque de sopro de criatura.
HP do jogador escala devagar.
Constituição não é atributo defensivo universal.
Stamina usa Level + CON + FOR + DES.
Light melee com Espada de Aço custa 25 Stamina.
Heavy melee com Espada de Aço custa 40 Stamina.
Dash custa 40 Stamina.
Dodge custa 40 Stamina.
Block hold custa 18 Stamina/s.
Block impact usa dano pós-armadura/mitigação contra HP máximo.
Dash, Dodge e Block não ocupam active slot.
4 active slots são para skills equipáveis.
Nem toda abertura dá crítico automático.
MinorOpening, CriticalWindow e CoreExposed são categorias diferentes.
Active combat budget da caverna não foi reduzido por causa da Stamina cara.
Companions/pets/gear/skills/consumíveis mitigam naturalmente a dificuldade com progressão.
```

---

# PARTE S — Pendências para specs futuras

```text
Definir CombatController input contract.
Definir PlayerAttackController.
Definir WeaponActionDataSO.
Definir StaminaCostProfileSO.
Definir BlockController e BlockImpact formula final.
Definir Dash/Dodge timing, i-frames, cooldown e recovery.
Definir DamageType final.
Definir Armor/Defense final.
Definir CriticalHit contract.
Definir MinorOpening/CriticalWindow/CoreExposed data contract.
Definir EnemyAction telegraph/recovery/window contract.
Definir UI feedback de Stamina baixa, block, critical window e vulnerability.
Definir integração com companions/pets.
Definir Play Mode telemetry para TTK/Stamina/HP/MP.
Definir validações Unity por spec.
