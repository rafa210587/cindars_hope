# Human Test Scenario — Farm Scene Collision Navigation

Status: NOT RUN — Unity Editor is unavailable locally.

## Setup

1. Open the project in Unity and regenerate FarmScene through the canonical scene-creation menu.
2. Run `CindarsHope/Validar Navegacao FarmScene` and confirm both `0 orphan collider(s)` and `Reachable required landmarks: 10/10`.
3. Enter Play Mode from the generated FarmScene with the default spawn.

## Route

1. Walk from spawn to the house door, shipping bin and sell point; each must be reachable.
2. Cross the bridge once in each direction. The player must not collide in the declared corridor.
3. Attempt to enter river water, the lake body and the northern mountain at three distinct points. Each attempt must block movement.
4. Walk to the fishing edge, crafting stations, both animal-building approaches, Fonte, cave entrance and town exit.
5. Verify no interaction trigger behaves as a solid wall and the Console contains no new errors.

## Pass checklist

- [ ] Validator reports 0 orphan colliders and 10/10 reachable landmarks.
- [ ] River, lake and mountain block movement.
- [ ] Bridge corridor is continuously passable.
- [ ] All required approaches are reachable from default spawn.
- [ ] No Player/input regression or Console error.
