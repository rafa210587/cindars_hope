# /review-non-regression

Audita o diff atual contra as regras de não-regressão do projeto e o change scope detectado.

**Argumento esperado:** (opcional) Forçar auditoria completa mesmo sem mudanças detectadas

## Pré-voo: checar o change scope

Primeiro, leia `.claude/.runtime/change-scope.json` (se existir, gerado por `/implement-spec` ou pelo hook detect-change-scope):

```json
{
  "docsChanged": true,
  "unityRuntimeChanged": false,
  "projectSettingsChanged": false,
  "forbiddenPathsChanged": false,
  "rootSpecsRecreated": false,
  "specDocsChanged": true,
  "specMigrationDetected": true,
  "refinementDocsChanged": false,
  "refinementMigrationDetected": false
}
```

**Falhas críticas (FAIL imediato):**
- `forbiddenPathsChanged == true` (docs_old/, root specs/)
- `rootSpecsRecreated == true` (specs/ ou spec/ criado na raiz)

**Ações legítimas (NÃO são falhas):**
- `specMigrationDetected == true` durante o closeout de `/implement-spec`
- `refinementMigrationDetected == true` durante o closeout de spec

## Checagens obrigatórias

### 1. Estrutura de arquivos

- [ ] Nenhuma criação de diretórios `specs/` ou `spec/` na raiz
- [ ] Nenhuma edição em `docs_old/**`
- [ ] Nenhuma edição fora do escopo permitido

### 2. Segurança de Git

- [ ] Nenhum `git push` executado
- [ ] Nenhum `git reset --hard` executado
- [ ] Nenhum `git clean` executado
- [ ] Nenhum `git stash` executado
- [ ] Branch limpo ou apenas com os commits pretendidos

### 3. Segurança de gameplay/runtime (se for tarefa de runtime)

- [ ] Nenhuma chamada `GameObject.Find()` introduzida
- [ ] Nenhuma chamada `FindObjectOfType()` introduzida
- [ ] Nenhuma chamada `FindObjectsByType()` introduzida
- [ ] Nenhuma comunicação direta MonoBehaviour-para-MonoBehaviour (use GameEventBus)
- [ ] Save não serializa refs UnityEngine (sem GameObject, Transform, MonoBehaviour, Sprite, Collider, Rigidbody)
- [ ] Save usa apenas IDs e simple types
- [ ] Nenhum game data hardcoded em MonoBehaviour (use ScriptableObject)
- [ ] Sem `StreamingAssets` para save data editável (use Application.persistentDataPath)
- [ ] ScriptableObjects com prefixo correto (ItemDataSO, WeaponDataSO, etc.)
- [ ] Events com prefixo correto (DayStartedEvent, ItemCraftedEvent, etc.)
- [ ] Nenhum namespace proibido criado (`CindarsHope.Debug`, etc.)

### 4. Evidência de runtime (se disponível)

Inspecione o diretório `.claude/.runtime/` em busca de arquivos de evidência:

- [ ] `.claude/.runtime/change-scope.json` — detecção de mudança persistida
- [ ] `.claude/.runtime/validation-results.json` — resultados de validação persistidos

Use-os como evidência para as validações obrigatórias.

### 5. Validações executadas (se usando /implement-spec)

Se `.claude/.runtime/validation-results.json` existir:

- [ ] Docs validation: PASS ou WARNING aceitável
- [ ] Unity compile: PASS ou NOT RUN documentado
- [ ] Log scan: PASS ou NOT RUN documentado
- [ ] Status geral: PASS/FAIL/WARNING

Se os arquivos estiverem ausentes, cheque o output do console em busca de evidência de validação.

### 6. Segurança de spec/roadmap (se for tarefa de spec)

- [ ] A tarefa não excede o escopo da spec
- [ ] Não implementou spec blocked/future (cheque SPEC_EXECUTION_ORDER.md)
- [ ] Não pulou specs na ordem errada
- [ ] Atualizações de status têm evidência no repo (código, assets ou logs validados)

### 7. Segurança de documentação

- [ ] Nenhuma spec marcada como implemented sem evidência
- [ ] Mudanças em IMPLEMENTATION_STATUS.md refletem o estado real
- [ ] PROJECT_LOG.md atualizado quando a tarefa foi significativa
- [ ] Sem entradas duplicadas em registries

## Saída esperada

```text
Status: PASS | WARNING | FAIL

Evidence:
  Changed files: <list>
  Scope: <task scope>
  Validations: <which were run>

Critical files analyzed:
  git diff --name-only: <output>
  Key changes: <summary>

Issues found:
  [If any]

Corrective actions required:
  [If any - mandatory to fix]

Residual risk:
  [If any]
```

## Exemplos

### PASS

```text
Status: PASS

Evidence:
  Changed files:
    - .specs/implementados/spec_12_player_combat.md
    - Assets/Scripts/Runtime/Combat/PlayerCombatManager.cs
    - Assets/_Game/Data/Combat/WeaponDataSO.cs

  Scope: SPEC 12 - Player Combat (approved, no overshoot)
  Validations: Docs PASS, Unity compile PASS

Critical files analyzed:
  - No docs_old/** edits ✓
  - No root specs/ ✓
  - No GameObject.Find() ✓
  - Save contract untouched ✓
  - Event bus used for combat events ✓

Issues found:
  None

Residual risk:
  Play Mode features await manual user testing
```

### WARNING

```text
Status: WARNING

Issues found:
  - DamageAppliedEvent now carries Transform reference (was TargetPosition ID before)

Corrective actions required:
  Change DamageAppliedEvent.TargetTransform → DamageAppliedEvent.TargetPositionId
  Verify no save serialization of this event

Residual risk:
  Runtime may break if event is serialized; needs validation in Play Mode
```

### FAIL

```text
Status: FAIL

Issues found:
  - Attempted: git push (BLOCKED)
  - Found: GameObject.Find() in PlayerCombatManager.cs:123
  - Found: Save serializing WeaponDataSO ref (forbidden)

Corrective actions required:
  1. Remove GameObject.Find() → use GameEventBus publish/subscribe
  2. Change save to store WeaponID (int) instead of WeaponDataSO ref
  3. Do NOT execute git push

Residual risk:
  CRITICAL - Task cannot proceed until these are fixed
```

---

## NÃO faça

- Ignorar warnings e dizer que está "perto o bastante"
- Pular esta checagem e afirmar que está tudo certo
- Marcar FAIL como aceitável para tarefa de produção
- Corrigir issues silenciosamente sem documentar em PROJECT_LOG.md

**Se a auditoria mostrar FAIL:** corrija as issues e rode este command de novo.
