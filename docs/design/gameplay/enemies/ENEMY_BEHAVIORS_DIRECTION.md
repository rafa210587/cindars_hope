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
> - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`  
> - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`  
> **Função:** definir como inimigos pensam, escolhem alvos, se movem, atacam, gastam Stamina/MP, reagem ao jogador, coordenam packs e participam de caverna, eventos e possíveis invasões da fazenda.  
> **Não é spec implementável.** Este documento define direção de design. Specs futuras devem converter isto em dados e sistemas.

---

## 0. Escopo

Este documento cobre comportamento de inimigos em qualquer ambiente do jogo.

Ambientes previstos:

```text
caverna
boss gates
nível 101
fazenda durante eventos/invasões
cidade durante eventos raros
áreas futuras de mundo externo
eventos narrativos
```

Regra:

```text
A caverna continua sendo a fonte principal de roster e monstros já definidos.
Este documento não substitui o roster da caverna.
Este documento define comportamento transversal para EnemyBrain, EnemyAction, movimento, decisão e eventos.
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
```

---

# PARTE A — Visão geral da IA inimiga

## 2. Filosofia

Inimigos devem parecer perigosos por comportamento, não apenas por números altos.

Inimigo bom deve:

```text
ter intenção legível
ter função clara no encontro
ter pelo menos uma forma de counterplay
ter telegraph para ataques relevantes
ter recovery/janela quando usa ação forte
usar Stamina/MP/cooldown de forma previsível
interagir com packs, ambiente e objetivo do evento
```

Inimigo ruim seria:

```text
perseguir sem parar sem custo
atacar instantaneamente sem telegraph
controlar o jogador sem counterplay
ter HP alto sem janela
ignorar colisão/pathing de forma injusta
spammar dash/blink/leap sem recovery
trivializar fazenda/crops/pets/companions sem chance de resposta
```

## 3. Papéis de inimigo

Papéis globais:

```text
Chaser
Guard
Ranged
Caster
Burrower
Swarm
Tank
Controller
TreasureTrap
Elite
Boss
Invader
Raider
CropDestroyer
LivestockPredator
ResourceThief
Ritualist
Summoner
HazardLurer
LoreGuardian
```

Regra:

```text
Um inimigo pode ter mais de um papel, mas deve ter um papel primário.
O papel primário define como ele decide alvo, se move, escolhe ações e recua.
```

---

# PARTE B — EnemyBrain

## 4. Camadas do EnemyBrain

EnemyBrain deve ser pensado em camadas.

```text
PerceptionLayer
  detecta jogador, companion, pet, aliados, objetivos, hazards, crops, estruturas e line of sight.

IntentLayer
  decide objetivo atual: atacar, guardar, flanquear, fugir, chamar ajuda, destruir recurso, roubar, castar, proteger ritual.

MovementLayer
  executa Move oficial ou movimento específico de evento.

ActionLayer
  escolhe ação usando range, cooldown, Stamina, MP, risco, telegraph e papel.

ReactionLayer
  reage a dano, block, dodge, dash, magia, pet, companion, stagger, status e morte de aliados.

StateLayer
  controla estados: idle, patrol, alert, engage, recover, retreat, enrage, phase shift, dead.
```

## 5. Estados globais

```text
Idle
Patrol
Guard
Suspicious
Alert
Engage
Reposition
Recover
Retreat
CallForHelp
ProtectObjective
AttackObjective
Cast
Staggered
Stunned
Fleeing
Enraged
PhaseTransition
Dead
```

Regras:

```text
Todo inimigo não-boss deve ter pelo menos Idle/Patrol ou Guard, Alert, Engage, Recover e Dead.
Elites devem ter Reposition ou outra camada tática.
Bosses devem ter PhaseTransition.
Invasores de fazenda devem ter ProtectObjective/AttackObjective ou Retreat conforme objetivo.
```

## 6. Percepção

Tipos de percepção:

```text
SightRadius
HearingRadius
DamageAggro
AllyCallRadius
ObjectiveAwareness
PetAwareness
CompanionAwareness
Light/Darkness modifiers futuro
```

Regras:

```text
Inimigo não deve ativar através de paredes sem regra específica.
AllyCallRadius deve ser controlado para evitar avalanche injusta.
TreasureTrap pode ignorar percepção até trigger específico.
Boss pode ter percepção de arena inteira.
```

## 7. Threat / aggro

Ameaça deve ser calculada por intenção de combate e papel.

Fatores possíveis:

```text
dano recebido
proximidade
cura/suporte do jogador
companion tankando
pet interrompendo
jogador usando magia de área
jogador com HP baixo
jogador carregando recurso/objetivo
crops/animais/estruturas em invasão de fazenda
ritual/anchor protegido
```

Regras:

```text
Nem todo inimigo deve focar sempre o jogador.
Tank/Guard tende a manter posição.
Predator tende a caçar alvo vulnerável.
Ranged/Caster tende a evitar melee direto.
Invader pode priorizar objetivo da invasão antes do jogador.
Boss pode trocar alvo por fase ou mecânica.
```

---

# PARTE C — Movimento

## 8. Fonte canônica de Moves

Para inimigos da caverna, os nomes oficiais de `Move` vêm do roster:

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

## 9. Moves adicionais para eventos/fazenda

Moves possíveis para invasões/eventos, se necessário:

```text
FarmEdgeApproach
CropLineAdvance
StructureHarass
LivestockHarass
ResourceStealRetreat
FenceBreakAttempt
TorchLure
PortalSpawnAdvance
RitualCircleHold
CivilianAvoidance
```

Regras:

```text
Esses Moves são candidatos, não implementação obrigatória.
Se um Move oficial do roster resolver o caso, preferir o Move oficial.
Invasor de fazenda deve ter objetivo claro e rota de entrada/saída clara.
```

## 10. Reação ao Dash longo do jogador

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
pack leader chamar aliados próximos
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
```

---

# PARTE D — Ações inimigas

## 11. Estrutura de EnemyAction

Toda ação relevante deve ter fases.

```text
Intent
  inimigo escolhe ação e começa orientação/posição.

Windup
  telegraph visual/sonoro antes do hit/efeito.

Active
  hitbox/projétil/zona/efeito acontece.

Recovery
  inimigo fica vulnerável ou menos eficiente.

Window
  MinorOpening, CriticalWindow ou CoreExposed, se aplicável.

Cooldown
  ação não pode ser repetida até cooldown terminar.
```

Regra:

```text
Ataques fortes precisam de Windup e Recovery.
Ações sem telegraph devem ser fracas, curtas ou apenas movimento.
```

## 12. Campos conceituais de EnemyActionSO

```text
ActionId
DisplayName
ActionType
DamageType
RangeMin
RangeMax
PreferredRange
StaminaCost
MPCost
Cooldown
WindupDuration
ActiveDuration
RecoveryDuration
MovementLock
TurnRate
CanHitPlayer
CanHitCompanion
CanHitPet
CanHitFarmObject
CanHitStructure
AppliesStatus
StatusChance
PostureDamage
GuardBreakPower
VulnerabilityWindowType
WindowDuration
TelegraphVisual
TelegraphAudio
Interruptible
RequiresLineOfSight
RequiresObjective
BossPhaseAllowed
Tags
```

Regra:

```text
Não hardcodar comportamento em scripts quando ele puder ser data-driven por EnemyActionSO.
```

## 13. Tipos de ação

```text
MeleeLight
MeleeHeavy
Charge
Leap
DashAttack
Projectile
Cone/BreathAttack
AreaCast
Summon
BuffAlly
DebuffPlayer
Guard
Block
Retreat
CallForHelp
Burrow
Blink
TrapPlace
TreasureAmbush
ObjectiveAttack
ResourceSteal
RitualChannel
PhaseTransition
```

Observação:

```text
BreathAttack é nome válido para ataque de sopro de criatura.
Não tem relação com atributo Breath/Fôlego removido.
```

---

# PARTE E — Stamina e MP dos inimigos

## 14. Regra geral

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

## 15. Uso de Stamina inimiga

Ações que devem gastar STA:

```text
ChargeLine
Leaper jump
DashBite / DashAttack
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

## 16. Uso de MP inimigo

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

## 17. Regeneração de inimigos

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

# PARTE F — Seleção de ação

## 18. Action scoring

EnemyBrain deve escolher ações por score, não por sequência fixa simples, exceto bosses roteirizados por fase.

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
Ação forte não deve ser escolhida se não houver telegraph/recovery compatível.
Ação de controle não deve ser repetida em loop sem cooldown.
```

## 19. Reação a Block

Inimigos devem reagir ao Block conforme papel.

```text
comum fraco: continua atacando e pode ser punido.
elite anti-block: usa GuardBreak com telegraph.
caster: pode trocar para zona/projétil/ângulo.
tank: pressiona postura.
swarm: tenta cercar, não quebrar block sozinho.
boss: tem padrões que testam block, dodge e movimento.
```

Regras:

```text
GuardBreak deve ter telegraph claro.
Anti-block não deve invalidar Block sempre.
Block deve ser bom contra alguns ataques e ruim contra outros.
```

## 20. Reação a Dodge

```text
comuns: podem whiffar e abrir MinorOpening.
fast predators: podem recuperar rápido, mas com baixa vida/janela.
elites: podem ter follow-up limitado, não infinito.
bosses: podem encadear padrões, mas com leitura clara.
```

Regra:

```text
Não punir Dodge correto com tracking impossível.
Ataques com tracking alto precisam de windup e limite.
```

## 21. Reação a Dash

```text
melee comum: reacquire target após breve delay.
flanker/predator: tenta cortar caminho.
ranged/caster: mantém pressão se line of sight existir.
guard/tank: pode não perseguir, mantendo objetivo.
boss: pode reposicionar por padrão/fase.
```

Regra:

```text
Dash deve criar espaço real.
Inimigo não deve colar instantaneamente após Dash sem motivo visual/mecânico.
```

## 22. Reação a ranged/magic

```text
melee: tenta encurtar distância ou usar cobertura se existir.
ranged: troca tiro/reposiciona.
caster: usa zona, shield, summon ou debuff.
tank: avança lento ou protege aliado.
flanker: tenta ângulo lateral.
```

Regra:

```text
Build ranged/magic deve funcionar, mas não ser kite infinito sem risco.
```

---

# PARTE G — Packs e coordenação

## 23. Pack roles

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

## 24. Pack coordination

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
```

## 25. Leash e reset

Leash evita cheese e avalanche.

```text
SoftLeashRadius
HardLeashRadius
ObjectiveLeashRadius
ReturnToAnchor
ReacquireAfterDash
DisengageIfTooFar
```

Regras:

```text
Leash não deve resetar HP injustamente no meio de luta normal.
Guardians podem voltar ao objetivo em vez de perseguir eternamente.
Bosses usam arena bounds, não leash comum.
Invasores de fazenda podem fugir quando objetivo falha ou tempo acaba.
```

---

# PARTE H — Janelas, vulnerabilidades e telegraph

## 26. Categorias de janela

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

## 27. Telegraph obrigatório

Todo ataque relevante precisa de:

```text
windup visual
windup sonoro quando possível
direção/intenção legível
active frames claros
recovery claro
janela declarada quando aplicável
```

Regra:

```text
Quanto mais letal o ataque, mais claro deve ser o telegraph ou maior deve ser o recovery.
```

## 28. Interrupção

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

# PARTE I — Farm invasions / invasões da fazenda

## 29. Premissa

Invasões da fazenda são conteúdo futuro possível.

Elas devem expandir o jogo sem transformar a fazenda em punição constante.

Direção:

```text
Invasão deve ser evento claro, legível e com aviso.
Não deve destruir progresso permanentemente sem chance real de defesa.
Pode ameaçar crops, animais, estruturas, baús externos, máquinas, cercas, fontes de recurso ou visitantes.
Deve ter reparo, mitigação, prevenção ou recuperação.
```

## 30. Tipos de invasão

Possibilidades:

```text
beasts atacando animais/crops
fungal corruption tentando contaminar solo/crops
goblins/kobolds tentando roubar recursos
cultistas tentando ritual perto da Fonte/limites da fazenda
constructs bromecianos antigos ativando próximo a máquinas
sombras/Nyx gerando evento noturno raro
Pedra Negra corrompendo área temporária
```

Regra:

```text
Invasão precisa fazer sentido com lore, estação, reputação, progresso da caverna ou evento.
Não usar invasão aleatória punitiva sem telegraph.
```

## 31. Objetivos de invasores

Invasores podem ter objetivo diferente de matar o jogador.

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

Regra:

```text
Dano permanente à fazenda deve ser limitado, reparável e sinalizado.
Não criar perda irreversível sem decisão clara do jogador.
```

## 32. Estados específicos de invasão

```text
ApproachFarmEdge
ScoutFarm
TargetObjective
AttackObjective
StealResource
RetreatWithLoot
FleeIfOutmatched
CallReinforcement
CorruptTile
HarassLivestock
BreakFence
ChannelRitual
EscapeMap
```

## 33. Defesas da fazenda futuras

Sistemas possíveis:

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

Regra:

```text
Defesas devem reduzir risco, atrasar invasores ou alterar comportamento.
Defesas não devem transformar tudo em tower defense obrigatório.
```

---

# PARTE J — Bosses e elites

## 34. Elites

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

## 35. Bosses

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

## 36. Boss AI por fase

```text
Phase 1
  ensina padrão principal.

Phase 2
  altera movimento, adiciona ação, hazard ou adds.

Phase 3
  aumenta pressão, mas mantém counterplay.
```

Regra:

```text
Cada fase deve declarar allowed actions, forbidden actions, movement mode, vulnerability window e recovery rules.
```

---

# PARTE K — Companions e pets

## 37. Reação a companions

```text
Tank companion aumenta threat.
Ranged companion pode virar alvo de flanker.
Healer/support pode gerar threat em elites/casters.
Controller companion pode ser priorizado por inimigos inteligentes.
Miner/hybrid pode ser ignorado por bestas, mas visado por humanoides táticos.
```

Regra:

```text
Inimigo não deve sempre ignorar companion.
Inimigo também não deve sempre focar companion para invalidar o sistema.
```

## 38. Reação a pets

```text
Cachorro pode alertar, marcar, interromper ou distrair inimigo pequeno.
Gato pode revelar anomalia/tesouro/ambush em chance ou contexto específico.
Inimigos pequenos podem reagir ao cachorro.
Elites podem ignorar pet salvo se pet ativar janela específica.
Boss não deve ser tankado por pet.
```

Regra:

```text
Pet é apoio tático e de exploração, não substituto de companion.
```

---

# PARTE L — Status aplicados por inimigos

## 39. Status permitidos

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

## 40. Status em invasões de fazenda

Possíveis efeitos:

```text
CropWitherTemporary
SoilCorruptionTemporary
AnimalFear
MachineJam
FenceDamage
StorageThreat
VisitorPanic
```

Regras:

```text
Status de fazenda deve ser reparável/curável.
Evitar perda permanente não anunciada.
Usar eventos para criar urgência, não punição arbitrária.
```

---

# PARTE M — Dados e runtime futuros

## 41. Data assets esperados

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
EnemyObjectiveProfileSO
EnemyInvasionProfileSO
LootTableSO
```

## 42. EnemyBrainProfileSO conceitual

```text
BrainId
PrimaryRole
SecondaryRoles
BehaviorProfile
MovementProfile
AggroProfile
TargetPriorityProfile
ActionSet
RetreatRules
LeashRules
PackCoordinationRules
ObjectiveRules
ReactionRules
AllowedBiomes
AllowedContexts
DebugTags
```

## 43. EnemyObjectiveProfileSO conceitual

Para eventos/fazenda:

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

---

# PARTE N — Telemetria e validação

## 44. Telemetria de IA

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
objetivos atacados
objetivos destruídos/danificados
recursos roubados
retreats
boss phase transitions
dashes do jogador que quebraram sala/encontro
body block incidents
```

## 45. Validação humana

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
Invasão da fazenda pareceu evento interessante, não punição arbitrária?
Dano a crops/estruturas foi sinalizado e recuperável?
```

---

# PARTE O — Decisões fechadas

```text
Enemy Behaviors é documento transversal; não fica preso à caverna.
Caverna continua com roster canônico próprio.
Moves oficiais da caverna vêm do CAVE_MONSTER_ROSTER_DIRECTION.md.
Specs devem reutilizar Moves oficiais antes de criar novos.
EnemyBrain deve ser orientado por percepção, intenção, movimento, ação, reação e estado.
Ações relevantes devem ter Intent, Windup, Active, Recovery, Window e Cooldown.
Todo inimigo tem STA; inimigos físicos podem ter MP 0.
Inimigos devem gastar STA/MP em ações fortes conforme dados da ação.
Dash longo do jogador não obriga reduzir dificuldade, velocidade ou densidade de inimigos.
Inimigos podem reagir ao Dash longo por reacquire, leash, line of sight, reposicionamento e objetivo.
GuardBreak precisa de telegraph.
Controle forte precisa de counterplay.
Packs devem ter coordenação legível.
Invasões da fazenda são conteúdo futuro possível e devem ser eventos sinalizados, reparáveis e não punitivos de forma arbitrária.
Pet e companion são considerados pelo EnemyBrain, mas não devem ser sempre ignorados nem sempre focados.
```

---

# PARTE P — Pendências para specs futuras

```text
Definir EnemyBrain runtime architecture.
Definir EnemyActionSO data contract.
Definir EnemyActionSetSO data contract.
Definir EnemyMovementProfileSO usando Moves oficiais.
Definir EnemyBehaviorProfileSO.
Definir EnemyThreat/Aggro model.
Definir EnemyTargetPriorityProfile.
Definir EnemyLeashRules.
Definir PackCoordinationRules.
Definir ReactionRules para Block/Dodge/Dash/ranged/magic/companion/pet.
Definir FarmInvasionProfileSO.
Definir EnemyObjectiveProfileSO para invasões/eventos.
Definir regras de dano reparável em crops/estruturas.
Definir telemetria de IA em Play Mode.
Validar em Unity pathfinding, body blocking, telegraph, windows e Dash longo.
```
