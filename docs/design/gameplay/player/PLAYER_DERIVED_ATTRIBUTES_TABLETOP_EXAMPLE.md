# Cindar's Hope — Player Derived Attributes Tabletop Example

> **Status:** documento complementar de validação de mesa  
> **Local:** `docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_TABLETOP_EXAMPLE.md`  
> **Complementa:** `docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md`  
> **Função:** demonstrar, com personagem exemplo, como atributos centrais viram atributos derivados, como esses valores se comportam contra monstros reais da caverna, e validar a régua sem Breath/Fôlego, com Constituição menos dominante, Stamina controlada e custos de ação altos.  
> **Não é spec implementável.** Os números abaixo são exemplos de mesa para validar escala relativa, não valores finais.

---

## 0. Veredito desta revisão

A nova régua usa Stamina como recurso decisivo.

Mudanças principais:

```text
Light melee com Espada de Aço: 25 Stamina.
Heavy melee com Espada de Aço: 40 Stamina.
Dash: 40 Stamina.
Dodge: 40 Stamina.
Block hold: 18 Stamina/s.
Block impact: proporcional ao dano pós-armadura/mitigação em relação ao HP máximo do jogador.
```

Consequência:

```text
Com 144 Stamina no level 30, o jogador não pode spammar ataque, dodge, dash e block.
Cada ação defensiva forte consome quase 28% da Stamina total.
Block segurado por 2s consome 36 Stamina antes mesmo do impacto.
O impacto do Block respeita Armor/Defense: golpes bem absorvidos drenam menos Stamina.
O jogo passa a exigir ritmo, janela, posicionamento, comida, skill e companion/pet.
```

---

# PARTE A — Modelo sem Breath e com Constituição redistribuída

## 1. Stamina

Stamina representa energia física imediata.

Gasta em:

```text
ataques físicos
ataques carregados
Block segurado ou impacto bloqueado
Dodge
Dash
corrida
uso de ferramentas
mineração
corte de madeira
plantio/rega/colheita quando aplicável
pesca
```

Regra:

```text
Stamina deve sempre exigir organização.
Level alto não deve permitir spam infinito.
Dash e Dodge devem ser escolhas táticas fortes, não movimento gratuito.
```

## 2. Cansaço

Cansaço representa desgaste acumulado.

Aumenta com:

```text
tempo acordado
gasto de Stamina
gasto de Stamina com fome baixa
combate prolongado
long runs na caverna
ambiente hostil
status negativos
```

É reduzido/mitigado por:

```text
Survival/Sobrevivente principalmente
Constituição em menor grau
comida
sono
descanso
equipamentos específicos
buffs específicos
```

## 3. Distribuição de papéis

| Função | Fonte principal | Fonte secundária |
|---|---|---|
| HP Max | Level + Constituição baixa | equipamento, comida, skill |
| Stamina Max | base + level baixo | Constituição, Força, Destreza, equipamento, skill |
| Stamina Regen | base + Destreza/Constituição leves | Survival, comida, equipamento |
| Block Stability | Melee + escudo | Constituição moderada, Força leve |
| Posture Resistance | equipamento + Melee | Constituição moderada |
| Dash/Dodge eficiência | Destreza + Survival | equipamento/status |
| Cansaço | sistema próprio | mitigado por Survival + CON leve |

---

# PARTE B — Personagem de teste

## 4. Identidade

```text
Nome de teste: Brann Ferrocinza
Raça: Anão de Khaz Baruk
Level: 30
Função: guerreiro/minerador de caverna
Build: Melee principal, Survival secundário, Crafting leve
Contexto: faixa 26-40 da caverna, Caverna de Gelo
```

Atributos centrais:

| Atributo | Valor | Papel |
|---|---:|---|
| Força | 9 | dano físico, stagger, mineração, ações pesadas |
| Constituição | 8 | HP moderado, Stamina moderada, estabilidade |
| Destreza | 6 | timing, dodge, dash, crit condicional |
| Inteligência | 4 | crafting básico, baixa magia técnica |
| Vontade | 5 | MP, resistência mental/espiritual |
| Carisma | 3 | social baixo |

SkillPoints estimados no level 30:

```text
~15 SkillPoints.
```

Distribuição:

| Árvore | Pontos | Skills relevantes |
|---|---:|---|
| Melee / Guerreiro | 10 | Treinamento Marcial r3, Ataque Pesado r3, Block r3, Guarda Firme r2, Corte Amplo r1 |
| Survival / Sobrevivente | 4 | Ritmo de Jornada r1, Reflexo de Esquiva r1, Passo de Impulso r1, Ritmo Controlado r1 |
| Crafting / Produção | 1 | Prospector de Superfície r1 |
| Ranged / Caçador | 0 | sem investimento |
| Magic / Arcano | 0 | sem investimento |

Equipamento:

| Slot | Item | Bônus de teste |
|---|---|---|
| Arma | Espada de Aço | WeaponDamage +34, crit +2%, stagger +10 |
| Escudo | Escudo de Ferro | Block Power +10%, Block Stability +8% |
| Armadura | Cota Reforçada | Armor +45, HP +20, Movement -4%, Stamina Regen -8% |
| Acessório 1 | Anel do Mineiro | Mining Efficiency +6%, Stamina +10 |
| Acessório 2 | Amuleto Simples | Fear Resistance +5%, MP +8 |

---

# PARTE C — Constantes de mesa

## 5. Constantes revisadas

```text
BaseHP = 110
HPPerLevel = 2
HPPerCon = 5

BaseMP = 40
MPPerLevel = 1
MPPerWill = 6
MPPerInt = 2

BaseStamina = 80
StaminaPerLevel = 0.6
StaminaPerCon = 2.0
StaminaPerStr = 1.5
StaminaPerDex = 1.0

BaseAttackValue = 8
BaseAttackPerStr = 3
BaseAttackPerDexSmall = 1
BaseAttackPerLevel = 0.5

BaseCritChance = 5%
BaseCritMultiplier = 1.5x
BaseDodgeIFrames = 0.18s
BaseDashDistance = 2.25 tiles
```

Custos de ação para este teste:

```text
Light melee com Espada de Aço: 25 Stamina
Heavy melee com Espada de Aço: 40 Stamina
Dash: 40 Stamina
Dodge: 40 Stamina
Block hold: 18 Stamina/s
Block impact comum: proporcional ao dano pós-armadura / HP máximo do jogador
Block impact elite: proporcional ao dano pós-armadura / HP máximo do jogador
```

---

# PARTE D — Atributos derivados revisados

## 6. HP Max

```text
HPMax = BaseHP + Level*2 + Constituição*5 + EquipmentHP
HPMax = 110 + 30*2 + 8*5 + 20
HPMax = 230
```

Leitura:

```text
230 HP fica abaixo de elites da faixa 26-40 e dentro da região alta de comuns.
O jogador não vira elite tank só por Constituição.
Sobrevivência vem de armor, block, dodge, comida, companion/pet e execução.
```

## 7. MP Max

```text
MPMax = BaseMP + Level*1 + Vontade*6 + Inteligência*2 + EquipmentMP
MPMax = 40 + 30 + 5*6 + 4*2 + 8
MPMax = 116
```

Leitura:

```text
MP existe, mas sem Magic/Arcano não vira poder mágico relevante.
```

## 8. Stamina Max

```text
StaminaMax = BaseStamina + Level*0.6 + Constituição*2.0 + Força*1.5 + Destreza*1.0 + EquipmentStamina
StaminaMax = 80 + 30*0.6 + 8*2 + 9*1.5 + 6*1 + 10
StaminaMax = 143.5 ≈ 144
```

Leitura:

```text
144 Stamina permite sequência curta de ações.
Com espada de aço, o jogador consegue cerca de 5 ataques leves sem contar regen.
Com Dash/Dodge a 40, cada evasão consome quase 28% da barra.
```

## 9. Stamina Regen

```text
StaminaRegen fora de combate = BaseRegen + Constituição*0.15 + Destreza*0.20 + SurvivalBonus - ArmorPenalty
StaminaRegen fora de combate = 5 + 8*0.15 + 6*0.20 + 1.5 - 1.2
StaminaRegen fora de combate ≈ 7.7/s

StaminaRegen em combate = 35% a 50% do valor fora de combate
StaminaRegen em combate ≈ 2.7 a 3.9/s

Com fome baixa/cansaço alto ≈ 1.5 a 2.8/s
```

Leitura:

```text
Em combate, Stamina não volta rápido o suficiente para sustentar spam.
Fora de combate, a recuperação não é punitiva demais.
```

---

# PARTE E — Movimento, defesa e ataque

## 10. Dash

```text
DashCost = 40 Stamina antes de reduções.
DashCooldown ≈ 1.12s no exemplo após pequenos bônus.
```

Leitura:

```text
Dash é reposicionamento forte.
Não deve ser usado em loop; 3 dashes quase zeram a barra.
```

## 11. Dodge

```text
DodgeCost = 40 Stamina antes de reduções.
DodgeIFrames = 0.18s + 0.02s por Reflexo de Esquiva r1
DodgeIFrames = 0.20s
```

Leitura:

```text
Dodge é uma defesa forte de timing.
Não deve substituir movimentação normal.
```

## 12. Block

```text
BlockPower = 49% por Block r3 + 10% escudo = 59%
BlockStability = 12% Guarda Firme r2 + 8% escudo + 4% Constituição moderada = 24%
Block hold = 18 Stamina/s
```

### Block impact proporcional ao dano pós-armadura

```text
MitigatedDamageForStamina = max(MinDamageForStamina, IncomingRawDamage - ArmorMitigationValue)
IncomingDamageRatio = MitigatedDamageForStamina / PlayerMaxHP
BlockImpactStaminaCost = PlayerMaxStamina * IncomingDamageRatio * BlockImpactMultiplier * (1 - BlockStability)
```

Se a spec futura separar resistência percentual e armadura flat:

```text
MitigatedDamageForStamina = max(
  MinDamageForStamina,
  (IncomingRawDamage * (1 - PhysicalResistance)) - ArmorFlatMitigation
)
```

Multiplicadores usados neste teste:

```text
comum = 0.90
elite = 1.20
boss = 1.50
MinDamageForStamina = 1 a 5
```

Exemplo comum com dano bruto 57 e mitigação 14:

```text
MitigatedDamageForStamina = 57 - 14 = 43
IncomingDamageRatio = 43 / 230 = 18.7%
BlockImpactStaminaCost = 144 * 0.187 * 0.90 * 0.76
BlockImpactStaminaCost ≈ 18 Stamina
```

Exemplo elite com dano bruto 90 e mitigação 14:

```text
MitigatedDamageForStamina = 90 - 14 = 76
IncomingDamageRatio = 76 / 230 = 33.0%
BlockImpactStaminaCost = 144 * 0.330 * 1.20 * 0.76
BlockImpactStaminaCost ≈ 43 Stamina
```

Leitura:

```text
Block contra comum custa relevante, mas sustentável.
Block contra elite custa muito, mas menos do que se fosse baseado em dano bruto total.
Armadura melhora sobrevivência e também reduz dreno de Stamina no block impact.
Se o jogador segurou block por 1s antes do impacto elite, gasta ~61 Stamina total.
```

## 13. Ataque físico

```text
BaseAttack = 8 + Força*3 + Destreza*1 + Level*0.5
BaseAttack = 8 + 9*3 + 6 + 15
BaseAttack = 56
```

```text
AttackDamage = (BaseAttack + WeaponDamage) * (1 + SkillDamageBonus)
AttackDamage = (56 + 34) * 1.06
AttackDamage ≈ 95
```

```text
CritChance normal = 5% + 3% Destreza + 2% arma = 10%
CritChance em abertura comum = 30%
CritDamage = 1.5x
```

```text
StaggerPower = 10 base + 10 arma + Força*2 + SkillBonus 15
StaggerPower = 53
PostureDamageNormal ≈ 40
PostureDamageCharged ≈ 80
```

## 14. Defesa

```text
Armor = 45
Defense = BaseDefense + Armor + Constituição*0.75
Defense = 5 + 45 + 8*0.75
Defense = 56
PhysicalResistance = 12%
PostureResistance = 15-18%
```

Mitigação sugerida:

```text
DamageTaken = IncomingDamage * (1 - PhysicalResistance) - (Defense * 0.25)
```

Em block:

```text
BlockedIncoming = IncomingDamage * (1 - BlockPower)
BlockedDamageTaken = BlockedIncoming * (1 - PhysicalResistance) - (Defense * 0.10 a 0.15)
```

Regra:

```text
Não aplicar Defense/Armor completo depois do Block.
```

---

# PARTE F — Comparação contra monstros reais da faixa 26-40

## 15. Monstros usados

A faixa 26-40 do roster corresponde à Caverna de Gelo.

| Monstro | HP | STA | FOR | CON | DES | Papel |
|---|---:|---:|---:|---:|---:|---|
| Roedor de Geada | 155 | 72 | 9 | 9 | 16 | comum rápido |
| Escavador Duergar | 230 | 76 | 15 | 16 | 7 | comum robusto |
| Quebra-Escudo Duergar | 520 | 84 | 22 | 22 | 5 | elite tank |
| Sentinela Enregelado | 480 | 40 | 18 | 24 | 3 | construct/tank |
| Osso de Vidro | 210 | 58 | 12 | 8 | 15 | ranged/chaser |
| Acólito do Frio | 190 | 48 | 5 | 8 | 8 | caster |
| Saltador Cristalino | 330 | 104 | 16 | 12 | 24 | elite móvel |
| Lamento Frio | 300 | 52 | 4 | 11 | 10 | controller/caster |
| Horror-Gancho de Gelo | 560 | 88 | 24 | 21 | 8 | elite duelist |
| Larva Devora-Mentes | 240 | 50 | 3 | 8 | 12 | controller aberrante |

Comparação:

```text
Brann HP 230
Brann Stamina 144
Brann AttackDamage 95
Brann Defense 56
Brann BlockPower 59%
```

Leitura:

```text
HP do Brann agora fica parecido com comum robusto, não com elite.
Stamina do Brann continua maior que a dos monstros porque ele carrega exploração, defesa, ferramentas e combate.
AttackDamage continua alto contra comuns; packs e stamina management precisam segurar o desafio.
```

---

# PARTE G — TTK e consumo de Stamina

## 16. Dano efetivo estimado

```text
hit contra leve/caster: 85-95
hit contra comum robusto: 70-80
hit contra elite móvel: 75-85
hit contra elite tank: 55-70
hit contra construct/tank com espada: 45-60
ataque carregado: +25% dano e mais PostureDamage
crítico normal: x1.5
critical window: crítico automático apenas em janelas claras/especiais; janelas comuns dão bônus de crit/dano
```

## 17. Hits para matar e Stamina

| Monstro | HP | Dano efetivo estimado | Hits normais | Stamina só em light attacks | Veredito |
|---|---:|---:|---:|---:|---|
| Roedor de Geada | 155 | 85-95 | 2 | ~50 | ok em pack |
| Escavador Duergar | 230 | 70-80 | 3-4 | ~75-100 | consome muito se lutar direto |
| Osso de Vidro | 210 | 80-90 | 3 | ~75 | ok, mas exige cuidado |
| Acólito do Frio | 190 | 85-95 | 2-3 | ~50-75 | frágil se isolado |
| Larva Devora-Mentes | 240 | 80-90 | 3 | ~75 | depende do controle |
| Lamento Frio | 300 | 75-85 | 4 | ~100 | caster/controller relevante |
| Saltador Cristalino | 330 | 75-85 | 4-5 | ~100-125 | elite móvel adequado |
| Sentinela Enregelado | 480 | 45-60 | 8-11 | ~200-275 | exige janela/arma adequada |
| Quebra-Escudo Duergar | 520 | 55-70 | 8-10 | ~200-250 | elite anti-block adequado |
| Horror-Gancho de Gelo | 560 | 55-70 | 8-11 | ~200-275 | elite duelist adequado |

Veredito:

```text
Contra comuns, o jogador consegue vencer, mas não pode errar muito se houver pack.
Contra elites, ataque leve puro é inviável sem regen, janelas, charged attacks, vulnerabilidades, comida, companion/pet ou recuo.
Isso reforça o papel de critical window e vulnerabilidades.
```

---

# PARTE H — Dano recebido

## 18. Fórmula provisória de dano inimigo

```text
CommonLightDamage = 18 + SpawnLevel*0.6 + FOR*1.4
CommonHeavyDamage = CommonLightDamage * 1.45

EliteLightDamage = 28 + SpawnLevel*0.8 + FOR*1.6
EliteHeavyDamage = EliteLightDamage * 1.55

CasterDamage = 22 + SpawnLevel*0.6 + INT*1.5 ou VON*1.3
```

Para level 30:

```text
Brann Defense = 56
PhysicalResistance = 12%
HP = 230
BlockPower = 59%
Stamina = 144
```

## 19. Dano recebido sem block

| Monstro | Ataque estimado | Dano bruto | Dano final aproximado | Hits para derrubar Brann |
|---|---|---:|---:|---:|
| Roedor de Geada | bite comum | ~49 | ~29 | 8 |
| Escavador Duergar | pick swing | ~57 | ~36 | 6-7 |
| Osso de Vidro | lunge/shard | ~53 | ~33 | 7 |
| Acólito do Frio | cold bolt | ~50 mágico | depende de resistência gelo | 5-7 se sem resistência |
| Saltador Cristalino | leap | ~78 elite | ~55 | 4-5 |
| Quebra-Escudo Duergar | crush | ~87 elite | ~62 | 4 |
| Horror-Gancho de Gelo | hook impale | ~90 elite | ~65 | 3-4 |

Leitura:

```text
Com HP 230, comuns não matam rápido isolados, mas packs pressionam.
Elites punem erro em 3-5 hits.
Isso fica mais próximo de difícil, mas possível.
```

## 20. Dano recebido com block

Exemplo contra elite:

```text
Incoming = 90
BlockPower = 59%
BlockedIncoming = 90 * 0.41 = 36.9
PhysicalResistance = 12%
DefenseFlat parcial = Defense * 0.12 = 6.7
DamageTaken = 36.9 * 0.88 - 6.7 ≈ 26
```

Custo de Stamina para o impacto:

```text
MitigatedDamageForStamina = 90 - 14 = 76
IncomingDamageRatio = 76 / 230 = 33.0%
BlockImpactStaminaCost = 144 * 0.330 * 1.20 * 0.76
BlockImpactStaminaCost ≈ 43
```

Com hold:

```text
Block hold por 1s = 18
Total aproximado se segurou 1s antes do impacto = 61
```

Leitura:

```text
Block reduz muito dano, mas custa Stamina relevante.
Armadura reduz o dano usado no cálculo do dreno de Stamina, então armor continua valiosa para builds de block.
```

---

# PARTE I — Simulações curtas

## 21. Brann vs Escavador Duergar do Gelo

```text
Escavador HP 230
Brann dano efetivo: 75
Hits para matar: 4
Stamina se só atacar leve: ~100
Dano recebido por hit: 36
```

Cenário jogando bem:

```text
Round 1: ataque normal, Duergar -75, Stamina -25
Round 2: Duergar ataca, Brann bloqueia por ~0.5s, recebe ~12-16, Stamina -9 hold -18 impacto
Round 3: ataque carregado, Duergar -95/posture pressure, Stamina -40
Round 4: finaliza em abertura, Stamina -25
```

Consumo aproximado:

```text
Stamina total consumida: ~117
Stamina restante antes de regen: ~27
```

Com regen em combate por ~4-6 segundos:

```text
recupera ~11-23 Stamina
fica com ~38-50 Stamina
```

Veredito:

```text
Comum robusto isolado ainda consome recurso relevante.
A armadura reduz o dreno do block impact, mas não torna block gratuito.
Em pack, o jogador precisa usar janela, recuar, comer, usar companion/pet ou evitar trocar golpe direto.
```

## 22. Brann vs Saltador Cristalino

```text
Saltador HP 330
Brann dano efetivo: 80
Hits para matar: 4-5
Saltador tem DES 24 e STA 104.
```

Leitura:

```text
O Saltador ameaça porque obriga Dodge/Dash caros.
Se o jogador usar 2 Dodges, gasta 80 Stamina.
O encontro precisa permitir leitura de telegraph, não forçar dodge impossível.
```

## 23. Brann vs Quebra-Escudo Duergar

```text
Quebra-Escudo HP 520
Brann dano efetivo: 55-70
Hits normais para matar: 8-10
FOR 22 / CON 22 / elite tank
```

Leitura:

```text
Esse inimigo testa Block.
Se Brann só segura Left Shift, GuardBreak e custo proporcional drenam a Stamina.
Se alterna Dodge, ataque carregado e janela, vence.
Sem janelas claras, pode ficar punitivo demais.
```

---

# PARTE J — Comparação level 50

## 24. Exemplo level 50 físico equilibrado

```text
Level 50
FOR 14
CON 12
DES 10
Equipamento HP +35
Equipamento Stamina +15
```

HP:

```text
HPMax = 110 + 50*2 + 12*5 + 35
HPMax = 305
```

Stamina:

```text
StaminaMax = 80 + 50*0.6 + 12*2 + 14*1.5 + 10*1 + 15
StaminaMax = 180
```

Leitura:

```text
Com ataques late/mid-late custando 30+ Stamina, level 50 ainda não vira spam.
Com Dash/Dodge em 40+, movimentação defensiva segue sendo decisão forte.
```

---

# PARTE K — Conclusão

## 25. Veredito

```text
A régua ficou mais difícil e tática.
Stamina é limitador real.
Comuns robustos já consomem recurso se enfrentados sem cuidado.
Elites exigem janelas, vulnerabilidades, companion/pet, comida, ou execução boa.
Dash/Dodge a 40 impedem spam defensivo.
Block é forte, mas caro contra dano alto.
Armor agora reduz também o dreno de Stamina do impacto bloqueado.
```

## 26. Decisões corrigidas

```text
Player HPPerLevel = 2.
Player HPPerCon = 5.
Player StaminaPerLevel = 0.6.
Player StaminaPerCon = 2.0.
Player StaminaPerStr = 1.5.
Player StaminaPerDex = 1.0.
Player DefensePerCon = 0.75.
Light melee com Espada de Aço = 25 Stamina.
Heavy melee com Espada de Aço = 40 Stamina.
Dash = 40 Stamina.
Dodge = 40 Stamina.
Block hold = 18 Stamina/s.
Block impact = proporcional ao dano pós-armadura/mitigação contra HP máximo do jogador.
Monstros não usam fórmula de HP do jogador.
Monstros usam multiplicador próprio de CON por família/papel.
```

## 27. Riscos restantes

```text
Custos altos podem ficar punitivos se o input ou hitbox forem imprecisos.
Dash/Dodge a 40 exigem telegraphs muito claros.
Packs densos demais podem ficar injustos se todos exigirem Dodge.
Stamina Regen em combate precisa ser validada em Unity.
Com 144 Stamina, early/mid-game precisa de comida, descanso e pacing bem calibrados.
Block proporcional precisa de cap mínimo/máximo para evitar casos extremos.
ArmorMitigationValue precisa ser definido de forma consistente na spec de combate.
```
