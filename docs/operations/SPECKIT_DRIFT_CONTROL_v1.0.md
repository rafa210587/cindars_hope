# SpecKit Drift Control v1.0

> **Objetivo:** evitar que specs, planos, tasks e documentaÃƒÂ§ÃƒÂ£o de design diverjam entre si.

---

## 1. Regra obrigatÃƒÂ³ria

Toda feature futura deve ter, no mÃƒÂ­nimo:

```text
docs/specs/a_implementar/spec_*.md
docs/specs/a_implementar/spec_*.md
docs/specs/a_implementar/spec_*.md
```

Documentos em `docs/` podem existir como material rico de design, mas nÃƒÂ£o substituem a estrutura SpecKit.

---

## 2. Papel de cada arquivo

### spec.md

Deve conter:

- user story;
- objetivos funcionais;
- non-goals;
- regras de negÃƒÂ³cio;
- entidades funcionais;
- critÃƒÂ©rios de aceite;
- dependÃƒÂªncias;
- observabilidade mÃƒÂ­nima;
- condiÃƒÂ§ÃƒÂ£o de pronto para plan.

### plan.md

Deve conter:

- estratÃƒÂ©gia tÃƒÂ©cnica;
- arquitetura proposta;
- novos arquivos/pastas;
- integraÃƒÂ§ÃƒÂµes;
- riscos;
- plano de teste manual;
- fora de escopo tÃƒÂ©cnico.

### tasks.md

Deve conter:

- PRs pequenos;
- escopo por PR;
- arquivos esperados;
- critÃƒÂ©rios por PR;
- smoke test final.

---

## 3. Regras anti-drift

1. Nenhum PR deve implementar feature sem apontar para um `docs/specs/a_implementar/spec_*.md`.
2. Nenhum `tasks.md` deve conter PR sem critÃƒÂ©rio de aceite.
3. Nenhum `plan.md` deve introduzir arquitetura que nÃƒÂ£o esteja refletida em `tasks.md`.
4. Nenhum `spec.md` deve citar regra de negÃƒÂ³cio que nÃƒÂ£o apareÃƒÂ§a em pelo menos uma task.
5. MudanÃƒÂ§a em doc de design relevante deve gerar ou atualizar SpecKit correspondente.
6. Specs antigas devem ser mantidas como histÃƒÂ³rico, mas deltas precisam apontar qual documento ÃƒÂ© fonte ativa.
7. Handoff deve citar a feature ativa e o prÃƒÂ³ximo PR exato.

---

## 4. Nomenclatura

Formato recomendado:

```text
docs/specs/a_implementar/spec_farm_irrigacao_solo_planting_ui.md
docs/specs/a_implementar/spec_player_combat_weapons_spells_skill_actions_runtime.md
docs/specs/a_implementar/spec_enemy_ai_roster_bestiary_faction_locks_runtime.md
```

---

## 5. Checklist antes de implementar PR

- [ ] Li `PROJECT_LOG.md`.
- [ ] Li `AGENTS.md` ou `CLAUDE.md`.
- [ ] Li `spec.md`.
- [ ] Li `plan.md`.
- [ ] Li `tasks.md`.
- [ ] Identifiquei o PR exato da task.
- [ ] Confirmei arquivos permitidos.
- [ ] Confirmei critÃƒÂ©rios de aceite.
- [ ] NÃƒÂ£o estou implementando mais de uma task sem autorizaÃƒÂ§ÃƒÂ£o.

---

## 6. Estado atual apÃƒÂ³s padronizaÃƒÂ§ÃƒÂ£o

Features com SpecKit:

- `FASE9C_TOOLS_FARM_COMBAT_REFINEMENT`
- `FASE9C_PLAYER_EQUIPMENT_ITEMS_COMBAT`
- `FASE9D_ENEMY_ARCHITECTURE_40_MONSTERS`

Docs ricos de design continuam em `docs/`, mas a execucao deve seguir `docs/specs/`.

