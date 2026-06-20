---
name: npc-dialogue-authoring
description: Cria NpcDataSO, DialogueTreeSO, NpcScenePlacementMarker e dados de shop/service para um ou mais NPCs. Use em qualquer tarefa que crie ou estenda NPCs com dialogue, shop services ou scene placement na TownScene ou em outras city scenes.
---

# Skill: Autoria de Dialogue de NPC

Use os assets canônicos do projeto (NpcDataSO, DialogueTreeSO, NpcScenePlacementMarker) e reutilize o `ShopManager` do `GameBootstrap` — nunca crie sistemas paralelos nem edite scene YAML à mão.

## Quando usar

A tarefa exige:
- Criar novos NPCs (name, zone, purpose, dialogue)
- Fazer wiring de shop ou service de NPC (buy/sell panels, service modals)
- Posicionar NPCs numa scene (marker approach, não YAML direto)
- Estender dialogue sets de NPCs existentes
- Auditar a completude do roster de NPCs

## Leitura mínima

1. `CLAUDE.md`
2. Spec alvo
3. Doc canônico de roster da wave relevante (ex.: `docs/validation/WAVE_INTEGRATION_12C_REFINED_NPC_CANONICAL_ROSTER.md`)
4. `docs/validation/WAVE_INTEGRATION_12C_REFINED_NPC_SHOP_SERVICES.md` (definições de service)

## Não ler por padrão

- Capítulos de NPC do GDD completo
- Todos os docs de NPC de waves anteriores
- Dialogue sets não relacionados

---

## Data model de NPC

### NpcDataSO

Campos canônicos:
```csharp
npcId        string   — kebab-case, e.g. "npc_sylveth_blacksmith"
displayName  string   — "Sylveth"
zone         string   — "TradeDistrict" / "CraftingQuarter" / "Plaza" / etc.
serviceType  enum     — Shop / Service / DialogueOnly
movementProfile string — "stationary" / "patrol_zone" / "wanders"
```

Convenção de naming:
```
npc_<name>_<role>
npc_sylveth_blacksmith
npc_pip_general_store
npc_brumdar_tavern
```

### DialogueTreeSO

Mínimo de 10 nodes por NPC. Estrutura:
```
node_0: greeting (always)
node_1: about_self
node_2: about_town / zone context
node_3–node_N: service / lore / quest hints / weather / day cycle
node_last: farewell
```

Tipos de node:
```
GREETING     — opening line, shown first
ABOUT        — backstory / role
SHOP_INTRO   — "I have wares if you have coin"
HINT         — gameplay hint (optional)
LORE         — world lore (optional)
FAREWELL     — closing line
```

**Contagem mínima de nodes**: 10 (requisito rígido de WAVE_INTEGRATION_12C).

---

## Wiring de shop / service

### Categorias de NPC

| Type | Wiring |
|------|--------|
| Full shop (buy+sell) | `NpcShopController` + `ShopMenuModal` + `BuyPanel` + `SellPanel` |
| Service only | `NpcShopController` + service modal (avançado — pode ser debt) |
| Dialogue-only | Sem wiring de shop; dialogue nodes explicam por que não há shop |

### Pattern de wiring de shop (reusar o existente)

```csharp
// NpcShopController.Start() resolves ShopManager via bootstrap
var bootstrap = GameBootstrap.Instance;
if (bootstrap != null)
    _shopManager = bootstrap.ShopManager;
```

Não crie um novo `ShopManager`. Reutilize `GameBootstrap.Instance.ShopManager`.

### Service NPCs com UI avançada (pattern de debt)

Se a UI de service (ex.: blacksmith upgrade modal, inn rest modal) ainda não foi implementada:
```
SHOP_RUNTIME_BASIC_WITH_ADVANCED_SERVICE_DEBT
```

Faça o wiring como basic shop (se eles compram/vendem items) e documente o debt do service avançado explicitamente.

---

## Scene placement (marker approach)

**Nunca edite o scene YAML diretamente.** Use `NpcScenePlacementMarker`:

```csharp
// NpcScenePlacementMarker.cs — existing component
// Set: npcId, zone, position (Vector2), facingDirection
// The scene creator (CreateMvpTownScene, etc.) reads markers to place NPCs
```

Documente as posições pretendidas no doc de placement map:
```
docs/validation/WAVE_INTEGRATION_<N>_NPC_PLACEMENT_MAP.md
```

Formato:
```
| NPC | Zone | Position | Facing | Service |
|-----|------|----------|--------|---------|
| Sylveth | CraftingQuarter | (12, -4) | right | blacksmith_shop |
```

---

## Docs de autoria exigidos

| Doc | Quando é exigido |
|-----|---------------|
| `WAVE_INTEGRATION_<N>_NPC_CANONICAL_ROSTER.md` | Novo batch de NPCs |
| `WAVE_INTEGRATION_<N>_NPC_DIALOGUE_SETS.md` | Conteúdo de dialogue para todos os novos NPCs |
| `WAVE_INTEGRATION_<N>_NPC_SHOP_SERVICES.md` | Definições de shop/service |
| `WAVE_INTEGRATION_<N>_NPC_PLACEMENT_MAP.md` | Posições na scene |
| `WAVE_INTEGRATION_<N>_NPC_MOVEMENT_SCHEDULES.md` | Perfis de patrol/wander (se houver) |

---

## Movement profiles

Use estes perfis canônicos (não invente novos):

| Profile | Behavior |
|---------|----------|
| `stationary` | Posição fixa, vira para o player ao interagir |
| `patrol_zone` | Anda de um lado para o outro dentro de um zone rect |
| `wanders` | Random walks dentro dos bounds da zone |

Schedules completos de NPC (time-of-day, day-of-week) ficam deferidos:
```
NPC_SCHEDULE_DEFERRED_TO_NPC_SOCIAL_SYSTEM
```

---

## Regras de qualidade de dialogue

- Sem placeholder text (`...`, `TODO`, `test dialogue`)
- A voz de cada NPC deve combinar com seu role e sua zone
- Pelo menos um node referencia o contexto atual in-game (season, economy, dica de progress do player)
- Node `FAREWELL` sempre presente
- Node `GREETING` sempre primeiro

---

## Fora de escopo (documentar como debt)

```
NPC_QUEST_RELATIONSHIP_DEFERRED     — quest unlock via NPC
NPC_SOCIAL_REPUTATION_DEFERRED      — reputation/friendship system
NPC_ROMANCE_DEFERRED                — romance/companion system
NPC_SCHEDULE_DEFERRED               — time-of-day schedules
NPC_ADVANCED_SERVICE_UI_DEFERRED    — blacksmith upgrade, inn rest, etc.
```

---

## Validação

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
if ($LASTEXITCODE -ne 0) { Write-Host "BUILD FAILED"; exit 1 }
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
if ($LASTEXITCODE -ne 0) { Write-Host "EDITOR BUILD FAILED"; exit 1 }
```

O checklist humano de Play Mode deve cobrir:
- Interagir com cada novo NPC → dialogue abre
- Dialogue tem ≥10 nodes, sem placeholder text
- Shop NPCs → BuyPanel e/ou SellPanel abre
- Dialogue-only NPCs → nenhuma shop UI abre
- Fechar dialogue com Esc / Back

---

## Regressões comuns

- Menos de 10 dialogue nodes → violação de spec
- Criar um novo `ShopManager` em vez de reutilizar o bootstrap
- Editar diretamente o YAML de TownScene.unity → risco de corromper a scene
- Inventar novos movement profiles fora da lista canônica
- Faltar a tag NPC_ADVANCED_SERVICE_UI_DEFERRED para service NPCs sem UI completa

## Quando parar e reportar

- Spec exige sistemas de romance/companion/quest → `BLOCKED`, fora de escopo
- Schema de NpcDataSO mudou e quebra NPCs já wired → parar, reportar
- O scene YAML precisa ser editado manualmente e o CreateScene approach não está disponível → `CODE_READY_HUMAN_UNITY_SCENE_ACTION_REQUIRED`
