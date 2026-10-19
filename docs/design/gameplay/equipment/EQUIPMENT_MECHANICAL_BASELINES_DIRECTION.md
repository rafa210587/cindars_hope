# Cindar's Hope — Equipment Mechanical Baselines Direction

> **Status:** documento canônico complementar de mecânicas numéricas iniciais de equipamentos  
> **Local:** `docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md`  
> **Complementa:** `docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md`  
> - `docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md`  
> - `docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`  
> - `docs/design/gameplay/enemies/ENEMY_BEHAVIORS_DIRECTION.md`  
> - `docs/design/gameplay/equipment/EQUIPMENT_ENEMY_VULNERABILITY_ADAPTER.md`  
> **Função:** explicitar os stats mecânicos iniciais de armas, armaduras, escudos, acessórios, arrows, wands, scrolls e materiais para specs futuras.  
> **Não é spec implementável.** Os valores são baseline de balance/playtest, não garantia final.

---

## 0. Regra de uso

Este documento existe para responder perguntas mecânicas como:

```text
quanto uma espada soma em WeaponDamage?
qual ASPD inicial de cada arma?
qual atributo escala cada item?
quanto pesa uma armor?
quanto altera Stamina?
qual efeito charged cada arma pode ter?
como bow/arrows funcionam?
como wands/scrolls entram no sistema mágico?
```

Regra:

```text
Os valores abaixo são baseline inicial de teste.
Specs futuras podem ajustar com telemetria, mas não devem inventar outro modelo sem registrar decisão.
Este documento define stats e tags aplicadas por equipamento.
Vulnerabilidades por família/inimigo pertencem ao roster/enemy data e ao Cave Combat Balance.
```

---

# PARTE A — Modelo mecânico comum

## 1. Fórmula base de dano físico

Fonte canônica da fórmula geral:

```text
PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
```

Uso com equipamentos:

```text
AttackDamage = (BaseAttack + WeaponDamage + EquipmentFlatDamage)
             * WeaponScaling
             * AttackActionMultiplier
             * (1 + SkillDamageBonus)
             * (1 + BuffDamageBonus)
             * EnemyResistanceMultiplier
             * VulnerabilityMultiplier quando aplicável
```

Definições:

```text
BaseAttack
  vem de Força, Destreza e Level.

WeaponDamage
  vem do tipo da arma + material/tier + upgrade.

EquipmentFlatDamage
  bônus raro de acessórios/relíquias/affixes.

WeaponScaling
  multiplicador de compatibilidade da arma com atributos e material.

AttackActionMultiplier
  light, heavy, charged, special, bow charged etc.

EnemyResistanceMultiplier
  resistência final do inimigo.

VulnerabilityMultiplier
  só aplica se o inimigo declarar vulnerabilidade compatível nos dados.
```

Regra:

```text
Equipamento não deve aplicar bônus de vulnerabilidade se o inimigo não declarar vulnerabilidade correspondente.
```

## 2. Campos mínimos de WeaponDataSO

```text
WeaponType
BaseWeaponDamage
BaseASPD
PrimaryAttribute
SecondaryAttribute
PrimaryScalingWeight
SecondaryScalingWeight
BaseLightStaminaCost
BaseHeavyStaminaCost
BaseChargedStaminaCost
AttackRangeTiles
PostureDamageModifier
CritChanceModifier
CritDamageModifier
WeightClass
MaterialTagsApplied
StatusTagsApplied
AllowedAmmoType
AllowedDamageTypes
DefaultActionSet
ChargedEffectProfileId
```

## 3. Campos mínimos de ArmorDataSO

```text
ArmorType
ArmorFlat
PhysicalResistance
ElementalResistances
MentalResistance
CorruptionResistance
WeightClass
MovementSpeedModifier
StaminaRegenModifier
DashDistanceModifier
DodgeRecoveryModifier
BlockStabilityModifier
DurabilityMax
```

## 4. Campos mínimos de ShieldDataSO

```text
ShieldType
BlockPowerBonus
BlockStabilityBonus
GuardBreakResistance
ArmorFlat
WeightClass
MovementSpeedModifier
StaminaRegenModifier
BlockHoldCostModifier
PerfectBlockWindowModifier
DurabilityMax
```

---

# PARTE B — Armas físicas baseline

## 5. Weapon baseline por tipo

Valores iniciais usando tier Aço como referência média.

| WeaponType | WeaponDamage aço | ASPD base | Atributo primário | Secundário | Scaling sugerido | Light STA | Heavy STA | Charged STA | Range | Peso | Papel |
|---|---:|---:|---|---|---|---:|---:|---:|---:|---|---|
| Sword | 12 | 1.20/s | Força | Destreza | FOR 70% / DES 30% | 25 | 40 | 48 | 1.25 tiles | Médio | baseline/aparo |
| Axe | 15 | 0.90/s | Força | Constituição | FOR 85% / CON 15% | 31 | 50 | 60 | 1.20 tiles | Médio/Pesado | dano/corte/bleed |
| Hammer | 17 | 0.75/s | Força | Constituição | FOR 90% / CON 10% | 36 | 58 | 70 | 1.10 tiles | Pesado | posture/knockback |
| Spear | 11 | 1.05/s | Destreza | Força | DES 60% / FOR 40% | 24 | 42 | 52 | 1.70 tiles | Médio | alcance/pierce |
| Dagger | 7 | 1.75/s | Destreza | Força | DES 80% / FOR 20% | 16 | 28 | 36 | 0.85 tiles | Leve | crit/velocidade |
| Bow | 10 | 0.85/s | Destreza | Inteligência | DES 75% / INT 25% | 22 | 38 charged | 44 full draw | 5.5-7.0 tiles | Leve/Médio | ranged/mark |
| Staff | 6 físico / 10 mágico | 0.90/s | Vontade | Inteligência | VON 55% / INT 45% | 18 físico | 0-STA / MP em magia | MP based | 1.10 físico / 5.0 mágico | Leve | magia/suporte |
| ToolAttack | 6-12 | 0.70-1.00/s | Força | Inteligência | depende da tool | 28-45 | 45-65 | custom | curto | varia | emergência |

Regras:

```text
ASPD é ataques por segundo antes de recovery, peso, status e skills.
Heavy não usa ASPD puro; usa windup/recovery próprio.
Charged é uma variação de alto risco com efeito especial por arma.
Dagger ataca mais vezes, mas cada hit causa menos dano e sofre contra armor.
Hammer causa mais posture e armor pressure, mas custa caro e erra mais facilmente contra alvos rápidos.
Bow depende de arrows/ammo/line of sight e sofre pressão de flankers/swarms.
Staff físico é fraco; seu valor real vem de MP, MagicPower, wands, scrolls e spells.
```

## 6. AttackActionMultiplier

| Ação | Multiplicador de dano | Multiplicador de posture | Observação |
|---|---:|---:|---|
| LightAttack | x1.00 | x1.00 | ação base |
| HeavyAttack | x1.45 | x1.60 | custo alto, recovery maior |
| ChargedAttack curto | x1.65 | x1.80 | exige charge |
| ChargedAttack longo | x1.90 | x2.20 | alto risco |
| BowQuickShot | x0.85 | x0.60 | rápido/seguro |
| BowChargedShot | x1.50 | x1.10 | bom contra flying/caster |
| BowFullDrawShot | x1.75 | x1.25 | alto risco, melhor Mark chance |
| StaffBasicBolt | x1.00 mágico | x0.40 | usa MP ou carga, não Stamina física |
| ToolAttack | x0.70-x0.95 | x0.80-x1.30 | depende de tool |

Regra:

```text
Heavy/Charged só compensam se o jogador usa janela, postura, armor counter ou range adequado.
```

---

# PARTE C — Charged effects por arma

## 7. Regra geral de charged effect

Cada arma deve ter um efeito de charged attack que reforça sua identidade.

Regras:

```text
Charged effect não deve ser garantido sempre.
Chance baixa/moderada por padrão.
Chance pode subir com skill, material, janela crítica, status do alvo ou capstone.
Efeito não deve ignorar resistência/imunidade do inimigo.
Efeito deve ter cooldown interno ou regra anti-spam quando puder quebrar balance.
```

## 8. Tabela de charged effects

| WeaponType | ChargedEffect | Chance base | O que faz | Não funciona bem contra | Observação |
|---|---|---:|---|---|---|
| Sword | ParryWindow / Aparar | 8%-12% | se usado contra ataque melee durante janela curta, reduz dano e abre MinorOpening | projéteis, AoE, boss unblockable | chance sobe com Destreza/Melee/escudo leve |
| Axe | Bleed / Sangramento | 18%-28% | aplica Bleed em alvo vivo vulnerável | undead, constructs, elementais minerais, slimes | usa STATUS_EFFECTS_DIRECTION.md |
| Hammer | Knockback / Empurrão | 12%-20% | empurra alvo pequeno/médio e causa posture extra | bosses, Huge, RootedGuard pesado | pode causar ArmorCracked em construct/armor se vulnerável |
| Spear | Impale / Perfuração | 12%-18% | aumenta Pierce e chance de interromper charge/leap | shield/tank frontal, construct pesado | forte em AfterChargeMiss/WingExposed |
| Dagger | Focused Critical | +20%-35% crit chance condicional | aumenta crit chance no próximo hit charged | armor pesada/construct | melhor contra BackTurned/CriticalWindow |
| Bow | Mark / Marcar | 20%-35% | aplica Marked curto, melhorando crit/ranged follow-up | shielded/untargetable | full draw aumenta chance |
| Staff | Arcane Channel | 100% se cast completo | converte charged em spell/foco, gasta MP | silence/interrupt/sem MP | efeito depende da spell/focus |
| ToolAttack | Utility Break | custom | maior dano contra node/obstáculo/construct frágil | combat elite/boss | não substitui arma |

## 9. Sword charged — Aparar

```text
Sword charged cria uma janela curta de aparo.
Se o inimigo usa melee direto durante essa janela, o jogador reduz parte do dano e pode abrir MinorOpening.
Não é Block completo.
Não substitui Shield/Block.
Não funciona contra AoE, magia de zona, breath, boss unblockable ou projétil salvo skill futura.
```

Baseline:

```text
ParryWindow: 0.18s-0.28s
BaseChance: 8%-12% se timing correto
DamageReductionOnSuccess: 30%-50%
MinorOpeningDuration: 0.35s-0.65s
```

## 10. Axe charged — Bleed

```text
Axe charged tem chance de aplicar Bleed em alvos vivos vulneráveis.
Bleed é definido em STATUS_EFFECTS_DIRECTION.md.
Axe não deve causar Bleed relevante em construct, undead, elemental mineral ou slime sem anatomia compatível.
```

Baseline:

```text
BaseBleedChance: 18%-28%
BleedDuration: usar StatusEffectDataSO
Stack: respeita cap do status
ExtraRule: se acertar CriticalWindow, chance pode subir moderadamente
```

## 11. Hammer charged — Knockback / ArmorCracked

```text
Hammer charged pode empurrar inimigos pequenos/médios e causar posture extra.
Contra inimigos blindados/constructs vulneráveis, pode aplicar ArmorCracked em vez de knockback.
```

Baseline:

```text
BaseKnockbackChance: 12%-20%
KnockbackDistance: 0.5-1.5 tiles conforme size/weight
PostureBonus: +25%-45%
ArmorCrackedChance contra vulnerável: 10%-18%
```

Regras:

```text
Bosses não devem ser empurrados salvo mecânica explícita.
Huge/Large reduzem knockback.
Knockback não deve jogar inimigo através de parede/collider.
```

## 12. Spear charged — Impale

```text
Spear charged é perfuração precisa.
Pode interromper charge/leap de inimigos vulneráveis e causar bônus em asas/pontos expostos sem exigir sistema de mira por parte corporal.
```

Baseline:

```text
ImpaleBonusPierce: +15%-25%
InterruptChanceVsChargeOrLeap: 12%-25%
Wing/Weakpoint window bonus: usa vulnerabilidade do inimigo
```

## 13. Dagger charged — Focused Critical

```text
Dagger charged sacrifica tempo/risco para aumentar chance de crítico.
Não garante crítico fora de CriticalWindow.
Em CriticalWindow, pode melhorar crit damage ou reduzir recovery de follow-up conforme skill.
```

Baseline:

```text
CritChanceBonus: +20%-35% no hit charged
CritDamageBonus: +0%-15% se skill/material permitir
RecoveryPenalty: maior se errar
```

## 14. Bow charged — Mark

```text
Bow charged/full draw pode aplicar Marked.
Marked aumenta a eficiência de follow-up ranged/crit por curta duração.
```

Baseline:

```text
MarkedChance charged: 20%-35%
MarkedDuration: 3s-6s
MarkedEffect: +10%-20% crit chance ranged ou +10%-15% damage ranged, não ambos altos sem skill
```

## 15. Staff charged — Arcane Channel

```text
Staff charged não é apenas pancada física.
Ele canaliza magia do staff/focus/spell equipada.
Gasta MP, pode ser interrompido e depende de INT/VON/Magic skill.
```

Baseline:

```text
CastWindup: 0.6s-1.2s
MPCost: definido pela spell/focus
Interruptible: sim por padrão
Effect: bolt, barrier, heal, purify, elemental burst ou support, conforme action equipada
```

---

# PARTE D — Material/tier modificando armas

## 16. WeaponDamage por tier/material

Aplicar como modificador ao `WeaponDamage aço` ou como tabela específica por item.

| Tier/Material | DamageModifier | WeightModifier | DurabilityModifier | StaminaCostModifier | Identidade |
|---|---:|---:|---:|---:|---|
| Madeira | -45% | -35% | -50% | -15% | leve/frágil |
| Pedra | -25% | +25% | -30% | +15% | pesado/improvisado |
| Cobre | -30% | -5% | -25% | -5% | early |
| Ferro | -12% | +5% | +0% | +0% | confiável |
| Aço | baseline | +8% | +15% | +0% | baseline médio |
| Aço refinado | +12% | +8% | +25% | +3% | dano físico |
| Prata | -5% físico | +0% | -5% | +0% | anti-espiritual |
| Mithril | +5% | -30% | +35% | -12% | leve/eficiente |
| Liga bromeciana | +10% | +12% | +40% | +5% | técnico/construct |
| Cristal arcano | -20% físico / +18% mágico | -15% | -10% | -5% físico / MP cost variável | magia |
| Pedra Negra estabilizada | +18% | +10% | +20% | +8% | late perigoso |
| Meteórico/Mana | +12% físico / +20% mágico | -10% | +30% | -5% | endgame raro |

Regras:

```text
Material pode alterar dano, peso, durabilidade e custo.
Material não deve ser só tier linear.
Mithril não é maior dano bruto; é eficiência/mobilidade.
Prata não é melhor contra tudo; é counter espiritual.
Pedra Negra estabilizada exige gating e risco.
```

## 17. Tags aplicadas por material/equipamento

Este documento só define tags que itens aplicam.

```text
Silver
FireOil
FrostOil
ShockOil
PurifyingOil
PoisonCoating
BleedEdge
BlackstoneEdge
BromecianAlloy
ArcaneCrystal
StabilizedBlackstone
ManaInfused
```

Regra:

```text
O cálculo de bônus contra inimigos usa EQUIPMENT_ENEMY_VULNERABILITY_ADAPTER.md.
A lista de vulnerabilidades por família/inimigo fica em CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md e no EnemyDataSO derivado do roster.
Este documento não deve duplicar matriz de vulnerabilidade por família.
```

---

# PARTE E — Bows, arrows e munição

## 18. Bow baseline

Bow usa arma + arrow.

```text
BowDamage = BowWeaponDamage + ArrowDamage + MaterialModifier + SkillBonus
```

Baseline:

| Bow tier | BowWeaponDamage | ASPD quick | Charged time | Range | Light STA | Charged STA |
|---|---:|---:|---:|---:|---:|---:|
| Madeira simples | 5 | 0.90/s | 0.70s | 5.0 tiles | 18 | 32 |
| Cobre/reforçado | 7 | 0.88/s | 0.75s | 5.5 tiles | 20 | 34 |
| Ferro | 9 | 0.85/s | 0.80s | 6.0 tiles | 21 | 36 |
| Aço | 10 | 0.85/s | 0.85s | 6.5 tiles | 22 | 38 |
| Mithril | 11 | 0.95/s | 0.75s | 7.0 tiles | 19 | 34 |
| Arcano/Mana | 8 físico / +magic channel | 0.80/s | 0.90s | 6.5 tiles | 20 | 32 + MP se mágico |

## 19. Arrow baseline

| ArrowType | ArrowDamage | DamageType | Tags aplicadas | Efeito | Recurso |
|---|---:|---|---|---|---|
| WoodenArrow | +2 | Pierce | WoodenArrow | básica | comum |
| IronArrow | +4 | Pierce | IronArrow | baseline | comum |
| SteelArrow | +6 | Pierce | SteelArrow | mid | comum/mid |
| SilverArrow | +4 | Pierce + Silver | SilverArrow, Silver | anti-undead/shadow se vulnerável | cara |
| FireArrow | +3 | Pierce + Fire | FireArrow, Fire | chance Burn baixa | craft/óleo |
| FrostArrow | +3 | Pierce + Ice | FrostArrow, Ice | chance Chill baixa | craft/óleo |
| ShockArrow | +3 | Pierce + Lightning | ShockArrow, Lightning | overload se vulnerável | craft/óleo |
| BarbedArrow | +2 | Pierce + Bleed | BarbedArrow, Bleed | chance Bleed | craft |
| ArcaneArrow | +2 físico + magic | Arcane | ArcaneArrow, Arcane | usa carga/MP | rara |
| PurifyingArrow | +2 físico + radiant | Light/Radiant | PurifyingArrow, Light/Radiant | anti-corruption se vulnerável | rara |

Regras:

```text
Arrow define dano secundário e tags aplicadas.
Bow define cadência, range e parte do dano base.
Arrow elemental só recebe bônus se inimigo tiver vulnerabilidade compatível.
Arrows especiais devem ser consumíveis/cargas, não infinitas no early game.
```

---

# PARTE F — Magic items: wands, scrolls, tomes e focuses

## 20. Categorias mágicas

```text
Staff
  arma/foco principal, usa MP e skills.

Wand
  item mágico de cargas, menor compromisso que staff.

Scroll / Pergaminho
  consumível de uso único ou poucas cargas.

Tome / Grimório
  unlock/modificador de spell, mais persistente.

Focus / Relic
  offhand/acessório que altera MP, cast, elemento ou cura.

Rune / Sigil
  componente ou socket futuro.
```

## 21. Wand baseline

Wand é equipamento/carga, não substitui build de magia completa.

| WandType | MagicDamage | MP Cost | Charges | Scaling | Tags aplicadas | Uso |
|---|---:|---:|---:|---|---|---|
| SimpleWand | 10 | 4 | 20 | INT 50% / VON 50% | Arcane | projétil básico |
| FireWand | 12 Fire | 5 | 15 | INT 60% / VON 40% | Fire, WandFire | burn leve |
| FrostWand | 10 Ice | 5 | 15 | INT 50% / VON 50% | Ice, WandFrost | chill leve |
| ShockWand | 11 Lightning | 6 | 12 | INT 65% / VON 35% | Lightning, WandShock | overload se vulnerável |
| PurifyingWand | 9 Light/Radiant | 7 | 10 | VON 70% / INT 30% | Light/Radiant, WandPurifying | anti-undead/corruption se vulnerável |
| BlackstoneWand | 15 Corruption | custom | baixa | INT/VON custom | Corruption, WandBlackstone | late, risco |

Regras:

```text
Wands usam carga e/ou MP.
Wands permitem magia utilitária sem build completa, mas não devem superar staff + skill tree.
Wands especiais devem ter custo, carga, cooldown ou risco.
```

## 22. Scroll / Pergaminho baseline

Pergaminho é consumível.

| ScrollType | Efeito | Tags aplicadas | Scaling | Custo | Risco/limite |
|---|---|---|---|---|---|
| ScrollFireburst | AoE Fire pequeno | Fire, AreaOfEffect | fixo + INT leve | consome scroll | friendly fire não no início |
| ScrollBarrier | barrier curta | Barrier | VON leve | consome scroll | cooldown global |
| ScrollPurify | remove Corruption/Poison leve | Purify, Light/Radiant | VON leve | consome scroll | raro |
| ScrollBlink | reposicionamento curto | Blink | fixo | consome scroll | não atravessa boss wall |
| ScrollReveal | revela trap/treasure | Reveal | INT leve | consome scroll | útil em cave |
| ScrollRecall | retorno/checkpoint futuro | Recall | fixo | raro | não usar em boss |
| ScrollRespecMinor futuro | ajuste pequeno | Respec | Fonte/Anya | raro | se permitido |

Regras:

```text
Scrolls são fortes porque não exigem build, então devem ser consumíveis e raros conforme efeito.
Scroll não deve invalidar Magic skill tree.
Scroll não deve trivializar boss gate.
```

## 23. Tome / Grimório baseline

Tomes são persistentes e mais raros.

```text
Tome não é consumível comum.
Pode desbloquear receita/spell/modificador.
Pode exigir INT/VON/Skill Magic.
Pode ser usado como gating para spell avançada.
```

## 24. Focus / offhand mágico baseline

| FocusType | Efeito | Tradeoff |
|---|---|---|
| ArcaneFocus | +MagicPower, -defesa offhand | sem shield |
| HealingFocus | +Healing/Barrier, baixo dano | suporte |
| ElementalFocus | +elemento específico | fraco fora do elemento |
| WardFocus | barrier/block mágico | MP cost |
| BlackstoneFocus | dano alto/risco | corrupção/custo |

---

# PARTE G — Armaduras baseline

## 25. Armor baseline por tipo

Valores iniciais por tier Aço/Equivalente médio.

| ArmorType | ArmorFlat | PhysicalRes | MoveMod | StaminaRegenMod | DashMod | DodgeRecoveryMod | Peso | Uso |
|---|---:|---:|---:|---:|---:|---:|---|---|
| LightArmor | 6 | 3% | 0% a -2% | 0% | 0% | 0% | Leve | mobilidade |
| MediumArmor | 12 | 6% | -4% | -5% | -5% | +5% recovery | Médio | equilíbrio |
| HeavyArmor | 20 | 10% | -10% | -12% | -12% | +12% recovery | Pesado | tank/block |
| ArcaneRobe | 4 | 1% | 0% | 0% | 0% | 0% | Leve | MP/magia |
| ReinforcedRobe | 8 | 3% | -3% | -3% | -3% | +3% recovery | Médio leve | battle mage |

Regras:

```text
ArmorFlat reduz dano e também reduz BlockImpact indiretamente porque BlockImpact usa dano pós-armadura.
Armadura pesada deve prejudicar Dash longo e Dodge comfort.
Armadura leve não deve dar muita mitigação.
Robes protegem pouco fisicamente, mas podem dar MP, MagicPower, resistance espiritual ou cast.
```

## 26. Armor por material

| Material | ArmorFlatMod | WeightMod | Resistência típica | Observação |
|---|---:|---:|---|---|
| Couro | baixo | leve | bleed/physical leve | early/light |
| Couro reforçado | médio-baixo | leve/médio | physical leve | mid light |
| Ferro | médio | médio | physical | baseline |
| Aço | alto | pesado | physical | mid/heavy |
| Aço refinado | alto+ | pesado | physical/posture | forte/caro |
| Prata | médio | médio | shadow/corruption leve | não tank físico principal |
| Mithril | médio | leve | physical moderada | mobilidade |
| Liga bromeciana | alto | médio/pesado | lightning/physical | técnico |
| Cristal arcano | baixo | leve | arcane/mental | robe/focus |
| Pedra Negra estabilizada | alto/custom | médio/pesado | corruption custom | risco/endgame |

---

# PARTE H — Escudos baseline

## 27. Shield baseline

| ShieldType | BlockPowerBonus | BlockStabilityBonus | ArmorFlat | MoveMod | RegenMod | BlockHoldMod | Peso | Identidade |
|---|---:|---:|---:|---:|---:|---:|---|---|
| Buckler | +12% | +4% | +2 | 0% | 0% | -5% | Leve | perfect block |
| RoundShield | +22% | +10% | +5 | -3% | -3% | 0% | Médio | equilíbrio |
| TowerShield | +35% | +20% | +10 | -10% | -10% | +8% | Pesado | tank |
| ArcaneWard | +18% físico / +30% mágico | +8% | +2 | -2% | 0% | MP upkeep possível | Leve | mágico |

Regra:

```text
Escudo melhora Block, mas não remove custo de Stamina.
TowerShield deve ser forte contra impacto frontal, mas ruim para mobilidade/Dash build.
Buckler deve favorecer timing/perfect block, não tankar boss.
```

---

# PARTE I — Consistência com vulnerabilidades de inimigos

## 28. Fonte de vulnerabilidades

Este documento não é a fonte da matriz de vulnerabilidades por família.

Fontes corretas:

```text
CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
  vulnerabilidades por família, janelas, multiplicadores e limites.

CAVE_MONSTER_ROSTER_DIRECTION.md
  inimigos concretos e futuras tags por inimigo.

EQUIPMENT_ENEMY_VULNERABILITY_ADAPTER.md
  regra de matching entre tags de equipamento e vulnerabilidades do inimigo.
```

Regra central:

```text
Equipamento aplica DamageType, MaterialTag, StatusTag ou AttackType.
O inimigo precisa declarar vulnerabilidade/resistência correspondente.
Se não declarar, o item usa dano normal ou resistência normal.
```

## 29. Efeitos temporários aplicados por equipamento

Alguns equipamentos podem aplicar estado temporário, mas isso precisa ser declarado em status/window data.

| Estado temporário | Aplicado por | Efeito | Fonte canônica |
|---|---|---|---|
| ArmorCracked | Hammer/Heavy/Charged | +Blunt/Posture recebido se inimigo permitir | Combat/Enemy specs futuras |
| ShockOverloaded | ShockOil/Lightning/Wand | janela em constructs se vulnerável | Cave vulnerability + adapter |
| Purified | PurifyingOil/Anya/Light | reduz corrupção/sombra | Status/Anya specs futuras |
| Burning | FireOil/FireArrow/Wand | Burn DoT/status | STATUS_EFFECTS_DIRECTION.md |
| Chilled | FrostOil/FrostArrow | Chill/slow leve | STATUS_EFFECTS_DIRECTION.md |
| Bleed | Axe/Dagger/BarbedArrow/BleedEdge | dano físico por tempo | STATUS_EFFECTS_DIRECTION.md |
| Marked | Bow skill/companion/pet | melhora crit/ranged follow-up | Combat/Skill specs futuras |
| ExposedCore | boss/construct mechanic | CriticalWindow/CoreExposed | Cave Combat Balance |

Regra:

```text
Estados temporários não podem sobrescrever imunidade/resistência forte sem regra explícita.
```

---

# PARTE J — Tooltips mecânicos

## 30. Tooltip mínimo de arma

Mostrar:

```text
WeaponDamage
ASPD
Light Stamina Cost
Heavy/Charged Stamina Cost
ChargedEffect
DamageType
MaterialTag
Atributo primário/secundário
Range
Posture
Crit modifier
Peso
Durabilidade
Tags aplicadas
```

Exemplo:

```text
Espada de Aço
WeaponDamage: 12
ASPD: 1.20/s
Light: 25 STA | Heavy: 40 STA | Charged: 48 STA
Scaling: FOR 70% / DES 30%
DamageType: Slash
ChargedEffect: Aparar, chance baixa com timing
Material: Steel
Peso: Médio
```

## 31. Tooltip mínimo de bow/arrow

Bow:

```text
BowDamage
ASPD quick
Charged time
Range
Stamina Cost
Scaling
ChargedEffect: Mark
Allowed Arrow Types
```

Arrow:

```text
ArrowDamage
DamageType
Material/StatusTag
Chance de status
Consumo/carga
```

## 32. Tooltip mínimo de wand/scroll

Wand:

```text
MagicDamage
MP Cost
Charges
DamageType
Scaling INT/VON
Cooldown
Tags aplicadas
```

Scroll:

```text
Efeito
Uso único/cargas
Scaling leve ou fixo
Cooldown global
Restrições
```

---

# PARTE K — Decisões fechadas

```text
WeaponDamage, ASPD, scaling de atributo, custo de Stamina, range e peso devem existir em WeaponDataSO.
Cada arma deve ter ChargedEffectProfileId.
Sword charged pode aparar com chance baixa/timing e abrir MinorOpening.
Axe charged pode aplicar Bleed em alvos vivos vulneráveis.
Hammer charged pode causar Knockback em alvos pequenos/médios e ArmorCracked em alvos vulneráveis.
Spear charged pode causar Impale/interrupção contra charge/leap vulnerável.
Dagger charged aumenta chance de crítico condicional.
Bow charged pode aplicar Marked.
Staff charged canaliza magia/foco e gasta MP.
Bleed é definido apenas em STATUS_EFFECTS_DIRECTION.md.
ArmorFlat, resistência, peso, StaminaRegenMod, DashMod e DodgeRecoveryMod devem existir em ArmorDataSO.
Escudos devem ter BlockPowerBonus, BlockStabilityBonus, peso e modificadores de Block/Stamina.
Bows usam BowData + ArrowData; arrows definem dano secundário/status/material.
Wands usam carga e/ou MP; não substituem staff + skill tree.
Pergaminhos são consumíveis fortes, raros conforme efeito.
Tomes/Grimórios são persistentes e podem desbloquear spells/modificadores.
Equipamento só explora vulnerabilidade se o inimigo declarar tag compatível.
Vulnerabilidades por família/inimigo não ficam neste documento; ficam em Cave Combat Balance, roster/enemy data e adapter.
```

---

# PARTE L — Pendências para specs futuras

```text
Definir enums finais: WeaponType, ArmorType, ShieldType, DamageType, MaterialTag, StatusTag, VulnerabilityTag.
Adicionar ChargedEffectProfileId ao contrato de WeaponDataSO.
Criar ChargedEffectDataSO.
Adicionar MaterialVulnerability ao contrato de EnemyDataSO.
Criar WeaponDataSO com os campos mecânicos deste documento.
Criar ArmorDataSO/ShieldDataSO com os campos mecânicos deste documento.
Criar BowDataSO e ArrowDataSO.
Criar WandDataSO, ScrollDataSO, TomeDataSO e FocusDataSO.
Criar EquipmentTooltip UI com stats mecânicos e ChargedEffect.
Criar EnemyFamilyEquipmentCounter table em dados, derivada do roster/Cave Combat Balance.
Validar WeaponDamage/ASPD/StaminaCost contra TTK real dos monstros do roster.
Validar armor/Block/Stamina contra active combat budget da caverna.
Validar Bleed contra Beast/Humanoid/Construct/Undead.
```
