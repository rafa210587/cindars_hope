# SPEC — Decoração determinística por bioma da FarmScene

> **Spec ID:** `spec_farm_scene_biome_decoration_v1`  
> **Status:** A implementar  
> **Wave:** WAVE FARM KEYART — Densidade e acabamento  
> **Priority:** P2  
> **Type:** Tooling / Art / Integration  
> **Domain:** Farm  
> **Parallelizable:** NO  
> **Parallel group:** FARM_KEYART_VISUAL  
> **Can run with:** N/A  
> **Must not run with:** qualquer edição do gerador, terreno ou composição da FarmScene  
> **Repo lock scope:** `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs`, `Assets/_Game/Scripts/Editor/Art/**`, `Assets/_Game/Scripts/Editor/Validation/ValidateFarmSceneDecoration.cs`, `Assets/_Game/Art/Generated/World/**`  
> **Depends on:** `spec_farm_scene_landmarks_and_agriculture_v1`  
> **Blocks:** `spec_farm_scene_keyart_playmode_acceptance_v1`  
> **Scope:** adicionar densidade visual determinística com assets existentes e gerar apenas lacunas comprovadas.  
> **Out of scope:** modificar colliders, colocar objetos interativos novos, alterar loot/spawn/save ou esconder falhas de terreno com clutter.  
> **Validation level alvo:** UNITY_VALIDATED + captura visual  
> **Executor:** Claude | Codex

required_adrs: []
required_game_rules: []

# /speckit.specify

## Dependências

- **Depende de:** `spec_farm_scene_landmarks_and_agriculture_v1`.
- **Bloqueia:** aceitação humana desta wave.

## 5. Contexto

A keyart é rica em pequenos detalhes; a cena atual tem variações de tile, mas grandes vazios. Já há flower patch, mushroom cluster, stump, log, bushes, trees, rocks, fences, hay e animais. A decoração precisa ser reproduzível após regenerar, respeitar paths/áreas de interação e não gerar obstáculos invisíveis.

## 6. Problema

Scatter aleatório sem exclusão pode cobrir marcos, criar ruído no campo e mudar a cada regeneração. Gerar sete sheets antes de reutilizar o catálogo atual aumentaria inconsistência estilística.

## 7. Objetivo

Ao final, a FarmScene contém composição decorativa idempotente, seeded e por bioma, com densidade mensurável e zero decoração em células proibidas; arte nova só cobre uma lista de ausência validada.

## 8. Fontes obrigatórias lidas

`AGENTS.md`; `docs/project/CURRENT_STATE.md`; `spec_farm_scene_landmarks_and_agriculture_v1`; `.agents/skills/rng-and-determinism/SKILL.md`; `.agents/skills/object-pooling-pattern/SKILL.md`; `.agents/skills/pixel-art-prompt-authoring/SKILL.md`; `.agents/skills/chatgpt-web-sprite-gen/SKILL.md`; `CreateMvpFarmScene.cs`; `WorldSpriteLibrary.cs`; `FarmSceneSpatialContract.cs`; art catalog `Assets/_Game/Art/Generated/World/**`.

## 9. Estado atual do repo — Phase 0 (auditado em 2026-08-14)

```text
Existe: grass tile variation com flower/pebble weight, mas não uma camada de props por bioma.
Existe: foliage flower_patch, mushroom_cluster, tree_stump, log_fallen, bush_berry, bush_leafy.
Existe: props well, fence, hay_bale, rock_ore_0..5 e trees oak/apple/pine/willow.
Ausente: plano seeded de placement, máscara de exclusão e validator de densidade.
```

## 13. Regras de não duplicação

Não criar spawner runtime, manager de decoração, Rigidbody ou colliders para clutter. Decor é materializada pelo gerador editor sob um único root `FarmDecoration`; o seed é constante nomeada e todo aleatório usa o padrão da skill `rng-and-determinism`.

## 15. Arquitetura alvo — CRIAR vs MODIFICAR

# /speckit.plan

```text
CRIAR:
  Assets/_Game/Scripts/Editor/Art/FarmDecorationPlanner.cs — plano puro seeded e candidatos por bioma.
  Assets/_Game/Scripts/Editor/Validation/ValidateFarmSceneDecoration.cs — exclusões, IDs e contagem por bioma.
  Assets/_Game/Tests/EditMode/Editor/FarmDecorationPlannerTests.cs — determinismo e exclusões.
MODIFICAR:
  Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs — chama o planner uma vez após terreno/marcos.
```

## 16. Contratos, dados e eventos

```csharp
public enum FarmDecorationBiome { Meadow, Forest, WaterEdge, CliffBase, Homestead, AnimalPen }
public readonly struct FarmDecorationPlacement { public string SpriteId { get; } public Vector2 Position { get; } public FarmDecorationBiome Biome { get; } }
public static class FarmDecorationPlanner
{
    public const int Seed = 20260814;
    public static IReadOnlyList<FarmDecorationPlacement> Plan(FarmSceneSpatialContract spatial);
}
```

Eventos/save/UI: N/A. O plano é editor-only e não persiste referências Unity.

## 17. Sistemas afetados

World art / editor scene generation / sprite sorting / deterministic generation / validators / capture.

## 18. Arquivos permitidos

```text
Assets/_Game/Scripts/Editor/Art/FarmDecorationPlanner.cs
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs
Assets/_Game/Scripts/Editor/Validation/ValidateFarmSceneDecoration.cs
Assets/_Game/Tests/EditMode/Editor/**
Assets/_Game/Art/Generated/World/foliage/**
Assets/_Game/Art/Generated/World/props/**
docs/validation/**
```

## 19. Arquivos proibidos

`Assets/**/*.unity`, `Assets/**/*.prefab`, `Assets/**/*.asset`, runtime `Assets/_Game/Scripts/Farm/**` (exceto se estritamente necessário e aprovado), `Packages/**`, `ProjectSettings/**`, `docs_old/**`.

## 20. Estratégia de implementação

### Fase 0 — Catálogo e lacunas

Montar tabela `asset existente → bioma`. Só após a tabela aprovada, listar lacunas que justificam geração GPT: normalmente juncos/lírios/cascata e bordas, não flores/cogumelos/toco já existentes.

### Fase 1 — Planner puro

Gerar candidatos por hash `(Seed, biome, cellX, cellY)`; aceitar só cells dentro do bioma, fora de water/solid/path/crop/interactable approach. Pesos e densidade são constantes nomeadas por bioma, não magic values dispersos.

### Fase 2 — Materialização

Em `CreateMvpFarmScene`, após landmarks e antes da câmera, criar root único e SpriteRenderer child para cada placement. Sem collider; pivot/sorting seguem `WorldSpritePivotImportStep` e precedentes de Y-sort.

### Fase 3 — Arte faltante

Se a Fase 0 declarar lacuna, gerar em batch isolado via pipeline oficial, importar Point/None/no mipmap, auditar folha antes do wiring e registrar prompt/asset IDs no relatório.

### Fase 4 — Validator/captura

Validar determinismo em duas gerações, exclusões e mínimo/máximo por bioma. Capturar full map e closeups.

## Ordem de execucao

1. Auditoria de reutilização; 2. planner/testes; 3. materialização; 4. arte faltante, se houver; 5. validator; 6. capturas; 7. relatório.

## 14. Critérios de aceite

### 14.1 Determinismo

- Resultado: duas chamadas com mesmo seed produzem mesma sequência de sprite ID/posição/bioma.
- DoD: `FarmDecorationPlannerTests.SameSeed_ProducesIdenticalPlan` passa via EditMode.

### 14.2 Exclusões respeitadas

- Resultado: nenhuma decoração ocupa água, sólido, path, campo, trigger/interactable ou approach cell.
- DoD: `ValidateFarmSceneDecoration` imprime `Forbidden decoration placements: 0`.

### 14.3 Densidade por bioma

- Resultado: meadow, forest, water edge, cliff base, homestead e animal pen têm contagem dentro das faixas do contrato.
- DoD: validator imprime `Decoration biomes valid: 6/6`.

### 14.4 Sem arte duplicada desnecessária

- Resultado: relatório lista cada asset novo com a lacuna que ele cobre; se não houver lacuna, nenhum PNG é criado.
- DoD: seção `Art gap audit` do relatório contém `existing reuse` ou `generated with justification` para 100% dos assets usados.

## 23. Edge cases / falhas

- Seed alterado: requer mudança explícita da const e comparação de captura, nunca `Random` global.
- Prop sobre ponte/porta: máscaras de approach excluem a cell.
- Alto número de renderers: manter pequena densidade e sem Update/runtime allocation; revisar com performance audit se a contagem exceder a faixa acordada.
- PNG gerado com fundo: reprovar antes do import/wiring.
- Props em árvore/edifício: planner usa bounds de exclusão do contrato, não apenas posição do pivot.

## 22. Validação e gates

EditMode, validator, compile e capturas. Play Mode visual é verificado na spec final, pois esta não muda interações/física.

# /speckit.tasks

- [ ] Executar as fases 0–4 na ordem da §21 e registrar a evidência exigida.
