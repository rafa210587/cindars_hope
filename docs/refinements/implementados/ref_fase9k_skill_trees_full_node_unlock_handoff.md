# REF — FASE9K skill trees full node unlock handoff

> Origem histórica: `docs_old/audits/FASE9K_SKILL_TREES_FULL_NODE_AND_UNLOCK_HANDOFF.md`
> Status: Refinamento implementado / handoff / audit preservado
> Relacionado a:
> - $_

---

# Handoff — FASE9K Skill Trees, Nodes, Unlocks & Build Progression

> **Data:** 2026-05-21  
> **Branch:** dev  
> **Responsável:** ChatGPT  
> **Escopo:** registrar criação da FASE9K como baseline de skill trees, nodes, unlocks, active slots, capstones e respec.

---

## 1. Arquivos criados

- `docs/FASE9K_SKILL_TREES_FULL_NODE_AND_UNLOCK_SPEC_v1.0.md`
- `specs/FASE9K_SKILL_TREES_FULL_NODE_AND_UNLOCK/spec.md`

---

## 2. Decisões registradas

- Jogador ganha 1 SkillPoint a cada 2 níveis.
- SkillPoints são concedidos nos níveis pares: 2, 4, 6... 100.
- Total máximo aproximado: 50 SkillPoints.
- Skills podem custar 1, 2 ou 3 pontos.
- Jogador pode aprender no máximo 2 capstones por árvore.
- Active skills usam 4 slots ativos.
- Passivas ficam sempre ativas após aprendidas.
- Respec existe na Fonte de Anya.
- Respec custa 100 gold por atributo ou skill resetado.
- Skills avançadas podem exigir atributo mínimo.
- Skills específicas podem exigir arma/equipamento específico.
- Skill tree fica desbloqueada desde o início.
- Nodes seguem inspiração conceitual de feats/skills/weapon masteries/spells de fantasia medieval, com nomes e efeitos próprios.

---

## 3. Árvores registradas

- Combat: estilos marciais, shield, heavy weapons, two-handed, stagger, special attacks, heavy dual wield.
- Dexterity: roll, dash, parry, bow, dagger, dual wield leve, charge shot, crit/mobilidade.
- Magic: staff, wand, focus, scroll, spells elementais, mana, cast time, status.
- Survival/Breath: sprint, stamina, durabilidade, resistência ambiental, crafting, gathering, cave sustain.

---

## 4. Hardening aplicado

- Jogador não deve conseguir comprar tudo até level 100.
- Capstones precisam mudar build, não só aumentar números.
- Feat-like nodes não devem ser apenas bônus percentuais genéricos.
- Combat e Dexterity não devem ocupar o mesmo espaço.
- Survival precisa ser competitiva, não apenas árvore utilitária fraca.
- Magic não deve permitir pegar todos os elementos fortes facilmente.
- Remover skill pré-requisito no respec deve remover ou bloquear dependentes.

---

## 5. Validação

- [x] Documento FASE9K criado no repo.
- [x] SpecKit FASE9K criado no repo.
- [x] Handoff documental criado.
- [ ] Unity não executado; alteração documental/spec.
- [ ] `PROJECT_LOG.md` não foi editado para evitar sobrescrever entradas paralelas recentes.

---

## 6. Próximo passo recomendado

Usar FASE9K como baseline quando a implementação chegar em:

- SkillTreeRulesSO;
- SkillTreeSO;
- SkillNodeSO;
- PlayerSkillProgressionSaveData;
- Skill purchase validator;
- Active skill slots;
- Respec na Fonte de Anya;
- Debug/OnGUI de skill tree.

MVP deve começar por subset de 5–8 nodes por árvore, não pela árvore completa gigante.





