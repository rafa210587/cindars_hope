---
name: system-reuse-audit
description: Auditoria pré-criação — antes de criar qualquer novo manager, service, SO type ou system, verifique que nada equivalente já existe. Use no Phase 0 de toda spec e sempre que estiver prestes a criar uma classe cujo nome/conceito pode já existir.
---

# Skill: System Reuse Audit

## Por que existe (incidentes reais neste repo)

- `Scripts/Craft/` (CraftingManager, RecipeDataSO, RecipeDatabaseSO — 12 arquivos) e `Scripts/Crafting/` (CraftingService, RecipeDefinition, RecipeType — 5 arquivos) são DOIS sistemas de crafting paralelos.
- `StatusEffectSO` existe duas vezes: `Combat/StatusEffectSO.cs` e `Combat/StatusEffect/StatusEffectSO.cs`.
- `SkillActionSO` existe duas vezes: `Combat/Skills/SkillActionSO.cs` e `Skills/SkillActionSO.cs`.

`spec_quality_gate` marca "criou sistema paralelo quando deveria reusar" como `NEEDS_REWORK`. O hook `runtime-code-guard` sinaliza nomes de classe duplicados no momento do Write — esta skill é a auditoria que você roda ANTES de escrever.

## Procedimento de auditoria (5 minutos, antes de criar qualquer coisa)

```powershell
# 1. Exact name and near-names
git grep -nE "class\s+(\w*)<CoreConcept>(\w*)" -- "Assets/_Game/Scripts/*.cs"

# 2. Concept synonyms (e.g., Craft|Crafting|Recipe|Workshop; Shop|Store|Vendor; Spawn|Materialize)
git grep -lE "<synonym1>|<synonym2>" -- "Assets/_Game/Scripts/*.cs"

# 3. Domain folder listing — does a folder for this domain already exist?
Get-ChildItem Assets\_Game\Scripts -Directory
```

## Matriz de decisão

| Achado | Ação |
|---|---|
| Mesmo conceito, sistema ativo | **Reuse/estenda.** Faça o wiring da sua spec nele. |
| Mesmo conceito, dois sistemas existentes (ex.: Craft vs Crafting) | **PARE — reporte ao humano.** Não escolha um silenciosamente e não adicione um terceiro. |
| Nome similar, conceito diferente | Renomeie o SEU novo tipo para remover a ambiguidade. |
| Nada encontrado | Crie, seguindo as convenções de domínio (`<Thing>DataSO`, service na pasta do domínio). |

## Evidência obrigatória no execution report

```text
## Existing Systems Audit
Searched: <patterns/synonyms used>
Found: <types/folders, with verdict reuse|extend|new|conflict>
Created new: <list + one-line justification each>
Conflicts reported to human: <none | list>
```

Uma reivindicação `BUILD_VALIDATED` sem esta seção é inválida (item 3 do checklist do spec_quality_gate).
