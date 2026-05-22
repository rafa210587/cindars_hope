# Cindar's Hope — Fase 7: Spec Kit v2.2
# MVP Fazenda — Specs Individuais e Completas

> **Fase:** 7 de 13
> **Status:** ✅ Specs MVP Fazenda completas — prontas para implementação
> **Última atualização:** 2026-05-16
> **Motivo da reescrita:** v1.0 não tinha specs de arte, animação, VFX e agrupava stories demais

---

## NOTA DE SINCRONIZAÇÃO v2.1

Esta versão apenas sincroniza status e ferramenta de arte. As specs e tasks do MVP Fazenda foram preservadas. A ferramenta principal de sprites passa a ser Aseprite; Pixelorama e LibreSprite permanecem alternativas válidas desde que exportem os mesmos PNGs/spritesheets.



## NOTA DE EXECUÇÃO v2.2 — Enriquecimento para Codex

A Fase 7 continua sendo a fonte das specs MVP Fazenda, mas a execução não deve ser feita spec inteira por spec inteira. A implementação deve seguir o plano `FASE8_EXECUTION_PLAN_CODEX_v1.0.md`, quebrando as tasks em PRs pequenos.

### Correções técnicas consolidadas nesta versão

1. **Save path:** usar `Application.persistentDataPath/saves/slot_1.json`; não usar `StreamingAssets` para escrita de save.
2. **Registry de dados:** save serializa IDs (`item_crop_wheat`, `seed_wheat`) e resolve via registries; não serializa referências Unity.
3. **Busca de objetos:** não usar `GameObject.Find`, `FindObjectOfType` ou `FindObjectsByType` em runtime. Para listas de cena, usar `[SerializeField]` ou Editor script/installer.
4. **Codex:** cada task deve ter arquivos permitidos/proibidos e teste manual.
5. **Sprites:** IA gera rascunho/conceito; Aseprite finaliza e exporta.

### Template obrigatório para adicionar a cada PR

```md
## CODEX_PR

**PR:** PR-XXX — Nome curto
**Specs relacionadas:** FARM-XXX, FARM-YYY
**Objetivo:** uma frase

**Arquivos permitidos:**
- caminho/arquivo.cs

**Arquivos proibidos:**
- docs/GDD_v2.6.md
- qualquer arquivo fora do escopo

**Regras:**
- Não implementar V2/FULL.
- Não alterar arquitetura.
- Não criar dependência externa.
- Não usar busca global em runtime.

**Definition of Done:**
- Unity compila.
- Teste manual passa.
- Console sem erro novo.
- Commit em português.
```

---
## HANDOFF

### Plano macro
| # | Fase | Status |
|---|---|---|
| 1–6 | Ideação → Histórias | ✅ Concluídas |
| 7 | **Spec Kit MVP Farm** | ✅ Specs completas |
| 8 | MVP — Implementação | 🔄 Próxima execução |
| 9–13 | Expansão → Launch | ⏳ Pendente |

### Convenção de camadas em cada spec
Cada spec tem 3 camadas de tasks:
- **[CODE]** — lógica C#, ScriptableObjects, sistemas Unity
- **[ART]** — sprites, tilesets e ícones (Aseprite final; ChatGPT/DALL-E para conceito/ícones simples; PixelLab/Scenario opcionais para sprites/tilesets consistentes; Pixelorama/LibreSprite como fallback)
- **[ANIM]** — animações, Animator Controller, parâmetros
- **[VFX]** — partículas, efeitos visuais, feedback
- **[UI]** — Canvas, layout, fonte, cores
- **[TEST]** — critérios de aceite verificáveis

### Convenção de arte
- Ferramenta principal final: **Aseprite** (pixel art, spritesheets, animação). IA auxiliar: ChatGPT/DALL-E para conceito e ícones simples; PixelLab/Scenario opcionais para sprites, tilesets e consistência em lote. Fallback: Pixelorama/LibreSprite
- Resolução padrão personagem: 32x48px
- Resolução padrão tile: 32x32px
- Resolução padrão ícone item: 32x32px
- Paleta fazenda: laranja #D4832A, bronze #8B6914, terra #6B3A2A, verde #4A6741
- Outline personagens: 1px #0A0A0A
- Export: PNG com fundo transparente
- Nomenclatura: `[Categoria]_[Nome]_[Tamanho].png`

### Índice de specs neste documento
| ID | Nome | Status |
|---|---|---|
| FARM-001 | Cena da Fazenda e Personagem | ✅ Spec completa |
| FARM-002 | Movimento do Jogador | ✅ Spec completa |
| FARM-003 | Layout da Fazenda | ✅ Spec completa |
| FARM-011 | SeedDataSO e Assets de Sementes | ✅ Spec completa |
| FARM-012 | CropTile — Estado e Visual | ✅ Spec completa |
| FARM-013 | Crescimento de Plantas | ✅ Spec completa |
| FARM-014 | InteractionSystem e IInteractable | ✅ Spec completa |
| FARM-015 | Colheita | ✅ Spec completa |
| FARM-016 | Menu de Plantio | ✅ Spec completa |
| FARM-021 | InventoryManager | ✅ Spec completa |
| FARM-022 | InventoryUI (lista texto MVP) | ✅ Spec completa |
| FARM-031 | TimeManager MVP | ✅ Spec completa |
| FARM-032 | HUD — Dia e Ouro | ✅ Spec completa |
| FARM-041 | TreeDataSO e Assets de Árvore | ✅ Spec completa |
| FARM-042 | TreeObject — Corte e Visual | ✅ Spec completa |
| FARM-051 | Lago — Tilemap e Visual | ✅ Spec completa |
| FARM-052 | FishingSpot — Pesca Básica | ✅ Spec completa |
| FARM-061 | HungerSystem | ✅ Spec completa |
| FARM-062 | HUD — HP e Fome | ✅ Spec completa |
| FARM-071 | SaveManager | ✅ Spec completa |
| FARM-072 | BootScene e Load | ✅ Spec completa |
| FARM-SELL | SellPoint e SellMenu | ✅ Spec completa |

---

---

# SPEC FARM-001 — Cena da Fazenda e Personagem

## /speckit.specify

**O QUE:** A FarmScene existe, o personagem aparece nela com sprite placeholder, pode se mover, e a câmera o segue.

**POR QUE:** É a fundação. Sem isso, nenhuma outra feature pode ser testada.

**Escopo desta spec:**
- Cena Unity com tilemap de chão
- Prefab do jogador com sprite placeholder
- Câmera com follow

**Fora de escopo:** Arte final, animações, sistemas de gameplay.

---

## /speckit.plan

**Arquitetura:**
```
FarmScene
├── Tilemaps/
│   ├── Ground_Tilemap       (walkable, Sorting: Ground)
│   └── Walls_Tilemap        (non-walkable, Sorting: Background)
├── Player (Prefab)
│   ├── SpriteRenderer       (Sorting: Characters)
│   ├── Rigidbody2D          (Kinematic, Collision: Continuous)
│   ├── BoxCollider2D        (28x44px — menor que o sprite)
│   └── PlayerController.cs
├── Managers/
│   ├── GameEventBus.cs      (estático, não precisa de GameObject)
│   └── PlayerManager.cs     (DontDestroyOnLoad)
└── CinemachineVirtualCamera
```

**ScriptableObjects novos:**
- `PlayerDataSO` — dados base do jogador

---

## /speckit.tasks

### [CODE]
- [ ] **FARM-001-C1:** Criar `GameEventBus.cs` em `Scripts/Core/` (implementação conforme ARCH_fase4_v2.0.md seção 2)
- [ ] **FARM-001-C2:** Criar `PlayerDataSO.cs` com campos: `float MoveSpeed = 5f`, `int BaseHP = 100`, `int StartingGold = 50`, `List<StartingItem> StartingItems`
- [ ] **FARM-001-C3:** Criar asset `Data/Config/PlayerData.asset` preenchido
- [ ] **FARM-001-C4:** Criar `PlayerManager.cs` (DontDestroyOnLoad): lê `PlayerDataSO`, expõe `int CurrentHP`, `int MaxHP`, `int Gold`
- [ ] **FARM-001-C5:** Criar `PlayerController.cs`: referência ao `PlayerDataSO`, Input System (`PlayerInputActions`), método `Move(Vector2 input)`
- [ ] **FARM-001-C6:** Criar `PlayerInputActions.inputactions`: Action Map `Player`, Action `Move` (Value, Vector2), Action `Interact` (Button), Action `Inventory` (Button), Action `Sleep` (Button)
- [ ] **FARM-001-C7:** Criar `FarmScene.unity` com hierarquia acima
- [ ] **FARM-001-C8:** Configurar Cinemachine Virtual Camera 2D: Follow = Player, Dead Zone X/Y = 0.5, Damping = 0.5

### [ART] — Placeholder (MVP)
- [ ] **FARM-001-A1:** Criar sprite `Player_Placeholder_32x48.png` no Aseprite
  - Canvas: 32x48px, fundo transparente
  - Corpo: retângulo sólido azul #3A7BD5, 28x40px centrado
  - Cabeça: quadrado azul-claro #5A9BF5, 20x20px centrado no topo
  - Outline: 1px #0A0A0A em tudo
  - Exportar para `Assets/_Game/Sprites/Placeholders/Player_Placeholder_32x48.png`

- [ ] **FARM-001-A2:** Criar tile `Tile_Ground_Farm_Placeholder_32x32.png`
  - Canvas: 32x32px
  - Preenchimento sólido: #C8A464 (areia laranja — tom provisório da fazenda)
  - Sem outline
  - Exportar para `Assets/_Game/Sprites/Placeholders/`

- [ ] **FARM-001-A3:** Criar tile `Tile_Wall_Farm_Placeholder_32x32.png`
  - Canvas: 32x32px
  - Preenchimento sólido: #5C3D1E (marrom-escuro)
  - Exportar para `Assets/_Game/Sprites/Placeholders/`

- [ ] **FARM-001-A4:** Importar todos os PNGs no Unity:
  - Texture Type: Sprite (2D and UI)
  - Pixels Per Unit: 32
  - Filter Mode: Point ← obrigatório
  - Compression: None ← obrigatório
  - Aplicar TextureImporterPreset `PixelArt_Sprite`

- [ ] **FARM-001-A5:** Criar Tile assets no Unity para cada PNG (Right-click PNG → Create → 2D → Tile)
  - `Tile_Ground_Farm_Placeholder.asset`
  - `Tile_Wall_Farm_Placeholder.asset`

- [ ] **FARM-001-A6:** Pintar tilemap da FarmScene com os tiles criados (Tile Palette)

### [ANIM] — Placeholder (MVP)
- [ ] **FARM-001-AN1:** Criar `AnimatorController` `Player_AC.controller` em `Animations/Characters/`
- [ ] **FARM-001-AN2:** Adicionar state `Idle` com clip vazio (sprite estático) — padrão no MVP
- [ ] **FARM-001-AN3:** Adicionar parâmetros (sem transições ativas no MVP, mas estrutura pronta):
  - `IsMoving` (bool)
  - `FacingDirection` (int: 0=Down, 1=Up, 2=Left, 3=Right)
  - `IsInteracting` (bool)
- [ ] **FARM-001-AN4:** Atribuir `Player_AC` ao componente `Animator` no prefab `Player`

### [TEST]
- [ ] **FARM-001-TEST1:** Abrir FarmScene → jogador aparece no centro → sprite azul visível → sem erros no Console
- [ ] **FARM-001-TEST2:** Câmera segue o jogador ao mover (verificar visualmente)
- [ ] **FARM-001-TEST3:** Jogador não sai dos limites do tilemap

---

---

# SPEC FARM-002 — Movimento do Jogador

## /speckit.specify

**O QUE:** Personagem se move com WASD em 4 direções, velocidade configurável, sem animação no MVP.

**POR QUE:** Sem movimento, nada pode ser testado in-game.

**Fora de escopo:** Animação de walk, sprint, dash, efeito de pegadas.

---

## /speckit.plan

- `PlayerController.cs` lê input do `PlayerInputActions`
- Usa `Rigidbody2D.MovePosition` para movimento físico preciso
- `SpeedMultiplier` exposto para modificação por outros sistemas (fome, status)
- Colisão automática via `TilemapCollider2D` + `CompositeCollider2D`

---

## /speckit.tasks

### [CODE]
- [ ] **FARM-002-C1:** Em `PlayerController.Update()`: ler `_moveInput` do PlayerInput, normalizar vetor (anti-diagonal), calcular `velocity = moveInput * MoveSpeed * SpeedMultiplier`
- [ ] **FARM-002-C2:** Em `PlayerController.FixedUpdate()`: `_rb.MovePosition(_rb.position + velocity * Time.fixedDeltaTime)`
- [ ] **FARM-002-C3:** Expor `float SpeedMultiplier = 1f` (public setter para HungerSystem usar)
- [ ] **FARM-002-C4:** Publicar `PlayerStepEvent { Vector2 Position }` a cada 1 unidade percorrida (acumular distância no Update)
- [ ] **FARM-002-C5:** Adicionar `TilemapCollider2D` (Used by Composite: true) + `CompositeCollider2D` (Geometry Type: Polygons) no Tilemap de bordas

### [ART]
- [ ] **FARM-002-A1:** Nenhum novo sprite necessário (usa placeholder de FARM-001-A1)
- [ ] **FARM-002-A2:** Adicionar indicador visual de direção no placeholder: linha de 4px na direção que o jogador enfrenta
  - Down: linha na base (#FFFFFF, 4x2px)
  - Up: linha no topo
  - Left/Right: linha na lateral
  - Atualizar `Player_Placeholder_32x48.png` com as 4 variações de direção
  - Exportar 4 sprites separados: `Player_Placeholder_Down_32x48.png`, etc.

### [ANIM]
- [ ] **FARM-002-AN1:** Em `PlayerController`: ao receber input, setar `Animator.SetBool("IsMoving", true/false)`
- [ ] **FARM-002-AN2:** Setar `Animator.SetInteger("FacingDirection", 0/1/2/3)` conforme direção do input
- [ ] **FARM-002-AN3:** Criar AnimationClip `Player_Idle_Down` (1 frame, sprite `Player_Placeholder_Down`)
- [ ] **FARM-002-AN4:** Criar clips `Player_Idle_Up`, `Player_Idle_Left`, `Player_Idle_Right` (1 frame cada)
- [ ] **FARM-002-AN5:** No AnimatorController: criar transições `Idle_Down ↔ Idle_Up ↔ Idle_Left ↔ Idle_Right` via `FacingDirection`

### [TEST]
- [ ] **FARM-002-TEST1:** WASD move nas 4 direções corretas
- [ ] **FARM-002-TEST2:** Diagonal normalizada (sem boost de velocidade)
- [ ] **FARM-002-TEST3:** Borda intransponível em todas as direções
- [ ] **FARM-002-TEST4:** `PlayerStepEvent` publicado a cada 1 unidade (verificar com Debug.Log temporário)

---

---

# SPEC FARM-003 — Layout da Fazenda

## /speckit.specify

**O QUE:** FarmScene tem mapa dividido em zonas: canteiros 3x3, lago, 4 árvores, espaço livre, bordas.

**POR QUE:** O jogador precisa identificar visualmente onde fazer cada ação.

**Dimensões do MVP (tamanho 1x):**
- Mapa total: 20x16 tiles (640x512px)
- Canteiros: bloco 3x3 na região central-esquerda
- Lago: 2x3 tiles na região direita
- Árvores: 4 posições fixas na região esquerda
- SellPoint: 1 tile próximo à entrada
- Spawn do jogador: centro do mapa

---

## /speckit.plan

**Tilemaps na cena:**
```
FarmScene/Tilemaps/
├── Ground_Tilemap       (Layer: Ground, Order: 0)
├── Water_Tilemap        (Layer: Ground, Order: 1, TilemapCollider2D)
├── Walls_Tilemap        (Layer: Ground, Order: 2, TilemapCollider2D)
└── Decoration_Tilemap   (Layer: Decoration, Order: 3, sem collider)
```

---

## /speckit.tasks

### [CODE]
- [ ] **FARM-003-C1:** Criar `FarmSaveData.cs`:
  ```csharp
  [Serializable] public class FarmSaveData {
      public PlotSaveData[] Plots = new PlotSaveData[9];
      public TreeSaveData[] Trees = new TreeSaveData[4];
      public int CurrentDay = 1;
  }
  [Serializable] public class PlotSaveData { public string SeedId; public int DaysGrown; public bool IsReady; }
  [Serializable] public class TreeSaveData { public int ChopLevel = 4; }
  ```
- [ ] **FARM-003-C2:** Criar 9 `PlotTile` GameObjects em grade 3x3, referencias salvas em `FarmSystem`
- [ ] **FARM-003-C3:** Criar 4 `TreeSpot` GameObjects em posições fixas
- [ ] **FARM-003-C4:** Criar `FishingSpot` GameObjects em 3 posições na borda do lago
- [ ] **FARM-003-C5:** Criar `SellPoint` GameObject próximo à borda inferior do mapa

### [ART]
- [ ] **FARM-003-A1:** Criar tile `Tile_Plot_Empty_32x32.png` (canteiro vazio)
  - Terra escura: #4A2E1A, textura de solo com 3–4 pixels de variação de cor (#5A3A2A, #3A1E0E)
  - Sem outline externo — blend com o chão
  - Exportar para `Sprites/Tilesets/Farm/`

- [ ] **FARM-003-A2:** Criar tile `Tile_Water_32x32.png` (lago)
  - Azul médio: #2A6896, pixels de reflexo de luz: #4A88B6 (diagonal superior)
  - Animação de 2 frames (MVP: estático; preparar structure para V2)
  - Exportar para `Sprites/Tilesets/Farm/`

- [ ] **FARM-003-A3:** Criar tile `Tile_Ground_Farm_32x32.png` (chão caminhável — substituir placeholder)
  - Base #C8A464, variações #B89454, #D8B474 em pixels dispersos (textura de grama/terra)

- [ ] **FARM-003-A4:** Criar tile `Tile_Wall_Farm_32x32.png` (borda — substituir placeholder)
  - Pedra #5C4A32, textura irregular com pixels #4A3A22, #6C5A42

- [ ] **FARM-003-A5:** Criar sprite `SellPoint_Placeholder_32x32.png`
  - Caixinha amarela #F0C040 com símbolo "G" de ouro em marrom
  - Exportar para `Sprites/Placeholders/`

- [ ] **FARM-003-A6:** Importar todos e criar Tile assets no Unity

- [ ] **FARM-003-A7:** Pintar mapa completo da FarmScene no Tile Palette conforme mockup:

```
Layout do mapa (20x16 tiles):
┌────────────────────────────────────────┐
│ W  W  W  W  W  W  W  W  W  W  W  W   │ W = Wall (borda)
│ W  .  .  .  .  .  .  .  .  .  .  W   │ . = Ground (chão)
│ W  T  .  .  C  C  C  .  .  Lk .  W   │ T = Árvore (TreeSpot)
│ W  .  .  .  C  C  C  .  .  Lk .  W   │ C = Canteiro (PlotTile)
│ W  T  .  .  C  C  C  .  .  Lk .  W   │ Lk= Lago (Water tile)
│ W  .  .  .  .  .  .  .  .  .  .  W   │ F = FishingSpot
│ W  T  .  .  .  .  .  .  F  .  .  W   │ P = Player spawn
│ W  .  .  .  .  P  .  .  .  .  .  W   │ S = SellPoint
│ W  T  .  .  .  .  .  .  .  .  .  W   │
│ W  .  .  S  .  .  .  .  .  .  .  W   │
│ W  W  W  W  W  W  W  W  W  W  W  W   │
└────────────────────────────────────────┘
```

### [VFX]
- [ ] **FARM-003-V1:** Criar highlight de interação para objetos interagíveis próximos
  - Sprite overlay de outline amarelo #F0C040 2px sobre o objeto
  - Implementar em `InteractionSystem`: ao detectar IInteractable, ativa o highlight no componente
  - Criar `InteractionHighlight.cs`: recebe `Enable()/Disable()`, controla SpriteRenderer do overlay

### [TEST]
- [ ] **FARM-003-TEST1:** Todas as áreas visíveis e reconhecíveis
- [ ] **FARM-003-TEST2:** Lago não-caminhável (jogador colide com água)
- [ ] **FARM-003-TEST3:** 9 PlotTiles em grade 3x3 existem na hierarquia
- [ ] **FARM-003-TEST4:** 4 TreeSpots existem nas posições corretas
- [ ] **FARM-003-TEST5:** SellPoint visível próximo à borda

---

---

# SPEC FARM-011 — SeedDataSO e Assets de Sementes

## /speckit.specify

**O QUE:** ScriptableObject `SeedDataSO` definido e populado para as 2 sementes do MVP (Trigo e Cenoura). Inclui ícones 32x32 para cada semente e item colhido.

**POR QUE:** Toda a lógica de plantio depende desses dados. Sem os SOs, nada pode ser instanciado.

---

## /speckit.plan

**Assets a criar:**

| Asset | Tipo | Dados principais |
|---|---|---|
| `Seed_Trigo.asset` | SeedDataSO | GrowthDays=3, MinYield=3 |
| `Seed_Cenoura.asset` | SeedDataSO | GrowthDays=4, MinYield=2 |
| `Item_Semente_Trigo.asset` | ItemDataSO | MaxStack=20, BaseValue=2 |
| `Item_Semente_Cenoura.asset` | ItemDataSO | MaxStack=20, BaseValue=3 |
| `Item_Trigo.asset` | ItemDataSO | MaxStack=99, BaseValue=5, HungerRestore=20 |
| `Item_Cenoura.asset` | ItemDataSO | MaxStack=99, BaseValue=8, HungerRestore=25 |

---

## /speckit.tasks

### [CODE]
- [ ] **FARM-011-C1:** Implementar `SeedDataSO.cs` com campos:
  ```csharp
  public ItemDataSO SeedItem;
  public Sprite[] GrowthStageSprites; // [0]=vazio_plantado, [1]=pronto
  public ItemDataSO[] HarvestItems;
  public int[] HarvestAmounts;
  public int GrowthDays;
  public GrowthPeriod Period; // enum: Day, Night, Both
  public int MinYield;
  public int MaxYield;
  public float FertilizerYieldMultiplier;
  ```
- [ ] **FARM-011-C2:** Implementar `ItemDataSO.cs` com campos:
  ```csharp
  public string ItemId;
  public string DisplayName;
  [TextArea] public string Description;
  public Sprite Icon;
  public ItemCategory Category; // enum
  public int MaxStack;
  public int BaseValue;
  public int HungerRestore; // 0 se não for comida
  public bool IsEquippable;
  ```
- [ ] **FARM-011-C3:** Criar assets em `Data/Items/` e `Data/Seeds/` preenchidos

### [ART] — Ícones 32x32 (DALL-E 3 + retoque Aseprite)

**Processo para cada ícone:**
1. Prompt ChatGPT/DALL-E ou PixelLab: `"pixel art icon 32x32, [item], RPG game item, cozy farm RPG readability, original Cindar's Hope visual identity, warm orange-bronze palette, transparent background, clean silhouette, no antialiasing"`
2. Baixar PNG → abrir Aseprite → ajustar para 32x32
3. Aplicar paleta do projeto (#D4832A, #8B6914, etc.)
4. Adicionar outline 1px #0A0A0A
5. Fundo transparente
6. Exportar PNG

- [ ] **FARM-011-A1:** Criar `Item_SementeTrigo_32x32.png` — ícone de saquinho de sementes de trigo
- [ ] **FARM-011-A2:** Criar `Item_SementeCenoura_32x32.png` — ícone de saquinho de sementes laranjas
- [ ] **FARM-011-A3:** Criar `Item_Trigo_32x32.png` — feixe de trigo dourado
- [ ] **FARM-011-A4:** Criar `Item_Cenoura_32x32.png` — cenoura laranja com folhas verdes
- [ ] **FARM-011-A5:** Importar todos no Unity (Filter: Point, Compression: None, PPU: 32)
- [ ] **FARM-011-A6:** Referenciar cada sprite no campo `Icon` do ItemDataSO correspondente

### [ART] — Sprites de crescimento para CropTile

- [ ] **FARM-011-A7:** Criar `Crop_Trigo_Stage0_32x32.png` (plantado, não visível ainda)
  - Terra com pequeno broto: #4A6741 (2–3px de broto verde saindo do solo)

- [ ] **FARM-011-A8:** Criar `Crop_Trigo_Stage1_32x32.png` (pronto para colheita)
  - Hastes de trigo dourado (#D4A850) com espigas, 28px de altura, fundo transparente

- [ ] **FARM-011-A9:** Criar sprites equivalentes para Cenoura:
  - `Crop_Cenoura_Stage0_32x32.png` — broto laranja minúsculo
  - `Crop_Cenoura_Stage1_32x32.png` — folhas verdes com topo de cenoura laranja visível

- [ ] **FARM-011-A10:** Referenciar sprites no `SeedDataSO.GrowthStageSprites[]`

### [TEST]
- [ ] **FARM-011-TEST1:** Abrir Inspector dos 6 assets — todos os campos preenchidos, sem referência nula
- [ ] **FARM-011-TEST2:** Ícones aparecem corretos no Inspector (miniatura visível)
- [ ] **FARM-011-TEST3:** Sprites de crescimento têm 2 frames (Stage0 e Stage1) por semente

---

---

# SPEC FARM-012 — CropTile: Estado e Visual

## /speckit.specify

**O QUE:** Cada um dos 9 canteiros tem 3 estados visuais (vazio, crescendo, pronto) e muda de sprite corretamente.

**POR QUE:** O jogador precisa ver visualmente o estado de cada canteiro para saber quando colher.

---

## /speckit.plan

```
CropTile (GameObject)
├── SpriteRenderer           (Sorting: Ground, Order: 2)
├── BoxCollider2D (Trigger)  (28x28px)
├── CropTile.cs
└── InteractionHighlight     (filho: SpriteRenderer overlay amarelo)
    └── Highlight.cs
```

**Estados e sprites:**

| Estado | Sprite | Cor de fallback (placeholder) |
|---|---|---|
| Empty | `Tile_Plot_Empty_32x32` | #4A2E1A (terra) |
| Growing (Stage0) | `Crop_[Seed]_Stage0_32x32` | #2A4A1A (verde-escuro) |
| Ready | `Crop_[Seed]_Stage1_32x32` | #4A8A1A (verde-claro) + brilho |

---

## /speckit.tasks

### [CODE]
- [ ] **FARM-012-C1:** Implementar `CropTile.cs`:
  ```csharp
  public enum PlotState { Empty, Growing, Ready }
  public PlotState State { get; private set; }
  public SeedDataSO CurrentSeed { get; private set; }
  public int DaysGrown { get; private set; }

  public bool Plant(SeedDataSO seed) { ... }
  public void AdvanceGrowth() { ... }  // chamado pelo FarmSystem
  public List<ItemDataSO> Harvest() { ... }
  ```
- [ ] **FARM-012-C2:** `Plant()`: valida canteiro vazio → seta `CurrentSeed`, `State = Growing`, `DaysGrown = 0` → atualiza sprite
- [ ] **FARM-012-C3:** `AdvanceGrowth()`: incrementa `DaysGrown` → se `>= CurrentSeed.GrowthDays` → `State = Ready` → atualiza sprite → publica `CropReadyEvent { TilePosition }`
- [ ] **FARM-012-C4:** `Harvest()`: retorna lista de itens (MinYield de cada HarvestItem) → reset estado → sprite vazio → publica `CropHarvestedEvent`
- [ ] **FARM-012-C5:** `UpdateSprite()`: `_renderer.sprite = State == Empty ? emptySprite : CurrentSeed.GrowthStageSprites[stageIndex]`
- [ ] **FARM-012-C6:** Implementar `IInteractable.Interact(PlayerController)`:
  - Se `Empty`: abre `PlantingMenu`
  - Se `Ready`: chama `Harvest()`, itens vão para `InventoryManager`
  - Se `Growing`: exibe mensagem "Ainda crescendo... (X dias)"

### [ART]
- [ ] **FARM-012-A1:** Criar sprite overlay de highlight para canteiro pronto:
  - `Crop_ReadyIndicator_32x32.png`
  - Estrelinhas douradas #F0C040 nos cantos do tile (4x4px cada)
  - Fundo transparente
  - Este sprite fica ativo quando `State == Ready`

- [ ] **FARM-012-A2:** Criar sprite de canteiro selecionado (hover/proximidade):
  - `Plot_Selected_32x32.png`
  - Outline amarelo #F0C040 2px ao redor do tile
  - Fundo transparente (overlay sobre o tile atual)

### [VFX]
- [ ] **FARM-012-V1:** Ao colher (`Harvest()`): partículas de folha saindo do canteiro
  - Criar `VFX_Harvest.prefab` em `Prefabs/VFX/`
  - Particle System 2D: 6–8 partículas, sprites de folha 8x8px verde
  - Duração: 0.5s, burst único, espalhamento radial 45°
  - Sorting Layer: Items (acima do chão)

- [ ] **FARM-012-V2:** Criar sprite de partícula: `VFX_Leaf_8x8.png` — folha verde simples #4A8A1A

- [ ] **FARM-012-V3:** Ao plantar (`Plant()`): pequeno efeito de terra sendo perturbada
  - `VFX_Plant.prefab`: 4 partículas de terra marrom #6B3A2A, burst 0.3s

### [ANIM]
- [ ] **FARM-012-AN1:** Criar AnimationClip `Crop_Ready_Pulse`:
  - 4 frames, duração total 0.8s, loop
  - Frames 0 e 2: escala 1.0, frames 1 e 3: escala 1.05
  - Efeito de "pulsação" suave indicando que está pronto
  - Atribuir ao CropTile quando `State == Ready`

### [TEST]
- [ ] **FARM-012-TEST1:** Canteiro vazio → sprite de terra → sem highlight
- [ ] **FARM-012-TEST2:** Após plantar → sprite Stage0 correto para a semente
- [ ] **FARM-012-TEST3:** Após GrowthDays → sprite Stage1 + pulsação ativa
- [ ] **FARM-012-TEST4:** Após colheita → sprite de terra vazio → sem pulsação
- [ ] **FARM-012-TEST5:** VFX de colheita dispara e desaparece em 0.5s

---

---

# SPEC FARM-013 — Crescimento de Plantas (FarmSystem)

## /speckit.specify

**O QUE:** `FarmSystem` ouve `DayStartedEvent` e chama `AdvanceGrowth()` em todos os canteiros ativos.

**POR QUE:** O crescimento das plantas é o loop de progressão central. Sem ele, plantar é inútil.

---

## /speckit.plan

```
FarmSystem.cs
├── Ouve: DayStartedEvent
├── Mantém: List<CropTile> _activePlots
└── A cada dia: itera _activePlots, chama AdvanceGrowth()
```

---

## /speckit.tasks

### [CODE]
- [ ] **FARM-013-C1:** Implementar `FarmSystem.cs`:
  - `void OnEnable()` → `GameEventBus.Subscribe<DayStartedEvent>(OnDayStarted)`
  - `void OnDisable()` → `GameEventBus.Unsubscribe<DayStartedEvent>(OnDayStarted)`
  - `void OnDayStarted(DayStartedEvent e)` → foreach plot: `plot.AdvanceGrowth()`
- [ ] **FARM-013-C2:** `FarmSystem` recebe `CropTile[] _plots` via `[SerializeField]` no Inspector ou por `FarmSceneInstaller` gerado em Editor-time. Não usar `FindObjectsByType`, `GameObject.Find`, `FindObjectOfType` ou busca global em runtime.
- [ ] **FARM-013-C3:** Expor `PlotSaveData[] GenerateSaveData()` e `void LoadFromSave(PlotSaveData[])` para o SaveManager

### [TEST]
- [ ] **FARM-013-TEST1:** Plantar Trigo → TAB 3x → canteiro fica Ready (GrowthDays=3)
- [ ] **FARM-013-TEST2:** Plantar Cenoura → TAB 4x → canteiro fica Ready (GrowthDays=4)
- [ ] **FARM-013-TEST3:** Log do Console mostra "DayStartedEvent recebido, X plots avançados"

---

---

# SPEC FARM-014 — InteractionSystem e IInteractable

## /speckit.specify

**O QUE:** Sistema centralizado que detecta tecla E e delega para o objeto interagível mais próximo. Mostra indicador visual de qual objeto está selecionado.

**POR QUE:** Sem isso, o jogador não consegue plantar, colher, pescar, cortar árvores ou vender.

---

## /speckit.plan

```
InteractionSystem.cs (componente no Player)
├── Input: tecla E (via PlayerInputActions.Interact)
├── Detecção: Physics2D.OverlapCircleAll(radius: 1.5f) a cada frame
├── Filtro: apenas IInteractable
├── Seleção: mais próximo do player
├── Display: chama EnableHighlight() no selecionado, DisableHighlight() nos outros
└── Trigger: tecla E → selected.Interact(playerController)
```

---

## /speckit.tasks

### [CODE]
- [ ] **FARM-014-C1:** Criar interface `IInteractable`:
  ```csharp
  public interface IInteractable {
      void Interact(PlayerController player);
      void EnableHighlight();
      void DisableHighlight();
      string GetInteractionHint(); // ex: "E: Plantar", "E: Colher", "E: Pescar"
  }
  ```
- [ ] **FARM-014-C2:** Implementar `InteractionSystem.cs`:
  - Campo `LayerMask InteractableLayer`
  - A cada frame: `OverlapCircleAll` → filtrar `IInteractable` → selecionar mais próximo
  - Ao mudar de selecionado: `DisableHighlight()` no anterior, `EnableHighlight()` no novo
  - Ao pressionar E: `_selected?.Interact(_playerController)`
- [ ] **FARM-014-C3:** `CropTile` implementa `IInteractable` (ver FARM-012-C6)

### [UI]
- [ ] **FARM-014-U1:** Criar `InteractionHintUI.cs` — TextMeshPro no HUD mostrando `GetInteractionHint()` do objeto selecionado
  - Posição: centro-baixo da tela, sobre o HUD
  - Desaparece quando nenhum objeto está selecionado
  - Formato: `[E] Plantar` com ícone de tecla

- [ ] **FARM-014-U2:** Criar sprite de tecla para hint: `UI_KeyE_24x24.png`
  - Tecla quadrada cinza #AAAAAA, letra "E" branca centralizada
  - Exportar para `Sprites/UI/`

### [TEST]
- [ ] **FARM-014-TEST1:** Chegar perto de CropTile → highlight ativa → hint "E: Plantar" aparece
- [ ] **FARM-014-TEST2:** Se afastar → highlight desativa → hint some
- [ ] **FARM-014-TEST3:** Dois objetos próximos → selecionado é o mais perto do player
- [ ] **FARM-014-TEST4:** Pressionar E → Interact() é chamado no selecionado

---

---

# SPEC FARM-015 — Colheita

## /speckit.specify

**O QUE:** Pressionar E num canteiro `Ready` colhe a planta: itens vão para inventário, canteiro volta ao estado Empty.

**POR QUE:** Fechar o loop de plantio é o objetivo central do MVP.

---

## /speckit.tasks

### [CODE]
- [ ] **FARM-015-C1:** Em `CropTile.Interact()` quando `State == Ready`: chamar `Harvest()`
- [ ] **FARM-015-C2:** `Harvest()` retorna `List<(ItemDataSO item, int amount)>` com MinYield de cada HarvestItem
- [ ] **FARM-015-C3:** `InteractionSystem` passa resultado para `InventoryManager.AddItem()`
- [ ] **FARM-015-C4:** Publicar `CropHarvestedEvent { string SeedId, int TotalYield, Vector2 Position }`

### [VFX]
- [ ] **FARM-015-V1:** Instanciar `VFX_Harvest.prefab` na posição do canteiro ao colher (criado em FARM-012-V1)
- [ ] **FARM-015-V2:** Criar `ItemPopup.prefab` — texto flutuante "+3 Trigo" que sobe e desaparece em 1s
  - TextMeshPro branco #FFFFFF com outline preto
  - Animação: move Y +1 unidade em 1s, Alpha de 1→0

### [UI]
- [ ] **FARM-015-U1:** Popup de colheita instanciado sobre o canteiro com o texto do item + quantidade

### [TEST]
- [ ] **FARM-015-TEST1:** Colher Trigo → "+3 Trigo" aparece → inventário tem 3 Trigos a mais
- [ ] **FARM-015-TEST2:** Canteiro volta para estado Empty após colheita
- [ ] **FARM-015-TEST3:** VFX de partículas dispara na colheita
- [ ] **FARM-015-TEST4:** Popup flutuante aparece e desaparece em 1s

---

---

# SPEC FARM-016 — Menu de Plantio

## /speckit.specify

**O QUE:** Ao pressionar E num canteiro vazio, abre menu de texto listando sementes disponíveis no inventário. Jogador seleciona com número (1, 2, 3...) ou fecha com ESC.

**POR QUE:** Interface para o jogador escolher o que plantar.

---

## /speckit.tasks

### [CODE]
- [ ] **FARM-016-C1:** Implementar `PlantingMenu.cs`:
  - `Open(CropTile targetPlot)`: consulta `InventoryManager` por itens com `Category == Seed` → popula lista
  - Ao selecionar (tecla numérica): `InventoryManager.RemoveItem(seed, 1)` → `targetPlot.Plant(seedData)` → fecha menu
  - ESC: fecha sem plantar
  - Desativa `PlayerController` enquanto menu está aberto (jogador não se move)

### [UI]
- [ ] **FARM-016-U1:** Criar `PlantingMenu` Canvas (World Space ou Screen Space — Screen Space Overlay)
  - Painel: fundo #1A1A1A com opacidade 85%, bordas #8B6914 (bronze), cantos arredondados 4px
  - Título: "O QUE PLANTAR?" em fonte pixel (ou TextMeshPro com fonte bitmap)
  - Lista: cada linha = ícone 16x16 + nome + quantidade + dias de crescimento
  - Rodapé: "[ESC] Cancelar"

- [ ] **FARM-016-U2:** Criar `UI_MenuPanel_9slice.png` — 9-slice panel para reutilizar em outros menus
  - 48x48px total, borda 6px, centro transparente
  - Cores: #1A1A1A, borda #8B6914

**Mockup do menu:**
```
╔═══════════════════════════════════╗  ← borda bronze #8B6914
║     🌾 O QUE PLANTAR? 🌾         ║
╠═══════════════════════════════════╣
║  [ícone] 1. Semente Trigo    x5   ║  ← crescimento: 3 dias
║          ▸ Cresce em 3 dias       ║
║  [ícone] 2. Semente Cenoura  x3   ║
║          ▸ Cresce em 4 dias       ║
╠═══════════════════════════════════╣
║  Pressione [1-9] para plantar     ║
║  [ESC] Cancelar                   ║
╚═══════════════════════════════════╝
```

### [TEST]
- [ ] **FARM-016-TEST1:** E em canteiro vazio com sementes → menu abre
- [ ] **FARM-016-TEST2:** E em canteiro vazio sem sementes → mensagem "Sem sementes no inventário"
- [ ] **FARM-016-TEST3:** Selecionar 1 → semente plantada → menu fecha → jogador se move
- [ ] **FARM-016-TEST4:** ESC → menu fecha sem plantar → semente não consumida
- [ ] **FARM-016-TEST5:** Jogador não se move enquanto menu está aberto

---

---

# SPEC FARM-021 — InventoryManager

## /speckit.specify

**O QUE:** Sistema central que gerencia todos os itens do jogador: adicionar, remover, verificar, stack automático.

**POR QUE:** Todos os outros sistemas dependem do inventário para dar e receber itens.

---

## /speckit.tasks

### [CODE]
- [ ] **FARM-021-C1:** Implementar `InventorySlot.cs`:
  ```csharp
  [Serializable] public class InventorySlot {
      public ItemDataSO Item;
      public int Amount;
      public bool IsEmpty => Item == null || Amount == 0;
  }
  ```
- [ ] **FARM-021-C2:** Implementar `InventoryManager.cs` (DontDestroyOnLoad):
  - `InventorySlot[] Slots = new InventorySlot[20]`
  - `bool AddItem(ItemDataSO item, int amount)` → stack automático → novo slot se necessário → false se cheio
  - `bool RemoveItem(ItemDataSO item, int amount)` → false se insuficiente
  - `bool HasItem(ItemDataSO item, int amount)` → verifica sem remover
  - `int GetAmount(ItemDataSO item)` → quantidade total
  - Publicar `ItemPickedUpEvent { ItemDataSO Item, int Amount }` ao adicionar
- [ ] **FARM-021-C3:** Itens iniciais lidos de `PlayerDataSO.StartingItems[]`: Semente Trigo x5, Semente Cenoura x3, Cana Básica x1
- [ ] **FARM-021-C4:** Campo `int Gold` em `PlayerManager` (não ocupa slot de inventário)
- [ ] **FARM-021-C5:** Expor `InventorySlotData[] GenerateSaveData()` e `void LoadFromSave(InventorySlotData[])` para SaveManager

### [TEST]
- [ ] **FARM-021-TEST1:** Iniciar jogo → inventário tem itens iniciais corretos
- [ ] **FARM-021-TEST2:** Adicionar 5 Trigos + adicionar mais 3 → slot único com 8
- [ ] **FARM-021-TEST3:** Remover 10 quando tem 5 → retorna false, inventário não muda
- [ ] **FARM-021-TEST4:** 20 slots cheios → AddItem retorna false

---

---

# SPEC FARM-022 — InventoryUI (MVP: lista texto)

## /speckit.specify

**O QUE:** Janela de inventário acessível com I, lista itens em texto com ícone 16x16, mostra ouro, permite usar item com U.

**POR QUE:** Jogador precisa ver o que tem e usar itens (sementes, comida).

---

## /speckit.tasks

### [CODE]
- [ ] **FARM-022-C1:** Implementar `InventoryUI.cs`:
  - Toggle com tecla I
  - Desativa `PlayerController` e `InteractionSystem` enquanto aberto
  - Popula lista de `InventoryManager.Slots` (só slots não-vazios)
  - Seleção com teclas numéricas 1–9
  - Tecla U → usa item selecionado (`InventoryManager.UseItem()`)
  - ESC → fecha
- [ ] **FARM-022-C2:** Implementar `InventoryManager.UseItem(InventorySlot slot)`:
  - Se `Category == Seed`: inicia fluxo de plantio (sinaliza `InteractionSystem`)
  - Se `HungerRestore > 0`: chama `HungerSystem.Restore(item.HungerRestore)`, remove 1 do slot
  - Outros usos: a implementar nas respectivas features

### [UI]
- [ ] **FARM-022-U1:** Criar painel `InventoryPanel` (Canvas Screen Space Overlay):
  - 320x400px, canto superior direito
  - Fundo: `UI_MenuPanel_9slice` (criado em FARM-016-U2)
  - Título: "INVENTÁRIO" + tecla I para fechar

- [ ] **FARM-022-U2:** Layout da lista:
  - Linha de ouro: ícone de moeda + "50 G" em dourado #F0C040
  - Separador horizontal
  - Cada slot: ícone 16x16 + número de seleção + nome + quantidade
  - Item selecionado: fundo #3A2A0A (destaque marrom)
  - Rodapé: "[U] Usar · [ESC] Fechar"

- [ ] **FARM-022-U3:** Criar sprite `UI_GoldCoin_16x16.png` — moeda dourada simples
  - Círculo #F0C040 com "G" marrom #8B6914 no centro, outline 1px
  - Exportar para `Sprites/UI/`

**Mockup:**
```
┌─────────────────────────────────┐
│ ▸ INVENTÁRIO              [I]   │
├─────────────────────────────────┤
│ 🪙 50 G                         │
├─────────────────────────────────┤
│ 1. [🌾] Semente Trigo      x5  │
│ 2. [🥕] Semente Cenoura    x3  │
│ 3. [🌾] Trigo              x6  │ ← selecionado (fundo destaque)
│ 4. [🪵] Madeira           x10  │
├─────────────────────────────────┤
│ [U] Usar  [ESC] Fechar          │
└─────────────────────────────────┘
```

### [TEST]
- [ ] **FARM-022-TEST1:** I → painel abre, itens listados corretamente
- [ ] **FARM-022-TEST2:** Itens com stack de 0 não aparecem
- [ ] **FARM-022-TEST3:** Selecionar item de comida → U → fome restaura → quantidade diminui
- [ ] **FARM-022-TEST4:** I ou ESC → fecha → jogador volta a se mover

---

---

# SPEC FARM-031 — TimeManager MVP

## /speckit.specify

**O QUE:** TimeManager controla o dia atual. TAB avança o dia, publica `DayStartedEvent`, aciona save.

**POR QUE:** Tempo é a mecânica que faz as plantas crescerem.

---

## /speckit.tasks

### [CODE]
- [ ] **FARM-031-C1:** Implementar `TimeManager.cs` (DontDestroyOnLoad):
  ```csharp
  public int CurrentDay { get; private set; } = 1;
  public void AdvanceDay() {
      CurrentDay++;
      GameEventBus.Publish(new DayStartedEvent { DayNumber = CurrentDay });
      _saveManager.Save(); // referência via [SerializeField]
  }
  ```
- [ ] **FARM-031-C2:** Criar `DayStartedEvent.cs` em `Scripts/Core/Events/`: `public int DayNumber;`
- [ ] **FARM-031-C3:** Detectar tecla TAB via `PlayerInputActions.Sleep` → chamar `AdvanceDay()`
- [ ] **FARM-031-C4:** Criar estrutura para V2 (sem ativar): campos `int CurrentHour`, `MoonType ActiveMoon`, `Season CurrentSeason` — apenas declarados

### [UI]
- [ ] **FARM-031-U1:** Feedback visual ao avançar o dia: tela escurece e clareia (fade preto 0→1→0 em 0.5s)
  - Implementar `DayTransitionUI.cs`: painel preto fullscreen, ativa via coroutine ao `DayStartedEvent`

### [TEST]
- [ ] **FARM-031-TEST1:** TAB → `CurrentDay` incrementa → DayStartedEvent publicado → plantas avançam → save criado
- [ ] **FARM-031-TEST2:** Fade visual de transição de dia funciona (0.5s)

---

---

# SPEC FARM-032 — HUD: Dia e Ouro

## /speckit.specify

**O QUE:** HUD permanente no canto superior da tela mostrando "Dia X" e "50 G".

**POR QUE:** Informação contextual básica que o jogador precisa ver o tempo todo.

---

## /speckit.tasks

### [CODE]
- [ ] **FARM-032-C1:** Implementar `TimeUI.cs`: ouve `DayStartedEvent`, atualiza TextMeshPro
- [ ] **FARM-032-C2:** Implementar `GoldUI.cs`: ouve `GoldChangedEvent`, atualiza TextMeshPro

### [UI]
- [ ] **FARM-032-U1:** Criar `HUD_Canvas` (Screen Space Overlay, Sort Order 10):
  - `HUD_TopBar`: faixa horizontal topo, 320x32px, fundo #1A1A1A 70% opacidade
  - Esquerda: ícone sol + "Dia 1" (TextMeshPro, branco, fonte pixel)
  - Direita: ícone moeda + "50 G" (TextMeshPro, dourado #F0C040)

- [ ] **FARM-032-U2:** Criar sprite `UI_SunIcon_16x16.png` — sol simples amarelo #F0C040 com raios
- [ ] **FARM-032-U3:** [V2] Slot para ícone de lua e hora — criar mas deixar invisível

**Mockup do HUD topo:**
```
┌──────────────────────────────────────────────────────┐
│ [☀] Dia 1                              [🪙] 50 G    │  ← HUD_TopBar
└──────────────────────────────────────────────────────┘
```

### [TEST]
- [ ] **FARM-032-TEST1:** HUD visível em cima do gameplay
- [ ] **FARM-032-TEST2:** TAB → "Dia 2" aparece com transição suave
- [ ] **FARM-032-TEST3:** Vender item → ouro atualiza no HUD

---

---

# SPEC FARM-041 — TreeDataSO e Assets de Árvore

## /speckit.specify

**O QUE:** ScriptableObject `TreeDataSO` definido. Sprites dos 4 níveis de corte criados. Assets configurados.

---

## /speckit.tasks

### [CODE]
- [ ] **FARM-041-C1:** Implementar `TreeDataSO.cs`:
  ```csharp
  public string TreeId;
  public Sprite[] ChopLevelSprites; // [0]=toco, [1]=pequena, [2]=média, [3]=cheia
  public ItemDataSO WoodItem;
  public int DaysToGrow = 5;
  public int MaxChopLevels = 4;
  public int[] WoodPerChop = {10, 10, 10, 10}; // MVP: sempre 10
  ```
- [ ] **FARM-041-C2:** Criar `ItemDataSO` para `Item_Madeira`: MaxStack=99, BaseValue=3
- [ ] **FARM-041-C3:** Criar asset `Data/Farm/Tree_Carvalho.asset`

### [ART]
- [ ] **FARM-041-A1:** Criar `Tree_Carvalho_Level3_32x64.png` (árvore cheia, 2 tiles de altura)
  - Tronco: marrom #5C3D1E, 10px de largura, 24px de altura (parte inferior)
  - Copa: verde #4A6741 com variações, oval 28x28px (parte superior)
  - Outline 1px #0A0A0A
  - Sorting: TreeTops para a copa, Characters para o tronco

- [ ] **FARM-041-A2:** Criar `Tree_Carvalho_Level2_32x64.png` (copa menor, 22x22px)
- [ ] **FARM-041-A3:** Criar `Tree_Carvalho_Level1_32x64.png` (copa mínima, 14x14px)
- [ ] **FARM-041-A4:** Criar `Tree_Carvalho_Level0_32x32.png` (só toco, 32x32px)
  - Toco de madeira #8B6914, serragem #C8A464 em cima

- [ ] **FARM-041-A5:** Criar `Item_Madeira_32x32.png` — tronco de madeira empilhado, tons de marrom
- [ ] **FARM-041-A6:** Importar todos, configurar PPU=32, Filter=Point, Compression=None
- [ ] **FARM-041-A7:** Referenciar sprites em `TreeDataSO.ChopLevelSprites[]`

### [TEST]
- [ ] **FARM-041-TEST1:** Inspector do `Tree_Carvalho.asset` — 4 sprites referenciados, sem null
- [ ] **FARM-041-TEST2:** Sprites visíveis em tamanho correto no Inspector

---

---

# SPEC FARM-042 — TreeObject: Corte e Visual

## /speckit.specify

**O QUE:** Cada árvore é um objeto com 4 níveis de corte. E → recebe 10 madeiras, sprite muda. Ao nível 0, árvore desaparece.

---

## /speckit.tasks

### [CODE]
- [ ] **FARM-042-C1:** Implementar `TreeObject.cs`:
  - `[SerializeField] TreeDataSO _data`
  - `int _currentLevel = 3` (0-indexed: nível 3 = árvore cheia)
  - Implementa `IInteractable`
  - `Interact()` → `Chop()`
  - `Chop()` → `InventoryManager.AddItem(WoodItem, WoodPerChop[_currentLevel])` → decrementa nível → atualiza sprite → se nível < 0: `gameObject.SetActive(false)` → publica `TreeChoppedEvent`
- [ ] **FARM-042-C2:** `GetInteractionHint()` → `"E: Cortar árvore (+10 madeira)"`
- [ ] **FARM-042-C3:** Expor `TreeSaveData GenerateSaveData()` e `void LoadFromSave(TreeSaveData)`

### [VFX]
- [ ] **FARM-042-V1:** Criar `VFX_ChopTree.prefab`:
  - 6–8 partículas de lascas de madeira marrom #8B6914
  - Sprites: `VFX_WoodChip_8x4.png` — lasca retangular marrom
  - Duração 0.6s, burst, espalhamento 180° lateral
- [ ] **FARM-042-V2:** Criar `VFX_WoodChip_8x4.png` no Aseprite — retângulo irregular marrom

### [ANIM]
- [ ] **FARM-042-AN1:** Animação de "shake" ao cortar:
  - `Tree_Chop_Shake`: 6 frames, 0.3s total, deslocamento X de 0→2→-2→1→-1→0px
  - Criar AnimatorController `Tree_AC.controller` com state `Idle` e trigger `OnChop`

### [UI]
- [ ] **FARM-042-U1:** Popup flutuante ao cortar: "+10 Madeira" (reusar `ItemPopup.prefab` de FARM-015-V2)

### [TEST]
- [ ] **FARM-042-TEST1:** E na árvore → shake → "+10 Madeira" → 10 madeiras no inventário
- [ ] **FARM-042-TEST2:** Sprite muda a cada corte (4→3→2→1→desaparece)
- [ ] **FARM-042-TEST3:** 4 cortes → árvore some → espaço livre no mapa
- [ ] **FARM-042-TEST4:** VFX de lascas dispara a cada corte

---

---

# SPEC FARM-051 — Lago: Tilemap e Visual

## /speckit.specify

**O QUE:** Área de lago com tiles de água não-caminháveis, borda caminhável com FishingSpots.

---

## /speckit.tasks

### [ART]
- [ ] **FARM-051-A1:** Criar `Tile_Water_32x32.png` (se ainda não feito em FARM-003-A2):
  - Azul #2A6896, pixels de luz #4A88B6 no canto superior
  - 2 variações de tile para reduzir repetição: `Tile_Water_A` e `Tile_Water_B`

- [ ] **FARM-051-A2:** Criar `Tile_WaterEdge_32x32.png` — borda de água (terra encontra água)
  - Transição gradual terra→água, 4 variações (N, S, E, O)

- [ ] **FARM-051-A3:** Criar animação de água MVP (2 frames alternando `Tile_Water_A/B` a 0.5s)
  - Implementar `AnimatedTile` do Unity 2D Extras para o lago

- [ ] **FARM-051-A4:** Criar sprite `FishingSpot_Indicator_32x32.png` — bóia vermelha flutuando
  - Círculo vermelho #CC3333, 12px, no tile de borda de lago
  - Indica onde o jogador pode pescar

### [CODE]
- [ ] **FARM-051-C1:** Criar `Water_Tilemap` com `TilemapCollider2D` + `CompositeCollider2D`
- [ ] **FARM-051-C2:** Pintar lago com `AnimatedTile` de água
- [ ] **FARM-051-C3:** Criar 3 `FishingSpot` GameObjects na borda do lago com `FishingSpot_Indicator`

### [TEST]
- [ ] **FARM-051-TEST1:** Jogador não atravessa tiles de água
- [ ] **FARM-051-TEST2:** Tiles de água animam (alternância visível)
- [ ] **FARM-051-TEST3:** Bóias visíveis nos FishingSpots

---

---

# SPEC FARM-052 — FishingSpot: Pesca Básica

## /speckit.specify

**O QUE:** E num FishingSpot → espera 3s → Peixe Comum no inventário. Requer cana de pescar.

---

## /speckit.tasks

### [CODE]
- [ ] **FARM-052-C1:** Criar `ItemDataSO` para `Item_CanaBasica` e `Item_PeixeComum`
  - PeixeComum: MaxStack=20, BaseValue=12, HungerRestore=35
- [ ] **FARM-052-C2:** Adicionar `Item_CanaBasica x1` nos `PlayerDataSO.StartingItems`
- [ ] **FARM-052-C3:** Implementar `FishingSpot.cs` implementando `IInteractable`:
  - `Interact()`: verifica `HasItem(CanaBasica)` → inicia Coroutine `FishingRoutine()`
  - Coroutine: desativa player movement → mostra "Pescando..." 3s → `AddItem(PeixeComum, 1)` → publica `FishCaughtEvent` → reativa movement
  - `GetInteractionHint()` → `"E: Pescar"`

### [ANIM]
- [ ] **FARM-052-AN1:** Criar animação do jogador pescando `Player_Fishing`:
  - 4 frames, 0.8s loop, braços estendidos para frente
  - Placeholder MVP: sprite estático com linha de pesca (linha branca de 8px saindo do sprite)
  - Criar clip `Player_Fishing_Placeholder`: sprite Player + linha extensão

### [VFX]
- [ ] **FARM-052-V1:** Criar `VFX_Fishing_Splash.prefab` — pequeno splash de água ao pegar o peixe
  - 4 partículas azuis #4A88B6, burst 0.3s, espalhamento 360°
- [ ] **FARM-052-V2:** Criar `VFX_WaterDrop_8x8.png` — gota d'água azul

### [UI]
- [ ] **FARM-052-U1:** Label "Pescando..." aparece sobre o jogador durante os 3s
  - TextMeshPro com fundo semi-transparente, pisca a cada 0.5s (ellipsis animado: ".", "..", "...")
- [ ] **FARM-052-U2:** Popup "+1 Peixe Comum" ao capturar (reusar `ItemPopup.prefab`)

### [TEST]
- [ ] **FARM-052-TEST1:** E no FishingSpot com cana → animação → 3s → peixe no inventário
- [ ] **FARM-052-TEST2:** E sem cana → mensagem "Você precisa de uma cana de pescar"
- [ ] **FARM-052-TEST3:** Jogador não se move durante a pesca
- [ ] **FARM-052-TEST4:** VFX de splash ao capturar

---

---

# SPEC FARM-061 — HungerSystem

## /speckit.specify

**O QUE:** Barra de fome 0–100 que drena por passo. Penalidades progressivas. Respawn ao morrer de fome.

---

## /speckit.tasks

### [CODE]
- [ ] **FARM-061-C1:** Criar eventos: `HungerChangedEvent { float Current, float Max }`, `HungerCriticalEvent`, `HungerEmptyEvent`
- [ ] **FARM-061-C2:** Implementar `HungerSystem.cs`:
  - `float _hunger = 100f`, `float _maxHunger = 100f`
  - Ouve `PlayerStepEvent` → a cada 10 steps: `_hunger -= 1` → publica `HungerChangedEvent`
  - Ao `_hunger <= 30`: publica `HungerCriticalEvent` (1x, não repetir até recuperar)
  - Ao `_hunger <= 0`: publica `HungerEmptyEvent`, inicia Coroutine de dano passivo
  - Dano passivo: 1 HP a cada 5s enquanto hunger == 0
  - `void Restore(float amount)`: `_hunger = Mathf.Min(_hunger + amount, _maxHunger)` → cancela dano passivo se hunger > 0 → publica `HungerChangedEvent`
- [ ] **FARM-061-C3:** `PlayerController` ouve `HungerCriticalEvent` → `SpeedMultiplier = 0.7f`; ouve `HungerChangedEvent` onde `Current > 30` → `SpeedMultiplier = 1f`
- [ ] **FARM-061-C4:** `PlayerManager` ouve `HungerEmptyEvent` → inicia dano; ao HP = 0: `Die()`
- [ ] **FARM-061-C5:** `PlayerManager.Die()`: fome = 50, HP = 100, posição = Vector2 spawn, publica `PlayerRespawnedEvent`

### [TEST]
- [ ] **FARM-061-TEST1:** Andar 10 passos → fome cai 1
- [ ] **FARM-061-TEST2:** Fome ≤ 30 → jogador fica mais lento (visível)
- [ ] **FARM-061-TEST3:** Fome = 0 → HP começa a cair lentamente
- [ ] **FARM-061-TEST4:** HP = 0 → spawn no ponto inicial, fome = 50, HP = 100
- [ ] **FARM-061-TEST5:** Comer com fome em 0 → HP para de cair

---

---

# SPEC FARM-062 — HUD: HP e Fome

## /speckit.specify

**O QUE:** Barras de HP e Fome visíveis no HUD, mudam de cor conforme nível crítico.

---

## /speckit.tasks

### [UI]
- [ ] **FARM-062-U1:** Adicionar ao `HUD_Canvas`:
  - `HUD_BottomLeft`: barras de HP e Fome, canto inferior esquerdo
  - HP bar: 120x12px, cor #CC3333 (vermelho), ícone coração 16x16px à esquerda
  - Fome bar: 120x12px, cor #CC8833 (laranja), ícone pão 16x16px à esquerda
  - Fundo das barras: #222222
  - Borda: 1px #444444

- [ ] **FARM-062-U2:** Criar sprites de ícone:
  - `UI_Heart_16x16.png` — coração vermelho simples #CC3333
  - `UI_Bread_16x16.png` — fatia de pão dourada #D4A850

- [ ] **FARM-062-U3:** Implementar `HungerUI.cs`:
  - Ouve `HungerChangedEvent` → atualiza `fillAmount` da barra
  - Fome > 30: cor #CC8833 (laranja)
  - Fome ≤ 30: cor #CC3333 (vermelho), ícone pisca a 1Hz

- [ ] **FARM-062-U4:** Implementar `HPUI.cs`:
  - Ouve `PlayerManager.OnHPChanged` → atualiza barra
  - HP > 50%: cor #33CC33 (verde)
  - HP ≤ 50%: cor #CCCC33 (amarelo)
  - HP ≤ 25%: cor #CC3333 (vermelho)

**Mockup HUD completo:**
```
┌──────────────────────────────────────────────────────┐
│ [☀] Dia 1                              [🪙] 50 G    │
│                                                      │
│                   [gameplay]                         │
│                                                      │
│ [❤] ████████████████████  100/100                   │
│ [🍞] ████████████████████  100/100                   │
└──────────────────────────────────────────────────────┘
```

### [TEST]
- [ ] **FARM-062-TEST1:** HUD visível com barras cheias ao iniciar
- [ ] **FARM-062-TEST2:** Andar muito → barra de fome diminui visivelmente
- [ ] **FARM-062-TEST3:** Fome ≤ 30 → barra fica vermelha + ícone pisca
- [ ] **FARM-062-TEST4:** Tomar dano de fome → barra de HP diminui

---

---

# SPEC FARM-071 — SaveManager

## /speckit.specify

**O QUE:** Salva estado do jogo em JSON local ao avançar o dia (TAB). Mostra confirmação visual.

---

## /speckit.tasks

### [CODE]
- [ ] **FARM-071-C1:** Criar classes serializáveis:
  ```csharp
  [Serializable] public class SaveData {
      public int CurrentDay;
      public PlayerSaveData Player;
      public FarmSaveData Farm;
      public InventorySlotData[] Inventory;
  }
  [Serializable] public class PlayerSaveData { public int HP; public float Hunger; public int Gold; }
  [Serializable] public class InventorySlotData { public string ItemId; public int Amount; }
  ```
- [ ] **FARM-071-C2:** Implementar `SaveManager.cs` (DontDestroyOnLoad):
  - `string SavePath = Application.persistentDataPath + "/saves/slot_1.json"`
  - `void Save()`: coleta dados de todos os sistemas → `JsonUtility.ToJson` → `File.WriteAllText`
  - `SaveData Load()`: `File.ReadAllText` → `JsonUtility.FromJson`
  - `bool HasSave()`: `File.Exists(SavePath)`
- [ ] **FARM-071-C3:** Criar diretório de saves se não existir (`Directory.CreateDirectory`)
- [ ] **FARM-071-C4:** `TimeManager.AdvanceDay()` chama `SaveManager.Save()` após publicar evento
- [ ] **FARM-071-C5:** `SaveManager.Save()` coleta de: `TimeManager`, `PlayerManager`, `InventoryManager`, `FarmSystem`

### [UI]
- [ ] **FARM-071-U1:** Criar `SaveNotificationUI.cs`:
  - Texto "✓ Jogo salvo" no canto inferior direito
  - Aparece por 2s com fade-out
  - Fonte branca, ícone de disquete ou check

### [TEST]
- [ ] **FARM-071-TEST1:** TAB → arquivo `slot_1.json` criado em `Application.persistentDataPath/saves/`
- [ ] **FARM-071-TEST2:** JSON contém dia, inventário, canteiros, HP, fome, ouro
- [ ] **FARM-071-TEST3:** Notificação "Jogo salvo" aparece por 2s e desaparece

---

---

# SPEC FARM-072 — BootScene e Load

## /speckit.specify

**O QUE:** BootScene verifica save existente e carrega o estado. Sem save: novo jogo com defaults.

---

## /speckit.tasks

### [CODE]
- [ ] **FARM-072-C1:** Criar `BootScene.unity` (primeira cena no Build Settings)
- [ ] **FARM-072-C2:** Criar `GameBootstrap.cs`:
  - Inicializa Managers (DontDestroyOnLoad): `TimeManager`, `SaveManager`, `PlayerManager`, `InventoryManager`
  - Se `SaveManager.HasSave()`: carrega save, carrega `FarmScene` com dados
  - Se não: inicializa com defaults do `PlayerDataSO`, carrega `FarmScene`
- [ ] **FARM-072-C3:** Cada sistema expõe `void LoadFromSave(SaveData data)` que restaura seu estado
- [ ] **FARM-072-C4:** Ordem de load: TimeManager → PlayerManager → InventoryManager → FarmSystem → SceneLoad

### [UI]
- [ ] **FARM-072-U1:** Tela de boot: fundo preto com logo "Cindar's Hope" centralizado
  - Logo: TextMeshPro com fonte pixel, "CINDAR'S HOPE", cor bronze #8B6914
  - Loading spinner: 4 frames de rotação simples
  - Desaparece ao carregar a FarmScene

- [ ] **FARM-072-U2:** Criar sprite `Logo_CindarsHope_Text.png`
  - 256x64px, letras pixel art, dourado #D4832A com sombra #5C3D1E

### [TEST]
- [ ] **FARM-072-TEST1:** Sem save → FarmScene carrega com estado inicial (Dia 1, 50G, itens iniciais)
- [ ] **FARM-072-TEST2:** Com save → FarmScene carrega com estado salvo (dia correto, inventário correto, canteiros no estado correto)
- [ ] **FARM-072-TEST3:** Corromper JSON → não crasha, inicia novo jogo e loga erro

---

---

# SPEC FARM-SELL — SellPoint e Menu de Venda

## /speckit.specify

**O QUE:** Objeto SellPoint na fazenda. E → menu de texto listando itens vendáveis com preço. Confirmar → remove itens, adiciona ouro.

---

## /speckit.tasks

### [CODE]
- [ ] **FARM-SELL-C1:** Implementar `SellPoint.cs` implementando `IInteractable`
- [ ] **FARM-SELL-C2:** Implementar `SellMenu.cs`:
  - Lista todos os itens do inventário com `BaseValue > 0`
  - Seleção numérica → escolhe item
  - Tecla V → vende tudo do item selecionado
  - Tecla Q/E → ajusta quantidade
  - Confirma: `RemoveItem` → `PlayerManager.Gold += valor` → `GoldChangedEvent`
- [ ] **FARM-SELL-C3:** `GetInteractionHint()` → `"E: Vender itens"`

### [ART]
- [ ] **FARM-SELL-A1:** Criar sprite `SellPoint_32x32.png` — caixinha de correio/baú de venda
  - Baú marrom #5C3D1E com moeda dourada na frente #F0C040
  - Outline 1px #0A0A0A
  - Substituir placeholder amarelo de FARM-003-C5

### [UI]
- [ ] **FARM-SELL-U1:** Criar `SellMenu` Canvas (reutilizar estilo `UI_MenuPanel_9slice`):
  - Colunas: Ícone | Nome | Qtd no inv | Valor unit | Valor total selecionado
  - Rodapé: "[V] Vender Selecionado · [ESC] Fechar"
  - Total acumulado da venda no rodapé: "Total: 45 G"

**Mockup:**
```
╔═════════════════════════════════════════╗
║  🛒 VENDER ITENS                  [E]  ║
╠═════════════════════════════════════════╣
║  Item            Qtd   /un   Total     ║
║  [🌾] Trigo      x 6   5G    30G  [sel]║
║  [🥕] Cenoura    x 4   8G    32G       ║
║  [🐟] Peixe Com  x 2  12G    24G       ║
╠═════════════════════════════════════════╣
║  Venda selecionada: 30 G               ║
║  [V] Vender · [Q/E] Qtd · [ESC] Sair  ║
╚═════════════════════════════════════════╝
```

### [TEST]
- [ ] **FARM-SELL-TEST1:** E no SellPoint → menu abre com itens corretos e preços
- [ ] **FARM-SELL-TEST2:** Vender 6 Trigos → ouro aumenta 30 → Trigo some do inventário
- [ ] **FARM-SELL-TEST3:** Ouro no HUD atualiza imediatamente
- [ ] **FARM-SELL-TEST4:** Tentar vender item sem ter → quantidade não vai negativa

---

---

# RESUMO: Ordem de Implementação MVP + Contagem de Tasks

## Por semana

| Semana | Specs | Tasks CODE | Tasks ART | Tasks ANIM | Tasks VFX | Tasks UI | Tasks TEST |
|---|---|---|---|---|---|---|---|
| 1 | FARM-001, 002, 003 | 18 | 16 | 9 | 2 | 2 | 12 |
| 2 | FARM-011, 021, 031, 032 | 14 | 12 | 0 | 0 | 6 | 10 |
| 3 | FARM-012, 013, 014, 015, 016 | 16 | 5 | 4 | 4 | 4 | 14 |
| 4 | FARM-041, 042, 051, 052 | 12 | 12 | 4 | 6 | 3 | 12 |
| 5 | FARM-061, 062, 071, 072, SELL | 18 | 6 | 0 | 2 | 10 | 14 |
| **Total** | **22 specs** | **78** | **51** | **17** | **14** | **25** | **62** |

**Total geral: ~247 tasks** distribuídas em 22 specs individuais.

## Milestone de validação do MVP
O loop mínimo validado é:
```
Iniciar jogo (BootScene) →
  Inventário com sementes iniciais →
  Plantar no canteiro (E → menu → selecionar) →
  Avançar dias (TAB x3) →
  Colher (E → popup +3 Trigo) →
  Vender (E no SellPoint → +15G) →
  Salvar (automático no TAB) →
  Fechar e reabrir → estado restaurado ✓
```

---

*Fase 7 — Spec MVP Farm v2.2. 22 specs individuais com camadas CODE, ART, ANIM, VFX, UI e TEST. ~247 tasks rastreáveis. Aseprite definido como ferramenta principal de sprites. Pronto para implementação com Codex na Fase 8.*
