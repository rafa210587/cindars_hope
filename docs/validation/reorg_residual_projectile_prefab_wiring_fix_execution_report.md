# Residual Projectile Prefab Wiring Fix Execution Report

**Date:** 2026-06-01  
**Branch:** dev  
**Executor:** Claude Code (Haiku mode)  
**Mode:** Residual Fix — pós Play Mode validator checkpoint  
**Spec ID:** reorg_residual_projectile_prefab_wiring_fix  

---

## Contexto

Após SPEC_12 closeout e residual asset fix, o validator `ProjectilePrefabValidator` ainda reportava 2 erros críticos:
1. `weapon_bow_basic.ProjectilePrefab == null` (Expected: Projectile_Arrow.prefab)
2. `spell_fireball.ProjectilePrefab == null` (Expected: Projectile_Fireball.prefab)

Apesar dos prefabs existirem e os campos estarem supostamente preenchidos no YAML, o Unity carregava-os como null. Solução: criar script editor de repair que wira via UnityEditor API.

---

## Tarefas Executadas

### T-001 — Criar RepairProjectilePrefabReferences.cs

**Arquivo criado:** `Assets/_Game/Scripts/Editor/Validation/RepairProjectilePrefabReferences.cs`

**Funcionalidade:**
- Menu item: `CindarsHope/Repair/Combat/Repair Projectile Prefab References`
- Method `RepairProjectilePrefabReferences()`:
  - Carrega `weapon_bow_basic.asset` via `AssetDatabase.LoadAssetAtPath<WeaponDataSO>()`
  - Carrega `Projectile_Arrow.prefab` via `AssetDatabase.LoadAssetAtPath<GameObject>()`
  - Atribui `arrowPrefab` ao campo `weapon.ProjectilePrefab`
  - Marca asset como dirty: `EditorUtility.SetDirty(weapon)`
  - Carrega `spell_fireball.asset`
  - Carrega `Projectile_Fireball.prefab`
  - Atribui `fireballPrefab` ao campo `spell.ProjectilePrefab`
  - Marca asset como dirty: `EditorUtility.SetDirty(spell)`
  - Log status PASS/FAIL para cada repair

- Method `RepairViaCommandLine()`:
  - Alternativa `-executeMethod` para batchmode
  - Chama `RepairProjectilePrefabReferences()`
  - Chama `AssetDatabase.SaveAssets()`
  - Chama `AssetDatabase.Refresh()`

**Não altera:**
- `ProjectileBehaviour`
- `PlayerAttackController`
- `BowArrowAttackService`
- `SpellCastService`
- Damage, range, speed, mana, cooldown, status chance

**Status:** CREATED & COMPILES

---

## Validações C# Executadas

| Validacao | Resultado | Detalhes |
|-----------|-----------|----------|
| `dotnet restore Assembly-CSharp.csproj` | ✓ PASS | Projetos atualizados |
| `dotnet restore Assembly-CSharp-Editor.csproj` | ✓ PASS | Projetos atualizados |
| `dotnet build Assembly-CSharp.csproj --no-restore` | ✓ PASS 0E/0W | 0.45s |
| `dotnet build Assembly-CSharp-Editor.csproj --no-restore` | ✓ PASS 0E/2W | 2 pre-existentes; 2.33s |
| `tools/docs/validate_docs.ps1` | ✓ PASS | 14/14 checks OK |

**Status:** ALL PASS

---

## Como Usar o Repair Script

### Via Menu no Unity Editor

1. Abrir Unity Project
2. Navigar para menu: `CindarsHope/Repair/Combat/Repair Projectile Prefab References`
3. Clique
4. Verificar Console para PASS/FAIL logs
5. Assets serão salvos automaticamente

### Via Batchmode Command Line

```bash
Unity -projectPath . -executeMethod CindarsHope.EditorTools.Repair.RepairProjectilePrefabReferences.RepairViaCommandLine -quit
```

### Verificação Pós-Repair

Após rodar o repair, executar validators:
1. `CindarsHope/Validate/Combat/Validate Projectile Prefabs` — esperado PASS
2. `CindarsHope/Validate/Combat/Validate Combat Databases` — esperado 0 ERRORS

---

## Assets Afetados

| Asset | Campo | Antes | Depois |
|-------|-------|-------|--------|
| `weapon_bow_basic.asset` | `ProjectilePrefab` | null | Projectile_Arrow.prefab (ref) |
| `spell_fireball.asset` | `ProjectilePrefab` | null | Projectile_Fireball.prefab (ref) |

**Não altera:** Damage, range, speed, mana cost, cooldown, status effect, nem nenhum outro campo.

---

## Comportamento Preservado

✓ Nenhuma mudança em gameplay  
✓ Nenhuma mudança em Q/E/Space  
✓ Nenhuma mudança em PlayerAttackController  
✓ Nenhuma mudança em BowArrowAttackService  
✓ Nenhuma mudança em SpellCastService  
✓ Nenhuma mudança em ProjectileBehaviour  
✓ Nenhuma mudança em save schema  
✓ Fallback Resources.Load preservado  
✓ Build C# runtime: 0E/0W  
✓ Build C# editor: 0E/2W (pre-existentes)  

---

## Checklist Final

- [x] Script editor de repair criado
- [x] Menu item implementado: `CindarsHope/Repair/Combat/Repair Projectile Prefab References`
- [x] Batchmode method implementado: `RepairViaCommandLine()`
- [x] AssetDatabase API usado (não YAML manual)
- [x] EditorUtility.SetDirty() chamado
- [x] Build C# runtime executado: PASS
- [x] Build C# editor executado: PASS (compila com novo script)
- [x] Docs validation executado: PASS
- [x] Nenhuma mudança em gameplay ou code runtime
- [x] Relatório criado

---

## Relatório Final

**Status:** ✓ COMPLETE — Repair script criado e pronto para uso

**Arquivo Criado:** 1 (RepairProjectilePrefabReferences.cs)  
**Build Status:** PASS (0E/0W runtime, 0E/2W editor pre-existentes)  
**Regressão:** 0 (script é editor-only, não afeta runtime)  

---

## Próxima Etapa

**Executar no Unity Editor:**
1. Abrir projeto em Unity
2. Menu: `CindarsHope/Repair/Combat/Repair Projectile Prefab References`
3. Confirmar PASS logs no Console
4. Rodar validators:
   - `CindarsHope/Validate/Combat/Validate Projectile Prefabs` — esperado PASS
   - `CindarsHope/Validate/Combat/Validate Combat Databases` — esperado 0 ERRORS

**Se validators passarem:** Repair fix completo, pronto para Play Mode validation.

---

## Conclusão

Repair script editor criado para wirer ProjectilePrefab refs que não estavam resolvendo corretamente. Script compila, não afeta runtime, usa UnityEditor API (não YAML manual). Pronto para ser rodado no Unity Editor via menu ou batchmode.
