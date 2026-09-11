# SPEC — Física e navegação coerentes da FarmScene

> **Spec ID:** `spec_farm_scene_collision_navigation_v1`  
> **Status:** A implementar  
> **Wave:** WAVE FARM KEYART — Física espacial  
> **Priority:** P0  
> **Type:** Runtime / Integration / Validation  
> **Domain:** Farm  
> **Parallelizable:** NO  
> **Parallel group:** FARM_KEYART_FOUNDATION  
> **Can run with:** N/A  
> **Must not run with:** qualquer edição da FarmScene ou de `FarmSceneSpatialContract`  
> **Repo lock scope:** `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs`, `Assets/_Game/Scripts/Farm/Scene/**`, `Assets/_Game/Scripts/Editor/Validation/ValidateFarmSceneSpatialContract.cs`  
> **Depends on:** `spec_farm_scene_spatial_contract_v1`  
> **Blocks:** `spec_farm_scene_keyart_macro_composition_v1`, `spec_farm_scene_organic_terrain_water_v1`, validação humana final  
> **Scope:** materializar física 2D e passagens a partir do contrato espacial, sem alterar controladores de movimento.  
> **Out of scope:** pathfinding de NPC, combate, novos sistemas de interação, arte final e save.  
> **Validation level alvo:** UNITY_VALIDATED + PLAYMODE  
> **Executor:** Claude | Codex

required_adrs: []
required_game_rules: []

# /speckit.specify

## Dependências

- **Depende de:** `spec_farm_scene_spatial_contract_v1`.
- **Bloqueia:** composição, terreno e aceitação humana desta wave.

## 5. Contexto

O screenshot em jogo expõe que zonas de fundação são overlays/trigger retangulares, enquanto a movimentação precisa seguir a geografia final. A fazenda já usa `BoxCollider2D` para bounds, prédios, água e recursos; não há autorização para alterar o player ou criar um motor de navegação paralelo.

## 6. Problema

Água é montada por segmentos, alguns sem collider para a ponte; lago, margens, prédios e áreas de interação podem ficar desalinhados. Isso cria bloqueio invisível, água atravessável e interação inacessível.

## 7. Objetivo

Ao final, o jogador bloqueia apenas em geometria sólida/água, atravessa os vãos declarados, alcança cada interactable obrigatório pelo lado caminhável e nunca nasce em área bloqueada.

## 8. Fontes obrigatórias lidas

`AGENTS.md`; `docs/project/CURRENT_STATE.md`; `spec_farm_scene_spatial_contract_v1`; `.agents/skills/spec-authoring/SKILL.md`; `.agents/skills/unity-validation/SKILL.md`; `.agents/skills/gameplay-test-scenario/SKILL.md`; `CreateMvpFarmScene.cs`; `FarmSceneSpatialContract.cs`; `FarmSceneZoneMarker.cs`; `PlayerController.cs`; `FarmSceneCapture.cs`.

## 9. Estado atual do repo — Phase 0 (auditado em 2026-08-14)

```text
Existe: CreateBounds() cria quatro BoxCollider2D em [-32,32]×[-22,22].
Existe: CreateRiverSegment() cria BoxCollider2D retangular por segmento, exceto `withCollider:false` sob a ponte.
Existe: CreateLakeBody() pinta tiles sem collider próprio.
Existe: FitBoxColliderToOpaqueSprite() para árvores e FitLakeBlockingCollider() para FishingSpot legado.
Existe: Player com Rigidbody2D/BoxCollider2D materializado pelo gerador.
Ausente: validação que prove que todo limite sólido coincide com um footprint e que cada marco é alcançável.
```

A Fase 0 repete esse inventário e registra os layers de física reais antes de editar.

## 13. Regras de não duplicação

Reusar a física `Collider2D` e `PlayerController` existente. Não criar navmesh, pathfinding ou global search. Não usar os triggers de `FarmSceneZoneMarker` como bloqueio. Reusar o `FarmSceneSpatialContract` da dependência.

## 15. Arquitetura alvo — CRIAR vs MODIFICAR

# /speckit.plan

```text
CRIAR:
  Assets/_Game/Scripts/Editor/Validation/ValidateFarmSceneNavigation.cs — validator de configuração e alcançabilidade discreta.
  Assets/_Game/Tests/EditMode/Farm/FarmSceneNavigationContractTests.cs — testes de regras puras do contrato.
MODIFICAR:
  Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs — cria colliders por footprint, com GameObjects nomeados `Collision_<footprintId>`.
  Assets/_Game/Scripts/Editor/Dev/FarmSceneCapture.cs — acrescenta captura opcional de debug de colliders, sem regenerar implicitamente.
```

## 16. Contratos, dados e eventos

```csharp
public static class FarmSceneNavigationPolicy
{
    public static bool IsExpectedWalkable(FarmSpatialUse use);
    public static bool RequiresSolidCollider(FarmSceneFootprint footprint);
    public static bool RequiresReachableApproach(FarmSceneFootprint footprint);
}
```

Eventos, save e UI: N/A. A política é pura e a cena é gerada pelo caminho editor existente.

## 17. Sistemas afetados

Farm generator / Physics2D / Player movement / interactables / farm tilling exclusions / editor validation / Play Mode scenario.

## 18. Arquivos permitidos

```text
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs
Assets/_Game/Scripts/Editor/Dev/FarmSceneCapture.cs
Assets/_Game/Scripts/Editor/Validation/ValidateFarmSceneNavigation.cs
Assets/_Game/Scripts/Farm/Scene/**
Assets/_Game/Tests/EditMode/Farm/**
docs/validation/**
```

## 19. Arquivos proibidos

`Assets/**/*.unity`, `Assets/**/*.prefab`, `Assets/**/*.asset`, `Assets/_Game/Scripts/Player/**`, `Packages/**`, `ProjectSettings/**`, `docs_old/**`.

## 20. Estratégia de implementação

### Fase 0 — Matriz de colisão

Para cada ID espacial, classificar: sólido, trigger, passável, arável e ponto de aproximação. Fotografar antes/depois com a opção de debug; não inferir sucesso pelo gizmo colorido.

### Fase 1 — Builder de collider

Em `CreateMvpFarmScene.CreateScene()`, após materializar os visuais de terreno e antes de criar a câmera, chamar helper que materializa colliders nomeados pelo ID do contrato. Usar `PolygonCollider2D` para lake/river/montanha irregulares; `BoxCollider2D` apenas para footprint retangular. Pontes e portas mantêm corredor sem collider sólido.

### Fase 2 — Acessibilidade

O validator rasteriza em célula de 1 unidade somente footprints sólidos e executa flood-fill a partir de `DefaultSpawn`. Deve exigir ao menos uma célula caminhável adjacente a: casa/porta, fonte, craft, shipping, sell point, cave, town exit, fishing spot e cada construção animal.

```text
queue <- default spawn cell
while queue not empty: visit 4-neighbors inside bounds and outside solid polygons
for each required landmark: fail if no approach cell was visited
```

### Fase 3 — Testes, Play Mode e relatório

Adicionar testes de política e produzir cenário humano com rota: spawn → casa → ponte → lago → craft → animais → fonte → cave → town exit. Reportar bloqueios reais, não apenas desenho da cena.

## Ordem de execucao

1. Reauditar; 2. política/testes; 3. builder; 4. validator; 5. regenerar pelo menu; 6. executar cenário humano; 7. relatório.

## 14. Critérios de aceite

### 14.1 Colisores têm dono

- Resultado: todo collider sólido de cenário é nomeado `Collision_<id>` e resolve em `FarmSceneSpatialContract`; nenhum trigger de zona é usado como sólido.
- DoD: `CindarsHope/Validar Navegação FarmScene` imprime `ValidateFarmSceneNavigation: 0 orphan collider(s)`.

### 14.2 Água e montanha bloqueiam; ponte passa

- Resultado: lake, river e mountain têm barreira física conforme o contrato e a ponte possui passagem contínua.
- DoD: `FarmSceneNavigationContractTests.WaterAndMountain_BlockMovement_BridgeDoesNot` passa; cenário humano marca 3 bloqueios e 1 travessia como PASS.

### 14.3 Marcos alcançáveis

- Resultado: os 10 marcos obrigatórios têm approach cell alcançável do spawn padrão.
- DoD: validator imprime `Reachable required landmarks: 10/10`.

### 14.4 Sem regressão de inputs

- Resultado: nenhum arquivo de Player/input foi alterado.
- DoD: `git diff --name-only` não lista `Assets/_Game/Scripts/Player/` e `RunUnityCompileValidation.ps1` retorna exit `0`.

## 23. Edge cases / falhas

- Pontes sobrepostas a polígono d'água: subtrair somente o corredor declarado, nunca desabilitar a colisão do rio inteiro.
- Sprite com área transparente: collider vem do contrato, não de bounds de sprite decorativo.
- Interactable dentro de sólido: validator falha com ID e coordenada.
- Spawn alterado por transição: validar também `spawn_farm_from_town` e `spawn_farm_from_cave` como células caminháveis.
- Custo do flood-fill: grid fixo 64×44, sem LINQ em runtime; validator é editor-only.

## 22. Validação e gates

EditMode, validator, compile e cenário Play Mode obrigatório. Se Unity não puder rodar, declarar literalmente `Unity validation: NOT RUN`, motivo, comando e risco residual conforme `AGENTS.md`.

# /speckit.tasks

- [ ] Executar as fases 0–3 na ordem da §21 e registrar a evidência exigida.
