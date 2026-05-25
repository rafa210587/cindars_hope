# /implement-spec

Execute uma spec inteira com validação e closeout obrigatório e automático.

**Argumento esperado:** `$ARGUMENTS` - número ou nome da spec (ex: "spec_12" ou "12" ou "spec_12_player_combat")

## Fluxo Completo Automático

Este comando orquestra o fluxo completo:

1. **Preparação** — Lê spec, identifica escopo, dependências
2. **Implementação** — Código conforme spec, sem ampliar escopo
3. **Validação Documental** — Automática se docs alterados
4. **Validação Unity** — Automática se runtime alterado (Assets/, ProjectSettings/, Packages/)
5. **Revisão Regressão** — Auditoria automática de violações
6. **Closeout** — Atualiza logs, status, move spec para implementados
7. **Entrega** — Resumo final com evidências

## Fluxo Detalhado

### Fase 0: Preparation (Automática)

**Executado automaticamente:**

1. Ler Camada 0:
   - `AGENTS.md` ou `CLAUDE.md`
   - `PROJECT_LOG.md` (topo/entradas recentes)
   - `docs/IMPLEMENTATION_STATUS.md`
   - `docs/operations/AGENT_EXECUTION_PROTOCOL.md`

2. Ler Camada 1 (spec):
   - `docs/specs/SPEC_SOURCE_OF_TRUTH.md` (se existe)
   - `docs/specs/SPEC_EXECUTION_ORDER.md` (se existe)
   - Spec alvo em `docs/specs/a_implementar/spec_*.md`
   - Refinement relacionado em `docs/refinements/a_implementar/pre_refinamentos/` (se existe)

3. Identificar:
   - Objetivo e escopo
   - Dependências (bloqueadas? prontas?)
   - Arquivos permitidos (apenas no escopo)
   - Arquivos proibidos (docs_old, root specs, etc.)
   - Skills aplicáveis
   - Validações obrigatórias (docs, Unity, regressão)

4. **Registrar plano de preparação e continuar automaticamente:**
   - Objetivo (1-2 sentencas)
   - Escopo exato
   - Dependências
   - Riscos
   - Se bloqueador real detectado, PARAR e reportar
   - Caso contrário, continuar para Fase 1 automaticamente

### Fase 1: Scope Lock

- [ ] Arquivos permitidos identificados
- [ ] Arquivos proibidos listados
- [ ] Escopo confirmado
- [ ] Dependências prontas (ou documentado se bloqueado)

**Se bloqueado:** PARAR. Reportar bloqueador.

### Fase 2: Implementation

- [ ] Implementar conforme spec exatamente
- [ ] Sem ampliar escopo
- [ ] Sem mexer em files fora do escopo
- [ ] Commits frequentes em português
- [ ] Do NOT mover spec para implementados ainda

**Progresso:** Relatar commits ao final de cada fase lógica.

### Fase 3: Validation (Automática)

Após implementação, sistema executa automaticamente:

**3a. Detectar tipo de alteração:**

```powershell
.\.claude\hooks\detect-change-scope.ps1
```

Saída: `.claude/.runtime/change-scope.json` com flags:
- `docsChanged`: docs foram alterados?
- `unityRuntimeChanged`: Assets/**/*.cs, *.unity, *.prefab, *.asset alterados?
- `projectSettingsChanged`: ProjectSettings/** alterado?
- `forbiddenPathsChanged`: docs_old/, specs/, spec/ alterados?
- `rootSpecsRecreated`: specs/ ou spec/ na raiz criadas?

**3b. Executar validações conforme flags:**

```powershell
.\.claude\hooks\run-required-validations.ps1
```

Lógica:
- Se `docsChanged == true`: rodar `tools/docs/validate_docs.ps1`
- Se `unityRuntimeChanged == true` OU `projectSettingsChanged == true`: rodar Unity validation (compile + log scan)
- Se nenhuma alteração: skip
- Se Unity não conseguir rodar: documentar reason e residual risk

**3c. Revisar Regressão:**

Executar `/review-non-regression` para auditar diff:

Verificar:
- Nenhum arquivo proibido alterado
- Nenhuma spec raiz criada
- Nenhuma alteração Unity em tarefa docs-only
- Validações obrigatórias executadas
- Nenhum padrão proibido (GameObject.Find, direct calls, etc.)

### Fase 4: Closeout (Automática)

**Executado automaticamente ao finalizar:**

1. Validações executadas?
   - Docs: ✅ PASS / ⚠️ WARNING / ❌ FAIL / ⊗ NOT RUN
   - Unity: ✅ PASS / ❌ FAIL / ⊗ NOT RUN
   - Regressão: ✅ PASS / ⚠️ WARNING / ❌ FAIL

2. Se todos PASS ou WARNING aceitável:
   - Mover spec: `docs/specs/a_implementar/` → `docs/specs/implementados/`
   - Adicionar evidence header com commit, files, validations
   - Mover refinement relacionado (se existe)
   - Atualizar registries/maps (se existem)

3. Atualizar documentação:
   - `PROJECT_LOG.md` — Adicionar entrada com data, deliverables, validations
   - `docs/IMPLEMENTATION_STATUS.md` — Atualizar status com evidência
   - Rodar docs validation final

4. Gerar resumo final (veja seção abaixo)

## Output Format Final

```markdown
## Resumo Técnico — [SPEC Name]

**Data:** YYYY-MM-DD
**Status:** COMPLETE / PARTIAL / BLOCKED

### Deliverables

[Lista do que foi implementado]

### Arquivos Alterados

[git diff --name-only]

### Commits

[git log --oneline da tarefa]

### Validações Executadas

| Tipo | Resultado | Evidência |
|---|---|---|
| Docs | ✅ PASS | tools/docs/validate_docs.ps1 |
| Unity compile | ✅ PASS | Logs/unity-compile-validation.log |
| Log scan | ✅ PASS | ScanUnityLogs output |
| Non-regression | ✅ PASS | No violations detected |
| Play Mode | ⊗ NOT RUN | Sandboxed environment |

### Validações Não Executadas

[Se alguma não rodou, reason e risk]

### Não-Regressão

Status: PASS / WARNING / FAIL

### Riscos Residuais

[Se houver]

### Pendências

[Se houver]

### Próximo Passo

[Qual spec vem depois, se aplicável]
```

## Regras Obrigatórias

- ✅ **DO:** Implementar spec exatamente conforme escopo
- ✅ **DO:** Rodar validações automáticas
- ✅ **DO:** Mover spec para implementados com evidência
- ✅ **DO:** Atualizar logs e status
- ❌ **DO NOT:** Executar `git push`
- ❌ **DO NOT:** Abrir PR/MR
- ❌ **DO NOT:** Mergear
- ❌ **DO NOT:** Ampliar escopo
- ❌ **DO NOT:** Alterar arquivos fora do escopo
- ❌ **DO NOT:** Marcar como implementado sem evidência

## Fluxo Simplificado (Para Referência Rápida)

```
1. /implement-spec SPEC_NAME
   ↓
2. [Sistema lê spec, registra plano e continua salvo bloqueador real]
   ↓
3. Implementar (sem ampliar)
   ↓
4. [Sistema detecta mudanças]
   ↓
5. [Sistema roda validações apropriadas]
   ↓
6. [Sistema faz closeout automático]
   ↓
7. [Sistema entrega resumo final]
```

## Se Algo Der Errado

- **Validação falha:** Reporta issue, pede fix
- **Regressão detectada:** Para e lista violações
- **Bloqueador encontrado:** Para e reporta
- **Não consegue rodar Unity:** Documenta reason e residual risk

**Importante:** Este comando NÃO faz `git push` ou merge. Entrega artefatos ao usuário para review.

---

**Próximo:** Plano registrado → Implementação continua → System valida → System fecha. Validação humana no final do pacote.
