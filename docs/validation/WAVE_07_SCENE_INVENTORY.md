# WAVE 07 Scene Inventory

## Status

VALIDATED

Inventory generated from:

```powershell
Get-ChildItem .\Assets -Recurse -Filter *.unity | Sort-Object FullName | Select-Object FullName
```

Unity Editor open validation was not run in this spec. Classifications are documental and based on path/name.

## Scenes found

| Scene | Path | Classification | Reason | Next action |
|---|---|---|---|---|
| CaveScene | `Assets/_Game/Scenes/CaveScene.unity` | ACTIVE_TARGET | Existing game scene and closest current cave runtime target. | Review for WAVE 07 cave entrance/runtime split before scene edits. |
| FarmScene | `Assets/_Game/Scenes/FarmScene.unity` | ACTIVE_TARGET | Existing game scene and primary farm integration target. | Use as initial FarmScene target in WAVE 07.02+. |
| TownScene | `Assets/_Game/Scenes/TownScene.unity` | ACTIVE_TARGET | Existing game scene and primary town integration target. | Use as initial TownScene target in WAVE 07.02+. |
| 01- Single Line TextMesh Pro | `Assets/TextMesh Pro/Examples & Extras/Scenes/01-  Single Line TextMesh Pro.unity` | LEGACY | Vendor/example scene, not a game scene. | Exclude from WAVE 07 gameplay targets. |
| 02 - Multi-line TextMesh Pro | `Assets/TextMesh Pro/Examples & Extras/Scenes/02 - Multi-line TextMesh Pro.unity` | LEGACY | Vendor/example scene, not a game scene. | Exclude from WAVE 07 gameplay targets. |
| 03 - Line Justification | `Assets/TextMesh Pro/Examples & Extras/Scenes/03 - Line Justification.unity` | LEGACY | Vendor/example scene, not a game scene. | Exclude from WAVE 07 gameplay targets. |
| 04 - Word Wrapping | `Assets/TextMesh Pro/Examples & Extras/Scenes/04 - Word Wrapping.unity` | LEGACY | Vendor/example scene, not a game scene. | Exclude from WAVE 07 gameplay targets. |
| 05 - Style Tags | `Assets/TextMesh Pro/Examples & Extras/Scenes/05 - Style Tags.unity` | LEGACY | Vendor/example scene, not a game scene. | Exclude from WAVE 07 gameplay targets. |
| 06 - Extra Rich Text Examples | `Assets/TextMesh Pro/Examples & Extras/Scenes/06 - Extra Rich Text Examples.unity` | LEGACY | Vendor/example scene, not a game scene. | Exclude from WAVE 07 gameplay targets. |
| 07 - Superscript & Subscript Example | `Assets/TextMesh Pro/Examples & Extras/Scenes/07 - Superscript & Subscript Example.unity` | LEGACY | Vendor/example scene, not a game scene. | Exclude from WAVE 07 gameplay targets. |
| 08 - Improved Text Alignment | `Assets/TextMesh Pro/Examples & Extras/Scenes/08 - Improved Text Alignment.unity` | LEGACY | Vendor/example scene, not a game scene. | Exclude from WAVE 07 gameplay targets. |
| 09 - Margin Tag Example | `Assets/TextMesh Pro/Examples & Extras/Scenes/09 - Margin Tag Example.unity` | LEGACY | Vendor/example scene, not a game scene. | Exclude from WAVE 07 gameplay targets. |
| 10 - Bullets & Numbered List Example | `Assets/TextMesh Pro/Examples & Extras/Scenes/10 - Bullets & Numbered List Example.unity` | LEGACY | Vendor/example scene, not a game scene. | Exclude from WAVE 07 gameplay targets. |
| 11 - The Style Tag | `Assets/TextMesh Pro/Examples & Extras/Scenes/11 - The Style Tag.unity` | LEGACY | Vendor/example scene, not a game scene. | Exclude from WAVE 07 gameplay targets. |
| 12 - Link Example | `Assets/TextMesh Pro/Examples & Extras/Scenes/12 - Link Example.unity` | LEGACY | Vendor/example scene, not a game scene. | Exclude from WAVE 07 gameplay targets. |
| 12a - Text Interactions | `Assets/TextMesh Pro/Examples & Extras/Scenes/12a - Text Interactions.unity` | LEGACY | Vendor/example scene, not a game scene. | Exclude from WAVE 07 gameplay targets. |
| 13 - Soft Hyphenation | `Assets/TextMesh Pro/Examples & Extras/Scenes/13 - Soft Hyphenation.unity` | LEGACY | Vendor/example scene, not a game scene. | Exclude from WAVE 07 gameplay targets. |
| 14 - Multi Font & Sprites | `Assets/TextMesh Pro/Examples & Extras/Scenes/14 - Multi Font & Sprites.unity` | LEGACY | Vendor/example scene, not a game scene. | Exclude from WAVE 07 gameplay targets. |
| 15 - Inline Graphics & Sprites | `Assets/TextMesh Pro/Examples & Extras/Scenes/15 - Inline Graphics & Sprites.unity` | LEGACY | Vendor/example scene, not a game scene. | Exclude from WAVE 07 gameplay targets. |
| 16 - Linked text overflow mode example | `Assets/TextMesh Pro/Examples & Extras/Scenes/16 - Linked text overflow mode example.unity` | LEGACY | Vendor/example scene, not a game scene. | Exclude from WAVE 07 gameplay targets. |
| 17 - Old Computer Terminal | `Assets/TextMesh Pro/Examples & Extras/Scenes/17 - Old Computer Terminal.unity` | LEGACY | Vendor/example scene, not a game scene. | Exclude from WAVE 07 gameplay targets. |
| 18 - ScrollRect & Masking & Layout | `Assets/TextMesh Pro/Examples & Extras/Scenes/18 - ScrollRect & Masking & Layout.unity` | LEGACY | Vendor/example scene, not a game scene. | Exclude from WAVE 07 gameplay targets. |
| 19 - Masking Texture & Soft Mask | `Assets/TextMesh Pro/Examples & Extras/Scenes/19 - Masking Texture & Soft Mask.unity` | LEGACY | Vendor/example scene, not a game scene. | Exclude from WAVE 07 gameplay targets. |
| 20 - Input Field with Scrollbar | `Assets/TextMesh Pro/Examples & Extras/Scenes/20 - Input Field with Scrollbar.unity` | LEGACY | Vendor/example scene, not a game scene. | Exclude from WAVE 07 gameplay targets. |
| 21 - Script Example | `Assets/TextMesh Pro/Examples & Extras/Scenes/21 - Script Example.unity` | LEGACY | Vendor/example scene, not a game scene. | Exclude from WAVE 07 gameplay targets. |
| 22 - Basic Scripting Example | `Assets/TextMesh Pro/Examples & Extras/Scenes/22 - Basic Scripting Example.unity` | LEGACY | Vendor/example scene, not a game scene. | Exclude from WAVE 07 gameplay targets. |
| 23 - Animating Vertex Attributes | `Assets/TextMesh Pro/Examples & Extras/Scenes/23 - Animating Vertex Attributes.unity` | LEGACY | Vendor/example scene, not a game scene. | Exclude from WAVE 07 gameplay targets. |
| 24 - Surface Shader Example URP | `Assets/TextMesh Pro/Examples & Extras/Scenes/24 - Surface Shader Example URP.unity` | LEGACY | Vendor/example scene, not a game scene. | Exclude from WAVE 07 gameplay targets. |
| 24 - Surface Shader Example | `Assets/TextMesh Pro/Examples & Extras/Scenes/24 - Surface Shader Example.unity` | LEGACY | Vendor/example scene, not a game scene. | Exclude from WAVE 07 gameplay targets. |
| 25 - Sunny Days Example | `Assets/TextMesh Pro/Examples & Extras/Scenes/25 - Sunny Days Example.unity` | LEGACY | Vendor/example scene, not a game scene. | Exclude from WAVE 07 gameplay targets. |
| 26 - Dropdown Placeholder Example | `Assets/TextMesh Pro/Examples & Extras/Scenes/26 - Dropdown Placeholder Example.unity` | LEGACY | Vendor/example scene, not a game scene. | Exclude from WAVE 07 gameplay targets. |
| 27 - Double Pass Shader Example | `Assets/TextMesh Pro/Examples & Extras/Scenes/27 - Double Pass Shader Example.unity` | LEGACY | Vendor/example scene, not a game scene. | Exclude from WAVE 07 gameplay targets. |
| 28 - HDRP Shader Example | `Assets/TextMesh Pro/Examples & Extras/Scenes/28 - HDRP Shader Example.unity` | LEGACY | Vendor/example scene, not a game scene. | Exclude from WAVE 07 gameplay targets. |
| Benchmark (Floating Text) | `Assets/TextMesh Pro/Examples & Extras/Scenes/Benchmark (Floating Text).unity` | LEGACY | Vendor/example scene, not a game scene. | Exclude from WAVE 07 gameplay targets. |

## Recommended WAVE 07 target scenes

| Role | Scene | Path | Status |
|---|---|---|---|
| BootScene | BootScene | `MISSING` | MISSING - should be created in 07.02 or later |
| PersistentManagers | PersistentManagers | `MISSING` | MISSING - should be created in 07.02 or later |
| FarmScene | FarmScene | `Assets/_Game/Scenes/FarmScene.unity` | EXISTS |
| TownScene | TownScene | `Assets/_Game/Scenes/TownScene.unity` | EXISTS |
| CaveEntranceScene | CaveEntranceScene | `MISSING` | MISSING - should be created in 07.02 or later |
| CaveRuntimeScene | CaveScene | `Assets/_Game/Scenes/CaveScene.unity` | EXISTS AS CURRENT CAVE TARGET; rename/split decision deferred |
| HomeInteriorScene | HomeInteriorScene | `MISSING` | MISSING - should be created in 07.02 or later |

## Scenes requiring human review

| Scene | Reason |
|---|---|
| CaveScene | Decide whether it remains CaveRuntimeScene or is split with CaveEntranceScene in WAVE 07.02+. |
| BootScene | Missing scene; creation belongs to 07.02 or later. |
| PersistentManagers | Missing scene; creation belongs to 07.02 or later. |
| CaveEntranceScene | Missing scene; creation belongs to 07.02 or later. |
| HomeInteriorScene | Missing scene; creation belongs to 07.02 or later. |
