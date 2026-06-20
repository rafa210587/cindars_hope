---
name: implementation-closeout
description: Checklist final para fechar qualquer tarefa relevante com validações e documentation. Use ao final de uma spec, bugfix com impacto de gameplay, docs migration, ou refactoring/estabilização significativos.
---

# Skill: Implementation Closeout

Use no fim de qualquer tarefa significativa para garantir que todas as validações, documentation e logging estão completos.

## Quando usar

- Após implementar uma spec
- Após corrigir um bug com impacto de gameplay
- Após migrar documentation
- Após refactoring ou trabalho de estabilização significativos

## Passos obrigatórios de closeout

### Step 1: Verificar Scope

```powershell
git diff --name-only
git status
```

**Checklist:**
- [ ] Todas as mudanças estão dentro do scope da tarefa
- [ ] Nenhuma edição acidental fora dos arquivos permitidos
- [ ] Nenhuma edição em `docs_old/**`
- [ ] Nenhum `specs/` ou `spec/` no root criado
- [ ] Branch está atualizada

### Step 2: Rodar todas as validações aplicáveis

**Docs validation (obrigatória para todas as tarefas):**
```powershell
.\tools\docs\validate_docs.ps1
```
- [ ] Resultado: PASS ✅ ou WARNING ⚠️ preexistente

**Unity compile validation (se runtime mudou):**
```powershell
.\tools\unity\RunUnityCompileValidation.ps1 `
  -UnityEditorPath "..." -ProjectPath "." `
  -LogFile ".\Logs\unity-compile-validation.log"

.\tools\unity\ScanUnityLogs.ps1 `
  -LogFile ".\Logs\unity-compile-validation.log"
```
- [ ] Resultado: PASS ✅ ou NOT RUN com razão documentada

**Non-regression review:**
```
Run /review-non-regression
```
- [ ] Resultado: PASS ✅ ou WARNING ⚠️ aceitável

### Step 3: Atualizar Documentation

**Se implementou uma spec:**
- [ ] Mova a spec de `.specs/a_implementar/` para `.specs/implementados/`
- [ ] Adicione header de evidência com commit, files, validations
- [ ] Atualize registries (se existirem)

**Se completou um refinement:**
- [ ] Mova de `docs/refinements/a_implementar/` para `docs/refinements/implementados/`

**Sempre:**
- [ ] Atualize `docs/IMPLEMENTATION_STATUS.md` com evidência (spec file ou logs validados)
- [ ] Atualize `PROJECT_LOG.md` com entry (data, objetivo, deliverables, validations)

### Step 4: Preparar Delivery Report

**Changed files:**
```
Assembled from: git diff --name-only
```

**Commits:**
```
From: git log --oneline -10
Include: All commits related to this task
```

**Resumo técnico** (1-3 frases):
- O que foi implementado ou corrigido
- Limites de scope
- Decisões-chave (se houver)

**Validações executadas:**
- [ ] Docs validation: PASS / WARNING / FAIL / NOT RUN
- [ ] Unity compile: PASS / FAIL / NOT RUN
- [ ] Log scan: PASS / FAIL / NOT RUN
- [ ] Non-regression: PASS / WARNING / FAIL
- [ ] Play Mode: NOT RUN (reason: sandboxed environment)

**Validações não executadas** (se houver):
- Razão (sandbox, permissions, timeout, etc.)
- Residual risk (o que poderia quebrar)

**Itens pendentes** (se houver):
- Liste o que não foi completado
- Razão
- Bloqueia a próxima spec?

**Residual risks** (se houver):
- Validações ausentes
- Features que não podem ser testadas aqui
- Gotchas conhecidos

**Próximo passo recomendado:**
- Qual spec vem a seguir?
- Algum dependency blocker?
- Alguma follow-up task?

### Step 5: NÃO fazer

- [ ] Executar `git push` (requer aprovação do usuário)
- [ ] Abrir PR/MR (requer aprovação do usuário)
- [ ] Fazer merge de branches (requer aprovação do usuário)
- [ ] Marcar a tarefa como "complete" em sistemas externos sem confirmação do usuário
- [ ] Esconder falhas de validação
- [ ] Declarar compliance sem evidência
- [ ] Commitar mudanças no PROJECT_LOG sem atualizá-lo

## Template de formato de saída

```markdown
## Closeout Report — [SPEC Name]

**Date:** YYYY-MM-DD  
**Branch:** [branch-name]  
**Status:** COMPLETE / PARTIAL / BLOCKED  

### Summary

[1-3 sentences: what was implemented, scope, key decisions]

### Changed Files

```
.specs/a_implementar/spec_12_player_combat.md
.specs/implementados/spec_12_player_combat.md
Assets/Scripts/Runtime/Combat/PlayerCombatManager.cs
Assets/Scripts/Runtime/Combat/WeaponDataSO.cs
Assets/_Game/Data/Combat/weapons-basic.asset
docs/IMPLEMENTATION_STATUS.md
PROJECT_LOG.md
```

### Commits

```
abc1234 feat: spec 12 - player combat melee/ranged attacks
def5678 fix: event bus pattern in attack delivery
```

### Validations Executed

| Validation | Result | Evidence |
|---|---|---|
| Docs validation | ✅ PASS | tools/docs/validate_docs.ps1 |
| Unity compile | ✅ PASS | Logs/unity-compile-validation.log: Tundra build success |
| Log scan | ✅ PASS | No new C# errors |
| Non-regression | ✅ PASS | No GameEvent.Find(), IDs in save, events published |
| Play Mode | ⊗ NOT RUN | Reason: sandboxed, cannot launch game client |

### Validations Not Executed

- **Play Mode features:** Cannot test game loop in sandbox
  - Risk: Weapon swap, damage calculation, status effects await user testing
  - Mitigation: User can manually test in Unity Editor

### Deliverables

- ✅ PlayerCombatManager with melee/ranged attack logic
- ✅ WeaponDataSO with stats (damage, range, cooldown)
- ✅ DamageRequest published via GameEventBus
- ✅ Weapon hotbar selection (if UI scope allowed)

### Pending

- (none) — task complete

### Residual Risks

- (none) — all validations passed

### Next Step

- SPEC 13: NPC Combat & Tactics
- Dependency: SPEC 12 (✅ complete)
- Ready to implement

---

Delivered for user review. Ready for merge pending user approval.
```

## Formato abreviado (para tarefas menores)

```markdown
## Closeout — Bug Fix: [Issue]

**Status:** COMPLETE

**Changed:** 
- Assets/Scripts/Runtime/Path/File.cs

**Validation:** 
- Docs: PASS
- Unity: PASS
- Non-regression: PASS

**Risk:** None

**Next:** [if applicable]
```

## Red Flags (NÃO entregar)

- ❌ Validação mostra FAIL e não foi corrigida
- ❌ Mudanças fora do scope da tarefa e não reconhecidas
- ❌ Non-regression mostra FAIL
- ❌ Spec marcada como implementada sem evidência
- ❌ Documentation não atualizada
- ❌ PROJECT_LOG não atualizado para tarefa significativa

## Checklisting

Antes de entregar o report:

- [ ] Todos os changed files listados
- [ ] Todas as validações aplicáveis rodadas e documentadas
- [ ] Non-regression audit completa
- [ ] Docs atualizados e validados
- [ ] PROJECT_LOG atualizado
- [ ] IMPLEMENTATION_STATUS atualizado (se spec/capability)
- [ ] Summary preciso
- [ ] Nenhuma falha escondida
- [ ] Nenhuma claim sem evidência
- [ ] Próximo passo recomendado

## Relacionados

- **Spec Execution** → Chama esta no Phase 4
- **Finish-Spec** → Depende desta para o report final
- **Non-Regression Review** → Fornece o input da auditoria
- **Docs Migration** → Atualiza os docs para esta skill usar
- **Unity Validation** → Fornece a evidência de validação

---

**O closeout NÃO está completo até o report ser gerado e entregue ao usuário.**
