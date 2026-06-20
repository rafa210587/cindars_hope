# Rule: Invariantes de Arquitetura Unity

Consolida: `no-runtime-global-search`, `event-bus-only-gameplay-communication`, `save-dto-simple-types-only` (os originais são stubs apontando para cá).

## 1. Sem global scene search em runtime

Proibido em runtime code (`Assets/_Game/Scripts/**`, exceto `Editor/`): `GameObject.Find`, `FindObjectOfType`, `FindObjectsOfType`, `FindObjectsByType`.

Permitido: editor tools, serialized references, injeção via GameBootstrap, métodos de configuração explícitos, `GetComponent<T>()` restrito ao mesmo object.

Se uma referência estiver ausente, logue um wiring error claro (scene, GameObject, component, campo ausente, id afetado) — nunca a mascare com um scene search silencioso.

> **Known debt (decisão pendente):** ~14 runtime files usam `FindObjectOfType`, em sua maioria classes `*RuntimeBootstrap` de self-wiring (Quests, Craft, NPC/Schedule, Farm, Player/Movement, World/Scenes, Cave). Até o humano abençoar ou banir esse idiom (ver skill `runtime-bootstrap-pattern`), **não** o copie para código novo; o hook `runtime-code-guard` sinaliza novas ocorrências.

## 2. Comunicação de gameplay apenas via GameEventBus

`GameEventBus.Publish()` / `Subscribe()` para toda comunicação de gameplay (combat, farming, inventory, UI state, NPC, cave, day cycle, economy). Chamadas diretas MonoBehaviour-to-MonoBehaviour de gameplay são proibidas.

Exceções permitidas: editor tools; refs de wiring do GameBootstrap; `GetComponent` no mesmo object para setup; internals do Unity lifecycle. Uma spec pode autorizar explicitamente uma chamada direta com justificativa.

## 3. Save DTOs: apenas simple types + stable IDs

Proibido em save DTOs: qualquer Unity object reference (`ScriptableObject`, `GameObject`, `Transform`, `MonoBehaviour`, `Sprite`, components).

Permitido: `string`, `int`, `float`, `bool`, enums, lists/arrays de simple values, nested simple DTOs, stable IDs.

Pattern: persista IDs; resolva objects após o load via registries/bootstrap; mantenha migrations backward-compatible; documente compatibilidade em `docs/validation/` quando o schema mudar.

## 4. Namespaces proibidos

`CindarsHope.Debug`, `CindarsHope.Temp` (use `CindarsHope.DebugTools`).

## Enforcement

- Hook `runtime-code-guard.ps1` (PostToolUse) sinaliza forbidden search APIs e namespaces em código recém-escrito.
- `/review-non-regression` audita o diff completo antes do closeout.
