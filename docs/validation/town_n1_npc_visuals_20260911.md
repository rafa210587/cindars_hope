# TOWN.N1 — NPC visuals (N-1)

Data: 2026-09-11  
Escopo: somente `npc_corvus` e `npc_vaalara_wanderer_01`.

## Fonte e round-trip

- Aseprite `1.3.18.5-x64`, `E:\SteamLibrary\steamapps\common\Aseprite\Aseprite.exe`.
- Fontes: `art/town-keyart-rework/npc/*_walk.aseprite`.
- Cada fonte reaberta/exportada com 25 frames 150×150, tags `down`, `downleft`, `right`, `up`, `upleft` e camadas de render/notas/contrato.
- PNGs finais: `Assets/_Game/Resources/NpcWalkSprites/{npc_corvus,npc_vaalara_wanderer_01}_walk.png`.
- Bases: `Assets/_Game/Resources/NpcSprites/{npc_corvus,npc_vaalara_wanderer_01}.png`.

## Integração persistente

- Método: `CindarsHope.Editor.NPC.IntegrateN1NpcVisuals.Execute`.
- Unity: `6000.5.7f1`.
- Log: `Logs/town_n1_npc_visuals_integration4.log`.
- Resultado: `PASS`, dois NPCs integrados; GUIDs dos PNGs preservados.
- `NpcDataSO.BodySprite` e `WalkAnimResourcesPath` atualizados somente para os dois IDs.
- Sem alterações em schedules, navegação, cenas, save, Farm ou Cave.

## Settings verificados

| ID | Slices | PPU | Mode | Filter | Mipmaps | Alpha | Compression | Base pivot |
|---|---:|---:|---|---|---|---|---|---|
| `npc_corvus` | 25/25 | 234 | Multiple | Point | off | on | Uncompressed | BottomCenter |
| `npc_vaalara_wanderer_01` | 25/25 | 234 | Multiple | Point | off | on | Uncompressed | BottomCenter |

Validator: `CindarsHope.Editor.NPC.ValidateNpcWalkAnimations.Validate`  
Log: `Logs/town_n1_npc_visuals_validate.log`  
Resultado: `NPCs checados: 29 | Erros: 0`.

Playback real de `NpcWalkAnimator` em Play Mode: **NOT RUN** nesta fatia; o validator comprova recursos, nomes, slicing e settings, mas não substitui observação temporal em cena.
