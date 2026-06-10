---
name: npc-dialogue-authoring
description: Create NpcDataSO, DialogueTreeSO, NpcScenePlacementMarker, and shop/service data for one or more NPCs
version: 1.0
when_to_use: Any task creating or extending NPCs with dialogue, shop services, or scene placement in TownScene or other city scenes
---

# NPC Dialogue Authoring Skill

## Use When

Task requires:
- Creating new NPCs (name, zone, purpose, dialogue)
- Wiring NPC shop or service (buy/sell panels, service modals)
- Placing NPCs in a scene (marker approach, not direct YAML)
- Extending existing NPC dialogue sets
- Auditing NPC roster completeness

## Required Reads

1. `CLAUDE.md`
2. Target spec
3. Canonical roster doc for the relevant wave (e.g., `docs/validation/WAVE_INTEGRATION_12C_REFINED_NPC_CANONICAL_ROSTER.md`)
4. `docs/validation/WAVE_INTEGRATION_12C_REFINED_NPC_SHOP_SERVICES.md` (service definitions)

Do NOT read by default:
- Full GDD NPC chapters
- All prior wave NPC docs
- Unrelated dialogue sets

---

## NPC Data Model

### NpcDataSO

Canonical fields:
```csharp
npcId        string   — kebab-case, e.g. "npc_sylveth_blacksmith"
displayName  string   — "Sylveth"
zone         string   — "TradeDistrict" / "CraftingQuarter" / "Plaza" / etc.
serviceType  enum     — Shop / Service / DialogueOnly
movementProfile string — "stationary" / "patrol_zone" / "wanders"
```

Naming convention:
```
npc_<name>_<role>
npc_sylveth_blacksmith
npc_pip_general_store
npc_brumdar_tavern
```

### DialogueTreeSO

Minimum 10 nodes per NPC. Structure:
```
node_0: greeting (always)
node_1: about_self
node_2: about_town / zone context
node_3–node_N: service / lore / quest hints / weather / day cycle
node_last: farewell
```

Node types:
```
GREETING     — opening line, shown first
ABOUT        — backstory / role
SHOP_INTRO   — "I have wares if you have coin"
HINT         — gameplay hint (optional)
LORE         — world lore (optional)
FAREWELL     — closing line
```

**Minimum node count**: 10 (hard requirement from WAVE_INTEGRATION_12C).

---

## Shop / Service Wiring

### NPC categories

| Type | Wiring |
|------|--------|
| Full shop (buy+sell) | `NpcShopController` + `ShopMenuModal` + `BuyPanel` + `SellPanel` |
| Service only | `NpcShopController` + service modal (advanced — may be debt) |
| Dialogue-only | No shop wiring; dialogue nodes explain why no shop |

### Shop wiring pattern (reuse existing)

```csharp
// NpcShopController.Start() resolves ShopManager via bootstrap
var bootstrap = GameBootstrap.Instance;
if (bootstrap != null)
    _shopManager = bootstrap.ShopManager;
```

Do NOT create a new ShopManager. Reuse `GameBootstrap.Instance.ShopManager`.

### Service NPCs with advanced UI (debt pattern)

If service UI (e.g., blacksmith upgrade modal, inn rest modal) is not yet implemented:
```
SHOP_RUNTIME_BASIC_WITH_ADVANCED_SERVICE_DEBT
```

Wire as basic shop (if they buy/sell items) and document the advanced service debt explicitly.

---

## Scene Placement (Marker approach)

**Never edit scene YAML directly.** Use `NpcScenePlacementMarker`:

```csharp
// NpcScenePlacementMarker.cs — existing component
// Set: npcId, zone, position (Vector2), facingDirection
// The scene creator (CreateMvpTownScene, etc.) reads markers to place NPCs
```

Document intended positions in placement map doc:
```
docs/validation/WAVE_INTEGRATION_<N>_NPC_PLACEMENT_MAP.md
```

Format:
```
| NPC | Zone | Position | Facing | Service |
|-----|------|----------|--------|---------|
| Sylveth | CraftingQuarter | (12, -4) | right | blacksmith_shop |
```

---

## Authoring Docs Required

| Doc | When required |
|-----|---------------|
| `WAVE_INTEGRATION_<N>_NPC_CANONICAL_ROSTER.md` | New batch of NPCs |
| `WAVE_INTEGRATION_<N>_NPC_DIALOGUE_SETS.md` | Dialogue content for all new NPCs |
| `WAVE_INTEGRATION_<N>_NPC_SHOP_SERVICES.md` | Shop/service definitions |
| `WAVE_INTEGRATION_<N>_NPC_PLACEMENT_MAP.md` | Scene positions |
| `WAVE_INTEGRATION_<N>_NPC_MOVEMENT_SCHEDULES.md` | Patrol/wander profiles (if any) |

---

## Movement Profiles

Use these canonical profiles (do not invent new ones):

| Profile | Behavior |
|---------|----------|
| `stationary` | Fixed position, faces player on interact |
| `patrol_zone` | Walks back and forth within a zone rect |
| `wanders` | Random walks within zone bounds |

Full NPC schedules (time-of-day, day-of-week) are deferred:
```
NPC_SCHEDULE_DEFERRED_TO_NPC_SOCIAL_SYSTEM
```

---

## Dialogue Quality Rules

- No placeholder text (`...`, `TODO`, `test dialogue`)
- Each NPC voice must match their role and zone
- At least one node references the current in-game context (season, economy, player progress hint)
- `FAREWELL` node is always present
- `GREETING` node is always first

---

## What is Out of Scope (document as debt)

```
NPC_QUEST_RELATIONSHIP_DEFERRED     — quest unlock via NPC
NPC_SOCIAL_REPUTATION_DEFERRED      — reputation/friendship system
NPC_ROMANCE_DEFERRED                — romance/companion system
NPC_SCHEDULE_DEFERRED               — time-of-day schedules
NPC_ADVANCED_SERVICE_UI_DEFERRED    — blacksmith upgrade, inn rest, etc.
```

---

## Validation

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
if ($LASTEXITCODE -ne 0) { Write-Host "BUILD FAILED"; exit 1 }
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
if ($LASTEXITCODE -ne 0) { Write-Host "EDITOR BUILD FAILED"; exit 1 }
```

Human Play Mode checklist must cover:
- Interact with each new NPC → dialogue opens
- Dialogue has ≥10 nodes, no placeholder text
- Shop NPCs → BuyPanel and/or SellPanel opens
- Dialogue-only NPCs → no shop UI opens
- Close dialogue with Esc / Back

---

## Common Regressions

- Fewer than 10 dialogue nodes → spec violation
- Creating a new ShopManager instead of reusing bootstrap
- Directly editing TownScene.unity YAML → scene corruption risk
- Inventing new movement profiles not in the canonical list
- Missing NPC_ADVANCED_SERVICE_UI_DEFERRED tag for service NPCs without full UI

## Stop Conditions

- Spec requires romance/companion/quest systems → `BLOCKED`, out of scope
- NpcDataSO schema changed and breaks existing wired NPCs → stop, report
- Scene YAML must be edited manually and CreateScene approach is not available → `CODE_READY_HUMAN_UNITY_SCENE_ACTION_REQUIRED`
