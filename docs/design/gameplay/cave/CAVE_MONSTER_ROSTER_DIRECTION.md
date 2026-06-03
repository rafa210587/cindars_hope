# Cindar's Hope — Cave Monster Roster Direction

> **Status:** documento canônico de roster, atributos, scaling, comportamento, movimento, packs e bosses da caverna  
> **Local:** `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`  
> - `docs/specs/a_implementar/spec_enemy_ai_roster_bestiary_faction_locks_runtime.md`  
> - `docs/specs/a_implementar/spec_cave_runtime_generation_checkpoints_boss_gates.md`  
> - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`  
> **Função:** detalhar criaturas, bosses, packs, comportamento, ataques, atributos reais, scaling, XP, recursos, tesouros e função de gameplay.  
> **Não é spec implementável.** Specs futuras devem converter estes dados em `EnemyDataSO`, `EnemyActionSO`, `EnemyActionSetSO`, `EnemySpawnProfileSO`, `EnemySpawnPackSO`, `LootTableSO` e bestiary entries.

---

## 0. Decisão canônica: sem atributo de fôlego

O antigo campo `BR` foi removido do roster.

Não existe mais como:

```text
atributo de monstro
recurso de monstro
coluna de tabela
barra de HUD
custo de ação
campo de save/load
```

Todo monstro deve continuar tendo:

```text
HP
MP
STA
FOR
CON
DES
INT
VON
CAR
XP
```

Leitura dos recursos:

```text
HP = vida.
MP = recurso mágico/espiritual/psíquico; criaturas físicas podem ter MP 0.
STA = recurso físico de ações, investidas, ataques especiais, defesa ativa e movimentação intensa.
FOR = dano físico, impacto, stagger, força de carga.
CON = vida, estabilidade, resistência física e tolerância a status físicos.
DES = mobilidade, reação, velocidade, esquiva, perseguição e ataques rápidos.
INT = magia técnica, IA tática, armadilhas, ruínas, comportamento caster.
VON = resistência mental/espiritual, magia espiritual, corrupção, medo, Nyx/Void.
CAR = liderança, comando de pack, presença ritual/social.
```

O que antes era representado por `BR` agora deve ser descrito por:

```text
Move
Behavior
Traits
```

Traits recomendadas:

```text
HighMobility
FastRecovery
SlowRecovery
LeapPressure
ChargePressure
LongChase
GuardBreak
Shielded
PackCoordination
CasterDiscipline
RitualAnchor
HazardAdapted
HeatAdapted
ColdAdapted
PoisonAdapted
CorruptionAdapted
MentalResistant
PhysicalStable
PostureHeavy
PostureFragile
TankBody
SwarmBody
FloatingBody
Burrower
Blinker
TreasureAmbush
ConstructBody
UndeadBody
AberrantMind
BossBody
NoNormalLoot
```

---

## 1. Regra de uso

Este documento é a fonte canônica de design para monstros da caverna.

Specs que criarem/alterarem inimigos devem ler este documento antes de mexer em:

```text
EnemyDataSO
EnemyActionSO
EnemyActionSetSO
EnemyBrain
EnemySpawnProfileSO
EnemySpawnPackSO
EnemyFactionLockSO
EnemyBestiaryEntrySO
LootTableSO
XP rewards
boss gate data
```

Regra de referência externa:

```text
Usar D&D apenas como referência de arquétipos de dungeon fantasy.
Não copiar statblocks, textos, habilidades proprietárias, lore proprietária ou progressão oficial.
Criaturas icônicas devem ser adaptadas para Vaalara com nomes, função, comportamento e dados próprios.
```

Exemplos de adaptação permitida:

```text
Beholder-like -> Observadores / Tiranos Oculares de Pedra Negra
Mimic-like -> Baú-Mordente / Arsenal-Mordente
Gelatinous cube-like -> Cubo de Lodo Translúcido / Lodo Devorador de Núcleo
Mind flayer-like -> Larva Devora-Mentes / Devora-Mentes Abissal
Rust monster-like -> Besouro Ferrugem
Displacer beast-like -> Pantera Distorcida
Owlbear-like -> Urso-Coruja Raiz-Oca
Bulette-like -> Tubarão de Pedra
Umber hulk-like -> Titã Escavador Quebra-Núcleo
```

---

# PARTE A — Progressão, atributos e desafio

## 2. Progressão do jogador usada como régua

```text
No level 1, o jogador começa com 1 ponto em cada atributo principal.
A cada level up, o jogador ganha +1 ponto para distribuir.
Atributos principais considerados:
- Força
- Constituição
- Destreza
- Inteligência
- Vontade
- Carisma
```

Inferência de design:

```text
Player level 1: soma aproximada de atributos principais = 6
Player level 10: soma aproximada = 15
Player level 25: soma aproximada = 30
Player level 40: soma aproximada = 45
Player level 55: soma aproximada = 60
Player level 70: soma aproximada = 75
Player level 85: soma aproximada = 90
Player level 100: soma aproximada = 105
```

Direção:

```text
Monstro comum do andar deve ter pelo menos 1-2 atributos dominantes acima do player médio esperado.
Elite deve ter 2-3 atributos dominantes bem acima do player médio esperado.
Boss deve funcionar como múltiplos inimigos em um corpo, com fases, comportamento alternado e janelas de vulnerabilidade.
```

## 3. Atributos usados por inimigos

Stats e atributos:

```text
HP
MP
STA
FOR
CON
DES
INT
VON
CAR
```

Abreviações canônicas:

```text
HP / MP / STA
FOR / CON / DES / INT / VON / CAR
```

Regra obrigatória:

```text
Todo monstro tem HP, MP e STA.
Criaturas físicas podem ter MP 0.
Criaturas mágicas, espirituais, aberrantes, corruptas, elementais, constructs avançados e bosses geralmente têm MP > 0.
Todo monstro tem STA > 0, inclusive casters, porque reposicionamento, esquiva, defesa, investida e ações físicas consomem stamina.
```

## 4. Escala real de atributos por faixa

| Faixa | Player médio esperado | Comum: soma atributos | Elite: soma atributos | Boss: soma atributos |
|---:|---:|---:|---:|---:|
| 1-10 | 6-15 | 14-24 | 26-38 | — |
| 11-25 | 16-30 | 28-46 | 48-68 | 95-125 |
| 26-40 | 31-45 | 44-68 | 70-96 | 130-170 |
| 41-55 | 46-60 | 62-88 | 90-125 | 180-230 |
| 56-70 | 61-75 | 82-112 | 120-160 | 240-310 |
| 71-85 | 76-90 | 105-140 | 150-200 | 330-430 |
| 86-99 | 91-104 | 130-175 | 190-250 | 460-620 |
| 100 | 105+ | elite only | 230-300 | 650-800 |
| 101 | endgame | bosses only | — | 800+ |

## 5. HP/MP/Stamina por faixa

| Faixa | Comum HP | Elite HP | Boss HP | Comum STA | Elite STA | Boss STA | Observação |
|---:|---:|---:|---:|---:|---:|---:|---|
| 1-10 | 25-80 | 90-160 | — | 18-45 | 30-70 | — | tutorial com risco real |
| 11-25 | 70-160 | 180-320 | 900-1300 | 28-75 | 70-100 | 90-140 | primeiro boss gate |
| 26-40 | 130-280 | 320-560 | 1500-2200 | 40-85 | 80-115 | 120-150 | frio, controle e elite anti-block |
| 41-55 | 220-420 | 500-850 | 2500-3600 | 55-100 | 100-135 | 140-180 | fogo e pressão agressiva |
| 56-70 | 360-650 | 750-1200 | 4000-5600 | 55-110 | 85-135 | 100-180 | ruínas/constructos resistentes |
| 71-85 | 560-900 | 1100-1800 | 6500-8500 | 75-140 | 95-160 | 130-220 | abismo e controllers |
| 86-99 | 780-1300 | 1600-2600 | 9000-13000 | 90-170 | 120-230 | 220-320 | núcleo corrompido |
| 100 | — | 2200-3600 | 15000-19000 | — | 180-260 | 220-320 | gate final |
| 101 | — | — | 18000+ | — | — | 160-340 | bosses em sequência |

MP por faixa:

```text
Físicos puros: MP 0.
Bestas contaminadas: MP baixo/médio.
Casters/cultistas/aberrantes: MP alto.
Bosses: MP alto mesmo quando físicos, se houver fase mágica, corrupção, arena ou poderes especiais.
Eco Silencioso de Anya: MP especial porque é evento/lore, não combate comum.
```

## 6. Scaling por nível da caverna

```text
NativeMinLevel
NativeMaxLevel
NativeMidLevel
SpawnLevel
Delta = SpawnLevel - NativeMidLevel
```

Scaling conceitual:

```text
HPScale = 1.0 + max(0, Delta) * 0.055
DamageScale = 1.0 + max(0, Delta) * 0.045
XPScale = 1.0 + max(0, Delta) * 0.050
DominantAttributeScale = +1 a cada 4 níveis acima
SecondaryAttributeScale = +1 a cada 6 níveis acima
StaminaScale = +1 a cada 5 níveis acima para físicos/móveis; +1 a cada 8 níveis para casters lentos
DropRareScale = +0.5% por nível acima, com cap por spec
```

Se aparecer abaixo:

```text
HPScale mínimo 0.65
DamageScale mínimo 0.70
XPScale mínimo 0.60
Reduzir chance de drop raro.
Evitar colocar monstro muito acima como trivial em andares baixos.
```

## 7. Variantes por profundidade

| Variante | Condição | Efeito |
|---|---|---|
| Veterano | +6 níveis acima | +HP, +dano, +XP, melhor IA |
| Elite | +10 níveis acima ou sala especial | nova ação, aura, drop raro |
| Corrompido | nível 86+ ou Pedra Negra | dano escuro, resistência maior, drop corrompido |
| Lunar | Nyx/Alihana/evento | sombra, deslocamento, confusão leve |
| Bromeciano | ruínas 56+ | partes mecânicas, defesa alta, drop técnico |
| Ígneo | 41-55 ou sala de fogo | Burn, resistência a calor |
| Gélido | 26-40 ou sala de gelo | Chill, resistência a frio |
| Raiz-Negra | floresta/corrupção | Root/Poison leve |

---

# PARTE B — Movimento, comportamento e IA

## 8. Movement profiles oficiais

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

## 9. Behavior profiles oficiais

```text
Predator
Scavenger
TerritorialGuard
ResourceGuardian
TreasureTrap
FactionPatrol
PackHunter
CasterSupport
EliteDuelist
SwarmPressure
Ambusher
BurrowPredator
ConstructProtocol
CultRitualist
CorruptedFrenzy
AberrantController
BossMultiPhase
LoreGuardian
```

## 10. Regras de IA por papel

| Papel | Comportamento esperado |
|---|---|
| Chaser | persegue, pressiona, força movimento |
| Guard | segura posição, protege node/baú/porta |
| Ranged | mantém distância, usa linha de visão |
| Caster | alterna cast, reposiciona, cria zona |
| Burrower | some/emerge com telegraph, pune jogador parado |
| Swarm | cerca, pressiona, morre rápido |
| Tank | bloqueia passagem, abre janela após ataque pesado |
| Controller | aplica Slow/Root/Fear/ConfusionLite com telegraph |
| TreasureTrap | fica passivo até interação/proximidade |
| Elite | tem pelo menos 2 ações e 1 janela de vulnerabilidade |
| Boss | tem 2-3 fases e comportamento/movimento por fase |

## 11. Boss phases

```text
Phase 1 — 100% a 70% HP
  leitura principal, ataques básicos, arena ainda simples.

Phase 2 — 70% a 35% HP
  muda movimento ou ganha nova ação.
  pode chamar adds, ativar hazards ou mudar arena.

Phase 3 — 35% a 0% HP
  comportamento agressivo, maior mobilidade ou controle.
  deve manter counterplay e telegraph claro.
```

Regras:

```text
Não usar ataque instantâneo sem telegraph.
Não prender o jogador sem saída.
Não fazer boss depender só de HP inflado.
Toda fase deve ter janela de vulnerabilidade clara.
```

---

# PARTE C — Factions, roles e size

## 12. Factions oficiais da caverna

```text
faction_beast
faction_fungal
faction_goblin
faction_kobold
faction_orc
faction_duergar
faction_drow
faction_gnome
faction_ninrorin
faction_undead
faction_cultist
faction_elemental
faction_construct
faction_abyssal
faction_corrupted
faction_draconic
faction_aberrant
```

## 13. Size classes e footprint visual

| SizeClass | Sprite visual sugerido | Collider/footbox | Uso |
|---|---:|---:|---|
| Tiny | 16x16 a 24x24 px | 12x8 px | enxames, wisps, ácaros |
| Small | 24x32 a 32x40 px | 16x10 px | goblins, kobolds, diabretes |
| Medium | 32x48 px | 20x12 a 24x16 px | humanoides, cultistas, mortos |
| Large | 48x56 a 64x64 px | 32x20 px | orcs grandes, feras, constructos |
| Huge | 80x80 a 96x96 px | 48x28 px | hulks, colossos, monstros de sala |
| Boss | 96x96 a 192x160 px | custom | boss gate e level 101 |

---

# PARTE D — Roster com atributos reais

Formato de cada entrada:

```text
enemy_id | Nome
Nível nativo: X-Y | Size ... | Behavior ... | Move ...
HP ... | MP ... | STA ... | FOR ... | CON ... | DES ... | INT ... | VON ... | CAR ... | XP ...
Traits: ...
Ataques: ...
Drops: ...
```

## 14. Níveis 1-10 — Caverna de Pedra

```text
enemy_cave_mite | Ácaro da Fenda
Nível nativo: 1-8 | Size Tiny | Behavior SwarmPressure | Move SwarmErratic
HP 28 | MP 0 | STA 20 | FOR 2 | CON 3 | DES 7 | INT 1 | VON 2 | CAR 0 | XP 8
Traits: SwarmBody, FastRecovery
Ataques: BiteSwarm, ShortHop
Drops: quitina pequena, pedra miúda

enemy_stone_rat | Rato de Basalto
Nível nativo: 1-10 | Size Small | Behavior Predator | Move GroundChase
HP 42 | MP 0 | STA 28 | FOR 4 | CON 5 | DES 6 | INT 1 | VON 2 | CAR 0 | XP 10
Traits: LongChase
Ataques: Bite, DashBite
Drops: pele áspera, carvão baixo

enemy_cave_bat | Morcego de Fenda
Nível nativo: 2-10 | Size Tiny | Behavior PackHunter | Move SwarmErratic/FloatingOrbit
HP 32 | MP 8 | STA 26 | FOR 2 | CON 2 | DES 8 | INT 2 | VON 3 | CAR 0 | XP 12
Traits: FloatingBody, PackCoordination, FastRecovery
Ataques: EchoPulse, ScratchDive
Drops: asa fina, eco mineral

enemy_goblin_grashnaar_scavenger | Saqueador Grash'naar
Nível nativo: 3-12 | Size Small | Behavior Scavenger/FactionPatrol | Move PackFlanker
HP 58 | MP 0 | STA 36 | FOR 4 | CON 4 | DES 7 | INT 4 | VON 3 | CAR 3 | XP 16
Traits: PackCoordination, RetreatAndCall
Ataques: Shiv, StoneThrow, RetreatAndCall
Drops: sucata, cobre baixo, moeda

enemy_kobold_scout | Batedor Kobold
Nível nativo: 3-14 | Size Small | Behavior FactionPatrol | Move KiteRanged/PackFlanker
HP 54 | MP 0 | STA 38 | FOR 3 | CON 4 | DES 7 | INT 5 | VON 3 | CAR 2 | XP 16
Traits: PackCoordination, Scout
Ataques: SpearJab, PebbleShot, MarkTarget
Drops: garra, pedra polida

enemy_mossling | Musguinho Errante
Nível nativo: 4-14 | Size Small | Behavior ResourceGuardian | Move TankSlowPush
HP 74 | MP 8 | STA 24 | FOR 4 | CON 8 | DES 2 | INT 1 | VON 5 | CAR 0 | XP 14
Traits: TankBody, PoisonAdapted, SlowRecovery
Ataques: BodySlam, SporePuff
Drops: esporo verde, fibra úmida

enemy_cracked_bone | Osso Rachado
Nível nativo: 5-15 | Size Medium | Behavior Predator | Move GroundChase
HP 82 | MP 0 | STA 30 | FOR 7 | CON 5 | DES 4 | INT 1 | VON 5 | CAR 0 | XP 18
Traits: UndeadBody, PhysicalStable
Ataques: BoneSwipe, Lunge
Drops: osso seco, pó mineral

enemy_blackroot_sprout | Broto Raiz-Negra
Nível nativo: 6-18 | Size Small | Behavior TerritorialGuard | Move GuardStationary
HP 68 | MP 18 | STA 18 | FOR 2 | CON 6 | DES 3 | INT 2 | VON 7 | CAR 0 | XP 18
Traits: RootedGuard, PoisonAdapted
Ataques: RootNeedle, ShortRoot
Drops: raiz negra fraca, seiva escura

enemy_translucent_sludge_cube | Cubo de Lodo Translúcido
Nível nativo: 7-18 | Size Large | Behavior TreasureTrap/Tank | Move TankSlowPush
HP 150 | MP 0 | STA 18 | FOR 8 | CON 12 | DES 1 | INT 1 | VON 6 | CAR 0 | XP 35
Traits: TankBody, TreasureAmbush, AcidBody, SlowRecovery
Ataques: AcidContact, EngulfSlow
Drops: gel ácido, moeda corroída, tesouro digerido

enemy_rust_beetle | Besouro Ferrugem
Nível nativo: 7-20 | Size Small | Behavior ResourceGuardian | Move GroundChase/HazardLure
HP 70 | MP 0 | STA 42 | FOR 3 | CON 5 | DES 9 | INT 2 | VON 4 | CAR 0 | XP 22
Traits: FastRecovery, DurabilityStress
Ataques: RustBite, DurabilityStress
Drops: carapaça oxidada, pó ferrugem

enemy_chest_biter | Baú-Mordente
Nível nativo: 8-22 | Size Medium | Behavior TreasureTrap | Move TreasureIdleAmbush/GroundChase
HP 130 | MP 0 | STA 30 | FOR 10 | CON 9 | DES 4 | INT 2 | VON 5 | CAR 0 | XP 40
Traits: TreasureAmbush, TankBody
Ataques: AmbushBite, TongueGrabShort
Drops: madeira viva, dente de baú, loot guardado
```

## 15. Níveis 11-25 — Floresta Subterrânea

```text
enemy_spore_imp | Diabrete de Esporo
HP 95 | MP 34 | STA 44 | FOR 4 | CON 6 | DES 11 | INT 7 | VON 8 | CAR 2 | XP 22
Behavior CasterSupport | Move CasterKeepAway | Traits: PoisonAdapted, FastRecovery | Ataques: SporeShot, PoisonCloudSmall

enemy_rootsnare | Garra-Raiz
HP 135 | MP 22 | STA 28 | FOR 8 | CON 13 | DES 3 | INT 2 | VON 10 | CAR 0 | XP 30
Behavior Ambusher/ResourceGuardian | Move BurrowAmbush | Traits: Burrower, RootedGuard, PhysicalStable | Ataques: RootGrab, ThornLash

enemy_hollow_stagling | Cervino Oco
HP 160 | MP 0 | STA 70 | FOR 10 | CON 9 | DES 14 | INT 2 | VON 6 | CAR 0 | XP 55
Behavior Predator | Move Leaper/ChargeLine | Traits: LeapPressure, ChargePressure, LongChase | Ataques: AntlerCharge, KickBack

enemy_goblin_urudakh_trapper | Armeiro Uru'dakh
HP 110 | MP 0 | STA 58 | FOR 6 | CON 7 | DES 12 | INT 12 | VON 7 | CAR 4 | XP 28
Behavior FactionPatrol | Move KiteRanged/RetreatAndCall | Traits: TrapUser, PackCoordination | Ataques: TrapPlace, JavelinThrow

enemy_thorn_archer | Espinhador Sombrio
HP 118 | MP 10 | STA 60 | FOR 7 | CON 7 | DES 14 | INT 8 | VON 7 | CAR 3 | XP 32
Behavior Ranged | Move KiteRanged | Traits: RangedDiscipline, RootAffinity | Ataques: ThornShot, PinningShot

enemy_orc_nyx_stalker | Espreitador Orc de Nyx
HP 170 | MP 18 | STA 72 | FOR 14 | CON 11 | DES 13 | INT 5 | VON 11 | CAR 3 | XP 48
Behavior PackHunter/Ambusher | Move PackFlanker/PhaseShortBlink leve | Traits: PackCoordination, Blinker, ShadowAdapted | Ataques: ShadowCleave, AmbushDash

enemy_mycobulwark | Baluarte Micélio
HP 280 | MP 26 | STA 42 | FOR 13 | CON 18 | DES 2 | INT 2 | VON 13 | CAR 0 | XP 75
Behavior ResourceGuardian/Tank | Move TankSlowPush/ProtectAnchor | Traits: TankBody, PoisonAdapted, ProtectAnchor | Ataques: Slam, SporeGuardAura

enemy_nyx_moth | Mariposa de Nyx
HP 90 | MP 42 | STA 50 | FOR 2 | CON 5 | DES 15 | INT 7 | VON 12 | CAR 2 | XP 38
Behavior CasterSupport/Swarm | Move FloatingOrbit | Traits: FloatingBody, ShadowAdapted, FastRecovery | Ataques: MoonDust, FearFlutter leve

enemy_root_owlbear | Urso-Coruja Raiz-Oca
HP 340 | MP 0 | STA 86 | FOR 18 | CON 17 | DES 9 | INT 2 | VON 9 | CAR 0 | XP 90
Behavior EliteDuelist/Predator | Move ChargeLine/Leaper | Traits: EliteBody, PostureHeavy, ChargePressure | Ataques: HeavyClaw, RootRoar, BeakCrush

enemy_basilisk_lizard | Lagarto Basilisco de Musgo
HP 240 | MP 38 | STA 48 | FOR 10 | CON 15 | DES 6 | INT 3 | VON 13 | CAR 0 | XP 85
Behavior Controller/Guard | Move GuardStationary/TankSlowPush | Traits: Controller, PoisonAdapted, GazeUser | Ataques: SlowGaze, VenomBite

enemy_panther_distorted | Pantera Distorcida
HP 220 | MP 24 | STA 92 | FOR 12 | CON 8 | DES 20 | INT 4 | VON 9 | CAR 0 | XP 95
Behavior EliteDuelist | Move PhaseShortBlink/PackFlanker | Traits: HighMobility, Blinker, FastRecovery | Ataques: PhasePounce, TwinClaw
```

## 16. Níveis 26-40 — Caverna de Gelo

```text
enemy_frost_gnawer | Roedor de Geada
HP 155 | MP 0 | STA 72 | FOR 9 | CON 9 | DES 16 | INT 2 | VON 7 | CAR 0 | XP 36
Behavior Predator | Move GroundChase | Traits: ColdAdapted, LongChase, PackCoordination | Ataques: ChillBite, PackRush

enemy_duergar_frostdelver | Escavador Duergar do Gelo
HP 230 | MP 0 | STA 76 | FOR 15 | CON 16 | DES 7 | INT 8 | VON 11 | CAR 2 | XP 48
Behavior FactionPatrol/ResourceGuardian | Move GroundPatrol | Traits: ColdAdapted, ResourceGuardian, PhysicalStable | Ataques: PickSwing, IceShardBreak

enemy_duergar_shieldbreaker | Quebra-Escudo Duergar
HP 520 | MP 0 | STA 84 | FOR 22 | CON 22 | DES 5 | INT 7 | VON 14 | CAR 2 | XP 105
Behavior Tank/Elite | Move TankSlowPush | Traits: EliteBody, GuardBreak, Shielded, PostureHeavy | Ataques: ShieldCrush, GuardBreak

enemy_icebound_sentinel | Sentinela Enregelado
HP 480 | MP 22 | STA 40 | FOR 18 | CON 24 | DES 3 | INT 5 | VON 18 | CAR 0 | XP 95
Behavior ConstructProtocol | Move GuardStationary/TankSlowPush | Traits: ConstructBody, ColdAdapted, TankBody, SlowRecovery | Ataques: IceSlam, ChillPulse

enemy_glassbone | Osso de Vidro
HP 210 | MP 16 | STA 58 | FOR 12 | CON 8 | DES 15 | INT 2 | VON 16 | CAR 0 | XP 52
Behavior Ranged/Chaser | Move GroundChase/KiteRanged | Traits: UndeadBody, PostureFragile, FastRecovery | Ataques: GlassShard, BoneLunge

enemy_cold_cult_acolyte | Acólito do Frio
HP 190 | MP 70 | STA 48 | FOR 5 | CON 8 | DES 8 | INT 15 | VON 18 | CAR 5 | XP 58
Behavior CasterSupport/CultRitualist | Move CasterKeepAway | Traits: CasterDiscipline, ColdAdapted, RitualAnchor | Ataques: FrostZone, ColdBolt

enemy_crystal_leaper | Saltador Cristalino
HP 330 | MP 18 | STA 104 | FOR 16 | CON 12 | DES 24 | INT 3 | VON 12 | CAR 0 | XP 110
Behavior EliteDuelist | Move Leaper/ChargeLine | Traits: HighMobility, FastRecovery, LeapPressure, PostureFragile | Ataques: CrystalLeap, ShardBurst

enemy_frost_wailer | Lamento Frio
HP 300 | MP 90 | STA 52 | FOR 4 | CON 11 | DES 10 | INT 14 | VON 24 | CAR 0 | XP 125
Behavior Caster/Controller | Move FloatingSlow/CasterKeepAway | Traits: FloatingBody, CasterDiscipline, MentalResistant, ColdAdapted | Ataques: WailPulse, ChillCone

enemy_hook_horror_ice | Horror-Gancho de Gelo
HP 560 | MP 0 | STA 88 | FOR 24 | CON 21 | DES 8 | INT 3 | VON 13 | CAR 0 | XP 135
Behavior EliteDuelist/TerritorialGuard | Move GroundChase | Traits: EliteBody, PostureHeavy, GuardBreak, ColdAdapted | Ataques: HookSweep, HookImpale

enemy_mind_eater_larva | Larva Devora-Mentes
HP 240 | MP 96 | STA 50 | FOR 3 | CON 8 | DES 12 | INT 19 | VON 18 | CAR 1 | XP 120
Behavior AberrantController | Move CasterKeepAway | Traits: AberrantMind, CasterDiscipline, MentalResistant | Ataques: PsychicPulse, AimDisrupt
```

## 17. Níveis 41-55 — Caverna de Fogo

```text
enemy_ember_tick | Carrapato de Brasa
HP 230 | MP 0 | STA 94 | FOR 8 | CON 9 | DES 23 | INT 1 | VON 8 | CAR 0 | XP 62
Behavior SwarmPressure | Move SwarmErratic | Traits: SwarmBody, HeatAdapted, FastRecovery | Ataques: EmberBite, MinorBurst

enemy_ash_crawler | Rastejante de Cinza
HP 360 | MP 18 | STA 82 | FOR 18 | CON 17 | DES 13 | INT 2 | VON 12 | CAR 0 | XP 76
Behavior Predator | Move GroundChase/HazardLure | Traits: HeatAdapted, LongChase, HazardAdapted | Ataques: AshClaw, BurnTrail

enemy_orc_kaand_berserker | Berserker Orc de Kaand
HP 760 | MP 0 | STA 120 | FOR 30 | CON 23 | DES 12 | INT 4 | VON 20 | CAR 5 | XP 170
Behavior EliteDuelist/CorruptedFrenzy | Move ChargeLine/GroundChase | Traits: EliteBody, ChargePressure, CorruptionAdapted | Ataques: RageCleave, LeapSmash

enemy_orc_kaand_ashcaller | Chamador de Cinzas de Kaand
HP 440 | MP 110 | STA 70 | FOR 10 | CON 14 | DES 9 | INT 17 | VON 24 | CAR 8 | XP 135
Behavior CasterSupport/CultRitualist | Move CasterKeepAway | Traits: CasterDiscipline, RitualAnchor, HeatAdapted | Ataques: AshCall, EmberBuff

enemy_lava_bulwark | Baluarte de Lava
HP 820 | MP 40 | STA 52 | FOR 24 | CON 32 | DES 3 | INT 2 | VON 18 | CAR 0 | XP 150
Behavior Tank/ResourceGuardian | Move TankSlowPush | Traits: TankBody, HeatAdapted, SlowRecovery | Ataques: LavaSlam, HeatAura

enemy_cinder_spitter | Cuspidor de Cinza
HP 340 | MP 30 | STA 76 | FOR 12 | CON 12 | DES 17 | INT 3 | VON 11 | CAR 0 | XP 92
Behavior Ranged | Move KiteRanged | Traits: HeatAdapted, FastRecovery, RangedDiscipline | Ataques: CinderSpit, SmokeRetreat

enemy_scorched_cultist | Cultista Chamuscado
HP 390 | MP 120 | STA 58 | FOR 7 | CON 12 | DES 9 | INT 20 | VON 22 | CAR 7 | XP 110
Behavior Caster/CultRitualist | Move CasterKeepAway | Traits: CasterDiscipline, RitualAnchor, HeatAdapted | Ataques: BurnSigil, FireBolt

enemy_furnace_warden | Guardião da Fornalha
HP 980 | MP 50 | STA 70 | FOR 26 | CON 34 | DES 4 | INT 10 | VON 24 | CAR 0 | XP 210
Behavior ConstructProtocol/Elite | Move ProtectAnchor/TankSlowPush | Traits: ConstructBody, TankBody, HeatAdapted, ProtectAnchor | Ataques: FurnaceHammer, FlameVent

enemy_fire_basilisk | Basilisco de Brasa
HP 720 | MP 80 | STA 72 | FOR 18 | CON 25 | DES 8 | INT 3 | VON 24 | CAR 0 | XP 185
Behavior Controller/Guard | Move GuardStationary/TankSlowPush | Traits: Controller, HeatAdapted, GazeUser | Ataques: HeatGaze, EmberBite

enemy_stone_bulette | Tubarão de Pedra
HP 1150 | MP 0 | STA 125 | FOR 34 | CON 32 | DES 10 | INT 2 | VON 18 | CAR 0 | XP 260
Behavior BurrowPredator/Elite | Move BurrowAmbush/ChargeLine | Traits: Burrower, EliteBody, ChargePressure, PostureHeavy | Ataques: EmergeCrush, StoneBite
```

## 18. Níveis 56-70 — Ruínas Antigas

```text
enemy_rune_shard | Lasca Rúnica
HP 360 | MP 85 | STA 56 | FOR 6 | CON 14 | DES 18 | INT 12 | VON 18 | CAR 0 | XP 98
Behavior SwarmPressure/ConstructProtocol | Move FloatingOrbit | Traits: ConstructBody, FloatingBody, FastRecovery | Ataques: RuneBolt, ArcSpark

enemy_clockwork_guard | Guarda de Corda
HP 680 | MP 30 | STA 70 | FOR 22 | CON 28 | DES 7 | INT 10 | VON 22 | CAR 0 | XP 130
Behavior ConstructProtocol | Move GroundPatrol/ProtectAnchor | Traits: ConstructBody, PhysicalStable, ProtectAnchor | Ataques: GearStrike, LockdownStep

enemy_gnome_gem_madcap | Gnomo de Gema Enlouquecido
HP 420 | MP 150 | STA 70 | FOR 6 | CON 12 | DES 16 | INT 28 | VON 16 | CAR 5 | XP 145
Behavior CasterSupport | Move KiteRanged | Traits: CasterDiscipline, ErraticCaster | Ataques: GemBurst, ErraticBolt

enemy_gnomorin_rune_tinker | Gnomorin Runa-Torta
HP 450 | MP 135 | STA 78 | FOR 7 | CON 13 | DES 18 | INT 30 | VON 18 | CAR 4 | XP 150
Behavior FactionPatrol/ConstructSupport | Move KiteRanged/RetreatAndCall | Traits: TrapUser, ConstructSupport, PackCoordination | Ataques: RuneTrap, GearShot

enemy_sealed_knight | Cavaleiro Selado
HP 1250 | MP 35 | STA 88 | FOR 32 | CON 36 | DES 8 | INT 5 | VON 30 | CAR 0 | XP 280
Behavior Tank/Elite | Move TankSlowPush | Traits: EliteBody, Shielded, PostureHeavy, MentalResistant | Ataques: SealedCleave, OathPulse

enemy_mirror_adept | Adepto do Espelho
HP 620 | MP 190 | STA 82 | FOR 8 | CON 14 | DES 20 | INT 30 | VON 26 | CAR 10 | XP 260
Behavior Caster/EliteDuelist | Move PhaseShortBlink | Traits: Blinker, CasterDiscipline, FastRecovery | Ataques: MirrorBolt, BlinkSlash

enemy_puzzle_golem | Golem de Enigma
HP 1500 | MP 80 | STA 58 | FOR 30 | CON 42 | DES 4 | INT 20 | VON 34 | CAR 0 | XP 320
Behavior ConstructProtocol/Tank | Move ProtectAnchor | Traits: ConstructBody, TankBody, VulnerabilityCycle, SlowRecovery | Ataques: LogicSlam, VulnerabilityCycle

enemy_oathless_shade | Sombra Sem-Juramento
HP 720 | MP 180 | STA 86 | FOR 12 | CON 16 | DES 24 | INT 18 | VON 34 | CAR 0 | XP 300
Behavior Caster/Phase | Move PhaseShortBlink/FloatingSlow | Traits: Blinker, FloatingBody, MentalResistant | Ataques: OathDrain, ShadeStep

enemy_beholder_kin_lesser | Observador Menor da Ruína
HP 1100 | MP 260 | STA 70 | FOR 8 | CON 24 | DES 14 | INT 32 | VON 32 | CAR 4 | XP 360
Behavior AberrantController/Elite | Move FloatingOrbit/BossArenaControl menor | Traits: AberrantMind, FloatingBody, BossArenaControl, MentalResistant | Ataques: BlackstoneBeamMinor, SlowGaze, EyeShardVolley

enemy_mimic_armory | Arsenal-Mordente
HP 1250 | MP 0 | STA 58 | FOR 34 | CON 36 | DES 8 | INT 8 | VON 22 | CAR 0 | XP 340
Behavior TreasureTrap/Tank | Move TreasureIdleAmbush/TankSlowPush | Traits: TreasureAmbush, TankBody, PhysicalStable | Ataques: AmbushBite, WeaponRackSlam

enemy_brain_jelly | Geleia-Memória
HP 760 | MP 230 | STA 50 | FOR 4 | CON 20 | DES 10 | INT 34 | VON 28 | CAR 0 | XP 310
Behavior AberrantController | Move FloatingSlow | Traits: AberrantMind, FloatingBody, MentalResistant | Ataques: MemoryPulse, ConfusionLiteWave
```

## 19. Níveis 71-85 — Abismo Sombrio

```text
enemy_drow_shadowblade | Lâmina Sombria Drow
HP 900 | MP 60 | STA 130 | FOR 22 | CON 20 | DES 40 | INT 18 | VON 26 | CAR 8 | XP 390
Behavior EliteDuelist/PackHunter | Move PackFlanker/PhaseShortBlink | Traits: HighMobility, Blinker, PackCoordination, ShadowAdapted | Ataques: ShadowBlade, BackstepSlash

enemy_drow_moon_caster | Conjurador Lunar Drow
HP 760 | MP 300 | STA 86 | FOR 8 | CON 18 | DES 20 | INT 38 | VON 42 | CAR 16 | XP 430
Behavior CasterSupport | Move CasterKeepAway/FloatingOrbit | Traits: CasterDiscipline, FloatingBody, ShadowAdapted | Ataques: MoonBolt, DarkZone

enemy_drow_web_scout | Batedor de Teia Drow
HP 780 | MP 80 | STA 120 | FOR 16 | CON 18 | DES 36 | INT 24 | VON 24 | CAR 8 | XP 360
Behavior Ranged/Controller | Move KiteRanged/PackFlanker | Traits: HighMobility, PackCoordination, Controller | Ataques: WebShotSlow, ShadowArrow

enemy_void_caster | Conjurador do Vazio
HP 950 | MP 380 | STA 82 | FOR 6 | CON 20 | DES 18 | INT 44 | VON 48 | CAR 4 | XP 520
Behavior AberrantController/Caster | Move CasterKeepAway | Traits: AberrantMind, CasterDiscipline, MentalResistant | Ataques: VoidPulse, GravitySnareSmall

enemy_abyss_wisp | Fagulha do Abismo
HP 420 | MP 170 | STA 90 | FOR 2 | CON 10 | DES 36 | INT 18 | VON 28 | CAR 0 | XP 240
Behavior Swarm/Caster | Move FloatingOrbit | Traits: FloatingBody, SwarmBody, FastRecovery, ShadowAdapted | Ataques: WispBolt, FearFlicker

enemy_moonless_hound | Cão Sem-Lua
HP 820 | MP 0 | STA 150 | FOR 30 | CON 22 | DES 38 | INT 4 | VON 22 | CAR 0 | XP 310
Behavior Predator/PackHunter | Move Leaper/PackFlanker | Traits: HighMobility, PackCoordination, LeapPressure, LongChase | Ataques: MoonlessPounce, PackHowl

enemy_black_lantern_cultist | Cultista da Lanterna Negra
HP 720 | MP 260 | STA 80 | FOR 8 | CON 18 | DES 16 | INT 34 | VON 40 | CAR 14 | XP 340
Behavior CultRitualist/CasterSupport | Move ProtectAnchor/CasterKeepAway | Traits: RitualAnchor, CasterDiscipline, ShadowAdapted | Ataques: BlackLanternFlare, FearGlare

enemy_oath_eater | Devorador de Juramento
HP 1750 | MP 180 | STA 95 | FOR 36 | CON 44 | DES 8 | INT 18 | VON 48 | CAR 0 | XP 560
Behavior Tank/Elite | Move TankSlowPush | Traits: EliteBody, TankBody, OathDrain, MentalResistant | Ataques: OathCrush, VowDrain

enemy_mind_eater_adult | Devora-Mentes Abissal
HP 1150 | MP 420 | STA 90 | FOR 8 | CON 20 | DES 18 | INT 52 | VON 44 | CAR 6 | XP 620
Behavior AberrantController/Elite | Move CasterKeepAway/PhaseShortBlink | Traits: AberrantMind, CasterDiscipline, MentalResistant, Blinker | Ataques: PsychicPulse, ConfusionLite, MindSpike

enemy_eye_tyrant_blackmoon | Tirano Ocular da Lua Negra
HP 4200 | MP 650 | STA 120 | FOR 10 | CON 52 | DES 20 | INT 58 | VON 58 | CAR 8 | XP 1600
Behavior MiniBoss/AberrantController | Move FloatingOrbit/BossArenaControl | Traits: BossBody, AberrantMind, FloatingBody, BossArenaControl, MentalResistant | Ataques: SlowGaze, FearGlare, ArcanePierce, MoonlessPulse
```

## 20. Níveis 86-99 — Núcleo Corrompido

```text
enemy_corrupt_hulk | Massa Corrompida
HP 2400 | MP 60 | STA 115 | FOR 58 | CON 62 | DES 8 | INT 4 | VON 44 | CAR 0 | XP 720
Behavior Tank/CorruptedFrenzy | Move TankSlowPush/ChargeLine | Traits: TankBody, CorruptionAdapted, PostureHeavy, ChargePressure | Ataques: CorruptSmash, GroundRupture

enemy_ninrorin_broken_oracle | Oráculo Ninrorin Quebrado
HP 1350 | MP 540 | STA 100 | FOR 6 | CON 24 | DES 20 | INT 64 | VON 62 | CAR 10 | XP 760
Behavior Caster/Controller | Move CasterKeepAway/PhaseShortBlink | Traits: CasterDiscipline, MentalResistant, Blinker, Controller | Ataques: BrokenProphecy, TimeShardSlow

enemy_corrupted_pseudodragon | Pseudodragão Corrompido
HP 980 | MP 260 | STA 150 | FOR 18 | CON 20 | DES 54 | INT 24 | VON 34 | CAR 8 | XP 620
Behavior Ranged/PackHunter | Move FloatingOrbit/Leaper | Traits: HighMobility, FloatingBody, CorruptionAdapted, PackCoordination | Ataques: CorruptSpit, WingDash

enemy_blackstone_wyvern | Wyvern de Pedra Negra
HP 9500 | MP 260 | STA 220 | FOR 72 | CON 68 | DES 42 | INT 18 | VON 52 | CAR 0 | XP boss/miniboss
Behavior BossMultiPhase/DraconicPredator | Move ChargeLine/Leaper/FloatingOrbit | Traits: BossBody, DraconicBody, CorruptionAdapted, ChargePressure, FloatingBody | Ataques: BlackstoneBite, TailSweep, CorruptCone

enemy_blackstone_cult_paragon | Paragon da Pedra Negra
HP 1300 | MP 520 | STA 100 | FOR 12 | CON 24 | DES 22 | INT 58 | VON 60 | CAR 18 | XP 680
Behavior CultRitualist/CasterSupport | Move CasterKeepAway | Traits: RitualAnchor, CasterDiscipline, CorruptionAdapted | Ataques: BlackstoneSigil, CorruptBolt

enemy_core_mirror | Reflexo do Núcleo
HP 1100 | MP 400 | STA 130 | FOR 14 | CON 22 | DES 52 | INT 42 | VON 44 | CAR 0 | XP 610
Behavior Phase/Controller | Move PhaseShortBlink | Traits: Blinker, HighMobility, Controller, CorruptionAdapted | Ataques: MirrorPierce, ReflectionStep

enemy_mana_warped_beast | Fera Distorcida por Mana
HP 1650 | MP 160 | STA 190 | FOR 52 | CON 38 | DES 52 | INT 6 | VON 36 | CAR 0 | XP 650
Behavior Predator/CorruptedFrenzy | Move Leaper/ChargeLine | Traits: HighMobility, ChargePressure, ManaWarped, CorruptionAdapted | Ataques: ManaClaw, WarpedPounce

enemy_anya_silent_echo | Eco Silencioso de Anya
HP especial | MP especial | STA especial | FOR 0 | CON 60 | DES 20 | INT 70 | VON 90 | CAR 0 | XP 0/especial
Behavior LoreGuardian | Move GuardStationary/FloatingSlow | Traits: LoreGuardian, NoNormalLoot, SpecialEncounter | Ataques: repelir, proteger, revelar lore; não farmável

enemy_beholder_blackstone | Observador Tirano da Pedra Negra
HP 6800 | MP 900 | STA 120 | FOR 8 | CON 64 | DES 22 | INT 76 | VON 74 | CAR 6 | XP 2600
Behavior Boss/AberrantController | Move FloatingOrbit/BossArenaControl | Traits: BossBody, AberrantMind, FloatingBody, CorruptionAdapted, BossArenaControl | Ataques: BlackstoneBeam, SlowGaze, FearGlare, CorruptionPulse

enemy_umber_hulk_corebreaker | Titã Escavador Quebra-Núcleo
HP 3200 | MP 0 | STA 220 | FOR 78 | CON 70 | DES 14 | INT 6 | VON 46 | CAR 0 | XP 950
Behavior BurrowPredator/Tank | Move BurrowAmbush/ChargeLine | Traits: Burrower, TankBody, PostureHeavy, ChargePressure | Ataques: CoreBreakCharge, MandibleCrush

enemy_core_devourer_slime | Lodo Devorador de Núcleo
HP 3600 | MP 220 | STA 60 | FOR 46 | CON 82 | DES 3 | INT 12 | VON 58 | CAR 0 | XP 880
Behavior Tank/Controller/TreasureTrap | Move TankSlowPush | Traits: TankBody, AcidBody, CorruptionAdapted, TreasureAmbush, SlowRecovery | Ataques: AcidFlood, Engulf, CorruptGelZone
```

---

# PARTE E — Bosses e fases

## 21. Boss gates 15/30/45/60/75/90/100

```text
boss_blackroot_matriarch | Gate 15
HP 1200 | MP 180 | STA 90 | FOR 24 | CON 34 | DES 8 | INT 12 | VON 28 | CAR 0
Traits: BossBody, RootedGuard, PoisonAdapted, RitualAnchor
Fase 1 100-70: ProtectAnchor, RootSwipe, SporeShot.
Fase 2 70-35: invoca rootsnare/blackroot_sprout, cria zonas de raiz.
Fase 3 35-0: fica imóvel, aumenta alcance de raiz, janela após cast longo.

boss_duergar_frost_captain | Gate 30
HP 2000 | MP 80 | STA 130 | FOR 38 | CON 44 | DES 12 | INT 18 | VON 34 | CAR 8
Traits: BossBody, ColdAdapted, Shielded, GuardBreak, PackLeader
Fase 1: patrulha pesada, escudo e martelo.
Fase 2: chama duergar patrol, usa GuardBreak.
Fase 3: quebra escudo, perde defesa, aumenta dano e velocidade.

boss_kaand_ember_champion | Gate 45
HP 3300 | MP 80 | STA 170 | FOR 54 | CON 42 | DES 20 | INT 8 | VON 44 | CAR 8
Traits: BossBody, HeatAdapted, ChargePressure, CorruptedFrenzy
Fase 1: duelos frontais e cleaves.
Fase 2: rage aura, investidas em linha, adds orc/ember.
Fase 3: menos defesa, mais agressão, grandes janelas após LeapSmash.

boss_bromecian_puzzle_colossus | Gate 60
HP 5200 | MP 240 | STA 80 | FOR 50 | CON 70 | DES 4 | INT 42 | VON 58 | CAR 0
Traits: BossBody, ConstructBody, VulnerabilityCycle, SlowRecovery
Fase 1: ataques lentos e VulnerabilityCycle.
Fase 2: ativa runas de arena e constructos pequenos.
Fase 3: expõe núcleo, alterna defesa alta e vulnerabilidade clara.

boss_moonless_drow_hierophant | Gate 75
HP 7600 | MP 720 | STA 130 | FOR 12 | CON 34 | DES 34 | INT 62 | VON 72 | CAR 28
Traits: BossBody, CasterDiscipline, ShadowAdapted, RitualAnchor, Blinker
Fase 1: kiting/casts lunares.
Fase 2: teleporte curto, sombras e adds drow.
Fase 3: ritual interrompível; forte dano mágico, baixa tolerância a pressão corpo a corpo.

boss_blackstone_wyvern | Gate 90
HP 11000 | MP 320 | STA 240 | FOR 82 | CON 76 | DES 46 | INT 20 | VON 60 | CAR 0
Traits: BossBody, DraconicBody, CorruptionAdapted, ChargePressure, FlyingPhase
Fase 1: mordida, cauda, reposicionamento.
Fase 2: voo baixo, investidas e CorruptCone.
Fase 3: asa danificada, menos mobilidade, golpes mais fortes e telegraph maior.

boss_core_oathbreaker | Gate 100
HP 17500 | MP 620 | STA 220 | FOR 78 | CON 92 | DES 22 | INT 50 | VON 95 | CAR 0
Traits: BossBody, CorruptionAdapted, OathDrain, PostureHeavy, RitualAnchor
Fase 1: tanque ritual, golpes pesados e bloqueios.
Fase 2: quebra arena, invoca reflexos/juramentos quebrados.
Fase 3: perde forma, alterna CorruptionPulse e HeavySmash; janela após EnragePulse.
```

## 22. Bosses do nível 101

```text
boss_blackstone_warden_prime
HP 19000 | MP 700 | STA 220 | FOR 88 | CON 100 | DES 18 | INT 48 | VON 100 | CAR 0
Traits: BossBody, CorruptionAdapted, TankBody, ArenaControl
Fases: guardião posicional -> arena com pilares -> núcleo exposto.

boss_elyndor_oath_construct
HP 21000 | MP 900 | STA 160 | FOR 72 | CON 110 | DES 12 | INT 86 | VON 100 | CAR 0
Traits: BossBody, ConstructBody, VulnerabilityCycle, LoreGuardian
Fases: protocolo defensivo -> puzzle/vulnerability cycle -> overload.

boss_nyx_broken_herald
HP 17500 | MP 1200 | STA 180 | FOR 28 | CON 58 | DES 70 | INT 96 | VON 110 | CAR 30
Traits: BossBody, Blinker, ShadowAdapted, CasterDiscipline, RitualAnchor
Fases: sombra/caster -> clones/teleporte -> ritual instável.

boss_draconic_core_remnant
HP 26000 | MP 850 | STA 300 | FOR 120 | CON 110 | DES 55 | INT 48 | VON 90 | CAR 0
Traits: BossBody, DraconicBody, CorruptionAdapted, ChargePressure, CoreExposedPhase
Fases: predador terrestre -> cone corrompido -> núcleo exposto e fúria final.

boss_anya_bound_echo
HP especial | MP especial | STA especial | FOR 0 | CON 120 | DES 30 | INT 140 | VON 180 | CAR 0
Traits: LoreGuardian, NoNormalLoot, SpecialEncounter, AnyaEcho
Fases: não é luta comum. Deve alternar proteção, teste de memória e escolha/interação. Não farmável.
```

---

# PARTE F — Packs com lógica ecológica/social

## 23. Regras de composição

Cada pack precisa ter motivo para existir.

Motivos válidos:

```text
mesma facção
predador + presas/carcaças
guardião + recurso protegido
culto + invocação/símbolo
constructo + operador/ruína
mimic/slime + tesouro digerido
fungos + bestas contaminadas
Nyx/drow + criaturas abissais
Pedra Negra + corrompidos/dracônicos
```

Evitar packs sem explicação, como monstros aleatórios de biomas incompatíveis.

## 24. Packs por faixa

```text
pack_stone_swarm_low
Composição: cave_mite x6-10 + stone_rat x2-4
Motivo: pequenos predadores disputam restos e carcaças em túneis baixos.

pack_goblin_kobold_low
Composição: goblin_grashnaar_scavenger x4-6 + kobold_scout x2-4
Motivo: aliança oportunista; goblins saqueiam, kobolds mapeiam rotas seguras.

pack_fungal_low
Composição: mossling x3-5 + blackroot_sprout x3-5 + cave_mite x2-4
Motivo: fungos colonizam sala úmida; insetos vivem da decomposição.

pack_sludge_treasure_low
Composição: translucent_sludge_cube x1 + stone_rat x2-3 + chest_biter x0-1
Motivo: tesouro/carcaça atrai lodo, ratos e imitações predatórias.

pack_rust_miner_low
Composição: rust_beetle x2-4 + cave_mite x4-6
Motivo: besouros procuram metal exposto; ácaros seguem a trilha mineral.

pack_spore_grove
Composição: spore_imp x4-7 + mossling x4-6 + nyx_moth x1-3
Motivo: bosque fúngico; mariposas são atraídas por esporos luminescentes.

pack_urudakh_ambush
Composição: goblin_urudakh_trapper x2-4 + thorn_archer x3-5 + goblin_grashnaar_scavenger x3-5
Motivo: grupo organizado de emboscada e saque.

pack_nyx_stalkers
Composição: orc_nyx_stalker x2-4 + nyx_moth x4-8 + panther_distorted x0-1
Motivo: caçada noturna; orcs seguem sinais de Nyx e predadores distorcidos acompanham sombra.

pack_basilisk_grove
Composição: basilisk_lizard x1 + rootsnare x2-3 + thorn_archer x2-4
Motivo: criatura territorial protegida por vegetação e arqueiros que conhecem o terreno.

pack_root_owlbear_den
Composição: root_owlbear x1 + hollow_stagling x1-2 + spore_imp x2-4
Motivo: covil de predador; cervinos contaminados e esporos marcam território.

pack_duergar_patrol
Composição: duergar_frostdelver x4-6 + duergar_shieldbreaker x1-2 + frost_gnawer x2-3
Motivo: patrulha de mineração; roedores seguem restos e calor corporal.

pack_frozen_guard
Composição: icebound_sentinel x1-3 + cold_cult_acolyte x2-4 + glassbone x2-4
Motivo: culto usa constructos e mortos congelados para proteger passagens.

pack_hook_horror_cave
Composição: hook_horror_ice x1-2 + frost_gnawer x4-6 + cold_cult_acolyte x1-2
Motivo: horror ocupa caverna; cultistas deixam presas/roedores como alarme vivo.

pack_ember_swarm
Composição: ember_tick x8-12 + ash_crawler x3-5
Motivo: ecologia de calor; enxames vivem nas cinzas que rastejantes revolvem.

pack_kaand_warband
Composição: orc_kaand_berserker x2-3 + orc_kaand_ashcaller x1-2 + ash_crawler x3-5
Motivo: bando de guerra ritual; ashcallers sustentam berserkers.

pack_furnace_room
Composição: furnace_warden x1-2 + scorched_cultist x2-4 + ember_tick x6-10
Motivo: sala de fornalha antiga; cultistas alimentam a forja, constructos guardam.

pack_stone_bulette_ambush
Composição: stone_bulette x1 + ash_crawler x3-5 + lava_bulwark x0-1
Motivo: predador subterrâneo caça sob túneis quentes; elementais ocupam rocha exposta.

pack_gnome_ruin_team
Composição: gnome_gem_madcap x2-4 + gnomorin_rune_tinker x3-5 + rune_shard x3-5
Motivo: equipe enlouquecida tentando consertar ruínas.

pack_puzzle_guard
Composição: puzzle_golem x1 + mirror_adept x1-2 + clockwork_guard x3-5 + rune_shard x3-5
Motivo: sala de mecanismo; golem é núcleo, guardas e runas são defesa.

pack_observer_ruin_minor
Composição: beholder_kin_lesser x1 + rune_shard x4-6 + mirror_adept x1
Motivo: observador usa ruínas como ninho e atrai magia refletida.

pack_mimic_armory_room
Composição: mimic_armory x1-2 + clockwork_guard x2-4 + gnomorin_rune_tinker x1-2
Motivo: arsenal antigo infectado; constructos ainda protegem armas.

pack_drow_patrol
Composição: drow_shadowblade x2-4 + drow_web_scout x3-5 + abyss_wisp x4-7
Motivo: patrulha drow guiada por wisps abissais.

pack_moon_cult
Composição: drow_moon_caster x1-3 + black_lantern_cultist x2-4 + moonless_hound x3-5
Motivo: célula ritual de Nyx; cães rastreiam intrusos.

pack_mind_eater_cell
Composição: mind_eater_adult x1-2 + abyss_wisp x4-6 + black_lantern_cultist x1-2
Motivo: horror psíquico manipula cultistas e usa wisps como sensores.

pack_blackmoon_eye_room
Composição: eye_tyrant_blackmoon x1 + drow_moon_caster x1-2 + abyss_wisp x5-8
Motivo: mini-arena abissal; drow preservam o observador como oráculo monstruoso.

pack_corrupted_core
Composição: corrupt_hulk x1-2 + blackstone_cult_paragon x2-4 + core_mirror x3-5
Motivo: culto da Pedra Negra usa massas corrompidas como muralhas vivas.

pack_draconic_corruption
Composição: corrupted_pseudodragon x4-6 + mana_warped_beast x1-3 + core_mirror x1-3
Motivo: corrupção dracônica contamina fauna e reflexos do núcleo.

pack_beholder_blackstone_chamber
Composição: beholder_blackstone x1 + core_mirror x3-5 + blackstone_cult_paragon x2-4
Motivo: observador tirano é foco de culto e distorção visual.

pack_corebreaker_mining_room
Composição: umber_hulk_corebreaker x1-2 + corrupt_hulk x1 + mana_warped_beast x2-4
Motivo: mining chamber rara; escavadores abrem passagem e corrompidos protegem.

pack_core_devourer_treasure
Composição: core_devourer_slime x1 + chest_biter x1-2 scaled + core_mirror x2-4
Motivo: sala de tesouro digerido; lodo e mímicos vivem do mesmo atrativo.

pack_anya_echo_event
Composição: anya_silent_echo x1
Motivo: evento de lore, não pack de combate comum. Sem loot normal.
```

---

# PARTE G — Poderes e janelas

## 25. Ataques comuns

```text
MeleeBite
MeleeClaw
MeleeHeavySmash
ShortDash
ShortLeap
RangedStoneThrow
RangedSporeShot
RangedShardShot
RangedCinderSpit
CasterPulse
CasterZoneSmall
GuardBlock
BurrowEmerge
PhaseShortBlink
FloatingRay
FloatingOrbitRay
TreasureAmbushBite
AcidContact
PsychicPulse
EyeBeamBlackstone
PackCall
ProtectiveAura
CorruptCone
```

## 26. Status permitidos na caverna

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
VulnerabilityWindow
```

Regras:

```text
ConfusionLite não tira controle total do jogador.
DurabilityStress não destrói item permanentemente sem spec própria.
Petrificação instantânea não existe.
Controle permanente não existe.
Todo status forte exige telegraph e cooldown.
```

## 27. Poderes de observadores oculares

```text
Blackstone Beam — dano mágico/escuro em linha reta com telegraph.
Slow Gaze — aplica Slow curto se o jogador permanecer no cone.
Fear Glare — aplica Medo leve com cooldown alto.
Arcane Pierce — projétil único rápido, evitável.
Corruption Pulse — zona circular curta ao redor do boss.
Eye Shard Volley — vários projéteis fracos com intervalo.
```

## 28. Vulnerability windows

```text
AfterAttackRecover
DuringChargeWindup
AfterBurrowEmerges
AfterCast
AfterProjectileVolley
AfterShieldDrop
AfterBlinkArrival
AfterEnragePulse
AfterEyeBeam
AfterTreasureReveal
AfterPhaseTransition
AlwaysForTest apenas debug
```

---

# PARTE H — Specs futuras derivadas

```text
spec_cave_enemy_attribute_scaling_rules.md
spec_cave_enemy_behavior_movement_profiles.md
spec_cave_enemy_packs_ecology_and_density_rebalance.md
spec_cave_monster_roster_data_expansion.md
spec_cave_enemy_actions_vulnerability_profiles.md
spec_cave_iconic_dungeon_archetypes_vaalara_adaptation.md
spec_cave_boss_phase_behaviours_gate_15_100.md
spec_cave_level_101_boss_phase_behaviours.md
spec_cave_bestiary_entries_lore_rewards.md
spec_cave_loot_tables_by_biome_and_faction.md
```

---

# PARTE I — Decisões fechadas

```text
O campo BR foi removido do roster.
Todo monstro mantém HP, MP e STA.
MP 0 é permitido para criaturas puramente físicas.
STA deve existir em todos os monstros.
Funções antigas de BR agora são descritas por Move, Behavior e Traits.
A escala antiga de atributos 1-8 não deve mais ser usada como atributo real.
Atributos de monstros usam valores reais compatíveis com a progressão do player.
Player começa com 1 em cada atributo principal e ganha +1 ponto por level.
Monstros comuns têm atributos dominantes acima do player médio esperado para a faixa.
Elites têm múltiplos atributos dominantes e pelo menos 2 ações relevantes.
Bosses têm 2-3 fases de comportamento e movimento conforme HP restante.
Packs precisam ter justificativa ecológica, social, faccional ou de sala.
Criaturas inspiradas em arquétipos clássicos entram apenas como adaptações de Vaalara.
Observadores oculares, mímicos, lodos, devora-mentes, besouros ferrugem, feras distorcidas, urso-coruja, tubarão de pedra e titãs escavadores são adaptações próprias de Vaalara.
```

---

# PARTE J — Pendências

```text
Rodar grep local por termos removidos.
Converter este roster em EnemyDataSO/EnemyActionSO em specs futuras.
Definir dano por ação em cada EnemyActionSO.
Definir stamina cost por ação inimiga.
Definir MP cost por magia/ação especial inimiga.
Definir Armor/Resistance por família de monstro.
Definir Posture/GuardBreak por elite e boss.
Definir Traits como enum/data asset.
Definir loot tables por bioma/faction.
Definir bestiary entries de lore e drops.
```
