# Cindar's Hope — Player Derived Attributes Tabletop Example

> **Status:** documento complementar de validação de mesa  
> **Local:** `docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_TABLETOP_EXAMPLE.md`  
> **Complementa:** `docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md`  
> **Função:** demonstrar, com personagem exemplo, como atributos centrais viram atributos derivados, como esses valores se comportam contra monstros reais da caverna, e registrar a remoção de **Breath/Fôlego** como atributo.  
> **Não é spec implementável.** Os números abaixo são exemplos de mesa para validar escala relativa, não valores finais.

---

## 0. Veredito desta revisão

A primeira simulação deixou uma confusão real:

```text
Stamina e Breath ficaram parcialmente sobrepostos.
Breath foi tratado como segunda barra de recurso gasta por Dash/Dodge/Block.
Isso não deve ser a direção final.
```

Decisão corrigida:

```text
Breath/Fôlego foi removido como atributo/recurso.
Não existe como barra.
Não existe como custo.
Não entra em HUD.
Não entra como stat base do jogador.
Não entra como stat base de monstro.
```

Substituições:

```text
Stamina = recurso físico imediato.
Cansaço = desgaste acumulado.
Constituição = tolerância física, HP, resistência, estabilidade.
Destreza = reação, dodge, movimento, timing.
Vontade = resistência mental/espiritual, MP e pressão mágica.
Survival = eficiência em runs, fome, cansaço, ambiente, dodge/dash melhorados.
Traits de monstro = padrão de movimento, perseguição, recuperação e pressão.
```

---

# PARTE A — Modelo final sem Breath

## 1. Stamina

Stamina representa energia física disponível para executar ações.

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

É uma barra de recurso.

Aparece na HUD.

## 2. Cansaço

Cansaço é o sistema de atrito longo.

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
Constituição
Survival/Sobrevivente
comida
sono
descanso
equipamentos específicos
buffs específicos
```

## 3. Movimento, esforço e recuperação sem Breath

O que antes seria Breath agora fica distribuído:

| Função antiga de Breath | Nova fonte |
|---|---|
| sustentar corrida/dash | Stamina + Destreza + Survival |
| manter Block | Stamina + Constituição + Melee + escudo |
| recuperar ritmo | Stamina Regen + Constituição + Cansaço/Fome |
| resistir a ambiente | Survival + Constituição + Vontade + resistências |
| perseguir como monstro | MovementProfile + traits + Stamina |
| resistir a controle | Constituição/Vontade + StatusResistance |

---

# PARTE B — Personagem de teste revisado

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
| Força | 9 | dano físico, stagger, mineração |
| Constituição | 8 | HP, Stamina, resistência, estabilidade |
| Destreza | 6 | timing, dodge, crit condicional |
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
| Survival / Sobrevivente | 4 | Ritmo de Jornada r1, Reflexo de Esquiva r1, Passo de Impulso r1, Respiração Controlada r1 |
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

# PARTE C — Constantes revisadas

## 5. Constantes de mesa

```text
BaseHP = 100
HPPerLevel = 4
HPPerCon = 12

BaseMP = 40
MPPerLevel = 1
MPPerWill = 6
MPPerInt = 2

BaseStamina = 75
StaminaPerLevel = 1.5
StaminaPerCon = 5
StaminaPerStrSmall = 1.5

BaseAttackValue = 8
BaseAttackPerStr = 3
BaseAttackPerDexSmall = 1
BaseAttackPerLevel = 0.5

BaseCritChance = 5%
BaseCritMultiplier = 1.5x
BaseDodgeIFrames = 0.18s
BaseDashDistance = 2.25 tiles
BaseDashCooldown = 1.20s
BaseDashCost = 22 Stamina
BaseDodgeCost = 14 Stamina
```

---

# PARTE D — Atributos derivados revisados

## 6. Recursos principais

### HP Max

```text
HPMax = BaseHP + Level*4 + Constituição*12 + EquipmentHP
HPMax = 100 + 30*4 + 8*12 + 20
HPMax = 336
```

Leitura:

```text
336 HP coloca o personagem acima de monstros comuns da faixa 26-40, mas abaixo/na borda de elites.
Isso é mais saudável que 420 HP para um level 30 sem companion/pet contabilizado.
```

### MP Max

```text
MPMax = BaseMP + Level*1 + Vontade*6 + Inteligência*2 + EquipmentMP
MPMax = 40 + 30 + 5*6 + 4*2 + 8
MPMax = 116
```

Leitura:

```text
MP existe, mas sem Magic/Arcano não vira poder mágico relevante.
```

### Stamina Max

```text
StaminaMax = BaseStamina + Level*1.5 + Constituição*5 + Força*1.5 + EquipmentStamina
StaminaMax = 75 + 30*1.5 + 8*5 + 9*1.5 + 10
StaminaMax = 183.5 ≈ 184
```

Leitura:

```text
184 Stamina ainda permite combate e mineração, mas impede spam excessivo.
É mais coerente que 291 para a faixa 26-40.
```

## 7. Regeneração e custos corrigidos

### Stamina Regen

```text
StaminaRegen fora de combate = BaseRegen + Constituição*0.55 + SurvivalBonus - ArmorPenalty
StaminaRegen fora de combate = 8 + 8*0.55 + 1.5 - 1.2
StaminaRegen fora de combate ≈ 12.7/s

StaminaRegen em combate = 55% a 70% do valor fora de combate
StaminaRegen em combate ≈ 7.0 a 8.9/s

Com fome baixa/cansaço alto ≈ 4.0 a 6.0/s
```

### Dash

```text
DashCost = BaseDashCost * (1 - PassoDeImpulsoBonus - DexSmallBonus)
DashCost = 22 * (1 - 0.05 - 0.02)
DashCost ≈ 20 Stamina

DashCooldown = 1.20s * (1 - 0.05 - 0.02)
DashCooldown ≈ 1.12s
```

### Dodge

```text
DodgeCost = BaseDodgeCost * (1 - ReflexoBonusSmall - DexSmallBonus)
DodgeCost = 14 * (1 - 0.03 - 0.02)
DodgeCost ≈ 13 Stamina

DodgeIFrames = 0.18s + 0.02s por Reflexo de Esquiva r1
DodgeIFrames = 0.20s
```

### Block

```text
Block não usa Breath.
Block usa Left Shift.
Block drena Stamina por tempo segurando e por impacto.
Constituição entra em Block Stability.
```

Exemplo:

```text
BlockPower = 49% por Block r3 + 10% escudo = 59%
BlockStability = 12% Guarda Firme r2 + 8% escudo + 8% Constituição = 28%
```

---

# PARTE E — Ofensivos e defensivos revisados

## 8. Ofensivos físicos

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

## 9. Defensivos

```text
Armor = 45
Defense = 5 + Constituição*2 + Armor
Defense = 66
PhysicalResistance = 12%
PostureResistance = 18%
```

Mitigação sugerida:

```text
DamageTaken = IncomingDamage * (1 - PhysicalResistance) - (Defense * 0.25)
```

Em golpes bloqueados:

```text
BlockedIncoming = IncomingDamage * (1 - BlockPower)
BlockedDamageTaken = BlockedIncoming * (1 - PhysicalResistance) - (Defense * 0.10 a 0.15)
```

Regra importante:

```text
Não aplicar Defense/Armor completo depois do Block.
Isso empilha mitigação demais.
```

---

# PARTE F — Comparação contra monstros reais da faixa 26-40

## 10. Fontes de comparação

A faixa 26-40 do roster corresponde à Caverna de Gelo.

Monstros usados:

```text
Roedor de Geada
Escavador Duergar do Gelo
Quebra-Escudo Duergar
Sentinela Enregelado
Osso de Vidro
Acólito do Frio
Saltador Cristalino
Lamento Frio
Horror-Gancho de Gelo
Larva Devora-Mentes
```

Observação:

```text
O roster ainda não define dano por ação em cada ataque.
A comparação abaixo usa HP/STA/atributos do roster e uma fórmula provisória para estimar dano.
A spec final precisa criar EnemyActionDamage por ação.
```

## 11. Leitura da faixa 26-40 sem BR

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

Comparação com Brann revisado:

```text
Brann HP 336
Brann Stamina 184
Brann AttackDamage 95
Brann Defense 66
Brann BlockPower 59%
```

Leitura:

```text
HP do Brann fica acima dos comuns, mas abaixo de elites tank.
Stamina do Brann é maior que a dos monstros porque player precisa carregar exploração, ferramentas, defesa e combate.
AttackDamage 95 mata comuns rápido, mas elites ainda exigem padrão, postura e janela.
```

---

# PARTE G — TTK aproximado contra monstros 26-40

## 12. Dano efetivo usado

Como o roster não tem Armor/Resistance por monstro individual ainda, esta comparação usa mitigação estimada por papel:

```text
comum leve: 0-10% mitigação
comum robusto: 15-20% mitigação
elite móvel: 10-15% mitigação
elite tank: 25-35% mitigação
caster: 5-10% mitigação
construct/tank: 30-40% mitigação se o jogador usar espada comum
```

Dano médio do Brann:

```text
hit contra leve/caster: 85-95
hit contra comum robusto: 70-80
hit contra elite móvel: 75-85
hit contra elite tank: 55-70
hit contra construct/tank com espada: 45-60
ataque carregado: +25% dano e mais PostureDamage
crítico normal: x1.5
critical window: depende da regra final; preferir crit automático apenas em janelas claras
```

## 13. Tabela de TTK

| Monstro | HP | Dano efetivo estimado | Hits normais para matar | Veredito |
|---|---:|---:|---:|---|
| Roedor de Geada | 155 | 85-95 | 2 | ok se vier em pack; fraco isolado |
| Escavador Duergar | 230 | 70-80 | 3-4 | ok como comum robusto |
| Osso de Vidro | 210 | 80-90 | 3 | ok, ameaça por ranged/mobilidade |
| Acólito do Frio | 190 | 85-95 | 2-3 | ok se protegido por pack; fraco isolado |
| Larva Devora-Mentes | 240 | 80-90 | 3 | ok se controle for perigoso |
| Lamento Frio | 300 | 75-85 | 4 | ok como caster/controller |
| Saltador Cristalino | 330 | 75-85 | 4-5 | ok; dificuldade vem da mobilidade |
| Sentinela Enregelado | 480 | 45-60 | 8-11 | ok se for tank/guard; espada não é ideal |
| Quebra-Escudo Duergar | 520 | 55-70 | 8-10 | ok como elite anti-block |
| Horror-Gancho de Gelo | 560 | 55-70 | 8-11 | ok como elite duelist |

Veredito:

```text
Com HP/Stamina revisados, o personagem fica mais coerente.
O dano do personagem ainda está alto contra comuns, mas isso é aceitável se comuns vierem em pack.
Elites ficam no intervalo bom: 8-11 hits normais, menos se o jogador usar charged attacks, vulnerabilidade e critical window.
```

---

# PARTE H — Dano recebido aproximado

## 14. Fórmula provisória de dano inimigo

Como o roster ainda não tem dano por ataque, usar mesa provisória:

```text
CommonLightDamage = 18 + SpawnLevel*0.6 + FOR*1.4
CommonHeavyDamage = CommonLightDamage * 1.45

EliteLightDamage = 28 + SpawnLevel*0.8 + FOR*1.6
EliteHeavyDamage = EliteLightDamage * 1.55

CasterDamage = 22 + SpawnLevel*0.6 + INT*1.5 ou VON*1.3
```

Para level 30:

```text
Brann Defense = 66
PhysicalResistance = 12%
HP = 336
BlockPower = 59%
```

## 15. Dano recebido sem block

| Monstro | Ataque estimado | Dano bruto | Dano final aproximado | Hits para derrubar Brann |
|---|---|---:|---:|---:|
| Roedor de Geada | bite comum | ~49 | ~27-32 | 10-12 |
| Escavador Duergar | pick swing | ~57 | ~34-40 | 8-10 |
| Osso de Vidro | lunge/shard | ~53 | ~30-36 | 9-11 |
| Acólito do Frio | cold bolt | ~50 mágico | depende de resistência gelo | 7-10 se sem resistência |
| Saltador Cristalino | leap | ~78 elite | ~52-60 | 6-7 |
| Quebra-Escudo Duergar | crush | ~87 elite | ~60-70 | 5-6 |
| Horror-Gancho de Gelo | hook impale | ~90 elite | ~63-73 | 5-6 |

Leitura:

```text
Com HP 336, golpes comuns não matam rápido.
Elites punem erro em 5-7 hits.
Caster de gelo fica perigoso se o jogador não tiver resistência a frio.
Isso conversa com a regra da caverna 26-40: solo difícil, companion recomendado forte, resistência a frio e gear esperado.
```

## 16. Dano recebido com block

Exemplo contra elite:

```text
Incoming = 90
BlockPower = 59%
BlockedIncoming = 90 * 0.41 = 36.9
PhysicalResistance = 12%
DefenseFlat parcial = Defense * 0.12 = 7.9
DamageTaken = 36.9 * 0.88 - 7.9 ≈ 25
```

Leitura:

```text
Block reduz elite hit de ~65 para ~25.
Isso é forte, mas custa Stamina e pode sofrer GuardBreak.
Contra Quebra-Escudo Duergar, parte dos ataques deve punir block segurado.
```

---

# PARTE I — Simulação curta contra três monstros

## 17. Brann vs Escavador Duergar do Gelo

```text
Escavador HP 230
Brann dano efetivo: 75
Hits para matar: 4
Dano recebido por hit: 35-40
```

Cenário jogando bem:

```text
Round 1: Brann acerta normal, Duergar -75
Round 2: Duergar usa PickSwing, Brann bloqueia, recebe ~12-16
Round 3: Brann usa ataque carregado, Duergar -95 e perde postura
Round 4: Brann finaliza com hit normal/crítico de janela
```

Veredito:

```text
Comum robusto adequado.
Isolado não é grande ameaça.
Em pack com caster/roedor, vira pressão real.
```

## 18. Brann vs Saltador Cristalino

```text
Saltador HP 330
Brann dano efetivo: 80
Hits para matar: 4-5
Saltador tem DES 24 e STA 104.
```

Leitura:

```text
O Saltador não deve tankar muito.
Ele deve ameaçar por mobilidade, burst e erro de dodge.
Se o jogador acertar todas as janelas, vence rápido.
Se errar dodge, recebe muito dano e perde ritmo.
```

## 19. Brann vs Quebra-Escudo Duergar

```text
Quebra-Escudo HP 520
Brann dano efetivo: 55-70
Hits normais para matar: 8-10
FOR 22 / CON 22 / elite tank
```

Leitura:

```text
Esse inimigo deve ser teste direto da build de Block.
Se Brann só segura Left Shift, deve ser punido por GuardBreak.
Se alterna Dodge, ataque carregado e janela, vence.
```

Veredito:

```text
Adequado como elite se tiver ShieldCrush/GuardBreak bem telegrafado.
Sem GuardBreak, o Block do jogador pode trivializar o encontro.
```

---

# PARTE J — Conclusão de balanceamento

## 20. Comparação final

Com os valores antigos:

```text
HP 420
Stamina 291
Breath 135 como barra/recurso
```

Problemas:

```text
HP alto demais para level 30 defensivo.
Stamina alta demais para custo de Dash/Dodge/Block.
Breath competia conceitualmente com Stamina.
```

Com os valores revisados:

```text
HP 336
Stamina 184
Breath removido
```

Resultado:

```text
HP fica próximo da borda baixa de elite, adequado para player tank level 30.
Stamina ainda é maior que a dos monstros, mas justificável para o player.
Stamina volta a ser o recurso físico principal.
Cansaço vira o limitador de longo prazo.
Constituição, Destreza e Vontade assumem o que antes estava disperso em Breath.
```

## 21. Decisões corrigidas

```text
Breath/Fôlego removido como atributo.
Dash custa Stamina.
Dodge custa Stamina.
Block drena Stamina.
Constituição melhora HP, Stamina, estabilidade, resistência física e tolerância a cansaço.
Destreza melhora dodge, movimento, attack speed, crit condicional e reação.
Vontade melhora MP, MP Regen lenta e resistência mental/espiritual.
Survival melhora runs longas, custo/cooldown/recovery de Dash/Dodge, fome, cansaço e ambiente.
```

## 22. Ajustes necessários em documentos futuros

```text
PLAYER_CORE_SYSTEMS_DIRECTION.md
  remover Breath como recurso/atributo derivado.

PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
  remover Breath Max, Breath Recovery, Dash/Dodge/Block com Breath.

PLAYER_SKILL_TREES_DIRECTION.md
  Respiração Controlada deve ser renomeada ou redefinida, porque não há Breath.

CAVE_MONSTER_ROSTER_DIRECTION.md
  remover BR dos monstros e substituir por MovementProfile, RecoveryProfile, PressureProfile ou traits.
```

## 23. Pendências para a spec de combate

```text
Criar EnemyActionDamage por ação.
Definir Armor/Resistance por monstro ou por família.
Definir se CriticalWindow é sempre crítico automático ou se varia por tipo de janela.
Definir como GuardBreak interage com Block.
Definir Stamina Regen em combate, fora de combate, com fome e com cansaço.
Definir como Constituição/Survival alteram FatigueGain.
Definir que Breath não aparece na HUD.
```
