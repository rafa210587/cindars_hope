# Cindar's Hope — Enemy Behaviors Direction

> **Status:** documento canônico transversal de comportamento de inimigos  
> **Local:** `docs/design/gameplay/enemies/ENEMY_BEHAVIORS_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/SPEC_SOURCE_MAP.md`  
> - `docs/design/SPECIFICATION_PROCESS.md`  
> - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`  
> - `docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md`  
> - `docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md`  
> - `docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`  
> **Função:** definir como inimigos pensam, percebem, escolhem alvos, se movem, atacam, gastam Stamina/MP, reagem ao jogador, coordenam packs e podem ser reutilizados em contextos futuros fora da caverna.  
> **Não é spec implementável.** Este documento define direção de design. Specs futuras devem converter isto em dados e sistemas.

---

## 0. Escopo atual e escopo futuro

Este documento é transversal, mas nem todo contexto descrito aqui deve virar spec agora.

### Escopo atual

```text
caverna
boss gates
nível 101
monstros já previstos no roster da caverna
packs de monstros
bosses
elites
reação a player, companions e pets em combate
EnemyBrain / EnemyAction / Movement / Reaction / PackCoordination
```

### Escopo futuro explícito

```text
invasões da fazenda
defesa da fazenda
dano a crops, animais, máquinas, cercas ou estruturas
inimigos em cidade durante eventos hostis
áreas futuras de mundo externo
raids narrativas fora da caverna
```

Regra:

```text
Specs atuais não devem implementar farm invasion, town hostile event ou world enemy events apenas porque este documento cita esses temas.
Essas seções existem para evitar conflito futuro e orientar arquitetura extensível.
Farm invasion só deve virar spec quando o roadmap explicitamente abrir esse tema.
```

---

## 1. Regra anti-duplicação

Este documento não deve duplicar fórmulas de atributos, dano, HP, Stamina, MP ou Block.

Fontes canônicas:

```text
PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
  fórmulas do jogador e regras de atributos derivados.

COMBAT_CORE_DIRECTION.md
  feeling, input, Stamina em combate, Dash, Dodge, Block, movimento, HUD e validação.

CAVE_MONSTER_ROSTER_DIRECTION.md
  roster de monstros da caverna, stats, moves, behaviors, traits, ataques, drops e scaling.

CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
  vulnerabilidades, janelas, TTK, active combat budget e telemetria de caverna.
```

Regra:

```text
Se houver conflito de stat/roster de monstro, o roster da caverna vence para monstros da caverna.
Se houver conflito de fórmula de combate, o documento de atributos derivados vence.
Se houver conflito de input/movimento do jogador, Combat Core vence.
Se houver conflito de vulnerabilidade/janela da caverna, Cave Combat Balance vence.
Enemy Behaviors define como os dados são usados pelo cérebro inimigo; não redefine números canônicos.
```

---

# PARTE A — Princípios gerais

## 2. Filosofia

Inimigos devem parecer perigosos por comportamento, leitura e combinação de ações, não apenas por números altos.

Inimigo bom deve:

```text
ter intenção legível
ter função clara no encontro
ter pelo menos uma forma de counterplay
usar telegraph em ataques relevantes
ter recovery/janela quando usa ação forte
usar Stamina/MP/cooldown de forma previsível
interagir com pack, ambiente, objetivo e facção
reagir a Dash, Dodge, Block, ranged, magic, pet e companion sem invalidar essas escolhas
```

Inimigo ruim seria:

```text
perseguir sem parar sem custo
atacar instantaneamente sem telegraph
controlar o jogador sem counterplay
ter HP alto sem janela
ignorar colisão/pathing de forma injusta
spammar dash/blink/leap sem recovery/cooldown
colar instantaneamente no jogador após Dash longo
reagir a toda estratégia do jogador com counter perfeito
```

## 3. Comportamentos são injetáveis, não totalmente fixos

O comportamento de um inimigo não deve ser uma classe rígida única.

Direção de arquitetura:

```text
EnemyData define stats, visual, família, facção, drops e identidade.
EnemyBrainProfile define como o inimigo toma decisões.
EnemyBehaviorProfile define módulos de comportamento injetáveis.
EnemyMovementProfile define Move oficial e parâmetros de movimento.
EnemyActionSet define ações disponíveis.
EnemyActionSO define cada ação em dados.
EnemyReactionRules definem respostas a player/companion/pet/status.
EnemyPackRole define função dentro do pack.
EnemyObjectiveProfile existe apenas para eventos objetivos/futuros.
```

Regra:

```text
Um mesmo monstro pode trocar comportamento por contexto sem duplicar o monstro inteiro.
Exemplo: um goblin pode ser Guard em uma sala, PackFlanker em outra, ResourceThief em invasão futura, ou RetreatAndCall em emboscada.
O roster define a identidade do monstro; os perfis injetados definem como ele age naquele encontro.
```

## 4. Papéis de inimigo

Papéis globais são tags de intenção, não classes rígidas.

| Papel | Função | Como joga | Counterplay esperado |
|---|---|---|---|
| Chaser | pressionar o jogador | aproxima e força reação | kiting curto, block, dodge, terreno |
| Guard | proteger ponto | segura área/anchor | puxar, flanquear, ranged, janela após ataque |
| Ranged | punir distância | mantém range e projéteis | linha de visão, Dash, interrupção |
| Caster | aplicar magia/controle | cast, zona, debuff, summon | interrupt, line of sight, cooldown |
| Burrower | emboscar | some/reaparece com tell | ler solo, punir emerge |
| Swarm | pressionar espaço | muitos pequenos | área, pet, posicionamento |
| Tank | bloquear avanço | lento, resistente, posture | heavy, hammer, flank, magia |
| Controller | mexer com espaço/estado | slow, root, fear, zona | resistências, interrupt, cooldown |
| TreasureTrap | punir ganância | finge loot/interação | leitura, pet/gato futuro, telegraph |
| Elite | testar mecânica | 2-3 ações relevantes | janela clara, execução |
| Boss | clímax/fase | padrões por fase | aprender fase, usar build/preparo |
| Ritualist | proteger/canalizar evento | canaliza, invoca, defende anchor | interromper ritual |
| Summoner | multiplicar pressão | chama adds controlados | focar caster, cooldown |
| HazardLurer | usar ambiente | puxa para hazard | posicionamento |
| LoreGuardian | encontro narrativo | pode não ser só matar | interação, purificação, resistência |

Papéis futuros não implementáveis agora:

| Papel futuro | Uso futuro | Status |
|---|---|---|
| Invader | inimigo que entra na fazenda/evento | futuro |
| Raider | rouba/recolhe recurso | futuro |
| CropDestroyer | ameaça crop/solo | futuro |
| LivestockPredator | ameaça animais | futuro |
| ResourceThief | foge com item/recurso | futuro |

Regra:

```text
Todo inimigo deve ter PrimaryRole.
SecondaryRoles são opcionais.
PrimaryRole define prioridade de alvo, movimento, ação preferida e retirada.
SecondaryRoles adicionam variação sem reescrever o inimigo.
```

---

# PARTE B — EnemyBrain modular

## 5. Camadas do EnemyBrain

EnemyBrain deve ser composto por módulos claros.

| Módulo | Responsabilidade | Observação |
|---|---|---|
| PerceptionModule | descobre alvos válidos | sem hearing/som por enquanto |
| IntentModule | escolhe intenção atual | atacar, guardar, recuar, castar, proteger |
| TargetingModule | escolhe alvo | player, companion, pet, anchor, objetivo |
| MovementModule | move usando Move oficial | GroundChase, KiteRanged etc. |
| ActionSelectorModule | escolhe ação | score por range, custo, cooldown e contexto |
| ResourceModule | valida STA/MP | impede spam de ações fortes |
| CooldownModule | controla repetição | evita loops injustos |
| ReactionModule | responde a eventos | dano, block, dodge, dash, pet, companion |
| PackModule | coordena aliados | call, flanker, leader, protect |
| LeashModule | limita perseguição | evita cheese e avalanche |
| PhaseModule | bosses/elites especiais | fases e transições |
| ObjectiveModule | objetivos de evento | futuro para farm/cidade |

Regra:

```text
Módulos são injetáveis por EnemyBrainProfile.
Nem todo inimigo precisa de todos os módulos.
Comum simples pode usar módulos mínimos.
Elite/boss usa mais módulos.
Farm/city objective modules são futuros e não entram em specs atuais sem decisão explícita.
```

## 6. Estados globais

Estados são estados de execução, não papéis.

| Estado | Significado | Saída típica |
|---|---|---|
| Idle | parado/inativo | percebe alvo ou trigger |
| Patrol | anda entre pontos | alerta, retorna ou engaja |
| Guard | protege posição/anchor | engaja dentro de raio |
| Suspicious | percebeu algo parcial | investiga ou volta |
| Alert | alvo detectado | escolhe intenção |
| Engage | em combate | ação/reposition/recover |
| Reposition | busca posição melhor | action ou retreat |
| Recover | após ação/reação | volta ao selector |
| Retreat | recua taticamente | cast, call, reset parcial |
| CallForHelp | chama pack próximo | cooldown e limite |
| ProtectObjective | protege anchor/ritual/recurso | futuro fora da caverna |
| AttackObjective | ataca objetivo não-player | futuro fora da caverna |
| Cast | canaliza magia/efeito | active/recovery/interruption |
| Staggered | postura quebrada | janela de punição |
| Stunned | controle temporário | recuperação |
| Fleeing | fuga por medo/baixa vida | saída, call ou morte |
| Enraged | comportamento agressivo | phase/rage cooldown |
| PhaseTransition | boss/elite muda fase | nova action set |
| Dead | morto | drop, XP, cleanup |

Regras:

```text
Todo inimigo não-boss deve ter pelo menos Idle/Patrol ou Guard, Alert, Engage, Recover e Dead.
Elites devem ter Reposition, Enraged ou outra camada tática.
Bosses devem ter PhaseTransition.
Estados de Objective são futuros fora da caverna.
```

## 7. PerceptionModule

PerceptionModule define o que o inimigo consegue perceber.

Campos conceituais:

| Campo | Significado | Direção |
|---|---|---|
| SightRadius | raio de visão/ativação | principal forma de detectar |
| SightAngle | cone/ângulo de visão | opcional, útil para patrulha |
| LineOfSightRequired | exige linha limpa | recomendado para maioria |
| DamageAggro | ativa ao sofrer dano | sim para quase todos |
| AllyCallRadius | recebe chamado de aliado | controlado por active budget |
| ObjectiveAwareness | percebe anchor/objetivo | atual na caverna, futuro fora dela |
| PetAwareness | considera pet como alvo/evento | opcional |
| CompanionAwareness | considera companion | recomendado em combate |
| ArenaAwareness | boss percebe arena toda | apenas boss/arena |
| TriggerAwareness | ativa por interação/proximidade | treasure traps, ambushes |
| LightDarknessModifier | modificador futuro | não implementar agora salvo spec própria |

Removido por decisão atual:

```text
HearingRadius / audição não será elemento de IA por enquanto.
Specs não devem implementar aggro por som sem nova decisão.
```

Regras:

```text
Inimigo não deve ativar através de paredes sem regra específica.
AllyCallRadius deve ser limitado para evitar avalanche injusta.
TreasureTrap pode ignorar percepção comum até trigger específico.
Boss pode ter ArenaAwareness.
```

## 8. IntentModule

Intent é o objetivo de curto prazo.

Intents possíveis:

```text
AttackPlayer
AttackCompanion
HarassPet
GuardAnchor
ProtectAlly
Flank
Kite
CastDamage
CastControl
Summon
Retreat
CallForHelp
RecoverStamina
RecoverMP
UseHazard
PhaseShift
Ambush
Flee
```

Intents futuros, não escopo atual:

```text
AttackFarmObject
StealResource
CorruptTile
HarassLivestock
BreakFence
ChannelFarmRitual
EscapeWithLoot
```

Regra:

```text
Intent não executa nada sozinho.
Intent informa MovementModule e ActionSelectorModule.
```

## 9. TargetingModule

Escolhe alvo com base em prioridade e papel.

Alvos possíveis atuais:

```text
Player
Companion
Pet
SummonedAlly futuro
Anchor/RitualObject da caverna
BossMechanicObject
```

Alvos futuros:

```text
CropTile
FarmAnimal
FarmStructure
Machine
StorageObject
TownCivilian
```

Fatores de target score:

```text
dano recente recebido
proximidade
linha de visão
alvo vulnerável
alvo bloqueando
alvo usando magia/ranged
companion tankando
pet interrompendo
objetivo protegido
fase do boss
```

Regra:

```text
Nem todo inimigo deve focar sempre o jogador.
Também não deve haver regra universal de sempre focar healer/support/companion.
Targeting deve variar por papel, inteligência do inimigo e contexto.
```

---

# PARTE C — MovementModule

## 10. Fonte canônica de Moves

Para inimigos da caverna, nomes oficiais de `Move` vêm de:

```text
CAVE_MONSTER_ROSTER_DIRECTION.md
```

Moves oficiais atuais:

```text
GroundChase
GroundPatrol
GuardStationary
KiteRanged
CasterKeepAway
BurrowAmbush
SwarmErratic
TankSlowPush
PhaseShortBlink
Leaper
FloatingSlow
FloatingOrbit
TreasureIdleAmbush
PackFlanker
PackLeader
RetreatAndCall
ProtectAnchor
CircleStrafe
ChargeLine
HazardLure
BossArenaControl
BossPhaseShift
```

Regra:

```text
Specs não devem criar sinônimos se já existe Move oficial equivalente.
Para ambientes fora da caverna, reutilizar Moves oficiais quando fizer sentido.
Moves novos só devem ser criados se o comportamento não existir no roster.
```

## 11. Explicação declarativa dos Moves oficiais

| Move | Comportamento | Quando usar | Cuidado |
|---|---|---|---|
| GroundChase | persegue por caminho direto | melee comum | precisa recovery/ataque legível |
| GroundPatrol | anda entre pontos | guarda, patrulha | não deve parecer aleatório demais |
| GuardStationary | fica parado/curto raio | sentinela, turret, baú | precisa forma de puxar/punir |
| KiteRanged | mantém distância e atira | arqueiro/cuspidor | evitar kiting infinito |
| CasterKeepAway | recua pouco e castas | caster frágil | casts precisam de windup |
| BurrowAmbush | some/reaparece do solo | verme, larva, predador | tell no chão obrigatório |
| SwarmErratic | movimento rápido irregular | morcegos, enxames | não travar jogador sem saída |
| TankSlowPush | avança lento e ocupa espaço | tank/construct | não virar parede injusta |
| PhaseShortBlink | blink curto | sombra/caster/elite | precisa tell/recovery |
| Leaper | salto com windup | predador/elite | landing deve abrir janela |
| FloatingSlow | flutua lento | wisp/eco | usar ameaça espacial |
| FloatingOrbit | orbita alvo/arena | olho/wisp/caster | evitar tiro impossível |
| TreasureIdleAmbush | inerte até trigger | mimic/tesouro | trigger legível ou detectável |
| PackFlanker | busca lateral | lobos/goblins | não teletransportar para trás |
| PackLeader | coordena pack | líder/elite | call limitado |
| RetreatAndCall | recua e chama | goblin/cultista | evitar avalanche |
| ProtectAnchor | protege node/ritual | guardião | leash ao anchor |
| CircleStrafe | circula alvo | duelista/ranged móvel | manter legibilidade |
| ChargeLine | investida em linha | brute/boar/elite | windup e linha clara |
| HazardLure | tenta levar para hazard | lurer/trap enemy | hazard deve ser visível |
| BossArenaControl | movimento custom de boss | boss | por fase |
| BossPhaseShift | transição de fase | boss | não causar dano sem tell |

## 12. Moves futuros para eventos/fazenda

Os Moves abaixo são **futuros** e não devem ser implementados em specs atuais.

```text
FarmEdgeApproach
CropLineAdvance
StructureHarass
LivestockHarass
ResourceStealRetreat
FenceBreakAttempt
PortalSpawnAdvance
RitualCircleHold
CivilianAvoidance
```

Regra:

```text
Esses Moves existem apenas para orientar extensibilidade.
Não devem entrar em spec de caverna ou combate atual.
Quando farm invasion entrar no roadmap, revisar esta seção antes de gerar specs.
```

## 13. Reação ao Dash longo do jogador

Dash longo do jogador pode chegar a aproximadamente 8 tiles com upgrades/skills fortes.

Regras para inimigos:

```text
Não aumentar velocidade geral dos inimigos só por causa do Dash longo.
Não reduzir densidade da caverna por causa do Dash longo.
Inimigos podem reagir por line of sight, reposicionamento, cooldowns e retarget.
Inimigos rápidos podem tentar reacquirir alvo após Dash.
Casters/ranged podem manter pressão se ainda tiverem linha de visão.
Tanks/guards podem proteger objetivo sem perseguir até o fim.
Bosses podem ter arena tools contra kiting excessivo, mas sempre com telegraph.
```

Anti-cheese permitido:

```text
reacquire target após Dash
retornar ao anchor se kited para longe demais
cooldown de leash
pack leader chamar aliados próximos dentro do budget
boss reposicionar por fase
hazard punir fuga óbvia em arena específica
```

Anti-cheese proibido:

```text
teleporte invisível sem tell
hitscan sem telegraph
puxar jogador de volta sem counterplay
resetar HP injustamente
atravessar paredes sem regra clara
colar instantaneamente no jogador após Dash
```

---

# PARTE D — ActionSelector e EnemyAction

## 14. Estrutura de EnemyAction

Toda ação relevante deve ter fases explícitas.

| Fase | Significado | Obrigatória? |
|---|---|---|
| Intent | inimigo decidiu ação e orienta corpo/posição | sim |
| Windup | telegraph antes do hit/efeito | sim para ação perigosa |
| Active | hitbox/projétil/zona/efeito acontece | sim |
| Recovery | inimigo fica exposto ou menos eficiente | sim para ação forte |
| Window | MinorOpening/CriticalWindow/CoreExposed se aplicável | conforme ação |
| Cooldown | impede repetição imediata | sim |

Regra:

```text
Ataques fortes precisam de Windup e Recovery.
Ações sem telegraph devem ser fracas, curtas ou apenas movimento.
Ação de controle forte sempre precisa de Windup, Cooldown e counterplay.
```

## 15. ActionType

Tipos de ação atuais:

| ActionType | O que faz | Observação |
|---|---|---|
| MeleeLight | ataque físico rápido | baixo telegraph, baixo dano |
| MeleeHeavy | ataque físico forte | windup/recovery claros |
| Charge | investida | linha/direção clara |
| Leap | salto | landing deve abrir janela |
| DashAttack | avanço curto ofensivo | não confundir com Dash do jogador |
| Projectile | projétil físico/mágico | line of sight recomendado |
| Cone/BreathAttack | sopro/cone de criatura | Breath aqui é nome de ataque |
| AreaCast | zona no chão/área | telegraph de chão |
| Summon | chama adds | budget controlado |
| BuffAlly | fortalece aliado | foco/counterplay |
| DebuffPlayer | aplica debuff | cooldown e resistência |
| Guard | defesa ativa | pode ter guard break |
| Block | bloqueio inimigo | usado por poucos |
| Retreat | recuo tático | não reset injusto |
| CallForHelp | chama aliados | radius e cooldown |
| Burrow | movimento subterrâneo | tell obrigatório |
| Blink | reposicionamento curto | tell/recovery |
| TrapPlace | coloca armadilha | visibilidade mínima |
| TreasureAmbush | ativa emboscada | trigger claro/detectável |
| RitualChannel | canaliza efeito | interrupção possível |
| PhaseTransition | muda fase | boss/elite |

Tipos futuros, fora do escopo atual:

```text
ObjectiveAttack
ResourceSteal
FarmObjectHarass
CropCorrupt
FenceBreak
LivestockHarass
```

## 16. Campos conceituais de EnemyActionSO explicados

| Campo | Significado | Direção |
|---|---|---|
| ActionId | identificador único | estável para save/debug |
| DisplayName | nome legível interno | não precisa aparecer ao jogador |
| ActionType | tipo da ação | usar enum/data contract |
| DamageType | tipo de dano | alinhar com Combat Core |
| RangeMin | distância mínima útil | evita usar colado se não faz sentido |
| RangeMax | distância máxima | evita ataques fora de alcance |
| PreferredRange | distância ideal | usado no scoring |
| StaminaCost | custo físico | ações fortes físicas gastam STA |
| MPCost | custo mágico/técnico | caster/summon/blink etc. |
| Cooldown | intervalo de repetição | evita spam |
| WindupDuration | antecipação | maior para ação perigosa |
| ActiveDuration | tempo ativo | hitbox/projétil/zona |
| RecoveryDuration | vulnerabilidade pós-ação | recompensa leitura |
| MovementLock | quanto prende movimento | forte em heavy/cast |
| TurnRate | velocidade de virar | evita tracking injusto |
| CanHitPlayer | pode acertar jogador | padrão sim em ataques |
| CanHitCompanion | pode acertar companion | conforme ação |
| CanHitPet | pode acertar pet | cuidado para não punir pet demais |
| CanHitObjective | pode acertar objetivo | futuro/anchors |
| CanHitFarmObject | futuro | não usar agora |
| CanHitStructure | futuro | não usar agora |
| AppliesStatus | status aplicado | Burn, Slow etc. |
| StatusChance | chance de status | evitar 100% sem tell |
| PostureDamage | dano de postura | heavy/hammer/brute |
| GuardBreakPower | força anti-block | telegraph obrigatório |
| VulnerabilityWindowType | abertura gerada | Minor/Critical/Core |
| WindowDuration | duração da janela | calibrar por letalidade |
| TelegraphVisual | indicação visual | obrigatório para ações fortes |
| TelegraphAudio | indicação sonora | opcional, complementar |
| Interruptible | pode interromper | comum/caster geralmente sim |
| RequiresLineOfSight | exige visão/linha | recomendado para ranged/caster |
| RequiresObjective | exige anchor/objetivo | rituals/futuro |
| BossPhaseAllowed | fases permitidas | bosses |
| Tags | tags de busca/filtro | Fire, Heavy, AntiBlock etc. |

Regra:

```text
Campos futuros podem existir no contrato, mas specs atuais devem ignorar campos de farm/city objective até roadmap abrir esses temas.
```

## 17. Action scoring

EnemyBrain deve escolher ações por score, não por sequência fixa simples, exceto boss scripts por fase.

Fatores de score:

```text
distância até alvo
linha de visão
ângulo/frente
cooldown disponível
STA disponível
MP disponível
papel do inimigo
HP atual
estado do pack
alvo vulnerável
player bloqueando
player usando ranged/magic
companion tankando
pet interrompendo
objetivo do encontro
hazard próximo
fase do boss
```

Regra:

```text
Ação forte não deve ser escolhida se não houver recurso, telegraph e recovery compatíveis.
Ação de controle não deve ser repetida em loop sem cooldown.
Score deve evitar que o inimigo use sempre a ação matematicamente mais forte.
```

---

# PARTE E — ResourceModule: Stamina e MP dos inimigos

## 18. Regra geral

Todo inimigo tem STA.

```text
STA = recurso para ações físicas, investidas, saltos, defesa ativa, reposicionamento e ataques especiais físicos.
MP = recurso mágico, espiritual, psíquico, corrupto, elemental ou técnico.
```

Fonte de stats:

```text
CAVE_MONSTER_ROSTER_DIRECTION.md para inimigos da caverna.
Docs futuros de farm/eventos para inimigos específicos fora da caverna.
```

## 19. Uso de Stamina inimiga

Ações que devem gastar STA:

```text
ChargeLine
Leaper jump
DashAttack
FastRecovery chain
Block/Guard ativo
GuardBreak físico
Burrow emerge attack
LongChase sprint
Swarm burst
Heavy melee
```

Regras:

```text
Inimigo sem STA suficiente deve escolher ação menor, recuperar, reposicionar ou pausar.
Inimigos físicos não devem spammar ação forte sem custo/cooldown.
STA inimiga não precisa aparecer na HUD comum.
STA pode ser exposta em debug/telemetria.
```

## 20. Uso de MP inimigo

Ações que devem gastar MP:

```text
projectile mágico
curse/debuff
summon
heal/support
shield mágico
blink mágico
fear/confusion/mental
corruption pulse
boss arena control
ritual channel
```

Regras:

```text
Caster sem MP deve alternar para ataque fraco, reposicionar, canalizar, recuar ou chamar ajuda.
Boss pode ter MP especial por fase, mas precisa ser claro na spec.
MP não deve permitir spam infinito de controle.
```

## 21. Recuperação de recurso

Direção:

```text
Inimigos comuns podem não regenerar STA/MP ou regenerar pouco.
Elites podem ter regen moderada conforme papel.
Bosses podem regenerar por fase/mecânica.
Summoners/casters podem recuperar MP ao canalizar, mas ficam vulneráveis.
```

Regra:

```text
Regen de inimigo deve existir para pacing, não para criar luta infinita.
```

---

# PARTE F — ReactionModule

## 22. Reação a Block

| Tipo de inimigo | Reação esperada |
|---|---|
| comum fraco | continua atacando e pode ser punido |
| swarm | tenta cercar, não quebrar block sozinho |
| elite anti-block | usa GuardBreak com telegraph |
| caster | troca para zona/projétil/ângulo |
| tank | pressiona postura e espaço |
| boss | alterna padrões que testam block, dodge e movimento |

Regras:

```text
GuardBreak deve ter telegraph claro.
Anti-block não deve invalidar Block sempre.
Block deve ser bom contra alguns ataques e ruim contra outros.
```

## 23. Reação a Dodge

```text
comuns podem errar e abrir MinorOpening.
fast predators podem recuperar rápido, mas precisam vida menor/janela.
elites podem ter follow-up limitado, não infinito.
bosses podem encadear padrões, mas com leitura clara.
```

Regra:

```text
Não punir Dodge correto com tracking impossível.
Ataques com tracking alto precisam de windup e limite de rotação.
```

## 24. Reação a Dash

```text
melee comum reacquire target após breve delay.
flanker/predator tenta cortar caminho.
ranged/caster mantém pressão se line of sight existir.
guard/tank pode não perseguir, mantendo objetivo.
boss pode reposicionar por padrão/fase.
```

Regra:

```text
Dash deve criar espaço real.
Inimigo não deve colar instantaneamente após Dash sem motivo visual/mecânico.
Dash longo deve ser validado por telemetria, não nerfado preventivamente por IA injusta.
```

## 25. Reação a ranged/magic

```text
melee tenta encurtar distância ou usar cobertura se existir.
ranged troca tiro/reposiciona.
caster usa zona, shield, summon ou debuff.
tank avança lento ou protege aliado.
flanker tenta ângulo lateral.
```

Regra:

```text
Build ranged/magic deve funcionar, mas não ser kite infinito sem risco.
```

## 26. Reação a companions e pets

Companion:

```text
Tank companion aumenta threat.
Ranged companion pode virar alvo de flanker.
Healer/support pode gerar threat em elites/casters.
Controller companion pode ser priorizado por inimigos inteligentes.
Miner/hybrid pode ser ignorado por bestas, mas visado por humanoides táticos.
```

Pet:

```text
Cachorro pode alertar, marcar, interromper ou distrair inimigo pequeno.
Gato pode revelar anomalia/tesouro/ambush em chance ou contexto específico futuro.
Elites podem ignorar pet salvo se pet ativar janela específica.
Boss não deve ser tankado por pet.
```

Regra:

```text
Inimigo não deve sempre ignorar companion/pet.
Inimigo também não deve sempre focar companion/pet para invalidar o sistema.
```

---

# PARTE G — PackModule e LeashModule

## 27. Pack roles

Um pack ideal tem composição lógica.

```text
frontline
flanker
ranged/caster
support/controller
leader
swarm pressure
objective holder
```

Nem todo pack precisa de todos os papéis.

## 28. Pack coordination

Comportamentos coordenados possíveis:

```text
PackLeader marca alvo.
Flanker tenta lateral.
Tank segura passagem.
Caster cria zona.
Ranged pune distância.
Swarm pressiona saída.
RetreatAndCall chama reforço próximo.
Guard protege baú/node/portal/ritual.
```

Regras:

```text
Coordenação deve ser legível.
Coordenação não deve virar avalanche sem limite.
AllyCallRadius precisa respeitar active combat budget do ambiente.
Pack coordination deve ser data-driven por PackCoordinationRules.
```

## 29. Leash e reset

Leash evita cheese e avalanche.

Campos conceituais:

| Campo | Significado |
|---|---|
| SoftLeashRadius | começa a reduzir perseguição |
| HardLeashRadius | força retorno/abandono |
| ObjectiveLeashRadius | raio ao redor de anchor/objetivo |
| ReturnToAnchor | volta ao ponto protegido |
| ReacquireAfterDash | tenta reencontrar alvo após Dash |
| DisengageIfTooFar | abandona perseguição |
| PreserveDamageOnLeash | não reseta HP injustamente |
| CooldownAfterReturn | evita reengage instantâneo |

Regras:

```text
Leash não deve resetar HP injustamente no meio de luta normal.
Guardians podem voltar ao objetivo em vez de perseguir eternamente.
Bosses usam arena bounds, não leash comum.
```

---

# PARTE H — Janelas, vulnerabilidades e telegraph

## 30. Categorias de janela

Fonte canônica:

```text
CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
```

Resumo:

```text
MinorOpening
  abertura comum; não garante crítico automático.

CriticalWindow
  janela clara/arriscada; pode garantir crítico.

CoreExposed / BossMechanicWindow / StaggeredWindow
  janela especial; pode garantir crítico + bônus moderado.
```

## 31. Telegraph obrigatório

Todo ataque relevante precisa de:

```text
windup visual
direção/intenção legível
active frames claros
recovery claro
janela declarada quando aplicável
```

TelegraphAudio é complementar, não obrigatório.

Regra:

```text
Quanto mais letal o ataque, mais claro deve ser o telegraph ou maior deve ser o recovery.
```

## 32. Interrupção

Ações podem ser:

```text
Interruptible
PartiallyInterruptible
Uninterruptible
BossProtected
```

Regras:

```text
Ação de caster comum deve ser interrompível com ferramenta certa.
Ação de elite pode exigir posture damage, stun ou timing.
Boss pode ter ações protegidas, mas com counterplay por fase.
Pet/companion podem abrir janela em ações específicas, não em tudo.
```

---

# PARTE I — Bosses e elites

## 33. Elites

Elites devem ter:

```text
2-3 ações relevantes
1 ação que testa uma defesa específica
1 janela clara
1 reação a player strategy
melhor drop/XP
papel claro no pack ou sala
```

Exemplos:

```text
elite anti-block usa GuardBreak telegráfico.
elite móvel testa Dash/Dodge.
elite caster testa interrupção/line of sight.
elite tank testa posture/charged attack.
```

## 34. Bosses

Bosses devem ter:

```text
2-3 fases
mudança de ataque ou movimento por fase
janelas por fase
telegraph forte
anti-cheese claro
momentos de pressão e recuperação
interação com arena/hazard/adds quando fizer sentido
```

Regra:

```text
Boss não deve ser só stat alto.
Boss não deve virar puzzle único sem combate.
Boss deve respeitar o Combat Core: leitura, recurso, janela e decisão.
```

## 35. Boss AI por fase

```text
Phase 1
  ensina padrão principal.

Phase 2
  altera movimento, adiciona ação, hazard ou adds.

Phase 3
  aumenta pressão, mas mantém counterplay.
```

Cada fase deve declarar:

```text
AllowedActions
ForbiddenActions
MovementMode
VulnerabilityWindow
RecoveryRules
AddRules
HazardRules
AntiCheeseRules
PhaseExitCondition
```

---

# PARTE J — Status aplicados por inimigos

## 36. Status permitidos

Fonte de lista geral:

```text
COMBAT_CORE_DIRECTION.md
```

Status comuns para inimigos:

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
HeatStress
ColdStress
Corruption
```

Regras:

```text
Controle forte precisa de telegraph e cooldown.
ConfusionLite não remove controle total.
Root deve ser raro, curto ou quebrável.
Fear deve deslocar/pressionar sem tirar agência total.
DurabilityStress não destrói permanentemente sem spec própria.
Corruption deve ser relevante, mas com cura/prevenção/purificação.
```

---

# PARTE K — Farm invasion e eventos hostis futuros

## 37. Status desta seção

Esta seção é **futura**.

Regra:

```text
Não gerar specs atuais de farm invasion com base nesta seção.
Não alterar sistemas atuais de fazenda por causa desta seção.
Não implementar dano a crops/animais/estruturas agora.
Esta seção existe para que a arquitetura de EnemyBrain não nasça presa à caverna.
```

## 38. Premissa futura

Invasões da fazenda, se entrarem no roadmap, devem expandir o jogo sem transformar a fazenda em punição constante.

Direção futura:

```text
Invasão deve ser evento claro, legível e com aviso.
Não deve destruir progresso permanentemente sem chance real de defesa.
Pode ameaçar crops, animais, estruturas, baús externos, máquinas, cercas, fontes de recurso ou visitantes.
Deve ter reparo, mitigação, prevenção ou recuperação.
```

## 39. Tipos futuros de invasão

```text
beasts atacando animais/crops
fungal corruption tentando contaminar solo/crops
goblins/kobolds tentando roubar recursos
cultistas tentando ritual perto da Fonte/limites da fazenda
constructs bromecianos antigos ativando próximo a máquinas
sombras/Nyx gerando evento noturno raro
Pedra Negra corrompendo área temporária
```

## 40. Objetivos futuros de invasores

```text
roubar item/recurso
quebrar cerca/estrutura leve
contaminar crop/solo
assustar animais
atacar pet/companion apenas como ameaça tática, sem morte permanente sem sistema próprio
canalizar ritual
proteger portal temporário
fugir com loot
chamar reforço
```

Regra futura:

```text
Dano permanente à fazenda deve ser limitado, reparável e sinalizado.
Não criar perda irreversível sem decisão clara do jogador.
```

## 41. Defesas futuras da fazenda

```text
cercas
iluminação
cachorro/pet alertando
companion guard duty
espantalho/wards mágicos
altares divinos com bônus de proteção
armadilhas leves
sino de alerta
reputação com cidade atraindo ajuda
```

Regra futura:

```text
Defesas devem reduzir risco, atrasar invasores ou alterar comportamento.
Defesas não devem transformar tudo em tower defense obrigatório.
```

---

# PARTE L — Data assets e contratos futuros

## 42. Data assets esperados

```text
EnemyDataSO
EnemyActionSO
EnemyActionSetSO
EnemyBrainProfileSO
EnemyMovementProfileSO
EnemyBehaviorProfileSO
EnemySpawnProfileSO
EnemySpawnPackSO
EnemyFactionLockSO
EnemyBestiaryEntrySO
EnemyObjectiveProfileSO futuro
EnemyInvasionProfileSO futuro
LootTableSO
```

## 43. EnemyBrainProfileSO explicado

| Campo | Significado |
|---|---|
| BrainId | identificador do perfil de cérebro |
| PrimaryRole | papel principal |
| SecondaryRoles | variações secundárias |
| BehaviorProfile | módulos injetados de comportamento |
| MovementProfile | Move oficial + parâmetros |
| AggroProfile | como calcula ameaça |
| TargetPriorityProfile | como escolhe alvo |
| ActionSet | ações disponíveis |
| RetreatRules | quando recua |
| LeashRules | até onde persegue |
| PackCoordinationRules | como conversa com pack |
| ObjectiveRules | futuro/anchors/eventos |
| ReactionRules | respostas a eventos |
| AllowedBiomes | biomas/contextos válidos |
| AllowedContexts | Cave, BossGate, FutureFarm etc. |
| DebugTags | filtros de debug |

## 44. EnemyBehaviorProfileSO explicado

| Campo | Significado |
|---|---|
| PerceptionModuleId | módulo de percepção |
| IntentModuleId | módulo de intenção |
| TargetingModuleId | módulo de alvo |
| MovementModuleId | módulo de movimento |
| ActionSelectorModuleId | módulo de decisão de ação |
| ResourceModuleId | módulo de STA/MP |
| CooldownModuleId | módulo de cooldown |
| ReactionModuleId | módulo de reação |
| PackModuleId | módulo de coordenação |
| LeashModuleId | módulo de leash |
| PhaseModuleId | módulo de fase, se boss/elite |
| ObjectiveModuleId | futuro/evento |

Regra:

```text
Perfis devem ser combináveis.
Não criar uma classe nova para cada monstro se a variação puder ser feita por módulos e dados.
```

## 45. EnemyObjectiveProfileSO futuro

Apenas para eventos/fazenda/cidade futuros.

```text
ObjectiveId
ObjectiveType
PreferredTargets
ForbiddenTargets
DamageRules
StealRules
CorruptionRules
RetreatCondition
SuccessCondition
FailureCondition
PlayerWarningLevel
RecoveryRules
```

Regra:

```text
Não implementar este asset agora sem roadmap específico de eventos objetivos.
```

---

# PARTE M — Telemetria e validação

## 46. Telemetria de IA

Registrar em Play Mode:

```text
actions escolhidas por inimigo
ações canceladas por falta de STA/MP
tempo em windup/active/recovery
quantidade de hits sem telegraph percebido
tempo perseguindo jogador
tempo preso em pathfinding
leash triggers
ally calls
pack activation count
retreats
boss phase transitions
dashes do jogador que quebraram sala/encontro
body block incidents
```

Telemetria futura, não atual:

```text
objetivos de fazenda atacados
crops afetados
estruturas danificadas
recursos roubados
recovery/reparo pós-invasão
```

## 47. Validação humana

Perguntas de validação:

```text
O inimigo parecia ter intenção clara?
O ataque perigoso teve telegraph?
A janela de punição foi legível?
O inimigo ficou preso ou atravessou algo injustamente?
O pack ficou desafiador sem avalanche injusta?
O Dash longo quebrou o encontro?
Block/Dodge/Dash tiveram respostas justas?
Companion/pet ajudaram sem trivializar?
A IA parecia variar sem parecer aleatória?
O comportamento veio de módulos reutilizáveis ou de exceção hardcoded?
```

Perguntas futuras para farm invasion:

```text
Invasão pareceu evento interessante, não punição arbitrária?
Dano a crops/estruturas foi sinalizado e recuperável?
O jogador teve aviso e resposta possível?
```

---

# PARTE N — Decisões fechadas

```text
Enemy Behaviors é documento transversal; não fica preso à caverna.
Farm invasion é futuro explícito e não deve gerar specs atuais.
Cidade/eventos hostis também são futuro explícito.
Caverna continua com roster canônico próprio.
Moves oficiais da caverna vêm do CAVE_MONSTER_ROSTER_DIRECTION.md.
Specs devem reutilizar Moves oficiais antes de criar novos.
Hearing/audição não será elemento de IA por enquanto.
Comportamentos são injetáveis por profiles/modules, não classes totalmente fixas.
EnemyBrain deve ser orientado por percepção, intenção, alvo, movimento, ação, recurso, reação, pack, leash e estado.
Ações relevantes devem ter Intent, Windup, Active, Recovery, Window e Cooldown.
Todo inimigo tem STA; inimigos físicos podem ter MP 0.
Inimigos devem gastar STA/MP em ações fortes conforme dados da ação.
Dash longo do jogador não obriga reduzir dificuldade, velocidade ou densidade de inimigos.
Inimigos podem reagir ao Dash longo por reacquire, leash, line of sight, reposicionamento e objetivo.
GuardBreak precisa de telegraph.
Controle forte precisa de counterplay.
Packs devem ter coordenação legível.
Pet e companion são considerados pelo EnemyBrain, mas não devem ser sempre ignorados nem sempre focados.
```

---

# PARTE O — Pendências para specs futuras

## 48. Specs atuais úteis

```text
Definir EnemyBrain runtime architecture.
Definir EnemyActionSO data contract.
Definir EnemyActionSetSO data contract.
Definir EnemyMovementProfileSO usando Moves oficiais.
Definir EnemyBehaviorProfileSO com módulos injetáveis.
Definir EnemyThreat/Aggro model.
Definir EnemyTargetPriorityProfile.
Definir EnemyLeashRules.
Definir PackCoordinationRules.
Definir ReactionRules para Block/Dodge/Dash/ranged/magic/companion/pet.
Definir telemetria de IA em Play Mode.
Validar em Unity pathfinding, body blocking, telegraph, windows e Dash longo.
```

## 49. Specs futuras, não atuais

```text
Definir FarmInvasionProfileSO.
Definir EnemyObjectiveProfileSO para invasões/eventos.
Definir regras de dano reparável em crops/estruturas.
Definir town hostile event behavior.
Definir mundo externo com inimigos fora da caverna.
```
