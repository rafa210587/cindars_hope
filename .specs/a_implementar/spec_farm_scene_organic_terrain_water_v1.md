# SPEC — Terreno orgânico, água e margens da FarmScene

> **Spec ID:** `spec_farm_scene_organic_terrain_water_v1`  
> **Status:** A implementar  
> **Wave:** WAVE FARM KEYART — Terreno  
> **Priority:** P1  
> **Type:** Tooling / Art / Integration  
> **Domain:** Farm  
> **Parallelizable:** NO  
> **Parallel group:** FARM_KEYART_VISUAL  
> **Can run with:** N/A  
> **Must not run with:** qualquer edição de `WorldTilemapGround.cs`, do gerador da FarmScene ou contratos espaciais  
> **Repo lock scope:** `Assets/_Game/Scripts/Editor/Art/WorldTilemapGround.cs`, `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs`, `Assets/_Game/Art/Generated/World/tiles/**`, `Assets/_Game/Scripts/Editor/Validation/ValidateFarmTerrainMasks.cs`  
> **Depends on:** `spec_farm_scene_keyart_macro_composition_v1`  
> **Blocks:** `spec_farm_scene_landmarks_and_agriculture_v1`, `spec_farm_scene_biome_decoration_v1`  
> **Scope:** substituir retângulos de água/caminho por máscaras orgânicas e transições consistentes com física.  
> **Out of scope:** gerar prédios, floresta densa, novos sistemas de tile runtime ou editar asset/YAML à mão.  
> **Validation level alvo:** UNITY_VALIDATED + captura visual  
> **Executor:** Claude | Codex

required_adrs: []
required_game_rules: []

# /speckit.specify

## Dependências

- **Depende de:** `spec_farm_scene_keyart_macro_composition_v1`.
- **Bloqueia:** marcos, decoração e aceitação humana desta wave.

## 5. Contexto

`WorldTilemapGround.PaintWater`, `PaintTile` e `PaintShoreRing` pintam retângulos. O lago atual é composto por quatro retângulos e o rio por segmentos, produzindo degraus e margem de areia uniforme. A keyart pede bordas irregulares grama–terra e água–pedra, mantendo passagem clara na ponte.

## 6. Problema

A geometria visual não é uma máscara: caminhos têm quinas, lago é escalonado e qualquer ajuste de collider pode divergir do tilemap. Scatter de decoração não deve mascarar esta falha estrutural.

## 7. Objetivo

Ao final, terreno, água, margem e collider usam os polígonos do contrato espacial; paths e campo têm contorno orgânico; tiles de transição são gerados/importados pelo fluxo canônico e não por YAML manual.

## 8. Fontes obrigatórias lidas

`AGENTS.md`; `docs/project/CURRENT_STATE.md`; `spec_farm_scene_spatial_contract_v1`; `spec_farm_scene_keyart_macro_composition_v1`; `.agents/skills/tilemap-world-rendering/SKILL.md`; `.agents/skills/unity-asset-generation/SKILL.md`; `.agents/skills/chatgpt-web-sprite-gen/SKILL.md`; `WorldTilemapGround.cs`; `CreateMvpFarmScene.cs`; `WorldSpriteLibrary.cs`; imports de `Assets/_Game/Art/Generated/World/tiles/**`.

## 9. Estado atual do repo — Phase 0 (auditado em 2026-08-14)

```text
Existe: WorldTilemapGround.PaintGrass() com pesos 60/15/15/6/4 (base/variações/flor/pedra).
Existe: PaintWater(), PaintTile() e PaintShoreRing() retangulares.
Existe: ground_grass, ground_path_dirt, ground_soil, ground_water, ground_sand_shore e cliff tiles.
Ausente: tiles de borda grama-terra e água-pedra; API que pinta a partir de polígono/máscara.
Existe: todos os imports verificados usam PPU 128, Filter Mode Point e Compression None.
```

## 13. Regras de não duplicação

Estender `WorldTilemapGround`; não criar segundo tile painter ou RuleTile framework. Reusar `WorldSpriteLibrary` e o pipeline de assets; não editar `.asset`, `.meta`, `.unity` ou `.prefab` manualmente. Antes de gerar arte, auditar o catálogo de tiles e registrar somente itens realmente ausentes.

## 15. Arquitetura alvo — CRIAR vs MODIFICAR

# /speckit.plan

```text
CRIAR:
  Assets/_Game/Scripts/Editor/Validation/ValidateFarmTerrainMasks.cs — valida cells pintadas contra footprints.
  Assets/_Game/Tests/EditMode/Editor/FarmTerrainMaskTests.cs — testes de rasterização de polígono e borda.
MODIFICAR:
  Assets/_Game/Scripts/Editor/Art/WorldTilemapGround.cs — PaintPolygon e PaintTransitionRing determinísticos.
  Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs — usa máscaras do contrato, não retângulos sobrepostos.
GERADO:
  Assets/_Game/Art/Generated/World/tiles/ground_dirt_edge_*.png e ground_water_rock_edge_*.png — somente se o audit confirmar ausência, via pipeline de geração/importação.
```

## 16. Contratos, dados e eventos

```csharp
public static void PaintPolygon(
    Transform parent, string gridName, string layerName, int sortingOrder,
    string tileName, IReadOnlyList<Vector2> polygon);

public static void PaintTransitionRing(
    Transform parent, string gridName, string layerName, int sortingOrder,
    IReadOnlyList<Vector2> polygon, TerrainTransitionKind kind);

public enum TerrainTransitionKind { GrassToDirt, WaterToRock }
```

Save/event/UI: N/A. A rasterização é editor-only e determinística.

## 17. Sistemas afetados

Tilemap editor / art generation and import / Farm generator / collision contract / validators / screenshots.

## 18. Arquivos permitidos

```text
Assets/_Game/Scripts/Editor/Art/WorldTilemapGround.cs
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs
Assets/_Game/Scripts/Editor/Validation/ValidateFarmTerrainMasks.cs
Assets/_Game/Tests/EditMode/Editor/**
Assets/_Game/Art/Generated/World/tiles/**
docs/validation/**
```

## 19. Arquivos proibidos

`Assets/**/*.unity`, `Assets/**/*.prefab`, `Assets/**/*.asset`, `Assets/_Game/Art/**/*.meta` (exceto alteração feita pelo importador Unity), `Packages/**`, `ProjectSettings/**`, `docs_old/**`.

## 20. Estratégia de implementação

### Fase 0 — Arte e máscara

Inventariar tiles já disponíveis; anexar prompt/grade do kit faltante ao relatório. Definir os polígonos de lake, river, paths e campo apenas no `FarmSceneSpatialContract`/contrato de composição.

### Fase 1 — Rasterização

Implementar point-in-polygon sobre células do Grid e pintar deterministamente. O anel de transição é calculado por vizinhança de 8 células: centro, lados, cantos externos e internos; ausência de tile específico deve falhar o validator, nunca cair em areia genérica silenciosamente.

### Fase 2 — Wiring

Trocar `CreateLakeBody` e `CreateRiverSegment` por chamadas de máscara. Paths e campo usam o mesmo mecanismo, com largura e curvas declaradas no contrato de composição. O collider continua vindo da spec de física, usando o mesmo polígono.

### Fase 3 — Validação

O validator checa: nenhuma tile water fora de Lake/River; borda rock circunda água; bridge corridor não recebe água/margem bloqueante; path não invade building/water; ground base cobre os 64×44 tiles.

## Ordem de execucao

1. Auditoria de tiles; 2. gerar/importar apenas faltantes; 3. testes de máscara; 4. painter; 5. gerador; 6. validator; 7. captura e relatório.

## 14. Critérios de aceite

### 14.1 Sem água retangular acidental

- Resultado: cada tile de água pertence ao polígono de lake ou river e os quatro retângulos antigos não são mais usados.
- DoD: `ValidateFarmTerrainMasks` imprime `Water cells outside contract: 0` e `Legacy rectangle painters referenced: 0`.

### 14.2 Margem correta

- Resultado: toda célula de água exposta a terreno possui transição `WaterToRock`; areia não é o fallback de margem do lago.
- DoD: validator imprime `Unringed exposed water cells: 0`.

### 14.3 Caminhos e campo orgânicos

- Resultado: path/crop field são pintados por máscara e possuem ao menos um canto/curva não retangular na captura.
- DoD: `FarmTerrainMaskTests.PaintPolygon_ConcaveMask_DoesNotFillBoundingBox` passa e a captura full-map é salva.

### 14.4 Importação pixel art válida

- Resultado: todo novo PNG de tile tem Point, Compression None e Mip Maps false.
- DoD: validador de importação do projeto retorna `0 error(s)` para os paths novos.

## 23. Edge cases / falhas

- Polígono côncavo: rasterizador não pode preencher o bounding box.
- Tile de borda ausente: interromper o passo de geração com erro contextual.
- Ponte: anel visual pode existir sob ela, mas corridor caminhável não pode receber collider sólido.
- Regeneração repetida: tilemaps são recriados idempotentemente pelo gerador, sem acúmulo de cells.
- Mistura de tamanho de célula: usar o cell size derivado da tile base e validar que é 1 unidade.

## 22. Validação e gates

EditMode + validators + compile + captura. Play Mode de travessia é reexecutado pela spec final; não alegar aprovação humana nesta etapa.

# /speckit.tasks

- [ ] Executar as fases 0–3 na ordem da §21 e registrar a evidência exigida.
