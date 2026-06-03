# Cindar's Hope — Player Derived Attributes Direction

> **Status:** documento canônico de direção dos atributos derivados do personagem  
> **Local:** `docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md`  
> **Complementa:**  
> - `docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md`  
> - `docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md`  
> **Não é spec implementável.** Este documento define quais atributos derivados existem, o que representam, como se conectam aos atributos centrais, skills, equipamentos, HUD, combate, produção e save/load.

---

## 0. Decisões canônicas atuais

### 0.1 Breath/Fôlego removido

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

### 0.2 Constituição não é atributo defensivo universal

Constituição estava forte demais porque acumulava:

```text
HP
Stamina
Stamina Regen
Defense
Block Stability
Posture Resistance
resistência física/status
tolerância a cansaço
```

Nova direção:

```text
Constituição continua importante, mas não deve dominar defesa, stamina e sobrevivência sozinha.
HP cresce pouco por Constituição.
Stamina cresce de forma distribuída entre Constituição, Força e Destreza.
Defense vem principalmente de Armor/equipamento, não de Constituição.
Block Stability vem de Melee + escudo + Constituição moderada.
Fatigue Resistance vem de Survival + Constituição moderada.
```

### 0.3 Stamina deve continuar exigindo organização

Stamina não deve escalar de forma que:

```text
level 1 = 4 ações antes de zerar
level 50 = 20-30 ataques antes de zerar
```

Direção correta:

```text
Stamina Max cresce devagar.
Custos de ações escalam com tier de arma, peso, ferramenta e tipo de ação.
Level alto dá mais margem e conforto, mas não permite spam infinito.
Stamina Regen em combate é baixa.
Fome e Cansaço reduzem Stamina Regen e eficiência.
```

---

# PARTE A — Modelo geral de cálculo

## 1. Fórmula em camadas

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
Percent bonuses precisam de cap/softcap quando puderem quebrar o jogo.
StatusPenalty deve ser claro e legível.
ContextMultiplier só deve existir quando houver condição explícita.
```

## 2. Softcap sugerido

```text
EffectiveValue = SoftCap + (RawValue - SoftCap) * DiminishingFactor
```

Usar em:

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
Stamina Regen
Block Power
Block Stability
Resource Yield Bonus
Gold Bonus
Vendor Price Modifier
```

---

# PARTE B — Atributos centrais e responsabilidades

## 3. Força

Representa potência física e esforço bruto.

Afeta principalmente:

```text
dano físico
armas pesadas
stagger
posture damage
knockback
ferramentas pesadas
quebra de troncos/rochas/obstáculos
Stamina Max em menor grau
custo relativo de ações pesadas em menor grau
```

Não afeta diretamente:

```text
yield de recurso
loot
gold
HP máximo
HP Regen
```

## 4. Constituição

Representa tolerância física e estrutura corporal.

Afeta principalmente:

```text
HP Max de forma moderada
Stamina Max de forma moderada
Status Resistance física
Posture Resistance
Block Stability de forma moderada
Fatigue Resistance de forma moderada
```

Não deve dominar:

```text
Defense
Stamina Regen
Block Power
HP Regen
```

Regra:

```text
Constituição é segurança e tolerância, não multiplicador universal de sobrevivência.
```

## 5. Destreza

Representa coordenação, reação e eficiência de movimento.

Afeta principalmente:

```text
Dodge
Dash
Movement Speed
Attack Speed
Recovery
Crit Chance condicional
Stamina Max em menor grau
redução de custo/recovery de ações leves
```

## 6. Inteligência

Afeta:

```text
Magic Power técnico
crafting
máquinas/tecnologia bromeciana
leitura de monstros/traps/recursos
uso de itens mágicos técnicos
```

## 7. Vontade

Afeta:

```text
MP Max
MP Regen lenta
Spiritual Power
resistência mental/espiritual
corrupção/Nyx/Void
Fonte de Anya
```

## 8. Carisma

Afeta:

```text
Social Influence
Vendor Price Modifier futuro
Relationship Gain
Companion/Pet bond futuro
reputação/eventos sociais
```

---

# PARTE C — Recursos principais

## 9. HP Max do jogador

HP representa vida física.

Nova fórmula direcional do jogador:

```text
HPMax = BaseHP
      + (Level * 2)
      + (Constituição * 5)
      + EquipmentHP
      + SkillHP
      + BuffHP
```

Constantes recomendadas para teste:

```text
BaseHP = 110
HPPerLevel = 2
HPPerCon = 5
```

Leitura:

```text
Level aumenta HP, mas devagar.
Constituição importa, mas não transforma o personagem em elite só por atributo.
Equipamentos, comida, skills e estilo defensivo completam a sobrevivência.
```

Exemplo level 30, CON 8, equipamento +20 HP:

```text
HPMax = 110 + 30*2 + 8*5 + 20
HPMax = 230
```

Exemplo level 30, CON 15, equipamento +20 HP:

```text
HPMax = 110 + 30*2 + 15*5 + 20
HPMax = 265
```

Resultado esperado:

```text
Mesmo focando Constituição, o jogador não alcança HP de elite tank apenas por atributo.
Build defensiva precisa também de armor, block, skills, comida, companions/pets e execução.
```

## 10. MP Max do jogador

```text
MPMax = BaseMP
      + (Level * MPPerLevel)
      + (Vontade * MPPerWill)
      + (Inteligência * MPPerInt)
      + EquipmentMP
      + SkillMP
      + BuffMP
```

Constantes de teste:

```text
BaseMP = 40
MPPerLevel = 1
MPPerWill = 6
MPPerInt = 2
```

Regra:

```text
Ter MP não torna o personagem mago.
Magic/Arcano, equipamentos e active skills definem uso real de magia.
```

## 11. Stamina Max do jogador

Stamina representa o recurso físico imediato.

Nova fórmula direcional:

```text
StaminaMax = BaseStamina
           + (Level * 0.6)
           + (Constituição * 2.0)
           + (Força * 1.5)
           + (Destreza * 1.0)
           + EquipmentStamina
           + SkillStamina
           + BuffStamina
```

Constantes recomendadas:

```text
BaseStamina = 80
StaminaPerLevel = 0.6
StaminaPerCon = 2.0
StaminaPerStr = 1.5
StaminaPerDex = 1.0
```

Leitura:

```text
Constituição não carrega Stamina sozinha.
Força ajuda ações pesadas.
Destreza ajuda economia de movimento e ações leves.
Level aumenta margem, mas devagar.
Equipamentos e skills dão conforto, não spam infinito.
```

Exemplo level 30, CON 8, FOR 9, DES 6, equipamento +10:

```text
StaminaMax = 80 + 30*0.6 + 8*2.0 + 9*1.5 + 6*1.0 + 10
StaminaMax = 143.5 ≈ 144
```

Exemplo level 50, CON 12, FOR 14, DES 10, equipamento +15:

```text
StaminaMax = 80 + 50*0.6 + 12*2.0 + 14*1.5 + 10*1.0 + 15
StaminaMax = 180
```

Resultado esperado:

```text
Level 50 não deve fazer 20-30 ataques seguidos sem gestão.
O personagem tem mais margem que no início, mas ainda precisa alternar ataque, reposicionamento, block, comida e timing.
```

## 12. Custos de Stamina devem escalar

Stamina Max só funciona se os custos também forem calibrados.

Tabela direcional:

| Ação | Early | Mid | Late/Endgame | Observação |
|---|---:|---:|---:|---|
| Light melee | 10-12 | 13-16 | 16-20 | armas leves custam menos |
| Heavy melee | 20-24 | 26-34 | 34-44 | armas pesadas custam mais |
| Bow shot | 9-12 | 12-16 | 15-20 | charged shot custa mais |
| Magic staff hit físico | 8-10 | 10-14 | 12-16 | magia usa MP à parte |
| Dash | 18-22 | 20-26 | 24-32 | reduzido por Survival/Destreza |
| Dodge | 12-15 | 14-18 | 16-22 | reduzido por Survival/Destreza |
| Block hold | 4-8/s | 6-10/s | 8-14/s | depende de escudo/skill |
| Block impact | 8-18 | 14-28 | 22-42 | depende do golpe inimigo |
| Pickaxe/Axe | 8-14 | 12-22 | 18-32 | reduzido por ferramenta/Força/Crafting |

Regra:

```text
Stamina de level alto aumenta, mas arma/ferramenta de tier alto também custa mais.
Upgrades devem melhorar eficiência, mas não eliminar custo.
```

## 13. Stamina Regen

Nova fórmula direcional:

```text
StaminaRegenOutOfCombat = BaseRegen
                         + (Constituição * 0.15)
                         + (Destreza * 0.20)
                         + SurvivalBonus
                         + EquipmentBonus
                         + FoodBuff
                         - ArmorPenalty

StaminaRegenInCombat = StaminaRegenOutOfCombat * CombatMultiplier
```

Constantes recomendadas:

```text
BaseRegen = 5
CombatMultiplier = 0.35 a 0.50
```

Regras:

```text
Stamina Regen em combate deve ser baixa.
Fome baixa reduz regen.
Cansaço alto reduz regen.
Armadura pesada reduz regen ou aumenta custo.
Survival e comida ajudam, mas com cap.
```

Exemplo level 30, CON 8, DES 6, Survival +1.5, armor -1.2:

```text
OutOfCombat = 5 + 8*0.15 + 6*0.20 + 1.5 - 1.2
OutOfCombat = 7.7/s
InCombat = 2.7 a 3.9/s
```

Resultado esperado:

```text
O jogador recupera bem entre ações e fora de combate.
Durante combate, ainda precisa escolher quando atacar, defender, esquivar ou recuar.
```

---

# PARTE D — Defesa

## 14. Defense e Armor

Defense não deve crescer muito por Constituição.

Nova fórmula direcional:

```text
Defense = BaseDefense
        + Armor
        + (Constituição * 0.75)
        + SkillDefense
        + BuffDefense
```

Regra:

```text
Armor é a principal fonte de mitigação flat.
Constituição dá estrutura, mas não substitui equipamento.
```

## 15. Physical Resistance

```text
PhysicalDamageTaken = IncomingPhysicalDamage
                    * (1 - PhysicalResistanceCapped)
                    - FlatDefenseMitigation
```

Regras:

```text
PhysicalResistance vem mais de equipamento, buffs e skills do que de Constituição pura.
Constituição pode ajudar status físico e posture, mas pouco em resistência percentual direta.
```

## 16. Block

```text
BlockedDamage = IncomingDamage * (1 - BlockPower)

BlockResourceDrain = IncomingImpactPower
                   * (1 - BlockStability)
                   * ShieldOrWeaponMultiplier
```

Influenciado por:

```text
Melee/Guerreiro
Block rank
Guarda Firme
escudo/arma
material do equipamento
Constituição moderada
Força em menor grau contra impacto pesado
capstone Kanthor/Kaand
```

Regra:

```text
Block drena Stamina.
Sem Stamina, Block quebra ou perde eficiência.
Não aplicar Defense/Armor completo depois de Block; usar mitigação flat parcial.
```

## 17. Posture Resistance

```text
PostureDamageTaken = IncomingPostureDamage * (1 - PostureResistance)
```

Influenciado por:

```text
Constituição moderada
Força em menor grau
equipamento pesado
Melee/Guerreiro
Block ativo
```

---

# PARTE E — Ofensivos físicos

## 18. Base Attack e Attack Damage

```text
BaseAttack = BaseAttackValue
           + (Força * BaseAttackPerStr)
           + (Destreza * BaseAttackPerDexSmall)
           + (Level * BaseAttackPerLevel)

AttackDamage = (BaseAttack + WeaponDamage + EquipmentFlatDamage)
             * WeaponScaling
             * (1 + SkillDamageBonus)
             * (1 + BuffDamageBonus)
             * ContextMultiplier
             * EnemyResistanceMultiplier
```

Direção:

```text
Força domina dano físico bruto.
Destreza ajuda armas leves, velocidade e crítico condicional.
Level ajuda pouco.
Arma/material/skill devem importar mais que atributo isolado.
```

## 19. Stagger e Posture Damage

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

# PARTE F — Movimento

## 20. Dash

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

## 21. Dodge

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

# PARTE G — Ofensivos mágicos

## 22. Magic Power

```text
MagicPower = BaseMagicPower
           + (Inteligência * MagicPerInt)
           + (Vontade * MagicPerWill)
           + EquipmentMagic
           + SkillMagic
           + BuffMagic
```

## 23. MP Regen

```text
MPRegenPerSecond = BaseMPRegen
                 + (Vontade * MPRegenPerWill)
                 + SkillMPRegen
                 + EquipmentMPRegen
                 + BuffMPRegen
```

Regra:

```text
MP Regen natural é lenta.
Regeneração rápida depende de efeito explícito.
```

## 24. Healing / Barrier

```text
HealingAmount = BaseHeal
              + (HealingPower * HealScaling)
              + SkillHealBonus
              + EquipmentHealBonus

BarrierHP = BaseBarrier
          + (MagicPower * BarrierScaling)
          + ShieldSkillBonus
          + EquipmentBarrierBonus
```

Regra:

```text
Cura mágica deve ser limitada, cara e com cooldown.
Barreira não deve bloquear tudo.
```

---

# PARTE H — Produção, coleta e economia

## 25. Resource Yield e Gathering Efficiency

```text
ExtraYieldChance = BaseExtraYieldChance
                 + SkillYieldBonus
                 + ToolYieldBonus
                 + BuffYieldBonus
                 + CompanionYieldBonus

HitsRequired = BaseHitsRequired
             * ToolTierMultiplier
             * StrengthObstacleMultiplier
             * SkillEfficiencyMultiplier
```

Diferença:

```text
Força reduz esforço/golpes.
Yield vem de skill, ferramenta, buff, companion ou sistema específico.
```

## 26. Tool Stamina Cost

```text
ToolStaminaCost = BaseToolCost
                * ToolTierCostMultiplier
                * FatigueMultiplier
                * HungerMultiplier
                * (1 - ToolCostReduction)
```

Regra:

```text
Ferramenta de tier alto pode ter custo maior ou igual, mas melhor eficiência.
Upgrade não deve sempre significar custo menor absoluto.
```

---

# PARTE I — Fome, cansaço e ambiente

## 27. Hunger Resistance

```text
HungerDrain = BaseHungerDrain
            * ActivityMultiplier
            * (1 - HungerResistanceCapped)
```

## 28. Fatigue Resistance

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
Survival principalmente
Constituição moderada
comida
sono
buffs
equipamentos
```

Regra:

```text
Fatigue Resistance reduz pressão, mas não elimina necessidade de dormir.
```

---

# PARTE J — Monstros

## 29. Monstros não usam a fórmula do jogador

Monstros têm HP autorado por:

```text
faixa de nível
família
papel no encontro
size class
role multiplier
CON do monstro
variante
boss/elite/common
```

Fórmula conceitual para specs futuras:

```text
EnemyHP = FamilyBaseHP
        + LevelBandHP
        + (CON * FamilyConHPFactor)
        * RoleHPMultiplier
        * VariantMultiplier
```

`FamilyConHPFactor` varia por família:

| Família/Papel | CON -> HP sugerido |
|---|---:|
| Swarm/Tiny | 1-3 por CON |
| Small fast | 2-4 por CON |
| Humanoid comum | 4-6 por CON |
| Beast comum | 5-8 por CON |
| Caster frágil | 2-5 por CON |
| Elite duelist | 6-10 por CON |
| Tank/Construct | 10-16 por CON |
| Huge/mini-boss | 14-24 por CON |
| Boss | autorado/custom |

Regra:

```text
A CON do monstro não espelha a CON do jogador.
Em monstro, CON ajuda HP conforme família e papel.
Em jogador, CON tem multiplicador baixo para evitar build defensiva universal.
```

---

# PARTE K — HUD e save/load

## 30. HUD gameplay

Mostrar sempre/quase sempre:

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

## 31. Save/load

Salvar:

```text
atributos centrais
level
skills/ranks/capstones
equipamentos
buffs persistentes
status persistentes
arquétipos ativos
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

---

# PARTE L — Decisões fechadas

```text
Breath/Fôlego removido como atributo/recurso.
Constituição não é mais atributo defensivo universal.
Player HPPerLevel recomendado = 2.
Player HPPerCon recomendado = 5.
Player Stamina usa Level + Constituição + Força + Destreza.
Player StaminaPerLevel recomendado = 0.6.
Player StaminaPerCon recomendado = 2.0.
Player StaminaPerStr recomendado = 1.5.
Player StaminaPerDex recomendado = 1.0.
Defense vem principalmente de Armor/equipamento.
DefensePerCon recomendado = 0.75.
Stamina costs escalam por tier/tipo/peso da ação.
Stamina Regen em combate deve ser baixa.
Monstros não usam fórmula de HP do jogador.
Monstros têm HP autorado por faixa, família, papel e multiplicador próprio de CON.
```

---

# PARTE M — Pendências

```text
Atualizar teste de mesa com HP 230 e Stamina 144 no exemplo level 30.
Validar custos de ataques por tier de arma.
Validar Stamina Regen em Unity.
Definir Armor/Resistance por família de monstro.
Definir EnemyActionDamage por ação inimiga.
Definir EnemyHP formula apenas para geração/validação de dados, não para runtime obrigatório.
Definir caps finais de Block Power, Block Stability e Stamina Regen.
