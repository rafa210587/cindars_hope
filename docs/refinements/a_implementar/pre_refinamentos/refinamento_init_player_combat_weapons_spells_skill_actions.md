# refinamento_init_player_combat_weapons_spells_skill_actions

> Status: Refinamento inicial a implementar
> Spec futura relacionada: `docs/specs/a_implementar/spec_player_combat_weapons_spells_skill_actions_runtime.md`
> Objetivo: substituir punch hardcoded por combate data-driven com armas equipadas nas maos, unarmed fallback, uso Q/E, dodge simples, bow placeholder, magia basica, mana, active skill slots R/T/Y/G e integracao com damage/stamina/equipment.

---

## 1. Estado atual

O combate atual do jogador e MVP/debug:

```text
PlayerAttackController
- tecla J
- dano fixo
- range fixo
- cooldown fixo
- overlap circle
```

Tambem existem schemas iniciais:

```text
WeaponDataSO
SpellDataSO
SkillActionSO
```

mas eles ainda nao dirigem o runtime principal.

---

## 2. Gaps

- Armas nao sao equipadas/consumidas pelo attack controller.
- WeaponDataSO nao define o ataque real.
- Maos `LeftHand`/`RightHand` ainda nao resolvem a acao de combate.
- Nao ha mana.
- Nao ha spell caster.
- Nao ha skill action executor.
- Nao ha active slots reais conectados a SkillActionSO.
- Nao ha stamina cost/cooldown por acao de forma unificada.
- Nao ha bow/ranged, dodge, block/parry.
- Ataques nao passam consistentemente pelo DamageCalculator da spec 11.

---

## 3. Decisoes aprovadas

- Punch hardcoded deixa de ser ataque principal.
- Se nao houver arma equipada, jogador ainda pode atacar usando `UnarmedAttackDataSO`.
- Acoes de uso das maos usam `Q` e `E`.
- `Q` usa a mao esquerda (`LeftHand`).
- `E` usa a mao direita (`RightHand`) somente quando nao houver interacao de mundo com prioridade.
- `E` continua sendo tecla de interacao para NPCs, tiles, workstations, shop, pickups e objetos interagiveis.
- Prioridade de input:

```text
1. Se modal/HUD interativa esta aberta, Q/E obedecem ao modal atual.
2. Se E tem objeto/tile/NPC interagivel valido em foco, E executa interacao.
3. Se nao ha interacao valida, E usa RightHand.
4. Q usa LeftHand em gameplay normal.
```

- A mao ativa/acao vem do que esta equipado nas maos.
- `LeftHand` e `RightHand` da spec 10 sao a fonte oficial de itens de mao.
- Light attack entra agora.
- Heavy attack fica hook futuro.
- Dodge simples entra agora.
- Dodge usa stamina definida na spec 09; nao redefinir custos de stamina aqui.
- Dodge MVP nao tem i-frames.
- Block/parry fica fora.
- Bow/ranged placeholder entra sem ammo.
- Bow default range inicial: `6.0` world units.
- Bow default projectile speed inicial: `10.0` world units/second.
- Magia entra agora de forma simples.
- Mana entra agora.
- Stamina ja foi definida na spec 09 e deve ser respeitada, nao redefinida aqui.
- Weapon attacks usam stamina.
- Dodge usa stamina.
- Spells usam mana.
- Skill actions podem usar stamina, mana ou ambos.
- Active skill slots usam teclas:

```text
R
T
Y
G
```

- `SkillActionSO` e usado apenas para skills/active slots, nao para uso normal de arma/tool.
- Skill tree/unlock/respec fica fora desta spec; existe spec dedicada futura para isso.
- Cooldown comeca somente se a acao executou.
- AttackSpeed da spec 10 reduz cooldown/tempo entre ataques:

```text
effectiveCooldown = BaseCooldown / FinalAttackSpeed
```

- Friendly fire fica off.
- Damage numbers pertencem a spec 11; esta spec apenas dispara o pipeline correto.

---

## 4. Inputs

Inputs MVP:

```text
Q = usar LeftHand
E = interagir se houver alvo interagivel; caso contrario, usar RightHand
Space = dodge simples
R/T/Y/G = active skill slots
1-6 = preservar comportamento existente de hotbar/selecoes da spec 10
Esc = menus/modais conforme specs anteriores
```

Regras:

- Nao quebrar `E` como interacao principal.
- Nao quebrar hotkeys `1-6`.
- Nao criar conflito entre active slots e hotbar.
- `R/T/Y/G` sao reservados para active skill slots desta spec.
- Se algum desses inputs ja estiver ocupado no runtime atual, registrar conflito e criar mapping configuravel.

---

## 5. Maos e uso normal

### LeftHand / Q

- `Q` executa a acao primaria do item equipado na `LeftHand`.
- Se `LeftHand` esta vazia, pode executar fallback unarmed ou retornar feedback simples.
- Tool em `LeftHand` usa comportamento de tool, nao SkillActionSO.
- Weapon em `LeftHand` usa WeaponDataSO.

### RightHand / E

- `E` primeiro tenta interagir com o mundo.
- Se nao houver interacao valida, `E` executa acao primaria do item equipado na `RightHand`.
- Tool em `RightHand` usa comportamento de tool.
- Weapon em `RightHand` usa WeaponDataSO.

### Sem arma equipada

- Se a mao usada nao possui arma/tool de combate valida, usar `UnarmedAttackDataSO` como fallback de combate.
- Unarmed fallback deve ser data-driven, nunca dano fixo hardcoded no controller.

---

## 6. Weapon combat

`WeaponDataSO` minimo:

```text
WeaponId
DisplayName
WeaponCategory
WeaponWeightClass
DamageType
BaseDamage
Range
ArcDegrees
BaseCooldownSeconds
StaminaCost
AttackSpeedMultiplier opcional se ainda nao vier da spec 10
StatusApplicationRules[] opcional
ProjectileId opcional
DefaultProjectileSpeed opcional
DefaultProjectileRange opcional
```

Categorias MVP:

```text
MeleeLight
MeleeHeavy
Bow
Unarmed
ToolAsWeapon opcional
```

Regras:

- Light attack usa `WeaponDataSO`.
- Heavy attack fica hook futuro em dados, sem runtime completo agora.
- Melee usa overlap arc/circle na direcao do facing/input do player.
- Bow usa projectile simples.
- Bow nao consome ammo no MVP.
- Bow usa Dexterity como atributo ofensivo.
- Tudo passa pelo `DamageCalculator` da spec 11.

---

## 7. Bow placeholder

Valores default iniciais:

```text
DefaultBowRange = 6.0 world units
DefaultBowProjectileSpeed = 10.0 world units/second
DefaultBowDamageType = Physical
DefaultBowAmmoRequired = false
```

Regras:

- Projectile desaparece ao atingir alvo, parede/obstaculo ou ao exceder range.
- Sem friendly fire.
- Sem ammo no MVP.
- Ammo fica hook futuro.

---

## 8. Dodge simples

Dodge MVP:

```text
Input = Space
Resource = Stamina
StaminaCost = usar valor da spec 09
CooldownSeconds = configuravel
Distance = configuravel
DurationSeconds = configuravel
InvulnerabilityFrames = false no MVP
```

Regras:

- Dodge move o player por curta distancia na direcao de input/facing.
- Dodge nao atravessa colisao bloqueante.
- Dodge nao inicia se stamina insuficiente.
- Cooldown so inicia se dodge executou.
- Sem i-frames no MVP.

---

## 9. Mana e spells

Criar ou equivalente:

```text
ManaManager
PlayerSpellCaster
SpellDataSO
SpellCastRequest
SpellCastResult
```

Valores iniciais:

```text
MaxMana = 100
CurrentMana = 100
ManaRegenPerSecond = 5
```

Regras:

- Mana salva/carrega.
- Mana regenera lentamente enquanto gameplay nao esta pausado.
- Spell nao executa se mana insuficiente.
- Mana so e consumida se spell executar.
- Cooldown so inicia se spell executar.

`SpellDataSO` minimo:

```text
SpellId
DisplayName
DamageType
BaseDamage
ManaCost
CooldownSeconds
CastTimeSeconds
Range
ProjectileSpeed
StatusApplicationRules[] opcional
```

Spell minima de teste:

```text
ArcaneBolt
DamageType = Arcane
ManaCost configuravel
Cooldown configuravel
Range = 7.0 world units
ProjectileSpeed = 8.0 world units/second
BaseDamage configuravel
```

---

## 10. Skill actions e active slots

Active skill slots MVP:

```text
Slot 0 = R
Slot 1 = T
Slot 2 = Y
Slot 3 = G
```

Regras:

- Active slots sao apenas para skills.
- Uso normal de arma/tool por Q/E nao usa `SkillActionSO`.
- Skill tree/unlock/respec nao entra nesta spec.
- Skills podem ser predesbloqueadas/test data para validacao.
- Active slots persistem por `SkillActionId`, nao por referencia Unity.

`SkillActionSO` minimo:

```text
SkillActionId
DisplayName
SkillActionType
CooldownSeconds
StaminaCost opcional
ManaCost opcional
DamageRequest opcional
StatusApplicationRules[] opcional
LinkedSpellId opcional
```

Tipos MVP:

```text
DamageSkill
SelfBuffSkill
LinkedSpellSkill opcional
```

---

## 11. HUD minima

Esta spec adiciona:

```text
Mana bar
Active skill slots R/T/Y/G
Cooldown/resource feedback simples
```

Layout minimo sugerido:

```text
Hunger
Stamina
Mana
Status
ActiveSkillSlots R T Y G
```

---

## 12. Save/load

Persistir usando IDs e tipos simples:

```text
PlayerCombatSaveData
- CurrentMana
- ActiveSkillSlots[]

ActiveSkillSlotSaveData
- SlotIndex
- InputKey
- SkillActionId opcional
```

Regras:

- Nao persistir cooldowns curtos no MVP, salvo se ja houver infraestrutura padrao.
- Nao serializar WeaponDataSO, SpellDataSO, SkillActionSO, GameObject, Transform, MonoBehaviour, Sprite, Collider ou Rigidbody.
- Equipment save continua responsabilidade da spec 10.
- Damage/status save continua responsabilidade da spec 11.

---

## 13. Invariantes anti-regressao

Esta spec nao pode quebrar:

- Equipment hands/hotkeys da spec 10;
- hotkeys `1-6` existentes;
- `E` como interacao principal;
- stamina/custos/time pause da spec 09;
- AttackSpeed da spec 10;
- DamageCalculator/damage numbers/status da spec 11;
- inventory/equip binding;
- modal stack;
- NPC/shop/crafting UI;
- future skill trees da spec 16;
- future enemy AI da spec 13;
- regra de nao usar `GameObject.Find()` ou `FindObjectOfType()`;
- regra de nao serializar referencias Unity em DTOs.

---

## 14. Definition of Done

- [ ] Punch hardcoded nao e ataque principal.
- [ ] Sem arma equipada, `UnarmedAttackDataSO` executa fallback data-driven.
- [ ] `Q` usa LeftHand.
- [ ] `E` interage com mundo quando houver alvo interagivel; se nao houver, usa RightHand.
- [ ] Acao de mao vem do item equipado na mao.
- [ ] Light attack funciona via WeaponDataSO.
- [ ] Heavy attack fica hook futuro.
- [ ] Dodge simples funciona com stamina da spec 09 e sem i-frames.
- [ ] Block/parry nao e implementado.
- [ ] Bow placeholder funciona sem ammo com range default 6.0.
- [ ] Spell basica `ArcaneBolt` funciona com mana.
- [ ] Mana inicia 100, regenera 5/s e salva/carrega.
- [ ] Active skill slots usam R/T/Y/G.
- [ ] SkillActionSO e usado apenas para skills/active slots.
- [ ] Skill tree/unlock/respec nao e implementado nesta spec.
- [ ] Cooldown so inicia se acao executou.
- [ ] AttackSpeed reduz cooldown de arma.
- [ ] Dano de arma/bow/spell/skill passa pela pipeline da spec 11.
- [ ] Mana bar e active slots aparecem na HUD minima.
- [ ] Save/load preserva mana e active skill slots.
- [ ] Invariantes anti-regressao preservadas.

---

## 15. Validacao

1. Sem arma equipada, atacar com fallback unarmed data-driven.
2. Equipar arma em LeftHand e usar Q.
3. Equipar arma/tool em RightHand e usar E sem interagivel em foco.
4. Com interagivel em foco, validar que E interage e nao ataca.
5. Validar que hotkeys 1-6 continuam funcionando.
6. Validar light melee attack com WeaponDataSO.
7. Validar cooldown reduzido por AttackSpeed.
8. Validar dodge com Space, custo de stamina e sem atravessar colisao.
9. Validar bow sem ammo, range 6.0 e projectile hit.
10. Castar ArcaneBolt com mana suficiente.
11. Tentar castar sem mana e validar que cooldown nao inicia.
12. Usar active slot R/T/Y/G com SkillActionSO.
13. Validar que SkillActionSO nao e usado para ataque normal/tool normal.
14. Validar damage/status/damage numbers via spec 11.
15. Salvar/carregar mana e active skill slots.
16. Validar Unity compile validation e docs validation.
