# Cindar's Hope — Spec Implementable Template

> **Status:** template canônico de geração de specs implementáveis.  
> **Local:** `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`  
> **Tipo:** guia/template, não é spec implementável.  
> **Função:** padronizar como novas specs em `.specs/a_implementar/` devem ser escritas para execução futura por Codex ou Claude Code.  
> **Regra:** este arquivo deve ser lido antes de gerar qualquer nova spec. Não deve ser executado como tarefa de implementação.
> **Profundidade obrigatória:** este é o skeleton. Para o nível "blueprint executável" (assinaturas, classes criar/modificar, plano por arquivo/fase, pattern nomeado, critério binário com comando), use a skill `spec-authoring` + o superset `.specs/_templates/SPEC_DEEP_TEMPLATE.md` e o exemplo `.specs/_templates/EXAMPLE_spec_content_enemy_status_kit_ids_v1.md`. Rode o Gate de Profundidade da skill antes de salvar em `a_implementar/`.

---

## 0. Regra principal

Este documento define a estrutura mínima obrigatória para specs implementáveis novas.

Ele não substitui:

```text
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
.specs/SPEC_GENERATION_ROADMAP_MASTER.md
.specs/SPEC_SOURCE_OF_TRUTH.md
.specs/SPEC_REGISTRY_TO_IMPLEMENT.md
.claude/rules/testing-quality-gate.md
```

Toda nova spec deve:

```text
ser pequena o suficiente para execução isolada;
declarar fontes obrigatórias;
declarar estado real do repo;
declarar se pode rodar em paralelo;
declarar arquivos permitidos/proibidos;
declarar critérios de aceite verificáveis;
declarar validação automatizada e Unity;
não depender de validação humana intermediária;
preparar evidência para validação humana final quando necessário.
```

---

## 1. Quem deve gerar specs

### Melhor agente para gerar specs

Para geração e refinamento de specs, a preferência é:

```text
1. ChatGPT / GPT reasoning
   Melhor para estruturar direção, coerência entre documentos, decomposição de escopo, dependências e critérios de aceite.

2. Claude Code
   Bom para revisar spec contra o repo real, apontar arquivos existentes, dependências e riscos de execução.

3. Codex
   Melhor para executar implementação baseada em spec clara; não é a primeira escolha para desenhar spec macro ou decompor produto.
```

Regra prática:

```text
ChatGPT gera/refina a spec.
Claude Code pode revisar contra o repo antes da execução.
Codex/Claude Code executam depois, com escopo fechado.
```

---

## 2. Validação humana

Não pedir validação humana spec a spec.

A validação humana do usuário deve acontecer apenas no final de um bloco/lote combinado ou no final de tudo, conforme decisão do usuário.

Durante criação/execução de specs:

```text
Pode criar cenário de validação final.
Pode documentar passos para Play Mode final.
Pode marcar validação humana como DEFERRED_TO_FINAL_VALIDATION.
Não deve bloquear micro-spec exigindo que o usuário abra Unity a cada etapa.
Não deve dizer que Play Mode humano passou sem evidência final.
```

Status recomendado quando uma spec runtime precisa de Play Mode mas a validação humana foi adiada:

```text
BUILD_VALIDATED ou UNITY_VALIDATED, conforme evidência automatizada disponível.
Nunca ACCEPTED por validação humana se ela não ocorreu.
```

---

## 3. Paralelização obrigatória

Toda spec nova deve declarar se pode ser executada em paralelo.

Formato obrigatório no cabeçalho:

```md
> **Parallelizable:** YES / NO / CONDITIONAL
> **Parallel group:** <nome do grupo ou N/A>
> **Can run with:** <lista de specs/waves compatíveis>
> **Must not run with:** <lista de specs/waves conflitantes>
> **Repo lock scope:** <arquivos/sistemas que não podem ser editados em paralelo>
```

Critérios:

```text
YES:
  Pode rodar em paralelo porque não altera os mesmos arquivos, contratos, eventos, save sections ou cenas.

NO:
  Não pode rodar em paralelo porque altera contratos centrais, save schema, event bus, bootstrap, cenas compartilhadas ou registries globais.

CONDITIONAL:
  Pode rodar em paralelo apenas se os agentes não editarem os mesmos arquivos/sistemas e se dependências estiverem fechadas.
```

Exemplos:

```text
Spec de UI inventory pode rodar em paralelo com spec de weather backend se não tocar HUD/global input.
Spec de save section ownership não deve rodar em paralelo com specs que alteram save schema.
Spec de event contracts não deve rodar em paralelo com specs que criam eventos dependentes dela.
Spec de data assets pode rodar em paralelo se não editar registry global compartilhado.
```

---

## 4. Cabeçalho obrigatório da spec

Toda nova spec deve começar com:

```md
# SPEC — <Nome humano da spec>

> **Spec ID:** `<nome_do_arquivo_sem_extensao>`  
> **Status:** A implementar  
> **Wave:** WAVE XX — <nome da wave>  
> **Priority:** P0 / P1 / P2 / P3 / P4 / P5  
> **Type:** Governance / Runtime / Data / UI / Validation / Save / Integration / Tooling  
> **Domain:** Core / Save / Events / Time / Quest / UI / Inventory / Farm / City / Player / Cave / Combat / Bestiary  
> **Parallelizable:** YES / NO / CONDITIONAL  
> **Parallel group:** <grupo ou N/A>  
> **Can run with:** <lista>  
> **Must not run with:** <lista>  
> **Repo lock scope:** <arquivos/sistemas bloqueados>  
> **Depends on:**  
> - `<spec/documento>`  
> **Blocks:**  
> - `<specs/waves bloqueadas>`  
> **Scope:** <1 frase objetiva>  
> **Out of scope:** <1 frase objetiva>
```

---

# /speckit.specify

## 5. Contexto

Explicar por que esta spec existe, qual problema resolve e qual wave do roadmap ela destrava.

Deve citar:

```text
roadmap/wave;
documentos fonte;
estado atual do repo;
por que o escopo é pequeno;
o que será deixado para specs futuras.
```

---

## 6. Problema

Descrever o risco concreto de não implementar esta spec.

Evitar formulações vagas.

Ruim:

```text
Precisamos melhorar o sistema.
```

Bom:

```text
Sem stable IDs centralizados, specs de save/load, inventory, quest e bestiary podem persistir identificadores inconsistentes e quebrar restore entre cenas.
```

---

## 7. Objetivo

Definir o resultado esperado em termos verificáveis.

Formato recomendado:

```text
Ao final desta spec, o projeto deve ter <resultado>, permitindo <sistema dependente>, sem alterar <fora de escopo>.
```

---

## 8. Fontes obrigatórias lidas

Listar todos os documentos que o agente deve ler antes de executar.

Sempre incluir:

```text
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
.specs/SPEC_GENERATION_ROADMAP_MASTER.md
```

Adicionar fontes específicas do domínio conforme `SPEC_SOURCE_MAP.md`.

Para runtime/code specs, incluir:

```text
.claude/rules/testing-quality-gate.md
.claude/skills/spec-execution/SKILL.md
.claude/skills/unity-validation/SKILL.md
```

---

## 9. Estado atual do repo

Obrigatório.

Deve responder:

```text
Quais arquivos/sistemas já existem?
Quais estão completos?
Quais estão parciais?
Quais não existem?
O que esta spec não deve recriar?
Quais arquivos devem ser auditados antes da implementação?
```

Quando o estado real não puder ser confirmado, escrever:

```text
Estado real precisa ser auditado na Phase 0 antes de implementação. Não recriar sistema existente sem confirmar ausência no repo.
```

---

## 10. User stories / engineering stories

Usar histórias curtas e técnicas.

Exemplo:

```text
Como agente executor, quero um registry único de stable IDs para não criar IDs divergentes por domínio.
Como maintainer, quero validação de ID duplicado para evitar save quebrado.
Como sistema de save, quero persistir apenas IDs simples e não referências Unity.
```

---

## 11. Escopo

Lista explícita do que entra.

Formato:

```text
Inclui:
- item verificável 1;
- item verificável 2;
- item verificável 3.
```

---

## 12. Fora de escopo

Lista explícita do que não entra.

Obrigatório para impedir expansão de escopo.

Formato:

```text
Não inclui:
- refactor massivo de sistemas existentes;
- alteração de cenas não listadas;
- balance final;
- arte final;
- validação humana imediata.
```

---

## 13. Regras de não duplicação

Declarar quais sistemas a spec não deve recriar.

Exemplo:

```text
Não recriar GameEventBus se já existir.
Não criar novo SaveManager se o atual for suficiente.
Não criar segundo registry se já houver registry canônico.
Não criar namespace paralelo para resolver conflito local.
```

---

## 14. Critérios de aceite

Critérios devem ser verificáveis por código, teste, validação ou inspeção documental.

Formato:

```md
### 14.1 Critério A

- Resultado esperado 1.
- Resultado esperado 2.
- Evidência esperada: <arquivo/teste/comando/log>.
```

Não usar:

```text
sistema bom;
UX agradável;
funciona melhor;
polido.
```

---

# /speckit.plan

## 15. Arquitetura alvo

Descrever módulos, classes, assets, docs, scripts ou validações esperadas.

Quando possível, usar diagrama textual:

```text
Assets/_Game/Scripts/<Domain>/
  <ClassA>.cs
  <ClassB>.cs

Assets/_Game/Data/<Domain>/
  <DataAsset>.asset

docs/validation/
  <spec_id>_execution_report.md
```

---

## 16. Contratos, dados e eventos

Obrigatório para runtime specs.

Separar em:

```md
### 16.1 Data contracts
### 16.2 Runtime contracts
### 16.3 Event contracts
### 16.4 Save contracts
### 16.5 UI contracts
```

Se não aplicável, escrever `N/A` com justificativa.

---

## 17. Sistemas afetados

Listar sistemas por domínio, não só arquivos.

Exemplo:

```text
Save/load
Inventory
Quest rewards
Event bus
HUD
Unity scene wiring
Validation reports
```

---

## 18. Arquivos permitidos

Listar paths permitidos.

Ser específico.

```text
Assets/_Game/Scripts/<Domain>/**
Assets/_Game/Tests/EditMode/<Domain>/**
docs/validation/**
```

---

## 19. Arquivos proibidos

Obrigatório.

Sempre considerar:

```text
Assets/**/*.unity salvo autorização explícita
Assets/**/*.prefab salvo autorização explícita
Assets/**/*.asset salvo data asset explícito
docs_old/**
docs/archive/**
Packages/**
ProjectSettings/**
```

Para docs-only specs:

```text
Assets/**
Packages/**
ProjectSettings/**
```

---

## 20. Estratégia de implementação

Quebrar em fases pequenas.

Formato:

```md
### Fase 0 — Auditoria
### Fase 1 — Contratos/dados
### Fase 2 — Runtime
### Fase 3 — Testes/validação
### Fase 4 — Relatório
```

A Fase 0 deve impedir recriação de sistema existente.

---

## 21. Ordem segura de execução

Listar a sequência exata.

Exemplo:

```text
1. Auditar arquivos existentes.
2. Criar contratos mínimos.
3. Adicionar validação.
4. Integrar sem alterar comportamento fora do escopo.
5. Rodar testes/compile/docs validation.
6. Registrar relatório.
```

---

## 22. Paralelização

Obrigatório.

Formato:

```md
## Paralelização

- Parallelizable: YES / NO / CONDITIONAL
- Parallel group: <nome>
- Can run with:
  - <spec>
- Must not run with:
  - <spec>
- Shared files/systems that require lock:
  - <path/sistema>
- Reason:
  - <justificativa>
```

---

## 23. Impacto em save/load

Declarar:

```text
Does this change save schema? YES/NO
Does this add a save section? YES/NO
Does this require migration? YES/NO
Does this persist Unity references? MUST BE NO
```

Se `YES`, explicar seção, owner e restore order.

---

## 24. Impacto em eventos

Declarar:

```text
Adds events: YES/NO
Changes existing events: YES/NO
Requires unsubscribe pattern: YES/NO
```

Listar eventos novos/alterados.

---

## 25. Impacto em UI/Unity

Declarar:

```text
Changes UI: YES/NO
Changes scenes: YES/NO
Changes prefabs: YES/NO
Changes ScriptableObjects/assets: YES/NO
Requires Play Mode final validation: YES/NO
Human validation timing: DEFERRED_TO_FINAL_VALIDATION / NOT REQUIRED
```

---

## 26. Riscos técnicos

Listar riscos concretos.

Exemplo:

```text
Risco: alterar save schema sem migration.
Mitigação: migration explícita e EditMode test de round-trip.
```

---

## 27. Rollback

Explicar como desfazer.

```text
Remover arquivos criados.
Reverter alterações em contracts.
Manter dados existentes intactos.
Não apagar save real do usuário.
```

---

# /speckit.tasks

## 28. Tasks

Tasks devem ser pequenas, rastreáveis e marcáveis.

Formato:

```md
- [ ] T001 — Auditar arquivos existentes de <domínio>.
- [ ] T002 — Criar/ajustar <contrato>.
- [ ] T003 — Adicionar validação <x>.
- [ ] T004 — Criar/atualizar testes <x>.
- [ ] T005 — Gerar execution report.
```

Não criar task genérica como:

```text
Implementar sistema todo.
```

---

## 29. Validações obrigatórias

Toda spec deve listar comandos.

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Runtime C# quando aplicável:

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
```

Editor C# quando aplicável:

```powershell
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
```

Unity compile quando aplicável:

```powershell
.\tools\unity\RunUnityCompileValidation.ps1 -ProjectPath "." -LogFile ".\Logs\unity-compile-validation.log"
.\tools\unity\ScanUnityLogs.ps1 -LogFile ".\Logs\unity-compile-validation.log"
```

EditMode/PlayMode automated quando harness existir:

```text
Unity Test Runner — EditMode
Unity Test Runner — PlayMode
```

Se algum comando não puder rodar, o report deve registrar `NOT RUN` com motivo e risco residual.

---

## 30. Testing Quality Gate

Obrigatório para toda spec.

Template:

```md
## Testing Quality Gate

- Changed deterministic logic: YES/NO
- Requires EditMode tests: YES/NO
- Requires PlayMode automated or final human scenario: YES/NO
- Requires regression test: YES/NO
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION / NOT REQUIRED
- Minimum validation evidence for ACCEPTED: <text>
```

Regras:

```text
Compile/build não é suficiente para ACCEPTED em spec runtime/gameplay.
Mudança de lógica determinística exige EditMode tests quando praticável.
Mudança de UI/cena/input/prefab exige PlayMode automatizado ou cenário final humano documentado.
Bugfix exige regression test ou risco residual documentado.
Sem teste obrigatório nem justificativa, status máximo é PARTIAL.
Runtime/gameplay sem PlayMode automatizado nem cenário final humano documentado fica no máximo BUILD_VALIDATED.
A validação humana real fica deferida para o final, salvo autorização explícita do usuário.
```

Para docs-only/governance specs:

```md
## Testing Quality Gate

- Changed deterministic logic: NO
- Requires EditMode tests: NO
- Requires PlayMode automated or final human scenario: NO
- Requires regression test: NO
- Human validation timing: NOT REQUIRED
- Minimum validation evidence for ACCEPTED: docs validation PASS; links/paths consistent; no runtime/code changed.
```

---

## 31. Definition of Done

Definir DoD realista.

Exemplo:

```text
Spec implementada dentro dos arquivos permitidos.
Nenhum arquivo proibido alterado.
Docs validation executada ou NOT RUN com motivo.
Unity compile executado quando aplicável ou NOT RUN com motivo.
EditMode tests adicionados quando exigidos.
Cenário de validação final criado quando Play Mode humano for necessário.
Execution report criado.
Sem claim de ACCEPTED sem evidência.
```

---

## 32. Anti-regressão

Listar o que não pode quebrar.

Exemplo:

```text
Não alterar IDs existentes sem migration.
Não alterar evento público sem atualizar consumidores.
Não serializar referência Unity em save.
Não usar GameObject.Find em runtime.
Não criar fluxo UI que deixe gameplay input ativo durante modal.
```

---

## 33. Notas para execução posterior

Registrar pendências futuras.

Exemplo:

```text
Esta spec não implementa UI final.
Esta spec não executa validação humana imediata.
Esta spec cria cenário para validação final do lote.
Esta spec deve ser revisada antes de execução em paralelo com specs que alterem o mesmo registry.
```

---

## 34. Checklist final da spec pronta

Antes de criar uma spec em `.specs/a_implementar/`, conferir:

```text
[ ] Tem cabeçalho completo.
[ ] Declara Parallelizable / Parallel group / locks.
[ ] Declara fontes obrigatórias lidas.
[ ] Declara estado atual do repo.
[ ] Tem escopo pequeno.
[ ] Tem fora de escopo explícito.
[ ] Tem regras de não duplicação.
[ ] Tem arquivos permitidos e proibidos.
[ ] Tem contratos/dados/eventos/save/UI quando aplicável.
[ ] Tem critérios de aceite verificáveis.
[ ] Tem validações obrigatórias.
[ ] Tem Testing Quality Gate.
[ ] Marca validação humana como DEFERRED_TO_FINAL_VALIDATION quando aplicável.
[ ] Tem riscos e rollback.
[ ] Não pede execução humana intermediária.
[ ] Não altera SPEC_EXECUTION_ORDER.md como se a spec já estivesse implementada.
```

---

## 35. Regra para geração em lote

Mesmo quando várias specs forem geradas em sequência, cada arquivo deve ser criado separadamente.

Após cada spec criada:

```text
Atualizar SPEC_REGISTRY_TO_IMPLEMENT.md se a spec for concreta e aprovada para backlog.
Não atualizar SPEC_EXECUTION_ORDER.md até autorização explícita de execução.
Não mover nada para implementados.
Não criar arquivos em Assets/ durante geração de spec.
```

---

## 36. Prompt mínimo para revisar spec antes de executar

```text
Estamos no repo rafa210587/cindars_hope, branch dev.

Revise a spec abaixo apenas como contrato de execução.
Não implemente código.
Verifique:
- se segue .specs/SPEC_IMPLEMENTABLE_TEMPLATE.md;
- se tem escopo pequeno;
- se declara paralelização;
- se tem arquivos permitidos/proibidos;
- se não pede validação humana intermediária;
- se o Testing Quality Gate está coerente;
- se há risco de duplicar sistema existente.

Responda com:
1. OK para executar / precisa ajuste;
2. riscos;
3. lacunas;
4. arquivos que exigem lock;
5. specs que podem rodar em paralelo.
```
