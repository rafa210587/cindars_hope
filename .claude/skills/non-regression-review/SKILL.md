---
name: non-regression-review
description: Audita o diff de uma implementação em busca de violações arquiteturais e riscos de regressão (forbidden search APIs, breach de scope, Unity refs em save DTOs, claims de status falsos). Use após implementar uma spec, antes do closeout, ou quando scope/rules mudaram de forma significativa.
---

# Skill: Non-Regression Review

Use para auditar mudanças em busca de violações das rules do projeto e dos patterns arquiteturais.

## Quando usar

- Após a implementação de uma spec
- Antes do closeout da tarefa
- Sempre que scope ou rules mudaram de forma significativa
- Como sanity check antes da aprovação do usuário

## Itens obrigatórios de auditoria

### 1. Estrutura de File & Directory

```
Check git diff --name-only for:
```

- [ ] Nenhuma criação de diretório `specs/` no root
- [ ] Nenhuma criação de diretório `spec/` no root
- [ ] Nenhuma edição em `docs_old/**` (apenas archive)
- [ ] Todas as mudanças dentro do scope permitido

**Action:** Se violado, desfaça as mudanças e reimplemente dentro do scope.

### 2. Git Safety

```
Check git log and git status:
```

- [ ] Nenhum `git push` executado (push futuro aguarda aprovação do usuário)
- [ ] Nenhum `git reset --hard` executado
- [ ] Nenhum `git clean` executado
- [ ] Nenhum `git stash` executado
- [ ] Branch limpa ou só com os commits pretendidos

**Action:** Restaure do backup se uma operação destrutiva foi executada.

### 3. Runtime/Gameplay Safety (se C# mudou)

```
Grep for violations:
```

- [ ] Nenhuma chamada `GameObject.Find()` (use GameEventBus ou refs do Bootstrap)
- [ ] Nenhuma chamada `FindObjectOfType()`
- [ ] Nenhuma chamada `FindObjectsByType()`
- [ ] Nenhuma chamada direta MonoBehaviour-to-MonoBehaviour (use GameEventBus.Publish/Subscribe)

**Exemplo de violação de pattern:**
```csharp
// ❌ WRONG
var enemy = FindObjectOfType<EnemyHealth>();
enemy.TakeDamage(damage);

// ✅ RIGHT
GameEventBus.Publish(new DamageAppliedEvent 
{ 
  TargetId = targetId, 
  DamageAmount = damage 
});
```

**Action:** Refatore para usar GameEventBus.

### 4. Save Data Safety

- [ ] O save NÃO serializa refs de `ScriptableObject`
- [ ] O save NÃO serializa refs de `GameObject`
- [ ] O save NÃO serializa refs de `Transform`
- [ ] O save NÃO serializa refs de `MonoBehaviour`
- [ ] O save NÃO serializa refs de `Sprite`
- [ ] O save NÃO serializa refs de `Collider`
- [ ] O save NÃO serializa refs de `Rigidbody`
- [ ] O save usa IDs e simple types (int, string, float, bool)
- [ ] O save usa `Application.persistentDataPath` (não `StreamingAssets`)

**Exemplo de violação de pattern:**
```csharp
// ❌ WRONG
[System.Serializable]
class ItemSaveData
{
    public ItemDataSO itemData; // Serializing ScriptableObject!
    public Transform dropTransform; // Serializing Transform!
}

// ✅ RIGHT
[System.Serializable]
class ItemSaveData
{
    public int itemId; // ID only
    public float dropPositionX, dropPositionY;
}
```

**Action:** Refatore a estrutura de save para usar apenas IDs.

### 5. Game Data em código

- [ ] Nenhum número de balancing hardcoded em MonoBehaviour
- [ ] Todo game data em ScriptableObject com o prefix correto (ItemDataSO, WeaponDataSO, etc.)
- [ ] ScriptableObjects referenciados corretamente a partir de `Assets/_Game/Data/`

**Exemplo de violação de pattern:**
```csharp
// ❌ WRONG
public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f; // Hardcoded!
}

// ✅ RIGHT
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private PlayerDataSO playerData;
    public float maxHealth => playerData.MaxHealth;
}
```

**Action:** Mova o data para ScriptableObject.

### 6. Uso do Event Bus

- [ ] Comunicação de gameplay usa `GameEventBus.Publish()`
- [ ] Todos os subscribers de event têm `Unsubscribe()` em `OnDisable` ou `OnDestroy`
- [ ] Events têm o prefix correto: `*Event` (DayStartedEvent, ItemCraftedEvent, etc.)
- [ ] Events carregam payload, não refs a sistemas

**Exemplo de violação de pattern:**
```csharp
// ❌ WRONG
public class ItemManager : MonoBehaviour
{
    public void OnItemUsed(ItemDataSO item)
    {
        GetComponent<PlayerStats>().AddExperience(item.ExpGain);
    }
}

// ✅ RIGHT
public class ItemManager : MonoBehaviour
{
    public void OnItemUsed(int itemId)
    {
        GameEventBus.Publish(new ItemUsedEvent { ItemId = itemId });
    }
}
```

**Action:** Refatore para publicar events.

### 7. Spec Execution Order

- [ ] Não implementou specs à frente dos blockers de SPEC_EXECUTION_ORDER.md
- [ ] Não pulou specs na dependency chain
- [ ] Consultou SPEC_EXECUTION_ORDER.md antes de começar

**Action:** Verifique as dependencies em `.specs/SPEC_EXECUTION_ORDER.md`.

### 8. Namespace Safety

- [ ] Nenhum namespace chamado `CindarsHope.Debug` criado
- [ ] Usou as alternativas: `CindarsHope.Runtime`, `CindarsHope.DebugTools`, `CindarsHope.Diagnostics`, ou `CindarsHope.Editor`

**Action:** Renomeie qualquer namespace proibido.

### 9. Integridade de Status & Documentation

- [ ] Nenhuma spec marcada como implementada sem evidência no repo
- [ ] Claims de IMPLEMENTATION_STATUS.md batem com o estado real de code/asset
- [ ] PROJECT_LOG.md atualizado quando a tarefa foi significativa
- [ ] Nenhuma entry órfã em registries

**Action:** Forneça evidência ou remova a claim.

## Formato de saída da auditoria

```text
Non-Regression Audit Report
───────────────────────────

Status: PASS | WARNING | FAIL

File & Directory Structure:
  ✓ No root specs/ or spec/ created
  ✓ No docs_old/** edits
  ✓ All changes within scope

Git Safety:
  ✓ No destructive operations
  ✓ Branch clean

Runtime Safety (C# changes):
  ✓ No GameObject.Find() or FindObjectOfType()
  ✓ No direct system calls
  ✓ GameEventBus used for gameplay communication

Save Data (if persistence task):
  ✓ No Unity refs serialized (only IDs and simple types)
  ✓ Using Application.persistentDataPath

Game Data:
  ✓ No hardcoded balancing in MonoBehaviour
  ✓ All game data in ScriptableObjects

Events:
  ✓ Proper event prefix (* Event)
  ✓ Unsubscribe in OnDisable/OnDestroy

Spec Order:
  ✓ Respects SPEC_EXECUTION_ORDER.md

Namespaces:
  ✓ No forbidden CindarsHope.Debug namespace

Status & Docs:
  ✓ IMPLEMENTATION_STATUS.md has evidence
  ✓ PROJECT_LOG.md current
  ✓ No orphaned claims

Issues found:
  (if any)

Corrective actions required:
  (if any)

Residual risk:
  (if any)
```

## Exemplos

### PASS

```text
Status: PASS

Summary:
- 3 files changed: PlayerCombatManager.cs, WeaponDataSO.cs, Player.cs
- Scope: SPEC 12 (approved)
- No violations detected
- All patterns followed

Ready for task closeout.
```

### WARNING

```text
Status: WARNING

Issues found:
  - DamageAppliedEvent now carries Transform (was int TargetId before)
  - Implies event may be serialized with Transform ref

Corrective actions required:
  Change DamageAppliedEvent.TargetTransform → DamageAppliedEvent.TargetPositionId (int)
  Verify save does not serialize this event

Residual risk:
  Minor: Runtime may serialize Transform unexpectedly
```

### FAIL

```text
Status: FAIL

Issues found:
  - Found: GetComponent<EnemyHealth>().TakeDamage() in PlayerCombat.cs:42 (direct call!)
  - Found: Save serializing WeaponDataSO reference in EquipmentSaveData
  - Found: Hardcoded maxHealth = 100f in PlayerHealth.cs

Corrective actions REQUIRED:
  1. Refactor PlayerCombat.GetComponent call → Use GameEventBus.Publish
  2. Change EquipmentSaveData to store weaponId (int) instead of WeaponDataSO
  3. Move maxHealth to PlayerDataSO

Residual risk:
  CRITICAL: Task cannot proceed to closeout until these are fixed.
```

## Regras

- [ ] NÃO declare PASS sem checar todos os 9 itens
- [ ] NÃO ignore WARNING (sinais precoces de problemas maiores)
- [ ] NÃO aceite FAIL sem corrigir
- [ ] NÃO esconda violações no summary
- [ ] NÃO declare compliance sem evidência

## Relacionados

- **Spec Execution** → Chama esta antes do Phase 4: Closeout
- **Implementation Closeout** → Requer um resultado PASS/WARNING/FAIL
- **Finish-Spec** → Não pode completar sem esta auditoria
