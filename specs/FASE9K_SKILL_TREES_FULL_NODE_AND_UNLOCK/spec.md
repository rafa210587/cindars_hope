# SpecKit — FASE9K Skill Trees, Nodes, Unlocks & Build Progression

> **Feature:** `FASE9K_SKILL_TREES_FULL_NODE_AND_UNLOCK`  
> **Status:** especificação funcional aprovada para planejamento.  
> **Fonte de design:** `docs_old/FASE9K_SKILL_TREES_FULL_NODE_AND_UNLOCK_SPEC_v1.0.md`

---

## 1. User story

Como jogador, quero investir SkillPoints em árvores com identidade forte, escolher skills ativas e passivas, montar builds diferentes e poder fazer respec na Fonte de Anya, para que minha progressão tenha escolhas reais até o level 100.

---

## 2. Objetivos funcionais

### O1 — SkillPoint economy

Jogador ganha 1 SkillPoint a cada 2 níveis, nos níveis pares, até aproximadamente 50 SkillPoints no level 100.

### O2 — Skill costs

Skills podem custar 1, 2 ou 3 pontos.

### O3 — Active skill slots

Jogador possui 4 Active Skill Slots. Passivas ficam sempre ativas.

### O4 — Capstone limits

Cada árvore pode ter múltiplas capstones disponíveis, mas jogador pode aprender no máximo 2 capstones por árvore.

### O5 — Respec

Respec existe na Fonte de Anya e custa 100 gold por atributo ou skill resetado.

### O6 — Requirements

Skills avançadas podem exigir level, pontos gastos na árvore, skills anteriores, atributo mínimo, arma ou equipamento específico.

### O7 — Tree identity

Árvores devem ter identidade clara:

```text
Combat = força, arma pesada, escudo, controle físico
Dexterity = evasão, parry, bow, dagger, dual wield leve
Magic = spells, elementos, mana, status, staff/wand/focus
Survival = stamina, cave sustain, crafting, gathering, resistências ambientais
```

### O8 — D&D-like fantasy archetypes without copying

Nodes devem usar inspiração conceitual de estilos de combate, maestrias, feats, skills e spells de fantasia medieval, mas com nomes/efeitos próprios de Cindar's Hope.

---

## 3. Regras de negócio

### R1 — SkillPoint gain

```text
SkillPointEveryLevels = 2
```

### R2 — Active slots

```text
ActiveSkillSlots = 4
```

### R3 — Capstones

```text
MaxCapstonesPerTree = 2
CapstoneCost = 3
```

### R4 — Respec

```text
RespecGoldCostPerPoint = 100
RespecLocation = Anya Fountain
```

### R5 — Dependency removal

Se remover uma skill pré-requisito, dependentes devem ser removidas ou bloqueadas.

### R6 — Trees unlocked

Skill trees ficam disponíveis desde o início, mas compra depende de pontos e requisitos.

---

## 4. Entidades funcionais

- `SkillTreeSO`
- `SkillNodeSO`
- `SkillTreeRulesSO`
- `PlayerSkillProgressionSaveData`
- `SkillEffect`
- `ActiveSkillSlot`
- `RespecService`
- `SkillPurchaseValidator`

---

## 5. Critérios de aceite

### CA1 — SkillPoint gain

Jogador ganha 1 SkillPoint a cada 2 níveis.

### CA2 — Skill costs

Skills podem custar 1, 2 ou 3 pontos.

### CA3 — Active slots

Jogador tem 4 Active Skill Slots.

### CA4 — Passives

Passivas ficam sempre ativas.

### CA5 — Capstones

Máximo de 2 capstones por árvore.

### CA6 — Respec

Respec existe na Fonte de Anya.

### CA7 — Respec cost

Respec custa 100 gold por atributo ou skill resetado.

### CA8 — Attribute requirements

Skills avançadas podem exigir atributo mínimo.

### CA9 — Weapon/equipment requirements

Skills específicas podem exigir arma/equipamento específico.

### CA10 — Trees available

Skill trees ficam disponíveis desde o início.

### CA11 — Combat identity

Combat Tree inclui estilos, maestrias, shield, heavy weapons e capstones.

### CA12 — Dexterity identity

Dexterity Tree inclui roll, dash, parry, bow, dagger, dual wield leve e capstones.

### CA13 — Magic identity

Magic Tree inclui spells por elemento, mana, cast, focus/staff/wand e capstones.

### CA14 — Survival identity

Survival Tree inclui sprint, stamina, durability, environmental resistance, crafting, gathering e capstones.

### CA15 — Strong node identity

Skill nodes têm identidade forte e não são apenas bônus numéricos genéricos.

---

## 6. Non-goals

Fora desta spec:

- UI final da skill tree;
- layout visual final dos nodes;
- balance final de todos os valores;
- animações finais de active skills;
- VFX/SFX finais;
- respec UI final;
- conteúdo completo de 180 nodes;
- sistema de classes rígidas;
- multiplayer/co-op skill sync.

---

## 7. MVP recomendado

1. `SkillTreeRulesSO` com SkillPointEveryLevels=2, ActiveSkillSlots=4, MaxCapstonesPerTree=2.
2. `SkillTreeSO` para Combat, Dexterity, Magic, Survival.
3. `SkillNodeSO` com subset inicial de 5–8 nodes por árvore.
4. `PlayerSkillProgressionSaveData` com LearnedSkillIds e EquippedActiveSkillIds.
5. Skill purchase validator.
6. Active skill equip validator.
7. Respec simples na Fonte de Anya.
8. Debug/OnGUI simples para comprar/equipar skills.

---

## 8. Dependências

- FASE9E Player Level Up/Progression.
- FASE9I Player Combat/Weapons/Magic/Skill Trees.
- FASE9J Cave Run Entry/Loadout/HUD/Failure Flow.

---

## 9. Pronto para Plan quando

- Player progression save estiver preparado para SkillPoints.
- Fonte de Anya existir ou estiver planejada.
- Action slots/skill unlocks forem aceitos para implementação incremental.


