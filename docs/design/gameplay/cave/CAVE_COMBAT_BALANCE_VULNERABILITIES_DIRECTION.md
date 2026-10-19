# Cindar's Hope — Cave Combat Balance & Vulnerabilities Direction

> **Status:** documento canônico complementar de balanceamento, vulnerabilidades, janelas de abertura/crítico e orçamento de combate da caverna  
> **Local:** `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`  
> - `docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md`  
> - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`  
> **Função:** garantir que a caverna seja difícil, rica e possível, definindo limites de combate ativo, expectativa de companions/pets/itens, vulnerabilidades, janelas de punição, janelas críticas, consumo esperado de Stamina e pacing.  
> **Não é spec implementável.** Specs futuras devem transformar estas regras em dados/sistemas.

---

## 0. Regra de uso

Toda spec que mexer em inimigos, packs, bosses, dano, status, armas, companions, pets, spawn density, active budget, loot, balanceamento, vulnerabilidades, stamina ou boss phases da caverna deve ler este documento.

Regra principal:

```text
A caverna deve ser difícil, mas possível.
O desafio deve vir de leitura, preparação, uso de recursos, vulnerabilidades e decisões de combate.
Não deve vir de HP inflado sem janelas, controle injusto do jogador ou dano impossível de ler.
```

Observação importante:

```text
A palavra Breath pode continuar existindo em nomes de ataques de sopro de criaturas.
Ela não representa mais atributo, barra, recurso, BR ou Fôlego do personagem/monstro.
Exemplos válidos: AfterBreath, CorruptBreath, DragonBreath, FrostBreath.
```

---

# PARTE A — Premissas de balanceamento

## 1. Player, companion, pet e itens

A dificuldade da caverna deve assumir uma progressão gradual.

| Faixa | Player solo | Companion | Pet | Itens/gear esperados |
|---:|---|---|---|---|
| 1-10 | deve ser possível | opcional | opcional | arma/ferramenta inicial + comida simples |
| 11-25 | possível, mas difícil | recomendado | útil | arma melhorada, comida, poções básicas |
| 26-40 | difícil solo | recomendado forte | útil | resistência a frio, arma/skill melhorada |
| 41-55 | não ideal solo | esperado | útil | resistência a calor, poções, comida forte |
| 56-70 | solo só com build boa | esperado | recomendado | gear intermediário/avançado, consumíveis |
| 71-85 | solo apenas avançado | esperado | recomendado | build definida, resistências, skill tree |
| 86-99 | solo extremo | esperado | recomendado forte | gear avançado, consumíveis, preparo sério |
| 100 | preparação completa | esperado | recomendado | boss kit completo |
| 101 | conteúdo endgame | esperado/permitido conforme design | opcional/restrito por fase | recuperação entre bosses e build final |

Regra:

```text
O início da caverna não pode exigir companion/pet.
A partir do meio do jogo, companions/pets/itens passam a ser parte esperada da dificuldade.
Skills, gear, companions, pets, comida e domínio do jogador devem mitigar naturalmente os custos altos de Stamina.
O começo pode e deve ser difícil, desde que o jogador tenha rotas de recuo e aprendizado.
```

## 2. Floor level não é igual a player level

O andar da caverna não deve ser tratado como igual ao level do jogador.

Cada faixa deve declarar:

```text
RecommendedPowerTier
ExpectedGearTier
ExpectedConsumableTier
ExpectedCompanionSupport
ExpectedPetSupport
```

Direção:

```text
Um jogador habilidoso e bem equipado pode avançar acima do power tier recomendado.
Um jogador mal preparado deve conseguir recuar, farmar, melhorar gear e voltar.
```

---

# PARTE B — Orçamento de inimigos ativos

## 3. Planned enemies vs active enemies

A nova densidade da caverna pode manter muitos inimigos planejados por andar, mas nem todos devem estar ativos ao mesmo tempo.

```text
EnemiesPlannedInSnapshot = total persistente do andar.
EnemiesActiveNearPlayer = inimigos atualmente materializados/agressivos perto do jogador.
EnemiesDormant/Inactive = inimigos planejados, mas fora do orçamento ativo.
```

Regra:

```text
A identidade dos inimigos planejados deve persistir no snapshot.
A materialização pode ser controlada por proximidade, salas, portas, triggers ou spawn groups.
```

## 4. Active combat budget por faixa

| Faixa | Inimigos planejados | Ativos simultâneos alvo | Ativos simultâneos máximo | Elites ativos máximo |
|---:|---:|---:|---:|---:|
| 1-10 | 22-34 | 5-8 | 10 | 1 |
| 11-25 | 28-42 | 7-10 | 12 | 1 |
| 26-40 | 30-44 | 8-12 | 14 | 2 |
| 41-55 | 32-48 | 10-14 | 16 | 2 |
| 56-70 | 34-50 | 10-16 | 18 | 3 |
| 71-85 | 36-52 | 12-18 | 20 | 3 |
| 86-99 | 38-56 | 14-20 | 22 | 4 |
| Boss gates | 12-32 + boss | controlado por fase | boss + 8 adds | 2-3 |
| 100 | 12-24 + boss final | controlado por fase | boss + 10 adds | 3 |
| 101 | bosses fixos | por encontro | definido por boss | boss-only ou adds roteirizados |

Regra:

```text
Se mais de um pack for puxado por erro de pathing/linha de visão, o sistema deve evitar avalanche injusta.
Não reduzir esta tabela apenas porque Stamina é cara: skills, companions, pets, consumíveis e progressão devem mitigar esse custo.
O desafio deve continuar alto.
```

## 5. Early game safety rules

Andares 1-3:

```text
sem elite obrigatório
sem treasure trap obrigatório
sem crowd control forte
sem inimigo Large obrigatório
packs pequenos
rota de fuga clara
```

Andares 4-7:

```text
0-1 elite opcional
primeira treasure trap rara
primeiro pack guardando recurso/baú
```

Andares 8-10:

```text
1 elite possível
primeiro miniboss raro opcional
primeira sala especial com risco real
```

---

# PARTE C — Vulnerabilidades

## 6. Todo monstro pode ter vulnerabilidades

Cada inimigo deve declarar ao menos uma destas categorias:

```text
ElementVulnerability
StatusVulnerability
AttackTypeVulnerability
WeaponVulnerability
BehavioralVulnerabilityWindow
```

Nem todo monstro precisa ser vulnerável a tudo. Mas todo monstro deve ter pelo menos um caminho de counterplay.

## 7. Tipos de vulnerabilidade

### ElementVulnerability

```text
Fire
Ice
Lightning
Water
Nature/Root
Light/Radiant
Shadow/Nyx
Arcane
Blackstone
Physical
```

### StatusVulnerability

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
Silence futuro
DurabilityStress apenas contra constructos/equipamentos, se existir
```

### AttackTypeVulnerability

```text
Slash
Pierce
Blunt
HeavyAttack
ChargedAttack
Backstab
RangedProjectile
MagicProjectile
AreaOfEffect
Trap
PetInterrupt
CompanionSkill
```

### WeaponVulnerability

```text
Sword
Axe
Hammer
Pickaxe
Spear
Bow
Staff
Dagger
Bomb/Explosive futuro
ToolAttack
```

## 8. Regra de dano de vulnerabilidade

A vulnerabilidade agora tem três níveis de abertura.

```text
MinorOpening
  abertura comum/curta.
  Não garante crítico automático.
  Pode dar +crit chance, +dano moderado, +posture damage ou menor custo de follow-up.

CriticalWindow
  janela clara, mais rara ou mais arriscada.
  Pode garantir crítico automático.
  Exige telegraph claro ou recompensa por execução.

CoreExposed / BossMechanicWindow / StaggeredWindow
  janela especial de mecânica, exposição de núcleo, quebra de postura ou fase de boss.
  Pode garantir crítico automático + bônus moderado de vulnerabilidade.
```

Multiplicadores conceituais:

```text
MinorOpening damage: x1.10 a x1.25 ou +15% a +35% crit chance.
Vulnerability damage: x1.25 a x1.50.
Critical hit: x1.50 a x2.00 conforme arma/build.
Critical + vulnerability: x2.00 a x2.50, nunca dano infinito.
CoreExposed/BossMechanicWindow: pode usar limite superior, mas com cooldown/fase clara.
Resistance: x0.50 a x0.75.
Immunity: evitar salvo boss/lore muito específico.
```

Regra:

```text
Nem toda janela comportamental vira crítico automático.
Crítico automático deve ser reservado para CriticalWindow, CoreExposed, StaggeredWindow ou mecânica bem telegrafada.
Evitar imunidades amplas.
Preferir resistência e janelas de counterplay.
```

---

# PARTE D — Janela de vulnerabilidade e crítico

## 9. Regra central

Todo monstro deve ter pelo menos uma janela de vulnerabilidade comportamental.

```text
BehavioralVulnerabilityWindow = momento de punição/counterplay.
Ela pode ser MinorOpening, CriticalWindow ou CoreExposed/BossMechanicWindow.
```

Regras:

```text
MinorOpening não garante crítico automático.
CriticalWindow pode garantir crítico automático.
CoreExposed/BossMechanicWindow pode garantir crítico automático + bônus moderado.
A spec final deve declarar a categoria de cada janela por inimigo/ação.
```

Isso pode valer para:

```text
ataques corpo a corpo
ataques carregados
projéteis
magias
skills
pet interrupt, se aplicável
companion skill, se aplicável
```

A spec final deve decidir se todo tipo de ataque pode critar ou se alguns tipos usam bônus específico.

## 10. Tipos de janela de vulnerabilidade

```text
AfterAttackRecover
DuringChargeWindup
AfterChargeMiss
AfterBurrowEmerges
DuringBurrowTell
AfterCast
DuringLongCast
AfterProjectileVolley
AfterShieldDrop
AfterBlockBreak
AfterBlinkArrival
AfterLeapLanding
AfterEyeBeam
AfterBreath
AfterTreasureReveal
AfterEnragePulse
AfterSummonAdds
AfterPhaseTransition
CoreExposed
BackTurned
StunnedWindow
RootedWindow
FrozenWindow
PetInterruptWindow
CompanionSetupWindow
```

Observação:

```text
AfterBreath é nome válido para janela depois de ataque de sopro de criatura.
Não tem relação com atributo Breath/Fôlego removido.
```

## 11. Duração sugerida das janelas

| Tipo de inimigo | Duração alvo | Observação |
|---|---:|---|
| Tiny/Small comum | 0.25s-0.60s | curto, mas frequente |
| Medium comum | 0.45s-0.90s | leitura clara |
| Large/Huge | 0.70s-1.40s | janela maior após ataques pesados |
| Elite | 0.50s-1.20s | menos frequente, mais valiosa |
| Boss phase normal | 0.80s-1.50s | após padrões maiores |
| Boss core exposed | 1.50s-3.00s | recompensa por executar mecânica |

Regra:

```text
Quanto mais letal o ataque do inimigo, maior ou mais clara deve ser a janela de punição.
Janelas críticas devem ser mais claras do que MinorOpenings.
```

## 12. Telegraph obrigatório

Janelas só funcionam bem se o jogador conseguir ler o momento.

Todo ataque relevante deve ter:

```text
windup
sinal visual/sonoro
execução
recovery
janela de vulnerabilidade quando aplicável
categoria da janela: MinorOpening, CriticalWindow ou CoreExposed/BossMechanicWindow
```

Bosses precisam de telegraph mais claro que mobs comuns.

---

# PARTE E — Vulnerabilidades por família de monstros

## 13. Fungal / Raiz-Negra

Exemplos:

```text
mossling
blackroot_sprout
spore_imp
rootsnare
mycobulwark
blackroot_matriarch
```

Vulnerabilidades:

```text
Element: Fire, Light/Radiant
Status: Burn, Stun moderado
AttackType: Slash contra raízes, AreaOfEffect contra enxames fúngicos
Weapon: Axe, Sword
Window: AfterSporeCast, AfterRootGrabMiss, CoreExposed em bosses
```

Resistências:

```text
Poison
Root
Nature/Root
```

## 14. Beast / predadores naturais

Exemplos:

```text
stone_rat
cave_bat
hollow_stagling
root_owlbear
moonless_hound
mana_warped_beast
```

Vulnerabilidades:

```text
Status: Bleed, Stun curto, Fear em alguns casos
AttackType: Backstab, HeavyAttack após investida errada
Weapon: Spear, Bow, Sword
Window: AfterChargeMiss, AfterLeapLanding, AfterPackHowl
```

Resistências:

```text
controle mental forte deve ser limitado
```

## 15. Goblin / Kobold / Humanoides de baixa faixa

Vulnerabilidades:

```text
Status: Fear, Stun, Bleed
AttackType: Backstab, RangedProjectile, CompanionSkill
Weapon: Sword, Dagger, Bow
Window: WhileRetreating, AfterTrapPlace, AfterThrow
```

Resistências:

```text
nenhuma ampla; vencem por número e tática
```

## 16. Orcs de Kaand

Vulnerabilidades:

```text
Element: Ice, Water, Light/Radiant
Status: Chill, Stun, Bleed
AttackType: Counter após RageCleave ou LeapSmash
Weapon: Spear, Hammer, Sword
Window: AfterLeapSmash, AfterRageCleave, DuringOverrage
```

Resistências:

```text
Burn
Fear reduzido
```

## 17. Duergar / Gelo

Vulnerabilidades:

```text
Element: Fire, Lightning
Status: Burn, Stun
AttackType: HeavyAttack, Blunt, Backstab contra casters
Weapon: Hammer, Pickaxe, Axe
Window: AfterShieldBreak, AfterPickSwing, AfterColdCast
```

Resistências:

```text
Ice
Chill
Blunt leve em unidades blindadas, salvo Hammer/HeavyAttack
```

## 18. Undead / Sombras

Vulnerabilidades:

```text
Element: Light/Radiant, Fire
Status: Stun sagrado/futuro, Fear não funciona bem
AttackType: Blunt contra ossos, MagicProjectile contra sombras
Weapon: Hammer, Staff, Sword encantada
Window: AfterWail, AfterShadeStep, AfterBoneLunge
```

Resistências:

```text
Poison
Bleed
Fear
```

## 19. Elementais

Vulnerabilidades:

```text
Fire elementals -> Water/Ice
Ice elementals -> Fire
Lava/stone -> Pickaxe, Hammer, Water
Crystal -> Blunt/HeavyAttack
```

Windows:

```text
AfterElementalBurst
AfterSlam
CoreExposed
```

## 20. Constructs / Bromecianos

Vulnerabilidades:

```text
Element: Lightning, Water em alguns casos
Status: Stun técnico/overload, Slow mecânico
AttackType: Blunt, HeavyAttack, ChargedAttack
Weapon: Hammer, Pickaxe
Window: AfterProtocolAttack, AfterShieldDrop, CoreExposed, OverloadWindow
```

Resistências:

```text
Poison
Bleed
Fear
```

## 21. Aberrants / observadores / devora-mentes / lodos

Vulnerabilidades:

```text
Element: Light/Radiant, Arcane controlado
Status: Stun curto, Silence futuro contra casters
AttackType: RangedProjectile nos olhos, HeavyAttack em lodos grandes, Backstab em devora-mentes
Weapon: Bow, Staff, Spear, Hammer contra lodos
Window: AfterEyeBeam, AfterPsychicPulse, AfterEngulfMiss, AfterTreasureReveal
```

Resistências:

```text
Fear parcial
ConfusionLite
Poison em lodos
```

Regra específica de observadores:

```text
Ataques no olho durante AfterEyeBeam podem ser CriticalWindow.
Ataques nas costas não são o counter principal.
```

## 22. Dracônicos / Pedra Negra

Vulnerabilidades:

```text
Element: Light/Radiant, Ice contra alguns, Arcane estabilizado futuro
Status: Stun curto, Chill moderado
AttackType: Pierce contra asas, HeavyAttack após breath, RangedProjectile em ponto fraco
Weapon: Spear, Bow, Hammer em placas
Window: AfterBreath, AfterTailSweep, WingExposed, CoreExposed
```

Resistências:

```text
Blackstone
Fear
Burn em alguns
```

Observação:

```text
breath/AfterBreath aqui significa ataque de sopro dracônico, não atributo de Fôlego.
```

## 23. Echo de Anya / Lore guardians

Regras:

```text
Não são monstros comuns.
Não devem ser farmáveis.
Podem ter vulnerabilidades mecânicas apenas para resolver encontro/teste.
O objetivo pode ser resistir, interagir, purificar ou interromper, não matar.
```

---

# PARTE F — Bosses e janelas críticas

## 24. Bosses devem ter vulnerabilidades por fase

Cada boss deve declarar:

```text
PhaseId
HPRange
MovementMode
MainAttackSet
SummonOrHazardRule
VulnerabilityWindow
WindowCategory
CriticalWindowRule
Element/Status/WeaponVulnerabilities
Resistances
StaminaPressureRule
```

## 25. Exemplo — Matriarca Raiz-Negra

```text
Phase 1 100-70
Move: ProtectAnchor
Vulnerabilities: Fire, Slash, Axe
Window: AfterRootSwipe
WindowCategory: MinorOpening ou CriticalWindow conforme telegraph

Phase 2 70-35
Move: GuardStationary + summon roots
Vulnerabilities: Fire, AreaOfEffect, CompanionSkill
Window: AfterSummonRoots
WindowCategory: CriticalWindow se o summon tiver long cast claro

Phase 3 35-0
Move: fixed core
Vulnerabilities: Fire, Light/Radiant, HeavyAttack
Window: CoreExposed after long cast
WindowCategory: CoreExposed
```

## 26. Exemplo — Wyvern de Pedra Negra

```text
Phase 1 100-70
Move: ChargeLine/Leaper
Vulnerabilities: Pierce, Bow, Spear
Window: AfterTailSweep
WindowCategory: MinorOpening ou CriticalWindow conforme recovery

Phase 2 70-35
Move: low flight / arena reposition
Vulnerabilities: Ice, RangedProjectile, WingHit
Window: AfterBreath
WindowCategory: CriticalWindow se o sopro tiver windup/recovery claro

Phase 3 35-0
Move: damaged wing / aggressive ground
Vulnerabilities: HeavyAttack, Hammer, Light/Radiant
Window: WingExposed or AfterCrashLanding
WindowCategory: CoreExposed/CriticalWindow
```

## 27. Exemplo — Observador Tirano da Pedra Negra

```text
Phase 1 100-70
Move: FloatingOrbit
Vulnerabilities: Bow, Staff, Arcane, Light/Radiant
Window: AfterEyeShardVolley
WindowCategory: MinorOpening

Phase 2 70-35
Move: BossArenaControl
Vulnerabilities: RangedProjectile no olho, Silence futuro, Stun curto
Window: AfterEyeBeam
WindowCategory: CriticalWindow

Phase 3 35-0
Move: unstable orbit + corruption pulse
Vulnerabilities: Light/Radiant, Arcane, ChargedAttack
Window: CentralEyeExposed after CorruptionPulse
WindowCategory: CoreExposed
```

## 28. Exemplo — Quebra-Juramento do Núcleo

```text
Phase 1 100-70
Move: TankSlowPush
Vulnerabilities: Hammer, HeavyAttack, Light/Radiant
Window: AfterHeavySmash
WindowCategory: CriticalWindow se o ataque for bem telegrafado

Phase 2 70-35
Move: BossArenaControl + summons
Vulnerabilities: CompanionSkill, AreaOfEffect, Arcane
Window: AfterSummonAdds or AfterShieldDrop
WindowCategory: MinorOpening/CriticalWindow conforme risco

Phase 3 35-0
Move: CorruptedFrenzy
Vulnerabilities: CoreExposed, Light/Radiant, Hammer
Window: AfterEnragePulse
WindowCategory: CoreExposed/CriticalWindow
```

---

# PARTE G — Pets e companions no balanceamento

## 29. Pet dog

O cachorro deve ajudar, mas não resolver combate sozinho.

Funções recomendadas:

```text
interromper cast fraco
marcar alvo
reduzir pressão de swarm
atrair 1-2 inimigos pequenos por pouco tempo
revelar treasure traps em chance baixa/média
abrir PetInterruptWindow em inimigos específicos
```

Não deve:

```text
tankar boss
matar elite sozinho
invalidar companion
ser obrigatório no early game
```

## 30. Cat/pet não-combatente

Gato pode ter papel indireto:

```text
sorte em loot
detecção de mimic/secret room
redução leve de Fear/Stress
bonus social/fazenda
```

## 31. Companions

Companions devem ser balanceados por função:

```text
Tank: segura pressão, mas não substitui dodge do jogador.
Ranged: ajuda em flying/floating enemies.
Healer/Support: sustenta long runs, mas com cooldown e recursos.
Controller: abre janelas, mas não stunlocka bosses.
Miner/Hybrid: ajuda em mining rooms e combate leve.
```

CompanionSkill pode criar janela em inimigos específicos:

```text
CompanionSetupWindow
PetInterruptWindow
ShieldBreakWindow
StaggerWindow
```

A categoria da janela deve ser declarada:

```text
MinorOpening
CriticalWindow
CoreExposed/BossMechanicWindow
```

---

# PARTE H — Time to kill, stamina e pacing

## 32. Time To Kill alvo

| Encontro | TTK alvo |
|---|---:|
| inimigo comum isolado | 5-20s |
| swarm pack pequeno | 20-45s |
| pack médio | 45-90s |
| elite isolado | 45-90s |
| elite pack | 90-180s |
| boss gate 15 | 2-4 min |
| boss gates 30-60 | 3-6 min |
| boss gates 75-100 | 5-8 min |
| cada boss 101 | 4-7 min |

Regra:

```text
Se o TTK real passar muito disso, o inimigo está virando esponja de HP.
Se ficar muito abaixo, o inimigo não cumpre papel de ameaça.
```

## 33. Observação de atrito de Stamina

Esta seção não define regra numérica de implementação.

Ela serve apenas para orientar validação, telemetria e playtest.

Durante testes, registrar:

```text
Stamina inicial do encontro
Stamina gasta em ataques
Stamina gasta em Dash/Dodge
Stamina gasta em Block hold
Stamina gasta em Block impact
Stamina recuperada durante o encontro
duração do encontro
uso de comida/poção
uso de companion/pet
janelas de vulnerabilidade exploradas
se o jogador precisou recuar
```

Leitura esperada:

```text
Encontros comuns devem ensinar ritmo sem obrigar consumo perfeito.
Comuns robustos devem gerar atrito perceptível.
Packs devem pressionar posicionamento, área, pet, companion ou recuo.
Elites devem exigir execução, janelas, vulnerabilidades, preparo ou consumíveis.
Bosses devem consumir múltiplos ciclos de Stamina ao longo de fases, com oportunidades claras de leitura e recuperação.
```

Regra:

```text
Não transformar esta seção em percentuais fixos de balanceamento.
Não usar esta seção para reduzir active combat budget.
Não exigir que specs futuras tentem bater números exatos de consumo de Stamina.
Usar apenas como guia de playtest para identificar extremos: encontros triviais, injustos, sem counterplay ou que invalidam skills/gear/companions.
```

## 34. Recuperação e atrito

A caverna pode ter atrito de HP/Stamina/itens, mas deve permitir decisões:

```text
continuar avançando
voltar para checkpoint
consumir recurso
usar companion/pet
evitar sala opcional
assumir risco por tesouro
```

Não deve:

```text
drenar recursos sem counterplay
forçar grinding artificial
travar o jogador sem rota de retorno
```

---

# PARTE I — Specs futuras derivadas

```text
spec_cave_active_enemy_budget.md
spec_cave_vulnerability_critical_windows.md
spec_cave_enemy_vulnerability_tables.md
spec_cave_boss_phase_vulnerabilities.md
spec_cave_companion_pet_combat_balance.md
spec_cave_time_to_kill_balance_targets.md
spec_cave_stamina_telemetry_playtest.md
spec_cave_treasure_trap_counterplay.md
```

---

# PARTE J — Decisões fechadas

```text
Todos os monstros devem ter pelo menos uma vulnerabilidade ou counterplay claro.
Vulnerabilidades podem ser de elemento, status, tipo de ataque, arma ou comportamento.
Todo monstro deve ter pelo menos uma janela de vulnerabilidade comportamental.
Nem toda janela comportamental causa crítico automático.
MinorOpening gera bônus moderado, maior chance de crítico, posture damage ou oportunidade tática.
CriticalWindow pode causar crítico automático.
CoreExposed/BossMechanicWindow pode causar crítico automático + bônus moderado.
Bosses devem ter vulnerabilidades e janelas por fase.
Breath pode continuar em nome de ataques de sopro de criaturas; não é atributo/recurso.
Inimigos planejados no snapshot não são iguais a inimigos ativos simultâneos.
A caverna deve usar active combat budget por faixa.
O active combat budget não foi reduzido pelo custo alto de Stamina.
Early game solo precisa ser possível, mas difícil.
Companions/pets/itens passam a ser expectativa progressiva, não requisito imediato.
TTK alvo deve ser usado para evitar inimigos esponja ou triviais.
Atrito de Stamina deve ser medido em playtest/telemetria, mas não deve virar regra percentual fixa.
```

---

# PARTE K — Pendências

```text
Definir DamageType final.
Definir CriticalHit contract.
Definir MinorOpening/CriticalWindow/CoreExposed data contract.
Definir VulnerabilityProfile data structure.
Definir CriticalWindow detector no EnemyBrain.
Definir integração com CompanionSkill e PetInterrupt.
Definir UI/feedback visual de MinorOpening, vulnerabilidade e crítico.
Definir se bestiário revela vulnerabilidades por descoberta ou automaticamente.
Definir Stamina telemetry em Play Mode.
Validar TTK real em Play Mode.
Validar consumo real de Stamina por faixa em Play Mode.
```
