# Rule: Invariantes de Arquitetura Unity

**1. Sem global scene search em runtime.** Proibido em `Assets/_Game/Scripts/**` (exceto `Editor/`): `GameObject.Find`, `FindObjectOfType`, `FindObjectsOfType`, `FindObjectsByType`. Use serialized refs, injeção via GameBootstrap ou configuração explícita. Referência ausente → logue wiring error claro, nunca mascare com scene search.

> Known debt: ~14 runtime files usam `FindObjectOfType` em `*RuntimeBootstrap` — não copie para código novo.

**2. Comunicação de gameplay entre sistemas via `GameEventBus.Publish()` / `Subscribe()`.** Chamadas diretas MonoBehaviour-to-MonoBehaviour de gameplay são proibidas. Helpers, policies e queries síncronas injetadas dentro da fronteira do owner são dependências explícitas; não converter cada chamada local em evento. Wiring, lifecycle Unity e editor tools mantêm seus contratos próprios.

**3. Save DTOs: apenas simple types + stable IDs.** Proibido: qualquer Unity ref (`ScriptableObject`, `GameObject`, `Transform`, `MonoBehaviour`, `Sprite`, components). Permitido: `string`, `int`, `float`, `bool`, enums, listas de simple values, nested DTOs, stable IDs.

**4. Namespaces proibidos:** `CindarsHope.Debug`, `CindarsHope.Temp` — use `CindarsHope.DebugTools`.

**5. Fronteiras modulares.** Código de jogo deve permanecer nas assemblies explícitas
`CindarsHope.Foundation`, `CindarsHope.Runtime`, `CindarsHope.Gameplay`, `CindarsHope.Editor` e assemblies de teste.
Foundation contém somente contratos/tipos puros e nunca referencia Unity. Dependências runtime novas
entram pelo `GameRuntimeCompositionRoot`/domain installers ou por wiring explícito; não criar novo auto-bootstrap.
Regras de domínio recebem contratos/valores necessários (DIP), sem interface por classe
nem service locator novo. Ver `solid-and-ai-context` e skill `solid-refactoring`.

**6. Ports canônicos.** Tempo usa `IGameClock`; aleatoriedade declara a finalidade Gameplay, World ou
Visual; pause usa tokens do `GameTimeScaleCoordinator`; compra que altera inventário e ouro usa uma
transação atômica; providers de save novos entram pelo registro tipado. Não reintroduzir acesso
concreto paralelo a esses caminhos.

**7. Performance e UI.** Hotspots recebem `ProfilerMarker` antes de otimização. Dispatch normal do
EventBus não pode alocar. UI nova usa Canvas + estado/projeção e atualização por evento/dirty flag;
não adicionar `OnGUI` nem polling vazio. Legado só é removido depois de busca de referências e smoke
PlayMode do fluxo afetado.

## Enforcement

Hook `runtime-code-guard.ps1` (PostToolUse) sinaliza forbidden APIs e namespaces em código recém-escrito.
