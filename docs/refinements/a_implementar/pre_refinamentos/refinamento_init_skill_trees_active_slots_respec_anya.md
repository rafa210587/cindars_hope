# refinamento_init_skill_trees_active_slots_respec_anya

> Status: Refinamento inicial a implementar
> Spec futura relacionada: `docs/specs/a_implementar/spec_skill_trees_active_slots_respec_anya_runtime.md`
> Objetivo: completar skill trees, skill points, active slots R/T/Y/G, skills passivas, skills equipaveis, capstones, save/load, modal de skill tree por tecla K e respec na Fonte de Anya.

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

Nao foi localizada no repo, via search, a lista antiga completa de skills inspiradas em D&D/feats/classes. Portanto a spec cria um catalogo inicial original, inspirado em arquetipos classicos de fantasia tabletop, sem copiar nomes proprietarios, textos, regras oficiais ou conteudo protegido.

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
6 nodes comuns
1 capstone
```

Total MVP:

```text
35 nodes
```

Regras:

- Cada node comum custa 1 SkillPoint.
- Capstone custa 1 SkillPoint.
- Capstone exige 5 nodes comprados na mesma arvore + prerequisites diretos.
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

`SkillTreeDataSO` minimo:

```text
TreeId
DisplayName
Description
NodeIds[]
CapstoneNodeId
```

`SkillNodeDataSO` minimo:

```text
SkillNodeId
TreeId
DisplayName
Description
NodeType
SkillCategory
SkillPointCost
MinimumPlayerLevel opcional
PrerequisiteNodeIds[]
RequiredPurchasedNodesInTree opcional
UnlockedSkillActionId opcional
LinkedSpellId opcional
PassiveModifiers[]
IsCapstone
```

---

## 6. Catalogo inicial de skills

A lista e original para Cindar's Hope, inspirada em arquetipos de classes/feats de fantasia tabletop, sem copiar nomes/regras oficiais.

### Melee tree

```text
melee_iron_grip: PassiveSkill, Attack +1 com melee
melee_guarded_stance: PassiveSkill, Defense +1
melee_battle_stride: PassiveSkill, pequeno bonus de MoveSpeed durante combate
melee_heavy_training: PassiveSkill, melhora AttackSpeed de armas pesadas via hook
melee_heavy_strike: EquippableSkill, ataque melee forte com stamina cost
melee_shieldless_resolve: PassiveSkill, MaxHP +5 quando sem offhand defensiva, hook seguro
melee_capstone_battle_rhythm: CapstonePassive, reduz levemente cooldown melee apos hit/kill, hook com fallback passivo
```

### Ranged tree

```text
ranged_steady_hand: PassiveSkill, BowDamageFlat +1
ranged_long_sight: PassiveSkill, BowRange +0.5
ranged_quick_nock: PassiveSkill, melhora cooldown de bow via ranged hook
ranged_bleeding_arrow: EquippableSkill, aplica Bleed via spec 11
ranged_piercing_shot: EquippableSkill, projectile atravessa 1 alvo se suportado; fallback dano maior single target
ranged_kiting_steps: PassiveSkill, pequeno bonus de MoveSpeed apos disparo, hook seguro
ranged_capstone_eagle_focus: CapstonePassive, BowRange +1, projectile speed +1 e bonus leve em skills ranged
```

### Magic tree

```text
magic_mana_well: PassiveSkill, MaxMana +10
magic_quick_channel: PassiveSkill, ManaRegen +1/s ou hook equivalente
magic_arcane_edge: PassiveSkill, ArcaneDamageFlat +1
magic_arcane_bolt_mastery: PassiveSkill, melhora ArcaneBolt da spec 12
magic_ward_pulse: EquippableSkill, pequeno pulso defensivo/arcano
magic_slowing_sigils: EquippableSkill, aplica Slow em area pequena
magic_capstone_anya_spark: CapstonePassive, MaxMana +15 e bonus de dano Arcane
```

### Survival tree

```text
survival_cave_lungs: PassiveSkill, MaxStamina +10
survival_hard_skin: PassiveSkill, MaxHP +5
survival_low_rations: PassiveSkill, reduz HungerDrain via hook da spec 09
survival_toxic_sense: PassiveSkill, ToxicResistance +1 ambiental
survival_cold_habit: PassiveSkill, ColdResistance +1 ambiental
survival_emergency_roll: EquippableSkill, dodge/utility especial; fallback buff curto de MoveSpeed
survival_capstone_caveborn: CapstonePassive, bonus leve em resistencias ambientais e MaxStamina
```

### Crafting tree

```text
crafting_fast_hands: PassiveSkill, CraftTime reduction leve
crafting_repair_care: PassiveSkill, RepairKit recupera um pouco mais, hook da spec 10
crafting_material_eye: PassiveSkill, pequena chance futura de bonus de recurso, hook sem prometer drop extra agora
crafting_field_patch: EquippableSkill, pequeno reparo/utility, se suportado
crafting_station_focus: PassiveSkill, bonus de craft em workstation, hook da spec 07
crafting_pack_order: PassiveSkill, hook para futura expansao/organizacao, sem aumentar capacidade se spec 03 nao suportar
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

Regras:

- `K` abre modal/painel interativo de skill tree em gameplay normal.
- Se outro modal prioritario estiver aberto, respeitar modal stack.
- `K` pode fechar o painel se ele estiver no topo da stack.
- Modal pausa tempo/logica de gameplay conforme spec 09.
- O painel deve refletir estado real de SkillPoints, nodes comprados, prerequisites, active slots e respec availability.

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

Regras:

- Compra publica evento.
- Compra aplica passives imediatamente se for PassiveSkill/CapstonePassive.
- Compra desbloqueia SkillAction se for EquippableSkill.
- Compra falha sem gastar ponto se qualquer validacao falhar.
- Nao permitir comprar node duplicado.

---

## 10. Capstones

Regra MVP:

```text
Cada arvore tem 1 capstone.
Capstone exige pelo menos 5 nodes comprados na mesma arvore + prerequisites diretos.
Capstone custa 1 SkillPoint.
```

---

## 11. Respec na Fonte de Anya

Respec so pode ocorrer na Fonte de Anya da spec 15.

Criar/usar:

```text
SkillRespecService
AnyaSkillRespecOption
```

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
- Se gold insuficiente, respec nao ocorre.

---

## 12. Derived stats e passives

Passives devem integrar com o pipeline de stats derivados da spec 10 e combat/mana/stamina das specs 09/12.

Regras:

- Derived stats recalculam ao comprar node, fazer respec e carregar save.
- Passives devem ser aplicados por IDs/modifiers, nao por estado hardcoded em controller.
- Se um modifier aponta para stat ainda nao implementado, registrar hook/pendencia e nao quebrar runtime.
- PassiveSkill nao precisa equipar.

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
CraftTimeReductionFlatOrPercent
RepairEfficiencyBonus
HungerDrainReduction
EnvironmentalResistanceBonus
```

---

## 13. Save/load

Persistir IDs e tipos simples:

```text
SkillTreeSaveData
- PurchasedNodeIds[]
- ActiveSkillSlots[]
- RespecCount

ActiveSkillSlotSaveData
- SlotIndex
- InputKey
- SkillActionId opcional
```

Regras:

- AvailableSkillPoints e SpentSkillPoints podem ser derivados no load.
- Se `SkillNodeId` salvo nao existir, ignorar com warning e nao gastar ponto.
- Se `SkillActionId` salvo nao existir ou nao estiver desbloqueado, limpar slot.
- Nunca serializar SkillTreeDataSO, SkillNodeDataSO, SkillActionSO, GameObject, Transform, MonoBehaviour, Sprite, Collider ou Rigidbody.

---

## 14. Definition of Done

- [ ] SkillPoint e concedido em niveis pares, comecando no nivel 2.
- [ ] Modal SkillTreePanel abre/fecha com `K`.
- [ ] Existem 5 arvores iniciais: Melee, Ranged, Magic, Survival, Crafting.
- [ ] Cada arvore possui 6 nodes comuns + 1 capstone.
- [ ] Existem skills passivas que nao precisam equipar.
- [ ] Existem skills equipaveis que podem ocupar active slots R/T/Y/G.
- [ ] PassiveSkill aplica efeito automaticamente apos compra.
- [ ] Active slot aceita apenas EquippableSkill desbloqueada.
- [ ] Compra valida custo, prerequisites e level minimo quando aplicavel.
- [ ] Capstone exige 5 nodes da arvore + prerequisites diretos.
- [ ] Respec full funciona somente na Fonte de Anya.
- [ ] Primeiro respec e gratuito; seguintes usam custo configuravel inicial de 250 gold.
- [ ] Respec nao altera level, XP, inventory, equipment, corpse, cave state, checkpoints ou boss gates.
- [ ] Save/load preserva PurchasedNodeIds, ActiveSkillSlots e RespecCount.
- [ ] Derived stats recalculam em purchase/respec/load.
- [ ] Invariantes anti-regressao preservadas.

---

## 15. Validacao

1. Subir para level 2 e validar +1 SkillPoint.
2. Subir para level 3 e validar que nao ganha SkillPoint.
3. Subir para level 4 e validar +1 SkillPoint.
4. Abrir/fechar SkillTreePanel com K.
5. Comprar node sem prerequisite e validar gasto de ponto.
6. Tentar comprar node sem prerequisite e validar falha sem gastar ponto.
7. Comprar PassiveSkill e validar efeito automatico sem equipar.
8. Comprar EquippableSkill e equipar em R/T/Y/G.
9. Validar que PassiveSkill nao pode ser equipada.
10. Usar skill equipada no active slot e validar integracao com spec 12.
11. Validar capstone bloqueado antes de 5 nodes da arvore.
12. Comprar 5 nodes e validar capstone disponivel.
13. Salvar/carregar purchased nodes, passives e active slots.
14. Fazer primeiro respec gratuito na Fonte de Anya.
15. Fazer segundo respec com custo configuravel ou validar bloqueio por gold insuficiente.
16. Validar que respec nao altera level, XP, inventory, equipment, corpse, cave state ou checkpoints.
17. Validar que Q/E dentro do modal nao acionam maos/interacao do player.
18. Validar Unity compile validation e docs validation.
