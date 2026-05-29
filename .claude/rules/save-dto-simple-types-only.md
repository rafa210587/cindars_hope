# Rule: Save DTO Simple Types Only

Save data must be portable, deterministic, and free of Unity object references.

## Prohibited In Save DTOs

- `ScriptableObject`
- `GameObject`
- `Transform`
- `MonoBehaviour`
- `Sprite`
- `Collider`
- `Rigidbody`
- Unity component references of any kind

## Allowed

- `string`
- `int`
- `float`
- `bool`
- enums
- lists/arrays of simple values
- DTOs that themselves contain only simple values
- stable IDs for assets, entities, rooms, nodes, enemies, and scenes

## Required Pattern

- Persist IDs.
- Resolve ScriptableObjects and runtime objects after load through existing registries/bootstrap.
- Keep migrations backward-compatible when schema changes.

## Validation

When adding or changing save data, inspect public fields and document compatibility in `docs/validation/`.
