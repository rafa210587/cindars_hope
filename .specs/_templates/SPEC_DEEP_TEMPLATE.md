# Cindar's Hope — Template PROFUNDO de Spec (blueprint executável)

> **Superset anotado** do `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`. Use este para escrever specs no nível "blueprint": prescritivas no O QUÊ, com pattern nomeado por skill. Governado pela skill `spec-authoring` (rode o Gate de Profundidade antes de salvar).
> **Como usar:** copie o bloco abaixo da linha `=====`, apague as anotações `» (…)` e preencha. Nenhum campo com `<…>` pode sobrar. Nenhum `TODO`.

Princípio: **prescrever** (assinaturas, classes criar/modificar, edição por MÉTODO/bloco, pseudo-código quando não-trivial, critério com DoD literal) e **delegar** o "como" genérico do pattern à skill (`use <skill>, molde <classe existente>`). Critérios são por resultado (binários) e a Fase 0 reconfirma a realidade antes de codar.

**Profundidade por TIPO** (ver a matriz na skill `spec-authoring`): Runtime/Data/UI → §16 com assinatura + classes + testes; Validation/Docs → estrutura EXATA do entregável + paths de evidência (§16 = N/A justificado); Tooling → interface do script + exit codes; Governance → decisão + onde registra. Preencha só o que o tipo pede, mas no nível fundo.

=====================================================================

# SPEC — <Nome humano>

> **Spec ID:** `<arquivo_sem_extensao>`
> **Status:** A implementar
> **Wave:** WAVE XX — <nome>
> **Priority:** P0..P5   » (P por risco/bloqueio, não por gosto)
> **Type:** Runtime / Data / UI / Save / Integration / Validation / Tooling / Governance
> **Domain:** Core / Save / Events / Time / Quest / UI / Inventory / Farm / City / Player / Cave / Combat / Bestiary
> **Parallelizable:** YES / NO / CONDITIONAL
> **Parallel group:** <grupo ou N/A>
> **Can run with:** <specs compatíveis ou N/A>
> **Must not run with:** <specs que tocam os mesmos arquivos/contratos>
> **Repo lock scope:** <paths reais que não podem ser co-editados — ex.: `Assets/_Game/Scripts/Combat/**`>
> **Depends on:** <spec/doc + por quê>
> **Blocks:** <specs que esperam esta>
> **Scope:** <1 frase objetiva do que ENTRA>
> **Out of scope:** <1 frase do que NÃO entra>
> **Validation level alvo:** BUILD_VALIDATED | UNITY_VALIDATED | PLAYMODE  » (por `validation-truth`)
> **Executor:** Claude | Codex | qualquer  » (a spec deve ser auto-suficiente para os dois)

## 5. Contexto
» Por que existe, qual wave destrava, o que fica para specs futuras. 2–5 linhas concretas.

## 6. Problema
» O risco concreto de NÃO fazer. Binário/observável, não "melhorar o sistema".
Ex.: `43 EnemyActionSO referenciam StatusApplicationId ausente do StatusEffectDatabaseSO → o validador falha e o status nunca é aplicado em combate.`

## 7. Objetivo
» "Ao final, o projeto tem <resultado verificável>, permitindo <X>, sem alterar <fora de escopo>."

## 8. Fontes obrigatórias lidas
» Docs + rules + skills + os arquivos de código-alvo REAIS (com path). Runtime specs incluem:
`.claude/rules/testing-quality-gate.md`, `.claude/skills/spec-execution/SKILL.md`, e a(s) skill(s) do pattern usado.

## 9. Estado atual do repo — Phase 0 (auditado, NÃO "auditar depois")
» A prova de que a spec não é fabricada. Cite comandos e resultados reais:
```text
Comando: Grep "StatusApplicationId" Assets/_Game/Data/Enemies/Actions  → 43 assets
Comando: <validador/snapshot>  → <resultado>
Existe: <ClasseX.cs:linha> faz <Y> (não recriar)
Parcial: <…>   Ausente: <…>
```
» Feche com: "A Fase 0 de EXECUÇÃO deve re-rodar <comandos> — números podem ter mudado."

## 13. Regras de não duplicação
» Nomear os sistemas existentes que a spec NÃO recria (por `code-minimalism-ladder`/`system-reuse-audit`).
Ex.: `Não criar novo catálogo de status: usar StatusEffectCatalog/StatusEffectDatabaseSO existentes.`

## 15. Arquitetura alvo — classes a CRIAR vs MODIFICAR
» Diagrama textual + duas listas explícitas. 1 linha de responsabilidade por item.
```text
CRIAR:
  Assets/_Game/Scripts/<Dom>/<NovaClasse>.cs   — <responsabilidade em 1 linha; pattern: (skill: <nome>)>
MODIFICAR:
  Assets/_Game/Scripts/<Dom>/<Existente>.cs     — <o que muda e por quê>
  Assets/_Game/Data/<Dom>/<asset>.asset         — <campo/valor que muda> (autorização de asset se aplicável)
```

## 16. Contratos, dados e eventos — COM ASSINATURA
» Obrigatório para runtime. `N/A` só com justificativa. Escreva a assinatura real, não a descrição.
### 16.1 Data contracts
```csharp
// StatusEffectSO já existe; nenhum campo novo. IDs canônicos como const:
public const string StatusSlowMinor = "status_slow_minor";
```
### 16.2 Runtime contracts
```csharp
public interface IStatusApplier { void Apply(string statusId, GameObject target, float duration); }
```
### 16.3 Event contracts  » DTO com campos+tipos, ou N/A
### 16.4 Save contracts   » campos simples/IDs, sem ref Unity, ou N/A
### 16.5 UI contracts     » ViewModel/projeção, ou N/A

## 17. Sistemas afetados
» Por domínio, não só arquivos: `Combat / StatusEffect / Enemy AI / Editor validation / EditMode tests`.

## 18. Arquivos permitidos  » paths reais e específicos
```text
Assets/_Game/Scripts/Combat/**
Assets/_Game/Data/Combat/StatusEffects/**   (data asset explícito)
Assets/_Game/Tests/EditMode/Combat/**
docs/validation/**
```
## 19. Arquivos proibidos  » sempre inclui .unity/.prefab (salvo autorização), Packages/, ProjectSettings/, docs_old/

## 20. Estratégia de implementação — por fase, por EDIÇÃO (método/bloco), pattern nomeado
» Cada passo diz a edição concreta: "em `Classe.Metodo()`, após `<bloco>`, `<a mudança exata>`". Onde o algoritmo é não-trivial (fórmula, ordem, condição), incluir pseudo-código. Não basta "editar X".
```md
### Fase 0 — Auditoria: re-rodar <comandos §9>; confirmar cada ID ausente e o gerador dono do database.
### Fase 1 — Contratos/dados: adicionar consts §16.1 em <Catalog.cs>; (se gerar assets) estender <Generator.cs> — pattern: (skill: data-catalog-authoring).
### Fase 2 — Runtime/wiring: <edição exata em cada arquivo>; pattern: (skill: <nome>, molde <classe>).
### Fase 3 — Testes/validação: EditMode <NomeTest> assertando <X>; rodar <validador>.
### Fase 4 — Relatório: docs/validation/<spec_id>_execution_report.md com bloco de `validation-truth`.
```

## 21. Ordem segura de execução
» Sequência exata numerada (Fase 0 → contratos → runtime → testes → validação → relatório).

## 14. Critérios de aceite — BINÁRIOS, cada um com Definition of Done
» Cada critério: resultado observável + **DoD** = comando exato + **saída literal** esperada + número antes→depois quando aplicável. Zero prosa.
```md
### 14.1 <Critério>
- Resultado: <observável e binário>.
- DoD: `<comando>` imprime literalmente `<saída esperada>` (ex.: `ValidateEnemyAttackKits` → `0 error(s)`, era `43 error(s)`).
- Teste: `<NomeDoTest>` asserta `<X>` == `<valor esperado>`.
```
Proibido: "sistema bom", "UX agradável", "funciona melhor", "polido".

## 23. Edge cases / falhas (obrigatório, não-vazio)
» O que pode dar errado e como a spec trata. Ex.: ID colide com existente → usar `AddOrUpdate` idempotente; variante "minor" sem parent → magnitude fixa mínima; regen de cena destrutiva → registrar evidência (rule unity-assets); mudança de forma pública de um tipo → atualizar todos os call sites listados. Liste 3–6 casos reais desta spec, cada um com a mitigação.

## 22. Validação e gates
» Nível-alvo (§header) + comandos de gate. Runtime exige por `testing-quality-gate`: EditMode test, Play Mode scenario, OU residual risk documentado. Declarar Play Mode humano como `DEFERRED_TO_FINAL_VALIDATION` quando adiado — nunca como PASS sem evidência.
