# SPEC — Marcos, campo agrícola e biomas funcionais da FarmScene

> **Spec ID:** `spec_farm_scene_landmarks_and_agriculture_v1`  
> **Status:** A implementar  
> **Wave:** WAVE FARM KEYART — Conteúdo espacial  
> **Priority:** P1  
> **Type:** Integration / Art / Runtime  
> **Domain:** Farm  
> **Parallelizable:** NO  
> **Parallel group:** FARM_KEYART_VISUAL  
> **Can run with:** N/A  
> **Must not run with:** qualquer edição do gerador FarmScene, contratos ou assets de world art  
> **Repo lock scope:** `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs`, `Assets/_Game/Scripts/Farm/**`, `Assets/_Game/Scripts/Editor/Validation/ValidateFarmSceneLandmarks.cs`  
> **Depends on:** `spec_farm_scene_organic_terrain_water_v1`  
> **Blocks:** `spec_farm_scene_biome_decoration_v1`, `spec_farm_scene_keyart_playmode_acceptance_v1`  
> **Scope:** tornar campo, homestead, bosque, animais, montanha e lago reconhecíveis como os marcos da keyart, reutilizando gameplay existente.  
> **Out of scope:** novos loops de animal/crop, novos NPCs, save schema, geração de uma segunda cena ou edição YAML.  
> **Validation level alvo:** UNITY_VALIDATED + PLAYMODE  
> **Executor:** Claude | Codex

required_adrs: []
required_game_rules: []

# /speckit.specify

## Dependências

- **Depende de:** `spec_farm_scene_organic_terrain_water_v1`.
- **Bloqueia:** decoração e aceitação humana desta wave.

## 5. Contexto

Os assets necessários já incluem crops, árvores, bush berry, flower patch, mushroom cluster, tree stump, well, fence, hay bale e animais. A cena tem sistemas de `FarmPlot`, `FishingSpot`, `AnimalReleaseHandler`, `ShippingBin`, `SellPoint`, `CraftingPoint`, Fonte de Anya e recursos. O problema é composição/wiring: campo não domina o centro, animais não têm pens e props existentes não formam biomas.

## 6. Problema

Marcos funcionais existem como objetos isolados, sem leitura espacial ou apoio visual. Gerar novos assets antes de reutilizar os existentes duplicaria conteúdo e ainda deixaria interações sem acesso.

## 7. Objetivo

Ao final, os marcos da keyart são visíveis, escalados e acessíveis: campo central arado/cultivado, homestead com poço e pátio, linha de animais com pens, bosque oeste com clareira da fonte, montanha/caverna norte e lago com pesca.

## 8. Fontes obrigatórias lidas

`AGENTS.md`; `docs/project/CURRENT_STATE.md`; todas as quatro specs anteriores deste lote; `.agents/skills/scene-interactable-wiring/SKILL.md`; `.agents/skills/crop-farming-systems/SKILL.md`; `.agents/skills/system-reuse-audit/SKILL.md`; `CreateMvpFarmScene.cs`; `FarmPlot.cs`; `AnimalReleaseHandler.cs`; `FishingSpot.cs`; `WorldSpriteLibrary.cs`; catálogo `Assets/_Game/Art/Generated/World/**`.

## 9. Estado atual do repo — Phase 0 (auditado em 2026-08-14)

```text
Existe: crops crop_0..5 e seed_0..5, ground_soil, fence, well, hay_bale, animal_0..4.
Existe: foliage bush_berry, flower_patch, log_fallen, mushroom_cluster, tree_stump.
Existe: CreateFarmPlots(), CreateAnimalHousings(), CreateProcessingAndGreenhouse(), CreateFonteAnya(), CreateFishingSpot().
Existe: FarmPlot/AnimalReleaseHandler/FishingSpot/ShippingBin/SellPoint; devem ser reutilizados.
Ausente: validator de presença + approach de marcos visuais e composição de pens/campo declarada.
```

## 13. Regras de não duplicação

Não criar novo FarmPlot, animal system, fishing spot, well interactable, crafting system ou tree resource. Reusar os existentes e criar apenas visual filho sem collider quando necessário. Todo objeto funcional continua no root que já possui sua interação; child visual não altera escala/colisor do root.

## 15. Arquitetura alvo — CRIAR vs MODIFICAR

# /speckit.plan

```text
CRIAR:
  Assets/_Game/Scripts/Editor/Validation/ValidateFarmSceneLandmarks.cs — presença, IDs, agrupamento e approach dos marcos.
  Assets/_Game/Tests/EditMode/Farm/FarmLandmarkCompositionTests.cs — regras puras de composição.
MODIFICAR:
  Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs — helpers de composição que instanciam visuais existentes sob roots funcionais.
  Assets/_Game/Scripts/Farm/FarmLevel1LayoutContract.cs — somente âncoras adicionais nomeadas, se essenciais.
```

## 16. Contratos, dados e eventos

```csharp
public enum FarmLandmarkId { CentralField, HomesteadYard, AnimalPens, WestForest, AnyaClearing, NorthCliff, SouthEastLake }
public static class FarmLandmarkCompositionContract
{
    public static IReadOnlyList<string> RequiredSceneObjects(FarmLandmarkId landmark);
    public static Bounds GetBounds(FarmLandmarkId landmark);
}
```

Eventos e save: N/A. Crops/animals continuam usando os DTOs e eventos existentes.

## 17. Sistemas afetados

Farm scene generator / crop interaction / animals / fishing / resource nodes / Fonte de Anya / visual sorting / editor validation.

## 18. Arquivos permitidos

```text
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs
Assets/_Game/Scripts/Editor/Validation/ValidateFarmSceneLandmarks.cs
Assets/_Game/Scripts/Farm/FarmLevel1LayoutContract.cs
Assets/_Game/Tests/EditMode/Farm/**
docs/validation/**
```

## 19. Arquivos proibidos

`Assets/**/*.unity`, `Assets/**/*.prefab`, `Assets/**/*.asset`, `Assets/_Game/Scripts/Save/**`, `Packages/**`, `ProjectSettings/**`, `docs_old/**`.

## 20. Estratégia de implementação

### Fase 0 — Reuse audit

Listar cada asset disponível e cada root funcional da cena. Se um dos systems de animal/crop/fishing tiver duplicata ativa, parar e reportar em vez de escolher um silenciosamente.

### Fase 1 — Campo central

Pintar `ground_soil` pela máscara do campo; criar uma composição visual de fileiras com crops existentes, mantendo tiles de jogo aráveis e evitando roots de `FarmPlot` duplicados. Cercas são visuais/obstáculos somente onde declaradas pela física.

### Fase 2 — Homestead e animais

Adicionar well, barrels e craft yard visuais; formar pens com `fence`, `hay_bale` e animals existentes. Os animais permanecem no `AnimalReleaseHandler` e têm passageiro/approach testado.

### Fase 3 — Biomas

Compor bosque oeste em camadas com árvores, berry/stump/log/mushroom já existentes; criar clareira para Fonte de Anya. Montanha recebe cave/ore props; lago preserva `FishingSpot` e sua approach cell.

### Fase 4 — Validators e Play Mode

Validator exige objetos/contagens mínimas e consulta a navegação. Executar interações reais: plantar/regar/colher, pescar, interagir com fonte, alimentar/liberar animal conforme o runtime existente.

## Ordem de execucao

1. Reuse audit; 2. campo; 3. homestead/animais; 4. bosque/montanha/lago; 5. validator; 6. Play Mode; 7. relatório.

## 14. Critérios de aceite

### 14.1 Campo protagonista e funcional

- Resultado: `CentralField` tem solo arado visual, ao menos quatro fileiras de crop e uma approach cell por borda, sem bloquear aragem permitida.
- DoD: validator imprime `CentralField: PASS (rows>=4, approaches>=4)`; cenário humano planta e rega um tile válido.

### 14.2 Marcos completos

- Resultado: os sete landmarks possuem todos os objetos requeridos pelo contrato.
- DoD: `ValidateFarmSceneLandmarks` imprime `Landmarks valid: 7/7`.

### 14.3 Nenhum sistema paralelo

- Resultado: os objetos de crop/animal/fishing mantêm os componentes runtime existentes, sem nova implementação desses sistemas.
- DoD: relatório traz `Existing Systems Audit` e `Created new runtime gameplay systems: none`.

### 14.4 Acessibilidade e compilação

- Resultado: marcos interativos são alcançáveis e assemblies compilam.
- DoD: `ValidateFarmSceneNavigation` mantém `10/10`; `RunUnityCompileValidation.ps1` retorna exit `0`.

## 23. Edge cases / falhas

- Cerca fecha passagem: validator exige approach externo para porta/animal building.
- Crop decorativo confundido com crop real: nomes devem usar prefixo `Visual_`; não recebem `FarmPlot`.
- Child visual muda collider do root: manter root em escala 1 e ajustar apenas child, como precedentes existentes.
- Fonte/caverna obstruída por props: approach test falha antes de aceitar.
- Save de tile arado: nenhum novo componente Unity entra no DTO.

## 22. Validação e gates

EditMode, validators, compile e Play Mode de interações são obrigatórios. Caso Play Mode não rode, registrar o bloco `NOT RUN` canônico e não marcar a spec como aceita.

# /speckit.tasks

- [ ] Executar as fases 0–4 na ordem da §21 e registrar a evidência exigida.
