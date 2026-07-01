# Rule: Escada de Minimalismo de Código (write-less-code)

> O melhor código é o que você não escreve. Antes de implementar qualquer coisa,
> suba a escada de decisão abaixo e pare no degrau mais alto que resolve. Adaptado
> do padrão "ponytail" (decision ladder) para o harness deste projeto.

**Invariante:** Nenhum código novo é escrito sem antes percorrer a escada de decisão. Prefira **não** escrever; só implemente no último degrau, e o mínimo.

## A escada (pare no degrau mais alto que serve)

1. **YAGNI — isto precisa existir?** Não implemente requisito que a spec não pede. Sem feature especulativa, sem flag/parâmetro "pro futuro", sem camada de abstração sem segundo caso real.
2. **Reusar o que já existe.** Já há manager/service/SO/util/evento que faz isto? Rode `system-reuse-audit` (Phase 0). Nunca crie sistema paralelo — ver [gameplay-design-patterns](./gameplay-design-patterns.md) e [unity-architecture](./unity-architecture.md). Comunicação já tem `GameEventBus`; estado já tem os managers do `GameBootstrap`.
3. **Built-in da BCL / .NET.** Use `System.*`/LINQ/coleções antes de escrever utilitário próprio (respeitando hot paths — ver [csharp-style](./csharp-style.md)).
4. **Nativo da engine.** Use API do Unity (componentes, lifecycle, `ScriptableObject`, `AssetDatabase` no editor) em vez de reimplementar o que a engine já oferece.
5. **Dependência já no projeto.** Use uma lib/asset já presente antes de adicionar uma nova.
6. **One-liner / menor expressão.** A solução mais curta que resolve, sem abstração prematura.
7. **Implementação mínima nova.** Só então escreva — o mínimo, atrás dos contratos existentes (interfaces, `GameEventBus`, IDs canônicos).

## Carve-out de segurança (minimalismo NÃO se aplica a guard-rails)

"Ser mínimo" nunca justifica cortar: validação ([validation-truth](./validation-truth.md)), tratamento de erro e wiring-error claro ([error-handling-resilience](./error-handling-resilience.md)), integridade de save ([unity-architecture](./unity-architecture.md) §3), segurança ([security-and-files](./security-and-files.md)). Minimalismo é sobre **não escrever código desnecessário**, não sobre pular checagens necessárias.

## Onde se aplica

Toda tarefa de implementação/refactor neste repo. No diff final, prefira a menor mudança que satisfaz os critérios de aceite da spec.

## Enforcement

Revisional (sem hook mecânico): `system-reuse-audit` no Phase 0, `architecture-reviewer` antes de wave/integração grande, e `non-regression-review` no closeout sinalizam código que poderia ter parado num degrau mais alto (duplicação, abstração prematura, sistema paralelo).
