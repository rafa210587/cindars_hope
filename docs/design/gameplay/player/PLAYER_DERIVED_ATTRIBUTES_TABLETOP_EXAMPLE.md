# Cindar's Hope — Player Derived Attributes Tabletop Example

> **Status:** documento complementar de validação de mesa  
> **Local:** `docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_TABLETOP_EXAMPLE.md`  
> **Complementa:** `docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md`  
> **Função:** demonstrar, com um personagem exemplo, como atributos centrais viram atributos derivados, como esses valores se comportam em combate e quais riscos de balanceamento aparecem antes de virar spec implementável.  
> **Não é spec implementável.** Os números abaixo são exemplos de mesa para validar escala relativa, não valores finais.

---

## 0. Regra deste teste

Este documento usa constantes provisórias para simular um personagem.

Objetivo:

```text
ver se os atributos derivados ficam legíveis
ver se o personagem fica forte demais
ver se o personagem consegue enfrentar uma criatura compatível
ver se o combate exige uso de recursos
ver se HP, Stamina, Breath, Block, Dodge, Crit e Posture fazem sentido juntos
identificar riscos antes de criar fórmulas finais em spec
```

Não fechar aqui:

```text
valores finais de fórmula
curva final de level
valores finais de armas
valores finais de monstros
TTK final
cooldowns finais
caps finais
```

---

# PARTE A — Personagem exemplo

## 1. Identidade do personagem

```text
Nome de teste: Brann Ferrocinza
Raça: Anão de Khaz Baruk
Level: 30
Função: guerreiro/minerador de caverna
Build: Melee principal, Survival secundário, Crafting leve
Contexto: mid game avançado, antes de capstones finais
```

Fantasia:

```text
Personagem resistente, bom de block, dano melee consistente, mineração eficiente, pouca magia e mobilidade razoável.
Não é glass cannon.
Não é mago.
Não é build social.
```

## 2. Atributos centrais

Regra usada:

```text
Level 1 começa com 1 em cada atributo.
A cada level: +1 ponto de atributo.
Level 30 = 6 pontos iniciais + 29 pontos distribuídos = 35 pontos totais.
```

Distribuição:

| Atributo central | Valor | Justificativa |
|---|---:|---|
| Força | 9 | dano físico, stagger, mineração, arma melee |
| Constituição | 8 | HP, Stamina, Breath, resistência, long runs |
| Destreza | 6 | dodge, crit condicional, attack speed moderado |
| Inteligência | 4 | crafting básico, leitura de sistemas, baixa magia técnica |
| Vontade | 5 | MP moderado, resistência mental, Breath secundário |
| Carisma | 3 | social baixo, sem foco em companions/loja |

Leitura:

```text
A build está bem focada em combate físico e sobrevivência.
O personagem não deve ter dano mágico alto.
O personagem não deve ter social/economia forte.
```

---

# PARTE B — Equipamentos e skills do exemplo

## 3. Equipamentos usados

| Slot | Item de teste | Bônus usado no teste |
|---|---|---|
| Arma | Espada de Aço | WeaponDamage +34, intervalo base 0.75s, crit +2%, stagger +10 |
| Escudo | Escudo de Ferro | Block Power +10%, Block Stability +8%, Breath Recovery -2 |
| Armadura | Cota Reforçada | Armor +45, HP +20, Movement Speed -4%, Breath Recovery -3 |
| Acessório 1 | Anel do Mineiro | Mining Efficiency +6%, Stamina Max +10 |
| Acessório 2 | Amuleto Simples | Fear Resistance +5%, MP +8 |

Observação:

```text
Equipamento de aço/ferro deve ser forte, mas não endgame.
A armadura ajuda muito na defesa, mas cobra custo em movimento/fôlego.
```

## 4. Skills usadas

SkillPoints estimados no level 30:

```text
1 SkillPoint a cada 2 níveis ≈ 15 SkillPoints.
```

Distribuição de teste:

| Árvore | Pontos | Skills relevantes |
|---|---:|---|
| Melee / Guerreiro | 10 | Treinamento Marcial rank 3, Ataque Pesado rank 3, Block rank 3, Guarda Firme rank 2, Corte Amplo rank 1 |
| Survival / Sobrevivente | 4 | Ritmo de Jornada rank 1, Reflexo de Esquiva rank 1, Passo de Impulso rank 1, Respiração Controlada rank 1 |
| Crafting / Produção | 1 | Prospector de Superfície rank 1 |
| Ranged / Caçador | 0 | sem investimento |
| Magic / Arcano | 0 | sem investimento |

Leitura:

```text
O personagem tem defesa ativa e dano melee decente.
Dash já existe por tutorial/progressão, mas só tem melhoria leve de Survival.
Não tem HP Regen ainda porque Regeneração Natural é Tier 3 de Survival e não cabe nessa build.
Não tem capstone porque Tier 5 exige 26 pontos na árvore.
```

---

# PARTE C — Constantes provisórias do teste

## 5. Constantes usadas

Estas constantes existem só para o teste de mesa.

```text
BaseHP = 100
HPPerLevel = 6
HPPerCon = 15

BaseMP = 40
MPPerLevel = 1
MPPerWill = 6
MPPerInt = 2

BaseStamina = 100
StaminaPerLevel = 3
StaminaPerCon = 8
StaminaPerStrSmall = 3

BaseBreath = 50
BreathPerCon = 6
BreathPerDex = 4
BreathPerWillSmall = 2

BaseAttackValue = 8
BaseAttackPerStr = 3
BaseAttackPerDexSmall = 1
BaseAttackPerLevel = 0.5

BaseCritChance = 5%
BaseCritMultiplier = 1.5x

BaseMovementSpeed = 1.00
BaseDodgeIFrames = 0.18s
BaseDashDistance = 2.25 tiles
BaseDashCooldown = 1.20s
BaseDashCost = 18 Stamina + 10 Breath
BaseDodgeCost = 12 Stamina + 8 Breath
```

Regra:

```text
Se esses valores finais parecerem altos/baixos no Unity, ajustar a constante base antes de mexer nos atributos.
```

---

# PARTE D — Atributos derivados calculados

## 6. Recursos principais

### HP Max

```text
HPMax = BaseHP + Level*6 + Constituição*15 + EquipmentHP
HPMax = 100 + 30*6 + 8*15 + 20
HPMax = 420
```

Leitura:

```text
420 HP no level 30 parece alto, mas aceitável para build anã defensiva.
Se inimigos equivalentes baterem 40-70 após mitigação, ele aguenta 6-10 hits.
```

### MP Max

```text
MPMax = BaseMP + Level*1 + Vontade*6 + Inteligência*2 + EquipmentMP
MPMax = 40 + 30 + 5*6 + 4*2 + 8
MPMax = 116
```

Leitura:

```text
MP existe, mas sem investimento em Magic/Arcano o personagem não usa bem.
MP não significa poder mágico alto.
```

### Stamina Max

```text
StaminaMax = BaseStamina + Level*3 + Constituição*8 + Força*3 + EquipmentStamina
StaminaMax = 100 + 30*3 + 8*8 + 9*3 + 10
StaminaMax = 291
```

Leitura:

```text
291 Stamina permite lutar, minerar e bloquear, mas não permite spam infinito.
Block, Dodge, Dash e Ataque Pesado ainda competem pelo mesmo recurso.
```

### Breath Max

```text
BreathMax = BaseBreath + Constituição*6 + Destreza*4 + Vontade*2 + SkillBreath + EquipmentPenalty
BreathMax = 50 + 8*6 + 6*4 + 5*2 + 8 - 5
BreathMax = 135
```

Leitura:

```text
135 Breath é bom para combate sustentado, mas a armadura reduz recuperação.
```

### Regenerações

| Derivado | Valor de teste | Leitura |
|---|---:|---|
| HP Regen | 0/s em combate; 0/s fora de combate | Sem Regeneração Natural, sem Fonte, sem comida |
| MP Regen | ~1.0 MP/s fora de combate; menor em combate | Vontade 5, sem Fluxo Lento |
| Stamina Regen | 18/s normal; 12/s cansado; 8/s com fome baixa | Depende muito de fome/cansaço |
| Breath Recovery | 16/s base, -5 por armadura/escudo = 11/s | Penalidade visível por equipamento pesado |

Risco identificado:

```text
Se Stamina Regen ficar alta demais, Block/Dodge/Dash deixam de competir.
Manter regen menor durante combate e menor ainda com fome/cansaço alto.
```

---

## 7. Ofensivos físicos

### Base Attack

```text
BaseAttack = 8 + Força*3 + Destreza*1 + Level*0.5
BaseAttack = 8 + 9*3 + 6 + 15
BaseAttack = 56
```

### Attack Damage com Espada de Aço

```text
AttackDamage = (BaseAttack + WeaponDamage) * (1 + SkillDamageBonus)
AttackDamage = (56 + 34) * 1.06
AttackDamage = 95.4
```

Arredondamento de mesa:

```text
Attack Damage exibido: 95
```

### Melee Damage

```text
MeleeDamage = AttackDamage * MeleeWeaponMultiplier
MeleeDamage = 95 * 1.00
MeleeDamage = 95
```

### Ranged Damage

```text
RangedDamage = baixo/não relevante
Sem arma ranged e sem Ranged/Caçador.
```

### Tool Attack Damage

```text
ToolAttackDamage = AttackDamage * 0.65 se usar ferramenta ofensivamente
ToolAttackDamage = 95 * 0.65 ≈ 62
```

Regra validada:

```text
Ferramenta machuca, mas não supera arma dedicada.
```

### Attack Speed

```text
AttackInterval = BaseWeaponInterval * (1 - DexBonus) * WeaponWeightMultiplier
AttackInterval = 0.75s * 0.95 * 1.00
AttackInterval ≈ 0.71s
```

Leitura:

```text
Ataque é responsivo, mas não rápido demais.
```

### Charge Speed

```text
BaseChargeTime = 1.20s
ChargeSpeedBonus de Ataque Pesado rank 3: 10%
ChargeTime = 1.20 * 0.90 = 1.08s
```

### Crit Chance

```text
CritChance normal = Base 5% + DestrezaBonus 3% + WeaponCrit 2%
CritChance normal = 10%
```

Contra alvo vulnerável/critical window:

```text
CritChance contextual = 10% + Lâmina de Abertura ausente + critical window bonus 20%
CritChance contextual = 30%
```

Observação:

```text
Como o personagem não comprou Lâmina de Abertura, o crítico condicional não está absurdo.
Se ele comprar Lâmina depois, pode ir para 40-50% em janela, ainda aceitável.
```

### Crit Damage

```text
CritDamage normal = 1.5x
CriticalHit = 95 * 1.5 = 142
```

### Stagger Power

```text
StaggerPower = BaseStagger + WeaponStagger + Força*2 + SkillBonus
StaggerPower = 10 + 10 + 9*2 + 15
StaggerPower = 53
```

### Posture Damage

Ataque normal:

```text
PostureDamageNormal = StaggerPower * 0.75
PostureDamageNormal = 53 * 0.75 ≈ 40
```

Ataque carregado:

```text
PostureDamageCharged = StaggerPower * 1.50
PostureDamageCharged = 53 * 1.50 ≈ 80
```

Leitura:

```text
Ataques carregados realmente servem para quebrar postura.
Ataque normal ainda contribui, mas bem menos.
```

### Armor Penetration e Knockback

| Derivado | Valor de teste | Leitura |
|---|---:|---|
| Armor Penetration | 0% | Espada de Aço comum, sem óleo, sem skill específica |
| Knockback Power | 18 | Moderado; não empurra elite pesado facilmente |

---

## 8. Ofensivos mágicos

| Derivado | Valor de teste | Leitura |
|---|---:|---|
| Magic Power | 34 | Baixo/médio; sem Magic/Arcano |
| Elemental Power | 34 base | Sem afinidade elemental |
| Fire Power | 34 | Sem bônus |
| Ice Power | 34 | Sem bônus |
| Lightning Power | 34 | Sem bônus |
| Water/Nature Power | 34 | Sem bônus |
| Arcane Power | 34 | Sem Foco Arcano |
| Spiritual Power | 38 | Vontade ajuda um pouco, mas sem build |
| Corruption Power | 0 | Futuro/controlado, não disponível |
| Healing Power | 30 | Não suficiente para cura relevante sem skill |
| Shield/Barrier Power | 28 | Não usa Selo de Proteção |
| Cast Speed | 0% bônus | Sem Magic/Arcano |
| MP Cost Reduction | 0% | Sem Canalização Serena |
| Magic Crit Chance | 0-3% | Não relevante sem Senya/itens |
| Magic Crit Damage | 1.3x se habilitado | Não relevante neste personagem |
| Status Application Power | baixo | Não é build de status |

Leitura:

```text
O personagem tem MP, mas não tem kit mágico.
Isso valida que MP Max sozinho não torna o personagem mago.
```

---

## 9. Defensivos

### Defense e Armor

```text
Armor = 45
Defense = BaseDefense + Constituição*2 + Armor
Defense = 5 + 8*2 + 45
Defense = 66
```

Fórmula de mitigação usada no teste:

```text
DamageTaken = IncomingDamage * (1 - PhysicalResistance) - (Defense * 0.30)
Dano mínimo sempre >= 1
```

### Resistências

| Derivado | Valor de teste | Fonte |
|---|---:|---|
| Physical Resistance | 12% | Constituição + armadura |
| Fire Resistance | 0% | sem item |
| Ice Resistance | 5% | anão/equipamento leve de teste, se mantido |
| Lightning Resistance | 0% | sem item |
| Water/Nature Resistance | 0% | sem item |
| Arcane Resistance | 0% | sem item |
| Shadow/Nyx Resistance | 5% | Vontade + amuleto simples |
| Blackstone/Corruption Resistance | 0-5% | baixa; sem Magic/Arcano |
| Poison Resistance | 8% | Constituição/anão |
| Bleed Resistance | 6% | Constituição/armadura |
| Burn Resistance | 0% | sem item |
| Chill Resistance | 5% | anão/equipamento leve |
| Fear Resistance | 10% | Vontade + amuleto |
| Confusion Resistance | 4% | Vontade |
| Stun Resistance | 6% | Constituição |

### Posture e Knockback Resistance

| Derivado | Valor de teste | Leitura |
|---|---:|---|
| Posture Resistance | 18% | bom, mas não imune |
| Knockback Resistance | 15% | inimigos grandes ainda empurram |

### Block

```text
BlockPower = Block rank 3 49% + Escudo 10%
BlockPower = 59%
```

```text
BlockStability = Guarda Firme rank 2 12% + Escudo 8% + ConstituiçãoBonus 5%
BlockStability = 25%
```

```text
BlockRecovery = Base 0.45s * (1 - 0.10)
BlockRecovery ≈ 0.40s
```

Leitura:

```text
Block é forte, mas não gratuito.
Se aplicar Defense completa depois do Block, pode ficar forte demais.
Recomendação do teste: para golpes bloqueados, aplicar BlockPower primeiro e depois mitigação física reduzida, não mitigação completa duplicada.
```

---

## 10. Movimento e controle

| Derivado | Valor de teste | Cálculo/leitura |
|---|---:|---|
| Movement Speed | 0.96x | armadura -4%, sem build de velocidade |
| Dash Distance | 2.45 tiles | base 2.25 + Passo de Impulso rank 1 |
| Dash Cooldown | 1.10s | base 1.20s com redução leve |
| Dash Cost | 16 Stamina + 9 Breath | redução leve por Survival |
| Dodge IFrames | 0.20s | base 0.18s + Reflexo rank 1 |
| Dodge Recovery | 0.32s | leve melhoria por Destreza/skill |
| Dodge Cost | 11 Stamina + 7 Breath | redução pequena |
| Collision Recovery | normal | sem investimento específico |

Leitura:

```text
Movimento não fica rápido demais.
Dash ajuda reposicionar, mas custa recurso.
Dodge tem janela curta; exige timing.
```

---

## 11. Produção, coleta e economia

| Derivado | Valor de teste | Leitura |
|---|---:|---|
| Resource Yield Bonus | +3% mineração comum | Prospector rank 1, baixo |
| Gathering Efficiency | +moderado contra rocha | Força 9 + ferramenta |
| Tool Stamina Cost Reduction | 0-5% | sem Mãos de Lavrador |
| Tool Action Speed | normal | sem skill forte |
| Mining Efficiency | +6% anel + Força alta + Prospector | bom para build mineradora |
| Woodcutting Efficiency | moderado | Força alta, sem Lenhador |
| Farming Efficiency | baixo | sem foco agrícola |
| Watering Efficiency | baixo | sem skill/automação |
| Fishing Efficiency | baixo/médio | Destreza 6, sem skill |
| Crafting Efficiency | baixo | Inteligência 4, sem Oficina |
| Construction Cost Reduction | 0% | sem Oficina Organizada |
| Craft Quality Bonus | baixo | sem Crafting |
| Cooking Quality Bonus | baixo | sem Cozinha |
| Potion/Consumable Potency | baixo | sem Alquimia |
| Loot Bonus | 0% | sem Survival Tier 4/Ranged |
| Gold Bonus | 0% | sem Saqueador |
| Treasure Quality Bonus | 0% | sem Faro de Tesouro |

Leitura:

```text
O personagem é bom em quebrar rochas e razoável em mineração, mas não vira mestre de produção.
Isso preserva espaço para builds Crafting/Survival dedicadas.
```

---

## 12. Condição, social e companions

| Derivado | Valor de teste | Leitura |
|---|---:|---|
| Hunger Resistance | 0% | sem Estômago Forte |
| Fatigue Resistance | +4% | Ritmo de Jornada rank 1 |
| Sleep Recovery Bonus | normal | sem cama/buff especial |
| Environmental Endurance | baixo/médio | Con/Vontade ok, sem Resistência Ambiental |
| Social Influence | baixo | Carisma 3 |
| Vendor Price Modifier | 0-2% | baixo, sem foco social |
| Relationship Gain Modifier | baixo | sem foco social |
| Companion Command | futuro/baixo | não expor ainda |
| Companion Bond Effect | futuro/baixo | não expor ainda |
| Pet Bond Effect | futuro/baixo | não expor ainda |
| Pet Combat Support | futuro/baixo | não expor ainda |

---

# PARTE E — Resumo da ficha derivada

## 13. Painel resumido do personagem

```text
Brann Ferrocinza — Level 30
Raça: Anão
Build: Melee/Sobrevivência leve

HP Max: 420
MP Max: 116
Stamina Max: 291
Breath Max: 135

Attack Damage: 95
Melee Damage: 95
Ranged Damage: baixo
Magic Power: 34
Healing Power: baixo

Attack Speed: 0.71s por ataque
Crit Chance: 10% normal / 30% em critical window
Crit Damage: 1.5x
Stagger Power: 53
Posture Damage: 40 normal / 80 carregado

Defense: 66
Armor: 45
Physical Resistance: 12%
Posture Resistance: 18%
Block Power: 59%
Block Stability: 25%

Movement Speed: 0.96x
Dash: 2.45 tiles / 1.10s cooldown / 16 Stamina + 9 Breath
Dodge: 0.20s i-frame / 11 Stamina + 7 Breath

HP Regen: 0
MP Regen: lenta
Fatigue Resistance: +4%
Mining Efficiency: boa
Social Influence: baixo
```

## 14. Veredito inicial da ficha

```text
O personagem parece forte, mas não quebrado.
Ele é durável e consistente em melee.
Ele não tem sustain automático de HP.
Ele não tem magia relevante.
Ele não tem economia/social forte.
Ele depende de Stamina/Breath para Block, Dodge, Dash e Ataque Pesado.
```

Risco principal:

```text
Block + Defense pode ficar forte demais se ambos mitigarem 100% em sequência.
```

Correção recomendada:

```text
Em golpes bloqueados:
1. aplicar BlockPower
2. aplicar resistência física
3. aplicar apenas parte da mitigação flat de Defense/Armor

Não aplicar a mitigação completa duas vezes.
```

---

# PARTE F — Criatura de teste

## 15. Criatura: Ghul de Pedra Negra

```text
Nome: Ghul de Pedra Negra
Faixa: caverna mid game avançada
Level de teste: 28
Função: elite menor / inimigo perigoso isolado
```

Stats de teste:

| Stat | Valor |
|---|---:|
| HP | 620 |
| Armor | 35 |
| Physical Resistance | 15% |
| Shadow/Nyx Resistance | 30% |
| Silver Vulnerability | +25% dano recebido de Prata |
| Spiritual Vulnerability | +20% dano recebido espiritual/purificação |
| Posture Max | 160 |
| Posture Resistance | 10% |
| Attack Damage comum | 72 |
| Heavy Attack Damage | 105 |
| Posture Damage causado | 35 comum / 60 pesado |
| Movement | médio/lento |

Comportamento:

```text
aproxima em zigue-zague curto
faz swipe comum se colado
faz heavy claw telegraphado a cada X segundos
fica vulnerável por 1.2s se errar heavy claw
fica staggered quando Posture zera
resiste a Shadow/Nyx
é vulnerável a Prata e espiritual
```

Critical window:

```text
Depois de errar heavy claw.
Depois de posture break.
Durante recovery de grito/canalização, se houver variante.
```

---

# PARTE G — Simulação de combate

## 16. Fórmulas usadas no combate

Dano do jogador contra o Ghul:

```text
RawHit = 95
EnemyPhysicalResistance = 15%
EnemyArmorMitigation = Armor * 0.20 = 35 * 0.20 = 7

NormalHit = RawHit * 0.85 - 7
NormalHit = 95 * 0.85 - 7
NormalHit ≈ 74
```

Ataque carregado:

```text
ChargedRaw = 95 * 1.25 = 119
ChargedHit = 119 * 0.85 - 7
ChargedHit ≈ 94
```

Crítico em janela:

```text
CriticalNormal = NormalHit * 1.5
CriticalNormal = 74 * 1.5 = 111

CriticalCharged = ChargedHit * 1.5
CriticalCharged = 94 * 1.5 = 141
```

Posture:

```text
NormalPostureDamage = 40 * 0.90 = 36
ChargedPostureDamage = 80 * 0.90 = 72
Posture Max do Ghul = 160
```

Dano recebido pelo jogador sem block:

```text
Incoming = 72
PhysicalResistance = 12%
DefenseFlat = Defense * 0.30 = 66 * 0.30 = 19.8

DamageTaken = 72 * 0.88 - 19.8
DamageTaken ≈ 44
```

Dano recebido com block:

```text
Incoming = 72
BlockPower = 59%
BlockedIncoming = 72 * 0.41 = 29.5
PhysicalResistance parcial = 12%
DefenseFlat parcial = Defense * 0.15 = 9.9

BlockedDamageTaken = 29.5 * 0.88 - 9.9
BlockedDamageTaken ≈ 16
```

Heavy attack sem block:

```text
Incoming = 105
DamageTaken = 105 * 0.88 - 19.8
DamageTaken ≈ 73
```

Heavy attack com block:

```text
BlockedIncoming = 105 * 0.41 = 43
BlockedDamageTaken = 43 * 0.88 - 9.9
BlockedDamageTaken ≈ 28
```

Leitura:

```text
Block reduz muito dano, mas consome Stamina/Breath.
Dodge evita dano, mas exige timing e também consome recurso.
Tomar heavy sem defesa dói bastante, mas não mata de uma vez.
```

---

## 17. Simulação em rounds de mesa

Estado inicial:

```text
Brann HP: 420
Brann Stamina: 291
Brann Breath: 135
Ghul HP: 620
Ghul Posture: 160
```

| Round | Ação do jogador | Resultado | Ação do Ghul | Estado |
|---:|---|---|---|---|
| 1 | Ataque normal | Ghul -74 HP, -36 posture | Swipe comum acerta | Brann -44 HP |
| 2 | Ataque carregado | Ghul -94 HP, -72 posture | Heavy claw telegraphado | Brann usa Dodge, evita dano, gasta recurso |
| 3 | Ataque carregado | Ghul -94 HP, -72 posture; posture quebra | Ghul staggered | abre critical window |
| 4 | Ataque normal em janela | crítico: Ghul -111 HP | Ghul recuperando | sem dano recebido |
| 5 | Corte Amplo/ataque normal | Ghul -74 HP | Swipe comum bloqueado | Brann -16 HP, gasta Stamina/Breath |
| 6 | Ataque normal | Ghul -74 HP | Heavy claw começa | Brann reposiciona com Dash |
| 7 | Ataque carregado em recovery | Ghul -94 HP | Ghul falha heavy | critical window curta |

Totais aproximados após round 7:

```text
Dano causado ao Ghul:
74 + 94 + 94 + 111 + 74 + 74 + 94 = 615
Ghul fica praticamente morto.

Dano recebido por Brann:
44 + 16 = 60
Brann HP restante: 360
```

Custo aproximado de recursos:

```text
2 ataques carregados relevantes: -50 a -70 Stamina total
1 Dodge: -11 Stamina / -7 Breath
1 Block: -recurso conforme impacto, ~15-25 Stamina/Breath equivalente
1 Dash: -16 Stamina / -9 Breath
```

Leitura:

```text
Se o jogador lê telegraphs e usa Dodge/Block/Dash bem, vence com folga.
Se o jogador erra heavy attacks e não defende, toma 44-73 por golpe e pode perder rápido.
O inimigo isolado não é ameaça extrema para uma build defensiva bem equipada.
Para ser elite real, ele precisa vir com pack, hazard, arena ruim ou variante mais agressiva.
```

---

## 18. Simulação alternativa: jogador joga mal

Cenário:

```text
Brann não usa Dodge no heavy.
Brann tenta spammar Ataque Pesado.
Brann bloqueia tarde.
```

Resultado provável:

```text
Round 1: -44 HP
Round 2: -73 HP
Round 3: -44 HP
Round 4: -73 HP
Round 5: -44 HP

Dano recebido em 5 rounds: ~278
HP restante: 142
```

Leitura:

```text
O personagem sobrevive alguns erros, mas não pode ignorar mecânica.
Isso é adequado para build defensiva mid game.
```

---

# PARTE H — Validação de balanceamento

## 19. O que parece adequado

```text
HP 420 não parece absurdo se inimigos equivalentes causam 40-70 de dano real.
Attack Damage 95 gera TTK aceitável contra elite de 620 HP.
Crit normal 10% é baixo o bastante para não dominar.
Crit em janela 30% valoriza leitura sem garantir sempre.
Posture break exige 2-3 ataques carregados, o que cria risco.
Block 59% é forte, mas consome recurso e precisa de ajuste contra mitigação dupla.
Dash/Dodge competem com Stamina/Breath e não parecem gratuitos.
Sem HP Regen, o personagem ainda precisa de comida/poção/Fonte.
```

## 20. O que pode ficar forte demais

```text
Block Power + Defense completa pode reduzir dano demais.
Stamina Regen alta demais pode permitir block/dodge/dash infinito.
Posture Damage alto demais pode trivializar elites.
Armor flat muito alto pode zerar dano de inimigos pequenos.
Critical window com crítico automático sempre pode explodir TTK se Crit Damage subir demais.
```

## 21. Ajustes recomendados antes de spec

```text
1. Definir que golpe bloqueado usa mitigação flat reduzida de Defense/Armor.
2. Definir Stamina Regen menor durante combate.
3. Definir que Breath Recovery cai durante block/corrida/dash spam.
4. Definir que elites/bosses têm Posture Resistance maior.
5. Definir que Critical Window garante crítico automático apenas em casos específicos; caso contrário, adiciona chance alta de crítico.
6. Definir que Armor flat nunca reduz dano abaixo de uma fração mínima do ataque.
7. Definir que HP Regen passiva não funciona em combate, salvo exceção explícita.
```

---

# PARTE I — Exemplo com capstone futuro

## 22. Mesmo personagem em endgame parcial

Para testar capstones, simular uma versão futura:

```text
Level: 60
SkillPoints: ~30
Melee points: 26+
Capstone escolhido: Voto do Aço Profundo de Kanthor ou Kaand
```

### Kanthor

Efeito esperado:

```text
mais estabilidade
mais block
mais resistência a stagger
recuperação leve em critical window
```

Risco:

```text
Pode virar tanque demais se Block + Defense + cura leve empilharem sem cooldown.
```

Controle recomendado:

```text
cura de 3% HP máximo deve ter cooldown interno por encontro ou por X segundos.
```

### Kaand

Efeito esperado:

```text
mais dano
mais crit damage
mais stagger
menos segurança durante efeito
```

Risco:

```text
Pode explodir boss se acumular com critical window garantida e ataque carregado.
```

Controle recomendado:

```text
bonus de Kaand deve durar pouco, exigir postura quebrada/crítico e reduzir Block Power durante a janela.
```

---

# PARTE J — Conclusão do teste

## 23. Veredito

```text
A estrutura de atributos derivados faz sentido.
A build de exemplo fica forte, mas não invencível.
O personagem tem identidade clara: melee defensivo/minerador.
O combate contra uma elite compatível é vencível com boa execução.
O combate pune erro sem virar morte instantânea.
O maior risco matemático é mitigação defensiva empilhada.
O segundo maior risco é regeneração de Stamina/Breath alta demais.
O terceiro risco é posture/critical window reduzindo demais o TTK.
```

## 24. Próximo teste de mesa recomendado

Criar mais três fichas comparativas:

```text
1. Ranged/Sobrevivente leve — baixa defesa, alto controle e marcação.
2. Magic/Anya — cura/suporte/MP, baixo dano físico.
3. Crafting/Produção — economia/fazenda/caverna com baixo combate direto.
```

Objetivo:

```text
comparar TTK
comparar dano recebido
comparar consumo de Stamina/Breath/MP
comparar sobrevivência sem companion/pet
comparar impacto de loot/economia
validar se Melee não está obrigatório
validar se Survival não está obrigatório
validar se Magic não cura demais
validar se Crafting não quebra economia
```
