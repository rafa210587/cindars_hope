# SPEC — Contrato espacial canônico da FarmScene

> **Spec ID:** `spec_farm_scene_spatial_contract_v1`  
> **Status:** A implementar  
> **Wave:** WAVE FARM KEYART — Fundação espacial  
> **Priority:** P0  
> **Type:** Runtime / Tooling / Validation  
> **Domain:** Farm  
> **Parallelizable:** NO  
> **Parallel group:** FARM_KEYART_FOUNDATION  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere `CreateMvpFarmScene.cs`, o contrato de layout ou validators da FarmScene  
> **Repo lock scope:** `Assets/_Game/Scripts/Farm/FarmLevel1LayoutContract.cs`, `Assets/_Game/Scripts/Farm/Scene/**`, `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs`, `Assets/_Game/Scripts/Editor/Validation/ValidateFarmSceneSpatialContract.cs`  
> **Depends on:** baseline materializada de `spec_farm_scene_relayout_v4`; `docs/project/PLANO_FARM_KEYART_FIDELIDADE.md`  
> **Blocks:** `spec_farm_scene_collision_navigation_v1`, `spec_farm_scene_keyart_macro_composition_v1`, `spec_farm_scene_organic_terrain_water_v1`  
> **Scope:** estabelecer uma única fonte de verdade, sem referências Unity, para footprints de gameplay e âncoras da fazenda 64×44.  
> **Out of scope:** alteração de arte, geração de sprites, rebalanceamento, save schema e edição manual de YAML.  
> **Validation level alvo:** UNITY_VALIDATED + EditMode  
> **Executor:** Claude | Codex

required_adrs: []
required_game_rules: []

# /speckit.specify

## Dependências

- **Depende de:** baseline materializada de `spec_farm_scene_relayout_v4` e plano de fidelidade.
- **Bloqueia:** física, composição e terreno desta wave.

## 5. Contexto

A cena é gerada por código e já possui `FarmLevel1LayoutContract`, `FarmSceneZoneMarker`, `FarmNonArableZones` e colliders avulsos. A captura de jogo mostra zonas retangulares sobrepostas que não correspondem à geometria final esperada; a pintura de água e os bloqueios são produzidos por caminhos separados. Antes de redesenhar a fazenda, todas as três leituras — visual, colisão e solo arável — precisam derivar do mesmo footprint.

## 6. Problema

Hoje uma alteração em `CreateMvpFarmScene` pode reposicionar um visual, um trigger, o bloqueio físico e a exclusão de aragem em quatro blocos diferentes. Isso permite água visível atravessável, margem bloqueada, prédio arável ou trigger fora do marco correspondente.

## 7. Objetivo

Ao final, cada área de gameplay relevante terá ID estável, polígono/retângulo em coordenadas de mundo, tipo de ocupação e regra explícita de bloqueio/aragem; o gerador, bootstrap e validador consultarão o mesmo catálogo.

## 8. Fontes obrigatórias lidas

`AGENTS.md`; `docs/project/CURRENT_STATE.md`; `.agents/skills/spec-authoring/SKILL.md`; `.agents/skills/system-reuse-audit/SKILL.md`; `.agents/skills/tilemap-world-rendering/SKILL.md`; `.agents/skills/unity-validation/SKILL.md`; `Assets/_Game/Scripts/Farm/FarmLevel1LayoutContract.cs`; `Assets/_Game/Scripts/Farm/FarmTileGrid.cs`; `Assets/_Game/Scripts/Farm/Runtime/FarmSceneRuntimeBootstrap.cs`; `Assets/_Game/Scripts/Farm/Scene/FarmSceneZoneMarker.cs`; `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs`.

## 9. Estado atual do repo — Phase 0 (auditado em 2026-08-14)

```text
Existe: FarmLevel1LayoutContract.cs — bounds centrados 64×44 e âncoras v6.
Existe: FarmSceneZoneMarker.cs — enum de zonas + BoxCollider2D trigger; não é contrato de colisão sólida.
Existe: FarmTileGrid/FarmNonArableZones — domínio de aragem, separado da geometria visual.
Existe: CreateMvpFarmScene.CreateFarmSceneFoundationZones() — cria 10 zonas retangulares trigger.
Existe: CreateMvpFarmScene.CreateRiverSegment() — pinta água e cria BoxCollider2D por segmento.
Ausente: catálogo único que descreva uma footprint e seja consumido por visual, física e aragem.
Comando: rg -n "FarmSceneZoneMarker|FarmNonArableZones|CreateRiverSegment" Assets/_Game/Scripts --glob '*.cs'
Resultado: sistemas existentes acima; nenhum catálogo espacial canônico localizado.
```

A Fase 0 de execução deve repetir o comando e confirmar que não surgiu outro contrato espacial.

## 13. Regras de não duplicação

Não criar um segundo gerador de FarmScene, grid de farm, sistema de zonas ou registry de colisão. Estender `FarmLevel1LayoutContract`, `FarmSceneRuntimeBootstrap` e o gerador existente. `FarmSceneZoneMarker` continua sendo marcador/trigger de zona; não deve ser reinterpretado silenciosamente como barreira sólida.

## 15. Arquitetura alvo — CRIAR vs MODIFICAR

# /speckit.plan

```text
CRIAR:
  Assets/_Game/Scripts/Farm/Scene/FarmSceneSpatialContract.cs — DTOs puros e catálogo estático de footprints; pattern: (skill: registry-catalog-pattern).
  Assets/_Game/Scripts/Editor/Validation/ValidateFarmSceneSpatialContract.cs — compara objetos materializados com o catálogo.
  Assets/_Game/Tests/EditMode/Farm/FarmSceneSpatialContractTests.cs — testa IDs, bounds, sobreposição proibida e classificação.
MODIFICAR:
  Assets/_Game/Scripts/Farm/FarmLevel1LayoutContract.cs — expõe as âncoras existentes usadas pelo catálogo, sem duplicar literais.
  Assets/_Game/Scripts/Farm/Runtime/FarmSceneRuntimeBootstrap.cs — registra exclusões aráveis a partir do catálogo.
  Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs — substitui coordenadas duplicadas por consultas ao catálogo.
```

## 16. Contratos, dados e eventos

```csharp
public enum FarmSpatialUse { Walkable, Solid, Water, Building, CropField, TriggerOnly, Spawn }

public readonly struct FarmSceneFootprint
{
    public string Id { get; }
    public FarmSpatialUse Use { get; }
    public bool BlocksMovement { get; }
    public bool BlocksTilling { get; }
    public IReadOnlyList<Vector2> Polygon { get; }
}

public static class FarmSceneSpatialContract
{
    public const string Lake = "farm_spatial_lake";
    public const string River = "farm_spatial_river";
    public const string Mountain = "farm_spatial_mountain";
    public static IReadOnlyList<FarmSceneFootprint> All { get; }
    public static bool TryGet(string id, out FarmSceneFootprint footprint);
}
```

Eventos e save: N/A. O contrato só descreve constantes e tipos simples; não serializa referências Unity.

## 17. Sistemas afetados

Farm layout contract / geração de cena / física 2D / aragem por tile / zone triggers / validação editor / testes EditMode.

## 18. Arquivos permitidos

```text
Assets/_Game/Scripts/Farm/FarmLevel1LayoutContract.cs
Assets/_Game/Scripts/Farm/Scene/**
Assets/_Game/Scripts/Farm/Runtime/FarmSceneRuntimeBootstrap.cs
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs
Assets/_Game/Scripts/Editor/Validation/ValidateFarmSceneSpatialContract.cs
Assets/_Game/Tests/EditMode/Farm/**
docs/validation/**
```

## 19. Arquivos proibidos

`Assets/**/*.unity`, `Assets/**/*.prefab`, `Assets/**/*.asset`, `Packages/**`, `ProjectSettings/**`, `docs_old/**`; não editar YAML manualmente.

## 20. Estratégia de implementação

### Fase 0 — Reauditar

Reexecutar os comandos da §9; abrir a FarmScene gerada e inventariar cada collider sólido, trigger e zona não-arável antes de mudar código.

### Fase 1 — Contrato puro

Em `FarmSceneSpatialContract`, declarar somente footprints necessários nesta wave: montanha, lago, rio, ponte passável, casa, estufa, construções animais, crafting yard, cave mouth, town exit e campo. Definir pontos do polígono em sentido horário e validar IDs únicos.

```text
for each footprint:
  assert polygon has >= 3 points
  assert all points are inside FarmLevel1LayoutContract bounds
  assert Solid/Water/Building => BlocksTilling
  assert Water/Mountain/Building => BlocksMovement, except bridge and explicit entrances
```

### Fase 2 — Consumidores

Em `CreateMvpFarmScene`, trocar coordenadas duplicadas apenas para os footprints abrangidos. Em `FarmSceneRuntimeBootstrap`, converter footprints `BlocksTilling` em exclusões do sistema existente, sem alterar save de tiles.

### Fase 3 — Validação

O novo `ValidateFarmSceneSpatialContract` abre a cena, verifica um objeto/colisor correspondente por ID e reporta sobreposições proibidas: spawn em sólido/água, ponte bloqueada e trigger de saída bloqueado.

### Fase 4 — Evidência

Gerar captura por `FarmSceneCapture` e relatório `docs/validation/spec_farm_scene_spatial_contract_v1_execution_report.md` com o bloco `Existing Systems Audit` exigido pela skill.

## Ordem de execucao

1. Auditoria; 2. contratos e testes puros; 3. consumidor de aragem; 4. gerador; 5. validator; 6. regeneração via menu canônico; 7. captura e relatório.

## 14. Critérios de aceite

### 14.1 Catálogo único válido

- Resultado: `All` contém exatamente uma entrada para lago, rio, montanha, ponte, casa, estufa, construções, craft, caverna, saída e campo; nenhum ID se repete.
- DoD: `FarmSceneSpatialContractTests.AllFootprints_HaveUniqueIdsAndAreInBounds` passa via `tools/unity/RunUnityEditModeTests.ps1` com exit code `0`.

### 14.2 Sem área física órfã

- Resultado: cada collider sólido/trigger de gameplay enumerado pelo validator referencia ID do contrato; nenhum footprint `BlocksMovement` fica sem collider.
- DoD: menu `CindarsHope/Validar Layout Espacial FarmScene` imprime literalmente `ValidateFarmSceneSpatialContract: 0 error(s)`.

### 14.3 Aragem coerente

- Resultado: água, montanha e footprints de construção do catálogo são rejeitados pelo grid; a ponte e caminhos permanecem navegáveis.
- DoD: `FarmSceneSpatialContractTests.BlockingFootprints_AreNonArable` passa com 0 falhas.

### 14.4 Compilação e evidência

- Resultado: assemblies compilam e existe relatório com inventário before/after.
- DoD: `tools/unity/RunUnityCompileValidation.ps1` termina em exit code `0`; relatório contém `Existing Systems Audit`.

## 23. Edge cases / falhas

- Polígono auto-intersectante: validator falha com o ID antes de salvar a cena.
- Ponte sobre água: pertence à água visual, mas declara `Walkable`; o teste exige ausência de collider sólido no vão.
- Trigger sobre collider sólido: permitido somente para cave/town entrance, declarado explicitamente no catálogo.
- Regeneração destrutiva: nenhuma `.unity` é editada manualmente; a evidência registra o comando de regeneração.
- Mudança de bounds: testes falham se qualquer ponto sair de `FarmLevel1LayoutContract`.

## 22. Validação e gates

Rodar EditMode, validator e compile. Play Mode é `DEFERRED_TO_FINAL_VALIDATION` nesta spec; a travessia física humana é validada pela spec final do lote.

# /speckit.tasks

- [ ] Executar as fases 0–4 na ordem da §21 e registrar a evidência exigida.
