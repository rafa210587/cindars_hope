# Cindar's Hope — Equipment Mechanical Baselines Direction

> **Status:** documento canônico complementar de mecânicas numéricas iniciais de equipamentos  
> **Local:** `docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md`  
> **Complementa:** `docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md`  
> - `docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`  
> - `docs/design/gameplay/enemies/ENEMY_BEHAVIORS_DIRECTION.md`  
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
qual material causa qual vulnerabilidade?
como bow/arrows funcionam?
como wands/scrolls entram no sistema mágico?
```

Regra:

```text
Os valores abaixo são baseline inicial de teste.
Specs futuras podem ajustar com telemetria, mas não devem inventar outro modelo sem registrar decisão.
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
             * MaterialEnemyMultiplier
             * (1 + SkillDamageBonus)
             * (1 + BuffDamageBonus)
             * EnemyResistanceMultiplier
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

MaterialEnemyMultiplier
  bônus condicional quando material/damage type explora vulnerabilidade do inimigo.

EnemyResistanceMultiplier
  resistência/vulnerabilidade final do inimigo.
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
MaterialSlots
AllowedAmmoType
AllowedDamageTypes
DefaultActionSet
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

| WeaponType | WeaponDamage aço | ASPD base | Atributo primário | Secundário | Scaling sugerido | Light STA | Heavy STA | Range | Peso | Papel |
|---|---:|---:|---|---|---|---:|---:|---:|---|---|
| Sword | 12 | 1.20/s | Força | Destreza | FOR 70% / DES 30% | 25 | 40 | 1.25 tiles | Médio | baseline |
| Axe | 15 | 0.90/s | Força | Constituição | FOR 85% / CON 15% | 31 | 50 | 1.20 tiles | Médio/Pesado | dano/corte |
| Hammer | 17 | 0.75/s | Força | Constituição | FOR 90% / CON 10% | 36 | 58 | 1.10 tiles | Pesado | posture/armor |
| Spear | 11 | 1.05/s | Destreza | Força | DES 60% / FOR 40% | 24 | 42 | 1.70 tiles | Médio | alcance/pierce |
| Dagger | 7 | 1.75/s | Destreza | Força | DES 80% / FOR 20% | 16 | 28 | 0.85 tiles | Leve | crit/velocidade |
| Bow | 10 | 0.85/s | Destreza | Inteligência | DES 75% / INT 25% | 22 | 38 charged | 5.5-7.0 tiles | Leve/Médio | ranged |
| Staff | 6 físico / 10 mágico | 0.90/s | Vontade | Inteligência | VON 55% / INT 45% | 18 físico | 0-STA / MP em magia | 1.10 físico / 5.0 mágico | Leve | magia/suporte |
| ToolAttack | 6-12 | 0.70-1.00/s | Força | Inteligência | depende da tool | 28-45 | 45-65 | curto | varia | emergência |

Regras:

```text
ASPD é ataques por segundo antes de recovery, peso, status e skills.
Heavy não usa ASPD puro; usa windup/recovery próprio.
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
| StaffBasicBolt | x1.00 mágico | x0.40 | usa MP ou carga, não Stamina física |
| ToolAttack | x0.70-x0.95 | x0.80-x1.30 | depende de tool |

Regra:

```text
Heavy/Charged só compensam se o jogador usa janela, postura, armor counter ou range adequado.
```

---

# PARTE C — Material/tier modificando armas

## 7. WeaponDamage por tier/material

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

## 8. MaterialEnemyMultiplier

Só aplica se o inimigo declarar vulnerabilidade por família, roster, tag ou spec.

| Material/Efeito | Vulnerabilidade exigida no inimigo | Multiplicador alvo | Observação |
|---|---|---:|---|
| Prata | MaterialVulnerability: Silver / Undead / Shadow / Curse / CorruptionLight | x1.25-x1.45 | não universal |
| Aço refinado | ArmorVulnerability / PhysicalMedium | x1.05-x1.15 | bônus pequeno |
| Mithril | FastWindow / MobilityCounter | não dano direto | melhora execução/custo |
| Liga bromeciana | Construct / Machine / Protocol | x1.15-x1.35 | melhor em technical gear |
| Cristal arcano | ArcaneVulnerability | x1.15-x1.35 | depende de MP/spell |
| Fire Oil | FireVulnerability / BurnVulnerability | x1.20-x1.40 | consumível/cargas |
| Frost Oil | IceVulnerability / ChillVulnerability | x1.15-x1.35 | bom contra Kaand/fire/beasts específicos |
| Shock Oil | LightningVulnerability / Overload | x1.20-x1.45 | constructs/duergar tech |
| Purifying Oil | Light/Radiant / Corruption / Undead | x1.25-x1.50 | raro/caro |
| Poison Coating | PoisonVulnerability | x1.10-x1.30 | inútil contra undead/construct |
| Bleed Edge | BleedVulnerability | x1.10-x1.35 | inútil contra constructs/undead sem carne |
| Blackstone Edge | BlackstoneVulnerability ou CorruptionInteraction | custom | late/endgame, risco |

Regra:

```text
Item não cria vulnerabilidade do nada por padrão.
Item aplica DamageType/Material/Status.
O inimigo precisa ter vulnerabilidade/resistência compatível nos dados.
```

---

# PARTE D — Bows, arrows e munição

## 9. Bow baseline

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

## 10. Arrow baseline

| ArrowType | ArrowDamage | DamageType | Efeito | Recurso |
|---|---:|---|---|---|
| WoodenArrow | +2 | Pierce | básica | comum |
| IronArrow | +4 | Pierce | baseline | comum |
| SteelArrow | +6 | Pierce | mid | comum/mid |
| SilverArrow | +4 | Pierce + Silver | anti-undead/shadow | cara |
| FireArrow | +3 | Pierce + Fire | chance Burn baixa | craft/óleo |
| FrostArrow | +3 | Pierce + Ice | chance Chill baixa | craft/óleo |
| ShockArrow | +3 | Pierce + Lightning | overload construct | craft/óleo |
| BarbedArrow | +2 | Pierce + Bleed | chance Bleed | craft |
| ArcaneArrow | +2 físico + magic | Arcane | usa carga/MP | rara |
| PurifyingArrow | +2 físico + radiant | Light/Radiant | anti-corruption | rara |

Regras:

```text
Arrow define dano secundário e efeito.
Bow define cadência, range e parte do dano base.
Arrow elemental só recebe bônus se inimigo tiver vulnerabilidade compatível.
Arrows especiais devem ser consumíveis/cargas, não infinitas no early game.
```

---

# PARTE E — Magic items: wands, scrolls, tomes e focuses

## 11. Categorias mágicas

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

## 12. Wand baseline

Wand é equipamento/carga, não substitui build de magia completa.

| WandType | MagicDamage | MP Cost | Charges | Scaling | Uso |
|---|---:|---:|---:|---|---|
| SimpleWand | 10 | 4 | 20 | INT 50% / VON 50% | projétil básico |
| FireWand | 12 Fire | 5 | 15 | INT 60% / VON 40% | burn leve |
| FrostWand | 10 Ice | 5 | 15 | INT 50% / VON 50% | chill leve |
| ShockWand | 11 Lightning | 6 | 12 | INT 65% / VON 35% | overload construct |
| PurifyingWand | 9 Light/Radiant | 7 | 10 | VON 70% / INT 30% | anti-undead/corruption |
| BlackstoneWand | 15 Corruption | custom | baixa | INT/VON custom | late, risco |

Regras:

```text
Wands usam carga e/ou MP.
Wands permitem magia utilitária sem build completa, mas não devem superar staff + skill tree.
Wands especiais devem ter custo, carga, cooldown ou risco.
```

## 13. Scroll / Pergaminho baseline

Pergaminho é consumível.

| ScrollType | Efeito | Scaling | Custo | Risco/limite |
|---|---|---|---|---|
| ScrollFireburst | AoE Fire pequeno | fixo + INT leve | consome scroll | friendly fire não no início |
| ScrollBarrier | barrier curta | VON leve | consome scroll | cooldown global |
| ScrollPurify | remove Corruption/Poison leve | VON leve | consome scroll | raro |
| ScrollBlink | reposicionamento curto | fixo | consome scroll | não atravessa boss wall |
| ScrollReveal | revela trap/treasure | INT leve | consome scroll | útil em cave |
| ScrollRecall | retorno/checkpoint futuro | fixo | raro | não usar em boss |
| ScrollRespecMinor futuro | ajuste pequeno | Fonte/Anya | raro | se permitido |

Regras:

```text
Scrolls são fortes porque não exigem build, então devem ser consumíveis e raros conforme efeito.
Scroll não deve invalidar Magic skill tree.
Scroll não deve trivializar boss gate.
```

## 14. Tome / Grimório baseline

Tomes são persistentes e mais raros.

```text
Tome não é consumível comum.
Pode desbloquear receita/spell/modificador.
Pode exigir INT/VON/Skill Magic.
Pode ser usado como gating para spell avançada.
```

## 15. Focus / offhand mágico baseline

| FocusType | Efeito | Tradeoff |
|---|---|---|
| ArcaneFocus | +MagicPower, -defesa offhand | sem shield |
| HealingFocus | +Healing/Barrier, baixo dano | suporte |
| ElementalFocus | +elemento específico | fraco fora do elemento |
| WardFocus | barrier/block mágico | MP cost |
| BlackstoneFocus | dano alto/risco | corrupção/custo |

---

# PARTE F — Armaduras baseline

## 16. Armor baseline por tipo

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

## 17. Armor por material

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

# PARTE G — Escudos baseline

## 18. Shield baseline

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

# PARTE H — Vulnerabilidades e consistência com monstros

## 19. Vulnerabilidade vem do inimigo, item só aplica tag

Regra central:

```text
Equipamento aplica DamageType, MaterialTag, StatusTag ou AttackType.
O inimigo precisa declarar vulnerabilidade/resistência correspondente.
Se não declarar, o item usa dano normal ou resistência normal.
```

Categorias que devem existir nos dados de inimigo:

```text
ElementVulnerability
StatusVulnerability
AttackTypeVulnerability
WeaponVulnerability
MaterialVulnerability
BehavioralVulnerabilityWindow
ResistanceTags
ImmunityTags raro
```

## 20. MaterialVulnerability mínima por família

Esta matriz deve ser refletida em specs futuras de roster/enemy data.

| Família | MaterialVulnerability sugerida | Resistências relevantes |
|---|---|---|
| Fungal/Raiz-Negra | FireOil, PurifyingOil, AxeEdge opcional | Poison, Root/Nature |
| Beast/Predator | Barbed/Bleed, SpearTip, BowProjectile | mental forte limitado |
| Goblin/Kobold | Steel, Bleed, FearTools | poucas resistências |
| Orcs de Kaand | FrostOil, Water, Hammer/Spear | Burn, Fear reduzido |
| Duergar/Gelo | FireOil, ShockOil, Hammer/Pickaxe | Ice, Chill |
| Undead/Sombras | Silver, PurifyingOil, Light/Radiant, BluntBone | Poison, Bleed, Fear |
| Elementals | elemento oposto, Hammer/Pickaxe em stone/crystal | elemento próprio |
| Constructs/Bromecianos | ShockOil, BromecianOverride, Hammer/Pickaxe | Poison, Bleed, Fear |
| Aberrants/Observadores | Light/Radiant, ArcaneStable, BowEyeShot | Fear parcial, ConfusionLite |
| Dracônicos/Pedra Negra | Light/Radiant, SpearPierce, BowWeakpoint, ArcaneStable | Blackstone, Fear, Burn alguns |

Regra:

```text
Se o item tiver tag Silver mas o inimigo não tiver MaterialVulnerability Silver, não aplicar bônus anti-espiritual.
Se o inimigo tiver Resistance Poison, Poison Coating deve falhar ou ser muito reduzido.
```

## 21. Efeitos que podem causar estado de vulnerabilidade temporária

Alguns equipamentos podem aplicar estado temporário, mas isso precisa ser declarado.

| Estado temporário | Aplicado por | Efeito | Restrições |
|---|---|---|---|
| ArmorCracked | Hammer/Heavy/Charged | +Blunt/Posture recebido | não em todos os bosses |
| ShockOverloaded | ShockOil/Lightning/Wand | janela em constructs | exige LightningVulnerability/Construct |
| Purified | PurifyingOil/Anya/Light | reduz corrupção/sombra | raro/caro |
| Burning | FireOil/FireArrow/Wand | DoT + abre FireVulnerability se inimigo permitir | não contra FireResist |
| Chilled | FrostOil/FrostArrow | slow leve + janela curta | não contra IceResist |
| Marked | Bow skill/companion/pet | melhora crit/ranged | duração curta |
| ExposedCore | boss/construct mechanic | CriticalWindow/CoreExposed | só por mecânica |

Regra:

```text
Estados temporários não podem sobrescrever imunidade/resistência forte sem regra explícita.
```

---

# PARTE I — Tooltips mecânicos

## 22. Tooltip mínimo de arma

Mostrar:

```text
WeaponDamage
ASPD
Light Stamina Cost
Heavy/Charged Stamina Cost
DamageType
MaterialTag
Atributo primário/secundário
Range
Posture
Crit modifier
Peso
Durabilidade
Vulnerabilidades que pode explorar
```

Exemplo:

```text
Espada de Aço
WeaponDamage: 12
ASPD: 1.20/s
Light: 25 STA | Heavy: 40 STA
Scaling: FOR 70% / DES 30%
DamageType: Slash
Material: Steel
Peso: Médio
Boa contra: humanoides, abertura média
Ruim contra: armor pesada/construct sem skill
```

## 23. Tooltip mínimo de bow/arrow

Bow:

```text
BowDamage
ASPD quick
Charged time
Range
Stamina Cost
Scaling
Allowed Arrow Types
```

Arrow:

```text
ArrowDamage
DamageType
Material/StatusTag
Chance de status
Consumo/carga
Famílias vulneráveis conhecidas se descobertas no bestiário
```

## 24. Tooltip mínimo de wand/scroll

Wand:

```text
MagicDamage
MP Cost
Charges
DamageType
Scaling INT/VON
Cooldown
Famílias vulneráveis conhecidas se descobertas
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

# PARTE J — Decisões fechadas

```text
WeaponDamage, ASPD, scaling de atributo, custo de Stamina, range e peso devem existir em WeaponDataSO.
ArmorFlat, resistência, peso, StaminaRegenMod, DashMod e DodgeRecoveryMod devem existir em ArmorDataSO.
Escudos devem ter BlockPowerBonus, BlockStabilityBonus, peso e modificadores de Block/Stamina.
Bows usam BowData + ArrowData; arrows definem dano secundário/status/material.
Wands usam carga e/ou MP; não substituem staff + skill tree.
Pergaminhos são consumíveis fortes, raros conforme efeito.
Tomes/Grimórios são persistentes e podem desbloquear spells/modificadores.
Equipamento só explora vulnerabilidade se o inimigo declarar tag compatível.
MaterialVulnerability deve ser adicionada aos dados futuros de inimigos junto de Element/Status/AttackType/Weapon/Window.
```

---

# PARTE K — Pendências para specs futuras

```text
Definir enums finais: WeaponType, ArmorType, ShieldType, DamageType, MaterialTag, StatusTag, VulnerabilityTag.
Adicionar MaterialVulnerability ao contrato de EnemyDataSO.
Criar WeaponDataSO com os campos mecânicos deste documento.
Criar ArmorDataSO/ShieldDataSO com os campos mecânicos deste documento.
Criar BowDataSO e ArrowDataSO.
Criar WandDataSO, ScrollDataSO, TomeDataSO e FocusDataSO.
Criar EquipmentTooltip UI com stats mecânicos.
Criar EnemyFamilyEquipmentCounter table em dados.
Validar WeaponDamage/ASPD/StaminaCost contra TTK real dos monstros do roster.
Validar armor/Block/Stamina contra active combat budget da caverna.
```
