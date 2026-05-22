# SpecKit â€” FASE9K Skill Trees, Nodes, Unlocks & Build Progression

> **Feature:** `FASE9K_SKILL_TREES_FULL_NODE_AND_UNLOCK`  
> **Status:** especificaÃ§Ã£o funcional aprovada para planejamento.  
> **Fonte de design:** `docs_old/FASE9K_SKILL_TREES_FULL_NODE_AND_UNLOCK_SPEC_v1.0.md`

---

## 1. User story

Como jogador, quero investir SkillPoints em Ã¡rvores com identidade forte, escolher skills ativas e passivas, montar builds diferentes e poder fazer respec na Fonte de Anya, para que minha progressÃ£o tenha escolhas reais atÃ© o level 100.

---

## 2. Objetivos funcionais

### O1 â€” SkillPoint economy

Jogador ganha 1 SkillPoint a cada 2 nÃ­veis, nos nÃ­veis pares, atÃ© aproximadamente 50 SkillPoints no level 100.

### O2 â€” Skill costs

Skills podem custar 1, 2 ou 3 pontos.

### O3 â€” Active skill slots

Jogador possui 4 Active Skill Slots. Passivas ficam sempre ativas.

### O4 â€” Capstone limits

Cada Ã¡rvore pode ter mÃºltiplas capstones disponÃ­veis, mas jogador pode aprender no mÃ¡ximo 2 capstones por Ã¡rvore.

### O5 â€” Respec

Respec existe na Fonte de Anya e custa 100 gold por atributo ou skill resetado.

### O6 â€” Requirements

Skills avanÃ§adas podem exigir level, pontos gastos na Ã¡rvore, skills anteriores, atributo mÃ­nimo, arma ou equipamento especÃ­fico.

### O7 â€” Tree identity

Ãrvores devem ter identidade clara:

```text
Combat = forÃ§a, arma pesada, escudo, controle fÃ­sico
Dexterity = evasÃ£o, parry, bow, dagger, dual wield leve
Magic = spells, elementos, mana, status, staff/wand/focus
Survival = stamina, cave sustain, crafting, gathering, resistÃªncias ambientais
```

### O8 â€” D&D-like fantasy archetypes without copying

Nodes devem usar inspiraÃ§Ã£o conceitual de estilos de combate, maestrias, feats, skills e spells de fantasia medieval, mas com nomes/efeitos prÃ³prios de Cindar's Hope.

---

## 3. Regras de negÃ³cio

### R1 â€” SkillPoint gain

```text
SkillPointEveryLevels = 2
```

### R2 â€” Active slots

```text
ActiveSkillSlots = 4
```

### R3 â€” Capstones

```text
MaxCapstonesPerTree = 2
CapstoneCost = 3
```

### R4 â€” Respec

```text
RespecGoldCostPerPoint = 100
RespecLocation = Anya Fountain
```

### R5 â€” Dependency removal

Se remover uma skill prÃ©-requisito, dependentes devem ser removidas ou bloqueadas.

### R6 â€” Trees unlocked

Skill trees ficam disponÃ­veis desde o inÃ­cio, mas compra depende de pontos e requisitos.

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

## 5. CritÃ©rios de aceite

### CA1 â€” SkillPoint gain

Jogador ganha 1 SkillPoint a cada 2 nÃ­veis.

### CA2 â€” Skill costs

Skills podem custar 1, 2 ou 3 pontos.

### CA3 â€” Active slots

Jogador tem 4 Active Skill Slots.

### CA4 â€” Passives

Passivas ficam sempre ativas.

### CA5 â€” Capstones

MÃ¡ximo de 2 capstones por Ã¡rvore.

### CA6 â€” Respec

Respec existe na Fonte de Anya.

### CA7 â€” Respec cost

Respec custa 100 gold por atributo ou skill resetado.

### CA8 â€” Attribute requirements

Skills avanÃ§adas podem exigir atributo mÃ­nimo.

### CA9 â€” Weapon/equipment requirements

Skills especÃ­ficas podem exigir arma/equipamento especÃ­fico.

### CA10 â€” Trees available

Skill trees ficam disponÃ­veis desde o inÃ­cio.

### CA11 â€” Combat identity

Combat Tree inclui estilos, maestrias, shield, heavy weapons e capstones.

### CA12 â€” Dexterity identity

Dexterity Tree inclui roll, dash, parry, bow, dagger, dual wield leve e capstones.

### CA13 â€” Magic identity

Magic Tree inclui spells por elemento, mana, cast, focus/staff/wand e capstones.

### CA14 â€” Survival identity

Survival Tree inclui sprint, stamina, durability, environmental resistance, crafting, gathering e capstones.

### CA15 â€” Strong node identity

Skill nodes tÃªm identidade forte e nÃ£o sÃ£o apenas bÃ´nus numÃ©ricos genÃ©ricos.

---

## 6. Non-goals

Fora desta spec:

- UI final da skill tree;
- layout visual final dos nodes;
- balance final de todos os valores;
- animaÃ§Ãµes finais de active skills;
- VFX/SFX finais;
- respec UI final;
- conteÃºdo completo de 180 nodes;
- sistema de classes rÃ­gidas;
- multiplayer/co-op skill sync.

---

## 7. MVP recomendado

1. `SkillTreeRulesSO` com SkillPointEveryLevels=2, ActiveSkillSlots=4, MaxCapstonesPerTree=2.
2. `SkillTreeSO` para Combat, Dexterity, Magic, Survival.
3. `SkillNodeSO` com subset inicial de 5â€“8 nodes por Ã¡rvore.
4. `PlayerSkillProgressionSaveData` com LearnedSkillIds e EquippedActiveSkillIds.
5. Skill purchase validator.
6. Active skill equip validator.
7. Respec simples na Fonte de Anya.
8. Debug/OnGUI simples para comprar/equipar skills.

---

## 8. DependÃªncias

- FASE9E Player Level Up/Progression.
- FASE9I Player Combat/Weapons/Magic/Skill Trees.
- FASE9J Cave Run Entry/Loadout/HUD/Failure Flow.

---

## 9. Pronto para Plan quando

- Player progression save estiver preparado para SkillPoints.
- Fonte de Anya existir ou estiver planejada.
- Action slots/skill unlocks forem aceitos para implementaÃ§Ã£o incremental.

