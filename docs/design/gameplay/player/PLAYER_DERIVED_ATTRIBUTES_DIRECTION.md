# Cindar's Hope — Player Derived Attributes Direction

> **Status:** documento canônico de direção dos atributos derivados do personagem  
> **Local:** `docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md`  
> **Complementa:**  
> - `docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md`  
> - `docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md`  
> **Não é spec implementável.** Este documento define quais atributos derivados existem, o que representam, como se conectam aos atributos centrais, skills, equipamentos, HUD, combate, produção e save/load.

---

## 0. Objetivo

Este documento define os atributos derivados do jogador.

Atributos centrais:

```text
Força
Constituição
Destreza
Inteligência
Vontade
Carisma
```

Atributos derivados são valores calculados a partir de:

```text
atributos centrais
level
equipamentos
material/tier do equipamento
skills
skill ranks
capstones
arquétipos/jobs ativos
buffs temporários
comida/poções
status negativos
condição atual do personagem
ambiente/bioma
vulnerabilidades/resistências do alvo
```

Regra central:

```text
Atributo central dá aptidão bruta.
Atributo derivado traduz aptidão em gameplay.
Skill tree dá domínio, desbloqueios e especialização.
Equipamento dá capacidade material.
Buff dá modificação temporária.
Status negativo aplica penalidade situacional.
```

---

# PARTE A — Modelo geral de cálculo

## 1. Fórmula em camadas

Todo atributo derivado deve seguir uma estrutura de cálculo parecida:

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

Nem todo stat precisa crescer sem limite.

Tipos de limite:

```text
Hard Cap
  limite absoluto. Exemplo: Crit Chance não passa de 60% fora de efeitos especiais.

Soft Cap
  depois de certo valor, cada ponto adicional rende menos.

Context Cap
  limite diferente por contexto. Exemplo: Movement Speed tem cap menor na cidade/fazenda e cap maior durante Dash.
```

Direção:

```text
Crit Chance, Attack Speed, Cast Speed, Movement Speed, HP Regen, MP Regen, Block Reduction e Resource Yield precisam de caps.
HP, MP, Stamina e Breath podem crescer mais livremente, mas ainda precisam de curva controlada.
```

## 3. Fórmula de softcap sugerida

Quando um stat precisar de diminishing returns:

```text
EffectiveValue = SoftCap + (RawValue - SoftCap) * DiminishingFactor
```

Exemplo:

```text
Se Crit Chance softcap = 35%
Raw Crit Chance = 50%
DiminishingFactor = 0.5
Effective Crit Chance = 35 + (50 - 35) * 0.5 = 42.5%
```

## 4. Ordem de cálculo recomendada

```text
1. Ler atributos centrais atuais.
2. Calcular recursos máximos: HP, MP, Stamina, Breath.
3. Calcular ofensivos base: Base Attack, Magic Power, Attack Speed, Cast Speed.
4. Aplicar arma/ferramenta/equipamento.
5. Aplicar skills e capstones.
6. Aplicar buffs/comida/poções.
7. Aplicar status negativos.
8. Aplicar contexto do alvo/ambiente.
9. Aplicar caps/softcaps.
10. Expor na HUD/menu conforme relevância.
```

---

# PARTE B — Lista canônica de atributos derivados

## 5. Recursos principais

```text
HP Max
MP Max
Stamina Max
Breath Max
HP Regen
MP Regen
Stamina Regen
Breath Recovery
```

## 6. Ofensivos físicos

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

## 7. Ofensivos mágicos

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

## 8. Defensivos

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

## 9. Movimento e controle

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

## 10. Produção, coleta e economia

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

## 11. Condição, social e companions

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

## 12. HP Max

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

Direção inicial sugerida:

```text
BaseHP: 100
HPPerLevel: 4-8
HPPerCon: 12-18
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

## 13. MP Max

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

Direção inicial:

```text
BaseMP: 30-50, ou 0 até magia ser desbloqueada, conforme decisão futura.
MPPerWill > MPPerInt.
```

HUD:

```text
Aparece quando magia/item mágico for desbloqueado ou equipado.
```

## 14. Stamina Max

Representa energia física disponível para ações.

Influenciado por:

```text
Constituição
Força em menor grau para ações pesadas
Level
Survival
Crafting
comida
equipamentos
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

Usos:

```text
ferramentas
ataques físicos
block
dodge
dash
corrida
pesca/mineração/corte/plantio
```

HUD:

```text
Barra secundária sempre visível.
```

## 15. Breath Max / Fôlego Máximo

Representa capacidade de sustentar esforço sob pressão.

Influenciado por:

```text
Constituição
Destreza
Vontade em menor grau
Survival/Sobrevivente
Melee/Guerreiro para block/armas pesadas
equipamentos leves/pesados
```

Fórmula direcional:

```text
BreathMax = BaseBreath
          + (Constituição * BreathPerCon)
          + (Destreza * BreathPerDex)
          + (Vontade * BreathPerWillSmall)
          + SkillBreath
          + EquipmentBreath
          + BuffBreath
```

Usos:

```text
corrida
dash
dodge
block sustentado
ritmo de combate
long fights
ambiente hostil
```

HUD:

```text
Medidor compacto, expandido em combate/caverna/movimento intenso.
```

## 16. HP Regen

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

Caps:

```text
Normal: até ~1% HP Max por segundo fora de combate.
Com item/capstone: pode dobrar temporariamente, mas com condição/cooldown.
Em combate: por padrão desativado ou muito reduzido.
```

## 17. MP Regen

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

## 18. Stamina Regen

Representa recuperação de stamina ao longo do tempo.

Influenciado por:

```text
Constituição
Breath atual
fome
cansaço
comida
Survival
equipamentos
```

Fórmula direcional:

```text
StaminaRegen = BaseStaminaRegen
             * HungerMultiplier
             * FatigueMultiplier
             * (1 + SkillBonus + EquipmentBonus + BuffBonus)
```

Regras:

```text
Fome baixa reduz regen.
Cansaço alto reduz regen.
Stamina não deve recuperar rápido durante ações pesadas contínuas.
```

## 19. Breath Recovery

Representa recuperação do fôlego.

Influenciado por:

```text
Constituição
Destreza
Survival
Respiração Controlada
cansaço
status ambientais
equipamento pesado
```

Fórmula direcional:

```text
BreathRecovery = BaseBreathRecovery
               + AttributeBonus
               + SkillBonus
               - EquipmentWeightPenalty
               - FatiguePenalty
               - StatusPenalty
```

---

# PARTE D — Ofensivos físicos

## 20. Base Attack

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

Uso:

```text
base para Attack Damage físico.
```

Exposição:

```text
Pode ser interno. No menu, exibir Attack Damage final é mais útil.
```

## 21. Attack Damage

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

Fontes:

```text
BaseAttack
arma equipada
material da arma
Melee/Guerreiro
Ranged/Caçador
buffs
critical windows
resistência/vulnerabilidade do inimigo
```

Regras:

```text
Força não aumenta yield de recurso.
Força aumenta dano físico e facilidade contra obstáculos físicos.
Attack Damage não deve substituir Stagger/Posture.
```

## 22. Melee Damage

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

Fórmula direcional:

```text
MeleeDamage = AttackDamage * MeleeWeaponMultiplier * MeleeSkillMultiplier
```

## 23. Ranged Damage

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

Fórmula direcional:

```text
RangedDamage = AttackDamage * RangedWeaponMultiplier * RangedSkillMultiplier * MarkMultiplier
```

## 24. Tool Attack Damage

Dano quando uma ferramenta é usada ofensivamente.

Influenciado por:

```text
Força
ferramenta
tier/material
Melee em menor grau, se permitido
```

Regras:

```text
Ferramentas podem causar dano, mas não devem superar armas dedicadas de mesmo tier.
```

## 25. Attack Speed

Representa velocidade de execução/recovery de ataques físicos.

Influenciado por:

```text
Destreza
tipo de arma
peso/material da arma
Melee/Guerreiro
Ranged/Caçador
Breath em ritmo sustentado
cansaço
```

Fórmula direcional:

```text
AttackInterval = BaseWeaponInterval
               * (1 - AttackSpeedBonusCapped)
               * FatiguePenaltyMultiplier
               * WeaponWeightMultiplier
```

Caps:

```text
Attack Speed não deve reduzir intervalo além de ~35-45% por meios permanentes.
Buffs temporários podem exceder levemente com cooldown/condição.
```

## 26. Charge Speed

Representa velocidade para carregar ataque pesado ou disparo carregado.

Influenciado por:

```text
Força para armas pesadas
Destreza para ranged/leve
Breath
skills específicas
```

Fórmula direcional:

```text
ChargeTime = BaseChargeTime * (1 - ChargeSpeedBonusCapped)
```

## 27. Crit Chance

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
Critical window pode garantir crítico automático se definido no sistema de combate/caverna.
```

## 28. Crit Damage

Representa multiplicador de dano crítico.

Fórmula direcional:

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

## 29. Stagger Power

Representa capacidade de abalar o inimigo.

Influenciado por:

```text
Força
arma pesada
Melee/Guerreiro
ataques carregados
material da arma
capstone Kaand
```

Fórmula direcional:

```text
StaggerPower = BaseStagger
             + WeaponStagger
             + (Força * StaggerPerStr)
             + SkillStaggerBonus
```

## 30. Posture Damage

Representa dano aplicado à barra/estado de postura do inimigo.

Fórmula direcional:

```text
PostureDamage = StaggerPower
              * AttackPostureMultiplier
              * VulnerabilityMultiplier
              * SkillPostureMultiplier
```

Regras:

```text
Posture Damage deve ser importante contra elites/bosses.
Nem todo inimigo precisa ter barra visível de postura.
```

## 31. Armor Penetration

Representa quanto da defesa/armadura inimiga é ignorada.

Influenciado por:

```text
armas perfurantes
prata/mithril/liga especial
skills específicas
buffs/óleos de arma
```

Fórmula direcional:

```text
EffectiveEnemyArmor = EnemyArmor * (1 - ArmorPenetrationCapped)
```

Caps:

```text
Permanente: até ~30%.
Condicional/consumível: até ~50%, com custo.
```

## 32. Knockback Power

Representa empurrão aplicado ao inimigo.

Influenciado por:

```text
Força
arma pesada
Golpe de Ruptura
peso do inimigo
resistência do inimigo
```

Fórmula direcional:

```text
Knockback = (AttackKnockback + StrengthBonus + SkillBonus) * EnemyWeightResistance
```

---

# PARTE E — Ofensivos mágicos

## 33. Magic Power

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

Fórmula direcional:

```text
MagicPower = BaseMagicPower
           + (Inteligência * MagicPerInt)
           + (Vontade * MagicPerWill)
           + EquipmentMagic
           + SkillMagic
           + BuffMagic
```

## 34. Elemental Power

Representa potência com magias elementais.

Subtipos:

```text
Fire Power
Ice Power
Lightning Power
Water/Nature Power
```

Fórmula direcional:

```text
ElementalPower[type] = MagicPower
                     * (1 + ElementAffinityBonus[type])
                     * (1 + EquipmentElementBonus[type])
                     * TargetElementMultiplier[type]
```

## 35. Arcane Power

Representa potência de magia arcana não-elemental.

Influenciado por:

```text
Inteligência
Foco Arcano
Projétil Arcano
itens mágicos
```

## 36. Spiritual Power

Representa potência espiritual/divina.

Influenciado por:

```text
Vontade
Eco da Fonte
Semente Arcana de Anya
Fonte de Anya
Água Viva
```

Usos:

```text
cura
purificação
barreiras espirituais
resistência a corrupção
```

## 37. Corruption Power futuro/controlado

Representa potência de efeitos sombrios/corrompidos.

Direção:

```text
Não é magia inicial livre.
Depende de lore, risco, Nyx/Void/Blackstone e decisões futuras.
Deve ter custo narrativo ou mecânico.
```

## 38. Healing Power

Representa potência de cura.

Fórmula direcional:

```text
HealingAmount = BaseHeal
              + (HealingPower * HealScaling)
              + SkillHealBonus
              + EquipmentHealBonus
```

Influenciado por:

```text
Vontade
Magic/Arcano
Fonte de Anya
Água Viva
itens de cura
equipamentos espirituais
Semente Arcana de Anya
```

Regras:

```text
Cura mágica deve ser limitada, cara e com cooldown.
Healing Power não deve invalidar comida, poções e Survival.
```

## 39. Shield / Barrier Power

Representa força de barreiras.

Fórmula direcional:

```text
BarrierHP = BaseBarrier
          + (MagicPower * BarrierScaling)
          + ShieldSkillBonus
          + EquipmentBarrierBonus
```

Regras:

```text
Barreira não deve bloquear tudo.
Duração curta.
Custo médio/alto de MP.
```

## 40. Cast Speed

Representa velocidade de conjuração/canalização.

Fórmula direcional:

```text
CastTime = BaseCastTime * (1 - CastSpeedBonusCapped)
```

Influenciado por:

```text
Inteligência
Destreza em menor grau
Magic/Arcano
equipamento
cansaço/status
```

Caps:

```text
Permanente: redução até ~35%.
Buffs temporários: maior, com custo/condição.
```

## 41. MP Cost Reduction

Representa redução de custo de MP.

Fórmula direcional:

```text
FinalMPCost = BaseMPCost * (1 - MPCostReductionCapped)
```

Caps:

```text
Permanente: até ~40%.
Efeitos especiais/capstones: podem reduzir mais em uma magia específica ou janela curta.
```

## 42. Magic Crit Chance e Magic Crit Damage

Magias podem critar apenas se o sistema final permitir.

Direção:

```text
Magic Crit deve ser mais raro que crítico físico comum.
Semente de Senya pode abrir crit mágico ofensivo.
Magia de cura não deve critar por padrão; pode ter bônus de efeito por Anya.
```

Fórmula direcional:

```text
MagicCritChance = BaseMagicCrit + SkillMagicCrit + ContextMagicCrit
MagicCritDamage = BaseMagicCritMultiplier + SkillMagicCritDamage
```

## 43. Status Application Power

Representa chance/potência de aplicar Burn, Chill, Shock, Slow, Poison etc.

Fórmula direcional:

```text
StatusApplyChance = BaseStatusChance
                  + ElementPowerBonus
                  + SkillStatusBonus
                  - TargetStatusResistance
```

Regras:

```text
Bosses devem ter resistência maior.
Status forte precisa de duração curta, telegraph ou cooldown.
```

---

# PARTE F — Defensivos

## 44. Defense

Representa defesa total do personagem contra dano físico.

Fórmula direcional:

```text
Defense = BaseDefense
        + (Constituição * DefensePerCon)
        + Armor
        + SkillDefense
        + BuffDefense
```

## 45. Armor

Representa proteção material do equipamento.

Fórmula direcional:

```text
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

## 46. Physical Resistance

Representa redução percentual contra dano físico.

Fórmula direcional:

```text
PhysicalDamageTaken = IncomingPhysicalDamage
                    * (1 - PhysicalResistanceCapped)
                    - FlatDefenseMitigation
```

Caps:

```text
Resistência física permanente não deve passar de ~50-60%.
Block e efeitos temporários podem reduzir mais, mas com custo/condição.
```

## 47. Elemental Resistance

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

Fórmula direcional:

```text
ElementalDamageTaken[type] = IncomingElementalDamage[type]
                           * (1 - ElementalResistance[type])
```

Influenciado por:

```text
Constituição
Vontade
equipamentos
comida
Survival
Magic
bioma/ambiente
```

## 48. Status Resistance

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

Fórmula direcional:

```text
FinalStatusChance = IncomingStatusChance - StatusResistance[type]
FinalStatusDuration = BaseDuration * (1 - StatusDurationReduction[type])
```

Regras:

```text
Constituição ajuda contra físico/tóxico.
Vontade ajuda contra mental/espiritual.
Survival ajuda contra ambiente/run.
Magic ajuda contra corrupção/Nyx/Void.
```

## 49. Posture Resistance

Representa resistência a stagger/knockback/quebra de postura.

Fórmula direcional:

```text
PostureDamageTaken = IncomingPostureDamage * (1 - PostureResistance)
```

Influenciado por:

```text
Constituição
Força em menor grau
equipamento pesado
Melee/Guerreiro
Block ativo
```

## 50. Block Power

Representa quanto dano o Block reduz.

Input:

```text
Left Shift.
```

Fórmula direcional:

```text
BlockedDamage = IncomingDamage * (1 - BlockPower)
```

Influenciado por:

```text
Block rank
escudo/arma
Força
Constituição
Melee/Guerreiro
material do equipamento
capstone Kanthor/Kaand
```

Caps:

```text
Block Power permanente não deve anular 100% do dano.
Perfect Block pode anular muito mais por timing, não por segurar botão.
```

## 51. Block Stability

Representa quanto Stamina/Breath o Block consome ao receber impacto.

Fórmula direcional:

```text
BlockResourceDrain = IncomingImpactPower
                   * (1 - BlockStability)
                   * ShieldOrWeaponMultiplier
```

Regras:

```text
Block forte reduz dano, mas não deve ser gratuito.
Impactos fortes drenam recurso.
Sem recurso, Block quebra ou perde eficiência.
```

## 52. Block Recovery

Representa tempo para agir após bloquear.

Fórmula direcional:

```text
BlockRecoveryTime = BaseBlockRecovery * (1 - BlockRecoveryReduction)
```

Influenciado por:

```text
Destreza
Guarda Firme
Contra-Ataque
equipamento
```

---

# PARTE G — Movimento

## 53. Movement Speed

Representa velocidade base de deslocamento.

Fórmula direcional:

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

Caps:

```text
Variação permanente deve ser pequena para não quebrar mapas, câmera e colisão.
```

## 54. Dash Distance

Input:

```text
Space + direção.
```

Fórmula direcional:

```text
DashDistance = BaseDashDistance
             * (1 + DashDistanceBonusCapped)
             * StatusMultiplier
```

Influenciado por:

```text
Dash base desbloqueado por tutorial/progressão
Passo de Impulso
Destreza
Breath
equipamento/status
```

## 55. Dash Cooldown Reduction

Fórmula direcional:

```text
DashCooldown = BaseDashCooldown * (1 - DashCooldownReductionCapped)
```

Caps:

```text
Dash não deve ficar spammável sem custo.
Cooldown mínimo precisa preservar leitura de combate.
```

## 56. Dash Cost Reduction

Fórmula direcional:

```text
DashCost = BaseDashCost * (1 - DashCostReductionCapped)
```

Influenciado por:

```text
Passo de Impulso
Respiração Controlada
Breath atual
equipamento leve
```

## 57. Dodge Invulnerability Window

Input:

```text
double tap direcional.
```

Fórmula direcional:

```text
DodgeIFrames = BaseDodgeIFrames + ReflexoDeEsquivaBonus - StatusPenalty
```

Regras:

```text
Dodge não deve tornar o jogador invulnerável continuamente.
Janela deve ser curta e legível.
```

## 58. Dodge Recovery

Representa tempo até o jogador poder agir plenamente após dodge.

Fórmula direcional:

```text
DodgeRecovery = BaseDodgeRecovery * (1 - DodgeRecoveryReductionCapped)
```

## 59. Dodge Cost Reduction

Fórmula direcional:

```text
DodgeCost = BaseDodgeCost * (1 - DodgeCostReductionCapped)
```

## 60. Collision Recovery

Representa quanto tempo o jogador fica travado/empurrado após colisões ou hits leves.

Influenciado por:

```text
Constituição
Posture Resistance
Destreza
status
```

---

# PARTE H — Produção, coleta e economia

## 61. Resource Yield Bonus

Representa chance/quantidade extra de recurso.

Influenciado por:

```text
Crafting/Produção
Coleta Eficiente
Prospector de Superfície
Garimpo de Run, para caverna
ferramenta/material
buffs
companions/pets
```

Não influenciado diretamente por:

```text
Força sozinha.
```

Fórmula direcional:

```text
ExtraYieldChance = BaseExtraYieldChance
                 + SkillYieldBonus
                 + ToolYieldBonus
                 + BuffYieldBonus
                 + CompanionYieldBonus
```

Caps:

```text
Yield extra comum pode crescer moderadamente.
Recursos raros/endgame devem ter cap menor e depender de contexto.
```

## 62. Gathering Efficiency

Representa custo/tempo/golpes para coletar.

Influenciado por:

```text
Força para obstáculos físicos
ferramenta
Crafting/Produção
Stamina
cansaço
```

Fórmula direcional:

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

## 63. Tool Stamina Cost Reduction

Representa redução de custo de stamina ao usar ferramenta.

Fórmula direcional:

```text
ToolStaminaCost = BaseToolCost
                * (1 - ToolCostReduction)
                * FatigueMultiplier
                * HungerMultiplier
```

Influenciado por:

```text
Mãos de Lavrador
ferramenta
equipamento
comida
cansaço/fome
```

## 64. Tool Action Speed

Representa velocidade de animação/execução de ferramenta.

Fórmula direcional:

```text
ToolActionTime = BaseToolActionTime * (1 - ToolSpeedBonusCapped)
```

Regras:

```text
Não acelerar demais a ponto de quebrar animação/tile interaction.
```

## 65. Mining Efficiency

Representa eficiência em mineração.

Influenciado por:

```text
Força para quebrar rochas
picareta/material
Prospector/Garimpo
caverna/bioma
buffs
```

Fórmula direcional:

```text
MiningResult = BaseMiningResult
             + YieldRoll
             + RareOreRoll
```

## 66. Woodcutting Efficiency

Representa eficiência em cortar madeira.

Influenciado por:

```text
Força
machado/material
Lenhador Prático
árvore/tronco
buffs
```

## 67. Farming Efficiency

Representa eficiência agrícola.

Influenciado por:

```text
Mãos de Lavrador
Coleta Eficiente
ferramentas
fertilizantes
irrigação
companions
fazenda nível
```

## 68. Watering Efficiency

Representa alcance/custo/automação de rega.

Influenciado por:

```text
regador
irrigação
automação
Crafting/Produção
Mãos de Lavrador
```

## 69. Fishing Efficiency

Representa chance de captura, qualidade e esforço na pesca.

Influenciado por:

```text
Destreza
vara de pesca
Crafting/Produção
Pescador, se existir/retornar como node
comida/buffs
bioma/água
```

## 70. Crafting Efficiency

Representa custo, qualidade, velocidade ou desperdício em crafting/construção.

Fórmula direcional:

```text
FinalCraftCost = BaseCraftCost * (1 - ConstructionCostReductionCapped)
CraftQuality = BaseQuality + CraftingSkillBonus + MaterialQuality + StationBonus
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

## 71. Construction Cost Reduction

Representa redução específica para construções/estruturas.

Fórmula direcional:

```text
FinalConstructionCost = BaseConstructionCost * (1 - ConstructionReductionCapped)
```

Caps:

```text
Materiais comuns podem reduzir até ~15-25%.
Materiais raros/únicos não devem ser reduzidos ou têm redução mínima.
```

## 72. Craft Quality Bonus

Representa qualidade extra em item criado.

Influenciado por:

```text
Inteligência
estação
material
Crafting/Produção
Forja Viva de Thoren
```

## 73. Cooking Quality Bonus

Representa melhoria de comida.

Influenciado por:

```text
Cozinha Sustentadora
ingredientes
qualidade da cozinha
Inteligência
```

## 74. Potion / Consumable Potency

Representa potência de poções, antídotos, óleos e bombas leves.

Influenciado por:

```text
Alquimia Prática
Inteligência
material
estação
```

## 75. Loot Bonus

Representa chance de loot melhor.

Fórmula direcional:

```text
LootQualityRoll = BaseLootRoll
                + SkillLootBonus
                + BuffLootBonus
                + ContextBonus
```

Influenciado por:

```text
Survival
Faro de Tesouro
Olho do Caçador
Ranged para criaturas orgânicas
itens raros
buffs
```

Regras:

```text
Loot Bonus deve ser moderado.
Não cria loot inexistente em boss/quest único.
```

## 76. Gold Bonus

Representa chance/quantidade extra de ouro em drops/tesouros.

Fórmula direcional:

```text
GoldReward = BaseGoldReward * (1 + GoldBonusCapped)
```

Caps:

```text
Gold Bonus deve ser baixo/moderado.
Economia da cidade e fazenda não pode ser quebrada por farming de ouro.
```

---

# PARTE I — Fome, cansaço e ambiente

## 77. Hunger Resistance

Representa resistência à perda de fome.

Fórmula direcional:

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

## 78. Fatigue Resistance

Representa resistência ao acúmulo de cansaço.

Fórmula direcional:

```text
FatigueGain = BaseFatigueGain
            + StaminaSpentFatigue
            + HungerFatiguePenalty
            + TimeOfDayFatigue
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

## 79. Sleep Recovery Bonus

Representa quanto sono recupera recursos e remove penalidades.

Influenciado por:

```text
qualidade da cama
casa/fazenda
cansaço acumulado
comida/eventos
possíveis buffs sociais/cônjuge/pet futuros
```

## 80. Environmental Endurance

Representa resistência geral a biomas/ambientes hostis.

Influenciado por:

```text
Survival
Resistência Ambiental
Constituição
Vontade
equipamentos
comida
```

Aplica em:

```text
frio
calor
gás
gelo ambiental
corrupção leve
pressão de caverna profunda
```

---

# PARTE J — Social, pets e companions

## 81. Social Influence

Representa influência social geral.

Fórmula direcional:

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

## 82. Vendor Price Modifier

Representa desconto/bonificação de venda.

Influenciado por:

```text
Carisma
reputação
cidade
quests
arquétipo mercador futuro
itens sociais
```

Fórmula direcional:

```text
BuyPrice = BaseBuyPrice * (1 - DiscountCapped)
SellPrice = BaseSellPrice * (1 + SellBonusCapped)
```

Caps:

```text
Descontos e bônus de venda precisam de cap forte para não quebrar economia.
```

## 83. Relationship Gain Modifier

Representa ganho de relação com NPCs.

Influenciado por:

```text
Carisma
presentes corretos
quests pessoais
eventos
festivais
reputação
```

## 84. Companion Command futuro

Representa eficiência em coordenar companions.

Influenciado por:

```text
Carisma
Vontade
relacionamento com companion
arquétipo Líder
quests
```

Direção:

```text
Não implementar como número exposto cedo se companions ainda estiverem simples.
```

## 85. Companion Bond Effect futuro

Representa força dos bônus de companion.

Influenciado por:

```text
relação
quests
Carisma
Vontade
uso recorrente
```

## 86. Pet Bond Effect futuro

Representa força dos bônus de pet.

Influenciado por:

```text
cuidado
alimentação
vínculo
Carisma
Animal/Pet systems futuros
itens
```

Direção:

```text
Pode afetar detecção de traps, ajuda contra swarms, achados, sorte e companhia na fazenda.
```

## 87. Pet Combat Support futuro

Representa eficiência de pet em combate.

Influenciado por:

```text
vínculo
treinamento
comida
pet específico
Carisma em menor grau
```

---

# PARTE K — HUD e menus

## 88. HUD gameplay

Mostrar sempre ou quase sempre:

```text
HP
Stamina
Fome/Cansaço em ícones compactos
```

Mostrar quando relevante:

```text
MP
Breath
Dash cooldown
Dodge feedback
Block state
HP Regen ativa
MP Regen discreta
status negativos
critical window
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

## 89. Menu de atributos

Menu deve mostrar:

```text
atributos centrais
stats derivados principais
fonte dos bônus
efeito de equipamento atual
efeito de skills
efeitos temporários
```

Sugestão de grupos:

```text
Recursos: HP, MP, Stamina, Breath, Regen
Ofensivo físico: Attack Damage, Crit, Attack Speed, Stagger
Ofensivo mágico: Magic Power, Elemental, Healing, Cast, MP Cost
Defensivo: Defense, Armor, Resistances, Block
Movimento: Movement, Dash, Dodge
Produção: Yield, Gathering, Crafting
Exploração: Loot, Gold, Hunger/Fatigue, Environment
Social: Social Influence, Companions, Pets
```

## 90. Tooltips e transparência

Cada stat derivado importante deve conseguir explicar sua origem:

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

## 91. Persistência

Não é necessário salvar todos os atributos derivados calculados, se eles puderem ser recalculados.

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
```

Recalcular ao carregar:

```text
HP Max
MP Max
Stamina Max
Breath Max
Attack Damage
Defense
resistências
movimento
produção
loot/social derivados
```

Salvar valor atual de recursos:

```text
HP atual
MP atual
Stamina atual
Breath atual
fome atual
cansaço atual
```

---

# PARTE M — Decisões fechadas

```text
Atributos derivados existem para traduzir atributos centrais em gameplay.
O cálculo deve ser em camadas: base + atributos + level + equipamento + skill + buff + contexto + caps.
Força melhora dano físico, stagger e esforço contra obstáculos, não yield direto.
Constituição aumenta HP e tolerância, não HP regen sozinha.
Vontade influencia MP e regen lenta de MP.
Destreza influencia timing, ataque leve, dodge e crit condicional.
Inteligência influencia magia técnica, crafting e leitura de sistemas.
Carisma influencia relação social, companions e economia social.
Dash Distance é derivado de movimento e Survival, não active slot.
Dodge Invulnerability Window é derivado de movimento e Reflexo de Esquiva.
Block Power e Block Stability são derivados defensivos ligados a Left Shift, Melee e equipamento.
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
