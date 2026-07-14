# SPEC — Checklist de Smoke Play Mode Humano (Rede de Validação Final)

> **Spec ID:** `spec_validation_human_playmode_smoke_v1`
> **Status:** A implementar
> **Wave:** WAVE VALIDATION — Rede de Validação Humana Final
> **Priority:** P1
> **Type:** Validation
> **Domain:** Core / Validation
> **Parallelizable:** YES
> **Parallel group:** docs_only_validation
> **Can run with:** qualquer spec (docs-only, não toca código/Assets)
> **Must not run with:** N/A
> **Repo lock scope:** `.specs/**`, `docs/validation/**` (nenhum lock de código)
> **Depends on (Depende de):**
> - `docs/project/CURRENT_STATE.md`
> - `.claude/rules/testing-quality-gate.md`
> **Blocks (Bloqueia):**
> - Nenhuma spec de execução — esta spec DESBLOQUEIA o fechamento final de specs de arquitetura
>   sensíveis que mexem em UI/cena/runtime visual, fornecendo o checklist que elas devem referenciar.
> **Scope:** Criar um checklist humano único, reusável, cobrindo os fluxos essenciais do jogo
> (TownScene, FarmScene, CaveScene, inventário, crafting, loja, conversa com NPC, quest offer/turn-in,
> combate básico, morte/respawn, save/load), com passos numerados, resultado esperado por passo, e um
> path de relatório padronizado para preenchimento. Esta é a REDE DE VALIDAÇÃO — o "guarda-chuva" que
> specs de arquitetura sensível devem citar como evidência de smoke final, em vez de cada spec inventar
> seu próprio cenário do zero.
> **Out of scope:** executar o checklist (é o humano quem roda, no fim do lote/wave); qualquer mudança
> de código, cena, prefab ou asset; balanceamento; conteúdo novo de gameplay.

required_adrs: []
required_game_rules: []

---

# /speckit.specify

## 5. Contexto

O projeto tem hoje dezenas de cenários de teste humano individuais em `docs/validation/playmode/`
(um por spec — `fable_NN_human_test_scenario.md`, `spec_cave_visual_polish_human_test_scenario.md`,
etc.), auditados nesta sessão via Glob (confirmados 30 arquivos existentes nessa pasta). Isso é bom
para escopo local de cada spec, mas não existe hoje um checklist ÚNICO e consolidado que cubra os
fluxos essenciais do jogo de ponta a ponta — o gap entre "código compila e passa EditMode" e "o jogo
realmente funciona visualmente quando um humano joga". Esta spec fecha esse gap: cria o checklist
guarda-chuva, sem duplicar os cenários específicos já existentes (que continuam válidos para o escopo
local deles).

Esta é, por definição, a PRIMEIRA spec de validação a existir no sentido de rede de segurança —
ela deve existir antes (ou em paralelo, por ser docs-only) de specs de arquitetura sensíveis como
`spec_arch_cave_integration_boundary_residual_v1`, que a referenciam como evidência de smoke final.

## 6. Problema

Sem um checklist consolidado, cada spec de arquitetura/refactor precisa inventar seu próprio cenário
de smoke visual do zero, o que é inconsistente, incompleto (cobre só a área tocada, não os fluxos
adjacentes que podem ter regredido silenciosamente) e não tem um path de relatório padronizado. Uma
mudança de fronteira modular (ex.: mover uma classe de um módulo para outro) pode compilar e passar
EditMode tests e ainda assim quebrar visualmente um NPC, uma tela de inventário, ou o fluxo de
save/load — sem que nenhum teste automatizado pegue isso, porque o projeto não tem PlayMode automated
tests para esses fluxos hoje.

## 7. Objetivo

Ao final desta spec, o projeto tem um checklist humano único em `docs/validation/` cobrindo TownScene,
FarmScene, CaveScene, inventário, crafting, loja, conversa com NPC, quest offer/turn-in, combate
básico, morte/respawn e save/load — com passos numerados, resultado esperado por passo, path de
relatório de saída definido, e um template de preenchimento PASS/FAIL/observação por passo. Isto
permite que specs de arquitetura sensível citem este checklist como evidência de smoke final em vez
de inventar cenário próprio, sem exigir que o humano rode o checklist a cada spec individual — a
execução real fica agendada para o fim do lote/wave.

## 8. Fontes obrigatórias lidas

```text
CLAUDE.md
docs/project/CURRENT_STATE.md
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
.claude/rules/testing-quality-gate.md
.claude/skills/gameplay-test-scenario/SKILL.md
```

## 9. Estado atual do repo (Phase 0)

```text
JÁ EXISTE: docs/validation/playmode/ com 30 arquivos de cenário humano por spec individual (confirmado
via Glob nesta sessão), ex.: fable_11_human_test_scenario.md, spec_cave_visual_polish_human_test_scenario.md,
spec_cave_decor_placement_human_test_scenario.md, village_economy_four_npcs_human_test_scenario.md,
spec_farm_till_anywhere_tilemap_human_playmode_scenario.md. Esses cenários continuam válidos para o
escopo local de cada spec — esta spec NÃO os substitui nem os invalida.

NÃO EXISTE: um checklist único consolidado cobrindo os 11 fluxos essenciais listados no objetivo desta
spec, nem um path de relatório padronizado reusável entre specs (cada cenário existente tem seu próprio
formato ad-hoc).

A skill gameplay-test-scenario já define o padrão de como um cenário humano individual deve ser escrito
(gerado por spec) — esta spec não recria esse padrão, mas produz o checklist AGREGADO que serve de rede
de segurança para o conjunto, e reusa a mesma convenção de formato (passo numerado + resultado esperado
+ PASS/FAIL) para consistência.
```

Auditoria adicional exigida na Fase 0 de execução: confirmar se `docs/validation/playmode/` ganhou
arquivos novos desde este snapshot antes de escrever o checklist, para não referenciar cenários que já
mudaram de nome/status.

## 10. User stories / engineering stories

```text
Como orquestrador de spec de arquitetura sensível, quero um checklist único para citar como evidência
de smoke final, em vez de inventar cenário próprio a cada spec.
Como humano validador, quero rodar um único checklist consolidado no fim de um lote/wave em vez de
juntar manualmente 30 cenários individuais dispersos.
Como mantenedor, quero um path de relatório padronizado para saber onde encontrar o resultado do smoke
mais recente.
```

## 11. Escopo

Inclui:
- Criar `docs/validation/playmode/PLAYMODE_SMOKE_CHECKLIST_MASTER.md` (checklist consolidado, o
  artefato principal desta spec) cobrindo, cada um com passos numerados + resultado esperado por passo:
  1. TownScene (carregar, mover, colidir com NPC/prédio, sem erro no console).
  2. FarmScene (carregar, plantar, regar, avançar dia, colher).
  3. CaveScene (carregar, entrar em um nível, revisitar o mesmo nível, confirmar ausência de reroll —
     cita a rule `cave-stable-run`).
  4. Inventário (abrir, mover item, empilhar, fechar sem travar input).
  5. Crafting (abrir estação, craftar item com ingredientes disponíveis, receber o item).
  6. Loja (abrir shop de um NPC, comprar item, vender item, saldo de ouro atualiza).
  7. Conversa com NPC (iniciar diálogo, avançar linhas, fechar diálogo).
  8. Quest offer/turn-in (aceitar quest de um NPC, completar objetivo, retornar e entregar).
  9. Combate básico (atacar um inimigo, receber dano, inimigo morre e dropa loot).
  10. Morte/respawn (player morre, respawna no ponto esperado, sem softlock).
  11. Save/load (salvar, fechar/reabrir ou recarregar, estado restaurado corretamente).
- Definir o path do relatório de saída de execução:
  `docs/validation/playmode/PLAYMODE_SMOKE_REPORT_<data>.md` (formato `YYYY-MM-DD`).
- Definir um template de preenchimento por passo: `PASS` / `FAIL` / observação livre, mais um campo de
  build/commit testado e data de execução.
- Declarar explicitamente, no próprio arquivo, que esta é a rede de validação guarda-chuva: nenhuma
  spec de arquitetura sensível que mexa em UI/cena/runtime visual deve ser marcada como implementada
  sem este smoke (ou um smoke equivalente/mais específico) quando aplicável.

Fora:
- Executar o checklist.
- Criar cenários novos por spec individual (isso é a skill `gameplay-test-scenario`, já existente).
- Automatizar qualquer um destes fluxos como PlayMode automated test (fora de escopo desta spec; se o
  projeto quiser isso no futuro, é uma spec de tooling separada).

## 12. Fora de escopo

```text
Não inclui: execução do checklist; automação PlayMode; mudança de código/cena/prefab/asset;
balanceamento; conteúdo novo de gameplay; substituir os 30 cenários individuais existentes.
```

## 13. Regras de não duplicação

```text
Não recriar os cenários individuais existentes em docs/validation/playmode/ — este checklist é agregado
e complementar, não substituto.
Não recriar o formato da skill gameplay-test-scenario — reusar a mesma convenção (passo + resultado
esperado + PASS/FAIL) para consistência.
Não criar um segundo sistema de relatório de validação — usar a pasta docs/validation/playmode/ já
canônica (ver docs-governance.md).
```

## 14. Critérios de aceite

### 14.1 Checklist consolidado existe e cobre os 11 fluxos

- `docs/validation/playmode/PLAYMODE_SMOKE_CHECKLIST_MASTER.md` existe e contém uma seção por fluxo
  listado na seção 11, cada uma com passos numerados e resultado esperado por passo.
- Evidência esperada: leitura do arquivo confirma as 11 seções presentes.

### 14.2 Path de relatório e template definidos

- O arquivo master define o path `docs/validation/playmode/PLAYMODE_SMOKE_REPORT_<data>.md` e um
  template de preenchimento (PASS/FAIL/observação por passo, campo de build/commit, data).
- Evidência esperada: seção de template presente no arquivo master, copiável para gerar um report real.

### 14.3 Declaração de guarda-chuva presente

- O arquivo master declara explicitamente que é a rede de validação final de specs de arquitetura
  sensível que tocam UI/cena/runtime visual.
- Evidência esperada: parágrafo explícito citado no arquivo.

### 14.4 Nenhum código/asset alterado

- Diff desta spec toca apenas `.specs/**` e `docs/validation/**`.
- Evidência esperada: `git status --short` (ou equivalente) mostra apenas esses paths.

## 15. Notas sobre esta ser a spec "guarda-chuva"

```text
Esta spec é intencionalmente docs-only e de baixo risco (Parallelizable: YES) porque não toca código.
Ela deve ser tratada como pré-requisito informal (não bloqueio mecânico) para o closeout de specs de
arquitetura sensíveis como spec_arch_cave_integration_boundary_residual_v1: essas specs devem citar o
PLAYMODE_SMOKE_CHECKLIST_MASTER.md (ou um cenário equivalente/mais específico) como evidência de smoke
final, em vez de inventar cenário próprio do zero.
```

---

# /speckit.plan

## 16. Arquitetura alvo

```text
docs/validation/playmode/
  PLAYMODE_SMOKE_CHECKLIST_MASTER.md   (novo — o checklist consolidado)

docs/validation/
  spec_validation_human_playmode_smoke_v1_execution_report.md (novo — evidência do closeout desta spec)
```

## 17. Contratos, dados e eventos

### 17.1 Data contracts
N/A — spec docs-only, nenhum contrato de dado.

### 17.2 Runtime contracts
N/A — nenhum código tocado.

### 17.3 Event contracts
N/A — nenhum evento novo ou alterado.

### 17.4 Save contracts
N/A — nenhum DTO novo; o fluxo de save/load é apenas um item do checklist, não uma mudança de schema.

### 17.5 UI contracts
N/A — nenhuma UI criada ou alterada; o checklist descreve fluxos de UI existentes, não os modifica.

## 18. Sistemas afetados

```text
Nenhum sistema runtime. Apenas documentação de validação (docs/validation/**).
```

## 19. Arquivos permitidos

```text
.specs/**
docs/validation/**
```

## 20. Arquivos proibidos

```text
Assets/**
Packages/**
ProjectSettings/**
docs_old/**
Assets/**/*.unity, *.prefab, *.asset
```

## 21. Estratégia de implementação

```md
### Fase 0 — Auditar docs/validation/playmode/ existente (confirmar os 30 arquivos e formato usado pela skill gameplay-test-scenario)
### Fase 1 — Escrever PLAYMODE_SMOKE_CHECKLIST_MASTER.md com as 11 seções, passos numerados, resultado esperado
### Fase 2 — Definir path de relatório e template de preenchimento dentro do master
### Fase 3 — Rodar validate_docs.ps1; escrever execution report
```

## 22. Ordem de execucao (ordem segura)

```text
1. Ler fontes obrigatórias e auditar docs/validation/playmode/ existente.
2. Escrever o checklist master com as 11 seções.
3. Definir path de relatório e template de preenchimento.
4. Rodar validate_docs.ps1.
5. Escrever execution report.
```

## 23. Paralelização

```md
## Paralelização

- Parallelizable: YES
- Parallel group: docs_only_validation
- Can run with: qualquer spec (não toca código/Assets, sem lock de sistema compartilhado)
- Must not run with: N/A
- Shared files/systems that require lock: nenhum
- Reason: spec puramente documental, escreve um arquivo novo em docs/validation/playmode/ sem editar
  nenhum cenário existente nem tocar registries/índices compartilhados de código.
```

## 24. Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? N/A
```

## 25. Impacto em eventos

```text
Adds events: NO
Changes existing events: NO
Requires unsubscribe pattern: NO
```

## 26. Impacto em UI/Unity

```text
Changes UI: NO
Changes scenes: NO
Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: NO (esta spec DEFINE o cenário; não exige execução humana imediata)
Human validation timing: NOT REQUIRED para esta spec em si — o checklist que ela cria é executado no
  fim do lote/wave, por decisão do usuário, não como parte do closeout desta spec.
```

## 27. Riscos técnicos

```text
Risco: checklist consolidado ficar genérico demais e não substituir de fato o valor de cenários
  específicos por spec.
Mitigação: o checklist master cobre só os fluxos ESSENCIAIS/transversais (smoke de alto nível); specs
  individuais continuam produzindo seu próprio cenário via skill gameplay-test-scenario para o detalhe
  específico da mudança.

Risco: o checklist master ficar desatualizado conforme novos fluxos são adicionados ao jogo.
Mitigação: registrar em "Notas para execução posterior" que o master deve ser revisado quando um novo
  loop/sistema core (não uma feature pontual) for adicionado ao jogo.
```

## 28. Rollback

```text
Remover docs/validation/playmode/PLAYMODE_SMOKE_CHECKLIST_MASTER.md.
Nenhum outro arquivo é afetado; rollback trivial e sem risco.
```

---

# /speckit.tasks

## 29. Tasks

```md
- [ ] T001 — Auditar docs/validation/playmode/ existente e o formato da skill gameplay-test-scenario (Fase 0).
- [ ] T002 — Escrever PLAYMODE_SMOKE_CHECKLIST_MASTER.md com as 11 seções (TownScene, FarmScene, CaveScene,
      inventário, crafting, loja, conversa NPC, quest offer/turn-in, combate básico, morte/respawn, save/load).
- [ ] T003 — Definir path de relatório (PLAYMODE_SMOKE_REPORT_<data>.md) e template de preenchimento.
- [ ] T004 — Declarar explicitamente o papel de guarda-chuva no arquivo master.
- [ ] T005 — Rodar validate_docs.ps1; escrever execution report.
```

## 30. Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
```

Nenhuma validação de build/Unity/EditMode é aplicável — spec docs-only, sem código tocado.

## 31. Testing Quality Gate

```md
## Testing Quality Gate

- Changed deterministic logic: NO
- Requires EditMode tests: NO
- Requires PlayMode automated or final human scenario: NO (esta spec É a definição do cenário; não
  exige que o humano rode a cada etapa — a execução real acontece no fim do lote/wave, por decisão do
  usuário)
- Requires regression test: NO
- Human validation timing: NOT REQUIRED para o closeout desta spec (o checklist produzido é executado
  no fim do lote/wave)
- Minimum validation evidence for ACCEPTED: docs validation PASS; PLAYMODE_SMOKE_CHECKLIST_MASTER.md
  criado com as 11 seções, path de relatório e template definidos; nenhum arquivo fora de .specs/** e
  docs/validation/** alterado.
```

## 32. Definition of Done

```text
PLAYMODE_SMOKE_CHECKLIST_MASTER.md criado em docs/validation/playmode/ com as 11 seções.
Path de relatório e template de preenchimento definidos dentro do master.
Papel de guarda-chuva declarado explicitamente no arquivo.
Nenhum arquivo fora de .specs/** e docs/validation/** alterado.
Docs validation executada (PASS ou NOT RUN com motivo).
Execution report criado.
```

## 33. Anti-regressão

```text
Não substituir nem invalidar nenhum dos 30 cenários individuais existentes em docs/validation/playmode/.
Não alterar Assets/**, Packages/**, ProjectSettings/**.
Não criar um segundo formato de relatório de validação divergente do canônico já usado no projeto.
```

## 34. Notas para execução posterior

```text
Revisar o PLAYMODE_SMOKE_CHECKLIST_MASTER.md quando um novo loop/sistema core (não uma feature pontual)
for adicionado ao jogo, para manter os 11 fluxos representativos do estado real do jogo.
Specs de arquitetura sensível (ex.: spec_arch_cave_integration_boundary_residual_v1) devem citar este
checklist master como evidência de smoke final quando aplicável, complementando (não substituindo) seu
próprio cenário específico via skill gameplay-test-scenario.
```
