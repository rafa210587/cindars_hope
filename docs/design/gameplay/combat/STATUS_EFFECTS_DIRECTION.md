# Cindar's Hope — Status Effects Direction

> **Status:** documento canônico de direção de status effects  
> **Local:** `docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md`  
> - `docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md`  
> - `docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md`  
> - `docs/design/gameplay/enemies/ENEMY_BEHAVIORS_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`  
> **Função:** definir o significado mecânico dos status aplicados por armas, magias, inimigos, itens, biomas e bosses.  
> **Não é spec implementável.** Specs futuras devem converter isto em dados e runtime.

---

## 0. Regra anti-duplicação

Este documento é a fonte canônica do significado dos status.

Outros documentos podem citar status, mas não devem redefinir seus efeitos.

Regra:

```text
Equipment aplica StatusTag.
EnemyData declara StatusVulnerability/ResistanceTags.
Combat runtime calcula duração, stack, dano por tick e feedback.
Status Effects Direction define o que cada status significa.
```

---

# PARTE A — Modelo geral

## 1. Campos mínimos de StatusEffectDataSO

```text
StatusId
DisplayName
Category
Duration
TickInterval
MaxStacks
StackBehavior
DamageType
DamagePerTick
StatModifiers
MovementModifiers
CanAffectPlayer
CanAffectEnemy
CanAffectBoss
BossEffectMultiplier
ResistanceTagsChecked
VulnerabilityTagsChecked
VisualFeedback
AudioFeedback
HudIcon
ClearConditions
SourceTags
```

## 2. Categorias

```text
DamageOverTime
Control
Debuff
Environmental
Corruption
EquipmentStress
Buff
WindowState
```

Regra:

```text
Status forte precisa de telegraph/counterplay.
Controle total do jogador deve ser raro, curto e nunca permanente.
Bosses podem ter multiplicadores reduzidos, mas não devem ser imunes por padrão sem lore/regra.
```

---

# PARTE B — Bleed

## 3. Bleed / Sangramento

`Bleed` é dano físico contínuo causado por cortes profundos, perfurações ou lâminas serrilhadas.

Fontes típicas:

```text
Axe charged effect
Dagger skills
BarbedArrow
BleedEdge
beast claws
certain traps
```

Efeito base:

```text
Bleed aplica dano físico por tempo.
Bleed não reduz armor diretamente.
Bleed é pior contra alvos vivos com carne/sangue.
Bleed é fraco ou inútil contra undead, constructs, elementais minerais e slimes sem anatomia compatível, salvo exceção explícita.
```

Baseline inicial:

```text
Duration: 4s-8s
TickInterval: 1s
DamagePerTick: 4%-8% do dano que aplicou Bleed, com mínimo baixo
MaxStacks: 3
StackBehavior: refresh duration + incrementa stack até cap
BossEffectMultiplier: 25%-50%, conforme boss
```

Exemplo:

```text
Ataque causa 40 de dano e aplica Bleed de 6s.
BleedDamagePerTick = 40 * 0.06 = 2.4, arredondado conforme spec.
Com 2 stacks, tick pode subir moderadamente, mas respeitando cap.
```

Regras:

```text
Bleed não deve explodir dano em ataques muito rápidos sem cap.
Bleed deve ter feedback visual discreto.
Bleed não deve aplicar em inimigos com ResistanceTags: BleedImmune, ConstructBody, UndeadBody ou SlimeBody, salvo override explícito.
Bleed pode ser StatusVulnerability em beasts, humanoides e alguns orcs.
```

---

# PARTE C — Outros status usados por equipamentos e inimigos

## 4. Burn

```text
DamageOverTime Fire.
Bom contra fungais, raízes e gelo.
Fraco contra fire/lava enemies.
Pode interagir com biomas inflamáveis no futuro.
```

## 5. Chill

```text
Debuff de velocidade/recovery.
Pode reduzir MovementSpeed e AttackRecovery levemente.
Forte contra inimigos de calor/fúria quando declararem vulnerabilidade.
Fraco contra gelo/duergar de gelo.
```

## 6. Poison

```text
DamageOverTime Poison.
Bom contra alvos vivos vulneráveis.
Fraco/inútil contra undead, constructs, elementais minerais e muitos slimes.
```

## 7. Stun

```text
Controle curto.
Interrompe ação se permitido.
Deve ser reduzido em elites/bosses.
Pode abrir StunnedWindow se a ação/inimigo permitir.
```

## 8. Slow

```text
Reduz movimento por curta duração.
Não deve impedir totalmente ação.
Root é status separado e mais forte.
```

## 9. Root

```text
Prende movimento por curta duração.
Deve permitir ação/defesa parcial ou ter duração baixa.
Não deve ser aplicado em loop.
```

## 10. Fear

```text
Pressão mental/espiritual.
Não remove controle total do jogador.
Pode reduzir ofensiva, alterar movimento curto ou forçar recuo leve em inimigos vulneráveis.
```

## 11. ConfusionLite

```text
Confusão leve.
Não inverte controles de forma agressiva no início.
Pode reduzir precisão, target lock ou leitura por pouco tempo.
```

## 12. DurabilityStress

```text
Afeta condição/durabilidade de equipamento.
Não destrói item permanentemente sem sistema claro de reparo.
Associado a acid, constructs, corrosão e hazards técnicos.
```

## 13. Corruption

```text
Status perigoso ligado a Pedra Negra/Nyx/sombra/corrupção.
Deve ter cura, prevenção ou purificação.
Não deve ser spam comum no early game.
```

---

# PARTE D — Window states não são status comuns

## 14. WindowState

Estados como abaixo são janelas de combate, não DoT/debuff comum:

```text
MinorOpening
CriticalWindow
CoreExposed
StaggeredWindow
PetInterruptWindow
CompanionSetupWindow
ArmorCracked
ShockOverloaded
Marked
ExposedCore
```

Regra:

```text
WindowState pode aparecer como tag runtime, mas sua fonte canônica de balance continua em Cave Combat Balance e Combat Core.
```

---

# PARTE E — Decisões fechadas

```text
Bleed é dano físico por tempo, forte contra alvos vivos e fraco/inútil contra constructs, undead, elementais minerais e slimes sem anatomia compatível.
Bleed deve ter stack cap.
Bleed não deve escalar sem limite com ASPD alta.
Status effects têm fonte canônica neste documento.
Vulnerabilidade/resistência ao status fica nos dados do inimigo/roster.
Equipamentos aplicam StatusTag; não redefinem o status.
```

---

# PARTE F — Pendências para specs futuras

```text
Criar StatusEffectDataSO.
Criar StatusResistance/Vulnerability contract em EnemyDataSO.
Criar HUD icons para status principais.
Criar regra de stack/cap por status.
Criar integração de Bleed com Axe/Dagger/BarbedArrow/BleedEdge.
Criar testes de Play Mode para Bleed contra Beast/Humanoid/Construct/Undead.
```
