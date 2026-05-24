# SPEC - Player combat, weapons, spells e skill actions runtime

> Spec ID: spec_player_combat_weapons_spells_skill_actions_runtime
> Status: A implementar
> Ordem de execucao: 12
> Depende de: 00-11
> Bloqueia: 13, 14, 16, 17
> Tipo: Runtime/UI minima
> Fonte: docs/specs/ como fonte unica; fontes absorvidas listadas abaixo.
> Escopo: Substituir punch hardcoded por combate data-driven com armas equipadas nas maos, unarmed fallback, uso Q/E, dodge simples, bow placeholder, magia basica, mana, active skill slots R/T/Y/G e integracao com damage/stamina/equipment.
> Fora de escopo: skill tree/unlock/respec, combos avancados, block/parry, heavy attack real, ammo, i-frames finais, IA inimiga, VFX/animacoes finais, UI final consolidada, Packages, ProjectSettings e docs_old.

Fontes absorvidas:
- specs/FASE9I_PLAYER_COMBAT_WEAPONS_MAGIC_SKILL_TREES/spec.md
- specs/FASE9C_PLAYER_EQUIPMENT_ITEMS_COMBAT/spec.md
- docs/refinements/a_implementar/pre_refinamentos/refinamento_init_player_combat_weapons_spells_skill_actions.md

---

# /speckit.specify

## Contexto

O combate atual do jogador ainda e MVP/debug:

```text
PlayerAttackController
- tecla J
- dano fixo
- range fixo
- cooldown fixo
- overlap circle
```

Ja existem schemas iniciais como:

```text
WeaponDataSO
SpellDataSO
SkillActionSO
```

mas eles ainda nao dirigem o runtime principal.

Specs anteriores relevantes:

```text
09 - Hunger/stamina/status/time: stamina e custos base
10 - Equipment/durability/environment/loot: LeftHand/RightHand, tools em mao, AttackSpeed, ItemInstanceId
11 - Damage/status/elements/resistances: DamageCalculator, DamageType, status, damage numbers
```

Esta spec deve substituir o ataque hardcoded por uma camada de combate do jogador conectada a equipamento, stamina, mana, dano oficial e active skill slots.

## Pre-condicoes

Implementar runtime somente depois de specs 02-11 estarem realmente implementadas:

```text
02 - save schema migration
03 - inventory slots/capacity/painel de itens
04 - farm irrigacao/solo/menu contextual
05 - world activities/fishing/trees/pickups/loot
06 - economy/shop/dialogue modal
07 - crafting/workstations/modal stack
08 - town/npc/dialogue
09 - hunger/stamina/status/time
10 - equipment/durability/environment/loot
11 - damage/status/elements/resistances
```

Antes de alterar codigo, revalidar:

```text
Assets/_Game/Scripts/Combat/**
Assets/_Game/Scripts/Equipment/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Player/**
Assets/_Game/Scripts/Skills/**
Assets/_Game/Scripts/UI/HUD/**
Assets/_Game/Scripts/UI/Hotbar/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Data/Combat/**
Assets/_Game/Data/Skills/**
```

Se specs 10 e 11 nao estiverem implementadas, nao criar formula paralela de equipment/damage. Registrar bloqueio.

## Problema

Gaps atuais:

- ataque principal usa dano/range/cooldown hardcoded;
- arma equipada nao dirige o ataque;
- maos `LeftHand`/`RightHand` ainda nao resolvem a acao de combate;
- nao ha mana;
- spells nao tem caster runtime real;
- active skill slots nao estao conectados a runtime;
- cooldown/stamina/mana ainda nao impedem spam de forma unificada;
- dodge/roll ainda nao existe como acao minima;
- bow/ranged ainda nao existe de forma data-driven;
- ataques nao passam consistentemente pelo `DamageCalculator` da spec 11.

## Objetivo

Implementar combate player MVP com:

- ataque sem arma via `UnarmedAttackDataSO` fallback;
- uso das maos por `Q` e `E`;
- `E` preservando prioridade de interacao quando houver objeto/NPC/tile interagivel;
- arma/tool equipada nas maos como fonte da acao;
- light attack como ataque real MVP;
- heavy attack como hook futuro;
- dodge simples com stamina, sem i-frames no MVP;
- bow/ranged placeholder sem ammo;
- spell basica com mana;
- active skill slots `R`, `T`, `Y`, `G` apenas para skills;
- cooldown, stamina e mana como gates antes da execucao;
- toda aplicacao de dano passando pelo pipeline da spec 11;
- mana bar minima na HUD;
- save/load de mana e active skill slots.

## Decisoes aprovadas

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
- Bow default range deve ser configuravel; valor inicial: `6.0` world units.
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

## Inputs e prioridade

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
- Se algum desses inputs ja estiver ocupado no runtime atual, registrar conflito e criar mapping configuravel, mas manter a intencao da spec.

## Maos e uso normal

### LeftHand / Q

- `Q` executa a acao primaria do item equipado na `LeftHand`.
- Se `LeftHand` esta vazia, pode executar fallback unarmed apenas se a regra de design permitir ou retornar feedback simples.
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
- Fallback pode ter dano/range/cooldown/stamina reduzidos.

## Weapon combat

### WeaponDataSO minimo

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
- Melee light usa Dexterity ou Strength conforme weight class da spec 10.
- Melee heavy/weapon pesada usa Strength.
- Tudo passa pelo `DamageCalculator` da spec 11.

### Bow placeholder

Valores default iniciais:

```text
DefaultBowRange = 6.0 world units
DefaultBowProjectileSpeed = 10.0 world units/second
DefaultBowDamageType = Physical
DefaultBowAmmoRequired = false
```

Regras:

- Projectile deve desaparecer ao atingir alvo, parede/obstaculo ou ao exceder range.
- Sem friendly fire.
- Sem ammo no MVP.
- Ammo fica hook futuro.

## Dodge simples

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
- Sem perfect dodge no MVP.

## Mana e spells

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
- Mana HUD minima deve refletir valor real.

### SpellDataSO minimo

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

MVP:

```text
CastTimeSeconds = 0
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

## Skill actions e active slots

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
- Se skill nao estiver disponivel/desbloqueada no runtime atual, slot fica vazio ou bloqueado com feedback.

### SkillActionSO minimo

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

Regras:

- SkillActionSO e wrapper/executor de skills ativas.
- Nao usar SkillActionSO para attack normal, tool use normal ou bow normal.
- Cooldown da skill e independente do cooldown de arma, salvo skill explicitamente vinculada.
- Skill usa stamina/mana conforme dados.
- Sem recurso suficiente, skill nao executa e cooldown nao inicia.

## Cooldown e AttackSpeed

Regras:

- Cooldown comeca somente se a acao executou com sucesso.
- Weapon cooldown usa AttackSpeed derivado da spec 10:

```text
effectiveWeaponCooldown = WeaponDataSO.BaseCooldownSeconds / FinalAttackSpeed
```

- Spell cooldown nao usa AttackSpeed.
- Skill cooldown nao usa AttackSpeed, salvo skill explicitamente marcada como weapon skill no futuro.
- Cooldown pausa junto com gameplay/time pause da spec 09 quando modal/pause estiver ativo.

## Hit detection e facing

Melee:

```text
Overlap arc/circle na direcao do facing/input
Range e ArcDegrees vêm de WeaponDataSO
```

Projectile:

```text
Spawn na posicao do player/hand origin
Direcao = facing/input
Range configuravel
Speed configuravel
Hit usa DamageCalculator
```

Regras:

- Player atinge inimigos.
- Player nao atinge NPCs, shops, workstations, farm plots ou pickups por dano de combate no MVP.
- Inimigos atingem player por seus proprios sistemas futuros.
- Sem friendly fire.

## Damage/status integration

- Todo hit de arma, bow, spell e skill de dano cria `DamageRequest` da spec 11.
- `DamageCalculator` resolve dano.
- DamageAppliedEvent/DamageNumberRequestedEvent sao responsabilidade da spec 11.
- Esta spec nao reimplementa damage numbers.
- StatusApplicationRules de WeaponDataSO, SpellDataSO ou SkillActionSO sao repassadas ao StatusEffectManager da spec 11.

## HUD minima

A spec 09 definiu:

```text
Hunger bar
Stamina bar
Status abaixo
```

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

Regras:

- HUD nao e modal.
- HUD nao pausa tempo.
- HUD nao substitui UI final da spec 17.
- HUD deve refletir runtime real.

## Save/load

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

## Eventos

Usar existentes se houver equivalentes. Criar somente se necessario:

```text
PlayerHandActionRequestedEvent
PlayerWeaponAttackStartedEvent
PlayerWeaponAttackResolvedEvent
PlayerDodgeStartedEvent
PlayerDodgeEndedEvent
SpellCastRequestedEvent
SpellCastSucceededEvent
SpellCastFailedEvent
ManaChangedEvent
SkillActionRequestedEvent
SkillActionExecutedEvent
SkillActionFailedEvent
ActiveSkillSlotChangedEvent
```

Nao duplicar eventos de damage/status/equipment se ja existirem.

## Invariantes anti-regressao

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

Se uma integracao ainda nao existir, registrar bloqueio/pendencia clara em vez de criar sistema paralelo.

## Criterios de aceite

- Punch hardcoded nao e ataque principal.
- Sem arma equipada, `UnarmedAttackDataSO` executa fallback data-driven.
- `Q` usa LeftHand.
- `E` interage com mundo quando houver alvo interagivel; se nao houver, usa RightHand.
- Acao de mao vem do item equipado na mao.
- Light attack funciona via WeaponDataSO.
- Heavy attack fica hook futuro.
- Dodge simples funciona com stamina da spec 09 e sem i-frames.
- Block/parry nao e implementado.
- Bow placeholder funciona sem ammo com range default 6.0.
- Spell basica `ArcaneBolt` funciona com mana.
- Mana inicia 100, regenera 5/s e salva/carrega.
- Active skill slots usam R/T/Y/G.
- SkillActionSO e usado apenas para skills/active slots.
- Skill tree/unlock/respec nao e implementado nesta spec.
- Cooldown so inicia se acao executou.
- AttackSpeed reduz cooldown de arma.
- Dano de arma/bow/spell/skill passa pela pipeline da spec 11.
- Mana bar e active slots aparecem na HUD minima.
- Save/load preserva mana e active skill slots.
- Nenhum item das invariantes anti-regressao e quebrado.
- Validacao documental e Unity compile validation registradas.

---

# /speckit.plan

## Arquitetura

Sistemas afetados:

```text
Assets/_Game/Scripts/Combat/**
Assets/_Game/Scripts/Combat/Player/**
Assets/_Game/Scripts/Combat/Weapon/**
Assets/_Game/Scripts/Combat/Magic/**
Assets/_Game/Scripts/Combat/Skills/**
Assets/_Game/Scripts/Equipment/**
Assets/_Game/Scripts/Player/**
Assets/_Game/Scripts/UI/HUD/**
Assets/_Game/Scripts/UI/Hotbar/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Data/Combat/**
Assets/_Game/Data/Skills/**
```

Controllers Unity devem ser bridges finas. Resolucao de input, recursos, cooldown, DamageRequest e SkillAction execution devem ficar em servicos/classes testaveis quando possivel.

## Ordem segura de implementacao

1. Revalidar PlayerAttackController, EquipmentManager, DamageCalculator, StaminaManager e HUD atual.
2. Confirmar specs 02-11 implementadas antes de runtime.
3. Criar/consolidar WeaponDataSO e UnarmedAttackDataSO.
4. Criar PlayerCombatController resolvendo Q/E e maos.
5. Implementar light melee attack via WeaponDataSO.
6. Implementar dodge simples.
7. Implementar bow projectile placeholder sem ammo.
8. Criar ManaManager e SpellDataSO/PlayerSpellCaster.
9. Criar ArcaneBolt testavel.
10. Criar SkillActionSO runtime apenas para active slots.
11. Implementar active slots R/T/Y/G e save/load.
12. Integrar tudo ao DamageCalculator da spec 11.
13. Atualizar HUD com Mana e active slots.
14. Validar anti-regressao e atualizar tracking.

## Fluxos

### Usar mao esquerda

```text
Input Q
Se modal aberto, ignorar gameplay input
Resolver LeftHand equipado
Se weapon/tool valido, executar acao primaria
Se vazio/invalido, executar UnarmedAttackDataSO ou feedback
Validar stamina/cooldown
Criar DamageRequest se ataque
```

### Usar mao direita/interagir

```text
Input E
Se modal aberto, seguir modal
Se ha interagivel em foco, executar interacao
Senao resolver RightHand equipado
Executar acao primaria como mao esquerda
```

### Cast spell

```text
Solicitar SpellDataSO
Validar mana/cooldown
Consumir mana
Criar projectile ou efeito
No hit, criar DamageRequest
```

### Active skill slot

```text
Input R/T/Y/G
Resolver SkillActionId no slot
Validar skill disponivel, recurso e cooldown
Executar SkillActionSO
Publicar eventos
```

## Riscos de regressao

- Quebrar E como interacao.
- Quebrar hotkeys 1-6.
- Criar segundo sistema de dano fora da spec 11.
- Criar segundo sistema de equipamento fora da spec 10.
- SkillActionSO virar wrapper de tudo e confundir uso normal.
- Mana/HUD conflitar com stamina/status da spec 09.
- Dodge atravessar colisao ou funcionar sem stamina.

## Mitigacao

- Prioridade clara de input.
- DamageCalculator unico.
- EquipmentManager como fonte unica de maos.
- SkillActionSO restrito a active skill slots.
- Gates de recurso antes da execucao.
- Cooldown inicia so no sucesso.
- Modal/time pause respeitado.

---

# /speckit.tasks

## Tasks

- [ ] Revalidar estado real de Combat/Equipment/Damage/Player/HUD antes de alterar runtime.
- [ ] Confirmar specs 02-11 implementadas antes de runtime.
- [ ] Criar/ajustar `UnarmedAttackDataSO`.
- [ ] Criar/ajustar `WeaponDataSO` para light attack real.
- [ ] Criar/ajustar `PlayerCombatController`.
- [ ] Implementar input Q para LeftHand.
- [ ] Implementar input E com prioridade de interacao e fallback RightHand.
- [ ] Implementar melee light attack por WeaponDataSO.
- [ ] Implementar dodge simples com stamina da spec 09.
- [ ] Implementar bow projectile placeholder sem ammo, range 6.0.
- [ ] Criar `ManaManager` e eventos de mana.
- [ ] Criar/ajustar `SpellDataSO`.
- [ ] Implementar `PlayerSpellCaster`.
- [ ] Criar spell testavel `ArcaneBolt`.
- [ ] Criar/ajustar `SkillActionSO` apenas para active slots.
- [ ] Implementar active slots R/T/Y/G.
- [ ] Implementar save/load de mana e active slots.
- [ ] Integrar todos os hits com `DamageCalculator`.
- [ ] Atualizar HUD minima com Mana e active slots.
- [ ] Atualizar docs implementados/status/registries/refinements/log.

## Arquivos permitidos

```text
Assets/_Game/Scripts/Combat/**
Assets/_Game/Scripts/Equipment/**
Assets/_Game/Scripts/Player/**
Assets/_Game/Scripts/UI/HUD/**
Assets/_Game/Scripts/UI/Hotbar/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Data/Combat/**
Assets/_Game/Data/Skills/**
docs/specs/**
docs/refinements/**
docs/IMPLEMENTATION_STATUS.md
PROJECT_LOG.md
```

## Arquivos proibidos

```text
docs_old/**
Packages/**
ProjectSettings/**
```

## Definition of Done

- Player combat data-driven funcionando.
- Q/E resolvem maos e preservam interacao.
- Dodge, bow placeholder, spell basica e active skill slots funcionam.
- Mana e active slots persistem.
- Dano passa pelo pipeline da spec 11.
- Invariantes anti-regressao preservadas.
- Validacao documental e Unity registrada.

## Validacao obrigatoria

Rodar:

```powershell
.\tools\docs\validate_docs.ps1
.\tools\unity\RunUnityCompileValidation.ps1
.\tools\unity\ScanUnityLogs.ps1
```

Play Mode minimo:

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
