# SPEC 17A — Visual Scale, Map Size, Character & Creature Size Rebaseline

> **Projeto:** Cindar's Hope  
> **Status:** Proposta de spec nova — executar antes da SPEC 17 UI/UX final  
> **Ordem sugerida:** após SPEC 16 e antes da SPEC 17  
> **Nome preservado:** `SPEC 17A`, porque a conversa anterior lembrava esse rótulo  
> **Escopo:** rebaseline de escala visual, tamanho de mapas, personagem, NPCs, criaturas, colisores, câmera, navegação e validações  
> **Fora de escopo:** arte final, UI final, criação de roster completo novo, balanceamento final de combate, minimap, novos biomas, novas quests

---

## 1. Contexto recuperado da conversa

Houve uma decisão discutida para abandonar a escala visual pequena do MVP e preparar o jogo para uma leitura mais confortável:

- mapas maiores;
- personagem maior;
- NPCs maiores;
- criaturas com categorias formais de tamanho;
- caverna e farm com mais espaço de navegação;
- ranges, colisores e câmera ajustados à nova escala.

A intenção registrada era:

```text
Mapas: aumentar em 4x.
Personagem: aumentar em 2x.
Criaturas: usar categoria de tamanho.
Exemplo:
- Tiny = 1x
- Average = 2x
- categorias maiores para inimigos grandes e bosses.
```

Esta spec formaliza essa decisão antes da SPEC 17 UI/UX final para evitar construir HUD, hotbar, context hints, cave UI e painéis finais sobre escala provisória.

---

## 2. Estado atual do projeto

### 2.1 Tamanhos atuais documentados

A base atual usa:

| Asset | Tamanho atual |
|---|---:|
| Tile | 32x32 |
| Ícone de item | 32x32 |
| Jogador | 32x48 |
| NPC humanoide | 32x48 |
| Inimigo placeholder | 32x32 |
| Árvore pequena | 32x64 |
| Portrait simples | 64x64 |
| VFX particle | 8x8 ou 8x4 |

### 2.2 Farm MVP atual

A FarmScene MVP foi especificada como:

```text
Mapa total: 20x16 tiles
Resolução em pixels: 640x512px
```

### 2.3 Problema

Com player 32x48 e inimigos 32x32:

- o personagem parece pequeno;
- criaturas médias parecem pequenas demais;
- mapas ficam comprimidos;
- caverna perde sensação de exploração;
- bosses ficam visualmente fracos;
- UI final pode ser desenhada para uma escala que será trocada depois.

---

## 3. Decisão principal

### 3.1 Manter tile base

Manter:

```text
Tile = 32x32
Pixels Per Unit = 32
```

Não mudar PPU global.

### 3.2 Não usar Transform scale como solução primária

Proibido usar como solução estrutural:

```csharp
transform.localScale = new Vector3(2, 2, 1);
```

Motivo:

- distorce pixel art;
- mascara tamanho real do sprite;
- quebra consistência de colliders;
- dificulta sorting;
- dificulta animação;
- dificulta hitboxes;
- complica prefab variants;
- cria bugs em VFX/projectiles.

Pode ser permitido apenas em debug/dev mode temporário, nunca como implementação final da spec.

### 3.3 Aumentar tamanho por asset, prefab e profile

A escala correta deve vir de:

- sprites com canvas maior;
- colliders reconfigurados;
- pivots bottom-center;
- SortingPoint correto;
- size profiles em ScriptableObject;
- mapas maiores por quantidade de tiles;
- camera bounds e orthographic size ajustados.

---

## 4. Decisão sobre mapa 4x

### 4.1 Recomendação

Interpretar `mapa 4x` como:

```text
4x em área total, não 4x em cada eixo.
```

Portanto:

```text
Largura x2
Altura x2
Área x4
```

### 4.2 FarmScene

Atual:

```text
20x16 tiles = 320 tiles
```

Novo recomendado:

```text
40x32 tiles = 1280 tiles
```

Não recomendado como padrão agora:

```text
80x64 tiles = 5120 tiles
```

Motivo: 80x64 é 16x em área, não 4x. Pode ser usado futuramente para áreas específicas, mas é grande demais para uma primeira rebaseline porque afeta navegação, câmera, densidade de objetos e tempo de travessia.

### 4.3 Caves

Caves devem usar o mesmo princípio:

```text
aumentar área jogável e corredores por configuração procedural,
não escalar Tilemap ou Transform.
```

Regras:

- aumentar dimensões de geração por profile;
- aumentar largura mínima de corredor;
- aumentar padding de salas;
- garantir espaço para criaturas Large/Huge/Boss;
- garantir que spawn anchors validem footprint;
- preservar a regra de stable run: nível já visitado não pode mudar na mesma CaveRunSeed.

### 4.4 TownScene

Town deve seguir:

```text
área 4x em relação ao layout MVP,
com ruas mais largas e espaço suficiente para NPCs 2x.
```

Pip e NPCs ambulantes precisam ter path sem colisão com player/NPCs grandes.

---

## 5. Decisão sobre personagem 2x

### 5.1 Player

Atual:

```text
32x48
```

Novo recomendado:

```text
64x96
```

Interpretação:

```text
2x visual, não 2x collider.
```

O sprite cresce, mas a área de colisão deve representar o “pé” do personagem, não o corpo inteiro.

### 5.2 Player collider

Recomendação inicial:

```text
Sprite visual: 64x96
Collider footprint: ~32x28 até 40x32
Pivot: bottom-center
Sorting: pelo pé/base
```

Não usar collider do tamanho completo do sprite.

### 5.3 Player movement

Manter movimento em unidades de mundo, não em pixels.

Avaliar após Play Mode:

```text
MoveSpeed atual pode permanecer inicialmente.
Se parecer lento pela escala/mapa maior, aumentar entre 10% e 25%.
Não dobrar automaticamente.
```

### 5.4 Player interaction

Recalibrar:

```text
InteractionRadius
ContextHint offset
Tool origin
Attack origin
Projectile spawn origin
Fishing/chop/plant interaction distances
```

---

## 6. NPCs

NPC humanoide deve seguir o player:

```text
NPC humanoide: 64x96
Collider footprint: ~32x28 até 40x32
Pivot: bottom-center
```

NPCs muito pequenos ou grandes podem usar CreatureSizeCategory, mas NPC humanoide comum deve ser `AverageHumanoid`.

---

## 7. Creature size system

### 7.1 Enum

Criar ou consolidar:

```csharp
public enum CreatureSizeCategory
{
    Tiny,
    Small,
    Average,
    Large,
    Huge,
    Boss
}
```

### 7.2 Tabela de tamanho visual

| Categoria | Multiplicador | Sprite visual recomendado | Uso |
|---|---:|---:|---|
| Tiny | 1x | 32x32 | morcego pequeno, slime pequeno, rato |
| Small | 1.5x | 48x48 | goblin pequeno, aranha, criatura baixa |
| Average | 2x | 64x64 | slime normal novo, lobo, inimigo comum |
| AverageHumanoid | 2x | 64x96 | humanoide, cultista, guarda, NPC hostil |
| Large | 3x | 96x96 | ogro, fera grande, mini-boss |
| Huge | 4x | 128x128 | criatura massiva, golem, elite rara |
| Boss | 5x–6x | 160x160 até 192x192 | boss de gate, boss de bioma |

### 7.3 Footprint separado do visual

Cada criatura deve ter:

```text
VisualBounds
FootprintTiles
ColliderSize
ColliderOffset
HitboxSize
HurtboxSize
AttackOriginOffset
ProjectileSpawnOffset
NameplateOffset
HealthBarOffset
ShadowSize
```

Exemplo:

| Categoria | Footprint inicial |
|---|---:|
| Tiny | 1x1 tile |
| Small | 1x1 tile |
| Average | 1x1 ou 2x1 tiles |
| AverageHumanoid | 1x1 tile |
| Large | 2x2 tiles |
| Huge | 3x3 tiles |
| Boss | 4x4+ tiles |

### 7.4 Multiplicadores de gameplay

Cada categoria pode expor:

```text
AggroRangeMultiplier
AttackRangeMultiplier
ContactDamageRadiusMultiplier
KnockbackResistanceMultiplier
MoveSpeedMultiplier
PathfindingClearanceTiles
```

Valores iniciais sugeridos:

| Categoria | Aggro | Attack range | Knockback resist | Speed |
|---|---:|---:|---:|---:|
| Tiny | 0.8 | 0.8 | 0.5 | 1.15 |
| Small | 0.9 | 0.9 | 0.75 | 1.05 |
| Average | 1.0 | 1.0 | 1.0 | 1.0 |
| Large | 1.15 | 1.2 | 1.5 | 0.9 |
| Huge | 1.3 | 1.4 | 2.0 | 0.8 |
| Boss | 1.5 | 1.6 | 3.0 | 0.75 |

---

## 8. Arquitetura proposta

### 8.1 Data assets

Criar:

```text
Assets/_Game/Data/Scale/VisualScaleProfileSO.asset
Assets/_Game/Data/Scale/MapScaleProfileSO.asset
Assets/_Game/Data/Scale/CreatureSizeProfileSO.asset
```

Ou um registry único:

```text
ScaleRegistrySO
```

### 8.2 Classes sugeridas

```text
CreatureSizeCategory.cs
CreatureSizeProfileSO.cs
VisualScaleProfileSO.cs
MapScaleProfileSO.cs
ScaleProfileRegistrySO.cs
ScaleProfileApplier.cs
ColliderFootprintApplier.cs
SortingPivotValidator.cs
SpawnFootprintValidator.cs
CameraScaleConfigSO.cs
```

### 8.3 Campos em EnemyDataSO

Adicionar ou consolidar:

```text
CreatureSizeCategory SizeCategory
string SizeProfileId
Vector2 VisualBounds
Vector2 ColliderSize
Vector2 ColliderOffset
Vector2 AttackOriginOffset
Vector2 HealthBarOffset
int FootprintWidthTiles
int FootprintHeightTiles
float AggroRangeMultiplier
float AttackRangeMultiplier
float KnockbackResistanceMultiplier
```

### 8.4 Campos em PlayerDataSO

Adicionar ou consolidar:

```text
string VisualScaleProfileId
Vector2 VisualSpriteSize
Vector2 ColliderSize
Vector2 ColliderOffset
Vector2 InteractionOriginOffset
Vector2 AttackOriginOffset
Vector2 ProjectileSpawnOffset
```

### 8.5 Camera

Criar configuração:

```text
CameraScaleConfigSO
```

Campos:

```text
OrthographicSizeFarm
OrthographicSizeTown
OrthographicSizeCave
FollowDamping
DeadZone
LookAhead
MinBoundsPadding
```

Recomendação inicial:

```text
Não fixar número definitivo sem Play Mode.
Começar com OrthographicSize um pouco maior que o atual e ajustar por cena.
```

---

## 9. Sistemas afetados

### 9.1 Farm

Atualizar:

```text
CreateMvpFarmScene
FarmSceneRuntimeReferenceInstaller
Farm layout constants
Plot positions
Tree positions
Lake size
FishingSpot positions
SellPoint position
Camera bounds
Interaction radius
```

Novo Farm MVP rebaseline:

```text
40x32 tiles
Canteiros iniciais: podem continuar 3x3, mas com mais espaço em volta
Lago: aumentar para 4x6 ou 5x7 tiles
Árvores: espalhar mais
SellPoint: próximo à entrada, mas fora do caminho central
```

### 9.2 Town

Atualizar:

```text
CreateMvpTownScene
NPC spawn points
Pip approach path
Shop NPC positions
Street width
Camera bounds
Portal positions
```

### 9.3 Cave

Atualizar:

```text
CaveGenerationConfigSO
CaveRuntimeMaterializer
CaveSpawnAnchor
CavePlayerPathConfinement
Enemy spawn validation
Resource node placement
Exit placement
Boss room sizing
Camera bounds
```

Regras:

- Large/Huge/Boss não podem spawnar em corredor estreito.
- Footprint precisa caber em tiles caminháveis.
- Boss gate room precisa ter espaço de combate.
- Revisit snapshot não pode mudar por causa da nova escala; se a scale config mudar a generation version, deve tratar como nova version de geração.

### 9.4 Combat

Atualizar:

```text
Melee range
Arc radius
Hitbox/hurtbox
Contact damage radius
Projectile spawn origin
Projectile collision radius
Dodge distance
Knockback distance
Floating damage number offset
Telegraph visual size
```

### 9.5 UI

Não implementar UI final aqui.

Apenas expor offsets para a SPEC 17:

```text
Health bar world offset
Nameplate offset
Interaction hint offset
Damage popup offset
Boss health UI anchor data
```

---

## 10. Save/Load

Não salvar sprite, Transform, collider ou SO.

Salvar apenas:

```text
SizeCategory
SizeProfileId
GenerationVersion
MapScaleProfileId
```

Se `MapScaleProfileId` ou `GenerationVersion` mudar:

- saves antigos devem migrar com fallback;
- snapshots de cave podem precisar ser versionados;
- nunca quebrar load por referência Unity ausente.

---

## 11. Migração de assets placeholder

### 11.1 Player

Criar placeholder:

```text
Player_Placeholder_Down_64x96.png
Player_Placeholder_Up_64x96.png
Player_Placeholder_Left_64x96.png
Player_Placeholder_Right_64x96.png
```

### 11.2 NPC

Criar:

```text
NPC_Placeholder_64x96.png
```

### 11.3 Enemies

Criar placeholders:

```text
Enemy_Tiny_Placeholder_32x32.png
Enemy_Small_Placeholder_48x48.png
Enemy_Average_Placeholder_64x64.png
Enemy_AverageHumanoid_Placeholder_64x96.png
Enemy_Large_Placeholder_96x96.png
Enemy_Huge_Placeholder_128x128.png
Enemy_Boss_Placeholder_160x160.png
```

### 11.4 Import

Todos:

```text
Texture Type: Sprite
PPU: 32
Filter Mode: Point
Compression: None
Generate Mip Maps: false
Pivot: Bottom Center quando aplicável
```

---

## 12. Tasks

### CODE

- [ ] **17A-C01:** Criar `CreatureSizeCategory`.
- [ ] **17A-C02:** Criar `CreatureSizeProfileSO`.
- [ ] **17A-C03:** Criar `VisualScaleProfileSO`.
- [ ] **17A-C04:** Criar `MapScaleProfileSO`.
- [ ] **17A-C05:** Criar/atualizar registry de scale profiles.
- [ ] **17A-C06:** Adicionar size fields em `EnemyDataSO`.
- [ ] **17A-C07:** Adicionar scale fields em `PlayerDataSO`.
- [ ] **17A-C08:** Criar `ScaleProfileApplier` para aplicar collider, offsets e sorting config.
- [ ] **17A-C09:** Criar `SpawnFootprintValidator` para garantir que criaturas cabem em tiles caminháveis.
- [ ] **17A-C10:** Atualizar geração/criação da FarmScene para 40x32 tiles.
- [ ] **17A-C11:** Atualizar criação/layout da TownScene para escala 4x área.
- [ ] **17A-C12:** Atualizar cave generation/materializer para respeitar footprint e corridor clearance.
- [ ] **17A-C13:** Atualizar camera bounds e camera scale por cena.
- [ ] **17A-C14:** Atualizar interação e combat origins para player 64x96.
- [ ] **17A-C15:** Atualizar enemy AI ranges usando size multipliers.
- [ ] **17A-C16:** Atualizar floating damage/nameplate/healthbar world offsets.
- [ ] **17A-C17:** Atualizar save DTOs somente com IDs/version simples, se necessário.
- [ ] **17A-C18:** Criar migration/fallback para saves antigos sem scale profile.

### ART / PLACEHOLDERS

- [ ] **17A-A01:** Criar player placeholder 64x96 nas 4 direções.
- [ ] **17A-A02:** Criar NPC placeholder 64x96.
- [ ] **17A-A03:** Criar enemy placeholders por categoria.
- [ ] **17A-A04:** Validar import pixel art.
- [ ] **17A-A05:** Ajustar pivots bottom-center.
- [ ] **17A-A06:** Validar sorting visual em Farm/Town/Cave.

### SCENE / PREFAB

- [ ] **17A-S01:** Atualizar Player prefab.
- [ ] **17A-S02:** Atualizar NPC prefab base.
- [ ] **17A-S03:** Atualizar Enemy prefab base.
- [ ] **17A-S04:** Atualizar Boss prefab placeholder.
- [ ] **17A-S05:** Atualizar scene installers.
- [ ] **17A-S06:** Atualizar editor scripts de criação de cena.
- [ ] **17A-S07:** Garantir que nenhuma cena dependa de scale manual no Transform.

### VALIDATION

- [ ] **17A-V01:** Rodar `tools/docs/validate_docs.ps1`.
- [ ] **17A-V02:** Rodar `tools/unity/RunUnityCompileValidation.ps1`.
- [ ] **17A-V03:** Rodar `tools/unity/ScanUnityLogs.ps1`.
- [ ] **17A-V04:** Criar FarmScene e validar mapa 40x32.
- [ ] **17A-V05:** Play Mode Farm: player 64x96 move, colide e interage.
- [ ] **17A-V06:** Play Mode Town: NPCs 64x96 não travam path/interação.
- [ ] **17A-V07:** Play Mode Cave: Tiny/Average/Large spawnam sem sobrepor parede.
- [ ] **17A-V08:** Play Mode Combat: melee e contact damage respeitam novos offsets.
- [ ] **17A-V09:** Play Mode Camera: câmera segue player e respeita bounds nas três cenas.
- [ ] **17A-V10:** Save/load não quebra com scale profile.
- [ ] **17A-V11:** Revisit cave level não muda layout/composição por causa da escala.
- [ ] **17A-V12:** Console sem erro vermelho.

---

## 13. Critérios de aceite

A spec só passa se:

- player visual estiver em 64x96;
- NPC humanoide base estiver em 64x96;
- FarmScene rebaseline estiver em 40x32 tiles;
- criatura tiver `CreatureSizeCategory`;
- inimigos puderem usar pelo menos Tiny, Average e Large;
- colliders forem ajustados por footprint, não por sprite inteiro;
- câmera funcionar nas três cenas;
- cave spawn validar footprint;
- combat/interaction origins forem recalibrados;
- nenhuma solução final depender de `Transform.localScale = 2`;
- Unity compile passar;
- Play Mode básico passar em Farm, Town e Cave;
- documentação ativa for atualizada;
- `PROJECT_LOG.md` e `docs/IMPLEMENTATION_STATUS.md` registrarem a mudança.

---

## 14. Prompt para Claude/Codex

```md
Estamos no projeto Cindar's Hope / repo rafa210587/cindars_hope.

Branch base: dev.

Objetivo:
Implementar SPEC 17A — Visual Scale, Map Size, Character & Creature Size Rebaseline.

Antes de alterar código, leia:
- AGENTS.md
- PROJECT_LOG.md
- docs/IMPLEMENTATION_STATUS.md
- docs/operations/AGENT_EXECUTION_PROTOCOL.md
- docs/operations/SPRITE_PIPELINE_AI_ASEPRITE_v1.0.md
- docs/specs/SPEC_EXECUTION_ORDER.md
- docs/specs/a_implementar/spec_ui_ux_full_gameplay_inventory_hotbar_menus.md
- specs/documentos/refinamentos relacionados somente se forem citados pela busca no repo

Decisão central:
- Manter Tile 32x32.
- Manter PPU 32.
- Interpretar mapa 4x como área 4x: Farm 20x16 -> 40x32.
- Player 2x: 32x48 -> 64x96.
- NPC humanoide 2x: 32x48 -> 64x96.
- Criaturas por SizeCategory:
  - Tiny = 1x = 32x32
  - Small = 1.5x = 48x48
  - Average = 2x = 64x64
  - AverageHumanoid = 2x = 64x96
  - Large = 3x = 96x96
  - Huge = 4x = 128x128
  - Boss = 5x-6x = 160x160/192x192
- Não usar Transform.localScale como solução final.
- Aumentar mapas por tiles/configuração, não por escala de Transform.

Escopo:
- criar profiles de escala;
- criar categorias de tamanho de criaturas;
- atualizar PlayerDataSO/EnemyDataSO com campos necessários;
- atualizar prefabs/placeholder assets via Editor quando aplicável;
- atualizar Farm/Town/Cave scene generation;
- atualizar câmera/bounds;
- atualizar colliders/footprints;
- atualizar interaction/combat origins/ranges;
- atualizar spawn validation para criaturas grandes;
- atualizar docs/status/log.

Fora de escopo:
- UI final da SPEC 17;
- arte final;
- roster completo novo;
- balanceamento final;
- minimap;
- quests;
- gamepad;
- multiplayer.

Validações obrigatórias:
- .\tools\docs\validate_docs.ps1
- .\tools\unity\RunUnityCompileValidation.ps1
- .\tools\unity\ScanUnityLogs.ps1 -LogFile ".\Logs\unity-compile-validation.log"
- Play Mode Farm: mapa 40x32, player 64x96, colisão/interação ok
- Play Mode Town: NPCs 64x96, interação ok
- Play Mode Cave: Tiny/Average/Large spawnam corretamente
- Play Mode Combat: hitbox/hurtbox/origin/range ok
- Save/load sem erro
- Console sem erro vermelho

Ao final, entregue:
- arquivos alterados;
- commits em português;
- validações executadas;
- validações não executadas com motivo;
- pendências;
- riscos;
- atualização em PROJECT_LOG.md;
- atualização em docs/IMPLEMENTATION_STATUS.md.
```

---

## 15. Risco principal

O maior risco não é visual. É sistêmico:

```text
sprite maior sem rebaseline de collider, camera, spawn e range
```

Se só trocar sprite, o jogo quebra em sensação, colisão, combate e navegação.

Por isso esta spec precisa vir antes da UI final.
