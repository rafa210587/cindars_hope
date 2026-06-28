# Rule: Invariantes de Arquitetura Unity

**1. Sem global scene search em runtime.** Proibido em `Assets/_Game/Scripts/**` (exceto `Editor/`): `GameObject.Find`, `FindObjectOfType`, `FindObjectsOfType`, `FindObjectsByType`. Use serialized refs, injeção via GameBootstrap ou configuração explícita. Referência ausente → logue wiring error claro, nunca mascare com scene search.

> Known debt: ~14 runtime files usam `FindObjectOfType` em `*RuntimeBootstrap` — não copie para código novo.

**2. Comunicação de gameplay apenas via `GameEventBus.Publish()` / `Subscribe()`.** Chamadas diretas MonoBehaviour-to-MonoBehaviour de gameplay são proibidas. Exceções: editor tools, wiring do GameBootstrap, `GetComponent` no mesmo object, lifecycle Unity.

**3. Save DTOs: apenas simple types + stable IDs.** Proibido: qualquer Unity ref (`ScriptableObject`, `GameObject`, `Transform`, `MonoBehaviour`, `Sprite`, components). Permitido: `string`, `int`, `float`, `bool`, enums, listas de simple values, nested DTOs, stable IDs.

**4. Namespaces proibidos:** `CindarsHope.Debug`, `CindarsHope.Temp` — use `CindarsHope.DebugTools`.

## Enforcement

Hook `runtime-code-guard.ps1` (PostToolUse) sinaliza forbidden APIs e namespaces em código recém-escrito.
