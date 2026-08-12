---
name: spec-authoring
description: Autora specs implementáveis PROFUNDAS (blueprint executável) — com assinaturas de contrato, classes a criar vs modificar, plano por EDIÇÃO real, pseudo-código quando não-trivial, pattern nomeado por skill, e critérios de aceite binários com Definition of Done (comando + saída literal esperada). Cobre profundidade por TIPO de spec (Runtime/Data/UI, Validation/Docs, Tooling, Governance). Usar ao gerar/refinar qualquer spec em `.specs/a_implementar/`, no /start-spec, ao decompor uma wave, ou quando uma spec parecer rasa. NÃO é para executar spec (isso é `spec-execution`).
---

# Skill: Autoria de Spec Profunda

Precedente: as specs fortes (lote `spec_arch_*_residual_v1`) são blueprints — header rico, contratos nomeados, arquivos exatos, critérios com evidência. As fracas (`spec_world_002`) são bullets vagos. Esta skill eleva o **piso** ao nível das fortes, para Claude **e** Codex executarem igual.

**Regra central: uma spec forte é um BLUEPRINT EXECUTÁVEL — prescreve o O QUÊ (assinaturas, classes criar vs modificar, EDIÇÃO por arquivo, pseudo-código quando não-trivial, critérios binários com Definition of Done) e NOMEIA o pattern via skill do projeto; NUNCA reensina o pattern. Toda spec passa pelo Gate de Profundidade antes de ir para `.specs/a_implementar/` — se falhar qualquer item, não está pronta.**

## Quando usar
- Gerar spec nova, ou refinar/reprovar uma rasa; `/start-spec`; decompor wave; planejar lote p/ Claude ou Codex
- Sempre que um critério for prosa ("funciona bem", "polido") em vez de binário com comando

## Profundidade por TIPO de spec (o que "profundo" exige em cada um)

| Type | Núcleo profundo obrigatório | §16 Contratos |
|---|---|---|
| **Runtime / Data / UI** | assinaturas + classes criar/modificar + EDIÇÃO por fase + testes nomeados | obrigatório |
| **Validation / Docs** | estrutura EXATA do entregável (seções/campos) + paths de evidência + DoD de "gerado" | N/A justificado |
| **Tooling** | interface do script (params/saída) + idempotência + exit codes esperados | parcial |
| **Governance** | a decisão + onde registra (ADR/game_rule) + o que passa a ser inválido | N/A justificado |

## O padrão — prescrever vs delegar

| PRESCREVER (parrudo, desta mudança) | DELEGAR (não reensinar) |
|---|---|
| Contratos com **assinatura real** (interface, método, DTO, campos de save, `const` de ID) | o *como genérico* do pattern → **nomear a skill** (`use registry-catalog-pattern, molde X`) |
| Classes a **CRIAR** (nome+responsabilidade) vs **MODIFICAR** (arquivo+o que muda) | racional/anatomia do pattern (mora na skill/rule) |
| Plano **por EDIÇÃO** (o que muda em cada método/bloco nomeado) + pseudo-código se não-trivial | estilo C#/hot paths → `csharp-style`/`non-regression-review` |
| Critério **binário + DoD** (comando + saída literal esperada) | convenções de teste → `editmode-test-authoring` |

## Camadas extras de profundidade (aplicar a TODA spec)
1. **Passo por edição:** cada fase diz a edição concreta ("em `X.Metodo()`, após `<bloco>`, iterar `<def>` chamando `<api>`"), não "editar X".
2. **DoD por critério:** o comando + a **saída literal** esperada (`ValidateEnemyAttackKits` imprime `0 error(s)`, era `43`). Números antes→depois.
3. **Testes nomeados:** cada teste com o nome do método + o que asserta + o valor esperado.
4. **Edge cases / falhas:** seção `## 23` — o que pode dar errado e como a spec trata (colisão de ID, parent ausente, regen destrutiva, mudança de forma pública).
5. **Pseudo-código:** onde o algoritmo é não-trivial (fórmula, ordem, condição), incluir o esqueleto.

## Gate de Profundidade (reprova a spec se QUALQUER item faltar)
```
[ ] Header completo: Spec ID, Wave, Type, Domain, Priority, Parallelizable + Repo lock scope, Depends/Blocks, Scope/Out-of-scope, Validation level, Executor
[ ] §9 Estado do repo = Phase 0 REAL (comandos + resultados/contagens de hoje), não "auditar depois"
[ ] Profundidade do TIPO atendida (ver matriz): code→§16 com ASSINATURA; validation/docs→estrutura exata do entregável
[ ] Classes a CRIAR (responsabilidade) e a MODIFICAR (a mudança) listadas
[ ] §20 Estratégia por fase COM edição por arquivo/método + pattern NOMEADO + pseudo-código onde não-trivial
[ ] Todo critério é BINÁRIO e traz DoD = comando + SAÍDA LITERAL esperada (com número antes→depois quando aplicável)
[ ] Testes nomeados com asserção + valor esperado (quando o tipo pede teste)
[ ] §23 Edge cases / falhas presente e não-vazio
[ ] §18/19 arquivos permitidos/proibidos específicos; nível de validação por `validation-truth`
[ ] Regra de não-duplicação (§13) nomeia os sistemas existentes que NÃO recriar
[ ] Auto-suficiente: executável por Claude E Codex sem esta conversa
```

## Procedimento de autoria
1. **Phase 0 real** — Grep/Read/validadores; preencher §9 com estado verdadeiro (arquivos, linhas, IDs, contagens, comandos).
2. **Escolher o TIPO** e aplicar a coluna certa da matriz.
3. **Contratos primeiro** (§16) — assinaturas exatas antes do plano (ou estrutura do entregável, se Validation/Docs).
4. **Plano por EDIÇÃO** (§15/17/20) — CRIAR vs MODIFICAR; pattern-skill por sistema; pseudo-código se não-trivial.
5. **Critérios binários + DoD** (§14) — comando + saída literal + número antes→depois. Preencher §23 edge cases.
6. **Header de paralelização/locks** (§3/4); rodar o **Gate**; só então salvar em `.specs/a_implementar/`. O hook `sync-harness-and-tracing` regenera o `SPEC_INDEX` no Stop.

Base obrigatória (copiar e preencher, não escrever do zero):
- **Template profundo:** `.specs/_templates/SPEC_DEEP_TEMPLATE.md`
- **Exemplo trabalhado:** `.specs/_templates/EXAMPLE_spec_content_enemy_status_kit_ids_v1.md`

## Quando NÃO usar
- Executar/implementar spec pronta → `spec-execution`
- Criar skill/rule/agent/command/hook → `harness-authoring`
- Mover spec para `implementados/` com evidência → `docs-migration`
- Planejar a wave macro (quais specs existir) → `/plan-wave` + `SPEC_GENERATION_ROADMAP_MASTER.md`

## Quando parar e reportar
- Phase 0 revela que o sistema já existe/diverge do pedido → reportar antes de escrever (evita spec que manda recriar)
- Escopo grande demais para execução isolada → quebrar em N specs com Depends/Blocks entre elas

## Relacionados
- `.specs/_templates/SPEC_DEEP_TEMPLATE.md` · `.specs/_templates/EXAMPLE_spec_content_enemy_status_kit_ids_v1.md`
- `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md` — skeleton canônico (o profundo é superset)
- `(skill: spec-execution)` · `(skill: system-reuse-audit)` · `(rule: validation-truth)` · `(rule: testing-quality-gate)` · `(rule: code-minimalism-ladder)`
