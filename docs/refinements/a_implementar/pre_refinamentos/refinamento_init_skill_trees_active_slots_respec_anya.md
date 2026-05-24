# refinamento_init_skill_trees_active_slots_respec_anya

> Status: Refinamento inicial a implementar
> Spec futura relacionada: `docs/specs/a_implementar/spec_skill_trees_active_slots_respec_anya_runtime.md`
> Objetivo: completar skill trees, skill points, active slots R/T/Y/G, skills passivas, skills equipaveis, capstones, save/load, modal de skill tree por tecla K e respec na Fonte de Anya. Inclui arvores expandidas de Melee, Ranged, Magic, Survival e Crafting.

---

## 1. Estado atual

A regra de SkillPoint foi estabilizada:

```text
+1 SkillPoint em niveis pares, comecando no nivel 2.
```

A spec 12 definiu active skill slots:

```text
R
T
Y
G
```

A spec 15 definiu a Fonte de Anya como ponto de respawn e deixou respec futuro bloqueado. Esta spec libera o respec na Fonte de Anya e completa a progressao de skill trees.

Nao foi localizada no repo, via search, a lista antiga completa de skills inspiradas em D&D/feats/classes. Portanto a spec cria um catalogo inicial original, inspirado em arquetipos classicos de fantasia tabletop, classes e feats de D&D em nivel conceitual, sem copiar nomes proprietarios, textos, regras oficiais, statblocks ou conteudo protegido.

---

## 2. Decisoes aprovadas

- SkillPoint:

```text
+1 em niveis pares, comecando no nivel 2.
Sem SkillPoint no nivel 1.
```

- Active slots continuam:

```text
R
T
Y
G
```

- Tecla para abrir modal de skill tree:

```text
K
```

- Dentro do modal, gameplay input fica bloqueado e o modal respeita modal stack.
- Existem dois grupos de skill:

```text
PassiveSkill: efeito sempre ativo apos comprar, nao equipa em active slot.
EquippableSkill: precisa estar em active slot R/T/Y/G para ser usada.
```

- `SkillActionSO` e usado apenas para skills/active slots, como definido na spec 12.
- Uso normal de arma/tool por Q/E nao usa SkillActionSO.
- Active slot salva `SkillActionId`.
- Skills passivas nunca precisam equipar.
- Skills equipaveis sao acoes ativas e ocupam R/T/Y/G.
- Basic block/parry normal ainda nao vira input global de combate; block aqui entra como skill equipavel/temporaria para nao contradizer a spec 12.
- Respec so funciona na Fonte de Anya.
- Full respec apenas no MVP; respec parcial por arvore fica fora.
- Primeiro respec e gratuito.
- Respecs seguintes custam gold configuravel.
- Valor inicial sugerido:

```text
RespecCostGold = 250
```

- Respec nao altera level, XP, inventory, equipment, cave state, corpse state ou checkpoints.

---

## 3. Arvores iniciais

Criar 5 arvores:

```text
Melee
Ranged
Magic
Survival
Crafting
```

Cada arvore MVP:

```text
10 nodes comuns
1 capstone
```

Total MVP:

```text
55 nodes
```

Regras:

- Cada node comum custa 1 SkillPoint.
- Capstone custa 1 SkillPoint.
- Capstone exige 8 nodes comprados na mesma arvore + prerequisites diretos.
- Futuras expansoes podem adicionar mais nodes sem quebrar save, desde que `SkillNodeId` seja estavel.

---

## 4. Tipos de node e categorias

Tipos:

```text
PassiveStat
PassiveModifier
UnlockSkillAction
UpgradeSkillAction
UnlockSpell
Capstone
```

Categorias funcionais:

```text
PassiveSkill
EquippableSkill
CapstonePassive
CapstoneEquippable opcional futuro
```

Regras:

- Passive skills aplicam modificadores automaticamente apos compra.
- Equippable skills desbloqueiam `SkillActionId` para active slots.
- UpgradeSkillAction modifica uma skill ja desbloqueada, se runtime suportar.
- UnlockSpell deve preferir wrapper via `SkillActionSO.LinkedSpellId`, nao equipar `SpellDataSO` diretamente.

---

## 5. Skill data

Criar/usar:

```text
SkillTreeDataSO
SkillNodeDataSO
SkillActionSO
SkillTreeRegistrySO
SkillPassiveModifierSO opcional
```

`SkillActionSO` deve suportar campos opcionais para os novos action shapes:

```text
ChargeTimeSeconds opcional
AreaShape opcional
ProjectileCount opcional
ProjectileSpreadDegrees opcional
LinePierceCount opcional
BlockDurationSeconds opcional
DashDistance opcional
LeapDistance opcional
```

---

## 6. Catalogo inicial de skills

A lista e original para Cindar's Hope, inspirada em arquetipos de classes/feats de fantasia tabletop, sem copiar nomes/regras oficiais.

### Melee tree

Objetivo: suportar uma arma, dual wield, two-handed, block, dodge, dash e leap attack.

```text
melee_iron_grip: PassiveSkill, Attack +1 com melee
melee_guarded_stance: PassiveSkill, Defense +1
melee_dual_wield_flow: PassiveSkill, bonus leve de AttackSpeed com duas armas leves
melee_offhand_cut: EquippableSkill, ataque curto usando offhand/LeftHand; fallback corte rapido
melee_two_handed_momentum: PassiveSkill, bonus de dano/cooldown para armas pesadas/two-handed
melee_guarded_block: EquippableSkill, block temporario que reduz proximo dano por curta janela
melee_battle_dash: EquippableSkill, dash curto na direcao do facing, com stamina cost, sem i-frame completo
melee_leap_attack: EquippableSkill, leap attack curto, dano em pequeno impacto/arc, respeitando colisao
melee_whirl_cut: EquippableSkill, ataque circular curto ao redor do player
melee_dodge_training: PassiveSkill, melhora levemente cooldown/custo de dodge via hook seguro
melee_capstone_battle_rhythm: CapstonePassive, reduz levemente cooldown melee apos hit/kill
```

### Ranged tree

Objetivo: suportar arco com charge, tiros de linha reta, piercing, multishot e mobilidade de arqueiro.

```text
ranged_steady_hand: PassiveSkill, BowDamageFlat +1
ranged_long_sight: PassiveSkill, BowRange +0.5
ranged_quick_nock: PassiveSkill, melhora cooldown de bow via ranged hook
ranged_charged_shot: EquippableSkill, charge antes de soltar; dano/range maior se carregar completo
ranged_line_piercer: EquippableSkill, disparo reto que perfura e acerta todos em linha ate limite de range/pierce
ranged_multishot_fan: EquippableSkill, 3 flechas: centro + duas diagonais, spread configuravel
ranged_bleeding_arrow: EquippableSkill, aplica Bleed via spec 11
ranged_kiting_steps: PassiveSkill, pequeno bonus de MoveSpeed apos disparo
ranged_marked_prey: EquippableSkill, marca alvo por curta duracao; proximos disparos causam bonus leve, se suportado
ranged_projectile_tuning: PassiveSkill, projectile speed +1 ou hook equivalente
ranged_capstone_eagle_focus: CapstonePassive, BowRange +1, projectile speed +1 e bonus leve em skills ranged
```

### Magic tree

Objetivo: suportar magias elementais alinhadas a DamageType da spec 11: Fire, Ice, Toxic, Lightning e Arcane.

```text
magic_mana_well: PassiveSkill, MaxMana +10
magic_quick_channel: PassiveSkill, ManaRegen +1/s ou hook equivalente
magic_arcane_edge: PassiveSkill, ArcaneDamageFlat +1
magic_fire_spark: EquippableSkill/UnlockSpell, Fire projectile simples
magic_ice_bind: EquippableSkill/UnlockSpell, Ice com dano leve e Slow
magic_toxic_cloud: EquippableSkill/UnlockSpell, pequena area Toxic/Poison por tick, se suportado
magic_lightning_chain: EquippableSkill/UnlockSpell, Lightning salta para alvo proximo, fallback single target
magic_arcane_bolt_mastery: PassiveSkill/UpgradeSkillAction, melhora ArcaneBolt da spec 12
magic_elemental_ward: EquippableSkill, buff temporario de resistencia Fire/Ice/Toxic/Lightning leve
magic_slowing_sigils: EquippableSkill, aplica Slow em area pequena
magic_capstone_elemental_confluence: CapstonePassive, bonus leve para dano elemental e mana regen; sem combo elemental avancado
```

### Survival tree

Objetivo: sobreviver melhor em cave, ambientes hostis e longas runs. Conversa com EnvironmentalResistance da spec 10 e DamageType/status da spec 11.

```text
survival_cave_lungs: PassiveSkill, MaxStamina +10
survival_hard_skin: PassiveSkill, MaxHP +5
survival_low_rations: PassiveSkill, reduz HungerDrain via hook da spec 09
survival_toxic_sense: PassiveSkill, ToxicResistance +1 ambiental
survival_cold_habit: PassiveSkill, ColdResistance +1 ambiental
survival_heat_temper: PassiveSkill, HeatResistance +1 ambiental
survival_status_recovery: PassiveSkill, reduz duracao de Poison/Burn/Slow em valor leve, hook seguro
survival_safe_step: PassiveSkill, reduz penalidade de terreno/hazard leve, se existir
survival_emergency_roll: EquippableSkill, dodge/utility especial; fallback buff curto de MoveSpeed
survival_last_breath: EquippableSkill, cura/escudo pequeno emergencial com cooldown alto, sem evitar morte automaticamente
survival_capstone_caveborn: CapstonePassive, bonus leve em Toxic/Cold/HeatResistance e MaxStamina
```

### Crafting tree

Objetivo: melhorar crafting, reparo, utilidade de campo e preparar hooks de economia/craft sem quebrar specs anteriores.

```text
crafting_fast_hands: PassiveSkill, CraftTime reduction leve
crafting_repair_care: PassiveSkill, RepairKit recupera um pouco mais, hook da spec 10
crafting_material_eye: PassiveSkill, pequena chance futura de bonus de recurso, hook sem prometer drop extra agora
crafting_field_patch: EquippableSkill, pequeno reparo/utility, se suportado
crafting_station_focus: PassiveSkill, bonus de craft em workstation, hook da spec 07
crafting_pack_order: PassiveSkill, hook para futura expansao/organizacao, sem aumentar capacidade se spec 03 nao suportar
crafting_quick_repair: EquippableSkill, acao de reparo rapido com material/RepairKit, se suportado
crafting_salvage_method: PassiveSkill, hook para melhor retorno ao destruir/salvage item no futuro
crafting_durable_finish: PassiveSkill, pequeno bonus de DurabilityMax para crafted gear futuro
crafting_shop_sense: PassiveSkill, hook para leve bonus de venda/compra futuro, sem alterar economy se nao suportado
crafting_capstone_master_artisan: CapstonePassive, craft time menor e repair efficiency maior
```

---

## 7. Active slots

Active slots sao definidos pela spec 12:

```text
Slot 0 = R
Slot 1 = T
Slot 2 = Y
Slot 3 = G
```

Regras:

- Active slot aceita apenas `EquippableSkill` desbloqueada.
- PassiveSkill nao aparece como equipavel.
- CapstonePassive nao aparece como equipavel.
- Active slot persiste `SkillActionId`.
- Se o skill action salvo nao existir ou nao estiver mais desbloqueado, limpar slot no load e logar warning.

---

## 8. Modal SkillTreePanel

Acesso:

```text
K = abrir/fechar SkillTreePanel
```

Navegacao minima:

```text
W/A/S/D = mover selecao entre nodes
Q/E = trocar aba/arvore dentro do modal
Enter ou E = comprar/confirmar node dentro do modal
R/T/Y/G = escolher active slot quando em modo de equipar skill
Esc ou K = fechar/voltar
```

Observacao:

- Dentro do modal, `Q/E` nao usam maos do player.
- Fora do modal, `Q/E` continuam seguindo spec 12.

---

## 9. Purchase rules

Compra valida se:

```text
Node ainda nao comprado
SkillPoint disponivel >= SkillPointCost
PrerequisiteNodeIds comprados
MinimumPlayerLevel atendido, se definido
RequiredPurchasedNodesInTree atendido, se definido
```

---

## 10. Capstones

Regra MVP:

```text
Cada arvore tem 1 capstone.
Capstone exige pelo menos 8 nodes comprados na mesma arvore + prerequisites diretos.
Capstone custa 1 SkillPoint.
```

---

## 11. Respec na Fonte de Anya

Regras:

- Fonte de Anya mostra opcao de respec desbloqueada apos esta spec.
- Primeiro respec gratuito.
- A partir do segundo, custo em gold configuravel, valor inicial 250.
- Respec e full respec.
- Respec remove todos purchased nodes.
- Respec remove passives aplicados.
- Respec recalcula SkillPoints disponiveis com base no level atual.
- Respec limpa active slots que usam SkillActionId nao desbloqueado.
- Respec nao altera level, XP, inventory, equipment, corpse, cave state, checkpoints ou boss gates.

---

## 12. Derived stats e passives

Exemplos de modifiers:

```text
AttackFlat
DefenseFlat
MaxHPFlat
MaxStaminaFlat
MaxManaFlat
ManaRegenFlat
BowRangeFlat
BowDamageFlat
BowProjectileSpeedFlat
CraftTimeReductionFlatOrPercent
RepairEfficiencyBonus
HungerDrainReduction
EnvironmentalResistanceBonus
DualWieldAttackSpeedBonus
TwoHandedDamageBonus
DodgeCostReduction
StatusDurationReduction
```

---

## 13. Definition of Done

- [ ] SkillPoint e concedido em niveis pares, comecando no nivel 2.
- [ ] Modal SkillTreePanel abre/fecha com `K`.
- [ ] Existem 5 arvores iniciais: Melee, Ranged, Magic, Survival, Crafting.
- [ ] Cada arvore possui 10 nodes comuns + 1 capstone.
- [ ] Existem skills passivas que nao precisam equipar.
- [ ] Existem skills equipaveis que podem ocupar active slots R/T/Y/G.
- [ ] Melee possui suporte data-driven para dual wield, two-handed, block temporario, dodge/dash, leap attack e ataque circular.
- [ ] Ranged possui suporte data-driven para charged shot, line piercer e multishot de 3 flechas.
- [ ] Magic possui suporte data-driven para Fire, Ice, Toxic, Lightning e Arcane.
- [ ] Survival possui suporte data-driven para Toxic/Cold/Heat resistance, status recovery e cave survival.
- [ ] PassiveSkill aplica efeito automaticamente apos compra.
- [ ] Active slot aceita apenas EquippableSkill desbloqueada.
- [ ] Compra valida custo, prerequisites e level minimo quando aplicavel.
- [ ] Capstone exige 8 nodes da arvore + prerequisites diretos.
- [ ] Respec full funciona somente na Fonte de Anya.
- [ ] Primeiro respec e gratuito; seguintes usam custo configuravel inicial de 250 gold.
- [ ] Respec nao altera level, XP, inventory, equipment, corpse, cave state, checkpoints ou boss gates.
- [ ] Save/load preserva PurchasedNodeIds, ActiveSkillSlots e RespecCount.
- [ ] Derived stats recalculam em purchase/respec/load.
- [ ] Invariantes anti-regressao preservadas.

---

## 14. Validacao

1. Subir para level 2 e validar +1 SkillPoint.
2. Subir para level 3 e validar que nao ganha SkillPoint.
3. Subir para level 4 e validar +1 SkillPoint.
4. Abrir/fechar SkillTreePanel com K.
5. Comprar PassiveSkill e validar efeito automatico sem equipar.
6. Comprar EquippableSkill e equipar em R/T/Y/G.
7. Validar Melee: dual wield passive, two-handed passive, guarded block, battle dash e leap attack.
8. Validar Ranged: charged shot, line piercer e multishot de 3 flechas.
9. Validar Magic: Fire, Ice, Toxic, Lightning e Arcane usam DamageType correto da spec 11.
10. Validar Survival: Toxic/Cold/HeatResistance alteram derived stats/environment hooks.
11. Validar capstone bloqueado antes de 8 nodes da arvore.
12. Comprar 8 nodes e validar capstone disponivel.
13. Salvar/carregar purchased nodes, passives e active slots.
14. Fazer primeiro respec gratuito na Fonte de Anya.
15. Fazer segundo respec com custo configuravel ou validar bloqueio por gold insuficiente.
16. Validar que respec nao altera level, XP, inventory, equipment, corpse, cave state ou checkpoints.
17. Validar que Q/E dentro do modal nao acionam maos/interacao do player.
18. Validar Unity compile validation e docs validation.
