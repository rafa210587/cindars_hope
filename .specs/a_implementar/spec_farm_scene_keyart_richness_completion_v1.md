# spec_farm_scene_keyart_richness_completion_v1

| Campo | Valor |
|---|---|
| **Spec ID** | `spec_farm_scene_keyart_richness_completion_v1` |
| **Wave** | FARM KEYART (fidelidade) |
| **Type** | Runtime (editor scene-gen) + Data (arte) |
| **Domain** | FarmScene / composição visual (densidade + bordas + props) |
| **Priority** | Média-alta (fecha a "cara de pintura" da keyart) |
| **Parallelizable** | Não (mesmo gerador + planner de decoração) |
| **Repo lock scope** | `CreateMvpFarmScene.cs`, `FarmDecorationPlanner.cs`, `WorldTilemapGround.cs`, `FarmSceneCompositionContract.cs`, `Art/Generated/World/**` (novos sprites/tiles), validators |
| **Depends** | `spec_farm_scene_keyart_visual_corrections_v1` (bugs corrigidos primeiro) |
| **Blocks** | `spec_farm_scene_keyart_playmode_acceptance_v1` |
| **Validation level** | BUILD_VALIDATED + Unity regen + captura sem Gizmos comparada à keyart |
| **Executor** | Codex (wiring) + arte gerada no ChatGPT (dep. externa marcada) |

## Objetivo
Fechar o gap de ESTILO restante entre a FarmScene e a keyart aprovada, adicionando o conteúdo/densidade
que ainda falta após a wave keyart e as correções v1. Referência de verdade: a keyart. Cada item abaixo
é validado por captura comparada.

## §9 — Estado (Phase 0 — executor confirma)
A wave keyart já entregou: contrato espacial (polígonos), colisão poligonal Lake/River/Mountain, terreno
poligonal com anel de transição de 8 vizinhos (`WorldTilemapGround`), planner de decoração determinístico
(`FarmDecorationPlanner`), crop rows visuais e validators. **Faltam** (comparando com a keyart): borda de
PEDRA no lago/rio (hoje é areia), props aquáticos (juncos/lírios/píer/barco/cascata), volume da montanha
(hoje faixa chapada) + props na base (feno/ore/boulders), densidade de decoração maior, e detalhes do
homestead/animais (poço, banca com toldo, cercados com galinhas).
```powershell
# Executor: confirmar helpers/densidade atuais
Select-String -Path Assets/_Game/Scripts/Editor/Art/WorldTilemapGround.cs -Pattern "PaintTile|PaintShoreRing|Transition|Neighbour|Ring|Autotile"
Select-String -Path Assets/_Game/Scripts/Editor/Art/FarmDecorationPlanner.cs -Pattern "density|Density|weight|biome|Scatter|count"
Get-ChildItem Assets/_Game/Art/Generated/World/props, Assets/_Game/Art/Generated/World/tiles -Filter *.png | Select Name
```

## §13 — Não duplicar
Reusar `FarmDecorationPlanner` (só estender biomas/pesos/props), `WorldTilemapGround` (transição já existe),
o contrato espacial (polígonos do lago/rio/montanha) e `CreateMvpFarmScene`. NÃO criar sistema paralelo de
decoração/terreno. Skills: `tilemap-world-rendering`, `chatgpt-web-sprite-gen` (arte), `scene-interactable-wiring`.

## §20 — Fases (cada uma: arte se preciso → wiring → DoD por captura vs keyart)

### R-Água — borda de pedra + props aquáticos  🎨+wiring
- **ARTE (ChatGPT):** tiles de **margem de pedra** (rock border 9-slice p/ água) + props: **juncos/cattails**,
  **vitórias-régias (lily pads)**, **cascata** (nascente). Píer + barco: usar props existentes ou gerar.
- **WIRING (`CreateMvpFarmScene`/`WorldTilemapGround`):** trocar a margem de AREIA do lago/rio por
  **rock border** (autotile no perímetro do polígono de água). Espalhar juncos na beira + lírios na água
  (via planner, bioma "WaterEdge"). Adicionar **cascata** na nascente do rio (topo) e **píer + barco** no lago.
- **DoD:** captura do lago = borda de pedra recortada (não faixa de areia), com juncos/lírios/píer/barco;
  cascata na nascente. Comparar com o canto SE da keyart.

### R-Montanha — volume + props na base  🎨+wiring
- **ARTE:** muralha de rocha com **profundidade/sombra** (não faixa chapada); opcional: coluna/ledge.
- **WIRING (`CreateMountainBarrier`):** muralha mais alta com o novo sprite; base com **fardos de feno**,
  **ore chunks** (minério brilhante) e **boulders** (reusar `props/rock_ore_*`, `props/hay_bale`).
- **DoD:** topo lê como montanha de rocha com volume + props na base, como na keyart (topo).

### R-Decoração densa — subir densidade  wiring
- **`FarmDecorationPlanner`:** aumentar a densidade de scatter na grama/floresta (mais flores/tufos/trevo/
  cogumelo/pedrinha por área) até a leitura bater com a keyart (densa, não esparsa), respeitando a máscara
  proibida (água/prédio/caminho — ver corrections v1). Reduzir peso da variante sombreada da grama.
- **DoD:** captura full mostra decoração densa e distribuída (sem vazios grandes de grama chapada) e
  **zero** decoração sobre água/prédio/caminho (`Forbidden decoration placements: 0`).

### R-Homestead/Animais — props + cercados  🎨parcial+wiring
- **ARTE (se faltar):** **poço** (well), **banca de venda com toldo**, peças de **cerca** (pen), **galinha**.
- **WIRING:** adicionar **poço** decorativo no centro-topo (entre homestead e cultivo); **banca com toldo**
  no pátio; **cercado (fence pen)** ao lado da casa; **cercado do galinheiro** com **galinhas** + feno.
  (Well/fence/hay já existem em `props/` — conferir; galinha e toldo podem precisar gerar.)
- **DoD:** homestead com poço + banca com toldo; galinheiro com cerca+galinhas, como na keyart.

### R-Floresta — densidade em camadas  wiring (arte de árvore vem da corrections v1 Fase E)
- **`CreateMvpFarmScene`/planner:** adensar o bosque oeste em camadas: pinheiros altos dominantes +
  macieiras (com fruta) nas clareiras + **tocos**, **cogumelos**, **arbustos de berry** no chão (reusar
  `foliage/*`, `props/*`). Fonte da Anya numa **clareira** com anel de pedra + flores ao redor.
- **DoD:** oeste lê como floresta densa e layered (Y-sort), Fonte numa clareira — comparar com a keyart (oeste).

## §14 — Critérios de aceite (binários)
1. `dotnet build .\CindarsHope.Editor.csproj` → exit 0.
2. `CindarsHope/Inicializar Projeto` sem erro no Console.
3. Validators (decoração/composição/navegação) → `0 error(s)`; `Forbidden decoration placements: 0`.
4. Captura sem Gizmos comparada à keyart, região por região (lago, montanha, chão, homestead, floresta):
   cada região "lê" como a keyart (borda de pedra, densidade, volume, props). Registrar as capturas.

## §23 — Edge cases / falhas
- **Arte ausente:** se um sprite (cascata/toldo/galinha/rock-border) ainda não existe, o wiring deve usar
  fallback existente + logar wiring-error, NÃO mascarar; a fase fica `PARTIAL` até a arte entrar.
- **Densidade x performance:** decoração é SpriteRenderer estático (editor-time), sem custo de runtime;
  ainda assim evitar contagem absurda (>alguns milhares) — usar densidade por área razoável.
- **Autotile de rock border:** exige as peças de canto/lado corretas; sem elas, cai no shore de areia
  (fallback) — não inventar peça.
- **Determinismo:** manter o hash/seed do planner (a decoração não pode rerollar a cada regen).
- **Y-sort:** props aquáticos/floresta com pivot na base e sort por Y pra profundidade correta.

## §18/19 — Arquivos permitidos / proibidos
- **Permitidos:** gerador, planner, `WorldTilemapGround`, contrato de composição, `Art/Generated/World/**`
  (novos PNG/meta), validators, relatório em `docs/validation/`.
- **Proibidos:** YAML manual de `.unity/.prefab/.asset`; Cave/Town; `[MenuItem]` avulso; save DTOs.

## Dependência de arte (gerar no ChatGPT — projeto "Sprites - Fazendeiro")
Sheets a gerar (ver `docs/project/PLANO_FARM_KEYART_FIDELIDADE.md` §"Arte a gerar"): rock-border 9-slice +
boulders; juncos/cattails/lily pads/cascata; muralha de rocha com volume; poço/banca-toldo/galinha; pinheiro
alto + macieira. Fatiar/reseam/importar em `Art/Generated/World/**` antes do wiring de cada fase.

## Validação (rule validation-truth)
Relatório com bloco de validação (exit code, validators com contagem, capturas por região comparadas à
keyart). Fase com arte faltante = `PARTIAL`, nunca PASS.
