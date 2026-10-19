# SPEC_11 Execution Report - Wave 6 Bootstrap Installers

**Date:** 2026-06-01  
**Branch:** dev  
**Executor:** Claude Code (Haiku mode)  
**Mode:** Sequential  
**Spec ID:** spec_arch_reorg_11_wave6_bootstrap_installers  

---

## Objetivo da Spec

Criar CombatRuntimeInstaller como piloto de instaladores bootstrap explícitos para validação de wiring do domínio combat, sem alterar lifecycle do GameBootstrap, sem scene/prefab edits, sem FindObjectOfType, sem AddComponent fallback novo. Logs de erro explícitos para refs required ausentes (FR-004: no silent fallback).

---

## O que foi feito

### T-001: Criar CombatRuntimeInstallContext

**Arquivo criado:** `Assets/_Game/Scripts/Core/Bootstrap/Installers/CombatRuntimeInstallContext.cs`

POCO [Serializable] com 8 campos:
```csharp
[Serializable]
public class CombatRuntimeInstallContext
{
    public ItemDatabaseSO ItemDatabase;
    public WeaponDatabaseSO WeaponDatabase;
    public SpellDatabaseSO SpellDatabase;
    public StatusEffectDatabaseSO StatusEffectDatabase;
    public EquipmentManager EquipmentManager;
    public InventoryManager InventoryManager;
    public StaminaManager StaminaManager;
    public ManaManager ManaManager;
}
```

Required: ItemDatabase, WeaponDatabase, SpellDatabase, EquipmentManager.  
Optional: StatusEffectDatabase, InventoryManager, StaminaManager, ManaManager.

### T-002: Criar CombatRuntimeInstaller

**Arquivo criado:** `Assets/_Game/Scripts/Core/Bootstrap/Installers/CombatRuntimeInstaller.cs`

Static class com Install(context, owner):
- context null → LogError + return
- ItemDatabase null → LogError (Required)
- WeaponDatabase null → LogError (Required)
- SpellDatabase null → LogError (Required)
- EquipmentManager null → LogError (Required)
- StaminaManager null → LogWarning (Optional — pode ser ausente em cenas não-combat)
- ManaManager null → LogWarning (Optional — pode ser ausente em cenas não-spell)
- Sucesso → Debug.Log com inventário de campos resolvidos

Princípio FR-004: nenhum fallback silencioso; wiring ausente é sempre logado.

### T-003: Delegate GameBootstrap

**Arquivo modificado:** `Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs`

Mudanças:
1. Adicionado `using CindarsHope.Core.Bootstrap.Installers;`
2. Novo método privado:
```csharp
private CombatRuntimeInstallContext BuildCombatInstallContext()
{
    return new CombatRuntimeInstallContext
    {
        ItemDatabase = _itemDatabase,
        WeaponDatabase = _weaponDatabase,
        SpellDatabase = _spellDatabase,
        StatusEffectDatabase = _statusEffectDatabase,
        EquipmentManager = _equipmentManager,
        InventoryManager = _inventoryManager,
        StaminaManager = _staminaManager,
        ManaManager = _manaManager
    };
}
```
3. Em `InitializeManagers()`, após `RebindOptionalRuntimeManagers(...)` e antes de `InitializeDeathSystem()`:
```csharp
CombatRuntimeInstaller.Install(BuildCombatInstallContext(), this);
```

Lifecycle preservado. DontDestroyOnLoad preservado. Getters públicos preservados. Nenhuma scene/YAML edit necessária.

### T-004: Atualizar Geradores

**Não necessário.** Installer é class-only; nenhum novo componente de cena criado. Geradores de cena não precisam de alteração.

### T-005: Atualizar MvpSceneValidator

**Arquivo modificado:** `Assets/_Game/Scripts/Editor/Validation/MvpSceneValidator.cs`

1. Adicionado novo método:
```csharp
private static bool ValidateSpec11CombatDatabases(GameBootstrap bootstrap)
{
    var passed = true;
    if (bootstrap.ItemDatabase == null)
        { Debug.LogError("MvpSceneValidator: SPEC_11 requires ItemDatabase wired on GameBootstrap."); passed = false; }
    if (bootstrap.WeaponDatabase == null)
        { Debug.LogError("MvpSceneValidator: SPEC_11 requires WeaponDatabase wired on GameBootstrap."); passed = false; }
    if (bootstrap.SpellDatabase == null)
        { Debug.LogError("MvpSceneValidator: SPEC_11 requires SpellDatabase wired on GameBootstrap."); passed = false; }
    return passed;
}
```
2. Chamado em `ValidateCaveScene()` após `ValidateSpec09Bootstrap(bootstrap)`.

### T-006: Assembly-CSharp.csproj

Adicionados após `StatusEffectDatabaseSO.cs`:
```xml
<Compile Include="Assets\_Game\Scripts\Core\Bootstrap\Installers\CombatRuntimeInstallContext.cs" />
<Compile Include="Assets\_Game\Scripts\Core\Bootstrap\Installers\CombatRuntimeInstaller.cs" />
```

---

## Validações Executadas

| Validacao | Resultado | Detalhes |
|-----------|-----------|----------|
| `dotnet restore Assembly-CSharp.csproj` | ✓ PASS | Todos projetos atualizados |
| `dotnet restore Assembly-CSharp-Editor.csproj` | ✓ PASS | Todos projetos atualizados |
| `dotnet build Assembly-CSharp.csproj --no-restore` | ✓ PASS 0E/0W | 1.51s |
| `dotnet build Assembly-CSharp-Editor.csproj --no-restore` | ✓ PASS 0E/2W | 2 warnings pre-existentes em CreateEnemyActionsAndSets.cs; 0.81s |
| `tools/docs/validate_docs.ps1` | ✓ PASS | 14/14 checks OK |
| Unity validation | NOT RUN | Motivo: code-only change; validators editor-only. Nenhum asset novo criado. |

---

## Verificacao de Premissas

| Premissa | Status | Evidencia |
|----------|--------|-----------|
| CombatRuntimeInstaller não altera lifecycle do GameBootstrap | ✓ OK | Install() é read-only (só loga); nenhuma mutação de estado |
| Context buildo de campos existentes em GameBootstrap | ✓ OK | BuildCombatInstallContext() usa campos já presentes; zero novos SerializeField |
| Sem scene/prefab edits | ✓ OK | Nenhum arquivo .unity ou .prefab modificado |
| Sem FindObjectOfType | ✓ OK | Não usado em nenhum arquivo modificado |
| DontDestroyOnLoad preservado | ✓ OK | Awake() inalterado |
| PlayerAttackController self-wire via GameBootstrap.Instance | ✓ OK | Start() inalterado |
| SPEC_09 StatusEffectDatabase preservado | ✓ OK | Getter permanece; também incluído no context |
| Getters públicos preservados | ✓ OK | Zero remoções em GameBootstrap |
| Build C# passa | ✓ PASS | 0 erros runtime, 0 erros editor (2 warnings pre-existentes) |

---

## Comportamento Preservado

✓ GameBootstrap lifecycle inalterado (Awake → InitializeManagers → InitializeDeathSystem)  
✓ DontDestroyOnLoad preservado  
✓ Getters públicos preservados  
✓ PlayerAttackController.Start() resolve via GameBootstrap.Instance sem mudança  
✓ SPEC_09 StatusEffectDatabase wiring preservado  
✓ Nenhum wiring adicional em scenes, prefabs ou YAML  
✓ Nenhuma mudança em Player/Inventory/Equipment/Farm/World/Cave/Death/Economy/Crafting/Stamina/GameTime/StatusEffects/Bestiary  
✓ Nenhuma mudança em save schema, migrations, save path  

---

## Riscos Residuais

1. **Databases null em scenes antigas** — Baixo. Install() loga LogError mas não impede execução. Scenes existentes já tem ItemDatabase/WeaponDatabase/SpellDatabase wireados (campos existiam antes da SPEC_11).

2. **Unity Play Mode não testado** — Muito baixo. Install() é lógica read-only (null checks + Debug.Log); nenhuma mudança em fluxo de gameplay.

---

## Arquivos Criados/Modificados

**Criados:** 2
- `Assets/_Game/Scripts/Core/Bootstrap/Installers/CombatRuntimeInstallContext.cs`
- `Assets/_Game/Scripts/Core/Bootstrap/Installers/CombatRuntimeInstaller.cs`

**Modificados:** 3
- `Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs`
- `Assets/_Game/Scripts/Editor/Validation/MvpSceneValidator.cs`
- `Assembly-CSharp.csproj`

---

## Checklist Final

- [x] Arquivos obrigatórios lidos (SPEC_11, GameBootstrap.cs, MvpSceneValidator.cs)
- [x] Escopo respeitado (Bootstrap/Installers/ e Editor/Validation/)
- [x] Nenhum arquivo proibido alterado (schema, migrations, save path, cenas, prefabs)
- [x] Nenhum FindObjectOfType introduzido
- [x] Nenhum fallback silencioso criado
- [x] Nenhum DI framework criado
- [x] GameBootstrap lifecycle preservado
- [x] DontDestroyOnLoad preservado
- [x] Build runtime executado (PASS 0E/0W)
- [x] Build editor executado (PASS 0E/2W pre-existentes)
- [x] validate_docs.ps1 executado (PASS)
- [x] Unity validation marcado como NOT RUN com motivo
- [x] Relatório criado
- [x] PROJECT_LOG.md atualizado

---

## Relatório Final

**Status:** ✓ COMPLETO

**Arquivos Criados:** 2 (CombatRuntimeInstallContext.cs, CombatRuntimeInstaller.cs)  
**Arquivos Modificados:** 3 (GameBootstrap.cs, MvpSceneValidator.cs, Assembly-CSharp.csproj)  
**Build Status:** PASS (0E/0W runtime, 0E/2W pre-existentes editor)  
**Behavior Alterado:** 0 (Install() é read-only — só loga refs ausentes)  
**Risco Residual:** Muito baixo  

---

## Próxima Etapa

✓ **SPEC_12 LIBERADA**

Pré-requisitos confirmados:
- CombatRuntimeInstaller criado e integrado em GameBootstrap
- CombatRuntimeInstallContext buildado de campos existentes
- MvpSceneValidator atualizado com ValidateSpec11CombatDatabases
- Build validação completa
- Padrão installer escalável para próximos domínios
