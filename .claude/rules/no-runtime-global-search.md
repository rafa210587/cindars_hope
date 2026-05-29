# Rule: No Runtime Global Search

Runtime gameplay code must not use global scene searches for wiring.

## Prohibited In Runtime

- `GameObject.Find(...)`
- `FindObjectOfType(...)`
- `FindObjectsOfType(...)`
- `FindObjectsByType(...)`

## Allowed

- Editor-only tools under `Assets/_Game/Scripts/Editor/**`.
- Serialized references.
- Bootstrap references.
- Explicit injection/configuration methods.
- Local component access on the same object, such as `GetComponent<T>()`, when used narrowly.

## Required Response

If a runtime system lacks a reference, log a clear wiring error with:

- scene;
- GameObject;
- component;
- missing field;
- affected id, when relevant.

Do not mask missing wiring with silent scene searches.
