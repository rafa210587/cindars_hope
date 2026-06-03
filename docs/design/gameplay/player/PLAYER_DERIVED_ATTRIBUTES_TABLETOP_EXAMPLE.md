# Cindar's Hope — Player Derived Attributes Tabletop Example

> **Status:** documento complementar de validação de mesa  
> **Local:** `docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_TABLETOP_EXAMPLE.md`  
> **Complementa:** `docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md`  
> **Função:** demonstrar, com personagem exemplo, como atributos centrais viram atributos derivados, como esses valores se comportam contra monstros reais da caverna, e validar a nova régua sem Breath/Fôlego, com Constituição menos dominante e Stamina mais controlada.  
> **Não é spec implementável.** Os números abaixo são exemplos de mesa para validar escala relativa, não valores finais.

---

## 0. Veredito desta revisão

A revisão anterior removeu Breath/Fôlego, mas ainda deixou Constituição forte demais.

Problema identificado:

```text
Constituição estava influenciando HP, Stamina, Stamina Regen, Defense, Block Stability, Posture Resistance, status físico e cansaço.
Isso fazia Constituição virar atributo defensivo universal.
```

Correção desta versão:

```text
HP do jogador cresce pouco por level e por Constituição.
Stamina do jogador cresce devagar.
Stamina é distribuída entre Constituição, Força e Destreza.
Stamina Regen em combate é baixa.
Custos de ataque, ferramenta, block, dash e dodge precisam escalar por tier/peso/tipo.
Monstros não usam fórmula do jogador; HP de monstro é autorado por família, papel e faixa.
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

## 3. Nova distribuição de papéis

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
Light melee com Espada de Aço: 14 Stamina
Heavy melee com Espada de Aço: 30 Stamina
Dash: 20 Stamina
Dodge: 13 Stamina
Block hold: 6 Stamina/s
Block impact comum: 12-18 Stamina
Block impact elite: 22-34 Stamina
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
144 Stamina permite sequência curta de ações, mas não spam.
Com espada de aço, 5-7 ataques leves já consomem boa parte da barra.
Heavy attacks, Dash e Block competem de verdade pelo mesmo recurso.
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
DashCost = 22 * (1 - 0.05 PassoDeImpulso - 0.02 DexSmallBonus)
DashCost ≈ 20 Stamina
DashCooldown ≈ 1.12s
```

## 11. Dodge

```text
DodgeCost = 14 * (1 - 0.03 ReflexoBonusSmall - 0.02 DexSmallBonus)
DodgeCost ≈ 13 Stamina
DodgeIFrames = 0.18s + 0.02s por Reflexo de Esquiva r1
DodgeIFrames = 0.20s
```

## 12. Block

```text
BlockPower = 49% por Block r3 + 10% escudo = 59%
BlockStability = 12% Guarda Firme r2 + 8% escudo + 4% Constituição moderada = 24%
```

Custos:

```text
Block hold: 6 Stamina/s
Block impact comum: 12-18 Stamina
Block impact elite: 22-34 Stamina
```

Leitura:

```text
Block continua forte, mas agora consome uma parcela relevante da Stamina total.
Se o jogador segurar block o tempo todo, esgota rápido.
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

## 17. Hits para matar

| Monstro | HP | Dano efetivo estimado | Hits normais | Stamina em ataques leves | Veredito |
|---|---:|---:|---:|---:|---|
| Roedor de Geada | 155 | 85-95 | 2 | ~28 | ok em pack |
| Escavador Duergar | 230 | 70-80 | 3-4 | ~42-56 | ok comum robusto |
| Osso de Vidro | 210 | 80-90 | 3 | ~42 | ok |
| Acólito do Frio | 190 | 85-95 | 2-3 | ~28-42 | frágil se isolado |
| Larva Devora-Mentes | 240 | 80-90 | 3 | ~42 | depende do controle |
| Lamento Frio | 300 | 75-85 | 4 | ~56 | ok caster/controller |
| Saltador Cristalino | 330 | 75-85 | 4-5 | ~56-70 | ok elite móvel |
| Sentinela Enregelado | 480 | 45-60 | 8-11 | ~112-154 | exige janela/arma adequada |
| Quebra-Escudo Duergar | 520 | 55-70 | 8-10 | ~112-140 | elite anti-block adequado |
| Horror-Gancho de Gelo | 560 | 55-70 | 8-11 | ~112-154 | elite duelist adequado |

Veredito:

```text
Contra comuns, o jogador consegue matar sem esgotar toda a Stamina.
Contra elites, se usar só ataque leve, fica perto de esgotar a barra.
Isso força charged attacks bem usados, critical windows, troca de ritmo, comida, companion/pet ou recuo.
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
Com HP 230, comuns não matam rápido, mas packs pressionam.
Elites punem erro em 3-5 hits.
Isso fica mais próximo de “difícil, mas possível”.
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

Custo de block:

```text
Block impact elite = 22-34 Stamina
Block hold por 1s = 6 Stamina
```

Leitura:

```text
Block reduz muito dano, mas consome 20% ou mais da Stamina total se usado contra elite.
Se o jogador bloquear errado, atacar e dar dash em sequência, a barra cai rápido.
```

---

# PARTE I — Simulações curtas

## 21. Brann vs Escavador Duergar do Gelo

```text
Escavador HP 230
Brann dano efetivo: 75
Hits para matar: 4
Stamina se só atacar leve: ~56
Dano recebido por hit: 36
```

Cenário jogando bem:

```text
Round 1: ataque normal, Duergar -75, Stamina -14
Round 2: Duergar ataca, Brann bloqueia, recebe ~12-16, Stamina -18 a -24
Round 3: ataque carregado, Duergar -95/posture pressure, Stamina -30
Round 4: finaliza em abertura, Stamina -14
```

Consumo aproximado:

```text
Stamina total consumida: 76-82
Stamina restante antes de regen: ~62-68
```

Veredito:

```text
Comum robusto adequado.
O jogador vence bem, mas gasta recurso relevante.
Em pack com caster/roedor, vira pressão real.
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
O Saltador não deve tankar muito.
Ele ameaça por mobilidade, burst e erro de dodge.
Se o jogador errar dodge, perde HP e Stamina rápido.
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
Se Brann só segura Left Shift, GuardBreak deve punir.
Se alterna Dodge, ataque carregado e janela, vence.
Sem GuardBreak, Block ainda pode trivializar.
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
Level 50 não pula para 20-30 ataques leves de graça.
Com arma late/mid-late custando 18-22 por ataque leve, ele ainda faz algo como 8-10 ataques leves antes de zerar, sem contar dash/block/dodge.
Isso é mais saudável.
```

---

# PARTE K — Conclusão

## 25. Veredito

```text
A nova régua é melhor.
Constituição deixa de ser atributo defensivo universal.
HP do jogador fica mais próximo de comum robusto, não de elite.
Stamina fica mais controlada.
Elites voltam a ser perigosos em 3-5 hits sem block/dodge.
Block continua forte, mas consome Stamina relevante.
Stamina exige organização mesmo no level 30 e 50.
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
Stamina costs escalam por tier/peso/tipo de ação.
Stamina Regen em combate fica baixa.
Monstros não usam fórmula de HP do jogador.
Monstros podem usar multiplicador próprio de CON por família/papel.
```

## 27. Riscos restantes

```text
AttackDamage 95 ainda pode estar alto contra comuns se eles aparecerem isolados.
Packs precisam ser densos o suficiente para gerar pressão.
Block precisa de GuardBreak, stamina drain e ataques que não sejam resolvidos só segurando Left Shift.
Critical window não deve ser sempre crítico automático.
Armor flat precisa de dano mínimo para inimigos pequenos continuarem relevantes em grupo.
Stamina Regen em combate precisa ser testada em Unity.
```
