---
name: spec-authoring
description: Autora specs implementáveis PROFUNDAS (blueprint executável) — com assinaturas de contrato, classes a criar vs modificar, plano por arquivo/fase, pattern nomeado por skill e critérios de aceite binários com comando de verificação. Usar ao gerar/refinar qualquer spec em `.specs/a_implementar/`, no /start-spec, ao decompor uma wave, ou quando uma spec parecer rasa/genérica. NÃO é para executar spec (isso é `spec-execution`).
---

# Skill: Autoria de Spec Profunda

Precedente: as specs fortes do projeto (ex.: lote `spec_arch_*_residual_v1`) são blueprints — header rico, contratos nomeados, arquivos exatos, critérios com evidência. As fracas (ex.: `spec_world_002`) são bullets vagos. Esta skill eleva o **piso** para o padrão das fortes.

**Regra central: uma spec forte é um BLUEPRINT EXECUTÁVEL — prescreve o O QUÊ (assinaturas de contrato, classes a criar vs modificar, edição por arquivo/fase, critérios binários com comando) e NOMEIA o pattern via skill do projeto; NUNCA reensina o pattern (isso mora na skill). Toda spec passa pelo Gate de Profundidade abaixo antes de ir para `.specs/a_implementar/` — se falhar qualquer item, não está pronta.**

## Quando usar

- Gerar spec nova, ou refinar/reprovar uma que parece rasa
- `/start-spec`, decompor uma wave em specs, planejar um lote para Claude **ou** Codex
- Sempre que um critério de aceite for prosa ("funciona bem", "polido") em vez de binário

## O padrão de profundidade — prescrever vs delegar

| PRESCREVER na spec (parrudo, específico desta mudança) | DELEGAR (não reensinar) |
|---|---|
| Contratos com **assinatura real**: `interface`, método, DTO de evento (campos+tipos), campos de save, `const` de ID | O *como genérico* do pattern → **nomear a skill** (`use registry-catalog-pattern, molde X`) |
| Classes a **CRIAR** (nome + 1 linha de responsabilidade) vs **MODIFICAR** (arquivo + o que muda) | Racional/anatomia do pattern (já está na skill/rule) |
| Plano **por fase, por arquivo**: qual edição entra em cada path nomeado | Estilo C#, naming, hot paths → `csharp-style`/`non-regression-review` |
| Critério **binário** + **comando/asserção** que o prova | Convenções de teste → `editmode-test-authoring` |

Antídoto ao extremo oposto (spec quebradiça): o **plano** prescreve o caminho, mas os **critérios** são por resultado (binários/testáveis) e a **Fase 0 reconfirma a realidade no código antes de codar**. Se a Fase 0 divergir do plano, o executor ajusta e registra — não segue cego.

## Índice de seleção de pattern (tipo de mudança → skill que carrega o "como")

- Catálogo/registry de dados por ID → `registry-catalog-pattern`, `data-catalog-authoring`
- God-class grande → `monobehaviour-decomposition`; nova seção de save → `save-section-provider`
- Comunicação entre sistemas → `event-bus-pattern`; nova tela/HUD → `ui-projection-pattern` + `hud-canvas-binding`
- Wiring de manager/serviço em cena → `bootstrap-wiring` / `runtime-bootstrap-pattern`
- Validador de integridade de conteúdo → `editor-validator-authoring`; gerador de asset → `unity-asset-generation`
- FSM/estado → `state-machine-design`; ability/effect componível → `ability-effect-composition`
- (catálogo completo: tabela Skills no `CLAUDE.md`)

## Gate de Profundidade (reprova a spec se QUALQUER item faltar)

```
[ ] Header completo: Spec ID, Wave, Type, Domain, Priority, Parallelizable + Repo lock scope, Depends/Blocks, Scope/Out-of-scope
[ ] §9 Estado do repo escrito como Phase 0 REAL (arquivos/linhas/comandos citados), não "auditar depois" genérico
[ ] §16 Contratos com ASSINATURA (não "N/A" sem justificativa; não descrição vaga)
[ ] Lista explícita de classes a CRIAR (com responsabilidade) e a MODIFICAR (com a mudança)
[ ] §18/19 Arquivos permitidos/proibidos específicos (paths reais, não globs vazios)
[ ] §20 Estratégia por fase COM edição por arquivo + pattern NOMEADO por skill em cada sistema
[ ] Todo critério de aceite é BINÁRIO e traz comando/teste/asserção de evidência
[ ] Nível de validação declarado (BUILD_VALIDATED / UNITY_VALIDATED / Play Mode) por regra `validation-truth`
[ ] Regra de não-duplicação (§13) nomeia os sistemas existentes que NÃO recriar
[ ] Executável por Claude E Codex sem contexto desta conversa (auto-suficiente)
```

## Procedimento de autoria

1. **Phase 0 real** — rodar Grep/Read/validadores no código; preencher §9 com o estado verdadeiro (arquivos, linhas, IDs, contagens, comandos). Sem isto a spec é fabricada.
2. **Contratos primeiro** (§16) — escrever as assinaturas exatas antes do plano.
3. **Plano por arquivo/fase** (§15/17/20) — CRIAR vs MODIFICAR; nomear a pattern-skill por sistema.
4. **Critérios binários** (§14) — cada um com comando/teste que o prova; sem prosa.
5. **Header de paralelização/locks** (§3/4) — Repo lock scope real; Depends/Blocks.
6. **Rodar o Gate** acima; só então salvar em `.specs/a_implementar/`. O hook `sync-harness-and-tracing` regenera o `SPEC_INDEX` no Stop.

Base obrigatória (copiar e preencher, não escrever do zero):
- **Template profundo:** `.specs/_templates/SPEC_DEEP_TEMPLATE.md` (superset anotado do `SPEC_IMPLEMENTABLE_TEMPLATE.md`).
- **Exemplo trabalhado:** `.specs/_templates/EXAMPLE_spec_content_enemy_status_kit_ids_v1.md` (mostra o nível-alvo).

## Quando NÃO usar

- Executar/implementar uma spec já escrita → `spec-execution`
- Criar skill/rule/agent/command/hook do harness → `harness-authoring`
- Mover spec para `implementados/` com evidência → `docs-migration`
- Planejar a wave macro (quais specs existir) → `/plan-wave` + `SPEC_GENERATION_ROADMAP_MASTER.md`

## Quando parar e reportar

- Phase 0 revela que o sistema já existe/diverge do pedido → reportar antes de escrever a spec (evita spec que manda recriar)
- Escopo grande demais para execução isolada → quebrar em N specs e declarar Depends/Blocks entre elas

## Relacionados

- `.specs/_templates/SPEC_DEEP_TEMPLATE.md` — template profundo (fonte de cópia)
- `.specs/_templates/EXAMPLE_spec_content_enemy_status_kit_ids_v1.md` — exemplo no nível-alvo
- `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md` — skeleton canônico (o profundo é superset)
- `(skill: spec-execution)` — executar a spec pronta · `(skill: system-reuse-audit)` — Phase 0 anti-duplicação
- `(rule: validation-truth)` · `(rule: testing-quality-gate)` · `(rule: code-minimalism-ladder)`
