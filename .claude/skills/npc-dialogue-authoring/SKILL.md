---
name: npc-dialogue-authoring
description: Cria NpcDataSO, DialogueTreeSO, NpcScenePlacementMarker e dados de shop/service para NPCs. Usar em specs que criem ou estendam NPCs com dialogue, shop services ou scene placement na TownScene ou outras city scenes.
---

# Skill: Autoria de Dialogue de NPC

Use os assets canônicos do projeto (`NpcDataSO`, `DialogueTreeSO`, `NpcScenePlacementMarker`) e reutilize o `ShopManager` do `GameBootstrap` — **nunca crie sistemas paralelos nem edite scene YAML à mão**.

## Quando usar

- Criar novos NPCs (name, zone, purpose, dialogue).
- Wiring de shop ou service (buy/sell panels, service modals).
- Posicionar NPCs numa scene (marker approach).
- Estender dialogue sets de NPCs existentes.
- Auditar completude do roster de NPCs.

## Quando NÃO usar

- NPC sem dialogue (trigger puro) → `scene-interactable-wiring`.
- Spec de quest ou reputação via NPC → registrar como `NPC_QUEST_RELATIONSHIP_DEFERRED`.
- Edição de YAML de scene diretamente → sempre bloqueado; usar marker approach.

## Leitura mínima

1. Spec alvo
2. Roster canônico da wave (ex.: `docs/validation/WAVE_INTEGRATION_12C_REFINED_NPC_CANONICAL_ROSTER.md`)
3. Definições de service (ex.: `docs/validation/WAVE_INTEGRATION_12C_REFINED_NPC_SHOP_SERVICES.md`)

**Não ler por padrão:** GDD completo, todos os docs de NPC de waves anteriores, dialogue sets não relacionados.

---

## Data model

### NpcDataSO

```
npcId          string  — kebab-case: "npc_sylveth_blacksmith"
displayName    string  — "Sylveth"
zone           string  — "TradeDistrict" | "CraftingQuarter" | "Plaza"
serviceType    enum    — Shop | Service | DialogueOnly
movementProfile string — "stationary" | "patrol_zone" | "wanders"
```

### DialogueTreeSO — mínimo 10 nodes

```
node_0:    GREETING  (sempre presente, sempre primeiro)
node_1:    ABOUT_SELF
node_2:    ABOUT_TOWN ou contexto da zone
node_3–N:  SHOP_INTRO | HINT | LORE | quest hints | weather/season
node_last: FAREWELL  (sempre presente)
```

Sem placeholder text (`...`, `TODO`, `test dialogue`). A voz deve combinar com role e zone.

---

## Wiring de shop / service

| Type | Wiring |
|------|--------|
| Full shop (buy+sell) | `NpcShopController` + `ShopMenuModal` + `BuyPanel` + `SellPanel` |
| Service only | `NpcShopController` + service modal (pode ser debt — ver abaixo) |
| Dialogue-only | Sem shop wiring; dialogue explica por que não há shop |

```csharp
// NpcShopController.Start() — sempre via bootstrap
var bootstrap = GameBootstrap.Instance;
if (bootstrap != null) _shopManager = bootstrap.ShopManager;
// NÃO crie novo ShopManager
```

Se a UI de service ainda não foi implementada:
```
SHOP_RUNTIME_BASIC_WITH_ADVANCED_SERVICE_DEBT
```

---

## Scene placement (marker approach)

**Nunca edite YAML de scene diretamente.** Use `NpcScenePlacementMarker`:

```
NpcScenePlacementMarker: npcId, zone, position (Vector2), facingDirection
```

Documente posições em:
```
docs/validation/WAVE_INTEGRATION_<N>_NPC_PLACEMENT_MAP.md

| NPC     | Zone            | Position | Facing | Service         |
|---------|-----------------|----------|--------|-----------------|
| Sylveth | CraftingQuarter | (12, -4) | right  | blacksmith_shop |
```

---

## Docs exigidos

| Doc | Quando |
|-----|--------|
| `WAVE_INTEGRATION_<N>_NPC_CANONICAL_ROSTER.md` | Novo batch de NPCs |
| `WAVE_INTEGRATION_<N>_NPC_DIALOGUE_SETS.md` | Conteúdo de dialogue |
| `WAVE_INTEGRATION_<N>_NPC_SHOP_SERVICES.md` | Definições de shop/service |
| `WAVE_INTEGRATION_<N>_NPC_PLACEMENT_MAP.md` | Posições na scene |
| `WAVE_INTEGRATION_<N>_NPC_MOVEMENT_SCHEDULES.md` | Patrol/wander (se houver) |

---

## Movement profiles canônicos (não inventar novos)

| Profile | Behavior |
|---------|----------|
| `stationary` | Posição fixa; vira para o player ao interagir |
| `patrol_zone` | Vai e volta dentro de um zone rect |
| `wanders` | Random walk dentro dos bounds da zone |

Schedules completos (time-of-day, day-of-week):
```
NPC_SCHEDULE_DEFERRED_TO_NPC_SOCIAL_SYSTEM
```

---

## Tokens DEFERRED (documentar, não implementar)

```
NPC_QUEST_RELATIONSHIP_DEFERRED     — quest unlock via NPC
NPC_SOCIAL_REPUTATION_DEFERRED      — reputation/friendship
NPC_ROMANCE_DEFERRED                — romance/companion
NPC_SCHEDULE_DEFERRED               — schedules de horário
NPC_ADVANCED_SERVICE_UI_DEFERRED    — blacksmith upgrade, inn rest, etc.
```

---

## Quando parar e reportar

- Spec exige romance/companion/quest via NPC → `BLOCKED`, fora de escopo.
- Schema de `NpcDataSO` mudou e quebra NPCs já wired → parar, reportar.
- O scene YAML precisa ser editado manualmente → `CODE_READY_HUMAN_UNITY_SCENE_ACTION_REQUIRED`.

## Regressões comuns

- Menos de 10 dialogue nodes (violação de spec).
- Criar novo `ShopManager` em vez de reutilizar o bootstrap.
- Editar diretamente `TownScene.unity` YAML.
- Inventar movement profiles fora da lista canônica.
- Faltar `NPC_ADVANCED_SERVICE_UI_DEFERRED` para service sem UI completa.

## Relacionados

- `(skill: localization-authoring)` — dialogue text via `LocalizationStringTable`
- `(skill: scene-interactable-wiring)` — NPCs como IInteractable na scene
- `(skill: wave-integration-slice)` — NPC wiring como parte de WAVE_INTEGRATION
- `(skill: loop-hierarchy-design)` — qual loop o NPC pertence (town loop)
- `(rule: unity-assets)` — sem edição manual de YAML de scene
