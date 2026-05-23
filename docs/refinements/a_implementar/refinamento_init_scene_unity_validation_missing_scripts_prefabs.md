# refinamento_init_scene_unity_validation_missing_scripts_prefabs

> **Status:** Refinamento inicial a implementar  
> **Spec futura sugerida:** `spec_scene_unity_validation_missing_scripts_prefabs.md`  
> **Objetivo:** criar validação operacional de Unity para detectar compile errors, Missing Scripts, refs ausentes, assets quebrados, prefabs inválidos e cenas inconsistentes.

---

## 1. Contexto

Algumas estabilizações foram feitas via análise estática/GitHub. Isso corrige código e documentação, mas não substitui validação local no Unity.

O risco principal é ter `.asset`, `.prefab` ou `.unity` salvo com scripts removidos ou referências faltantes.

---

## 2. Gaps

- Não há pipeline automatizado local obrigatório para Unity batchmode.
- Não há scanner de Missing Script em assets/prefabs/scenes.
- Não há relatório padronizado de validação Unity.
- Não há checklist de cenas obrigatórias.
- Não há validação de ScriptableObjects com IDs vazios/duplicados fora dos registries.
- Não há validação de referências críticas de CaveScene/FarmScene/TownScene.

---

## 3. Escopo esperado

### Scripts de validação

Criar scripts PowerShell ou Editor scripts:

```text
tools/unity/RunUnityCompileValidation.ps1
tools/unity/ScanUnityLogs.ps1
Assets/_Game/Scripts/Editor/Validation/MissingScriptScanner.cs
Assets/_Game/Scripts/Editor/Validation/SceneReferenceValidator.cs
Assets/_Game/Scripts/Editor/Validation/DataIdValidator.cs
```

### Batchmode

Rodar:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" `
  -batchmode `
  -quit `
  -projectPath "D:\Projetos\Jogos\Cindars_hope\cindars_hope" `
  -logFile "Logs\unity-compile-validation.log"
```

### Log scan

Buscar:

```text
error CS
Compilation failed
Scripts have compiler errors
NullReferenceException
MissingReferenceException
Missing script
The referenced script on this Behaviour is missing
not found
Failed
AssetDatabase
ScriptableObject
```

### Asset scan

Validar extensões:

```text
*.unity
*.prefab
*.asset
*.controller
```

Buscar Missing Script e referências aos modelos removidos:

```text
CindarsHope.Enemy.EnemyDataSO
CindarsHope.Crafting.CraftingRecipeSO
```

---

## 4. Cenas obrigatórias

Validar pelo menos:

```text
BootScene
FarmScene
TownScene
CaveScene
```

Para cada cena:

- abre sem erro;
- não tem Missing Script;
- managers obrigatórios existem;
- installers têm referências preenchidas;
- player/camera/spawn estão válidos;
- portais têm target scene configurada.

---

## 5. Arquivos prováveis

```text
tools/unity/RunUnityCompileValidation.ps1
tools/unity/ScanUnityLogs.ps1
Assets/_Game/Scripts/Editor/Validation/MissingScriptScanner.cs
Assets/_Game/Scripts/Editor/Validation/SceneReferenceValidator.cs
Assets/_Game/Scripts/Editor/Validation/DataIdValidator.cs
docs/validation/UNITY_VALIDATION_CHECKLIST.md
docs/implementation_runs/RUN_UNITY_VALIDATION_*.md
```

---

## 6. Definition of Done

- [ ] Há comando único para rodar compile validation.
- [ ] Log scanner destaca erros relevantes.
- [ ] Missing Script scanner roda via menu/editor/batchmode.
- [ ] Scene validator valida Boot/Farm/Town/Cave.
- [ ] DataIdValidator encontra IDs vazios/duplicados.
- [ ] Relatório final é salvo em `docs/implementation_runs/`.
- [ ] Validação falha com exit code != 0 se houver erro bloqueante.

---

## 7. Validação manual mínima

```powershell
.\tools\docs\validate_docs.ps1
.\tools\unity\RunUnityCompileValidation.ps1
.\tools\unity\ScanUnityLogs.ps1 -LogFile "Logs\unity-compile-validation.log"
```

Depois abrir Unity e validar manualmente:

1. BootScene.
2. FarmScene.
3. TownScene.
4. CaveScene.
5. Console sem erro vermelho.
6. Sem Missing Script no Inspector.
7. Entrar/sair de cada portal principal.
