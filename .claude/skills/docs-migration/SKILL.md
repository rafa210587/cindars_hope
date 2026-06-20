---
name: docs-migration
description: Move specs e refinements de a_implementar/ para implementados/ com evidência (header de evidence, registries, IMPLEMENTATION_STATUS, PROJECT_LOG, docs validation). Use ao fazer o closeout de uma spec ou refinement implementado e validado.
---

# Skill: Docs Migration

Use ao fechar uma spec ou refinement para mover os arquivos de documentation para as pastas implementados/.

## Regras

1. **`.specs/` é a única source of truth.** Nunca recrie `specs/` ou `spec/` no root.
2. **Specs futuras ficam em `a_implementar/`.** Mova só quando a implementação estiver completa e validada.
3. **Specs completas vão para `implementados/`.** File path: `.specs/implementados/spec_*.md`
4. **Refinements futuros em pre_refinements/.** Path: `docs/refinements/a_implementar/pre_refinamentos/ref_*.md`
5. **Refinements completos vão para `implementados/`.** Path: `docs/refinements/implementados/ref_*.md`
6. **Registries devem permanecer consistentes.** Atualize após mover specs/refinements.
7. **IMPLEMENTATION_STATUS.md deve refletir a realidade.** Nenhuma claim sem evidência.
8. **PROJECT_LOG.md deve ser atualizado.** Data, spec e deliverables registrados.

## Passos de migration

### 1. Verificar evidência

Antes de mover uma spec para implementados/:

- [ ] Mudanças de código existem no repo (checado via `git diff` ou `git log`)
- [ ] Validação passou (docs, Unity compile, log scan) OU documentada como NOT RUN
- [ ] Nenhum gap deixado na compliance com as rules do CLAUDE.md
- [ ] Non-regression audit passou (PASS ou WARNING aceitável)

**Exemplo de evidência:**
```
Spec 12 Implementation Evidence:
✓ Commit: "feat: spec 12 - player combat melee/ranged attacks"
✓ Files: Assets/Scripts/Runtime/Combat/PlayerCombatManager.cs, WeaponDataSO.cs
✓ Validation: Unity compile PASS, Docs PASS
✓ Non-regression: PASS
```

### 2. Mover o spec file

Mova de `.specs/a_implementar/spec_*.md` para `.specs/implementados/spec_*.md`

```powershell
Move-Item -Path ".specs/a_implementar/spec_12_player_combat.md" `
          -Destination ".specs/implementados/spec_12_player_combat.md"
```

### 3. Atualizar o header do spec file

Adicione header de implementation evidence à spec movida:

```markdown
---
status: implemented
date_implemented: 2026-05-26
evidence:
  - Commit: feat: spec 12 - player combat melee/ranged attacks
  - Files: Assets/Scripts/Runtime/Combat/*
  - Validation: docs PASS, unity compile PASS
  - Non-regression: PASS
---

# Spec 12 - Player Combat (Implemented)

[rest of original spec content]
```

### 4. Mover o refinement relacionado (se aplicável)

Se a spec referencia um pre-refinement:

```powershell
Move-Item -Path "docs/refinements/a_implementar/pre_refinamentos/ref_spec12_*.md" `
          -Destination "docs/refinements/implementados/ref_spec12_*.md"
```

### 5. Atualizar registries (se existirem)

Se estes arquivos existirem, atualize-os:

- `.specs/SPEC_REGISTRY_IMPLEMENTED.md` — Adicionar spec 12 à lista
- `.specs/SPEC_REGISTRY_TO_IMPLEMENT.md` — Remover spec 12 da lista
- `docs/refinements/implementados/ref_implementados_map.md` — Adicionar refinement se movido
- `docs/refinements/a_implementar/ref_futuro_map.md` — Remover refinement se movido

**Exemplo de atualização:**

```markdown
## Specs Implemented

- Spec 01: Core Event Bus
- Spec 02: Bootstrap Managers
- ...
- Spec 12: Player Combat ✅ Implemented 2026-05-26
```

### 6. Atualizar IMPLEMENTATION_STATUS.md

Adicione ou atualize a entry de capability com evidência:

```markdown
| Player Combat/Weapons/Spells | Implementado | spec_player_combat_melee_ranged.md, commit abc1234 |
```

**Regra:** Só adicione se:
- Spec movida para implementados/ OU
- Código validado existe com evidência

### 7. Atualizar PROJECT_LOG.md

Adicione entry no topo:

```markdown
## Sessão 2026-05-26 (NN) - Implementar SPEC 12 (Player Combat)

**Data:** 2026-05-26  
**Foco:** Implementar melee/ranged attacks com UI de combate  
**Status:** COMPLETO

### Deliverables

- PlayerCombatManager com attack logic
- WeaponDataSO com stats de melee/ranged
- DamageRequest via GameEventBus
- Weapon hotbar selection (if UI scope allowed)

### Validações

```text
Docs validation: PASS
Unity compile: PASS - Tundra build success
Log scanner: PASS
Non-regression: PASS
```

### Commit

```
abc1234 feat: spec 12 - player combat melee/ranged attacks
```

### Próxima SPEC

- SPEC 13: [name]
- Blocked? Status?
```

### 8. Rodar Docs Validation

```powershell
.\tools\docs\validate_docs.ps1
```

**Esperado:** PASS ou WARNING preexistente

**Action:**
- Se PASS: Prossiga para a conclusão
- Se FAIL: Corrija os problemas de docs, rode de novo, e então prossiga

## Patterns comuns

### Migration de spec única (mais comum)

```powershell
# 1. Move file
Move-Item ".specs/a_implementar/spec_12_*.md" ".specs/implementados/"

# 2. Update header in moved file with evidence

# 3. Update registries (if exist)

# 4. Update IMPLEMENTATION_STATUS.md

# 5. Update PROJECT_LOG.md

# 6. Validate
.\tools\docs\validate_docs.ps1
```

### Spec + Refinement relacionado

```powershell
# Same as above, plus:

# Move refinement
Move-Item "docs/refinements/a_implementar/pre_refinamentos/ref_spec12_*.md" `
          "docs/refinements/implementados/"

# Update refinement maps
```

## Checklist de auditoria

Antes de finalizar a migration:

- [ ] Spec file existe em implementados/ (não em a_implementar/)
- [ ] Refinement file existe em implementados/ (se aplicável)
- [ ] Headers de evidência adicionados aos arquivos movidos
- [ ] Registries atualizados e consistentes
- [ ] IMPLEMENTATION_STATUS.md reflete o novo estado
- [ ] PROJECT_LOG.md tem entry com data e deliverables
- [ ] Docs validation: PASS
- [ ] Nenhum link quebrado nos arquivos movidos
- [ ] Nenhuma referência órfã a specs de a_implementar/

## Red Flags (NÃO migrar)

- ❌ Nenhuma mudança de código existe para a spec (claim sem evidência)
- ❌ Validação mostra FAIL e não corrigida
- ❌ Non-regression mostra FAIL
- ❌ Scope da spec amplificado além do que foi implementado
- ❌ Save references mudaram sem schema migration documentada
- ❌ Validação não rodada e sem como documentar a razão

## Saída esperada

```text
Migration Summary
─────────────────

Spec moved:
  Source: .specs/a_implementar/spec_12_player_combat.md
  Dest:   .specs/implementados/spec_12_player_combat.md
  Evidence header: ✓ Added

Refinement (if applicable):
  Source: docs/refinements/a_implementar/pre_refinamentos/ref_spec12_combat.md
  Dest:   docs/refinements/implementados/ref_spec12_combat.md

Registries updated:
  ✓ SPEC_REGISTRY_IMPLEMENTED.md
  ✓ SPEC_REGISTRY_TO_IMPLEMENT.md
  (if they exist)

Documentation updated:
  ✓ IMPLEMENTATION_STATUS.md
  ✓ PROJECT_LOG.md
  ✓ Docs validation: PASS

Status: READY FOR CLOSEOUT
```

## Relacionados

- **Spec Execution Skill** → Chama esta no Phase 4: Closeout
- **Implementation Closeout** → Depende desta para o estado final de docs
- **Docs Health Check** → Verifica que os registries permanecem consistentes após a migration
