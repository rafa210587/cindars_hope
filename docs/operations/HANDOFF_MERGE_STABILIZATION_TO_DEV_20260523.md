# HANDOFF — Merge da estabilização para dev — 2026-05-23

> **Branch fonte:** `review/stabilize-overnight-specs`  
> **Branch destino:** `dev`  
> **Objetivo:** validar e mergear a estabilização documental/código sem perder refinamentos futuros.  
> **Importante:** não implementar novas features durante este merge.

---

## 1. O que esta branch contém

Esta branch consolida a estabilização pós-execução overnight:

- Corrige `ItemCategory` para preservar enum serialization antiga.
- Corrige regra de SkillPoint para 1 ponto a cada 2 níveis.
- Centraliza regra em `PlayerProgressionRules`.
- Alinha `PlayerProgressionManager` e `LevelUpManager`.
- Consolida enemy data no modelo oficial `CindarsHope.Combat.EnemyDataSO`.
- Remove modelo paralelo `Assets/_Game/Scripts/Enemy/EnemyDataSO.cs`.
- Consolida crafting recipe no modelo oficial `CindarsHope.Craft.Data.RecipeDataSO`.
- Remove modelo paralelo `Assets/_Game/Scripts/Crafting/CraftingRecipeSO.cs`.
- Rebaixa delivery report de `COMPLETE` para `PARTIAL`.
- Registra run de estabilização.
- Adiciona refinamentos init futuros para tudo que ainda falta completar.

---

## 2. Prompt para Claude Code ou Codex

```md
Estamos no repo `rafa210587/cindars_hope`.

Objetivo:
validar e mergear a branch `review/stabilize-overnight-specs` em `dev` com segurança.

Não implemente novas features.
Não altere escopo de gameplay.
Não faça refactors oportunistas.
A tarefa é exclusivamente validação, correção de conflito se houver, e merge da estabilização.

Passos obrigatórios:

1. Garanta que está no repositório correto:

```powershell
git remote -v
git status
```

2. Atualize referências remotas:

```powershell
git fetch origin --prune
```

3. Garanta que `dev` está atualizada:

```powershell
git checkout dev
git pull origin dev
```

4. Garanta que a branch fonte existe e está atualizada:

```powershell
git checkout review/stabilize-overnight-specs
git pull origin review/stabilize-overnight-specs
```

5. Revise o diff antes do merge:

```powershell
git diff --stat dev..review/stabilize-overnight-specs
git diff --name-status dev..review/stabilize-overnight-specs
```

6. Verifique pontos críticos no diff:

- `Assets/_Game/Scripts/Inventory/Data/ItemCategory.cs`
- `Assets/_Game/Scripts/Player/Progression/PlayerProgressionRules.cs`
- `Assets/_Game/Scripts/Player/Progression/PlayerProgressionManager.cs`
- `Assets/_Game/Scripts/Core/Progression/LevelUpManager.cs`
- `Assets/_Game/Scripts/Combat/EnemyDataSO.cs`
- `Assets/_Game/Scripts/Editor/EnemyDataInitializer.cs`
- `Assets/_Game/Scripts/Craft/Data/RecipeDataSO.cs`
- `Assets/_Game/Scripts/Editor/CraftingRecipeInitializer.cs`
- remoção de `Assets/_Game/Scripts/Enemy/EnemyDataSO.cs`
- remoção de `Assets/_Game/Scripts/Crafting/CraftingRecipeSO.cs`
- `docs/refinements/a_implementar/refinamento_init_*.md`
- `docs/implementation_runs/RUN_STABILIZATION_OVERNIGHT_20260523.md`

7. Antes do merge, rode validação textual/local:

```powershell
Select-String -Path "Assets\_Game\Scripts\**\*.cs" -Pattern "GameObject.Find|FindObjectOfType|FindObjectsByType"
Select-String -Path "Assets\**\*.asset","Assets\**\*.prefab","Assets\**\*.unity" -Pattern "CindarsHope.Enemy.EnemyDataSO|CindarsHope.Crafting.CraftingRecipeSO|Missing Script"
```

8. Rode validação documental se o script existir:

```powershell
if (Test-Path ".\tools\docs\validate_docs.ps1") { .\tools\docs\validate_docs.ps1 }
```

9. Rode Unity batchmode:

```powershell
New-Item -ItemType Directory -Force -Path "Logs" | Out-Null
& "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" `
  -batchmode `
  -quit `
  -projectPath "D:\Projetos\Jogos\Cindars_hope\cindars_hope" `
  -logFile "Logs\unity-compile-stabilization-merge.log"

Select-String -Path "Logs\unity-compile-stabilization-merge.log" -Pattern `
  "error CS|Compilation failed|Scripts have compiler errors|Exception|NullReferenceException|MissingReferenceException|Missing script|The referenced script on this Behaviour is missing|not found|Failed|AssetDatabase|ScriptableObject"
```

10. Se houver erro de compile ou Missing Script:

- Não faça merge.
- Leia o erro.
- Planeje correção mínima.
- Corrija na branch `review/stabilize-overnight-specs`.
- Rode Unity batchmode novamente.

11. Se tudo passar, faça merge em dev:

```powershell
git checkout dev
git merge --no-ff review/stabilize-overnight-specs -m "merge: estabilizar specs overnight e refinamentos futuros"
```

12. Rode validação novamente em `dev` após merge:

```powershell
git status
if (Test-Path ".\tools\docs\validate_docs.ps1") { .\tools\docs\validate_docs.ps1 }

& "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" `
  -batchmode `
  -quit `
  -projectPath "D:\Projetos\Jogos\Cindars_hope\cindars_hope" `
  -logFile "Logs\unity-compile-dev-after-stabilization.log"

Select-String -Path "Logs\unity-compile-dev-after-stabilization.log" -Pattern `
  "error CS|Compilation failed|Scripts have compiler errors|Exception|NullReferenceException|MissingReferenceException|Missing script|The referenced script on this Behaviour is missing|not found|Failed|AssetDatabase|ScriptableObject"
```

13. Se dev estiver ok, faça push:

```powershell
git push origin dev
```

14. Gere/atualize um relatório final em:

```text
docs/implementation_runs/RUN_MERGE_STABILIZATION_TO_DEV_20260523.md
```

O relatório deve conter:

- commit/merge sha;
- validações rodadas;
- resultado do Unity batchmode;
- se houve Missing Script;
- arquivos principais alterados;
- pendências futuras preservadas em `refinamento_init_*.md`.
```

---

## 3. Checklist manual pós-merge no Unity

Abrir o Unity normalmente e validar:

### 3.1 Console

- [ ] Console sem erro vermelho.
- [ ] Sem `Missing Script`.
- [ ] Sem `NullReferenceException` novo.
- [ ] Sem warning bloqueante de registry crítico ausente.

### 3.2 Boot/Farm

- [ ] BootScene abre.
- [ ] FarmScene carrega.
- [ ] Player aparece.
- [ ] Player move.
- [ ] HUD/debug aparece se esperado.
- [ ] Inventory inicial carrega.
- [ ] Plantar/colher ainda funciona se já funcionava antes.

### 3.3 Save/load

- [ ] Salvar cria/atualiza `slot_1.json` em `Application.persistentDataPath`.
- [ ] Fechar e reabrir restaura dia, inventory, gold, fome e farm state.
- [ ] Não há exceção de schema version.

### 3.4 Progression

- [ ] Ganhar XP até nível 2 concede 1 SkillPoint.
- [ ] Nível 3 não concede SkillPoint.
- [ ] Nível 4 concede +1 SkillPoint.
- [ ] AttributePoint continua sendo concedido por level up.

### 3.5 Cave

- [ ] Entrar na CaveScene não gera warning de registry ausente.
- [ ] Player spawna em posição segura.
- [ ] Cave materializa tiles/walls/portal.
- [ ] Voltar/sair não quebra save.

### 3.6 Crafting/enemy assets

- [ ] Assets novos de enemy usam `CindarsHope.Combat.EnemyDataSO`.
- [ ] Assets novos de recipe usam `CindarsHope.Craft.Data.RecipeDataSO`.
- [ ] Não há assets/prefabs com scripts removidos.

---

## 4. Decisão de merge

Merge permitido apenas se:

- [ ] Unity batchmode sem erro.
- [ ] Sem Missing Script.
- [ ] Sem referências aos modelos removidos em assets/prefabs/scenes.
- [ ] `dev` passa validação após merge.
- [ ] Refinamentos init estão presentes e mapeados.

Se qualquer item falhar, manter em `review/stabilize-overnight-specs` e corrigir antes de merge.
