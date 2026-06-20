# Rule: Estilo & Craft em C#

Convenções gerais de craft em C# para runtime code. Esta rule trata de *como uma única classe/método se lê*; os invariantes arquiteturais (no global search, GameEventBus, save DTOs, forbidden namespaces) ficam em [unity-architecture.md](./unity-architecture.md) e não são repetidos aqui.

## Naming & shape

- Nomes orientados ao domínio, explícitos. Nada de catch-alls `Manager`/`Helper`/`Utils` quando existe um substantivo de domínio.
- Classes pequenas, uma responsabilidade clara por método. Se um método precisa de um comentário de seção para explicar um bloco, esse bloco normalmente é um método.
- `readonly` para dependências que nunca mudam após a construção; prefira `private set`/`init` a propriedades públicas mutáveis sem motivo.
- Novos tipos `sealed` por padrão; abertos para herança só quando uma relação base/subtipo é intencional e documentada.

## Sinalização de falha (use a convenção já existente no projeto — não invente uma paralela)

- **Falhas de gameplay esperadas** (cooldown ativo, target inválido, sem stamina, item ausente, save ausente): retorne um `bool` de um método `TryX(...)` e/ou exponha uma string `FailureReason` via um HUD event do `GameEventBus` — nunca um no-op silencioso, nunca uma exception como control flow. (Ver game-feel-checklist: recusas devem expor um motivo.)
- Antes de introduzir um wrapper `Result<T>`/`Either`/`Outcome`, rode a skill `system-reuse-audit` — o projeto já sinaliza falhas com `bool` + `FailureReason`; um wrapper type concorrente é um sistema paralelo que o ethos do `runtime-code-guard` proíbe.
- **Invariantes quebrados** (dependência obrigatória null, estado impossível, save version não suportada): throw ou assert — fail fast. Ver [error-handling-resilience.md](./error-handling-resilience.md) para a taxonomia completa.

## Collections & imutabilidade

- Exponha `IReadOnlyList<T>` / `IReadOnlyCollection<T>` quando o caller não pode mutar.
- Prefira `TryGet...(out var x)` a métodos que retornam `null` e forçam o caller a checar null.
- Não distribua referências a listas mutáveis internas; copie ou faça wrap.

## Async & consciência de custo por frame

- Sem `async void`, exceto event handlers de framework/Unity que o exijam.
- Sem idiomas com allocation pesado (LINQ, closures capturando locals, boxing, string interpolation por frame) em `Update`/`FixedUpdate`/`LateUpdate`/callbacks de colisão ou loops por entidade. O detalhe de hot path e o checklist autoritativo ficam com o agent `performance-auditor` e a skill `object-pooling-pattern`.
- Sem `DateTime.Now` / `UnityEngine.Random` (estado global) em lógica que precisa ser determinística ou salva — use `System.Random` seeded por sistema (skill `rng-and-determinism`).

## Enforcement

Estilo é revisado, não hard-gated: `/code-review`, `/review-non-regression`, e o agent `architecture-reviewer`. O hook `runtime-code-guard` já bloqueia o subconjunto arquitetural (search APIs, forbidden namespaces, classes duplicadas).
