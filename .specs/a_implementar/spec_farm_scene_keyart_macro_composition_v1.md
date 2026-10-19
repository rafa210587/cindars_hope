# SPEC — Rebase de escala e macrocomposição da FarmScene pela keyart

> **Spec ID:** `spec_farm_scene_keyart_macro_composition_v1`  
> **Status:** A implementar  
> **Wave:** WAVE FARM KEYART — Composição  
> **Priority:** P1  
> **Type:** Tooling / Integration / Art  
> **Domain:** Farm  
> **Parallelizable:** NO  
> **Parallel group:** FARM_KEYART_VISUAL  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere o gerador da FarmScene ou perfis de escala  
> **Repo lock scope:** `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs`, `Assets/_Game/Scripts/World/Scale/**`, `Assets/_Game/Scripts/Editor/Dev/FarmSceneCapture.cs`, `Assets/_Game/Scripts/Editor/Validation/ValidateFarmSceneComposition.cs`  
> **Depends on:** `spec_farm_scene_collision_navigation_v1`  
> **Blocks:** `spec_farm_scene_organic_terrain_water_v1`, `spec_farm_scene_landmarks_and_agriculture_v1`  
> **Scope:** alinhar escala, massas principais e corredores da fazenda à keyart aprovada sem mudar sistemas de gameplay.  
> **Out of scope:** novos sprites, decoração densa, mudanças em Town/Cave, balance e UI.  
> **Validation level alvo:** UNITY_VALIDATED + captura visual  
> **Executor:** Claude | Codex

required_adrs: []
required_game_rules: []

# /speckit.specify

## Dependências

- **Depende de:** `spec_farm_scene_collision_navigation_v1`.
- **Bloqueia:** terreno, marcos e decoração desta wave.

## 5. Contexto

A captura geral revela grande desproporção: casa/ponte ocupam massa visual excessiva, árvores parecem ícones pequenos e o campo cultivável não organiza o centro. A keyart é top-down com leitura de massas: montanha norte, bosque oeste, cultivo central, homestead nordeste, construções ao sul e lago sudeste. O atual gerador já contém esses elementos, mas em escala e distribuição divergentes.

## 6. Problema

Adicionar flores ou props sobre a composição atual não resolverá a diferença de estilo: as silhuetas principais continuam fora de escala e os vazios/corredores não conduzem o olhar como a referência.

## 7. Objetivo

Ao final, a captura full-map tem as seis massas da keyart em posições e proporções verificáveis; cada elemento funcional permanece dentro do footprint físico da dependência.

## 8. Fontes obrigatórias lidas

`AGENTS.md`; `docs/project/CURRENT_STATE.md`; `docs/project/PLANO_FARM_KEYART_FIDELIDADE.md`; `spec_farm_scene_collision_navigation_v1`; `.agents/skills/spec-authoring/SKILL.md`; `.agents/skills/tilemap-world-rendering/SKILL.md`; `.agents/skills/unity-asset-generation/SKILL.md`; `CreateMvpFarmScene.cs`; `VisualScaleProfileSO.cs`; `FarmSceneCapture.cs`; referência `docs/art_catalog/_reference_keyart_GPT/farm_keyart_layout_aprovado_v1.png`.

## 9. Estado atual do repo — Phase 0 (auditado em 2026-08-14)

```text
Capturas existentes: docs/validation/playmode/farm_capture.png, _closeup.png e _animals.png (14/08/2026).
Existe: PPU 128 em farmhouse, bridge, grass, trees e well; a incompatibilidade vem de dimensões de sprites e escalas locais, não de PPU misto.
Existe: bridge localScale = (4.6, 2); edifícios e props têm escalas locais próprias no gerador.
Existe: CreateMountainBarrier(), CreateRiverAndBridge(), CreateFarmPathNetworks(), CreateAnimalHousings() e CreateFarmWalkInHouse().
Ausente: validação de composição, matriz de escala aprovada e captura de referência anotada.
```

## 13. Regras de não duplicação

Reusar `WorldSpriteLibrary`, `ScaleProfileLibrary` e o único `CreateMvpFarmScene`. Não modificar importadores `.meta` nem criar escala ad-hoc por sprite sem registrar no perfil/catálogo existente. Não alterar colliders fora do contrato espacial.

## 15. Arquitetura alvo — CRIAR vs MODIFICAR

# /speckit.plan

```text
CRIAR:
  Assets/_Game/Scripts/Editor/Validation/ValidateFarmSceneComposition.cs — verifica âncoras, grupos e limites de escala definidos na spec.
  docs/validation/farm_keyart_composition_baseline.md — tabela de referência e capturas anotadas.
MODIFICAR:
  Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs — aplica posições/escala por helper de marco, sem escalas mágicas dispersas.
  Assets/_Game/Scripts/World/Scale/VisualScaleProfileSO.cs e gerador de perfil — somente se o audit provar que categoria existente não expressa a escala.
```

## 16. Contratos, dados e eventos

```csharp
public enum FarmCompositionGroup { NorthCliff, WestForest, CentralAgriculture, NorthEastHomestead, SouthAnimalRow, SouthEastLake }

public static class FarmSceneCompositionContract
{
    public static Bounds GetTargetBounds(FarmCompositionGroup group);
    public static bool IsScaleWithinApprovedRange(string sceneObjectName, Vector3 localScale);
}
```

Eventos e save: N/A. A composição apenas consome IDs/âncoras existentes.

## 17. Sistemas afetados

Gerador de FarmScene / perfis visuais / tilemap / colliders derivados do contrato / captura e validação editor.

## 18. Arquivos permitidos

```text
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs
Assets/_Game/Scripts/Editor/Validation/ValidateFarmSceneComposition.cs
Assets/_Game/Scripts/Editor/Dev/FarmSceneCapture.cs
Assets/_Game/Scripts/World/Scale/**
docs/validation/**
```

## 19. Arquivos proibidos

`Assets/**/*.unity`, `Assets/**/*.prefab`, `Assets/_Game/Art/**/*.meta`, `Packages/**`, `ProjectSettings/**`, `docs_old/**`.

## 20. Estratégia de implementação

### Fase 0 — Medição

Capturar a cena atual e registrar, em unidades de mundo, bounds visuais de casa, ponte, lago, campo, bosque, linha sul e muralha. A referência visual define relações, não pixels copiados: ponte menor que largura do rio; campo central é a maior massa jogável; lago não ocupa toda a metade inferior.

### Fase 1 — Contrato de grupos

Declarar os bounds-alvo das seis massas e validar que elas não invadem footprints sólidos/passáveis proibidos. Usar constantes nomeadas, nunca literais de escala dentro de métodos de criação.

### Fase 2 — Reposição no gerador

Em cada helper de marco, mover somente a raiz visual e sua âncora espacial correspondente; manter ID, interações e área de aproximação. Reduzir ponte e casa para a escala aprovada pelo baseline; redistribuir árvores como massa de floresta em vez de ícones dispersos.

### Fase 3 — Evidência

Rodar regeneração, capturas full/closeup/animals e o validator. A captura visual entra no relatório como comparação lado a lado, sem declarar fidelidade final antes das specs de terreno e decoração.

## Ordem de execucao

1. Medir; 2. registrar contrato; 3. alterar grupos um de cada vez; 4. regenerar; 5. validar física; 6. capturar; 7. documentar.

## 14. Critérios de aceite

### 14.1 Seis massas presentes

- Resultado: os seis grupos têm pelo menos um marco visual e respeitam os target bounds.
- DoD: `CindarsHope/Validar Composição FarmScene` imprime `ValidateFarmSceneComposition: 6/6 groups valid`.

### 14.2 Escala sem outliers

- Resultado: casa, ponte, árvores e construções sul passam nas faixas da matriz de escala.
- DoD: validator imprime `Scale outliers: 0`; a ponte não usa escala literal fora do helper/contrato.

### 14.3 Física preservada

- Resultado: cada marco movido mantém o mesmo ID e uma approach cell válida.
- DoD: `ValidateFarmSceneNavigation` continua com `Reachable required landmarks: 10/10`.

### 14.4 Capturas geradas

- Resultado: há captura full map, homestead e animais após a regeneração.
- DoD: `FarmSceneCapture` registra literalmente três linhas iniciadas por `[FarmSceneCapture] salvo:`.

## 23. Edge cases / falhas

- Redução visual torna collider maior que sprite: revalidar com a spec de física, sem alterar collider por escala de child.
- Casa invade borda ou portal: validator falha pelo grupo/ID.
- Ícone de árvore sem escala de perfil: usar categoria existente ou registrar decisão; não criar multiplicador local oculto.
- Mudança de screenshot por câmera: manter resolução e ortho explicitamente registrados no baseline.

## 22. Validação e gates

Compile, validators de composição/navegação e capturas são obrigatórios. Play Mode completo fica para a spec final, mas movimentação básica é reexecutada após regenerar.

# /speckit.tasks

- [ ] Executar as fases 0–3 na ordem da §21 e registrar a evidência exigida.
