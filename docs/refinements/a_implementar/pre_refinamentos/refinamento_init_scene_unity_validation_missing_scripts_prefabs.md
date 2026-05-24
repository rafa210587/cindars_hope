# refinamento_init_scene_unity_validation_missing_scripts_prefabs

> **Status:** Refinamento inicial a implementar
> **Spec futura sugerida:** `spec_scene_unity_validation_missing_scripts_prefabs.md`
> **Objetivo:** criar validaÃ§Ã£o operacional de Unity para detectar compile errors, Missing Scripts, refs ausentes, assets quebrados, prefabs invÃ¡lidos e cenas inconsistentes.

---

## 1. Contexto

Algumas estabilizaÃ§Ãµes foram feitas via anÃ¡lise estÃ¡tica/GitHub. Isso corrige cÃ³digo e documentaÃ§Ã£o, mas nÃ£o substitui validaÃ§Ã£o local no Unity.

O risco principal Ã© ter `.asset`, `.prefab` ou `.unity` salvo com scripts removidos ou referÃªncias faltantes.

---

## 2. Gaps

- NÃ£o hÃ¡ pipeline automatizado local obrigatÃ³rio para Unity batchmode.
- NÃ£o hÃ¡ scanner de Missing Script em assets/prefabs/scenes.
- NÃ£o hÃ¡ relatÃ³rio padronizado de validaÃ§Ã£o Unity.
- NÃ£o hÃ¡ checklist de cenas obrigatÃ³rias.
- NÃ£o hÃ¡ validaÃ§Ã£o de ScriptableObjects com IDs vazios/duplicados fora dos registries.
- NÃ£o hÃ¡ validaÃ§Ã£o de referÃªncias crÃ­ticas de CaveScene/FarmScene/TownScene.

---

## 3. Escopo esperado

### Scripts de validaÃ§Ã£o

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

Validar extensÃµes:

```text
*.unity
*.prefab
*.asset
*.controller
```

Buscar Missing Script e referÃªncias aos modelos removidos:

```text
CindarsHope.Enemy.EnemyDataSO
CindarsHope.Crafting.CraftingRecipeSO
```

---

## 4. Cenas obrigatÃ³rias

Validar pelo menos:

```text
BootScene
FarmScene
TownScene
CaveScene
```

Para cada cena:

- abre sem erro;
- nÃ£o tem Missing Script;
- managers obrigatÃ³rios existem;
- installers tÃªm referÃªncias preenchidas;
- player/camera/spawn estÃ£o vÃ¡lidos;
- portais tÃªm target scene configurada.

---

## 5. Arquivos provÃ¡veis

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

- [ ] HÃ¡ comando Ãºnico para rodar compile validation.
- [ ] Log scanner destaca erros relevantes.
- [ ] Missing Script scanner roda via menu/editor/batchmode.
- [ ] Scene validator valida Boot/Farm/Town/Cave.
- [ ] DataIdValidator encontra IDs vazios/duplicados.
- [ ] RelatÃ³rio final Ã© salvo em `docs/implementation_runs/`.
- [ ] ValidaÃ§Ã£o falha com exit code != 0 se houver erro bloqueante.

---

## 7. ValidaÃ§Ã£o manual mÃ­nima

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
