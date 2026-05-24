# SPEC - Skill trees, active slots e respec na Fonte de Anya

> Spec ID: spec_skill_trees_active_slots_respec_anya_runtime
> Status: A implementar
> Ordem de execucao: 16
> Depende de: 00-15
> Bloqueia: 17
> Tipo: Runtime/UI
> Fonte: docs/specs/ como fonte unica; fontes absorvidas listadas abaixo.
> Escopo: Completar skill trees, skill points, active slots R/T/Y/G, skills passivas, skills equipaveis, capstones, save/load, modal de skill tree por tecla K e respec na Fonte de Anya.
> Fora de escopo: copiar textos/regras oficiais de D&D, subclasses completas, multiclass, balance final de builds, UI final/polish, animações/VFX finais, multiplayer, Packages, ProjectSettings e docs_old.

Fontes absorvidas:
- specs/FASE9K_SKILL_TREES_FULL_NODE_AND_UNLOCK/spec.md
- specs/FASE9E_PLAYER_LEVEL_UP_PROGRESSION/spec.md
- docs/refinements/a_implementar/pre_refinamentos/refinamento_init_skill_trees_active_slots_respec_anya.md

---

# /speckit.specify

## Contexto

A regra de SkillPoint foi estabilizada anteriormente:

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

A spec 15 definiu a Fonte de Anya como ponto de respawn e deixou respec futuro bloqueado. Esta spec 16 libera o respec na Fonte de Anya e completa a progressao de skill trees.

Nao foi localizada no repo, via search, a lista antiga completa de skills inspiradas em D&D/feats/classes. Portanto esta spec cria um catalogo inicial original, inspirado em arquetipos classicos de fantasia tabletop, sem copiar nomes proprietarios, textos, regras oficiais ou conteudo protegido.

## Pre-condicoes

Implementar runtime somente depois de specs 02-15 estarem realmente implementadas:

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
12 - player combat/weapons/spells/skill actions
13 - enemy AI/roster/bestiary/faction locks
14 - cave runtime generation/checkpoints/boss gates
15 - cave entry/death/Anya/corpse recovery
```

Antes de alterar codigo, revalidar:

```text
Assets/_Game/Scripts/Skills/**
Assets/_Game/Scripts/Combat/Skills/**
Assets/_Game/Scripts/Combat/Magic/**
Assets/_Game/Scripts/Player/**
Assets/_Game/Scripts/Progression/**
Assets/_Game/Scripts/UI/Skills/**
Assets/_Game/Scripts/UI/HUD/**
Assets/_Game/Scripts/UI/Anya/**
Assets/_Game/Scripts/UI/Modal/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Data/Skills/**
Assets/_Game/Data/Progression/**
```

Se algum sistema de XP/level ainda nao existir de forma formal, criar contrato minimo seguro e registrar pendencia; nao criar progressao paralela incompatível.

## Problema

Gaps atuais:

- Skill points ainda nao estao conectados de forma final ao level up.
- Skill tree data/runtime ainda esta parcial.
- Compra de nodes precisa validar custo, prerequisite e level minimo.
- Skills passivas e skills equipaveis ainda nao possuem contrato claro.
- Active slots R/T/Y/G precisam aceitar apenas skills equipaveis desbloqueadas.
- Modal de skill tree precisa abrir pela tecla `K`.
- Capstones precisam existir por arvore.
- Respec na Fonte de Anya precisa limpar nodes, passives e slots invalidos com custo claro.
- Save/load precisa persistir nodes comprados, active slots e respec count.

## Objetivo

Implementar skill tree MVP com:

- SkillPoint a cada 2 niveis;
- 5 arvores iniciais;
- skills passivas que nao precisam equipar;
- skills equipaveis que usam active slots R/T/Y/G;
- modal de skill tree acessado pela tecla `K`;
- nodes, prerequisites, capstones e purchase;
- active slot assignment por `SkillActionId`;
- respec full na Fonte de Anya;
- save/load seguro;
- derived stats recalculados ao comprar/respec/load.

## Decisoes aprovadas

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

## Arvores iniciais

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

## Tipos de node

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

## Skill data

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

`SkillActionSO` minimo ja definido na spec 12, mas aqui reforcado:

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

## Catalogo inicial de skills

A lista abaixo e original para Cindar's Hope, inspirada em arquetipos de classes/feats de fantasia tabletop, sem copiar nomes/regras oficiais.

### Melee tree

| NodeId | Nome | Tipo | Categoria | Efeito MVP |
|---|---|---|---|---|
| melee_iron_grip | Pegada de Ferro | PassiveStat | PassiveSkill | Attack +1 com melee |
| melee_guarded_stance | Postura Guardada | PassiveStat | PassiveSkill | Defense +1 |
| melee_battle_stride | Passo de Batalha | PassiveModifier | PassiveSkill | pequeno bonus de MoveSpeed durante combate |
| melee_heavy_training | Treino de Peso | PassiveModifier | PassiveSkill | melhora AttackSpeed de armas pesadas via hook da spec 10 |
| melee_heavy_strike | Golpe Pesado | UnlockSkillAction | EquippableSkill | skill equipada de ataque melee forte com stamina cost |
| melee_shieldless_resolve | Resolucao Sem Escudo | PassiveStat | PassiveSkill | MaxHP +5 quando sem offhand defensiva, hook seguro |
| melee_capstone_battle_rhythm | Ritmo de Batalha | Capstone | CapstonePassive | reduz levemente cooldown melee apos hit/kill, hook com fallback passivo |

### Ranged tree

| NodeId | Nome | Tipo | Categoria | Efeito MVP |
|---|---|---|---|---|
| ranged_steady_hand | Mao Firme | PassiveStat | PassiveSkill | BowDamageFlat +1 |
| ranged_long_sight | Mira Longa | PassiveModifier | PassiveSkill | BowRange +0.5 |
| ranged_quick_nock | Encaixe Rapido | PassiveModifier | PassiveSkill | melhora cooldown de bow via AttackSpeed/ranged hook |
| ranged_bleeding_arrow | Flecha Sangrante | UnlockSkillAction | EquippableSkill | skill equipada que aplica Bleed via spec 11 |
| ranged_piercing_shot | Disparo Perfurante | UnlockSkillAction | EquippableSkill | projectile atravessa 1 alvo, se suportado; fallback: dano maior single target |
| ranged_kiting_steps | Passos de Kiting | PassiveModifier | PassiveSkill | pequeno bonus de MoveSpeed apos disparo, hook seguro |
| ranged_capstone_eagle_focus | Foco da Aguia | Capstone | CapstonePassive | BowRange +1, projectile speed +1 e bonus leve em skills ranged |

### Magic tree

| NodeId | Nome | Tipo | Categoria | Efeito MVP |
|---|---|---|---|---|
| magic_mana_well | Poco de Mana | PassiveStat | PassiveSkill | MaxMana +10 |
| magic_quick_channel | Canalizacao Rapida | PassiveModifier | PassiveSkill | ManaRegen +1/s ou hook equivalente |
| magic_arcane_edge | Fio Arcano | PassiveStat | PassiveSkill | ArcaneDamageFlat +1 |
| magic_arcane_bolt_mastery | Dominio do Raio Arcano | UpgradeSkillAction | PassiveSkill | melhora ArcaneBolt da spec 12 |
| magic_ward_pulse | Pulso de Guarda | UnlockSkillAction | EquippableSkill | skill equipada de pequeno pulso defensivo/arcano |
| magic_slowing_sigils | Sigilos Lentificantes | UnlockSkillAction | EquippableSkill | skill equipada que aplica Slow em area pequena |
| magic_capstone_anya_spark | Fagulha de Anya | Capstone | CapstonePassive | MaxMana +15 e bonus de dano Arcane |

### Survival tree

| NodeId | Nome | Tipo | Categoria | Efeito MVP |
|---|---|---|---|---|
| survival_cave_lungs | Pulmoes da Caverna | PassiveStat | PassiveSkill | MaxStamina +10 |
| survival_hard_skin | Pele Dura | PassiveStat | PassiveSkill | MaxHP +5 |
| survival_low_rations | Racoes Curtas | PassiveModifier | PassiveSkill | reduz HungerDrain via hook da spec 09 |
| survival_toxic_sense | Senso Toxico | PassiveStat | PassiveSkill | ToxicResistance +1 ambiental |
| survival_cold_habit | Habito do Frio | PassiveStat | PassiveSkill | ColdResistance +1 ambiental |
| survival_emergency_roll | Rolamento de Emergencia | UnlockSkillAction | EquippableSkill | skill/dodge especial com custo maior, se suportado; fallback: buff curto de MoveSpeed |
| survival_capstone_caveborn | Nascido da Caverna | Capstone | CapstonePassive | bonus leve em resistencias ambientais e MaxStamina |

### Crafting tree

| NodeId | Nome | Tipo | Categoria | Efeito MVP |
|---|---|---|---|---|
| crafting_fast_hands | Maos Ageis | PassiveModifier | PassiveSkill | CraftTime reduction leve |
| crafting_repair_care | Cuidado no Reparo | PassiveModifier | PassiveSkill | RepairKit recupera um pouco mais, hook da spec 10 |
| crafting_material_eye | Olho de Material | PassiveModifier | PassiveSkill | pequena chance futura de bonus de recurso, hook sem prometer drop extra agora |
| crafting_field_patch | Remendo de Campo | UnlockSkillAction | EquippableSkill | skill equipada de pequeno reparo/utility, se suportado |
| crafting_station_focus | Foco de Bancada | PassiveModifier | PassiveSkill | bonus de craft em workstation, hook da spec 07 |
| crafting_pack_order | Mochila Ordenada | PassiveModifier | PassiveSkill | hook para futura expansao/organizacao, sem aumentar capacidade se spec 03 nao suportar |
| crafting_capstone_master_artisan | Mestre Artesao | Capstone | CapstonePassive | craft time menor e repair efficiency maior |

## Active slots

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

## Modal SkillTreePanel

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

## Purchase rules

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

## Capstones

Regra MVP:

```text
Cada arvore tem 1 capstone.
Capstone exige pelo menos 5 nodes comprados na mesma arvore + prerequisites diretos.
Capstone custa 1 SkillPoint.
```

Capstones podem ser passivos fortes no MVP.

## Respec na Fonte de Anya

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

## Derived stats e passives

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

## Save/load

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

## Eventos

Criar/usar eventos oficiais:

```text
SkillPointGrantedEvent
SkillNodePurchaseRequestedEvent
SkillNodePurchasedEvent
SkillPurchaseFailedEvent
SkillPassiveAppliedEvent
SkillPassiveRemovedEvent
ActiveSkillSlotAssignRequestedEvent
ActiveSkillSlotAssignedEvent
ActiveSkillSlotClearedEvent
SkillTreeOpenedEvent
SkillTreeClosedEvent
SkillTreeRespecRequestedEvent
SkillTreeRespecCompletedEvent
SkillTreeRespecFailedEvent
SkillDerivedStatsChangedEvent
```

Nao duplicar eventos equivalentes se ja existirem.

## UI minima

- SkillTreePanel modal entra agora.
- Abas/arvores: Melee, Ranged, Magic, Survival, Crafting.
- Mostrar SkillPoints disponiveis.
- Mostrar nodes comprados/bloqueados/disponiveis.
- Mostrar prerequisites basicos.
- Mostrar active slots R/T/Y/G.
- Permitir equipar EquippableSkill em active slot.
- Mostrar que PassiveSkill nao precisa equipar.
- Polish final fica spec 17.

## Invariantes anti-regressao

Esta spec nao pode quebrar:

- Active slots R/T/Y/G da spec 12;
- SkillActionSO somente para skills/active slots;
- Q/E como uso de maos/interacao fora de modal;
- Fonte de Anya da spec 15;
- death/corpse da spec 15;
- stamina/hunger/time da spec 09;
- equipment/derived stats da spec 10;
- damage/status da spec 11;
- player combat/mana da spec 12;
- save migration e DTOs simples da spec 02;
- modal stack;
- GameEventBus;
- regra de nao usar `GameObject.Find()` ou `FindObjectOfType()`;
- regra de nao serializar referencias Unity em DTOs.

Se alguma integracao ainda nao existir, registrar hook/pendencia clara sem criar sistema paralelo.

## Criterios de aceite

- SkillPoint e concedido em niveis pares, comecando no nivel 2.
- Modal SkillTreePanel abre/fecha com `K`.
- Existem 5 arvores iniciais: Melee, Ranged, Magic, Survival, Crafting.
- Cada arvore possui 6 nodes comuns + 1 capstone.
- Existem skills passivas que nao precisam equipar.
- Existem skills equipaveis que podem ocupar active slots R/T/Y/G.
- PassiveSkill aplica efeito automaticamente apos compra.
- Active slot aceita apenas EquippableSkill desbloqueada.
- Compra valida custo, prerequisites e level minimo quando aplicavel.
- Capstone exige 5 nodes da arvore + prerequisites diretos.
- Respec full funciona somente na Fonte de Anya.
- Primeiro respec e gratuito; seguintes usam custo configuravel inicial de 250 gold.
- Respec nao altera level, XP, inventory, equipment, corpse, cave state, checkpoints ou boss gates.
- Save/load preserva PurchasedNodeIds, ActiveSkillSlots e RespecCount.
- Derived stats recalculam em purchase/respec/load.
- Nenhum item das invariantes anti-regressao e quebrado.
- Validacao documental e Unity compile validation registradas.

---

# /speckit.plan

## Arquitetura

Sistemas afetados:

```text
Assets/_Game/Scripts/Skills/**
Assets/_Game/Scripts/Combat/Skills/**
Assets/_Game/Scripts/Combat/Magic/**
Assets/_Game/Scripts/Player/**
Assets/_Game/Scripts/Progression/**
Assets/_Game/Scripts/UI/Skills/**
Assets/_Game/Scripts/UI/HUD/**
Assets/_Game/Scripts/UI/Anya/**
Assets/_Game/Scripts/UI/Modal/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Data/Skills/**
Assets/_Game/Data/Progression/**
```

Managers/bridges Unity devem ser finos. Purchase validation, passive application, slot assignment e respec devem ficar em services/classes testaveis quando possivel.

## Ordem segura de implementacao

1. Revalidar progression/XP/level atual, SkillActionSO, active slots e Fonte de Anya.
2. Confirmar specs 02-15 implementadas antes de runtime.
3. Criar/consolidar skill data assets e registry.
4. Implementar SkillPoint grant por level par.
5. Implementar SkillPurchaseService.
6. Implementar passive modifiers e derived stats integration.
7. Implementar active slot assignment R/T/Y/G para EquippableSkill.
8. Implementar SkillTreePanel com tecla K.
9. Implementar capstone rules.
10. Implementar respec na Fonte de Anya.
11. Implementar save/load/migration.
12. Validar anti-regressao e atualizar tracking.

## Fluxos

### Ganhar SkillPoint

```text
LevelChangedEvent ou equivalente
Se novo level par e >= 2
Conceder 1 SkillPoint
Publicar SkillPointGrantedEvent
```

### Comprar node

```text
Abrir SkillTreePanel com K
Selecionar node
Validar custo/prerequisites/level
Gastar SkillPoint
Marcar PurchasedNodeId
Aplicar passive ou desbloquear skill action
Publicar eventos
```

### Equipar skill ativa

```text
Selecionar EquippableSkill desbloqueada
Escolher slot R/T/Y/G
Salvar SkillActionId no slot
Publicar ActiveSkillSlotAssignedEvent
```

### Respec

```text
Interagir com Fonte de Anya
Selecionar Respec
Validar custo
Remover nodes/passives
Limpar active slots invalidos
Recalcular SkillPoints
Incrementar RespecCount
Publicar SkillTreeRespecCompletedEvent
```

## Riscos de regressao

- SkillPoint duplicar ao recarregar save.
- Passive aplicar duas vezes.
- Active slot equipar skill bloqueada.
- Respec apagar XP/level/inventory por engano.
- K conflitar com modal stack.
- Q/E dentro do modal afetar maos do player.
- Save carregar node inexistente e quebrar runtime.

## Mitigacao

- SkillPoints derivados por level + purchased nodes quando possivel.
- Passive recalculado por estado comprado, nao acumulado incrementalmente sem reset.
- Active slot valida unlock no assign e no load.
- Respec isolado a skill state.
- Modal stack para SkillTreePanel.
- Warnings para IDs inexistentes.

---

# /speckit.tasks

## Tasks

- [ ] Revalidar SkillTreeManager, SkillActionSO, progression/XP/level, active slots, Anya e save atuais.
- [ ] Confirmar specs 02-15 implementadas antes de runtime.
- [ ] Criar/ajustar SkillTreeDataSO.
- [ ] Criar/ajustar SkillNodeDataSO.
- [ ] Criar/ajustar SkillTreeRegistrySO.
- [ ] Criar/ajustar SkillPassiveModifierSO ou equivalente.
- [ ] Criar catalogo inicial de 5 arvores, 35 nodes.
- [ ] Implementar SkillPoint por level par.
- [ ] Implementar SkillPurchaseService com prerequisites/custo/level.
- [ ] Implementar passive application/recalculation.
- [ ] Implementar SkillTreePanel modal com tecla K.
- [ ] Implementar active slot assignment R/T/Y/G para EquippableSkill.
- [ ] Implementar capstone rules.
- [ ] Implementar respec full na Fonte de Anya.
- [ ] Implementar save/load/migration de skill tree state.
- [ ] Atualizar docs implementados/status/registries/refinements/log.

## Arquivos permitidos

```text
Assets/_Game/Scripts/Skills/**
Assets/_Game/Scripts/Combat/Skills/**
Assets/_Game/Scripts/Combat/Magic/**
Assets/_Game/Scripts/Player/**
Assets/_Game/Scripts/Progression/**
Assets/_Game/Scripts/UI/Skills/**
Assets/_Game/Scripts/UI/HUD/**
Assets/_Game/Scripts/UI/Anya/**
Assets/_Game/Scripts/UI/Modal/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Data/Skills/**
Assets/_Game/Data/Progression/**
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

- Skill tree runtime e UI minima funcionam.
- SkillPoint, purchase, passives, equipaveis, active slots, capstones e respec funcionam.
- Save/load preserva estado.
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
