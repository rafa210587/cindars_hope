# Cindar's Hope — Player Derived Attributes Direction

> **Status:** documento canônico de direção dos atributos derivados do personagem  
> **Local:** `docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md`  
> **Complementa:**  
> - `docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md`  
> - `docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md`  
> **Não é spec implementável.** Este documento define quais atributos derivados existem, o que representam, como se conectam aos atributos centrais, skills, equipamentos, HUD, combate, produção e save/load.

---

## 0. Decisão canônica: Breath/Fôlego removido

`Breath` / `Fôlego` foi removido como atributo, recurso, barra e custo.

Não deve existir como:

```text
atributo central
atributo derivado
barra de HUD
custo de Dash
custo de Dodge
custo de Block
stat de monstro
campo BR em tabela nova
recurso salvo no save/load
```

Motivo:

```text
Breath estava sobreposto com Stamina, Constituição e Cansaço.
Ele criava uma segunda stamina sem função clara.
O sistema fica mais legível removendo Breath e redistribuindo suas funções.
```

Substituições:

```text
Stamina
  recurso físico imediato gasto em ações.

Cansaço
  desgaste acumulado de longo prazo.

Constituição
  HP, Stamina, estabilidade física, resistência a dano/status físico, tolerância a cansaço.

Destreza
  dodge, dash, movement, attack speed, reação, crítico condicional.

Vontade
  MP, MP Regen lenta, resistência mental/espiritual, corrupção/Nyx/Void.

Survival/Sobrevivente
  redução de fome/cansaço, eficiência em runs, resistência ambiental, melhoria de Dash/Dodge.

Melee/Guerreiro
  Block, estabilidade, postura, stagger, defesa ativa.

Traits de monstros
  movimento, perseguição, recuperação, pressão e comportamento.
```

---

# PARTE A — Modelo geral de cálculo

## 1. Fórmula em camadas

Todo atributo derivado deve seguir uma estrutura parecida:

```text
DerivedStat = BaseValue
            + AttributeContribution
            + LevelContribution
            + EquipmentFlatBonus
            + SkillFlatBonus
            + BuffFlatBonus

FinalStat = DerivedStat
          * (1 + SkillPercentBonus)
          * (1 + EquipmentPercentBonus)
          * (1 + BuffPercentBonus)
          * (1 - StatusPenalty)
          * ContextMultiplier
```

Regras:

```text
Flat bonuses entram antes dos multiplicadores.
Percent bonuses devem ter cap ou softcap quando puderem quebrar o jogo.
StatusPenalty deve ser claro e legível.
ContextMultiplier só deve existir quando houver condição explícita, como critical window, alvo marcado ou vulnerabilidade elemental.
```

## 2. Caps e softcaps

Tipos de limite:

```text
Hard Cap
  limite absoluto.

Soft Cap
  depois de certo valor, cada ponto adicional rende menos.

Context Cap
  limite diferente por contexto.
```

Stats que precisam de caps/softcaps:

```text
Crit Chance
Crit Damage
Attack Speed
Cast Speed
Movement Speed
Dash Cooldown Reduction
Dodge Invulnerability Window
HP Regen
MP Regen
Block Power
Block Stability
Resource Yield Bonus
Gold Bonus
Vendor Price Modifier
```

Fórmula de softcap sugerida:

```text
EffectiveValue = SoftCap + (RawValue - SoftCap) * DiminishingFactor
```

---

# PARTE B — Atributos derivados canônicos

## 3. Recursos principais

```text
HP Max
MP Max
Stamina Max
HP Regen
MP Regen
Stamina Regen
```

Não existe:

```text
Breath Max
Breath Recovery
Fôlego Max
BR
```

## 4. Ofensivos físicos

```text
Base Attack
Attack Damage
Melee Damage
Ranged Damage
Tool Attack Damage
Attack Speed
Charge Speed
Crit Chance
Crit Damage
Stagger Power
Posture Damage
Armor Penetration
Knockback Power
```

## 5. Ofensivos mágicos

```text
Magic Power
Elemental Power
Fire Power
Ice Power
Lightning Power
Water/Nature Power
Arcane Power
Spiritual Power
Corruption Power futuro/controlado
Healing Power
Shield/Barrier Power
Cast Speed
MP Cost Reduction
Magic Crit Chance
Magic Crit Damage
Status Application Power
```

## 6. Defensivos

```text
Defense
Armor
Physical Resistance
Elemental Resistance
Fire Resistance
Ice Resistance
Lightning Resistance
Water/Nature Resistance
Arcane Resistance
Shadow/Nyx Resistance
Blackstone/Corruption Resistance
Status Resistance
Poison Resistance
Bleed Resistance
Burn Resistance
Chill Resistance
Fear Resistance
Confusion Resistance
Stun Resistance
Posture Resistance
Knockback Resistance
Block Power
Block Stability
Block Recovery
```

## 7. Movimento e controle

```text
Movement Speed
Dash Distance
Dash Cooldown Reduction
Dash Cost Reduction
Dodge Invulnerability Window
Dodge Recovery
Dodge Cost Reduction
Collision Recovery
Turn/Acceleration Feel
```

## 8. Produção, coleta e economia

```text
Resource Yield Bonus
Gathering Efficiency
Tool Stamina Cost Reduction
Tool Action Speed
Mining Efficiency
Woodcutting Efficiency
Farming Efficiency
Watering Efficiency
Fishing Efficiency
Crafting Efficiency
Construction Cost Reduction
Craft Quality Bonus
Cooking Quality Bonus
Potion/Consumable Potency
Loot Bonus
Gold Bonus
Treasure Quality Bonus
```

## 9. Condição, social e companions

```text
Hunger Resistance
Fatigue Resistance
Sleep Recovery Bonus
Environmental Endurance
Social Influence
Vendor Price Modifier
Relationship Gain Modifier
Companion Command futuro
Companion Bond Effect futuro
Pet Bond Effect futuro
Pet Combat Support futuro
```

---

# PARTE C — Recursos principais

## 10. HP Max

Representa vida máxima.

Influenciado por:

```text
Constituição principalmente
Level em menor grau
equipamentos
buffs de comida
skills defensivas
arquétipos
Fonte de Anya/eventos especiais
```

Fórmula direcional:

```text
HPMax = BaseHP
      + (Level * HPPerLevel)
      + (Constituição * HPPerCon)
      + EquipmentHP
      + SkillHP
      + BuffHP
```

Regras:

```text
HP Max não representa regeneração.
Constituição aumenta vida e tolerância, não cura automática.
```

HUD:

```text
Barra principal sempre visível.
```

## 11. MP Max

Representa reserva mágica.

Influenciado por:

```text
Vontade principalmente
Inteligência secundária
Level em menor grau
equipamentos mágicos
Magic/Arcano
Fruto de Mana
Fonte de Anya
```

Fórmula direcional:

```text
MPMax = BaseMP
      + (Level * MPPerLevel)
      + (Vontade * MPPerWill)
      + (Inteligência * MPPerInt)
      + EquipmentMP
      + SkillMP
      + BuffMP
```

HUD:

```text
Aparece quando magia/item mágico for desbloqueado ou equipado.
```

## 12. Stamina Max

Representa energia física imediata para ações.

Influenciado por:

```text
Constituição principalmente
Força em menor grau para esforço físico pesado
Level em menor grau
Survival/Sobrevivente
Crafting/Produção para rotinas produtivas
equipamentos
comida/buffs
```

Fórmula direcional:

```text
StaminaMax = BaseStamina
           + (Level * StaminaPerLevel)
           + (Constituição * StaminaPerCon)
           + (Força * StaminaPerStrSmall)
           + EquipmentStamina
           + SkillStamina
           + BuffStamina
```

Gasta em:

```text
ataques físicos
ataques carregados
Dash
Dodge
Block
corrida
ferramentas
mineração
corte de madeira
plantio/rega/colheita
pesca
```

HUD:

```text
Barra secundária sempre visível.
```

## 13. HP Regen

Representa regeneração de vida.

Influenciado por:

```text
Regeneração Natural
Descanso Curto
itens amplificadores
comida/poções
Fonte de Anya
Magic em cura ativa
capstones específicos
```

Não influenciado diretamente por:

```text
Constituição sozinha.
```

Fórmula direcional:

```text
HPRegenPerSecond = BaseHPRegenFromSkill
                 * (1 + ItemRegenAmplifier)
                 * (1 + BuffRegenAmplifier)
                 * ContextMultiplier
```

Regras:

```text
BaseHPRegen sem skill = 0 ou quase 0.
HP Regen passivo deve ser lento e geralmente fora de combate.
Tomar dano pausa HP Regen por X segundos.
HP Regen forte exige item, Fonte, magia, comida ou condição.
```

## 14. MP Regen

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

Fórmula direcional:

```text
MPRegenPerSecond = BaseMPRegen
                 + (Vontade * MPRegenPerWill)
                 + SkillMPRegen
                 + EquipmentMPRegen
                 + BuffMPRegen
```

Regras:

```text
MP Regen natural é lenta.
Vontade melhora a regeneração, mas não a torna rápida sozinha.
Regeneração rápida depende de efeito explícito.
```

## 15. Stamina Regen

Representa recuperação de stamina ao longo do tempo.

Influenciado por:

```text
Constituição
fome
cansaço
comida
Survival/Sobrevivente
equipamentos
status negativos
```

Fórmula direcional:

```text
StaminaRegen = BaseStaminaRegen
             * HungerMultiplier
             * FatigueMultiplier
             * CombatMultiplier
             * (1 + SkillBonus + EquipmentBonus + BuffBonus)
```

Regras:

```text
Fome baixa reduz regen.
Cansaço alto reduz regen.
Stamina Regen em combate deve ser menor que fora de combate.
Stamina não deve recuperar rápido durante ações pesadas contínuas.
```

---

# PARTE D — Ofensivos físicos

## 16. Base Attack

Representa potência ofensiva física antes de arma e skill.

Influenciado por:

```text
Força principalmente
Destreza secundária para armas leves/ranged
Level em menor grau
```

Fórmula direcional:

```text
BaseAttack = BaseAttackValue
           + (Força * BaseAttackPerStr)
           + (Destreza * BaseAttackPerDexSmall)
           + (Level * BaseAttackPerLevel)
```

Exposição:

```text
Pode ser interno. No menu, exibir Attack Damage final é mais útil.
```

## 17. Attack Damage

Representa dano físico final após arma, material, skill, buff, resistência inimiga e contexto.

Fórmula direcional:

```text
AttackDamage = (BaseAttack + WeaponDamage + EquipmentFlatDamage)
             * WeaponScaling
             * (1 + SkillDamageBonus)
             * (1 + BuffDamageBonus)
             * ContextMultiplier
             * EnemyResistanceMultiplier
```

Regras:

```text
Força não aumenta yield de recurso.
Força aumenta dano físico e facilidade contra obstáculos físicos.
Attack Damage não substitui Stagger/Posture.
```

## 18. Melee Damage

Subtipo de Attack Damage para armas corpo a corpo.

Influenciado por:

```text
Attack Damage
Força
Destreza para armas leves
Melee/Guerreiro
material da arma
capstone Kanthor/Kaand
```

## 19. Ranged Damage

Subtipo de Attack Damage para arcos/projéteis.

Influenciado por:

```text
Attack Damage
Destreza
Inteligência em leitura/marcação
Ranged/Caçador
munição/material
alvo marcado
```

## 20. Tool Attack Damage

Dano quando uma ferramenta é usada ofensivamente.

Regras:

```text
Ferramentas podem causar dano, mas não devem superar armas dedicadas de mesmo tier.
```

## 21. Attack Speed

Representa velocidade de execução/recovery de ataques físicos.

Influenciado por:

```text
Destreza
tipo de arma
peso/material da arma
Melee/Guerreiro
Ranged/Caçador
cansaço
status negativos
```

Fórmula direcional:

```text
AttackInterval = BaseWeaponInterval
               * (1 - AttackSpeedBonusCapped)
               * FatiguePenaltyMultiplier
               * WeaponWeightMultiplier
```

## 22. Crit Chance

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

Fórmula direcional:

```text
CritChance = BaseCritChance
           + DexCritBonus
           + WeaponCritBonus
           + SkillConditionalCritBonus
           + BuffCritBonus
           + ContextCritBonus
```

Caps:

```text
Base/permanente: softcap ~35%.
Com condição: pode chegar a ~60%.
Critical window pode garantir crítico automático apenas em casos especiais bem telegrafados.
```

## 23. Crit Damage

Representa multiplicador de dano crítico.

```text
CriticalDamage = NormalDamage * CritMultiplier
CritMultiplier = BaseCritMultiplier + SkillCritDamage + EquipmentCritDamage + ContextCritDamage
```

Direção inicial:

```text
BaseCritMultiplier: 1.5x.
Builds especializadas podem chegar a 2.0x-2.5x em condição.
Acima disso apenas com capstone, item raro ou janela especial.
```

## 24. Stagger Power / Posture Damage

Stagger Power representa capacidade de abalar o inimigo.

Posture Damage representa dano aplicado à barra/estado de postura.

```text
StaggerPower = BaseStagger
             + WeaponStagger
             + (Força * StaggerPerStr)
             + SkillStaggerBonus

PostureDamage = StaggerPower
              * AttackPostureMultiplier
              * VulnerabilityMultiplier
              * SkillPostureMultiplier
```

---

# PARTE E — Ofensivos mágicos

## 25. Magic Power

Representa potência mágica geral.

Influenciado por:

```text
Inteligência
Vontade
Magic/Arcano
equipamentos mágicos
Fruto de Mana
Fonte de Anya
capstone Anya/Senya
```

```text
MagicPower = BaseMagicPower
           + (Inteligência * MagicPerInt)
           + (Vontade * MagicPerWill)
           + EquipmentMagic
           + SkillMagic
           + BuffMagic
```

## 26. Elemental / Arcane / Spiritual Power

```text
ElementalPower[type] = MagicPower
                     * (1 + ElementAffinityBonus[type])
                     * (1 + EquipmentElementBonus[type])
                     * TargetElementMultiplier[type]
```

Subtipos:

```text
Fire Power
Ice Power
Lightning Power
Water/Nature Power
Arcane Power
Spiritual Power
```

Corruption Power:

```text
futuro/controlado
não é magia inicial livre
depende de lore, risco, Nyx/Void/Blackstone e decisões futuras
```

## 27. Healing Power

Representa potência de cura.

```text
HealingAmount = BaseHeal
              + (HealingPower * HealScaling)
              + SkillHealBonus
              + EquipmentHealBonus
```

Regras:

```text
Cura mágica deve ser limitada, cara e com cooldown.
Healing Power não deve invalidar comida, poções e Survival.
```

## 28. Shield / Barrier Power

Representa força de barreiras.

```text
BarrierHP = BaseBarrier
          + (MagicPower * BarrierScaling)
          + ShieldSkillBonus
          + EquipmentBarrierBonus
```

## 29. Cast Speed / MP Cost Reduction

```text
CastTime = BaseCastTime * (1 - CastSpeedBonusCapped)
FinalMPCost = BaseMPCost * (1 - MPCostReductionCapped)
```

Regras:

```text
Cast Speed não permite spam sem custo de MP.
MP Cost Reduction permanente precisa de cap.
```

## 30. Magic Crit e Status Application

```text
MagicCritChance = BaseMagicCrit + SkillMagicCrit + ContextMagicCrit
MagicCritDamage = BaseMagicCritMultiplier + SkillMagicCritDamage
```

Direção:

```text
Magic Crit deve ser mais raro que crítico físico comum.
Semente de Senya pode abrir crit mágico ofensivo.
Magia de cura não deve critar por padrão; Anya aumenta efeito de suporte/cura.
```

```text
StatusApplyChance = BaseStatusChance
                  + ElementPowerBonus
                  + SkillStatusBonus
                  - TargetStatusResistance
```

---

# PARTE F — Defensivos

## 31. Defense / Armor

```text
Defense = BaseDefense
        + (Constituição * DefensePerCon)
        + Armor
        + SkillDefense
        + BuffDefense

Armor = ArmorBaseFromGear
      + MaterialArmorBonus
      + QualityArmorBonus
      + CraftingBonus
```

Diferença:

```text
Defense = defesa total.
Armor = contribuição material do equipamento.
```

## 32. Physical / Elemental Resistance

```text
PhysicalDamageTaken = IncomingPhysicalDamage
                    * (1 - PhysicalResistanceCapped)
                    - FlatDefenseMitigation

ElementalDamageTaken[type] = IncomingElementalDamage[type]
                           * (1 - ElementalResistance[type])
```

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

## 33. Status Resistance

```text
FinalStatusChance = IncomingStatusChance - StatusResistance[type]
FinalStatusDuration = BaseDuration * (1 - StatusDurationReduction[type])
```

Tipos:

```text
Poison
Bleed
Burn
Chill
Fear
ConfusionLite
Stun
Root
Slow
Corruption
```

## 34. Posture / Knockback Resistance

```text
PostureDamageTaken = IncomingPostureDamage * (1 - PostureResistance)
Knockback = IncomingKnockback * (1 - KnockbackResistance)
```

Influenciado por:

```text
Constituição
Força em menor grau
equipamento pesado
Melee/Guerreiro
Block ativo
```

## 35. Block Power / Stability / Recovery

Input:

```text
Left Shift.
```

```text
BlockedDamage = IncomingDamage * (1 - BlockPower)

BlockResourceDrain = IncomingImpactPower
                   * (1 - BlockStability)
                   * ShieldOrWeaponMultiplier

BlockRecoveryTime = BaseBlockRecovery * (1 - BlockRecoveryReduction)
```

Influenciado por:

```text
Block rank
Guarda Firme
Constituição
Força em menor grau
escudo/arma
material do equipamento
capstone Kanthor/Kaand
```

Regras:

```text
Block forte reduz dano, mas não é gratuito.
Block drena Stamina.
Sem Stamina, Block quebra ou perde eficiência.
Perfect Block pode anular muito mais por timing, não por segurar botão.
```

---

# PARTE G — Movimento

## 36. Movement Speed

```text
MovementSpeed = BaseMovementSpeed
              * (1 + MovementBonusCapped)
              * FatigueMultiplier
              * HungerMultiplier
              * EquipmentWeightMultiplier
              * StatusMultiplier
```

Influenciado por:

```text
Destreza em menor grau
equipamento/peso
cansaço
fome
status negativos
buffs
```

## 37. Dash

Input:

```text
Space + direção.
```

```text
DashDistance = BaseDashDistance
             * (1 + DashDistanceBonusCapped)
             * StatusMultiplier

DashCooldown = BaseDashCooldown * (1 - DashCooldownReductionCapped)

DashCost = BaseDashCost * (1 - DashCostReductionCapped)
```

Influenciado por:

```text
Destreza
Passo de Impulso
Survival/Sobrevivente
equipamento leve/pesado
cansaço/status
```

Custo:

```text
Stamina.
```

## 38. Dodge

Input:

```text
double tap direcional.
```

```text
DodgeIFrames = BaseDodgeIFrames + ReflexoDeEsquivaBonus - StatusPenalty
DodgeRecovery = BaseDodgeRecovery * (1 - DodgeRecoveryReductionCapped)
DodgeCost = BaseDodgeCost * (1 - DodgeCostReductionCapped)
```

Influenciado por:

```text
Destreza
Reflexo de Esquiva
Survival/Sobrevivente
cansaço/status
```

Custo:

```text
Stamina.
```

---

# PARTE H — Produção, coleta e economia

## 39. Resource Yield Bonus

```text
ExtraYieldChance = BaseExtraYieldChance
                 + SkillYieldBonus
                 + ToolYieldBonus
                 + BuffYieldBonus
                 + CompanionYieldBonus
```

Não influenciado diretamente por:

```text
Força sozinha.
```

## 40. Gathering Efficiency

```text
HitsRequired = BaseHitsRequired
             * ToolTierMultiplier
             * StrengthObstacleMultiplier
             * SkillEfficiencyMultiplier
```

Diferença:

```text
Gathering Efficiency = coletar com menos esforço.
Resource Yield Bonus = ganhar mais recurso.
```

## 41. Tool Stamina Cost Reduction

```text
ToolStaminaCost = BaseToolCost
                * (1 - ToolCostReduction)
                * FatigueMultiplier
                * HungerMultiplier
```

## 42. Crafting / Construction / Quality

```text
FinalCraftCost = BaseCraftCost * (1 - ConstructionCostReductionCapped)
CraftQuality = BaseQuality + CraftingSkillBonus + MaterialQuality + StationBonus
FinalConstructionCost = BaseConstructionCost * (1 - ConstructionReductionCapped)
```

Influenciado por:

```text
Inteligência
Crafting/Produção
Oficina Organizada
material
estação de trabalho
Forja Viva de Thoren
```

## 43. Loot / Gold / Treasure

```text
LootQualityRoll = BaseLootRoll
                + SkillLootBonus
                + BuffLootBonus
                + ContextBonus

GoldReward = BaseGoldReward * (1 + GoldBonusCapped)
```

Regras:

```text
Loot Bonus e Gold Bonus devem ser moderados.
Não criam loot inexistente em boss/quest único.
Não podem quebrar economia.
```

---

# PARTE I — Fome, cansaço e ambiente

## 44. Hunger Resistance

```text
HungerDrain = BaseHungerDrain
            * ActivityMultiplier
            * (1 - HungerResistanceCapped)
```

Influenciado por:

```text
Estômago Forte
comida
Constituição em menor grau
Survival
status negativos
```

## 45. Fatigue Resistance

```text
FatigueGain = BaseFatigueGain
            + StaminaSpentFatigue
            + HungerFatiguePenalty
            + TimeOfDayFatigue
            + EnvironmentFatiguePenalty
            - FatigueResistance
```

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

## 46. Sleep Recovery / Environmental Endurance

```text
SleepRecovery = BaseSleepRecovery * BedQualityMultiplier * HomeMultiplier * BuffMultiplier
EnvironmentalPenalty = BaseEnvironmentPenalty * (1 - EnvironmentalEndurance)
```

Influenciado por:

```text
qualidade da cama
casa/fazenda
Survival
Resistência Ambiental
Constituição
Vontade
equipamentos
comida
```

---

# PARTE J — Social, pets e companions

## 47. Social Influence

```text
SocialInfluence = BaseSocial
                + (Carisma * SocialPerCha)
                + ReputationBonus
                + GiftContextBonus
                + QuestBonus
                + ItemBonus
```

Uso:

```text
amizade
preços
contratos
eventos
flerte/casamento
```

## 48. Vendor Price Modifier

```text
BuyPrice = BaseBuyPrice * (1 - DiscountCapped)
SellPrice = BaseSellPrice * (1 + SellBonusCapped)
```

## 49. Relationship Gain Modifier

Influenciado por:

```text
Carisma
presentes corretos
quests pessoais
eventos
festivais
reputação
```

## 50. Companion/Pet futuros

```text
Companion Command futuro
Companion Bond Effect futuro
Pet Bond Effect futuro
Pet Combat Support futuro
```

Direção:

```text
Não expor como número cedo se companions/pets ainda estiverem simples.
```

---

# PARTE K — HUD e menus

## 51. HUD gameplay

Mostrar sempre ou quase sempre:

```text
HP
Stamina
Fome/Cansaço em ícones compactos
```

Mostrar quando relevante:

```text
MP
Dash cooldown
Dodge feedback
Block state
HP Regen ativa
MP Regen discreta
status negativos
critical window
```

Não mostrar:

```text
Breath
Fôlego
BR
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

## 52. Menu de atributos

Grupos sugeridos:

```text
Recursos: HP, MP, Stamina, Regen
Ofensivo físico: Attack Damage, Crit, Attack Speed, Stagger
Ofensivo mágico: Magic Power, Elemental, Healing, Cast, MP Cost
Defensivo: Defense, Armor, Resistances, Block
Movimento: Movement, Dash, Dodge
Produção: Yield, Gathering, Crafting
Exploração: Loot, Gold, Hunger/Fatigue, Environment
Social: Social Influence, Companions, Pets
```

## 53. Tooltips e transparência

Cada stat derivado importante deve explicar sua origem:

```text
Attack Damage
  Base: X
  Força: +Y
  Arma: +Z
  Skill: +A%
  Buff: +B%

Block Power
  Base Block: X%
  Skill Block: +Y%
  Escudo: +Z%
  Penalidade atual: -A%
```

---

# PARTE L — Save/load

## 54. Persistência

Salvar:

```text
atributos centrais
level
skills/ranks/capstones
equipamentos
buffs persistentes
status persistentes
arquétipos ativos
flags de Fonte/Anya/capstones exclusivos
HP atual
MP atual
Stamina atual
fome atual
cansaço atual
```

Recalcular ao carregar:

```text
HP Max
MP Max
Stamina Max
Attack Damage
Defense
resistências
movimento
produção
loot/social derivados
```

Não salvar:

```text
Breath atual
Breath Max
BR
```

---

# PARTE M — Decisões fechadas

```text
Breath/Fôlego removido como atributo/recurso.
O cálculo deve ser em camadas: base + atributos + level + equipamento + skill + buff + contexto + caps.
Força melhora dano físico, stagger e esforço contra obstáculos, não yield direto.
Constituição aumenta HP, Stamina, estabilidade física, resistência e tolerância a cansaço.
Constituição não gera HP Regen sozinha.
Vontade influencia MP e MP Regen lenta.
Destreza influencia timing, ataque leve, dodge, dash, movement e crit condicional.
Inteligência influencia magia técnica, crafting e leitura de sistemas.
Carisma influencia relação social, companions e economia social.
Dash custa Stamina.
Dodge custa Stamina.
Block drena Stamina.
Resource Yield Bonus é derivado de skills/ferramentas/buffs, não Força pura.
Gathering Efficiency é diferente de Resource Yield Bonus.
HP Regen é derivado de skill/efeito explícito, não Constituição pura.
MP Regen é lenta e ligada principalmente a Vontade/Magic.
Crit, Attack Speed, Movement Speed, HP Regen, MP Regen, Block Reduction e Yield precisam de caps/softcaps.
Nem todo derivado aparece na HUD; muitos aparecem só em menu/tooltip.
A maioria dos derivados deve ser recalculada no load, não salva como valor fixo.
```

---

# PARTE N — Pendências

```text
Remover Breath/Fôlego dos demais documentos canônicos.
Remover BR das tabelas de monstros.
Substituir BR por MovementProfile, RecoveryProfile, PressureProfile ou traits.
Renomear/redefinir Respiração Controlada em Survival.
Definir fórmulas finais numéricas.
Definir nomes finais em PT-BR/EN para cada stat.
Definir quais stats aparecem no menu inicial.
Definir quais stats são ocultos e só afetam internamente.
Definir se Defense e Armor serão separados ou fundidos na implementação.
Definir se Base Attack precisa existir exposto ou apenas como cálculo interno.
Definir valores base por level.
Definir impacto exato de cada atributo central por ponto.
Definir impacto de equipamentos e materiais.
Definir limites máximos/caps finais de Crit Chance, Attack Speed, Cast Speed, Movement Speed e HP/MP Regen.
Definir se magia terá crítico por padrão ou apenas com Senya/itens.
Definir se armor penetration entra no primeiro ciclo de combate ou fica futuro.
Definir como buffs temporários aparecem no menu/HUD.
Definir contratos de save/load para buffs e stats permanentes.
