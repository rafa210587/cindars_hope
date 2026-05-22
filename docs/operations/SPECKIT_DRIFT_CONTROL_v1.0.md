# SpecKit Drift Control v1.0

> **Objetivo:** evitar que specs, planos, tasks e documentação de design diverjam entre si.

---

## 1. Regra obrigatória

Toda feature futura deve ter, no mínimo:

```text
specs/<FEATURE_ID>/spec.md
specs/<FEATURE_ID>/plan.md
specs/<FEATURE_ID>/tasks.md
```

Documentos em `docs/` podem existir como material rico de design, mas não substituem a estrutura SpecKit.

---

## 2. Papel de cada arquivo

### spec.md

Deve conter:

- user story;
- objetivos funcionais;
- non-goals;
- regras de negócio;
- entidades funcionais;
- critérios de aceite;
- dependências;
- observabilidade mínima;
- condição de pronto para plan.

### plan.md

Deve conter:

- estratégia técnica;
- arquitetura proposta;
- novos arquivos/pastas;
- integrações;
- riscos;
- plano de teste manual;
- fora de escopo técnico.

### tasks.md

Deve conter:

- PRs pequenos;
- escopo por PR;
- arquivos esperados;
- critérios por PR;
- smoke test final.

---

## 3. Regras anti-drift

1. Nenhum PR deve implementar feature sem apontar para um `specs/<FEATURE_ID>/tasks.md`.
2. Nenhum `tasks.md` deve conter PR sem critério de aceite.
3. Nenhum `plan.md` deve introduzir arquitetura que não esteja refletida em `tasks.md`.
4. Nenhum `spec.md` deve citar regra de negócio que não apareça em pelo menos uma task.
5. Mudança em doc de design relevante deve gerar ou atualizar SpecKit correspondente.
6. Specs antigas devem ser mantidas como histórico, mas deltas precisam apontar qual documento é fonte ativa.
7. Handoff deve citar a feature ativa e o próximo PR exato.

---

## 4. Nomenclatura

Formato recomendado:

```text
specs/FASE9C_TOOLS_FARM_COMBAT_REFINEMENT/
specs/FASE9C_PLAYER_EQUIPMENT_ITEMS_COMBAT/
specs/FASE9D_ENEMY_ARCHITECTURE_40_MONSTERS/
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
- [ ] Confirmei critérios de aceite.
- [ ] Não estou implementando mais de uma task sem autorização.

---

## 6. Estado atual após padronização

Features com SpecKit:

- `FASE9C_TOOLS_FARM_COMBAT_REFINEMENT`
- `FASE9C_PLAYER_EQUIPMENT_ITEMS_COMBAT`
- `FASE9D_ENEMY_ARCHITECTURE_40_MONSTERS`

Docs ricos de design continuam em `docs/`, mas a execução deve seguir `specs/`.

