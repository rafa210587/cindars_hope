# SPEC — Consulta seletiva de contexto de classes

> **Spec ID:** `spec_solid_class_context_v1`
> **Status:** CODE_COMPLETE_WITH_GLOBAL_GATES_FAILING
> **Wave:** SOLID_AI
> **Priority:** P2
> **Type:** Tooling
> **Domain:** Architecture
> **Parallelizable:** YES
> **Parallel group:** SOLID_AI
> **Can run with:** `spec_solid_inventory_save_refactor_v1`, `spec_solid_ai_harness_hardening_v1`
> **Must not run with:** outra edição dos scripts abaixo
> **Repo lock scope:** `tools/architecture/Get-ClassContext.ps1`, `tools/architecture/Test-ClassContext.ps1`
> **Depends on:** PowerShell 7 com Roslyn incluído; csproj gerados determinam símbolos condicionais
> **Blocks:** nenhum fluxo de gameplay
> **Scope:** consulta e exportação de fatos sintáticos por tipo, documentação XML existente e testes do contrato.
> **Out of scope:** cabeçalhos repetitivos em massa, dependências semânticas, métricas de tokens sem medição e modificações de assets.
> **Validation level alvo:** BUILD_VALIDATED condicionado ao strict; testes tooling reportados separadamente
> **Executor:** Codex ou Claude
> **Ordem de execucao:** parser e contrato, testes, consulta real e revisão
> **Depende de:** auditoria e Roslyn do PowerShell 7
> **Bloqueia:** documentação da consulta de contexto SOLID_AI

required_adrs: []
required_game_rules: []

# /speckit.specify

## 5. Contexto

O pedido humano inclui avaliação crítica de frontmatter em toda classe para facilitar trabalho com AI. Foi recomendada consulta gerada por classe com XML útil; pergunta opcional não respondida durante a auditoria, portanto execução seguiu a recomendação comunicada.

## 6. Problema

Duplicar nome, imports, dependências e métodos em milhares de comentários aumenta contexto e risco de drift. Regex não distingue declarações de texto, comentários e tipos aninhados.

## 7. Objetivo

Cada declaração de tipo na configuração atual pode ser localizada por nome/módulo, incluindo partes de classes partial. Sem filtros o comando retorna resumo, sem despejar o índice no prompt.

## 8. Fontes

`AGENTS.md`, `CURRENT_STATE.md`, `solid-and-ai-context`, `harness-authoring`, asmdefs e csproj do checkout; auditoria `docs/validation/SOLID_AI_PROJECT_AUDIT.md`.

## 9. Phase 0 e estado observado

Inventário físico pós-slice: 1698 arquivos Scripts+Tests. Consulta Roslyn: 2633 declarações, 2618 tipos distintos, 1120 declarações com summary XML. Parser do PowerShell 7 já disponível; nenhum pacote instalado. Primeiro protótipo foi exercitado durante análise, e este documento registra seu contrato de execução e validação, sem atribuir aprovação prévia à spec.

## 13. Não duplicação

Usar Roslyn incluído no PowerShell; nenhuma biblioteca de parser própria, ferramenta de build nova ou catálogo manual paralelo. Ratchet e snapshot modular existentes permanecem responsáveis por seus diagnósticos.

# /speckit.plan

## 15. Criar / modificar

- CRIAR `Get-ClassContext.ps1`: extrair fatos de sintaxe, ler XML e resolver a asmdef ancestral.
- CRIAR `Test-ClassContext.ps1`: fixtures efêmeras e testes de contrato do índice.
- Nenhuma classe de gameplay nova para a consulta.

## 16. Contrato tooling

```powershell
Get-ClassContext.ps1 [-ProjectRoot path] [-Type pattern] [-Module pattern] `
  [-IncludeTests] [-Members] [-Summary] [-MaxResults 10] [-OutputPath path.jsonl]
```

Registro JSONL: symbol, kind, module, assembly, path, line, endLine, partial, bases, summary, remarks, sourceHash, preprocessorSource; members somente com opt-in. Símbolos aninhados usam `+`, aridade genérica é explícita. Partial mantém um registro por localização. Exportação ordenada e sem timestamp, idempotente. Sem arquivo persistente consultado por padrão: sempre lê o checkout.

## 17. Sistemas afetados

Navegação de código para AI e revisão arquitetural. Sem save/event/UI contract.

## 18. Arquivos permitidos

Os dois scripts de §15, esta spec, `docs/architecture/AI_CODE_CONTEXT.md` e execução/audit em `docs/validation/`.

## 19. Proibidos

Não escrever em Assets, scenes, prefabs, data assets, Packages, ProjectSettings ou docs_old. Fixtures isoladas sob diretório temporário com checagem de containment antes da remoção.

# /speckit.tasks

## 20. Edições e ordem

1. Resolver asmdef ancestral e defines do csproj correspondente; parser Roslyn percorre declarações de classe/record/struct/interface/enum.
2. Construir nomes qualificados e extrair summary/remarks sem inventar descrição quando ausente.
3. Filtrar saída, limitar resultados e exportar JSONL somente quando solicitado.
4. Testar fixtures e consultar checkout inteiro, sem instalar dependências nem alterar arquivos Unity.

## 21. Ordem segura

Parser → contrato de registros → filtros/exportação → testes → índice real → documentação e review.

## 14. Aceite binário

- `pwsh -File tools/architecture/Test-ClassContext.ps1` imprime `CLASS_CONTEXT_TESTS_PASS: 15 contracts`, exit 0.
- Fixture: tipos em strings/comentários/branch inativo não aparecem; generic partial mantém duas localizações; nested inclui parent; assembly/linha conferem.
- XML preserva entities e see references; members omite bodies/field initializers, preserva defaults de parâmetros e membros implicitamente públicos de interfaces.
- IncludeTests é opt-in; MaxResults limita saída; exportações repetidas têm mesmo SHA256.
- OutputPath em Assets é recusado; erro sintático falha sem sobrescrever índice anterior.
- `Get-ClassContext.ps1 -Type InventoryManager` retorna owner correto e sourceHash; `-Type SaveManager` retorna as partes.

## 23. Edge cases

Sem csproj: declarar que não há defines, nunca presumir configuração de build. Declarações em branches inativos não são cobertura de todas as configurações. Sem summary: null, sem inferência artificial. Erro sintático: falha explícita, sem relatório parcial como completo. Mais resultados que limite: warning e orientação para filtrar/exportar. Índice auxilia navegação, não substitui leitura de código/callers nem prova SOLID.

## 22. Validação

Testes de contrato e consulta real completos. Docs/strict globais continuam sujeitos ao baseline de falhas do projeto; não esconder FAIL nem promover a spec sem evidência.
