# REF — FASE9I player combat weapons magic skill trees handoff

> Origem histórica: `docs_old/audits/FASE9I_PLAYER_COMBAT_WEAPONS_MAGIC_SKILL_TREES_HANDOFF.md`
> Status: Refinamento implementado / handoff / audit preservado
> Relacionado a:
> - $_

---

# Handoff — FASE9I Player Combat, Weapons, Magic & Skill Trees

> **Data:** 2026-05-20  
> **Branch:** dev  
> **Responsável:** ChatGPT  
> **Escopo:** registrar criação da FASE9I como baseline de player combat, weapons, magic e skill trees sem reescrever specs antigas.

---

## 1. Arquivos criados

- `docs/FASE9I_PLAYER_COMBAT_WEAPONS_MAGIC_SKILL_TREES_SPEC_v1.0.md`
- `specs/FASE9I_PLAYER_COMBAT_WEAPONS_MAGIC_SKILL_TREES/spec.md`

---

## 2. Decisões registradas

- Combate do jogador será action RPG.
- Roll é dodge defensivo e skill de Dexterity.
- Dash é avanço ofensivo e skill de Dexterity.
- Parry é skill de Dexterity e funciona contra Physical melee/projéteis físicos.
- Block exige Shield e skill específica.
- Sprint é skill de Survival/Breath.
- Todas as ações especiais consomem stamina ou mana.
- Magia exige Staff, Wand, Focus ou Scroll.
- Cada arma tem Light Attack e Special Attack.
- Dual wield leve usa ataque alternado automático.
- Dual wield pesado é skill avançada de Combat/Strength.
- Bow shot padrão é instantâneo, consome flecha + stamina e escala com Dexterity.
- Charge Shot é skill.
- Existem flechas elementais.
- Magias existem por elemento: Arcane, Fire, Ice, Poison/Nature, Shadow, Lightning/Thunder, Acid e Corruption.
- Skill trees iniciais: Combat, Dexterity, Magic, Survival/Breath.

---

## 3. Hardening aplicado

- Roll e Dash são ações separadas.
- Roll é defensivo; Dash é ofensivo.
- Block exige shield e não funciona com two-handed/dual wield.
- Parry não funciona contra magia pura, AoE, pools, breath ou beams.
- Spell cast sem item mágico equipado é proibido, salvo scroll consumível.
- Crit vem principalmente de skill/equipment.
- Armor weight afeta Roll, Dash, Sprint e stamina cost.
- Special Attack consome stamina/mana.
- Bow usa durabilidade padrão até spec futura mais específica.

---

## 4. Validação

- [x] Documento FASE9I criado no repo.
- [x] SpecKit FASE9I criado no repo.
- [x] Handoff documental criado.
- [ ] Unity não executado; alteração documental/spec.
- [ ] `PROJECT_LOG.md` não foi editado para evitar sobrescrever entradas paralelas recentes.

---

## 5. Próximo passo recomendado

Quando a implementação chegar nesta área, usar FASE9I como baseline para:

- player action data;
- skill-gated actions;
- weapon combat profiles;
- spell data;
- stamina/mana validation;
- bow/ammo flow;
- dual wield;
- shield block;
- parry rules;
- skill tree contracts.

MVP deve começar por contratos e poucas ações, não pela árvore gigante completa.




