# Rule: Error Handling & Resiliência

Classifique toda falha antes de tratá-la. A estratégia de tratamento é ditada pela categoria, e as categorias mapeiam diretamente para sistemas que já existem neste projeto.

## As quatro categorias

| # | Categoria | Exemplos | Tratamento neste projeto |
|---|----------|----------|--------------------------|
| 1 | **Gameplay esperado** | ação inválida, cooldown, sem stamina, item ausente, target fora de alcance | `bool TryX` + string `FailureReason` exposta via um HUD event do `GameEventBus` (skill `game-feel-checklist`). Nunca uma exception, nunca um no-op silencioso. |
| 2 | **Config / asset / content** | prefab ausente, item sem icon, quest aponta para id inválido, campo de SO não preenchido | Capture em **editor/build time** com um validator (pattern de 60 validators, skill `editor-validator-authoring`); em runtime aplique um fallback seguro e logado. Corrupção de config obrigatória faz fail fast em dev. |
| 3 | **Infraestrutura** | escrita de save falhou, arquivo corrompido/ausente no load | Atomic save flow + validação pós-load + recovery (skill `save-load-pattern`). Nunca sobrescreva um save bom antes de o novo validar. |
| 4 | **Bug / invariante quebrado** | dependência obrigatória null, estado impossível, save version não suportada | **Fail fast** em desenvolvimento (throw/assert) com um log de wiring claro; não mascare com um scene search ou uma exception engolida (rule `unity-architecture`). |

## Logs precisam carregar contexto

Uma linha de log precisa permitir que um humano localize a falha sem um debugger. Inclua: sistema, entity/id, scene/level, operação, estado relevante, e o fallback aplicado (se houver). Proíba linhas peladas como `"Error loading data"`. Para referências obrigatórias ausentes, logue scene + GameObject + component + missing field + affected id (rule `unity-architecture`).

## Desenvolvimento vs. shipped build

- **Desenvolvimento:** falhe cedo para expor bugs (categoria 4 dá throw/assert).
- **Shipped:** recupere onde categoria 1–3 permite, mas sempre logue com contexto.
- **Nunca** esconda um erro corrompendo estado silenciosamente (ex.: escrevendo um save parcial, continuando com um null que vai dar NRE três frames depois).

## O que nunca acontece

- Exception usada como control flow normal de gameplay (isso é categoria 1 → `bool`/`FailureReason`).
- `catch { }` que engole e continua (categoria 4 precisa fail fast; 2–3 precisam log + fallback).
- Um "fallback" que produz um estado inválido em vez de um default seguro (rule `data-driven-content`: prefira Null Object / default seguro).

## Enforcement

Revisado pela skill `non-regression-review`, `/code-review`, e o agent `bugfix-investigator` (que também exige um regression test conforme a rule `testing-quality-gate`). Sem hook mecânico — esta é uma lente de review.
