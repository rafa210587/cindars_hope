# spec_farm_scene_keyart_visual_corrections_v1

| Campo | Valor |
|---|---|
| **Spec ID** | `spec_farm_scene_keyart_visual_corrections_v1` |
| **Wave** | FARM KEYART (correções pós-execução) |
| **Type** | Runtime (editor scene-gen) + Validation |
| **Domain** | FarmScene / composição visual + navegação |
| **Priority** | Alta (bloqueia leitura da keyart e jogabilidade da ponte) |
| **Parallelizable** | Não (mesmo arquivo `CreateMvpFarmScene.cs` + contrato de composição) |
| **Repo lock scope** | `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs`, `Assets/_Game/Scripts/World/Scale/FarmSceneCompositionContract.cs`, `Assets/_Game/Scripts/Editor/Art/FarmDecorationPlanner.cs`, `Assets/_Game/Scripts/Farm/Scene/FarmSceneSpatialContract.cs`, `Assets/_Game/Scripts/Farm/Scene/FarmSceneNavigationPolicy.cs`, validators correspondentes |
| **Depends** | wave FARM KEYART (contratos espacial/composição/decoração já existem) |
| **Blocks** | `spec_farm_scene_keyart_playmode_acceptance_v1` |
| **Validation level** | BUILD_VALIDATED + Unity regen + captura sem Gizmos + Play Mode da ponte |
| **Executor** | Codex (ou Claude) |

## Objetivo
Corrigir 5 defeitos visuais/físicos reportados pelo humano sobre a FarmScene gerada (pós-wave keyart),
aproximando-a da keyart aprovada (`docs/art_catalog/_reference_keyart_GPT/farm_keyart_layout_aprovado_v1.png`):
1. **Crafts gigantes** — precisam ficar ~do tamanho do personagem (é estação usada por ele).
2. **Árvore/objetos DENTRO do lago** — decoração/árvores estão sendo colocadas sobre a água.
3. **Casa** — (a) uma "porta gigante" sem sentido embaixo da casa; (b) estufa e props dos fundos mal
   posicionados; (c) casa um pouco maior.
4. **Ponte não é atravessável** — o jogador não consegue andar sobre ela.
5. **Árvores não correspondem à keyart** — arte/estilo das árvores destoa (pinheiros altos + macieiras).

## §9 — Estado do repo (Phase 0 — parcial feita por Claude; Codex confirma o resto)
Verificado 2026-08-14 (Claude):
- `FarmSceneCompositionContract` (World/Scale) define as escalas-alvo consumidas pelo gerador:
  `CraftingVisualTargetHeight = 1.8f`, `HomesteadRoofTargetWidth = 7.5f`, `GreenhouseVisualTargetHeight = 5.5f`,
  `BridgeVisualLocalScale = (1.8,0.8,1)`, `BridgePassageProbe = (15,3)`, `WestForestTreeScaleMultiplier = 1.35f`,
  tree scale range 3–5.
- `FarmDecorationPlanner.IsForbiddenCell(Vector2)` (linha ~97) é a máscara que barra decoração; o planner
  itera células e pula `if (IsForbiddenCell(position))`.
- Gerador único: `CreateMvpFarmScene.CreateScene()`. Água/ponte via contrato espacial `FarmSceneSpatialContract`
  (polígonos) + `FarmSceneNavigationPolicy` (colisão Lake/River/Mountain como `PolygonCollider2D`).
- Player: sprite 32×48 (`FarmScaleContract.PlayerVisualHeightPixels = 48`), altura visível de jogo ~1.3–1.5u.

**Comandos Phase 0 que o EXECUTOR deve rodar e colar o resultado antes de editar:**
```powershell
Select-String -Path Assets/_Game/Scripts/World/Scale/FarmSceneCompositionContract.cs -Pattern "CraftingVisualTargetHeight|HomesteadRoofTargetWidth|GreenhouseVisualTargetHeight|BridgePassageProbe"
Select-String -Path Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs -Pattern "CreateFarmWalkInHouse|threshold|leaf|door|CreateGreenhouse|CreateFarmHouseDoor|FarmDoorGapWidth|FarmWallThickness"
Select-String -Path Assets/_Game/Scripts/Editor/Art/FarmDecorationPlanner.cs -Pattern "IsForbiddenCell|SouthEastLake|Water|GetTargetBounds|polygon|Contains"
Select-String -Path Assets/_Game/Scripts/Farm/Scene/FarmSceneNavigationPolicy.cs -Pattern "Bridge|RiverCrossing|Passage|corridor|15|gap"
Select-String -Path Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs -Pattern "CreateFarmPerimeterTreeline|CreateTree|CreateTrees|treeline"
```

## §13 — Não duplicar (reusar o que existe)
NÃO criar novo sistema de escala/decoração/colisão. Ajustar os **contratos e o gerador existentes**:
`FarmSceneCompositionContract` (constantes de escala), `FarmDecorationPlanner` (máscara), `CreateMvpFarmScene`
(posições/porta/estufa/treeline), `FarmSceneSpatialContract`/`FarmSceneNavigationPolicy` (corredor da ponte).
Reusar `system-reuse-audit`, `tilemap-world-rendering`, `scene-interactable-wiring`.

## §16 — Contratos afetados (assinaturas)
- `FarmSceneCompositionContract` (público estático): `CraftingVisualTargetHeight`, `HomesteadRoofTargetWidth`,
  `GreenhouseVisualTargetHeight` — só valores mudam. Adicionar (se útil) `FishingPierTargetHeight`,
  mantendo `IsScaleWithinApprovedRange`.
- `FarmDecorationPlanner.IsForbiddenCell(Vector2 position) -> bool` — estender a máscara para incluir o
  polígono de água (Lake/River) + buffer, e o footprint de casa/estufa/pátio (já parcial).
- `FarmSceneNavigationPolicy` — garantir corredor livre no `BridgePassageProbe (15,3)` e água bloqueada
  em `(15,5)` e `(15,1)` (o validator já checa; a geometria precisa bater).

## §20 — Estratégia por fase (edição por arquivo/método + DoD)

### Fase A — Crafts do tamanho do personagem
- **Arquivo:** `FarmSceneCompositionContract.cs`. **Edição:** `CraftingVisualTargetHeight = 1.8f → 1.3f`
  (~altura do player). Se o gerador escala o pai inteiro do sprite (com margem), usar `1.2f`.
- **Arquivo:** `CreateMvpFarmScene.cs` (`CreateCraftingStation`): confirmar que o sprite bespoke usa
  `ApplyUniformBespokeScale(..., FarmSceneCompositionContract.CraftingVisualTargetHeight)`; o `SolidBody`
  collider e o trigger de interação **não mudam** (já existem, ver handoff Codex).
- **DoD:** após `CindarsHope/Inicializar Projeto`, capturar close no homestead; os 3 crafts (workbench/forge/
  cooking) têm altura visível ≈ a do player (±0.3u), NÃO 2–3× o player. Validator de composição
  (`CindarsHope/Validar ...`) imprime `0 error(s)` para escala de crafts.

### Fase B — Nada de árvore/decoração sobre a água
- **Arquivo:** `FarmDecorationPlanner.cs` (`IsForbiddenCell`): incluir o polígono de ÁGUA
  (`FarmSceneSpatialContract` Lake + River) + **buffer de 1u** como célula proibida (point-in-polygon).
  Se a máscara já cobre "spatial water", conferir que o polígono usado é o REAL (não o bounds retangular).
- **Arquivo:** `CreateMvpFarmScene.cs` (`CreateFarmPerimeterTreeline` e qualquer scatter de árvore):
  antes de instanciar cada árvore, checar `!FarmDecorationPlanner.IsForbiddenCell(pos)` (ou o
  point-in-polygon de água do contrato). Remover a árvore reportada no meio do lago.
- **DoD:** captura full mostra **zero** árvore/decoração/pedra sobre o polígono do lago ou do rio.
  `ValidateFarmSceneDecoration` imprime `Forbidden decoration placements: 0`.

### Fase C — Casa: porta gigante + estufa/props + tamanho
- **Arquivo:** `CreateMvpFarmScene.cs` (`CreateFarmWalkInHouse`, `CreateFarmHouseDoor`):
  - **Porta gigante:** hoje há um `threshold`/`leaf`/door sprite esticado (`localScale` grande ~
    `FarmDoorGapWidth × FarmWallThickness*2`) que renderiza como uma "porta enorme" abaixo da fachada.
    Como agora a casa usa o sprite bespoke `Building("farmhouse")` (que JÁ tem porta desenhada),
    **remover/ocultar** o `leaf`/threshold visual solto (manter só o `HouseDoorInteractable`+collider de
    porta se a mecânica de entrar exigir; o VISUAL da porta vem do sprite da casa). Se o walk-in precisa
    de um trigger de porta, deixá-lo SEM SpriteRenderer (só collider trigger).
  - **Estufa e props dos fundos:** reposicionar `CreateGreenhouse` para ficar **ao lado da casa**
    (à direita/leste, como na keyart), não sobreposta nem "atrás". Conferir `barrels`/props traseiros
    do homestead pra não colidir com a casa (usar `HomesteadDecorationClearance = 2f`).
  - **Tamanho:** `FarmSceneCompositionContract.HomesteadRoofTargetWidth = 7.5f → 8.5f` (casa um pouco maior,
    mantendo dominância sobre os secundários).
- **DoD:** close no homestead mostra: casa com porta coerente (sem "porta gigante" flutuando), estufa ao
  lado (não sobreposta), casa ~8.5u. Sem sobreposição casa↔estufa↔props.

### Fase D — Ponte atravessável
- **Arquivos:** `FarmSceneSpatialContract.cs` / `FarmSceneNavigationPolicy.cs`.
- **Edição:** garantir que o polígono de colisão da ÁGUA (River) tenha um **vão real** no corredor da
  ponte em torno de `BridgePassageProbe (15,3)` (largura ≥ 2u), e que o segmento visual
  `RiverCrossing_Bridge` **não** tenha collider (é só água visual sob a ponte). O jogador deve poder
  ficar em `(15,3)` e cruzar de `(15, >3)` para `(15, <3)` sem colidir.
- **DoD (Play Mode):** entrar em Play, mover o player até a ponte e **atravessar o rio pela ponte**
  (de um lado ao outro) sem travar. `ValidateFarmSceneNavigation` imprime corredor livre em `(15,3)`
  e água bloqueada em `(15,5)` e `(15,1)` (`0 error(s)`).

### Fase E — Árvores fiéis à keyart *(dependência de ARTE — ver nota)*
- A arte atual de árvore (`Art/Generated/World/trees/*`) destoa da keyart (pinheiros altos + macieiras
  com fruta). **Esta fase depende de novos sprites de árvore** (gerados no ChatGPT — fora do escopo de
  código puro). Enquanto a arte nova não existe:
  - **Código (Codex pode fazer já):** aumentar variedade/uso — garantir que `CreateTree` sorteia entre
    `tree_pine`/`tree_oak`/`tree_apple`/`tree_willow` (já existem) com peso maior de PINHEIRO no bosque
    oeste (a keyart é dominada por pinheiros), e macieira nas clareiras. Usar
    `WestForestTreeScaleMultiplier` e a faixa 3–5.
  - **Arte (humano/Claude via ChatGPT):** gerar sprites de pinheiro alto e macieira com fruta fiéis à
    keyart, importar em `Art/Generated/World/trees/`, e o `CreateTree` passa a usá-los. Registrar como
    sub-tarefa de arte no plano de fidelidade.
- **DoD (parte código):** bosque oeste com predominância de pinheiro; captura comparada à keyart.
  DoD (parte arte): sprites novos importados e wirados (spec/sub-tarefa separada se preferir isolar).

## §14 — Critérios de aceite (binários)
1. `dotnet build .\CindarsHope.Editor.csproj` → exit 0.
2. `CindarsHope/Inicializar Projeto` regenera a FarmScene sem erro no Console (0 vermelho).
3. `CindarsHope/Validar Navegacao FarmScene` (e o validator de composição/decoração) imprimem `0 error(s)`
   para: escala de craft, corredor da ponte, decoração sobre água (`0 placements`).
4. Captura sem Gizmos (full + close homestead): crafts ~player-size; sem árvore no lago; casa sem porta
   gigante + estufa ao lado; ponte com vão visível.
5. Play Mode: player atravessa a ponte de um lado ao outro.

## §23 — Edge cases / falhas
- **Regen destrutiva:** `Inicializar Projeto` recria a cena; garantir idempotência (sem duplicar roots).
- **Máscara de água por bounds vs polígono:** se `IsForbiddenCell` usa o *bounds* retangular do lago
  (26×14) em vez do polígono, o buffer pode proibir grama boa OU liberar água real — usar o POLÍGONO.
- **Colisor da ponte:** remover o collider do vão pode abrir um buraco por onde o player cai fora do rio;
  garantir que o vão é só no corredor (15,3±1), não no rio todo.
- **Escala de craft muito pequena:** 1.2f pode esconder o sprite atrás do player (Y-sort); manter
  `spriteSortPoint = Pivot` e pivot na base.
- **Porta:** se o walk-in depende do `leaf` para o RoofReveal/entrada, não remover o collider/trigger —
  só o SpriteRenderer duplicado.

## §18/19 — Arquivos permitidos / proibidos
- **Permitidos:** os do Repo lock scope acima + os validators citados + este spec/relatório em
  `docs/validation/`.
- **Proibidos:** editar `.unity/.prefab/.asset` YAML na mão; tocar Cave/Town; criar `[MenuItem]` avulso
  (usar os 3 comandos canônicos); mudar save DTOs.

## Validação (rule validation-truth)
Bloco obrigatório no relatório: método, exit code, Assembly-CSharp-Editor PASS/FAIL, validators
(nav/composição/decoração) com contagem `error(s)`, e captura + resultado do Play Mode da ponte.
Nada é PASS sem a saída/print correspondente.
